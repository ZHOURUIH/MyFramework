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
	// 获取用于更新的列表,
	public List<T> StartForeach() 
	{
		// 由于需要考虑嵌套,所以只能创建一个新的列表,复制当前主列表的数据
		List<T> tempList = newList(out tempList);
		tempList.AddRange(mMainList);
		return tempList;
	}
	// 获取UpdateList,遍历结束后,需要手动调用EndUpdate,对临时列表进行回收
	public void EndForeach(List<T> list)
	{
		destroyList(list);
	}
	// 获取主列表,存储着当前实时的数据列表,所有的删除和新增都会立即更新此列表
	// 不能用主列表进行遍历,要遍历应该使用GetUpdateList
	public List<T> GetMainList() { return mMainList; }
	public bool Contains(T value) { return mMainList.Contains(value); }
	public int Count() { return mMainList.Count; }
	public void Add(T value)
	{
		mMainList.Add(value);
	}
	public void Remove(T value)
	{
		mMainList.Remove(value);
	}
	// 清空所有数据,不能正在遍历时调用
	public void Clear()
	{
		mMainList.Clear();
	}
}