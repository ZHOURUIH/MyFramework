using System.Collections.Generic;
using UnityEngine;
using static TestAssert;

// myUGUIImageAnimPro: Image 序列帧动画(Pro 变体), 核心委托给纯 C# AnimControl 状态机,
// 与 myUGUIImageAnim/myUGUISpriteAnim 同模式, 直接 new 不 init 可测
public static class MyUGUIImageAnimProTest
{
	public static void Run()
	{
		testDefaultValues();
		testSetLoop();
		testSetInterval();
		testSetSpeed();
		testSetPlayDirection();
		testSetStartEndIndex();
		testSetAutoHide();
		testPlayPauseStop();
		testSetCurFrameIndexClampZero();
		testSetTexturePosList();
		testSetEffectAlign();
		testSetUseTextureSize();
		testGetLength();
		testCallbackStorage();
	}

	// 构造默认值: 纯 C# 对象直接 new 可测(不 init, 避免 ImageAtlasPath 组件依赖)
	private static void testDefaultValues()
	{
		myUGUIImageAnimPro anim = new myUGUIImageAnimPro();
		assertEqual(LOOP_MODE.LOOP, anim.getLoop(), "默认循环模式 LOOP");
		assertEqual(0.033f, anim.getInterval(), 0.0001f, "默认间隔 0.033");
		assertEqual(0, anim.getStartIndex(), "默认起始下标 0");
		assertEqual(-1, anim.getEndIndex(), "默认终止下标 -1(自动)");
		assertEqual(PLAY_STATE.STOP, anim.getPlayState(), "默认播放状态 STOP");
		assertTrue(anim.getPlayDirection(), "默认正向播放");
		assertTrue(anim.isAutoHide(), "默认自动隐藏");
		assertEqual(0, anim.getCurFrameIndex(), "默认当前帧 0");
		assertEqual(0, anim.getRealEndIndex(), "无序列帧时实际终止下标 0");
		assertEqual(0, anim.getTextureFrameCount(), "无序列帧时帧数 0");
		assertEqual(0.0f, anim.getLength(), 0.0001f, "无序列帧时总时长 0");
		assertTrue(anim.getTextureSet() == null, "默认序列帧名为 null");
	}

	// setLoop: 循环模式读写
	private static void testSetLoop()
	{
		myUGUIImageAnimPro anim = new myUGUIImageAnimPro();
		anim.setLoop(LOOP_MODE.ONCE);
		assertEqual(LOOP_MODE.ONCE, anim.getLoop(), "setLoop(ONCE)");
		anim.setLoop(LOOP_MODE.PING_PONG);
		assertEqual(LOOP_MODE.PING_PONG, anim.getLoop(), "setLoop(PING_PONG)");
		anim.setLoop(LOOP_MODE.LOOP);
		assertEqual(LOOP_MODE.LOOP, anim.getLoop(), "setLoop(LOOP)");
	}

	// setInterval: 间隔读写
	private static void testSetInterval()
	{
		myUGUIImageAnimPro anim = new myUGUIImageAnimPro();
		anim.setInterval(0.5f);
		assertEqual(0.5f, anim.getInterval(), 0.0001f, "setInterval(0.5) 读回");
		anim.setInterval(0.1f);
		assertEqual(0.1f, anim.getInterval(), 0.0001f, "setInterval(0.1) 读回");
	}

	// setSpeed: 速度与间隔换算(speedToInterval(speed) = 0.0333/speed, MathUtility.cs:2591),
	// setSpeed(0) 会除零不测; 断言 getSpeed 往返 + interval 换算正确值
	private static void testSetSpeed()
	{
		myUGUIImageAnimPro anim = new myUGUIImageAnimPro();
		anim.setSpeed(0.5f);
		assertEqual(0.5f, anim.getSpeed(), 0.0001f, "setSpeed(0.5) getSpeed 往返");
		assertEqual(0.0666f, anim.getInterval(), 0.001f, "setSpeed(0.5) → interval 0.0333/0.5");
		anim.setSpeed(2.0f);
		assertEqual(2.0f, anim.getSpeed(), 0.0001f, "setSpeed(2) getSpeed 往返");
		assertEqual(0.01665f, anim.getInterval(), 0.001f, "setSpeed(2) → interval 0.0333/2");
	}

	// setPlayDirection: 播放方向读写
	private static void testSetPlayDirection()
	{
		myUGUIImageAnimPro anim = new myUGUIImageAnimPro();
		anim.setPlayDirection(false);
		assertFalse(anim.getPlayDirection(), "setPlayDirection(false)");
		anim.setPlayDirection(true);
		assertTrue(anim.getPlayDirection(), "setPlayDirection(true)");
	}

