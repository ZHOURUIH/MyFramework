using System.Collections.Generic;
using UnityEngine;
using static FrameBase;

// 音效管理器的调试信息
public class AudioManagerDebug : MonoBehaviour
{
	public List<string> AudioList = new();			// 音频列表
	public List<string> LoadedAudioList = new();	// 已加载音频列表
	public void Update()
	{
		if (mGameFramework == null || !mGameFramework.mParam.mEnableScriptDebug)
		{
			return;
		}
		AudioList.Clear();
		LoadedAudioList.Clear();
		foreach (AudioInfo item in mAudioManager.getAudioList().Values)
		{
			if (item.mIsLocal)
			{
				AudioList.Add(item.mState + "\t" + item.mAudioName + ",  InResources");
			}
			else
			{
				AudioList.Add(item.mState + "\t" + item.mAudioName);
			}
			if (item.mState == LOAD_STATE.LOADED)
			{
				LoadedAudioList.Add(item.mAudioName);
			}
		}
	}
}