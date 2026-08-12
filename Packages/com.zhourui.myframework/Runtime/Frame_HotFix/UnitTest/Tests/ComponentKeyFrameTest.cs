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
