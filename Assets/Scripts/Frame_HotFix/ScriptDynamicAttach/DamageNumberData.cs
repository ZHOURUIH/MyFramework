using System.Collections.Generic;
using UnityEngine;
using static FrameUtility;
using static MathUtility;
using static UnityUtility;

public class DamageNumberData : ClassObject
{
	public Dictionary<float, Vector3> mPositionKeyFrames;	// 位置关键帧列表
	public Dictionary<float, Vector3> mScaleKeyFrames;      // 缩放关键帧列表
	public float[] mPositionTimeList;						// 关键帧中的时间列表
	public Vector3[] mPositionList;							// 关键帧中的位置列表
	public float[] mScaleTimeList;							// 关键帧中时间列表
	public Vector3[] mScaleList;							// 关键帧中的缩放列表
	public Vector3 mPositionOffset;
	public Vector3 mScaleOffset;
	public Vector3 mPosition;
	public Vector3 mScale;
	public List<byte> mNumbers;
	public float mSpeed = 1.0f;
	public float mCurTime;
	public float mTotalWidth;
	public float mKeyFrameMaxTime;
	public int mLastPositionKeyIndex;                    // 缓存上一次查找的mPositionTimeList的下标
	public int mLastScaleKeyIndex;                       // 缓存上一次查找的mScaleTimeList的下标
	public override void destroy()
	{
		base.destroy();
		UN_LIST(mNumbers);
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mPositionKeyFrames = null;
		mScaleKeyFrames = null;
		mPositionTimeList.setAllValue(0.0f);
		mPositionList.setAllValue(Vector3.zero);
		mScaleTimeList.setAllValue(0.0f);
		mScaleList.setAllValue(Vector3.zero);
		mPositionOffset = Vector3.zero;
		mScaleOffset = Vector3.zero;
		mPosition = Vector3.zero;
		mScale = Vector3.zero;
		mNumbers = null;
		mSpeed = 1.0f;
		mCurTime = 0.0f;
		mTotalWidth = 0.0f;
		mKeyFrameMaxTime = 0.0f;
		mLastPositionKeyIndex = 0;
		mLastScaleKeyIndex = 0;
	}
	public void cloneTo(DamageNumberData other)
	{
		other.setPositionKeyframes(mPositionKeyFrames);
		other.setScaleKeyframes(mScaleKeyFrames);
		other.mPositionOffset = mPositionOffset;
		other.mScaleOffset = mScaleOffset;
		other.mPosition = mPosition;
		other.mScale = mScale;
		LIST_PERSIST(out other.mNumbers).setRange(mNumbers);
		other.mSpeed = mSpeed;
		other.mCurTime = mCurTime;
		other.mTotalWidth = mTotalWidth;
		other.mKeyFrameMaxTime = mKeyFrameMaxTime;
	}
	public void setPositionKeyframes(Dictionary<float, Vector3> keyframes)
	{
		mPositionKeyFrames = keyframes;
		if (mPositionTimeList.count() < mPositionKeyFrames.Count)
		{
			mPositionTimeList = new float[mPositionKeyFrames.Count];
		}
		if (mPositionList.count() < mPositionKeyFrames.Count)
		{
			mPositionList = new Vector3[mPositionKeyFrames.Count];
		}
		mPositionTimeList.setRange(mPositionKeyFrames.Keys);
		mPositionList.setRange(mPositionKeyFrames.Values);
	}
	public void setScaleKeyframes(Dictionary<float, Vector3> keyframes)
	{
		mScaleKeyFrames = keyframes;
		if (mScaleTimeList.count() < mScaleKeyFrames.Count)
		{
			mScaleTimeList = new float[mScaleKeyFrames.Count];
		}
		if (mScaleList.count() < mScaleKeyFrames.Count)
		{
			mScaleList = new Vector3[mScaleKeyFrames.Count];
		}
		mScaleTimeList.setRange(mScaleKeyFrames.Keys);
		mScaleList.setRange(mScaleKeyFrames.Values);
	}
	public void init()
	{
		mKeyFrameMaxTime = mPositionTimeList[^1];
		if (!isFloatEqual(mPositionTimeList[^1], mScaleTimeList[^1]))
		{
			logError("两个关键帧的最大时间需要一致!");
		}
	}
}