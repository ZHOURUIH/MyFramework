using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class RoleDataBenchmark : MonoBehaviour
{
	private const int ENTITY_COUNT = 500000;
	private const int SAMPLE_COUNT = 15;
	private const int WARMUP_COUNT = 3;
	private static double mResultSink;
	private List<RoleData> mList;
	private RoleData[] mArray;
	private RoleDataECSList mECSList;
	private struct BenchmarkResult
	{
		public double mMedian;
		public double mMin;
		public double mMax;
		public double mNsPerEntity;
	}
	private void Awake()
	{
		runBenchmark();
	}
	private void runBenchmark()
	{
		Debug.Log("================ RoleData Correctness Test Start ================");
		runCorrectnessTests();
		Debug.Log("================ RoleData Correctness Test Pass ================");
#if UNITY_EDITOR
		Debug.Log("Unity Editor环境,已完成ECS正确性检测,跳过性能Benchmark");
		return;
#else
		initializePerformanceData();
		try
		{
			Debug.Log("================ RoleData Benchmark Start ================");
			Debug.Log("ECS Backend:" + RoleDataECSList.BackendName);
			Debug.Log("Backend Reason:" + RoleDataECSList.BackendReason);
			Debug.Log("EntityCount:" + ENTITY_COUNT);
			Debug.Log("SampleCount:" + SAMPLE_COUNT);
			Debug.Log("WarmupCount:" + WARMUP_COUNT);
			runTest1();
			runTest2();
			runTest4();
			Debug.Log("ResultSink:" + mResultSink);
			Debug.Log("================ RoleData Benchmark End ================");
		}
		finally
		{
			mECSList.Dispose();
			mECSList = null;
		}
#endif
	}
	private void runCorrectnessTests()
	{
		runCorrectnessTest("Add/Get/Resize", testAddGetResize);
		runCorrectnessTest("Set", testSet);
		runCorrectnessTest("Indexer直接修改", testIndexerModify);
		runCorrectnessTest("RoleDataRef", testRoleDataRef);
		runCorrectnessTest("Resize后RoleDataRef", testResizeAfterRoleDataRef);
		runCorrectnessTest("Direct Column", testDirectColumn);
		runCorrectnessTest("Clear后重新使用", testClearReuse);
		runCorrectnessTest("RemoveAtSwapBack", testRemoveAtSwapBack);
		runCorrectnessTest("重复Dispose", testDoubleDispose);
	}
	private void runCorrectnessTest(string name, Action action)
	{
		action();
		Debug.Log("RoleData CorrectnessTest Pass:" + name);
	}
	private void testAddGetResize()
	{
		RoleDataECSList list = new RoleDataECSList(1);
		try
		{
			for (int i = 0; i < 32; ++i)
			{
				list.Add(createData(i));
			}
			check(list.Count == 32, "Count错误");
			check(list.Capacity >= 32, "Resize后Capacity错误");
			for (int i = 0; i < 32; ++i)
			{
				RoleData value = list.Get(i);
				check(value.mHP == 100 + i, "mHP错误,Index:" + i);
				check(value.mSpeed == i + 0.5f, "mSpeed错误,Index:" + i);
				check(value.mPositionX == i * 2.0f, "mPositionX错误,Index:" + i);
				check(value.mPositionY == i * 3.0f, "mPositionY错误,Index:" + i);
				check(value.mID == i, "mID错误,Index:" + i);
				check(value.mCamp == i % 3, "mCamp错误,Index:" + i);
			}
		}
		finally
		{
			list.Dispose();
		}
	}
	private void testSet()
	{
		RoleDataECSList list = new RoleDataECSList();
		try
		{
			list.Add(createData(1));
			RoleData value = createData(9);
			value.mHP = 999;
			list.Set(0, value);
			RoleData result = list.Get(0);
			check(result.mHP == 999, "Set mHP失败");
			check(result.mID == 9, "Set AoS字段失败");
		}
		finally
		{
			list.Dispose();
		}
	}
	private void testIndexerModify()
	{
		RoleDataECSList list = new RoleDataECSList();
		try
		{
			list.Add(createData(1));
			list[0].mHP = 777;
			list[0].mSpeed = 8.5f;
			list[0].mID = 99;
			check(list.Get(0).mHP == 777, "Indexer修改mHP失败");
			check(list.Get(0).mSpeed == 8.5f, "Indexer修改mSpeed失败");
			check(list.Get(0).mID == 99, "Indexer修改AoS字段失败");
		}
		finally
		{
			list.Dispose();
		}
	}
	private void testRoleDataRef()
	{
		RoleDataECSList list = new RoleDataECSList();
		try
		{
			list.Add(createData(1));
			RoleDataRef value = list[0];
			value.mHP += 10;
			value.mPositionX += 3.0f;
			value.mCamp = 8;
			check(list.Get(0).mHP == 111, "Ref修改mHP失败");
			check(list.Get(0).mPositionX == 5.0f, "Ref修改mPositionX失败");
			check(list.Get(0).mCamp == 8, "Ref修改AoS字段失败");
		}
		finally
		{
			list.Dispose();
		}
	}
	private void testResizeAfterRoleDataRef()
	{
		RoleDataECSList list = new RoleDataECSList(1);
		try
		{
			list.Add(createData(1));
			RoleDataRef first = list[0];
			for (int i = 2; i <= 64; ++i)
			{
				list.Add(createData(i));
			}
			first.mHP = 12345;
			first.mPositionY = 456.0f;
			first.mID = 888;
			check(list.Get(0).mHP == 12345, "Resize后旧Ref修改mHP失败");
			check(list.Get(0).mPositionY == 456.0f, "Resize后旧Ref修改mPositionY失败");
			check(list.Get(0).mID == 888, "Resize后旧Ref修改AoS字段失败");
		}
		finally
		{
			list.Dispose();
		}
	}
	private void testDirectColumn()
	{
		RoleDataECSList list = new RoleDataECSList();
		try
		{
			list.Add(createData(1));
			list.Add(createData(2));
			var hp = list.getHPColumn();
			var speed = list.getSpeedColumn();
			var positionX = list.getPositionXColumn();
			hp[0] = 1000;
			hp[1] = 2000;
			speed[0] = 10.0f;
			positionX[1] = 20.0f;
			check(list.Get(0).mHP == 1000, "Direct HP[0]失败");
			check(list.Get(1).mHP == 2000, "Direct HP[1]失败");
			check(list.Get(0).mSpeed == 10.0f, "Direct Speed失败");
			check(list.Get(1).mPositionX == 20.0f, "Direct PositionX失败");
		}
		finally
		{
			list.Dispose();
		}
	}
	private void testClearReuse()
	{
		RoleDataECSList list = new RoleDataECSList(2);
		try
		{
			list.Add(createData(1));
			list.Add(createData(2));
			int capacity = list.Capacity;
			list.Clear();
			check(list.Count == 0, "Clear后Count错误");
			check(list.Capacity == capacity, "Clear不应修改Capacity");
			list.Add(createData(9));
			check(list.Get(0).mID == 9, "Clear后重新Add失败");
		}
		finally
		{
			list.Dispose();
		}
	}
	private void testRemoveAtSwapBack()
	{
		RoleDataECSList list = new RoleDataECSList();
		try
		{
			list.Add(createData(1));
			list.Add(createData(2));
			list.Add(createData(3));
			list.RemoveAtSwapBack(1);
			check(list.Count == 2, "RemoveAtSwapBack Count错误");
			check(list.Get(1).mID == 3, "SwapBack数据错误");
			check(list.Get(1).mHP == 103, "SwapBack SoA字段错误");
		}
		finally
		{
			list.Dispose();
		}
	}
	private void testDoubleDispose()
	{
		RoleDataECSList list = new RoleDataECSList();
		list.Add(createData(1));
		list.Dispose();
		list.Dispose();
	}
	private void initializePerformanceData()
	{
		mList = new List<RoleData>(ENTITY_COUNT);
		mArray = new RoleData[ENTITY_COUNT];
		mECSList = new RoleDataECSList(ENTITY_COUNT);
		for (int i = 0; i < ENTITY_COUNT; ++i)
		{
			RoleData value = createData(i);
			mList.Add(value);
			mArray[i] = value;
			mECSList.Add(value);
		}
	}
	private void runTest1()
	{
		BenchmarkResult managedList = measure(runListOneField, ENTITY_COUNT);
		BenchmarkResult array = measure(runArrayOneField, ENTITY_COUNT);
		BenchmarkResult index = measure(runECSIndexerOneField, ENTITY_COUNT);
		BenchmarkResult localRef = measure(runECSRefOneField, ENTITY_COUNT);
		BenchmarkResult direct = measure(runECSDirectOneField, ENTITY_COUNT);
		printResult("修改1个字段", managedList, array, index, localRef, direct);
	}
	private void runTest2()
	{
		BenchmarkResult managedList = measure(runListTwoFields, ENTITY_COUNT);
		BenchmarkResult array = measure(runArrayTwoFields, ENTITY_COUNT);
		BenchmarkResult index = measure(runECSIndexerTwoFields, ENTITY_COUNT);
		BenchmarkResult localRef = measure(runECSRefTwoFields, ENTITY_COUNT);
		BenchmarkResult direct = measure(runECSDirectTwoFields, ENTITY_COUNT);
		printResult("访问2个字段", managedList, array, index, localRef, direct);
	}
	private void runTest4()
	{
		BenchmarkResult managedList = measure(runListFourFields, ENTITY_COUNT);
		BenchmarkResult array = measure(runArrayFourFields, ENTITY_COUNT);
		BenchmarkResult index = measure(runECSIndexerFourFields, ENTITY_COUNT);
		BenchmarkResult localRef = measure(runECSRefFourFields, ENTITY_COUNT);
		BenchmarkResult direct = measure(runECSDirectFourFields, ENTITY_COUNT);
		printResult("访问4个字段", managedList, array, index, localRef, direct);
	}
	private void runListOneField()
	{
		for (int i = 0; i < mList.Count; ++i)
		{
			RoleData value = mList[i];
			value.mHP += 1;
			mList[i] = value;
		}
		mResultSink += mList[ENTITY_COUNT - 1].mHP;
	}
	private void runArrayOneField()
	{
		for (int i = 0; i < mArray.Length; ++i)
		{
			mArray[i].mHP += 1;
		}
		mResultSink += mArray[ENTITY_COUNT - 1].mHP;
	}
	private void runECSIndexerOneField()
	{
		for (int i = 0; i < mECSList.Count; ++i)
		{
			mECSList[i].mHP += 1;
		}
		mResultSink += mECSList[mECSList.Count - 1].mHP;
	}
	private void runECSRefOneField()
	{
		for (int i = 0; i < mECSList.Count; ++i)
		{
			RoleDataRef value = mECSList[i];
			value.mHP += 1;
		}
		mResultSink += mECSList[mECSList.Count - 1].mHP;
	}
	private void runECSDirectOneField()
	{
		var hp = mECSList.getHPColumn();
		for (int i = 0; i < mECSList.Count; ++i)
		{
			hp[i] += 1;
		}
		mResultSink += hp[mECSList.Count - 1];
	}
	private void runListTwoFields()
	{
		for (int i = 0; i < mList.Count; ++i)
		{
			RoleData value = mList[i];
			value.mPositionX += value.mSpeed;
			mList[i] = value;
		}
		mResultSink += mList[ENTITY_COUNT - 1].mPositionX;
	}
	private void runArrayTwoFields()
	{
		for (int i = 0; i < mArray.Length; ++i)
		{
			mArray[i].mPositionX += mArray[i].mSpeed;
		}
		mResultSink += mArray[ENTITY_COUNT - 1].mPositionX;
	}
	private void runECSIndexerTwoFields()
	{
		for (int i = 0; i < mECSList.Count; ++i)
		{
			mECSList[i].mPositionX += mECSList[i].mSpeed;
		}
		mResultSink += mECSList[mECSList.Count - 1].mPositionX;
	}
	private void runECSRefTwoFields()
	{
		for (int i = 0; i < mECSList.Count; ++i)
		{
			RoleDataRef value = mECSList[i];
			value.mPositionX += value.mSpeed;
		}
		mResultSink += mECSList[mECSList.Count - 1].mPositionX;
	}
	private void runECSDirectTwoFields()
	{
		var speed = mECSList.getSpeedColumn();
		var positionX = mECSList.getPositionXColumn();
		for (int i = 0; i < mECSList.Count; ++i)
		{
			positionX[i] += speed[i];
		}
		mResultSink += positionX[mECSList.Count - 1];
	}
	private void runListFourFields()
	{
		for (int i = 0; i < mList.Count; ++i)
		{
			RoleData value = mList[i];
			value.mHP += 1;
			value.mPositionX += value.mSpeed;
			value.mPositionY -= value.mSpeed;
			mList[i] = value;
		}
		mResultSink += mList[ENTITY_COUNT - 1].mHP + mList[ENTITY_COUNT - 1].mPositionX + mList[ENTITY_COUNT - 1].mPositionY;
	}
	private void runArrayFourFields()
	{
		for (int i = 0; i < mArray.Length; ++i)
		{
			mArray[i].mHP += 1;
			mArray[i].mPositionX += mArray[i].mSpeed;
			mArray[i].mPositionY -= mArray[i].mSpeed;
		}
		mResultSink += mArray[ENTITY_COUNT - 1].mHP + mArray[ENTITY_COUNT - 1].mPositionX + mArray[ENTITY_COUNT - 1].mPositionY;
	}
	private void runECSIndexerFourFields()
	{
		for (int i = 0; i < mECSList.Count; ++i)
		{
			mECSList[i].mHP += 1;
			mECSList[i].mPositionX += mECSList[i].mSpeed;
			mECSList[i].mPositionY -= mECSList[i].mSpeed;
		}
		RoleDataRef last = mECSList[mECSList.Count - 1];
		mResultSink += last.mHP + last.mPositionX + last.mPositionY;
	}
	private void runECSRefFourFields()
	{
		for (int i = 0; i < mECSList.Count; ++i)
		{
			RoleDataRef value = mECSList[i];
			value.mHP += 1;
			value.mPositionX += value.mSpeed;
			value.mPositionY -= value.mSpeed;
		}
		RoleDataRef last = mECSList[mECSList.Count - 1];
		mResultSink += last.mHP + last.mPositionX + last.mPositionY;
	}
	private void runECSDirectFourFields()
	{
		var hp = mECSList.getHPColumn();
		var speed = mECSList.getSpeedColumn();
		var positionX = mECSList.getPositionXColumn();
		var positionY = mECSList.getPositionYColumn();
		for (int i = 0; i < mECSList.Count; ++i)
		{
			hp[i] += 1;
			positionX[i] += speed[i];
			positionY[i] -= speed[i];
		}
		int last = mECSList.Count - 1;
		mResultSink += hp[last] + positionX[last] + positionY[last];
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
		return new BenchmarkResult { mMedian = median, mMin = min, mMax = max, mNsPerEntity = median * 1000000.0 / operationCount };
	}
	private void printResult(string title, BenchmarkResult managedList, BenchmarkResult array, BenchmarkResult index, BenchmarkResult localRef, BenchmarkResult direct)
	{
		Debug.Log("\n================ " + title + " ================\n" + format("List<RoleData>", managedList) + "\n" + format("RoleData[]", array) + "\n" + format("ECS list[i]", index) + "\n" + format("ECS Ref", localRef) + "\n" + format("ECS Direct", direct) + "\n--------------------------------------------------\nIndex / Direct       : " + ratio(index.mMedian, direct.mMedian) + "\nRef / Direct         : " + ratio(localRef.mMedian, direct.mMedian) + "\nIndex / Ref          : " + ratio(index.mMedian, localRef.mMedian) + "\nManaged AoS / Direct : " + ratio(array.mMedian, direct.mMedian) + "\nManaged AoS / Ref    : " + ratio(array.mMedian, localRef.mMedian) + "\n==================================================");
	}
	private string format(string name, BenchmarkResult result)
	{
		return name.PadRight(20) + " Median:" + result.mMedian.ToString("0.000").PadLeft(9) + " ms | Min:" + result.mMin.ToString("0.000").PadLeft(8) + " | Max:" + result.mMax.ToString("0.000").PadLeft(8) + " | " + result.mNsPerEntity.ToString("0.000").PadLeft(8) + " ns/entity";
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
