#ifndef _FRAME_MACRO_H_
#define _FRAME_MACRO_H_

// 平台标识宏
#define PLATFORM_WINDOWS 1
#define PLATFORM_LINUX 2
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

// 最大并发连接数为64,需要在winsock.h之前进行定义
#ifdef FD_SETSIZE
#undef FD_SETSIZE
#endif
#define FD_SETSIZE 64

// 由于下面定义的部分宏容易在系统头文件中被定义,从而造成编译无法通关,所以先包含系统头文件,然后再定义自己的宏
#if RUN_PLATFORM == PLATFORM_WINDOWS
// 链接静态库
#pragma comment(lib, "ws2_32.lib")
#pragma comment(lib, "winmm.lib")
#ifdef _MYSQL
#pragma comment(lib, "libmysql.lib")
#endif
#pragma warning(disable: 4005)
// libevent的头文件只能在windows库文件之前包含,否则会有定义冲突的报错
// 部分平台未安装libevent,所以需要使用宏来判断是否需要编译libevent相关代码
#ifdef _LIBEVENT
#include "event2/event.h"
#include "event2/buffer.h"
#include "event2/http.h"
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
#ifdef _LIBEVENT
#include "event2/event.h"
#include "event2/buffer.h"
#include "event2/http.h"
#endif
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
#include <functional>
// 部分平台未安装mysql,所以需要使用宏来判断是否需要编译mysql相关代码
#ifdef _MYSQL
#include <mysql.h>
#endif

#include "sqlite3/sqlite3.h"
#include "md5/md5.h"
#ifdef BUILDING_LIBCURL
#include "curl/curl.h"
#endif

//-----------------------------------------------------------------------------------------------------------------------------------------------
// 宏定义
#if RUN_PLATFORM == PLATFORM_WINDOWS
#define MY_THREAD							HANDLE
#define MY_SOCKET							SOCKET
#define NULL_THREAD							nullptr
#define THREAD_CALLBACK_DECLEAR(func)		static DWORD WINAPI func(LPVOID args)
#define THREAD_CALLBACK(class, func)		DWORD WINAPI class##::##func(LPVOID args)
#define CREATE_THREAD(thread, func, args)	thread = CreateThread(nullptr, 0, func, args, 0, nullptr)
#define CLOSE_THREAD(thread)		\
if (thread != NULL_THREAD)			\
{									\
									\
	TerminateThread(thread, 0);		\
	CloseHandle(thread);			\
	thread = NULL_THREAD;			\
}
#define CLOSE_SOCKET(socket)						closesocket(socket);
#define SPRINTF(buffer, bufferSize, ...)			sprintf_s(buffer, bufferSize, __VA_ARGS__)
#define MEMCPY(dest, bufferSize, src, count)		memcpy_s((void*)(dest), bufferSize, (void*)(src), count)
// 获取不同平台下的字面常量字符串的UTF8编码字符串,只能处理字面常量,也就是在代码中写死的字符串
// windows下需要由GB2312转换为UTF8,而linux则直接就是UTF8的
// 将一个字面常量字符串转换为UTF8后存储为变量
#define UNIFIED_UTF8(var, size, constantString)		Array<size> var{ 0 }; StringUtility::ANSIToUTF8(constantString, var.toBuffer(), size, false)
#define UNIFIED_UTF8_STRING(str, constantString)	StringUtility::ANSIToUTF8(constantString, str, false)
#elif RUN_PLATFORM == PLATFORM_LINUX
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
#define CLOSE_SOCKET(socket)				close(socket);
#ifdef __GNUC__
#define CSET_GBK							"GBK"
#define CSET_UTF8							"UTF-8"
#define LC_NAME_zh_CN						"zh_CN"
#endif
#define LC_NAME_zh_CN_GBK					LC_NAME_zh_CN "." CSET_GBK
#define LC_NAME_zh_CN_UTF8					LC_NAME_zh_CN "." CSET_UTF8
#define LC_NAME_zh_CN_DEFAULT				LC_NAME_zh_CN_GBK
#define SPRINTF(buffer, bufferSize, ...)	sprintf(buffer, __VA_ARGS__)
// 因为bufferSize在windows下是需要的,linux下不需要,所以为了避免警告,仍然使用此参数参与,但是不产生任何影响
#define MEMCPY(dest, bufferSize, src, count) memcpy((void*)((char*)dest + (bufferSize) - (bufferSize)), (void*)(src), count)
// 将一个字面常量字符串转换为UTF8后存储为变量
#define UNIFIED_UTF8(var, size, constantString)		Array<size> var{ 0 };	var.setString(constantString)
#define UNIFIED_UTF8_STRING(str, constantString)	str = constantString
#endif

