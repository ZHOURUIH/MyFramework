using System;
using System.Collections.Generic;
using System.Diagnostics;
using Unity.Profiling;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class RoleDataDictionaryEnumeratorBenchmark : MonoBehaviour
{
	private readonly struct GenericArrayEnumerable<T>
	{
		private readonly T[] mArray;
		public GenericArrayEnumerable(T[] array)
		{
			mArray = array;
		}
		public GenericArrayEnumerator<T> GetEnumerator()
		{
			return new GenericArrayEnumerator<T>(mArray);
		}
	}
	private struct GenericArrayEnumerator<T>
	{
		private readonly T[] mArray;
		private readonly int mCount;
		private int mIndex;
		public GenericArrayEnumerator(T[] array)
		{
			mArray = array;
			mCount = array.Length;
			mIndex = -1;
		}
		public T Current
		{
			get
			{
				return mArray[mIndex];
			}
		}
		public bool MoveNext()
		{
			int nextIndex = mIndex + 1;
			if ((uint)nextIndex < (uint)mCount)
			{
				mIndex = nextIndex;
				return true;
			}
			mIndex = mCount;
			return false;
		}
	}
	private const int ENTITY_COUNT = 500000;
	private const int SAMPLE_COUNT = 15;
	private const int WARMUP_COUNT = 3;
	private const int GC_REPEAT_COUNT = 16;
	private const int GC_WARMUP_COUNT = 2;
	private const int GC_RECORDER_CAPACITY = 32768;
	private const int GC_SELF_CHECK_COUNT = 64;
	private static double mResultSink;
	private int[] mKeys;
	private RoleData[] mValues;
	private Dictionary<int, RoleData> mDictionary;
	private RoleDataECSDictionary<int> mECS;
	private struct BenchmarkResult
	{
		public double mMedian;
		public double mMin;
		public double mMax;
		public double mNsPerOp;
	}
	private struct GCAllocResult
	{
		public bool mValid;
		public bool mWrappedAround;
		public long mAllocEvents;
		public double mEventsPerRun;
		public double mEventsPerOp;
	}
	private void Awake()
	{
#if UNITY_EDITOR
		return;
#else
		runBenchmark();
#endif
	}
	private void runBenchmark()
	{
		initializeData();
		try
		{
			Debug.Log("================ ECSDictionary Enumerator Benchmark Start ================");
			Debug.Log("ECS Backend:" + RoleDataECSList.BackendName);
			Debug.Log("Backend Reason:" + RoleDataECSList.BackendReason);
			Debug.Log("EntityCount:" + ENTITY_COUNT);
			Debug.Log("SampleCount:" + SAMPLE_COUNT);
			Debug.Log("WarmupCount:" + WARMUP_COUNT);
			runKeyBenchmark();
			runValueReadBenchmark();
			runValueWriteBenchmark();
			runKeyValueBenchmark();
			runGCAllocMarkerBenchmark();
			Debug.Log("ResultSink:" + mResultSink);
			Debug.Log("================ ECSDictionary Enumerator Benchmark End ================");
		}
		finally
		{
			mECS.Dispose();
			mECS = null;
		}
	}
	private void initializeData()
	{
		mKeys = new int[ENTITY_COUNT];
		mValues = new RoleData[ENTITY_COUNT];
		mDictionary = new Dictionary<int, RoleData>(ENTITY_COUNT);
		mECS = new RoleDataECSDictionary<int>(ENTITY_COUNT);
		for (int i = 0; i < ENTITY_COUNT; ++i)
		{
			int key = i + 1000000;
			RoleData value = createData(i);
			mKeys[i] = key;
			mValues[i] = value;
			mDictionary.Add(key, value);
			mECS.Add(key, value);
		}
	}
	private void runKeyBenchmark()
	{
		Debug.Log("ECS KeyEnumerationStrategy:" + RoleDataECSDictionary<int>.KeyEnumerationStrategy);
		BenchmarkResult arrayFor = measure(runKeyArrayFor, ENTITY_COUNT);
		BenchmarkResult arrayForeach = measure(runKeyArrayForeach, ENTITY_COUNT);
		BenchmarkResult readOnlySpanForeach = measure(runKeyReadOnlySpanForeach, ENTITY_COUNT);
		BenchmarkResult readOnlySpanManual = measure(runKeyReadOnlySpanManual, ENTITY_COUNT);
		BenchmarkResult genericForeach = measure(runKeyGenericArrayForeach, ENTITY_COUNT);
		BenchmarkResult genericManual = measure(runKeyGenericArrayManual, ENTITY_COUNT);
		BenchmarkResult genericMoveNextOnly = measure(runKeyGenericArrayMoveNextOnly, ENTITY_COUNT);
		BenchmarkResult ecsFor = measure(runKeyECSFor, ENTITY_COUNT);
		BenchmarkResult ecsForeach = measure(runKeyECSForeach, ENTITY_COUNT);
		BenchmarkResult ecsKeys = measure(runKeyECSKeysForeach, ENTITY_COUNT);
		BenchmarkResult ecsKeysManual = measure(runKeyECSKeysManual, ENTITY_COUNT);
		BenchmarkResult ecsKeysMoveNextOnly = measure(runKeyECSKeysMoveNextOnly, ENTITY_COUNT);
		BenchmarkResult dictionaryForeach = measure(runKeyDictionaryForeach, ENTITY_COUNT);
		BenchmarkResult dictionaryKeys = measure(runKeyDictionaryKeysForeach, ENTITY_COUNT);
		BenchmarkResult dictionaryKeysManual = measure(runKeyDictionaryKeysManual, ENTITY_COUNT);
		Debug.Log(
			"\n================ Enumerator:仅读取Key ================\n" +
			format("int[] for", arrayFor) + "\n" +
			format("int[] foreach", arrayForeach) + "\n" +
			format("ReadOnlySpan<int> foreach", readOnlySpanForeach) + "\n" +
			format("ReadOnlySpan<int> manual", readOnlySpanManual) + "\n" +
			format("GenericArray<T> foreach", genericForeach) + "\n" +
			format("GenericArray<T> manual", genericManual) + "\n" +
			format("GenericArray<T> MoveNextOnly", genericMoveNextOnly) + "\n" +
			format("ECS for + getKeyAt", ecsFor) + "\n" +
			format("ECS foreach dict + item.Key", ecsForeach) + "\n" +
			format("ECS foreach dict.Keys", ecsKeys) + "\n" +
			format("ECS Keys手动Enumerator", ecsKeysManual) + "\n" +
			format("ECS Keys MoveNextOnly", ecsKeysMoveNextOnly) + "\n" +
			format("Dictionary foreach", dictionaryForeach) + "\n" +
			format("Dictionary foreach Keys", dictionaryKeys) + "\n" +
			format("Dictionary Keys手动Enumerator", dictionaryKeysManual) +
			"\n--------------------------------------------------\n" +
			"ECS Keys / ECS for              : " + ratio(ecsKeys.mMedian, ecsFor.mMedian) + "\n" +
			"ECS Keys / ReadOnlySpan         : " + ratio(ecsKeys.mMedian, readOnlySpanForeach.mMedian) + "\n" +
			"Generic Current / MoveNextOnly  : " + ratio(genericManual.mMedian, genericMoveNextOnly.mMedian) + "\n" +
			"ECS Keys Current / MoveNextOnly : " + ratio(ecsKeysManual.mMedian, ecsKeysMoveNextOnly.mMedian) + "\n" +
			"ECS foreach / ECS Keys          : " + ratio(ecsForeach.mMedian, ecsKeys.mMedian) + "\n" +
			"Dictionary foreach / ECS foreach: " + ratio(dictionaryForeach.mMedian, ecsForeach.mMedian) + "\n" +
			"ECS Keys foreach/manual         : " + ratio(ecsKeys.mMedian, ecsKeysManual.mMedian) + "\n" +
			"==================================================");
	}
	private void runValueReadBenchmark()
	{
		BenchmarkResult arrayFor = measure(runValueArrayFor, ENTITY_COUNT);
		BenchmarkResult arrayForeach = measure(runValueArrayForeach, ENTITY_COUNT);
		BenchmarkResult ecsFor = measure(runValueECSFor, ENTITY_COUNT);
		BenchmarkResult ecsForeach = measure(runValueECSForeachRead, ENTITY_COUNT);
		BenchmarkResult ecsValues = measure(runValueECSValuesRead, ENTITY_COUNT);
		BenchmarkResult ecsValuesManual = measure(runValueECSValuesManualRead, ENTITY_COUNT);
		BenchmarkResult dictionaryForeach = measure(runValueDictionaryForeach, ENTITY_COUNT);
		BenchmarkResult dictionaryValues = measure(runValueDictionaryValuesForeach, ENTITY_COUNT);
		BenchmarkResult dictionaryValuesManual = measure(runValueDictionaryValuesManual, ENTITY_COUNT);
		Debug.Log("\n================ Enumerator:仅读取Value.mHP ================\n" + format("RoleData[] for", arrayFor) + "\n" + format("RoleData[] foreach", arrayForeach) + "\n" + format("ECS for + getValueAt", ecsFor) + "\n" + format("ECS foreach dict + Value", ecsForeach) + "\n" + format("ECS foreach dict.Values", ecsValues) + "\n" + format("ECS Values手动Enumerator", ecsValuesManual) + "\n" + format("Dictionary foreach", dictionaryForeach) + "\n" + format("Dictionary foreach Values", dictionaryValues) + "\n" + format("Dictionary Values手动Enumerator", dictionaryValuesManual) + "\n--------------------------------------------------\nECS foreach / ECS Values          : " + ratio(ecsForeach.mMedian, ecsValues.mMedian) + "\nECS Values / ECS for              : " + ratio(ecsValues.mMedian, ecsFor.mMedian) + "\nDictionary foreach / ECS foreach  : " + ratio(dictionaryForeach.mMedian, ecsForeach.mMedian) + "\nECS Values foreach/manual         : " + ratio(ecsValues.mMedian, ecsValuesManual.mMedian) + "\n==================================================");
	}
	private void runValueWriteBenchmark()
	{
		BenchmarkResult ecsFor = measure(runValueECSForWrite, ENTITY_COUNT);
		BenchmarkResult ecsForeach = measure(runValueECSForeachWrite, ENTITY_COUNT);
		BenchmarkResult ecsValues = measure(runValueECSValuesWrite, ENTITY_COUNT);
		BenchmarkResult ecsValuesManual = measure(runValueECSValuesManualWrite, ENTITY_COUNT);
		BenchmarkResult direct = measure(runValueECSDirectWrite, ENTITY_COUNT);
		Debug.Log("\n================ Enumerator:修改Value.mHP ================\n" + format("ECS for + getValueAt", ecsFor) + "\n" + format("ECS foreach dict + Value", ecsForeach) + "\n" + format("ECS foreach dict.Values", ecsValues) + "\n" + format("ECS Values手动Enumerator", ecsValuesManual) + "\n" + format("ECS Direct Column", direct) + "\n--------------------------------------------------\nECS foreach / ECS Values : " + ratio(ecsForeach.mMedian, ecsValues.mMedian) + "\nECS Values / ECS for     : " + ratio(ecsValues.mMedian, ecsFor.mMedian) + "\nECS Values / Direct      : " + ratio(ecsValues.mMedian, direct.mMedian) + "\nECS Values foreach/manual: " + ratio(ecsValues.mMedian, ecsValuesManual.mMedian) + "\n==================================================");
	}
	private void runKeyValueBenchmark()
	{
		BenchmarkResult ecsFor = measure(runKeyValueECSFor, ENTITY_COUNT);
		BenchmarkResult ecsForeach = measure(runKeyValueECSForeach, ENTITY_COUNT);
		BenchmarkResult ecsManual = measure(runKeyValueECSManual, ENTITY_COUNT);
		BenchmarkResult dictionaryForeach = measure(runKeyValueDictionaryForeach, ENTITY_COUNT);
		BenchmarkResult dictionaryManual = measure(runKeyValueDictionaryManual, ENTITY_COUNT);
		Debug.Log("\n================ Enumerator:同时读取Key+Value ================\n" + format("ECS for getKeyAt+getValueAt", ecsFor) + "\n" + format("ECS foreach dict", ecsForeach) + "\n" + format("ECS 手动Enumerator", ecsManual) + "\n" + format("Dictionary foreach", dictionaryForeach) + "\n" + format("Dictionary 手动Enumerator", dictionaryManual) + "\n--------------------------------------------------\nECS foreach / ECS for     : " + ratio(ecsForeach.mMedian, ecsFor.mMedian) + "\nDictionary / ECS foreach  : " + ratio(dictionaryForeach.mMedian, ecsForeach.mMedian) + "\nECS foreach/manual         : " + ratio(ecsForeach.mMedian, ecsManual.mMedian) + "\nDictionary foreach/manual  : " + ratio(dictionaryForeach.mMedian, dictionaryManual.mMedian) + "\n==================================================");
	}
	private void runGCAllocMarkerBenchmark()
	{
		Debug.Log("\n================ Enumerator Profiler GC.Alloc Regression ================");
		Debug.Log("Metric:ProfilerRecorder(ProfilerCategory.Internal,\"GC.Alloc\")");
		Debug.Log("RepeatCount:" + GC_REPEAT_COUNT + ",CurrentThreadOnly:true,RecorderCapacity:" + GC_RECORDER_CAPACITY);
		object[] selfCheckObjects = new object[GC_SELF_CHECK_COUNT];
		GCAllocResult selfCheck = measureGCAlloc(() =>
		{
			for (int i = 0; i < GC_SELF_CHECK_COUNT; ++i)
			{
				selfCheckObjects[i] = new byte[128 + (i & 7)];
			}
		}, GC_SELF_CHECK_COUNT, 1);
		printGCAlloc("ProfilerRecorder SelfCheck", selfCheck, false);
		if (!selfCheck.mValid || selfCheck.mAllocEvents == 0 || selfCheck.mWrappedAround)
		{
			Debug.LogError("GC.Alloc ProfilerRecorder自检失败,后续0事件结果不可作为无GC结论。");
		}
		mResultSink += ((byte[])selfCheckObjects[GC_SELF_CHECK_COUNT - 1]).Length;
		printGCAlloc("int[] foreach", measureGCAlloc(runKeyArrayForeach, ENTITY_COUNT, GC_REPEAT_COUNT), true);
		printGCAlloc("ReadOnlySpan<int> foreach", measureGCAlloc(runKeyReadOnlySpanForeach, ENTITY_COUNT, GC_REPEAT_COUNT), true);
		printGCAlloc("GenericArray<T> foreach", measureGCAlloc(runKeyGenericArrayForeach, ENTITY_COUNT, GC_REPEAT_COUNT), true);
		printGCAlloc("ECS foreach dict + item.Key", measureGCAlloc(runKeyECSForeach, ENTITY_COUNT, GC_REPEAT_COUNT), true);
		printGCAlloc("ECS foreach dict.Keys", measureGCAlloc(runKeyECSKeysForeach, ENTITY_COUNT, GC_REPEAT_COUNT), true);
		printGCAlloc("Dictionary foreach Keys", measureGCAlloc(runKeyDictionaryKeysForeach, ENTITY_COUNT, GC_REPEAT_COUNT), true);
		printGCAlloc("RoleData[] foreach", measureGCAlloc(runValueArrayForeach, ENTITY_COUNT, GC_REPEAT_COUNT), true);
		printGCAlloc("ECS foreach dict + Value", measureGCAlloc(runValueECSForeachRead, ENTITY_COUNT, GC_REPEAT_COUNT), true);
		printGCAlloc("ECS foreach dict.Values", measureGCAlloc(runValueECSValuesRead, ENTITY_COUNT, GC_REPEAT_COUNT), true);
		printGCAlloc("ECS Values手动Enumerator", measureGCAlloc(runValueECSValuesManualRead, ENTITY_COUNT, GC_REPEAT_COUNT), true);
		printGCAlloc("Dictionary foreach Values", measureGCAlloc(runValueDictionaryValuesForeach, ENTITY_COUNT, GC_REPEAT_COUNT), true);
		printGCAlloc("ECS foreach dict Key+Value", measureGCAlloc(runKeyValueECSForeach, ENTITY_COUNT, GC_REPEAT_COUNT), true);
		printGCAlloc("Dictionary foreach Key+Value", measureGCAlloc(runKeyValueDictionaryForeach, ENTITY_COUNT, GC_REPEAT_COUNT), true);
		Debug.Log("================ Enumerator Profiler GC.Alloc Regression End ================");
	}
	private GCAllocResult measureGCAlloc(Action action, int operationCount, int repeatCount)
	{
		for (int i = 0; i < GC_WARMUP_COUNT; ++i)
		{
			action();
		}
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
			for (int i = 0; i < repeatCount; ++i)
			{
				action();
			}
			recorder.Stop();
			long events = recorder.Count;
			long totalOperations = (long)operationCount * repeatCount;
			return new GCAllocResult
			{
				mValid = valid,
				mWrappedAround = recorder.WrappedAround,
				mAllocEvents = events,
				mEventsPerRun = repeatCount > 0 ? (double)events / repeatCount : 0.0,
				mEventsPerOp = totalOperations > 0 ? (double)events / totalOperations : 0.0,
			};
		}
	}
	private void printGCAlloc(string name, GCAllocResult result, bool expectedZero)
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
		Debug.Log(
			name.PadRight(38) +
			" AllocEvents:" + result.mAllocEvents.ToString().PadLeft(7) +
			" | " + result.mEventsPerRun.ToString("0.000").PadLeft(9) + " event/run" +
			" | " + result.mEventsPerOp.ToString("0.000000").PadLeft(10) + " event/op" +
			" | Gate:" + gate);
	}
	private void runKeyArrayFor()
	{
		long sum = 0;
		for (int i = 0; i < mKeys.Length; ++i)
		{
			sum += mKeys[i];
		}
		mResultSink += sum;
	}
	private void runKeyArrayForeach()
	{
		long sum = 0;
		foreach (int key in mKeys)
		{
			sum += key;
		}
		mResultSink += sum;
	}
	private void runKeyReadOnlySpanForeach()
	{
		long sum = 0;
		global::System.ReadOnlySpan<int> span = new global::System.ReadOnlySpan<int>(mKeys);
		foreach (int key in span)
		{
			sum += key;
		}
		mResultSink += sum;
	}
	private void runKeyReadOnlySpanManual()
	{
		long sum = 0;
		var enumerator = new global::System.ReadOnlySpan<int>(mKeys).GetEnumerator();
		while (enumerator.MoveNext())
		{
			sum += enumerator.Current;
		}
		mResultSink += sum;
	}
	private void runKeyGenericArrayForeach()
	{
		long sum = 0;
		foreach (int key in new GenericArrayEnumerable<int>(mKeys))
		{
			sum += key;
		}
		mResultSink += sum;
	}
	private void runKeyGenericArrayManual()
	{
		long sum = 0;
		var enumerator = new GenericArrayEnumerator<int>(mKeys);
		while (enumerator.MoveNext())
		{
			sum += enumerator.Current;
		}
		mResultSink += sum;
	}
	private void runKeyGenericArrayMoveNextOnly()
	{
		int count = 0;
		var enumerator = new GenericArrayEnumerator<int>(mKeys);
		while (enumerator.MoveNext())
		{
			++count;
		}
		mResultSink += count;
	}
	private void runKeyECSFor()
	{
		long sum = 0;
		for (int i = 0; i < mECS.Count; ++i)
		{
			sum += mECS.getKeyAt(i);
		}
		mResultSink += sum;
	}
	private void runKeyECSForeach()
	{
		long sum = 0;
		foreach (var item in mECS)
		{
			sum += item.Key;
		}
		mResultSink += sum;
	}
	private void runKeyECSKeysForeach()
	{
		long sum = 0;
		foreach (int key in mECS.Keys)
		{
			sum += key;
		}
		mResultSink += sum;
	}
	private void runKeyECSKeysManual()
	{
		long sum = 0;
		var enumerator = mECS.Keys.GetEnumerator();
		while (enumerator.MoveNext())
		{
			sum += enumerator.Current;
		}
		mResultSink += sum;
	}
	private void runKeyECSKeysMoveNextOnly()
	{
		int count = 0;
		var enumerator = mECS.Keys.GetEnumerator();
		while (enumerator.MoveNext())
		{
			++count;
		}
		mResultSink += count;
	}
	private void runKeyDictionaryForeach()
	{
		long sum = 0;
		foreach (KeyValuePair<int, RoleData> item in mDictionary)
		{
			sum += item.Key;
		}
		mResultSink += sum;
	}
	private void runKeyDictionaryKeysForeach()
	{
		long sum = 0;
		foreach (int key in mDictionary.Keys)
		{
			sum += key;
		}
		mResultSink += sum;
	}
	private void runKeyDictionaryKeysManual()
	{
		long sum = 0;
		Dictionary<int, RoleData>.KeyCollection.Enumerator enumerator = mDictionary.Keys.GetEnumerator();
		while (enumerator.MoveNext())
		{
			sum += enumerator.Current;
		}
		mResultSink += sum;
	}
	private void runValueArrayFor()
	{
		long sum = 0;
		for (int i = 0; i < mValues.Length; ++i)
		{
			sum += mValues[i].mHP;
		}
		mResultSink += sum;
	}
	private void runValueArrayForeach()
	{
		long sum = 0;
		foreach (RoleData value in mValues)
		{
			sum += value.mHP;
		}
		mResultSink += sum;
	}
	private void runValueECSFor()
	{
		long sum = 0;
		for (int i = 0; i < mECS.Count; ++i)
		{
			sum += mECS.getValueAt(i).mHP;
		}
		mResultSink += sum;
	}
	private void runValueECSForeachRead()
	{
		long sum = 0;
		foreach (var item in mECS)
		{
			sum += item.Value.mHP;
		}
		mResultSink += sum;
	}
	private void runValueECSValuesRead()
	{
		long sum = 0;
		foreach (RoleDataRef value in mECS.Values)
		{
			sum += value.mHP;
		}
		mResultSink += sum;
	}
	private void runValueECSValuesManualRead()
	{
		long sum = 0;
		var enumerator = mECS.Values.GetEnumerator();
		while (enumerator.MoveNext())
		{
			RoleDataRef value = enumerator.Current;
			sum += value.mHP;
		}
		mResultSink += sum;
	}
	private void runValueDictionaryForeach()
	{
		long sum = 0;
		foreach (KeyValuePair<int, RoleData> item in mDictionary)
		{
			sum += item.Value.mHP;
		}
		mResultSink += sum;
	}
	private void runValueDictionaryValuesForeach()
	{
		long sum = 0;
		foreach (RoleData value in mDictionary.Values)
		{
			sum += value.mHP;
		}
		mResultSink += sum;
	}
	private void runValueDictionaryValuesManual()
	{
		long sum = 0;
		Dictionary<int, RoleData>.ValueCollection.Enumerator enumerator = mDictionary.Values.GetEnumerator();
		while (enumerator.MoveNext())
		{
			sum += enumerator.Current.mHP;
		}
		mResultSink += sum;
	}
	private void runValueECSForWrite()
	{
		for (int i = 0; i < mECS.Count; ++i)
		{
			mECS.getValueAt(i).mHP += 1;
		}
		mResultSink += mECS.getValueAt(mECS.Count - 1).mHP;
	}
	private void runValueECSForeachWrite()
	{
		foreach (var item in mECS)
		{
			item.Value.mHP += 1;
		}
		mResultSink += mECS.getValueAt(mECS.Count - 1).mHP;
	}
	private void runValueECSValuesWrite()
	{
		foreach (RoleDataRef value in mECS.Values)
		{
			value.mHP += 1;
		}
		mResultSink += mECS.getValueAt(mECS.Count - 1).mHP;
	}
	private void runValueECSValuesManualWrite()
	{
		var enumerator = mECS.Values.GetEnumerator();
		while (enumerator.MoveNext())
		{
			RoleDataRef value = enumerator.Current;
			value.mHP += 1;
		}
		mResultSink += mECS.getValueAt(mECS.Count - 1).mHP;
	}
	private void runValueECSDirectWrite()
	{
		var hp = mECS.getHPColumn();
		for (int i = 0; i < mECS.Count; ++i)
		{
			hp[i] += 1;
		}
		mResultSink += hp[mECS.Count - 1];
	}
	private void runKeyValueECSFor()
	{
		long sum = 0;
		for (int i = 0; i < mECS.Count; ++i)
		{
			sum += mECS.getKeyAt(i);
			sum += mECS.getValueAt(i).mHP;
		}
		mResultSink += sum;
	}
	private void runKeyValueECSForeach()
	{
		long sum = 0;
		foreach (var item in mECS)
		{
			sum += item.Key;
			sum += item.Value.mHP;
		}
		mResultSink += sum;
	}
	private void runKeyValueECSManual()
	{
		long sum = 0;
		var enumerator = mECS.GetEnumerator();
		while (enumerator.MoveNext())
		{
			var item = enumerator.Current;
			sum += item.Key;
			sum += item.Value.mHP;
		}
		mResultSink += sum;
	}
	private void runKeyValueDictionaryForeach()
	{
		long sum = 0;
		foreach (KeyValuePair<int, RoleData> item in mDictionary)
		{
			sum += item.Key;
			sum += item.Value.mHP;
		}
		mResultSink += sum;
	}
	private void runKeyValueDictionaryManual()
	{
		long sum = 0;
		Dictionary<int, RoleData>.Enumerator enumerator = mDictionary.GetEnumerator();
		while (enumerator.MoveNext())
		{
			KeyValuePair<int, RoleData> item = enumerator.Current;
			sum += item.Key;
			sum += item.Value.mHP;
		}
		mResultSink += sum;
	}
	private BenchmarkResult measure(Action action, int operationCount)
	{
		for (int i = 0; i < WARMUP_COUNT; ++i)
		{
			action();
		}
		double[] samples = new double[SAMPLE_COUNT];
		double min = double.MaxValue;
		double max = double.MinValue;
		for (int i = 0; i < SAMPLE_COUNT; ++i)
		{
			long start = Stopwatch.GetTimestamp();
			action();
			long end = Stopwatch.GetTimestamp();
			double milliseconds = (end - start) * 1000.0 / Stopwatch.Frequency;
			samples[i] = milliseconds;
			if (milliseconds < min)
			{
				min = milliseconds;
			}
			if (milliseconds > max)
			{
				max = milliseconds;
			}
		}
		Array.Sort(samples);
		double median = samples[samples.Length / 2];
		return new BenchmarkResult { mMedian = median, mMin = min, mMax = max, mNsPerOp = median * 1000000.0 / operationCount };
	}
	private string format(string name, BenchmarkResult result)
	{
		return name.PadRight(38) + " Median:" + result.mMedian.ToString("0.000").PadLeft(9) + " ms | Min:" + result.mMin.ToString("0.000").PadLeft(8) + " | Max:" + result.mMax.ToString("0.000").PadLeft(8) + " | " + result.mNsPerOp.ToString("0.000").PadLeft(8) + " ns/op";
	}
	private string ratio(double left, double right)
	{
		return (left / right).ToString("0.00") + "x";
	}
	private RoleData createData(int id)
	{
		return new RoleData { mHP = 100 + id, mSpeed = id + 0.5f, mPositionX = id * 2.0f, mPositionY = id * 3.0f, mID = id, mCamp = id % 3 };
	}
}
