using System.Collections;
using System.Collections.Generic;

// 自定义的对float[]的封装,可用于序列化
public class FLOATS : Serializable, IEnumerable<float>
{
	public List<float> mValue = new();			// 值
	public float this[int index]
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
	public void add(float value)
	{
		mValue.Add(value);
	}
	public void addRange(IEnumerable<float> value)
	{
		mValue.AddRange(value);
	}
	public static implicit operator List<float>(FLOATS value)
	{
		return value.mValue;
	}
	public IEnumerator<float> GetEnumerator() { return mValue.GetEnumerator(); }
	IEnumerator IEnumerable.GetEnumerator() { return mValue.GetEnumerator(); }
}