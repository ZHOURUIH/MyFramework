using System;
using System.Collections.Generic;
using static FrameUtility;

// 事件管理器,用于分发所有的事件
public class EventSystem : FrameSystem
{
	protected Dictionary<long, Dictionary<Type, SafeDeepList<GameEventRegisteInfo>>> mCharacterEventList = new();	// 指定角色的事件监听列表
	protected Dictionary<IEventListener, List<GameEventRegisteInfo>> mListenerList = new();							// 每个监听者所监听的所有事件信息列表
	protected Dictionary<Type, SafeDeepList<GameEventRegisteInfo>> mGlobalListenerEventList = new();				// 全局事件监听列表
	protected bool mNeedCheckEmptyEvent;																			// 是否需要检测有没有空的角色事件监听列表
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		if (!mNeedCheckEmptyEvent)
		{
			return;
		}
		// 检查所有的列表是否有空的,将空的列表移除,避免列表的长度一直增大
		using var a = new ListScope<Type>(out var removeEvent);
		List<long> removeCharacterEvent = null;
		foreach (var itemCharacter in mCharacterEventList)
		{
			var characterListenerList = itemCharacter.Value;
			if (characterListenerList.Count == 0)
			{
				continue;
			}
			removeEvent.Clear();
			foreach (var itemEvent in characterListenerList)
			{
				if (itemEvent.Value.count() == 0)
				{
					removeEvent.Add(itemEvent.Key);
				}
			}
			if (removeEvent.Count > 0)
			{
				foreach (Type item in removeEvent)
				{
					characterListenerList.Remove(item, out var removedList);
					UN_CLASS(ref removedList);
				}
				if (characterListenerList.Count == 0)
				{
					LIST(out removeCharacterEvent);
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
		do
		{
			if (!mCharacterEventList.TryGetValue(characterID, out var eventTypeList))
			{
				break;
			}
			if (eventTypeList.Count == 0)
			{
				UN_DIC(ref eventTypeList);
				mCharacterEventList.Remove(characterID);
				break;
			}
			Type eventType = param.GetType();
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
			// 这里需要拷贝一份出来,否则在callback中可能还会调用pushEvent造成错误
			// 虽然可以使用DeepSafeList,但是DeppSafeList性能较差
			using var a = new SafeDeepListReader<GameEventRegisteInfo>(eventList);
			foreach (GameEventRegisteInfo item in a.mReadList)
			{
				item.call(param);
			}
		} while (false);
	}
	public void pushEvent<T>() where T : GameEvent, new()
	{
		using var a = new ClassScope<T>(out var param);
		pushEvent(param);
	}
	public void pushEvent<T>(T param) where T : GameEvent
	{
		// 发送到全局事件监听
		if (mGlobalListenerEventList.TryGetValue(param.GetType(), out var infoList))
		{
			using var a = new SafeDeepListReader<GameEventRegisteInfo>(infoList);
			foreach (GameEventRegisteInfo item in a.mReadList)
			{
				item.call(param);
			}
		}
	}
	public void listenEvent(Type eventType, Action callback, IEventListener listener)
	{
		GameEventRegisteInfo info = createEventAddToListenList(eventType, 0, callback, listener);

		// 加入全局事件监听列表中
		mGlobalListenerEventList.tryGetOrAddClass(info.mEventType).add(info);
	}
	public void listenEvent<T>(Action<T> callback, IEventListener listener) where T : GameEvent
	{
		GameEventRegisteInfo info = createEventAddToListenList(0, callback, listener);

		// 加入全局事件监听列表中
		mGlobalListenerEventList.tryGetOrAddClass(info.mEventType).add(info);
	}
	public void listenEvent<T>(long characterID, Action<T> callback, IEventListener listener) where T : GameEvent
	{
		GameEventRegisteInfo info = createEventAddToListenList(characterID, callback, listener);

		// 加入指定角色事件监听列表中
		var characterEventList = mCharacterEventList.tryGetOrAddListPersist(characterID);
		characterEventList.tryGetOrAddClass(info.mEventType).add(info);
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
		Type eventType = typeof(T);
		for (int i = 0; i < infoList.Count; ++i)
		{
			GameEventRegisteInfo info = infoList[i];
			if (info.mEventType != eventType)
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
		foreach (var item in eventTypeList.Values)
		{
			// 从监听者查找列表中移除
			using var a = new SafeDeepListReader<GameEventRegisteInfo>(item);
			foreach (GameEventRegisteInfo eventInfo in a.mReadList)
			{
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
			UN_CLASS_LIST(item.getMainList());
			item.clear();
		}
		mNeedCheckEmptyEvent = true;
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected GameEventRegisteInfo createEventAddToListenList<T>(long characterID, Action<T> callback, IEventListener listener) where T : GameEvent
	{
		CLASS(out GameEventRegisteInfoT<T> info);
		info.mEventType = typeof(T);
		info.mCharacterID = characterID;
		info.mCallback = callback;
		info.mListener = listener;
		return mListenerList.tryGetOrAddListPersist(listener).add(info);
	}
	protected GameEventRegisteInfo createEventAddToListenList(Type eventType, long characterID, Action callback, IEventListener listener)
	{
		CLASS(out GameEventRegisteInfo info);
		info.mEventType = eventType;
		info.mCharacterID = characterID;
		info.mBaseCallback = callback;
		info.mListener = listener;
		return mListenerList.tryGetOrAddListPersist(listener).add(info);
	}
	protected void removeFromCharacterListenList(GameEventRegisteInfo info)
	{
		if (info.mCharacterID == 0 || 
			!mCharacterEventList.TryGetValue(info.mCharacterID, out var characterEventList) ||
			!characterEventList.TryGetValue(info.mEventType, out var eventList))
		{
			return;
		}
		eventList.remove(info);
		if (eventList.count() == 0)
		{
			if (!eventList.isForeaching())
			{
				characterEventList.Remove(info.mEventType, out var removedList0);
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
		if (!mGlobalListenerEventList.TryGetValue(info.mEventType, out var eventList))
		{
			return;
		}
		eventList.remove(info);
		if (eventList.count() == 0 && mGlobalListenerEventList.Remove(info.mEventType, out var removedList))
		{
			UN_CLASS(ref removedList);
		}
	}
}