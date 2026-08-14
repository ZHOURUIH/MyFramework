using static TestAssert;

// MyCurve 关键帧曲线数学测试: 直接 new 具体曲线子类(纯 C# 公式, 无外部依赖)
// CurveQuadIn: t² / CurveQuadOut: 2t-t² / CurveOneZero: 1-t / CurveCubicIn: t³
public static class MyCurveTest
{
	public static void Run()
	{
		testQuadInFormula();
		testQuadOutFormula();
		testQuadInOutComplement();
		testOneZeroLinear();
		testCubicInFormula();
		testGetLengthDefault();
		testEvaluateNoNaN();
		testEndpointsNormalized();
		testOneZeroEndpoints();
		testResetPropertySafe();
		testBounceEndpoints();
		testElasticEndpoints();
		testExpoInShape();
		testCircleInShape();
		testBounceOutEndpoints();
		testCurveMidpointsFinite();
		testQuadInOutFormula();
		testOneZeroOneShape();
		testQuintInEndpoints();
		testQuintOutEndpoints();
	}

	// QuadIn: t² 公式逐点验证
	private static void testQuadInFormula()
	{
		CurveQuadIn curve = new CurveQuadIn();
		assertEqual(0.0f, curve.evaluate(0.0f), 0.0001f, "QuadIn(0)=0");
		assertEqual(0.0625f, curve.evaluate(0.25f), 0.0001f, "QuadIn(0.25)=0.0625");
		assertEqual(0.25f, curve.evaluate(0.5f), 0.0001f, "QuadIn(0.5)=0.25");
		assertEqual(1.0f, curve.evaluate(1.0f), 0.0001f, "QuadIn(1)=1");
	}

	// QuadOut: 2t-t² 公式逐点验证
	private static void testQuadOutFormula()
	{
		CurveQuadOut curve = new CurveQuadOut();
		assertEqual(0.0f, curve.evaluate(0.0f), 0.0001f, "QuadOut(0)=0");
		assertEqual(0.75f, curve.evaluate(0.5f), 0.0001f, "QuadOut(0.5)=0.75");
		assertEqual(1.0f, curve.evaluate(1.0f), 0.0001f, "QuadOut(1)=1");
	}

	// 组合: QuadIn 与 QuadOut 在任意 t 处互补(和为 1)
	private static void testQuadInOutComplement()
	{
		CurveQuadIn quadIn = new CurveQuadIn();
		CurveQuadOut quadOut = new CurveQuadOut();
		// 正确关系: QuadOut(t) = 1 - QuadIn(1-t)(镜像对称, 不是同点相加=1!)
		// QuadIn(t)+QuadOut(t) = t²+(2t-t²) = 2t, 仅 t=0.5 时为 1
		for (int i = 0; i <= 10; ++i)
		{
			float t = i / 10.0f;
			float mirrored = 1.0f - quadIn.evaluate(1.0f - t);
			assertTrue(quadOut.evaluate(t).isEqual(mirrored, 0.0001f), "QuadOut(t)=1-QuadIn(1-t): t=" + t);
		}
	}

	// OneZero: 从 1 到 0 的直线
	private static void testOneZeroLinear()
	{
		CurveOneZero curve = new CurveOneZero();
		assertEqual(1.0f, curve.evaluate(0.0f), 0.0001f, "OneZero(0)=1");
		assertEqual(0.5f, curve.evaluate(0.5f), 0.0001f, "OneZero(0.5)=0.5");
		assertEqual(0.0f, curve.evaluate(1.0f), 0.0001f, "OneZero(1)=0");
	}

	// CubicIn: t³ 公式验证
	private static void testCubicInFormula()
	{
		CurveCubicIn curve = new CurveCubicIn();
		assertEqual(0.125f, curve.evaluate(0.5f), 0.0001f, "CubicIn(0.5)=0.125");
		assertEqual(0.027f, curve.evaluate(0.3f), 0.0001f, "CubicIn(0.3)=0.027");
	}

	// getLength 默认 1.0(基类 MyCurve)
	private static void testGetLengthDefault()
	{
		CurveQuadIn curve = new CurveQuadIn();
		assertEqual(1.0f, curve.getLength(), 0.0001f, "getLength 默认 1.0");
	}

	// 组合: 多个曲线在 0.5 处 evaluate 非 NaN
	private static void testEvaluateNoNaN()
	{
		MyCurve[] curves = { new CurveQuadIn(), new CurveQuadOut(), new CurveOneZero(), new CurveCubicIn() };
		foreach (MyCurve curve in curves)
		{
			float v = curve.evaluate(0.5f);
			assertTrue(!float.IsNaN(v) && !float.IsInfinity(v), "evaluate(0.5) 非 NaN/Infinity, type=" + curve.GetType().Name);
		}
	}

	// 组合: 上升曲线端点归一(0→0, 1→1)
	private static void testEndpointsNormalized()
	{
		MyCurve[] curves = { new CurveQuadIn(), new CurveQuadOut(), new CurveCubicIn() };
		foreach (MyCurve curve in curves)
		{
			assertEqual(0.0f, curve.evaluate(0.0f), 0.0001f, "起点 evaluate(0)=0, type=" + curve.GetType().Name);
			assertEqual(1.0f, curve.evaluate(1.0f), 0.0001f, "终点 evaluate(1)=1, type=" + curve.GetType().Name);
		}
	}

	// OneZero 端点: 1→0
	private static void testOneZeroEndpoints()
	{
		CurveOneZero curve = new CurveOneZero();
		assertEqual(1.0f, curve.evaluate(0.0f), 0.0001f, "起点=1");
		assertEqual(0.0f, curve.evaluate(1.0f), 0.0001f, "终点=0");
	}

