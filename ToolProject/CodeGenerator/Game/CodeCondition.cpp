#include "CodeCondition.h"

void CodeCondition::generate()
{
	print("正在生成组件代码");
	// Game
	const string registerCppPath = cppGamePath + "/ConditionManager/";
	const string registerCSPath = ClientHotFixPath + "/ConditionManager/";

	// 先读取表格描述
	CSVInfo csvInfo;
	parseCSV(ExcelPath + "Condition.csv", csvInfo.mHeader, csvInfo.mDataList);
	if (csvInfo.mDataList.size() == 0)
	{
		return;
	}
	int nameColumn = -1;
	FOR_VECTOR(csvInfo.mHeader.mColumnDataList)
	{
		if (csvInfo.mHeader.mColumnDataList[i]->mName == "VariableName")
		{
			nameColumn = i;
			break;
		}
	}
	if (nameColumn < 0)
	{
		return;
	}
	myVector<string> conditionList;
	FOR_VECTOR(csvInfo.mDataList)
	{
		conditionList.push_back(csvInfo.mDataList[i][nameColumn]);
	}
	generateCppConditionRegister(conditionList, registerCppPath);
	generateCSharpConditionRegister(conditionList, registerCSPath);
	print("完成生成组件代码");
	print("");
}

// ConditionRegister.h和ConditionRegister.cpp
void CodeCondition::generateCppConditionRegister(const myVector<string>& conditionList, const string& filePath)
{
	// 头文件
	string header;
	line(header, "// auto generate start");
	line(header, "#pragma once");
	line(header, "");
	line(header, "#include \"GameBase.h\"");
	line(header, "");
	line(header, "class ConditionRegister");
	line(header, "{");
	line(header, "public:");
	line(header, "\tstatic void registeAll();");
	line(header, "};");
	line(header, "// auto generate end", false);
	writeFile(filePath + "ConditionRegister.h", header);

	// 源文件
	string source;
	line(source, "// auto generate start");
	line(source, "#include \"GameHeader.h\"");
	line(source, "");
	line(source, "void ConditionRegister::registeAll()");
	line(source, "{");
	FOR_VECTOR(conditionList)
	{
		line(source, "\tmConditionFactoryManager->addFactory<Condition" + conditionList[i] + ">(EDCondition::" + conditionList[i] + ");");
	}
	line(source, "}");
	line(source, "// auto generate end", false);
	writeFile(filePath + "ConditionRegister.cpp", source);
}

void CodeCondition::generateCSharpConditionRegister(const myVector<string>& conditionList, const string& filePath)
{
	string content;
	line(content, "// auto generate start");
	line(content, "using static GBR;");
	line(content, "");
	line(content, "public class ConditionRegister");
	line(content, "{");
	line(content, "\tpublic static void registeAll()");
	line(content, "\t{");
	FOR_VECTOR(conditionList)
	{
		line(content, "\t\tregiste<Condition" + conditionList[i] + ", Condition" + conditionList[i] + "Param>(EDCondition." + conditionList[i] + ");");
	}
	line(content, "\t}");
	line(content, "\t//------------------------------------------------------------------------------------------------------------------------------");
	line(content, "\tprotected static void registe<T, Param>(int type) where T : Condition where Param : ConditionParam");
	line(content, "\t{");
	line(content, "\t\tmConditionManager.registe(typeof(T), typeof(Param), type);");
	line(content, "\t}");
	line(content, "};");
	line(content, "// auto generate end", false);
	writeFile(filePath + "ConditionRegister.cs", content);
}