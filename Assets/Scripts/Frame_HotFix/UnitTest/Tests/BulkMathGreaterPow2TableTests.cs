#if UNITY_EDITOR || DEVELOPMENT_BUILD
using static MathUtility;
using static TestAssert;

// 代表性参数 + 随机用例，不枚举所有数字
public static class BulkMathGreaterPow2TableTests
{
	public static void Run()
	{
		// 边界/关键点覆盖
		int[] samples =
		{
			0, 1, 2, 3, 4, 5, 6, 7,
			8, 9, 10, 15, 16, 17,
			31, 32, 33, 63, 64, 65,
			127, 128, 129, 255, 256, 257,
			511, 512, 513, 1023, 1024, 1025,
			4095, 4096, 4097, 8191, 8192, 8193,
			16383, 16384, 16385, 32767, 32768, 32769
		};

		foreach (int v in samples)
		{
			int expected = getExpected(v);
			int actual = getGreaterPow2(v);
			assertEqual(expected, actual, "gp2(" + v + ")");

			// 性质检查：除非函数在极端情况返回 0，否则结果应为严格的 2 次幂
			if (actual != 0)
			{
				assert(isPow2Strict(actual), "gp2 pow2 strict: " + v);
				if (v > 1) assert(actual >= v, "gp2>=v: " + v);
			}
		}

		// 随机代表性覆盖（确定性随机）
		uint state = 123456789u;
		for (int i = 0; i < 400; ++i)
		{
			state = state * 1664525u + 1013904223u;
			int v = (int)(state % 50000u); // 0..49999

			int expected = getExpected(v);
			int actual = getGreaterPow2(v);
			assertEqual(expected, actual, "gp2(r" + i + ":" + v + ")");
		}
	}

	private static int getExpected(int value)
	{
		// MathUtility.getGreaterPow2 的行为：对 value<=1 返回 2
		if (value <= 1)
			return 2;

		// 返回 >= value 的最小 2 的 n 次幂
		uint u = (uint)value;
		uint p = 1;
		while (p < u)
		{
			p <<= 1;
			// 溢出保护
			if (p == 0)
				return 0;
		}
		return (int)p;
	}

	private static bool isPow2Strict(int v) { return v > 0 && (v & (v - 1)) == 0; }
}
#endif

