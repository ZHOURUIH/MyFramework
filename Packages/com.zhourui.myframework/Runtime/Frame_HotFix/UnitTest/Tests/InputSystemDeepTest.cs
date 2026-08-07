using System;
using System.Collections.Generic;
using UnityEngine;
using static FrameUtility;
using static TestAssert;

// InputSystem 深度测试 — 按键监听注册/移除的双字典同步
// 覆盖 listenKeyCurrentDown / unlistenKey 的复杂注册表维护逻辑
//   listenKeyCurrentDown: 同时写入 mKeyListenList(按键→回调) 和 mListenerList(监听者→回调)
//   unlistenKey: 从两个字典同步移除该监听者的所有回调
// 这些是纯逻辑, 不依赖 UnityEngine.Input, 通过测试子类暴露内部字典精确断言同步状态
public static class InputSystemDeepTest
{
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

	public static void Run()
	{
		testListenSingleKey();
		testUnlistenRemovesAll();
		testListenMultipleKeysOneListener();
		testMultipleListenersIndependent();
		testUnlistenNonExistent();
		testSameKeyMultipleListeners();
	}

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
