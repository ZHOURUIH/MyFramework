using System;
using System.Collections.Generic;
using static FrameUtility;
using static UnityUtility;

// 事件管理器,用于分发所有的事件
public class EventSystem : FrameSystem
{
	protected Dictionary<long, Dictionary<int, SafeList0<GameEventRegisteInfo>>> mCharacterEventList = new();		// 指定角色的事件监听列表
	protected Dictionary<IEventListener, List<GameEventRegisteInfo>> mListenerList = new();							// 每个监听者所监听的所有事件信息列表
	protected Dictionary<int, SafeList0<GameEventRegisteInfo>> mGlobalListenerEventList = new();					// 全局事件监听列表
	protected bool mNeedCheckEmptyEvent;																			// 是否需要检测有没有空的角色事件监听列表
	protected int mDispatchDepth;
	protected const int MAX_DEPTH = 20;
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		if (!mNeedCheckEmptyEvent)
		{
			return;
		}
		// 检查所有的列表是否有空的,将空的列表移除,避免列表的长度一直增大
		using var a = new ListScope<int>(out var removeEvent);
		List<long> removeCharacterEvent = null;
		foreach (var itemCharacter in mCharacterEventList)
		{
			var characterListenerList = itemCharacter.Value;
			if (characterListenerList.isEmpty())
			{
				continue;
			}
			removeEvent.Clear();
			foreach (var itemEvent in characterListenerList)
			{
				removeEvent.addIf(itemEvent.Key, itemEvent.Value.isEmpty());
			}
			if (!removeEvent.isEmpty())
			{
				foreach (int item in removeEvent)
				{
					characterListenerList.Remove(item, out var removedList);
					UN_CLASS(ref removedList);
				}
				if (characterListenerList.isEmpty())
				{
					removeCharacterEvent ??= LIST<long>();
					removeCharacterEvent.Add(itemCharacter.Key);
				}
			}
		}
		if (removeCharacterEvent != null)
		{
			foreach (long item in removeCharacterEvent)
			{
				mCharacterEventList.Remove(item, out var removedList);
				UN_DIC(ref removedList);
			}
			UN_LIST(removeCharacterEvent);
		}
		mNeedCheckEmptyEvent = false;
	}
	public void pushEvent<T>(long characterID) where T : GameEvent, new()
	{
		using var a = new ClassScope<T>(out var param);
		pushEvent(param, characterID);
	}
	public void pushEvent<T>(T param, long characterID) where T : GameEvent
	{
		// 即使只是指定角色的事件,也会先广播全局监听
		pushEvent(param);

		// 发送给玩家的事件监听
		if (characterID == 0)
		{
			return;
		}
		if (mDispatchDepth > MAX_DEPTH)
		{
			logError("事件递归栈深度超过上限");
			return;
		}
		++mDispatchDepth;
		do
		{
			if (!mCharacterEventList.TryGetValue(characterID, out var eventTypeList))
			{
				break;
			}
			if (eventTypeList.isEmpty())
			{
				UN_DIC(ref eventTypeList);
				mCharacterEventList.Remove(characterID);
				break;
			}
			int eventType = TypeID<T>.ID;
			if (!eventTypeList.TryGetValue(eventType, out var eventList))
			{
				break;
			}
			if (eventList.count() == 0)
			{
				UN_CLASS(ref eventList);
				eventTypeList.Remove(eventType);
				break;
			}
			// 由于在遍历过程中有可能会再次修改或者遍历此列表,所以这里的遍历次数是固定的,因为遍历中删除时不会真正移除元素,新增元素也只会添加到末尾
			// 所以就不能写成for (int i = 0; i < eventList.count(); ++i)
			int count = eventList.count();
			for (int i = 0; i < count; ++i)
			{
				try 
				{
					eventList.get(i)?.call(param); 
				}
				catch (Exception e) 
				{
					logException(e); 
				}
			}
		} while (false);
		--mDispatchDepth;
	}
	public void pushEvent<T>() where T : GameEvent, new()
	{
		using var a = new ClassScope<T>(out var param);
		pushEvent(param);
	}
	public void pushEvent<T>(T param) where T : GameEvent
	{
		if (mDispatchDepth > MAX_DEPTH)
		{
			logError("事件递归栈深度超过上限");
			return;
		}
		++mDispatchDepth;
		// 发送到全局事件监听
		if (mGlobalListenerEventList.TryGetValue(TypeID<T>.ID, out var infoList))
		{
			// 由于在遍历过程中有可能会再次修改或者遍历此列表,所以这里的遍历次数是固定的,因为遍历中删除时不会真正移除元素,新增元素也只会添加到末尾
			// 所以就不能写成for (int i = 0; i < infoList.count(); ++i)
			int count = infoList.count();
			for (int i = 0; i < count; ++i)
			{
				try 
				{
					infoList.get(i)?.call(param); 
				}
				catch (Exception e) 
				{
					logException(e); 
				}
			}
		}
		--mDispatchDepth;
	}
	public void listenEvent(int eventTypeID, Action callback, IEventListener listener)
	{
		GameEventRegisteInfo info = createEventAddToListenList(eventTypeID, 0, callback, listener);

		// 加入全局事件监听列表中
		mGlobalListenerEventList.getOrAddClass(info.mEventTypeID).add(info);
	}
	public void listenEvent<T>(Action callback, IEventListener listener)
	{
		GameEventRegisteInfo info = createEventAddToListenList(TypeID<T>.ID, 0, callback, listener);

		// 加入全局事件监听列表中
		mGlobalListenerEventList.getOrAddClass(info.mEventTypeID).add(info);
	}
	public void listenEvent<T>(Action<T> callback, IEventListener listener) where T : GameEvent
	{
		GameEventRegisteInfo info = createEventAddToListenList(0, callback, listener);

		// 加入全局事件监听列表中
		mGlobalListenerEventList.getOrAddClass(info.mEventTypeID).add(info);
	}
	public void listenEvent<T>(long characterID, Action<T> callback, IEventListener listener) where T : GameEvent
	{
		GameEventRegisteInfo info = createEventAddToListenList(characterID, callback, listener);

		// 加入指定角色事件监听列表中
		var characterEventList = mCharacterEventList.getOrAddListPersist(characterID);
		characterEventList.getOrAddClass(info.mEventTypeID).add(info);
	}
	public void listenEvent<T>(long characterID, Action callback, IEventListener listener) where T : GameEvent
	{
		GameEventRegisteInfo info = createEventAddToListenList(TypeID<T>.ID, characterID, callback, listener);

		// 加入指定角色事件监听列表中
		var characterEventList = mCharacterEventList.getOrAddListPersist(characterID);
		characterEventList.getOrAddClass(info.mEventTypeID).add(info);
	}
	public void unlistenEvent(IEventListener listener)
	{
		if (!mListenerList.TryGetValue(listener, out var infoList))
		{
			return;
		}

		// 从指定玩家事件监听列表和全局事件监听列表中移除
		foreach (GameEventRegisteInfo item in infoList)
		{
			removeFromCharacterListenList(item);
			removeFromGlobalListenList(item);
		}
		infoList.Clear();
		mListenerList.Remove(listener, out var removedList);
		UN_LIST(ref removedList);
	}
	public void unlistenEvent<T>(IEventListener listener) where T : GameEvent
	{
		if (!mListenerList.TryGetValue(listener, out var infoList))
		{
			return;
		}
		int eventType = TypeID<T>.ID;
		for (int i = 0; i < infoList.Count; ++i)
		{
			GameEventRegisteInfo info = infoList[i];
			if (info.mEventTypeID != eventType)
			{
				continue;
			}
			// 从指定玩家事件监听列表和全局事件监听列表中移除
			removeFromCharacterListenList(info);
			removeFromGlobalListenList(info);
			infoList.RemoveAt(i--);
		}
		if (infoList.Count == 0)
		{
			mListenerList.Remove(listener, out var removedList);
			UN_LIST(ref removedList);
		}
	}
	public void removeCharacterEvent(long characterID)
	{
		if (!mCharacterEventList.TryGetValue(characterID, out var eventTypeList))
		{
			return;
		}
		foreach (var item in eventTypeList)
		{
			var list = item.Value;
			// 从监听者查找列表中移除
			// 由于在遍历过程中有可能会再次修改或者遍历此列表,所以这里的遍历次数是固定的,因为遍历中删除时不会真正移除元素,新增元素也只会添加到末尾
			// 所以就不能写成for (int i = 0; i < list.count(); ++i)
			int count = list.count();
			for (int i = 0; i < count; ++i)
			{
				GameEventRegisteInfo eventInfo = list.get(i);
				if (!mListenerList.TryGetValue(eventInfo.mListener, out var listenList))
				{
					continue;
				}
				listenList.Remove(eventInfo);
				if (listenList.Count == 0)
				{
					UN_LIST(ref listenList);
					mListenerList.Remove(eventInfo.mListener);
				}
			}
			UN_CLASS_LIST(list.getMainList());
			list.clear();
		}
		mNeedCheckEmptyEvent = true;
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected GameEventRegisteInfo createEventAddToListenList<T>(long characterID, Action<T> callback, IEventListener listener) where T : GameEvent
	{
		CLASS(out GameEventRegisteInfoT<T> info);
		info.mEventTypeID = TypeID<T>.ID;
		info.mCharacterID = characterID;
		info.mCallback = callback;
		info.mListener = listener;
		return mListenerList.getOrAddListPersist(listener).add(info);
	}
	protected GameEventRegisteInfo createEventAddToListenList(int eventTypeID, long characterID, Action callback, IEventListener listener)
	{
		CLASS(out GameEventRegisteInfo info);
		info.mEventTypeID = eventTypeID;
		info.mCharacterID = characterID;
		info.mBaseCallback = callback;
		info.mListener = listener;
		return mListenerList.getOrAddListPersist(listener).add(info);
	}
	protected void removeFromCharacterListenList(GameEventRegisteInfo info)
	{
		if (info.mCharacterID == 0 || 
			!mCharacterEventList.TryGetValue(info.mCharacterID, out var characterEventList) ||
			!characterEventList.TryGetValue(info.mEventTypeID, out var eventList))
		{
			return;
		}
		eventList.remove(info);
		if (eventList.count() == 0)
		{
			if (!eventList.isForeaching())
			{
				characterEventList.Remove(info.mEventTypeID, out var removedList0);
				UN_CLASS(ref removedList0);
				if (characterEventList.Count == 0)
				{
					mCharacterEventList.Remove(info.mCharacterID, out var removedList1);
					UN_DIC(ref removedList1);
				}
			}
			else
			{
				mNeedCheckEmptyEvent = true;
			}
		}
	}
	protected void removeFromGlobalListenList(GameEventRegisteInfo info)
	{
		if (!mGlobalListenerEventList.TryGetValue(info.mEventTypeID, out var eventList))
		{
			return;
		}
		eventList.remove(info);
		if (eventList.count() == 0 && mGlobalListenerEventList.Remove(info.mEventTypeID, out var removedList))
		{
			UN_CLASS(ref removedList);
		}
	}
}