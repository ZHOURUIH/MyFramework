using static TestAssert;

// FormItemParam 表单字段参数测试
public static class FormItemParamTest
{
    public static void Run()
    {
        testConstructor();
        testResetProperty();
    }

    static void testConstructor()
    {
        FormItemParam param = new FormItemParam("key1", "value1");
        assertEqual("key1", param.mKey, "key");
        assertEqual("value1", param.mValue, "value");
    }

    static void testResetProperty()
    {
        FormItemParam param = new FormItemParam("key1", "value1");
        param.resetProperty();
        assertNull(param.mKey, "reset key=null");
        assertNull(param.mValue, "reset value=null");
    }
}
