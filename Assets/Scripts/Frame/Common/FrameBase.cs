using UnityEngine;
using System;
using System.Collections.Generic;

// 管理类初始化完成调用
// 这个父类的添加是方便代码的书写
// 继承FileUtility是为了在调用工具函数时方便,把四个完全独立的工具函数类串起来继承,所有继承自FrameBase的类都可以直接访问四大工具类中的函数
public class FrameBase : UnityUtility
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
	public static SQLite mSQLite;
#endif
	public static DataBase mDataBase;
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
	public static BytesPool mBytesPool;
	public static BytesPoolThread mBytesPoolThread;
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
	public static StringBuilderPool mStringBuilderPool;
	public static StringBuilderPoolThread mStringBuilderPoolThread;
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
		mSQLite = mGameFramework.getSystem(Typeof<SQLite>()) as SQLite;
#endif
		mDataBase = mGameFramework.getSystem(Typeof<DataBase>()) as DataBase;
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
		mBytesPool = mGameFramework.getSystem(Typeof<BytesPool>()) as BytesPool;
		mBytesPoolThread = mGameFramework.getSystem(Typeof<BytesPoolThread>()) as BytesPoolThread;
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
		mStringBuilderPool = mGameFramework.getSystem(Typeof<StringBuilderPool>()) as StringBuilderPool;
		mStringBuilderPoolThread = mGameFramework.getSystem(Typeof<StringBuilderPoolThread>()) as StringBuilderPoolThread;
#if USE_ILRUNTIME
		mILRSystem = mGameFramework.getSystem(Typeof<ILRSystem>()) as ILRSystem;
#endif
	}
	// 方便书写代码添加的命令相关函数
	// 创建主工程中的命令实例
	public static T CMD<T>(out T cmd, bool show = true, bool delay = false) where T : Command
	{
		Type type = Typeof<T>();
		if (type == null)
		{
			logError("无法使用CMD创建非主工程的命令");
		}
		return cmd = mCommandSystem.newCmd(type, show, delay) as T;
	}
	public static void pushMainCommand<T>(CommandReceiver cmdReceiver, bool show = true, bool delay = false) where T : Command
	{
		CMD(out T cmd, show, delay);
		mCommandSystem.pushCommand(cmd, cmdReceiver);
	}
	public static void pushCommand(Command cmd, CommandReceiver cmdReceiver)
	{
		mCommandSystem.pushCommand(cmd, cmdReceiver);
	}
	public static T pushDelayMainCommand<T>(IDelayCmdWatcher watcher, CommandReceiver cmdReceiver, float delayExecute = 0.001f, bool show = true) where T : Command
	{
		CMD(out T cmd, show, true);
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
		CMD(out CommandGameSceneChangeProcedure cmd, true, true);
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
	public static List<T> LIST<T>(out List<T> list, bool onlyOnce = true)
	{
		return mListPool.newList(out list, onlyOnce);
	}
	public static void UN_LIST<T>(List<T> list)
	{
		mListPool.destroyList(list);
	}
	public static Dictionary<K, V> LIST<K, V>(out Dictionary<K, V> list, bool onlyOnce = true)
	{
		return mDictionaryPool.newList(out list, onlyOnce);
	}
	public static void UN_LIST<K, V>(Dictionary<K, V> list)
	{
		mDictionaryPool.destroyList(list);
	}
	public static T CLASS<T>(out T value) where T : class, IClassObject
	{
		Type type = Typeof<T>();
		value = mClassPool.newClass(type) as T;
		return value;
	}
	public static T CLASS<T>(Type type) where T : class, IClassObject
	{
		return mClassPool.newClass(type) as T;
	}
	public static void UN_CLASS(IClassObject obj)
	{
		mClassPool.destroyClass(obj);
	}
	public static MyStringBuilder newBuilder()
	{
		if(mStringBuilderPool == null)
		{
			return new MyStringBuilder();
		}
		return mStringBuilderPool.newBuilder();
	}
	public static void destroyBuilder(MyStringBuilder builder, bool destroyReally = false)
	{
		mStringBuilderPool?.destroyBuilder(builder, destroyReally);
	}
	public static string valueDestroyBuilder(MyStringBuilder builder, bool destroyReally = false)
	{
		string str = builder.ToString();
		mStringBuilderPool?.destroyBuilder(builder, destroyReally);
		return str;
	}
}