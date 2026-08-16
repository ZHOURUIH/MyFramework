using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager.UI;
using UnityEngine;
using PackageInfo = UnityEditor.PackageManager.PackageInfo;

public static class EasyECSMenu
{
	private const string PACKAGE_NAME = "com.zhourui.easyecs";
	private const string SAMPLE_NAME = "Benchmark";
	[MenuItem("EasyECS/Import Benchmark Sample")]
	private static void importBenchmarkSample()
	{
		PackageInfo packageInfo = PackageInfo.FindForAssetPath("Packages/" + PACKAGE_NAME + "/package.json");
		if (packageInfo == null)
		{
			Debug.LogError("[EasyECS] 无法获取PackageInfo:" + PACKAGE_NAME);
			return;
		}
		Sample sample = Sample.FindByPackage(packageInfo.name, packageInfo.version).FirstOrDefault(item => item.displayName == SAMPLE_NAME);
		if (string.IsNullOrEmpty(sample.displayName))
		{
			Debug.LogError("[EasyECS] 未找到测试用例:" + SAMPLE_NAME);
			return;
		}
		if (sample.isImported)
		{
			if (!EditorUtility.DisplayDialog("EasyECS", "Benchmark测试用例已经导入,是否重新导入并覆盖?", "重新导入", "取消"))
			{
				return;
			}
			if (!sample.Import(Sample.ImportOptions.OverridePreviousImports))
			{
				Debug.LogError("[EasyECS] Benchmark测试用例重新导入失败");
				return;
			}
		}
		else
		{
			if (!sample.Import())
			{
				Debug.LogError("[EasyECS] Benchmark测试用例导入失败");
				return;
			}
		}
		AssetDatabase.Refresh();
		Debug.Log("[EasyECS] Benchmark测试用例导入成功:" + sample.importPath);
	}
}