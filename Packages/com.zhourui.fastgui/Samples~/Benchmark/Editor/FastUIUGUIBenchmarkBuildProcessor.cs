using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

public class FastUIUGUIBenchmarkBuildProcessor : IPreprocessBuildWithReport
{
	public int callbackOrder { get { return -10000; } }
	public void OnPreprocessBuild(BuildReport report)
	{
		if (report.summary.platform != BuildTarget.StandaloneWindows && report.summary.platform != BuildTarget.StandaloneWindows64)
		{
			return;
		}
		if (!PlayerSettings.enableFrameTimingStats)
		{
			PlayerSettings.enableFrameTimingStats = true;
		}
		if (!PlayerSettings.enableFrameTimingStats)
		{
			throw new BuildFailedException("[FastGUI Benchmark] 无法启用Frame Timing Stats，停止Windows Benchmark构建.");
		}
		Debug.Log("[FastGUI Benchmark Build] Windows Build预处理完成 | FrameTimingStats=True | 支持普通Build/Build And Run");
	}
}
