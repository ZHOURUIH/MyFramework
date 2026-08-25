using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;

public static class SafeListFastPathBenchmark
{
	private const int TARGET_VISITS = 1000000;
	private const int MIN_LOOP_COUNT = 2000;
	private const int WARMUP_COUNT = 2;
	private const int SAMPLE_COUNT = 9;
	private static readonly int[] mCounts = { 1, 4, 8, 32, 128 };
	private struct BenchmarkResult
	{
		public double mMedianMS;
		public double mMinMS;
		public double mMaxMS;
	}
	public static void Run()
	{
		StringBuilder builder = new();
		builder.AppendLine("================ SafeList FastPath Benchmark Start ================");
		builder.AppendLine("SampleCount:" + SAMPLE_COUNT + " WarmupCount:" + WARMUP_COUNT);
		builder.AppendLine("SafeList Foreach:正式SafeList无修改常态foreach");
		builder.AppendLine("MainList For    :直接List for循环下限参考");
		builder.AppendLine("Modify Between  :每轮foreach之间removeAt+add,测试非遍历修改是否保持快路径");
		for (int i = 0; i < mCounts.Length; ++i)
		{
			runCase(builder, mCounts[i]);
		}
		builder.AppendLine("================ SafeList FastPath Benchmark End ==================");
		UnityEngine.Debug.Log(builder.ToString());
	}
	private static void runCase(StringBuilder builder, int count)
	{
		SafeList<int> safeList = new();
		for (int i = 0; i < count; ++i)
		{
			safeList.add(i);
		}
		// 建立稳定状态
		foreach (int value in safeList)
		{
			_ = value;
		}
		int loopCount = Math.Max(MIN_LOOP_COUNT, TARGET_VISITS / count);
		long checksum = 0;
		BenchmarkResult safeForeach = measure(() =>
		{
			long local = 0;
			for (int loop = 0; loop < loopCount; ++loop)
			{
				foreach (int value in safeList)
				{
					local += value;
				}
			}
			checksum += local;
		});
		List<int> mainList = safeList.getMainList();
		BenchmarkResult mainFor = measure(() =>
		{
			long local = 0;
			for (int loop = 0; loop < loopCount; ++loop)
			{
				for (int i = 0, listCount = mainList.Count; i < listCount; ++i)
				{
					local += mainList[i];
				}
			}
			checksum += local;
		});
		BenchmarkResult modifyBetween = measure(() =>
		{
			long local = 0;
			for (int loop = 0; loop < loopCount; ++loop)
			{
				if (safeList.count() > 0)
				{
					int value = safeList.removeAt(safeList.count() - 1);
					safeList.add(value);
				}
				foreach (int value in safeList)
				{
					local += value;
				}
			}
			checksum += local;
		});
		builder.AppendLine("---------------- Count:" + count + " LoopCount:" + loopCount + " ----------------");
		appendResult(builder, "SafeList Foreach", safeForeach, loopCount, count);
		appendResult(builder, "MainList For", mainFor, loopCount, count);
		appendResult(builder, "Modify Between", modifyBetween, loopCount, count);
		builder.AppendLine("SafeList / MainList".PadRight(24) + ": " + (safeForeach.mMedianMS / mainFor.mMedianMS).ToString("F2") + "x");
		builder.AppendLine("Checksum:" + checksum);
	}
	private static BenchmarkResult measure(Action action)
	{
		for (int i = 0; i < WARMUP_COUNT; ++i)
		{
			action();
		}
		GC.Collect();
		GC.WaitForPendingFinalizers();
		GC.Collect();
		double[] samples = new double[SAMPLE_COUNT];
		for (int i = 0; i < SAMPLE_COUNT; ++i)
		{
			long start = Stopwatch.GetTimestamp();
			action();
			long end = Stopwatch.GetTimestamp();
			samples[i] = (end - start) * 1000.0 / Stopwatch.Frequency;
		}
		Array.Sort(samples);
		return new BenchmarkResult
		{
			mMedianMS = samples[SAMPLE_COUNT >> 1],
			mMinMS = samples[0],
			mMaxMS = samples[SAMPLE_COUNT - 1]
		};
	}
	private static void appendResult(StringBuilder builder, string name, BenchmarkResult result, int loopCount, int count)
	{
		double nsPerLoop = result.mMedianMS * 1000000.0 / loopCount;
		double nsPerItem = nsPerLoop / count;
		builder.AppendLine(name.PadRight(20) +
			" Median:" + result.mMedianMS.ToString("F3").PadLeft(9) +
			" ms | Min:" + result.mMinMS.ToString("F3").PadLeft(8) +
			" | Max:" + result.mMaxMS.ToString("F3").PadLeft(8) +
			" | " + nsPerLoop.ToString("F2").PadLeft(10) + " ns/loop" +
			" | " + nsPerItem.ToString("F2").PadLeft(9) + " ns/item");
	}
}
