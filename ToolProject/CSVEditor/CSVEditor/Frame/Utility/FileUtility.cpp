#include "FrameHeader.h"

namespace FileUtility
{
	void validPath(string& path)
	{
		if (path.length() > 0)
		{
			// 不以/结尾,则加上/
			if (path[path.length() - 1] != '/')
			{
				path += "/";
			}
		}
	}

	void findFiles(const string& path, Vector<string>& files, const string* patterns, const int patternCount, const bool recursive)
	{
		string tempPath = path;
		validPath(tempPath);
		WIN32_FIND_DATAA FindFileData;
		const HANDLE hFind = FindFirstFileA((tempPath + "*").c_str(), &FindFileData);
		// 如果找不到文件夹就直接返回
		if (INVALID_HANDLE_VALUE == hFind)
		{
			return;
		}
		do
		{
			// 过滤.和..
			if (strcmp(FindFileData.cFileName, ".") == 0 ||
				strcmp(FindFileData.cFileName, "..") == 0)
			{
				continue;
			}

			// 构造完整路径
			const string fullname = tempPath + string(FindFileData.cFileName);
			if (FindFileData.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY)
			{
				if (recursive)
				{
					findFiles(fullname, files, patterns, patternCount, recursive);
				}
			}
			else
			{
				if (patternCount > 0)
				{
					FOR(patternCount)
					{
						if (endWith(fullname, patterns[i].c_str(), false))
						{
							files.push_back(fullname);
						}
					}
				}
				else
				{
					files.push_back(fullname);
				}
			}
		} while (FindNextFileA(hFind, &FindFileData));
		FindClose(hFind);
	}

	void findFolders(const string& path, Vector<string>& folders, const bool recursive)
	{
		string tempPath = path;
		validPath(tempPath);
		WIN32_FIND_DATAA FindFileData;
		const HANDLE hFind = FindFirstFileA((tempPath + "*").c_str(), &FindFileData);
		// 如果找不到文件夹就直接返回
		if (INVALID_HANDLE_VALUE == hFind)
		{
			return;
		}
		do
		{
			// 过滤.和..
			if (strcmp(FindFileData.cFileName, ".") == 0 ||
				strcmp(FindFileData.cFileName, "..") == 0)
			{
				continue;
			}

			// 构造完整路径
			const string fullname = tempPath + string(FindFileData.cFileName);
			// 是文件夹则先放入列表,然后判断是否递归查找
			if (FindFileData.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY)
			{
				folders.push_back(fullname);
				if (recursive)
				{
					findFolders(fullname, folders, recursive);
				}
			}
		} while (FindNextFileA(hFind, &FindFileData));
		FindClose(hFind);
	}

	void deleteFolder(const string& path)
	{
		string tempPath = path;
		validPath(tempPath);
		WIN32_FIND_DATAA FindData;
		// 构造路径
		const string pathName = tempPath + "*.*";
		const HANDLE hFind = FindFirstFileA(pathName.c_str(), &FindData);
		if (hFind == INVALID_HANDLE_VALUE)
		{
			return;
		}
		while (FindNextFileA(hFind, &FindData))
		{
			// 过滤.和..
			if (strcmp(FindData.cFileName, ".") == 0 ||
				strcmp(FindData.cFileName, "..") == 0)
			{
				continue;
			}

			// 构造完整路径
			const string fullname = tempPath + string(FindData.cFileName);
			// 如果是文件夹,则递归删除文件夹
			if (FindData.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY)
			{
				deleteFolder(fullname);
			}
			// 如果是文件,则直接删除文件
			else
			{
				DeleteFileA(fullname.c_str());
			}
		}
		FindClose(hFind);
		// 删除文件夹自身
		RemoveDirectoryA(tempPath.c_str());
	}

	bool deleteEmptyFolder(const string& path)
	{
		string tempPath = path;
		validPath(tempPath);
		WIN32_FIND_DATAA findFileData;
		const HANDLE hFind = FindFirstFileA((tempPath + "*").c_str(), &findFileData);
		// 如果找不到文件夹就直接返回
		if (INVALID_HANDLE_VALUE == hFind)
		{
			return true;
		}
		bool isEmpty = true;
		do
		{
			// 过滤.和..
			if (strcmp(findFileData.cFileName, ".") == 0 ||
				strcmp(findFileData.cFileName, "..") == 0)
			{
				continue;
			}
			// 构造完整路径
			// 如果是文件夹,则递归删除空文件夹
			if (findFileData.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY)
			{
				// 如果文件夹未删除成功,则表示不是空文件夹
				if (!deleteEmptyFolder(tempPath + string(findFileData.cFileName)))
				{
					isEmpty = false;
				}
			}
			// 是文件,则标记为有文件,然后退出循环
			else
			{
				isEmpty = false;
			}
		} while (FindNextFileA(hFind, &findFileData));
		FindClose(hFind);
		if (isEmpty)
		{
			RemoveDirectoryA(path.c_str());
		}
		return isEmpty;
	}

