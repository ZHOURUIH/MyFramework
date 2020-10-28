using System;
using System.Collections.Generic;
using UnityEngine;

public class ComponentKeyFramePhysics : ComponentKeyFrameBase
{
	public override void fixedUpdate(float elapsedTime)
	{
		base.fixedUpdate(elapsedTime);
		if (mKeyFrame != null && mPlayState == PLAY_STATE.PLAY)
		{
			mCurrentTime += elapsedTime;
			mPlayedTime += elapsedTime;
			bool done = false;
			// 无限播放当前震动
			if (mPlayLength < 0.0f)
			{
				if (mCurrentTime > mOnceLength)
				{
					mCurrentTime = 0.0f;
				}
			}
			// 播放固定长度的震动
			else
			{
				// 超过时间则停止,暂时不播放最后一帧
				if (mPlayedTime > mPlayLength)
				{
					done = true;
					mCurrentTime = mOffset + mPlayLength;
				}
				else if (mCurrentTime > mOnceLength)
				{
					mCurrentTime = 0.0f;
				}
			}
			mCurValue = mKeyFrame.Evaluate(mCurrentTime / mOnceLength) * mAmplitude;
			applyTrembling(mCurValue);
			afterApplyTrembling(done);
		}
	}
}