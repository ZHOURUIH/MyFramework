using UnityEngine;
using static FrameBaseUtility;

// 用于管理所有跟Java交互的对象
public class AndroidPluginManager : FrameSystem
{
	protected static AndroidJavaClass mUnityPlayer;     // 固定的UnityPlayer的Java实例
	protected static AndroidJavaObject mMainActivity;   // 固定的MainActivity的Java实例
	protected static AndroidJavaObject mApplication;    // 固定的Application的Java实例
	protected static string mAndroidPackageName;
	public static void initAnroidPlugin(string packageName)
	{
		mAndroidPackageName = packageName;
		if (!isEditor() && isAndroid())
		{
			mUnityPlayer = new("com.unity3d.player.UnityPlayer");
			mMainActivity = mUnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
			mApplication = mMainActivity.Call<AndroidJavaObject>("getApplication");
			if (mMainActivity == null)
			{
				logErrorBase("mMainActivity is null");
			}
		}
	}
	public override void destroy()
	{
		mUnityPlayer?.Dispose();
		mMainActivity?.Dispose();
		mApplication?.Dispose();
		mUnityPlayer = null;
		mMainActivity = null;
		mApplication = null;
		base.destroy();
	}
	public static AndroidJavaObject getMainActivity() { return mMainActivity; }
	public static AndroidJavaObject getApplication() { return mApplication; }
	public static string getPackageName() { return mAndroidPackageName; }
}