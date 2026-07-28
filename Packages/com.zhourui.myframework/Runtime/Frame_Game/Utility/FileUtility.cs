using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using static FrameBase;
using static FrameBaseDefine;
using static FrameBaseUtility;
using static StringUtility;

// 文件工具函数类
public class FileUtility
{
	public static void validPath(ref string path)
	{
		// 不以/结尾,则加上/
		if (!path.isEmpty() && path[^1] != '/')
		{
			path += "/";
		}
	}
	public static byte[] stringToBytes(string str, Encoding encoding = null)
	{
		if (str == null)
		{
			return null;
		}
		// 默认为UTF8
		return (encoding ?? Encoding.UTF8).GetBytes(str);
	}
	public static string bytesToString(byte[] bytes, Encoding encoding = null)
	{
		if (bytes == null)
		{
			return null;
		}
		if (bytes.Length == 0)
		{
			return "";
		}
		// 默认为UTF8
		return removeLastZero((encoding ?? Encoding.UTF8).GetString(bytes));
	}
	// 字节数组转换为字符串时,末尾可能会带有数字0,此时在字符串比较时会出现错误,所以需要移除字符串末尾的0
	public static string removeLastZero(string str)
	{
		int strLen = str.Length;
		int newLen = strLen;
		for (int i = 0; i < strLen; ++i)
		{
			if (str[i] == 0)
			{
				newLen = i;
				break;
			}
		}
		return str.startString(newLen);
	}
	// 打开一个二进制文件,fileName为绝对路径,返回值为文件长度
	// 使用完毕后需要使用releaseFile回收文件内存
	// 这里添加Sync后缀是为了避免跟EditorFileUtility命名冲突
	public static byte[] openFileSync(string fileName, bool errorIfNull)
	{
		byte[] fileBuffer = null;
		try
		{
			if (isEditor() || isIOS() || isWindows())
			{
				using FileStream fs = new(fileName, FileMode.Open, FileAccess.Read);
				if (fs == null)
				{
					if (errorIfNull)
					{
						logErrorBase("文件加载失败! : " + fileName);
					}
					return null;
				}
				int fileSize = (int)fs.Length;
				fileBuffer = new byte[fileSize];
				fs.Read(fileBuffer, 0, fileSize);
				return fileBuffer;
			}
			else if (isAndroid())
			{
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
					logErrorBase("openFile invalid path : " + fileName);
				}
				if (fileBuffer == null && errorIfNull)
				{
					logErrorBase("open file failed! filename : " + fileName);
				}
				return fileBuffer;
			}
			else if (isWebGL())
			{
				if (isByteDance())
				{
					return TTFileSystem.readBytes(fileName);
				}
				else if (isWeiXin())
				{
					return WeChatFileSystem.readBytes(fileName);
				}
				else
				{
					logErrorBase("not supported openFileSync");
				}
			}
		}
		catch (Exception e)
		{
			if (errorIfNull)
			{
				logExceptionBase(e, "文件加载失败! : " + fileName);
			}
		}
		return null;
	}
	public static void openTxtFileAsync(string fileName, bool errorIfNull, StringCallback callback)
	{
		openFileAsync(fileName, errorIfNull, (byte[] bytes) =>
		{
			callback?.Invoke(bytesToString(bytes));
		});
	}
	// fileName为绝对路径
	public static void openFileAsync(string fileName, bool errorIfNull, BytesCallback callback)
	{
		if (!isFileExist(fileName))
		{
			if (errorIfNull)
			{
				logErrorBase("file not eixst:" + fileName);
			}
			callback?.Invoke(null);
			return;
		}
		GameEntryBase.startCoroutine(openFileAsyncInternal(fileName, errorIfNull, callback));
	}
	// fileNameList为绝对路径
	public static void openFileListAsync(List<string> fileNameList, bool errorIfNull, StringBytesCallback callback)
	{
		GameEntryBase.startCoroutine(openFileListAsyncInternal(fileNameList, errorIfNull, callback));
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
			yield return openFileAsyncInternal(fileName, errorIfNull, (byte[] bytes) =>
			{
				callback?.Invoke(fileName, bytes);
			});
		}
	}
	// 写一个文本文件,fileName为绝对路径,content是写入的字符串
	public static void writeFile(string fileName, byte[] buffer, int size, bool appendData = false)
	{
		createDir(getFilePath(fileName));
		// 检测路径是否存在,如果不存在就创建一个
		if (isEditor() || isIOS() || isWindows())
		{
			using FileStream file = new(fileName, appendData ? FileMode.Append : FileMode.Create, FileAccess.Write);
			if (buffer != null && buffer.Length > 0 && size > 0)
			{
				file.Write(buffer, 0, size);
			}
			file.Flush();
		}
		else if (isAndroid())
		{
			AndroidAssetLoader.writeFile(fileName, buffer, size, appendData);
		}
		else if (isWebGL())
		{
			if (buffer.Length > size)
			{
				logWarningBase("写入的文件长度小于buffer的长度,将会产生一次复制和GC");
				byte[] buffer0 = new byte[size];
				Buffer.BlockCopy(buffer, 0, buffer0, 0, buffer0.Length);
				buffer = buffer0;
			}
			if (isByteDance())
			{
				TTFileSystem.writeBytes(fileName, buffer);
			}
			else if (isWeiXin())
			{
				WeChatFileSystem.writeBytes(fileName, buffer);
			}
			else
			{
				logErrorBase("not supported writeFile");
			}
		}
	}
	// 删除文件,参数为绝对路径
	public static bool deleteFile(string path)
	{
		path = path.Replace('\\', '/');
		if (!isFileExist(path))
		{
			return false;
		}
		try
		{
			if (isEditor() || isIOS() || isWindows())
			{
				File.Delete(path);
			}
			else if (isAndroid())
			{
				return AndroidAssetLoader.deleteFile(path);
			}
			else if (isWebGL())
			{
				if (isByteDance())
				{
					TTFileSystem.deleteFile(path);
				}
				else if (isWeiXin())
				{
					WeChatFileSystem.deleteFile(path);
				}
				else
				{
					logErrorBase("not supported deleteFile");
				}
			}
		}
		catch (Exception e)
		{
			logWarningBase(e.Message);
			return false;
		}
		return true;
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
				writeFile(dest, bytes, bytes.Length);
			}
			doneCallback?.Invoke();
		});
	}
	// 目录是否存在,dir是绝对路径
	public static bool isDirExist(string dir)
	{
		if (dir.isEmpty() || dir == "./" || dir == "../")
		{
			return true;
		}
		if (dir[^1] != '/')
		{
			dir += "/";
		}
		if (isEditor() || isIOS() || isWindows())
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
			logErrorBase("isDirExist invalid path : " + dir);
			return false;
		}
		else if (isWebGL())
		{
			// 由于小游戏的接口本身无法访问StreamingAssets,所以只要是StreamingAssets中的就直接返回true
			if (dir.startWith(F_STREAMING_ASSETS_PATH))
			{
				return true;
			}
			if (isByteDance())
			{
				return TTFileSystem.isDirectoryExist(dir);
			}
			else if (isWeiXin())
			{
				return WeChatFileSystem.isDirectoryExist(dir);
			}
			else
			{
				logErrorBase("not supported isDirExist");
			}
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
		if (isEditor() || isIOS() || isWindows())
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
			logErrorBase("isFileExist invalid path : " + fileName);
			return false;
		}
		else if (isWebGL())
		{
			// 由于小游戏的接口本身无法访问StreamingAssets,所以只要是StreamingAssets中的就直接返回true
			if (fileName.startWith(F_STREAMING_ASSETS_PATH))
			{
				return true;
			}
			if (isByteDance())
			{
				return TTFileSystem.isFileExist(fileName);
			}
			else if (isWeiXin())
			{
				return WeChatFileSystem.isFileExist(fileName);
			}
			else
			{
				logErrorBase("not supported isFileExist");
			}
		}
		return false;
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
		if (isEditor() || isIOS() || isWindows())
		{
			Directory.CreateDirectory(dir);
		}
		else if (isAndroid())
		{
			AndroidAssetLoader.createDirectoryRecursive(dir);
		}
		else if (isWebGL())
		{
			if (isByteDance())
			{
				TTFileSystem.createDirectory(dir);
			}
			else if (isWeiXin())
			{
				WeChatFileSystem.createDirectory(dir);
			}
			else
			{
				logErrorBase("not supported createDir");
			}
		}
	}
	// 查找指定目录下的所有文件,path为绝对路径
	public static void findFilesInternal(string path, List<string> fileList, List<string> patterns, bool recursive)
	{
		try
		{
			if (isEditor() || isIOS() || isWindows())
			{
				validPath(ref path);
				if (!isDirExist(path))
				{
					return;
				}
				DirectoryInfo folder = new(path);
				int patternCount = patterns.count();
				foreach (FileInfo info in folder.GetFiles())
				{
					string fileName = info.Name;
					// 如果需要过滤后缀名,则判断后缀
					if (patternCount > 0)
					{
						foreach (string pattern in patterns)
						{
							if (fileName.endWith(pattern, false))
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
					foreach (string dir in Directory.GetDirectories(path))
					{
						findFilesInternal(dir, fileList, patterns, recursive);
					}
				}
			}
			if (isAndroid())
			{
				AndroidAssetLoader.findFiles(path, fileList, patterns, recursive);
			}
			else if (isWebGL())
			{
				logErrorBase("not supported findFilesInternal");
			}
		}
		// 此处暂时不抛出异常信息
		catch (Exception e)
		{
			if (isEditor())
			{
				logBase(e.Message);
			}
		}
	}
	public static void generateMD5List(List<string> fileNameList, StringListCallback callback)
	{
		if (callback == null)
		{
			return;
		}
		if (isEditor() || isWindows() || isWebGL())
		{
			GameEntryBase.startCoroutine(generateMD5ListAsyncInternal(fileNameList, callback));
		}
		else if (isIOS())
		{
			callback?.Invoke(generateMD5ListInternalIOS(fileNameList));
		}
		// 安卓平台下容易oom,所以调用java的函数通过流的方式来计算md5
		else if (isAndroid())
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
	}
	// 计算一个文件的MD5
	public static string generateFileMD5(byte[] fileContent, int length = -1)
	{
		if (fileContent == null || fileContent.Length == 0)
		{
			return "";
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
			logErrorBase(e.Message);
		}
		return bytesToHEXString(md5Bytes);
	}
	// AES 解密
	public static byte[] decryptAES(byte[] data, byte[] key, byte[] iv)
	{
		if (key == null || key.Length == 0 || iv == null || iv.Length == 0)
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
	// 查找指定目录下的所有文件,path为StreamingAssets下的相对路径,返回的路径列表为绝对路径
	public static void findStreamingAssetsFiles(string path, List<string> fileList, List<string> patterns = null, bool recursive = true, bool keepAbsolutePath = false)
	{
		if (isEditor() || isIOS() || isWindows())
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
		else if (isAndroid())
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
		else if (isWebGL())
		{
			logErrorBase("not supported findFilesInternal");
		}
	}
	// 写一个文本文件,fileName为绝对路径,content是写入的字符串
	public static void writeTxtFile(string fileName, string content, bool appendData = false)
	{
		if (isEditor() || isIOS() || isWindows() || isWebGL())
		{
			byte[] bytes = stringToBytes(content);
			if (bytes != null)
			{
				writeFile(fileName, bytes, bytes.Length, appendData);
			}
		}
		else if (isAndroid())
		{
			// 检测路径是否存在,如果不存在就创建一个
			createDir(getFilePath(fileName));
			AndroidAssetLoader.writeTxtFile(fileName, content, appendData);
		}
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
			if (!remoteInfoList.TryGetValue(item.Key, out GameFileInfo remoteInfo) ||
				remoteInfo.mMD5 != item.Value.mMD5)
			{
				deleteList.Add(item.Key);
			}
		}
	}
	// 筛选出新增和内容被修改的文件
	public static void checkNeedDownloadFile(List<string> modifyList, Dictionary<string, GameFileInfo> streamingInfoList, Dictionary<string, GameFileInfo> persistentInfoList, Dictionary<string, GameFileInfo> remoteInfoList, List<string> ignorePathList)
	{
		// 新增文件和已修改文件都认为是已修改文件
		// 如果不在本地文件列表中,则是新增的文件,在本地文件中,但是大小或MD5不同,则是已修改的文件
		// 遍历远端文件列表
		foreach (var item in remoteInfoList)
		{
			GameFileInfo remoteInfo = item.Value;
			// 动态下载目录中的文件不需要下载
			bool isIgnoreFile = false;
			if (ignorePathList != null)
			{
				foreach (string ignorePath in ignorePathList)
				{
					if (remoteInfo.mFileName.StartsWith(ignorePath.ToLower()))
					{
						isIgnoreFile = true;
						break;
					}
				}
			}
			if (isIgnoreFile)
			{
				continue;
			}

			GameFileInfo streamInfo = streamingInfoList.get(remoteInfo.mFileName);
			GameFileInfo persistentInfo = persistentInfoList.get(remoteInfo.mFileName);
			// streaming或者persistent中的大小和md5都与远端一致,则不需要下载
			if ((persistentInfo != null && persistentInfo.mFileSize == remoteInfo.mFileSize && persistentInfo.mMD5 == remoteInfo.mMD5) ||
				(streamInfo != null && streamInfo.mFileSize == remoteInfo.mFileSize && streamInfo.mMD5 == remoteInfo.mMD5))
			{
				continue;
			}

			modifyList.Add(remoteInfo.mFileName);
			// 打印出需要下载的原因,用于调试
			if (streamInfo == null && persistentInfo == null)
			{
				logBase("文件在本地不存在:" + remoteInfo.mFileName);
			}
			else if (streamInfo == null && persistentInfo != null)
			{
				if (persistentInfo.mFileSize != remoteInfo.mFileSize)
				{
					logBase("文件:" + remoteInfo.mFileName + "与远端大小不一致,远端:" + remoteInfo.mFileSize + ", persistent本地:" + persistentInfo.mFileSize);
				}
				if (persistentInfo.mMD5 != remoteInfo.mMD5)
				{
					logBase("文件:" + remoteInfo.mFileName + "与远端MD5不一致,远端:" + remoteInfo.mMD5 + ", persistent本地:" + persistentInfo.mMD5);
				}
			}
			else if (streamInfo != null && persistentInfo == null)
			{
				if (streamInfo.mFileSize != remoteInfo.mFileSize)
				{
					logBase("文件:" + remoteInfo.mFileName + "与远端大小不一致,远端:" + remoteInfo.mFileSize + ", StreamingAssets本地:" + streamInfo.mFileSize);
				}
				if (streamInfo.mMD5 != remoteInfo.mMD5)
				{
					logBase("文件:" + remoteInfo.mFileName + "与远端MD5不一致,远端:" + remoteInfo.mMD5 + ", StreamingAssets本地:" + streamInfo.mMD5);
				}
			}
			else if (streamInfo != null && persistentInfo != null)
			{
				if (persistentInfo.mFileSize != remoteInfo.mFileSize)
				{
					logBase("文件:" + remoteInfo.mFileName + "与远端大小不一致,远端:" + remoteInfo.mFileSize + ", persistent本地:" + persistentInfo.mFileSize);
				}
				if (persistentInfo.mMD5 != remoteInfo.mMD5)
				{
					logBase("文件:" + remoteInfo.mFileName + "与远端MD5不一致,远端:" + remoteInfo.mMD5 + ", persistent本地:" + persistentInfo.mMD5);
				}
				if (streamInfo.mFileSize != remoteInfo.mFileSize)
				{
					logBase("文件:" + remoteInfo.mFileName + "与远端大小不一致,远端:" + remoteInfo.mFileSize + ", StreamingAssets本地:" + streamInfo.mFileSize);
				}
				if (streamInfo.mMD5 != remoteInfo.mMD5)
				{
					logBase("文件:" + remoteInfo.mFileName + "与远端MD5不一致,远端:" + remoteInfo.mMD5 + ", StreamingAssets本地:" + streamInfo.mMD5);
				}
			}
		}
	}
	public static void writeFileList(string path, string content)
	{
		writeTxtFile(path + FILE_LIST, content);
	}
	// 获得一个合适的文件加载路径,fileName是StreamingAssets下的相对路径,带后缀
	public static string availableReadPath(string fileName)
	{
		if (isEditor())
		{
			// 编辑器中从StreamingAssets读取
			return F_ASSET_BUNDLE_PATH + fileName;
		}
		else
		{
			// 非编辑器中时,根据文件对比结果来判断从哪儿加载
			return mAssetVersionSystem.getFileReadPath(fileName);
		}
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected static IEnumerator generateMD5ListAsyncInternal(List<string> fileNameList, StringListCallback callback)
	{
		List<string> md5List = new();
		int count = fileNameList.Count;
		for (int i = 0; i < count; ++i)
		{
			md5List.Add("");
		}
		for (int i = 0; i < count; ++i)
		{
			int index = i;
			yield return openFileAsyncInternal(fileNameList[i], true, (byte[] bytes) =>
			{
				md5List[index] = generateFileMD5(bytes);
			});
		}
		try
		{
			callback?.Invoke(md5List);
		}
		catch (Exception e)
		{
			logExceptionBase(e);
		}
	}
	protected static byte[] openFileIOS(string fileName)
	{
		using FileStream fs = new(fileName, FileMode.Open, FileAccess.Read);
		if (fs == null)
		{
			logErrorBase("文件加载失败! : " + fileName);
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
		for (int i = 0; i < count; ++i)
		{
			md5List.Add("");
		}
		for (int i = 0; i < count; ++i)
		{
			md5List[i] = generateFileMD5(openFileIOS(fileNameList[i]));
		}
		return md5List;
	}
}