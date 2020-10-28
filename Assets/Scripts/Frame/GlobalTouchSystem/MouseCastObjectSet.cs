using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class MouseCastObjectSet
{
	public List<IMouseEventCollect> mObjectOrderList;
	public GameCamera mCamera;
	public MouseCastObjectSet(GameCamera camera)
	{
		mCamera = camera;
		mObjectOrderList = new List<IMouseEventCollect>();
	}
	public void addObject(IMouseEventCollect obj)
	{
		mObjectOrderList.Add(obj);
	}
	public bool hasObject(IMouseEventCollect obj)
	{
		return mObjectOrderList.Contains(obj);
	}
	public void removeObject(IMouseEventCollect obj)
	{
		mObjectOrderList.Remove(obj);
	}
	public bool isEmpty() { return mObjectOrderList.Count == 0; }
	public static int depthDescend(MouseCastObjectSet a, MouseCastObjectSet b)
	{
		if (a.mCamera == null && b.mCamera == null)
		{
			return 0;
		}
		if (a.mCamera == null)
		{
			return 1;
		}
		if (b.mCamera == null)
		{
			return -1;
		}
		return (int)MathUtility.sign(b.mCamera.getCameraDepth() - a.mCamera.getCameraDepth());
	}
}