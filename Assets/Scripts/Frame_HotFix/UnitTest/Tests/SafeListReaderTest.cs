#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections.Generic;
using static TestAssert;

public class SafeListReaderTest
{
	public static void Run()
	{
		testConstructorPopulatesReadList();
		testReadListMatchesSource();
		testDisposeEndsForeach();
		testUsingPattern();
		testEmptyList();
		testMultipleReaders();
		testModifyDuringRead();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	private static void testConstructorPopulatesReadList()
	{
		SafeList<int> list = new();
		list.add(1);
		list.add(2);
		list.add(3);
		var reader = new SafeListReader<int>(list);
		assertEqual(3, reader.mReadList.Count);
		assertEqual(1, reader.mReadList[0]);
		assertEqual(2, reader.mReadList[1]);
		assertEqual(3, reader.mReadList[2]);
		reader.Dispose();
	}
	private static void testReadListMatchesSource()
	{
		SafeList<string> list = new();
		list.add("hello");
		list.add("world");
		var reader = new SafeListReader<string>(list);
		assertEqual(list.count(), reader.mReadList.Count);
		assertEqual(list.getMainList()[0], reader.mReadList[0]);
		assertEqual(list.getMainList()[1], reader.mReadList[1]);
		reader.Dispose();
	}
	private static void testDisposeEndsForeach()
	{
		SafeList<int> list = new();
		list.add(10);
		var reader = new SafeListReader<int>(list);
		reader.Dispose();
	}
	private static void testUsingPattern()
	{
		SafeList<int> list = new();
		list.add(100);
		int sum = 0;
		using (var reader = new SafeListReader<int>(list))
		{
			foreach (int val in reader.mReadList)
			{
				sum += val;
			}
		}
		assertEqual(100, sum);
	}
	private static void testEmptyList()
	{
		SafeList<int> list = new();
		var reader = new SafeListReader<int>(list);
		assertEqual(0, reader.mReadList.Count);
		reader.Dispose();
	}
	private static void testMultipleReaders()
	{
		SafeList<int> list = new();
		list.add(1);
		list.add(2);
		// SafeList 不支持嵌套遍历，顺序创建 Reader
		var reader1 = new SafeListReader<int>(list);
		assertEqual(2, reader1.mReadList.Count);
		reader1.Dispose();
		var reader2 = new SafeListReader<int>(list);
		assertEqual(2, reader2.mReadList.Count);
		reader2.Dispose();
	}
	private static void testModifyDuringRead()
	{
		SafeList<int> list = new();
		list.add(1);
		list.add(2);
		list.add(3);
		var reader = new SafeListReader<int>(list);
		// 遍历期间修改源列表
		list.add(4);
		list.remove(1);
		list.remove(2);
		// 快照不受影响
		assertEqual(3, reader.mReadList.Count);
		reader.Dispose();
	}
}
#endif
