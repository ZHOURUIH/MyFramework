using System;
using static UnityUtility;

// 准备跳转到当前场景的指定流程,准备跳转过程中不会被中断
public class CmdGameScenePrepareChangeProcedure
{
	// 要跳转到的流程类型
	// 跳转时要传递的参数
	// 准备跳转的时间
	public static void execute(GameScene scene, Type procedure, string intent, float prepareTime)
	{
		// 准备时间必须大于0
		if (prepareTime <= 0.0f)
		{
			logError("preapare time must be larger than 0!");
			return;
		}
		// 正在准备跳转时,不允许再次准备跳转
		SceneProcedure curProcedure = scene.getCurProcedure();
		if (curProcedure.isPreparingExit())
		{
			logError("procedure is preparing to exit, can not prepare again!");
			return;
		}
		scene.prepareChangeProcedure(procedure, prepareTime, intent);
	}
}