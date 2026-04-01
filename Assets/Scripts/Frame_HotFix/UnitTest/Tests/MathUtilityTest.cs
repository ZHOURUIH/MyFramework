#if UNITY_EDITOR || DEVELOPMENT_BUILD
using static TestAssert;
using UnityEngine;
using static MathUtility;

// MathUtility 纯计算函数测试
// 覆盖：ceil / floor / round / clamp / abs / sign / pow / hasMask /
//        indexToX/Y / isEven / isPow2 / getGreaterPow2 / saturate /
//        frac / fmod / step / dot / isFloatEqual 等
public static class MathUtilityTest
{
    public static void Run()
    {
        testCeil();
        testFloor();
        testRound();
        testAbs();
        testSign();
        testClamp();
        testClampMin();
        testClampMax();
        testPow();
        testHasMask();
        testIndexToXY();
        testIsEven();
        testIsPow2();
        testGetGreaterPow2();
        testSaturate();
        testFrac();
        testFmod();
        testStep();
        testDot();
        testIsFloatEqual();
        testDivideInt();
        testGenerateBatchCount();
    }

    // ─── ceil ───────────────────────────────────────────────────────────────
    private static void testCeil()
    {
        assertEqual(3, ceil(2.1f),  "ceil 正数小数");
        assertEqual(2, ceil(2.0f),  "ceil 正数整数");
        assertEqual(-2, ceil(-2.0f), "ceil 负数整数");
        assertEqual(-1, ceil(-1.9f), "ceil 负数小数 → 向零方向");
        assertEqual(0, ceil(-0.5f),  "ceil -0.5");
        assertEqual(1, ceil(0.5f),   "ceil 0.5");
    }

    // ─── floor ──────────────────────────────────────────────────────────────
    private static void testFloor()
    {
        assertEqual(2,  floor(2.9f),  "floor 正数小数");
        assertEqual(2,  floor(2.0f),  "floor 正数整数");
        assertEqual(-3, floor(-2.1f), "floor 负数小数");
        assertEqual(-2, floor(-2.0f), "floor 负数整数");
        assertEqual(0,  floor(0.9f),  "floor 0.9");
    }

    // ─── round ──────────────────────────────────────────────────────────────
    private static void testRound()
    {
        assertEqual(3,  round(2.5f),  "round 0.5 向上");
        assertEqual(2,  round(2.4f),  "round 0.4 向下");
        assertEqual(-3, round(-2.5f), "round 负数 -0.5 向下");
        assertEqual(-2, round(-2.4f), "round 负数 -0.4 向上");
        assertEqual(0,  round(0.0f),  "round 0");
    }

    // ─── abs ────────────────────────────────────────────────────────────────
    private static void testAbs()
    {
        assertEqual(5,    abs(-5),     "abs int 负数");
        assertEqual(5,    abs(5),      "abs int 正数");
        assertEqual(0,    abs(0),      "abs 0");
        assertEqual(3.5f, abs(-3.5f),  "abs float 负数");
        assertEqual(5L,   abs(-5L),    "abs long 负数");
        assertEqual(0L,   abs(0L),     "abs long 0");
        assertEqual(0.0f, abs(0.0f),   "abs float 0");
        // 较大正值不变
        assertEqual(int.MaxValue, abs(int.MaxValue), "abs int.MaxValue 不变");
        assertEqual(100L, abs(-100L),  "abs long 负数-100");
    }

    // ─── sign ───────────────────────────────────────────────────────────────
    private static void testSign()
    {
        assertEqual(-1,   sign(-10),   "sign 负数");
        assertEqual(1,    sign(10),    "sign 正数");
        assertEqual(0,    sign(0),     "sign 零");
        assertEqual(-1.0f, sign(-1.5f), "sign float 负数");
        assertEqual(1.0f,  sign(0.01f), "sign float 正数");
        assertEqual(0.0f,  sign(0.0f),  "sign float 零");
    }

