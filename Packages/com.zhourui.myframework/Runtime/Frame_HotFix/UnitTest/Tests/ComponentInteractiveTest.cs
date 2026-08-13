using System;
using UnityEngine;
using static TestAssert;

// ComponentInteractive 交互组件状态机深度测试
//   onTouchEnter/Leave/Down/Up: 纯逻辑触点状态机(悬停/按下/点击检测链)
//   update: 长按检测(mPressing && threshold<0 短路, 不访问输入系统)
//   addLongPress/removeLongPress/clearLongPress: 长按回调池管理
// 环境: new TestComponentInteractive()(GameComponent 子类直接 new)
// 测试辅助: TestComponentInteractive 暴露 protected 状态字段
public static class ComponentInteractiveTest
{
	public static void Run()
	{
		testSetterGetterRoundTrip();
		testCallbackStorage();
		testOnTouchEnterChain();
		testOnTouchLeaveChain();
		testOnTouchDownChain();
		testClickChain();
		testClickDistanceThreshold();
		testLongPressProgress();
		testLongPressComplete();
		testLongPressInterrupted();
		testAddLongPressDedup();
		testRemoveLongPressCleanup();
		testClearLongPress();
		testUpdateNotPressing();
	}

	// ═════════════════════════════════════════════════════════════════
	// 辅助
	// ═════════════════════════════════════════════════════════════════
	private static TestComponentInteractive createInteractive()
	{
		return new TestComponentInteractive();
	}

	// setter/getter 往返
	private static void testSetterGetterRoundTrip()
	{
		TestComponentInteractive touch = createInteractive();
		try
		{
			touch.setHandleInput(false);
			assertTrue(!touch.isHandleInput(), "setHandleInput(false)");
			touch.setPassRay(false);
			assertTrue(!touch.isPassRay(), "setPassRay(false)");
			touch.setPassDragEvent(true);
			assertTrue(touch.isPassDragEvent(), "setPassDragEvent(true)");
			touch.setClickSound(5);
			assertEqual(5, touch.getClickSound(), "setClickSound(5)");
			touch.setLongPressLengthThreshold(20.0f);
			assertEqual(20.0f, touch.getLongPressLengthThreshold(), 0.001f, "setLongPressLengthThreshold(20)");
			touch.setDepthOverAllChild(true);
			assertTrue(touch.isDepthOverAllChild(), "setDepthOverAllChild(true)");
			touch.setAllowGenerateDepth(false);
			assertTrue(!touch.isAllowGenerateDepth(), "setAllowGenerateDepth(false)");
			assertTrue(touch.isColliderForClick(), "isColliderForClick 默认 true");
			assertTrue(!touch.isReceiveScreenMouse(), "isReceiveScreenMouse 默认 false(无屏幕触点回调)");
			assertNotNull(touch.getDepth(), "getDepth 默认创建非 null");
		}
		finally
		{
			touch.destroy();
		}
	}

	// 回调存储: setter 后 getter 读回 + isReceiveScreenMouse 联动
	private static void testCallbackStorage()
	{
		TestComponentInteractive touch = createInteractive();
		try
		{
			int clickCount = 0;
			touch.setClickCallback(() => clickCount++);
			touch.setPreClickCallback(() => clickCount += 10);
			touch.setPressCallback((press) => { });
			touch.setHoverCallback((hover) => { });
			touch.setOnScreenTouchUp((pos, id) => { });
			assertTrue(touch.isReceiveScreenMouse(), "设置屏幕触点回调后 isReceiveScreenMouse true");
		}
		finally
		{
			touch.destroy();
		}
	}

	// onTouchEnter: 首次进入触发 hover(true) + onTouchEnter; 重复进入只触发 onTouchEnter
	private static void testOnTouchEnterChain()
	{
		TestComponentInteractive touch = createInteractive();
		try
		{
			int hoverCount = 0;
			int enterCount = 0;
			bool lastHover = false;
			touch.setHoverCallback((hover) => { hoverCount++; lastHover = hover; });
			touch.setOnTouchEnter((pos, id) => enterCount++);
			touch.onTouchEnter(new Vector3(10.0f, 0.0f, 0.0f), 0);
			assertTrue(touch.getMouseHoveredForTest(), "进入后悬停状态 true");
			assertEqual(1, hoverCount, "首次进入触发 hover 回调");
			assertTrue(lastHover, "hover 回调参数 true");
			assertEqual(1, enterCount, "onTouchEnter 回调触发");
			touch.onTouchEnter(new Vector3(20.0f, 0.0f, 0.0f), 0);
			assertEqual(1, hoverCount, "重复进入不重复触发 hover");
			assertEqual(2, enterCount, "重复进入仍触发 onTouchEnter");
		}
		finally
		{
			touch.destroy();
		}
	}