	bool isFileExist(const string& fullPath)
	{
#ifdef WIN32
		const int ret = _access(fullPath.c_str(), 0);
#elif defined LINUX
		const int ret = access(fullPath.c_str(), 0);
#endif
		return ret == 0;
	}

	bool isDirExist(const string& fullPath)
	{
#ifdef WIN32
		const int ret = _access(fullPath.c_str(), 0);
#elif defined LINUX
		const int ret = access(fullPath.c_str(), 0);
#endif
		return ret == 0;
	}

	bool copyFile(const string& sourceFile, const string& destFile, const bool overWrite)
	{
		// 不允许覆盖则不拷贝
		if (isFileExist(destFile) && !overWrite)
		{
			return true;
		}
		FileContent file;
		if (!openBinaryFile(sourceFile, &file))
		{
			return false;
		}
		createFolder(getFilePath(destFile, false));
		return writeFileSimple(destFile, file.mBuffer, file.mFileSize);
	}

	bool copyFolder(const string& sourcePath, const string& destPath, const bool overWrite)
	{
		// 打开目录下所有的文件,再写入到目标目录一份
		bool result = true;
		Vector<string> sourceFileList;
		findFiles(sourcePath, sourceFileList);
		for (const string& item : sourceFileList)
		{
			result &= copyFile(item, destPath + removeStartString(item, sourcePath));
		}
		return result;
	}

	bool createFolder(const string& path)
	{
		// 如果目录已经存在,则返回true
		if (isFileExist(path))
		{
			return true;
		}
		// 如果有上一级目录,并且上一级目录不存在,则先创建上一级目录
		const string parentDir = getFilePath(path, false);
		if (parentDir != path)
		{
			createFolder(parentDir);
		}
#ifdef WIN32
		if (0 != _mkdir(path.c_str()))
#elif defined LINUX
		if (0 != mkdir(path.c_str(), S_IRUSR | S_IWUSR | S_IXUSR))
#endif
		{
			return false;
		}
		return true;
	}

	bool setFileAccess(const string& fileName, int mode)
	{
		return _chmod(fileName.c_str(), mode) == 0;
	}

	bool setDirAccess(const string& path, const int mode)
	{
		bool success = _chmod(path.c_str(), mode) == 0;
		// 先设置所有目录的权限
		Vector<string> folders;
		findFolders(path, folders, true);
		for (const string& item : folders)
		{
			if (_chmod(item.c_str(), mode) != 0)
			{
				success = false;
				break;
			}
		}

		// 再设置所有文件的权限
		Vector<string> files;
		findFiles(path, files);
		for (const string& item : files)
		{
			if (_chmod(item.c_str(), mode) != 0)
			{
				success = false;
				break;
			}
		}
		return success;
	}

	bool writeFile(const string& filePath, const char* buffer, const int length, const bool append)
	{
		// 检查参数
		if (length > 0 && buffer == nullptr)
		{
			ERROR("buffer is nullptr! can not write file! file path : " + filePath);
			return false;
		}
		// 检查路径
		string newPath = filePath;
		rightToLeft(newPath);
		const string path = getFilePath(newPath, false);
		if (!createFolder(path))
		{
			ERROR("can not create folder, name : " + path);
			return false;
		}
		return writeFileSimple(newPath, buffer, length, append);
	}

	bool writeFileSimple(const string& fileName, const char* buffer, const int writeCount, const bool append)
	{
		const char* accessMode = append ? "ab+" : "wb+";
		FILE* pFile = nullptr;
#ifdef WIN32
		fopen_s(&pFile, fileName.c_str(), accessMode);
#elif defined LINUX
		// linux下为了能正常打开中文名的文件,需要设置一下编码
		{
			LocaleUTF8Scope scope(mSetLocaleLock);
			pFile = fopen(fileName.c_str(), accessMode);
		}
#endif
		if (pFile == nullptr)
		{
			ERROR("can not write file, name : " + fileName);
			return false;
		}
		fwrite(buffer, sizeof(char), writeCount, pFile);
		fclose(pFile);
		return true;
	}

	bool openFile(const string& filePath, FileContent* fileContent, bool addZero, const bool showError)
	{
		FILE* pFile = nullptr;
#ifdef WIN32
		fopen_s(&pFile, filePath.c_str(), "rb");
#elif defined LINUX
		// linux下为了能正常打开中文名的文件,需要设置一下编码
		{
			LocaleUTF8Scope scope(mSetLocaleLock);
			pFile = fopen(filePath.c_str(), "rb");
		}
#endif
		if (pFile == nullptr)
		{
			if (showError)
			{
#ifdef LINUX
				//转换错误码为对应的错误信息
				ERROR(string("strerror: ") + strerror(errno) + ",filename:" + filePath);
#endif
			}
			return false;
		}
		fseek(pFile, 0, SEEK_END);
		const int fileSize = ftell(pFile);
		rewind(pFile);
		const int bufferLen = addZero ? fileSize + 1 : fileSize;
		// 因为文件长度大多都不一样,所以不使用内存池中的数组
		fileContent->createBuffer(bufferLen);
		if ((int)fread(fileContent->mBuffer, sizeof(char), fileSize, pFile) != fileSize)
		{
			ERROR("read count error!");
		}
		fclose(pFile);
		if (addZero)
		{
			fileContent->mBuffer[bufferLen - 1] = 0;
		}
		return true;
	}

