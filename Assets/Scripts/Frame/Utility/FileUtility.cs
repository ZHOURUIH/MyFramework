using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using static UnityUtility;
using static StringUtility;
using static MathUtility;
using static BinaryUtility;
using static FrameDefine;
using static FrameUtility;

// 文件工具函数类
public class FileUtility
{
	private static List<string> mTempPatternList = new List<string>();	// 用于避免GC
	private static List<string> mTempFileList = new List<string>();		// 用于避免GC
	private static List<string> mTempFileList1 = new List<string>();	// 用于避免GC
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
	// 打开一个二进制文件,fileName为绝对路径,返回值为文件长度
	// 使用完毕后需要使用releaseFile回收文件内存
	public static byte[] openFile(string fileName, bool errorIfNull)
	{
		byte[] fileBuffer = null;
		try
		{
#if !UNITY_ANDROID || UNITY_EDITOR
			using (FileStream fs = new FileStream(fileName, FileMode.Open, FileAccess.Read))
			{
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
			}
#else
			// 安卓平台如果要读取StreamingAssets下的文件,只能使用AssetManager
			if (startWith(fileName, F_STREAMING_ASSETS_PATH))
			{
				// 改为相对路径
				fileName = fileName.Substring(F_STREAMING_ASSETS_PATH.Length);
				fileBuffer = AndroidAssetLoader.loadAsset(fileName, errorIfNull);
			}
			// 安卓平台如果要读取persistentDataPath的文件,则可以使用File
			else if (startWith(fileName, F_PERSISTENT_DATA_PATH))
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
		if (isEmpty(fileContent))
		{
			fileLines = null;
			return 0;
		}
		splitLine(fileContent, out fileLines, !keepEmptyLine);
		return fileLines != null ? fileLines.Length : 0;
	}
	// 写一个文本文件,fileName为绝对路径,content是写入的字符串
	public static void writeFile(string fileName, byte[] buffer, int size, bool appendData = false)
	{
		// 检测路径是否存在,如果不存在就创建一个
		createDir(getFilePath(fileName));
#if !UNITY_ANDROID || UNITY_EDITOR
		using (FileStream file = new FileStream(fileName, appendData ? FileMode.Append : FileMode.Create, FileAccess.Write))
		{
			file.Write(buffer, 0, size);
			file.Flush();
		}
#else
		AndroidAssetLoader.writeFile(fileName, buffer, size, appendData);
#endif
	}
	// 写一个文本文件,fileName为绝对路径,content是写入的字符串
	public static void writeTxtFile(string fileName, string content, bool appendData = false)
	{
#if !UNITY_ANDROID || UNITY_EDITOR
		byte[] bytes = stringToBytes(content);
		if (bytes != null)
		{
			writeFile(fileName, bytes, bytes.Length, appendData);
		}
#else
		if(FrameBase.mAndroidAssetLoader != null)
		{
			// 检测路径是否存在,如果不存在就创建一个
			createDir(getFilePath(fileName));
			AndroidAssetLoader.writeTxtFile(fileName, content, appendData);
		}
#endif
	}
	// 重命名文件,参数为绝对路径
	public static bool renameFile(string fileName, string newName)
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		logError("can not rename file on android!");
		return false;
#endif
		if (!isFileExist(fileName) || isFileExist(newName))
		{
			return false;
		}
		Directory.Move(fileName, newName);
		return true;
	}
	// 删除目录,参数为绝对路径
	public static void deleteFolder(string path)
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		if (!AndroidAssetLoader.deleteDirectory(path))
		{
			logWarning("删除目录失败:" + path);
		}
		return;
#endif
		validPath(ref path);
		if (!isDirExist(path))
		{
			return;
		}
		string[] dirList = Directory.GetDirectories(path);
		// 先删除所有文件夹
		int dirCount = dirList.Length;
		for (int i = 0; i < dirCount; ++i)
		{
			deleteFolder(dirList[i]);
		}
		// 再删除所有文件
		string[] fileList = Directory.GetFiles(path);
		int fileCount = fileList.Length;
		for (int i = 0; i < fileCount; ++i)
		{
			deleteFile(fileList[i]);
		}
		// 再删除文件夹自身
		Directory.Delete(path);
	}
	// 删除path中的所有空目录,参数为绝对路径,deleteSelfIfEmpty表示path本身为空时是否需要删除
	public static bool deleteEmptyFolder(string path, bool deleteSelfIfEmpty = true)
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		logError("can not delete empty dir on android!");
		return false;
