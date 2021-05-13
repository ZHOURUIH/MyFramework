using System;
using System.Collections.Generic;

public class EventSystem : FrameSystem
{
	protected Dictionary<object, List<GameEventInfo>> mListenerList;		// 每个监听者所监听的所有事件信息列表
	protected Dictionary<int, List<GameEventInfo>> mListenerEventList;		// 每个事件对应的监听者列表
	public EventSystem()
	{
		mListenerList = new Dictionary<object, List<GameEventInfo>>();
		mListenerEventList = new Dictionary<int, List<GameEventInfo>>();
	}
	public void pushEvent(int eventType, GameEvent param)
	{
		if (mListenerEventList.TryGetValue(eventType, out List<GameEventInfo> infoList))
		{
			int infoCount = infoList.Count;
			for (int i = 0; i < infoCount; ++i)
			{
				infoList[i].mCallback(param);
			}
		}
		// 回收事件参数
		UN_CLASS(param);
	}
	public void listenEvent(int eventType, EventCallback callback, object listener)
	{
		CLASS_MAIN(out GameEventInfo info);
		info.mCallback = callback;
		info.mType = eventType;
		info.mLisntener = listener;
		if (!mListenerEventList.TryGetValue(eventType, out List<GameEventInfo> eventList))
		{
			eventList = new List<GameEventInfo>();
			mListenerEventList.Add(eventType, eventList);
		}
		eventList.Add(info);
		if (!mListenerList.TryGetValue(listener, out List<GameEventInfo> infoList))
		{
			infoList = new List<GameEventInfo>();
			mListenerList.Add(listener, infoList);
		}
		infoList.Add(info);
	}
	public void unlistenEvent(object listener)
	{
		if (!mListenerList.TryGetValue(listener, out List<GameEventInfo> infoList))
		{
			return;
		}
		int infoCount = infoList.Count;
		for(int i = 0; i < infoCount; ++i)
		{
			GameEventInfo info = infoList[i];
			if (!mListenerEventList.TryGetValue(info.mType, out List<GameEventInfo> eventList))
			{
				continue;
			}
			eventList.Remove(info);
			if (eventList.Count == 0)
			{
				mListenerEventList.Remove(info.mType);
			}
		}
		infoList.Clear();
		mListenerList.Remove(listener);
	}
}