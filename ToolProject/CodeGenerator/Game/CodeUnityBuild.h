#ifndef _CODE_UNITY_BUILD_H_
#define _CODE_UNITY_BUILD_H_

#include "CodeUtility.h"

class CodeUnityBuild : public CodeUtility
{
public:
	static void generate();
protected:
	//c++
	static void generateCppUnityBuild(const string& filePath, const string& unityBuildFileName);
protected:
};

#endif