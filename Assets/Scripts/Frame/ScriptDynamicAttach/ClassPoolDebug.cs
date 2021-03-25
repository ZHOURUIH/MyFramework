using System.Collections.Generic;
using UnityEngine;

public class ClassPoolDebug : MonoBehaviour
{
	public List<string> PersistentInuseList = new List<string>();
	public List<string> InuseList = new List<string>();
	public List<string> UnuseList = new List<string>();
	public void Update()
	{
		if (!FrameBase.mGameFramework.isEnableScriptDebug())
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