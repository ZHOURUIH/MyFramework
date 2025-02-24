using System.Collections.Generic;
using UnityEngine;
using static FrameBaseHotFix;
using static StringUtility;

// HashSet对象池的调试信息
public class HashSetPoolDebug : MonoBehaviour
{
	public List<string> PersistentInuseList = new();	// 持久使用的列表
	public List<string> InuseList = new();				// 单帧使用的列表
	public List<string> UnuseList = new();				// 未使用列表
	public void Update()
	{
		if (mGameFrameworkHotFix == null || !mGameFrameworkHotFix.mParam.mEnableScriptDebug)
		{
			return;
		}
		PersistentInuseList.Clear();
		foreach (var item in mHashSetPool.getPersistentInusedList())
		{
			if (item.Value.Count == 0)
			{
				continue;
			}
			PersistentInuseList.Add(item.Key + ", 数量:" + IToS(item.Value.Count));
		}

		InuseList.Clear();
		foreach(var item in mHashSetPool.getInusedList())
		{
			if (item.Value.Count == 0)
			{
				continue;
			}
			InuseList.Add(item.Key + ", 数量:" + IToS(item.Value.Count));
		}

		UnuseList.Clear();
		foreach (var item in mHashSetPool.getUnusedList())
		{
			if (item.Value.Count == 0)
			{
				continue;
			}
			UnuseList.Add(item.Key + ", 数量:" + IToS(item.Value.Count));
		}
	}
}