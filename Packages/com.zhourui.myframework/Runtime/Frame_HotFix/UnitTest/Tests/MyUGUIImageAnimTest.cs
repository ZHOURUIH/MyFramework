using static TestAssert;
using System.Collections.Generic;
using UnityEngine;

// myUGUIImageAnim: Image 序列帧动画——核心委托给纯 C# AnimControl 状态机
// 无图集环境, 不调 init; 不测 setTextureSet(无图集时源码会触发 logError: invalid sprite anim)
public static class MyUGUIImageAnimTest
{
	public static void Run()
	{
		testDefaultValues();
		testSetLoop();
		testSetInterval();
		testSetSpeed();
		testSetStartEndIndex();
		testSetPlayDirection();
		testPlayPauseStop();
		testSetAutoHide();
		testSetTexturePosList();
		testSetCurFrameIndexEmpty();
		testCallbackStorage();
		testSetEffectAlignNoThrow();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// AnimControl 构造默认值(经 myUGUIImageAnim 委托)
	private static void testDefaultValues()
	{
		myUGUIImageAnim anim = new myUGUIImageAnim();
		assertEqual(LOOP_MODE.LOOP, anim.getLoop(), "默认循环模式 LOOP");
		assertEqual(0.033f, anim.getInterval(), 0.0001f, "默认间隔 0.033");
		assertEqual(0, anim.getStartIndex(), "默认起始帧 0");
		assertEqual(-1, anim.getEndIndex(), "默认终止帧 -1");
		assertEqual(PLAY_STATE.STOP, anim.getPlayState(), "默认停止");
		assertTrue(anim.getPlayDirection(), "默认正向播放");
		assertTrue(anim.isAutoHide(), "默认播放完自动重置");
		assertEqual(0, anim.getCurFrameIndex(), "默认当前帧 0");
		assertEqual(0, anim.getTextureFrameCount(), "无图集时帧数为 0");
		assertEqual(0, anim.getRealEndIndex(), "无图集时实际终止帧为 0");
		assertEqual(0.0f, anim.getLength(), 0.0001f, "无帧时长度为 0");
		assertNull(anim.getTextureSet(), "默认序列帧名字为 null");
	}

	// setLoop/getLoop 读写
	private static void testSetLoop()
	{
		myUGUIImageAnim anim = new myUGUIImageAnim();
		anim.setLoop(LOOP_MODE.ONCE);
		assertEqual(LOOP_MODE.ONCE, anim.getLoop(), "ONCE 读回");
		anim.setLoop(LOOP_MODE.PING_PONG);
		assertEqual(LOOP_MODE.PING_PONG, anim.getLoop(), "PING_PONG 读回");
	}

	// setInterval/getInterval 读写
	private static void testSetInterval()
	{
		myUGUIImageAnim anim = new myUGUIImageAnim();
		anim.setInterval(0.1f);
		assertEqual(0.1f, anim.getInterval(), 0.0001f, "setInterval(0.1) 读回");
	}

	// setSpeed/getSpeed: speed ↔ interval 往返换算
	private static void testSetSpeed()
	{
		myUGUIImageAnim anim = new myUGUIImageAnim();
		anim.setSpeed(2.0f);
		assertEqual(2.0f, anim.getSpeed(), 0.001f, "setSpeed(2) 读回");
		anim.setSpeed(0.5f);
		assertEqual(0.5f, anim.getSpeed(), 0.001f, "setSpeed(0.5) 读回");
	}

	// setStartIndex/setEndIndex 读写
	private static void testSetStartEndIndex()
	{
		myUGUIImageAnim anim = new myUGUIImageAnim();
		anim.setStartIndex(3);
		assertEqual(3, anim.getStartIndex(), "setStartIndex(3) 读回");
		anim.setEndIndex(7);
		assertEqual(7, anim.getEndIndex(), "setEndIndex(7) 读回");
	}

	// setPlayDirection/getPlayDirection
	private static void testSetPlayDirection()
	{
		myUGUIImageAnim anim = new myUGUIImageAnim();
		anim.setPlayDirection(false);
		assertFalse(anim.getPlayDirection(), "setPlayDirection(false) 读回");
	}

	// play/pause/stop: 状态切换
	private static void testPlayPauseStop()
	{
		myUGUIImageAnim anim = new myUGUIImageAnim();
		anim.play();
		assertEqual(PLAY_STATE.PLAY, anim.getPlayState(), "play 后状态 PLAY");
		anim.pause();
		assertEqual(PLAY_STATE.PAUSE, anim.getPlayState(), "pause 后状态 PAUSE");
		anim.stop();
		assertEqual(PLAY_STATE.STOP, anim.getPlayState(), "stop 后状态 STOP");
	}

	// setAutoHide/isAutoHide
	private static void testSetAutoHide()
	{
		myUGUIImageAnim anim = new myUGUIImageAnim();
		anim.setAutoHide(false);
		assertFalse(anim.isAutoHide(), "setAutoHide(false) 读回");
		anim.setAutoHide(true);
		assertTrue(anim.isAutoHide(), "setAutoHide(true) 读回");
	}

	// setTexturePosList/getTexturePosList: 引用存储
	private static void testSetTexturePosList()
	{
		myUGUIImageAnim anim = new myUGUIImageAnim();
		List<Vector2> posList = new() { new Vector2(0.0f, 0.0f), new Vector2(10.0f, 20.0f) };
		anim.setTexturePosList(posList);
		assertTrue(ReferenceEquals(posList, anim.getTexturePosList()), "getTexturePosList 返回同一引用");
		anim.setTexturePosList(null);
		assertNull(anim.getTexturePosList(), "setTexturePosList(null) 后为 null");
	}

	// setCurFrameIndex: 无序列帧时被 clamp 到 0
	private static void testSetCurFrameIndexEmpty()
	{
		myUGUIImageAnim anim = new myUGUIImageAnim();
		anim.setCurFrameIndex(2);
		assertEqual(0, anim.getCurFrameIndex(), "无帧时 setCurFrameIndex(2) 被 clamp 到 0");
	}

	// 回调存储: addPlayEndCallback(clear=true) 添加时触发旧回调(中断语义), addPlayingCallback 只清空不触发
	private static void testCallbackStorage()
	{
		myUGUIImageAnim anim = new myUGUIImageAnim();
		int endCallCount = 0;
		bool playingCalled = false;
		// addPlayEndCallback(clear=true): 首次添加列表空不触发, 再次添加触发旧回调(isBreak=true)
		anim.addPlayEndCallback((isBreak) => ++endCallCount);
		assertEqual(0, endCallCount, "首次添加不触发旧回调");
		anim.addPlayEndCallback((isBreak) => ++endCallCount);
		assertEqual(1, endCallCount, "添加新回调时触发旧回调 1 次(中断通知)");
		// addPlayingCallback(clear=true): 只清空旧列表, 不触发回调
		// addPlayingCallback 参数是单参 BoolCallback(void(bool))
		anim.addPlayingCallback((isPlaying) => playingCalled = true);
		anim.addPlayingCallback((isPlaying) => playingCalled = true);
		assertFalse(playingCalled, "添加播放中回调不触发");
		anim.clearCallback();
		anim.addPlayEndCallback((isBreak) => ++endCallCount);
		anim.clearCallback();
		// 无异常即通过
	}

	// setEffectAlign/setUseTextureSize: 无 getter, 验证不抛
	private static void testSetEffectAlignNoThrow()
	{
		myUGUIImageAnim anim = new myUGUIImageAnim();
		anim.setEffectAlign(EFFECT_ALIGN.POSITION_LIST);
		anim.setEffectAlign(EFFECT_ALIGN.PARENT_BOTTOM);
		anim.setUseTextureSize(true);
		anim.setUseTextureSize(false);
		// 无异常即通过
	}
}
