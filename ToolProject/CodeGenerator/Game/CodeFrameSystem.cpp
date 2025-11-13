#include "CodeFrameSystem.h"

void CodeFrameSystem::generate()
{
	print("正在生成框架组件");
	generateFrameSystem(cppGamePath, "Common/GameBase.h", "Game/Game.cpp", "GameBase", "");
	generateFrameSystem(cppFramePath, "Common/FrameBase.h", "ServerFramework/ServerFramework.cpp", "FrameBase", "MICRO_LEGEND_FRAME_API ");
	print("完成生成框架组件");
	print("");
}

void CodeFrameSystem::generateFrameSystem(const string& cppPath, const string& baseFilePathNoSuffix, const string& gameFilePath, const string& baseClassName, const string& exportMacro)
{
	myVector<string> frameSystemList = findTargetHeaderFile(cppPath,
		[](const string& fileName) { return endWith(fileName, "System") || endWith(fileName, "Manager"); },
		[](const string& line)
		{
			return findSubstr(line, " : public FrameComponent") && 
				   findClassName(line) != "FactoryManager" && 
				   findClassName(line) != "ClassPool" &&
				   findClassName(line) != "ClassPoolThread" &&
				   findClassName(line) != "ClassTypePool" &&
				   findClassName(line) != "ClassTypePoolThread" &&
				   findClassName(line) != "ClassBaseTypePool" &&
				   findClassName(line) != "ClassBaseTypePoolThread" &&
				   findClassName(line) != "ClassKeyPool" && 
				   findClassName(line) != "ClassKeyPoolThread" &&
				   findClassName(line) != "ArrayPool" &&
				   findClassName(line) != "ArrayPoolThread";
		});
	myVector<string> classPoolList = findTargetHeaderFile(cppPath,
		[](const string& fileName) { return endWith(fileName, "Pool") || endWith(fileName, "PoolThread"); },
		[](const string& line)
		{
			return findSubstr(line, " : public ClassPool<") ||
				   findSubstr(line, " : public ClassPoolThread<") ||
				   findSubstr(line, " : public ClassTypePool<") ||
				   findSubstr(line, " : public ClassTypePoolThread<") ||
				   findSubstr(line, " : public ClassBaseTypePool<") ||
				   findSubstr(line, " : public ClassBaseTypePoolThread<") ||
				   findSubstr(line, " : public ClassKeyPool<") ||
				   findSubstr(line, " : public ClassKeyPoolThread<") ||
				   findSubstr(line, " : public PodPoolThread<") ||
				   findSubstr(line, " ArrayPool ") ||
				   findSubstr(line, " ArrayPoolThread ");
		});
	myVector<string> factoryList = findTargetHeaderFile(cppPath,
		[](const string& fileName) { return endWith(fileName, "FactoryManager"); },
		[](const string& line) { return findSubstr(line, " : public FactoryManager<"); });
	// 暂时只能特殊判断,把ServerFramework加上
	if (baseClassName == "FrameBase")
	{
		frameSystemList.insert(0, "ServerFramework");
	}
	frameSystemList.addRange(classPoolList);
	frameSystemList.addRange(factoryList);
	generateFrameSystemRegister(frameSystemList, cppPath + gameFilePath);
	generateFrameSystemDeclare(frameSystemList, cppPath + baseFilePathNoSuffix, exportMacro);
	const string gameBaseSource = cppPath + replaceSuffix(baseFilePathNoSuffix, ".cpp");
	generateFrameSystemDefine(frameSystemList, gameBaseSource);
	generateFrameSystemGet(frameSystemList, gameBaseSource);
	generateFrameSystemClear(frameSystemList, gameBaseSource);
}

void CodeFrameSystem::generateFrameSystemRegister(const myVector<string>& frameSystemList, const string& gameCppPath)
{
	// 更新Game.cpp的特定部分代码
	myVector<string> codeList;
	int lineStart = -1;
	if (!findCustomCode(gameCppPath, codeList, lineStart,
		[](const string& codeLine) { return endWith(codeLine, "// auto generate start FrameSystem Register"); },
		[](const string& codeLine) { return endWith(codeLine, "// auto generate end FrameSystem Register"); }))
	{
		return;
	}

	for (const string& item : frameSystemList)
	{
		if (item == "ServerFramework")
		{
			continue;
		}
		if (startWith(item, "MySQL"))
		{
			codeList.insert(++lineStart, "#ifdef _MYSQL");
		}
		codeList.insert(++lineStart, "\tregisteSystem<" + item + ">(STR(" + item + "));");
		if (startWith(item, "MySQL"))
		{
			codeList.insert(++lineStart, "#endif");
		}
	}
	writeFileIfChanged(gameCppPath, ANSIToUTF8(codeListToString(codeList).c_str(), true));
}

