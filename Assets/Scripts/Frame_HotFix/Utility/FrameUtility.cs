using System;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using System.IO;
using System.IO.Compression;
#if USE_SEVEN_ZIP
using SevenZip;
#endif
using static UnityUtility;
using static StringUtility;
using static MathUtility;
using static FileUtility;
using static FrameDefine;
using static BinaryUtility;
using static FrameBaseHotFix;
using static FrameBaseDefine;
using static FrameBaseUtility;
using UDebug = UnityEngine.Debug;

// 一些框架层的方便使用的工具函数,包含命令,对象池,列表池,字符串拼接池,延迟执行函数,以及其他的与框架层逻辑有关的工具函数
public class FrameUtility
{
	protected static int mIDMaker;						// 用于生成客户端唯一ID的种子
	protected static LONG mTempStateID = new();			// 避免GC
	public static GameCamera getMainCamera() { return mCameraManager?.getMainCamera(); }
	public static bool isKeyCurrentDown(KeyCode key, FOCUS_MASK mask = FOCUS_MASK.SCENE) { return mInputSystem.isKeyCurrentDown(key, mask); }
	public static bool isKeyCurrentUp(KeyCode key, FOCUS_MASK mask = FOCUS_MASK.SCENE) { return mInputSystem.isKeyCurrentUp(key, mask); }
	public static bool isKeyDown(KeyCode key, FOCUS_MASK mask = FOCUS_MASK.SCENE) { return mInputSystem.isKeyDown(key, mask); }
	public static bool isKeyUp(KeyCode key, FOCUS_MASK mask = FOCUS_MASK.SCENE) { return mInputSystem.isKeyUp(key, mask); }
	// 仅编辑器或桌面端使用,与多点触控相关的逻辑时不能使用此鼠标坐标,应该获取相应触点的坐标,否则会出错误
	public static Vector3 getMousePosition(bool leftBottomAsOrigin = true)
	{
		if (isEditor() || isStandalone())
		{
			Vector3 mousePos = mInputSystem.getTouchPoint((int)MOUSE_BUTTON.LEFT).getCurPosition();
			if (!leftBottomAsOrigin)
			{
				mousePos -= getHalfScreenSize().toVec3();
			}
			return mousePos;
		}
		else
		{
			return Vector3.zero;
		}
	}
	public static GameScene getCurScene() { return mGameSceneManager.getCurScene(); }
	public static bool atProcedure(Type type) { return getCurScene().atProcedure(type); }
	public static bool atProcedure<T>() where T : SceneProcedure { return getCurScene().atProcedure(typeof(T)); }
	// 百分比一般用于属性增幅之类的
	public static string toPercent(string value, int precision = 1) { return FToS(SToF(value) * 100, precision); }
	public static string toPercent(float value, int precision = 1) { return FToS(value * 100, precision); }
	// 几率类的一般是万分比的格式填写的
	public static string toProbability(string value) { return FToS(SToF(value) * 0.01f); }
	public static string toProbability(float value) { return FToS(value * 0.01f); }
	public static string fixedAndPercent(int value, float percent)
	{
		if (value > 0 && percent > 0.0f)
		{
			return IToS(value) + "+" + toPercent(percent) + "%";
		}
		if (value > 0)
		{
			return IToS(value);
		}
		if (percent > 0.0f)
		{
			return toPercent(percent) + "%";
		}
		return "";
	}
	public static myUGUIObject getUGUIRoot() { return mLayoutManager?.getUIRoot(); }
	public static Canvas getUGUIRootComponent() { return mLayoutManager?.getUIRoot()?.getCanvas(); }
	public static Camera getUICamera() { return mCameraManager.getUICamera()?.getCamera(); }
	// 将UI的宽高调整为偶数
	public static void makeSizeEven(myUGUIObject obj)
	{
		Vector2 scrollRectSize = obj.getWindowSize();
		int intScrollSizeX = ceil(scrollRectSize.x);
		int intScrollSizeY = ceil(scrollRectSize.y);
		float newScrollSizeX = intScrollSizeX + (intScrollSizeX & 1);
		float newScrollSizeY = intScrollSizeY + (intScrollSizeY & 1);
		if (!isFloatEqual(newScrollSizeX, scrollRectSize.x) || !isFloatEqual(newScrollSizeY, scrollRectSize.y))
		{
			obj.setWindowSize(new(newScrollSizeX, newScrollSizeY));
		}
	}
	public static T PACKET<T>() where T : NetPacket
	{
		return mNetPacketFactory.createSocketPacket(typeof(T)) as T;
	}
	public static T PACKET<T>(out T packet) where T : NetPacket
	{
		return packet = mNetPacketFactory.createSocketPacket(typeof(T)) as T;
	}
	// 获得一个合适的文件写入路径,fileName是StreamingAssets下的相对路径,带后缀
	public static string availableWritePath(string fileName)
	{
		// 只有编辑器是在StreamingAssets中,其他的从PersistentDataPath中加载
		if (isEditor())
		{
			return F_ASSET_BUNDLE_PATH + fileName;
		}
		else
		{
			return F_PERSISTENT_ASSETS_PATH + fileName;
		}
	}
	// 获得一个合适的文件加载路径,fileName是StreamingAssets下的相对路径,带后缀
	public static string availableReadPath(string fileName)
	{
		if (isEditor())
		{
			// 编辑器中从StreamingAssets读取
			return F_ASSET_BUNDLE_PATH + fileName;
		}
		else
		{
			// 非编辑器中时,根据文件对比结果来判断从哪儿加载
			return mAssetVersionSystem.getFileReadPath(fileName);
		}
	}
	public static void writeFileList(string path, string content)
	{
		writeTxtFile(path + FILE_LIST, content);
		// 再生成此文件的MD5文件,用于客户端校验文件内容是否改变
		writeTxtFile(path + FILE_LIST_MD5, generateFileMD5(stringToBytes(content), -1));
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 跳转流程或场景的工具函数
	public static T changeProcedure<T>() where T : SceneProcedure
	{
		return getCurScene().changeProcedure<T>() as T;
	}
	public static void changeProcedureDelay<T>(float delayTime = 0.001f) where T : SceneProcedure
	{
		delayCall(()=>{ getCurScene().changeProcedure<T>(); }, delayTime);
	}
	public static void prepareChangeProcedure<T>(float prepareTime = 0.001f) where T : SceneProcedure
	{
		getCurScene().prepareChangeProcedure<T>(prepareTime);
	}
	public static SceneProcedure enterScene<T>(Type startProcedure = null) where T : GameScene
	{
		return mGameSceneManager.enterScene<T>(startProcedure);
	}
	// 延迟到下一帧跳转
	public static void enterSceneDelay<T>(Type startProcedure = null) where T : GameScene
	{
		delayCall(() => { mGameSceneManager.enterScene<T>(startProcedure); });
	}
	public static SceneProcedure changeProcedure(Type procedure)
	{
		return getCurScene().changeProcedure(procedure);
	}
	public static void changeProcedureDelay(Type procedure, float delayTime = 0.001f)
	{
		delayCall(() => { getCurScene().changeProcedure(procedure); }, delayTime);
	}
	public static void prepareChangeProcedure(Type procedure, float prepareTime = 0.001f)
	{
		getCurScene().prepareChangeProcedure(procedure, prepareTime);
	}
	public static SceneProcedure enterScene(Type sceneType, Type startProcedure = null)
	{
		return mGameSceneManager.enterScene(sceneType, startProcedure);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 命令
	// 在主线程中创建立即执行的命令
	public static Command CMD(Type type, LOG_LEVEL logLevel = LOG_LEVEL.NORMAL)
	{
		if (mClassPool == null)
		{
			return null;
		}
		Command cmd = mClassPool.newClass(type, false) as Command;
		cmd.setCmdLogLevel(logLevel);
		cmd.setDelayCommand(false);
		cmd.setThreadCommand(false);
		return cmd;
	}
	// 在主线程中创建延迟执行的命令
	public static Command CMD_DELAY(Type type, LOG_LEVEL logLevel = LOG_LEVEL.NORMAL)
	{
		if (mClassPool == null)
		{
			return null;
		}
		Command cmd = mClassPool.newClass(type, false) as Command;
		cmd.setCmdLogLevel(logLevel);
		cmd.setDelayCommand(true);
		cmd.setThreadCommand(false);
		return cmd;
	}
	// 在子线程中创建立即执行的命令
	public static Command CMD_THREAD(Type type, LOG_LEVEL logLevel = LOG_LEVEL.NORMAL)
	{
		if (mClassPoolThread == null)
		{
			return null;
		}
		Command cmd = mClassPoolThread.newClass(type) as Command;
		cmd.setCmdLogLevel(logLevel);
		cmd.setDelayCommand(false);
		cmd.setThreadCommand(true);
		return cmd;
	}
	// 在子线程中创建延迟执行的命令
	public static Command CMD_DELAY_THREAD(Type type, LOG_LEVEL logLevel = LOG_LEVEL.NORMAL)
	{
		if (mClassPoolThread == null)
		{
			return null;
		}
		Command cmd = mClassPoolThread.newClass(type) as Command;
		cmd.setCmdLogLevel(logLevel);
		cmd.setDelayCommand(true);
		cmd.setThreadCommand(true);
		return cmd;
	}
	// 在主线程中创建立即执行的命令
	public static void CMD<T>(out T cmd, LOG_LEVEL logLevel = LOG_LEVEL.NORMAL) where T : Command
	{
		cmd = CMD(typeof(T), logLevel) as T;
	}
	// 在主线程中创建延迟执行的命令
	public static void CMD_DELAY<T>(out T cmd, LOG_LEVEL logLevel = LOG_LEVEL.NORMAL) where T : Command
	{
		cmd = CMD_DELAY(typeof(T), logLevel) as T;
	}
	// 在子线程中创建立即执行的命令
	public static void CMD_THREAD<T>(out T cmd, LOG_LEVEL logLevel = LOG_LEVEL.NORMAL) where T : Command
	{
		cmd = CMD_THREAD(typeof(T), logLevel) as T;
	}
	// 在子线程中创建延迟执行的命令
	public static void CMD_DELAY_THREAD<T>(out T cmd, LOG_LEVEL logLevel = LOG_LEVEL.NORMAL) where T : Command
	{
		cmd = CMD_DELAY_THREAD(typeof(T), logLevel) as T;
	}
	// 在子线程中发送一个指定类型的命令
	public static void pushCommandThread<T>(CommandReceiver cmdReceiver, LOG_LEVEL logLevel = LOG_LEVEL.NORMAL) where T : Command
	{
		CMD_THREAD(out T cmd, logLevel);
		pushCommand(cmd, cmdReceiver);
	}
	// 在主线程中发送一个指定类型的命令
	public static void pushCommand<T>(CommandReceiver cmdReceiver, LOG_LEVEL logLevel = LOG_LEVEL.NORMAL) where T : Command
	{
		CMD(out T cmd, logLevel);
		pushCommand(cmd, cmdReceiver);
	}
	// 在主线程中发送一个指定类型的命令
	public static void pushCommand(Command cmd, CommandReceiver cmdReceiver)
	{
		mCommandSystem?.pushCommand(cmd, cmdReceiver);
	}
	// 在子线程中发送一个指定类型的命令,并且会延迟到主线程执行
	public static T pushDelayCommandThread<T>(DelayCmdWatcher watcher, CommandReceiver cmdReceiver, float delayExecute = 0.001f, LOG_LEVEL logLevel = LOG_LEVEL.NORMAL) where T : Command
	{
		CMD_DELAY_THREAD(out T cmd, logLevel);
		pushDelayCommand(cmd, cmdReceiver, delayExecute, watcher);
		return cmd;
	}
	// 在主线程中发送一个指定类型的命令,并且在主线程中延迟执行
	public static T pushDelayCommand<T>(DelayCmdWatcher watcher, CommandReceiver cmdReceiver, float delayExecute = 0.001f, LOG_LEVEL logLevel = LOG_LEVEL.NORMAL) where T : Command
	{
		CMD_DELAY(out T cmd, logLevel);
		pushDelayCommand(cmd, cmdReceiver, delayExecute, watcher);
		return cmd;
	}
	public static void pushDelayCommand(Command cmd, CommandReceiver cmdReceiver, float delayExecute)
	{
		pushDelayCommand(cmd, cmdReceiver, delayExecute, null);
	}
	public static void pushDelayCommand(Command cmd, CommandReceiver cmdReceiver)
	{
		pushDelayCommand(cmd, cmdReceiver, 0.0f, null);
	}
	public static void pushDelayCommand(Command cmd, CommandReceiver cmdReceiver, float delayExecute, DelayCmdWatcher watcher)
	{
		mCommandSystem?.pushDelayCommand(cmd, cmdReceiver, delayExecute, watcher);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 列表对象池
	public static List<T> LIST<T>(IEnumerable<T> initList = null)
	{
		LIST(out List<T> list, initList);
		return list;
	}
	public static void LIST<T>(out List<T> list, IEnumerable<T> initList = null)
	{
		if (GameEntry.getInstance() == null || mListPool == null)
		{
			list = new();
			return;
		}
		string stackTrace = EMPTY;
		if (GameEntry.getInstance().mFramworkParam.mEnablePoolStackTrace)
		{
			stackTrace = getStackTrace();
		}
		list = mListPool.newList(typeof(T), typeof(List<T>), stackTrace, true) as List<T>;
		if (initList != null)
		{
			list.AddRange(initList);
		}
	}
	public static List<T> LIST_PERSIST<T>(IEnumerable<T> initList = null)
	{
		LIST_PERSIST(out List<T> list, initList);
		return list;
	}
	public static void LIST_PERSIST<T>(out List<T> list, IEnumerable<T> initList = null)
	{
		if (GameEntry.getInstance() == null || mListPool == null)
		{
			list = new();
			return;
		}
		string stackTrace = EMPTY;
		if (GameEntry.getInstance().mFramworkParam.mEnablePoolStackTrace)
		{
			stackTrace = getStackTrace();
		}
		list = mListPool.newList(typeof(T), typeof(List<T>), stackTrace, false) as List<T>;
		if (initList != null)
		{
			list.AddRange(initList);
		}
	}
	public static void UN_LIST<T>(List<T> list)
	{
		UN_LIST(ref list);
	}
	public static void UN_LIST<T>(ref List<T> list)
	{
		if (mListPool == null || list == null)
		{
			return;
		}
		mListPool.destroyList(ref list, typeof(T));
	}
	public static HashSet<T> SET_PERSIST<T>()
	{
		SET_PERSIST(out HashSet<T> list);
		return list;
	}
	public static void SET_PERSIST<T>(out HashSet<T> list, IEnumerable<T> initList = null)
	{
		if (GameEntry.getInstance() == null || mListPool == null)
		{
			list = new();
			return;
		}
		string stackTrace = EMPTY;
		if (GameEntry.getInstance().mFramworkParam.mEnablePoolStackTrace)
		{
			stackTrace = getStackTrace();
		}
		list = mHashSetPool.newList(typeof(T), typeof(HashSet<T>), stackTrace, false) as HashSet<T>;
		if (initList != null)
		{
			list.addRange(initList);
		}
	}
	public static void UN_SET<T>(HashSet<T> list)
	{
		UN_SET(ref list);
	}
	public static void UN_SET<T>(ref HashSet<T> list)
	{
		if (mGameFrameworkHotFix == null || mHashSetPool == null || list == null)
		{
			return;
		}
		mHashSetPool.destroyList(ref list, typeof(T));
	}
	public static Dictionary<K, V> DIC_PERSIST<K, V>()
	{
		DIC_PERSIST(out Dictionary<K, V> list);
		return list;
	}
	public static void DIC_PERSIST<K, V>(out Dictionary<K, V> list)
	{
		if (GameEntry.getInstance() == null || mListPool == null)
		{
			list = new();
			return;
		}
		string stackTrace = EMPTY;
		if (GameEntry.getInstance().mFramworkParam.mEnablePoolStackTrace)
		{
			stackTrace = getStackTrace();
		}
		list = mDictionaryPool.newList(typeof(K), typeof(V), typeof(Dictionary<K, V>), stackTrace, false) as Dictionary<K, V>;
	}
	public static void UN_DIC<K, V>(Dictionary<K, V> list)
	{
		UN_DIC(ref list);
	}
	public static void UN_DIC<K, V>(ref Dictionary<K, V> list)
	{
		if (mDictionaryPool == null || list == null)
		{
			return;
		}
		mDictionaryPool.destroyList(ref list, typeof(K), typeof(V));
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 对象池
	public static ClassObject CLASS_ONCE(Type type)
	{
		if (mClassPool == null)
		{
			return createInstance<ClassObject>(type);
		}
		return mClassPool?.newClass(type, true);
	}
	public static T CLASS_ONCE<T>(Type type) where T : ClassObject
	{
		if (mClassPool == null)
		{
			return createInstance<ClassObject>(type) as T;
		}
		return mClassPool?.newClass(type, true) as T;
	}
	public static T CLASS_ONCE<T>(out T value) where T : ClassObject, new()
	{
		if (mClassPool == null)
		{
			value = new();
			return null;
		}
		Type type = typeof(T);
		value = mClassPool?.newClass(type, true) as T;
		return value;
	}
	public static T CLASS<T>(out T value) where T : ClassObject, new()
	{
		if (mClassPool == null)
		{
			value = new();
			return null;
		}
		Type type = typeof(T);
		value = mClassPool?.newClass(type, false) as T;
		return value;
	}
	public static T CLASS<T>() where T : ClassObject, new()
	{
		if (mClassPool == null)
		{
			return new();
		}
		Type type = typeof(T);
		return mClassPool?.newClass(type, false) as T;
	}
	public static ClassObject CLASS(Type type)
	{
		if (mClassPool == null)
		{
			return createInstance<ClassObject>(type);
		}
		return mClassPool?.newClass(type, false);
	}
	public static T CLASS<T>(Type type) where T : ClassObject
	{
		if (mClassPool == null)
		{
			return createInstance<ClassObject>(type) as T;
		}
		return mClassPool?.newClass(type, false) as T;
	}
	public static T CLASS_THREAD<T>() where T : ClassObject, new()
	{
		if (mClassPoolThread == null)
		{
			return new();
		}
		return mClassPoolThread?.newClass(typeof(T)) as T;
	}
	public static void CLASS_THREAD<T>(out T value) where T : ClassObject, new()
	{
		if (mClassPoolThread == null)
		{
			value = new();
			return;
		}
		value = mClassPoolThread?.newClass(typeof(T)) as T;
	}
	public static void UN_CLASS<T>(ref T obj) where T : ClassObject
	{
		mClassPool?.destroyClass(ref obj);
	}
	public static void UN_CLASS<T>(T obj) where T : ClassObject
	{
		mClassPool?.destroyClass(ref obj);
	}
	public static void UN_CLASS_LIST<T>(IList<T> objList) where T : ClassObject
	{
		mClassPool?.destroyClassList(objList);
		objList?.Clear();
	}
	public static void UN_CLASS_LIST<T>(HashSet<T> objList) where T : ClassObject
	{
		mClassPool?.destroyClassList(objList);
		objList?.Clear();
	}
	public static void UN_CLASS_LIST<T0, T1>(IDictionary<T0, T1> objList) where T1 : ClassObject
	{
		mClassPool?.destroyClassList(objList.Values);
		objList?.Clear();
	}
	public static void UN_CLASS_LIST<T>(Queue<T> objList) where T : ClassObject
	{
		mClassPool?.destroyClass(objList);
	}
	public static void UN_CLASS_THREAD<T>(ref T obj) where T : ClassObject
	{
		mClassPoolThread?.destroyClass(ref obj);
	}
	public static void UN_CLASS_LIST_THREAD<T>(IList<T> objList) where T : ClassObject
	{
		mClassPoolThread?.destroyClassList(objList);
		objList.Clear();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 数组对象池
	public static void ARRAY_PERSIST<T>(out T[] array, int count)
	{
		if (mArrayPool == null)
		{
			array = new T[count];
			return;
		}
		array = mArrayPool.newArray<T>(count, false);
	}
	public static void UN_ARRAY<T>(T[] array, bool destroyReally = false)
	{
		UN_ARRAY(ref array, destroyReally);
	}
	public static void UN_ARRAY<T>(ref T[] array, bool destroyReally = false)
	{
		mArrayPool?.destroyArray(ref array, destroyReally);
	}
	public static void UN_ARRAY<T>(ICollection<T[]> arrayList, bool destroyReally = false)
	{
		mArrayPool?.destroyArrayList(arrayList, destroyReally);
	}
	public static void ARRAY_BYTE_PERSIST(out byte[] array, int count)
	{
		if (mByteArrayPool == null)
		{
			array = new byte[count];
			return;
		}
		array = mByteArrayPool.newArray(count, false);
	}
	public static void UN_ARRAY_BYTE(ref byte[] array, bool destroyReally = false)
	{
		mByteArrayPool?.destroyArray(ref array, destroyReally);
	}
	public static void UN_ARRAY_BYTE(ICollection<byte[]> arrayList, bool destroyReally = false)
	{
		mByteArrayPool?.destroyArrayList(arrayList, destroyReally);
	}
	public static void ARRAY_THREAD<T>(out T[] array, int count)
	{
		if (mArrayPoolThread == null)
		{
			array = new T[count];
			return;
		}
		array = mArrayPoolThread.newArray<T>(count);
	}
	public static void UN_ARRAY_THREAD<T>(ref T[] array, bool destroyReally = false)
	{
		mArrayPoolThread?.destroyArray(ref array, destroyReally);
	}
	public static void UN_ARRAY_THREAD<T>(ICollection<T[]> arrayList, bool destroyReally = false)
	{
		mArrayPoolThread?.destroyArrayList(arrayList, destroyReally);
	}
	public static void ARRAY_BYTE_THREAD(out byte[] array, int count)
	{
		if (mByteArrayPoolThread == null)
		{
			array = new byte[count];
			return;
		}
		array = mByteArrayPoolThread.newArray(count);
	}
	public static void UN_ARRAY_BYTE_THREAD(ref byte[] array, bool destroyReally = false)
	{
		mByteArrayPoolThread?.destroyArray(ref array, destroyReally);
	}
	public static void UN_ARRAY_BYTE_THREAD(byte[] array, bool destroyReally = false)
	{
		mByteArrayPoolThread?.destroyArray(ref array, destroyReally);
	}
	public static void UN_ARRAY_BYTE_THREAD(ICollection<byte[]> arrayList, bool destroyReally = false)
	{
		mByteArrayPoolThread?.destroyArrayList(arrayList, destroyReally);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// delayCall和delayCallSafe都可以实现在一定条件下阻止延迟函数执行,只不过通过delayCall的DelayCmdWatcher可以决定任意时刻终止延迟执行
	// 而delayCallSafe只有在guard销毁以后才能终止延迟执行
	// 在主线程中发起延迟调用函数,函数将在主线程中调用,如果watcher在开始执行命令时被销毁了,则命令不会被执行
	// LayoutScript不能作为watcher,因为不是从对象池中创建的
	public static long delayCall(Action function, float delayTime = 0.0f, DelayCmdWatcher watcher = null)
	{
		if (function == null || mGlobalCmdReceiver == null)
		{
			return 0;
		}
		if (isMainThread())
		{
			CMD_DELAY(out CmdGlobalDelayCall cmd);
			if (cmd == null)
			{
				return 0;
			}
			cmd.mFunction = function;
			pushDelayCommand(cmd, mGlobalCmdReceiver, delayTime, watcher);
			return cmd.getAssignID();
		}
		else
		{
			CMD_DELAY_THREAD(out CmdGlobalDelayCall cmd);
			if (cmd == null)
			{
				return 0;
			}
			cmd.mFunction = function;
			pushDelayCommand(cmd, mGlobalCmdReceiver, delayTime, watcher);
			return cmd.getAssignID();
		}
	}
	public static long delayCall<T0>(Action<T0> function, T0 param0, float delayTime = 0.0f, DelayCmdWatcher watcher = null)
	{
		if (function == null || mGlobalCmdReceiver == null)
		{
			return 0;
		}
		if (isMainThread())
		{
			CMD_DELAY(out CmdGlobalDelayCallParam1<T0> cmd);
			if (cmd == null)
			{
				return 0;
			}
			cmd.mFunction = function;
			cmd.mParam = param0;
			pushDelayCommand(cmd, mGlobalCmdReceiver, delayTime, watcher);
			return cmd.getAssignID();
		}
		else
		{
			CMD_DELAY_THREAD(out CmdGlobalDelayCallParam1<T0> cmd);
			if (cmd == null)
			{
				return 0;
			}
			cmd.mFunction = function;
			cmd.mParam = param0;
			pushDelayCommand(cmd, mGlobalCmdReceiver, delayTime, watcher);
			return cmd.getAssignID();
		}
	}
	public static long delayCall<T0, T1>(Action<T0, T1> function, T0 param0, T1 param1, float delayTime = 0.0f, DelayCmdWatcher watcher = null)
	{
		if (function == null || mGlobalCmdReceiver == null)
		{
			return 0;
		}
		if (isMainThread())
		{
			CMD_DELAY(out CmdGlobalDelayCallParam2<T0, T1> cmd);
			if (cmd == null)
			{
				return 0;
			}
			cmd.mFunction = function;
			cmd.mParam0 = param0;
			cmd.mParam1 = param1;
			pushDelayCommand(cmd, mGlobalCmdReceiver, delayTime, watcher);
			return cmd.getAssignID();
		}
		else
		{
			CMD_DELAY_THREAD(out CmdGlobalDelayCallParam2<T0, T1> cmd);
			if (cmd == null)
			{
				return 0;
			}
			cmd.mFunction = function;
			cmd.mParam0 = param0;
			cmd.mParam1 = param1;
			pushDelayCommand(cmd, mGlobalCmdReceiver, delayTime, watcher);
			return cmd.getAssignID();
		}
	}
	public static long delayCall<T0, T1, T2>(Action<T0, T1, T2> function, T0 param0, T1 param1, T2 param2, float delayTime = 0.0f, DelayCmdWatcher watcher = null)
	{
		if (function == null || mGlobalCmdReceiver == null)
		{
			return 0;
		}
		if (isMainThread())
		{
			CMD_DELAY(out CmdGlobalDelayCallParam3<T0, T1, T2> cmd);
			if (cmd == null)
			{
				return 0;
			}
			cmd.mFunction = function;
			cmd.mParam0 = param0;
			cmd.mParam1 = param1;
			cmd.mParam2 = param2;
			pushDelayCommand(cmd, mGlobalCmdReceiver, delayTime, watcher);
			return cmd.getAssignID();
		}
		else
		{
			CMD_DELAY_THREAD(out CmdGlobalDelayCallParam3<T0, T1, T2> cmd);
			if (cmd == null)
			{
				return 0;
			}
			cmd.mFunction = function;
			cmd.mParam0 = param0;
			cmd.mParam1 = param1;
			cmd.mParam2 = param2;
			pushDelayCommand(cmd, mGlobalCmdReceiver, delayTime, watcher);
			return cmd.getAssignID();
		}
	}
	public static long delayCall<T0, T1, T2, T3>(Action<T0, T1, T2, T3> function, T0 param0, T1 param1, T2 param2, T3 param3, float delayTime = 0.0f, DelayCmdWatcher watcher = null)
	{
		if (function == null || mGlobalCmdReceiver == null)
		{
			return 0;
		}
		if (isMainThread())
		{
			CMD_DELAY(out CmdGlobalDelayCallParam4<T0, T1, T2, T3> cmd);
			if (cmd == null)
			{
				return 0;
			}
			cmd.mFunction = function;
			cmd.mParam0 = param0;
			cmd.mParam1 = param1;
			cmd.mParam2 = param2;
			cmd.mParam3 = param3;
			pushDelayCommand(cmd, mGlobalCmdReceiver, delayTime, watcher);
			return cmd.getAssignID();
		}
		else
		{
			CMD_DELAY_THREAD(out CmdGlobalDelayCallParam4<T0, T1, T2, T3> cmd);
			if (cmd == null)
			{
				return 0;
			}
			cmd.mFunction = function;
			cmd.mParam0 = param0;
			cmd.mParam1 = param1;
			cmd.mParam2 = param2;
			cmd.mParam3 = param3;
			pushDelayCommand(cmd, mGlobalCmdReceiver, delayTime, watcher);
			return cmd.getAssignID();
		}
	}
	public static long delayCall<T0, T1, T2, T3, T4>(Action<T0, T1, T2, T3, T4> function, T0 param0, T1 param1, T2 param2, T3 param3, T4 param4, float delayTime = 0.0f, DelayCmdWatcher watcher = null)
	{
		if (function == null || mGlobalCmdReceiver == null)
		{
			return 0;
		}
		if (isMainThread())
		{
			CMD_DELAY(out CmdGlobalDelayCallParam5<T0, T1, T2, T3, T4> cmd);
			if (cmd == null)
			{
				return 0;
			}
			cmd.mFunction = function;
			cmd.mParam0 = param0;
			cmd.mParam1 = param1;
			cmd.mParam2 = param2;
			cmd.mParam3 = param3;
			cmd.mParam4 = param4;
			pushDelayCommand(cmd, mGlobalCmdReceiver, delayTime, watcher);
			return cmd.getAssignID();
		}
		else
		{
			CMD_DELAY_THREAD(out CmdGlobalDelayCallParam5<T0, T1, T2, T3, T4> cmd);
			if (cmd == null)
			{
				return 0;
			}
			cmd.mFunction = function;
			cmd.mParam0 = param0;
			cmd.mParam1 = param1;
			cmd.mParam2 = param2;
			cmd.mParam3 = param3;
			cmd.mParam4 = param4;
			pushDelayCommand(cmd, mGlobalCmdReceiver, delayTime, watcher);
			return cmd.getAssignID();
		}
	}
	// 在主线程中发起延迟调用函数,函数将在主线程中调用
	public static long delayCallSafe(Action function, ClassObject guard, float delayTime = 0.0f)
	{
		if (function == null || mGlobalCmdReceiver == null || guard == null)
		{
			return 0;
		}
		if (isMainThread())
		{
			CMD_DELAY(out CmdGlobalDelayCall cmd);
			if (cmd == null)
			{
				return 0;
			}
			cmd.mFunction = function;
			cmd.setGuard(guard);
			pushDelayCommand(cmd, mGlobalCmdReceiver, delayTime);
			return cmd.getAssignID();
		}
		else
		{
			CMD_DELAY_THREAD(out CmdGlobalDelayCall cmd);
			if (cmd == null)
			{
				return 0;
			}
			cmd.mFunction = function;
			cmd.setGuard(guard);
			pushDelayCommand(cmd, mGlobalCmdReceiver, delayTime);
			return cmd.getAssignID();
		}
	}
	public static long delayCallSafe<T0>(Action<T0> function, T0 param0, ClassObject guard, float delayTime = 0.0f)
	{
		if (function == null || mGlobalCmdReceiver == null || guard == null)
		{
			return 0;
		}
		if (isMainThread())
		{
			CMD_DELAY(out CmdGlobalDelayCallParam1<T0> cmd);
			if (cmd == null)
			{
				return 0;
			}
			cmd.mFunction = function;
			cmd.mParam = param0;
			cmd.setGuard(guard);
			pushDelayCommand(cmd, mGlobalCmdReceiver, delayTime);
			return cmd.getAssignID();
		}
		else
		{
			CMD_DELAY_THREAD(out CmdGlobalDelayCallParam1<T0> cmd);
			if (cmd == null)
			{
				return 0;
			}
			cmd.mFunction = function;
			cmd.mParam = param0;
			cmd.setGuard(guard);
			pushDelayCommand(cmd, mGlobalCmdReceiver, delayTime);
			return cmd.getAssignID();
		}
	}
	public static long delayCallSafe<T0, T1>(Action<T0, T1> function, T0 param0, T1 param1, ClassObject guard, float delayTime = 0.0f)
	{
		if (function == null || mGlobalCmdReceiver == null || guard == null)
		{
			return 0;
		}
		if (isMainThread())
		{
			CMD_DELAY(out CmdGlobalDelayCallParam2<T0, T1> cmd);
			if (cmd == null)
			{
				return 0;
			}
			cmd.mFunction = function;
			cmd.mParam0 = param0;
			cmd.mParam1 = param1;
			cmd.setGuard(guard);
			pushDelayCommand(cmd, mGlobalCmdReceiver, delayTime);
			return cmd.getAssignID();
		}
		else
		{
			CMD_DELAY_THREAD(out CmdGlobalDelayCallParam2<T0, T1> cmd);
			if (cmd == null)
			{
				return 0;
			}
			cmd.mFunction = function;
			cmd.mParam0 = param0;
			cmd.mParam1 = param1;
			cmd.setGuard(guard);
			pushDelayCommand(cmd, mGlobalCmdReceiver, delayTime);
			return cmd.getAssignID();
		}
	}
	public static long delayCallSafe<T0, T1, T2>(Action<T0, T1, T2> function, T0 param0, T1 param1, T2 param2, ClassObject guard, float delayTime = 0.0f)
	{
		if (function == null || mGlobalCmdReceiver == null || guard == null)
		{
			return 0;
		}
		if (isMainThread())
		{
			CMD_DELAY(out CmdGlobalDelayCallParam3<T0, T1, T2> cmd);
			if (cmd == null)
			{
				return 0;
			}
			cmd.mFunction = function;
			cmd.mParam0 = param0;
			cmd.mParam1 = param1;
			cmd.mParam2 = param2;
			cmd.setGuard(guard);
			pushDelayCommand(cmd, mGlobalCmdReceiver, delayTime);
			return cmd.getAssignID();
		}
		else
		{
			CMD_DELAY_THREAD(out CmdGlobalDelayCallParam3<T0, T1, T2> cmd);
			if (cmd == null)
			{
				return 0;
			}
			cmd.mFunction = function;
			cmd.mParam0 = param0;
			cmd.mParam1 = param1;
			cmd.mParam2 = param2;
			cmd.setGuard(guard);
			pushDelayCommand(cmd, mGlobalCmdReceiver, delayTime);
			return cmd.getAssignID();
		}
	}
	public static long delayCallSafe<T0, T1, T2, T3>(Action<T0, T1, T2, T3> function, T0 param0, T1 param1, T2 param2, T3 param3, ClassObject guard, float delayTime = 0.0f)
	{
		if (function == null || mGlobalCmdReceiver == null || guard == null)
		{
			return 0;
		}
		if (isMainThread())
		{
			CMD_DELAY(out CmdGlobalDelayCallParam4<T0, T1, T2, T3> cmd);
			if (cmd == null)
			{
				return 0;
			}
			cmd.mFunction = function;
			cmd.mParam0 = param0;
			cmd.mParam1 = param1;
			cmd.mParam2 = param2;
			cmd.mParam3 = param3;
			cmd.setGuard(guard);
			pushDelayCommand(cmd, mGlobalCmdReceiver, delayTime);
			return cmd.getAssignID();
		}
		else
		{
			CMD_DELAY_THREAD(out CmdGlobalDelayCallParam4<T0, T1, T2, T3> cmd);
			if (cmd == null)
			{
				return 0;
			}
			cmd.mFunction = function;
			cmd.mParam0 = param0;
			cmd.mParam1 = param1;
			cmd.mParam2 = param2;
			cmd.mParam3 = param3;
			cmd.setGuard(guard);
			pushDelayCommand(cmd, mGlobalCmdReceiver, delayTime);
			return cmd.getAssignID();
		}
	}
	public static long delayCallSafe<T0, T1, T2, T3, T4>(Action<T0, T1, T2, T3, T4> function, T0 param0, T1 param1, T2 param2, T3 param3, T4 param4, ClassObject guard, float delayTime = 0.0f)
	{
		if (function == null || mGlobalCmdReceiver == null || guard == null)
		{
			return 0;
		}
		if (isMainThread())
		{
			CMD_DELAY(out CmdGlobalDelayCallParam5<T0, T1, T2, T3, T4> cmd);
			if (cmd == null)
			{
				return 0;
			}
			cmd.mFunction = function;
			cmd.mParam0 = param0;
			cmd.mParam1 = param1;
			cmd.mParam2 = param2;
			cmd.mParam3 = param3;
			cmd.mParam4 = param4;
			cmd.setGuard(guard);
			pushDelayCommand(cmd, mGlobalCmdReceiver, delayTime);
			return cmd.getAssignID();
		}
		else
		{
			CMD_DELAY_THREAD(out CmdGlobalDelayCallParam5<T0, T1, T2, T3, T4> cmd);
			if (cmd == null)
			{
				return 0;
			}
			cmd.mFunction = function;
			cmd.mParam0 = param0;
			cmd.mParam1 = param1;
			cmd.mParam2 = param2;
			cmd.mParam3 = param3;
			cmd.mParam4 = param4;
			cmd.setGuard(guard);
			pushDelayCommand(cmd, mGlobalCmdReceiver, delayTime);
			return cmd.getAssignID();
		}
	}
	public static ushort generateCRC16(byte[] buffer, int count, int bufferOffset = 0)
	{
		return (ushort)(crc16(0x1F, buffer, count, bufferOffset) ^ 0x123F);
	}
	public static ushort generateCRC16(ushort value)
	{
		ushortToBytes(value, out byte byte0, out byte byte1);
		return (ushort)(crc16(0x1F, byte0, byte1) ^ 0x123F);
	}
	public static ushort generateCRC16(int value)
	{
		intToBytes(value, out byte byte0, out byte byte1, out byte byte2, out byte byte3);
		return (ushort)(crc16(0x1F, byte0, byte1, byte2, byte3) ^ 0x123F);
	}
	public static string getLocalIP()
	{
		foreach (IPAddress item in Dns.GetHostAddresses(Dns.GetHostName()))
		{
			if (item.AddressFamily == AddressFamily.InterNetwork)
			{
				return item.ToString();
			}
		}
		return "";
	}
	public static T createInstance<T>(Type classType, params object[] param) where T : class
	{
		try
		{
			return Activator.CreateInstance(classType, param) as T;
		}
		catch (Exception e)
		{
			logException(e, "create instance error! type:" + classType);
			return null;
		}
	}
	public static T deepCopy<T>(T obj) where T : class
	{
		// 如果是字符串或值类型则直接返回
		if (obj == null || obj is string || obj.GetType().IsValueType)
		{
			return obj;
		}
		object retval = createInstance<object>(obj.GetType());
		foreach (FieldInfo field in obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
		{
			field.SetValue(retval, deepCopy(field.GetValue(obj)));
		}
		return (T)retval;
	}
	public static T intToEnum<T, IntT>(IntT value) where T : Enum
	{
		return (T)Enum.ToObject(typeof(T), value);
	}
	public static int enumToInt<T>(T enumValue) where T : Enum
	{
		return Convert.ToInt32(enumValue);
	}
	public static sbyte findMax(Span<sbyte> list)
	{
		int count = list.Length;
		sbyte maxValue = list[0];
		for (int i = 1; i < count; ++i)
		{
			clampMin(ref maxValue, list[i]);
		}
		return maxValue;
	}
	public static sbyte findMax(List<sbyte> list)
	{
		int count = list.Count;
		sbyte maxValue = list[0];
		for (int i = 1; i < count; ++i)
		{
			clampMin(ref maxValue, list[i]);
		}
		return maxValue;
	}
	public static byte findMax(Span<byte> list)
	{
		int count = list.Length;
		byte maxValue = list[0];
		for (int i = 1; i < count; ++i)
		{
			clampMin(ref maxValue, list[i]);
		}
		return maxValue;
	}
	public static byte findMax(List<byte> list)
	{
		int count = list.Count;
		byte maxValue = list[0];
		for (int i = 1; i < count; ++i)
		{
			clampMin(ref maxValue, list[i]);
		}
		return maxValue;
	}
	public static short findMax(Span<short> list)
	{
		int count = list.Length;
		short maxValue = list[0];
		for (int i = 1; i < count; ++i)
		{
			clampMin(ref maxValue, list[i]);
		}
		return maxValue;
	}
	public static short findMax(List<short> list)
	{
		int count = list.Count;
		short maxValue = list[0];
		for (int i = 1; i < count; ++i)
		{
			clampMin(ref maxValue, list[i]);
		}
		return maxValue;
	}
	public static ushort findMax(Span<ushort> list)
	{
		int count = list.Length;
		ushort maxValue = list[0];
		for (int i = 1; i < count; ++i)
		{
			clampMin(ref maxValue, list[i]);
		}
		return maxValue;
	}
	public static ushort findMax(List<ushort> list)
	{
		int count = list.Count;
		ushort maxValue = list[0];
		for (int i = 1; i < count; ++i)
		{
			clampMin(ref maxValue, list[i]);
		}
		return maxValue;
	}
	public static int findMax(Span<int> list)
	{
		int count = list.Length;
		int maxValue = list[0];
		for (int i = 1; i < count; ++i)
		{
			clampMin(ref maxValue, list[i]);
		}
		return maxValue;
	}
	public static int findMax(List<int> list)
	{
		int count = list.Count;
		int maxValue = list[0];
		for (int i = 1; i < count; ++i)
		{
			clampMin(ref maxValue, list[i]);
		}
		return maxValue;
	}
	public static uint findMax(Span<uint> list)
	{
		int count = list.Length;
		uint maxValue = list[0];
		for (int i = 1; i < count; ++i)
		{
			clampMin(ref maxValue, list[i]);
		}
		return maxValue;
	}
	public static uint findMax(List<uint> list)
	{
		int count = list.Count;
		uint maxValue = list[0];
		for (int i = 1; i < count; ++i)
		{
			clampMin(ref maxValue, list[i]);
		}
		return maxValue;
	}
	public static long findMax(Span<long> list)
	{
		int count = list.Length;
		long maxValue = list[0];
		for (int i = 1; i < count; ++i)
		{
			clampMin(ref maxValue, list[i]);
		}
		return maxValue;
	}
	public static long findMax(List<long> list)
	{
		int count = list.Count;
		long maxValue = list[0];
		for (int i = 1; i < count; ++i)
		{
			clampMin(ref maxValue, list[i]);
		}
		return maxValue;
	}
	public static ulong findMax(Span<ulong> list)
	{
		int count = list.Length;
		ulong maxValue = list[0];
		for (int i = 1; i < count; ++i)
		{
			clampMin(ref maxValue, list[i]);
		}
		return maxValue;
	}
	public static ulong findMax(List<ulong> list)
	{
		int count = list.Count;
		ulong maxValue = list[0];
		for (int i = 1; i < count; ++i)
		{
			clampMin(ref maxValue, list[i]);
		}
		return maxValue;
	}
	public static float findMax(Span<float> list)
	{
		int count = list.Length;
		float maxValue = list[0];
		for (int i = 1; i < count; ++i)
		{
			clampMin(ref maxValue, list[i]);
		}
		return maxValue;
	}
	public static float findMax(List<float> list)
	{
		int count = list.Count;
		float maxValue = list[0];
		for (int i = 1; i < count; ++i)
		{
			clampMin(ref maxValue, list[i]);
		}
		return maxValue;
	}
	public static double findMax(Span<double> list)
	{
		int count = list.Length;
		double maxValue = list[0];
		for (int i = 1; i < count; ++i)
		{
			clampMin(ref maxValue, list[i]);
		}
		return maxValue;
	}
	public static double findMax(List<double> list)
	{
		int count = list.Count;
		double maxValue = list[0];
		for (int i = 1; i < count; ++i)
		{
			clampMin(ref maxValue, list[i]);
		}
		return maxValue;
	}
	public static sbyte findMaxAbs(Span<sbyte> list)
	{
		int count = list.Length;
		sbyte maxValue = abs(list[0]);
		for (int i = 1; i < count; ++i)
		{
			clampMin(ref maxValue, abs(list[i]));
		}
		return maxValue;
	}
	public static sbyte findMaxAbs(List<sbyte> list)
	{
		int count = list.Count;
		sbyte maxValue = abs(list[0]);
		for (int i = 1; i < count; ++i)
		{
			clampMin(ref maxValue, abs(list[i]));
		}
		return maxValue;
	}
	public static short findMaxAbs(Span<short> list)
	{
		int count = list.Length;
		short maxValue = abs(list[0]);
		for (int i = 1; i < count; ++i)
		{
			clampMin(ref maxValue, abs(list[i]));
		}
		return maxValue;
	}
	public static short findMaxAbs(List<short> list)
	{
		int count = list.Count;
		short maxValue = abs(list[0]);
		for (int i = 1; i < count; ++i)
		{
			clampMin(ref maxValue, abs(list[i]));
		}
		return maxValue;
	}
	public static int findMaxAbs(Span<int> list)
	{
		int count = list.Length;
		int maxValue = abs(list[0]);
		for (int i = 1; i < count; ++i)
		{
			clampMin(ref maxValue, abs(list[i]));
		}
		return maxValue;
	}
	public static int findMaxAbs(List<int> list)
	{
		int count = list.Count;
		int maxValue = abs(list[0]);
		for (int i = 1; i < count; ++i)
		{
			clampMin(ref maxValue, abs(list[i]));
		}
		return maxValue;
	}
	public static long findMaxAbs(Span<long> list)
	{
		int count = list.Length;
		long maxValue = abs(list[0]);
		for (int i = 1; i < count; ++i)
		{
			clampMin(ref maxValue, abs(list[i]));
		}
		return maxValue;
	}
	public static long findMaxAbs(List<long> list)
	{
		int count = list.Count;
		long maxValue = abs(list[0]);
		for (int i = 1; i < count; ++i)
		{
			clampMin(ref maxValue, abs(list[i]));
		}
		return maxValue;
	}
	public static float findMaxAbs(Span<float> list)
	{
		int count = list.Length;
		float maxValue = abs(list[0]);
		for (int i = 1; i < count; ++i)
		{
			clampMin(ref maxValue, abs(list[i]));
		}
		return maxValue;
	}
	public static float findMaxAbs(List<float> list)
	{
		int count = list.Count;
		float maxValue = abs(list[0]);
		for (int i = 1; i < count; ++i)
		{
			clampMin(ref maxValue, abs(list[i]));
		}
		return maxValue;
	}
	public static double findMaxAbs(Span<double> list)
	{
		int count = list.Length;
		double maxValue = abs(list[0]);
		for (int i = 1; i < count; ++i)
		{
			clampMin(ref maxValue, abs(list[i]));
		}
		return maxValue;
	}
	public static double findMaxAbs(List<double> list)
	{
		int count = list.Count;
		double maxValue = abs(list[0]);
		for (int i = 1; i < count; ++i)
		{
			clampMin(ref maxValue, abs(list[i]));
		}
		return maxValue;
	}
	public static void parseFileList(string content, Dictionary<string, GameFileInfo> list)
	{
		if (content.isEmpty())
		{
			return;
		}
		foreach (string line in splitLine(content))
		{
			var info = GameFileInfo.createInfo(line);
			list.addNotNullKey(info?.mFileName, info);
		}
	}
	public static bool isIgnorePath(string fullPath, List<string> ignorePath)
	{
		foreach (string path in ignorePath.safe())
		{
			if (fullPath.Contains(path))
			{
				return true;
			}
		}
		return false;
	}
	// ensureInterval为true表示保证每次间隔一定不小于interval,false表示保证一定时间内的触发次数,而不保证每次间隔一定小于interval
	public static bool tickTimerLoop(ref float timer, float elapsedTime, float interval, bool ensureInterval = false)
	{
		if (timer < 0.0f)
		{
			return false;
		}
		timer -= elapsedTime;
		if (timer <= 0.0f)
		{
			if (ensureInterval)
			{
				timer = interval;
			}
			else
			{
				timer += interval;
				// 如果加上间隔以后还是小于0,则可能间隔太小了,需要将计时重置到间隔时间,避免计时停止
				if (timer <= 0.0f)
				{
					timer = interval;
				}
			}
			return true;
		}
		return false;
	}
	public static bool tickTimerOnce(ref float timer, float elapsedTime)
	{
		if (timer < 0.0f)
		{
			return false;
		}
		timer -= elapsedTime;
		if (timer <= 0.0f)
		{
			timer = -1.0f;
			return true;
		}
		return false;
	}
	// preFrameCount为1表示返回调用getLineNum的行号
	public static int getLineNum(int preFrameCount = 1)
	{
		return new StackTrace(preFrameCount, true).GetFrame(0).GetFileLineNumber();
	}
	// preFrameCount为1表示返回调用getCurSourceFileName的文件名
	public static string getCurSourceFileName(int preFrameCount = 1)
	{
		return new StackTrace(preFrameCount, true).GetFrame(0).GetFileName();
	}
	// 此处不使用MyStringBuilder,因为打印堆栈时一般都是产生了某些错误,再使用MyStringBuilder可能会引起无限递归
	public static string getStackTrace(int depth = 20)
	{
		++depth;
		StringBuilder fullTrace = new();
		StackTrace trace = new(true);
		for (int i = 0; i < trace.FrameCount; ++i)
		{
			if (i == 0)
			{
				continue;
			}
			if (i >= depth)
			{
				break;
			}
			StackFrame frame = trace.GetFrame(i);
			if (frame.GetFileName().isEmpty())
			{
				break;
			}
			fullTrace.Append("at ");
			fullTrace.Append(frame.GetFileName());
			fullTrace.Append(":");
			fullTrace.AppendLine(IToS(frame.GetFileLineNumber()));
		}
		return fullTrace.ToString();
	}
	public static int makeID()
	{
		if (mIDMaker >= 0x7FFFFFFF)
		{
			logError("ID已超过最大值");
		}
		return ++mIDMaker;
	}
	public static void notifyIDUsed(int id)
	{
		mIDMaker = getMax(mIDMaker, id);
	}
	// 移除数组中的第index个元素,validElementCount是数组中有效的元素个数
	public static void removeElement<T>(T[] array, int validElementCount, int index)
	{
		if (index < 0 || index >= validElementCount)
		{
			return;
		}
		int moveCount = validElementCount - index - 1;
		for (int i = 0; i < moveCount; ++i)
		{
			array[index + i] = array[index + i + 1];
		}
	}
	// 移除数组中的所有value,T为引用类型
	public static int removeClassElement<T>(T[] array, int validElementCount, T value) where T : class
	{
		for (int i = 0; i < validElementCount; ++i)
		{
			if (array[i] == value)
			{
				removeElement(array, validElementCount--, i--);
			}
		}
		return validElementCount;
	}
	// 移除数组中的所有value,T为继承自IEquatable的值类型
	public static int removeValueElement<T>(T[] array, int validElementCount, T value) where T : IEquatable<T>
	{
		for (int i = 0; i < validElementCount; ++i)
		{
			if (array[i].Equals(value))
			{
				removeElement(array, validElementCount--, i--);
			}
		}
		return validElementCount;
	}
	public static bool arrayContains<T>(T[] array, T value, int arrayLen = -1)
	{
		if (array.isEmpty())
		{
			return false;
		}
		if (arrayLen == -1)
		{
			arrayLen = array.Length;
		}
		for (int i = 0; i < arrayLen; ++i)
		{
			if (EqualityComparer<T>.Default.Equals(array[i], value))
			{
				return true;
			}
		}
		return false;
	}
	// 比较两个列表是否完全一致
	public static bool compareList<T>(List<T> list0, List<T> list1)
	{
		if (list0 == null && list1 == null)
		{
			return true;
		}
		if (list0 == null || list1 == null)
		{
			return false;
		}
		int count = list0.Count;
		if (count != list1.Count)
		{
			return false;
		}
		for (int i = 0; i < count; ++i)
		{
			if (!EqualityComparer<T>.Default.Equals(list0[i], list1[i]))
			{
				return false;
			}
		}
		return true;
	}
	// 反转列表顺序
	public static void inverseList<T>(IList<T> list)
	{
		if (list.isEmpty())
		{
			return;
		}
		int count = list.Count;
		int halfCount = list.Count >> 1;
		for (int i = 0; i < halfCount; ++i)
		{
			T temp = list[i];
			list[i] = list[count - 1 - i];
			list[count - 1 - i] = temp;
		}
	}
	public static IPAddress hostNameToIPAddress(string hostName)
	{
		return Dns.GetHostAddresses(hostName).get(0);
	}
	public static bool isEnumValid<T>(T value) where T : Enum
	{
		return typeof(T).IsEnumDefined(value);
	}
	public static void checkEnum<T>(T value) where T : Enum
	{
		if (!typeof(T).IsEnumDefined(value))
		{
			logError(typeof(T) + "枚举不包含值:" + value);
		}
	}
#if UNITY_STANDALONE_WIN || UNITY_EDITOR
	public static bool launchExe(string dir, string args, int timeoutMilliseconds, bool hidden = false)
	{
		ProcessCreationFlags flags = hidden ? ProcessCreationFlags.CREATE_NO_WINDOW : ProcessCreationFlags.NONE;
		STARTUPINFO startupinfo = new()
		{
			cb = (uint)Marshal.SizeOf<STARTUPINFO>()
		};
		string path = getFilePath(dir);
		PROCESS_INFORMATION processinfo = new();
		bool result = Kernel32.CreateProcessW(null, dir + " " + args, IntPtr.Zero, IntPtr.Zero, false, flags,
											  IntPtr.Zero, path, ref startupinfo, ref processinfo);
		Kernel32.WaitForSingleObject(processinfo.hProcess, timeoutMilliseconds);
		return result;
	}
#endif
	// 以批处理方式运行exe
	public static void executeExeBatch(string fullExePath, string args, string workingDir = null, int waitMilliSeconds = -1, bool showError = true, bool showInfo = true, StringCallback infoCallback = null)
	{
		using Process process = new();
		ProcessStartInfo startInfo = process.StartInfo;
		startInfo.FileName = fullExePath;
		startInfo.Arguments = args;
		startInfo.WorkingDirectory = workingDir;
		startInfo.CreateNoWindow = true;
		startInfo.UseShellExecute = false;
		startInfo.RedirectStandardOutput = true;
		startInfo.RedirectStandardError = true;
		if (showInfo)
		{
			if (infoCallback != null)
			{
				process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
				{
					if (!e.Data.isEmpty())
					{
						infoCallback(e.Data);
					}
				};
			}
			else
			{
				process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
				{
					if (!e.Data.isEmpty())
					{
						log(e.Data);
					}
				};
			}
		}
		if (showError)
		{
			process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
			{
				if (e != null && !e.Data.isEmpty())
				{
					UDebug.LogError(e.Data);
				}
			};
		}
		try
		{
			process.Start();
			process.BeginOutputReadLine();
			process.BeginErrorReadLine();
			process.WaitForExit(waitMilliSeconds);
			process.Close();
		}
		catch (Exception e)
		{
			logError(e.Message);
		}
	}
	public static void executeBat(string batFullPath, string[] args)
	{
		executeExeBatch(batFullPath, stringsToString(args, " "), getFilePath(batFullPath));
	}
	public static void executeBat(string batFullPath, string arg0, string arg1)
	{
		executeExeBatch(batFullPath, arg0 + " " + arg1, getFilePath(batFullPath));
	}
	public static void executeBat(string batFullPath, string arg)
	{
		executeExeBatch(batFullPath, arg, getFilePath(batFullPath));
	}
	public static void startExe(string fullPath)
	{
		Process.Start(fullPath);
	}
	// 直接执行命令行,windows下执行cmd,macOS中执行bash
	public static void executeCmd(string[] cmdList, bool showError, bool showInfo, StringCallback infoCallback = null)
	{
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			executeExeBatch("cmd.exe", "/c " + stringsToString(cmdList, " & "), null, -1, showError, showInfo, infoCallback);
		}
		else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
		{
			executeExeBatch("/bin/bash", "-c \"" + stringsToString(cmdList, " ; ") + "\"", null, -1, showError, showInfo, infoCallback);
		}
	}
	// 在指定仓库中执行git命令
	public static void executeGitCmd(string args, string repoFullPath, bool showError, bool showInfo, StringCallback infoCallback = null)
	{
		log("execute git cmd : " + args);
		executeExeBatch("git", args, repoFullPath, -1, showError, showInfo, infoCallback);
	}
	// 执行脚本文件,macOS中使用
	public static void executeShell(string args, bool showError, bool showInfo, StringCallback infoCallback = null)
	{
		log("execute shell : " + args);
		executeExeBatch("/bin/sh", args, null, -1, showError, showInfo, infoCallback);
	}
	// 压缩为zip文件,路径为绝对路径
	public static void compressZipFile(string fileNameWithPath, string zipFileNameWithPath)
	{
		using ZipArchive zip = ZipFile.Open(zipFileNameWithPath, ZipArchiveMode.Create);
		zip.CreateEntryFromFile(fileNameWithPath, getFileNameWithSuffix(fileNameWithPath), System.IO.Compression.CompressionLevel.Optimal);
	}
	// 解压zip文件,路径为绝对路径
	public static void decompressZipFile(string zipFileNameWithPath, string extractPath)
	{
		using ZipArchive zip = ZipFile.OpenRead(zipFileNameWithPath);
		createDir(extractPath);
		foreach (ZipArchiveEntry entry in zip.Entries)
		{
			entry.ExtractToFile(extractPath + "/" + entry.FullName, true);
		}
	}
	// 目前7z只能在windows下使用
#if USE_SEVEN_ZIP && UNITY_STANDALONE_WIN
	// 压缩为7z文件,路径为绝对路径
	public static void compress7ZFile(string fileNameWithPath, string zipFileNameWithPath)
	{
		if (!isFileExist(fileNameWithPath))
		{
			logError("要压缩的文件不存在");
			return;
		}
		SevenZipBase.SetLibraryPath(F_PLUGINS_PATH + (IntPtr.Size == 4 ? "7z.dll" : "7z64.dll"));
		SevenZipCompressor tmp = new();
		tmp.ScanOnlyWritable = true;
		tmp.CompressFiles(zipFileNameWithPath, fileNameWithPath);
	}
	// 解压7z文件,路径为绝对路径
	public static void decompress7ZFile(string zipFileNameWithPath, string extractPath)
	{
		if (!isFileExist(zipFileNameWithPath))
		{
			logError("要解压的文件不存在");
			return;
		}
		SevenZipBase.SetLibraryPath(F_PLUGINS_PATH + (IntPtr.Size == 4 ? "7z.dll" : "7z64.dll"));
		validPath(ref extractPath);
		using SevenZipExtractor tmp = new(zipFileNameWithPath);
		using MemoryStream stream = new();
		for (int i = 0; i < tmp.ArchiveFileData.Count; ++i)
		{
			stream.Position = 0;
			tmp.ExtractFile(tmp.ArchiveFileData[i].Index, stream);
			byte[] bytes = stream.ToArray();
			writeFile(extractPath + tmp.ArchiveFileData[i].FileName, bytes, bytes.Length);
		}
	}
	// 解压7z文件,archiveBytes是压缩包文件的字节数组
	public static void decompress7ZFile(byte[] archiveBytes, int byteCount, string extractPath)
	{
		if (archiveBytes.isEmpty())
		{
			return;
		}
		SevenZipBase.SetLibraryPath(F_PLUGINS_PATH + (IntPtr.Size == 4 ? "7z.dll" : "7z64.dll"));
		validPath(ref extractPath);
		using MemoryStream archiveStream = new(archiveBytes, 0, byteCount);
		using SevenZipExtractor tmp = new(archiveStream);
		using MemoryStream stream = new();
		for (int i = 0; i < tmp.ArchiveFileData.Count; ++i)
		{
			stream.Position = 0;
			tmp.ExtractFile(tmp.ArchiveFileData[i].Index, stream);
			byte[] bytes = stream.ToArray();
			writeFile(extractPath + tmp.ArchiveFileData[i].FileName, bytes, bytes.Length);
		}
	}
	// 解压压缩包中的第一个文件
	public static void decompress7ZFirstFile(byte[] archiveBytes, int byteCount, out byte[] outFileBuffer)
	{
		outFileBuffer = null;
		if (archiveBytes.isEmpty())
		{
			return;
		}
		SevenZipBase.SetLibraryPath(F_PLUGINS_PATH + (IntPtr.Size == 4 ? "7z.dll" : "7z64.dll"));
		using MemoryStream archiveStream = new(archiveBytes, 0, byteCount);
		using SevenZipExtractor tmp = new(archiveStream);
		if (tmp.ArchiveFileData.Count == 0)
		{
			return;
		}
		using MemoryStream stream = new();
		tmp.ExtractFile(tmp.ArchiveFileData[0].Index, stream);
		outFileBuffer = stream.ToArray();
	}
#endif
	public static void recoverCrossParam()
	{
		mResourceManager.setDownloadURL(FrameCrossParam.mDownloadURL);
		mLocalizationManager.setCurrentLanguage(FrameCrossParam.mLocalizationName);
		mAssetVersionSystem.setStreamingAssetsVersion(FrameCrossParam.mStreamingAssetsVersion);
		mAssetVersionSystem.setPersistentAssetsVersion(FrameCrossParam.mPersistentDataVersion);
		mAssetVersionSystem.setRemoteVersion(FrameCrossParam.mRemoteVersion);
		mAssetVersionSystem.setStreamingAssetsFile(FrameCrossParam.mStreamingAssetsFileList);
		mAssetVersionSystem.setPersistentAssetsFile(FrameCrossParam.mPersistentAssetsFileList);
		mAssetVersionSystem.setRemoteAssetsFile(FrameCrossParam.mRemoteAssetsFileList);
		mAssetVersionSystem.setTotalDownloadedFiles(FrameCrossParam.mTotalDownloadedFiles);
		mAssetVersionSystem.setTotalDownloadedByteCount(FrameCrossParam.mTotalDownloadByteCount);
		mAssetVersionSystem.setAssetReadPath(FrameCrossParam.mAssetReadPath);
	}
}