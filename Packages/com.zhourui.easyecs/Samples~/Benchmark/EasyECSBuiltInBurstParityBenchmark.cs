using System;
using System.Diagnostics;
using EasyECS;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using Debug = UnityEngine.Debug;

public static class EasyECSBuiltInBurstParityBenchmark
{
	private const int ENTITY_COUNT = 2000000;
	private const int SAMPLE_COUNT = 21;
	private const int WARMUP_COUNT = 5;
	private const int INNER_LOOP_BATCH_COUNT = 256;
	private const int JOB_REPEAT_COUNT = 8;
	private static long mResultSink;
	[BurstCompile]
	private unsafe struct BaselineIntJob : IJobParallelFor
	{
		[NativeDisableUnsafePtrRestriction] public EasyECSParityIntData_ECSList.BurstView mData;
		public void Execute(int index)
		{
			mData.mValue[index] = mData.mValue[index] * 3 + 1;
		}
	}
	[BurstCompile]
	private unsafe struct BuiltInIntJob : IJobParallelFor
	{
		[NativeDisableUnsafePtrRestriction] public Int_ECSList.BurstView mData;
		public void Execute(int index)
		{
			mData.mValue[index] = mData.mValue[index] * 3 + 1;
		}
	}
	[BurstCompile]
	private unsafe struct SharedIntJob : IJobParallelFor
	{
		[NativeDisableUnsafePtrRestriction] public int* mValue;
		public void Execute(int index)
		{
			mValue[index] = mValue[index] * 3 + 1;
		}
	}
	[BurstCompile]
	private unsafe struct BaselineVector2Job : IJobParallelFor
	{
		[NativeDisableUnsafePtrRestriction] public EasyECSParityUVData_ECSList.BurstView mData;
		public void Execute(int index)
		{
			mData.x[index] = mData.x[index] * 1.001f + 0.01f;
			mData.y[index] = mData.y[index] * 0.999f + 0.02f;
		}
	}
	[BurstCompile]
	private unsafe struct BuiltInVector2Job : IJobParallelFor
	{
		[NativeDisableUnsafePtrRestriction] public Vector2_ECSList.BurstView mData;
		public void Execute(int index)
		{
			mData.x[index] = mData.x[index] * 1.001f + 0.01f;
			mData.y[index] = mData.y[index] * 0.999f + 0.02f;
		}
	}
	[BurstCompile]
	private unsafe struct SharedVector2Job : IJobParallelFor
	{
		[NativeDisableUnsafePtrRestriction] public float* x;
		[NativeDisableUnsafePtrRestriction] public float* y;
		public void Execute(int index)
		{
			x[index] = x[index] * 1.001f + 0.01f;
			y[index] = y[index] * 0.999f + 0.02f;
		}
	}
	[BurstCompile]
	private unsafe struct BaselineVector2IntJob : IJobParallelFor
	{
		[NativeDisableUnsafePtrRestriction] public EasyECSParityRangeData_ECSList.BurstView mData;
		public void Execute(int index)
		{
			mData.mStart[index] += 1;
			mData.mEnd[index] += 2;
		}
	}
	[BurstCompile]
	private unsafe struct BuiltInVector2IntJob : IJobParallelFor
	{
		[NativeDisableUnsafePtrRestriction] public Vector2Int_ECSList.BurstView mData;
		public void Execute(int index)
		{
			mData.x[index] += 1;
			mData.y[index] += 2;
		}
	}
	[BurstCompile]
	private unsafe struct SharedVector2IntJob : IJobParallelFor
	{
		[NativeDisableUnsafePtrRestriction] public int* x;
		[NativeDisableUnsafePtrRestriction] public int* y;
		public void Execute(int index)
		{
			x[index] += 1;
			y[index] += 2;
		}
	}
	public static void runBenchmark()
	{
		Debug.Log("================ EasyECS BuiltIn Burst Parity Benchmark Start ================");
		Debug.Log("EntityCount:" + ENTITY_COUNT + ",SampleCount:" + SAMPLE_COUNT + ",WarmupCount:" + WARMUP_COUNT + ",BatchCount:" + INNER_LOOP_BATCH_COUNT + ",JobRepeatCount:" + JOB_REPEAT_COUNT);
		runIntBenchmark();
		runVector2Benchmark();
		runVector2IntBenchmark();
		validateDictionaryBurstForwarding();
		Debug.Log("ResultSink:" + mResultSink);
		Debug.Log("================ EasyECS BuiltIn Burst Parity Benchmark End ==================");
	}
	private static unsafe void runIntBenchmark()
	{
		EasyECSParityIntData_ECSList baseline = new EasyECSParityIntData_ECSList(ENTITY_COUNT);
		Int_ECSList builtIn = new Int_ECSList(ENTITY_COUNT);
		try
		{
			for (int i = 0; i < ENTITY_COUNT; ++i)
			{
				baseline.Add(new EasyECSParityIntData(i + 1));
				builtIn.Add(i + 1);
			}
			validateBuiltInResizeCompletesBurst(builtIn);
			EasyECSParityIntData_ECSList.BurstView baselineView = baseline.GetBurstView();
			Int_ECSList.BurstView builtInView = builtIn.GetBurstView();
			logPointerLayout("Int", baselineView.mValue, builtInView.mValue);
			measureAndLog("Burst Int TypedView", () =>
			{
				for (int repeat = 0; repeat < JOB_REPEAT_COUNT; ++repeat)
				{
					baseline.ScheduleBurst(new BaselineIntJob { mData = baseline.GetBurstView() }, INNER_LOOP_BATCH_COUNT);
				}
				baseline.CompleteBurstJobs();
				return baseline.getValueColumn()[ENTITY_COUNT - 1];
			}, () =>
			{
				for (int repeat = 0; repeat < JOB_REPEAT_COUNT; ++repeat)
				{
					builtIn.ScheduleBurst(new BuiltInIntJob { mData = builtIn.GetBurstView() }, INNER_LOOP_BATCH_COUNT);
				}
				builtIn.CompleteBurstJobs();
				return builtIn.getValueColumn()[ENTITY_COUNT - 1];
			});
			measureAndLog("Burst Int SharedJob+ContainerSchedule", () =>
			{
				EasyECSParityIntData_ECSList.BurstView view = baseline.GetBurstView();
				for (int repeat = 0; repeat < JOB_REPEAT_COUNT; ++repeat)
				{
					baseline.ScheduleBurst(new SharedIntJob { mValue = view.mValue }, INNER_LOOP_BATCH_COUNT);
				}
				baseline.CompleteBurstJobs();
				return baseline.getValueColumn()[ENTITY_COUNT - 1];
			}, () =>
			{
				Int_ECSList.BurstView view = builtIn.GetBurstView();
				for (int repeat = 0; repeat < JOB_REPEAT_COUNT; ++repeat)
				{
					builtIn.ScheduleBurst(new SharedIntJob { mValue = view.mValue }, INNER_LOOP_BATCH_COUNT);
				}
				builtIn.CompleteBurstJobs();
				return builtIn.getValueColumn()[ENTITY_COUNT - 1];
			});
			measureAndLog("Burst Int SharedJob+DirectSchedule", () =>
			{
				EasyECSParityIntData_ECSList.BurstView view = baseline.GetBurstView();
				JobHandle handle = default(JobHandle);
				for (int repeat = 0; repeat < JOB_REPEAT_COUNT; ++repeat)
				{
					handle = IJobParallelForExtensions.Schedule(new SharedIntJob { mValue = view.mValue }, ENTITY_COUNT, INNER_LOOP_BATCH_COUNT, handle);
				}
				handle.Complete();
				return baseline.getValueColumn()[ENTITY_COUNT - 1];
			}, () =>
			{
				Int_ECSList.BurstView view = builtIn.GetBurstView();
				JobHandle handle = default(JobHandle);
				for (int repeat = 0; repeat < JOB_REPEAT_COUNT; ++repeat)
				{
					handle = IJobParallelForExtensions.Schedule(new SharedIntJob { mValue = view.mValue }, ENTITY_COUNT, INNER_LOOP_BATCH_COUNT, handle);
				}
				handle.Complete();
				return builtIn.getValueColumn()[ENTITY_COUNT - 1];
			});
		}
		finally
		{
			baseline.Dispose();
			builtIn.Dispose();
		}
	}
	private static unsafe void runVector2Benchmark()
	{
		EasyECSParityUVData_ECSList baseline = new EasyECSParityUVData_ECSList(ENTITY_COUNT);
		EasyECSParityUVData_ECSList baselineControl = new EasyECSParityUVData_ECSList(ENTITY_COUNT);
		Vector2_ECSList builtIn = new Vector2_ECSList(ENTITY_COUNT);
		try
		{
			for (int i = 0; i < ENTITY_COUNT; ++i)
			{
				Vector2 value = new Vector2(i * 0.001f, i * 0.002f);
				baseline.Add(new EasyECSParityUVData(value));
				baselineControl.Add(new EasyECSParityUVData(value));
				builtIn.Add(value);
			}
			EasyECSParityUVData_ECSList.BurstView baselineView = baseline.GetBurstView();
			EasyECSParityUVData_ECSList.BurstView baselineControlView = baselineControl.GetBurstView();
			Vector2_ECSList.BurstView builtInView = builtIn.GetBurstView();
			logPointerLayout("Vector2.x", baselineView.x, builtInView.x);
			logPointerLayout("Vector2.y", baselineView.y, builtInView.y);
			logPointerLayout("Vector2Control.x", baselineView.x, baselineControlView.x);
			logPointerLayout("Vector2Control.y", baselineView.y, baselineControlView.y);
			Debug.Log("BurstView Size:BaselineVector2=" + sizeof(EasyECSParityUVData_ECSList.BurstView) + ",BuiltInVector2=" + sizeof(Vector2_ECSList.BurstView));
			measureAndLog("Burst Vector2 TypedView", () =>
			{
				for (int repeat = 0; repeat < JOB_REPEAT_COUNT; ++repeat)
				{
					baseline.ScheduleBurst(new BaselineVector2Job { mData = baseline.GetBurstView() }, INNER_LOOP_BATCH_COUNT);
				}
				baseline.CompleteBurstJobs();
				return (long)baseline.getXColumn()[ENTITY_COUNT - 1];
			}, () =>
			{
				for (int repeat = 0; repeat < JOB_REPEAT_COUNT; ++repeat)
				{
					builtIn.ScheduleBurst(new BuiltInVector2Job { mData = builtIn.GetBurstView() }, INNER_LOOP_BATCH_COUNT);
				}
				builtIn.CompleteBurstJobs();
				return (long)builtIn.getXColumn()[ENTITY_COUNT - 1];
			});
			measureAndLog("Burst Vector2 SharedJob+ContainerSchedule", () =>
			{
				EasyECSParityUVData_ECSList.BurstView view = baseline.GetBurstView();
				for (int repeat = 0; repeat < JOB_REPEAT_COUNT; ++repeat)
				{
					baseline.ScheduleBurst(new SharedVector2Job { x = view.x, y = view.y }, INNER_LOOP_BATCH_COUNT);
				}
				baseline.CompleteBurstJobs();
				return (long)baseline.getXColumn()[ENTITY_COUNT - 1];
			}, () =>
			{
				Vector2_ECSList.BurstView view = builtIn.GetBurstView();
				for (int repeat = 0; repeat < JOB_REPEAT_COUNT; ++repeat)
				{
					builtIn.ScheduleBurst(new SharedVector2Job { x = view.x, y = view.y }, INNER_LOOP_BATCH_COUNT);
				}
				builtIn.CompleteBurstJobs();
				return (long)builtIn.getXColumn()[ENTITY_COUNT - 1];
			});
			measureAndLog("Burst Vector2 SharedJob+DirectSchedule", () =>
			{
				EasyECSParityUVData_ECSList.BurstView view = baseline.GetBurstView();
				JobHandle handle = default(JobHandle);
				for (int repeat = 0; repeat < JOB_REPEAT_COUNT; ++repeat)
				{
					handle = IJobParallelForExtensions.Schedule(new SharedVector2Job { x = view.x, y = view.y }, ENTITY_COUNT, INNER_LOOP_BATCH_COUNT, handle);
				}
				handle.Complete();
				return (long)baseline.getXColumn()[ENTITY_COUNT - 1];
			}, () =>
			{
				Vector2_ECSList.BurstView view = builtIn.GetBurstView();
				JobHandle handle = default(JobHandle);
				for (int repeat = 0; repeat < JOB_REPEAT_COUNT; ++repeat)
				{
					handle = IJobParallelForExtensions.Schedule(new SharedVector2Job { x = view.x, y = view.y }, ENTITY_COUNT, INNER_LOOP_BATCH_COUNT, handle);
				}
				handle.Complete();
				return (long)builtIn.getXColumn()[ENTITY_COUNT - 1];
			});
			measureAndLogDiagnostic("Burst Vector2 SelfControl SameMemory", () =>
			{
				EasyECSParityUVData_ECSList.BurstView view = baseline.GetBurstView();
				JobHandle handle = default(JobHandle);
				for (int repeat = 0; repeat < JOB_REPEAT_COUNT; ++repeat)
				{
					handle = IJobParallelForExtensions.Schedule(new SharedVector2Job { x = view.x, y = view.y }, ENTITY_COUNT, INNER_LOOP_BATCH_COUNT, handle);
				}
				handle.Complete();
				return (long)baseline.getXColumn()[ENTITY_COUNT - 1];
			}, () =>
			{
				EasyECSParityUVData_ECSList.BurstView view = baseline.GetBurstView();
				JobHandle handle = default(JobHandle);
				for (int repeat = 0; repeat < JOB_REPEAT_COUNT; ++repeat)
				{
					handle = IJobParallelForExtensions.Schedule(new SharedVector2Job { x = view.x, y = view.y }, ENTITY_COUNT, INNER_LOOP_BATCH_COUNT, handle);
				}
				handle.Complete();
				return (long)baseline.getXColumn()[ENTITY_COUNT - 1];
			});
			measureAndLogDiagnostic("Burst Vector2 SelfControl IndependentBaseline", () =>
			{
				EasyECSParityUVData_ECSList.BurstView view = baseline.GetBurstView();
				JobHandle handle = default(JobHandle);
				for (int repeat = 0; repeat < JOB_REPEAT_COUNT; ++repeat)
				{
					handle = IJobParallelForExtensions.Schedule(new SharedVector2Job { x = view.x, y = view.y }, ENTITY_COUNT, INNER_LOOP_BATCH_COUNT, handle);
				}
				handle.Complete();
				return (long)baseline.getXColumn()[ENTITY_COUNT - 1];
			}, () =>
			{
				EasyECSParityUVData_ECSList.BurstView view = baselineControl.GetBurstView();
				JobHandle handle = default(JobHandle);
				for (int repeat = 0; repeat < JOB_REPEAT_COUNT; ++repeat)
				{
					handle = IJobParallelForExtensions.Schedule(new SharedVector2Job { x = view.x, y = view.y }, ENTITY_COUNT, INNER_LOOP_BATCH_COUNT, handle);
				}
				handle.Complete();
				return (long)baselineControl.getXColumn()[ENTITY_COUNT - 1];
			});
		}
		finally
		{
			baseline.Dispose();
			baselineControl.Dispose();
			builtIn.Dispose();
		}
	}
	private static unsafe void runVector2IntBenchmark()
	{
		EasyECSParityRangeData_ECSList baseline = new EasyECSParityRangeData_ECSList(ENTITY_COUNT);
		Vector2Int_ECSList builtIn = new Vector2Int_ECSList(ENTITY_COUNT);
		try
		{
			for (int i = 0; i < ENTITY_COUNT; ++i)
			{
				baseline.Add(new EasyECSParityRangeData(i, i + 10));
				builtIn.Add(new Vector2Int(i, i + 10));
			}
			EasyECSParityRangeData_ECSList.BurstView baselineView = baseline.GetBurstView();
			Vector2Int_ECSList.BurstView builtInView = builtIn.GetBurstView();
			logPointerLayout("Vector2Int.x", baselineView.mStart, builtInView.x);
			logPointerLayout("Vector2Int.y", baselineView.mEnd, builtInView.y);
			Debug.Log("BurstView Size:BaselineVector2Int=" + sizeof(EasyECSParityRangeData_ECSList.BurstView) + ",BuiltInVector2Int=" + sizeof(Vector2Int_ECSList.BurstView));
			measureAndLog("Burst Vector2Int TypedView", () =>
			{
				for (int repeat = 0; repeat < JOB_REPEAT_COUNT; ++repeat)
				{
					baseline.ScheduleBurst(new BaselineVector2IntJob { mData = baseline.GetBurstView() }, INNER_LOOP_BATCH_COUNT);
				}
				baseline.CompleteBurstJobs();
				return (long)baseline.getStartColumn()[ENTITY_COUNT - 1] + baseline.getEndColumn()[ENTITY_COUNT - 1];
			}, () =>
			{
				for (int repeat = 0; repeat < JOB_REPEAT_COUNT; ++repeat)
				{
					builtIn.ScheduleBurst(new BuiltInVector2IntJob { mData = builtIn.GetBurstView() }, INNER_LOOP_BATCH_COUNT);
				}
				builtIn.CompleteBurstJobs();
				return (long)builtIn.getXColumn()[ENTITY_COUNT - 1] + builtIn.getYColumn()[ENTITY_COUNT - 1];
			});
			measureAndLog("Burst Vector2Int SharedJob+ContainerSchedule", () =>
			{
				EasyECSParityRangeData_ECSList.BurstView view = baseline.GetBurstView();
				for (int repeat = 0; repeat < JOB_REPEAT_COUNT; ++repeat)
				{
					baseline.ScheduleBurst(new SharedVector2IntJob { x = view.mStart, y = view.mEnd }, INNER_LOOP_BATCH_COUNT);
				}
				baseline.CompleteBurstJobs();
				return (long)baseline.getStartColumn()[ENTITY_COUNT - 1] + baseline.getEndColumn()[ENTITY_COUNT - 1];
			}, () =>
			{
				Vector2Int_ECSList.BurstView view = builtIn.GetBurstView();
				for (int repeat = 0; repeat < JOB_REPEAT_COUNT; ++repeat)
				{
					builtIn.ScheduleBurst(new SharedVector2IntJob { x = view.x, y = view.y }, INNER_LOOP_BATCH_COUNT);
				}
				builtIn.CompleteBurstJobs();
				return (long)builtIn.getXColumn()[ENTITY_COUNT - 1] + builtIn.getYColumn()[ENTITY_COUNT - 1];
			});
			measureAndLog("Burst Vector2Int SharedJob+DirectSchedule", () =>
			{
				EasyECSParityRangeData_ECSList.BurstView view = baseline.GetBurstView();
				JobHandle handle = default(JobHandle);
				for (int repeat = 0; repeat < JOB_REPEAT_COUNT; ++repeat)
				{
					handle = IJobParallelForExtensions.Schedule(new SharedVector2IntJob { x = view.mStart, y = view.mEnd }, ENTITY_COUNT, INNER_LOOP_BATCH_COUNT, handle);
				}
				handle.Complete();
				return (long)baseline.getStartColumn()[ENTITY_COUNT - 1] + baseline.getEndColumn()[ENTITY_COUNT - 1];
			}, () =>
			{
				Vector2Int_ECSList.BurstView view = builtIn.GetBurstView();
				JobHandle handle = default(JobHandle);
				for (int repeat = 0; repeat < JOB_REPEAT_COUNT; ++repeat)
				{
					handle = IJobParallelForExtensions.Schedule(new SharedVector2IntJob { x = view.x, y = view.y }, ENTITY_COUNT, INNER_LOOP_BATCH_COUNT, handle);
				}
				handle.Complete();
				return (long)builtIn.getXColumn()[ENTITY_COUNT - 1] + builtIn.getYColumn()[ENTITY_COUNT - 1];
			});
		}
		finally
		{
			baseline.Dispose();
			builtIn.Dispose();
		}
	}
	private static void validateBuiltInResizeCompletesBurst(Int_ECSList list)
	{
		int oldCapacity = list.Capacity;
		Int_ECSList.BurstView view = list.GetBurstView();
		list.ScheduleBurst(new BuiltInIntJob { mData = view }, INNER_LOOP_BATCH_COUNT);
		list.EnsureCapacity(oldCapacity + 1);
		if (list.Capacity <= oldCapacity)
		{
			throw new InvalidOperationException("Int_ECSList Burst Resize自动Complete失败");
		}
		Debug.Log("BuiltIn Burst Correctness Pass:Int_ECSList Resize自动Complete");
	}
	private static void validateDictionaryBurstForwarding()
	{
		Int_ECSDictionary<int> dict = new Int_ECSDictionary<int>(1024);
		try
		{
			for (int i = 0; i < 1024; ++i)
			{
				dict.Add(i, i + 1);
			}
			Int_ECSList.BurstView view = dict.GetBurstView();
			dict.ScheduleBurst(new BuiltInIntJob { mData = view }, 64);
			dict.CompleteBurstJobs();
			if (dict.GetValue(0) != 4)
			{
				throw new InvalidOperationException("Int_ECSDictionary Burst转发失败");
			}
			Debug.Log("BuiltIn Burst Correctness Pass:Int_ECSDictionary转发");
		}
		finally
		{
			dict.Dispose();
		}
	}
	private static void measureAndLog(string name, Func<long> baselineAction, Func<long> builtInAction)
	{
		double baselineMedian;
		double builtInMedian;
		double pairedMedianRatio;
		measurePair(baselineAction, builtInAction, out baselineMedian, out builtInMedian, out pairedMedianRatio);
		logCase(name, baselineMedian, builtInMedian, pairedMedianRatio);
	}
	private static void measureAndLogDiagnostic(string name, Func<long> firstAction, Func<long> secondAction)
	{
		double firstMedian;
		double secondMedian;
		double pairedMedianRatio;
		measurePair(firstAction, secondAction, out firstMedian, out secondMedian, out pairedMedianRatio);
		double medianRatio = secondMedian / firstMedian;
		Debug.Log("\n================ " + name + " ================\n" +
			"First          Median:" + firstMedian.ToString("F3").PadLeft(9) + " ms\n" +
			"Second         Median:" + secondMedian.ToString("F3").PadLeft(9) + " ms\n" +
			"MedianRatio          : " + medianRatio.ToString("F3") + "x\n" +
			"PairedMedianRatio    : " + pairedMedianRatio.ToString("F3") + "x\n" +
			"Diagnostic only      : NO RELEASE GATE\n" +
			"==================================================");
	}
	private static void measurePair(Func<long> baselineAction, Func<long> builtInAction, out double baselineMedian, out double builtInMedian, out double pairedMedianRatio)
	{
		for (int i = 0; i < WARMUP_COUNT; ++i)
		{
			mResultSink ^= baselineAction();
			mResultSink ^= builtInAction();
		}
		double[] baselineSamples = new double[SAMPLE_COUNT];
		double[] builtInSamples = new double[SAMPLE_COUNT];
		double[] pairedRatios = new double[SAMPLE_COUNT];
		for (int i = 0; i < SAMPLE_COUNT; ++i)
		{
			if ((i & 1) == 0)
			{
				baselineSamples[i] = measure(baselineAction);
				builtInSamples[i] = measure(builtInAction);
			}
			else
			{
				builtInSamples[i] = measure(builtInAction);
				baselineSamples[i] = measure(baselineAction);
			}
			pairedRatios[i] = builtInSamples[i] / baselineSamples[i];
		}
		Array.Sort(baselineSamples);
		Array.Sort(builtInSamples);
		Array.Sort(pairedRatios);
		baselineMedian = baselineSamples[SAMPLE_COUNT >> 1];
		builtInMedian = builtInSamples[SAMPLE_COUNT >> 1];
		pairedMedianRatio = pairedRatios[SAMPLE_COUNT >> 1];
	}
	private static double measure(Func<long> action)
	{
		long start = Stopwatch.GetTimestamp();
		long value = action();
		long end = Stopwatch.GetTimestamp();
		mResultSink ^= value;
		return (end - start) * 1000.0 / Stopwatch.Frequency;
	}
	private static unsafe void logPointerLayout(string name, void* baselinePointer, void* builtInPointer)
	{
		ulong baseline = (ulong)baselinePointer;
		ulong builtIn = (ulong)builtInPointer;
		Debug.Log("Burst Pointer:" + name + ",BaselineMod16:" + (baseline & 15UL) + ",BuiltInMod16:" + (builtIn & 15UL) + ",BaselineMod64:" + (baseline & 63UL) + ",BuiltInMod64:" + (builtIn & 63UL));
	}
	private static void logCase(string name, double baselineMedian, double builtInMedian, double pairedMedianRatio)
	{
		double medianRatio = builtInMedian / baselineMedian;
		bool releasePass = pairedMedianRatio <= 1.0;
		Debug.Log("\n================ " + name + " ================\n" +
			"手写[ECS]      Median:" + baselineMedian.ToString("F3").PadLeft(9) + " ms\n" +
			"BuiltIn       Median:" + builtInMedian.ToString("F3").PadLeft(9) + " ms\n" +
			"MedianRatio          : " + medianRatio.ToString("F3") + "x\n" +
			"PairedMedianRatio    : " + pairedMedianRatio.ToString("F3") + "x\n" +
			"Release Gate(<=1.00x): " + (releasePass ? "PASS" : "FAIL") + "\n" +
			"==================================================");
		if (!releasePass)
		{
			Debug.LogError("BuiltIn Burst性能回归:" + name + ",PairedRatio:" + pairedMedianRatio.ToString("F3") + "x");
		}
	}
}
