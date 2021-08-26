using System.Collections.Generic;
using UnityEngine;

public class ArrayPoolThreadDebug : MonoBehaviour
{
	public List<string> InuseList = new List<string>();
	public List<string> UnuseList = new List<string>();
	public void Update()
	{
		if (!FrameBase.mGameFramework.mEnableScriptDebug)
		{
			return;
		}

		InuseList.Clear();
		var inuse = FrameBase.mArrayPoolThread.getInusedList();
		FrameBase.mArrayPoolThread.lockList();
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
		var unuse = FrameBase.mArrayPoolThread.getUnusedList();
		foreach (var itemTypeList in unuse)
		{
			foreach (var array in itemTypeList.Value)
			{
				if(array.Value.Count > 0)
				{
					UnuseList.Add(StringUtility.strcat(itemTypeList.Key.ToString(), 
									": 长度:", 
									StringUtility.IToS(array.Key), 
									", 个数:", 
									StringUtility.IToS(array.Value.Count)));
				}
			}
		}
		FrameBase.mArrayPoolThread.unlockList();
	}
}