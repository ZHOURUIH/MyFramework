using UnityEngine;
using System.Collections.Generic;
using System;

public class LayoutLoadInfo : GB
{
	public GameLayout mLayout;
	public int mID;
	public int mOrder;
	public LAYOUT_ORDER mOrderType;
	public override void resetProperty()
	{
		base.resetProperty();
		mID = 0;
		mOrder = 0;
		mOrderType = LAYOUT_ORDER.ALWAYS_TOP;
		mLayout = null;
	}
}