using System;
using static FrameUtility;

// 创建一个自定义的可等待对象,设置条件以及执行内容,会等到条件满足时执行内容
public class WaitingManager : FrameSystem
{
	public static WaitingManager mWaitingManager;
	protected SafeList<Waiting> mList = new();
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		using var a = new SafeListReader<Waiting>(mList);
		int count = a.mReadList.Count;
		for (int i = 0; i < count; ++i)
		{
			Waiting item = a.mReadList[i];
			bool isDone = item.isDone();
			if (isDone || item.isCancel())
			{
				if (isDone)
				{
					item.done();
				}
				if (item.isAutoDestroy())
				{
					mList.removeAt(i);
					UN_CLASS(ref item);
				}
			}
		}
	}
	public Waiting createWaiting(CustomAsyncOperation op0, Action done)
	{
		Waiting wait = createWaiting(done);
		wait.addAsyncOperation(op0);
		return wait;
	}
	public Waiting createWaiting(CustomAsyncOperation op0, CustomAsyncOperation op1, Action done)
	{
		Waiting wait = createWaiting(done);
		wait.addAsyncOperation(op0);
		wait.addAsyncOperation(op1);
		return wait;
	}
	public Waiting createWaiting(CustomAsyncOperation op0, CustomAsyncOperation op1, CustomAsyncOperation op2, Action done)
	{
		Waiting wait = createWaiting(done);
		wait.addAsyncOperation(op0);
		wait.addAsyncOperation(op1);
		wait.addAsyncOperation(op2);
		return wait;
	}
	public Waiting createWaiting(BoolFunction condition, Action done)
	{
		Waiting wait = createWaiting(done);
		wait.addCondition(condition);
		return wait;
	}
	public Waiting createWaiting(Action done, bool autoDestroy = true)
	{
		Waiting waiting = mList.addClass();
		waiting.setDoneFunction(done);
		waiting.setAutoDestroy(autoDestroy);
		return waiting;
	}
	public void cancel(ref Waiting waiting)
	{
		destroyWaiting(ref waiting);
	}
	public void destroyWaiting(ref Waiting waiting)
	{
		mList.remove(waiting);
		UN_CLASS(ref waiting);
	}
}