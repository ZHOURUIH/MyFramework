using System;

public class CommandSocketClientGameState : Command
{
	public NET_STATE mNetState;
	public override void init()
	{
		base.init();
		mNetState = NET_STATE.SERVER_CLOSE;
	}
	public override void execute()
	{
		;
	}
	public override string showDebugInfo()
	{
		return base.showDebugInfo() + ": mNetState:" + mNetState;
	}
	//------------------------------------------------------------------------------------
	protected static void onMessageOK(bool ok, object userData)
	{
		mGame.stop();
	}
}