#include "CodeNetPacket.h"

void CodeNetPacket::generate()
{
	print("正在生成网络消息");
	myVector<PacketStruct> structInfoList;
	myVector<PacketInfo> packetInfoList;
	parsePacketConfig(structInfoList, packetInfoList);

	generateCpp(structInfoList, packetInfoList);

	if (!ClientHotFixPath.empty())
	{
		generateCSharp(structInfoList, packetInfoList);
	}
	print("完成生成网络消息");
	print("");
}

void CodeNetPacket::generateVirtualClient()
{
	if (VirtualClientSocketPath.empty())
	{
		ERROR("未配置虚拟客户端项目路径");
		return;
	}
	print("正在生成虚拟客户端网络消息");
	myVector<PacketStruct> structInfoList;
	myVector<PacketInfo> packetInfoList;
	parsePacketConfig(structInfoList, packetInfoList);

	generateCSharpVirtualClient(structInfoList, packetInfoList);
	print("完成生成虚拟客户端网络消息");
	print("");
}

void CodeNetPacket::parsePacketConfig(myVector<PacketStruct>& structInfoList, myVector<PacketInfo>& packetInfoList)
{
	// 解析模板文件
	myVector<string> csLines = openTxtFileLines("PacketCS.txt");
	myVector<string> scLines = openTxtFileLines("PacketSC.txt");
	myVector<string> structLines = openTxtFileLines("PacketStruct.txt");
	if (csLines.size() == 0)
	{
		ERROR("未找到协议文件PacketCS.txt");
		return;
	}
	if (scLines.size() == 0)
	{
		ERROR("未找到协议文件PacketSC.txt");
		return;
	}

	// 解析结构体定义
	bool structStart = false;
	myVector<PacketMember> tempStructMemberList;
	int tempStructNameLine = 0;
	FOR_VECTOR(structLines)
	{
		string line = structLines[i];
		// 忽略注释
		if (startWith(line, "//"))
		{
			continue;
		}
		// 如果后面插有注释,则去除
		int pos = -1;
		if (findString(line.c_str(), "//", &pos))
		{
			line = line.substr(0, pos);
		}
		// 去除所有制表符,分号
		removeAll(line, '\t', ';');
		// 没有成员变量的消息包
		if (line == "{}")
		{
			PacketStruct info;
			parseStructName(structLines[i - 1], info);
			info.mComment = structLines[i - 2];
			structInfoList.push_back(info);
			continue;
		}
		// 成员变量列表起始
		if (line == "{")
		{
			structStart = true;
			tempStructNameLine = i - 1;
			tempStructMemberList.clear();
			continue;
		}
		// 成员变量列表结束
		if (line == "}")
		{
			if (!structStart)
			{
				ERROR("未找到前一个匹配的{, PacketStruct,前5行内容:");
				int printStartLine = (int)i - 5;
				clampMin(printStartLine, 0);
				for (int j = printStartLine; j <= (int)i; ++j)
				{
					ERROR(structLines[j]);
				}
			}
			PacketStruct info;
			parseStructName(structLines[tempStructNameLine], info);
			info.mMemberList = tempStructMemberList;
			info.mComment = structLines[tempStructNameLine - 1];
			structInfoList.push_back(info);
			structStart = false;
			tempStructMemberList.clear();
			tempStructNameLine = -1;
			continue;
		}
		if (structStart)
		{
			PacketMember member = parseMemberLine(line);
			member.mIndex = tempStructMemberList.size();
			tempStructMemberList.push_back(member);
			if (tempStructMemberList.size() >= 64 && tempStructMemberList[tempStructMemberList.size() - 1].mOptional)
			{
				ERROR("仅支持前64个字段允许设置为可选字段,包名:" + structLines[tempStructNameLine]);
			}
		}
	}

	// 解析消息定义
	myVector<string> allLines;
	allLines.addRange(csLines);
	allLines.addRange(scLines);
	bool packetStart = false;
	myVector<PacketMember> tempMemberList;
	int tempPacketNameLine = 0;
	FOR_VECTOR(allLines)
	{
		string line = allLines[i];
		// 忽略注释
		if (startWith(line, "//"))
		{
			continue;
		}
		// 去除所有制表符,分号
		removeAll(line, '\t', ';');
		// 没有成员变量的消息包
		if (line == "{}")
		{
			PacketInfo info;
			parsePacketName(allLines[i - 1], info);
			info.mComment = allLines[i - 2];
			packetInfoList.push_back(info);
			continue;
		}
		// 成员变量列表起始
		if (line == "{")
		{
			packetStart = true;
			tempPacketNameLine = i - 1;
			tempMemberList.clear();
			continue;
		}
		// 成员变量列表结束
		if (line == "}")
		{
			if (!packetStart)
			{
				ERROR("未找到前一个匹配的{, NetPacket,前5行内容:");
				int printStartLine = (int)i - 5;
				clampMin(printStartLine, 0);
				for (int j = printStartLine; j <= (int)i; ++j)
				{
					ERROR(allLines[j]);
				}
			}
			PacketInfo info;
			parsePacketName(allLines[tempPacketNameLine], info);
			info.mMemberList = tempMemberList;
			info.mComment = allLines[tempPacketNameLine - 1];
			packetInfoList.push_back(info);
			packetStart = false;
			tempMemberList.clear();
			tempPacketNameLine = -1;
			continue;
		}
		if (packetStart)
		{
			PacketMember member = parseMemberLine(line);
			member.mIndex = tempMemberList.size();
			tempMemberList.push_back(member);
			if (tempMemberList.size() >= 64 && tempMemberList[tempMemberList.size() - 1].mOptional)
			{
				ERROR("仅支持前64个字段允许设置为可选字段,包名:" + allLines[tempPacketNameLine]);
			}
		}
	}
}

void CodeNetPacket::generateCpp(const myVector<PacketStruct>& structInfoList, const myVector<PacketInfo>& packetInfoList)
{
	myVector<string> gamePacketNameList;
	for (const PacketInfo& packetInfo : packetInfoList)
	{
		gamePacketNameList.push_back(packetInfo.mPacketName);
	}
	myVector<string> structNameList;
	for (const PacketStruct& structInfo : structInfoList)
	{
		structNameList.push_back(structInfo.mStructName);
	}
	// Game层的消息
	string cppGameCSPacketPath = cppGamePath + "Socket/ClientServer/";
	string cppGameSCPacketPath = cppGamePath + "Socket/ServerClient/";
	string cppGameStructPath = cppGamePath + "Socket/Struct/";
	string cppGamePacketDefinePath = cppGamePath + "Socket/";
	// 删除无用的消息
	// c++ CS
	myVector<string> cppGameCSFiles;
	findFiles(cppGameCSPacketPath, cppGameCSFiles, nullptr, 0);
	for (int i = 0; i < cppGameCSFiles.size(); ++i)
	{
		if (!gamePacketNameList.contains(getFileNameNoSuffix(cppGameCSFiles[i], true)))
		{
			deleteFile(cppGameCSFiles[i]);
			cppGameCSFiles.erase(i--);
		}
	}
	// c++ SC
	myVector<string> cppGameSCFiles;
	findFiles(cppGameSCPacketPath, cppGameSCFiles, nullptr, 0);
	for (int i = 0; i < cppGameSCFiles.size(); ++i)
	{
		if (!gamePacketNameList.contains(getFileNameNoSuffix(cppGameSCFiles[i], true)))
		{
			deleteFile(cppGameSCFiles[i]);
			cppGameSCFiles.erase(i--);
		}
	}
	// 消息结构体代码
	myVector<string> cppNetStructFiles;
	findFiles(cppGameStructPath, cppNetStructFiles, nullptr, 0);
	for (int i = 0; i < cppNetStructFiles.size(); ++i)
	{
		if (!structNameList.contains(getFileNameNoSuffix(cppNetStructFiles[i], true)))
		{
			deleteFile(cppNetStructFiles[i]);
			cppNetStructFiles.erase(i--);
		}
	}

	// 生成c++代码
	for (const PacketInfo& packetInfo : packetInfoList)
	{
		// 找到有没有此文件,有就在原来的文件上修改
		string csHeaderPath = cppGameCSPacketPath;
		string csSourcePath = cppGameCSPacketPath;
		string scHeaderPath = cppGameSCPacketPath;
		for (const string& file : cppGameCSFiles)
		{
			if (endWith(file, packetInfo.mPacketName + ".h"))
			{
				csHeaderPath = getFilePath(file) + "/";
			}
			if (endWith(file, packetInfo.mPacketName + ".cpp"))
			{
				csSourcePath = getFilePath(file) + "/";
			}
		}
		for (const string& file : cppGameSCFiles)
		{
			if (endWith(file, packetInfo.mPacketName + ".h"))
			{
				scHeaderPath = getFilePath(file) + "/";
			}
		}
		generateCppCSPacketFileHeader(packetInfo, csHeaderPath);
		generateCppCSPacketFileSource(packetInfo, csSourcePath);
		generateCppSCPacketFileHeader(packetInfo, scHeaderPath);
		generateCppSCPacketFileSource(packetInfo, scHeaderPath);
	}
	generateCppGamePacketDefineFile(packetInfoList, cppGamePacketDefinePath);
	generateCppGamePacketRegisteFile(packetInfoList, structInfoList, cppGamePacketDefinePath);

	for (const PacketStruct& info : structInfoList)
	{
		generateCppStruct(info, cppGameStructPath);
	}
}

void CodeNetPacket::generateCSharp(const myVector<PacketStruct>& structInfoList, const myVector<PacketInfo>& packetInfoList)
{
	string csharpCSHotfixPath = ClientHotFixPath + "Socket/ClientServer/";
	string csharpSCHotfixPath = ClientHotFixPath + "Socket/ServerClient/";
	string csharpStructHotfixPath = ClientHotFixPath + "Socket/Struct/";
	string csharpPacketDefinePath = ClientHotFixPath + "Socket/";

	myVector<string> hotfixList;
	for (const PacketInfo& packetInfo : packetInfoList)
	{
		hotfixList.push_back(packetInfo.mPacketName);
	}
	myVector<string> hotfixStructList;
	for (const PacketStruct& structInfo : structInfoList)
	{
		hotfixStructList.push_back(structInfo.mStructName);
	}
	// 删除无用的消息
	// c# CS热更
	myVector<string> csharpCSHotfixFiles;
	findFiles(csharpCSHotfixPath, csharpCSHotfixFiles, ".cs");
	for (const string& file : csharpCSHotfixFiles)
	{
		if (!hotfixList.contains(getFileNameNoSuffix(file, true)))
		{
			deleteFile(file);
		}
	}
	// c# SC热更
	myVector<string> csharpSCHotfixFiles;
	findFiles(csharpSCHotfixPath, csharpSCHotfixFiles, ".cs");
	for (const string& file : csharpSCHotfixFiles)
	{
		if (!hotfixList.contains(getFileNameNoSuffix(file, true)))
		{
			deleteFile(file);
		}
	}
	// 删除无用的结构体代码
	myVector<string> csharpHotfixStructFiles;
	findFiles(csharpStructHotfixPath, csharpHotfixStructFiles, ".cs");
	for (const string& file : csharpHotfixStructFiles)
	{
		if (!hotfixStructList.contains(getFileNameNoSuffix(file, true)))
		{
			deleteFile(file);
		}
	}

	// 生成cs代码
	for (const PacketInfo& packetInfo : packetInfoList)
	{
		generateCSharpPacketFile(packetInfo, csharpCSHotfixPath, csharpSCHotfixPath);
	}
	generateCSharpPacketDefineFile(packetInfoList, csharpPacketDefinePath);
	generateCSharpPacketRegisteFile(packetInfoList, structInfoList, csharpPacketDefinePath);

	// 生成结构体代码
	for (const PacketStruct& item : structInfoList)
	{
		generateCSharpStruct(item, csharpStructHotfixPath);
	}
}

