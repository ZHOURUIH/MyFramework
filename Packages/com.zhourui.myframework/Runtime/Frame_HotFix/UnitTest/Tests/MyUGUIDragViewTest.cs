using System.IO;
using System;
using UnityEngine;
using static FrameBaseHotFix;
using static TestAssert;

// myUGUIDragView 深度测试(拖拽滑动窗口封装, 完整 init 链路):
//   ⚠️ 不继承 myUGUIDragView: BASE001 分析器强制 override init 必须调 base.init(),
//      而 myUGUIDragView.init 有布局依赖(mLayout.getScript().bindPassOnlyParent)
//      → 直接 new myUGUIDragView() + 注入真实 GameLayout + 父链, 让 base.init() 完整跑通
//   节点链: rootGo(Canvas) / Grand / Parent / DragView
//   setLayout(layout) 注入布局 / setParent(parent) 提供父链(父还需有自己的父)
//   setDragDirection/getDragDirection / setClampInner/isClampInner
//   setMaxRelativePos/getMaxRelativePos / setMinRelativePos/getMinRelativePos
//   setAllowDragOnlyOverParentSize/isAllowDragOnlyOverParentSize / setDragView 组合
//   onTouchDown/onTouchMove/onTouchStay/onScreenTouchUp 拖拽链路 / stopMoving
//   autoClampPosition/autoResetPosition/notifyParentSizeChange / getViewport
public static class MyUGUIDragViewTest
{
	private const string TEST_LAYOUT_PATH = "TestDragViewLayout.prefab";
	private const string TEST_LAYOUT_NAME = "TestDragViewLayout";
	private static readonly string sPrefabFile = "Assets/GameResources/" + TEST_LAYOUT_PATH;
	private static bool sRegistered;

	public static void Run()
	{
		try
		{
			if (!File.Exists(sPrefabFile))
			{
				File.WriteAllText(sPrefabFile, "");
			}
			if (!sRegistered)
			{
				mLayoutManager.registeLayout(typeof(TestDragViewLayout), TEST_LAYOUT_PATH, LAYOUT_LIFE_CYCLE.PERSIST, null);
				sRegistered = true;
			}
			testInitCreatesComponent();
			testDragDirection();
			testClampInner();
			testClampAndAllowFlags();
			testRelativePos();
			testSetDragViewCombo();
			testCallbackAndConfigSetters();
			testOnTouchChain();
			testStopMoving();
			testAutoClampReset();
			testNotifyParentSizeChange();
			testViewportAndReceive();
		}
		finally
		{
			sRegistered = false;
			if (File.Exists(sPrefabFile))
			{
				File.Delete(sPrefabFile);
			}
		}
	

		testInitEnvironment();
		testTouchDownThenUp();
		testMoveBelowThreshold();
		testMoveStartVerticalDrag();
		testMoveWrongDirection();
		testMoveSizeNotEnough();
		testMoveWrongTouchID();
		testTouchUpAfterDrag();
		testStartCallbackRef();
		testSettersGetters();
		testSetDragView();
		testGetViewport();
	}

	// ═════════════════════════════════════════════════════════════════
	// 辅助: 完整布局环境 + 节点链(Grand → Parent → DragView)
	// ═════════════════════════════════════════════════════════════════
	private static myUGUIDragView createDragView(out GameLayout layout, out GameObject rootGo, out GameObject parentGo, out GameObject grandGo)
	{
		rootGo = new GameObject(TEST_LAYOUT_NAME);
		rootGo.AddComponent<RectTransform>();
		rootGo.AddComponent<Canvas>();
		layout = new GameLayout();
		layout.setName(TEST_LAYOUT_NAME);
		layout.setType(typeof(TestDragViewLayout));
		layout.setParent(null);
		layout.init();

		// Grand 节点(父的父, bindPassOnlyParent 需要 parent.getParent().getDepth())
		grandGo = new GameObject("Grand");
		grandGo.AddComponent<RectTransform>();
		grandGo.transform.SetParent(rootGo.transform);   // 挂到布局根下, 便于统一销毁
		myUGUIObject grand = new myUGUIObject();
		grand.setObject(grandGo);
		grand.init();

		parentGo = new GameObject("Parent");
		parentGo.AddComponent<RectTransform>();
		myUGUIObject parent = new myUGUIObject();
		parent.setObject(parentGo);
		parent.init();
		parentGo.transform.SetParent(grandGo.transform);
		parent.setParent(grand, false);

		GameObject go = new GameObject("DragView");
		myUGUIDragView drag = new myUGUIDragView();
		drag.setObject(go);
		drag.setLayout(layout);          // 注入布局: base.init 的 mLayout.getScript() 依赖
		go.transform.SetParent(parentGo.transform);
		drag.setParent(parent, false);   // mParent 非 null
		drag.init();                     // 完整 init: 布局依赖全部满足
		return drag;
	}

