#if !UNITY_EDITOR
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Net;
using UnityEngine.SceneManagement;

#if UNITY_STANDALONE_WIN
public class LocalLog : GameBase
{
	protected CustomThread mWriteLogThread;
	// 日志双缓冲,使用双缓冲可以快速进行前后台切换,避免数据同步时出现耗时操作
	protected DoubleBuffer<string> mLogBufferList;
	protected string mLogFilePath;
	public LocalLog()
	{
		mLogBufferList = new DoubleBuffer<string>();
		mWriteLogThread = new CustomThread("WriteLocalLog");
		mLogFilePath = CommonDefine.F_ASSETS_PATH + "log.txt";
	}
	public void init()
	{
		// 清空已经存在的日志文件
		writeTxtFile(mLogFilePath, null);
		mWriteLogThread.start(writeLocalLog);
	}
	public void destroy()
	{
		mWriteLogThread.destroy();
		// 线程停止后仍然需要保证将列表中的全部日志信息写入文件
		writeLogToFile();
	}
	public void log(string info)
	{
		// 将日志保存到当前缓冲中
		mLogBufferList.addToBuffer(info);
	}
	//-----------------------------------------------------------------------------------------------------------
	protected void writeLocalLog(ref bool run)
	{
		// 将当前写入缓冲区中的内容写入文件
		writeLogToFile();
	}
	protected void writeLogToFile()
	{
		string totalString = EMPTY_STRING;
		var readList = mLogBufferList.getReadList();
		int count = readList.Count;
		if (count > 0)
		{
			for (int i = 0; i < count; ++i)
			{
				totalString += readList[i] + "\r\n";
			}
			writeTxtFile(mLogFilePath, totalString, true);
		}
		readList.Clear();
	}
}
#else
public class LocalLog : GameBase
{
	// 日志双缓冲,使用双缓冲可以快速进行前后台切换,避免数据同步时出现耗时操作
	protected DoubleBuffer<string> mLogBufferList;
	protected string mLogFilePath;
	public LocalLog()
	{
		mLogBufferList = new DoubleBuffer<string>();
		mLogFilePath = CommonDefine.F_PERSISTENT_DATA_PATH + "log.txt";
	}
	public void init()
	{
		// 清空已经存在的日志文件
		deleteFile(mLogFilePath);
	}
	public void destroy()
	{
		// 线程停止后仍然需要保证将列表中的全部日志信息写入文件
		writeLogToFile();
	}
	public void log(string info)
	{
		// 将日志保存到当前缓冲中
		mLogBufferList.addToBuffer(info);
	}
	public void update(float elapsedTime)
	{
		// 在每一帧将累积的日志写入到文件中
		writeLogToFile();
	}
	//-----------------------------------------------------------------------------------------------------------
	protected void writeLogToFile()
	{
		string totalString = EMPTY_STRING;
		var readList = mLogBufferList.getReadList();
		int count = readList.Count;
		if (count > 0)
		{
			for (int i = 0; i < count; ++i)
			{
				totalString += readList[i] + "\r\n";
			}
			writeTxtFile(mLogFilePath, totalString, true);
		}
		readList.Clear();
	}
}
#endif
#endif