void CodeNetPacket::generateCSharpVirtualClient(const myVector<PacketStruct>& structInfoList, const myVector<PacketInfo>& packetInfoList)
{
	string csharpCSGamePath = VirtualClientSocketPath + "ClientServer/";
	string csharpSCGamePath = VirtualClientSocketPath + "ServerClient/";
	string csharpStructGamePath = VirtualClientSocketPath + "Struct/";
	string csharpPacketDefinePath = VirtualClientSocketPath;

	myVector<string> packetNameList;
	for (const PacketInfo& packetInfo : packetInfoList)
	{
		packetNameList.push_back(packetInfo.mPacketName);
	}
	myVector<string> structNameList;
	for (const PacketStruct& structInfo : structInfoList)
	{
		structNameList.push_back(structInfo.mStructName);
	}

	// 删除无用的消息
	// CS
	myVector<string> csharpCSFiles;
	findFiles(csharpCSGamePath, csharpCSFiles, ".cs");
	for (const string& file : csharpCSFiles)
	{
		if (!packetNameList.contains(getFileNameNoSuffix(file, true)))
		{
			deleteFile(file);
			deleteFile(file + ".meta");
		}
	}
	// SC
	myVector<string> csharpSCFiles;
	findFiles(csharpSCGamePath, csharpSCFiles, ".cs");
	for (const string& file : csharpSCFiles)
	{
		if (!packetNameList.contains(getFileNameNoSuffix(file, true)))
		{
			deleteFile(file);
			deleteFile(file + ".meta");
		}
	}
	// 删除无用的结构体代码
	myVector<string> csharpStructFiles;
	findFiles(csharpStructGamePath, csharpStructFiles, ".cs");
	for (const string& file : csharpStructFiles)
	{
		if (!structNameList.contains(getFileNameNoSuffix(file, true)))
		{
			deleteFile(file);
		}
	}

	// 生成cs代码
	for (const PacketInfo& packetInfo : packetInfoList)
	{
		generateCSharpPacketFile(packetInfo, csharpCSGamePath, csharpSCGamePath);
	}
	generateCSharpPacketDefineFile(packetInfoList, csharpPacketDefinePath);
	generateCSharpPacketRegisteFile(packetInfoList, structInfoList, csharpPacketDefinePath);

	// 生成结构体代码
	for (const PacketStruct& item : structInfoList)
	{
		generateCSharpStruct(item, csharpStructGamePath);
	}
}

// PacketDefine.h文件
void CodeNetPacket::generateCppGamePacketDefineFile(const myVector<PacketInfo>& packetList, const string& filePath)
{
	const string fullPath = filePath + "GamePacketDefine.h";
	myVector<string> generateList;
	generateList.push_back("\tconstexpr static ushort MIN = 0;");
	generateList.push_back("");
	int csMinValue = 10000;
	generateList.push_back("\tconstexpr static ushort CS_MIN = " + intToString(csMinValue) + ";");
	uint packetCount = packetList.size();
	FOR_I(packetCount)
	{
		if (startWith(packetList[i].mPacketName, "CS"))
		{
			generateList.push_back("\tconstexpr static ushort " + packetList[i].mPacketName + " = " + intToString(++csMinValue) + ";");
		}
	}
	generateList.push_back("");
	int scMinValue = 20000;
	generateList.push_back("\tconstexpr static ushort SC_MIN = " + intToString(scMinValue) + ";");
	FOR_I(packetCount)
	{
		if (startWith(packetList[i].mPacketName, "SC"))
		{
			generateList.push_back("\tconstexpr static ushort " + packetList[i].mPacketName + " = " + intToString(++scMinValue) + ";");
		}
	}
	if (isFileExist(fullPath))
	{
		myVector<string> codeList;
		int lineStart = -1;
		if (!findCustomCode(fullPath, codeList, lineStart,
			[](const string& codeLine) { return endWith(codeLine, "// auto generate start"); },
			[](const string& codeLine) { return endWith(codeLine, "// auto generate end"); }))
		{
			return;
		}
		for (const string& line : generateList)
		{
			codeList.insert(++lineStart, line);
		}
		writeFile(fullPath, ANSIToUTF8(codeListToString(codeList).c_str(), true));
	}
	else
	{
		string str;
		line(str, "#pragma once");
		line(str, "");
		line(str, "#include \"FrameDefine.h\"");
		line(str, "");
		line(str, "class PACKET_TYPE");
		line(str, "{");
		line(str, "public:");
		for (const string& code : generateList)
		{
			line(str, code);
		}
		line(str, "};", false);

		writeFile(fullPath, ANSIToUTF8(str.c_str(), true));
	}
}

// PacketRegister.cpp文件
void CodeNetPacket::generateCppGamePacketRegisteFile(const myVector<PacketInfo>& packetList, const myVector<PacketStruct>& structInfoList, const string& filePath)
{
	string str;
	line(str, "#include \"GameHeader.h\"");
	
	line(str, "");
	line(str, "string GamePacketRegister::PACKET_VERSION = \"" + generatePacketVersion(packetList, structInfoList) + "\";");
	line(str, "void GamePacketRegister::registeAll()");
	
	line(str, "{");
	myVector<PacketInfo> udpCSList;
	for (const auto& info : packetList)
	{
		if (startWith(info.mPacketName, "CS") && info.mUDP)
		{
			udpCSList.push_back(info);
		}
	}
	for (const auto& info : packetList)
	{
		const string& packetName = info.mPacketName;
		if (!startWith(packetName, "CS"))
		{
			continue;
		}
		line(str, "\tmPacketTCPFactoryManager->addFactory<" + packetName + ">(PACKET_TYPE::" + packetName + ");");
	}
	line(str, "");
	myVector<PacketInfo> udpSCList;
	for (const auto& info : packetList)
	{
		if (!startWith(info.mPacketName, "SC"))
		{
			continue;
		}
		if (info.mUDP)
		{
			udpSCList.push_back(info);
		}
	}
	for (const auto& info : packetList)
	{
		const string& packetName = info.mPacketName;
		if (!startWith(packetName, "SC"))
		{
			continue;
		}
		line(str, "\tmPacketTCPFactoryManager->addFactory<" + packetName + ">(PACKET_TYPE::" + packetName + ");");
	}
	if (udpCSList.size() > 0)
	{
		line(str, "");
		for (const auto& info : udpCSList)
		{
			line(str, "\tmPacketTCPFactoryManager->addUDPPacket(PACKET_TYPE::" + info.mPacketName + "); ");
		}
	}
	if (udpSCList.size() > 0)
	{
		line(str, "");
		for (const auto& info : udpSCList)
		{
			line(str, "\tmPacketTCPFactoryManager->addUDPPacket(PACKET_TYPE::" + info.mPacketName + "); ");
		}
	}
	line(str, "};", false);

	writeFile(filePath + "GamePacketRegister.cpp", ANSIToUTF8(str.c_str(), true));
}

// CSPacket.h
void CodeNetPacket::generateCppCSPacketFileHeader(const PacketInfo& packetInfo, const string& filePath)
{
	const string& packetName = packetInfo.mPacketName;
	if (!startWith(packetName, "CS"))
	{
		return;
	}

	bool hasOptional = false;
	for (const auto& item : packetInfo.mMemberList)
	{
		if (item.mOptional)
		{
			hasOptional = true;
			break;
		}
	}

	myVector<string> generateCodes;
	generateCodes.push_back(packetInfo.mComment);
	generateCodes.push_back("class " + packetName + " : public PacketTCP");
	generateCodes.push_back("{");
	generateCodes.push_back("\tBASE(" + packetName + ", PacketTCP);");
	if (hasOptional)
	{
		generateCodes.push_back("public:");
		generateCodes.push_back("\tenum class Field : byte");
		generateCodes.push_back("\t{");
		FOR_I(packetInfo.mMemberList.size())
		{
			const auto& item = packetInfo.mMemberList[i];
			if (item.mOptional)
			{
				if (i >= 64)
				{
					ERROR("可选字段的下标不能超过63");
					break;
				}
				generateCodes.push_back("\t\t" + item.mMemberNameNoPrefix + " = " + intToString(i) + ",");
			}
		}
		generateCodes.push_back("\t};");
	}
	generateCodes.push_back("public:");
	generateCppPacketMemberDeclare(packetInfo.mMemberList, generateCodes);
	generateCodes.push_back("\tstatic string mPacketName;");
	generateCodes.push_back("public:");
	generateCodes.push_back("\t" + packetName + "()");
	generateCodes.push_back("\t{");
	generateCodes.push_back("\t\tmType = PACKET_TYPE::" + packetName + ";");
	generateCodes.push_back("\t\tmShowInfo = " + boolToString(packetInfo.mShowInfo) + ";");
	generateCodes.push_back("\t}");
	generateCodes.push_back("\tstatic const string& getStaticPacketName() { return mPacketName; }");
	generateCodes.push_back("\tconst string& getPacketName() override { return mPacketName; }");
	generateCppPacketReadWrite(packetInfo, generateCodes);
	generateCodes.push_back("\tvoid execute() override;");

	// CSPacket.h
	string headerFullPath = filePath + packetName + ".h";
	if (isFileExist(headerFullPath))
	{
		myVector<string> codeList;
		int lineStart = -1;
		if (!findCustomCode(headerFullPath, codeList, lineStart,
			[](const string& codeLine) { return codeLine == "// auto generate start"; },
			[](const string& codeLine) { return endWith(codeLine, "// auto generate end"); }))
		{
			return;
		}
		for (const string& line : generateCodes)
		{
			codeList.insert(++lineStart, line);
		}
		writeFile(headerFullPath, ANSIToUTF8(codeListToString(codeList).c_str(), true));
	}
	else
	{
		myVector<string> codeList;
		codeList.push_back("#pragma once");
		codeList.push_back("");
		codeList.push_back("#include \"PacketTCP.h\"");
		codeList.push_back("#include \"GamePacketDefine.h\"");
		codeList.push_back("");
		codeList.push_back("// auto generate start");
		codeList.addRange(generateCodes);
		codeList.push_back("\t// auto generate end");
		codeList.push_back("\tvoid debugInfo(MyString<1024>& buffer) override");
		codeList.push_back("\t{");
		codeList.push_back("\t\tdebug(buffer, \"\");");
		codeList.push_back("\t}");
		codeList.push_back("};");
		writeFile(headerFullPath, ANSIToUTF8(codeListToString(codeList).c_str(), true));
	}
}

