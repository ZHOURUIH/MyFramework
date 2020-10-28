using System;
using System.Collections;
using System.Collections.Generic;

public class FLOAT : OBJECT
{
	protected const int TYPE_SIZE = sizeof(float);
	public float mValue;
	public FLOAT()
	{
		mType = Typeof<float>();
		mSize = TYPE_SIZE;
	}
	public FLOAT(float value)
	{
		mValue = value;
		mType = Typeof<float>();
		mSize = TYPE_SIZE;
	}
	public override void zero() { mValue = 0.0f; }
	public void set(float value) { mValue = value; }
	public override bool readFromBuffer(byte[] buffer, ref int index)
	{
		bool success;
		mValue = readFloat(buffer, ref index, out success);
		return success;
	}
	public override bool writeToBuffer(byte[] buffer, ref int index)
	{
		return writeFloat(buffer, ref index, mValue);
	}
}