using System.Collections;
using System.Collections.Generic;

// 自定义的对long[]的封装,可用于序列化
public class LONGS : Serializable, IEnumerable<long>
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
	public override bool read(SerializerRead reader)
	{
		return reader.readList(mValue);
	}
	public override void write(SerializerWrite writer)
	{
		writer.writeList(mValue);
	}
	public void add(long value)
	{
		mValue.Add(value);
	}
	public void addRange(IEnumerable<long> value)
	{
		mValue.AddRange(value);
	}
	public static implicit operator List<long>(LONGS value)
	{
		return value.mValue;
	}
	public IEnumerator<long> GetEnumerator() { return mValue.GetEnumerator(); }
	IEnumerator IEnumerable.GetEnumerator() { return mValue.GetEnumerator(); }
}