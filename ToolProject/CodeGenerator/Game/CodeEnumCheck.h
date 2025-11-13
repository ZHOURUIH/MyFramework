#pragma once

#include "CodeUtility.h"

class CodeEnumCheck : public CodeUtility
{
public:
	static void generate();
protected:
	static void doGenerate(const string& className, const string& path, const string& headerFile);
	static void collectEnum(const string& fileName, myMap<string, myVector<string>>& allEnumValueList);
	static void generateEnumCheck(const myMap<string, myVector<string>>& allEnumValueList, const string& className, const string& fullPath, const string& headerFile);
};