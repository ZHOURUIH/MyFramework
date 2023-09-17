using System;
using static FrameBase;

// 进入到指定的场景
public class CmdGameSceneManagerEnter : Command
{
	public Type mStartProcedure;	// 进入场景后要进入的流程,为空则表示进入场景的默认起始流程
	public Type mSceneType;			// 场景类型
	public string mIntent;			// 进入流程时要传递的参数
	public override void resetProperty()
	{
		base.resetProperty();
		mStartProcedure = null;
		mSceneType = null;
		mIntent = null;
	}
	public override void execute()
	{
		mGameSceneManager.enterScene(mSceneType, mStartProcedure, mIntent);
	}
	public override void debugInfo(MyStringBuilder builder)
	{
		builder.append(": mSceneType:", mSceneType).
				append(", mStartProcedure:", mStartProcedure).
				append(", mIntent:", mIntent);
	}
}