#ifndef INVALID_SOCKET
#define INVALID_SOCKET (unsigned int)~0
#endif

#define CAST			static_cast
#define STR(t)			#t
#define LINE_STR(v)		STR(v)
#define _FILE_LINE_		"File : " + string(__FILE__) + ", Line : " + LINE_STR(__LINE__)
// 生成静态字符串常量的名字
#define NAME(name) STR_##name

//--------------------------------------------------------------------------------------------------------------------------------------------
// 基础数据类型转字符串
// 以_STR结尾的是构造一个char[]类型的字符串
#define INT_STR(strBuffer, value)				\
Array<16> strBuffer;							\
StringUtility::intToString(strBuffer, value);

#define UINT_STR(strBuffer, value)				\
Array<16> strBuffer;							\
StringUtility::uintToString(strBuffer, value);

#define FLOAT_STR(strBuffer, value)				\
Array<16> strBuffer;							\
StringUtility::floatToString(strBuffer, value);

#define LLONG_STR(strBuffer, value)				\
Array<32> strBuffer;							\
StringUtility::llongToString(strBuffer, value);

#define ULLONG_STR(strBuffer, value)				\
Array<32> strBuffer;								\
StringUtility::ullongToString(strBuffer, value);

#define USHORTS_STR(strBuffer, valueArray, bufferCount, count)	\
Array<16 * bufferCount> strBuffer;								\
StringUtility::ushortsToString(strBuffer, valueArray, count);

#define INTS_STR(strBuffer, valueArray, bufferCount, count) \
Array<16 * bufferCount> strBuffer;							\
StringUtility::intsToString(strBuffer, valueArray, count);

#define UINTS_STR(strBuffer, valueArray, bufferCount, count)	\
Array<16 * bufferCount> strBuffer;								\
StringUtility::uintsToString(strBuffer, valueArray, count);

#define FLOATS_STR(strBuffer, valueArray, bufferCount, count)	\
Array<16 * bufferCount> strBuffer;								\
StringUtility::floatsToString(strBuffer, valueArray, count);

#define ULLONGS_STR(strBuffer, valueArray, bufferCount, count)	\
Array<20 * bufferCount> strBuffer;								\
StringUtility::ullongsToString(strBuffer, valueArray, count);

#define LLONGS_STR(strBuffer, valueArray, bufferCount, count)	\
Array<20 * bufferCount> strBuffer;								\
StringUtility::llongsToString(strBuffer, valueArray, count);

//--------------------------------------------------------------------------------------------------------------------------------------------
// 线程锁相关宏
#ifdef LOCK
#undef LOCK
#endif
#define LOCK(lock)							\
(lock).waitForUnlock(__FILE__, __LINE__);	\
try											\
{

#ifdef UNLOCK
#undef UNLOCK
#endif
#define UNLOCK(lock)						\
}											\
catch(...){}								\
(lock).unlock()

//--------------------------------------------------------------------------------------------------------------------------------------------
// 日志打印相关宏
#ifdef ERROR
#undef ERROR
#endif
#define ERROR(info)									cout << "error:" << info << endl; system("pause");
#define LOG(info)									cout << "error:" << info << endl;
#define LOG_NO_PRINT(info)							cout << "error:" << info << endl;

//---------------------------------------------------------------------------------------------------------------------
//内存相关宏定义
#ifdef CHECK_MEMORY
// 带内存合法检测的常规内存申请和释放
#define NORMAL_NEW(className, ptr, ...)	\
new className(__VA_ARGS__);				\
if(ptr != nullptr)							\
{										\
	MemoryCheck::usePtr(ptr);			\
}										\
else									\
{										\
	ERROR(string("can not alloc memory! className :") + STR(className));\
}

#define NORMAL_NEW_ARRAY(className, count, ptr)		\
new className[count];								\
if(count <= 0)										\
{													\
	ERROR("无法申请大小为0的数组");					\
}													\
if (ptr != nullptr)									\
{													\
	MemoryCheck::usePtr(ptr);						\
}													\
else												\
{													\
	ERROR(string("can not alloc memory Array! className : ") + STR(className) + ", count : " + StringUtility::intToString(count));\
}

