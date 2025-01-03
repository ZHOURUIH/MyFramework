using System;

// 用于延迟调用指定的函数
public class CmdGlobalDelayCallParam5<T0, T1, T2, T3, T4> : Command
{
	public Action<T0, T1, T2, T3, T4> mFunction;	// 延迟调用的函数
	public T0 mParam0;								// 函数的参数0
	public T1 mParam1;								// 函数的参数1
	public T2 mParam2;								// 函数的参数2
	public T3 mParam3;								// 函数的参数3
	public T4 mParam4;                              // 函数的参数4
	protected ClassObject mGuard;					// 用于校验是否可以执行延迟函数
	protected long mGuardAssignID;					// 用于校验是否可以执行延迟函数
	public override void resetProperty()
	{
		base.resetProperty();
		mFunction = null;
		mParam0 = default;
		mParam1 = default;
		mParam2 = default;
		mParam3 = default;
		mParam4 = default;
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
		mFunction?.Invoke(mParam0, mParam1, mParam2, mParam3, mParam4);
	}
}