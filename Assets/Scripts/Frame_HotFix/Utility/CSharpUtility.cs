using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using System.IO;
using System.IO.Compression;
#if USE_SEVEN_ZIP
using SevenZip;
#endif
using static UnityUtility;
using static StringUtility;
using static MathUtility;
using static FileUtility;
using static FrameDefine;
using UDebug = UnityEngine.Debug;

// 与C#有关的工具函数
public class CSharpUtility
{
	protected static int mIDMaker;			// 用于生成客户端唯一ID的种子
	public static string getLocalIP()
	{
		foreach (IPAddress item in Dns.GetHostAddresses(Dns.GetHostName()))
		{
			if (item.AddressFamily == AddressFamily.InterNetwork)
			{
				return item.ToString();
			}
		}
		return "";
	}
	public static T createInstance<T>(Type classType, params object[] param) where T : class
	{
		try
		{
			return Activator.CreateInstance(classType, param) as T;
		}
		catch (Exception e)
		{
			logException(e, "create instance error! type:" + classType);
			return null;
		}
	}
	public static T deepCopy<T>(T obj) where T : class
	{
		// 如果是字符串或值类型则直接返回
		if (obj == null || obj is string || obj.GetType().IsValueType)
		{
			return obj;
		}
		object retval = createInstance<object>(obj.GetType());
		foreach (FieldInfo field in obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static))
		{
			field.SetValue(retval, deepCopy(field.GetValue(obj)));
		}
		return (T)retval;
	}
	public static T intToEnum<T, IntT>(IntT value) where T : Enum
	{
		return (T)Enum.ToObject(typeof(T), value);
	}
	public static int enumToInt<T>(T enumValue) where T : Enum
	{
		return Convert.ToInt32(enumValue);
	}
	public static sbyte findMax(Span<sbyte> list)
	{
		int count = list.Length;
		sbyte maxValue = list[0];
		for (int i = 1; i < count; ++i)
		{
			clampMin(ref maxValue, list[i]);
		}
		return maxValue;
	}
	public static sbyte findMax(List<sbyte> list)
	{
		int count = list.Count;
		sbyte maxValue = list[0];
		for (int i = 1; i < count; ++i)
		{
			clampMin(ref maxValue, list[i]);
		}
		return maxValue;
	}
	public static byte findMax(Span<byte> list)
	{
		int count = list.Length;
		byte maxValue = list[0];
		for (int i = 1; i < count; ++i)
		{
			clampMin(ref maxValue, list[i]);
		}
		return maxValue;
	}
	public static byte findMax(List<byte> list)
	{
		int count = list.Count;
		byte maxValue = list[0];
		for (int i = 1; i < count; ++i)
		{
			clampMin(ref maxValue, list[i]);
		}
		return maxValue;
	}
	public static short findMax(Span<short> list)
	{
		int count = list.Length;
		short maxValue = list[0];
		for (int i = 1; i < count; ++i)
		{
			clampMin(ref maxValue, list[i]);
		}
		return maxValue;
	}
	public static short findMax(List<short> list)
	{
		int count = list.Count;
		short maxValue = list[0];
		for (int i = 1; i < count; ++i)
		{
			clampMin(ref maxValue, list[i]);
		}
		return maxValue;
	}
	public static ushort findMax(Span<ushort> list)
	{
		int count = list.Length;
		ushort maxValue = list[0];
		for (int i = 1; i < count; ++i)
		{
			clampMin(ref maxValue, list[i]);
		}
		return maxValue;
	}
	public static ushort findMax(List<ushort> list)
	{
		int count = list.Count;
		ushort maxValue = list[0];
		for (int i = 1; i < count; ++i)
		{
			clampMin(ref maxValue, list[i]);
		}
		return maxValue;
	}
	public static int findMax(Span<int> list)
	{
		int count = list.Length;
		int maxValue = list[0];
		for (int i = 1; i < count; ++i)
		{
			clampMin(ref maxValue, list[i]);
		}
		return maxValue;
	}
	public static int findMax(List<int> list)
	{
		int count = list.Count;
		int maxValue = list[0];
		for (int i = 1; i < count; ++i)
		{
			clampMin(ref maxValue, list[i]);
		}
		return maxValue;
	}
	public static uint findMax(Span<uint> list)
	{
		int count = list.Length;
		uint maxValue = list[0];
		for (int i = 1; i < count; ++i)
		{
			clampMin(ref maxValue, list[i]);
		}
		return maxValue;
	}
	public static uint findMax(List<uint> list)
	{
		int count = list.Count;
		uint maxValue = list[0];
		for (int i = 1; i < count; ++i)
		{
			clampMin(ref maxValue, list[i]);
		}
		return maxValue;
	}
	public static long findMax(Span<long> list)
	{
		int count = list.Length;
		long maxValue = list[0];
		for (int i = 1; i < count; ++i)
		{
			clampMin(ref maxValue, list[i]);
		}
		return maxValue;
	}
	public static long findMax(List<long> list)
	{
		int count = list.Count;
		long maxValue = list[0];
		for (int i = 1; i < count; ++i)
		{
			clampMin(ref maxValue, list[i]);
		}
		return maxValue;
	}
	public static ulong findMax(Span<ulong> list)
	{
		int count = list.Length;
		ulong maxValue = list[0];
		for (int i = 1; i < count; ++i)
		{
			clampMin(ref maxValue, list[i]);
		}
		return maxValue;
	}
	public static ulong findMax(List<ulong> list)
	{
		int count = list.Count;
		ulong maxValue = list[0];
		for (int i = 1; i < count; ++i)
		{
			clampMin(ref maxValue, list[i]);
		}
		return maxValue;
	}
	public static float findMax(Span<float> list)
	{
		int count = list.Length;
		float maxValue = list[0];
		for (int i = 1; i < count; ++i)
		{
			clampMin(ref maxValue, list[i]);
		}
		return maxValue;
	}
	public static float findMax(List<float> list)
	{
		int count = list.Count;
		float maxValue = list[0];
		for (int i = 1; i < count; ++i)
		{
			clampMin(ref maxValue, list[i]);
		}
		return maxValue;
	}
	public static double findMax(Span<double> list)
	{
		int count = list.Length;
		double maxValue = list[0];
		for (int i = 1; i < count; ++i)
		{
			clampMin(ref maxValue, list[i]);
		}
		return maxValue;
	}
	public static double findMax(List<double> list)
	{
		int count = list.Count;
		double maxValue = list[0];
		for (int i = 1; i < count; ++i)
		{
			clampMin(ref maxValue, list[i]);
		}
		return maxValue;
	}
	public static sbyte findMaxAbs(Span<sbyte> list)
	{
		int count = list.Length;
		sbyte maxValue = abs(list[0]);
		for (int i = 1; i < count; ++i)
		{
			clampMin(ref maxValue, abs(list[i]));
		}
		return maxValue;
	}
	public static sbyte findMaxAbs(List<sbyte> list)
	{
		int count = list.Count;
		sbyte maxValue = abs(list[0]);
		for (int i = 1; i < count; ++i)
		{
			clampMin(ref maxValue, abs(list[i]));
		}
		return maxValue;
	}
	public static short findMaxAbs(Span<short> list)
	{
		int count = list.Length;
		short maxValue = abs(list[0]);
		for (int i = 1; i < count; ++i)
		{
			clampMin(ref maxValue, abs(list[i]));
		}
		return maxValue;
	}
	public static short findMaxAbs(List<short> list)
	{
		int count = list.Count;
		short maxValue = abs(list[0]);
		for (int i = 1; i < count; ++i)
		{
			clampMin(ref maxValue, abs(list[i]));
		}
		return maxValue;
	}
	public static int findMaxAbs(Span<int> list)
	{
		int count = list.Length;
		int maxValue = abs(list[0]);
		for (int i = 1; i < count; ++i)
		{
			clampMin(ref maxValue, abs(list[i]));
		}
		return maxValue;
	}
	public static int findMaxAbs(List<int> list)
	{
		int count = list.Count;
		int maxValue = abs(list[0]);
		for (int i = 1; i < count; ++i)
		{
			clampMin(ref maxValue, abs(list[i]));
		}
		return maxValue;
	}
	public static long findMaxAbs(Span<long> list)
	{
		int count = list.Length;
		long maxValue = abs(list[0]);
		for (int i = 1; i < count; ++i)
		{
			clampMin(ref maxValue, abs(list[i]));
		}
		return maxValue;
	}
	public static long findMaxAbs(List<long> list)
	{
		int count = list.Count;
		long maxValue = abs(list[0]);
		for (int i = 1; i < count; ++i)
		{
			clampMin(ref maxValue, abs(list[i]));
		}
		return maxValue;
	}
	public static float findMaxAbs(Span<float> list)
	{
		int count = list.Length;
		float maxValue = abs(list[0]);
		for (int i = 1; i < count; ++i)
		{
			clampMin(ref maxValue, abs(list[i]));
		}
		return maxValue;
	}
	public static float findMaxAbs(List<float> list)
	{
		int count = list.Count;
		float maxValue = abs(list[0]);
		for (int i = 1; i < count; ++i)
		{
			clampMin(ref maxValue, abs(list[i]));
		}
		return maxValue;
	}
	public static double findMaxAbs(Span<double> list)
	{
		int count = list.Length;
		double maxValue = abs(list[0]);
		for (int i = 1; i < count; ++i)
		{
			clampMin(ref maxValue, abs(list[i]));
		}
		return maxValue;
	}
	public static double findMaxAbs(List<double> list)
	{
		int count = list.Count;
		double maxValue = abs(list[0]);
		for (int i = 1; i < count; ++i)
		{
			clampMin(ref maxValue, abs(list[i]));
		}
		return maxValue;
	}
	public static void parseFileList(string content, Dictionary<string, GameFileInfo> list)
	{
		if (content.isEmpty())
		{
			return;
		}
		foreach (string line in splitLine(content))
		{
			var info = GameFileInfo.createInfo(line);
			list.addNotNullKey(info?.mFileName, info);
		}
	}
	public static bool isIgnorePath(string fullPath, List<string> ignorePath)
	{
		foreach (string path in ignorePath.safe())
		{
			if (fullPath.Contains(path))
			{
				return true;
			}
		}
		return false;
	}
	// ensureInterval为true表示保证每次间隔一定不小于interval,false表示保证一定时间内的触发次数,而不保证每次间隔一定小于interval
	public static bool tickTimerLoop(ref float timer, float elapsedTime, float interval, bool ensureInterval = false)
	{
		if (timer < 0.0f)
		{
			return false;
		}
		timer -= elapsedTime;
		if (timer <= 0.0f)
		{
			if (ensureInterval)
			{
				timer = interval;
			}
			else
			{
				timer += interval;
				// 如果加上间隔以后还是小于0,则可能间隔太小了,需要将计时重置到间隔时间,避免计时停止
				if (timer <= 0.0f)
				{
					timer = interval;
				}
			}
			return true;
		}
		return false;
	}
	public static bool tickTimerOnce(ref float timer, float elapsedTime)
	{
		if (timer < 0.0f)
		{
			return false;
		}
		timer -= elapsedTime;
		if (timer <= 0.0f)
		{
			timer = -1.0f;
			return true;
		}
		return false;
	}
	// preFrameCount为1表示返回调用getLineNum的行号
	public static int getLineNum(int preFrameCount = 1)
	{
		return new StackTrace(preFrameCount, true).GetFrame(0).GetFileLineNumber();
	}
	// preFrameCount为1表示返回调用getCurSourceFileName的文件名
	public static string getCurSourceFileName(int preFrameCount = 1)
	{
		return new StackTrace(preFrameCount, true).GetFrame(0).GetFileName();
	}
	// 此处不使用MyStringBuilder,因为打印堆栈时一般都是产生了某些错误,再使用MyStringBuilder可能会引起无限递归
	public static string getStackTrace(int depth = 20)
	{
		++depth;
		StringBuilder fullTrace = new();
		StackTrace trace = new(true);
		for (int i = 0; i < trace.FrameCount; ++i)
		{
			if (i == 0)
			{
				continue;
			}
			if (i >= depth)
			{
				break;
			}
			StackFrame frame = trace.GetFrame(i);
			if (frame.GetFileName().isEmpty())
			{
				break;
			}
			fullTrace.Append("at ");
			fullTrace.Append(frame.GetFileName());
			fullTrace.Append(":");
			fullTrace.AppendLine(IToS(frame.GetFileLineNumber()));
		}
		return fullTrace.ToString();
	}
	public static int makeID()
	{
		if (mIDMaker >= 0x7FFFFFFF)
		{
			logError("ID已超过最大值");
		}
		return ++mIDMaker;
	}
	public static void notifyIDUsed(int id)
	{
		mIDMaker = getMax(mIDMaker, id);
	}
	// 移除数组中的第index个元素,validElementCount是数组中有效的元素个数
	public static void removeElement<T>(T[] array, int validElementCount, int index)
	{
		if (index < 0 || index >= validElementCount)
		{
			return;
		}
		int moveCount = validElementCount - index - 1;
		for (int i = 0; i < moveCount; ++i)
		{
			array[index + i] = array[index + i + 1];
		}
	}
	// 移除数组中的所有value,T为引用类型
	public static int removeClassElement<T>(T[] array, int validElementCount, T value) where T : class
	{
		for (int i = 0; i < validElementCount; ++i)
		{
			if (array[i] == value)
			{
				removeElement(array, validElementCount--, i--);
			}
		}
		return validElementCount;
	}
	// 移除数组中的所有value,T为继承自IEquatable的值类型
	public static int removeValueElement<T>(T[] array, int validElementCount, T value) where T : IEquatable<T>
	{
		for (int i = 0; i < validElementCount; ++i)
		{
			if (array[i].Equals(value))
			{
				removeElement(array, validElementCount--, i--);
			}
		}
		return validElementCount;
	}
	public static bool arrayContains<T>(T[] array, T value, int arrayLen = -1)
	{
		if (array.isEmpty())
		{
			return false;
		}
		if (arrayLen == -1)
		{
			arrayLen = array.Length;
		}
		for (int i = 0; i < arrayLen; ++i)
		{
			if (EqualityComparer<T>.Default.Equals(array[i], value))
			{
				return true;
			}
		}
		return false;
	}
	// 比较两个列表是否完全一致
	public static bool compareList<T>(List<T> list0, List<T> list1)
	{
		if (list0 == null && list1 == null)
		{
			return true;
		}
		if (list0 == null || list1 == null)
		{
			return false;
		}
		int count = list0.Count;
		if (count != list1.Count)
		{
			return false;
		}
		for (int i = 0; i < count; ++i)
		{
			if (!EqualityComparer<T>.Default.Equals(list0[i], list1[i]))
			{
				return false;
			}
		}
		return true;
	}
	// 反转列表顺序
	public static void inverseList<T>(IList<T> list)
	{
		if (list.isEmpty())
		{
			return;
		}
		int count = list.Count;
		int halfCount = list.Count >> 1;
		for (int i = 0; i < halfCount; ++i)
		{
			T temp = list[i];
			list[i] = list[count - 1 - i];
			list[count - 1 - i] = temp;
		}
	}
	public static IPAddress hostNameToIPAddress(string hostName)
	{
		return Dns.GetHostAddresses(hostName).get(0);
	}
	public static bool isEnumValid<T>(T value) where T : Enum
	{
		return typeof(T).IsEnumDefined(value);
	}
	public static void checkEnum<T>(T value) where T : Enum
	{
		if (!typeof(T).IsEnumDefined(value))
		{
			logError(typeof(T) + "枚举不包含值:" + value);
		}
	}
	public static bool launchExe(string dir, string args, int timeoutMilliseconds, bool hidden = false)
	{
		ProcessCreationFlags flags = hidden ? ProcessCreationFlags.CREATE_NO_WINDOW : ProcessCreationFlags.NONE;
		STARTUPINFO startupinfo = new()
		{
			cb = (uint)Marshal.SizeOf<STARTUPINFO>()
		};
		string path = getFilePath(dir);
		PROCESS_INFORMATION processinfo = new();
		bool result = Kernel32.CreateProcessW(null, dir + " " + args, IntPtr.Zero, IntPtr.Zero, false, flags, 
											  IntPtr.Zero, path, ref startupinfo, ref processinfo);
		Kernel32.WaitForSingleObject(processinfo.hProcess, timeoutMilliseconds);
		return result;
	}
	// 以批处理方式运行exe
	public static void executeExeBatch(string workingDir, string fullExePath, string args, int waitMilliSeconds = -1, bool showError = true, bool showInfo = true, StringCallback infoCallback = null)
	{
		using Process process = new();
		ProcessStartInfo startInfo = process.StartInfo;
		startInfo.FileName = fullExePath;
		startInfo.Arguments = args;
		startInfo.WorkingDirectory = workingDir;
		startInfo.CreateNoWindow = true;
		startInfo.UseShellExecute = false;
		startInfo.RedirectStandardOutput = true;
		startInfo.RedirectStandardError = true;
		if (showInfo)
		{
			if (infoCallback != null)
			{
				process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
				{
					if (!e.Data.isEmpty())
					{
						infoCallback(e.Data);
					}
				};
			}
			else
			{
				process.OutputDataReceived += (object sender, DataReceivedEventArgs e) =>
				{
					if (!e.Data.isEmpty())
					{
						log(e.Data);
					}
				};
			}
		}
		if (showError)
		{
			process.ErrorDataReceived += (object sender, DataReceivedEventArgs e) =>
			{
				if (e != null && !e.Data.isEmpty())
				{
					UDebug.LogError(e.Data);
				}
			};
		}
		try
		{
			process.Start();
			process.BeginOutputReadLine();
			process.BeginErrorReadLine();
			process.WaitForExit(waitMilliSeconds);
			process.Close();
		}
		catch (Exception e)
		{
			logError(e.Message);
		}
	}
	// 直接执行命令行,windows下执行cmd,macOS中执行bash
	public static void executeCmd(string[] cmdList, bool showError, bool showInfo, StringCallback infoCallback = null)
	{
		if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			executeExeBatch(null, "cmd.exe", "/c " + stringsToString(cmdList, " & "), -1, showError, showInfo, infoCallback);
		}
		else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
		{
			executeExeBatch(null, "/bin/bash", "-c \"" + stringsToString(cmdList, " ; ") + "\"", -1, showError, showInfo, infoCallback);
		}
	}
	// 在指定仓库中执行git命令
	public static void executeGitCmd(string repoFullPath, string args, bool showError, bool showInfo, StringCallback infoCallback = null)
	{
		log("execute git cmd : " + args);
		executeExeBatch(repoFullPath, "git", args, -1, showError, showInfo, infoCallback);
	}
	// 执行脚本文件,macOS中使用
	public static void executeShell(string args, bool showError, bool showInfo, StringCallback infoCallback = null)
	{
		log("execute shell : " + args);
		executeExeBatch(null, "/bin/sh", args, -1, showError, showInfo, infoCallback);
	}
	// 压缩为zip文件,路径为绝对路径
	public static void compressZipFile(string fileNameWithPath, string zipFileNameWithPath)
	{
		using ZipArchive zip = ZipFile.Open(zipFileNameWithPath, ZipArchiveMode.Create);
		zip.CreateEntryFromFile(fileNameWithPath, getFileNameWithSuffix(fileNameWithPath), CompressionLevel.Optimal);
	}
	// 解压zip文件,路径为绝对路径
	public static void decompressZipFile(string zipFileNameWithPath, string extractPath)
	{
		using ZipArchive zip = ZipFile.OpenRead(zipFileNameWithPath);
		createDir(extractPath);
		foreach (ZipArchiveEntry entry in zip.Entries)
		{
			entry.ExtractToFile(extractPath + "/" + entry.FullName, true);
		}
	}
	// 目前7z只能在windows下使用
