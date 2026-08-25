using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;
using static FrameBaseUtility;

public static class MouseCastWindowSetBenchmark
{
	private const int TARGET_VISIT_COUNT = 1000000;
	private const int MIN_FRAME_COUNT = 200;
	private const int WARMUP_COUNT = 2;
	private const int SAMPLE_COUNT = 9;
	private static readonly int[] mWindowCounts = { 32, 128, 512, 2048 };
	private sealed class BenchmarkWindow : myUGUIObject
	{
		private readonly Vector2 mBenchmarkSize = new(100.0f, 100.0f);
		public override Vector2 getSize(bool transformed = false) { return mBenchmarkSize; }
		public override bool isActiveInHierarchy() { return true; }
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
		builder.AppendLine("================ MouseCastWindowSet Benchmark Start ================");
		builder.AppendLine("SampleCount:" + SAMPLE_COUNT + " WarmupCount:" + WARMUP_COUNT);
		builder.AppendLine("Current:每帧调用update标脏,随后getWindowOrderList重新筛选并排序");
		builder.AppendLine("Cached :窗口状态不变时直接复用上一次getWindowOrderList结果");
		builder.AppendLine("说明:这里只测试窗口列表重建成本,不包含Collider.Raycast。");
		for (int i = 0; i < mWindowCounts.Length; ++i)
		{
			runCase(builder, mWindowCounts[i]);
		}
		builder.AppendLine("================ MouseCastWindowSet Benchmark End ==================");
		UnityEngine.Debug.Log(builder.ToString());
	}
	private static void runCase(StringBuilder builder, int windowCount)
	{
		GameObject cameraObject = null;
		List<GameObject> windowObjects = new(windowCount);
		List<BenchmarkWindow> windows = new(windowCount);
		try
		{
			GameCamera camera = createCamera(out cameraObject);
			MouseCastWindowSet set = new();
			set.setCamera(camera);
			for (int i = 0; i < windowCount; ++i)
			{
				GameObject go = new("MouseCastBenchmarkWindow_" + i, typeof(RectTransform));
				RectTransform rect = go.GetComponent<RectTransform>();
				rect.position = Vector3.zero;
				windowObjects.Add(go);
				BenchmarkWindow window = new();
				window.setObject(go);
				// 打乱深度顺序,避免排序数据天然有序
				int depth = (i * 97 + 31) % windowCount;
				window.setDepth(null, depth);
				windows.Add(window);
				set.addWindow(window);
			}
			// 先生成一次缓存列表
			set.getWindowOrderList();
			int frameCount = Math.Max(MIN_FRAME_COUNT, TARGET_VISIT_COUNT / windowCount);
			long checksum = 0;
			BenchmarkResult current = measure(() => checksum += runCurrent(set, frameCount));
			BenchmarkResult cached = measure(() => checksum += runCached(set, frameCount));
			builder.AppendLine("---------------- WindowCount:" + windowCount + " FrameCount:" + frameCount + " ----------------");
			appendResult(builder, "Current Dirty+Get", current, frameCount);
			appendResult(builder, "Cached Get", cached, frameCount);
			double ratio = cached.mMedianMS > 0.0 ? current.mMedianMS / cached.mMedianMS : 0.0;
			double saveNS = (current.mMedianMS - cached.mMedianMS) * 1000000.0 / frameCount;
			builder.AppendLine("Current / Cached".PadRight(24) + ": " + ratio.ToString("F2") + "x | Save:" + saveNS.ToString("F2") + " ns/frame");
			builder.AppendLine("Checksum:" + checksum);
		}
		finally
		{
			for (int i = 0; i < windows.Count; ++i)
			{
				myUGUIObject.destroyWindowSingle(windows[i], false);
			}
			for (int i = 0; i < windowObjects.Count; ++i)
			{
				destroyObject(windowObjects[i]);
			}
			destroyObject(cameraObject);
		}
	}
	private static GameCamera createCamera(out GameObject go)
	{
		go = new GameObject("MouseCastBenchmarkCamera");
		Camera unityCamera = go.AddComponent<Camera>();
		unityCamera.orthographic = true;
		unityCamera.orthographicSize = 5.0f;
		unityCamera.transform.position = new(0.0f, 0.0f, -10.0f);
		GameCamera camera = new();
		camera.setObject(go);
		return camera;
	}
	private static long runCurrent(MouseCastWindowSet set, int frameCount)
	{
		long checksum = 0;
		for (int frame = 0; frame < frameCount; ++frame)
		{
			set.update();
			checksum += set.getWindowOrderList().Count;
		}
		return checksum;
	}
	private static long runCached(MouseCastWindowSet set, int frameCount)
	{
		long checksum = 0;
		for (int frame = 0; frame < frameCount; ++frame)
		{
			checksum += set.getWindowOrderList().Count;
		}
		return checksum;
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
	private static void destroyObject(UnityEngine.Object obj)
	{
		if (obj == null)
		{
			return;
		}
		if (isEditor())
		{
			UnityEngine.Object.DestroyImmediate(obj);
		}
		else
		{
			UnityEngine.Object.Destroy(obj);
		}
	}
}
