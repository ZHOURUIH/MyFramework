using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.Profiling;

public class FileUtility : MathUtility
{
	private static List<string> mTempPatternList = new List<string>();
	private static List<string> mTempFileList = new List<string>();
	private static List<string> mTempFileList1 = new List<string>();
	public static void validPath(ref string path)
	{
		if (path.Length > 0)
		{
			// 不以/结尾,则加上/
			if (path[path.Length - 1] != '/')
			{
				path += "/";
			}
		}
	}
	// 打开一个二进制文件,fileName为绝对路径
	public static void openFile(string fileName, out byte[] fileBuffer, bool errorIfNull)
	{
		fileBuffer = null;
		try
		{
#if !UNITY_ANDROID || UNITY_EDITOR
			FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read);
			if(fs == null)
			{
				if(errorIfNull)
				{
					UnityUtility.logInfo("open file failed! filename : " + fileName);
				}
				return;
			}
			int fileSize = (int)fs.Length;
			fileBuffer = new byte[fileSize];
			fs.Read(fileBuffer, 0, fileSize);
			fs.Close();
			fs.Dispose();
#else
			// 安卓平台如果要读取StreamingAssets下的文件,只能使用AssetManager
			if (startWith(fileName, CommonDefine.F_STREAMING_ASSETS_PATH))
			{
				// 改为相对路径
				fileName = fileName.Substring(CommonDefine.F_STREAMING_ASSETS_PATH.Length, fileName.Length - CommonDefine.F_STREAMING_ASSETS_PATH.Length);
				fileBuffer = AndroidAssetLoader.loadAsset(fileName, errorIfNull);
			}
			// 安卓平台如果要读取persistentDataPath的文件,则可以使用File
			else if (startWith(fileName, CommonDefine.F_PERSISTENT_DATA_PATH))
			{
				fileBuffer = AndroidAssetLoader.loadFile(fileName, errorIfNull);
			}
			else
			{
				UnityUtility.logError("openFile invalid path : " + fileName);
			}
#endif
		}
		catch (Exception)
		{
			UnityUtility.logInfo("open file failed! filename : " + fileName);
		}
	}
	// 打开一个文本文件,fileName为绝对路径
	public static string openTxtFile(string fileName, bool errorIfNull)
	{
		try
		{
#if !UNITY_ANDROID || UNITY_EDITOR
			StreamReader streamReader = File.OpenText(fileName);
			if (streamReader == null)
			{
				if(errorIfNull)
				{
					UnityUtility.logInfo("open file failed! filename : " + fileName);
				}	
				return FrameBase.EMPTY_STRING;
			}
			string fileBuffer = streamReader.ReadToEnd();
			streamReader.Close();
			streamReader.Dispose();
			return fileBuffer;
#else
			// 安卓平台如果要读取StreamingAssets下的文件,只能使用AssetManager
			if (startWith(fileName, CommonDefine.F_STREAMING_ASSETS_PATH))
			{
				// 改为相对路径
				fileName = fileName.Substring(CommonDefine.F_STREAMING_ASSETS_PATH.Length, fileName.Length - CommonDefine.F_STREAMING_ASSETS_PATH.Length);
				return AndroidAssetLoader.loadTxtAsset(fileName, errorIfNull);
			}
			// 安卓平台如果要读取persistentDataPath的文件,则可以使用File
			else if (startWith(fileName, CommonDefine.F_PERSISTENT_DATA_PATH))
			{
				return AndroidAssetLoader.loadTxtFile(fileName, errorIfNull);
			}
			else
			{
				UnityUtility.logError("openTxtFile invalid path : " + fileName);
			}
			return FrameBase.EMPTY_STRING;
#endif
		}
		catch (Exception)
		{
			UnityUtility.logInfo("open file failed! filename : " + fileName);
			return FrameBase.EMPTY_STRING;
		}
	}
	// 写一个文本文件,fileName为绝对路径,content是写入的字符串
	public static void writeFile(string fileName, byte[] buffer, int size, bool appendData = false)
	{
		// 检测路径是否存在,如果不存在就创建一个
		createDir(getFilePath(fileName));
#if !UNITY_ANDROID || UNITY_EDITOR
		FileStream file = null;
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
#else
		AndroidAssetLoader.writeFile(fileName, buffer, size, appendData);
#endif
	}
	// 写一个文本文件,fileName为绝对路径,content是写入的字符串
	public static void writeTxtFile(string fileName, string content, bool appendData = false)
	{
#if !UNITY_ANDROID || UNITY_EDITOR
		byte[] bytes = stringToBytes(content, Encoding.UTF8);
		writeFile(fileName, bytes, bytes.Length, appendData);
#else
		if(FrameBase.mAndroidAssetLoader != null)
		{
			// 检测路径是否存在,如果不存在就创建一个
			createDir(getFilePath(fileName));
			AndroidAssetLoader.writeTxtFile(fileName, content, appendData);
		}
#endif
	}
	public static bool renameFile(string fileName, string newName)
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		UnityUtility.logError("can not rename file on android!");
		return false;
