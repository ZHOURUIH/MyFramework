using UnityEngine;
using static FrameBase;

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