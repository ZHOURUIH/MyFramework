using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class MainSceneLoading : ILRSceneProcedure
{
	protected override void onInit(SceneProcedure lastProcedure, string intent)
	{
		changeProcedureDelay(typeof(MainSceneLogin));
	}
	protected override void onExit(SceneProcedure nextProcedure) { }
}