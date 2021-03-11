using System;
using System.Collections.Generic;

public class SHORT : OBJECT
{
	protected const int TYPE_SIZE = sizeof(short);
	public short mValue;
	public SHORT()
	{
		mType = Typeof<short>();
		mSize = TYPE_SIZE;
	}
	public SHORT(short value)
	{
		mValue = value;
		mType = Typeof<short>();
		mSize = TYPE_SIZE;
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