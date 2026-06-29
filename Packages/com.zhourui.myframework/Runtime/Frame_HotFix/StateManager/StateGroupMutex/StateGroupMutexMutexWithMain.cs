using System;

// 仅与主状态互斥,添加主状态时移除其他所有状态,有主状态时不可添加其他状态,没有主状态时可任意添加其他状态
public class StateGroupMutexMutexWithMain : StateGroupMutex
{
	public override bool allowKeepState(Type newState, Type existState)
	{
		return mGroup.mMainState != newState;
	}
	public override bool allowAddState(Type newState, Type existState)
	{
		// 有主状态时不可添加其他状态
		return mGroup.mMainState != existState;
	}
}