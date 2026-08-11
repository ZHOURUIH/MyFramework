using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static TestAssert;

// IUIAnimation 动画系列深度测试(myUGUIImageAnim/myUGUIImageAnimPro/myUGUISpriteAnim/myUGUIRawImageAnim):
//   状态接口(setLoop/getLoop, setInterval/getInterval, setSpeed/getSpeed, setPlayDirection,
//            setAutoHide/isAutoHide, setStartIndex/getStartIndex, setEndIndex/getEndIndex)
//   setTextureSet("") 清空序列 / getTextureFrameCount / setTexturePosList/getTexturePosList
//   setUseTextureSize / setEffectAlign / addPlayEndCallback/addPlayingCallback/clearCallback
//   getPlayState / stop 守卫
public static class MyUIIAnimationTest
{
	public static void Run()
	{
		testImageAnimState();
		testImageAnimCallbacks();
		testImageAnimProState();
		testSpriteAnimState();
		testRawImageAnimState();
	}

	// ═════════════════════════════════════════════════════════════════
	// 辅助
	// ═════════════════════════════════════════════════════════════════
	private static myUGUIImageAnim createImageAnim(out GameObject go)
	{
		go = new GameObject("ImageAnim");
		go.AddComponent<RectTransform>();
		go.AddComponent<Image>();
		myUGUIImageAnim anim = new myUGUIImageAnim();
		anim.setObject(go);
		anim.init();
		return anim;
	}

	private static myUGUIImageAnimPro createImageAnimPro(out GameObject go)
	{
		go = new GameObject("ImageAnimPro");
		go.AddComponent<RectTransform>();
		go.AddComponent<Image>();
		myUGUIImageAnimPro anim = new myUGUIImageAnimPro();
		anim.setObject(go);
		anim.init();
		return anim;
	}

	private static myUGUISpriteAnim createSpriteAnim(out GameObject go)
	{
		go = new GameObject("SpriteAnim");
		go.AddComponent<RectTransform>();
		SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
		renderer.sharedMaterial = null;   // 跳过 myUGUISprite.init 的 MaterialPath 检查
		myUGUISpriteAnim anim = new myUGUISpriteAnim();
		anim.setObject(go);
		anim.init();
		return anim;
	}

	private static myUGUIRawImageAnim createRawImageAnim(out GameObject go)
	{
		go = new GameObject("RawImageAnim");
		go.AddComponent<RectTransform>();
		go.AddComponent<RawImage>();
		// init 要求该组件; 字段必须与 myUGUIRawImageAnim 初始值(EMPTY="")一致,
		// 否则 setTexturePath(null,null,0) 走完流程 → 空列表触发 logError("无效的图片序列帧")
		RawImageAnimPath animPath = go.AddComponent<RawImageAnimPath>();
		animPath.mTexturePath = "";
		animPath.mTextureName = "";
		myUGUIRawImageAnim anim = new myUGUIRawImageAnim();
		anim.setObject(go);
		anim.init();
		return anim;
	}

