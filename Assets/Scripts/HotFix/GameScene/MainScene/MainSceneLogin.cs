
public class MainSceneLogin : SceneProcedure
{
	protected override void onInit(SceneProcedure lastProcedure)
	{
		LT.LOAD_SHOW<UILogin>();
	}
	protected override void onExit(SceneProcedure nextProcedure)
	{
		LT.HIDE<UILogin>();
	}
}