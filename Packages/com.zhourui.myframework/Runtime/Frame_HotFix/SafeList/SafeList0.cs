using System.Collections.Generic;
using static FrameUtility;

// 非线程安全
// 效率更高的可在遍历中再次开始遍历的列表
// 不过由于删除不是立即删除的,所以在部分情况下使用时需要注意
public class SafeList0<T> : ClassObject
{
	protected List<T> mMainList = new();
	protected int mForeachDepth;
	protected bool mNeedCompact;
	public override void resetProperty()
	{
		base.resetProperty();
		mMainList.Clear();
		mForeachDepth = 0;
		mNeedCompact = false;
	}
	public List<T> startForeach()
	{
		++mForeachDepth;
		return mMainList;
	}
	public void endForeach()
	{
		if (--mForeachDepth == 0 && mNeedCompact)
		{
			compact();
		}
	}
	public bool isEmpty() { return mMainList.isEmpty(); }
	public bool isForeaching() { return mForeachDepth > 0; }
    public List<T> getMainList() { return mMainList; }
    public int count() { return mMainList.Count; }
    // 根据下标获取元素,如果下标不合法则返回默认值
    public T get(int index)
    {
        return mMainList.get(index);
    }
    public void add(T value)
	{
		mMainList.add(value);
	}
	public bool remove(T value)
	{
		int index = mMainList.IndexOf(value);
		if (index < 0)
		{
			return false;
		}
		if (mForeachDepth > 0)
		{
			// 标记删除
			mMainList[index] = default;
			mNeedCompact = true;
		}
		else
		{
			mMainList.removeAt(index);
		}
		return true;
	}
	public void clear()
	{
		if (mForeachDepth > 0)
		{
			for (int i = 0; i < mMainList.Count; ++i)
			{
				mMainList[i] = default;
			}
			mNeedCompact = true;
		}
		else
		{
			mMainList.Clear();
		}
	}
	protected void compact()
	{
		int write = 0;
		int count = mMainList.Count;
		for (int i = 0; i < count; ++i)
		{
			if (!equal(mMainList[i], default))
			{
				mMainList[write++] = mMainList[i];
			}
		}
		mMainList.RemoveRange(write, count - write);
		mNeedCompact = false;
	}
}