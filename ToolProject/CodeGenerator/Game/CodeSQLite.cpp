#include "CodeSQLite.h"
#include "SQLiteDescription.h"
#include "SQLiteGlobal.h"

void CodeSQLite::generate()
{
	print("正在生成SQLite");

	// 先读取表格描述
	myVector<SQLiteInfo> sqliteInfoList;
	myVector<string> dbFiles;
	findFiles(SQLitePath, dbFiles, ".db");
	FOR_VECTOR(dbFiles)
	{
		SQLiteDescription table;
		table.setTableName("Z_Description");
		table.init(dbFiles[i]);
		const myMap<int, TDDescription*>& list = table.queryAll();
		if (list.size() == 0)
		{
			continue;
		}
		SQLiteInfo info;
		info.mMemberList.clear();
		info.mSQLiteName = getFileNameNoSuffix(dbFiles[i], true);
		FOREACH(item, list)
		{
			TDDescription* data = item->second;
			if (item->first == 1)
			{
				info.mComment = data->mName;
			}
			else if (item->first == 2)
			{
				;
			}
			else if (item->first == 3)
			{
				if (data->mName == "All")
				{
					info.mOwner = SQLITE_OWNER::BOTH;
				}
				else if (data->mName == "Client")
				{
					info.mOwner = SQLITE_OWNER::CLIENT_ONLY;
				}
				else if (data->mName == "Server")
				{
					info.mOwner = SQLITE_OWNER::SERVER_ONLY;
				}
				else if (data->mName == "None")
				{
					info.mOwner = SQLITE_OWNER::NONE;
				}
				else
				{
					ERROR("表格所属错误:" + info.mSQLiteName);
				}
			}
			else if (item->first == 4)
			{
				info.mClientSQLite = StringUtility::stringToBool(data->mName);
			}
			else
			{
				SQLiteMember member;
				if (data->mOwner == "All")
				{
					member.mOwner = SQLITE_OWNER::BOTH;
				}
				else if (data->mOwner == "Client")
				{
					member.mOwner = SQLITE_OWNER::CLIENT_ONLY;
				}
				else if (data->mOwner == "Server")
				{
					member.mOwner = SQLITE_OWNER::SERVER_ONLY;
				}
				else if (data->mOwner == "None")
				{
					member.mOwner = SQLITE_OWNER::NONE;
				}
				else
				{
					ERROR("owner错误:" + info.mSQLiteName);
				}
				member.mMemberName = data->mName;
				member.mComment = data->mDesc;
				member.mTypeName = data->mType;
				int leftPos = 0;
				int rightPos = 0;
				if (findSubstr(member.mTypeName, "(", &leftPos) && findSubstr(member.mTypeName, ")", &rightPos))
				{
					member.mEnumRealType = member.mTypeName.substr(leftPos + 1, rightPos - leftPos - 1);
					member.mTypeName = member.mTypeName.erase(leftPos, rightPos - leftPos + 1);
				}
				info.mMemberList.push_back(member);
			}
		}
		sqliteInfoList.push_back(info);
	}
	
	// cpp
	string cppGameDataPath = cppGamePath + "DataBase/SQLite/Data/";
	string cppGameTablePath = cppGamePath + "DataBase/SQLite/Table/";
	myVector<SQLiteInfo> serverGameSQLiteList;
	for (const SQLiteInfo& info : sqliteInfoList)
	{
		if ((info.mOwner == SQLITE_OWNER::BOTH || info.mOwner == SQLITE_OWNER::SERVER_ONLY))
		{
			serverGameSQLiteList.push_back(info);
		}
	}
	// 删除C++的代码文件
	deleteFolder(cppGameDataPath);
	// SQLite的Table文件选择性删除,只删除非服务器使用的文件
	string patterns[2]{ ".cpp", ".h" };
	myVector<string> cppGameTableList;
	findFiles(cppGameTablePath, cppGameTableList, patterns, 2);
	for (const string& str : cppGameTableList)
	{
		bool needDelete = true;
		for (const SQLiteInfo& info : serverGameSQLiteList)
		{
			if ("SQLite" + info.mSQLiteName == getFileNameNoSuffix(str, true))
			{
				needDelete = false;
				break;
			}
		}
		if (needDelete)
		{
			deleteFile(str);
		}
	}

	// 生成代码文件
	for (const SQLiteInfo& info : serverGameSQLiteList)
	{
		// .h代码
		generateCppSQLiteDataFile(info, cppGameDataPath);
		generateCppSQLiteTableFile(info, cppGameTablePath);
	}

	const string gameBaseHeaderPath = cppGamePath + "Common/GameBase.h";
	const string gameBaseSourcePath = cppGamePath + "Common/GameBase.cpp";
	const string gameSTLPoolSourcePath = cppGamePath + "Common/GameSTLPoolRegister.cpp";
	generateCppSQLiteRegisteFile(serverGameSQLiteList, getFilePath(cppGameDataPath) + "/");
	generateCppSQLiteInstanceDeclare(serverGameSQLiteList, gameBaseHeaderPath, "");
	generateCppSQLiteInstanceDefine(serverGameSQLiteList, gameBaseSourcePath);
	generateCppSQLiteSTLPoolRegister(serverGameSQLiteList, gameSTLPoolSourcePath);
	generateCppSQLiteInstanceClear(serverGameSQLiteList, gameBaseSourcePath);
	FOR_VECTOR(serverGameSQLiteList)
	{
		if (serverGameSQLiteList[i].mSQLiteName == "Global")
		{
			generateCppGlobalConfig(serverGameSQLiteList[i], cppGameTablePath);
			break;
		}
	}

	//------------------------------------------------------------------------------------------------------------------------------
	// csharp
	if (!ClientHotFixPath.empty())
	{
		string csExcelDataHotFixPath = ClientHotFixPath + "DataBase/Excel/Data/";
		string csExcelTableHotFixPath = ClientHotFixPath + "DataBase/Excel/Table/";
		string csExcelTableDeclareHotFixPath = ClientHotFixPath + "Common/";

		string csSQLiteDataHotFixPath = ClientHotFixPath + "DataBase/SQLite/Data/";
		string csSQLiteTableHotFixPath = ClientHotFixPath + "DataBase/SQLite/Table/";
		// 筛选出Client的表格
		myVector<SQLiteInfo> clientSQLiteList;
		myVector<string> sqliteNameList;
		for (const SQLiteInfo& info : sqliteInfoList)
		{
			if (info.mOwner == SQLITE_OWNER::BOTH || info.mOwner == SQLITE_OWNER::CLIENT_ONLY)
			{
				clientSQLiteList.push_back(info);
				sqliteNameList.push_back(info.mSQLiteName);
			}
		}
		// 删除C#的代码文件,c#的只删除代码文件,不删除meta文件
		myVector<string> csDataFileList;
		findFiles(csExcelDataHotFixPath, csDataFileList, ".cs");
		findFiles(csSQLiteDataHotFixPath, csDataFileList, ".cs");
		for (const string& str : csDataFileList)
		{
			deleteFile(str);
		}
		myVector<string> csTableFileList;
		findFiles(csExcelTableHotFixPath, csTableFileList, ".cs");
		for (const string& str : csTableFileList)
		{
			// 只删除已经不存在的表格
			if (!sqliteNameList.contains(removeStartString(getFileNameNoSuffix(str, true), "Excel")))
			{
				deleteFile(str);
			}
		}

		// 生成代码文件
		for (const SQLiteInfo& info : clientSQLiteList)
		{
			// .cs代码的SQLite格式
			if (info.mClientSQLite)
			{
				generateCSharpSQLiteDataFile(info, csSQLiteDataHotFixPath);
				generateCSharpSQLiteTableFile(info, csSQLiteTableHotFixPath);
			}
			// .cs代码的Excel格式
			else
			{
				generateCSharpExcelDataFile(info, csExcelDataHotFixPath);
				generateCSharpExcelTableFile(info, csExcelTableHotFixPath);
			}
		}

		// 在上一层目录生成ExcelRegister.cs和SQLiteRegister.cs文件
		generateCSharpExcelRegisteFileFile(clientSQLiteList, getFilePath(csExcelDataHotFixPath) + "/");
		generateCSharpSQLiteRegisteFileFile(clientSQLiteList, getFilePath(csSQLiteDataHotFixPath) + "/");
		generateCSharpExcelDeclare(clientSQLiteList, csExcelTableDeclareHotFixPath);

		FOR_VECTOR(clientSQLiteList)
		{
			if (clientSQLiteList[i].mSQLiteName == "Global")
			{
				generateCSharpGlobalConfig(clientSQLiteList[i], csExcelTableHotFixPath);
				break;
			}
		}
	}
	print("完成生成SQLite");
	print("");
}

