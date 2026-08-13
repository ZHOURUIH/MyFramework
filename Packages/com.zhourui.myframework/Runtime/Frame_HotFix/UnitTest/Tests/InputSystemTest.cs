using System;
using System.Collections.Generic;
using UnityEngine;
using static FrameUtility;
using static TestAssert;

// InputSystem 单元测试
// 覆盖可脱离真实输入设备/框架环境的纯逻辑:
//   setMask / hasMask 焦点掩码判断
//   registeInputField / unregisteInputField 输入框登记
//   KeyListenInfo.resetProperty (ClassObject)
// 注: 依赖真实按键/触点的 update 逻辑需运行时, 不在此覆盖
public static class InputSystemTest
{
	public static void Run()
	{
		// ─── FOCUS_MASK 掩码 ───
		testDefaultMask();
		testSetMaskScene();
		testSetMaskUI();
		testSetMaskBoth();
		testMaskNoneAlwaysTrue();
		testMaskNoneWhenMaskZero();
		testMaskNonOverlap();
		// ─── 输入框登记 ───
		testRegisteUnregisteInputField();
		// ─── KeyListenInfo ───
		testKeyListenInfoDefault();
		testKeyListenInfoReset();
	

		testListenSingleKey();
		testUnlistenRemovesAll();
		testListenMultipleKeysOneListener();
		testMultipleListenersIndependent();
		testUnlistenNonExistent();
		testSameKeyMultipleListeners();
	}

	// ═════════════════════════════════════════════════════════════════
	// FOCUS_MASK
	// ═════════════════════════════════════════════════════════════════
	private static void testDefaultMask()
	{
		InputSystem input = new();
		// hasMask: mask==NONE || mFocusMask==0 || (mFocusMask&mask)!=0
		// 默认 mFocusMask=0, 因 mFocusMask==0 分支, 任何掩码都返回 true
		assertTrue(input.hasMask(FOCUS_MASK.NONE), "NONE 掩码始终 true");
		assertTrue(input.hasMask(FOCUS_MASK.SCENE), "mask=0 时 SCENE 为 true (mFocusMask==0 分支)");
		assertTrue(input.hasMask(FOCUS_MASK.UI), "mask=0 时 UI 为 true (mFocusMask==0 分支)");
	}
	private static void testSetMaskScene()
	{
		InputSystem input = new();
		input.setMask(FOCUS_MASK.SCENE);
		assertTrue(input.hasMask(FOCUS_MASK.SCENE), "set SCENE 后 hasMask(SCENE) true");
		assertFalse(input.hasMask(FOCUS_MASK.UI), "仅 SCENE 时 UI false");
	}
	private static void testSetMaskUI()
	{
		InputSystem input = new();
		input.setMask(FOCUS_MASK.UI);
		assertTrue(input.hasMask(FOCUS_MASK.UI), "set UI 后 hasMask(UI) true");
		assertFalse(input.hasMask(FOCUS_MASK.SCENE), "仅 UI 时 SCENE false");
	}
	private static void testSetMaskBoth()
	{
		InputSystem input = new();
		input.setMask((FOCUS_MASK)((int)FOCUS_MASK.SCENE | (int)FOCUS_MASK.UI));
		assertTrue(input.hasMask(FOCUS_MASK.SCENE));
		assertTrue(input.hasMask(FOCUS_MASK.UI));
	}
	private static void testMaskNoneAlwaysTrue()
	{
		InputSystem input = new();
		input.setMask(FOCUS_MASK.SCENE);
		assertTrue(input.hasMask(FOCUS_MASK.NONE), "NONE 掩码始终 true, 与当前 mask 无关");
	}
	private static void testMaskNoneWhenMaskZero()
	{
		InputSystem input = new();
		// mask 不为 NONE, 但当前掩码为0 → 因 mFocusMask==0 分支返回 true
		assertTrue(input.hasMask(FOCUS_MASK.UI), "mFocusMask==0 时非 NONE 掩码 true");
	}
	private static void testMaskNonOverlap()
	{
		InputSystem input = new();
		input.setMask(FOCUS_MASK.SCENE);
		// UI(1<<2) 与 SCENE(1<<1) 不重叠
		assertFalse(input.hasMask(FOCUS_MASK.UI));
	}

	// ═════════════════════════════════════════════════════════════════
	// 输入框登记
	// ═════════════════════════════════════════════════════════════════
	private static void testRegisteUnregisteInputField()
	{
		InputSystem input = new();
		TestInputField field = new();
		input.registeInputField(field);
		// 无 getter 直接验证, 但 unregiste 不应抛异常(存在即登记成功)
		input.unregisteInputField(field);
		// 重复移除不应抛异常
		input.unregisteInputField(field);
	}

	// ═════════════════════════════════════════════════════════════════
	// KeyListenInfo
	// ═════════════════════════════════════════════════════════════════
	private static void testKeyListenInfoDefault()
	{
		KeyListenInfo info = new();
		assertNull(info.mCallback);
		assertNull(info.mListener);
		assertEqual(COMBINATION_KEY.NONE, info.mCombinationKey);
		assertEqual(UnityEngine.KeyCode.None, info.mKey);
	}
	private static void testKeyListenInfoReset()
	{
		KeyListenInfo info = new();
		info.mCallback = () => { };
		info.mListener = new TestEventListener();
		info.mCombinationKey = COMBINATION_KEY.CTRL;
		info.mKey = UnityEngine.KeyCode.A;
		info.resetProperty();
		assertNull(info.mCallback, "reset 后回调清空");
		assertNull(info.mListener, "reset 后监听者清空");
		assertEqual(COMBINATION_KEY.NONE, info.mCombinationKey, "reset 后组合键 NONE");
		assertEqual(UnityEngine.KeyCode.None, info.mKey, "reset 后按键 None");
	}


