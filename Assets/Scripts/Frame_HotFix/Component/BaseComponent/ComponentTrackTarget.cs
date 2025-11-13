using UnityEngine;
using static UnityUtility;
using static MathUtility;

// 追踪目标的组件
public class ComponentTrackTarget : GameComponent, IComponentModifyPosition, IComponentBreakable
{
	protected BoolCallback mTrackingCallback;	// 追踪时回调
	protected BoolCallback mDoneCallback;		// 追踪完成时回调
	protected ITransformable mTarget;			// 追踪目标
	protected Vector3 mTargetOffset;            // 追踪目标点的偏移
	protected long mTargetAssignID;				// 用于检测目标是否已经被销毁
	protected float mNearRange;                 // 距离目标还剩多少距离时认为是追踪完成
	protected float mSpeed;                     // 追踪速度
	protected bool mUpdateInFixedTick;			// 是否在FixedUpdate中执行更新
	public override void init(ComponentOwner owner)
	{
		base.init(owner);
		if (mComponentOwner is not ITransformable)
		{
			logError("ComponentTrackTarget can only add to ITransformable");
		}
	}
	public override void destroy()
	{
		mDoneCallback?.Invoke(true);
		base.destroy();
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mTrackingCallback = null;
		mDoneCallback = null;
		mTarget = null;
		mTargetOffset = Vector3.zero;
		mTargetAssignID = 0;
		mNearRange = 0.0f;
		mSpeed = 0.0f;
		mUpdateInFixedTick = false;
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		if (mUpdateInFixedTick)
		{
			return;
		}
		tick(elapsedTime);
	}
	public override void fixedUpdate(float elapsedTime)
	{
		base.fixedUpdate(elapsedTime);
		if (!mUpdateInFixedTick)
		{
			return;
		}
		tick(elapsedTime);
	}
	public void setMoveDoneTrack(ITransformable target, BoolCallback doneCallback, bool callLast = true)
	{
		BoolCallback tempCallback = mDoneCallback;
		mDoneCallback = null;
		// 如果回调函数当前不为空,则是中断了正在进行的变化
		if (callLast)
		{
			tempCallback?.Invoke(true);
		}
		mDoneCallback = doneCallback;
		mTarget = target;
		mTargetAssignID = mTarget?.getAssignID() ?? 0;
		if (mTarget == null)
		{
			setActive(false);
		}
	}
	public void setTrackingCallback(BoolCallback callback)	{ mTrackingCallback = callback; }
	public void setSpeed(float speed)						{ mSpeed = speed; }
	public void setTargetOffset(Vector3 offset)				{ mTargetOffset = offset; }
	public void setTrackNearRange(float range)				{ mNearRange = range; }
	public void setUpdateInFixedTick(bool inFixedTick)		{ mUpdateInFixedTick = inFixedTick; }
	public ITransformable getTrackTarget()					{ return mTarget; }
	public void notifyBreak()
	{
		setMoveDoneTrack(null, null);
		setTrackingCallback(null);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected virtual Vector3 getPosition()
	{
		return (mComponentOwner as ITransformable).getPosition();
	}
	protected virtual void setPosition(Vector3 pos)
	{
		(mComponentOwner as ITransformable).setPosition(pos);
	}
	protected virtual Vector3 getTargetPosition() { return mTarget.getWorldPosition() + mTarget.localToWorldDirection(mTargetOffset); }
	protected virtual void tick(float elapsedTime)
	{
		if (mTarget != null && (mTarget.getAssignID() != mTargetAssignID || mTarget.isDestroy()))
		{
			mTarget = null;
			BoolCallback tempCallback = mDoneCallback;
			mDoneCallback = null;
			// 如果回调函数当前不为空,则是中断了正在进行的变化
			tempCallback?.Invoke(true);
		}
		if (mTarget == null)
		{
			return;
		}
		Vector3 targetPos = getTargetPosition();
		Vector3 curPos = getPosition();
		float moveDelta = mSpeed * elapsedTime;
		if (lengthGreater(targetPos - curPos, moveDelta + mNearRange))
		{
			setPosition(normalize(targetPos - curPos) * moveDelta + curPos);
			mTrackingCallback?.Invoke(false);
		}
		else
		{
			setPosition(targetPos);
			mTrackingCallback?.Invoke(false);
			BoolCallback tempCallback = mDoneCallback;
			mDoneCallback = null;
			// 如果回调函数当前不为空,则是中断了正在进行的变化
			tempCallback?.Invoke(false);
		}
	}
}