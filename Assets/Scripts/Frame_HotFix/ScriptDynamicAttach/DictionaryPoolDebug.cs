using System.Collections.Generic;
using UnityEngine;
using static FrameBaseHotFix;
using static StringUtility;

// 字典池的调试信息
public class DictionaryPoolDebug : MonoBehaviour
{
	public List<string> PersistentInuseList = new();    // 持久使用的对象列表
	public List<string> InuseList = new();              // 单帧使用的对象列表
	public List<string> UnuseList = new();              // 未使用的对象列表
	public void Update()
	{
		if (GameEntry.getInstance() == null || !GameEntry.getInstance().mFramworkParam.mEnableScriptDebug)
		{
			return;
		}
		PersistentInuseList.Clear();
		foreach (var item in mDictionaryPool.getPersistentInusedList())
		{
			PersistentInuseList.Add(item.Key + ":" + IToS(item.Value.Count));
		}

		InuseList.Clear();
		foreach (var item in mDictionaryPool.getInusedList())
		{
			if (item.Value.Count > 0)
			{
				InuseList.Add(item.Key + ":" + IToS(item.Value.Count));
			}
		}

		UnuseList.Clear();
		foreach (var item in mDictionaryPool.getUnusedList())
		{
			if (item.Value.Count > 0)
			{
				UnuseList.Add(item.Key + ":" + IToS(item.Value.Count));
			}
		}
	}
}