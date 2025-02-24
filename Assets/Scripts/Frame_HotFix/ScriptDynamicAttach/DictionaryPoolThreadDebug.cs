using System.Collections.Generic;
using UnityEngine;
using static FrameBaseHotFix;
using static StringUtility;

// 线程安全的字典池的调试信息
public class DictionaryPoolThreadDebug : MonoBehaviour
{
	public List<string> InuseList = new();  // 已使用列表
	public List<string> UnuseList = new();  // 未使用列表
	public void Update()
	{
		if (mGameFrameworkHotFix == null || !mGameFrameworkHotFix.mParam.mEnableScriptDebug)
		{
			return;
		}
		InuseList.Clear();
		UnuseList.Clear();
		using (new ThreadLockScope(mDictionaryPoolThread.getLock()))
		{
			foreach (var item in mDictionaryPoolThread.getInusedList())
			{
				if (item.Value.Count > 0)
				{
					InuseList.Add(item.Key + ":" + IToS(item.Value.Count));
				}
			}

			foreach (var item in mDictionaryPoolThread.getUnusedList())
			{
				if (item.Value.Count > 0)
				{
					UnuseList.Add(item.Key + ":" + IToS(item.Value.Count));
				}
			}
		}
	}
}