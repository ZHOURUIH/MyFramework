using System;
using System.Collections.Generic;

public class LONGS : OBJECTS
{
	protected const int TYPE_SIZE = sizeof(long);
	public long[] mValue;
	protected bool mIntReplace;
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
		mType = typeof(long[]);
		mSize = TYPE_SIZE * mValue.Length;
	}
	public LONGS(long[] value)
	{
		mValue = value;
		mType = typeof(long[]);
		mSize = TYPE_SIZE * mValue.Length;
	}
	public override void setIntReplaceULLong(bool replace)
	{
		mIntReplace = replace;
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
		if (mElementCount == 0)
		{
			return true;
		}
		// 如果数组的第一个字节的最高位是,则表示每个元素只需要读取4字节的数据,否则还是读取8字节
		// 因为存储是小端模式,最高的字节在高地址,所以为了方便解析,使低地址的最高位作为标记位,先左移一位,使低字节的最低字节空出来作为标记位
		// 所以解析时首先找到标记位
		if (!mIntReplace || getLowestBit(buffer[index]) == 0)
		{
			return readLongs(buffer, ref index, mValue, mElementCount);
		}
		int intCount = mElementCount < 0 ? mValue.Length : mElementCount;
		for (int i = 0; i < intCount; ++i)
		{
			int value = readInt(buffer, ref index, out success);
			// 恢复标记位,右移一位,只需要处理第一个元素
			if (i == 0)
			{
				setLowestBit(ref value, 0);
				// 因为右移ing可能会使符号位变为1从而变成负数,最终造成数据错误,所以需要转换为uint进行右移
				mValue[i] = ((uint)value) >> 1;
			}
			else
			{
				mValue[i] = value;
			}
			if (!success)
			{
				return false;
			}
		}
		return true;
	}
	public override bool writeToBuffer(byte[] buffer, ref int index)
	{
		if (!mVariableLength)
		{
			return writeLongs(buffer, ref index, mValue);
		}
		bool useInt = true;
		for (int i = 0; i < mElementCount; ++i)
		{
			if (mValue[i] >= 0x7FFFFFFF)
			{
				useInt = false;
				break;
			}
		}
		// 先写入数据的实际字节长度,无论是否使用int代替ullong
		if (!writeUShort(buffer, ref index, mRealSize))
		{
			return false;
		}
		// 如果使用int就可以表示,则只写入4字节的数据
		if (useInt)
		{
			bool ret = true;
			int count = mElementCount < 0 ? mValue.Length : mElementCount;
			for (int i = 0; i < count; ++i)
			{
				int value = (int)mValue[i];
				// 只设置数组的第一个元素的标记位
				// 左移一位,设置标记位
				if (i == 0)
				{
					value <<= 1;
					setLowestBit(ref value, 1);
				}
				ret = writeInt(buffer, ref index, value) && ret;
			}
			return ret;
		}
		else
		{
			return writeLongs(buffer, ref index, mValue, mElementCount);
		}
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