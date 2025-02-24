using UnityEngine;
using static FrameBaseHotFix;

public class GoogleToken : MonoBehaviour
{
    public void onLogin(string token)
    {
		mGoogleLogin.onLogin(token);
    }
	public void onLoginError(string errorCode)
	{
		mGoogleLogin.onLoginError(errorCode);
	}
}