void CodeNetPacket::generateCppStruct(const PacketStruct& structInfo, const string& filePath)
{
	const string& structName = structInfo.mStructName;

	// 是否需要移动构造,当有列表,或者字符串等可移动的变量时,就需要有移动构造
	bool hasMoveConstruct = false;
	bool hasOptional = false;
	for (const PacketMember& member : structInfo.mMemberList)
	{
		if (!hasMoveConstruct && (member.mTypeName == "string" || startWith(member.mTypeName, "Vector<")))
		{
			hasMoveConstruct = true;
		}
		if (!hasOptional && member.mOptional)
		{
			hasOptional = true;
		}
	}

	// PacketStruct.h
	const string headerFullPath = filePath + structName + ".h";
	myVector<string> headerCodeList;
	headerCodeList.push_back("#pragma once");
	headerCodeList.push_back("");
	headerCodeList.push_back("#include \"SerializableBitData.h\"");
	headerCodeList.push_back("");
	headerCodeList.push_back(structInfo.mComment);
	headerCodeList.push_back("class " + structName + " : public SerializableBitData");
	headerCodeList.push_back("{");
	headerCodeList.push_back("\tBASE(" + structName + ", SerializableBitData);");
	// 是否有可选字段
	if (hasOptional)
	{
		headerCodeList.push_back("public:");
		headerCodeList.push_back("\tenum class Field : byte");
		headerCodeList.push_back("\t{");
		FOR_I (structInfo.mMemberList.size())
		{
			const PacketMember& member = structInfo.mMemberList[i];
			if (member.mOptional)
			{
				if (i >= 64)
				{
					ERROR("可选字段的下标不能超过63");
					break;
				}
				headerCodeList.push_back("\t\t" + member.mMemberNameNoPrefix + " = " + intToString(i) + ",");
			}
		}
		headerCodeList.push_back("\t};");
	}
	headerCodeList.push_back("public:");
	generateCppPacketMemberDeclare(structInfo.mMemberList, headerCodeList);
	headerCodeList.push_back("public:");
	headerCodeList.push_back("\t" + structName + "() = default;");
	string constructParams;
	string constructMoveParams;
	const int memberCount = structInfo.mMemberList.size();
	// 当结构体成员数量不超过6时,提供带参构造和带可移动参的构造
	if (memberCount <= 6)
	{
		FOR_I(memberCount)
		{
			const PacketMember& member = structInfo.mMemberList[i];
			// 成员变量的命名格式都是以m开头,且后面的第一个字母是大写,所以需要去除m,将大写字母变为小写
			string tempParamName = member.mMemberName.substr(2);
			tempParamName.insert(0, 1, toLower(member.mMemberName[1]));
			if (member.mTypeName == "string" || 
				startWith(member.mTypeName, "Vector<") || 
				member.mTypeName == "Vector2" || 
				member.mTypeName == "Vector2UShort" || 
				member.mTypeName == "Vector2UInt" || 
				member.mTypeName == "Vector2Int" || 
				member.mTypeName == "Vector3" || 
				member.mTypeName == "Vector4")
			{
				constructParams += "const " + member.mTypeName + "& " + tempParamName;
			}
			else
			{
				constructParams += member.mTypeName + " " + tempParamName;
			}
			if (i != memberCount - 1)
			{
				constructParams += ", ";
			}
		}
		headerCodeList.push_back("\t" + structName + "(" + constructParams + ");");
		if (hasMoveConstruct)
		{
			FOR_I(memberCount)
			{
				const PacketMember& member = structInfo.mMemberList[i];
				// 成员变量的命名格式都是以m开头,且后面的第一个字母是大写,所以需要去除m,将大写字母变为小写
				string tempParamName = member.mMemberName.substr(2);
				tempParamName.insert(0, 1, toLower(member.mMemberName[1]));
				if (member.mTypeName == "string" || startWith(member.mTypeName, "Vector<"))
				{
					constructMoveParams += "" + member.mTypeName + "&& " + tempParamName;
				}
				else
				{
					constructMoveParams += member.mTypeName + " " + tempParamName;
				}
				if (i != memberCount - 1)
				{
					constructMoveParams += ", ";
				}
			}
			headerCodeList.push_back("\t" + structName + "(" + constructMoveParams + ");");
		}		
	}
	if (hasMoveConstruct)
	{
		headerCodeList.push_back("\t" + structName + "(const " + structName + "& other);");
		headerCodeList.push_back("\t" + structName + "(" + structName + "&& other);");
		headerCodeList.push_back("\t" + structName + "& operator=(" + structName + "&& other);");
	}
	headerCodeList.push_back("\t" + structName + "& operator=(const " + structName + "& other);");
	headerCodeList.push_back("\tbool readFromBuffer(SerializerBitRead* reader) override;");
	headerCodeList.push_back("\tbool writeToBuffer(SerializerBitWrite* serializer) const override;");
	headerCodeList.push_back("\tvoid resetProperty() override;");
	if (hasOptional)
	{
		headerCodeList.push_back("\tstatic constexpr ullong fullOptionFlag()");
		headerCodeList.push_back("\t{");
		headerCodeList.push_back("\t\tullong fieldFlag = 0;");
		for (const PacketMember& item : structInfo.mMemberList)
		{
			if (item.mOptional)
			{
				headerCodeList.push_back("\t\tsetBitOne(fieldFlag, (byte)Field::" + item.mMemberNameNoPrefix + ");");
			}
		}
		headerCodeList.push_back("\t\treturn fieldFlag;");
		headerCodeList.push_back("\t}");
	}
	
	headerCodeList.push_back("};");
	writeFile(headerFullPath, ANSIToUTF8(codeListToString(headerCodeList).c_str(), true));
	
	myVector<myVector<PacketMember>> memberGroupList;
	generateMemberGroup(structInfo.mMemberList, memberGroupList);

	// PacketStruct.cpp
	string sourceFullPath = filePath + structName + ".cpp";
	myVector<string> sourceCodeList;
	sourceCodeList.push_back("#include \"GameHeader.h\"");
	if (constructParams.length() > 0)
	{
		sourceCodeList.push_back("");
		sourceCodeList.push_back(structName + "::" + structName + "(" + constructParams + ") :");
		FOR_I(memberCount)
		{
			const PacketMember& member = structInfo.mMemberList[i];
			const string endComma = i != memberCount - 1 ? "," : "";
			// 成员变量的命名格式都是以m开头,且后面的第一个字母是大写,所以需要去除m,将大写字母变为小写
			string tempParamName = member.mMemberName.substr(2);
			tempParamName.insert(0, 1, toLower(member.mMemberName[1]));
			sourceCodeList.push_back("\t" + member.mMemberName + "(" + tempParamName + ")" + endComma);
		}
		sourceCodeList.push_back("{}");
	}
	if (constructMoveParams.length() > 0)
	{
		sourceCodeList.push_back("");
		sourceCodeList.push_back(structName + "::" + structName + "(" + constructMoveParams + ") :");
		FOR_I(memberCount)
		{
			const PacketMember& member = structInfo.mMemberList[i];
			const string endComma = i != memberCount - 1 ? "," : "";
			// 成员变量的命名格式都是以m开头,且后面的第一个字母是大写,所以需要去除m,将大写字母变为小写
			string tempParamName = member.mMemberName.substr(2);
			tempParamName.insert(0, 1, toLower(member.mMemberName[1]));
			if (member.mTypeName == "string" || startWith(member.mTypeName, "Vector<"))
			{
				sourceCodeList.push_back("\t" + member.mMemberName + "(move(" + tempParamName + "))" + endComma);
			}
			else
			{
				sourceCodeList.push_back("\t" + member.mMemberName + "(" + tempParamName + ")" + endComma);
			}
		}
		sourceCodeList.push_back("{}");
	}
	if (hasMoveConstruct)
	{
		sourceCodeList.push_back("");
		sourceCodeList.push_back(structName + "::" + structName + "(const " + structName + "& other) :");
		FOR_I(memberCount)
		{
			const PacketMember& member = structInfo.mMemberList[i];
			const string endComma = i != memberCount - 1 ? "," : "";
			sourceCodeList.push_back("\t" + member.mMemberName + "(other." + member.mMemberName + ")" + endComma);
		}
		sourceCodeList.push_back("{}");
		sourceCodeList.push_back("");
		sourceCodeList.push_back(structName + "::" + structName + "(" + structName + "&& other) :");
		FOR_I(memberCount)
		{
			const PacketMember& member = structInfo.mMemberList[i];
			const string endComma = i != memberCount - 1 ? "," : "";
			if (member.mTypeName == "string" || startWith(member.mTypeName, "Vector<"))
			{
				sourceCodeList.push_back("\t" + member.mMemberName + "(move(other." + member.mMemberName + "))" + endComma);
			}
			else
			{
				sourceCodeList.push_back("\t" + member.mMemberName + "(other." + member.mMemberName + ")" + endComma);
			}
		}
		sourceCodeList.push_back("{}");
		sourceCodeList.push_back("");
		sourceCodeList.push_back(structName + "& " + structName + "::operator=(" + structName + "&& other)");
		sourceCodeList.push_back("{");
		FOR_I(memberCount)
		{
			const PacketMember& member = structInfo.mMemberList[i];
			if (member.mTypeName == "string" || startWith(member.mTypeName, "Vector<"))
			{
				sourceCodeList.push_back("\t" + member.mMemberName + " = move(other." + member.mMemberName + ");");
			}
			else
			{
				sourceCodeList.push_back("\t" + member.mMemberName + " = other." + member.mMemberName + ";");
			}
		}
		sourceCodeList.push_back("\treturn *this;");
		sourceCodeList.push_back("}");
	}
	sourceCodeList.push_back("");
	sourceCodeList.push_back(structName + "& " + structName + "::operator=(const " + structName + "& other)");
	sourceCodeList.push_back("{");
	for (const PacketMember& member : structInfo.mMemberList)
	{
		sourceCodeList.push_back("\t" + member.mMemberName + " = other." + member.mMemberName + ";");
	}
	sourceCodeList.push_back("\treturn *this;");
	sourceCodeList.push_back("}");

	// readFromBuffer
	sourceCodeList.push_back("");
	sourceCodeList.push_back("bool " + structName + "::readFromBuffer(SerializerBitRead* reader)");
	sourceCodeList.push_back("{");
	if (hasOptional)
	{
		sourceCodeList.push_back("\t// 从缓冲区读取位标记");
		sourceCodeList.push_back("\tbool useFlag = false;");
		sourceCodeList.push_back("\treader->readBool(useFlag);");
		sourceCodeList.push_back("\tullong fieldFlag = FrameDefine::FULL_FIELD_FLAG;");
		sourceCodeList.push_back("\tif (useFlag)");
		sourceCodeList.push_back("\t{");
		sourceCodeList.push_back("\t\treader->readUnsigned(fieldFlag);");
		sourceCodeList.push_back("\t}");
		sourceCodeList.push_back("");
		sourceCodeList.push_back("\t// 再根据位标记读取字段数据");
	}
	sourceCodeList.push_back("\tbool success = true;");
	FOR_VECTOR(memberGroupList)
	{
		const myVector<PacketMember>& memberGroup = memberGroupList[i];
		if (memberGroup.size() == 1)
		{
			const PacketMember& item = memberGroup[0];
			// 可选字段需要特别判断一下
			if (item.mOptional)
			{
				sourceCodeList.push_back("\tif (hasBit(fieldFlag, (byte)Field::" + item.mMemberNameNoPrefix + "))");
				sourceCodeList.push_back("\t{");
				sourceCodeList.push_back("\t\t" + singleMemberReadLine(item.mMemberName, item.mTypeName, false));
				sourceCodeList.push_back("\t}");
			}
			else
			{
				sourceCodeList.push_back("\t" + singleMemberReadLine(item.mMemberName, item.mTypeName, false));
			}
		}
		else
		{
			myVector<string> nameList;
			string groupTypeName = expandMembersInGroup(memberGroup, nameList);
			myVector<string> list = multiMemberReadLine(nameList, groupTypeName, false);
			FOR_VECTOR(list)
			{
				sourceCodeList.push_back("\t" + list[i]);
			}
		}
	}
	sourceCodeList.push_back("\treturn success;");
	sourceCodeList.push_back("}");

	// writeToBuffer
	sourceCodeList.push_back("");
	sourceCodeList.push_back("bool " + structName + "::writeToBuffer(SerializerBitWrite* serializer) const");
	sourceCodeList.push_back("{");
	if (hasOptional)
	{
		sourceCodeList.push_back("\t// 将位标记写入到缓冲区");
		sourceCodeList.push_back("\t// 如果没有可选字段,则不使用位标记(在生成代码时会进行判断,所以不需要运行时再判断)");
		sourceCodeList.push_back("\t// 如果有可选字段,但是所有字段都需要同步,则也不使用位标记");
		sourceCodeList.push_back("\tbool useFlag = fullOptionFlag() != mFieldFlag;");
		sourceCodeList.push_back("\tserializer->writeBool(useFlag);");
		sourceCodeList.push_back("\tif (useFlag)");
		sourceCodeList.push_back("\t{");
		sourceCodeList.push_back("\t\tserializer->writeUnsigned(mFieldFlag);");
		sourceCodeList.push_back("\t}");
		sourceCodeList.push_back("\t");
		sourceCodeList.push_back("\t// 再根据位标记将字段数据写入缓冲区");
	}
	sourceCodeList.push_back("\tbool success = true;");
	FOR_VECTOR(memberGroupList)
	{
		const myVector<PacketMember>& memberGroup = memberGroupList[i];
		if (memberGroup.size() == 1)
		{
			const PacketMember& item = memberGroup[0];
			// 可选字段需要特别判断一下
			if (item.mOptional)
			{
				sourceCodeList.push_back("\tif (isFieldValid(Field::" + item.mMemberNameNoPrefix + "))");
				sourceCodeList.push_back("\t{");
				sourceCodeList.push_back("\t\t" + singleMemberWriteLine(item.mMemberName, item.mTypeName, false));
				sourceCodeList.push_back("\t}");
			}
			else
			{
				sourceCodeList.push_back("\t" + singleMemberWriteLine(item.mMemberName, item.mTypeName, false));
			}
		}
		else
		{
			myVector<string> nameList;
			string groupTypeName = expandMembersInGroup(memberGroup, nameList);
			myVector<string> list = multiMemberWriteLine(nameList, groupTypeName, false);
			FOR_VECTOR(list)
			{
				sourceCodeList.push_back("\t" + list[i]);
			}
		}
	}
	sourceCodeList.push_back("\treturn success;");
	sourceCodeList.push_back("}");
	
	// resetProperty
	sourceCodeList.push_back("");
	sourceCodeList.push_back("void " + structName + "::resetProperty()");
	sourceCodeList.push_back("{");
	sourceCodeList.push_back("\tbase::resetProperty();");
	for (const PacketMember& item : structInfo.mMemberList)
	{
		if (item.mTypeName == "Vector<bool>")
		{
			ERROR("不支持Vector<bool>类型,请使用Vector<byte>代替,packetType:" + structName);
		}
		if (item.mTypeName == "string" || 
			startWith(item.mTypeName, "Vector<") || 
			item.mTypeName == "Vector2" || 
			item.mTypeName == "Vector2UShort" || 
			item.mTypeName == "Vector2UInt" || 
			item.mTypeName == "Vector2Int" || 
			item.mTypeName == "Vector3" || 
			item.mTypeName == "Vector4")
		{
			sourceCodeList.push_back("\t" + item.mMemberName + ".clear();");
		}
		else if (item.mTypeName == "bool")
		{
			sourceCodeList.push_back("\t" + item.mMemberName + " = false;");
		}
		else if (isPodInteger(item.mTypeName))
		{
			sourceCodeList.push_back("\t" + item.mMemberName + " = 0;");
		}
		else if (item.mTypeName == "float")
		{
			sourceCodeList.push_back("\t" + item.mMemberName + " = 0.0f;");
		}
		else if (item.mTypeName == "double")
		{
			sourceCodeList.push_back("\t" + item.mMemberName + " = 0.0;");
		}
		else
		{
			ERROR("结构体中不支持自定义结构体:" + item.mTypeName);
		}
	}
	sourceCodeList.push_back("}");

	writeFile(sourceFullPath, ANSIToUTF8(codeListToString(sourceCodeList).c_str(), true));
}

