using System;

// 用于延迟调用指定的函数
public class CmdGlobalDelayCall : Command
{
	public Action mFunction;			// 延迟调用的函数
	protected ClassObject mGuard;		// 用于校验是否可以执行延迟函数
	protected long mGuardAssignID;		// 用于校验是否可以执行延迟函数
	public override void resetProperty()
	{
		base.resetProperty();
		mFunction = null;
		mGuard = null;
		mGuardAssignID = 0;
	}
	public void setGuard(ClassObject guard)
	{
		mGuard = guard;
		mGuardAssignID = mGuard.getAssignID();
	}
	public override void execute()
	{
		if (mGuard != null && mGuard.getAssignID() != mGuardAssignID)
		{
			return;
		}
		mFunction?.Invoke();
	}
}