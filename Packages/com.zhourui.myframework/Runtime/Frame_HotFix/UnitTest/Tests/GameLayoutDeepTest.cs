using System.Collections.Generic;
using UnityEngine;
using UObject = UnityEngine.Object;
using static TestAssert;

// GameLayout 深度测试: 布局容器生命周期 + LayoutScript 生命周期(纯逻辑/空安全路径)
// 不调 GameLayout.init(依赖 mLayoutManager.createScript + 真实布局 prefab 文件, 测试环境不可行)
// 覆盖:
//   - registerUIObject/unregisterUIObject 容器操作 + getUIObject 反查
//   - notifyUIObjectNeedUpdate 增删 mNeedUpdateList + canUIObjectUpdate
//   - update() 空安全(mRoot=null → isVisible()=false → 直接 return)
//   - LayoutScript setLayout/setRoot 后 updateAllDragView/onHide 空安全(空容器 + 空安全依赖链)
public static class GameLayoutDeepTest
{
	public static void Run()
	{
		testRegisterUnregisterUIObject();
		testRegisterMultipleAndLookup();
		testNotifyNeedUpdate();
		testUpdateNullRootSafe();
		testScriptSetLayoutRoot();
		testScriptUpdateAllDragViewEmpty();
		testScriptOnHideEmpty();
		testDestroyNullRoot();
	}

	// ═════════════════════════════════════════════════════════════════
	// 辅助: 创建带 RectTransform 的 myUGUIObject
	// ═════════════════════════════════════════════════════════════════
	private static myUGUIObject createUI()
	{
		GameObject go = new GameObject("TestLayoutUI");
		go.AddComponent<RectTransform>();
		myUGUIObject ui = new myUGUIObject();
		ui.setObject(go);
		ui.init();
		return ui;
	}

