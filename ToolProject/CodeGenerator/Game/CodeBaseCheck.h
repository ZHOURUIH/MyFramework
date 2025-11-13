#pragma once

#include "CodeUtility.h"

class CodeBaseCheck : public CodeUtility
{
public:
	static void generate();
protected:
	static void doGenerate(const string& path);
};