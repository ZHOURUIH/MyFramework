using System.Collections.Generic;
using UnityEngine;
using static FrameBaseHotFix;

// 字典池的调试信息
public class DictionaryPoolDebug : MonoBehaviour
{
	public List<string> PersistentInuseList = new();    // 持久使用的对象列表
	public List<string> InuseList = new();              // 单帧使用的对象列表
	public List<string> UnuseList = new();              // 未使用的对象列表
	public void Update()
	{
		if (GameEntryBase.getInstance() == null || !GameEntryBase.getInstance().mFrameworkParam.mEnableScriptDebug)
		{
			return;
		}
		PersistentInuseList.Clear();
		foreach (var item in mDictionaryPool.getPersistentInusedList())
		{
			PersistentInuseList.Add(item.Key + ":" + item.Value.Count.IToS());
		}

		InuseList.Clear();
		foreach (var item in mDictionaryPool.getInusedList())
		{
			InuseList.addIf(item.Key + ":" + item.Value.Count.IToS(), item.Value.Count > 0);
		}

		UnuseList.Clear();
		foreach (var item in mDictionaryPool.getUnusedList())
		{
			UnuseList.addIf(item.Key + ":" + item.Value.Count.IToS(), item.Value.Count > 0);
		}
	}
}