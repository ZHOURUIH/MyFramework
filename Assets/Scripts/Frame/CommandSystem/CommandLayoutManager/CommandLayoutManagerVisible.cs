using UnityEngine;
using System.Collections;

public class CommandLayoutManagerVisible : Command
{
	public int mLayoutID;
	public string mParam;
	public bool mForce;
	public bool mImmediately;
	public bool mVisibility;
	public override void init()
	{
		base.init();
		mLayoutID = LAYOUT.NONE;
		mParam = null;
		mForce = false;
		mImmediately = false;
		mVisibility = true;
	}
	public override void execute()
	{
		GameLayout layout = mLayoutManager.getGameLayout(mLayoutID);
		if (layout == null)
		{
			return;
		}
		// 自动计算渲染顺序的布局在显示时,需要重新计算当前渲染顺序
		LAYOUT_ORDER orderType = layout.getRenderOrderType();
		if (mVisibility && (orderType == LAYOUT_ORDER.ALWAYS_TOP_AUTO || orderType == LAYOUT_ORDER.AUTO))
		{
			int renderOrder = mLayoutManager.generateRenderOrder(layout.getRenderOrder(), orderType);
			if (renderOrder != layout.getRenderOrder())
			{
				layout.setRenderOrder(renderOrder);
			}
		}
		if (!mForce)
		{
			layout.setVisible(mVisibility, mImmediately, mParam);
		}
		else
		{
			layout.setVisibleForce(mVisibility);
		}
	}
	public override string showDebugInfo()
	{
		return base.showDebugInfo() + ": mLayoutID:" + mLayoutID + ", mVisibility:" + mVisibility + ", mForce:" + mForce + ", mImmediately:" + mImmediately + ", mParam:" + mParam;
	}
}