    // ─── clamp ──────────────────────────────────────────────────────────────
    private static void testClamp()
    {
        assertEqual(5,    clamp(10, 0, 5),   "clamp int 超上限");
        assertEqual(0,    clamp(-5, 0, 5),   "clamp int 超下限");
        assertEqual(3,    clamp(3, 0, 5),    "clamp int 在范围内");
        assertEqual(1.0f, clamp(2.0f, 0.0f, 1.0f), "clamp float 超上限");
        assertEqual(0.0f, clamp(-1.0f, 0.0f, 1.0f), "clamp float 超下限");
        // 恰好等于边界
        assertEqual(5,    clamp(5, 0, 5),    "clamp int 恰好=上限");
        assertEqual(0,    clamp(0, 0, 5),    "clamp int 恰好=下限");
        assertEqual(1.0f, clamp(1.0f, 0.0f, 1.0f), "clamp float 恰好=上限");
        // min==max 时应返回该值
        assertEqual(3,    clamp(7, 3, 3),    "clamp min==max 超上限时返回min");
        assertEqual(3,    clamp(-1, 3, 3),   "clamp min==max 超下限时返回min");
    }

    // ─── clampMin ───────────────────────────────────────────────────────────
    private static void testClampMin()
    {
        assertEqual(5,  clampMin(3, 5),  "clampMin 小于下限");
        assertEqual(10, clampMin(10, 5), "clampMin 大于下限");
    }

    // ─── clampMax ───────────────────────────────────────────────────────────
    private static void testClampMax()
    {
        assertEqual(5,  clampMax(10, 5), "clampMax 超上限");
        assertEqual(3,  clampMax(3, 5),  "clampMax 不超上限");
    }

    // ─── pow ────────────────────────────────────────────────────────────────
    private static void testPow()
    {
        assert(isFloatEqual(pow(2.0f, 3), 8.0f),  "pow float 2^3=8");
        assert(isFloatEqual(pow(3.0f, 0), 1.0f),  "pow 0次方=1");
        assertEqual(100, pow10(2), "pow10(2)=100");
        assertEqual(1000, pow10(3), "pow10(3)=1000");
    }

    // ─── hasMask ────────────────────────────────────────────────────────────
    private static void testHasMask()
    {
        assert(hasMask(0b1010, 0b0010),  "hasMask 命中");
        assert(!hasMask(0b1010, 0b0001), "hasMask 未命中");
        assert(!hasMask(0, 0xFF),         "hasMask 0 & 任意 = false");
    }

    // ─── indexToX / indexToY ────────────────────────────────────────────────
    private static void testIndexToXY()
    {
        // 宽度为 5 的网格
        // index 7 → x=2, y=1
        assertEqual(2, indexToX(7, 5), "indexToX");
        assertEqual(1, indexToY(7, 5), "indexToY");
        assertEqual(0, indexToX(0, 5), "indexToX 0");
        assertEqual(0, indexToY(0, 5), "indexToY 0");
        // intPosToIndex 逆运算
        assertEqual(7, intPosToIndex(2, 1, 5), "intPosToIndex");
    }

    // ─── isEven ─────────────────────────────────────────────────────────────
    private static void testIsEven()
    {
        assert(isEven(0),   "isEven 0");
        assert(isEven(2),   "isEven 2");
        assert(!isEven(1),  "isEven 1");
        assert(!isEven(-3), "isEven -3");
    }

    // ─── isPow2 ─────────────────────────────────────────────────────────────
    private static void testIsPow2()
    {
        assert(isPow2(1),   "isPow2 1");
        assert(isPow2(2),   "isPow2 2");
        assert(isPow2(16),  "isPow2 16");
        assert(!isPow2(3),  "isPow2 3");
        assert(!isPow2(12), "isPow2 12");
    }

    // ─── getGreaterPow2 ─────────────────────────────────────────────────────
	private static void testGetGreaterPow2()
	{
		// getGreaterPow2 对 value<=1 固定返回 2（查表实现，mGreaterPow2[0]=mGreaterPow2[1]=2）
		assertEqual(2,   getGreaterPow2(1),   "gGP2(1)=2");
		assertEqual(4,   getGreaterPow2(3),   "gGP2(3)=4");
		assertEqual(8,   getGreaterPow2(5),   "gGP2(5)=8");
		assertEqual(16,  getGreaterPow2(16),  "gGP2(16)=16");
		assertEqual(32,  getGreaterPow2(17),  "gGP2(17)=32");
		assertEqual(256, getGreaterPow2(200), "gGP2(200)=256");
	}

