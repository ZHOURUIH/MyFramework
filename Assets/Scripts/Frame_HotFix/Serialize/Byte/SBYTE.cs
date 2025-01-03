
// 自定义的对byte的封装,提供类似于byte指针的功能,可用于序列化
public class SBYTE : Serializable
{
	public sbyte mValue;			// 值
	public override void resetProperty()
	{
		base.resetProperty();
		mValue = 0;
	}
	public void set(sbyte value) { mValue = value; }
	public override bool read(SerializerRead reader)
	{
		return reader.read(out mValue);
	}
	public override void write(SerializerWrite writer)
	{
		writer.write(mValue);
	}
	public static implicit operator sbyte(SBYTE value)
	{
		return value.mValue;
	}
}