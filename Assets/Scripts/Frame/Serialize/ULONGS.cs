using System;
using System.Collections.Generic;

public class ULONGS : OBJECTS
{
	public ulong[] mValue;
	protected bool mIntReplace;
	public ulong this[int index]
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
	public ULONGS(int count)
		: base(count)
	{
		mValue = new ulong[count];
		mType = typeof(ulong[]);
		mTypeSize = sizeof(ulong);
		mSize = mTypeSize * mValue.Length;
	}
	public ULONGS(ulong[] value)
		: base(value.Length)
	{
		mValue = value;
		mType = typeof(ulong[]);
		mTypeSize = sizeof(ulong);
		mSize = mTypeSize * mValue.Length;
	}
	public override void setIntReplaceULLong(bool replace) { mIntReplace = replace; }
	public override void zero() { memset(mValue, 0u); }
	public override bool readFromBuffer(byte[] buffer, ref int index)
	{
		if (!mVariableLength)
		{
			return readULongs(buffer, ref index, mValue);
		}

		// 先读取数据的实际字节长度
		setElementCount(readElementCount(buffer, ref index, out bool success));
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
			return readULongs(buffer, ref index, mValue, mElementCount);
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
				mValue[i] = (ulong)value;
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
			return writeULongs(buffer, ref index, mValue);
		}

		// 先写入数据的实际字节长度,无论是否使用int代替ullong
		if (!writeElementCount(buffer, ref index))
		{
			return false;
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
			return writeULongs(buffer, ref index, mValue, mElementCount);
		}
	}
	public void set(ulong[] value)
	{
		int minCount = getMin(value.Length, mValue.Length);
		memcpy(mValue, value, 0, 0, minCount * mTypeSize);
		if (mVariableLength)
		{
			setElementCount(minCount);
		}
	}
	public void set(List<ulong> value)
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