	// onTouchLeave: hover(false) + pressing 复位 + onTouchLeave
	private static void testOnTouchLeaveChain()
	{
		TestComponentInteractive touch = createInteractive();
		try
		{
			int hoverCount = 0;
			bool lastHover = true;
			int leaveCount = 0;
			touch.setHoverCallback((hover) => { hoverCount++; lastHover = hover; });
			touch.setOnTouchLeave((pos, id) => leaveCount++);
			touch.onTouchEnter(new Vector3(0.0f, 0.0f, 0.0f), 0);
			touch.onTouchDown(new Vector3(0.0f, 0.0f, 0.0f), 0);
			assertTrue(touch.getPressingForTest(), "按下后 pressing true");
			touch.onTouchLeave(new Vector3(0.0f, 0.0f, 0.0f), 0);
			assertTrue(!touch.getMouseHoveredForTest(), "离开后悬停 false");
			assertTrue(!touch.getPressingForTest(), "离开后 pressing 复位");
			assertEqual(2, hoverCount, "enter+leave 共 2 次 hover 回调");
			assertTrue(!lastHover, "leave 时 hover 参数 false");
			assertEqual(1, leaveCount, "onTouchLeave 回调触发");
		}
		finally
		{
			touch.destroy();
		}
	}

	// onTouchDown: press(true) + pressing 状态 + 长按 reset
	private static void testOnTouchDownChain()
	{
		TestComponentInteractive touch = createInteractive();
		try
		{
			bool lastPress = false;
			int pressCount = 0;
			touch.setPressCallback((press) => { pressCount++; lastPress = press; });
			touch.onTouchDown(new Vector3(5.0f, 8.0f, 0.0f), 3);
			assertTrue(touch.getPressingForTest(), "按下后 pressing true");
			assertEqual(0.0f, touch.getPressedTimeForTest(), 0.001f, "按下后计时从 0 开始");
			assertEqual(1, pressCount, "press 回调触发");
			assertTrue(lastPress, "press 回调参数 true");
		}
		finally
		{
			touch.destroy();
		}
	}

	// 点击链: down 后立即 up(距离 0 < 15, 时间 < 0.5s) → preClick + click
	private static void testClickChain()
	{
		TestComponentInteractive touch = createInteractive();
		try
		{
			int preClickCount = 0;
			int clickCount = 0;
			int pressCount = 0;
			bool lastPress = true;
			touch.setPreClickCallback(() => preClickCount++);
			touch.setClickCallback(() => clickCount++);
			touch.setPressCallback((press) => { pressCount++; lastPress = press; });
			touch.onTouchDown(new Vector3(0.0f, 0.0f, 0.0f), 0);
			touch.onTouchUp(new Vector3(0.0f, 0.0f, 0.0f), 0);
			assertEqual(1, preClickCount, "preClick 回调触发");
			assertEqual(1, clickCount, "click 回调触发");
			assertEqual(2, pressCount, "press 回调 2 次(按下+抬起)");
			assertTrue(!lastPress, "抬起时 press 参数 false");
			assertTrue(!touch.getPressingForTest(), "抬起后 pressing 复位");
		}
		finally
		{
			touch.destroy();
		}
	}

	// 点击距离阈值: 移动 100 > CLICK_LENGTH(15) → 无 click
	private static void testClickDistanceThreshold()
	{
		TestComponentInteractive touch = createInteractive();
		try
		{
			int clickCount = 0;
			touch.setClickCallback(() => clickCount++);
			touch.onTouchDown(new Vector3(0.0f, 0.0f, 0.0f), 0);
			touch.onTouchUp(new Vector3(100.0f, 0.0f, 0.0f), 0);
			assertEqual(0, clickCount, "移动超阈值不触发 click");
		}
		finally
		{
			touch.destroy();
		}
	}

	// 长按进度: mPressing=true + threshold<0(短路) → update 累加计时 → 进度回调
	private static void testLongPressProgress()
	{
		TestComponentInteractive touch = createInteractive();
		try
		{
			int pressCount = 0;
			bool longPressed = false;
			float lastProgress = -1.0f;
			touch.setPressCallback((press) => pressCount++);
			touch.addLongPress(() => longPressed = true, 0.1f, (progress) => lastProgress = progress);
			touch.onTouchDown(new Vector3(0.0f, 0.0f, 0.0f), 0);
			touch.update(0.05f);
			assertEqual(0.05f, touch.getPressedTimeForTest(), 0.001f, "update 累加计时 0.05");
			assertEqual(0.5f, lastProgress, 0.001f, "进度回调 = 0.05/0.1 = 0.5");
			assertTrue(!longPressed, "未达阈值不触发长按");
			touch.update(0.03f);
			assertEqual(0.8f, lastProgress, 0.001f, "进度回调 = 0.08/0.1 = 0.8");
		}
		finally
		{
			touch.destroy();
		}
	}

