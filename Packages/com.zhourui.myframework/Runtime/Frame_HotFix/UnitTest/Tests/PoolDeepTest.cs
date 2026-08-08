using UnityEngine;
using System.Collections.Generic;
using static TestAssert;

// 对象池系统深度测试
// ============================================================================
// 与 PoolTest(单接口调用/空值守卫) 的区别:
//   本测试聚焦对象池的**复杂调用链与状态契约**——
//   ① ClassPool 的"复用即重置"状态卫生(复用对象必须被 resetProperty 清干净)、
//      onlyOnce(仅此帧) vs 持久使用 两套已使用列表的归属、destroyClass 的
//      ref 置空/onCreate 每次调用、destroyClass(Queue) 清空队列;
//   ② GameObjectPool 的"newObject→destroyObject→复用同一实例"生命周期、
//      moveToHide 移远/SetActive(false) 两种归还方式、clearUnused 销毁未用缓存。
// 覆盖单接口测不到的跨调用链行为与复用状态过渡。
// ============================================================================
public static class PoolDeepTest
{
	public static void Run()
	{
		// ─── ClassPool 复用状态卫生 ───
		testReuseResetPropertyHygiene();
		// ─── ClassPool onlyOnce vs 持久 已使用列表归属 ───
		testOnlyOnceGoesToFrameInused();
		testPersistentGoesToPersistentInused();
		// ─── destroyClass 语义 ───
		testDestroyClassNullsRefAndRecycles();
		testDestroyClassPendingDestroySet();
		// ─── onCreate 每次调用 ───
		testOnCreateCalledOnFreshAndReuse();
		// ─── destroyClass(Queue) 清空队列 ───
		testDestroyClassQueueClearsQueue();
		// ─── 复用 FIFO 顺序 ───
		testReuseFifoOrder();
		// ─── 混合 onlyOnce 与持久不互相污染 ───
		testOnlyOnceDoesNotLeakToPersistent();
		// ─── GameObjectPool 生命周期 ───
		testGameObjectPoolNewObjectActive();
		testGameObjectPoolDestroyObjectSetActiveFalse();
		testGameObjectPoolDestroyObjectMoveToHide();
		testGameObjectPoolReuseSameInstance();
		testGameObjectPoolClearUnused();
		testGameObjectPoolDestroyObjectNullSafe();
		testGameObjectPoolNewObjectAfterDestroyResetsName();
	}

	// 复用即重置: 对象在销毁归还后必须被 resetProperty 清干净, 再次分配时状态是全新的
	static void testReuseResetPropertyHygiene()
	{
		ClassPool pool = new ClassPool();
		DeepPoolClass obj = pool.newClass<DeepPoolClass>(true);
		assertNotNull(obj, "首次分配不应为 null");
		obj.mCounter = 42;
		obj.mName = "dirty";
		pool.destroyClass(ref obj);
		assertNull(obj, "destroyClass 应把外部引用置 null");

		// 再次分配应复用同一实例, 且实例状态已被 resetProperty 清空
		DeepPoolClass reused = pool.newClass<DeepPoolClass>(true);
		assertNotNull(reused, "复用分配不应为 null");
		assertEqual(0, reused.mCounter, "复用对象应被 resetProperty 清零 mCounter");
		assertNull(reused.mName, "复用对象应被 resetProperty 清空 mName");
		pool.destroyClass(ref reused);
		pool.destroy();
	}

	// onlyOnce=true 应进入"仅此帧"已使用列表 mInusedList, 而非持久列表
	static void testOnlyOnceGoesToFrameInused()
	{
		ClassPool pool = new ClassPool();
		DeepPoolClass obj = pool.newClass<DeepPoolClass>(true);
		assertNotNull(obj, "onlyOnce 分配不应为 null");

		HashSet<ClassObject> frameInused = pool.getInusedList().get(typeof(DeepPoolClass), null);
		HashSet<ClassObject> persistentInused = pool.getPersistentInusedList().get(typeof(DeepPoolClass), null);
		assertNotNull(frameInused, "仅此帧对象应登记在 mInusedList");
		assertTrue(frameInused.Contains(obj), "仅此帧对象应在 mInusedList 中");
		assertTrue(persistentInused == null || !persistentInused.Contains(obj), "仅此帧对象不应出现在持久列表");

		pool.destroyClass(ref obj);
		pool.destroy();
	}

	// onlyOnce=false 应进入持久已使用列表 mPersistentInuseList
	static void testPersistentGoesToPersistentInused()
	{
		ClassPool pool = new ClassPool();
		DeepPoolClass obj = pool.newClass<DeepPoolClass>(false);
		assertNotNull(obj, "持久分配不应为 null");

		HashSet<ClassObject> persistentInused = pool.getPersistentInusedList().get(typeof(DeepPoolClass), null);
		assertNotNull(persistentInused, "持久对象应登记在 mPersistentInuseList");
		assertTrue(persistentInused.Contains(obj), "持久对象应在 mPersistentInuseList 中");

		pool.destroyClass(ref obj);
		pool.destroy();
	}

