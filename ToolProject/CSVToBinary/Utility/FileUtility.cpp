#include "FrameHeader.h"

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

void FileUtility::findFiles(const string& pathName, Vector<string>& files)
{
	findFiles(pathName, files, nullptr, 0, true);
}

void FileUtility::findFiles(const string& pathName, Vector<string>& files, const string& patterns, bool recursive)
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

void FileUtility::findFiles(const string& path, Vector<string>& files, const string* patterns, uint patternCount, bool recursive)
{
	string tempPath = path;
	validPath(tempPath);
	DIR* pDir = opendir(tempPath.c_str());
	if (pDir == nullptr)
	{
		return;
	}
	Array<1024> szTmpPath{ 0 };
	dirent* pDirent = nullptr;
	while ((pDirent = readdir(pDir)) != nullptr)
	{
		//如果是.或者..跳过
		if (string(pDirent->d_name) == "." || string(pDirent->d_name) == "..")
		{
			continue;
		}
		//判断是否为文件夹
		SPRINTF(szTmpPath.toBuffer(), szTmpPath.size(), "%s%s", tempPath.c_str(), pDirent->d_name);
		if (isDirectory(szTmpPath.toString()))
		{
			if (recursive)
			{
				//如果是文件夹则进行递归
				findFiles(szTmpPath.toString(), files, patterns, patternCount, recursive);
			}
		}
		else
		{
			if (patternCount > 0)
			{
				FOR_I(patternCount)
				{
					if (endWith(szTmpPath.toString(), patterns[i].c_str(), false))
					{
						files.push_back(szTmpPath.toString());
						break;
					}
				}
			}
			else
			{
				files.push_back(szTmpPath.toString());
			}
		}
	}
	closedir(pDir);
}