string CodeNetPacket::generatePacketVersion(const myVector<PacketInfo>& packetList, const myVector<PacketStruct>& structInfoList)
{
	string allParamString;
	for (const PacketInfo& packetInfo : packetList)
	{
		allParamString += packetInfo.mPacketName;
		for (const PacketMember& member : packetInfo.mMemberList)
		{
			allParamString += member.mTypeName + member.mMemberName;
		}
	}
	for (const PacketStruct& structInfo : structInfoList)
	{
		allParamString += structInfo.mStructName;
		for (const PacketMember& member : structInfo.mMemberList)
		{
			allParamString += member.mTypeName + member.mMemberName;
		}
	}
	return generateStringMD5(allParamString);
}

// CSPacket.cpp
void CodeNetPacket::generateCppCSPacketFileSource(const PacketInfo& packetInfo, const string& filePath)
{
	const string& packetName = packetInfo.mPacketName;
	if (!startWith(packetName, "CS"))
	{
		return;
	}

	const string cppFullPath = filePath + packetName + ".cpp";
	if (isFileExist(cppFullPath))
	{
		myVector<string> codeList;
		int lineStart = -1;
		if (!findCustomCode(cppFullPath, codeList, lineStart,
			[](const string& codeLine) { return codeLine == "// auto generate start"; },
			[](const string& codeLine) { return endWith(codeLine, "// auto generate end"); }, false))
		{
			// 找不到就认为第一行是include,在第一行的下面插入
			lineStart = 0;
			codeList.insert(++lineStart, "");
			codeList.insert(++lineStart, "// auto generate start");
			codeList.insert(++lineStart, "string " + packetName + "::mPacketName = STR(" + packetName + ");");
			codeList.insert(++lineStart, "// auto generate end");
			writeFile(cppFullPath, ANSIToUTF8(codeListToString(codeList).c_str(), true));
			return;
		}
		codeList.insert(++lineStart, "string " + packetName + "::mPacketName = STR(" + packetName + ");");
		writeFile(cppFullPath, ANSIToUTF8(codeListToString(codeList).c_str(), true));
	}
	else
	{
		string source;
		line(source, "#include \"GameHeader.h\"");
		line(source, "");
		line(source, "// auto generate start");
		line(source, "string " + packetName + "::mPacketName = STR(" + packetName + ");");
		line(source, "// auto generate end");
		line(source, "");
		line(source, "void " + packetName + "::execute()");
		line(source, "{");
		line(source, "\tCharacterPlayer* player = getPlayer(mClient->getPlayerGUID());");
		line(source, "\tif (player == nullptr)");
		line(source, "\t{");
		line(source, "\t\treturn;");
		line(source, "\t}");
		line(source, "}", false);
		writeFile(cppFullPath, ANSIToUTF8(source.c_str(), true));
	}
}

void CodeNetPacket::generateCppPacketMemberDeclare(const myVector<PacketMember>& memberList, myVector<string>& generateCodes)
{
	for (const PacketMember& item : memberList)
	{
		string line;
		if (item.mTypeName == "byte" ||
			item.mTypeName == "char" ||
			item.mTypeName == "short" ||
			item.mTypeName == "ushort" ||
			item.mTypeName == "int" ||
			item.mTypeName == "uint" ||
			item.mTypeName == "llong" ||
			item.mTypeName == "ullong")
		{
			line = "\t" + item.mTypeName + " " + item.mMemberName + " = 0;";
		}
		else if (item.mTypeName == "float" ||
				 item.mTypeName == "double")
		{
			line = "\t" + item.mTypeName + " " + item.mMemberName + " = 0.0f;";
		}
		else if (item.mTypeName == "bool")
		{
			line = "\t" + item.mTypeName + " " + item.mMemberName + " = false;";
		}
		else
		{
			line = "\t" + item.mTypeName + " " + item.mMemberName + ";";
		}
		if (!item.mComment.empty())
		{
			appendWithAlign(line, "// " + item.mComment, 52);
		}
		generateCodes.push_back(line);
	}
}

string CodeNetPacket::singleMemberReadLine(const string& memberName, const string& memberType, bool supportCustom)
{
	if (memberType == "string")
	{
		return "success = success && reader->readString(" + memberName + ");";
	}
	else if (startWith(memberType, "Vector<"))
	{
		int lastPos;
		findSubstr(memberType, ">", &lastPos);
		const string elementType = memberType.substr(strlen("Vector<"), lastPos - strlen("Vector<"));
		if (elementType == "string")
		{
			return "success = success && reader->readStringList(" + memberName + ");";
		}
		else if (elementType == "bool")
		{
			ERROR("不支持bool的列表");
			return "";
		}
		else if (elementType == "char" || elementType == "short" || elementType == "int" || elementType == "llong")
		{
			return "success = success && reader->readSignedList(" + memberName + ");";
		}
		else if (elementType == "byte" || elementType == "ushort" || elementType == "uint" || elementType == "ullong")
		{
			return "success = success && reader->readUnsignedList(" + memberName + ");";
		}
		else if (elementType == "float")
		{
			return "success = success && reader->readFloatList(" + memberName + ");";
		}
		else if (elementType == "double")
		{
			return "success = success && reader->readDoubleList(" + memberName + ");";
		}
		else if (elementType == "Vector2")
		{
			return "success = success && reader->readVector2(" + memberName + ");";
		}
		else if (elementType == "Vector2UShort")
		{
			return "success = success && reader->readVector2UShort(" + memberName + ");";
		}
		else if (elementType == "Vector2UInt")
		{
			return "success = success && reader->readVector2UInt(" + memberName + ");";
		}
		else if (elementType == "Vector2Int")
		{
			return "success = success && reader->readVector2Int(" + memberName + ");";
		}
		else if (elementType == "Vector3")
		{
			return "success = success && reader->readVector3(" + memberName + ");";
		}
		else if (elementType == "Vector4")
		{
			return "success = success && reader->readVector4(" + memberName + ");";
		}
		else
		{
			if (supportCustom)
			{
				return "success = success && reader->readCustomList(" + memberName + ");";
			}
			else
			{
				ERROR("不支持自定义结构体:" + memberType);
			}
		}
	}
	else if (memberType == "bool")
	{
		return "success = success && reader->readBool(" + memberName + ");";
	}
	else if (memberType == "char" || memberType == "short" || memberType == "int" || memberType == "llong")
	{
		return "success = success && reader->readSigned(" + memberName + ");";
	}
	else if (memberType == "byte" || memberType == "ushort" || memberType == "uint" || memberType == "ullong")
	{
		return "success = success && reader->readUnsigned(" + memberName + ");";
	}
	else if (memberType == "float")
	{
		return "success = success && reader->readFloat(" + memberName + ");";
	}
	else if (memberType == "double")
	{
		return "success = success && reader->readDouble(" + memberName + ");";
	}
	else if (memberType == "Vector2")
	{
		return "success = success && reader->readVector2(" + memberName + ");";
	}
	else if (memberType == "Vector2UShort")
	{
		return "success = success && reader->readVector2UShort(" + memberName + ");";
	}
	else if (memberType == "Vector2UInt")
	{
		return "success = success && reader->readVector2UInt(" + memberName + ");";
	}
	else if (memberType == "Vector2Int")
	{
		return "success = success && reader->readVector2Int(" + memberName + ");";
	}
	else if (memberType == "Vector3")
	{
		return "success = success && reader->readVector3(" + memberName + ");";
	}
	else if (memberType == "Vector4")
	{
		return "success = success && reader->readVector4(" + memberName + ");";
	}
	if (supportCustom)
	{
		return "success = success && reader->readCustom(" + memberName + ");";
	}
	else
	{
		ERROR("不支持自定义结构体:" + memberType);
		return "";
	}
}

myVector<string> CodeNetPacket::multiMemberReadLine(const myVector<string>& memberNameList, const string& memberType, bool supportCustom)
{
	string members;
	FOR_VECTOR(memberNameList)
	{
		members += memberNameList[i];
		if (i < memberNameList.size() - 1)
		{
			members += ", ";
		}
	}

	myVector<string> list;
	if (memberType == "string")
	{
		FOR_VECTOR(memberNameList)
		{
			list.push_back("success = success && reader->readString(" + memberNameList[i] + ");");
		}
	}
	else if (startWith(memberType, "Vector<"))
	{
		int lastPos;
		findSubstr(memberType, ">", &lastPos);
		const string elementType = memberType.substr(strlen("Vector<"), lastPos - strlen("Vector<"));
		FOR_VECTOR(memberNameList)
		{
			if (elementType == "string")
			{
				list.push_back("success = success && reader->readStringList(" + memberNameList[i] + ");");
			}
			else if (elementType == "bool")
			{
				ERROR("不支持bool的列表");
			}
			else if (elementType == "char" || elementType == "short" || elementType == "int" || elementType == "llong")
			{
				list.push_back("success = success && reader->readSignedList(" + memberNameList[i] + ");");
			}
			else if (elementType == "byte" || elementType == "ushort" || elementType == "uint" || elementType == "ullong")
			{
				list.push_back("success = success && reader->readUnsignedList(" + memberNameList[i] + ");");
			}
			else if (elementType == "float")
			{
				list.push_back("success = success && reader->readFloatList(" + memberNameList[i] + ");");
			}
			else if (elementType == "double")
			{
				list.push_back("success = success && reader->readDoubleList(" + memberNameList[i] + ");");
			}
			else if (elementType == "Vector2")
			{
				list.push_back("success = success && reader->readVector2List(" + memberNameList[i] + ");");
			}
			else if (elementType == "Vector2UShort")
			{
				list.push_back("success = success && reader->readVector2UShortList(" + memberNameList[i] + ");");
			}
			else if (elementType == "Vector2UInt")
			{
				list.push_back("success = success && reader->readVector2UIntList(" + memberNameList[i] + ");");
			}
			else if (elementType == "Vector2Int")
			{
				list.push_back("success = success && reader->readVector2IntList(" + memberNameList[i] + ");");
			}
			else if (elementType == "Vector3")
			{
				list.push_back("success = success && reader->readVector3List(" + memberNameList[i] + ");");
			}
			else if (elementType == "Vector4")
			{
				list.push_back("success = success && reader->readVector4List(" + memberNameList[i] + ");");
			}
			else
			{
				if (supportCustom)
				{
					list.push_back("success = success && reader->readCustomList(" + memberNameList[i] + ");");
				}
				else
				{
					ERROR("不支持自定义结构体:" + memberType);
				}
			}
		}
	}
	else if (memberType == "bool")
	{
		FOR_VECTOR(memberNameList)
		{
			list.push_back("success = success && reader->readBool(" + memberNameList[i] + ");");
		}
	}
	else if (memberType == "char" || memberType == "short" || memberType == "int" || memberType == "llong")
	{
		list.push_back("success = success && reader->readSigned(" + members + ");");
	}
	else if (memberType == "byte" || memberType == "ushort" || memberType == "uint" || memberType == "ullong")
	{
		list.push_back("success = success && reader->readUnsigned(" + members + ");");
	}
	else if (memberType == "float")
	{
		list.push_back("success = success && reader->readFloat(" + members + ");");
	}
	else if (memberType == "double")
	{
		list.push_back("success = success && reader->readDouble(" + members + ");");
	}
	else
	{
		if (supportCustom)
		{
			FOR_VECTOR(memberNameList)
			{
				list.push_back("success = success && reader->readCustom(" + memberNameList[i] + ");");
			}
		}
		else
		{
			ERROR("不支持自定义结构体:" + memberType);
		}
	}
	return list;
}

