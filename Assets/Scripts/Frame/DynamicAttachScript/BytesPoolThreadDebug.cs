using System.Collections.Generic;
using UnityEngine;

public class BytesPoolThreadDebug : MonoBehaviour
{
	public List<string> InuseList = new List<string>();
	public List<string> UnuseList = new List<string>();
	public void Update()
	{
		if (!FrameBase.mGameFramework.isEnableScriptDebug())
		{
			return;
		}
		InuseList.Clear();
		var inuse = FrameBase.mBytesPoolThread.getInusedList();
		foreach(var item in inuse)
		{
			InuseList.Add(item.Key + ": " + item.Value.Count + "个");
		}
		UnuseList.Clear();
		var unuse = FrameBase.mBytesPoolThread.getUnusedList();
		foreach (var item in unuse)
		{
			UnuseList.Add(item.Key + ": " + item.Value.Count + "个");
		}
	}
	//-------------------------------------------------------------------------------------------------------
}