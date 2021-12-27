using System;
using System.Collections.Generic;
using UnityEngine;

// 一些框架层的方便使用的工具函数,包含命令,对象池,列表池,字符串拼接池,延迟执行函数,以及其他的与框架层逻辑有关的工具函数
public class FrameUtility : WidgetUtility
{
	protected static LONG mTempStateID = new LONG();	// 避免GC
	// 移除第一个指定类型的状态
	public static void characterRemoveState<T>(Character character, string param = null) where T : CharacterState
	{
#if UNITY_EDITOR && USE_ILRUNTIME
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("热更工程无法使用characterRemoveState<T>, T:" + typeof(T));
		}
#else
		Type type = typeof(T);
#endif
		CharacterState state = character.getFirstState(type);
		if (state == null)
		{
			return;
		}
		CMD(out CmdCharacterRemoveState cmd, LOG_LEVEL.LOW);
		cmd.mState = state;
		cmd.mParam = param;
		pushCommand(cmd, character);
	}
	// 移除指定的状态
	public static void characterRemoveState(Character character, CharacterState state, string param = null)
	{
		CMD(out CmdCharacterRemoveState cmd, LOG_LEVEL.LOW);
		cmd.mState = state;
		cmd.mParam = param;
		pushCommand(cmd, character);
	}
	public static CharacterState characterAddState(Type stateType, Character character, StateParam param = null, float stateTime = -1.0f)
	{
		CMD(out CmdCharacterAddState cmd, LOG_LEVEL.LOW);
		cmd.mStateType = stateType;
		cmd.mParam = param;
		cmd.mStateTime = stateTime;
		cmd.mOutStateID = mTempStateID;
		pushCommand(cmd, character);
		return character.getState(mTempStateID.mValue);
	}
	// 添加指定类型的状态
	public static CharacterState characterAddState<T>(Character character, StateParam param = null, float stateTime = -1.0f) where T : CharacterState
	{
#if UNITY_EDITOR && USE_ILRUNTIME
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("热更工程无法使用characterAddState<T>, T:" + typeof(T));
		}
#else
		Type type = typeof(T);
#endif
		return characterAddState(type, character, param, stateTime);
	}
	public static GameCamera getMainCamera() { return FrameBase.mCameraManager.getMainCamera(); }
	public static bool isKeyCurrentDown(KeyCode key, FOCUS_MASK mask = FOCUS_MASK.NONE) { return FrameBase.mInputSystem.isKeyCurrentDown(key, mask); }
	public static bool isKeyCurrentUp(KeyCode key, FOCUS_MASK mask = FOCUS_MASK.NONE) { return FrameBase.mInputSystem.isKeyCurrentUp(key, mask); }
	public static bool isKeyDown(KeyCode key, FOCUS_MASK mask = FOCUS_MASK.NONE) { return FrameBase.mInputSystem.isKeyDown(key, mask); }
	public static bool isKeyUp(KeyCode key, FOCUS_MASK mask = FOCUS_MASK.NONE) { return FrameBase.mInputSystem.isKeyUp(key, mask); }
	// 仅编辑器或桌面端使用,与多点触控相关的逻辑时不能使用此鼠标坐标,应该获取相应触点的坐标,否则会出错误
#if UNITY_EDITOR || UNITY_STANDALONE
	public static Vector3 getMousePosition(bool leftBottomAsOrigin = true)
	{
		Vector3 mousePos = FrameBase.mInputSystem.getTouchPoint((int)MOUSE_BUTTON.LEFT).getCurPosition();
		if (leftBottomAsOrigin)
		{
			return mousePos;
		}
		else
		{
			return mousePos - (Vector3)getHalfScreenSize();
		}
	}
#endif
	public static GameScene getCurScene() { return FrameBase.mGameSceneManager.getCurScene(); }
	public static bool atProcedure(Type type) { return getCurScene().atProcedure(type); }
	public static bool atProcedure<T>() where T : SceneProcedure { return getCurScene().atProcedure(typeof(T)); }
	// 百分比一般用于属性增幅之类的
	public static string toPercent(string value, int precision = 1) { return FToS(SToF(value) * 100, precision); }
	public static string toPercent(float value, int precision = 1) { return FToS(value * 100, precision); }
	// 几率类的一般是万分比的格式填写的
	public static string toProbability(string value) { return FToS(SToF(value) * 0.01f); }
	public static string toProbability(float value) { return FToS(value * 0.01f); }
	public static myUGUIObject getUGUIRoot() { return FrameBase.mLayoutManager?.getUIRoot(); }
	public static Canvas getUGUIRootComponent() { return FrameBase.mLayoutManager?.getUIRoot()?.getCanvas(); }
	public static Camera getUICamera() { return FrameBase.mCameraManager.getUICamera()?.getCamera(); }
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
			obj.setWindowSize(new Vector2(newScrollSizeX, newScrollSizeY));
		}
	}
	public static T getData<T>(int id) where T : ExcelData
	{
		return FrameBase.mExcelManager.getData<T>(id);
	}
	// 直接获取一个指定图集的图片,此图集应该在Atlas/GameAtlas/图集名/图集名
	public static Sprite getGameAtlasSprite(string atlas, string spriteName)
	{
		return FrameBase.mTPSpriteManager.getSprite(FrameDefine.R_ATLAS_GAME_ATLAS_PATH + atlas + "/" + atlas, spriteName);
	}
	// atlas是一个GameResources下的相对路径
	public static Sprite getSprite(string atlas, string spriteName)
	{
		return FrameBase.mTPSpriteManager.getSprite(atlas, spriteName);
	}
	public static T PACKET<T>(out T packet) where T : NetPacket
	{
#if UNITY_EDITOR && USE_ILRUNTIME
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("热更工程无法使用PACKET<T>, T:" + typeof(T));
		}
