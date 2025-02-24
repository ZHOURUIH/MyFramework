
// 管理类初始化完成调用,这个类的添加是方便代码的书写
public class FrameBase
{
	// FrameSystem
	public static GameFramework mGameFramework;
	public static CommandSystem mCommandSystem;
	public static GlobalCmdReceiver mGlobalCmdReceiver;
	public static AudioManager mAudioManager;
	public static GameSceneManager mGameSceneManager;
	public static LayoutManager mLayoutManager;
	public static KeyFrameManager mKeyFrameManager;
	public static GlobalTouchSystem mGlobalTouchSystem;
	public static CameraManager mCameraManager;
	public static ResourceManager mResourceManager;
	public static PrefabPoolManager mPrefabPoolManager;
	public static InputSystem mInputSystem;
	public static ClassPool mClassPool;
	public static ClassPoolThread mClassPoolThread;
	public static ListPool mListPool;
	public static HashSetPool mHashSetPool;
	public static DictionaryPool mDictionaryPool;
	public static ArrayPoolThread mArrayPoolThread;
	public static AndroidPluginManager mAndroidPluginManager;
	public static AndroidAssetLoader mAndroidAssetLoader;
	public static AndroidMainClass mAndroidMainClass;
	public static TPSpriteManager mTPSpriteManager;
	public static GameObjectPool mGameObjectPool;
	public static AssetVersionSystem mAssetVersionSystem;
	public static AsyncTaskGroupManager mAsyncTaskGroupManager;
	public static ScreenOrientationSystem mScreenOrientationSystem;
	public static WaitingManager mWaitingManager;
}