#endif
		validPath(ref path);
		// 先删除所有空的文件夹
		string[] dirList = Directory.GetDirectories(path);
		bool isEmpty = true;
		int dirCount = dirList.Length;
		for (int i = 0; i < dirCount; ++i)
		{
			isEmpty = deleteEmptyFolder(dirList[i], true) && isEmpty;
		}
		isEmpty = isEmpty && Directory.GetFiles(path).Length == 0;
		if (isEmpty && deleteSelfIfEmpty)
		{
			Directory.Delete(path);
		}
		return isEmpty;
	}
	// 移动文件,参数为绝对路径
	public static void moveFile(string source, string dest, bool overwrite = true)
	{
		if (!isFileExist(source))
		{
			return;
		}
#if UNITY_ANDROID && !UNITY_EDITOR
		logError("can not move file on android!");
		return;
#endif
		if (isFileExist(dest))
		{
			// 先删除目标文件,因为File.Move不支持覆盖文件,目标文件存在时,File.Move会失败
			if (!overwrite)
			{
				return;
			}
			deleteFile(dest);
		}
		else
		{
			// 如果目标文件所在的目录不存在,则先创建目录
			createDir(getFilePath(dest));
		}
		File.Move(source, dest);
	}
	// 将StreamingAsset中的文件拷贝到PersistentDataPath中,参数为绝对路径,比直接调用copyFile节省内存
	public static void copyStreamingAssetToPersistentFile(string source, string dest, bool overwrite = true)
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		// 安卓平台如果要读取StreamingAssets下的文件,只能使用AssetManager
		if (startWith(source, F_STREAMING_ASSETS_PATH))
		{
			// 改为相对路径
			source = source.Substring(F_STREAMING_ASSETS_PATH.Length);
		}
		AndroidAssetLoader.copyAssetToPersistentPath(source, dest, true);
#else
		copyFile(source, dest, overwrite);
#endif
	}
	// 拷贝文件,参数为绝对路径
	public static void copyFile(string source, string dest, bool overwrite = true)
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		byte[] fileBuffer = openFile(source, true);
		if(!isFileExist(dest) || overwrite)
		{
			writeFile(dest, fileBuffer, fileBuffer.Length);
		}
#else
		// 如果目标文件所在的目录不存在,则先创建目录
		createDir(getFilePath(dest));
		File.Copy(source, dest, overwrite);
