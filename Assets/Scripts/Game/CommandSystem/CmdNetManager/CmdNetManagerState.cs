using System;

public class CmdNetManagerState : Command
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
	public override void debugInfo(MyStringBuilder builder)
	{
		builder.append(": mNetState:", mNetState.ToString());
	}
	//------------------------------------------------------------------------------------
	protected static void onMessageOK(bool ok, object userData)
	{
		mGameFramework.stop();
	}
}