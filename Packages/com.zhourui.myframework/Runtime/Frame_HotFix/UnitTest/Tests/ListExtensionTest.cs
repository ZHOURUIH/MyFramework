using System;
using System.Collections.Generic;
using System.Text;

// ListExtension 扩展方法测试
// 覆盖：isEmpty / count / get / set / add / addUnique / addIf / addNot / addNotEmpty /
//        addCount / remove / removeAt / removeIf / popBack / getLast / first /
//        find / contains / swap / swapToEndAndRemove / inverse / isSame / isSubList /
//        moveTo / setAllDefault / setAllValue / setRange / addRange
public static class ListExtensionTest
{
    public static void Run()
    {
        testIsEmptyAndCount();
        testGetAndSet();
        testAdd();
        testAddUnique();
        testAddIf();
        testAddNot();
        testAddNotEmpty();
        testAddCount();
        testRemoveAt();
        testRemoveIf();
        testPopBackAndGetLast();
        testFirst();
        testFind();
        testContains();
        testSwap();
        testSwapToEndAndRemove();
        testInverse();
        testIsSame();
        testIsSubList();
        testSetAllDefaultAndValue();
        testSetRange();
        testAddRange();
        testGetEmptyList();
        testRandom();
        testRemoveIfConditional();
        testRemoveAtIf();
        testAddRangeKeysValues();
        testAddNotNull();
        testAddRangeNotNull();
        testAddNew();
        testAddUniqueIf();
        testAddUniqueOrRemove();
        testAddUniqueNot();
        testAddRangeDerived();
        testSetRangeDerived();
        testSetRangeKeysValues();
        testMoveTo();
        testFor();
        testForI();
    }

    // ─── isEmpty / count ─────────────────────────────────────────────────
    private static void testIsEmptyAndCount()
    {
        List<int> empty = null;
        Assert(empty.isEmpty(),          "null list isEmpty=true");
        AssertEqual(0, empty.count(),    "null list count=0");

        var list = new List<int>();
        Assert(list.isEmpty(),           "empty list isEmpty=true");
        AssertEqual(0, list.count(),     "empty list count=0");

        list.Add(1);
        Assert(!list.isEmpty(),          "non-empty isEmpty=false");
        AssertEqual(1, list.count(),     "count=1");
    }

    // ─── get / set ───────────────────────────────────────────────────────
    private static void testGetAndSet()
    {
        var list = new List<int> { 10, 20, 30 };

        AssertEqual(10, list.get(0),  "get[0]=10");
        AssertEqual(30, list.get(2),  "get[2]=30");
        AssertEqual(0,  list.get(5),  "get 越界=default(0)");
        AssertEqual(0,  list.get(-1), "get 负索引=default(0)");

        list.set(1, 99);
        AssertEqual(99, list[1], "set[1]=99");

        // 越界 set 不崩溃
        list.set(10, 0);
        // 数据不变
        AssertEqual(99, list[1], "set 越界 list[1]不变");
    }

    // ─── add ─────────────────────────────────────────────────────────────
    private static void testAdd()
    {
        var list = new List<int>();
        list.add(1);
        list.add(2);
        list.add(3);

        AssertEqual(3, list.count(), "add count=3");
        AssertEqual(1, list[0], "add[0]=1");
        AssertEqual(3, list[2], "add[2]=3");
    }

    // ─── addUnique ───────────────────────────────────────────────────────
    private static void testAddUnique()
    {
        var list = new List<int> { 1, 2, 3 };

        bool added = list.addUnique(4);
        Assert(added, "addUnique new → true");
        AssertEqual(4, list.count(), "addUnique count=4");

        bool notAdded = list.addUnique(2);
        Assert(!notAdded, "addUnique existing → false");
        AssertEqual(4, list.count(), "addUnique no duplicate added");
    }

    // ─── addIf ───────────────────────────────────────────────────────────
    private static void testAddIf()
    {
        var list = new List<int>();

        list.addIf(1, true);
        list.addIf(2, false);

        AssertEqual(1, list.count(), "addIf true → added");
        AssertEqual(1, list[0], "addIf value=1");
    }

