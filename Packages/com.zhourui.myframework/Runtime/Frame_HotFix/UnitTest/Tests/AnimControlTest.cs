using static TestAssert;

// AnimControl 单元测试：播放状态机/循环模式/方向/帧切换逻辑
public static class AnimControlTest
{
	public static void Run()
	{
		testDefaultState();
		testResetProperty();
		testPlay();
		testPause();
		testStop();
		testStopNoReset();
		testStopCallback();
		testSetFrameCount();
		testSetFrameCountClampIndex();
		testGetRealEndIndexAuto();
		testGetRealEndIndexSet();
		testSetStartIndex();
		testSetEndIndex();
		testSetLoopMode();
		testSetPlayDirection();
		testSetInterval();
		testSetSpeed();
		testUpdateNotPlaying();
		testUpdateOnceComplete();
		testUpdateLoopMode();
		testUpdatePingPongMode();
		testGetLength();
		testSetCurFrameIndex();
		testSetCurFrameIndexCallback();
		testSetAutoHide();
		testSetPlayEndCallback();
		testSetPlayingCallback();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	private static void testDefaultState()
	{
		var ac = new AnimControl();
		assertEqual(PLAY_STATE.STOP, ac.getPlayState(), "默认 STOP");
		assertEqual(LOOP_MODE.LOOP, ac.getLoop(), "默认 LOOP");
		assertEqual(0.033f, ac.getInterval(), "默认 interval=0.033");
		assertTrue(ac.getPlayDirection(), "默认正向播放");
		assertEqual(0, ac.getStartIndex(), "默认 startIndex=0");
		assertEqual(-1, ac.getEndIndex(), "默认 endIndex=-1");
		assertTrue(ac.isAutoResetIndex(), "默认 autoReset=true");
		assertEqual(0, ac.getTextureFrameCount(), "默认 frameCount=0");
		assertEqual(0, ac.getCurFrameIndex(), "默认 curFrameIndex=0");
	}

	private static void testResetProperty()
	{
		var ac = new AnimControl();
		ac.setFrameCount(10);
		ac.setLoop(LOOP_MODE.ONCE);
		ac.setInterval(0.1f);
		ac.setPlayDirection(false);
		ac.setAutoHide(false);
		ac.play();

		ac.resetProperty();
		assertEqual(PLAY_STATE.STOP, ac.getPlayState(), "reset 后 STOP");
		assertEqual(LOOP_MODE.LOOP, ac.getLoop(), "reset 后 LOOP");
		assertEqual(0.033f, ac.getInterval(), "reset 后 interval=0.033");
		assertTrue(ac.getPlayDirection(), "reset 后正向");
		assertEqual(0, ac.getTextureFrameCount(), "reset 后 frameCount=0");
		assertTrue(ac.isAutoResetIndex(), "reset 后 autoReset=true");
	}

	private static void testPlay()
	{
		var ac = new AnimControl();
		ac.setFrameCount(5);
		ac.play();
		assertEqual(PLAY_STATE.PLAY, ac.getPlayState(), "play 后状态=PLAY");
	}

	private static void testPause()
	{
		var ac = new AnimControl();
		ac.setFrameCount(5);
		ac.play();
		ac.pause();
		assertEqual(PLAY_STATE.PAUSE, ac.getPlayState(), "pause 后状态=PAUSE");
	}

	private static void testStop()
	{
		var ac = new AnimControl();
		ac.setFrameCount(5);
		ac.play();
		ac.stop();
		assertEqual(PLAY_STATE.STOP, ac.getPlayState(), "stop 后状态=STOP");
		assertEqual(ac.getStartIndex(), ac.getCurFrameIndex(), "stop 后帧归位到 startIndex");
	}

	private static void testStopNoReset()
	{
		var ac = new AnimControl();
		ac.setFrameCount(10);
		ac.play();
		// 手动设置到第5帧
		ac.setCurFrameIndex(5);
		ac.stop(false);
		assertEqual(PLAY_STATE.STOP, ac.getPlayState(), "stop 后 STOP");
		assertEqual(5, ac.getCurFrameIndex(), "stop(false) 不重置帧");
	}

	private static void testStopCallback()
	{
		var ac = new AnimControl();
		ac.setFrameCount(5);
		ac.setLoop(LOOP_MODE.ONCE);

		bool callbackCalled = false;
		bool callbackIsBreak = false;
		ac.setPlayEndCallback((bool cb, bool isBreak) => {
			callbackCalled = true;
			callbackIsBreak = isBreak;
		});

		ac.play();
		ac.stop(true, true, true);
		assertTrue(callbackCalled, "stop 触发回调");
		assertTrue(callbackIsBreak, "stop isBreak=true");
	}

	private static void testSetFrameCount()
	{
		var ac = new AnimControl();
		ac.setFrameCount(8);
		assertEqual(8, ac.getTextureFrameCount(), "setFrameCount(8)");
		assertEqual(0, ac.getCurFrameIndex(), "setFrameCount 重置到 startIndex");
	}

	private static void testSetFrameCountClampIndex()
	{
		var ac = new AnimControl();
		ac.setFrameCount(10);
		ac.setStartIndex(8);
		ac.setEndIndex(9);
		// setFrameCount(5) 会 clamp startIndex 和 endIndex
		ac.setFrameCount(5);
		assertTrue(ac.getStartIndex() <= 4, "startIndex 被 clamp 到 [0,4]");
		assertTrue(ac.getEndIndex() <= 4, "endIndex 被 clamp 到 [0,4]");
	}

	private static void testGetRealEndIndexAuto()
	{
		var ac = new AnimControl();
		ac.setFrameCount(10);
		assertEqual(9, ac.getRealEndIndex(), "endIndex=-1 时返回 frameCount-1");
		ac.setFrameCount(0);
		assertEqual(0, ac.getRealEndIndex(), "frameCount=0 时返回 0");
	}

	private static void testGetRealEndIndexSet()
	{
		var ac = new AnimControl();
		ac.setFrameCount(10);
		ac.setEndIndex(5);
		assertEqual(5, ac.getRealEndIndex(), "手动设置 endIndex=5 时返回5");
	}

	private static void testSetStartIndex()
	{
		var ac = new AnimControl();
		ac.setFrameCount(10);
		ac.setStartIndex(3);
		assertEqual(3, ac.getStartIndex(), "setStartIndex(3)");
		ac.setStartIndex(20);
		assertEqual(9, ac.getStartIndex(), "setStartIndex(20) 被 clamp 到 9");
	}

	private static void testSetEndIndex()
	{
		var ac = new AnimControl();
		ac.setFrameCount(10);
		ac.setEndIndex(7);
		assertEqual(7, ac.getEndIndex(), "setEndIndex(7)");
		ac.setEndIndex(20);
		assertEqual(9, ac.getEndIndex(), "setEndIndex(20) 被 clamp 到 9");
	}

	private static void testSetLoopMode()
	{
		var ac = new AnimControl();
		ac.setLoop(LOOP_MODE.ONCE);
		assertEqual(LOOP_MODE.ONCE, ac.getLoop(), "setLoop(ONCE)");
		ac.setLoop(LOOP_MODE.PING_PONG);
		assertEqual(LOOP_MODE.PING_PONG, ac.getLoop(), "setLoop(PING_PONG)");
	}

	private static void testSetPlayDirection()
	{
		var ac = new AnimControl();
		assertTrue(ac.getPlayDirection(), "默认正向");
		ac.setPlayDirection(false);
		assertFalse(ac.getPlayDirection(), "setPlayDirection(false)");
	}

	private static void testSetInterval()
	{
		var ac = new AnimControl();
		ac.setInterval(0.5f);
		assertEqual(0.5f, ac.getInterval(), "setInterval(0.5)");
	}

	private static void testSetSpeed()
	{
		var ac = new AnimControl();
		ac.setSpeed(30f);  // 30fps
		float interval = ac.getInterval();
		assertTrue(interval > 0, "setSpeed 计算 interval > 0");
	}

	private static void testUpdateNotPlaying()
	{
		var ac = new AnimControl();
		ac.setFrameCount(5);
		// 不调用 play，update 不改变帧
		ac.update(1.0f);
		assertEqual(0, ac.getCurFrameIndex(), "未播放时 update 不改变帧");
	}

	private static void testUpdateOnceComplete()
	{
		var ac = new AnimControl();
		ac.setFrameCount(5);
		ac.setLoop(LOOP_MODE.ONCE);
		ac.setInterval(0.1f);
		ac.play();
		// 足够长的更新时间使动画完成
		ac.update(5.0f);
		assertEqual(PLAY_STATE.STOP, ac.getPlayState(), "ONCE 模式播放完成后 STOP");
	}

	private static void testUpdateLoopMode()
	{
		var ac = new AnimControl();
		ac.setFrameCount(5);
		ac.setLoop(LOOP_MODE.LOOP);
		ac.setInterval(0.1f);
		ac.play();
		ac.update(5.0f);
		assertEqual(PLAY_STATE.PLAY, ac.getPlayState(), "LOOP 模式一直 PLAY");
	}

	private static void testUpdatePingPongMode()
	{
		var ac = new AnimControl();
		ac.setFrameCount(5);
		ac.setLoop(LOOP_MODE.PING_PONG);
		ac.setInterval(0.1f);
		ac.play();
		// 播放到末尾后会反向
		ac.update(5.0f);
		assertEqual(PLAY_STATE.PLAY, ac.getPlayState(), "PING_PONG 模式一直 PLAY");
	}

	private static void testGetLength()
	{
		var ac = new AnimControl();
		ac.setFrameCount(10);
		ac.setInterval(0.1f);
		assertEqual(1.0f, ac.getLength(), "length=frameCount*interval=1.0");
	}

	private static void testSetCurFrameIndex()
	{
		var ac = new AnimControl();
		ac.setFrameCount(10);
		ac.setCurFrameIndex(5);
		assertEqual(5, ac.getCurFrameIndex(), "setCurFrameIndex(5)");
		// 超出范围被 clamp
		ac.setCurFrameIndex(20);
		assertEqual(9, ac.getCurFrameIndex(), "setCurFrameIndex(20) clamp 到 9");
		ac.setCurFrameIndex(-5);
		assertEqual(0, ac.getCurFrameIndex(), "setCurFrameIndex(-5) clamp 到 0");
	}

	private static void testSetCurFrameIndexCallback()
	{
		var ac = new AnimControl();
		ac.setFrameCount(5);
		bool callbackFired = false;
		ac.setPlayingCallback((int index, bool isPlaying) => {
			callbackFired = true;
		});
		ac.setCurFrameIndex(2);
		assertTrue(callbackFired, "setCurFrameIndex 触发 playingCallback");
	}

	private static void testSetAutoHide()
	{
		var ac = new AnimControl();
		assertTrue(ac.isAutoResetIndex(), "默认 autoReset=true");
		ac.setAutoHide(false);
		assertFalse(ac.isAutoResetIndex(), "setAutoHide(false)");
	}

	private static void testSetPlayEndCallback()
	{
		var ac = new AnimControl();
		bool called = false;
		ac.setPlayEndCallback((bool b1, bool b2) => { called = true; });
		ac.setLoop(LOOP_MODE.ONCE);
		ac.setFrameCount(3);
		ac.setInterval(0.01f);
		ac.play();
		ac.update(1.0f);
		assertTrue(called, "ONCE 模式完成触发 playEndCallback");
	}

	private static void testSetPlayingCallback()
	{
		var ac = new AnimControl();
		int lastFrame = -1;
		ac.setPlayingCallback((int frame, bool playing) => {
			lastFrame = frame;
		});
		ac.setFrameCount(5);
		ac.setInterval(0.1f);
		ac.play();
		ac.update(1.0f);
		// playingCallback 应在帧切换时触发
		assertTrue(lastFrame >= 0, "playingCallback 被触发");
	}
}
