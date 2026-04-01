#if UNITY_EDITOR || DEVELOPMENT_BUILD
using static MathUtility;
using static TestAssert;

// 代表性参数 + 少量随机用例，避免把所有组合“写死在源码里”
public static class BulkMathDivideIntTableTests
{
	public static void Run()
	{
		int[] aSamples =
		{
			0, 1, -1, 2, -2, 3, -3, 5, -5, 7, -7, 8, -8,
			10, -10, 31, -31, 32, -32, 63, -63, 64, -64,
			127, -127, 128, -128, 255, -255, 256, -256,
		};

		int[] bSamples = { -12, -7, -3, -1, 1, 2, 3, 4, 5, 7, 12 };

		foreach (int a in aSamples)
		{
			foreach (int b in bSamples)
			{
				int expectedI = a / b;
				long expectedL = (long)a / (long)b;

				int actualI = divideInt(a, b);
				long actualL = divideLong((long)a, (long)b);

				assertEqual(expectedI, actualI, "divI(" + a + "," + b + ")");
				assertEqual(expectedL, actualL, "divL(" + a + "," + b + ")");
			}

			// 除数为 0：验证 defaultValue 行为
			assertEqual(0, divideInt(a, 0), "divI(" + a + ",0)");
			assertEqual(-1, divideInt(a, 0, -1), "divI(" + a + ",0,-1)");
			assertEqual(0L, divideLong((long)a, 0L), "divL(" + a + ",0)");
			assertEqual(-1L, divideLong((long)a, 0L, -1), "divL(" + a + ",0,-1)");
		}

		// 随机用例（确定性）
		uint state = 987654321u;
		for (int i = 0; i < 600; ++i)
		{
			state = state * 1664525u + 1013904223u;
			int a = (int)((int)(state % 200000u) - 100000); // [-100000, 100000)

			state = state * 1664525u + 1013904223u;
			int b = (int)(state % 2000u); // [0,1999)
			if (b == 0) b = 1; // 避免除 0
			if ((i & 1) == 0) b = -b;

			int expectedI = a / b;
			long expectedL = (long)a / (long)b;

			assertEqual(expectedI, divideInt(a, b), "divI(r" + i + ")");
			assertEqual(expectedL, divideLong((long)a, (long)b), "divL(r" + i + ")");
		}
	}
}
#endif

