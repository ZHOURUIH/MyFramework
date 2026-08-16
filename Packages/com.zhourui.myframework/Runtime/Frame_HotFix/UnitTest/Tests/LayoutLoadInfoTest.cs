using System;
using static TestAssert;
using static FrameUtility;

// LayoutLoadInfo 布局加载信息测试(ClassObject, resetProperty 清空字段)
public static class LayoutLoadInfoTest
{
	public static void Run()
	{
		testClassCreate();
		testFieldsAssignable();
		testResetPropertyClears();
		testResetPropertyTwice();
		testSetAllFieldsThenReset();
		testDestroyRecycles();
	}

	// CLASS 从池创建
	private static void testClassCreate()
	{
		LayoutLoadInfo info = CLASS<LayoutLoadInfo>();
		assertNotNull(info, "CLASS 创建非 null");
		UN_CLASS(ref info);
	}

	// 字段赋值读回
	private static void testFieldsAssignable()
	{
		LayoutLoadInfo info = CLASS<LayoutLoadInfo>();
		info.mType = typeof(TestLayoutScript);
		info.mOrder = 7;
		info.mOrderType = LAYOUT_ORDER.FIXED;
		info.mIsScene = true;
		assertEqual(typeof(TestLayoutScript), info.mType, "mType 读回");
		assertEqual(7, info.mOrder, "mOrder 读回");
		assertEqual(LAYOUT_ORDER.FIXED, info.mOrderType, "mOrderType 读回");
		assertTrue(info.mIsScene, "mIsScene 读回");
		UN_CLASS(ref info);
	}

	// resetProperty 清空
	private static void testResetPropertyClears()
	{
		LayoutLoadInfo info = CLASS<LayoutLoadInfo>();
		info.mType = typeof(TestLayoutScript);
		info.mOrder = 7;
		info.mIsScene = true;
		info.resetProperty();
		assertNull(info.mType, "reset 后 mType null");
		assertEqual(0, info.mOrder, "reset 后 mOrder 0");
		assertNull(info.mLayout, "reset 后 mLayout null");
		assertFalse(info.mIsScene, "reset 后 mIsScene false");
		assertEqual(LAYOUT_ORDER.ALWAYS_TOP, info.mOrderType, "reset 后 mOrderType ALWAYS_TOP");
		UN_CLASS(ref info);
	}

	// resetProperty 两次
	private static void testResetPropertyTwice()
	{
		LayoutLoadInfo info = CLASS<LayoutLoadInfo>();
		info.resetProperty();
		info.resetProperty();
		assertNull(info.mType, "两次 reset 后 mType null");
		UN_CLASS(ref info);
	}

	// 全字段设置后 reset 恢复默认
	private static void testSetAllFieldsThenReset()
	{
		LayoutLoadInfo info = CLASS<LayoutLoadInfo>();
		info.mLayout = new GameLayout();
		info.mType = typeof(TestLayoutScriptDeep);
		info.mOrder = 99;
		info.mOrderType = LAYOUT_ORDER.ALWAYS_TOP_AUTO;
		info.mIsScene = true;
		info.resetProperty();
		assertNull(info.mLayout, "reset 后 mLayout null");
		assertNull(info.mType, "reset 后 mType null");
		assertEqual(LAYOUT_ORDER.ALWAYS_TOP, info.mOrderType, "reset 后 mOrderType ALWAYS_TOP");
		UN_CLASS(ref info);
	}

	// destroy/UN_CLASS 回收
	private static void testDestroyRecycles()
	{
		LayoutLoadInfo info = CLASS<LayoutLoadInfo>();
		UN_CLASS(ref info);
		assertNull(info, "UN_CLASS 后外部引用置 null");
	}
}
