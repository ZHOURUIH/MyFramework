using UnityEngine;
using static MathUtility;

// 封装Unity的Camera
public class GameCamera : MovableObject
{
	protected Camera mCamera;				// 摄像机组件
	protected int mLastVisibleLayer;		// 上一次的渲染层
	public override void setObject(GameObject obj)
	{
		base.setObject(obj);
		mObject.TryGetComponent(out mCamera);
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mCamera = null;
		mLastVisibleLayer = 0;
	}
	public Camera getCamera() { return mCamera; }
	public float getFOVY(bool radian = false)
	{
		return radian ? toRadian(mCamera.fieldOfView) : mCamera.fieldOfView;
	}
	public float getCameraDepth() { return mCamera.depth; }
}