#if USE_SEVEN_ZIP && UNITY_STANDALONE_WIN
	// 压缩为7z文件,路径为绝对路径
	public static void compress7ZFile(string fileNameWithPath, string zipFileNameWithPath)
	{
		if (!isFileExist(fileNameWithPath))
		{
			logError("要压缩的文件不存在");
			return;
		}
		SevenZipBase.SetLibraryPath(F_PLUGINS_PATH + (IntPtr.Size == 4 ? "7z.dll" : "7z64.dll"));
		SevenZipCompressor tmp = new();
		tmp.ScanOnlyWritable = true;
		tmp.CompressFiles(zipFileNameWithPath, fileNameWithPath);
	}
	// 解压7z文件,路径为绝对路径
	public static void decompress7ZFile(string zipFileNameWithPath, string extractPath)
	{
		if (!isFileExist(zipFileNameWithPath))
		{
			logError("要解压的文件不存在");
			return;
		}
		SevenZipBase.SetLibraryPath(F_PLUGINS_PATH + (IntPtr.Size == 4 ? "7z.dll" : "7z64.dll"));
		validPath(ref extractPath);
		using SevenZipExtractor tmp = new(zipFileNameWithPath);
		using MemoryStream stream = new();
		for (int i = 0; i < tmp.ArchiveFileData.Count; ++i)
		{
			stream.Position = 0;
			tmp.ExtractFile(tmp.ArchiveFileData[i].Index, stream);
			byte[] bytes = stream.ToArray();
			writeFile(extractPath + tmp.ArchiveFileData[i].FileName, bytes, bytes.Length);
		}
	}
	// 解压7z文件,archiveBytes是压缩包文件的字节数组
	public static void decompress7ZFile(byte[] archiveBytes, int byteCount, string extractPath)
	{
		if (archiveBytes.isEmpty())
		{
			return;
		}
		SevenZipBase.SetLibraryPath(F_PLUGINS_PATH + (IntPtr.Size == 4 ? "7z.dll" : "7z64.dll"));
		validPath(ref extractPath);
		using MemoryStream archiveStream = new(archiveBytes, 0, byteCount);
		using SevenZipExtractor tmp = new(archiveStream);
		using MemoryStream stream = new();
		for (int i = 0; i < tmp.ArchiveFileData.Count; ++i)
		{
			stream.Position = 0;
			tmp.ExtractFile(tmp.ArchiveFileData[i].Index, stream);
			byte[] bytes = stream.ToArray();
			writeFile(extractPath + tmp.ArchiveFileData[i].FileName, bytes, bytes.Length);
		}
	}
	// 解压压缩包中的第一个文件
	public static void decompress7ZFirstFile(byte[] archiveBytes, int byteCount, out byte[] outFileBuffer)
	{
		outFileBuffer = null;
		if (archiveBytes.isEmpty())
		{
			return;
		}
		SevenZipBase.SetLibraryPath(F_PLUGINS_PATH + (IntPtr.Size == 4 ? "7z.dll" : "7z64.dll"));
		using MemoryStream archiveStream = new(archiveBytes, 0, byteCount);
		using SevenZipExtractor tmp = new(archiveStream);
		if (tmp.ArchiveFileData.Count == 0)
		{
			return;
		}
		using MemoryStream stream = new();
		tmp.ExtractFile(tmp.ArchiveFileData[0].Index, stream);
		outFileBuffer = stream.ToArray();
	}
#endif
}