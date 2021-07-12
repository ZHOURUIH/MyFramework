using System.Collections.Generic;
using UnityEngine;

public class ListPoolDebug : MonoBehaviour
{
	public List<string> PersistentInuseList = new List<string>();
	public List<string> InuseList = new List<string>();
	public List<string> UnuseList = new List<string>();
	public void Update()
	{
		if (!FrameBase.mGameFramework.mEnableScriptDebug)
		{
			return;
		}
		PersistentInuseList.Clear();
		var persistentInuse = FrameBase.mListPool.getPersistentInusedList();
		foreach (var item in persistentInuse)
		{
			if (item.Value.Count > 0)
			{
				PersistentInuseList.Add(item.Key.ToString() + ", 数量:" + StringUtility.IToS(item.Value.Count));
			}
		}

		InuseList.Clear();
		var inuse = FrameBase.mListPool.getInusedList();
		foreach(var item in inuse)
		{
			if (item.Value.Count > 0)
			{
				InuseList.Add(item.Key.ToString() + ", 数量:" + StringUtility.IToS(item.Value.Count));
			}
		}

		UnuseList.Clear();
		var unuse = FrameBase.mListPool.getUnusedList();
		foreach (var item in unuse)
		{
			if (item.Value.Count > 0)
			{
				UnuseList.Add(item.Key.ToString() + ", 数量:" + StringUtility.IToS(item.Value.Count));
			}
		}
	}
}