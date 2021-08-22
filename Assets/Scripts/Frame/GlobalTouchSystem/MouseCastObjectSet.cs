using System;
using System.Collections.Generic;

public class MouseCastObjectSet : FrameBase
{
	public static Comparison<MouseCastObjectSet> mCompareDescend = depthDescend;
	public List<IMouseEventCollect> mObjectOrderList;
	public GameCamera mCamera;
	public MouseCastObjectSet()
	{
		mObjectOrderList = new List<IMouseEventCollect>();
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mObjectOrderList.Clear();
		mCamera = null;
	}
	public void setCamera(GameCamera camera) { mCamera = camera; }
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
	//------------------------------------------------------------------------------------------------------------------------------
	protected static int depthDescend(MouseCastObjectSet a, MouseCastObjectSet b)
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
		return (int)sign(b.mCamera.getCameraDepth() - a.mCamera.getCameraDepth());
	}
}