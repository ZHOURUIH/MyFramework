using System;
using System.Collections.Generic;
using UnityEngine;

public class GameCamera : MovableObject
{
	protected List<CameraLinker> mLinkerList;	// 由于对连接器的操作比较多,所以单独放到一个表中,组件列表中不变
	protected CameraLinker mCurLinker;			// 只是记录当前连接器方便外部获取
	protected Camera mCamera;					// 摄像机组件
	protected int mLastVisibleLayer;			// 上一次的渲染层
	protected bool mProcessKey;					// 锁定摄像机的按键控制
	// 如果要实现摄像机震动,则需要将摄像机挂接到一个节点上,一般操作的是父节点的Transform,震动时是操作摄像机自身节点的Transform
	public GameCamera()
	{
		mLinkerList = new List<CameraLinker>();
		setDestroyObject(false);
	}
	public override void setObject(GameObject obj, bool destroyOld = true)
	{
		base.setObject(obj, destroyOld);
		mCamera = mObject.GetComponent<Camera>();
#if UNITY_EDITOR
		getUnityComponent<CameraDebug>().setCamera(this);
#endif
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mCamera = null;
		mCurLinker = null;
		mLinkerList.Clear();
		mProcessKey = false;
		mLastVisibleLayer = 0;
	}
	public void unlinkTarget()
	{
		mCurLinker?.setLinkObject(null);
		mCurLinker?.setActive(false);
		mCurLinker = null;
	}
	public CameraLinker linkTarget(Type linkerType, MovableObject target)
	{
		var linker = getComponent(linkerType) as CameraLinker;
		if(linker == null)
		{
			return null;
		}
		// 先断开旧的连接器
		unlinkTarget();
		if (target == null)
		{
			return null;
		}
		mCurLinker = linker;
		mCurLinker.setLinkObject(target);
		mCurLinker.setActive(true);
		return mCurLinker;
	}
	public Camera getCamera(){ return mCamera;}
	public void setProcessKey(bool process) { mProcessKey = process; }
	public bool isProcessKey() { return mProcessKey; }
	public CameraLinker getCurLinker() { return mCurLinker; }
	public float getNearClip() { return mCamera.nearClipPlane; }
	public float getFOVX(bool radian = false)
	{
		float radianFovX = atan(getAspect() * tan(getFOVY(true) * 0.5f)) * 2.0f;
		if (!radian)
		{
			return toDegree(radianFovX);
		}
		return radianFovX;
	}
	public void setFOVY(float fovy, bool radian = false)
	{
		if(radian)
		{
			fovy = toDegree(fovy);
		}
		mCamera.fieldOfView = fovy;
	}
	public float getFOVY(bool radian = false)
	{
		if(radian)
		{
			return toRadian(mCamera.fieldOfView);
		}
		return mCamera.fieldOfView;
	}
	public float getAspect() { return mCamera.aspect; }
	public float getOrthoSize() { return mCamera.orthographicSize; }
	public void setOrthoSize(float size) { mCamera.orthographicSize = size; }
	public float getCameraDepth() { return mCamera.depth; }
	public void copyCamera(GameObject obj)
	{
		copyObjectTransform(obj);
		Camera camera = obj.GetComponent<Camera>();
		mCamera.fieldOfView = camera.fieldOfView;
		mCamera.cullingMask = camera.cullingMask;
	}
	public void setVisibleLayer(int layer)
	{
		if(layer == 0)
		{
			return;
		}
		mLastVisibleLayer = mCamera.cullingMask;
		mCamera.cullingMask = layer;
	}
	public int getLastVisibleLayer() { return mLastVisibleLayer; }
	public void setRenderTarget(RenderTexture renderTarget) { mCamera.targetTexture = renderTarget; }
	// 因为允许外部扩展连接器,所以此函数为public
	public void addLinker(Type type)
	{
		mLinkerList.Add(addComponent(type) as CameraLinker);
	}
	//-----------------------------------------------------------------------------------------------------------------------------------------------
	protected override void initComponents()
	{
		base.initComponents();
		addLinker(typeof(CameraLinkerAcceleration));
		addLinker(typeof(CameraLinkerFixed));
		addLinker(typeof(CameraLinkerSmoothFollow));
		addLinker(typeof(CameraLinkerSmoothRotate));
	}
}