// TDSQLite.h和TDSQLite.cpp文件
void CodeSQLite::generateCppSQLiteDataFile(const SQLiteInfo& sqliteInfo, const string& dataFilePath)
{
	// 不含ID的成员字段列表
	myVector<SQLiteMember> memberNoIDList;
	for (const SQLiteMember& member : sqliteInfo.mMemberList)
	{
		if (member.mMemberName == "ID")
		{
			continue;
		}
		memberNoIDList.push_back(member);
	}
	// 不含ID以及非服务器字段的成员字段列表
	myVector<SQLiteMember> memberUsedInServerNoIDList;
	for (const SQLiteMember& member : memberNoIDList)
	{
		if (member.mOwner != SQLITE_OWNER::SERVER_ONLY && member.mOwner != SQLITE_OWNER::BOTH)
		{
			continue;
		}
		memberUsedInServerNoIDList.push_back(member);
	}

	// TDSQLite.h
	string header;
	string dataClassName = "TD" + sqliteInfo.mSQLiteName;
	line(header, "#pragma once");
	line(header, "");
	line(header, "#include \"SQLiteData.h\"");
	line(header, "");
	line(header, "// " + sqliteInfo.mComment);
	line(header, "class " + dataClassName + " : public SQLiteData");
	
	line(header, "{");
	line(header, "\tBASE(" + dataClassName + ", SQLiteData);");
	if (memberUsedInServerNoIDList.size() > 0)
	{
		line(header, "public:");
		for (const SQLiteMember& member : memberUsedInServerNoIDList)
		{
			string memberLine;
			if (member.mTypeName == "byte" ||
				member.mTypeName == "char" ||
				member.mTypeName == "ushort" ||
				member.mTypeName == "short" ||
				member.mTypeName == "int" ||
				member.mTypeName == "uint" ||
				member.mTypeName == "llong" ||
				member.mTypeName == "ullong")
			{
				memberLine = "\t" + member.mTypeName + " m" + member.mMemberName + " = 0;";
			}
			else if (member.mTypeName == "bool")
			{
				memberLine = "\t" + member.mTypeName + " m" + member.mMemberName + " = false;";
			}
			else if (member.mTypeName == "float")
			{
				memberLine = "\t" + member.mTypeName + " m" + member.mMemberName + " = 0.0f;";
			}
			else if (!member.mEnumRealType.empty())
			{
				if (startWith(member.mTypeName, "Vector<"))
				{
					memberLine = "\t" + member.mTypeName + " m" + member.mMemberName + ";";
				}
				else
				{
					memberLine = "\t" + member.mTypeName + " m" + member.mMemberName + " = (" + member.mTypeName + ")0;";
				}
			}
			else
			{
				memberLine = "\t" + member.mTypeName + " m" + member.mMemberName + ";";
			}
			appendWithAlign(memberLine, "// " + member.mComment, 60);
			line(header, memberLine);
		}
	}
	line(header, "public:");
	line(header, "\t" + dataClassName + "()");
	line(header, "\t{");
	FOR_VECTOR(memberNoIDList)
	{
		const SQLiteMember& member = memberNoIDList[i];
		const string& name = member.mMemberName;
		if (member.mOwner != SQLITE_OWNER::SERVER_ONLY && member.mOwner != SQLITE_OWNER::BOTH)
		{
			continue;
		}
		if (member.mEnumRealType.empty())
		{
			line(header, "\t\tregisteParam(m" + name + ", " + StringUtility::intToString(i + 1) + ");");
		}
		else
		{
			if (startWith(member.mTypeName, "Vector<"))
			{
				line(header, "\t\tregisteEnumListParam<" + member.mEnumRealType + ">(m" + name + ", " + StringUtility::intToString(i + 1) + ");");
			}
			else
			{
				line(header, "\t\tregisteEnumParam<" + member.mEnumRealType + ">(m" + name + ", " + StringUtility::intToString(i + 1) + ");");
			}
		}
	}
	line(header, "\t}");
	line(header, "\tvoid clone(SQLiteData* target) override;");
	line(header, "\tvoid checkAllColName(SQLiteTableBase* table) override;");
	line(header, "};", false);
	writeFileIfChanged(dataFilePath + dataClassName + ".h", ANSIToUTF8(header.c_str(), true));

	// TDSQLite.cpp
	string source;
	line(source, "#include \"" + dataClassName + ".h\"");
	line(source, "");
	line(source, "void " + dataClassName + "::clone(SQLiteData* target)");
	line(source, "{");
	line(source, "\tbase::clone(target);");
	// 先检查一下有没有需要拷贝的属性
	if (memberUsedInServerNoIDList.size() > 0)
	{
		line(source, "\tauto* targetData = static_cast<This*>(target);");
		for (const SQLiteMember& member : memberUsedInServerNoIDList)
		{
			// 如果是列表则调用列表的clone
			if (startWith(member.mTypeName, "Vector<"))
			{
				line(source, "\tm" + member.mMemberName + ".clone(targetData->m" + member.mMemberName + ");");
			}
			else
			{
				line(source, "\ttargetData->m" + member.mMemberName + " = m" + member.mMemberName + ";");
			}
		}
	}
	line(source, "}");
	line(source, "");
	line(source, "void " + dataClassName + "::checkAllColName(SQLiteTableBase* table)");
	line(source, "{");
	FOR_VECTOR(memberNoIDList)
	{
		const SQLiteMember& member = memberNoIDList[i];
		if (member.mOwner != SQLITE_OWNER::SERVER_ONLY && member.mOwner != SQLITE_OWNER::BOTH)
		{
			continue;
		}
		line(source, "\ttable->checkColName(\"" + member.mMemberName + "\", " + StringUtility::intToString(i + 1) + ");");
	}
	line(source, "}", false);
	writeFileIfChanged(dataFilePath + dataClassName + ".cpp", ANSIToUTF8(source.c_str(), true));
}

