using System.Collections.Generic;

// 非线程安全
// 可深度嵌套安全遍历的列表,支持在遍历过程中嵌套遍历和对列表进行修改
// 由于可嵌套,即在遍历中再次开始遍历,所以效率较低,使用方法上比普通列表稍复杂
public class SafeDeepList<T>
{
	protected HashSet<List<T>> mTempInuseList;  // 用于缓存正在使用的临时列表,由于需要考虑到效率,所以不使用对象池
	protected List<List<T>> mTempUnuseList;     // 用于缓存未使用的临时列表
	protected List<T> mMainList;				// 用于存储实时数据的列表
	public SafeDeepList()
	{
		mTempInuseList = new HashSet<List<T>>();
		mTempUnuseList = new List<List<T>>();
		mMainList = new List<T>();
	}
	// 获取用于更新的列表
	public List<T> startForeach() 
	{
		// 由于需要考虑嵌套,所以只能创建一个新的列表,复制当前主列表的数据
		List<T> tempList;
		if (mTempUnuseList.Count > 0)
		{
			tempList = mTempUnuseList[mTempUnuseList.Count - 1];
			tempList.Clear();
			mTempUnuseList.RemoveAt(mTempUnuseList.Count - 1);
		}
		else
		{
			tempList = new List<T>();
		}
		mTempInuseList.Add(tempList);
		tempList.AddRange(mMainList);
		return tempList;
	}
	// 遍历结束后,需要手动调用endForeach,对临时列表进行回收
	public void endForeach(List<T> list)
	{
		mTempInuseList.Remove(list);
		mTempUnuseList.Add(list);
	}
	// 获取主列表,存储着当前实时的数据列表,所有的删除和新增都会立即更新此列表
	// 如果确保在遍历过程中不会对列表进行修改,则可以使用MainList
	// 如果可能会对列表进行修改,则应该使用startForeach
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