using System;

// 自定义的对bool的封装,提供类似于bool指针的功能,可用于序列化
public class BOOL : SerializableBit
{
	public bool mValue;		// 值
	public override void resetProperty() 
	{
		base.resetProperty();
		mValue = false; 
	}
	public void set(bool value) { mValue = value; }
	public override bool read(SerializerBitRead reader)
	{
		return reader.read(out mValue);
	}
	public override void write(SerializerBitWrite writer)
	{
		writer.write(mValue);
	}
}