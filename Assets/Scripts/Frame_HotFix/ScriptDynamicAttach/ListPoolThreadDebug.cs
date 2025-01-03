using System.Collections.Generic;
using UnityEngine;
using static FrameBase;
using static StringUtility;

// 线程安全的对象池列表
public class ListPoolThreadDebug : MonoBehaviour
{
	public List<string> InuseList = new();		// 已使用列表
	public List<string> UnuseList = new();		// 未使用列表
	public void Update()
	{
		if (mGameFramework == null || !mGameFramework.mParam.mEnableScriptDebug)
		{
			return;
		}
		InuseList.Clear();
		UnuseList.Clear();
		using (new ThreadLockScope(mListPoolThread.getLock()))
		{
			foreach (var item in mListPoolThread.getInusedList())
			{
				if (item.Value.Count == 0)
				{
					continue;
				}
				InuseList.Add(item.Key + ", 数量:" + IToS(item.Value.Count));
			}

			foreach (var item in mListPoolThread.getUnusedList())
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