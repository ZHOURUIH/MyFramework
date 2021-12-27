using System;
using System.Collections.Generic;

// 自定义的对float[]的封装,可用于序列化
public class FLOATS : OBJECTS
{
	public float[] mValue;			// 值
	public float this[int index]
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
	public FLOATS(int count)
		: base(count)
	{
		mValue = new float[count];
		mType = typeof(float[]);
		mTypeSize = sizeof(float);
		mSize = mTypeSize * mValue.Length;
	}
	public FLOATS(float[] value)
		: base(value.Length)
	{
		mValue = value;
		mType = typeof(float[]);
		mTypeSize = sizeof(float);
		mSize = mTypeSize * mValue.Length;
	}
	public override void zero() { memset(mValue, 0.0f); }
	public override bool readFromBuffer(byte[] buffer, ref int index)
	{
		if (!mVariableLength)
		{
			return readFloats(buffer, ref index, mValue);
		}

		// 先读取数据的实际字节长度
		setElementCount(readElementCount(buffer, ref index, out bool success));
		if (!success)
		{
			return false;
		}
		return readFloats(buffer, ref index, mValue, mElementCount);
	}
	public override bool writeToBuffer(byte[] buffer, ref int index)
	{
		if (!mVariableLength)
		{
			return writeFloats(buffer, ref index, mValue);
		}

		// 先写入数据的实际字节长度
		if (!writeElementCount(buffer, ref index))
		{
			return false;
		}
		return writeFloats(buffer, ref index, mValue, mElementCount);
	}
	public void set(float[] value)
	{
		int minCount = getMin(value.Length, mValue.Length);
		memcpy(mValue, value, 0, 0, minCount * mTypeSize);
		if (mVariableLength)
		{
			setElementCount(minCount);
		}
	}
	public void set(float[] value, int destOffset, int srcOffset, int count)
	{
		int minCount = getMin(count, mValue.Length);
		memcpy(mValue, value, destOffset * mTypeSize, srcOffset * mTypeSize, minCount * mTypeSize);
		if (mVariableLength)
		{
			setElementCount(minCount);
		}
	}
}