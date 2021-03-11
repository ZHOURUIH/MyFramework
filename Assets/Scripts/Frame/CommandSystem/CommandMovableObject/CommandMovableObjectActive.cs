using System;

public class CommandMovableObjectActive : Command
{
	public bool mActive;
	public override void resetProperty()
	{
		base.resetProperty();
		mActive = true;
	}
	public override void execute()
	{
		var obj = mReceiver as MovableObject;
		obj.setActive(mActive);
	}
	public override string showDebugInfo()
	{
		return base.showDebugInfo() + ": mActive:" + mActive;
	}
}