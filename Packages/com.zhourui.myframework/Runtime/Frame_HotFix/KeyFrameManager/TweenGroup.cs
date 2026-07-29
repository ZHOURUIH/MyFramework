using System;
using System.Collections.Generic;

[Serializable]
// 补间组,将多个补间组合在一起,可统一暂停/恢复/停止
public class TweenGroup
{
	public List<TweenTrack> mTrackList = new();
	public float getGroupLength()
	{
		float length = 0.0f;
		foreach (TweenTrack track in mTrackList)
		{
			if (!track.mEnable)
			{
				continue;
			}
			length += track.mDuration;
			length += track.mStartDelay;
		}
		return length;
	}
	public bool hasSelfValueType()
	{
		foreach (TweenTrack track in mTrackList)
		{
			if (track.mStartMode == START_MODE.SELF || track.mTargetMode == TARGET_MODE.SELF)
			{
				return true;
			}
		}
		return false;
	}
}