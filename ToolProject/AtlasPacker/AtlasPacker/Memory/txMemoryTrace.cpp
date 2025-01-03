#include "txMemoryTrace.h"
#ifdef TRACE_MEMORY
#include "txSerializer.h"
#include "TimeLock.h"
#include "CustomThread.h"

myMap<void*, MemoryInfo> txMemoryTrace::mMemoryInfo;
myMap<string, MemoryType> txMemoryTrace::mMemoryType;
mySet<string> txMemoryTrace::mIgnoreClass;
mySet<string> txMemoryTrace::mIgnoreClassKeyword;
mySet<string> txMemoryTrace::mShowOnlyDetailClass;
mySet<string> txMemoryTrace::mShowOnlyStatisticsClass;
bool txMemoryTrace::mShowDetail = true;
bool txMemoryTrace::mShowStatistics = true;
bool txMemoryTrace::mShowTotalCount = true;
bool txMemoryTrace::mShowAll = true;
myMap<string, int> txMemoryTrace::mMemoryTypeIndex;
MemoryType txMemoryTrace::mMemoryList[MAX_COUNT];
int txMemoryTrace::mMemoryCount = 0;
txShareMemoryServer* txMemoryTrace::mShareMemoryServer = NULL;
ThreadLock txMemoryTrace::mInfoLock;
bool txMemoryTrace::mWriteOrDebug = false;
CustomThread* txMemoryTrace::mThread = NULL;

txMemoryTrace::txMemoryTrace(const string& name)
{
	mShowDetail = true;
	mShowStatistics = true;
	mShowTotalCount = true;
	mShowAll = true;
	mWriteOrDebug = true;
	mShareMemoryServer = TRACE_NEW(txShareMemoryServer, mShareMemoryServer);
	mThread = TRACE_NEW(CustomThread, mThread, "MemoryTrace");
}

txMemoryTrace::~txMemoryTrace()
{
	TRACE_DELETE(mThread);
	TRACE_DELETE(mShareMemoryServer);
}

void txMemoryTrace::init()
{
	mShareMemoryServer->Create("MemoryTrace", 1024 * 1024);
	mThread->start(mWriteOrDebug ? writeMemoryTrace : debugMemoryTrace, NULL, 1000);
}

bool txMemoryTrace::debugMemoryTrace(void* args)
{
	int memoryCount = mMemoryInfo.size();
	int memorySize = 0;
	if (mShowAll)
	{
		// 首先检测是否可以读取,如果不可以,则等待解锁读取
		LOCK(mInfoLock);
		LOG_INFO("\n\n---------------------------------------------memory info begin-----------------------------------------------------------\n");

		// 内存详细信息
		auto iter = mMemoryInfo.begin();
		auto iterEnd = mMemoryInfo.end();
		FOR(mMemoryInfo, ; iter != iterEnd; ++iter)
		{
			memorySize += iter->second.size;
			if (!mShowDetail)
			{
				continue;
			}
			// 如果该类型已忽略,则不显示
			if (mIgnoreClass.contains(iter->second.type))
			{
				continue;
			}

			// 如果仅显示的类型列表不为空,则只显示列表中的类型
			if (mShowOnlyDetailClass.size() > 0 && mShowOnlyDetailClass.find(iter->second.type) == mShowOnlyDetailClass.end())
			{
				continue;
			}

			// 如果类型包含关键字,则不显示
			bool show = true;
			auto iterKeyword = mIgnoreClassKeyword.begin();
			auto iterKeywordEnd = mIgnoreClassKeyword.end();
			FOR(mIgnoreClassKeyword, ; iterKeyword != iterKeywordEnd; ++iterKeyword)
			{
				if (strstr(iter->second.type.c_str(), iterKeyword->c_str()) != NULL)
				{
					show = false;
					break;
				}
			}
			END(mIgnoreClassKeyword);

			if (show)
			{
				LOG_INFO("size : %d, file : %s, line : %d, class : %s\n", iter->second.size, iter->second.file.c_str(), iter->second.line, iter->second.type.c_str());
			}
		}
		END(mMemoryInfo);
		UNLOCK(mInfoLock);

		if (mShowTotalCount)
		{
			LOG_INFO("-------------------------------------------------memory count : %d, total size : %.3fKB\n", memoryCount, memorySize / 1000.0f);
		}
		// 显示统计数据
		if (mShowStatistics)
		{
			auto iterType = mMemoryType.begin();
			auto iterTypeEnd = mMemoryType.end();
			FOR(mMemoryType, ; iterType != iterTypeEnd; ++iterType)
			{
				// 如果该类型已忽略,则不显示
				if (mIgnoreClass.contains(iterType->first))
				{
					continue;
				}
				// 如果仅显示的类型列表不为空,则只显示列表中的类型
				if (mShowOnlyStatisticsClass.size() > 0 && mShowOnlyStatisticsClass.find(iterType->first) == mShowOnlyStatisticsClass.end())
				{
					continue;
				}
				// 如果类型包含关键字,则不显示
				bool show = true;
				auto iterKeyword = mIgnoreClassKeyword.begin();
				auto iterKeywordEnd = mIgnoreClassKeyword.end();
				FOR(mIgnoreClassKeyword, ; iterKeyword != iterKeywordEnd; ++iterKeyword)
				{
					if (strstr(iterType->first.c_str(), iterKeyword->c_str()) != NULL)
					{
						show = false;
						break;
					}
				}
				END(mIgnoreClassKeyword);
				if (show)
				{
					LOG_INFO("%s : %d个, %.3fKB\n", iterType->first.c_str(), iterType->second.count, iterType->second.size / 1000.0f);
				}
			}
			END(mMemoryType);
		}
		LOG_INFO("---------------------------------------------memory info end-----------------------------------------------------------\n");
	}
	return true;
}

