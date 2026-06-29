using UnityEngine;

// 自定义的对Vector2Int的封装,提供类似于Vector2Int指针的功能,可用于序列化
public class VECTOR2_INT : Serializable
{
	public Vector2Int mValue;      // 值
	public override void resetProperty()
	{
		base.resetProperty();
		mValue = Vector2Int.zero;
	}
	public void set(Vector2Int value) { mValue = value; }
	public override bool read(SerializerRead reader)
	{
		return reader.read(out mValue);
	}
	public override void write(SerializerWrite writer)
	{
		writer.write(mValue);
	}
	public static implicit operator Vector2Int(VECTOR2_INT value)
	{
		return value.mValue;
	}
	public static implicit operator Vector2(VECTOR2_INT value)
	{
		return value.mValue;
	}
	public int x { get { return mValue.x; } }
	public int y { get { return mValue.y; } }
}