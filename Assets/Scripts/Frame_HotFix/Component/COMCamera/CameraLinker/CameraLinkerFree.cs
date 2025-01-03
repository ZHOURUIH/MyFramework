using UnityEngine;
using static MathUtility;
using static FrameUtility;
using static FrameBase;
using static FrameEditorUtility;

// 完全自由的摄像机,不连接任何物体
public class CameraLinkerFree : CameraLinker
{
	protected Vector2 mRotateAngle;			// 这一帧旋转的角度,角度制
	protected Vector3 mMoveDelta;           // 这一帧需要移动的量
	protected float mMouseWheelSpeed;		// 鼠标滚轮移动摄像机的速度
	protected float mRotateSpeed;			// 鼠标转动摄像机的速度
	protected float mMoveSpeed;             // 使用按键控制时的移动速度
	protected bool mEnableKeyboard;			// 是否自动检测键盘控制摄像机移动
	public CameraLinkerFree()
	{
		mMouseWheelSpeed = 10.0f / 120.0f;
		mRotateSpeed = 0.1f;
		mMoveSpeed = 10.0f;
		mUpdateMoment = LINKER_UPDATE.UPDATE;
		mEnableKeyboard = true;
		mLookAtTarget = false;
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mRotateAngle = Vector3.zero;
		mMoveDelta = Vector3.zero;
		mMouseWheelSpeed = 10.0f / 120.0f;
		mRotateSpeed = 0.1f;
		mMoveSpeed = 10.0f;
		mUpdateMoment = LINKER_UPDATE.UPDATE;
		mEnableKeyboard = true;
		mLookAtTarget = false;
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
			mMoveDelta = Vector3.zero;
			// 向前移动摄像机
			if (isKeyDown(KeyCode.W))
			{
				mMoveDelta += rotateVector3(moveLength * Vector3.forward, mCamera.getRotationQuaternion());
			}
			// 向左移动摄像机
			if (isKeyDown(KeyCode.A))
			{
				mMoveDelta += rotateVector3(moveLength * Vector3.left, mCamera.getRotationQuaternion());
			}
			// 向后移动摄像机
			if (isKeyDown(KeyCode.S))
			{
				mMoveDelta += rotateVector3(moveLength * Vector3.back, mCamera.getRotationQuaternion());
			}
			// 向右移动摄像机
			if (isKeyDown(KeyCode.D))
			{
				mMoveDelta += rotateVector3(moveLength * Vector3.right, mCamera.getRotationQuaternion());
			}
			// 竖直向上移动摄像机
			if (isKeyDown(KeyCode.Q))
			{
				mMoveDelta += moveLength * Vector3.up;
			}
			// 竖直向下移动摄像机
			if (isKeyDown(KeyCode.E))
			{
				mMoveDelta += moveLength * Vector3.down;
			}
		}
		if (!isVectorZero(mMoveDelta))
		{
			mCamera.move(mMoveDelta, Space.World);
		}

		// 鼠标旋转摄像机
		if (isEditor() || isWindows())
		{
			if (mEnableKeyboard)
			{
				if (mInputSystem.isMouseRightDown())
				{
					mRotateAngle = mInputSystem.getTouchPoint((int)MOUSE_BUTTON.RIGHT).getMoveDelta() * mRotateSpeed;
				}
				else
				{
					mRotateAngle = Vector2.zero;
				}
			}
		}
		if (!isVectorZero(mRotateAngle))
		{
			mCamera.yawPitch(mRotateAngle.x, -mRotateAngle.y);
		}

		// 鼠标滚轮移动摄像机
		if (mEnableKeyboard)
		{
			if (isEditor() || isWindows())
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
	}
	// 当使用此连接器时调用
	public override void onLinked()				
	{
		mMoveDelta = Vector3.zero;
		mRotateAngle = Vector3.zero;
	}
	public float getMouseWheelSpeed()			{ return mMouseWheelSpeed; }
	public float getRotateSpeed()				{ return mRotateSpeed; }
	public float getMoveSpeed()					{ return mMoveSpeed; }
	public Vector3 getMoveDelta()				{ return mMoveDelta; }
	public Vector2 getRotateAngle()				{ return mRotateAngle; }
	public bool isEnableKeyboard()				{ return mEnableKeyboard; }
	public void setMouseWheelSpeed(float value) { mMouseWheelSpeed = value; }
	public void setRotateSpeed(float value)		{ mRotateSpeed = value; }
	public void setMoveSpeed(float value)		{ mMoveSpeed = value; }
	public void setMoveDelta(Vector3 value)		{ mMoveDelta = value; }
	public void setRotateAngle(Vector2 value)	{ mRotateAngle = value; }
	public void setEnableKeyboard(bool value)	{ mEnableKeyboard = value; }
	// 自由摄像机不会看向目标,因为没有目标
	public override void setLookAtTarget(bool lookat) { mLookAtTarget = false; }
}