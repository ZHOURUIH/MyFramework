using System;
using System.Collections.Generic;

// 自定义的对byte[]的封装,可用于序列化
public class BYTES : SerializableBit
{
	public List<byte> mValue = new List<byte>();
	public byte this[int index]
	{
		get { return mValue[index]; }
		set { mValue[index] = value; }
	}
	public int Count { get { return mValue.Count; } }
	public override void resetProperty() 
	{
		base.resetProperty();
		mValue.Clear(); 
	}
	public override bool read(SerializerBitRead reader)
	{
		return reader.readList(mValue);
	}
	public override void write(SerializerBitWrite writer)
	{
		writer.writeList(mValue);
	}
	public void Add(byte value)
	{
		mValue.Add(value);
	}
}