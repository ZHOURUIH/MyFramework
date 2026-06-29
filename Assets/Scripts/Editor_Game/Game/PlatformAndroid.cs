using UnityEditor;
using UnityEditor.Build.Reporting;
#if USE_GOOGLE_PLAY_ASSET_DELIVERY
using Google.Android.AppBundle.Editor;
#endif
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
		BuildOptions options = generateBuildOption(mTestClient);
		BuildResult result = BuildResult.Unknown;
		if (mGooglePlay)
		{
#if USE_GOOGLE_PLAY_ASSET_DELIVERY
			// 将所有AssetBundle文件备份到其他地方,然后再打包
			AssetPackConfig config = new();
			// 查找当前的所有AssetBundle,因为在这之前已经将不需要打包的AssetBundle备份到了其他地方
			config.AddAssetsFolder("assetbundles", INSTALL_TIME_TEMP_PATH, AssetPackDeliveryMode.InstallTime);
			result = buildGoogleAAB(outputFullPath, options, config);
#endif
		}
		else
		{
			result = buildAndroid(outputFullPath, options);
		}

#if USE_GOOGLE_PLAY_ASSET_DELIVERY
		// 打包后进行签名,虽然已经设置了签名文件,但是似乎打出来的包还是无法上传GooglePlay,需要再执行一次签名
		// 如果这里执行签名命令失败,则说明上面的打包应该已经有签名了
		string keyStoreFile = F_PROJECT_PATH + "KeyStore/user.keystore";
		string storePass = PlayerSettings.Android.keystorePass;
		string keyPass = PlayerSettings.Android.keyaliasPass;
		string aliasName = PlayerSettings.Android.keyaliasName;
		string outputFile = outputPath + folderPreName + "_" + version + "_signed" + (googlePlay ? ".aab" : ".apk");
		string cmd = "\"C:\\Program Files\\Java\\jdk-11\\bin\\jarsigner.exe\" -verbose -keystore " + keyStoreFile + " -sigalg SHA256withRSA -storepass " +
					storePass + " -keypass " + keyPass + " -digestalg SHA-256 -signedjar " + outputFile + " " + fullPath + " " + aliasName;
		executeCmd(new string[] { cmd }, true, true);
#endif
		return result;
	}
}