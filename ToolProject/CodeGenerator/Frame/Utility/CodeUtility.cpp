#include "CodeUtility.h"
#include "Utility.h"

string CodeUtility::ServerGameProjectPath;
string CodeUtility::ServerFrameProjectPath;
string CodeUtility::ClientProjectPath;
string CodeUtility::ClientHotFixPath;
string CodeUtility::VirtualClientProjectPath;
myVector<string> CodeUtility::ServerExcludeIncludePath;
string CodeUtility::cppGamePath;
string CodeUtility::cppFramePath;
string CodeUtility::cppGameStringDefineHeaderFile;
string CodeUtility::VirtualClientSocketPath;
string CodeUtility::SQLitePath;
string CodeUtility::ExcelPath;
string CodeUtility::START_FALG = "#start";
bool CodeUtility::mGenerateVirtualClient = false;
myMap<string, myVector<string>> CodeUtility::mCachedFileLines;

myVector<string> CodeUtility::openFile(const string& file)
{
	auto* cache = mCachedFileLines.getPtr(file);
	if (cache != nullptr)
	{
		return *cache;
	}
	auto lines = openTxtFileLines1(file);
	mCachedFileLines.insert(file, lines);
	return lines;
}

void CodeUtility::writeFile(const string& file, const myVector<string>& content)
{
	if (openFile(file) == content)
	{
		return;
	}
	mCachedFileLines.set(file, content);
	FileUtility::writeFile1(file, ANSIToUTF8(codeListToString(content).c_str(), true));
}

void CodeUtility::writeFile(const string& file, const string& ansiContent)
{
	if (codeListToString(openFile(file)) == ansiContent)
	{
		return;
	}
	mCachedFileLines.set(file, splitLine(ansiContent.c_str(), false));
	FileUtility::writeFile1(file, ANSIToUTF8(ansiContent.c_str(), true));
}

void CodeUtility::deleteFile(const string& file)
{
	mCachedFileLines.erase(file);
	FileUtility::deleteFile1(file);
}

string CodeUtility::replaceVariable(const myMap<string, string>& variableDefine, const string& value)
{
	string temp = value;
	for (const auto& item : variableDefine)
	{
		if (findSubstr(temp, "{" + item.first + "}"))
		{
			replaceAll(temp, "{" + item.first + "}", item.second);
		}
	}
	return temp;
}

bool CodeUtility::initPath()
{
	myVector<string> configLines = openTxtFileLines1("./CodeGenerator_Config.txt");
	if (configLines.size() == 0)
	{
		ERROR("未找到配置文件CodeGenerator_Config.txt");
		return false;
	}
	myMap<string, string> variableDefine;
	for (const string& line : configLines)
	{
		if (startWith(line, "//") || line == "")
		{
			continue;
		}
		if (startWith(line, "#start"))
		{
			break;
		}
		if (line.find_first_of('=') >= 0)
		{
			myVector<string> pairs;
			split(line.c_str(), "=", pairs);
			if (pairs.size() != 2)
			{
				ERROR("配置错误:" + line);
				return false;
			}
			// 检查是否有引用之前的变量定义
			pairs[1] = replaceVariable(variableDefine, pairs[1]);
			rightToLeft(pairs[1]);
			variableDefine.insert(pairs[0], pairs[1], false);
		}
	}
	bool startConfig = false;
	FOR_VECTOR(configLines)
	{
		if (startWith(configLines[i], "//") || configLines[i] == "")
		{
			continue;
		}
		if (startWith(configLines[i], "#start"))
		{
			startConfig = true;
			continue;
		}
		myVector<string> params;
		removeAll(configLines[i], ' ', '\t');
		split(configLines[i].c_str(), "=", params);
		if (params.size() != 2)
		{
			continue;
		}
		const string& paramName = params[0];
		string& paramValue = params[1];
		paramValue = replaceVariable(variableDefine, paramValue);
		rightToLeft(paramValue);
		if (paramName == "CLIENT_PROJECT_PATH")
		{
			ClientProjectPath = paramValue;
		}
		else if (paramName == "CLIENT_HOTFIX_PATH")
		{
			ClientHotFixPath = paramValue;
		}
		else if (paramName == "SERVER_GAME_PROJECT_PATH")
		{
			ServerGameProjectPath = paramValue;
		}
		else if (paramName == "SERVER_FRAME_PROJECT_PATH")
		{
			ServerFrameProjectPath = paramValue;
		}
		else if (paramName == "SERVER_EXCLUDE_INCLUDE_PATH")
		{
			rightToLeft(paramValue);
			split(paramValue.c_str(), ",", ServerExcludeIncludePath);
		}
		else if (paramName == "VIRTUAL_CLIENT_PROJECT_PATH")
		{
			VirtualClientProjectPath = paramValue;
		}
		else if (paramName == "SQLITE_PATH")
		{
			SQLitePath = paramValue;
		}
		else if (paramName == "EXCEL_PATH")
		{
			ExcelPath = paramValue;
		}
		else if (paramName == "VIRTUAL_CLIENT")
		{
			mGenerateVirtualClient = SToBool(paramValue);
		}
	}

	if (ServerGameProjectPath.empty() && ClientProjectPath.empty() && VirtualClientProjectPath.empty())
	{
		return false;
	}

	rightToLeft(ServerGameProjectPath);
	rightToLeft(ServerFrameProjectPath);
	rightToLeft(SQLitePath);
	rightToLeft(ExcelPath);
	rightToLeft(ClientProjectPath);
	rightToLeft(ClientHotFixPath);
	rightToLeft(VirtualClientProjectPath);
	validPath(ServerGameProjectPath);
	validPath(ServerFrameProjectPath);
	validPath(SQLitePath);
	validPath(ExcelPath);
	validPath(ClientProjectPath);
	validPath(ClientHotFixPath);
	validPath(VirtualClientProjectPath);
	if (!ServerGameProjectPath.empty())
	{
		cppGamePath = ServerGameProjectPath + "Game/";
		cppFramePath = ServerFrameProjectPath + "Frame/";
		cppGameStringDefineHeaderFile = cppGamePath + "Common/GameStringDefine.h";
	}
	if (!VirtualClientProjectPath.empty())
	{
		VirtualClientSocketPath = VirtualClientProjectPath + "Assets/Scripts/HotFix/Socket/";
	}
	return true;
}

