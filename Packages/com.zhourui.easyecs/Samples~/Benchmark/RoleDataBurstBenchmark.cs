using System;
using System.Diagnostics;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using Debug = UnityEngine.Debug;

public sealed class RoleDataBurstBenchmark : MonoBehaviour
{
	private const int ENTITY_COUNT = 500000;
	private const int SAMPLE_COUNT = 15;
	private const int WARMUP_COUNT = 3;
	private const int INNER_LOOP_BATCH_COUNT = 256;
	private const float DELTA_TIME = 1.0f / 60.0f;
	private static double mResultSink;
	private RoleData_ECSList mECSList;
	private struct BenchmarkResult
	{
		public double mMedian;
		public double mMin;
		public double mMax;
		public double mNsPerEntity;
	}
	[BurstCompile]
	private unsafe struct UpdateSingleJob : IJob
	{
		[NativeDisableUnsafePtrRestriction] public RoleData_ECSList.BurstView mData;
		public float mDeltaTime;
		public void Execute()
		{
			for (int i = 0; i < mData.Count; ++i)
			{
				if (mData.mHP[i] <= 0)
				{
					continue;
				}
				float speed = mData.mSpeed[i];
				float x = mData.mPositionX[i];
				float y = mData.mPositionY[i];
				x += speed * mDeltaTime;
				y += (speed * 0.5f + x * 0.001f) * mDeltaTime;
				mData.mPositionX[i] = x;
				mData.mPositionY[i] = y;
			}
		}
	}
	[BurstCompile]
	private unsafe struct UpdateParallelJob : IJobParallelFor
	{
		[NativeDisableUnsafePtrRestriction] public RoleData_ECSList.BurstView mData;
		public float mDeltaTime;
		public void Execute(int index)
		{
			if (mData.mHP[index] <= 0)
			{
				return;
			}
			float speed = mData.mSpeed[index];
			float x = mData.mPositionX[index];
			float y = mData.mPositionY[index];
			x += speed * mDeltaTime;
			y += (speed * 0.5f + x * 0.001f) * mDeltaTime;
			mData.mPositionX[index] = x;
			mData.mPositionY[index] = y;
		}
	}
	private void Awake()
	{
#if UNITY_EDITOR
		Debug.Log("Unity Editor环境跳过RoleDataBurstBenchmark,请使用Development Player测试Burst性能");
#else
		runBenchmark();
		EasyECSBuiltInBurstParityBenchmark.runBenchmark();
#endif
	}
	private void runBenchmark()
	{
		mECSList = new RoleData_ECSList(ENTITY_COUNT);
		try
		{
			initializeData();
			validateBurstCorrectness();
			Debug.Log("================ EasyECS Burst Benchmark Start ================");
			Debug.Log("Backend:" + RoleData_ECSList.BackendName + ",Reason:" + RoleData_ECSList.BackendReason);
			Debug.Log("BurstEnabled:" + BurstCompiler.IsEnabled);
			Debug.Log("EntityCount:" + ENTITY_COUNT + ",SampleCount:" + SAMPLE_COUNT + ",WarmupCount:" + WARMUP_COUNT + ",BatchCount:" + INNER_LOOP_BATCH_COUNT);
			BenchmarkResult direct = measure(runDirect);
			BenchmarkResult burstSingle = measure(runBurstSingle);
			BenchmarkResult burstParallel = measure(runBurstParallel);
			Debug.Log("\n================ 连续数值更新 ================\n" +
				format("EasyECS Direct C#", direct) + "\n" +
				format("EasyECS Burst IJob", burstSingle) + "\n" +
				format("EasyECS Burst ParallelFor", burstParallel) + "\n" +
				"--------------------------------------------------\n" +
				"Direct / Burst IJob       : " + ratio(direct.mMedian, burstSingle.mMedian) + "x\n" +
				"Direct / Burst Parallel   : " + ratio(direct.mMedian, burstParallel.mMedian) + "x\n" +
				"Burst IJob / Parallel     : " + ratio(burstSingle.mMedian, burstParallel.mMedian) + "x\n" +
				"==================================================");
			mECSList.CompleteBurstJobs();
			var positionX = mECSList.getPositionXColumn();
			var positionY = mECSList.getPositionYColumn();
			mResultSink += positionX[ENTITY_COUNT - 1] + positionY[ENTITY_COUNT - 1];
			Debug.Log("ResultSink:" + mResultSink);
			Debug.Log("================ EasyECS Burst Benchmark End ==================");
		}
		finally
		{
			if (mECSList != null)
			{
				mECSList.Dispose();
				mECSList = null;
			}
		}
	}
	private void initializeData()
	{
		for (int i = 0; i < ENTITY_COUNT; ++i)
		{
			mECSList.Add(new RoleData
			{
				mHP = 100 + (i & 31),
				mSpeed = 1.0f + (i & 15) * 0.125f,
				mPositionX = i * 0.01f,
				mPositionY = i * 0.005f,
				mID = i,
				mModelID = i & 127,
				mCamp = i & 3,
			});
		}
	}
	private void validateBurstCorrectness()
	{
		mECSList.CompleteBurstJobs();
		var positionX = mECSList.getPositionXColumn();
		var positionY = mECSList.getPositionYColumn();
		float oldX = positionX[0];
		float oldY = positionY[0];
		RoleData_ECSList.BurstView view = mECSList.GetBurstView();
		mECSList.ScheduleBurst(new UpdateParallelJob { mData = view, mDeltaTime = DELTA_TIME }, INNER_LOOP_BATCH_COUNT);
		mECSList.CompleteBurstJobs();
		if (positionX[0] == oldX || positionY[0] == oldY)
		{
			throw new InvalidOperationException("Burst Job没有修改EasyECS原始Column");
		}
		int oldCapacity = mECSList.Capacity;
		view = mECSList.GetBurstView();
		mECSList.ScheduleBurst(new UpdateParallelJob { mData = view, mDeltaTime = DELTA_TIME }, INNER_LOOP_BATCH_COUNT);
		mECSList.EnsureCapacity(oldCapacity + 1);
		if (mECSList.Capacity <= oldCapacity)
		{
			throw new InvalidOperationException("Burst生命周期Resize保护测试失败");
		}
		Debug.Log("Burst Correctness Pass:原地Column修改/Job跟踪/Resize自动Complete");
	}
	private void runDirect()
	{
		var hp = mECSList.getHPColumn();
		var speed = mECSList.getSpeedColumn();
		var positionX = mECSList.getPositionXColumn();
		var positionY = mECSList.getPositionYColumn();
		for (int i = 0; i < ENTITY_COUNT; ++i)
		{
			if (hp[i] <= 0)
			{
				continue;
			}
			float curSpeed = speed[i];
			float x = positionX[i];
			float y = positionY[i];
			x += curSpeed * DELTA_TIME;
			y += (curSpeed * 0.5f + x * 0.001f) * DELTA_TIME;
			positionX[i] = x;
			positionY[i] = y;
		}
	}
	private void runBurstSingle()
	{
		RoleData_ECSList.BurstView view = mECSList.GetBurstView();
		JobHandle handle = new UpdateSingleJob { mData = view, mDeltaTime = DELTA_TIME }.Schedule(mECSList.GetBurstDependency());
		mECSList.RegisterBurstJob(handle);
		mECSList.CompleteBurstJobs();
	}
	private void runBurstParallel()
	{
		RoleData_ECSList.BurstView view = mECSList.GetBurstView();
		mECSList.ScheduleBurst(new UpdateParallelJob { mData = view, mDeltaTime = DELTA_TIME }, INNER_LOOP_BATCH_COUNT);
		mECSList.CompleteBurstJobs();
	}
	private BenchmarkResult measure(Action action)
	{
		for (int i = 0; i < WARMUP_COUNT; ++i)
		{
			action();
		}
		double[] samples = new double[SAMPLE_COUNT];
		double min = double.MaxValue;
		double max = 0.0;
		Stopwatch stopwatch = new Stopwatch();
		for (int i = 0; i < SAMPLE_COUNT; ++i)
		{
			stopwatch.Restart();
			action();
			stopwatch.Stop();
			double milliseconds = stopwatch.Elapsed.TotalMilliseconds;
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
		return new BenchmarkResult
		{
			mMedian = median,
			mMin = min,
			mMax = max,
			mNsPerEntity = median * 1000000.0 / ENTITY_COUNT,
		};
	}
	private static string format(string name, BenchmarkResult result)
	{
		return name.PadRight(30) + " Median:" + result.mMedian.ToString("F3").PadLeft(9) + " ms | Min:" + result.mMin.ToString("F3").PadLeft(8) + " | Max:" + result.mMax.ToString("F3").PadLeft(8) + " | " + result.mNsPerEntity.ToString("F3").PadLeft(8) + " ns/entity";
	}
	private static string ratio(double left, double right)
	{
		if (right <= 0.0)
		{
			return "N/A";
		}
		return (left / right).ToString("F2");
	}
}
