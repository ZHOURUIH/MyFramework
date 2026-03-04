using UnityEngine;
using System.Collections.Generic;
using static UnityUtility;
using static MathUtility;

// 指定路径关键帧和时间关键帧进行变换
public abstract class ComponentPath : ComponentKeyFrame
{
	protected Dictionary<float, Vector3> mValueKeyFrame;    // 值与时间的关键帧列表
	protected List<Vector3> mValueList = new();				// 值的列表
	protected List<float> mTimeList = new();                // 时间列表
	protected Vector3 mValueOffset;                         // 值的偏移
	protected float mMaxLength;                             // mTimeList中的最大值
	protected float mSpeed = 1.0f;                          // 变化速度
	protected int mLastKeyIndex;
	protected bool mOffsetBlendAdd = true;                  // true表示直接相加,false表示相乘
	public override void resetProperty()
	{
		base.resetProperty();
		mValueKeyFrame = null;
		mValueList.Clear();
		mTimeList.Clear();
		mValueOffset = Vector3.zero;
		mMaxLength = 0.0f;
		mSpeed = 1.0f;
		mLastKeyIndex = 0;
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
		mMaxLength = 0.0f;
		if (!mValueKeyFrame.isEmpty())
		{
			mValueList.setRange(mValueKeyFrame.Values);
			mTimeList.setRange(mValueKeyFrame.Keys);
			mMaxLength = mTimeList[^1];
		}
		mLastKeyIndex = 0;
		base.play(keyframeID, loop, mMaxLength, timeOffset);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected override void applyTrembling(float value)
	{
		// 根据当前的距离找出位于哪两个点之间
		saturate(ref value);
		float curTime = value * mMaxLength;
		mLastKeyIndex = findPointIndex(mTimeList, curTime, mLastKeyIndex);
		Vector3 startValue = mValueList[mLastKeyIndex];
		if (mLastKeyIndex < mTimeList.Count - 1)
		{
			startValue = lerp(startValue, mValueKeyFrame[mLastKeyIndex + 1], inverseLerp(mTimeList[mLastKeyIndex], mTimeList[mLastKeyIndex + 1], curTime));
		}
		setValue(mOffsetBlendAdd ? startValue + mValueOffset : multiVector3(startValue, mValueOffset));
	}
	protected abstract void setValue(Vector3 value);
}