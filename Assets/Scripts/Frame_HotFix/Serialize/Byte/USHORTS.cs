using System.Collections;
using System.Collections.Generic;

// 自定义的对ushort[]的封装,可用于序列化
public class USHORTS : Serializable, IEnumerable<ushort>
{
	public List<ushort> mValue = new();    // 值
	public ushort this[int index]
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
	public void add(ushort value)
	{
		mValue.Add(value);
	}
	public void addRange(IEnumerable<ushort> value)
	{
		mValue.AddRange(value);
	}
	public static implicit operator List<ushort>(USHORTS value)
	{
		return value.mValue;
	}
	public IEnumerator<ushort> GetEnumerator() { return mValue.GetEnumerator(); }
	IEnumerator IEnumerable.GetEnumerator() { return mValue.GetEnumerator(); }
}