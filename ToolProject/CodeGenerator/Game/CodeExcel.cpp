#include "CodeExcel.h"
#include "CodeSQLite.h"

void CodeExcel::generate()
{
	print("正在生成Excel");

	// 先读取表格描述
	myVector<CSVInfo> infoList;
	myVector<string> csvFiles;
	findFiles(ExcelPath, csvFiles, ".csv");
	FOR_VECTOR(csvFiles)
	{
		infoList.push_back(CSVInfo());
		parseCSV(csvFiles[i], infoList[i].mHeader, infoList[i].mDataList);
	}
	
	// cpp
	string cppGameDataPath = cppGamePath + "DataBase/Excel/Data/";
	string cppGameTablePath = cppGamePath + "DataBase/Excel/Table/";
	myVector<string> serverTableNameList;
	myVector<CSVInfo> serverInfoList;
	for (const CSVInfo& info : infoList)
	{
		if ((info.mHeader.mOwner == OWNER::BOTH || info.mHeader.mOwner == OWNER::SERVER_ONLY))
		{
			serverTableNameList.push_back(info.mHeader.mTableName);
			serverInfoList.push_back(info);
		}
	}
	// 删除C++的代码文件,只删Data的代码,Table的代码由于包含手动写的代码,而且Excel和SQLite混在一起,所以不删除
	// 删除Data时也需要注意不要把已经从SQLite文件生成好的Table代码删了
	string patterns[2]{ ".cpp", ".h" };
	for (const string& str : findFiles(cppGameDataPath, patterns, 2))
	{
		if (CodeSQLite::mSQLiteForServerTableList.contains(getFileNameNoSuffix(str, true).substr(strlen("ED"))))
		{
			continue;
		}
		deleteFile(str);
	}

	// 生成代码文件
	for (const CSVInfo& info : serverInfoList)
	{
		generateCppExcelDataFile(info, cppGameDataPath);
		generateCppExcelTableFile(info, cppGameTablePath);
	}

	const string gameBaseHeaderPath = cppGamePath + "Common/GameBase.h";
	const string gameBaseSourcePath = cppGamePath + "Common/GameBase.cpp";
	const string gameSTLPoolSourcePath = cppGamePath + "Common/GameSTLPoolRegister.cpp";
	// 服务器中Excel和SQLite的表格名字列表
	myVector<string> allTableNameList;
	allTableNameList.addRange(serverTableNameList);
	allTableNameList.addRange(CodeSQLite::mSQLiteForServerTableList);
	generateCppExcelRegisteFile(allTableNameList, getFilePath(cppGameDataPath) + "/");
	generateCppExcelInstanceDeclare(allTableNameList, gameBaseHeaderPath, "");
	generateCppExcelInstanceDefine(allTableNameList, gameBaseSourcePath);
	generateCppExcelSTLPoolRegister(allTableNameList, gameSTLPoolSourcePath);
	generateCppExcelInstanceClear(allTableNameList, gameBaseSourcePath);
	for (const CSVInfo& info : serverInfoList)
	{
		if (info.mHeader.mTableName == "Global")
		{
			generateCppGlobalConfig(info, cppGameTablePath);
		}
		else if (info.mHeader.mTableName == "Buff")
		{
			generateCppBuff(info);
		}
	}

	//------------------------------------------------------------------------------------------------------------------------------
	// csharp
	if (!ClientHotFixPath.empty())
	{
		string csExcelDataPath = ClientHotFixPath + "DataBase/Excel/Data/";
		string csExcelTablePath = ClientHotFixPath + "DataBase/Excel/Table/";
		string csExcelTableDeclareHotFixPath = ClientHotFixPath + "Common/";

		// 筛选出Client的表格
		myVector<CSVInfo> clientExcelList;
		myVector<string> tableNameList;
		for (const CSVInfo& info : infoList)
		{
			if (info.mHeader.mOwner == OWNER::BOTH || info.mHeader.mOwner == OWNER::CLIENT_ONLY)
			{
				clientExcelList.push_back(info);
				tableNameList.push_back(info.mHeader.mTableName);
			}
		}
		// 删除C#的代码文件,c#的只删除代码文件,不删除meta文件
		for (const string& str : findFiles(csExcelDataPath, ".cs"))
		{
			deleteFile(str);
		}
		for (const string& str : findFiles(csExcelTablePath, ".cs"))
		{
			// 只删除已经不存在的表格
			if (!tableNameList.contains(removeStartString(getFileNameNoSuffix(str, true), "Excel")))
			{
				deleteFile(str);
			}
		}

		// 生成代码文件
		for (const CSVInfo& info : clientExcelList)
		{
			generateCSharpExcelDataFile(info, csExcelDataPath);
			generateCSharpExcelTableFile(info, csExcelTablePath);
		}

		// 在上一层目录生成ExcelRegister.cs文件
		generateCSharpExcelRegisteFileFile(clientExcelList, getFilePath(csExcelDataPath) + "/");
		generateCSharpExcelDeclare(clientExcelList, csExcelTableDeclareHotFixPath);

		for (const CSVInfo& info : clientExcelList)
		{
			if (info.mHeader.mTableName == "Global")
			{
				generateCSharpGlobalConfig(info, csExcelTablePath);
			}
			else if (info.mHeader.mTableName == "Buff")
			{
				generateCSharpBuff(info);
			}
		}
	}
	print("完成生成Excel");
	print("");
}

