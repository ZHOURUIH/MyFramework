#pragma once

#include "CodeUtility.h"

// 只能在CodeExcel之前执行
class CodeSQLite : public CodeUtility
{
public:
	static myVector<string> mSQLiteForServerTableList;
public:
	static void generate();
protected:
	//c++,服务器生成的都是按照Excel的格式生成的,因为无论是Excel还是SQLite在服务器都是生成同样的二进制文件
	static void generateCppSQLiteDataFile(const SQLiteInfo& sqliteInfo, const string& dataFilePath);
	static void generateCppSQLiteTableFile(const SQLiteInfo& sqliteInfo, const string& tableFilePath);
	//c#
	static void generateCSharpSQLiteDataFile(const SQLiteInfo& sqliteInfo, const string& dataFileHotFixPath);
	static void generateCSharpSQLiteTableFile(const SQLiteInfo& sqliteInfo, const string& tableFileHotFixPath);
	static void generateCSharpSQLiteRegisteFileFile(const myVector<SQLiteInfo>& sqliteInfo, const string& fileHotFixPath);
protected:
	static string paramNameToFunctionName(const string& paramName);
};