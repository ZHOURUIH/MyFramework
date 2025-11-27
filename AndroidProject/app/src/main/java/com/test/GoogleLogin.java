//package 包名;
//
//import android.content.Context;
//import com.google.android.libraries.identity.googleid.GetSignInWithGoogleOption;
//import com.google.android.libraries.identity.googleid.GoogleIdTokenCredential;
//import java.util.concurrent.Executors;
//import androidx.annotation.NonNull;
//import androidx.credentials.ClearCredentialStateRequest;
//import androidx.credentials.Credential;
//import androidx.credentials.CredentialManager;
//import androidx.credentials.CredentialManagerCallback;
//import androidx.credentials.CustomCredential;
//import androidx.credentials.GetCredentialRequest;
//import androidx.credentials.GetCredentialResponse;
//import androidx.credentials.PasswordCredential;
//import androidx.credentials.PublicKeyCredential;
//import androidx.credentials.exceptions.ClearCredentialException;
//import androidx.credentials.exceptions.GetCredentialException;
//import com.unity3d.player.UnityPlayer;
//
//// 谷歌凭据管理器登录
//public class GoogleLogin
//{
//    private static CredentialManager mCredentialManager;
//    public static void login(Context context, String clientID)
//    {
//        mCredentialManager = CredentialManager.Companion.create(context);
//        GetSignInWithGoogleOption googleIdOption = new GetSignInWithGoogleOption.Builder(clientID)
//                .build();
//        GetCredentialRequest request = new GetCredentialRequest.Builder().addCredentialOption(googleIdOption).build();
//
//        android.os.CancellationSignal cancellationSignal = new android.os.CancellationSignal();
//        cancellationSignal.setOnCancelListener(() ->
//        {
//            MainClass.unityLog("Preparing credentials with Google was cancelled.");
//        });
//
//        mCredentialManager.getCredentialAsync(context, request, cancellationSignal, Executors.newSingleThreadExecutor(),
//                new CredentialManagerCallback<GetCredentialResponse, GetCredentialException>()
//                {
//                    @Override
//                    public void onResult(GetCredentialResponse result)
//                    {
//                        handleSignIn(result);
//                    }
//                    @Override
//                    public void onError(GetCredentialException e)
//                    {
//                        MainClass.unityError("GetCredentialException:" + e.getMessage());
//                        doLogin(null, "1");
//                    }
//                });
//    }
//    public static void doLogin(String token, String errorCode)
//    {
//        if (errorCode == null)
//        {
//            UnityPlayer.UnitySendMessage("GoogleLogin", "onLogin", token);
//        }
//        else
//        {
//            UnityPlayer.UnitySendMessage("GoogleLogin", "onLoginError", errorCode);
//        }
//    }
//    public static void handleSignIn(GetCredentialResponse result)
//    {
//        if (result == null)
//        {
//            doLogin(null, "2");
//        }
//        try
//        {
//            Credential credential = result.getCredential();
//            if (credential instanceof PublicKeyCredential)
//            {
//                //String responseJson = ((PublicKeyCredential) credential).getAuthenticationResponseJson();
//                doLogin(null, "3");
//            }
//            else if (credential instanceof PasswordCredential)
//            {
//                //String username = ((PasswordCredential) credential).getId();
//                //String password = ((PasswordCredential) credential).getPassword();
//                doLogin(null, "4");
//            }
//            else if (credential instanceof CustomCredential)
//            {
//                if (GoogleIdTokenCredential.TYPE_GOOGLE_ID_TOKEN_CREDENTIAL.equals(credential.getType()))
//                {
//                    GoogleIdTokenCredential googleIdTokenCredential = GoogleIdTokenCredential.createFrom(((CustomCredential) credential).getData());
//                    doLogin(googleIdTokenCredential.getIdToken(), null);
//                }
//                else
//                {
//                    MainClass.unityError("Unexpected type of credential");
//                    doLogin(null, "5");
//                }
//            }
//            else
//            {
//                MainClass.unityError("Unexpected type of credential");
//                doLogin(null, "6");
//            }
//        }
//        catch (Exception e)
//        {
//            MainClass.unityError("credential exception:" + e.getMessage());
//            doLogin(null, "7");
//        }
//    }
//    // 注销登录
//    public static void Logout()
//    {
//        ClearCredentialStateRequest clearCredentialStateRequest = new ClearCredentialStateRequest();
//        android.os.CancellationSignal cancellationSignal = new android.os.CancellationSignal();
//        cancellationSignal.setOnCancelListener(() ->
//        {
//            MainClass.unityLog("Preparing credentials with Google was cancelled.");
//        });
//        if (mCredentialManager == null)
//        {
//            return;
//        }
//        mCredentialManager.clearCredentialStateAsync(clearCredentialStateRequest, cancellationSignal, Executors.newSingleThreadExecutor(),
//                new CredentialManagerCallback<Void, ClearCredentialException>()
//                {
//                    @Override
//                    public void onResult(Void unused) {}
//                    @Override
//                    public void onError(@NonNull ClearCredentialException e) {}
//                }
//        );
//        MainClass.unityLog("google注销登录");
//    }
//}