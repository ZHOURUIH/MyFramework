using System.Collections.Generic;
using System.Threading;
using static UnityUtility;
using static MathUtility;

// 双缓冲,线程安全的缓冲区,可在多个线程中写入数据,一个线程中读取数据
public class DoubleBuffer<T>
{
	protected List<T>[] mBufferList;			// 双缓冲列表
	protected ThreadLock mBufferLock = new();	// 双缓冲锁
	protected int mWriteListLimit;				// 写缓冲区可存储的最大数量,当到达上限时无法再写入,等于0表示无上限
	protected int mWriteIndex = 0;				// 当前的写入列表下标
	protected int mReadIndex = 1;				// 当前的读取列表下标
	protected int mReadThreadID;				// 读取数据的线程ID
	protected volatile int mReading;			// 是否正在使用读列表中
	public DoubleBuffer()
	{
		mBufferList = new List<T>[2] { new(), new()};
	}
	public void destroy()
	{
		clear();
		mReadThreadID = 0;
		mReading = 0;
	}
	// 切换缓冲区,获得可读列表,在遍历可读列表期间,不能再次调用get,否则会出现不可预知的错误,并且该函数不能在多个线程中调用
	public List<T> get()
	{
		int curThreadID = Thread.CurrentThread.ManagedThreadId;
		List<T> readList = null;
		using (new ThreadLockScope(mBufferLock))
		{
			if (Interlocked.CompareExchange(ref mReading, 0, 0) != 0)
			{
				logError("读列表正在使用中,不能再次获取读列表");
				return null;
			}
			Interlocked.Exchange(ref mReading, 1);
			if (mReadThreadID == 0)
			{
				mReadThreadID = curThreadID;
			}
			swap(ref mReadIndex, ref mWriteIndex);
			readList = mBufferList[mReadIndex];
		}
		if (mReadThreadID != curThreadID)
		{
			logError("不能在不同线程中获得可读列表");
			return null;
		}
		return readList;
	}
	public void endGet()
	{
		Interlocked.Exchange(ref mReading, 0);
	}
	// 向可写列表中添加数据,可在不同线程中调用
	public void add(T value)
	{
		using (new ThreadLockScope(mBufferLock))
		{
			List<T> writeList = mBufferList[mWriteIndex];
			writeList.addIf(value, mWriteListLimit == 0 || writeList.Count < mWriteListLimit);
		}
	}
	public void setWriteListLimit(int limit) { mWriteListLimit = limit; }
	public void clear()
	{
		if (Interlocked.CompareExchange(ref mReading, 0, 0) != 0)
		{
			logError("正在使用读列表中,无法清空");
		}
		using (new ThreadLockScope(mBufferLock))
		{
			mBufferList[mReadIndex].Clear();
			mBufferList[mWriteIndex].Clear();
		}
	}
	// 只有当完全确定双缓冲的状态时才能直接访问内部列表,否则可能会出错误
	public List<T>[] getBufferList() { return mBufferList; }
}