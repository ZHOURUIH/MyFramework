using System.Collections;
using System.Collections.Generic;

// 自定义的对short[]的封装,可用于序列化
public class BIT_SHORTS : SerializableBit, IEnumerable<short>
{
	public List<short> mValue = new();  // 值
	public short this[int index]
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
	public void add(short value)
	{
		mValue.Add(value);
	}
	public void addRange(IEnumerable<short> value)
	{
		mValue.AddRange(value);
	}
	public static implicit operator List<short>(BIT_SHORTS value)
	{
		return value.mValue;
	}
	public IEnumerator<short> GetEnumerator() { return mValue.GetEnumerator(); }
	IEnumerator IEnumerable.GetEnumerator() { return mValue.GetEnumerator(); }
}