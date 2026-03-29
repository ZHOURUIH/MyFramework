
// 自定义的对ulong的封装,提供类似于ulong指针的功能,可用于序列化
public class BIT_ULONG : SerializableBit
{
	public ulong mValue;			// 值
	public override void resetProperty() 
	{
		base.resetProperty();
		mValue = 0; 
	}
	public void set(ulong value) { mValue = value; }
	public override bool read(SerializerBitRead reader, bool needReadSign)
	{
		return reader.read(out mValue);
	}
	public override void write(SerializerBitWrite writer, bool needWriteSign)
	{
		writer.write(mValue);
	}
	public static implicit operator ulong(BIT_ULONG value)
	{
		return value.mValue;
	}
}