using System;
using System.Collections.Generic;

public class SHORTS : OBJECTS
{
	protected const int TYPE_SIZE = sizeof(short);
	public short[] mValue;
	public short this[int index]
	{
		get
		{
			if (index >= mElementCount)
			{
				logError("下标超出有效数据长度");
			}
			return mValue[index];
		}
		set
		{
			if (index >= mElementCount)
			{
				logError("下标超出有效数据长度");
			}
			mValue[index] = value;
		}
	}
	public SHORTS(int count)
	{
		mValue = new short[count];
		mType = Typeof<short[]>();
		mSize = TYPE_SIZE * mValue.Length;
	}
	public SHORTS(short[] value)
	{
		mValue = value;
		mType = Typeof<short[]>();
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
	public override void zero() { memset(mValue, (short)0); }
	public override bool readFromBuffer(byte[] buffer, ref int index)
	{
		if (!mVariableLength)
		{
			return readShorts(buffer, ref index, mValue);
		}
		// 先读取数据的实际字节长度
		setRealSize(readUShort(buffer, ref index, out bool success));
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
		if (!writeUShort(buffer, ref index, mRealSize))
		{
			return false;
		}
		return writeShorts(buffer, ref index, mValue, mElementCount);
	}
	public void set(short[] value)
	{
		int minCount = getMin(value.Length, mValue.Length);
		memcpy(mValue, value, 0, 0, minCount * TYPE_SIZE);
		if (mVariableLength)
		{
			setElementCount(minCount);
		}
	}
}