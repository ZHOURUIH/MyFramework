using System.Collections.Generic;
using UnityEngine;
using static SQLUtility;
using static TestAssert;

// SQLUtility SQL语句构造工具函数测试
public static class SQLUtilityTest
{
	public static void Run()
	{
		testAppendValueString();
		testAppendValueVector2();
		testAppendValueVector2Int();
		testAppendValueVector3();
		testAppendValueInt();
		testAppendValueUInt();
		testAppendValueFloat();
		testAppendValueFloats();
		testAppendValueInts();
		testAppendConditionString();
		testAppendConditionInt();
		testAppendUpdateString();
		testAppendUpdateInt();
		testAppendUpdateInts();
		testAppendUpdateFloats();
	}

	// ---- appendValueString ----
	static void testAppendValueString()
	{
		string query = "";
		appendValueString(ref query, "hello");
		assertEqual("\"hello\",", query, "appendValueString hello");
		appendValueString(ref query, "world");
		assertEqual("\"hello\",\"world\",", query, "appendValueString world");
		appendValueString(ref query, "");
		assertEqual("\"hello\",\"world\",\"\",", query, "appendValueString empty");
	}

	// ---- appendValueVector2 ----
	static void testAppendValueVector2()
	{
		string query = "";
		appendValueVector2(ref query, new Vector2(1.5f, 2.5f));
		// V2ToS(4) default precision: "1.5000,2.5000"
		string expected = new Vector2(1.5f, 2.5f).V2ToS() + ",";
		assertEqual(expected, query, "appendValueVector2");
	}

	// ---- appendValueVector2Int ----
	static void testAppendValueVector2Int()
	{
		string query = "";
		appendValueVector2Int(ref query, new Vector2Int(3, 4));
		assertEqual("3,4,", query, "appendValueVector2Int");
	}

	// ---- appendValueVector3 ----
	static void testAppendValueVector3()
	{
		string query = "";
		appendValueVector3(ref query, new Vector3(1, 2, 3));
		string expected = new Vector3(1, 2, 3).V3ToS() + ",";
		assertEqual(expected, query, "appendValueVector3");
	}

	// ---- appendValueInt ----
	static void testAppendValueInt()
	{
		string query = "";
		appendValueInt(ref query, 42);
		assertEqual("42,", query, "appendValueInt 42");
		appendValueInt(ref query, -7);
		assertEqual("42,-7,", query, "appendValueInt -7");
		appendValueInt(ref query, 0);
		assertEqual("42,-7,0,", query, "appendValueInt 0");
	}

	// ---- appendValueUInt ----
	static void testAppendValueUInt()
	{
		string query = "";
		appendValueUInt(ref query, 100u);
		assertEqual("100,", query, "appendValueUInt 100");
		appendValueUInt(ref query, 0u);
		assertEqual("100,0,", query, "appendValueUInt 0");
	}

	// ---- appendValueFloat ----
	static void testAppendValueFloat()
	{
		string query = "";
		appendValueFloat(ref query, 3.14f);
		// FToS default removes trailing zeros
		assertEqual("3.14,", query, "appendValueFloat 3.14");
		appendValueFloat(ref query, 0.0f);
		assertEqual("3.14,0,", query, "appendValueFloat 0");
	}

	// ---- appendValueFloats ----
	static void testAppendValueFloats()
	{
		string query = "";
		List<float> list = new() { 1.5f, 2.0f, 3.25f };
		appendValueFloats(ref query, list);
		assertEqual("\"1.5,2,3.25\",", query, "appendValueFloats");
		List<float> empty = new();
		string query2 = "";
		appendValueFloats(ref query2, empty);
		assertEqual("\"\",", query2, "appendValueFloats empty");
	}

	// ---- appendValueInts ----
	static void testAppendValueInts()
	{
		string query = "";
		List<int> list = new() { 1, 2, 3 };
		appendValueInts(ref query, list);
		assertEqual("\"1,2,3\",", query, "appendValueInts");
		List<int> empty = new();
		string query2 = "";
		appendValueInts(ref query2, empty);
		assertEqual("\"\",", query2, "appendValueInts empty");
	}

	// ---- appendConditionString ----
	static void testAppendConditionString()
	{
		string cond = "";
		appendConditionString(ref cond, "name", "Alice", " AND ");
		assertEqual("name = \"Alice\" AND ", cond, "appendConditionString AND");
		appendConditionString(ref cond, "city", "NYC", "");
		assertEqual("name = \"Alice\" AND city = \"NYC\"", cond, "appendConditionString end");
	}

	// ---- appendConditionInt ----
	static void testAppendConditionInt()
	{
		string cond = "";
		appendConditionInt(ref cond, "age", 25, " AND ");
		assertEqual("age = 25 AND ", cond, "appendConditionInt AND");
		appendConditionInt(ref cond, "score", 100, "");
		assertEqual("age = 25 AND score = 100", cond, "appendConditionInt end");
	}

	// ---- appendUpdateString ----
	static void testAppendUpdateString()
	{
		string update = "";
		appendUpdateString(ref update, "name", "Bob");
		assertEqual("name = \"Bob\",", update, "appendUpdateString Bob");
		appendUpdateString(ref update, "title", "Engineer");
		assertEqual("name = \"Bob\",title = \"Engineer\",", update, "appendUpdateString title");
	}

	// ---- appendUpdateInt ----
	static void testAppendUpdateInt()
	{
		string update = "";
		appendUpdateInt(ref update, "level", 5);
		assertEqual("level = 5,", update, "appendUpdateInt 5");
		appendUpdateInt(ref update, "exp", 0);
		assertEqual("level = 5,exp = 0,", update, "appendUpdateInt 0");
	}

	// ---- appendUpdateInts ----
	static void testAppendUpdateInts()
	{
		string update = "";
		List<int> list = new() { 10, 20, 30 };
		appendUpdateInts(ref update, "ids", list);
		assertEqual("ids = \"10,20,30\",", update, "appendUpdateInts");
		List<int> empty = new();
		string update2 = "";
		appendUpdateInts(ref update2, "ids", empty);
		assertEqual("ids = \"\",", update2, "appendUpdateInts empty");
	}

	// ---- appendUpdateFloats ----
	static void testAppendUpdateFloats()
	{
		string update = "";
		List<float> list = new() { 1.5f, 2.0f };
		appendUpdateFloats(ref update, "values", list);
		assertEqual("values = \"1.5,2\",", update, "appendUpdateFloats");
		List<float> empty = new();
		string update2 = "";
		appendUpdateFloats(ref update2, "values", empty);
		assertEqual("values = \"\",", update2, "appendUpdateFloats empty");
	}
}
