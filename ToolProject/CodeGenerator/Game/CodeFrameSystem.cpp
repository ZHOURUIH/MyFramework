#include "CodeFrameSystem.h"

void CodeFrameSystem::generate()
{
	print("正在生成框架组件");
	generateFrameSystem(cppGamePath, "Common/GameBase.h", "Game/Game.cpp", "GameBase", "");
	generateFrameSystem(cppFramePath, "Common/FrameBase.h", "ServerFramework/ServerFramework.cpp", "FrameBase", "MICRO_LEGEND_FRAME_API ");
	print("完成生成框架组件");
	print("");
}

bool CodeFrameSystem::isSystemFile(const string fileName)
{
	return endWith(fileName, "System") || endWith(fileName, "Manager");
}

bool CodeFrameSystem::isFrameClass(const string& className, const string& line)
{
	if (className.empty())
	{
		return false;
	}
	return findSubstr(line, " : public FrameComponent") &&
		className != "FactoryManager" &&
		className != "ClassPool" &&
		className != "ClassPoolThread" &&
		className != "ClassTypePool" &&
		className != "ClassTypePoolThread" &&
		className != "ClassBaseTypePool" &&
		className != "ClassBaseTypePoolThread" &&
		className != "ClassKeyPool" &&
		className != "ClassKeyPoolThread" &&
		className != "ArrayPool" &&
		className != "ClassPoolBase" &&
		className != "ArrayPoolThread";
}

bool CodeFrameSystem::isPoolFile(const string& fileName)
{
	return endWith(fileName, "Pool") || endWith(fileName, "PoolThread");
}

bool CodeFrameSystem::isPoolClass(const string& line)
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
		findSubstr(line, " ArrayPool : public ") ||
		findSubstr(line, " ArrayPoolThread : public ");
}

bool CodeFrameSystem::isClassObjectPoolFile(const string& fileName)
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
		return false;
	}
	return true;
}

bool CodeFrameSystem::isFactoryFile(const string& fileName)
{
	return endWith(fileName, "FactoryManager") && fileName != "FactoryManager";
}

bool CodeFrameSystem::isFactoryClass(const string& line)
{
	return findSubstr(line, " : public FactoryManager<");
}

void CodeFrameSystem::generateFrameSystem(const string& cppPath, const string& baseFilePathNoSuffix, const string& gameFilePath, const string& baseClassName, const string& exportMacro)
{
	const myVector<string> headerFiles = findFiles(cppPath, ".h");
	myVector<string> frameSystemList;
	myVector<string> classPoolList;
	myVector<string> classOjbectPoolList;
	myVector<string> factoryList;
	myVector<string> allLines;
	for (const string& file : headerFiles)
	{
		// 需要排除掉第三方的库
		if (findSubstr(file, "Dependency/"))
		{
			continue;
		}
		allLines.addRange(openFile(file));
	}
	for (const string& line : allLines)
	{
		string className = findClassName(line);
		if (!className.empty())
		{
			if (isPoolClass(line))
			{
				classPoolList.push_back(className);
			}
			if (isFrameClass(className, line))
			{
				frameSystemList.push_back(className);
			}
			if (isFactoryClass(line))
			{
				factoryList.push_back(className);
			}
		}
		else
		{
			string poolName0 = getPoolName(line, "CLASS_POOL");
			if (!poolName0.empty())
			{
				classOjbectPoolList.push_back(poolName0 + "Pool");
			}
		}
	}

	// 暂时只能特殊判断,把ServerFramework加上
	if (baseClassName == "FrameBase")
	{
		frameSystemList.insert(0, "ServerFramework");
	}
	frameSystemList.addRange(classPoolList);
	frameSystemList.addRange(classOjbectPoolList);
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
	writeFile(gameCppPath, codeList);
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
	writeFile(gameBaseSourceFile, codeList);
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
	writeFile(gameBaseHeaderFile, codeList);
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
	writeFile(gameBaseSourceFile, codeList);
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
	writeFile(gameBaseSourceFile, codeList);
}