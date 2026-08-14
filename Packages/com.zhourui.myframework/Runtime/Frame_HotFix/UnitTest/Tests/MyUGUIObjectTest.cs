using static TestAssert;

using UObject = UnityEngine.Object;
using UnityEngine.UI;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
// myUGUIObject 中纯静态/轻量的方法
public static class MyUGUIObjectTest
{
	public static void Run()
	{
		// 复现测试放第一位: 确保只要本类被调用就必然执行, 不受前面任何测试失败中断影响
		testSetSiblingAfterDestroy();
		testDefaultClickSound();
        testDestroyWindowNull();
    

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
	

		testUGUIClick();
		testUGUIMouseEnterExit();
		testUGUIMouseDownUpStateMachine();
		testUGUIMouseMoveStay();
		testClearMousePointer();
		testEventTriggerAllEvents();
		testOnScreenTouchUpStorage();
	

		testStateFlags();
		testClickSound();
		testLongPressThreshold();
		testColliderForClick();
		testDepthFlags();
		testPassDragEvent();
		testDepth();
		testCallbackStorage();
		testAlphaWithChild();
		testSibling();
		testSortChild();
		testAddLongPress();
		testNotifyAnchorApply();
		testRefreshChildDepthByPositionZ();
	

		testSetTopToParentTop();
		testSetBottomToParentBottom();
		testSetLeftToParentLeft();
		testSetRightToParentRight();
		testSetTopCenterToParentTopCenter();
		testSetBottomCenterToParentBottomCenter();
		testSetLeftCenterToParentLeftCenter();
		testSetRightCenterToParentRightCenter();
		testSetInParentCenter();
		testGetInParent();
		testGetInSelf();
		testGetPositionNoPivot();
		testSetXInParentDirect();
		testRelativeOther();
		testRelativeSameSide();
	

		testInitSetsRectTransform();
		testSetSize();
		testSetWidthHeight();
		testPivot();
		testSelfBounds();
		testPosition();
		testParentBounds();
		testAlignToOther();
		testInParentCenter();
		testSetInParentRoundTrip();
		testCloneFrom();
	

		testSetParentEstablishesHierarchy();
		testSetParentSameParentNoDuplicates();
		testSetParentNullDetach();
		testRemoveChild();
		testSetLeftToParentLeft_Deep();
		testSetRightToParentRight_Deep();
		testSetLeftCenterToParentLeftCenter_Deep();
		testSetInParentCenter_Deep();
	}

    // destroyWindow / destroyWindowSingle: null 窗口直接安全返回
    // (完整销毁链路涉及对象池与全局系统交互, 风险高, 此处只测 null 分支确保稳定)
    static void testDestroyWindowNull()
    {
        myUGUIObject.destroyWindow(null, false);
        myUGUIObject.destroyWindow(null, true);
        myUGUIObject.destroyWindowSingle(null, false);
        myUGUIObject.destroyWindowSingle(null, true);
        assertTrue(true, "destroyWindow/destroyWindowSingle null 分支调用成功");
    }

