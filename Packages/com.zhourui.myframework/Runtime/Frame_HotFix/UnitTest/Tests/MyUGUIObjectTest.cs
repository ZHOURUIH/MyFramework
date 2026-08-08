using static TestAssert;

// myUGUIObject 中纯静态/轻量的方法
public static class MyUGUIObjectTest
{
    public static void Run()
    {
        testDefaultClickSound();
        testDestroyWindowNull();
    }

    // destroyWindow / destroyWindowSingle: null 窗口直接安全返回
    // (完整销毁链路涉及对象池与全局系统交互, 风险高, 此处只测 null 分支确保稳定)
    static void testDestroyWindowNull()
    {
        myUGUIObject.destroyWindow(null, false);
        myUGUIObject.destroyWindow(null, true);
        myUGUIObject.destroyWindowSingle(null, false);
        myUGUIObject.destroyWindowSingle(null, true);
        assertTrue(true, "destroyWindow/destroyWindowSingle null 分支调用成功");
    }

    // setDefaultClickSound / getDefaultClickSound: 静态 int 字段读写
    // 测试后会恢复原值,避免污染全局点击音效状态
    static void testDefaultClickSound()
    {
        int original = myUGUIObject.getDefaultClickSound();
        try
        {
            myUGUIObject.setDefaultClickSound(0);
            assertEqual(0, myUGUIObject.getDefaultClickSound(), "set/get 0");

            myUGUIObject.setDefaultClickSound(123);
            assertEqual(123, myUGUIObject.getDefaultClickSound(), "set/get 123");

            myUGUIObject.setDefaultClickSound(-7);
            assertEqual(-7, myUGUIObject.getDefaultClickSound(), "set/get -7");
        }
        finally
        {
            myUGUIObject.setDefaultClickSound(original);
        }
    }
}
