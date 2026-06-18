#include "CodeSQLite.h"
#include "SQLiteDescription.h"
#include "SQLiteGlobal.h"
#include "SQLiteCommon.h"

myVector<string> CodeSQLite::mSQLiteForServerTableList;

void CodeSQLite::generate()
{
	print("正在生成SQLite");

	// 先读取表格描述
	myVector<SQLiteInfo> sqliteInfoList;
	for (const string& file : findFiles(SQLitePath, ".db"))
	{
		SQLiteDescription table;
		table.setTableName("Z_Description");
		table.init(file);
		const auto& list = table.queryAll();
		if (list.size() == 0)
		{
			continue;
		}
		SQLiteInfo info;
		info.mMemberList.clear();
		info.mSQLiteName = getFileNameNoSuffix(file, true);
		for (const auto& item : list)
		{
			TDDescription* data = item.second;
			if (item.first == 1)
			{
				info.mComment = data->mName;
			}
			else if (item.first == 2)
			{
				;
			}
			else if (item.first == 3)
			{
				if (data->mName == "All")
				{
					info.mOwner = OWNER::BOTH;
				}
				else if (data->mName == "Client")
				{
					info.mOwner = OWNER::CLIENT_ONLY;
				}
				else if (data->mName == "Server")
				{
					info.mOwner = OWNER::SERVER_ONLY;
				}
				else if (data->mName == "None")
				{
					info.mOwner = OWNER::NONE;
				}
				else
				{
					ERROR("表格所属错误:" + info.mSQLiteName);
				}
			}
			else if (item.first == 4)
			{
				info.mClientSQLite = StringUtility::SToBool(data->mName);
			}
			else
			{
				SQLiteMember member;
				if (data->mOwner == "All")
				{
					member.mOwner = OWNER::BOTH;
				}
				else if (data->mOwner == "Client")
				{
					member.mOwner = OWNER::CLIENT_ONLY;
				}
				else if (data->mOwner == "Server")
				{
					member.mOwner = OWNER::SERVER_ONLY;
				}
				else if (data->mOwner == "None")
				{
					member.mOwner = OWNER::NONE;
				}
				else
				{
					ERROR("owner错误:" + info.mSQLiteName);
				}
				member.mName = data->mName;
				member.mComment = data->mDesc;
				member.mType = data->mType;
				member.mLinkTable = data->mLinkTable;
				int leftPos = 0;
				int rightPos = 0;
				if (findSubstr(member.mType, "(", &leftPos) && findSubstr(member.mType, ")", &rightPos))
				{
					member.mEnumRealType = member.mType.substr(leftPos + 1, rightPos - leftPos - 1);
					member.mType = member.mType.erase(leftPos, rightPos - leftPos + 1);
				}
				info.mMemberList.push_back(member);
			}
		}
		SQLiteCommon tableMain;
		tableMain.setTableName(info.mSQLiteName.c_str());
		tableMain.init(file);
		const myMap<int, TDCommon*>& listMain = tableMain.queryAll();
		FOREACH(item0, listMain)
		{
			info.mDataMap.insert(item0->first, item0->second->getDataList());
		}
		sqliteInfoList.push_back(info);
	}
	
	// cpp
	string cppGameDataPath = cppGamePath + "DataBase/Excel/Data/";
	string cppGameTablePath = cppGamePath + "DataBase/Excel/Table/";
	myVector<SQLiteInfo> serverGameSQLiteList;
	mSQLiteForServerTableList.clear();
	for (const SQLiteInfo& info : sqliteInfoList)
	{
		if ((info.mOwner == OWNER::BOTH || info.mOwner == OWNER::SERVER_ONLY))
		{
			serverGameSQLiteList.push_back(info);
			mSQLiteForServerTableList.push_back(info.mSQLiteName);
		}
	}
	// 删除C++的代码文件,只删ExcelData中的,因为里面都是自动生成的,ExcelTable中的包含手动写的代码,而且是Excel和SQLite混在一起,就不删除了
	deleteFolder(cppGameDataPath);

	// 生成代码文件
	for (const SQLiteInfo& info : serverGameSQLiteList)
	{
		generateCppSQLiteDataFile(info, cppGameDataPath);
		generateCppSQLiteTableFile(info, cppGameTablePath);
	}

	//------------------------------------------------------------------------------------------------------------------------------
	// csharp
	if (!ClientHotFixPath.empty())
	{
		string csSQLiteDataHotFixPath = ClientHotFixPath + "DataBase/SQLite/Data/";
		string csSQLiteTableHotFixPath = ClientHotFixPath + "DataBase/SQLite/Table/";
		// 筛选出Client的表格
		myVector<SQLiteInfo> clientSQLiteList;
		myVector<string> sqliteNameList;
		for (const SQLiteInfo& info : sqliteInfoList)
		{
			if (info.mOwner == OWNER::BOTH || info.mOwner == OWNER::CLIENT_ONLY)
			{
				clientSQLiteList.push_back(info);
				sqliteNameList.push_back(info.mSQLiteName);
			}
		}
		// 删除C#的代码文件,c#的只删除代码文件,不删除meta文件
		for (const string& str : findFiles(csSQLiteDataHotFixPath, ".cs"))
		{
			deleteFile(str);
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
		}

		// 在上一层目录生成SQLiteRegister.cs文件
		generateCSharpSQLiteRegisteFileFile(clientSQLiteList, getFilePath(csSQLiteDataHotFixPath) + "/");
	}
	print("完成生成SQLite");
	print("");
}

