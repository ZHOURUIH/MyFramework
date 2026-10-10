using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
#if OPPO_MINI_GAME
using QGMiniGame;
#endif
using static FrameDefine;
using static UnityUtility;
using static QuickGameBuildUtility;
using UBuilResult = UnityEditor.Build.Reporting.BuildResult;

public class PlatformInternal_Oppo
{
	public bool mPortrait;
	public bool mUseSubpackages;
	protected const string BUILD_PACKAGE = "Build";
	protected const string RESOURCE_PACKAGE = "StreamingAssets";
	protected const long MAX_MAIN_PACKAGE_SIZE = 6 * 1024 * 1024;
	protected const long MAX_SUBPACKAGES_SIZE = 32 * 1024 * 1024;
	protected const long MAX_UPLOAD_PACKAGE_SIZE = 30 * 1000 * 1000;
	public void getBuildParameters(Dictionary<string, string> parameters)
	{
		parameters["打包方式"] = mUseSubpackages ? "分包" : "不分包";
		parameters["SDK 目录"] = MiniGameSettings.get().OppoSDKPath;
		parameters["游戏图标"] = MiniGameSettings.get().QuickGameIconPath;
		parameters["签名证书"] = getSignPath("certificate.pem");
		parameters["签名私钥"] = getSignPath("private.pem");
	}
	public bool preBuild(string buildVersion, bool isTest, string outputPath, string folderPreName)
	{
#if OPPO_MINI_GAME
		MiniGameSettings miniGameSettings = MiniGameSettings.get();
		int versionCode = getVersionCode(buildVersion);
		if (miniGameSettings.OppoPackageName.isEmpty() || versionCode == 0)
		{
			logError("请检查 MiniGameSettings 中的 OPPO 包名、游戏名称和三段数字版本号；后两段必须小于 1000，版本整数不能超过 int.MaxValue");
			return false;
		}
		string sdkPath = QuickGamePluginScope.normalizeSDKPath(miniGameSettings.OppoSDKPath);
		if (!sdkPath.StartsWith("Assets/", StringComparison.Ordinal) || !Directory.Exists(Path.Combine(F_PROJECT_PATH, sdkPath)))
		{
			logError("请在 MiniGameSettings 中配置有效的 OPPO SDK 目录:" + miniGameSettings.OppoSDKPath);
			return false;
		}
		var config = BuildConfigAsset.Fundamentals;
		config.packageName = miniGameSettings.OppoPackageName;
		config.projectName = FrameSettings.getGameNameCN();
		config.iconPath = string.IsNullOrWhiteSpace(miniGameSettings.QuickGameIconPath) ? "" :
			Path.Combine(F_PROJECT_PATH, miniGameSettings.QuickGameIconPath);
		config.exportPath = getExportPath(buildVersion, outputPath, folderPreName);
		config.projectVersionName = buildVersion;
		config.projectVersion = versionCode;
		config.minPlatformVersion = BuildFundamentalConfig.MIN_PLATFORM_VERSION;
		config.orientation = (int)(PlayerSettings.defaultInterfaceOrientation == UIOrientation.Portrait ?
			GlobalDefines.Orientation.Portrait : GlobalDefines.Orientation.Landscape);
		config.useRemoteStreamingAssets = false;
		config.streamingAssetsURL = "";
		config.useCustomSign = !isTest;
		config.signCertificate = config.useCustomSign ? getSignPath("certificate.pem") : "";
		config.signPrivate = config.useCustomSign ? getSignPath("private.pem") : "";
		BuildConfigAsset.AssetCache.available = false;
		var settings = BuildConfigAsset.OtherSettingsConfig;
		settings.autoInstall = false;
		settings.autoInstallAvailable = false;
		settings.environmentVariablePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "npm");
		BuildConfigAsset.Save();
		if (!File.Exists(config.iconPath))
		{
			logError("OPPO 游戏图标不存在:" + config.iconPath);
			return false;
		}
		if (config.useCustomSign && (!File.Exists(config.signCertificate) || !File.Exists(config.signPrivate)))
		{
			logError("OPPO 正式包需要固定签名，请检查 MiniGameSettings.OppoSignDirectory:" + miniGameSettings.OppoSignDirectory);
			return false;
		}
		try
		{
			if (string.IsNullOrWhiteSpace(ShellHelper.ExecuteCommand("quickgame -V")))
			{
				logError("OPPO 打包工具未返回版本号，请先执行 npm install -g @oppo-minigame/cli");
				return false;
			}
		}
		catch (Exception e)
		{
			logError("OPPO 打包工具不可用，请先执行 npm install -g @oppo-minigame/cli，并确保 Node 在 PATH 中:\n" + e.Message);
			return false;
		}
		Directory.CreateDirectory(config.exportPath);
		SpritePackerMode atlasMode = EditorSettings.spritePackerMode;
		QGGameTools.SetPlayer();
		PlayerSettings.defaultInterfaceOrientation = mPortrait ? UnityEditor.UIOrientation.Portrait : UnityEditor.UIOrientation.LandscapeLeft;
		EditorSettings.spritePackerMode = atlasMode;
		prepareWebGL();
		// 在 HybridCLR/Generate/All 前设置，补充元数据与最终程序必须使用相同裁剪规则。
		PlayerSettings.stripEngineCode = true;
		// Unity 的 DiskSize 默认使用 -Os；最终链接使用 -Oz 进一步减少 WASM 体积。
		PlayerSettings.WebGL.emscriptenArgs = Regex.Replace(PlayerSettings.WebGL.emscriptenArgs ?? "",
			@"(^|\s)-O(?:[0-3sgz])(?=\s|$)", "$1").Trim() + " -Oz";
		PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.WebGL, miniGameSettings.OppoPackageName);
		return true;
