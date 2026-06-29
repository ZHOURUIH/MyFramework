
// 自定义的对int的封装,提供类似于int指针的功能,可用于序列化
public class INT : Serializable
{
	public int mValue;			// 值
	public override void resetProperty() 
	{
		base.resetProperty();
		mValue = 0; 
	}
	public void set(int value) { mValue = value; }
	public override bool read(SerializerRead reader)
	{
		return reader.read(out mValue);
	}
	public override void write(SerializerWrite writer)
	{
		writer.write(mValue);
	}
	public static implicit operator int(INT value)
	{
		return value.mValue;
	}
}