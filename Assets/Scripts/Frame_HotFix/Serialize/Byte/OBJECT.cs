using System;

// 基础数据类型再包装基类
public abstract class OBJECT : ClassObject
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
	public abstract bool readFromBuffer(SerializerRead reader);
	public abstract void writeToBuffer(SerializerWrite writer);
}