    // setDefaultClickSound / getDefaultClickSound: 静态 int 字段读写
    // 测试后会恢复原值,避免污染全局点击音效状态
    static void testDefaultClickSound()
    {
        int original = myUGUIObject.getDefaultClickSound();
        try
        {
            myUGUIObject.setDefaultClickSound(0);
            assertEqual(0, myUGUIObject.getDefaultClickSound(), "set/get 0");

            myUGUIObject.setDefaultClickSound(123);
            assertEqual(123, myUGUIObject.getDefaultClickSound(), "set/get 123");

            myUGUIObject.setDefaultClickSound(-7);
            assertEqual(-7, myUGUIObject.getDefaultClickSound(), "set/get -7");
        }
        finally
        {
            myUGUIObject.setDefaultClickSound(original);
        }
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


	

	// ═════════════════════════════════════════════════════════════════
	// 辅助
	// ═════════════════════════════════════════════════════════════════
	private static myUGUIObject createUI_Deep3(out GameObject go)
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
		myUGUIObject ui = createUI_Deep3(out GameObject go);
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
		myUGUIObject ui = createUI_Deep3(out GameObject go);
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
		myUGUIObject ui = createUI_Deep3(out GameObject go);
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
		myUGUIObject ui = createUI_Deep3(out GameObject go);
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
		myUGUIObject ui = createUI_Deep3(out GameObject go);
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
		myUGUIObject ui = createUI_Deep3(out GameObject go);
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
		myUGUIObject ui = createUI_Deep3(out GameObject go);
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


	

	// 暴露 protected addChild, 用于构造 mChildList 测试 sortChild
	public class TestUIObjectAccessor : myUGUIObject
	{
		public void exposeAddChild(myUGUIObject child, bool refreshDepth)
		{
			addChild(child, refreshDepth);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 辅助
	// ═════════════════════════════════════════════════════════════════
	private static myUGUIObject createUI_Deep2(out GameObject go)
	{
		go = new GameObject("UIObject");
		myUGUIObject ui = new myUGUIObject();
		ui.setObject(go);
		ui.init();
		return ui;
	}

	// setDestroyImmediately/setIsNewObject 守卫式 + setReceiveLayoutHide 读回
	private static void testStateFlags()
	{
		myUGUIObject ui = createUI_Deep3(out GameObject go);
		try
		{
			ui.setDestroyImmediately(true);   // 守卫: 无 getter, 调用不崩
			ui.setIsNewObject(true);          // 守卫: 无 getter, 调用不崩
			ui.setReceiveLayoutHide(true);
			assertTrue(ui.isReceiveLayoutHide(), "setReceiveLayoutHide(true) 读回");
			ui.setReceiveLayoutHide(false);
			assertFalse(ui.isReceiveLayoutHide(), "setReceiveLayoutHide(false) 读回");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setClickSound/getClickSound 写读
	private static void testClickSound()
	{
		myUGUIObject ui = createUI_Deep3(out GameObject go);
		try
		{
			ui.setClickSound(7);
			assertEqual(7, ui.getClickSound(), "setClickSound(7) 读回");
			ui.setClickSound(0);
			assertEqual(0, ui.getClickSound(), "setClickSound(0) 读回");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setLongPressLengthThreshold/getLongPressLengthThreshold 写读
	private static void testLongPressThreshold()
	{
		myUGUIObject ui = createUI_Deep3(out GameObject go);
		try
		{
			ui.setLongPressLengthThreshold(0.8f);
			assertEqual(0.8f, ui.getLongPressLengthThreshold(), 0.001f, "setLongPressLengthThreshold(0.8) 读回");
			ui.setLongPressLengthThreshold(0.0f);
			assertEqual(0.0f, ui.getLongPressLengthThreshold(), 0.001f, "重置 0 读回");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setColliderForClick/isColliderForClick 往返
	private static void testColliderForClick()
	{
		myUGUIObject ui = createUI_Deep3(out GameObject go);
		try
		{
			ui.setColliderForClick(true);
			assertTrue(ui.isColliderForClick(), "setColliderForClick(true) 读回");
			ui.setColliderForClick(false);
			assertFalse(ui.isColliderForClick(), "setColliderForClick(false) 读回");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setDepthOverAllChild/setAllowGenerateDepth 读回
	private static void testDepthFlags()
	{
		myUGUIObject ui = createUI_Deep3(out GameObject go);
		try
		{
			ui.setDepthOverAllChild(true);
			assertTrue(ui.isDepthOverAllChild(), "setDepthOverAllChild(true) 读回");
			ui.setDepthOverAllChild(false);
			assertFalse(ui.isDepthOverAllChild(), "setDepthOverAllChild(false) 读回");
			ui.setAllowGenerateDepth(true);
			assertTrue(ui.isAllowGenerateDepth(), "setAllowGenerateDepth(true) 读回");
			ui.setAllowGenerateDepth(false);
			assertFalse(ui.isAllowGenerateDepth(), "setAllowGenerateDepth(false) 读回");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setPassDragEvent: 无 COMWindowDrag 组件时 isPassDragEvent 恒 true(短路, 文档化)
	private static void testPassDragEvent()
	{
		myUGUIObject ui = createUI_Deep3(out GameObject go);
		try
		{
			ui.setPassDragEvent(true);
			assertTrue(ui.isPassDragEvent(), "setPassDragEvent(true) → true");
			ui.setPassDragEvent(false);
			assertTrue(ui.isPassDragEvent(), "无 drag 组件时恒 true(!isDraggable 短路, 文档化)");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setDepth/getDepth: orderInParent 写读
	private static void testDepth()
	{
		myUGUIObject ui = createUI_Deep3(out GameObject go);
		try
		{
			ui.setDepth(new UIDepth(), 3);
			assertEqual(3, ui.getDepth().getOrderInParent(), "setDepth orderInParent=3 读回");
			ui.setDepth(new UIDepth(), 0);
			assertEqual(0, ui.getDepth().getOrderInParent(), "setDepth orderInParent=0 读回");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setPressCallback/setOnScreenTouchUp 回调存储读回(同一引用)
	private static void testCallbackStorage()
	{
		myUGUIObject ui = createUI_Deep3(out GameObject go);
		try
		{
			BoolCallback press = (hover) => { };
			ui.setPressCallback(press);
			assertTrue(ReferenceEquals(press, ui.getPressCallback()), "setPressCallback 读回同一回调");
			Vector3IntCallback touchUp = (pos, id) => { };
			ui.setOnScreenTouchUp(touchUp);
			ComponentInteractive com = ui.getComponent<ComponentInteractive>();
			assertTrue(ReferenceEquals(touchUp, com.getOnScreenTouchUp()), "setOnScreenTouchUp 读回同一回调");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setAlphaWithChild: 自身 getAlpha 恒 1.0(空实现), 子节点 Graphic 组件色被递归设置
	private static void testAlphaWithChild()
	{
		myUGUIObject ui = createUI_Deep3(out GameObject go);
		try
		{
			GameObject childGo = new GameObject("Child");
			childGo.AddComponent<RectTransform>();
			childGo.AddComponent<Image>();
			childGo.transform.SetParent(go.transform);
			ui.setAlphaWithChild(0.3f);
			Image childImage = childGo.GetComponent<Image>();
			assertEqual(0.3f, childImage.color.a, 0.001f, "子节点 Image alpha 被递归设置");
			assertEqual(1.0f, ui.getAlpha(), 0.001f, "自身 getAlpha 恒 1.0(空实现文档化)");
			// 恢复 alpha 不影响
			ui.setAlphaWithChild(1.0f);
			assertEqual(1.0f, childImage.color.a, 0.001f, "恢复 1.0");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setSibling/getSibling: 移动兄弟索引 + 相同位置返回 false
	private static void testSibling()
	{
		GameObject parentGo = new GameObject("Parent");
		myUGUIObject parent = new myUGUIObject();
		parent.setObject(parentGo);
		parent.init();
		try
		{
			myUGUIObject a = createChildUI("A", parentGo);
			myUGUIObject b = createChildUI("B", parentGo);
			myUGUIObject c = createChildUI("C", parentGo);
			// 初始 a(0) b(1) c(2)
			assertEqual(0, a.getSibling(), "初始 A index=0");
			assertEqual(1, b.getSibling(), "初始 B index=1");
			assertEqual(2, c.getSibling(), "初始 C index=2");
			assertTrue(a.setSibling(2, false), "setSibling(2) 返回 true");
			assertEqual(2, a.getSibling(), "A 移到 index=2");
			assertEqual(0, b.getSibling(), "B 变 index=0");
			assertFalse(a.setSibling(2, false), "相同位置返回 false");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(parentGo);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 复现线上报错: 窗口的 RectTransform 被销毁后仍调用 setSibling
	// 线上堆栈: myUGUIObject.setSibling → mTransform.GetSiblingIndex()
	//   → "The object of type 'UnityEngine.RectTransform' has been destroyed but you are still trying to access it."
	// 场景: 布局脚本(UIScene.update)持有已销毁窗口的引用, 仍调用 setSibling
	// 说明: 当前框架行为为抛异常(已知缺陷); 若后续在 setSibling 中加入已销毁保护,
	//       此测试需改为断言"安全返回/不抛异常"
	// ═════════════════════════════════════════════════════════════════
	private static void testSetSiblingAfterDestroy()
	{
		GameObject go = new GameObject("DestroyedUI", typeof(RectTransform));
		myUGUIObject ui = new myUGUIObject();
		ui.setObject(go);
		ui.init();
		RectTransform rt = ui.getRectTransform();
		assertNotNull(rt, "init 后 getRectTransform 非 null");

		// 按框架规范销毁窗口: setDestroyImmediately(true) + destroyWindow 立即销毁,
		// 与线上(UIScene 持有窗口引用, 窗口被销毁)路径一致
		ui.setDestroyImmediately(true);
		myUGUIObject.destroyWindow(ui, true);

		// 销毁后 RectTransform 必须处于"已销毁"状态(UnityEngine.Object == null 判定, 所有模式成立)
		bool rtDestroyed = rt == null;
		assertTrue(rtDestroyed, "destroyWindow(true) 后 RectTransform 仍有效——销毁未生效, 请检查 destroyWindow 链路");

		// 复现线上崩溃: setSibling → mTransform.GetSiblingIndex() 访问已销毁 RectTransform
		// 编辑器交互模式抛 MissingReferenceException(消息含 "has been destroyed")
		bool threw = false;
		string exceptionMsg = "";
		try
		{
			ui.setSibling(0);
		}
		catch (Exception e)
		{
			threw = true;
			exceptionMsg = e.GetType().Name + ": " + e.Message;
		}
		assertTrue(threw, "复现失败: 已销毁窗口调用 setSibling 未抛异常(RectTransform已销毁=" + rtDestroyed +
			")——请在编辑器交互模式(非批处理/非Player)运行测试");
		assertTrue(exceptionMsg.Contains("destroyed"), "复现出的异常不是'已销毁对象访问': " + exceptionMsg);
	}

	// sortChild: 按 sibling index 排序内部 mChildList
	private static void testSortChild()
	{
		GameObject parentGo = new GameObject("Parent");
		TestUIObjectAccessor parent = new TestUIObjectAccessor();
		parent.setObject(parentGo);
		parent.init();
		try
		{
			myUGUIObject a = createChildUI("A", parentGo);
			myUGUIObject b = createChildUI("B", parentGo);
			myUGUIObject c = createChildUI("C", parentGo);
			// 乱序加入内部列表: a(0) c(2) b(1)
			parent.exposeAddChild(a, false);
			parent.exposeAddChild(c, false);
			parent.exposeAddChild(b, false);
			parent.sortChild();
			System.Collections.Generic.List<myUGUIObject> list = parent.getChildList();
			assertTrue(ReferenceEquals(a, list[0]), "排序后第 0 个是 A");
			assertTrue(ReferenceEquals(b, list[1]), "排序后第 1 个是 B");
			assertTrue(ReferenceEquals(c, list[2]), "排序后第 2 个是 C");
			// 已排序后再次调用直接返回(无副作用)
			parent.sortChild();
			assertTrue(ReferenceEquals(a, list[0]), "重复 sortChild 无副作用");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(parentGo);
		}
	}

	// addLongPress/clearLongPress/removeLongPress 守卫式(不崩, 重复 add 不重复)
	// addLongPress/removeLongPress/clearLongPress 守卫式
	// 注意: removeLongPress 置 null 不移除元素(框架行为), 之后任何遍历(add/remove)会 NRE,
	//       必须先 clearLongPress 清掉残留 null 再继续
	private static void testAddLongPress()
	{
		myUGUIObject ui = createUI_Deep3(out GameObject go);
		try
		{
			Action callback = () => { };
			ui.addLongPress(callback, 0.5f, null);   // 首次添加
			ui.addLongPress(callback, 0.5f, null);   // 重复添加不重复
			ui.clearLongPress();                     // 清空
			ui.addLongPress(callback, 0.5f, null);
			ui.removeLongPress(callback);            // 移除指定 → 列表残留 null(框架行为)
			ui.clearLongPress();                     // 必须先清掉残留 null
			ui.removeLongPress(callback);            // 空列表 remove 安全
			ui.clearLongPress();                     // 空清空安全
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// notifyAnchorApply 空实现, 调用安全
	private static void testNotifyAnchorApply()
	{
		myUGUIObject ui = createUI_Deep3(out GameObject go);
		try
		{
			ui.notifyAnchorApply();   // 空实现, 不崩
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// refreshChildDepthByPositionZ: 按 z 降序重排兄弟(z 大 index 靠前)
	private static void testRefreshChildDepthByPositionZ()
	{
		GameObject parentGo = new GameObject("Parent");
		myUGUIObject parent = new myUGUIObject();
		parent.setObject(parentGo);
		parent.init();
		try
		{
			myUGUIObject high = createChildUI("High", parentGo);
			myUGUIObject low = createChildUI("Low", parentGo);
			high.setPosition(new Vector3(0.0f, 0.0f, 1.0f));    // z=1
			low.setPosition(new Vector3(0.0f, 0.0f, -1.0f));    // z=-1
			parent.refreshChildDepthByPositionZ();
			// z 大(1)的排到 index=0
			assertEqual(0, high.getSibling(), "z 大的排前面 index=0");
			assertEqual(1, low.getSibling(), "z 小的排后面 index=1");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(parentGo);
		}
	}

	private static myUGUIObject createChildUI(string name, GameObject parentGo)
	{
		GameObject go = new GameObject(name);
		go.AddComponent<RectTransform>();
		go.transform.SetParent(parentGo.transform);
		myUGUIObject ui = new myUGUIObject();
		ui.setObject(go);
		ui.init();
		return ui;
	}


	

	// ═════════════════════════════════════════════════════════════════
	// 辅助
	// ═════════════════════════════════════════════════════════════════
	private static myUGUIObject createWindow(GameObject go, string name, Vector2 size)
	{
		go.name = name;
		go.AddComponent<RectTransform>();
		myUGUIObject obj = new myUGUIObject();
		obj.setIsNewObject(true);
		obj.setObject(go);
		obj.init();
		obj.setSize(size);
		return obj;
	}

	// parent(100x100) + child(50x50) 树
	private static void createTree(out GameObject parentGo, out myUGUIObject parent, out GameObject childGo, out myUGUIObject child)
	{
		parentGo = new GameObject("AnchorParent");
		parent = createWindow(parentGo, "AnchorParent", new Vector2(100.0f, 100.0f));
		childGo = new GameObject("AnchorChild");
		child = createWindow(childGo, "AnchorChild", new Vector2(50.0f, 50.0f));
		child.setParent(parent, false);
	}

	// 顶停靠: y = parentTop(50) - childTopInSelf(25) = 25
	private static void testSetTopToParentTop()
	{
		createTree(out GameObject parentGo, out myUGUIObject parent, out GameObject childGo, out myUGUIObject child);
		try
		{
			child.setTopToParentTop();
			assertEqual(25.0f, child.getPosition().y, 0.001f, "child 顶 = parent 顶 → y=25");
			assertEqual(50.0f, child.getTopInParent(), 0.001f, "顶边界 = parent 顶 50");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(parentGo);
		}
	}

	// 底停靠: y = parentBottom(-50) - childBottomInSelf(-25) = -25
	private static void testSetBottomToParentBottom()
	{
		createTree(out GameObject parentGo, out myUGUIObject parent, out GameObject childGo, out myUGUIObject child);
		try
		{
			child.setBottomToParentBottom();
			assertEqual(-25.0f, child.getPosition().y, 0.001f, "child 底 = parent 底 → y=-25");
			assertEqual(-50.0f, child.getBottomInParent(), 0.001f, "底边界 = parent 底 -50");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(parentGo);
		}
	}

	// 左停靠: x = parentLeft(-50) - childLeftInSelf(-25) = -25
	private static void testSetLeftToParentLeft()
	{
		createTree(out GameObject parentGo, out myUGUIObject parent, out GameObject childGo, out myUGUIObject child);
		try
		{
			child.setLeftToParentLeft();
			assertEqual(-25.0f, child.getPosition().x, 0.001f, "child 左 = parent 左 → x=-25");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(parentGo);
		}
	}

	// 右停靠: x = parentRight(50) - childRightInSelf(25) = 25
	private static void testSetRightToParentRight()
	{
		createTree(out GameObject parentGo, out myUGUIObject parent, out GameObject childGo, out myUGUIObject child);
		try
		{
			child.setRightToParentRight();
			assertEqual(25.0f, child.getPosition().x, 0.001f, "child 右 = parent 右 → x=25");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(parentGo);
		}
	}

	// 顶中: y=25, x=0
	private static void testSetTopCenterToParentTopCenter()
	{
		createTree(out GameObject parentGo, out myUGUIObject parent, out GameObject childGo, out myUGUIObject child);
		try
		{
			child.setTopCenterToParentTopCenter();
			assertEqual(25.0f, child.getPosition().y, 0.001f, "顶中 y=25");
			assertEqual(0.0f, child.getPosition().x, 0.001f, "顶中 x=0");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(parentGo);
		}
	}

	// 底中: y=-25, x=0
	private static void testSetBottomCenterToParentBottomCenter()
	{
		createTree(out GameObject parentGo, out myUGUIObject parent, out GameObject childGo, out myUGUIObject child);
		try
		{
			child.setBottomCenterToParentBottomCenter();
			assertEqual(-25.0f, child.getPosition().y, 0.001f, "底中 y=-25");
			assertEqual(0.0f, child.getPosition().x, 0.001f, "底中 x=0");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(parentGo);
		}
	}

	// 左中: x=-25, y=0
	private static void testSetLeftCenterToParentLeftCenter()
	{
		createTree(out GameObject parentGo, out myUGUIObject parent, out GameObject childGo, out myUGUIObject child);
		try
		{
			child.setLeftCenterToParentLeftCenter();
			assertEqual(-25.0f, child.getPosition().x, 0.001f, "左中 x=-25");
			assertEqual(0.0f, child.getPosition().y, 0.001f, "左中 y=0");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(parentGo);
		}
	}

	// 右中: x=25, y=0
	private static void testSetRightCenterToParentRightCenter()
	{
		createTree(out GameObject parentGo, out myUGUIObject parent, out GameObject childGo, out myUGUIObject child);
		try
		{
			child.setRightCenterToParentRightCenter();
			assertEqual(25.0f, child.getPosition().x, 0.001f, "右中 x=25");
			assertEqual(0.0f, child.getPosition().y, 0.001f, "右中 y=0");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(parentGo);
		}
	}

	// 父中心: setInParentCenterX/Y → 0
	private static void testSetInParentCenter()
	{
		createTree(out GameObject parentGo, out myUGUIObject parent, out GameObject childGo, out myUGUIObject child);
		try
		{
			child.setPosition(new Vector3(30.0f, 40.0f, 0.0f));
			child.setInParentCenterX();
			assertEqual(0.0f, child.getPosition().x, 0.001f, "setInParentCenterX → x=0");
			assertEqual(40.0f, child.getPosition().y, 0.001f, "y 不受影响");
			child.setInParentCenterY();
			assertEqual(0.0f, child.getPosition().y, 0.001f, "setInParentCenterY → y=0");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(parentGo);
		}
	}

	// getLeftInParent 等: 位置 0 时 = ±25
	private static void testGetInParent()
	{
		createTree(out GameObject parentGo, out myUGUIObject parent, out GameObject childGo, out myUGUIObject child);
		try
		{
			child.setPosition(Vector3.zero);
			assertEqual(-25.0f, child.getLeftInParent(), 0.001f, "左边界 -25");
			assertEqual(25.0f, child.getRightInParent(), 0.001f, "右边界 25");
			assertEqual(25.0f, child.getTopInParent(), 0.001f, "顶边界 25");
			assertEqual(-25.0f, child.getBottomInParent(), 0.001f, "底边界 -25");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(parentGo);
		}
	}

	// getXInSelf: 与 pivot/size 的纯数学
	private static void testGetInSelf()
	{
		createTree(out GameObject parentGo, out myUGUIObject parent, out GameObject childGo, out myUGUIObject child);
		try
		{
			assertEqual(-25.0f, child.getLeftInSelf(), 0.001f, "leftInSelf = -50*0.5 = -25");
			assertEqual(25.0f, child.getRightInSelf(), 0.001f, "rightInSelf = 50*0.5 = 25");
			assertEqual(25.0f, child.getTopInSelf(), 0.001f, "topInSelf = 50*0.5 = 25");
			assertEqual(-25.0f, child.getBottomInSelf(), 0.001f, "bottomInSelf = -50*0.5 = -25");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(parentGo);
		}
	}

	// getPositionNoPivot: pivot 0.5 时 = localPosition
	private static void testGetPositionNoPivot()
	{
		createTree(out GameObject parentGo, out myUGUIObject parent, out GameObject childGo, out myUGUIObject child);
		try
		{
			child.setPosition(new Vector3(10.0f, 20.0f, 0.0f));
			Vector3 noPivot = child.getPositionNoPivot();
			assertEqual(10.0f, noPivot.x, 0.001f, "pivot 0.5 → x 无偏移");
			assertEqual(20.0f, noPivot.y, 0.001f, "pivot 0.5 → y 无偏移");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(parentGo);
		}
	}

	// setLeftInParent(0): x = 0 - leftInSelf(-25) = 25
	private static void testSetXInParentDirect()
	{
		createTree(out GameObject parentGo, out myUGUIObject parent, out GameObject childGo, out myUGUIObject child);
		try
		{
			child.setLeftInParent(0.0f);
			assertEqual(25.0f, child.getPosition().x, 0.001f, "setLeftInParent(0) → x=25");
			child.setRightInParent(0.0f);
			assertEqual(-25.0f, child.getPosition().x, 0.001f, "setRightInParent(0) → x=-25");
			child.setTopInParent(0.0f);
			assertEqual(-25.0f, child.getPosition().y, 0.001f, "setTopInParent(0) → y=-25");
			child.setBottomInParent(0.0f);
			assertEqual(25.0f, child.getPosition().y, 0.001f, "setBottomInParent(0) → y=25");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(parentGo);
		}
	}

	// 相对定位(异侧): child2 相对 child1 的四边 + interval
	private static void testRelativeOther()
	{
		createTree(out GameObject parentGo, out myUGUIObject parent, out GameObject childGo, out myUGUIObject child);
		GameObject otherGo = new GameObject("AnchorOther");
		myUGUIObject other = createWindow(otherGo, "AnchorOther", new Vector2(30.0f, 30.0f));
		other.setParent(parent, false);
		try
		{
			child.setPosition(new Vector3(10.0f, 20.0f, 0.0f));
			// child1: left=-15, right=35, top=45, bottom=-5
			// other2(30x30, 边界±15): setRightToOtherLeft → 右边 = child1左-5 = -20 → x = -20-15 = -35
			other.setRightToOtherLeft(child, 5.0f);
			assertEqual(-35.0f, other.getPosition().x, 0.001f, "other 右 = child 左 -5 → x=-35");
			// setLeftToOtherRight → 左边 = child1右+5 = 40 → x = 40-(-15) = 55
			other.setLeftToOtherRight(child, 5.0f);
			assertEqual(55.0f, other.getPosition().x, 0.001f, "other 左 = child 右 +5 → x=55");
			// setBottomToOtherTop → 底 = child1顶+5 = 50 → y = 50-(-15) = 65
			other.setBottomToOtherTop(child, 5.0f);
			assertEqual(65.0f, other.getPosition().y, 0.001f, "other 底 = child 顶 +5 → y=65");
			// setTopToOtherBottom → 顶 = child1底-5 = -10 → y = -10-15 = -25
			other.setTopToOtherBottom(child, 5.0f);
			assertEqual(-25.0f, other.getPosition().y, 0.001f, "other 顶 = child 底 -5 → y=-25");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(otherGo);
			UnityEngine.Object.DestroyImmediate(parentGo);
		}
	}

		// 相对定位(同侧): 直接公式(非 setXInParent 组合, interval 符号各不同)
		//   setLeftToOtherLeft:  x = other.x - other.size.x*0.5 + this.size.x*0.5 + interval
		//   setRightToOtherRight: x = other.x + other.size.x*0.5 - this.size.x*0.5 - interval
		//   setTopToOtherTop:     y = other.y + other.size.y*0.5 - this.size.y*0.5 - interval
		private static void testRelativeSameSide()
		{
			createTree(out GameObject parentGo, out myUGUIObject parent, out GameObject childGo, out myUGUIObject child);
			GameObject otherGo = new GameObject("AnchorOther2");
			myUGUIObject other = createWindow(otherGo, "AnchorOther2", new Vector2(30.0f, 30.0f));
			other.setParent(parent, false);
			try
			{
				child.setPosition(new Vector3(10.0f, 20.0f, 0.0f));
				// child(50x50) @(10,20), other(30x30)
				// setLeftToOtherLeft: x = 10-25+15+5 = 5
				other.setLeftToOtherLeft(child, 5.0f);
				assertEqual(5.0f, other.getPosition().x, 0.001f, "other 左对齐 child 左 +5 → x=5");
				// setRightToOtherRight: x = 10+25-15-5 = 15
				other.setRightToOtherRight(child, 5.0f);
				assertEqual(15.0f, other.getPosition().x, 0.001f, "other 右对齐 child 右 -5 → x=15");
				// setTopToOtherTop: y = 20+25-15-5 = 25
				other.setTopToOtherTop(child, 5.0f);
				assertEqual(25.0f, other.getPosition().y, 0.001f, "other 顶对齐 child 顶 -5 → y=25");
			}
			finally
			{
				UnityEngine.Object.DestroyImmediate(otherGo);
				UnityEngine.Object.DestroyImmediate(parentGo);
			}
		}


	

	// ═════════════════════════════════════════════════════════════════
	// init 后 mRectTransform 非 null, getRectTransform 返回同一对象
	// ═════════════════════════════════════════════════════════════════
	private static void testInitSetsRectTransform()
	{
		GameObject go = new GameObject("TestUI");
		try
		{
			RectTransform rt = go.AddComponent<RectTransform>();
			myUGUIObject ui = new myUGUIObject();
			ui.setObject(go);
			ui.init();
			assertNotNull(ui.getRectTransform(), "init 后 getRectTransform 非 null");
			assertTrue(ReferenceEquals(rt, ui.getRectTransform()), "getRectTransform 返回 Go 上的 RectTransform");
		}
		finally
		{
			UObject.DestroyImmediate(go);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// setSize → getSize 一致(无父节点: sizeDelta=size)
	// ═════════════════════════════════════════════════════════════════
	private static void testSetSize()
	{
		GameObject go = new GameObject("TestUI");
		try
		{
			go.AddComponent<RectTransform>();
			myUGUIObject ui = new myUGUIObject();
			ui.setObject(go);
			ui.init();
			ui.setSize(new Vector2(100.0f, 50.0f));
			Vector2 size = ui.getSize();
			assertEqual(100.0f, size.x, 0.001f, "setSize 后 getSize.x=100");
			assertEqual(50.0f, size.y, 0.001f, "setSize 后 getSize.y=50");
			// 再设一次不同大小
			ui.setSize(new Vector2(200.0f, 80.0f));
			size = ui.getSize();
			assertEqual(200.0f, size.x, 0.001f, "二次 setSize 后 getSize.x=200");
			assertEqual(80.0f, size.y, 0.001f, "二次 setSize 后 getSize.y=80");
		}
		finally
		{
			UObject.DestroyImmediate(go);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// setWidth/setHeight — 只改单轴
	// ═════════════════════════════════════════════════════════════════
	private static void testSetWidthHeight()
	{
		GameObject go = new GameObject("TestUI");
		try
		{
			go.AddComponent<RectTransform>();
			myUGUIObject ui = new myUGUIObject();
			ui.setObject(go);
			ui.init();
			ui.setSize(new Vector2(100.0f, 50.0f));
			ui.setWidth(150.0f);
			Vector2 size = ui.getSize();
			assertEqual(150.0f, size.x, 0.001f, "setWidth 后 getSize.x=150");
			assertEqual(50.0f, size.y, 0.001f, "setWidth 不影响 getSize.y");
			ui.setHeight(75.0f);
			size = ui.getSize();
			assertEqual(150.0f, size.x, 0.001f, "setHeight 不影响 getSize.x");
			assertEqual(75.0f, size.y, 0.001f, "setHeight 后 getSize.y=75");
		}
		finally
		{
			UObject.DestroyImmediate(go);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// setPivot/getPivot — pivot 读写
	// ═════════════════════════════════════════════════════════════════
	private static void testPivot()
	{
		GameObject go = new GameObject("TestUI");
		try
		{
			go.AddComponent<RectTransform>();
			myUGUIObject ui = new myUGUIObject();
			ui.setObject(go);
			ui.init();
			// RectTransform 默认 pivot 是 (0.5, 0.5)
			Vector2 def = ui.getPivot();
			assertEqual(0.5f, def.x, 0.001f, "默认 pivot.x=0.5");
			assertEqual(0.5f, def.y, 0.001f, "默认 pivot.y=0.5");
			ui.setPivot(new Vector2(0.0f, 0.0f));
			Vector2 p = ui.getPivot();
			assertEqual(0.0f, p.x, 0.001f, "setPivot(0,0) 后 pivot.x=0");
			assertEqual(0.0f, p.y, 0.001f, "setPivot(0,0) 后 pivot.y=0");
		}
		finally
		{
			UObject.DestroyImmediate(go);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// getLeftInSelf/getRightInSelf/getTopInSelf/getBottomInSelf
	// 基于 size × pivot 的边界计算
	// ═════════════════════════════════════════════════════════════════
	private static void testSelfBounds()
	{
		GameObject go = new GameObject("TestUI");
		try
		{
			go.AddComponent<RectTransform>();
			myUGUIObject ui = new myUGUIObject();
			ui.setObject(go);
			ui.init();
			ui.setSize(new Vector2(100.0f, 50.0f));
			// pivot 默认 (0.5, 0.5): left=-50, right=50, top=25, bottom=-25
			assertEqual(-50.0f, ui.getLeftInSelf(), 0.001f, "pivot=0.5, size=100 → left=-50");
			assertEqual(50.0f, ui.getRightInSelf(), 0.001f, "pivot=0.5, size=100 → right=50");
			assertEqual(25.0f, ui.getTopInSelf(), 0.001f, "pivot=0.5, size=50 → top=25");
			assertEqual(-25.0f, ui.getBottomInSelf(), 0.001f, "pivot=0.5, size=50 → bottom=-25");
			// pivot=(0,0): left=0, right=100, top=50, bottom=0
			ui.setPivot(new Vector2(0.0f, 0.0f));
			assertEqual(0.0f, ui.getLeftInSelf(), 0.001f, "pivot=0 → left=0");
			assertEqual(100.0f, ui.getRightInSelf(), 0.001f, "pivot=0 → right=100");
			assertEqual(50.0f, ui.getTopInSelf(), 0.001f, "pivot=0 → top=50");
			assertEqual(0.0f, ui.getBottomInSelf(), 0.001f, "pivot=0 → bottom=0");
		}
		finally
		{
			UObject.DestroyImmediate(go);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// setPosition/getPosition — localPosition 读写
	// ═════════════════════════════════════════════════════════════════
	private static void testPosition()
	{
		GameObject go = new GameObject("TestUI");
		try
		{
			go.AddComponent<RectTransform>();
			myUGUIObject ui = new myUGUIObject();
			ui.setObject(go);
			ui.init();
			ui.setPosition(new Vector3(10.0f, 20.0f, 0.0f));
			Vector3 pos = ui.getPosition();
			assertEqual(10.0f, pos.x, 0.001f, "setPosition 后 x=10");
			assertEqual(20.0f, pos.y, 0.001f, "setPosition 后 y=20");
			// setPositionX 只改 x
			ui.setPositionX(30.0f);
			pos = ui.getPosition();
			assertEqual(30.0f, pos.x, 0.001f, "setPositionX 后 x=30");
			assertEqual(20.0f, pos.y, 0.001f, "setPositionX 不影响 y");
		}
		finally
		{
			UObject.DestroyImmediate(go);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// getLeftInParent/getRightInParent/getTopInParent/getBottomInParent
	// = getPosition + self 边界(无父也成立, 纯公式)
	// ═════════════════════════════════════════════════════════════════
	private static void testParentBounds()
	{
		GameObject go = new GameObject("TestUI");
		try
		{
			go.AddComponent<RectTransform>();
			myUGUIObject ui = new myUGUIObject();
			ui.setObject(go);
			ui.init();
			ui.setSize(new Vector2(100.0f, 50.0f));
			ui.setPosition(new Vector3(20.0f, 10.0f, 0.0f));
			// pivot=0.5: leftInSelf=-50, rightInSelf=50 → leftInParent=20-50=-30, rightInParent=20+50=70
			assertEqual(-30.0f, ui.getLeftInParent(), 0.001f, "leftInParent = pos.x + leftInSelf = -30");
			assertEqual(70.0f, ui.getRightInParent(), 0.001f, "rightInParent = pos.x + rightInSelf = 70");
			assertEqual(35.0f, ui.getTopInParent(), 0.001f, "topInParent = pos.y + topInSelf = 10+25=35");
			assertEqual(-15.0f, ui.getBottomInParent(), 0.001f, "bottomInParent = pos.y + bottomInSelf = 10-25=-15");
		}
		finally
		{
			UObject.DestroyImmediate(go);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// setLeftToOtherLeft 等对齐方法 — 无父时 getParent 都 null → 走对齐公式
	// setPositionX(other.x - other.size.x/2 + self.size.x/2 + interval)
	// ═════════════════════════════════════════════════════════════════
	private static void testAlignToOther()
	{
		GameObject goA = new GameObject("TestUIA");
		GameObject goB = new GameObject("TestUIB");
		try
		{
			goA.AddComponent<RectTransform>();
			goB.AddComponent<RectTransform>();
			myUGUIObject uiA = new myUGUIObject();
			uiA.setObject(goA);
			uiA.init();
			myUGUIObject uiB = new myUGUIObject();
			uiB.setObject(goB);
			uiB.init();
			// A: size=100x50, pos=(10,0); B: size=60x30, interval=5
			uiA.setSize(new Vector2(100.0f, 50.0f));
			uiA.setPosition(new Vector3(10.0f, 0.0f, 0.0f));
			uiB.setSize(new Vector2(60.0f, 30.0f));
			uiB.setLeftToOtherLeft(uiA, 5.0f);
			// B 新 x = 10 - 50 + 30 + 5 = -5; B 左边界 = -5 - 30 = -35 = A 左边界(-40)+5
			Vector3 posB = uiB.getPosition();
			assertEqual(-5.0f, posB.x, 0.001f, "setLeftToOtherLeft 后 B.x=-5");
			assertEqual(-35.0f, uiB.getLeftInParent(), 0.001f, "B 左边界对齐 A 左边界+interval");
			// setLeftToOtherRight: B 左边界 = A 右边界 + interval
			// B 新 x = 10 + 50 + 30 + 5 = 95 → B 左边界 = 95-30 = 65 = A 右边界(60)+5
			uiB.setLeftToOtherRight(uiA, 5.0f);
			posB = uiB.getPosition();
			assertEqual(95.0f, posB.x, 0.001f, "setLeftToOtherRight 后 B.x=95");
			assertEqual(65.0f, uiB.getLeftInParent(), 0.001f, "B 左边界对齐 A 右边界+interval");
			// setRightToOtherLeft: B 右边界 = A 左边界 - interval
			// B 新 x = 10 - 50 - 30 - 5 = -75 → B 右边界 = -75+30 = -45 = A 左边界(-40)-5
			uiB.setRightToOtherLeft(uiA, 5.0f);
			posB = uiB.getPosition();
			assertEqual(-75.0f, posB.x, 0.001f, "setRightToOtherLeft 后 B.x=-75");
			assertEqual(-45.0f, uiB.getRightInParent(), 0.001f, "B 右边界对齐 A 左边界-interval");
		}
		finally
		{
			UObject.DestroyImmediate(goA);
			UObject.DestroyImmediate(goB);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// setInParentCenterX/Y — 位置归零(无父时即 localPosition=0)
	// ═════════════════════════════════════════════════════════════════
	private static void testInParentCenter()
	{
		GameObject go = new GameObject("TestUI");
		try
		{
			go.AddComponent<RectTransform>();
			myUGUIObject ui = new myUGUIObject();
			ui.setObject(go);
			ui.init();
			ui.setPosition(new Vector3(30.0f, 40.0f, 0.0f));
			ui.setInParentCenterX();
			assertEqual(0.0f, ui.getPosition().x, 0.001f, "setInParentCenterX 后 x=0");
			assertEqual(40.0f, ui.getPosition().y, 0.001f, "setInParentCenterX 不影响 y");
			ui.setInParentCenterY();
			assertEqual(0.0f, ui.getPosition().y, 0.001f, "setInParentCenterY 后 y=0");
		}
		finally
		{
			UObject.DestroyImmediate(go);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// setLeftInParent/setRightInParent/setTopInParent/setBottomInParent
	// 与 getXxxInParent 互为逆运算: set 后 get 还原
	// ═════════════════════════════════════════════════════════════════
	private static void testSetInParentRoundTrip()
	{
		GameObject go = new GameObject("TestUI");
		try
		{
			go.AddComponent<RectTransform>();
			myUGUIObject ui = new myUGUIObject();
			ui.setObject(go);
			ui.init();
			ui.setSize(new Vector2(100.0f, 50.0f));
			// 目标边界值
			ui.setLeftInParent(-30.0f);
			assertEqual(-30.0f, ui.getLeftInParent(), 0.001f, "setLeftInParent 后 getLeftInParent 还原");
			ui.setRightInParent(70.0f);
			assertEqual(70.0f, ui.getRightInParent(), 0.001f, "setRightInParent 后 getRightInParent 还原");
			ui.setTopInParent(35.0f);
			assertEqual(35.0f, ui.getTopInParent(), 0.001f, "setTopInParent 后 getTopInParent 还原");
			ui.setBottomInParent(-15.0f);
			assertEqual(-15.0f, ui.getBottomInParent(), 0.001f, "setBottomInParent 后 getBottomInParent 还原");
			// 逆运算一致性: 设左边界-30 → position.x = -30 - leftInSelf = -30+50 = 20
			assertEqual(20.0f, ui.getPosition().x, 0.001f, "setLeftInParent(-30) → pos.x=20");
		}
		finally
		{
			UObject.DestroyImmediate(go);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// cloneFrom — 同类型克隆 position/rotation/scale
	// ═════════════════════════════════════════════════════════════════
	private static void testCloneFrom()
	{
		GameObject goA = new GameObject("TestUIA");
		GameObject goB = new GameObject("TestUIB");
		try
		{
			goA.AddComponent<RectTransform>();
			goB.AddComponent<RectTransform>();
			myUGUIObject src = new myUGUIObject();
			src.setObject(goA);
			src.init();
			myUGUIObject dst = new myUGUIObject();
			dst.setObject(goB);
			dst.init();
			src.setPosition(new Vector3(15.0f, 25.0f, 5.0f));
			src.setScale(new Vector3(2.0f, 2.0f, 2.0f));
			dst.cloneFrom(src);
			Vector3 pos = dst.getPosition();
			assertEqual(15.0f, pos.x, 0.001f, "cloneFrom 复制 position.x");
			assertEqual(25.0f, pos.y, 0.001f, "cloneFrom 复制 position.y");
			Vector3 scale = dst.getScale();
			assertEqual(2.0f, scale.x, 0.001f, "cloneFrom 复制 scale.x");
		}
		finally
		{
			UObject.DestroyImmediate(goA);
			UObject.DestroyImmediate(goB);
		}
	}


	

	// ═════════════════════════════════════════════════════════════════
	// 辅助: 创建 UI 对象(setObject+init, 无父时 setSize → sizeDelta=size 确定)
	// ═════════════════════════════════════════════════════════════════
	private static myUGUIObject createUI_Deep4(string name, Vector2 size, out GameObject go)
	{
		go = new GameObject(name);
		go.AddComponent<RectTransform>();
		myUGUIObject ui = new myUGUIObject();
		ui.setObject(go);
		ui.init();
		ui.setSize(size);
		return ui;
	}

	// ═════════════════════════════════════════════════════════════════
	// setParent: 建立父子层级(Transform 父子 + 父列表)
	// ═════════════════════════════════════════════════════════════════
	private static void testSetParentEstablishesHierarchy()
	{
		myUGUIObject parent = createUI_Deep4("Parent", new Vector2(200.0f, 100.0f), out GameObject goP);
		myUGUIObject child = createUI_Deep4("Child", new Vector2(60.0f, 30.0f), out GameObject goC);
		try
		{
			child.setParent(parent, false);
			assertTrue(ReferenceEquals(parent, child.getParent()), "child.getParent()==parent");
			assertTrue(ReferenceEquals(goP.transform, goC.transform.parent), "Transform 父子层级建立");
			assertEqual(1, parent.getChildList().Count, "parent 子列表含 1 个");
			assertTrue(parent.getChildList().Contains(child), "子列表包含 child");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(goC);
			UnityEngine.Object.DestroyImmediate(goP);
		}
	}

	// 重复 setParent 相同 parent: 直接 return, 不重复添加
	private static void testSetParentSameParentNoDuplicates()
	{
		myUGUIObject parent = createUI_Deep4("Parent", new Vector2(200.0f, 100.0f), out GameObject goP);
		myUGUIObject child = createUI_Deep4("Child", new Vector2(60.0f, 30.0f), out GameObject goC);
		try
		{
			child.setParent(parent, false);
			child.setParent(parent, false);
			assertEqual(1, parent.getChildList().Count, "重复 setParent 不重复添加");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(goC);
			UnityEngine.Object.DestroyImmediate(goP);
		}
	}

	// setParent(null): 解绑(从父列表移除 + mParent 置空)
	private static void testSetParentNullDetach()
	{
		myUGUIObject parent = createUI_Deep4("Parent", new Vector2(200.0f, 100.0f), out GameObject goP);
		myUGUIObject child = createUI_Deep4("Child", new Vector2(60.0f, 30.0f), out GameObject goC);
		try
		{
			child.setParent(parent, false);
			child.setParent(null, false);
			assertNull(child.getParent(), "setParent(null) 后 getParent 为 null");
			assertEqual(0, parent.getChildList().Count, "从父列表移除");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(goC);
			UnityEngine.Object.DestroyImmediate(goP);
		}
	}

	// removeChild: 只操作父列表, 不改 child.mParent(真实语义)
	private static void testRemoveChild()
	{
		myUGUIObject parent = createUI_Deep4("Parent", new Vector2(200.0f, 100.0f), out GameObject goP);
		myUGUIObject child = createUI_Deep4("Child", new Vector2(60.0f, 30.0f), out GameObject goC);
		try
		{
			child.setParent(parent, false);
			parent.removeChild(child);
			assertEqual(0, parent.getChildList().Count, "removeChild 后父列表空");
			assertTrue(ReferenceEquals(parent, child.getParent()), "removeChild 不改 child.mParent(真实语义)");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(goC);
			UnityEngine.Object.DestroyImmediate(goP);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 有父对齐: 左边界对齐父左边界
	// parent left = -100, child left = -30 → x = -70
	// ═════════════════════════════════════════════════════════════════
	private static void testSetLeftToParentLeft_Deep()
	{
		myUGUIObject parent = createUI_Deep4("Parent", new Vector2(200.0f, 100.0f), out GameObject goP);
		myUGUIObject child = createUI_Deep4("Child", new Vector2(60.0f, 30.0f), out GameObject goC);
		try
		{
			child.setParent(parent, false);
			child.setLeftToParentLeft();
			Vector3 pos = child.getPosition();
			assertEqual(-70.0f, pos.x, 0.001f, "左边界对齐 parent 左(-100): x=-70");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(goC);
			UnityEngine.Object.DestroyImmediate(goP);
		}
	}

	// 右边界对齐父右边界
	// parent right = +100, child right = +30 → x = 70
	private static void testSetRightToParentRight_Deep()
	{
		myUGUIObject parent = createUI_Deep4("Parent", new Vector2(200.0f, 100.0f), out GameObject goP);
		myUGUIObject child = createUI_Deep4("Child", new Vector2(60.0f, 30.0f), out GameObject goC);
		try
		{
			child.setParent(parent, false);
			child.setRightToParentRight();
			Vector3 pos = child.getPosition();
			assertEqual(70.0f, pos.x, 0.001f, "右边界对齐 parent 右(+100): x=70");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(goC);
			UnityEngine.Object.DestroyImmediate(goP);
		}
	}

	// 左边界中心对齐: X 对齐左边界 + Y 居中
	private static void testSetLeftCenterToParentLeftCenter_Deep()
	{
		myUGUIObject parent = createUI_Deep4("Parent", new Vector2(200.0f, 100.0f), out GameObject goP);
		myUGUIObject child = createUI_Deep4("Child", new Vector2(60.0f, 30.0f), out GameObject goC);
		try
		{
			child.setParent(parent, false);
			child.setPosition(new Vector3(10.0f, 20.0f, 0.0f));
			child.setLeftCenterToParentLeftCenter();
			Vector3 pos = child.getPosition();
			assertEqual(-70.0f, pos.x, 0.001f, "左中心对齐: x=-70");
			assertEqual(0.0f, pos.y, 0.001f, "左中心对齐: y=0(居中)");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(goC);
			UnityEngine.Object.DestroyImmediate(goP);
		}
	}

	// 居中: setInParentCenterX/Y → 位置归零
	private static void testSetInParentCenter_Deep()
	{
		myUGUIObject parent = createUI_Deep4("Parent", new Vector2(200.0f, 100.0f), out GameObject goP);
		myUGUIObject child = createUI_Deep4("Child", new Vector2(60.0f, 30.0f), out GameObject goC);
		try
		{
			child.setParent(parent, false);
			child.setPosition(new Vector3(50.0f, 60.0f, 0.0f));
			child.setInParentCenterX();
			assertEqual(0.0f, child.getPosition().x, 0.001f, "setInParentCenterX 后 x=0");
			assertEqual(60.0f, child.getPosition().y, 0.001f, "setInParentCenterX 不影响 y");
			child.setInParentCenterY();
			assertEqual(0.0f, child.getPosition().y, 0.001f, "setInParentCenterY 后 y=0");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(goC);
			UnityEngine.Object.DestroyImmediate(goP);
		}
	}
}
