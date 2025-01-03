using System.Collections;
using System.Collections.Generic;

// 自定义的对uint[]的封装,可用于序列化
public class UINTS : Serializable, IEnumerable<uint>
{
	public List<uint> mValue = new();    // 值
	public uint this[int index]
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
	public void add(uint value)
	{
		mValue.Add(value);
	}
	public void addRange(IEnumerable<uint> value)
	{
		mValue.AddRange(value);
	}
	public static implicit operator List<uint>(UINTS value)
	{
		return value.mValue;
	}
	public IEnumerator<uint> GetEnumerator() { return mValue.GetEnumerator(); }
	IEnumerator IEnumerable.GetEnumerator() { return mValue.GetEnumerator(); }
}