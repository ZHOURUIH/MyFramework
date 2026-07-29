
// 属性重置接口,对象池中的对象需实现此接口,回收时重置属性
public interface IResetProperty
{
	void resetProperty();
}