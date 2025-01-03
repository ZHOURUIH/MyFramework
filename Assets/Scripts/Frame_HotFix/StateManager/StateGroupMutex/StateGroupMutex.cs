using System;

// 状态组的互斥关系
public abstract class StateGroupMutex : ClassObject
{
	protected StateGroup mGroup;		// 所属的状态组
	protected GROUP_MUTEX mMutexType;	// 互斥关系类型
	public override void resetProperty()
	{
		base.resetProperty();
		mGroup = null;
		mMutexType = GROUP_MUTEX.COEXIST;
	}
	public GROUP_MUTEX getMutexType() { return mMutexType; }
	public StateGroup getGroup() { return mGroup; }
	public void setMutexType(GROUP_MUTEX type) { mMutexType = type; }
	public void setGroup(StateGroup group) { mGroup = group; }
	// 添加了newState以后是否还会保留existState
	public abstract bool allowKeepState(Type newState, Type existState);
	// 当存在existState时,是否允许添加newState
	public abstract bool allowAddState(Type newState, Type existState);
}