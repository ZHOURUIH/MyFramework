using static FrameBase;

public class LaunchSceneExit : SceneProcedure
{
	public override void exit()
	{
		base.exit();
		// 因为更新资源以后,所有的资源都会卸载然后重新加载一次,已经加载的资源就会出现丢失的情况,所以此处卸载之前加载的布局
		mLayoutManager.unloadAllLayout();
	}
}