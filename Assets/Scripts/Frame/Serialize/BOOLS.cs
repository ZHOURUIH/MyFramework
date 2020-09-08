using System;
using System.Collections;
using System.Collections.Generic;

public class BOOLS : OBJECTS
{
	protected const int TYPE_SIZE = sizeof(bool);
	public bool[] mValue;
	public BOOLS(int count)
	{
		mValue = new bool[count];
		mType = typeof(bool[]);
		mSize = TYPE_SIZE * mValue.Length;
	}
	public BOOLS(bool[] value)
	{
		mValue = value;
		mType = typeof(bool[]);
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
	public override void zero() { memset(mValue, false); }
	public override bool readFromBuffer(byte[] buffer, ref int index)
	{
		if(mVariableLength)
		{
			// 先读取数据的实际字节长度
			bool success;
			setRealSize(readUShort(buffer, ref index, out success));
			if(!success)
			{
				return success;
			}
			return readBools(buffer, ref index, mValue, mElementCount);
		}
		else
		{
			return readBools(buffer, ref index, mValue);
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
			return writeBools(buffer, ref index, mValue, mElementCount);
		}
		else
		{
			return writeBools(buffer, ref index, mValue);
		}
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
}