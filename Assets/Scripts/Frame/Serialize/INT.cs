using System;
using System.Collections;
using System.Collections.Generic;

public class INT : OBJECT
{
	protected const int TYPE_SIZE = sizeof(int);
	public int mValue;
	public INT()
	{
		mType = typeof(int);
		mSize = TYPE_SIZE;
	}
	public INT(int value)
	{
		mValue = value;
		mType = typeof(int);
		mSize = TYPE_SIZE;
	}
	public override void zero() { mValue = 0; }
	public void set(int value) { mValue = value; }
	public override bool readFromBuffer(byte[] buffer, ref int index)
	{
		bool success;
		mValue = readInt(buffer, ref index, out success);
		return success;
	}
	public override bool writeToBuffer(byte[] buffer, ref int index)
	{
		return writeInt(buffer, ref index, mValue);
	}
}