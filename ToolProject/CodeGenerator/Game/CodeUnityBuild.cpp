#include "CodeUnityBuild.h"

void CodeUnityBuild::generate()
{
	print("正在生成UnityBuild");
	if (ServerGameProjectPath.length() == 0)
	{
		return;
	}

	// 生成UnityBuild.cpp文件
	generateCppUnityBuild(cppGamePath, "UnityBuildGame.cpp");
	generateCppUnityBuild(cppFramePath, "UnityBuildFrame.cpp");
	print("完成生成UnityBuild");
	print("");
}

// UnityBuild.cpp
void CodeUnityBuild::generateCppUnityBuild(const string& filePath, const string& unityBuildFileName)
{
	myVector<string> fileList;
	myVector<string> patterns{ ".cpp" };
	findFiles(filePath, fileList, patterns.data(), patterns.size());
	for (string& fileName : fileList)
	{
		// 根据项目文件的附加包含目录来决定保留什么目录
		string excludePath;
		for (const string& exclude : ServerExcludeIncludePath)
		{
			if (startWith(fileName, exclude))
			{
				excludePath = exclude;
				break;
			}
		}
		if (excludePath.empty())
		{
			fileName = getFileName(fileName);
		}
		else
		{
			// 文件所在目录不在包含目录中时,暂时只保留此文件的最后一级目录
			fileName = getFileName(getFilePath(fileName)) + "/" + getFileName(fileName);
		}
	}
	fileList.remove(unityBuildFileName);

	if (filePath == cppGamePath)
	{
		fileList.push_back("main.cpp");
	}
	string str0;
	uint count = fileList.size();
	FOR_I(count)
	{
		line(str0, "#include \"" + fileList[i] + "\"", i != count - 1);
	}
	writeFileIfChanged(filePath + unityBuildFileName, ANSIToUTF8(str0.c_str(), true));
}