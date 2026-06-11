#ifndef _FILE_UTILITY_H_
#define _FILE_UTILITY_H_

#include "MathUtility.h"

class FileContent;
class FileUtility : public MathUtility
{
public:
	static void validPath(string& path);
	static void findFiles(const string& path, Vector<string>& files);
	static void findFiles(const string& path, Vector<string>& files, const string& patterns, bool recursive = true);
#if RUN_PLATFORM == PLATFORM_LINUX
	static bool isDirectory(const string& path);
#endif
	static void findFiles(const string& path, Vector<string>& files, const string* patterns, uint patternCount, bool recursive = true);
	static void findFolders(const string& path, Vector<string>& folders, bool recursive = false);
	static void deleteFolder(const string& path);
	static bool deleteEmptyFolder(const string& path);
	static bool deleteFile(const string& path);
	static bool isFileExist(const string& fullPath);
	static bool isDirExist(const string& fullPath);
	// 将sourceFile拷贝到destFile,sourceFile和destFile都是带可直接访问的路径的文件名,overWrite指定当目标文件已经存在时是否要覆盖文件
	static bool copyFile(const string& sourceFile, const string& destFile, bool overWrite = true);
	// 将文件夹sourcePath拷贝到destPath,sourcePath和destPath都是带以/结尾,overWrite指定当目标文件已经存在时是否要覆盖文件
	static bool copyFolder(const string& sourcePath, const string& destPath, bool overWrite = true);
	// 创建一个文件夹,path是一个不以/结尾的可直接访问的相对或者绝对的文件夹名
	static bool createFolder(const string& path);
	static bool writeEmptyFile(const string& filePath);
	static bool writeFile(const string& filePath, const char* buffer, uint length, bool append = false);
	static bool writeFile(const string& filePath, const string& text, bool append = false);
	static bool writeFileSimple(const string& fileName, const char* buffer, uint writeCount, bool append = false);
	static bool writeFileSimple(const string& fileName, const string& text, bool append = false);
	static bool openFile(const string& filePath, FileContent* fileContent, bool addZero, bool showError = true);
	static string openTxtFile(const string& filePath, bool utf8ToANSI, bool showError = true);
	static bool openTxtFile(const string& filePath, string& fileContent, bool utf8ToANSI, bool showError = true);
	static bool openTxtFileLines(const string& filePath, Vector<string>& fileLines, bool utf8ToANSI, bool showError = true);
	static bool openBinaryFile(const string& filePath, FileContent* fileContent, bool showError = true);
	static bool moveFile(const string& fileName, const string& newName, bool forceCover = false);
	static bool renameFile(const string& fileName, const string& newName, bool forceCover = false);
	static bool renameFolder(const string& folderName, const string& newName, bool forceCover = false);
	static string generateFileMD5(const string& fileName);
	static string generateFileMD5(const string& fileName, char* buffer, uint bufferSize);
	static string generateFileMD5(const char* buffer, uint bufferSize);
	static uint getFileSize(const string& filePath);
};

#endif