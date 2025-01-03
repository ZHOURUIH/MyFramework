#ifndef _MEMORY_INFO_H_
#define _MEMORY_INFO_H_

#include "ServerDefine.h"

struct MemoryInfo
{
	MemoryInfo(int s, const string& f, int l, const string& t)
		:
		size(s),
		file(f),
		line(l),
		type(t)
	{}
	int size;			// 内存大小
	string file;	// 开辟内存的文件
	int line;			// 开辟内存的代码行号
	string type;	// 内存的对象类型
};

struct MemoryType
{
	MemoryType(const string& t = "")
		:
		type(t),
		count(0),
		size(0)
	{}
	MemoryType(const string& t, int c, int s)
		:
		type(t),
		count(c),
		size(s)
	{}
	string type;
	int count;
	int size;
};

#endif