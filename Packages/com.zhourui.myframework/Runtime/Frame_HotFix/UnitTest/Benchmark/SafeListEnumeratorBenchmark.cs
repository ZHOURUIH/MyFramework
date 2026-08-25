using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;

public static class SafeListEnumeratorBenchmark
{
	private const int TARGET_VISITS = 1000000;
	private const int MIN_LOOP_COUNT = 2000;
	private const int WARMUP_COUNT = 2;
	private const int SAMPLE_COUNT = 9;
	private static readonly int[] mCounts = { 1, 4, 8, 32, 128 };
	private sealed class BenchmarkSafeList : SafeList<int>
	{
		public List<int> beginOldEnumerator() { return startForeach(); }
		public void endOldEnumerator() { endForeach(); }
	}
	private struct BenchmarkResult
	{
		public double mMedianMS;
		public double mMinMS;
		public double mMaxMS;
	}
	public static void Run()
	{
		StringBuilder builder = new();
		builder.AppendLine("================ SafeList Enumerator Benchmark Start ================");
		builder.AppendLine("SampleCount:" + SAMPLE_COUNT + " WarmupCount:" + WARMUP_COUNT);
		builder.AppendLine("Production Index : 当前正式SafeList索引式Enumerator");
		builder.AppendLine("Old ListEnum     : 模拟上一版startForeach + List<T>.Enumerator");
		builder.AppendLine("Snapshot For     : 同一SafeList快照直接for,安全遍历理论下限");
		builder.AppendLine("MainList For     : 直接实时主列表for,仅作绝对下限参考");
		for (int i = 0; i < mCounts.Length; ++i)
		{
			runCase(builder, mCounts[i]);
		}
		builder.AppendLine("================ SafeList Enumerator Benchmark End ==================");
		UnityEngine.Debug.Log(builder.ToString());
	}
	private static void runCase(StringBuilder builder, int count)
	{
		BenchmarkSafeList list = new();
		for (int i = 0; i < count; ++i)
		{
			list.add(i);
		}
		foreach (int value in list)
		{
			_ = value;
		}
		int loopCount = Math.Max(MIN_LOOP_COUNT, TARGET_VISITS / count);
		long checksum = 0;
		BenchmarkResult production = measure(() =>
		{
			long local = 0;
			for (int loop = 0; loop < loopCount; ++loop)
			{
				foreach (int value in list)
				{
					local += value;
				}
			}
			checksum += local;
		});
		BenchmarkResult oldEnumerator = measure(() =>
		{
			long local = 0;
			for (int loop = 0; loop < loopCount; ++loop)
			{
				List<int> snapshot = list.beginOldEnumerator();
				List<int>.Enumerator enumerator = snapshot.GetEnumerator();
				while (enumerator.MoveNext())
				{
					local += enumerator.Current;
				}
				enumerator.Dispose();
				list.endOldEnumerator();
			}
			checksum += local;
		});
		BenchmarkResult snapshotFor = measure(() =>
		{
			long local = 0;
			for (int loop = 0; loop < loopCount; ++loop)
			{
				List<int> snapshot = list.beginOldEnumerator();
				for (int i = 0, snapshotCount = snapshot.Count; i < snapshotCount; ++i)
				{
					local += snapshot[i];
				}
				list.endOldEnumerator();
			}
			checksum += local;
		});
		List<int> mainList = list.getMainList();
		BenchmarkResult mainFor = measure(() =>
		{
			long local = 0;
			for (int loop = 0; loop < loopCount; ++loop)
			{
				for (int i = 0, mainCount = mainList.Count; i < mainCount; ++i)
				{
					local += mainList[i];
				}
			}
			checksum += local;
		});
		builder.AppendLine("---------------- Count:" + count + " LoopCount:" + loopCount + " ----------------");
		appendResult(builder, "Production Index", production, loopCount, count);
		appendResult(builder, "Old ListEnum", oldEnumerator, loopCount, count);
		appendResult(builder, "Snapshot For", snapshotFor, loopCount, count);
		appendResult(builder, "MainList For", mainFor, loopCount, count);
		builder.AppendLine("Old / Production".PadRight(24) + ": " + (oldEnumerator.mMedianMS / production.mMedianMS).ToString("F2") + "x");
		builder.AppendLine("Production / Snapshot".PadRight(24) + ": " + (production.mMedianMS / snapshotFor.mMedianMS).ToString("F2") + "x");
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