void FileUtility::findFolders(const string& path, Vector<string>& folders, bool recursive)
{
	string tempPath = path;
	validPath(tempPath);
	struct dirent* pDirent;
	DIR* pDir = opendir(tempPath.c_str());
	if (pDir == nullptr)
	{
		return;
	}
	Array<1024> szTmpPath{ 0 };
	while ((pDirent = readdir(pDir)) != nullptr)
	{
		//如果是.或者..跳过
		if (string(pDirent->d_name) == "." || string(pDirent->d_name) == "..")
		{
			continue;
		}
		//判断是否为文件夹
		SPRINTF(szTmpPath.toBuffer(), szTmpPath.size(), "%s%s", tempPath.c_str(), pDirent->d_name);
		// 如果是文件夹,则先将文件夹放入列表,然后判断是否递归查找
		if (isDirectory(szTmpPath.toString()))
		{
			folders.push_back(szTmpPath.toString());
			if (recursive)
			{
				findFolders(szTmpPath.toString(), folders, recursive);
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
	if (pDir == nullptr)
	{
		return true;
	}
	bool isEmpty = true;
	Array<1024> szTmpPath{ 0 };
	while ((pDirent = readdir(pDir)) != nullptr)
	{
		//如果是.或者..跳过
		if (string(pDirent->d_name) == "." || string(pDirent->d_name) == "..")
		{
			continue;
		}
		//判断是否为文件夹
		SPRINTF(szTmpPath.toBuffer(), szTmpPath.size(), "%s%s", tempPath.c_str(), pDirent->d_name);
		// 如果是文件夹,则递归删除空文件夹
		if (isDirectory(szTmpPath.toString()))
		{
			// 如果文件夹未删除成功,则表示不是空文件夹
			if (!deleteEmptyFolder(szTmpPath.toString()))
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
	string tempPath = path;
	validPath(tempPath);
	struct dirent* pDirent;
	DIR* pDir = opendir(tempPath.c_str());
	if (pDir == nullptr)
	{
		return;
	}
	Array<1024> szTmpPath{ 0 };
	while ((pDirent = readdir(pDir)) != nullptr)
	{
		//如果是.或者..跳过
		if (string(pDirent->d_name) == "." || string(pDirent->d_name) == "..")
		{
			continue;
		}
		//判断是否为文件夹
		SPRINTF(szTmpPath.toBuffer(), szTmpPath.size(), "%s%s", tempPath.c_str(), pDirent->d_name);
		// 如果是文件夹,则递归删除文件夹
		if (isDirectory(szTmpPath.toString()))
		{
			deleteFolder(szTmpPath.toString());
		}
		// 是文件,则删除文件
		else
		{
			remove(szTmpPath.toString());
		}
	}
	closedir(pDir);
	rmdir(tempPath.c_str());
}

#elif RUN_PLATFORM == PLATFORM_WINDOWS
void FileUtility::findFiles(const string& path, Vector<string>& files, const string* patterns, uint patternCount, bool recursive)
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

void FileUtility::findFolders(const string& path, Vector<string>& folders, bool recursive)
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

bool FileUtility::deleteFile(const string& fileName)
{
	return remove(fileName.c_str()) == 0;
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

bool FileUtility::isDirExist(const string& fullPath)
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
	createFolder(getFilePath(destFile));
	return writeFileSimple(destFile, file.mBuffer, file.mFileSize);
}

bool FileUtility::copyFolder(const string& sourcePath, const string& destPath, bool overWrite)
{
	// 打开目录下所有的文件,再写入到目标目录一份
	bool result = true;
	Vector<string> sourceFileList;
	findFiles(sourcePath, sourceFileList);
	FOR_VECTOR(sourceFileList)
	{
		result &= copyFile(sourceFileList[i], destPath + removeStartString(sourceFileList[i], sourcePath));
	}
	return result;
}

bool FileUtility::createFolder(const string& path)
{
	// 如果目录已经存在,则返回true
	if (isFileExist(path))
	{
		return true;
	}
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
	return true;
}

bool FileUtility::writeEmptyFile(const string& fileName)
{
	const char* accesMode = "wb+";
#if RUN_PLATFORM == PLATFORM_WINDOWS
	FILE* pFile = nullptr;
	fopen_s(&pFile, fileName.c_str(), accesMode);
#elif RUN_PLATFORM == PLATFORM_LINUX
	FILE* pFile = fopen(fileName.c_str(), accesMode);
#endif
	if (pFile == nullptr)
	{
		ERROR("can not write file, name : " + fileName);
		return false;
	}
	fclose(pFile);
	return true;
}

bool FileUtility::writeFile(const string& filePath, const string& text, bool append)
{
	return writeFile(filePath, text.c_str(), (uint)text.length(), append);
}

bool FileUtility::writeFile(const string& filePath, const char* buffer, uint length, bool append)
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
	string path = getFilePath(newPath);
	if (!createFolder(path))
	{
		ERROR("can not create folder, name : " + path);
		return false;
	}
	return writeFileSimple(newPath, buffer, length, append);
}

bool FileUtility::writeFileSimple(const string& fileName, const char* buffer, uint writeCount, bool append)
{
	const char* accesMode = append ? "ab+" : "wb+";
#if RUN_PLATFORM == PLATFORM_WINDOWS
	FILE* pFile = nullptr;
	fopen_s(&pFile, fileName.c_str(), accesMode);
#elif RUN_PLATFORM == PLATFORM_LINUX
	FILE* pFile = fopen(fileName.c_str(), accesMode);
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

bool FileUtility::writeFileSimple(const string& fileName, const string& text, bool append)
{
	return writeFileSimple(fileName, text.c_str(), (uint)text.length(), append);
}

bool FileUtility::openFile(const string& filePath, FileContent* fileContent, bool addZero, bool showError)
{
	FILE* pFile = nullptr;
#if RUN_PLATFORM == PLATFORM_WINDOWS
	pFile = _fsopen(filePath.c_str(), "rb", _SH_DENYNO);
#elif RUN_PLATFORM == PLATFORM_LINUX
	pFile = fopen(filePath.c_str(), "rb");
#endif
	if (pFile == nullptr)
	{
		if (showError)
		{
#if RUN_PLATFORM == PLATFORM_LINUX
			//转换错误码为对应的错误信息
			ERROR(string("strerror: ") + strerror(errno) + ",filename:" + filePath);
#endif
		}
		return false;
	}
	fseek(pFile, 0, SEEK_END);
	uint fileSize = ftell(pFile);
	rewind(pFile);
	uint bufferLen = addZero ? fileSize + 1 : fileSize;
	// 因为文件长度大多都不一样,所以不使用内存池中的数组
	fileContent->createBuffer(bufferLen);
	if (fread(fileContent->mBuffer, sizeof(char), fileSize, pFile) != fileSize)
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

string FileUtility::openTxtFile(const string& filePath, bool utf8ToANSI, bool showError)
{
	string content;
	openTxtFile(filePath, content, utf8ToANSI, showError);
	return content;
}

bool FileUtility::openTxtFile(const string& filePath, string& fileContent, bool utf8ToANSI, bool showError)
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
		UTF8ToANSI(fileContent.c_str(), fileContent, true);
	}
	return true;
}

bool FileUtility::openTxtFileLines(const string& filePath, Vector<string>& fileLines, bool utf8ToANSI, bool showError)
{
	string fileContent;
	if (!openTxtFile(filePath, fileContent, utf8ToANSI, showError))
	{
		return false;
	}
	split(fileContent, "\n", fileLines);
	FOR_VECTOR(fileLines)
	{
		removeAll(fileLines[i], '\r');
	}
	return true;
}

bool FileUtility::openBinaryFile(const string& filePath, FileContent* fileContent, bool showError)
{
	return openFile(filePath, fileContent, false, showError);
}

bool FileUtility::moveFile(const string& fileName, const string& newName, bool forceCover)
{
	if (fileName == newName)
	{
		return true;
	}
	createFolder(getFilePath(newName));
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
		if (!forceCover)
		{
			return false;
		}
		deleteFile(newName);
	}
	return rename(fileName.c_str(), newName.c_str()) == 0;
}

bool FileUtility::renameFolder(const string& folderName, const string& newName, bool forceCover)
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

string FileUtility::generateFileMD5(const string& fileName)
{
	uint bufferSize = 1024 * 8;
	char* buffer = newCharArray(bufferSize);
	string md5 = generateFileMD5(fileName, buffer, bufferSize);
	deleteCharArray(buffer);
	return md5;
}

string FileUtility::generateFileMD5(const string& fileName, char* buffer, uint bufferSize)
{
	FILE* pFile = nullptr;
#if RUN_PLATFORM == PLATFORM_WINDOWS
	fopen_s(&pFile, fileName.c_str(), "rb");
#elif RUN_PLATFORM == PLATFORM_LINUX
	pFile = fopen(fileName.c_str(), "rb");
#endif
	if (pFile == nullptr)
	{
		return "";
	}
	fseek(pFile, 0, SEEK_END);
	uint fileSize = (uint)ftell(pFile);
	rewind(pFile);
	MD5 md5;
	uint times = fileSize / bufferSize;
	FOR_I(times)
	{
		// 读取文件一段内存
		uint readCount = (uint)fread(buffer, sizeof(char), bufferSize, pFile);
		if (readCount != bufferSize)
		{
			ERROR("read error");
		}
		md5.update(buffer, bufferSize);
	}
	uint remainLength = fileSize - bufferSize * times;
	if (remainLength > 0)
	{
		uint readCount = (uint)fread(buffer, sizeof(char), remainLength, pFile);
		if (readCount != remainLength)
		{
			ERROR("read error");
		}
		md5.update(buffer, remainLength);
	}
	fclose(pFile);
	string md5Str = md5.finalize().hexdigest();
	return toUpper(md5Str);
}

string FileUtility::generateFileMD5(const char* buffer, uint bufferSize)
{
	MD5 md5;
	md5.update(buffer, bufferSize);
	return toUpper(md5.finalize().hexdigest());
}

uint FileUtility::getFileSize(const string& filePath)
{
	struct stat info;
	stat(filePath.c_str(), &info);
	return (uint)info.st_size;
}