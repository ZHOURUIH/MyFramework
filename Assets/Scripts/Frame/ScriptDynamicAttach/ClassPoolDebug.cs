using System.Collections.Generic;
using UnityEngine;
using static FrameBase;
using static StringUtility;

// ClassPool调试信息
public class ClassPoolDebug : MonoBehaviour
{
	public List<string> PersistentInuseList = new List<string>();		// 持久使用的对象列表
	public List<string> InuseList = new List<string>();					// 单帧使用的对象列表
	public List<string> UnuseList = new List<string>();					// 未使用的对象列表
	public void Update()
	{
		if (mGameFramework == null || !mGameFramework.mEnableScriptDebug)
		{
			return;
		}
		PersistentInuseList.Clear();
		var persistentInuse = mClassPool.getPersistentInusedList();
		foreach (var itemTypeList in persistentInuse)
		{
			if (itemTypeList.Value.Count == 0)
			{
				continue;
			}
			PersistentInuseList.Add(itemTypeList.Key + ": 个数:" + IToS(itemTypeList.Value.Count));
		}

		InuseList.Clear();
		var inuse = mClassPool.getInusedList();
		foreach (var itemTypeList in inuse)
		{
			if (itemTypeList.Value.Count == 0)
			{
				continue;
			}
			InuseList.Add(itemTypeList.Key + ": 个数:" + IToS(itemTypeList.Value.Count));
		}

		UnuseList.Clear();
		var unuse = mClassPool.getUnusedList();
		foreach (var itemTypeList in unuse)
		{
			if (itemTypeList.Value.Count == 0)
			{
				continue;
			}
			UnuseList.Add(itemTypeList.Key + ": 个数:" + IToS(itemTypeList.Value.Count));
		}
	}
}