#else
		logError("构建 OPPO 快游戏需要启用 " + FrameMacro.OPPO_MINI_GAME+ " 宏");
		return false;
#endif
	}
	public UBuilResult buildInternal(out string outputFullPath)
	{
		outputFullPath = "";
#if OPPO_MINI_GAME
		try
		{
			string exportPath = BuildConfigAsset.Fundamentals.exportPath;
			string packagePath = Path.Combine(exportPath, "quickgame");
			bool built;
			using (new QuickGamePluginScope(MiniGameSettings.get().OppoSDKPath))
			{
				built = QGGameTools.BuildGame();
			}
			if (!built)
			{
				return UBuilResult.Failed;
			}
			stripWasmDebugNames(Path.Combine(packagePath, BUILD_PACKAGE, "webgl.wasm"));
			if (mUseSubpackages)
			{
				prepareSubpackages(packagePath, "main.js", new[] { BUILD_PACKAGE, RESOURCE_PACKAGE }, true);
			}
			else
			{
				prepareWholePackage(packagePath);
			}
			if (!prepare(packagePath))
			{
				return UBuilResult.Failed;
			}
			var previousPackages = capturePackages(packagePath);
			var config = BuildConfigAsset.Fundamentals;
			string fileName = config.packageName + (config.useCustomSign ? ".signed.rpk" : ".rpk");
			if (!runNodeCli("@oppo-minigame/cli/lib/bin/quickgame", config.useCustomSign ? "pack release" : "pack", packagePath, "OPPO") ||
				!checkPackages(packagePath, previousPackages) ||
				(mUseSubpackages && !checkSubpackages(Path.Combine(packagePath, "dist", fileName),
					Path.Combine(packagePath, "manifest.json"), "OPPO", MAX_MAIN_PACKAGE_SIZE, MAX_SUBPACKAGES_SIZE)) ||
				!checkUploadPackageBudget(Path.Combine(packagePath, "dist", fileName), MAX_UPLOAD_PACKAGE_SIZE))
			{
				return UBuilResult.Failed;
			}
			outputFullPath = packagePath;
			return UBuilResult.Succeeded;
		}
		catch (Exception e)
		{
			logError("OPPO 快游戏打包失败:" + e);
		}
#else
		logError("构建 OPPO 快游戏需要启用 OPPO_MINI_GAME 宏");
#endif
		return UBuilResult.Failed;
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected string getExportPath(string version, string outputPath, string folderPreName)
	{
		return Path.GetFullPath(Path.Combine(outputPath, folderPreName + "_OPPO_" + version));
	}
	protected static string getSignPath(string fileName)
	{
		string directory = MiniGameSettings.get().OppoSignDirectory;
		return string.IsNullOrWhiteSpace(directory) ? "" : Path.Combine(F_PROJECT_PATH, directory, fileName);
	}
	protected static bool prepare(string packagePath)
	{
		string nodePath = getNodeExecutable();
		if (string.IsNullOrEmpty(nodePath))
		{
			throw new FileNotFoundException("OPPO 运行时适配需要 Node.js，请将 Node 加入 PATH");
		}
		string scriptPath = Path.GetFullPath(Path.Combine(Application.dataPath, "../Tools/Build/PrepareOppoRuntime.cjs"));
		ProcessStartInfo startInfo = new()
		{
			FileName = nodePath,
			Arguments = "\"" + scriptPath + "\" \"" + Path.GetFullPath(packagePath).TrimEnd('\\', '/') + "\"",
			UseShellExecute = false,
			CreateNoWindow = true,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			StandardOutputEncoding = Encoding.UTF8,
			StandardErrorEncoding = Encoding.UTF8,
		};
		using Process process = Process.Start(startInfo);
		var output = process.StandardOutput.ReadToEndAsync();
		var error = process.StandardError.ReadToEndAsync();
		process.WaitForExit();
		string result = output.GetAwaiter().GetResult();
		string failure = error.GetAwaiter().GetResult();
		if (process.ExitCode != 0)
		{
			UnityEngine.Debug.LogError("OPPO 运行时适配失败:\n" + result + "\n" + failure);
			return false;
		}
		UnityEngine.Debug.Log(result);
		return true;
	}
}
