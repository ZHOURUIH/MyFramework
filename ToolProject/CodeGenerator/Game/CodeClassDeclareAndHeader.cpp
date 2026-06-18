#include "CodeClassDeclareAndHeader.h"

void CodeClassDeclareAndHeader::generate()
{
	print("正在生成类声明");
	generateCppFrameClassAndHeader(ServerFrameProjectPath, cppFramePath + "Common/");
	generateCppGameClassAndHeader(cppGamePath, cppGamePath + "Common/");
	print("完成生成类声明");
	print("");
}

void CodeClassDeclareAndHeader::generateCppFrameClassAndHeader(const string& path, const string& targetFilePath)
{
	const string headerFileName = "FrameHeader";
	myVector<string> frameClassList;
	myVector<string> frameHeaderList;
	myVector<string> headerFiles = findFiles(path, ".h");
	FOR_INVERSE_I(headerFiles.size())
	{
		if (findSubstr(headerFiles[i], "Dependency/"))
		{
			headerFiles.erase(i);
		}
	}
	for (const string& fileName : headerFiles)
	{
		if (getFileNameNoSuffix(fileName, true) != headerFileName)
		{
			frameHeaderList.push_back(getFileName(fileName));
		}
		myVector<string> fileLines = openFile(fileName);
		FOR_J(fileLines.size())
		{
			if (findSubstr(j > 0 ? fileLines[j - 1] : EMPTY_STRING, "template"))
			{
				continue;
			}
			string className = findClassName(fileLines[j]);
			if (!className.empty())
			{
				frameClassList.push_back(className);
			}
		}
	}
	frameClassList.addRange(findPoolClass(headerFiles));

	// FrameClassDeclare.h
	string str1;
	line(str1, "// auto generate start");
	line(str1, "#pragma once");
	line(str1, "");
	FOR_I(frameClassList.size())
	{
		line(str1, "class " + frameClassList[i] + ";");
	}
	line(str1, "// auto generate end", false);
	writeFile(targetFilePath + "FrameClassDeclare.h", str1);

	// FrameHeader.h
	string str0;
	line(str0, "// auto generate start");
	line(str0, "#pragma once");
	line(str0, "");
	FOR_I(frameHeaderList.size())
	{
		line(str0, "#include \"" + frameHeaderList[i] + "\"");
	}
	line(str0, "// auto generate end", false);
	writeFile(targetFilePath + headerFileName + ".h", str0);
}

void CodeClassDeclareAndHeader::generateCppGameClassAndHeader(const string& path, const string& targetFilePath)
{
	const string headerFileName = "GameHeader";
	myVector<string> gameClassList;
	myVector<string> gameHeaderList;
	myVector<string> headerFiles = findFiles(path, ".h");
	for (const string& fileName : headerFiles)
	{
		if (getFileNameNoSuffix(fileName, true) != headerFileName)
		{
			gameHeaderList.push_back(getFileName(fileName));
		}
		myVector<string> fileLines = openFile(fileName);
		FOR_J(fileLines.size())
		{
			if (findSubstr(j > 0 ? fileLines[j - 1] : EMPTY_STRING, "template"))
			{
				continue;
			}
			string className = findClassName(fileLines[j]);
			if (!className.empty())
			{
				gameClassList.push_back(className);
			}
		}
	}
	gameClassList.addRange(findPoolClass(headerFiles));

	// GameClassDeclare.h
	string str1;
	line(str1, "// auto generate start");
	line(str1, "#pragma once");
	line(str1, "");
	uint count0 = gameClassList.size();
	FOR_I(count0)
	{
		line(str1, "class " + gameClassList[i] + ";");
	}
	line(str1, "// auto generate end", false);
	writeFile(targetFilePath + "GameClassDeclare.h", str1);

	// GameHeader.h
	string str0;
	line(str0, "// auto generate start");
	line(str0, "#pragma once");
	line(str0, "");
	line(str0, "#include \"FrameHeader.h\"");
	FOR_I(gameHeaderList.size())
	{
		line(str0, "#include \"" + gameHeaderList[i] + "\"");
	}
	line(str0, "// auto generate end", false);
	writeFile(targetFilePath + headerFileName + ".h", str0);
}