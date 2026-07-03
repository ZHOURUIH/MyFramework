using System.Collections.Generic;
using UnityEngine;
using static FrameBaseHotFix;
using static StringUtility;

// 线程安全的对象池列表
public class ListPoolThreadDebug : MonoBehaviour
{
	public List<string> InuseList = new();		// 已使用列表
	public List<string> UnuseList = new();		// 未使用列表
	public void Update()
	{
		if (GameEntryBase.getInstance() == null || !GameEntryBase.getInstance().mFrameworkParam.mEnableScriptDebug)
		{
			return;
		}
		InuseList.Clear();
		UnuseList.Clear();
		using (new ThreadLockScope(mListPoolThread.getLock()))
		{
			foreach (var item in mListPoolThread.getInusedList())
			{
				InuseList.addIf(item.Key + ", 数量:" + item.Value.Count.IToS(), item.Value.Count != 0);
			}

			foreach (var item in mListPoolThread.getUnusedList())
			{
				UnuseList.addIf(item.Key + ", 数量:" + item.Value.Count.IToS(), item.Value.Count != 0);
			}
		}
	}
}