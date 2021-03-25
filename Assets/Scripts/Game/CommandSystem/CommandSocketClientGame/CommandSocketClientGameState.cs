using System;

public class CommandSocketClientGameState : Command
{
	public NET_STATE mNetState;
	public override void resetProperty()
	{
		base.resetProperty();
		mNetState = NET_STATE.SERVER_CLOSE;
	}
	public override void execute()
	{
		;
	}
	public override void showDebugInfo(MyStringBuilder builder)
	{
		builder.Append(": mNetState:", mNetState.ToString());
	}
	//------------------------------------------------------------------------------------
	protected static void onMessageOK(bool ok, object userData)
	{
		mGame.stop();
	}
}