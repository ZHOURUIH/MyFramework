
// 自定义的对int的封装,提供类似于int指针的功能,可用于序列化
public class BIT_INT : SerializableBit
{
	public int mValue;			// 值
	public override void resetProperty() 
	{
		base.resetProperty();
		mValue = 0; 
	}
	public void set(int value) { mValue = value; }
	public override bool read(SerializerBitRead reader, bool needReadSign)
	{
		return reader.read(out mValue, needReadSign);
	}
	public override void write(SerializerBitWrite writer, bool needWriteSign)
	{
		writer.write(mValue, needWriteSign);
	}
	public static implicit operator int(BIT_INT value)
	{
		return value.mValue;
	}
}