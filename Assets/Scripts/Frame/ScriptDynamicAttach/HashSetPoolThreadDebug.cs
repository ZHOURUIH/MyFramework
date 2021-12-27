using System.Collections.Generic;
using UnityEngine;

// 线程安全的HashSet对象池的调试信息
public class HashSetPoolThreadDebug : MonoBehaviour
{
	public List<string> InuseList = new List<string>();		// 已使用列表
	public List<string> UnuseList = new List<string>();		// 未使用列表
	public void Update()
	{
		if (!FrameBase.mGameFramework.mEnableScriptDebug)
		{
			return;
		}
		FrameBase.mHashSetPoolThread.lockList();
		InuseList.Clear();
		var inuse = FrameBase.mHashSetPoolThread.getInusedList();
		foreach(var item in inuse)
		{
			if (item.Value.Count > 0)
			{
				InuseList.Add(item.Key.ToString() + ", 数量:" + StringUtility.IToS(item.Value.Count));
			}
		}

		UnuseList.Clear();
		var unuse = FrameBase.mHashSetPoolThread.getUnusedList();
		foreach (var item in unuse)
		{
			if(item.Value.Count > 0)
			{
				UnuseList.Add(item.Key.ToString() + ", 数量:" + StringUtility.IToS(item.Value.Count));
			}
		}
		FrameBase.mHashSetPoolThread.unlockList();
	}
}