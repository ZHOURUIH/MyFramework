using System;
using System.Collections.Generic;

// 自定义的对int[]的封装,可用于序列化
public class INTS : OBJECTS
{
	public int[] mValue;			// 值
	public int this[int index]
	{
		get
		{
			checkIndex(index);
			return mValue[index];
		}
		set
		{
			checkIndex(index);
			mValue[index] = value;
		}
	}
	public INTS(int count)
		:base(count)
	{
		mValue = new int[count];
		mType = typeof(int[]);
		mTypeSize = sizeof(int);
		mSize = mTypeSize * mValue.Length;
	}
	public INTS(int[] value)
		: base(value.Length)
	{
		mValue = value;
		mType = typeof(int[]);
		mTypeSize = sizeof(int);
		mSize = mTypeSize * mValue.Length;
	}
	public override void zero() { memset(mValue, 0); }
	public override bool readFromBuffer(byte[] buffer, ref int index)
	{
		if (!mVariableLength)
		{
			return readInts(buffer, ref index, mValue);
		}

		// 先读取数据的实际字节长度
		setElementCount(readElementCount(buffer, ref index, out bool success));
		if (!success)
		{
			return false;
		}
		return readInts(buffer, ref index, mValue, mElementCount);
	}
	public override bool writeToBuffer(byte[] buffer, ref int index)
	{
		if (!mVariableLength)
		{
			return writeInts(buffer, ref index, mValue);
		}

		// 先写入数据的实际字节长度
		if (!writeElementCount(buffer, ref index))
		{
			return false;
		}
		return writeInts(buffer, ref index, mValue, mElementCount);
	}
	public void set(int[] value)
	{
		int minCount = getMin(value.Length, mValue.Length);
		memcpy(mValue, value, 0, 0, minCount * mTypeSize);
		if (mVariableLength)
		{
			setElementCount(minCount);
		}
	}
}