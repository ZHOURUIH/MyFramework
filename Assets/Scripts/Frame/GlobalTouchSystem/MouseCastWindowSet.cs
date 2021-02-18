using UnityEngine;
using System;
using System.Collections.Generic;

public class MouseCastWindowSet : FrameBase
{
	protected HashSet<IMouseEventCollect> mWindowSet;		// 用于查找的窗口列表
	protected List<IMouseEventCollect> mWindowOrderList;	// 深度由大到小的窗口列表
	protected GameCamera mCamera;
	protected bool mDepthDirty;
	public static Comparison<MouseCastWindowSet> mComparisonDescend = cameraDepthDescend;
	public static Comparison<IMouseEventCollect> mUIDepthDescend = UIDepthDescend;
	public MouseCastWindowSet(GameCamera camera)
	{
		mCamera = camera;
		mWindowOrderList = new List<IMouseEventCollect>();
		mWindowSet = new HashSet<IMouseEventCollect>();
	}
	public void addWindow(IMouseEventCollect window)
	{
		if (mWindowSet.Contains(window))
		{
			return;
		}
		mWindowOrderList.Add(window);
		mWindowSet.Add(window);
		mDepthDirty = true;
	}
	public bool hasWindow(IMouseEventCollect window)
	{
		return mWindowSet.Contains(window);
	}
	public void windowDepthChanged()
	{
		mDepthDirty = true;
	}
	public GameCamera getCamera() { return mCamera; }
	public List<IMouseEventCollect> getWindowOrderList()
	{
		if(mDepthDirty)
		{
			mDepthDirty = false;
			quickSort(mWindowOrderList, mUIDepthDescend);
		}
		return mWindowOrderList;
	}
	public void removeWindow(IMouseEventCollect window)
	{
		if (!mWindowSet.Remove(window))
		{
			return;
		}
		mWindowOrderList.Remove(window);
	}
	public bool isEmpty() { return mWindowSet.Count == 0; }
	//-------------------------------------------------------------------------------------------------------
	// a小于b返回1, a等于b返回0, a大于b返回-1
	protected static int UIDepthDescend(IMouseEventCollect a, IMouseEventCollect b)
	{
		return UIDepth.compare(a.getDepth(), b.getDepth());
	}
	protected static int cameraDepthDescend(MouseCastWindowSet a, MouseCastWindowSet b)
	{
		return (int)sign(b.mCamera.getCameraDepth() - a.mCamera.getCameraDepth());
	}
}