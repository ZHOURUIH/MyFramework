﻿using UnityEngine;

// 自定义的对Vector3的封装,提供类似于Vector3指针的功能,可用于序列化
public class VECTOR3 : Serializable
{
	public Vector3 mValue;      // 值
	public override void resetProperty()
	{
		base.resetProperty();
		mValue = Vector3.zero;
	}
	public void set(Vector3 value) { mValue = value; }
	public override bool read(SerializerRead reader)
	{
		return reader.read(out mValue);
	}
	public override void write(SerializerWrite writer)
	{
		writer.write(mValue);
	}
	public static implicit operator Vector3(VECTOR3 value)
	{
		return value.mValue;
	}
	public float x { get { return mValue.x; } }
	public float y { get { return mValue.y; } }
	public float z { get { return mValue.z; } }
}