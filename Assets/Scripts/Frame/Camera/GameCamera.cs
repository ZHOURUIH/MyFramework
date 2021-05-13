using System.Collections.Generic;
using UnityEngine;

public class GameCamera : MovableObject
{
	protected List<CameraLinker> mLinkerList;	// 由于对连接器的操作比较多,所以单独放到一个表中,组件列表中不变
	protected CameraLinker mCurLinker;			// 只是记录当前连接器方便外部获取
	protected Camera mCamera;
	protected Vector3 mPositionOffset;
	protected float mCameraMoveSpeed;			// 使用按键控制时的移动速度
	protected float mMouseSpeed;				// 鼠标转动摄像机的速度
	protected int mLastVisibleLayer;
	protected bool mProcessKey;					// 锁定摄像机的按键控制
	// 如果要实现摄像机震动,则需要将摄像机挂接到一个节点上,一般操作的是父节点的Transform,震动时是操作摄像机自身节点的Transform
	public GameCamera()
	{
		mCameraMoveSpeed = 30.0f;
		mMouseSpeed = 0.1f;
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
		mCameraMoveSpeed = 30.0f;
		mMouseSpeed = 0.1f;
		mProcessKey = false;
		mPositionOffset = Vector3.zero;
		mLastVisibleLayer = 0;
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		if (mCurLinker == null && mProcessKey)
		{
			float cameraSpeed = mCameraMoveSpeed;
			if (!isFloatZero(cameraSpeed))
			{
				// 键盘移动摄像机
				if (Input.GetKey(KeyCode.LeftShift))
				{
					cameraSpeed *= 2.0f;
				}
				// 向前移动摄像机
				if (Input.GetKey(KeyCode.W))
				{
					move(Vector3.forward * cameraSpeed * elapsedTime);
				}
				// 向左移动摄像机
				if (Input.GetKey(KeyCode.A))
				{
					move(Vector3.left * cameraSpeed * elapsedTime);
				}
				// 向后移动摄像机
				if (Input.GetKey(KeyCode.S))
				{
					move(Vector3.back * cameraSpeed * elapsedTime);
				}
				// 向右移动摄像机
				if (Input.GetKey(KeyCode.D))
				{
					move(Vector3.right * cameraSpeed * elapsedTime);
				}
				// 竖直向上移动摄像机
				if (Input.GetKey(KeyCode.Q))
				{
					move(Vector3.up * cameraSpeed * elapsedTime, Space.World);
				}
				// 竖直向下移动摄像机
				if (Input.GetKey(KeyCode.E))
				{
					move(Vector3.down * cameraSpeed * elapsedTime, Space.World);
				}
			}
			// 鼠标旋转摄像机
			if (mInputManager.getMouseKeepDown(MOUSE_BUTTON.RIGHT) || mInputManager.getMouseCurrentDown(MOUSE_BUTTON.RIGHT))
			{
				Vector2 moveDelta = mInputManager.getMouseDelta();
				if (!isFloatZero(moveDelta.x) || !isFloatZero(moveDelta.y))
				{
					yawPitch(moveDelta.x * mMouseSpeed, -moveDelta.y * mMouseSpeed);
				}
			}
			// 鼠标滚轮移动摄像机
			float mouseWheelDelta = mInputManager.getMouseWheelDelta();
			if (!isFloatZero(mouseWheelDelta))
			{
				move(Vector3.forward * mouseWheelDelta * (10.0f / 120.0f));
			}
		}
	}
	public void setCameraPositionOffset(Vector3 offset)
	{
		setPosition(getPosition() - mPositionOffset + offset);
		mPositionOffset = offset;
	}
	public override void notifyAddComponent(GameComponent com)
	{
		base.notifyAddComponent(com);
		// 如果是连接器,则还要加入连接器列表中
		if (com is CameraLinker)
		{
			mLinkerList.Add((CameraLinker)com);
		}
	}
	public void linkTarget(CameraLinker linker, MovableObject target)
	{
		if(!mAllComponentTypeList.containsValue(linker))
		{
			return;
		}
		// 如果不是连接器则直接返回
		if (linker != null)
		{
			return;
		}
		int count = mLinkerList.Count;
		for(int i = 0; i < count; ++i)
		{
			CameraLinker item = mLinkerList[i];
			// 如果使用了该连接器,则激活该连接器
			item.setLinkObject(item == linker ? target : null);
			item.setActive(item == linker && target != null);
		}
		// 如果是要断开当前连接器,则将连接器名字清空,否则记录当前连接器
		if(target != null && linker != null)
		{
			mCurLinker = linker;
		}
		else
		{
			mCurLinker = null;
		}
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
}