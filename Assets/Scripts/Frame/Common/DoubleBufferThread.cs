using System.Collections.Generic;
using System.Threading;

// 双缓冲,线程安全的缓冲区,可在多个线程中写入数据,一个线程中读取数据
public class DoubleBufferThread<T> : FrameBase
{
	protected List<T>[] mBufferList;
	protected ThreadLock mBufferLock;
	protected uint mWriteListLimit;      // 写缓冲区可存储的最大数量,当到达上限时无法再写入,等于0表示无上限
	protected int mWriteIndex;
	protected int mReadIndex;
	protected int mReadThreadID;
	public DoubleBufferThread()
	{
		mBufferList = new List<T>[2];
		mBufferList[0] = new List<T>();
		mBufferList[1] = new List<T>();
		mBufferLock = new ThreadLock();
		mWriteListLimit = 0;
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
	// 向可写列表中添加数据,可在不同线程中调用
	public void add(T value)
	{
		mBufferLock.waitForUnlock();
		List<T> writeList = mBufferList[mWriteIndex];
		if (mWriteListLimit == 0 || writeList.Count < mWriteListLimit)
		{
			writeList.Add(value);
		}
		mBufferLock.unlock();
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