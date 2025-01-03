#ifndef _SYSTEM_UTILITY_H_
#define _SYSTEM_UTILITY_H_

#include "ThreadLock.h"
#include "FileUtility.h"

class SystemUtility : public FileUtility
{
public:
	static void stop();
	static void sleep(ulong timeMS);
	// 获取系统从启动到现在所经过的毫秒,每帧更新一次的
	static ulong getTimeMS() { return mTimeMS; }
	static void setTimeMS(ulong timeMS) { mTimeMS = timeMS; }
	// 获取系统从启动到现在所经过的毫秒,实时的
	static ulong getRealTimeMS();
	// 获取当前时间的周几(从1开始),年(从1970开始),月(从1开始),天(从1开始),时(从0开始),分,秒
	static void getTime(int& weekDay, int& year, int& month, int& day, int& hour, int& minute, int& second);
	// 获取当前时间的年,月,天,时,分,秒
	static void getTime(int& year, int& month, int& day, int& hour, int& minute, int& second);
	// 获取当前时间的时,分,秒
	static void getTime(int& hour, int& minute, int& second);
	// 获取当前时间,以字符串形式表示
	static string getTime(bool timeStamp = false);
	// 获取从1970年1月1日到现在的秒数
	static ullong getTimeSecond() { return time(NULL); }
	// 获得今天是周几
	static int getDayOfWeek();
	// 判断两次时间戳是否是在同一天
	static bool isSameDay(const time_t& timeStamp0, const time_t& timeStamp1);
	// 通过一个media下的相对路径,或者绝对路径,转化为一个可直接打开的路径
	static string checkResourcePath(const string& name)
	{
		string mediaPath = MEDIA_PATH;
		// 如果文件名已经是不带media路径并且不是绝对路径
		if (mediaPath != "" &&
			!startWith(name.c_str(), mediaPath.c_str()) &&
			name.length() > 1 &&
			name[1] != ':')
		{
			return mediaPath + "/" + name;
		}
		return name;
	}
	// 获得cpu核心数
	static int getCPUCoreCount()
	{
#if RUN_PLATFORM == PLATFORM_WINDOWS
		SYSTEM_INFO si;
		GetSystemInfo(&si);
		return si.dwNumberOfProcessors;
#elif RUN_PLATFORM == PLATFORM_LINUX
		return get_nprocs();
#endif
	}
	static void print(const string& str, bool nextLine = true)
	{
		print(str.c_str(), nextLine);
	}
	static void print(const char* str, bool nextLine = true)
	{
#if RUN_PLATFORM == PLATFORM_WINDOWS
		cout << str;
		if (nextLine)
		{
			cout << "\n";
		}
#elif RUN_PLATFORM == PLATFORM_LINUX
		printf("%s", str);
		if (nextLine)
		{
			printf("%s", "\n");
		}
#endif
	}
	static void input(string& str)
	{
#if RUN_PLATFORM == PLATFORM_WINDOWS
		cin >> str;
#elif RUN_PLATFORM == PLATFORM_LINUX
		array<char, 256> i{ 0 };
		scanf("%s", i.data());
		str = i.data();
#endif
	}
	
	static uint getThreadID()
	{
#if RUN_PLATFORM == PLATFORM_WINDOWS
		return GetCurrentThreadId();
#elif RUN_PLATFORM == PLATFORM_LINUX
		return pthread_self();
#endif
	}
	// 获得程序当前所在路径,带文件名
	static string getProgramFile()
	{
		array<char, 256> name{ 0 };
#if RUN_PLATFORM == PLATFORM_WINDOWS
		GetModuleFileNameA(NULL, name.data(), (uint)name.size());
#elif RUN_PLATFORM == PLATFORM_LINUX
		ssize_t ret = readlink("/proc/self/exe", name.data(), name.size());
		if (ret == -1)
		{
			return "";
		}
#endif
		return name.data();
	}
	// 参数为文件名,不带路径,不带后缀
	static bool isExeRunning(const string& fileName);
#if RUN_PLATFORM == PLATFORM_WINDOWS
#elif RUN_PLATFORM == PLATFORM_LINUX
	static void getPidFromStr(const char* str, char* pid, uint size);
#endif
	static ullong makeID() { return ++mIDSeed; }
	static string getStackTrace(int depth);
	static bool isMainThread() { return getThreadID() == mMainThread; }
	static void setMainThread(uint id) { mMainThread = id; }
protected:
	static ullong mIDSeed;
	static ulong mTimeMS;		// 系统从启动到现在所经过的毫秒,每帧获取一次,避免频繁获取造成性能下降
	static uint mMainThread;
};

#endif
