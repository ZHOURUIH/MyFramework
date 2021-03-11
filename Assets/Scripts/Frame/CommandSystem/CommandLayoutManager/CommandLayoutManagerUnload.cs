using System;

public class CommandLayoutManagerUnload : Command 
{
	public int mLayoutID;
	public override void resetProperty()
	{
		base.resetProperty();
		mLayoutID = LAYOUT.NONE;
	}
	public override void execute()
	{
		mLayoutManager.destroyLayout(mLayoutID);
	}
	public override string showDebugInfo()
	{
		return base.showDebugInfo() + ": mLayoutID:" + mLayoutID;
	}
}