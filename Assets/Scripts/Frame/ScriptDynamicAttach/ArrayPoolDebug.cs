using System.Collections.Generic;
using UnityEngine;

// ArrayPool的调试信息
public class ArrayPoolDebug : MonoBehaviour
{
	public List<string> PersistentInuseList = new List<string>();		// 持久使用的列表对象
	public List<string> InuseList = new List<string>();					// 单帧使用的列表对象
	public List<string> UnuseList = new List<string>();					// 未使用的列表对象
	public void Update()
	{
		if (!FrameBase.mGameFramework.mEnableScriptDebug)
		{
			return;
		}
		PersistentInuseList.Clear();
		var persistentInuse = FrameBase.mArrayPool.getPersistentInusedList();
		foreach (var itemTypeList in persistentInuse)
		{
			foreach (var array in itemTypeList.Value)
			{
				if(array.Value.Count > 0)
				{
					PersistentInuseList.Add(StringUtility.strcat(itemTypeList.Key.ToString(), 
											": 长度:", 
											StringUtility.IToS(array.Key), 
											", 个数:", 
											StringUtility.IToS(array.Value.Count)));
				}
			}
		}

		InuseList.Clear();
		var inuse = FrameBase.mArrayPool.getInusedList();
		foreach (var itemTypeList in inuse)
		{
			foreach (var array in itemTypeList.Value)
			{
				if(array.Value.Count > 0)
				{
					InuseList.Add(StringUtility.strcat(itemTypeList.Key.ToString(), 
														": 长度:", 
														StringUtility.IToS(array.Key), 
														", 个数:", 
														StringUtility.IToS(array.Value.Count)));
				}
			}
		}

		UnuseList.Clear();
		var unuse = FrameBase.mArrayPool.getUnusedList();
		foreach (var itemTypeList in unuse)
		{
			foreach (var array in itemTypeList.Value)
			{
				if (array.Value.Count > 0)
				{
					UnuseList.Add(StringUtility.strcat(itemTypeList.Key.ToString(), 
									": 长度:", 
									StringUtility.IToS(array.Key), 
									", 个数:", 
									StringUtility.IToS(array.Value.Count)));
				}
			}
		}
	}
}