// ExcelData.h和ExcelData.cpp文件
void CodeExcel::generateCppExcelDataFile(const CSVInfo& info, const string& dataFilePath)
{
	// 不含ID的成员字段列表
	myVector<ColumnData*> memberNoIDList;
	for (ColumnData* member : info.mHeader.mColumnDataList)
	{
		if (member->mName == "ID")
		{
			continue;
		}
		memberNoIDList.push_back(member);
	}
	// 不含ID以及非服务器字段的成员字段列表
	myVector<ColumnData*> memberUsedInServerNoIDList;
	for (ColumnData* member : memberNoIDList)
	{
		if (member->mOwner != OWNER::SERVER_ONLY && member->mOwner != OWNER::BOTH)
		{
			continue;
		}
		memberUsedInServerNoIDList.push_back(member);
	}

	// first是变量名,second是注释,用于通过变量来访问ID
	myMap<int, pair<string, string>> variableList;
	myMap<string, int> colNameList;
	int variableNameIndex = -1;
	int variableCommentIndex = -1;
	FOR_VECTOR(info.mHeader.mColumnDataList)
	{
		const string& colName = info.mHeader.mColumnDataList[i]->mName;
		colNameList.insert(colName, i);
		if (colName == "VariableName")
		{
			variableNameIndex = i;
		}
		else if (colName == "VariableComment")
		{
			variableCommentIndex = i;
		}
	}

	if (variableNameIndex >= 0 && variableCommentIndex >= 0)
	{
		FOR_VECTOR(info.mDataList)
		{
			const auto& rowData = info.mDataList[i];
			const string& variableName = rowData[variableNameIndex];
			const string& variableComment = rowData[variableCommentIndex];
			if (variableName.length() > 0)
			{
				variableList.insert(SToI(rowData[0]), make_pair(variableName, variableComment));
			}
		}
	}

	int customParamCount = 0;
	FOR_I(20)
	{
		if (!colNameList.contains("Param" + IToS(i)) ||
			!colNameList.contains("ParamType" + IToS(i)) ||
			!colNameList.contains("ParamName" + IToS(i)) ||
			!colNameList.contains("ParamComment" + IToS(i)))
		{
			customParamCount = i;
			break;
		}
	}

	// ExcelData.h
	string header;
	string dataClassName = "ED" + info.mHeader.mTableName;
	line(header, "// auto generate start");
	line(header, "#pragma once");
	line(header, "");
	line(header, "#include \"ExcelData.h\"");
	line(header, "#include \"GameEnum.h\"");
	line(header, "");
	line(header, "// " + info.mHeader.mComment);
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
	for (const ColumnData* member : memberUsedInServerNoIDList)
	{
		const string& type = member->mType;
		const string& name = member->mName;
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
		else if (!member->mEnumRealType.empty())
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
		appendWithAlign(memberLine, "// " + member->mComment, 60);
		line(header, memberLine);
	}
	line(header, "public:");
	line(header, "\tvoid cloneTo(ExcelData* target) override;");
	line(header, "\tvoid read(SerializerRead* reader) override;");
	line(header, "};");

	// 要生成参数的代码,必须要有对应ID的常量名
	if (customParamCount > 0 && variableNameIndex > 0)
	{
		bool hasCustomParam = false;
		mySet<int> paramIDList;
		for (const auto& row : info.mDataList)
		{
			if (!row[colNameList["Param0"]].empty())
			{
				hasCustomParam = true;
				break;
			}
		}
		if (hasCustomParam)
		{
			for (const auto& row : info.mDataList)
			{
				if (row[colNameList["Param0"]].empty())
				{
					continue;
				}
				const string paramClassName = "ED" + info.mHeader.mTableName + "_" + row[variableNameIndex];
				line(header, "");
				line(header, "class " + paramClassName);
				line(header, "{");
				line(header, "public:");
				// 变量定义
				FOR_I(customParamCount)
				{
					string indexSuffix = IToS(i);
					const string& paramValue = row[colNameList["Param" + indexSuffix]];
					const string& paramType = row[colNameList["ParamType" + indexSuffix]];
					const string& paramName = row[colNameList["ParamName" + indexSuffix]];
					if (paramValue.empty())
					{
						break;
					}
					if (paramType == "Vector<int>")
					{
						myVector<int> values;
						SToIs(paramValue, values);
						line(header, "\tstatic constexpr Array<" + IToS(values.size()) + ", int> m" + paramName + " { " + IsToS(values.data(), values.size(), 0, ", ") + " };");
					}
					else if (paramType == "Vector<llong>")
					{
						myVector<llong> values;
						SToLLs(paramValue, values);
						line(header, "\tstatic constexpr Array<" + IToS(values.size()) + ", llong> m" + paramName + " { " + LLsToS(values.data(), values.size(), 0, ", ") + " };");
					}
					else if (paramType == "Vector<float>")
					{
						myVector<float> values;
						SToFs(paramValue, values);
						string valueStr;
						FOR_VECTOR(values)
						{
							string str = FToS(values[i]);
							if ((int)str.find_first_of('.') < 0)
							{
								str += ".0f";
							}
							else
							{
								str += "f";
							}
							valueStr += str;
							if (i != values.size())
							{
								valueStr += ", ";
							}
						}
						line(header, "\tstatic constexpr Array<" + IToS(values.size()) + ", float> m" + paramName + " { " + valueStr + " };");
					}
					else if (paramType == "int" || paramType == "llong")
					{
						line(header, "\tstatic constexpr " + paramType + " m" + paramName + " = " + paramValue + ";");
					}
					else if (paramType == "float")
					{
						string str = paramValue;
						if ((int)str.find_first_of('.') < 0)
						{
							str += ".0f";
						}
						else
						{
							str += "f";
						}
						line(header, "\tstatic constexpr " + paramType + " m" + paramName + " = " + str + ";");
					}
					else
					{
						ERROR("不支持的参数类型:" + paramType + ", 表格:" + info.mHeader.mTableName + ", id:" + row[0]);
					}
				}
				line(header, "};");
			}
		}
	}
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
		for (const ColumnData* member : memberUsedInServerNoIDList)
		{
			const string& name = member->mName;
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
	for (const ColumnData* member : memberUsedInServerNoIDList)
	{
		const string& type = member->mType;
		const string& name = member->mName;
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
void CodeExcel::generateCppExcelTableFile(const CSVInfo& info, const string& tableFilePath)
{
	// ExcelTable.h
	string dataClassName = "ED" + info.mHeader.mTableName;
	string tableClassName = "Excel" + info.mHeader.mTableName;
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
	myMap<string, myVector<int>> linkLengthMap;
	for (const ColumnData* member : info.mHeader.mColumnDataList)
	{
		if (member->mOwner == OWNER::BOTH || member->mOwner == OWNER::SERVER_ONLY)
		{
			const string& linkLength = member->mLinkLength;
			if (!linkLength.empty())
			{
				if (!linkLengthMap.contains(linkLength))
				{
					myVector<int> tmep{ member->mIndex };
					linkLengthMap.insert(linkLength, tmep);
				}
				else
				{
					linkLengthMap[linkLength].push_back(member->mIndex);
				}
			}
			const string& name = member->mName;
			const string& linkTable = member->mLinkTable;
			if (!linkTable.empty())
			{
				if (!isFileExist(ExcelPath + linkTable + ".csv") && !isFileExist(ExcelPath + linkTable + ".db"))
				{
					ERROR("找不到服务器索引的表格:" + linkTable + ", 当前表格:" + info.mHeader.mTableName + ", 字段名:" + name);
					continue;
				}
				if (member->mEnumRealType.empty())
				{
					insertLines.push_back("\t\tmExcel" + linkTable + "->checkData(data->m" + name + ", item.first, this);");
				}
				else
				{
					insertLines.push_back("\t\tmExcel" + linkTable + "->checkData((int)data->m" + name + ", item.first, this);");
				}
				hasCheck = true;
			}
			if (!member->mEnumRealType.empty())
			{
				insertLines.push_back("\t\tcheckEnumResult(GameEnumCheck::checkEnum(data->m" + name + "), \"m" + name + "\", item.first);");
				hasCheck = true;
			}
		}
	}
	FOREACH(iterLink, linkLengthMap)
	{
		const auto& colList = iterLink->second;
		if (colList.size() > 1)
		{
			FOR_I(colList.size() - 1)
			{
				const string& name0 = info.mHeader.mColumnDataList[colList[i]]->mName;
				const string& name1 = info.mHeader.mColumnDataList[colList[i + 1]]->mName;
				insertLines.push_back("\t\tcheckListPair(item.second->m" + name0 + ", item.second->m" + name1 + ", item.first);");
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

// ExcelRegister.h和ExcelRegister.cpp文件
void CodeExcel::generateCppExcelRegisteFile(const myVector<string>& infoList, const string& filePath)
{
	// ExcelRegister.h
	string str0;
	line(str0, "// auto generate start");
	line(str0, "#pragma once");
	line(str0, "");
	line(str0, "#include \"GameBase.h\"");
	line(str0, "");
	line(str0, "class ExcelRegister");
	line(str0, "{");
	line(str0, "public:");
	line(str0, "\tstatic void registeAll();");
	line(str0, "};");
	line(str0, "// auto generate end", false);
	writeFile(filePath + "ExcelRegister.h", str0);

	string str1;
	line(str1, "// auto generate start");
	line(str1, "#include \"GameHeader.h\"");
	line(str1, "");
	line(str1, "void ExcelRegister::registeAll()");
	line(str1, "{");
	FOR_VECTOR(infoList)
	{
		const string& tableName = infoList[i];
		line(str1, "\tmExcel" + tableName + " = mExcelManager->registeExcel<Excel" + tableName + ">(\"" + tableName + "\");");
	}
	line(str1, "}");
	line(str1, "// auto generate end", false);
	writeFile(filePath + "ExcelRegister.cpp", str1);
}

string CodeExcel::paramNameToFunctionName(const string& paramName)
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

// 生成ExcelGlobal对应的C++代码
void CodeExcel::generateCppGlobalConfig(const CSVInfo& globalConfig, const string& tableFilePath)
{
	// ExcelGlobal.h
	string dataClassName = "ED" + globalConfig.mHeader.mTableName;
	string tableClassName = "Excel" + globalConfig.mHeader.mTableName;
	string tableFileName = tableFilePath + tableClassName + ".h";
	string tableString;
	line(tableString, "// auto generate start");
	line(tableString, "#pragma once");
	line(tableString, "");
	line(tableString, "#include \"" + dataClassName + ".h\"");
	line(tableString, "#include \"ExcelTable.h\"");
	line(tableString, "");
	line(tableString, "class " + tableClassName + " : public ExcelTable<" + dataClassName + ">");
	line(tableString, "{");
	line(tableString, "\tBASE(" + tableClassName + ", ExcelTable<" + dataClassName + ">);");
	line(tableString, "public:");
	line(tableString, "\tvoid init(const string& tableName) override;");
	line(tableString, "\tvoid checkAllDataDefault() override;");
	line(tableString, "public:");
	int paramTypeIndex = -1;
	int paramNameIndex = -1;
	int paramValueIndex = -1;
	int paramDescIndex = -1;
	FOR_VECTOR(globalConfig.mHeader.mColumnDataList)
	{
		const string& colName = globalConfig.mHeader.mColumnDataList[i]->mName;
		if (colName == "ParamType")
		{
			paramTypeIndex = i;
		}
		else if (colName == "ParamName")
		{
			paramNameIndex = i;
		}
		else if (colName == "ParamValue")
		{
			paramValueIndex = i;
		}
		else if (colName == "ParamDesc")
		{
			paramDescIndex = i;
		}
	}
	FOR_VECTOR(globalConfig.mDataList)
	{
		const auto& row = globalConfig.mDataList[i];
		const string& paramType = row[paramTypeIndex];
		const string& paramName = row[paramNameIndex];
		const string& paramValue = row[paramValueIndex];
		const string& paramDesc = row[paramDescIndex];
		if (paramType == "float")
		{
			string floatStr = paramValue;
			if ((int)floatStr.find_first_of('.') < 0)
			{
				floatStr += ".0";
			}
			string temp = "\tstatic constexpr " + paramType + " " + paramName + " = " + floatStr + "f;";
			appendWithAlign(temp, "// " + paramDesc, 76);
			line(tableString, temp);
		}
		else if (paramType == "int" || paramType == "llong")
		{
			string temp = "\tstatic constexpr " + paramType + " " + paramName + " = " + paramValue + ";";
			appendWithAlign(temp, "// " + paramDesc, 76);
			line(tableString, temp);
		}
		else
		{
			string temp = "\tstatic " + paramType + " " + paramName + ";";
			appendWithAlign(temp, "// " + paramDesc, 76);
			line(tableString, temp);
		}
	}
	line(tableString, "};");
	line(tableString, "// auto generate end", false);
	writeFile(tableFileName, tableString);

	// ExcelGlobal.cpp
	string cppFileName = tableFilePath + tableClassName + ".cpp";
	myVector<string> codeList;
	codeList.push_back("// auto generate start");
	codeList.push_back("#include \"GameHeader.h\"");
	codeList.push_back("");
	FOR_VECTOR(globalConfig.mDataList)
	{
		const auto& row = globalConfig.mDataList[i];
		const string& paramType = row[paramTypeIndex];
		const string& paramName = row[paramNameIndex];
		const string& paramValue = row[paramValueIndex];
		const string& paramDesc = row[paramDescIndex];
		if (paramType != "float" && paramType != "int" && paramType != "llong")
		{
			codeList.push_back(paramType + " ExcelGlobal::" + paramName + ";");
		}
	}
	codeList.push_back("");
	codeList.push_back("void ExcelGlobal::init(const string& tableName)");
	codeList.push_back("{");
	codeList.push_back("\tbase::init(tableName);");
	codeList.push_back("\tMap<string, string> paramMap;");
	codeList.push_back("\tfor (const auto& item : getAllData())");
	codeList.push_back("\t{");
	codeList.push_back("\t\tremoveAll(item.second->mParamValue, ' ');");
	codeList.push_back("\t\tparamMap.add(item.second->mParamName, item.second->mParamValue);");
	codeList.push_back("\t}");
	FOR_VECTOR(globalConfig.mDataList)
	{
		const auto& row = globalConfig.mDataList[i];
		const string& paramType = row[paramTypeIndex];
		const string& paramName = row[paramNameIndex];
		const string& paramValue = row[paramValueIndex];
		const string& paramDesc = row[paramDescIndex];
		if (paramType == "Vector2Int")
		{
			codeList.push_back("\t" + paramName + " = SToV2I(paramMap[STR(" + paramName + ")]);");
		}
		else if (paramType == "Vector2")
		{
			codeList.push_back("\t" + paramName + " = SToV2(paramMap[STR(" + paramName + ")]);");
		}
		else if (paramType == "Vector3")
		{
			codeList.push_back("\t" + paramName + " = SToV3(paramMap[STR(" + paramName + ")]);");
		}
		else if (paramType == "Vector3Int")
		{
			codeList.push_back("\t" + paramName + " = SToV3I(paramMap[STR(" + paramName + ")]);");
		}
		else if (paramType == "Vector<int>")
		{
			codeList.push_back("\tSToIs(paramMap[STR(" + paramName + ")], " + paramName + ");");
		}
		else if (paramType == "Vector<float>")
		{
			codeList.push_back("\tSToFs(paramMap[STR(" + paramName + ")], " + paramName + ");");
		}
	}
	codeList.push_back("}");
	codeList.push_back("");
	codeList.push_back("void ExcelGlobal::checkAllDataDefault() {}");
	codeList.push_back("// auto generate end", false);
	writeFile(cppFileName, codeList);
}

// 根据BuffDetail表格生成对应的C++代码,包括类定义和类注册
void CodeExcel::generateCppBuff(const CSVInfo& config)
{
	int variableNameIndex = -1;
	FOR_VECTOR(config.mHeader.mColumnDataList)
	{
		const string& name = config.mHeader.mColumnDataList[i]->mName;
		if (name == "VariableName")
		{
			variableNameIndex = i;
			break;
		}
	}

	myVector<string> buffClassNameList;
	if (variableNameIndex >= 0)
	{
		for (const auto& item : config.mDataList)
		{
			buffClassNameList.push_back(item[variableNameIndex]);
		}
	}
	const string registerFilePath = cppGamePath + "Character/Component/StateMachine/StateRegister.cpp";
	// 更新特定部分代码
	myVector<string> codeList;
	int lineStart = -1;
	if (!findCustomCode(registerFilePath, codeList, lineStart,
		[](const string& codeLine) { return endWith(codeLine, "// auto generate start"); },
		[](const string& codeLine) { return endWith(codeLine, "// auto generate end"); }))
	{
		return;
	}

	for (const string& info : buffClassNameList)
	{
		codeList.insert(++lineStart, "\tSTATE_FACTORY(" + info + ");");
	}
	writeFile(registerFilePath, codeList);
}

void CodeExcel::generateCppExcelInstanceDeclare(const myVector<string>& infoList, const string& gameBaseHeaderFileName, const string& exprtMacro)
{
	// 更新特定部分代码
	myVector<string> codeList;
	int lineStart = -1;
	if (!findCustomCode(gameBaseHeaderFileName, codeList, lineStart,
		[](const string& codeLine) { return endWith(codeLine, "// auto generate start Excel Extern"); },
		[](const string& codeLine) { return endWith(codeLine, "// auto generate end Excel Extern"); }))
	{
		// 如果找不到,就添加到文件的最后
		auto tempCodeList = openFile(gameBaseHeaderFileName);
		FOR_INVERSE_I(tempCodeList.size())
		{
			if (tempCodeList[i] == "};" || tempCodeList[i] == "}")
			{
				lineStart = i - 1;
				break;
			}
		}
		if (lineStart >= 0)
		{
			codeList.insert(++lineStart, "");
			codeList.insert(++lineStart, "\t// auto generate start Excel Extern");
			codeList.insert(lineStart + 1, "\t// auto generate end Excel Extern");
		}
	}

	for (const string& info : infoList)
	{
		codeList.insert(++lineStart, "\t" + exprtMacro + "extern Excel" + info + "* mExcel" + info + ";");
	}
	writeFile(gameBaseHeaderFileName, codeList);
}

void CodeExcel::generateCppExcelInstanceDefine(const myVector<string>& infoList, const string& gameBaseCppFileName)
{
	// 更新GameBase.cpp的特定部分代码
	myVector<string> codeList;
	int lineStart = -1;
	if (!findCustomCode(gameBaseCppFileName, codeList, lineStart,
		[](const string& codeLine) { return endWith(codeLine, "// auto generate start Excel Define"); },
		[](const string& codeLine) { return endWith(codeLine, "// auto generate end Excel Define"); }))
	{
		// 如果找不到,就添加到文件的最开头
		auto tempCodeList = openFile(gameBaseCppFileName);
		FOR_I(tempCodeList.size())
		{
			if (tempCodeList[i] == "{")
			{
				lineStart = i;
				break;
			}
		}
		if (lineStart >= 0)
		{
			codeList.insert(++lineStart, "\t// auto generate start Excel Define");
			codeList.insert(lineStart + 1, "\t// auto generate end Excel Define");
		}
	}
	for (const string& info : infoList)
	{
		codeList.insert(++lineStart, "\tExcel" + info + "* mExcel" + info + ";");
	}
	writeFile(gameBaseCppFileName, codeList);
}

// GameSTLPoolRegister.cpp
void CodeExcel::generateCppExcelSTLPoolRegister(const myVector<string>& infoList, const string& gameSTLPoolFile)
{
	// 更新GameBase.h的特定部分代码
	myVector<string> codeList;
	int lineStart = -1;
	if (!findCustomCode(gameSTLPoolFile, codeList, lineStart,
		[](const string& codeLine) { return endWith(codeLine, "// auto generate start Excel数据类型"); },
		[](const string& codeLine) { return endWith(codeLine, "// auto generate end Excel数据类型"); }))
	{
		return;
	}
	for (const string& info : infoList)
	{
		codeList.insert(++lineStart, "\tmVectorPoolManager->registeVectorPool<ED" + info + "*>();");
	}
	writeFile(gameSTLPoolFile, codeList);
}

void CodeExcel::generateCppExcelInstanceClear(const myVector<string>& infoList, const string& gameBaseCppFileName)
{
	// 更新GameBase.cpp的特定部分
	myVector<string> codeList;
	int lineStart = -1;
	if (!findCustomCode(gameBaseCppFileName, codeList, lineStart,
		[](const string& codeLine) { return endWith(codeLine, "// auto generate start Excel Clear"); },
		[](const string& codeLine) { return endWith(codeLine, "// auto generate end Excel Clear"); }))
	{
		return;
	}

	for (const string& info : infoList)
	{
		codeList.insert(++lineStart, "\t\tmExcel" + info + " = nullptr;");
	}
	writeFile(gameBaseCppFileName, codeList);
}

// ExcelData.cs文件
void CodeExcel::generateCSharpExcelDataFile(const CSVInfo& info, const string& dataFileHotFixPath)
{
	if (info.mHeader.mOwner == OWNER::SERVER_ONLY || info.mHeader.mOwner == OWNER::NONE)
	{
		return;
	}

	myMap<string, int> colNameList;
	int variableNameIndex = -1;
	int variableCommentIndex = -1;
	FOR_VECTOR(info.mHeader.mColumnDataList)
	{
		const string& name = info.mHeader.mColumnDataList[i]->mName;
		colNameList.insert(name, i);
		if (name == "VariableName")
		{
			variableNameIndex = i;
		}
		else if (name == "VariableComment")
		{
			variableCommentIndex = i;
		}
	}

	int customParamCount = 0;
	FOR_I(20)
	{
		if (!colNameList.contains("Param" + IToS(i)) ||
			!colNameList.contains("ParamType" + IToS(i)) ||
			!colNameList.contains("ParamName" + IToS(i)) ||
			!colNameList.contains("ParamComment" + IToS(i)))
		{
			customParamCount = i;
			break;
		}
	}

	// first是变量名,second是注释,用于通过变量来访问ID
	myMap<int, pair<string, string>> variableList;
	if (variableNameIndex >= 0)
	{
		for (const auto& row : info.mDataList)
		{
			if (!row[variableNameIndex].empty())
			{
				variableList.insert(StringUtility::SToI(row[0]), make_pair(row[variableNameIndex], row[variableCommentIndex]));
			}
		}
	}

	string file;
	string dataClassName = "ED" + info.mHeader.mTableName;
	line(file, "// auto generate start");
	line(file, "using System;");
	line(file, "using System.Collections.Generic;");
	line(file, "using UnityEngine;");
	line(file, "");
	line(file, "// " + info.mHeader.mComment);
	line(file, "public class " + dataClassName + " : ExcelData");
	line(file, "{");
	// 表示ID的静态变量
	if (variableList.size() > 0)
	{
		for (const auto& item : variableList)
		{
			string str = "\tpublic static int " + item.second.first + " = " + IToS(item.first) + ";";
			appendWithAlign(str, "// " + item.second.second, 52);
			line(file, str);
		}
		line(file, "");
	}
	uint memberCount = info.mHeader.mColumnDataList.size();
	mySet<string> listMemberSet;
	myVector<pair<string, string>> listMemberList;
	FOR_I(memberCount)
	{
		const ColumnData* member = info.mHeader.mColumnDataList[i];
		const string& name = member->mName;
		if (name == "ID")
		{
			continue;
		}

		// 不在客户端使用的则不定义成员变量
		if (member->mOwner != OWNER::CLIENT_ONLY && member->mOwner != OWNER::BOTH)
		{
			continue;
		}
		string typeName = convertToCSharpType(member->mType);
		// 列表类型的成员变量存储到单独的列表,因为需要分配内存
		bool isList = findString(typeName.c_str(), "List");
		if (isList)
		{
			listMemberList.push_back(make_pair(typeName, name));
			listMemberSet.insert(name);
		}
		string memberLine;
		if (!isList)
		{
			memberLine = "\tpublic " + typeName + " m" + name + ";";
		}
		else
		{
			memberLine = "\tpublic " + typeName + " m" + name + " = new();";
		}
		appendWithAlign(memberLine, "// " + info.mHeader.mColumnDataList[i]->mComment, 52);
		line(file, memberLine);
	}
	line(file, "\tpublic override bool read(SerializerRead reader)");
	line(file, "\t{");
	if (memberCount == 0)
	{
		line(file, "\t\treturn base.read(reader);");
	}
	else
	{
		line(file, "\t\tbool result = base.read(reader);");
		FOR_I(memberCount)
		{
			const ColumnData* memberInfo = info.mHeader.mColumnDataList[i];
			const string& name = memberInfo->mName;
			const string& type = memberInfo->mType;
			const string& enumRealType = memberInfo->mEnumRealType;
			if (name == "ID")
			{
				continue;
			}
			// 不在客户端使用的则不读取
			if (memberInfo->mOwner != OWNER::CLIENT_ONLY && memberInfo->mOwner != OWNER::BOTH)
			{
				continue;
			}
			const string typeName = convertToCSharpType(type);
			if (typeName == "string")
			{
				line(file, "\t\tresult = result && reader.readString(out m" + name + ");");
			}
			else if (listMemberSet.contains(name))
			{
				if (enumRealType == "byte")
				{
					line(file, "\t\tresult = result && reader.readEnumByteList(m" + name + ");");
				}
				else
				{
					line(file, "\t\tresult = result && reader.readList(m" + name + ");");
				}
			}
			else if (enumRealType == "byte")
			{
				line(file, "\t\tresult = result && reader.readEnumByte(out m" + name + ");");
			}
			else
			{
				line(file, "\t\tresult = result && reader.read(out m" + name + ");");
			}
		}
		line(file, "\t\treturn result;");
	}
	line(file, "\t}");
	line(file, "}");

	// 要生成参数的代码,必须要有对应ID的常量名
	if (customParamCount > 0 && variableNameIndex > 0)
	{
		bool hasCustomParam = false;
		mySet<int> paramIDList;
		for (const auto& row : info.mDataList)
		{
			if (!row[colNameList["Param0"]].empty())
			{
				hasCustomParam = true;
				break;
			}
		}
		if (hasCustomParam)
		{
			for (const auto& row : info.mDataList)
			{
				if (row[colNameList["Param0"]].empty())
				{
					continue;
				}
				const string paramClassName = "ED" + info.mHeader.mTableName + "_" + row[variableNameIndex];
				line(file, "");
				line(file, "public class " + paramClassName);
				line(file, "{");
				// 变量定义
				FOR_I(customParamCount)
				{
					string indexSuffix = IToS(i);
					const string& paramValue = row[colNameList["Param" + indexSuffix]];
					const string& paramType = row[colNameList["ParamType" + indexSuffix]];
					const string& paramName = row[colNameList["ParamName" + indexSuffix]];
					if (paramValue.empty())
					{
						break;
					}
					string csharpType = cppTypeToCSharpType(paramType);
					if (paramType == "Vector<int>")
					{
						myVector<int> values;
						SToIs(paramValue, values);
						line(file, "\tpublic static " + csharpType + " m" + paramName + " = new() { " + IsToS(values.data(), values.size(), 0, ", ") + " };");
					}
					else if (paramType == "Vector<llong>")
					{
						myVector<llong> values;
						SToLLs(paramValue, values);
						line(file, "\tpublic static " + csharpType + " m" + paramName + " = new() { " + LLsToS(values.data(), values.size(), 0, ", ") + " };");
					}
					else if (paramType == "Vector<float>")
					{
						myVector<float> values;
						SToFs(paramValue, values);
						string valueStr;
						FOR_VECTOR(values)
						{
							string str = FToS(values[i]);
							if ((int)str.find_first_of('.') < 0)
							{
								str += ".0f";
							}
							else
							{
								str += "f";
							}
							valueStr += str;
							if (i != values.size())
							{
								valueStr += ", ";
							}
						}
						line(file, "\tpublic static " + csharpType + " m" + paramName + " = new() { " + valueStr + " };");
					}
					else if (paramType == "int" || paramType == "llong")
					{
						line(file, "\tpublic static " + csharpType + " m" + paramName + " = " + paramValue + ";");
					}
					else if (paramType == "float")
					{
						string str = paramValue;
						if ((int)str.find_first_of('.') < 0)
						{
							str += ".0f";
						}
						else
						{
							str += "f";
						}
						line(file, "\tpublic static " + csharpType + " m" + paramName + " = " + str + ";");
					}
					else
					{
						ERROR("不支持的参数类型:" + paramType + ", 表格:" + info.mHeader.mTableName + ", id:" + row[0]);
					}
				}
				line(file, "}");
			}
		}
	}
	line(file, "// auto generate end", false);
	writeFile(dataFileHotFixPath + dataClassName + ".cs", file);
}

// ExcelTable.cs文件
void CodeExcel::generateCSharpExcelTableFile(const CSVInfo& info, const string& tableFilePath)
{
	if (info.mHeader.mOwner == OWNER::SERVER_ONLY || info.mHeader.mOwner == OWNER::NONE)
	{
		return;
	}
	string tableClassName = "Excel" + info.mHeader.mTableName;
	string dataClassName = "ED" + info.mHeader.mTableName;
	myVector<string> insertLines;
	insertLines.push_back("\tprotected override void checkAllDataDefault()");
	insertLines.push_back("\t{");
	insertLines.push_back("\t\tforeach (" + dataClassName + " item in queryAll())");
	insertLines.push_back("\t\t{");
	
	int preCheckLines = insertLines.size();
	myMap<string, myVector<int>> linkLengthMap;
	for (const ColumnData* member : info.mHeader.mColumnDataList)
	{
		if (member->mOwner == OWNER::BOTH || member->mOwner == OWNER::CLIENT_ONLY)
		{
			const string& linkLength = member->mLinkLength;
			if (!linkLength.empty())
			{
				if (!linkLengthMap.contains(linkLength))
				{
					myVector<int> tmep{ member->mIndex };
					linkLengthMap.insert(linkLength, tmep);
				}
				else
				{
					linkLengthMap[linkLength].push_back(member->mIndex);
				}
			}
			const string& name = member->mName;
			const string& linkTable = member->mLinkTable;
			if (!linkTable.empty())
			{
				bool isLinkExcel = false;
				if (isFileExist(SQLitePath + linkTable + ".db"))
				{
					isLinkExcel = false;
				}
				else if (isFileExist(ExcelPath + linkTable + ".csv"))
				{
					isLinkExcel = true;
				}
				else
				{
					ERROR("找不到客户端索引的表格:" + linkTable + ", 当前表格:" + info.mHeader.mTableName + ", 字段名:" + name);
					continue;
				}
				const string tableVarPrefix = isLinkExcel ? "mExcel" : "mSQLite";
				if (member->mEnumRealType.empty())
				{
					insertLines.push_back("\t\t\t" + tableVarPrefix + linkTable + ".checkData(item.m" + name + ", item.mID, this);");
				}
				else
				{
					insertLines.push_back("\t\t\t" + tableVarPrefix + linkTable + ".checkData((int)item.m" + name + ", item.mID, this);");
				}
			}
			if (!member->mEnumRealType.empty())
			{
				insertLines.push_back("\t\t\tcheckEnum(item.m" + name + ", \"m" + name + "\", item.mID);");
			}
			if (!member->mFlag.empty())
			{
				if (member->mFlag == "Path")
				{
					insertLines.push_back("\t\t\tif (!item.m" + name + ".isEmpty())");
					insertLines.push_back("\t\t\t{");
					insertLines.push_back("\t\t\t\tcheckPath(item.m" + name + ");");
					insertLines.push_back("\t\t\t}");
				}
				else if (member->mFlag == "ItemName")
				{
					string idColName = removeEndString(name, "Name");
					// 如果直接去掉Name后缀以后没有对应的字段名,则再尝试加上ID
					if (!info.mHeader.mColumnNameList.contains(idColName))
					{
						idColName += "ID";
					}
					if (startWith(member->mType, "Vector<"))
					{
						string tempListVarName = name;
						tempListVarName[0] = toLower(tempListVarName[0]);
						insertLines.push_back("\t\t\tusing var a" + name + " = new ListScope<string>(out var " + tempListVarName + ");");
						insertLines.push_back("\t\t\tfor (int i = 0; i < item.m" + idColName + ".Count; ++i)");
						insertLines.push_back("\t\t\t{");
						insertLines.push_back("\t\t\t\t" + tempListVarName + ".add(mExcelItem.query(item.m" + idColName + "[i])?.mName);");
						insertLines.push_back("\t\t\t}");
						insertLines.push_back("\t\t\tcheckStringValue(item.m" + name + ", " + tempListVarName + ", item.mID);");
					}
					else
					{
						insertLines.push_back("\t\t\tcheckStringValue(item.m" + name + ", mExcelItem.query(item.m" + idColName + ", false)?.mName, item.mID);");
					}
				}
				else if (member->mFlag == "PropertyName")
				{
					insertLines.push_back("\t\t\tcheckStringValue(item.m" + name + ", GD.PROPERTY_NAME.get(item.m" + removeEndString(name, "Name") + "), item.mID);");
				}
				else if (member->mFlag == "EquipTypeName")
				{
					insertLines.push_back("\t\t\tcheckStringValue(item.m" + name + ", GD.EQUIP_TYPE_NAME.get(item.m" + removeEndString(name, "Name") + "), item.mID);");
				}
			}
		}
	}
	FOREACH(iterLink, linkLengthMap)
	{
		const auto& colList = iterLink->second;
		if (colList.size() > 1)
		{
			FOR_I(colList.size() - 1)
			{
				const string& name0 = info.mHeader.mColumnDataList[colList[i]]->mName;
				const string& name1 = info.mHeader.mColumnDataList[colList[i + 1]]->mName;
				insertLines.push_back("\t\t\tcheckListPair(item.m" + name0 + ", item.m" + name1 + ", item.mID);");
			}
		}
	}
	const int checkLines = insertLines.size() - preCheckLines;
	insertLines.push_back("\t\t}");
	insertLines.push_back("\t}");
	// 如果没有任何需要检查的,就只插入一个空函数
	if (checkLines == 0)
	{
		insertLines.clear();
		insertLines.push_back("\tprotected override void checkAllDataDefault() {}");
	}

	// ExcelTable.cs文件
	string csFileName = tableFilePath + tableClassName + ".cs";
	if (!isFileExist(csFileName))
	{
		string table;
		line(table, "using System;");
		line(table, "using System.Collections.Generic;");
		line(table, "using static GBR;");
		line(table, "");
		line(table, "public class " + tableClassName + " : ExcelTableT<" + dataClassName + ">");
		line(table, "{");
		line(table, "\t// auto generate start");
		for (const string& str : insertLines)
		{
			line(table, str);
		}
		line(table, "\t// auto generate end");
		line(table, "}", false);
		writeFile(csFileName, table);
	}
	else
	{
		myVector<string> codeList;
		int lineStart = -1;
		if (!findCustomCode(csFileName, codeList, lineStart,
			[](const string& codeLine) { return endWith(codeLine, "// auto generate start"); },
			[](const string& codeLine) { return endWith(codeLine, "// auto generate end"); }, false))
		{
			// 如果找不到就在倒数第二行插入
			int lineIndex = codeList.size() - 2;
			codeList.insert(++lineIndex, "\t// auto generate start");
			codeList.insert(++lineIndex, "\t// auto generate end");
			lineStart = lineIndex - 1;
		}
		for (const string& str : insertLines)
		{
			codeList.insert(++lineStart, str);
		}
		writeFile(csFileName, codeList);
	}
}

// ExcelRegister.cs文件
void CodeExcel::generateCSharpExcelRegisteFileFile(const myVector<CSVInfo>& info, const string& fileHotFixPath)
{
	string hotFixfile;
	line(hotFixfile, "// auto generate start");
	line(hotFixfile, "using System;");
	line(hotFixfile, "using static GBR;");
	line(hotFixfile, "using static FrameBaseHotFix;");
	line(hotFixfile, "");
	line(hotFixfile, "public class ExcelRegister");
	line(hotFixfile, "{");
	line(hotFixfile, "\tpublic static void registeAll()");
	line(hotFixfile, "\t{");
	for (const CSVInfo& info : info)
	{
		if (info.mHeader.mOwner != OWNER::SERVER_ONLY && info.mHeader.mOwner != OWNER::NONE)
		{
			string lineStr = "\t\tregisteTable(out mExcel%s, typeof(ED%s), \"%s\");";
			replaceAll(lineStr, "%s", info.mHeader.mTableName);
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
	line(hotFixfile, "}");
	line(hotFixfile, "// auto generate end", false);
	writeFile(fileHotFixPath + "ExcelRegister.cs", hotFixfile);
}

// GameBaseExcel.cs文件
void CodeExcel::generateCSharpExcelDeclare(const myVector<CSVInfo>& info, const string& fileHotFixPath)
{
	// 热更工程中的表格注册
	string hotFixfile;
	line(hotFixfile, "// auto generate start");
	line(hotFixfile, "using System;");
	line(hotFixfile, "");
	line(hotFixfile, "// FrameBase的部分类,用于定义Excel表格的对象");
	line(hotFixfile, "public partial class GBR");
	line(hotFixfile, "{");
	for (const CSVInfo& info : info)
	{
		if (info.mHeader.mOwner != OWNER::SERVER_ONLY && info.mHeader.mOwner != OWNER::NONE)
		{
			line(hotFixfile, "\tpublic static Excel" + info.mHeader.mTableName + " mExcel" + info.mHeader.mTableName + ";");
		}
	}
	line(hotFixfile, "}");
	line(hotFixfile, "// auto generate end", false);
	writeFile(fileHotFixPath + "GameBaseExcelILR.cs", hotFixfile);
}

// 生成ExcelGlobal对应的cs代码
void CodeExcel::generateCSharpGlobalConfig(const CSVInfo& globalConfig, const string& tableFilePath)
{
	// ExcelGlobal.cs
	string dataClassName = "ED" + globalConfig.mHeader.mTableName;
	string tableClassName = "Excel" + globalConfig.mHeader.mTableName;
	string tableFileName = tableFilePath + tableClassName + ".cs";
	string tableString;
	line(tableString, "// auto generate start");
	line(tableString, "using UnityEngine;");
	line(tableString, "using System.Collections.Generic;");
	line(tableString, "using static StringUtility;");
	line(tableString, "");
	line(tableString, "public class " + tableClassName + " : ExcelTableT<" + dataClassName + ">");
	line(tableString, "{");

	int paramTypeIndex = -1;
	int paramNameIndex = -1;
	int paramValueIndex = -1;
	int paramDescIndex = -1;
	FOR_VECTOR(globalConfig.mHeader.mColumnDataList)
	{
		const string& name = globalConfig.mHeader.mColumnDataList[i]->mName;
		if (name == "ParamType")
		{
			paramTypeIndex = i;
		}
		else if (name == "ParamName")
		{
			paramNameIndex = i;
		}
		else if (name == "ParamValue")
		{
			paramValueIndex = i;
		}
		else if (name == "ParamDesc")
		{
			paramDescIndex = i;
		}
	}
	FOR_VECTOR(globalConfig.mDataList)
	{
		const auto& row = globalConfig.mDataList[i];
		const string& paramType = row[paramTypeIndex];
		const string& paramName = row[paramNameIndex];
		const string& paramValue = row[paramValueIndex];
		const string& paramDesc = row[paramDescIndex];
		if (paramType == "float")
		{
			string floatStr = paramValue;
			if ((int)floatStr.find_first_of('.') < 0)
			{
				floatStr += ".0";
			}
			string temp = "\tpublic const " + paramType + " " + paramName + " = " + floatStr + "f;";
			appendWithAlign(temp, "// " + paramDesc, 68);
			line(tableString, temp);
		}
		else if (paramType == "int")
		{
			string temp = "\tpublic const " + paramType + " " + paramName + " = " + paramValue + ";";
			appendWithAlign(temp, "// " + paramDesc, 68);
			line(tableString, temp);
		}
		else if (paramType == "llong")
		{
			string temp = "\tpublic const " + cppTypeToCSharpType(paramType) + " " + paramName + " = " + paramValue + ";";
			appendWithAlign(temp, "// " + paramDesc, 68);
			line(tableString, temp);
		}
		else
		{
			string temp = "\tpublic static " + cppTypeToCSharpType(paramType) + " " + paramName + ";";
			appendWithAlign(temp, "// " + paramDesc, 68);
			line(tableString, temp);
		}
	}
	line(tableString, "\tprotected override void onOpenFile()");
	line(tableString, "\t{");
	line(tableString, "\t\tusing var a = new DicScope<string, string>(out var paramMap);");
	line(tableString, "\t\tforeach (EDGlobal data in queryAll())");
	line(tableString, "\t\t{");
	line(tableString, "\t\t\tparamMap.add(data.mParamName, data.mParamValue.removeAllEmpty());");
	line(tableString, "\t\t}");
	FOR_VECTOR(globalConfig.mDataList)
	{
		const auto& row = globalConfig.mDataList[i];
		const string& paramType = row[paramTypeIndex];
		const string& paramName = row[paramNameIndex];
		const string& paramValue = row[paramValueIndex];
		const string& paramDesc = row[paramDescIndex];
		if (paramType == "Vector2Int")
		{
			line(tableString, "\t\t" + paramName + " = SToV2I(paramMap[\"" + paramName + "\"]);");
		}
		else if (paramType == "Vector2")
		{
			line(tableString, "\t\t" + paramName + " = SToV2(paramMap[\"" + paramName + "\"]);");
		}
		else if (paramType == "Vector3")
		{
			line(tableString, "\t\t" + paramName + " = SToV3(paramMap[\"" + paramName + "\"]);");
		}
		else if (paramType == "Vector3Int")
		{
			line(tableString, "\t\t" + paramName + " = SToV3I(paramMap[\"" + paramName + "\"]);");
		}
		else if (paramType == "Vector<int>")
		{
			line(tableString, "\t\t" + paramName + " = SToIs(paramMap[\"" + paramName + "\"]);");
		}
		else if (paramType == "Vector<float>")
		{
			line(tableString, "\t\t" + paramName + " = SToFs(paramMap[\"" + paramName + "\"]);");
		}
	}
	line(tableString, "\t}");
	line(tableString, "}");
	line(tableString, "// auto generate end", false);
	writeFile(tableFileName, tableString);
}

void CodeExcel::generateCSharpBuff(const CSVInfo& config)
{
	int variableNameIndex = -1;
	FOR_VECTOR(config.mHeader.mColumnDataList)
	{
		const string& name = config.mHeader.mColumnDataList[i]->mName;
		if (name == "VariableName")
		{
			variableNameIndex = i;
			break;
		}
	}

	myVector<string> buffClassNameList;
	if (variableNameIndex >= 0)
	{
		for (const auto& item : config.mDataList)
		{
			buffClassNameList.push_back(item[variableNameIndex]);
		}
	}
	const string registerFilePath = ClientHotFixPath + "Character/CharacterComponent/StateMachine/StateRegister.cs";
	// 更新特定部分代码
	myVector<string> codeList;
	int lineStart = -1;
	if (!findCustomCode(registerFilePath, codeList, lineStart,
		[](const string& codeLine) { return endWith(codeLine, "// auto generate start"); },
		[](const string& codeLine) { return endWith(codeLine, "// auto generate end"); }))
	{
		return;
	}

	for (const string& name : buffClassNameList)
	{
		codeList.insert(++lineStart, "\t\tregisteState<" + name + ", " + name + "Param>(EDBuff." + name + ");");
	}
	writeFile(registerFilePath, codeList);
}