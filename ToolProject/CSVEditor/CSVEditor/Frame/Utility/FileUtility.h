#pragma once

#include "MathUtility.h"

namespace FileUtility
{
	void validPath(string& path);
	void findFiles(const string& path, Vector<string>& files, const string* patterns, int patternCount, bool recursive = true);
	inline void findFiles(const string& path, Vector<string>& files) { findFiles(path, files, nullptr, 0, true); }
	inline void findFiles(const string& path, Vector<string>& files, const string& patterns, bool recursive = true)
	{
		findFiles(path, files, &patterns, 1, recursive);
	}
	void findFolders(const string& path, Vector<string>& folders, bool recursive = false);
	void deleteFolder(const string& path);
	bool deleteEmptyFolder(const string& path);
	inline bool deleteFile(const string& path) { return remove(path.c_str()) == 0; }
	bool isFileExist(const string& fullPath);
	bool isDirExist(const string& fullPath);
	// 将sourceFile拷贝到destFile,sourceFile和destFile都是带可直接访问的路径的文件名,overWrite指定当目标文件已经存在时是否要覆盖文件
	bool copyFile(const string& sourceFile, const string& destFile, bool overWrite = true);
	// 将文件夹sourcePath拷贝到destPath,sourcePath和destPath都是带以/结尾,overWrite指定当目标文件已经存在时是否要覆盖文件
	bool copyFolder(const string& sourcePath, const string& destPath, bool overWrite = true);
	// 创建一个文件夹,path是一个不以/结尾的可直接访问的相对或者绝对的文件夹名
	bool createFolder(const string& path);
	// 设置文件的访问权限
	bool setFileAccess(const string& fileName, int mode);
	// 设置文件夹中所有文件的访问权限
	bool setDirAccess(const string& path, int mode);
	bool writeFile(const string& filePath, const char* buffer, int length, bool append = false);
	inline bool writeFile(const string& filePath, const string& text, bool append = false) { return writeFile(filePath, text.c_str(), (int)text.length(), append); }
	bool writeFileSimple(const string& fileName, const char* buffer, int writeCount, bool append = false);
	inline bool writeFileSimple(const string& fileName, const string& text, bool append = false) { return writeFileSimple(fileName, text.c_str(), (int)text.length(), append); }
	bool openFile(const string& filePath, FileContent* fileContent, bool addZero, bool showError = true);
	string openTxtFile(const string& filePath, bool utf8ToANSI, bool showError = true);
	bool openTxtFile(const string& filePath, string& fileContent, bool utf8ToANSI, bool showError = true);
	bool openTxtFileLines(const string& filePath, Vector<string>& fileLines, bool utf8ToANSI, bool showError = true);
	inline bool openBinaryFile(const string& filePath, FileContent* fileContent, bool showError = true) { return openFile(filePath, fileContent, false, showError); }
	bool moveFile(const string& fileName, const string& newName, bool forceCover = false);
	bool renameFile(const string& fileName, const string& newName, bool forceCover = false);
	bool renameFolder(const string& folderName, const string& newName, bool forceCover = false);
	string generateFileMD5(const string& fileName, bool isUpper);
	string generateFileMD5(const string& fileName, char* buffer, int bufferSize, bool isUpper);
	string generateMD5(const char* buffer, int bufferSize, bool isUpper);
	string generateMD5(const string& data, bool isUpper);
	int getFileSize(const string& filePath);
}

using FileUtility::writeFile;
using FileUtility::openTxtFile;
using FileUtility::findFiles;
using FileUtility::deleteFile;
using FileUtility::openBinaryFile;
using FileUtility::openTxtFileLines;
using FileUtility::generateFileMD5;
using FileUtility::generateMD5;
using FileUtility::isFileExist;