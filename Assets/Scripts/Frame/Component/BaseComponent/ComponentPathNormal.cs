using UnityEngine;
using System.Collections.Generic;

// 指定路径关键帧和时间关键帧进行变换
public abstract class ComponentPathNormal : ComponentKeyFrameNormal
{
	protected Dictionary<float, Vector3> mValueKeyFrame;
	protected List<float> mTimeList;
	protected Vector3 mValueOffset;
	protected float mMaxLength;
	protected float mSpeed;
	protected bool mOffsetBlendAdd;		// true表示直接相加,false表示相乘
	public ComponentPathNormal()
	{
		mTimeList = new List<float>();
		mSpeed = 1.0f;
		mOffsetBlendAdd = true;
	}
	public void setValueKeyFrame(Dictionary<float, Vector3> path) { mValueKeyFrame = path; }
	public void setSpeed(float speed) { mSpeed = speed; }
	public void setValueOffset(Vector3 offset) { mValueOffset = offset; }
	public virtual void setOffsetBlendAdd(bool blendMode) { mOffsetBlendAdd = blendMode; }
	public override void play(int keyframe, bool loop, float onceLength, float offset, bool fullOnce, float amplitude)
	{
		logError("use play(bool loop, float timeOffset, bool fullOnce) instead!");
	}
	public virtual void play(int keyframeID, bool loop, float timeOffset, bool fullOnce)
	{
		// 获取单次播放长度
		if (mValueKeyFrame != null && mValueKeyFrame.Count > 0)
		{
			mTimeList.Clear();
			mTimeList.AddRange(mValueKeyFrame.Keys);
			mMaxLength = mTimeList[mTimeList.Count - 1];
		}
		else
		{
			mMaxLength = 0.0f;
		}
		base.play(keyframeID, loop, mMaxLength, timeOffset, fullOnce, 1.0f);
	}
	//-------------------------------------------------------------------------------------------------------------
	protected override void applyTrembling(float value)
	{
		// 根据当前的距离找出位于哪两个点之间
		saturate(ref value);
		float curTime = value * mMaxLength;
		int index = findPointIndex(mTimeList, curTime, 0, mTimeList.Count - 1);
		if (index != mTimeList.Count - 1)
		{
			Vector3 startValue = mValueKeyFrame[mTimeList[index]];
			Vector3 endValue = mValueKeyFrame[mTimeList[index + 1]];
			float timePercentInSection = inverseLerp(mTimeList[index], mTimeList[index + 1], curTime);
			if(mOffsetBlendAdd)
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
				setValue(mValueKeyFrame[mTimeList[index]] + mValueOffset);
			}
			else
			{
				setValue(multiVector3(mValueKeyFrame[mTimeList[index]], mValueOffset));
			}
		}
	}
	protected abstract void setValue(Vector3 value);
}