	// destroyClass 语义: 外部引用被置 null, 对象归还到未使用队列
	static void testDestroyClassNullsRefAndRecycles()
	{
		ClassPool pool = new ClassPool();
		DeepPoolClass obj = pool.newClass<DeepPoolClass>(true);
		assertNotNull(obj, "分配不应为 null");
		pool.destroyClass(ref obj);
		assertNull(obj, "destroyClass 后外部引用应为 null");

		Queue<ClassObject> unused = pool.getUnusedList().get(typeof(DeepPoolClass), null);
		assertNotNull(unused, "未使用队列应已创建");
		assertEqual(1, unused.Count, "销毁对象应回收到未使用队列");
		pool.destroy();
	}

	// destroyClass 会设置 pendingDestroy, 随后归还前被 resetProperty
	static void testDestroyClassPendingDestroySet()
	{
		ClassPool pool = new ClassPool();
		DeepPoolClass obj = pool.newClass<DeepPoolClass>(true);
		assertNotNull(obj, "分配不应为 null");
		assertFalse(obj.isPendingDestroy(), "新分配对象不应处于 pendingDestroy");

		pool.destroyClass(ref obj);
		// 复用分配时 pendingDestroy 应被清零
		DeepPoolClass reused = pool.newClass<DeepPoolClass>(true);
		assertNotNull(reused, "复用分配不应为 null");
		assertFalse(reused.isPendingDestroy(), "复用对象不应残留 pendingDestroy");

		pool.destroyClass(ref reused);
		pool.destroy();
	}

	// onCreate 在"新建"与"复用"两条路径上都会被调用
	static void testOnCreateCalledOnFreshAndReuse()
	{
		ClassPool pool = new ClassPool();
		// 第一次分配: 新建路径, onCreate 计数应为 1
		DeepPoolClass obj = pool.newClass<DeepPoolClass>(true);
		assertEqual(1, obj.mOnCreateCount, "新建对象 onCreate 应被调用一次");
		pool.destroyClass(ref obj);

		// 复用路径: 再次分配触发 onCreate
		DeepPoolClass reused = pool.newClass<DeepPoolClass>(true);
		assertEqual(1, reused.mOnCreateCount, "复用对象 onCreate 也应被调用一次(resetProperty 后计数归零再+1)");
		pool.destroyClass(ref reused);
		pool.destroy();
	}

	// destroyClass(Queue) 会逐个归还并最终清空队列
	static void testDestroyClassQueueClearsQueue()
	{
		ClassPool pool = new ClassPool();
		Queue<DeepPoolClass> queue = new Queue<DeepPoolClass>();
		DeepPoolClass a = pool.newClass<DeepPoolClass>(true);
		DeepPoolClass b = pool.newClass<DeepPoolClass>(true);
		queue.Enqueue(a);
		queue.Enqueue(b);

		pool.destroyClass(queue);
		assertEqual(0, queue.Count, "destroyClass(Queue) 应清空队列");

		Queue<ClassObject> unused = pool.getUnusedList().get(typeof(DeepPoolClass), null);
		assertNotNull(unused, "未使用队列应已创建");
		assertEqual(2, unused.Count, "两个对象都应回收到未使用队列");
		pool.destroy();
	}

	// 未使用队列是 FIFO: 先销毁的先被复用
	// 注意: 不能用 mName/mCounter 作为实例标记, 因为 destroyClass 会 resetProperty 把它们清掉;
	// 必须保留强引用, 用 ReferenceEquals 核对返回的是哪个具体实例。
	static void testReuseFifoOrder()
	{
		ClassPool pool = new ClassPool();
		DeepPoolClass a = pool.newClass<DeepPoolClass>(true);
		DeepPoolClass b = pool.newClass<DeepPoolClass>(true);
		DeepPoolClass keepA = a;
		DeepPoolClass keepB = b;
		pool.destroyClass(ref a);
		pool.destroyClass(ref b);

		// 第一次复用应拿到先销毁的 keepA, 第二次拿到 keepB(FIFO)
		DeepPoolClass r1 = pool.newClass<DeepPoolClass>(true);
		assertTrue(ReferenceEquals(keepA, r1), "FIFO 先复用先销毁的对象");
		DeepPoolClass r2 = pool.newClass<DeepPoolClass>(true);
		assertTrue(ReferenceEquals(keepB, r2), "FIFO 再复用后销毁的对象");
		pool.destroyClass(ref r1);
		pool.destroyClass(ref r2);
		pool.destroy();
	}

