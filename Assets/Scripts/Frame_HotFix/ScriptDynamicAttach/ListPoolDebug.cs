using System.Collections.Generic;
using UnityEngine;
using static FrameBase;
using static StringUtility;

// 列表池的调试信息
public class ListPoolDebug : MonoBehaviour
{
	public List<string> PersistentInuseList = new();	// 持久使用的列表
	public List<string> InuseList = new();				// 单帧使用的列表
	public List<string> UnuseList = new();				// 未使用的列表
	public void Update()
	{
		if (mGameFramework == null || !mGameFramework.mParam.mEnableScriptDebug)
		{
			return;
		}
		PersistentInuseList.Clear();
		foreach (var item in mListPool.getPersistentInusedList())
		{
			if (item.Value.Count == 0)
			{
				continue;
			}
			PersistentInuseList.Add(item.Key + ", 数量:" + IToS(item.Value.Count));
		}

		InuseList.Clear();
		foreach(var item in mListPool.getInusedList())
		{
			if (item.Value.Count == 0)
			{
				continue;
			}
			InuseList.Add(item.Key + ", 数量:" + IToS(item.Value.Count));
		}

		UnuseList.Clear();
		foreach (var item in mListPool.getUnusedList())
		{
			if (item.Value.Count == 0)
			{
				continue;
			}
			UnuseList.Add(item.Key + ", 数量:" + IToS(item.Value.Count));
		}
	}
}