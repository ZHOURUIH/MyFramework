using System;
using System.Collections.Generic;
using System.IO;
#if VIVO_MINI_GAME
using QGMiniGameCore;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
#endif
using static FrameDefine;
using static UnityUtility;
using UBuilResult = UnityEditor.Build.Reporting.BuildResult;

public class PlatformInternal_Vivo
{
	public bool mPortrait;
	public bool mUseSubpackages = true;
	public static string SDK_PATH = Directory.Exists("Assets/VIVO-GAME-SDK") ? "Assets/VIVO-GAME-SDK/" : "Assets/3rdParty/VIVO-GAME-SDK/";
	protected const string SIGN_DIRECTORY = "BuildSigning/VIVO";
	protected const string CLI_PRELOAD_PATH = "Tools/Build/VivoCliPreload.cjs";
	protected const string MIN_PLATFORM_VERSION = "1104";
	protected const string RESOURCE_PACKAGE = "StreamingAssets";
	protected const long MAX_MAIN_PACKAGE_SIZE = 7 * 1024 * 1024;
	protected const long MAX_SUBPACKAGES_SIZE = 40 * 1024 * 1024;
	protected bool mWebGLBuilt;
#if VIVO_MINI_GAME
	protected QGGameConfig mConfig;
	public bool SetPlayer(bool useWebgl2)
	{
		bool result = QGGameBuild.Instance.SetPlayer(useWebgl2);
		PlayerSettings.defaultInterfaceOrientation = mPortrait ? UnityEditor.UIOrientation.Portrait : UnityEditor.UIOrientation.LandscapeLeft;
		return result;
	}
	public bool BuildWebGL(string srcPath, QGGameConfig config, string buildVersion, bool isTest, string gameNameCN)
	{
		configureGame(config, buildVersion, isTest, gameNameCN);
		QGGameTools.SaveEditorConfigLocal(config);
		BuildReport previousReport = BuildReport.GetLatestReport();
		bool built;
        string previousCores = Environment.GetEnvironmentVariable("BINARYEN_CORES");
        try
        {
            if (UnityEngine.Application.platform == UnityEngine.RuntimePlatform.WindowsEditor)
                Environment.SetEnvironmentVariable("BINARYEN_CORES", "1");
            built = QGGameBuild.Instance.BuildWebGL(srcPath, config);
        }
        finally
        {
            Environment.SetEnvironmentVariable("BINARYEN_CORES", previousCores);
        }
		BuildReport report = BuildReport.GetLatestReport();
		if (!built || report == null || report == previousReport || report.summary.result != UBuilResult.Succeeded)
		{
			throw new InvalidOperationException("vivo WebGL 构建失败，停止转换和 RPK 打包");
		}
		mWebGLBuilt = true;
		return true;
	}
	public void OnZipFile(string zipOutPath, Dictionary<string, string> fileMap)
	{
		((IUnityCompatible)QGGameBuild.Instance).OnZipFile(zipOutPath, fileMap);
	}
#else
	public bool SetPlayer(bool useWebgl2) { return false; }
#endif
	public void getBuildParameters(Dictionary<string, string> parameters, string gameNameCN)
	{
		parameters["游戏名称"] = gameNameCN;
		parameters["打包方式"] = mUseSubpackages ? "分包" : "不分包";
		parameters["签名证书"] = getSignPath("certificate.pem");
		parameters["签名私钥"] = getSignPath("private.pem");
	}
	public bool preBuild(string buildVersion, string outputPath, string folderPreName, bool isTest, string gameNameCN)
	{
#if VIVO_MINI_GAME
		PlayerSettings.defaultInterfaceOrientation = mPortrait ? UnityEditor.UIOrientation.Portrait : UnityEditor.UIOrientation.LandscapeLeft;
		QGEditorWindowNew.OnInitEnv();
		var helper = QGWindowHepler.Instance;
		helper.OnConfigSetting();
		mConfig = helper.GetQGGameConfig();
		if (mConfig == null || mConfig.envConfig == null || mConfig.projectConf == null)
		{
			logError("vivo SDK 配置初始化失败");
			return false;
		}
		configureGame(mConfig, buildVersion, isTest, gameNameCN);
		if (mConfig.envConfig.package.isEmpty() || mConfig.envConfig.name.isEmpty() || QuickGameBuildUtility.getVersionCode(buildVersion) == 0)
		{
			logError("请检查 MiniGameSettings 中的 vivo 包名、游戏名称以及打包版本号（三段数字，后两段小于 1000，版本整数不超过 int.MaxValue）");
			return false;
		}
		if (!File.Exists(mConfig.envConfig.icon))
		{
			logError("vivo 游戏图标不存在:" + mConfig.envConfig.icon);
			return false;
		}
		if (mConfig.useReleaseSign && (!File.Exists(getSignPath("certificate.pem")) || !File.Exists(getSignPath("private.pem"))))
		{
			logError("vivo 正式包需要固定签名，请将 certificate.pem 和 private.pem 放入:" + Path.Combine(F_PROJECT_PATH, SIGN_DIRECTORY));
			return false;
		}
		if (mConfig.useSelfLoading && (mConfig.useSubPkgLoading || string.IsNullOrWhiteSpace(mConfig.envConfig.wasmUrl)))
		{
			logError("vivo 首包加载方式配置错误，请检查 CDN 地址和分包选项");
			return false;
		}
		string exportPath = getExportPath(buildVersion, outputPath, folderPreName);
		// SDK 同步配置时会读取旧导出的 manifest，先清空路径以保留项目参数。
		mConfig.buildSrc = "";
		mConfig.useWebgl2 = true;
		helper.OnConfigSetting();
		mConfig.buildSrc = exportPath;
		QGSettingsHelperInterface.helper.SetBuildSrc(exportPath);
		QGGameTools.SaveEditorConfigLocal(mConfig);
		SpritePackerMode atlasMode = EditorSettings.spritePackerMode;
		QGGameBuild.Instance.SetPlayer(mConfig.useWebgl2);
		EditorSettings.spritePackerMode = atlasMode;
		QuickGameBuildUtility.prepareWebGL(WebGLDebugSymbolMode.External);
		PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.WebGL, MiniGameSettings.get().VivoPackageName);
		return true;
#else
		logError("构建 vivo 快游戏需要启用 VIVO_MINI_GAME 宏");
		return false;
#endif
	}
	public UBuilResult buildInternal(out string outputFullPath
#if VIVO_MINI_GAME
		, IUnityCompatible compatible
#endif
		)
	{
		outputFullPath = "";
#if VIVO_MINI_GAME
		var helper = QGWindowHepler.Instance;
		try
		{
			mWebGLBuilt = false;
			helper.SetUnityCompatible(compatible);
			helper.SetIsBuildRpk(false);
			using (new QuickGamePluginScope(SDK_PATH))
			{
				helper.DoBuildCMD();
			}
			string projectPath = Path.Combine(mConfig.buildSrc, QGGameConfig.vivoWebglDir);
			if (!mWebGLBuilt || !Directory.Exists(projectPath))
			{
				return UBuilResult.Failed;
			}
			string packagePath = Path.Combine(projectPath, "dist");
			var previousPackages = QuickGameBuildUtility.capturePackages(packagePath);
			if (!buildPackage(projectPath) || !QuickGameBuildUtility.checkPackages(packagePath, previousPackages))
			{
				return UBuilResult.Failed;
			}
			outputFullPath = packagePath;
			return UBuilResult.Succeeded;
		}
		catch (Exception e)
		{
			logError("vivo 快游戏打包失败:" + e);
		}
		finally
		{
			helper.SetUnityCompatible(QGGameBuild.Instance);
			helper.SetIsBuildRpk(true);
		}
#else
		logError("构建 vivo 快游戏需要启用 " + FrameMacro.VIVO_MINI_GAME + " 宏");
#endif
		return UBuilResult.Failed;
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected static string getSignPath(string fileName)
	{
		return Path.Combine(F_PROJECT_PATH, SIGN_DIRECTORY, fileName);
	}
	protected string getExportPath(string version, string outputPath, string folderPreName)
	{
		return Path.GetFullPath(Path.Combine(outputPath, folderPreName + "_Vivo_" + version));
	}
#if VIVO_MINI_GAME
	protected void configureGame(QGGameConfig config, string buildVersion, bool isTest, string gameNameCN)
	{
		config.envConfig.package = MiniGameSettings.get().VivoPackageName;
		config.envConfig.name = gameNameCN;
		config.envConfig.icon = Path.Combine(F_PROJECT_PATH, QuickGameBuildUtility.ICON_PATH);
		config.envConfig.versionName = buildVersion;
		config.envConfig.versionCode = QuickGameBuildUtility.getVersionCode(buildVersion).ToString();
		config.envConfig.orientation = PlayerSettings.defaultInterfaceOrientation == UIOrientation.Portrait ? "portrait" : "landscape";
		config.envConfig.minPlatformVersion = MIN_PLATFORM_VERSION;
		config.projectConf.il2CppOptimizeSize = true;
		config.projectConf.profilingFuncs = false;
		config.useReleaseSign = !isTest;
		config.useSelfLoading = false;
		config.useSubPkgLoading = mUseSubpackages;
		config.envConfig.wasmUrl = "";
		config.envConfig.wasmDataUrl = "";
	}
	protected bool buildPackage(string projectPath)
	{
		if (mConfig.useReleaseSign)
		{
			string signPath = Path.Combine(projectPath, "sign", "release");
			Directory.CreateDirectory(signPath);
			File.Copy(getSignPath("certificate.pem"), Path.Combine(signPath, "certificate.pem"), true);
			File.Copy(getSignPath("private.pem"), Path.Combine(signPath, "private.pem"), true);
		}
		string contentRoot = Path.Combine(projectPath, "src");
		QuickGameBuildUtility.copyStreamingAssets(Path.Combine(Path.GetDirectoryName(projectPath), "webgl", "StreamingAssets"), contentRoot);
		foreach (string wasmPath in Directory.GetFiles(contentRoot, "*.wasm.code.unityweb", SearchOption.AllDirectories))
		{
			QuickGameBuildUtility.stripWasmDebugNames(wasmPath);
		}
		if (mUseSubpackages)
		{
			QuickGameBuildUtility.prepareSubpackages(contentRoot, "game.js", new[] { RESOURCE_PACKAGE });
		}
		else
		{
			QuickGameBuildUtility.prepareWholePackage(contentRoot, "game.js", false);
		}
		string fileName = mConfig.envConfig.package + (mConfig.useReleaseSign ? ".signed.rpk" : ".rpk");
		return QuickGameBuildUtility.runNodeCli("@vivo-minigame/cli/bin/cli-service.js", mConfig.useReleaseSign ? "release" : "build", projectPath, "vivo", Path.Combine(F_PROJECT_PATH, CLI_PRELOAD_PATH)) &&
			(mUseSubpackages ? QuickGameBuildUtility.checkSubpackages(Path.Combine(projectPath, "dist", fileName),
				Path.Combine(contentRoot, "manifest.json"), "vivo", MAX_MAIN_PACKAGE_SIZE, MAX_SUBPACKAGES_SIZE) :
				QuickGameBuildUtility.checkUploadPackageBudget(Path.Combine(projectPath, "dist", fileName), MAX_MAIN_PACKAGE_SIZE, "vivo"));
	}
#endif
}
