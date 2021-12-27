using System;
using System.Collections.Generic;
using System.Text;

// 自定义的对byte[]的封装,可用于序列化
public class BYTES : OBJECTS
{
	public byte[] mValue;			// 值
	public byte this[int index]
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
	public BYTES(int count)
		: base(count)
	{
		mValue = new byte[count];
		mType = typeof(byte[]);
		mTypeSize = sizeof(byte);
		mSize = mTypeSize * mValue.Length;
	}
	public BYTES(byte[] value)
		: base(value.Length)
	{
		mValue = value;
		mType = typeof(byte[]);
		mTypeSize = sizeof(byte);
		mSize = mTypeSize * mValue.Length;
	}
	public override void zero() { memset(mValue, (byte)0); }
	public override bool readFromBuffer(byte[] buffer, ref int index)
	{
		if (!mVariableLength)
		{
			return readBytes(buffer, ref index, mValue);
		}

		// 先读取数据的实际字节长度
		setElementCount(readElementCount(buffer, ref index, out bool success));
		if (!success)
		{
			return success;
		}
		return readBytes(buffer, ref index, mValue, -1, -1, mElementCount);
	}
	public override bool writeToBuffer(byte[] buffer, ref int index)
	{
		if (!mVariableLength)
		{
			return writeBytes(buffer, ref index, mValue);
		}

		// 先写入数据的实际字节长度
		if (!writeElementCount(buffer, ref index))
		{
			return false;
		}
		return writeBytes(buffer, ref index, mValue, -1, -1, mElementCount);
	}
	public void set(string value, Encoding encode, int destOffset = 0)
	{
		if (value == null)
		{
			return;
		}
		byte[] bytes = stringToBytes(value, encode);
		int count = getMin(bytes.Length, mValue.Length);
		memcpy(mValue, bytes, destOffset, 0, count);
		if (mVariableLength)
		{
			setElementCount(count);
		}
	}
	// 以UTF8编码转换为字节数组,因为stringToBytes中默认为UTF8
	public void set(string value, int destOffset = 0)
	{
		if(value == null)
		{
			return;
		}
		byte[] bytes = stringToBytes(value);
		int count = getMin(bytes.Length, mValue.Length);
		memcpy(mValue, bytes, destOffset, 0, count);
		if (mVariableLength)
		{
			setElementCount(count);
		}
	}
	public void set(byte[] value)
	{
		int minCount = getMin(value.Length, mValue.Length);
		memcpy(mValue, value, 0, 0, minCount);
		if (mVariableLength)
		{
			setElementCount(minCount);
		}
	}
	public void set(byte[] value, int destOffset, int srcOffset, int count)
	{
		int minCount = getMin(count, mValue.Length);
		memcpy(mValue, value, destOffset, srcOffset, minCount);
		if (mVariableLength)
		{
			setElementCount(minCount);
		}
	}
	public string getString(int startIndex = 0, int length = -1)
	{
		if(length == -1)
		{
			length = mElementCount - startIndex;
		}
		return bytesToString(mValue, startIndex, length);
	}
	public string getString(ref int startIndex, int length = -1)
	{
		if (length == -1)
		{
			length = mElementCount - startIndex;
		}
		string str = bytesToString(mValue, startIndex, length);
		startIndex += length;
		return str;
	}
	public string getString(Encoding encoding, int startIndex = 0, int length = -1)
	{
		if (length == -1)
		{
			length = mElementCount - startIndex;
		}
		return bytesToString(mValue, startIndex, length, encoding);
	}
}