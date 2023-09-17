using System.Collections.Generic;
using System;

public struct DoubleBufferReader<T> : IDisposable
{
	public DoubleBuffer<T> mBuffer;
	public List<T> mList;
	public DoubleBufferReader(DoubleBuffer<T> buffer, out List<T> readingList)
	{
		mBuffer = buffer;
		mList = mBuffer.get();
		readingList = mList;
	}
	public void Dispose()
	{
		mList?.Clear();
		mBuffer.endGet();
	}
}