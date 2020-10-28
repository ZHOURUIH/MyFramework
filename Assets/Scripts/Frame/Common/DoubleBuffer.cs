using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

// 双缓冲,线程安全的缓冲区,可在多个线程中写入数据,一个线程中读取数据
public class DoubleBuffer<T> : FrameBase
{
	protected List<T>[] mBufferList;
	protected ThreadLock mBufferLock;
	protected uint mWriteListLimit;      // 写缓冲区可存储的最大数量,当到达上限时无法再写入,等于0表示无上限
	protected int mWriteIndex;
	protected int mReadIndex;
	public DoubleBuffer()
	{
		mBufferList = new List<T>[2];
		mBufferList[0] = new List<T>();
		mBufferList[1] = new List<T>();
		mBufferLock = new ThreadLock();
		mWriteListLimit = 0;
		mWriteIndex = 0;
		mReadIndex = 1;
	}
	// 切换缓冲区,获得可读列表,在遍历可读列表期间,不能再次调用getReadList,否则会出现不可预知的错误,并且该函数不能在多个线程中调用
	public List<T> getReadList()
	{
		mBufferLock.waitForUnlock();
		swap(ref mReadIndex, ref mWriteIndex);
		mBufferLock.unlock();
		return mBufferList[mReadIndex];
	}
	// 向可写列表中添加数据,可在不同线程中调用
	public void addToBuffer(T value)
	{
		mBufferLock.waitForUnlock();
		if(mWriteListLimit == 0 || mBufferList[mWriteIndex].Count < mWriteListLimit)
		{
			mBufferList[mWriteIndex].Add(value);
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