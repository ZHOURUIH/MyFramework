using System.Collections.Generic;
using UnityEngine;

public class ClassPoolThreadDebug : MonoBehaviour
{
	public List<string> TypeList = new List<string>();
	public void Update()
	{
		if (!FrameBase.mGameFramework.isEnableScriptDebug())
		{
			return;
		}

		FrameBase.mClassPoolThread.lockList();
		TypeList.Clear();
		var poolList = FrameBase.mClassPoolThread.getPoolList();
		foreach(var item in poolList)
		{
			MyStringBuilder builder = FrameBase.STRING(item.Key.ToString());
			if(item.Value.getInusedList().Count > 0)
			{
				builder.Append(", 已使用:", StringUtility.IToS(item.Value.getInusedList().Count));
			}
			if(item.Value.getUnusedList().Count > 0)
			{
				builder.Append(", 未使用:", StringUtility.IToS(item.Value.getUnusedList().Count));
			}
			TypeList.Add(FrameBase.END_STRING(builder));
		}
		FrameBase.mClassPoolThread.unlockList();
	}
}