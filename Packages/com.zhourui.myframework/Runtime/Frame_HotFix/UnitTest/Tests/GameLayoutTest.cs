using static TestAssert;

using System.Collections.Generic;
using UnityEngine;
using UObject = UnityEngine.Object;
// GameLayout 布局实例单元测试
// GameLayout 是纯 POCO 类（无基类），可直接 new GameLayout() 测试全部 getter/setter
// init/setVisible/setVisibleForce/destroy/registerUIObject/unregisterUIObject 等方法需要完整运行时初始化
public static class GameLayoutTest
{
	public static void Run()
	{
		// === 默认值 ===
		testDefaultName();
		testDefaultType();
		testDefaultRenderOrder();
		testDefaultRenderOrderType();
		testDefaultLayer();
		testDefaultCheckBoxAnchor();
		testDefaultIgnoreTimeScale();
		testDefaultScriptControlHide();
		testDefaultBlurBack();
		testDefaultAnchorApplied();
		testDefaultScript();
		testDefaultRoot();
		testDefaultUIObjectList();
		testDefaultVisible();

		// === getter/setter 配对 ===
		testSetName();
		testSetNameNull();
		testSetNameEmpty();
		testSetNameMultiple();
		testSetType();
		testSetTypeNull();
		testSetOrderTypeAllValues();
		testSetCheckBoxAnchor();
		testSetCheckBoxAnchorMultiple();
		testSetIgnoreTimeScale();
		testSetIgnoreTimeScaleMultiple();
		testSetScriptControlHide();
		testSetBlurBack();
		testSetBlurBackMultiple();

		// === setRenderOrder ===
		testSetRenderOrderZero();
		testSetRenderOrderPositive();
		testSetRenderOrderHighValue();
		testSetRenderOrderMultipleTimes();

		// === setPrefab / setParent ===
		testSetPrefabNull();
		testSetParentNull();

		// === isVisible ===
		testIsVisibleNullRoot();

		// === canUIObjectUpdate ===
		testCanUIObjectUpdateNull();
		testCanUIObjectUpdateNotRegistered();

		// === getUIObjectList ===
		testGetUIObjectListEmpty();
		testGetUIObjectListIsReference();

		// === getDefaultLayer ===
		testDefaultLayerValue();
	

		testRegisterUnregisterUIObject();
		testRegisterMultipleAndLookup();
		testNotifyNeedUpdate();
		testUpdateNullRootSafe();
		testScriptSetLayoutRoot();
		testScriptUpdateAllDragViewEmpty();
		testScriptOnHideEmpty();
		testDestroyNullRoot();
	}
	// ================================================================
	//  默认值
	// ================================================================
	private static void testDefaultName()
	{
		var layout = new GameLayout();
		assertNull(layout.getName());
	}
	private static void testDefaultType()
	{
		var layout = new GameLayout();
		assertNull(layout.getType());
	}
	private static void testDefaultRenderOrder()
	{
		var layout = new GameLayout();
		assertEqual(0, layout.getRenderOrder());
	}
	private static void testDefaultRenderOrderType()
	{
		var layout = new GameLayout();
		assertEqual(LAYOUT_ORDER.ALWAYS_TOP, layout.getRenderOrderType());
	}
	private static void testDefaultLayer()
	{
		var layout = new GameLayout();
		assertEqual(0, layout.getDefaultLayer());
	}
	private static void testDefaultCheckBoxAnchor()
	{
		var layout = new GameLayout();
		assertTrue(layout.isCheckBoxAnchor());
	}
	private static void testDefaultIgnoreTimeScale()
	{
		var layout = new GameLayout();
		assertFalse(layout.isIgnoreTimeScale());
	}
	private static void testDefaultScriptControlHide()
	{
		var layout = new GameLayout();
		assertFalse(layout.isScriptControlHide());
	}
	private static void testDefaultBlurBack()
	{
		var layout = new GameLayout();
		assertFalse(layout.isBlurBack());
	}
	private static void testDefaultAnchorApplied()
	{
		var layout = new GameLayout();
		assertFalse(layout.isAnchorApplied());
	}
	private static void testDefaultScript()
	{
		var layout = new GameLayout();
		assertNull(layout.getScript());
	}
	private static void testDefaultRoot()
	{
		var layout = new GameLayout();
		assertNull(layout.getRoot());
	}
	private static void testDefaultUIObjectList()
	{
		var layout = new GameLayout();
		var list = layout.getUIObjectList();
		assertNotNull(list);
		assertEqual(0, list.Count);
	}
	private static void testDefaultVisible()
	{
		var layout = new GameLayout();
		assertFalse(layout.isVisible());
	}
	// ================================================================
	//  getter/setter 配对
	// ================================================================
	private static void testSetName()
	{
		var layout = new GameLayout();
		layout.setName("MainMenu");
		assertEqual("MainMenu", layout.getName());
	}
	private static void testSetNameNull()
	{
		var layout = new GameLayout();
		layout.setName(null);
		assertNull(layout.getName());
	}
	private static void testSetNameEmpty()
	{
		var layout = new GameLayout();
		layout.setName("");
		assertEqual("", layout.getName());
	}
	private static void testSetNameMultiple()
	{
		var layout = new GameLayout();
		layout.setName("A");
		layout.setName("B");
		layout.setName("C");
		assertEqual("C", layout.getName());
	}
	private static void testSetType()
	{
		var layout = new GameLayout();
		layout.setType(typeof(string));
		assertEqual(typeof(string), layout.getType());
	}
	private static void testSetTypeNull()
	{
		var layout = new GameLayout();
		layout.setType(null);
		assertNull(layout.getType());
		layout.setType(typeof(int));
		assertEqual(typeof(int), layout.getType());
	}
	private static void testSetOrderTypeAllValues()
	{
		var layout = new GameLayout();
		layout.setOrderType(LAYOUT_ORDER.FIXED);
		assertEqual(LAYOUT_ORDER.FIXED, layout.getRenderOrderType());
		layout.setOrderType(LAYOUT_ORDER.ALWAYS_TOP);
		assertEqual(LAYOUT_ORDER.ALWAYS_TOP, layout.getRenderOrderType());
		layout.setOrderType(LAYOUT_ORDER.AUTO);
		assertEqual(LAYOUT_ORDER.AUTO, layout.getRenderOrderType());
		layout.setOrderType(LAYOUT_ORDER.ALWAYS_TOP_AUTO);
		assertEqual(LAYOUT_ORDER.ALWAYS_TOP_AUTO, layout.getRenderOrderType());
	}
	private static void testSetCheckBoxAnchor()
	{
		var layout = new GameLayout();
		layout.setCheckBoxAnchor(false);
		assertFalse(layout.isCheckBoxAnchor());
		layout.setCheckBoxAnchor(true);
		assertTrue(layout.isCheckBoxAnchor());
	}
	private static void testSetCheckBoxAnchorMultiple()
	{
		var layout = new GameLayout();
		layout.setCheckBoxAnchor(false);
		layout.setCheckBoxAnchor(false);
		assertFalse(layout.isCheckBoxAnchor());
	}
	private static void testSetIgnoreTimeScale()
	{
		var layout = new GameLayout();
		layout.setIgnoreTimeScale(true);
		assertTrue(layout.isIgnoreTimeScale());
		layout.setIgnoreTimeScale(false);
		assertFalse(layout.isIgnoreTimeScale());
	}
	private static void testSetIgnoreTimeScaleMultiple()
	{
		var layout = new GameLayout();
		layout.setIgnoreTimeScale(true);
		layout.setIgnoreTimeScale(true);
		assertTrue(layout.isIgnoreTimeScale());
	}
	private static void testSetScriptControlHide()
	{
		var layout = new GameLayout();
		layout.setScriptControlHide(true);
		assertTrue(layout.isScriptControlHide());
		layout.setScriptControlHide(false);
		assertFalse(layout.isScriptControlHide());
	}
	private static void testSetBlurBack()
	{
		var layout = new GameLayout();
		layout.setBlurBack(true);
		assertTrue(layout.isBlurBack());
		layout.setBlurBack(false);
		assertFalse(layout.isBlurBack());
	}
	private static void testSetBlurBackMultiple()
	{
		var layout = new GameLayout();
		layout.setBlurBack(true);
		layout.setBlurBack(true);
		assertTrue(layout.isBlurBack());
	}
	// ================================================================
	//  setRenderOrder
	// ================================================================
	private static void testSetRenderOrderZero()
	{
		var layout = new GameLayout();
		layout.setRenderOrder(0);
		assertEqual(0, layout.getRenderOrder());
	}
	private static void testSetRenderOrderPositive()
	{
		var layout = new GameLayout();
		layout.setRenderOrder(42);
		assertEqual(42, layout.getRenderOrder());
	}
	private static void testSetRenderOrderHighValue()
	{
		var layout = new GameLayout();
		layout.setRenderOrder(10000);
		assertEqual(10000, layout.getRenderOrder());
	}
	private static void testSetRenderOrderMultipleTimes()
	{
		var layout = new GameLayout();
		layout.setRenderOrder(10);
		layout.setRenderOrder(20);
		layout.setRenderOrder(30);
		assertEqual(30, layout.getRenderOrder());
	}
	// ================================================================
	//  setPrefab / setParent — 安全无操作
	// ================================================================
	private static void testSetPrefabNull()
	{
		var layout = new GameLayout();
		layout.setPrefab(null);
	}
	private static void testSetParentNull()
	{
		var layout = new GameLayout();
		layout.setParent(null);
	}
	// ================================================================
	//  isVisible — mRoot 为 null 时始终 false
	// ================================================================
	private static void testIsVisibleNullRoot()
	{
		var layout = new GameLayout();
		assertFalse(layout.isVisible());
	}
	// ================================================================
	//  canUIObjectUpdate
	// ================================================================
	private static void testCanUIObjectUpdateNull()
	{
		var layout = new GameLayout();
		assertFalse(layout.canUIObjectUpdate(null));
	}
	private static void testCanUIObjectUpdateNotRegistered()
	{
		var layout = new GameLayout();
		// 未注册的 uiObj 返回 false
		assertFalse(layout.canUIObjectUpdate(null));
	}
	// ================================================================
	//  getUIObjectList
	// ================================================================
	private static void testGetUIObjectListEmpty()
	{
		var layout = new GameLayout();
		assertEqual(0, layout.getUIObjectList().Count);
	}
	private static void testGetUIObjectListIsReference()
	{
		var layout = new GameLayout();
		var list1 = layout.getUIObjectList();
		var list2 = layout.getUIObjectList();
		// 多次调用返回同一个引用
		assertTrue(ReferenceEquals(list1, list2));
	}
	// ================================================================
	//  getDefaultLayer
	// ================================================================
	private static void testDefaultLayerValue()
	{
		var layout = new GameLayout();
		assertEqual(0, layout.getDefaultLayer());
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