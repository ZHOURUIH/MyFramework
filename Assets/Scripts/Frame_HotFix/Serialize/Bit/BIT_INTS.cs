using System.Collections.Generic;

// 自定义的对int[]的封装,可用于序列化
public class BIT_INTS : SerializableBit
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
	public override bool read(SerializerBitRead reader, bool needReadSign)
	{
		return reader.readList(mValue, needReadSign);
	}
	public override void write(SerializerBitWrite writer, bool needWriteSign)
	{
		writer.writeList(mValue, needWriteSign);
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
	public List<int>.Enumerator GetEnumerator() { return mValue.GetEnumerator(); }
}