using System;
using System.Collections;
using System.Collections.Generic;

public abstract class OBJECTS : OBJECT
{
	public ushort mRealSize;	// 表示数组中有效的字节个数
	public bool mVariableLength;// true表示数组长度为变长,false表示数组长度为定长
	public int mElementCount;   // 表示数组中有效的元素个数
	public abstract void setRealSize(ushort realSize);
	public abstract void setElementCount(int elementCount);
	public override void setVariableLength(bool variable) { mVariableLength = variable; }
	public override bool getVariableLength() { return mVariableLength; }
	public override int getSize()
	{
		if(!mVariableLength)
		{
			return base.getSize();
		}
		return mRealSize;
	}
}