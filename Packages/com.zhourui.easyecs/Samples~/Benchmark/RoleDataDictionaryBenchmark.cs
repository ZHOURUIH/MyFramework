using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class RoleDataDictionaryBenchmark : MonoBehaviour
{
	private const int ENTITY_COUNT = 500000;
	private const int RANDOM_WRITE_COUNT = 50000;
	private const int SAMPLE_COUNT = 15;
	private const int WARMUP_COUNT = 3;
	private static double mResultSink;
	private Dictionary<int, RoleData> mStandardDictionary;
	private Dictionary<int, int> mIndexMap;
	private RoleData[] mDenseAoS;
	private int[] mDenseHP;
	private RoleDataECSDictionary<int> mECS;
	private int[] mRandomKeys;
	private struct BenchmarkResult
	{
		public double mMedian;
		public double mMin;
		public double mMax;
		public double mNsPerOp;
	}
	private void Awake()
	{
		runCorrectnessTests();
#if UNITY_EDITOR
		Debug.Log("Unity Editor环境,已完成ECSDictionary正确性检测,跳过性能Benchmark");
#else
		runPerformanceBenchmark();
#endif
	}
	private void runCorrectnessTests()
	{
		Debug.Log("================ RoleData ECSDictionary Correctness Test Start ================");
		runCorrectnessTest("Add/Indexer/Resize", testAddIndexerResize);
		runCorrectnessTest("TryAdd与重复Key", testTryAddDuplicate);
		runCorrectnessTest("TryGetValue与Ref修改", testTryGetValueModify);
		runCorrectnessTest("TryGetIndex", testTryGetIndex);
		runCorrectnessTest("Remove与SwapBack映射", testRemoveSwapBack);
		runCorrectnessTest("DenseIndex与DirectColumn", testDenseIndexDirectColumn);
		runCorrectnessTest("foreach Key+Value", testForeachKeyValue);
		runCorrectnessTest("foreach Keys", testForeachKeys);
		runCorrectnessTest("foreach Values", testForeachValues);
		runCorrectnessTest("Clear后重新使用", testClearReuse);
		runCorrectnessTest("自定义Comparer", testComparer);
		runCorrectnessTest("重复Dispose", testDoubleDispose);
		Debug.Log("================ RoleData ECSDictionary Correctness Test Pass ================");
	}
	private void runCorrectnessTest(string name, Action action)
	{
		action();
		Debug.Log("RoleData ECSDictionary CorrectnessTest Pass:" + name);
	}
	private void testAddIndexerResize()
	{
		RoleDataECSDictionary<int> dict = new RoleDataECSDictionary<int>(1);
		try
		{
			for (int i = 0; i < 32; ++i)
			{
				dict.Add(1000 + i, createData(i));
			}
			check(dict.Count == 32, "Count错误");
			check(dict.Capacity >= 32, "Capacity错误");
			for (int i = 0; i < 32; ++i)
			{
				RoleDataRef value = dict[1000 + i];
				check(value.mID == i, "Indexer mID错误,Index:" + i);
				check(value.mHP == 100 + i, "Indexer mHP错误,Index:" + i);
			}
			dict[1005].mHP = 555;
			check(dict[1005].mHP == 555, "Indexer直接修改失败");
		}
		finally
		{
			dict.Dispose();
		}
	}
	private void testTryAddDuplicate()
	{
		RoleDataECSDictionary<int> dict = new RoleDataECSDictionary<int>();
		try
		{
			check(dict.TryAdd(1, createData(1)), "第一次TryAdd失败");
			check(!dict.TryAdd(1, createData(2)), "重复TryAdd应返回false");
			check(dict.Count == 1, "重复TryAdd改变Count");
			bool exception = false;
			try
			{
				dict.Add(1, createData(3));
			}
			catch (ArgumentException)
			{
				exception = true;
			}
			check(exception, "Add重复Key应抛ArgumentException");
		}
		finally
		{
			dict.Dispose();
		}
	}
	private void testTryGetValueModify()
	{
		RoleDataECSDictionary<int> dict = new RoleDataECSDictionary<int>();
		try
		{
			dict.Add(10, createData(1));
			check(dict.TryGetValue(10, out RoleDataRef value), "TryGetValue已有Key失败");
			value.mHP = 999;
			value.mPositionX = 123.0f;
			check(dict[10].mHP == 999, "TryGetValue Ref修改mHP失败");
			check(dict[10].mPositionX == 123.0f, "TryGetValue Ref修改mPositionX失败");
			check(!dict.TryGetValue(11, out RoleDataRef missing), "不存在Key应返回false");
		}
		finally
		{
			dict.Dispose();
		}
	}
	private void testTryGetIndex()
	{
		RoleDataECSDictionary<int> dict = new RoleDataECSDictionary<int>();
		try
		{
			dict.Add(10, createData(1));
			dict.Add(20, createData(2));
			check(dict.TryGetIndex(20, out int index), "TryGetIndex失败");
			check(index == 1, "TryGetIndex索引错误");
			check(!dict.TryGetIndex(30, out int missing), "不存在Key TryGetIndex应返回false");
		}
		finally
		{
			dict.Dispose();
		}
	}
	private void testRemoveSwapBack()
	{
		RoleDataECSDictionary<int> dict = new RoleDataECSDictionary<int>();
		try
		{
			dict.Add(10, createData(1));
			dict.Add(20, createData(2));
			dict.Add(30, createData(3));
			check(dict.Remove(20), "Remove失败");
			check(dict.Count == 2, "Remove Count错误");
			check(!dict.ContainsKey(20), "删除Key仍存在");
			check(dict.ContainsKey(30), "SwapBack Key丢失");
			check(dict.TryGetIndex(30, out int movedIndex), "SwapBack Key索引丢失");
			check(movedIndex == 1, "SwapBack索引错误");
			check(dict.getKeyAt(1) == 30, "getKeyAt映射错误");
			check(dict.getValueAt(1).mID == 3, "getValueAt SwapBack数据错误");
			check(!dict.Remove(999), "删除不存在Key应返回false");
		}
		finally
		{
			dict.Dispose();
		}
	}
	private void testDenseIndexDirectColumn()
	{
		RoleDataECSDictionary<int> dict = new RoleDataECSDictionary<int>();
		try
		{
			dict.Add(10, createData(1));
			dict.Add(20, createData(2));
			check(dict.getKeyAt(0) == 10, "getKeyAt[0]错误");
			check(dict.getValueAt(1).mID == 2, "getValueAt[1]错误");
			var hp = dict.getHPColumn();
			hp[0] = 111;
			hp[1] = 222;
			check(dict[10].mHP == 111, "DirectColumn[0]错误");
			check(dict[20].mHP == 222, "DirectColumn[1]错误");
		}
		finally
		{
			dict.Dispose();
		}
	}
	private void testForeachKeyValue()
	{
		RoleDataECSDictionary<int> dict = new RoleDataECSDictionary<int>();
		try
		{
			dict.Add(10, createData(1));
			dict.Add(20, createData(2));
			dict.Add(30, createData(3));
			int keySum = 0;
			int hpSum = 0;
			int count = 0;
			foreach (var item in dict)
			{
				++count;
				keySum += item.Key;
				hpSum += item.Value.mHP;
				item.Value.mHP += 1;
			}
			check(count == 3, "foreach数量错误");
			check(keySum == 60, "foreach Key错误");
			check(hpSum == 306, "foreach Value错误");
			check(dict[10].mHP == 102, "foreach Ref修改失败");
		}
		finally
		{
			dict.Dispose();
		}
	}
	private void testForeachKeys()
	{
		RoleDataECSDictionary<int> dict = new RoleDataECSDictionary<int>();
		try
		{
			dict.Add(10, createData(1));
			dict.Add(20, createData(2));
			int sum = 0;
			foreach (int key in dict.Keys)
			{
				sum += key;
			}
			check(sum == 30, "Keys遍历错误");
		}
		finally
		{
			dict.Dispose();
		}
	}
	private void testForeachValues()
	{
		RoleDataECSDictionary<int> dict = new RoleDataECSDictionary<int>();
		try
		{
			dict.Add(10, createData(1));
			dict.Add(20, createData(2));
			int sum = 0;
			foreach (RoleDataRef value in dict.Values)
			{
				sum += value.mHP;
				value.mHP += 10;
			}
			check(sum == 203, "Values遍历错误");
			check(dict[10].mHP == 111, "Values修改失败");
			check(dict[20].mHP == 112, "Values第二个修改失败");
		}
		finally
		{
			dict.Dispose();
		}
	}
	private void testClearReuse()
	{
		RoleDataECSDictionary<int> dict = new RoleDataECSDictionary<int>(2);
		try
		{
			dict.Add(1, createData(1));
			dict.Add(2, createData(2));
			int capacity = dict.Capacity;
			dict.Clear();
			check(dict.Count == 0, "Clear后Count错误");
			check(dict.Capacity == capacity, "Clear不应改变Capacity");
			check(!dict.ContainsKey(1), "Clear后旧Key仍存在");
			dict.Add(99, createData(9));
			check(dict[99].mID == 9, "Clear后重新使用失败");
		}
		finally
		{
			dict.Dispose();
		}
	}
	private void testComparer()
	{
		RoleDataECSDictionary<string> dict = new RoleDataECSDictionary<string>(4, StringComparer.OrdinalIgnoreCase);
		try
		{
			dict.Add("Role", createData(1));
			check(dict.ContainsKey("role"), "Comparer未生效");
			check(dict["ROLE"].mID == 1, "Comparer Indexer未生效");
			check(ReferenceEquals(dict.Comparer, StringComparer.OrdinalIgnoreCase), "Comparer属性错误");
		}
		finally
		{
			dict.Dispose();
		}
	}
	private void testDoubleDispose()
	{
		RoleDataECSDictionary<int> dict = new RoleDataECSDictionary<int>();
		dict.Add(1, createData(1));
		dict.Dispose();
		dict.Dispose();
	}
	private void runPerformanceBenchmark()
	{
		initializePerformanceData();
		try
		{
			Debug.Log("================ RoleData ECSDictionary Benchmark Start ================");
			Debug.Log("ECS Backend:" + RoleDataECSList.BackendName);
			Debug.Log("Backend Reason:" + RoleDataECSList.BackendReason);
			Debug.Log("EntityCount:" + ENTITY_COUNT);
			Debug.Log("RandomWriteCount:" + RANDOM_WRITE_COUNT);
			Debug.Log("SampleCount:" + SAMPLE_COUNT);
			Debug.Log("WarmupCount:" + WARMUP_COUNT);
			runRandomReadBenchmark();
			runRandomWriteBenchmark();
			runDenseOneFieldBenchmark();
			runDenseFourFieldBenchmark();
			runMixedBenchmark();
			runEnumeratorBenchmark();
			EasyECSExtendedAPIBenchmark.runDictionaryBenchmark();
			Debug.Log("ResultSink:" + mResultSink);
			Debug.Log("================ RoleData ECSDictionary Benchmark End ================");
		}
		finally
		{
			mECS.Dispose();
			mECS = null;
		}
	}
	private void initializePerformanceData()
	{
		mStandardDictionary = new Dictionary<int, RoleData>(ENTITY_COUNT);
		mIndexMap = new Dictionary<int, int>(ENTITY_COUNT);
		mDenseAoS = new RoleData[ENTITY_COUNT];
		mDenseHP = new int[ENTITY_COUNT];
		mECS = new RoleDataECSDictionary<int>(ENTITY_COUNT);
		for (int i = 0; i < ENTITY_COUNT; ++i)
		{
			RoleData value = createData(i);
			int key = i + 1000000;
			mStandardDictionary.Add(key, value);
			mIndexMap.Add(key, i);
			mDenseAoS[i] = value;
			mDenseHP[i] = value.mHP;
			mECS.Add(key, value);
		}
		mRandomKeys = new int[ENTITY_COUNT];
		System.Random random = new System.Random(1234567);
		for (int i = 0; i < mRandomKeys.Length; ++i)
		{
			mRandomKeys[i] = random.Next(ENTITY_COUNT) + 1000000;
		}
	}
	private void runRandomReadBenchmark()
	{
		Debug.Log("================ 随机Key读取详细测试 ================");
		print("随机读取 Dictionary<int,RoleData>", measure(runRandomReadStandardDictionary, ENTITY_COUNT));
		print("随机读取 IndexMap + RoleData[]", measure(runRandomReadDenseAoS, ENTITY_COUNT));
		print("随机读取 IndexMap + int[]", measure(runRandomReadDenseSoA, ENTITY_COUNT));
		print("随机读取 ECS Inline Indexer", measure(runRandomReadECSInlineIndexer, ENTITY_COUNT));
		print("随机读取 ECS Local Ref", measure(runRandomReadECSLocalRef, ENTITY_COUNT));
		print("随机读取 ECS TryGetValue", measure(runRandomReadECSTryGet, ENTITY_COUNT));
		Debug.Log("================ 随机Key读取详细测试结束 ================");
	}
	private void runRandomWriteBenchmark()
	{
		Debug.Log("================ 随机Key修改详细测试 ================");
		print("随机修改 Dictionary<int,RoleData>", measure(runRandomWriteStandardDictionary, RANDOM_WRITE_COUNT));
		print("随机修改 IndexMap + RoleData[]", measure(runRandomWriteDenseAoS, RANDOM_WRITE_COUNT));
		print("随机修改 IndexMap + int[]", measure(runRandomWriteDenseSoA, RANDOM_WRITE_COUNT));
		print("随机修改 ECS Inline Indexer", measure(runRandomWriteECSInlineIndexer, RANDOM_WRITE_COUNT));
		print("随机修改 ECS Local Ref", measure(runRandomWriteECSLocalRef, RANDOM_WRITE_COUNT));
		print("随机修改 ECS TryGetValue", measure(runRandomWriteECSTryGet, RANDOM_WRITE_COUNT));
		runTryGetValueWriteBreakdown();
		Debug.Log("================ 随机Key修改详细测试结束 ================");
	}
	private void runTryGetValueWriteBreakdown()
	{
		BenchmarkResult tryOnly = measure(runRandomTryGetOnly, RANDOM_WRITE_COUNT);
		BenchmarkResult tryRead = measure(runRandomTryGetRead, RANDOM_WRITE_COUNT);
		BenchmarkResult tryWrite = measure(runRandomWriteECSTryGet, RANDOM_WRITE_COUNT);
		BenchmarkResult indexOnly = measure(runRandomTryGetIndexOnly, RANDOM_WRITE_COUNT);
		BenchmarkResult indexRefWrite = measure(runRandomTryGetIndexRefWrite, RANDOM_WRITE_COUNT);
		BenchmarkResult indexDirectWrite = measure(runRandomTryGetIndexDirectWrite, RANDOM_WRITE_COUNT);
		BenchmarkResult indexerWrite = measure(runRandomWriteECSInlineIndexer, RANDOM_WRITE_COUNT);
		BenchmarkResult localRefWrite = measure(runRandomWriteECSLocalRef, RANDOM_WRITE_COUNT);
		Debug.Log(
			"\n================ SafeSpan TryGetValue写入路径拆解 ================\n" +
			format("TryGetValue only", tryOnly) + "\n" +
			format("TryGetValue + read", tryRead) + "\n" +
			format("TryGetValue + write", tryWrite) + "\n" +
			format("TryGetIndex only", indexOnly) + "\n" +
			format("TryGetIndex + Ref write", indexRefWrite) + "\n" +
			format("TryGetIndex + Direct write", indexDirectWrite) + "\n" +
			format("Indexer write", indexerWrite) + "\n" +
			format("Local Ref write", localRefWrite) +
			"\n--------------------------------------------------\n" +
			"TryWrite / TryRead          : " + ratio(tryWrite.mMedian, tryRead.mMedian) + "\n" +
			"TryWrite / IndexRefWrite    : " + ratio(tryWrite.mMedian, indexRefWrite.mMedian) + "\n" +
			"IndexRefWrite / Indexer     : " + ratio(indexRefWrite.mMedian, indexerWrite.mMedian) + "\n" +
			"IndexDirectWrite / Indexer  : " + ratio(indexDirectWrite.mMedian, indexerWrite.mMedian) + "\n" +
			"TryOnly / IndexOnly         : " + ratio(tryOnly.mMedian, indexOnly.mMedian) + "\n" +
			"==================================================");
	}
	private void runRandomTryGetOnly()
	{
		int found = 0;
		for (int i = 0; i < RANDOM_WRITE_COUNT; ++i)
		{
			if (mECS.TryGetValue(mRandomKeys[i], out RoleDataRef value))
			{
				++found;
			}
		}
		mResultSink += found;
	}
	private void runRandomTryGetRead()
	{
		long sum = 0;
		for (int i = 0; i < RANDOM_WRITE_COUNT; ++i)
		{
			if (mECS.TryGetValue(mRandomKeys[i], out RoleDataRef value))
			{
				sum += value.mHP;
			}
		}
		mResultSink += sum;
	}
	private void runRandomTryGetIndexOnly()
	{
		long sum = 0;
		for (int i = 0; i < RANDOM_WRITE_COUNT; ++i)
		{
			if (mECS.TryGetIndex(mRandomKeys[i], out int index))
			{
				sum += index;
			}
		}
		mResultSink += sum;
	}
	private void runRandomTryGetIndexRefWrite()
	{
		for (int i = 0; i < RANDOM_WRITE_COUNT; ++i)
		{
			if (mECS.TryGetIndex(mRandomKeys[i], out int index))
			{
				mECS.getValueAt(index).mHP += 1;
			}
		}
		mResultSink += mECS.getValueAt(mECS.Count - 1).mHP;
	}
	private void runRandomTryGetIndexDirectWrite()
	{
		var hp = mECS.getHPColumn();
		for (int i = 0; i < RANDOM_WRITE_COUNT; ++i)
		{
			if (mECS.TryGetIndex(mRandomKeys[i], out int index))
			{
				hp[index] += 1;
			}
		}
		mResultSink += hp[mECS.Count - 1];
	}
	private void runDenseOneFieldBenchmark()
	{
		BenchmarkResult standard = measure(runDenseOneStandardDictionary, ENTITY_COUNT);
		BenchmarkResult denseAoS = measure(runDenseOneAoS, ENTITY_COUNT);
		BenchmarkResult denseSoA = measure(runDenseOneSoA, ENTITY_COUNT);
		BenchmarkResult ecsRef = measure(runDenseOneECSRef, ENTITY_COUNT);
		BenchmarkResult ecsDirect = measure(runDenseOneECSDirect, ENTITY_COUNT);
		Debug.Log("\n================ 连续存储全量修改1个字段 ================\n" + format("Dictionary Key全量更新", standard) + "\n" + format("Dense RoleData[]", denseAoS) + "\n" + format("Dense int[]", denseSoA) + "\n" + format("ECS Dense Ref", ecsRef) + "\n" + format("ECS Direct", ecsDirect) + "\n--------------------------------------------------\nDense AoS / Dense SoA : " + ratio(denseAoS.mMedian, denseSoA.mMedian) + "\nDense SoA / ECS Direct: " + ratio(denseSoA.mMedian, ecsDirect.mMedian) + "\nECS Ref / Direct      : " + ratio(ecsRef.mMedian, ecsDirect.mMedian) + "\n==================================================");
	}
	private void runDenseFourFieldBenchmark()
	{
		BenchmarkResult standard = measure(runDenseFourStandardDictionary, ENTITY_COUNT);
		BenchmarkResult denseAoS = measure(runDenseFourAoS, ENTITY_COUNT);
		BenchmarkResult ecsRef = measure(runDenseFourECSRef, ENTITY_COUNT);
		BenchmarkResult ecsDirect = measure(runDenseFourECSDirect, ENTITY_COUNT);
		Debug.Log("\n================ 连续存储全量访问4个字段 ================\n" + format("Dictionary Key全量更新", standard) + "\n" + format("Dense RoleData[]", denseAoS) + "\n" + format("ECS Dense Ref", ecsRef) + "\n" + format("ECS Direct", ecsDirect) + "\n--------------------------------------------------\nDense AoS / ECS Direct : " + ratio(denseAoS.mMedian, ecsDirect.mMedian) + "\nECS Ref / Direct       : " + ratio(ecsRef.mMedian, ecsDirect.mMedian) + "\n==================================================");
	}
	private void runMixedBenchmark()
	{
		BenchmarkResult standard = measure(runMixedStandard, ENTITY_COUNT + RANDOM_WRITE_COUNT);
		BenchmarkResult manual = measure(runMixedManual, ENTITY_COUNT + RANDOM_WRITE_COUNT);
		BenchmarkResult ecs = measure(runMixedECS, ENTITY_COUNT + RANDOM_WRITE_COUNT);
		Debug.Log("\n================ 混合场景:Dense全量更新+10%随机Key修改 ================\n" + format("Dictionary<int,RoleData>", standard) + "\n" + format("IndexMap + RoleData[]", manual) + "\n" + format("ECS Direct+LocalRef", ecs) + "\n--------------------------------------------------\nStandard / ECS : " + ratio(standard.mMedian, ecs.mMedian) + "\nManual / ECS   : " + ratio(manual.mMedian, ecs.mMedian) + "\n==================================================");
	}
	private void runEnumeratorBenchmark()
	{
		Debug.Log("================ ECSDictionary遍历性能测试 ================");
		BenchmarkResult keyFor = measure(runEnumeratorKeyFor, ENTITY_COUNT);
		BenchmarkResult keyForeach = measure(runEnumeratorKeyForeach, ENTITY_COUNT);
		BenchmarkResult keys = measure(runEnumeratorKeys, ENTITY_COUNT);
		Debug.Log("\n================ 仅遍历Key ================\n" + format("for + getKeyAt", keyFor) + "\n" + format("foreach dict + item.Key", keyForeach) + "\n" + format("foreach dict.Keys", keys) + "\n--------------------------------------------------\nforeach dict / Keys : " + ratio(keyForeach.mMedian, keys.mMedian) + "\nfor / Keys          : " + ratio(keyFor.mMedian, keys.mMedian) + "\n==================================================");
		BenchmarkResult valueFor = measure(runEnumeratorValueFor, ENTITY_COUNT);
		BenchmarkResult valueForeach = measure(runEnumeratorValueForeach, ENTITY_COUNT);
		BenchmarkResult values = measure(runEnumeratorValues, ENTITY_COUNT);
		BenchmarkResult direct = measure(runEnumeratorDirect, ENTITY_COUNT);
		Debug.Log("\n================ 仅遍历Value修改1个字段 ================\n" + format("for + getValueAt", valueFor) + "\n" + format("foreach dict + item.Value", valueForeach) + "\n" + format("foreach dict.Values", values) + "\n" + format("Direct Column", direct) + "\n--------------------------------------------------\nforeach dict / Values : " + ratio(valueForeach.mMedian, values.mMedian) + "\nfor / Values          : " + ratio(valueFor.mMedian, values.mMedian) + "\nValues / Direct       : " + ratio(values.mMedian, direct.mMedian) + "\n==================================================");
		BenchmarkResult keyValueFor = measure(runEnumeratorKeyValueFor, ENTITY_COUNT);
		BenchmarkResult keyValueForeach = measure(runEnumeratorKeyValueForeach, ENTITY_COUNT);
		Debug.Log("\n================ 同时遍历Key+Value ================\n" + format("for getKeyAt+getValueAt", keyValueFor) + "\n" + format("foreach dict", keyValueForeach) + "\n--------------------------------------------------\nfor / foreach : " + ratio(keyValueFor.mMedian, keyValueForeach.mMedian) + "\n==================================================");
		Debug.Log("================ ECSDictionary遍历性能测试结束 ================");
	}
	private void runRandomReadStandardDictionary()
	{
		long sum = 0;
		for (int i = 0; i < mRandomKeys.Length; ++i)
		{
			sum += mStandardDictionary[mRandomKeys[i]].mHP;
		}
		mResultSink += sum;
	}
	private void runRandomReadDenseAoS()
	{
		long sum = 0;
		for (int i = 0; i < mRandomKeys.Length; ++i)
		{
			sum += mDenseAoS[mIndexMap[mRandomKeys[i]]].mHP;
		}
		mResultSink += sum;
	}
	private void runRandomReadDenseSoA()
	{
		long sum = 0;
		for (int i = 0; i < mRandomKeys.Length; ++i)
		{
			sum += mDenseHP[mIndexMap[mRandomKeys[i]]];
		}
		mResultSink += sum;
	}
	private void runRandomReadECSInlineIndexer()
	{
		long sum = 0;
		for (int i = 0; i < mRandomKeys.Length; ++i)
		{
			sum += mECS[mRandomKeys[i]].mHP;
		}
		mResultSink += sum;
	}
	private void runRandomReadECSLocalRef()
	{
		long sum = 0;
		for (int i = 0; i < mRandomKeys.Length; ++i)
		{
			RoleDataRef value = mECS[mRandomKeys[i]];
			sum += value.mHP;
		}
		mResultSink += sum;
	}
	private void runRandomReadECSTryGet()
	{
		long sum = 0;
		for (int i = 0; i < mRandomKeys.Length; ++i)
		{
			if (mECS.TryGetValue(mRandomKeys[i], out RoleDataRef value))
			{
				sum += value.mHP;
			}
		}
		mResultSink += sum;
	}
	private void runRandomWriteStandardDictionary()
	{
		for (int i = 0; i < RANDOM_WRITE_COUNT; ++i)
		{
			int key = mRandomKeys[i];
			RoleData value = mStandardDictionary[key];
			value.mHP += 1;
			mStandardDictionary[key] = value;
		}
		mResultSink += mStandardDictionary[mRandomKeys[RANDOM_WRITE_COUNT - 1]].mHP;
	}
	private void runRandomWriteDenseAoS()
	{
		for (int i = 0; i < RANDOM_WRITE_COUNT; ++i)
		{
			mDenseAoS[mIndexMap[mRandomKeys[i]]].mHP += 1;
		}
		mResultSink += mDenseAoS[mIndexMap[mRandomKeys[RANDOM_WRITE_COUNT - 1]]].mHP;
	}
	private void runRandomWriteDenseSoA()
	{
		for (int i = 0; i < RANDOM_WRITE_COUNT; ++i)
		{
			mDenseHP[mIndexMap[mRandomKeys[i]]] += 1;
		}
		mResultSink += mDenseHP[mIndexMap[mRandomKeys[RANDOM_WRITE_COUNT - 1]]];
	}
	private void runRandomWriteECSInlineIndexer()
	{
		for (int i = 0; i < RANDOM_WRITE_COUNT; ++i)
		{
			mECS[mRandomKeys[i]].mHP += 1;
		}
		mResultSink += mECS[mRandomKeys[RANDOM_WRITE_COUNT - 1]].mHP;
	}
	private void runRandomWriteECSLocalRef()
	{
		for (int i = 0; i < RANDOM_WRITE_COUNT; ++i)
		{
			RoleDataRef value = mECS[mRandomKeys[i]];
			value.mHP += 1;
		}
		mResultSink += mECS[mRandomKeys[RANDOM_WRITE_COUNT - 1]].mHP;
	}
	private void runRandomWriteECSTryGet()
	{
		for (int i = 0; i < RANDOM_WRITE_COUNT; ++i)
		{
			if (mECS.TryGetValue(mRandomKeys[i], out RoleDataRef value))
			{
				value.mHP += 1;
			}
		}
		mResultSink += mECS[mRandomKeys[RANDOM_WRITE_COUNT - 1]].mHP;
	}
	private void runDenseOneStandardDictionary()
	{
		for (int i = 0; i < ENTITY_COUNT; ++i)
		{
			int key = i + 1000000;
			RoleData value = mStandardDictionary[key];
			value.mHP += 1;
			mStandardDictionary[key] = value;
		}
		mResultSink += mStandardDictionary[1000000 + ENTITY_COUNT - 1].mHP;
	}
	private void runDenseOneAoS()
	{
		for (int i = 0; i < ENTITY_COUNT; ++i)
		{
			mDenseAoS[i].mHP += 1;
		}
		mResultSink += mDenseAoS[ENTITY_COUNT - 1].mHP;
	}
	private void runDenseOneSoA()
	{
		for (int i = 0; i < ENTITY_COUNT; ++i)
		{
			mDenseHP[i] += 1;
		}
		mResultSink += mDenseHP[ENTITY_COUNT - 1];
	}
	private void runDenseOneECSRef()
	{
		for (int i = 0; i < mECS.Count; ++i)
		{
			mECS.getValueAt(i).mHP += 1;
		}
		mResultSink += mECS.getValueAt(mECS.Count - 1).mHP;
	}
	private void runDenseOneECSDirect()
	{
		var hp = mECS.getHPColumn();
		for (int i = 0; i < mECS.Count; ++i)
		{
			hp[i] += 1;
		}
		mResultSink += hp[mECS.Count - 1];
	}
	private void runDenseFourStandardDictionary()
	{
		for (int i = 0; i < ENTITY_COUNT; ++i)
		{
			int key = i + 1000000;
			RoleData value = mStandardDictionary[key];
			value.mHP += 1;
			value.mPositionX += value.mSpeed;
			value.mPositionY -= value.mSpeed;
			mStandardDictionary[key] = value;
		}
		RoleData last = mStandardDictionary[1000000 + ENTITY_COUNT - 1];
		mResultSink += last.mHP + last.mPositionX + last.mPositionY;
	}
	private void runDenseFourAoS()
	{
		for (int i = 0; i < ENTITY_COUNT; ++i)
		{
			mDenseAoS[i].mHP += 1;
			mDenseAoS[i].mPositionX += mDenseAoS[i].mSpeed;
			mDenseAoS[i].mPositionY -= mDenseAoS[i].mSpeed;
		}
		RoleData last = mDenseAoS[ENTITY_COUNT - 1];
		mResultSink += last.mHP + last.mPositionX + last.mPositionY;
	}
	private void runDenseFourECSRef()
	{
		for (int i = 0; i < mECS.Count; ++i)
		{
			RoleDataRef value = mECS.getValueAt(i);
			value.mHP += 1;
			value.mPositionX += value.mSpeed;
			value.mPositionY -= value.mSpeed;
		}
		RoleDataRef last = mECS.getValueAt(mECS.Count - 1);
		mResultSink += last.mHP + last.mPositionX + last.mPositionY;
	}
	private void runDenseFourECSDirect()
	{
		var hp = mECS.getHPColumn();
		var speed = mECS.getSpeedColumn();
		var positionX = mECS.getPositionXColumn();
		var positionY = mECS.getPositionYColumn();
		for (int i = 0; i < mECS.Count; ++i)
		{
			hp[i] += 1;
			positionX[i] += speed[i];
			positionY[i] -= speed[i];
		}
		int last = mECS.Count - 1;
		mResultSink += hp[last] + positionX[last] + positionY[last];
	}
	private void runMixedStandard()
	{
		for (int i = 0; i < ENTITY_COUNT; ++i)
		{
			int key = i + 1000000;
			RoleData value = mStandardDictionary[key];
			value.mPositionX += value.mSpeed;
			mStandardDictionary[key] = value;
		}
		for (int i = 0; i < RANDOM_WRITE_COUNT; ++i)
		{
			int key = mRandomKeys[i];
			RoleData value = mStandardDictionary[key];
			value.mHP += 1;
			mStandardDictionary[key] = value;
		}
		mResultSink += mStandardDictionary[mRandomKeys[RANDOM_WRITE_COUNT - 1]].mHP;
	}
	private void runMixedManual()
	{
		for (int i = 0; i < ENTITY_COUNT; ++i)
		{
			mDenseAoS[i].mPositionX += mDenseAoS[i].mSpeed;
		}
		for (int i = 0; i < RANDOM_WRITE_COUNT; ++i)
		{
			mDenseAoS[mIndexMap[mRandomKeys[i]]].mHP += 1;
		}
		mResultSink += mDenseAoS[mIndexMap[mRandomKeys[RANDOM_WRITE_COUNT - 1]]].mHP;
	}
	private void runMixedECS()
	{
		var speed = mECS.getSpeedColumn();
		var positionX = mECS.getPositionXColumn();
		for (int i = 0; i < mECS.Count; ++i)
		{
			positionX[i] += speed[i];
		}
		for (int i = 0; i < RANDOM_WRITE_COUNT; ++i)
		{
			RoleDataRef value = mECS[mRandomKeys[i]];
			value.mHP += 1;
		}
		mResultSink += mECS[mRandomKeys[RANDOM_WRITE_COUNT - 1]].mHP;
	}
	private void runEnumeratorKeyFor()
	{
		long sum = 0;
		for (int i = 0; i < mECS.Count; ++i)
		{
			sum += mECS.getKeyAt(i);
		}
		mResultSink += sum;
	}
	private void runEnumeratorKeyForeach()
	{
		long sum = 0;
		foreach (var item in mECS)
		{
			sum += item.Key;
		}
		mResultSink += sum;
	}
	private void runEnumeratorKeys()
	{
		long sum = 0;
		foreach (int key in mECS.Keys)
		{
			sum += key;
		}
		mResultSink += sum;
	}
	private void runEnumeratorValueFor()
	{
		for (int i = 0; i < mECS.Count; ++i)
		{
			mECS.getValueAt(i).mHP += 1;
		}
		mResultSink += mECS.getValueAt(mECS.Count - 1).mHP;
	}
	private void runEnumeratorValueForeach()
	{
		foreach (var item in mECS)
		{
			item.Value.mHP += 1;
		}
		mResultSink += mECS.getValueAt(mECS.Count - 1).mHP;
	}
	private void runEnumeratorValues()
	{
		foreach (RoleDataRef value in mECS.Values)
		{
			value.mHP += 1;
		}
		mResultSink += mECS.getValueAt(mECS.Count - 1).mHP;
	}
	private void runEnumeratorDirect()
	{
		var hp = mECS.getHPColumn();
		for (int i = 0; i < mECS.Count; ++i)
		{
			hp[i] += 1;
		}
		mResultSink += hp[mECS.Count - 1];
	}
	private void runEnumeratorKeyValueFor()
	{
		long sum = 0;
		for (int i = 0; i < mECS.Count; ++i)
		{
			sum += mECS.getKeyAt(i);
			sum += mECS.getValueAt(i).mHP;
		}
		mResultSink += sum;
	}
	private void runEnumeratorKeyValueForeach()
	{
		long sum = 0;
		foreach (var item in mECS)
		{
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
	private void print(string name, BenchmarkResult result)
	{
		Debug.Log(format(name, result));
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
	private void check(bool condition, string message)
	{
		if (!condition)
		{
			throw new Exception(message);
		}
	}
}
