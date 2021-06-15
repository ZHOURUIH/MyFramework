using System;
using System.Collections.Generic;

public abstract class StateGroupMutex : FrameBase
{
	protected StateGroup mGroup;
	protected GROUP_MUTEX mMutexType;
	public GROUP_MUTEX getMutexType() { return mMutexType; }
	public StateGroup getGroup() { return mGroup; }
	public void setMutexType(GROUP_MUTEX type) { mMutexType = type; }
	public void setGroup(StateGroup group) { mGroup = group; }
	// 添加了newState以后是否还会保留existState
	public abstract bool allowKeepState(Type newState, Type existState);
	// 当存在existState时,是否允许添加newState
	public abstract bool allowAddState(Type newState, Type existState);
}