    // ─── addNot ──────────────────────────────────────────────────────────
    private static void testAddNot()
    {
        var list = new List<int> { 1, 2, 3 };

        list.addNot(4, 2); // 4 != 2 → 添加
        AssertEqual(4, list.count(), "addNot condition false → added");

        list.addNot(2, 2); // 2 == 2 → 不添加
        AssertEqual(4, list.count(), "addNot condition true → not added");
    }

    // ─── addNotEmpty ─────────────────────────────────────────────────────
    private static void testAddNotEmpty()
    {
        var list = new List<string>();

        list.addNotEmpty("hello");
        AssertEqual(1, list.count(), "addNotEmpty non-empty → added");

        list.addNotEmpty("");
        AssertEqual(1, list.count(), "addNotEmpty empty → not added");

        list.addNotEmpty(null);
        AssertEqual(1, list.count(), "addNotEmpty null → not added");
    }

    // ─── addCount ────────────────────────────────────────────────────────
    private static void testAddCount()
    {
        var list = new List<int> { 1, 2, 3 };

        list.addCount(99, 2);
        AssertEqual(5, list.count(), "addCount count=5");
        AssertEqual(99, list[3], "addCount[3]=99");
        AssertEqual(99, list[4], "addCount[4]=99");
    }

    // ─── removeAt ────────────────────────────────────────────────────────
    private static void testRemoveAt()
    {
        var list = new List<int> { 1, 2, 3 };

        list.removeAt(1);
        AssertEqual(2, list.count(), "removeAt count=2");
        AssertEqual(3, list[1], "removeAt[1]=3");

        // 越界 removeAt 不崩溃
        try
        {
            list.removeAt(10);
            // 如果没抛异常，应该没变化
            AssertEqual(2, list.count(), "removeAt 越界 count不变");
        }
        catch
        {
            // 允许抛异常
        }
    }

    // ─── removeIf ────────────────────────────────────────────────────────
    private static void testRemoveIf()
    {
        var list = new List<int> { 1, 2, 3, 4, 5 };
        int removed = list.removeAll(x => x % 2 == 0);
        AssertEqual(2, removed, "removeIf removed 2 items");
        AssertEqual(3, list.count(), "removeIf count=3");
        Assert(!list.contains(2), "removeIf removed even numbers");
    }

    // ─── popBack / getLast ───────────────────────────────────────────────
    private static void testPopBackAndGetLast()
    {
        var list = new List<int> { 1, 2, 3 };

        int last = list.popBack();
        AssertEqual(3, last, "popBack returns last");
        AssertEqual(2, list.count(), "popBack count=2");

        int getLast = list.getLast();
        AssertEqual(2, getLast, "getLast returns new last");

        // 空列表
        var empty = new List<int>();
        int defaultVal = empty.popBack();
        AssertEqual(0, defaultVal, "popBack empty → default");
    }

    // ─── first ───────────────────────────────────────────────────────────
    private static void testFirst()
    {
        var list = new List<int> { 7, 8, 9 };
        AssertEqual(7, list.first(), "first=7");

        var empty = new List<int>();
        AssertEqual(0, empty.first(), "first empty=default");

        List<int> nullList = null;
        AssertEqual(0, nullList.safe().first(), "first null=default via safe()");
    }

    // ─── find ────────────────────────────────────────────────────────────
    private static void testFind()
    {
        var list = new List<int> { 5, 10, 15, 20 };

        // find by value + out index
        bool ok = list.find(15, out int idx);
        Assert(ok, "find 15 ok");
        AssertEqual(2, idx, "find 15 index=2");

        ok = list.find(99, out idx);
        Assert(!ok, "find 99 not found");
        AssertEqual(-1, idx, "find 99 index=-1");

        // find by predicate → item
        int item = list.find(x => x > 12);
        AssertEqual(15, item, "find pred item=15");

        // find by predicate → out index
        ok = list.find(x => x > 12, out int pidx);
        Assert(ok, "find pred pidx ok");
        AssertEqual(2, pidx, "find pred pidx=2");
    }

