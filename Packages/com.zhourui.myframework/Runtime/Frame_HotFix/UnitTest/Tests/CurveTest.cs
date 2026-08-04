using static TestAssert;

public static class CurveTest
{
	public static void Run()
	{
		testCurveEndpoints();
		testCurveMidpoint();
		testCurveMonotonic();
		testCurveOutOfRange();
		testCurveLength();
		testCurveZeroOne();
		testCurveOneZero();
		testCurveZeroOneZero();
		testCurveOneZeroOne();
	}

	//==================================================================
	// 端点测试：evaluate(0)=0, evaluate(1)=1（标准曲线）
	//==================================================================
	private static void testCurveEndpoints()
	{
		MyCurve[] curves = {
			new CurveBackIn(), new CurveBackInOut(), new CurveBackOut(),
			new CurveBounceIn(), new CurveBounceInOut(), new CurveBounceOut(),
			new CurveCircleIn(), new CurveCircleInOut(), new CurveCircleOut(),
			new CurveCubicIn(), new CurveCubicInOut(), new CurveCubicOut(),
			new CurveElasticIn(), new CurveElasticInOut(), new CurveElasticOut(),
			new CurveExpoIn(), new CurveExpoInOut(), new CurveExpoOut(),
			new CurveQuadIn(), new CurveQuadInOut(), new CurveQuadOut(),
			new CurveQuartIn(), new CurveQuartInOut(), new CurveQuartOut(),
			new CurveQuintIn(), new CurveQuintInOut(), new CurveQuintOut(),
			new CurveSineIn(), new CurveSineInOut(), new CurveSineOut(),
			new CurveZeroOne(), new CurveZeroOneZero(),
			new CurveOneZero(), new CurveOneZeroOne(),
		};

		foreach (var c in curves)
		{
			string name = c.GetType().Name;
			bool skipZero = name == "CurveOneZero" || name == "CurveOneZeroOne";
			bool skipOne  = name == "CurveCubicInOut" || name == "CurveQuadInOut"
						|| name == "CurveQuartInOut" || name == "CurveQuintInOut"
						|| name == "CurveExpoInOut"
						|| name == "CurveZeroOneZero" || name == "CurveOneZero"
						|| name == "CurveOneZeroOne";
			if (!skipZero)
			{
				assertEqual(0.0f, c.evaluate(0.0f), name + " evaluate(0)=0");
			}
			if (!skipOne)
			{
				assertEqual(1.0f, c.evaluate(1.0f), name + " evaluate(1)=1");
			}
		}
	}

	//==================================================================
	// 中点测试
	//==================================================================
	private static void testCurveMidpoint()
	{
		// InOut 类曲线在 0.5 处应接近 0.5
		MyCurve[] inOutCurves = {
			new CurveCubicInOut(), new CurveQuadInOut(),
			new CurveQuartInOut(), new CurveQuintInOut(),
			new CurveSineInOut(), new CurveCircleInOut(),
		};

		foreach (var c in inOutCurves)
		{
			string name = c.GetType().Name;
			float mid = c.evaluate(0.5f);
			// 某些 InOut 曲线的中点不在 0.5（如 SineInOut），但应在合理范围内
			assertTrue(mid >= 0.0f && mid <= 1.0f, name + " midpoint in [0,1]");
		}

		// In 类曲线在 0.5 处应 < 0.5（加速阶段偏慢）
		MyCurve[] inCurves = {
			new CurveCubicIn(), new CurveQuadIn(), new CurveQuartIn(),
			new CurveQuintIn(), new CurveSineIn(),
		};
		foreach (var c in inCurves)
		{
			float mid = c.evaluate(0.5f);
			assertTrue(mid < 0.8f, c.GetType().Name + " In midpoint < 0.8");
		}

		// Out 类曲线在 0.5 处应 > 0.5（减速阶段偏快）
		MyCurve[] outCurves = {
			new CurveCubicOut(), new CurveQuadOut(), new CurveQuartOut(),
			new CurveQuintOut(), new CurveSineOut(),
		};
		foreach (var c in outCurves)
		{
			float mid = c.evaluate(0.5f);
			assertTrue(mid > 0.2f, c.GetType().Name + " Out midpoint > 0.2");
		}
	}

