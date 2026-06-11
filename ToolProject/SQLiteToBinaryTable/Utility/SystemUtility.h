#ifndef _SYSTEM_UTILITY_H_
#define _SYSTEM_UTILITY_H_

#include "FileUtility.h"

class SystemUtility : public FileUtility
{
public:
	static void sleep(ulong timeMS);
	// 获取系统从启动到现在所经过的毫秒,每帧更新一次的
	static llong getTimeMS() { return mTimeMS; }
	static void setTimeMS(llong timeMS) { mTimeMS = timeMS; }
	// 获取系统从启动到现在所经过的毫秒,实时的
	static llong getRealTimeMS();
	// 获取当前时间的周几(从1开始),年(从1970开始),月(从1开始),天(从1开始),时(从0开始),分,秒
	static void getTime(int& weekDay, int& year, int& month, int& day, int& hour, int& minute, int& second);
	// 获取当前时间的年,月,天,时,分,秒
	static void getTime(int& year, int& month, int& day, int& hour, int& minute, int& second);
	// 获取当前时间的时,分,秒
	static void getTime(int& hour, int& minute, int& second);
	// 获取当前时间是今天的几点,24小时制
	static int getTimeHourInDay();
	// 获取当前时间,以字符串形式表示
	static string getTime(bool timeStamp = false);
	// 获取从1970年1月1日到现在的秒数,本地时间,非UTC时间
	static llong getTimeSecond() { return mTimeS; }
	static void setTimeSecond(llong timeS) { mTimeS = timeS; }
	// 获得今天是周几,0表示周日,1表示周一
	static int getDayOfWeek();
	// 判断两次时间戳是否是在同一天
	static bool isSameDay(const time_t& timeStamp0, const time_t& timeStamp1);
	// 获得cpu核心数
	static int getCPUCoreCount();
	// 获取外网IP
	static string getInternetIP(MY_SOCKET socket);
	// 获取内网IP
	static string getLocalIP(MY_SOCKET socket);
	static void print(const char* str, bool nextLine = true);
	static void input(string& str);
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
		Array<256> name{ 0 };
#if RUN_PLATFORM == PLATFORM_WINDOWS
		GetModuleFileNameA(nullptr, name.toBuffer(), name.size());
#elif RUN_PLATFORM == PLATFORM_LINUX
		ssize_t ret = readlink("/proc/self/exe", name.toBuffer(), name.size());
		if (ret == -1)
		{
			return "";
		}
#endif
		return name.toString();
	}
	// 参数为绝对路径,并且在windows上需要将所有的'/'转换为'\\'
	static bool launchExe(const string& path, const string& fullName);
	static llong makeID();
	static bool isMainThread() { return getThreadID() == mMainThread; }
	static void setMainThread(uint id) { mMainThread = id; }
protected:
	static llong mIDSeed;		// 用于生成唯一ID的种子
	static llong mTimeMS;		// 系统从启动到现在所经过的毫秒,每帧获取一次,避免频繁获取造成性能下降
	static llong mTimeS;		// 从1970年1月1日到现在的秒数,每帧获取一次,避免频繁获取造成性能下降
	static uint mMainThread;	// 主线程的线程ID
};

#endif
