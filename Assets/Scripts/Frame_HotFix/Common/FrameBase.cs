
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
	public static GamePluginManager mGamePluginManager;
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
	public static ByteArrayPool mByteArrayPool;
	public static ByteArrayPoolThread mByteArrayPoolThread;
	public static AndroidPluginManager mAndroidPluginManager;
	public static AndroidAssetLoader mAndroidAssetLoader;
	public static AndroidMainClass mAndroidMainClass;
	public static HeadTextureManager mHeadTextureManager;
	public static TimeManager mTimeManager;
	public static MovableObjectManager mMovableObjectManager;
	public static EffectManager mEffectManager;
	public static TPSpriteManager mTPSpriteManager;
	public static NetPacketFactory mNetPacketFactory;
	public static PathKeyframeManager mPathKeyframeManager;
	public static EventSystem mEventSystem;
	public static TweenerManager mTweenerManager;
	public static StateManager mStateManager;
	public static NetPacketTypeManager mNetPacketTypeManager;
	public static GameObjectPool mGameObjectPool;
	public static ExcelManager mExcelManager;
	public static RedPointSystem mRedPointSystem;
	public static GameSetting mGameSetting;
	public static AssetVersionSystem mAssetVersionSystem;
	public static GlobalKeyProcess mGlobalKeyProcess;
	public static LocalizationManager mLocalizationManager;
	public static AsyncTaskGroupManager mAsyncTaskGroupManager;
	public static GoogleLogin mGoogleLogin;
	public static AppleLogin mAppleLogin;
	public static ScreenOrientationSystem mScreenOrientationSystem;
	public static WaitingManager mWaitingManager;
	public static UndoManager mUndoManager;
	public static AndroidPurchasing mAndroidPurchasing;
	public static PurchasingSystem mPurchasingSystem;
	// 一些方便获取的组件对象
	public static COMGameSettingAudio mCOMGameSettingAudio;
	public static void frameSystemInitDone()
	{
		mGameSetting.getComponent(out mCOMGameSettingAudio);
	}
}