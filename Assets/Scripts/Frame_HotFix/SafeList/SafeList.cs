using System;
using System.Collections.Generic;
using static UnityUtility;
using static FrameBaseUtility;

// 非线程安全
// 可安全遍历的列表,支持在遍历过程中对列表进行修改
public class SafeList<T> : ClassObject
{
	protected List<SafeListModify<T>> mModifyList = new();  // 记录操作的列表,按顺序存储所有的操作
	protected List<T> mUpdateList = new();                  // 用于遍历更新的列表
	protected List<T> mMainList = new();                    // 用于存储实时数据的列表
	protected string mLastFileName;							// 上一次开始遍历时的文件名
	protected bool mForeaching;                             // 当前是否正在遍历中
	public override void resetProperty()
	{
		base.resetProperty();
		mModifyList.Clear();
		mUpdateList.Clear();
		mMainList.Clear();
		mLastFileName = null;
		mForeaching = false;
	}
	// 获取用于更新的列表,会自动从主列表同步,遍历结束时需要调用endForeach
	// 搭配SafeListScope使用,using var a = new SafeListScope<T>(safeList);然后遍历a.mReadList
	public List<T> startForeach(string fileName = null)
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
						if (isEditor() && !value.mValue.Equals(mUpdateList[value.mRemoveIndex]))
						{
							logError("同步列表数据错误");
						}
						mUpdateList.RemoveAt(value.mRemoveIndex);
					}
				}
			}
			// 主列表元素较少,则直接同步主列表到更新列表
			else
			{
				mUpdateList.setRange(mMainList);
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
	public bool isForeaching()		{ return mForeaching; }
	// 获取主列表,存储着当前实时的数据列表,所有的删除和新增都会立即更新此列表
	// 如果确保在遍历过程中不会对列表进行修改,则可以使用MainList
	// 如果可能会对列表进行修改,则应该使用startForeach
	public List<T> getMainList()	{ return mMainList; }
	public bool contains(T value)	{ return mMainList.Contains(value); }
	public T get(int index)			{ return mMainList[index]; }
	public int count()				{ return mMainList.Count; }
	public T find(Predicate<T> predicate) { return mMainList.Find(predicate); }
	// 因为只能保证开始遍历时mUpdateList与mMainList一致,但是遍历结束后两个列表可能就不一致了,所以即使没有正在遍历时,也只能是记录操作,而不是直接修改mUpdateList
	public T add(T value)
	{
		mMainList.Add(value);
		mModifyList.Add(new(value, true, -1));
		return value;
	}
	public void addUnique(T value)
	{
		if (!contains(value))
		{
			add(value);
		}
	}
	public void addNotNull(T value)
	{
		if (value == null)
		{
			return;
		}
		add(value);
	}
	public void addRange(IList<T> list)
	{
		foreach (T item in list)
		{
			mMainList.Add(item);
			mModifyList.Add(new(item, true, -1));
		}
	}
	public void setRange(IList<T> list)
	{
		clear();
		addRange(list);
	}
	public bool remove(T value)
	{
		int index = mMainList.IndexOf(value);
		if (index < 0 || index >= mMainList.Count)
		{
			return false;
		}
		mMainList.RemoveAt(index);
		mModifyList.Add(new(value, false, index));
		return true;
	}
	public void removeAt(int index)
	{
		if (index < 0 || index >= mMainList.Count)
		{
			return;
		}
		mModifyList.Add(new(mMainList.removeAt(index), false, index));
	}
	// 清空所有数据
	public void clear()
	{
		if (mForeaching)
		{
			int count = mMainList.Count;
			for (int i = 0; i < count; ++i)
			{
				mModifyList.Add(new(mMainList[i], false, i));
			}
		}
		else
		{
			mModifyList.Clear();
			mUpdateList.Clear();
		}
		mMainList.Clear();
	}
}