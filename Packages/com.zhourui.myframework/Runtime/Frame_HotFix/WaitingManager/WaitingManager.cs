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
	// 当op0完成时,调用done回调
	public Waiting wait(CustomAsyncOperation op0, Action done, bool autoDestroy = true)
	{
		Waiting waitObj = createWait(done, autoDestroy);
		waitObj.addAsyncOperation(op0);
		waitObj.setAutoDestroy(autoDestroy);
		return waitObj;
	}
	// 当op0,op1都完成时,调用done回调
	public Waiting wait(CustomAsyncOperation op0, CustomAsyncOperation op1, Action done, bool autoDestroy = true)
	{
		Waiting waitObj = createWait(done, autoDestroy);
		waitObj.addAsyncOperation(op0);
		waitObj.addAsyncOperation(op1);
		waitObj.setAutoDestroy(autoDestroy);
		return waitObj;
	}
	// 当op0,op1,op2都完成时,调用done回调
	public Waiting wait(CustomAsyncOperation op0, CustomAsyncOperation op1, CustomAsyncOperation op2, Action done, bool autoDestroy = true)
	{
		Waiting waitObj = createWait(done, autoDestroy);
		waitObj.addAsyncOperation(op0);
		waitObj.addAsyncOperation(op1);
		waitObj.addAsyncOperation(op2);
		waitObj.setAutoDestroy(autoDestroy);
		return waitObj;
	}
	// 当condition返回true时,调用done回调
	public Waiting wait(BoolFunction condition, Action done, bool autoDestroy = true)
	{
		Waiting waitObj = createWait(done, autoDestroy);
		waitObj.addCondition(condition);
		waitObj.setAutoDestroy(autoDestroy);
		return waitObj;
	}
	public Waiting createWait(Action done, bool autoDestroy = true)
	{
		Waiting waiting = mList.addClass();
		waiting.setDoneFunction(done);
		waiting.setAutoDestroy(autoDestroy);
		return waiting;
	}
	public void cancel(ref Waiting waiting)
	{
		destroyWait(ref waiting);
	}
	public void destroyWait(ref Waiting waiting)
	{
		mList.remove(waiting);
		UN_CLASS(ref waiting);
	}
}