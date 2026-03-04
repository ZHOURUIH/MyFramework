using System.Collections.Generic;
using UnityEngine;
using static FrameBaseHotFix;
using static StringUtility;

// 线程安全的HashSet对象池的调试信息
public class HashSetPoolThreadDebug : MonoBehaviour
{
	public List<string> InuseList = new();		// 已使用列表
	public List<string> UnuseList = new();		// 未使用列表
	public void Update()
	{
		if (GameEntry.getInstance() == null || !GameEntry.getInstance().mFramworkParam.mEnableScriptDebug)
		{
			return;
		}
		InuseList.Clear();
		UnuseList.Clear();
		using (new ThreadLockScope(mHashSetPoolThread.getLock()))
		{
			foreach (var item in mHashSetPoolThread.getInusedList())
			{
				InuseList.addIf(item.Key + ", 数量:" + IToS(item.Value.Count), item.Value.Count != 0);
			}

			foreach (var item in mHashSetPoolThread.getUnusedList())
			{
				UnuseList.addIf(item.Key + ", 数量:" + IToS(item.Value.Count), item.Value.Count != 0);
			}
		}
	}
}