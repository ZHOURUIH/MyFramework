﻿using static FrameBaseHotFix;

public class MainSceneExit : SceneProcedure
{
	protected override void onInit(SceneProcedure lastProcedure, string intent) { }
	protected override void onExit(SceneProcedure nextProcedure)
	{
		// 一般在场景的Exit流程中,卸载该场景的所有布局,确保没有资源遗留
		genericExit(0);
	}
}