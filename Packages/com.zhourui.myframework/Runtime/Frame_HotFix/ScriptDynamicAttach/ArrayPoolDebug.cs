using System.Collections.Generic;
using UnityEngine;
using static StringUtility;
using static FrameBaseHotFix;

// ArrayPool的调试信息
public class ArrayPoolDebug : MonoBehaviour
{
	public List<string> PersistentInuseList = new();		// 持久使用的列表对象
	public List<string> InuseList = new();					// 单帧使用的列表对象
	public List<string> UnuseList = new();					// 未使用的列表对象
	public void Update()
	{
		if (GameEntryBase.getInstance() == null || !GameEntryBase.getInstance().mFrameworkParam.mEnableScriptDebug)
		{
			return;
		}
		PersistentInuseList.Clear();
		foreach (var itemTypeList in mArrayPool.getPersistentInusedList())
		{
			foreach (var array in itemTypeList.Value)
			{
				PersistentInuseList.addIf(strcat(itemTypeList.Key.ToString(), ": 长度:", array.Key.IToS(), ", 个数:", array.Value.Count.IToS()), array.Value.Count != 0);
			}
		}

		InuseList.Clear();
		foreach (var itemTypeList in mArrayPool.getInusedList())
		{
			foreach (var array in itemTypeList.Value)
			{
				InuseList.addIf(strcat(itemTypeList.Key.ToString(), ": 长度:", array.Key.IToS(), ", 个数:", array.Value.Count.IToS()), array.Value.Count != 0);
			}
		}

		UnuseList.Clear();
		foreach (var itemTypeList in mArrayPool.getUnusedList())
		{
			foreach (var array in itemTypeList.Value)
			{
				UnuseList.addIf(strcat(itemTypeList.Key.ToString(), ": 长度:", array.Key.IToS(), ", 个数:", array.Value.Count.IToS()), array.Value.Count != 0);
			}
		}
	}
}