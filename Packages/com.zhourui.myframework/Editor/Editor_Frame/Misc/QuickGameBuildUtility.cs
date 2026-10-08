using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine.Rendering;
using static UnityUtility;

public class QuickGameBuildUtility
{
	public const string ICON_PATH = "Assets/GameResources/GameIcon/Icon.png";
	private const string SUBPACKAGE_LOADER_MARKER = "// bundled resource loader";
	public static string getNodeExecutable()
	{
		foreach (string entry in getBuildEnvironmentPath().Split(Path.PathSeparator))
		{
			if (string.IsNullOrWhiteSpace(entry))
			{
				continue;
			}
			string directory = Environment.ExpandEnvironmentVariables(entry.Trim().Trim('"'));
			string nodeFile = Path.Combine(directory, Path.DirectorySeparatorChar == '\\' ? "node.exe" : "node");
			if (File.Exists(nodeFile))
			{
				return nodeFile;
			}
		}
		return "";
	}
	public static int getVersionCode(string version)
	{
		if (string.IsNullOrEmpty(version))
		{
			return 0;
		}
		string[] parts = version.Split('.');
		if (parts.Length != 3 ||
			!int.TryParse(parts[0], NumberStyles.None, CultureInfo.InvariantCulture, out int major) ||
			!int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out int minor) ||
			!int.TryParse(parts[2], NumberStyles.None, CultureInfo.InvariantCulture, out int patch) ||
			minor >= 1000 || patch >= 1000)
		{
			return 0;
		}
		long code = major * 1000000L + minor * 1000L + patch + 1;
		return code > int.MaxValue ? 0 : (int)code;
	}
	public static void prepareWebGL(WebGLDebugSymbolMode symbolMode = WebGLDebugSymbolMode.Off)
	{
		EditorUserBuildSettings.development = false;
		EditorUserBuildSettings.allowDebugging = false;
		EditorUserBuildSettings.connectProfiler = false;
		EditorUserBuildSettings.buildWithDeepProfilingSupport = false;
		PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.WebGL, false);
		PlayerSettings.SetGraphicsAPIs(BuildTarget.WebGL, new[] { GraphicsDeviceType.OpenGLES3 });
		PlayerSettings.WebGL.debugSymbolMode = symbolMode;
		PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.ExplicitlyThrownExceptionsOnly;
		PlayerSettings.WebGL.nameFilesAsHashes = false;
		PlayerSettings.SetIl2CppCodeGeneration(NamedBuildTarget.WebGL, Il2CppCodeGeneration.OptimizeSize);
		EditorUserBuildSettings.SetPlatformSettings("WebGL", "CodeOptimization", "DiskSize");
		PlayerSettings.WebGL.emscriptenArgs = Regex.Replace(PlayerSettings.WebGL.emscriptenArgs ?? "", @"(^|\s)--profiling-funcs(?=\s|$)", "$1").Trim();
	}
	public static void stripWasmDebugNames(string filePath)
	{
		byte[] data = File.ReadAllBytes(filePath);
		if (data.Length < 8 || data[0] != 0 || data[1] != 'a' || data[2] != 's' || data[3] != 'm' ||
			data[4] != 1 || data[5] != 0 || data[6] != 0 || data[7] != 0)
		{
			throw new InvalidDataException("WASM 文件格式或版本错误:" + filePath);
		}
		using MemoryStream output = new MemoryStream(data.Length);
		output.Write(data, 0, 8);
		int position = 8;
		int removedSections = 0;
		while (position < data.Length)
		{
			int sectionStart = position;
			byte sectionID = data[position++];
			uint sectionSize = readWasmUnsigned(data, ref position, data.Length);
			if (sectionSize > (uint)(data.Length - position))
			{
				throw new InvalidDataException("WASM 节长度超出文件边界:" + filePath);
			}
			int sectionEnd = position + (int)sectionSize;
			bool removeSection = false;
			if (sectionID == 0)
			{
				uint nameSize = readWasmUnsigned(data, ref position, sectionEnd);
				if (nameSize > (uint)(sectionEnd - position))
				{
					throw new InvalidDataException("WASM 自定义节名称超出节边界:" + filePath);
				}
				removeSection = nameSize == 4 && data[position] == 'n' && data[position + 1] == 'a' &&
					data[position + 2] == 'm' && data[position + 3] == 'e';
			}
			if (removeSection)
			{
				++removedSections;
			}
			else
			{
				output.Write(data, sectionStart, sectionEnd - sectionStart);
			}
			position = sectionEnd;
		}
		if (removedSections > 0)
		{
			File.WriteAllBytes(filePath, output.ToArray());
			log("WASM 已移除 name 调试节:" + filePath + "，减少 " + (data.Length - output.Length) + " bytes");
		}
	}
	public static void copyStreamingAssets(string sourcePath, string contentRoot)
	{
		sourcePath = Path.GetFullPath(sourcePath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
		string targetPath = Path.Combine(Path.GetFullPath(contentRoot), "StreamingAssets");
		if (!Directory.Exists(Path.Combine(sourcePath, "WebGL")))
		{
			throw new DirectoryNotFoundException("内置资源不存在:" + sourcePath);
		}
		if (string.Equals(sourcePath, targetPath, StringComparison.OrdinalIgnoreCase))
		{
			throw new InvalidOperationException("内置资源源目录与目标目录相同:" + targetPath);
		}
		if (Directory.Exists(targetPath))
		{
			Directory.Delete(targetPath, true);
		}
		foreach (string file in Directory.GetFiles(sourcePath, "*", SearchOption.AllDirectories))
		{
			if (file.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
			{
				continue;
			}
			string targetFile = Path.Combine(targetPath, file.Substring(sourcePath.Length + 1));
			Directory.CreateDirectory(Path.GetDirectoryName(targetFile));
			File.Copy(file, targetFile, true);
		}
	}
	public static void prepareSubpackages(string contentRoot, string entryFileName, string[] packageNames, bool markUnityGame = false)
	{
		string manifestPath = Path.Combine(contentRoot, "manifest.json");
		JObject manifest = JObject.Parse(File.ReadAllText(manifestPath));
		JArray packages = manifest["subpackages"] as JArray ?? new JArray();
		foreach (string name in packageNames)
		{
			string root = Path.Combine(contentRoot, name);
			if (!Directory.Exists(root))
			{
				throw new DirectoryNotFoundException("资源分包目录不存在:" + root);
			}
			string entryPath = Path.Combine(root, entryFileName);
			// OPPO 在全局脚本环境中执行分包入口，没有 CommonJS 的 module 对象。
			string entryScript = "void 0;\n";
			string existingScript = File.Exists(entryPath) ? File.ReadAllText(entryPath) : "";
			if (!string.IsNullOrWhiteSpace(existingScript) && existingScript.Trim() != entryScript.Trim() &&
				existingScript.Trim() != "module.exports = {};")
			{
				throw new InvalidOperationException("资源分包已有启动脚本，停止覆盖:" + entryPath);
			}
			File.WriteAllText(entryPath, entryScript, new UTF8Encoding(false));
			JObject config = null;
			foreach (JObject package in packages)
			{
				if (package.Value<string>("name") == name)
				{
					config = package;
					break;
				}
			}
			if (config == null)
			{
				config = new JObject { ["name"] = name };
				packages.Add(config);
			}
			config["root"] = name + "/";
		}
		manifest["subpackages"] = packages;
		File.WriteAllText(manifestPath, manifest.ToString(Formatting.Indented), new UTF8Encoding(false));
		prepareSubpackageLoader(contentRoot, entryFileName, packageNames, markUnityGame);
	}
	public static bool runNodeCli(string cliRelativePath, string arguments, string workingDirectory, string platformName, string preloadPath = "")
	{
		using var process = createBuildProcess(cliRelativePath, arguments, workingDirectory, preloadPath);
		var output = process.StandardOutput.ReadToEndAsync();
		var error = process.StandardError.ReadToEndAsync();
		process.WaitForExit();
		string outputText = output.GetAwaiter().GetResult();
		string errorText = error.GetAwaiter().GetResult();
		if (!string.IsNullOrWhiteSpace(outputText))
		{
			log(outputText);
		}
		if (process.ExitCode != 0)
		{
			logError(platformName + " RPK 打包失败，退出码:" + process.ExitCode + "\n" + outputText + "\n" + errorText);
			return false;
		}
		if (!string.IsNullOrWhiteSpace(errorText))
		{
			log(errorText);
		}
		return true;
	}
	public static Dictionary<string, long> capturePackages(string path)
	{
		Dictionary<string, long> packages = new();
		if (Directory.Exists(path))
		{
			foreach (string file in Directory.GetFiles(path, "*.rpk", SearchOption.AllDirectories))
			{
				packages.Add(file, File.GetLastWriteTimeUtc(file).Ticks);
			}
		}
		return packages;
	}
	public static bool checkPackages(string path, Dictionary<string, long> previousPackages)
	{
		if (Directory.Exists(path))
		{
			foreach (string file in Directory.GetFiles(path, "*.rpk", SearchOption.AllDirectories))
			{
				FileInfo info = new(file);
				if (info.Length > 0 && (!previousPackages.TryGetValue(file, out long time) || info.LastWriteTimeUtc.Ticks != time))
				{
					log("快游戏 RPK 已生成:" + file);
					return true;
				}
			}
		}
		logError("未生成本次构建的 RPK，请检查平台 SDK 和打包工具的错误日志:" + path);
		return false;
	}
	public static bool checkSubpackages(string rpkPath, string manifestPath, string platformName, long maxMainBytes, long maxSubpackagesBytes)
	{
		using var archive = ZipFile.OpenRead(rpkPath);
		ZipArchiveEntry mainPackage = archive.GetEntry("main.rpk");
		if (mainPackage == null)
		{
			logError(platformName + " 未生成分包结构，缺少 main.rpk:" + rpkPath);
			return false;
		}
		JObject manifest = JObject.Parse(File.ReadAllText(manifestPath));
		long subpackagesSize = 0;
		bool valid = true;
		foreach (JObject package in (JArray)manifest["subpackages"])
		{
			string name = package.Value<string>("name");
			ZipArchiveEntry entry = archive.GetEntry(name + ".rpk");
			if (entry == null)
			{
				logError(platformName + " 缺少资源分包:" + name + ".rpk");
				valid = false;
				continue;
			}
			subpackagesSize += entry.Length;
			log(platformName + " 分包 " + name + ":" + (entry.Length / 1024.0 / 1024.0).ToString("F2") + " MiB，" + entry.Length + " bytes");
		}
		log(platformName + " 主包:" + (mainPackage.Length / 1024.0 / 1024.0).ToString("F2") + " MiB，" + mainPackage.Length + " bytes；分包合计:" +
			(subpackagesSize / 1024.0 / 1024.0).ToString("F2") + " MiB，" + subpackagesSize + " bytes");
		if (mainPackage.Length > maxMainBytes)
		{
			logError(platformName + " 主包超过限制:" + mainPackage.Length + " bytes，限制:" + maxMainBytes + " bytes");
			valid = false;
		}
		if (subpackagesSize > maxSubpackagesBytes)
		{
			logError(platformName + " 分包合计超过限制:" + subpackagesSize + " bytes，限制:" + maxSubpackagesBytes + " bytes");
			valid = false;
		}
		return valid;
	}
	public static bool checkUploadPackageBudget(string rpkPath, long maxBytes, string platformName = "OPPO")
	{
		long packageSize = new FileInfo(rpkPath).Length;
		log(platformName + " 最终上传 RPK:" + (packageSize / 1024.0 / 1024.0).ToString("F2") + " MiB，" + packageSize +
			" bytes；总包上限:" + maxBytes + " bytes");
		if (packageSize == 0 || packageSize > maxBytes)
		{
			logError(platformName + " 最终上传 RPK 超过大小限制或文件为空:" + rpkPath);
			return false;
		}
		return true;
	}
	public static void prepareWholePackage(string contentRoot, string entryFileName = "main.js", bool markUnityGame = true)
	{
		string manifestPath = Path.Combine(contentRoot, "manifest.json");
		JObject manifest = JObject.Parse(File.ReadAllText(manifestPath));
		manifest.Remove("subpackages");
		File.WriteAllText(manifestPath, manifest.ToString(), new UTF8Encoding(false));
		string entry = Path.Combine(contentRoot, entryFileName);
		string original = Path.Combine(contentRoot, "sdk-" + entryFileName);
		string script = File.ReadAllText(entry);
		if (script.StartsWith(SUBPACKAGE_LOADER_MARKER, StringComparison.Ordinal))
		{
			script = File.ReadAllText(original);
		}
		if (markUnityGame && !script.StartsWith("qg.setIsUnityGame(true);", StringComparison.Ordinal))
		{
			script = "qg.setIsUnityGame(true);\n" + script;
		}
		File.WriteAllText(entry, script, new UTF8Encoding(false));
		if (File.Exists(original))
		{
			File.Delete(original);
		}
	}
	//---------
	protected static void prepareSubpackageLoader(string contentRoot, string entryFileName, string[] packageNames, bool markUnityGame)
	{
		string entryPath = Path.Combine(contentRoot, entryFileName);
		string originalName = "sdk-" + entryFileName;
		string originalPath = Path.Combine(contentRoot, originalName);
		string entryScript = File.ReadAllText(entryPath);
		if (entryScript.StartsWith("// OPPO bundled resource loader", StringComparison.Ordinal) ||
			entryScript.StartsWith("// vivo bundled resource loader", StringComparison.Ordinal))
		{
			File.Copy(Path.Combine(contentRoot, "archer-" + entryFileName), originalPath, true);
		}
		else if (!entryScript.StartsWith(SUBPACKAGE_LOADER_MARKER, StringComparison.Ordinal))
		{
			File.Copy(entryPath, originalPath, true);
		}
		if (!File.Exists(originalPath))
		{
			throw new FileNotFoundException("原始 SDK 启动脚本不存在", originalPath);
		}
		File.WriteAllText(entryPath, SUBPACKAGE_LOADER_MARKER + "\n" +
			(markUnityGame ? "qg.setIsUnityGame(true);\n" : "") +
			"var packages = " + JsonConvert.SerializeObject(packageNames) + ";\n" +
			"var remaining = packages.length;\n" +
			"var failed = false;\n" +
			"packages.forEach(function (name) {\n" +
			"  var finished = false;\n" +
			"  qg.loadSubpackage({\n" +
			"    name: name,\n" +
			"    success: function () {\n" +
			"      if (finished) { return; }\n" +
			"      finished = true;\n" +
			"      if (--remaining === 0 && !failed) { require(" + JsonConvert.SerializeObject("./" + originalName) + "); }\n" +
			"    },\n" +
			"    fail: function (error) {\n" +
			"      if (finished) { return; }\n" +
			"      finished = true;\n" +
			"      failed = true;\n" +
			"      console.error('内置资源分包加载失败', name, error);\n" +
			"      qg.showToast({ " + (markUnityGame ? "title" : "message") + ": '资源加载失败，请重新启动游戏' });\n" +
			"    }\n" +
			"  });\n" +
			"});\n", new UTF8Encoding(false));
	}
	protected static string getBuildEnvironmentPath()
	{
		string environmentPath = Environment.GetEnvironmentVariable("PATH") ?? "";
		if (Path.DirectorySeparatorChar == '\\')
		{
			environmentPath += ";" + Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.User) + ";" +
				Environment.GetEnvironmentVariable("PATH", EnvironmentVariableTarget.Machine) + ";" +
				Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "npm");
		}
		return environmentPath;
	}
	protected static Process createBuildProcess(string cliRelativePath, string arguments, string workingDirectory, string preloadPath)
	{
		bool windows = Path.DirectorySeparatorChar == '\\';
		string environmentPath = getBuildEnvironmentPath();
		string nodePath = getNodeExecutable();
		string cliPath = "";
		foreach (string entry in (environmentPath ?? "").Split(Path.PathSeparator))
		{
			if (string.IsNullOrWhiteSpace(entry))
			{
				continue;
			}
			string directory = Environment.ExpandEnvironmentVariables(entry.Trim().Trim('"'));
			string cliFile = Path.Combine(directory, "node_modules", cliRelativePath);
			if (!windows && !File.Exists(cliFile))
			{
				cliFile = Path.GetFullPath(Path.Combine(directory, "../lib/node_modules", cliRelativePath));
			}
			if (cliPath.Length == 0 && File.Exists(cliFile))
			{
				cliPath = cliFile;
			}
		}
		if (nodePath.Length == 0 || cliPath.Length == 0)
		{
			throw new InvalidOperationException("未找到 Node.js 或打包工具:" + cliRelativePath + "，请安装对应平台 CLI 并确保 Node 在 PATH 中");
		}
		ProcessStartInfo startInfo = new()
		{
			FileName = nodePath,
			Arguments = (string.IsNullOrEmpty(preloadPath) ? "" : "--require \"" + preloadPath + "\" ") + "\"" + cliPath + "\" " + arguments,
			WorkingDirectory = workingDirectory,
			UseShellExecute = false,
			CreateNoWindow = true,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
			StandardOutputEncoding = Encoding.UTF8,
			StandardErrorEncoding = Encoding.UTF8,
		};
		startInfo.EnvironmentVariables["PATH"] = environmentPath;
		return Process.Start(startInfo);
	}
	private static uint readWasmUnsigned(byte[] data, ref int position, int end)
	{
		uint result = 0;
		for (int i = 0; i < 5; ++i)
		{
			if (position >= end)
			{
				throw new InvalidDataException("WASM LEB128 数据不完整");
			}
			byte value = data[position++];
			if (i == 4 && (value & 0xF0) != 0)
			{
				throw new InvalidDataException("WASM LEB128 超出 uint32 范围");
			}
			result |= (uint)(value & 0x7F) << (i * 7);
			if ((value & 0x80) == 0)
			{
				return result;
			}
		}
		throw new InvalidDataException("WASM LEB128 长度错误");
	}
}
