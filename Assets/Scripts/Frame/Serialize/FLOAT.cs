using System;
using System.Collections.Generic;

// 自定义的对float的封装,提供类似于float指针的功能,可用于序列化
public class FLOAT : OBJECT
{
	public float mValue;			// 值
	public FLOAT()
	{
		mType = typeof(float);
		mSize = sizeof(float);
	}
	public FLOAT(float value)
	{
		mValue = value;
		mType = typeof(float);
		mSize = sizeof(float);
	}
	public override void zero() { mValue = 0.0f; }
	public void set(float value) { mValue = value; }
	public override bool readFromBuffer(byte[] buffer, ref int index)
	{
		mValue = readFloat(buffer, ref index, out bool success);
		return success;
	}
	public override bool writeToBuffer(byte[] buffer, ref int index)
	{
		return writeFloat(buffer, ref index, mValue);
	}
}