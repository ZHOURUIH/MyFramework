using System;
using System.Collections.Generic;

public class BOOLS : OBJECTS
{
	protected const int TYPE_SIZE = sizeof(bool);
	public bool[] mValue;
	public bool this[int index]
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
	public BOOLS(int count)
	{
		mValue = new bool[count];
		mType = Typeof<bool[]>();
		mSize = TYPE_SIZE * mValue.Length;
	}
	public BOOLS(bool[] value)
	{
		mValue = value;
		mType = Typeof<bool[]>();
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
		if (!mVariableLength)
		{
			return readBools(buffer, ref index, mValue);
		}
		// 先读取数据的实际字节长度
		setRealSize(readUShort(buffer, ref index, out bool success));
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
		if (!writeUShort(buffer, ref index, mRealSize))
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