	// 清理: 注销碰撞体 + 布局销毁 + 节点销毁
	private static void cleanupDragView(GameLayout layout, myUGUIDragView drag, GameObject rootGo)
	{
		mGlobalTouchSystem.unregisteCollider(drag);
		if (drag.getViewport() != null)
		{
			mGlobalTouchSystem.unregisteCollider(drag.getViewport());
		}
		layout.destroy();
		UnityEngine.Object.DestroyImmediate(rootGo);
	}

	// init(完整链路) 后组件已创建
	private static void testInitCreatesComponent()
	{
		myUGUIDragView drag = createDragView(out GameLayout layout, out GameObject rootGo, out GameObject parentGo, out GameObject grandGo);
		try
		{
			assertNotNull(drag.getDragViewComponent(), "init 后拖拽组件已创建");
			assertFalse(drag.isDragging(), "初始未在拖拽");
			assertTrue(ReferenceEquals(parentGo.transform, drag.getGameObject().transform.parent), "DragView 挂到 Parent 下");
		}
		finally
		{
			cleanupDragView(layout, drag, rootGo);
		}
	}

	// setDragDirection/getDragDirection 往返
	private static void testDragDirection()
	{
		myUGUIDragView drag = createDragView(out GameLayout layout, out GameObject rootGo, out GameObject parentGo, out GameObject grandGo);
		try
		{
			drag.setDragDirection(DRAG_DIRECTION.HORIZONTAL);
			assertTrue(DRAG_DIRECTION.HORIZONTAL == drag.getDragDirection(), "横向读回");
			drag.setDragDirection(DRAG_DIRECTION.VERTICAL);
			assertTrue(DRAG_DIRECTION.VERTICAL == drag.getDragDirection(), "纵向读回");
			drag.setDragDirection(DRAG_DIRECTION.FREE);
			assertTrue(DRAG_DIRECTION.FREE == drag.getDragDirection(), "自由读回");
		}
		finally
		{
			cleanupDragView(layout, drag, rootGo);
		}
	}

	// setClampInner/isClampInner 往返
	private static void testClampInner()
	{
		myUGUIDragView drag = createDragView(out GameLayout layout, out GameObject rootGo, out GameObject parentGo, out GameObject grandGo);
		try
		{
			drag.setClampInner(true);
			assertTrue(drag.isClampInner(), "setClampInner(true) 读回");
			drag.setClampInner(false);
			assertFalse(drag.isClampInner(), "setClampInner(false) 读回");
		}
		finally
		{
			cleanupDragView(layout, drag, rootGo);
		}
	}

	// setAllowDragOnlyOverParentSize 往返 + setClampInRange/setClampType 守卫
	private static void testClampAndAllowFlags()
	{
		myUGUIDragView drag = createDragView(out GameLayout layout, out GameObject rootGo, out GameObject parentGo, out GameObject grandGo);
		try
		{
			drag.setAllowDragOnlyOverParentSize(true);
			assertTrue(drag.isAllowDragOnlyOverParentSize(), "allowDragOnly=true 读回");
			drag.setAllowDragOnlyOverParentSize(false);
			assertFalse(drag.isAllowDragOnlyOverParentSize(), "allowDragOnly=false 读回");
			drag.setClampInRange(true);
			drag.setClampType(CLAMP_TYPE.CENTER_IN_RECT);
			drag.setClampType(CLAMP_TYPE.EDGE_IN_RECT);
		}
		finally
		{
			cleanupDragView(layout, drag, rootGo);
		}
	}

