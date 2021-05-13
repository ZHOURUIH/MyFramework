using UnityEngine;
using System;
using System.Collections.Generic;

// 管理类初始化完成调用
// 这个父类的添加是方便代码的书写
public class FrameBase : ClassObject
{
	// FrameSystem
	public static GameFramework mGameFramework;
	public static CommandSystem mCommandSystem;
	public static AudioManager mAudioManager;
	public static GameSceneManager mGameSceneManager;
	public static CharacterManager mCharacterManager;
	public static LayoutManager mLayoutManager;
	public static KeyFrameManager mKeyFrameManager;
	public static GlobalTouchSystem mGlobalTouchSystem;
	public static ShaderManager mShaderManager;
#if !UNITY_IOS && !NO_SQLITE
	public static SQLiteManager mSQLiteManager;
#endif
	public static CameraManager mCameraManager;
	public static ResourceManager mResourceManager;
	public static ApplicationConfig mApplicationConfig;
	public static FrameConfig mFrameConfig;
	public static ObjectPool mObjectPool;
	public static InputManager mInputManager;
	public static SceneSystem mSceneSystem;
	public static ClassPool mClassPool;
	public static ClassPoolThread mClassPoolThread;
	public static ListPool mListPool;
	public static ListPoolThread mListPoolThread;
	public static HashSetPool mHashSetPool;
	public static HashSetPoolThread mHashSetPoolThread;
	public static DictionaryPool mDictionaryPool;
	public static DictionaryPoolThread mDictionaryPoolThread;
	public static ArrayPool mArrayPool;
	public static ArrayPoolThread mArrayPoolThread;
	public static AndroidPluginManager mAndroidPluginManager;
	public static AndroidAssetLoader mAndroidAssetLoader;
	public static HeadTextureManager mHeadTextureManager;
	public static TimeManager mTimeManager;
	public static MovableObjectManager mMovableObjectManager;
	public static EffectManager mEffectManager;
	public static TPSpriteManager mTPSpriteManager;
	public static SocketFactory mSocketFactory;
	public static SocketFactoryThread mSocketFactoryThread;
	public static PathKeyframeManager mPathKeyframeManager;
	public static EventSystem mEventSystem;
	public static TweenerManager mTweenerManager;
#if USE_ILRUNTIME
	public static ILRSystem mILRSystem;
#endif
#if !UNITY_EDITOR
	//public static LocalLog mLocalLog;
#endif
	public virtual void notifyConstructDone()
	{
		mGameFramework = GameFramework.mGameFramework;
		mCommandSystem = mGameFramework.getSystem(typeof(CommandSystem)) as CommandSystem;
		mAudioManager = mGameFramework.getSystem(typeof(AudioManager)) as AudioManager;
		mGameSceneManager = mGameFramework.getSystem(typeof(GameSceneManager)) as GameSceneManager;
		mCharacterManager = mGameFramework.getSystem(typeof(CharacterManager)) as CharacterManager;
		mLayoutManager = mGameFramework.getSystem(typeof(LayoutManager)) as LayoutManager;
		mKeyFrameManager = mGameFramework.getSystem(typeof(KeyFrameManager)) as KeyFrameManager;
		mGlobalTouchSystem = mGameFramework.getSystem(typeof(GlobalTouchSystem)) as GlobalTouchSystem;
		mShaderManager = mGameFramework.getSystem(typeof(ShaderManager)) as ShaderManager;
#if !UNITY_IOS && !NO_SQLITE
		mSQLiteManager = mGameFramework.getSystem(typeof(SQLiteManager)) as SQLiteManager;
#endif
		mCameraManager = mGameFramework.getSystem(typeof(CameraManager)) as CameraManager;
		mResourceManager = mGameFramework.getSystem(typeof(ResourceManager)) as ResourceManager;
		mApplicationConfig = mGameFramework.getSystem(typeof(ApplicationConfig)) as ApplicationConfig;
		mFrameConfig = mGameFramework.getSystem(typeof(FrameConfig)) as FrameConfig;
		mObjectPool = mGameFramework.getSystem(typeof(ObjectPool)) as ObjectPool;
		mInputManager = mGameFramework.getSystem(typeof(InputManager)) as InputManager;
		mSceneSystem = mGameFramework.getSystem(typeof(SceneSystem)) as SceneSystem;
		mClassPool = mGameFramework.getSystem(typeof(ClassPool)) as ClassPool;
		mClassPoolThread = mGameFramework.getSystem(typeof(ClassPoolThread)) as ClassPoolThread;
		mListPool = mGameFramework.getSystem(typeof(ListPool)) as ListPool;
		mListPoolThread = mGameFramework.getSystem(typeof(ListPoolThread)) as ListPoolThread;
		mHashSetPool = mGameFramework.getSystem(typeof(HashSetPool)) as HashSetPool;
		mHashSetPoolThread = mGameFramework.getSystem(typeof(HashSetPoolThread)) as HashSetPoolThread;
		mDictionaryPool = mGameFramework.getSystem(typeof(DictionaryPool)) as DictionaryPool;
		mDictionaryPoolThread = mGameFramework.getSystem(typeof(DictionaryPoolThread)) as DictionaryPoolThread;
		mArrayPool = mGameFramework.getSystem(typeof(ArrayPool)) as ArrayPool;
		mArrayPoolThread = mGameFramework.getSystem(typeof(ArrayPoolThread)) as ArrayPoolThread;
		mAndroidPluginManager = mGameFramework.getSystem(typeof(AndroidPluginManager)) as AndroidPluginManager;
		mAndroidAssetLoader = mGameFramework.getSystem(typeof(AndroidAssetLoader)) as AndroidAssetLoader;
		mHeadTextureManager = mGameFramework.getSystem(typeof(HeadTextureManager)) as HeadTextureManager;
		mTimeManager = mGameFramework.getSystem(typeof(TimeManager)) as TimeManager;
		mMovableObjectManager = mGameFramework.getSystem(typeof(MovableObjectManager)) as MovableObjectManager;
		mEffectManager = mGameFramework.getSystem(typeof(EffectManager)) as EffectManager;
		mTPSpriteManager = mGameFramework.getSystem(typeof(TPSpriteManager)) as TPSpriteManager;
		mSocketFactory = mGameFramework.getSystem(typeof(SocketFactory)) as SocketFactory;
		mSocketFactoryThread = mGameFramework.getSystem(typeof(SocketFactoryThread)) as SocketFactoryThread;
		mPathKeyframeManager = mGameFramework.getSystem(typeof(PathKeyframeManager)) as PathKeyframeManager;
		mEventSystem = mGameFramework.getSystem(typeof(EventSystem)) as EventSystem;
		mTweenerManager = mGameFramework.getSystem(typeof(TweenerManager)) as TweenerManager;
#if USE_ILRUNTIME
		mILRSystem = mGameFramework.getSystem(typeof(ILRSystem)) as ILRSystem;
#endif
	}
	// 方便书写代码添加的命令相关函数
	public static void changeProcedure(Type procedure, string intent = null)
	{
		CMD_MAIN(out CmdGameSceneChangeProcedure cmd);
		cmd.mProcedure = procedure;
		cmd.mIntent = intent;
		pushCommand(cmd, mGameSceneManager.getCurScene());
	}
	public static CmdGameSceneChangeProcedure changeProcedureDelay(Type procedure, float delayTime = 0.001f, string intent = null)
	{
		CMD_MAIN_DELAY(out CmdGameSceneChangeProcedure cmd, true);
		cmd.mProcedure = procedure;
		cmd.mIntent = intent;
		pushDelayCommand(cmd, mGameSceneManager.getCurScene(), delayTime);
		return cmd;
	}
	public static void prepareChangeProcedure(Type procedure, float prepareTime = 0.001f, string intent = null)
	{
		CMD_MAIN(out CmdGameScenePrepareChangeProcedure cmd);
		cmd.mProcedure = procedure;
		cmd.mIntent = intent;
		cmd.mPrepareTime = prepareTime;
		pushCommand(cmd, mGameSceneManager.getCurScene());
	}
	public static bool getKeyCurrentDown(KeyCode key, FOCUS_MASK mask = FOCUS_MASK.NONE) { return mInputManager.getKeyCurrentDown(key, mask); }
	public static bool getKeyCurrentUp(KeyCode key, FOCUS_MASK mask = FOCUS_MASK.NONE) { return mInputManager.getKeyCurrentUp(key, mask); }
	public static bool getKeyDown(KeyCode key, FOCUS_MASK mask = FOCUS_MASK.NONE) { return mInputManager.getKeyDown(key, mask); }
	public static bool getKeyUp(KeyCode key, FOCUS_MASK mask = FOCUS_MASK.NONE) { return mInputManager.getKeyUp(key, mask); }
	public static Vector3 getMousePosition() { return mGlobalTouchSystem.getCurMousePosition(); }
	public static GameScene getCurScene() { return mGameSceneManager.getCurScene(); }
	// 百分比一般用于属性增幅之类的
	public static string toPercent(string value, int precision = 1) { return FToS(SToF(value) * 100, precision); }
	public static string toPercent(float value, int precision = 1) { return FToS(value * 100, precision); }
	// 几率类的一般是万分比的格式填写的
	public static string toProbability(string value) { return FToS(SToF(value) * 0.01f); }
	public static T PACKET_MAIN<T>(out T packet) where T : SocketPacket
	{
		return packet = mSocketFactory.createSocketPacket(typeof(T)) as T;
	}
	//-----------------------------------------------------------------------------------------------------------------------------------
	// 命令
	public static T CMD_MAIN<T>(out T cmd, bool show = true) where T : Command
	{
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("无法使用CMD创建非主工程的命令");
		}
		return cmd = mCommandSystem.newCmd(type, show) as T;
	}
	public static T CMD_MAIN_DELAY<T>(out T cmd, bool show = true) where T : Command
	{
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("无法使用CMD_DELAY创建非主工程的命令");
		}
		return cmd = mCommandSystem.newCmd(type, show, true) as T;
	}
	public static void pushMainCommand<T>(CommandReceiver cmdReceiver, bool show = true) where T : Command
	{
		CMD_MAIN(out T cmd, show);
		mCommandSystem.pushCommand(cmd, cmdReceiver);
	}
	public static void pushCommand(Command cmd, CommandReceiver cmdReceiver)
	{
		mCommandSystem.pushCommand(cmd, cmdReceiver);
	}
	public static T pushDelayMainCommand<T>(IDelayCmdWatcher watcher, CommandReceiver cmdReceiver, float delayExecute = 0.001f, bool show = true) where T : Command
	{
		CMD_MAIN_DELAY(out T cmd, show);
		mCommandSystem.pushDelayCommand(cmd, cmdReceiver, delayExecute, watcher);
		return cmd;
	}
	public static void pushDelayCommand(Command cmd, CommandReceiver cmdReceiver, float delayExecute, IDelayCmdWatcher watcher)
	{
		mCommandSystem.pushDelayCommand(cmd, cmdReceiver, delayExecute, watcher);
	}
	public static void pushDelayCommand(Command cmd, CommandReceiver cmdReceiver, float delayExecute)
	{
		mCommandSystem.pushDelayCommand(cmd, cmdReceiver, delayExecute, null);
	}
	public static void pushDelayCommand(Command cmd, CommandReceiver cmdReceiver)
	{
		mCommandSystem.pushDelayCommand(cmd, cmdReceiver, 0.0f, null);
	}
	//-----------------------------------------------------------------------------------------------------------------------------------
	// 对象池
	public static void LIST_MAIN<T>(out List<T> list)
	{
		string stackTrace = EMPTY;
		if(mGameFramework.isEnablePoolStackTrace())
		{
			stackTrace = getStackTrace();
		}
		list = mListPool.newList(Typeof<T>(), Typeof<List<T>>(), stackTrace, true) as List<T>;
	}
	public static void LIST_MAIN_PERSIST<T>(out List<T> list)
	{
		string stackTrace = EMPTY;
		if (mGameFramework.isEnablePoolStackTrace())
		{
			stackTrace = getStackTrace();
		}
		list = mListPool.newList(Typeof<T>(), Typeof<List<T>>(), stackTrace, false) as List<T>;
	}
	public static void UN_LIST_MAIN<T>(List<T> list)
	{
		mListPool?.destroyList(list, Typeof<T>());
	}
	public static void LIST_MAIN<T>(out HashSet<T> list)
	{
		string stackTrace = EMPTY;
		if (mGameFramework.isEnablePoolStackTrace())
		{
			stackTrace = getStackTrace();
		}
		list = mHashSetPool.newList(Typeof<T>(), Typeof<HashSet<T>>(), stackTrace, true) as HashSet<T>;
	}
	public static void LIST_MAIN_PERSIST<T>(out HashSet<T> list)
	{
		string stackTrace = EMPTY;
		if (mGameFramework.isEnablePoolStackTrace())
		{
			stackTrace = getStackTrace();
		}
		list = mHashSetPool.newList(Typeof<T>(), Typeof<HashSet<T>>(), stackTrace, false) as HashSet<T>;
	}
	public static void UN_LIST_MAIN<T>(HashSet<T> list)
	{
		list.Clear();
		mHashSetPool?.destroyList(list, Typeof<T>());
	}
	public static void LIST_MAIN<K, V>(out Dictionary<K, V> list)
	{
		string stackTrace = EMPTY;
		if (mGameFramework.isEnablePoolStackTrace())
		{
			stackTrace = getStackTrace();
		}
		list = mDictionaryPool.newList(Typeof<K>(), Typeof<V>(), Typeof<Dictionary<K, V>>(), stackTrace, true) as Dictionary<K, V>;
	}
	public static void LIST_MAIN_PERSIST<K, V>(out Dictionary<K, V> list)
	{
		string stackTrace = EMPTY;
		if (mGameFramework.isEnablePoolStackTrace())
		{
			stackTrace = getStackTrace();
		}
		list = mDictionaryPool.newList(Typeof<K>(), Typeof<V>(), Typeof<Dictionary<K, V>>(), stackTrace, false) as Dictionary<K, V>;
	}
	public static void UN_LIST_MAIN<K, V>(Dictionary<K, V> list)
	{
		list.Clear();
		mDictionaryPool?.destroyList(list, Typeof<K>(), Typeof<V>());
	}
	public static void CLASS_MAIN_ONCE<T>(out T value) where T : ClassObject
	{
		value = mClassPool?.newClass(Typeof<T>(), true) as T;
	}
	public static ClassObject CLASS_MAIN_ONCE(Type type)
	{
		return mClassPool?.newClass(type, true);
	}
	public static void CLASS_MAIN<T>(out T value) where T : ClassObject
	{
		value = mClassPool?.newClass(Typeof<T>(), false) as T;
	}
	public static ClassObject CLASS_MAIN(Type type)
	{
		return mClassPool?.newClass(type, false);
	}
	public static void CLASS_MAIN_THREAD<T>(out T value) where T : ClassObject
	{
		value = mClassPoolThread?.newClass(Typeof<T>(), out _) as T;
	}
	public static T CLASS_MAIN_THREAD<T>(Type type) where T : ClassObject
	{
		return mClassPoolThread?.newClass(type, out _) as T;
	}
	public static void UN_CLASS(ClassObject obj)
	{
		mClassPool?.destroyClass(obj);
	}
	public static void UN_CLASS_THREAD(ClassObject obj)
	{
		mClassPoolThread?.destroyClass(obj);
	}
	public static void ARRAY_MAIN<T>(out T[] array, int count)
	{
		array = mArrayPool.newArray<T>(count, true);
	}
	public static void ARRAY_MAIN_PERSIST<T>(out T[] array, int count)
	{
		array = mArrayPool.newArray<T>(count, false);
	}
	public static void UN_ARRAY_MAIN<T>(T[] array, bool destroyReally = false)
	{
		mArrayPool.destroyArray(array, destroyReally);
	}
	public static void ARRAY_MAIN_THREAD<T>(out T[] array, int count)
	{
		array = mArrayPoolThread.newArray<T>(count);
	}
	public static void UN_ARRAY_MAIN_THREAD<T>(T[] array, bool destroyReally = false)
	{
		mArrayPoolThread.destroyArray(array, destroyReally);
	}
	//-----------------------------------------------------------------------------------------------------------------------------------
	// 字符串拼接
	public static MyStringBuilder STRING()
	{
		if(mClassPool == null)
		{
			return new MyStringBuilder();
		}
		return CLASS_MAIN_ONCE(typeof(MyStringBuilder)) as MyStringBuilder;
	}
	public static MyStringBuilder STRING(string str)
	{
		return STRING().Append(str);
	}
	public static MyStringBuilder STRING(string str0, string str1)
	{
		return STRING().Append(str0, str1);
	}
	public static MyStringBuilder STRING(string str0, string str1, string str2)
	{
		return STRING().Append(str0, str1, str2);
	}
	public static MyStringBuilder STRING(string str0, string str1, string str2, string str3)
	{
		return STRING().Append(str0, str1, str2, str3);
	}
	public static MyStringBuilder STRING(string str0, string str1, string str2, string str3, string str4)
	{
		return STRING().Append(str0, str1, str2, str3, str4);
	}
	public static MyStringBuilder STRING(string str0, string str1, string str2, string str3, string str4, string str5)
	{
		return STRING().Append(str0, str1, str2, str3, str4, str5);
	}
	public static MyStringBuilder STRING(string str0, string str1, string str2, string str3, string str4, string str5, string str6)
	{
		return STRING().Append(str0, str1, str2, str3, str4, str5, str6);
	}
	public static MyStringBuilder STRING(string str0, string str1, string str2, string str3, string str4, string str5, string str6, string str7)
	{
		return STRING().Append(str0, str1, str2, str3, str4, str5, str6, str7);
	}
	public static MyStringBuilder STRING(string str0, string str1, string str2, string str3, string str4, string str5, string str6, string str7, string str8)
	{
		return STRING().Append(str0, str1, str2, str3, str4, str5, str6, str7, str8);
	}
	public static MyStringBuilder STRING(string str0, string str1, string str2, string str3, string str4, string str5, string str6, string str7, string str8, string str9)
	{
		return STRING().Append(str0, str1, str2, str3, str4, str5, str6, str7, str8, str9);
	}
	public static MyStringBuilder STRING_THREAD()
	{
		if (mClassPoolThread == null)
		{
			return new MyStringBuilder();
		}
		return CLASS_MAIN_THREAD<MyStringBuilder>(typeof(MyStringBuilder));
	}
	public static MyStringBuilder STRING_THREAD(string str)
	{
		return STRING_THREAD().Append(str);
	}
	public static MyStringBuilder STRING_THREAD(string str0, string str1)
	{
		return STRING_THREAD().Append(str0, str1);
	}
	public static MyStringBuilder STRING_THREAD(string str0, string str1, string str2)
	{
		return STRING_THREAD().Append(str0, str1, str2);
	}
	public static MyStringBuilder STRING_THREAD(string str0, string str1, string str2, string str3)
	{
		return STRING_THREAD().Append(str0, str1, str2, str3);
	}
	public static MyStringBuilder STRING_THREAD(string str0, string str1, string str2, string str3, string str4)
	{
		return STRING_THREAD().Append(str0, str1, str2, str3, str4);
	}
	public static MyStringBuilder STRING_THREAD(string str0, string str1, string str2, string str3, string str4, string str5)
	{
		return STRING_THREAD().Append(str0, str1, str2, str3, str4, str5);
	}
	public static MyStringBuilder STRING_THREAD(string str0, string str1, string str2, string str3, string str4, string str5, string str6)
	{
		return STRING_THREAD().Append(str0, str1, str2, str3, str4, str5, str6);
	}
	public static MyStringBuilder STRING_THREAD(string str0, string str1, string str2, string str3, string str4, string str5, string str6, string str7)
	{
		return STRING_THREAD().Append(str0, str1, str2, str3, str4, str5, str6, str7);
	}
	public static MyStringBuilder STRING_THREAD(string str0, string str1, string str2, string str3, string str4, string str5, string str6, string str7, string str8)
	{
		return STRING_THREAD().Append(str0, str1, str2, str3, str4, str5, str6, str7, str8);
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
}