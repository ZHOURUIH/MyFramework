using System.Collections.Generic;
using UnityEngine;
using static FrameBaseHotFix;
using static StringUtility;

// 线性安全的对象池调试信息
public class ClassPoolThreadDebug : MonoBehaviour
{
	public List<string> TypeList = new();	// 类型信息列表
	public void Update()
	{
		if (mGameFrameworkHotFix == null || !mGameFrameworkHotFix.mParam.mEnableScriptDebug)
		{
			return;
		}

		using (new ThreadLockScope(mClassPoolThread.getLock()))
		{
			TypeList.Clear();
			foreach (var item in mClassPoolThread.getPoolList())
			{
				using var a = new MyStringBuilderScope(out var builder);
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