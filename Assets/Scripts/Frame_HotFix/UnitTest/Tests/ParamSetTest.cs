#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using System.Collections.Generic;
using static TestAssert;

public class ParamSetTest
{
	public static void Run()
	{
		testRegisterAndCount();
		testSetParamString();
		testSetParamFloat();
		testInitFromParam();
		testResetProperty();
	}

	// 测试注册回调和获取数量
	private static void testRegisterAndCount()
	{
		ParamSet paramSet = new ParamSet();
		int stringCallCount = 0;
		int floatCallCount = 0;

		paramSet.registeParam((string s) => { stringCallCount++; });
		paramSet.registeParam((float f) => { floatCallCount++; });

		assertEqual(2, paramSet.getParamCount());
	}

	// 测试字符串参数设置
	private static void testSetParamString()
	{
		ParamSet paramSet = new ParamSet();
		string receivedValue = null;
		paramSet.registeParam((string s) => { receivedValue = s; });

		bool result = paramSet.setParam(0, "test_value");
		assertTrue(result);
		assertEqual("test_value", receivedValue);
	}

	// 测试浮点数参数设置
	private static void testSetParamFloat()
	{
		ParamSet paramSet = new ParamSet();
		float receivedFloat = 0f;
		string receivedString = null;
		paramSet.registeParam((float f) => { receivedFloat = f; });
		paramSet.registeParam((string s) => { receivedString = s; });

		bool result = paramSet.setParam(0, 3.14f, "unused");
		assertTrue(result);
		assertTrue(Math.Abs(receivedFloat - 3.14f) < 0.001f, $"Expected ~3.14 but got {receivedFloat}");
	}

	// 测试从列表初始化
	private static void testInitFromParam()
	{
		ParamSet paramSet = new ParamSet();
		List<string> receivedValues = new List<string>();
		paramSet.registeParam((string s) => { receivedValues.Add(s); });
		paramSet.registeParam((float f) => { });

		List<string> paramList = new List<string> { "a", "b" };
		paramSet.initFromParam(paramList);

		// 只有 string 回调会往 receivedValues 加内容，float 回调是空的
		assertEqual(1, receivedValues.Count);
		assertEqual("a", receivedValues[0]);
		// 第二个参数会调用 float 回调，但我们的 float 回调什么都不做
	}

	// 测试重置属性
	private static void testResetProperty()
	{
		ParamSet paramSet = new ParamSet();
		paramSet.registeParam((string s) => { });
		assertEqual(1, paramSet.getParamCount());

		paramSet.resetProperty();
		assertEqual(0, paramSet.getParamCount());
	}
}
#endif