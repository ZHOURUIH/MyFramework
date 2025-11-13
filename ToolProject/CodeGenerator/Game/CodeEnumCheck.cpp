#include "CodeEnumCheck.h"

void CodeEnumCheck::generate()
{
	print("正在生成枚举检测");
	// 只收集指定文件中定义的枚举
	doGenerate("GameEnumCheck", cppGamePath + "Common/", cppGamePath + "Common/GameEnum.h");
	doGenerate("FrameEnumCheck", cppFramePath + "Common/", cppFramePath + "Common/FrameEnum.h");
	print("完成生成枚举检测");
	print("");
}

//-------------------------------------------------------------------------------------------------------------------------------
void CodeEnumCheck::doGenerate(const string& className, const string& path, const string& headerFile)
{
	myMap<string, myVector<string>> allEnumValueList;
	collectEnum(headerFile, allEnumValueList);
	generateEnumCheck(allEnumValueList, className, path + className + ".h", getFileName(headerFile));
}

void CodeEnumCheck::collectEnum(const string& fileName, myMap<string, myVector<string>>& allEnumValueList)
{
	string curEnumName;
	myVector<string> enumValueList;
	myVector<string> lines = openTxtFileLines(fileName);
	FOR_VECTOR(lines)
	{
		const string& line = lines[i];
		if (curEnumName.length() == 0)
		{
			int pos = 0;
			if (findSubstr(line, "enum class ", &pos))
			{
				int startPos = pos + (int)strlen("enum class ");
				int pos0 = 0;
				int pos1 = 0;
				findSubstr(line, ":", &pos0, startPos);
				findSubstr(line, " ", &pos1, startPos);
				if (pos0 < 0 && pos1 < 0)
				{
					ERROR("无效的枚举定义:" + line);
					continue;
				}
				curEnumName = line.substr(startPos, getMin(pos0, pos1) - startPos);
			}
		}
		else
		{
			if (line[0] == '{')
			{
				continue;
			}
			if (line.find_first_of('}') != -1)
			{
				// 一个枚举查找结束
				allEnumValueList.insert(curEnumName, enumValueList);
				curEnumName = "";
				enumValueList.clear();
				continue;
			}
			// 获得枚举值
			int firstPos = findFirstNonEmptyChar(line);
			int endPos = 0;
			if (!findSubstr(line, ",", &endPos))
			{
				continue;
			}
			int firstEqual = -1;
			findSubstr(line, "=", &firstEqual);
			if (firstEqual >= 0)
			{
				endPos = getMin(endPos, firstEqual);
			}
			string enumValue = line.substr(firstPos, endPos - firstPos);
			removeAll(enumValue, ' ', '\t');
			enumValueList.push_back(enumValue);
		}
	}
}

void CodeEnumCheck::generateEnumCheck(const myMap<string, myVector<string>>& allEnumValueList, const string& className, const string& fullPath, const string& headerFile)
{
	string header;
	line(header, "#pragma once");
	line(header, "");
	line(header, "#include \"" + headerFile + "\"");
	line(header, "");
	line(header, "// auto generated file");
	line(header, "class " + className);
	line(header, "{");
	line(header, "public:");
	for (const auto& itemPair : allEnumValueList)
	{
		line(header, "\tstatic constexpr bool checkEnum(const " + itemPair.first + " value)");
		line(header, "\t{");
		line(header, "\t\tswitch (value)");
		line(header, "\t\t{");
		for (const auto& item0 : itemPair.second)
		{
			line(header, "\t\tcase " + itemPair.first + "::" + item0 + ":break;");
		}
		line(header, "\t\tdefault:return false;");
		line(header, "\t\t}");
		line(header, "\t\treturn true;");
		line(header, "\t}");
		line(header, "");
	}
	removeLast(header, '\r');
	removeLast(header, '\n');
	line(header, "};", false);
	writeFile(fullPath, ANSIToUTF8(header.c_str(), true));
}