#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine;
using static MathUtility;
using static TestAssert;

// pow10 / inversePow10 / generateBatchCount / 单位换算 的代表性测试
public static class BulkMathPowerAndUnitTableTests
{
	public static void Run()
	{
		// 1) pow10 / pow10Long / inversePow10 / inversePow10Long：数组下标全覆盖（长度很小）
		for (int i = 0; i < POWER_INT_10.Length; ++i)
		{
			assertEqual((int)POWER_INT_10[i], pow10(i), "pow10(" + i + ")");
		}
		for (int i = 0; i < POWER_LLONG_10.Length; ++i)
		{
			assertEqual(POWER_LLONG_10[i], pow10Long(i), "pow10Long(" + i + ")");
		}
		for (int i = 0; i < INVERSE_POWER_INT_10.Length; ++i)
		{
			float expected = INVERSE_POWER_INT_10[i];
			float actual = inversePow10(i);
			assert(Mathf.Abs(actual - expected) < 0.000001f, "invPow10(" + i + ")");
		}
		for (int i = 0; i < INVERSE_POWER_LLONG_10.Length; ++i)
		{
			double expected = INVERSE_POWER_LLONG_10[i];
			double actual = inversePow10Long(i);
			assert(Mathf.Abs((float)(actual - expected)) < 0.0000001f, "invPow10Long(" + i + ")");
		}

		// 2) generateBatchCount：边界 + 随机（使用公式期望，不枚举所有数字）
		int[] batchSamples = { 1, 2, 3, 5, 7, 10, 16, 31, 64, 100 };
		uint state = 246340595u;
		for (int i = 0; i < 2000; ++i)
		{
			state = state * 1664525u + 1013904223u;
			int total = (int)(state % 200000u); // 0..199999

			int batch = batchSamples[i % batchSamples.Length];
			int expected = total == 0 ? 0 : (total + batch - 1) / batch; // ceil(total/batch)
			int actual = generateBatchCount(total, batch);
			assertEqual(expected, actual, "gb(" + total + "," + batch + ")");
		}

		// 3) 单位换算：KMH<->MS、MtoKM
		float[] kmhSamples = { 0f, 0.01f, 1f, 18f, 36f, 72.5f, 120f, 333.3f };
		foreach (float kmh in kmhSamples)
		{
			float ms = KMHtoMS(kmh);
			float kmh2 = MStoKMH(ms);
			assert(Mathf.Abs(kmh2 - kmh) < 0.05f, "kmh<->ms: " + kmh);
		}

		float[] mSamples = { 0f, 1f, 1000f, 2500.5f, 1000000f };
		foreach (float m in mSamples)
		{
			float km = MtoKM(m);
			assert(Mathf.Abs(km - m * 0.001f) < 1e-4f, "m->km: " + m);
		}
	}
}
#endif

