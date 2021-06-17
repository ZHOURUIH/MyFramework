using System;
using UnityEngine;

public class CameraLinkerFree : CameraLinker
{
	protected float mMouseWheelSpeed;		// 鼠标滚轮移动摄像机的速度
	protected float mRotateSpeed;			// 鼠标转动摄像机的速度
	protected float mMoveSpeed;             // 使用按键控制时的移动速度
	protected float mMoveForward;			// 这一帧需要向前移动的距离
	protected float mMoveBack;              // 这一帧需要向后移动的距离
	protected float mMoveLeft;              // 这一帧需要向左移动的距离
	protected float mMoveRight;             // 这一帧需要向右移动的距离
	protected float mMoveUp;                // 这一帧需要向上移动的距离
	protected float mMoveDown;              // 这一帧需要向下移动的距离
	protected Vector2 mRotateAngle;			// 这一帧旋转的角度,角度制
	protected bool mEnableKeyboard;			// 是否自动检测键盘控制摄像机移动
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
			mMoveForward = 0.0f;
			mMoveLeft = 0.0f;
			mMoveBack = 0.0f;
			mMoveRight = 0.0f;
			mMoveUp = 0.0f;
			mMoveDown = 0.0f;
			// 向前移动摄像机
			if (isKeyDown(KeyCode.W))
			{
				mMoveForward = moveLength;
			}
			// 向左移动摄像机
			if (isKeyDown(KeyCode.A))
			{
				mMoveLeft = moveLength;
			}
			// 向后移动摄像机
			if (isKeyDown(KeyCode.S))
			{
				mMoveBack = moveLength;
			}
			// 向右移动摄像机
			if (isKeyDown(KeyCode.D))
			{
				mMoveRight = moveLength;
			}
			// 世界空间下摄像机的移动
			// 竖直向上移动摄像机
			if (isKeyDown(KeyCode.Q))
			{
				mMoveUp = moveLength;
			}
			// 竖直向下移动摄像机
			if (isKeyDown(KeyCode.E))
			{
				mMoveDown = moveLength;
			}
		}
		Vector3 dir = Vector3.zero;
		dir += mMoveForward * Vector3.forward;
		dir += mMoveLeft * Vector3.left;
		dir += mMoveBack * Vector3.back;
		dir += mMoveRight * Vector3.right;
		// 前后左右的移动是在本地空间,需要转换为世界空间
		if (!isVectorZero(dir))
		{
			dir = rotateVector3(dir, mCamera.getQuaternionRotation());
		}
		dir += mMoveUp * Vector3.up;
		dir += mMoveDown * Vector3.down;
		if (!isVectorZero(dir))
		{
			mCamera.move(dir, Space.World);
		}

		// 鼠标旋转摄像机
		if(mEnableKeyboard)
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
	public float getMouseWheelSpeed()			{ return mMouseWheelSpeed; }
	public float getRotateSpeed()				{ return mRotateSpeed; }
	public float getMoveSpeed()					{ return mMoveSpeed; }
	public float getMoveForward()				{ return mMoveForward; }
	public float getMoveBack()					{ return mMoveBack; }
	public float getMoveLeft()					{ return mMoveLeft; }
	public float getMoveRight()					{ return mMoveRight; }
	public float getMoveUp()					{ return mMoveUp; }
	public float getMoveDown()					{ return mMoveDown; }
	public Vector2 getRotateAngle()				{ return mRotateAngle; }
	public bool isEnableKeyboard()				{ return mEnableKeyboard; }
	public void setMouseWheelSpeed(float value) { mMouseWheelSpeed = value; }
	public void setRotateSpeed(float value)		{ mRotateSpeed = value; }
	public void setMoveSpeed(float value)		{ mMoveSpeed = value; }
	public void setMoveForward(float value)		{ mMoveForward = value; }
	public void setMoveBack(float value)		{ mMoveBack = value; }
	public void setMoveLeft(float value)		{ mMoveLeft = value; }
	public void setMoveRight(float value)		{ mMoveRight = value; }
	public void setMoveUp(float value)			{ mMoveUp = value; }
	public void setMoveDown(float value)		{ mMoveDown = value; }
	public void setRotateAngle(Vector2 value)	{ mRotateAngle = value; }
	public void setEnableKeyboard(bool value)	{ mEnableKeyboard = value; }
}