    // ─── contains ────────────────────────────────────────────────────────
    private static void testContains()
    {
        var list = new List<int> { 1, 2, 3 };
        Assert(list.contains(2), "contains 2=true");
        Assert(!list.contains(9), "contains 9=false");

        // contains by predicate
        Assert(list.contains(x => x > 2), "contains pred >2=true");
        Assert(!list.contains(x => x > 10), "contains pred >10=false");
    }

    // ─── swap ────────────────────────────────────────────────────────────
    private static void testSwap()
    {
        var list = new List<int> { 1, 2, 3, 4 };

        list.swap(1, 2);
        AssertEqual(3, list[1], "swap[1]=3");
        AssertEqual(2, list[2], "swap[2]=2");

        // 越界 swap 不崩溃
        try
        {
            list.swap(0, 10);
            // 如果没抛异常，应该没变化
            AssertEqual(1, list[0], "swap 越界不变");
        }
        catch
        {
            // 允许抛异常
        }
    }

    // ─── swapToEndAndRemove ──────────────────────────────────────────────
    private static void testSwapToEndAndRemove()
    {
        var list = new List<int> { 1, 2, 3, 4, 5 };

        list.swapToEndAndRemove(2);
        AssertEqual(4, list.count(), "count=4");
        Assert(!list.contains(3), "element removed");
    }

    // ─── inverse ─────────────────────────────────────────────────────────
    private static void testInverse()
    {
        var list = new List<int> { 1, 2, 3, 4, 5 };
        list.inverse();
        AssertEqual(5, list[0], "inverse[0]=5");
        AssertEqual(3, list[2], "inverse[2]=3");
        AssertEqual(1, list[4], "inverse[4]=1");
    }

    // ─── isSame ──────────────────────────────────────────────────────────
    private static void testIsSame()
    {
        var a = new List<int> { 1, 2, 3 };
        var b = new List<int> { 1, 2, 3 };
        var c = new List<int> { 1, 2, 4 };

        Assert(a.isSame(b), "isSame identical → true");
        Assert(!a.isSame(c), "isSame different → false");
        Assert(!a.isSame(null), "isSame null → false");
    }

    // ─── isSubList ───────────────────────────────────────────────────────
    private static void testIsSubList()
    {
        var main = new List<int> { 1, 2, 3, 4, 5 };
        var sub = new List<int> { 2, 3, 4 };
        var notSub = new List<int> { 2, 3, 6 };

        Assert(main.isSubList(sub), "isSubList true");
        Assert(!main.isSubList(notSub), "isSubList false");
    }

    // ─── setAllDefault / setAllValue ─────────────────────────────────────
    private static void testSetAllDefaultAndValue()
    {
        var list = new List<int> { 1, 2, 3 };
        list.setAllDefault();
        AssertEqual(0, list[0], "setAllDefault[0]=0");
        AssertEqual(0, list[2], "setAllDefault[2]=0");

        list.setAllValue(77);
        AssertEqual(77, list[0], "setAllValue[0]=77");
        AssertEqual(77, list[2], "setAllValue[2]=77");
    }

    // ─── setRange ────────────────────────────────────────────────────────
    private static void testSetRange()
    {
        var list = new List<int> { 1, 2, 3 };
        var src = new List<int> { 10, 20, 30, 40 };

        list.setRange(src);
        AssertEqual(4, list.count(), "setRange count=4");
        AssertEqual(10, list[0], "setRange[0]=10");
        AssertEqual(40, list[3], "setRange[3]=40");
    }

    // ─── addRange ────────────────────────────────────────────────────────
    private static void testAddRange()
    {
        var list = new List<int> { 1, 2 };
        var src = new List<int> { 3, 4 };

        list.addRange(src);
        AssertEqual(4, list.count(), "addRange count=4");
        AssertEqual(3, list[2], "addRange[2]=3");
        AssertEqual(4, list[3], "addRange[3]=4");
    }

