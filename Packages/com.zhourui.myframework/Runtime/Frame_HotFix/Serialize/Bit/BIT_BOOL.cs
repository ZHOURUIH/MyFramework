
// 自定义的对bool的封装,提供类似于bool指针的功能,可用于序列化
public class BIT_BOOL : SerializableBit
{
	public bool mValue;		// 值
	public override void resetProperty() 
	{
		base.resetProperty();
		mValue = false; 
	}
	public void set(bool value) { mValue = value; }
	public override bool read(SerializerBitRead reader, bool needReadSign)
	{
		return reader.read(out mValue);
	}
	public override void write(SerializerBitWrite writer, bool needWriteSign)
	{
		writer.write(mValue);
	}
	public static implicit operator bool(BIT_BOOL value)
	{
		return value.mValue;
	}
}