using System.Collections.Generic;
using UnityEngine;
using static TestAssert;

// myUGUIRawImageAnim: RawImage 序列帧动画, 核心委托给纯 C# AnimControl 状态机
// 直接 new 不 init 可测(init 依赖 RawImageAnimPath 组件, 无环境; setTexturePath 依赖资源管理器跳过)
public static class MyUGUIRawImageAnimTest
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
		testSetUseTextureSize();
		testCallbackStorage();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	// 构造默认值: 全部委托 AnimControl 纯 C# 状态机
	private static void testDefaultValues()
	{
		myUGUIRawImageAnim anim = new myUGUIRawImageAnim();
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
		assertEqual("", anim.getTextureSet(), "默认纹理路径为空");
	}

	// setLoop: 三种循环模式读写
	private static void testSetLoop()
	{
		myUGUIRawImageAnim anim = new myUGUIRawImageAnim();
		anim.setLoop(LOOP_MODE.ONCE);
		assertEqual(LOOP_MODE.ONCE, anim.getLoop(), "setLoop(ONCE) 读回");
		anim.setLoop(LOOP_MODE.PING_PONG);
		assertEqual(LOOP_MODE.PING_PONG, anim.getLoop(), "setLoop(PING_PONG) 读回");
		anim.setLoop(LOOP_MODE.LOOP);
		assertEqual(LOOP_MODE.LOOP, anim.getLoop(), "setLoop(LOOP) 读回");
	}

	// setInterval: 间隔读写
	private static void testSetInterval()
	{
		myUGUIRawImageAnim anim = new myUGUIRawImageAnim();
		anim.setInterval(0.1f);
		assertEqual(0.1f, anim.getInterval(), 0.0001f, "setInterval(0.1) 读回");
		anim.setInterval(0.5f);
		assertEqual(0.5f, anim.getInterval(), 0.0001f, "setInterval(0.5) 读回");
	}

	// setSpeed: 速度与间隔换算(speedToInterval(speed) = 0.0333/speed, MathUtility.cs:2591),
	// setSpeed(0) 会除零不测; 只断言 getSpeed 往返 + interval 换算正确值
	private static void testSetSpeed()
	{
		myUGUIRawImageAnim anim = new myUGUIRawImageAnim();
		anim.setSpeed(0.5f);
		assertEqual(0.5f, anim.getSpeed(), 0.0001f, "setSpeed(0.5) getSpeed 往返");
		assertEqual(0.0666f, anim.getInterval(), 0.001f, "setSpeed(0.5) → interval 0.0333/0.5");
		anim.setSpeed(2.0f);
		assertEqual(2.0f, anim.getSpeed(), 0.0001f, "setSpeed(2) getSpeed 往返");
		assertEqual(0.01665f, anim.getInterval(), 0.001f, "setSpeed(2) → interval 0.0333/2");
		anim.setSpeed(1.0f);
		assertEqual(1.0f, anim.getSpeed(), 0.0001f, "setSpeed(1) getSpeed 往返");
		assertEqual(0.0333f, anim.getInterval(), 0.001f, "setSpeed(1) → interval 0.0333");
	}

	// setPlayDirection: 播放方向读写
	private static void testSetPlayDirection()
	{
		myUGUIRawImageAnim anim = new myUGUIRawImageAnim();
		anim.setPlayDirection(false);
		assertFalse(anim.getPlayDirection(), "setPlayDirection(false)");
		anim.setPlayDirection(true);
		assertTrue(anim.getPlayDirection(), "setPlayDirection(true)");
	}

	// setStartIndex/setEndIndex: 下标读写 + getRealEndIndex 联动
	private static void testSetStartEndIndex()
	{
		myUGUIRawImageAnim anim = new myUGUIRawImageAnim();
		anim.setStartIndex(2);
		assertEqual(2, anim.getStartIndex(), "setStartIndex(2) 读回");
		anim.setEndIndex(5);
		assertEqual(5, anim.getEndIndex(), "setEndIndex(5) 读回");
		assertEqual(5, anim.getRealEndIndex(), "显式终止下标时 getRealEndIndex 返回该值");
		anim.setEndIndex(-1);
		assertEqual(0, anim.getRealEndIndex(), "恢复自动终止, 无序列帧时 0");
	}

	// setAutoHide: 自动隐藏读写
	private static void testSetAutoHide()
	{
		myUGUIRawImageAnim anim = new myUGUIRawImageAnim();
		anim.setAutoHide(false);
		assertFalse(anim.isAutoHide(), "setAutoHide(false)");
		anim.setAutoHide(true);
		assertTrue(anim.isAutoHide(), "setAutoHide(true)");
	}

	// play/pause/stop: 纯状态机切换, 无序列帧也安全
	private static void testPlayPauseStop()
	{
		myUGUIRawImageAnim anim = new myUGUIRawImageAnim();
		anim.play();
		assertEqual(PLAY_STATE.PLAY, anim.getPlayState(), "play 后状态 PLAY");
		anim.pause();
		assertEqual(PLAY_STATE.PAUSE, anim.getPlayState(), "pause 后状态 PAUSE");
		anim.stop();
		assertEqual(PLAY_STATE.STOP, anim.getPlayState(), "stop 后状态 STOP");
		anim.play();
		anim.stop();
		assertEqual(PLAY_STATE.STOP, anim.getPlayState(), "play 后立即 stop 回到 STOP");
	}

	// setCurFrameIndex: 无序列帧时 clamp 到 0
	private static void testSetCurFrameIndexClampZero()
	{
		myUGUIRawImageAnim anim = new myUGUIRawImageAnim();
		anim.setCurFrameIndex(5);
		assertEqual(0, anim.getCurFrameIndex(), "无序列帧时 setCurFrameIndex(5) clamp 到 0");
		anim.setCurFrameIndex(-3);
		assertEqual(0, anim.getCurFrameIndex(), "无序列帧时负数 clamp 到 0");
	}

	// setTexturePosList: 位置偏移列表引用存储(含 null)
	private static void testSetTexturePosList()
	{
		myUGUIRawImageAnim anim = new myUGUIRawImageAnim();
		List<Vector2> posList = new List<Vector2> { new Vector2(1, 2), new Vector2(3, 4) };
		anim.setTexturePosList(posList);
		assertTrue(ReferenceEquals(posList, anim.getTexturePosList()), "setTexturePosList 引用存储");
		anim.setTexturePosList(null);
		assertTrue(anim.getTexturePosList() == null, "setTexturePosList(null) 存 null");
	}

	// setUseTextureSize: 无 getter, 调用不抛即通过
	private static void testSetUseTextureSize()
	{
		myUGUIRawImageAnim anim = new myUGUIRawImageAnim();
		anim.setUseTextureSize(true);
		anim.setUseTextureSize(false);
		// 无异常即通过
	}

	// 回调存储: addPlayEndCallback 二次添加触发旧回调(中断语义), addPlayingCallback 只 Clear 不触发
	private static void testCallbackStorage()
	{
		myUGUIRawImageAnim anim = new myUGUIRawImageAnim();
		int endCallCount = 0;
		int playingCallCount = 0;
		// 首次添加(列表为空): 不触发
		anim.addPlayEndCallback((isBreak) => ++endCallCount);
		assertEqual(0, endCallCount, "首次添加播放结束回调不触发");
		// 再次添加(clear=true): 触发旧回调 1 次(isBreak=true, 中断通知)
		anim.addPlayEndCallback((isBreak) => ++endCallCount);
		assertEqual(1, endCallCount, "再次添加触发旧回调 1 次(中断语义)");
		// addPlayingCallback 添加不触发
		anim.addPlayingCallback((isPlaying) => ++playingCallCount);
		anim.addPlayingCallback((isPlaying) => ++playingCallCount);
		assertEqual(0, playingCallCount, "addPlayingCallback 添加不触发");
		// 传 null 不崩
		anim.addPlayEndCallback(null);
		anim.addPlayingCallback(null);
	}
}
