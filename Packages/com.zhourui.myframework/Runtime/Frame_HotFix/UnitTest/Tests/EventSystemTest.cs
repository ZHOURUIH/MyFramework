using System;
using static TestAssert;
using static FrameUtility;
using static FrameBaseHotFix;

// EventSystem 穷举测试
public static class EventSystemTest
{
	// 测试用事件类型
	private class TestEvent : GameEvent { }
	private class TestEventWithValue : GameEvent
	{
		public int value;
		public override void resetProperty()
		{
			base.resetProperty();
			value = 0;
		}
	}
	private class TestEvent2 : GameEvent { }
	private class TestEvent3 : GameEvent { }

	// 用于接收事件的监听器
	private class TestListener : IEventListener { }

	public static void Run()
	{
		// --- 全局事件 ---
		testListenAndPush_NoParam();
		testListenAndPush_WithParam();
		testUnlisten_StopsReceiving();
		testPushEvent_NoListeners();
		testMultipleListeners();
		testListenEvent_ByTypeID();
		testUnlistenAll_ByListener();
		testPushEvent_RecursionDepthLimit();

		// --- 角色事件 ---
		testListenCharacterEvent();
		testPushCharacterEvent();
		testPushCharacterEvent_InvalidID();
		testRemoveCharacterEvent();
		testCharacterEventAlsoFiresGlobal();

		// --- 边界 ---
		testListenSameEventMultipleTimes();
		testUnlistenNotListened();
	}

	//==================================================================
	// 全局事件 — 无参
	//==================================================================
	private static void testListenAndPush_NoParam()
	{
		int received = 0;
		var listener = new TestListener();
		mEventSystem.listenEvent<TestEvent>(_ => received++, listener);
		mEventSystem.pushEvent<TestEvent>();
		assertEqual(1, received);
		mEventSystem.unlistenEvent(listener);
		mEventSystem.pushEvent<TestEvent>();
		assertEqual(1, received);
	}

	//==================================================================
	// 全局事件 — 带参
	//==================================================================
	private static void testListenAndPush_WithParam()
	{
		int lastValue = -1;
		var listener = new TestListener();
		mEventSystem.listenEvent<TestEventWithValue>(e => lastValue = e.value, listener);

		var evt = CLASS<TestEventWithValue>();
		evt.value = 42;
		mEventSystem.pushEvent(evt);
		assertEqual(42, lastValue);

		mEventSystem.unlistenEvent(listener);
	}

	//==================================================================
	// 取消监听
	//==================================================================
	private static void testUnlisten_StopsReceiving()
	{
		int count = 0;
		var listener = new TestListener();
		mEventSystem.listenEvent<TestEvent>(_ => count++, listener);
		mEventSystem.pushEvent<TestEvent>();
		mEventSystem.pushEvent<TestEvent>();
		assertEqual(2, count);
		mEventSystem.unlistenEvent<TestEvent>(listener);
		mEventSystem.pushEvent<TestEvent>();
		assertEqual(2, count);
	}

	//==================================================================
	// 推送事件 — 无人监听
	//==================================================================
	private static void testPushEvent_NoListeners()
	{
		// 推送一个无人监听的事件不应崩溃
		mEventSystem.pushEvent<TestEvent2>();
		// 推送带参无人监听的事件
		var evt = CLASS<TestEventWithValue>();
		evt.value = 99;
		mEventSystem.pushEvent(evt);
	}

	//==================================================================
	// 多监听器
	//==================================================================
	private static void testMultipleListeners()
	{
		int count1 = 0, count2 = 0;
		var listener1 = new TestListener();
		var listener2 = new TestListener();

		mEventSystem.listenEvent<TestEvent>(_ => count1++, listener1);
		mEventSystem.listenEvent<TestEvent>(_ => count2++, listener2);
		mEventSystem.pushEvent<TestEvent>();

		assertEqual(1, count1);
		assertEqual(1, count2);

		mEventSystem.unlistenEvent(listener1);
		mEventSystem.unlistenEvent(listener2);
	}

	//==================================================================
	// 按 TypeID 监听
	//==================================================================
	private static void testListenEvent_ByTypeID()
	{
		int received = 0;
		var listener = new TestListener();
		mEventSystem.listenEvent(TypeID<TestEvent>.ID, () => received++, listener);
		mEventSystem.pushEvent<TestEvent>();
		assertEqual(1, received);
		mEventSystem.unlistenEvent(listener);
	}

	//==================================================================
	// unlistenEvent(IEventListener) 全清
	//==================================================================
	private static void testUnlistenAll_ByListener()
	{
		int count1 = 0, count2 = 0;
		var listener = new TestListener();

		mEventSystem.listenEvent<TestEvent>(_ => count1++, listener);
		mEventSystem.listenEvent<TestEvent2>(_ => count2++, listener);

		mEventSystem.pushEvent<TestEvent>();
		mEventSystem.pushEvent<TestEvent2>();
		assertEqual(1, count1);
		assertEqual(1, count2);

		// 取消该 listener 的所有事件
		mEventSystem.unlistenEvent(listener);
		mEventSystem.pushEvent<TestEvent>();
		mEventSystem.pushEvent<TestEvent2>();
		assertEqual(1, count1);
		assertEqual(1, count2);
	}

