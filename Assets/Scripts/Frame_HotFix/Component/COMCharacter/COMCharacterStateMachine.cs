using UnityEngine;
using System;
using System.Collections.Generic;
using static UnityUtility;
using static FrameUtility;
using static FrameBaseHotFix;
using static MathUtility;

// 角色状态机组件,用于处理角色状态切换的内部逻辑
public class COMCharacterStateMachine : GameComponent
{
	protected SafeDictionary<Type, SafeList<CharacterState>> mStateTypeList = new();	// 以状态类型为索引的状态列表
	protected Dictionary<long, CharacterState> mStateMap = new();                       // 以状态唯一ID为索引的列表,只用来根据ID查找状态
	protected SafeList<CharacterState> mStateTickList = new();							// 以状态唯一ID为索引的列表,用来更新遍历,只存储需要调用update的状态
	protected Dictionary<Type, int> mGroupStateCountList = new();						// 存储每个状态组中拥有的状态数量,key是组类型,value是当前该组中拥有的状态数量
	protected Action mStateChangedCallback;												// 当状态改变时的回调,新增或者删除
	protected Character mCharacter;														// 状态机所属角色
	public override void init(ComponentOwner owner)
	{
		base.init(owner);
		mCharacter = owner as Character;
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mStateTypeList.clear();
		mStateMap.Clear();
		mStateTickList.clear();
		mGroupStateCountList.Clear();
		mStateChangedCallback = null;
		mCharacter = null;
	}
	public void clearState()
	{
		// 先退出所有状态
		using var a = new SafeDictionaryReader<Type, SafeList<CharacterState>>(mStateTypeList);
		foreach (var item in a.mReadList.Values)
		{
			using var b = new SafeListReader<CharacterState>(item);
			foreach (CharacterState state in b.mReadList)
			{
				leaveStateInternal(state, true, true);
			}
		}
		mStateTypeList.clear();
		mStateMap.Clear();
		mStateTickList.clear();
		mGroupStateCountList.Clear();
	}
	public override void destroy()
	{
		clearState();
		base.destroy();
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		using var b = new SafeListReader<CharacterState>(mStateTickList);
		foreach (CharacterState state in b.mReadList)
		{
			if (isDestroy() || !state.isActive() || state.isDestroy())
			{
				continue;
			}
			if (state.isJustEnter())
			{
				state.setJustEnter(false);
				continue;
			}
			state.update(state.isIgnoreTimeScale() ? Time.unscaledDeltaTime : elapsedTime);
		}
	}
	public override void fixedUpdate(float elapsedTime)
	{
		base.fixedUpdate(elapsedTime);
		using var a = new SafeListReader<CharacterState>(mStateTickList);
		foreach (CharacterState state in a.mReadList)
		{
			if (isDestroy() || !state.isActive() || state.isDestroy())
			{
				continue;
			}
			if (state.isJustEnter())
			{
				state.setJustEnter(false);
				continue;
			}
			state.fixedUpdate(elapsedTime);
		}
	}
	public T addStateIfNotExist<T>(StateParam param = null, float stateTime = -1.0f, long id = 0) where T : CharacterState
	{
		Type type = typeof(T);
		if (hasState(type))
		{
			return null;
		}
		return addState(type, param, stateTime, id) as T;
	}
	public CharacterState addStateIfNotExist(Type type, StateParam param = null, float stateTime = -1.0f, long id = 0)
	{
		if (hasState(type))
		{
			return null;
		}
		return addState(type, param, stateTime, id);
	}
	public T addState<T>(StateParam param = null, float stateTime = -1.0f, long id = 0) where T : CharacterState
	{
		return addState(typeof(T), param, stateTime, id) as T;
	}
	public CharacterState addState(Type type, StateParam param = null, float stateTime = -1.0f, long id = 0)
	{
		if (id > 0 && mStateMap.ContainsKey(id))
		{
			logWarning("不能重复添加状态,type:" + type + ", stateTime:" + stateTime + ", id:" + id + ", character:" + mComponentOwner.getName());
			return null;
		}
		CharacterState state = createState(type, param, id);
		state.setCharacter(mCharacter);

		if (stateTime >= 0.0f)
		{
			state.setStateMaxTime(stateTime);
			// 如果时间是0,则不再调整时间,因为会立即移除掉
			if (stateTime > 0.0f)
			{
				state.setStateTime(stateTime + (float)(DateTime.Now - mGameFrameworkHotFix.getFrameStartTime()).TotalSeconds);
			}
			else
			{
				state.setStateTime(stateTime);
			}
		}
		if (!canAddState(state))
		{
			destroyState(state);
			return null;
		}

		// 移除自身不可共存的状态
		CharacterState existState = getFirstState(type);
		if (existState != null)
		{
			mutexWithExistState(state, existState, out CharacterState needRemoveState);
			if (needRemoveState != null)
			{
				removeState(needRemoveState, true);
				if (existState == needRemoveState)
				{
					existState = null;
				}
			}
		}

		// 移除状态组互斥的状态
		List<CharacterState> tempList = null;
		foreach (var itemList in mStateTypeList.getMainList())
		{
			foreach (CharacterState item in itemList.Value.getMainList())
			{
				if (item != state && item.isActive() && !mStateManager.allowKeepStateByGroup(type, item.GetType()))
				{
					tempList ??= LIST<CharacterState>();
					tempList.Add(item);
				}
			}
		}
		foreach (CharacterState item in tempList.safe())
		{
			removeState(item, true);
		}
		UN_LIST(ref tempList);

		// 移除完状态以后,如果是叠加层数的buff,则需要通知当前buff有新的相同buff添加
		if (existState != null && existState.getMutexType() == STATE_MUTEX.OVERLAP_LAYER)
		{
			existState.addSameState(state);
			// 因为没有调用enter,所以只需要直接销毁
			destroyState(state);
			return null;
		}

		// 进入状态
		state.enter();
		state.setJustEnter(true);
		state.setParam(null);

		// 如果没有持续时间,则只执行一个enter,不存储起来
		if (isFloatZero(state.getStateTime()))
		{
			leaveStateInternal(state, false, false);
			return null;
		}

		// 添加到状态列表
		addStateToList(state);

		mStateChangedCallback?.Invoke();
		return state;
	}
	public bool removeFirstState<T>(bool isBreak, string param = null) where T : CharacterState
	{
		return removeState(getFirstState(typeof(T)), isBreak, param);
	}
	// 移除第一个指定类型的状态,isBreak表示是否是因为与要添加的状态冲突,所以才移除的状态
	public bool removeFirstState(Type type, bool isBreak, string param = null)
	{
		return removeState(getFirstState(type), isBreak, param);
	}
	// isBreak表示是否是因为与要添加的状态冲突,所以才移除的状态
	public bool removeState(CharacterState state, bool isBreak, string param = null)
	{
		if (state == null || !state.isActive())
		{
			return false;
		}
		// 从状态列表中移除
		if (!removeStateFromList(state))
		{
			return false;
		}
		leaveStateInternal(state, isBreak, false, param);
		return true;
	}
	// 移除一个状态组中的所有状态
	public void removeStateInGroup(Type group, bool isBreak, string param)
	{
		using var a = new ListScope<CharacterState>(out var tempList);
		foreach (var item in mStateTypeList.getMainList())
		{
			List<Type> groupList = mStateManager.getGroupList(item.Key);
			if (groupList != null && groupList.Contains(group))
			{
				tempList.AddRange(item.Value.getMainList());
			}
		}
		foreach (CharacterState item in tempList)
		{
			removeState(item, isBreak, param);
		}
	}
	public SafeDictionary<Type, SafeList<CharacterState>> getStateList() { return mStateTypeList; }
	public CharacterState getState(long instanceID) { return mStateMap.get(instanceID); }
	public SafeList<CharacterState> getState(Type type) { return mStateTypeList.get(type); }
	public SafeList<CharacterState> getState<T>() where T : CharacterState { return mStateTypeList.get(typeof(T)); }
	public T getFirstState<T>() where T : CharacterState { return getFirstState(typeof(T)) as T; }
	public CharacterState getFirstState(Type type)
	{
		foreach (CharacterState item in (mStateTypeList.get(type)?.getMainList()).safe())
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
		foreach (Type item in (mStateManager.getGroup(groupType)?.mStateList).safe())
		{
			var curStateList = getState(item);
			if (curStateList != null && !curStateList.getMainList().isEmpty())
			{
				return curStateList.getMainList()[0];
			}
		}
		return null;
	}
	public void getGroupState(Type groupType, List<CharacterState> stateList)
	{
		foreach (Type item in (mStateManager.getGroup(groupType)?.mStateList).safe())
		{
			stateList.AddRange((getState(item)?.getMainList()).safe());
		}
	}
	// 是否拥有该组的任意一个状态
	public bool hasStateGroup<T>() where T : StateGroup { return mGroupStateCountList.get(typeof(T)) > 0; }
	public bool hasStateGroup(Type group) { return mGroupStateCountList.get(group) > 0; }
	public bool hasState<T>() where T : CharacterState { return getFirstState(typeof(T)) != null; }
	public bool hasState(Type state) { return getFirstState(state) != null; }
	public void setStateChangedCallback(Action callback) { mStateChangedCallback = callback; }
	//------------------------------------------------------------------------------------------------------------------------------
	protected void leaveStateInternal(CharacterState state, bool isBreak, bool willDestroy, string param = null)
	{
		// 为了避免在一个状态中移除了其他状态,被移除的状态还可能继续更新的问题
		// 因为状态更新列表在遍历过程中是不会改变的,所以只能通过是否激活来准确判断状态是否有效
		state.setActive(false);
		// 通知即将移除此状态
		state.callWillRemoveCallback();
		state.leave(isBreak, willDestroy, param);
		// 销毁状态
		destroyState(state);

		mStateChangedCallback?.Invoke();
	}
	protected static CharacterState createState(Type type, StateParam param, long id = 0)
	{
		// 用对象池的方式创建状态对象
		var state = CLASS<CharacterState>(type);
		state.setParam(param);
		state.setID(id == 0 ? generateGUID() : id);
		return state;
	}
	protected void destroyState(CharacterState state)
	{
		UN_CLASS(ref state);
	}
	protected bool canAddState(CharacterState state)
	{
		Type stateType = state.GetType();
		if (!mutexWithExistState(state, getFirstState(stateType), out _))
		{
			return false;
		}
		// 检查是否有跟要添加的状态互斥且不能移除的状态
		foreach (var itemList in mStateTypeList.getMainList())
		{
			foreach (CharacterState item in itemList.Value.getMainList())
			{
				if (item != state && item.isActive() && !mStateManager.allowAddStateByGroup(stateType, item.GetType()))
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
	protected bool mutexWithExistState(CharacterState state, CharacterState existState, out CharacterState needRemoveState)
	{
		needRemoveState = null;
		// 移除自身不可共存的状态
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
		// 叠层数的状态允许添加,但是后续会在调用enter之前就销毁,仅仅只会通知已有的状态
		else if (operate == STATE_MUTEX.OVERLAP_LAYER)
		{
			return true;
		}
		return true;
	}
	protected void addStateToList(CharacterState state)
	{
		mStateTickList.add(state);
		Type type = state.GetType();
		if (!mStateTypeList.tryGetValue(type, out var stateList))
		{
			stateList = new();
			mStateTypeList.add(type, stateList);
		}
		stateList.add(state);
		mStateMap.Add(state.getID(), state);

		// 添加状态组计数
		foreach (Type item in mStateManager.getGroupList(type).safe())
		{
			mGroupStateCountList.addOrIncreaseValue(item, 1);
		}
	}
	protected bool removeStateFromList(CharacterState state)
	{
		if (!mStateMap.Remove(state.getID()))
		{
			return false;
		}
		mStateTickList.remove(state);
		Type type = state.GetType();
		mStateTypeList.tryGetValue(type, out var stateList);
		if (stateList == null)
		{
			logError("state type error:" + type.Name);
		}
		stateList?.remove(state);
		
		// 从状态组计数中移除
		foreach (Type item in mStateManager.getGroupList(type).safe())
		{
			mGroupStateCountList.addOrIncreaseValue(item, -1);
		}
		return true;
	}
}