// SQLiteTable.h文件
void CodeSQLite::generateCppSQLiteTableFile(const SQLiteInfo& sqliteInfo, const string& tableFilePath)
{
	// SQLiteTable.h
	string dataClassName = "TD" + sqliteInfo.mSQLiteName;
	string tableClassName = "SQLite" + sqliteInfo.mSQLiteName;
	string tableFileName = tableFilePath + tableClassName + ".h";
	if (!isFileExist(tableFileName))
	{
		string table;
		line(table, "#pragma once");
		line(table, "");
		line(table, "#include \"" + dataClassName + ".h\"");
		line(table, "#include \"SQLiteTable.h\"");
		line(table, "");
		line(table, "class " + tableClassName + " : public SQLiteTable<" + dataClassName + ">");
		line(table, "{");
		line(table, "public:");
		line(table, "};", false);

		writeFileIfChanged(tableFileName, ANSIToUTF8(table.c_str(), true));
	}
}

// SQLiteRegister.h和SQLiteRegister.cpp文件
void CodeSQLite::generateCppSQLiteRegisteFile(const myVector<SQLiteInfo>& sqliteList, const string& filePath)
{
	// SQLiteRegister.h
	string str0;
	line(str0, "#pragma once");
	line(str0, "");
	line(str0, "#include \"GameBase.h\"");
	line(str0, "");
	line(str0, "class SQLiteRegister");
	line(str0, "{");
	line(str0, "public:");
	line(str0, "\tstatic void registeAll();");
	line(str0, "};", false);
	writeFileIfChanged(filePath + "SQLiteRegister.h", ANSIToUTF8(str0.c_str(), true));

	string str1;
	line(str1, "#include \"GameHeader.h\"");
	line(str1, "");
	line(str1, "void SQLiteRegister::registeAll()");
	line(str1, "{");
	FOR_I(sqliteList.size())
	{
		const string& sqliteName = sqliteList[i].mSQLiteName;
		line(str1, "\tmSQLite" + sqliteName + " = mSQLiteManager->registerTable<SQLite" + sqliteName + ">(\"" + sqliteName + "\");");
	}
	line(str1, "}", false);
	writeFileIfChanged(filePath + "SQLiteRegister.cpp", ANSIToUTF8(str1.c_str(), true));
}

