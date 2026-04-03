#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;

// DictionaryExtension 扩展方法测试
// 覆盖：isEmpty / count / get / add / addIf / addOrSet / replace / setRange / addRange /
//        getKeyOfValue / getOrAdd / getOrAddNew / addOrIncreaseValue /
//        remove / removeIf / find / findKey / findValue / containsKey / containsValue /
//        first / firstKey / firstValue / ForKey / ForValue / safe / EmptyDictionary
public static class DictionaryExtensionTest
{
    public static void Run()
    {
        testIsEmptyAndCount();
        testGetValue();
        testAddAndAddOrSet();
        testAddIf();
        testReplace();
        testSetRangeAndAddRange();
        testGetKeyOfValue();
        testGetOrAdd();
        testGetOrAddNew();
        testAddOrIncreaseValue();
        testRemove();
        testRemoveIf();
        testFind();
        testContains();
        testFirst();
        testForKeyAndForValue();
        testSafe();
        testEmptyDictionary();
    }

    // ─── isEmpty / count ─────────────────────────────────────────────────
    private static void testIsEmptyAndCount()
    {
        Dictionary<int, string> nullDic = null;
        Assert(nullDic.isEmpty(),        "null dic isEmpty=true");
        AssertEqual(0, nullDic.count(),  "null dic count=0");

        var dic = new Dictionary<int, string>();
        Assert(dic.isEmpty(),            "empty dic isEmpty=true");
        dic.Add(1, "a");
        Assert(!dic.isEmpty(),           "non-empty isEmpty=false");
        AssertEqual(1, dic.count(),      "count=1");
    }

    // ─── get ─────────────────────────────────────────────────────────────
    private static void testGetValue()
    {
        var dic = new Dictionary<int, string> { { 1, "one" }, { 2, "two" } };

        AssertEqual("one", dic.get(1),      "get existing key");
        AssertEqual("two", dic.get(2),      "get existing key 2");

        // 不存在的 key 返回 default
        AssertEqual(null, dic.get(99),      "get non-existing key → default");

        // 带默认值的 get
        AssertEqual("default", dic.get(99, "default"), "get with default value");

        // null 字典
        Dictionary<int, string> nullDic = null;
        AssertEqual(null, nullDic.get(1),   "null dic get → default");
    }

    // ─── add / addOrSet ──────────────────────────────────────────────────
    private static void testAddAndAddOrSet()
    {
        var dic = new Dictionary<int, string>();

        // add
        dic.add(1, "first");
        AssertEqual("first", dic[1], "add new key");

        // addOrSet (不存在时添加)
        dic.addOrSet(2, "second");
        AssertEqual("second", dic[2], "addOrSet new key");

        // addOrSet (存在时替换)
        dic.addOrSet(1, "updated");
        AssertEqual("updated", dic[1], "addOrSet existing key → replace");
    }

    // ─── addIf ───────────────────────────────────────────────────────────
    private static void testAddIf()
    {
        var dic = new Dictionary<int, string>();

        dic.addIf(1, "yes", true);
        dic.addIf(2, "no", false);

        Assert(dic.ContainsKey(1), "addIf true → added");
        Assert(!dic.ContainsKey(2), "addIf false → not added");
    }

    // ─── replace ─────────────────────────────────────────────────────────
    private static void testReplace()
    {
        var dic = new Dictionary<int, string> { { 1, "old" } };

        string old = dic.replace(1, "new");
        AssertEqual("old", old, "replaced value updated");
        AssertEqual("new", dic[1], "replace value updated");

        bool replaced = dic.replace(99, "new", out _);
        Assert(replaced, "replace non-existing key → true");
        AssertEqual("new", dic[99], "replace non-existing key should insert");
    }

    // ─── setRange / addRange ─────────────────────────────────────────────
    private static void testSetRangeAndAddRange()
    {
        var dic = new Dictionary<int, string> { { 1, "a" } };
        var src = new Dictionary<int, string> { { 2, "b" }, { 3, "c" } };

        // setRange (覆盖)
        dic.setRange(src);
        AssertEqual(2, dic.count(), "setRange count=2");
        AssertEqual("b", dic[2], "setRange key2=b");

        // addRange (不覆盖)
        dic.addRange(new Dictionary<int, string> { { 4, "d" } });
        AssertEqual(3, dic.count(), "addRange count=3");
    }

    // ─── getKeyOfValue ───────────────────────────────────────────────────
    private static void testGetKeyOfValue()
    {
        var dic = new Dictionary<int, string> { { 1, "a" }, { 2, "b" }, { 3, "a" } };

        int key = dic.getKeyOfValue("a");
        Assert(key == 1 || key == 3, "getKeyOfValue returns first match");

        int notFound = dic.getKeyOfValue("xyz");
        AssertEqual(0, notFound, "getKeyOfValue not found → default");
    }

    // ─── getOrAdd ────────────────────────────────────────────────────────
    private static void testGetOrAdd()
    {
        var dic = new Dictionary<int, string>();

		// 不存在时添加
		string val1 = dic.getOrAdd(1, "default");
        AssertEqual("default", val1, "getOrAdd new key → default value");
        AssertEqual("default", dic[1], "getOrAdd added to dict");

        // 已存在时返回现有值
        dic[1] = "updated";
        string val2 = dic.getOrAdd(1, "newDefault");
        AssertEqual("updated", val2, "getOrAdd existing key → current value");
    }

