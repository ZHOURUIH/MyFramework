
// 自定义的对long的封装,提供类似于long指针的功能,可用于序列化
public class LONG : Serializable
{
	public long mValue;			// 值
	public override void resetProperty() 
	{
		base.resetProperty();
		mValue = 0; 
	}
	public void set(long value) { mValue = value; }
	public override bool read(SerializerRead reader)
	{
		return reader.read(out mValue);
	}
	public override void write(SerializerWrite writer)
	{
		writer.write(mValue);
	}
	public static implicit operator long(LONG value)
	{
		return value.mValue;
	}
}