using System;
using static TestAssert;

// LayoutRegisteInfo 布局注册信息测试(结构体, 3 字段)
public static class LayoutRegisteInfoTest
{
	public static void Run()
	{
		testDefaultValues();
		testAssignFields();
		testStructCopyIndependent();
		testCallbackAssignable();
	}

	// 默认字段值
	private static void testDefaultValues()
	{
		LayoutRegisteInfo info = new LayoutRegisteInfo();
		assertNull(info.mScriptType, "默认 mScriptType null");
		assertNull(info.mCallback, "默认 mCallback null");
	}

	// 赋值读回
	private static void testAssignFields()
	{
		LayoutRegisteInfo info = new LayoutRegisteInfo();
		info.mScriptType = typeof(TestLayoutScript);
		info.mLifeCycle = LAYOUT_LIFE_CYCLE.PERSIST;
		assertEqual(typeof(TestLayoutScript), info.mScriptType, "mScriptType 读回");
		assertEqual(LAYOUT_LIFE_CYCLE.PERSIST, info.mLifeCycle, "mLifeCycle 读回");
	}

	// 结构体复制独立
	private static void testStructCopyIndependent()
	{
		LayoutRegisteInfo a = new LayoutRegisteInfo();
		a.mScriptType = typeof(TestLayoutScript);
		LayoutRegisteInfo b = a;
		b.mScriptType = typeof(TestLayoutScriptDeep);
		assertEqual(typeof(TestLayoutScript), a.mScriptType, "a 不受 b 修改影响");
		assertEqual(typeof(TestLayoutScriptDeep), b.mScriptType, "b 独立");
	}

	// 回调赋值
	private static void testCallbackAssignable()
	{
		LayoutRegisteInfo info = new LayoutRegisteInfo();
		bool called = false;
		info.mCallback = (layout) => { called = true; };
		info.mCallback(new TestLayoutScript());
		assertTrue(called, "回调被调用");
	}
}
