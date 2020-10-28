using System;
using System.Collections;
using System.Collections.Generic;

public class FLOATS : OBJECTS
{
	protected const int TYPE_SIZE = sizeof(float);
	public float[] mValue;
	public FLOATS(int count)
	{
		mValue = new float[count];
		mType = Typeof<float[]>();
		mSize = TYPE_SIZE * mValue.Length;
	}
	public FLOATS(float[] value)
	{
		mValue = value;
		mType = Typeof<float[]>();
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
	public override void zero() { memset(mValue, 0.0f); }
	public override bool readFromBuffer(byte[] buffer, ref int index)
	{
		if(mVariableLength)
		{
			// 先读取数据的实际字节长度
			bool success;
			setRealSize(readUShort(buffer, ref index, out success));
			if(!success)
			{
				return false;
			}
			return readFloats(buffer, ref index, mValue, mElementCount);
		}
		else
		{
			return readFloats(buffer, ref index, mValue);
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
			return writeFloats(buffer, ref index, mValue, mElementCount);
		}
		else
		{
			return writeFloats(buffer, ref index, mValue);
		}
	}
	public void set(float[] value)
	{
		int minCount = getMin(value.Length, mValue.Length);
		memcpy(mValue, value, 0, 0, minCount * TYPE_SIZE);
		if (mVariableLength)
		{
			setElementCount(minCount);
		}
	}
	public void set(float[] value, int destOffset, int srcOffset, int count)
	{
		int minCount = getMin(count, mValue.Length);
		memcpy(mValue, value, destOffset * TYPE_SIZE, srcOffset * TYPE_SIZE, minCount * TYPE_SIZE);
		if (mVariableLength)
		{
			setElementCount(minCount);
		}
	}
}