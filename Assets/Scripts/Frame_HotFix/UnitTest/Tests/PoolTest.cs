#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using static FrameUtility;

// ClassPool / ListPool / ByteArrayPool 集成测试
// 框架完整初始化后通过 FrameUtility 工具函数调用真实 Pool 系统
public static class PoolTest
{
    public static void Run()
    {
        testClassPool_NewAndDestroy();
        testClassPool_Reuse();
        testListPool_NewAndDestroy();
        testByteArrayPool_AllocAndFree();
    }

    // ─── ClassPool ────────────────────────────────────────────────────────

    private static void testClassPool_NewAndDestroy()
    {
        var obj = CLASS<TestPoolObj>();
        AssertNotNull(obj, "CLASS<T>: 不应返回 null");
        obj.mData = 42;

        UN_CLASS(ref obj);
        AssertNull(obj, "UN_CLASS: 归还后引用应为 null");
    }

    private static void testClassPool_Reuse()
    {
        // 申请 -> 归还 -> 再申请，复用对象的数据应已被 resetProperty 清零
        var a = CLASS<TestPoolObj>();
        a.mData = 99;
        UN_CLASS(ref a);

        var b = CLASS<TestPoolObj>();
        AssertEqual(0, b.mData, "CLASS<T> 复用: resetProperty 后 mData 应归零");
        UN_CLASS(ref b);
    }

    // ─── ListPool ─────────────────────────────────────────────────────────

    private static void testListPool_NewAndDestroy()
    {
        var list = LIST<int>();
        AssertNotNull(list, "LIST<T>: 不应返回 null");
        list.Add(1);
        list.Add(2);
        list.Add(3);

        UN_LIST(ref list);
        AssertNull(list, "UN_LIST: 归还后引用应为 null");
    }

    // ─── ByteArrayPool ────────────────────────────────────────────────────

    private static void testByteArrayPool_AllocAndFree()
    {
        byte[] arr = ARRAY_BYTE(128);
        AssertNotNull(arr, "BYTE_ARRAY: 不应返回 null");
        AssertEqual(128, arr.Length, "BYTE_ARRAY: 长度应为 128");

        // 填充数据
        for (int i = 0; i < arr.Length; i++)
        {
			arr[i] = (byte)(i % 256);
		}

		UN_ARRAY_BYTE(ref arr);
        AssertNull(arr, "UN_BYTE_ARRAY: 归还后引用应为 null");
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

    private static void AssertNull(object obj, string message = "")
    {
        if (obj != null)
        {
            throw new Exception(string.IsNullOrEmpty(message) ? "Object should be null" : message);
        }
    }
}

// 测试专用池化对象
public class TestPoolObj : ClassObject
{
    public int mData;
    public override void resetProperty()
    {
        base.resetProperty();
        mData = 0;
    }
}
#endif