using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public struct CacheState
{
	public Type mClassType;
	public List<PlayerState> mStateList;
}

public class CharacterStateMachine : GameComponent
{
	protected static Dictionary<Type, List<Type>> mStateGroupList;  // 查找状态所在的所有组,key为状态类型,value为该状态所在的所有状态组
	protected static Dictionary<Type, StateGroup> mGroupStateList;  // 查找该组中的所有状态
	protected Dictionary<uint, PlayerState> mCurStateIDMap;         // 以ID为索引的当前最新的状态列表,当状态添加删除时立即修改该列表
	protected Dictionary<Type, List<PlayerState>> mCurStateMap;		// 当前最新的状态列表,当状态添加删除时立即修改该列表
	protected List<PlayerState> mCurStateList;						// 当前最新的状态列表,当状态添加删除时立即修改该列表
	protected List<CacheState> mCacheStateList;                     // 缓存状态列表,只在当前帧更新前从最新列表同步到此列表,用于更新
	protected Dictionary<Type, CacheState> mCacheStateMap;          // 缓存状态查询列表,只用于辅助mCacheStateList查询
	protected List<CacheState> mCacheAddList;                       // 用于收集状态添加时的顺序,在同步缓存时合并到缓存列表中
	protected Character mPlayer;        // 状态机所属角色
	protected string mLockFunction;     // 当前是否有函数在遍历mCurStateIDMap,如果有则表示函数名
	protected bool mCacheDirty;         // 缓冲列表mCacheStateList是否需要同步
	public CharacterStateMachine()
	{
		mCurStateIDMap = new Dictionary<uint, PlayerState>();
		mCurStateMap = new Dictionary<Type, List<PlayerState>>();
		mCurStateList = new List<PlayerState>();
		mCacheStateList = new List<CacheState>();
		mCacheStateMap = new Dictionary<Type, CacheState>();
		mCacheAddList = new List<CacheState>();
		mCacheDirty = true;
	}
	public override void init(ComponentOwner owner)
	{
		base.init(owner);
		mPlayer = owner as Character;
	}
	public override void destroy()
	{
		// 移除全部状态
		clearState();
		base.destroy();
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		// 移除所有未激活的状态
		for (int i = 0; i < mCurStateList.Count; ++i)
		{
			if(!mCurStateList[i].isActive())
			{
				PlayerState state = mCurStateList[i];
				removeStateFromList(state);
				destroyState(state);
				--i;
			}
		}
		// 只有myself才能响应按键
		bool processKey = mPlayer is ICharacterMyself;
		// 同步状态到缓存列表中
		syncToCache();
		// 遍历缓存列表
		foreach (var itemList in mCacheStateList)
		{
			foreach(var item in itemList.mStateList)
			{
				if (!item.isActive())
				{
					continue;
				}
				float frameTime = item.isIgnoreTimeScale() ? Time.unscaledDeltaTime : elapsedTime;
				item.update(frameTime);
				if (processKey)
				{
					item.keyProcess(frameTime);
				}
			}
		}
	}
	public override void fixedUpdate(float elapsedTime)
	{
		base.fixedUpdate(elapsedTime);
		// 同步状态到缓存列表中
		syncToCache();
		// 遍历缓存列表
		foreach (var itemList in mCacheStateList)
		{
			foreach (var item in itemList.mStateList)
			{
				if (item.isActive())
				{
					item.fixedUpdate(elapsedTime);
				}
			}
		}
	}
	public bool addState<T>(StateParam param, out PlayerState outState, uint id = 0) where T : PlayerState
	{
		return addState(typeof(T), param, out outState);
	}
	public bool addState(Type type, StateParam param, out PlayerState state, uint id = 0)
	{
		state = createState(type, param, id);
		state.setPlayer(mPlayer);
		if(!canAddState(state))
		{
			destroyState(state);
			return false;
		}
		// 移除不能共存的状态
		removeMutexState(state);
		// 进入状态,并添加到状态列表
		state.enter();
		addStateToList(type, state);
		// 通知角色有状态添加
		notifyStateChanged(state);
		return true;
	}
	// isBreak表示是否是因为与要添加的状态冲突,所以才移除的状态
	public bool removeState(PlayerState state, bool isBreak, string param)
	{
		if (!mCurStateMap.ContainsKey(state.GetType()))
		{
			return false;
		}
		// 移除状态并非真正从列表中移除,只是暂时先禁用状态,下一帧更新时才会真正移除
		state.setActive(false);
		state.leave(isBreak, param);
		// 通知角色有状态移除
		notifyStateChanged(state);
		return true;
	}
	// 移除一个状态组中的所有状态
	public void removeStateInGroup(Type group, bool isBreak, string param)
	{
		mLockFunction = "removeStateInGroup";
		foreach (var item in mCurStateIDMap)
		{
			var groupList = item.Value.getGroup();
			if(groupList != null && groupList.Contains(group))
			{
				removeState(item.Value, isBreak, param);
			}
		}
		mLockFunction = null;
	}
	public void setStateUpdateAfter(Type state, Type after)
	{
		// 同步到缓存列表,所以该函数不能在状态更新中调用
		syncToCache();
		int index0 = -1;
		int index1 = -1;
		int count = mCacheStateList.Count;
		for (int i = 0; i < count; ++i)
		{
			Type type = mCacheStateList[i].mClassType;
			if (type == state)
			{
				index0 = i;
				// 两个下标都找到了就不用再遍历了
				if(index1 != -1)
				{
					break;
				}
			}
			else if(type == after)
			{
				index1 = i;
				// 两个下标都找到了就不用再遍历了
				if (index0 != -1)
				{
					break;
				}
			}
		}
		if(index0 != -1 && index1 != -1 && index0 < index1)
		{
			// 交换两个类型的状态在列表中的位置
			CacheState item0 = mCacheStateList[index0];
			mCacheStateList[index0] = mCacheStateList[index1];
			mCacheStateList[index1] = item0;
		}
	}
	public void clearState()
	{
		mLockFunction = "clearState";
		// 遍历当前列表,待添加列表,退出所有状态,清空列表,通知角色
		foreach (var item in mCurStateIDMap)
		{
			item.Value.leave(true, CommonDefine.DESTROY_PLAYER_STATE);
			item.Value.setActive(false);
			destroyState(item.Value);
		}
		mLockFunction = null;
		mCurStateIDMap.Clear();
		mCurStateMap.Clear();
		mCurStateList.Clear();
		mCacheDirty = true;
		notifyStateChanged(null);
	}
	public Dictionary<Type, List<PlayerState>> getStateList() { return mCurStateMap; }
	public List<CacheState> getCacheStateList() { return mCacheStateList; }
	public T getState<T>(uint id) where T : PlayerState
	{
		if(mCurStateIDMap.ContainsKey(id))
		{
			return mCurStateIDMap[id] as T;
		}
		return null;
	}
	public List<PlayerState> getState<T>() where T : PlayerState
	{
		Type type = typeof(T);
		if(mCurStateMap.ContainsKey(type))
		{
			return mCurStateMap[type];
		}
		return null;
	}
	public List<PlayerState> getState(Type type)
	{
		if (mCurStateMap.ContainsKey(type))
		{
			return mCurStateMap[type];
		}
		return null;
	}
	public T getFirstState<T>() where T : PlayerState
	{
		return getFirstState(typeof(T)) as T;
	}
	public PlayerState getFirstState(Type type)
	{
		if (!mCurStateMap.ContainsKey(type))
		{
			return null;
		}
		var itemList = mCurStateMap[type];
		foreach (var item in itemList)
		{
			if (item.isActive())
			{
				return item;
			}
		}
		return null;
	}
	public void getGroupState<T>(List<PlayerState> stateList) where T : StateGroup
	{
		Type group = typeof(T);
		if (!mGroupStateList.ContainsKey(group))
		{
			return;
		}
		int count0 = mGroupStateList[group].mStateList.Count;
		for (int i = 0; i < count0; ++i)
		{
			var curStateList = getState(mGroupStateList[group].mStateList[i]);
			if(curStateList != null)
			{
				stateList.AddRange(curStateList);
			}
		}
	}
	// 是否拥有该组的任意一个状态
	public bool hasStateGroup<T>() where T : StateGroup
	{
		Type group = typeof(T);
		if (!mGroupStateList.ContainsKey(group))
		{
			return false;
		}
		int count0 = mGroupStateList[group].mStateList.Count;
		for (int i = 0; i < count0; ++i)
		{
			if (hasState(mGroupStateList[group].mStateList[i]))
			{
				return true;
			}
		}
		return false;
	}
	public bool hasState(Type state)
	{
		return getFirstState(state) != null;
	}
	// 是否拥有指定状态
	public bool hasState<T>() where T : PlayerState
	{
		return hasState(typeof(T));
	}
	public static void registeGroup<T>(GROUP_MUTEX_OPERATION mutex = GROUP_MUTEX_OPERATION.GMO_COEXIST) where T : StateGroup, new()
	{
		if (mGroupStateList == null)
		{
			mGroupStateList = new Dictionary<Type, StateGroup>();
		}
		StateGroup group = new T();
		group.setCoexist(mutex);
		mGroupStateList.Add(typeof(T), group);
	}
	public static void assignGroup<Group, State>(bool mainState = false) where Group : StateGroup where State : PlayerState
	{
		if(mStateGroupList == null)
		{
			mStateGroupList = new Dictionary<Type, List<Type>>();
		}
		Type group = typeof(Group);
		Type state = typeof(State);
		if (!mStateGroupList.ContainsKey(state))
		{
			mStateGroupList.Add(state, new List<Type>());
		}
		mStateGroupList[state].Add(group);
		if (mGroupStateList.ContainsKey(group))
		{
			mGroupStateList[group].addState<State>();
			if(mainState)
			{
				mGroupStateList[group].setMainState<State>();
			}
		}
	}
	//----------------------------------------------------------------------------------------------------------------
	// state表示当前是什么状态改变,null表示清空状态列表
	protected void notifyStateChanged(PlayerState state)
	{
		mPlayer.notifyStateChanged(state);
	}
	protected PlayerState createState(Type type, StateParam param, uint id = 0)
	{
		// 用对象池的方式创建状态对象
		IClassObject stateObject;
		mClassPool.newClass(out stateObject, type);
		PlayerState state = stateObject as PlayerState;
		state.setParam(param);
		if (mStateGroupList.ContainsKey(type))
		{
			state.setGroup(mStateGroupList[type]);
		}
		else
		{
			state.setGroup(null);
		}
		if(id != 0)
		{
			state.setID(id);
		}
		return state;
	}
	protected T createState<T>(StateParam param, uint id = 0) where T : PlayerState
	{
		return createState(typeof(T), param, id) as T;
	}
	protected void destroyState(PlayerState state)
	{
		mClassPool.destroyClass(state);
	}
	protected bool canAddState(PlayerState state)
	{
		PlayerState existState = getFirstState(state.GetType());
		// 已经存在有一个同样的状态,并且该状态不允许叠加,则移除已经存在的状态
		if (existState != null)
		{
			SAME_STATE_OPERATE operate = existState.allowSuperposition();
			// 移除旧状态
			if (operate == SAME_STATE_OPERATE.SSO_REMOVE_OLD)
			{
				;
			}
			// 不允许添加新状态
			else if (operate == SAME_STATE_OPERATE.SSO_CAN_NOT_ADD_NEW)
			{
				return false;
			}
			// 相同的状态可以共存
			else if (operate == SAME_STATE_OPERATE.SSO_COEXIST)
			{
				;
			}
			// 只保留优先级最高的状态
			else if (operate == SAME_STATE_OPERATE.SSO_USE_HIGH_PRIORITY)
			{
				// 移除优先级低的旧状态
				if (existState.getPriority() >= state.getPriority())
				{
					return false;
				}
			}
		}
		// 检查是否有跟要添加的状态互斥且不能移除的状态
		foreach (var itemList in mCurStateMap)
		{
			foreach (var item in itemList.Value)
			{
				if (item != state && item.isActive() && !allowAddStateByGroup(state, item))
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
	protected void removeMutexState(PlayerState state)
	{
		// 移除自身不可共存的状态
		PlayerState existState = getFirstState(state.GetType());
		if(existState != null)
		{
			SAME_STATE_OPERATE operate = existState.allowSuperposition();
			// 移除旧状态
			if (operate == SAME_STATE_OPERATE.SSO_REMOVE_OLD)
			{
				removeState(existState, true, EMPTY_STRING);
			}
			// 不允许添加新状态
			else if (operate == SAME_STATE_OPERATE.SSO_CAN_NOT_ADD_NEW)
			{
				;
			}
			// 相同的状态可以共存
			else if (operate == SAME_STATE_OPERATE.SSO_COEXIST)
			{
				;
			}
			// 只保留优先级最高的状态
			else if (operate == SAME_STATE_OPERATE.SSO_USE_HIGH_PRIORITY)
			{
				// 移除优先级低的旧状态
				if (existState.getPriority() < state.getPriority())
				{
					removeState(existState, true, EMPTY_STRING);
				}
			}
		}
		// 移除状态组互斥的状态
		foreach (var itemList in mCurStateMap)
		{
			foreach (var item in itemList.Value)
			{
				if (item != state && item.isActive() && !allowKeepStateByGroup(state, item))
				{
					removeState(item, true, EMPTY_STRING);
				}
			}
		}
	}
	// 添加了newState以后是否还会保留existState
	protected bool allowKeepStateByGroup(PlayerState newState, PlayerState existState)
	{
		List<Type> newGroup = newState.getGroup();
		List<Type> existGroup = existState.getGroup();
		// 有一个状态不属于任何状态组时两个状态是可以共存的
		if(newGroup == null || existGroup == null)
		{
			return true;
		}
		int count0 = newGroup.Count;
		int count1 = existGroup.Count;
		for(int i = 0; i < count0; ++i)
		{
			for(int j = 0; j < count1; ++j)
			{
				if(newGroup[i] != existGroup[j])
				{
					continue;
				}
				StateGroup group = mGroupStateList[newGroup[i]];
				GROUP_MUTEX_OPERATION operate = group.mCoexist;
				// 状态可共存
				if (operate == GROUP_MUTEX_OPERATION.GMO_COEXIST)
				{
					;
				}
				// 只与主状态互斥
				else if (operate == GROUP_MUTEX_OPERATION.GMO_MUETX_WITH_MAIN)
				{
					// newState为主状态时不能保留其他状态
					if (group.mMainState == newState.GetType())
					{
						return false;
					}
				}
				// 不可添加新状态
				else if (operate == GROUP_MUTEX_OPERATION.GMO_NO_NEW)
				{
					return false;
				}
				// 添加新状态时移除所有旧状态
				else if (operate == GROUP_MUTEX_OPERATION.GMO_REMOVE_OTHERS)
				{
					return false;
				}
			}
		}
		return true;
	}
	// 当存在existState时,是否允许添加newState
	protected bool allowAddStateByGroup(PlayerState newState, PlayerState existState)
	{
		// 任意一个状态没有所属组,则不在同一组
		List<Type> newGroup = newState.getGroup();
		List<Type> existGroup = existState.getGroup();
		// 有一个状态不属于任何状态组时两个状态是可以共存的
		if (newGroup == null || existGroup == null)
		{
			return true;
		}
		int count0 = newGroup.Count;
		int count1 = existGroup.Count;
		for (int i = 0; i < count0; ++i)
		{
			for (int j = 0; j < count1; ++j)
			{
				// 属于同一状态组,并且该状态组中的所有状态都不能共存,而且不允许添加跟当前状态互斥的状态,则不允许添加该状态
				if (newGroup[i] != existGroup[j])
				{
					continue;
				}
				StateGroup group = mGroupStateList[newGroup[i]];
				GROUP_MUTEX_OPERATION operate = group.mCoexist;
				// 状态可共存
				if (operate == GROUP_MUTEX_OPERATION.GMO_COEXIST)
				{
					;
				}
				// 只与主状态互斥
				else if (operate == GROUP_MUTEX_OPERATION.GMO_MUETX_WITH_MAIN)
				{
					// 有主状态时不可添加其他状态
					if (group.mMainState == existState.GetType())
					{
						return false;
					}
				}
				// 不可添加新状态
				else if (operate == GROUP_MUTEX_OPERATION.GMO_NO_NEW)
				{
					return false;
				}
				// 添加新状态时移除所有旧状态
				else if (operate == GROUP_MUTEX_OPERATION.GMO_REMOVE_OTHERS)
				{
					;
				}
			}
		}
		return true;
	}
	protected void addStateToList(Type type, PlayerState state)
	{
		if (mLockFunction != null)
		{
			logError("state list is locked! LockFunction:" + mLockFunction);
			return;
		}
		if (!mCacheStateMap.ContainsKey(type))
		{
			CacheState cacheState = new CacheState();
			cacheState.mClassType = type;
			cacheState.mStateList = new List<PlayerState>();
			mCacheAddList.Add(cacheState);
			mCacheStateMap.Add(type, cacheState);
		}
		if (!mCurStateMap.ContainsKey(type))
		{
			mCurStateMap.Add(type, new List<PlayerState>());
		}
		mCurStateMap[type].Add(state);
		mCurStateIDMap.Add(state.getID(), state);
		mCurStateList.Add(state);
		mCacheDirty = true;
	}
	protected void removeStateFromList(PlayerState state)
	{
		if (mLockFunction != null)
		{
			logError("state list is locked! LockFunction:" + mLockFunction);
			return;
		}
		mCurStateIDMap.Remove(state.getID());
		if (mCurStateMap.ContainsKey(state.GetType()))
		{
			mCurStateMap[state.GetType()].Remove(state);
		}
		mCurStateList.Remove(state);
		mCacheDirty = true;
	}
	protected void syncToCache()
	{
		if (!mCacheDirty)
		{
			return;
		}
		foreach (var item in mCacheStateList)
		{
			item.mStateList.Clear();
		}
		mCacheStateList.AddRange(mCacheAddList);
		mCacheAddList.Clear();
		int stateCount = mCurStateList.Count;
		for (int i = 0; i < stateCount; ++i)
		{
			Type type = mCurStateList[i].GetType();
			mCacheStateMap[type].mStateList.Add(mCurStateList[i]);
		}
		mCacheDirty = false;
	}
}