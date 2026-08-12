using UnityEngine;
using static TestAssert;

// ComponentDrag 拖拽组件状态机深度测试
// 复杂调用链: 准备拖拽(threshold 阈值) → 方向检测(angleThreshold) → 开始拖拽(start 回调可拒绝)
//             → 拖动中(dragging 回调) → 结束(end 回调 cancel 标志) → 完全结束(endTotally 回调)
//
// 可测性分析(源码确认):
//   touchDown/touchMove/touchUp/notifyDragEndTotally 全是纯逻辑(不依赖真实输入系统)
//   getScreenPosition 基类默认返回 Vector3.zero(不依赖相机)
//   checkStartDrag 依赖 mouseInObject(基类默认 false)+mGlobalTouchSystem → 不测, 直接驱动 touchDown/touchMove
//   TouchPoint 可手动创建+update 设置位置(TouchPointTest 已验证)
//   mComponentOwner 为 null 时回调收到 null, 测试回调不引用它即可
//
// 环境: new TestComponentDrag()(无参构造, 不调 init) + 手动 TouchPoint
public static class ComponentDragDeepTest
{
	public static void Run()
	{
		testResetDefaults();
		testInitDrag();
		testTouchDownEntersPreparing();
		testMoveBelowThreshold();
		testMoveStartDrag();
		testDragStartRejected();
		testWrongDirectionRejected();
		testNoDirectionLimit();
		testDraggingCallback();
		testTouchUpEndsDrag();
		testTouchUpCancel();
		testTouchUpWhenNotDragging();
		testNotifyDragEndTotally();
		testCancelDrag();
		testDragMouseOffset();
		testSetters();
	}

	// ═════════════════════════════════════════════════════════════════
	// 辅助: 创建组件 + 触点(按下在原点, 移动到目标位置)
	// ═════════════════════════════════════════════════════════════════
	private static TestComponentDrag createDrag(out TouchPoint tp, Vector3 moveTarget)
	{
		TestComponentDrag drag = new TestComponentDrag();
		tp = new TouchPoint();
		tp.pointDown(new Vector3(0.0f, 0.0f, 0.0f));
		drag.setTouchPointForTest(tp);
		drag.touchDownForTest();          // mPrepareDragMousePosition = (0,0)
		tp.update(moveTarget);            // curPosition = moveTarget
		return drag;
	}

	// resetProperty 后的默认配置
	private static void testResetDefaults()
	{
		TestComponentDrag drag = new TestComponentDrag();
		assertTrue(!drag.isDragging(), "默认未拖拽");
		assertTrue(drag.isBreakDragWhenMultiTouch(), "默认多触点中断");
		assertEqual(20.0f, drag.getStartDragThresholdForTest(), 0.001f, "默认阈值 20");
		assertEqual(45.0f.toRadian(), drag.getDragStartAngleThresholdForTest(), 0.001f, "默认角度阈值 45°");
		assertTrue(drag.isMovableForTest(), "默认可移动");
		assertTrue(!drag.isCenterAlignForTest(), "默认不对齐鼠标中心");
		assertEqual(Vector2.zero, drag.getAllowDragDirectionForTest(), "默认不限制方向");
		assertTrue(drag.getTouchPoint() == null, "默认无触点");
	}

	// initDrag: 一键配置拖拽参数
	private static void testInitDrag()
	{
		TestComponentDrag drag = new TestComponentDrag();
		drag.initDrag(new Vector2(0.0f, 1.0f), 30.0f.toRadian(), true, false);
		assertEqual(0.0f, drag.getAllowDragDirectionForTest().x, 0.001f, "方向 x");
		assertEqual(1.0f, drag.getAllowDragDirectionForTest().y, 0.001f, "方向 y");
		assertEqual(30.0f.toRadian(), drag.getDragStartAngleThresholdForTest(), 0.001f, "角度阈值");
		assertTrue(drag.isCenterAlignForTest(), "中心对齐");
		assertTrue(!drag.isMovableForTest(), "不可移动");
	}

	// touchDown: 进入准备拖拽状态
	private static void testTouchDownEntersPreparing()
	{
		TestComponentDrag drag = new TestComponentDrag();
		TouchPoint tp = new TouchPoint();
		tp.pointDown(new Vector3(5.0f, 8.0f, 0.0f));
		drag.setTouchPointForTest(tp);
		drag.touchDownForTest();
		assertTrue(drag.isPreparingForTest(), "touchDown 后进入准备拖拽");
	}

	// touchMove: 移动量低于阈值 → 保持准备状态, 不开始拖拽
	private static void testMoveBelowThreshold()
	{
		TestComponentDrag drag = createDrag(out TouchPoint tp, new Vector3(0.0f, 5.0f, 0.0f));
		int startCount = 0;
		drag.setDragStartCallback((ComponentOwner owner, TouchPoint point, ref bool allow) => ++startCount);
		drag.touchMoveForTest();   // 移动 5 < 阈值 20
		assertTrue(!drag.isDragging(), "移动量 5 < 20 不开始拖拽");
		assertTrue(drag.isPreparingForTest(), "未超阈值保持准备状态");
		assertEqual(0, startCount, "start 回调未触发");
	}

