using System;
using System.Collections.Generic;

public class UINTS : OBJECTS
{
	protected const int TYPE_SIZE = sizeof(uint);
	public uint[] mValue;
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
	{
		mValue = new uint[count];
		mType = typeof(uint[]);
		mSize = TYPE_SIZE * mValue.Length;
	}
	public UINTS(uint[] value)
	{
		mValue = value;
		mType = typeof(uint[]);
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
	public override void zero() { memset(mValue, (uint)0); }
	public override bool readFromBuffer(byte[] buffer, ref int index)
	{
		if (!mVariableLength)
		{
			return readUInts(buffer, ref index, mValue);
		}
		// 先读取数据的实际字节长度
		setRealSize(readUShort(buffer, ref index, out bool success));
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
		if (!writeUShort(buffer, ref index, mRealSize))
		{
			return false;
		}
		return writeUInts(buffer, ref index, mValue, mElementCount);
	}
	public void set(uint[] value)
	{
		int minCount = getMin(value.Length, mValue.Length);
		memcpy(mValue, value, 0, 0, minCount * TYPE_SIZE);
		if (mVariableLength)
		{
			setElementCount(minCount);
		}
	}
}