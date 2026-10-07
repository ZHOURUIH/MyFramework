using System;
using System.Collections.Generic;
using UnityEditor;

// 两个 SDK 的 QG* JS 导出同名但参数不同；原生插件不能只依赖 C# 宏约束。
public class QuickGamePluginScope : IDisposable
{
	private readonly List<(PluginImporter importer, bool compatible)> mChanged = new();
	public QuickGamePluginScope(string sdkDirectory)
	{
		if (sdkDirectory != PlatformInternal_Oppo.SDK_PATH && sdkDirectory != PlatformInternal_Vivo.SDK_PATH)
		{
			throw new ArgumentException("未知快游戏 SDK 目录:" + sdkDirectory);
		}
		try
		{
			foreach (PluginImporter importer in PluginImporter.GetAllImporters())
			{
				string path = importer.assetPath.Replace('\\', '/');
				if (!path.EndsWith(".jslib", StringComparison.OrdinalIgnoreCase) ||
					(!path.StartsWith(PlatformInternal_Oppo.SDK_PATH, StringComparison.Ordinal) &&
					 !path.StartsWith(PlatformInternal_Vivo.SDK_PATH, StringComparison.Ordinal)))
				{
					continue;
				}
				bool previous = importer.GetCompatibleWithPlatform(BuildTarget.WebGL);
				// 保留当前 SDK 自己的 WebGL1/2、色彩空间等插件选择，只禁用另一 SDK。
				bool selected = path.StartsWith(sdkDirectory, StringComparison.Ordinal) && previous;
				if (previous == selected)
				{
					continue;
				}
				mChanged.Add((importer, previous));
				importer.SetCompatibleWithPlatform(BuildTarget.WebGL, selected);
				importer.SaveAndReimport();
			}
		}
		catch
		{
			Dispose();
			throw;
		}
	}
	public void Dispose()
	{
		foreach (var item in mChanged)
		{
			item.importer.SetCompatibleWithPlatform(BuildTarget.WebGL, item.compatible);
			item.importer.SaveAndReimport();
		}
		mChanged.Clear();
	}
}
