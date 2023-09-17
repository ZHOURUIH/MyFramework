using System;
using static CSharpUtility;

// 管理类初始化完成调用
// 这个父类的添加是方便代码的书写
// 因为要允许应用层来扩展此类,添加自己的系统组件引用,以及其他需要快速访问的变量,所以是partial类
public partial class FrameBase
{
	// FrameSystem
	public static GameFramework mGameFramework;
	public static CommandSystem mCommandSystem;
	public static GlobalCmdReceiver mGlobalCmdReceiver;
	public static AudioManager mAudioManager;
	public static GameSceneManager mGameSceneManager;
	public static CharacterManager mCharacterManager;
	public static LayoutManager mLayoutManager;
	public static KeyFrameManager mKeyFrameManager;
	public static GlobalTouchSystem mGlobalTouchSystem;
	public static ShaderManager mShaderManager;
	public static SQLiteManager mSQLiteManager;
	public static DllImportSystem mDllImportSystem;
	public static CameraManager mCameraManager;
	public static ResourceManager mResourceManager;
	public static PrefabPoolManager mPrefabPoolManager;
	public static InputSystem mInputSystem;
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
	public static NetPacketFactory mSocketFactory;
	public static PathKeyframeManager mPathKeyframeManager;
	public static EventSystem mEventSystem;
	public static TweenerManager mTweenerManager;
	public static StateManager mStateManager;
	public static NetPacketTypeManager mNetPacketTypeManager;
	public static GameObjectPool mGameObjectPool;
	public static ExcelManager mExcelManager;
	public static RedPointSystem mRedPointSystem;
	public static GameSetting mGameSetting;
#if USE_ILRUNTIME
	public static ILRSystem mILRSystem;
#endif
	// 一些方便获取的组件对象
	public static COMGameSettingAudio mCOMGameSettingAudio;
	public static void constructFrameDone()
	{
		mGameFramework = GameFramework.mGameFramework;
		getFrameSystemMain(out mCommandSystem);
		getFrameSystemMain(out mGlobalCmdReceiver);
		getFrameSystemMain(out mAudioManager);
		getFrameSystemMain(out mGameSceneManager);
		getFrameSystemMain(out mCharacterManager);
		getFrameSystemMain(out mLayoutManager);
		getFrameSystemMain(out mKeyFrameManager);
		getFrameSystemMain(out mGlobalTouchSystem);
		getFrameSystemMain(out mShaderManager);
#if !NO_SQLITE
		getFrameSystemMain(out mSQLiteManager);
#endif
		getFrameSystemMain(out mDllImportSystem);
		getFrameSystemMain(out mCameraManager);
		getFrameSystemMain(out mResourceManager);
		getFrameSystemMain(out mPrefabPoolManager);
		getFrameSystemMain(out mInputSystem);
		getFrameSystemMain(out mSceneSystem);
		getFrameSystemMain(out mClassPool);
		getFrameSystemMain(out mClassPoolThread);
		getFrameSystemMain(out mListPool);
		getFrameSystemMain(out mListPoolThread);
		getFrameSystemMain(out mHashSetPool);
		getFrameSystemMain(out mHashSetPoolThread);
		getFrameSystemMain(out mDictionaryPool);
		getFrameSystemMain(out mDictionaryPoolThread);
		getFrameSystemMain(out mArrayPool);
		getFrameSystemMain(out mArrayPoolThread);
		getFrameSystemMain(out mAndroidPluginManager);
		getFrameSystemMain(out mAndroidAssetLoader);
		getFrameSystemMain(out mHeadTextureManager);
		getFrameSystemMain(out mTimeManager);
		getFrameSystemMain(out mMovableObjectManager);
		getFrameSystemMain(out mEffectManager);
		getFrameSystemMain(out mTPSpriteManager);
		getFrameSystemMain(out mSocketFactory);
		getFrameSystemMain(out mPathKeyframeManager);
		getFrameSystemMain(out mEventSystem);
		getFrameSystemMain(out mTweenerManager);
		getFrameSystemMain(out mStateManager);
		getFrameSystemMain(out mNetPacketTypeManager);
		getFrameSystemMain(out mGameObjectPool);
		getFrameSystemMain(out mExcelManager);
		getFrameSystemMain(out mRedPointSystem);
		getFrameSystemMain(out mGameSetting);
#if USE_ILRUNTIME
		getFrameSystemMain(out mILRSystem);
#endif
	}
	public static void frameSystemInitDone()
	{
		mCOMGameSettingAudio = mGameSetting.getComponent<COMGameSettingAudio>(false, false);
	}
	public static void getFrameSystemMain<T>(out T system) where T : FrameSystem
	{
		system = mGameFramework.getSystem(Typeof<T>()) as T;
	}
}