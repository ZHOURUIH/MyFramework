using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Unity.Profiling;
using static FrameBaseHotFix;

public static class ComponentOwnerUpdateBenchmark
{
	private const int TARGET_COMPONENT_VISITS = 300000;
	private const int MIN_FRAME_COUNT = 2000;
	private const int WARMUP_COUNT = 2;
	private const int SAMPLE_COUNT = 9;
	private const float ELAPSED_TIME = 0.016f;
	private static readonly int[] mComponentCounts = { 1, 4, 8, 32 };
	private sealed class BenchmarkComponent : GameComponent
	{
		public int mValue;
		public override void resetProperty()
		{
			base.resetProperty();
			mValue = 0;
		}
		public override void update(float elapsedTime)
		{
			base.update(elapsedTime);
			mValue += elapsedTime > 0.0f ? 1 : 0;
		}
	}
	private sealed class BenchmarkOwner : ComponentOwner
	{
		private ProfilerMarker mCachedMarker = new("BenchmarkComponent");
		public override void resetProperty()
		{
			base.resetProperty();
			mCachedMarker = new("BenchmarkComponent");
		}
		public void prepare(int componentCount)
		{
			setDestroy(false);
			mComponentList = new SafeList<GameComponent>();
			mDisableTypeList = null;
			for (int i = 0; i < componentCount; ++i)
			{
				BenchmarkComponent component = new();
				component.setDestroy(false);
				component.init(this);
				mComponentList.add(component);
			}
		}
		// 与当前ComponentOwner.update保持相同判断,仅去掉ProfilerScope,用于测Profiler本身的成本
		public void updateNoProfiler(float elapsedTime)
		{
			if (mComponentList == null || mComponentList.count() == 0)
			{
				return;
			}
			foreach (GameComponent com in mComponentList)
			{
				if (mComponentList.count() == 0 || isDestroy())
				{
					return;
				}
				if (com.isValid() && com.isActive() && !mDisableTypeList.contains(com.getType()))
				{
					com.update(com.isIgnoreTimeScale() ? mGameFrameworkHotFix.getUnscaledTime() : elapsedTime);
				}
			}
		}
		// 只优化最常见的禁用列表为空路径,其余语义与当前代码一致
		public void updateFastDisableCheck(float elapsedTime)
		{
			if (mComponentList == null || mComponentList.count() == 0)
			{
				return;
			}
			foreach (GameComponent com in mComponentList)
			{
				if (mComponentList.count() == 0 || isDestroy())
				{
					return;
				}
				if (com.isValid() && com.isActive() && (mDisableTypeList == null || !mDisableTypeList.Contains(com.getType())))
				{
					using var scope = new ProfilerScope(com.getTypeName());
					com.update(com.isIgnoreTimeScale() ? mGameFrameworkHotFix.getUnscaledTime() : elapsedTime);
				}
			}
		}
		// 模拟把组件ProfilerMarker缓存起来后的成本,不再每帧new ProfilerMarker(string)
		public void updateCachedMarker(float elapsedTime)
		{
			if (mComponentList == null || mComponentList.count() == 0)
			{
				return;
			}
			foreach (GameComponent com in mComponentList)
			{
				if (mComponentList.count() == 0 || isDestroy())
				{
					return;
				}
				if (com.isValid() && com.isActive() && (mDisableTypeList == null || !mDisableTypeList.Contains(com.getType())))
				{
					using var scope = mCachedMarker.Auto();
					com.update(com.isIgnoreTimeScale() ? mGameFrameworkHotFix.getUnscaledTime() : elapsedTime);
				}
			}
		}
		// SafeList安全遍历的理论核心成本,不含状态判断和Profiler
		public void updateSafeListCore(float elapsedTime)
		{
			foreach (GameComponent com in mComponentList)
			{
				com.update(elapsedTime);
			}
		}
		// 普通List只是下限参考,不代表可以直接替换SafeList,因为它不支持遍历过程中安全增删
		public void updateMainListCore(float elapsedTime)
		{
			List<GameComponent> list = mComponentList.getMainList();
			for (int i = 0, count = list.Count; i < count; ++i)
			{
				list[i].update(elapsedTime);
			}
		}
		public long getChecksum()
		{
			long value = 0;
			List<GameComponent> list = mComponentList.getMainList();
			for (int i = 0; i < list.Count; ++i)
			{
				value += ((BenchmarkComponent)list[i]).mValue;
			}
			return value;
		}
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
		builder.AppendLine("================ ComponentOwner Update Benchmark Start ================");
		builder.AppendLine("SampleCount:" + SAMPLE_COUNT + " WarmupCount:" + WARMUP_COUNT);
		builder.AppendLine("场景:组件全部Active,mDisableTypeList=null,更新过程中不增删组件。");
		builder.AppendLine("Current              : 当前ComponentOwner.update完整路径");
		builder.AppendLine("NoProfiler           : 仅移除ProfilerScope(string),其余判断保持当前实现");
		builder.AppendLine("FastDisableCheck     : 当前逻辑+禁用列表为空时不调用GetType");
		builder.AppendLine("CachedProfilerMarker : FastDisableCheck+复用ProfilerMarker,模拟正式可优化方案");
		builder.AppendLine("SafeListCore         : SafeList foreach+虚函数update的理论核心成本");
		builder.AppendLine("MainListCore         : 普通List for循环下限参考,不代表可直接替换SafeList");
		for (int i = 0; i < mComponentCounts.Length; ++i)
		{
			runCase(builder, mComponentCounts[i]);
		}
		builder.AppendLine("================ ComponentOwner Update Benchmark End ==================");
		UnityEngine.Debug.Log(builder.ToString());
	}
	private static void runCase(StringBuilder builder, int componentCount)
	{
		BenchmarkOwner owner = new();
		owner.prepare(componentCount);
		int frameCount = Math.Max(MIN_FRAME_COUNT, TARGET_COMPONENT_VISITS / componentCount);
		// 先调用一次,让SafeList同步mUpdateList并让组件TypeName完成缓存
		owner.updateNoProfiler(ELAPSED_TIME);
		owner.update(ELAPSED_TIME);
		BenchmarkResult current = measure(() => run(owner.update, frameCount));
		BenchmarkResult noProfiler = measure(() => run(owner.updateNoProfiler, frameCount));
		BenchmarkResult fastDisable = measure(() => run(owner.updateFastDisableCheck, frameCount));
		BenchmarkResult cachedMarker = measure(() => run(owner.updateCachedMarker, frameCount));
		BenchmarkResult safeListCore = measure(() => run(owner.updateSafeListCore, frameCount));
		BenchmarkResult mainListCore = measure(() => run(owner.updateMainListCore, frameCount));
		builder.AppendLine("---------------- ComponentCount:" + componentCount + " FrameCount:" + frameCount + " ----------------");
		appendResult(builder, "Current", current, frameCount, componentCount);
		appendResult(builder, "NoProfiler", noProfiler, frameCount, componentCount);
		appendResult(builder, "FastDisableCheck", fastDisable, frameCount, componentCount);
		appendResult(builder, "CachedProfilerMarker", cachedMarker, frameCount, componentCount);
		appendResult(builder, "SafeListCore", safeListCore, frameCount, componentCount);
		appendResult(builder, "MainListCore", mainListCore, frameCount, componentCount);
		appendCompare(builder, "Current / NoProfiler", current, noProfiler, frameCount);
		appendCompare(builder, "Current / CachedMarker", current, cachedMarker, frameCount);
		appendCompare(builder, "SafeList / MainList Core", safeListCore, mainListCore, frameCount);
		builder.AppendLine("Checksum:" + owner.getChecksum());
	}
	private static void run(Action<float> action, int frameCount)
	{
		for (int i = 0; i < frameCount; ++i)
		{
			action(ELAPSED_TIME);
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
	private static void appendResult(StringBuilder builder, string name, BenchmarkResult result, int frameCount, int componentCount)
	{
		double nsPerFrame = result.mMedianMS * 1000000.0 / frameCount;
		double nsPerComponent = nsPerFrame / componentCount;
		builder.AppendLine(name.PadRight(22) +
			" Median:" + result.mMedianMS.ToString("F3").PadLeft(9) +
			" ms | Min:" + result.mMinMS.ToString("F3").PadLeft(8) +
			" | Max:" + result.mMaxMS.ToString("F3").PadLeft(8) +
			" | " + nsPerFrame.ToString("F2").PadLeft(10) + " ns/frame" +
			" | " + nsPerComponent.ToString("F2").PadLeft(9) + " ns/component");
	}
	private static void appendCompare(StringBuilder builder, string name, BenchmarkResult current, BenchmarkResult target, int frameCount)
	{
		double ratio = target.mMedianMS > 0.0 ? current.mMedianMS / target.mMedianMS : 0.0;
		double saveNS = (current.mMedianMS - target.mMedianMS) * 1000000.0 / frameCount;
		builder.AppendLine(name.PadRight(28) + ": " + ratio.ToString("F2") + "x | Save:" + saveNS.ToString("F2") + " ns/frame");
	}
}
