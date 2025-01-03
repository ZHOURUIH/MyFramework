
// 自定义的对float的封装,提供类似于float指针的功能,可用于序列化
public class FLOAT : Serializable
{
	public float mValue;			// 值
	public override void resetProperty() 
	{
		base.resetProperty();
		mValue = 0.0f; 
	}
	public void set(float value) { mValue = value; }
	public override bool read(SerializerRead reader)
	{
		return reader.read(out mValue);
	}
	public override void write(SerializerWrite writer)
	{
		writer.write(mValue);
	}
	public static implicit operator float(FLOAT value)
	{
		return value.mValue;
	}
}