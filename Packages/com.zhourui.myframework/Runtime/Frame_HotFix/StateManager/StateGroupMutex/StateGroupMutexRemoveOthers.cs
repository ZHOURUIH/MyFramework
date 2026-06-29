using System;

// 添加新状态时移除组中的其他所有状态
public class StateGroupMutexRemoveOthers : StateGroupMutex
{
	public override bool allowKeepState(Type newState, Type existState)
	{
		return false;
	}
	public override bool allowAddState(Type newState, Type existState)
	{
		return true;
	}
}