using System;
using UnityEngine;
using UnityEngine.EventSystems;
using static TestAssert;

// myUGUIObject 交互链路深度测试(UGUI 事件手动模拟驱动)
//   setUGUIClick / setUGUIMouseEnter/Exit/Down/Up/Move/Stay
//   COMWindowUGUIInteractive 状态机: down 后才能 up / 重复 down 忽略 / 非当前触点 up 忽略
//   update 驱动 move(delta 非零)/stay(delta 零) / clearMousePointer
//   EventTriggerListener 8 事件直接调用(OnClick/Down/Up/Enter/Exit/Move/Select/UpdateSelected)
public static class MyUGUIObjectInteractiveDeepTest
{
	public static void Run()
	{
		testUGUIClick();
		testUGUIMouseEnterExit();
		testUGUIMouseDownUpStateMachine();
		testUGUIMouseMoveStay();
		testClearMousePointer();
		testEventTriggerAllEvents();
		testOnScreenTouchUpStorage();
	}

	// ═════════════════════════════════════════════════════════════════
	// 辅助
	// ═════════════════════════════════════════════════════════════════
	private static myUGUIObject createUI(out GameObject go)
	{
		go = new GameObject("Interactive");
		myUGUIObject ui = new myUGUIObject();
		ui.setObject(go);
		ui.init();
		return ui;
	}

