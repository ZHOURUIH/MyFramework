using System.Collections.Generic;
using UnityEngine;

// 线程安全的对象池列表
public class ListPoolThreadDebug : MonoBehaviour
{
	public List<string> InuseList = new List<string>();		// 已使用列表
	public List<string> UnuseList = new List<string>();		// 未使用列表
	public void Update()
	{
		if (!FrameBase.mGameFramework.mEnableScriptDebug)
		{
			return;
		}
		FrameBase.mListPoolThread.lockList();
		InuseList.Clear();
		var inuse = FrameBase.mListPoolThread.getInusedList();
		foreach(var item in inuse)
		{
			if (item.Value.Count > 0)
			{
				InuseList.Add(item.Key.ToString() + ", 数量:" + StringUtility.IToS(item.Value.Count));
			}
		}

		UnuseList.Clear();
		var unuse = FrameBase.mListPoolThread.getUnusedList();
		foreach (var item in unuse)
		{
			if(item.Value.Count > 0)
			{
				UnuseList.Add(item.Key.ToString() + ", 数量:" + StringUtility.IToS(item.Value.Count));
			}
		}
		FrameBase.mListPoolThread.unlockList();
	}
}