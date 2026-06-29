
// 自定义的对Vector2Int的封装,提供类似于Vector2Int指针的功能,可用于序列化
public class VECTOR2_SHORT : Serializable
{
	public Vector2Short mValue;      // 值
	public override void resetProperty()
	{
		base.resetProperty();
		mValue.x = 0;
		mValue.y = 0;
	}
	public void set(Vector2Short value) { mValue = value; }
	public override bool read(SerializerRead reader)
	{
		return reader.read(out mValue);
	}
	public override void write(SerializerWrite writer)
	{
		writer.write(mValue);
	}
	public static implicit operator Vector2Short(VECTOR2_SHORT value)
	{
		return value.mValue;
	}
	public short x { get { return mValue.x; } }
	public short y { get { return mValue.y; } }
}