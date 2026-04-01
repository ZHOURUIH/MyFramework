#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;
using System.Text;

// ArrayExtension 扩展方法测试
// 覆盖：isEmpty / count / get / set / contains / find / first / inverse /
//        setAllDefault / setAllValue / setRange / ForI / For /
//        bytesToString / safe / EmptyArray
public static class ArrayExtensionTest
{
    public static void Run()
    {
        try
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
            Console.WriteLine("ArrayExtensionTest: All tests passed");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ArrayExtensionTest: Test failed - {ex.Message}");
            throw;
        }
    }

    // ─── isEmpty / count ─────────────────────────────────────────────────
    private static void testIsEmptyAndCount()
    {
        int[] nullArr = null;
        Assert(nullArr.isEmpty(),       "null array isEmpty=true");
        AssertEqual(0, nullArr.count(), "null array count=0");

        int[] empty = new int[0];
        Assert(empty.isEmpty(),         "empty array isEmpty=true");
        AssertEqual(0, empty.count(),   "empty array count=0");

        int[] arr = { 1, 2, 3 };
        Assert(!arr.isEmpty(),          "non-empty isEmpty=false");
        AssertEqual(3, arr.count(),     "count=3");
    }

    // ─── get / set ───────────────────────────────────────────────────────
    private static void testGetAndSet()
    {
        int[] arr = { 10, 20, 30 };

        AssertEqual(10, arr.get(0),  "get[0]=10");
        AssertEqual(30, arr.get(2),  "get[2]=30");
        AssertEqual(0,  arr.get(5),  "get 越界=default(0)");
        AssertEqual(0,  arr.get(-1), "get 负索引=default(0)");

        arr.set(1, 99);
        AssertEqual(99, arr[1], "set[1]=99");

        // 越界 set 不崩溃
        arr.set(10, 0);
        // 数据不变
        AssertEqual(99, arr[1], "set 越界 arr[1]不变");
    }

    // ─── contains ────────────────────────────────────────────────────────
    private static void testContains()
    {
        int[] arr = { 1, 2, 3 };
        Assert(arr.contains(2),         "contains 2=true");
        Assert(!arr.contains(9),        "contains 9=false");

        // contains by predicate
        Assert(arr.contains(x => x > 2), "contains pred >2=true");

        string[] sarr = { "a", "b", "c" };
        Assert( sarr.contains(s => s == "b"),   "contains pred string=true");
        Assert(!sarr.contains(s => s == "xyz"), "contains pred string not found=false");

        int[] nullArr = null;
        Assert(!nullArr.contains(1), "null contains=false");
    }

    // ─── find ─────────────────────────────────────────────────────────────
    private static void testFind()
    {
        int[] arr = { 5, 10, 15, 20 };

        // find by value + out index
        bool ok = arr.find(15, out int idx);
        Assert(ok,              "find 15 ok");
        AssertEqual(2, idx,     "find 15 index=2");

        ok = arr.find(99, out idx);
        Assert(!ok,             "find 99 not found");
        AssertEqual(-1, idx,    "find 99 index=-1");

        // find by value → index
        AssertEqual(0,  arr.find(5),  "find(5)=0");
        AssertEqual(-1, arr.find(99), "find(99)=-1");

        // find by predicate → item
        int item = arr.find(x => x > 12);
        AssertEqual(15, item, "find pred item=15");

        // find by predicate → out index
        ok = arr.find(x => x > 12, out int pidx);
        Assert(ok,           "find pred pidx ok");
        AssertEqual(2, pidx, "find pred pidx=2");

        // 未找到
        int notFound = arr.find(x => x > 100);
        AssertEqual(0, notFound, "find pred not found=default");

        // 空数组
        int[] empty = new int[0];
        AssertEqual(-1, empty.find(1), "find empty array=-1");
    }

    // ─── first ───────────────────────────────────────────────────────────
    private static void testFirst()
    {
        int[] arr = { 7, 8, 9 };
        AssertEqual(7, arr.first(), "first=7");

        int[] empty = new int[0];
        AssertEqual(0, empty.first(), "first empty=default");

        int[] nullArr = null;
        AssertEqual(0, nullArr.first(), "first null=default");
    }

    // ─── inverse ─────────────────────────────────────────────────────────
    private static void testInverse()
    {
        int[] arr = { 1, 2, 3, 4, 5 };
        arr.inverse();
        AssertEqual(5, arr[0], "inverse[0]=5");
        AssertEqual(3, arr[2], "inverse[2]=3");
        AssertEqual(1, arr[4], "inverse[4]=1");

        // 单元素
        int[] single = { 42 };
        single.inverse();
        AssertEqual(42, single[0], "inverse 单元素不变");

        // 偶数长度
        int[] even = { 1, 2, 3, 4 };
        even.inverse();
        AssertEqual(4, even[0], "inverse even[0]=4");
        AssertEqual(1, even[3], "inverse even[3]=1");
    }

    // ─── setAllDefault / setAllValue ─────────────────────────────────────
    private static void testSetAllDefaultAndValue()
    {
        int[] arr = { 1, 2, 3 };
        arr.setAllDefault();
        AssertEqual(0, arr[0], "setAllDefault[0]=0");
        AssertEqual(0, arr[2], "setAllDefault[2]=0");

        arr.setAllValue(77);
        AssertEqual(77, arr[0], "setAllValue[0]=77");
        AssertEqual(77, arr[2], "setAllValue[2]=77");

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
        AssertEqual(10, arr[0], "setRange list[0]=10");
        AssertEqual(40, arr[3], "setRange list[3]=40");
    }

    // ─── For / ForI ──────────────────────────────────────────────────────
    private static void testForAndForI()
    {
        int[] arr = { 1, 2, 3 };
        int sum = 0;
        arr.For(x => sum += x);
        AssertEqual(6, sum, "For sum=6");

        int idxSum = 0;
        arr.ForI(i => idxSum += i);
        AssertEqual(3, idxSum, "ForI idxSum=0+1+2=3");

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
        AssertEqual(3, even, "count even=3");

        int above10 = arr.count(x => x > 10);
        AssertEqual(0, above10, "count >10 = 0");
    }

    // ─── bytesToString ───────────────────────────────────────────────────
    private static void testBytesToString()
    {
        byte[] bytes = Encoding.UTF8.GetBytes("Hello");
        string s = bytes.bytesToString();
        AssertEqual("Hello", s, "bytesToString UTF8");

        // 带 count 参数
        string s2 = bytes.bytesToString(3);
        AssertEqual("Hel", s2, "bytesToString count=3");

        // 带 startIndex + count
        string s3 = bytes.bytesToString(1, 3);
        AssertEqual("ell", s3, "bytesToString offset=1 count=3");

        // 空数组
        byte[] empty = new byte[0];
        string s4 = empty.bytesToString();
        AssertEqual("", s4, "bytesToString empty=''");

        // null
        byte[] nullArr = null;
        string s5 = nullArr.bytesToString();
        AssertEqual("", s5, "bytesToString null=''");
    }

    // ─── safe ─────────────────────────────────────────────────────────────
    private static void testSafe()
    {
        int[] nullArr = null;
        int[] safe = nullArr.safe();
        AssertNotNull(safe, "safe null → non-null");
        AssertEqual(0, safe.count(), "safe null → empty array");

        int[] arr = { 1, 2 };
        int[] safe2 = arr.safe();
        Assert(safe2 != null, "safe non-null → not null");
        AssertEqual(2, safe2.count(), "safe non-null → same count");
    }

    // ─── EmptyArray ───────────────────────────────────────────────────────
    private static void testEmptyArray()
    {
        int[] e1 = EmptyArray<int>.getEmptyList();
        int[] e2 = EmptyArray<int>.getEmptyList();
        AssertNotNull(e1, "EmptyArray not null");
        AssertEqual(0, e1.Length, "EmptyArray length=0");
        // 单例：两次调用返回同一个对象
        Assert(e1 == e2, "EmptyArray 单例");
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