using System.Collections.Generic;
using static TestAssert;

// ClassPool: 多类型全局对象池(纯 C# 池逻辑, 局部 new 不调 init 可测)
// 环境: isEditor() 路径需要 GameEntryBase 单例(测试环境已初始化); TestClass 复用 ClassPoolSingleTest 的定义
public static class ClassPoolTest
{
	public static void Run()
	{
		testMultiTypeIsolation();
		testOnlyOnceInused();
		testOutNewClass();
		testReuseSameType();
		testDestroyRefNull();
		testDestroyQueueBatch();
		testDestroyListThree();
		testAssignIDIncrement();
		testClearUnused();
	}

	private static ClassPool createPool()
	{
		return new ClassPool();
	}

	// 多类型隔离: 不同类型独立 unused/inused
	private static void testMultiTypeIsolation()
	{
		ClassPool pool = createPool();
		TestClass a = pool.newClass<TestClass>(true);
		TestClass2 b = pool.newClass<TestClass2>(true);
		assertTrue(a != null && b != null, "两种类型都可分配");
		assertTrue(pool.getInusedList().ContainsKey(typeof(TestClass)), "TestClass 在 inused");
		assertTrue(pool.getInusedList().ContainsKey(typeof(TestClass2)), "TestClass2 在 inused");
		// 销毁 a 只影响 TestClass 的 unused
		pool.destroyClass(ref a);
		assertEqual(1, pool.getUnusedList()[typeof(TestClass)].Count, "TestClass unused 1");
		assertFalse(pool.getUnusedList().ContainsKey(typeof(TestClass2)), "TestClass2 无 unused 队列");
		pool.destroyClass(ref b);
	}

	// onlyOnce=true → mInusedList; false → mPersistentInuseList
	private static void testOnlyOnceInused()
	{
		ClassPool pool = createPool();
		TestClass temp = pool.newClass<TestClass>(true);
		TestClass persistent = pool.newClass<TestClass>(false);
		assertTrue(pool.getInusedList()[typeof(TestClass)].Contains(temp), "onlyOnce=true 对象在 inused 列表");
		assertTrue(pool.getPersistentInusedList()[typeof(TestClass)].Contains(persistent), "onlyOnce=false 对象在持久 inused 列表");
		pool.destroyClass(ref temp);
		pool.destroyClass(ref persistent);
	}

	// out 版本 newClass
	private static void testOutNewClass()
	{
		ClassPool pool = createPool();
		pool.newClass<TestClass>(out TestClass obj, true);
		assertTrue(obj != null, "out 版本分配非空");
		assertTrue(obj is TestClass, "out 版本类型正确");
		pool.destroyClass(ref obj);
	}

	// 同类型销毁后复用(保存引用, destroyClass 置空外部引用)
	private static void testReuseSameType()
	{
		ClassPool pool = createPool();
		TestClass obj = pool.newClass<TestClass>(true);
		TestClass saved = obj;
		pool.destroyClass(ref obj);
		TestClass reused = pool.newClass<TestClass>(true);
		assertTrue(ReferenceEquals(reused, saved), "同类型销毁后复用同一实例");
		pool.destroyClass(ref reused);
	}

	// destroyClass 置空外部引用
	private static void testDestroyRefNull()
	{
		ClassPool pool = createPool();
		TestClass obj = pool.newClass<TestClass>(true);
		assertTrue(obj != null, "分配非空");
		pool.destroyClass(ref obj);
		assertTrue(obj == null, "destroyClass 后外部引用置空");
	}

	// destroyClass(Queue) 批量: 队列被清空 + FIFO 复用
	private static void testDestroyQueueBatch()
	{
		ClassPool pool = createPool();
		Queue<TestClass> queue = new Queue<TestClass>();
		TestClass first = pool.newClass<TestClass>(true);
		TestClass saved = first;
		queue.Enqueue(first);
		queue.Enqueue(pool.newClass<TestClass>(true));
		queue.Enqueue(pool.newClass<TestClass>(true));
		pool.destroyClass(queue);
		assertEqual(0, queue.Count, "destroyClass(Queue) 后队列清空");
		assertEqual(3, pool.getUnusedList()[typeof(TestClass)].Count, "批量销毁后 unused 3");
		// FIFO 复用第一个
		TestClass reused = pool.newClass<TestClass>(true);
		assertTrue(ReferenceEquals(reused, saved), "批量销毁后 FIFO 复用第一个");
		pool.destroyClass(ref reused);
	}

	// destroyClassList 三种容器: List/HashSet/Dictionary
	private static void testDestroyListThree()
	{
		// 每种容器用独立 pool, 避免 newClass 复用 unused 干扰计数
		// List(注意: ClassPool 版本不清空传入列表, 只回收对象)
		ClassPool pool1 = createPool();
		List<TestClass> list = new List<TestClass> { pool1.newClass<TestClass>(true), pool1.newClass<TestClass>(true) };
		pool1.destroyClassList(list);
		assertEqual(2, pool1.getUnusedList()[typeof(TestClass)].Count, "List 批量销毁后 unused 2");
		// HashSet
		ClassPool pool2 = createPool();
		HashSet<TestClass> set = new HashSet<TestClass> { pool2.newClass<TestClass>(true) };
		pool2.destroyClassList(set);
		assertEqual(1, pool2.getUnusedList()[typeof(TestClass)].Count, "HashSet 批量销毁后 unused 1");
		// Dictionary
		ClassPool pool3 = createPool();
		Dictionary<int, TestClass> dict = new Dictionary<int, TestClass>();
		dict[1] = pool3.newClass<TestClass>(true);
		dict[2] = pool3.newClass<TestClass>(true);
		pool3.destroyClassList(dict);
		assertEqual(2, pool3.getUnusedList()[typeof(TestClass)].Count, "Dictionary 批量销毁后 unused 2");
	}

	// assignID 递增(全局 seed 跨类型共享)
	private static void testAssignIDIncrement()
	{
		ClassPool pool = createPool();
		TestClass a = pool.newClass<TestClass>(true);
		TestClass b = pool.newClass<TestClass>(true);
		assertTrue(b.getAssignID() > a.getAssignID(), "连续分配 assignID 递增");
		pool.destroyClass(ref a);
		pool.destroyClass(ref b);
	}

	// clearUnused 清空未使用列表
	private static void testClearUnused()
	{
		ClassPool pool = createPool();
		TestClass obj = pool.newClass<TestClass>(true);
		pool.destroyClass(ref obj);
		assertEqual(1, pool.getUnusedList()[typeof(TestClass)].Count, "销毁后 unused 1");
		pool.clearUnused();
		assertEqual(0, pool.getUnusedList()[typeof(TestClass)].Count, "clearUnused 后 unused 0");
	}
}

// 测试辅助: 第二个池化类型(避免与 ClassPoolSingleTest.TestClass 重名)
public class TestClass2 : ClassObject
{
	public int mValue;
	public override void resetProperty()
	{
		base.resetProperty();
		mValue = 0;
	}
}
