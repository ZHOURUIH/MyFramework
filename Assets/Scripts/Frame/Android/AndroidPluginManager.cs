using UnityEngine;

// 用于管理所有跟Java交互的对象
public class AndroidPluginManager : FrameSystem
{
	protected static AndroidJavaClass mUnityPlayer;		// 固定的UnityPlayer的Java实例
	protected static AndroidJavaObject mMainActivity;	// 固定的MainActivity的Java实例
	public AndroidPluginManager()
	{
#if !UNITY_EDITOR && UNITY_ANDROID
		mUnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		mMainActivity = mUnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
#endif
	}
	public static int getKeyboardHeight()
	{
		var view = mMainActivity.Get<AndroidJavaObject>("mUnityPlayer").Call<AndroidJavaObject>("getView");
		var rect = new AndroidJavaObject("android.graphics.Rect");
		view.Call("getWindowVisibleDisplayFrame", rect);
		return (int)getScreenSize().y - rect.Call<int>("height");
	}
	public override void destroy()
	{
#if !UNITY_EDITOR && UNITY_ANDROID
		mUnityPlayer.Dispose();
		mMainActivity.Dispose();
#endif
		base.destroy();
	}
	public static AndroidJavaObject getMainActivity() { return mMainActivity; }
}