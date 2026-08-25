using System;
using System.Collections.Generic;
using UnityEngine;
using static MathUtility;

// 用于存储触点检测时的物体
public class MouseCastObjectSet : ClassObject
{
	public static Comparison<MouseCastObjectSet> mCompareDescend = depthDescend;	// 避免GC的委托
	public List<IMouseEventCollect> mObjectOrderList = new();		// 物体列表
	public GameCamera mCamera;										// 渲染此物体列表的摄像机
	protected Camera mLastCamera;                                   // 上一次实际用于射线检测的Unity Camera
	protected Matrix4x4 mLastWorldToCameraMatrix;                   // 上一次摄像机世界到相机矩阵
	protected Matrix4x4 mLastProjectionMatrix;                      // 上一次摄像机投影矩阵
	protected Rect mLastPixelRect;                                  // 上一次摄像机像素区域
	protected float mLastCameraDepth;                               // 上一次摄像机深度
	protected bool mCameraStateValid;                               // 摄像机状态缓存是否有效
	public override void resetProperty()
	{
		base.resetProperty();
		mObjectOrderList.Clear();
		mCamera = null;
		mLastCamera = null;
		mLastWorldToCameraMatrix = Matrix4x4.identity;
		mLastProjectionMatrix = Matrix4x4.identity;
		mLastPixelRect = default;
		mLastCameraDepth = 0.0f;
		mCameraStateValid = false;
	}
	public void setCamera(GameCamera camera)
	{
		if (mCamera == camera)
		{
			return;
		}
		mCamera = camera;
		mCameraStateValid = false;
	}
	public void addObject(IMouseEventCollect obj) { mObjectOrderList.Add(obj); }
	public bool removeObject(IMouseEventCollect obj) { return mObjectOrderList.Remove(obj); }
	public bool isEmpty() { return mObjectOrderList.Count == 0; }
	public bool checkCameraStateChanged(GameCamera cameraObject)
	{
		Camera camera = cameraObject?.getCamera();
		if (camera == null)
		{
			bool changed = mCameraStateValid || mLastCamera != null;
			mLastCamera = null;
			mCameraStateValid = false;
			return changed;
		}
		Matrix4x4 worldToCameraMatrix = camera.worldToCameraMatrix;
		Matrix4x4 projectionMatrix = camera.projectionMatrix;
		Rect pixelRect = camera.pixelRect;
		float cameraDepth = camera.depth;
		if (mCameraStateValid &&
			mLastCamera == camera &&
			mLastWorldToCameraMatrix == worldToCameraMatrix &&
			mLastProjectionMatrix == projectionMatrix &&
			mLastPixelRect == pixelRect &&
			mLastCameraDepth == cameraDepth)
		{
			return false;
		}
		mLastCamera = camera;
		mLastWorldToCameraMatrix = worldToCameraMatrix;
		mLastProjectionMatrix = projectionMatrix;
		mLastPixelRect = pixelRect;
		mLastCameraDepth = cameraDepth;
		mCameraStateValid = true;
		return true;
	}
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