string CodeUtility::getElementTypeCpp(const string& type)
{
	int lastPos;
	findSubstr(type, ">", &lastPos);
	return type.substr(strlen("Vector<"), lastPos - strlen("Vector<"));
}

string CodeUtility::getElementTypeCS(const string& type)
{
	int lastPos;
	findSubstr(type, ">", &lastPos);
	return type.substr(strlen("List<"), lastPos - strlen("List<"));
}

MySQLMember CodeUtility::parseMySQLMemberLine(string line)
{
	MySQLMember memberInfo;

	// 字段注释
	int pos = -1;
	if (findString(line.c_str(), "//", &pos))
	{
		memberInfo.mComment = line.substr(pos + strlen("//"));
		removeStartAll(memberInfo.mComment, ' ');
		line = line.substr(0, pos);
	}

	// 字段类型和字段名
	myVector<string> memberStrList;
	split(line.c_str(), " ", memberStrList);
	if (findString(memberStrList[0].c_str(), "(utf8)"))
	{
		strReplaceAll(memberStrList[0], "(utf8)", "");
		memberInfo.mUTF8 = true;
	}
	else
	{
		memberInfo.mUTF8 = false;
	}
	memberInfo.mTypeName = memberStrList[0];
	memberInfo.mMemberName = memberStrList[1];
	return memberInfo;
}

PacketMember CodeUtility::parseMemberLine(const string& line)
{
	string newLine = line;
	string comment;
	int commentIndex = -1;
	if (findSubstr(newLine, "// ", &commentIndex))
	{
		comment = newLine.substr(commentIndex + strlen("// "));
		newLine = newLine.substr(0, commentIndex);
	}
	myVector<string> memberStrList;
	split(newLine.c_str(), " ", memberStrList);
	if (memberStrList.size() != 2 && memberStrList.size() != 3)
	{
		ERROR("成员变量行错误:" + line);
		return PacketMember();
	}
	strReplaceAll(memberStrList[0], "\t", "");
	PacketMember memberInfo;
	myVector<string> tagList = parseTagList(memberStrList[0], memberInfo.mTypeName);
	memberInfo.mOptional = tagList.contains("[Optional]");
	memberInfo.mMemberName = memberStrList[1];
	memberInfo.mMemberNameNoPrefix = memberInfo.mMemberName;
	memberInfo.mComment = comment;
	if (memberInfo.mMemberNameNoPrefix[0] == 'm')
	{
		memberInfo.mMemberNameNoPrefix.erase(0, 1);
	}
	return memberInfo;
}