	// 测试子类, 暴露 protected 字典
	class TestInputSystem : InputSystem
	{
		public int GetKeyListenCount(KeyCode key)
		{
			return mKeyListenList.tryGetValue(key, out var list) ? list.count() : 0;
		}
		public int GetListenerInfoCount(IEventListener listener)
		{
			return mListenerList.TryGetValue(listener, out var list) ? list.Count : 0;
		}
	}

	class TestListener : IEventListener { }

	

	// ═════════════════════════════════════════════════════════════════
	// 注册单个按键监听 → 两个字典都写入
	// ═════════════════════════════════════════════════════════════════
	private static void testListenSingleKey()
	{
		var sys = new TestInputSystem();
		var listener = new TestListener();
		sys.listenKeyCurrentDown(KeyCode.W, () => { }, listener);

		assertEqual(1, sys.GetKeyListenCount(KeyCode.W), "W 键注册了1个回调");
		assertEqual(1, sys.GetListenerInfoCount(listener), "监听者注册了1条信息");
	}

	// ═════════════════════════════════════════════════════════════════
	// unlistenKey 从两个字典同步移除该监听者的所有回调
	// ═════════════════════════════════════════════════════════════════
	private static void testUnlistenRemovesAll()
	{
		var sys = new TestInputSystem();
		var listener = new TestListener();
		sys.listenKeyCurrentDown(KeyCode.W, () => { }, listener);
		sys.listenKeyCurrentDown(KeyCode.A, () => { }, listener);
		sys.listenKeyCurrentDown(KeyCode.S, () => { }, listener);

		assertEqual(1, sys.GetKeyListenCount(KeyCode.W), "W 有1个");
		assertEqual(1, sys.GetKeyListenCount(KeyCode.A), "A 有1个");
		assertEqual(1, sys.GetKeyListenCount(KeyCode.S), "S 有1个");
		assertEqual(3, sys.GetListenerInfoCount(listener), "监听者有3条信息");

		sys.unlistenKey(listener);
		assertEqual(0, sys.GetKeyListenCount(KeyCode.W), "unlisten 后 W 清空");
		assertEqual(0, sys.GetKeyListenCount(KeyCode.A), "unlisten 后 A 清空");
		assertEqual(0, sys.GetKeyListenCount(KeyCode.S), "unlisten 后 S 清空");
		assertEqual(0, sys.GetListenerInfoCount(listener), "unlisten 后监听者信息清空");
	}

	// ═════════════════════════════════════════════════════════════════
	// 同一监听者同一按键注册多个回调 → 都进入, unlisten 全移除
	// ═════════════════════════════════════════════════════════════════
	private static void testListenMultipleKeysOneListener()
	{
		var sys = new TestInputSystem();
		var listener = new TestListener();
		sys.listenKeyCurrentDown(KeyCode.W, () => { }, listener);
		sys.listenKeyCurrentDown(KeyCode.W, () => { }, listener);

		assertEqual(2, sys.GetKeyListenCount(KeyCode.W), "同一按键注册2个回调");
		assertEqual(2, sys.GetListenerInfoCount(listener), "监听者有2条信息");

		sys.unlistenKey(listener);
		assertEqual(0, sys.GetKeyListenCount(KeyCode.W), "unlisten 后清空");
	}

	// ═════════════════════════════════════════════════════════════════
	// 多个监听者不同按键 → unlisten 一个不影响其他
	// ═════════════════════════════════════════════════════════════════
	private static void testMultipleListenersIndependent()
	{
		var sys = new TestInputSystem();
		var l1 = new TestListener();
		var l2 = new TestListener();
		sys.listenKeyCurrentDown(KeyCode.W, () => { }, l1);
		sys.listenKeyCurrentDown(KeyCode.A, () => { }, l2);

		sys.unlistenKey(l1);
		assertEqual(0, sys.GetKeyListenCount(KeyCode.W), "unlisten l1 后 W 清空");
		assertEqual(1, sys.GetKeyListenCount(KeyCode.A), "l2 的 A 不受影响");
		assertEqual(0, sys.GetListenerInfoCount(l1), "l1 信息清空");
		assertEqual(1, sys.GetListenerInfoCount(l2), "l2 信息保留");
	}

	// ═════════════════════════════════════════════════════════════════
	// unlisten 未注册的监听者 → 不崩溃
	// ═════════════════════════════════════════════════════════════════
	private static void testUnlistenNonExistent()
	{
		var sys = new TestInputSystem();
		sys.unlistenKey(new TestListener());
		// 不崩溃
	}

	// ═════════════════════════════════════════════════════════════════
	// 同一按键多个监听者 → unlisten 一个只移除自己的
	// ═════════════════════════════════════════════════════════════════
	private static void testSameKeyMultipleListeners()
	{
		var sys = new TestInputSystem();
		var l1 = new TestListener();
		var l2 = new TestListener();
		sys.listenKeyCurrentDown(KeyCode.W, () => { }, l1);
		sys.listenKeyCurrentDown(KeyCode.W, () => { }, l2);

		assertEqual(2, sys.GetKeyListenCount(KeyCode.W), "同一按键2个监听者");

		sys.unlistenKey(l1);
		assertEqual(1, sys.GetKeyListenCount(KeyCode.W), "移除l1后剩l2的1个");
		assertEqual(0, sys.GetListenerInfoCount(l1), "l1信息清空");
		assertEqual(1, sys.GetListenerInfoCount(l2), "l2信息保留");
	}
}

// 测试用 IInputField 实现
public class TestInputField : IInputField
{
	public bool isFocused() { return false; }
	public bool isVisible() { return true; }
}

// 测试用 IEventListener 实现
public class TestEventListener : IEventListener
{
	public void resetProperty() { }
}
