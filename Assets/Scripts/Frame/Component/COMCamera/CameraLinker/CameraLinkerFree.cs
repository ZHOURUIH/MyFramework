using System;
using UnityEngine;

public class CameraLinkerFree : CameraLinker
{
	protected Vector2 mRotateAngle;         // 这一帧旋转的角度,角度制
	protected float mMouseWheelSpeed;       // 鼠标滚轮移动摄像机的速度
	protected float mRotateSpeed;           // 鼠标转动摄像机的速度
	protected float mMoveSpeed;             // 使用按键控制时的移动速度
	protected Vector3 mMoveForward;         // 这一帧需要向前移动的量
	protected Vector3 mMoveBack;            // 这一帧需要向后移动的量
	protected Vector3 mMoveLeft;            // 这一帧需要向左移动的量
	protected Vector3 mMoveRight;           // 这一帧需要向右移动的量
	protected Vector3 mMoveUp;              // 这一帧需要向上移动的量
	protected Vector3 mMoveDown;            // 这一帧需要向下移动的量
	protected bool mEnableKeyboard;         // 是否自动检测键盘控制摄像机移动
	public CameraLinkerFree()
	{
		mMouseWheelSpeed = 10.0f / 120.0f;
		mRotateSpeed = 0.1f;
		mMoveSpeed = 10.0f;
		mUpdateMoment = LINKER_UPDATE.UPDATE;
		mEnableKeyboard = true;
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mMouseWheelSpeed = 10.0f / 120.0f;
		mRotateSpeed = 0.1f;
		mMoveSpeed = 10.0f;
		mUpdateMoment = LINKER_UPDATE.UPDATE;
		mEnableKeyboard = true;
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		// 键盘移动摄像机
		if (mEnableKeyboard)
		{
			float moveLength = mMoveSpeed * elapsedTime;
			if (isKeyDown(KeyCode.LeftShift) || isKeyDown(KeyCode.RightShift))
			{
				moveLength *= 2.0f;
			}
			mMoveForward = Vector3.zero;
			mMoveLeft = Vector3.zero;
			mMoveBack = Vector3.zero;
			mMoveRight = Vector3.zero;
			mMoveUp = Vector3.zero;
			mMoveDown = Vector3.zero;
			// 向前移动摄像机
			if (isKeyDown(KeyCode.W))
			{
				mMoveForward = rotateVector3(moveLength * Vector3.forward, mCamera.getQuaternionRotation());
			}
			// 向左移动摄像机
			if (isKeyDown(KeyCode.A))
			{
				mMoveLeft = rotateVector3(moveLength * Vector3.left, mCamera.getQuaternionRotation());
			}
			// 向后移动摄像机
			if (isKeyDown(KeyCode.S))
			{
				mMoveBack = rotateVector3(moveLength * Vector3.back, mCamera.getQuaternionRotation());
			}
			// 向右移动摄像机
			if (isKeyDown(KeyCode.D))
			{
				mMoveRight = rotateVector3(moveLength * Vector3.right, mCamera.getQuaternionRotation());
			}
			// 竖直向上移动摄像机
			if (isKeyDown(KeyCode.Q))
			{
				mMoveUp = moveLength * Vector3.up;
			}
			// 竖直向下移动摄像机
			if (isKeyDown(KeyCode.E))
			{
				mMoveDown = moveLength * Vector3.down;
			}
		}
		Vector3 dir = mMoveForward + mMoveLeft + mMoveBack + mMoveRight + mMoveUp + mMoveDown;
		if (!isVectorZero(dir))
		{
			mCamera.move(dir, Space.World);
		}

		// 鼠标旋转摄像机
		if (mEnableKeyboard)
		{
			if (mInputSystem.isMouseDown(MOUSE_BUTTON.RIGHT))
			{
				mRotateAngle = mInputSystem.getMouseDelta() * mRotateSpeed;
			}
			else
			{
				mRotateAngle = Vector2.zero;
			}
		}
		if (!isVectorZero(mRotateAngle))
		{
			mCamera.yawPitch(mRotateAngle.x, -mRotateAngle.y);
		}

		// 鼠标滚轮移动摄像机
		if (mEnableKeyboard)
		{
			float mouseWheelDelta = mInputSystem.getMouseWheelDelta();
			if (!isFloatZero(mouseWheelDelta))
			{
				// 键盘移动摄像机
				if (isKeyDown(KeyCode.LeftShift) || isKeyDown(KeyCode.RightShift))
				{
					mouseWheelDelta *= 3.0f;
				}
				mCamera.move(mouseWheelDelta * mMouseWheelSpeed * Vector3.forward);
			}
		}
	}
	public float getMouseWheelSpeed() { return mMouseWheelSpeed; }
	public float getRotateSpeed() { return mRotateSpeed; }
	public float getMoveSpeed() { return mMoveSpeed; }
	public Vector3 getMoveForward() { return mMoveForward; }
	public Vector3 getMoveBack() { return mMoveBack; }
	public Vector3 getMoveLeft() { return mMoveLeft; }
	public Vector3 getMoveRight() { return mMoveRight; }
	public Vector3 getMoveUp() { return mMoveUp; }
	public Vector3 getMoveDown() { return mMoveDown; }
	public Vector2 getRotateAngle() { return mRotateAngle; }
	public bool isEnableKeyboard() { return mEnableKeyboard; }
	public void setMouseWheelSpeed(float value) { mMouseWheelSpeed = value; }
	public void setRotateSpeed(float value) { mRotateSpeed = value; }
	public void setMoveSpeed(float value) { mMoveSpeed = value; }
	public void setMoveForward(Vector3 value) { mMoveForward = value; }
	public void setMoveBack(Vector3 value) { mMoveBack = value; }
	public void setMoveLeft(Vector3 value) { mMoveLeft = value; }
	public void setMoveRight(Vector3 value) { mMoveRight = value; }
	public void setMoveUp(Vector3 value) { mMoveUp = value; }
	public void setMoveDown(Vector3 value) { mMoveDown = value; }
	public void setRotateAngle(Vector2 value) { mRotateAngle = value; }
	public void setEnableKeyboard(bool value) { mEnableKeyboard = value; }
}