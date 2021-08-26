using System.Collections.Generic;
using System.Threading;

// 双缓冲,线程安全的缓冲区,可在一个线程中写入数据,另一个线程中读取数据
public class DoubleBuffer<T> : FrameBase
{
	protected List<T>[] mBufferList;	// 双缓冲列表
	protected ThreadLock mBufferLock;	// 双缓冲锁
	protected uint mWriteListLimit;		// 写缓冲区可存储的最大数量,当到达上限时无法再写入,等于0表示无上限
	protected int mWriteIndex;			// 当前的写入列表下标
	protected int mReadIndex;			// 当前的读取列表下标
	protected int mReadThreadID;		// 读取数据的线程ID
	protected int mWriteThreadID;		// 写入数据的线程ID
	protected bool mReading;			// 是否正在使用读列表中
	public DoubleBuffer()
	{
		mBufferList = new List<T>[2];
		mBufferList[0] = new List<T>();
		mBufferList[1] = new List<T>();
		mBufferLock = new ThreadLock();
		mWriteIndex = 0;
		mReadIndex = 1;
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mBufferList[0].Clear();
		mBufferList[1].Clear();
		mBufferLock.unlock();
		mWriteListLimit = 0;
		mWriteIndex = 0;
		mReadIndex = 1;
		mReadThreadID = 0;
		mWriteThreadID = 0;
		mReading = false;
	}
	// 切换缓冲区,获得可读列表,在遍历可读列表期间,不能再次调用get,否则会出现不可预知的错误,并且该函数不能在多个线程中调用
	public List<T> get()
	{
		if (mReading)
		{
			logError("读列表正在使用中,不能再次获取读列表");
			return null;
		}
		mReading = true;
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
	public void endGet()
	{
		mReading = false;
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
		if (mReading)
		{
			logError("正在使用读列表中,无法清空");
		}
		mBufferLock.waitForUnlock();
		mBufferList[mReadIndex].Clear();
		mBufferList[mWriteIndex].Clear();
		mBufferLock.unlock();
	}
	// 只有当完全确定双缓冲的状态时才能直接访问内部列表,否则可能会出错误
	public List<T>[] getBufferList() { return mBufferList; }
}