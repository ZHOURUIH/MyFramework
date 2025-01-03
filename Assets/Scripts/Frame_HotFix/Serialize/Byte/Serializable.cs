
// 序列化基类
public abstract class Serializable : ClassObject
{
	public bool mValid;         // 此字段是否有效
	public bool mOptional;      // 此字段是否为可选的
	public Serializable()
	{
		mValid = true;
	}
	public abstract bool read(SerializerRead reader);
	public abstract void write(SerializerWrite writer);
	public override void resetProperty()
	{
		base.resetProperty();
		mValid = true;
		// 构造中赋值的,不需要重置
		// mOptional = false;
	}
}