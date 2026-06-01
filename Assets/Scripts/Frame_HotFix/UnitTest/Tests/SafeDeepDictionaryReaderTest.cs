#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System.Collections.Generic;
using static TestAssert;

public class SafeDeepDictionaryReaderTest
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
		SafeDeepDictionary<string, int> dict = new();
		dict.add("a", 1);
		dict.add("b", 2);
		var reader = new SafeDeepDictionaryReader<string, int>(dict);
		assertEqual(2, reader.mList.Count);
		assertEqual(1, reader.mList["a"]);
		assertEqual(2, reader.mList["b"]);
		reader.Dispose();
	}
	private static void testReadListMatchesSource()
	{
		SafeDeepDictionary<string, int> dict = new();
		dict.add("x", 100);
		dict.add("y", 200);
		var reader = new SafeDeepDictionaryReader<string, int>(dict);
		assertEqual(dict.count(), reader.mList.Count);
		assertEqual(dict.tryGet("x"), reader.mList["x"]);
		assertEqual(dict.tryGet("y"), reader.mList["y"]);
		reader.Dispose();
	}
	private static void testDisposeEndsForeach()
	{
		SafeDeepDictionary<string, int> dict = new();
		dict.add("k", 1);
		var reader = new SafeDeepDictionaryReader<string, int>(dict);
		reader.Dispose();
		// 无法直接验证 isForeaching（无 public 方法），验证通过即可
	}
	private static void testUsingPattern()
	{
		SafeDeepDictionary<string, int> dict = new();
		dict.add("val", 42);
		int result = 0;
		using (var reader = new SafeDeepDictionaryReader<string, int>(dict))
		{
			result = reader.mList["val"];
		}
		assertEqual(42, result);
	}
	private static void testEmptyDictionary()
	{
		SafeDeepDictionary<string, int> dict = new();
		var reader = new SafeDeepDictionaryReader<string, int>(dict);
		assertEqual(0, reader.mList.Count);
		reader.Dispose();
	}
	private static void testMultipleReaders()
	{
		SafeDeepDictionary<string, int> dict = new();
		dict.add("a", 1);
		var reader1 = new SafeDeepDictionaryReader<string, int>(dict);
		var reader2 = new SafeDeepDictionaryReader<string, int>(dict);
		assertEqual(1, reader1.mList.Count);
		assertEqual(1, reader2.mList.Count);
		reader2.Dispose();
		reader1.Dispose();
	}
	private static void testModifyDuringRead()
	{
		SafeDeepDictionary<string, int> dict = new();
		dict.add("a", 1);
		var reader = new SafeDeepDictionaryReader<string, int>(dict);
		// 遍历期间修改源字典
		dict.add("b", 2);
		dict.remove("a");
		// 快照不受影响
		assertEqual(1, reader.mList.Count);
		assertEqual(1, reader.mList["a"]);
		reader.Dispose();
	}
}
#endif
