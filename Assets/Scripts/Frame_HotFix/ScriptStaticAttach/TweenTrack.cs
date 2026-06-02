using System;
using UnityEngine;
using static FrameBaseHotFix;

public enum TWEEN_TYPE
{
	MOVE,
	ROTATE,
	SCALE,
}

[Serializable]
public class TweenTrack
{
	public TWEEN_TYPE mType;
	public int mCurveID;
	protected MyCurve mCurve;			// 用于在运行时缓存曲线对象
	public float mDuration = 0.3f;
	public float mStartDelay;
	public Vector3 mStartValue;
	public Vector3 mTargetValue;
	public MyCurve getCurve()
	{
		if (mCurve == null && mCurveID > 0)
		{
			if (mKeyFrameManager != null)
			{
				mCurve = mKeyFrameManager.getKeyFrame(mCurveID);
			}
			else
			{
				mCurve = EditorCurveFactory.getCurve(mCurveID);
			}
		}
		return mCurve;
	}
}