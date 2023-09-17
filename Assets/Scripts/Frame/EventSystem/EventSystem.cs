using System;
using System.Collections.Generic;
using static FrameUtility;

// 事件管理器,用于分发所有的事件
public class EventSystem : FrameSystem
{
	protected Dictionary<object, List<GameEventRegisteInfo>> mListenerList;			// 每个监听者所监听的所有事件信息列表
	protected Dictionary<int, List<GameEventRegisteInfo>> mListenerEventList;		// 每个事件对应的监听者列表
	public EventSystem()
	{
		mListenerList = new Dictionary<object, List<GameEventRegisteInfo>>();
		mListenerEventList = new Dictionary<int, List<GameEventRegisteInfo>>();
	}
	public void pushEvent(int eventType, GameEvent param = null)
	{
		if (mListenerEventList.TryGetValue(eventType, out List<GameEventRegisteInfo> infoList))
		{
			int infoCount = infoList.Count;
			for (int i = 0; i < infoCount; ++i)
			{
				infoList[i].mCallback(param);
			}
		}
	}
	public void listenEvent(int eventType, EventCallback callback, object listener)
	{
		CLASS(out GameEventRegisteInfo info);
		info.mCallback = callback;
		info.mEventType = eventType;
		info.mLisntener = listener;
		if (!mListenerEventList.TryGetValue(eventType, out List<GameEventRegisteInfo> eventList))
		{
			eventList = new List<GameEventRegisteInfo>();
			mListenerEventList.Add(eventType, eventList);
		}
		eventList.Add(info);
		if (!mListenerList.TryGetValue(listener, out List<GameEventRegisteInfo> infoList))
		{
			infoList = new List<GameEventRegisteInfo>();
			mListenerList.Add(listener, infoList);
		}
		infoList.Add(info);
	}
	public void unlistenEvent(object listener)
	{
		if (!mListenerList.TryGetValue(listener, out List<GameEventRegisteInfo> infoList))
		{
			return;
		}
		int infoCount = infoList.Count;
		for(int i = 0; i < infoCount; ++i)
		{
			GameEventRegisteInfo info = infoList[i];
			if (!mListenerEventList.TryGetValue(info.mEventType, out List<GameEventRegisteInfo> eventList))
			{
				continue;
			}
			eventList.Remove(info);
			if (eventList.Count == 0)
			{
				mListenerEventList.Remove(info.mEventType);
			}
		}
		infoList.Clear();
		mListenerList.Remove(listener);
	}
	public void unlistenEvent(object listener, int eventType)
	{
		if (!mListenerList.TryGetValue(listener, out List<GameEventRegisteInfo> infoList))
		{
			return;
		}
		for (int i = infoList.Count - 1; i >= 0; --i)
		{
			GameEventRegisteInfo info = infoList[i];
			if (info.mEventType != eventType)
			{
				continue;
			}
			if (!mListenerEventList.TryGetValue(info.mEventType, out List<GameEventRegisteInfo> eventList))
			{
				continue;
			}
			eventList.Remove(info);
			if (eventList.Count == 0)
			{
				mListenerEventList.Remove(info.mEventType);
			}
			infoList.RemoveAt(i);
		}
		if (infoList.Count == 0)
		{
			mListenerList.Remove(listener);
		}
	}
}