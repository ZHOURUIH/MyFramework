using System;
using System.Collections.Generic;
using UnityEngine;

// 音效管理器的调试信息
public class AudioManagerDebug : MonoBehaviour
{
	public List<string> AudioList = new List<string>();
	public List<string> LoadedAudioList = new List<string>();
	public void Update()
	{
		if (FrameBase.mGameFramework == null || !FrameBase.mGameFramework.mEnableScriptDebug)
		{
			return;
		}
		AudioList.Clear();
		LoadedAudioList.Clear();
		var audioList = FrameBase.mAudioManager.getAudioList();
		foreach(var item in audioList)
		{
			if (item.Value.mIsResource)
			{
				AudioList.Add(item.Value.mState + "\t" + item.Value.mAudioName + ",  InResources");
			}
			else
			{
				AudioList.Add(item.Value.mState + "\t" + item.Value.mAudioName);
			}
			if (item.Value.mState == LOAD_STATE.LOADED)
			{
				LoadedAudioList.Add(item.Value.mAudioName);
			}
		}
	}
}