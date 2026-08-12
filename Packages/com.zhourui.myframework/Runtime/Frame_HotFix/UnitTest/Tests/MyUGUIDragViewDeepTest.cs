using UnityEngine;
using static TestAssert;

// myUGUIDragView 拖拽状态机深度测试
// 复杂调用链: init(完整 UI 环境) → onTouchDown → onTouchMove(状态机: 阈值/方向/尺寸/触点检测)
//             → onScreenTouchUp(释放回调 + 状态重置) + 全套 setter/getter
//
// 可测性分析(源码确认):
//   onTouchDown/onTouchMove/onScreenTouchUp/onTouchStay 全是纯逻辑(不依赖真实输入系统)
//   checkCanDrag: 尺寸检查(allowDragOnlyOverParentSize) + 方向角度检测(getAngleBetweenVector)
//                 + 互斥拖拽检测 + mDragging 状态切换 + start 回调(RefBoolCallback)
//   update 里的窗口位置变化依赖 mInputSystem.getTouchPoint → 设备依赖, 不测
//   autoClampPosition/autoResetPosition 依赖 mLayoutManager.getUIRoot() → 不测
//
// 环境: TestLayoutScriptDeep + GameLayout(setScript 注入, 满足 init 里 mLayout.getScript())
//       + myUGUICanvas(根) + viewport(200x100) + dragView(200x400, 内容高 > viewport 高)
// 清理: DestroyImmediate(rootGo) 连带销毁所有子节点(dragView/viewport 非池对象)
public static class MyUGUIDragViewDeepTest
{
	public static void Run()
	{
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
	private static myUGUIDragView createDragView(TestLayoutScriptDeep script, out myUGUIObject viewport)
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
			myUGUIDragView drag = createDragView(script, out viewport);
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
			myUGUIDragView drag = createDragView(script, out myUGUIObject viewport);
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
			myUGUIDragView drag = createDragView(script, out myUGUIObject viewport);
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
			myUGUIDragView drag = createDragView(script, out myUGUIObject viewport);
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
			myUGUIDragView drag = createDragView(script, out myUGUIObject viewport);
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
			myUGUIDragView drag = createDragView(script, out myUGUIObject viewport);
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
			myUGUIDragView drag = createDragView(script, out myUGUIObject viewport);
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
			myUGUIDragView drag = createDragView(script, out myUGUIObject viewport);
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
			myUGUIDragView drag = createDragView(script, out myUGUIObject viewport);
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
			myUGUIDragView drag = createDragView(script, out myUGUIObject viewport);
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
			myUGUIDragView drag = createDragView(script, out myUGUIObject viewport);
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
			myUGUIDragView drag = createDragView(script, out viewport);
			assertTrue(ReferenceEquals(viewport, drag.getViewport()), "getViewport 返回父节点");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}
}
