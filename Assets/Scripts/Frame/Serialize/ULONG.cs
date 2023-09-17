using System;

// 自定义的对ulong的封装,提供类似于ulong指针的功能,可用于序列化
public class ULONG : SerializableBit
{
	public ulong mValue;			// 值
	public override void resetProperty() 
	{
		base.resetProperty();
		mValue = 0; 
	}
	public void set(ulong value) { mValue = value; }
	public override bool read(SerializerBitRead reader)
	{
		return reader.read(out mValue);
	}
	public override void write(SerializerBitWrite writer)
	{
		writer.write(mValue);
	}
}