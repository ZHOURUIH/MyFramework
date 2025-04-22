using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityUtility;
using static CSharpUtility;
using static StringUtility;
using static FileUtility;
using static MathUtility;
using static BinaryUtility;
using static FrameBaseHotFix;
using static FrameDefine;
using static FrameBaseDefine;
using static FrameBaseUtility;

// 一些框架层的方便使用的工具函数,包含命令,对象池,列表池,字符串拼接池,延迟执行函数,以及其他的与框架层逻辑有关的工具函数
public class FrameUtility
{
	protected static LONG mTempStateID = new();				// 避免GC
	public static GameCamera getMainCamera() { return mCameraManager?.getMainCamera(); }
	public static bool isKeyCurrentDown(KeyCode key, FOCUS_MASK mask = FOCUS_MASK.NONE) { return mInputSystem.isKeyCurrentDown(key, mask); }
	public static bool isKeyCurrentUp(KeyCode key, FOCUS_MASK mask = FOCUS_MASK.NONE) { return mInputSystem.isKeyCurrentUp(key, mask); }
	public static bool isKeyDown(KeyCode key, FOCUS_MASK mask = FOCUS_MASK.NONE) { return mInputSystem.isKeyDown(key, mask); }
	public static bool isKeyUp(KeyCode key, FOCUS_MASK mask = FOCUS_MASK.NONE) { return mInputSystem.isKeyUp(key, mask); }
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
	public static T getData<T>(int id) where T : ExcelData
	{
		return mExcelManager.getData<T>(id);
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
	public static void changeProcedure<T>() where T : SceneProcedure
	{
		getCurScene().changeProcedure<T>();
	}
	public static void changeProcedureDelay<T>(float delayTime = 0.001f) where T : SceneProcedure
	{
		delayCall(()=>{ getCurScene().changeProcedure<T>(); }, delayTime);
	}
	public static void prepareChangeProcedure<T>(float prepareTime = 0.001f) where T : SceneProcedure
	{
		getCurScene().prepareChangeProcedure<T>(prepareTime);
	}
	public static void enterScene<T>(Type startProcedure = null) where T : GameScene
	{
		mGameSceneManager.enterScene<T>(startProcedure);
	}
	// 延迟到下一帧跳转
	public static void enterSceneDelay<T>(Type startProcedure = null) where T : GameScene
	{
		mGameSceneManager.enterScene<T>(startProcedure);
	}
	public static void changeProcedure(Type procedure)
	{
		getCurScene().changeProcedure(procedure);
	}
	public static void changeProcedureDelay(Type procedure, float delayTime = 0.001f)
	{
		delayCall(() => { getCurScene().changeProcedure(procedure); }, delayTime);
	}
	public static void prepareChangeProcedure(Type procedure, float prepareTime = 0.001f)
	{
		getCurScene().prepareChangeProcedure(procedure, prepareTime);
	}
	public static void enterScene(Type sceneType, Type startProcedure = null)
	{
		mGameSceneManager.enterScene(sceneType, startProcedure);
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