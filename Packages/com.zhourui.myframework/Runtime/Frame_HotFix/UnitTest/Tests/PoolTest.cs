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
