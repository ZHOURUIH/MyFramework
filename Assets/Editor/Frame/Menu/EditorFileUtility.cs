using System;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;
using static StringUtility;
using static FrameDefine;
using static BinaryUtility;
using static UnityUtility;
using static FileUtility;

public class EditorFileUtility
{
	// 打开一个二进制文件,fileName为绝对路径,返回值为文件长度
	// 使用完毕后需要使用releaseFile回收文件内存
	public static byte[] openFile(string fileName, bool errorIfNull)
	{
		byte[] fileBuffer = null;
		try
		{
#if !UNITY_ANDROID || UNITY_EDITOR
			using FileStream fs = new(fileName, FileMode.Open, FileAccess.Read);
			if (fs == null)
			{
				if (errorIfNull)
				{
					logError("文件加载失败! : " + fileName);
				}
				return null;
			}
			int fileSize = (int)fs.Length;
			fileBuffer = new byte[fileSize];
			fs.Read(fileBuffer, 0, fileSize);
			return fileBuffer;
#else
			// 安卓平台如果要读取StreamingAssets下的文件,只能使用AssetManager
			if (fileName.startWith(F_STREAMING_ASSETS_PATH))
			{
				// 改为相对路径
				fileName = fileName.removeStartCount(F_STREAMING_ASSETS_PATH.Length);
				fileBuffer = AndroidAssetLoader.loadAsset(fileName, errorIfNull);
			}
			// 安卓平台如果要读取persistentDataPath的文件,则可以使用File
			else if (fileName.startWith(F_PERSISTENT_DATA_PATH))
			{
				fileBuffer = AndroidAssetLoader.loadFile(fileName, errorIfNull);
			}
			else
			{
				logError("openFile invalid path : " + fileName);
			}
			if (fileBuffer == null && errorIfNull)
			{
				logError("open file failed! filename : " + fileName);
			}
			return fileBuffer;
#endif
		}
		catch (Exception e)
		{
			if (errorIfNull)
			{
				logException(e, "文件加载失败! : " + fileName);
			}
		}
		return null;
	}
	// 打开一个文本文件,fileName为绝对路径
	public static string openTxtFile(string fileName, bool errorIfNull)
	{
		byte[] fileContent = openFile(fileName, errorIfNull);
		if (fileContent == null)
		{
			return EMPTY;
		}
		return bytesToString(fileContent, 0, fileContent.Length);
	}
	// 打开一个文本文件,fileName为绝对路径,并且自动将文件拆分为多行,移除末尾的换行符(\r或者\n),存储在fileLines中,包含空行,返回值是行数
	public static int openTxtFileLines(string fileName, out string[] fileLines, bool errorIfNull = true, bool keepEmptyLine = true)
	{
		string fileContent = openTxtFile(fileName, errorIfNull);
		if (fileContent.isEmpty())
		{
			fileLines = null;
			return 0;
		}
		splitLine(fileContent, out fileLines, !keepEmptyLine);
		return fileLines.count();
	}
	// 打开一个文本文件,fileName为绝对路径,并且自动将文件拆分为多行,移除末尾的换行符(\r或者\n),存储在fileLines中,包含空行,返回值是行数
	public static string[] openTxtFileLines(string fileName, bool errorIfNull = true, bool keepEmptyLine = true)
	{
		openTxtFileLines(fileName, out string[] lines, errorIfNull, keepEmptyLine);
		return lines;
	}
	// 打开一个文本文件进行处理,然后再写回文本文件
	public static void processFileLine(string fileName, Action<List<string>> process)
	{
		using var a = new ListScope<string>(out var fileLineList, openTxtFileLines(fileName));
		process(fileLineList);
		writeTxtFile(fileName, stringsToString(fileLineList, "\r\n"));
	}
	public static void copyFile(string source, string target, bool overwrite = true)
	{
		createDir(getFilePath(target));
		File.Copy(source, target, overwrite);
	}
	// 计算一个文件的MD5,fileName为绝对路径
	public static string generateFileMD5(string fileName, bool upperOrLower = true)
	{
		// 安卓平台下容易oom,所以调用java的函数通过流的方式来计算md5
		byte[] fileContent = openFile(fileName, true);
		if (fileContent == null)
		{
			return EMPTY;
		}
		return generateFileMD5(fileContent, fileContent.Length, upperOrLower);
	}
	// 计算一个文件的MD5
	public static string generateFileMD5(byte[] fileContent, int length = -1, bool upperOrLower = true)
	{
		if (length < 0)
		{
			length = fileContent.Length;
		}
		HashAlgorithm algorithm = MD5.Create();
		return bytesToHEXString(algorithm.ComputeHash(fileContent, 0, length), 0, 0, false, upperOrLower);
	}
	public static void encryptFileAES(string fileFullPath, byte[] key, byte[] vi)
	{
		byte[] fileBytes = openFile(fileFullPath, true);
		if (fileBytes.count() == 0)
		{
			return;
		}
		byte[] bytes = encryptAES(fileBytes, key, vi);
		writeFile(fileFullPath, bytes, bytes.Length);
	}
}