	// touchMove: 移动量超阈值 → 开始拖拽 + start 回调
	private static void testMoveStartDrag()
	{
		TestComponentDrag drag = createDrag(out TouchPoint tp, new Vector3(0.0f, 30.0f, 0.0f));
		int startCount = 0;
		drag.setDragStartCallback((ComponentOwner owner, TouchPoint point, ref bool allow) => ++startCount);
		drag.touchMoveForTest();   // 移动 30 > 阈值 20
		assertTrue(drag.isDragging(), "移动量 30 > 20 开始拖拽");
		assertTrue(!drag.isPreparingForTest(), "开始拖拽后退出准备状态");
		assertEqual(1, startCount, "start 回调触发");
	}

	// start 回调: allowDrag=false 可拒绝拖拽
	private static void testDragStartRejected()
	{
		TestComponentDrag drag = createDrag(out TouchPoint tp, new Vector3(0.0f, 30.0f, 0.0f));
		drag.setDragStartCallback((ComponentOwner owner, TouchPoint point, ref bool allow) => allow = false);
		drag.touchMoveForTest();
		assertTrue(!drag.isDragging(), "start 回调拒绝后不拖拽");
	}

	// 方向限制: 允许纵向, 实际水平移动 → 夹角 90° ≥ 阈值 → 拒绝
	private static void testWrongDirectionRejected()
	{
		TestComponentDrag drag = createDrag(out TouchPoint tp, new Vector3(30.0f, 0.0f, 0.0f));
		drag.setAllowDragDirection(new Vector2(0.0f, 1.0f));
		int startCount = 0;
		drag.setDragStartCallback((ComponentOwner owner, TouchPoint point, ref bool allow) => ++startCount);
		drag.touchMoveForTest();   // 移动 (30,0) 水平, 与允许方向 (0,1) 夹角 90°
		assertTrue(!drag.isDragging(), "方向不符(夹角 90° ≥ 45°)拒绝拖拽");
		assertEqual(0, startCount, "start 回调未触发");
	}

	// 无方向限制: allowDragDirection 为 0 → 任意方向可拖
	private static void testNoDirectionLimit()
	{
		TestComponentDrag drag = createDrag(out TouchPoint tp, new Vector3(30.0f, 0.0f, 0.0f));
		// 默认 allowDragDirection = zero, 不限制方向
		drag.touchMoveForTest();
		assertTrue(drag.isDragging(), "无方向限制时水平移动也可拖拽");
	}

	// dragging 回调: 拖拽过程中触发(收到触点位置)
	private static void testDraggingCallback()
	{
		TestComponentDrag drag = createDrag(out TouchPoint tp, new Vector3(0.0f, 30.0f, 0.0f));
		int dragCount = 0;
		Vector3 lastPos = Vector3.zero;
		drag.setDraggingCallback((owner, pos) => { ++dragCount; lastPos = pos; });
		drag.touchMoveForTest();
		assertEqual(1, dragCount, "一次 touchMove(超阈值)触发 dragging 回调");
		assertEqual(0.0f, lastPos.x, 0.001f, "dragging 收到触点位置 x");
		assertEqual(30.0f, lastPos.y, 0.001f, "dragging 收到触点位置 y");
	}

	// touchUp(false): 结束拖拽 + end 回调(cancel=false)
	private static void testTouchUpEndsDrag()
	{
		TestComponentDrag drag = createDrag(out TouchPoint tp, new Vector3(0.0f, 30.0f, 0.0f));
		drag.touchMoveForTest();
		int endCount = 0;
		bool lastCancel = true;
		drag.setDragEndCallback((owner, pos, cancel) => { ++endCount; lastCancel = cancel; });
		drag.touchUpForTest(false);
		assertTrue(!drag.isDragging(), "touchUp 后停止拖拽");
		assertEqual(1, endCount, "end 回调触发");
		assertTrue(!lastCancel, "touchUp(false) 时 cancel=false");
	}

	// touchUp(true): end 回调 cancel=true
	private static void testTouchUpCancel()
	{
		TestComponentDrag drag = createDrag(out TouchPoint tp, new Vector3(0.0f, 30.0f, 0.0f));
		drag.touchMoveForTest();
		bool lastCancel = false;
		drag.setDragEndCallback((owner, pos, cancel) => lastCancel = cancel);
		drag.touchUpForTest(true);
		assertTrue(lastCancel, "touchUp(true) 时 cancel=true");
	}

