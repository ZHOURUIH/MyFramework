using UnityEngine;
using static TestAssert;

// WindowPool / WindowPoolBase 深度测试
// 覆盖窗口池完整生命周期:
//   assignTemplate + init(mParent 记录 + 模板隐藏)
//   newItem 新建/复用(未使用列表优先), setName 重命名, setAsLastSibling 排序
//   unuseItem / tryUnuseItem / unuseAll / unuseRange / unuseIndex 回收
//   ensureCapacity 预创建, getInUseCount / getWindowList
//   setDestroyCallback: 回收时调用自定义回调替代 setActive(false)
//   WindowPoolBase: isRootPool / setAutoRefreshDepth / setMoveToLast
//
// 环境: TestLayoutScriptDeep + setLayout(裸 GameLayout) + setRoot(myUGUICanvas, 预加 Canvas)
// 清理: pool 内窗口由池管理(走 mInusedList), 模板 UI 手动 destroyObject + DestroyImmediate(rootGo)
public static class WindowPoolTest
{
	public static void Run()
	{
		testPoolInitAndNewItem();
		testPoolReuseItem();
		testPoolUnuseItem();
		testPoolTryUnuseItem();
		testPoolUnuseAll();
		testPoolUnuseRange();
		testPoolEnsureCapacity();
		testPoolNewItemIf();
		testPoolDestroyCallback();
		testPoolBaseFlag();
	}

	// ═════════════════════════════════════════════════════════════════
	// 辅助: 创建环境(script + root + 模板节点)
	// ═════════════════════════════════════════════════════════════════
	private static TestLayoutScriptDeep createScript(out GameObject rootGo, out myUGUIObject template)
	{
		rootGo = new GameObject("TestWindowPoolRoot");
		rootGo.AddComponent<RectTransform>();
		rootGo.AddComponent<Canvas>();
		myUGUICanvas root = new myUGUICanvas();
		root.setObject(rootGo);
		root.init();
		TestLayoutScriptDeep script = new TestLayoutScriptDeep();
		script.setLayout(new GameLayout());
		script.setRoot(root);
		template = script.createUGUIObject<myUGUIObject>(null, "WindowTemplate", true);
		// 显式设置模板名, 保证 newItem 默认名断言稳定(mName 默认为 null)
		template.setName("WindowTemplate");
		return script;
	}

	// 创建已初始化、带模板的窗口池
	private static WindowPool<myUGUIObject> createPool(TestLayoutScriptDeep script, myUGUIObject template)
	{
		WindowPool<myUGUIObject> pool = new WindowPool<myUGUIObject>(script);
		pool.assignTemplate(template);
		pool.init();
		return pool;
	}

