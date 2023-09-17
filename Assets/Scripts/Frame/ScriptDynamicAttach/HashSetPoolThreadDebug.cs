using System.Collections.Generic;
using UnityEngine;
using static FrameBase;
using static StringUtility;

// 线程安全的HashSet对象池的调试信息
public class HashSetPoolThreadDebug : MonoBehaviour
{
	public List<string> InuseList = new List<string>();		// 已使用列表
	public List<string> UnuseList = new List<string>();		// 未使用列表
	public void Update()
	{
		if (mGameFramework == null || !mGameFramework.mEnableScriptDebug)
		{
			return;
		}
		InuseList.Clear();
		UnuseList.Clear();
		using (new ThreadLockScope(mHashSetPoolThread.getLock()))
		{
			var inuse = mHashSetPoolThread.getInusedList();
			foreach (var item in inuse)
			{
				if (item.Value.Count == 0)
				{
					continue;
				}
				InuseList.Add(item.Key + ", 数量:" + IToS(item.Value.Count));
			}

			var unuse = mHashSetPoolThread.getUnusedList();
			foreach (var item in unuse)
			{
				if (item.Value.Count == 0)
				{
					continue;
				}
				UnuseList.Add(item.Key + ", 数量:" + IToS(item.Value.Count));
			}
		}
	}
}