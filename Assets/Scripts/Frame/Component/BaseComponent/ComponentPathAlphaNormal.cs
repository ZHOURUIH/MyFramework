using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

// 指定路径关键帧和时间关键帧进行变换
public abstract class ComponentPathAlphaNormal : ComponentKeyFrameNormal
{
	protected Dictionary<float, float> mValueKeyFrame;
	protected List<float> mTimeList;
	protected float mValueOffset;
	protected float mMaxLength;
	protected float mSpeed;
	public ComponentPathAlphaNormal()
	{
		mValueOffset = 1.0f;
		mSpeed = 1.0f;
		mTimeList = new List<float>();
	}
	public void setValueKeyFrame(Dictionary<float, float> path) { mValueKeyFrame = path; }
	public void setSpeed(float speed) { mSpeed = speed; }
	public void setValueOffset(float offset) { mValueOffset = offset; }
	public override void play(string name, bool loop, float onceLength, float offset, bool fullOnce, float amplitude)
	{
		logError("use play(bool loop, float timeOffset, bool fullOnce) instead!");
	}
	public virtual void play(bool loop, float timeOffset, bool fullOnce)
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
		base.play(FrameDefine.ZERO_ONE, loop, mMaxLength, timeOffset, fullOnce, 1.0f);
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
			float startValue = mValueKeyFrame[mTimeList[index]];
			float endValue = mValueKeyFrame[mTimeList[index + 1]];
			float timePercentInSection = inverseLerp(mTimeList[index], mTimeList[index + 1], curTime);
			setValue(lerp(startValue, endValue, timePercentInSection) * mValueOffset);
		}
		else
		{
			setValue(mValueKeyFrame[mTimeList[index]] * mValueOffset);
		}
	}
	protected abstract void setValue(float value);
}