	// setMaxRelativePos/setMinRelativePos 读写
	private static void testRelativePos()
	{
		myUGUIDragView drag = createDragView(out GameLayout layout, out GameObject rootGo, out GameObject parentGo, out GameObject grandGo);
		try
		{
			Vector3 max = new Vector3(100.0f, 50.0f, 0.0f);
			drag.setMaxRelativePos(max);
			assertTrue(max == drag.getMaxRelativePos(), "maxRelativePos 读回");
			Vector3 min = new Vector3(-10.0f, 0.0f, 0.0f);
			drag.setMinRelativePos(min);
			assertTrue(min == drag.getMinRelativePos(), "minRelativePos 读回");
		}
		finally
		{
			cleanupDragView(layout, drag, rootGo);
		}
	}

	// setDragView 组合: 一次设置 5 个参数
	private static void testSetDragViewCombo()
	{
		myUGUIDragView drag = createDragView(out GameLayout layout, out GameObject rootGo, out GameObject parentGo, out GameObject grandGo);
		try
		{
			drag.setDragView(DRAG_DIRECTION.HORIZONTAL, 0.5f, true, false, true);
			assertTrue(DRAG_DIRECTION.HORIZONTAL == drag.getDragDirection(), "组合: 方向");
			assertTrue(drag.isClampInner(), "组合: clampInner");
			assertFalse(drag.isAllowDragOnlyOverParentSize(), "组合: allowDragOnly");
		}
		finally
		{
			cleanupDragView(layout, drag, rootGo);
		}
	}

	// 回调与配置 setter 守卫式
	private static void testCallbackAndConfigSetters()
	{
		myUGUIDragView drag = createDragView(out GameLayout layout, out GameObject rootGo, out GameObject parentGo, out GameObject grandGo);
		try
		{
			drag.setMoveSpeedScale(2.0f);
			drag.setDragViewStartCallback((ref bool b) => { });
			drag.setDraggingCallback(() => { });
			drag.setReleaseDragCallback(() => { });
			drag.setPositionChangeCallback(() => { });
			drag.setAutoMoveToEdge(true);
			drag.setDragLengthThreshold(5.0f);
			drag.setDragAngleThreshold(0.5f);
			drag.setAttenuateFactor(0.8f);
			drag.setAutoClampSpeed(3.0f);
			drag.setAlignTopOrLeft(true);
			// 全部守卫式调用不崩
		}
		finally
		{
			cleanupDragView(layout, drag, rootGo);
		}
	}

	// 拖拽链路: onTouchDown → onTouchMove(同 id 生效, 不同 id 忽略) → onScreenTouchUp
	private static void testOnTouchChain()
	{
		myUGUIDragView drag = createDragView(out GameLayout layout, out GameObject rootGo, out GameObject parentGo, out GameObject grandGo);
		try
		{
			drag.onTouchDown(new Vector3(10.0f, 20.0f, 0.0f), 1);
			drag.onTouchMove(new Vector3(11.0f, 20.0f, 0.0f), new Vector3(1.0f, 0.0f, 0.0f), 0.1f, 1);
			drag.onTouchMove(new Vector3(12.0f, 20.0f, 0.0f), new Vector3(1.0f, 0.0f, 0.0f), 0.1f, 2);   // 不同触点忽略
			drag.onTouchStay(new Vector3(12.0f, 20.0f, 0.0f), 1);
			drag.onScreenTouchUp(new Vector3(12.0f, 20.0f, 0.0f), 1);
			// 全链路不崩
		}
		finally
		{
			cleanupDragView(layout, drag, rootGo);
		}
	}

	// stopMoving 调用安全
	private static void testStopMoving()
	{
		myUGUIDragView drag = createDragView(out GameLayout layout, out GameObject rootGo, out GameObject parentGo, out GameObject grandGo);
		try
		{
			drag.stopMoving();
		}
		finally
		{
			cleanupDragView(layout, drag, rootGo);
		}
	}

	// autoClampPosition/autoResetPosition: 有父子链环境调用不崩
	private static void testAutoClampReset()
	{
		myUGUIDragView drag = createDragView(out GameLayout layout, out GameObject rootGo, out GameObject parentGo, out GameObject grandGo);
		try
		{
			drag.setSize(new Vector2(200.0f, 200.0f));
			drag.autoClampPosition();
			drag.autoResetPosition();
		}
		finally
		{
			cleanupDragView(layout, drag, rootGo);
		}
	}