	// 长按完成: 累计计时 >= 阈值 → 长按回调 + mFinish
	private static void testLongPressComplete()
	{
		TestComponentInteractive touch = createInteractive();
		try
		{
			int longPressCount = 0;
			float lastProgress = -1.0f;
			touch.addLongPress(() => longPressCount++, 0.1f, (progress) => lastProgress = progress);
			touch.onTouchDown(new Vector3(0.0f, 0.0f, 0.0f), 0);
			touch.update(0.05f);
			touch.update(0.06f);   // 累计 0.11 >= 0.1
			assertEqual(1, longPressCount, "达到阈值触发长按回调");
			assertEqual(1.0f, lastProgress, 0.001f, "进度夹到 1.0");
			touch.update(0.05f);
			assertEqual(1, longPressCount, "mFinish 后不再触发");
		}
		finally
		{
			touch.destroy();
		}
	}

	// 长按中断: onTouchLeave → pressedTime=-1 → update → mFinish + 进度 0
	private static void testLongPressInterrupted()
	{
		TestComponentInteractive touch = createInteractive();
		try
		{
			int longPressCount = 0;
			float lastProgress = -1.0f;
			touch.addLongPress(() => longPressCount++, 0.1f, (progress) => lastProgress = progress);
			touch.onTouchDown(new Vector3(0.0f, 0.0f, 0.0f), 0);
			touch.update(0.05f);
			assertEqual(0.5f, lastProgress, 0.001f, "中断前进度 0.5");
			touch.onTouchLeave(new Vector3(0.0f, 0.0f, 0.0f), 0);
			assertEqual(-1.0f, touch.getPressedTimeForTest(), 0.001f, "离开后计时复位 -1");
			touch.update(0.05f);
			assertEqual(0, longPressCount, "中断后不触发长按回调");
			assertEqual(0.0f, lastProgress, 0.001f, "中断回调进度 0");
		}
		finally
		{
			touch.destroy();
		}
	}

	// addLongPress 去重: 相同回调不重复添加
	private static void testAddLongPressDedup()
	{
		TestComponentInteractive touch = createInteractive();
		try
		{
			Action callback = () => { };
			touch.addLongPress(callback, 0.1f);
			touch.addLongPress(callback, 0.2f);
			assertEqual(1, touch.getLongPressCountForTest(), "相同回调去重");
			touch.addLongPress(() => { }, 0.1f);
			assertEqual(2, touch.getLongPressCountForTest(), "不同回调可添加");
		}
		finally
		{
			touch.destroy();
		}
	}

	// removeLongPress: 置 null 后 update 清理列表
	private static void testRemoveLongPressCleanup()
	{
		TestComponentInteractive touch = createInteractive();
		try
		{
			Action callback = () => { };
			touch.addLongPress(callback, 0.1f);
			touch.addLongPress(() => { }, 0.1f);
			assertEqual(2, touch.getLongPressCountForTest(), "添加 2 个");
			touch.removeLongPress(callback);
			touch.update(0.05f);   // 未按下, 但触发 null 清理
			assertEqual(1, touch.getLongPressCountForTest(), "update 后 null 项被清理");
		}
		finally
		{
			touch.destroy();
		}
	}

	// clearLongPress: 全清
	private static void testClearLongPress()
	{
		TestComponentInteractive touch = createInteractive();
		try
		{
			touch.addLongPress(() => { }, 0.1f);
			touch.addLongPress(() => { }, 0.2f);
			touch.clearLongPress();
			assertEqual(0, touch.getLongPressCountForTest(), "clearLongPress 清空");
		}
		finally
		{
			touch.destroy();
		}
	}

	// update 未按下: mPressing=false → 计时复位 -1, 无回调
	private static void testUpdateNotPressing()
	{
		TestComponentInteractive touch = createInteractive();
		try
		{
			int pressCount = 0;
			float lastProgress = -1.0f;
			bool longPressed = false;
			touch.setPressCallback((press) => pressCount++);
			touch.addLongPress(() => longPressed = true, 0.1f, (progress) => lastProgress = progress);
			touch.update(0.5f);
			assertEqual(-1.0f, touch.getPressedTimeForTest(), 0.001f, "未按下计时保持 -1");
			assertEqual(0, pressCount, "未按下无 press 回调");
			assertTrue(!longPressed, "未按下不触发长按");
		}
		finally
		{
			touch.destroy();
		}
	}
}

// 测试辅助: 暴露 protected 状态字段
public class TestComponentInteractive : ComponentInteractive
{
	public void setPressingForTest(bool pressing) { mPressing = pressing; }
	public bool getPressingForTest() { return mPressing; }
	public float getPressedTimeForTest() { return mPressedTime; }
	public bool getMouseHoveredForTest() { return mMouseHovered; }
	public int getLongPressCountForTest() { return mLongPressList.Count; }
}
