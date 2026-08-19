using System;
using System.Collections.Generic;
using System.Diagnostics;
using EasyECS;
using Unity.Profiling;
using UnityEngine;
using Debug = UnityEngine.Debug;

[ECS]
public struct ManagedRoleDataStructuralBenchmarkData
{
	public int mHP;
	public string mName;
	public object mPayload;
	[NotECS] public int mID;
	[NotECS] public string mPath;
}

public sealed class RoleDataListStructuralBenchmark : MonoBehaviour
{
	private const int BASE_ENTITY_COUNT = 20000;
	private const int OPERATION_COUNT = 256;
	private const int SAMPLE_COUNT = 9;
	private const int WARMUP_COUNT = 2;
	private const int RESIZE_SAMPLE_COUNT = 9;
	private const int RESIZE_GC_CAPACITY = 65536;
	private const int GC_OPERATION_COUNT = 512;
	private const int GC_LOOKUP_COUNT = 100000;
	private const int GC_RECORDER_CAPACITY = 32768;
	private const int GC_SELF_CHECK_COUNT = 128;
	private const double MAX_ACCEPTABLE_SLOWDOWN = 1.05;
	private const double TINY_OPERATION_US = 0.05;
	private static double mResultSink;
	private List<RoleData> mList;
	private RoleData_ECSList mECSList;
	private List<ManagedRoleDataStructuralBenchmarkData> mManagedList;
	private ManagedRoleDataStructuralBenchmarkData_ECSList mManagedECSList;
	private readonly object mSharedPayload = new object();
	private static readonly int[] mResizeCapacities = { 1024, 8192, 32768, 49152, 57344, 61440, 65536, 69632, 73728, 81920, 98304, 131072, 262144 };
	private static readonly int[] mHybridResizeCapacities = { 1024, 8192, 32768, 49152, 65536, 81920, 98304, 131072 };
	private struct BenchmarkResult
	{
		public double mMedian;
		public double mMin;
		public double mMax;
		public double mUsPerOperation;
	}
	private struct ResizeResult
	{
		public double mMedianUs;
		public double mMinUs;
		public double mMaxUs;
		public int mOldCapacity;
		public int mNewCapacity;
	}
	private struct GCAllocResult
	{
		public bool mValid;
		public bool mWrappedAround;
		public long mAllocEvents;
		public double mEventsPerOperation;
	}
	private void Awake()
	{
#if UNITY_EDITOR
		Debug.Log("Unity Editor环境跳过RoleDataListStructuralBenchmark,请使用Player测试Insert/RemoveAt性能");
#else
		runBenchmark();
#endif
	}
	private void runBenchmark()
	{
		Debug.Log("================ ECSList Structural Benchmark Start ================");
		Debug.Log("RoleData Backend:" + RoleData_ECSList.BackendName + ",Reason:" + RoleData_ECSList.BackendReason);
		Debug.Log("Managed Backend:" + ManagedRoleDataStructuralBenchmarkData_ECSList.BackendName + ",Reason:" + ManagedRoleDataStructuralBenchmarkData_ECSList.BackendReason);
		Debug.Log("BaseEntityCount:" + BASE_ENTITY_COUNT);
		Debug.Log("OperationCount:" + OPERATION_COUNT);
		Debug.Log("SampleCount:" + SAMPLE_COUNT);
		Debug.Log("WarmupCount:" + WARMUP_COUNT);
		Debug.Log("UnsafeStructuralMove:InsertNative=Buffer.MemoryCopy,RemoveAtNative=ForwardLoop,Managed=Array.Copy");
		runInsertBenchmark("Insert头部", 0);
		runInsertBenchmark("Insert中间", 1);
		runInsertBenchmark("Insert尾部", 2);
		runRemoveAtBenchmark("RemoveAt头部", 0);
		runRemoveAtBenchmark("RemoveAt中间", 1);
		runRemoveAtBenchmark("RemoveAt尾部", 2);
		runSwapBackBenchmark();
		runManagedHybridBenchmark();
		runCapacityGrowthBenchmark();
		runProfilerGCRegression();
		Debug.Log("ResultSink:" + mResultSink);
		Debug.Log("================ ECSList Structural Benchmark End =================");
	}
	private void runInsertBenchmark(string title, int positionMode)
	{
		BenchmarkResult standard = measure(
			() => setupList(BASE_ENTITY_COUNT, OPERATION_COUNT),
			() => runListInsert(positionMode),
			cleanupList,
			OPERATION_COUNT);
		BenchmarkResult ecs = measure(
			() => setupECSList(BASE_ENTITY_COUNT, OPERATION_COUNT),
			() => runECSInsert(positionMode),
			cleanupECSList,
			OPERATION_COUNT);
		printCompare(title, standard, ecs);
	}
	private void runRemoveAtBenchmark(string title, int positionMode)
	{
		BenchmarkResult standard = measure(
			() => setupList(BASE_ENTITY_COUNT + OPERATION_COUNT, 0),
			() => runListRemoveAt(positionMode),
			cleanupList,
			OPERATION_COUNT);
		BenchmarkResult ecs = measure(
			() => setupECSList(BASE_ENTITY_COUNT + OPERATION_COUNT, 0),
			() => runECSRemoveAt(positionMode),
			cleanupECSList,
			OPERATION_COUNT);
		printCompare(title, standard, ecs);
	}
	private void runSwapBackBenchmark()
	{
		BenchmarkResult removeAt = measure(
			() => setupECSList(BASE_ENTITY_COUNT + OPERATION_COUNT, 0),
			runECSRemoveAtMiddle,
			cleanupECSList,
			OPERATION_COUNT);
		BenchmarkResult swapBack = measure(
			() => setupECSList(BASE_ENTITY_COUNT + OPERATION_COUNT, 0),
			runECSRemoveAtSwapBackMiddle,
			cleanupECSList,
			OPERATION_COUNT);
		Debug.Log(
			"\n================ RemoveAt中间 vs RemoveAtSwapBack ================\n" +
			format("ECS RemoveAt", removeAt) + "\n" +
			format("ECS RemoveAtSwapBack", swapBack) + "\n" +
			"--------------------------------------------------\n" +
			"RemoveAt / SwapBack : " + ratio(removeAt.mMedian, swapBack.mMedian) + "x\n" +
			"==================================================");
	}
	private void runManagedHybridBenchmark()
	{
		BenchmarkResult listInsert = measure(
			() => setupManagedList(BASE_ENTITY_COUNT, OPERATION_COUNT),
			runManagedListInsertMiddle,
			cleanupManagedList,
			OPERATION_COUNT);
		BenchmarkResult ecsInsert = measure(
			() => setupManagedECSList(BASE_ENTITY_COUNT, OPERATION_COUNT),
			runManagedECSInsertMiddle,
			cleanupManagedECSList,
			OPERATION_COUNT);
		printCompare("Managed Hybrid Insert中间", listInsert, ecsInsert);
		BenchmarkResult listRemove = measure(
			() => setupManagedList(BASE_ENTITY_COUNT + OPERATION_COUNT, 0),
			runManagedListRemoveAtMiddle,
			cleanupManagedList,
			OPERATION_COUNT);
		BenchmarkResult ecsRemove = measure(
			() => setupManagedECSList(BASE_ENTITY_COUNT + OPERATION_COUNT, 0),
			runManagedECSRemoveAtMiddle,
			cleanupManagedECSList,
			OPERATION_COUNT);
		printCompare("Managed Hybrid RemoveAt中间", listRemove, ecsRemove);
	}
	private void runCapacityGrowthBenchmark()
	{
		Debug.Log("\n================ Capacity/Resize Benchmark ================");
		Debug.Log("ResizeSampleCount:" + RESIZE_SAMPLE_COUNT);
		for (int i = 0; i < mResizeCapacities.Length; ++i)
		{
			int capacity = mResizeCapacities[i];
			ResizeResult list = measureListForcedResize(capacity);
			ResizeResult ecs = measureECSForcedResize(capacity);
			Debug.Log(
				"\nCapacity:" + capacity + "\n" +
				formatResize("List<T> forced resize", list) + "\n" +
				formatResize("ECSList forced resize", ecs) + "\n" +
				"ECS/List resize time : " + ratio(ecs.mMedianUs, list.mMedianUs) + "x");
		}
		for (int i = 0; i < mHybridResizeCapacities.Length; ++i)
		{
			int capacity = mHybridResizeCapacities[i];
			ResizeResult list = measureManagedListForcedResize(capacity);
			ResizeResult ecs = measureManagedECSForcedResize(capacity);
			Debug.Log(
				"\nHybrid Capacity:" + capacity + "\n" +
				formatResize("Managed List resize", list) + "\n" +
				formatResize("Hybrid ECSList resize", ecs) + "\n" +
				"ECS/List resize time : " + ratio(ecs.mMedianUs, list.mMedianUs) + "x");
		}
		runResizeGCAllocMarkerBenchmark();
		Debug.Log("================ Capacity/Resize Benchmark End ================");
	}
	private string formatResize(string name, ResizeResult result)
	{
		return name.PadRight(26) +
			" Median:" + result.mMedianUs.ToString("0.000").PadLeft(10) + " us" +
			" | Min:" + result.mMinUs.ToString("0.000").PadLeft(9) +
			" | Max:" + result.mMaxUs.ToString("0.000").PadLeft(9) +
			" | Capacity:" + result.mOldCapacity + "->" + result.mNewCapacity;
	}
	private ResizeResult measureListForcedResize(int capacity)
	{
		double[] samples = new double[RESIZE_SAMPLE_COUNT];
		double min = double.MaxValue;
		double max = double.MinValue;
		int newCapacity = 0;
		for (int sample = 0; sample < RESIZE_SAMPLE_COUNT; ++sample)
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			List<RoleData> list = new List<RoleData>(capacity);
			for (int i = 0; i < capacity; ++i)
			{
				list.Add(createData(i));
			}
			long start = Stopwatch.GetTimestamp();
			list.Add(createData(-1));
			long end = Stopwatch.GetTimestamp();
			double us = (end - start) * 1000000.0 / Stopwatch.Frequency;
			samples[sample] = us;
			if (us < min)
			{
				min = us;
			}
			if (us > max)
			{
				max = us;
			}
			newCapacity = list.Capacity;
			mResultSink += list[list.Count - 1].mHP;
		}
		Array.Sort(samples);
		return new ResizeResult { mMedianUs = samples[samples.Length / 2], mMinUs = min, mMaxUs = max, mOldCapacity = capacity, mNewCapacity = newCapacity };
	}
	private ResizeResult measureECSForcedResize(int capacity)
	{
		double[] samples = new double[RESIZE_SAMPLE_COUNT];
		double min = double.MaxValue;
		double max = double.MinValue;
		int newCapacity = 0;
		for (int sample = 0; sample < RESIZE_SAMPLE_COUNT; ++sample)
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			RoleData_ECSList list = new RoleData_ECSList(capacity);
			try
			{
				for (int i = 0; i < capacity; ++i)
				{
					list.Add(createData(i));
				}
				long start = Stopwatch.GetTimestamp();
				list.Add(createData(-1));
				long end = Stopwatch.GetTimestamp();
				double us = (end - start) * 1000000.0 / Stopwatch.Frequency;
				samples[sample] = us;
				if (us < min)
				{
					min = us;
				}
				if (us > max)
				{
					max = us;
				}
				newCapacity = list.Capacity;
				mResultSink += list[list.Count - 1].mHP;
			}
			finally
			{
				list.Dispose();
			}
		}
		Array.Sort(samples);
		return new ResizeResult { mMedianUs = samples[samples.Length / 2], mMinUs = min, mMaxUs = max, mOldCapacity = capacity, mNewCapacity = newCapacity };
	}
	private ResizeResult measureManagedListForcedResize(int capacity)
	{
		double[] samples = new double[RESIZE_SAMPLE_COUNT];
		double min = double.MaxValue;
		double max = double.MinValue;
		int newCapacity = 0;
		for (int sample = 0; sample < RESIZE_SAMPLE_COUNT; ++sample)
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			List<ManagedRoleDataStructuralBenchmarkData> list = new List<ManagedRoleDataStructuralBenchmarkData>(capacity);
			for (int i = 0; i < capacity; ++i)
			{
				list.Add(createManagedData(i));
			}
			long start = Stopwatch.GetTimestamp();
			list.Add(createManagedData(-1));
			long end = Stopwatch.GetTimestamp();
			double us = (end - start) * 1000000.0 / Stopwatch.Frequency;
			samples[sample] = us;
			if (us < min)
			{
				min = us;
			}
			if (us > max)
			{
				max = us;
			}
			newCapacity = list.Capacity;
			mResultSink += list[list.Count - 1].mHP;
		}
		Array.Sort(samples);
		return new ResizeResult { mMedianUs = samples[samples.Length / 2], mMinUs = min, mMaxUs = max, mOldCapacity = capacity, mNewCapacity = newCapacity };
	}
	private ResizeResult measureManagedECSForcedResize(int capacity)
	{
		double[] samples = new double[RESIZE_SAMPLE_COUNT];
		double min = double.MaxValue;
		double max = double.MinValue;
		int newCapacity = 0;
		for (int sample = 0; sample < RESIZE_SAMPLE_COUNT; ++sample)
		{
			GC.Collect();
			GC.WaitForPendingFinalizers();
			GC.Collect();
			ManagedRoleDataStructuralBenchmarkData_ECSList list = new ManagedRoleDataStructuralBenchmarkData_ECSList(capacity);
			try
			{
				for (int i = 0; i < capacity; ++i)
				{
					list.Add(createManagedData(i));
				}
				long start = Stopwatch.GetTimestamp();
				list.Add(createManagedData(-1));
				long end = Stopwatch.GetTimestamp();
				double us = (end - start) * 1000000.0 / Stopwatch.Frequency;
				samples[sample] = us;
				if (us < min)
				{
					min = us;
				}
				if (us > max)
				{
					max = us;
				}
				newCapacity = list.Capacity;
				mResultSink += list[list.Count - 1].mHP;
			}
			finally
			{
				list.Dispose();
			}
		}
		Array.Sort(samples);
		return new ResizeResult { mMedianUs = samples[samples.Length / 2], mMinUs = min, mMaxUs = max, mOldCapacity = capacity, mNewCapacity = newCapacity };
	}
	private void runResizeGCAllocMarkerBenchmark()
	{
		Debug.Log("\n================ Resize GC.Alloc Marker Benchmark ================");
		Debug.Log("Metric:Unity ProfilerRecorder -> ProfilerCategory.Internal / GC.Alloc");
		Debug.Log("CurrentThreadOnly:true,RecorderCapacity:" + GC_RECORDER_CAPACITY);
		RoleData value = createData(-1);
		List<RoleData> list = new List<RoleData>(RESIZE_GC_CAPACITY);
		for (int i = 0; i < RESIZE_GC_CAPACITY; ++i)
		{
			list.Add(createData(i));
		}
		GCAllocResult listResult = measureGCAlloc(() => list.Add(value), 1);
		printGCAlloc("List<RoleData> resize", listResult, false);
		RoleData_ECSList ecs = new RoleData_ECSList(RESIZE_GC_CAPACITY);
		try
		{
			for (int i = 0; i < RESIZE_GC_CAPACITY; ++i)
			{
				ecs.Add(createData(i));
			}
			GCAllocResult result = measureGCAlloc(() => ecs.Add(value), 1);
			printGCAlloc("ECSList<RoleData> resize", result, true);
		}
		finally
		{
			ecs.Dispose();
		}
		ManagedRoleDataStructuralBenchmarkData managedValue = createManagedData(-1);
		List<ManagedRoleDataStructuralBenchmarkData> managedList = new List<ManagedRoleDataStructuralBenchmarkData>(RESIZE_GC_CAPACITY);
		for (int i = 0; i < RESIZE_GC_CAPACITY; ++i)
		{
			managedList.Add(createManagedData(i));
		}
		GCAllocResult managedListResult = measureGCAlloc(() => managedList.Add(managedValue), 1);
		printGCAlloc("Managed List resize", managedListResult, false);
		ManagedRoleDataStructuralBenchmarkData_ECSList managedECS = new ManagedRoleDataStructuralBenchmarkData_ECSList(RESIZE_GC_CAPACITY);
		try
		{
			for (int i = 0; i < RESIZE_GC_CAPACITY; ++i)
			{
				managedECS.Add(createManagedData(i));
			}
			GCAllocResult result = measureGCAlloc(() => managedECS.Add(managedValue), 1);
			printGCAlloc("Hybrid ECSList resize", result, false);
		}
		finally
		{
			managedECS.Dispose();
		}
		Debug.Log("说明:ECS纯Unsafe Resize预期GC.Alloc=0;List/Managed/Hybrid Resize需要新托管数组,预期GC.Alloc>0。这里只统计托管分配事件数量,Native Malloc不属于GC.Alloc。");
		Debug.Log("================ Resize GC.Alloc Marker Benchmark End ================");
	}
	private void runProfilerGCRegression()
	{
		Debug.Log("\n================ Structural Profiler GC.Alloc Regression ================");
		Debug.Log("Metric:ProfilerRecorder(ProfilerCategory.Internal,\"GC.Alloc\")");
		Debug.Log("CurrentThreadOnly:true,RecorderCapacity:" + GC_RECORDER_CAPACITY);
		object[] selfCheckObjects = new object[GC_SELF_CHECK_COUNT];
		GCAllocResult selfCheck = measureGCAlloc(() =>
		{
			for (int i = 0; i < GC_SELF_CHECK_COUNT; ++i)
			{
				selfCheckObjects[i] = new byte[256 + (i & 15)];
			}
		}, GC_SELF_CHECK_COUNT);
		printGCAlloc("ProfilerRecorder SelfCheck", selfCheck, false);
		if (!selfCheck.mValid || selfCheck.mAllocEvents == 0 || selfCheck.mWrappedAround)
		{
			Debug.LogError("GC.Alloc ProfilerRecorder自检失败,后续0事件结果不可作为无GC结论。请使用Development Build并确认Profiler可用。");
		}
		mResultSink += ((byte[])selfCheckObjects[GC_SELF_CHECK_COUNT - 1]).Length;
		RoleData value = createData(-1);
		List<RoleData> listAdd = new List<RoleData>(GC_OPERATION_COUNT * 2);
		GCAllocResult listAddResult = measureGCAlloc(() =>
		{
			for (int i = 0; i < GC_OPERATION_COUNT; ++i)
			{
				listAdd.Add(value);
			}
		}, GC_OPERATION_COUNT);
		printGCAlloc("List<T> Add无Resize", listAddResult, true);
		RoleData_ECSList ecsAdd = new RoleData_ECSList(GC_OPERATION_COUNT * 2);
		try
		{
			GCAllocResult result = measureGCAlloc(() =>
			{
				for (int i = 0; i < GC_OPERATION_COUNT; ++i)
				{
					ecsAdd.Add(value);
				}
			}, GC_OPERATION_COUNT);
			printGCAlloc("ECSList Add无Resize", result, true);
		}
		finally
		{
			ecsAdd.Dispose();
		}
		RoleData_ECSList ecsInsert = new RoleData_ECSList(GC_OPERATION_COUNT * 4);
		try
		{
			for (int i = 0; i < GC_OPERATION_COUNT * 2; ++i)
			{
				ecsInsert.Add(createData(i));
			}
			GCAllocResult result = measureGCAlloc(() =>
			{
				for (int i = 0; i < GC_OPERATION_COUNT; ++i)
				{
					ecsInsert.Insert(ecsInsert.Count >> 1, value);
				}
			}, GC_OPERATION_COUNT);
			printGCAlloc("ECSList Insert无Resize", result, true);
		}
		finally
		{
			ecsInsert.Dispose();
		}
		RoleData_ECSList ecsRemove = new RoleData_ECSList(GC_OPERATION_COUNT * 4);
		try
		{
			for (int i = 0; i < GC_OPERATION_COUNT * 3; ++i)
			{
				ecsRemove.Add(createData(i));
			}
			GCAllocResult result = measureGCAlloc(() =>
			{
				for (int i = 0; i < GC_OPERATION_COUNT; ++i)
				{
					ecsRemove.RemoveAt(ecsRemove.Count >> 1);
				}
			}, GC_OPERATION_COUNT);
			printGCAlloc("ECSList RemoveAt", result, true);
		}
		finally
		{
			ecsRemove.Dispose();
		}
		RoleData_ECSList ecsSwapBack = new RoleData_ECSList(GC_OPERATION_COUNT * 4);
		try
		{
			for (int i = 0; i < GC_OPERATION_COUNT * 3; ++i)
			{
				ecsSwapBack.Add(createData(i));
			}
			GCAllocResult result = measureGCAlloc(() =>
			{
				for (int i = 0; i < GC_OPERATION_COUNT; ++i)
				{
					ecsSwapBack.RemoveAtSwapBack(ecsSwapBack.Count >> 1);
				}
			}, GC_OPERATION_COUNT);
			printGCAlloc("ECSList RemoveAtSwapBack", result, true);
		}
		finally
		{
			ecsSwapBack.Dispose();
		}
		RoleData_ECSList ecsClear = new RoleData_ECSList(4096);
		try
		{
			for (int i = 0; i < 4096; ++i)
			{
				ecsClear.Add(createData(i));
			}
			printGCAlloc("ECSList Clear", measureGCAlloc(ecsClear.Clear, 1), true);
		}
		finally
		{
			ecsClear.Dispose();
		}
		ManagedRoleDataStructuralBenchmarkData managedValue = createManagedData(-1);
		ManagedRoleDataStructuralBenchmarkData_ECSList hybrid = new ManagedRoleDataStructuralBenchmarkData_ECSList(GC_OPERATION_COUNT * 4);
		try
		{
			for (int i = 0; i < GC_OPERATION_COUNT; ++i)
			{
				hybrid.Add(createManagedData(i));
			}
			GCAllocResult addResult = measureGCAlloc(() =>
			{
				for (int i = 0; i < GC_OPERATION_COUNT; ++i)
				{
					hybrid.Add(managedValue);
				}
			}, GC_OPERATION_COUNT);
			printGCAlloc("Hybrid ECSList Add无Resize", addResult, true);
			GCAllocResult insertResult = measureGCAlloc(() =>
			{
				for (int i = 0; i < GC_OPERATION_COUNT; ++i)
				{
					hybrid.Insert(hybrid.Count >> 1, managedValue);
				}
			}, GC_OPERATION_COUNT);
			printGCAlloc("Hybrid ECSList Insert无Resize", insertResult, true);
		}
		finally
		{
			hybrid.Dispose();
		}
		RoleData_ECSDictionary<int> dictLookup = new RoleData_ECSDictionary<int>(4096);
		try
		{
			for (int i = 0; i < 2048; ++i)
			{
				dictLookup.Add(i, createData(i));
			}
			GCAllocResult result = measureGCAlloc(() =>
			{
				long sum = 0;
				for (int i = 0; i < GC_LOOKUP_COUNT; ++i)
				{
					if (dictLookup.TryGetValue(i & 2047, out RoleDataRef item))
					{
						sum += item.mHP;
					}
				}
				mResultSink += sum;
			}, GC_LOOKUP_COUNT);
			printGCAlloc("ECSDictionary TryGetValue", result, true);
		}
		finally
		{
			dictLookup.Dispose();
		}
		RoleData_ECSDictionary<int> dictAdd = new RoleData_ECSDictionary<int>(4096);
		try
		{
			for (int i = 0; i < 1024; ++i)
			{
				dictAdd.Add(i, createData(i));
			}
			GCAllocResult result = measureGCAlloc(() =>
			{
				for (int i = 0; i < GC_OPERATION_COUNT; ++i)
				{
					dictAdd.Add(100000 + i, value);
				}
			}, GC_OPERATION_COUNT);
			printGCAlloc("ECSDictionary Add预留容量", result, true);
		}
		finally
		{
			dictAdd.Dispose();
		}
		RoleData_ECSDictionary<int> dictRemove = new RoleData_ECSDictionary<int>(4096);
		try
		{
			for (int i = 0; i < 2048; ++i)
			{
				dictRemove.Add(i, createData(i));
			}
			GCAllocResult result = measureGCAlloc(() =>
			{
				for (int i = 0; i < GC_OPERATION_COUNT; ++i)
				{
					dictRemove.Remove(i);
				}
			}, GC_OPERATION_COUNT);
			printGCAlloc("ECSDictionary Remove", result, true);
		}
		finally
		{
			dictRemove.Dispose();
		}
		RoleData_ECSDictionary<int> dictClear = new RoleData_ECSDictionary<int>(4096);
		try
		{
			for (int i = 0; i < 2048; ++i)
			{
				dictClear.Add(i, createData(i));
			}
			printGCAlloc("ECSDictionary Clear", measureGCAlloc(dictClear.Clear, 1), true);
		}
		finally
		{
			dictClear.Dispose();
		}
		Dictionary<int, RoleData> standardGrowth = new Dictionary<int, RoleData>(1);
		GCAllocResult standardGrowthResult = measureGCAlloc(() =>
		{
			for (int i = 0; i < 4096; ++i)
			{
				standardGrowth.Add(i, value);
			}
		}, 4096);
		printGCAlloc("Dictionary Growth workload", standardGrowthResult, false);
		RoleData_ECSDictionary<int> ecsGrowth = new RoleData_ECSDictionary<int>(1);
		try
		{
			GCAllocResult result = measureGCAlloc(() =>
			{
				for (int i = 0; i < 4096; ++i)
				{
					ecsGrowth.Add(i, value);
				}
			}, 4096);
			printGCAlloc("ECSDictionary Growth workload", result, false);
		}
		finally
		{
			ecsGrowth.Dispose();
		}
		Debug.Log("ExpectedZero=true的Case目标是AllocEvents=0;SelfCheck/Growth/托管Resize目标是AllocEvents>0。若SelfCheck为0,整组测试视为无效。");
		Debug.Log("================ Structural Profiler GC.Alloc Regression End ================");
	}
	private GCAllocResult measureGCAlloc(Action action, int operationCount)
	{
		GC.Collect();
		GC.WaitForPendingFinalizers();
		GC.Collect();
		using (ProfilerRecorder recorder = ProfilerRecorder.StartNew(
			ProfilerCategory.Internal,
			"GC.Alloc",
			GC_RECORDER_CAPACITY,
			ProfilerRecorderOptions.CollectOnlyOnCurrentThread))
		{
			bool valid = recorder.Valid;
			action();
			recorder.Stop();
			long events = recorder.Count;
			return new GCAllocResult
			{
				mValid = valid,
				mWrappedAround = recorder.WrappedAround,
				mAllocEvents = events,
				mEventsPerOperation = operationCount > 0 ? (double)events / operationCount : 0.0,
			};
		}
	}
	private string formatGCAlloc(string name, GCAllocResult result, bool expectedZero)
	{
		string gate;
		if (!result.mValid)
		{
			gate = "INVALID";
		}
		else if (result.mWrappedAround)
		{
			gate = "OVERFLOW";
		}
		else if (expectedZero)
		{
			gate = result.mAllocEvents == 0 ? "PASS" : "FAIL";
		}
		else
		{
			gate = result.mAllocEvents > 0 ? "PASS" : "FAIL";
		}
		return name.PadRight(36) +
			" AllocEvents:" + result.mAllocEvents.ToString().PadLeft(7) +
			" | " + result.mEventsPerOperation.ToString("0.000000").PadLeft(10) + " event/op" +
			" | Expected:" + (expectedZero ? "ZERO" : ">0").PadLeft(4) +
			" | Gate:" + gate;
	}
	private void printGCAlloc(string name, GCAllocResult result, bool expectedZero)
	{
		Debug.Log(formatGCAlloc(name, result, expectedZero));
	}
	private void setupList(int count, int extraCapacity)
	{
		mList = new List<RoleData>(count + extraCapacity + 4);
		for (int i = 0; i < count; ++i)
		{
			mList.Add(createData(i));
		}
	}
	private void cleanupList()
	{
		mList = null;
	}
	private void setupECSList(int count, int extraCapacity)
	{
		mECSList = new RoleData_ECSList(count + extraCapacity + 4);
		for (int i = 0; i < count; ++i)
		{
			mECSList.Add(createData(i));
		}
	}
	private void cleanupECSList()
	{
		if (mECSList != null)
		{
			mECSList.Dispose();
			mECSList = null;
		}
	}
	private void setupManagedList(int count, int extraCapacity)
	{
		mManagedList = new List<ManagedRoleDataStructuralBenchmarkData>(count + extraCapacity + 4);
		for (int i = 0; i < count; ++i)
		{
			mManagedList.Add(createManagedData(i));
		}
	}
	private void cleanupManagedList()
	{
		mManagedList = null;
	}
	private void setupManagedECSList(int count, int extraCapacity)
	{
		mManagedECSList = new ManagedRoleDataStructuralBenchmarkData_ECSList(count + extraCapacity + 4);
		for (int i = 0; i < count; ++i)
		{
			mManagedECSList.Add(createManagedData(i));
		}
	}
	private void cleanupManagedECSList()
	{
		if (mManagedECSList != null)
		{
			mManagedECSList.Dispose();
			mManagedECSList = null;
		}
	}
	private void runListInsert(int positionMode)
	{
		RoleData value = createData(-1);
		for (int i = 0; i < OPERATION_COUNT; ++i)
		{
			mList.Insert(getInsertIndex(mList.Count, positionMode), value);
		}
		mResultSink += mList[mList.Count - 1].mHP + mList.Count;
	}
	private void runECSInsert(int positionMode)
	{
		RoleData value = createData(-1);
		for (int i = 0; i < OPERATION_COUNT; ++i)
		{
			mECSList.Insert(getInsertIndex(mECSList.Count, positionMode), value);
		}
		mResultSink += mECSList[mECSList.Count - 1].mHP + mECSList.Count;
	}
	private void runListRemoveAt(int positionMode)
	{
		for (int i = 0; i < OPERATION_COUNT; ++i)
		{
			mList.RemoveAt(getRemoveIndex(mList.Count, positionMode));
		}
		mResultSink += mList[mList.Count - 1].mHP + mList.Count;
	}
	private void runECSRemoveAt(int positionMode)
	{
		for (int i = 0; i < OPERATION_COUNT; ++i)
		{
			mECSList.RemoveAt(getRemoveIndex(mECSList.Count, positionMode));
		}
		mResultSink += mECSList[mECSList.Count - 1].mHP + mECSList.Count;
	}
	private void runECSRemoveAtMiddle()
	{
		for (int i = 0; i < OPERATION_COUNT; ++i)
		{
			mECSList.RemoveAt(mECSList.Count >> 1);
		}
		mResultSink += mECSList[mECSList.Count - 1].mHP;
	}
	private void runECSRemoveAtSwapBackMiddle()
	{
		for (int i = 0; i < OPERATION_COUNT; ++i)
		{
			mECSList.RemoveAtSwapBack(mECSList.Count >> 1);
		}
		mResultSink += mECSList[mECSList.Count - 1].mHP;
	}
	private void runManagedListInsertMiddle()
	{
		ManagedRoleDataStructuralBenchmarkData value = createManagedData(-1);
		for (int i = 0; i < OPERATION_COUNT; ++i)
		{
			mManagedList.Insert(mManagedList.Count >> 1, value);
		}
		mResultSink += mManagedList.Count + mManagedList[mManagedList.Count - 1].mHP;
	}
	private void runManagedECSInsertMiddle()
	{
		ManagedRoleDataStructuralBenchmarkData value = createManagedData(-1);
		for (int i = 0; i < OPERATION_COUNT; ++i)
		{
			mManagedECSList.Insert(mManagedECSList.Count >> 1, value);
		}
		mResultSink += mManagedECSList.Count + mManagedECSList[mManagedECSList.Count - 1].mHP;
	}
	private void runManagedListRemoveAtMiddle()
	{
		for (int i = 0; i < OPERATION_COUNT; ++i)
		{
			mManagedList.RemoveAt(mManagedList.Count >> 1);
		}
		mResultSink += mManagedList.Count + mManagedList[mManagedList.Count - 1].mHP;
	}
	private void runManagedECSRemoveAtMiddle()
	{
		for (int i = 0; i < OPERATION_COUNT; ++i)
		{
			mManagedECSList.RemoveAt(mManagedECSList.Count >> 1);
		}
		mResultSink += mManagedECSList.Count + mManagedECSList[mManagedECSList.Count - 1].mHP;
	}
	private static int getInsertIndex(int count, int positionMode)
	{
		if (positionMode == 0)
		{
			return 0;
		}
		if (positionMode == 1)
		{
			return count >> 1;
		}
		return count;
	}
	private static int getRemoveIndex(int count, int positionMode)
	{
		if (positionMode == 0)
		{
			return 0;
		}
		if (positionMode == 1)
		{
			return count >> 1;
		}
		return count - 1;
	}
	private RoleData createData(int id)
	{
		return new RoleData
		{
			mHP = 100 + id,
			mSpeed = id * 0.1f,
			mPositionX = id * 2.0f,
			mPositionY = id * 3.0f,
			mID = id,
			mCamp = id & 3,
		};
	}
	private ManagedRoleDataStructuralBenchmarkData createManagedData(int id)
	{
		return new ManagedRoleDataStructuralBenchmarkData
		{
			mHP = 100 + id,
			mName = "SharedName",
			mPayload = mSharedPayload,
			mID = id,
			mPath = "Shared/Path",
		};
	}
	private BenchmarkResult measure(Action setup, Action action, Action cleanup, int operationCount)
	{
		for (int i = 0; i < WARMUP_COUNT; ++i)
		{
			setup();
			try
			{
				action();
			}
			finally
			{
				cleanup();
			}
		}
		double[] samples = new double[SAMPLE_COUNT];
		Stopwatch stopwatch = new Stopwatch();
		for (int i = 0; i < SAMPLE_COUNT; ++i)
		{
			setup();
			try
			{
				stopwatch.Restart();
				action();
				stopwatch.Stop();
				samples[i] = stopwatch.Elapsed.TotalMilliseconds;
			}
			finally
			{
				cleanup();
			}
		}
		Array.Sort(samples);
		double median = samples[SAMPLE_COUNT / 2];
		return new BenchmarkResult
		{
			mMedian = median,
			mMin = samples[0],
			mMax = samples[SAMPLE_COUNT - 1],
			mUsPerOperation = median * 1000.0 / operationCount,
		};
	}
	private void printCompare(string title, BenchmarkResult standard, BenchmarkResult ecs)
	{
		bool tinyOperation = standard.mUsPerOperation < TINY_OPERATION_US;
		double slowdown = standard.mMedian > 0.0 ? ecs.mMedian / standard.mMedian : 0.0;
		string gate;
		if (tinyOperation)
		{
			gate = "SKIP(TinyOperation)";
		}
		else
		{
			gate = slowdown <= MAX_ACCEPTABLE_SLOWDOWN ? "PASS" : "FAIL";
		}
		Debug.Log(
			"\n================ " + title + " ================\n" +
			format("List<T>", standard) + "\n" +
			format("ECSList", ecs) + "\n" +
			"--------------------------------------------------\n" +
			"List / ECS : " + ratio(standard.mMedian, ecs.mMedian) + "x\n" +
			"ECS/List    : " + slowdown.ToString("0.000") + "x\n" +
			"5% Gate     : " + gate + "\n" +
			"==================================================");
	}
	private static string format(string name, BenchmarkResult result)
	{
		return name.PadRight(28) +
			"Median:" + result.mMedian.ToString("0.000").PadLeft(9) + " ms | " +
			"Min:" + result.mMin.ToString("0.000").PadLeft(8) + " | " +
			"Max:" + result.mMax.ToString("0.000").PadLeft(8) + " | " +
			result.mUsPerOperation.ToString("0.000").PadLeft(9) + " us/op";
	}
	private static string ratio(double a, double b)
	{
		if (b <= 0.0)
		{
			return "N/A";
		}
		return (a / b).ToString("0.00");
	}
}
