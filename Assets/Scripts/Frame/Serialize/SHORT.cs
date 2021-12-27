using System;
using System.Collections.Generic;

// 自定义的对short的封装,提供类似于short指针的功能,可用于序列化
public class SHORT : OBJECT
{
	public short mValue;		// 值
	public SHORT()
	{
		mType = typeof(short);
		mSize = sizeof(short);
	}
	public SHORT(short value)
	{
		mValue = value;
		mType = typeof(short);
		mSize = sizeof(short);
	}
	public override void zero() { mValue = 0; }
	public void set(short value) { mValue = value; }
	public override bool readFromBuffer(byte[] buffer, ref int index)
	{
		mValue = readShort(buffer, ref index, out bool success);
		return success;
	}
	public override bool writeToBuffer(byte[] buffer, ref int index)
	{
		return writeShort(buffer, ref index, mValue);
	}
}