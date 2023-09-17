using System.Collections.Generic;
using static UnityUtility;

// 非线程安全
// 可安全遍历的列表,支持在遍历过程中对列表进行修改
public class SafeHashSet<T>
{
	protected List<SafeListModify<T>> mModifyList;  // 记录操作的列表,按顺序存储所有的操作
	protected HashSet<T> mUpdateList;               // 用于遍历更新的列表
	protected HashSet<T> mMainList;                 // 用于存储实时数据的列表
	protected bool mForeaching;						// 当前是否正在遍历中
	public SafeHashSet()
	{
		mModifyList = new List<SafeListModify<T>>();
		mUpdateList = new HashSet<T>();
		mMainList = new HashSet<T>();
	}
	// 获取用于更新的列表,会自动从主列表同步,遍历结束时需要调用endForeach
	public HashSet<T> startForeach()
	{
		if (mForeaching)
		{
			logError("当前列表正在遍历中,无法再次开始遍历");
			return null;
		}
		mForeaching = true;
		// 获取更新列表前,先同步主列表到更新列表,为了避免当列表过大时每次同步量太大
		// 所以单独使用了添加列表和移除列表,用来存储主列表的添加和移除的元素
		int mainCount = mMainList.Count;
		// 主列表为空,则直接清空即可
		if (mainCount == 0)
		{
			mUpdateList.Clear();
		}
		else
		{
			// 操作记录较少,则根据操作进行增删
			int modifyCount = mModifyList.Count;
			if (modifyCount < mainCount)
			{
				for (int i = 0; i < modifyCount; ++i)
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
			// 主列表元素较少,则直接同步主列表到更新列表
			else
			{
				mUpdateList.Clear();
				mUpdateList.UnionWith(mMainList);
			}
		}
		if (mUpdateList.Count != mMainList.Count)
		{
			logError("同步失败");
		}
		mModifyList.Clear();
		return mUpdateList;
	}
	public void endForeach()		{ mForeaching = false; }
	// 获取主列表,存储着当前实时的数据列表,所有的删除和新增都会立即更新此列表
	// 如果确保在遍历过程中不会对列表进行修改,则可以使用MainList
	// 如果可能会对列表进行修改,则应该使用getForeach
	public HashSet<T> getMainList() { return mMainList; }
	public bool contains(T value)	{ return mMainList.Contains(value); }
	public int count()				{ return mMainList.Count; }
	public bool add(T value)
	{
		if (!mMainList.Add(value))
		{
			return false;
		}
		mModifyList.Add(new SafeListModify<T>(value, true, -1));
		return true;
	}
	public bool remove(T value)
	{
		if (!mMainList.Remove(value))
		{
			return false;
		}
		mModifyList.Add(new SafeListModify<T>(value, false, -1));
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