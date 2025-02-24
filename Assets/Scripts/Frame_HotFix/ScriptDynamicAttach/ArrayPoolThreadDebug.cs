using System.Collections.Generic;
using UnityEngine;
using static FrameBaseHotFix;
using static StringUtility;

// 线程安全的ArrayPool调试信息
public class ArrayPoolThreadDebug : MonoBehaviour
{
	public List<string> InuseList = new();		// 已使用对象列表
	public List<string> UnuseList = new();		// 未使用对象列表
	public void Update()
	{
		if (mGameFrameworkHotFix == null || !mGameFrameworkHotFix.mParam.mEnableScriptDebug)
		{
			return;
		}

		InuseList.Clear();
		using (new ThreadLockScope(mArrayPoolThread.getLock()))
		{
			foreach (var itemTypeList in mArrayPoolThread.getInusedList())
			{
				foreach (var array in itemTypeList.Value)
				{
					if (array.Value.Count == 0)
					{
						continue;
					}
					InuseList.Add(strcat(itemTypeList.Key.ToString(), ": 长度:", IToS(array.Key), ", 个数:", IToS(array.Value.Count)));
				}
			}

			UnuseList.Clear();
			foreach (var itemTypeList in mArrayPoolThread.getUnusedList())
			{
				foreach (var array in itemTypeList.Value)
				{
					if (array.Value.Count == 0)
					{
						continue;
					}
					UnuseList.Add(strcat(itemTypeList.Key.ToString(), ": 长度:", IToS(array.Key), ", 个数:", IToS(array.Value.Count)));
				}
			}
		}
	}
}