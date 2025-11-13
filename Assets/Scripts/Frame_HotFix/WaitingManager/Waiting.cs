using System;
using System.Collections.Generic;
using static MathUtility;

public class Waiting : ClassObject
{
	protected List<CustomAsyncOperation> mAsyncList;
	protected List<BoolFunction> mConditionList;
	protected BoolFunction mCancelCondition;
	protected Action mDoneFunction;
	protected bool mHasCallDone;                        // mDoneFunction是否已经调用过了
	protected bool mAutoDestroy;						// 是否在所有条件完成后自动销毁
	public override void resetProperty()
	{
		base.resetProperty();
		mAsyncList?.Clear();
		mConditionList?.Clear();
		mCancelCondition = null;
		mDoneFunction = null;
		mHasCallDone = false;
		mAutoDestroy = false;
	}
	// 就只简单根据数量来计算进度
	public float getProgress()
	{
		if (mAsyncList.count() + mConditionList.count() == 0)
		{
			return 1.0f;
		}
		int doneCount = 0;
		foreach (CustomAsyncOperation op in mAsyncList.safe())
		{
			if (!op.keepWaiting)
			{
				++doneCount;
			}
		}
		foreach (BoolFunction func in mConditionList.safe())
		{
			if (func())
			{
				++doneCount;
			}
		}
		return divide(doneCount, mAsyncList.count() + mConditionList.count());
	}
	public void setCancelCondition(BoolFunction func) { mCancelCondition = func; }
	public void addCondition(BoolFunction func) 
	{
		mConditionList ??= new();
		mConditionList?.Add(func); 
	}
	public void addAsyncOperation(CustomAsyncOperation op) 
	{
		mAsyncList ??= new();
		mAsyncList.Add(op);
	}
	public void setDoneFunction(Action func) { mDoneFunction = func; }
	public bool isDone() 
	{
		foreach (CustomAsyncOperation op in mAsyncList.safe())
		{
			if (op.keepWaiting)
			{
				return false;
			}
		}
		foreach (BoolFunction func in mConditionList.safe())
		{
			if (!func())
			{
				return false;
			}
		}
		return true;
	}
	public bool isCancel() { return mCancelCondition?.Invoke() ?? false; }
	public void done() 
	{
		if (mHasCallDone)
		{
			return;
		}
		mHasCallDone = true;
		mDoneFunction?.Invoke(); 
	}
	public void setAutoDestroy(bool auto) { mAutoDestroy = auto; }
	public bool isAutoDestroy() { return mAutoDestroy; }
}