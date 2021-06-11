using UnityEngine;
using System;
using System.Collections.Generic;

public class COMCharacterStateMachine : GameComponent
{
	protected static Dictionary<Type, List<Type>> mStateGroupList;  // 查找状态所在的所有组,key为状态类型,value为该状态所在的所有状态组
	protected static Dictionary<Type, StateGroup> mGroupStateList;  // 查找该组中的所有状态
	protected SafeDeepDictionary<Type, SafeDeepList<PlayerState>> mStateTypeList;   // 以状态类型为索引的状态列表
	protected Dictionary<Type, int> mGroupStateCountList;			// 存储每个状态组中拥有的状态数量,key是组类型,value是当前该组中拥有的状态数量
	protected Dictionary<uint, PlayerState> mStateMap;				// 以状态唯一ID为索引的列表,只用来根据ID查找状态
	protected Character mPlayer;									// 状态机所属角色
	public COMCharacterStateMachine()
	{
		mStateTypeList = new SafeDeepDictionary<Type, SafeDeepList<PlayerState>>();
		mStateMap = new Dictionary<uint, PlayerState>();
		mGroupStateCountList = new Dictionary<Type, int>();
	}
	public override void init(ComponentOwner owner)
	{
		base.init(owner);
		mPlayer = owner as Character;
		if (mGroupStateList != null)
		{
			foreach (var item in mGroupStateList)
			{
				mGroupStateCountList.Add(item.Key, 0);
			}
		}
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mGroupStateCountList.Clear();
		mStateTypeList.clear();
		mStateMap.Clear();
		mPlayer = null;
	}
	public override void destroy()
	{
		// 移除全部状态
		clearState();
		mGroupStateCountList.Clear();
		base.destroy();
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		var stateTypeList = mStateTypeList.startForeach();
		foreach (var itemList in stateTypeList)
		{
			var stateList = itemList.Value.startForeach();
			foreach(var item in stateList)
			{
				if (!item.isActive())
				{
					continue;
				}
				float frameTime = item.isIgnoreTimeScale() ? Time.unscaledDeltaTime : elapsedTime;
				item.update(frameTime);
				// 只有myself才能响应按键
				if (mPlayer.isMyself())
				{
					item.keyProcess(frameTime);
				}
			}
			itemList.Value.endForeach(stateList);
		}
		mStateTypeList.endForeach(stateTypeList);
	}
	public override void fixedUpdate(float elapsedTime)
	{
		base.fixedUpdate(elapsedTime);
		var stateTypeList = mStateTypeList.startForeach();
		foreach (var itemList in stateTypeList)
		{
			var stateList = itemList.Value.startForeach();
			foreach (var item in stateList)
			{
				if (!item.isActive())
				{
					continue;
				}
				item.fixedUpdate(elapsedTime);
			}
			itemList.Value.endForeach(stateList);
		}
		mStateTypeList.endForeach(stateTypeList);
	}
	public bool addState(Type type, StateParam param, out PlayerState state, float stateTime, uint id = 0)
	{
		state = createState(type, param, id);
		state.setPlayer(mPlayer);
		
		if (stateTime >= 0.0f)
		{
			state.setStateMaxTime(stateTime);
			state.setStateTime(stateTime);
		}
		if(!canAddState(state))
		{
			destroyState(state);
			return false;
		}

		// 移除不能共存的状态
		removeMutexState(state);
		// 进入状态
		state.enter();

		// 添加到状态列表
		addStateToList(state);
		return true;
	}
	// isBreak表示是否是因为与要添加的状态冲突,所以才移除的状态
	public bool removeState(PlayerState state, bool isBreak, string param = null)
	{
		if (!mStateMap.ContainsKey(state.getID()))
		{
			return false;
		}
		if (!state.isActive())
		{
			return false;
		}
		// 为了避免在一个状态中移除了其他状态,被移除的状态还可能继续更新的问题
		// 因为状态更新列表在遍历过程中是不会改变的,所以只能通过是否激活来准确判断状态是否有效
		state.setActive(false);

		// 从状态列表中移除
		removeStateFromList(state);

		state.leave(isBreak, param);

		// 销毁状态
		destroyState(state);
		return true;
	}
	// 移除一个状态组中的所有状态
	public void removeStateInGroup(Type group, bool isBreak, string param)
	{
		var stateUpdateList = mStateTypeList.startForeach();
		foreach (var item in stateUpdateList)
		{
			Type stateType = item.Key;
			if (!mStateGroupList.TryGetValue(stateType, out List<Type> groupList))
			{
				continue;
			}
			if (groupList.Contains(group))
			{
				var updateList = item.Value.startForeach();
				foreach(var state in updateList)
				{
					removeState(state, isBreak, param);
				}
				item.Value.endForeach(updateList);
			}
		}
		mStateTypeList.endForeach(stateUpdateList);
	}
	public void clearState()
	{
		// 遍历当前列表,待添加列表,退出所有状态,清空列表,通知角色
		var stateUpdateList = mStateTypeList.startForeach();
		foreach (var item in stateUpdateList)
		{
			var updateList = item.Value.startForeach();
			foreach (var state in updateList)
			{
				state.destroy();
				state.setActive(false);
				destroyState(state);
			}
			item.Value.endForeach(updateList);
		}
		mStateTypeList.endForeach(stateUpdateList);
		mStateTypeList.clear();
	}
	public SafeDeepDictionary<Type, SafeDeepList<PlayerState>> getStateList() { return mStateTypeList; }
	public PlayerState getState(uint id)
	{
		mStateMap.TryGetValue(id, out PlayerState state);
		return state;
	}
	public SafeDeepList<PlayerState> getState(Type type)
	{
		mStateTypeList.tryGetValue(type, out SafeDeepList<PlayerState> stateList);
		return stateList;
	}
	public PlayerState getFirstState(Type type)
	{
		if (!mStateTypeList.tryGetValue(type, out SafeDeepList<PlayerState> stateList))
		{
			return null;
		}
		foreach (var item in stateList.getMainList())
		{
			if (item.isActive())
			{
				return item;
			}
		}
		return null;
	}
	public PlayerState getFirstGroupState(Type groupType)
	{
		if (!mGroupStateList.TryGetValue(groupType, out StateGroup stateGroup))
		{
			return null;
		}
		foreach (var item in stateGroup.mStateList)
		{
			var curStateList = getState(item);
			if (curStateList != null && curStateList.count() > 0)
			{
				return curStateList.getMainList()[0];
			}
		}
		return null;
	}
	public void getGroupState(Type groupType, List<PlayerState> stateList)
	{
		if (!mGroupStateList.TryGetValue(groupType, out StateGroup stateGroup))
		{
			return;
		}
		foreach(var item in stateGroup.mStateList)
		{
			var curStateList = getState(item);
			if(curStateList != null)
			{
				stateList.AddRange(curStateList.getMainList());
			}
		}
	}
	// 是否拥有该组的任意一个状态
	public bool hasStateGroup(Type group)
	{
		return mGroupStateCountList[group] > 0;
	}
	public bool hasState(Type state)
	{
		return getFirstState(state) != null;
	}
	public static void registeGroup(Type type, GROUP_MUTEX mutex = GROUP_MUTEX.COEXIST)
	{
		if (mGroupStateList == null)
		{
			mGroupStateList = new Dictionary<Type, StateGroup>();
		}
		var group = createInstance<StateGroup>(type);
		group.setMutex(mutex);
		mGroupStateList.Add(type, group);
	}
	public static void assignGroup(Type groupType, Type state, bool mainState = false)
	{
		if (mStateGroupList == null)
		{
			mStateGroupList = new Dictionary<Type, List<Type>>();
		}
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
	public static StateGroup getStateGroup(Type groupType)
	{
		mGroupStateList.TryGetValue(groupType, out StateGroup group);
		return group;
	}
	//----------------------------------------------------------------------------------------------------------------
	protected PlayerState createState(Type type, StateParam param, uint id = 0)
	{
		// 用对象池的方式创建状态对象
		var state = CLASS(type) as PlayerState;
		state.setParam(param);
		if (id == 0)
		{
			id = generateGUID();
		}
		state.setID(id);
		return state;
	}
	protected void destroyState(PlayerState state)
	{
		UN_CLASS(state);
	}
	protected bool canAddState(PlayerState state)
	{
		if (!mutexWithExistState(state, out _))
		{
			return false;
		}
		Type stateType = Typeof(state);
		// 检查是否有跟要添加的状态互斥且不能移除的状态
		var stateTypeList = mStateTypeList.getMainList();
		foreach (var itemList in stateTypeList)
		{
			var stateList = itemList.Value.getMainList();
			foreach (var item in stateList)
			{
				if (item != state && item.isActive() && !allowAddStateByGroup(stateType, Typeof(item)))
				{
					return false;
				}
			}
		}
		// 检查自身限制条件
		if (!state.canEnter())
		{
			return false;
		}
		return true;
	}
	// 是否因为与当前已存在的同类型的互斥而无法添加此状态,返回值表示是否可以添加新的状态,如果可添加,则needRemoveState返回需要被移除的状态
	protected bool mutexWithExistState(PlayerState state, out PlayerState needRemoveState)
	{
		needRemoveState = null;
		// 移除自身不可共存的状态
		PlayerState existState = getFirstState(Typeof(state));
		if (existState == null)
		{
			return true;
		}
		STATE_MUTEX operate = existState.getMutexType();
		// 移除旧状态
		if (operate == STATE_MUTEX.REMOVE_OLD)
		{
			needRemoveState = existState;
			return true;
		}
		// 不允许添加新状态
		else if (operate == STATE_MUTEX.CAN_NOT_ADD_NEW)
		{
			return false;
		}
		// 相同的状态可以共存
		else if (operate == STATE_MUTEX.COEXIST)
		{
			return true;
		}
		// 只保留优先级最高的状态
		else if (operate == STATE_MUTEX.KEEP_HIGH_PRIORITY)
		{
			// 移除优先级低的旧状态,因为每次都只会保留优先级最高的状态,所以同时最多只会存在一个状态
			if (existState.getPriority() < state.getPriority())
			{
				needRemoveState = existState;
			}
			return existState.getPriority() < state.getPriority();
		}
		return true;
	}
	protected void removeMutexState(PlayerState state)
	{
		// 移除自身不可共存的状态
		PlayerState needRemoveState;
		mutexWithExistState(state, out needRemoveState);
		if (needRemoveState != null)
		{
			removeState(needRemoveState, true);
		}

		Type stateType = Typeof(state);
		// 移除状态组互斥的状态
		var stateForeachList = mStateTypeList.startForeach();
		foreach (var itemList in stateForeachList)
		{
			var stateList = itemList.Value.startForeach();
			foreach (var item in stateList)
			{
				if (item != state && item.isActive() && !allowKeepStateByGroup(stateType, Typeof(item)))
				{
					removeState(item, true);
				}
			}
			itemList.Value.endForeach(stateList);
		}
		mStateTypeList.endForeach(stateForeachList);
	}
	// 添加了newState以后是否还会保留existState
	protected bool allowKeepStateByGroup(Type newState, Type existState)
	{
		// 此处不考虑同类型状态,因为在这之前已经判断过了
		if(newState == existState)
		{
			return true;
		}
		if (!mStateGroupList.TryGetValue(newState, out List<Type> newGroup) ||
			!mStateGroupList.TryGetValue(existState, out List<Type> existGroup))
		{
			return true;
		}
		// 有一个状态不属于任何状态组时两个状态是可以共存的
		if(newGroup == null || existGroup == null)
		{
			return true;
		}
		int count0 = newGroup.Count;
		int count1 = existGroup.Count;
		for(int i = 0; i < count0; ++i)
		{
			StateGroup group = mGroupStateList[newGroup[i]];
			for (int j = 0; j < count1; ++j)
			{
				if(newGroup[i] != existGroup[j])
				{
					continue;
				}
				// 状态可共存
				if (group.mMutex == GROUP_MUTEX.COEXIST)
				{
					;
				}
				// 仅与主状态互斥,添加主状态时移除其他所有状态
				else if (group.mMutex == GROUP_MUTEX.MUTEX_WITH_MAIN)
				{
					if (group.mMainState == newState)
					{
						return false;
					}
				}
				// 仅与主状态互斥,添加主状态时移除其他所有状态
				else if (group.mMutex == GROUP_MUTEX.MUTEX_WITH_MAIN_ONLY)
				{
					if (group.mMainState == newState)
					{
						return false;
					}
				}
				// 不可添加新状态
				else if (group.mMutex == GROUP_MUTEX.NO_NEW)
				{
					return false;
				}
				// 添加新状态时移除所有旧状态
				else if (group.mMutex == GROUP_MUTEX.REMOVE_OTHERS)
				{
					return false;
				}
				// 主状态反向互斥,添加其他状态时,立即将主状态移除
				else if (group.mMutex == GROUP_MUTEX.MUTEX_INVERSE_MAIN)
				{
					// 存在主状态,则将主状态移除
					if(group.mMainState == existState)
					{
						return false;
					}
				}
			}
		}
		return true;
	}
	// 当存在existState时,是否允许添加newState
	protected bool allowAddStateByGroup(Type newState, Type existState)
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
				// 状态可共存
				if (group.mMutex == GROUP_MUTEX.COEXIST)
				{
					;
				}
				// 仅与主状态互斥,有主状态时不可添加其他状态,没有主状态时可任意添加其他状态
				else if (group.mMutex == GROUP_MUTEX.MUTEX_WITH_MAIN)
				{
					// 有主状态时不可添加其他状态
					if (group.mMainState == existState)
					{
						return false;
					}
				}
				// 仅与主状态互斥,无论是否有主状态都可以添加其他状态
				else if (group.mMutex == GROUP_MUTEX.MUTEX_WITH_MAIN_ONLY)
				{
					;
				}
				// 不可添加新状态
				else if (group.mMutex == GROUP_MUTEX.NO_NEW)
				{
					return false;
				}
				// 添加新状态时移除所有旧状态
				else if (group.mMutex == GROUP_MUTEX.REMOVE_OTHERS)
				{
					;
				}
				// 主状态反向互斥,有其他状态时,不允许添加主状态
				else if (group.mMutex == GROUP_MUTEX.MUTEX_INVERSE_MAIN)
				{
					if(group.mMainState == newState)
					{
						return false;
					}
				}
			}
		}
		return true;
	}
	protected void addStateToList(PlayerState state)
	{
		Type type = Typeof(state);
		if (!mStateTypeList.tryGetValue(type, out SafeDeepList<PlayerState> stateList))
		{
			stateList = new SafeDeepList<PlayerState>();
			mStateTypeList.add(type, stateList);
		}
		stateList.add(state);
		mStateMap.Add(state.getID(), state);

		// 添加状态组计数
		mStateGroupList.TryGetValue(type, out List<Type> groupList);
		if(groupList != null)
		{
			int groupCount = groupList.Count;
			for(int i = 0; i < groupCount; ++i)
			{
				mGroupStateCountList[groupList[i]] += 1;
			}
		}
	}
	protected void removeStateFromList(PlayerState state)
	{
		Type type = Typeof(state);
		mStateTypeList.tryGetValue(type, out SafeDeepList<PlayerState> stateList);
		if(stateList == null)
		{
			logError("state type error:" + type.Name);
		}
		stateList.remove(state);
		mStateMap.Remove(state.getID());
		// 从状态组计数中移除
		mStateGroupList.TryGetValue(type, out List<Type> groupList);
		if (groupList != null)
		{
			int groupCount = groupList.Count;
			for (int i = 0; i < groupCount; ++i)
			{
				mGroupStateCountList[groupList[i]] -= 1;
			}
		}
	}
}