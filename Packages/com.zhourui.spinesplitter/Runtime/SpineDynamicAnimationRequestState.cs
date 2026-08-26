using System;
using System.Collections.Generic;
using UnityEngine;

// 记录每个Spine组件各轨道最后一次播放请求,避免异步加载完成后播放已经过期的动画。
public class SpineDynamicAnimationRequestState : MonoBehaviour
{
    private readonly Dictionary<int, int> mTrackRequestVersions = new Dictionary<int, int>();
    private readonly Dictionary<int, string> mTrackRequestAnimationNames = new Dictionary<int, string>();
    public int beginRequest(int trackIndex, string animationName)
    {
        mTrackRequestVersions.TryGetValue(trackIndex, out int version);
        ++version;
        mTrackRequestVersions[trackIndex] = version;
        mTrackRequestAnimationNames[trackIndex] = animationName;
        return version;
    }
    public bool isCurrentRequest(int trackIndex, int version)
    {
        return mTrackRequestVersions.TryGetValue(trackIndex, out int currentVersion) && currentVersion == version;
    }
    public bool isRequesting(int trackIndex, string animationName)
    {
        return mTrackRequestAnimationNames.TryGetValue(trackIndex, out string currentAnimationName) && 
            string.Equals(currentAnimationName, animationName, StringComparison.Ordinal);
    }
    public void completeRequest(int trackIndex, int version)
    {
        if (isCurrentRequest(trackIndex, version))
        {
            mTrackRequestAnimationNames.Remove(trackIndex);
        }
    }
    public void cancelTrack(int trackIndex)
    {
        mTrackRequestVersions.TryGetValue(trackIndex, out int version);
        mTrackRequestVersions[trackIndex] = version + 1;
        mTrackRequestAnimationNames.Remove(trackIndex);
    }
}