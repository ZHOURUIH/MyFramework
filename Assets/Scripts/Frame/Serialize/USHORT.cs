using System;
using System.Collections.Generic;

// 自定义的对ushort的封装,提供类似于ushort指针的功能,可用于序列化
public class USHORT : OBJECT
{
	public ushort mValue;		// 值
	public USHORT()
	{
		mType = typeof(ushort);
		mSize = sizeof(ushort);
	}
	public USHORT(ushort value)
	{
		mValue = value;
		mType = typeof(ushort);
		mSize = sizeof(ushort);
	}
	public override void zero() { mValue = 0; }
	public void set(ushort value) { mValue = value; }
	public override bool readFromBuffer(byte[] buffer, ref int index)
	{
		mValue = readUShort(buffer, ref index, out bool success);
		return success;
	}
	public override bool writeToBuffer(byte[] buffer, ref int index)
	{
		return writeUShort(buffer, ref index, mValue);
	}
}