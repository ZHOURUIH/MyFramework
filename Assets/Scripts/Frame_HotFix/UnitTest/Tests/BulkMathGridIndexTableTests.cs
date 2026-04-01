#if UNITY_EDITOR || DEVELOPMENT_BUILD
using static MathUtility;
using static TestAssert;

// indexToX/indexToY/intPosToIndex 的往返一致性测试（代表性参数 + 随机）
public static class BulkMathGridIndexTableTests
{
	public static void Run()
	{
		int[] widths = { 1, 2, 3, 4, 5, 8, 10, 14, 16, 24 };

		// 1) 小范围“穷举少量”的点
		foreach (int width in widths)
		{
			int maxY = 20;
			// x 只能在 [0, width-1]，否则 intPosToIndex / indexToX 会涉及“溢出到其它格子”的含义
			int[] xs = { 0, width / 2, width - 1 };

			for (int y = 0; y <= maxY; ++y)
			{
				foreach (int x in xs)
				{
					int idx = intPosToIndex(x, y, width);
					int ax = indexToX(idx, width);
					int ay = indexToY(idx, width);

					assertEqual(x, ax, "ipi->ix: w" + width + " x" + x + " y" + y);
					assertEqual(y, ay, "ipi->iy: w" + width + " x" + x + " y" + y);
					assertEqual(idx, intPosToIndex(ax, ay, width), "roundtrip idx: w" + width);
				}
			}
		}

		// 2) 随机 idx 测试：验证 indexToX/Y 的数学定义
		uint state = 246813579u;
		for (int i = 0; i < 800; ++i)
		{
			state = state * 1664525u + 1013904223u;
			int width = (int)(state % 64u) + 1; // 1..64

			state = state * 1664525u + 1013904223u;
			int idxMax = width * 200;
			int idx = (int)(state % (uint)idxMax); // 0..width*200-1

			int expectedX = idx % width;
			int expectedY = idx / width;

			assertEqual(expectedX, indexToX(idx, width), "ix(r" + i + ")");
			assertEqual(expectedY, indexToY(idx, width), "iy(r" + i + ")");
			assertEqual(idx, intPosToIndex(expectedX, expectedY, width), "ipi(r" + i + ")");
		}
	}
}
#endif

