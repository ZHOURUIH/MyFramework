using System;
using UnityEngine;

public class CameraLinkerFree : CameraLinker
{
	protected float mCameraMoveSpeed;           // 使用按键控制时的移动速度
	protected float mMouseSpeed;                // 鼠标转动摄像机的速度
	public CameraLinkerFree()
	{
		mCameraMoveSpeed = 30.0f;
		mMouseSpeed = 0.1f;
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mCameraMoveSpeed = 30.0f;
		mMouseSpeed = 0.1f;
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		float cameraSpeed = mCameraMoveSpeed;
		if (!isFloatZero(cameraSpeed))
		{
			// 键盘移动摄像机
			if (Input.GetKey(KeyCode.LeftShift))
			{
				cameraSpeed *= 2.0f;
			}
			float moveLength = cameraSpeed * elapsedTime;
			Vector3 dir = Vector3.zero;
			// 向前移动摄像机
			if (Input.GetKey(KeyCode.W))
			{
				dir += moveLength * Vector3.forward;
			}
			// 向左移动摄像机
			if (Input.GetKey(KeyCode.A))
			{
				dir += moveLength * Vector3.left;
			}
			// 向后移动摄像机
			if (Input.GetKey(KeyCode.S))
			{
				dir += moveLength * Vector3.back;
			}
			// 向右移动摄像机
			if (Input.GetKey(KeyCode.D))
			{
				dir += moveLength * Vector3.right;
			}
			// WASD是在本地空间的移动,需要转换为世界空间
			if(!isVectorZero(dir))
			{
				dir = rotateVector3(dir, mCamera.getQuaternionRotation());
			}
			
			// 世界空间下摄像机的移动
			// 竖直向上移动摄像机
			if (Input.GetKey(KeyCode.Q))
			{
				dir += moveLength * Vector3.up;
			}
			// 竖直向下移动摄像机
			if (Input.GetKey(KeyCode.E))
			{
				dir += moveLength * Vector3.down;
			}
			if (!isVectorZero(dir))
			{
				mCamera.move(dir, Space.World);
			}
		}
		// 鼠标旋转摄像机
		if (mInputSystem.isMouseKeepDown(MOUSE_BUTTON.RIGHT) || mInputSystem.isMouseCurrentDown(MOUSE_BUTTON.RIGHT))
		{
			Vector2 moveDelta = mInputSystem.getMouseDelta();
			if (!isFloatZero(moveDelta.x) || !isFloatZero(moveDelta.y))
			{
				mCamera.yawPitch(moveDelta.x * mMouseSpeed, -moveDelta.y * mMouseSpeed);
			}
		}
		// 鼠标滚轮移动摄像机
		float mouseWheelDelta = mInputSystem.getMouseWheelDelta();
		if (!isFloatZero(mouseWheelDelta))
		{
			mCamera.move(mouseWheelDelta * (10.0f / 120.0f) * Vector3.forward);
		}
	}
}