using System.Collections;
using System.Collections.Generic;

// 自定义的对byte[]的封装,可用于序列化
public class BIT_SBYTES : SerializableBit, IEnumerable<sbyte>
{
	public List<sbyte> mValue = new();  // 值
	public sbyte this[int index]
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
	public void add(sbyte value)
	{
		mValue.Add(value);
	}
	public void addRange(IEnumerable<sbyte> value)
	{
		mValue.AddRange(value);
	}
	public static implicit operator List<sbyte>(BIT_SBYTES value)
	{
		return value.mValue;
	}
	public IEnumerator<sbyte> GetEnumerator() { return mValue.GetEnumerator(); }
	IEnumerator IEnumerable.GetEnumerator() { return mValue.GetEnumerator(); }
}