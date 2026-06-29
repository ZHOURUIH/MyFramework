
public class LaunchSceneVersion : SceneProcedure
{
    public override void init()
    {
        base.init();
        HybridCLRSystem.launchHotFix(null, null, null);
    }
}