#endif
		if (!isFileExist(fileName) || isFileExist(newName))
		{
			return false;
		}
		Directory.Move(fileName, newName);
		return true;
	}
	public static void deleteFolder(string path)
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		UnityUtility.logError("can not delete dir on android!");
		return;
#endif
		validPath(ref path);
		string[] dirList = Directory.GetDirectories(path);
		// 先删除所有文件夹
		foreach (var item in dirList)
		{
			deleteFolder(item);
		}
		// 再删除所有文件
		string[] fileList = Directory.GetFiles(path);
		foreach (var item in fileList)
		{
			deleteFile(item);
		}
		// 再删除文件夹自身
		Directory.Delete(path);
	}
	public static bool deleteEmptyFolder(string path, bool deleteSelfIfEmpty = true)
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		UnityUtility.logError("can not delete empty dir on android!");
		return false;
#endif
		validPath(ref path);
		// 先删除所有空的文件夹
		string[] dirList = Directory.GetDirectories(path);
		bool isEmpty = true;
		foreach (var item in dirList)
		{
			isEmpty = deleteEmptyFolder(item, true) && isEmpty;
		}
		isEmpty = isEmpty && Directory.GetFiles(path).Length == 0;
		if (isEmpty && deleteSelfIfEmpty)
		{
			Directory.Delete(path);
		}
		return isEmpty;
	}
	public static void moveFile(string source, string dest, bool overwrite = true)
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		UnityUtility.logError("can not copy file on android!");
		return;
#endif
		if (isFileExist(dest))
		{
			// 先删除目标文件,因为File.Move不支持覆盖文件,目标文件存在时,File.Move会失败
			if (overwrite)
			{
				deleteFile(dest);
			}
			else
			{
				return;
			}
		}
		else
		{
			// 如果目标文件所在的目录不存在,则先创建目录
			string parentDir = getFilePath(dest);
			createDir(parentDir);
		}
		File.Move(source, dest);
	}
	public static void copyFile(string source, string dest, bool overwrite = true)
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		byte[] fileBuffer = null;
		openFile(source, out fileBuffer, true);
		if(!isFileExist(dest) || overwrite)
		{
			writeFile(dest, fileBuffer, fileBuffer.Length);
		}
#else
		// 如果目标文件所在的目录不存在,则先创建目录
		string parentDir = getFilePath(dest);
		createDir(parentDir);
		File.Copy(source, dest, overwrite);
#endif
	}
	public static int getFileSize(string file)
	{
		try
		{
#if !UNITY_ANDROID || UNITY_EDITOR
			FileInfo fileInfo = new FileInfo(file);
			return (int)fileInfo.Length;
#else
			return AndroidAssetLoader.getFileSize(file);
#endif
		}
		catch
		{
			return 0;
		}
	}
	public static bool isDirExist(string dir)
	{
		if(dir.Length == 0 || dir == "./" || dir == "../")
		{
			return true;
		}
#if !UNITY_ANDROID || UNITY_EDITOR
		return Directory.Exists(dir);
#else
		// 安卓平台如果要读取StreamingAssets下的文件,只能使用AssetManager
		if(startWith(dir + "/", CommonDefine.F_STREAMING_ASSETS_PATH))
		{
			// 改为相对路径
			dir = dir.Substring(CommonDefine.F_STREAMING_ASSETS_PATH.Length, dir.Length - CommonDefine.F_STREAMING_ASSETS_PATH.Length);
			return AndroidAssetLoader.isAssetExist(dir);
		}
		// 安卓平台如果要读取persistentDataPath的文件,则可以使用File
		else if (startWith(dir + "/", CommonDefine.F_PERSISTENT_DATA_PATH))
		{
			return AndroidAssetLoader.isDirExist(dir);
		}
		else
		{
			UnityUtility.logError("isDirExist invalid path : " + dir);
		}
		return false;
#endif
	}
	public static bool isFileExist(string fileName)
	{
#if !UNITY_ANDROID || UNITY_EDITOR
		return File.Exists(fileName);
#else
		// 安卓平台如果要读取StreamingAssets下的文件,只能使用AssetManager
		if(startWith(fileName, CommonDefine.F_STREAMING_ASSETS_PATH))
		{
			// 改为相对路径
			fileName = fileName.Substring(CommonDefine.F_STREAMING_ASSETS_PATH.Length, fileName.Length - CommonDefine.F_STREAMING_ASSETS_PATH.Length);
			return AndroidAssetLoader.isAssetExist(fileName);
		}
		// 安卓平台如果要读取persistentDataPath的文件,则可以使用File
		else if (startWith(fileName, CommonDefine.F_PERSISTENT_DATA_PATH))
		{
			return AndroidAssetLoader.isFileExist(fileName);
		}
		else
		{
			UnityUtility.logError("isFileExist invalid path : " + fileName);
		}
		return false;
#endif
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
#if UNITY_ANDROID && !UNITY_EDITOR
		AndroidAssetLoader.createDirectory(dir);
#else
		Directory.CreateDirectory(dir);
#endif
	}
	public static List<string> findResourcesFilesNonAlloc(string path, string pattern, bool recursive = true, bool keepAbsolutePath = false)
	{
		mTempPatternList.Clear();
		if (pattern.Length != 0)
		{
			mTempPatternList.Add(pattern);
		}
		mTempFileList.Clear();
		findResourcesFiles(path, mTempFileList, mTempPatternList, recursive, keepAbsolutePath);
		return mTempFileList;
	}
	// path为GameResources下的相对路径
	public static List<string> findResourcesFilesNonAlloc(string path, List<string> patterns = null, bool recursive = true, bool keepAbsolutePath = false)
	{
		mTempFileList.Clear();
		findResourcesFiles(path, mTempFileList, patterns, recursive, keepAbsolutePath);
		return mTempFileList;
	}
	// path为GameResources下的相对路径
	public static void findResourcesFiles(string path, List<string> fileList, string pattern, bool recursive = true, bool keepAbsolutePath = false)
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		UnityUtility.logError("can not find resouces files on android!");
		return;
