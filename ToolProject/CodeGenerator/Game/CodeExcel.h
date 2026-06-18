#pragma once

#include "CodeUtility.h"

class CodeExcel : public CodeUtility
{
public:
	static void generate();
protected:
	//c++
	static void generateCppExcelDataFile(const CSVInfo& sqliteInfo, const string& dataFilePath);
	static void generateCppExcelTableFile(const CSVInfo& sqliteInfo, const string& tableFilePath);
	static void generateCppExcelRegisteFile(const myVector<string>& tableFileList, const string& filePath);
	static void generateCppExcelInstanceDeclare(const myVector<string>& tableFileList, const string& gameBaseHeaderFileName, const string& exprtMacro);
	static void generateCppExcelInstanceDefine(const myVector<string>& tableFileList, const string& gameBaseCppFileName);
	static void generateCppExcelSTLPoolRegister(const myVector<string>& tableFileList, const string& gameSTLPoolFile);
	static void generateCppExcelInstanceClear(const myVector<string>& tableFileList, const string& gameBaseCppFileName);
	static void generateCppGlobalConfig(const CSVInfo& globalConfig, const string& tableFilePath);
	static void generateCppBuff(const CSVInfo& config);
	//c#,C#这里不再使用SQLite,而是将SQLite转换为自定义的数据来读取,也跟Excel转换以后的数据一样
	static void generateCSharpExcelDataFile(const CSVInfo& sqliteInfo, const string& dataFileHotFixPath);
	static void generateCSharpExcelTableFile(const CSVInfo& sqliteInfo, const string& tableFileHotFixPath);
	static void generateCSharpExcelRegisteFileFile(const myVector<CSVInfo>& sqliteInfo, const string& fileHotFixPath);
	static void generateCSharpExcelDeclare(const myVector<CSVInfo>& sqliteInfo, const string& fileHotFixPath);
	static void generateCSharpGlobalConfig(const CSVInfo& globalConfig, const string& tableFilePath);
	static void generateCSharpBuff(const CSVInfo& config);
protected:
	static string paramNameToFunctionName(const string& paramName);
};