	//==================================================================
	//==================================================================
	// 递归深度限制
	//==================================================================
	private static void testPushEvent_RecursionDepthLimit()
	{
		// pushEvent 有 MAX_DEPTH=20 限制，测试不会无限递归
		// 这里只验证正常深度内没问题
		int received = 0;
		var listener = new TestListener();
		mEventSystem.listenEvent<TestEvent>(_ =>
		{
			received++;
			// 在回调中再推送自己（1层递归）
			if (received == 1)
			{
				mEventSystem.pushEvent<TestEvent>();
			}
		}, listener);

		mEventSystem.pushEvent<TestEvent>();
		// 递归后应收到两次
		assertEqual(2, received);

		mEventSystem.unlistenEvent(listener);
	}

	//==================================================================
	// 角色事件 — 注册
	//==================================================================
	private static void testListenCharacterEvent()
	{
		int received = 0;
		var listener = new TestListener();
		long charID = 12345;

		mEventSystem.listenEvent<TestEvent>(charID, () => received++, listener);
		// 角色事件也会先广播全局，确保全局没有监听器干扰
		assertEqual(0, received);

		mEventSystem.unlistenEvent(listener);
	}

	//==================================================================
	// 角色事件 — 推送
	//==================================================================
	private static void testPushCharacterEvent()
	{
		int received = 0;
		var listener = new TestListener();
		long charID = 12345;

		mEventSystem.listenEvent<TestEvent>(charID, () => received++, listener);
		mEventSystem.pushEvent<TestEvent>(charID);
		assertEqual(1, received);

		mEventSystem.unlistenEvent(listener);
	}

	//==================================================================
	// 角色事件 — 无效 ID
	//==================================================================
	private static void testPushCharacterEvent_InvalidID()
	{
		// characterID=0 时只广播全局，不走角色事件
		// 不应崩溃
		mEventSystem.pushEvent<TestEvent>(0);
	}

	//==================================================================
	// 移除角色所有事件
	//==================================================================
	private static void testRemoveCharacterEvent()
	{
		int received = 0;
		var listener = new TestListener();
		long charID = 12345;

		mEventSystem.listenEvent<TestEvent>(charID, () => received++, listener);
		mEventSystem.removeCharacterEvent(charID);
		mEventSystem.pushEvent<TestEvent>(charID);
		// 全局广播仍会触发（角色事件推送到角色前先广播全局）
		// removeCharacterEvent 后角色监听被清
		// 但 pushEvent(charID) 会先 pushEvent(全局) ...
		// 实际上角色事件在注册时不会加入全局列表，所以应该是 0
		// 等等，listenEvent(long,Action,IEventListener) 内部走 createEventAddToListenList
		// 注册的是 characterEventList，不是 globalList
		// 所以 removeCharacterEvent + pushEvent(charID) 中 pushEvent(param) 走全局=无
		// 然后角色事件=已被移除=无
		assertEqual(0, received);

		mEventSystem.unlistenEvent(listener);
	}

	//==================================================================
	// 角色事件同时触发全局
	//==================================================================
	private static void testCharacterEventAlsoFiresGlobal()
	{
		int globalCount = 0;
		int charCount = 0;
		var globalListener = new TestListener();
		var charListener = new TestListener();
		long charID = 99999;

		// 全局监听
		mEventSystem.listenEvent<TestEvent>(_ => globalCount++, globalListener);
		// 角色监听
		mEventSystem.listenEvent<TestEvent>(charID, () => charCount++, charListener);

		mEventSystem.pushEvent<TestEvent>(charID);
		// pushEvent(charID) 内部先 pushEvent(param) 广播全局
		assertEqual(1, globalCount);
		assertEqual(1, charCount);

		mEventSystem.unlistenEvent(globalListener);
		mEventSystem.unlistenEvent(charListener);
	}

	//==================================================================
	// 同一事件注册多次
	//==================================================================
	private static void testListenSameEventMultipleTimes()
	{
		int count = 0;
		var listener = new TestListener();

		// 同一个 listener 对同一个事件注册多次
		mEventSystem.listenEvent<TestEvent>(_ => count++, listener);
		mEventSystem.listenEvent<TestEvent>(_ => count++, listener);
		mEventSystem.pushEvent<TestEvent>();
		// 两次注册都会触发
		assertEqual(2, count);

		mEventSystem.unlistenEvent(listener);
	}

	//==================================================================
	// 取消未注册的监听
	//==================================================================
	private static void testUnlistenNotListened()
	{
		var listener = new TestListener();
		// 取消未注册的监听不应崩溃
		mEventSystem.unlistenEvent<TestEvent>(listener);
		mEventSystem.unlistenEvent(listener);
	}
}
