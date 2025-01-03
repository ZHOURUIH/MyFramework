using System.Collections.Generic;

// 非线程安全
// 可深度嵌套安全遍历的列表,支持在遍历过程中嵌套遍历和对列表进行修改
// 由于可嵌套,即在遍历中再次开始遍历,所以效率较低,使用方法上比普通列表稍复杂
public class SafeDeepDictionary<Key, Value> : ClassObject
{
	protected HashSet<Dictionary<Key, Value>> mTempInuseList = new();	// 用于缓存正在使用的临时列表,由于需要考虑到效率,所以不使用对象池
	protected Queue<Dictionary<Key, Value>> mTempUnuseList = new();		// 用于缓存未使用的临时列表
	protected Dictionary<Key, Value> mMainList = new();					// 用于存储实时数据的列表
	public override void resetProperty()
	{
		base.resetProperty();
		mTempInuseList.Clear();
		mTempUnuseList.Clear();
		mMainList.Clear();
	}
	// 获取用于更新的列表
	public Dictionary<Key, Value> startForeach() 
	{
		// 由于需要考虑嵌套,所以只能创建一个新的列表,复制当前主列表的数据
		Dictionary<Key, Value> tempList = mTempUnuseList.Count > 0 ? mTempUnuseList.Dequeue() : new();
		mTempInuseList.Add(tempList);
		tempList.setRange(mMainList);
		return tempList;
	}
	// 遍历结束后,需要手动调用endForeach,对临时列表进行回收
	public void endForeach(Dictionary<Key, Value> list)
	{
		list.Clear();
		mTempInuseList.Remove(list);
		mTempUnuseList.Enqueue(list);
	}
	// 获取主列表,存储着当前实时的数据列表,所有的删除和新增都会立即更新此列表
	// 如果确保在遍历过程中不会对列表进行修改,则可以使用MainList
	// 如果可能会对列表进行修改,则应该使用startForeach
	public Dictionary<Key, Value> getMainList() { return mMainList; }
	public bool tryGetValue(Key key, out Value value) { return mMainList.TryGetValue(key, out value); }
	public Value tryGet(Key key) { return mMainList.get(key); }
	public bool containsKey(Key key) { return mMainList.ContainsKey(key); }
	public void add(Key key, Value value)
	{
		mMainList.Add(key, value);
	}
	public void remove(Key key)
	{
		mMainList.Remove(key);
	}
	public void clear()
	{
		mMainList.Clear();
	}
}