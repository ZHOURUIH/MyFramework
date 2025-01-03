using System;
#if USE_APPLE_LOGIN
using AppleAuth;
using AppleAuth.Enums;
using AppleAuth.Extensions;
using AppleAuth.Interfaces;
using AppleAuth.Native;
#endif
using UnityEngine;
using static UnityUtility;

// 用于Apple登录
public class AppleLogin : FrameSystem
{
#if USE_APPLE_LOGIN
	protected string mAppleUserIdKey = "AppleUserId";
	protected IAppleAuthManager mAppleAuthManager;
	protected Action<string, ICredential> mLoginSuccessCallback;		// 登录成功的回调
	protected Action<AuthorizationErrorCode> mLoginErrorCallback;		// 登录失败的回调
	protected Action mLoginRevokedCallback;								// 登录撤销的回调
	public AppleLogin()
	{
		if (AppleAuthManager.IsCurrentPlatformSupported)
		{
			// Creates a default JSON deserializer, to transform JSON Native responses to C# instances
			// Creates an Apple Authentication manager with the deserializer
			mAppleAuthManager = new AppleAuthManager(new PayloadDeserializer());
		}
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		mAppleAuthManager?.Update();
	}
	public void setAppleUserIDKey(string key) { mAppleUserIdKey = key; }
	// 设置回调
	public void initCallback(Action<string, ICredential> successCallback, Action<AuthorizationErrorCode> errorCallback, Action loginRevokedCallback)
	{
		mLoginSuccessCallback = successCallback;
		mLoginErrorCallback = errorCallback;
		mLoginRevokedCallback = loginRevokedCallback;
		// 登录被撤销的回调
		mAppleAuthManager?.SetCredentialsRevokedCallback((result) =>
		{
			log("Received revoked callback " + result);
			mLoginRevokedCallback?.Invoke();
			PlayerPrefs.DeleteKey(mAppleUserIdKey);
		});
	}
	// 主动点击登录
	public void login()
	{
		mAppleAuthManager.LoginWithAppleId(new AppleAuthLoginArgs(LoginOptions.IncludeEmail | LoginOptions.IncludeFullName), (credential) =>
		{
			// If a sign in with apple succeeds, we should have obtained the credential with the user id, name, and email, save it
			PlayerPrefs.SetString(mAppleUserIdKey, credential.User);
			mLoginSuccessCallback?.Invoke(credential.User, credential);
		},
		(error) =>
		{
			AuthorizationErrorCode authorizationErrorCode = error.GetAuthorizationErrorCode();
			logWarning("Sign in with Apple failed " + authorizationErrorCode.ToString() + " " + error.ToString());
			mLoginErrorCallback?.Invoke(authorizationErrorCode);
		});
	}
	// 根据缓存数据尝试自动登录
	public void tryAutoLogin()
	{
		// If we have an Apple User Id available, get the credential status for it
		if (PlayerPrefs.HasKey(mAppleUserIdKey))
		{
			string storedAppleUserId = PlayerPrefs.GetString(mAppleUserIdKey);
			// If there is an apple ID available, we should check the credential state
			mAppleAuthManager.GetCredentialState(storedAppleUserId, (state) =>
			{
				// If it's authorized, login with that user id
				if (state == CredentialState.Authorized)
				{
					mLoginSuccessCallback?.Invoke(storedAppleUserId, null);
				}
				// If it was revoked, or not found, we need a new sign in with apple attempt
				// Discard previous apple user id
				else if (state == CredentialState.Revoked)
				{
					PlayerPrefs.DeleteKey(mAppleUserIdKey);
					mLoginRevokedCallback?.Invoke();
				}
				else if (state == CredentialState.NotFound)
				{
					PlayerPrefs.DeleteKey(mAppleUserIdKey);
					mLoginErrorCallback?.Invoke(AuthorizationErrorCode.Unknown);
					logWarning("Error while trying to get credential state, CredentialState:" + state);
				}
			},
			(error) =>
			{
				AuthorizationErrorCode authorizationErrorCode = error.GetAuthorizationErrorCode();
				logWarning("Error while trying to get credential state, error code:" + authorizationErrorCode.ToString() + ", " + error.ToString());
				mLoginErrorCallback?.Invoke(authorizationErrorCode);
			});
		}
		// If we do not have an stored Apple User Id, attempt a quick login
		else
		{
			// Quick login should succeed if the credential was authorized before and not revoked
			mAppleAuthManager.QuickLogin(new AppleAuthQuickLoginArgs(), (credential) =>
			{
				// If it's an Apple credential, save the user ID, for later logins
				if (credential is IAppleIDCredential)
				{
					PlayerPrefs.SetString(mAppleUserIdKey, credential.User);
				}
				mLoginSuccessCallback?.Invoke(credential.User, credential);
			}, (error) =>
			{
				// If Quick Login fails, we should show the normal sign in with apple menu, to allow for a normal Sign In with apple
				AuthorizationErrorCode authorizationErrorCode = error.GetAuthorizationErrorCode();
				logWarning("Quick Login Failed " + authorizationErrorCode.ToString() + " " + error.ToString());
				mLoginErrorCallback?.Invoke(authorizationErrorCode);
			});
		}
	}
#endif
}