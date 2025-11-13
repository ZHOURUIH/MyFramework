#include "Utility.h"

void FileUtility::validPath(string& path)
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

void FileUtility::findFiles(const string& pathName, myVector<string>& files, const string& patterns, bool recursive)
{
	findFiles(pathName, files, &patterns, 1, recursive);
}
#if RUN_PLATFORM == PLATFORM_LINUX
// 判断是否为目录
bool FileUtility::isDirectory(const string& pszName)
{
	struct stat S_stat;
	// 取得文件状态
	if (lstat(pszName.c_str(), &S_stat) < 0)
	{
		return false;
	}
	// 判断文件是否为文件夹
	return S_ISDIR(S_stat.st_mode);
}

void FileUtility::findFiles(const string& path, myVector<string>& files, const string* patterns, uint patternCount, bool recursive)
{
	string tempPath = validPath(path);
	DIR* pDir = opendir(tempPath.c_str());
	if (pDir == NULL)
	{
		return;
	}
	array<char, 1024> szTmpPath;
	dirent* pDirent = NULL;
	while ((pDirent = readdir(pDir)) != NULL)
	{
		//如果是.或者..跳过
		if (string(pDirent->d_name) == "." || string(pDirent->d_name) == "..")
		{
			continue;
		}
		//判断是否为文件夹
		SPRINTF(szTmpPath._Elems, szTmpPath.size(), "%s%s", tempPath.c_str(), pDirent->d_name);
		if (isDirectory(szTmpPath._Elems))
		{
			if (recursive)
			{
				//如果是文件夹则进行递归
				findFiles(szTmpPath._Elems, files, patterns, recursive);
			}
		}
		else
		{
			uint patternCount = patterns.size();
			if(patternCount > 0)
			{
				FOR_I(patternCount)
				{
					if (StringUtility::endWith(szTmpPath._Elems, patterns[i], false))
					{
						files.push_back(szTmpPath._Elems);
						break;
					}
				}
			}
			else
			{
				files.push_back(szTmpPath._Elems);
			}
		}
	}
	closedir(pDir);
}

void FileUtility::findFolders(const string& path, myVector<string>& folders, bool recursive)
{
	string tempPath = validPath(path);
	struct dirent* pDirent;
	DIR* pDir = opendir(tempPath.c_str());
	if (pDir == NULL)
	{
		return;
	}
	array<char, 1024> szTmpPath;
	while ((pDirent = readdir(pDir)) != NULL)
	{
		//如果是.或者..跳过
		if (string(pDirent->d_name) == "." || string(pDirent->d_name) == "..")
		{
			continue;
		}
		//判断是否为文件夹
		SPRINTF(szTmpPath._Elems, szTmpPath.size(), "%s%s", tempPath.c_str(), pDirent->d_name);
		// 如果是文件夹,则先将文件夹放入列表,然后判断是否递归查找
		if (isDirectory(szTmpPath._Elems))
		{
			folders.push_back(szTmpPath._Elems);
			if (recursive)
			{
				findFolders(szTmpPath._Elems, folders, recursive);
			}
		}
	}
	closedir(pDir);
}

bool FileUtility::deleteEmptyFolder(const string& path)
{
	string tempPath = path;
	validPath(tempPath);
	struct dirent* pDirent;
	DIR* pDir = opendir(tempPath.c_str());
	if (pDir == NULL)
	{
		return true;
	}
	bool isEmpty = true;
	array<char, 1024> szTmpPath;
	while ((pDirent = readdir(pDir)) != NULL)
	{
		//如果是.或者..跳过
		if (string(pDirent->d_name) == "." || string(pDirent->d_name) == "..")
		{
			continue;
		}
		//判断是否为文件夹
		SPRINTF(szTmpPath._Elmes, szTmpPath.size(), "%s%s", tempPath.c_str(), pDirent->d_name);
		// 如果是文件夹,则递归删除空文件夹
		if (isDirectory(szTmpPath._Elmes))
		{
			// 如果文件夹未删除成功,则表示不是空文件夹
			if (!deleteEmptyFolder(szTmpPath._Elmes))
			{
				isEmpty = false;
			}
		}
		// 是文件,则标记为有文件,然后退出循环
		else
		{
			isEmpty = false;
		}
	}
	closedir(pDir);
	if (isEmpty)
	{
		rmdir(tempPath.c_str());
	}
	return isEmpty;
}

