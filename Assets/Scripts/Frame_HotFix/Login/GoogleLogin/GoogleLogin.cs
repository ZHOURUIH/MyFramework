using UnityEngine;
using static UnityUtility;
using static FrameBaseUtility;

// 用于Android平台下Google登录
public class GoogleLogin : FrameSystem
{
	protected static AndroidJavaClass mGoogleLogin;             // Java中加载类的实例
	protected string mServerClientID;
	protected string mAndroidClassPath;
	protected String2Callback mOnReceiveGoogleToken;
	public GoogleLogin()
	{
		if (!isEditor() && isAndroid())
		{
			mCreateObject = true;
		}
	}
	public override void init()
	{
		base.init();
		if (!isEditor() && isAndroid())
		{
			mObject.AddComponent<GoogleToken>();
		}
	}
	public void googleLogin()
	{
		if (AndroidPluginManager.getMainActivity() == null)
		{
			logError("AndroidPluginManager.getMainActivity() is null");
			return;
		}
		if (mGoogleLogin == null && !isEditor() && isAndroid())
		{
			mGoogleLogin = new(mAndroidClassPath);
		}
		mGoogleLogin.CallStatic("login", AndroidPluginManager.getMainActivity(), mServerClientID);
	}
	public void setReceiveGoogleTokenCallback(String2Callback callback) { mOnReceiveGoogleToken = callback; }
	public void setServerClientID(string serverClientID) { mServerClientID = serverClientID; }
	public void setAndroidClassPath(string path) { mAndroidClassPath = path; }
	public void onLogin(string gooleToken) { mOnReceiveGoogleToken?.Invoke(gooleToken, null); }
	public void onLoginError(string errorCode) { mOnReceiveGoogleToken?.Invoke(null, errorCode);  }
	public override void destroy()
	{
		base.destroy();
		mGoogleLogin?.Dispose();
		mGoogleLogin = null;
	}
}