using UnityEngine;
using static MathUtility;

// 插值位置的组件
public abstract class ComponentLerpPosition : ComponentLerp, IComponentModifyPosition
{
	protected Vector3 mTargetPosition;	// 目标位置
	protected float mMinRange;			// 最近距离,当差值小于此距离时将直接设置到目标点
	public ComponentLerpPosition()
	{
		mMinRange = 0.001f;
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mTargetPosition = Vector3.zero;
		mMinRange = 0.001f;
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		Vector3 curPos = lerp(getPosition(), mTargetPosition, mLerpSpeed * elapsedTime, mMinRange);
		applyPosition(curPos);
		afterApplyLerp(isVectorEqual(curPos, mTargetPosition));
	}
	public override void play()
	{
		if (isVectorEqual(getPosition(), mTargetPosition))
		{
			stop();
			return;
		}
		base.play();
	}
	public void setTargetPosition(Vector3 target) { mTargetPosition = target; }
	public Vector3 getTargetPosition() { return mTargetPosition; }
	public void setMinRange(float minRange) { mMinRange = minRange; }
	public float getMinRange() { return mMinRange; }
	//------------------------------------------------------------------------------------------------------------------------------
	protected abstract void applyPosition(Vector3 position);
	protected abstract Vector3 getPosition();
}