	// ═════════════════════════════════════════════════════════════════
	// registerUIObject → 加入 mObjectList/mGameObjectSearchList/mNeedUpdateList
	// unregisterUIObject → 全部移除
	// ═════════════════════════════════════════════════════════════════
	private static void testRegisterUnregisterUIObject()
	{
		GameLayout layout = new GameLayout();
		myUGUIObject ui = createUI();
		try
		{
			layout.registerUIObject(ui);
			Dictionary<int, myUGUIObject> list = layout.getUIObjectList();
			assertEqual(1, list.Count, "register 后 mGameObjectSearchList 含 1 个");
			assertTrue(ReferenceEquals(ui, layout.getUIObject(ui.getGameObject())), "getUIObject 按 GameObject 反查命中");
			// 默认 mDefaultUpdateWindow=true → 注册后进入 mNeedUpdateList
			assertTrue(layout.canUIObjectUpdate(ui), "mDefaultUpdateWindow=true → 注册后 canUIObjectUpdate");
			layout.unregisterUIObject(ui);
			assertEqual(0, layout.getUIObjectList().Count, "unregister 后 mGameObjectSearchList 清空");
			assertFalse(layout.canUIObjectUpdate(ui), "unregister 后 canUIObjectUpdate=false");
			assertNull(layout.getUIObject(ui.getGameObject()), "unregister 后 getUIObject 返回 null");
		}
		finally
		{
			layout.unregisterUIObject(ui);
			UObject.DestroyImmediate(ui.getGameObject());
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// register 多个 UI → 列表计数 + 独立反查
	// ═════════════════════════════════════════════════════════════════
	private static void testRegisterMultipleAndLookup()
	{
		GameLayout layout = new GameLayout();
		myUGUIObject ui1 = createUI();
		myUGUIObject ui2 = createUI();
		try
		{
			layout.registerUIObject(ui1);
			layout.registerUIObject(ui2);
			assertEqual(2, layout.getUIObjectList().Count, "register 2 个后列表含 2 个");
			assertTrue(ReferenceEquals(ui1, layout.getUIObject(ui1.getGameObject())), "反查 ui1");
			assertTrue(ReferenceEquals(ui2, layout.getUIObject(ui2.getGameObject())), "反查 ui2");
			// 不相同的 GameObject 反查不到
			GameObject other = new GameObject("Other");
			try
			{
				assertNull(layout.getUIObject(other), "未注册的 GameObject 反查 null");
			}
			finally
			{
				UObject.DestroyImmediate(other);
			}
		}
		finally
		{
			layout.unregisterUIObject(ui1);
			layout.unregisterUIObject(ui2);
			UObject.DestroyImmediate(ui1.getGameObject());
			UObject.DestroyImmediate(ui2.getGameObject());
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// notifyUIObjectNeedUpdate — 显式增删 mNeedUpdateList
	// ═════════════════════════════════════════════════════════════════
	private static void testNotifyNeedUpdate()
	{
		GameLayout layout = new GameLayout();
		myUGUIObject ui = createUI();
		try
		{
			// 初始未注册 → canUIObjectUpdate=false
			assertFalse(layout.canUIObjectUpdate(ui), "未注册时 canUIObjectUpdate=false");
			layout.notifyUIObjectNeedUpdate(ui, true);
			assertTrue(layout.canUIObjectUpdate(ui), "notify(true) 后 canUIObjectUpdate=true");
			layout.notifyUIObjectNeedUpdate(ui, false);
			assertFalse(layout.canUIObjectUpdate(ui), "notify(false) 后 canUIObjectUpdate=false");
			// 再次添加→移除
			layout.notifyUIObjectNeedUpdate(ui, true);
			assertTrue(layout.canUIObjectUpdate(ui), "二次 notify(true) 后 canUIObjectUpdate=true");
			layout.notifyUIObjectNeedUpdate(ui, false);
			assertFalse(layout.canUIObjectUpdate(ui), "二次 notify(false) 后 canUIObjectUpdate=false");
			// 移除不存在的元素安全(SafeList.remove 返回 false)
			layout.notifyUIObjectNeedUpdate(ui, false);
			assertFalse(layout.canUIObjectUpdate(ui), "空列表 notify(false) 仍 false");
		}
		finally
		{
			layout.notifyUIObjectNeedUpdate(ui, false);
			UObject.DestroyImmediate(ui.getGameObject());
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// update — mRoot=null → isVisible()=false → 直接 return 不抛异常
	// ═════════════════════════════════════════════════════════════════
	private static void testUpdateNullRootSafe()
	{
		GameLayout layout = new GameLayout();
		assertFalse(layout.isVisible(), "mRoot=null → isVisible=false");
		layout.update(0.1f);   // 空安全
		layout.lateUpdate(0.1f); // 空安全
	}

	// ═════════════════════════════════════════════════════════════════
	// LayoutScript — setLayout/setRoot 读写
	// ═════════════════════════════════════════════════════════════════
	private static void testScriptSetLayoutRoot()
	{
		GameLayout layout = new GameLayout();
		LayoutScript script = new TestLayoutScript();
		assertNull(script.getLayout(), "默认 getLayout=null");
		script.setLayout(layout);
		assertTrue(ReferenceEquals(layout, script.getLayout()), "setLayout 后 getLayout 同一引用");
		assertNull(script.getRoot(), "默认 getRoot=null");
		myUGUIObject root = createUI();
		try
		{
			script.setRoot(root);
			assertTrue(ReferenceEquals(root, script.getRoot()), "setRoot 后 getRoot 同一引用");
		}
		finally
		{
			UObject.DestroyImmediate(root.getGameObject());
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// updateAllDragView — 空容器空安全
	// ═════════════════════════════════════════════════════════════════
	private static void testScriptUpdateAllDragViewEmpty()
	{
		LayoutScript script = new TestLayoutScript();
		script.setLayout(new GameLayout());
		// 空 mDragViewLoopList/mWindowObjectRootList → safe() 空安全
		script.updateAllDragView();
		script.update(0.1f);      // 空实现
		script.lateUpdate(0.1f);  // 空实现
	}

	// ═════════════════════════════════════════════════════════════════
	// onHide — 空容器 + 空安全依赖链(mEventSystem?/mInputSystem?/mPoolRootList.safe())
	// ═════════════════════════════════════════════════════════════════
	private static void testScriptOnHideEmpty()
	{
		LayoutScript script = new TestLayoutScript();
		script.setLayout(new GameLayout());
		script.onHide();
		// onHide 内部: interruptAllCommand/clearLocalization/mEventSystem?.unlistenEvent
		//            /mWindowObjectRootList.safe() 空容器/mPoolRootList.For 空容器/mInputSystem?.unlistenKey
		script.onHide(); // 重复调用也安全
	}

	// ═════════════════════════════════════════════════════════════════
	// destroy — mRoot=null 时仅 mScript=null 分支跳过 + destroyWindow(null) 安全
	// (mPrefab 为 null → mResourceManager.unload 需确认 null 安全)
	// ═════════════════════════════════════════════════════════════════
	private static void testDestroyNullRoot()
	{
		GameLayout layout = new GameLayout();
		// mScript=null → 跳过 mScript.destroy/mLayoutManager.notifyLayoutChanged
		// mRoot=null → destroyWindow(null, true) 空安全(MyUGUIObjectTest 已验证)
		layout.destroy();
	}
}
