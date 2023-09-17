using System;
using static FrameUtility;

public class MainSceneLoading : SceneProcedure
{
	protected override void onInit(SceneProcedure lastProcedure, string intent)
	{
		changeProcedureDelay(typeof(MainSceneLogin));
	}
	protected override void onExit(SceneProcedure nextProcedure) { }
}