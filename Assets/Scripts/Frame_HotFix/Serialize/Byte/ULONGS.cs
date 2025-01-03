using System.Collections;
using System.Collections.Generic;

// 自定义的对ulong[]的封装,可用于序列化
public class ULONGS : Serializable, IEnumerable<ulong>
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
	public override bool read(SerializerRead reader)
	{
		return reader.readList(mValue);
	}
	public override void write(SerializerWrite writer)
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
	public static implicit operator List<ulong>(ULONGS value)
	{
		return value.mValue;
	}
	public IEnumerator<ulong> GetEnumerator() { return mValue.GetEnumerator(); }
	IEnumerator IEnumerable.GetEnumerator() { return mValue.GetEnumerator(); }
}