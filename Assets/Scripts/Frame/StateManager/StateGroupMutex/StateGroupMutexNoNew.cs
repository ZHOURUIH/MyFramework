using System;
using System.Collections.Generic;

// 状态组中有状态时不允许添加新状态
public class StateGroupMutexNoNew : StateGroupMutex
{
	public override bool allowKeepState(Type newState, Type existState)
	{
		return true;
	}
	public override bool allowAddState(Type newState, Type existState)
	{
		return false;
	}
}