using System;
using System.Collections.Generic;

// 自定义的对byte的封装,提供类似于byte指针的功能,可用于序列化
public class BYTE : OBJECT
{
	public byte mValue;			// 值
	public BYTE()
	{
		mType = typeof(byte);
		mSize = sizeof(byte);
	}
	public BYTE(byte value)
	{
		mValue = value;
		mType = typeof(byte);
		mSize = sizeof(byte);
	}
	public override void zero()
	{
		mValue = 0;
	}
	public void set(byte value) { mValue = value; }
	public override bool readFromBuffer(byte[] buffer, ref int index)
	{
		mValue = readByte(buffer, ref index, out bool success);
		return success;
	}
	public override bool writeToBuffer(byte[] buffer, ref int index)
	{
		return writeByte(buffer, ref index, mValue);
	}
}