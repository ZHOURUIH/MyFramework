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
#include <windows.h>
#include <mmsystem.h>
#include <iostream>
#include <io.h>
#include <direct.h>
#include <winsock.h>
#endif
#if RUN_PLATFORM == PLATFORM_LINUX
#include <netinet/in.h>
#include <netinet/tcp.h>
#include <sys/socket.h>
#include <sys/epoll.h>
#include <sys/un.h>
#include <sys/time.h>
#include <arpa/inet.h>
#include <unistd.h>
#include <errno.h>
#include <fcntl.h>
#include <netdb.h>
#include <stdarg.h>
#include <signal.h>
#include <dirent.h>
#include <pthread.h>
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
#include <atomic>
#include <array>

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
#define SPRINTF(buffer, bufferSize, ...) sprintf_s(buffer, bufferSize, __VA_ARGS__)
#define MEMCPY(dest, bufferSize, src, count) memcpy_s((void*)(dest), bufferSize, (void*)(src), count)
// 获取不同平台下的字面常量字符串的UTF8编码字符串,只能处理字面常量,也就是在代码中写死的字符串
// windows下需要由GB2312转换为UTF8,而linux则直接就是UTF8的
// 而且不能将返回结果存储为变量,因为只是一个临时的返回结果
#define UNIFIED_UTF8(constantString) StringUtility::ANSIToUTF8(constantString).c_str()
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
#define SPRINTF(buffer, bufferSize, ...) sprintf(buffer, __VA_ARGS__)
// 因为bufferSize在windows下是需要的,linux下不需要,所以为了避免警告,仍然使用此参数参与,但是不产生任何影响
#define MEMCPY(dest, bufferSize, src, count) memcpy((void*)((char*)dest + (bufferSize) - (bufferSize)), (void*)(src), count)
#define UNIFIED_UTF8(constantString) constantString
#endif

#ifndef INVALID_SOCKET
#define INVALID_SOCKET (unsigned int)~0
#endif

#define INVALID_ID (unsigned int)~0
#define INVALID_INT_ID -1

#ifndef NULL
#define NULL 0
#endif

#define NOT_FIND static_cast<size_t>(-1)

// 将类的基类重命名为base,方便使用
#define BASE_CLASS(baseClass) typedef baseClass base

// 再次封装后的容器的遍历宏
#ifdef _DEBUG
// 需要在循环结束后添加END
#define FOR(stl, expression) stl.lock(STL_LOCK::WRITE, __FILE__, __LINE__);for (expression)
#define END(stl) stl.unlock(STL_LOCK::WRITE);
#else
#define FOR(stl, expression)for (expression)
#define END(stl)
#endif

