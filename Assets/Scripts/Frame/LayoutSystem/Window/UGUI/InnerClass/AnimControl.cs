using System;

public class AnimControl : FrameBase
{
	protected OnPlayEndCallback mPlayEndCallback;   // 一个序列播放完时的回调函数,只在非循环播放状态下有效
	protected OnPlayingCallback mPlayingCallback;   // 一个序列正在播放时的回调函数
	protected myUIObject mControlObject;			// 控制的窗口
	protected float mPlayedTime;                    // 已经播放的时长,不包含循环次数
	protected float mInterval;                      // 隔多少秒切换图片
	protected float mCurTime;						// 当前播放计时
	protected int mCurTextureIndex;					// 当前播放的下标
	protected int mTextureCount;					// 序列帧数量
	protected int mStartIndex;                      // 序列帧的起始帧下标,默认为0,即从头开始
	protected int mEndIndex;                        // 序列帧的终止帧下标,默认为-1,即播放到尾部
	protected bool mUseTextureSelfSize;             // 在切换图片时是否使用图片自身的大小
	protected bool mAutoResetIndex;                 // 是否在播放完毕后自动重置当前帧下标,也表示是否在非循环播放完毕后自动隐藏
	protected bool mPlayDirection;                  // 播放方向,true为正向播放(从mStartIndex到mEndIndex),false为返向播放(从mEndIndex到mStartIndex)
	protected PLAY_STATE mPlayState;				// 当前播放状态
	protected LOOP_MODE mLoopMode;					// 循环方式
	public AnimControl()
	{
		mInterval = 0.033f;
		mEndIndex = -1;
		mPlayDirection = true;
		mAutoResetIndex = true;
		mUseTextureSelfSize = true;
		mLoopMode = LOOP_MODE.ONCE;
		mPlayState = PLAY_STATE.STOP;
	}
	public void setObject(myUIObject obj) { mControlObject = obj; }
	public void update(float elapsedTime)
	{
		if (mPlayState != PLAY_STATE.PLAY)
		{
			return;
		}
		int lastIndex = mCurTextureIndex;
		if (mTextureCount != 0)
		{
			mCurTime += elapsedTime;
			if (mInterval > 0.0f && mCurTime > mInterval)
			{
				// 一帧时间内可能会跳过多帧序列帧
				int elapsedFrames = (int)(mCurTime / mInterval);
				mCurTime -= elapsedFrames * mInterval;
				if (mPlayDirection)
				{
					if (mCurTextureIndex + elapsedFrames <= getRealEndIndex())
					{
						mCurTextureIndex += elapsedFrames;
					}
					else
					{
						if (mLoopMode == LOOP_MODE.ONCE)
						{
							// 非循环播放时播放完成后,停止播放
							stop(mAutoResetIndex, true, false);
						}
						// 普通循环,则将下标重置到起始下标
						else if (mLoopMode == LOOP_MODE.LOOP)
						{
							mCurTextureIndex = mStartIndex;
						}
						// 来回循环,则将下标重置到终止下标,并且开始反向播放
						else if (mLoopMode == LOOP_MODE.PING_PONG)
						{
							mCurTextureIndex = getRealEndIndex();
							mPlayDirection = !mPlayDirection;
						}
					}
				}
				else
				{
					if (mCurTextureIndex - elapsedFrames >= mStartIndex)
					{
						mCurTextureIndex -= elapsedFrames;
					}
					else
					{
						if (mLoopMode == LOOP_MODE.ONCE)
						{
							// 非循环播放时播放完成后,停止播放
							stop(mAutoResetIndex, true, false);
						}
						// 普通循环,则将下标重置到终止下标
						else if (mLoopMode == LOOP_MODE.LOOP)
						{
							mCurTextureIndex = getRealEndIndex();
						}
						// 来回循环,则将下标重置到起始下标,并且开始正向播放
						else if (mLoopMode == LOOP_MODE.PING_PONG)
						{
							mCurTextureIndex = mStartIndex;
							mPlayDirection = !mPlayDirection;
						}
					}
				}
			}
			if (mPlayDirection)
			{
				mPlayedTime = mCurTime + mCurTextureIndex * mInterval;
			}
			else
			{
				mPlayedTime = (mTextureCount - mCurTextureIndex) * mInterval - mCurTime;
			}
		}
		else
		{
			mCurTextureIndex = 0;
		}
		if(lastIndex != mCurTextureIndex)
		{
			mPlayingCallback?.Invoke(this, mCurTextureIndex, true);
		}
	}
	public LOOP_MODE getLoop() { return mLoopMode; }
	public float getInterval() { return mInterval; }
	public float getSpeed() { return intervalToSpeed(mInterval); }
	public int getStartIndex() { return mStartIndex; }
	public PLAY_STATE getPlayState() { return mPlayState; }
	public bool getPlayDirection() { return mPlayDirection; }
	public int getEndIndex() { return mEndIndex; }
	public int getTextureFrameCount() { return mTextureCount; }
	public bool isAutoResetIndex() { return mAutoResetIndex; }
	public float getPlayedTime() { return mPlayedTime; }
	public float getLength() { return mTextureCount * mInterval; }
	// 获得实际的终止下标,如果是自动获得,则返回最后一张的下标
	public int getRealEndIndex()
	{
		if (mEndIndex < 0)
		{
			return getMax(getTextureFrameCount() - 1, 0);
		}
		else
		{
			return mEndIndex;
		}
	}
	public void setLoop(LOOP_MODE loop) { mLoopMode = loop; }
	public void setInterval(float interval) { mInterval = interval; }
	public void setSpeed(float speed) { setInterval(speedToInterval(speed)); }
	public void setFrameCount(int count)
	{
		mTextureCount = count;
		// 重新判断起始下标和终止下标,确保下标不会越界
		clamp(ref mStartIndex, 0, getTextureFrameCount() - 1);
		if (mEndIndex >= 0)
		{
			clamp(ref mEndIndex, 0, getTextureFrameCount() - 1);
		}
		if (getTextureFrameCount() > 0 && mStartIndex >= 0 && mStartIndex < getTextureFrameCount())
		{
			mCurTextureIndex = mStartIndex;
			mPlayingCallback?.Invoke(this, mCurTextureIndex, false);
		}
	}
	public void setPlayDirection(bool direction) { mPlayDirection = direction; }
	public void setAutoHide(bool autoReset) { mAutoResetIndex = autoReset; }
	public void setStartIndex(int startIndex)
	{
		mStartIndex = startIndex;
		clamp(ref mStartIndex, 0, getTextureFrameCount() - 1);
	}
	public void setEndIndex(int endIndex)
	{
		mEndIndex = endIndex;
		if (mEndIndex >= 0)
		{
			clamp(ref mEndIndex, 0, getTextureFrameCount() - 1);
		}
	}
	public void stop(bool resetStartIndex = true, bool callback = true, bool isBreak = true)
	{
		mPlayState = PLAY_STATE.STOP;
		if (resetStartIndex)
		{
			setCurFrameIndex(mStartIndex);
		}
		// 中断序列帧播放时调用回调函数,只在非循环播放时才调用
		mPlayEndCallback?.Invoke(this, callback, isBreak);
	}
	public void play()
	{
		mPlayState = PLAY_STATE.PLAY; 
		// 开始播放时确认当前序列中下标,以便通知外部回调
		setCurFrameIndex(mCurTextureIndex); 
	}
	public void pause() { mPlayState = PLAY_STATE.PAUSE; }
	public void setPlayEndCallback(OnPlayEndCallback callback) { mPlayEndCallback = callback; }
	public void setPlayingCallback(OnPlayingCallback callback) { mPlayingCallback = callback; }
	public int getCurFrameIndex() { return mCurTextureIndex; }
	public void setCurFrameIndex(int index)
	{
		mCurTextureIndex = index;
		mCurTime = 0.0f;
		clamp(ref mCurTextureIndex, mStartIndex, getRealEndIndex());
		mPlayingCallback?.Invoke(this, mCurTextureIndex, false);
	}
}