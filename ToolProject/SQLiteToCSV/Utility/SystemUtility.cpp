#include "FrameHeader.h"

llong SystemUtility::mIDSeed = 0;
uint SystemUtility::mMainThread = 0;
llong SystemUtility::mTimeMS = 0;
llong SystemUtility::mTimeS = 0;

void SystemUtility::sleep(ulong timeMS)
{
#if RUN_PLATFORM == PLATFORM_WINDOWS
	Sleep(timeMS);
#elif RUN_PLATFORM == PLATFORM_LINUX
	usleep(timeMS * 1000);
#endif
}

llong SystemUtility::getRealTimeMS()
{
#if RUN_PLATFORM == PLATFORM_WINDOWS
	return timeGetTime();
#elif RUN_PLATFORM == PLATFORM_LINUX
	struct timeval tv;
	if (gettimeofday(&tv, nullptr) != 0)
	{
		ERROR("time get error : " + intToString(errno));
	}
	return tv.tv_sec * 1000 + (llong)(tv.tv_usec * 0.001f);
#endif
}

void SystemUtility::getTime(int& weekDay, int& year, int& month, int& day, int& hour, int& minute, int& second)
{
#if RUN_PLATFORM == PLATFORM_WINDOWS
	SYSTEMTIME sys;
	GetLocalTime(&sys);
	weekDay = sys.wDayOfWeek;
	year = sys.wYear;
	month = sys.wMonth;
	day = sys.wDay;
	hour = sys.wHour;
	minute = sys.wMinute;
	second = sys.wSecond;
#elif RUN_PLATFORM == PLATFORM_LINUX
	time_t tt;
	time(&tt);
	struct tm date;
	localtime_r(&tt, &date);
	weekDay = date.tm_wday;
	year = date.tm_year + 1900;
	month = date.tm_mon + 1;
	day = date.tm_mday;
	hour = date.tm_hour;
	minute = date.tm_min;
	second = date.tm_sec;
#endif
}

void SystemUtility::getTime(int& year, int& month, int& day, int& hour, int& minute, int& second)
{
#if RUN_PLATFORM == PLATFORM_WINDOWS
	SYSTEMTIME sys;
	GetLocalTime(&sys);
	year = sys.wYear;
	month = sys.wMonth;
	day = sys.wDay;
	hour = sys.wHour;
	minute = sys.wMinute;
	second = sys.wSecond;
#elif RUN_PLATFORM == PLATFORM_LINUX
	time_t tt;
	time(&tt);
	struct tm date;
	localtime_r(&tt, &date);
	year = date.tm_year + 1900;
	month = date.tm_mon + 1;
	day = date.tm_mday;
	hour = date.tm_hour;
	minute = date.tm_min;
	second = date.tm_sec;
#endif
}

void SystemUtility::getTime(int& hour, int& minute, int& second)
{
#if RUN_PLATFORM == PLATFORM_WINDOWS
	SYSTEMTIME sys;
	GetLocalTime(&sys);
	hour = sys.wHour;
	minute = sys.wMinute;
	second = sys.wSecond;
#elif RUN_PLATFORM == PLATFORM_LINUX
	time_t tt;
	time(&tt);
	struct tm date;
	localtime_r(&tt, &date);
	hour = date.tm_hour;
	minute = date.tm_min;
	second = date.tm_sec;
#endif
}

int SystemUtility::getTimeHourInDay()
{
#if RUN_PLATFORM == PLATFORM_WINDOWS
	SYSTEMTIME sys;
	GetLocalTime(&sys);
	return sys.wHour;
#elif RUN_PLATFORM == PLATFORM_LINUX
	time_t tt;
	time(&tt);
	struct tm date;
	localtime_r(&tt, &date);
	return date.tm_hour;
#endif
}

string SystemUtility::getTime(bool timeStamp)
{
	Array<128> timeBuffer{ 0 };
#if RUN_PLATFORM == PLATFORM_WINDOWS
	SYSTEMTIME sys;
	GetLocalTime(&sys);
	if (timeStamp)
	{
		SPRINTF(timeBuffer.toBuffer(), timeBuffer.size(), "%d-%02d-%02d %02d:%02d:%02d", sys.wYear, sys.wMonth, sys.wDay, sys.wHour, sys.wMinute, sys.wSecond);
	}
	else
	{
		SPRINTF(timeBuffer.toBuffer(), timeBuffer.size(), "%04d年%02d月%02d日%02d时%02d分%02d秒%03d毫秒", sys.wYear, sys.wMonth, sys.wDay, sys.wHour, sys.wMinute, sys.wSecond, sys.wMilliseconds);
	}
#elif RUN_PLATFORM == PLATFORM_LINUX
	time_t tt;
	time(&tt);
	struct tm date;
	localtime_r(&tt, &date);
	if (timeStamp)
	{
		SPRINTF(timeBuffer.toBuffer(), timeBuffer.size(), "%d-%02d-%02d %02d:%02d:%02d", date.tm_year + 1900, date.tm_mon + 1, date.tm_mday, date.tm_hour, date.tm_min, date.tm_sec);
	}
	else
	{
		SPRINTF(timeBuffer.toBuffer(), timeBuffer.size(), "%04d年%02d月%02d日%02d时%02d分%02d秒", date.tm_year + 1900, date.tm_mon + 1, date.tm_mday, date.tm_hour, date.tm_min, date.tm_sec);
	}
#endif
	return timeBuffer.toString();
}

