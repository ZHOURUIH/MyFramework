using System;
using static TestAssert;

// Frame_Game 精简层 LayoutInfo 结构体测试(3 字段)
public static class LayoutInfoTest
{
	public static void Run()
	{
		testDefaultValues();
		testAssignFields();
		testStructCopyIndependent();
		testCallbackInvoke();
	}

	// 默认字段
	static void testDefaultValues()
	{
		LayoutInfo info = new LayoutInfo();
		assertNull(info.mType, "默认 mType null");
		assertNull(info.mCallback, "默认 mCallback null");
		assertEqual(0, info.mRenderOrder, "默认渲染顺序 0");
	}

	// 赋值读回
	static void testAssignFields()
	{
		LayoutInfo info = new LayoutInfo();
		info.mType = typeof(GameLayout);
		info.mRenderOrder = 5;
		assertEqual(typeof(GameLayout), info.mType, "mType 读回");
		assertEqual(5, info.mRenderOrder, "渲染顺序读回");
	}

	// 结构体复制独立(值语义)
	static void testStructCopyIndependent()
	{
		LayoutInfo a = new LayoutInfo();
		a.mRenderOrder = 3;
		LayoutInfo b = a;
		b.mRenderOrder = 9;
		assertEqual(3, a.mRenderOrder, "a 不受 b 修改影响");
		assertEqual(9, b.mRenderOrder, "b 独立");
	}

	// 回调赋值触发(GameLayoutCallback)
	static void testCallbackInvoke()
	{
		LayoutInfo info = new LayoutInfo();
		bool called = false;
		info.mCallback = (layout) => { called = true; };
		info.mCallback(new GameLayout());
		assertTrue(called, "回调被调用");
	}
}
