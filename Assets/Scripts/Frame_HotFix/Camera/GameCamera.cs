using System;
using UnityEngine;
#if USE_URP
using UnityEngine.Rendering.Universal;
#endif
using static MathUtility;
using static UnityUtility;
using static FrameEditorUtility;

// 封装Unity的Camera
public class GameCamera : MovableObject
{
	protected CameraLinker mCurLinker;		// 只是记录当前连接器方便外部获取
	protected Camera mCamera;               // 摄像机组件
	protected int mOverlayDepth;			// 作为Overlay摄像机时的排序深度
	protected int mLastVisibleLayer;		// 上一次的渲染层
	// 如果要实现摄像机震动,则需要将摄像机挂接到一个节点上,一般操作的是父节点的Transform,震动时是操作摄像机自身节点的Transform
	public override void setObject(GameObject obj)
	{
		base.setObject(obj);
		mObject.TryGetComponent(out mCamera);
		if (isEditor())
		{
			getOrAddUnityComponent<CameraDebug>().setCamera(this);
		}
	}
	public override void destroy()
	{
		base.destroy();
		tryGetUnityComponent<CameraDebug>(out var debug);
		destroyUnityObject(ref debug);
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mCamera = null;
		mCurLinker = null;
		mOverlayDepth = 0;
		mLastVisibleLayer = 0;
	}
	public void unlinkTarget()
	{
		mCurLinker?.onUnlink();
		mCurLinker?.setLinkObject(null);
		mCurLinker?.setActive(false);
		mCurLinker = null;
	}
	public CameraLinker linkTarget(Type linkerType, MovableObject target)
	{
		if (getOrAddComponent(linkerType) is not CameraLinker linker)
		{
			return null;
		}
		// 先断开旧的连接器
		unlinkTarget();
		mCurLinker = linker;
		mCurLinker.setLinkObject(target);
		mCurLinker.setActive(true);
		mCurLinker.onLinked();
		return mCurLinker;
	}
	public Camera getCamera() { return mCamera; }
	public CameraLinker getCurLinker() { return mCurLinker; }
	public float getNearClip() { return mCamera.nearClipPlane; }
	public float getFOVX(bool radian = false)
	{
		float radianFovX = atan(getAspect() * tan(getFOVY(true) * 0.5f)) * 2.0f;
		return radian ? radianFovX : toDegree(radianFovX);
	}
	// 计算透视投影下显示的宽高,与屏幕大小无关,只是指定距离下视锥体的截面宽高
	public Vector2 getViewSize(float distance)
	{
		float viewHeight = tan(getFOVY(true) * 0.5f) * abs(distance) * 2.0f;
		return new(viewHeight * getAspect(), viewHeight);
	}
	// radian为true表示输入的fovy是弧度制的值,false表示角度制的值
	public void setFOVY(float fovy, bool radian = false)
	{
		mCamera.fieldOfView = radian ? toDegree(fovy) : fovy;
	}
	public float getFOVY(bool radian = false)
	{
		return radian ? toRadian(mCamera.fieldOfView) : mCamera.fieldOfView;
	}
	public float getAspect() { return mCamera.aspect; }
	public float getOrthoSize() { return mCamera.orthographicSize; }
	public void setOrthoSize(float size) { mCamera.orthographicSize = size; }
	public float getCameraDepth() { return mCamera.depth; }
	public void setCameraDepth(float depth) { mCamera.depth = depth; }
	public int getOverlayDepth() { return mOverlayDepth;}
	public void setOverlayDepth(int depth) { mOverlayDepth = depth; }
	public void copyCamera(GameObject obj)
	{
		copyObjectTransform(obj);
		obj.TryGetComponent(out Camera camera);
		mCamera.fieldOfView = camera.fieldOfView;
		mCamera.cullingMask = camera.cullingMask;
	}
	public void setVisibleLayer(int layer)
	{
		if (layer == 0)
		{
			return;
		}
		mLastVisibleLayer = mCamera.cullingMask;
		mCamera.cullingMask = layer;
	}
	public int getLastVisibleLayer() { return mLastVisibleLayer; }
	public void setPostProcessing(bool post)
	{
#if USE_URP
		getOrAddUnityComponent<UniversalAdditionalCameraData>().renderPostProcessing = post;
#endif
	}
	public void setRenderTarget(RenderTexture renderTarget)
	{
#if USE_URP
		if (getOrAddUnityComponent<UniversalAdditionalCameraData>().cameraStack.Count > 0)
		{
			logError("设置RenderTexture的摄像机不能再添加cameraStack,请移除此摄像机上所有的cameraStack");
		}
#endif
		mCamera.targetTexture = renderTarget;
	}
	public RenderTexture createRenderTarget(Vector2 size)
	{
		if (mCamera.targetTexture != null)
		{
			return mCamera.targetTexture;
		}
		RenderTexture rt = RenderTexture.GetTemporary((int)size.x, (int)size.y, 4);
		setRenderTarget(rt);
		return rt;
	}
	public void destroyRenderTexture()
	{
		if (mCamera.targetTexture == null)
		{
			return;
		}
		RenderTexture.ReleaseTemporary(mCamera.targetTexture);
		mCamera.targetTexture = null;
	}
	public RenderTexture getRenderTarget() { return mCamera.targetTexture; }
	//------------------------------------------------------------------------------------------------------------------------------
	protected override void initComponents()
	{
		base.initComponents();
		addInitComponent<CameraLinkerAcceleration>(false);
		addInitComponent<CameraLinkerThirdPerson>(false);
		addInitComponent<CameraLinkerFree>(false);
		addInitComponent<CameraLinkerSmoothFollow>(false);
		addInitComponent<CameraLinkerSmoothRotate>(false);
		addInitComponent<CameraLinkerFirstPerson>(false);
	}
}