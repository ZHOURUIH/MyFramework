using System;
using System.Collections.Generic;
using UnityEngine;

public class ListPoolDebug : MonoBehaviour
{
	public List<string> InuseList = new List<string>();
	public List<string> UnuseList = new List<string>();
	public void Update()
	{
		InuseList.Clear();
		var inuse = FrameBase.mListPool.getInusedList();
		foreach(var item in inuse)
		{
			InuseList.Add(item.Key + ":" + item.Value.Count);
		}
		UnuseList.Clear();
		var unuse = FrameBase.mListPool.getUnusedList();
		foreach (var item in unuse)
		{
			UnuseList.Add(item.Key + ":" + item.Value.Count);
		}
	}
	//-------------------------------------------------------------------------------------------------------
}