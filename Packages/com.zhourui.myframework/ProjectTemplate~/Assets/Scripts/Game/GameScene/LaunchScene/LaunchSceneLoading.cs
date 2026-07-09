using static GameUtility;
using static FileUtility;
using static FrameBaseUtility;

public class LaunchSceneVersion : SceneProcedure
{
    public override void init()
    {
        base.init();
        launch();
    }
    //---------------------------------------------------------------------------------------------------------------------------
    protected void onLaunchError()
    {
        logBase("dll资源加载失败");
    }
    protected void launch()
    {
        HybridCLRSystem.launchHotFix(getAESKeyBytes(), getAESIVBytes(), (string fileName, BytesIntCallback callback) =>
        {
            openFileAsync(availableReadPath(fileName), true, bytes => callback?.Invoke(bytes, bytes.Length));
        }, onLaunchError);
    }
}