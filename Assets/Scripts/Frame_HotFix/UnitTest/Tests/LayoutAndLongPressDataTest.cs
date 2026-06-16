using static TestAssert;

public static class LayoutAndLongPressDataTest
{
	public static void Run()
	{
		testLayoutInfoCallbacksAndReset();
		testLayoutLoadInfoReset();
		testLayoutRegisteInfoFields();
		testLongPressProgressFinishInterruptAndReset();
	}
	private static void testLayoutInfoCallbacksAndReset()
	{
		LayoutInfo info = new();
		int callbackCount = 0;
		info.mOrderType = LAYOUT_ORDER.FIXED;
		info.mType = typeof(LayoutAndLongPressDataTest);
		info.mName = "TestLayout";
		info.mIsScene = true;
		info.mRenderOrder = 99;
		info.addCallback(layout => ++callbackCount);
		info.addCallback(layout => ++callbackCount);
		info.callAll(null);
		assertEqual(2, callbackCount, "LayoutInfo.callAll 应调用全部回调");
		info.callAll(null);
		assertEqual(2, callbackCount, "LayoutInfo.callAll 后回调列表应被移空");
		info.resetProperty();
		assertEqual(LAYOUT_ORDER.AUTO, info.mOrderType);
		assertNull(info.mType);
		assertNull(info.mName);
		assertFalse(info.mIsScene);
		assertEqual(0, info.mRenderOrder);
	}
	private static void testLayoutLoadInfoReset()
	{
		LayoutLoadInfo info = new();
		info.mType = typeof(LayoutAndLongPressDataTest);
		info.mOrder = 10;
		info.mOrderType = LAYOUT_ORDER.FIXED;
		info.mIsScene = true;
		info.resetProperty();
		assertNull(info.mLayout);
		assertNull(info.mType);
		assertEqual(0, info.mOrder);
		assertEqual(LAYOUT_ORDER.ALWAYS_TOP, info.mOrderType);
		assertFalse(info.mIsScene);
	}
	private static void testLayoutRegisteInfoFields()
	{
		bool called = false;
		LayoutRegisteInfo info = new()
		{
			mScriptType = typeof(LayoutScript),
			mLifeCycle = LAYOUT_LIFE_CYCLE.PERSIST,
			mCallback = script => called = true,
		};
		assertEqual(typeof(LayoutScript), info.mScriptType);
		assertEqual(LAYOUT_LIFE_CYCLE.PERSIST, info.mLifeCycle);
		info.mCallback(null);
		assertTrue(called);
	}
	private static void testLongPressProgressFinishInterruptAndReset()
	{
		LongPressData data = new();
		int longPressCount = 0;
		float lastProgress = -1.0f;
		data.mLongPressTime = 2.0f;
		data.mOnLongPress = () => ++longPressCount;
		data.mOnLongPressing = progress => lastProgress = progress;
		data.update(0.5f);
		assertEqual(0, longPressCount);
		assertTrue(lastProgress > 0.24f && lastProgress < 0.26f, "长按进度应为 pressedTime / mLongPressTime");
		data.update(2.5f);
		assertEqual(1, longPressCount, "达到阈值后触发长按");
		assertTrue(data.mFinish);
		data.update(3.0f);
		assertEqual(1, longPressCount, "mFinish 后不应重复触发");
		data.reset();
		assertFalse(data.mFinish);
		data.update(-1.0f);
		assertTrue(data.mFinish, "pressedTime 为负表示中断");
		assertEqual(0.0f, lastProgress);
		data.resetProperty();
		assertNull(data.mOnLongPress);
		assertNull(data.mOnLongPressing);
		assertEqual(0.0f, data.mLongPressTime);
		assertFalse(data.mFinish);
	}
}