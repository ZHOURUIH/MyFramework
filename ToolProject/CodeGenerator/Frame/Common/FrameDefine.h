#ifndef _SERVER_DEFINE_H_
#define _SERVER_DEFINE_H_

// 平台标识宏
#define PLATFORM_WINDOWS 0
#define PLATFORM_LINUX 1
#define PLATFORM_ANDROID PLATFORM_LINUX

// 正在运行的平台标识
#ifdef WINDOWS
#define RUN_PLATFORM PLATFORM_WINDOWS
#endif

#ifdef LINUX
#define RUN_PLATFORM PLATFORM_LINUX
#endif

#ifndef RUN_PLATFORM
#define RUN_PLATFORM -1
#error "wrong platform!"
#endif

#if RUN_PLATFORM == PLATFORM_WINDOWS
#pragma comment(lib, "ws2_32.lib")
#pragma comment(lib, "winmm.lib")
#pragma warning(disable: 4005)
// libevent的头文件只能在windows库文件之前包含,否则会有定义冲突的报错
// 部分平台未安装libevent,所以需要使用宏来判断是否需要编译libevent相关代码
#ifdef _LIBEVENT
#include "event2/event.h"
#include "event2/buffer.h"
#include "event2/http.h"
#endif
#endif

#if RUN_PLATFORM == PLATFORM_WINDOWS
#include <windows.h>
#include <mmsystem.h>
#include <iostream>
#include <io.h>
#include <direct.h>
#include <winsock.h>
#include <tlhelp32.h>
#include <dbghelp.h>
#endif
#if RUN_PLATFORM == PLATFORM_LINUX
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
#endif
#include <string>
#include <map>
#include <vector>
#include <set>
#include <list>
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
#include <array>
#include <functional>
// 部分平台未安装mysql,所以需要使用宏来判断是否需要编译mysql相关代码
#ifdef _MYSQL
#include <mysql.h>
#endif

#include "sqlite3.h"
#include "md5.h"
#include "ServerEnum.h"
#ifdef BUILDING_LIBCURL
#include "curl/curl.h"
#endif
#if RUN_PLATFORM == PLATFORM_LINUX
#ifdef _LIBEVENT
#include "event2/event.h"
#include "event2/buffer.h"
#include "event2/http.h"
#endif
#endif

using namespace std;

//-----------------------------------------------------------------------------------------------------------------------------------------------
// 宏定义
#if RUN_PLATFORM == PLATFORM_WINDOWS
#define MY_THREAD HANDLE
#define MY_SOCKET SOCKET
#define NULL_THREAD NULL
#define THREAD_CALLBACK_DECLEAR(func) static DWORD WINAPI func(LPVOID args)
#define THREAD_CALLBACK(class, func) DWORD WINAPI class##::##func(LPVOID args)
#define CREATE_THREAD(thread, func, args) thread = CreateThread(NULL, 0, func, args, 0, NULL)
#define CLOSE_THREAD(thread)	\
if (thread != NULL_THREAD)		\
{								\
	TerminateThread(thread, 0);	\
	CloseHandle(thread);		\
	thread = NULL_THREAD;		\
}
#define CLOSE_SOCKET(socket) closesocket(socket);
#define CLASS_NAME(T) string(typeid(T).name()).substr(strlen("class "))
#elif RUN_PLATFORM == PLATFORM_LINUX
#define MY_THREAD pthread_t
#define MY_SOCKET unsigned int
#define NULL_THREAD 0
#define SOCKADDR_IN sockaddr_in
#define THREAD_CALLBACK_DECLEAR(func) static void* func(void* args)
#define THREAD_CALLBACK(class, func) void* class##::##func(void* args)
#define CREATE_THREAD(thread, func, args) pthread_create(&thread, NULL, func, args);
#define CLOSE_THREAD(thread)	\
if (thread != NULL_THREAD)		\
{								\
	pthread_cancel(thread);		\
	thread = NULL_THREAD;		\
}
#define CLOSE_SOCKET(socket) close(socket);
#ifdef __GNUC__
#define CSET_GBK    "GBK"
#define CSET_UTF8   "UTF-8"
#define LC_NAME_zh_CN   "zh_CN"
#endif
#define LC_NAME_zh_CN_GBK       LC_NAME_zh_CN "." CSET_GBK
#define LC_NAME_zh_CN_UTF8      LC_NAME_zh_CN "." CSET_UTF8
#define LC_NAME_zh_CN_DEFAULT   LC_NAME_zh_CN_GBK
#define CLASS_NAME(T) removePreNumber(typeid(T).name())
#endif

