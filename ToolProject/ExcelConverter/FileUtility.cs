using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.Cryptography;

public class FileUtility : StringUtility
{
	public static void validPath(ref string path)
	{
		if (isEmpty(path))
		{
			return;
		}
		// 不以/结尾,则加上/
		if (path[path.Length - 1] != '/')
		{
			path += "/";
		}
	}
	// 打开一个文本文件,fileName为绝对路径
	public static byte[] openFile(string fileName)
	{
		try
		{
			using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
			{
				if (fs == null)
				{
					return null;
				}
				int fileSize = (int)fs.Length;
				byte[] fileBuffer = new byte[fileSize];
				fs.Read(fileBuffer, 0, fileSize);
				return fileBuffer;
			}
		}
		catch
		{
			return null;
		}
	}
	// 打开一个文本文件,fileName为绝对路径
	public static string openTxtFile(string fileName)
	{
		try
		{
			StreamReader streamReader = File.OpenText(fileName);
			if (streamReader == null)
			{
				return null;
			}
			string fileBuffer = streamReader.ReadToEnd();
			streamReader.Close();
			streamReader.Dispose();
			return fileBuffer;
		}
		catch
		{
			return null;
		}
	}
	public static void openTxtFileLines(string filePath, out string[] fileLines)
	{
		fileLines = split(openTxtFile(filePath), true, "\n");
		if (fileLines == null)
		{
			return;
		}
		for (int i = 0; i < fileLines.Length; ++i)
		{
			fileLines[i] = removeAll(fileLines[i], "\r");
		}
	}
	// 写一个文本文件,fileName为绝对路径,content是写入的字符串
	public static void writeFile(string fileName, byte[] buffer, int size, bool appendData = false)
	{
		// 检测路径是否存在,如果不存在就创建一个
		createDir(getFilePath(fileName));
		FileStream file;
		if(appendData)
		{
			file = new FileStream(fileName, FileMode.Append, FileAccess.Write);
		}
		else
		{
			file = new FileStream(fileName, FileMode.Create, FileAccess.Write);
		}
		file.Write(buffer, 0, size);
		file.Flush();
		file.Close();
		file.Dispose();
	}
	// 写一个文本文件,fileName为绝对路径,content是写入的字符串
	public static void writeTxtFile(string fileName, string content, bool appendData = false)
	{
		byte[] bytes = stringToBytes(content, Encoding.UTF8);
		if(bytes != null)
		{
			writeFile(fileName, bytes, bytes.Length, appendData);
		}
	}
	public static void writeTxtFileBOM(string fileName, string content, bool appendData = false)
	{
		byte[] bytes = stringToBytes(content, Encoding.UTF8);
		byte[] newBytes = new byte[bytes.Length + 3];
		newBytes[0] = 0xEF;
		newBytes[1] = 0xBB;
		newBytes[2] = 0xBF;
		if (bytes != null)
		{
			memcpy(newBytes, bytes, 3, 0, bytes.Length);
		}
		writeFile(fileName, newBytes, newBytes.Length, appendData);
	}
	public static void deleteFolder(string path)
	{
		validPath(ref path);
		if (!isDirExist(path))
		{
			return;
		}
		string[] dirList = Directory.GetDirectories(path);
		// 先删除所有文件夹
		int dirCount = dirList.Length;
		for(int i = 0; i < dirCount; ++i)
		{
			deleteFolder(dirList[i]);
		}
		// 再删除所有文件
		string[] fileList = Directory.GetFiles(path);
		int fileCount = fileList.Length;
		for(int i = 0; i < fileCount; ++i)
		{
			deleteFile(fileList[i]);
		}
		// 再删除文件夹自身
		Directory.Delete(path);
	}
	public static bool isDirExist(string dir)
	{
		if(string.IsNullOrEmpty(dir) || dir == "./" || dir == "../")
		{
			return true;
		}
		return Directory.Exists(dir);
	}
	public static bool isFileExist(string fileName)
	{
		return File.Exists(fileName);
	}
	public static void createDir(string dir)
	{
		if (isDirExist(dir))
		{
			return;
		}
		// 如果有上一级目录,并且上一级目录不存在,则先创建上一级目录
		string parentDir = getFilePath(dir);
		if (parentDir != dir)
		{
			createDir(parentDir);
		}
		Directory.CreateDirectory(dir);
	}
	// path为绝对路径
	public static void findFiles(string path, List<string> fileList, string pattern, bool recursive = true)
	{
		List<string> patterns = new List<string>();
		if (!string.IsNullOrEmpty(pattern))
		{
			patterns.Add(pattern);
		}
		findFiles(path, fileList, patterns, recursive);
	}
	// path为绝对路径
	public static void findFiles(string path, List<string> fileList, List<string> patterns = null, bool recursive = true)
	{
		validPath(ref path);
		if (!isDirExist(path))
		{
			return;
		}
		DirectoryInfo folder = new DirectoryInfo(path);
		FileInfo[] fileInfoList = folder.GetFiles();
		int fileCount = fileInfoList.Length;
		int patternCount = patterns != null ? patterns.Count : 0;
		for (int i = 0; i < fileCount; ++i)
		{
			string fileName = fileInfoList[i].Name;
			// 如果需要过滤后缀名,则判断后缀
			if (patternCount > 0)
			{
				for (int j = 0; j < patternCount; ++j)
				{
					if (endWith(fileName, patterns[j], false))
					{
						fileList.Add(path + fileName);
					}
				}
			}
			// 不需要过滤,则直接放入列表
			else
			{
				fileList.Add(path + fileName);
			}
		}
		// 查找所有子目录
		if (recursive)
		{
			string[] dirs = Directory.GetDirectories(path);
			int count = dirs.Length;
			for(int i = 0; i < count; ++i)
			{
				findFiles(dirs[i], fileList, patterns, recursive);
			}
		}
	}
	public static void deleteFile(string path)
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		AndroidAssetLoader.deleteFile(path);
#endif
		File.Delete(path);
	}
	// 计算一个文件的MD5,fileName为绝对路径
	public static string generateFileMD5(string fileName, bool upperOrLower = true)
	{
		byte[] fileContent = openFile(fileName);
		if (fileContent == null)
		{
			return EMPTY;
		}
		return generateMD5(fileContent, fileContent.Length, upperOrLower);
	}
	// 计算一个文件的MD5,fileName为绝对路径
	public static string generateMD5(string str, bool upperOrLower = true)
	{
		return generateMD5(stringToBytes(str), -1, upperOrLower);
	}
	// 计算一个文件的MD5
	public static string generateMD5(byte[] fileContent, int length = -1, bool upperOrLower = true)
	{
		if (length < 0)
		{
			length = fileContent.Length;
		}
		HashAlgorithm algorithm = MD5.Create();
		return bytesToHEXString(algorithm.ComputeHash(fileContent, 0, length), 0, 0, false, upperOrLower);
	}
}