myVector<string> CodeNetPacket::multiMemberWriteLine(const myVector<string>& memberNameList, const string& memberType, bool supportCustom)
{
	string members;
	FOR_VECTOR(memberNameList)
	{
		members += memberNameList[i];
		if (i < memberNameList.size() - 1)
		{
			members += ", ";
		}
	}

	myVector<string> list;
	if (memberType == "string")
	{
		FOR_VECTOR(memberNameList)
		{
			list.push_back("success = success && serializer->writeString(" + memberNameList[i] + ");");
		}
	}
	else if (startWith(memberType, "Vector<"))
	{
		int lastPos;
		findSubstr(memberType, ">", &lastPos);
		const string elementType = memberType.substr(strlen("Vector<"), lastPos - strlen("Vector<"));
		FOR_VECTOR(memberNameList)
		{
			if (elementType == "string")
			{
				list.push_back("success = success && serializer->writeStringList(" + memberNameList[i] + ");");
			}
			else if (elementType == "bool")
			{
				ERROR("不支持bool的列表");
			}
			else if (elementType == "char" || elementType == "short" || elementType == "int" || elementType == "llong")
			{
				list.push_back("success = success && serializer->writeSignedList(" + memberNameList[i] + ");");
			}
			else if (elementType == "byte" || elementType == "ushort" || elementType == "uint" || elementType == "ullong")
			{
				list.push_back("success = success && serializer->writeUnsignedList(" + memberNameList[i] + ");");
			}
			else if (elementType == "float")
			{
				list.push_back("success = success && serializer->writeFloatList(" + memberNameList[i] + ");");
			}
			else if (elementType == "double")
			{
				list.push_back("success = success && serializer->writeDoubleList(" + memberNameList[i] + ");");
			}
			else if (elementType == "Vector2")
			{
				list.push_back("success = success && serializer->writeVector2List(" + memberNameList[i] + ");");
			}
			else if (elementType == "Vector2UShort")
			{
				list.push_back("success = success && serializer->writeVector2UShortList(" + memberNameList[i] + ");");
			}
			else if (elementType == "Vector2UInt")
			{
				list.push_back("success = success && serializer->writeVector2UIntList(" + memberNameList[i] + ");");
			}
			else if (elementType == "Vector2Int")
			{
				list.push_back("success = success && serializer->writeVector2IntList(" + memberNameList[i] + ");");
			}
			else if (elementType == "Vector3")
			{
				list.push_back("success = success && serializer->writeVector3List(" + memberNameList[i] + ");");
			}
			else if (elementType == "Vector4")
			{
				list.push_back("success = success && serializer->writeVector4List(" + memberNameList[i] + ");");
			}
			else
			{
				if (supportCustom)
				{
					list.push_back("success = success && serializer->readCustomList(" + memberNameList[i] + ");");
				}
				else
				{
					ERROR("不支持自定义结构体:" + memberType);
				}
			}
		}
	}
	else if (memberType == "bool")
	{
		FOR_VECTOR(memberNameList)
		{
			list.push_back("success = success && serializer->writeBool(" + memberNameList[i] + ");");
		}
	}
	else if (memberType == "char" || memberType == "short" || memberType == "int" || memberType == "llong")
	{
		list.push_back("success = success && serializer->writeSigned(" + members + ");");
	}
	else if (memberType == "byte" || memberType == "ushort" || memberType == "uint" || memberType == "ullong")
	{
		list.push_back("success = success && serializer->writeUnsigned(" + members + ");");
	}
	else if (memberType == "float")
	{
		list.push_back("success = success && serializer->writeFloat(" + members + ");");
	}
	else if (memberType == "double")
	{
		list.push_back("success = success && serializer->writeDouble(" + members + ");");
	}
	else
	{
		if (supportCustom)
		{
			FOR_VECTOR(memberNameList)
			{
				list.push_back("success = success && serializer->writeCustom(" + memberNameList[i] + ");");
			}
		}
		else
		{
			ERROR("不支持自定义结构体:" + memberType);
		}
	}
	return list;
}

string CodeNetPacket::singleMemberReadLineCSharp(const string& memberName, const string& memberType)
{
	if (memberType == "string")
	{
		return "success = success && reader.readString(out " + memberName + ".mValue);";
	}
	else if (startWith(memberType, "Vector<"))
	{
		return "success = success && reader.readList(" + memberName + ".mValue);";
	}
	else
	{
		return "success = success && reader.read(out " + memberName + ".mValue);";
	}
}

string CodeNetPacket::singleMemberWriteLineCSharp(const string& memberName, const string& memberType)
{
	if (memberType == "string")
	{
		return "writer.writeString(" + memberName + ");";
	}
	else if (startWith(memberType, "Vector<"))
	{
		return "writer.writeList(" + memberName + ");";
	}
	if (memberType == "bool" || memberType == "char" || memberType == "byte" || memberType == "sbyte" || memberType == "short" || 
		memberType == "ushort" || memberType == "int" || memberType == "uint" || memberType == "llong" || memberType == "ullong" || 
		memberType == "float" || memberType == "double" || memberType == "Vector2" || memberType == "Vector2UShort" || 
		memberType == "Vector2Int" || memberType == "Vector2UInt" || memberType == "Vector3" || memberType == "Vector3Int" || memberType == "Vector4")
	{
		return "writer.write(" + memberName + ");";
	}
	ERROR("不支持的类型:" + memberType);
	return "";
}

myVector<string> CodeNetPacket::multiMemberReadLineCSharp(const myVector<string>& memberNameList, const string& memberType, bool supportCustom)
{
	myVector<string> list;
	if (memberType == "string")
	{
		FOR_VECTOR(memberNameList)
		{
			list.push_back("success = success && reader.readString(out " + memberNameList[i] + ");");
		}
		return list;
	}
	if (memberType == "bool")
	{
		FOR_VECTOR(memberNameList)
		{
			list.push_back("success = success && reader.read(out " + memberNameList[i] + ");");
		}
		return list;
	}
	if (startWith(memberType, "Vector<"))
	{
		int lastPos;
		findSubstr(memberType, ">", &lastPos);
		const string elementType = memberType.substr(strlen("Vector<"), lastPos - strlen("Vector<"));
		FOR_VECTOR(memberNameList)
		{
			if (elementType == "string" || elementType == "char" || elementType == "short" || elementType == "int" || elementType == "llong" || 
				elementType == "byte" || elementType == "ushort" || elementType == "uint" || elementType == "ullong" || elementType == "float" || 
				elementType == "double" || elementType == "Vector2" || elementType == "Vector2UShort" || elementType == "Vector2Int" || elementType == "Vector2UInt" ||
				elementType == "Vector3" || elementType == "Vector3Int" || elementType == "Vector4")
			{
				list.push_back("success = success && reader.readList(" + memberNameList[i] + ");");
			}
			else if (elementType == "bool")
			{
				ERROR("不支持bool的列表");
			}
			else
			{
				if (supportCustom)
				{
					list.push_back("success = success && reader.readCustomList(" + memberNameList[i] + ");");
				}
				else
				{
					ERROR("不支持自定义结构体:" + memberType);
				}
			}
		}
		return list;
	}
	if (memberType == "byte" || memberType == "sbyte" || memberType == "short" || memberType == "ushort" || memberType == "int" || 
		memberType == "uint" || memberType == "llong" || memberType == "ullong" || memberType == "float" || memberType == "double" || 
		memberType == "Vector2" || memberType == "Vector2UShort" || memberType == "Vector2Int" || memberType == "Vector2UInt" || memberType == "Vector3" ||
		memberType == "Vector3Int" || memberType == "Vector4")
	{
		const int memberCount = memberNameList.size();
		if (memberCount <= 4)
		{
			string members;
			FOR_I(memberCount)
			{
				members += "out " + memberNameList[i];
				if (i < memberCount - 1)
				{
					members += ", ";
				}
			}
			list.push_back("success = success && reader.read(" + members + ");");
		}
		else
		{
			string tempVarName = "values" + memberNameList[0].substr(1, memberNameList[0].find_first_of('.') - 1);
			string csharpType = cppTypeToCSharpType(memberType);
			list.push_back("Span<" + csharpType + "> " + tempVarName + " = stackalloc " + csharpType + "[" + intToString(memberCount) + "];");
			list.push_back("success = success && reader.read(ref " + tempVarName + ");");
			for (int i = 0; i < memberCount; ++i)
			{
				list.push_back(memberNameList[i] + " = " + tempVarName + "[" + intToString(i) + "];");
			}
		}
	}
	else
	{
		if (supportCustom)
		{
			FOR_VECTOR(memberNameList)
			{
				list.push_back("success = success && reader.readCustomList(" + memberNameList[i] + ");");
			}
		}
		else
		{
			ERROR("不支持自定义结构体:" + memberType);
		}
	}
	return list;
}

myVector<string> CodeNetPacket::multiMemberWriteLineCSharp(const myVector<string>& memberNameList, const string& memberType, bool supportCustom)
{
	myVector<string> list;
	if (memberType == "string")
	{
		FOR_VECTOR(memberNameList)
		{
			list.push_back("writer.writeString(out " + memberNameList[i] + ");");
		}
		return list;
	}
	if (memberType == "bool")
	{
		FOR_VECTOR(memberNameList)
		{
			list.push_back("writer.write(out " + memberNameList[i] + ");");
		}
		return list;
	}
	if (startWith(memberType, "Vector<"))
	{
		int lastPos;
		findSubstr(memberType, ">", &lastPos);
		const string elementType = memberType.substr(strlen("Vector<"), lastPos - strlen("Vector<"));
		FOR_VECTOR(memberNameList)
		{
			if (elementType == "string" || elementType == "char" || elementType == "short" || elementType == "int" || elementType == "llong" ||
				elementType == "byte" || elementType == "ushort" || elementType == "uint" || elementType == "ullong" || elementType == "float" ||
				elementType == "double" || elementType == "Vector2" || elementType == "Vector2UShort" || elementType == "Vector2Int" || elementType == "Vector2UInt" ||
				elementType == "Vector3" || elementType == "Vector4")
			{
				list.push_back("writer.writeList(" + memberNameList[i] + ");");
			}
			else if (elementType == "bool")
			{
				ERROR("不支持bool的列表");
			}
			else
			{
				if (supportCustom)
				{
					list.push_back("writer.writeCustomList(" + memberNameList[i] + ");");
				}
				else
				{
					ERROR("不支持自定义结构体:" + memberType);
				}
			}
		}
		return list;
	}
	if (memberType == "byte" || memberType == "sbyte" || memberType == "short" || memberType == "ushort" || memberType == "int" || 
		memberType == "uint" || memberType == "llong" || memberType == "ullong" || memberType == "float" || memberType == "double")
	{
		string members;
		FOR_VECTOR(memberNameList)
		{
			members += memberNameList[i];
			if (i < memberNameList.size() - 1)
			{
				members += ", ";
			}
		}
		list.push_back("writer.write(stackalloc " + cppTypeToCSharpType(memberType) + "[" + intToString(memberNameList.size()) + "]{ " + members + " });");
	}
	else
	{
		if (supportCustom)
		{
			FOR_VECTOR(memberNameList)
			{
				list.push_back("writer.writeCustom(" + memberNameList[i] + ");");
			}
		}
		else
		{
			ERROR("不支持自定义结构体:" + memberType);
		}
	}
	return list;
}

