#pragma once

#include <wxHeader.h>

// 最大并发连接数为128,需要在winsock.h之前进行定义
#ifdef FD_SETSIZE
#undef FD_SETSIZE
#endif
#define FD_SETSIZE 128

// 由于下面定义的部分宏容易在系统头文件中被定义,从而造成编译无法通关,所以先包含系统头文件,然后再定义自己的宏
#ifdef WIN32
// 链接静态库
#pragma comment(lib, "ws2_32.lib")
#pragma comment(lib, "winmm.lib")
#ifdef _LIBEVENT
#pragma comment(lib, "event.lib")
#pragma comment(lib, "event_core.lib")
#pragma comment(lib, "event_extra.lib")
#pragma comment(lib, "event_openssl.lib")
#pragma comment(lib, "libssl.lib")
#pragma comment(lib, "libcrypto.lib")
#endif
#ifdef _MYSQL
#pragma comment(lib, "libmysql.lib")
#endif
#pragma warning(disable: 4005)
#pragma warning(disable: 4251)
// libevent的头文件只能在windows库文件之前包含,否则会有定义冲突的报错
// 部分平台未安装libevent,所以需要使用宏来判断是否需要编译libevent相关代码
#ifdef _LIBEVENT
#include "event2/event.h"
#include "event2/buffer.h"
#include "event2/http.h"
#include "event2/bufferevent_ssl.h"
#include "event2/thread.h"
#include "openssl/ssl.h"
#endif
#include <windows.h>
#include <mmsystem.h>
#include <iostream>
#include <io.h>
#include <direct.h>
#include <winsock.h>
#include <tlhelp32.h>
#include <dbghelp.h>
#endif
#ifdef LINUX
#include <netinet/in.h>
#include <netinet/tcp.h>
#include <sys/socket.h>
#include <sys/epoll.h>
#include <sys/sysinfo.h>
#include <sys/un.h>
#include <sys/time.h>
#include <arpa/inet.h>
#include <unistd.h>
#include <errno.h>
#include <netdb.h>
#include <stdarg.h>
#include <signal.h>
#include <dirent.h>
#include <pthread.h>
#include <locale.h>
#include <execinfo.h>
// linux上包含的libevent的头文件并不是项目中的文件,而是linux系统中的库文件
#ifdef _LIBEVENT
#include "event2/event.h"
#include "event2/buffer.h"
#include "event2/http.h"
#include "event2/bufferevent_ssl.h"
#include "event2/thread.h"
#include "openssl/ssl.h"
#endif
#endif
#include <string>
#include <map>
#include <vector>
#include <set>
#include <list>
#include <stack>
#include <queue>
#include <stdio.h>
#include <stdlib.h>
#include <sys/stat.h>
#include <typeinfo>
#include <memory>
#include <time.h>
#include <cmath>
#include <math.h>
#include <sys/types.h>
#include <cctype>
#include <algorithm>
#include <assert.h>
#include <fcntl.h>
#include <atomic>
#include <functional>
#include <type_traits>
#include <fstream>
#include <array>
#include <sstream>
#include <unordered_map>
#include <iomanip>
// 部分平台未安装mysql,所以需要使用宏来判断是否需要编译mysql相关代码
#ifdef _MYSQL
#include <mysql.h>
#endif
#include "md5/md5.h"

