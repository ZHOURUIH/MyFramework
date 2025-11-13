#ifndef _CODE_FRAME_SYSTEM_H_
#define _CODE_FRAME_SYSTEM_H_

#include "CodeUtility.h"

class CodeFrameSystem : public CodeUtility
{
public:
	static void generate();
protected:
	static void generateFrameSystem(const string& cppPath, const string& baseFilePathNoSuffix, const string& gameFilePath, const string& baseClassName, const string& exportMacro);
	static void generateFrameSystemRegister(const myVector<string>& frameSystemList, const string& filePath);
	static void generateFrameSystemClear(const myVector<string>& frameSystemList, const string& gameBaseSourceFile);
	static void generateFrameSystemDeclare(const myVector<string>& frameSystemList, const string& gameBaseHeaderFile, const string& exportMacro);
	static void generateFrameSystemDefine(const myVector<string>& frameSystemList, const string& gameBaseSourceFile);
	static void generateFrameSystemGet(const myVector<string>& frameSystemList, const string& gameBaseSourceFile);
};

#endif