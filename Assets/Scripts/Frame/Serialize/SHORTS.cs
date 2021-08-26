using System;
using System.Collections.Generic;

public class SHORTS : OBJECTS
{
	public short[] mValue;
	public short this[int index]
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
	public SHORTS(int count)
		: base(count)
	{
		mValue = new short[count];
		mType = typeof(short[]);
		mTypeSize = sizeof(short);
		mSize = mTypeSize * mValue.Length;
	}
	public SHORTS(short[] value)
		: base(value.Length)
	{
		mValue = value;
		mType = typeof(short[]);
		mTypeSize = sizeof(short);
		mSize = mTypeSize * mValue.Length;
	}
	public override void zero() { memset(mValue, (short)0); }
	public override bool readFromBuffer(byte[] buffer, ref int index)
	{
		if (!mVariableLength)
		{
			return readShorts(buffer, ref index, mValue);
		}

		// 先读取数据的实际字节长度
		setElementCount(readElementCount(buffer, ref index, out bool success));
		if (!success)
		{
			return false;
		}
		return readShorts(buffer, ref index, mValue, mElementCount);
	}
	public override bool writeToBuffer(byte[] buffer, ref int index)
	{
		if (!mVariableLength)
		{
			return writeShorts(buffer, ref index, mValue);
		}

		// 先写入数据的实际字节长度
		if (!writeElementCount(buffer, ref index))
		{
			return false;
		}
		return writeShorts(buffer, ref index, mValue, mElementCount);
	}
	public void set(short[] value)
	{
		int minCount = getMin(value.Length, mValue.Length);
		memcpy(mValue, value, 0, 0, minCount * mTypeSize);
		if (mVariableLength)
		{
			setElementCount(minCount);
		}
	}
}