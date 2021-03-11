using System;
using System.Text;
using System.Collections.Generic;

// 线程安全的池,但是效率较低
public class StringBuilderPoolThread : FrameSystem
{
	protected HashSet<MyStringBuilder> mInusedList;
	protected HashSet<MyStringBuilder> mUnusedList;
	protected ThreadLock mListLock;
	public StringBuilderPoolThread()
	{
		mInusedList = new HashSet<MyStringBuilder>();
		mUnusedList = new HashSet<MyStringBuilder>();
		mListLock = new ThreadLock();
		mCreateObject = true;
	}
	public override void init()
	{
		base.init();
#if UNITY_EDITOR
		mObject.AddComponent<StringBuilderPoolThreadDebug>();
#endif
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
#if UNITY_EDITOR
		mListLock.waitForUnlock();
		foreach (var item in mInusedList)
		{
			logError("有临时对象正在使用中,是否在申请后忘记回收到池中!");
			break;
		}
		mListLock.unlock();
#endif
	}
	public HashSet<MyStringBuilder> getInusedList() { return mInusedList; }
	public HashSet<MyStringBuilder> getUnusedList() { return mUnusedList; }
	public new MyStringBuilder newBuilder()
	{
		MyStringBuilder builder = null;
		// 锁定期间不能调用任何其他非库函数,否则可能会发生死锁
		mListLock.waitForUnlock();
		try
		{
			// 先从未使用的列表中查找是否有可用的对象
			if (mUnusedList.Count > 0)
			{
				foreach(var item in mUnusedList)
				{
					builder = item;
					break;
				}
				mUnusedList.Remove(builder);
			}
			// 未使用列表中没有,创建一个新的
			else
			{
				builder = new MyStringBuilder();
			}
			// 标记为已使用
			addInuse(builder);
		}
		catch (Exception e)
		{
			logError(e.Message);
		}
		mListLock.unlock();
		return builder;
	}
	// destroyReally表示是否真的要销毁bytes,如果真的回收,则会交给GC回收掉
	public new void destroyBuilder(MyStringBuilder builder, bool destroyReally = false)
	{
		if (builder == null)
		{
			return;
		}
		mListLock.waitForUnlock();
		try
		{
			if(!destroyReally)
			{
				addUnuse(builder);
			}
			removeInuse(builder);
		}
		catch (Exception e)
		{
			logError(e.Message);
		}
		mListLock.unlock();
	}
	//----------------------------------------------------------------------------------------------------------------------------------------------
	protected void addInuse(MyStringBuilder builder)
	{
		if (mInusedList.Contains(builder))
		{
			logError("加入已使用列表失败");
			return;
		}
		// 加入使用列表
		mInusedList.Add(builder);
	}
	protected void removeInuse(MyStringBuilder builder)
	{
		// 从使用列表移除,要确保操作的都是从本类创建的实例
		if (!mInusedList.Remove(builder))
		{
			logError("从已使用列表移除失败");
		}
	}
	protected void addUnuse(MyStringBuilder builder)
	{
		// 加入未使用列表
		if (mUnusedList.Contains(builder))
		{
			logError("加入未使用列表失败");
			return;
		}
		mUnusedList.Add(builder);
	}
}