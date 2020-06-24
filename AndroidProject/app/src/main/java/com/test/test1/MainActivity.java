package 此处替换为自己的包名;

import android.os.Bundle;
import android.util.Log;

import com.unity3d.player.UnityPlayer;
import com.unity3d.player.UnityPlayerActivity;

public class MainActivity extends UnityPlayerActivity
{
    public AssetLoader mAssetLoader;
    @Override
    protected void onCreate(Bundle savedInstanceState)
    {
        super.onCreate(savedInstanceState);
        mAssetLoader = new AssetLoader();
        mAssetLoader.setAssetManager(getAssets());
    }
    public static void unityLog(String info)
    {
        Log.i("MainActivity", info);
        UnityPlayer.UnitySendMessage("UnityLog", "log", info);
    }
    public static void unityError(String info)
    {
        Log.i("MainActivity", info);
        UnityPlayer.UnitySendMessage("UnityLog", "logError", info);
    }
}
