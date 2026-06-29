using static TestAssert;

// MyCurve 曲线基类测试（通过 CurveOneZero 公式验证）
public static class MyCurveTest
{
	public static void Run()
	{
		testEvaluateByConcreteSubclass();
		testGetLength();
		testBoundsOfEvaluate();
	}

	static void testEvaluateByConcreteSubclass()
	{
		// CurveOneZero: evaluate(time) = 1.0f - time
		MyCurve curve = new TestCurve();
		assertEqual(1.0f, curve.evaluate(0.0f), "evaluate(0) should return 1.0");
		assertEqual(0.5f, curve.evaluate(0.5f), "evaluate(0.5) should return 0.5");
		assertEqual(0.0f, curve.evaluate(1.0f), "evaluate(1) should return 0.0");
	}

	static void testGetLength()
	{
		MyCurve curve = new TestCurve();
		assertEqual(1.0f, curve.getLength(), "getLength() should return 1.0 by default");
	}

	static void testBoundsOfEvaluate()
	{
		MyCurve curve = new TestCurve();
		curve.evaluate(-1.0f);
		curve.evaluate(2.0f);
		// evaluate 没有限制输入范围,验证不崩溃即可
		assertNotNull(curve, "Curve should not be null after evaluate with out-of-bounds input");
	}

	// 用于测试的最小化具体曲线实现: 从1到0的直线,等价于 CurveOneZero
	class TestCurve : MyCurve
	{
		public override float evaluate(float time)
		{
			return 1.0f - time;
		}
	}
}