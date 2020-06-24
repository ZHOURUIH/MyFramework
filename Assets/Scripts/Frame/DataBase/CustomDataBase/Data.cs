using System;
using System.Collections.Generic;

public class Data
{
	protected Type mType;
	public void setType(Type type) { mType = type; }
	public Type getType() { return mType; }
	public virtual void read(byte[] data) { }
	public virtual int getDataSize() { return 0; }
}