    // ─── saturate ───────────────────────────────────────────────────────────
    private static void testSaturate()
    {
        assert(isFloatEqual(saturate(-1.0f), 0.0f), "saturate -1→0");
        assert(isFloatEqual(saturate(2.0f),  1.0f), "saturate 2→1");
        assert(isFloatEqual(saturate(0.5f),  0.5f), "saturate 0.5 不变");
    }

    // ─── frac ───────────────────────────────────────────────────────────────
    private static void testFrac()
    {
        assert(isFloatEqual(frac(3.75f), 0.75f), "frac 3.75");
        assert(isFloatEqual(frac(2.0f),  0.0f),  "frac 整数");
        assert(isFloatEqual(frac(-1.3f), -0.3f, 0.001f), "frac 负数");
    }

    // ─── fmod ───────────────────────────────────────────────────────────────
    private static void testFmod()
    {
        assert(isFloatEqual(fmod(7.5f, 2.5f), 0.0f, 0.001f), "fmod 整除");
        assert(isFloatEqual(fmod(7.0f, 3.0f), 1.0f, 0.001f), "fmod 余1");
    }

    // ─── step ───────────────────────────────────────────────────────────────
    private static void testStep()
    {
        assertEqual(1, step(3.0f, 5.0f), "step v1>v0");
        assertEqual(1, step(3.0f, 3.0f), "step v1==v0");
        assertEqual(0, step(5.0f, 3.0f), "step v1<v0");
    }

    // ─── dot ────────────────────────────────────────────────────────────────
    private static void testDot()
    {
        Vector3 a = new(1, 2, 3);
        Vector3 b = new(4, 5, 6);
        assert(isFloatEqual(dot(a, b), 32.0f), "dot V3 1*4+2*5+3*6=32");
        Vector2 c = new(3, 4);
        Vector2 d = new(1, 2);
        assert(isFloatEqual(dot(c, d), 11.0f), "dot V2 3*1+4*2=11");
    }

    // ─── isFloatEqual ───────────────────────────────────────────────────────
    private static void testIsFloatEqual()
    {
        assert(isFloatEqual(1.0f, 1.0f),           "isFloatEqual 相等");
        assert(isFloatEqual(1.0f, 1.0001f, 0.001f),"isFloatEqual 在容差内");
        assert(!isFloatEqual(1.0f, 1.1f),           "isFloatEqual 不等");
    }

    // ─── divideInt ──────────────────────────────────────────────────────────
    private static void testDivideInt()
    {
        assertEqual(3, divideInt(10, 3), "divideInt 10/3=3");
        assertEqual(-3, divideInt(-10, 3), "divideInt -10/3=-3 (向零取整)");
        // 除数为 0 时返回 defaultValue=0
        assertEqual(0, divideInt(999, 0),         "divideInt 除数0→defaultValue=0");
        assertEqual(-1, divideInt(999, 0, -1),    "divideInt 除数0→自定义defaultValue=-1");
        // 整除
        assertEqual(5, divideInt(10, 2),          "divideInt 整除10/2=5");
        // 边界
        assertEqual(0, divideInt(0, 100),         "divideInt 0/100=0");
        assertEqual(1, divideInt(int.MaxValue, int.MaxValue), "divideInt MaxValue/MaxValue=1");
    }

    // ─── generateBatchCount ─────────────────────────────────────────────
    private static void testGenerateBatchCount()
    {
        assertEqual(2, generateBatchCount(10, 5), "batch 10/5=2");
        assertEqual(3, generateBatchCount(11, 5), "batch 11/5=3 (有余)");
        assertEqual(1, generateBatchCount(3, 5),  "batch 3/5=1 (不足一批)");
        assertEqual(0, generateBatchCount(0, 5),  "batch 0/5=0");
        assertEqual(1, generateBatchCount(1, 1),  "batch 1/1=1");
    }
}
#endif
