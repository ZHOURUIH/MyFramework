using UnityEngine;
#if USE_OBFUZ
using Obfuz;
#endif
using static FrameBaseHotFix;

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