	// resetProperty: ClassObject 子类调用安全
	private static void testResetPropertySafe()
	{
		CurveQuadIn curve = new CurveQuadIn();
		curve.resetProperty();
		// resetProperty 后 evaluate 仍正常(无字段可复位)
		assertEqual(0.25f, curve.evaluate(0.5f), 0.0001f, "resetProperty 后 evaluate 正常");
	}

	// BounceIn 端点归一: bounceEaseIn(0)=1-bounceEaseOut(1)=0, (1)=1-bounceEaseOut(0)=1
	private static void testBounceEndpoints()
	{
		CurveBounceIn curve = new CurveBounceIn();
		assertEqual(0.0f, curve.evaluate(0.0f), 0.0001f, "BounceIn(0)=0");
		assertEqual(1.0f, curve.evaluate(1.0f), 0.0001f, "BounceIn(1)=1");
	}

	// BounceOut 端点归一
	private static void testBounceOutEndpoints()
	{
		CurveBounceOut curve = new CurveBounceOut();
		assertEqual(0.0f, curve.evaluate(0.0f), 0.0001f, "BounceOut(0)=0");
		assertEqual(1.0f, curve.evaluate(1.0f), 0.0001f, "BounceOut(1)=1");
	}

	// ElasticIn 端点: 源码特判 0→0, 1→1
	private static void testElasticEndpoints()
	{
		CurveElasticIn curve = new CurveElasticIn();
		assertEqual(0.0f, curve.evaluate(0.0f), 0.0001f, "ElasticIn(0)=0(特判)");
		assertEqual(1.0f, curve.evaluate(1.0f), 0.0001f, "ElasticIn(1)=1(特判)");
	}

	// ExpoIn 形状: 0 特判 0, 1→1, 0.5→2^-5=0.03125(先慢后快)
	private static void testExpoInShape()
	{
		CurveExpoIn curve = new CurveExpoIn();
		assertEqual(0.0f, curve.evaluate(0.0f), 0.0001f, "ExpoIn(0)=0(特判)");
		assertEqual(1.0f, curve.evaluate(1.0f), 0.0001f, "ExpoIn(1)=1");
		assertEqual(0.03125f, curve.evaluate(0.5f), 0.0001f, "ExpoIn(0.5)=2^-5=0.03125");
	}

	// CircleIn 形状: 1-sqrt(1-t²), 0.5→0.134(先慢)
	private static void testCircleInShape()
	{
		CurveCircleIn curve = new CurveCircleIn();
		assertEqual(0.0f, curve.evaluate(0.0f), 0.0001f, "CircleIn(0)=0");
		assertEqual(1.0f, curve.evaluate(1.0f), 0.0001f, "CircleIn(1)=1");
		float v = curve.evaluate(0.5f);
		assertTrue(v.isEqual(0.134f, 0.001f), "CircleIn(0.5)≈0.134, 实际 " + v);
	}

	// 组合: 更多曲线中间点有限(振荡曲线中间可越界但不 NaN)
	private static void testCurveMidpointsFinite()
	{
		MyCurve[] curves = { new CurveBounceIn(), new CurveElasticIn(), new CurveExpoIn(), new CurveCircleIn(), new CurveBackIn() };
		foreach (MyCurve curve in curves)
		{
			for (int i = 1; i <= 9; ++i)
			{
				float v = curve.evaluate(i / 10.0f);
				assertTrue(!float.IsNaN(v) && !float.IsInfinity(v), "曲线 " + curve.GetType().Name + " 在 " + i / 10.0f + " 有限");
			}
		}
	}

	// QuadInOut: 0.5t²(t<2 恒真), 注意在 0..1 只到 0.5
	private static void testQuadInOutFormula()
	{
		CurveQuadInOut curve = new CurveQuadInOut();
		assertEqual(0.0f, curve.evaluate(0.0f), 0.0001f, "QuadInOut(0)=0");
		assertEqual(0.125f, curve.evaluate(0.5f), 0.0001f, "QuadInOut(0.5)=0.125");
		assertEqual(0.5f, curve.evaluate(1.0f), 0.0001f, "QuadInOut(1)=0.5(0..1 只到半程)");
	}

	// OneZeroOne: 1→0→1 折线(中点谷值)
	private static void testOneZeroOneShape()
	{
		CurveOneZeroOne curve = new CurveOneZeroOne();
		assertEqual(1.0f, curve.evaluate(0.0f), 0.0001f, "OneZeroOne(0)=1");
		assertEqual(0.0f, curve.evaluate(0.5f), 0.0001f, "OneZeroOne(0.5)=0(谷值)");
		assertEqual(1.0f, curve.evaluate(1.0f), 0.0001f, "OneZeroOne(1)=1");
	}

	// QuintIn: t⁵ 端点与中点
	private static void testQuintInEndpoints()
	{
		CurveQuintIn curve = new CurveQuintIn();
		assertEqual(0.0f, curve.evaluate(0.0f), 0.0001f, "QuintIn(0)=0");
		assertEqual(1.0f, curve.evaluate(1.0f), 0.0001f, "QuintIn(1)=1");
		assertEqual(0.03125f, curve.evaluate(0.5f), 0.0001f, "QuintIn(0.5)=0.5^5=0.03125");
	}

	// QuintOut: (t-1)⁵+1 端点与中点
	private static void testQuintOutEndpoints()
	{
		CurveQuintOut curve = new CurveQuintOut();
		assertEqual(0.0f, curve.evaluate(0.0f), 0.0001f, "QuintOut(0)=0");
		assertEqual(1.0f, curve.evaluate(1.0f), 0.0001f, "QuintOut(1)=1");
		assertEqual(0.96875f, curve.evaluate(0.5f), 0.0001f, "QuintOut(0.5)=1-0.5^5=0.96875");
	}
}