#else
		Type type = typeof(T);
#endif
		return packet = FrameBase.mSocketFactory.createSocketPacket(type) as T;
	}
	// 获得一个合适的文件写入路径,fileName是StreamingAssets下的相对路径,带后缀
	public static string availableWritePath(string fileName)
	{
		// 安卓真机和iOS真机上是优先从PersistentDataPath中加载,其他的都是在StreamingAssets中
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
		return FrameDefine.F_PERSISTENT_ASSETS_PATH + fileName;
#else
		return FrameDefine.F_ASSET_BUNDLE_PATH + fileName;
#endif
	}
	// 获得一个合适的文件加载路径,fileName是StreamingAssets下的相对路径,带后缀
	public static string availableReadPath(string fileName)
	{
		// 安卓真机和iOS真机上是优先从PersistentDataPath中加载,其他的都是在StreamingAssets中
#if !UNITY_EDITOR && (UNITY_ANDROID || UNITY_IOS)
		// 优先从PersistentDataPath中加载
		string path = FrameDefine.F_PERSISTENT_ASSETS_PATH + fileName;
		if (isFileExist(path))
		{
			return path;
		}
		// PersistentDataPath中没有,再从AssetBundlePath中加载,原本在StreamingAssets中的文件也都在AssetBundlePath中
		return FrameDefine.F_ASSET_BUNDLE_PATH + fileName;
#else
		return FrameDefine.F_ASSET_BUNDLE_PATH + fileName;
#endif
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 跳转流程或场景的工具函数
	// 主工程中可调用的跳转流程的函数
	public static void changeProcedure<T>(string intent = null) where T : SceneProcedure
	{
#if UNITY_EDITOR && USE_ILRUNTIME
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("热更工程无法使用changeProcedure<T>, T:" + typeof(T));
		}
#else
		Type type = typeof(T);
#endif
		CMD(out CmdGameSceneChangeProcedure cmd, LOG_LEVEL.FORCE);
		cmd.mProcedure = type;
		cmd.mIntent = intent;
		pushCommand(cmd, getCurScene());
	}
	public static CmdGameSceneChangeProcedure changeProcedureDelay<T>(float delayTime = 0.001f, string intent = null) where T : SceneProcedure
	{
#if UNITY_EDITOR && USE_ILRUNTIME
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("热更工程无法使用changeProcedure<T>, T:" + typeof(T));
		}
#else
		Type type = typeof(T);
#endif
		CMD_DELAY(out CmdGameSceneChangeProcedure cmd, LOG_LEVEL.FORCE);
		cmd.mProcedure = type;
		cmd.mIntent = intent;
		pushDelayCommand(cmd, getCurScene(), delayTime);
		return cmd;
	}
	public static void prepareChangeProcedure<T>(float prepareTime = 0.001f, string intent = null) where T : SceneProcedure
	{
#if UNITY_EDITOR && USE_ILRUNTIME
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("热更工程无法使用changeProcedure<T>, T:" + typeof(T));
		}
#else
		Type type = typeof(T);
#endif
		CMD(out CmdGameScenePrepareChangeProcedure cmd, LOG_LEVEL.FORCE);
		cmd.mProcedure = type;
		cmd.mIntent = intent;
		cmd.mPrepareTime = prepareTime;
		pushCommand(cmd, getCurScene());
	}
	public static void enterScene<T>(Type startProcedure = null, string intent = null) where T : GameScene
	{
#if UNITY_EDITOR && USE_ILRUNTIME
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("热更工程无法使用enterScene<T>, T:" + typeof(T));
		}
#else
		Type type = typeof(T);
#endif
		CMD(out CmdGameSceneManagerEnter cmd, LOG_LEVEL.FORCE);
		cmd.mSceneType = type;
		cmd.mStartProcedure = startProcedure;
		cmd.mIntent = intent;
		pushCommand(cmd, FrameBase.mGameSceneManager);
	}
	// 延迟到下一帧跳转
	public static void enterSceneDelay<T>(Type startProcedure = null, string intent = null) where T : GameScene
	{
#if UNITY_EDITOR && USE_ILRUNTIME
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("热更工程无法使用enterScene<T>, T:" + typeof(T));
		}
