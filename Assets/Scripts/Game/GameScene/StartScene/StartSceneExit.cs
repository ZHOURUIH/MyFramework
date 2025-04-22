using static FrameBase;

public class StartSceneExit : SceneProcedure
{
	public override void exit()
	{
		// 一般在资源更新结束以后都会卸载所有在res中的资源,虽然很可能在这之前已经调用Game层的destroy了
		mLayoutManager.unloadAllLayout();
	}
}