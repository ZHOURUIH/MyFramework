using UnityEngine;
using System;
using System.Collections.Generic;

public class COMCharacterStateMachine : GameComponent
{
	protected SafeDeepDictionary<Type, SafeDeepList<CharacterState>> mStateTypeList;   // 以状态类型为索引的状态列表
	protected Dictionary<Type, int> mGroupStateCountList;			// 存储每个状态组中拥有的状态数量,key是组类型,value是当前该组中拥有的状态数量
	protected Dictionary<uint, CharacterState> mStateMap;			// 以状态唯一ID为索引的列表,只用来根据ID查找状态
	protected Character mPlayer;									// 状态机所属角色
	public COMCharacterStateMachine()
	{
		mStateTypeList = new SafeDeepDictionary<Type, SafeDeepList<CharacterState>>();
		mStateMap = new Dictionary<uint, CharacterState>();
		mGroupStateCountList = new Dictionary<Type, int>();
	}
	public override void init(ComponentOwner owner)
	{
		base.init(owner);
		mPlayer = owner as Character;
		foreach (var item in mStateManager.getGroupStateList())
		{
			mGroupStateCountList.Add(item.Key, 0);
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
			foreach (var item in stateList)
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
	public bool addState(Type type, StateParam param, out CharacterState state, float stateTime, uint id = 0)
	{
		state = createState(type, param, id);
		state.setCharacter(mPlayer);

		if (stateTime >= 0.0f)
		{
			state.setStateMaxTime(stateTime);
			state.setStateTime(stateTime);
		}
		if (!canAddState(state))
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
	public bool removeState(CharacterState state, bool isBreak, string param = null)
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
			List<Type> groupList = mStateManager.getGroupList(item.Key);
			if (groupList != null && groupList.Contains(group))
			{
				var updateList = item.Value.startForeach();
				foreach (var state in updateList)
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
	public SafeDeepDictionary<Type, SafeDeepList<CharacterState>> getStateList() { return mStateTypeList; }
	public CharacterState getState(uint instanceID)
	{
		mStateMap.TryGetValue(instanceID, out CharacterState state);
		return state;
	}
	public SafeDeepList<CharacterState> getState(Type type)
	{
		mStateTypeList.tryGetValue(type, out SafeDeepList<CharacterState> stateList);
		return stateList;
	}
	public CharacterState getFirstState(Type type)
	{
		if (!mStateTypeList.tryGetValue(type, out SafeDeepList<CharacterState> stateList))
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
	public CharacterState getFirstGroupState(Type groupType)
	{
		StateGroup group = mStateManager.getGroup(groupType);
		if (group == null)
		{
			return null;
		}
		foreach (var item in group.mStateList)
		{
			var curStateList = getState(item);
			if (curStateList != null && curStateList.count() > 0)
			{
				return curStateList.getMainList()[0];
			}
		}
		return null;
	}
	public void getGroupState(Type groupType, List<CharacterState> stateList)
	{
		StateGroup group = mStateManager.getGroup(groupType);
		if (group == null)
		{
			return;
		}
		foreach (var item in group.mStateList)
		{
			var curStateList = getState(item);
			if (curStateList != null)
			{
				stateList.AddRange(curStateList.getMainList());
			}
		}
	}
	// 是否拥有该组的任意一个状态
	public bool hasStateGroup(Type group) { return mGroupStateCountList[group] > 0; }
	public bool hasState(Type state) { return getFirstState(state) != null; }
	//------------------------------------------------------------------------------------------------------------------------------
	protected CharacterState createState(Type type, StateParam param, uint id = 0)
	{
		// 用对象池的方式创建状态对象
		var state = CLASS(type) as CharacterState;
		state.setParam(param);
		if (id == 0)
		{
			id = generateGUID();
		}
		state.setID(id);
		return state;
	}
	protected void destroyState(CharacterState state)
	{
		UN_CLASS(state);
	}
	protected bool canAddState(CharacterState state)
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
				if (item != state && item.isActive() && !mStateManager.allowAddStateByGroup(stateType, Typeof(item)))
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
	protected bool mutexWithExistState(CharacterState state, out CharacterState needRemoveState)
	{
		needRemoveState = null;
		// 移除自身不可共存的状态
		CharacterState existState = getFirstState(Typeof(state));
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
		else if (operate == STATE_MUTEX.NO_NEW)
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
	protected void removeMutexState(CharacterState state)
	{
		// 移除自身不可共存的状态
		CharacterState needRemoveState;
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
				if (item != state && item.isActive() && !mStateManager.allowKeepStateByGroup(stateType, Typeof(item)))
				{
					removeState(item, true);
				}
			}
			itemList.Value.endForeach(stateList);
		}
		mStateTypeList.endForeach(stateForeachList);
	}
	protected void addStateToList(CharacterState state)
	{
		Type type = Typeof(state);
		if (!mStateTypeList.tryGetValue(type, out SafeDeepList<CharacterState> stateList))
		{
			stateList = new SafeDeepList<CharacterState>();
			mStateTypeList.add(type, stateList);
		}
		stateList.add(state);
		mStateMap.Add(state.getID(), state);

		// 添加状态组计数
		List<Type> groupList = mStateManager.getGroupList(type);
		if (groupList != null)
		{
			int groupCount = groupList.Count;
			for (int i = 0; i < groupCount; ++i)
			{
				mGroupStateCountList[groupList[i]] += 1;
			}
		}
	}
	protected void removeStateFromList(CharacterState state)
	{
		Type type = Typeof(state);
		mStateTypeList.tryGetValue(type, out SafeDeepList<CharacterState> stateList);
		if (stateList == null)
		{
			logError("state type error:" + type.Name);
		}
		stateList.remove(state);
		mStateMap.Remove(state.getID());
		// 从状态组计数中移除
		List<Type> groupList = mStateManager.getGroupList(type);
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