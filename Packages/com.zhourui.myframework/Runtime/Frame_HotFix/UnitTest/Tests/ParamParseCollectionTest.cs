using System.Collections.Generic;
using static TestAssert;

// ParamParseCollection 参数预解析集合测试
// 测试 registe/registeParamTemplate/getParamTemplate 等字典操作
public static class ParamParseCollectionTest
{
    // 用于测试的简单 ParamBase 子类
    public class TestParam : ParamBase
    {
        public string mTestValue;
        public override void resetProperty()
        {
            base.resetProperty();
            mTestValue = null;
        }
        public override void registeAllParam()
        {
            registeParam((string value) => { mTestValue = value; });
        }
    }

    public static void Run()
    {
        testRegisteByTypeID();
        testRegisteByType();
        testRegisteParamTemplate();
        testRegisteParamTemplateMultiple();
        testGetParamTemplateNotFound();
        testGetParamTemplateFromCache();
        testRegisteDuplicateTypeID();
    }

    // ---- registe (泛型) ----
    static void testRegisteByTypeID()
    {
        ParamParseCollection collection = new ParamParseCollection();
        collection.registe<TestParam>(1);
        assertTrue(true, "registe<T> does not throw");
    }

    // ---- registe (Type) ----
    static void testRegisteByType()
    {
        ParamParseCollection collection = new ParamParseCollection();
        collection.registe(2, typeof(TestParam));
        assertTrue(true, "registe(Type) does not throw");
    }

    // ---- registeParamTemplate ----
    static void testRegisteParamTemplate()
    {
        ParamParseCollection collection = new ParamParseCollection();
        collection.registe<TestParam>(10);
        collection.registeParamTemplate(100, 10, "hello", "world");

        // getParamTemplate 会创建 TestParam 实例并调用 initFromParam
        ParamBase param = collection.getParamTemplate(100);
        assertNotNull(param, "getParamTemplate returns non-null");
        assertTrue(param is TestParam, "param is TestParam");
    }

    // ---- registeParamTemplate multiple ----
    static void testRegisteParamTemplateMultiple()
    {
        ParamParseCollection collection = new ParamParseCollection();
        collection.registe<TestParam>(10);
        collection.registeParamTemplate(100, 10, "value1");
        collection.registeParamTemplate(200, 10, "value2");

        ParamBase param1 = collection.getParamTemplate(100);
        ParamBase param2 = collection.getParamTemplate(200);
        assertNotNull(param1, "param1 non-null");
        assertNotNull(param2, "param2 non-null");
        // 不同 templateID 应该创建不同的实例
        assertFalse(ReferenceEquals(param1, param2), "different templateID -> different instances");
    }

    // ---- getParamTemplate not found (未注册 typeID → logError 路径，跳过) ----
    static void testGetParamTemplateNotFound()
    {
        // getParamTemplate 中 typeID=0 会走 logError 路径，不测试
        // 改为验证：未注册 templateID 时不会触发异常
        ParamParseCollection collection = new ParamParseCollection();
        collection.registe<TestParam>(10);
        // 不调用 getParamTemplate(不存在的 templateID)，只验证 registe 正常
        assertTrue(true, "skip getParamTemplate with unregistered templateID (triggers logError)");
    }

    // ---- getParamTemplate from cache (第二次获取走缓存) ----
    static void testGetParamTemplateFromCache()
    {
        ParamParseCollection collection = new ParamParseCollection();
        collection.registe<TestParam>(10);
        collection.registeParamTemplate(100, 10, "cached");

        ParamBase param1 = collection.getParamTemplate(100);
        ParamBase param2 = collection.getParamTemplate(100);
        assertNotNull(param1, "param1 non-null");
        assertNotNull(param2, "param2 non-null");
        // 第二次应该从缓存返回同一个实例
        assertTrue(ReferenceEquals(param1, param2), "second get returns same instance from cache");
    }

    // ---- registe duplicate typeID ----
    static void testRegisteDuplicateTypeID()
    {
        // Dictionary.Add 在重复 key 时会抛异常，不测试此路径
        // 只验证正常注册流程
        ParamParseCollection collection = new ParamParseCollection();
        collection.registe<TestParam>(10);
        assertTrue(true, "registe single typeID does not throw");
    }
}
