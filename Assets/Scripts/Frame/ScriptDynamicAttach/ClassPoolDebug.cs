using System.Collections.Generic;
using UnityEngine;

// ClassPool调试信息
public class ClassPoolDebug : MonoBehaviour
{
	public List<string> PersistentInuseList = new List<string>();		// 持久使用的对象列表
	public List<string> InuseList = new List<string>();					// 单帧使用的对象列表
	public List<string> UnuseList = new List<string>();					// 未使用的对象列表
	public void Update()
	{
		if (!FrameBase.mGameFramework.mEnableScriptDebug)
		{
			return;
		}
		PersistentInuseList.Clear();
		var persistentInuse = FrameBase.mClassPool.getPersistentInusedList();
		foreach (var itemTypeList in persistentInuse)
		{
			if(itemTypeList.Value.Count > 0)
			{
				PersistentInuseList.Add(itemTypeList.Key.ToString() + ": 个数:" + StringUtility.IToS(itemTypeList.Value.Count));
			}
		}

		InuseList.Clear();
		var inuse = FrameBase.mClassPool.getInusedList();
		foreach (var itemTypeList in inuse)
		{
			if(itemTypeList.Value.Count > 0)
			{
				InuseList.Add(itemTypeList.Key.ToString() + ": 个数:" + StringUtility.IToS(itemTypeList.Value.Count));
			}
		}

		UnuseList.Clear();
		var unuse = FrameBase.mClassPool.getUnusedList();
		foreach (var itemTypeList in unuse)
		{
			if(itemTypeList.Value.Count > 0)
			{
				UnuseList.Add(itemTypeList.Key.ToString() + ": 个数:" + StringUtility.IToS(itemTypeList.Value.Count));
			}
		}
	}
}