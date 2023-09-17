using System.Collections.Generic;
using UnityEngine;
using static FrameBase;
using static StringUtility;

// 列表池的调试信息
public class ListPoolDebug : MonoBehaviour
{
	public List<string> PersistentInuseList = new List<string>();	// 持久使用的列表
	public List<string> InuseList = new List<string>();				// 单帧使用的列表
	public List<string> UnuseList = new List<string>();				// 未使用的列表
	public void Update()
	{
		if (mGameFramework == null || !mGameFramework.mEnableScriptDebug)
		{
			return;
		}
		PersistentInuseList.Clear();
		var persistentInuse = mListPool.getPersistentInusedList();
		foreach (var item in persistentInuse)
		{
			if (item.Value.Count == 0)
			{
				continue;
			}
			PersistentInuseList.Add(item.Key + ", 数量:" + IToS(item.Value.Count));
		}

		InuseList.Clear();
		var inuse = mListPool.getInusedList();
		foreach(var item in inuse)
		{
			if (item.Value.Count == 0)
			{
				continue;
			}
			InuseList.Add(item.Key + ", 数量:" + IToS(item.Value.Count));
		}

		UnuseList.Clear();
		var unuse = mListPool.getUnusedList();
		foreach (var item in unuse)
		{
			if (item.Value.Count == 0)
			{
				continue;
			}
			UnuseList.Add(item.Key + ", 数量:" + IToS(item.Value.Count));
		}
	}
}