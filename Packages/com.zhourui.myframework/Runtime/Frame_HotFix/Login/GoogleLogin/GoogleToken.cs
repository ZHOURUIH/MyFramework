using UnityEngine;
using Obfuz;
using static FrameBaseHotFix;

public class GoogleToken : MonoBehaviour
{
	[ObfuzIgnore]
	public void onLogin(string token)
    {
		mGoogleLogin.onLogin(token);
    }
	[ObfuzIgnore]
	public void onLoginError(string errorCode)
	{
		mGoogleLogin.onLoginError(errorCode);
	}
}