string CodeUtility::packetNameToUpper(const string& packetName)
{
	// 根据大写字母拆分
	myVector<string> macroList;
	int length = (int)packetName.length();
	const int prefixLength = 2;
	if (packetName.substr(0, prefixLength) != "CS" && packetName.substr(0, prefixLength) != "SC")
	{
		ERROR("包名前缀错误");
		return "";
	}
	return packetName.substr(0, prefixLength) + nameToUpper(packetName.substr(prefixLength));
}

string CodeUtility::nameToUpper(const string& sqliteName, bool preUnderLine)
{
	// 根据大写字母拆分
	myVector<string> macroList;
	int length = (int)sqliteName.length();
	int lastIndex = 0;
	// 从1开始,因为第0个始终都是大写,会截取出空字符串,最后一个字母也肯不会被分割
	for (int i = 1; i < length; ++i)
	{
		// 以大写字母为分隔符,但是连续的大写字符不能被分隔
		// 非连续数字也会分隔
		char curChar = sqliteName[i];
		char lastChar = sqliteName[i - 1];
		char nextChar = i + 1 < length ? sqliteName[i + 1] : '\0';
		if (isUpper(curChar) && (!isUpper(lastChar) || (nextChar != '\0' && !isUpper(nextChar))) ||
			isNumber(curChar) && (!isNumber(lastChar) || (nextChar != '\0' && !isNumber(nextChar))))
		{
			macroList.push_back(sqliteName.substr(lastIndex, i - lastIndex));
			lastIndex = i;
		}
	}
	macroList.push_back(sqliteName.substr(lastIndex, length - lastIndex));
	string headerMacro;
	FOR_VECTOR(macroList)
	{
		headerMacro += "_" + toUpper(macroList[i]);
	}
	if (!preUnderLine)
	{
		headerMacro = headerMacro.erase(0, 1);
	}
	return headerMacro;
}

string CodeUtility::cSharpPushParamString(const PacketMember& memberInfo)
{
	return "pushParam(" + memberInfo.mMemberName + ");";
}

string CodeUtility::cppTypeToCSharpType(const string& cppType)
{
	string csharpType = cppType;
	replaceAll(csharpType, "char", "sbyte");
	replaceAll(csharpType, "llong", "long");
	replaceAll(csharpType, "Vector<", "List<");
	return csharpType;
}

string CodeUtility::cSharpTypeToWrapType(const string& csharpType)
{
	if (csharpType == "bool")
	{
		return "BIT_BOOL";
	}
	if (csharpType == "byte")
	{
		return "BIT_BYTE";
	}
	if (csharpType == "List<byte>")
	{
		return "BIT_BYTES";
	}
	if (csharpType == "sbyte")
	{
		return "BIT_SBYTE";
	}
	if (csharpType == "List<sbyte>")
	{
		return "BIT_SBYTES";
	}
	if (csharpType == "short")
	{
		return "BIT_SHORT";
	}
	if (csharpType == "List<short>")
	{
		return "BIT_SHORTS";
	}
	if (csharpType == "ushort")
	{
		return "BIT_USHORT";
	}
	if (csharpType == "List<ushort>")
	{
		return "BIT_USHORTS";
	}
	if (csharpType == "int")
	{
		return "BIT_INT";
	}
	if (csharpType == "List<int>")
	{
		return "BIT_INTS";
	}
	if (csharpType == "uint")
	{
		return "BIT_UINT";
	}
	if (csharpType == "List<uint>")
	{
		return "BIT_UINTS";
	}
	if (csharpType == "long")
	{
		return "BIT_LONG";
	}
	if (csharpType == "List<long>")
	{
		return "BIT_LONGS";
	}
	if (csharpType == "float")
	{
		return "BIT_FLOAT";
	}
	if (csharpType == "List<float>")
	{
		return "BIT_FLOATS";
	}
	if (csharpType == "string")
	{
		return "BIT_STRING";
	}
	if (csharpType == "List<string>")
	{
		return "BIT_STRINGS";
	}
	if (csharpType == "Vector2")
	{
		return "BIT_VECTOR2";
	}
	if (csharpType == "Vector2UShort")
	{
		return "BIT_VECTOR2_USHORT";
	}
	if (csharpType == "Vector2Int")
	{
		return "BIT_VECTOR2_INT";
	}
	if (csharpType == "Vector2UInt")
	{
		return "BIT_VECTOR2_UINT";
	}
	if (csharpType == "Vector3")
	{
		return "BIT_VECTOR3";
	}
	if (csharpType == "Vector4")
	{
		return "BIT_VECTOR4";
	}
	// 如果是自定义的参数类型的列表,则是此类型的名字加上_List后缀
	if (startWith(csharpType, "List<"))
	{
		int lastPos;
		findSubstr(csharpType, ">", &lastPos);
		return csharpType.substr(strlen("List<"), lastPos - strlen("List<")) + "_List";
	}
	return csharpType;
}

