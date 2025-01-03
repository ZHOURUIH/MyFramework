using System;

// 基础数据类型再包装基类
public abstract class BIT_OBJECT : ClassObject
{
	protected Type mType;			// 类型
	public override void resetProperty()
	{
		base.resetProperty();
		// mType不重置
		// mType = null;
		zero();
	}
	public abstract void zero();
	public abstract bool readFromBuffer(SerializerBitRead reader);
	public abstract void writeToBuffer(SerializerBitWrite writer);
}