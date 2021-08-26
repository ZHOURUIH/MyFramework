using System;
using System.Collections.Generic;

public abstract class OBJECTS : OBJECT
{
	protected int mTypeSize;                    // 单个元素的大小,由子类决定
	protected int mMaxElementCount;				// 初始设置的最大的元素个数
	public int mElementCount;					// 表示数组中有效的元素个数
	public int mRealSize;						// 表示数组中有效的字节个数,但是在使用int替换ullong时不表示真实数据长度
	public OBJECTS(int maxCount)
	{
		mVariableLength = true;
		mMaxElementCount = maxCount;
	}
	public void setRealSize(int realSize)
	{
		mRealSize = realSize;
		mElementCount = mRealSize / mTypeSize;
	}
	public void setElementCount(int elementCount)
	{
		mElementCount = elementCount;
		mRealSize = mElementCount * mTypeSize;
	}
	public int getMaxElementCount() { return mMaxElementCount; }
	public override int getSize()
	{
		if (!mVariableLength)
		{
			return base.getSize();
		}
		return mRealSize;
	}
	public void checkIndex(int index)
	{
		if (index >= mElementCount)
		{
			logError("下标超出有效数据长度:index:" + IToS(index) + ", mElementCount:" + IToS(mElementCount));
		}
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected int readElementCount(byte[] buffer, ref int index, out bool success)
	{
		int count;
		if (mMaxElementCount <= 0xFF)
		{
			count = readByte(buffer, ref index, out success);
		}
		else if (mMaxElementCount <= 0xFFFF)
		{
			count = readUShort(buffer, ref index, out success);
		}
		else
		{
			count = readInt(buffer, ref index, out success);
		}
		return count;
	}
	protected bool writeElementCount(byte[] buffer, ref int index)
	{
		bool success;
		if (mMaxElementCount <= 0xFF)
		{
			success = writeByte(buffer, ref index, (byte)mElementCount);
		}
		else if (mMaxElementCount <= 0xFFFF)
		{
			success = writeUShort(buffer, ref index, (ushort)mElementCount);
		}
		else
		{
			success = writeInt(buffer, ref index, mElementCount);
		}
		return success;
	}
}