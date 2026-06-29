using System;

// 各状态可完全共存
public class StateGroupMutexCoexist : StateGroupMutex
{
	public override bool allowKeepState(Type newState, Type existState)
	{
		return true;
	}
	public override bool allowAddState(Type newState, Type existState)
	{
		return true;
	}
}