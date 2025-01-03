
// 自定义的对byte的封装,提供类似于byte指针的功能,可用于序列化
public class BIT_SBYTE : SerializableBit
{
	public sbyte mValue;			// 值
	public override void resetProperty()
	{
		base.resetProperty();
		mValue = 0;
	}
	public void set(sbyte value) { mValue = value; }
	public override bool read(SerializerBitRead reader)
	{
		return reader.read(out mValue);
	}
	public override void write(SerializerBitWrite writer)
	{
		writer.write(mValue);
	}
	public static implicit operator sbyte(BIT_SBYTE value)
	{
		return value.mValue;
	}
}