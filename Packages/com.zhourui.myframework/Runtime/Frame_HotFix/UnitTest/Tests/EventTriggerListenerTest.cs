using UnityEngine;
using UnityEngine.EventSystems;
using static TestAssert;

// EventTriggerListener 深度测试
// UGUI 鼠标事件监听脚本, 继承 Unity EventTrigger, 把各事件转发给对应 Action 回调
// 测试环境: 裸 GameObject + AddComponent<EventTriggerListener>
// 触发方式: 直接手动调用 OnPointerClick/OnPointerDown/... 等方法(不依赖真实 EventSystem)
// 清理: 测试自己 new 的裸 GameObject, 手动 DestroyImmediate
public static class EventTriggerListenerTest
{
	public static void Run()
	{
		testPointerClickCallback();
		testPointerDownUpCallbacks();
		testPointerEnterExitCallbacks();
		testMoveSelectCallbacks();
		testNullCallbackSafety();
	}

	// ═════════════════════════════════════════════════════════════════
	// 辅助: 创建监听组件
	// ═════════════════════════════════════════════════════════════════
	private static EventTriggerListener createListener(out GameObject go)
	{
		go = new GameObject("EventListenerGO");
		return go.AddComponent<EventTriggerListener>();
	}

	// OnPointerClick → mOnClick 回调(收到 eventData + gameObject)
	private static void testPointerClickCallback()
	{
		EventTriggerListener listener = createListener(out GameObject go);
		try
		{
			PointerEventData data = new PointerEventData(null);
			int count = 0;
			PointerEventData receivedData = null;
			GameObject receivedGo = null;
			listener.mOnClick = (d, g) => { ++count; receivedData = d; receivedGo = g; };
			listener.OnPointerClick(data);
			assertEqual(1, count, "OnPointerClick 触发 mOnClick");
			assertTrue(ReferenceEquals(data, receivedData), "回调收到同一个 PointerEventData");
			assertTrue(ReferenceEquals(go, receivedGo), "回调收到监听者的 gameObject");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// OnPointerDown / OnPointerUp → mOnDown / mOnUp
	private static void testPointerDownUpCallbacks()
	{
		EventTriggerListener listener = createListener(out GameObject go);
		try
		{
			int downCount = 0;
			int upCount = 0;
			listener.mOnDown = (d, g) => ++downCount;
			listener.mOnUp = (d, g) => ++upCount;
			PointerEventData data = new PointerEventData(null);
			listener.OnPointerDown(data);
			listener.OnPointerUp(data);
			listener.OnPointerDown(data);
			assertEqual(2, downCount, "按下 2 次触发 2 次");
			assertEqual(1, upCount, "抬起 1 次触发 1 次");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// OnPointerEnter / OnPointerExit → mOnEnter / mOnExit
	private static void testPointerEnterExitCallbacks()
	{
		EventTriggerListener listener = createListener(out GameObject go);
		try
		{
			int enterCount = 0;
			int exitCount = 0;
			listener.mOnEnter = (d, g) => ++enterCount;
			listener.mOnExit = (d, g) => ++exitCount;
			PointerEventData data = new PointerEventData(null);
			listener.OnPointerEnter(data);
			listener.OnPointerExit(data);
			assertEqual(1, enterCount, "进入触发 1 次");
			assertEqual(1, exitCount, "离开触发 1 次");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// OnMove / OnSelect / OnUpdateSelected → mOnMove / mOnSelect / mOnUpdateSelect
	private static void testMoveSelectCallbacks()
	{
		EventTriggerListener listener = createListener(out GameObject go);
		try
		{
			int moveCount = 0;
			int selectCount = 0;
			int updateSelectCount = 0;
			listener.mOnMove = (d, g) => ++moveCount;
			listener.mOnSelect = (d, g) => ++selectCount;
			listener.mOnUpdateSelect = (d, g) => ++updateSelectCount;
			listener.OnMove(new AxisEventData(null));
			listener.OnSelect(new BaseEventData(null));
			listener.OnUpdateSelected(new BaseEventData(null));
			assertEqual(1, moveCount, "OnMove 触发 1 次");
			assertEqual(1, selectCount, "OnSelect 触发 1 次");
			assertEqual(1, updateSelectCount, "OnUpdateSelected 触发 1 次");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// 未注册回调时调用各事件方法不崩溃
	private static void testNullCallbackSafety()
	{
		EventTriggerListener listener = createListener(out GameObject go);
		try
		{
			// 全部回调未注册, 直接调用不崩
			listener.OnPointerClick(new PointerEventData(null));
			listener.OnPointerDown(new PointerEventData(null));
			listener.OnPointerUp(new PointerEventData(null));
			listener.OnPointerEnter(new PointerEventData(null));
			listener.OnPointerExit(new PointerEventData(null));
			listener.OnMove(new AxisEventData(null));
			listener.OnSelect(new BaseEventData(null));
			listener.OnUpdateSelected(new BaseEventData(null));
			// 注册后回调正常
			bool called = false;
			listener.mOnClick = (d, g) => called = true;
			listener.OnPointerClick(new PointerEventData(null));
			assertTrue(called, "注册后点击回调可触发");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}
}
