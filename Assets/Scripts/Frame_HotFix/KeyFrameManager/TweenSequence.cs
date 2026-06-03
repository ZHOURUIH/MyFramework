using System.Collections.Generic;
using UnityEngine;
using static MathUtility;

public class TweenSequence : MonoBehaviour
{
	public List<TweenGroup> mGroupList = new();
	[HideInInspector]
	public float mPreviewTime;
	public bool mLoop;
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
	// 获取缓动序列在curTime时间点的位置、缩放和旋转
	public void evaluateSequence(float curTime, out Vector3 pos, out Vector3 scale, out Vector3 rotation)
	{
		pos = Vector3.zero;
		scale = Vector3.one;
		rotation = Vector3.zero;
		foreach (TweenGroup group in mGroupList)
		{
			float currentStartTime = 0.0f;
			foreach (TweenTrack track in group.mTrackList)
			{
				float currentEndTime = currentStartTime + track.mDuration;
				MyCurve curve = track.getCurve();
				if (curve == null)
				{
					currentStartTime = currentEndTime + track.mStartDelay;
					continue;
				}

				// 当前Track还没开始
				if (curTime < currentStartTime)
				{
					break;
				}

				float percent;
				// 当前Track已经结束
				if (curTime >= currentEndTime)
				{
					percent = 1.0f;
				}
				// 当前Track正在播放
				else
				{
					percent = (curTime - currentStartTime) / getMax(track.mDuration, 0.0001f);
				}

				Vector3 result = lerpSimple(track.mStartValue, track.getTargetValue(), curve.evaluate(percent));
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
				currentStartTime = currentEndTime + track.mStartDelay;
			}
		}
	}
}