using System;
using System.Collections.Generic;
using System.Text;
using static TestAssert;

// ArrayExtension 扩展方法测试
// 覆盖：isEmpty / count / get / set / contains / find / first / inverse /
//        setAllDefault / setAllValue / setRange / ForI / For /
//        bytesToString / safe / EmptyArray
public static class ArrayExtensionTest
{
    public static void Run()
    {
        testIsEmptyAndCount();
        testGetAndSet();
        testContains();
        testFind();
        testFirst();
        testInverse();
        testSetAllDefaultAndValue();
        testSetRange();
        testForAndForI();
        testCountWithCondition();
        testBytesToString();
        testSafe();
        testEmptyArray();
        testRemoveIndex();
        testRemoveValue();
        testSortWithComparison();
        testSort();
        testSortWithComparer();
        testFindPredicateOutValue();
        testSetRangeSpan();
    }

    // ─── isEmpty / count ─────────────────────────────────────────────────
    private static void testIsEmptyAndCount()
    {
        int[] nullArr = null;
        assert(nullArr.isEmpty(),       "null array isEmpty=true");
        assertEqual(0, nullArr.count(), "null array count=0");

        int[] empty = new int[0];
        assert(empty.isEmpty(),         "empty array isEmpty=true");
        assertEqual(0, empty.count(),   "empty array count=0");

        int[] arr = { 1, 2, 3 };
        assert(!arr.isEmpty(),          "non-empty isEmpty=false");
        assertEqual(3, arr.count(),     "count=3");
    }

    // ─── get / set ───────────────────────────────────────────────────────
    private static void testGetAndSet()
    {
        int[] arr = { 10, 20, 30 };

        assertEqual(10, arr.get(0),  "get[0]=10");
        assertEqual(30, arr.get(2),  "get[2]=30");
        assertEqual(0,  arr.get(5),  "get 越界=default(0)");
        assertEqual(0,  arr.get(-1), "get 负索引=default(0)");

        arr.set(1, 99);
        assertEqual(99, arr[1], "set[1]=99");

        // 越界 set 不崩溃
        arr.set(10, 0);
        // 数据不变
        assertEqual(99, arr[1], "set 越界 arr[1]不变");
    }

    // ─── contains ────────────────────────────────────────────────────────
    private static void testContains()
    {
        int[] arr = { 1, 2, 3 };
        assert(arr.contains(2),         "contains 2=true");
        assert(!arr.contains(9),        "contains 9=false");

        // contains by predicate
        assert(arr.contains(x => x > 2), "contains pred >2=true");

        string[] sarr = { "a", "b", "c" };
        assert( sarr.contains(s => s == "b"),   "contains pred string=true");
        assert(!sarr.contains(s => s == "xyz"), "contains pred string not found=false");

        int[] nullArr = null;
        assert(!nullArr.contains(1), "null contains=false");
    }

    // ─── find ─────────────────────────────────────────────────────────────
    private static void testFind()
    {
        int[] arr = { 5, 10, 15, 20 };

        // find by value + out index
        bool ok = arr.find(15, out int idx);
        assert(ok,              "find 15 ok");
        assertEqual(2, idx,     "find 15 index=2");

        ok = arr.find(99, out idx);
        assert(!ok,             "find 99 not found");
        assertEqual(-1, idx,    "find 99 index=-1");

        // find by value → index
        assertEqual(0,  arr.find(5),  "find(5)=0");
        assertEqual(-1, arr.find(99), "find(99)=-1");

        // find by predicate → item
        int item = arr.find(x => x > 12);
        assertEqual(15, item, "find pred item=15");

        // find by predicate → out index
        ok = arr.find(x => x > 12, out int pidx);
        assert(ok,           "find pred pidx ok");
        assertEqual(2, pidx, "find pred pidx=2");

        // 未找到
        int notFound = arr.find(x => x > 100);
        assertEqual(0, notFound, "find pred not found=default");

        // 空数组
        int[] empty = new int[0];
        assertEqual(-1, empty.find(1), "find empty array=-1");
    }

    // ─── first ───────────────────────────────────────────────────────────
    private static void testFirst()
    {
        int[] arr = { 7, 8, 9 };
        assertEqual(7, arr.first(), "first=7");

        int[] empty = new int[0];
        assertEqual(0, empty.first(), "first empty=default");

        int[] nullArr = null;
        assertEqual(0, nullArr.first(), "first null=default");
    }

    // ─── inverse ─────────────────────────────────────────────────────────
    private static void testInverse()
    {
        int[] arr = { 1, 2, 3, 4, 5 };
        arr.inverse();
        assertEqual(5, arr[0], "inverse[0]=5");
        assertEqual(3, arr[2], "inverse[2]=3");
        assertEqual(1, arr[4], "inverse[4]=1");

        // 单元素
        int[] single = { 42 };
        single.inverse();
        assertEqual(42, single[0], "inverse 单元素不变");

        // 偶数长度
        int[] even = { 1, 2, 3, 4 };
        even.inverse();
        assertEqual(4, even[0], "inverse even[0]=4");
        assertEqual(1, even[3], "inverse even[3]=1");
    }

