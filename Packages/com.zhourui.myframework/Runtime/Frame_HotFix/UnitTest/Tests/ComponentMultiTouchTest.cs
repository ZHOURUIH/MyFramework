using UnityEngine;
using static TestAssert;

// ComponentMultiTouch 深度测试(多触点手势组件)
//   resetProperty: 清空触点/回调/手势, 恢复默认阈值
//   setter 链: 移动/缩放/旋转回调 + 各阈值
//   update 依赖 mInputSystem 设备输入, 不测
// 环境: new ComponentMultiTouch()(GameComponent, 无参构造)
public static class ComponentMultiTouchTest
{
	public static void Run()
	{
		testResetDefaults();
		testMoveCallbackSetter();
		testScaleCallbackSetter();
		testRotateCallbackSetter();
		testThresholdSetters();
	}

	// ═════════════════════════════════════════════════════════════════
	// 辅助
	// ═════════════════════════════════════════════════════════════════
	private static TestComponentMultiTouch createTouch()
	{
		TestComponentMultiTouch touch = new TestComponentMultiTouch();
		touch.resetProperty();
		return touch;
	}

	// resetProperty 默认值
	private static void testResetDefaults()
	{
		TestComponentMultiTouch touch = createTouch();
		assertTrue(touch.getGestureForTest() == MULTI_TOUCH_GESTURE.NONE, "默认手势 NONE");
		assertEqual(10.0f, touch.getRotateThresholdForTest(), 0.001f, "默认旋转阈值 10");
		assertEqual(50.0f, touch.getScaleThresholdForTest(), 0.001f, "默认缩放阈值 50");
		assertEqual(400.0f, touch.getMoveFingerStartDistanceThresholdForTest(), 0.001f, "默认平移起始间距 400");
		assertEqual(30.0f, touch.getMoveFingerDistanceThresholdForTest(), 0.001f, "默认平移间距变化 30");
		assertEqual(10.0f, touch.getMoveThresholdForTest(), 0.001f, "默认平移移动阈值 10");
	}

	// 平移回调 setter
	private static void testMoveCallbackSetter()
	{
		TestComponentMultiTouch touch = createTouch();
		bool called = false;
		touch.setTwoFingerMoveCallback((pos) => called = true);
		// 回调已存储(不触发, update 依赖输入)
		assertTrue(touch.getMoveCallbackForTest() != null, "平移回调已存储");
	}

	// 缩放回调 setter
	private static void testScaleCallbackSetter()
	{
		TestComponentMultiTouch touch = createTouch();
		touch.setTwoFingerScaleCallback((a, b, c) => { });
		assertTrue(touch.getScaleCallbackForTest() != null, "缩放回调已存储");
	}

	// 旋转回调 setter
	private static void testRotateCallbackSetter()
	{
		TestComponentMultiTouch touch = createTouch();
		touch.setTwoFingerRotateCallback((angle) => { });
		assertTrue(touch.getRotateCallbackForTest() != null, "旋转回调已存储");
	}

	// 阈值 setter
	private static void testThresholdSetters()
	{
		TestComponentMultiTouch touch = createTouch();
		touch.setRotateThresholdForTest(20.0f);
		assertEqual(20.0f, touch.getRotateThresholdForTest(), 0.001f, "旋转阈值修改");
		touch.setScaleThresholdForTest(80.0f);
		assertEqual(80.0f, touch.getScaleThresholdForTest(), 0.001f, "缩放阈值修改");
	}
}

// ═════════════════════════════════════════════════════════════════
// 测试辅助: 暴露 ComponentMultiTouch 的 protected 字段
// ═════════════════════════════════════════════════════════════════
public class TestComponentMultiTouch : ComponentMultiTouch
{
	public MULTI_TOUCH_GESTURE getGestureForTest() { return mGesture; }

	public float getRotateThresholdForTest() { return mRotateThreshold; }

	public float getScaleThresholdForTest() { return mScaleThreshold; }

	public float getMoveFingerStartDistanceThresholdForTest() { return mMoveFingerStartDistanceThreshold; }

	public float getMoveFingerDistanceThresholdForTest() { return mMoveFingerDistanceThreshold; }

	public float getMoveThresholdForTest() { return mMoveThreshold; }

	public void setRotateThresholdForTest(float value) { mRotateThreshold = value; }

	public void setScaleThresholdForTest(float value) { mScaleThreshold = value; }

	public Vector3Callback getMoveCallbackForTest() { return mTwoFingerMoveCallback; }

	public Float3Callback getScaleCallbackForTest() { return mTwoFingerScaleCallback; }

	public FloatCallback getRotateCallbackForTest() { return mTwoFingerRotateCallback; }
}
