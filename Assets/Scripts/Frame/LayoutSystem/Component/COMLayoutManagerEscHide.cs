using UnityEngine;
using System;
using System.Collections.Generic;

public class COMLayoutManagerEscHide : GameComponent
{
	protected Comparison<GameLayout> mCompareLayoutRenderOrder;	// 比较布局渲染顺序的函数,避免GC
	protected List<GameLayout> mLayoutRenderOrderList;			// 按渲染顺序排序的布局列表,只有已显示的列表
	public COMLayoutManagerEscHide()
	{
		mLayoutRenderOrderList = new List<GameLayout>();
		mCompareLayoutRenderOrder = compareLayoutRenderOrder;
	}
	public override void init(ComponentOwner owner)
	{
		base.init(owner);
		mInputSystem.listenKeyCurrentDown(KeyCode.Escape, onESCDown, this);
	}
	public override void resetProperty()
	{
		base.resetProperty();
		// mCompareLayoutRenderOrder不重置
		// mCompareLayoutRenderOrder = null;
		mLayoutRenderOrderList.Clear();
	}
	public void notifyLayoutRenderOrder()
	{
		mLayoutRenderOrderList.Sort(mCompareLayoutRenderOrder);
	}
	public void notifyLayoutVisible(bool visible, GameLayout layout)
	{
		if (visible)
		{
			mLayoutRenderOrderList.Add(layout);
			mLayoutRenderOrderList.Sort(mCompareLayoutRenderOrder);
		}
		else
		{
			mLayoutRenderOrderList.Remove(layout);
		}
	}
	public override void destroy()
	{
		mInputSystem.unlistenKey(this);
		base.destroy();
	}
	public void notifyLayoutDestroy(GameLayout layout)
	{
		mLayoutRenderOrderList.Remove(layout);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void onESCDown()
	{
		// 从上往下逐级发送ESC按下事件,有布局处理后就不再传递
		int count = mLayoutRenderOrderList.Count;
		for (int i = count - 1; i >= 0; --i)
		{
			if (mLayoutRenderOrderList[i].getScript().onESCDown())
			{
				break;
			}
		}
	}
	protected int compareLayoutRenderOrder(GameLayout x, GameLayout y)
	{
		return sign(x.getRenderOrder() - y.getRenderOrder());
	}
}