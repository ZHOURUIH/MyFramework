using System.Collections.Generic;
using UnityEngine;
using static TestAssert;
using static FrameBaseHotFix;

// UGUIScroll 深度测试
// 覆盖滑动列表控件的核心逻辑(不依赖真实 Camera 的部分):
//   initScroll: 容器列表 + mainFocus 计算(默认中间/指定)
//   setItemList: 项列表 + mMaxControlValue + defaultIndex 定位 + 滚动回调
//   scroll/scrollToIndex: 直接定位 + 越界 clamp + checkValueRange 开关
//   scrollToIndexWithTime: 定时滚动状态机(SCROLL_TO_TARGET) + update 逐帧逼近 + 到达停止
//   lerpToTarget: 曲线插值聚焦(LERP_SCROLL_TO_TARGET) + 到达停止
//   getNearIndex: 最近下标查找(含中点边界)
//   updateItem: 非循环模式下越界项隐藏 + 容器插值 lerpItem 调用
//   拖拽状态机: onMouseDown/onMouseMove(速度计算)/onMouseStay/onScreenMouseUp/stop
//
// 环境: TestLayoutScriptDeep + GameLayout + myUGUICanvas
// 数据: 5 容器(mainFocus=2) + 4 项(非循环 mMaxControlValue=3)
// 清理: 节点用 destroyObject 立即销毁, rootGo 手动 DestroyImmediate
public static class UGUIScrollTest
{
	public static void Run()
	{
		testScrollInitAndDefaults();
		testScrollInitScrollMainFocus();
		testScrollSetItemList();
		testScrollScrollClamp();
		testScrollToIndex();
		testScrollToIndexWithTime();
		testLerpToTarget();
		testGetNearIndex();
		testDragStateMachine();
		testUpdateItemLerp();
	}

	// ═════════════════════════════════════════════════════════════════
	// 辅助: 创建 script + 根节点
	// ═════════════════════════════════════════════════════════════════
	private static TestLayoutScriptDeep createScript(out GameObject rootGo, out myUGUIObject scrollRoot)
	{
		rootGo = new GameObject("TestScrollRoot");
		rootGo.AddComponent<RectTransform>();
		rootGo.AddComponent<Canvas>();
		myUGUICanvas root = new myUGUICanvas();
		root.setObject(rootGo);
		root.init();
		TestLayoutScriptDeep script = new TestLayoutScriptDeep();
		script.setLayout(new GameLayout());
		script.setRoot(root);
		scrollRoot = script.createUGUIObject<myUGUIObject>(null, "ScrollRoot", true);
		return script;
	}

	// 创建已 init 的滚动控件 + 5 容器 + 4 项, 返回项列表供断言
	private static TestScrollUGUI createScroll(TestLayoutScriptDeep script, out List<TestScrollItem> items, out List<TestScrollContainer> containers, out List<myUGUIObject> allUI)
	{
		allUI = new List<myUGUIObject>();
		TestScrollUGUI scroll = new TestScrollUGUI(script);
		scroll.assignWindow(script.createUGUIObject<myUGUIObject>(null, "ScrollRoot", true));
		scroll.init();
		allUI.Add(scroll.getScrollRootForTest());
		// 容器: 5 个, 每个绑定真实节点
		containers = new List<TestScrollContainer>();
		for (int i = 0; i < 5; ++i)
		{
			myUGUIObject cRoot = script.createUGUIObject<myUGUIObject>(null, "Container" + i, true);
			allUI.Add(cRoot);
			containers.Add(new TestScrollContainer(cRoot));
		}
		// 项: 4 个, 每个绑定真实节点
		items = new List<TestScrollItem>();
		for (int i = 0; i < 4; ++i)
		{
			myUGUIObject iRoot = script.createUGUIObject<myUGUIObject>(null, "Item" + i, true);
			allUI.Add(iRoot);
			items.Add(new TestScrollItem(iRoot));
		}
		return scroll;
	}

