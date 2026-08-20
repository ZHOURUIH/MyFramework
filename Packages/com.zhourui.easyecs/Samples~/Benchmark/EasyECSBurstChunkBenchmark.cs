using System;
using System.Diagnostics;
using EasyECS;
using Unity.Burst;
using Unity.Jobs;
using UnityEngine;
using Debug = UnityEngine.Debug;
public static class EasyECSBurstChunkBenchmark
{
	private const int ENTITY_COUNT = 2000000;
	private const int SAMPLE_COUNT = 21;
	private const int WARMUP_COUNT = 5;
	private const int ELEMENT_BATCH_COUNT = 256;
	private const int CHUNK_SIZE = 8192;
	private static double mResultSink;
	private struct BenchmarkResult
	{
		public double mSingleMs;
		public double mElementParallelMs;
		public double mChunkDirectMs;
		public double mChunkContainerMs;
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
		Debug.Log("================ EasyECS Burst Chunk Benchmark Start ================");
		Debug.Log("EntityCount:" + ENTITY_COUNT + ",SampleCount:" + SAMPLE_COUNT + ",WarmupCount:" + WARMUP_COUNT + ",ElementBatch:" + ELEMENT_BATCH_COUNT + ",ChunkSize:" + CHUNK_SIZE);
		Debug.Log("定义:Single=单个Burst IJob连续循环;ElementParallel=原ScheduleBurst逐元素IJobParallelFor;ChunkDirect=手动Schedule(chunkCount,1);ChunkContainer=EasyECS ScheduleBurstChunk。");
		runCorrectnessTest();
		BenchmarkResult floatResult = runFloatBenchmark();
		BenchmarkResult intResult = runIntBenchmark();
		BenchmarkResult vector2Result = runVector2Benchmark();
		BenchmarkResult color32Result = runColor32Benchmark();
		double directGeo = geometricMean4(ratio(floatResult.mSingleMs, floatResult.mChunkDirectMs), ratio(intResult.mSingleMs, intResult.mChunkDirectMs), ratio(vector2Result.mSingleMs, vector2Result.mChunkDirectMs), ratio(color32Result.mSingleMs, color32Result.mChunkDirectMs));
		double containerGeo = geometricMean4(ratio(floatResult.mSingleMs, floatResult.mChunkContainerMs), ratio(intResult.mSingleMs, intResult.mChunkContainerMs), ratio(vector2Result.mSingleMs, vector2Result.mChunkContainerMs), ratio(color32Result.mSingleMs, color32Result.mChunkContainerMs));
		double vsElementGeo = geometricMean4(ratio(floatResult.mElementParallelMs, floatResult.mChunkContainerMs), ratio(intResult.mElementParallelMs, intResult.mChunkContainerMs), ratio(vector2Result.mElementParallelMs, vector2Result.mChunkContainerMs), ratio(color32Result.mElementParallelMs, color32Result.mChunkContainerMs));
		Debug.Log("\n================ Burst Chunk Summary ================\n" +
			"Chunk Direct / Single Geomean Speedup   : " + directGeo.ToString("F3") + "x\n" +
			"Chunk Container / Single Geomean Speedup: " + containerGeo.ToString("F3") + "x\n" +
			"Chunk Container / ElementParallel       : " + vsElementGeo.ToString("F3") + "x\n" +
			"==================================================");
		Debug.Log("ResultSink:" + mResultSink);
		Debug.Log("================ EasyECS Burst Chunk Benchmark End ==================");
	}
	private static void runCorrectnessTest()
	{
		Int_ECSList list = new Int_ECSList(17);
		try
		{
			for (int i = 0; i < 17; ++i) list.Add(i);
			Int_ECSList.BurstView view = list.GetBurstView();
			if (view.GetChunkCount(8) != 3) throw new Exception("GetChunkCount错误");
			view.GetChunkRange(0, 8, out int start0, out int count0);
			view.GetChunkRange(2, 8, out int start2, out int count2);
			if (start0 != 0 || count0 != 8 || start2 != 16 || count2 != 1) throw new Exception("GetChunkRange错误");
			list.ScheduleBurstChunk(new IntChunkJob { mView = view, mChunkSize = 8 }, 8);
			list.CompleteBurstJobs();
			for (int i = 0; i < 17; ++i) if (list[i] != i + 3) throw new Exception("ScheduleBurstChunk结果错误,index:" + i);
			Debug.Log("Burst Chunk Correctness Pass:GetChunkCount/GetChunkRange/ScheduleBurstChunk/Tail");
		}
		finally { list.Dispose(); }
	}
	private static BenchmarkResult runFloatBenchmark()
	{
		float[] source = new float[ENTITY_COUNT];
		for (int i = 0; i < source.Length; ++i) source[i] = i * 0.001f;
		Float_ECSList list = new Float_ECSList(ENTITY_COUNT);
		try
		{
			list.AddRange(source);
			Float_ECSList.BurstView view = list.GetBurstView();
			BenchmarkResult result = measure("Float MAD", () => new FloatSingleJob { mView = view }.Run(), () => { list.ScheduleBurst(new FloatElementJob { mView = view }, ELEMENT_BATCH_COUNT); list.CompleteBurstJobs(); }, () => new FloatChunkJob { mView = view, mChunkSize = CHUNK_SIZE }.Schedule(view.GetChunkCount(CHUNK_SIZE), 1).Complete(), () => { list.ScheduleBurstChunk(new FloatChunkJob { mView = view, mChunkSize = CHUNK_SIZE }, CHUNK_SIZE); list.CompleteBurstJobs(); });
			mResultSink += list[ENTITY_COUNT - 1];
			return result;
		}
		finally { list.Dispose(); }
	}
	private static BenchmarkResult runIntBenchmark()
	{
		int[] source = new int[ENTITY_COUNT];
		for (int i = 0; i < source.Length; ++i) source[i] = i;
		Int_ECSList list = new Int_ECSList(ENTITY_COUNT);
		try
		{
			list.AddRange(source);
			Int_ECSList.BurstView view = list.GetBurstView();
			BenchmarkResult result = measure("Int Add", () => new IntSingleJob { mView = view }.Run(), () => { list.ScheduleBurst(new IntElementJob { mView = view }, ELEMENT_BATCH_COUNT); list.CompleteBurstJobs(); }, () => new IntChunkJob { mView = view, mChunkSize = CHUNK_SIZE }.Schedule(view.GetChunkCount(CHUNK_SIZE), 1).Complete(), () => { list.ScheduleBurstChunk(new IntChunkJob { mView = view, mChunkSize = CHUNK_SIZE }, CHUNK_SIZE); list.CompleteBurstJobs(); });
			mResultSink += list[ENTITY_COUNT - 1];
			return result;
		}
		finally { list.Dispose(); }
	}
	private static BenchmarkResult runVector2Benchmark()
	{
		Vector2[] source = new Vector2[ENTITY_COUNT];
		for (int i = 0; i < source.Length; ++i) source[i] = new Vector2(i * 0.001f, i * 0.002f);
		Vector2_ECSList list = new Vector2_ECSList(ENTITY_COUNT);
		try
		{
			list.AddRange(source);
			Vector2_ECSList.BurstView view = list.GetBurstView();
			BenchmarkResult result = measure("Vector2 MAD 双float SoA", () => new Vector2SingleJob { mView = view }.Run(), () => { list.ScheduleBurst(new Vector2ElementJob { mView = view }, ELEMENT_BATCH_COUNT); list.CompleteBurstJobs(); }, () => new Vector2ChunkJob { mView = view, mChunkSize = CHUNK_SIZE }.Schedule(view.GetChunkCount(CHUNK_SIZE), 1).Complete(), () => { list.ScheduleBurstChunk(new Vector2ChunkJob { mView = view, mChunkSize = CHUNK_SIZE }, CHUNK_SIZE); list.CompleteBurstJobs(); });
			Vector2 last = list[ENTITY_COUNT - 1];
			mResultSink += last.x + last.y;
			return result;
		}
		finally { list.Dispose(); }
	}
	private static BenchmarkResult runColor32Benchmark()
	{
		Color32[] source = new Color32[ENTITY_COUNT];
		for (int i = 0; i < source.Length; ++i) source[i] = new Color32((byte)i, (byte)(i >> 1), (byte)(i >> 2), 255);
		Color32_ECSList list = new Color32_ECSList(ENTITY_COUNT);
		try
		{
			list.AddRange(source);
			Color32_ECSList.BurstView view = list.GetBurstView();
			BenchmarkResult result = measure("Color32 Add 四byte SoA", () => new Color32SingleJob { mView = view }.Run(), () => { list.ScheduleBurst(new Color32ElementJob { mView = view }, ELEMENT_BATCH_COUNT); list.CompleteBurstJobs(); }, () => new Color32ChunkJob { mView = view, mChunkSize = CHUNK_SIZE }.Schedule(view.GetChunkCount(CHUNK_SIZE), 1).Complete(), () => { list.ScheduleBurstChunk(new Color32ChunkJob { mView = view, mChunkSize = CHUNK_SIZE }, CHUNK_SIZE); list.CompleteBurstJobs(); });
			Color32 last = list[ENTITY_COUNT - 1];
			mResultSink += last.r + last.g + last.b + last.a;
			return result;
		}
		finally { list.Dispose(); }
	}
	private static BenchmarkResult measure(string title, Action single, Action elementParallel, Action chunkDirect, Action chunkContainer)
	{
		Action[] actions = { single, elementParallel, chunkDirect, chunkContainer };
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
			mChunkDirectMs = medianMilliseconds(samples[2]),
			mChunkContainerMs = medianMilliseconds(samples[3])
		};
		Debug.Log("\n================ Burst Chunk " + title + " ================\n" +
			"Burst Single IJob        Median:" + result.mSingleMs.ToString("F3").PadLeft(9) + " ms\n" +
			"Element ParallelFor      Median:" + result.mElementParallelMs.ToString("F3").PadLeft(9) + " ms | Speedup/Single:" + ratio(result.mSingleMs, result.mElementParallelMs).ToString("F3") + "x\n" +
			"Chunk Direct(8192)       Median:" + result.mChunkDirectMs.ToString("F3").PadLeft(9) + " ms | Speedup/Single:" + ratio(result.mSingleMs, result.mChunkDirectMs).ToString("F3") + "x\n" +
			"Chunk Container(8192)    Median:" + result.mChunkContainerMs.ToString("F3").PadLeft(9) + " ms | Speedup/Single:" + ratio(result.mSingleMs, result.mChunkContainerMs).ToString("F3") + "x | /Element:" + ratio(result.mElementParallelMs, result.mChunkContainerMs).ToString("F3") + "x\n" +
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
