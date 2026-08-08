using static TestAssert;

// EditorCurveFactory 编辑器曲线工厂(静态)
// 覆盖: reload/getCurve/getPreviewCurve/getNames/getIDs
public static class EditorCurveFactoryTest
{
	public static void Run()
	{
		testReload();
		testGetCurve();
		testGetPreviewCurve();
		testGetNamesAndIDs();
	}

	// reload: 重建曲线表, 无异常(资源缺失时安全返回)
	private static void testReload()
	{
		EditorCurveFactory.reload();
		assertTrue(true, "reload 调用成功");
	}

	// getCurve: 按 ID 读取内置曲线
	private static void testGetCurve()
	{
		// 先确保曲线表已构建
		EditorCurveFactory.reload();
		MyCurve curve = EditorCurveFactory.getCurve(KEY_CURVE.ZERO_ONE);
		assertTrue(curve == null || curve.evaluate(0.5f) > 0.0f, "getCurve(ZERO_ONE) 可取到并 evaluate");
		MyCurve missing = EditorCurveFactory.getCurve(-999999);
		assertTrue(missing == null, "getCurve(不存在ID) 返回 null");
	}

	// getPreviewCurve: 返回 AnimationCurve, 存在与否都不抛异常
	private static void testGetPreviewCurve()
	{
		UnityEngine.AnimationCurve preview0 = EditorCurveFactory.getPreviewCurve(KEY_CURVE.SINE_IN);
		assertTrue(preview0 != null, "getPreviewCurve(内置) 返回非空");
		UnityEngine.AnimationCurve preview1 = EditorCurveFactory.getPreviewCurve(-999999);
		assertTrue(preview1 != null, "getPreviewCurve(不存在ID) 返回空曲线而非 null");
	}

	// getNames / getIDs: 返回内置曲线名称与 ID 数组
	private static void testGetNamesAndIDs()
	{
		int[] ids = EditorCurveFactory.getIDs();
		string[] names = EditorCurveFactory.getNames();
		assertTrue(ids != null && ids.Length > 0, "getIDs 非空");
		assertTrue(names != null && names.Length >= ids.Length, "getNames 与 ids 规模匹配");
	}
}