    // ─── setAllDefault / setAllValue ─────────────────────────────────────
    private static void testSetAllDefaultAndValue()
    {
        int[] arr = { 1, 2, 3 };
        arr.setAllDefault();
        assertEqual(0, arr[0], "setAllDefault[0]=0");
        assertEqual(0, arr[2], "setAllDefault[2]=0");

        arr.setAllValue(77);
        assertEqual(77, arr[0], "setAllValue[0]=77");
        assertEqual(77, arr[2], "setAllValue[2]=77");

        // 空数组不崩溃
        int[] empty = new int[0];
        empty.setAllDefault();
        empty.setAllValue(1);
    }

    // ─── setRange ────────────────────────────────────────────────────────
    private static void testSetRange()
    {
        int[] arr = new int[4];
        var src = new List<int> { 10, 20, 30, 40 };
        arr.setRange(src);
        assertEqual(10, arr[0], "setRange list[0]=10");
        assertEqual(40, arr[3], "setRange list[3]=40");
    }

    // ─── For / ForI ──────────────────────────────────────────────────────
    private static void testForAndForI()
    {
        int[] arr = { 1, 2, 3 };
        int sum = 0;
        arr.For(x => sum += x);
        assertEqual(6, sum, "For sum=6");

        int idxSum = 0;
        arr.ForI(i => idxSum += i);
        assertEqual(3, idxSum, "ForI idxSum=0+1+2=3");

        // 空数组不崩溃
        int[] empty = new int[0];
        empty.For(x => { });
        empty.ForI(i => { });
    }

    // ─── count with condition ────────────────────────────────────────────
    private static void testCountWithCondition()
    {
        int[] arr = { 1, 2, 3, 4, 5, 6 };
        int even = arr.count(x => x % 2 == 0);
        assertEqual(3, even, "count even=3");

        int above10 = arr.count(x => x > 10);
        assertEqual(0, above10, "count >10 = 0");
    }

    // ─── bytesToString ───────────────────────────────────────────────────
    private static void testBytesToString()
    {
        byte[] bytes = Encoding.UTF8.GetBytes("Hello");
        string s = bytes.bytesToString();
        assertEqual("Hello", s, "bytesToString UTF8");

        // 带 count 参数
        string s2 = bytes.bytesToString(3);
        assertEqual("Hel", s2, "bytesToString count=3");

        // 带 startIndex + count
        string s3 = bytes.bytesToString(1, 3);
        assertEqual("ell", s3, "bytesToString offset=1 count=3");

        // 空数组
        byte[] empty = new byte[0];
        string s4 = empty.bytesToString();
        assertEqual("", s4, "bytesToString empty=''");

        // null
        byte[] nullArr = null;
        string s5 = nullArr.bytesToString();
        assertEqual("", s5, "bytesToString null=''");
    }

    // ─── safe ─────────────────────────────────────────────────────────────
    private static void testSafe()
    {
        int[] nullArr = null;
        int[] safe = nullArr.safe();
        assertNotNull(safe, "safe null → non-null");
        assertEqual(0, safe.count(), "safe null → empty array");

        int[] arr = { 1, 2 };
        int[] safe2 = arr.safe();
        assert(safe2 != null, "safe non-null → not null");
        assertEqual(2, safe2.count(), "safe non-null → same count");
    }

    // ─── EmptyArray ───────────────────────────────────────────────────────
    private static void testEmptyArray()
    {
        int[] e1 = EmptyArray<int>.getEmptyList();
        int[] e2 = EmptyArray<int>.getEmptyList();
        assertNotNull(e1, "EmptyArray not null");
        assertEqual(0, e1.Length, "EmptyArray length=0");
        // 单例：两次调用返回同一个对象
        assert(e1 == e2, "EmptyArray 单例");
    }

    // ─── removeIndex ─────────────────────────────────────────────────────
    private static void testRemoveIndex()
    {
        int[] arr = { 1, 2, 3, 4, 5 };
        arr.removeIndex(5, 1); // 移除 index=1 (值=2), 后面元素左移
        assertEqual(1, arr[0], "removeIndex[0]=1");
        assertEqual(3, arr[1], "removeIndex[1]=3");
        assertEqual(4, arr[2], "removeIndex[2]=4");
        assertEqual(5, arr[3], "removeIndex[3]=5");

        // 移除最后一个元素: 无元素左移
        int[] arr2 = { 10, 20, 30 };
        arr2.removeIndex(3, 2); // 移除 index=2 (值=30)
        assertEqual(10, arr2[0], "removeIndex last[0]=10");
        assertEqual(20, arr2[1], "removeIndex last[1]=20");

        // 移除第一个元素
        int[] arr3 = { 1, 2, 3 };
        arr3.removeIndex(3, 0);
        assertEqual(2, arr3[0], "removeIndex first[0]=2");
        assertEqual(3, arr3[1], "removeIndex first[1]=3");
    }

