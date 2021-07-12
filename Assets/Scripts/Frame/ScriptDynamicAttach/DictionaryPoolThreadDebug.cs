using System.Collections.Generic;
using UnityEngine;

public class DictionaryPoolThreadDebug : MonoBehaviour
{
	public List<string> InuseList = new List<string>();
	public List<string> UnuseList = new List<string>();
	public void Update()
	{
		if (!FrameBase.mGameFramework.mEnableScriptDebug)
		{
			return;
		}
		FrameBase.mDictionaryPoolThread.lockList();
		InuseList.Clear();
		var inuse = FrameBase.mDictionaryPoolThread.getInusedList();
		foreach(var item in inuse)
		{
			if(item.Value.Count > 0)
			{
				InuseList.Add(item.Key.ToString() + ":" + StringUtility.IToS(item.Value.Count));
			}
		}

		UnuseList.Clear();
		var unuse = FrameBase.mDictionaryPoolThread.getUnusedList();
		foreach (var item in unuse)
		{
			if (item.Value.Count > 0)
			{
				UnuseList.Add(item.Key.ToString() + ":" + StringUtility.IToS(item.Value.Count));
			}
		}
		FrameBase.mDictionaryPoolThread.unlockList();
	}
}