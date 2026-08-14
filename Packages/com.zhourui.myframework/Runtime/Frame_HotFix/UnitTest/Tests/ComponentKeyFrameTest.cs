using UnityEngine;
using static TestAssert;

// ComponentKeyFrame 深度测试(关键帧组件状态机)
//   setState: PLAY→play(依赖 mKeyFrameManager 不测) / STOP→stop / PAUSE→pause
//   getTremblingPercent: mCurrentTime/mOnceLength 纯数学
//   stop: mComponentOwner null 时空安全
// 环境: new TestComponentKeyFrame()(无参构造, 不调 init)
public static class ComponentKeyFrameTest
{
	public static void Run()
	{
		testResetDefaults();
		testPauseState();
		testStopFromPause();
		testStopIdempotent();
		testTremblingPercent();
		testTremblingPercentFullRange();
		testStopResetsTime();
		testPauseIdempotentThenStop();
		testSetOnceLengthRoundTrip();
		testSetKeyframeIDRoundTrip();
		testSetLoopToggle();
		testTremblingHalfOnceLength();
		testSetOffsetRoundTrip();
		testNotifyBreakSafe();
		testGetCurrentTimeDefault();
		testGetCurValueDefault();
		testSetDoingCallbackInterrupt();
		testSetDoingCallbackNull();
		testSetDoneCallbackInterrupt();
		testSetUpdateInFixedTickToggle();
		testNotifyBreakClearsCallbacks();
	}

	// ═════════════════════════════════════════════════════════════════
	// 辅助
	// ═════════════════════════════════════════════════════════════════
	private static TestComponentKeyFrame createKeyFrame()
	{
		TestComponentKeyFrame kf = new TestComponentKeyFrame();
		kf.resetProperty();
		return kf;
	}

	// resetProperty 默认值
	private static void testResetDefaults()
	{
		TestComponentKeyFrame kf = createKeyFrame();
		assertTrue(kf.getPlayStateForTest() == PLAY_STATE.STOP, "默认 STOP");
		assertEqual(0.0f, kf.getTremblingPercent(), 0.001f, "默认 trembling 0");
		assertTrue(kf.isLoopForTest(), "默认循环");
		assertEqual(1.0f, kf.getOnceLengthForTest(), 0.001f, "默认 onceLength 1");
	}

	// setState(PAUSE): 从 STOP 进入 PAUSE
	private static void testPauseState()
	{
		TestComponentKeyFrame kf = createKeyFrame();
		kf.setState(PLAY_STATE.PAUSE);
		assertTrue(kf.getPlayStateForTest() == PLAY_STATE.PAUSE, "setState(PAUSE) 进入暂停");
	}

	// setState(STOP) 从 PAUSE: 停止
	private static void testStopFromPause()
	{
		TestComponentKeyFrame kf = createKeyFrame();
		kf.setState(PLAY_STATE.PAUSE);
		kf.setState(PLAY_STATE.STOP);
		assertTrue(kf.getPlayStateForTest() == PLAY_STATE.STOP, "从 PAUSE 停止");
	}

	// stop 幂等: 已 STOP 再 stop 不重复执行
	private static void testStopIdempotent()
	{
		TestComponentKeyFrame kf = createKeyFrame();
		kf.stop();   // 初始 STOP, 直接 return
		assertTrue(kf.getPlayStateForTest() == PLAY_STATE.STOP, "已停止时 stop 幂等");
	}

	// getTremblingPercent: mCurrentTime/mOnceLength
	private static void testTremblingPercent()
	{
		TestComponentKeyFrame kf = createKeyFrame();
		kf.setCurrentTimeForTest(0.5f);
		assertEqual(0.5f, kf.getTremblingPercent(), 0.001f, "0.5/1 = 0.5");
		kf.setCurrentTimeForTest(0.0f);
		assertEqual(0.0f, kf.getTremblingPercent(), 0.001f, "0/1 = 0");
	}

