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
        HybridCLRSystem.launchHotFix(onLaunchError);
    }
}