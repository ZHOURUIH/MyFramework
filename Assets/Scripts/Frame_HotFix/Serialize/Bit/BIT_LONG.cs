
// 自定义的对long的封装,提供类似于long指针的功能,可用于序列化
public class BIT_LONG : SerializableBit
{
	public long mValue;			// 值
	public override void resetProperty() 
	{
		base.resetProperty();
		mValue = 0; 
	}
	public void set(long value) { mValue = value; }
	public override bool read(SerializerBitRead reader)
	{
		return reader.read(out mValue);
	}
	public override void write(SerializerBitWrite writer)
	{
		writer.write(mValue);
	}
	public static implicit operator long(BIT_LONG value)
	{
		return value.mValue;
	}
}