	// setStartIndex/setEndIndex: 读写 + getRealEndIndex 联动(显式 endIndex 直接返回, -1 时回落无帧最大值 0)
	private static void testSetStartEndIndex()
	{
		myUGUIImageAnimPro anim = new myUGUIImageAnimPro();
		anim.setStartIndex(2);
		assertEqual(2, anim.getStartIndex(), "setStartIndex(2)");
		anim.setEndIndex(5);
		assertEqual(5, anim.getEndIndex(), "setEndIndex(5)");
		assertEqual(5, anim.getRealEndIndex(), "显式终止下标时 getRealEndIndex 返回该值");
		anim.setEndIndex(-1);
		assertEqual(0, anim.getRealEndIndex(), "恢复自动终止, 无帧时回落 0");
	}

	// setAutoHide: 播放完自动隐藏读写
	private static void testSetAutoHide()
	{
		myUGUIImageAnimPro anim = new myUGUIImageAnimPro();
		anim.setAutoHide(false);
		assertFalse(anim.isAutoHide(), "setAutoHide(false)");
		anim.setAutoHide(true);
		assertTrue(anim.isAutoHide(), "setAutoHide(true)");
	}

	// play/pause/stop: 纯状态机切换(无序列帧也安全)
	private static void testPlayPauseStop()
	{
		myUGUIImageAnimPro anim = new myUGUIImageAnimPro();
		anim.play();
		assertEqual(PLAY_STATE.PLAY, anim.getPlayState(), "play 后状态 PLAY");
		anim.pause();
		assertEqual(PLAY_STATE.PAUSE, anim.getPlayState(), "pause 后状态 PAUSE");
		anim.stop();
		assertEqual(PLAY_STATE.STOP, anim.getPlayState(), "stop 后状态 STOP");
	}

	// setCurFrameIndex: 无序列帧时任意值 clamp 到 0
	private static void testSetCurFrameIndexClampZero()
	{
		myUGUIImageAnimPro anim = new myUGUIImageAnimPro();
		anim.setCurFrameIndex(5);
		assertEqual(0, anim.getCurFrameIndex(), "无帧时 setCurFrameIndex(5) clamp 到 0");
		anim.setCurFrameIndex(-3);
		assertEqual(0, anim.getCurFrameIndex(), "无帧时负数 clamp 到 0");
	}

	// setTexturePosList: 引用存储(含 null)
	private static void testSetTexturePosList()
	{
		myUGUIImageAnimPro anim = new myUGUIImageAnimPro();
		List<Vector2> posList = new List<Vector2> { new Vector2(1, 2), new Vector2(3, 4) };
		anim.setTexturePosList(posList);
		assertTrue(ReferenceEquals(posList, anim.getTexturePosList()), "setTexturePosList 引用存储");
		anim.setTexturePosList(null);
		assertTrue(anim.getTexturePosList() == null, "setTexturePosList(null) 存 null");
	}

	// setEffectAlign: 无 getter, 调用不抛即通过
	private static void testSetEffectAlign()
	{
		myUGUIImageAnimPro anim = new myUGUIImageAnimPro();
		anim.setEffectAlign(EFFECT_ALIGN.PARENT_BOTTOM);
		anim.setEffectAlign(EFFECT_ALIGN.POSITION_LIST);
		anim.setEffectAlign(EFFECT_ALIGN.NONE);
	}

	// setUseTextureSize: 无 getter, 调用不抛即通过
	private static void testSetUseTextureSize()
	{
		myUGUIImageAnimPro anim = new myUGUIImageAnimPro();
		anim.setUseTextureSize(true);
		anim.setUseTextureSize(false);
	}

	// getLength: 总时长 = 帧数 * 间隔, 无帧时为 0
	private static void testGetLength()
	{
		myUGUIImageAnimPro anim = new myUGUIImageAnimPro();
		assertEqual(0.0f, anim.getLength(), 0.0001f, "无帧时 getLength 0");
		anim.setInterval(0.5f);
		assertEqual(0.0f, anim.getLength(), 0.0001f, "设置间隔后无帧仍为 0");
	}

	// 回调存储: addPlayEndCallback 有"添加时触发旧回调(isBreak=true)"的中断语义;
	// addPlayingCallback 只 Clear 不触发; clearCallback 清空后不崩
	private static void testCallbackStorage()
	{
		myUGUIImageAnimPro anim = new myUGUIImageAnimPro();
		int endCallCount = 0;
		int playingCallCount = 0;
		// 首次添加(列表为空): 不触发
		anim.addPlayEndCallback((isBreak) => ++endCallCount);
		assertEqual(0, endCallCount, "首次添加播放结束回调不触发");
		// 再次添加(clear=true): 触发旧回调 1 次(中断通知 isBreak=true)
		anim.addPlayEndCallback((isBreak) => ++endCallCount);
		assertEqual(1, endCallCount, "再次添加触发旧回调 1 次(中断语义)");
		// addPlayingCallback(clear=true) 只 Clear 不触发
		anim.addPlayingCallback((isPlaying) => ++playingCallCount);
		anim.addPlayingCallback((isPlaying) => ++playingCallCount);
		assertEqual(0, playingCallCount, "addPlayingCallback 添加不触发");
		// clearCallback 清空 + 传 null 不崩
		anim.clearCallback();
		anim.addPlayEndCallback(null);
		anim.addPlayingCallback(null);
		anim.clearCallback();
	}
}
