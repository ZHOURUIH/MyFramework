using static LT;

public class MainSceneLogin : SceneProcedure
{
	protected override void onInit(SceneProcedure lastProcedure)
	{
		LOAD<UILogin>();
	}
	protected override void onExit(SceneProcedure nextProcedure)
	{
		HIDE<UILogin>();
	}
}