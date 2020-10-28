using System;
using System.Collections;
using System.Collections.Generic;

// 基础数据类型再包装基类
public abstract class OBJECT : FrameBase
{
	public Type mType;
	public int mSize;
	public abstract void zero();
	public abstract bool readFromBuffer(byte[] buffer, ref int index);
	public abstract bool writeToBuffer(byte[] buffer, ref int index);
	public virtual void setVariableLength(bool variable) {}
	public virtual bool getVariableLength() { return false; }
	public virtual int getSize() { return mSize; }
}