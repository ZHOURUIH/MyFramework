using UnityEngine;
using static MathUtility;

// 物体的移动信息
public class COMMovableObjectMoveInfo : GameComponent
{
	protected Vector3 mLastPhysicsSpeedVector;      // FixedUpdate中上一帧的移动速度
	protected Vector3 mLastPhysicsPosition;         // 上一帧FixedUpdate中的位置
	protected Vector3 mPhysicsAcceleration;         // FixedUpdate中的加速度
	protected Vector3 mPhysicsSpeedVector;          // FixedUpdate中的移动速度
	protected Vector3 mCurFramePosition;            // 当前位置
	protected Vector3 mMoveSpeedVector;             // 当前移动速度向量,根据上一帧的位置和当前位置以及时间计算出来的实时速度
	protected Vector3 mLastSpeedVector;             // 上一帧的移动速度向量
	protected Vector3 mLastPosition;                // 上一帧的位置
	protected float mRealtimeMoveSpeed;             // 当前实时移动速率
	protected bool mEnableFixedUpdate;              // 是否启用FixedUpdate来计算Physics相关属性
	protected bool mMovedDuringFrame;               // 角色在这一帧内是否移动过
	protected bool mHasLastPosition;                // mLastPosition是否有效
	public COMMovableObjectMoveInfo()
	{
		mEnableFixedUpdate = true;
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mLastPhysicsSpeedVector = Vector3.zero;
		mLastPhysicsPosition = Vector3.zero;
		mPhysicsAcceleration = Vector3.zero;
		mPhysicsSpeedVector = Vector3.zero;
		mCurFramePosition = Vector3.zero;
		mMoveSpeedVector = Vector3.zero;
		mLastSpeedVector = Vector3.zero;
		mLastPosition = Vector3.zero;
		mRealtimeMoveSpeed = 0.0f;
		mEnableFixedUpdate = true;
		mMovedDuringFrame = false;
		mHasLastPosition = false;
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		var movableObject = mComponentOwner as MovableObject;
		if (movableObject.isDestroy())
		{
			return;
		}
		if (elapsedTime > 0.0f)
		{
			mCurFramePosition = movableObject.getPosition();
			mMoveSpeedVector = mHasLastPosition ? divide(mCurFramePosition - mLastPosition, elapsedTime) : Vector3.zero;
			mRealtimeMoveSpeed = getLength(mMoveSpeedVector);
			mMovedDuringFrame = !isVectorEqual(mLastPosition, mCurFramePosition) && mHasLastPosition;
			mLastPosition = mCurFramePosition;
			mLastSpeedVector = mMoveSpeedVector;
			mHasLastPosition = true;
		}
	}
	public override void fixedUpdate(float elapsedTime)
	{
		if (!mEnableFixedUpdate)
		{
			return;
		}
		base.fixedUpdate(elapsedTime);
		var movableObject = mComponentOwner as MovableObject;
		Vector3 curPos = movableObject.getPosition();
		mPhysicsSpeedVector = divide(curPos - mLastPhysicsPosition, elapsedTime);
		mLastPhysicsPosition = curPos;
		mPhysicsAcceleration = divide(mPhysicsSpeedVector - mLastPhysicsSpeedVector, elapsedTime);
		mLastPhysicsSpeedVector = mPhysicsSpeedVector;
	}
	public Vector3 getPhysicsSpeed()									{ return mPhysicsSpeedVector; }
	public Vector3 getPhysicsAcceleration()								{ return mPhysicsAcceleration; }
	public bool hasMovedDuringFrame()									{ return mMovedDuringFrame; }
	public bool isEnableFixedUpdate()									{ return mEnableFixedUpdate; }
	public Vector3 getMoveSpeedVector()									{ return mMoveSpeedVector; }
	public Vector3 getLastSpeedVector()									{ return mLastSpeedVector; }
	public Vector3 getLastPosition()									{ return mLastPosition; }
	public bool hasLastPosition()										{ return mHasLastPosition; }
}