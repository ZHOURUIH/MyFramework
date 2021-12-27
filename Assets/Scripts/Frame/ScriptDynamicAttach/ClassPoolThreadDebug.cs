using System.Collections.Generic;
using UnityEngine;

// 线性安全的对象池调试信息
public class ClassPoolThreadDebug : MonoBehaviour
{
	public List<string> TypeList = new List<string>();	// 类型信息列表
	public void Update()
	{
		if (!FrameBase.mGameFramework.mEnableScriptDebug)
		{
			return;
		}

		FrameBase.mClassPoolThread.lockList();
		TypeList.Clear();
		var poolList = FrameBase.mClassPoolThread.getPoolList();
		foreach(var item in poolList)
		{
			MyStringBuilder builder = FrameUtility.STRING(item.Key.ToString());
			if(item.Value.getInusedList().Count > 0)
			{
				builder.append(", 已使用:", StringUtility.IToS(item.Value.getInusedList().Count));
			}
			if(item.Value.getUnusedList().Count > 0)
			{
				builder.append(", 未使用:", StringUtility.IToS(item.Value.getUnusedList().Count));
			}
			TypeList.Add(FrameUtility.END_STRING(builder));
		}
		FrameBase.mClassPoolThread.unlockList();
	}
}