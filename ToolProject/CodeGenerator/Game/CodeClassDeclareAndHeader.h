#pragma once

#include "CodeUtility.h"

class CodeClassDeclareAndHeader : public CodeUtility
{
public:
	static void generate();
protected:
	//c++
	static void generateCppFrameClassAndHeader(const string& path, const string& targetFilePath);
	static void generateCppGameClassAndHeader(const string& path, const string& targetFilePath);
protected:
};