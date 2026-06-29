using System;
using System.Collections.Generic;
using static MathUtility;

// 用于存储触点检测时的物体
public class MouseCastObjectSet : ClassObject
{
	public static Comparison<MouseCastObjectSet> mCompareDescend = depthDescend;	// 避免GC的委托
	public List<IMouseEventCollect> mObjectOrderList = new();		// 物体列表
	public GameCamera mCamera;										// 渲染此物体列表的摄像机
	public override void resetProperty()
	{
		base.resetProperty();
		mObjectOrderList.Clear();
		mCamera = null;
	}
	public void setCamera(GameCamera camera) { mCamera = camera; }
	public void addObject(IMouseEventCollect obj) { mObjectOrderList.Add(obj); }
	public bool removeObject(IMouseEventCollect obj) { return mObjectOrderList.Remove(obj); }
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