	// notifyParentSizeChange + setSize 组合(触发 onWindowSizeChange)
	private static void testNotifyParentSizeChange()
	{
		myUGUIDragView drag = createDragView(out GameLayout layout, out GameObject rootGo, out GameObject parentGo, out GameObject grandGo);
		try
		{
			drag.notifyParentSizeChange();
			drag.setSize(new Vector2(100.0f, 100.0f));
		}
		finally
		{
			cleanupDragView(layout, drag, rootGo);
		}
	}

	// getViewport 返回父节点 + isReceiveScreenTouch 恒 true
	private static void testViewportAndReceive()
	{
		myUGUIDragView drag = createDragView(out GameLayout layout, out GameObject rootGo, out GameObject parentGo, out GameObject grandGo);
		try
		{
			assertTrue(drag.isReceiveScreenTouch(), "isReceiveScreenTouch 恒 true");
			assertTrue(ReferenceEquals(parentGo.transform, drag.getViewport().getGameObject().transform), "getViewport 返回父节点");
		}
		finally
		{
			cleanupDragView(layout, drag, rootGo);
		}
	}


	

	// ═════════════════════════════════════════════════════════════════
	// 辅助: 创建完整 UI 环境(根 Canvas + script + GameLayout)
	// ═════════════════════════════════════════════════════════════════
	private static TestLayoutScriptDeep createScript(out GameObject rootGo)
	{
		rootGo = new GameObject("DragViewRoot");
		rootGo.AddComponent<RectTransform>();
		rootGo.AddComponent<Canvas>();
		myUGUICanvas root = new myUGUICanvas();
		root.setObject(rootGo);
		root.init();
		TestLayoutScriptDeep script = new TestLayoutScriptDeep();
		GameLayout layout = new GameLayout();
		layout.setScript(script);   // myUGUIDragView.init 里 mLayout.getScript() 必须非 null
		script.setLayout(layout);
		script.setRoot(root);
		return script;
	}

	// 创建 dragView: viewport 200x100(有父 root), content 200x400(高 > viewport 高)
	private static myUGUIDragView createDragView_Deep(TestLayoutScriptDeep script, out myUGUIObject viewport)
	{
		GameObject vpGo = new GameObject("ViewportGO");
		vpGo.AddComponent<RectTransform>();
		viewport = new myUGUIObject();
		viewport.setObject(vpGo);
		viewport.init();
		viewport.setParent(script.getRoot(), false);
		viewport.setSize(new Vector2(200.0f, 100.0f));

		GameObject dragGo = new GameObject("ContentGO");
		dragGo.AddComponent<RectTransform>();
		myUGUIDragView drag = new myUGUIDragView();
		drag.setObject(dragGo);
		drag.setLayout(script.getLayout());
		drag.setParent(viewport, false);
		drag.init();
		drag.setSize(new Vector2(200.0f, 400.0f));   // 内容高 400 > viewport 高 100
		return drag;
	}

