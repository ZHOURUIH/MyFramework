using System.Collections.Generic;

public struct DictionaryModify<Key, Value>
{
	public Key mKey;
	public Value mValue;
	public bool mAdd;
	public DictionaryModify(Key key)
	{
		mKey = key;
		mValue = (Value)default;
		mAdd = false;
	}
	public DictionaryModify(Key key, Value value)
	{
		mKey = key;
		mValue = value;
		mAdd = true;
	}
}

// 非线程安全
// 可安全遍历的列表,支持在遍历过程中对列表进行修改
public class SafeDictionary<Key, Value> : FrameBase
{
	// 由于在增删操作时可能会出现key相同但是value不相同的情况,甚至value的GetHashCode被重写导致即使GetHashCode相同,实例也不同的情况
	// 所以不再进行增量同步,而是按顺序记录所有的操作,使之完全与主列表同步
	protected List<DictionaryModify<Key, Value>> mModifyList;   // 记录操作的列表
	protected Dictionary<Key, Value> mUpdateList;               // 用于遍历更新的列表
	protected Dictionary<Key, Value> mMainList;                 // 用于存储实时数据的列表
	public SafeDictionary()
	{
		mModifyList = new List<DictionaryModify<Key, Value>>();
		mUpdateList = new Dictionary<Key, Value>();
		mMainList = new Dictionary<Key, Value>();
	}
	// 获取用于更新的列表,会自动从主列表同步
	public Dictionary<Key, Value> getUpdateList()
	{
		// 获取更新列表前,先同步主列表到更新列表,为了避免当列表过大时每次同步量太大
		// 所以单独使用了添加列表和移除列表,用来存储主列表的添加和移除的元素
		int modifyCount = mModifyList.Count;
		if (modifyCount < mMainList.Count)
		{
			for (int i = 0; i < modifyCount; ++i)
			{
				var modify = mModifyList[i];
				if (modify.mAdd)
				{
					mUpdateList.Add(modify.mKey, modify.mValue);
				}
				else
				{
					mUpdateList.Remove(modify.mKey);
				}
			}
		}
		else
		{
			mUpdateList.Clear();
			foreach (var item in mMainList)
			{
				mUpdateList.Add(item.Key, item.Value);
			}
		}
		if (mUpdateList.Count != mMainList.Count)
		{
			logError("同步失败");
		}
		else
		{
#if UNITY_EDITOR
			foreach (var item in mMainList)
			{
				if (!mUpdateList.TryGetValue(item.Key, out Value value))
				{
					logError("列表长度相同,但是值不一致");
				}
				else
				{
					if (value == null)
					{
						if (item.Value != null)
						{
							logError("列表长度相同,但是值不一致");
						}
					}
					else
					{
						if (!value.Equals(item.Value))
						{
							logError("列表长度相同,但是值不一致");
						}
					}
				}
			}
#endif
		}
		mModifyList.Clear();
		return mUpdateList;
	}
	// 获取主列表,存储着当前实时的数据列表,所有的删除和新增都会立即更新此列表
	// 不能用主列表进行遍历,要遍历应该使用GetUpdateList,但是也需要注意避免遍历UpdateList再次GetUpdateList
	// 也不能使用主列表直接进行添加或删除
	public Dictionary<Key, Value> getMainList() { return mMainList; }
	public bool tryGetValue(Key key, out Value value) { return mMainList.TryGetValue(key, out value); }
	public bool containsKey(Key key) { return mMainList.ContainsKey(key); }
	public bool containsValue(Value value) { return mMainList.ContainsValue(value); }
	public int count() { return mMainList.Count; }
	public bool add(Key key, Value value)
	{
		if (mMainList.ContainsKey(key))
		{
			return false;
		}
		mMainList.Add(key, value);
		mModifyList.Add(new DictionaryModify<Key, Value>(key, value));
		return true;
	}
	public bool remove(Key key)
	{
		if (!mMainList.Remove(key))
		{
			return false;
		}
		mModifyList.Add(new DictionaryModify<Key, Value>(key));
		return true;
	}
	// 清空所有数据,不能正在遍历时调用
	public void clear()
	{
		mMainList.Clear();
		mUpdateList.Clear();
		mModifyList.Clear();
	}
}