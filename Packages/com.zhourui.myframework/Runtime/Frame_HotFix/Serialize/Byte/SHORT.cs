
// 自定义的对short的封装,提供类似于short指针的功能,可用于序列化
public class SHORT : Serializable
{
	public short mValue;		// 值
	public override void resetProperty() 
	{
		base.resetProperty();
		mValue = 0; 
	}
	public void set(short value) { mValue = value; }
	public override bool read(SerializerRead reader)
	{
		return reader.read(out mValue);
	}
	public override void write(SerializerWrite writer)
	{
		writer.write(mValue);
	}
	public static implicit operator short(SHORT value)
	{
		return value.mValue;
	}
}