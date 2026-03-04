using UnityEngine;
using System.Collections.Generic;
using static UnityUtility;
using static MathUtility;

// 指定路径关键帧和时间关键帧进行变换,包含两条不同的关键帧
public abstract class ComponentPath2 : ComponentKeyFrame
{
	protected Dictionary<float, Vector3> mValueKeyFrame0;		// 值与时间的关键帧列表
	protected Dictionary<float, Vector3> mValueKeyFrame1;		// 值与时间的关键帧列表
	protected List<Vector3> mValueList0 = new();				// 值的列表
	protected List<float> mTimeList0 = new();					// 时间列表
	protected List<Vector3> mValueList1 = new();                // 值的列表
	protected List<float> mTimeList1 = new();					// 时间列表
	protected Vector3 mValueOffset0;							// 值的偏移
	protected Vector3 mValueOffset1;							// 值的偏移
	protected float mMaxLength;									// mTimeList中的最大值,两个TimeList中的最大值应该是一致的
	protected float mSpeed = 1.0f;                              // 变化速度
	protected int mLastValueIndex0;								// 缓存的上一次查找到的下标
	protected int mLastValueIndex1;                             // 缓存的上一次查找到的下标
	protected bool mOffsetBlendAdd0 = true;						// true表示直接相加,false表示相乘
	protected bool mOffsetBlendAdd1 = true;						// true表示直接相加,false表示相乘
	public override void resetProperty()
	{
		base.resetProperty();
		mValueKeyFrame0 = null;
		mValueKeyFrame1 = null;
		mValueList0.Clear();
		mTimeList0.Clear();
		mValueList1.Clear();
		mTimeList1.Clear();
		mValueOffset0 = Vector3.zero;
		mValueOffset1 = Vector3.zero;
		mMaxLength = 0.0f;
		mSpeed = 1.0f;
		mLastValueIndex0 = 0;
		mLastValueIndex1 = 0;
		mOffsetBlendAdd0 = true;
		mOffsetBlendAdd1 = true;
	}
	public void setValueKeyFrame0(Dictionary<float, Vector3> path) { mValueKeyFrame0 = path; }
	public void setValueKeyFrame1(Dictionary<float, Vector3> path) { mValueKeyFrame1 = path; }
	public void setSpeed(float speed) { mSpeed = speed; }
	public void setValueOffset0(Vector3 offset) { mValueOffset0 = offset; }
	public void setValueOffset1(Vector3 offset) { mValueOffset1 = offset; }
	public virtual void setOffsetBlendAdd0(bool blendMode) { mOffsetBlendAdd0 = blendMode; }
	public virtual void setOffsetBlendAdd1(bool blendMode) { mOffsetBlendAdd1 = blendMode; }
	public override void play(int keyframe, bool loop, float onceLength, float offset)
	{
		logError("use play(bool loop, float timeOffset) instead!");
	}
	public virtual void play(int keyframeID, bool loop, float timeOffset)
	{
		// 获取单次播放长度
		mMaxLength = 0.0f;
		if (!mValueKeyFrame0.isEmpty())
		{
			mValueList0.setRange(mValueKeyFrame0.Values);
			mTimeList0.setRange(mValueKeyFrame0.Keys);
			mValueList1.setRange(mValueKeyFrame1.Values);
			mTimeList1.setRange(mValueKeyFrame1.Keys);
			mMaxLength = mTimeList0[^1];
			if (!isFloatEqual(mTimeList0[^1], mTimeList1[^1]))
			{
				logError("两个关键帧的最大时间需要一致!");
			}
		}
		mLastValueIndex0 = 0;
		mLastValueIndex1 = 0;
		base.play(keyframeID, loop, mMaxLength, timeOffset);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected override void applyTrembling(float value)
	{
		// 根据当前的距离找出位于哪两个点之间
		saturate(ref value);
		float curTime = value * mMaxLength;

		mLastValueIndex0 = findPointIndex(mTimeList0, curTime, mLastValueIndex0);
		Vector3 startValue0 = mValueList0[mLastValueIndex0];
		if (mLastValueIndex0 < mTimeList0.Count - 1)
		{
			startValue0 = lerp(startValue0, mValueList0[mLastValueIndex0 + 1], inverseLerp(mTimeList0[mLastValueIndex0], mTimeList0[mLastValueIndex0 + 1], curTime));
		}
		Vector3 value0 = mOffsetBlendAdd0 ? startValue0 + mValueOffset0 : multiVector3(startValue0, mValueOffset0);

		mLastValueIndex1 = findPointIndex(mTimeList1, curTime, mLastValueIndex1);
		Vector3 startValue1 = mValueList1[mLastValueIndex1];
		if (mLastValueIndex1 < mTimeList1.Count - 1)
		{
			startValue1 = lerp(startValue1, mValueList1[mLastValueIndex1 + 1], inverseLerp(mTimeList1[mLastValueIndex1], mTimeList1[mLastValueIndex1 + 1], curTime));
		}
		Vector3 value1 = mOffsetBlendAdd1 ? startValue1 + mValueOffset1 : multiVector3(startValue1, mValueOffset1);
		setValue(value0, value1);
	}
	protected abstract void setValue(Vector3 value0, Vector3 value1);
}