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
		if (GameEntry.getInstance() == null || !GameEntry.getInstance().mFramworkParam.mEnableScriptDebug)
		{
			return;
		}

		using (new ThreadLockScope(mClassPoolThread.getLock()))
		{
			TypeList.Clear();
			foreach (var item in mClassPoolThread.getPoolList())
			{
				var list = item.Value;
				using var a = new MyStringBuilderScope(out var builder);
				builder.add(item.Key.ToString());
				builder.addIf(", 已使用:", IToS(list.getInusedList().Count), list.getInusedList().Count > 0);
				builder.addIf(", 未使用:", IToS(list.getUnusedList().Count), list.getUnusedList().Count > 0);
				TypeList.Add(builder.ToString());
			}
		}
	}
}