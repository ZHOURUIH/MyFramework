using System;

public class CmdLayoutManagerUnload : Command 
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
	public override void debugInfo(MyStringBuilder builder)
	{
		builder.Append(": mLayoutID:", mLayoutID);
	}
}