void CodeFrameSystem::generateFrameSystemClear(const myVector<string>& frameSystemList, const string& gameBaseSourceFile)
{
	// 更新GameBase.cpp的特定部分代码
	myVector<string> codeList;
	int lineStart = -1;
	if (!findCustomCode(gameBaseSourceFile, codeList, lineStart,
		[](const string& codeLine) { return endWith(codeLine, "// auto generate start FrameSystem Clear"); },
		[](const string& codeLine) { return endWith(codeLine, "// auto generate end FrameSystem Clear"); }))
	{
		return;
	}
	for (const string& item : frameSystemList)
	{
		if (startWith(item, "MySQL"))
		{
			codeList.insert(++lineStart, "#ifdef _MYSQL");
		}
		codeList.insert(++lineStart, "\t\tm" + item + " = nullptr;");
		if (startWith(item, "MySQL"))
		{
			codeList.insert(++lineStart, "#endif");
		}
	}
	writeFileIfChanged(gameBaseSourceFile, ANSIToUTF8(codeListToString(codeList).c_str(), true));
}

void CodeFrameSystem::generateFrameSystemDeclare(const myVector<string>& frameSystemList, const string& gameBaseHeaderFile, const string& exportMacro)
{
	// 更新GameBase.h的特定部分代码
	myVector<string> codeList;
	int lineStart = -1;
	if (!findCustomCode(gameBaseHeaderFile, codeList, lineStart,
		[](const string& codeLine) { return endWith(codeLine, "// auto generate start FrameSystem Extern"); },
		[](const string& codeLine) { return endWith(codeLine, "// auto generate end FrameSystem Extern"); }))
	{
		return;
	}
	for (const string& item : frameSystemList)
	{
		if (startWith(item, "MySQL"))
		{
			codeList.insert(++lineStart, "#ifdef _MYSQL");
		}
		codeList.insert(++lineStart, "\t" + exportMacro + "extern " + item + "* m" + item + ";");
		if (startWith(item, "MySQL"))
		{
			codeList.insert(++lineStart, "#endif");
		}
	}
	writeFileIfChanged(gameBaseHeaderFile, ANSIToUTF8(codeListToString(codeList).c_str(), true));
}

void CodeFrameSystem::generateFrameSystemDefine(const myVector<string>& frameSystemList, const string& gameBaseSourceFile)
{
	// 更新GameBase.cpp的特定部分代码
	myVector<string> codeList;
	int lineStart = -1;
	if (!findCustomCode(gameBaseSourceFile, codeList, lineStart,
		[](const string& codeLine) { return endWith(codeLine, "// auto generate start FrameSystem Define"); },
		[](const string& codeLine) { return endWith(codeLine, "// auto generate end FrameSystem Define"); }))
	{
		return;
	}

	for (const string& item : frameSystemList)
	{
		if (startWith(item, "MySQL"))
		{
			codeList.insert(++lineStart, "#ifdef _MYSQL");
		}
		codeList.insert(++lineStart, "\t" + item + "* m" + item + ";");
		if (startWith(item, "MySQL"))
		{
			codeList.insert(++lineStart, "#endif");
		}
	}
	writeFileIfChanged(gameBaseSourceFile, ANSIToUTF8(codeListToString(codeList).c_str(), true));
}

void CodeFrameSystem::generateFrameSystemGet(const myVector<string>& frameSystemList, const string& gameBaseSourceFile)
{
	// 更新GameBase.cpp的特定部分代码
	myVector<string> codeList;
	int lineStart = -1;
	if (!findCustomCode(gameBaseSourceFile, codeList, lineStart,
		[](const string& codeLine) { return endWith(codeLine, "// auto generate start FrameSystem Get"); },
		[](const string& codeLine) { return endWith(codeLine, "// auto generate end FrameSystem Get"); }))
	{
		return;
	}

	for (const string& item : frameSystemList)
	{
		if (startWith(item, "MySQL"))
		{
			codeList.insert(++lineStart, "#ifdef _MYSQL");
		}
		if (item == "ServerFramework")
		{
			codeList.insert(++lineStart, "\t\tmServerFramework = ServerFramework::getSingleton();");
		}
		else
		{
			codeList.insert(++lineStart, "\t\tmServerFramework->getSystem(STR(" + item + "), m" + item + ");");
		}
		if (startWith(item, "MySQL"))
		{
			codeList.insert(++lineStart, "#endif");
		}
	}
	writeFileIfChanged(gameBaseSourceFile, ANSIToUTF8(codeListToString(codeList).c_str(), true));
}