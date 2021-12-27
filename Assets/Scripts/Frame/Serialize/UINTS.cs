using System;
using System.Collections.Generic;

// 自定义的对uint[]的封装,可用于序列化
public class UINTS : OBJECTS
{
	public uint[] mValue;		// 值
	public uint this[int index]
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
	public UINTS(int count)
		: base(count)
	{
		mValue = new uint[count];
		mType = typeof(uint[]);
		mTypeSize = sizeof(uint);
		mSize = mTypeSize * mValue.Length;
	}
	public UINTS(uint[] value)
		: base(value.Length)
	{
		mValue = value;
		mType = typeof(uint[]);
		mTypeSize = sizeof(uint);
		mSize = mTypeSize * mValue.Length;
	}
	public override void zero() { memset(mValue, (uint)0); }
	public override bool readFromBuffer(byte[] buffer, ref int index)
	{
		if (!mVariableLength)
		{
			return readUInts(buffer, ref index, mValue);
		}

		// 先读取数据的实际字节长度
		setElementCount(readElementCount(buffer, ref index, out bool success));
		if (!success)
		{
			return false;
		}
		return readUInts(buffer, ref index, mValue, mElementCount);
	}
	public override bool writeToBuffer(byte[] buffer, ref int index)
	{
		if (!mVariableLength)
		{
			return writeUInts(buffer, ref index, mValue);
		}

		// 先写入数据的实际字节长度
		if (!writeElementCount(buffer, ref index))
		{
			return false;
		}
		return writeUInts(buffer, ref index, mValue, mElementCount);
	}
	public void set(uint[] value)
	{
		int minCount = getMin(value.Length, mValue.Length);
		memcpy(mValue, value, 0, 0, minCount * mTypeSize);
		if (mVariableLength)
		{
			setElementCount(minCount);
		}
	}
}