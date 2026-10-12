using System.IO;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.PackageManager;
using UnityEditor.UnityLinker;

// UPM 包中的 link.xml 通过此构建回调交给 UnityLinker，所有项目共用框架保留规则。
public class FrameLinkerProcessor : IUnityLinkerProcessor
{
	public int callbackOrder
	{
		get
		{
			return 0;
		}
	}
	public string GenerateAdditionalLinkXmlFile(BuildReport report, UnityLinkerBuildPipelineData data)
	{
		string packagePath = PackageInfo.FindForAssembly(typeof(FrameLinkerProcessor).Assembly).resolvedPath;
		return Path.GetFullPath(Path.Combine(packagePath, "Runtime/Frame_Base/link.xml"));
	}
}