	// onlyOnce 与持久对象各自登记, 互不串入对方列表
	static void testOnlyOnceDoesNotLeakToPersistent()
	{
		ClassPool pool = new ClassPool();
		DeepPoolClass once = pool.newClass<DeepPoolClass>(true);
		DeepPoolClass pers = pool.newClass<DeepPoolClass>(false);

		HashSet<ClassObject> frameInused = pool.getInusedList().get(typeof(DeepPoolClass), null);
		HashSet<ClassObject> persistentInused = pool.getPersistentInusedList().get(typeof(DeepPoolClass), null);
		assertTrue(frameInused != null && frameInused.Contains(once), "onlyOnce 对象应在 mInusedList");
		assertFalse(frameInused != null && frameInused.Contains(pers), "持久对象不应混入 mInusedList");
		assertTrue(persistentInused != null && persistentInused.Contains(pers), "持久对象应在 mPersistentInuseList");
		assertFalse(persistentInused != null && persistentInused.Contains(once), "onlyOnce 对象不应混入持久列表");

		pool.destroyClass(ref once);
		pool.destroyClass(ref pers);
		pool.destroy();
	}

	// ──────────── GameObjectPool ────────────

	// newObject 返回活跃对象并登记到 inuse 列表
	static void testGameObjectPoolNewObjectActive()
	{
		GameObjectPool pool = new GameObjectPool();
		GameObject go = pool.newObject();
		assertNotNull(go, "newObject 不应为 null");
		assertTrue(go.activeSelf, "newObject 返回的对象应处于活跃状态");
		assertTrue(pool.getInuseList().Contains(go), "新对象应登记在 inuse 列表");

		pool.destroyObject(go, false);
		pool.clearUnused();
		pool.destroy();
	}

	// destroyObject(moveToHide=false) 用 SetActive(false) 归还
	static void testGameObjectPoolDestroyObjectSetActiveFalse()
	{
		GameObjectPool pool = new GameObjectPool();
		GameObject go = pool.newObject();
		pool.destroyObject(go, false);
		assertFalse(go.activeSelf, "moveToHide=false 应 SetActive(false)");
		assertTrue(pool.getUnusedList().Contains(go), "归还对象应在 unused 队列");
		assertFalse(pool.getInuseList().Contains(go), "归还对象应移出 inuse 列表");

		pool.clearUnused();
		pool.destroy();
	}

	// destroyObject(moveToHide=true) 把对象移动到 FAR_POSITION 而不是 SetActive(false)
	static void testGameObjectPoolDestroyObjectMoveToHide()
	{
		GameObjectPool pool = new GameObjectPool();
		GameObject go = pool.newObject();
		pool.destroyObject(go, true);
		assertTrue(go.activeSelf, "moveToHide=true 应保持活跃(移远而非关闭)");
		assertEqual(FrameDefine.FAR_POSITION, go.transform.localPosition, "moveToHide=true 应移动到 FAR_POSITION");

		pool.clearUnused();
		pool.destroy();
	}

	// newObject → destroyObject → newObject 复用同一实例
	static void testGameObjectPoolReuseSameInstance()
	{
		GameObjectPool pool = new GameObjectPool();
		GameObject go = pool.newObject();
		pool.destroyObject(go, false);

		GameObject reused = pool.newObject();
		assertTrue(ReferenceEquals(go, reused), "再次 newObject 应复用刚归还的实例");
		assertTrue(reused.activeSelf, "复用对象应重新激活");

		pool.destroyObject(reused, false);
		pool.clearUnused();
		pool.destroy();
	}

	// clearUnused 销毁所有未使用缓存, 队列清空
	static void testGameObjectPoolClearUnused()
	{
		GameObjectPool pool = new GameObjectPool();
		GameObject a = pool.newObject();
		GameObject b = pool.newObject();
		pool.destroyObject(a, false);
		pool.destroyObject(b, false);
		assertEqual(2, pool.getUnusedList().Count, "归还两个对象");

		pool.clearUnused();
		assertEqual(0, pool.getUnusedList().Count, "clearUnused 应清空 unused 队列");
		pool.destroy();
	}

	// destroyObject(null) 空安全
	static void testGameObjectPoolDestroyObjectNullSafe()
	{
		GameObjectPool pool = new GameObjectPool();
		pool.destroyObject(null, false);
		pool.destroyObject(null, true);
		assertEqual(0, pool.getUnusedList().Count, "销毁 null 不应产生副作用");
		pool.destroy();
	}

	// 复用时会重新设置名称
	static void testGameObjectPoolNewObjectAfterDestroyResetsName()
	{
		GameObjectPool pool = new GameObjectPool();
		GameObject go = pool.newObject("originalName");
		pool.destroyObject(go, false);

		GameObject reused = pool.newObject("renamed");
		assertTrue(ReferenceEquals(go, reused), "应复用同一实例");
		assertEqual("renamed", reused.name, "newObject 应重新设置对象名");

		pool.destroyObject(reused, false);
		pool.clearUnused();
		pool.destroy();
	}
}

// 供池测试使用的辅助类: 带可观察字段以验证 resetProperty 复用卫生
public class DeepPoolClass : ClassObject
{
	public int mCounter;
	public int mOnCreateCount;
	public string mName;

	public override void onCreate()
	{
		base.onCreate();
		++mOnCreateCount;
	}

	public override void resetProperty()
	{
		base.resetProperty();
		mCounter = 0;
		mOnCreateCount = 0;
		mName = null;
	}
}
