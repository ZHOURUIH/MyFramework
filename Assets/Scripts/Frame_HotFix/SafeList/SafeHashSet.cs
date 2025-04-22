using System.Collections.Generic;
using static UnityUtility;

// 非线程安全
// 可安全遍历的列表,支持在遍历过程中对列表进行修改
public class SafeHashSet<T> : ClassObject
{
	protected List<SafeHashSetModify<T>> mModifyList = new();	// 记录操作的列表,按顺序存储所有的操作
	protected HashSet<T> mUpdateList = new();					// 用于遍历更新的列表
	protected HashSet<T> mMainList = new();						// 用于存储实时数据的列表
	protected string mLastFileName;                             // 上一次开始遍历时的文件名
	protected bool mForeaching;									// 当前是否正在遍历中
	public override void resetProperty()
	{
		base.resetProperty();
		mModifyList.Clear();
		mUpdateList.Clear();
		mMainList.Clear();
		mLastFileName = null;
		mForeaching = false;
	}
	// 获取用于更新的列表,会自动从主列表同步,遍历结束时需要调用endForeach,一般使用SafeHashSetReader来安全遍历
	public HashSet<T> startForeach(string fileName = null)
	{
		if (mForeaching)
		{
			logError("当前列表正在遍历中,无法再次开始遍历, 上一次开始遍历的地方:" + (mLastFileName ?? "") + ", 当前遍历的地方:" + fileName);
			return null;
		}
		mLastFileName = fileName;
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
			if (mModifyList.Count < mainCount)
			{
				foreach (var value in mModifyList)
				{
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
	// 如果可能会对列表进行修改,则应该使用startForeach
	public HashSet<T> getMainList() { return mMainList; }
	public bool contains(T value)	{ return mMainList.Contains(value); }
	public int count()				{ return mMainList.Count; }
	// 因为只能保证开始遍历时mUpdateList与mMainList一致,但是遍历结束后两个列表可能就不一致了,所以即使没有正在遍历时,也只能是记录操作,而不是直接修改mUpdateList
	public bool add(T value)
	{
		if (!mMainList.Add(value))
		{
			return false;
		}
		mModifyList.Add(new(value, true));
		return true;
	}
	public bool remove(T value)
	{
		if (!mMainList.Remove(value))
		{
			return false;
		}
		mModifyList.Add(new(value, false));
		return true;
	}
	// 清空所有数据
	public void clear()
	{
		if (mForeaching)
		{
			foreach (T item in mMainList)
			{
				mModifyList.Add(new(item, false));
			}
		}
		else
		{
			mUpdateList.Clear();
			mModifyList.Clear();
		}
		mMainList.Clear();
	}
}