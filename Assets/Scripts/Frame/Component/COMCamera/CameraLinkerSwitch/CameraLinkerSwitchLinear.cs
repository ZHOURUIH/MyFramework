using UnityEngine;

public class CameraLinkerSwitchLinear : CameraLinkerSwitch
{
	protected Vector3 mDirection;		// 此次转换的方向,用于避免不必要的向量重复计算
	protected float mMovedDistance;		// 当前已经移动的距离
	protected float mDistance;			// 此次转换的总长度,用于避免不必要的向量长度重复计算
	public CameraLinkerSwitchLinear()
	{
		mSpeed = 7.0f;
	}
	public override void init(Vector3 origin, Vector3 target, float speed)
	{
		base.init(origin, target, speed);
		mMovedDistance = 0.0f;
		mDistance = getLength(mOriginRelative - mTargetRelative);
		mDirection = normalize(mTargetRelative - mOriginRelative);
	}
	public override void update(float elapsedTime)
	{
		if (mLinker == null)
		{
			return;
		}
		mMovedDistance += mSpeed * elapsedTime;
		if (mMovedDistance < mDistance)
		{
			mLinker.setRelativePosition(mOriginRelative + mDirection * mMovedDistance);
		}
		else
		{
			// 如果已经超过了转换距离,则设置校正位置到指定点,并且通知连接器转换完成
			mMovedDistance = mDistance;
			mLinker.setRelativePosition(mTargetRelative);
			mLinker.notifyFinishSwitching();
		}
	}
}