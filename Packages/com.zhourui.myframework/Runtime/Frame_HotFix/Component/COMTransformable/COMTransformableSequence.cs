using UnityEngine;

// 用于播放物体的缓动序列
public class COMTransformableSequence : GameComponent
{
	protected SequenceCallback mDoneCallback;   // 序列播放完成的回调,参数1:当前组件,参数2:是否被打断
	protected TweenSequence mSequence;          // 当前正在播放的缓动序列
	protected float mCurrentTime;               // 从上一次从头开始播放到现在的时长
	protected float mTotalLength;               // 序列的总长度,即所有TweenGroup中最长的长度
	protected PLAY_STATE mPlayState;            // 播放状态
    public override void resetProperty()
    {
        base.resetProperty();
		mDoneCallback = null;
		mSequence = null;
		mCurrentTime = 0.0f;
        mTotalLength = 0.0f;
		mPlayState = PLAY_STATE.NONE;
    }
	public void setDoneCallback(SequenceCallback callback)
	{
		mDoneCallback = callback;
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		if (mPlayState == PLAY_STATE.PLAY)
		{
			mCurrentTime += elapsedTime;
			mSequence.evaluateSequence(mCurrentTime, out Vector3 pos, out Vector3 scale, out Vector3 rotation);
			var transformable = mComponentOwner as Transformable;
			transformable.setPosition(pos);
			transformable.setScale(scale);
			transformable.setRotation(rotation);
			// 是否结束播放
			if (mCurrentTime >= mTotalLength)
			{
				if (mSequence.mLoop)
				{
					mCurrentTime -= mTotalLength;
				}
				else
				{
					stop(false);
				}
			}
		}
	}
	public void stop(bool isBreak)
	{
		setActive(false);
		mSequence.stop();
		mSequence = null;
		mCurrentTime = 0.0f;
		mPlayState = PLAY_STATE.STOP;
		SequenceCallback callback = mDoneCallback;
		mDoneCallback = null;
		callback?.Invoke(this, isBreak);
	}
	public void play(TweenSequence sequence)
	{
		mSequence = sequence;
		if (sequence == null)
		{
			stop(true);
			return;
		}
		// 播放之前先确认所有轨道都是在停止状态的
		mSequence.stop(true);
		mSequence.play();
		mCurrentTime = 0.0f;
		mPlayState = PLAY_STATE.PLAY;
		mTotalLength = mSequence.getTotalLength();
	}
}