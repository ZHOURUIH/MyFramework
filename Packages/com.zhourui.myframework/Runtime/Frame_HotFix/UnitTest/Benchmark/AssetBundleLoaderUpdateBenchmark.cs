using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;

public static class AssetBundleLoaderUpdateBenchmark
{
	private const int ACTIVE_BUNDLE_COUNT = 32;
	private const int MIN_FRAME_COUNT = 1000;
	private const int TARGET_VISIT_COUNT = 2000000;
	private const int WARMUP_COUNT = 2;
	private const int SAMPLE_COUNT = 9;
	private const float ELAPSED_TIME = 0.016f;
	private static readonly int[] mBundleCounts = { 100, 1000, 5000, 10000, 20000 };
	private sealed class BenchmarkAssetBundleLoader : AssetBundleLoader
	{
		public void prepare(int bundleCount, List<AssetBundleInfo> activeList)
		{
			mAssetBundleInfoList.Clear();
			mDelayUnloadAssetBundleList.Clear();
			activeList.Clear();
			for (int i = 0; i < bundleCount; ++i)
			{
				AssetBundleInfo info = new("benchmark_" + i);
				mAssetBundleInfoList.Add("benchmark_" + i, info);
				if (i < ACTIVE_BUNDLE_COUNT)
				{
					activeList.Add(info);
				}
			}
			mInited = true;
		}
		public void updateBaseline(float elapsedTime)
		{
			foreach (var bundle in mAssetBundleInfoList)
			{
				if (bundle.Value.getAssetBundle() != null)
				{
					bundle.Value.update(elapsedTime);
				}
			}
		}
	}
	public static void Run()
	{
		StringBuilder builder = new();
		builder.AppendLine("================ AssetBundleLoader Update Benchmark Start ================");
		builder.AppendLine("ActiveBundleCount:" + ACTIVE_BUNDLE_COUNT + " SampleCount:" + SAMPLE_COUNT + " WarmupCount:" + WARMUP_COUNT);
		builder.AppendLine("说明:Baseline保留优化前的全量AB元数据扫描; Optimized调用当前AssetBundleLoader.update。");
		builder.AppendLine("说明:不包含AssetBundle.Load/Unload成本,这里只测试每帧调度与扫描成本。");
		foreach (int bundleCount in mBundleCounts)
		{
			runCase(builder, bundleCount);
		}
		builder.AppendLine("================ AssetBundleLoader Update Benchmark End ==================");
		UnityEngine.Debug.Log(builder.ToString());
	}
	private static void runCase(StringBuilder builder, int bundleCount)
	{
		BenchmarkAssetBundleLoader loader = new();
		List<AssetBundleInfo> activeList = new(ACTIVE_BUNDLE_COUNT);
		loader.prepare(bundleCount, activeList);
		int frameCount = Math.Max(MIN_FRAME_COUNT, TARGET_VISIT_COUNT / bundleCount);
		BenchmarkResult baseline = measure(() => runBaselineScan(loader, frameCount));
		BenchmarkResult optimized = measure(() => runOptimized(loader, frameCount));
		BenchmarkResult active = measure(() => runActiveList(activeList, frameCount));
		builder.AppendLine("---------------- BundleCount:" + bundleCount + " FrameCount:" + frameCount + " ----------------");
		appendResult(builder, "Baseline Scan", baseline, frameCount);
		appendResult(builder, "Optimized Update", optimized, frameCount);
		appendResult(builder, "ActiveList 32", active, frameCount);
		double ratio = optimized.mMedianMS > 0.0 ? baseline.mMedianMS / optimized.mMedianMS : 0.0;
		double saveNS = (baseline.mMedianMS - optimized.mMedianMS) * 1000000.0 / frameCount;
		builder.AppendLine("Baseline / Optimized".PadRight(24) + ": " + ratio.ToString("F2") + "x | Save:" + saveNS.ToString("F2") + " ns/frame");
	}
	private static void runBaselineScan(BenchmarkAssetBundleLoader loader, int frameCount)
	{
		for (int i = 0; i < frameCount; ++i)
		{
			loader.updateBaseline(ELAPSED_TIME);
		}
	}
	private static void runOptimized(BenchmarkAssetBundleLoader loader, int frameCount)
	{
		for (int i = 0; i < frameCount; ++i)
		{
			loader.update(ELAPSED_TIME);
		}
	}
	private static void runActiveList(List<AssetBundleInfo> activeList, int frameCount)
	{
		for (int frame = 0; frame < frameCount; ++frame)
		{
			for (int i = 0; i < activeList.Count; ++i)
			{
				activeList[i].update(ELAPSED_TIME);
			}
		}
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
	private static void appendResult(StringBuilder builder, string name, BenchmarkResult result, int frameCount)
	{
		double nsPerFrame = result.mMedianMS * 1000000.0 / frameCount;
		builder.AppendLine(name.PadRight(20) + " Median:" + result.mMedianMS.ToString("F3").PadLeft(9) + " ms | Min:" + result.mMinMS.ToString("F3").PadLeft(8) + " | Max:" + result.mMaxMS.ToString("F3").PadLeft(8) + " | " + nsPerFrame.ToString("F2").PadLeft(10) + " ns/frame");
	}
	private struct BenchmarkResult
	{
		public double mMedianMS;
		public double mMinMS;
		public double mMaxMS;
	}
}
