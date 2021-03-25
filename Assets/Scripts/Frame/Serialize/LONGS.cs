using System;
using System.Collections.Generic;

public class LONGS : OBJECTS
{
	protected const int TYPE_SIZE = sizeof(long);
	public long[] mValue;
	public long this[int index]
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
	public LONGS(int count)
	{
		mValue = new long[count];
		mType = Typeof<long[]>();
		mSize = TYPE_SIZE * mValue.Length;
	}
	public LONGS(long[] value)
	{
		mValue = value;
		mType = Typeof<long[]>();
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
	public override void zero()
	{
		memset(mValue, 0);
	}
	public override bool readFromBuffer(byte[] buffer, ref int index)
	{
		if (!mVariableLength)
		{
			return readLongs(buffer, ref index, mValue);
		}
		// 先读取数据的实际字节长度
		setRealSize(readUShort(buffer, ref index, out bool success));
		if (!success)
		{
			return false;
		}
		return readLongs(buffer, ref index, mValue, mElementCount);
	}
	public override bool writeToBuffer(byte[] buffer, ref int index)
	{
		if (!mVariableLength)
		{
			return writeLongs(buffer, ref index, mValue);
		}
		// 先写入数据的实际字节长度
		if (!writeUShort(buffer, ref index, mRealSize))
		{
			return false;
		}
		return writeLongs(buffer, ref index, mValue, mElementCount);
	}
	public void set(long[] value)
	{
		int minCount = getMin(value.Length, mValue.Length);
		memcpy(mValue, value, 0, 0, minCount * TYPE_SIZE);
		if (mVariableLength)
		{
			setElementCount(minCount);
		}
	}
	public void set(List<long> value)
	{
		int minCount = getMin(value.Count, mValue.Length);
		for(int i = 0; i < minCount; ++i)
		{
			mValue[i] = value[i];
		}
		if (mVariableLength)
		{
			setElementCount(minCount);
		}
	}
}