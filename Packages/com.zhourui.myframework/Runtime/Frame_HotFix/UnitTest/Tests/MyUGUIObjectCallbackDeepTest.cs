using System;
using UnityEngine;
using UnityEngine.EventSystems;
using static TestAssert;

// myUGUIObject 回调入口深度测试(GlobalTouchSystem 回调手动驱动):
//   onLayoutHide(清除触点) / onTouchStay / onScreenTouchUp / onScreenTouchDown
//   onTouchEnter/Leave(hover 状态机 + mOnTouchEnter/Leave 链路)
//   onTouchDown(触发 press/pressDetail/onTouchDown 全链路)
//   onReceiveDrag(ref continueEvent 置 false) / onDragHovered
//   剩余 detail 回调 setter 守卫式
public static class MyUGUIObjectCallbackDeepTest
{
	public static void Run()
	{
		testOnLayoutHideClearsPointer();
		testTouchStayLink();
		testScreenTouchUpLink();
		testScreenTouchDownLink();
		testHoverStateMachine();
		testTouchEnterLeaveLink();
		testTouchDownChain();
		testReceiveDragLink();
		testDragHoverLink();
		testDetailCallbackSetters();
		testSetOnTouchUpGuard();
	}

	// ═════════════════════════════════════════════════════════════════
	// 辅助
	// ═════════════════════════════════════════════════════════════════
	private static myUGUIObject createUI(out GameObject go)
	{
		go = new GameObject("Callback");
		myUGUIObject ui = new myUGUIObject();
		ui.setObject(go);
		ui.init();
		return ui;
	}

