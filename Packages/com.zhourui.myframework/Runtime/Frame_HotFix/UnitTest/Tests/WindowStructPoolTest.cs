using UnityEngine;
using static TestAssert;

// WindowStructPool / WindowStructPoolMap / WindowStructPoolUnOrder 对象池深度测试
// 覆盖三种窗口结构对象池的完整生命周期:
//   assignTemplate + init 前置条件(mTemplate 为空时 init 报错不测)
//   newItem 新建/复用路径(未使用列表优先), assignID 递增唯一性, reset/setActive 调用链
//   unuseItem / unuseAll / unuseRange / unuseIndex 回收, 回收后 getInUseCount 递减
//   newItemList 批量创建 + 回调 + 复用, checkCapacity 预创建
//   WindowStructPoolMap: hasKey/getItem/newItem(key)/unuseItem(key)
//   WindowStructPoolUnOrder: 无序列表的 newItem/unuseAll
//   moveItem 跨池移动
//
// 环境: TestLayoutScriptDeep + setLayout(裸 GameLayout) + setRoot(myUGUICanvas, 预加 Canvas)
// 清理: pool.destroy() 销毁池内所有窗口, 模板 UI 手动 destroyObject + DestroyImmediate(rootGo)
public static class WindowStructPoolTest
{
	public static void Run()
	{
		testStructPoolInitAndNewItem();
		testStructPoolReuseItem();
		testStructPoolAssignIDIncrease();
		testStructPoolUnuseItem();
		testStructPoolUnuseAll();
		testStructPoolUnuseRange();
		testStructPoolCheckCapacity();
		testStructPoolNewItemList();
		testStructPoolNewItemListReuse();
		testStructPoolMoveItem();
		testMapPoolBasic();
		testMapPoolUnuseItem();
		testUnOrderPoolBasic();
		testUnOrderPoolCheckCapacity();
	}

	// ═════════════════════════════════════════════════════════════════
	// 辅助: 创建环境(script + root + 模板节点)
	// 注意: 池模板类型必须满足 WindowObjectT.assignWindow 的类型检查,
	//       T=TestRecyclableWindow 的内部 T 是 myUGUIObject, 所以模板用普通 myUGUIObject
	// ═════════════════════════════════════════════════════════════════
	private static TestLayoutScriptDeep createScript(out GameObject rootGo, out myUGUIObject templateRoot)
	{
		rootGo = new GameObject("TestStructPoolRoot");
		rootGo.AddComponent<RectTransform>();
		rootGo.AddComponent<Canvas>();
		myUGUICanvas root = new myUGUICanvas();
		root.setObject(rootGo);
		root.init();
		TestLayoutScriptDeep script = new TestLayoutScriptDeep();
		script.setLayout(new GameLayout());
		script.setRoot(root);
		templateRoot = script.createUGUIObject<myUGUIObject>(null, "TemplateRoot", true);
		return script;
	}

