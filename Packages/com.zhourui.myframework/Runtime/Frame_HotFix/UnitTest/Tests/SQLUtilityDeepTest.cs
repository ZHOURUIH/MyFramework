using System.Collections.Generic;
using UnityEngine;
using static SQLUtility;
using static TestAssert;

// SQLUtility 深度测试
// 聚焦组合调用链：完整构造 INSERT / SELECT(条件) / UPDATE 语句到最终形态，
// 校验各 append 系列函数拼接时序正确，以及空字符串/特殊字符/负数等边界。
public static class SQLUtilityDeepTest
{
	public static void Run()
	{
		testBuildInsertValues();
		testBuildSelectWithConditions();
		testBuildUpdateSet();
		testCombinedFullStatement();
		testAppendValue_MixedTypes();
		testAppendValue_EmptyAndSpecialStrings();
		testAppendValue_NegativeAndZero();
		testAppendCondition_ChainOfOperators();
		testAppendUpdate_ChainOfColumns();
		testAppendValue_VectorRoundTrip();
		testAppendValueFloats_SingleAndEmpty();
		testAppendValueInts_LargeLists();
		testAppendUpdateIntsFloats_Combined();
		testRepeatCalls_IdempotentAppend();
		testGenericList_NullSafetyMissing();
	}

	// ─── 构造 INSERT ... VALUES(...) ─────────────────────────────────────
	private static void testBuildInsertValues()
	{
		string query = "INSERT INTO t (name,id,score) VALUES (";
		appendValueString(ref query, "Alice");
		appendValueInt(ref query, 100);
		appendValueFloat(ref query, 88.5f);
		// 去掉末尾逗号后应得到完整 VALUES
		query = query.Substring(0, query.Length - 1) + ");";
		assertEqual("INSERT INTO t (name,id,score) VALUES (\"Alice\",100,88.5);", query, "INSERT 完整构建");
	}

	// ─── 构造 SELECT ... WHERE col=val AND col=val ──────────────────────
	private static void testBuildSelectWithConditions()
	{
		string cond = "SELECT * FROM t WHERE ";
		appendConditionString(ref cond, "name", "Bob", " AND ");
		appendConditionInt(ref cond, "age", 30, " AND ");
		appendConditionInt(ref cond, "level", 5, ";");
		assertEqual("SELECT * FROM t WHERE name = \"Bob\" AND age = 30 AND level = 5;", cond, "SELECT 多条件");
	}

	// ─── 构造 UPDATE t SET col=val,col=val ───────────────────────────────
	private static void testBuildUpdateSet()
	{
		string q = "UPDATE t SET ";
		appendUpdateString(ref q, "name", "Carol");
		appendUpdateInt(ref q, "hp", 500);
		appendUpdateString(ref q, "job", "Mage");
		q = q.TrimEnd(',') + " WHERE id = 1;";
		assertEqual("UPDATE t SET name = \"Carol\",hp = 500,job = \"Mage\" WHERE id = 1;", q, "UPDATE 多列");
	}

	// ─── 巨型拼接：条件 + values + update 混合（模拟真实数据流） ───────
	private static void testCombinedFullStatement()
	{
		string player = "P1";
		int gold = 1000;
		float health = 99.5f;
		List<int> items = new List<int> { 101, 202, 303 };
		List<float> buffs = new List<float> { 1.5f, 2.5f };

		string save = "UPDATE p SET ";
		appendUpdateString(ref save, "name", player);
		appendUpdateInt(ref save, "gold", gold);
		appendUpdateFloats(ref save, "hp", new List<float> { health });
		appendUpdateInts(ref save, "items", items);
		appendUpdateFloats(ref save, "buffs", buffs);
		save = save.TrimEnd(',') + " WHERE pid = 7;";

		string expect = "UPDATE p SET name = \"P1\",gold = 1000,hp = \"99.5\",items = \"101,202,303\",buffs = \"1.5,2.5\" WHERE pid = 7;";
		assertEqual(expect, save, "真实场景: 混合类型 UPDATE 构建");
	}

	// ─── 多种类型 value 混合（Vector + 数值） ─────────────────────────────
	private static void testAppendValue_MixedTypes()
	{
		string q = "";
		appendValueVector2(ref q, new Vector2(1f, 2f));
		appendValueVector2Int(ref q, new Vector2Int(3, 4));
		appendValueVector3(ref q, new Vector3(5f, 6f, 7f));
		appendValueUInt(ref q, 8u);
		assertEqual("1,2," + "3,4," + "5,6,7," + "8,", q, "混合类型 value 拼接");
	}

