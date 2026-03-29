
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
	public override bool read(SerializerBitRead reader, bool needReadSign)
	{
		return reader.read(out mValue, needReadSign);
	}
	public override void write(SerializerBitWrite writer, bool needWriteSign)
	{
		writer.write(mValue, needWriteSign);
	}
	public static implicit operator sbyte(BIT_SBYTE value)
	{
		return value.mValue;
	}
}