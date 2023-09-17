using System;

public abstract class SerializableBit : ClassObject
{
	public bool mValid = true;      // 此字段是否有效
	public bool mOptional = false;  // 此字段是否为可选的
	public abstract bool read(SerializerBitRead reader);
	public abstract void write(SerializerBitWrite writer);
	public override void resetProperty()
	{
		base.resetProperty();
		mValid = true;
		// 构造中赋值的,不需要重置
		//mOptional = false;
	}
}