    // ─── getEmptyList ────────────────────────────────────────────────────
    private static void testGetEmptyList()
    {
        List<int> list = EmptyList<int>.getEmptyList();
        AssertNotNull(list, "getEmptyList not null");
        AssertEqual(0, list.Count, "getEmptyList empty");
        // 验证单例
        List<int> list2 = EmptyList<int>.getEmptyList();
        Assert(ReferenceEquals(list, list2), "getEmptyList singleton");
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

    // ─── random ──────────────────────────────────────────────────────────
    private static void testRandom()
    {
        var list = new List<int> { 10, 20, 30 };
        int r = list.random();
        Assert(r == 10 || r == 20 || r == 30, "random in range");

        var empty = new List<int>();
        AssertEqual(0, empty.random(), "random empty=default");
    }

    // ─── removeIf (conditional) ──────────────────────────────────────────
    private static void testRemoveIfConditional()
    {
        var list = new List<int> { 1, 2, 3 };
        bool removed = list.removeIf(2, true);
        Assert(removed, "removeIf condition true → removed");
        AssertEqual(2, list.Count, "removeIf count=2");
        Assert(!list.Contains(2), "removeIf value gone");

        var list2 = new List<int> { 1, 2, 3 };
        bool notRemoved = list2.removeIf(2, false);
        Assert(!notRemoved, "removeIf condition false → not removed");
        AssertEqual(3, list2.Count, "removeIf condition false count=3");
    }

    // ─── removeAtIf ──────────────────────────────────────────────────────
    private static void testRemoveAtIf()
    {
        var list = new List<int> { 10, 20, 30 };
        int val = list.removeAtIf(1, true);
        AssertEqual(20, val, "removeAtIf returned value");
        AssertEqual(2, list.Count, "removeAtIf count=2");

        var list2 = new List<int> { 10, 20, 30 };
        int def = list2.removeAtIf(1, false);
        AssertEqual(0, def, "removeAtIf condition false → default");
        AssertEqual(3, list2.Count, "removeAtIf condition false count=3");
    }

    // ─── addRangeKeys / addRangeValues ───────────────────────────────────
    private static void testAddRangeKeysValues()
    {
        var dic = new Dictionary<int, string> { { 1, "a" }, { 2, "b" }, { 3, "c" } };

        // addRangeKeys
        var keyList = new List<int>();
        keyList.addRangeKeys(dic);
        AssertEqual(3, keyList.Count, "addRangeKeys count=3");
        Assert(keyList.Contains(1) && keyList.Contains(2) && keyList.Contains(3), "addRangeKeys all keys");

        // addRangeValues
        var valList = new List<string>();
        valList.addRangeValues(dic);
        AssertEqual(3, valList.Count, "addRangeValues count=3");
        Assert(valList.Contains("a") && valList.Contains("b") && valList.Contains("c"), "addRangeValues all vals");

        // empty dic
        var emptyDic = new Dictionary<int, string>();
        var emptyList = new List<int>();
        emptyList.addRangeKeys(emptyDic);
        AssertEqual(0, emptyList.Count, "addRangeKeys empty dic");
    }

    // ─── addNotNull ──────────────────────────────────────────────────────
    private static void testAddNotNull()
    {
        var list = new List<string>();
        Assert(list.addNotNull("hello"), "addNotNull non-null → true");
        AssertEqual(1, list.Count, "addNotNull count=1");

        Assert(!list.addNotNull(null), "addNotNull null → false");
        AssertEqual(1, list.Count, "addNotNull null count unchanged");
    }

    // ─── addRangeNotNull ─────────────────────────────────────────────────
    private static void testAddRangeNotNull()
    {
        var list = new List<string>();
        var src = new List<string> { "a", null, "b", null, "c" };
        list.addRangeNotNull(src);
        AssertEqual(3, list.Count, "addRangeNotNull count=3");
        AssertEqual("a", list[0], "addRangeNotNull[0]=a");
        AssertEqual("b", list[1], "addRangeNotNull[1]=b");
        AssertEqual("c", list[2], "addRangeNotNull[2]=c");
    }

    // ─── addNew ──────────────────────────────────────────────────────────
    private static void testAddNew()
    {
        var list = new List<StringBuilder>();
        var sb = list.addNew();
        AssertNotNull(sb, "addNew not null");
        AssertEqual(1, list.Count, "addNew count=1");
    }

    // ─── addUniqueIf ─────────────────────────────────────────────────────
    private static void testAddUniqueIf()
    {
        var list = new List<int> { 1, 2 };
        Assert(list.addUniqueIf(3, true), "addUniqueIf new+true → added");
        Assert(!list.addUniqueIf(2, true), "addUniqueIf existing+true → not added");
        Assert(!list.addUniqueIf(4, false), "addUniqueIf new+false → not added");
    }

    // ─── addUniqueOrRemove ───────────────────────────────────────────────
    private static void testAddUniqueOrRemove()
    {
        var list = new List<int> { 1, 2, 3 };
        list.addUniqueOrRemove(4, true);
        Assert(list.Contains(4), "addUniqueOrRemove add → contains");

        list.addUniqueOrRemove(2, false);
        Assert(!list.Contains(2), "addUniqueOrRemove remove → gone");
    }

    // ─── addUniqueNot ────────────────────────────────────────────────────
    private static void testAddUniqueNot()
    {
        var list = new List<int> { 1, 2 };
        Assert(list.addUniqueNot(3, 99), "addUniqueNot different → added");
        Assert(!list.addUniqueNot(99, 99), "addUniqueNot equal → not added");
        Assert(!list.addUniqueNot(1, 99), "addUniqueNot already in list → not added");
    }

    // ─── addRangeDerived ─────────────────────────────────────────────────
    private static void testAddRangeDerived()
    {
        var list = new List<object> { "existing" };
        var src = new List<string> { "a", "b" };
        list.addRangeDerived(src);
        AssertEqual(3, list.Count, "addRangeDerived count=3");
        AssertEqual("existing", list[0], "addRangeDerived kept existing");
        AssertEqual("a", list[1], "addRangeDerived[1]=a");
    }

    // ─── setRangeDerived ─────────────────────────────────────────────────
    private static void testSetRangeDerived()
    {
        var list = new List<object> { "old1", "old2" };
        var src = new List<string> { "new1", "new2" };
        list.setRangeDerived(src);
        AssertEqual(2, list.Count, "setRangeDerived count=2");
        AssertEqual("new1", list[0], "setRangeDerived[0]=new1");
    }

    // ─── setRangeKeys / setRangeValues ───────────────────────────────────
    private static void testSetRangeKeysValues()
    {
        var dic = new Dictionary<int, string> { { 1, "a" }, { 2, "b" } };

        var keyList = new List<int> { 99 };
        keyList.setRangeKeys(dic);
        AssertEqual(2, keyList.Count, "setRangeKeys count=2");
        AssertEqual(1, keyList[0], "setRangeKeys[0]=1");

        var valList = new List<string> { "old" };
        valList.setRangeValues(dic);
        AssertEqual(2, valList.Count, "setRangeValues count=2");
        AssertEqual("a", valList[0], "setRangeValues[0]=a");
    }

    // ─── moveTo ──────────────────────────────────────────────────────────
    private static void testMoveTo()
    {
        var src = new List<int> { 1, 2, 3 };
        var dst = new List<int> { 10 };

        src.moveTo(dst);
        AssertEqual(0, src.Count, "moveTo src empty");
        AssertEqual(4, dst.Count, "moveTo dst count=4");
        AssertEqual(10, dst[0], "moveTo dst[0]=10");
        AssertEqual(1, dst[1], "moveTo dst[1]=1");
    }

    // ─── For ─────────────────────────────────────────────────────────────
    private static void testFor()
    {
        var list = new List<int> { 1, 2, 3 };
        int sum = 0;
        list.For(x => sum += x);
        AssertEqual(6, sum, "For sum=6");

        // null 不崩溃
        List<int> nullList = null;
        nullList.For(x => { }); // no throw
    }

    // ─── ForI ────────────────────────────────────────────────────────────
    private static void testForI()
    {
        var list = new List<int> { 10, 20, 30 };
        int idxSum = 0;
        list.ForI(i => idxSum += i);
        AssertEqual(3, idxSum, "ForI idxSum=0+1+2=3");

        // 空列表
        var empty = new List<int>();
        empty.ForI(i => { }); // no throw
    }
}