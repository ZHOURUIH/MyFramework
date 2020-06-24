using System;
using System.Collections;
using System.Collections.Generic;

public class USHORT : OBJECT
{
	protected const int TYPE_SIZE = sizeof(ushort);
	public ushort mValue;
	public USHORT()
	{
		mType = typeof(ushort);
		mSize = TYPE_SIZE;
	}
	public USHORT(ushort value)
	{
		mValue = value;
		mType = typeof(ushort);
		mSize = TYPE_SIZE;
	}
	public override void zero() { mValue = 0; }
	public void set(ushort value) { mValue = value; }
	public override bool readFromBuffer(byte[] buffer, ref int index)
	{
		bool success;
		mValue = readUShort(buffer, ref index, out success);
		return success;
	}
	public override bool writeToBuffer(byte[] buffer, ref int index)
	{
		return writeUShort(buffer, ref index, mValue);
	}
}