string CodeSQLite::paramNameToFunctionName(const string& paramName)
{
	myVector<string> elements;
	split(paramName.c_str(), "_", elements);
	string functionName;
	FOR_VECTOR(elements)
	{
		string temp = toLower(elements[i]);
		temp[0] = toUpper(temp[0]);
		functionName += temp;
	}
	return functionName;
}

// 生成SQLiteGlobal对应的C++代码
void CodeSQLite::generateCppGlobalConfig(SQLiteInfo& globalConfig, const string& tableFilePath)
{
	// 先拆分出浮点数和整数的参数列表
	myVector<TDGlobal*> floatList;
	myVector<TDGlobal*> intList;
	SQLiteGlobal table;
	table.setTableName(globalConfig.mSQLiteName.c_str());
	table.init(SQLitePath + globalConfig.mSQLiteName + ".db");
	for (const auto& item : table.queryAll())
	{
		if (item.second->mParamValue > 0 && item.second->mParamValueInt > 0)
		{
			ERROR("全局参数表中浮点数和整数只能填写一个,参数名:" + item.second->mParamName);
			return;
		}
		if (item.second->mParamValue > 0)
		{
			floatList.push_back(item.second);
		}
		else if (item.second->mParamValueInt > 0)
		{
			intList.push_back(item.second);
		}
	}

	// SQLiteGlobal.h
	string dataClassName = "TD" + globalConfig.mSQLiteName;
	string tableClassName = "SQLite" + globalConfig.mSQLiteName;
	string tableFileName = tableFilePath + tableClassName + ".h";
	string tableString;
	line(tableString, "#pragma once");
	line(tableString, "");
	line(tableString, "#include \"" + dataClassName + ".h\"");
	line(tableString, "#include \"SQLiteTable.h\"");
	line(tableString, "");
	line(tableString, "class " + tableClassName + " : public SQLiteTable<" + dataClassName + ">");
	line(tableString, "{");
	line(tableString, "public:");
	line(tableString, "\tvoid init(const string& filePath) override;");
	FOR_VECTOR(floatList)
	{
		string temp = "\tfloat get" + paramNameToFunctionName(floatList[i]->mParamName) + "() const";
		appendWithAlign(temp, "{ return " + floatList[i]->mParamName + "; }", 40);
		line(tableString, temp);
	}
	FOR_VECTOR(intList)
	{
		string temp = "\tint get" + paramNameToFunctionName(intList[i]->mParamName) + "() const";
		appendWithAlign(temp, "{ return " + intList[i]->mParamName + "; }", 40);
		line(tableString, temp);
	}
	line(tableString, "protected:");
	FOR_VECTOR(floatList)
	{
		string temp = "\tfloat " + floatList[i]->mParamName + " = 0.0f;";
		appendWithAlign(temp, "// " + floatList[i]->mParamDesc, 40);
		line(tableString, temp);
	}
	FOR_VECTOR(intList)
	{
		string temp = "\tint " + intList[i]->mParamName + " = 0;";
		appendWithAlign(temp, "// " + intList[i]->mParamDesc, 40);
		line(tableString, temp);
	}
	line(tableString, "};", false);

	writeFileIfChanged(tableFileName, ANSIToUTF8(tableString.c_str(), true));

	// SQLiteGlobal.cpp 只更新特定部分代码
	string cppFileName = tableFilePath + tableClassName + ".cpp";
	myVector<string> codeList;
	int lineStart = -1;
	if (!findCustomCode(cppFileName, codeList, lineStart,
		[](const string& codeLine) { return endWith(codeLine, "// auto generate start"); },
		[](const string& codeLine) { return endWith(codeLine, "// auto generate end"); }))
	{
		return;
	}
	FOR_VECTOR(floatList)
	{
		codeList.insert(++lineStart, "\tGET_FLOAT(" + floatList[i]->mParamName + ");");
	}
	codeList.insert(++lineStart, "");
	FOR_VECTOR(intList)
	{
		codeList.insert(++lineStart, "\tGET_INT(" + intList[i]->mParamName + ");");
	}
	writeFileIfChanged(cppFileName, ANSIToUTF8(codeListToString(codeList).c_str(), true));
}

void CodeSQLite::generateCppSQLiteInstanceDeclare(const myVector<SQLiteInfo>& sqliteList, const string& gameBaseHeaderFileName, const string& exprtMacro)
{
	// 更新GameBase.h的特定部分代码
	myVector<string> codeList;
	int lineStart = -1;
	if (!findCustomCode(gameBaseHeaderFileName, codeList, lineStart,
		[](const string& codeLine) { return endWith(codeLine, "// auto generate start SQLite Extern"); },
		[](const string& codeLine) { return endWith(codeLine, "// auto generate end SQLite Extern"); }))
	{
		return;
	}

	for (const SQLiteInfo& info : sqliteList)
	{
		codeList.insert(++lineStart, "\t" + exprtMacro + "extern SQLite" + info.mSQLiteName + "* mSQLite" + info.mSQLiteName + ";");
	}
	writeFileIfChanged(gameBaseHeaderFileName, ANSIToUTF8(codeListToString(codeList).c_str(), true));
}