// 使用迭代器遍历列表,要在循环结束后添加END
#define FOREACH(iter, stl)\
auto iter = stl.begin();\
auto iter##End = stl.end();\
FOR(stl, ; iter != iter##End; ++iter)

// 使用迭代器遍历列表,不需要在循环结束后添加END
#define FOREACH_CONST(iter, stl)\
auto iter = stl.cbegin();\
auto iter##End = stl.cend();\
for(; iter != iter##End; ++iter)

// 使用下标遍历列表,需要在循环结束后添加END
#define FOR_VECTOR(stl) int stl##Count = (int)stl.size(); FOR(stl, int i = 0; i < stl##Count; ++i)
#define FOR_VECTOR_J(stl) int stl##Count = (int)stl.size(); FOR(stl, int j = 0; j < stl##Count; ++j)
#define FOR_VECTOR_K(stl) int stl##Count = (int)stl.size(); FOR(stl, int k = 0; k < stl##Count; ++k)
#define FOR_VECTOR_INVERSE(stl) int stl##Count = (int)stl.size(); FOR(stl, int i = stl##Count - 1; i >= 0; --i)
// 使用下标遍历常量列表,不需要在循环结束后添加END
#define FOR_VECTOR_CONST(stl) int stl##Count = (int)stl.size(); for(int i = 0; i < stl##Count; ++i)
#define FOR_VECTOR_CONST_J(stl) int stl##Count = (int)stl.size(); for(int j = 0; j < stl##Count; ++j)
#define FOR_VECTOR_CONST_K(stl) int stl##Count = (int)stl.size(); for(int k = 0; k < stl##Count; ++k)
#define FOR_VECTOR_CONST_INVERSE(stl) uint stl##Count = stl.size(); for(stl, int i = stl##Count - 1; i >= 0; --i)
// 简单的for循环
#define FOR_I(count) for (int i = 0; i < (int)count; ++i)
#define FOR_J(count) for (int j = 0; j < (int)count; ++j)
#define FOR_K(count) for (int k = 0; k < (int)count; ++k)
#define FOR_X(count) for (int x = 0; x < (int)count; ++x)
#define FOR_Y(count) for (int y = 0; y < (int)count; ++y)
#define FOR_INVERSE_I(count) for (int i = count - 1; i >= 0; --i)
#define FOR_INVERSE_J(count) for (int j = count - 1; j >= 0; --j)
#define FOR_INVERSE_K(count) for (int k = count - 1; k >= 0; --k)
#define FOR_ONCE for(byte tempI = 0; tempI < 1; ++tempI)

#define STR(t) #t
#define LINE_STR(v) STR(v)
// 设置value的指定位置pos的字节的值为byte,并且不影响其他字节
#define SET_BYTE(value, byte, pos) value = (value & ~(0x000000ff << (8 * pos))) | (byte << (8 * pos))
// 获得value的指定位置pos的字节的值
#define GET_BYTE(value, pos) (value & (0x000000ff << (8 * pos))) >> (8 * pos)
#define GET_BIT(value, pos) (((value & (1 << (pos))) >> (pos)) & 1)
#define SET_BIT(value, pos, bit) value = value & ~(1 << (pos)) | (bit << (pos))
#define GET_HIGHEST_BIT(value) GET_BIT(value, sizeof(value) * 8 - 1)
#define SET_HIGHEST_BIT(value, bit) SET_BIT(value, sizeof(value) * 8 - 1, bit);
#define _FILE_LINE_ "File : " + string(__FILE__) + ", Line : " + LINE_STR(__LINE__)
#define NEW_PACKET(packet, type) NetServer::createPacket(packet, type);

// 基础数据类型转字符串
// 以_STR结尾的是构造一个char[]类型的字符串
#define INT_STR(strBuffer, value) \
char strBuffer[16];\
StringUtility::intToString(strBuffer, 16, value);

#define FLOAT_STR(strBuffer, value) \
char strBuffer[16];\
StringUtility::floatToString(strBuffer, 16, value);

#define ULLONG_STR(strBuffer, value)\
 char strBuffer[32];\
StringUtility::ullongToString(strBuffer, 32, value);

#define INTS_STR(strBuffer, valueArray, bufferCount, count) \
char strBuffer[16 * bufferCount];\
StringUtility::intsToString(strBuffer, 16 * bufferCount, valueArray, count);

#define FLOATS_STR(strBuffer, valueArray, bufferCount, count) \
char strBuffer[16 * bufferCount];\
StringUtility::floatsToString(strBuffer, 16 * bufferCount, valueArray, count);

#define ULLONGS_STR(strBuffer, valueArray, bufferCount, count) \
char strBuffer[20 * bufferCount];\
StringUtility::ullongsToString(strBuffer, 20 * bufferCount, valueArray, count);

// 以_CHARS结尾的构造出array<char, SIZE>类型的字符串
#define INT_CHARS(strBuffer, value) \
array<char, 16> strBuffer{0};\
StringUtility::intToString(strBuffer, value);

#define FLOAT_CHARS(strBuffer, value) \
array<char, 16> strBuffer{0};\
StringUtility::floatToString(strBuffer, value);

#define ULLONG_CHARS(strBuffer, value) \
array<char, 32> strBuffer{0};\
StringUtility::ullongToString(strBuffer, value);

#define INTS_CHARS(strBuffer, valueArray, bufferCount, count) \
array<char, 16 * bufferCount> strBuffer{0};\
StringUtility::intsToString(strBuffer, valueArray, count);

#define FLOATS_CHARS(strBuffer, valueArray, bufferCount, count) \
array<char, 16 * bufferCount> strBuffer{0};\
StringUtility::floatsToString(strBuffer, valueArray, count);

#define ULLONGS_CHARS(strBuffer, valueArray, bufferCount, count) \
array<char, 32 * bufferCount> strBuffer{0};\
StringUtility::ullongsToString(strBuffer, valueArray, count);

// 字符串拼接,将str0,str1等字符串拼接后放入charArray中,会覆盖charArray中的内容
// charArray为array<char, int>类型
#define STRCAT2(charArray, str0, str1)\
charArray[0] = '\0';\
const char* sourceArray[]{str0, str1};\
StringUtility::strcat_s(charArray, sourceArray, 2);

#define STRCAT3(charArray, str0, str1, str2)\
charArray[0] = '\0';\
const char* sourceArray[]{str0, str1, str2};\
StringUtility::strcat_s(charArray, sourceArray, 3);

#define STRCAT4(charArray, str0, str1, str2, str3)\
charArray[0] = '\0';\
const char* sourceArray[]{str0, str1, str2, str3};\
StringUtility::strcat_s(charArray, sourceArray, 4);

#define STRCAT5(charArray, str0, str1, str2, str3, str4)\
charArray[0] = '\0';\
const char* sourceArray[]{str0, str1, str2, str3, str4};\
StringUtility::strcat_s(charArray, sourceArray, 5);

#define STRCAT6(charArray, str0, str1, str2, str3, str4, str5)\
charArray[0] = '\0';\
const char* sourceArray[]{str0, str1, str2, str3, str4, str5};\
StringUtility::strcat_s(charArray, sourceArray, 6);

#define STRCAT7(charArray, str0, str1, str2, str3, str4, str5, str6)\
charArray[0] = '\0';\
const char* sourceArray[]{str0, str1, str2, str3, str4, str5, str6};\
StringUtility::strcat_s(charArray, sourceArray, 7);

#define STRCAT8(charArray, str0, str1, str2, str3, str4, str5, str6, str7)\
charArray[0] = '\0';\
const char* sourceArray[]{str0, str1, str2, str3, str4, str5, str6, str7};\
StringUtility::strcat_s(charArray, sourceArray, 8);

#define STRCAT9(charArray, str0, str1, str2, str3, str4, str5, str6, str7, str8)\
charArray[0] = '\0';\
const char* sourceArray[]{str0, str1, str2, str3, str4, str5, str6, str7, str8};\
StringUtility::strcat_s(charArray, sourceArray, 9);

// 字符串拼接,将str0,str1等字符串拼接后放入charArray中,会覆盖charArray中的内容
// charArray为char[]类型,_N表示普通数组
#define STRCAT2_N(charArray, size, str0, str1)\
charArray[0] = '\0';\
const char* sourceArray[]{str0, str1};\
StringUtility::strcat_s(charArray, size, sourceArray, 2);

#define STRCAT3_N(charArray, size, str0, str1, str2)\
charArray[0] = '\0';\
const char* sourceArray[]{str0, str1, str2};\
StringUtility::strcat_s(charArray, size, sourceArray, 3);

#define STRCAT4_N(charArray, size, str0, str1, str2, str3)\
charArray[0] = '\0';\
const char* sourceArray[]{str0, str1, str2, str3};\
StringUtility::strcat_s(charArray, size, sourceArray, 4);

#define STRCAT5_N(charArray, size, str0, str1, str2, str3, str4)\
charArray[0] = '\0';\
const char* sourceArray[]{str0, str1, str2, str3, str4};\
StringUtility::strcat_s(charArray, size, sourceArray, 5);

#define STRCAT6_N(charArray, size, str0, str1, str2, str3, str4, str5)\
charArray[0] = '\0';\
const char* sourceArray[]{str0, str1, str2, str3, str4, str5};\
StringUtility::strcat_s(charArray, size, sourceArray, 6);

#define STRCAT7_N(charArray, size, str0, str1, str2, str3, str4, str5, str6)\
charArray[0] = '\0';\
const char* sourceArray[]{str0, str1, str2, str3, str4, str5, str6};\
StringUtility::strcat_s(charArray, size, sourceArray, 7);

#define STRCAT8_N(charArray, size, str0, str1, str2, str3, str4, str5, str6, str7)\
charArray[0] = '\0';\
const char* sourceArray[]{str0, str1, str2, str3, str4, str5, str6, str7};\
StringUtility::strcat_s(charArray, size, sourceArray, 8);

#define STRCAT9_N(charArray, size, str0, str1, str2, str3, str4, str5, str6, str7, str8)\
charArray[0] = '\0';\
const char* sourceArray[]{str0, str1, str2, str3, str4, str5, str6, str7, str8};\
StringUtility::strcat_s(charArray, size, sourceArray, 9);

// 字符串拼接,将str0,str1等字符串拼接在charArray中的字符串后面,不会覆盖charArray的内容
// charArray为array<char, int>类型
#define STR_APPEND1(charArray, str0)\
const char* sourceArray[]{str0};\
StringUtility::strcat_s(charArray, sourceArray, 1);

#define STR_APPEND2(charArray, str0, str1)\
const char* sourceArray[]{str0, str1};\
StringUtility::strcat_s(charArray, sourceArray, 2);

#define STR_APPEND3(charArray, str0, str1, str2)\
const char* sourceArray[]{str0, str1, str2};\
StringUtility::strcat_s(charArray, sourceArray, 3);

#define STR_APPEND4(charArray, str0, str1, str2, str3)\
const char* sourceArray[]{str0, str1, str2, str3};\
StringUtility::strcat_s(charArray, sourceArray, 4);

#define STR_APPEND5(charArray, str0, str1, str2, str3, str4)\
const char* sourceArray[]{str0, str1, str2, str3, str4};\
StringUtility::strcat_s(charArray, sourceArray, 5);

#define STR_APPEND6(charArray, str0, str1, str2, str3, str4, str5)\
const char* sourceArray[]{str0, str1, str2, str3, str4, str5};\
StringUtility::strcat_s(charArray, sourceArray, 6);

#define STR_APPEND7(charArray, str0, str1, str2, str3, str4, str5, str6)\
const char* sourceArray[]{str0, str1, str2, str3, str4, str5, str6};\
StringUtility::strcat_s(charArray, sourceArray, 7);

#define STR_APPEND8(charArray, str0, str1, str2, str3, str4, str5, str6, str7)\
const char* sourceArray[]{str0, str1, str2, str3, str4, str5, str6, str7};\
StringUtility::strcat_s(charArray, sourceArray, 8);

#define STR_APPEND9(charArray, str0, str1, str2, str3, str4, str5, str6, str7, str8)\
const char* sourceArray[]{str0, str1, str2, str3, str4, str5, str6, str7, str8};\
StringUtility::strcat_s(charArray, sourceArray, 9);

// 字符串拼接,将str0,str1等字符串拼接在charArray中的字符串后面,不会覆盖charArray的内容
// charArray为char[]类型,_N表示普通数组
#define STR_APPEND1_N(charArray, size, str0)\
const char* sourceArray[]{str0};\
StringUtility::strcat_s(charArray, size, sourceArray, 1);

#define STR_APPEND2_N(charArray, size, str0, str1)\
const char* sourceArray[]{str0, str1};\
StringUtility::strcat_s(charArray, size, sourceArray, 2);

#define STR_APPEND3_N(charArray, size, str0, str1, str2)\
const char* sourceArray[]{str0, str1, str2};\
StringUtility::strcat_s(charArray, size, sourceArray, 3);

#define STR_APPEND4_N(charArray, size, str0, str1, str2, str3)\
const char* sourceArray[]{str0, str1, str2, str3};\
StringUtility::strcat_s(charArray, size, sourceArray, 4);

#define STR_APPEND5_N(charArray, size, str0, str1, str2, str3, str4)\
const char* sourceArray[]{str0, str1, str2, str3, str4};\
StringUtility::strcat_s(charArray, size, sourceArray, 5);

#define STR_APPEND6_N(charArray, size, str0, str1, str2, str3, str4, str5)\
const char* sourceArray[]{str0, str1, str2, str3, str4, str5};\
StringUtility::strcat_s(charArray, size, sourceArray, 6);

#define STR_APPEND7_N(charArray, size, str0, str1, str2, str3, str4, str5, str6)\
const char* sourceArray[]{str0, str1, str2, str3, str4, str5, str6};\
StringUtility::strcat_s(charArray, size, sourceArray, 7);

#define STR_APPEND8_N(charArray, size, str0, str1, str2, str3, str4, str5, str6, str7)\
const char* sourceArray[]{str0, str1, str2, str3, str4, str5, str6, str7};\
StringUtility::strcat_s(charArray, size, sourceArray, 8);

#define STR_APPEND9_N(charArray, size, str0, str1, str2, str3, str4, str5, str6, str7, str8)\
const char* sourceArray[]{str0, str1, str2, str3, str4, str5, str6, str7, str8};\
StringUtility::strcat_s(charArray, size, sourceArray, 9);

// 最大并发连接数为64
#ifdef FD_SETSIZE
#undef FD_SETSIZE
#define FD_SETSIZE 64
#endif

#define LOCK(l) \
l.waitForUnlock(__FILE__, __LINE__);\
try\
{

#define UNLOCK(l) \
}catch(...){}\
l.unlock()

//内存相关宏定义
//---------------------------------------------------------------------------------------------------------------------
#ifdef CHECK_MEMORY
// 带内存合法检测的常规内存申请和释放
#define NORMAL_NEW(className, ptr, ...)	\
NULL;									\
ptr = new className(__VA_ARGS__);		\
if(ptr != NULL)							\
{										\
	MemoryCheck::usePtr(ptr);			\
}										\
else									\
{										\
	/*ERROR(string("can not alloc memory! className :") + STR(className));*/\
}

#define NORMAL_NEW_ARRAY(className, count, ptr)		\
NULL;												\
if(count <= 0)										\
{													\
	/*ERROR("无法申请大小为0的数组");*/					\
}													\
ptr = new className[count];							\
if (ptr != NULL)									\
{													\
	MemoryCheck::usePtr(ptr);						\
}													\
else												\
{													\
	/*ERROR(string("can not alloc memory array! className : ") + STR(className) + ", count : " + StringUtility::intToString(count));*/\
}

#define NORMAL_DELETE(ptr)			\
if (ptr != NULL)					\
{									\
	MemoryCheck::unusePtr(ptr);	\
	delete ptr;						\
	ptr = NULL;						\
}

#define NORMAL_DELETE_ARRAY(ptr)	\
if (ptr != NULL)					\
{									\
	MemoryCheck::unusePtr(ptr);	\
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
	/*ERROR(string("can not alloc memory! className : ") + STR(className));*/\
}

#define NORMAL_NEW_ARRAY(className, count, ptr)		\
NULL;												\
if(count <= 0)										\
{													\
	/*ERROR("无法申请大小为0的数组");*/					\
}													\
ptr = new className[count];							\
if(ptr == NULL)										\
{													\
	/*ERROR(string("can not alloc memory array! className : ") + STR(className) + ", count : " + StringUtility::intToString(count));*/\
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
	MemoryTrace::insertPtr(ptr, MemoryInfo(sizeof(className), __FILE__, __LINE__, typeid(className).name())); \
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
MemoryTrace::erasePtr((void*)ptr);\
NORMAL_DELETE(ptr)

// 释放TRACE_NEW_ARRAY申请的内存
#define DELETE_ARRAY(ptr)			\
MemoryTrace::erasePtr((void*)ptr);\
NORMAL_DELETE_ARRAY(ptr)
#else
#define NEW(className, ptr, ...)			NORMAL_NEW(className, ptr, __VA_ARGS__)
#define NEW_ARRAY(className, count, ptr)	NORMAL_NEW_ARRAY(className, count, ptr)
#define DELETE(ptr)							NORMAL_DELETE(ptr)
#define DELETE_ARRAY(ptr)					NORMAL_DELETE_ARRAY(ptr)
#endif

// 基础数据类型简化定义
typedef unsigned char byte;
typedef unsigned short ushort;
typedef unsigned int uint;
typedef unsigned long ulong;
typedef unsigned long long ullong;

#include "myVector.h"
#include "myMap.h"
#include "mySet.h"
#include "myStack.h"
#include "myQueue.h"
#include "mySafeVector.h"
#include "mySafeMap.h"
#include "mySafeSet.h"
#include "Vector2.h"
#include "Vector2Int.h"
#include "Vector2UShort.h"
#include "Vector3.h"
#include "Vector4.h"
#include "Vector4Int.h"
#include "ServerCallback.h"
#include "ServerEnum.h"

//-------------------------------------------------------------------------------------------------------------------------------------------------------
// 结构体定义

//-------------------------------------------------------------------------------------------------------------------------------------------------------
// 常量数字定义
const int CLIENT_TEMP_BUFFER_SIZE = 2 * 1024;	// 客户端临时缓冲区大小,应该不小于单个消息包最大的大小
const int CLIENT_BUFFER_SIZE = 512 * 1024;		// 客户端发送和接收数据缓冲区大小
const int HEADER_SIZE = sizeof(short) + sizeof(short);

//-------------------------------------------------------------------------------------------------------------------------------------------------------
// 常量字符串定义
const string MEDIA_PATH = "../media";
const string GAME_DATA_PATH = "GameDataFile/";
const string CONFIG_PATH = "Config/";
const string LOG_PATH = "Log/";
const string EMPTY_STRING = "";

#endif