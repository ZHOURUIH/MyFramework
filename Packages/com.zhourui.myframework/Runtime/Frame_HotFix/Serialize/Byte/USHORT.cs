
// 自定义的对ushort的封装,提供类似于ushort指针的功能,可用于序列化
public class USHORT : Serializable
{
	public ushort mValue;		// 值
	public override void resetProperty() 
	{
		base.resetProperty();
		mValue = 0; 
	}
	public void set(ushort value) { mValue = value; }
	public override bool read(SerializerRead reader)
	{
		return reader.read(out mValue);
	}
	public override void write(SerializerWrite writer)
	{
		writer.write(mValue);
	}
	public static implicit operator ushort(USHORT value)
	{
		return value.mValue;
	}
}