void CodeSQLite::generateCppSQLiteInstanceDefine(const myVector<SQLiteInfo>& sqliteList, const string& gameBaseCppFileName)
{
	// 更新GameBase.cpp的特定部分代码
	myVector<string> codeList;
	int lineStart = -1;
	if (!findCustomCode(gameBaseCppFileName, codeList, lineStart,
		[](const string& codeLine) { return endWith(codeLine, "// auto generate start SQLite Define"); },
		[](const string& codeLine) { return endWith(codeLine, "// auto generate end SQLite Define"); }))
	{
		return;
	}
	for (const SQLiteInfo& info : sqliteList)
	{
		codeList.insert(++lineStart, "\tSQLite" + info.mSQLiteName + "* mSQLite" + info.mSQLiteName + ";");
	}
	writeFileIfChanged(gameBaseCppFileName, ANSIToUTF8(codeListToString(codeList).c_str(), true));
}

// SQLiteSTLPoolRegister.h
void CodeSQLite::generateCppSQLiteSTLPoolRegister(const myVector<SQLiteInfo>& sqliteList, const string& gameSTLPoolFile)
{
	// 更新GameBase.h的特定部分代码
	myVector<string> codeList;
	int lineStart = -1;
	if (!findCustomCode(gameSTLPoolFile, codeList, lineStart,
		[](const string& codeLine) { return endWith(codeLine, "// SQLite数据类型"); },
		[](const string& codeLine) { return codeLine.length() == 0 || findSubstr(codeLine, "}"); }))
	{
		return;
	}
	for (const SQLiteInfo& info : sqliteList)
	{
		codeList.insert(++lineStart, "\tmVectorPoolManager->registeVectorPool<TD" + info.mSQLiteName + "*>();");
	}
	writeFileIfChanged(gameSTLPoolFile, ANSIToUTF8(codeListToString(codeList).c_str(), true));
}

void CodeSQLite::generateCppSQLiteInstanceClear(const myVector<SQLiteInfo>& sqliteList, const string& gameBaseCppFileName)
{
	// 更新GameBase.cpp的特定部分
	myVector<string> codeList;
	int lineStart = -1;
	if (!findCustomCode(gameBaseCppFileName, codeList, lineStart,
		[](const string& codeLine) { return endWith(codeLine, "// auto generate start SQLite Clear"); },
		[](const string& codeLine) { return endWith(codeLine, "// auto generate end SQLite Clear"); }))
	{
		return;
	}

	for (const SQLiteInfo& info : sqliteList)
	{
		codeList.insert(++lineStart, "\t\tmSQLite" + info.mSQLiteName + " = nullptr;");
	}
	writeFileIfChanged(gameBaseCppFileName, ANSIToUTF8(codeListToString(codeList).c_str(), true));
}

// ExcelData.cs文件
void CodeSQLite::generateCSharpExcelDataFile(const SQLiteInfo& sqliteInfo, const string& dataFileHotFixPath)
{
	if (sqliteInfo.mClientSQLite || sqliteInfo.mOwner == SQLITE_OWNER::SERVER_ONLY || sqliteInfo.mOwner == SQLITE_OWNER::NONE)
	{
		return;
	}
	string file;
	string dataClassName = "ED" + sqliteInfo.mSQLiteName;
	line(file, "using System;");
	line(file, "using System.Collections.Generic;");
	line(file, "using UnityEngine;");
	line(file, "");
	line(file, "// " + sqliteInfo.mComment);
	line(file, "public class " + dataClassName + " : ExcelData");
	line(file, "{");
	uint memberCount = sqliteInfo.mMemberList.size();
	mySet<string> listMemberSet;
	myVector<pair<string, string>> listMemberList;
	FOR_I(memberCount)
	{
		const SQLiteMember& member = sqliteInfo.mMemberList[i];
		if (member.mMemberName == "ID")
		{
			continue;
		}
		// 不在客户端使用的则不定义成员变量
		if (member.mOwner != SQLITE_OWNER::CLIENT_ONLY && member.mOwner != SQLITE_OWNER::BOTH)
		{
			continue;
		}
		string typeName = convertToCSharpType(member.mTypeName);
		// 列表类型的成员变量存储到单独的列表,因为需要分配内存
		bool isList = findString(typeName.c_str(), "List");
		if (isList)
		{
			listMemberList.push_back(make_pair(typeName, member.mMemberName));
			listMemberSet.insert(member.mMemberName);
		}
		string memberLine;
		if (!isList)
		{
			memberLine = "\tpublic " + typeName + " m" + member.mMemberName + ";";
		}
		else
		{
			memberLine = "\tpublic " + typeName + " m" + member.mMemberName + " = new();";
		}
		appendWithAlign(memberLine, "// " + sqliteInfo.mMemberList[i].mComment, 52);
		line(file, memberLine);
	}
	line(file, "\tpublic override void read(SerializerRead reader)");
	line(file, "\t{");
	line(file, "\t\tbase.read(reader);");
	FOR_I(memberCount)
	{
		const SQLiteMember& memberInfo = sqliteInfo.mMemberList[i];
		if (memberInfo.mMemberName == "ID")
		{
			continue;
		}
		// 不在客户端使用的则不读取
		if (memberInfo.mOwner != SQLITE_OWNER::CLIENT_ONLY && memberInfo.mOwner != SQLITE_OWNER::BOTH)
		{
			continue;
		}
		string typeName = convertToCSharpType(memberInfo.mTypeName);
		if (typeName == "string")
		{
			line(file, "\t\treader.readString(out m" + memberInfo.mMemberName + ");");
		}
		else if (listMemberSet.contains(memberInfo.mMemberName))
		{
			if (memberInfo.mEnumRealType == "byte")
			{
				line(file, "\t\treader.readEnumByteList(m" + memberInfo.mMemberName + ");");
			}
			else
			{
				line(file, "\t\treader.readList(m" + memberInfo.mMemberName + ");");
			}
		}
		else if (memberInfo.mEnumRealType == "byte")
		{
			line(file, "\t\treader.readEnumByte(out m" + memberInfo.mMemberName + ");");
		}
		else
		{
			line(file, "\t\treader.read(out m" + memberInfo.mMemberName + ");");
		}
	}
	line(file, "\t}");
	line(file, "}", false);
	writeFileIfChanged(dataFileHotFixPath + dataClassName + ".cs", ANSIToUTF8(file.c_str(), true));
}