int SystemUtility::getDayOfWeek()
{
#if RUN_PLATFORM == PLATFORM_WINDOWS
	SYSTEMTIME sys;
	GetLocalTime(&sys);
	return sys.wDayOfWeek;
#elif RUN_PLATFORM == PLATFORM_LINUX
	time_t tt;
	time(&tt);
	struct tm date;
	localtime_r(&tt, &date);
	return date.tm_wday;
#endif
}

bool SystemUtility::isSameDay(const time_t& timeStamp0, const time_t& timeStamp1)
{
	struct tm dateTime0;
	struct tm dateTime1;
#if RUN_PLATFORM == PLATFORM_WINDOWS
	localtime_s(&dateTime0, &timeStamp0);
	localtime_s(&dateTime1, &timeStamp1);
#elif RUN_PLATFORM == PLATFORM_LINUX
	localtime_r(&timeStamp0, &dateTime0);
	localtime_r(&timeStamp1, &dateTime1);
#endif
	return dateTime0.tm_year == dateTime1.tm_year &&
		   dateTime0.tm_mon == dateTime1.tm_mon &&
		   dateTime0.tm_mday == dateTime1.tm_mday;
}

// 获得cpu核心数
int SystemUtility::getCPUCoreCount()
{
#if RUN_PLATFORM == PLATFORM_WINDOWS
	SYSTEM_INFO si;
	GetSystemInfo(&si);
	return si.dwNumberOfProcessors;
#elif RUN_PLATFORM == PLATFORM_LINUX
	return get_nprocs();
#endif
}

// 获取外网的IP
string SystemUtility::getInternetIP(MY_SOCKET socket)
{
#if RUN_PLATFORM == PLATFORM_WINDOWS
	SOCKADDR_IN addr_conn;
	int nSize = sizeof(addr_conn);
	memset((void*)&addr_conn, 0, sizeof(addr_conn));
	getpeername(socket, (SOCKADDR*)(&addr_conn), &nSize);
	return string(inet_ntoa(addr_conn.sin_addr));
#elif RUN_PLATFORM == PLATFORM_LINUX
	struct sockaddr_in addr_conn;
	int nSize = sizeof(addr_conn);
	memset((void*)&addr_conn, 0, sizeof(addr_conn));
	getpeername(socket, (struct sockaddr*)(&addr_conn), (socklen_t*)&nSize);
	return string(inet_ntoa(addr_conn.sin_addr));
#endif
}

string SystemUtility::getLocalIP(MY_SOCKET socket)
{
#if RUN_PLATFORM == PLATFORM_WINDOWS
	SOCKADDR_IN addr_conn;
	int nSize = sizeof(addr_conn);
	memset((void*)&addr_conn, 0, sizeof(addr_conn));
	getsockname(socket, (SOCKADDR*)(&addr_conn), &nSize);
	return string(inet_ntoa(addr_conn.sin_addr));
#elif RUN_PLATFORM == PLATFORM_LINUX
	struct sockaddr_in addr_conn;
	int nSize = sizeof(addr_conn);
	memset((void*)&addr_conn, 0, sizeof(addr_conn));
	getsockname(socket, (struct sockaddr*)(&addr_conn), (socklen_t*)&nSize);
	return string(inet_ntoa(addr_conn.sin_addr));
#endif
}

void SystemUtility::print(const char* str, bool nextLine)
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

void SystemUtility::input(string& str)
{
#if RUN_PLATFORM == PLATFORM_WINDOWS
	cin >> str;
#elif RUN_PLATFORM == PLATFORM_LINUX
	Array<256> i{ 0 };
	if (scanf("%s", i.toBuffer()) == 1)
	{
		str = i.toString();
	}
#endif
}

bool SystemUtility::launchExe(const string& path, const string& fullName)
{
#if RUN_PLATFORM == PLATFORM_WINDOWS
	// 启动游戏程序
	SHELLEXECUTEINFO sei;
	memset(&sei, 0, sizeof(sei));
	sei.cbSize = sizeof(sei);
	sei.lpFile = fullName.c_str();
	sei.lpDirectory = path.c_str();
	sei.lpVerb = "open";
#ifndef __WXWINCE__
	sei.nShow = SW_SHOWNORMAL; // SW_SHOWDEFAULT not defined under CE (#10216)
#else
	sei.nShow = SW_SHOWDEFAULT;
#endif
	sei.fMask = SEE_MASK_FLAG_NO_UI;
	return ShellExecuteEx(&sei) == TRUE;
#elif RUN_PLATFORM == PLATFORM_LINUX
	if (vfork() == 0)
	{
		char* argv[] = { (char*)"", (char*)nullptr};
		execv(fullName.c_str(), argv);
	}
	return true;
#endif
}

llong SystemUtility::makeID()
{
	llong id = 0;
	id = ++mIDSeed; 
	return id;
}