	// 未拖拽时 touchUp: 无 end 回调
	private static void testTouchUpWhenNotDragging()
	{
		TestComponentDrag drag = new TestComponentDrag();
		int endCount = 0;
		drag.setDragEndCallback((owner, pos, cancel) => ++endCount);
		drag.touchUpForTest(false);
		assertEqual(0, endCount, "未拖拽时 touchUp 不触发 end 回调");
	}

	// notifyDragEndTotally: 完全结束回调
	private static void testNotifyDragEndTotally()
	{
		TestComponentDrag drag = new TestComponentDrag();
		int totallyCount = 0;
		bool lastCancel = false;
		drag.setDragEndTotallyCallback((owner, pos, cancel) => { ++totallyCount; lastCancel = cancel; });
		drag.notifyDragEndTotallyForTest(new Vector3(1.0f, 2.0f, 0.0f), true);
		assertEqual(1, totallyCount, "notifyDragEndTotally 触发");
		assertTrue(lastCancel, "cancel 标志传递");
	}

	// cancelDrag: 取消拖拽 → touchUp(true)
	private static void testCancelDrag()
	{
		TestComponentDrag drag = createDrag(out TouchPoint tp, new Vector3(0.0f, 30.0f, 0.0f));
		drag.touchMoveForTest();
		bool lastCancel = false;
		drag.setDragEndCallback((owner, pos, cancel) => lastCancel = cancel);
		drag.cancelDrag();
		assertTrue(!drag.isDragging(), "cancelDrag 停止拖拽");
		assertTrue(lastCancel, "cancelDrag 传 cancel=true");
	}

	// 拖拽偏移: centerAlign=false → offset = 触点位置 - 窗口位置(getScreenPosition 默认 zero)
	private static void testDragMouseOffset()
	{
		// centerAlign=false: offset = curPosition - zero = curPosition
		TestComponentDrag drag = createDrag(out TouchPoint tp, new Vector3(0.0f, 30.0f, 0.0f));
		drag.touchMoveForTest();
		Vector3 offset = drag.getDragMouseOffsetForTest();
		assertEqual(0.0f, offset.x, 0.001f, "offset x = 触点 x");
		assertEqual(30.0f, offset.y, 0.001f, "offset y = 触点 y(getScreenPosition 默认 zero)");

		// centerAlign=true: offset = zero
		TestComponentDrag dragCenter = createDrag(out TouchPoint tp2, new Vector3(0.0f, 30.0f, 0.0f));
		dragCenter.setObjectCenterAlignMouse(true);
		dragCenter.touchMoveForTest();
		assertEqual(Vector3.zero, dragCenter.getDragMouseOffsetForTest(), "centerAlign=true 时 offset 为 zero");
	}

	// setter 全套读回
	private static void testSetters()
	{
		TestComponentDrag drag = new TestComponentDrag();
		drag.setStartDragThreshold(35.0f);
		assertEqual(35.0f, drag.getStartDragThresholdForTest(), 0.001f, "setStartDragThreshold 读回");
		drag.setAllowDragDirection(new Vector2(1.0f, 0.0f));
		assertEqual(1.0f, drag.getAllowDragDirectionForTest().x, 0.001f, "setAllowDragDirection x");
		drag.setDragStartAngleThreshold(60.0f.toRadian());
		assertEqual(60.0f.toRadian(), drag.getDragStartAngleThresholdForTest(), 0.001f, "setDragStartAngleThreshold 读回");
		drag.setMovable(false);
		assertTrue(!drag.isMovableForTest(), "setMovable(false) 读回");
		drag.setObjectCenterAlignMouse(true);
		assertTrue(drag.isCenterAlignForTest(), "setObjectCenterAlignMouse(true) 读回");
		drag.setBreakDragWhenMultiTouch(false);
		assertTrue(!drag.isBreakDragWhenMultiTouch(), "setBreakDragWhenMultiTouch(false) 读回");
	}
}

// ═════════════════════════════════════════════════════════════════
// 测试辅助: 暴露 ComponentDrag 的 protected 字段与方法
// ═════════════════════════════════════════════════════════════════
public class TestComponentDrag : ComponentDrag
{
	public void setTouchPointForTest(TouchPoint tp) { mTouchPoint = tp; }

	public void touchDownForTest() { touchDown(); }

	public void touchMoveForTest() { touchMove(); }

	public void touchUpForTest(bool cancel) { touchUp(cancel); }

	public void notifyDragEndTotallyForTest(Vector3 pos, bool cancel) { notifyDragEndTotally(pos, cancel); }

	public bool isPreparingForTest() { return mPreparingDrag; }

	public Vector3 getDragMouseOffsetForTest() { return mDragMouseOffset; }

	public float getStartDragThresholdForTest() { return mStartDragThreshold; }

	public Vector2 getAllowDragDirectionForTest() { return mAllowDragDirection; }

	public float getDragStartAngleThresholdForTest() { return mDragStartAngleThreshold; }

	public bool isMovableForTest() { return mMovable; }

	public bool isCenterAlignForTest() { return mObjectCenterAlignMouse; }
}