// ExcelTable.cs文件
void CodeSQLite::generateCSharpExcelTableFile(const SQLiteInfo& sqliteInfo, const string& tableFileHotFixPath)
{
	if (sqliteInfo.mClientSQLite || sqliteInfo.mOwner == SQLITE_OWNER::SERVER_ONLY || sqliteInfo.mOwner == SQLITE_OWNER::NONE)
	{
		return;
	}
	// 如果文件已经存在,则不会修改
	string tableClassName = "Excel" + sqliteInfo.mSQLiteName;
	if (isFileExist(tableFileHotFixPath + tableClassName + ".cs"))
	{
		return;
	}
	// SQLiteTable.cs文件
	string dataClassName = "ED" + sqliteInfo.mSQLiteName;
	string table;
	line(table, "using System;");
	line(table, "using System.Collections.Generic;");
	line(table, "");
	line(table, "public class " + tableClassName + " : ExcelTableT<" + dataClassName + ">");
	line(table, "{}", false);
	writeFileIfChanged(tableFileHotFixPath + tableClassName + ".cs", ANSIToUTF8(table.c_str(), true));
}

// SQLiteData.cs文件
void CodeSQLite::generateCSharpSQLiteDataFile(const SQLiteInfo& sqliteInfo, const string& dataFileHotFixPath)
{
	if (!sqliteInfo.mClientSQLite || sqliteInfo.mOwner == SQLITE_OWNER::SERVER_ONLY || sqliteInfo.mOwner == SQLITE_OWNER::NONE)
	{
		return;
	}
	string file;
	string dataClassName = "SD" + sqliteInfo.mSQLiteName;
	line(file, "using Mono.Data.Sqlite;");
	line(file, "using System;");
	line(file, "using System.Collections.Generic;");
	line(file, "using UnityEngine;");
	line(file, "");
	line(file, "// " + sqliteInfo.mComment);
	line(file, "public class " + dataClassName + " : SQLiteData");
	line(file, "{");
	myVector<pair<string, string>> listMemberList;
	for (const SQLiteMember& member : sqliteInfo.mMemberList)
	{
		if (member.mMemberName == "ID")
		{
			continue;
		}
		line(file, "\tpublic const string " + member.mMemberName + " = " + "\"" + member.mMemberName + "\";");
	}
	for (const SQLiteMember& member : sqliteInfo.mMemberList)
	{
		if (member.mMemberName == "ID")
		{
			continue;
		}
		// 因为模板文件是按照C++来写的,但是有些类型在C#中是没有的,所以要转换为C#中对应的类型
		string typeName = cppTypeToCSharpType(member.mTypeName);
		if (!member.mEnumRealType.empty())
		{
			int pos0;
			if (findString(typeName.c_str(), "List<", &pos0))
			{
				int pos1;
				findString(typeName.c_str(), ">", &pos1);
				replace(typeName, pos0 + (int)strlen("List<"), pos1, member.mEnumRealType);
			}
			else
			{
				typeName = convertToCSharpType(member.mEnumRealType);
			}
		}

		string publicType;
		if (member.mOwner == SQLITE_OWNER::CLIENT_ONLY || member.mOwner == SQLITE_OWNER::BOTH)
		{
			publicType = "public";
		}
		else
		{
			publicType = "protected";
		}
		// 列表类型的成员变量存储到单独的列表,因为需要分配内存
		bool isList = findString(typeName.c_str(), "List");
		if (isList)
		{
			listMemberList.push_back(make_pair(typeName, member.mMemberName));
		}

		string memberLine;
		if (!isList)
		{
			memberLine = "\t" + publicType + " " + typeName + " m" + member.mMemberName + ";";
		}
		else
		{
			memberLine = "\t" + publicType + " " + typeName + " m" + member.mMemberName + " = new();";
		}
		appendWithAlign(memberLine, "// " + member.mComment, 52);
		line(file, memberLine);
	}
	line(file, "\tpublic override void parse(SqliteDataReader reader)");
	line(file, "\t{");
	line(file, "\t\tbase.parse(reader);");
	const uint memberCount = sqliteInfo.mMemberList.size();
	FOR_I(memberCount)
	{
		const SQLiteMember& member = sqliteInfo.mMemberList[i];
		if (member.mMemberName == "ID")
		{
			continue;
		}
		line(file, "\t\tparseParam(reader, ref m" + member.mMemberName + ", " + intToString(i) + ");");
	}
	line(file, "\t}");
	line(file, "}", false);
	writeFileIfChanged(dataFileHotFixPath + dataClassName + ".cs", ANSIToUTF8(file.c_str(), true));
}