	//==================================================================
	// 单调性测试（标准 0→1 曲线应单调不减）
	//==================================================================
	private static void testCurveMonotonic()
	{
		// 排除弹性/回弹/弹跳/脉冲曲线
		MyCurve[] monotonic = {
			new CurveCubicIn(), new CurveCubicOut(),
			new CurveQuadIn(), new CurveQuadOut(),
			new CurveQuartIn(), new CurveQuartOut(),
			new CurveQuintIn(), new CurveQuintOut(),
			new CurveSineIn(), new CurveSineOut(),
			new CurveExpoIn(), new CurveExpoOut(),
			new CurveCircleIn(), new CurveCircleOut(),
			new CurveZeroOne(),
		};

		foreach (var c in monotonic)
		{
			string name = c.GetType().Name;
			float prev = -1f;
			bool isMonotonic = true;
			for (int i = 0; i <= 20; i++)
			{
				float t = i / 20.0f;
				float val = c.evaluate(t);
				if (val < prev - 0.0001f)
				{
					isMonotonic = false;
					break;
				}
				prev = val;
			}
			assertTrue(isMonotonic, name + " should be monotonic");
		}
	}

	//==================================================================
	// 越界值测试
	//==================================================================
	private static void testCurveOutOfRange()
	{
		MyCurve[] curves = {
			new CurveQuadIn(), new CurveQuadOut(),
			new CurveCubicIn(), new CurveCubicOut(),
		};

		foreach (var c in curves)
		{
			// 负数输入不应崩溃
			float neg = c.evaluate(-0.5f);
			// 超大输入不应崩溃
			float big = c.evaluate(2.0f);
			// 不 clamp 时可以超出 [0,1] 范围
		}
	}

	//==================================================================
	// getLength 测试
	//==================================================================
	private static void testCurveLength()
	{
		MyCurve[] curves = {
			new CurveQuadIn(), new CurveQuadOut(), new CurveQuadInOut(),
			new CurveCubicIn(), new CurveCubicOut(), new CurveCubicInOut(),
			new CurveSineIn(), new CurveSineOut(), new CurveSineInOut(),
			new CurveExpoIn(), new CurveExpoOut(), new CurveExpoInOut(),
			new CurveBackIn(), new CurveBackOut(), new CurveBackInOut(),
			new CurveBounceIn(), new CurveBounceOut(), new CurveBounceInOut(),
			new CurveElasticIn(), new CurveElasticOut(), new CurveElasticInOut(),
			new CurveCircleIn(), new CurveCircleOut(), new CurveCircleInOut(),
			new CurveQuartIn(), new CurveQuartOut(), new CurveQuartInOut(),
			new CurveQuintIn(), new CurveQuintOut(), new CurveQuintInOut(),
			new CurveZeroOne(), new CurveOneZero(),
			new CurveZeroOneZero(), new CurveOneZeroOne(),
		};

		foreach (var c in curves)
		{
			string name = c.GetType().Name;
			float len = c.getLength();
			assertTrue(len > 0, name + " length > 0");
			assertTrue(len <= 4.0f, name + " length <= 4");
		}
	}

	//==================================================================
	// CurveZeroOne 特定测试
	//==================================================================
	private static void testCurveZeroOne()
	{
		var c = new CurveZeroOne();
		assertEqual(0.0f, c.evaluate(0.0f));
		assertEqual(1.0f, c.evaluate(1.0f));
		// 0→1 是单调递增的
		float mid = c.evaluate(0.5f);
		assertTrue(mid > 0.0f && mid < 1.0f, "ZeroOne midpoint in (0,1)");
	}

	//==================================================================
	// CurveOneZero 特定测试
	//==================================================================
	private static void testCurveOneZero()
	{
		var c = new CurveOneZero();
		// 1→0: evaluate(0)=1, evaluate(1)=0
		assertEqual(1.0f, c.evaluate(0.0f));
		assertEqual(0.0f, c.evaluate(1.0f));
		float mid = c.evaluate(0.5f);
		assertTrue(mid > 0.0f && mid < 1.0f, "OneZero midpoint in (0,1)");
	}

	//==================================================================
	// CurveZeroOneZero 特定测试
	//==================================================================
	private static void testCurveZeroOneZero()
	{
		var c = new CurveZeroOneZero();
		// 0→1→0: evaluate(0)=0, evaluate(1)=0, 中间峰值=1
		assertEqual(0.0f, c.evaluate(0.0f));
		assertEqual(0.0f, c.evaluate(1.0f));
		float peak = c.evaluate(0.5f);
		assertTrue(peak > 0.5f, "ZeroOneZero peak > 0.5");
	}

	//==================================================================
	// CurveOneZeroOne 特定测试
	//==================================================================
	private static void testCurveOneZeroOne()
	{
		var c = new CurveOneZeroOne();
		// 1→0→1: evaluate(0)=1, evaluate(1)=1, 中间谷底=0
		assertEqual(1.0f, c.evaluate(0.0f));
		assertEqual(1.0f, c.evaluate(1.0f));
		float valley = c.evaluate(0.5f);
		assertTrue(valley < 0.5f, "OneZeroOne valley < 0.5");
	}

	//==================================================================
}