#endif
	}
	// 删除文件,参数为绝对路径
	public static bool deleteFile(string path)
	{
		rightToLeft(ref path);
#if UNITY_ANDROID && !UNITY_EDITOR
		return AndroidAssetLoader.deleteFile(path);
#else
		File.Delete(path);
		return true;
#endif
	}
	// 获得文件大小,file为绝对路径
	public static int getFileSize(string file)
	{
		rightToLeft(ref file);
		try
		{
#if !UNITY_ANDROID || UNITY_EDITOR
			return (int)new FileInfo(file).Length;
#else
			return AndroidAssetLoader.getFileSize(file);
#endif
		}
		catch(Exception e)
		{
			logException(e, "getFileSize error! file:" + file);
			return 0;
		}
	}
	// 目录是否存在,dir是绝对路径
	public static bool isDirExist(string dir)
	{
		if (isEmpty(dir) || dir == "./" || dir == "../")
		{
			return true;
		}
		validPath(ref dir);
#if !UNITY_ANDROID || UNITY_EDITOR
		return Directory.Exists(dir);
#else
		// 安卓平台如果要读取StreamingAssets下的文件,只能使用AssetManager
		if(startWith(dir, F_STREAMING_ASSETS_PATH))
		{
			// 改为相对路径
			dir = dir.Substring(F_STREAMING_ASSETS_PATH.Length);
			return AndroidAssetLoader.isAssetExist(dir);
		}
		// 安卓平台如果要读取persistentDataPath的文件,则可以使用File
		if (startWith(dir, F_PERSISTENT_DATA_PATH))
		{
			return AndroidAssetLoader.isDirExist(dir);
		}
		logError("isDirExist invalid path : " + dir);
		return false;
#endif
	}
	// 文件是否存在,fileName为绝对路径
	public static bool isFileExist(string fileName)
	{
#if !UNITY_ANDROID || UNITY_EDITOR
		return File.Exists(fileName);
#else
		// 安卓平台如果要读取StreamingAssets下的文件,只能使用AssetManager
		if (startWith(fileName, F_STREAMING_ASSETS_PATH))
		{
			// 改为相对路径
			fileName = fileName.Substring(F_STREAMING_ASSETS_PATH.Length);
			return AndroidAssetLoader.isAssetExist(fileName);
		}
		// 安卓平台如果要读取persistentDataPath的文件,则可以使用File
		if (startWith(fileName, F_PERSISTENT_DATA_PATH))
		{
			return AndroidAssetLoader.isFileExist(fileName);
		}
		logError("isFileExist invalid path : " + fileName);
		return false;
#endif
	}
	// 创建文件夹,dir绝对路径
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
		AndroidAssetLoader.createDirectoryRecursive(dir);
#else
		Directory.CreateDirectory(dir);
#endif
	}
	// 查找指定目录下的所有文件,path为GameResources下的相对路径
	public static List<string> findResourcesFilesNonAlloc(string path, string pattern, bool recursive = true, bool keepAbsolutePath = false)
	{
		mTempPatternList.Clear();
		if (!isEmpty(pattern))
		{
			mTempPatternList.Add(pattern);
		}
		mTempFileList.Clear();
		findResourcesFiles(path, mTempFileList, mTempPatternList, recursive, keepAbsolutePath);
		return mTempFileList;
	}
	// 查找指定目录下的所有文件,path为GameResources下的相对路径
	public static List<string> findResourcesFilesNonAlloc(string path, List<string> patterns = null, bool recursive = true, bool keepAbsolutePath = false)
	{
		mTempFileList.Clear();
		findResourcesFiles(path, mTempFileList, patterns, recursive, keepAbsolutePath);
		return mTempFileList;
	}
	// 查找指定目录下的所有文件,path为GameResources下的相对路径
	public static void findResourcesFiles(string path, List<string> fileList, string pattern, bool recursive = true, bool keepAbsolutePath = false)
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		logError("can not find resouces files on android!");
		return;
#endif
		mTempPatternList.Clear();
		if (!isEmpty(pattern))
		{
			mTempPatternList.Add(pattern);
		}
		findResourcesFiles(path, fileList, mTempPatternList, recursive);
	}
	// 查找指定目录下的所有文件,path为GameResources下的相对路径
	public static void findResourcesFiles(string path, List<string> fileList, List<string> patterns = null, bool recursive = true, bool keepAbsolutePath = false)
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		logError("can not find resouces files on android!");
		return;
