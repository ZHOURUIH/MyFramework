using System;
using System.Collections.Generic;

// 自定义的对bool[]的封装,可用于序列化
public class BOOLS : OBJECTS
{
	public bool[] mValue;		// 值
	public bool this[int index]
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
	public BOOLS(int count)
		: base(count)
	{
		mValue = new bool[count];
		mType = typeof(bool[]);
		mTypeSize = sizeof(bool);
		mSize = mTypeSize * mValue.Length;
	}
	public BOOLS(bool[] value)
		: base(value.Length)
	{
		mValue = value;
		mType = typeof(bool[]);
		mTypeSize = sizeof(bool);
		mSize = mTypeSize * mValue.Length;
	}
	public override void zero() { memset(mValue, false); }
	public override bool readFromBuffer(byte[] buffer, ref int index)
	{
		if (!mVariableLength)
		{
			return readBools(buffer, ref index, mValue);
		}

		// 先读取数据的实际字节长度
		setElementCount(readElementCount(buffer, ref index, out bool success));
		if (!success)
		{
			return success;
		}
		return readBools(buffer, ref index, mValue, mElementCount);
	}
	public override bool writeToBuffer(byte[] buffer, ref int index)
	{
		if (!mVariableLength)
		{
			return writeBools(buffer, ref index, mValue);
		}

		// 先写入数据的实际字节长度
		if (!writeElementCount(buffer, ref index))
		{
			return false;
		}
		return writeBools(buffer, ref index, mValue, mElementCount);
	}
	public void set(bool[] value)
	{
		int minCount = getMin(value.Length, mValue.Length);
		memcpy(mValue, value, 0, 0, minCount);
		if(mVariableLength)
		{
			setElementCount(minCount);
		}
	}
	public void set(bool value, int count)
	{
		int minCount = getMin(count, mValue.Length);
		for (int i = 0; i < minCount; ++i)
		{
			mValue[i] = value;
		}
		if (mVariableLength)
		{
			setElementCount(minCount);
		}
	}
	public void set(List<bool> value)
	{
		int minCount = getMin(value.Count, mValue.Length);
		for (int i = 0; i < minCount; ++i)
		{
			mValue[i] = value[i];
		}
		if (mVariableLength)
		{
			setElementCount(minCount);
		}
	}
}