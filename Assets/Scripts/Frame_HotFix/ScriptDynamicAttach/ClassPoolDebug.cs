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
			PersistentInuseList.addIf(itemTypeList.Key + ": 个数:" + IToS(itemTypeList.Value.Count), itemTypeList.Value.Count != 0);
		}

		InuseList.Clear();
		foreach (var itemTypeList in mClassPool.getInusedList())
		{
			InuseList.addIf(itemTypeList.Key + ": 个数:" + IToS(itemTypeList.Value.Count), itemTypeList.Value.Count != 0);
		}

		UnuseList.Clear();
		foreach (var itemTypeList in mClassPool.getUnusedList())
		{
			UnuseList.addIf(itemTypeList.Key + ": 个数:" + IToS(itemTypeList.Value.Count), itemTypeList.Value.Count != 0);
		}
	}
}