using System.Collections.Generic;
using UnityEngine;
using static FrameBaseHotFix;

// 线性安全的对象池调试信息
public class ClassPoolThreadDebug : MonoBehaviour
{
	public List<string> TypeList = new();	// 类型信息列表
	public void Update()
	{
		if (GameEntryBase.getInstance() == null || !GameEntryBase.getInstance().mFrameworkParam.mEnableScriptDebug)
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
				builder.addIf(", 已使用:", list.getInusedList().Count.IToS(), list.getInusedList().Count > 0);
				builder.addIf(", 未使用:", list.getUnusedList().Count.IToS(), list.getUnusedList().Count > 0);
				TypeList.Add(builder.ToString());
			}
		}
	}
}