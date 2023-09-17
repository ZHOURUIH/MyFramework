using System;
using System.Collections.Generic;

// 自定义的对uint[]的封装,可用于序列化
public class UINTS : SerializableBit
{
	public List<uint> mValue = new List<uint>();
	public uint this[int index]
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
	public void Add(uint value)
	{
		mValue.Add(value);
	}
}