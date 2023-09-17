using System;
using UnityEngine;

// 自定义的对Vector2的封装,提供类似于Vector2指针的功能,可用于序列化
public class VECTOR2 : SerializableBit
{
	public Vector2 mValue;		// 值
	public override void resetProperty() 
	{
		base.resetProperty();
		mValue = Vector2.zero; 
	}
	public void set(Vector2 value) { mValue = value; }
	public override bool read(SerializerBitRead reader)
	{
		return reader.read(out mValue);
	}
	public override void write(SerializerBitWrite writer)
	{
		writer.write(mValue);
	}
}