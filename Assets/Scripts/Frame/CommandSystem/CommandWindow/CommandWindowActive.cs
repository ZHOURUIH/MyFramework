using UnityEngine;

public class CommandWindowActive : Command
{
	public bool mActive;
	public override void init()
	{
		base.init();
		mActive = true;
	}
	public override void execute()
	{
		myUIObject uiObjcet = mReceiver as myUIObject;
		uiObjcet.setActive(mActive);
	}
	public override string showDebugInfo()
	{
		return base.showDebugInfo() + ": mActive:" + mActive;
	}
}