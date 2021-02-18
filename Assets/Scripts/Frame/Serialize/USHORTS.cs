using System;
using System.Collections.Generic;

public class USHORTS : OBJECTS
{
	protected const int TYPE_SIZE = sizeof(ushort);
	public ushort[] mValue;
	public USHORTS(int count)
	{
		mValue = new ushort[count];
		mType = Typeof<ushort[]>();
		mSize = TYPE_SIZE * mValue.Length;
	}
	public USHORTS(ushort[] value)
	{
		mValue = value;
		mType = Typeof<ushort[]>();
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
	public override void zero() { memset(mValue, (ushort)0); }
	public override bool readFromBuffer(byte[] buffer, ref int index)
	{
		if (!mVariableLength)
		{
			return readUShorts(buffer, ref index, mValue);
		}
		setRealSize(readUShort(buffer, ref index, out bool success));
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
		if (!writeUShort(buffer, ref index, mRealSize))
		{
			return false;
		}
		return writeUShorts(buffer, ref index, mValue, mElementCount);
	}
	public void set(ushort[] value)
	{
		int minCount = getMin(value.Length, mValue.Length);
		memcpy(mValue, value, 0, 0, minCount * TYPE_SIZE);
		if (mVariableLength)
		{
			setElementCount(minCount);
		}
	}
}