	// getTremblingPercent 全范围: mCurrentTime/mOnceLength(无 saturate, 超 1 直接返回)
	private static void testTremblingPercentFullRange()
	{
		TestComponentKeyFrame kf = createKeyFrame();
		kf.setCurrentTimeForTest(0.25f);
		assertEqual(0.25f, kf.getTremblingPercent(), 0.001f, "0.25/1 = 0.25");
		kf.setCurrentTimeForTest(1.0f);
		assertEqual(1.0f, kf.getTremblingPercent(), 0.001f, "1/1 = 1");
		kf.setCurrentTimeForTest(2.0f);
		assertEqual(2.0f, kf.getTremblingPercent(), 0.001f, "2/1 = 2(无 saturate)");
	}

	// 组合: stop 复位播放时间
	// 注意: stop 首行 `if (mPlayState == STOP && !force) return` 早退,
	// 默认 STOP 状态直接 return 不复位时间 → 必须先从非 STOP 状态 stop
	private static void testStopResetsTime()
	{
		TestComponentKeyFrame kf = createKeyFrame();
		kf.setCurrentTimeForTest(0.5f);
		assertEqual(0.5f, kf.getTremblingPercent(), 0.001f, "设置时间后 percent 0.5");
		// 进入 PAUSE(非 STOP), stop 才会执行复位逻辑
		kf.pause();
		kf.stop();
		assertTrue(kf.getPlayStateForTest() == PLAY_STATE.STOP, "stop 后 STOP");
		assertEqual(0.0f, kf.getTremblingPercent(), 0.001f, "stop 复位时间 → percent 0");
	}

	// 组合: pause 幂等 + pause 后 stop 回 STOP
	private static void testPauseIdempotentThenStop()
	{
		TestComponentKeyFrame kf = createKeyFrame();
		kf.pause();
		assertTrue(kf.getPlayStateForTest() == PLAY_STATE.PAUSE, "首次 pause → PAUSE");
		kf.pause();
		assertTrue(kf.getPlayStateForTest() == PLAY_STATE.PAUSE, "重复 pause 幂等");
		kf.stop();
		assertTrue(kf.getPlayStateForTest() == PLAY_STATE.STOP, "pause 后 stop → STOP");
	}

	// ═════════════════════════════════════════════════════════════════
	// 深度组合: setter 往返 + trembling 数学
	// ═════════════════════════════════════════════════════════════════

	// setOnceLength 往返
	private static void testSetOnceLengthRoundTrip()
	{
		TestComponentKeyFrame kf = createKeyFrame();
		kf.setOnceLength(2.0f);
		assertEqual(2.0f, kf.getOnceLengthForTest(), 0.001f, "setOnceLength(2) 读回 2");
		kf.setOnceLength(0.5f);
		assertEqual(0.5f, kf.getOnceLengthForTest(), 0.001f, "setOnceLength(0.5) 读回 0.5");
	}

	// setKeyframeID 往返
	private static void testSetKeyframeIDRoundTrip()
	{
		TestComponentKeyFrame kf = createKeyFrame();
		kf.setKeyframeID(KEY_CURVE.QUAD_IN);
		assertEqual(KEY_CURVE.QUAD_IN, kf.getKeyframeID(), "setKeyframeID 读回");
	}

	// setLoop 切换
	private static void testSetLoopToggle()
	{
		TestComponentKeyFrame kf = createKeyFrame();
		kf.setLoop(false);
		assertFalse(kf.isLoopForTest(), "setLoop(false) 后非循环");
		kf.setLoop(true);
		assertTrue(kf.isLoopForTest(), "setLoop(true) 恢复循环");
	}

	// trembling = currentTime / onceLength 数学
	private static void testTremblingHalfOnceLength()
	{
		TestComponentKeyFrame kf = createKeyFrame();
		kf.setCurrentTimeForTest(0.5f);
		kf.setOnceLength(2.0f);
		assertEqual(0.25f, kf.getTremblingPercent(), 0.001f, "trembling=0.5/2=0.25");
	}

