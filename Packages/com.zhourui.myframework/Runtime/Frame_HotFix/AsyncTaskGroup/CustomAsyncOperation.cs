using UnityEngine;

// 自定义异步操作,在协程中等待一个异步任务完成时使用
public class CustomAsyncOperation : CustomYieldInstruction
{
	protected bool mFinish;
	public override bool keepWaiting { get { return !mFinish; } }
	public override void Reset()
	{
		base.Reset();
		mFinish = false;
	}
	public CustomAsyncOperation setFinish() 
	{
		mFinish = true;
		return this; 
	}
}