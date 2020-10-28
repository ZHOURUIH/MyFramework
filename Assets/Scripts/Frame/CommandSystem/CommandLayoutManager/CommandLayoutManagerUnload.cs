using UnityEngine;
using System.Collections;

public class CommandLayoutManagerUnload : Command 
{
	public int mLayoutID;
	public override void init()
	{
		base.init();
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