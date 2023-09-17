using System;

// 自定义的对byte的封装,提供类似于byte指针的功能,可用于序列化
public class STRING : SerializableBit
{
	public string mValue;			// 值
	public override void resetProperty()
	{
		base.resetProperty();
		mValue = string.Empty;
	}
	public void set(string value) { mValue = value; }
	public override bool read(SerializerBitRead reader)
	{
		return reader.readString(out mValue);
	}
	public override void write(SerializerBitWrite writer)
	{
		writer.writeString(mValue);
	}
}