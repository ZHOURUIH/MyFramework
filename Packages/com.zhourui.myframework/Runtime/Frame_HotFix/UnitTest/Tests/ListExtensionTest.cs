using System;
using System.Collections.Generic;
using System.Text;
using static FrameUtility;
using static TestAssert;

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
        testAddCountDefault();
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
        testAddClass();
        testRemoveList();
        testRemoveFirstMatch();
        testAddRangeListCount();
        testAddRangeListStartCount();
        testAddRangeMultipleLists();
        testAddRangeArray();
        testAddRangeArrayCount();
        testAddRangeArrayStartCount();
        testAddRangeMultipleArrays();
        testAddRangeHashSet();
        testAddRangeSpan();
        testAddRangeSpanCount();
        testAddRangeNotNullArray();
        testAddRangeDerivedArray();
        testSetRangeArray();
        testSetRangeDerivedArray();
        testSetRangeSpan();
        testSetRangeSpanCount();
        testAddMulti();
        testFindPredicateOutValue();
        testFindValue();
        testFindPredicateOutIndexAndValue();
        testFindStartIndex();
        testFindStartIndexCount();
        testSafe();
        testFirstPredicate();
        testCountPredicate();
    }

    // ─── isEmpty / count ─────────────────────────────────────────────────
    private static void testIsEmptyAndCount()
    {
        List<int> empty = null;
        assert(empty.isEmpty(),          "null list isEmpty=true");
        assertEqual(0, empty.count(),    "null list count=0");

        var list = new List<int>();
        assert(list.isEmpty(),           "empty list isEmpty=true");
        assertEqual(0, list.count(),     "empty list count=0");

        list.Add(1);
        assert(!list.isEmpty(),          "non-empty isEmpty=false");
        assertEqual(1, list.count(),     "count=1");
    }

    // ─── get / set ───────────────────────────────────────────────────────
    private static void testGetAndSet()
    {
        var list = new List<int> { 10, 20, 30 };

        assertEqual(10, list.get(0),  "get[0]=10");
        assertEqual(30, list.get(2),  "get[2]=30");
        assertEqual(0,  list.get(5),  "get 越界=default(0)");
        assertEqual(0,  list.get(-1), "get 负索引=default(0)");

        list.set(1, 99);
        assertEqual(99, list[1], "set[1]=99");

        // 越界 set 不崩溃
        list.set(10, 0);
        // 数据不变
        assertEqual(99, list[1], "set 越界 list[1]不变");
    }

    // ─── add ─────────────────────────────────────────────────────────────
    private static void testAdd()
    {
        var list = new List<int>();
        list.add(1);
        list.add(2);
        list.add(3);

        assertEqual(3, list.count(), "add count=3");
        assertEqual(1, list[0], "add[0]=1");
        assertEqual(3, list[2], "add[2]=3");
    }

    // ─── addUnique ───────────────────────────────────────────────────────
    private static void testAddUnique()
    {
        var list = new List<int> { 1, 2, 3 };

        bool added = list.addUnique(4);
        assert(added, "addUnique new → true");
        assertEqual(4, list.count(), "addUnique count=4");

        bool notAdded = list.addUnique(2);
        assert(!notAdded, "addUnique existing → false");
        assertEqual(4, list.count(), "addUnique no duplicate added");
    }

    // ─── addIf ───────────────────────────────────────────────────────────
    private static void testAddIf()
    {
        var list = new List<int>();

        list.addIf(1, true);
        list.addIf(2, false);

        assertEqual(1, list.count(), "addIf true → added");
        assertEqual(1, list[0], "addIf value=1");
    }

    // ─── addNot ──────────────────────────────────────────────────────────
    private static void testAddNot()
    {
        var list = new List<int> { 1, 2, 3 };

        list.addNot(4, 2); // 4 != 2 → 添加
        assertEqual(4, list.count(), "addNot condition false → added");

        list.addNot(2, 2); // 2 == 2 → 不添加
        assertEqual(4, list.count(), "addNot condition true → not added");
    }

    // ─── addNotEmpty ─────────────────────────────────────────────────────
    private static void testAddNotEmpty()
    {
        var list = new List<string>();

        list.addNotEmpty("hello");
        assertEqual(1, list.count(), "addNotEmpty non-empty → added");

        list.addNotEmpty("");
        assertEqual(1, list.count(), "addNotEmpty empty → not added");

        list.addNotEmpty(null);
        assertEqual(1, list.count(), "addNotEmpty null → not added");
    }

    // ─── addCount ────────────────────────────────────────────────────────
    private static void testAddCount()
    {
        var list = new List<int> { 1, 2, 3 };

        list.addCount(99, 2);
        assertEqual(5, list.count(), "addCount count=5");
        assertEqual(99, list[3], "addCount[3]=99");
        assertEqual(99, list[4], "addCount[4]=99");
    }

    // ─── addCount (无值版, 只加 count 个 default) ─────────────────────────
    private static void testAddCountDefault()
    {
        var list = new List<int> { 1, 2, 3 };
        list.addCount(2);
        assertEqual(5, list.count(), "addCountDefault count=5");
        assertEqual(0, list[3], "addCountDefault[3]=default(0)");
        assertEqual(0, list[4], "addCountDefault[4]=default(0)");

        // count=0 不变
        var list2 = new List<int> { 1 };
        list2.addCount(0);
        assertEqual(1, list2.count(), "addCountDefault count=0 → unchanged");

        // 负数不崩溃
        var list3 = new List<int> { 1 };
        list3.addCount(-5);
        assertEqual(1, list3.count(), "addCountDefault negative → unchanged");
    }

    // ─── removeAt ────────────────────────────────────────────────────────
    private static void testRemoveAt()
    {
        var list = new List<int> { 1, 2, 3 };

        list.removeAt(1);
        assertEqual(2, list.count(), "removeAt count=2");
        assertEqual(3, list[1], "removeAt[1]=3");

        // 越界 removeAt 不崩溃
        try
        {
            list.removeAt(10);
            // 如果没抛异常，应该没变化
            assertEqual(2, list.count(), "removeAt 越界 count不变");
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
        assertEqual(2, removed, "removeIf removed 2 items");
        assertEqual(3, list.count(), "removeIf count=3");
        assert(!list.contains(2), "removeIf removed even numbers");
    }

    // ─── popBack / getLast ───────────────────────────────────────────────
    private static void testPopBackAndGetLast()
    {
        var list = new List<int> { 1, 2, 3 };

        int last = list.popBack();
        assertEqual(3, last, "popBack returns last");
        assertEqual(2, list.count(), "popBack count=2");

        int getLast = list.getLast();
        assertEqual(2, getLast, "getLast returns new last");

        // 空列表
        var empty = new List<int>();
        int defaultVal = empty.popBack();
        assertEqual(0, defaultVal, "popBack empty → default");
    }

    // ─── first ───────────────────────────────────────────────────────────
    private static void testFirst()
    {
        var list = new List<int> { 7, 8, 9 };
        assertEqual(7, list.first(), "first=7");

        var empty = new List<int>();
        assertEqual(0, empty.first(), "first empty=default");

        List<int> nullList = null;
        assertEqual(0, nullList.safe().first(), "first null=default via safe()");
    }

    // ─── find ────────────────────────────────────────────────────────────
    private static void testFind()
    {
        var list = new List<int> { 5, 10, 15, 20 };

        // find by value + out index
        bool ok = list.find(15, out int idx);
        assert(ok, "find 15 ok");
        assertEqual(2, idx, "find 15 index=2");

        ok = list.find(99, out idx);
        assert(!ok, "find 99 not found");
        assertEqual(-1, idx, "find 99 index=-1");

        // find by predicate → item
        int item = list.find(x => x > 12);
        assertEqual(15, item, "find pred item=15");

        // find by predicate → out index
        ok = list.find(x => x > 12, out int pidx);
        assert(ok, "find pred pidx ok");
        assertEqual(2, pidx, "find pred pidx=2");
    }

    // ─── contains ────────────────────────────────────────────────────────
    private static void testContains()
    {
        var list = new List<int> { 1, 2, 3 };
        assert(list.contains(2), "contains 2=true");
        assert(!list.contains(9), "contains 9=false");

        // contains by predicate
        assert(list.contains(x => x > 2), "contains pred >2=true");
        assert(!list.contains(x => x > 10), "contains pred >10=false");
    }

    // ─── swap ────────────────────────────────────────────────────────────
    private static void testSwap()
    {
        var list = new List<int> { 1, 2, 3, 4 };

        list.swap(1, 2);
        assertEqual(3, list[1], "swap[1]=3");
        assertEqual(2, list[2], "swap[2]=2");

        // 越界 swap 不崩溃
        try
        {
            list.swap(0, 10);
            // 如果没抛异常，应该没变化
            assertEqual(1, list[0], "swap 越界不变");
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
        assertEqual(4, list.count(), "count=4");
        assert(!list.contains(3), "element removed");
    }

    // ─── inverse ─────────────────────────────────────────────────────────
    private static void testInverse()
    {
        var list = new List<int> { 1, 2, 3, 4, 5 };
        list.inverse();
        assertEqual(5, list[0], "inverse[0]=5");
        assertEqual(3, list[2], "inverse[2]=3");
        assertEqual(1, list[4], "inverse[4]=1");
    }

    // ─── isSame ──────────────────────────────────────────────────────────
    private static void testIsSame()
    {
        var a = new List<int> { 1, 2, 3 };
        var b = new List<int> { 1, 2, 3 };
        var c = new List<int> { 1, 2, 4 };

        assert(a.isSame(b), "isSame identical → true");
        assert(!a.isSame(c), "isSame different → false");
        assert(!a.isSame(null), "isSame null → false");
    }

    // ─── isSubList ───────────────────────────────────────────────────────
    private static void testIsSubList()
    {
        var main = new List<int> { 1, 2, 3, 4, 5 };
        var sub = new List<int> { 2, 3, 4 };
        var notSub = new List<int> { 2, 3, 6 };

        assert(main.isSubList(sub), "isSubList true");
        assert(!main.isSubList(notSub), "isSubList false");
    }

    // ─── setAllDefault / setAllValue ─────────────────────────────────────
    private static void testSetAllDefaultAndValue()
    {
        var list = new List<int> { 1, 2, 3 };
        list.setAllDefault();
        assertEqual(0, list[0], "setAllDefault[0]=0");
        assertEqual(0, list[2], "setAllDefault[2]=0");

        list.setAllValue(77);
        assertEqual(77, list[0], "setAllValue[0]=77");
        assertEqual(77, list[2], "setAllValue[2]=77");
    }

    // ─── setRange ────────────────────────────────────────────────────────
    private static void testSetRange()
    {
        var list = new List<int> { 1, 2, 3 };
        var src = new List<int> { 10, 20, 30, 40 };

        list.setRange(src);
        assertEqual(4, list.count(), "setRange count=4");
        assertEqual(10, list[0], "setRange[0]=10");
        assertEqual(40, list[3], "setRange[3]=40");
    }

    // ─── addRange ────────────────────────────────────────────────────────
    private static void testAddRange()
    {
        var list = new List<int> { 1, 2 };
        var src = new List<int> { 3, 4 };

        list.addRange(src);
        assertEqual(4, list.count(), "addRange count=4");
        assertEqual(3, list[2], "addRange[2]=3");
        assertEqual(4, list[3], "addRange[3]=4");
    }

    // ─── getEmptyList ────────────────────────────────────────────────────
    private static void testGetEmptyList()
    {
        List<int> list = EmptyList<int>.getEmptyList();
        assertNotNull(list, "getEmptyList not null");
        assertEqual(0, list.Count, "getEmptyList empty");
        // 验证单例
        List<int> list2 = EmptyList<int>.getEmptyList();
        assert(ReferenceEquals(list, list2), "getEmptyList singleton");
    }

    // ─── random ──────────────────────────────────────────────────────────
    private static void testRandom()
    {
        var list = new List<int> { 10, 20, 30 };
        int r = list.random();
        assert(r == 10 || r == 20 || r == 30, "random in range");

        var empty = new List<int>();
        assertEqual(0, empty.random(), "random empty=default");
    }

    // ─── removeIf (conditional) ──────────────────────────────────────────
    private static void testRemoveIfConditional()
    {
        var list = new List<int> { 1, 2, 3 };
        bool removed = list.removeIf(2, true);
        assert(removed, "removeIf condition true → removed");
        assertEqual(2, list.Count, "removeIf count=2");
        assert(!list.Contains(2), "removeIf value gone");

        var list2 = new List<int> { 1, 2, 3 };
        bool notRemoved = list2.removeIf(2, false);
        assert(!notRemoved, "removeIf condition false → not removed");
        assertEqual(3, list2.Count, "removeIf condition false count=3");
    }

    // ─── removeAtIf ──────────────────────────────────────────────────────
    private static void testRemoveAtIf()
    {
        var list = new List<int> { 10, 20, 30 };
        int val = list.removeAtIf(1, true);
        assertEqual(20, val, "removeAtIf returned value");
        assertEqual(2, list.Count, "removeAtIf count=2");

        var list2 = new List<int> { 10, 20, 30 };
        int def = list2.removeAtIf(1, false);
        assertEqual(0, def, "removeAtIf condition false → default");
        assertEqual(3, list2.Count, "removeAtIf condition false count=3");
    }

    // ─── addRangeKeys / addRangeValues ───────────────────────────────────
    private static void testAddRangeKeysValues()
    {
        var dic = new Dictionary<int, string> { { 1, "a" }, { 2, "b" }, { 3, "c" } };

        // addRangeKeys
        var keyList = new List<int>();
        keyList.addRangeKeys(dic);
        assertEqual(3, keyList.Count, "addRangeKeys count=3");
        assert(keyList.Contains(1) && keyList.Contains(2) && keyList.Contains(3), "addRangeKeys all keys");

        // addRangeValues
        var valList = new List<string>();
        valList.addRangeValues(dic);
        assertEqual(3, valList.Count, "addRangeValues count=3");
        assert(valList.Contains("a") && valList.Contains("b") && valList.Contains("c"), "addRangeValues all vals");

        // empty dic
        var emptyDic = new Dictionary<int, string>();
        var emptyList = new List<int>();
        emptyList.addRangeKeys(emptyDic);
        assertEqual(0, emptyList.Count, "addRangeKeys empty dic");
    }

    // ─── addNotNull ──────────────────────────────────────────────────────
    private static void testAddNotNull()
    {
        var list = new List<string>();
        assert(list.addNotNull("hello"), "addNotNull non-null → true");
        assertEqual(1, list.Count, "addNotNull count=1");

        assert(!list.addNotNull(null), "addNotNull null → false");
        assertEqual(1, list.Count, "addNotNull null count unchanged");
    }

    // ─── addRangeNotNull ─────────────────────────────────────────────────
    private static void testAddRangeNotNull()
    {
        var list = new List<string>();
        var src = new List<string> { "a", null, "b", null, "c" };
        list.addRangeNotNull(src);
        assertEqual(3, list.Count, "addRangeNotNull count=3");
        assertEqual("a", list[0], "addRangeNotNull[0]=a");
        assertEqual("b", list[1], "addRangeNotNull[1]=b");
        assertEqual("c", list[2], "addRangeNotNull[2]=c");
    }

    // ─── addNew ──────────────────────────────────────────────────────────
    private static void testAddNew()
    {
        var list = new List<StringBuilder>();
        var sb = list.addNew();
        assertNotNull(sb, "addNew not null");
        assertEqual(1, list.Count, "addNew count=1");
    }

    // ─── addUniqueIf ─────────────────────────────────────────────────────
    private static void testAddUniqueIf()
    {
        var list = new List<int> { 1, 2 };
        assert(list.addUniqueIf(3, true), "addUniqueIf new+true → added");
        assert(!list.addUniqueIf(2, true), "addUniqueIf existing+true → not added");
        assert(!list.addUniqueIf(4, false), "addUniqueIf new+false → not added");
    }

    // ─── addUniqueOrRemove ───────────────────────────────────────────────
    private static void testAddUniqueOrRemove()
    {
        var list = new List<int> { 1, 2, 3 };
        list.addUniqueOrRemove(4, true);
        assert(list.Contains(4), "addUniqueOrRemove add → contains");

        list.addUniqueOrRemove(2, false);
        assert(!list.Contains(2), "addUniqueOrRemove remove → gone");
    }

    // ─── addUniqueNot ────────────────────────────────────────────────────
    private static void testAddUniqueNot()
    {
        var list = new List<int> { 1, 2 };
        assert(list.addUniqueNot(3, 99), "addUniqueNot different → added");
        assert(!list.addUniqueNot(99, 99), "addUniqueNot equal → not added");
        assert(!list.addUniqueNot(1, 99), "addUniqueNot already in list → not added");
    }

    // ─── addRangeDerived ─────────────────────────────────────────────────
    private static void testAddRangeDerived()
    {
        var list = new List<object> { "existing" };
        var src = new List<string> { "a", "b" };
        list.addRangeDerived(src);
        assertEqual(3, list.Count, "addRangeDerived count=3");
        assertEqual("existing", list[0], "addRangeDerived kept existing");
        assertEqual("a", list[1], "addRangeDerived[1]=a");
    }

    // ─── setRangeDerived ─────────────────────────────────────────────────
    private static void testSetRangeDerived()
    {
        var list = new List<object> { "old1", "old2" };
        var src = new List<string> { "new1", "new2" };
        list.setRangeDerived(src);
        assertEqual(2, list.Count, "setRangeDerived count=2");
        assertEqual("new1", list[0], "setRangeDerived[0]=new1");
    }

    // ─── setRangeKeys / setRangeValues ───────────────────────────────────
    private static void testSetRangeKeysValues()
    {
        var dic = new Dictionary<int, string> { { 1, "a" }, { 2, "b" } };

        var keyList = new List<int> { 99 };
        keyList.setRangeKeys(dic);
        assertEqual(2, keyList.Count, "setRangeKeys count=2");
        assertEqual(1, keyList[0], "setRangeKeys[0]=1");

        var valList = new List<string> { "old" };
        valList.setRangeValues(dic);
        assertEqual(2, valList.Count, "setRangeValues count=2");
        assertEqual("a", valList[0], "setRangeValues[0]=a");
    }

    // ─── moveTo ──────────────────────────────────────────────────────────
    private static void testMoveTo()
    {
        var src = new List<int> { 1, 2, 3 };
        var dst = new List<int> { 10 };

        src.moveTo(dst);
        assertEqual(0, src.Count, "moveTo src empty");
        assertEqual(4, dst.Count, "moveTo dst count=4");
        assertEqual(10, dst[0], "moveTo dst[0]=10");
        assertEqual(1, dst[1], "moveTo dst[1]=1");
    }

    // ─── For ─────────────────────────────────────────────────────────────
    private static void testFor()
    {
        var list = new List<int> { 1, 2, 3 };
        int sum = 0;
        list.For(x => sum += x);
        assertEqual(6, sum, "For sum=6");

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
        assertEqual(3, idxSum, "ForI idxSum=0+1+2=3");

        // 空列表
        var empty = new List<int>();
        empty.ForI(i => { }); // no throw
    }

    // ─── remove(List<T>) ─────────────────────────────────────────────────
    private static void testRemoveList()
    {
        var list = new List<int> { 1, 2, 3, 4, 5 };
        var toRemove = new List<int> { 2, 4 };
        list.remove(toRemove);
        assertEqual(3, list.Count, "remove(List) count=3");
        assert(!list.Contains(2), "remove(List) 2 gone");
        assert(!list.Contains(4), "remove(List) 4 gone");
        assert(list.Contains(1), "remove(List) 1 kept");

        // 空移除列表
        var emptyRemove = new List<int>();
        list.remove(emptyRemove); // 不崩溃
        assertEqual(3, list.Count, "remove(List) empty → unchanged");
    }

    // ─── remove(Predicate) ───────────────────────────────────────────────
    private static void testRemoveFirstMatch()
    {
        var list = new List<int> { 1, 2, 3, 2, 4 };
        bool removed = list.remove(x => x == 2); // 只移除第一个匹配
        assert(removed, "remove(Predicate) first 2 removed");
        assertEqual(4, list.Count, "remove(Predicate) count=4");
        // 第二个2应该还在
        assert(list.Contains(2), "remove(Predicate) second 2 still there");

        // 无匹配
        bool notRemoved = list.remove(x => x > 100);
        assert(!notRemoved, "remove(Predicate) no match → false");

        // 空列表
        var empty = new List<int>();
        assert(!empty.remove(x => true), "remove(Predicate) empty → false");
    }

    // ─── addRange(List, int count) ────────────────────────────────────────
    private static void testAddRangeListCount()
    {
        var list = new List<int> { 1 };
        var src = new List<int> { 10, 20, 30, 40 };
        list.addRange(src, 2);
        assertEqual(3, list.Count, "addRange count limit=3");
        assertEqual(10, list[1], "addRange count[1]=10");
        assertEqual(20, list[2], "addRange count[2]=20");
    }

    // ─── addRange(List, startIndex, count) ────────────────────────────────
    private static void testAddRangeListStartCount()
    {
        var list = new List<int> { 1 };
        var src = new List<int> { 10, 20, 30, 40 };
        list.addRange(src, 1, 2);
        assertEqual(3, list.Count, "addRange start+count=3");
        assertEqual(20, list[1], "addRange start+count[1]=20");
        assertEqual(30, list[2], "addRange start+count[2]=30");
    }

    // ─── addRange(List, List, List) ───────────────────────────────────────
    private static void testAddRangeMultipleLists()
    {
        var list = new List<int> { 0 };
        var a = new List<int> { 1, 2 };
        var b = new List<int> { 3, 4 };
        var c = new List<int> { 5, 6 };
        var d = new List<int> { 7, 8 };

        list.addRange(a, b);
        assertEqual(5, list.Count, "addRange 2 lists count=5");
        assertEqual(1, list[1], "addRange 2 lists[1]=1");
        assertEqual(4, list[4], "addRange 2 lists[4]=4");

        var list2 = new List<int> { 0 };
        list2.addRange(a, b, c);
        assertEqual(7, list2.Count, "addRange 3 lists count=7");

        var list3 = new List<int> { 0 };
        list3.addRange(a, b, c, d);
        assertEqual(9, list3.Count, "addRange 4 lists count=9");
    }

    // ─── addRange(T[]) ────────────────────────────────────────────────────
    private static void testAddRangeArray()
    {
        var list = new List<int> { 1, 2 };
        int[] src = { 3, 4 };
        list.addRange(src);
        assertEqual(4, list.Count, "addRange array count=4");
        assertEqual(3, list[2], "addRange array[2]=3");
        assertEqual(4, list[3], "addRange array[3]=4");
    }

    // ─── addRange(T[], count) ─────────────────────────────────────────────
    private static void testAddRangeArrayCount()
    {
        var list = new List<int> { 1 };
        int[] src = { 10, 20, 30, 40 };
        list.addRange(src, 2);
        assertEqual(3, list.Count, "addRange arr count=3");
        assertEqual(10, list[1], "addRange arr count[1]=10");
        assertEqual(20, list[2], "addRange arr count[2]=20");
    }

    // ─── addRange(T[], startIndex, count) ─────────────────────────────────
    private static void testAddRangeArrayStartCount()
    {
        var list = new List<int> { 1 };
        int[] src = { 10, 20, 30, 40 };
        list.addRange(src, 1, 2);
        assertEqual(3, list.Count, "addRange arr start+count=3");
        assertEqual(20, list[1], "addRange arr start+count[1]=20");
        assertEqual(30, list[2], "addRange arr start+count[2]=30");
    }

    // ─── addRange(T[], T[]) ───────────────────────────────────────────────
    private static void testAddRangeMultipleArrays()
    {
        var list = new List<int> { 0 };
        int[] a = { 1, 2 };
        int[] b = { 3, 4 };
        int[] c = { 5, 6 };
        int[] d = { 7, 8 };

        list.addRange(a, b);
        assertEqual(5, list.Count, "addRange 2 arrays count=5");

        var list2 = new List<int> { 0 };
        list2.addRange(a, b, c);
        assertEqual(7, list2.Count, "addRange 3 arrays count=7");

        var list3 = new List<int> { 0 };
        list3.addRange(a, b, c, d);
        assertEqual(9, list3.Count, "addRange 4 arrays count=9");
    }

    // ─── addRange(HashSet) ────────────────────────────────────────────────
    private static void testAddRangeHashSet()
    {
        var list = new List<int> { 1 };
        var set = new HashSet<int> { 2, 3, 4 };
        list.addRange(set);
        assertEqual(4, list.Count, "addRange HashSet count=4");
        assert(list.Contains(2) && list.Contains(3) && list.Contains(4), "addRange HashSet all added");
    }

    // ─── addRange(Span) ───────────────────────────────────────────────────
    private static void testAddRangeSpan()
    {
        var list = new List<int> { 1 };
        Span<int> span = stackalloc int[] { 10, 20 };
        list.addRange(span);
        assertEqual(3, list.Count, "addRange Span count=3");
        assertEqual(10, list[1], "addRange Span[1]=10");
        assertEqual(20, list[2], "addRange Span[2]=20");
    }

    // ─── addRange(Span, count) ────────────────────────────────────────────
    private static void testAddRangeSpanCount()
    {
        var list = new List<int> { 1 };
        Span<int> span = stackalloc int[] { 10, 20, 30, 40 };
        list.addRange(span, 2);
        assertEqual(3, list.Count, "addRange Span count=3");
        assertEqual(10, list[1], "addRange Span count[1]=10");
    }

    // ─── addRangeNotNull(T[]) ─────────────────────────────────────────────
    private static void testAddRangeNotNullArray()
    {
        var list = new List<string>();
        string[] src = { "a", null, "b", null, "c" };
        list.addRangeNotNull(src);
        assertEqual(3, list.Count, "addRangeNotNull arr count=3");
        assertEqual("a", list[0], "addRangeNotNull arr[0]=a");
        assertEqual("b", list[1], "addRangeNotNull arr[1]=b");
        assertEqual("c", list[2], "addRangeNotNull arr[2]=c");
    }

    // ─── addRangeDerived(T[]) ─────────────────────────────────────────────
    private static void testAddRangeDerivedArray()
    {
        var list = new List<object> { "existing" };
        string[] src = { "a", "b" };
        list.addRangeDerived(src);
        assertEqual(3, list.Count, "addRangeDerived arr count=3");
        assertEqual("existing", list[0], "addRangeDerived arr kept existing");
        assertEqual("a", list[1], "addRangeDerived arr[1]=a");
    }

    // ─── setRange(T[]) ────────────────────────────────────────────────────
    private static void testSetRangeArray()
    {
        var list = new List<int> { 1, 2, 3 };
        int[] src = { 10, 20 };
        list.setRange(src);
        assertEqual(2, list.Count, "setRange arr count=2");
        assertEqual(10, list[0], "setRange arr[0]=10");
        assertEqual(20, list[1], "setRange arr[1]=20");
    }

    // ─── setRangeDerived(T[]) ─────────────────────────────────────────────
    private static void testSetRangeDerivedArray()
    {
        var list = new List<object> { "old1", "old2" };
        string[] src = { "new1", "new2" };
        list.setRangeDerived(src);
        assertEqual(2, list.Count, "setRangeDerived arr count=2");
        assertEqual("new1", list[0], "setRangeDerived arr[0]=new1");
    }

    // ─── setRange(Span) ───────────────────────────────────────────────────
    private static void testSetRangeSpan()
    {
        var list = new List<int> { 1, 2, 3 };
        Span<int> span = stackalloc int[] { 10, 20 };
        list.setRange(span);
        assertEqual(2, list.Count, "setRange Span count=2");
        assertEqual(10, list[0], "setRange Span[0]=10");
        assertEqual(20, list[1], "setRange Span[1]=20");
    }

    // ─── setRange(Span, count) ────────────────────────────────────────────
    private static void testSetRangeSpanCount()
    {
        var list = new List<int> { 1, 2, 3 };
        Span<int> span = stackalloc int[] { 10, 20, 30, 40 };
        list.setRange(span, 2);
        assertEqual(2, list.Count, "setRange Span count=2");
        assertEqual(10, list[0], "setRange Span count[0]=10");
    }

    // ─── add(T, T) / add(T, T, T) / add(T, T, T, T) / add(T, T, T, T, T) ─
    private static void testAddMulti()
    {
        var list = new List<int>();
        list.add(1, 2);
        assertEqual(2, list.Count, "add 2 args count=2");
        assertEqual(1, list[0], "add 2 args[0]=1");
        assertEqual(2, list[1], "add 2 args[1]=2");

        var list2 = new List<int>();
        list2.add(1, 2, 3);
        assertEqual(3, list2.Count, "add 3 args count=3");

        var list3 = new List<int>();
        list3.add(1, 2, 3, 4);
        assertEqual(4, list3.Count, "add 4 args count=4");

        var list4 = new List<int>();
        list4.add(1, 2, 3, 4, 5);
        assertEqual(5, list4.Count, "add 5 args count=5");
    }

    // ─── find(Predicate, out T) ───────────────────────────────────────────
    // 注意: 对 List<int> T=int, find(Predicate,out int) 和 find(Predicate,out int index)
    // 签名冲突。因此用 string 类型测试 out T 重载。
    private static void testFindPredicateOutValue()
    {
        var list = new List<string> { "a", "bb", "ccc", "dddd" };
        bool ok = list.find(x => x.Length > 2, out string item);
        assert(ok, "find pred out value ok");
        assertEqual("ccc", item, "find pred out value=ccc");

        // 未找到
        ok = list.find(x => x.Length > 10, out string notFound);
        assert(!ok, "find pred out value not found");

        // 空列表
        var empty = new List<string>();
        ok = empty.find(x => true, out string emptyItem);
        assert(!ok, "find pred out value empty → false");
    }

    // ─── find(T) ──────────────────────────────────────────────────────────
    private static void testFindValue()
    {
        var list = new List<int> { 5, 10, 15, 20 };
        int idx = list.find(15);
        assertEqual(2, idx, "find value index=2");

        int notFound = list.find(99);
        assertEqual(-1, notFound, "find value not found=-1");

        // 空列表
        var empty = new List<int>();
        assertEqual(-1, empty.find(1), "find value empty=-1");
    }

    // ─── find(Predicate, out int, out T) ──────────────────────────────────
    private static void testFindPredicateOutIndexAndValue()
    {
        var list = new List<int> { 5, 10, 15, 20 };
        bool ok = list.find(x => x > 12, out int idx, out int item);
        assert(ok, "find out idx+val ok");
        assertEqual(2, idx, "find out idx+val idx=2");
        assertEqual(15, item, "find out idx+val item=15");

        // 未找到
        ok = list.find(x => x > 100, out int nfIdx, out int nfItem);
        assert(!ok, "find out idx+val not found");
        assertEqual(-1, nfIdx, "find out idx+val idx=-1");
    }

    // ─── find(startIndex, Predicate, out int) ─────────────────────────────
    private static void testFindStartIndex()
    {
        var list = new List<int> { 5, 10, 15, 20, 25 };
        bool ok = list.find(2, x => x > 10, out int idx);
        assert(ok, "find startIndex ok");
        assertEqual(2, idx, "find startIndex idx=2"); // 15

        // 从中间开始找不到后面的
        ok = list.find(2, x => x < 10, out int idx2);
        assert(!ok, "find startIndex no match before start");

        // 空列表
        var empty = new List<int>();
        ok = empty.find(0, x => true, out int emptyIdx);
        assert(!ok, "find startIndex empty → false");
    }

    // ─── find(startIndex, count, Predicate, out int) ──────────────────────
    private static void testFindStartIndexCount()
    {
        var list = new List<int> { 5, 10, 15, 20, 25 };
        bool ok = list.find(1, 2, x => x > 10, out int idx);
        assert(ok, "find range ok");
        assertEqual(2, idx, "find range idx=2");

        // 范围内找不到
        ok = list.find(0, 2, x => x > 10, out int idx2);
        assert(!ok, "find range no match");

        // 空列表
        var empty = new List<int>();
        ok = empty.find(0, 0, x => true, out int emptyIdx);
        assert(!ok, "find range empty → false");
    }

    // ─── safe ─────────────────────────────────────────────────────────────
    private static void testSafe()
    {
        List<int> nullList = null;
        var safe = nullList.safe();
        assertNotNull(safe, "safe null → non-null");
        assertEqual(0, safe.Count, "safe null → empty");

        var list = new List<int> { 1, 2 };
        var safe2 = list.safe();
        assert(ReferenceEquals(list, safe2), "safe non-null → same ref");
    }

    // ─── first(Predicate) ─────────────────────────────────────────────────
    private static void testFirstPredicate()
    {
        var list = new List<int> { 5, 10, 15, 20 };
        int first = list.first(x => x > 10);
        assertEqual(15, first, "first pred=15");

        // 无匹配
        int notFound = list.first(x => x > 100);
        assertEqual(0, notFound, "first pred no match=default");

        // 空列表
        var empty = new List<int>();
        assertEqual(0, empty.first(x => true), "first pred empty=default");
    }

    // ─── count(Predicate) ─────────────────────────────────────────────────
    private static void testCountPredicate()
    {
        var list = new List<int> { 1, 2, 3, 4, 5, 6 };
        int even = list.count(x => x % 2 == 0);
        assertEqual(3, even, "count even=3");

        int above10 = list.count(x => x > 10);
        assertEqual(0, above10, "count >10=0");

        // 空列表
        var empty = new List<int>();
        assertEqual(0, empty.count(x => true), "count empty=0");
    }

    // ─── addClass ────────────────────────────────────────────────────────
    private static void testAddClass()
    {
        var list = new List<TestListClass>();
        TestListClass obj = list.addClass();
        assertNotNull(obj, "addClass not null");
        assertEqual(1, list.Count, "addClass count=1");
        assert(obj == list[0], "addClass same ref");
        UN_CLASS(ref obj);
    }

    public class TestListClass : ClassObject { }
}