	// 创建已初始化、带模板的结构池
	private static WindowStructPool<TestRecyclableWindow> createStructPool(TestLayoutScriptDeep script, myUGUIObject templateRoot)
	{
		WindowStructPool<TestRecyclableWindow> pool = new WindowStructPool<TestRecyclableWindow>(script);
		pool.assignTemplate(templateRoot);
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
	// 初始化 + newItem 创建: init 后 mInited=true, newItem 创建新窗口并激活
	// ═════════════════════════════════════════════════════════════════
	private static void testStructPoolInitAndNewItem()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo, out myUGUIObject templateRoot);
		WindowStructPool<TestRecyclableWindow> pool = null;
		try
		{
			pool = createStructPool(script, templateRoot);
			TestRecyclableWindow item = pool.newItem();
			assertNotNull(item, "newItem 返回非 null");
			assertEqual(1, pool.getInUseCount(), "newItem 后 inUseCount=1");
			assertTrue(pool.isUsed(item), "newItem 后 item 在 used 列表");
			assertTrue(item.isActive(), "newItem 后 item 激活");
			assertTrue(item.getAssignID() > 0, "newItem 分配正数 assignID");
			// 创建第二个: assignID 递增
			TestRecyclableWindow item2 = pool.newItem();
			assertEqual(2, pool.getInUseCount(), "两次 newItem 后 inUseCount=2");
			assertTrue(item2.getAssignID() > item.getAssignID(), "assignID 单调递增");
		}
		finally
		{
			pool?.destroy();
			destroyUI(ref templateRoot);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// 复用: unuseItem 后再 newItem 复用同一实例(不新建)
	private static void testStructPoolReuseItem()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo, out myUGUIObject templateRoot);
		WindowStructPool<TestRecyclableWindow> pool = null;
		try
		{
			pool = createStructPool(script, templateRoot);
			TestRecyclableWindow item = pool.newItem();
			TestRecyclableWindow item2 = pool.newItem();
			pool.unuseItem(item);
			assertEqual(1, pool.getInUseCount(), "回收 1 个后 inUseCount=1");
			TestRecyclableWindow reused = pool.newItem();
			assertTrue(ReferenceEquals(item, reused), "unuse 后 newItem 复用同一实例");
			// 未复用的 item2 仍在使用中
			assertTrue(pool.isUsed(item2), "未回收的 item2 仍在使用");
			assertEqual(2, pool.getInUseCount(), "复用后 inUseCount=2");
		}
		finally
		{
			pool?.destroy();
			destroyUI(ref templateRoot);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// assignID 递增: 复用不重置 seed, 新创建继续递增
	private static void testStructPoolAssignIDIncrease()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo, out myUGUIObject templateRoot);
		WindowStructPool<TestRecyclableWindow> pool = null;
		try
		{
			pool = createStructPool(script, templateRoot);
			TestRecyclableWindow item = pool.newItem();
			long firstID = item.getAssignID();
			pool.unuseItem(item);
			TestRecyclableWindow reused = pool.newItem();
			assertTrue(reused.getAssignID() > firstID, "复用后 assignID 仍递增(seed 只增不减)");
			TestRecyclableWindow item2 = pool.newItem();
			assertTrue(item2.getAssignID() > reused.getAssignID(), "连续 newItem assignID 持续递增");
		}
		finally
		{
			pool?.destroy();
			destroyUI(ref templateRoot);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// unuseItem: 回收后移出 used 列表, assignID 重置为 -1
	private static void testStructPoolUnuseItem()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo, out myUGUIObject templateRoot);
		WindowStructPool<TestRecyclableWindow> pool = null;
		try
		{
			pool = createStructPool(script, templateRoot);
			TestRecyclableWindow item = pool.newItem();
			TestRecyclableWindow item2 = pool.newItem();
			bool result = pool.unuseItem(item);
			assertTrue(result, "unuseItem 返回 true");
			assertFalse(pool.isUsed(item), "回收后 item 不在 used 列表");
			assertTrue(pool.isUsed(item2), "未回收的 item2 仍在 used 列表");
			assertEqual(1, pool.getInUseCount(), "回收后 inUseCount=1");
			// ref 重载: 回收并置空外部引用
			TestRecyclableWindow item3 = pool.newItem();
			TestRecyclableWindow refItem = item3;
			bool refResult = pool.unuseItem(ref refItem);
			assertTrue(refResult, "unuseItem(ref) 返回 true");
			assertNull(refItem, "unuseItem(ref) 后外部引用置空");
			assertEqual(1, pool.getInUseCount(), "再次回收后 inUseCount=1");
			// 回收不属于池的对象: 类型一致但不在池中, 返回 false(showError=false 不触发日志)
			TestRecyclableWindow foreign = new TestRecyclableWindow(script);
			foreign.assignWindow(templateRoot);
			foreign.setAssignID(100);
			bool foreignResult = pool.unuseItem(foreign, false);
			assertFalse(foreignResult, "回收不属于池的对象返回 false");
			foreign.destroy();
		}
		finally
		{
			pool?.destroy();
			destroyUI(ref templateRoot);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// unuseAll: 全部回收
	private static void testStructPoolUnuseAll()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo, out myUGUIObject templateRoot);
		WindowStructPool<TestRecyclableWindow> pool = null;
		try
		{
			pool = createStructPool(script, templateRoot);
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
			destroyUI(ref templateRoot);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// unuseRange / unuseIndex: 按下标回收
	private static void testStructPoolUnuseRange()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo, out myUGUIObject templateRoot);
		WindowStructPool<TestRecyclableWindow> pool = null;
		try
		{
			pool = createStructPool(script, templateRoot);
			pool.newItem(4);
			assertEqual(4, pool.getInUseCount(), "批量创建 4 个");
			pool.unuseIndex(0);
			assertEqual(3, pool.getInUseCount(), "unuseIndex(0) 回收 1 个");
			pool.unuseRange(0, 2);
			assertEqual(1, pool.getInUseCount(), "unuseRange(0,2) 再回收 2 个");
			pool.unuseRange(0);
			assertEqual(0, pool.getInUseCount(), "unuseRange(0) 默认回收到底");
			// 越界 count 自动截断
			pool.newItem(2);
			pool.unuseRange(1, 100);
			assertEqual(1, pool.getInUseCount(), "越界 count 截断为剩余数量");
		}
		finally
		{
			pool?.destroy();
			destroyUI(ref templateRoot);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// checkCapacity: 确保容量至少为 capacity(只增不减)
	private static void testStructPoolCheckCapacity()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo, out myUGUIObject templateRoot);
		WindowStructPool<TestRecyclableWindow> pool = null;
		try
		{
			pool = createStructPool(script, templateRoot);
			pool.checkCapacity(3);
			assertEqual(3, pool.getInUseCount(), "checkCapacity(3) 后 inUseCount=3");
			pool.checkCapacity(2);
			assertEqual(3, pool.getInUseCount(), "checkCapacity(2) 小于当前数量不回收");
			pool.checkCapacity(5);
			assertEqual(5, pool.getInUseCount(), "checkCapacity(5) 继续创建到 5");
		}
		finally
		{
			pool?.destroy();
			destroyUI(ref templateRoot);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// newItemList: 批量创建 + 回调顺序
	private static void testStructPoolNewItemList()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo, out myUGUIObject templateRoot);
		WindowStructPool<TestRecyclableWindow> pool = null;
		try
		{
			pool = createStructPool(script, templateRoot);
			var dataList = new System.Collections.Generic.List<int> { 10, 20, 30 };
			int callbackCount = 0;
			pool.newItemList(dataList, (item, data) =>
			{
				assertEqual(10 + callbackCount * 10, data, "回调数据顺序传递");
				callbackCount++;
			});
			assertEqual(3, callbackCount, "回调调用 3 次");
			assertEqual(3, pool.getInUseCount(), "newItemList 后 inUseCount=3");
			// 再次 newItemList 会先 unuseAll 再新建(复用)
			pool.newItemList(dataList, (item, data) => { });
			assertEqual(3, pool.getInUseCount(), "重复 newItemList 后 inUseCount=3");
		}
		finally
		{
			pool?.destroy();
			destroyUI(ref templateRoot);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// newItemList 复用: 第二次创建复用第一批对象(不新建)
	// 注意: newItem(count) 从 unused 列表尾部 popBack, 复用顺序与回收顺序相反,
	//       不能断言"首个对象相同", 只能断言集合级复用(全部来自第一批)
	private static void testStructPoolNewItemListReuse()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo, out myUGUIObject templateRoot);
		WindowStructPool<TestRecyclableWindow> pool = null;
		try
		{
			pool = createStructPool(script, templateRoot);
			var dataList = new System.Collections.Generic.List<int> { 1, 2 };
			var firstRound = new System.Collections.Generic.List<TestRecyclableWindow>();
			pool.newItemList(dataList, (item, data) => firstRound.Add(item));
			assertEqual(2, firstRound.Count, "第一批创建 2 个");
			var secondRound = new System.Collections.Generic.List<TestRecyclableWindow>();
			pool.newItemList(dataList, (item, data) => secondRound.Add(item));
			assertEqual(2, secondRound.Count, "第二批回调 2 次");
			// 集合级复用: 第二批每个对象都是第一批的实例(无新建)
			foreach (TestRecyclableWindow item in secondRound)
			{
				assertTrue(firstRound.Contains(item), "第二次 newItemList 全部复用第一批对象");
			}
			assertEqual(2, pool.getInUseCount(), "复用后 inUseCount=2");
		}
		finally
		{
			pool?.destroy();
			destroyUI(ref templateRoot);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// moveItem: 将对象从源池移动到目标池
	private static void testStructPoolMoveItem()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo, out myUGUIObject templateRoot);
		WindowStructPool<TestRecyclableWindow> sourcePool = null;
		WindowStructPool<TestRecyclableWindow> targetPool = null;
		try
		{
			sourcePool = createStructPool(script, templateRoot);
			targetPool = createStructPool(script, templateRoot);
			TestRecyclableWindow item = sourcePool.newItem();
			sourcePool.unuseItem(item);
			// 从未使用状态移动到目标池(未使用): 源池移除, 目标池加入 unused 列表
			targetPool.moveItem(sourcePool, item, false);
			assertEqual(0, sourcePool.getInUseCount(), "移动后源池 inUseCount=0");
			assertEqual(0, targetPool.getInUseCount(), "移动为未使用状态目标池 inUseCount=0");
			// 从目标池取出: 复用移动来的对象
			TestRecyclableWindow reused = targetPool.newItem();
			assertTrue(ReferenceEquals(item, reused), "目标池 newItem 复用移动来的对象");
			assertEqual(1, targetPool.getInUseCount(), "取出后目标池 inUseCount=1");
			// 跨池移动使用中对象: sourcePool 新建(使用中) → 移动到 targetPool(使用中)
			TestRecyclableWindow item2 = sourcePool.newItem();
			targetPool.moveItem(sourcePool, item2, true);
			assertEqual(0, sourcePool.getInUseCount(), "移动使用中对象后源池 inUseCount=0");
			assertEqual(2, targetPool.getInUseCount(), "移动使用中对象后目标池 inUseCount=2");
			assertTrue(targetPool.isUsed(item2), "item2 在目标池使用中");
		}
		finally
		{
			sourcePool?.destroy();
			targetPool?.destroy();
			destroyUI(ref templateRoot);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// WindowStructPoolMap: Key 索引池
	// ═════════════════════════════════════════════════════════════════
	private static WindowStructPoolMap<string, TestRecyclableWindow> createMapPool(TestLayoutScriptDeep script, myUGUIObject templateRoot)
	{
		WindowStructPoolMap<string, TestRecyclableWindow> pool = new WindowStructPoolMap<string, TestRecyclableWindow>(script);
		pool.assignTemplate(templateRoot);
		pool.init();
		return pool;
	}

	private static void testMapPoolBasic()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo, out myUGUIObject templateRoot);
		WindowStructPoolMap<string, TestRecyclableWindow> pool = null;
		try
		{
			pool = createMapPool(script, templateRoot);
			TestRecyclableWindow item = pool.newItem("key1");
			assertNotNull(item, "newItem(key) 返回非 null");
			assertTrue(pool.hasKey("key1"), "newItem 后 hasKey=true");
			assertEqual(item, pool.getItem("key1"), "getItem(key) 返回同一对象");
			assertEqual(1, pool.getInUseCount(), "newItem 后 inUseCount=1");
			// 同一 key 重复添加触发异常不测(原生 Dictionary.Add), 用不同 key
			TestRecyclableWindow item2 = pool.newItem("key2");
			assertTrue(pool.hasKey("key2"), "第二个 key 注册成功");
			assertEqual(2, pool.getInUseCount(), "两个 key 后 inUseCount=2");
			assertTrue(item2.getAssignID() > item.getAssignID(), "Map 池 assignID 也递增");
		}
		finally
		{
			pool?.destroy();
			destroyUI(ref templateRoot);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	private static void testMapPoolUnuseItem()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo, out myUGUIObject templateRoot);
		WindowStructPoolMap<string, TestRecyclableWindow> pool = null;
		try
		{
			pool = createMapPool(script, templateRoot);
			pool.newItem("key1");
			pool.newItem("key2");
			bool result = pool.unuseItem("key1");
			assertTrue(result, "unuseItem(key) 返回 true");
			assertFalse(pool.hasKey("key1"), "回收后 hasKey=false");
			assertEqual(1, pool.getInUseCount(), "回收后 inUseCount=1");
			// 不存在的 key: 返回 false
			bool missing = pool.unuseItem("missing", false);
			assertFalse(missing, "回收不存在的 key 返回 false");
			// unuseAll
			pool.unuseAll();
			assertEqual(0, pool.getInUseCount(), "unuseAll 后 inUseCount=0");
			assertFalse(pool.hasKey("key2"), "unuseAll 后 hasKey=false");
		}
		finally
		{
			pool?.destroy();
			destroyUI(ref templateRoot);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// ═════════════════════════════════════════════════════════════════
	// WindowStructPoolUnOrder: 无序池
	// ═════════════════════════════════════════════════════════════════
	private static WindowStructPoolUnOrder<TestRecyclableWindow> createUnOrderPool(TestLayoutScriptDeep script, myUGUIObject templateRoot)
	{
		WindowStructPoolUnOrder<TestRecyclableWindow> pool = new WindowStructPoolUnOrder<TestRecyclableWindow>(script);
		pool.assignTemplate(templateRoot);
		pool.init();
		return pool;
	}

	private static void testUnOrderPoolBasic()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo, out myUGUIObject templateRoot);
		WindowStructPoolUnOrder<TestRecyclableWindow> pool = null;
		try
		{
			pool = createUnOrderPool(script, templateRoot);
			TestRecyclableWindow item = pool.newItem();
			TestRecyclableWindow item2 = pool.newItem();
			assertEqual(2, pool.getInUseCount(), "两个 newItem 后 inUseCount=2");
			assertTrue(pool.getUsedList().Contains(item), "item 在 used 集合中");
			bool result = pool.unuseItem(item);
			assertTrue(result, "unuseItem 返回 true");
			assertEqual(1, pool.getInUseCount(), "回收后 inUseCount=1");
			// 复用: 从队列取出
			TestRecyclableWindow reused = pool.newItem();
			assertTrue(ReferenceEquals(item, reused), "UnOrder 池复用回收对象");
			// ref 重载
			TestRecyclableWindow refItem = item2;
			pool.unuseItem(ref refItem);
			assertNull(refItem, "unuseItem(ref) 置空外部引用");
			assertEqual(1, pool.getInUseCount(), "再回收后 inUseCount=1");
		}
		finally
		{
			pool?.destroy();
			destroyUI(ref templateRoot);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	private static void testUnOrderPoolCheckCapacity()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo, out myUGUIObject templateRoot);
		WindowStructPoolUnOrder<TestRecyclableWindow> pool = null;
		try
		{
			pool = createUnOrderPool(script, templateRoot);
			pool.checkCapacity(3);
			assertEqual(3, pool.getInUseCount(), "checkCapacity(3) 后 inUseCount=3");
			pool.newItem(2);
			assertEqual(5, pool.getInUseCount(), "newItem(2) 批量创建后 inUseCount=5");
			pool.unuseAll();
			assertEqual(0, pool.getInUseCount(), "unuseAll 后 inUseCount=0");
		}
		finally
		{
			pool?.destroy();
			destroyUI(ref templateRoot);
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}
}
