#pragma once

#include "MathUtility.h"

namespace SystemUtility
{
	extern llong mIDSeedMain;			// 用于生成唯一ID的种子,仅用于主线程
	extern atomic<llong> mIDSeedThread;	// 用于生成唯一ID的种子,用于多线程
	extern llong mTimeMS;				// 系统从启动到现在所经过的毫秒,每帧获取一次,避免频繁获取造成性能下降
	extern llong mTimeS;				// 从1970年1月1日到现在的秒数,每帧获取一次,避免频繁获取造成性能下降
	extern int mMainThread;				// 主线程的线程ID
	//------------------------------------------------------------------------------------------------------------------------------
	inline void sleep(ulong timeMS)
	{
#ifdef WIN32
		Sleep(timeMS);
#elif defined LINUX
		usleep(timeMS * 1000);
#endif
	}
	// 获取系统从启动到现在所经过的毫秒,每帧更新一次的
	inline llong getTimeMS() { return mTimeMS; }
	inline void setTimeMS(const llong timeMS) { mTimeMS = timeMS; }
	// 获取系统从启动到现在所经过的毫秒,实时的
	inline llong getRealTimeMS()
	{
#ifdef WIN32
		return timeGetTime();
#elif defined LINUX
		struct timeval tv;
		if (gettimeofday(&tv, nullptr) != 0)
		{
			ERROR("time get error : " + IToS(errno));
		}
		return tv.tv_sec * 1000 + (llong)(tv.tv_usec * 0.001f);
#endif
	}
	// 是否为闰年
	inline bool isLeapYear(int year) { return (year % 4 == 0 && year % 100 != 0) || (year % 400 == 0); }
	// 获取当前这个月有多少天
	int getDaysInMonth();
	// 获取一个月有多少天
	int getDaysInMonth(int year, int month);
	// 获取年份,从1970开始
	int getYear();
	// 获取月份,从1开始
	int getMonth();
	// 获取年月份,从1开始
	void getYearMonth(int& year, int& month);
	// 获取天,从1开始
	int getDay();
	// 获取当前时间的周几(从1开始),年(从1970开始),月(从1开始),天(从1开始),时(从0开始),分,秒
	void getTime(int& weekDay, int& year, int& month, int& day, int& hour, int& minute, int& second);
	// 获取当前时间的年,月,天,时,分,秒
	void getTime(int& year, int& month, int& day, int& hour, int& minute, int& second);
	// 获取当前时间的时,分,秒
	void getTime(int& hour, int& minute, int& second);
	// 获取当前时间是今天的几点,24小时制
	int getTimeHourInDay();
	// 获取当前时间,以字符串形式表示
	string getTime(bool timeStamp = false);
	// 获取从1970年1月1日到现在的秒数,本地时间,非UTC时间
	inline llong getTimeSecond() { return mTimeS; }
	inline void setTimeSecond(llong timeS) { mTimeS = timeS; }
	inline constexpr int daysToSeconds(int days) { return 24 * 60 * 60 * days; }
	// 获取今天24点的时间戳
	llong getTodayEnd();
	// 获取今天0点的时间戳
	llong getTodayBegin();
	// 获得今天是周几,0表示周日,1表示周一
	int getDayOfWeek();
	// 判断两次时间戳是否是在同一天
	bool isSameDay(time_t timeStamp0, time_t timeStamp1);
	// 根据时间戳获取时间结构体
	struct tm getTimeStruct(time_t timeStamp);
	// 获取指定时间的时间戳
	time_t convertToTimestamp(int year, int month, int day, int hour, int minute, int second);
	// 获得cpu核心数
	int getCPUCoreCount();
	// 获取外网IP
	string getInternetIP(MY_SOCKET socket);
	// 获取内网IP
	string getLocalIP(MY_SOCKET socket);
	void makeSockAddrByHost(sockaddr_in& addr, const char* host, ushort port);
	void makeSockAddrByIP(sockaddr_in& addr, const char* ip, ushort port);
	void makeSockAddr(sockaddr_in& addr, ulong hostlong, ushort port);
	inline int getThreadID()
	{
#ifdef WIN32
		return (int)GetCurrentThreadId();
#elif defined LINUX
		return (int)pthread_self();
#endif
	}
	// 获得程序当前所在路径,带文件名
	inline string getProgramFile()
	{
		MyString<256> name;
#ifdef WIN32
		GetModuleFileNameA(nullptr, name.toBuffer(), name.size());
#elif defined LINUX
		const ssize_t ret = readlink("/proc/self/exe", name.toBuffer(), name.size());
		if (ret == -1)
		{
			return "";
		}
#endif
		return name.str();
	}
	// 参数为绝对路径,并且在windows上需要将所有的'/'转换为'\\'
	bool launchExe(const string& path, const string& fullName);
	inline bool isMainThread() { return getThreadID() == mMainThread; }
	inline void setMainThread(const int id) { mMainThread = id; }
	inline void checkMainThread()
	{
#ifdef WIN32
		if (!isMainThread())
		{
			ERROR("只能在主线程调用");
		}
#endif
	}
	inline llong makeIDMain()
	{
		checkMainThread();
		return ++mIDSeedMain;
	}
	inline llong makeIDThread() { return ++mIDSeedThread; }
	string getStackTrace(int depth);
}

using SystemUtility::getTimeSecond;
using SystemUtility::getTimeMS;
using SystemUtility::getRealTimeMS;
using SystemUtility::checkMainThread;
using SystemUtility::isMainThread;
using SystemUtility::getThreadID;
using SystemUtility::getTime;
using SystemUtility::getYear;
using SystemUtility::getMonth;
using SystemUtility::getYearMonth;
using SystemUtility::getDay;
using SystemUtility::makeIDMain;
using SystemUtility::getDayOfWeek;
using SystemUtility::getLocalIP;
using SystemUtility::getStackTrace;
using SystemUtility::isSameDay;
using SystemUtility::getTimeHourInDay;
using SystemUtility::makeSockAddr;
using SystemUtility::setMainThread;
using SystemUtility::setTimeMS;
using SystemUtility::setTimeSecond;
using SystemUtility::makeIDThread;
using SystemUtility::sleep;
using SystemUtility::getTimeStruct;
using SystemUtility::convertToTimestamp;
using SystemUtility::daysToSeconds;
using SystemUtility::getTodayEnd;
using SystemUtility::getTodayBegin;
using SystemUtility::getDaysInMonth;