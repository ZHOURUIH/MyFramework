using System.Collections.Generic;
using UnityEngine;
using static FrameBaseHotFix;

// 音效管理器的调试信息
public class AudioManagerDebug : MonoBehaviour
{
	public List<string> AudioList = new();			// 音频列表
	public List<string> LoadedAudioList = new();	// 已加载音频列表
	public void Update()
	{
		if (GameEntry.getInstance() == null || !GameEntry.getInstance().mFramworkParam.mEnableScriptDebug)
		{
			return;
		}
		AudioList.Clear();
		LoadedAudioList.Clear();
		foreach (var item in mAudioManager.getAudioList())
		{
			AudioInfo info = item.Value;
			if (info.mIsLocal)
			{
				AudioList.Add(info.mState + "\t" + info.mAudioName + ",  InResources");
			}
			else
			{
				AudioList.Add(info.mState + "\t" + info.mAudioName);
			}
			if (info.mState == LOAD_STATE.LOADED)
			{
				LoadedAudioList.Add(info.mAudioName);
			}
		}
	}
}