using System;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using static FrameDefine;
using static UnityUtility;

public class MenuQuickGameSigning
{
	[MenuItem("游戏平台/生成正式签名/OPPO")]
	public static void generateOppoSigning()
	{
		generateSigning("OPPO", MiniGameSettings.get().OppoPackageName);
	}
	[MenuItem("游戏平台/生成正式签名/vivo")]
	public static void generateVivoSigning()
	{
		generateSigning("VIVO", MiniGameSettings.get().VivoPackageName);
	}
	//--------------------------------------------------------------------------------------------------------------
	private static void generateSigning(string platform, string packageName)
	{
		string signPath = Path.GetFullPath(Path.Combine(F_PROJECT_PATH, "BuildSigning", platform));
		if (Directory.Exists(signPath) && Directory.GetFileSystemEntries(signPath).Length > 0)
		{
			EditorUtility.DisplayDialog("保留已有签名", "签名目录已有文件，不会重新生成或覆盖。后续版本请复用原证书和私钥；文件不完整时请恢复备份。\n" + signPath, "确定");
			return;
		}
		if (string.IsNullOrWhiteSpace(packageName) || !Regex.IsMatch(packageName, @"^[A-Za-z][A-Za-z0-9_]*(\.[A-Za-z][A-Za-z0-9_]*)+$"))
		{
			logError("请先在平台配置中填写正确的 " + platform + " 包名");
			return;
		}
		string temporaryPath = signPath + ".generating-" + Guid.NewGuid().ToString("N");
		try
		{
			string openssl = findOpenSSL();
			Directory.CreateDirectory(temporaryPath);
			runOpenSSL(openssl, "genrsa -out private.pem 2048", temporaryPath);
			runOpenSSL(openssl, "req -new -x509 -key private.pem -out certificate.pem -sha256 -days 3650 -subj \"/C=CN/CN=" + packageName + "\"", temporaryPath);
			if (Directory.Exists(signPath))
			{
				Directory.Delete(signPath);
			}
			Directory.Move(temporaryPath, signPath);
			log(platform + " 正式签名已生成:" + signPath);
			EditorUtility.DisplayDialog("正式签名已生成", "certificate.pem 和 private.pem 已保存到:\n" + signPath + "\n请备份这两个文件，后续发布沿用同一套签名。", "确定");
		}
		catch (Exception e)
		{
			logError("生成 " + platform + " 正式签名失败:" + e.Message);
		}
		finally
		{
			if (Directory.Exists(temporaryPath))
			{
				File.Delete(Path.Combine(temporaryPath, "private.pem"));
				File.Delete(Path.Combine(temporaryPath, "certificate.pem"));
				Directory.Delete(temporaryPath);
			}
		}
	}
	private static string findOpenSSL()
	{
		foreach (string directory in (Environment.GetEnvironmentVariable("PATH") ?? "").Split(Path.PathSeparator))
		{
			if (string.IsNullOrWhiteSpace(directory))
			{
				continue;
			}
			string path = Path.Combine(directory.Trim().Trim('"'), "openssl.exe");
			if (File.Exists(path))
			{
				return path;
			}
		}
		string programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
		foreach (string folder in new[] { "OpenSSL-Win64", "OpenSSL-Win32" })
		{
			string path = Path.Combine(programFiles, folder, "bin", "openssl.exe");
			if (File.Exists(path))
			{
				return path;
			}
		}
		throw new FileNotFoundException("未找到 OpenSSL，请安装后加入 PATH，或安装到 Program Files/OpenSSL-Win64");
	}
	private static void runOpenSSL(string executable, string arguments, string workingDirectory)
	{
		ProcessStartInfo startInfo = new()
		{
			FileName = executable,
			Arguments = arguments,
			WorkingDirectory = workingDirectory,
			UseShellExecute = false,
			CreateNoWindow = true,
			RedirectStandardOutput = true,
			RedirectStandardError = true,
		};
		using Process process = Process.Start(startInfo);
		var output = process.StandardOutput.ReadToEndAsync();
		var error = process.StandardError.ReadToEndAsync();
		if (!process.WaitForExit(30000))
		{
			process.Kill();
			process.WaitForExit();
			throw new TimeoutException("OpenSSL 生成签名超时");
		}
		string message = error.GetAwaiter().GetResult();
		output.GetAwaiter().GetResult();
		if (process.ExitCode != 0)
		{
			throw new InvalidOperationException("OpenSSL 退出码:" + process.ExitCode + "\n" + message);
		}
	}
}
