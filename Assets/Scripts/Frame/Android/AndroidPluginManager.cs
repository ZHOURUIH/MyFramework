using System;
using System.Collections.Generic;
using UnityEngine;

public class AndroidPluginManager : FrameSystem
{
	protected static AndroidJavaClass mUnityPlayer;
	protected static AndroidJavaObject mMainActivity;
	public AndroidPluginManager()
	{
#if !UNITY_EDITOR && UNITY_ANDROID
		mUnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		mMainActivity = mUnityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
#endif
	}
	public static int getKeyboardHeight()
	{
		AndroidJavaObject View = mMainActivity.Get<AndroidJavaObject>("mUnityPlayer").Call<AndroidJavaObject>("getView");
		AndroidJavaObject Rct = new AndroidJavaObject("android.graphics.Rect");
		View.Call("getWindowVisibleDisplayFrame", Rct);
		return (int)getScreenSize().y - Rct.Call<int>("height");
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