#else
		Type type = typeof(T);
#endif
		CMD_DELAY(out CmdGameSceneManagerEnter cmd, LOG_LEVEL.FORCE);
		cmd.mSceneType = type;
		cmd.mStartProcedure = startProcedure;
		cmd.mIntent = intent;
		pushDelayCommand(cmd, FrameBase.mGameSceneManager);
	}
	public static void changeProcedure(Type procedure, string intent = null)
	{
		CMD(out CmdGameSceneChangeProcedure cmd, LOG_LEVEL.FORCE);
		cmd.mProcedure = procedure;
		cmd.mIntent = intent;
		pushCommand(cmd, getCurScene());
	}
	public static CmdGameSceneChangeProcedure changeProcedureDelay(Type procedure, float delayTime = 0.001f, string intent = null)
	{
		CMD_DELAY(out CmdGameSceneChangeProcedure cmd, LOG_LEVEL.FORCE);
		cmd.mProcedure = procedure;
		cmd.mIntent = intent;
		pushDelayCommand(cmd, getCurScene(), delayTime);
		return cmd;
	}
	public static void prepareChangeProcedure(Type procedure, float prepareTime = 0.001f, string intent = null)
	{
		CMD(out CmdGameScenePrepareChangeProcedure cmd, LOG_LEVEL.FORCE);
		cmd.mProcedure = procedure;
		cmd.mIntent = intent;
		cmd.mPrepareTime = prepareTime;
		pushCommand(cmd, getCurScene());
	}
	public static void enterScene(Type sceneType, Type startProcedure = null, string intent = null)
	{
		CMD(out CmdGameSceneManagerEnter cmd, LOG_LEVEL.FORCE);
		cmd.mSceneType = sceneType;
		cmd.mStartProcedure = startProcedure;
		cmd.mIntent = intent;
		pushCommand(cmd, FrameBase.mGameSceneManager);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 命令
	// 在主线程中创建立即执行的命令
	public static Command CMD(Type type, LOG_LEVEL logLevel = LOG_LEVEL.NORMAL)
	{
		Command cmd = FrameBase.mClassPool.newClass(type, false) as Command;
		cmd.setCmdLogLevel(logLevel);
		cmd.setDelayCommand(false);
		cmd.setThreadCommand(false);
		return cmd;
	}
	// 在主线程中创建延迟执行的命令
	public static Command CMD_DELAY(Type type, LOG_LEVEL logLevel = LOG_LEVEL.NORMAL)
	{
		Command cmd = FrameBase.mClassPool.newClass(type, false) as Command;
		cmd.setCmdLogLevel(logLevel);
		cmd.setDelayCommand(true);
		cmd.setThreadCommand(false);
		return cmd;
	}
	// 在子线程中创建立即执行的命令
	public static Command CMD_THREAD(Type type, LOG_LEVEL logLevel = LOG_LEVEL.NORMAL)
	{
		Command cmd = FrameBase.mClassPoolThread.newClass(type, out _) as Command;
		cmd.setCmdLogLevel(logLevel);
		cmd.setDelayCommand(false);
		cmd.setThreadCommand(true);
		return cmd;
	}
	// 在子线程中创建延迟执行的命令
	public static Command CMD_DELAY_THREAD(Type type, LOG_LEVEL logLevel = LOG_LEVEL.NORMAL)
	{
		Command cmd = FrameBase.mClassPoolThread.newClass(type, out _) as Command;
		cmd.setCmdLogLevel(logLevel);
		cmd.setDelayCommand(true);
		cmd.setThreadCommand(true);
		return cmd;
	}
	// 在主线程中创建立即执行的命令
	public static void CMD<T>(out T cmd, LOG_LEVEL logLevel = LOG_LEVEL.NORMAL) where T : Command
	{
#if UNITY_EDITOR && USE_ILRUNTIME
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("热更工程无法使用CMD<T>, T:" + typeof(T));
		}
#else
		Type type = typeof(T);
#endif
		cmd = CMD(type, logLevel) as T;
	}
	// 在主线程中创建延迟执行的命令
	public static void CMD_DELAY<T>(out T cmd, LOG_LEVEL logLevel = LOG_LEVEL.NORMAL) where T : Command
	{
#if UNITY_EDITOR && USE_ILRUNTIME
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("热更工程无法使用CMD_DELAY<T>, T:" + typeof(T));
		}
#else
		Type type = typeof(T);
#endif
		cmd = CMD_DELAY(type, logLevel) as T;
	}
	// 在子线程中创建立即执行的命令
	public static void CMD_THREAD<T>(out T cmd, LOG_LEVEL logLevel = LOG_LEVEL.NORMAL) where T : Command
	{
#if UNITY_EDITOR && USE_ILRUNTIME
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("热更工程无法使用CMD_THREAD<T>, T:" + typeof(T));
		}
#else
		Type type = typeof(T);
