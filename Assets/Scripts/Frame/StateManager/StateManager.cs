using System;
using System.Collections.Generic;

// 用作全局的状态管理,存储状态的类型相关信息
public class StateManager : FrameSystem
{
	protected Dictionary<GROUP_MUTEX, Type> mGroupMutexList;	// 根据枚举查找互斥关系的对象类型
	protected Dictionary<Type, List<Type>> mStateGroupList;		// 查找状态所在的所有组,key为状态类型,value为该状态所在的所有状态组
	protected Dictionary<Type, StateGroup> mGroupStateList;		// 状态组列表
	protected Dictionary<int, Type> mStateParamList;			// 状态参数类型列表
	protected Dictionary<int, Type> mStateTypeList;				// 状态类型列表
	public StateManager()
	{
		mGroupMutexList = new Dictionary<GROUP_MUTEX, Type>();
		mStateGroupList = new Dictionary<Type, List<Type>>();
		mGroupStateList = new Dictionary<Type, StateGroup>();
		mStateParamList = new Dictionary<int, Type>();
		mStateTypeList = new Dictionary<int, Type>();
		registeAllGroupMutex();
	}
	public override void init()
	{
		base.init();
	}
	public override void destroy()
	{
		base.destroy();
		foreach(var item in mGroupStateList)
		{
			item.Value.destroy();
			UN_CLASS(item.Value);
		}
		mGroupStateList.Clear();
	}
	public Type getStateType(int id)
	{
		mStateTypeList.TryGetValue(id, out Type type);
		return type;
	}
	public Type getParamType(int id)
	{
		mStateParamList.TryGetValue(id, out Type type);
		return type;
	}
	public StateParam createStateParam(int id)
	{
		Type type = getParamType(id);
		if (type == null)
		{
			return null;
		}
		// 创建的参数只允许在这一帧使用,所以需要使用UN_CLASS进行回收
		return CLASS_ONCE(type) as StateParam;
	}
	public Dictionary<Type, StateGroup> getGroupStateList() { return mGroupStateList; }
	public Dictionary<Type, List<Type>> getStateGroupList() { return mStateGroupList; }
	public List<Type> getGroupList(Type stateType) 
	{
		mStateGroupList.TryGetValue(stateType, out List<Type> groupList);
		return groupList;
	}
	public StateGroup getGroup(Type groupType) 
	{
		mGroupStateList.TryGetValue(groupType, out StateGroup group);
		return group;
	}
	public Type getGroupMutex(GROUP_MUTEX mutex)
	{
		mGroupMutexList.TryGetValue(mutex, out Type type);
		return type;
	}
	public void registeState(int id, Type stateType, Type paramType)
	{
		mStateTypeList.Add(id, stateType);
		mStateParamList.Add(id, paramType);
	}
	public void registeGroup(Type type, GROUP_MUTEX mutex = GROUP_MUTEX.COEXIST)
	{
		var group = CLASS(type) as StateGroup;
		group.setMutex(mutex);
		mGroupStateList.Add(type, group);
	}
	public void assignGroup(Type groupType, Type state, bool mainState = false)
	{
		if (!mStateGroupList.TryGetValue(state, out List<Type> stateTypeList))
		{
			stateTypeList = new List<Type>();
			mStateGroupList.Add(state, stateTypeList);
		}
		stateTypeList.Add(groupType);
		if (!mGroupStateList.TryGetValue(groupType, out StateGroup group))
		{
			logError("找不到状态组,无法将状态指定到组中,状态组类型:" + groupType);
			return;
		}
		group.addState(state);
		if (mainState)
		{
			group.setMainState(state);
		}
	}
	public StateGroup getStateGroup(Type groupType)
	{
		mGroupStateList.TryGetValue(groupType, out StateGroup group);
		return group;
	}
	// 添加了newState以后是否还会保留existState
	public bool allowKeepStateByGroup(Type newState, Type existState)
	{
		// 此处不考虑同类型状态,因为在这之前已经判断过了
		if (newState == existState)
		{
			return true;
		}
		if (!mStateGroupList.TryGetValue(newState, out List<Type> newGroup) ||
			!mStateGroupList.TryGetValue(existState, out List<Type> existGroup))
		{
			return true;
		}
		// 有一个状态不属于任何状态组时两个状态是可以共存的
		if (newGroup == null || existGroup == null)
		{
			return true;
		}
		int count0 = newGroup.Count;
		int count1 = existGroup.Count;
		for (int i = 0; i < count0; ++i)
		{
			StateGroup group = mGroupStateList[newGroup[i]];
			for (int j = 0; j < count1; ++j)
			{
				if (newGroup[i] != existGroup[j])
				{
					continue;
				}
				if(!group.allowKeepState(newState, existState))
				{
					return false;
				}
			}
		}
		return true;
	}
	// 当存在existState时,是否允许添加newState
	public bool allowAddStateByGroup(Type newState, Type existState)
	{
		// 此处不考虑同类型状态,因为在这之前已经判断过了
		if (newState == existState)
		{
			return true;
		}
		// 任意一个状态没有所属组,则不在同一组
		if (!mStateGroupList.TryGetValue(newState, out List<Type> newGroup) ||
			!mStateGroupList.TryGetValue(existState, out List<Type> existGroup))
		{
			return true;
		}
		// 有一个状态不属于任何状态组时两个状态是可以共存的
		if (newGroup == null || existGroup == null)
		{
			return true;
		}
		int count0 = newGroup.Count;
		int count1 = existGroup.Count;
		for (int i = 0; i < count0; ++i)
		{
			StateGroup group = mGroupStateList[newGroup[i]];
			for (int j = 0; j < count1; ++j)
			{
				// 属于同一状态组,并且该状态组中的所有状态都不能共存,而且不允许添加跟当前状态互斥的状态,则不允许添加该状态
				if (newGroup[i] != existGroup[j])
				{
					continue;
				}
				if(!group.allowAddState(newState, existState))
				{
					return false;
				}
			}
		}
		return true;
	}
	public void registeGroupMutex(Type type, GROUP_MUTEX mutex)
	{
		mGroupMutexList.Add(mutex, type);
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void registeAllGroupMutex()
	{
		registeGroupMutex(typeof(StateGroupMutexCoexist), GROUP_MUTEX.COEXIST);
		registeGroupMutex(typeof(StateGroupMutexRemoveOthers), GROUP_MUTEX.REMOVE_OTHERS);
		registeGroupMutex(typeof(StateGroupMutexNoNew), GROUP_MUTEX.NO_NEW);
		registeGroupMutex(typeof(StateGroupMutexMutexWithMain), GROUP_MUTEX.MUTEX_WITH_MAIN);
		registeGroupMutex(typeof(StateGroupMutexMutexWithMainOnly), GROUP_MUTEX.MUTEX_WITH_MAIN_ONLY);
		registeGroupMutex(typeof(StateGroupMutexMutexInverseMain), GROUP_MUTEX.MUTEX_INVERSE_MAIN);
	}
}