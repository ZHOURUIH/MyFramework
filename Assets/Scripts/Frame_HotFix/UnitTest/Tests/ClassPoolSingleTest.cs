#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;
using System.Threading;
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
#endif
