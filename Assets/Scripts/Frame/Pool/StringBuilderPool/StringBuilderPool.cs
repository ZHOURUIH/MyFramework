using System.Collections.Generic;
using System.Text;

// 只能在主线程中使用的池
public class StringBuilderPool : FrameSystem
{
	protected HashSet<MyStringBuilder> mInusedList;
	protected HashSet<MyStringBuilder> mUnusedList;
	public StringBuilderPool()
	{
		mInusedList = new HashSet<MyStringBuilder>();
		mUnusedList = new HashSet<MyStringBuilder>();
		mCreateObject = true;
	}
	public override void init()
	{
		base.init();
#if UNITY_EDITOR
		mObject.AddComponent<StringBuilderPoolDebug>();
#endif
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
#if UNITY_EDITOR
		foreach (var item in mInusedList)
		{
			logError("有临时对象正在使用中,是否在申请后忘记回收到池中!");
			break;
		}
#endif
	}
	public HashSet<MyStringBuilder> getInusedList() { return mInusedList; }
	public HashSet<MyStringBuilder> getUnusedList() { return mUnusedList; }
	public new MyStringBuilder newBuilder()
	{
		if (!isMainThread())
		{
			logError("只能在主线程中使用StringBuilderPool");
			return null;
		}
		MyStringBuilder builder = null;
		// 先从未使用的列表中查找是否有可用的对象
		if (mUnusedList.Count > 0)
		{
			foreach (var item in mUnusedList)
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
		return builder;
	}
	// destroyReally表示是否真的要销毁bytes,如果真的回收,则会交给GC回收掉
	public new void destroyBuilder(MyStringBuilder builder, bool destroyReally = false)
	{
		if (!isMainThread())
		{
			logError("只能在主线程中使用StringBuilderPool");
			return;
		}
		if (builder == null)
		{
			return;
		}
		builder.Clear();
		if (!destroyReally)
		{
			addUnuse(builder);
		}
		removeInuse(builder);
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
			logError("builder is in Unused list! can not add again!");
			return;
		}
		mUnusedList.Add(builder);
	}
}