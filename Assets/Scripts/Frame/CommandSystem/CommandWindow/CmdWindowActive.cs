using UnityEngine;

public class CmdWindowActive : Command
{
	public bool mActive;
	public override void resetProperty()
	{
		base.resetProperty();
		mActive = true;
	}
	public override void execute()
	{
		var uiObjcet = mReceiver as myUIObject;
		uiObjcet.setActive(mActive);
	}
	public override void showDebugInfo(MyStringBuilder builder)
	{
		builder.Append(": mActive:", mActive);
	}
}