#endif
		cmd = CMD_THREAD(type, logLevel) as T;
	}
	// 在子线程中创建延迟执行的命令
	public static void CMD_DELAY_THREAD<T>(out T cmd, LOG_LEVEL logLevel = LOG_LEVEL.NORMAL) where T : Command
	{
#if UNITY_EDITOR && USE_ILRUNTIME
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("热更工程无法使用CMD_DELAY_THREAD<T>, T:" + typeof(T));
		}
#else
		Type type = typeof(T);
#endif
		cmd = CMD_DELAY_THREAD(type, logLevel) as T;
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
		FrameBase.mCommandSystem.pushCommand(cmd, cmdReceiver);
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
		FrameBase.mCommandSystem.pushDelayCommand(cmd, cmdReceiver, delayExecute, watcher);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 列表对象池
	public static void LIST<T>(out List<T> list)
	{
		if (FrameBase.mGameFramework == null || FrameBase.mListPool == null)
		{
			list = new List<T>();
			return;
		}
#if UNITY_EDITOR && USE_ILRUNTIME
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("热更工程无法使用LIST<T>, T:" + typeof(T));
		}
#else
		Type type = typeof(T);
#endif
		string stackTrace = EMPTY;
		if (FrameBase.mGameFramework.mEnablePoolStackTrace)
		{
			stackTrace = getStackTrace();
		}
		list = FrameBase.mListPool.newList(type, Typeof<List<T>>(), stackTrace, true) as List<T>;
	}
	public static void LIST_PERSIST<T>(out List<T> list)
	{
		if (FrameBase.mGameFramework == null || FrameBase.mListPool == null)
		{
			list = new List<T>();
			return;
		}
#if UNITY_EDITOR && USE_ILRUNTIME
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("热更工程无法使用LIST_PERSIST<T>, T:" + typeof(T));
		}
#else
		Type type = typeof(T);
#endif
		string stackTrace = EMPTY;
		if (FrameBase.mGameFramework.mEnablePoolStackTrace)
		{
			stackTrace = getStackTrace();
		}
		list = FrameBase.mListPool.newList(type, Typeof<List<T>>(), stackTrace, false) as List<T>;
	}
	public static void UN_LIST<T>(List<T> list)
	{
		if (FrameBase.mGameFramework == null || FrameBase.mListPool == null)
		{
			return;
		}
#if UNITY_EDITOR && USE_ILRUNTIME
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("热更工程无法使用UN_LIST<T>, T:" + typeof(T));
		}
#else
		Type type = typeof(T);
#endif
		FrameBase.mListPool?.destroyList(list, type);
	}
	public static void LIST<T>(out HashSet<T> list)
	{
		if (FrameBase.mGameFramework == null || FrameBase.mListPool == null)
		{
			list = new HashSet<T>();
			return;
		}
#if UNITY_EDITOR && USE_ILRUNTIME
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("热更工程无法使用LIST<T>, T:" + typeof(T));
		}
#else
		Type type = typeof(T);
#endif
		string stackTrace = EMPTY;
		if (FrameBase.mGameFramework.mEnablePoolStackTrace)
		{
			stackTrace = getStackTrace();
		}
		list = FrameBase.mHashSetPool.newList(type, Typeof<HashSet<T>>(), stackTrace, true) as HashSet<T>;
	}
	public static void LIST_PERSIST<T>(out HashSet<T> list)
	{
		if (FrameBase.mGameFramework == null || FrameBase.mListPool == null)
		{
			list = new HashSet<T>();
			return;
		}
#if UNITY_EDITOR && USE_ILRUNTIME
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("热更工程无法使用LIST_PERSIST<T>, T:" + typeof(T));
		}
#else
		Type type = typeof(T);
#endif
		string stackTrace = EMPTY;
		if (FrameBase.mGameFramework.mEnablePoolStackTrace)
		{
			stackTrace = getStackTrace();
		}
		list = FrameBase.mHashSetPool.newList(type, Typeof<HashSet<T>>(), stackTrace, false) as HashSet<T>;
	}
	public static void UN_LIST<T>(HashSet<T> list)
	{
		if (FrameBase.mGameFramework == null || FrameBase.mListPool == null)
		{
			return;
		}
#if UNITY_EDITOR && USE_ILRUNTIME
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("热更工程无法使用UN_LIST<T>, T:" + typeof(T));
		}
#else
		Type type = typeof(T);
