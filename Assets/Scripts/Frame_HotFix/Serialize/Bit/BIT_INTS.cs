using System.Collections;
using System.Collections.Generic;

// 自定义的对int[]的封装,可用于序列化
public class BIT_INTS : SerializableBit, IEnumerable<int>
{
	public List<int> mValue = new();			// 值
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
	public void add(int value)
	{
		mValue.Add(value);
	}
	public void addRange(IEnumerable<int> value)
	{
		mValue.AddRange(value);
	}
	public static implicit operator List<int>(BIT_INTS value)
	{
		return value.mValue;
	}
	public IEnumerator<int> GetEnumerator() { return mValue.GetEnumerator(); }
	IEnumerator IEnumerable.GetEnumerator() { return mValue.GetEnumerator(); }
}