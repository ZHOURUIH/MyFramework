using UnityEngine;
using static MathUtility;

// 按抛物线轨迹追踪目标的组件
public class ComponentTrackTargetParabola : ComponentTrackTarget
{
	protected Vector3 mStartPosition;		// 抛物线起点
	protected float mLastMovedDistanceX;    // 上一帧累计移动的距离,仅计算抛物线X轴的距离
	protected float mMaxHeight;				// 抛物线的最高高度,相对于起点
	public override void resetProperty()
	{
		base.resetProperty();
		mStartPosition = Vector3.zero;
		mLastMovedDistanceX = 0.0f;
		mMaxHeight = 0.0f;
	}
	public void setMaxHeight(float maxHeight) { mMaxHeight = maxHeight; }
	public void setStartPosition(Vector3 pos) { mStartPosition = pos; }
	//------------------------------------------------------------------------------------------------------------------------------
	protected override void tick(float elapsedTime)
	{
		if (mTarget != null && (mTarget.getAssignID() != mTargetAssignID || mTarget.isDestroy()))
		{
			mTarget = null;
			mDoneCallback?.Invoke(this, true);
		}
		if (mTarget == null)
		{
			return;
		}

		Vector3 targetPos = getTargetPosition();
		// 根据固定的抛物线高度,抛物线上一个点,计算出一个过原点的抛物线公式
		generateParabola(mMaxHeight, mStartPosition, targetPos, out float factorA, out float factorB);
		float moveDeltaX = mSpeed * elapsedTime;
		// 这里的moveDeltaX所处的空间是临时构建的,以mStartPosition为原点的空间
		// 所以只关心当前点在这个空间中的X轴上的移动距离
		if (lengthGreaterEqual(resetY(targetPos - mStartPosition), moveDeltaX + mNearRange + mLastMovedDistanceX))
		{
			// 在X轴上累计移动距离
			mLastMovedDistanceX += moveDeltaX;
			// 根据X轴的距离和抛物线公式计算出Y坐标
			float y = factorA * mLastMovedDistanceX * mLastMovedDistanceX + factorB * mLastMovedDistanceX;
			// 根据X轴的距离,计算出目标点到起点连线上的坐标,不考虑Y轴
			Vector3 finalPosXZ = setLength(resetY(targetPos - mStartPosition), mLastMovedDistanceX) + mStartPosition;
			setPosition(replaceY(finalPosXZ, y + mStartPosition.y));
			mTrackingCallback?.Invoke(this, false);
		}
		else
		{
			setPosition(targetPos);
			mTrackingCallback?.Invoke(this, false);
			mDoneCallback?.Invoke(this, false);
		}
	}
}