	// init: 完整环境初始化 + 默认配置
	private static void testInitEnvironment()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo);
		myUGUIObject viewport = null;
		try
		{
			myUGUIDragView drag = createDragView_Deep(script, out viewport);
			assertTrue(drag.getDragViewComponent() != null, "init 后拖拽组件已创建");
			assertTrue(drag.getDragDirection() == DRAG_DIRECTION.VERTICAL, "默认纵向拖拽");
			assertTrue(!drag.isClampInner(), "默认 clampInner=false");
			assertTrue(drag.isAllowDragOnlyOverParentSize(), "默认 allowDragOnlyOverParentSize=true");
			assertTrue(!drag.isDragging(), "init 后未在拖拽");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// onTouchDown → onScreenTouchUp: 释放回调触发 + 状态重置
	private static void testTouchDownThenUp()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo);
		try
		{
			myUGUIDragView drag = createDragView_Deep(script, out myUGUIObject viewport);
			int releaseCount = 0;
			drag.setReleaseDragCallback(() => ++releaseCount);
			drag.onTouchDown(new Vector3(0.0f, 0.0f, 0.0f), 0);
			drag.onScreenTouchUp(new Vector3(0.0f, 0.0f, 0.0f), 0);
			assertEqual(1, releaseCount, "按下后抬起触发释放回调");
			assertTrue(!drag.isDragging(), "抬起后未在拖拽");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// onTouchMove: 移动量低于阈值(10) → 不开始拖拽
	private static void testMoveBelowThreshold()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo);
		try
		{
			myUGUIDragView drag = createDragView_Deep(script, out myUGUIObject viewport);
			int startCount = 0;
			drag.setDragViewStartCallback((ref bool allow) => ++startCount);
			drag.onTouchDown(new Vector3(0.0f, 0.0f, 0.0f), 0);
			drag.onTouchMove(new Vector3(0.0f, 5.0f, 0.0f), new Vector3(0.0f, 5.0f, 0.0f), 0.1f, 0);
			assertTrue(!drag.isDragging(), "移动量 5 < 阈值 10 不开始拖拽");
			assertEqual(0, startCount, "未触发开始拖拽回调");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// onTouchMove: 垂直移动量超阈值 + 方向正确 → 开始拖拽 + start 回调
	private static void testMoveStartVerticalDrag()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo);
		try
		{
			myUGUIDragView drag = createDragView_Deep(script, out myUGUIObject viewport);
			int startCount = 0;
			drag.setDragViewStartCallback((ref bool allow) => ++startCount);
			drag.onTouchDown(new Vector3(0.0f, 0.0f, 0.0f), 0);
			drag.onTouchMove(new Vector3(0.0f, 15.0f, 0.0f), new Vector3(0.0f, 15.0f, 0.0f), 0.1f, 0);
			assertTrue(drag.isDragging(), "垂直移动 15 > 阈值且方向一致 → 开始拖拽");
			assertEqual(1, startCount, "开始拖拽回调触发");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// onTouchMove: 垂直方向拖拽但实际移动方向水平 → 角度检测拒绝
	private static void testMoveWrongDirection()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo);
		try
		{
			myUGUIDragView drag = createDragView_Deep(script, out myUGUIObject viewport);
			drag.onTouchDown(new Vector3(0.0f, 0.0f, 0.0f), 0);
			drag.onTouchMove(new Vector3(15.0f, 0.0f, 0.0f), new Vector3(15.0f, 0.0f, 0.0f), 0.1f, 0);
			assertTrue(!drag.isDragging(), "垂直拖拽方向但水平移动(夹角 90° > 45°) → 拒绝");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// onTouchMove: 内容尺寸小于父节点 + allowDragOnlyOverParentSize → 拒绝拖拽
	private static void testMoveSizeNotEnough()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo);
		try
		{
			myUGUIDragView drag = createDragView_Deep(script, out myUGUIObject viewport);
			drag.setSize(new Vector2(200.0f, 50.0f));   // 内容高 50 < viewport 高 100
			drag.onTouchDown(new Vector3(0.0f, 0.0f, 0.0f), 0);
			drag.onTouchMove(new Vector3(0.0f, 15.0f, 0.0f), new Vector3(0.0f, 15.0f, 0.0f), 0.1f, 0);
			assertTrue(!drag.isDragging(), "内容未超过父节点且 allowDragOnly=true → 拒绝拖拽");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// onTouchMove: 触点 ID 与按下时不符 → 直接忽略
	private static void testMoveWrongTouchID()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo);
		try
		{
			myUGUIDragView drag = createDragView_Deep(script, out myUGUIObject viewport);
			drag.onTouchDown(new Vector3(0.0f, 0.0f, 0.0f), 1);
			drag.onTouchMove(new Vector3(0.0f, 15.0f, 0.0f), new Vector3(0.0f, 15.0f, 0.0f), 0.1f, 2);
			assertTrue(!drag.isDragging(), "触点 ID 不一致 → 忽略移动");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// 拖拽中抬起: 停止拖拽 + 释放回调
	private static void testTouchUpAfterDrag()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo);
		try
		{
			myUGUIDragView drag = createDragView_Deep(script, out myUGUIObject viewport);
			int releaseCount = 0;
			drag.setReleaseDragCallback(() => ++releaseCount);
			drag.onTouchDown(new Vector3(0.0f, 0.0f, 0.0f), 0);
			drag.onTouchMove(new Vector3(0.0f, 15.0f, 0.0f), new Vector3(0.0f, 15.0f, 0.0f), 0.1f, 0);
			assertTrue(drag.isDragging(), "前置: 已开始拖拽");
			drag.onScreenTouchUp(new Vector3(0.0f, 15.0f, 0.0f), 0);
			assertTrue(!drag.isDragging(), "抬起后停止拖拽");
			assertEqual(1, releaseCount, "释放回调触发");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// start 回调: RefBoolCallback 收到 ref bool(允许拖拽标志)
	private static void testStartCallbackRef()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo);
		try
		{
			myUGUIDragView drag = createDragView_Deep(script, out myUGUIObject viewport);
			bool callbackCalled = false;
			bool allowValue = false;
			drag.setDragViewStartCallback((ref bool allow) => { callbackCalled = true; allowValue = allow; });
			drag.onTouchDown(new Vector3(0.0f, 0.0f, 0.0f), 0);
			drag.onTouchMove(new Vector3(0.0f, 15.0f, 0.0f), new Vector3(0.0f, 15.0f, 0.0f), 0.1f, 0);
			assertTrue(callbackCalled, "开始拖拽回调已触发");
			assertTrue(drag.isDragging(), "回调后进入拖拽状态");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// setter/getter 全套读回
	private static void testSettersGetters()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo);
		try
		{
			myUGUIDragView drag = createDragView_Deep(script, out myUGUIObject viewport);
			drag.setDragDirection(DRAG_DIRECTION.HORIZONTAL);
			assertTrue(drag.getDragDirection() == DRAG_DIRECTION.HORIZONTAL, "setDragDirection 读回");
			drag.setClampInner(true);
			assertTrue(drag.isClampInner(), "setClampInner(true) 读回");
			drag.setAllowDragOnlyOverParentSize(false);
			assertTrue(!drag.isAllowDragOnlyOverParentSize(), "setAllowDragOnlyOverParentSize(false) 读回");
			drag.setMaxRelativePos(new Vector3(0.5f, 0.8f, 0.0f));
			Vector3 max = drag.getMaxRelativePos();
			assertEqual(0.5f, max.x, 0.001f, "setMaxRelativePos x 读回");
			assertEqual(0.8f, max.y, 0.001f, "setMaxRelativePos y 读回");
			drag.setMinRelativePos(new Vector3(-0.5f, -0.8f, 0.0f));
			Vector3 min = drag.getMinRelativePos();
			assertEqual(-0.5f, min.x, 0.001f, "setMinRelativePos x 读回");
			assertEqual(-0.8f, min.y, 0.001f, "setMinRelativePos y 读回");
			// 无 getter 的 setter: 调用不崩溃(值写入组件)
			drag.setClampInRange(false);
			drag.setDragAngleThreshold(30.0f.toRadian());
			drag.setDragLengthThreshold(20.0f);
			drag.setAttenuateFactor(3.0f);
			drag.setMoveSpeedScale(0.5f);
			drag.setAutoMoveToEdge(true);
			drag.setAutoClampSpeed(5.0f);
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// setDragView: 一键配置所有拖拽参数
	private static void testSetDragView()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo);
		try
		{
			myUGUIDragView drag = createDragView_Deep(script, out myUGUIObject viewport);
			drag.setDragView(DRAG_DIRECTION.HORIZONTAL, 30.0f.toRadian(), true, false, true);
			assertTrue(drag.getDragDirection() == DRAG_DIRECTION.HORIZONTAL, "setDragView 方向");
			assertTrue(drag.isClampInner(), "setDragView clampInner");
			assertTrue(!drag.isAllowDragOnlyOverParentSize(), "setDragView allowDragOnly");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// getViewport: 返回父节点
	private static void testGetViewport()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo);
		myUGUIObject viewport = null;
		try
		{
			myUGUIDragView drag = createDragView_Deep(script, out viewport);
			assertTrue(ReferenceEquals(viewport, drag.getViewport()), "getViewport 返回父节点");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}
}

// 空布局脚本(为 myUGUIDragView.init 的 mLayout.getScript() 提供脚本实例)
public class TestDragViewLayout : LayoutScript
{
	public override void assignWindow()
	{
	}

	public new void resetProperty()
	{
		base.resetProperty();
	}
}
