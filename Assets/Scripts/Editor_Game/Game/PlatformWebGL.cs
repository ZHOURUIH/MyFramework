using UnityEditor.Build.Reporting;
using static EditorCommonUtility;

public class PlatformWebGL : PlatformInfo
{
	protected override BuildResult buildInternal(out string outputFullPath)
	{
		// 打包生成的路径,需要加上版本号,然后进行打包
		outputFullPath = mOutputPath + mFolderPreName + "_" + mBuildVersion + "/";
		return buildWebGL(outputFullPath, generateBuildOption(mTestClient));
	}
}