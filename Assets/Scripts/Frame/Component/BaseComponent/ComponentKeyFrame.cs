using UnityEngine;
using static UnityUtility;
using static MathUtility;
using static FrameBase;

// 关键帧组件基类
public class ComponentKeyFrame : GameComponent, IComponentBreakable
{
	// 用于设置的参数
	protected KeyFrameCallback mDoingCallback;	// 变化中回调
	protected KeyFrameCallback mDoneCallback;	// 变化完成时回调
	protected MyCurve mKeyFrame;				// 当前使用的关键帧
	protected float mPlayLength;				// 小于0表示无限播放, 大于0表示播放length时长
	protected float mStopValue;					// 当组件停止时,需要应用的关键帧值
	protected float mOnceLength;				// 关键帧长度默认为1秒
	protected float mOffset;					// 起始的时间偏移
	protected int mKeyframeID;					// 关键帧曲线ID
	protected bool mLoop;						// 是否循环
	//------------------------------------------------------------------------------------------------------------------------------
	// 用于实时计算的参数
	protected float mCurrentTime;				// 从上一次从头开始播放到现在的时长
	protected float mPlayedTime;				// 本次震动已经播放的时长,从上一次开始播放到现在的累计时长
	protected float mCurValue;					// 当前曲线上的取值
	protected PLAY_STATE mPlayState;			// 播放状态
	public ComponentKeyFrame()
	{
		mLoop = true;
		mOnceLength = 1.0f;
		mPlayState = PLAY_STATE.STOP;
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mKeyFrame = null;
		mDoingCallback = null;
		mDoneCallback = null;
		mPlayLength = 0.0f;
		mStopValue = 0.0f;
		mOnceLength = 1.0f;
		mOffset = 0.0f;
		mKeyframeID = 0;
		mLoop = true;
		mCurrentTime = 0.0f;
		mPlayedTime = 0.0f;
		mCurValue = 0.0f;
		mPlayState = PLAY_STATE.STOP;
	}
	public override void destroy()
	{
		// 销毁前需要确定会执行回调
		doneCallback(ref mDoneCallback, this, true);
		base.destroy();
	}
	public override void setActive(bool active)
	{
		base.setActive(active);
		if (!active)
		{
			stop();
		}
	}
	public virtual void play(int keyframe, bool loop, float onceLength, float offset)
	{
		if (!isActive())
		{
			return;
		}
		setKeyframeID(keyframe);
		mKeyFrame = mKeyFrameManager.getKeyFrame(mKeyframeID);
		if (onceLength < 0.0f)
		{
			logError("onceLength can not be negative!");
			return;
		}
		if (mKeyFrame == null || isFloatZero(onceLength))
		{
			mStopValue = 0.0f;
			// 停止并禁用组件
			afterApplyTrembling(true);
			return;
		}
		else
		{
			mStopValue = mKeyFrame.evaluate(mKeyFrame.getLength());
		}
		if (offset > onceLength)
		{
			logError("offset must be less than onceLength!");
		}
		mOnceLength = onceLength;
		mPlayState = PLAY_STATE.PLAY;
		mLoop = loop;
		mOffset = offset;
		mCurrentTime = mOffset;
		mPlayedTime = 0.0f;
		if (mLoop)
		{
			mPlayLength = -1.0f;
		}
		else
		{
			mPlayLength = mOnceLength - offset;
		}
		update(0.0f);
	}
	public virtual void stop(bool force = false)
	{
		// 如果已经是停止的状态,并且不是要强制停止,则不再执行
		if (mPlayState == PLAY_STATE.STOP && !force)
		{
			return;
		}
		// 构建值全部为默认值的关键帧
		if (mComponentOwner != null)
		{
			applyTrembling(mStopValue);
		}
		mPlayState = PLAY_STATE.STOP;
		mKeyFrame = null;
		mCurrentTime = 0.0f;
		mPlayedTime = 0.0f;
	}
	public virtual void pause() { mPlayState = PLAY_STATE.PAUSE; }
	public void setState(PLAY_STATE state)
	{
		if (mPlayState == state)
		{
			return;
		}
		if (state == PLAY_STATE.PLAY)
		{
			play(mKeyframeID, mLoop, mOnceLength, mOffset);
		}
		else if (state == PLAY_STATE.STOP)
		{
			stop();
		}
		else if (state == PLAY_STATE.PAUSE)
		{
			pause();
		}
	}
	public float getTremblingPercent()
	{
		return mOnceLength > 0.0f ? mCurrentTime / mOnceLength : 0.0f;
	}
	public void setDoingCallback(KeyFrameCallback callback)
	{
		setCallback(callback, ref mDoingCallback, this);
	}
	public void setDoneCallback(KeyFrameCallback callback)
	{
		setCallback(callback, ref mDoneCallback, this);
	}
	public void notifyBreak()
	{
		setDoingCallback(null);
		setDoneCallback(null);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 获得成员变量
	public bool isLoop() { return mLoop; }
	public float getOnceLength() { return mOnceLength; }
	public float getOffset() { return mOffset; }
	public PLAY_STATE getState() { return mPlayState; }
	public float getCurrentTime() { return mCurrentTime; }
	public MyCurve getKeyFrame() { return mKeyFrame; }
	public int getKeyframeID() { return mKeyframeID; }
	public float getCurValue() { return mCurValue; }
	//------------------------------------------------------------------------------------------------------------------------------
	// 设置成员变量
	public void setLoop(bool loop) { mLoop = loop; }
	public void setOnceLength(float length) { mOnceLength = length; }
	public void setOffset(float offset) { mOffset = offset; }
	public void setCurrentTime(float time) { mCurrentTime = time; }
	public void setKeyframeID(int keyframe) { mKeyframeID = keyframe; }
	//------------------------------------------------------------------------------------------------------------------------------
	protected void clearCallback()
	{
		mDoingCallback = null;
		mDoneCallback = null;
	}
	protected void afterApplyTrembling(bool done)
	{
		mDoingCallback?.Invoke(this, false);
		if (done)
		{
			setActive(false);
			// 强制停止组件
			stop(true);
			doneCallback(ref mDoneCallback, this, false);
		}
	}
	protected static void doneCallback(ref KeyFrameCallback curDoneCallback, ComponentKeyFrame com, bool isBreak)
	{
		// 先保存回调,然后再调用回调之前就清空回调,确保在回调函数执行时已经完全完成
		KeyFrameCallback tempCallback = curDoneCallback;
		com.clearCallback();
		tempCallback?.Invoke(com, isBreak);
	}
	protected static void setCallback(KeyFrameCallback callback, ref KeyFrameCallback curCallback, ComponentKeyFrame com)
	{
		KeyFrameCallback tempCallback = curCallback;
		curCallback = null;
		// 如果回调函数当前不为空,则是中断了正在进行的变化
		tempCallback?.Invoke(com, true);
		curCallback = callback;
	}
	protected virtual void applyTrembling(float value) { }
}