using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using static FileUtility;
using static FrameUtility;
using static EditorCommonUtility;
using static EditorFileUtility;

public class PlatformIOS : PlatformInfo
{
	protected override BuildResult buildInternal(out string outputFullPath)
	{
		// 这里需要设置自己项目的ios参数
		PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.iOS, "com.test.sample");
		PlayerSettings.iOS.appleDeveloperTeamID = "";
		PlayerSettings.iOS.iOSManualProvisioningProfileID = "";
		PlayerSettings.iOS.iOSManualProvisioningProfileType = ProvisioningProfileType.Distribution;

		// 打包,此处只是到处一个xcode工程
		outputFullPath = mOutputPath + mFolderPreName + "_" + mBuildVersion;
		// 删除打包目录中的文件
		return buildIOS(outputFullPath, generateBuildOption(mTestClient));
	}
	protected override void afterBuild(string fullPath)
	{
		base.afterBuild(fullPath);
		displayProgressBar("上传ios", "正在生成并上传ios包", 0, 1);
		validPath(ref fullPath);
		// 将批处理文件拷贝到打包出的目录中
		string shellPath = mOutputPath + "../Shell/";
		string destCommand = fullPath + "/archive.sh";
		copyFile(shellPath + "archive.sh", destCommand);
		copyFile(shellPath + "config.plist", fullPath + "config.plist");
		string folderName = mFolderPreName + "_" + mBuildVersion;
		// 这里需要填写自己的参数,用来调用xcode打包,并上传
		string account = "";
		string password = "";
		executeShell(destCommand + " " + fullPath + " " + folderName + " " + account + " " + password, true, true);
		clearProgress();
	}
}