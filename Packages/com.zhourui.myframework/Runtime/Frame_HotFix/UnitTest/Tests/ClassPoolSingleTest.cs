using System.Collections.Generic;
using static TestAssert;

public static class ClassPoolSingleTest
{
    public static void Run()
    {
        testNewAndDestroy();
        testReuse();
        testClearUnused();
        testMultiType();
        testInusedTracking();
        testReuseResetsState();
        testDestroySetsRefNull();
        testAssignIDIncrements();
        testFIFOReuseOrder();
        testInterleavedCycle();
        testDestroyListBatch();
        testDestroyRemovesFromInused();
    }

    static void testNewAndDestroy()
    {
        ClassPoolSingle pool = new ClassPoolSingle();
        pool.setType(typeof(TestClass));

        ClassObject obj = pool.newClass();
        assertNotNull(obj, "newClass should return an object");
        assertTrue(obj is TestClass, "newClass should create the correct type");

        pool.destroyClass(ref obj);
        // After destroy, object should be returned to unused pool
        Queue<ClassObject> unused = pool.getUnusedList();
        assertEqual(1, unused.Count, "destroyClass should add to unused pool");
    }

    static void testReuse()
    {
        ClassPoolSingle pool = new ClassPoolSingle();
        pool.setType(typeof(TestClass));

        // Create and destroy to populate unused pool
        ClassObject obj1 = pool.newClass();
        ClassObject savedRef = obj1; // 在 destroyClass 前保存引用
        pool.destroyClass(ref obj1);

        // Second allocation should reuse
        ClassObject obj2 = pool.newClass();
        assertTrue(ReferenceEquals(savedRef, obj2), "newClass should reuse destroyed object");

        // Third allocation should create new since unused is now empty
        ClassObject obj3 = pool.newClass();
        assertFalse(ReferenceEquals(obj2, obj3), "Should create new when pool is empty");
    }

    static void testClearUnused()
    {
        ClassPoolSingle pool = new ClassPoolSingle();
        pool.setType(typeof(TestClass));

        ClassObject obj1 = pool.newClass();
        ClassObject obj2 = pool.newClass();
        pool.destroyClass(ref obj1);
        pool.destroyClass(ref obj2);

        assertEqual(2, pool.getUnusedList().Count, "Should have 2 unused");

        pool.clearUnused();
        assertEqual(0, pool.getUnusedList().Count, "clearUnused should remove all unused");
    }

    static void testMultiType()
    {
        ClassPoolSingle intPool = new ClassPoolSingle();
        intPool.setType(typeof(TestClass));
        ClassObject obj = intPool.newClass();
        assertTrue(obj is TestClass, "Type should be TestClass");
        intPool.destroyClass(ref obj);
    }

    static void testInusedTracking()
    {
        ClassPoolSingle pool = new ClassPoolSingle();
        pool.setType(typeof(TestClass));

        HashSet<ClassObject> inused = pool.getInusedList();
        int beforeCount = inused.Count;

        ClassObject obj = pool.newClass();
        // In editor/dev mode, inused list tracks allocated objects
        pool.destroyClass(ref obj);
    }

    // ── 深度组合场景 ──────────────────────────────────────────────
    // 复用对象经过 destroy+resetProperty: 状态被清空
    static void testReuseResetsState()
    {
        ClassPoolSingle pool = new ClassPoolSingle();
        pool.setType(typeof(TestClass));
        ClassObject obj = pool.newClass();
        ClassObject saved = obj; // destroyClass 会置空外部引用, 先保存
        ((TestClass)saved).mValue = 99;
        pool.destroyClass(ref obj);
        // 复用同一实例, mValue 应已被 resetProperty 清空
        ClassObject reused = pool.newClass();
        assertTrue(ReferenceEquals(reused, saved), "复用的是同一实例");
        assertEqual(0, ((TestClass)reused).mValue, "复用对象状态已重置 mValue=0");
        // 复用对象非 pendingDestroy
        assertFalse(reused.isPendingDestroy(), "复用对象非 pendingDestroy");
        assertFalse(reused.isDestroy(), "复用对象非 destroy 状态");
        pool.destroyClass(ref reused);
    }

    // destroyClass(ref) 将外部引用置空
    static void testDestroySetsRefNull()
    {
        ClassPoolSingle pool = new ClassPoolSingle();
        pool.setType(typeof(TestClass));
        ClassObject obj = pool.newClass();
        assertNotNull(obj, "创建对象非空");
        pool.destroyClass(ref obj);
        assertTrue(obj == null, "destroyClass 后外部引用被置空");
    }

