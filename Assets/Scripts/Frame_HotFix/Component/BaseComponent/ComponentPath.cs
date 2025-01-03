using UnityEngine;
using System.Collections.Generic;
using static UnityUtility;
using static MathUtility;

// 指定路径关键帧和时间关键帧进行变换
public abstract class ComponentPath : ComponentKeyFrame
{
	protected Dictionary<float, Vector3> mValueKeyFrame;    // 值与时间的关键帧列表
	protected List<float> mTimeList = new();                // 计算出的时间列表
	protected Vector3 mValueOffset;                         // 值的偏移
	protected float mMaxLength;                             // mTimeList中的最大值
	protected float mSpeed = 1.0f;                          // 变化速度
	protected bool mOffsetBlendAdd = true;                  // true表示直接相加,false表示相乘
	public override void resetProperty()
	{
		base.resetProperty();
		mValueKeyFrame = null;
		mTimeList.Clear();
		mValueOffset = Vector3.zero;
		mMaxLength = 0.0f;
		mSpeed = 1.0f;
		mOffsetBlendAdd = true;
	}
	public void setValueKeyFrame(Dictionary<float, Vector3> path) { mValueKeyFrame = path; }
	public void setSpeed(float speed) { mSpeed = speed; }
	public void setValueOffset(Vector3 offset) { mValueOffset = offset; }
	public virtual void setOffsetBlendAdd(bool blendMode) { mOffsetBlendAdd = blendMode; }
	public override void play(int keyframe, bool loop, float onceLength, float offset)
	{
		logError("use play(bool loop, float timeOffset) instead!");
	}
	public virtual void play(int keyframeID, bool loop, float timeOffset)
	{
		// 获取单次播放长度
		if (!mValueKeyFrame.isEmpty())
		{
			mTimeList.setRange(mValueKeyFrame.Keys);
			mMaxLength = mTimeList[^1];
		}
		else
		{
			mMaxLength = 0.0f;
		}
		base.play(keyframeID, loop, mMaxLength, timeOffset);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected override void applyTrembling(float value)
	{
		// 根据当前的距离找出位于哪两个点之间
		saturate(ref value);
		float curTime = value * mMaxLength;
		int index = findPointIndex(mTimeList, curTime, 0, mTimeList.Count - 1);
		float time = mTimeList[index];
		Vector3 startValue = mValueKeyFrame.get(time);
		if (index < mTimeList.Count - 1)
		{
			Vector3 endValue = mValueKeyFrame.get(mTimeList[index + 1]);
			float timePercentInSection = inverseLerp(time, mTimeList[index + 1], curTime);
			if (mOffsetBlendAdd)
			{
				setValue(lerp(startValue, endValue, timePercentInSection) + mValueOffset);
			}
			else
			{
				setValue(multiVector3(lerp(startValue, endValue, timePercentInSection), mValueOffset));
			}
		}
		else
		{
			if (mOffsetBlendAdd)
			{
				setValue(startValue + mValueOffset);
			}
			else
			{
				setValue(multiVector3(startValue, mValueOffset));
			}
		}
	}
	protected abstract void setValue(Vector3 value);
}