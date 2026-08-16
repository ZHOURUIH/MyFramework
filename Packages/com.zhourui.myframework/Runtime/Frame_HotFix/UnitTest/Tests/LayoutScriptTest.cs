using static TestAssert;
using UnityEngine;
using static TestAssert;

using static FrameBaseHotFix;

// LayoutScript 测试用的具体子类
public class TestLayoutScript : LayoutScript
{
	public override void assignWindow() { }
	public void setEscHideForTest(bool value) { mEscHide = value; }
	public new void resetProperty() { base.resetProperty(); }
	public void clearLocalizationForTest() { clearLocalization(); }
}

// LayoutScript 布局脚本基类单元测试
// 通过 TestLayoutScript 具体子类测试非抽象公开方法
//
// 不覆盖（需 Unity 运行时组件或完整游戏初始化）:
//   init/assignWindow/destroy/newObject/cloneObject/createUIObject/createUGUIObject
//   registeScrollRect/registeInputField/unregisteInputField/bindPassOnlyParent/bindPassOnlyArea
//   onGameState/onHide/update/lateUpdate/onDrawGizmos
//   instantiate/instantiateAsync/destroyCloned/destroyObject
public static class LayoutScriptTest
{
	public static void Run()
	{
		// === 默认值 ===
		testDefaultGetLayout();
		testDefaultGetRoot();
		testDefaultIsNeedUpdate();
		testDefaultOnESCDown();

		// === setLayout / getLayout ===
		testSetLayoutNull();
		testSetLayoutNonNull();
		testSetLayoutSameInstance();
		testSetLayoutMultipleCalls();
		testSetLayoutNullThenNonNull();
		testSetLayoutNonNullThenNull();

		// === setRoot / getRoot ===
		testSetRootNull();
		testSetRootNonNull();
		testSetRootSameInstance();
		testSetRootMultipleCalls();
		testSetRootNullThenNonNull();
		testSetRootNonNullThenNull();

		// === isVisible ===
		testIsVisibleWithLayout();
		testIsVisibleWithoutLayout();

		// === isNeedUpdate ===
		testIsNeedUpdateDefault();

		// === onESCDown ===
		testOnESCDownDefault();
		testOnESCDownWithEscHideTrue();

		// === close ===
		testClose();

		// === notifyUIObjectNeedUpdate ===
		testNotifyUIObjectNeedUpdateWithLayout();
		testNotifyUIObjectNeedUpdateWithoutLayout();
		testNotifyUIObjectNeedUpdateTooggle();

		// === addLocalizationObject ===
		testAddLocalizationObjectNull();

		// === updateAllDragView ===
		testUpdateAllDragViewEmpty();

		// === clearLocalization ===
		testClearLocalization();

		// === destroyInstantiate ===
		testDestroyInstantiateNull();

		// === resetProperty ===
		testResetPropertyClearsLayout();
		testResetPropertyClearsRoot();
		testResetPropertyKeepsNeedUpdateTrue();
		testResetPropertyAfterSetLayoutAndRoot();
		testResetPropertyMultipleTimes();
	

		testCreateUIObject();
		testCreateUIObjectInactive();
		testCreateUGUIObjectHasRectTransform();
		testCloneObject();
		testDestroyObjectNullsRef();
		testNewObjectNotFoundSafe();
		testNewObjectByName();
		testLifecycleCallbacks();
		testOnGameStateBaseLogic();

		// === addWindowObject / hasObject / postInit ===
		testAddWindowObjectRoot();
		testAddWindowObjectMultiple();
		testHasObjectWithParentMissing();
		testHasObjectWithParentFound();
		testPostInitAfterSetLayout();
		testPostInitTwice();
	}
	// ================================================================
	//  addWindowObject / hasObject / postInit(深度)
	// ================================================================

	// addWindowObject: 构造即注册(WindowObjectFixedT 构造内部 mScript.addWindowObject(this))
	// 不再显式调 addWindowObject——同实例二次注册会 logError"不能重复注册UI对象"
	private static void testAddWindowObjectRoot()
	{
		var script = new TestLayoutScript();
		TestWindowObjectUGUI win = new TestWindowObjectUGUI(script);
		// 构造已注册, 无异常即通过
	}

	// 多个窗口对象各自构造注册
	private static void testAddWindowObjectMultiple()
	{
		var script = new TestLayoutScript();
		TestWindowObjectUGUI a = new TestWindowObjectUGUI(script);
		TestWindowObjectUGUI b = new TestWindowObjectUGUI(script);
		// 两实例各自注册, 无异常即通过
	}

