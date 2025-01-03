#pragma once

#include "ServerDefine.h"
#include "FrameUtility.h"

class Config : public FrameUtility
{
public:
	static string mAtlasPath;
public:
	static bool parse(const string& configPath)
	{
		myVector<string> lines;
		openTxtFileLines(configPath, lines, false);
		if (lines.size() == 0)
		{
			return false;
		}
		for (const string& line : lines)
		{
			if (startWith(line, "//"))
			{
				continue;
			}
			myVector<string> param;
			split(line.c_str(), "=", param);
			if (param.size() != 2)
			{
				cout << "配置文件读取错误:" + line << endl;
				return false;
			}
			if (param[0] == "AtlasPath")
			{
				mAtlasPath = param[1];
				validPath(mAtlasPath);
				rightToLeft(mAtlasPath);
			}
		}
		if (mAtlasPath.empty())
		{
			cout << "路径解析错误,找不到AtlasPath的配置";
			return false;
		}
		return true;
	}
};