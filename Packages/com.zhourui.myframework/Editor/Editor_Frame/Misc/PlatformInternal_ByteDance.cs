using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Build;
#if BYTE_DANCE
using TTSDK.Tool;
using TTSDK.Tool.API;
#endif
using static UnityUtility;
using UBuilResult = UnityEditor.Build.Reporting.BuildResult;

public class PlatformInternal_ByteDance
{
	public bool mPortrait;
	public bool mCompress = true;
	public bool preBuild()
	{
#if BYTE_DANCE
		string packageName = MiniGameSettings.get().ByteDancePackageName;
		if (packageName.isEmpty() || MiniGameSettings.get().ByteDanceAppID.isEmpty())
		{
			logError("请先在 MiniGameSettings 中填写抖音包名和 AppID");
			return false;
		}
		PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.WebGL, packageName);
		QuickGameBuildUtility.prepareWebGL();
#endif
		return true;
	}
	public UBuilResult buildInternal(string outputFullPath, string cdnURL = "")
	{
#if BYTE_DANCE
		var settings = StarkBuilderSettings.Instance;
		settings.OutputDir = outputFullPath;
		settings.appId = MiniGameSettings.get().ByteDanceAppID;
		settings.profiling = false;
		settings.isWebGL2 = true;
		settings.wasmMemorySize = 512;
		settings.needCompress = mCompress;
		settings.orientation = mPortrait ? StarkBuilderSettings.Orientation.Portrait : StarkBuilderSettings.Orientation.Landscape;
		settings.symbolMode = WebGLDebugSymbolMode.Off;
		settings.buildOptions = BuildOptions.CompressWithLz4HC | BuildOptions.CleanBuildCache;
		settings.urlCacheList = cdnURL.isEmpty() ? System.Array.Empty<string>() : new string[] { cdnURL.removeStart("https://").removeEnd("/") };
		settings.dontCacheFileNames = new string[] { "Version", "FileList", "StreamingAssets.bytes" };
		string result = BuildManager.Build(Framework.Wasm, false).GetAwaiter().GetResult();
		log("build result:" + result);
		return !result.isEmpty() ? UBuilResult.Succeeded : UBuilResult.Failed;
#else
		logError("构建抖音小游戏需要启用 BYTE_DANCE 宏");
		return UBuilResult.Failed;
#endif
	}
}
