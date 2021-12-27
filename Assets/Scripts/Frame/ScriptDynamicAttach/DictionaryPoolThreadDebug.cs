using System.Collections.Generic;
using UnityEngine;

// 线程安全的字典池的调试信息
public class DictionaryPoolThreadDebug : MonoBehaviour
{
	public List<string> InuseList = new List<string>();	// 已使用列表
	public List<string> UnuseList = new List<string>();	// 未使用列表
	public void Update()
	{
		if (!FrameBase.mGameFramework.mEnableScriptDebug)
		{
			return;
		}
		FrameBase.mDictionaryPoolThread.lockList();
		InuseList.Clear();
		var inuse = FrameBase.mDictionaryPoolThread.getInusedList();
		foreach(var item in inuse)
		{
			if(item.Value.Count > 0)
			{
				InuseList.Add(item.Key.ToString() + ":" + StringUtility.IToS(item.Value.Count));
			}
		}

		UnuseList.Clear();
		var unuse = FrameBase.mDictionaryPoolThread.getUnusedList();
		foreach (var item in unuse)
		{
			if (item.Value.Count > 0)
			{
				UnuseList.Add(item.Key.ToString() + ":" + StringUtility.IToS(item.Value.Count));
			}
		}
		FrameBase.mDictionaryPoolThread.unlockList();
	}
}