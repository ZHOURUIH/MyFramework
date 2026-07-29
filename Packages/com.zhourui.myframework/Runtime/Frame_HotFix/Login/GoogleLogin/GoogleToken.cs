using UnityEngine;
#if USE_OBFUZ
using Obfuz;
#endif
using static FrameBaseHotFix;

// Google登录返回的Token信息
public class GoogleToken : MonoBehaviour
{
#if USE_OBFUZ
    [ObfuzIgnore]
#endif
	public void onLogin(string token)
    {
		mGoogleLogin.onLogin(token);
    }
#if USE_OBFUZ
    [ObfuzIgnore]
#endif
	public void onLoginError(string errorCode)
	{
		mGoogleLogin.onLoginError(errorCode);
	}
}