// ExcelData.h和ExcelData.cpp文件
void CodeSQLite::generateCppSQLiteDataFile(const SQLiteInfo& sqliteInfo, const string& dataFilePath)
{
	// 不含ID的成员字段列表
	myVector<SQLiteMember> memberNoIDList;
	for (const SQLiteMember& member : sqliteInfo.mMemberList)
	{
		if (member.mName == "ID")
		{
			continue;
		}
		memberNoIDList.push_back(member);
	}
	// 不含ID以及非服务器字段的成员字段列表
	myVector<SQLiteMember> memberUsedInServerNoIDList;
	for (const SQLiteMember& member : memberNoIDList)
	{
		if (member.mOwner != OWNER::SERVER_ONLY && member.mOwner != OWNER::BOTH)
		{
			continue;
		}
		memberUsedInServerNoIDList.push_back(member);
	}

	// first是变量名,second是注释,用于通过变量来访问ID
	myMap<int, pair<string, string>> variableList;
	for (const auto& item : sqliteInfo.mDataMap)
	{
		const auto& tempMap = item.second;
		// 从固定的字段名中获取变量名
		const string& variableName = tempMap.get("VariableName", "");
		const string& variableComment = tempMap.get("VariableComment", "");
		if (variableName.length() > 0)
		{
			variableList.insert(item.first, make_pair(variableName, variableComment));
		}
	}

	// ExcelData.h
	string header;
	string dataClassName = "ED" + sqliteInfo.mSQLiteName;
	line(header, "// auto generate start");
	line(header, "#pragma once");
	line(header, "");
	line(header, "#include \"ExcelData.h\"");
	line(header, "");
	line(header, "// " + sqliteInfo.mComment);
	line(header, "class " + dataClassName + " : public ExcelData");
	line(header, "{");
	line(header, "\tBASE(" + dataClassName + ", ExcelData);");
	if (variableList.size() > 0 || memberUsedInServerNoIDList.size() > 0)
	{
		line(header, "public:");
	}
	if (variableList.size() > 0)
	{
		for (const auto& item : variableList)
		{
			string str = "\tstatic constexpr int " + item.second.first + " = " + IToS(item.first) + ";";
			appendWithAlign(str, "// " + item.second.second, 64);
			line(header, str);
		}
		line(header, "");
	}
	for (const SQLiteMember& member : memberUsedInServerNoIDList)
	{
		const string& type = member.mType;
		const string& name = member.mName;
		string memberLine;
		if (type == "byte" ||
			type == "char" ||
			type == "ushort" ||
			type == "short" ||
			type == "int" ||
			type == "uint" ||
			type == "llong" ||
			type == "ullong")
		{
			memberLine = "\t" + type + " m" + name + " = 0;";
		}
		else if (type == "bool")
		{
			memberLine = "\t" + type + " m" + name + " = false;";
		}
		else if (type == "float")
		{
			memberLine = "\t" + type + " m" + name + " = 0.0f;";
		}
		else if (!member.mEnumRealType.empty())
		{
			if (startWith(type, "Vector<"))
			{
				memberLine = "\t" + type + " m" + name + ";";
			}
			else
			{
				memberLine = "\t" + type + " m" + name + " = (" + type + ")0;";
			}
		}
		else
		{
			memberLine = "\t" + type + " m" + name + ";";
		}
		appendWithAlign(memberLine, "// " + member.mComment, 60);
		line(header, memberLine);
	}
	line(header, "public:");
	line(header, "\tvoid cloneTo(ExcelData* target) override;");
	line(header, "\tvoid read(SerializerRead* reader) override;");
	line(header, "};");
	line(header, "// auto generate end", false);
	writeFile(dataFilePath + dataClassName + ".h", header);

	// ExcelData.cpp
	string source;
	line(source, "// auto generate start");
	line(source, "#include \"" + dataClassName + ".h\"");
	line(source, "");
	line(source, "void " + dataClassName + "::cloneTo(ExcelData* target)");
	line(source, "{");
	line(source, "\tbase::cloneTo(target);");
	// 先检查一下有没有需要拷贝的属性
	if (memberUsedInServerNoIDList.size() > 0)
	{
		line(source, "\tauto* targetData = static_cast<This*>(target);");
		for (const SQLiteMember& member : memberUsedInServerNoIDList)
		{
			const string& name = member.mName;
			// 如果是列表则调用列表的cloneTo
			if (startWith(name, "Vector<"))
			{
				line(source, "\tm" + name + ".cloneTo(targetData->m" + name + ");");
			}
			else
			{
				line(source, "\ttargetData->m" + name + " = m" + name + ";");
			}
		}
	}
	line(source, "}");
	line(source, "");
	line(source, "void " + dataClassName + "::read(SerializerRead* reader)");
	line(source, "{");
	line(source, "\tbase::read(reader);");
	for (const SQLiteMember& member : memberUsedInServerNoIDList)
	{
		const string& type = member.mType;
		const string& name = member.mName;
		if (type == "string")
		{
			line(source, "\treader->readString(m" + name + ");");
		}
		else if (type == "Vector2Int")
		{
			line(source, "\treader->readVector2Int(m" + name + ");");
		}
		else if (type == "Vector2")
		{
			line(source, "\treader->readVector2(m" + name + ");");
		}
		else if (type == "Vector3")
		{
			line(source, "\treader->readVector3(m" + name + ");");
		}
		else if (type == "Vector3Int")
		{
			line(source, "\treader->readVector3Int(m" + name + ");");
		}
		else if (startWith(type, "Vector<"))
		{
			const string elementType = type.substr(strlen("Vector<"), type.length() - strlen("Vector<") - 1);
			if (elementType == "string")
			{
				line(source, "\treader->readStringList(m" + name + ");");
			}
			else if (elementType == "Vector2")
			{
				line(source, "\treader->readVector2List(m" + name + ");");
			}
			else if (elementType == "Vector2Int")
			{
				line(source, "\treader->readVector2IntList(m" + name + ");");
			}
			else if (elementType == "Vector3")
			{
				line(source, "\treader->readVector3List(m" + name + ");");
			}
			else if (elementType == "Vector3Int")
			{
				line(source, "\treader->readVector3IntList(m" + name + ");");
			}
			else
			{
				line(source, "\treader->readList(m" + name + ");");
			}
		}
		else
		{
			line(source, "\treader->read(m" + name + ");");
		}
	}
	line(source, "}");
	line(source, "// auto generate end", false);
	writeFile(dataFilePath + dataClassName + ".cpp", source);
}

