
// 自定义的对ushort的封装,提供类似于ushort指针的功能,可用于序列化
public class BIT_USHORT : SerializableBit
{
	public ushort mValue;		// 值
	public override void resetProperty() 
	{
		base.resetProperty();
		mValue = 0; 
	}
	public void set(ushort value) { mValue = value; }
	public override bool read(SerializerBitRead reader, bool needReadSign)
	{
		return reader.read(out mValue);
	}
	public override void write(SerializerBitWrite writer, bool needWriteSign)
	{
		writer.write(mValue);
	}
	public static implicit operator ushort(BIT_USHORT value)
	{
		return value.mValue;
	}
}