    // ─── getOrAddNew ─────────────────────────────────────────────────────
    private static void testGetOrAddNew()
    {
        var dic = new Dictionary<int, List<string>>();

        List<string> list = dic.getOrAddNew(1);
        AssertNotNull(list, "getOrAddNew returns new list");
        AssertEqual(0, list.Count, "getOrAddNew list empty");

        list.Add("item");
        List<string> sameList = dic.getOrAddNew(1);
        AssertEqual(1, sameList.Count, "getOrAddNew existing key → same list");
    }

    // ─── addOrIncreaseValue ──────────────────────────────────────────────
    private static void testAddOrIncreaseValue()
    {
        var dic = new Dictionary<string, int>();

        dic.addOrIncreaseValue("a", 5);
        AssertEqual(5, dic["a"], "addOrIncreaseValue new key");

        dic.addOrIncreaseValue("a", 3);
        AssertEqual(8, dic["a"], "addOrIncreaseValue existing key → sum");
    }

    // ─── remove ──────────────────────────────────────────────────────────
    private static void testRemove()
    {
        var dic = new Dictionary<int, string> { { 1, "a" }, { 2, "b" } };
        dic.remove(1, 2);
        Assert(!dic.ContainsKey(1), "remove key removed");
    }

    // ─── removeIf ────────────────────────────────────────────────────────
    private static void testRemoveIf()
    {
        var dic = new Dictionary<int, string> { { 1, "a" }, { 2, "b" }, { 3, "c" } };

        int removed = dic.remove((k, v) => k % 2 == 0);
        AssertEqual(1, removed, "removeIf removed 1 item");
        Assert(!dic.ContainsKey(2), "removeIf removed even key");
        AssertEqual(2, dic.count(), "removeIf count=2");
    }

    // ─── find ────────────────────────────────────────────────────────────
    private static void testFind()
    {
        var dic = new Dictionary<int, string> { { 1, "a" }, { 2, "b" }, { 3, "c" } };

        // find by predicate → KeyValuePair
        dic.find((k, v) => v == "b", out var kv);
        Assert(kv.Key == 2, "find by value key=2");

        // findKey
        dic.findKey(k => k == 3, out int key);
        AssertEqual(3, key, "findKey value=c → key=3");

        // findValue
        string val = dic.findValue(k => k == 1);
        AssertEqual("a", val, "findValue key=1 → value=a");
    }

    // ─── containsKey / containsValue ─────────────────────────────────────
    private static void testContains()
    {
        var dic = new Dictionary<int, string> { { 1, "a" } };
        Assert(dic.containsValue("a"), "containsValue existing");
        Assert(!dic.containsValue("xyz"), "containsValue non-existing");
    }

    // ─── first / firstKey / firstValue ───────────────────────────────────
    private static void testFirst()
    {
        var dic = new Dictionary<int, string> { { 1, "a" }, { 2, "b" } };

        var first = dic.first();
        Assert(first.Key == 1 || first.Key == 2, "first returns some item");

        int firstKey = dic.firstKey();
        Assert(firstKey == 1 || firstKey == 2, "firstKey returns some key");

        string firstValue = dic.firstValue();
        Assert(firstValue == "a" || firstValue == "b", "firstValue returns some value");

        // 空字典
        var empty = new Dictionary<int, string>();
        var emptyFirst = empty.first();
        AssertEqual(0, emptyFirst.Key, "first empty → default");
        AssertEqual(null, emptyFirst.Value, "first empty → default value");
    }

    // ─── ForKey / ForValue ───────────────────────────────────────────────
    private static void testForKeyAndForValue()
    {
        var dic = new Dictionary<int, string> { { 1, "a" }, { 2, "b" } };

        int keySum = 0;
        dic.forKey(k => keySum += k);
        AssertEqual(3, keySum, "ForKey sum keys=3");

        string valueConcat = "";
        dic.forValue(v => valueConcat += v);
        Assert(valueConcat.Contains("a") && valueConcat.Contains("b"), "ForValue concatenates values");
    }

    // ─── safe ────────────────────────────────────────────────────────────
    private static void testSafe()
    {
        Dictionary<int, string> nullDic = null;
        var safe = nullDic.safe();
        AssertNotNull(safe, "safe null → non-null");
        AssertEqual(0, safe.count(), "safe null → empty");

        var dic = new Dictionary<int, string> { { 1, "a" } };
        var safe2 = dic.safe();
        AssertEqual(1, safe2.count(), "safe non-null → same");
    }

    // ─── EmptyDictionary ─────────────────────────────────────────────────
    private static void testEmptyDictionary()
    {
        var e1 = EmptyDictionary<int, string>.getEmptyList();
        var e2 = EmptyDictionary<int, string>.getEmptyList();
        AssertNotNull(e1, "EmptyDictionary not null");
        AssertEqual(0, e1.Count, "EmptyDictionary count=0");
        // 单例
        Assert(e1 == e2, "EmptyDictionary singleton");
    }

    // Simple assertion methods
    private static void Assert(bool condition, string message = "")
    {
        if (!condition)
        {
            throw new Exception($"Assertion failed: {message}");
        }
    }

    private static void AssertEqual<T>(T expected, T actual, string message = "")
    {
        bool eq = (expected == null && actual == null)
               || (expected != null && expected.Equals(actual));
        if (!eq)
        {
            throw new Exception(
                string.IsNullOrEmpty(message)
                    ? $"Expected [{expected}] but got [{actual}]"
                    : $"{message} - Expected [{expected}] but got [{actual}]");
        }
    }

    private static void AssertNotNull(object obj, string message = "")
    {
        if (obj == null)
        {
            throw new Exception(string.IsNullOrEmpty(message) ? "Object should not be null" : message);
        }
    }
}
#endif
