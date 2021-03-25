using UnityEngine;
using System;
using System.Collections.Generic;

// 管理类初始化完成调用
// 这个父类的添加是方便代码的书写
// 继承FileUtility是为了在调用工具函数时方便,把四个完全独立的工具函数类串起来继承,所有继承自FrameBase的类都可以直接访问四大工具类中的函数
public class FrameBase : WidgetUtility
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
#if USE_ILRUNTIME
	public static ILRSystem mILRSystem;
#endif
#if !UNITY_EDITOR
	public static LocalLog mLocalLog;
#endif
	public virtual void notifyConstructDone()
	{
		mGameFramework = GameFramework.mGameFramework;
		mCommandSystem = mGameFramework.getSystem(Typeof<CommandSystem>()) as CommandSystem;
		mAudioManager = mGameFramework.getSystem(Typeof<AudioManager>()) as AudioManager;
		mGameSceneManager = mGameFramework.getSystem(Typeof<GameSceneManager>()) as GameSceneManager;
		mCharacterManager = mGameFramework.getSystem(Typeof<CharacterManager>()) as CharacterManager;
		mLayoutManager = mGameFramework.getSystem(Typeof<LayoutManager>()) as LayoutManager;
		mKeyFrameManager = mGameFramework.getSystem(Typeof<KeyFrameManager>()) as KeyFrameManager;
		mGlobalTouchSystem = mGameFramework.getSystem(Typeof<GlobalTouchSystem>()) as GlobalTouchSystem;
		mShaderManager = mGameFramework.getSystem(Typeof<ShaderManager>()) as ShaderManager;
#if !UNITY_IOS && !NO_SQLITE
		mSQLiteManager = mGameFramework.getSystem(Typeof<SQLiteManager>()) as SQLiteManager;
#endif
		mCameraManager = mGameFramework.getSystem(Typeof<CameraManager>()) as CameraManager;
		mResourceManager = mGameFramework.getSystem(Typeof<ResourceManager>()) as ResourceManager;
		mApplicationConfig = mGameFramework.getSystem(Typeof<ApplicationConfig>()) as ApplicationConfig;
		mFrameConfig = mGameFramework.getSystem(Typeof<FrameConfig>()) as FrameConfig;
		mObjectPool = mGameFramework.getSystem(Typeof<ObjectPool>()) as ObjectPool;
		mInputManager = mGameFramework.getSystem(Typeof<InputManager>()) as InputManager;
		mSceneSystem = mGameFramework.getSystem(Typeof<SceneSystem>()) as SceneSystem;
		mClassPool = mGameFramework.getSystem(Typeof<ClassPool>()) as ClassPool;
		mClassPoolThread = mGameFramework.getSystem(Typeof<ClassPoolThread>()) as ClassPoolThread;
		mListPool = mGameFramework.getSystem(Typeof<ListPool>()) as ListPool;
		mListPoolThread = mGameFramework.getSystem(Typeof<ListPoolThread>()) as ListPoolThread;
		mDictionaryPool = mGameFramework.getSystem(Typeof<DictionaryPool>()) as DictionaryPool;
		mDictionaryPoolThread = mGameFramework.getSystem(Typeof<DictionaryPoolThread>()) as DictionaryPoolThread;
		mArrayPool = mGameFramework.getSystem(Typeof<ArrayPool>()) as ArrayPool;
		mArrayPoolThread = mGameFramework.getSystem(Typeof<ArrayPoolThread>()) as ArrayPoolThread;
		mAndroidPluginManager = mGameFramework.getSystem(Typeof<AndroidPluginManager>()) as AndroidPluginManager;
		mAndroidAssetLoader = mGameFramework.getSystem(Typeof<AndroidAssetLoader>()) as AndroidAssetLoader;
		mHeadTextureManager = mGameFramework.getSystem(Typeof<HeadTextureManager>()) as HeadTextureManager;
		mTimeManager = mGameFramework.getSystem(Typeof<TimeManager>()) as TimeManager;
		mMovableObjectManager = mGameFramework.getSystem(Typeof<MovableObjectManager>()) as MovableObjectManager;
		mEffectManager = mGameFramework.getSystem(Typeof<EffectManager>()) as EffectManager;
		mTPSpriteManager = mGameFramework.getSystem(Typeof<TPSpriteManager>()) as TPSpriteManager;
		mSocketFactory = mGameFramework.getSystem(Typeof<SocketFactory>()) as SocketFactory;
		mSocketFactoryThread = mGameFramework.getSystem(Typeof<SocketFactoryThread>()) as SocketFactoryThread;
		mPathKeyframeManager = mGameFramework.getSystem(Typeof<PathKeyframeManager>()) as PathKeyframeManager;
		mEventSystem = mGameFramework.getSystem(Typeof<EventSystem>()) as EventSystem;
#if USE_ILRUNTIME
		mILRSystem = mGameFramework.getSystem(Typeof<ILRSystem>()) as ILRSystem;
#endif
	}
	// 方便书写代码添加的命令相关函数
	// 创建主工程中的命令实例
	public static T CMD<T>(out T cmd, bool show = true) where T : Command
	{
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("无法使用CMD创建非主工程的命令");
		}
		return cmd = mCommandSystem.newCmd(type, show) as T;
	}
	public static T CMD_DELAY<T>(out T cmd, bool show = true) where T : Command
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
		CMD(out T cmd, show);
		mCommandSystem.pushCommand(cmd, cmdReceiver);
	}
	public static void pushCommand(Command cmd, CommandReceiver cmdReceiver)
	{
		mCommandSystem.pushCommand(cmd, cmdReceiver);
	}
	public static T pushDelayMainCommand<T>(IDelayCmdWatcher watcher, CommandReceiver cmdReceiver, float delayExecute = 0.001f, bool show = true) where T : Command
	{
		CMD_DELAY(out T cmd, show);
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
	public static void changeProcedure(Type procedure, string intent = null)
	{
		CMD(out CommandGameSceneChangeProcedure cmd);
		cmd.mProcedure = procedure;
		cmd.mIntent = intent;
		pushCommand(cmd, mGameSceneManager.getCurScene());
	}
	public static CommandGameSceneChangeProcedure changeProcedureDelay(Type procedure, float delayTime = 0.001f, string intent = null)
	{
		CMD_DELAY(out CommandGameSceneChangeProcedure cmd, true);
		cmd.mProcedure = procedure;
		cmd.mIntent = intent;
		pushDelayCommand(cmd, mGameSceneManager.getCurScene(), delayTime);
		return cmd;
	}
	public static void prepareChangeProcedure(Type procedure, float prepareTime = 0.001f, string intent = null)
	{
		CMD(out CommandGameScenePrepareChangeProcedure cmd);
		cmd.mProcedure = procedure;
		cmd.mIntent = intent;
		cmd.mPrepareTime = prepareTime;
		pushCommand(cmd, mGameSceneManager.getCurScene());
	}
	public static bool getKeyCurrentDown(KeyCode key, FOCUS_MASK mask = FOCUS_MASK.NONE)
	{
		return mInputManager.getKeyCurrentDown(key, mask);
	}
	public static bool getKeyCurrentUp(KeyCode key, FOCUS_MASK mask = FOCUS_MASK.NONE)
	{
		return mInputManager.getKeyCurrentUp(key, mask);
	}
	public static bool getKeyDown(KeyCode key, FOCUS_MASK mask = FOCUS_MASK.NONE)
	{
		return mInputManager.getKeyDown(key, mask);
	}
	public static bool getKeyUp(KeyCode key, FOCUS_MASK mask = FOCUS_MASK.NONE)
	{
		return mInputManager.getKeyUp(key, mask);
	}
	public static Vector3 getMousePosition()
	{
		return mGlobalTouchSystem.getCurMousePosition();
	}
	public static GameScene getCurScene()
	{
		return mGameSceneManager.getCurScene();
	}
	public static void LIST<T>(out List<T> list, bool onlyOnce = true)
	{
		mListPool.newList(out list, onlyOnce);
	}
	public static void UN_LIST<T>(List<T> list)
	{
		mListPool?.destroyList(list);
	}
	public static void LIST<K, V>(out Dictionary<K, V> list, bool onlyOnce = true)
	{
		mDictionaryPool.newList(out list, onlyOnce);
	}
	public static void UN_LIST<K, V>(Dictionary<K, V> list)
	{
		mDictionaryPool?.destroyList(list);
	}
	public static void CLASS_ONCE<T>(out T value) where T : class, IClassObject
	{
		value = mClassPool?.newClass(Typeof<T>(), true) as T;
	}
	public static T CLASS_ONCE<T>(Type type) where T : class, IClassObject
	{
		return mClassPool?.newClass(type, true) as T;
	}
	public static void CLASS<T>(out T value) where T : class, IClassObject
	{
		value = mClassPool?.newClass(Typeof<T>(), false) as T;
	}
	public static T CLASS<T>(Type type) where T : class, IClassObject
	{
		return mClassPool?.newClass(type, false) as T;
	}
	public static void CLASS_THREAD<T>(out T value) where T : class, IClassObject
	{
		value = mClassPoolThread?.newClass(Typeof<T>(), out _) as T;
	}
	public static T CLASS_THREAD<T>(Type type) where T : class, IClassObject
	{
		return mClassPoolThread?.newClass(type, out _) as T;
	}
	public static void UN_CLASS(IClassObject obj)
	{
		mClassPool?.destroyClass(obj);
	}
	public static void UN_CLASS_THREAD(IClassObject obj)
	{
		mClassPoolThread?.destroyClass(obj);
	}
	public static void ARRAY<T>(out T[] array, int count, bool onlyOnce = true)
	{
		array = mArrayPool.newArray<T>(count, onlyOnce);
	}
	public static void UN_ARRAY<T>(T[] array, bool destroyReally = false)
	{
		mArrayPool.destroyArray(array, destroyReally);
	}
	public static void ARRAY_THREAD<T>(out T[] array, int count)
	{
		array = mArrayPoolThread.newArray<T>(count);
	}
	public static void UN_ARRAY_THREAD<T>(T[] array, bool destroyReally = false)
	{
		mArrayPoolThread.destroyArray(array, destroyReally);
	}
	public static MyStringBuilder STRING()
	{
		if(mClassPool == null)
		{
			return new MyStringBuilder();
		}
		return CLASS_ONCE<MyStringBuilder>(Typeof<MyStringBuilder>());
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
		return CLASS_THREAD<MyStringBuilder>(Typeof<MyStringBuilder>());
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