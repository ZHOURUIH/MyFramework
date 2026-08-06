using UnityEngine;
using System.Collections.Generic;
using static TestAssert;

// TouchPoint 单元测试 — 覆盖触摸点的按下/抬起/移动/双击/重置逻辑
public static class TouchPointTest
{
	public static void Run()
	{
		testResetPropertyDefaults();
		testPointDown();
		testUpdateMove();
		testPointUpClick();
		testPointUpDrag();
		testDoubleClick();
		testLateUpdateClear();
		testResetState();
		testMouseAndTouchID();
		testGetPositions();
	}

	// ─── resetProperty 默认值 ─────────────────────────────────────────
	private static void testResetPropertyDefaults()
	{
		var tp = new TouchPoint();
		assertEqual(0, tp.getTouchID(), "reset 后 touchID 为 0");
		assertFalse(tp.isMouse(), "reset 后 mouse 为 false");
		assertFalse(tp.isCurrentUp(), "reset 后 currentUp 为 false");
		assertFalse(tp.isCurrentDown(), "reset 后 currentDown 为 false");
		assertFalse(tp.isDoubleClick(), "reset 后 doubleClick 为 false");
		assertFalse(tp.isClick(), "reset 后 click 为 false");
		assertFalse(tp.isDown(), "reset 后 down 为 false");
		assertEqual(Vector3.zero, tp.getDownPosition(), "reset 后 downPosition 为 zero");
		assertEqual(Vector3.zero, tp.getCurPosition(), "reset 后 curPosition 为 zero");
		assertEqual(Vector3.zero, tp.getLastPosition(), "reset 后 lastPosition 为 zero");
		// getMoveDelta 返回 Vector3(内部字段为 Vector2)
		assertEqual(Vector3.zero, tp.getMoveDelta(), "reset 后 moveDelta 为 zero");
	}

	// ─── pointDown 按下 ───────────────────────────────────────────────
	private static void testPointDown()
	{
		var tp = new TouchPoint();
		var pos = new Vector3(10, 20, 0);
		tp.pointDown(pos);
		assertTrue(tp.isDown(), "pointDown 后 down 为 true");
		assertTrue(tp.isCurrentDown(), "pointDown 后 currentDown 为 true");
		assertEqual(pos, tp.getDownPosition(), "pointDown 记录按下位置");
		assertEqual(pos, tp.getCurPosition(), "pointDown 后当前位置为按下位置");
		assertEqual(pos, tp.getLastPosition(), "pointDown 后上一位置为按下位置");
	}

	// ─── update 移动 ──────────────────────────────────────────────────
	private static void testUpdateMove()
	{
		var tp = new TouchPoint();
		tp.pointDown(new Vector3(0, 0, 0));
		tp.update(new Vector3(5, 3, 0));
		assertEqual(new Vector3(0, 0, 0), tp.getLastPosition(), "update 后 lastPosition 为旧位置");
		assertEqual(new Vector3(5, 3, 0), tp.getCurPosition(), "update 后 curPosition 为新位置");
		// getMoveDelta 返回 Vector3
		assertEqual(new Vector3(5, 3, 0), tp.getMoveDelta(), "update 计算移动增量");
	}

	// ─── pointUp 有效点击(按下抬起同位置) ─────────────────────────────
	private static void testPointUpClick()
	{
		var tp = new TouchPoint();
		var deadList = new List<DeadClick>();
		tp.pointDown(new Vector3(5, 5, 0));
		tp.pointUp(new Vector3(5, 5, 0), deadList);
		assertTrue(tp.isClick(), "同位置按下抬起判定为有效点击");
		assertTrue(tp.isCurrentUp(), "pointUp 后 currentUp 为 true");
		assertFalse(tp.isDown(), "pointUp 后 down 为 false");
		assertFalse(tp.isDoubleClick(), "单次点击不是双击");
	}

	// ─── pointUp 拖动(远距离) ────────────────────────────────────────
	private static void testPointUpDrag()
	{
		var tp = new TouchPoint();
		var deadList = new List<DeadClick>();
		tp.pointDown(new Vector3(0, 0, 0));
		// 移动超过 CLICK_LENGTH(15) 的距离
		tp.pointUp(new Vector3(100, 0, 0), deadList);
		assertFalse(tp.isClick(), "位移过大不判定为点击");
		assertTrue(tp.isCurrentUp(), "仍记录 currentUp");
		assertFalse(tp.isDown(), "抬起后 down 为 false");
	}

	// ─── 双击判定 ─────────────────────────────────────────────────────
	private static void testDoubleClick()
	{
		var tp = new TouchPoint();
		var deadList = new List<DeadClick>();
		var pos = new Vector3(5, 5, 0);
		// 第一次点击
		tp.pointDown(pos);
		tp.pointUp(pos, deadList);
		// 记录这次单击,用于二次检测
		deadList.Add(new DeadClick(pos));
		// 第二次点击,位置与时间都相近 → 判定双击
		tp.pointDown(pos);
		tp.pointUp(pos, deadList);
		assertTrue(tp.isDoubleClick(), "两次相近点击判定为双击");
	}

	// ─── lateUpdate 清除本帧标志 ─────────────────────────────────────
	private static void testLateUpdateClear()
	{
		var tp = new TouchPoint();
		var deadList = new List<DeadClick>();
		tp.pointDown(new Vector3(5, 5, 0));
		tp.pointUp(new Vector3(5, 5, 0), deadList);
		assertTrue(tp.isCurrentUp(), "lateUpdate 前 currentUp 为 true");
		tp.lateUpdate();
		assertFalse(tp.isCurrentUp(), "lateUpdate 清除 currentUp");
		assertFalse(tp.isCurrentDown(), "lateUpdate 清除 currentDown");
		assertFalse(tp.isClick(), "lateUpdate 清除 click");
		assertFalse(tp.isDoubleClick(), "lateUpdate 清除 doubleClick");
	}

	// ─── resetState 重置交互状态 ──────────────────────────────────────
	private static void testResetState()
	{
		var tp = new TouchPoint();
		tp.pointDown(new Vector3(5, 5, 0));
		tp.resetState();
		assertFalse(tp.isDown(), "resetState 清除 down");
		assertFalse(tp.isCurrentUp(), "resetState 清除 currentUp");
		assertFalse(tp.isClick(), "resetState 清除 click");
		assertFalse(tp.isDoubleClick(), "resetState 清除 doubleClick");
	}

	// ─── setMouse / setTouchID ────────────────────────────────────────
	private static void testMouseAndTouchID()
	{
		var tp = new TouchPoint();
		tp.setMouse(true);
		assertTrue(tp.isMouse(), "setMouse(true) 后 isMouse 为 true");
		tp.setTouchID(7);
		assertEqual(7, tp.getTouchID(), "setTouchID 后 getTouchID 返回该值");
	}

	// ─── 位置 getter ──────────────────────────────────────────────────
	private static void testGetPositions()
	{
		var tp = new TouchPoint();
		tp.pointDown(new Vector3(1, 2, 3));
		tp.update(new Vector3(4, 5, 6));
		assertEqual(new Vector3(1, 2, 3), tp.getLastPosition(), "getLastPosition 返回上一帧位置");
		assertEqual(new Vector3(4, 5, 6), tp.getCurPosition(), "getCurPosition 返回当前位置");
		assertEqual(new Vector3(1, 2, 3), tp.getDownPosition(), "getDownPosition 返回按下位置");
		// getMoveDelta 返回 Vector3, 内部 Vector2 字段截断 z
		assertEqual(new Vector3(3, 3, 0), tp.getMoveDelta(), "getMoveDelta 返回移动增量");
	}
}
