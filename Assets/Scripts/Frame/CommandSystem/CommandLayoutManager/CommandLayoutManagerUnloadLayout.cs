using UnityEngine;
using System.Collections;

public class CommandLayoutManagerUnloadLayout : Command 
{
	public LAYOUT mLayoutType;
	public override void init()
	{
		base.init();
		mLayoutType = LAYOUT.MAX;
	}
	public override void execute()
	{
		mLayoutManager.destroyLayout(mLayoutType);
	}
	public override string showDebugInfo()
	{
		return base.showDebugInfo() + ": mLayoutType:" + mLayoutType;
	}
}