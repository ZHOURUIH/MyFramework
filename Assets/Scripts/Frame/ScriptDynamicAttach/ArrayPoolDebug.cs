using System.Collections.Generic;
using UnityEngine;
using static StringUtility;
using static FrameBase;

// ArrayPool的调试信息
public class ArrayPoolDebug : MonoBehaviour
{
	public List<string> PersistentInuseList = new List<string>();		// 持久使用的列表对象
	public List<string> InuseList = new List<string>();					// 单帧使用的列表对象
	public List<string> UnuseList = new List<string>();					// 未使用的列表对象
	public void Update()
	{
		if (mGameFramework == null || !mGameFramework.mEnableScriptDebug)
		{
			return;
		}
		PersistentInuseList.Clear();
		var persistentInuse = mArrayPool.getPersistentInusedList();
		foreach (var itemTypeList in persistentInuse)
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
		var inuse = mArrayPool.getInusedList();
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
		var unuse = mArrayPool.getUnusedList();
		foreach (var itemTypeList in unuse)
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