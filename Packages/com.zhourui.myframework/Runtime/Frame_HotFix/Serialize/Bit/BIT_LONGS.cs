using System.Collections.Generic;

// 自定义的对long[]的封装,可用于序列化
public class BIT_LONGS : SerializableBit
{
	public List<long> mValue = new();    // 值
	public long this[int index]
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
	public override bool read(SerializerBitRead reader, bool needReadSign)
	{
		return reader.readList(mValue, needReadSign);
	}
	public override void write(SerializerBitWrite writer, bool needWriteSign)
	{
		writer.writeList(mValue, needWriteSign);
	}
	public void add(long value)
	{
		mValue.Add(value);
	}
	public void addRange(IEnumerable<long> value)
	{
		mValue.AddRange(value);
	}
	public static implicit operator List<long>(BIT_LONGS value)
	{
		return value.mValue;
	}
	public List<long>.Enumerator GetEnumerator() { return mValue.GetEnumerator(); }
}