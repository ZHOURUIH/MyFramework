#pragma once

#include "CodeUtility.h"

class CodeComponent : public CodeUtility
{
public:
	static void generate();
protected:
	//c++
	static void generateGameComponentRegister(const myVector<string>& componentList, const string& filePath);
protected:
};