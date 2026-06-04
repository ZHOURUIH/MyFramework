using System;
using UnityEngine;
using static FrameBaseHotFix;

[Serializable]
public class TweenTrack
{
	protected MyCurve mCurve;							// 用于在运行时缓存曲线对象
	protected Vector3 mRuntimeStart;					// 缓存的起始值,避免在START_MODE.CURRENT模式下每次都要获取目标对象的当前值导致错误
	protected Vector3 mRuntimeTarget;					// 轨道开始播放时的目标值
	protected bool mPlaying;                            // 是否正在播放
	protected float mBeginTime;                         // 轨道的开始时间,由TweenSequence在buildTimeline()时设置
	protected float mEndTime;                           // 轨道的结束时间,由TweenSequence在buildTimeline()时设置
	public TARGET_MODE mTargetMode = TARGET_MODE.VALUE; // 目标值的获取方式
	public START_MODE mStartMode = START_MODE.VALUE;    // 起始值的获取方式
	public Transform mTargetTransform;					// 参考的目标节点,TRANSFORM模式
	public Vector3 mTargetOffset;						// 是否加偏移
	public TWEEN_TYPE mType;							// 轨道类型,如位置、缩放、旋转等
	public int mCurveID;								// 曲线ID,用于在运行时获取曲线对象
	public float mDuration = 0.3f;						// 持续时间
	public float mStartDelay;							// 开始前的延迟时间
	public Vector3 mStartValue;							// 起始值,在编辑器中设置,运行时根据目标对象的当前值进行调整
	public Vector3 mTargetValue;                        // 目标值,用于VALUE模式,此字段仅用于序列化,要获取实际的目标值请调用getTargetValue()方法
	public bool mEnable = true;                         // 轨道是否启用
	public float getBeginTime() { return mBeginTime; }
	public float getEndTime() { return mEndTime; }
	public void setBeginTime(float time) { mBeginTime = time; }
	public void setEndTime(float time) { mEndTime = time; }
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
			case TARGET_MODE.VALUE:					return mTargetValue;
			case TARGET_MODE.TRANSFORM_REALTIME:	return generateTargetValue(mTargetTransform);
			case TARGET_MODE.TRANSFORM_SNAPSHOT:	return mRuntimeTarget;
			case TARGET_MODE.SELF:					return mRuntimeTarget;
		}
		Debug.LogError("Unsupported tween type:" + mType);
		return Vector3.zero;
	}
	public Vector3 getStartValue() { return mRuntimeStart; }
	// 开始播放
	public void play(Transform transform)
	{
		mPlaying = true;
		// 起点只能在开始播放时获取,终点可以实时获取
		switch (mStartMode)
		{
			case START_MODE.VALUE: mRuntimeStart = mStartValue; break;
			case START_MODE.SELF: mRuntimeStart = getTransformValue(transform);break;
		}
		if (mTargetMode == TARGET_MODE.TRANSFORM_SNAPSHOT)
		{
			mRuntimeTarget = generateTargetValue(mTargetTransform);
		}
		else if (mTargetMode == TARGET_MODE.SELF)
		{
			mRuntimeTarget = generateTargetValue(transform);
		}
	}
	public void stop() { mPlaying = false; }
	public bool isPlaying() { return mPlaying; }
	//------------------------------------------------------------------------------------------------------------------------------
	// 生成目标值,如果是TRANSFORM模式,则根据目标对象的当前值和偏移计算出目标值
	protected Vector3 generateTargetValue(Transform transform)
	{
		if (transform == null)
		{
			return mTargetOffset;
		}
		return getTransformValue(transform) + mTargetOffset;
	}
	// 根据轨道类型获取Transform上的对应值
	protected Vector3 getTransformValue(Transform transform)
	{
		switch (mType)
		{
			case TWEEN_TYPE.MOVE: return transform.localPosition;
			case TWEEN_TYPE.SCALE: return transform.localScale;
			case TWEEN_TYPE.ROTATE: return transform.localEulerAngles;
		}
		return Vector3.zero;
	}
}