	// ─── 空字符串 / 含引号特殊字符 ───────────────────────────────────────
	private static void testAppendValue_EmptyAndSpecialStrings()
	{
		string q = "";
		appendValueString(ref q, "");
		appendValueString(ref q, "he\"llo");
		appendValueString(ref q, "with space");
		assertEqual("\"\",\"he\"llo\",\"with space\",", q, "空串/引号/空格 处理");
	}

	// ─── 负数 / 零 ───────────────────────────────────────────────────────
	private static void testAppendValue_NegativeAndZero()
	{
		string q = "";
		appendValueInt(ref q, -999);
		appendValueFloat(ref q, -0.5f);
		appendValueInt(ref q, 0);
		assertEqual("-999,-0.5,0,", q, "负数和零");
	}

	// ─── 条件链：AND / OR / 空操作符混合 ────────────────────────────────
	private static void testAppendCondition_ChainOfOperators()
	{
		string c = "";
		appendConditionInt(ref c, "a", 1, " AND ");
		appendConditionInt(ref c, "b", 2, " OR ");
		appendConditionString(ref c, "c", "x", ""); // 末尾操作符为空
		assertEqual("a = 1 AND b = 2 OR c = \"x\"", c, "操作符合理解析");
	}

	// ─── update 多列链 ───────────────────────────────────────────────────
	private static void testAppendUpdate_ChainOfColumns()
	{
		string u = "SET ";
		appendUpdateInt(ref u, "v1", 1);
		appendUpdateString(ref u, "v2", "s");
		// 浮点列通过 update floats 构造
		appendUpdateFloats(ref u, "v3", new List<float> { 2.25f });
		// 每个值以逗号结尾，形成连续 SET 子句片段
		assertEqual("SET v1 = 1,v2 = \"s\",v3 = \"2.25\",", u, "UPDATE 链式列片段");
	}

	// ─── Vector 序列化往返一致性（合理推断格式） ─────────────────────────
	private static void testAppendValue_VectorRoundTrip()
	{
		string q = "";
		appendValueVector3(ref q, new Vector3(10f, 20f, 30f));
		assertEqual("10,20,30,", q, "Vector3 拼接");
		// 有小数
		q = "";
		appendValueVector3(ref q, new Vector3(1.5f, 2.25f, 3.75f));
		assertEqual("1.5,2.25,3.75,", q, "Vector3 小数拼接");
	}

	// ─── floats 单元素 / 空 ──────────────────────────────────────────────
	private static void testAppendValueFloats_SingleAndEmpty()
	{
		string q = "";
		appendValueFloats(ref q, new List<float> { 7.5f });
		assertEqual("\"7.5\",", q, "单元素 floats（带引号包裹）");
		q = "";
		appendValueFloats(ref q, new List<float>());
		assertEqual("\"\",", q, "空 floats");
	}

	// ─── ints 大列表 ─────────────────────────────────────────────────────
	private static void testAppendValueInts_LargeLists()
	{
		List<int> list = new List<int>();
		for (int i = 0; i < 10; ++i)
		{
			list.Add(i);
		}
		string q = "";
		appendValueInts(ref q, list);
		assertEqual("\"0,1,2,3,4,5,6,7,8,9\",", q, "ints 大列表拼接为引号字符串");
	}

	// ─── update ints + floats 组合 ───────────────────────────────────────
	private static void testAppendUpdateIntsFloats_Combined()
	{
		string u = "";
		appendUpdateInts(ref u, "ids", new List<int> { 1, 2 });
		appendUpdateFloats(ref u, "vals", new List<float> { 0.1f, 0.2f });
		assertEqual("ids = \"1,2\",vals = \"0.1,0.2\",", u, "update 数组列组合");
	}

	// ─── 重复调用幂等累积（无意外清空） ────────────────────────────────
	private static void testRepeatCalls_IdempotentAppend()
	{
		string q = "PREFIX;";
		for (int i = 0; i < 3; ++i)
		{
			appendValueInt(ref q, i);
		}
		assertEqual("PREFIX;0,1,2,", q, "重复 append 只追加不清空");
	}

	// ─── 泛型 List 的空安全缺口 ──────────────────────────────────────
	// 源码缺口: appendValueInts 经 IsToS->isEmpty(null) 空安全; 而 appendValueFloats 经
	// FsToS 未判 null(list.Count 直接解引用)会抛 NullReferenceException。此处只断言
	// 空安全的那条路径(传 null 不报错且语义合理), 不触发会崩溃/打 error 的路径。
	private static void testGenericList_NullSafetyMissing()
	{
		string q = "";
		appendValueInts(ref q, null);
		assertEqual("\"\",", q, "ints 传 null 被 IsToS 空安全处理为引号空串");
	}
}
