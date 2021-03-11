using System.Collections.Generic;
using System.Threading;

// 双缓冲,线程安全的缓冲区,可在一个线程中写入数据,另一个线程中读取数据
public class DoubleBuffer<T> : FrameBase
{
	protected List<T>[] mBufferList;
	protected ThreadLock mBufferLock;
	protected uint mWriteListLimit;      // 写缓冲区可存储的最大数量,当到达上限时无法再写入,等于0表示无上限
	protected int mWriteIndex;
	protected int mReadIndex;
	protected int mReadThreadID;
	protected int mWriteThreadID;
	public DoubleBuffer()
	{
		mBufferList = new List<T>[2];
		mBufferList[0] = new List<T>();
		mBufferList[1] = new List<T>();
		mBufferLock = new ThreadLock();
		mWriteIndex = 0;
		mReadIndex = 1;
	}
	// 切换缓冲区,获得可读列表,在遍历可读列表期间,不能再次调用get,否则会出现不可预知的错误,并且该函数不能在多个线程中调用
	public List<T> get()
	{
		int curThreadID = Thread.CurrentThread.ManagedThreadId;
		mBufferLock.waitForUnlock();
		if (mReadThreadID == 0)
		{
			mReadThreadID = curThreadID;
		}
		swap(ref mReadIndex, ref mWriteIndex);
		List<T> readList = mBufferList[mReadIndex];
		mBufferLock.unlock();
		if (mReadThreadID != curThreadID)
		{
			logError("不能在不同线程中获得可读列表");
			return null;
		}
		return readList;
	}
	// 向可写列表中添加数据
	public void add(T value)
	{
		int curThreadID = Thread.CurrentThread.ManagedThreadId;
		mBufferLock.waitForUnlock();
		if (mWriteThreadID == 0)
		{
			mWriteThreadID = curThreadID;
		}
		List<T> writeList = mBufferList[mWriteIndex];
		mBufferLock.unlock();
		if (mWriteThreadID != curThreadID)
		{
			logError("不能在不同线程中向双缓冲添加数据");
			return;
		}
		if (mWriteListLimit == 0 || writeList.Count < mWriteListLimit)
		{
			writeList.Add(value);
		}
	}
	public void setWriteListLimit(uint limit) { mWriteListLimit = limit; }
	public void clear()
	{
		mBufferLock.waitForUnlock();
		mBufferList[mReadIndex].Clear();
		mBufferList[mWriteIndex].Clear();
		mBufferLock.unlock();
	}
}