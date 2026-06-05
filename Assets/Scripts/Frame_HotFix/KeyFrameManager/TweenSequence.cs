using System.Collections.Generic;
using UnityEngine;
using static MathUtility;
using static UnityUtility;

public class TweenSequence : MonoBehaviour
{
	public List<TweenGroup> mGroupList = new();
	public bool mLoop;
	private Vector3 mOriginPos;
	private Vector3 mOriginScale;
	private Vector3 mOriginRot;
	private bool mNeedResetOrigin;		// 是否需要重置原始位置、缩放和旋转,当播放过以后就需要重置
	public bool mResetWhenStop;			// 停止播放时是否重置到原始位置、缩放和旋转
	// 获取缓动序列的总长度,即所有TweenGroup中最长的长度
	public float getTotalLength()
	{
		float totalLength = 0.0f;
		foreach (TweenGroup group in mGroupList)
		{
			totalLength = getMax(totalLength, group.getGroupLength());
		}
		return totalLength;
	}
	public bool hasSelfValueType()
	{
		foreach (TweenGroup group in mGroupList)
		{
			if (group.hasSelfValueType())
			{
				return true;
			}
		}
		return false;
	}
	public void stop(bool forceReset = false)
	{
		foreach (var group in mGroupList)
		{
			foreach (var track in group.mTrackList)
			{
				track.stop();
			}
		}
		if (mNeedResetOrigin && (mResetWhenStop || forceReset))
		{
			transform.localPosition = mOriginPos;
			transform.localScale = mOriginScale;
			transform.localEulerAngles = mOriginRot;
			mNeedResetOrigin = false;
		}
	}
	public void play()
	{
		if (mLoop && hasSelfValueType())
		{
			logError("存在SELF模式轨道时不允许循环播放");
			mLoop = false;
		}
		// 开始播放时设置每个Track的开始时间和结束时间
		foreach (TweenGroup group in mGroupList)
		{
			float time = 0.0f;
			foreach (TweenTrack track in group.mTrackList)
			{
				if (!track.mEnable)
				{
					continue;
				}
				time += track.mStartDelay;
				track.setBeginTime(time);
				time += track.mDuration;
				track.setEndTime(time);
			}
		}
		mOriginPos = transform.localPosition;
		mOriginScale = transform.localScale;
		mOriginRot = transform.localEulerAngles;
		mNeedResetOrigin = true;
	}
	// 获取缓动序列在curTime时间点的位置、缩放和旋转
	public void evaluateSequence(float curTime, out Vector3 pos, out Vector3 scale, out Vector3 rotation)
	{
		pos = transform.localPosition;
		scale = transform.localScale;
		rotation = transform.localEulerAngles;
		foreach (TweenGroup group in mGroupList)
		{
			foreach (TweenTrack track in group.mTrackList)
			{
				if (!track.mEnable)
				{
					continue;
				}
				MyCurve curve = track.getCurve();
				if (curve == null)
				{
					continue;
				}

				float currentStartTime = track.getBeginTime();
				float currentEndTime = track.getEndTime();
				// 当前Track还没开始
				if (curTime < currentStartTime)
				{
					break;
				}
				// 当前Track已经结束
				if (curTime > currentEndTime)
				{
					// Track结束后确认已经停止Track
					if (track.isPlaying())
					{
						track.stop();
						// 要处理好最后一帧,因为上一帧track没有结束,这一帧超过了track结束时间,就要保证track的最后一帧数据是正确的
						Vector3 result0 = lerpSimple(track.getStartValue(), track.getTargetValue(), curve.evaluate(1.0f));
						switch (track.mType)
						{
							case TWEEN_TYPE.MOVE: pos = result0; break;
							case TWEEN_TYPE.SCALE: scale = result0; break;
							case TWEEN_TYPE.ROTATE: rotation = result0; break;
						}
					}
					continue;
				}

				if (!track.isPlaying())
				{
					track.play(transform);
				}

				float percent;
				// 当前Track已经结束,这里再次判断,为了处理好track的最后一帧
				if (curTime >= currentEndTime)
				{
					percent = 1.0f;
				}
				// 当前Track正在播放
				else
				{
					percent = clampMax((curTime - currentStartTime) / clampMin(track.mDuration, 0.0001f), 1.0f);
				}

				Vector3 result = lerpSimple(track.getStartValue(), track.getTargetValue(), curve.evaluate(percent));
				switch (track.mType)
				{
					case TWEEN_TYPE.MOVE: pos = result; break;
					case TWEEN_TYPE.SCALE: scale = result; break;
					case TWEEN_TYPE.ROTATE: rotation = result; break;
				}

				// 正在当前Track中
				if (curTime < currentEndTime)
				{
					break;
				}
				// Track在这一帧结束
				if (percent >= 1.0f)
				{
					track.stop();
				}
			}
		}
	}
}