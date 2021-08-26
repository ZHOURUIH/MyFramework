using System;
using System.Collections.Generic;

public class USHORTS : OBJECTS
{
	public ushort[] mValue;
	public ushort this[int index]
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
	public USHORTS(int count)
		: base(count)
	{
		mValue = new ushort[count];
		mType = typeof(ushort[]);
		mTypeSize = sizeof(ushort);
		mSize = mTypeSize * mValue.Length;
	}
	public USHORTS(ushort[] value)
		: base(value.Length)
	{
		mValue = value;
		mType = typeof(ushort[]);
		mTypeSize = sizeof(ushort);
		mSize = mTypeSize * mValue.Length;
	}
	public override void zero() { memset(mValue, (ushort)0); }
	public override bool readFromBuffer(byte[] buffer, ref int index)
	{
		if (!mVariableLength)
		{
			return readUShorts(buffer, ref index, mValue);
		}

		setElementCount(readElementCount(buffer, ref index, out bool success));
		if (!success)
		{
			return false;
		}
		return readUShorts(buffer, ref index, mValue, mElementCount);
	}
	public override bool writeToBuffer(byte[] buffer, ref int index)
	{
		if (!mVariableLength)
		{
			return writeUShorts(buffer, ref index, mValue);
		}

		if (!writeElementCount(buffer, ref index))
		{
			return false;
		}
		return writeUShorts(buffer, ref index, mValue, mElementCount);
	}
	public void set(ushort[] value)
	{
		int minCount = getMin(value.Length, mValue.Length);
		memcpy(mValue, value, 0, 0, minCount * mTypeSize);
		if (mVariableLength)
		{
			setElementCount(minCount);
		}
	}
}