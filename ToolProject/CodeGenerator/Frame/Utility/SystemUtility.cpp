#include "SystemUtility.h"
#include "Utility.h"

long SystemUtility::mIDSeed = 0;
uint SystemUtility::mMainThread = 0;

void SystemUtility::sleep(ulong timeMS)
{
#if RUN_PLATFORM == PLATFORM_WINDOWS
	Sleep(timeMS);
#elif RUN_PLATFORM == PLATFORM_LINUX
	usleep(timeMS * 1000);
#endif
}

long SystemUtility::getTimeMS()
{
#if RUN_PLATFORM == PLATFORM_WINDOWS
	return timeGetTime();
#elif RUN_PLATFORM == PLATFORM_LINUX
	struct timeval tv;
	if(gettimeofday(&tv, NULL) != 0)
	{
		ERROR("time get error : " + intToString(errno));
	}
	return tv.tv_sec * 1000 + (long)(tv.tv_usec * 0.001f);
#endif
}

string SystemUtility::getTime(bool timeStamp)
{
	array<char, 128> timeBuffer;
#if RUN_PLATFORM == PLATFORM_WINDOWS
	SYSTEMTIME sys;
	GetLocalTime(&sys);
	if (timeStamp)
	{
		SPRINTF(timeBuffer._Elems, timeBuffer.size(), "%d-%02d-%02d %02d:%02d:%02d", sys.wYear, sys.wMonth, sys.wDay, sys.wHour, sys.wMinute, sys.wSecond);
	}
	else
	{
		SPRINTF(timeBuffer._Elems, timeBuffer.size(), "%02d月%02d日%02d时%02d分%02d秒%03d毫秒", sys.wMonth, sys.wDay, sys.wHour, sys.wMinute, sys.wSecond, sys.wMilliseconds);
	}
#elif RUN_PLATFORM == PLATFORM_LINUX
	time_t tt;
	time(&tt);
	struct tm date;
	localtime_r(&tt, &date);
	if (timeStamp)
	{
		SPRINTF(timeBuffer._Elems, timeBuffer.size(), "%d-%02d-%02d %02d:%02d:%02d", date.tm_year + 1900, date.tm_mon + 1, date.tm_mday, date.tm_hour, date.tm_min, date.tm_sec);
	}
	else
	{
		SPRINTF(timeBuffer._Elems, timeBuffer.size(), "%02d月%02d日%02d时%02d分%02d秒", date.tm_mon + 1, date.tm_mday, date.tm_hour, date.tm_min, date.tm_sec);
	}
#endif
	return timeBuffer._Elems;
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
		char* argv[] = { "", (char*)NULL};
		execv(fullName.c_str(), argv);
	}
	return true;
#endif
}

#if RUN_PLATFORM == PLATFORM_WINDOWS
bool SystemUtility::isExeRunning(const string& fileName)
{
	// 判断当前是否已经运行了该游戏,如果已经运行则直接退出
	mySet<string> processList;
	getAllProcess(processList);
	return processList.contains(fileName);
}

void SystemUtility::getAllProcess(mySet<string>& processList)
{
	HANDLE handle = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);//获得系统快照句柄 
	PROCESSENTRY32 info;
	info.dwSize = sizeof(PROCESSENTRY32);
	//调用一次 Process32First 函数，从快照中获取进程列表 
	Process32First(handle, &info);
	//重复调用 Process32Next，直到函数返回 FALSE 为止 
	while (Process32Next(handle, &info))
	{
		processList.insert(info.szExeFile);
	}
}
#elif RUN_PLATFORM == PLATFORM_LINUX
char* SystemUtility::getPidFromStr(const char* str)
{
	static array<char, 8> sPID{ 0 };
	uint pos1 = 0;
	uint pos2 = 0;
	uint length = strlen(str);
	for (uint i = 0; i < length; i++)
	{
		uint tmp = 0;
		if (tmp == 0 && str[i] >= '0' && str[i] <= '9')
		{
			tmp = 1;
			pos1 = i;
		}
		if (tmp == 1 && str[i] < '0' || str[i] > '9')
		{
			pos2 = i;
			break;
		}
	}
	uint j = 0;
	for (uint i = pos1; i < pos2; ++i, ++j) 
	{
		sPID[j] = str[i];
	}
	return sPID;
}

bool SystemUtility::isExeRunning(const string& fileName)
{
	array<char, 16> sCurrPid{ 0 };
	sprintf(sCurrPid, "%d", getpid());

	FILE* fstream = NULL;
	array<char, 1024> buff{ 0 };
	if (NULL == (fstream = popen(("ps -e -o pid,comm | grep " + fileName + " | grep -v PID | grep -v grep").c_str(), "r")))
	{
		return false;
	}
	bool isRunning = false;
	while (NULL != fgets(buff, sizeof(buff), fstream)) 
	{
		char* oldPID = getPidFromStr(buff);
		if (strcmp(sCurrPid, oldPID) != 0) 
		{
			isRunning = true;
			break;
		}
	}
	pclose(fstream);
	return isRunning;
}
#endif

string SystemUtility::getEnvironmentValue(const string& name)
{
	char* path = getenv(name.c_str());
	if (path == NULL)
	{
		return "";
	}
	return path;
}