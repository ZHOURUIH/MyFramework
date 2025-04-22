using static FrameUtility;
using static GBH;

public class MainSceneLoading : SceneProcedure
{
	protected override void onInit(SceneProcedure lastProcedure)
	{
		// 连接服务器
		mNetManager.connect((bool success)=>
		{
			changeProcedure<MainSceneLogin>();
		});
	}
	protected override void onExit(SceneProcedure nextProcedure) { }
}