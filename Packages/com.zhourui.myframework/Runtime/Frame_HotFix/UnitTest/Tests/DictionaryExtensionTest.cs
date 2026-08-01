using System;
using System.Collections.Generic;
using static FrameUtility;
using static TestAssert;

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
        testFor();
        testAddNotNullKeyValue();
        testAddOrRemove();
        testSetAllValue();
        testTryGetValueKey();
        testRemoveIfConditional();
        testRemoveKeys();
        testRemoveFirstValue();
        testContainsKeyPredicate();
        testContainsPredicate2();
        testAddClass();
        testGetOrAddClass();
        testGetOrAddListPersist();
        testSet();
        testAddIfKvp();
        testAddKvp();
        testAddOrIncreaseFloat();
        testGetOrAddOut();
        testGetOrAddNewOut();
        testGetKeyOfValueOut();
        testRemoveByKeyPredicate();
        testRemoveByValuePredicate();
        testRemoveByKeyList();
        testRemoveMultiKeys();
        testCountByKeyPredicate();
        testCountByValuePredicate();
        testCountByPredicate2();
        testFirstByKeyPredicate();
        testFindKeyByValue();
        testFindKeyByPredicate2Out();
        testFindKeyByPredicate2();
        testFindKeyByValuePredicateOut();
        testFindKeyByKeyPredicate();
        testFindKeyByValuePredicate();
        testFindValueByValuePredicateOut();
        testFindValueByValuePredicate();
        testFindValueByKeyPredicate();
        testFindValueByPredicate2Out();
        testFindValueByPredicate2();
        testContainsValuePredicate();
        testFindKeyByKeyPredicateOut();
        testFindValueByKeyPredicateOut();
    }

    // ─── isEmpty / count ─────────────────────────────────────────────────
    private static void testIsEmptyAndCount()
    {
        Dictionary<int, string> nullDic = null;
        assert(nullDic.isEmpty(),        "null dic isEmpty=true");
        assertEqual(0, nullDic.count(),  "null dic count=0");

        var dic = new Dictionary<int, string>();
        assert(dic.isEmpty(),            "empty dic isEmpty=true");
        dic.Add(1, "a");
        assert(!dic.isEmpty(),           "non-empty isEmpty=false");
        assertEqual(1, dic.count(),      "count=1");
    }

    // ─── get ─────────────────────────────────────────────────────────────
    private static void testGetValue()
    {
        var dic = new Dictionary<int, string> { { 1, "one" }, { 2, "two" } };

        assertEqual("one", dic.get(1),      "get existing key");
        assertEqual("two", dic.get(2),      "get existing key 2");

        // 不存在的 key 返回 default
        assertEqual(null, dic.get(99),      "get non-existing key → default");

        // 带默认值的 get
        assertEqual("default", dic.get(99, "default"), "get with default value");

        // null 字典
        Dictionary<int, string> nullDic = null;
        assertEqual(null, nullDic.get(1),   "null dic get → default");
    }

    // ─── add / addOrSet ──────────────────────────────────────────────────
    private static void testAddAndAddOrSet()
    {
        var dic = new Dictionary<int, string>();

        // add
        dic.add(1, "first");
        assertEqual("first", dic[1], "add new key");

        // addOrSet (不存在时添加)
        dic.addOrSet(2, "second");
        assertEqual("second", dic[2], "addOrSet new key");

        // addOrSet (存在时替换)
        dic.addOrSet(1, "updated");
        assertEqual("updated", dic[1], "addOrSet existing key → replace");
    }

    // ─── addIf ───────────────────────────────────────────────────────────
    private static void testAddIf()
    {
        var dic = new Dictionary<int, string>();

        dic.addIf(1, "yes", true);
        dic.addIf(2, "no", false);

        assert(dic.ContainsKey(1), "addIf true → added");
        assert(!dic.ContainsKey(2), "addIf false → not added");
    }

    // ─── replace ─────────────────────────────────────────────────────────
    private static void testReplace()
    {
        var dic = new Dictionary<int, string> { { 1, "old" } };

        string old = dic.replace(1, "new");
        assertEqual("old", old, "replaced value updated");
        assertEqual("new", dic[1], "replace value updated");

        bool replaced = dic.replace(99, "new", out _);
        assert(replaced, "replace non-existing key → true");
        assertEqual("new", dic[99], "replace non-existing key should insert");
    }

    // ─── setRange / addRange ─────────────────────────────────────────────
    private static void testSetRangeAndAddRange()
    {
        var dic = new Dictionary<int, string> { { 1, "a" } };
        var src = new Dictionary<int, string> { { 2, "b" }, { 3, "c" } };

        // setRange (覆盖)
        dic.setRange(src);
        assertEqual(2, dic.count(), "setRange count=2");
        assertEqual("b", dic[2], "setRange key2=b");

        // addRange (不覆盖)
        dic.addRange(new Dictionary<int, string> { { 4, "d" } });
        assertEqual(3, dic.count(), "addRange count=3");
    }

    // ─── getKeyOfValue ───────────────────────────────────────────────────
    private static void testGetKeyOfValue()
    {
        var dic = new Dictionary<int, string> { { 1, "a" }, { 2, "b" }, { 3, "a" } };

        int key = dic.getKeyOfValue("a");
        assert(key == 1 || key == 3, "getKeyOfValue returns first match");

        int notFound = dic.getKeyOfValue("xyz");
        assertEqual(0, notFound, "getKeyOfValue not found → default");
    }

    // ─── getOrAdd ────────────────────────────────────────────────────────
    private static void testGetOrAdd()
    {
        var dic = new Dictionary<int, string>();

		// 不存在时添加
		string val1 = dic.getOrAdd(1, "default");
        assertEqual("default", val1, "getOrAdd new key → default value");
        assertEqual("default", dic[1], "getOrAdd added to dict");

        // 已存在时返回现有值
        dic[1] = "updated";
        string val2 = dic.getOrAdd(1, "newDefault");
        assertEqual("updated", val2, "getOrAdd existing key → current value");
    }

    // ─── getOrAddNew ─────────────────────────────────────────────────────
    private static void testGetOrAddNew()
    {
        var dic = new Dictionary<int, List<string>>();

        List<string> list = dic.getOrAddNew(1);
        assertNotNull(list, "getOrAddNew returns new list");
        assertEqual(0, list.Count, "getOrAddNew list empty");

        list.Add("item");
        List<string> sameList = dic.getOrAddNew(1);
        assertEqual(1, sameList.Count, "getOrAddNew existing key → same list");
    }

    // ─── addOrIncreaseValue ──────────────────────────────────────────────
    private static void testAddOrIncreaseValue()
    {
        var dic = new Dictionary<string, int>();

        dic.addOrIncreaseValue("a", 5);
        assertEqual(5, dic["a"], "addOrIncreaseValue new key");

        dic.addOrIncreaseValue("a", 3);
        assertEqual(8, dic["a"], "addOrIncreaseValue existing key → sum");
    }

    // ─── remove ──────────────────────────────────────────────────────────
    private static void testRemove()
    {
        var dic = new Dictionary<int, string> { { 1, "a" }, { 2, "b" } };
        dic.remove(1, 2);
        assert(!dic.ContainsKey(1), "remove key removed");
    }

    // ─── removeIf ────────────────────────────────────────────────────────
    private static void testRemoveIf()
    {
        var dic = new Dictionary<int, string> { { 1, "a" }, { 2, "b" }, { 3, "c" } };

        int removed = dic.remove((k, v) => k % 2 == 0);
        assertEqual(1, removed, "removeIf removed 1 item");
        assert(!dic.ContainsKey(2), "removeIf removed even key");
        assertEqual(2, dic.count(), "removeIf count=2");
    }

    // ─── find ────────────────────────────────────────────────────────────
    private static void testFind()
    {
        var dic = new Dictionary<int, string> { { 1, "a" }, { 2, "b" }, { 3, "c" } };

        // find by predicate → KeyValuePair
        dic.find((k, v) => v == "b", out var kv);
        assert(kv.Key == 2, "find by value key=2");

        // findKey
        dic.findKey(k => k == 3, out int key);
        assertEqual(3, key, "findKey value=c → key=3");

        // findValue
        string val = dic.findValue(k => k == 1);
        assertEqual("a", val, "findValue key=1 → value=a");
    }

    // ─── containsKey / containsValue ─────────────────────────────────────
    private static void testContains()
    {
        var dic = new Dictionary<int, string> { { 1, "a" } };
        assert(dic.containsValue("a"), "containsValue existing");
        assert(!dic.containsValue("xyz"), "containsValue non-existing");
    }

    // ─── first / firstKey / firstValue ───────────────────────────────────
    private static void testFirst()
    {
        var dic = new Dictionary<int, string> { { 1, "a" }, { 2, "b" } };

        var first = dic.first();
        assert(first.Key == 1 || first.Key == 2, "first returns some item");

        int firstKey = dic.firstKey();
        assert(firstKey == 1 || firstKey == 2, "firstKey returns some key");

        string firstValue = dic.firstValue();
        assert(firstValue == "a" || firstValue == "b", "firstValue returns some value");

        // 空字典
        var empty = new Dictionary<int, string>();
        var emptyFirst = empty.first();
        assertEqual(0, emptyFirst.Key, "first empty → default");
        assertEqual(null, emptyFirst.Value, "first empty → default value");
    }

    // ─── ForKey / ForValue ───────────────────────────────────────────────
    private static void testForKeyAndForValue()
    {
        var dic = new Dictionary<int, string> { { 1, "a" }, { 2, "b" } };

        int keySum = 0;
        dic.forKey(k => keySum += k);
        assertEqual(3, keySum, "ForKey sum keys=3");

        string valueConcat = "";
        dic.forValue(v => valueConcat += v);
        assert(valueConcat.Contains("a") && valueConcat.Contains("b"), "ForValue concatenates values");
    }

    // ─── safe ────────────────────────────────────────────────────────────
    private static void testSafe()
    {
        Dictionary<int, string> nullDic = null;
        var safe = nullDic.safe();
        assertNotNull(safe, "safe null → non-null");
        assertEqual(0, safe.count(), "safe null → empty");

        var dic = new Dictionary<int, string> { { 1, "a" } };
        var safe2 = dic.safe();
        assertEqual(1, safe2.count(), "safe non-null → same");
    }

    // ─── EmptyDictionary ─────────────────────────────────────────────────
    private static void testEmptyDictionary()
    {
        var e1 = EmptyDictionary<int, string>.getEmptyList();
        var e2 = EmptyDictionary<int, string>.getEmptyList();
        assertNotNull(e1, "EmptyDictionary not null");
        assertEqual(0, e1.Count, "EmptyDictionary count=0");
        // 单例
        assert(e1 == e2, "EmptyDictionary singleton");
    }

    // ─── For ─────────────────────────────────────────────────────────────
    private static void testFor()
    {
        var dic = new Dictionary<int, string> { { 1, "a" }, { 2, "b" } };
        int keySum = 0;
        string valConcat = "";
        dic.For(kv => { keySum += kv.Key; valConcat += kv.Value; });
        assertEqual(3, keySum, "For key sum=3");
        assertEqual("ab", valConcat, "For value concat=ab");

        // null 不崩溃
        Dictionary<int, string> nullDic = null;
        nullDic.For(kv => { }); // no throw
    }

    // ─── addNotNullKey / addNotNullValue ─────────────────────────────────
    private static void testAddNotNullKeyValue()
    {
        var dic = new Dictionary<string, int>();

        // addNotNullKey
        dic.addNotNullKey("valid", 1);
        assertEqual(1, dic.Count, "addNotNullKey valid → added");

        dic.addNotNullKey(null, 2);
        assertEqual(1, dic.Count, "addNotNullKey null → not added");

        // addNotNullValue
        var dic2 = new Dictionary<int, string>();
        dic2.addNotNullValue(1, "valid");
        assertEqual(1, dic2.Count, "addNotNullValue valid → added");

        dic2.addNotNullValue(2, null);
        assertEqual(1, dic2.Count, "addNotNullValue null → not added");
    }

    // ─── addOrRemove ─────────────────────────────────────────────────────
    private static void testAddOrRemove()
    {
        var dic = new Dictionary<int, string>();
        dic.addOrRemove(1, "a", true);
        assert(dic.ContainsKey(1), "addOrRemove add → contains");

        dic.addOrRemove(1, "a", false);
        assert(!dic.ContainsKey(1), "addOrRemove remove → gone");
    }

    // ─── setAllValue ─────────────────────────────────────────────────────
    private static void testSetAllValue()
    {
        var dic = new Dictionary<int, string> { { 1, "a" }, { 2, "b" }, { 3, "c" } };
        dic.setAllValue("x");
        assertEqual("x", dic[1], "setAllValue[1]=x");
        assertEqual("x", dic[2], "setAllValue[2]=x");
        assertEqual("x", dic[3], "setAllValue[3]=x");
    }

    // ─── tryGetValueKey ──────────────────────────────────────────────────
    private static void testTryGetValueKey()
    {
        var dic = new Dictionary<int, string> { { 1, "a" }, { 2, "b" }, { 3, "a" } };
        int key = dic.tryGetValueKey("b", -1);
        assertEqual(2, key, "tryGetValueKey found=2");

        int notFound = dic.tryGetValueKey("xyz", -1);
        assertEqual(-1, notFound, "tryGetValueKey not found → default");
    }

    // ─── removeIf (conditional) ──────────────────────────────────────────
    private static void testRemoveIfConditional()
    {
        var dic = new Dictionary<int, string> { { 1, "a" }, { 2, "b" } };
        assert(dic.removeIf(1, true), "removeIf condition true → removed");
        assert(!dic.ContainsKey(1), "removeIf key gone");

        assert(!dic.removeIf(2, false), "removeIf condition false → not removed");
        assert(dic.ContainsKey(2), "removeIf key still there");
    }

    // ─── removeKeys ──────────────────────────────────────────────────────
    private static void testRemoveKeys()
    {
        var dic = new Dictionary<int, string> { { 1, "a" }, { 2, "b" }, { 3, "c" } };
        var other = new Dictionary<int, int> { { 2, 100 }, { 4, 200 } };
        dic.removeKeys(other);
        assertEqual(2, dic.Count, "removeKeys count=2");
        assert(!dic.ContainsKey(2), "removeKeys removed key 2");
        assert(dic.ContainsKey(1), "removeKeys kept key 1");
    }

    // ─── removeFirstValue ────────────────────────────────────────────────
    private static void testRemoveFirstValue()
    {
        var dic = new Dictionary<int, string> { { 1, "a" }, { 2, "b" }, { 3, "a" } };
        dic.removeFirstValue("a");
        assertEqual(2, dic.Count, "removeFirstValue count=2");
        // 第一个匹配 "a" 被删除 (key 1 或 key 3)
    }

    // ─── containsKey (predicate) ─────────────────────────────────────────
    private static void testContainsKeyPredicate()
    {
        var dic = new Dictionary<int, string> { { 1, "a" }, { 2, "b" }, { 3, "c" } };
        assert(dic.containsKey(k => k > 2), "containsKey pred k>2 → true");
        assert(!dic.containsKey(k => k > 10), "containsKey pred k>10 → false");
    }

    // ─── contains (Predicate2) ───────────────────────────────────────────
    private static void testContainsPredicate2()
    {
        var dic = new Dictionary<int, string> { { 1, "a" }, { 2, "b" }, { 3, "c" } };
        assert(dic.contains((k, v) => v == "b"), "contains Predicate2 b → true");
        assert(!dic.contains((k, v) => v == "xyz"), "contains Predicate2 xyz → false");
    }

    // ─── addClass ────────────────────────────────────────────────────────
    private static void testAddClass()
    {
        var dic = new Dictionary<int, TestDictClass>();
        TestDictClass obj = dic.addClass(1);
        assertNotNull(obj, "addClass not null");
        assertEqual(1, dic.Count, "addClass count=1");
        assert(dic.ContainsKey(1), "addClass contains key");
    }

    // ─── getOrAddClass (两个重载) ────────────────────────────────────────
    private static void testGetOrAddClass()
    {
        var dic = new Dictionary<int, TestDictClass>();

        // 重载1: 不存在时创建
        TestDictClass obj1 = dic.getOrAddClass(1);
        assertNotNull(obj1, "getOrAddClass new not null");
        assert(dic.ContainsKey(1), "getOrAddClass new → added");

        // 重载1: 已存在时返回
        TestDictClass obj2 = dic.getOrAddClass(1);
        assert(obj1 == obj2, "getOrAddClass existing → same ref");

        // 重载2: out 参数版, 不存在时创建
        var dic2 = new Dictionary<int, TestDictClass>();
        bool existed = dic2.getOrAddClass(2, out TestDictClass obj3);
        assert(!existed, "getOrAddClass out new → false");
        assertNotNull(obj3, "getOrAddClass out not null");

        // 重载2: 已存在时返回
        existed = dic2.getOrAddClass(2, out TestDictClass obj4);
        assert(existed, "getOrAddClass out existing → true");
        assert(obj3 == obj4, "getOrAddClass out → same ref");
    }

    // ─── getOrAddListPersist (3个重载: Dictionary/List/HashSet) ──────────
    private static void testGetOrAddListPersist()
    {
        // 重载1: Dictionary<T0, T1> 版本
        var dicMap = new Dictionary<int, Dictionary<int, string>>();
        var innerDic = dicMap.getOrAddListPersist(1);
        assertNotNull(innerDic, "getOrAddListPersist dic not null");
        assert(dicMap.ContainsKey(1), "getOrAddListPersist dic → added");

        // 重载2: List<T> 版本
        var listMap = new Dictionary<int, List<int>>();
        var innerList = listMap.getOrAddListPersist(1);
        assertNotNull(innerList, "getOrAddListPersist list not null");
        assertEqual(0, innerList.Count, "getOrAddListPersist list empty");

        // 重载3: HashSet<T> 版本
        var setMap = new Dictionary<int, HashSet<int>>();
        var innerSet = setMap.getOrAddListPersist(1);
        assertNotNull(innerSet, "getOrAddListPersist set not null");
        assertEqual(0, innerSet.Count, "getOrAddListPersist set empty");
    }

    // ─── set ──────────────────────────────────────────────────────────────
    private static void testSet()
    {
        var dic = new Dictionary<int, string> { { 1, "a" } };
        dic.set(1, "updated");
        assertEqual("updated", dic[1], "set existing key → updated");
    }

    // ─── addIf(KeyValuePair) ──────────────────────────────────────────────
    private static void testAddIfKvp()
    {
        var dic = new Dictionary<int, string>();
        dic.addIf(new KeyValuePair<int, string>(1, "yes"), true);
        assert(dic.ContainsKey(1), "addIf kvp true → added");

        dic.addIf(new KeyValuePair<int, string>(2, "no"), false);
        assert(!dic.ContainsKey(2), "addIf kvp false → not added");
    }

    // ─── add(KeyValuePair) ────────────────────────────────────────────────
    private static void testAddKvp()
    {
        var dic = new Dictionary<int, string>();
        dic.add(new KeyValuePair<int, string>(1, "hello"));
        assertEqual("hello", dic[1], "add kvp value");
    }

    // ─── addOrIncreaseValue(float) ────────────────────────────────────────
    private static void testAddOrIncreaseFloat()
    {
        var dic = new Dictionary<string, float>();
        dic.addOrIncreaseValue("a", 5.5f);
        assert(dic["a"].isEqual(5.5f, 0.001f), "addOrIncrease float new");

        dic.addOrIncreaseValue("a", 3.2f);
        assert(dic["a"].isEqual(8.7f, 0.001f), "addOrIncrease float existing → sum");
    }

    // ─── getOrAdd(out) ────────────────────────────────────────────────────
    // 注意: key 不存在时 TryGetValue 将 out existValue 设为 default(TValue),
    // 然后才 Add(value), 所以 out val 在新增时是 default 而非传入的 value。
    private static void testGetOrAddOut()
    {
        var dic = new Dictionary<int, string>();
        bool existed = dic.getOrAdd(1, "default", out string val);
        assert(!existed, "getOrAdd out new → false");
        assertNull(val, "getOrAdd out new value → null (TryGetValue default)");

        existed = dic.getOrAdd(1, "newDefault", out string val2);
        assert(existed, "getOrAdd out existing → true");
        assertEqual("default", val2, "getOrAdd out existing value unchanged");
    }

    // ─── getOrAddNew(out) ─────────────────────────────────────────────────
    private static void testGetOrAddNewOut()
    {
        var dic = new Dictionary<int, List<string>>();
        bool existed = dic.getOrAddNew(1, out List<string> list);
        assert(!existed, "getOrAddNew out new → false");
        assertNotNull(list, "getOrAddNew out not null");

        existed = dic.getOrAddNew(1, out List<string> sameList);
        assert(existed, "getOrAddNew out existing → true");
        assert(list == sameList, "getOrAddNew out same ref");
    }

    // ─── getKeyOfValue(out) ───────────────────────────────────────────────
    private static void testGetKeyOfValueOut()
    {
        var dic = new Dictionary<int, string> { { 1, "a" }, { 2, "b" }, { 3, "a" } };
        bool found = dic.getKeyOfValue("a", out int key);
        assert(found, "getKeyOfValue out found");
        assert(key == 1 || key == 3, "getKeyOfValue out first match");

        found = dic.getKeyOfValue("xyz", out int notFound);
        assert(!found, "getKeyOfValue out not found");
    }

    // ─── remove(Predicate<Key>) ───────────────────────────────────────────
    private static void testRemoveByKeyPredicate()
    {
        var dic = new Dictionary<int, string> { { 1, "a" }, { 2, "b" }, { 3, "c" } };
        int removed = dic.remove((Predicate<int>)(k => k % 2 == 0));
        assertEqual(1, removed, "remove(Key pred) removed=1");
        assert(!dic.ContainsKey(2), "remove(Key pred) key 2 gone");
        assertEqual(2, dic.count(), "remove(Key pred) count=2");
    }

    // ─── remove(Predicate<Value>) ─────────────────────────────────────────
    private static void testRemoveByValuePredicate()
    {
        var dic = new Dictionary<int, string> { { 1, "aa" }, { 2, "bb" }, { 3, "ac" } };
        int removed = dic.remove((Predicate<string>)(v => v.StartsWith("a")));
        assertEqual(2, removed, "remove(Value pred) removed=2");
        assertEqual(1, dic.count(), "remove(Value pred) count=1");
    }

    // ─── remove(List<Key>) ────────────────────────────────────────────────
    private static void testRemoveByKeyList()
    {
        var dic = new Dictionary<int, string> { { 1, "a" }, { 2, "b" }, { 3, "c" } };
        var keys = new List<int> { 1, 3 };
        dic.remove(keys);
        assertEqual(1, dic.count(), "remove(List<Key>) count=1");
        assert(dic.ContainsKey(2), "remove(List<Key>) key 2 kept");
    }

    // ─── remove(Key, Key, Key) ────────────────────────────────────────────
    private static void testRemoveMultiKeys()
    {
        var dic = new Dictionary<int, string> { { 1, "a" }, { 2, "b" }, { 3, "c" }, { 4, "d" }, { 5, "e" } };
        dic.remove(2, 4);
        assertEqual(3, dic.count(), "remove 2 keys count=3");
        assert(!dic.ContainsKey(2), "remove 2 keys key2 gone");
        assert(!dic.ContainsKey(4), "remove 2 keys key4 gone");

        var dic2 = new Dictionary<int, string> { { 1, "a" }, { 2, "b" }, { 3, "c" }, { 4, "d" }, { 5, "e" } };
        dic2.remove(1, 2, 3);
        assertEqual(2, dic2.count(), "remove 3 keys count=2");

        var dic3 = new Dictionary<int, string> { { 1, "a" }, { 2, "b" }, { 3, "c" }, { 4, "d" }, { 5, "e" } };
        dic3.remove(1, 2, 3, 4);
        assertEqual(1, dic3.count(), "remove 4 keys count=1");

        var dic4 = new Dictionary<int, string> { { 1, "a" }, { 2, "b" }, { 3, "c" }, { 4, "d" }, { 5, "e" } };
        dic4.remove(1, 2, 3, 4, 5);
        assertEqual(0, dic4.count(), "remove 5 keys count=0");
    }

    // ─── count(Predicate<Key>) ────────────────────────────────────────────
    private static void testCountByKeyPredicate()
    {
        var dic = new Dictionary<int, string> { { 1, "a" }, { 2, "b" }, { 3, "c" }, { 4, "d" } };
        int even = dic.count((Predicate<int>)(k => k % 2 == 0));
        assertEqual(2, even, "count(Key pred) even=2");
    }

    // ─── count(Predicate<Value>) ──────────────────────────────────────────
    private static void testCountByValuePredicate()
    {
        var dic = new Dictionary<int, string> { { 1, "aa" }, { 2, "bb" }, { 3, "ac" } };
        int aStart = dic.count((Predicate<string>)(v => v.StartsWith("a")));
        assertEqual(2, aStart, "count(Value pred)=2");
    }

    // ─── count(Predicate2) ────────────────────────────────────────────────
    private static void testCountByPredicate2()
    {
        var dic = new Dictionary<int, string> { { 1, "a" }, { 2, "b" }, { 3, "c" } };
        int both = dic.count((k, v) => k > 1 && v != "b");
        assertEqual(1, both, "count(Predicate2)=1"); // key=3, value=c
    }

    // ─── first(Predicate<Key>) ────────────────────────────────────────────
    private static void testFirstByKeyPredicate()
    {
        var dic = new Dictionary<int, string> { { 1, "a" }, { 2, "b" }, { 3, "c" } };
        var kv = dic.first((Predicate<int>)(k => k > 1));
        assert(kv.Key == 2 || kv.Key == 3, "first(Key pred) key found");

        var empty = dic.first((Predicate<int>)(k => k > 100));
        assertEqual(0, empty.Key, "first(Key pred) not found → default");
    }

    // ─── findKey(value, out Key) ──────────────────────────────────────────
    private static void testFindKeyByValue()
    {
        var dic = new Dictionary<int, string> { { 1, "a" }, { 2, "b" }, { 3, "a" } };
        bool found = dic.findKey("b", out int key);
        assert(found, "findKey by value found");
        assertEqual(2, key, "findKey by value key=2");

        found = dic.findKey("xyz", out int notFound);
        assert(!found, "findKey by value not found");
    }

    // ─── findKey(Predicate2, out Key) ─────────────────────────────────────
    private static void testFindKeyByPredicate2Out()
    {
        var dic = new Dictionary<int, string> { { 1, "a" }, { 2, "b" }, { 3, "c" } };
        bool found = dic.findKey((k, v) => v == "c", out int key);
        assert(found, "findKey Predicate2 out found");
        assertEqual(3, key, "findKey Predicate2 out key=3");
    }

    // ─── findKey(Predicate2) ──────────────────────────────────────────────
    private static void testFindKeyByPredicate2()
    {
        var dic = new Dictionary<int, string> { { 1, "a" }, { 2, "b" }, { 3, "c" } };
        int key = dic.findKey((k, v) => v == "b");
        assertEqual(2, key, "findKey Predicate2 key=2");

        int notFound = dic.findKey((k, v) => v == "xyz");
        assertEqual(0, notFound, "findKey Predicate2 not found=default");
    }

    // ─── findKey(Predicate<Value>, out Key) ───────────────────────────────
    private static void testFindKeyByValuePredicateOut()
    {
        var dic = new Dictionary<int, string> { { 1, "aa" }, { 2, "bb" }, { 3, "ac" } };
        bool found = dic.findKey((Predicate<string>)(v => v.StartsWith("a")), out int key);
        assert(found, "findKey Value pred out found");
        assert(key == 1 || key == 3, "findKey Value pred out first match");
    }

    // ─── findKey(Predicate<Key>) ──────────────────────────────────────────
    private static void testFindKeyByKeyPredicate()
    {
        var dic = new Dictionary<int, string> { { 1, "a" }, { 2, "b" }, { 3, "c" } };
        int key = dic.findKey((Predicate<int>)(k => k > 1));
        assert(key == 2 || key == 3, "findKey Key pred found");

        int notFound = dic.findKey((Predicate<int>)(k => k > 100));
        assertEqual(0, notFound, "findKey Key pred not found=default");
    }

    // ─── findKey(Predicate<Value>) ────────────────────────────────────────
    private static void testFindKeyByValuePredicate()
    {
        var dic = new Dictionary<int, string> { { 1, "aa" }, { 2, "bb" }, { 3, "ac" } };
        int key = dic.findKey((Predicate<string>)(v => v.StartsWith("b")));
        assertEqual(2, key, "findKey Value pred key=2");
    }

    // ─── findValue(Predicate<Value>, out Value) ───────────────────────────
    private static void testFindValueByValuePredicateOut()
    {
        var dic = new Dictionary<int, string> { { 1, "aa" }, { 2, "bb" }, { 3, "ac" } };
        bool found = dic.findValue((Predicate<string>)(v => v.StartsWith("b")), out string val);
        assert(found, "findValue Value pred out found");
        assertEqual("bb", val, "findValue Value pred out val=bb");
    }

    // ─── findValue(Predicate<Value>) ──────────────────────────────────────
    private static void testFindValueByValuePredicate()
    {
        var dic = new Dictionary<int, string> { { 1, "aa" }, { 2, "bb" }, { 3, "ac" } };
        string val = dic.findValue((Predicate<string>)(v => v.StartsWith("a")));
        assert(val == "aa" || val == "ac", "findValue Value pred found");
    }

    // ─── findValue(Predicate<Key>) ────────────────────────────────────────
    private static void testFindValueByKeyPredicate()
    {
        var dic = new Dictionary<int, string> { { 1, "a" }, { 2, "b" }, { 3, "c" } };
        string val = dic.findValue((Predicate<int>)(k => k == 2));
        assertEqual("b", val, "findValue Key pred val=b");
    }

    // ─── findValue(Predicate2, out Value) ─────────────────────────────────
    private static void testFindValueByPredicate2Out()
    {
        var dic = new Dictionary<int, string> { { 1, "a" }, { 2, "b" }, { 3, "c" } };
        bool found = dic.findValue((k, v) => k > 1 && v != "b", out string val);
        assert(found, "findValue Predicate2 out found");
        assertEqual("c", val, "findValue Predicate2 out val=c");
    }

    // ─── findValue(Predicate2) ────────────────────────────────────────────
    private static void testFindValueByPredicate2()
    {
        var dic = new Dictionary<int, string> { { 1, "a" }, { 2, "b" }, { 3, "c" } };
        string val = dic.findValue((k, v) => k == 3);
        assertEqual("c", val, "findValue Predicate2 val=c");
    }

    // ─── containsValue(Predicate) ─────────────────────────────────────────
    private static void testContainsValuePredicate()
    {
        var dic = new Dictionary<int, string> { { 1, "aa" }, { 2, "bb" }, { 3, "cc" } };
        assert(dic.containsValue((Predicate<string>)(v => v.StartsWith("b"))), "containsValue pred b → true");
        assert(!dic.containsValue((Predicate<string>)(v => v.StartsWith("x"))), "containsValue pred x → false");
    }

    // ─── findKey(Predicate<Key>, out Key) ────────────────────────────────
    private static void testFindKeyByKeyPredicateOut()
    {
        var dic = new Dictionary<int, string> { { 1, "a" }, { 2, "b" }, { 3, "c" } };
        bool found = dic.findKey((Predicate<int>)(k => k == 2), out int key);
        assert(found, "findKey Key pred out found");
        assertEqual(2, key, "findKey Key pred out key=2");

        // 未找到
        found = dic.findKey((Predicate<int>)(k => k > 100), out int notFound);
        assert(!found, "findKey Key pred out not found");

        // 空字典
        var empty = new Dictionary<int, string>();
        found = empty.findKey((Predicate<int>)(k => true), out int emptyKey);
        assert(!found, "findKey Key pred out empty → false");
    }

    // ─── findValue(Predicate<Key>, out Value) ────────────────────────────
    private static void testFindValueByKeyPredicateOut()
    {
        var dic = new Dictionary<int, string> { { 1, "a" }, { 2, "b" }, { 3, "c" } };
        bool found = dic.findValue((Predicate<int>)(k => k == 2), out string val);
        assert(found, "findValue Key pred out found");
        assertEqual("b", val, "findValue Key pred out val=b");

        // 未找到
        found = dic.findValue((Predicate<int>)(k => k > 100), out string notFound);
        assert(!found, "findValue Key pred out not found");

        // 空字典
        var empty = new Dictionary<int, string>();
        found = empty.findValue((Predicate<int>)(k => true), out string emptyVal);
        assert(!found, "findValue Key pred out empty → false");
    }

    public class TestDictClass : ClassObject { }
}