using UnityEngine;

// 自定义的对ushort的封装,提供类似于ushort指针的功能,可用于序列化
public class BIT_VECTOR4 : SerializableBit
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
	public static implicit operator Vector4(BIT_VECTOR4 value)
	{
		return value.mValue;
	}
	public float x { get { return mValue.x; } }
	public float y { get { return mValue.y; } }
	public float z { get { return mValue.z; } }
	public float w { get { return mValue.w; } }
}