string CodeNetPacket::singleMemberWriteLine(const string& memberName, const string& memberType, bool supportCustom)
{
	if (memberType == "string")
	{
		return "success = success && serializer->writeString(" + memberName + ");";
	}
	else if (startWith(memberType, "Vector<"))
	{
		int lastPos;
		findSubstr(memberType, ">", &lastPos);
		const string elementType = memberType.substr(strlen("Vector<"), lastPos - strlen("Vector<"));
		if (elementType == "string")
		{
			return "success = success && serializer->writeStringList(" + memberName + ");";
		}
		else if (elementType == "bool")
		{
			ERROR("不支持bool的列表");
			return "";
		}
		else if (elementType == "char" || elementType == "short" || elementType == "int" || elementType == "llong")
		{
			return "success = success && serializer->writeSignedList(" + memberName + ");";
		}
		else if (elementType == "byte" || elementType == "ushort" || elementType == "uint" || elementType == "ullong")
		{
			return "success = success && serializer->writeUnsignedList(" + memberName + ");";
		}
		else if (elementType == "float")
		{
			return "success = success && serializer->writeFloatList(" + memberName + ");";
		}
		else if (elementType == "double")
		{
			return "success = success && serializer->writeDoubleList(" + memberName + ");";
		}
		else if (elementType == "Vector2")
		{
			return "success = success && serializer->writeVector2List(" + memberName + ");";
		}
		else if (elementType == "Vector2UInt")
		{
			return "success = success && serializer->writeVector2UIntList(" + memberName + ");";
		}
		else if (elementType == "Vector2Int")
		{
			return "success = success && serializer->writeVector2IntList(" + memberName + ");";
		}
		else if (elementType == "Vector2UShort")
		{
			return "success = success && serializer->writeVector2UShortList(" + memberName + ");";
		}
		else if (elementType == "Vector3")
		{
			return "success = success && serializer->writeVector3List(" + memberName + ");";
		}
		else if (elementType == "Vector4")
		{
			return "success = success && serializer->writeVector4List(" + memberName + ");";
		}
		else
		{
			if (supportCustom)
			{
				return "success = success && serializer->writeCustomList(" + memberName + ");";
			}
			else
			{
				ERROR("不支持自定义结构体:" + memberName);
			}
		}
	}
	else if (memberType == "bool")
	{
		return "success = success && serializer->writeBool(" + memberName + ");";
	}
	else if (memberType == "char" || memberType == "short" || memberType == "int" || memberType == "llong")
	{
		return "success = success && serializer->writeSigned(" + memberName + ");";
	}
	else if (memberType == "byte" || memberType == "ushort" || memberType == "uint" || memberType == "ullong")
	{
		return "success = success && serializer->writeUnsigned(" + memberName + ");";
	}
	else if (memberType == "float")
	{
		return "success = success && serializer->writeFloat(" + memberName + ");";
	}
	else if (memberType == "double")
	{
		return "success = success && serializer->writeDouble(" + memberName + ");";
	}
	else if (memberType == "Vector2")
	{
		return "success = success && serializer->writeVector2(" + memberName + ");";
	}
	else if (memberType == "Vector2UInt")
	{
		return "success = success && serializer->writeVector2UInt(" + memberName + ");";
	}
	else if (memberType == "Vector2Int")
	{
		return "success = success && serializer->writeVector2Int(" + memberName + ");";
	}
	else if (memberType == "Vector2UShort")
	{
		return "success = success && serializer->writeVector2UShort(" + memberName + ");";
	}
	else if (memberType == "Vector3")
	{
		return "success = success && serializer->writeVector3(" + memberName + ");";
	}
	else if (memberType == "Vector4")
	{
		return "success = success && serializer->writeVector4(" + memberName + ");";
	}
	if (supportCustom)
	{
		return "success = success && serializer->writeCustom(" + memberName + ");";
	}
	else
	{
		ERROR("不支持自定义结构体:" + memberName);
		return "";
	}
}

bool CodeNetPacket::isSameType(const string& sourceType, const string& curType)
{
	// string和bool类型不合并
	if (sourceType == "string" || sourceType == "bool" || curType == "string" || curType == "bool")
	{
		return false;
	}
	if (sourceType == curType)
	{
		return true;
	}
	if (sourceType == "float")
	{
		return curType == "Vector2" || curType == "Vector3" || curType == "Vector4";
	}
	if (sourceType == "ushort")
	{
		return curType == "Vector2UShort";
	}
	if (sourceType == "int")
	{
		return curType == "Vector2Int" || curType == "Vector3Int";
	}
	if (sourceType == "uint")
	{
		return curType == "Vector2UInt" || curType == "Vector3UInt";
	}
	return false;
}

string CodeNetPacket::toPODType(const string& type)
{
	if (type == "Vector2" || type == "Vector3" || type == "Vector4")
	{
		return "float";
	}
	if (type == "Vector2UShort")
	{
		return "ushort";
	}
	if (type == "Vector2Int" || type == "Vector3Int")
	{
		return "int";
	}
	if (type == "Vector2UInt" || type == "Vector3UInt")
	{
		return "uint";
	}
	return type;
}

bool CodeNetPacket::isCustomStructType(const string& type)
{
	return startWith(type, "NetStruct");
}

void CodeNetPacket::generateMemberGroup(const myVector<PacketMember>& memberList, myVector<myVector<PacketMember>>& memberNameList)
{
	myVector<PacketMember> sameTypeMemberList;
	FOR_VECTOR(memberList)
	{
		const PacketMember& item = memberList[i];
		if (i > 0)
		{
			const PacketMember& lastItem = memberList[i - 1];
			// 如果上一个成员变量与当前的类型不一致,或者当前是一个optional变量,则将之前的变量写入,自定义的结构体类型的变量也不会归到一组
			if (item.mOptional || lastItem.mOptional || !isSameType(toPODType(item.mTypeName), toPODType(lastItem.mTypeName)) || isCustomStructType(item.mTypeName))
			{
				memberNameList.push_back(sameTypeMemberList);
				sameTypeMemberList.clear();
			}
			// 继续向后遍历
			sameTypeMemberList.push_back(item);
		}
		// 特殊判断第0个元素,因为没有上一个可以做比较
		else
		{
			if (item.mOptional)
			{
				memberNameList.push_back(myVector<PacketMember>{ item });
			}
			else
			{
				sameTypeMemberList.push_back(item);
			}
		}
	}
	if (sameTypeMemberList.size() > 0)
	{
		memberNameList.push_back(sameTypeMemberList);
	}
}

string CodeNetPacket::expandMembersInGroup(const myVector<PacketMember>& memberList, myVector<string>& memberNameList)
{
	if (memberList.size() == 0)
	{
		return "";
	}
	FOR_VECTOR(memberList)
	{
		const string& typeName = memberList[i].mTypeName;
		const string& memberName = memberList[i].mMemberName;
		if (typeName == "Vector2" || typeName == "Vector2UShort" || typeName == "Vector2Int" || typeName == "Vector2UInt")
		{
			memberNameList.push_back(memberName + ".x");
			memberNameList.push_back(memberName + ".y");
		}
		else if (typeName == "Vector3" || typeName == "Vector3Int")
		{
			memberNameList.push_back(memberName + ".x");
			memberNameList.push_back(memberName + ".y");
			memberNameList.push_back(memberName + ".z");
		}
		else if (typeName == "Vector4")
		{
			memberNameList.push_back(memberName + ".x");
			memberNameList.push_back(memberName + ".y");
			memberNameList.push_back(memberName + ".z");
			memberNameList.push_back(memberName + ".w");
		}
		else
		{
			memberNameList.push_back(memberName);
		}
	}
	return toPODType(memberList[0].mTypeName);
}

string CodeNetPacket::expandMembersInGroupCSharp(const myVector<PacketMember>& memberList, myVector<string>& memberNameList, bool supportSimplify)
{
	if (memberList.size() == 0)
	{
		return "";
	}
	FOR_VECTOR(memberList)
	{
		const string& typeName = memberList[i].mTypeName;
		const string& memberName = memberList[i].mMemberName;
		if (typeName == "Vector2" || typeName == "Vector2UShort" || typeName == "Vector2Int" || typeName == "Vector2UInt")
		{
			memberNameList.push_back(memberName + ".mValue.x");
			memberNameList.push_back(memberName + ".mValue.y");
		}
		else if (typeName == "Vector3" || typeName == "Vector3Int")
		{
			memberNameList.push_back(memberName + ".mValue.x");
			memberNameList.push_back(memberName + ".mValue.y");
			memberNameList.push_back(memberName + ".mValue.z");
		}
		else if (typeName == "Vector4")
		{
			memberNameList.push_back(memberName + ".mValue.x");
			memberNameList.push_back(memberName + ".mValue.y");
			memberNameList.push_back(memberName + ".mValue.z");
			memberNameList.push_back(memberName + ".mValue.w");
		}
		else
		{
			if (!supportSimplify)
			{
				memberNameList.push_back(memberName + ".mValue");
			}
			else
			{
				memberNameList.push_back(memberName);
			}
		}
	}
	return toPODType(memberList[0].mTypeName);
}

void CodeNetPacket::generateCppPacketReadWrite(const PacketInfo& packetInfo, myVector<string>& generateCodes)
{
	if (packetInfo.mMemberList.size() > 0)
	{
		myVector<myVector<PacketMember>> memberGroupList;
		generateMemberGroup(packetInfo.mMemberList, memberGroupList);

		// readFromBuffer
		generateCodes.push_back("\tbool readFromBuffer(SerializerBitRead* reader) override");
		generateCodes.push_back("\t{");
		generateCodes.push_back("\t\tbool success = true;");
		FOR_VECTOR(memberGroupList)
		{
			const myVector<PacketMember>& memberList = memberGroupList[i];
			// 该组中只有一个成员,才有可能是optional
			if (memberList.size() == 1)
			{
				const PacketMember& item = memberList[0];
				if (item.mOptional)
				{
					generateCodes.push_back("\t\tif (isFieldValid(Field::" + item.mMemberNameNoPrefix + "))");
					generateCodes.push_back("\t\t{");
					generateCodes.push_back("\t\t\t" + singleMemberReadLine(item.mMemberName, item.mTypeName, true));
					generateCodes.push_back("\t\t}");
				}
				else
				{
					generateCodes.push_back("\t\t" + singleMemberReadLine(item.mMemberName, item.mTypeName, true));
				}
			}
			else
			{
				myVector<string> nameList;
				string groupTypeName = expandMembersInGroup(memberList, nameList);
				myVector<string> list = multiMemberReadLine(nameList, groupTypeName, true);
				FOR_VECTOR(list)
				{
					generateCodes.push_back("\t\t" + list[i]);
				}
			}
		}
		generateCodes.push_back("\t\treturn success;");
		generateCodes.push_back("\t}");

		// writeToBuffer
		generateCodes.push_back("\tbool writeToBuffer(SerializerBitWrite* serializer) const override");
		generateCodes.push_back("\t{");
		generateCodes.push_back("\t\tbool success = true;");
		FOR_VECTOR(memberGroupList)
		{
			const myVector<PacketMember>& memberList = memberGroupList[i];
			// 该组中只有一个成员,才有可能是optional
			if (memberList.size() == 1)
			{
				const PacketMember& item = memberList[0];
				if (item.mOptional)
				{
					generateCodes.push_back("\t\tif (isFieldValid(Field::" + item.mMemberNameNoPrefix + "))");
					generateCodes.push_back("\t\t{");
					generateCodes.push_back("\t\t\t" + singleMemberWriteLine(item.mMemberName, item.mTypeName, true));
					generateCodes.push_back("\t\t}");
				}
				else
				{
					generateCodes.push_back("\t\t" + singleMemberWriteLine(item.mMemberName, item.mTypeName, true));
				}
			}
			else
			{
				myVector<string> nameList;
				string groupTypeName = expandMembersInGroup(memberList, nameList);
				myVector<string> list = multiMemberWriteLine(nameList, groupTypeName, true);
				FOR_VECTOR(list)
				{
					generateCodes.push_back("\t\t" + list[i]);
				}
			}
		}
		generateCodes.push_back("\t\treturn success;");
		generateCodes.push_back("\t}");

		// resetProperty
		generateCodes.push_back("\tvoid resetProperty() override");
		generateCodes.push_back("\t{");
		generateCodes.push_back("\t\tbase::resetProperty();");
		int startLineCount = generateCodes.size();
		for (const PacketMember& item : packetInfo.mMemberList)
		{
			if (item.mTypeName == "string" || 
				startWith(item.mTypeName, "Vector<") || 
				item.mTypeName == "Vector2" || 
				item.mTypeName == "Vector2UShort" || 
				item.mTypeName == "Vector2UInt" || 
				item.mTypeName == "Vector2Int" || 
				item.mTypeName == "Vector3" || 
				item.mTypeName == "Vector4")
			{
				generateCodes.push_back("\t\t" + item.mMemberName + ".clear();");
			}
			else if (item.mTypeName == "bool")
			{
				generateCodes.push_back("\t\t" + item.mMemberName + " = false;");
			}
			else if (item.mTypeName == "float" || item.mTypeName == "double")
			{
				generateCodes.push_back("\t\t" + item.mMemberName + " = 0.0f;");
			}
			else if (isPodInteger(item.mTypeName))
			{
				generateCodes.push_back("\t\t" + item.mMemberName + " = 0;");
			}
			else
			{
				generateCodes.push_back("\t\t" + item.mMemberName + ".resetProperty();");
			}
		}
		generateCodes.push_back("\t}");
	}
	else
	{
		generateCodes.push_back("\tbool readFromBuffer(SerializerBitRead* reader) override { return true; }");
		generateCodes.push_back("\tbool writeToBuffer(SerializerBitWrite* serializer) const override { return true; }");
		generateCodes.push_back("\tvoid resetProperty() override");
		generateCodes.push_back("\t{");
		generateCodes.push_back("\t\tbase::resetProperty();");
		generateCodes.push_back("\t}");
	}
}