#define NORMAL_DELETE(ptr)			\
if (ptr != nullptr)					\
{									\
	MemoryCheck::unusePtr(ptr);		\
	delete ptr;						\
	ptr = nullptr;						\
}

#define NORMAL_DELETE_ARRAY(ptr)	\
if (ptr != nullptr)					\
{									\
	MemoryCheck::unusePtr(ptr);		\
	delete[] ptr;					\
	ptr = nullptr;						\
}
#else
// 不带内存合法检测的常规内存申请和释放
#define NORMAL_NEW(className, ptr, ...)			\
new className(__VA_ARGS__);						\
if(ptr == nullptr)									\
{												\
	ERROR(string("can not alloc memory! className : ") + STR(className));\
}

#define NORMAL_NEW_ARRAY(className, count, ptr)		\
new className[count];								\
if(count <= 0)										\
{													\
	ERROR("无法申请大小为0的数组");					\
}													\
if(ptr == nullptr)										\
{													\
	ERROR(string("can not alloc memory Array! className : ") + STR(className) + ", count : " + StringUtility::intToString(count));\
}

#define NORMAL_DELETE(ptr)		\
if (ptr != nullptr)				\
{								\
	delete ptr;					\
	ptr = nullptr;					\
}

#define NORMAL_DELETE_ARRAY(ptr)\
if (ptr != nullptr)				\
{								\
	delete[] ptr;				\
	ptr = nullptr;					\
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
if(ptr != nullptr)							\
{											\
	MemoryTrace::insertPtr(ptr, MemoryInfo(__FILE__, sizeof(className), typeid(className).name(), __LINE__)); \
}

// 申请无参构造的类或者基础数据类型数组内存
#define NEW_ARRAY(className, count, ptr)		\
NORMAL_NEW_ARRAY(className, count, ptr)			\
if(ptr != nullptr)								\
{												\
	txMemoryTrace::insertPtr(ptr, MemoryInfo(__FILE__, typeid(className).name(), sizeof(className)* count, __LINE__)); \
}

// 释放TRACE_NEW申请的内存
#define DELETE(ptr)					\
MemoryTrace::erasePtr((void*)ptr);	\
NORMAL_DELETE(ptr)

// 释放TRACE_NEW_ARRAY申请的内存
#define DELETE_ARRAY(ptr)			\
MemoryTrace::erasePtr((void*)ptr);	\
NORMAL_DELETE_ARRAY(ptr)
#else
#define NEW(className, ptr, ...)			NORMAL_NEW(className, ptr, __VA_ARGS__)
#define NEW_ARRAY(className, count, ptr)	NORMAL_NEW_ARRAY(className, count, ptr)
#define DELETE(ptr)							NORMAL_DELETE(ptr)
#define DELETE_ARRAY(ptr)					NORMAL_DELETE_ARRAY(ptr)
#endif

// 为了简化代码书写而添加的宏
//--------------------------------------------------------------------------------------------------------------------------------------------
// 再次封装后的容器的遍历宏

// 使用迭代器遍历列表
#define FOREACH(iter, stl)							\
auto iter = stl.begin();							\
auto iter##End = stl.end();							\
for (; iter != iter##End; ++iter)

// 使用迭代器遍历列表
#define FOREACH_CONST(iter, stl)					\
auto iter = stl.cbegin();							\
auto iter##End = stl.cend();						\
for(; iter != iter##End; ++iter)

// 使用下标遍历列表
#define FOR_VECTOR(stl)								\
uint stl##Count = stl.size();						\
for(uint i = 0; i < stl##Count; ++i)

#define FOR_VECTOR_J(stl)							\
uint stl##Count = stl.size();						\
for(uint j = 0; j < stl##Count; ++j)

#define FOR_VECTOR_K(stl)							\
uint stl##Count = stl.size();						\
for(uint k = 0; k < stl##Count; ++k)

#define FOR_VECTOR_INVERSE(stl)						\
uint stl##Count = stl.size();						\
for(int i = stl##Count - 1; i >= 0; --i)

// 使用下标遍历常量列表
#define FOR_CONST(stl)								\
uint stl##Count = stl.size();						\
for(uint i = 0; i < stl##Count; ++i)

#define FOR_CONST_J(stl)							\
uint stl##Count = stl.size();						\
for(uint j = 0; j < stl##Count; ++j)

#define FOR_CONST_K(stl)							\
uint stl##Count = stl.size();						\
for(uint k = 0; k < stl##Count; ++k)

#define FOR_CONST_INVERSE(stl)						\
uint stl##Count = stl.size();						\
for(stl, int i = stl##Count - 1; i >= 0; --i)

// 简单的for循环
#define FOR_I(count)			for (int i = 0; i < (int)count; ++i)
#define FOR_J(count)			for (int j = 0; j < (int)count; ++j)
#define FOR_K(count)			for (int k = 0; k < (int)count; ++k)
#define FOR_INVERSE_I(count)	for (int i = count - 1; i >= 0; --i)
#define FOR_INVERSE_J(count)	for (int j = count - 1; j >= 0; --j)
#define FOR_INVERSE_K(count)	for (int k = count - 1; k >= 0; --k)
#define FOR_ONCE				for(int tempI = 0; tempI < 1; ++tempI)

// 创建一个消息对象
#define PACKET(classType, packet) auto packet = FrameBase::mNetServer->createPacket<classType>(StringDefine::NAME(classType))
#define SEND_PACKET(classType, player)	{PACKET(classType, packet##classType); sendPacket(packet##classType, player);}
// 创建一个事件参数对象
#define EVENT(classType, eventParam) auto eventParam = FrameBase::mEventSystem->createEvent<classType>(StringDefine::NAME(classType))
// 注册一个系统组件
#define REGISTE_SYSTEM(type)							\
{														\
	type* component = NEW(type, component);				\
	component->setName(STR(type));						\
	mFrameComponentVector.push_back(component);			\
	mFrameComponentMap.insert(STR(type), component);	\
	if (component->isClassPool())						\
	{													\
		mPoolSystemList.insert(component);				\
	}													\
	else if (component->isFactory())					\
	{													\
		mFactorySystemList.insert(component);			\
	}													\
}
// 将类的基类重命名为base,方便使用
#define BASE(baseClass) typedef baseClass base
// 从对象池中创建一个MySQLData对象
#define NEW_MYSQL(className, var) auto var = FrameBase::mMySQLDataBase->createData<className>(StringDefine::NAME(className))
#define NEW_MYSQL_1(className, var) var = FrameBase::mMySQLDataBase->createData<className>(StringDefine::NAME(className))
#define VECTOR(T, var)									\
Vector<T>* temp##var = nullptr;							\
{														\
	auto pool = VectorPoolManager::getPool<T>();		\
	if (pool == nullptr)								\
	{													\
		ERROR("找不到对应的列表对象池");				\
	}													\
	temp##var = pool->newVector();						\
}														\
Vector<T>& var = *temp##var;

// 当调用UN_VECTOR后,var就不应该再被访问,否则可能会引起不可预知的错误
#define UN_VECTOR(T, var)								\
{														\
	auto pool = VectorPoolManager::getPool<T>();		\
	if (pool == nullptr)								\
	{													\
		ERROR("找不到对应的列表对象池");				\
	}													\
	auto ptr = &var;									\
	pool->destroyVector(&ptr);							\
}

#define VECTOR_THREAD(T, var)							\
Vector<T>* temp##var = nullptr;							\
{														\
	auto pool = VectorPoolManager::getPoolThread<T>();	\
	if (pool == nullptr)								\
	{													\
		ERROR("找不到对应的列表对象池");				\
	}													\
	temp##var = pool->newVector();						\
}														\
Vector<T>& var = *temp##var;

// 当调用UN_VECTOR后,var就不应该再被访问,否则可能会引起不可预知的错误
#define UN_VECTOR_THREAD(T, var)						\
{														\
	auto pool = VectorPoolManager::getPoolThread<T>();	\
	if (pool == nullptr)								\
	{													\
		ERROR("找不到对应的列表对象池");				\
	}													\
	auto ptr = &var;									\
	pool->destroyVector(&ptr);							\
}

#ifndef ADD_COMPONENT
#define ADD_COMPONENT(T) CAST<T*>(addComponent(StringDefine::NAME(T)));
#endif

#ifndef ADD_COMPONENT_ACTIVE
#define ADD_COMPONENT_ACTIVE(T) CAST<T*>(addComponent(StringDefine::NAME(T), true));
#endif

#ifndef GET_COMPONENT
#define GET_COMPONENT(var, T) CAST<T*>((var)->getComponent(StringDefine::NAME(T)));
#endif

#define GET_SYSTEM(type) mServerFramework->getSystem<type>(STR(type), m##type)
#define DECALRE_SYSTEM(name) static name* m##name
#define DEFINE_SYSTEM_FRAME(name) name* FrameBase::m##name

#endif