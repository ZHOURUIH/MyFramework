using System.Collections.Generic;
using UnityEngine;
using static MathUtility;
using static FrameUtility;
using static UnityUtility;

public class DamageNumberData : ClassObject
{
	public Dictionary<float, Vector3> mPositionKeyFrames;	// 位置关键帧列表
	public Dictionary<float, Vector3> mScaleKeyFrames;      // 缩放关键帧列表
	public float[] mPositionTimeList;						// 关键帧中的时间列表
	public Vector3[] mPositionList;							// 关键帧中的位置列表
	public float[] mScaleTimeList;							// 关键帧中时间列表
	public Vector3[] mScaleList;							// 关键帧中的缩放列表
	public Vector3 mPositionOffset;							// 位置偏移
	public Vector3 mScaleOffset;							// 缩放偏移
	public Vector3 mPosition;								// 当前的整体位置
	public Vector3 mScale;									// 当前的整体缩放
	public List<byte> mNumbers = new();						// 显示的数字
	public List<DamageNumberFlag> mExtraFlags = new();		// 额外显示的标记
	public float mSpeed = 1.0f;								// 移动速度倍率
	public float mCurTime;									// 当前计时
	public float mTotalWidth;								// 数字显示的总宽度,会考虑间距,不含标记
	public float mKeyFrameMaxTime;							// 整个移动的时间,从关键帧列表中获取的,获取不到就默认为0
	public int mLastPositionKeyIndex;						// 缓存上一次查找的mPositionTimeList的下标
	public int mLastScaleKeyIndex;                          // 缓存上一次查找的mScaleTimeList的下标
	public override void destroy()
	{
		base.destroy();
		UN_CLASS_LIST(mExtraFlags);
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
		mNumbers.Clear();
		mExtraFlags.Clear();
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
		other.mNumbers.setRange(mNumbers);
		other.mExtraFlags.setRange(mExtraFlags);
		other.mSpeed = mSpeed;
		other.mCurTime = mCurTime;
		other.mTotalWidth = mTotalWidth;
		other.mKeyFrameMaxTime = mKeyFrameMaxTime;
	}
	public void setPositionKeyframes(Dictionary<float, Vector3> keyframes)
	{
		mPositionKeyFrames = keyframes;
		if (mPositionKeyFrames.isEmpty())
		{
			return;
		}
		if (mPositionTimeList.count() < mPositionKeyFrames.Count)
		{
			mPositionTimeList = new float[mPositionKeyFrames.Count];
		}
		if (mPositionList.count() < mPositionKeyFrames.Count)
		{
			mPositionList = new Vector3[mPositionKeyFrames.Count];
		}
		int index = 0;
		foreach (var item in mPositionKeyFrames)
		{
			mPositionTimeList[index] = item.Key;
			mPositionList[index++] = item.Value;
		}
	}
	public void setScaleKeyframes(Dictionary<float, Vector3> keyframes)
	{
		mScaleKeyFrames = keyframes;
		if (mScaleKeyFrames.isEmpty())
		{
			return;
		}
		if (mScaleTimeList.count() < mScaleKeyFrames.Count)
		{
			mScaleTimeList = new float[mScaleKeyFrames.Count];
		}
		if (mScaleList.count() < mScaleKeyFrames.Count)
		{
			mScaleList = new Vector3[mScaleKeyFrames.Count];
		}
		int index = 0;
		foreach (var item in mScaleKeyFrames)
		{
			mScaleTimeList[index] = item.Key;
			mScaleList[index++] = item.Value;
		}
	}
	public void init()
	{
		if (!mPositionTimeList.isEmpty())
		{
			mKeyFrameMaxTime = mPositionTimeList[^1];
			if (!isFloatEqual(mPositionTimeList[^1], mScaleTimeList[^1]))
			{
				logError("两个关键帧的最大时间需要一致!");
			}
		}
		else
		{
			mKeyFrameMaxTime = 1.0f;
		}
		mPosition = mPositionOffset;
		mScale = mScaleOffset;
	}
}