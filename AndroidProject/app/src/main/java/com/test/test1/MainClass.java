package 包名;

import android.content.Context;
import android.app.Activity;
import android.content.Intent;
import android.net.Uri;
import android.os.Build;
import androidx.core.content.FileProvider;
import android.os.BatteryManager;
import android.util.Log;

//import com.android.installreferrer.api.InstallReferrerClient;
//import com.android.installreferrer.api.InstallReferrerStateListener;
//import com.android.installreferrer.api.ReferrerDetails;
import com.unity3d.player.UnityPlayer;

import java.io.File;

public class MainClass
{
    static BatteryManager mBatteryManager;
    public static void gameStart(Context context)
    {
        unityLog("gameStart");
        //startGoogleCollection(context);
    }
    public static void unityLog(String info)
    {
        Log.i("MainActivity", info);
        // 第一个参数是GameObject的名字
        UnityPlayer.UnitySendMessage("UnityLog", "log", info);
    }
    public static void unityError(String info)
    {
        Log.e("MainActivity", info);
        // 第一个参数是GameObject的名字
        UnityPlayer.UnitySendMessage("UnityLog", "logError", info);
    }
    // 获取当前电流大小,单位微安
    public static int getBatteryEnergy()
    {
        if (mBatteryManager == null)
        {
            mBatteryManager = (BatteryManager) UnityPlayer.currentActivity.getSystemService(Context.BATTERY_SERVICE);
        }
        if (Build.VERSION.SDK_INT >= Build.VERSION_CODES.LOLLIPOP)
        {
            return mBatteryManager.getIntProperty(BatteryManager.BATTERY_PROPERTY_CURRENT_NOW);
        }
        else
        {
            return 0;
        }
    }
//		这个函数是用于解析谷歌广告归因
//    public static void startGoogleCollection(Context context)
//    {
//        InstallReferrerClient referrerClient = InstallReferrerClient.newBuilder(context).build();
//        referrerClient.startConnection(new InstallReferrerStateListener()
//        {
//            @Override
//            public void onInstallReferrerSetupFinished(int responseCode)
//            {
//                switch (responseCode)
//                {
//                    case InstallReferrerClient.InstallReferrerResponse.OK:
//                        try
//                        {
//                            ReferrerDetails response = referrerClient.getInstallReferrer();
//                            String referrerUrl = response.getInstallReferrer();
//                            unityLog("referrerURL:" + referrerUrl);
//                            long clickTimestamp = response.getReferrerClickTimestampSeconds();
//                            long installTimestamp = response.getInstallBeginTimestampSeconds();
//                            // 解析并处理获取到的广告来源信息，例如 referrerUrl 中的 utm_source 等
//                            UnityPlayer.UnitySendMessage("ReferrerDetailsListener", "setReferrerDetails", "{\"referrerUrl\":\"" + referrerUrl + "\", \"clickTimestamp\":" + clickTimestamp + ", \"installTimestamp\":" + installTimestamp + "}");
//                        }
//                        catch (RemoteException e)
//                        {
//                            unityLog("onInstallReferrerSetupFinished error:" + e.getMessage());
//                        }
//                        break;
//                    case InstallReferrerClient.InstallReferrerResponse.FEATURE_NOT_SUPPORTED:
//                        unityLog("API不支持");
//                        break;
//                    case InstallReferrerClient.InstallReferrerResponse.SERVICE_UNAVAILABLE:
//                        unityLog("Google Play 服务不可用");
//                        break;
//                }
//            }
//            @Override
//            public void onInstallReferrerServiceDisconnected()
//            {
//                // 处理断开连接
//            }
//        });
//    }
}

