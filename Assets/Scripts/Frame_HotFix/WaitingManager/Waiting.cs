using System;

public class Waiting : ClassObject
{
	protected BoolFunction mCondition;
	protected BoolFunction mCancelCondition;
	protected Action mDoneFunction;
	public override void resetProperty()
	{
		base.resetProperty();
		mCondition = null;
		mCancelCondition = null;
		mDoneFunction = null;
	}
	public void setCancelCondition(BoolFunction func) { mCancelCondition = func; }
	public void setCondition(BoolFunction func) { mCondition = func; }
	public void setDoneFunction(Action func) { mDoneFunction = func; }
	public bool isDone() { return mCondition?.Invoke() ?? false; }
	public bool isCancel() { return mCancelCondition?.Invoke() ?? false; }
	public void done() { mDoneFunction?.Invoke(); }
}