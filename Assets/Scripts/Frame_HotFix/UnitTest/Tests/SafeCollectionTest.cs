#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;
using static FrameUtility;

// Safe 集合类型测试
// 覆盖：SafeList<T> / SafeDictionary<K,V> / SafeHashSet<T>
// 重点：add / remove / clear / count / contains / startForeach / endForeach
// 以及遍历过程中的增删安全性
public static class SafeCollectionTest
{
    public static void Run()
    {
        try
        {
            testSafeListBasic();
            testSafeListForeachModify();
            testSafeListClear();
            testSafeDictionaryBasic();
            testSafeDictionaryForeachModify();
            testSafeHashSetBasic();
            Console.WriteLine("SafeCollectionTest: All tests passed");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"SafeCollectionTest: Test failed - {ex.Message}");
            throw;
        }
    }

    // ─── SafeList: 基础操作 ──────────────────────────────────────────────────
    private static void testSafeListBasic()
    {
        CLASS(out SafeList<int> list);
        list.clear();

        list.add(1);
        list.add(2);
        list.add(3);

        AssertEqual(3, list.count(), "SafeList.count 应为 3");
        Assert(list.contains(2), "SafeList.contains 应找到元素");
        Assert(!list.contains(99), "SafeList.contains 不应找到不存在的元素");

        list.remove(2);
        AssertEqual(2, list.count(), "SafeList.remove 后 count 应为 2");
        Assert(!list.contains(2), "SafeList.remove 后不应包含被删元素");

        UN_CLASS(ref list);
    }

    // ─── SafeList: 遍历时修改 ─────────────────────────────────────────────────
    private static void testSafeListForeachModify()
    {
        CLASS(out SafeList<int> list);
        list.clear();

        for (int i = 0; i < 5; i++)
            list.add(i);

        int sum = 0;
        var iterList = list.startForeach();
        try
        {
            foreach (int val in iterList)
            {
                sum += val;
                // 遍历过程中添加元素（应被延迟到 endForeach 后生效）
                if (val == 2)
                {
					list.add(99);
				}
            }
        }
        finally
        {
            list.endForeach();
        }

        AssertEqual(10, sum, "SafeList 遍历: 0+1+2+3+4 = 10");
        AssertEqual(6, list.count(), "SafeList 遍历中添加的元素应在 endForeach 后生效");

        UN_CLASS(ref list);
    }

    // ─── SafeList: clear ──────────────────────────────────────────────────────
    private static void testSafeListClear()
    {
        CLASS(out SafeList<int> list);
        list.clear();

        for (int i = 0; i < 10; i++)
            list.add(i);

        AssertEqual(10, list.count(), "SafeList 添加后 count 应为 10");

        list.clear();
        AssertEqual(0, list.count(), "SafeList.clear 后 count 应为 0");

        UN_CLASS(ref list);
    }

    // ─── SafeDictionary: 基础操作 ─────────────────────────────────────────────
    private static void testSafeDictionaryBasic()
    {
        CLASS(out SafeDictionary<string, int> dict);
        dict.clear();

        dict.add("a", 1);
        dict.add("b", 2);
        dict.add("c", 3);

        AssertEqual(3, dict.count(), "SafeDictionary.count 应为 3");
        Assert(dict.containsKey("b"), "SafeDictionary.contains 应找到键");
        AssertEqual(2, dict.get("b"), "SafeDictionary.get 应返回值");

        dict.remove("b");
        AssertEqual(2, dict.count(), "SafeDictionary.remove 后 count 应为 2");
        Assert(!dict.containsKey("b"), "SafeDictionary.remove 后不应包含被删键");

        UN_CLASS(ref dict);
    }

    // ─── SafeDictionary: 遍历时修改 ────────────────────────────────────────────
    private static void testSafeDictionaryForeachModify()
    {
        CLASS(out SafeDictionary<int, string> dict);
        dict.clear();

        for (int i = 0; i < 5; i++)
            dict.add(i, $"value{i}");

        int count = 0;
        var iterDict = dict.startForeach();
        try
        {
            foreach (var kv in iterDict)
            {
                count++;
                // 遍历过程中删除元素（应被延迟到 endForeach 后生效）
                if (kv.Key == 2)
                    dict.remove(2);
            }
        }
        finally
        {
            dict.endForeach();
        }

        AssertEqual(5, count, "SafeDictionary 遍历: 应遍历所有 5 个元素");
        AssertEqual(4, dict.count(), "SafeDictionary 遍历中删除的元素应在 endForeach 后生效");

        UN_CLASS(ref dict);
    }

    // ─── SafeHashSet: 基础操作 ────────────────────────────────────────────────
    private static void testSafeHashSetBasic()
    {
        CLASS(out SafeHashSet<int> set);
        set.clear();

        set.add(1);
        set.add(2);
        set.add(3);
        set.add(2); // 重复添加

        AssertEqual(3, set.count(), "SafeHashSet.count 应为 3（去重）");
        Assert(set.contains(2), "SafeHashSet.contains 应找到元素");
        Assert(!set.contains(99), "SafeHashSet.contains 不应找到不存在的元素");

        set.remove(2);
        AssertEqual(2, set.count(), "SafeHashSet.remove 后 count 应为 2");
        Assert(!set.contains(2), "SafeHashSet.remove 后不应包含被删元素");

        UN_CLASS(ref set);
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
