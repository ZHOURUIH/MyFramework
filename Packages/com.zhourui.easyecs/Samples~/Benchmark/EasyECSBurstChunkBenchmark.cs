using System;
using System.Diagnostics;
using EasyECS;
using Unity.Burst;
using Unity.Jobs;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine;
using Debug = UnityEngine.Debug;
public static class EasyECSBurstChunkBenchmark
{
	private const int ENTITY_COUNT = 2000000;
	private const int SAMPLE_COUNT = 21;
	private const int WARMUP_COUNT = 5;
	private const int ELEMENT_BATCH_COUNT = 256;
	private const int FIXED_CHUNK_SIZE = 8192;
	private static double mResultSink;
	private struct BenchmarkResult
	{
		public double mSingleMs;
		public double mElementParallelMs;
		public double mFixedChunkMs;
		public double mAutoChunkMs;
	}
	[BurstCompile(OptimizeFor = OptimizeFor.Performance)]
	private unsafe struct FloatSingleJob : IJob
	{
		public Float_ECSList.BurstView mView;
		public void Execute() { for (int i = 0; i < mView.Count; ++i) mView.mValue[i] = mView.mValue[i] * 1.000001f + 0.125f; }
	}
	[BurstCompile(OptimizeFor = OptimizeFor.Performance)]
	private unsafe struct FloatElementJob : IJobParallelFor
	{
		public Float_ECSList.BurstView mView;
		public void Execute(int index) { mView.mValue[index] = mView.mValue[index] * 1.000001f + 0.125f; }
	}
	[BurstCompile(OptimizeFor = OptimizeFor.Performance)]
	private unsafe struct FloatChunkJob : IJobParallelFor
	{
		public Float_ECSList.BurstView mView;
		public int mChunkSize;
		public void Execute(int chunkIndex)
		{
			mView.GetChunkRange(chunkIndex, mChunkSize, out int start, out int count);
			int end = start + count;
			for (int i = start; i < end; ++i) mView.mValue[i] = mView.mValue[i] * 1.000001f + 0.125f;
		}
	}
	[BurstCompile(OptimizeFor = OptimizeFor.Performance)]
	private unsafe struct IntSingleJob : IJob
	{
		public Int_ECSList.BurstView mView;
		public void Execute() { for (int i = 0; i < mView.Count; ++i) mView.mValue[i] += 3; }
	}
	[BurstCompile(OptimizeFor = OptimizeFor.Performance)]
	private unsafe struct IntElementJob : IJobParallelFor
	{
		public Int_ECSList.BurstView mView;
		public void Execute(int index) { mView.mValue[index] += 3; }
	}
	[BurstCompile(OptimizeFor = OptimizeFor.Performance)]
	private unsafe struct IntChunkJob : IJobParallelFor
	{
		public Int_ECSList.BurstView mView;
		public int mChunkSize;
		public void Execute(int chunkIndex)
		{
			mView.GetChunkRange(chunkIndex, mChunkSize, out int start, out int count);
			int end = start + count;
			for (int i = start; i < end; ++i) mView.mValue[i] += 3;
		}
	}
	[BurstCompile(OptimizeFor = OptimizeFor.Performance)]
	private unsafe struct Vector2SingleJob : IJob
	{
		public Vector2_ECSList.BurstView mView;
		public void Execute()
		{
			for (int i = 0; i < mView.Count; ++i)
			{
				mView.x[i] = mView.x[i] * 1.000001f + 0.125f;
				mView.y[i] = mView.y[i] * 0.999999f - 0.25f;
			}
		}
	}
	[BurstCompile(OptimizeFor = OptimizeFor.Performance)]
	private unsafe struct Vector2ElementJob : IJobParallelFor
	{
		public Vector2_ECSList.BurstView mView;
		public void Execute(int index)
		{
			mView.x[index] = mView.x[index] * 1.000001f + 0.125f;
			mView.y[index] = mView.y[index] * 0.999999f - 0.25f;
		}
	}
	[BurstCompile(OptimizeFor = OptimizeFor.Performance)]
	private unsafe struct Vector2ChunkJob : IJobParallelFor
	{
		public Vector2_ECSList.BurstView mView;
		public int mChunkSize;
		public void Execute(int chunkIndex)
		{
			mView.GetChunkRange(chunkIndex, mChunkSize, out int start, out int count);
			int end = start + count;
			for (int i = start; i < end; ++i)
			{
				mView.x[i] = mView.x[i] * 1.000001f + 0.125f;
				mView.y[i] = mView.y[i] * 0.999999f - 0.25f;
			}
		}
	}
	[BurstCompile(OptimizeFor = OptimizeFor.Performance)]
	private unsafe struct Color32SingleJob : IJob
	{
		public Color32_ECSList.BurstView mView;
		public void Execute()
		{
			for (int i = 0; i < mView.Count; ++i)
			{
				mView.r[i] += 1;
				mView.g[i] += 2;
				mView.b[i] += 3;
				mView.a[i] += 4;
			}
		}
	}
	[BurstCompile(OptimizeFor = OptimizeFor.Performance)]
	private unsafe struct Color32ElementJob : IJobParallelFor
	{
		public Color32_ECSList.BurstView mView;
		public void Execute(int index)
		{
			mView.r[index] += 1;
			mView.g[index] += 2;
			mView.b[index] += 3;
			mView.a[index] += 4;
		}
	}
	[BurstCompile(OptimizeFor = OptimizeFor.Performance)]
	private unsafe struct Color32ChunkJob : IJobParallelFor
	{
		public Color32_ECSList.BurstView mView;
		public int mChunkSize;
		public void Execute(int chunkIndex)
		{
			mView.GetChunkRange(chunkIndex, mChunkSize, out int start, out int count);
			int end = start + count;
			for (int i = start; i < end; ++i)
			{
				mView.r[i] += 1;
				mView.g[i] += 2;
				mView.b[i] += 3;
				mView.a[i] += 4;
			}
		}
	}
	public static void runBenchmark()
	{
		int autoChunkSize = EasyECSJobUtility.calculateChunkSize(ENTITY_COUNT);
		int fixedJobCount = (ENTITY_COUNT + FIXED_CHUNK_SIZE - 1) / FIXED_CHUNK_SIZE;
		int autoJobCount = (ENTITY_COUNT + autoChunkSize - 1) / autoChunkSize;
		Debug.Log("================ EasyECS Burst Chunk Benchmark Start ================");
		Debug.Log("EntityCount:" + ENTITY_COUNT + ",SampleCount:" + SAMPLE_COUNT + ",WarmupCount:" + WARMUP_COUNT + ",WorkerCount:" + JobsUtility.JobWorkerCount + ",JobsPerWorker:" + EasyECSJobUtility.JOB_COUNT_PER_WORKER + ",MinEntityPerJob:" + EasyECSJobUtility.MIN_ENTITY_COUNT_PER_JOB);
		Debug.Log("FixedChunkSize:" + FIXED_CHUNK_SIZE + ",FixedJobCount:" + fixedJobCount + ",AutoChunkSize:" + autoChunkSize + ",AutoJobCount:" + autoJobCount);
		Debug.Log("定义:Single=单个Burst IJob;ElementParallel=逐元素IJobParallelFor;FixedChunk=固定8192;AutoChunk=根据EntityCount+JobWorkerCount自动计算ChunkSize。");
		runCorrectnessTest();
		runChunkSelectionTable();
		BenchmarkResult floatResult = runFloatBenchmark(autoChunkSize);
		BenchmarkResult intResult = runIntBenchmark(autoChunkSize);
		BenchmarkResult vector2Result = runVector2Benchmark(autoChunkSize);
		BenchmarkResult color32Result = runColor32Benchmark(autoChunkSize);
		double autoVsSingleGeo = geometricMean4(ratio(floatResult.mSingleMs, floatResult.mAutoChunkMs), ratio(intResult.mSingleMs, intResult.mAutoChunkMs), ratio(vector2Result.mSingleMs, vector2Result.mAutoChunkMs), ratio(color32Result.mSingleMs, color32Result.mAutoChunkMs));
		double autoVsElementGeo = geometricMean4(ratio(floatResult.mElementParallelMs, floatResult.mAutoChunkMs), ratio(intResult.mElementParallelMs, intResult.mAutoChunkMs), ratio(vector2Result.mElementParallelMs, vector2Result.mAutoChunkMs), ratio(color32Result.mElementParallelMs, color32Result.mAutoChunkMs));
		double autoVsFixedGeo = geometricMean4(ratio(floatResult.mFixedChunkMs, floatResult.mAutoChunkMs), ratio(intResult.mFixedChunkMs, intResult.mAutoChunkMs), ratio(vector2Result.mFixedChunkMs, vector2Result.mAutoChunkMs), ratio(color32Result.mFixedChunkMs, color32Result.mAutoChunkMs));
		Debug.Log("\n================ Burst Chunk Summary ================\n" +
			"AutoChunk / Single Geomean Speedup : " + autoVsSingleGeo.ToString("F3") + "x\n" +
			"AutoChunk / ElementParallel        : " + autoVsElementGeo.ToString("F3") + "x\n" +
			"AutoChunk / Fixed8192              : " + autoVsFixedGeo.ToString("F3") + "x\n" +
			"==================================================");
		Debug.Log("ResultSink:" + mResultSink);
		Debug.Log("================ EasyECS Burst Chunk Benchmark End ==================");
	}
	private static void runCorrectnessTest()
	{
		if (EasyECSJobUtility.calculateChunkSize(0) != 1) throw new Exception("calculateChunkSize(0)错误");
		if (EasyECSJobUtility.calculateChunkSize(17) != 17) throw new Exception("小数据量ChunkSize错误");
		Int_ECSList list = new Int_ECSList(17);
		try
		{
			for (int i = 0; i < 17; ++i) list.Add(i);
			int chunkSize = EasyECSJobUtility.calculateChunkSize(list.Count);
			Int_ECSList.BurstView view = list.GetBurstView();
			list.ScheduleBurstChunk(new IntChunkJob { mView = view, mChunkSize = chunkSize }, chunkSize);
			list.CompleteBurstJobs();
			for (int i = 0; i < 17; ++i) if (list[i] != i + 3) throw new Exception("AutoChunk结果错误,index:" + i);
			Debug.Log("Burst AutoChunk Correctness Pass:calculateChunkSize/GetChunkRange/ScheduleBurstChunk/Tail");
		}
		finally { list.Dispose(); }
	}
	private static void runChunkSelectionTable()
	{
		int[] counts = { 1000, 10000, 50000, 100000, 500000, 1000000, 2000000, 5000000 };
		Debug.Log("\n================ Auto Chunk Selection ================");
		for (int i = 0; i < counts.Length; ++i)
		{
			int entityCount = counts[i];
			int chunkSize = EasyECSJobUtility.calculateChunkSize(entityCount);
			int jobCount = (entityCount + chunkSize - 1) / chunkSize;
			Debug.Log("EntityCount:" + entityCount.ToString().PadLeft(8) + " | ChunkSize:" + chunkSize.ToString().PadLeft(6) + " | JobCount:" + jobCount.ToString().PadLeft(4));
		}
		Debug.Log("==================================================");
	}
	private static BenchmarkResult runFloatBenchmark(int autoChunkSize)
	{
		float[] source = new float[ENTITY_COUNT];
		for (int i = 0; i < source.Length; ++i) source[i] = i * 0.001f;
		Float_ECSList list = new Float_ECSList(ENTITY_COUNT);
		try
		{
			list.AddRange(source);
			Float_ECSList.BurstView view = list.GetBurstView();
			BenchmarkResult result = measure("Float MAD", () => new FloatSingleJob { mView = view }.Run(), () => { list.ScheduleBurst(new FloatElementJob { mView = view }, ELEMENT_BATCH_COUNT); list.CompleteBurstJobs(); }, () => { list.ScheduleBurstChunk(new FloatChunkJob { mView = view, mChunkSize = FIXED_CHUNK_SIZE }, FIXED_CHUNK_SIZE); list.CompleteBurstJobs(); }, () => { list.ScheduleBurstChunk(new FloatChunkJob { mView = view, mChunkSize = autoChunkSize }, autoChunkSize); list.CompleteBurstJobs(); }, autoChunkSize);
			mResultSink += list[ENTITY_COUNT - 1];
			return result;
		}
		finally { list.Dispose(); }
	}
	private static BenchmarkResult runIntBenchmark(int autoChunkSize)
	{
		int[] source = new int[ENTITY_COUNT];
		for (int i = 0; i < source.Length; ++i) source[i] = i;
		Int_ECSList list = new Int_ECSList(ENTITY_COUNT);
		try
		{
			list.AddRange(source);
			Int_ECSList.BurstView view = list.GetBurstView();
			BenchmarkResult result = measure("Int Add", () => new IntSingleJob { mView = view }.Run(), () => { list.ScheduleBurst(new IntElementJob { mView = view }, ELEMENT_BATCH_COUNT); list.CompleteBurstJobs(); }, () => { list.ScheduleBurstChunk(new IntChunkJob { mView = view, mChunkSize = FIXED_CHUNK_SIZE }, FIXED_CHUNK_SIZE); list.CompleteBurstJobs(); }, () => { list.ScheduleBurstChunk(new IntChunkJob { mView = view, mChunkSize = autoChunkSize }, autoChunkSize); list.CompleteBurstJobs(); }, autoChunkSize);
			mResultSink += list[ENTITY_COUNT - 1];
			return result;
		}
		finally { list.Dispose(); }
	}
	private static BenchmarkResult runVector2Benchmark(int autoChunkSize)
	{
		Vector2[] source = new Vector2[ENTITY_COUNT];
		for (int i = 0; i < source.Length; ++i) source[i] = new Vector2(i * 0.001f, i * 0.002f);
		Vector2_ECSList list = new Vector2_ECSList(ENTITY_COUNT);
		try
		{
			list.AddRange(source);
			Vector2_ECSList.BurstView view = list.GetBurstView();
			BenchmarkResult result = measure("Vector2 MAD 双float SoA", () => new Vector2SingleJob { mView = view }.Run(), () => { list.ScheduleBurst(new Vector2ElementJob { mView = view }, ELEMENT_BATCH_COUNT); list.CompleteBurstJobs(); }, () => { list.ScheduleBurstChunk(new Vector2ChunkJob { mView = view, mChunkSize = FIXED_CHUNK_SIZE }, FIXED_CHUNK_SIZE); list.CompleteBurstJobs(); }, () => { list.ScheduleBurstChunk(new Vector2ChunkJob { mView = view, mChunkSize = autoChunkSize }, autoChunkSize); list.CompleteBurstJobs(); }, autoChunkSize);
			Vector2 last = list[ENTITY_COUNT - 1];
			mResultSink += last.x + last.y;
			return result;
		}
		finally { list.Dispose(); }
	}
	private static BenchmarkResult runColor32Benchmark(int autoChunkSize)
	{
		Color32[] source = new Color32[ENTITY_COUNT];
		for (int i = 0; i < source.Length; ++i) source[i] = new Color32((byte)i, (byte)(i >> 1), (byte)(i >> 2), 255);
		Color32_ECSList list = new Color32_ECSList(ENTITY_COUNT);
		try
		{
			list.AddRange(source);
			Color32_ECSList.BurstView view = list.GetBurstView();
			BenchmarkResult result = measure("Color32 Add 四byte SoA", () => new Color32SingleJob { mView = view }.Run(), () => { list.ScheduleBurst(new Color32ElementJob { mView = view }, ELEMENT_BATCH_COUNT); list.CompleteBurstJobs(); }, () => { list.ScheduleBurstChunk(new Color32ChunkJob { mView = view, mChunkSize = FIXED_CHUNK_SIZE }, FIXED_CHUNK_SIZE); list.CompleteBurstJobs(); }, () => { list.ScheduleBurstChunk(new Color32ChunkJob { mView = view, mChunkSize = autoChunkSize }, autoChunkSize); list.CompleteBurstJobs(); }, autoChunkSize);
			Color32 last = list[ENTITY_COUNT - 1];
			mResultSink += last.r + last.g + last.b + last.a;
			return result;
		}
		finally { list.Dispose(); }
	}
	private static BenchmarkResult measure(string title, Action single, Action elementParallel, Action fixedChunk, Action autoChunk, int autoChunkSize)
	{
		Action[] actions = { single, elementParallel, fixedChunk, autoChunk };
		for (int warmup = 0; warmup < WARMUP_COUNT; ++warmup)
		{
			int start = warmup & 3;
			for (int step = 0; step < 4; ++step) actions[(start + step) & 3]();
		}
		long[][] samples = { new long[SAMPLE_COUNT], new long[SAMPLE_COUNT], new long[SAMPLE_COUNT], new long[SAMPLE_COUNT] };
		Stopwatch watch = new Stopwatch();
		for (int sample = 0; sample < SAMPLE_COUNT; ++sample)
		{
			int start = sample & 3;
			for (int step = 0; step < 4; ++step)
			{
				int index = (start + step) & 3;
				watch.Restart();
				actions[index]();
				watch.Stop();
				samples[index][sample] = watch.ElapsedTicks;
			}
		}
		BenchmarkResult result = new BenchmarkResult
		{
			mSingleMs = medianMilliseconds(samples[0]),
			mElementParallelMs = medianMilliseconds(samples[1]),
			mFixedChunkMs = medianMilliseconds(samples[2]),
			mAutoChunkMs = medianMilliseconds(samples[3])
		};
		int autoJobCount = (ENTITY_COUNT + autoChunkSize - 1) / autoChunkSize;
		Debug.Log("\n================ Burst Chunk " + title + " ================\n" +
			"Burst Single IJob        Median:" + result.mSingleMs.ToString("F3").PadLeft(9) + " ms\n" +
			"Element ParallelFor      Median:" + result.mElementParallelMs.ToString("F3").PadLeft(9) + " ms | /Single:" + ratio(result.mSingleMs, result.mElementParallelMs).ToString("F3") + "x\n" +
			"Fixed Chunk(8192)        Median:" + result.mFixedChunkMs.ToString("F3").PadLeft(9) + " ms | /Single:" + ratio(result.mSingleMs, result.mFixedChunkMs).ToString("F3") + "x\n" +
			"Auto Chunk(" + autoChunkSize + "," + autoJobCount + " jobs) Median:" + result.mAutoChunkMs.ToString("F3").PadLeft(9) + " ms | /Single:" + ratio(result.mSingleMs, result.mAutoChunkMs).ToString("F3") + "x | /Element:" + ratio(result.mElementParallelMs, result.mAutoChunkMs).ToString("F3") + "x | /Fixed:" + ratio(result.mFixedChunkMs, result.mAutoChunkMs).ToString("F3") + "x\n" +
			"==================================================");
		return result;
	}
	private static double medianMilliseconds(long[] ticks)
	{
		long[] copy = (long[])ticks.Clone();
		Array.Sort(copy);
		return copy[copy.Length >> 1] * 1000.0 / Stopwatch.Frequency;
	}
	private static double ratio(double baseline, double target) { return target <= 0.0 ? 0.0 : baseline / target; }
	private static double geometricMean4(double a, double b, double c, double d) { return Math.Pow(a * b * c * d, 0.25); }
}
