using static TestAssert;
using static FrameUtility;

// LayoutLoadGroup 中可脱离真实布局加载的逻辑
public static class LayoutLoadGroupTest
{
    public static void Run()
    {
        testCreate();
        testProgressAndAllLoaded();
        testAddLayoutRegisters();
        testAddSceneUIRegisters();
        testAddTopLayoutRegisters();
        testResetPropertyClears();
        testStartLoadEmpty();
        testMultipleAddsNotLoaded();
        testDestroySafe();
    }

    // create: 从对象池创建 LayoutLoadGroup
    static void testCreate()
    {
        LayoutLoadGroup group = LayoutLoadGroup.create();
        assertTrue(group != null, "create not null");
        UN_CLASS(ref group);

        LayoutLoadGroup group2 = LayoutLoadGroup.create(false);
        assertTrue(group2 != null, "create(false) not null");
        UN_CLASS(ref group2);
    }

    // getProgress / isAllLoaded: 空组时 progress 为 0, 且视为已加载
    static void testProgressAndAllLoaded()
    {
        LayoutLoadGroup group = LayoutLoadGroup.create();
        try
        {
            // 空组: mLoadInfo.Count == 0 -> getProgress 为 0.0
            assertTrue(group.getProgress() >= 0.0f, "empty group progress >= 0");
            assertTrue(group.isAllLoaded(), "empty group is all loaded");
        }
        finally
        {
            UN_CLASS(ref group);
        }
    }

    // ═════════════════════════════════════════════════════════════════
    // 深度组合
    // ═════════════════════════════════════════════════════════════════

    // addLayout(Type) 注册后非 all loaded(未加载完成)
    static void testAddLayoutRegisters()
    {
        LayoutLoadGroup group = LayoutLoadGroup.create();
        try
        {
            group.addLayout(typeof(TestLayoutScript));
            assertFalse(group.isAllLoaded(), "注册 1 个后未全部加载");
            assertEqual(0.0f, group.getProgress(), 0.001f, "progress 仍 0");
        }
        finally
        {
            UN_CLASS(ref group);
        }
    }

    // addSceneUI 注册
    static void testAddSceneUIRegisters()
    {
        LayoutLoadGroup group = LayoutLoadGroup.create();
        try
        {
            group.addSceneUI(typeof(TestLayoutScript));
            assertFalse(group.isAllLoaded(), "addSceneUI 注册后未全部加载");
        }
        finally
        {
            UN_CLASS(ref group);
        }
    }

    // addTopLayout 注册
    static void testAddTopLayoutRegisters()
    {
        LayoutLoadGroup group = LayoutLoadGroup.create();
        try
        {
            group.addTopLayout<TestLayoutScript>();
            assertFalse(group.isAllLoaded(), "addTopLayout 注册后未全部加载");
        }
        finally
        {
            UN_CLASS(ref group);
        }
    }

    // resetProperty 清空注册
    static void testResetPropertyClears()
    {
        LayoutLoadGroup group = LayoutLoadGroup.create();
        try
        {
            group.addLayout(typeof(TestLayoutScript));
            assertFalse(group.isAllLoaded(), "注册后非全加载");
            group.resetProperty();
            assertTrue(group.isAllLoaded(), "resetProperty 后清空 → 全加载");
        }
        finally
        {
            UN_CLASS(ref group);
        }
    }

    // 空组 startLoad 返回 op 不崩
    static void testStartLoadEmpty()
    {
        LayoutLoadGroup group = LayoutLoadGroup.create();
        try
        {
            CustomAsyncOperation op = group.startLoad(null);
            assertNotNull(op, "空组 startLoad 返回 op");
        }
        finally
        {
            UN_CLASS(ref group);
        }
    }

    // 多个注册(不同 Type) → 非 all loaded
    static void testMultipleAddsNotLoaded()
    {
        LayoutLoadGroup group = LayoutLoadGroup.create();
        try
        {
            group.addLayout(typeof(TestLayoutScript));
            group.addSceneUI(typeof(TestLayoutScriptDeep));
            assertFalse(group.isAllLoaded(), "2 个注册未全部加载");
            assertEqual(0.0f, group.getProgress(), 0.001f, "progress 0");
        }
        finally
        {
            UN_CLASS(ref group);
        }
    }

    // 注: 同 Type 重复 add 会抛 ArgumentException(Dictionary.add = 原生 Add)——框架行为, 合法不测

    // destroy 直接销毁(内部 base.destroy + UN_CLASS_LIST 清空列表)
    static void testDestroySafe()
    {
        LayoutLoadGroup group = LayoutLoadGroup.create();
        group.addLayout(typeof(TestLayoutScript));
        group.destroy();
        // 无异常即通过
    }
}
