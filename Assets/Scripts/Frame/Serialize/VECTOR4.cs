using System;
using UnityEngine;

// 自定义的对ushort的封装,提供类似于ushort指针的功能,可用于序列化
public class VECTOR4 : SerializableBit
{
	public Vector4 mValue;		// 值
	public override void resetProperty() 
	{
		base.resetProperty();
		mValue = Vector4.zero; 
	}
	public void set(Vector4 value) { mValue = value; }
	public override bool read(SerializerBitRead reader)
	{
		return reader.read(out mValue);
	}
	public override void write(SerializerBitWrite writer)
	{
		writer.write(mValue);
	}
}