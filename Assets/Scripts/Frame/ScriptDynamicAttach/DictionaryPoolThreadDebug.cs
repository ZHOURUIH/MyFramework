using System.Collections.Generic;
using UnityEngine;
using static FrameBase;
using static StringUtility;

// 线程安全的字典池的调试信息
public class DictionaryPoolThreadDebug : MonoBehaviour
{
	public List<string> InuseList = new List<string>();	// 已使用列表
	public List<string> UnuseList = new List<string>();	// 未使用列表
	public void Update()
	{
		if (mGameFramework == null || !mGameFramework.mEnableScriptDebug)
		{
			return;
		}
		InuseList.Clear();
		UnuseList.Clear();
		using (new ThreadLockScope(mDictionaryPoolThread.getLock()))
		{
			var inuse = mDictionaryPoolThread.getInusedList();
			foreach (var item in inuse)
			{
				if (item.Value.Count > 0)
				{
					InuseList.Add(item.Key + ":" + IToS(item.Value.Count));
				}
			}

			var unuse = mDictionaryPoolThread.getUnusedList();
			foreach (var item in unuse)
			{
				if (item.Value.Count > 0)
				{
					UnuseList.Add(item.Key + ":" + IToS(item.Value.Count));
				}
			}
		}
	}
}