	// setUGUIClick → checkEventTrigger 自动加 EventTriggerListener → 手动 OnPointerClick 驱动全链路
	private static void testUGUIClick()
	{
		myUGUIObject ui = createUI(out GameObject go);
		try
		{
			int count = 0;
			PointerEventData received = null;
			GameObject receivedGo = null;
			ui.setUGUIClick((data, target) => { ++count; received = data; receivedGo = target; });
			EventTriggerListener listener = go.GetComponent<EventTriggerListener>();
			assertNotNull(listener, "setUGUIClick 自动添加 EventTriggerListener");
			PointerEventData data = new PointerEventData(null);
			listener.OnPointerClick(data);
			assertEqual(1, count, "OnPointerClick 触发点击回调");
			assertTrue(ReferenceEquals(data, received), "回调收到同一 PointerEventData");
			assertTrue(ReferenceEquals(go, receivedGo), "回调收到目标 GameObject");
			// 再次触发仍有效
			listener.OnPointerClick(data);
			assertEqual(2, count, "重复点击再次触发");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setUGUIMouseEnter/Exit: 手动 OnPointerEnter/OnPointerExit 驱动
	private static void testUGUIMouseEnterExit()
	{
		myUGUIObject ui = createUI(out GameObject go);
		try
		{
			int enterCount = 0;
			int exitCount = 0;
			ui.setUGUIMouseEnter((data, target) => { ++enterCount; });
			ui.setUGUIMouseExit((data, target) => { ++exitCount; });
			EventTriggerListener listener = go.GetComponent<EventTriggerListener>();
			listener.OnPointerEnter(new PointerEventData(null));
			listener.OnPointerExit(new PointerEventData(null));
			assertEqual(1, enterCount, "OnPointerEnter 触发进入回调");
			assertEqual(1, exitCount, "OnPointerExit 触发离开回调");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setUGUIMouseDown/Up 状态机: down 后才能 up / 重复 down 忽略 / 非当前触点 up 忽略
	private static void testUGUIMouseDownUpStateMachine()
	{
		myUGUIObject ui = createUI(out GameObject go);
		try
		{
			int downCount = 0;
			int upCount = 0;
			ui.setUGUIMouseDown((data, target) => { ++downCount; });
			ui.setUGUIMouseUp((data, target) => { ++upCount; });
			EventTriggerListener listener = go.GetComponent<EventTriggerListener>();
			PointerEventData pointerA = new PointerEventData(null);
			PointerEventData pointerB = new PointerEventData(null);
			listener.OnPointerDown(pointerA);
			assertEqual(1, downCount, "OnPointerDown 触发按下回调");
			listener.OnPointerDown(pointerA);   // mMousePointer 已占用 → 忽略
			assertEqual(1, downCount, "重复按下忽略");
			listener.OnPointerUp(pointerB);     // 非当前触点 → 忽略
			assertEqual(0, upCount, "非当前触点抬起忽略");
			listener.OnPointerUp(pointerA);     // 当前触点抬起 → 触发
			assertEqual(1, upCount, "当前触点抬起触发");
			listener.OnPointerUp(pointerA);     // 已抬起(mMousePointer null) → 忽略
			assertEqual(1, upCount, "重复抬起忽略");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setUGUIMouseMove/Stay: 按下后 update 按 delta 判断驱动 move 或 stay
	private static void testUGUIMouseMoveStay()
	{
		myUGUIObject ui = createUI(out GameObject go);
		try
		{
			int moveCount = 0;
			int stayCount = 0;
			Vector2 lastDelta = Vector2.zero;
			Vector3 lastPos = Vector3.zero;
			ui.setUGUIMouseMove((delta, pos) => { ++moveCount; lastDelta = delta; lastPos = pos; });
			ui.setUGUIMouseStay((pos) => { ++stayCount; lastPos = pos; });
			EventTriggerListener listener = go.GetComponent<EventTriggerListener>();
			PointerEventData pointer = new PointerEventData(null);
			listener.OnPointerDown(pointer);    // 建立 mMousePointer
			// delta 非零 → move
			pointer.delta = new Vector2(3.0f, 0.0f);
			pointer.position = new Vector2(100.0f, 200.0f);
			ui.update(0.01f);
			assertEqual(1, moveCount, "delta 非零驱动 move");
			assertEqual(0, stayCount, "move 时不触发 stay");
			assertEqual(3.0f, lastDelta.x, 0.001f, "move 回调收到 delta");
			assertEqual(100.0f, lastPos.x, 0.001f, "move 回调收到位置");
			// delta 零 → stay
			pointer.delta = Vector2.zero;
			ui.update(0.01f);
			assertEqual(1, stayCount, "delta 零驱动 stay");
			assertEqual(100.0f, lastPos.x, 0.001f, "stay 回调收到位置");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// clearMousePointer: 清除后 update 不再驱动 move/stay
	private static void testClearMousePointer()
	{
		myUGUIObject ui = createUI(out GameObject go);
		try
		{
			int moveCount = 0;
			ui.setUGUIMouseMove((delta, pos) => { ++moveCount; });
			EventTriggerListener listener = go.GetComponent<EventTriggerListener>();
			PointerEventData pointer = new PointerEventData(null);
			listener.OnPointerDown(pointer);
			pointer.delta = new Vector2(1.0f, 0.0f);
			COMWindowUGUIInteractive com = ui.getComponent<COMWindowUGUIInteractive>();
			assertNotNull(com, "getComponent 拿到交互组件");
			com.clearMousePointer();
			ui.update(0.01f);
			assertEqual(0, moveCount, "clearMousePointer 后 update 不驱动");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// EventTriggerListener 剩余 3 个事件: OnMove/OnSelect/OnUpdateSelected(字段直接赋值)
	private static void testEventTriggerAllEvents()
	{
		myUGUIObject ui = createUI(out GameObject go);
		try
		{
			int moveEvent = 0;
			int selectEvent = 0;
			int updateSelectEvent = 0;
			ui.setUGUIClick(null);   // 触发 checkEventTrigger 添加监听器
			EventTriggerListener listener = go.GetComponent<EventTriggerListener>();
			listener.mOnMove = (data, target) => { ++moveEvent; };
			listener.mOnSelect = (data, target) => { ++selectEvent; };
			listener.mOnUpdateSelect = (data, target) => { ++updateSelectEvent; };
			listener.OnMove(new AxisEventData(null));
			listener.OnSelect(new BaseEventData(null));
			listener.OnUpdateSelected(new BaseEventData(null));
			assertEqual(1, moveEvent, "OnMove 触发");
			assertEqual(1, selectEvent, "OnSelect 触发");
			assertEqual(1, updateSelectEvent, "OnUpdateSelected 触发");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setOnScreenTouchUp 存储读回 + isReceiveScreenMouse(经 ComponentInteractive 组件)
	private static void testOnScreenTouchUpStorage()
	{
		myUGUIObject ui = createUI(out GameObject go);
		try
		{
			Vector3IntCallback callback = (pos, id) => { };
			ui.setOnScreenTouchUp(callback);
			ComponentInteractive com = ui.getComponent<ComponentInteractive>();
			assertNotNull(com, "setOnScreenTouchUp 后组件已创建");
			assertTrue(ReferenceEquals(callback, com.getOnScreenTouchUp()), "setOnScreenTouchUp 读回同一回调");
			assertTrue(com.isReceiveScreenMouse(), "设置回调后 isReceiveScreenMouse true");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}
}
