using System.Collections.Generic;
using static UnityUtility;
using static MathUtility;

// 指定路径关键帧和时间关键帧进行变换
public abstract class ComponentPathAlpha : ComponentKeyFrame
{
	protected Dictionary<float, float> mValueKeyFrame;		// 透明度与时间的关键帧列表
	protected List<float> mTimeList = new();				// 计算出的时间列表
	protected float mValueOffset = 1.0f;					// 透明度偏移
	protected float mMaxLength;								// mTimeList中的最大值
	protected float mSpeed = 1.0f;							// 变化速度
	public override void resetProperty()
	{
		base.resetProperty();
		mValueKeyFrame = null;
		mTimeList.Clear();
		mValueOffset = 1.0f;
		mMaxLength = 0.0f;
		mSpeed = 1.0f;
	}
	public void setValueKeyFrame(Dictionary<float, float> path) { mValueKeyFrame = path; }
	public void setSpeed(float speed) { mSpeed = speed; }
	public void setValueOffset(float offset) { mValueOffset = offset; }
	public override void play(int keyframe, bool loop, float onceLength, float offset)
	{
		logError("use play(bool loop, float timeOffset) instead!");
	}
	public virtual void play(bool loop, float timeOffset)
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
		base.play(KEY_CURVE.ZERO_ONE, loop, mMaxLength, timeOffset);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected override void applyTrembling(float value)
	{
		// 根据当前的距离找出位于哪两个点之间
		saturate(ref value);
		float curTime = value * mMaxLength;
		int index = findPointIndex(mTimeList, curTime, 0, mTimeList.Count - 1);
		float startValue = mValueKeyFrame.get(mTimeList[index]);
		if (index < mTimeList.Count - 1)
		{
			float endValue = mValueKeyFrame.get(mTimeList[index + 1]);
			float timePercentInSection = inverseLerp(mTimeList[index], mTimeList[index + 1], curTime);
			setValue(lerp(startValue, endValue, timePercentInSection) * mValueOffset);
		}
		else
		{
			setValue(startValue * mValueOffset);
		}
	}
	protected abstract void setValue(float value);
}