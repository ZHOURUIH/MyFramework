#ifndef _SYSTEM_UTILITY_H_
#define _SYSTEM_UTILITY_H_

#include "ThreadLock.h"
#include "FileUtility.h"

class SystemUtility : public FileUtility
{
public:
	static void sleep(ulong timeMS);
	static long getTimeMS();
	static string getTime(bool timeStamp = false);
	// 返回media的路径,不带/
	static const string& getMediaPath()
	{
		return MEDIA_PATH;
	}
	// 通过一个media下的相对路径,或者绝对路径,转化为一个可直接打开的路径
	static string getAvailableResourcePath(string name)
	{
		string mediaPath = getMediaPath();
		// 如果文件名已经是不带media路径并且不是绝对路径
		if (mediaPath != "" && (name.length() <= mediaPath.length() || name.substr(0, mediaPath.length()) != mediaPath) && (name.length() > 1 && name[1] != ':'))
		{
			name = mediaPath + "/" + name;
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
	static string getIPFromSocket(const MY_SOCKET& socket)
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

	static void print(const string& str, bool nextLine = true)
	{
#if RUN_PLATFORM == PLATFORM_WINDOWS
		cout << str;
		if (nextLine)
		{
			cout << "\n";
		}
#elif RUN_PLATFORM == PLATFORM_LINUX
		printf("%s", str.c_str());
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
		aray<char, 256> i;
		scanf("%s", i._Elems);
		str = i._Elems;
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
		array<char, 256> name;
#if RUN_PLATFORM == PLATFORM_WINDOWS
		GetModuleFileNameA(NULL, name._Elems, (DWORD)name.size());
#elif RUN_PLATFORM == PLATFORM_LINUX
		ssize_t ret = readlink("/proc/self/exe", name._Elems, name.size());
		if (ret == -1)
		{
			return "";
		}
#endif
		return name._Elems;
	}
	// 参数为绝对路径,并且在windows上需要将所有的'/'转换为'\\'
	static bool launchExe(const string& path, const string& fullName);
	// 参数为文件名,不带路径,不带后缀
	static bool isExeRunning(const string& fileName);
#if RUN_PLATFORM == PLATFORM_WINDOWS
	static void getAllProcess(mySet<string>& processList);
#elif RUN_PLATFORM == PLATFORM_LINUX
	static char* getPidFromStr(const char* str);
#endif
	static uint generateGUID()
	{
		// 获得当前时间
		return (uint)(getTimeMS() % 0x7FFFFFFF) + (++mIDSeed);
	}
	template<typename T, typename K>
	static void mapKeyToList(const myMap<K, T>& map, myVector<K>& keyList)
	{
		FOREACH_CONST(iter, map)
		{
			keyList.push_back(iter->first);
		}
	}
	template<typename T, typename K>
	static bool mapValueToList(const myMap<K, T>& map, K* keyList, int maxCount)
	{
		int index = 0;
		FOREACH_CONST(iter, map)
		{
			if (index >= maxCount)
			{
				return false;
			}
			keyList[index] = iter->first;
			++index;
		}
		return true;
	}
	template<typename T, typename K>
	static void mapValueToList(const myMap<K, T>& map, myVector<T>& valueList)
	{
		FOREACH_CONST(iter, map)
		{
			valueList.push_back(iter->second);
		}
	}
	template<typename T, typename K, uint Length>
	static bool mapValueToList(const myMap<K, T>& map, array<T, Length>& valueList, bool showError = true)
	{
		uint index = 0;
		FOREACH_CONST(iter, map)
		{
			if (index >= Length)
			{
				if (showError)
				{
					ERROR("buffer is too small");
				}
				return false;
			}
			valueList[index] = iter->second;
			++index;
		}
		return true;
	}
	template<typename T, typename K>
	static bool mapValueToList(const myMap<K, T>& map, T* valueList, uint maxCount, bool showError = true)
	{
		uint index = 0;
		FOREACH_CONST(iter, map)
		{
			if (index >= maxCount)
			{
				if (showError)
				{
					ERROR("buffer is too small");
				}
				return false;
			}
			valueList[index] = iter->second;
			++index;
		}
		return true;
	}
	static string getEnvironmentValue(const string& name);
	static bool isMainThread() { return getThreadID() == mMainThread; }
	static void setMainThread(uint id) { mMainThread = id; }
protected:
	static long mIDSeed;
	static uint mMainThread;
};

#endif