#if RUN_PLATFORM == PLATFORM_WINDOWS
#define SPRINTF(buffer, bufferSize, ...) sprintf_s(buffer, bufferSize, __VA_ARGS__)
#define MEMCPY(dest, bufferSize, src, count) memcpy_s((void*)(dest), bufferSize, (void*)(src), count)
#elif RUN_PLATFORM == PLATFORM_LINUX
#define SPRINTF(buffer, bufferSize, ...) sprintf(buffer, __VA_ARGS__)
#define MEMCPY(dest, bufferSize, src, count) memcpy(dest, src, count)
#endif

#ifndef INVALID_SOCKET
#define INVALID_SOCKET (unsigned int)~0
#endif

#ifdef _DEBUG
#define CAST dynamic_cast
#else
#define CAST static_cast
#endif

#define STR(t) #t
#define LINE_STR(v) STR(v)
// 设置value的指定位置pos的字节的值为byte,并且不影响其他字节
#define SET_BYTE(value, b, pos) value = (value & ~(0x000000FF << (8 * pos))) | (b << (8 * pos))
// 获得value的指定位置pos的字节的值
#define GET_BYTE(value, pos) (value & (0x000000FF << (8 * pos))) >> (8 * pos)
#define GET_BIT(value, pos) (((value & (1 << (pos))) >> (pos)) & 1)
#define SET_BIT(value, pos, bit) value = value & ~(1 << (pos)) | ((bit) << (pos))
#define GET_HIGHEST_BIT(value) GET_BIT(value, sizeof(value) * 8 - 1)
#define SET_HIGHEST_BIT(value, bit) SET_BIT(value, sizeof(value) * 8 - 1, bit)
// 是否为偶数
#define IS_DOUBLE(value) value & 1 == 0
#define IS_POW2(value) value & (value - 1) == 0
#define _FILE_LINE_ "File : " + string(__FILE__) + ", Line : " + LINE_STR(__LINE__)
// 通过定义多个宏的方式,改变宏的展开顺序,从而使__LINE__宏先展开,再进行拼接,达到自动根据行号定义一个唯一标识符的功能
#define MAKE_LABEL2(label, L) label##L
#define MAKE_LABEL1(label, L) MAKE_LABEL2(label, L)
#define UNIQUE_IDENTIFIER(label) MAKE_LABEL1(label, __LINE__)
// 创建一个消息对象
#define PACKET(classType, packet) classType* packet = mNetServer->createPacket(packet, NAME(classType))
#define PACKET_DONT_DESTROY(classType, packet) classType* packet = mNetServer->createPacket(packet, NAME(classType));packet->setAutoDestroy(false)

