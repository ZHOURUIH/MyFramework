using System.Collections.Generic;

// 多个异步等待操作组合起来的异步操作,所有异步操作完成时才认为是当前操作完成
// 与AsyncTaskGroup的区别在于CustomMultiAsyncOperation本身就是一个可等待的异步操作,适用于返回异步操作的情况
// AsyncTaskGroup适用于需要设置回调函数的情况
public class CustomMultiAsyncOperation : CustomAsyncOperation
{
	protected List<CustomAsyncOperation> mOperationList = new();
	public void addOperation(CustomAsyncOperation op) { mOperationList.add(op); }
	public override bool keepWaiting 
	{
		get 
		{
			foreach (CustomAsyncOperation op in mOperationList)
			{
				if (op.keepWaiting)
				{
					return true;
				}
			}
			return false;
		} 
	}
	public override void Reset()
	{
		base.Reset();
		mOperationList.Clear();
	}
}