using System;
using System.Collections.Generic;

[Serializable]
public class TweenGroup
{
	public List<TweenTrack> mTrackList = new();
	public float getGroupLength()
	{
		float length = 0.0f;
		for (int i = 0; i < mTrackList.Count; ++i)
		{
			TweenTrack track = mTrackList[i];
			length += track.mDuration;
			length += track.mStartDelay;
		}
		return length;
	}
}