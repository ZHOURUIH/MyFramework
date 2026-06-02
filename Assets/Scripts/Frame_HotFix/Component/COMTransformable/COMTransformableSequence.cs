using UnityEngine;

// 用于播放物体的缓动序列
public class COMTransformableSequence : GameComponent
{
	protected SequenceCallback mDoneCallback;
	protected TweenSequence mSequence;
	protected float mCurrentTime;               // 从上一次从头开始播放到现在的时长
	protected float mTotalLength;
	protected PLAY_STATE mPlayState;            // 播放状态
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
		mCurrentTime = 0.0f;
		mPlayState = PLAY_STATE.PLAY;
		mTotalLength = mSequence.getTotalLength();
	}
}