#endif
		mTempPatternList.Clear();
		if (pattern.Length != 0)
		{
			mTempPatternList.Add(pattern);
		}
		findResourcesFiles(path, fileList, mTempPatternList, recursive);
	}
	// path为GameResources下的相对路径
	public static void findResourcesFiles(string path, List<string> fileList, List<string> patterns = null, bool recursive = true, bool keepAbsolutePath = false)
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		UnityUtility.logError("can not find resouces files on android!");
		return;
#endif
		validPath(ref path);
		if (!startWith(path, CommonDefine.F_GAME_RESOURCES_PATH))
		{
			path = CommonDefine.F_GAME_RESOURCES_PATH + path;
		}
		findFiles(path, fileList, patterns, recursive);
		if (!keepAbsolutePath)
		{
			int removeLength = CommonDefine.F_GAME_RESOURCES_PATH.Length;
			int count = fileList.Count;
			for (int i = 0; i < count; ++i)
			{
				fileList[i] = fileList[i].Substring(removeLength);
			}
		}
	}
	public static List<string> findStreamingAssetsFilesNonAlloc(string path, string pattern, bool recursive = true, bool keepAbsolutePath = false)
	{
		mTempPatternList.Clear();
		if (pattern.Length != 0)
		{
			mTempPatternList.Add(pattern);
		}
		mTempFileList.Clear();
		findStreamingAssetsFiles(path, mTempFileList, mTempPatternList, recursive, keepAbsolutePath);
		return mTempFileList;
	}
	public static List<string> findStreamingAssetsFilesNonAlloc(string path, List<string> patterns = null, bool recursive = true, bool keepAbsolutePath = false)
	{
		mTempFileList.Clear();
		findStreamingAssetsFiles(path, mTempFileList, patterns, recursive, keepAbsolutePath);
		return mTempFileList;
	}
	// path为StreamingAssets下的相对路径
	public static void findStreamingAssetsFiles(string path, List<string> fileList, string pattern, bool recursive = true, bool keepAbsolutePath = false)
	{
		mTempPatternList.Clear();
		if (pattern.Length != 0)
		{
			mTempPatternList.Add(pattern);
		}
		findStreamingAssetsFiles(path, fileList, mTempPatternList, recursive, keepAbsolutePath);
	}
	// path为StreamingAssets下的相对路径,返回的路径列表为绝对路径
	public static void findStreamingAssetsFiles(string path, List<string> fileList, List<string> patterns = null, bool recursive = true, bool keepAbsolutePath = false)
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		// 转换为相对路径
		if (startWith(path, CommonDefine.F_STREAMING_ASSETS_PATH))
		{
			path = path.Substring(CommonDefine.F_STREAMING_ASSETS_PATH.Length);
		}
		AndroidAssetLoader.findAssets(path, fileList, patterns, recursive);
		// 查找后的路径本身就是相对路径,如果需要保留绝对路径,则需要将路径加上
		if(keepAbsolutePath)
		{
			int removeLength = CommonDefine.F_STREAMING_ASSETS_PATH.Length;
			int count = fileList.Count;
			for(int i = 0; i < count; ++i)
			{
				fileList[i] = CommonDefine.F_STREAMING_ASSETS_PATH + fileList[i];
			}
		}