	// onLayoutHide: 清除触点 → update 不再驱动 move
	private static void testOnLayoutHideClearsPointer()
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
			ui.onLayoutHide();   // 布局隐藏 → clearMousePointer
			ui.update(0.01f);
			assertEqual(0, moveCount, "onLayoutHide 清除触点后 update 不驱动");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setOnTouchStay → onTouchStay 转发链路
	private static void testTouchStayLink()
	{
		myUGUIObject ui = createUI(out GameObject go);
		try
		{
			Vector3 got = Vector3.zero;
			int gotId = -1;
			ui.setOnTouchStay((pos, id) => { got = pos; gotId = id; });
			ui.onTouchStay(new Vector3(10.0f, 20.0f, 0.0f), 5);
			assertEqual(10.0f, got.x, 0.001f, "onTouchStay 转发位置 x");
			assertEqual(20.0f, got.y, 0.001f, "onTouchStay 转发位置 y");
			assertEqual(5, gotId, "onTouchStay 转发 touchID");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setOnScreenTouchUp → onScreenTouchUp 转发链路
	private static void testScreenTouchUpLink()
	{
		myUGUIObject ui = createUI(out GameObject go);
		try
		{
			bool received = false;
			int gotId = -1;
			ui.setOnScreenTouchUp((pos, id) => { received = true; gotId = id; });
			ui.onScreenTouchUp(new Vector3(1.0f, 2.0f, 0.0f), 3);
			assertTrue(received, "onScreenTouchUp 触发回调");
			assertEqual(3, gotId, "onScreenTouchUp 转发 touchID");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setOnScreenTouchDown(组件级) → onScreenTouchDown 转发链路
	private static void testScreenTouchDownLink()
	{
		myUGUIObject ui = createUI(out GameObject go);
		try
		{
			bool received = false;
			Vector3 got = Vector3.zero;
			// 先创建组件(setOnTouchStay 触发 getCOMInteractive)
			ui.setOnTouchStay((pos, id) => { });
			ComponentInteractive com = ui.getComponent<ComponentInteractive>();
			com.setOnScreenTouchDown((pos, id) => { received = true; got = pos; });
			ui.onScreenTouchDown(new Vector3(7.0f, 8.0f, 0.0f), 1);
			assertTrue(received, "onScreenTouchDown 触发回调");
			assertEqual(7.0f, got.x, 0.001f, "onScreenTouchDown 转发位置");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// onTouchEnter/Leave 的 hover 状态机(首次触发, 重复忽略)
	private static void testHoverStateMachine()
	{
		myUGUIObject ui = createUI(out GameObject go);
		try
		{
			int hoverTrue = 0;
			int hoverFalse = 0;
			ui.setHoverCallback((h) => { if (h) ++hoverTrue; else ++hoverFalse; });
			ui.onTouchEnter(new Vector3(1.0f, 1.0f, 0.0f), 1);
			assertEqual(1, hoverTrue, "首次 enter → hover(true)");
			ui.onTouchEnter(new Vector3(2.0f, 2.0f, 0.0f), 1);
			assertEqual(1, hoverTrue, "重复 enter 不再触发 hover");
			ui.onTouchLeave(new Vector3(1.0f, 1.0f, 0.0f), 1);
			assertEqual(1, hoverFalse, "leave → hover(false)");
			ui.onTouchLeave(new Vector3(1.0f, 1.0f, 0.0f), 1);
			assertEqual(1, hoverFalse, "重复 leave 不再触发 hover");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setOnTouchEnter/Leave → onTouchEnter/Leave 转发链路(每次调用都触发)
	private static void testTouchEnterLeaveLink()
	{
		myUGUIObject ui = createUI(out GameObject go);
		try
		{
			int enterCount = 0;
			int leaveCount = 0;
			ui.setOnTouchEnter((pos, id) => { ++enterCount; });
			ui.setOnTouchLeave((pos, id) => { ++leaveCount; });
			ui.onTouchEnter(new Vector3(1.0f, 1.0f, 0.0f), 2);
			ui.onTouchEnter(new Vector3(1.0f, 1.0f, 0.0f), 2);
			ui.onTouchLeave(new Vector3(1.0f, 1.0f, 0.0f), 2);
			assertEqual(2, enterCount, "每次 onTouchEnter 都触发 mOnTouchEnter");
			assertEqual(1, leaveCount, "onTouchLeave 触发 mOnTouchLeave");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// onTouchDown 链路: press(true) + pressDetail + mOnTouchDown 全部触发
	private static void testTouchDownChain()
	{
		myUGUIObject ui = createUI(out GameObject go);
		try
		{
			bool press = false;
			bool pressDetail = false;
			bool touchDown = false;
			ui.setPressCallback((h) => { press = h; });
			ui.setPressDetailCallback((pos, h) => { pressDetail = h; });
			ui.setOnTouchDown((pos, id) => { touchDown = true; });
			ui.onTouchDown(new Vector3(5.0f, 6.0f, 0.0f), 2);
			assertTrue(press, "onTouchDown 触发 press(true)");
			assertTrue(pressDetail, "onTouchDown 触发 pressDetail");
			assertTrue(touchDown, "onTouchDown 触发 mOnTouchDown");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// onReceiveDrag: 触发回调 + continueEvent 被置 false
	private static void testReceiveDragLink()
	{
		myUGUIObject ui = createUI(out GameObject go);
		try
		{
			bool received = false;
			bool continueEvent = true;
			// lambda 参数不能带 ref, 用匿名 delegate(支持 ref 参数)
			ui.setOnReceiveDrag(delegate (IMouseEventCollect obj, Vector3 pos, ref bool cont) { received = true; });
			ui.onReceiveDrag(null, new Vector3(1.0f, 2.0f, 0.0f), ref continueEvent);
			assertTrue(received, "onReceiveDrag 触发回调");
			assertFalse(continueEvent, "回调后 continueEvent 被置 false");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// onDragHovered: 触发 setOnDragHover 回调
	private static void testDragHoverLink()
	{
		myUGUIObject ui = createUI(out GameObject go);
		try
		{
			bool received = false;
			ui.setOnDragHover((obj, pos, h) => { received = h; });
			ui.onDragHovered(null, new Vector3(1.0f, 2.0f, 0.0f), true);
			assertTrue(received, "onDragHovered 触发回调");
			ui.onDragHovered(null, new Vector3(1.0f, 2.0f, 0.0f), false);
			assertFalse(received, "hover=false 转发");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setOnTouchUp 守卫式(触发需 GlobalTouch 流程, 调用不崩即覆盖)
	private static void testSetOnTouchUpGuard()
	{
		myUGUIObject ui = createUI(out GameObject go);
		try
		{
			ui.setOnTouchUp((pos, id) => { });
			// 守卫式: 无 getter, 调用不崩
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// 剩余 detail 回调 setter(守卫式, 无 getter) + setHoverDetailCallback 可验证
	private static void testDetailCallbackSetters()
	{
		myUGUIObject ui = createUI(out GameObject go);
		try
		{
			ui.setPreClickCallback(() => { });
			ui.setPreClickDetailCallback((pos) => { });
			ui.setClickDetailCallback((pos) => { });
			ui.setDoubleClickCallback(() => { });
			ui.setDoubleClickDetailCallback((pos) => { });
			// setHoverDetailCallback 可验证: onTouchEnter 里触发 hoverDetail(true)
			bool hoverDetail = false;
			ui.setHoverDetailCallback((pos, h) => { hoverDetail = h; });
			ui.onTouchEnter(new Vector3(1.0f, 1.0f, 0.0f), 1);
			assertTrue(hoverDetail, "onTouchEnter 触发 hoverDetail(true)");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}
}
