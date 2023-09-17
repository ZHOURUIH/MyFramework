using System;

// 自定义的对short的封装,提供类似于short指针的功能,可用于序列化
public class SHORT : SerializableBit
{
	public short mValue;		// 值
	public override void resetProperty() 
	{
		base.resetProperty();
		mValue = 0; 
	}
	public void set(short value) { mValue = value; }
	public override bool read(SerializerBitRead reader)
	{
		return reader.read(out mValue);
	}
	public override void write(SerializerBitWrite writer)
	{
		writer.write(mValue);
	}
}