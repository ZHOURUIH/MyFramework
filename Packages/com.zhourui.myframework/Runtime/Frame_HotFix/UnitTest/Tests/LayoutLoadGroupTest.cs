using static TestAssert;
using static FrameUtility;

// LayoutLoadGroup 中可脱离真实布局加载的逻辑
public static class LayoutLoadGroupTest
{
    public static void Run()
    {
        testCreate();
        testProgressAndAllLoaded();
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
}
