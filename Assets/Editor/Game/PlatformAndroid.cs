using UnityEditor;
using UnityEditor.Build.Reporting;
using static EditorCommonUtility;
using static UnityUtility;

public class PlatformAndroid : PlatformInfo
{
	protected override BuildResult buildInternal(out string outputFullPath)
	{
		log("已自动填充Android密码");
		// 这里需要设置自己的签名密码
		PlayerSettings.Android.keystorePass = "";
		PlayerSettings.Android.keyaliasPass = "";

		// 打包
		outputFullPath = mOutputPath + mFolderPreName + "_" + mBuildVersion;
		if (!mExportAndroidProject)
		{
			outputFullPath += ".apk";
		}
		return buildAndroid(outputFullPath, generateBuildOption(mTestClient));
	}
}