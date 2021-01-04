using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

// 非线程安全
// 可安全遍历的列表,支持在遍历过程中对列表进行修改
public class SafeList<T> : FrameBase
{
	protected List<T> mMainList;	// 用于存储实时数据的列表
	protected List<T> mUpdateList;	// 用于遍历更新的列表
	protected List<T> mAddList;		// 添加操作的列表
	protected List<T> mRemoveList;	// 移除操作的列表
	public SafeList()
	{
		mMainList = new List<T>();
		mUpdateList = new List<T>();
		mAddList = new List<T>();
		mRemoveList = new List<T>();
	}
	// 获取用于更新的列表,会自动从主列表同步
	public List<T> GetUpdateList()
	{
		// 获取更新列表前,先同步主列表到更新列表,为了避免当列表过大时每次同步量太大
		// 所以单独使用了添加列表和移除列表,用来存储主列表的添加和移除的元素
		int removeCount = mRemoveList.Count;
		if (removeCount > 0)
		{
			for(int i = 0; i < removeCount; ++i)
			{
				mUpdateList.Remove(mRemoveList[i]);
			}
			mRemoveList.Clear();
		}
		int addCount = mAddList.Count;
		if (addCount > 0)
		{
			for(int i = 0; i < addCount; ++i)
			{
				mUpdateList.Add(mAddList[i]);
			}
			mAddList.Clear();
		}
		return mUpdateList;
	}
	// 获取主列表,存储着当前实时的数据列表,所有的删除和新增都会立即更新此列表
	// 不能用主列表进行遍历,要遍历应该使用GetUpdateList
	public List<T> GetMainList() { return mMainList; }
	public bool Contains(T value) { return mMainList.Contains(value); }
	public void Add(T value)
	{
		mRemoveList.Remove(value);
		mAddList.Add(value);
		mMainList.Add(value);
	}
	public void Remove(T value)
	{
		if (!mMainList.Contains(value))
		{
			return;
		}
		mRemoveList.Add(value);
		mAddList.Remove(value);
		mMainList.Remove(value);
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