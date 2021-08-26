using System;

// 卸载一个布局
public class CmdLayoutManagerUnload : Command 
{
	public int mLayoutID;		// 布局ID
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
		builder.append(": mLayoutID:", mLayoutID);
	}
}