using System.Collections;
using System.Collections.Generic;

// 自定义的对byte[]的封装,可用于序列化
public class BYTES : Serializable, IEnumerable<byte>
{
	public List<byte> mValue = new();		// 值
	public byte this[int index]
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
	public void add(byte value)
	{
		mValue.Add(value);
	}
	public void addRange(IEnumerable<byte> value)
	{
		mValue.AddRange(value);
	}
	public static implicit operator List<byte>(BYTES value)
	{
		return value.mValue;
	}
	public IEnumerator<byte> GetEnumerator() { return mValue.GetEnumerator(); }
	IEnumerator IEnumerable.GetEnumerator() { return mValue.GetEnumerator(); }
}