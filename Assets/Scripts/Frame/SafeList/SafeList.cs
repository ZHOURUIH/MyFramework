using System.Collections.Generic;
using static UnityUtility;

// 非线程安全
// 可安全遍历的列表,支持在遍历过程中对列表进行修改
public class SafeList<T>
{
	protected List<SafeListModify<T>> mModifyList;  // 记录操作的列表,按顺序存储所有的操作
	protected List<T> mUpdateList;                  // 用于遍历更新的列表
	protected List<T> mMainList;                    // 用于存储实时数据的列表
	protected bool mForeaching;                     // 当前是否正在遍历中
	protected string mLastFileName;                                 // 上一次开始遍历时的文件名
	public SafeList()
	{
		mModifyList = new List<SafeListModify<T>>();
		mUpdateList = new List<T>();
		mMainList = new List<T>();
	}
	// 获取用于更新的列表,会自动从主列表同步,遍历结束时需要调用endForeach
	public List<T> startForeach(string fileName = null)
	{
		if (mForeaching)
		{
			string lastFileName = mLastFileName != null ? mLastFileName : "";
			logError("当前列表正在遍历中,无法再次开始遍历, 上一次开始遍历的地方:" + lastFileName + ", 当前遍历的地方:" + fileName);
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
#if UNITY_EDITOR
						if (!value.mValue.Equals(mUpdateList[value.mRemoveIndex]))
						{
							logError("同步列表数据错误");
						}
#endif
						mUpdateList.RemoveAt(value.mRemoveIndex);
					}
				}
			}
			// 主列表元素较少,则直接同步主列表到更新列表
			else
			{
				mUpdateList.Clear();
				mUpdateList.AddRange(mMainList);
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
	public List<T> getMainList()	{ return mMainList; }
	public bool contains(T value)	{ return mMainList.Contains(value); }
	public T get(int index)			{ return mMainList[index]; }
	public int count()				{ return mMainList.Count; }
	public void add(T value)
	{
		mMainList.Add(value);
		mModifyList.Add(new SafeListModify<T>(value, true, -1));
	}
	public void remove(T value)
	{
		int index = mMainList.IndexOf(value);
		if (index < 0 || index >= mMainList.Count)
		{
			return;
		}
		mMainList.RemoveAt(index);
		mModifyList.Add(new SafeListModify<T>(value, false, index));
	}
	public void removeAt(int index)
	{
		if (index < 0 || index >= mMainList.Count)
		{
			return;
		}
		mModifyList.Add(new SafeListModify<T>(mMainList[index], false, index));
		mMainList.RemoveAt(index);
	}
	// 清空所有数据,不能正在遍历时调用
	public void clear()
	{
		mMainList.Clear();
		mUpdateList.Clear();
		mModifyList.Clear();
	}
}