// SQLiteTable.cs文件
void CodeSQLite::generateCSharpSQLiteTableFile(const SQLiteInfo& sqliteInfo, const string& tableFileHotFixPath)
{
	if (!sqliteInfo.mClientSQLite || sqliteInfo.mOwner == SQLITE_OWNER::SERVER_ONLY || sqliteInfo.mOwner == SQLITE_OWNER::NONE)
	{
		return;
	}
	string tableClassName = "SQLite" + sqliteInfo.mSQLiteName;
	const string fullPath = tableFileHotFixPath + tableClassName + ".cs";
	// 不覆盖现有文件
	if (isFileExist(fullPath))
	{
		return;
	}
	// SQLiteTable.cs文件
	string table;
	line(table, "using System;");
	line(table, "using System.Collections.Generic;");
	line(table, "");
	line(table, "public class " + tableClassName + " : SQLiteTable");
	line(table, "{");
	line(table, "}", false);
	writeFileIfChanged(fullPath, ANSIToUTF8(table.c_str(), true));
}

// ExcelRegister.cs文件
void CodeSQLite::generateCSharpExcelRegisteFileFile(const myVector<SQLiteInfo>& sqliteInfo, const string& fileHotFixPath)
{
	bool hasHotFixMember = false;
	for (const SQLiteInfo& info : sqliteInfo)
	{
		if (!info.mClientSQLite && info.mOwner != SQLITE_OWNER::SERVER_ONLY && info.mOwner != SQLITE_OWNER::NONE)
		{
			hasHotFixMember = true;
			break;
		}
	}

	if (hasHotFixMember)
	{
		// 热更工程中的表格注册
		string hotFixfile;
		line(hotFixfile, "using System;");
		line(hotFixfile, "using static GBR;");
		line(hotFixfile, "using static FrameBaseHotFix;");
		line(hotFixfile, "");
		line(hotFixfile, "public class ExcelRegister");
		line(hotFixfile, "{");
		line(hotFixfile, "\tpublic static void registeAll()");
		line(hotFixfile, "\t{");
		for (const SQLiteInfo& info : sqliteInfo)
		{
			if (!info.mClientSQLite && info.mOwner != SQLITE_OWNER::SERVER_ONLY && info.mOwner != SQLITE_OWNER::NONE)
			{
				string lineStr = "\t\tregisteTable(out mExcel%s, typeof(ED%s), \"%s\");";
				replaceAll(lineStr, "%s", info.mSQLiteName);
				line(hotFixfile, lineStr);
			}
		}
		line(hotFixfile, "");
		line(hotFixfile, "\t\t// 进入热更以后,所有资源都处于可用状态");
		line(hotFixfile, "\t\tmExcelManager.resourceAvailable();");
		line(hotFixfile, "\t}");
		line(hotFixfile, "\t//------------------------------------------------------------------------------------------------------------------------------");
		line(hotFixfile, "\tprotected static void registeTable<T>(out T table, Type dataType, string tableName) where T : ExcelTable");
		line(hotFixfile, "\t{");
		line(hotFixfile, "\t\ttable = mExcelManager.registe(tableName, typeof(T), dataType) as T;");
		line(hotFixfile, "\t}");
		line(hotFixfile, "}", false);

		writeFileIfChanged(fileHotFixPath + "ExcelRegister.cs", ANSIToUTF8(hotFixfile.c_str(), true));
	}
	else
	{
		deleteFile(fileHotFixPath + "ExcelRegister.cs");
	}
}

// SQLiteRegister.cs文件
void CodeSQLite::generateCSharpSQLiteRegisteFileFile(const myVector<SQLiteInfo>& sqliteInfo, const string& fileHotFixPath)
{
	// 热更工程中的表格注册
	bool hasHotFixSQLite = false;
	for (const SQLiteInfo& info : sqliteInfo)
	{
		if (info.mClientSQLite && info.mOwner != SQLITE_OWNER::SERVER_ONLY && info.mOwner != SQLITE_OWNER::NONE)
		{
			hasHotFixSQLite = true;
		}
	}
	if (hasHotFixSQLite)
	{
		string hotFixfile;
		line(hotFixfile, "using System;");
		line(hotFixfile, "using static GBR;");
		line(hotFixfile, "using static FrameBaseHotFix;");
		line(hotFixfile, "");
		line(hotFixfile, "public class SQLiteRegister");
		line(hotFixfile, "{");
		line(hotFixfile, "\tpublic static void registeAll()");
		line(hotFixfile, "\t{");
		for (const SQLiteInfo& info : sqliteInfo)
		{
			if (info.mClientSQLite && info.mOwner != SQLITE_OWNER::SERVER_ONLY && info.mOwner != SQLITE_OWNER::NONE)
			{
				string lineStr = "\t\tregisteTable(out mSQLite%s, typeof(SD%s), \"%s\");";
				replaceAll(lineStr, "%s", info.mSQLiteName);
				line(hotFixfile, lineStr);
			}
		}
		line(hotFixfile, "");
		line(hotFixfile, "\t\t// 进入热更以后,所有资源都处于可用状态");
		line(hotFixfile, "\t\tmSQLiteManager.resourceAvailable();");
		line(hotFixfile, "\t}");
		line(hotFixfile, "\t//------------------------------------------------------------------------------------------------------------------------------");
		line(hotFixfile, "\tprotected static void registeTable<T>(out T table, Type dataType, string tableName) where T : SQLiteTable");
		line(hotFixfile, "\t{");
		line(hotFixfile, "\t\ttable = mSQLiteManager.registeTable(typeof(T), dataType, tableName) as T;");
		line(hotFixfile, "\t}");
		line(hotFixfile, "}", false);

		writeFileIfChanged(fileHotFixPath + "SQLiteRegister.cs", ANSIToUTF8(hotFixfile.c_str(), true));
	}
	else
	{
		deleteFile(fileHotFixPath + "SQLiteRegister.cs");
	}
}

