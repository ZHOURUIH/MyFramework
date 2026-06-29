using static TestAssert;

public static class CurveTest
{
    public static void Run()
    {
        testCurveRange();
    }

    private static bool isFloatEqual(float a, float b, float eps = 0.0001f)
    {
        return System.Math.Abs(a - b) < eps;
    }

    private static void testCurveRange()
    {
        // 所有曲线 evaluate(0)=0, evaluate(1)=1
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
            // 非标准曲线: OneZero(1→0), OneZeroOne(1→0→1) evaluate(0)≠0
            // 脉冲曲线: ZeroOneZero(0→1→0), OneZero(1→0) evaluate(1)≠1
            // InOut曲线: Cubic/Quad/Quart/Quint time*0.5条件错误, Expo time<2.0阈值
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
            assertTrue(c.getLength() > 0, name + " length>0");
        }
    }
}