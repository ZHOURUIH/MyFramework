#if !UNITY_EDITOR
using System;

public class LocalLog : FrameBase
{
	protected MyThread mWriteLogThread;
	// 日志双缓冲,使用双缓冲可以快速进行前后台切换,避免数据同步时出现耗时操作
	protected DoubleBuffer<string> mLogBufferList;
	protected string mLogFilePath;
	public LocalLog()
	{
		mLogBufferList = new DoubleBuffer<string>();
		mWriteLogThread = new MyThread("WriteLocalLog");
#if UNITY_STANDALONE_WIN
		mLogFilePath = FrameDefine.F_ASSETS_PATH + "log.txt";
#else
		mLogFilePath = FrameDefine.F_PERSISTENT_DATA_PATH + "log.txt";
#endif
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
		//mLogBufferList.add(info);
	}
	//-----------------------------------------------------------------------------------------------------------
	protected void writeLocalLog(BOOL run)
	{
		// 将当前写入缓冲区中的内容写入文件
		writeLogToFile();
	}
	protected void writeLogToFile()
	{
		//var readList = mLogBufferList.get();
		//int count = readList.Count;
		//if (count > 0)
		//{
		//	MyStringBuilder totalString = STRING_THREAD();
		//	for (int i = 0; i < count; ++i)
		//	{
		//		totalString.Append(readList[i], "\r\n");
		//	}
		//	writeTxtFile(mLogFilePath, END_STRING_THREAD(totalString), true);
		//}
		//readList.Clear();
	}
}
#endif