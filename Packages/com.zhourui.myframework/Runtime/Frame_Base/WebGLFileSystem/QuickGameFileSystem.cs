using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using static FrameBaseDefine;
using System.Text;
#if OPPO_MINI_GAME || VIVO_MINI_GAME
using QGMiniGame;
#endif
public class QuickGameFileSystem
{
	public static bool isDirectoryExist(string dirPath)
	{
#if OPPO_MINI_GAME
		if (!dirPath.StartsWith(F_PERSISTENT_DATA_PATH, StringComparison.Ordinal))
		{
			return isPackageDirectoryExist(dirPath);
		}
		string path = getNativePath(dirPath);
		return path == "/" || QG.StatSync(path, false)?.isDirectory == true;
#elif VIVO_MINI_GAME
		if (!dirPath.StartsWith(F_PERSISTENT_DATA_PATH, StringComparison.Ordinal))
		{
			return isPackageDirectoryExist(dirPath);
		}
		QGStat[] stat = QG.GetFileSystemManager().StatSync(getNativePath(dirPath), false);
		return stat != null && stat.Length == 1 && stat[0].stats != null && (stat[0].stats.mode & 0xF000) == 0x4000;
#else
		return false;
#endif
	}
	public static bool isFileExist(string filePath)
	{
#if OPPO_MINI_GAME
		if (!filePath.StartsWith(F_PERSISTENT_DATA_PATH, StringComparison.Ordinal))
		{
			return isPackageFileExist(filePath);
		}
		return QG.AccessSync(getNativePath(filePath));
#elif VIVO_MINI_GAME
		if (!filePath.StartsWith(F_PERSISTENT_DATA_PATH, StringComparison.Ordinal))
		{
			return isPackageFileExist(filePath);
		}
		return QG.GetFileSystemManager().AccessSync(getNativePath(filePath)) == "access:ok";
#else
		return false;
#endif
	}
	public static void createDirectory(string dirPath)
	{
		if (isDirectoryExist(dirPath))
		{
			return;
		}
#if OPPO_MINI_GAME
		if (!QG.MkdirSync(getNativePath(dirPath), true))
		{
			throw new IOException("OPPO directory creation failed: " + dirPath);
		}
#elif VIVO_MINI_GAME
		string result = QG.GetFileSystemManager().MkdirSync(getNativePath(dirPath), true);
		if (result != "mkdir:ok")
		{
			throw new IOException("vivo directory creation failed: " + dirPath + ", " + result);
		}
#endif
	}
	public static void deleteDirectory(string dirPath)
	{
		if (!isDirectoryExist(dirPath))
		{
			return;
		}
#if OPPO_MINI_GAME
		if (!QG.RmdirSync(getNativePath(dirPath), true))
		{
			throw new IOException("OPPO directory removal failed: " + dirPath);
		}
#elif VIVO_MINI_GAME
		string result = QG.GetFileSystemManager().RmdirSync(getNativePath(dirPath), true);
		if (result != "rmdir:ok")
		{
			throw new IOException("vivo directory removal failed: " + dirPath + ", " + result);
		}
#endif
	}
	public static void deleteFile(string filePath)
	{
		if (!isFileExist(filePath))
		{
			return;
		}
#if OPPO_MINI_GAME
		if (!QG.UnlinkSync(getNativePath(filePath)))
		{
			throw new IOException("OPPO file removal failed: " + filePath);
		}
#elif VIVO_MINI_GAME
		string result = QG.GetFileSystemManager().UnlinkSync(getNativePath(filePath));
		if (result != "unlink:ok")
		{
			throw new IOException("vivo file removal failed: " + filePath + ", " + result);
		}
#endif
	}
	public static void writeText(string filePath, string fileContent)
	{
		writeBytes(filePath, Encoding.UTF8.GetBytes(fileContent));
	}
	public static long getFileSize(string filePath)
	{
		if (!isFileExist(filePath))
		{
			return -1;
		}
#if (OPPO_MINI_GAME || VIVO_MINI_GAME) && UNITY_WEBGL && !UNITY_EDITOR
		if (!filePath.StartsWith(F_PERSISTENT_DATA_PATH, StringComparison.Ordinal))
		{
			return QuickGamePackageSize(getPackagePath(filePath));
		}
#endif
#if OPPO_MINI_GAME
		StatResponse stat = QG.StatSync(getNativePath(filePath), false);
		return stat != null && stat.isFile ? stat.size : -1;
#elif VIVO_MINI_GAME
		QGStat[] stat = QG.GetFileSystemManager().StatSync(getNativePath(filePath), false);
		return stat != null && stat.Length == 1 && stat[0].stats != null ? stat[0].stats.size : -1;
#else
		return -1;
#endif
	}
	public static bool renameFile(string sourceFile, string destFile)
	{
		if (!isFileExist(sourceFile) || isFileExist(destFile))
		{
			return false;
		}
#if OPPO_MINI_GAME
		if (!QG.RenameSync(getNativePath(sourceFile), getNativePath(destFile)))
		{
			return false;
		}
#elif VIVO_MINI_GAME
		// The vivo SDK returns void and logs native errors without throwing.
		QG.GetFileSystemManager().RenameSync(getNativePath(sourceFile), getNativePath(destFile));
#endif
		return !isFileExist(sourceFile) && isFileExist(destFile);
	}
	public static void copyFile(string sourceFilePath, string destFilePath, bool overwrite)
	{
		if (!overwrite && isFileExist(destFilePath))
		{
			throw new IOException("File already exists: " + destFilePath);
		}
		createDirectory(destFilePath.Substring(0, destFilePath.LastIndexOf('/') + 1));
		if (sourceFilePath.StartsWith(F_PERSISTENT_DATA_PATH, StringComparison.Ordinal))
		{
#if OPPO_MINI_GAME
			if (!QG.CopyFileSync(getNativePath(sourceFilePath), getNativePath(destFilePath)))
			{
				throw new IOException("OPPO file copy failed: " + sourceFilePath);
			}
			return;
#elif VIVO_MINI_GAME
			string result = QG.GetFileSystemManager().CopyFileSync(getNativePath(sourceFilePath), getNativePath(destFilePath));
			if (result != "copyFile:ok")
			{
				throw new IOException("vivo file copy failed: " + sourceFilePath + ", " + result);
			}
			return;
#endif
		}
		byte[] bytes = readBytes(sourceFilePath);
		if (bytes == null)
		{
			throw new FileNotFoundException("File not found", sourceFilePath);
		}
		writeBytes(destFilePath, bytes);
	}
	public static string[] readDirectory(string dirPath)
	{
#if (OPPO_MINI_GAME || VIVO_MINI_GAME) && UNITY_WEBGL && !UNITY_EDITOR
		if (!dirPath.StartsWith(F_PERSISTENT_DATA_PATH, StringComparison.Ordinal))
		{
			return QuickGamePackageDirectory(getPackagePath(dirPath)).Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
		}
#endif
#if OPPO_MINI_GAME
		return QG.ReadDirSync(getNativePath(dirPath))?.files ?? Array.Empty<string>();
#elif VIVO_MINI_GAME
		return QG.GetFileSystemManager().ReaddirSync(getNativePath(dirPath)) ?? Array.Empty<string>();
#else
		return Array.Empty<string>();
#endif
	}
	public static void findFiles(string path, List<string> files, List<string> patterns, List<string> excludePatterns, bool recursive)
	{
		path = path.TrimEnd('/') + "/";
		foreach (string name in readDirectory(path))
		{
			string child = path + name;
			if (isDirectoryExist(child + "/"))
			{
				if (recursive)
				{
					findFiles(child, files, patterns, excludePatterns, true);
				}
			}
			else if ((patterns == null || patterns.Count == 0 || matches(name, patterns)) && !matches(name, excludePatterns))
			{
				files.Add(child);
			}
		}
	}
	public static void findFolders(string path, List<string> folders, List<string> excludeNames, bool recursive)
	{
		path = path.TrimEnd('/') + "/";
		foreach (string name in readDirectory(path))
		{
			string child = path + name;
			if (!isDirectoryExist(child + "/") || excludeNames != null && excludeNames.Contains(name))
			{
				continue;
			}
			folders.Add(child);
			if (recursive)
			{
				findFolders(child, folders, excludeNames, true);
			}
		}
	}
	public static void writeBytes(string filePath, byte[] byteArray, bool append = false)
	{
		if (append && isFileExist(filePath))
		{
			byte[] previous = readBytes(filePath);
			if (previous == null)
			{
				throw new IOException("Cannot append to unreadable file: " + filePath);
			}
			byte[] combined = new byte[previous.Length + byteArray.Length];
			Buffer.BlockCopy(previous, 0, combined, 0, previous.Length);
			Buffer.BlockCopy(byteArray, 0, combined, previous.Length, byteArray.Length);
			byteArray = combined;
		}
		createDirectory(filePath.Substring(0, filePath.LastIndexOf('/') + 1));
#if OPPO_MINI_GAME
		if (!QG.WriteFileSync(getNativePath(filePath), byteArray, false))
		{
			throw new IOException("OPPO file write failed: " + filePath);
		}
#elif VIVO_MINI_GAME
		string result = QG.GetFileSystemManager().WriteFileSync(getNativePath(filePath), byteArray, "binary");
		if (result != "ok")
		{
			throw new IOException("vivo file write failed: " + filePath + ", " + result);
		}
#endif
	}
	public static byte[] readBytes(string filePath)
	{
		if (!isFileExist(filePath))
		{
			return null;
		}
#if OPPO_MINI_GAME || VIVO_MINI_GAME
		if (!filePath.StartsWith(F_PERSISTENT_DATA_PATH, StringComparison.Ordinal))
		{
			return readPackageBytes(filePath);
		}
#endif
#if OPPO_MINI_GAME
		// The SDK retains its last synchronous response; clear it before a new read.
		QGMiniGameManager.readFileSyncResponse = new ReadFileResponse();
		return QG.ReadFileSync(getNativePath(filePath), "binary")?.dataBytes;
#elif VIVO_MINI_GAME
		string path = getNativePath(filePath);
		byte[] content = QG.GetFileSystemManager().ReadFileSync(path, null, null);
		if (content == null)
		{
			// The vivo SDK returns null for an existing empty file as well as for a failed read.
			QGStat[] stat = QG.GetFileSystemManager().StatSync(path, false);
			if (stat != null && stat.Length == 1 && stat[0].stats != null && stat[0].stats.size == 0)
			{
				return Array.Empty<byte>();
			}
		}
		return content;
#else
		return null;
#endif
	}
	public static IEnumerator readBytesAsync(string filePath, BytesCallback callback)
	{
		if (!isFileExist(filePath))
		{
			callback?.Invoke(null);
			yield break;
		}
		if (!filePath.StartsWith(F_PERSISTENT_DATA_PATH, StringComparison.Ordinal))
		{
			callback?.Invoke(readBytes(filePath));
			yield break;
		}
		bool isReading = true;
		byte[] content = null;
		DateTime started = DateTime.UtcNow;
#if OPPO_MINI_GAME
		try
		{
			QG.ReadFile(getNativePath(filePath), "binary", (ReadFileResponse response) =>
			{
				content = response.dataBytes;
				isReading = false;
			}, (_) =>
			{
				isReading = false;
			});
		}
		catch (Exception e)
		{
			FrameBaseUtility.logWarningBase(e.Message);
			isReading = false;
		}
#elif VIVO_MINI_GAME
		try
		{
			QG.GetFileSystemManager().ReadFile(new ReadFileParam
			{
				filePath = getNativePath(filePath),
				success = (response) =>
				{
					content = response.binData ?? (getFileSize(filePath) == 0 ? Array.Empty<byte>() : null);
					isReading = false;
				},
				fail = (_) =>
				{
					isReading = false;
				}
			});
		}
		catch (Exception e)
		{
			FrameBaseUtility.logWarningBase(e.Message);
			isReading = false;
		}
#else
		isReading = false;
#endif
		while (isReading && (DateTime.UtcNow - started).TotalSeconds < 30.0)
		{
			yield return null;
		}
		callback?.Invoke(content);
	}
	//--------------------------------------------------------------------------------------------------------------
	private static bool matches(string name, List<string> patterns)
	{
		if (patterns != null)
		{
			foreach (string pattern in patterns)
			{
				if (name.EndsWith(pattern, StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}
		}
		return false;
	}
	private static string getNativePath(string filePath)
	{
		string root = F_PERSISTENT_DATA_PATH;
		if (string.IsNullOrEmpty(filePath) || !filePath.StartsWith(root, StringComparison.Ordinal))
		{
			throw new IOException("Quick game file path must be inside USER_DATA_PATH: " + filePath);
		}
#if OPPO_MINI_GAME
		return "/" + filePath.Substring(root.Length).TrimEnd('/');
#else
		return filePath.TrimEnd('/');
#endif
	}
	private static string getPackagePath(string filePath)
	{
		if (!filePath.StartsWith(F_STREAMING_ASSETS_PATH, StringComparison.Ordinal))
		{
			throw new IOException("Unsupported quick game file path: " + filePath);
		}
		return "StreamingAssets/" + filePath.Substring(F_STREAMING_ASSETS_PATH.Length);
	}
	private static bool isPackageDirectoryExist(string dirPath)
	{
#if (OPPO_MINI_GAME || VIVO_MINI_GAME) && UNITY_WEBGL && !UNITY_EDITOR
		return QuickGamePackageIsDirectory(getPackagePath(dirPath)) != 0;
#else
		return false;
#endif
	}
	private static bool isPackageFileExist(string filePath)
	{
#if (OPPO_MINI_GAME || VIVO_MINI_GAME) && UNITY_WEBGL && !UNITY_EDITOR
		return QuickGamePackageAccess(getPackagePath(filePath)) != 0;
#else
		return false;
#endif
	}
	private static byte[] readPackageBytes(string filePath)
	{
#if (OPPO_MINI_GAME || VIVO_MINI_GAME) && UNITY_WEBGL && !UNITY_EDITOR
		IntPtr pointer = QuickGamePackageRead(getPackagePath(filePath), out int length);
		if (pointer == IntPtr.Zero)
		{
			return null;
		}
		try
		{
			byte[] bytes = new byte[length];
			Marshal.Copy(pointer, bytes, 0, length);
			return bytes;
		}
		finally
		{
			QuickGamePackageFree(pointer);
		}
#else
		return null;
#endif
	}
#if (OPPO_MINI_GAME || VIVO_MINI_GAME) && UNITY_WEBGL && !UNITY_EDITOR
	[DllImport("__Internal")]
	private static extern int QuickGamePackageAccess(string path);
	[DllImport("__Internal")]
	private static extern int QuickGamePackageIsDirectory(string path);
	[DllImport("__Internal")]
	private static extern int QuickGamePackageSize(string path);
	[DllImport("__Internal")]
	private static extern string QuickGamePackageDirectory(string path);
	[DllImport("__Internal")]
	private static extern IntPtr QuickGamePackageRead(string path, out int length);
	[DllImport("__Internal")]
	private static extern void QuickGamePackageFree(IntPtr pointer);
#endif
}
