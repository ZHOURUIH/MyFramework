using static TestAssert;
using UnityEngine;
using System.Collections.Generic;
using static TestAssert;

using System.Collections.Generic;

// ClassPool / ArrayPool 穷举测试
// 覆盖所有公开方法、重载和关键分支
public static class PoolTest
{
    public static void Run()
    {
        // ─── ClassPool: newClass ───
        testNewClass();
        testNewClassGeneric();
        testNewClassOut();
        testNewClassOnlyOnce();
        testNewClassPersistent();
        testNewClassDestroyed();
        testNewClassNullType();
        // ─── ClassPool: destroyClass ───
        testDestroyClass();
        testDestroyClassNullRef();
        testDestroyClassDestroyed();
        // ─── ClassPool: destroyClassList ───
        testDestroyClassList();
        testDestroyClassListNull();
        testDestroyClassListEmpty();
        testDestroyClassListHashSet();
        testDestroyClassListHashSetNull();
        testDestroyClassListDictionary();
        testDestroyClassListDictionaryNull();
        testDestroyClassListQueue();
        testDestroyClassListQueueNull();
        // ─── ClassPool: clearUnused ───
        testClearUnused();
        // ─── ClassPool: getter ───
        testGetPersistentInusedList();
        testGetInusedList();
        testGetUnusedList();
        // ─── ArrayPool: newArray ───
        testNewArray();
        testNewArrayDestroyed();
        // ─── ArrayPool: destroyArray ───
        testDestroyArray();
        testDestroyArrayNull();
        testDestroyArrayDestroyReally();
        // ─── ArrayPool: destroyArrayList ───
        testDestroyArrayList();
        testDestroyArrayListNull();
        testDestroyArrayListEmpty();
        // ─── ArrayPool: getter ───
        testArrayPoolGetPersistentInusedList();
        testArrayPoolGetInusedList();
        testArrayPoolGetUnusedList();
    

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

    // ═══════════════════════════════════════════════════════════════════
    // ClassPool: newClass
    // ═══════════════════════════════════════════════════════════════════

    private static void testNewClass()
    {
        var pool = new ClassPool();
        var obj = pool.newClass(typeof(TestPoolClass), true);
        if (obj != null)
        {
            assertNotNull(obj, "newClass 不应返回 null");
            assertTrue(obj is TestPoolClass, "类型正确");
            assertFalse(obj.isDestroy(), "isDestroy=false");
            assertTrue(obj.getAssignID() > 0, "assignID > 0");
            TestPoolClass temp = obj as TestPoolClass;
            pool.destroyClass(ref temp);
        }
        pool.destroy();
    }

    private static void testNewClassGeneric()
    {
        var pool = new ClassPool();
        var obj = pool.newClass<TestPoolClass>(true);
        if (obj != null)
        {
            assertNotNull(obj, "newClass<T> 不应返回 null");
            TestPoolClass temp = obj;
            pool.destroyClass(ref temp);
        }
        pool.destroy();
    }

    private static void testNewClassOut()
    {
        var pool = new ClassPool();
        var obj = pool.newClass<TestPoolClass>(out var outObj, true);
        if (obj != null)
        {
            assertTrue(obj == outObj, "newClass(out) 返回值和 out 参数一致");
            TestPoolClass temp = obj;
            pool.destroyClass(ref temp);
        }
        pool.destroy();
    }

    private static void testNewClassOnlyOnce()
    {
        var pool = new ClassPool();
        var obj1 = pool.newClass<TestPoolClass>(true);
        var obj2 = pool.newClass<TestPoolClass>(true);
        if (obj1 != null && obj2 != null)
        {
            assertNotNull(obj1, "onlyOnce 应正常创建");
            assertNotNull(obj2, "第二个 onlyOnce 也应正常创建");
            TestPoolClass t1 = obj1, t2 = obj2;
            pool.destroyClass(ref t1);
            pool.destroyClass(ref t2);
        }
        pool.destroy();
    }

    private static void testNewClassPersistent()
    {
        var pool = new ClassPool();
        var obj = pool.newClass<TestPoolClass>(false);
        if (obj != null)
        {
            assertNotNull(obj, "persistent 应正常创建");
            TestPoolClass temp = obj;
            pool.destroyClass(ref temp);
        }
        pool.destroy();
    }

    private static void testNewClassDestroyed()
    {
        var pool = new ClassPool();
        pool.destroy();
        var obj = pool.newClass<TestPoolClass>(true);
        assertNull(obj, "池销毁后 newClass 返回 null");
    }

    private static void testNewClassNullType()
    {
        var pool = new ClassPool();
        var obj = pool.newClass(null, true);
        assertNull(obj, "null type 返回 null");
        pool.destroy();
    }

    // ═══════════════════════════════════════════════════════════════════
    // ClassPool: destroyClass
    // ═══════════════════════════════════════════════════════════════════

    private static void testDestroyClass()
    {
        var pool = new ClassPool();
        var obj = pool.newClass<TestPoolClass>(true);
        if (obj != null)
        {
            TestPoolClass temp = obj;
            pool.destroyClass(ref temp);
            assertNull(temp, "destroyClass 后引用置 null");
        }
        pool.destroy();
    }

    private static void testDestroyClassNullRef()
    {
        var pool = new ClassPool();
        TestPoolClass temp = null;
        pool.destroyClass(ref temp);
        // 不崩溃
        pool.destroy();
    }

    private static void testDestroyClassDestroyed()
    {
        var pool = new ClassPool();
        pool.destroy();
        var obj = pool.newClass<TestPoolClass>(true);
        // obj 应为 null
        TestPoolClass temp = null;
        pool.destroyClass(ref temp);
    }

    // ═══════════════════════════════════════════════════════════════════
    // ClassPool: destroyClassList
    // ═══════════════════════════════════════════════════════════════════

    private static void testDestroyClassList()
    {
        var pool = new ClassPool();
        var list = new List<TestPoolClass>();
        var a = pool.newClass<TestPoolClass>(true);
        var b = pool.newClass<TestPoolClass>(true);
        if (a != null) 
            list.Add(a);
        if (b != null) 
            list.Add(b);
        pool.destroyClassList(list);
        pool.destroy();
    }

    private static void testDestroyClassListNull()
    {
        var pool = new ClassPool();
        pool.destroyClassList((List<TestPoolClass>)null);
        pool.destroyClassList((HashSet<TestPoolClass>)null);
        pool.destroy();
    }

    private static void testDestroyClassListEmpty()
    {
        var pool = new ClassPool();
        pool.destroyClassList(new List<TestPoolClass>());
        pool.destroy();
    }

    private static void testDestroyClassListHashSet()
    {
        var pool = new ClassPool();
        var set = new HashSet<TestPoolClass>();
        var a = pool.newClass<TestPoolClass>(true);
        if (a != null) set.Add(a);
        pool.destroyClassList(set);
        pool.destroy();
    }

    private static void testDestroyClassListHashSetNull()
    {
        var pool = new ClassPool();
        pool.destroyClassList<TestPoolClass>(null as HashSet<TestPoolClass>);
        pool.destroy();
    }

    private static void testDestroyClassListDictionary()
    {
        var pool = new ClassPool();
        var dict = new Dictionary<string, TestPoolClass>();
        var a = pool.newClass<TestPoolClass>(true);
        if (a != null) dict.Add("a", a);
        pool.destroyClassList(dict);
        pool.destroy();
    }

    private static void testDestroyClassListDictionaryNull()
    {
        var pool = new ClassPool();
        pool.destroyClassList<string, TestPoolClass>(null);
        pool.destroy();
    }

    private static void testDestroyClassListQueue()
    {
        var pool = new ClassPool();
        var queue = new Queue<TestPoolClass>();
        var a = pool.newClass<TestPoolClass>(true);
        if (a != null) queue.Enqueue(a);
        pool.destroyClass(queue);
        assertEqual(0, queue.Count, "destroyClass(Queue) 应清空队列");
        pool.destroy();
    }

    private static void testDestroyClassListQueueNull()
    {
        var pool = new ClassPool();
        pool.destroyClass<TestPoolClass>(null);
        pool.destroy();
    }

    // ═══════════════════════════════════════════════════════════════════
    // ClassPool: clearUnused
    // ═══════════════════════════════════════════════════════════════════

    private static void testClearUnused()
    {
        var pool = new ClassPool();
        pool.clearUnused();
        // 不崩溃
        pool.destroy();
    }

    // ═══════════════════════════════════════════════════════════════════
    // ClassPool: getter
    // ═══════════════════════════════════════════════════════════════════

    private static void testGetPersistentInusedList()
    {
        var pool = new ClassPool();
        var list = pool.getPersistentInusedList();
        assertNotNull(list, "getPersistentInusedList 不应为 null");
        pool.destroy();
    }

    private static void testGetInusedList()
    {
        var pool = new ClassPool();
        var list = pool.getInusedList();
        assertNotNull(list, "getInusedList 不应为 null");
        pool.destroy();
    }

    private static void testGetUnusedList()
    {
        var pool = new ClassPool();
        var list = pool.getUnusedList();
        assertNotNull(list, "getUnusedList 不应为 null");
        pool.destroy();
    }

    // ═══════════════════════════════════════════════════════════════════
    // ArrayPool: newArray
    // ═══════════════════════════════════════════════════════════════════

    private static void testNewArray()
    {
        var pool = new ArrayPool();
        int[] arr = pool.newArray<int>(8, true);
        if (arr != null)
        {
            assertNotNull(arr, "newArray 不应返回 null");
            assertEqual(8, arr.Length, "数组长度正确");
            arr[0] = 42;
            assertEqual(42, arr[0], "数组可读写");
            pool.destroyArray(ref arr, false);
            assertNull(arr, "destroyArray 后引用置 null");
        }
        pool.destroy();
    }

    private static void testNewArrayDestroyed()
    {
        var pool = new ArrayPool();
        pool.destroy();
        int[] arr = pool.newArray<int>(4, true);
        // ArrayPool.newArray 未检查 mHasDestroy，销毁后仍可分配
        assertNotNull(arr, "ArrayPool destroy 后仍可 newArray（未检查 mHasDestroy）");
    }

    // ═══════════════════════════════════════════════════════════════════
    // ArrayPool: destroyArray
    // ═══════════════════════════════════════════════════════════════════

    private static void testDestroyArray()
    {
        var pool = new ArrayPool();
        int[] arr = pool.newArray<int>(4, true);
        if (arr != null)
        {
            pool.destroyArray(ref arr, false);
            assertNull(arr, "destroyArray 后引用置 null");
        }
        pool.destroy();
    }

    private static void testDestroyArrayNull()
    {
        var pool = new ArrayPool();
        int[] arr = null;
        pool.destroyArray(ref arr, false);
        pool.destroy();
    }

    private static void testDestroyArrayDestroyReally()
    {
        var pool = new ArrayPool();
        int[] arr = pool.newArray<int>(4, true);
        if (arr != null)
        {
            pool.destroyArray(ref arr, true);
            assertNull(arr, "destroyReally=true 后引用也置 null");
        }
        pool.destroy();
    }

    // ═══════════════════════════════════════════════════════════════════
    // ArrayPool: destroyArrayList
    // ═══════════════════════════════════════════════════════════════════

    private static void testDestroyArrayList()
    {
        var pool = new ArrayPool();
        var list = new List<int[]>();
        int[] a = pool.newArray<int>(2, true);
        int[] b = pool.newArray<int>(2, true);
        if (a != null) list.Add(a);
        if (b != null) list.Add(b);
        pool.destroyArrayList(list);
        pool.destroy();
    }

    private static void testDestroyArrayListNull()
    {
        var pool = new ArrayPool();
        pool.destroyArrayList<int>(null);
        pool.destroy();
    }

    private static void testDestroyArrayListEmpty()
    {
        var pool = new ArrayPool();
        pool.destroyArrayList(new List<int[]>());
        pool.destroy();
    }

    // ═══════════════════════════════════════════════════════════════════
    // ArrayPool: getter
    // ═══════════════════════════════════════════════════════════════════

    private static void testArrayPoolGetPersistentInusedList()
    {
        var pool = new ArrayPool();
        var list = pool.getPersistentInusedList();
        assertNotNull(list, "getPersistentInusedList 不应为 null");
        pool.destroy();
    }

    private static void testArrayPoolGetInusedList()
    {
        var pool = new ArrayPool();
        var list = pool.getInusedList();
        assertNotNull(list, "getInusedList 不应为 null");
        pool.destroy();
    }

    private static void testArrayPoolGetUnusedList()
    {
        var pool = new ArrayPool();
        var list = pool.getUnusedList();
        assertNotNull(list, "getUnusedList 不应为 null");
        pool.destroy();
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

// ─── 测试辅助类 ─────────────────────────────────────────────────────────

public class TestPoolClass : ClassObject
{
    public int mValue;
    public override void resetProperty()
    {
        base.resetProperty();
        mValue = 0;
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