    // ─── removeValue ─────────────────────────────────────────────────────
    private static void testRemoveValue()
    {
        int[] arr = { 1, 2, 3, 2, 4 };
        int newCount = arr.removeValue(5, 2); // 移除所有值为2的元素
        assertEqual(3, newCount, "removeValue newCount=3"); // 返回新的有效元素个数
        assertEqual(1, arr[0], "removeValue[0]=1");
        assertEqual(3, arr[1], "removeValue[1]=3");
        assertEqual(4, arr[2], "removeValue[2]=4");

        // 不存在
        int[] arr2 = { 1, 2, 3 };
        int r2 = arr2.removeValue(3, 99);
        assertEqual(3, r2, "removeValue not found→count unchanged");

        // 多个相同值: 全部移除
        int[] arr3 = { 5, 5, 5, 5 };
        int r3 = arr3.removeValue(4, 5);
        assertEqual(0, r3, "removeValue all removed→0");
    }

    // ─── sort with Comparison ────────────────────────────────────────────
    private static void testSortWithComparison()
    {
        int[] arr = { 3, 1, 4, 1, 5, 9 };
        arr.sort((a, b) => a.CompareTo(b));
        assertEqual(1, arr[0], "sort asc[0]=1");
        assertEqual(9, arr[5], "sort asc[5]=9");

        // 降序
        int[] arr2 = { 3, 1, 4 };
        arr2.sort((a, b) => b.CompareTo(a));
        assertEqual(4, arr2[0], "sort desc[0]=4");
        assertEqual(1, arr2[2], "sort desc[2]=1");

        // 单元素
        int[] single = { 42 };
        single.sort((a, b) => a.CompareTo(b));
        assertEqual(42, single[0], "sort single=42");

        // 空数组
        int[] empty = new int[0];
        empty.sort((a, b) => a.CompareTo(b)); // 不崩溃
    }

    // ─── sort (无参) ─────────────────────────────────────────────────────
    private static void testSort()
    {
        int[] arr = { 3, 1, 4, 1, 5, 9 };
        arr.sort(); // 使用 Array.Sort 默认比较器
        assertEqual(1, arr[0], "sort()[0]=1");
        assertEqual(9, arr[5], "sort()[5]=9");

        // 字符串排序
        string[] sarr = { "c", "a", "b" };
        sarr.sort();
        assertEqual("a", sarr[0], "sort() string[0]=a");
        assertEqual("c", sarr[2], "sort() string[2]=c");

        // 空数组
        int[] empty = new int[0];
        empty.sort(); // 不崩溃
    }

    // ─── sort with IComparer ─────────────────────────────────────────────
    private static void testSortWithComparer()
    {
        int[] arr = { 3, 1, 4, 1, 5, 9 };
        arr.sort(System.Collections.Comparer.Default);
        assertEqual(1, arr[0], "sort(IComparer)[0]=1");
        assertEqual(9, arr[5], "sort(IComparer)[5]=9");

        // 空数组
        int[] empty = new int[0];
        empty.sort(System.Collections.Comparer.Default); // 不崩溃
    }

    // ─── find(Predicate, out T) ──────────────────────────────────────────
    // 注意: 对 int[] 类型 T=int, find(Predicate,out int) 和 find(Predicate,out int index)
    // 签名冲突, C# 重载解析会选择返回索引的版本。因此用 string[] 测试 out T 重载。
    private static void testFindPredicateOutValue()
    {
        string[] arr = { "a", "bb", "ccc", "dddd" };
        bool ok = arr.find(x => x.Length > 2, out string item);
        assert(ok, "find pred out value ok");
        assertEqual("ccc", item, "find pred out value=ccc");

        // 未找到
        ok = arr.find(x => x.Length > 10, out string notFound);
        assert(!ok, "find pred out value not found");
        assertEqual(null, notFound, "find pred out value default=null");

        // 空数组
        string[] empty = new string[0];
        ok = empty.find(x => true, out string emptyItem);
        assert(!ok, "find pred out value empty → false");

        // null 数组
        string[] nullArr = null;
        ok = nullArr.find(x => true, out string nullItem);
        assert(!ok, "find pred out value null → false");
    }

    // ─── setRange(Span) ──────────────────────────────────────────────────
    private static void testSetRangeSpan()
    {
        int[] arr = new int[4];
        Span<int> span = stackalloc int[] { 10, 20, 30, 40 };
        arr.setRange(span);
        assertEqual(10, arr[0], "setRange span[0]=10");
        assertEqual(40, arr[3], "setRange span[3]=40");
    }
}