#endif
		list.Clear();
		FrameBase.mHashSetPool?.destroyList(list, type);
	}
	public static void LIST<K, V>(out Dictionary<K, V> list)
	{
		if (FrameBase.mGameFramework == null || FrameBase.mListPool == null)
		{
			list = new Dictionary<K, V>();
			return;
		}
#if UNITY_EDITOR && USE_ILRUNTIME
		Type typeKey = Typeof<K>();
		Type typeValue = Typeof<V>();
		if (typeKey == null || typeValue == null)
		{
			logError("热更工程无法使用LIST<K, V>, K:" + typeof(K) + ", V:" + typeof(V));
		}
#else
		Type typeKey = typeof(K);
		Type typeValue = typeof(V);
#endif
		string stackTrace = EMPTY;
		if (FrameBase.mGameFramework.mEnablePoolStackTrace)
		{
			stackTrace = getStackTrace();
		}
		list = FrameBase.mDictionaryPool.newList(typeKey, typeValue, Typeof<Dictionary<K, V>>(), stackTrace, true) as Dictionary<K, V>;
	}
	public static void LIST_PERSIST<K, V>(out Dictionary<K, V> list)
	{
		if (FrameBase.mGameFramework == null || FrameBase.mListPool == null)
		{
			list = new Dictionary<K, V>();
			return;
		}
#if UNITY_EDITOR && USE_ILRUNTIME
		Type typeKey = Typeof<K>();
		Type typeValue = Typeof<V>();
		if (typeKey == null || typeValue == null)
		{
			logError("热更工程无法使用LIST_PERSIST<K, V>, K:" + typeof(K) + ", V:" + typeof(V));
		}
#else
		Type typeKey = typeof(K);
		Type typeValue = typeof(V);
#endif
		string stackTrace = EMPTY;
		if (FrameBase.mGameFramework.mEnablePoolStackTrace)
		{
			stackTrace = getStackTrace();
		}
		list = FrameBase.mDictionaryPool.newList(typeKey, typeValue, Typeof<Dictionary<K, V>>(), stackTrace, false) as Dictionary<K, V>;
	}
	public static void UN_LIST<K, V>(Dictionary<K, V> list)
	{
		if (FrameBase.mGameFramework == null || FrameBase.mListPool == null)
		{
			return;
		}
#if UNITY_EDITOR && USE_ILRUNTIME
		Type typeKey = Typeof<K>();
		Type typeValue = Typeof<V>();
		if (typeKey == null || typeValue == null)
		{
			logError("热更工程无法使用UN_LIST<K, V>, K:" + typeof(K) + ", V:" + typeof(V));
		}
#else
		Type typeKey = typeof(K);
		Type typeValue = typeof(V);
#endif
		list.Clear();
		FrameBase.mDictionaryPool?.destroyList(list, typeKey, typeValue);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 对象池
	public static void CLASS_ONCE<T>(out T value) where T : ClassObject
	{
		if (FrameBase.mClassPool == null)
		{
			value = createInstance<T>(typeof(T));
			return;
		}
#if UNITY_EDITOR && USE_ILRUNTIME
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("热更工程无法使用CLASS_ONCE<T>, T:" + typeof(T));
		}
#else
		Type type = typeof(T);
#endif
		value = FrameBase.mClassPool?.newClass(type, true) as T;
	}
	public static ClassObject CLASS_ONCE(Type type)
	{
		if (FrameBase.mClassPool == null)
		{
			return createInstance<ClassObject>(type);
		}
		return FrameBase.mClassPool?.newClass(type, true);
	}
	public static void CLASS<T>(out T value) where T : ClassObject
	{
		if (FrameBase.mClassPool == null)
		{
			value = createInstance<T>(typeof(T));
			return;
		}
#if UNITY_EDITOR && USE_ILRUNTIME
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("热更工程无法使用CLASS<T>, T:" + typeof(T));
		}
#else
		Type type = typeof(T);
#endif
		value = FrameBase.mClassPool?.newClass(type, false) as T;
	}
	public static T CLASS<T>() where T : ClassObject
	{
		if (FrameBase.mClassPool == null)
		{
			return createInstance<T>(typeof(T));
		}
#if UNITY_EDITOR && USE_ILRUNTIME
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("热更工程无法使用CLASS<T>, T:" + typeof(T));
		}
#else
		Type type = typeof(T);
#endif
		return FrameBase.mClassPool?.newClass(type, false) as T;
	}
	public static void CLASS<T>(out T value, out bool isNew) where T : ClassObject
	{
#if UNITY_EDITOR && USE_ILRUNTIME
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("热更工程无法使用CLASS<T>, T:" + typeof(T));
		}
#else
		Type type = typeof(T);
