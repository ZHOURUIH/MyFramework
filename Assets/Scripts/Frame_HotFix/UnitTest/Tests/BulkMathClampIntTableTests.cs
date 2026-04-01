#if UNITY_EDITOR || DEVELOPMENT_BUILD
using static MathUtility;
using static TestAssert;

// clamp / clampMin / clampMax 的性质 + 代表性用例测试
public static class BulkMathClampIntTableTests
{
	public static void Run()
	{
		int[] vSamples = { -100, -10, -5, -4, -3, -2, -1, 0, 1, 2, 3, 4, 5, 10, 100 };
		int[] loSamples = { -30, -10, -5, 0, 3, 10 };
		int[] deltas = { 0, 1, 2, 5, 10, 20 };

		// 1) 基础校验：clamp 的“手工定义”应完全一致
		foreach (int lo in loSamples)
		{
			foreach (int d in deltas)
			{
				int hi = lo + d;
				foreach (int v in vSamples)
				{
					int expected = clampManual(v, lo, hi);
					int actual = clamp(v, lo, hi);

					assertEqual(expected, actual, "clamp(" + v + "," + lo + "," + hi + ")");
					assert(actual >= lo, "clamp>=lo: " + v);
					assert(actual <= hi, "clamp<=hi: " + v);

					// 幂等性：clamp(clamp(x)) == clamp(x)
					int again = clamp(actual, lo, hi);
					assertEqual(actual, again, "clamp idempotent: " + v);
				}
			}
		}

		// 2) clampMin / clampMax 的定义校验 + 随机补充
		int[] minSamples = { -50, -10, -1, 0, 1, 5, 10 };
		int[] maxSamples = { -10, -1, 0, 1, 5, 10, 50 };

		foreach (int v in vSamples)
		{
			foreach (int minV in minSamples)
			{
				int expected = v < minV ? minV : v;
				assertEqual(expected, clampMin(v, minV), "clampMin(" + v + "," + minV + ")");
			}
			foreach (int maxV in maxSamples)
			{
				int expected = v > maxV ? maxV : v;
				assertEqual(expected, clampMax(v, maxV), "clampMax(" + v + "," + maxV + ")");
			}
		}

		// 3) 随机测试：更多覆盖
		uint state = 135791357u;
		for (int i = 0; i < 1000; ++i)
		{
			state = state * 1664525u + 1013904223u;
			int v = (int)(state % 200000u) - 100000;

			state = state * 1664525u + 1013904223u;
			int lo = (int)(state % 2000u) - 1000;

			state = state * 1664525u + 1013904223u;
			int hi = lo + (int)(state % 2001u); // 保证 hi>=lo

			int actual = clamp(v, lo, hi);
			assert(actual >= lo && actual <= hi, "rand clamp range i=" + i);
			assertEqual(clampManual(v, lo, hi), actual, "rand clamp exact i=" + i);
		}
	}

	private static int clampManual(int v, int lo, int hi)
	{
		if (v < lo) return lo;
		if (v > hi) return hi;
		return v;
	}
}
#endif

