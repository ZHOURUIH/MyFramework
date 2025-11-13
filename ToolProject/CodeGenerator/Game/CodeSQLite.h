#ifndef _CODE_SQLITE_H_
#define _CODE_SQLITE_H_

#include "CodeUtility.h"

class CodeSQLite : public CodeUtility
{
public:
	static void generate();
protected:
	//c++
	static void generateCppSQLiteDataFile(const SQLiteInfo& sqliteInfo, const string& dataFilePath);
	static void generateCppSQLiteTableFile(const SQLiteInfo& sqliteInfo, const string& tableFilePath);
	static void generateCppSQLiteRegisteFile(const myVector<SQLiteInfo>& sqliteList, const string& filePath);
	static void generateCppSQLiteInstanceDeclare(const myVector<SQLiteInfo>& sqliteList, const string& gameBaseHeaderFileName, const string& exprtMacro);
	static void generateCppSQLiteInstanceDefine(const myVector<SQLiteInfo>& sqliteList, const string& gameBaseCppFileName);
	static void generateCppSQLiteSTLPoolRegister(const myVector<SQLiteInfo>& sqliteList, const string& gameSTLPoolFile);
	static void generateCppSQLiteInstanceClear(const myVector<SQLiteInfo>& sqliteList, const string& gameBaseCppFileName);
	static void generateCppGlobalConfig(SQLiteInfo& globalConfig, const string& tableFilePath);
	//c#,C#这里不再使用SQLite,而是将SQLite转换为自定义的数据来读取,也跟Excel转换以后的数据一样
	static void generateCSharpExcelDataFile(const SQLiteInfo& sqliteInfo, const string& dataFileHotFixPath);
	static void generateCSharpExcelTableFile(const SQLiteInfo& sqliteInfo, const string& tableFileHotFixPath);
	static void generateCSharpSQLiteDataFile(const SQLiteInfo& sqliteInfo, const string& dataFileHotFixPath);
	static void generateCSharpSQLiteTableFile(const SQLiteInfo& sqliteInfo, const string& tableFileHotFixPath);
	static void generateCSharpExcelRegisteFileFile(const myVector<SQLiteInfo>& sqliteInfo, const string& fileHotFixPath);
	static void generateCSharpSQLiteRegisteFileFile(const myVector<SQLiteInfo>& sqliteInfo, const string& fileHotFixPath);
	static void generateCSharpExcelDeclare(const myVector<SQLiteInfo>& sqliteInfo, const string& fileHotFixPath);
	static void generateCSharpGlobalConfig(SQLiteInfo& globalConfig, const string& tableFilePath);
protected:
	static string paramNameToFunctionName(const string& paramName);
};

#endif