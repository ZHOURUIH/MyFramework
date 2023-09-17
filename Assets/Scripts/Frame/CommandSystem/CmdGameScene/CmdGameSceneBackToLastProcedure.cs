using System;

// 返回到上一个流程
public class CmdGameSceneBackToLastProcedure
{
	// intent,跳转流程时要传递的参数
	public static void execute(GameScene scene, string intent)
	{
		scene.backToLastProcedure(intent);
	}
}