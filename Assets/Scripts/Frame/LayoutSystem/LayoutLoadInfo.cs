using UnityEngine;
using System.Collections.Generic;
using System;

// 布局加载时的一些信息
public class LayoutLoadInfo : FrameBase
{
	public GameLayout mLayout;		// 加载的布局
	public int mID;					// 布局ID
	public int mOrder;				// 布局渲染顺序
	public LAYOUT_ORDER mOrderType;	// 布局渲染顺序类型
	public override void resetProperty()
	{
		base.resetProperty();
		mID = 0;
		mOrder = 0;
		mOrderType = LAYOUT_ORDER.ALWAYS_TOP;
		mLayout = null;
	}
}