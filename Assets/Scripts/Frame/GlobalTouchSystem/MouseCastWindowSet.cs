using UnityEngine;
using System;
using System.Collections.Generic;

public class MouseCastWindowSet : FrameBase
{
	public static Comparison<MouseCastWindowSet> mComparisonDescend = cameraDepthDescend;
	public static Comparison<IMouseEventCollect> mUIDepthDescend = UIDepthDescend;
	protected HashSet<IMouseEventCollect> mWindowSet;		// 用于查找的窗口列表
	protected List<IMouseEventCollect> mWindowOrderList;	// 深度由大到小的窗口列表
	protected GameCamera mCamera;
	protected bool mDepthDirty;
	public MouseCastWindowSet()
	{
		mWindowOrderList = new List<IMouseEventCollect>();
		mWindowSet = new HashSet<IMouseEventCollect>();
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mWindowSet.Clear();
		mWindowOrderList.Clear();
		mCamera = null;
		mDepthDirty = false;
	}
	public void setCamera(GameCamera camera) { mCamera = camera; }
	public void addWindow(IMouseEventCollect window)
	{
		if (mWindowSet.Contains(window))
		{
			return;
		}
		if (window.isDestroy())
		{
			logError("窗口已经被销毁,无法访问:" + window.getName());
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
		if (window.isDestroy())
		{
			logError("窗口已经被销毁,无法访问:" + window.getName());
			return;
		}
		mWindowOrderList.Remove(window);
	}
	public bool isEmpty() { return mWindowSet.Count == 0; }
	//------------------------------------------------------------------------------------------------------------------------------
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