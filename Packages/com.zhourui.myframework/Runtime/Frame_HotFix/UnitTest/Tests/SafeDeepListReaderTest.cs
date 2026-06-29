using static TestAssert;

public class SafeDeepListReaderTest
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
		SafeDeepList<int> list = new();
		list.add(1);
		list.add(2);
		list.add(3);
		var reader = new SafeDeepListReader<int>(list);
		assertEqual(3, reader.mReadList.Count);
		assertEqual(1, reader.mReadList[0]);
		assertEqual(2, reader.mReadList[1]);
		assertEqual(3, reader.mReadList[2]);
		reader.Dispose();
	}
	private static void testReadListMatchesSource()
	{
		SafeDeepList<string> list = new();
		list.add("a");
		list.add("b");
		var reader = new SafeDeepListReader<string>(list);
		// mReadList 是快照（副本），与主列表内容一致
		assertEqual(list.count(), reader.mReadList.Count);
		assertEqual(list.getMainList()[0], reader.mReadList[0]);
		assertEqual(list.getMainList()[1], reader.mReadList[1]);
		reader.Dispose();
	}
	private static void testDisposeEndsForeach()
	{
		SafeDeepList<int> list = new();
		list.add(10);
		var reader = new SafeDeepListReader<int>(list);
		assertTrue(list.isForeaching());
		reader.Dispose();
		assertFalse(list.isForeaching());
	}
	private static void testUsingPattern()
	{
		SafeDeepList<int> list = new();
		list.add(42);
		int sum = 0;
		using (var reader = new SafeDeepListReader<int>(list))
		{
			foreach (int val in reader.mReadList)
			{
				sum += val;
			}
		}
		assertEqual(42, sum);
		assertFalse(list.isForeaching());
	}
	private static void testEmptyList()
	{
		SafeDeepList<int> list = new();
		var reader = new SafeDeepListReader<int>(list);
		assertEqual(0, reader.mReadList.Count);
		reader.Dispose();
	}
	private static void testMultipleReaders()
	{
		SafeDeepList<int> list = new();
		list.add(1);
		list.add(2);
		var reader1 = new SafeDeepListReader<int>(list);
		var reader2 = new SafeDeepListReader<int>(list);
		assertEqual(2, reader1.mReadList.Count);
		assertEqual(2, reader2.mReadList.Count);
		reader2.Dispose();
		reader1.Dispose();
	}
	private static void testModifyDuringRead()
	{
		SafeDeepList<int> list = new();
		list.add(1);
		list.add(2);
		var reader = new SafeDeepListReader<int>(list);
		// 遍历期间修改源列表
		list.add(3);
		list.remove(1);
		// 快照不受影响
		assertEqual(2, reader.mReadList.Count);
		assertEqual(1, reader.mReadList[0]);
		assertEqual(2, reader.mReadList[1]);
		reader.Dispose();
	}
}