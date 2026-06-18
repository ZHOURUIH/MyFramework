#pragma once

#include "CodeUtility.h"

class CodeCondition : public CodeUtility
{
public:
	static void generate();
protected:
	//c++
	static void generateCppConditionRegister(const myVector<string>& conditionList, const string& filePath);
	// c#
	static void generateCSharpConditionRegister(const myVector<string>& conditionList, const string& filePath);
protected:
};