using System.Collections.Generic;
using static TestAssert;

// KeyFrameManager 关键帧曲线管理器
// 覆盖: KeyFrameManager.loadAllCalculatedCurve(静态, 向字典填充全部内置公式曲线)
public static class KeyFrameManagerTest
{
	public static void Run()
	{
		testLoadAllCalculatedCurve();
	}

	// loadAllCalculatedCurve: 静态纯填充, 无副作用, 可重复调用
	private static void testLoadAllCalculatedCurve()
	{
		Dictionary<int, MyCurve> curveList = new();
		KeyFrameManager.loadAllCalculatedCurve(curveList);

		// 内置公式曲线数量应大于 0
		assertTrue(curveList.Count > 0, "loadAllCalculatedCurve 应填充内置曲线");
		assertTrue(curveList.ContainsKey(KEY_CURVE.ZERO_ONE), "应包含 ZERO_ONE");
		assertTrue(curveList.ContainsKey(KEY_CURVE.SINE_OUT), "应包含 SINE_OUT");
		assertTrue(curveList.ContainsKey(KEY_CURVE.QUAD_IN), "应包含 QUAD_IN");

		// 取出 ZERO_ONE 曲线实例并验证其可 evaluate
		MyCurve zeroOne = curveList[KEY_CURVE.ZERO_ONE];
		assertTrue(zeroOne != null, "ZERO_ONE 曲线实例非空");
		float mid = zeroOne.evaluate(0.5f);
		assertTrue(mid > 0.0f && mid < 1.0f, "ZERO_ONE 在 0.5 处取值在 (0,1)");

		// 再次填充到独立字典, 确认不互相污染, 且无异常
		Dictionary<int, MyCurve> curveList2 = new();
		KeyFrameManager.loadAllCalculatedCurve(curveList2);
		assertTrue(curveList2.Count == curveList.Count, "两次填充数量一致");
	}
}