#endif
		isNew = false;
		value = FrameBase.mClassPool?.newClass(type, out isNew, false) as T;
	}
	public static ClassObject CLASS(Type type)
	{
		if (FrameBase.mClassPool == null)
		{
			return createInstance<ClassObject>(type);
		}
		return FrameBase.mClassPool?.newClass(type, false);
	}
	public static ClassObject CLASS(Type type, out bool isNew)
	{
		if (FrameBase.mClassPool == null)
		{
			isNew = true;
			return createInstance<ClassObject>(type);
		}
		isNew = false;
		return FrameBase.mClassPool?.newClass(type, out isNew, false);
	}
	// 由于热更工程无法使用多线程,所以此处不考虑热更工程
	public static void CLASS_THREAD<T>(out T value) where T : ClassObject
	{
		if (FrameBase.mClassPool == null)
		{
			value = createInstance<T>(typeof(T));
			return;
		}
#if UNITY_EDITOR && USE_ILRUNTIME
			if (Typeof<T>() == null)
		{
			logError("热更工程无法使用CLASS_THREAD<T>, T:" + typeof(T));
		}
#endif
		value = FrameBase.mClassPoolThread?.newClass(typeof(T), out _) as T;
	}
	public static void CLASS_THREAD<T>(out T value, out bool isNew) where T : ClassObject
	{
		if (FrameBase.mClassPool == null)
		{
			isNew = true;
			value = createInstance<T>(typeof(T));
			return;
		}
#if UNITY_EDITOR && USE_ILRUNTIME
		if (Typeof<T>() == null)
		{
			logError("热更工程无法使用CLASS_THREAD<T>, T:" + typeof(T));
		}
#endif
		isNew = false;
		value = FrameBase.mClassPoolThread?.newClass(typeof(T), out isNew) as T;
	}
	public static ClassObject CLASS_THREAD(Type type)
	{
		if (FrameBase.mClassPool == null)
		{
			return createInstance<ClassObject>(type);
		}
		return FrameBase.mClassPoolThread?.newClass(type, out _);
	}
	public static ClassObject CLASS_THREAD(Type type, out bool isNew)
	{
		if (FrameBase.mClassPool == null)
		{
			isNew = true;
			return createInstance<ClassObject>(type);
		}
		isNew = false;
		return FrameBase.mClassPoolThread?.newClass(type, out isNew);
	}
	public static void UN_CLASS(ClassObject obj)
	{
		FrameBase.mClassPool?.destroyClass(obj);
	}
	public static void UN_CLASS_THREAD(ClassObject obj)
	{
		FrameBase.mClassPoolThread?.destroyClass(obj);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 数组对象池
	public static void ARRAY<T>(out T[] array, int count)
	{
#if UNITY_EDITOR && USE_ILRUNTIME
		if (Typeof<T>() == null)
		{
			logError("热更工程无法使用ARRAY<T>, T:" + typeof(T));
		}
#endif
		if (FrameBase.mArrayPool == null)
		{
			array = new T[count];
			return;
		}
		array = FrameBase.mArrayPool.newArray<T>(count, true);
	}
	public static void ARRAY_PERSIST<T>(out T[] array, int count)
	{
#if UNITY_EDITOR && USE_ILRUNTIME
		if (Typeof<T>() == null)
		{
			logError("热更工程无法使用ARRAY_PERSIST<T>, T:" + typeof(T));
		}
#endif
		if (FrameBase.mArrayPool == null)
		{
			array = new T[count];
			return;
		}
		array = FrameBase.mArrayPool.newArray<T>(count, false);
	}
	public static void UN_ARRAY<T>(T[] array, bool destroyReally = false)
	{
#if UNITY_EDITOR && USE_ILRUNTIME
		if (Typeof<T>() == null)
		{
			logError("热更工程无法使用UN_ARRAY<T>, T:" + typeof(T));
		}
#endif
		FrameBase.mArrayPool?.destroyArray(array, destroyReally);
	}
	public static void ARRAY_THREAD<T>(out T[] array, int count)
	{
#if UNITY_EDITOR && USE_ILRUNTIME
		if (Typeof<T>() == null)
		{
			logError("热更工程无法使用ARRAY_THREAD<T>, T:" + typeof(T));
		}
#endif
		if (FrameBase.mArrayPoolThread == null)
		{
			array = new T[count];
			return;
		}
		array = FrameBase.mArrayPoolThread.newArray<T>(count);
	}
	public static void UN_ARRAY_THREAD<T>(T[] array, bool destroyReally = false)
	{
#if UNITY_EDITOR && USE_ILRUNTIME
		if (Typeof<T>() == null)
		{
			logError("热更工程无法使用UN_ARRAY_THREAD<T>, T:" + typeof(T));
		}
#endif
		FrameBase.mArrayPoolThread?.destroyArray(array, destroyReally);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 字符串拼接
	public static MyStringBuilder STRING()
	{
		if (FrameBase.mClassPool == null)
		{
			return new MyStringBuilder();
		}
		return CLASS_ONCE(typeof(MyStringBuilder)) as MyStringBuilder;
	}
	public static MyStringBuilder STRING(string str)
	{
		return STRING().append(str);
	}
	public static MyStringBuilder STRING(string str0, string str1)
	{
		return STRING().append(str0, str1);
	}
	public static MyStringBuilder STRING(string str0, string str1, string str2)
	{
		return STRING().append(str0, str1, str2);
	}
	public static MyStringBuilder STRING(string str0, string str1, string str2, string str3)
	{
		return STRING().append(str0, str1, str2, str3);
	}
	public static MyStringBuilder STRING(string str0, string str1, string str2, string str3, string str4)
	{
		return STRING().append(str0, str1, str2, str3, str4);
	}
	public static MyStringBuilder STRING(string str0, string str1, string str2, string str3, string str4, string str5)
	{
		return STRING().append(str0, str1, str2, str3, str4, str5);
	}
	public static MyStringBuilder STRING(string str0, string str1, string str2, string str3, string str4, string str5, string str6)
	{
		return STRING().append(str0, str1, str2, str3, str4, str5, str6);
	}
	public static MyStringBuilder STRING(string str0, string str1, string str2, string str3, string str4, string str5, string str6, string str7)
	{
		return STRING().append(str0, str1, str2, str3, str4, str5, str6, str7);
	}
	public static MyStringBuilder STRING(string str0, string str1, string str2, string str3, string str4, string str5, string str6, string str7, string str8)
	{
		return STRING().append(str0, str1, str2, str3, str4, str5, str6, str7, str8);
	}
	public static MyStringBuilder STRING(string str0, string str1, string str2, string str3, string str4, string str5, string str6, string str7, string str8, string str9)
	{
		return STRING().append(str0, str1, str2, str3, str4, str5, str6, str7, str8, str9);
	}
	public static MyStringBuilder STRING_THREAD()
	{
		if (FrameBase.mClassPoolThread == null)
		{
			return new MyStringBuilder();
		}
		return CLASS_THREAD(typeof(MyStringBuilder)) as MyStringBuilder;
	}
	public static MyStringBuilder STRING_THREAD(string str)
	{
		return STRING_THREAD().append(str);
	}
	public static MyStringBuilder STRING_THREAD(string str0, string str1)
	{
		return STRING_THREAD().append(str0, str1);
	}
	public static MyStringBuilder STRING_THREAD(string str0, string str1, string str2)
	{
		return STRING_THREAD().append(str0, str1, str2);
	}
	public static MyStringBuilder STRING_THREAD(string str0, string str1, string str2, string str3)
	{
		return STRING_THREAD().append(str0, str1, str2, str3);
	}
	public static MyStringBuilder STRING_THREAD(string str0, string str1, string str2, string str3, string str4)
	{
		return STRING_THREAD().append(str0, str1, str2, str3, str4);
	}
	public static MyStringBuilder STRING_THREAD(string str0, string str1, string str2, string str3, string str4, string str5)
	{
		return STRING_THREAD().append(str0, str1, str2, str3, str4, str5);
	}
	public static MyStringBuilder STRING_THREAD(string str0, string str1, string str2, string str3, string str4, string str5, string str6)
	{
		return STRING_THREAD().append(str0, str1, str2, str3, str4, str5, str6);
	}
	public static MyStringBuilder STRING_THREAD(string str0, string str1, string str2, string str3, string str4, string str5, string str6, string str7)
	{
		return STRING_THREAD().append(str0, str1, str2, str3, str4, str5, str6, str7);
	}
	public static MyStringBuilder STRING_THREAD(string str0, string str1, string str2, string str3, string str4, string str5, string str6, string str7, string str8)
	{
		return STRING_THREAD().append(str0, str1, str2, str3, str4, str5, str6, str7, str8);
	}
	public static MyStringBuilder STRING_THREAD(string str0, string str1, string str2, string str3, string str4, string str5, string str6, string str7, string str8, string str9)
	{
		return STRING_THREAD().append(str0, str1, str2, str3, str4, str5, str6, str7, str8, str9);
	}
	public static string END_STRING_THREAD(MyStringBuilder builder)
	{
		string str = builder.ToString();
		UN_CLASS_THREAD(builder);
		return str;
	}
	public static string END_STRING(MyStringBuilder builder)
	{
		string str = builder.ToString();
		UN_CLASS(builder);
		return str;
	}
	public static void DESTROY_STRING(MyStringBuilder builder)
	{
		UN_CLASS(builder);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 在主线程中发起延迟调用函数,函数将在主线程中调用
	public static long delayCall(Action function, float delayTime = 0.0f)
	{
		CMD_DELAY(out CmdGlobalDelayCall cmd);
		cmd.mFunction = function;
		pushDelayCommand(cmd, FrameBase.mGlobalCmdReceiver, delayTime);
		return cmd.getAssignID();
	}
	public static long delayCall<T0>(Action<T0> function, T0 param0, float delayTime = 0.0f)
	{
		CMD_DELAY(out CmdGlobalDelayCallParam1<T0> cmd);
		cmd.mFunction = function;
		cmd.mParam = param0;
		pushDelayCommand(cmd, FrameBase.mGlobalCmdReceiver, delayTime);
		return cmd.getAssignID();
	}
	public static long delayCall<T0, T1>(Action<T0, T1> function, T0 param0, T1 param1, float delayTime = 0.0f)
	{
		CMD_DELAY(out CmdGlobalDelayCallParam2<T0, T1> cmd);
		cmd.mFunction = function;
		cmd.mParam0 = param0;
		cmd.mParam1 = param1;
		pushDelayCommand(cmd, FrameBase.mGlobalCmdReceiver, delayTime);
		return cmd.getAssignID();
	}
	public static long delayCall<T0, T1, T2>(Action<T0, T1, T2> function, T0 param0, T1 param1, T2 param2, float delayTime = 0.0f)
	{
		CMD_DELAY(out CmdGlobalDelayCallParam3<T0, T1, T2> cmd);
		cmd.mFunction = function;
		cmd.mParam0 = param0;
		cmd.mParam1 = param1;
		cmd.mParam2 = param2;
		pushDelayCommand(cmd, FrameBase.mGlobalCmdReceiver, delayTime);
		return cmd.getAssignID();
	}
	public static long delayCall<T0, T1, T2, T3>(Action<T0, T1, T2, T3> function, T0 param0, T1 param1, T2 param2, T3 param3, float delayTime = 0.0f)
	{
		CMD_DELAY(out CmdGlobalDelayCallParam4<T0, T1, T2, T3> cmd);
		cmd.mFunction = function;
		cmd.mParam0 = param0;
		cmd.mParam1 = param1;
		cmd.mParam2 = param2;
		cmd.mParam3 = param3;
		pushDelayCommand(cmd, FrameBase.mGlobalCmdReceiver, delayTime);
		return cmd.getAssignID();
	}
	public static long delayCall<T0, T1, T2, T3, T4>(Action<T0, T1, T2, T3, T4> function, T0 param0, T1 param1, T2 param2, T3 param3, T4 param4, float delayTime = 0.0f)
	{
		CMD_DELAY(out CmdGlobalDelayCallParam5<T0, T1, T2, T3, T4> cmd);
		cmd.mFunction = function;
		cmd.mParam0 = param0;
		cmd.mParam1 = param1;
		cmd.mParam2 = param2;
		cmd.mParam3 = param3;
		cmd.mParam4 = param4;
		pushDelayCommand(cmd, FrameBase.mGlobalCmdReceiver, delayTime);
		return cmd.getAssignID();
	}
	// 在子线程中发起延迟调用函数,函数将在主线程中调用
	public static long delayCallThread(Action function, float delayTime = 0.0f)
	{
		CMD_DELAY_THREAD(out CmdGlobalDelayCall cmd);
		cmd.mFunction = function;
		pushDelayCommand(cmd, FrameBase.mGlobalCmdReceiver, delayTime);
		return cmd.getAssignID();
	}
	public static long delayCallThread<T0>(Action<T0> function, T0 param0, float delayTime = 0.0f)
	{
		CMD_DELAY_THREAD(out CmdGlobalDelayCallParam1<T0> cmd);
		cmd.mFunction = function;
		cmd.mParam = param0;
		pushDelayCommand(cmd, FrameBase.mGlobalCmdReceiver, delayTime);
		return cmd.getAssignID();
	}
	public static long delayCallThread<T0, T1>(Action<T0, T1> function, T0 param0, T1 param1, float delayTime = 0.0f)
	{
		CMD_DELAY_THREAD(out CmdGlobalDelayCallParam2<T0, T1> cmd);
		cmd.mFunction = function;
		cmd.mParam0 = param0;
		cmd.mParam1 = param1;
		pushDelayCommand(cmd, FrameBase.mGlobalCmdReceiver, delayTime);
		return cmd.getAssignID();
	}
	public static long delayCallThread<T0, T1, T2>(Action<T0, T1, T2> function, T0 param0, T1 param1, T2 param2, float delayTime = 0.0f)
	{
		CMD_DELAY_THREAD(out CmdGlobalDelayCallParam3<T0, T1, T2> cmd);
		cmd.mFunction = function;
		cmd.mParam0 = param0;
		cmd.mParam1 = param1;
		cmd.mParam2 = param2;
		pushDelayCommand(cmd, FrameBase.mGlobalCmdReceiver, delayTime);
		return cmd.getAssignID();
	}
	public static long delayCallThread<T0, T1, T2, T3>(Action<T0, T1, T2, T3> function, T0 param0, T1 param1, T2 param2, T3 param3, float delayTime = 0.0f)
	{
		CMD_DELAY_THREAD(out CmdGlobalDelayCallParam4<T0, T1, T2, T3> cmd);
		cmd.mFunction = function;
		cmd.mParam0 = param0;
		cmd.mParam1 = param1;
		cmd.mParam2 = param2;
		cmd.mParam3 = param3;
		pushDelayCommand(cmd, FrameBase.mGlobalCmdReceiver, delayTime);
		return cmd.getAssignID();
	}
	public static long delayCallThread<T0, T1, T2, T3, T4>(Action<T0, T1, T2, T3, T4> function, T0 param0, T1 param1, T2 param2, T3 param3, T4 param4, float delayTime = 0.0f)
	{
		CMD_DELAY_THREAD(out CmdGlobalDelayCallParam5<T0, T1, T2, T3, T4> cmd);
		cmd.mFunction = function;
		cmd.mParam0 = param0;
		cmd.mParam1 = param1;
		cmd.mParam2 = param2;
		cmd.mParam3 = param3;
		cmd.mParam4 = param4;
		pushDelayCommand(cmd, FrameBase.mGlobalCmdReceiver, delayTime);
		return cmd.getAssignID();
	}
}