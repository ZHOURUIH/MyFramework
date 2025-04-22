using System.Collections.Generic;
using UnityEngine;
using static FrameBaseHotFix;
using static StringUtility;

// ClassPool调试信息
public class ClassPoolDebug : MonoBehaviour
{
	public List<string> PersistentInuseList = new();		// 持久使用的对象列表
	public List<string> InuseList = new();					// 单帧使用的对象列表
	public List<string> UnuseList = new();					// 未使用的对象列表
	public void Update()
	{
		if (GameEntry.getInstance() == null || !GameEntry.getInstance().mFramworkParam.mEnableScriptDebug)
		{
			return;
		}
		PersistentInuseList.Clear();
		foreach (var itemTypeList in mClassPool.getPersistentInusedList())
		{
			if (itemTypeList.Value.Count == 0)
			{
				continue;
			}
			PersistentInuseList.Add(itemTypeList.Key + ": 个数:" + IToS(itemTypeList.Value.Count));
		}

		InuseList.Clear();
		foreach (var itemTypeList in mClassPool.getInusedList())
		{
			if (itemTypeList.Value.Count == 0)
			{
				continue;
			}
			InuseList.Add(itemTypeList.Key + ": 个数:" + IToS(itemTypeList.Value.Count));
		}

		UnuseList.Clear();
		foreach (var itemTypeList in mClassPool.getUnusedList())
		{
			if (itemTypeList.Value.Count == 0)
			{
				continue;
			}
			UnuseList.Add(itemTypeList.Key + ": 个数:" + IToS(itemTypeList.Value.Count));
		}
	}
}