#define FOREACH(iter, stl)\
auto iter = stl.begin();\
auto iter##End = stl.end();\
for(; iter != iter##End; ++iter)

#define FOR_VECTOR(stl)			const int UNIQUE_IDENTIFIER(Count) = (stl).size(); for(int i = 0; i < UNIQUE_IDENTIFIER(Count); ++i)
#define FOR_VECTOR_J(stl)		const int UNIQUE_IDENTIFIER(Count) = (stl).size(); for(int j = 0; j < UNIQUE_IDENTIFIER(Count); ++j)
#define FOR_VECTOR_INVERSE(stl) for(int i = (stl).size() - 1; i >= 0; --i)

// 简单的for循环
#define FOR_I(count)			for (int i = 0; i < (int)count; ++i)
#define FOR_J(count)			for (int j = 0; j < (int)count; ++j)
#define FOR_K(count)			for (int k = 0; k < (int)count; ++k)
#define FOR_INVERSE_I(count)	for (int i = (int)count - 1; i >= 0; --i)
#define FOR_ONCE				for (int UNIQUE_IDENTIFIER(temp) = 0; UNIQUE_IDENTIFIER(temp) < 1; ++UNIQUE_IDENTIFIER(temp))

// 基础数据类型转字符串
#define INT_TO_STRING(strBuffer, value)\
char strBuffer[16];\
intToString(strBuffer, 16, value);

#define INT_TO_STR(strBuffer, value)\
array<char, 16> strBuffer;\
intToString(strBuffer, value);

#define FLOAT_TO_STRING(strBuffer, value)\
char strBuffer[16];\
floatToString(strBuffer, 16, value);

#define FLOAT_TO_STR(strBuffer, value)\
array<char, 16> strBuffer;\
floatToString(strBuffer, value);

#define ULLONG_TO_STRING(strBuffer, value)\
char strBuffer[32];\
ullongToString(strBuffer, 32, value);

#define ULLONG_TO_STR(strBuffer, value)\
array<char, 32> strBuffer;\
ullongToString(strBuffer, value);

#define INT_ARRAY_TO_STRING(strBuffer, valueArray, count)\
char strBuffer[16 * count];\
intArrayToString(strBuffer, 16 * count, valueArray, count);

#define INT_ARRAY_TO_STR(strBuffer, valueArray, count)\
array<char, 16 * count> strBuffer;\
intArrayToString(strBuffer, valueArray, count);

// 字符串拼接,将str0,str1等字符串拼接后放入charArray中,会覆盖charArray中的内容
// charArray为array<char, int>类型
#define STRCAT2(charArray, str0, str1)\
charArray[0] = '\0';\
StringUtility::strcat_s(charArray, str0);\
StringUtility::strcat_s(charArray, str1);

#define STRCAT3(charArray, str0, str1, str2)\
charArray[0] = '\0';\
StringUtility::strcat_s(charArray, str0);\
StringUtility::strcat_s(charArray, str1);\
StringUtility::strcat_s(charArray, str2);

#define STRCAT4(charArray, str0, str1, str2, str3)\
charArray[0] = '\0';\
StringUtility::strcat_s(charArray, str0);\
StringUtility::strcat_s(charArray, str1);\
StringUtility::strcat_s(charArray, str2);\
StringUtility::strcat_s(charArray, str3);

#define STRCAT5(charArray, str0, str1, str2, str3, str4)\
charArray[0] = '\0';\
StringUtility::strcat_s(charArray, str0);\
StringUtility::strcat_s(charArray, str1);\
StringUtility::strcat_s(charArray, str2);\
StringUtility::strcat_s(charArray, str3);\
StringUtility::strcat_s(charArray, str4);

#define STRCAT6(charArray, str0, str1, str2, str3, str4, str5)\
charArray[0] = '\0';\
StringUtility::strcat_s(charArray, str0);\
StringUtility::strcat_s(charArray, str1);\
StringUtility::strcat_s(charArray, str2);\
StringUtility::strcat_s(charArray, str3);\
StringUtility::strcat_s(charArray, str4);\
StringUtility::strcat_s(charArray, str5);

#define STRCAT7(charArray, str0, str1, str2, str3, str4, str5, str6)\
charArray[0] = '\0';\
StringUtility::strcat_s(charArray, str0);\
StringUtility::strcat_s(charArray, str1);\
StringUtility::strcat_s(charArray, str2);\
StringUtility::strcat_s(charArray, str3);\
StringUtility::strcat_s(charArray, str4);\
StringUtility::strcat_s(charArray, str5);\
StringUtility::strcat_s(charArray, str6);

#define STRCAT8(charArray, str0, str1, str2, str3, str4, str5, str6, str7)\
charArray[0] = '\0';\
StringUtility::strcat_s(charArray, str0);\
StringUtility::strcat_s(charArray, str1);\
StringUtility::strcat_s(charArray, str2);\
StringUtility::strcat_s(charArray, str3);\
StringUtility::strcat_s(charArray, str4);\
StringUtility::strcat_s(charArray, str5);\
StringUtility::strcat_s(charArray, str6);\
StringUtility::strcat_s(charArray, str7);

// 字符串拼接,将str0,str1等字符串拼接后放入charArray中,会覆盖charArray中的内容
// charArray为char[]类型,_N表示普通数组
#define STRCAT2_N(charArray, size, str0, str1)\
charArray[0] = '\0';\
StringUtility::strcat_s(charArray, size, str0);\
StringUtility::strcat_s(charArray, size, str1);

#define STRCAT3_N(charArray, size, str0, str1, str2)\
charArray[0] = '\0';\
StringUtility::strcat_s(charArray, size, str0);\
StringUtility::strcat_s(charArray, size, str1);\
StringUtility::strcat_s(charArray, size, str2);

#define STRCAT4_N(charArray, size, str0, str1, str2, str3)\
charArray[0] = '\0';\
StringUtility::strcat_s(charArray, size, str0);\
StringUtility::strcat_s(charArray, size, str1);\
StringUtility::strcat_s(charArray, size, str2);\
StringUtility::strcat_s(charArray, size, str3);

#define STRCAT5_N(charArray, size, str0, str1, str2, str3, str4)\
charArray[0] = '\0';\
StringUtility::strcat_s(charArray, size, str0);\
StringUtility::strcat_s(charArray, size, str1);\
StringUtility::strcat_s(charArray, size, str2);\
StringUtility::strcat_s(charArray, size, str3);\
StringUtility::strcat_s(charArray, size, str4);

#define STRCAT6_N(charArray, size, str0, str1, str2, str3, str4, str5)\
charArray[0] = '\0';\
StringUtility::strcat_s(charArray, size, str0);\
StringUtility::strcat_s(charArray, size, str1);\
StringUtility::strcat_s(charArray, size, str2);\
StringUtility::strcat_s(charArray, size, str3);\
StringUtility::strcat_s(charArray, size, str4);\
StringUtility::strcat_s(charArray, size, str5);

#define STRCAT7_N(charArray, size, str0, str1, str2, str3, str4, str5, str6)\
charArray[0] = '\0';\
StringUtility::strcat_s(charArray, size, str0);\
StringUtility::strcat_s(charArray, size, str1);\
StringUtility::strcat_s(charArray, size, str2);\
StringUtility::strcat_s(charArray, size, str3);\
StringUtility::strcat_s(charArray, size, str4);\
StringUtility::strcat_s(charArray, size, str5);\
StringUtility::strcat_s(charArray, size, str6);

#define STRCAT8_N(charArray, size, str0, str1, str2, str3, str4, str5, str6, str7)\
charArray[0] = '\0';\
StringUtility::strcat_s(charArray, size, str0);\
StringUtility::strcat_s(charArray, size, str1);\
StringUtility::strcat_s(charArray, size, str2);\
StringUtility::strcat_s(charArray, size, str3);\
StringUtility::strcat_s(charArray, size, str4);\
StringUtility::strcat_s(charArray, size, str5);\
StringUtility::strcat_s(charArray, size, str6);\
StringUtility::strcat_s(charArray, size, str7);

// 字符串拼接,将str0,str1等字符串拼接在charArray中的字符串后面,不会覆盖charArray的内容
// charArray为array<char, int>类型
#define STR_APPEND1(charArray, str0)\
StringUtility::strcat_s(charArray, str0);

#define STR_APPEND2(charArray, str0, str1)\
StringUtility::strcat_s(charArray, str0); \
StringUtility::strcat_s(charArray, str1);

#define STR_APPEND3(charArray, str0, str1, str2)\
StringUtility::strcat_s(charArray, str0); \
StringUtility::strcat_s(charArray, str1); \
StringUtility::strcat_s(charArray, str2);

#define STR_APPEND4(charArray, str0, str1, str2, str3)\
StringUtility::strcat_s(charArray, str0); \
StringUtility::strcat_s(charArray, str1); \
StringUtility::strcat_s(charArray, str2); \
StringUtility::strcat_s(charArray, str3);

#define STR_APPEND5(charArray, str0, str1, str2, str3, str4)\
StringUtility::strcat_s(charArray, str0); \
StringUtility::strcat_s(charArray, str1); \
StringUtility::strcat_s(charArray, str2); \
StringUtility::strcat_s(charArray, str3); \
StringUtility::strcat_s(charArray, str4);

#define STR_APPEND6(charArray, str0, str1, str2, str3, str4, str5)\
StringUtility::strcat_s(charArray, str0); \
StringUtility::strcat_s(charArray, str1); \
StringUtility::strcat_s(charArray, str2); \
StringUtility::strcat_s(charArray, str3); \
StringUtility::strcat_s(charArray, str4); \
StringUtility::strcat_s(charArray, str5);

// 字符串拼接,将str0,str1等字符串拼接在charArray中的字符串后面,不会覆盖charArray的内容
// charArray为char[]类型,_N表示普通数组
#define STR_APPEND1_N(charArray, size, str0)\
StringUtility::strcat_s(charArray, size, str0);

#define STR_APPEND2_N(charArray, size, str0, str1)\
StringUtility::strcat_s(charArray, size, str0); \
StringUtility::strcat_s(charArray, size, str1);

#define STR_APPEND3_N(charArray, size, str0, str1, str2)\
StringUtility::strcat_s(charArray, size, str0); \
StringUtility::strcat_s(charArray, size, str1); \
StringUtility::strcat_s(charArray, size, str2);

#define STR_APPEND4_N(charArray, size, str0, str1, str2, str3)\
StringUtility::strcat_s(charArray, size, str0); \
StringUtility::strcat_s(charArray, size, str1); \
StringUtility::strcat_s(charArray, size, str2); \
StringUtility::strcat_s(charArray, size, str3);

#define STR_APPEND5_N(charArray, size, str0, str1, str2, str3, str4)\
StringUtility::strcat_s(charArray, size, str0); \
StringUtility::strcat_s(charArray, size, str1); \
StringUtility::strcat_s(charArray, size, str2); \
StringUtility::strcat_s(charArray, size, str3); \
StringUtility::strcat_s(charArray, size, str4);

#define STR_APPEND6_N(charArray, size, str0, str1, str2, str3, str4, str5)\
StringUtility::strcat_s(charArray, size, str0); \
StringUtility::strcat_s(charArray, size, str1); \
StringUtility::strcat_s(charArray, size, str2); \
StringUtility::strcat_s(charArray, size, str3); \
StringUtility::strcat_s(charArray, size, str4); \
StringUtility::strcat_s(charArray, size, str5);

#ifdef ERROR
#undef ERROR
#endif
#define ERROR(info) std::cout << info << " File:" << __FILE__ << ", Line:" << __LINE__ << std::endl;FrameDefine::mHasError = true
#define LOG(info) std::cout << info << std::endl

typedef unsigned char byte;
typedef unsigned short ushort;
typedef unsigned int uint;
typedef unsigned long ulong;
typedef long long llong;
typedef unsigned long long ullong;

// 最大并发连接数为64
#ifdef FD_SETSIZE
#undef FD_SETSIZE
#endif
#define FD_SETSIZE 64

#ifdef LOCK
#undef LOCK
#endif
#define LOCK(l) \
(l).waitForUnlock(__FILE__, __LINE__);\
try\
{

#ifdef UNLOCK
#undef UNLOCK
#endif
#define UNLOCK(l) \
}catch(...){}\
(l).unlock()

//内存相关宏定义
//---------------------------------------------------------------------------------------------------------------------
#ifdef CHECK_MEMORY
// 带内存合法检测的常规内存申请和释放
#define NORMAL_NEW(className, ptr, ...)	\
NULL;									\
ptr = new className(__VA_ARGS__);		\
if(ptr != NULL)							\
{										\
	txMemoryCheck::usePtr(ptr);			\
}										\
else									\
{										\
	ERROR(string("can not alloc memory! ") + "className : " + STR(className));	\
}

#define NORMAL_NEW_ARRAY(className, count, ptr)		\
NULL;												\
ptr = new className[count];							\
if (ptr != NULL)									\
{													\
	/*如果是char数组类型,则需要将数组清零*/			\
	if (typeid(className).hash_code() == BinaryUtility::mCharType)\
	{												\
		memset(ptr, 0, sizeof(className) * count);	\
	}												\
	txMemoryCheck::usePtr(ptr);						\
}													\
else												\
{													\
	ERROR(string("can not alloc memory array! ") + "className : " + STR(className) + ", count : " + StringUtility::intToString(count));	\
}

#define NORMAL_DELETE(ptr)			\
if (ptr != NULL)					\
{									\
	txMemoryCheck::unusePtr(ptr);	\
	delete ptr;						\
	ptr = NULL;						\
}

#define NORMAL_DELETE_ARRAY(ptr)	\
if (ptr != NULL)					\
{									\
	txMemoryCheck::unusePtr(ptr);	\
	delete[] ptr;					\
	ptr = NULL;						\
}
#else
// 不带内存合法检测的常规内存申请和释放
#define NORMAL_NEW(className, ptr, ...)			\
NULL;											\
ptr = new className(__VA_ARGS__);				\
if(ptr == NULL)									\
{												\
	ERROR(string("can not alloc memory! ") + "className : " + STR(className));\
}

#define NORMAL_NEW_ARRAY(className, count, ptr)		\
NULL;												\
ptr = new className[count];							\
if(ptr != NULL)										\
{													\
	/*如果是char数组类型,则需要将数组清零*/			\
	if (typeid(className).hash_code() == BinaryUtility::mCharType)\
	{												\
		memset(ptr, 0, sizeof(className) * count);	\
	}												\
}													\
else												\
{													\
	ERROR(string("can not alloc memory array! ") + "className : " + STR(className) + ", count : " + StringUtility::intToString(count));	\
}

#define NORMAL_DELETE(ptr)	\
if (ptr != NULL)			\
{							\
	delete ptr;				\
	ptr = NULL;				\
}

#define NORMAL_DELETE_ARRAY(ptr)\
if (ptr != NULL)				\
{								\
	delete[] ptr;				\
	ptr = NULL;					\
}
#endif

#ifdef NEW
#undef NEW
#endif

#ifdef DELETE
#undef DELETE
#endif

#ifdef NEW_ARRAY
#undef NEW_ARRAY
#endif

#ifdef DELETE_ARRAY
#undef DELETE_ARRAY
#endif

#ifdef TRACE_MEMORY
// 申请无参或者带参构造类的内存
#define NEW(className, ptr, ...)			\
NORMAL_NEW(className, ptr, __VA_ARGS__)		\
if(ptr != NULL)								\
{											\
	txMemoryTrace::insertPtr(ptr, MemoryInfo(sizeof(className), __FILE__, __LINE__, typeid(className).name())); \
}

// 申请无参构造的类或者基础数据类型数组内存
#define NEW_ARRAY(className, count, ptr)		\
NORMAL_NEW_ARRAY(className, count, ptr)			\
if(ptr != NULL)									\
{												\
	txMemoryTrace::insertPtr(ptr, MemoryInfo(sizeof(className)* count, __FILE__, __LINE__, typeid(className).name())); \
}

// 释放TRACE_NEW申请的内存
#define DELETE(ptr)					\
txMemoryTrace::erasePtr((void*)ptr);\
NORMAL_DELETE(ptr)

// 释放TRACE_NEW_ARRAY申请的内存
#define DELETE_ARRAY(ptr)			\
txMemoryTrace::erasePtr((void*)ptr);\
NORMAL_DELETE_ARRAY(ptr)
#else
#define NEW(className, ptr, ...)			NORMAL_NEW(className, ptr, __VA_ARGS__)
#define NEW_ARRAY(className, count, ptr)	NORMAL_NEW_ARRAY(className, count, ptr)
#define DELETE(ptr)							NORMAL_DELETE(ptr)
#define DELETE_ARRAY(ptr)					NORMAL_DELETE_ARRAY(ptr)
#endif
//------------------------------------------------------------------------------------------------------------------------------------
// 自定义的基础头文件
#include "myVector.h"
#include "myMap.h"
#include "mySet.h"
#include "Vector2.h"
#include "Vector2Int.h"
#include "Vector2Short.h"
#include "Vector2UShort.h"
#include "Vector3.h"
#include "Vector4.h"
#include "Vector4Int.h"
#include "Color.h"

using namespace std;

//-------------------------------------------------------------------------------------------------------------------------------------------------------
// 结构体定义
//-------------------------------------------------------------------------------------------------------------------------------------------------------
class FrameDefine
{
public:
	// 常量数字定义
	static const uint CLIENT_TEMP_BUFFER = 8 * 1024;				// 客户端临时缓冲区大小,应该不小于单个消息包最大的大小
	static const uint CLIENT_SEND_BUFFER = 1 * 1024 * 1024;			// 客户端发送数据缓冲区大小,1MB
	static const uint CLIENT_RECV_BUFFER = 1 * 1024 * 1024;			// 客户端接收数据缓冲区大小,1MB
	static const uint SERVER_RECV_BUFFER = 2 * 1024 * 1024;			// 从socket接收数据时使用的缓冲区,2MB
	static const uint HEADER_SIZE = sizeof(uint) + sizeof(ushort);	// 消息包头大小
	// 只有成功解析5个消息包以后的客户端才认为是有效客户端,当无效客户端接收到错误消息时直接断开连接并且不报错
	static const uint MIN_PARSE_COUNT = 5;
	// 常量字符串定义
	static const string MEDIA_PATH;
	static const string MAP_PATH;
	static const string CONFIG_PATH;
	static const string LOG_PATH;
	static const string EMPTY_STRING;
	static const char* DESTROY_CHARACTER_STATE;
	static const char* ZERO_ONE;
	static bool mHasError;
};

#endif