#endif
		validPath(ref path);
		if (!startWith(path, F_GAME_RESOURCES_PATH))
		{
			path = F_GAME_RESOURCES_PATH + path;
		}
		findFiles(path, fileList, patterns, recursive);
		if (!keepAbsolutePath)
		{
			int removeLength = F_GAME_RESOURCES_PATH.Length;
			int count = fileList.Count;
			for (int i = 0; i < count; ++i)
			{
				fileList[i] = fileList[i].Substring(removeLength);
			}
		}
	}
	// 查找指定目录下的所有文件,path为StreamingAssets下的相对路径
	public static List<string> findStreamingAssetsFilesNonAlloc(string path, string pattern, bool recursive = true, bool keepAbsolutePath = false)
	{
		mTempPatternList.Clear();
		if (!isEmpty(pattern))
		{
			mTempPatternList.Add(pattern);
		}
		mTempFileList.Clear();
		findStreamingAssetsFiles(path, mTempFileList, mTempPatternList, recursive, keepAbsolutePath);
		return mTempFileList;
	}
	// 查找指定目录下的所有文件,path为StreamingAssets下的相对路径
	public static List<string> findStreamingAssetsFilesNonAlloc(string path, List<string> patterns = null, bool recursive = true, bool keepAbsolutePath = false)
	{
		mTempFileList.Clear();
		findStreamingAssetsFiles(path, mTempFileList, patterns, recursive, keepAbsolutePath);
		return mTempFileList;
	}
	// 查找指定目录下的所有文件,path为StreamingAssets下的相对路径
	public static void findStreamingAssetsFiles(string path, List<string> fileList, string pattern, bool recursive = true, bool keepAbsolutePath = false)
	{
		mTempPatternList.Clear();
		if (!isEmpty(pattern))
		{
			mTempPatternList.Add(pattern);
		}
		findStreamingAssetsFiles(path, fileList, mTempPatternList, recursive, keepAbsolutePath);
	}
	// 查找指定目录下的所有文件,path为StreamingAssets下的相对路径,返回的路径列表为绝对路径
	public static void findStreamingAssetsFiles(string path, List<string> fileList, List<string> patterns = null, bool recursive = true, bool keepAbsolutePath = false)
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		// 转换为相对路径
		removeStartString(ref path, F_STREAMING_ASSETS_PATH);
		AndroidAssetLoader.findAssets(path, fileList, patterns, recursive);
		// 查找后的路径本身就是相对路径,如果需要保留绝对路径,则需要将路径加上
		if (keepAbsolutePath)
		{
			int count = fileList.Count;
			for(int i = 0; i < count; ++i)
			{
				fileList[i] = F_STREAMING_ASSETS_PATH + fileList[i];
			}
		}
#else
		if (!startWith(path, F_STREAMING_ASSETS_PATH))
		{
			path = F_STREAMING_ASSETS_PATH + path;
		}
		findFiles(path, fileList, patterns, recursive);
		if (!keepAbsolutePath)
		{
			int removeLength = F_STREAMING_ASSETS_PATH.Length;
			int count = fileList.Count;
			for (int i = 0; i < count; ++i)
			{
				fileList[i] = fileList[i].Substring(removeLength);
			}
		}
#endif
	}
	// 查找指定目录下的所有目录,path为StreamingAssets下的相对路径,返回的路径列表为绝对路径
	public static void findStreamingAssetsFolders(string path, List<string> folderList, bool recursive = true, bool keepAbsolutePath = false)
	{
#if UNITY_ANDROID && !UNITY_EDITOR
		// 转换为相对路径
		removeStartString(ref path, F_STREAMING_ASSETS_PATH);
		AndroidAssetLoader.findAssetsFolder(path, folderList, recursive);
		// 查找后的路径本身就是相对路径,如果需要保留绝对路径,则需要将路径加上
		if (keepAbsolutePath)
		{
			int count = folderList.Count;
			for(int i = 0; i < count; ++i)
			{
				folderList[i] = F_STREAMING_ASSETS_PATH + folderList[i];
			}
		}
#else
		// 非安卓平台则查找普通的文件夹
		if (!startWith(path, F_STREAMING_ASSETS_PATH))
		{
			path = F_STREAMING_ASSETS_PATH + path;
		}
		findFolders(path, folderList, null, recursive);
		if (!keepAbsolutePath)
		{
			int removeLength = F_STREAMING_ASSETS_PATH.Length;
			int count = folderList.Count;
			for (int i = 0; i < count; ++i)
			{
				folderList[i] = folderList[i].Substring(removeLength);
			}
		}
#endif
	}
	// 查找指定目录下的所有文件,path为绝对路径
	public static List<string> findFilesNonAlloc(string path, string pattern, bool recursive = true)
	{
		mTempPatternList.Clear();
		if (!isEmpty(pattern))
		{
			mTempPatternList.Add(pattern);
		}
		mTempFileList1.Clear();
		findFiles(path, mTempFileList1, mTempPatternList, recursive);
		return mTempFileList1;
	}
	// 查找指定目录下的所有文件,path为绝对路径
	public static List<string> findFilesNonAlloc(string path, List<string> patterns = null, bool recursive = true)
	{
		mTempFileList1.Clear();
		findFiles(path, mTempFileList1, patterns, recursive);
		return mTempFileList1;
	}
	// 查找指定目录下的所有文件,path为绝对路径
	public static void findFiles(string path, List<string> fileList, string pattern, bool recursive = true)
	{
		// 主线程中可以使用静态变量
		if (CSharpUtility.isMainThread())
		{
			mTempPatternList.Clear();
			if (!isEmpty(pattern))
			{
				mTempPatternList.Add(pattern);
			}
			findFiles(path, fileList, mTempPatternList, recursive);
		}
		else
		{
			findFiles(path, fileList, new List<string>() { pattern }, recursive);
		}
	}
	// 查找指定目录下的所有文件,path为绝对路径
	public static void findFiles(string path, List<string> fileList, List<string> patterns = null, bool recursive = true)
	{
		try
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
				int count = dirs.Length;
				for (int i = 0; i < count; ++i)
				{
					findFiles(dirs[i], fileList, patterns, recursive);
				}
			}