    // assignID 递增: 每次分配获得新 ID
    static void testAssignIDIncrements()
    {
        ClassPoolSingle pool = new ClassPoolSingle();
        pool.setType(typeof(TestClass));
        ClassObject obj1 = pool.newClass();
        ClassObject obj2 = pool.newClass();
        long id1 = obj1.getAssignID();
        long id2 = obj2.getAssignID();
        assertTrue(id2 > id1, "第二次分配 assignID 更大: " + id1 + " < " + id2);
        // 销毁后复用, 分配 ID 仍递增
        pool.destroyClass(ref obj1);
        ClassObject obj3 = pool.newClass();
        assertTrue(obj3.getAssignID() > id2, "复用后 assignID 仍递增");
        pool.destroyClass(ref obj2);
        pool.destroyClass(ref obj3);
    }

    // FIFO 复用顺序: 先销毁的先复用(Queue)
    static void testFIFOReuseOrder()
    {
        ClassPoolSingle pool = new ClassPoolSingle();
        pool.setType(typeof(TestClass));
        ClassObject first = pool.newClass();
        ClassObject second = pool.newClass();
        ClassObject savedFirst = first;   // destroyClass 会置空引用, 先保存
        ClassObject savedSecond = second;
        // 按顺序销毁 first → second
        pool.destroyClass(ref first);
        pool.destroyClass(ref second);
        // FIFO: 先复用的是 first
        ClassObject reused = pool.newClass();
        assertTrue(ReferenceEquals(reused, savedFirst), "FIFO 先复先用先销毁的 first");
        pool.destroyClass(ref reused);
        ClassObject reused2 = pool.newClass();
        assertTrue(ReferenceEquals(reused2, savedSecond), "FIFO 第二次复用的是 second");
        pool.destroyClass(ref reused2);
    }

    // 交错创建/销毁循环: 复用不新建对象, 池对象总数(unused + inused)恒为 1
    static void testInterleavedCycle()
    {
        ClassPoolSingle pool = new ClassPoolSingle();
        pool.setType(typeof(TestClass));
        assertEqual(0, pool.getUnusedList().Count + pool.getInusedList().Count, "初始池对象总数 0");
        for (int i = 0; i < 5; ++i)
        {
            ClassObject obj = pool.newClass();
            // 第 2 次起复用 unused 中的对象, 不新建 → 总数保持 1
            assertEqual(1, pool.getUnusedList().Count + pool.getInusedList().Count, "第 " + (i + 1) + " 次分配后池对象总数仍 1(复用)");
            assertEqual(0, pool.getUnusedList().Count, "分配后 unused 0");
            pool.destroyClass(ref obj);
            assertEqual(1, pool.getUnusedList().Count, "销毁后 unused 1");
            assertEqual(1, pool.getUnusedList().Count + pool.getInusedList().Count, "池对象总数守恒 1");
        }
    }

    // destroyClassList: 批量销毁 + 列表被清空 + 后续复用
    static void testDestroyListBatch()
    {
        ClassPoolSingle pool = new ClassPoolSingle();
        pool.setType(typeof(TestClass));
        List<ClassObject> list = new List<ClassObject>();
        list.Add(pool.newClass());
        list.Add(pool.newClass());
        list.Add(pool.newClass());
        assertEqual(3, pool.getInusedList().Count, "创建 3 个对象");
        ClassObject firstAlloc = list[0];
        pool.destroyClassList(list);
        assertEqual(0, list.Count, "destroyClassList 后列表被清空");
        assertEqual(3, pool.getUnusedList().Count, "3 个对象进入未使用池");
        // 复用批量销毁的对象
        ClassObject reused = pool.newClass();
        assertTrue(ReferenceEquals(reused, firstAlloc), "批量销毁后复用的是第一个对象(FIFO)");
        pool.destroyClass(ref reused);
    }

    // 销毁后从 inused 列表移除(编辑器模式)
    static void testDestroyRemovesFromInused()
    {
        ClassPoolSingle pool = new ClassPoolSingle();
        pool.setType(typeof(TestClass));
        ClassObject obj = pool.newClass();
        ClassObject saved = obj;
        assertTrue(pool.getInusedList().Contains(saved), "创建后在 inused 列表");
        pool.destroyClass(ref obj);
        assertFalse(pool.getInusedList().Contains(saved), "销毁后从 inused 列表移除");
    }
}

// Simple test class for pool testing
public class TestClass : ClassObject
{
    public int mValue;
    public override void resetProperty()
    {
        base.resetProperty();
        mValue = 0;
    }
}