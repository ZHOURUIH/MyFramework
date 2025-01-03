using System;
using static FrameUtility;

// 用于等待多个协程执行完成
public class AsyncTaskGroupManager : FrameSystem
{
	public SafeList<AsyncTaskGroup> mGroupList = new();    // 协程迭代器列表
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		using var a = new SafeListReader<AsyncTaskGroup>(mGroupList);
		foreach (AsyncTaskGroup item in a.mReadList)
		{
			if (item.checkDone())
			{
				destroyGroup(item);
			}
		}
	}
	public AsyncTaskGroup createGroup(Action callback)
	{
		CLASS(out AsyncTaskGroup group).setCallback(callback);
		mGroupList.add(group);
		return group;
	}
	// 只有当外部需要中断异步任务时,才会需要调用此函数,管理器会自动将完成的对象销毁
	public void destroyGroup(AsyncTaskGroup group)
	{
		if (mGroupList.remove(group))
		{
			UN_CLASS(ref group);
		}
	}
}