// GameBaseExcel.cs文件
void CodeSQLite::generateCSharpExcelDeclare(const myVector<SQLiteInfo>& sqliteInfo, const string& fileHotFixPath)
{
	bool hasHotfixMember = false;
	for (const SQLiteInfo& info : sqliteInfo)
	{
		if (!info.mClientSQLite && info.mOwner != SQLITE_OWNER::SERVER_ONLY && info.mOwner != SQLITE_OWNER::NONE)
		{
			hasHotfixMember = true;
			break;
		}
	}

	if (hasHotfixMember)
	{
		// 热更工程中的表格注册
		string hotFixfile;
		line(hotFixfile, "using System;");
		line(hotFixfile, "");
		line(hotFixfile, "// FrameBase的部分类,用于定义Excel表格的对象");
		line(hotFixfile, "public partial class GBR");
		line(hotFixfile, "{");
		for (const SQLiteInfo& info : sqliteInfo)
		{
			if (!info.mClientSQLite && info.mOwner != SQLITE_OWNER::SERVER_ONLY && info.mOwner != SQLITE_OWNER::NONE)
			{
				line(hotFixfile, "\tpublic static Excel" + info.mSQLiteName + " mExcel" + info.mSQLiteName + ";");
			}
		}
		line(hotFixfile, "}", false);

		writeFileIfChanged(fileHotFixPath + "GameBaseExcelILR.cs", ANSIToUTF8(hotFixfile.c_str(), true));
	}
	else
	{
		deleteFile(fileHotFixPath + "GameBaseExcelILR.cs");
	}
}

// 生成ExcelGlobal对应的cs代码
void CodeSQLite::generateCSharpGlobalConfig(SQLiteInfo& globalConfig, const string& tableFilePath)
{
	// 先拆分出浮点数和整数的参数列表
	myVector<TDGlobal*> floatList;
	myVector<TDGlobal*> intList;
	SQLiteGlobal table;
	table.setTableName(globalConfig.mSQLiteName.c_str());
	table.init(SQLitePath + globalConfig.mSQLiteName + ".db");
	for (const auto& item : table.queryAll())
	{
		if (item.second->mParamValue > 0 && item.second->mParamValueInt > 0)
		{
			ERROR("全局参数表中浮点数和整数只能填写一个,参数名:" + item.second->mParamName);
			return;
		}
		if (item.second->mParamValue > 0)
		{
			floatList.push_back(item.second);
		}
		else if (item.second->mParamValueInt > 0)
		{
			intList.push_back(item.second);
		}
	}

	// ExcelGlobal.cs
	string dataClassName = "ED" + globalConfig.mSQLiteName;
	string tableClassName = "Excel" + globalConfig.mSQLiteName;
	string tableFileName = tableFilePath + tableClassName + ".cs";
	string tableString;
	line(tableString, "using static MathUtility;");
	line(tableString, "");
	line(tableString, "public partial class " + tableClassName + " : ExcelTableT<" + dataClassName + ">");
	line(tableString, "{");
	FOR_VECTOR(floatList)
	{
		string temp = "\tprotected float " + floatList[i]->mParamName + ";";
		appendWithAlign(temp, "// " + floatList[i]->mParamDesc, 60);
		line(tableString, temp);
	}
	FOR_VECTOR(intList)
	{
		string temp = "\tprotected int " + intList[i]->mParamName + ";";
		appendWithAlign(temp, "// " + intList[i]->mParamDesc, 60);
		line(tableString, temp);
	}

	line(tableString, "\tprotected override void onOpenFile()");
	line(tableString, "\t{");
	line(tableString, "\t\tusing var a = new DicScope<string, int>(out var intParamList);");
	line(tableString, "\t\tusing var b = new DicScope<string, float>(out var floatParamList);");
	line(tableString, "\t\tforeach (EDGlobal data in queryAll())");
	line(tableString, "\t\t{");
	line(tableString, "\t\t\t// 填了整数类型的,就认为是整数类型的参数");
	line(tableString, "\t\t\tif (data.mParamValueInt != 0)");
	line(tableString, "\t\t\t{");
	line(tableString, "\t\t\t\tintParamList.Add(data.mParamName, data.mParamValueInt);");
	line(tableString, "\t\t\t}");
	line(tableString, "\t\t\telse if (!isFloatZero(data.mParamValue))");
	line(tableString, "\t\t\t{");
	line(tableString, "\t\t\t\tfloatParamList.Add(data.mParamName, data.mParamValue);");
	line(tableString, "\t\t\t}");
	line(tableString, "\t\t}");
	FOR_VECTOR(floatList)
	{
		line(tableString, "\t\t" + floatList[i]->mParamName + " = floatParamList[\"" + floatList[i]->mParamName + "\"];");
	}
	line(tableString, "");
	FOR_VECTOR(intList)
	{
		line(tableString, "\t\t" + intList[i]->mParamName + " = intParamList[\"" + intList[i]->mParamName + "\"];");
	}
	line(tableString, "\t}");
	FOR_VECTOR(floatList)
	{
		string temp = "\tpublic float get" + paramNameToFunctionName(floatList[i]->mParamName) + "()";
		appendWithAlign(temp, "{ return " + floatList[i]->mParamName + "; }", 60);
		line(tableString, temp);
	}
	FOR_VECTOR(intList)
	{
		string temp = "\tpublic int get" + paramNameToFunctionName(intList[i]->mParamName) + "()";
		appendWithAlign(temp, "{ return " + intList[i]->mParamName + "; }", 60);
		line(tableString, temp);
	}
	line(tableString, "}");
	writeFileIfChanged(tableFileName, ANSIToUTF8(tableString.c_str(), true));
}