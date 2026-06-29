
// 自定义的对uint的封装,提供类似于uint指针的功能,可用于序列化
public class BIT_UINT : SerializableBit
{
	public uint mValue;		// 值
	public override void resetProperty() 
	{
		base.resetProperty();
		mValue = 0; 
	}
	public void set(uint value) { mValue = value; }
	public override bool read(SerializerBitRead reader, bool needReadSign)
	{
		return reader.read(out mValue);
	}
	public override void write(SerializerBitWrite writer, bool needWriteSign)
	{
		writer.write(mValue);
	}
	public static implicit operator uint(BIT_UINT value)
	{
		return value.mValue;
	}
}