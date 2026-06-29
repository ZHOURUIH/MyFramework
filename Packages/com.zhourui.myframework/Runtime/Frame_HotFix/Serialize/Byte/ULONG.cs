
// 自定义的对ulong的封装,提供类似于ulong指针的功能,可用于序列化
public class ULONG : Serializable
{
	public ulong mValue;			// 值
	public override void resetProperty() 
	{
		base.resetProperty();
		mValue = 0; 
	}
	public void set(ulong value) { mValue = value; }
	public override bool read(SerializerRead reader)
	{
		return reader.read(out mValue);
	}
	public override void write(SerializerWrite writer)
	{
		writer.write(mValue);
	}
	public static implicit operator ulong(ULONG value)
	{
		return value.mValue;
	}
}