//-----------------------------------------------------------------------------------------------------------------------------------------------
// 宏定义
#ifdef WIN32
#define MY_THREAD							HANDLE
#define MY_SOCKET							SOCKET
#define NULL_THREAD							nullptr
#define THREAD_CALLBACK_DECLEAR(func)		static DWORD WINAPI func(LPVOID args)
#define THREAD_CALLBACK(class, func)		DWORD WINAPI class##::##func(LPVOID args)
#define CREATE_THREAD(thread, func, args)	thread = CreateThread(nullptr, 0, func, args, 0, nullptr)
#define CLOSE_THREAD(thread)		\
if (thread != NULL_THREAD)			\
{									\
	TerminateThread(thread, 0);		\
	CloseHandle(thread);			\
	thread = NULL_THREAD;			\
}
#define CLOSE_SOCKET(socket)		\
if (socket != INVALID_SOCKET)		\
{									\
	closesocket(socket);			\
	socket = INVALID_SOCKET;		\
}
#define SPRINTF(buffer, bufferSize, ...)			sprintf_s(buffer, bufferSize, __VA_ARGS__)
#define MEMCPY(dest, bufferSize, src, count)		memcpy_s((void*)(dest), (bufferSize), (void*)(src), (count))
#define MEMMOV(dest, bufferSize, src, count)		memmove_s((void*)(dest), (bufferSize), (void*)(src), (count))
// 获取不同平台下的字面常量字符串的UTF8编码字符串,只能处理字面常量,也就是在代码中写死的字符串
// windows下需要由GB2312转换为UTF8,而linux则直接就是UTF8的
// 将一个字面常量字符串转换为UTF8后存储为变量
#define UNIFIED_UTF8(var, size, constantString)		MyString<size> var; ANSIToUTF8(constantString, var.toBuffer(), size, false)
#define UNIFIED_UTF8_STRING(var, constantString)	string var = ANSIToUTF8(constantString, false)
#elif defined LINUX
#define MY_THREAD							pthread_t
#define MY_SOCKET							unsigned int
#define NULL_THREAD							0
#define SOCKADDR_IN							sockaddr_in
#define THREAD_CALLBACK_DECLEAR(func)		static void* func(void* args)
#define THREAD_CALLBACK(class, func)		void* class##::##func(void* args)
#define CREATE_THREAD(thread, func, args)	pthread_create(&thread, nullptr, func, args);
#define CLOSE_THREAD(thread)	\
if (thread != NULL_THREAD)		\
{								\
	pthread_cancel(thread);		\
	thread = NULL_THREAD;		\
}
#define CLOSE_SOCKET(socket)		\
if (socket != INVALID_SOCKET)		\
{									\
	close(socket);					\
	socket = INVALID_SOCKET;		\
}
#ifdef __GNUC__
#define CSET_GBK							"GBK"
#define CSET_UTF8							"UTF-8"
#define LC_NAME_zh_CN						"zh_CN"
#endif
#define LC_NAME_zh_CN_GBK					LC_NAME_zh_CN "." CSET_GBK
#define LC_NAME_zh_CN_UTF8					LC_NAME_zh_CN "." CSET_UTF8
#define LC_NAME_zh_CN_DEFAULT				LC_NAME_zh_CN_GBK
#define SPRINTF(buffer, bufferSize, ...)	snprintf(buffer, bufferSize, __VA_ARGS__)
// 因为bufferSize在windows下是需要的,linux下不需要,只是为了让宏与windows下的统一
#define MEMCPY(dest, bufferSize, src, count) memcpy((void*)(dest), (void*)(src), (count))
#define MEMMOV(dest, bufferSize, src, count) memmove((void*)(dest), (void*)(src), (count))
// 将一个字面常量字符串转换为UTF8后存储为变量
#define UNIFIED_UTF8(var, size, constantString)		MyString<size> var;	var.setString(constantString)
#define UNIFIED_UTF8_STRING(var, constantString)	string var = constantString
#endif

#ifndef INVALID_SOCKET
#define INVALID_SOCKET (unsigned int)~0
#endif

#define STR(t)			#t
#define LINE_STR(v)		STR(v)
#define _FILE_LINE_		"File : " + string(__FILE__) + ", Line : " + LINE_STR(__LINE__)
// 通过定义多个宏的方式,改变宏的展开顺序,从而使__LINE__宏先展开,再进行拼接,达到自动根据行号定义一个唯一标识符的功能
#define MAKE_LABEL2(label, L) label##L
#define MAKE_LABEL1(label, L) MAKE_LABEL2(label, L)
#define UNIQUE_IDENTIFIER(label) MAKE_LABEL1(label, __LINE__)

// 为了简化代码书写而添加的宏
//--------------------------------------------------------------------------------------------------------------------------------------------
// 使用下标遍历列表
#define FOR_VECTOR(stl)			const int UNIQUE_IDENTIFIER(Count) = (stl).size(); for(int i = 0; i < UNIQUE_IDENTIFIER(Count); ++i)
#define FOR_VECTOR_J(stl)		const int UNIQUE_IDENTIFIER(Count) = (stl).size(); for(int j = 0; j < UNIQUE_IDENTIFIER(Count); ++j)
#define FOR_VECTOR_K(stl)		const int UNIQUE_IDENTIFIER(Count) = (stl).size(); for(int k = 0; k < UNIQUE_IDENTIFIER(Count); ++k)
#define FOR_VECTOR_INVERSE(stl) for(int i = (stl).size() - 1; i >= 0; --i)

// 简单的for循环
#define FOR(count)				const int UNIQUE_IDENTIFIER(Count) = (int)count; for (int i = 0; i < UNIQUE_IDENTIFIER(Count); ++i)
#define FOR_J(count)			const int UNIQUE_IDENTIFIER(Count) = (int)count; for (int j = 0; j < UNIQUE_IDENTIFIER(Count); ++j)
#define FOR_INVERSE_I(count)	for (int i = (int)count - 1; i >= 0; --i)
#define FOR_ONCE				for (int UNIQUE_IDENTIFIER(temp) = 0; UNIQUE_IDENTIFIER(temp) < 1; ++UNIQUE_IDENTIFIER(temp))

// 将将当前类重命名为This,类的基类重命名为base,方便使用
#define BASE(thisType, baseClass)	typedef thisType This;typedef baseClass base
#define THIS(thisType)				typedef thisType This

#ifdef ERROR
#undef ERROR
#endif
#define ERROR(str) wxLogError(str)

#ifdef LOG
#undef LOG
#endif
#define LOG(str) wxLogDebug(str)

#define CALL(func, ...)		\
if (func != nullptr)		\
{							\
	func(__VA_ARGS__);		\
}