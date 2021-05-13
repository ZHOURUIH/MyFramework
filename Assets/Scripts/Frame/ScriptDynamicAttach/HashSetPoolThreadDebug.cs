using System.Collections.Generic;
using UnityEngine;

public class HashSetPoolThreadDebug : MonoBehaviour
{
	public List<string> InuseList = new List<string>();
	public List<string> UnuseList = new List<string>();
	public void Update()
	{
		if (!FrameBase.mGameFramework.isEnableScriptDebug())
		{
			return;
		}
		FrameBase.mHashSetPoolThread.lockList();
		InuseList.Clear();
		var inuse = FrameBase.mHashSetPoolThread.getInusedList();
		foreach(var item in inuse)
		{
			if (item.Value.Count > 0)
			{
				InuseList.Add(item.Key.ToString() + ", 数量:" + StringUtility.IToS(item.Value.Count));
			}
		}

		UnuseList.Clear();
		var unuse = FrameBase.mHashSetPoolThread.getUnusedList();
		foreach (var item in unuse)
		{
			if(item.Value.Count > 0)
			{
				UnuseList.Add(item.Key.ToString() + ", 数量:" + StringUtility.IToS(item.Value.Count));
			}
		}
		FrameBase.mHashSetPoolThread.unlockList();
	}
}