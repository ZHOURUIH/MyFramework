#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;

// ClassObject 生命周期测试
// 验证 isPendingDestroy / setPendingDestroy / isDestroy / setDestroy /
// resetProperty / getObjectInstanceID / getAssignID 等行为
public static class ClassObjectTest
{
    public static void Run()
    {
        testDefaultState();
        testPendingDestroy();
        testSetDestroy();
        testResetProperty();
        testInstanceIDUnique();
        testAssignID();
        testEquals();
    }

    // ─── 默认状态 ────────────────────────────────────────────────────────

    private static void testDefaultState()
    {
        // 直接 new 出来的对象：mHasDestroy = true，mPendingDestroy = false
        var obj = new TestClassObj();
        Assert(obj.isDestroy(),         "默认状态: isDestroy 应为 true");
        Assert(!obj.isPendingDestroy(), "默认状态: isPendingDestroy 应为 false");
        AssertEqual(0L, obj.getAssignID(), "默认状态: assignID 应为 0");
    }

    // ─── PendingDestroy ──────────────────────────────────────────────────

    private static void testPendingDestroy()
    {
        var obj = new TestClassObj();
        obj.setPendingDestroy(true);
        Assert(obj.isPendingDestroy(),  "setPendingDestroy(true): isPendingDestroy 应为 true");

        obj.setPendingDestroy(false);
        Assert(!obj.isPendingDestroy(), "setPendingDestroy(false): isPendingDestroy 应为 false");
    }

    // ─── setDestroy ──────────────────────────────────────────────────────

    private static void testSetDestroy()
    {
        var obj = new TestClassObj();
        obj.setDestroy(false);
        Assert(!obj.isDestroy(), "setDestroy(false): isDestroy 应为 false");

        obj.setDestroy(true);
        Assert(obj.isDestroy(),  "setDestroy(true): isDestroy 应为 true");
    }

    // ─── resetProperty ───────────────────────────────────────────────────

    private static void testResetProperty()
    {
        var obj = new TestClassObj();
        obj.setDestroy(false);
        obj.setPendingDestroy(true);
        obj.setAssignID(999L);
        obj.mCustomData = 42;

        // 记录重置前的 instanceID（不应被重置）
        long instanceID = obj.getObjectInstanceID();

        obj.resetProperty();

        // 验证各字段恢复到 resetProperty 约定值
        AssertEqual(0L,   obj.getAssignID(),      "resetProperty: assignID 应归零");
        Assert(obj.isDestroy(),                    "resetProperty: mHasDestroy 应为 true");
        Assert(!obj.isPendingDestroy(),            "resetProperty: mPendingDestroy 应为 false");
        AssertEqual(instanceID, obj.getObjectInstanceID(), "resetProperty: instanceID 不应被重置");
        AssertEqual(0, obj.mCustomData,            "resetProperty: 子类自定义字段应被清零");
    }

    // ─── InstanceID 唯一性 ───────────────────────────────────────────────

    private static void testInstanceIDUnique()
    {
        var a = new TestClassObj();
        var b = new TestClassObj();
        var c = new TestClassObj();
        Assert(a.getObjectInstanceID() != b.getObjectInstanceID(), "InstanceID: a 与 b 应不同");
        Assert(b.getObjectInstanceID() != c.getObjectInstanceID(), "InstanceID: b 与 c 应不同");
        Assert(a.getObjectInstanceID() != c.getObjectInstanceID(), "InstanceID: a 与 c 应不同");
    }

    // ─── AssignID ────────────────────────────────────────────────────────

    private static void testAssignID()
    {
        var obj = new TestClassObj();
        obj.setAssignID(12345L);
        AssertEqual(12345L, obj.getAssignID(), "setAssignID: getAssignID 应返回设置值");

        obj.setAssignID(0L);
        AssertEqual(0L, obj.getAssignID(), "setAssignID(0): getAssignID 应返回 0");
    }

    // ─── Equals ──────────────────────────────────────────────────────────

    private static void testEquals()
    {
        var a = new TestClassObj();
        var b = new TestClassObj();
        Assert(!a.Equals(b), "Equals: 不同实例 instanceID 不同，应不相等");
        Assert(a.Equals(a),  "Equals: 同一实例自比，应相等");
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
}

// 测试专用 ClassObject 子类，带一个自定义字段验证 resetProperty
public class TestClassObj : ClassObject
{
    public int mCustomData;
    public override void resetProperty()
    {
        base.resetProperty();
        mCustomData = 0;
    }
}
#endif