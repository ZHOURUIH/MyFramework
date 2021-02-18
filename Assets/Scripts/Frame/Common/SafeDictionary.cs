using System.Collections.Generic;

// 非线程安全
// 可安全遍历的列表,支持在遍历过程中对列表进行修改
public class SafeDictionary<Key, Value> : FrameBase
{
	protected Dictionary<Key, Value> mMainList;     // 用于存储实时数据的列表
	protected Dictionary<Key, Value> mUpdateList;   // 用于遍历更新的列表
	protected Dictionary<Key, Value> mAddList;      // 添加操作的列表
	protected List<Key> mRemoveList;                // 移除操作的列表
	public SafeDictionary()
	{
		mMainList = new Dictionary<Key, Value>();
		mUpdateList = new Dictionary<Key, Value>();
		mAddList = new Dictionary<Key, Value>();
		mRemoveList = new List<Key>();
	}
	// 获取用于更新的列表,会自动从主列表同步
	public Dictionary<Key, Value> GetUpdateList()
	{
		// 获取更新列表前,先同步主列表到更新列表,为了避免当列表过大时每次同步量太大
		// 所以单独使用了添加列表和移除列表,用来存储主列表的添加和移除的元素
		int removeCount = mRemoveList.Count;
		if (removeCount > 0)
		{
			for (int i = 0; i < removeCount; ++i)
			{
				mUpdateList.Remove(mRemoveList[i]);
			}
			mRemoveList.Clear();
		}
		if (mAddList.Count > 0)
		{
			foreach (var item in mAddList)
			{
				mUpdateList.Add(item.Key, item.Value);
			}
			mAddList.Clear();
		}
		if (mUpdateList.Count != mMainList.Count)
		{
			logError("同步失败");
		}
		return mUpdateList;
	}
	// 获取主列表,存储着当前实时的数据列表,所有的删除和新增都会立即更新此列表
	// 不能用主列表进行遍历,要遍历应该使用GetUpdateList,但是也需要注意避免遍历UpdateList再次GetUpdateList
	// 也不能使用主列表直接进行添加或删除
	public Dictionary<Key, Value> GetMainList() { return mMainList; }
	public bool TryGetValue(Key key, out Value value) { return mMainList.TryGetValue(key, out value); }
	public bool ContainsKey(Key key) { return mMainList.ContainsKey(key); }
	public bool ContainsValue(Value value) { return mMainList.ContainsValue(value); }
	public void Add(Key key, Value value)
	{
		if (mMainList.ContainsKey(key))
		{
			return;
		}
		// 只有不在删除列表中才会认为是有效的添加操作
		if (!mRemoveList.Remove(key))
		{
			mAddList.Add(key, value);
		}
		mMainList.Add(key, value);
	}
	public void Remove(Key key)
	{
		if (!mMainList.ContainsKey(key))
		{
			return;
		}
		// 只有不在添加列表中才认为是有效的删除操作
		if (!mAddList.Remove(key))
		{
			mRemoveList.Add(key);
		}
		mMainList.Remove(key);
	}
	// 清空所有数据,不能正在遍历时调用
	public void Clear()
	{
		mMainList.Clear();
		mUpdateList.Clear();
		mAddList.Clear();
		mRemoveList.Clear();
	}
}