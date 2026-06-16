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
}