bool txMemoryTrace::writeMemoryTrace(void* args)
{
	// 这里就不能在对序列化的内存进行跟踪,否则会陷入死锁
	txSerializer serializer(false);
	// 锁定列表
	LOCK(mInfoLock);
	// 写入详细信息数量
	int infoCount = mMemoryInfo.size();
	serializer.write(infoCount);
	auto iterInfo = mMemoryInfo.begin();
	auto iterInfoEnd = mMemoryInfo.end();
	FOR(mMemoryInfo, ; iterInfo != iterInfoEnd; ++iterInfo)
	{
		serializer.write((int)iterInfo->first);						// 地址
		serializer.write(iterInfo->second.size);				// 内存大小
		serializer.writeString(iterInfo->second.file.c_str());	// 文件名
		serializer.write(iterInfo->second.line);				// 行号
		serializer.writeString(iterInfo->second.type.c_str());	// 类型
	}
	END(mMemoryInfo);

	// 写入类型数量
	int typeCount = mMemoryTypeIndex.size();
	serializer.write(typeCount);
	auto iterIndex = mMemoryTypeIndex.begin();
	auto iterIndexEnd = mMemoryTypeIndex.end();
	FOR(mMemoryTypeIndex, ; iterIndex != iterIndexEnd; ++iterIndex)
	{
		serializer.writeString(mMemoryList[iterIndex->second].type.c_str());	// 类型名
		serializer.write(mMemoryList[iterIndex->second].count);					// 个数
		serializer.write(mMemoryList[iterIndex->second].size);					// 大小
	}
	END(mMemoryTypeIndex);
	// 解锁列表
	UNLOCK(mInfoLock);
	DATA_HEADER header;
	header.mCmd = MEMORY_TRACE_CMD;
	header.mDataSize = serializer.getDataSize();
	mShareMemoryServer->WriteCmdData(header, (void*)serializer.getBuffer());
	return true;
}

void txMemoryTrace::insertPtr(void* ptr, MemoryInfo& info)
{
	// 锁定列表
	LOCK(mInfoLock);
	int lastPos = info.file.find_last_of('\\');
	if (lastPos != -1)
	{
		info.file = info.file.substr(lastPos + 1, info.file.length() - lastPos - 1);
	}
	mMemoryInfo.insert(ptr, info);

	auto iterType = mMemoryType.find(info.type);
	if (iterType != mMemoryType.end())
	{
		++(iterType->second.count);
		iterType->second.size += info.size;
	}
	else
	{
		mMemoryType.insert(info.type, MemoryType(info.type, 1, info.size));
	}

	if(mWriteOrDebug)
	{
		// 在类型下标列表中查找该类型,如果有,则更新类型信息
		auto iterIndex = mMemoryTypeIndex.find(info.type);
		if (iterIndex != mMemoryTypeIndex.end())
		{
			auto iterType = mMemoryType.find(info.type);
			mMemoryList[iterIndex->second] = iterType->second;
		}
		// 如果没有,则添加类型信息
		else
		{
			if (mMemoryCount < MAX_COUNT)
			{
				auto iterType = mMemoryType.find(info.type);
				mMemoryTypeIndex.insert(info.type, mMemoryCount);
				mMemoryList[mMemoryCount] = iterType->second;
				++mMemoryCount;
			}
		}
	}
	
	// 解锁列表
	UNLOCK(mInfoLock);
}

void txMemoryTrace::erasePtr(void* ptr)
{
	// 锁定列表
	LOCK(mInfoLock);
	// 加一层循环是为了方便解锁
	do
	{
		// 从内存信息列表中移除
		auto iterTrace = mMemoryInfo.find(ptr);
		if (iterTrace == mMemoryInfo.end())
		{
			break;
		}
		string type = iterTrace->second.type;
		int size = iterTrace->second.size;
		mMemoryInfo.erase(iterTrace);
		// 从内存类型列表中移除
		auto iterType = mMemoryType.find(type);
		if (iterType == mMemoryType.end())
		{
			break;
		}
		--(iterType->second.count);
		iterType->second.size -= size;
		if(mWriteOrDebug)
		{
			// 在下标列表中查找该类型的下标,如果有,则将类型信息中的信息清空
			auto iterIndex = mMemoryTypeIndex.find(type);
			if (iterIndex == mMemoryTypeIndex.end())
			{
				break;
			}
			--(mMemoryList[iterIndex->second].count);
			mMemoryList[iterIndex->second].size -= size;
		}
	} while (false);
	// 解锁列表
	UNLOCK(mInfoLock);
}

#endif