using static TestAssert;

public static class LayoutScriptExtensionTest
{
    public static void Run()
    {
        testSafeNullScript();
        testSafeNonNullNotVisible();
    }

    // ─── safe<T>: null 脚本返回 null ─────────────────────────────────
    static void testSafeNullScript()
    {
        TestLayoutScript script = null;
        var result = script.safe();
        assertNull(result, "safe null script returns null");
    }

    // ─── safe<T>: 不可见脚本返回 null ─────────────────────────────────
    static void testSafeNonNullNotVisible()
    {
        var script = new TestLayoutScript();
        // 没有 setLayout → isVisible() 可能抛异常或返回 false
        // 先设置一个 layout 来触发 isVisible=false 的路径
        script.setLayout(new GameLayout());
        // GameLayout 默认 mRoot=null → isVisible() 返回 false
        var result = script.safe();
        assertNull(result, "safe not visible returns null");
    }

    // ─── safe<T>: 可见脚本返回自身 ────────────────────────────────────
}
