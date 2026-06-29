
// 自定义的对Vector2Int的封装,提供类似于Vector2Int指针的功能,可用于序列化
public class VECTOR2_USHORT : Serializable
{
	public Vector2UShort mValue;      // 值
	public override void resetProperty()
	{
		base.resetProperty();
		mValue.x = 0;
		mValue.y = 0;
	}
	public void set(Vector2UShort value) { mValue = value; }
	public override bool read(SerializerRead reader)
	{
		return reader.read(out mValue);
	}
	public override void write(SerializerWrite writer)
	{
		writer.write(mValue);
	}
	public static implicit operator Vector2UShort(VECTOR2_USHORT value)
	{
		return value.mValue;
	}
	public ushort x { get { return mValue.x; } }
	public ushort y { get { return mValue.y; } }
}