string CodeUtility::cSharpMemberDeclareString(const PacketMember& memberInfo)
{
	// c#里面不用char,使用byte,也没有ullong,使用long
	string typeName = cSharpTypeToWrapType(cppTypeToCSharpType(memberInfo.mTypeName));
	string str = "public " + typeName + " " + memberInfo.mMemberName + " = new();";
	if (!memberInfo.mComment.empty())
	{
		appendWithAlign(str, "// " + memberInfo.mComment, 52);
	}
	return str;
}

myVector<string> CodeUtility::parseTagList(const string& line, string& newLine)
{
	newLine = line;
	myVector<string> tagList;
	while (true)
	{
		int startIndex = -1;
		int endIndex = -1;
		if (!findSubstr(newLine, "[", &startIndex) || !findSubstr(newLine, "]", &endIndex, startIndex))
		{
			break;
		}
		tagList.push_back(newLine.substr(startIndex, endIndex - startIndex + 1));
		newLine = newLine.erase(startIndex, endIndex - startIndex + 1);
	}
	return tagList;
}

void CodeUtility::parseStructName(const string& line, PacketStruct& structInfo)
{
	// 查找标签
	parseTagList(line, structInfo.mStructName);
}

void CodeUtility::parsePacketName(const string& line, PacketInfo& packetInfo)
{
	// 查找标签
	myVector<string> tagList = parseTagList(line, packetInfo.mPacketName);
	packetInfo.mUDP = tagList.contains("[UDP]");
	packetInfo.mShowInfo = !tagList.contains("[NoLog]");
}

string CodeUtility::convertToCSharpType(const string& cppType)
{
	string newType = cppType;
	// 因为模板文件是按照C++来写的,但是有些类型在C#中是没有的,所以要转换为C#中对应的类型
	// Vector替换为List,char替换为sbyte,llong替换为long
	if (startWith(cppType, "Vector<"))
	{
		strReplaceAll(newType, "Vector<", "List<");
	}
	strReplaceAll(newType, "char", "sbyte");
	strReplaceAll(newType, "llong", "long");
	return newType;
}

bool CodeUtility::findCustomCode(const string& fullPath, myVector<string>& codeList, int& lineStart, 
								const LineMatchCallback& startLineMatch, const LineMatchCallback& endLineMatch, bool showError)
{
	codeList = openFile(fullPath);
	lineStart = -1;
	int endCode = -1;
	for (int i = 0; i < codeList.size(); ++i)
	{
		if (lineStart < 0 && startLineMatch(codeList[i]))
		{
			lineStart = i;
			continue;
		}
		if (lineStart >= 0)
		{
			if (endLineMatch(codeList[i]))
			{
				endCode = i;
				break;
			}
		}
	}
	if (lineStart < 0)
	{
		if (showError)
		{
			ERROR("找不到代码特定起始段,文件名:" + fullPath);
		}
		return false;
	}
	if (endCode < 0)
	{
		if (showError)
		{
			ERROR("找不到代码特定结束段,文件名:" + fullPath);
		}
		return false;
	}
	int removeLineCount = endCode - lineStart - 1;
	for (int i = 0; i < removeLineCount; ++i)
	{
		codeList.erase(lineStart + 1);
	}
	return true;
}

myVector<string> CodeUtility::findClass(const string& path, const LineMatchCallback& fileNameMatch, const LineMatchCallback& lineMatch)
{
	return findClass(findFiles(path, ".h"), fileNameMatch, lineMatch);
}

myVector<string> CodeUtility::findClass(const myVector<string>& files, const LineMatchCallback& fileNameMatch, const LineMatchCallback& lineMatch)
{
	myVector<string> classList;
	for (const string& fileName : files)
	{
		if (!fileNameMatch(getFileNameNoSuffix(fileName, true)))
		{
			continue;
		}
		for (const string& line : openFile(fileName))
		{
			if (lineMatch(line))
			{
				string className = findClassName(line);
				if (!className.empty())
				{
					classList.push_back(className);
				}
			}
		}
	}
	return classList;
}

