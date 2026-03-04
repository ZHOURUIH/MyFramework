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
		if (GameEntry.getInstance() == null || !GameEntry.getInstance().mFramworkParam.mEnableScriptDebug)
		{
			return;
		}
		PersistentInuseList.Clear();
		foreach (var item in mHashSetPool.getPersistentInusedList())
		{
			PersistentInuseList.addIf(item.Key + ", 数量:" + IToS(item.Value.Count), item.Value.Count != 0);
		}

		InuseList.Clear();
		foreach(var item in mHashSetPool.getInusedList())
		{
			InuseList.addIf(item.Key + ", 数量:" + IToS(item.Value.Count), item.Value.Count != 0);
		}

		UnuseList.Clear();
		foreach (var item in mHashSetPool.getUnusedList())
		{
			UnuseList.addIf(item.Key + ", 数量:" + IToS(item.Value.Count), item.Value.Count != 0);
		}
	}
}