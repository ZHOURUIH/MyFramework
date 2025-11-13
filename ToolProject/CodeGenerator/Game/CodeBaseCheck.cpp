#include "CodeBaseCheck.h"

void CodeBaseCheck::generate()
{
	print("正在进行Base检测");
	// 只收集指定文件中定义的枚举
	myVector<string> files;
	findFiles(ServerGameProjectPath, files, ".h");
	findFiles(ServerFrameProjectPath, files, ".h");
	for (const string& file : files)
	{
		doGenerate(file);
	}
	print("完成Base检测");
	print("");
}

//-------------------------------------------------------------------------------------------------------------------------------
void CodeBaseCheck::doGenerate(const string& path)
{
	if (findString(path.c_str(), "Dependency/"))
	{
		return;
	}
	bool hasBOM = false;
	auto lines = openTxtFileLines(path, true, &hasBOM);
	for (int i = 0; i < lines.size(); ++i)
	{
		// 如果带逗号就跳过,基类中带逗号的无法添加BASE宏
		if (findString(lines[i].c_str(), ","))
		{
			continue;
		}
		const string className = findClassName(lines[i]);
		const string baseClassName = findClassBaseName(lines[i]);
		if (className != "" && baseClassName != "" && i + 2 < lines.size())
		{
			if (findString(lines[i + 2].c_str(), "BASE("))
			{
				lines[i + 2] = "\tBASE(" + className + ", " + baseClassName + ");";
			}
			else if (findString(lines[i + 1].c_str(), "{};"))
			{
				lines[i + 1] = "{";
				lines[i + 2] = "};";
				lines.insert(i + 2, "\tBASE(" + className + ", " + baseClassName + ");");
			}
			else
			{
				lines.insert(i + 2, "\tBASE(" + className + ", " + baseClassName + ");");
			}
		}
	}
	writeFileIfChanged(path, ANSIToUTF8(codeListToString(lines).c_str(), hasBOM));
}