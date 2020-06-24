using UnityEngine;
using System.Collections;
using System;

// 管理类初始化完成调用
// 这个父类的添加是方便代码的书写
// 继承FileUtility是为了在调用工具函数时方便,把四个完全独立的工具函数类串起来继承,所有继承自FrameBase的类都可以直接访问四大工具类中的函数
public class FrameBase : UnityUtility
{
	// FrameComponent
	public static GameFramework				mGameFramework			= null;
	public static CommandSystem				mCommandSystem			= null;
	public static AudioManager				mAudioManager			= null;
	public static GameSceneManager			mGameSceneManager		= null;
	public static CharacterManager			mCharacterManager		= null;
	public static GameLayoutManager			mLayoutManager			= null;
	public static KeyFrameManager			mKeyFrameManager		= null;
	public static GlobalTouchSystem			mGlobalTouchSystem		= null;
	public static ShaderManager				mShaderManager			= null;
#if !UNITY_IOS && !NO_SQLITE
	public static SQLite					mSQLite					= null;
#endif
	public static DataBase					mDataBase				= null;
	public static CameraManager				mCameraManager			= null;
	public static ResourceManager			mResourceManager		= null;
	public static ApplicationConfig			mApplicationConfig		= null;
	public static FrameConfig				mFrameConfig			= null;
	public static ObjectPool				mObjectPool				= null;
	public static InputManager				mInputManager			= null;
	public static SceneSystem				mSceneSystem			= null;
	public static IFrameLogSystem			mFrameLogSystem			= null;
	public static ClassPool					mClassPool				= null;
	public static AndroidPluginManager		mAndroidPluginManager	= null;
	public static AndroidAssetLoader		mAndroidAssetLoader		= null;
	public static HeadTextureManager		mHeadTextureManager		= null;
	public static TimeManager				mTimeManager			= null;
	public static MovableObjectManager		mMovableObjectManager	= null;
	public static EffectManager				mEffectManager			= null;
	public static TPSpriteManager			mTPSpriteManager		= null;
	public static SocketFactory				mSocketFactory			= null;
	public static PathKeyframeManager		mPathKeyframeManager	= null;
#if !UNITY_EDITOR
	public static LocalLog					mLocalLog				= null;
#endif
	// 由于该常量非常常用,所以定义到FrameBase中,方便使用
	public const string EMPTY_STRING = "";
	public virtual void notifyConstructDone()
	{
		mGameFramework = GameFramework.mGameFramework;
		mGameFramework.getSystem(out mCommandSystem);
		mGameFramework.getSystem(out mAudioManager);
		mGameFramework.getSystem(out mGameSceneManager);
		mGameFramework.getSystem(out mCharacterManager);
		mGameFramework.getSystem(out mLayoutManager);
		mGameFramework.getSystem(out mKeyFrameManager);
		mGameFramework.getSystem(out mGlobalTouchSystem);
		mGameFramework.getSystem(out mShaderManager);
#if !UNITY_IOS && !NO_SQLITE
		mGameFramework.getSystem(out mSQLite);
#endif
		mGameFramework.getSystem(out mDataBase);
		mGameFramework.getSystem(out mCameraManager);
		mGameFramework.getSystem(out mResourceManager);
		mGameFramework.getSystem(out mApplicationConfig);
		mGameFramework.getSystem(out mFrameConfig);
		mGameFramework.getSystem(out mObjectPool);
		mGameFramework.getSystem(out mInputManager);
		mGameFramework.getSystem(out mSceneSystem);
		mGameFramework.getSystem(out mClassPool);
		mGameFramework.getSystem(out mAndroidPluginManager);
		mGameFramework.getSystem(out mAndroidAssetLoader);
		mGameFramework.getSystem(out mHeadTextureManager);
		mGameFramework.getSystem(out mTimeManager);
		mGameFramework.getSystem(out mMovableObjectManager);
		mGameFramework.getSystem(out mEffectManager);
		mGameFramework.getSystem(out mTPSpriteManager);
		mGameFramework.getSystem(out mSocketFactory);
		mGameFramework.getSystem(out mPathKeyframeManager);
	}
	// 方便书写代码添加的命令相关函数
	public static T newCmd<T>(out T cmd, bool show = true, bool delay = false) where T : Command, new()
	{
		return cmd = mCommandSystem.newCmd<T>(show, delay);
	}
	public static Command newCmd(out Command cmd, Type type, bool show = true, bool delay = false)
	{
		return cmd = mCommandSystem.newCmd(type, show, delay);
	}
	public static void pushCommand<T>(CommandReceiver cmdReceiver, bool show = true) where T : Command, new()
	{
		mCommandSystem.pushCommand<T>(cmdReceiver, show);
	}
	public static void pushCommand(Command cmd, CommandReceiver cmdReceiver)
	{
		mCommandSystem.pushCommand(cmd, cmdReceiver);
	}
	public static T pushDelayCommand<T>(CommandReceiver cmdReceiver, float delayExecute = 0.001f, bool show = true) where T : Command, new()
	{
		return mCommandSystem.pushDelayCommand<T>(cmdReceiver, delayExecute);
	}
	public static void pushDelayCommand(Command cmd, CommandReceiver cmdReceiver, float delayExecute = 0.001f)
	{
		mCommandSystem.pushDelayCommand(cmd, cmdReceiver, delayExecute);
	}
	public static void changeProcedure<T>(string intent = EMPTY_STRING) where T : SceneProcedure
	{
		CommandGameSceneChangeProcedure cmd = newCmd(out cmd);
		cmd.mProcedure = typeof(T);
		cmd.mIntent = intent;
		pushCommand(cmd, mGameSceneManager.getCurScene());
	}
	public static void changeProcedure(Type procedure, string intent = EMPTY_STRING)
	{
		CommandGameSceneChangeProcedure cmd = newCmd(out cmd);
		cmd.mProcedure = procedure;
		cmd.mIntent = intent;
		pushCommand(cmd, mGameSceneManager.getCurScene());
	}
	public static CommandGameSceneChangeProcedure changeProcedureDelay<T>(float delayTime = 0.001f, string intent = EMPTY_STRING) where T : SceneProcedure
	{
		CommandGameSceneChangeProcedure cmd = newCmd(out cmd, true, true);
		cmd.mProcedure = typeof(T);
		cmd.mIntent = intent;
		pushDelayCommand(cmd, mGameSceneManager.getCurScene(), delayTime);
		return cmd;
	}
	public static CommandGameSceneChangeProcedure changeProcedureDelay(Type procedure, float delayTime = 0.001f, string intent = EMPTY_STRING)
	{
		CommandGameSceneChangeProcedure cmd = newCmd(out cmd, true, true);
		cmd.mProcedure = procedure;
		cmd.mIntent = intent;
		pushDelayCommand(cmd, mGameSceneManager.getCurScene(), delayTime);
		return cmd;
	}
	public static void prepareChangeProcedure<T>(float prepareTime = 0.001f, string intent = EMPTY_STRING) where T : SceneProcedure
	{
		CommandGameScenePrepareChangeProcedure cmd = newCmd(out cmd);
		cmd.mProcedure = typeof(T);
		cmd.mIntent = intent;
		cmd.mPrepareTime = prepareTime;
		pushCommand(cmd, mGameSceneManager.getCurScene());
	}
	public static void prepareChangeProcedure(Type procedure, float prepareTime = 0.001f, string intent = EMPTY_STRING)
	{
		CommandGameScenePrepareChangeProcedure cmd = newCmd(out cmd);
		cmd.mProcedure = procedure;
		cmd.mIntent = intent;
		cmd.mPrepareTime = prepareTime;
		pushCommand(cmd, mGameSceneManager.getCurScene());
	}
	public static bool getKeyCurrentDown(KeyCode key, FOCUS_MASK mask = FOCUS_MASK.FM_NONE)
	{
		return mInputManager.getKeyCurrentDown(key, mask);
	}
	public static bool getKeyCurrentUp(KeyCode key, FOCUS_MASK mask = FOCUS_MASK.FM_NONE)
	{
		return mInputManager.getKeyCurrentUp(key, mask);
	}
	public static bool getKeyDown(KeyCode key, FOCUS_MASK mask = FOCUS_MASK.FM_NONE)
	{
		return mInputManager.getKeyDown(key, mask);
	}
	public static bool getKeyUp(KeyCode key, FOCUS_MASK mask = FOCUS_MASK.FM_NONE)
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
	public static T getCurScene<T>() where T : GameScene
	{
		return mGameSceneManager.getCurScene() as T;
	}
}