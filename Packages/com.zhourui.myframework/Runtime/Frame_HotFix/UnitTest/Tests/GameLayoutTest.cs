using static TestAssert;

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
}