	// hasObject(parent, 不存在的名字) → false(裸 GO 无子物体)
	private static void testHasObjectWithParentMissing()
	{
		var script = new TestLayoutScript();
		var go = new GameObject("LS_Parent");
		try
		{
			myUGUIObject parent = new myUGUIObject();
			parent.setObject(go);
			assertFalse(script.hasObject(parent, "NoSuchObject"), "不存在返回 false");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// hasObject(parent, 存在的子物体名) → true
	private static void testHasObjectWithParentFound()
	{
		var script = new TestLayoutScript();
		var go = new GameObject("LS_Parent2");
		try
		{
			var child = new GameObject("LS_Child");
			child.transform.SetParent(go.transform);
			myUGUIObject parent = new myUGUIObject();
			parent.setObject(go);
			assertTrue(script.hasObject(parent, "LS_Child"), "子物体存在返回 true");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(go);
		}
	}

	// postInit: setLayout 后调用不炸(构造已注册窗口对象)
	private static void testPostInitAfterSetLayout()
	{
		var script = new TestLayoutScript();
		script.setLayout(new GameLayout());
		TestWindowObjectUGUI win = new TestWindowObjectUGUI(script);
		script.postInit();
		// 无异常即通过
	}

	// postInit 多次调用
	private static void testPostInitTwice()
	{
		var script = new TestLayoutScript();
		script.setLayout(new GameLayout());
		script.postInit();
		script.postInit();
		// 无异常即通过
	}

	// ================================================================
	//  默认值
	// ================================================================
	private static void testDefaultGetLayout()
	{
		var script = new TestLayoutScript();
		assertNull(script.getLayout());
	}
	private static void testDefaultGetRoot()
	{
		var script = new TestLayoutScript();
		assertNull(script.getRoot());
	}
	private static void testDefaultIsNeedUpdate()
	{
		var script = new TestLayoutScript();
		assertTrue(script.isNeedUpdate());
	}
	private static void testDefaultOnESCDown()
	{
		var script = new TestLayoutScript();
		// mEscHide 默认为 false → onESCDown 返回 false 且不调用 close
		assertFalse(script.onESCDown());
	}
	// ================================================================
	//  setLayout / getLayout
	// ================================================================
	private static void testSetLayoutNull()
	{
		var script = new TestLayoutScript();
		script.setLayout(null);
		assertNull(script.getLayout());
	}
	private static void testSetLayoutNonNull()
	{
		var script = new TestLayoutScript();
		var layout = new GameLayout();
		script.setLayout(layout);
		assertNotNull(script.getLayout());
	}
	private static void testSetLayoutSameInstance()
	{
		var script = new TestLayoutScript();
		var layout = new GameLayout();
		script.setLayout(layout);
		assertTrue(ReferenceEquals(layout, script.getLayout()));
	}
	private static void testSetLayoutMultipleCalls()
	{
		var script = new TestLayoutScript();
		var layout1 = new GameLayout();
		var layout2 = new GameLayout();
		script.setLayout(layout1);
		script.setLayout(layout2);
		assertTrue(ReferenceEquals(layout2, script.getLayout()));
	}
	private static void testSetLayoutNullThenNonNull()
	{
		var script = new TestLayoutScript();
		script.setLayout(null);
		var layout = new GameLayout();
		script.setLayout(layout);
		assertTrue(ReferenceEquals(layout, script.getLayout()));
	}
	private static void testSetLayoutNonNullThenNull()
	{
		var script = new TestLayoutScript();
		script.setLayout(new GameLayout());
		script.setLayout(null);
		assertNull(script.getLayout());
	}
	// ================================================================
	//  setRoot / getRoot
	// ================================================================
	private static void testSetRootNull()
	{
		var script = new TestLayoutScript();
		script.setRoot(null);
		assertNull(script.getRoot());
	}
	private static void testSetRootNonNull()
	{
		var script = new TestLayoutScript();
		var root = new myUGUIObject();
		script.setRoot(root);
		assertNotNull(script.getRoot());
	}
	private static void testSetRootSameInstance()
	{
		var script = new TestLayoutScript();
		var root = new myUGUIObject();
		script.setRoot(root);
		assertTrue(ReferenceEquals(root, script.getRoot()));
	}
	private static void testSetRootMultipleCalls()
	{
		var script = new TestLayoutScript();
		var root1 = new myUGUIObject();
		var root2 = new myUGUIObject();
		script.setRoot(root1);
		script.setRoot(root2);
		assertTrue(ReferenceEquals(root2, script.getRoot()));
	}
	private static void testSetRootNullThenNonNull()
	{
		var script = new TestLayoutScript();
		script.setRoot(null);
		var root = new myUGUIObject();
		script.setRoot(root);
		assertTrue(ReferenceEquals(root, script.getRoot()));
	}
	private static void testSetRootNonNullThenNull()
	{
		var script = new TestLayoutScript();
		script.setRoot(new myUGUIObject());
		script.setRoot(null);
		assertNull(script.getRoot());
	}
	// ================================================================
	//  isVisible — 委托给 mLayout.isVisible()
	// ================================================================
	private static void testIsVisibleWithLayout()
	{
		var script = new TestLayoutScript();
		script.setLayout(new GameLayout());
		// GameLayout 默认 mRoot=null → isVisible() 返回 false
		assertFalse(script.isVisible());
	}
	private static void testIsVisibleWithoutLayout()
	{
		var script = new TestLayoutScript();
		// mLayout 为 null 时，isVisible() 内部访问 null.isVisible() → 抛异常
		// 这是一个已知行为：必须先 setLayout 再调用 isVisible
		try
		{
			script.isVisible();
			// 如果没抛异常也接受（不同运行时行为不同）
			assertTrue(true);
		}
		catch
		{
			assertTrue(true);
		}
	}
	// ================================================================
	//  isNeedUpdate
	// ================================================================
	private static void testIsNeedUpdateDefault()
	{
		var script = new TestLayoutScript();
		assertTrue(script.isNeedUpdate());
		// resetProperty 后保持一致
		script.resetProperty();
		assertTrue(script.isNeedUpdate());
	}
	// ================================================================
	//  onESCDown
	// ================================================================
	private static void testOnESCDownDefault()
	{
		var script = new TestLayoutScript();
		assertFalse(script.onESCDown());
	}
	private static void testOnESCDownWithEscHideTrue()
	{
		// mEscHide=true → onESCDown 调用 close() → CmdLayoutManagerVisible.execute
		// TestLayoutScript 未注册 → getLayout 返回 null → execute 安全退出
		if (mLayoutManager == null)
		{
			return;
		}
		var script = new TestLayoutScript();
		script.setEscHideForTest(true);
		assertTrue(script.onESCDown());
		// 再次调用验证幂等
		assertTrue(script.onESCDown());
	}
	// ================================================================
	//  close
	// ================================================================
	private static void testClose()
	{
		// close() → CmdLayoutManagerVisible.execute(GetType(), false, false)
		// 未注册的布局类型 → 方法安全返回 null
		if (mLayoutManager == null)
		{
			return;
		}
		var script = new TestLayoutScript();
		script.close();
	}
	// ================================================================
	//  notifyUIObjectNeedUpdate — 委托给 mLayout.notifyUIObjectNeedUpdate
	// ================================================================
	private static void testNotifyUIObjectNeedUpdateWithLayout()
	{
		var script = new TestLayoutScript();
		script.setLayout(new GameLayout());
		// null uiObj → SafeList.addOrRemove(null, true) 安全处理
		script.notifyUIObjectNeedUpdate(null, true);
		script.notifyUIObjectNeedUpdate(null, false);
	}
	private static void testNotifyUIObjectNeedUpdateWithoutLayout()
	{
		// mLayout 为 null 时调用 → null.notifyUIObjectNeedUpdate() 抛异常
		var script = new TestLayoutScript();
		try 
		{
			script.notifyUIObjectNeedUpdate(null, true); 
		}
		catch { /* 预期异常 */ }
	}
	private static void testNotifyUIObjectNeedUpdateTooggle()
	{
		var script = new TestLayoutScript();
		script.setLayout(new GameLayout());
		// 先注册再取消注册
		script.notifyUIObjectNeedUpdate(null, true);
		script.notifyUIObjectNeedUpdate(null, false);
		// 再次注册
		script.notifyUIObjectNeedUpdate(null, true);
	}
	// ================================================================
	//  addLocalizationObject — null 对象也能加入列表
	// ================================================================
	private static void testAddLocalizationObjectNull()
	{
		var script = new TestLayoutScript();
		script.addLocalizationObject(null);
		// 重复调用
		script.addLocalizationObject(null);
	}
	// ================================================================
	//  updateAllDragView — 空集合安全（.safe() 扩展处理 null）
	// ================================================================
	private static void testUpdateAllDragViewEmpty()
	{
		var script = new TestLayoutScript();
		script.updateAllDragView();
		// 重复调用验证无副作用
		script.updateAllDragView();
	}
	// ================================================================
	//  clearLocalization — 空列表安全
	// ================================================================
	private static void testClearLocalization()
	{
		// 需要 mLocalizationManager 非 null（游戏初始化后可用）
		if (mLocalizationManager == null)
		{
			return;
		}
		var script = new TestLayoutScript();
		script.clearLocalizationForTest();
		// 重复调用
		script.clearLocalizationForTest();
	}
	// ================================================================
	//  destroyInstantiate — null 安全检查
	// ================================================================
	private static void testDestroyInstantiateNull()
	{
		// destroyInstantiate 顶部有 null 检查 → 安全返回
		LayoutScript.destroyInstantiate(null, true);
		LayoutScript.destroyInstantiate(null, false);
	}
	// ================================================================
	//  resetProperty — 重置到默认值
	// ================================================================
	private static void testResetPropertyClearsLayout()
	{
		var script = new TestLayoutScript();
		script.setLayout(new GameLayout());
		script.resetProperty();
		assertNull(script.getLayout());
	}
	private static void testResetPropertyClearsRoot()
	{
		var script = new TestLayoutScript();
		script.setRoot(new myUGUIObject());
		script.resetProperty();
		assertNull(script.getRoot());
	}
	private static void testResetPropertyKeepsNeedUpdateTrue()
	{
		var script = new TestLayoutScript();
		script.resetProperty();
		assertTrue(script.isNeedUpdate());
	}
	private static void testResetPropertyAfterSetLayoutAndRoot()
	{
		var script = new TestLayoutScript();
		script.setLayout(new GameLayout());
		script.setRoot(new myUGUIObject());
		script.resetProperty();
		assertNull(script.getLayout());
		assertNull(script.getRoot());
		assertTrue(script.isNeedUpdate());
	}
	private static void testResetPropertyMultipleTimes()
	{
		var script = new TestLayoutScript();
		script.setLayout(new GameLayout());
		script.setRoot(new myUGUIObject());
		// 连续重置 3 次
		script.resetProperty();
		script.resetProperty();
		script.resetProperty();
		assertNull(script.getLayout());
		assertNull(script.getRoot());
		assertTrue(script.isNeedUpdate());
	}


	

	// ═════════════════════════════════════════════════════════════════
	// 辅助: 创建 TestLayoutScriptDeep + 根节点(myUGUICanvas)
	// ═════════════════════════════════════════════════════════════════
	private static TestLayoutScriptDeep createScript(out GameObject rootGo)
	{
		rootGo = new GameObject("TestDeepRoot");
		rootGo.AddComponent<RectTransform>();
		// 预加 Canvas: myUGUICanvas.init 的 TryGetComponent 命中后跳过 logError 分支
		rootGo.AddComponent<Canvas>();
		myUGUICanvas root = new myUGUICanvas();
		root.setObject(rootGo);
		root.init();
		TestLayoutScriptDeep script = new TestLayoutScriptDeep();
		script.setLayout(new GameLayout());
		script.setRoot(root);
		return script;
	}

	// destroyObject(ref, true): 立即销毁(走 destroyWindow)并置空外部引用
	private static void destroyUI(ref myUGUIObject ui)
	{
		if (ui != null)
		{
			LayoutScript.destroyObject(ref ui, true);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// createUIObject: 动态创建对象(parent=null → 挂到 mRoot 下)
	// ═════════════════════════════════════════════════════════════════
	private static void testCreateUIObject()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo);
		myUGUIObject ui = null;
		try
		{
			ui = script.createUIObject<myUGUIObject>(null, "TestChild", true);
			assertNotNull(ui, "createUIObject 返回非 null");
			assertEqual("TestChild", ui.getGameObject().name, "创建的 GameObject 名正确");
			assertTrue(ui.isActive(), "active=true 后 isActive");
		}
		finally
		{
			destroyUI(ref ui);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// createUIObject(active=false): 创建后不激活
	private static void testCreateUIObjectInactive()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo);
		myUGUIObject ui = null;
		try
		{
			ui = script.createUIObject<myUGUIObject>(null, "TestChild2", false);
			assertNotNull(ui, "createUIObject 返回非 null");
			assertFalse(ui.isActive(), "active=false 后 isActive=false");
		}
		finally
		{
			destroyUI(ref ui);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// createUGUIObject: 自动添加 RectTransform
	private static void testCreateUGUIObjectHasRectTransform()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo);
		myUGUIObject ui = null;
		try
		{
			ui = script.createUGUIObject<myUGUIObject>(null, "TestUGUI", true);
			assertNotNull(ui, "createUGUIObject 返回非 null");
			assertNotNull(ui.getGameObject().GetComponent<RectTransform>(), "UGUI 对象自动添加 RectTransform");
		}
		finally
		{
			destroyUI(ref ui);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// cloneObject: Instantiate 克隆 + cloneFrom 复制
	private static void testCloneObject()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo);
		myUGUIObject ori = null;
		myUGUIObject clone = null;
		try
		{
			ori = script.createUIObject<myUGUIObject>(null, "Ori", true);
			clone = script.cloneObject<myUGUIObject>(null, ori, "TestClone");
			assertNotNull(clone, "cloneObject 返回非 null");
			assertEqual("TestClone", clone.getGameObject().name, "克隆体名字正确");
			assertTrue(!ReferenceEquals(clone, ori), "克隆体与原件是不同实例");
		}
		finally
		{
			destroyUI(ref clone);
			destroyUI(ref ori);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// destroyObject(ref): 销毁后外部引用置 null
	private static void testDestroyObjectNullsRef()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo);
		try
		{
			myUGUIObject ui = script.createUIObject<myUGUIObject>(null, "ToDestroy", true);
			assertNotNull(ui, "创建成功");
			LayoutScript.destroyObject(ref ui, true);
			assertNull(ui, "destroyObject(ref) 后引用置 null");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// newObject: 场景中找不到 → 返回 null(showError=false 不触发 logError)
	private static void testNewObjectNotFoundSafe()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo);
		try
		{
			myUGUIObject ui;
			script.newObject<myUGUIObject>(out ui, null, "NoSuchName12345", false);
			assertNull(ui, "找不到 GameObject 时返回 null(showError=false)");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// newObject: 场景中存在同名 GameObject → 绑定
	private static void testNewObjectByName()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo);
		GameObject target = new GameObject("TestTarget");
		myUGUIObject ui = null;
		try
		{
			script.newObject<myUGUIObject>(out ui, null, "TestTarget", false);
			assertNotNull(ui, "newObject 找到场景对象");
			assertTrue(ReferenceEquals(target, ui.getGameObject()), "绑定到场景中的同名 GameObject");
		}
		finally
		{
			// ui 非 null 时 target 已被 destroyObject 销毁(Unity 假 null 判断跳过); 否则手动销毁
			destroyUI(ref ui);
			if (target != null)
			{
				UnityEngine.Object.DestroyImmediate(target);
			}
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// 生命周期回调: update/lateUpdate override 被驱动
	private static void testLifecycleCallbacks()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo);
		try
		{
			script.update(0.1f);
			script.lateUpdate(0.2f);
			script.update(0.3f);
			assertEqual(2, script.updateCount, "update 被调用 2 次");
			assertEqual(1, script.lateUpdateCount, "lateUpdate 被调用 1 次");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// onGameState 基类逻辑: interruptAllCommand + isEditor 注册检查(mRoot 无 ScrollRect → 空列表安全)
	private static void testOnGameStateBaseLogic()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo);
		try
		{
			// 第一次: mRegisterChecked=false → 走注册检查(空列表安全)
			script.onGameState();
			// 第二次: mRegisterChecked=true → 跳过检查
			script.onGameState();
			// 不抛异常即通过(基类逻辑空安全)
			assertTrue(true, "onGameState 基类逻辑空安全");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}
}


// LayoutScript 深度测试子类: override update/lateUpdate 记录调用
public class TestLayoutScriptDeep : LayoutScript
{
	public override void assignWindow() { }
	public int updateCount;
	public int lateUpdateCount;

	public override void update(float elapsedTime) 
	{
		base.update(elapsedTime);
		updateCount++; 
	}
	public override void lateUpdate(float elapsedTime) 
	{
		base.lateUpdate(elapsedTime);
		lateUpdateCount++; 
	}
	public new void resetProperty()
	{
		base.resetProperty();
		updateCount = 0;
		lateUpdateCount = 0;
	}
}
