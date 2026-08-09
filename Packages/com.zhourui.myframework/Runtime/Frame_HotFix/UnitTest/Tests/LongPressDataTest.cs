using System;
using static TestAssert;

// LongPressData 单元测试
// 纯逻辑长按状态机(继承 ClassObject), 可覆盖:
//   update 无回调时空安全
//   pressedTime<0 中断 → mFinish + mOnLongPressing(0)
//   未达阈值 → mOnLongPressing(progress<1), 不触发长按, 不 finish
//   已达阈值 → mOnLongPress 触发 + mFinish
//   进度 clampMax 1
//   mFinish 后跳过
//   reset() 重置 mFinish
public static class LongPressDataTest
{
	public static void Run()
	{
		testNoCallbacks();
		testInterrupt();
		testProgressNotReached();
		testLongPressTriggered();
		testProgressClamp();
		testFinishSkips();
		testReset();
	}

	// ═════════════════════════════════════════════════════════════════
	// update 无回调 / 中断
	// ═════════════════════════════════════════════════════════════════
	private static void testNoCallbacks()
	{
		LongPressData data = new();
		// 无回调时 update 直接 return, 不抛异常
		data.mLongPressTime = 1.0f;
		data.update(0.5f);
		assertFalse(data.mFinish, "无回调时 update 不改变 finish");
		data.resetProperty();
	}
	private static void testInterrupt()
	{
		LongPressData data = new();
		data.mLongPressTime = 1.0f;
		float progress = -1.0f;
		data.mOnLongPressing = p => progress = p;
		int pressCalls = 0;
		data.mOnLongPress = () => ++pressCalls;
		// pressedTime < 0 表示中断
		data.update(-1.0f);
		assertTrue(data.mFinish, "中断后 mFinish 应为 true");
		assertEqual(0.0f, progress, 0.001f, "中断时 mOnLongPressing(0.0f)");
		assertEqual(0, pressCalls, "中断不触发长按");
		data.resetProperty();
	}

	// ═════════════════════════════════════════════════════════════════
	// 未达阈值 / 已达阈值
	// ═════════════════════════════════════════════════════════════════
	private static void testProgressNotReached()
	{
		LongPressData data = new();
		data.mLongPressTime = 2.0f;
		float progress = 0.0f;
		data.mOnLongPressing = p => progress = p;
		int pressCalls = 0;
		data.mOnLongPress = () => ++pressCalls;
		// pressedTime=1.0, 阈值=2.0 → progress=0.5
		data.update(1.0f);
		assertEqual(0.5f, progress, 0.001f, "未达阈值时进度 = pressedTime/阈值 = 0.5");
		assertEqual(0, pressCalls, "未达阈值不触发长按");
		assertFalse(data.mFinish, "未达阈值不 finish");
		data.resetProperty();
	}
	private static void testLongPressTriggered()
	{
		LongPressData data = new();
		data.mLongPressTime = 2.0f;
		float progress = 0.0f;
		data.mOnLongPressing = p => progress = p;
		int pressCalls = 0;
		data.mOnLongPress = () => ++pressCalls;
		// pressedTime=2.0, 阈值=2.0 → 已达阈值, 触发长按
		data.update(2.0f);
		assertEqual(1, pressCalls, "达阈值应触发长按回调");
		assertTrue(data.mFinish, "达阈值后 mFinish 应为 true");
		assertEqual(1.0f, progress, 0.001f, "达阈值时进度 = 1.0");
		data.resetProperty();
	}

	// ═════════════════════════════════════════════════════════════════
	// 进度 clamp / finish 跳过 / reset
	// ═════════════════════════════════════════════════════════════════
	private static void testProgressClamp()
	{
		LongPressData data = new();
		data.mLongPressTime = 1.0f;
		float progress = 0.0f;
		data.mOnLongPressing = p => progress = p;
		data.mOnLongPress = () => { };
		// pressedTime=5.0 > 阈值=1.0 → 进度 clamp 到 1.0
		data.update(5.0f);
		assertEqual(1.0f, progress, 0.001f, "超过阈值时进度 clamp 到 1.0");
		data.resetProperty();
	}
	private static void testFinishSkips()
	{
		LongPressData data = new();
		data.mLongPressTime = 1.0f;
		data.mFinish = true;
		int pressCalls = 0;
		data.mOnLongPress = () => ++pressCalls;
		// mFinish=true 时 update 直接跳过
		data.update(5.0f);
		assertEqual(0, pressCalls, "mFinish=true 时 update 跳过, 不触发长按");
		data.resetProperty();
	}
	private static void testReset()
	{
		LongPressData data = new();
		data.mLongPressTime = 1.0f;
		data.mFinish = true;
		data.reset();
		assertFalse(data.mFinish, "reset() 应重置 mFinish 为 false");
		data.resetProperty();
	}
}
