#ifndef _FILE_UTILITY_H_
#define _FILE_UTILITY_H_

#include "MathUtility.h"

class FileContent
{
public:
	char* mBuffer;
	uint mFileSize;
	FileContent()
	{
		mBuffer = NULL;
		mFileSize = 0;
	}
	virtual ~FileContent()
	{
		DELETE_ARRAY(mBuffer);
	}
	void createBuffer(int bufferSize)
	{
		mFileSize = bufferSize;
		mBuffer = NEW_ARRAY(char, mFileSize, mBuffer);
		if (mBuffer == NULL)
		{
			//ERROR("create file buffer failed!");
		}
	}
};

class FileUtility : public MathUtility
{
public:
	static void validPath(string& path);
	static void findFiles(const string& path, myVector<string>& files, const string& patterns, bool recursive = true);
#if RUN_PLATFORM == PLATFORM_LINUX
	static bool isDirectory(const string& path);
#endif
	static void findFiles(const string& path, myVector<string>& files, const string* patterns = NULL, uint patternCount = 0, bool recursive = true);
	static void findFolders(const string& path, myVector<string>& folders, bool recursive = false);
	static bool isEmptyFolder(const string& path);
	static void deleteFolder(const string& path);
	static bool deleteEmptyFolder(const string& path);
	static void deleteFile(const string& path);
	static bool isFileExist(const string& fullPath);
	// 将sourceFile拷贝到destFile,sourceFile和destFile都是带可直接访问的路径的文件名,overWrite指定当目标文件已经存在时是否要覆盖文件
	static bool copyFile(const string& sourceFile, const string& destFile, bool overWrite = true);
	// 创建一个文件夹,path是一个不以/结尾的可直接访问的相对或者绝对的文件夹名
	static bool createFolder(const string& path);
	static bool writeFile(string filePath, const char* buffer, uint length, bool append = false);
	static bool writeFile(string filePath, const string& text, bool append = false);
	static bool writeFileSimple(const string& fileName, const char* buffer, uint writeCount, bool append = false);
	static bool writeFileSimple(const string& fileName, const string& text, bool append = false);
	static void openFile(const string& filePath, FileContent& fileContent, bool addZero);
	static string openTxtFile(const string& filePath, bool utf8ToANSI);
	static void openTxtFile(const string& filePath, string& fileContent, bool utf8ToANSI);
	static void openTxtFileLines(const string& filePath, myVector<string>& fileLines, bool utf8ToANSI);
	static void openBinaryFile(const string& filePath, FileContent& fileContent);
	static bool moveFile(const string& fileName, const string& newName, bool forceCover = false);
	static bool renameFile(const string& fileName, const string& newName, bool forceCover = false);
	static bool renameFolder(const string& fileName, const string& newName);
	static uint getFileSize(const string& filePath);
};

#endif