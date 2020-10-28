using System;
using System.Collections;
using System.Collections.Generic;

public class SHORTS : OBJECTS
{
	protected const int TYPE_SIZE = sizeof(short);
	public short[] mValue;
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
		if (mVariableLength)
		{
			// 先读取数据的实际字节长度
			bool success;
			setRealSize(readUShort(buffer, ref index, out success));
			if(!success)
			{
				return false;
			}
			return readShorts(buffer, ref index, mValue, mElementCount);
		}
		else
		{
			return readShorts(buffer, ref index, mValue);
		}
	}
	public override bool writeToBuffer(byte[] buffer, ref int index)
	{
		if (mVariableLength)
		{
			// 先写入数据的实际字节长度
			if(!writeUShort(buffer, ref index, mRealSize))
			{
				return false;
			}
			return writeShorts(buffer, ref index, mValue, mElementCount);
		}
		else
		{
			return writeShorts(buffer, ref index, mValue);
		}
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