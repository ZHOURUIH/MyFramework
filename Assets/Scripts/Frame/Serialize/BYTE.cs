using System;
using System.Collections;
using System.Collections.Generic;

public class BYTE : OBJECT
{
	protected const int TYPE_SIZE = sizeof(byte);
	public byte mValue;
	public BYTE()
	{
		mType = typeof(byte);
		mSize = TYPE_SIZE;
	}
	public BYTE(byte value)
	{
		mValue = value;
		mType = typeof(byte);
		mSize = TYPE_SIZE;
	}
	public override void zero()
	{
		mValue = 0;
	}
	public void set(byte value) { mValue = value; }
	public override bool readFromBuffer(byte[] buffer, ref int index)
	{
		bool success;
		mValue = readByte(buffer, ref index, out success);
		return success;
	}
	public override bool writeToBuffer(byte[] buffer, ref int index)
	{
		return writeByte(buffer, ref index, mValue);
	}
}