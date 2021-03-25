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
	public override void showDebugInfo(MyStringBuilder builder)
	{
		builder.Append(": mActive:", mActive);
	}
}