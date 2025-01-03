using UnityEngine;

// 自定义的对Vector2的封装,提供类似于Vector2指针的功能,可用于序列化
public class VECTOR2 : Serializable
{
	public Vector2 mValue;		// 值
	public override void resetProperty() 
	{
		base.resetProperty();
		mValue = Vector2.zero; 
	}
	public void set(Vector2 value) { mValue = value; }
	public override bool read(SerializerRead reader)
	{
		return reader.read(out mValue);
	}
	public override void write(SerializerWrite writer)
	{
		writer.write(mValue);
	}
	public static implicit operator Vector2(VECTOR2 value)
	{
		return value.mValue;
	}
	public static implicit operator Vector3(VECTOR2 value)
	{
		return value.mValue;
	}
	public float x { get { return mValue.x; } }
	public float y { get { return mValue.y; } }
}