using UnityEngine;
using static UnityUtility;
using static FrameBaseUtility;

// 用于管理所有跟Java交互的对象
public class AndroidPluginManager : FrameSystem
{
	protected static AndroidJavaClass mUnityPlayer;			// 固定的UnityPlayer的Java实例
	protected static AndroidJavaObject mMainActivity;		// 固定的MainActivity的Java实例
	protected static AndroidJavaObject mApplication;		// 固定的Application的Java实例
	protected static AndroidJavaObject mApplicationContext;	// 固定的ApplicationContext的Java实例
	protected static string mAndroidPackageName;
	public static void initAnroidPlugin(string packageName)
	{
		mAndroidPackageName = packageName;
		if (!isEditor() && isAndroid())
		{
			mUnityPlayer = new("com.unity3d.player.UnityPlayer");
			mMainActivity = mUnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
			mApplication = mMainActivity.Call<AndroidJavaObject>("getApplication");
			mApplicationContext = mMainActivity.Call<AndroidJavaObject>("getApplicationContext");
			if (mMainActivity == null)
			{
				logError("mMainActivity is null");
			}
		}
	}
	public static int getKeyboardHeight()
	{
		var view = mMainActivity.Get<AndroidJavaObject>("mUnityPlayer").Call<AndroidJavaObject>("getView");
		AndroidJavaObject rect = new("android.graphics.Rect");
		view.Call("getWindowVisibleDisplayFrame", rect);
		return getScreenSize().y - rect.Call<int>("height");
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
	public static AndroidJavaClass getUnityPlayer() { return mUnityPlayer; }
	public static AndroidJavaObject getMainActivity() { return mMainActivity; }
	public static AndroidJavaObject getApplication() { return mApplication; }
	public static AndroidJavaObject getApplicationContext() { return mApplicationContext; }
	public static string getPackageName() { return mAndroidPackageName; }
}