myVector<string> CodeUtility::findPoolClass(const myVector<string>& files)
{
	myVector<string> classList;
	for (const string& fileName : files)
	{
		string cleanFileName = getFileNameNoSuffix(fileName, true);
		// 排除不可能是对象池的类
		if (startWith(cleanFileName, "CS") || 
			startWith(cleanFileName, "SC") ||
			startWith(cleanFileName, "NetStruct") ||
			startWith(cleanFileName, "MD") ||
			startWith(cleanFileName, "ED") ||
			startWith(cleanFileName, "SD") ||
			(startWith(cleanFileName, "Cmd") && isUpper(cleanFileName[3])) ||
			endWith(cleanFileName, "Test"))
		{
			continue;
		}
		for (const string& line : openFile(fileName))
		{
			string poolName0 = getPoolName(line, "CLASS_POOL");
			if (!poolName0.empty())
			{
				classList.push_back(poolName0 + "Pool");
				continue;
			}
		}
	}
	return classList;
}

string CodeUtility::getPoolName(const string& line, const string& preKey)
{
	if (!startWith(line, preKey))
	{
		return "";
	}
	int index0 = -1;
	if (!findSubstr(line, preKey + "(", &index0) || index0 != 0)
	{
		return "";
	}
	int index1 = -1;
	if (findSubstr(line, ",", &index1, index0))
	{
		return line.substr(index0 + preKey.length() + 1, index1 - (index0 + preKey.length() + 1));
	}
	else
	{
		return line.substr(index0 + preKey.length() + 1, line.length() - (index0 + preKey.length() + 1) - 2);
	}
}

string CodeUtility::codeListToString(const myVector<string>& codeList)
{
	string str;
	for (int i = 0; i < codeList.size(); ++i)
	{
		line(str, codeList[i], i != codeList.size() - 1);
	}
	return str;
}

string CodeUtility::findClassName(const string& line)
{
	if ((int)line.find_first_of('#') >= 0)
	{
		return "";
	}
	string newLine = line;
	int index0 = -1;
	if (findSubstr(line, "{", &index0))
	{
		newLine = line.substr(0, index0);
		trimEnd(newLine);
	}
	if (startWith(newLine, "class ") && !findString(newLine.c_str(), ";"))
	{
		// 截取出:前面的字符串
		int colonPos = (int)newLine.find(':');
		// 有:,最靠近:的才是类名
		if (colonPos != -1)
		{
			myVector<string> elements;
			split(newLine.substr(0, colonPos).c_str(), " ", elements);
			if (elements.size() == 0)
			{
				return "";
			}
			return elements[elements.size() - 1];
		}
		// 没有:,则最后一个元素是类名
		else
		{
			myVector<string> elements;
			split(newLine.c_str(), " ", elements);
			if (elements.size() == 0)
			{
				return "";
			}
			return elements[elements.size() - 1];
		}
	}
	return "";
}

string CodeUtility::findClassBaseName(const string& line)
{
	if (startWith(line, "class ") && !findString(line.c_str(), ";"))
	{
		// 截取出:前面的字符串
		int colonPos = (int)line.find(':');
		// 有:,冒号右边的才是基类名
		if (colonPos != -1)
		{
			myVector<string> elements;
			split(line.substr(colonPos + 1).c_str(), " ", elements);
			if (elements.size() < 2)
			{
				return "";
			}
			return elements[1];
		}
	}
	return "";
}

void CodeUtility::generateStringDefine(const myVector<string>& defineList, int startID, const string& key, const string& stringDefineHeaderFile)
{
	// 更新StringDefine.h的特定部分
	myVector<string> codeListHeader;
	int lineStartHeader = -1;
	if (!findCustomCode(stringDefineHeaderFile, codeListHeader, lineStartHeader,
		[key](const string& codeLine) { return endWith(codeLine, "// auto generate start " + key); },
		[key](const string& codeLine) { return endWith(codeLine, "// auto generate end " + key); }))
	{
		return;
	}

	for (const string& item : defineList)
	{
		const string line = "\tstatic constexpr ushort " + item + " = " + IToS(++startID) + ";";
		codeListHeader.insert(++lineStartHeader, line);
	}
	writeFile(stringDefineHeaderFile, codeListHeader);
}

