using System;
using System.Collections.Generic;

// 自定义的对bool的封装,提供类似于bool指针的功能,可用于序列化
public class BOOL : OBJECT
{
	public bool mValue;		// 值
	public BOOL()
	{
		mType = typeof(bool);
		mSize = sizeof(bool);
	}
	public BOOL(bool value)
	{
		mValue = value;
		mType = typeof(bool);
		mSize = sizeof(bool);
	}
	public override void zero() { mValue = false; }
	public void set(bool value) { mValue = value; }
	public override bool readFromBuffer(byte[] buffer, ref int index)
	{
		mValue = readBool(buffer, ref index, out bool success);
		return success;
	}
	public override bool writeToBuffer(byte[] buffer, ref int index)
	{
		return writeBool(buffer, ref index, mValue);
	}
}