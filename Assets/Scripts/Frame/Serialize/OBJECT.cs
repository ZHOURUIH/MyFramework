using System;
using System.Collections.Generic;

// 基础数据类型再包装基类
public abstract class OBJECT : FrameBase
{
	public Type mType;
	public int mSize;
	public bool mVariableLength;                // true表示数据长度为变长,一般为数组,false表示数据长度为定长
	public abstract void zero();
	public abstract bool readFromBuffer(byte[] buffer, ref int index);
	public abstract bool writeToBuffer(byte[] buffer, ref int index);
	public bool getVariableLength() { return mVariableLength; }
	public virtual int getSize() { return mSize; }
	public virtual void setIntReplaceULLong(bool replace) { }
}