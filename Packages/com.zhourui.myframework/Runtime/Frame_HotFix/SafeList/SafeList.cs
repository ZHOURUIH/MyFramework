using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using static UnityUtility;
using static FrameUtility;
using static FrameBaseUtility;

// 非线程安全
// 可安全遍历的列表,支持在遍历过程中对列表进行修改
public class SafeList<T> : ClassObject
{
	public struct SafeListEnumerator : IDisposable
	{
		private SafeList<T> mOwner;
		private List<T> mList;
		private T mCurrent;
		private int mIndex;
		private int mCount;
		public SafeListEnumerator(SafeList<T> safeList)
		{
			mOwner = safeList;
			mList = safeList.startForeach();
			mCurrent = default;
			mIndex = 0;
			mCount = mList.Count;
		}
		public T Current => mCurrent;
		// mUpdateList在一次SafeList foreach期间不会被修改,因此无需List<T>.Enumerator的version检查。
		// 固定遍历开始时的Count,继续保持当前foreach只看到旧快照的语义。
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public bool MoveNext()
		{
			int index = mIndex;
			if ((uint)index < (uint)mCount)
			{
				mCurrent = mList[index];
				mIndex = index + 1;
				return true;
			}
			mCurrent = default;
			return false;
		}
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public void Dispose() { mOwner.endForeach(); }
	}
	protected List<SafeListModify<T>> mModifyList = new();  // 记录无法立即同步到更新列表的操作,通常只会在遍历过程中产生
	protected List<T> mUpdateList = new();                  // 用于遍历更新的列表
	protected List<T> mMainList = new();                    // 用于存储实时数据的列表
	protected string mLastFileName;                         // 上一次开始遍历时的文件名
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
	public bool isForeaching() { return mForeaching; }
	public bool addOrRemove(T value, bool isAdd)
	{
		if (isAdd)
		{
			add(value);
		}
		else
		{
			remove(value);
		}
		return isAdd;
	}
	public bool addIf(T value, bool condition)
	{
		if (condition)
		{
			add(value);
		}
		return condition;
	}
	public void For(Action<T> action)
	{
		foreach (T item in mMainList)
		{
			action(item);
		}
	}
	// 安全遍历枚举器：foreach 时自动调用 startForeach，枚举器 Dispose 时自动调用 endForeach
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public SafeListEnumerator GetEnumerator() { return new(this); }
	// 获取主列表,存储着当前实时的数据列表,所有的删除和新增都会立即更新此列表
	// 如果确保在遍历过程中不会对列表进行修改,则可以使用MainList
	// 如果可能会对列表进行修改,则应该使用foreach安全遍历
	public List<T> getMainList() { return mMainList; }
	public bool contains(T value) { return mMainList.Contains(value); }
	public T get(int index) { return mMainList[index]; }
	public int count() { return mMainList.Count; }
	public T find(Predicate<T> predicate) { return mMainList.Find(predicate); }
	public T add(T value)
	{
		mMainList.Add(value);
		// 当前没有遍历并且没有历史待同步操作时,mUpdateList与mMainList必然同步,
		// 直接同时修改两个列表,避免为了下一次foreach再记录并回放一次操作
		if (!mForeaching && mModifyList.Count == 0)
		{
			mUpdateList.Add(value);
		}
		else
		{
			mModifyList.Add(new(value, true, -1));
		}
		return value;
	}
	public bool addUnique(T value)
	{
		if (!contains(value))
		{
			add(value);
			return true;
		}
		return false;
	}
	public void addNotNull(T value)
	{
		if (value == null)
		{
			return;
		}
		add(value);
	}
	public void addRange(List<T> list)
	{
		bool directSync = !mForeaching && mModifyList.Count == 0;
		foreach (T item in list)
		{
			mMainList.Add(item);
			if (directSync)
			{
				mUpdateList.Add(item);
			}
			else
			{
				mModifyList.Add(new(item, true, -1));
			}
		}
	}
	public void addRange(HashSet<T> list)
	{
		bool directSync = !mForeaching && mModifyList.Count == 0;
		foreach (T item in list)
		{
			mMainList.Add(item);
			if (directSync)
			{
				mUpdateList.Add(item);
			}
			else
			{
				mModifyList.Add(new(item, true, -1));
			}
		}
	}
	public void setRange(List<T> list)
	{
		clear();
		addRange(list);
	}
	public void setRange(HashSet<T> list)
	{
		clear();
		addRange(list);
	}
	public bool removeIf(T value, bool condition)
	{
		if (condition)
		{
			return remove(value);
		}
		return false;
	}
	public bool remove(T value)
	{
		int index = mMainList.IndexOf(value);
		if (index < 0 || index >= mMainList.Count)
		{
			return false;
		}
		bool directSync = !mForeaching && mModifyList.Count == 0;
		mMainList.RemoveAt(index);
		if (directSync)
		{
			if (isEditor() && !equal(value, mUpdateList[index]))
			{
				logError("同步列表数据错误");
			}
			mUpdateList.RemoveAt(index);
		}
		else
		{
			mModifyList.Add(new(value, false, index));
		}
		return true;
	}
	public T removeAt(int index)
	{
		if (index < 0 || index >= mMainList.Count)
		{
			return default;
		}
		bool directSync = !mForeaching && mModifyList.Count == 0;
		T value = mMainList.removeAt(index);
		if (directSync)
		{
			if (isEditor() && !equal(value, mUpdateList[index]))
			{
				logError("同步列表数据错误");
			}
			mUpdateList.RemoveAt(index);
		}
		else
		{
			mModifyList.Add(new(value, false, index));
		}
		return value;
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
	//------------------------------------------------------------------------------------------------------------------------------
	// 获取用于更新的列表。绝大多数foreach都没有待同步操作,先走极短快路径;
	// 只有遍历过程中发生过修改时,下一次foreach才进入syncUpdateList。
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected List<T> startForeach(string fileName = null)
	{
		if (mForeaching)
		{
			return startForeachError(fileName);
		}
		mLastFileName = fileName;
		mForeaching = true;
		if (mModifyList.Count == 0)
		{
			if (isEditor() && mUpdateList.Count != mMainList.Count)
			{
				logError("同步失败");
			}
			return mUpdateList;
		}
		return syncUpdateList();
	}
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	protected void endForeach() { mForeaching = false; }
	private List<T> startForeachError(string fileName)
	{
		logError("当前列表正在遍历中,无法再次开始遍历, 上一次开始遍历的地方:" + (mLastFileName ?? "") + ", 当前遍历的地方:" + fileName);
		// SafeList本身不支持同一列表嵌套foreach,保持原先safe()后的实际行为:内层得到空列表
		return EmptyList<T>.getEmptyList();
	}
	private List<T> syncUpdateList()
	{
		int mainCount = mMainList.Count;
		int modifyCount = mModifyList.Count;
		if (mainCount == 0)
		{
			mUpdateList.Clear();
		}
		else if (modifyCount < mainCount)
		{
			for (int i = 0; i < modifyCount; ++i)
			{
				SafeListModify<T> value = mModifyList[i];
				if (value.mAdd)
				{
					mUpdateList.Add(value.mValue);
				}
				else
				{
					if (isEditor() && !equal(value.mValue, mUpdateList[value.mRemoveIndex]))
					{
						logError("同步列表数据错误");
					}
					mUpdateList.RemoveAt(value.mRemoveIndex);
				}
			}
		}
		else
		{
			mUpdateList.setRange(mMainList);
		}
		if (mUpdateList.Count != mMainList.Count)
		{
			logError("同步失败");
		}
		mModifyList.Clear();
		return mUpdateList;
	}
}

// SafeList的扩展方法,提供便捷的添加ClassObject操作
public static class SafeListExtension
{
	// 由于需要添加额外的约束,所以只能写扩展函数
	public static T0 addClass<T0>(this SafeList<T0> list) where T0 : ClassObject, new()
	{
		CLASS(out T0 value);
		list.add(value);
		return value;
	}
}