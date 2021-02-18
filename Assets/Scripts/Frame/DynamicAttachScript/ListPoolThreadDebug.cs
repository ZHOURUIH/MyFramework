using System.Collections.Generic;
using UnityEngine;

public class ListPoolThreadDebug : MonoBehaviour
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
		var persistentInuse = FrameBase.mListPool.getPersistentInusedList();
		foreach (var item in persistentInuse)
		{
			PersistentInuseList.Add(item.Key + ", 数量:" + item.Value.Count);
		}

		InuseList.Clear();
		var inuse = FrameBase.mListPool.getInusedList();
		foreach(var item in inuse)
		{
			InuseList.Add(item.Key + ", 数量:" + item.Value.Count);
		}

		UnuseList.Clear();
		var unuse = FrameBase.mListPool.getUnusedList();
		foreach (var item in unuse)
		{
			UnuseList.Add(item.Key + ", 数量:" + item.Value.Count);
		}
	}
	//-------------------------------------------------------------------------------------------------------
}