using System.Collections.Generic;
using UnityEngine;
using static FrameBase;
using static StringUtility;

// 线程安全的ArrayPool调试信息
public class ArrayPoolThreadDebug : MonoBehaviour
{
	public List<string> InuseList = new List<string>();		// 已使用对象列表
	public List<string> UnuseList = new List<string>();		// 未使用对象列表
	public void Update()
	{
		if (mGameFramework == null || !mGameFramework.mEnableScriptDebug)
		{
			return;
		}

		InuseList.Clear();
		using (new ThreadLockScope(mArrayPoolThread.getLock()))
		{
			var inuse = mArrayPoolThread.getInusedList();
			foreach (var itemTypeList in inuse)
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
			var unuse = mArrayPoolThread.getUnusedList();
			foreach (var itemTypeList in unuse)
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