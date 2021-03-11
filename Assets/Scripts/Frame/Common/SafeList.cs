using System.Collections.Generic;

public struct ListModify<T>
{
	public T mValue;
	public bool mAdd;
	public ListModify(T value, bool add)
	{
		mValue = value;
		mAdd = add;
	}
}

// 非线程安全
// 可安全遍历的列表,支持在遍历过程中对列表进行修改
public class SafeList<T> : FrameBase
{
	protected List<ListModify<T>> mModifyList;	// 记录操作的列表,按顺序存储所有的操作
	protected List<T> mUpdateList;				// 用于遍历更新的列表
	protected List<T> mMainList;				// 用于存储实时数据的列表
	public SafeList()
	{
		mModifyList = new List<ListModify<T>>();
		mUpdateList = new List<T>();
		mMainList = new List<T>();
	}
	// 获取用于更新的列表,会自动从主列表同步
	public List<T> getUpdateList()
	{
		// 获取更新列表前,先同步主列表到更新列表,为了避免当列表过大时每次同步量太大
		// 所以单独使用了添加列表和移除列表,用来存储主列表的添加和移除的元素
		int modifyCount = mModifyList.Count;
		if(modifyCount < mMainList.Count)
		{
			for(int i = 0; i < modifyCount; ++i)
			{
				var value = mModifyList[i];
				if (value.mAdd)
				{
					mUpdateList.Add(value.mValue);
				}
				else
				{
					mUpdateList.Remove(value.mValue);
				}
			}
		}
		else
		{
			mUpdateList.Clear();
			mUpdateList.AddRange(mMainList);
		}
		if (mUpdateList.Count != mMainList.Count)
		{
			logError("同步失败");
		}
		else
		{
#if UNITY_EDITOR
			int count = mMainList.Count;
			for(int i = 0; i < count; ++i)
			{
				if (mMainList[i] == null)
				{
					if(mUpdateList[i] != null)
					{
						logError("列表长度相同,但是值不一致");
					}
				}
				else
				{
					if(!mMainList[i].Equals(mUpdateList[i]))
					{
						logError("列表长度相同,但是值不一致");
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
	public List<T> getMainList() { return mMainList; }
	public bool contains(T value) { return mMainList.Contains(value); }
	public T get(int index) { return mMainList[index]; }
	public int count() { return mMainList.Count; }
	public void add(T value)
	{
		mMainList.Add(value);
		mModifyList.Add(new ListModify<T>(value, true));
	}
	public void remove(T value)
	{
		if (!mMainList.Remove(value))
		{
			return;
		}
		mModifyList.Add(new ListModify<T>(value, false));
	}
	// 清空所有数据,不能正在遍历时调用
	public void clear()
	{
		mMainList.Clear();
		mUpdateList.Clear();
		mModifyList.Clear();
	}
}