#endif
		}
		// 此处暂时不抛出异常信息
		catch (Exception e) 
		{
#if UNITY_EDITOR
			logForce(e.Message);
#endif
		}
	}
	// 得到指定目录下的所有第一级子目录,path为绝对路径
	public static bool findFolders(string path, List<string> dirList, List<string> excludeList, bool recursive = true)
	{
		// 非安卓平台则查找普通文件夹
		try
		{
			validPath(ref path);
			if (!isDirExist(path))
			{
				return false;
			}
#if UNITY_ANDROID && !UNITY_EDITOR
			AndroidAssetLoader.findFolders(path, dirList, recursive);
#else
			string[] dirs = Directory.GetDirectories(path);
			int count = dirs.Length;
			for (int i = 0; i < count; ++i)
			{
				string dir = dirs[i];
				if (excludeList != null && excludeList.Count > 0)
				{
					string folderName = getFileNameThread(dir);
					if (excludeList.Contains(folderName))
					{
						continue;
					}
				}
				dirList.Add(dir);
				if (recursive)
				{
					findFolders(dir, dirList, excludeList, recursive);
				}
			}
#endif
		}
		// 只捕获异常,暂时不抛出报错信息
		catch (Exception e)
		{
#if UNITY_EDITOR
			logForce(e.Message);
#endif
		}
		return true;
	}
	// 计算一个文件的MD5,fileName为绝对路径
	public static string generateFileMD5(string fileName, bool upperOrLower = true)
	{
		// 安卓平台下容易oom,所以调用java的函数通过流的方式来计算md5
#if !UNITY_EDITOR && UNITY_ANDROID
		string md5 = AndroidAssetLoader.generateMD5(fileName);
		if (md5 == null)
		{
			return EMPTY;
		}
		if (upperOrLower)
		{
			return md5.ToUpper();
		}
		else
		{
			return md5.ToLower();
		}
#else
		byte[] fileContent = openFile(fileName, true);
		if (fileContent == null)
		{
			return EMPTY;
		}
		return generateFileMD5(fileContent, fileContent.Length, upperOrLower);
#endif
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
	public static string generateFileMD5Thread(byte[] fileContent, int length = -1, bool upperOrLower = true)
	{
		if (length < 0)
		{
			length = fileContent.Length;
		}
		HashAlgorithm algorithm = MD5.Create();
		return bytesToHEXStringThread(algorithm.ComputeHash(fileContent, 0, length), 0, 0, false, upperOrLower);
	}
	// 打开一个文本文件进行处理,然后再写回文本文件
	public static void processFileLine(string fileName, Action<List<string>> process)
	{
		using (new ListScope<string>(out var fileLineList))
		{
			openTxtFileLines(fileName, out string[] fileLines);
			fileLineList.AddRange(fileLines);

			process(fileLineList);

			string fileTxt = stringsToString(fileLineList, "\r\n");
			writeTxtFile(fileName, fileTxt);
		}
	}
}