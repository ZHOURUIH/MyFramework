using static TestAssert;

// LayoutInfo 布局加载参数信息单元测试
// addCallback/callAll 是纯回调列表逻辑, 不依赖 UI 组件, 可直接测
public static class LayoutInfoTest
{
	public static void Run()
	{
		testDefaultValues();
		testAddCallbackOne();
		testAddCallbackMultiple();
		testCallAllEmpty();
		testCallAllInvokesCallbacks();
		testResetProperty();
	}

	// ─── 默认值 ─────────────────────────────────────────────────────
	private static void testDefaultValues()
	{
		var info = new LayoutInfo();
		// LAYOUT_ORDER 枚举首个成员 ALWAYS_TOP=0, 故 new 后字段默认值为 ALWAYS_TOP(非 AUTO, AUTO 是 resetProperty 后的值)
		assertEqual(LAYOUT_ORDER.ALWAYS_TOP, info.mOrderType, "默认显示顺序为 ALWAYS_TOP(枚举0值)");
		assertTrue(info.mType == null, "默认 type 为 null");
		assertTrue(info.mName == null, "默认 name 为 null");
		assertFalse(info.mIsScene, "默认不是场景布局");
		assertEqual(0, info.mRenderOrder, "默认渲染顺序为 0");
	}

	// ─── addCallback 单个回调 ────────────────────────────────────────
	private static void testAddCallbackOne()
	{
		var info = new LayoutInfo();
		bool called = false;
		info.addCallback(layout => called = true);
		info.callAll(null);
		assertTrue(called, "addCallback 后 callAll 应触发回调");
	}

	// ─── addCallback 多个回调 ────────────────────────────────────────
	private static void testAddCallbackMultiple()
	{
		var info = new LayoutInfo();
		int count = 0;
		info.addCallback(layout => count++);
		info.addCallback(layout => count++);
		info.callAll(null);
		assertEqual(2, count, "多个回调按顺序全部执行");
	}

	// ─── callAll 空列表 ─────────────────────────────────────────────
	private static void testCallAllEmpty()
	{
		var info = new LayoutInfo();
		// 未 addCallback 时 callAll 直接 return, 不抛异常
		info.callAll(null);
		assertTrue(true, "空回调列表 callAll 不抛异常");
	}

	// ─── callAll 执行回调并传入 layout ─────────────────────────────
	private static void testCallAllInvokesCallbacks()
	{
		var info = new LayoutInfo();
		GameLayout received = null;
		var layout = new GameLayout();
		info.addCallback(l => received = l);
		info.callAll(layout);
		assertTrue(ReferenceEquals(layout, received), "callAll 把 layout 传给回调");
	}

	// ─── resetProperty 重置 ─────────────────────────────────────────
	private static void testResetProperty()
	{
		var info = new LayoutInfo();
		info.mOrderType = LAYOUT_ORDER.FIXED;
		info.mIsScene = true;
		info.mRenderOrder = 5;
		info.addCallback(layout => { });
		info.resetProperty();
		assertEqual(LAYOUT_ORDER.AUTO, info.mOrderType, "resetProperty 重置显示顺序为 AUTO");
		assertTrue(info.mType == null, "resetProperty 重置 type 为 null");
		assertTrue(info.mName == null, "resetProperty 重置 name 为 null");
		assertFalse(info.mIsScene, "resetProperty 重置 isScene 为 false");
		assertEqual(0, info.mRenderOrder, "resetProperty 重置渲染顺序为 0");
	}
}
