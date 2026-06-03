using System;
using UnityEngine;
using static FrameBaseHotFix;

[Serializable]
public class TweenTrack
{
	protected MyCurve mCurve;					// 用于在运行时缓存曲线对象
	public TARGET_MODE mTargetMode = TARGET_MODE.VALUE;
	public Transform mTargetTransform;          // 参考的目标节点,TRANSFORM模式
	public Vector3 mTargetOffset;               // 是否加偏移
	public TWEEN_TYPE mType;                    // 轨道类型,如位置、缩放、旋转等
	public int mCurveID;                        // 曲线ID,用于在运行时获取曲线对象
	public float mDuration = 0.3f;              // 持续时间
	public float mStartDelay;                   // 开始前的延迟时间
	public Vector3 mStartValue;                 // 起始值,在编辑器中设置,运行时根据目标对象的当前值进行调整
	public Vector3 mTargetValue;                // 目标值,用于VALUE模式,此字段仅用于序列化,要获取实际的目标值请调用getTargetValue()方法
	public void setCurveID(int id)
	{
		if (mCurveID != id)
		{
			mCurve = null;
		}
		mCurveID = id;
	}
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
	public Vector3 getTargetValue()
	{
		switch (mTargetMode)
		{
			case TARGET_MODE.VALUE:return mTargetValue;
			case TARGET_MODE.TRANSFORM:
			{
				if (mTargetTransform == null)
				{
					Debug.LogError("Tween target transform is null");
					return mTargetOffset;
				}
				switch (mType)
				{
					case TWEEN_TYPE.MOVE: return mTargetTransform.localPosition + mTargetOffset;
					case TWEEN_TYPE.SCALE: return mTargetTransform.localScale + mTargetOffset;
					case TWEEN_TYPE.ROTATE: return mTargetTransform.localEulerAngles + mTargetOffset;
				}
				break;
			}
		}
		Debug.LogError("Unsupported tween type:" + mType);
		return Vector3.zero;
	}
}