	string openTxtFile(const string& filePath, const bool utf8ToANSI, const bool showError)
	{
		string content;
		openTxtFile(filePath, content, utf8ToANSI, showError);
		return content;
	}

	bool openTxtFile(const string& filePath, string& fileContent, const bool utf8ToANSI, const bool showError)
	{
		FileContent file;
		openFile(filePath, &file, true, showError);
		if (file.mBuffer == nullptr)
		{
			return false;
		}
		fileContent.clear();
		fileContent.append(file.mBuffer);
		if (utf8ToANSI)
		{
			fileContent = UTF8ToANSI(fileContent, true);
		}
		return true;
	}

	bool openTxtFileLines(const string& filePath, Vector<string>& fileLines, const bool utf8ToANSI, const bool showError)
	{
		string fileContent;
		if (!openTxtFile(filePath, fileContent, utf8ToANSI, showError))
		{
			return false;
		}
		split(fileContent, "\n", fileLines);
		for (string& item : fileLines)
		{
			removeAll(item, '\r');
		}
		return true;
	}

	bool moveFile(const string& fileName, const string& newName, const bool forceCover)
	{
		if (fileName == newName)
		{
			return true;
		}
		createFolder(getFilePath(newName, false));
		return renameFile(fileName, newName, forceCover);
	}

	bool renameFile(const string& fileName, const string& newName, const bool forceCover)
	{
		if (!isFileExist(fileName))
		{
			return false;
		}
		if (isFileExist(newName))
		{
			if (!forceCover)
			{
				return false;
			}
			deleteFile(newName);
		}
		return rename(fileName.c_str(), newName.c_str()) == 0;
	}

	bool renameFolder(const string& folderName, const string& newName, const bool forceCover)
	{
		if (!isDirExist(folderName))
		{
			return false;
		}
		if (isDirExist(newName))
		{
			if (!forceCover)
			{
				return false;
			}
			deleteFile(newName);
		}
		return rename(folderName.c_str(), newName.c_str()) == 0;
	}

	string generateFileMD5(const string& fileName, const bool isUpper)
	{
		constexpr int bufferSize = 1024 * 8;
		CharArrayScope buffer(bufferSize);
		return generateFileMD5(fileName, buffer.mArray, bufferSize, isUpper);
	}

	string generateFileMD5(const string& fileName, char* buffer, const int bufferSize, const bool isUpper)
	{
		FILE* pFile = nullptr;
#ifdef WIN32
		fopen_s(&pFile, fileName.c_str(), "rb");
#elif defined LINUX
		// linux下为了能正常打开中文名的文件,需要设置一下编码
		{
			LocaleUTF8Scope scope(mSetLocaleLock);
			pFile = fopen(fileName.c_str(), "rb");
		}
#endif
		if (pFile == nullptr)
		{
			return "";
		}
		fseek(pFile, 0, SEEK_END);
		const int fileSize = (int)ftell(pFile);
		rewind(pFile);
		MD5 md5;
		const int times = divideInt(fileSize, bufferSize);
		FOR(times)
		{
			// 读取文件一段内存
			const int readCount = (int)fread(buffer, sizeof(char), bufferSize, pFile);
			if (readCount != bufferSize)
			{
				ERROR("read error");
			}
			md5.update(buffer, bufferSize);
		}
		const int remainLength = fileSize - bufferSize * times;
		if (remainLength > 0)
		{
			const int readCount = (int)fread(buffer, sizeof(char), remainLength, pFile);
			if (readCount != remainLength)
			{
				ERROR("read error");
			}
			md5.update(buffer, remainLength);
		}
		fclose(pFile);
		string str = md5.finalize().hexdigest();
		return isUpper ? toUpper(str) : str;
	}

	string generateMD5(const char* buffer, const int bufferSize, const bool isUpper)
	{
		MD5 md5;
		md5.update(buffer, bufferSize);
		string str = md5.finalize().hexdigest();
		return isUpper ? toUpper(str) : str;
	}

	string generateMD5(const string& data, const bool isUpper)
	{
		MD5 md5;
		md5.update(data.c_str(), (int)data.length());
		string str = md5.finalize().hexdigest();
		return isUpper ? toUpper(str) : str;
	}

	int getFileSize(const string& filePath)
	{
		struct stat info;
		stat(filePath.c_str(), &info);
		return (int)info.st_size;
	}
}