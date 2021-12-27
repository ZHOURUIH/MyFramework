using System;
using System.Collections.Generic;

// 自定义的对uint的封装,提供类似于uint指针的功能,可用于序列化
public class UINT : OBJECT
{
	public uint mValue;		// 值
	public UINT()
	{
		mType = typeof(uint);
		mSize = sizeof(uint);
	}
	public UINT(uint value)
	{
		mValue = value;
		mType = typeof(uint);
		mSize = sizeof(uint);
	}
	public override void zero() { mValue = 0; }
	public void set(uint value) { mValue = value; }
	public override bool readFromBuffer(byte[] buffer, ref int index)
	{
		mValue = readUInt(buffer, ref index, out bool success);
		return success;
	}
	public override bool writeToBuffer(byte[] buffer, ref int index)
	{
		return writeUInt(buffer, ref index, mValue);
	}
}