using System;
using System.Collections.Generic;

// 自定义的对ulong的封装,提供类似于ulong指针的功能,可用于序列化
public class ULONG : OBJECT
{
	public ulong mValue;			// 值
	protected bool mIntReplace;		// 是否在合适的条件下使用int代替long来减小内存占用
	public ULONG()
	{
		mType = typeof(ulong);
		mSize = sizeof(ulong);
	}
	public ULONG(ulong value)
	{
		mValue = value;
		mType = typeof(ulong);
		mSize = sizeof(ulong);
	}
	public override void setIntReplaceULLong(bool replace)
	{
		mIntReplace = replace;
	}
	public override void zero() { mValue = 0; }
	public void set(ulong value) { mValue = value; }
	public override bool readFromBuffer(byte[] buffer, ref int index)
	{
		bool success;
		// 如果第一个字节的最高位是1,则表示应该读取4字节
		// 因为存储是小端模式,最高的字节在高地址,所以为了方便解析,使低地址的最高位作为标记位,先左移一位,使低字节的最低字节空出来作为标记位
		// 所以解析时首先找到标记位
		if (mIntReplace && getLowestBit(buffer[index]) == 1)
		{
			// 右移一位,还原数据
			// 因为右移int可能会使符号位改变,最终造成数据错误,所以需要转换为uint进行右移
			mValue = ((uint)readInt(buffer, ref index, out success)) >> 1;
		}
		else
		{
			// 右移一位,还原数据
			mValue = readULong(buffer, ref index, out success) >> 1;
		}
		return success;
	}
	public override bool writeToBuffer(byte[] buffer, ref int index)
	{
		if (mIntReplace && mValue < 0x7FFFFFFF)
		{
			// 左移一位,设置标记位
			int value = (int)mValue << 1;
			setLowestBit(ref value, 1);
			return writeInt(buffer, ref index, value);
		}
		else
		{
			// 左移一位,使标记位为0
			return writeULong(buffer, ref index, mValue << 1);
		}
	}
}