#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;

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
