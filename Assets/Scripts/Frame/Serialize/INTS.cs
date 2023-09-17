using System;
using System.Collections.Generic;

// 自定义的对int[]的封装,可用于序列化
public class INTS : SerializableBit
{
	public List<int> mValue = new List<int>();			// 值
	public int this[int index]
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
	public void Add(int value)
	{
		mValue.Add(value);
	}
}