#ifndef _INDEPENDENT_LOG_H_
#define _INDEPENDENT_LOG_H_

#include <string>

using std::string;

// 任何地方都可以直接包含的用于打日志的类
class IndependentLog
{
public:
	static void directError(const string& info);
	static void directLog(const string& info);
};

#endif