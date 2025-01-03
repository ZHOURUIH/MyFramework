using System.Collections.Generic;
using UnityEngine;
using static StringUtility;
using static FrameBase;

// ArrayPool的调试信息
public class ArrayPoolDebug : MonoBehaviour
{
	public List<string> PersistentInuseList = new();		// 持久使用的列表对象
	public List<string> InuseList = new();					// 单帧使用的列表对象
	public List<string> UnuseList = new();					// 未使用的列表对象
	public void Update()
	{
		if (mGameFramework == null || !mGameFramework.mParam.mEnableScriptDebug)
		{
			return;
		}
		PersistentInuseList.Clear();
		foreach (var itemTypeList in mArrayPool.getPersistentInusedList())
		{
			foreach (var array in itemTypeList.Value)
			{
				if (array.Value.Count == 0)
				{
					continue;
				}
				PersistentInuseList.Add(strcat(itemTypeList.Key.ToString(), ": 长度:", IToS(array.Key), ", 个数:", IToS(array.Value.Count)));
			}
		}

		InuseList.Clear();
		foreach (var itemTypeList in mArrayPool.getInusedList())
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
		foreach (var itemTypeList in mArrayPool.getUnusedList())
		{
			foreach (var array in itemTypeList.Value)
			{
				if (array.Value.Count > 0)
				{
					continue;
				}
				UnuseList.Add(strcat(itemTypeList.Key.ToString(), ": 长度:", IToS(array.Key), ", 个数:", IToS(array.Value.Count)));
			}
		}
	}
}