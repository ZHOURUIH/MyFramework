using System.Collections.Generic;
using UnityEngine;
using static SQLUtility;
using static TestAssert;

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

    // ─── appendValueString ───────────────────────────────────────────────
    static void testAppendValueString()
    {
        string query = "";
        appendValueString(ref query, "hello");
        assertEqual("\"hello\",", query, "appendValueString basic");

        query = "SELECT ";
        appendValueString(ref query, "world");
        assertEqual("SELECT \"world\",", query, "appendValueString prefix");
    }

    // ─── appendValueVector2 ──────────────────────────────────────────────
    static void testAppendValueVector2()
    {
        string query = "";
        appendValueVector2(ref query, new Vector2(1.0f, 2.0f));
        assertEqual("1,2,", query, "appendValueVector2");
    }

    // ─── appendValueVector2Int ───────────────────────────────────────────
    static void testAppendValueVector2Int()
    {
        string query = "";
        appendValueVector2Int(ref query, new Vector2Int(3, 4));
        assertEqual("3,4,", query, "appendValueVector2Int");
    }

    // ─── appendValueVector3 ──────────────────────────────────────────────
    static void testAppendValueVector3()
    {
        string query = "";
        appendValueVector3(ref query, new Vector3(1.0f, 2.0f, 3.0f));
        assertEqual("1,2,3,", query, "appendValueVector3");
    }

    // ─── appendValueInt ──────────────────────────────────────────────────
    static void testAppendValueInt()
    {
        string query = "";
        appendValueInt(ref query, 42);
        assertEqual("42,", query, "appendValueInt");

        query = "";
        appendValueInt(ref query, -7);
        assertEqual("-7,", query, "appendValueInt negative");

        query = "";
        appendValueInt(ref query, 0);
        assertEqual("0,", query, "appendValueInt zero");
    }

    // ─── appendValueUInt ─────────────────────────────────────────────────
    static void testAppendValueUInt()
    {
        string query = "";
        appendValueUInt(ref query, 100u);
        assertEqual("100,", query, "appendValueUInt");

        query = "";
        appendValueUInt(ref query, 0u);
        assertEqual("0,", query, "appendValueUInt zero");
    }

    // ─── appendValueFloat ────────────────────────────────────────────────
    static void testAppendValueFloat()
    {
        string query = "";
        appendValueFloat(ref query, 3.14f);
        assertEqual("3.14,", query, "appendValueFloat");

        query = "";
        appendValueFloat(ref query, 0.0f);
        assertEqual("0,", query, "appendValueFloat zero");
    }

    // ─── appendValueFloats ───────────────────────────────────────────────
    static void testAppendValueFloats()
    {
        string query = "";
        var list = new List<float> { 1.0f, 2.0f, 3.0f };
        appendValueFloats(ref query, list);
        assertEqual("\"1,2,3\",", query, "appendValueFloats");

        // 空列表
        query = "";
        var empty = new List<float>();
        appendValueFloats(ref query, empty);
        assertEqual("\"\",", query, "appendValueFloats empty");
    }

    // ─── appendValueInts ─────────────────────────────────────────────────
    static void testAppendValueInts()
    {
        string query = "";
        var list = new List<int> { 10, 20, 30 };
        appendValueInts(ref query, list);
        assertEqual("\"10,20,30\",", query, "appendValueInts");

        // 空列表
        query = "";
        var empty = new List<int>();
        appendValueInts(ref query, empty);
        assertEqual("\"\",", query, "appendValueInts empty");
    }

    // ─── appendConditionString ───────────────────────────────────────────
    static void testAppendConditionString()
    {
        string cond = "";
        appendConditionString(ref cond, "name", "Alice", " AND ");
        assertEqual("name = \"Alice\" AND ", cond, "appendConditionString AND");

        cond = "";
        appendConditionString(ref cond, "id", "123", "");
        assertEqual("id = \"123\"", cond, "appendConditionString no operate");
    }

    // ─── appendConditionInt ──────────────────────────────────────────────
    static void testAppendConditionInt()
    {
        string cond = "";
        appendConditionInt(ref cond, "age", 25, " OR ");
        assertEqual("age = 25 OR ", cond, "appendConditionInt OR");

        cond = "";
        appendConditionInt(ref cond, "level", 10, "");
        assertEqual("level = 10", cond, "appendConditionInt no operate");

        cond = "";
        appendConditionInt(ref cond, "count", 0, " AND ");
        assertEqual("count = 0 AND ", cond, "appendConditionInt zero");
    }

    // ─── appendUpdateString ──────────────────────────────────────────────
    static void testAppendUpdateString()
    {
        string update = "";
        appendUpdateString(ref update, "name", "Bob");
        assertEqual("name = \"Bob\",", update, "appendUpdateString");

        update = "SET ";
        appendUpdateString(ref update, "title", "Manager");
        assertEqual("SET title = \"Manager\",", update, "appendUpdateString prefix");
    }

    // ─── appendUpdateInt ─────────────────────────────────────────────────
    static void testAppendUpdateInt()
    {
        string update = "";
        appendUpdateInt(ref update, "age", 30);
        assertEqual("age = 30,", update, "appendUpdateInt");

        update = "";
        appendUpdateInt(ref update, "score", -1);
        assertEqual("score = -1,", update, "appendUpdateInt negative");
    }

    // ─── appendUpdateInts ────────────────────────────────────────────────
    static void testAppendUpdateInts()
    {
        string update = "";
        var list = new List<int> { 1, 2, 3 };
        appendUpdateInts(ref update, "ids", list);
        assertEqual("ids = \"1,2,3\",", update, "appendUpdateInts");

        update = "";
        var empty = new List<int>();
        appendUpdateInts(ref update, "ids", empty);
        assertEqual("ids = \"\",", update, "appendUpdateInts empty");
    }

    // ─── appendUpdateFloats ──────────────────────────────────────────────
    static void testAppendUpdateFloats()
    {
        string update = "";
        var list = new List<float> { 1.5f, 2.5f };
        appendUpdateFloats(ref update, "values", list);
        assertEqual("values = \"1.5,2.5\",", update, "appendUpdateFloats");

        update = "";
        var empty = new List<float>();
        appendUpdateFloats(ref update, "values", empty);
        assertEqual("values = \"\",", update, "appendUpdateFloats empty");
    }
}
