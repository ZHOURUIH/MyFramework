using System.Collections.Generic;
using System;

// 辅助读取DoubleBuffer的类,可以自动清空读取列表,并且结束读取
public struct DoubleBufferReader<T> : IDisposable
{
	private DoubleBuffer<T> mBuffer;    // 双缓冲对象
	public List<T> mReadList;				// 读取的列表
	public DoubleBufferReader(DoubleBuffer<T> buffer)
	{
		mBuffer = buffer;
		mReadList = mBuffer.get();
	}
	public readonly void Dispose()
	{
		mReadList?.Clear();
		mBuffer.endGet();
	}
}