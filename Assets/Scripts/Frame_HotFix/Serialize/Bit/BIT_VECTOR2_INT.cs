using UnityEngine;

// 自定义的对Vector2Int的封装,提供类似于Vector2Int指针的功能,可用于序列化
public class BIT_VECTOR2_INT : SerializableBit
{
	public Vector2IntMy mValue;      // 值
	public override void resetProperty()
	{
		base.resetProperty();
		mValue.x = 0;
		mValue.y = 0;
	}
	public void set(Vector2Int value) { mValue.x = value.x; mValue.y = value.y; }
	public Vector2Int get() { return new(mValue.x, mValue.y); }
	public override bool read(SerializerBitRead reader)
	{
		return reader.read(out mValue);
	}
	public override void write(SerializerBitWrite writer)
	{
		writer.write(mValue);
	}
	public static implicit operator Vector2Int(BIT_VECTOR2_INT value)
	{
		return value.mValue.toVec2Int();
	}
	public static implicit operator Vector2(BIT_VECTOR2_INT value)
	{
		return value.mValue.toVec2();
	}
	public int x { get { return mValue.x; } }
	public int y { get { return mValue.y; } }
}