	private static void destroyAllUI(List<myUGUIObject> allUI)
	{
		if (allUI == null)
		{
			return;
		}
		for (int i = allUI.Count - 1; i >= 0; --i)
		{
			myUGUIObject ui = allUI[i];
			if (ui != null)
			{
				LayoutScript.destroyObject(ref ui, true);
			}
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// init 默认值 + 全部 setter 往返
	// ═════════════════════════════════════════════════════════════════
	private static void testScrollInitAndDefaults()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo, out myUGUIObject scrollRoot);
		TestScrollUGUI scroll = null;
		try
		{
			scroll = new TestScrollUGUI(script);
			scroll.assignWindow(scrollRoot);
			scroll.init();
			assertTrue(SCROLL_STATE.NONE == scroll.getState(), "init 后状态 NONE");
			assertEqual(0.0f, scroll.getCurOffsetValue(), 0.001f, "init 后偏移 0");
			// setter 往返
			scroll.setDragDirection(DRAG_DIRECTION.VERTICAL);
			scroll.setLoop(true);
			scroll.setDragSensitive(2.5f);
			scroll.setFocusSpeedThreshold(3.0f);
			scroll.setAttenuateFactor(5.0f);
			scroll.setOnScrollItem(null);
			assertTrue(scroll.getDragDirectionForTest() == DRAG_DIRECTION.VERTICAL, "setDragDirection 读回");
			assertTrue(scroll.getLoopForTest(), "setLoop(true) 读回");
			assertEqual(2.5f, scroll.getDragSensitiveForTest(), 0.001f, "setDragSensitive 读回");
			assertEqual(3.0f, scroll.getFocusSpeedThresholdForTest(), 0.001f, "setFocusSpeedThreshold 读回");
			assertEqual(5.0f, scroll.getAttenuateFactorForTest(), 0.001f, "setAttenuateFactor 读回");
		}
		finally
		{
			scroll?.destroy();
			destroyUI(ref scrollRoot);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// initScroll: 容器列表 + mainFocus 计算
	private static void testScrollInitScrollMainFocus()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo, out myUGUIObject scrollRoot);
		TestScrollUGUI scroll = null;
		try
		{
			scroll = new TestScrollUGUI(script);
			scroll.assignWindow(scrollRoot);
			scroll.init();
			List<TestScrollContainer> containers = new List<TestScrollContainer>();
			for (int i = 0; i < 5; ++i)
			{
				containers.Add(new TestScrollContainer(script.createUGUIObject<myUGUIObject>(null, "C" + i, true)));
			}
			// mainContainer=-1 → 默认聚焦中间 (5>>1 = 2)
			scroll.initScroll(containers);
			assertEqual(5, scroll.getContainerCountForTest(), "initScroll 后容器数=5");
			assertEqual(2, scroll.getMainFocusForTest(), "mainContainer=-1 默认聚焦中间 (count>>1)");
			assertEqual(4.0f, scroll.getMaxContainerValueForTest(), 0.001f, "mMaxContainerValue=count-1");
			// 指定 mainContainer
			scroll.initScroll(containers, 3);
			assertEqual(3, scroll.getMainFocusForTest(), "指定 mainContainer=3");
			// 空容器: 不崩溃, mMaxContainerValue 保持
			scroll.initScroll(new List<TestScrollContainer>());
			assertEqual(0, scroll.getContainerCountForTest(), "空容器 initScroll 后容器数=0");
		}
		finally
		{
			scroll?.destroy();
			destroyUI(ref scrollRoot);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// setItemList: 项列表 + defaultIndex 定位 + 滚动回调
	private static void testScrollSetItemList()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo, out myUGUIObject scrollRoot);
		TestScrollUGUI scroll = null;
		try
		{
			scroll = new TestScrollUGUI(script);
			scroll.assignWindow(scrollRoot);
			scroll.init();
			List<TestScrollContainer> containers = new List<TestScrollContainer>();
			for (int i = 0; i < 5; ++i)
			{
				containers.Add(new TestScrollContainer(script.createUGUIObject<myUGUIObject>(null, "C" + i, true)));
			}
			scroll.initScroll(containers);
			List<TestScrollItem> items = new List<TestScrollItem>();
			for (int i = 0; i < 4; ++i)
			{
				items.Add(new TestScrollItem(script.createUGUIObject<myUGUIObject>(null, "I" + i, true)));
			}
			int callbackIndex = -1;
			IScrollItem callbackItem = null;
			scroll.setOnScrollItem((item, index) => { callbackItem = item; callbackIndex = index; });
			// defaultIndex=0 → 定位到第 0 项并触发回调
			scroll.setItemList(items, 0);
			assertEqual(4, scroll.getItemCount(), "setItemList 后项数=4");
			assertEqual(3.0f, scroll.getMaxControlValueForTest(), 0.001f, "非循环 mMaxControlValue=count-1");
			assertEqual(0, callbackIndex, "defaultIndex 触发滚动回调 index=0");
			assertTrue(ReferenceEquals(items[0], callbackItem), "defaultIndex 触发滚动回调 item[0]");
			// offset = mainFocus - 0 = 2
			assertEqual(2.0f, scroll.getCurOffsetValue(), 0.001f, "scrollToIndex(0) 后 offset=mainFocus");
			assertEqual(0, scroll.getNearIndex(), "offset=2 时最近项=0");
			// 空列表: mMaxControlValue=0, 不触发回调
			scroll.setItemList(new List<TestScrollItem>());
			assertEqual(0.0f, scroll.getMaxControlValueForTest(), 0.001f, "空项列表 mMaxControlValue=0");
			assertEqual(0, scroll.getItemCount(), "空项列表项数=0");
		}
		finally
		{
			scroll?.destroy();
			destroyUI(ref scrollRoot);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// scroll: 直接定位 + 越界 clamp + checkValueRange 开关
	private static void testScrollScrollClamp()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo, out myUGUIObject scrollRoot);
		TestScrollUGUI scroll = null;
		try
		{
			scroll = new TestScrollUGUI(script);
			scroll.assignWindow(scrollRoot);
			scroll.init();
			List<TestScrollContainer> containers = new List<TestScrollContainer>();
			for (int i = 0; i < 5; ++i)
			{
				containers.Add(new TestScrollContainer(script.createUGUIObject<myUGUIObject>(null, "C" + i, true)));
			}
			scroll.initScroll(containers);
			List<TestScrollItem> items = new List<TestScrollItem>();
			for (int i = 0; i < 4; ++i)
			{
				items.Add(new TestScrollItem(script.createUGUIObject<myUGUIObject>(null, "I" + i, true)));
			}
			scroll.setItemList(items, 0);
			// 中间值: offset = 2 - controlValue
			scroll.scroll(1.0f);
			assertEqual(1.0f, scroll.getCurOffsetValue(), 0.001f, "scroll(1) → offset=1");
			// 合法范围 [0,3]
			scroll.scroll(3.0f);
			assertEqual(-1.0f, scroll.getCurOffsetValue(), 0.001f, "scroll(3) → offset=-1");
			// 越界: 上溢 clamp 到最大项(index 3 → offset=-1)
			scroll.scroll(5.0f);
			assertEqual(-1.0f, scroll.getCurOffsetValue(), 0.001f, "scroll(5) 越界 clamp 到 offset=-1(最大项)");
			// 越界: 下溢 clamp 到最小项(index 0 → offset=2)
			scroll.scroll(-1.0f);
			assertEqual(2.0f, scroll.getCurOffsetValue(), 0.001f, "scroll(-1) 越界 clamp 到 offset=2(最小项)");
			// checkValueRange=false: 不做 clamp, 直接 offset = mainFocus - controlValue
			scroll.scroll(5.0f, false);
			assertEqual(-3.0f, scroll.getCurOffsetValue(), 0.001f, "scroll(5,false) 不 clamp → offset=-3");
		}
		finally
		{
			scroll?.destroy();
			destroyUI(ref scrollRoot);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// scrollToIndex: 越界 clamp + 回调携带正确下标
	private static void testScrollToIndex()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo, out myUGUIObject scrollRoot);
		TestScrollUGUI scroll = null;
		try
		{
			scroll = new TestScrollUGUI(script);
			scroll.assignWindow(scrollRoot);
			scroll.init();
			List<TestScrollContainer> containers = new List<TestScrollContainer>();
			for (int i = 0; i < 5; ++i)
			{
				containers.Add(new TestScrollContainer(script.createUGUIObject<myUGUIObject>(null, "C" + i, true)));
			}
			scroll.initScroll(containers);
			List<TestScrollItem> items = new List<TestScrollItem>();
			for (int i = 0; i < 4; ++i)
			{
				items.Add(new TestScrollItem(script.createUGUIObject<myUGUIObject>(null, "I" + i, true)));
			}
			scroll.setItemList(items);
			int callbackIndex = -1;
			scroll.setOnScrollItem((item, index) => callbackIndex = index);
			scroll.scrollToIndex(2);
			assertEqual(2, callbackIndex, "scrollToIndex(2) 回调 index=2");
			assertEqual(2, scroll.getNearIndex(), "scrollToIndex(2) 后最近项=2");
			assertEqual(0.0f, scroll.getCurOffsetValue(), 0.001f, "scrollToIndex(2) → offset=mainFocus-2=0");
			// 越界 clamp 到 [0,3]
			scroll.scrollToIndex(99);
			assertEqual(3, callbackIndex, "scrollToIndex(99) clamp 到 3");
			assertEqual(-1.0f, scroll.getCurOffsetValue(), 0.001f, "scrollToIndex(99) → offset=-1");
			scroll.scrollToIndex(-5);
			assertEqual(0, callbackIndex, "scrollToIndex(-5) clamp 到 0");
			assertEqual(2.0f, scroll.getCurOffsetValue(), 0.001f, "scrollToIndex(-5) → offset=2");
			// 空项列表: 直接 return, 不触发回调, callbackIndex 保持上一步的值(0)
			scroll.setItemList(new List<TestScrollItem>());
			scroll.scrollToIndex(1);
			assertEqual(0, callbackIndex, "空项列表 scrollToIndex 不触发回调, 回调保持上次值");
		}
		finally
		{
			scroll?.destroy();
			destroyUI(ref scrollRoot);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// scrollToIndexWithTime: 定时滚动状态机 + update 逼近 + 到达停止
	private static void testScrollToIndexWithTime()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo, out myUGUIObject scrollRoot);
		TestScrollUGUI scroll = null;
		try
		{
			scroll = new TestScrollUGUI(script);
			scroll.assignWindow(scrollRoot);
			scroll.init();
			List<TestScrollContainer> containers = new List<TestScrollContainer>();
			for (int i = 0; i < 5; ++i)
			{
				containers.Add(new TestScrollContainer(script.createUGUIObject<myUGUIObject>(null, "C" + i, true)));
			}
			scroll.initScroll(containers);
			List<TestScrollItem> items = new List<TestScrollItem>();
			for (int i = 0; i < 4; ++i)
			{
				items.Add(new TestScrollItem(script.createUGUIObject<myUGUIObject>(null, "I" + i, true)));
			}
			scroll.setItemList(items, 0);   // offset=2
			int callbackIndex = -1;
			scroll.setOnScrollItem((item, index) => callbackIndex = index);
			// 从 offset=2 滚动到 index=3 → target offset = 2-3 = -1
			scroll.scrollToIndexWithTime(3, 0.1f);
			assertTrue(SCROLL_STATE.SCROLL_TO_TARGET == scroll.getState(), "scrollToIndexWithTime 后状态 SCROLL_TO_TARGET");
			assertEqual(-1.0f, scroll.getTargetOffsetValueForTest(), 0.001f, "target offset = mainFocus-3 = -1");
			assertEqual(-30.0f, scroll.getScrollSpeedForTest(), 0.001f, "speed = (target-cur)/time = (-1-2)/0.1 = -30");
			assertEqual(3, callbackIndex, "scrollToIndexWithTime 触发回调 index=3");
			// update 逐帧逼近: 3 次 0.05s 后到达(-1), 状态回到 NONE
			for (int i = 0; i < 5; ++i)
			{
				scroll.update(0.05f);
			}
			assertTrue(SCROLL_STATE.NONE == scroll.getState(), "到达目标后状态回到 NONE");
			assertEqual(-1.0f, scroll.getCurOffsetValue(), 0.001f, "到达后 offset=-1");
			// 空项列表: 直接 return, 状态不变
			scroll.setItemList(new List<TestScrollItem>());
			scroll.scrollToIndexWithTime(1, 0.1f);
			assertTrue(SCROLL_STATE.NONE == scroll.getState(), "空项列表不进入滚动状态");
		}
		finally
		{
			scroll?.destroy();
			destroyUI(ref scrollRoot);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// lerpToTarget: 曲线插值聚焦 + update 到达停止
	private static void testLerpToTarget()
	{
		// 曲线依赖 mKeyFrameManager 全局单例(测试环境已初始化)
		if (mKeyFrameManager == null)
		{
			return;
		}
		TestLayoutScriptDeep script = createScript(out GameObject rootGo, out myUGUIObject scrollRoot);
		TestScrollUGUI scroll = null;
		try
		{
			scroll = new TestScrollUGUI(script);
			scroll.assignWindow(scrollRoot);
			scroll.init();
			List<TestScrollContainer> containers = new List<TestScrollContainer>();
			for (int i = 0; i < 5; ++i)
			{
				containers.Add(new TestScrollContainer(script.createUGUIObject<myUGUIObject>(null, "C" + i, true)));
			}
			scroll.initScroll(containers);
			List<TestScrollItem> items = new List<TestScrollItem>();
			for (int i = 0; i < 4; ++i)
			{
				items.Add(new TestScrollItem(script.createUGUIObject<myUGUIObject>(null, "I" + i, true)));
			}
			scroll.setItemList(items, 0);   // offset=2
			int callbackIndex = -1;
			scroll.setOnScrollItem((item, index) => callbackIndex = index);
			scroll.lerpToTarget(3);
			assertTrue(SCROLL_STATE.LERP_SCROLL_TO_TARGET == scroll.getState(), "lerpToTarget 后状态 LERP_SCROLL_TO_TARGET");
			assertEqual(-1.0f, scroll.getTargetOffsetValueForTest(), 0.001f, "lerp 目标 offset=-1");
			assertEqual(3, callbackIndex, "lerpToTarget 触发回调 index=3");
			// update(mScrollToTargetMaxTime=0.2): 曲线 evaluate(1.0)=1.0 → lerp(start,target,1)=target → 到达停止
			scroll.update(0.2f);
			assertTrue(SCROLL_STATE.NONE == scroll.getState(), "插值完成(percent=1)后状态回到 NONE");
			assertEqual(-1.0f, scroll.getCurOffsetValue(), 0.001f, "插值完成后 offset=-1");
		}
		finally
		{
			scroll?.destroy();
			destroyUI(ref scrollRoot);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// getNearIndex: 各控制值下的最近下标(含中点边界)
	private static void testGetNearIndex()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo, out myUGUIObject scrollRoot);
		TestScrollUGUI scroll = null;
		try
		{
			scroll = new TestScrollUGUI(script);
			scroll.assignWindow(scrollRoot);
			scroll.init();
			List<TestScrollContainer> containers = new List<TestScrollContainer>();
			for (int i = 0; i < 5; ++i)
			{
				containers.Add(new TestScrollContainer(script.createUGUIObject<myUGUIObject>(null, "C" + i, true)));
			}
			scroll.initScroll(containers);
			List<TestScrollItem> items = new List<TestScrollItem>();
			for (int i = 0; i < 4; ++i)
			{
				items.Add(new TestScrollItem(script.createUGUIObject<myUGUIObject>(null, "I" + i, true)));
			}
			scroll.setItemList(items);
			// scroll(controlValue): offset = 2 - controlValue; getNearIndex = getItemIndex(controlValue, nearest)
			scroll.scroll(0.0f);
			assertEqual(0, scroll.getNearIndex(), "controlValue=0 → 最近项 0");
			scroll.scroll(0.4f);
			assertEqual(0, scroll.getNearIndex(), "controlValue=0.4 → 离 0 更近(0.4<0.6) → 0");
			scroll.scroll(1.4f);
			assertEqual(1, scroll.getNearIndex(), "controlValue=1.4 → 离 1 更近(0.4<0.6) → 1");
			scroll.scroll(2.5f);
			assertEqual(2, scroll.getNearIndex(), "controlValue=2.5 → 中点两侧距离相等(0.5>=0.5) → 偏小下标 2");
			scroll.scroll(3.0f);
			assertEqual(3, scroll.getNearIndex(), "controlValue=3 → 最近项 3");
			// 空项列表: getItemIndex 返回 -1
			scroll.setItemList(new List<TestScrollItem>());
			assertEqual(-1, scroll.getNearIndex(), "空项列表 getNearIndex=-1");
		}
		finally
		{
			scroll?.destroy();
			destroyUI(ref scrollRoot);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// 拖拽状态机: down→DRAGING / move 速度计算 / stay 清零 / up→SCROLL_TO_STOP / stop
	private static void testDragStateMachine()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo, out myUGUIObject scrollRoot);
		TestScrollUGUI scroll = null;
		try
		{
			scroll = new TestScrollUGUI(script);
			scroll.assignWindow(scrollRoot);
			scroll.init();
			List<TestScrollContainer> containers = new List<TestScrollContainer>();
			for (int i = 0; i < 5; ++i)
			{
				containers.Add(new TestScrollContainer(script.createUGUIObject<myUGUIObject>(null, "C" + i, true)));
			}
			scroll.initScroll(containers);
			List<TestScrollItem> items = new List<TestScrollItem>();
			for (int i = 0; i < 4; ++i)
			{
				items.Add(new TestScrollItem(script.createUGUIObject<myUGUIObject>(null, "I" + i, true)));
			}
			scroll.setItemList(items, 0);   // offset=2
			// 未按下时 move 不生效
			scroll.mouseMoveForTest(Vector3.zero, new Vector3(-100.0f, 0.0f, 0.0f), 0.1f, 0);
			assertEqual(0.0f, scroll.getScrollSpeedForTest(), 0.001f, "未按下时 move 不改变速度");
			// 按下 → DRAGING
			scroll.mouseDownForTest(Vector3.zero, 0);
			assertTrue(scroll.isMouseDownForTest(), "mouseDown 后 mMouseDown=true");
			assertTrue(SCROLL_STATE.DRAGING == scroll.getState(), "mouseDown 后状态 DRAGING");
			// move: 水平方向 delta.x=-100, time=0.1 → speed = sign(100)*|−100/0.1|*1.0*0.01 = 10
			scroll.mouseMoveForTest(Vector3.zero, new Vector3(-100.0f, 0.0f, 0.0f), 0.1f, 0);
			assertEqual(10.0f, scroll.getScrollSpeedForTest(), 0.001f, "水平 move 速度=10");
			// update: DRAGING 分支 offset 变化 = -dt*speed = -0.1*10 = -1 → 2→1
			scroll.update(0.1f);
			assertEqual(1.0f, scroll.getCurOffsetValue(), 0.001f, "拖拽中 update 偏移减 dt*speed");
			// stay: 速度清零
			scroll.mouseStayForTest(Vector3.zero, 0);
			assertEqual(0.0f, scroll.getScrollSpeedForTest(), 0.001f, "mouseStay 后速度清零");
			// 抬起 → SCROLL_TO_STOP
			scroll.mouseUpForTest(Vector3.zero, 0);
			assertFalse(scroll.isMouseDownForTest(), "mouseUp 后 mMouseDown=false");
			assertTrue(SCROLL_STATE.SCROLL_TO_STOP == scroll.getState(), "拖拽中抬起 → SCROLL_TO_STOP");
			// update: 速度衰减, 最终回到 NONE(速度低于阈值后 scrollToTarget 聚焦)
			for (int i = 0; i < 60; ++i)
			{
				scroll.update(0.1f);
			}
			assertTrue(SCROLL_STATE.NONE == scroll.getState(), "减速停止后状态回到 NONE");
			// stop: 强制停止
			scroll.mouseDownForTest(Vector3.zero, 0);
			scroll.stop();
			assertTrue(SCROLL_STATE.NONE == scroll.getState(), "stop 后状态 NONE");
			assertEqual(0.0f, scroll.getScrollSpeedForTest(), 0.001f, "stop 后速度清零");
		}
		finally
		{
			scroll?.destroy();
			destroyUI(ref scrollRoot);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// updateItem: 非循环模式下越界项隐藏 + 容器插值 lerpItem 调用
	private static void testUpdateItemLerp()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo, out myUGUIObject scrollRoot);
		TestScrollUGUI scroll = null;
		try
		{
			scroll = new TestScrollUGUI(script);
			scroll.assignWindow(scrollRoot);
			scroll.init();
			List<TestScrollContainer> containers = new List<TestScrollContainer>();
			for (int i = 0; i < 5; ++i)
			{
				containers.Add(new TestScrollContainer(script.createUGUIObject<myUGUIObject>(null, "C" + i, true)));
			}
			scroll.initScroll(containers);
			List<TestScrollItem> items = new List<TestScrollItem>();
			for (int i = 0; i < 4; ++i)
			{
				items.Add(new TestScrollItem(script.createUGUIObject<myUGUIObject>(null, "I" + i, true)));
			}
			scroll.setItemList(items, 0);   // offset=2
			// offset=2: item0(newControlValue=2)/item1(3)/item2(4) 在容器范围[0,4]内 → active + lerp
			//          item3(newControlValue=5) 越界 → inactive
			assertTrue(items[0].getItemRoot().isActive(), "offset=2 时 item0 激活");
			assertTrue(items[1].getItemRoot().isActive(), "offset=2 时 item1 激活");
			assertTrue(items[2].getItemRoot().isActive(), "offset=2 时 item2 激活");
			assertFalse(items[3].getItemRoot().isActive(), "offset=2 时 item3 越界隐藏");
			assertEqual(3, items[0].getLerpCount() + items[1].getLerpCount() + items[2].getLerpCount() + items[3].getLerpCount(), "4 项共 lerp 3 次(越界项不参与)");
			// item0: containerIndex=2 → lerpItem(container[2], container[3], 0)
			assertTrue(ReferenceEquals(containers[2], items[0].getLastCurContainer()), "item0 插值起点 container[2]");
			assertEqual(0.0f, items[0].getLastPercent(), 0.001f, "item0 恰在容器 2 上 percent=0");
			// item2: containerIndex=4, 无下一个容器 → lerpItem(container[4], container[4], 1.0)
			assertTrue(ReferenceEquals(containers[4], items[2].getLastCurContainer()), "item2 插值起点 container[4]");
			assertEqual(1.0f, items[2].getLastPercent(), 0.001f, "末尾容器 lerp percent=1.0");
		}
		finally
		{
			scroll?.destroy();
			destroyUI(ref scrollRoot);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// 销毁独立 UI 对象(非池内, 可立即销毁)
	private static void destroyUI(ref myUGUIObject ui)
	{
		if (ui != null)
		{
			LayoutScript.destroyObject(ref ui, true);
		}
	}
}

// ═════════════════════════════════════════════════════════════════
// 测试辅助: 暴露 UGUIScroll 的 protected 字段与拖拽链方法
// ═════════════════════════════════════════════════════════════════
public class TestScrollUGUI : UGUIScroll
{
	public TestScrollUGUI(IWindowObjectOwner parent) : base(parent) { }
	public void mouseDownForTest(Vector3 pos, int touchID) { onMouseDown(pos, touchID); }
	public void mouseUpForTest(Vector3 pos, int touchID) { onScreenMouseUp(pos, touchID); }
	public void mouseMoveForTest(Vector3 pos, Vector3 delta, float time, int touchID) { onMouseMove(pos, delta, time, touchID); }
	public void mouseStayForTest(Vector3 pos, int touchID) { onMouseStay(pos, touchID); }
	public bool isMouseDownForTest() { return mMouseDown; }
	public float getScrollSpeedForTest() { return mScrollSpeed; }
	public float getTargetOffsetValueForTest() { return mTargetOffsetValue; }
	public float getMaxControlValueForTest() { return mMaxControlValue; }
	public float getMaxContainerValueForTest() { return mMaxContainerValue; }
	public int getMainFocusForTest() { return mMainFocus; }
	public int getContainerCountForTest() { return mContainerList.Count; }
	public DRAG_DIRECTION getDragDirectionForTest() { return mDragDirection; }
	public bool getLoopForTest() { return mLoop; }
	public float getDragSensitiveForTest() { return mDragSensitive; }
	public float getFocusSpeedThresholdForTest() { return mFocusSpeedThreshold; }
	public float getAttenuateFactorForTest() { return mAttenuateFactor; }
	public myUGUIObject getScrollRootForTest() { return mRoot; }
}

// ═════════════════════════════════════════════════════════════════
// 测试辅助: 滚动容器实现(绑定真实 UI 节点)
// ═════════════════════════════════════════════════════════════════
public class TestScrollContainer : IScrollContainer
{
	private myUGUIObject mRoot;
	public TestScrollContainer(myUGUIObject root) { mRoot = root; }
	public myUGUIObject getContainerRoot() { return mRoot; }
}

// ═════════════════════════════════════════════════════════════════
// 测试辅助: 滚动项实现(记录插值调用, 绑定真实 UI 节点)
// ═════════════════════════════════════════════════════════════════
public class TestScrollItem : IScrollItem
{
	private myUGUIObject mRoot;
	private int mLerpCount;
	private float mLastPercent;
	private IScrollContainer mLastCur;
	public TestScrollItem(myUGUIObject root) { mRoot = root; }
	public void lerpItem(IScrollContainer curItem, IScrollContainer nextItem, float percent)
	{
		++mLerpCount;
		mLastCur = curItem;
		mLastPercent = percent;
	}
	public myUGUIObject getItemRoot() { return mRoot; }
	public int getLerpCount() { return mLerpCount; }
	public float getLastPercent() { return mLastPercent; }
	public IScrollContainer getLastCurContainer() { return mLastCur; }
}
