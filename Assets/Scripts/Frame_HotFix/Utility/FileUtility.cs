using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEngine.Networking;
using static UnityUtility;
using static StringUtility;
using static BinaryUtility;
using static FrameDefine;
using static FrameBaseHotFix;
using static FrameBaseUtility;
using static FrameBaseDefine;

// 文件工具函数类
public class FileUtility
{
	private static List<string> mTempPatternList = new();   // 用于避免GC
	private static List<string> mTempFileList = new();      // 用于避免GC
	private static List<string> mTempFileList1 = new();     // 用于避免GC
	private static byte[] BOM = new byte[] { 0xEF, 0xBB, 0xBF };	// UTF8的BOM头
	public static void validPath(ref string path)
	{
		// 不以/结尾,则加上/
		if (!path.isEmpty() && path[^1] != '/')
		{
			path += "/";
		}
	}
	// fileNameList为绝对路径
	public static IEnumerator openFileListAsyncInternal(List<string> fileNameList, bool errorIfNull, StringBytesCallback callback)
	{
		int count = fileNameList.Count;
		if (count == 0)
		{
			callback?.Invoke(null, null);
			yield break;
		}
		for (int i = 0; i < count; ++i)
		{
			string fileName = fileNameList[i];
			checkDownloadPath(ref fileName);
			using var www = UnityWebRequest.Get(fileName);
			yield return www.SendWebRequest();
			if (errorIfNull && www.downloadHandler.data == null)
			{
				logError("open file failed:" + fileName + ", info:" + www.error);
			}
			callback?.Invoke(fileNameList[i], www.downloadHandler.data);
		}
	}
	// fileName为绝对路径
	public static IEnumerator openFileAsyncInternal(string fileName, bool errorIfNull, BytesCallback callback)
	{
		if (fileName.isEmpty())
		{
			callback?.Invoke(null);
			yield break;
		}
		DateTime start = DateTime.Now;
		checkDownloadPath(ref fileName);
		using var www = UnityWebRequest.Get(fileName);
		yield return www.SendWebRequest();
		if (errorIfNull && www.downloadHandler.data == null)
		{
			logError("open file failed:" + fileName + ", info:" + www.error + ", error:" + www.downloadHandler.error);
		}
		log("打开文件耗时:" + (int)(DateTime.Now - start).TotalMilliseconds + "毫秒,file:" + fileName);
		try
		{
			callback?.Invoke(www.downloadHandler.data);
		}
		catch (Exception e)
		{
			logException(e);
		}
	}
	// fileName为绝对路径
	public static void openFileAsync(string fileName, bool errorIfNull, BytesCallback callback)
	{
		if (!isWebGL() && !isFileExist(fileName))
		{
			if (errorIfNull)
			{
				logError("file not exist:" + fileName);
			}
			callback?.Invoke(null);
			return;
		}
		GameEntry.startCoroutine(openFileAsyncInternal(fileName, errorIfNull, callback));
	}
	// fileNameList为绝对路径
	public static void openFileListAsync(List<string> fileNameList, bool errorIfNull, StringBytesCallback callback)
	{
		GameEntry.startCoroutine(openFileListAsyncInternal(fileNameList, errorIfNull, callback));
	}
	public static void openTxtFileAsync(string fileName, bool errorIfNull, StringCallback callback)
	{
		openFileAsync(fileName, errorIfNull, (byte[] bytes) =>
		{
			int offset = 0;
			if (bytes.Length >= BOM.Length &&
				bytes[0] == BOM[0] &&
				bytes[1] == BOM[1] &&
				bytes[2] == BOM[2])
			{
				offset = BOM.Length;
			}
			callback?.Invoke(bytesToString(bytes, offset, bytes.Length - offset));
		});
	}
	public static void openTxtFileLinesAsync(string fileName, bool errorIfNull, StringArrayCallback callback)
	{
		openFileAsync(fileName, errorIfNull, (byte[] bytes) =>
		{
			int offset = 0;
			if (bytes.Length >= BOM.Length &&
				bytes[0] == BOM[0] &&
				bytes[1] == BOM[1] &&
				bytes[2] == BOM[2])
			{
				offset = BOM.Length;
			}
			callback?.Invoke(splitLine(bytesToString(bytes, offset, bytes.Length - offset)));
		});
	}
	// 打开一个二进制文件,fileName为绝对路径,返回值为文件长度
	// 使用完毕后需要使用releaseFile回收文件内存
	// 这里添加Sync后缀是为了避免跟EditorFileUtility命名冲突
	public static byte[] openFileSync(string fileName, bool errorIfNull)
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
	public static string openTxtFileSync(string fileName, bool errorIfNull)
	{
		byte[] fileContent = openFileSync(fileName, errorIfNull);
		if (fileContent == null)
		{
			return EMPTY;
		}
		// 如果有BOM头,则需要去掉
		int offset = 0;
		if (fileContent.Length >= BOM.Length &&
			fileContent[0] == BOM[0] &&
			fileContent[1] == BOM[1] &&
			fileContent[2] == BOM[2])
		{
			offset = BOM.Length;
		}
		return bytesToString(fileContent, offset, fileContent.Length - offset);
	}
	// 打开一个文本文件,fileName为绝对路径,并且自动将文件拆分为多行,移除末尾的换行符(\r或者\n),存储在fileLines中,包含空行,返回值是行数
	public static int openTxtFileLinesSync(string fileName, out string[] fileLines, bool errorIfNull = true, bool keepEmptyLine = true)
	{
		string fileContent = openTxtFileSync(fileName, errorIfNull);
		if (fileContent.isEmpty())
		{
			fileLines = null;
			return 0;
		}
		splitLine(fileContent, out fileLines, !keepEmptyLine);
		return fileLines.count();
	}
	// 打开一个文本文件,fileName为绝对路径,并且自动将文件拆分为多行,移除末尾的换行符(\r或者\n),存储在fileLines中,包含空行,返回值是行数
	public static string[] openTxtFileLinesSync(string fileName, bool errorIfNull = true, bool keepEmptyLine = true)
	{
		openTxtFileLinesSync(fileName, out string[] lines, errorIfNull, keepEmptyLine);
		return lines;
	}
	public static void writeFile(string fileName, byte[] buffer)
	{
		writeFile(fileName, buffer, buffer.Length, false);
	}
	public static void writeAppendFile(string fileName, byte[] buffer)
	{
		writeAppendFile(fileName, buffer, buffer.Length);
	}
	// 写一个文本文件,fileName为绝对路径,content是写入的字符串
	public static void writeFile(string fileName, byte[] buffer, int size, bool addBOM)
	{
		if (fileName == null)
		{
			return;
		}
		// 检测路径是否存在,如果不存在就创建一个
		createDir(getFilePath(fileName));
		if (isEditor() || !isAndroid())
		{
			using FileStream file = new(fileName, FileMode.Create, FileAccess.Write);
			if (!buffer.isEmpty() && size > 0)
			{
				if (addBOM)
				{
					file.Write(BOM, 0, BOM.Length);
				}
				file.Write(buffer, 0, size);
			}
			file.Flush();
		}
		else
		{
			AndroidAssetLoader.writeFile(fileName, buffer, size, false);
		}
	}
	public static void writeAppendFile(string fileName, byte[] buffer, int size)
	{
		if (fileName == null)
		{
			return;
		}
		// 检测路径是否存在,如果不存在就创建一个
		createDir(getFilePath(fileName));
		if (isEditor() || !isAndroid())
		{
			using FileStream file = new(fileName, FileMode.Append, FileAccess.Write);
			if (!buffer.isEmpty() && size > 0)
			{
				file.Write(buffer, 0, size);
			}
			file.Flush();
		}
		else
		{
			AndroidAssetLoader.writeFile(fileName, buffer, size, true);
		}
	}
	// 写一个文本文件,fileName为绝对路径,content是写入的字符串
	public static void writeTxtFile(string fileName, string content, bool addBOM = false)
	{
		if (isEditor() || isIOS() || isWindows() || isWebGL())
		{
			byte[] bytes = stringToBytes(content);
			if (bytes != null)
			{
				writeFile(fileName, bytes, bytes.Length, addBOM);
			}
		}
		else if (isAndroid())
		{
			// 检测路径是否存在,如果不存在就创建一个
			createDir(getFilePath(fileName));
			AndroidAssetLoader.writeTxtFile(fileName, content, false);
		}
	}
	// 重命名文件,参数为绝对路径
	public static bool renameFile(string fileName, string newName)
	{
		if (!isEditor() && isAndroid())
		{
			logError("can not rename file on android!");
			return false;
		}
		if (!isFileExist(fileName) || isFileExist(newName))
		{
			return false;
		}
		try
		{
			Directory.Move(fileName, newName);
		}
		catch(Exception e)
		{
			logException(e, "fileName:" + fileName + ", newName:" + newName);
		}
		return true;
	}
	// 删除目录,参数为绝对路径
	public static void deleteFolder(string path)
	{
		if (!isEditor() && isAndroid())
		{
			if (!AndroidAssetLoader.deleteDirectory(path))
			{
				logWarning("删除目录失败:" + path);
			}
			return;
		}
		validPath(ref path);
		if (!isDirExist(path))
		{
			return;
		}
		// 先删除所有文件夹
		foreach (string dir in Directory.GetDirectories(path))
		{
			deleteFolder(dir);
		}
		// 再删除所有文件
		foreach (string file in Directory.GetFiles(path))
		{
			deleteFile(file);
		}
		// 再删除文件夹自身
		Directory.Delete(path);
	}
	// 删除path中的所有空目录,参数为绝对路径,deleteSelfIfEmpty表示path本身为空时是否需要删除
	public static bool deleteEmptyFolder(string path, bool deleteSelfIfEmpty = true)
	{
		if (!isEditor() && isAndroid())
		{
			logError("can not delete empty dir on android!");
			return false;
		}
		validPath(ref path);
		// 先删除所有空的文件夹
		bool isEmpty = true;
		foreach (string dir in Directory.GetDirectories(path))
		{
			isEmpty = deleteEmptyFolder(dir, true) && isEmpty;
		}
		isEmpty = isEmpty && Directory.GetFiles(path).Length == 0;
		if (isEmpty && deleteSelfIfEmpty)
		{
			Directory.Delete(path);
			File.Delete(removeEndSlash(path) + ".meta");
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
		if (!isEditor() && isAndroid())
		{
			logError("can not move file on android!");
			return;
		}
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
	// 拷贝文件,参数为绝对路径
	public static void copyFileAsync(string source, string dest, Action doneCallback)
	{
		openFileAsync(source, true, (byte[] bytes) =>
		{
			if (bytes != null)
			{
				// 如果目标文件所在的目录不存在,则先创建目录
				createDir(getFilePath(dest));
				writeFile(dest, bytes, bytes.Length, false);
			}
			doneCallback?.Invoke();
		});
	}
	// 删除文件,参数为绝对路径
	public static bool deleteFile(string path)
	{
		path = path.rightToLeft();
		if (!isFileExist(path))
		{
			return false;
		}
		try
		{
			if (isEditor())
			{
				File.Delete(path);
			}
			else if (isAndroid())
			{
				return AndroidAssetLoader.deleteFile(path);
			}
			else if (isIOS())
			{
				File.Delete(path);
			}
			else if (isWindows())
			{
				File.Delete(path);
			}
			else if (isWebGL())
			{
				File.Delete(path);
			}
		}
		catch (Exception e)
		{
			logWarning(e.Message);
			return false;
		}
		return true;
	}
	// 获得文件大小,file为绝对路径
	public static int getFileSize(string file)
	{
		file = file.rightToLeft();
		try
		{
			if (isEditor())
			{
				return (int)new FileInfo(file).Length;
			}
			else if (isAndroid())
			{
				return AndroidAssetLoader.getFileSize(file);
			}
			else if (isIOS())
			{
				return (int)new FileInfo(file).Length;
			}
			else if (isWindows())
			{
				return (int)new FileInfo(file).Length;
			}
			else if (isWebGL())
			{
				return (int)new FileInfo(file).Length;
			}
			return 0;
		}
		catch (Exception e)
		{
			logException(e, "getFileSize error! file:" + file);
			return 0;
		}
	}
	// 目录是否存在,dir是绝对路径
	public static bool isDirExist(string dir)
	{
		if (dir.isEmpty() || dir == "./" || dir == "../")
		{
			return true;
		}
		validPath(ref dir);
		if (isEditor())
		{
			return Directory.Exists(dir);
		}
		else if (isAndroid())
		{
			// 安卓平台如果要读取StreamingAssets下的文件,只能使用AssetManager
			if (dir.startWith(F_STREAMING_ASSETS_PATH))
			{
				// 改为相对路径
				return AndroidAssetLoader.isAssetExist(dir.removeStartCount(F_STREAMING_ASSETS_PATH.Length));
			}
			// 安卓平台如果要读取persistentDataPath的文件,则可以使用File
			if (dir.startWith(F_PERSISTENT_DATA_PATH))
			{
				return AndroidAssetLoader.isDirExist(dir);
			}
			logError("isDirExist invalid path : " + dir);
			return false;
		}
		else if (isIOS())
		{
			return Directory.Exists(dir);
		}
		else if (isWindows())
		{
			return Directory.Exists(dir);
		}
		else if (isWebGL())
		{
			return Directory.Exists(dir);
		}
		return false;
	}
	// 文件是否存在,fileName为绝对路径
	public static bool isFileExist(string fileName)
	{
		if (fileName.isEmpty())
		{
			return false;
		}
		if (isEditor())
		{
			return File.Exists(fileName);
		}
		else if (isAndroid())
		{
			// 安卓平台如果要读取StreamingAssets下的文件,只能使用AssetManager
			if (fileName.startWith(F_STREAMING_ASSETS_PATH))
			{
				// 改为相对路径
				return AndroidAssetLoader.isAssetExist(fileName.removeStartCount(F_STREAMING_ASSETS_PATH.Length));
			}
			// 安卓平台如果要读取persistentDataPath的文件,则可以使用File
			if (fileName.startWith(F_PERSISTENT_DATA_PATH))
			{
				return AndroidAssetLoader.isFileExist(fileName);
			}
			logError("isFileExist invalid path : " + fileName);
			return false;
		}
		else if (isIOS())
		{
			return File.Exists(fileName);
		}
		else if (isWindows())
		{
			return File.Exists(fileName);
		}
		else if (isWebGL())
		{
			return File.Exists(fileName);
		}
		return false;
	}
	// 创建文件夹,dir绝对路径
	public static void createDir(string dir)
	{
		if (dir.isEmpty())
		{
			return;
		}
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
		if (isEditor())
		{
			Directory.CreateDirectory(dir);
		}
		else if (isAndroid())
		{
			AndroidAssetLoader.createDirectoryRecursive(dir);
		}
		else if (isIOS())
		{
			Directory.CreateDirectory(dir);
		}
		else if (isWindows())
		{
			Directory.CreateDirectory(dir);
		}
		else if (isWebGL())
		{
			Directory.CreateDirectory(dir);
		}
	}
	// 查找指定目录下的所有文件,path为GameResources下的相对路径
	public static List<string> findResourcesFilesNonAlloc(string path, string pattern, bool recursive = true, bool keepAbsolutePath = false)
	{
		mTempPatternList.Clear();
		mTempPatternList.addNotEmpty(pattern);
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
		if (!isEditor() && isAndroid())
		{
			logError("can not find resources files on android!");
			return;
		}
		mTempPatternList.Clear();
		mTempPatternList.addNotEmpty(pattern);
		findResourcesFiles(path, fileList, mTempPatternList, recursive);
	}
	// 查找指定目录下的所有文件,path为GameResources下的相对路径
	public static void findResourcesFiles(string path, List<string> fileList, List<string> patterns = null, bool recursive = true, bool keepAbsolutePath = false)
	{
		if (!isEditor() && isAndroid())
		{
			logError("can not find resources files on android!");
			return;
		}
		validPath(ref path);
		path = path.ensurePrefix(F_GAME_RESOURCES_PATH);
		findFilesInternal(path, fileList, patterns, recursive);
		if (!keepAbsolutePath)
		{
			int removeLength = F_GAME_RESOURCES_PATH.Length;
			int count = fileList.Count;
			for (int i = 0; i < count; ++i)
			{
				fileList[i] = fileList[i].removeStartCount(removeLength);
			}
		}
	}
	// 查找指定目录下的所有文件,path为StreamingAssets下的相对路径
	public static List<string> findStreamingAssetsFilesNonAlloc(string path, string pattern, bool recursive = true, bool keepAbsolutePath = false)
	{
		mTempPatternList.Clear();
		mTempPatternList.addNotEmpty(pattern);
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
		mTempPatternList.addNotEmpty(pattern);
		findStreamingAssetsFiles(path, fileList, mTempPatternList, recursive, keepAbsolutePath);
	}
	// 查找指定目录下的所有文件,path为StreamingAssets下的相对路径,返回的路径列表为绝对路径
	public static void findStreamingAssetsFiles(string path, List<string> fileList, List<string> patterns = null, bool recursive = true, bool keepAbsolutePath = false)
	{
		if (!isEditor() && isAndroid())
		{
			// 转换为相对路径
			path = path.removeStartString(F_STREAMING_ASSETS_PATH);
			AndroidAssetLoader.findAssets(path, fileList, patterns, recursive);
			// 查找后的路径本身就是相对路径,如果需要保留绝对路径,则需要将路径加上
			if (keepAbsolutePath)
			{
				int count = fileList.Count;
				for (int i = 0; i < count; ++i)
				{
					fileList[i] = F_STREAMING_ASSETS_PATH + fileList[i];
				}
			}
		}
		else
		{
			path = path.ensurePrefix(F_STREAMING_ASSETS_PATH);
			findFilesInternal(path, fileList, patterns, recursive);
			if (!keepAbsolutePath)
			{
				int removeLength = F_STREAMING_ASSETS_PATH.Length;
				int count = fileList.Count;
				for (int i = 0; i < count; ++i)
				{
					fileList[i] = fileList[i].removeStartCount(removeLength);
				}
			}
		}
	}
	// 查找指定目录下的所有目录,path为StreamingAssets下的相对路径,返回的路径列表为绝对路径
	public static void findStreamingAssetsFolders(string path, List<string> folderList, bool recursive = true, bool keepAbsolutePath = false)
	{
		if (!isEditor() && isAndroid())
		{
			// 转换为相对路径
			path = path.removeStartString(F_STREAMING_ASSETS_PATH);
			AndroidAssetLoader.findAssetsFolder(path, folderList, recursive);
			// 查找后的路径本身就是相对路径,如果需要保留绝对路径,则需要将路径加上
			if (keepAbsolutePath)
			{
				int count = folderList.Count;
				for (int i = 0; i < count; ++i)
				{
					folderList[i] = F_STREAMING_ASSETS_PATH + folderList[i];
				}
			}
		}
		else
		{
			// 非安卓平台则查找普通的文件夹
			path = path.ensurePrefix(F_STREAMING_ASSETS_PATH);
			findFolders(path, folderList, null, recursive);
			if (!keepAbsolutePath)
			{
				int removeLength = F_STREAMING_ASSETS_PATH.Length;
				int count = folderList.Count;
				for (int i = 0; i < count; ++i)
				{
					folderList[i] = folderList[i].removeStartCount(removeLength);
				}
			}
		}
	}
	// 查找指定目录下的所有文件,path为绝对路径
	public static List<string> findFilesNonAlloc(string path, string pattern, bool recursive = true)
	{
		mTempPatternList.Clear();
		mTempPatternList.addNotEmpty(pattern);
		mTempFileList1.Clear();
		findFilesInternal(path, mTempFileList1, mTempPatternList, recursive);
		return mTempFileList1;
	}
	// 查找指定目录下的所有文件,path为绝对路径
	public static List<string> findFilesNonAlloc(string path, IList<string> patterns = null, bool recursive = true)
	{
		mTempFileList1.Clear();
		findFilesInternal(path, mTempFileList1, patterns, recursive);
		return mTempFileList1;
	}
	public static void findFiles(string path, List<string> fileList, List<string> patterns)
	{
		findFilesInternal(path, fileList, patterns, true);
	}
	public static void findFiles(string path, List<string> fileList, string pattern, bool recursive = true)
	{
		if (isMainThread())
		{
			mTempPatternList.Clear();
			mTempPatternList.addNotEmpty(pattern);
			findFilesInternal(path, fileList, mTempPatternList, recursive);
		}
		else
		{
			findFilesInternal(path, fileList, new List<string>() { pattern }, true);
		}
	}
	public static void findFiles(string path, List<string> fileList, bool recursive = true)
	{
		findFilesInternal(path, fileList, null, recursive);
	}
	// 查找指定目录下的所有文件,path为绝对路径
	public static void findFilesInternal(string path, List<string> fileList, IList<string> patterns, bool recursive)
	{
		try
		{
			if (!isEditor() && isAndroid())
			{
				AndroidAssetLoader.findFiles(path, fileList, patterns, recursive);
			}
			else
			{
				validPath(ref path);
				if (!isDirExist(path))
				{
					return;
				}
				DirectoryInfo folder = new(path);
				foreach (FileInfo info in folder.GetFiles())
				{
					string fileName = info.Name;
					// 不需要过滤,则直接放入列表
					fileList.addIf(path + fileName, patterns.isEmpty());
					// 如果需要过滤后缀名,则判断后缀
					foreach (string pattern in patterns.safe())
					{
						fileList.addIf(path + fileName, fileName.endWith(pattern, false));
					}
				}
				// 查找所有子目录
				if (recursive)
				{
					foreach (string dir in Directory.GetDirectories(path))
					{
						findFilesInternal(dir, fileList, patterns, recursive);
					}
				}
			}
		}
		// 此处暂时不抛出异常信息
		catch (Exception e)
		{
			if (isEditor())
			{
				log(e.Message);
			}
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
			if (!isEditor() && isAndroid())
			{
				AndroidAssetLoader.findFolders(path, dirList, recursive);
			}
			else
			{
				foreach (string dir in Directory.GetDirectories(path))
				{
					if (excludeList.contains(getFileNameThread(dir)))
					{
						continue;
					}
					dirList.Add(dir);
					if (recursive)
					{
						findFolders(dir, dirList, excludeList, recursive);
					}
				}
			}
		}
		// 只捕获异常,暂时不抛出报错信息
		catch (Exception e)
		{
			if (isEditor())
			{
				log(e.Message);
			}
		}
		return true;
	}
	// 计算一个文件的MD5,fileName为绝对路径
	public static void generateFileMD5(string fileName, StringCallback callback)
	{
		if (callback == null)
		{
			return;
		}
		// 安卓平台下容易oom,所以调用java的函数通过流的方式来计算md5
		if (!isEditor() && isAndroid())
		{
			string md5 = AndroidAssetLoader.generateMD5(fileName);
			if (md5 == null)
			{
				return;
			}
			callback(md5.ToLower());
		}
		else
		{
			openFileAsync(fileName, true, (byte[] fileContent) =>
			{
				callback(generateFileMD5(fileContent, fileContent.Length));
			});
		}
	}
	public static void generateMD5List(List<string> fileNameList, StringListCallback callback)
	{
		if (callback == null)
		{
			return;
		}
		// 安卓平台下容易oom,所以调用java的函数通过流的方式来计算md5
		if (!isEditor() && isAndroid())
		{
			List<string> list = new();
			AndroidAssetLoader.generateMD5List(fileNameList, list);
			int count = list.Count;
			for (int i = 0; i < count; ++i)
			{
				list[i] = list[i].ToLower();
			}
			callback(list);
		}
		else
		{
			if (isIOS())
			{
				callback?.Invoke(generateMD5ListInternalIOS(fileNameList));
			}
			else
			{
				GameEntry.startCoroutine(generateMD5ListAsyncInternal(fileNameList, callback));
			}
		}
	}
	// 计算一个文件的MD5
	public static string generateFileMD5(byte[] fileContent, int length = -1)
	{
		if (fileContent.count() == 0)
		{
			return EMPTY;
		}
		if (length < 0)
		{
			length = fileContent.Length;
		}
		byte[] md5Bytes = null;
		try
		{
			using MD5CryptoServiceProvider md5Obj = new();
			md5Bytes = md5Obj.ComputeHash(fileContent, 0, length);
		}
		catch (Exception e)
		{
			logError(e.Message);
		}
		return bytesToHEXString(md5Bytes, 0, 0, false, false);
	}
	// 筛选出新增和内容被修改的文件
	public static List<string> checkNeedUploadFile(Dictionary<string, GameFileInfo> remoteList, Dictionary<string, GameFileInfo> localInfoList)
	{
		List<string> modifyList = new();
		checkNeedUploadFile(modifyList, remoteList, localInfoList);
		return modifyList;
	}
	// 筛选出新增和内容被修改的文件
	public static void checkNeedUploadFile(List<string> modifyList, Dictionary<string, GameFileInfo> remoteList, Dictionary<string, GameFileInfo> localInfoList)
	{
		// 新增文件和已修改文件都认为是已修改文件
		// 遍历本地文件列表
		foreach (GameFileInfo localInfo in localInfoList.Values)
		{
			// 如果不在远端文件列表中,则是新增的文件,在远端文件中,但是大小或MD5不同,则是已修改的文件
			if (!remoteList.TryGetValue(localInfo.mFileName, out GameFileInfo info) ||
				info.mFileSize != localInfo.mFileSize ||
				info.mMD5 != localInfo.mMD5)
			{
				modifyList.Add(localInfo.mFileName);
				if (remoteList.ContainsKey(localInfo.mFileName))
				{
					if (info.mFileSize != localInfo.mFileSize)
					{
						log("文件:" + localInfo.mFileName + "与本地大小不一致,本地大小:" + localInfo.mFileSize + ", 远端大小:" + info.mFileSize);
					}
					else if (info.mMD5 != localInfo.mMD5)
					{
						log("文件:" + localInfo.mFileName + "与本地MD5不一致,本地:" + localInfo.mMD5 + ", 远端:" + info.mMD5);
					}
				}
				else
				{
					log("文件在远端不存在:" + localInfo.mFileName);
				}
			}
		}
	}
	// 检查两个文件列表是否一致
	public static bool checkDiff(Dictionary<string, GameFileInfo> list0, Dictionary<string, GameFileInfo> list1, bool checkMD5)
	{
		if (list0.Count != list1.Count)
		{
			logError("file count not same, local count:" + list0.Count + ", remote count:" + list1.Count);
			return false;
		}
		foreach (var item in list0)
		{
			GameFileInfo info0 = item.Value;
			if (!list1.TryGetValue(item.Key, out GameFileInfo info1) ||
				info0.mFileSize != info1.mFileSize ||
				(checkMD5 && info0.mMD5 != info1.mMD5))
			{
				if (info1.mFileName == null)
				{
					logError("remote file missing:" + item.Key);
				}
				else if (info0.mFileSize != info1.mFileSize)
				{
					logError("file size not same:" + item.Key);
				}
				else if (info0.mMD5 != info1.mMD5)
				{
					logError("file md5 not same:" + item.Key);
				}
				return false;
			}
		}
		return true;
	}
	// AES 加密
	public static byte[] encryptAES(byte[] data, byte[] key, byte[] iv)
	{
		using Aes aesAlg = Aes.Create();
		aesAlg.Key = key;
		aesAlg.IV = iv;
		using MemoryStream msEncrypt = new();
		using CryptoStream csEncrypt = new(msEncrypt, aesAlg.CreateEncryptor(), CryptoStreamMode.Write);
		csEncrypt.Write(data, 0, data.Length);
		csEncrypt.FlushFinalBlock();
		return msEncrypt.ToArray();
	}
	// AES 解密
	public static byte[] decryptAES(byte[] data, byte[] key, byte[] iv)
	{
		if (key.isEmpty() || iv.isEmpty())
		{
			return data;
		}
		using Aes aesAlg = Aes.Create();
		aesAlg.Key = key;
		aesAlg.IV = iv;
		using MemoryStream msDecrypt = new(data);
		using CryptoStream csDecrypt = new(msDecrypt, aesAlg.CreateDecryptor(), CryptoStreamMode.Read);
		using MemoryStream msResult = new();
		csDecrypt.CopyTo(msResult);
		return msResult.ToArray();
	}
	// 筛选出需要删除的文件
	public static List<string> checkDeleteFile(Dictionary<string, GameFileInfo> localInfoList, Dictionary<string, GameFileInfo> remoteInfoList)
	{
		List<string> deleteList = new();
		checkDeleteFile(localInfoList, remoteInfoList, deleteList);
		return deleteList;
	}
	// 筛选出需要删除的文件
	public static void checkDeleteFile(Dictionary<string, GameFileInfo> localInfoList, Dictionary<string, GameFileInfo> remoteInfoList, List<string> deleteList)
	{
		// 遍历本地文件列表
		foreach (var item in localInfoList)
		{
			// 如果已经不在远端文件列表中,则是已删除的文件
			// 在本地,但是与远端文件不一致,也需要删除,因为动态下载的文件不会在启动时下载,所以为了避免加载到旧的文件,需要删除本地的旧文件,虽然在资源版本系统中已经判断了是否一致了
			if (!remoteInfoList.TryGetValue(item.Key, out GameFileInfo remoteInfo) || remoteInfo.mMD5 != item.Value.mMD5)
			{
				deleteList.Add(item.Key);
			}
		}
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected static IEnumerator generateMD5ListAsyncInternal(List<string> fileNameList, StringListCallback callback)
	{
		List<string> md5List = new();
		int count = fileNameList.Count;
		md5List.addCount(EMPTY, count);
		for (int i = 0; i < count; ++i)
		{
			string fileName = fileNameList[i];
			checkDownloadPath(ref fileName);
			using var www = UnityWebRequest.Get(fileName);
			yield return www.SendWebRequest();
			if (www.downloadHandler.data == null)
			{
				logError("open file failed:" + fileName + ", info:" + www.error);
			}
			md5List[i] = generateFileMD5(www.downloadHandler.data);
			yield return null;
		}
		try
		{
			callback?.Invoke(md5List);
		}
		catch (Exception e)
		{
			logException(e);
		}
	}
	protected static byte[] openFileIOS(string fileName)
	{
		using FileStream fs = new(fileName, FileMode.Open, FileAccess.Read);
		if (fs == null)
		{
			logError("文件加载失败! : " + fileName);
			return null;
		}
		int fileSize = (int)fs.Length;
		byte[] fileBuffer = new byte[fileSize];
		fs.Read(fileBuffer, 0, fileSize);
		return fileBuffer;
	}
	protected static List<string> generateMD5ListInternalIOS(List<string> fileNameList)
	{
		List<string> md5List = new();
		int count = fileNameList.Count;
		md5List.addCount(EMPTY, count);
		for (int i = 0; i < count; ++i)
		{
			md5List[i] = generateFileMD5(openFileIOS(fileNameList[i]));
		}
		return md5List;
	}
}