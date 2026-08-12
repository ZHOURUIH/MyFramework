using UnityEngine;
using UnityEngine.UI;
using static TestAssert;

// myUGUIButton 深度测试
// 对 UGUI Button 组件的封装:
//   init: 无 Button 组件时自动添加(isNewObject 不 logError), 有则直接绑定
//   setUGUIButtonClick: 注册点击回调(UnityEvent.AddListener), 通过 onClick.Invoke() 触发
//
// 环境: 裸 GameObject + RectTransform + Image + Button + myUGUIButton(setObject+init)
// 清理: 测试自己 new 的裸 GameObject, 手动 DestroyImmediate
public static class MyUGUIButtonTest
{
	public static void Run()
	{
		testButtonInitWithComponents();
		testButtonInitAutoAddButton();
		testButtonSetClickCallback();
		testButtonMultipleClickCallbacks();
		testButtonClickCallbackOrder();
	}

	// ═════════════════════════════════════════════════════════════════
	// 辅助: 创建已 init 的 myUGUIButton(预加 Image + Button 组件)
	// ═════════════════════════════════════════════════════════════════
	private static myUGUIButton createButton(out GameObject go, out Button unityButton)
	{
		go = new GameObject("ButtonGO");
		go.AddComponent<RectTransform>();
		go.AddComponent<Image>();
		unityButton = go.AddComponent<Button>();
		myUGUIButton ui = new myUGUIButton();
		ui.setObject(go);
		ui.init();
		return ui;
	}

	// init: 预加 Image + Button 组件 → 直接绑定, 无 logError
	private static void testButtonInitWithComponents()
	{
		myUGUIButton ui = createButton(out GameObject go, out Button unityButton);
		try
		{
			// setUGUIButtonClick 内部依赖 mButton 有效
			bool called = false;
			ui.setUGUIButtonClick(() => called = true);
			unityButton.onClick.Invoke();
			assertTrue(called, "init 后 Button 组件绑定成功, 点击回调可触发");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// init: 无 Button 组件 + isNewObject=true → 自动 AddComponent
	private static void testButtonInitAutoAddButton()
	{
		GameObject go = new GameObject("ButtonAuto");
		go.AddComponent<RectTransform>();
		go.AddComponent<Image>();
		try
		{
			myUGUIButton ui = new myUGUIButton();
			ui.setIsNewObject(true);   // 自动补组件, 不触发 logError
			ui.setObject(go);
			ui.init();
			// 自动添加的 Button 可正常注册回调
			bool called = false;
			ui.setUGUIButtonClick(() => called = true);
			go.GetComponent<Button>().onClick.Invoke();
			assertTrue(called, "自动添加的 Button 组件可注册点击回调");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setUGUIButtonClick: 点击触发回调(UnityEvent 链)
	private static void testButtonSetClickCallback()
	{
		myUGUIButton ui = createButton(out GameObject go, out Button unityButton);
		try
		{
			int callbackCount = 0;
			ui.setUGUIButtonClick(() => callbackCount++);
			// 点击 3 次
			unityButton.onClick.Invoke();
			unityButton.onClick.Invoke();
			unityButton.onClick.Invoke();
			assertEqual(3, callbackCount, "点击 3 次回调触发 3 次");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setUGUIButtonClick: 多个回调依次触发
	private static void testButtonMultipleClickCallbacks()
	{
		myUGUIButton ui = createButton(out GameObject go, out Button unityButton);
		try
		{
			int countA = 0;
			int countB = 0;
			ui.setUGUIButtonClick(() => countA++);
			ui.setUGUIButtonClick(() => countB++);
			unityButton.onClick.Invoke();
			assertEqual(1, countA, "回调 A 触发");
			assertEqual(1, countB, "回调 B 触发");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// setUGUIButtonClick: 未注册回调时点击不崩溃
	private static void testButtonClickCallbackOrder()
	{
		myUGUIButton ui = createButton(out GameObject go, out Button unityButton);
		try
		{
			// 未注册任何回调时 Invoke 安全
			unityButton.onClick.Invoke();
			// 注册后再触发
			int count = 0;
			ui.setUGUIButtonClick(() => count++);
			unityButton.onClick.Invoke();
			assertEqual(1, count, "注册后点击触发 1 次");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}
}
