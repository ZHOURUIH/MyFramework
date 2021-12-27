using System;
using System.Collections.Generic;

// 自定义的对int的封装,提供类似于int指针的功能,可用于序列化
public class INT : OBJECT
{
	public int mValue;			// 值
	public INT()
	{
		mType = typeof(int);
		mSize = sizeof(int);
	}
	public INT(int value)
	{
		mValue = value;
		mType = typeof(int);
		mSize = sizeof(int);
	}
	public override void zero() { mValue = 0; }
	public void set(int value) { mValue = value; }
	public override bool readFromBuffer(byte[] buffer, ref int index)
	{
		mValue = readInt(buffer, ref index, out bool success);
		return success;
	}
	public override bool writeToBuffer(byte[] buffer, ref int index)
	{
		return writeInt(buffer, ref index, mValue);
	}
}