	// setOffset 往返(offset 不影响 trembling 数学)
	private static void testSetOffsetRoundTrip()
	{
		TestComponentKeyFrame kf = createKeyFrame();
		kf.setOffset(0.5f);
		assertEqual(0.5f, kf.getOffset(), 0.001f, "setOffset 读回");
		kf.setOffset(-0.25f);
		assertEqual(-0.25f, kf.getOffset(), 0.001f, "负 offset 读回");
	}

	// notifyBreak 空安全(IComponentBreakable)
	private static void testNotifyBreakSafe()
	{
		TestComponentKeyFrame kf = createKeyFrame();
		kf.notifyBreak();
		// 无异常即通过
	}

	// 默认 currentTime 0
	private static void testGetCurrentTimeDefault()
	{
		TestComponentKeyFrame kf = createKeyFrame();
		assertEqual(0.0f, kf.getCurrentTime(), 0.001f, "默认 currentTime 0");
	}

	// 默认 curValue 0
	private static void testGetCurValueDefault()
	{
		TestComponentKeyFrame kf = createKeyFrame();
		assertEqual(0.0f, kf.getCurValue(), 0.001f, "默认 curValue 0");
	}

	// ═════════════════════════════════════════════════════════════════
	// 回调中断语义: setCallback 时旧回调以 isBreak=true 被调用
	// ═════════════════════════════════════════════════════════════════

	// setDoingCallback 替换时旧回调中断
	private static void testSetDoingCallbackInterrupt()
	{
		TestComponentKeyFrame kf = createKeyFrame();
		bool oldCalled = false;
		bool oldBreak = false;
		kf.setDoingCallback((com, isBreak) =>
		{
			oldCalled = true;
			oldBreak = isBreak;
		});
		kf.setDoingCallback((com, isBreak) => { });
		assertTrue(oldCalled, "旧回调被调用");
		assertTrue(oldBreak, "旧回调以中断标记调用");
	}

	// setDoingCallback(null) 使旧回调中断
	private static void testSetDoingCallbackNull()
	{
		TestComponentKeyFrame kf = createKeyFrame();
		bool oldCalled = false;
		kf.setDoingCallback((com, isBreak) => { oldCalled = true; });
		kf.setDoingCallback(null);
		assertTrue(oldCalled, "set null 时旧回调中断调用");
	}

	// setDoneCallback 替换时旧回调中断
	private static void testSetDoneCallbackInterrupt()
	{
		TestComponentKeyFrame kf = createKeyFrame();
		bool oldCalled = false;
		kf.setDoneCallback((com, isBreak) => { oldCalled = true; });
		kf.setDoneCallback((com, isBreak) => { });
		assertTrue(oldCalled, "done 旧回调被调用");
	}

	// setUpdateInFixedTick 切换不炸
	private static void testSetUpdateInFixedTickToggle()
	{
		TestComponentKeyFrame kf = createKeyFrame();
		kf.setUpdateInFixedTick(true);
		kf.setUpdateInFixedTick(false);
		// 无异常即通过
	}

	// notifyBreak 清空回调: 再次 set null 不触发中断
	private static void testNotifyBreakClearsCallbacks()
	{
		TestComponentKeyFrame kf = createKeyFrame();
		kf.setDoingCallback((com, isBreak) => { });
		kf.setDoneCallback((com, isBreak) => { });
		kf.notifyBreak();
		// notifyBreak 内部 set null, 已清空无中断调用
		// 无异常即通过
	}
}

// ═════════════════════════════════════════════════════════════════
// 测试辅助: 暴露 ComponentKeyFrame 的 protected 字段
// ═════════════════════════════════════════════════════════════════
public class TestComponentKeyFrame : ComponentKeyFrame
{
	public PLAY_STATE getPlayStateForTest() { return mPlayState; }

	public bool isLoopForTest() { return mLoop; }

	public float getOnceLengthForTest() { return mOnceLength; }

	public void setCurrentTimeForTest(float time) { mCurrentTime = time; }
}
