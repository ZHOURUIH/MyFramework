using System.Collections.Generic;

// 自定义的对ulong[]的封装,可用于序列化
public class BIT_ULONGS : SerializableBit
{
	public List<ulong> mValue = new();  // 值
	public ulong this[int index]
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
		return reader.readList(mValue);
	}
	public override void write(SerializerBitWrite writer, bool needWriteSign)
	{
		writer.writeList(mValue);
	}
	public void add(ulong value)
	{
		mValue.Add(value);
	}
	public void addRange(IEnumerable<ulong> value)
	{
		mValue.AddRange(value);
	}
	public static implicit operator List<ulong>(BIT_ULONGS value)
	{
		return value.mValue;
	}
	public List<ulong>.Enumerator GetEnumerator() { return mValue.GetEnumerator(); }
}