#else
		if (!startWith(path, CommonDefine.F_STREAMING_ASSETS_PATH))
		{
			path = CommonDefine.F_STREAMING_ASSETS_PATH + path;
		}
		findFiles(path, fileList, patterns, recursive);
		if (!keepAbsolutePath)
		{
			int removeLength = CommonDefine.F_STREAMING_ASSETS_PATH.Length;
			int count = fileList.Count;
			for (int i = 0; i < count; ++i)
			{
				fileList[i] = fileList[i].Substring(removeLength);
			}
		}
#endif
	}
	// path为StreamingAssets下的相对路径,返回的路径列表为绝对路径
	public static void findStreamingAssetsFolders(string path, List<string> folderList, bool recursive = true, bool keepAbsolutePath = false)
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		// 转换为相对路径
		if (startWith(path, CommonDefine.F_STREAMING_ASSETS_PATH))
		{
			path = path.Substring(CommonDefine.F_STREAMING_ASSETS_PATH.Length);
		}
		AndroidAssetLoader.findAssetsFolder(path, folderList, recursive);
		// 查找后的路径本身就是相对路径,如果需要保留绝对路径,则需要将路径加上
		if(keepAbsolutePath)
		{
			int removeLength = CommonDefine.F_STREAMING_ASSETS_PATH.Length;
			int count = folderList.Count;
			for(int i = 0; i < count; ++i)
			{
				folderList[i] = CommonDefine.F_STREAMING_ASSETS_PATH + folderList[i];
			}
		}
#else
		// 非安卓平台则查找普通的文件夹
		if (!startWith(path, CommonDefine.F_STREAMING_ASSETS_PATH))
		{
			path = CommonDefine.F_STREAMING_ASSETS_PATH + path;
		}
		findFolders(path, folderList, recursive);
		if (!keepAbsolutePath)
		{
			int removeLength = CommonDefine.F_STREAMING_ASSETS_PATH.Length;
			int count = folderList.Count;
			for (int i = 0; i < count; ++i)
			{
				folderList[i] = folderList[i].Substring(removeLength);
			}
		}
#endif
	}
	public static List<string> findFilesNonAlloc(string path, string pattern, bool recursive = true)
	{
		mTempPatternList.Clear();
		if (pattern.Length != 0)
		{
			mTempPatternList.Add(pattern);
		}
		mTempFileList1.Clear();
		findFiles(path, mTempFileList1, mTempPatternList, recursive);
		return mTempFileList1;
	}
	public static List<string> findFilesNonAlloc(string path, List<string> patterns = null, bool recursive = true)
	{
		mTempFileList1.Clear();
		findFiles(path, mTempFileList1, patterns, recursive);
		return mTempFileList1;
	}
	// path为绝对路径
	public static void findFiles(string path, List<string> fileList, string pattern, bool recursive = true)
	{
		mTempPatternList.Clear();
		if(pattern.Length != 0)
		{
			mTempPatternList.Add(pattern);
		}
		findFiles(path, fileList, mTempPatternList, recursive);
	}
	// path为绝对路径
	public static void findFiles(string path, List<string> fileList, List<string> patterns = null, bool recursive = true)
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		AndroidAssetLoader.findFiles(path, fileList, patterns, recursive);
#else
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
			foreach (var item in dirs)
			{
				findFiles(item, fileList, patterns, recursive);
			}
		}
#endif
	}
	// 得到指定目录下的所有第一级子目录
	// path为绝对路径
	public static bool findFolders(string path, List<string> dirList, bool recursive = true)
	{
		validPath(ref path);
		if (!isDirExist(path))
		{
			return false;
		}
#if UNITY_ANDROID && !UNITY_EDITOR
		AndroidAssetLoader.findFolders(path, dirList, recursive);
#else
		// 非安卓平台则查找普通文件夹
		string[] dirs = Directory.GetDirectories(path);
		foreach (var item in dirs)
		{
			dirList.Add(item);
			if (recursive)
			{
				findFolders(item, dirList, recursive);
			}
		}
#endif
		return true;
	}
	public static void deleteFile(string path)
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		AndroidAssetLoader.deleteFile(path);
#endif
		File.Delete(path);
	}
	public static string generateFileMD5(string fileName, bool upperOrLower = true)
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		UnityUtility.logError("can not generate file md5 on android!");
		return FrameBase.EMPTY_STRING;
#endif
		FileStream file = new FileStream(fileName, FileMode.Open);
		HashAlgorithm algorithm = MD5.Create();
		byte[] md5Bytes = algorithm.ComputeHash(file);
		file.Close();
		file.Dispose();
		return bytesToHEXString(md5Bytes, false, upperOrLower);
	}
}