void FileUtility::deleteFolder(const string& path)
{
	string tempPath = validPath(path);
	struct dirent* pDirent;
	DIR* pDir = opendir(tempPath.c_str());
	if (pDir == NULL)
	{
		return;
	}
	array<char, 1024> szTmpPath;
	while ((pDirent = readdir(pDir)) != NULL)
	{
		//如果是.或者..跳过
		if (string(pDirent->d_name) == "." || string(pDirent->d_name) == "..")
		{
			continue;
		}
		//判断是否为文件夹
		SPRINTF(szTmpPath._Elems, szTmpPath.size(), "%s%s", tempPath.c_str(), pDirent->d_name);
		// 如果是文件夹,则递归删除文件夹
		if (isDirectory(szTmpPath._Elems))
		{
			deleteFolder(szTmpPath._Elems);
		}
		// 是文件,则删除文件
		else
		{
			remove(szTmpPath._Elems);
		}
	}
	closedir(pDir);
	rmdir(tempPath.c_str());
}

#elif RUN_PLATFORM == PLATFORM_WINDOWS
void FileUtility::findFiles(const string& path, myVector<string>& files, const string* patterns, uint patternCount, bool recursive)
{
	string tempPath = path;
	validPath(tempPath);
	WIN32_FIND_DATAA FindFileData;
	HANDLE hFind = FindFirstFileA((tempPath + "*").c_str(), &FindFileData);
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
		string fullname = tempPath + string(FindFileData.cFileName);
		if (FindFileData.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY)
		{
			if (recursive)
			{
				findFiles(fullname.c_str(), files, patterns, patternCount, recursive);
			}
		}
		else
		{
			if (patternCount > 0)
			{
				FOR_I(patternCount)
				{
					if (endWith(fullname, patterns[i], false))
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
	}while (FindNextFileA(hFind, &FindFileData));
	FindClose(hFind);
}

void FileUtility::findFolders(const string& path, myVector<string>& folders, bool recursive)
{
	string tempPath = path;
	validPath(tempPath);
	WIN32_FIND_DATAA FindFileData;
	HANDLE hFind = FindFirstFileA((tempPath + "*").c_str(), &FindFileData);
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
		string fullname = tempPath + string(FindFileData.cFileName);
		// 是文件夹则先放入列表,然后判断是否递归查找
		if (FindFileData.dwFileAttributes & FILE_ATTRIBUTE_DIRECTORY)
		{
			folders.push_back(fullname);
			if (recursive)
			{
				findFolders(fullname.c_str(), folders, recursive);
			}
		}
	} while (FindNextFileA(hFind, &FindFileData));
	FindClose(hFind);
}

void FileUtility::deleteFolder(const string& path)
{
	string tempPath = path;
	validPath(tempPath);
	WIN32_FIND_DATAA FindData;
	// 构造路径
	string pathName = tempPath + "*.*";
	HANDLE hFind = FindFirstFileA(pathName.c_str(), &FindData);
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
		string fullname = tempPath + string(FindData.cFileName);
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

bool FileUtility::deleteEmptyFolder(const string& path)
{
	string tempPath = path;
	validPath(tempPath);
	WIN32_FIND_DATAA findFileData;
	HANDLE hFind = FindFirstFileA((tempPath + "*").c_str(), &findFileData);
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
#endif

void FileUtility::deleteFile(const string& fileName)
{
	if (!isFileExist(fileName))
	{
		return;
	}
	remove(fileName.c_str());
}

bool FileUtility::isFileExist(const string& fullPath)
{
#if RUN_PLATFORM == PLATFORM_WINDOWS
	int ret = _access(fullPath.c_str(), 0);
#elif RUN_PLATFORM == PLATFORM_LINUX
	int ret = access(fullPath.c_str(), 0);
#endif
	return ret == 0;
}

bool FileUtility::copyFile(const string& sourceFile, const string& destFile, bool overWrite)
{
	// 如果目标文件所在的目录不存在,则先创建目录
	string parentDir = getFilePath(destFile);
	createFolder(parentDir);
#if RUN_PLATFORM == PLATFORM_WINDOWS
	return CopyFileA(sourceFile.c_str(), destFile.c_str(), !overWrite) == TRUE;
#elif RUN_PLATFORM == PLATFORM_LINUX
	ERROR("linux暂时不支持拷贝文件");
	return false;
#endif
}

bool FileUtility::createFolder(const string& path)
{
	// 如果目录已经存在,则返回true
	if (isFileExist(path))
	{
		return true;
	}
	// 如果文件不存在则检查父目录是否存在
	if (!isFileExist(path))
	{
		// 如果有上一级目录,并且上一级目录不存在,则先创建上一级目录
		string parentDir = getFilePath(path);
		if (parentDir != path)
		{
			createFolder(parentDir);
		}
#if RUN_PLATFORM == PLATFORM_WINDOWS
		if (0 != _mkdir(path.c_str()))
#elif RUN_PLATFORM == PLATFORM_LINUX
		if (0 != mkdir(path.c_str(), S_IRUSR | S_IWUSR | S_IXUSR))
#endif
		{
			return false;
		}
	}
	return true;
}

bool FileUtility::writeFileIfChanged(const string& filePath, const string& text)
{
	if (openTxtFile(filePath, false) != text)
	{
		return writeFile(filePath, text);
	}
	return true;
}

bool FileUtility::writeFile(string filePath, const string& text, bool append)
{
	return writeFile(filePath, text.c_str(), (uint)text.length(), append);
}

bool FileUtility::writeFile(string filePath, const char* buffer, uint length, bool append)
{
	// 检查参数
	if (buffer == NULL)
	{
		ERROR("file length error! can not write file! file path : " + filePath);
		return false;
	}
	if (length > 0 && buffer == NULL)
	{
		ERROR("buffer is NULL! can not write file! file path : " + filePath);
		return false;
	}
	// 检查路径
	rightToLeft(filePath);
	string path = getFilePath(filePath);
	if (!createFolder(path))
	{
		ERROR("can not create folder, name : " + path);
		return false;
	}
	return writeFileSimple(filePath, buffer, length, append);
}

bool FileUtility::writeFileSimple(const string& fileName, const char* buffer, uint writeCount, bool append)
{
	const char* accesMode = append ? "ab+" : "wb+";
#if RUN_PLATFORM == PLATFORM_WINDOWS
	FILE* pFile = NULL;
	fopen_s(&pFile, fileName.c_str(), accesMode);
#elif RUN_PLATFORM == PLATFORM_LINUX
	FILE* pFile = fopen(fileName.c_str(), accesMode);
#endif
	if (pFile == NULL)
	{
		ERROR("can not write file, name : " + fileName);
		return false;
	}
	fwrite(buffer, sizeof(char), writeCount, pFile);
	fclose(pFile);
	return true;
}

bool FileUtility::writeFileSimple(const string& fileName, const string& text, bool append)
{
	return writeFileSimple(fileName, text.c_str(), (uint)text.length(), append);
}

void FileUtility::openFile(const string& filePath, FileContent& fileContent, bool addZero, bool* hasBOM)
{
	FILE* pFile = NULL;
#if RUN_PLATFORM == PLATFORM_WINDOWS
	fopen_s(&pFile, filePath.c_str(), "rb");
#elif RUN_PLATFORM == PLATFORM_LINUX
	pFile = fopen(filePath.c_str(), "rb");
#endif
	if (pFile == NULL)
	{
#if RUN_PLATFORM == PLATFORM_LINUX
		//转换错误码为对应的错误信息
		ERROR(string("strerror: ") + strerror(errno) + ",filename:" + filePath);
#endif
		return;
	}
	fseek(pFile, 0, SEEK_END);
	uint fileSize = ftell(pFile);
	rewind(pFile);
	uint bufferLen = addZero ? fileSize + 1 : fileSize;
	// 因为文件长度大多都不一样,所以不使用内存池中的数组
	fileContent.createBuffer(bufferLen);
	if (fread(fileContent.mBuffer, sizeof(char), fileSize, pFile) != fileSize)
	{
		LOG("read count error!");
	}
	fclose(pFile);
	if (hasBOM != nullptr)
	{
		*hasBOM = fileContent.mFileSize >= 3 && 
				  fileContent.mBuffer[0] == BOM[0] &&
				  fileContent.mBuffer[1] == BOM[1] &&
				  fileContent.mBuffer[2] == BOM[2];
	}
	if (addZero)
	{
		fileContent.mBuffer[bufferLen - 1] = 0;
	}
}

string FileUtility::openTxtFile(const string& filePath, bool utf8ToANSI, bool* hasBOM)
{
	FileContent file;
	openFile(filePath, file, true, hasBOM);
	if (file.mBuffer == nullptr)
	{
		return "";
	}
	if (utf8ToANSI)
	{
		return UTF8ToANSI(file.mBuffer, true);
	}
	else
	{
		return file.mBuffer;
	}
}

myVector<string> FileUtility::openTxtFileLines(const string& filePath, bool utf8ToANSI, bool* hasBOM)
{
	myVector<string> fileLines;
	split(openTxtFile(filePath, utf8ToANSI, hasBOM).c_str(), "\n", fileLines, false);
	FOR_VECTOR(fileLines)
	{
		removeAll(fileLines[i], '\r');
	}
	return fileLines;
}

void FileUtility::openBinaryFile(const string& filePath, FileContent& fileContent, bool* hasBOM)
{
	return openFile(filePath, fileContent, false, hasBOM);
}

bool FileUtility::moveFile(const string& fileName, const string& newName, bool forceCover)
{
	string newPath = getFilePath(newName);
	createFolder(newPath);
	return renameFile(fileName, newName, forceCover);
}

bool FileUtility::renameFile(const string& fileName, const string& newName, bool forceCover)
{
	if (!isFileExist(fileName))
	{
		return false;
	}
	if (isFileExist(newName))
	{
		if (forceCover)
		{
			deleteFile(newName);
		}
		else
		{
			return false;
		}
	}
	return rename(fileName.c_str(), newName.c_str()) == 0;
}

uint FileUtility::getFileSize(const string& filePath)
{
	struct stat info;
	stat(filePath.c_str(), &info);
	return (uint)info.st_size;
}

string FileUtility::generateFileMD5(const string& fileName)
{
	constexpr int bufferSize = 1024 * 8;
	char buffer[bufferSize]{};
	return generateFileMD5(fileName, buffer, bufferSize);
}

string FileUtility::generateFileMD5(const string& fileName, char* buffer, const int bufferSize)
{
	FILE* pFile = nullptr;
#ifdef WINDOWS
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
	const int times = fileSize / bufferSize;
	FOR_I(times)
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
	const string md5Str = md5.finalize().hexdigest();
	return toUpper(md5Str);
}

string FileUtility::generateFileMD5(const char* buffer, const int bufferSize)
{
	MD5 md5;
	md5.update(buffer, bufferSize);
	return toUpper(md5.finalize().hexdigest());
}

string FileUtility::generateStringMD5(const string& str)
{
	return generateFileMD5(str.c_str(), (int)str.length());
}