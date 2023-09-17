using System;

// 自定义的对float的封装,提供类似于float指针的功能,可用于序列化
public class FLOAT : SerializableBit
{
	public float mValue;			// 值
	public override void resetProperty() 
	{
		base.resetProperty();
		mValue = 0.0f; 
	}
	public void set(float value) { mValue = value; }
	public override bool read(SerializerBitRead reader)
	{
		return reader.read(out mValue);
	}
	public override void write(SerializerBitWrite writer)
	{
		writer.write(mValue);
	}
}