// SCPacket.h文件
void CodeNetPacket::generateCppSCPacketFileHeader(const PacketInfo& packetInfo, const string& filePath)
{
	const string& packetName = packetInfo.mPacketName;
	if (!startWith(packetName, "SC"))
	{
		return;
	}

	bool hasOptional = false;
	for (const auto& item : packetInfo.mMemberList)
	{
		if (item.mOptional)
		{
			hasOptional = true;
			break;
		}
	}

	myVector<string> generateCodes;
	generateCodes.push_back(packetInfo.mComment);
	generateCodes.push_back("class " + packetName + " : public PacketTCP");
	generateCodes.push_back("{");
	generateCodes.push_back("\tBASE(" + packetName + ", PacketTCP);");
	if (hasOptional)
	{
		generateCodes.push_back("public:");
		generateCodes.push_back("\tenum class Field : byte");
		generateCodes.push_back("\t{");
		FOR_I(packetInfo.mMemberList.size())
		{
			const auto& item = packetInfo.mMemberList[i];
			if (item.mOptional)
			{
				if (i >= 64)
				{
					ERROR("可选字段的下标不能超过63");
					break;
				}
				generateCodes.push_back("\t\t" + item.mMemberNameNoPrefix + " = " + intToString(i) + ",");
			}
		}
		generateCodes.push_back("\t};");
	}
	generateCodes.push_back("public:");
	generateCppPacketMemberDeclare(packetInfo.mMemberList, generateCodes);
	generateCodes.push_back("private:");
	generateCodes.push_back("\tstatic " + packetName + " mStaticObject;");
	generateCodes.push_back("\tstatic string mPacketName;");
	generateCodes.push_back("public:");
	generateCodes.push_back("\t" + packetName + "()");
	generateCodes.push_back("\t{");
	generateCodes.push_back("\t\tmType = PACKET_TYPE::" + packetName + ";");
	generateCodes.push_back("\t\tmShowInfo = " + boolToString(packetInfo.mShowInfo) + ";");
	generateCodes.push_back("\t}");
	generateCodes.push_back("\tstatic " + packetName + "& get()");
	generateCodes.push_back("\t{");
	generateCodes.push_back("\t\tmStaticObject.resetProperty();");
	generateCodes.push_back("\t\treturn mStaticObject;");
	generateCodes.push_back("\t}");
	generateCodes.push_back("\tconst string& getPacketName() override { return mPacketName; }");
	generateCodes.push_back("\tstatic const string& getStaticPacketName() { return mPacketName; }");
	generateCodes.push_back("\tstatic constexpr ushort getStaticType() { return PACKET_TYPE::" + packetName + "; }");
	if (packetInfo.mMemberList.size() > 0)
	{
		generateCodes.push_back("\tstatic constexpr bool hasMember() { return true; }");
	}
	else
	{
		generateCodes.push_back("\tstatic constexpr bool hasMember() { return false; }");
	}
	generateCppPacketReadWrite(packetInfo, generateCodes);

	string headerFullPath = filePath + packetName + ".h";
	if (isFileExist(headerFullPath))
	{
		myVector<string> codeList;
		int lineStart = -1;
		if (!findCustomCode(headerFullPath, codeList, lineStart,
			[](const string& codeLine) { return codeLine == "// auto generate start"; },
			[](const string& codeLine) { return endWith(codeLine, "// auto generate end"); }))
		{
			return;
		}
		for (const string& line : generateCodes)
		{
			codeList.insert(++lineStart, line);
		}
		writeFile(headerFullPath, ANSIToUTF8(codeListToString(codeList).c_str(), true));
	}
	else
	{
		myVector<string> codeList;
		codeList.push_back("#pragma once");
		codeList.push_back("");
		codeList.push_back("#include \"PacketTCP.h\"");
		codeList.push_back("#include \"GamePacketDefine.h\"");
		codeList.push_back("");
		codeList.push_back("// auto generate start");
		codeList.addRange(generateCodes);
		codeList.push_back("\t// auto generate end");
		codeList.push_back("\tvoid debugInfo(MyString<1024>& buffer) override");
		codeList.push_back("\t{");
		codeList.push_back("\t\tdebug(buffer, \"\");");
		codeList.push_back("\t}");
		codeList.push_back("\tstatic void send(CharacterPlayer* player);");
		codeList.push_back("};");
		writeFile(headerFullPath, ANSIToUTF8(codeListToString(codeList).c_str(), true));
	}
}

// SCPacket.cpp
void CodeNetPacket::generateCppSCPacketFileSource(const PacketInfo& packetInfo, const string& filePath)
{
	const string& packetName = packetInfo.mPacketName;
	if (!startWith(packetName, "SC"))
	{
		return;
	}
	string cppFullPath = filePath + packetName + ".cpp";
	if (isFileExist(cppFullPath))
	{
		myVector<string> codeList;
		int lineStart = -1;
		if (!findCustomCode(cppFullPath, codeList, lineStart,
			[](const string& codeLine) { return codeLine == "// auto generate start"; },
			[](const string& codeLine) { return endWith(codeLine, "// auto generate end"); }))
		{
			return;
		}
		codeList.insert(++lineStart, packetName + " " + packetName + "::mStaticObject;");
		codeList.insert(++lineStart, "string " + packetName + "::mPacketName = STR(" + packetName + ");");
		writeFile(cppFullPath, ANSIToUTF8(codeListToString(codeList).c_str(), true));
	}
	else
	{
		myVector<string> codeList;
		codeList.push_back("#include \"GameHeader.h\"");
		codeList.push_back("");
		codeList.push_back("// auto generate start");
		codeList.push_back(packetName + " " + packetName + "::mStaticObject;");
		codeList.push_back("string " + packetName + "::mPacketName = STR(" + packetName + ");");
		codeList.push_back("// auto generate end");
		codeList.push_back("void " + packetName + "::send(CharacterPlayer* player)");
		codeList.push_back("{");
		codeList.push_back("\t" + packetName + "& packet = get();");
		codeList.push_back("\tsendPacketTCP(&packet, player->getClient());");
		codeList.push_back("}");
		writeFile(cppFullPath, ANSIToUTF8(codeListToString(codeList).c_str(), true));
	}
}

// PacketDefine.cs文件
void CodeNetPacket::generateCSharpPacketDefineFile(const myVector<PacketInfo>& packetList, const string& filePath)
{
	string str;
	line(str, "using System;");
	
	line(str, "");
	line(str, "public class PACKET_TYPE");
	line(str, "{");
	line(str, "\tpublic static ushort MIN = 0;");
	line(str, "");
	int csPacketNumber = 10000;
	for (const auto& item : packetList)
	{
		if (startWith(item.mPacketName, "CS"))
		{
			line(str, "\tpublic static ushort " + packetNameToUpper(item.mPacketName) + " = " + intToString(++csPacketNumber) + ";");
		}
	}
	line(str, "");
	int scPacketNumber = 20000;
	for (const auto& item : packetList)
	{
		if (startWith(item.mPacketName, "SC"))
		{
			line(str, "\tpublic static ushort " + packetNameToUpper(item.mPacketName) + " = " + intToString(++scPacketNumber) + ";");
		}
	}
	line(str, "}", false);

	writeFile(filePath + "PacketDefine.cs", ANSIToUTF8(str.c_str(), true));
}

// PacketRegister.cs文件
void CodeNetPacket::generateCSharpPacketRegisteFile(const myVector<PacketInfo>& packetList, const myVector<PacketStruct>& structInfoList, const string& filePath)
{
	string str;
	line(str, "using System;");
	line(str, "using static FrameBaseHotFix;");
	line(str, "");
	line(str, "public class PacketRegister");
	line(str, "{");
	line(str, "\tpublic static string PACKET_VERSION = \"" + generatePacketVersion(packetList, structInfoList) + "\";");
	line(str, "\tpublic static void registeAll()");
	line(str, "\t{");
	myVector<PacketInfo> udpCSList;
	uint packetCount = packetList.size();
	FOR_I(packetCount)
	{
		if (!startWith(packetList[i].mPacketName, "CS"))
		{
			continue;
		}
		line(str, "\t\tregistePacket<" + packetList[i].mPacketName + ">(PACKET_TYPE." + packetNameToUpper(packetList[i].mPacketName) + ");");
		if (packetList[i].mUDP)
		{
			udpCSList.push_back(packetList[i]);
		}
	}
	line(str, "");
	myVector<PacketInfo> udpSCList;
	FOR_I(packetCount)
	{
		if (!startWith(packetList[i].mPacketName, "SC"))
		{
			continue;
		}
		line(str, "\t\tregistePacket<" + packetList[i].mPacketName + ">(PACKET_TYPE." + packetNameToUpper(packetList[i].mPacketName) + ");");
		if (packetList[i].mUDP)
		{
			udpSCList.push_back(packetList[i]);
		}
	}
	if (udpCSList.size() > 0)
	{
		line(str, "");
		FOR_VECTOR(udpCSList)
		{
			line(str, "\t\tregisteUDP(PACKET_TYPE." + packetNameToUpper(udpCSList[i].mPacketName) + ", \"" + udpCSList[i].mPacketName + "\");");
		}
	}
	if (udpSCList.size() > 0)
	{
		line(str, "");
		FOR_VECTOR(udpSCList)
		{
			line(str, "\t\tregisteUDP(PACKET_TYPE." + packetNameToUpper(udpSCList[i].mPacketName) + ", \"" + udpSCList[i].mPacketName + "\");");
		}
	}
	line(str, "\t}");
	line(str, "\tprotected static void registePacket<T>(ushort type) where T : NetPacketBit");
	line(str, "\t{");
	line(str, "\t\tmNetPacketTypeManager.registePacket(typeof(T), type);");
	line(str, "\t}");
	line(str, "\tprotected static void registeUDP(ushort type, string packetName)");
	line(str, "\t{");
	line(str, "\t\tmNetPacketTypeManager.registeUDPPacketName(type, packetName);");
	line(str, "\t}");
	line(str, "}", false);

	writeFile(filePath + "PacketRegister.cs", ANSIToUTF8(str.c_str(), true));
}

