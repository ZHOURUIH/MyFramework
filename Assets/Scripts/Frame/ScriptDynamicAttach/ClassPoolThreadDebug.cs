using System.Collections.Generic;
using UnityEngine;
using static FrameBase;
using static StringUtility;
using static FrameUtility;

// 线性安全的对象池调试信息
public class ClassPoolThreadDebug : MonoBehaviour
{
	public List<string> TypeList = new List<string>();	// 类型信息列表
	public void Update()
	{
		if (mGameFramework == null || !mGameFramework.mEnableScriptDebug)
		{
			return;
		}

		using (new ThreadLockScope(mClassPoolThread.getLock()))
		{
			TypeList.Clear();
			var poolList = mClassPoolThread.getPoolList();
			foreach (var item in poolList)
			{
				using (new ClassScope<MyStringBuilder>(out var builder))
				{
					builder.append(item.Key.ToString());
					if (item.Value.getInusedList().Count > 0)
					{
						builder.append(", 已使用:", IToS(item.Value.getInusedList().Count));
					}
					if (item.Value.getUnusedList().Count > 0)
					{
						builder.append(", 未使用:", IToS(item.Value.getUnusedList().Count));
					}
					TypeList.Add(builder.ToString());
				}
			}
		}
	}
}