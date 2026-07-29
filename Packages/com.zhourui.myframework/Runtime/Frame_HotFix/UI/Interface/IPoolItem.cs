
// 池对象接口,可放入对象池的项需实现此接口
public interface IPoolItem<T>
{
	void setData(T data);
}