using UnityEngine;
using System.Collections;

public class CommandLayoutManagerLayoutVisible : Command
{
	public LAYOUT mLayoutType;
	public string mParam;
	public bool mForce;
	public bool mImmediately;
	public bool mVisibility;
	public override void init()
	{
		base.init();
		mLayoutType = LAYOUT.MAX;
		mParam = null;
		mForce = false;
		mImmediately = false;
		mVisibility = true;
	}
	public override void execute()
	{
		GameLayout layout = mLayoutManager.getGameLayout(mLayoutType);
		if (!mForce)
		{
			layout?.setVisible(mVisibility, mImmediately, mParam);
		}
		else
		{
			layout?.setVisibleForce(mVisibility);
		}
	}
	public override string showDebugInfo()
	{
		return base.showDebugInfo() + ": mLayoutType:" + mLayoutType + ", mVisibility:" + mVisibility + ", mForce:" + mForce + ", mImmediately:" + mImmediately + ", mParam:" + mParam;
	}
}