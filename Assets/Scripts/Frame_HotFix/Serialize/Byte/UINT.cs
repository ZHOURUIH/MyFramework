
// 自定义的对uint的封装,提供类似于uint指针的功能,可用于序列化
public class UINT : Serializable
{
	public uint mValue;		// 值
	public override void resetProperty() 
	{
		base.resetProperty();
		mValue = 0; 
	}
	public void set(uint value) { mValue = value; }
	public override bool read(SerializerRead reader)
	{
		return reader.read(out mValue);
	}
	public override void write(SerializerWrite writer)
	{
		writer.write(mValue);
	}
	public static implicit operator uint(UINT value)
	{
		return value.mValue;
	}
}