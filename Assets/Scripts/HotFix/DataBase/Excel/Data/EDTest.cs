// auto generate start
using System;
using System.Collections.Generic;
using UnityEngine;

// 测试例子的表格
public class EDTest : ExcelData
{
	public static int TEST_0_ID = 1;				// 测试字段
	public static int TEST_1_ID = 2;				// 测试字段

	public static EDTest TEST_0;					// 测试字段
	public static EDTest TEST_1;					// 测试字段

	public string mTestString;						// 测试字符串
	public int mTestInt;							// 测试整数
	public long mTestLong;							// 测试长整数
	public Vector2Int mTestVector2Int;				// 测试整数二维向量
	public Vector2 mTestVector2;					// 测试二维向量
	public TEST_ENUM mTestEnum;						// 测试枚举
	public List<int> mTestList0 = new();			// 测试整数列表
	public List<int> mTestList1 = new();			// 测试整数列表
	public float mTestFloat;						// 测试浮点数
	public List<string> mTestStringList = new();	// 测试字符串列表
	public int mTestLinkTable0;						// 测试索引到其他表格
	public List<int> mTestLinkTable1 = new();		// 测试索引到其他表格
	public string mTestPath;						// 测试文件路径,GameResources下的相对路径
	public override bool read(SerializerRead reader)
	{
		bool result = base.read(reader);
		result = result && reader.readString(out mTestString);
		result = result && reader.read(out mTestInt);
		result = result && reader.read(out mTestLong);
		result = result && reader.read(out mTestVector2Int);
		result = result && reader.read(out mTestVector2);
		result = result && reader.readEnumByte(out mTestEnum);
		result = result && reader.readList(mTestList0);
		result = result && reader.readList(mTestList1);
		result = result && reader.read(out mTestFloat);
		result = result && reader.readList(mTestStringList);
		result = result && reader.read(out mTestLinkTable0);
		result = result && reader.readList(mTestLinkTable1);
		result = result && reader.readString(out mTestPath);
		return result;
	}
	public static void postLoadAll(ExcelTableT<EDTest> table)
	{
		TEST_0 = table.query(TEST_0_ID);
		TEST_1 = table.query(TEST_1_ID);
	}
}

public class EDTest_TEST_0
{
	public static int mTestParam0 = 1;				// 用于生成测试的ID参数
	public static float mTestParam1 = 1.5f;			// 用于生成测试的ID参数
}
// auto generate end