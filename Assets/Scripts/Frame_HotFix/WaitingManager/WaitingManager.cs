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
				UN_CLASS(item);
				mList.removeAt(i);
			}
		}
	}
	public Waiting createWaiting(CustomAsyncOperation op0, Action done)
	{
		return createWaiting(() => { return !op0.keepWaiting; }, done);
	}
	public Waiting createWaiting(CustomAsyncOperation op0, CustomAsyncOperation op1, Action done)
	{
		return createWaiting(() => { return !op0.keepWaiting && !op1.keepWaiting; }, done);
	}
	public Waiting createWaiting(CustomAsyncOperation op0, CustomAsyncOperation op1, CustomAsyncOperation op2, Action done)
	{
		return createWaiting(() => { return !op0.keepWaiting && !op1.keepWaiting && !op2.keepWaiting; }, done);
	}
	public Waiting createWaiting(BoolFunction condition, Action done)
	{
		// 如果条件立即满足,则不再创建对象,直接执行
		if (condition?.Invoke() ?? false)
		{
			done?.Invoke();
			return null;
		}

		mList.add(CLASS(out Waiting waiting));
		waiting.setCondition(condition);
		waiting.setDoneFunction(done);
		return waiting;
	}
	public void cancel(ref Waiting waiting)
	{
		UN_CLASS(waiting);
		mList.remove(waiting);
		waiting = null;
	}
}