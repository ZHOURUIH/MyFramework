using UnityEngine;
using static TestAssert;

// SQLiteData 深度测试(数据表行封装, ClassObject)
//   mID/mValues: 数据字段
//   insert(ref string): 把 mID 追加到字符串
//   insert(MyStringBuilder): 同上(构建器版本)
//   getValue: 已有下标读回(缺失下标固定 logError 不测)
//   resetProperty: 清空字段
// 环境: new SQLiteData()(纯对象, 无 Unity 依赖)
public static class SQLiteDataTest
{
	public static void Run()
	{
#if USE_SQLITE
		testDefaults();
		testSetIDAndValue();
		testInsertRefString();
		testInsertBuilder();
		testResetProperty();
#endif
	}

#if USE_SQLITE
	// 默认值
	private static void testDefaults()
	{
		SQLiteData data = new SQLiteData();
		assertEqual(0, data.mID, "默认 mID=0");
		assertTrue(data.mValues.Count == 0, "默认 mValues 空");
		assertTrue(data.checkData(), "checkData 默认 true");
	}

	// mID/mValues 读写
	private static void testSetIDAndValue()
	{
		SQLiteData data = new SQLiteData();
		data.mID = 42;
		data.mValues[0] = "hello";
		data.mValues[1] = "world";
		assertEqual(42, data.mID, "mID 读回");
		assertEqual("hello", data.getValue(0), "getValue(0) 读回");
		assertEqual("world", data.getValue(1), "getValue(1) 读回");
	}

	// insert(ref string): mID 追加
	private static void testInsertRefString()
	{
		SQLiteData data = new SQLiteData();
		data.mID = 7;
		string value = "prefix;";
		data.insert(ref value);
		// appendValueInt 把 mID 追加到末尾
		assertTrue(value.Contains("7"), "insert 后字符串包含 mID");
		assertTrue(value.StartsWith("prefix;"), "insert 保留前缀");
	}

	// insert(MyStringBuilder): 构建器版本
	private static void testInsertBuilder()
	{
		SQLiteData data = new SQLiteData();
		data.mID = 9;
		MyStringBuilder builder = new MyStringBuilder();
		data.insert(builder);
		assertTrue(builder.ToString().Contains("9"), "构建器 insert 包含 mID");
	}

	// resetProperty: 清空
	private static void testResetProperty()
	{
		SQLiteData data = new SQLiteData();
		data.mID = 5;
		data.mValues[0] = "x";
		data.resetProperty();
		assertEqual(0, data.mID, "reset 后 mID=0");
		assertTrue(data.mValues.Count == 0, "reset 后 mValues 清空");
	}
#endif
}
