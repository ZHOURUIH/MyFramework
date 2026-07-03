using System.Collections.Generic;
using UnityEngine;
using static FrameBaseHotFix;

// 线程安全的字典池的调试信息
public class DictionaryPoolThreadDebug : MonoBehaviour
{
	public List<string> InuseList = new();  // 已使用列表
	public List<string> UnuseList = new();  // 未使用列表
	public void Update()
	{
		if (GameEntryBase.getInstance() == null || !GameEntryBase.getInstance().mFrameworkParam.mEnableScriptDebug)
		{
			return;
		}
		InuseList.Clear();
		UnuseList.Clear();
		using (new ThreadLockScope(mDictionaryPoolThread.getLock()))
		{
			foreach (var item in mDictionaryPoolThread.getInusedList())
			{
				InuseList.addIf(item.Key + ":" + item.Value.Count.IToS(), item.Value.Count > 0);
			}

			foreach (var item in mDictionaryPoolThread.getUnusedList())
			{
				UnuseList.addIf(item.Key + ":" + item.Value.Count.IToS(), item.Value.Count > 0);
			}
		}
	}
}