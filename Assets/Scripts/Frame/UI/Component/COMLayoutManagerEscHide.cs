using UnityEngine;
using System;
using System.Collections.Generic;
using static UnityUtility;
using static FrameBase;
using static MathUtility;

// 用于处理一些需要监听ESC键来关闭的布局
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
			if (!mLayoutRenderOrderList.Contains(layout))
			{
				mLayoutRenderOrderList.Add(layout);
				mLayoutRenderOrderList.Sort(mCompareLayoutRenderOrder);
			}
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
			GameLayout layout = mLayoutRenderOrderList[i];
			if (layout == null)
			{
				continue;
			}
			if (layout.getScript() == null)
			{
				logError(layout.getName() + "已经销毁");
				continue;
			}
			try
			{
				if (layout.getScript().onESCDown())
				{
					break;
				}
			}
			catch(Exception e)
			{
				logException(e, "layout:" + layout.getName());
				break;
			}
		}
	}
	protected int compareLayoutRenderOrder(GameLayout x, GameLayout y)
	{
		return sign(x.getRenderOrder() - y.getRenderOrder());
	}
}