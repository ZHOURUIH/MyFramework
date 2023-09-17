using System;
using UnityEngine;

// 自定义的对Vector2Int的封装,提供类似于Vector2Int指针的功能,可用于序列化
public class VECTOR2INT : SerializableBit
{
	public Vector2Int mValue;      // 值
	public override void resetProperty()
	{
		base.resetProperty();
		mValue = Vector2Int.zero;
	}
	public void set(Vector2Int value) { mValue = value; }
	public override bool read(SerializerBitRead reader)
	{
		return reader.read(out mValue);
	}
	public override void write(SerializerBitWrite writer)
	{
		writer.write(mValue);
	}
}