void CodeUtility::parseCSVLine(const string& fullContent, myVector<myVector<string>>& result)
{
	myVector<string> row;
	string field;
	bool inQuotes = false;
	for (int i = 0; i < (int)fullContent.size(); ++i)
	{
		const char c = fullContent[i];
		if (c == '"')
		{
			if (inQuotes && i + 1 < fullContent.size() && fullContent[i + 1] == '"')
			{
				// 转义的双引号 ""
				field.push_back('"');
				++i;
			}
			else
			{
				// 进入或退出引号
				inQuotes = !inQuotes;
			}
		}
		else if (c == ',' && !inQuotes)
		{
			// 逗号分隔列
			row.push_back(field);
			field.clear();
		}
		else if ((c == '\n' || c == '\r') && !inQuotes)
		{
			// 遇到换行，完成一行,即使空行也要放进去
			//if (!field.empty() || !row.isEmpty())
			{
				row.push_back(field);
				field.clear();
				result.push_back(row);
				row.clear();
			}
			// 处理 \r\n 的情况：如果当前是 \r 且下一个是 \n，就跳过 \n
			if (c == '\r' && i + 1 < fullContent.size() && fullContent[i + 1] == '\n')
			{
				++i;
			}
		}
		else
		{
			// 普通字符
			field.push_back(c);
		}
	}

	// 最后一行如果没加进去，要补上
	if (!field.empty() || row.size() != 0)
	{
		row.push_back(field);
		result.push_back(row);
	}
}

OWNER CodeUtility::getOwner(const string& owner)
{
	if (owner == "None")
	{
		return OWNER::NONE;
	}
	else if (owner == "Server")
	{
		return OWNER::SERVER_ONLY;
	}
	else if (owner == "Client")
	{
		return OWNER::CLIENT_ONLY;
	}
	else if (owner == "Both")
	{
		return OWNER::BOTH;
	}
	return OWNER::NONE;
}

void CodeUtility::parseCSV(const string& file, CSVHeader& header, myVector<myVector<string>>& dataList)
{
	header.mTableName = getFileNameNoSuffix(file, true);
	myVector<myVector<string>> result;
	parseCSVLine(openTxtFile1(file), result);
	if (result.size() < HEADER_DATA_ROW)
	{
		return;
	}
	FOR_I(HEADER_DATA_ROW)
	{
		const myVector<string>& line = result[i];
		// 表名
		if (i == ROW_TABLE_NAME)
		{
			header.mComment = line[0];
		}
		// 表所属
		else if (i == ROW_TABLE_OWNER)
		{
			header.mOwner = getOwner(line[0]);
		}
		// 字段名
		else if (i == ROW_COLUMN_NAME)
		{
			FOR_VECTOR_J(line)
			{
				ColumnData* colData = new ColumnData;
				colData->mIndex = j;
				colData->mName = line[j];
				header.mColumnDataList.push_back(colData);
				header.mColumnNameList.insert(colData->mName, colData);
			}
		}
		// 字段类型
		else if (i == ROW_COLUMN_TYPE)
		{
			FOR_VECTOR_J(line)
			{
				string type = line[j];
				int leftPos = 0;
				int rightPos = 0;
				if (findSubstr(type, "(", &leftPos) && findSubstr(type, ")", &rightPos))
				{
					header.mColumnDataList[j]->mEnumRealType = type.substr(leftPos + 1, rightPos - leftPos - 1);
					type = type.erase(leftPos, rightPos - leftPos + 1);
				}
				header.mColumnDataList[j]->mType = type;
			}
		}
		// 字段所属
		else if (i == ROW_COLUMN_OWNER)
		{
			FOR_VECTOR_J(line)
			{
				header.mColumnDataList[j]->mOwner = getOwner(line[j]);
			}
		}
		// 字段注释
		else if (i == ROW_COLUMN_COMMENT)
		{
			FOR_VECTOR_J(line)
			{
				header.mColumnDataList[j]->mComment = line[j];
			}
		}
		// 链接表格
		else if (i == ROW_COLUMN_LINK_TABLE)
		{
			FOR_VECTOR_J(line)
			{
				header.mColumnDataList[j]->mLinkTable = line[j];
			}
		}
		// 链接表格
		else if (i == ROW_COLUMN_LINK_LENGTH)
		{
			FOR_VECTOR_J(line)
			{
				header.mColumnDataList[j]->mLinkLength = line[j];
			}
		}
		// 字段标签
		else if (i == ROW_COLUMN_FLAG)
		{
			FOR_VECTOR_J(line)
			{
				header.mColumnDataList[j]->mFlag = line[j];
			}
		}
	}
	for (int i = HEADER_DATA_ROW; i < result.size(); ++i)
	{
		dataList.push_back(result[i]);
	}
}