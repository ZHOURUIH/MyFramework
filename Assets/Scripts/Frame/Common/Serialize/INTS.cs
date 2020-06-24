using System;
using System.Collections;
using System.Collections.Generic;

public class INTS : OBJECTS
{
	protected const int TYPE_SIZE = sizeof(int);
	public int[] mValue;
	public INTS(int count)
	{
		mValue = new int[count];
		mType = typeof(int[]);
		mSize = TYPE_SIZE * mValue.Length;
	}
	public INTS(int[] value)
	{
		mValue = value;
		mType = typeof(int[]);
		mSize = TYPE_SIZE * mValue.Length;
	}
	public override void setRealSize(ushort realSize)
	{
		mRealSize = realSize;
		mElementCount = mRealSize / TYPE_SIZE;
	}
	public override void setElementCount(int elementCount)
	{
		mElementCount = elementCount;
		mRealSize = (ushort)(mElementCount * TYPE_SIZE);
	}
	public override void zero() { memset(mValue, 0); }
	public override bool readFromBuffer(byte[] buffer, ref int index)
	{
		if(mVariableLength)
		{
			// 先读取数据的实际字节长度
			bool success;
			setRealSize(readUShort(buffer, ref index, out success));
			if(!success)
			{
				return false;
			}
			return readInts(buffer, ref index, mValue, mElementCount);
		}
		else
		{
			return readInts(buffer, ref index, mValue);
		}
	}
	public override bool writeToBuffer(byte[] buffer, ref int index)
	{
		if(mVariableLength)
		{
			// 先写入数据的实际字节长度
			if(!writeUShort(buffer, ref index, mRealSize))
			{
				return false;
			}
			return writeInts(buffer, ref index, mValue, mRealSize / mElementCount);
		}
		else
		{
			return writeInts(buffer, ref index, mValue);
		}
	}
	public void set(int[] value)
	{
		int minCount = getMin(value.Length, mValue.Length);
		memcpy(mValue, value, 0, 0, minCount * TYPE_SIZE);
		if (mVariableLength)
		{
			setElementCount(minCount);
		}
	}
}