	// ═════════════════════════════════════════════════════════════════
	// 状态接口(以 myUGUIImageAnim 为代表, 4 类接口一致)
	// ═════════════════════════════════════════════════════════════════
	private static void testImageAnimState()
	{
		myUGUIImageAnim anim = createImageAnim(out GameObject go);
		try
		{
			// 初始状态
			assertEqual(0, anim.getTextureFrameCount(), "初始帧数 0");
			assertNull(anim.getTextureSet(), "初始序列名 null");
			// setTextureSet("") 清空序列(非空名字会走图集资源加载, 不测)
			anim.setTextureSet("");
			assertEqual("", anim.getTextureSet(), "setTextureSet(空) 读回");
			assertEqual(0, anim.getTextureFrameCount(), "空序列帧数 0");
			anim.setTextureSet("");   // 重复幂等(mTextureSetName 相同 return)
			// setTexturePosList 往返 + null 安全
			List<Vector2> posList = new() { new Vector2(1.0f, 2.0f), new Vector2(3.0f, 4.0f) };
			anim.setTexturePosList(posList);
			assertTrue(ReferenceEquals(posList, anim.getTexturePosList()), "getTexturePosList 同一引用");
			anim.setTexturePosList(null);
			assertNull(anim.getTexturePosList(), "null 传参安全");
			// AnimControl 状态 setter/getter
			anim.setLoop(LOOP_MODE.LOOP);
			assertTrue(LOOP_MODE.LOOP == anim.getLoop(), "setLoop 读回");
			anim.setLoop(LOOP_MODE.ONCE);
			assertTrue(LOOP_MODE.ONCE == anim.getLoop(), "setLoop(ONCE) 读回");
			anim.setInterval(0.5f);
			assertEqual(0.5f, anim.getInterval(), 0.001f, "setInterval 读回");
			anim.setSpeed(2.0f);
			assertEqual(2.0f, anim.getSpeed(), 0.001f, "setSpeed 读回");
			anim.setPlayDirection(true);
			assertTrue(anim.getPlayDirection(), "setPlayDirection(true) 读回");
			anim.setPlayDirection(false);
			assertFalse(anim.getPlayDirection(), "setPlayDirection(false) 读回");
			anim.setAutoHide(true);
			assertTrue(anim.isAutoHide(), "setAutoHide(true) 读回");
			anim.setAutoHide(false);
			assertFalse(anim.isAutoHide(), "setAutoHide(false) 读回");
			anim.setStartIndex(1);
			assertEqual(1, anim.getStartIndex(), "setStartIndex 读回");
			anim.setEndIndex(5);
			assertEqual(5, anim.getEndIndex(), "setEndIndex 读回");
			// 守卫式(无 getter)
			anim.setUseTextureSize(true);
			anim.setEffectAlign(EFFECT_ALIGN.POSITION_LIST);
			anim.setEffectAlign(EFFECT_ALIGN.NONE);
			// null 图集: atlas?.getAtlas()==getAtlas()?.getAtlas()(null==null) → 直接 return
			anim.setAtlasWithFirstSprite(null, false, false);
			// 播放状态(AnimControl 初始 STOP)
			assertTrue(PLAY_STATE.STOP == anim.getPlayState(), "初始播放状态 STOP");
			anim.stop();   // 守卫
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// 回调注册/清空
	private static void testImageAnimCallbacks()
	{
		myUGUIImageAnim anim = createImageAnim(out GameObject go);
		try
		{
			anim.addPlayEndCallback((isBreak) => { }, true);
			anim.addPlayEndCallback((isBreak) => { }, false);   // clear=false 追加
			anim.addPlayingCallback((playing) => { }, true);
			anim.addPlayingCallback((playing) => { }, false);
			anim.clearCallback();
			anim.clearCallback();   // 重复清空安全
			anim.addPlayEndCallback((isBreak) => { }, true);
			anim.clearCallback();
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// myUGUIImageAnimPro 状态接口
	private static void testImageAnimProState()
	{
		myUGUIImageAnimPro anim = createImageAnimPro(out GameObject go);
		try
		{
			assertEqual(0, anim.getTextureFrameCount(), "初始帧数 0");
			anim.setTextureSet("");
			assertEqual("", anim.getTextureSet(), "setTextureSet(空) 读回");
			anim.setLoop(LOOP_MODE.PING_PONG);
			assertTrue(LOOP_MODE.PING_PONG == anim.getLoop(), "setLoop 读回");
			anim.setInterval(0.2f);
			assertEqual(0.2f, anim.getInterval(), 0.001f, "setInterval 读回");
			anim.setSpeed(3.0f);
			assertEqual(3.0f, anim.getSpeed(), 0.001f, "setSpeed 读回");
			anim.setAutoHide(true);
			assertTrue(anim.isAutoHide(), "setAutoHide 读回");
			anim.setPlayDirection(true);
			assertTrue(anim.getPlayDirection(), "setPlayDirection 读回");
			anim.setStartIndex(2);
			assertEqual(2, anim.getStartIndex(), "setStartIndex 读回");
			anim.setEndIndex(8);
			assertEqual(8, anim.getEndIndex(), "setEndIndex 读回");
			anim.setUseTextureSize(false);
			anim.setEffectAlign(EFFECT_ALIGN.PARENT_BOTTOM);
			anim.addPlayEndCallback((isBreak) => { }, true);
			anim.addPlayingCallback((playing) => { }, true);
			anim.clearCallback();
			anim.stop();
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// myUGUISpriteAnim 状态接口
	private static void testSpriteAnimState()
	{
		myUGUISpriteAnim anim = createSpriteAnim(out GameObject go);
		try
		{
			assertEqual(0, anim.getTextureFrameCount(), "初始帧数 0");
			anim.setTextureSet("");
			assertEqual("", anim.getTextureSet(), "setTextureSet(空) 读回");
			anim.setLoop(LOOP_MODE.LOOP);
			assertTrue(LOOP_MODE.LOOP == anim.getLoop(), "setLoop 读回");
			anim.setInterval(0.1f);
			assertEqual(0.1f, anim.getInterval(), 0.001f, "setInterval 读回");
			anim.setSpeed(1.5f);
			assertEqual(1.5f, anim.getSpeed(), 0.001f, "setSpeed 读回");
			anim.setAutoHide(true);
			assertTrue(anim.isAutoHide(), "setAutoHide 读回");
			anim.setPlayDirection(false);
			assertFalse(anim.getPlayDirection(), "setPlayDirection 读回");
			anim.setStartIndex(0);
			assertEqual(0, anim.getStartIndex(), "setStartIndex 读回");
			anim.setEndIndex(3);
			assertEqual(3, anim.getEndIndex(), "setEndIndex 读回");
			anim.setUseTextureSize(true);
			anim.setEffectAlign(EFFECT_ALIGN.NONE);
			anim.addPlayEndCallback((isBreak) => { }, true);
			anim.addPlayingCallback((playing) => { }, true);
			anim.clearCallback();
			anim.stop();
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// myUGUIRawImageAnim 状态接口
	private static void testRawImageAnimState()
	{
		myUGUIRawImageAnim anim = createRawImageAnim(out GameObject go);
		try
		{
			assertEqual(0, anim.getTextureFrameCount(), "初始帧数 0(RawImageAnimPath 空字段)");
			anim.setLoop(LOOP_MODE.LOOP);
			assertTrue(LOOP_MODE.LOOP == anim.getLoop(), "setLoop 读回");
			anim.setInterval(0.3f);
			assertEqual(0.3f, anim.getInterval(), 0.001f, "setInterval 读回");
			anim.setSpeed(2.5f);
			assertEqual(2.5f, anim.getSpeed(), 0.001f, "setSpeed 读回");
			anim.setAutoHide(true);
			assertTrue(anim.isAutoHide(), "setAutoHide 读回");
			anim.setPlayDirection(true);
			assertTrue(anim.getPlayDirection(), "setPlayDirection 读回");
			anim.setStartIndex(1);
			assertEqual(1, anim.getStartIndex(), "setStartIndex 读回");
			anim.setEndIndex(4);
			assertEqual(4, anim.getEndIndex(), "setEndIndex 读回");
			anim.setUseTextureSize(true);
			// 与当前路径相同(空=EMPTY) → setTexturePath 提前 return, 不触发空序列 logError
			anim.setTexturePath("", "", 0);
			anim.addPlayEndCallback((isBreak) => { }, true);
			anim.addPlayingCallback((playing) => { }, true);
			anim.stop();
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}
}
