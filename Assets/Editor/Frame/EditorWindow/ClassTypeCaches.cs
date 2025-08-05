using System;
using System.Collections.Generic;
using UnityEditor;

[InitializeOnLoad]
public class ClassTypeCaches
{
	// 缓存所有类名
	protected static HashSet<string> classNames = new();
	static ClassTypeCaches()
	{
		// 初始缓存刷新
		refreshTypeCache();

		// 注册编译完成事件
		AssemblyReloadEvents.afterAssemblyReload += refreshTypeCache;
	}
	public static bool hasClass(string name) { return classNames.Contains(name); }
	// 刷新类型缓存
	protected static void refreshTypeCache()
	{
		classNames.Clear();
		// 使用TypeCache获取所有MonoBehaviour子类
		foreach (Type type in TypeCache.GetTypesDerivedFrom<WindowObjectBase>())
		{
			if (!type.FullName.isEmpty())
			{
				classNames.Add(type.Name);
			}
		}
	}
}