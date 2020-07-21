using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class MouseCastWindowSet
{
	public GameCamera mCamera;
	public SortedDictionary<UIDepth, List<IMouseEventCollect>> mWindowOrderList; // 深度由大到小的窗口列表
	public MouseCastWindowSet(GameCamera camera)
	{
		mCamera = camera;
		mWindowOrderList = new SortedDictionary<UIDepth, List<IMouseEventCollect>>(new DepthDescend());
	}
	public void addWindow(UIDepth depth, IMouseEventCollect window)
	{
		if (!mWindowOrderList.ContainsKey(depth))
		{
			mWindowOrderList.Add(depth, new List<IMouseEventCollect>());
		}
		mWindowOrderList[depth].Add(window);
	}
	public bool hasWindow(UIDepth depth, IMouseEventCollect window)
	{
		if(!mWindowOrderList.ContainsKey(depth))
		{
			return false;
		}
		return mWindowOrderList[depth].Contains(window);
	}
	public void windowDepthChanged(UIDepth lastDepth, IMouseEventCollect window)
	{
		// 移除旧的按钮深度
		mWindowOrderList[lastDepth].Remove(window);
		// 添加新的按钮深度
		UIDepth newDepth = window.getUIDepth();
		if (!mWindowOrderList.ContainsKey(newDepth))
		{
			mWindowOrderList.Add(newDepth, new List<IMouseEventCollect>());
		}
		mWindowOrderList[newDepth].Add(window);
	}
	public void removeWindow(IMouseEventCollect window)
	{
		UIDepth depth = window.getUIDepth();
		mWindowOrderList[depth].Remove(window);
		if(mWindowOrderList[depth].Count == 0)
		{
			mWindowOrderList.Remove(depth);
		}
	}
	public bool isEmpty() { return mWindowOrderList.Count == 0; }
	public static int depthDescend(MouseCastWindowSet a, MouseCastWindowSet b)
	{
		return (int)MathUtility.sign(b.mCamera.getCameraDepth() - a.mCamera.getCameraDepth());
	}
}