// CSPacket.cs和SCPacket.cs文件
void CodeNetPacket::generateCSharpPacketFile(const PacketInfo& packetInfo, const string& csFileHotfixPath, const string& scFileHotfixPath)
{
	string packetName = packetInfo.mPacketName;
	if (!startWith(packetName, "CS") && !startWith(packetName, "SC"))
	{
		return;
	}

	myVector<string> usingList;
	myVector<string> customList;
	string fullPath;
	if (startWith(packetName, "CS"))
	{
		fullPath = csFileHotfixPath + packetName + ".cs";
	}
	else if (startWith(packetName, "SC"))
	{
		fullPath = scFileHotfixPath + packetName + ".cs";
	}

	myVector<myVector<PacketMember>> memberGroupList;
	generateMemberGroup(packetInfo.mMemberList, memberGroupList);

	myVector<string> generateCodes;
	generateCodes.push_back(packetInfo.mComment);
	generateCodes.push_back("public class " + packetName + " : NetPacketBit");
	generateCodes.push_back("{");
	for (const PacketMember& item : packetInfo.mMemberList)
	{
		generateCodes.push_back("\t" + cSharpMemberDeclareString(item));
	}
	if (packetInfo.mMemberList.size() > 0)
	{
		// init
		generateCodes.push_back("\tpublic " + packetName + "()");
		generateCodes.push_back("\t{");
		for (const PacketMember& item : packetInfo.mMemberList)
		{
			generateCodes.push_back("\t\taddParam(" + item.mMemberName + ", " + (item.mOptional ? "true" : "false") + ");");
		}
		generateCodes.push_back("\t}");

		// get
		if (startWith(packetName, "CS"))
		{
			generateCodes.push_back("\tpublic static " + packetName + " get() { return PACKET<" + packetName + ">(); }");
		}

		// read
		generateCodes.push_back("\tpublic override bool read(SerializerBitRead reader, ulong fieldFlag)");
		generateCodes.push_back("\t{");
		generateCodes.push_back("\t\tbool success = true;");
		FOR_VECTOR(memberGroupList)
		{
			const myVector<PacketMember>& memberGroup = memberGroupList[i];
			if (memberGroup.size() == 1)
			{
				const PacketMember& item = memberGroup[0];
				// 可选字段需要特别判断一下
				if (item.mOptional)
				{
					generateCodes.push_back("\t\tif (hasBit(fieldFlag, " + intToString(item.mIndex) + "))");
					generateCodes.push_back("\t\t{");
					generateCodes.push_back("\t\t\tsuccess = success && " + item.mMemberName + ".read(reader);");
					generateCodes.push_back("\t\t}");
				}
				else
				{
					generateCodes.push_back("\t\tsuccess = success && " + item.mMemberName + ".read(reader);");
				}
			}
			else
			{
				myVector<string> nameList;
				string groupTypeName = expandMembersInGroupCSharp(memberGroup, nameList, false);
				myVector<string> list = multiMemberReadLineCSharp(nameList, groupTypeName, true);
				FOR_VECTOR(list)
				{
					generateCodes.push_back("\t\t" + list[i]);
				}
			}
		}
		generateCodes.push_back("\t\treturn success;");
		generateCodes.push_back("\t}");

		// write
		generateCodes.push_back("\tpublic override void write(SerializerBitWrite writer, out ulong fieldFlag)");
		generateCodes.push_back("\t{");
		generateCodes.push_back("\t\tbase.write(writer, out fieldFlag);");
		FOR_VECTOR(memberGroupList)
		{
			const myVector<PacketMember>& memberGroup = memberGroupList[i];
			if (memberGroup.size() == 1)
			{
				const PacketMember& item = memberGroup[0];
				// 可选字段需要特别判断一下
				if (item.mOptional)
				{
					generateCodes.push_back("\t\tif (" + item.mMemberName + ".mValid)");
					generateCodes.push_back("\t\t{");
					generateCodes.push_back("\t\t\tsetBitOne(ref fieldFlag, " + intToString(item.mIndex) + ");");
					generateCodes.push_back("\t\t\t" + item.mMemberName + ".write(writer);");
					generateCodes.push_back("\t\t}");
				}
				else
				{
					generateCodes.push_back("\t\t" + item.mMemberName + ".write(writer);");
				}
			}
			else
			{
				myVector<string> nameList;
				string groupTypeName = expandMembersInGroupCSharp(memberGroup, nameList, true);
				myVector<string> list = multiMemberWriteLineCSharp(nameList, groupTypeName, true);
				FOR_VECTOR(list)
				{
					generateCodes.push_back("\t\t" + list[i]);
				}
			}
		}
		generateCodes.push_back("\t}");
	}
	else
	{
		// get
		if (startWith(packetName, "CS"))
		{
			generateCodes.push_back("\tpublic static " + packetName + " get() { return PACKET<" + packetName + ">(); }");
		}
	}
	if (isFileExist(fullPath))
	{
		myVector<string> codeList;
		int lineStart = -1;
		if (!findCustomCode(fullPath, codeList, lineStart,
			[](const string& codeLine) { return codeLine == "// auto generate start"; },
			[](const string& codeLine) { return endWith(codeLine, "// auto generate end"); }))
		{
			return;
		}
		for (const string& line : generateCodes)
		{
			codeList.insert(++lineStart, line);
		}
		writeFile(fullPath, ANSIToUTF8(codeListToString(codeList).c_str(), true));
	}
	else
	{
		myVector<string> codeList;
		codeList.push_back("using static FrameUtility;");
		codeList.push_back("using static GU;");
		codeList.push_back("");
		codeList.push_back("// auto generate start");
		codeList.addRange(generateCodes);
		codeList.push_back("\t// auto generate end");
		if (startWith(packetName, "SC"))
		{
			codeList.push_back("\tpublic override void execute()");
			codeList.push_back("\t{}");
		}
		else if (startWith(packetName, "CS"))
		{
			codeList.push_back("\tpublic static void send()");
			codeList.push_back("\t{");
			codeList.push_back("\t\tsendPacket(get());");
			codeList.push_back("\t}");
		}
		codeList.push_back("}");
		writeFile(fullPath, ANSIToUTF8(codeListToString(codeList).c_str(), true));
	}
}

void CodeNetPacket::generateCSharpStruct(const PacketStruct& structInfo, const string& hotFixPath)
{
	bool hasOptional = false;
	for (const PacketMember& item : structInfo.mMemberList)
	{
		hasOptional |= item.mOptional;
	}
	myVector<string> codeList;
	codeList.push_back("using System;");
	codeList.push_back("using UnityEngine;");
	codeList.push_back("using System.Collections;");
	codeList.push_back("using System.Collections.Generic;");
	codeList.push_back("using static FrameUtility;");
	if (hasOptional)
	{
		codeList.push_back("using static BinaryUtility;");
	}
	codeList.push_back("");
	codeList.push_back("public class " + structInfo.mStructName + " : NetStructBit");
	codeList.push_back("{");
	for (const PacketMember& item : structInfo.mMemberList)
	{
		codeList.push_back("\t" + cSharpMemberDeclareString(item));
	}

	// 构造
	codeList.push_back("\tpublic " + structInfo.mStructName + "()");
	codeList.push_back("\t{");
	for (const PacketMember& item : structInfo.mMemberList)
	{
		codeList.push_back("\t\taddParam(" + item.mMemberName + ", " + (item.mOptional ? "true" : "false") + ");");
	}
	codeList.push_back("\t}");
	
	myVector<myVector<PacketMember>> memberGroupList;
	generateMemberGroup(structInfo.mMemberList, memberGroupList);

	// readInternal
	codeList.push_back("\tprotected override bool readInternal(ulong fieldFlag, SerializerBitRead reader)");
	codeList.push_back("\t{");
	codeList.push_back("\t\tbool success = true;");
	FOR_VECTOR(memberGroupList)
	{
		const myVector<PacketMember>& memberGroup = memberGroupList[i];
		if (memberGroup.size() == 1)
		{
			const PacketMember& member = memberGroup[0];
			if (member.mOptional)
			{
				codeList.push_back("\t\tif (" + member.mMemberName + ".mValid = hasBit(fieldFlag, " + intToString(member.mIndex) + "))");
				codeList.push_back("\t\t{");
				codeList.push_back("\t\t\t" + singleMemberReadLineCSharp(member.mMemberName, member.mTypeName));
				codeList.push_back("\t\t}");
			}
			else
			{
				codeList.push_back("\t\t" + singleMemberReadLineCSharp(member.mMemberName, member.mTypeName));
			}
		}
		else
		{
			myVector<string> nameList;
			string groupTypeName = expandMembersInGroupCSharp(memberGroup, nameList, false);
			myVector<string> list = multiMemberReadLineCSharp(nameList, groupTypeName, false);
			FOR_VECTOR(list)
			{
				codeList.push_back("\t\t" + list[i]);
			}
		}
	}
	codeList.push_back("\t\treturn success;");
	codeList.push_back("\t}");

	// write
	codeList.push_back("\tpublic override void write(SerializerBitWrite writer)");
	codeList.push_back("\t{");
	codeList.push_back("\t\tbase.write(writer);");
	FOR_VECTOR(memberGroupList)
	{
		const myVector<PacketMember>& memberGroup = memberGroupList[i];
		if (memberGroup.size() == 1)
		{
			const PacketMember& member = memberGroup[0];
			if (member.mOptional)
			{
				codeList.push_back("\t\tif (" + member.mMemberName + ".mValid)");
				codeList.push_back("\t\t{");
				codeList.push_back("\t\t\t" + singleMemberWriteLineCSharp(member.mMemberName, member.mTypeName));
				codeList.push_back("\t\t}");
			}
			else
			{
				codeList.push_back("\t\t" + singleMemberWriteLineCSharp(member.mMemberName, member.mTypeName));
			}
		}
		else
		{
			myVector<string> nameList;
			string groupTypeName = expandMembersInGroupCSharp(memberGroup, nameList, true);
			myVector<string> list = multiMemberWriteLineCSharp(nameList, groupTypeName, false);
			FOR_VECTOR(list)
			{
				codeList.push_back("\t\t" + list[i]);
			}
		}
	}
	codeList.push_back("\t}");

	// resetProperty
	codeList.push_back("\tpublic override void resetProperty()");
	codeList.push_back("\t{");
	codeList.push_back("\t\tbase.resetProperty();");
	for (const PacketMember& item : structInfo.mMemberList)
	{
		codeList.push_back("\t\t" + item.mMemberName + ".resetProperty();");
	}
	codeList.push_back("\t}");

	codeList.push_back("}");
	codeList.push_back("");

	// StructList
	codeList.push_back("public class " + structInfo.mStructName + "_List : SerializableBit, IEnumerable<" + structInfo.mStructName + ">");
	codeList.push_back("{");
	codeList.push_back("\tpublic List<" + structInfo.mStructName + "> mList = new();");
	codeList.push_back("\tpublic " + structInfo.mStructName + " this[int index]");
	codeList.push_back("\t{");
	codeList.push_back("\t\tget { return mList[index]; }");
	codeList.push_back("\t\tset { mList[index] = value; }");
	codeList.push_back("\t}");
	codeList.push_back("\tpublic int Count{ get { return mList.Count; } }");
	codeList.push_back("\tpublic override bool read(SerializerBitRead reader)");
	codeList.push_back("\t{");
	codeList.push_back("\t\treturn reader.readCustomList(mList);");
	codeList.push_back("\t}");
	codeList.push_back("\tpublic override void write(SerializerBitWrite writer)");
	codeList.push_back("\t{");
	codeList.push_back("\t\twriter.writeCustomList(mList);");
	codeList.push_back("\t}");
	codeList.push_back("\tpublic override void resetProperty()");
	codeList.push_back("\t{");
	codeList.push_back("\t\tbase.resetProperty();");
	codeList.push_back("\t\tUN_CLASS_LIST(mList);");
	codeList.push_back("\t}");
	codeList.push_back("\tpublic IEnumerator<" + structInfo.mStructName + "> GetEnumerator(){ return mList.GetEnumerator(); }");
	codeList.push_back("\tIEnumerator IEnumerable.GetEnumerator() { return mList.GetEnumerator(); }");
	codeList.push_back("}");

	writeFile(hotFixPath + structInfo.mStructName + ".cs", ANSIToUTF8(codeListToString(codeList).c_str(), true));
}