	private static void destroyUI(ref myUGUIObject ui)
	{
		if (ui != null)
		{
			LayoutScript.destroyObject(ref ui, true);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// 初始化 + newItem 创建: init 记录 mParent, newItem 克隆模板并激活
	// ═════════════════════════════════════════════════════════════════
	private static void testPoolInitAndNewItem()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo, out myUGUIObject template);
		WindowPool<myUGUIObject> pool = null;
		try
		{
			pool = createPool(script, template);
			myUGUIObject window = pool.newItem();
			assertNotNull(window, "newItem 返回非 null");
			assertEqual(1, pool.getInUseCount(), "newItem 后 inUseCount=1");
			assertTrue(window.isActive(), "newItem 后窗口激活");
			assertEqual("WindowTemplate", window.getName(), "newItem 默认使用模板名");
			assertEqual(1, pool.getWindowList().Count, "getWindowList 含 1 个窗口");
			// 自定义名字
			myUGUIObject named = pool.newItem("NamedWindow");
			assertEqual("NamedWindow", named.getName(), "newItem(name) 重命名窗口");
			assertEqual(2, pool.getInUseCount(), "两个窗口后 inUseCount=2");
		}
		finally
		{
			pool?.destroy();
			destroyUI(ref template);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// 复用: unuseItem 后再 newItem 复用同一实例
	private static void testPoolReuseItem()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo, out myUGUIObject template);
		WindowPool<myUGUIObject> pool = null;
		try
		{
			pool = createPool(script, template);
			myUGUIObject window = pool.newItem();
			myUGUIObject window2 = pool.newItem();
			pool.unuseItem(window);
			assertEqual(1, pool.getInUseCount(), "回收 1 个后 inUseCount=1");
			myUGUIObject reused = pool.newItem();
			assertTrue(ReferenceEquals(window, reused), "unuse 后 newItem 复用同一实例");
			assertEqual(2, pool.getInUseCount(), "复用后 inUseCount=2");
			// 未回收的 window2 仍在使用
			assertTrue(pool.getWindowList().Contains(window2), "window2 仍在使用列表");
		}
		finally
		{
			pool?.destroy();
			destroyUI(ref template);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// unuseItem: 回收后移出使用列表并隐藏
	private static void testPoolUnuseItem()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo, out myUGUIObject template);
		WindowPool<myUGUIObject> pool = null;
		try
		{
			pool = createPool(script, template);
			myUGUIObject window = pool.newItem();
			bool result = pool.unuseItem(window);
			assertTrue(result, "unuseItem 返回 true");
			assertEqual(0, pool.getInUseCount(), "回收后 inUseCount=0");
			assertFalse(window.isActive(), "回收后窗口隐藏");
			// 回收 null: 返回 false(不触发日志)
			bool nullResult = pool.unuseItem(null);
			assertFalse(nullResult, "unuseItem(null) 返回 false");
			// 注意: unuseItem(不属于池的窗口) 源码固定 logError, 不测该分支
			//       由 tryUnuseItem 先检查再回收, 见 testPoolTryUnuseItem
		}
		finally
		{
			pool?.destroy();
			destroyUI(ref template);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// tryUnuseItem: 先检查是否属于池, 不属于则直接返回 false 不回收
	private static void testPoolTryUnuseItem()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo, out myUGUIObject template);
		WindowPool<myUGUIObject> pool = null;
		try
		{
			pool = createPool(script, template);
			myUGUIObject window = pool.newItem();
			// 属于池: 正常回收
			bool inPool = pool.tryUnuseItem(window);
			assertTrue(inPool, "tryUnuseItem(池内窗口) 返回 true");
			assertEqual(0, pool.getInUseCount(), "回收后 inUseCount=0");
			// 不属于池: 返回 false
			myUGUIObject foreign = script.createUGUIObject<myUGUIObject>(null, "ForeignWindow", true);
			bool outPool = pool.tryUnuseItem(foreign);
			assertFalse(outPool, "tryUnuseItem(池外窗口) 返回 false");
			assertTrue(foreign.isActive(), "池外窗口不受影响仍激活");
			destroyUI(ref foreign);
		}
		finally
		{
			pool?.destroy();
			destroyUI(ref template);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// unuseAll: 全部回收
	private static void testPoolUnuseAll()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo, out myUGUIObject template);
		WindowPool<myUGUIObject> pool = null;
		try
		{
			pool = createPool(script, template);
			pool.newItem();
			pool.newItem();
			pool.newItem();
			assertEqual(3, pool.getInUseCount(), "创建 3 个后 inUseCount=3");
			pool.unuseAll();
			assertEqual(0, pool.getInUseCount(), "unuseAll 后 inUseCount=0");
			// 空池再 unuseAll 不报错
			pool.unuseAll();
			assertEqual(0, pool.getInUseCount(), "空池 unuseAll 保持 0");
		}
		finally
		{
			pool?.destroy();
			destroyUI(ref template);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// unuseRange / unuseIndex: 按下标回收
	private static void testPoolUnuseRange()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo, out myUGUIObject template);
		WindowPool<myUGUIObject> pool = null;
		try
		{
			pool = createPool(script, template);
			pool.newItem(4);
			assertEqual(4, pool.getInUseCount(), "批量创建 4 个");
			pool.unuseIndex(0);
			assertEqual(3, pool.getInUseCount(), "unuseIndex(0) 回收 1 个");
			pool.unuseRange(0, 2);
			assertEqual(1, pool.getInUseCount(), "unuseRange(0,2) 再回收 2 个");
			pool.unuseRange(0);
			assertEqual(0, pool.getInUseCount(), "unuseRange(0) 默认回收到底");
		}
		finally
		{
			pool?.destroy();
			destroyUI(ref template);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// ensureCapacity: 预创建到目标容量(只增不减)
	private static void testPoolEnsureCapacity()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo, out myUGUIObject template);
		WindowPool<myUGUIObject> pool = null;
		try
		{
			pool = createPool(script, template);
			pool.ensureCapacity(3);
			assertEqual(3, pool.getInUseCount(), "ensureCapacity(3) 后 inUseCount=3");
			pool.ensureCapacity(2);
			assertEqual(3, pool.getInUseCount(), "ensureCapacity(2) 小于当前数量不回收");
			pool.ensureCapacity(5);
			assertEqual(5, pool.getInUseCount(), "ensureCapacity(5) 继续创建到 5");
		}
		finally
		{
			pool?.destroy();
			destroyUI(ref template);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// newItemIf: 条件为真才创建
	private static void testPoolNewItemIf()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo, out myUGUIObject template);
		WindowPool<myUGUIObject> pool = null;
		try
		{
			pool = createPool(script, template);
			myUGUIObject created = pool.newItemIf(true);
			assertNotNull(created, "newItemIf(true) 创建窗口");
			assertEqual(1, pool.getInUseCount(), "创建后 inUseCount=1");
			myUGUIObject skipped = pool.newItemIf(false);
			assertNull(skipped, "newItemIf(false) 返回 null");
			assertEqual(1, pool.getInUseCount(), "条件为假不创建");
		}
		finally
		{
			pool?.destroy();
			destroyUI(ref template);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// setDestroyCallback: 回收时调用自定义回调替代 setActive(false)
	private static void testPoolDestroyCallback()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo, out myUGUIObject template);
		WindowPool<myUGUIObject> pool = null;
		try
		{
			pool = createPool(script, template);
			int callbackCount = 0;
			pool.setDestroyCallback((myUGUIObject window) => callbackCount++);
			myUGUIObject window = pool.newItem();
			pool.unuseItem(window);
			assertEqual(1, callbackCount, "unuseItem 时调用 destroyCallback");
			// 有回调时窗口不被 setActive(false), 保持激活(由外部决定销毁方式)
			assertTrue(window.isActive(), "有 destroyCallback 时回收不自动隐藏窗口");
			// unuseAll 也触发回调
			pool.newItem();
			pool.newItem();
			pool.unuseAll();
			assertEqual(3, callbackCount, "unuseAll 逐个调用 destroyCallback");
		}
		finally
		{
			pool?.destroy();
			destroyUI(ref template);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// WindowPoolBase: isRootPool / setAutoRefreshDepth / setMoveToLast 标志
	private static void testPoolBaseFlag()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo, out myUGUIObject template);
		WindowPool<myUGUIObject> rootPool = null;
		WindowPool<myUGUIObject> childPool = null;
		try
		{
			// owner 为 script → 根池
			rootPool = new WindowPool<myUGUIObject>(script);
			assertTrue(rootPool.isRootPool(), "owner 为 script 时是根池");
			// owner 为窗口 → 非根池
			myUGUIObject ownerRoot = script.createUGUIObject<myUGUIObject>(null, "OwnerRoot", true);
			TestWindowObjectUGUI owner = new TestWindowObjectUGUI(script);
			owner.assignWindow(ownerRoot);
			childPool = new WindowPool<myUGUIObject>(owner);
			assertFalse(childPool.isRootPool(), "owner 为窗口时非根池");
			assertEqual(1, owner.getWindowPoolCount(), "owner 窗口持有 1 个窗口池");
			// init 需要先 assignTemplate(mParent = mTemplate.getParent() 否则 NRE)
			childPool.assignTemplate(template);
			childPool.init();
			childPool.setAutoRefreshDepth(true);
			childPool.setMoveToLast(false);
			destroyUI(ref ownerRoot);
		}
		finally
		{
			rootPool?.destroy();
			childPool?.destroy();
			destroyUI(ref template);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}
}
