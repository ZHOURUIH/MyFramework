using System;

// 自定义的对ushort的封装,提供类似于ushort指针的功能,可用于序列化
public class USHORT : SerializableBit
{
	public ushort mValue;		// 值
	public override void resetProperty() 
	{
		base.resetProperty();
		mValue = 0; 
	}
	public void set(ushort value) { mValue = value; }
	public override bool read(SerializerBitRead reader)
	{
		return reader.read(out mValue);
	}
	public override void write(SerializerBitWrite writer)
	{
		writer.write(mValue);
	}
}