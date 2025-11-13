#include "CodeComponent.h"

void CodeComponent::generate()
{
	print("正在生成组件代码");
	// Game
	const string cppGameRegisterPath = cppGamePath + "/Component/";
	myVector<string> gameComFiles = findTargetHeaderFile(cppGamePath, 
	[](const string& fileName) 
	{ 
		return startWith(fileName, "COM") && 
			   fileName != "COMCharacterGame" && 
			   fileName != "COMPlayer" && 
			   fileName != "COMNPC"; 
	},
	[](const string& line)
	{
		return findSubstr(line, " : public GameComponent") || 
			   findSubstr(line, " : public COMCharacterGame") || 
			   findSubstr(line, " : public COMPlayer") || 
			   findSubstr(line, " : public COMNPC") || 
			   findSubstr(line, " : public COMCharacterSkill") || 
			   findSubstr(line, " : public GameComponent");
	});
	// 生成StringDefine文件
	CodeUtility::generateStringDefine(gameComFiles, 20000, "// Component", cppGameStringDefineHeaderFile);
	// ComponentRegister.cpp
	generateGameComponentRegister(gameComFiles, cppGameRegisterPath);
	print("完成生成组件代码");
	print("");
}

// GameComponentRegister.cpp
void CodeComponent::generateGameComponentRegister(const myVector<string>& componentList, const string& filePath)
{
	string source;
	line(source, "#include \"GameHeader.h\"");
	line(source, "");
	line(source, "void GameComponentRegister::registeAll()");
	line(source, "{");
	FOR_VECTOR(componentList)
	{
		line(source, "\tmGameComponentFactoryManager->addFactory<" + componentList[i] + ">();");
	}
	line(source, "}", false);
	writeFile(filePath + "GameComponentRegister.cpp", ANSIToUTF8(source.c_str(), true));
}