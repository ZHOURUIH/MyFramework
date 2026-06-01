#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections.Generic;
using static TestAssert;

public class SafeHashSetReaderTest
{
	public static void Run()
	{
		testConstructorPopulatesReadList();
		testReadListMatchesSource();
		testUsingPattern();
		testEmptySet();
		testMultipleReaders();
		testModifyDuringRead();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	private static void testConstructorPopulatesReadList()
	{
		SafeHashSet<int> set = new();
		set.add(1);
		set.add(2);
		set.add(3);
		var reader = new SafeHashSetReader<int>(set);
		assertEqual(3, reader.mReadList.Count);
		assertTrue(reader.mReadList.Contains(1));
		assertTrue(reader.mReadList.Contains(2));
		assertTrue(reader.mReadList.Contains(3));
		reader.Dispose();
	}
	private static void testReadListMatchesSource()
	{
		SafeHashSet<string> set = new();
		set.add("hello");
		set.add("world");
		var reader = new SafeHashSetReader<string>(set);
		assertEqual(set.count(), reader.mReadList.Count);
		reader.Dispose();
	}
	private static void testUsingPattern()
	{
		SafeHashSet<int> set = new();
		set.add(42);
		bool found = false;
		using (var reader = new SafeHashSetReader<int>(set))
		{
			found = reader.mReadList.Contains(42);
		}
		assertTrue(found);
	}
	private static void testEmptySet()
	{
		SafeHashSet<int> set = new();
		var reader = new SafeHashSetReader<int>(set);
		assertEqual(0, reader.mReadList.Count);
		reader.Dispose();
	}
	private static void testMultipleReaders()
	{
		SafeHashSet<int> set = new();
		set.add(1);
		set.add(2);
		// SafeHashSet 不支持嵌套遍历，顺序创建 Reader
		var reader1 = new SafeHashSetReader<int>(set);
		assertEqual(2, reader1.mReadList.Count);
		reader1.Dispose();
		var reader2 = new SafeHashSetReader<int>(set);
		assertEqual(2, reader2.mReadList.Count);
		reader2.Dispose();
	}
	private static void testModifyDuringRead()
	{
		SafeHashSet<int> set = new();
		set.add(1);
		set.add(2);
		set.add(3);
		var reader = new SafeHashSetReader<int>(set);
		// 遍历期间修改源集合
		set.add(4);
		set.remove(1);
		// 快照不受影响
		assertEqual(3, reader.mReadList.Count);
		reader.Dispose();
	}
}
#endif
