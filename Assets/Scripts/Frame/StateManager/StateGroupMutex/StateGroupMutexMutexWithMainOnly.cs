using System;
using System.Collections.Generic;

// 仅与主状态互斥,添加主状态时移除其他所有状态,无论是否有主状态都可以添加其他状态
public class StateGroupMutexMutexWithMainOnly : StateGroupMutex
{
	public override bool allowKeepState(Type newState, Type existState)
	{
		if (mGroup.mMainState == newState)
		{
			return false;
		}
		return true;
	}
	public override bool allowAddState(Type newState, Type existState)
	{
		return true;
	}
}