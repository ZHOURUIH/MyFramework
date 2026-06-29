using static TestAssert;

public class SafeDictionaryReaderTest
{
	public static void Run()
	{
		testConstructorPopulatesReadList();
		testReadListMatchesSource();
		testDisposeEndsForeach();
		testUsingPattern();
		testEmptyDictionary();
		testMultipleReaders();
		testModifyDuringRead();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	private static void testConstructorPopulatesReadList()
	{
		SafeDictionary<string, int> dict = new();
		dict.add("a", 1);
		dict.add("b", 2);
		var reader = new SafeDictionaryReader<string, int>(dict);
		assertEqual(2, reader.mReadList.Count);
		assertEqual(1, reader.mReadList["a"]);
		assertEqual(2, reader.mReadList["b"]);
		reader.Dispose();
	}
	private static void testReadListMatchesSource()
	{
		SafeDictionary<string, int> dict = new();
		dict.add("x", 100);
		dict.add("y", 200);
		var reader = new SafeDictionaryReader<string, int>(dict);
		assertEqual(dict.count(), reader.mReadList.Count);
		reader.Dispose();
	}
	private static void testDisposeEndsForeach()
	{
		SafeDictionary<string, int> dict = new();
		dict.add("k", 1);
		var reader = new SafeDictionaryReader<string, int>(dict);
		reader.Dispose();
	}
	private static void testUsingPattern()
	{
		SafeDictionary<string, int> dict = new();
		dict.add("val", 42);
		int result = 0;
		using (var reader = new SafeDictionaryReader<string, int>(dict))
		{
			result = reader.mReadList["val"];
		}
		assertEqual(42, result);
	}
	private static void testEmptyDictionary()
	{
		SafeDictionary<string, int> dict = new();
		var reader = new SafeDictionaryReader<string, int>(dict);
		assertEqual(0, reader.mReadList.Count);
		reader.Dispose();
	}
	private static void testMultipleReaders()
	{
		SafeDictionary<string, int> dict = new();
		dict.add("a", 1);
		// SafeDictionary 不支持嵌套遍历，顺序创建 Reader
		var reader1 = new SafeDictionaryReader<string, int>(dict);
		assertEqual(1, reader1.mReadList.Count);
		reader1.Dispose();
		var reader2 = new SafeDictionaryReader<string, int>(dict);
		assertEqual(1, reader2.mReadList.Count);
		reader2.Dispose();
	}
	private static void testModifyDuringRead()
	{
		SafeDictionary<string, int> dict = new();
		dict.add("a", 1);
		var reader = new SafeDictionaryReader<string, int>(dict);
		// 遍历期间修改源字典
		dict.add("b", 2);
		dict.remove("a");
		// 快照不受影响
		assertEqual(1, reader.mReadList.Count);
		assertEqual(1, reader.mReadList["a"]);
		reader.Dispose();
	}
}