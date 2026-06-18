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
			ERROR("create file buffer failed!");
		}
	}
};

class FileUtility : public MathUtility
{
public:
	static void validPath(string& path);
	static myVector<string> findFiles(const string& path, const string& patterns, bool recursive = true);
	static myVector<string> findFiles(const string& path, const string* patterns, uint patternCount, bool recursive = true);
	static void findFiles(const string& path, myVector<string>& files, const string& patterns, bool recursive = true);
#if RUN_PLATFORM == PLATFORM_LINUX
	static bool isDirectory(const string& path);
#endif
	static void findFiles(const string& path, myVector<string>& files, const string* patterns, uint patternCount, bool recursive = true);
	static void findFolders(const string& path, myVector<string>& folders, bool recursive = false);
	static void deleteFolder(const string& path);
	static bool deleteEmptyFolder(const string& path);
	static void deleteFile1(const string& path);
	static bool isFileExist(const string& fullPath);
	// 将sourceFile拷贝到destFile,sourceFile和destFile都是带可直接访问的路径的文件名,overWrite指定当目标文件已经存在时是否要覆盖文件
	static bool copyFile(const string& sourceFile, const string& destFile, bool overWrite = true);
	// 创建一个文件夹,path是一个不以/结尾的可直接访问的相对或者绝对的文件夹名
	static bool createFolder(const string& path);
	static bool writeFileIfChanged1(const string& filePath, const string& text);
	static bool writeFile1(string filePath, const char* buffer, uint length, bool append = false);
	static bool writeFile1(string filePath, const string& text, bool append = false);
	static bool writeFileSimple1(const string& fileName, const char* buffer, uint writeCount, bool append = false);
	static bool writeFileSimple1(const string& fileName, const string& text, bool append = false);
	static void openFile1(const string& filePath, FileContent& fileContent, bool addZero, bool* hasBOM = nullptr);
	static string openTxtFile1(const string& filePath, bool utf8ToANSI = true, bool* hasBOM = nullptr);
	static myVector<string> openTxtFileLines1(const string& filePath, bool utf8ToANSI = true, bool* hasBOM = nullptr);
	static void openBinaryFile1(const string& filePath, FileContent& fileContent, bool* hasBOM = nullptr);
	static bool moveFile(const string& fileName, const string& newName, bool forceCover = false);
	static bool renameFile(const string& fileName, const string& newName, bool forceCover = false);
	static uint getFileSize(const string& filePath);
	static string generateFileMD5(const string& fileName);
	static string generateFileMD5(const string& fileName, char* buffer, int bufferSize);
	static string generateFileMD5(const char* buffer, int bufferSize);
	static string generateStringMD5(const string& str);
};

#endif