using UnityEditor.Build.Reporting;
using static EditorCommonUtility;
using static GameEditorDefine;

public class PlatformMacOS : PlatformInfo
{
	protected override BuildResult buildInternal(out string outputFullPath)
	{
		// 打包生成的exe所在的路径,需要加上版本号
		mFolderPreName += "_mac";
		// 打包
		outputFullPath = mOutputPath + mFolderPreName + "_" + mBuildVersion + "/" + GAME_NAME + ".app";
		return buildMacOS(outputFullPath, generateBuildOption(mTestClient));
	}
}