using System.Collections.Generic;

// 非线程安全
// 可深度嵌套安全遍历的列表,支持在遍历过程中嵌套遍历和对列表进行修改
// 由于可嵌套,即在遍历中再次开始遍历,所以效率较低,使用方法上比普通列表稍复杂
public class SafeDeepList<T> : FrameBase
{
	protected List<T> mMainList;		// 用于存储实时数据的列表
	public SafeDeepList()
	{
		mMainList = new List<T>();
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mMainList.Clear();
	}
	// 获取用于更新的列表
	public List<T> startForeach() 
	{
		// 由于需要考虑嵌套,所以只能创建一个新的列表,复制当前主列表的数据
		LIST(out List<T>  tempList);
		tempList.AddRange(mMainList);
		return tempList;
	}
	// 遍历结束后,需要手动调用endForeach,对临时列表进行回收
	public void endForeach(List<T> list)
	{
		UN_LIST(list);
	}
	// 获取主列表,存储着当前实时的数据列表,所有的删除和新增都会立即更新此列表
	// 不能用主列表进行遍历,要遍历应该使用startForeach
	public List<T> getMainList() { return mMainList; }
	public bool contains(T value) { return mMainList.Contains(value); }
	public int count() { return mMainList.Count; }
	public void add(T value)
	{
		mMainList.Add(value);
	}
	public void remove(T value)
	{
		mMainList.Remove(value);
	}
	public void clear()
	{
		mMainList.Clear();
	}
}