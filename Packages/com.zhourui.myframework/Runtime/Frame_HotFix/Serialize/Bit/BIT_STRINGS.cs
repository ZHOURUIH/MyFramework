using System.Collections.Generic;

// 自定义的对byte[]的封装,可用于序列化
public class BIT_STRINGS : SerializableBit
{
	public List<string> mValue = new();    // 值
	public string this[int index]
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
	public void add(string value)
	{
		mValue.Add(value);
	}
	public void addRange(IEnumerable<string> value)
	{
		mValue.AddRange(value);
	}
	public static implicit operator List<string>(BIT_STRINGS value)
	{
		return value.mValue;
	}
	public List<string>.Enumerator GetEnumerator() { return mValue.GetEnumerator(); }
}