// ExcelTable.h和ExcelTable.cpp文件
void CodeSQLite::generateCppSQLiteTableFile(const SQLiteInfo& sqliteInfo, const string& tableFilePath)
{
	// ExcelTable.h
	string dataClassName = "ED" + sqliteInfo.mSQLiteName;
	string tableClassName = "Excel" + sqliteInfo.mSQLiteName;
	string tableHeaderFile = tableFilePath + tableClassName + ".h";
	if (!isFileExist(tableHeaderFile))
	{
		string table;
		line(table, "#pragma once");
		line(table, "");
		line(table, "#include \"" + dataClassName + ".h\"");
		line(table, "#include \"ExcelTable.h\"");
		line(table, "");
		line(table, "class " + tableClassName + " : public ExcelTable<" + dataClassName + ">");
		line(table, "{");
		line(table, "public:");
		line(table, "\t// auto generate start");
		line(table, "\tvoid checkAllDataDefault() override;");
		line(table, "\t// auto generate end");
		line(table, "};", false);

		writeFile(tableHeaderFile, table);
	}
	else
	{
		myVector<string> codeList;
		int lineStart = -1;
		if (!findCustomCode(tableHeaderFile, codeList, lineStart,
			[](const string& codeLine) { return endWith(codeLine, "// auto generate start"); },
			[](const string& codeLine) { return endWith(codeLine, "// auto generate end"); }, false))
		{
			// 如果找不到就在第一个public下一行插入
			FOR_VECTOR(codeList)
			{
				if (endWith(codeList[i], "public:"))
				{
					codeList.insert(++i, "\t// auto generate start");
					lineStart = i;
					codeList.insert(++i, "\t// auto generate end");
					break;
				}
			}
		}
		codeList.insert(++lineStart, "\tvoid checkAllDataDefault() override;");
		writeFile(tableHeaderFile, codeList);
	}

	// ExcelTable.cpp
	myVector<string> insertLines;
	insertLines.push_back("void " + tableClassName + "::checkAllDataDefault()");
	insertLines.push_back("{");
	insertLines.push_back("\tfor (const auto& item : getAllData())");
	insertLines.push_back("\t{");
	insertLines.push_back("\t\t" + dataClassName + "* data = item.second;");
	bool hasCheck = false;
	for (const SQLiteMember& member : sqliteInfo.mMemberList)
	{
		if (member.mOwner == OWNER::BOTH || member.mOwner == OWNER::SERVER_ONLY)
		{
			const string& name = member.mName;
			const string& linkTable = member.mLinkTable;
			if (!linkTable.empty())
			{
				if (!isFileExist(ExcelPath + linkTable + ".csv") && !isFileExist(ExcelPath + linkTable + ".db"))
				{
					ERROR("找不到服务器索引的表格:" + linkTable + ", 当前表格:" + sqliteInfo.mSQLiteName + ", 字段名:" + name);
					continue;
				}
			}
			if (!linkTable.empty())
			{
				if (member.mEnumRealType.empty())
				{
					insertLines.push_back("\t\tmExcel" + linkTable + "->checkData(data->m" + name + ", item.first, this);");
				}
				else
				{
					insertLines.push_back("\t\tmExcel" + linkTable + "->checkData((int)data->m" + name + ", item.first, this);");
				}
				hasCheck = true;
			}
			if (!member.mEnumRealType.empty())
			{
				insertLines.push_back("\t\tcheckEnumResult(GameEnumCheck::checkEnum(data->m" + name + "), \"m" + name + "\", item.first);");
				hasCheck = true;
			}
		}
	}
	insertLines.push_back("\t}");
	insertLines.push_back("}");
	// 如果没有任何需要检查的,就只插入一个空函数
	if (!hasCheck)
	{
		insertLines.clear();
		insertLines.push_back("void " + tableClassName + "::checkAllDataDefault() {}");
	}
	string tableSourceFile = tableFilePath + tableClassName + ".cpp";
	if (!isFileExist(tableSourceFile))
	{
		string table;
		line(table, "#include \"GameHeader.h\"");
		line(table, "");
		line(table, "// auto generate start");
		for (const string& str : insertLines)
		{
			line(table, str);
		}
		line(table, "// auto generate end", false);
		writeFile(tableSourceFile, table);
	}
	else
	{
		myVector<string> codeList;
		int lineStart = -1;
		if (!findCustomCode(tableSourceFile, codeList, lineStart,
			[](const string& codeLine) { return endWith(codeLine, "// auto generate start"); },
			[](const string& codeLine) { return endWith(codeLine, "// auto generate end"); }, false))
		{
			// 如果找不到就在最后一行插入
			int lineIndex = codeList.size() - 1;
			codeList.insert(++lineIndex, "");
			codeList.insert(++lineIndex, "// auto generate start");
			codeList.insert(++lineIndex, "// auto generate end");
			lineStart = lineIndex - 1;
		}
		for (const string& str : insertLines)
		{
			codeList.insert(++lineStart, str);
		}
		writeFile(tableSourceFile, codeList);
	}
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

// SQLiteData.cs文件
void CodeSQLite::generateCSharpSQLiteDataFile(const SQLiteInfo& sqliteInfo, const string& dataFileHotFixPath)
{
	if (!sqliteInfo.mClientSQLite || sqliteInfo.mOwner == OWNER::SERVER_ONLY || sqliteInfo.mOwner == OWNER::NONE)
	{
		return;
	}
	string file;
	string dataClassName = "SD" + sqliteInfo.mSQLiteName;
	line(file, "// auto generate start");
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
		const string& name = member.mName;
		if (name == "ID")
		{
			continue;
		}
		line(file, "\tpublic const string " + name + " = " + "\"" + name + "\";");
	}
	for (const SQLiteMember& member : sqliteInfo.mMemberList)
	{
		const string& name = member.mName;
		if (name == "ID")
		{
			continue;
		}
		// 因为模板文件是按照C++来写的,但是有些类型在C#中是没有的,所以要转换为C#中对应的类型
		string typeName = cppTypeToCSharpType(member.mType);
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
		if (member.mOwner == OWNER::CLIENT_ONLY || member.mOwner == OWNER::BOTH)
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
			listMemberList.push_back(make_pair(typeName, name));
		}

		string memberLine;
		if (!isList)
		{
			memberLine = "\t" + publicType + " " + typeName + " m" + name + ";";
		}
		else
		{
			memberLine = "\t" + publicType + " " + typeName + " m" + name + " = new();";
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
		if (member.mName == "ID")
		{
			continue;
		}
		line(file, "\t\tparseParam(reader, ref m" + member.mName + ", " + IToS(i) + ");");
	}
	line(file, "\t}");
	line(file, "}");
	line(file, "// auto generate end", false);
	writeFile(dataFileHotFixPath + dataClassName + ".cs", file);
}

// SQLiteTable.cs文件
void CodeSQLite::generateCSharpSQLiteTableFile(const SQLiteInfo& sqliteInfo, const string& tableFileHotFixPath)
{
	if (!sqliteInfo.mClientSQLite || sqliteInfo.mOwner == OWNER::SERVER_ONLY || sqliteInfo.mOwner == OWNER::NONE)
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
	writeFile(fullPath, table);
}

// SQLiteRegister.cs文件
void CodeSQLite::generateCSharpSQLiteRegisteFileFile(const myVector<SQLiteInfo>& sqliteInfo, const string& fileHotFixPath)
{
	string hotFixfile;
	line(hotFixfile, "// auto generate start");
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
		if (info.mClientSQLite && info.mOwner != OWNER::SERVER_ONLY && info.mOwner != OWNER::NONE)
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
	line(hotFixfile, "}");
	line(hotFixfile, "// auto generate end", false);
	writeFile(fileHotFixPath + "SQLiteRegister.cs", hotFixfile);
}