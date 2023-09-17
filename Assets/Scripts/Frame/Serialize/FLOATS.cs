using System;
using System.Collections.Generic;

// 自定义的对float[]的封装,可用于序列化
public class FLOATS : SerializableBit
{
	public List<float> mValue = new List<float>();			// 值
	public float this[int index]
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
	public void Add(float value)
	{
		mValue.Add(value);
	}
}