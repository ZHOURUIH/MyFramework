using static TestAssert;
using System.Collections.Generic;
using UnityEngine;

// myUGUISpriteAnim: 序列帧动画——核心委托给纯 C# AnimControl 状态机
// 无图集环境, 不调 init(setTextureSet 在 mAtlasPtr 无效时只记录名字不加载序列)
public static class MyUGUISpriteAnimTest
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
		testSetTextureSet();
		testSetTexturePosList();
		testGetLength();
		testSetEffectAlign();
		testSetUseTextureSize();
		testSetCurFrameIndexEmpty();
		testCallbackStorage();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// AnimControl 构造默认值(经 myUGUISpriteAnim 委托)
	private static void testDefaultValues()
	{
		myUGUISpriteAnim anim = new myUGUISpriteAnim();
		assertEqual(LOOP_MODE.LOOP, anim.getLoop(), "默认循环模式 LOOP");
		assertEqual(0.033f, anim.getInterval(), 0.0001f, "默认间隔 0.033");
		assertEqual(0, anim.getStartIndex(), "默认起始帧 0");
		assertEqual(-1, anim.getEndIndex(), "默认终止帧 -1(播放到尾部)");
		assertEqual(PLAY_STATE.STOP, anim.getPlayState(), "默认停止");
		assertTrue(anim.getPlayDirection(), "默认正向播放");
		assertTrue(anim.isAutoHide(), "默认播放完自动重置");
		assertEqual(0, anim.getCurFrameIndex(), "默认当前帧 0");
		assertEqual(0, anim.getTextureFrameCount(), "无图集时帧数为 0");
		assertEqual(0, anim.getRealEndIndex(), "无图集时实际终止帧为 0");
	}

	// setLoop/getLoop 读写
	private static void testSetLoop()
	{
		myUGUISpriteAnim anim = new myUGUISpriteAnim();
		anim.setLoop(LOOP_MODE.ONCE);
		assertEqual(LOOP_MODE.ONCE, anim.getLoop(), "ONCE 读回");
		anim.setLoop(LOOP_MODE.PING_PONG);
		assertEqual(LOOP_MODE.PING_PONG, anim.getLoop(), "PING_PONG 读回");
	}

	// setInterval/getInterval 读写
	private static void testSetInterval()
	{
		myUGUISpriteAnim anim = new myUGUISpriteAnim();
		anim.setInterval(0.1f);
		assertEqual(0.1f, anim.getInterval(), 0.0001f, "setInterval(0.1) 读回");
		anim.setInterval(0.5f);
		assertEqual(0.5f, anim.getInterval(), 0.0001f, "setInterval(0.5) 读回");
	}

	// setSpeed/getSpeed: speed ↔ interval 往返换算
	private static void testSetSpeed()
	{
		myUGUISpriteAnim anim = new myUGUISpriteAnim();
		anim.setSpeed(2.0f);
		assertEqual(2.0f, anim.getSpeed(), 0.001f, "setSpeed(2) 读回");
		anim.setSpeed(0.5f);
		assertEqual(0.5f, anim.getSpeed(), 0.001f, "setSpeed(0.5) 读回");
	}

	// setStartIndex/setEndIndex 读写
	private static void testSetStartEndIndex()
	{
		myUGUISpriteAnim anim = new myUGUISpriteAnim();
		anim.setStartIndex(3);
		assertEqual(3, anim.getStartIndex(), "setStartIndex(3) 读回");
		anim.setEndIndex(7);
		assertEqual(7, anim.getEndIndex(), "setEndIndex(7) 读回");
	}

	// setPlayDirection/getPlayDirection
	private static void testSetPlayDirection()
	{
		myUGUISpriteAnim anim = new myUGUISpriteAnim();
		anim.setPlayDirection(false);
		assertFalse(anim.getPlayDirection(), "setPlayDirection(false) 读回");
		anim.setPlayDirection(true);
		assertTrue(anim.getPlayDirection(), "setPlayDirection(true) 读回");
	}

	// play/pause/stop: 状态切换
	private static void testPlayPauseStop()
	{
		myUGUISpriteAnim anim = new myUGUISpriteAnim();
		anim.play();
		assertEqual(PLAY_STATE.PLAY, anim.getPlayState(), "play 后状态 PLAY");
		anim.pause();
		assertEqual(PLAY_STATE.PAUSE, anim.getPlayState(), "pause 后状态 PAUSE");
		anim.play();
		assertEqual(PLAY_STATE.PLAY, anim.getPlayState(), "再次 play 后状态 PLAY");
		anim.stop();
		assertEqual(PLAY_STATE.STOP, anim.getPlayState(), "stop 后状态 STOP");
	}

	// setAutoHide/isAutoHide
	private static void testSetAutoHide()
	{
		myUGUISpriteAnim anim = new myUGUISpriteAnim();
		anim.setAutoHide(false);
		assertFalse(anim.isAutoHide(), "setAutoHide(false) 读回");
		anim.setAutoHide(true);
		assertTrue(anim.isAutoHide(), "setAutoHide(true) 读回");
	}

	// setTextureSet: 无图集时只记录名字, 不加载序列
	private static void testSetTextureSet()
	{
		myUGUISpriteAnim anim = new myUGUISpriteAnim();
		anim.setTextureSet("hero_attack");
		assertEqual("hero_attack", anim.getTextureSet(), "setTextureSet 后名字记录");
		assertEqual(0, anim.getTextureFrameCount(), "无图集时帧数为 0");
		// 相同名字重复设置直接返回
		anim.setTextureSet("hero_attack");
		assertEqual("hero_attack", anim.getTextureSet(), "重复设置同名字不变");
	}

	// setTexturePosList/getTexturePosList: 引用存储
	private static void testSetTexturePosList()
	{
		myUGUISpriteAnim anim = new myUGUISpriteAnim();
		List<Vector2> posList = new() { new Vector2(0.0f, 0.0f), new Vector2(10.0f, 20.0f) };
		anim.setTexturePosList(posList);
		assertTrue(ReferenceEquals(posList, anim.getTexturePosList()), "getTexturePosList 返回同一引用");
		anim.setTexturePosList(null);
		assertNull(anim.getTexturePosList(), "setTexturePosList(null) 后为 null");
	}

	// setCurFrameIndex: 无序列帧时被 clamp 到 0
	private static void testSetCurFrameIndexEmpty()
	{
		myUGUISpriteAnim anim = new myUGUISpriteAnim();
		anim.setCurFrameIndex(2);
		assertEqual(0, anim.getCurFrameIndex(), "无帧时 setCurFrameIndex(2) 被 clamp 到 0");
	}

	// 回调存储: addPlayEndCallback(clear=true) 添加时触发旧回调(中断语义), addPlayingCallback 只清空不触发
	private static void testCallbackStorage()
	{
		myUGUISpriteAnim anim = new myUGUISpriteAnim();
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
		// 清空后再次添加不崩
		anim.clearCallback();
		anim.addPlayEndCallback((isBreak) => ++endCallCount);
		anim.addPlayingCallback((isPlaying) => playingCalled = true);
		anim.clearCallback();
		// 无异常即通过
	}

	// getLength: 总时长 = 帧数 * 间隔, 无帧时为 0
	private static void testGetLength()
	{
		myUGUISpriteAnim anim = new myUGUISpriteAnim();
		assertEqual(0.0f, anim.getLength(), 0.0001f, "无帧时 getLength 0");
		anim.setInterval(0.5f);
		assertEqual(0.0f, anim.getLength(), 0.0001f, "设置间隔后无帧仍为 0");
	}

	// setEffectAlign: 无 getter, 调用不抛即通过
	private static void testSetEffectAlign()
	{
		myUGUISpriteAnim anim = new myUGUISpriteAnim();
		anim.setEffectAlign(EFFECT_ALIGN.PARENT_BOTTOM);
		anim.setEffectAlign(EFFECT_ALIGN.POSITION_LIST);
		anim.setEffectAlign(EFFECT_ALIGN.NONE);
	}

	// setUseTextureSize: 空实现, 调用不抛即通过
	private static void testSetUseTextureSize()
	{
		myUGUISpriteAnim anim = new myUGUISpriteAnim();
		anim.setUseTextureSize(true);
		anim.setUseTextureSize(false);
	}
}
