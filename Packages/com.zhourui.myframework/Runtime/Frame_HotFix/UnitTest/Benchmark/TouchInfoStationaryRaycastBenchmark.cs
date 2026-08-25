using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;
using static FrameBaseUtility;

public static class TouchInfoStationaryRaycastBenchmark
{
	private const int TARGET_RAYCAST_COUNT = 1000000;
	private const int MIN_FRAME_COUNT = 200;
	private const int WARMUP_COUNT = 2;
	private const int SAMPLE_COUNT = 9;
	private static readonly int[] mWindowCounts = { 32, 128, 512, 2048 };
	private enum TEST_MODE
	{
		ALL_MISS,
		ALL_PASS,
		FIRST_BLOCK,
	}
	private sealed class BenchmarkGlobalTouchSystem : GlobalTouchSystem
	{
		public void raycast(List<IMouseEventCollect> windowList, Ray ray, List<IMouseEventCollect> resultList)
		{
			bool continueRay = true;
			raycastLayout(ray, windowList, resultList, ref continueRay, true);
		}
	}
	private sealed class BenchmarkMouseObject : IMouseEventCollect
	{
		private readonly GameObject mObject;
		private readonly Collider mCollider;
		private readonly bool mPassRay;
		public BenchmarkMouseObject(GameObject obj, Collider collider, bool passRay)
		{
			mObject = obj;
			mCollider = collider;
			mPassRay = passRay;
		}
		public string getName() { return mObject.name; }
		public string getDescription() { return mObject.name; }
		public bool isDestroy() { return mObject == null; }
		public bool isActiveInHierarchy() { return mObject.activeInHierarchy; }
		public bool isHandleInput() { return true; }
		public void onTouchLeave(Vector3 touchPos, int touchID) { }
		public void onTouchEnter(Vector3 touchPos, int touchID) { }
		public void onTouchMove(Vector3 touchPos, Vector3 moveDelta, float moveTime, int touchID) { }
		public void onTouchStay(Vector3 touchPos, int touchID) { }
		public Collider getCollider(bool addIfNotExist = false) { return mCollider; }
		public UIDepth getDepth() { return null; }
		public bool isReceiveScreenTouch() { return false; }
		public void onScreenTouchDown(Vector3 touchPos, int touchID) { }
		public void onScreenTouchUp(Vector3 touchPos, int touchID) { }
		public void onTouchDown(Vector3 touchPos, int touchID) { }
		public void onTouchUp(Vector3 touchPos, int touchID) { }
		public bool isPassRay() { return mPassRay; }
		public bool isPassDragEvent() { return false; }
		public void onReceiveDrag(IMouseEventCollect dragObj, Vector3 touchPos, ref bool continueEvent) { }
		public bool isDraggable() { return false; }
		public bool isChildOf(IMouseEventCollect parent) { return false; }
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
		builder.AppendLine("================ TouchInfo Stationary Raycast Benchmark Start ================");
		builder.AppendLine("SampleCount:" + SAMPLE_COUNT + " WarmupCount:" + WARMUP_COUNT);
		builder.AppendLine("Current:模拟TouchInfo.update中每帧重新Raycast、构建新Hover集合、比较Enter/Leave并覆盖旧Hover集合");
		builder.AppendLine("Cached :模拟鼠标位置和输入场景版本都未变化时直接复用上一帧Hover结果");
		builder.AppendLine("ALL_MISS   : 所有Collider都未命中,需要扫描全部窗口");
		builder.AppendLine("ALL_PASS   : 所有Collider都命中且允许穿透,需要扫描并收集全部窗口");
		builder.AppendLine("FIRST_BLOCK: 第一个Collider命中且不允许穿透,理论上的最理想情况");
		for (int i = 0; i < mWindowCounts.Length; ++i)
		{
			runCase(builder, mWindowCounts[i], TEST_MODE.ALL_MISS);
			runCase(builder, mWindowCounts[i], TEST_MODE.ALL_PASS);
			runCase(builder, mWindowCounts[i], TEST_MODE.FIRST_BLOCK);
		}
		builder.AppendLine("================ TouchInfo Stationary Raycast Benchmark End ==================");
		UnityEngine.Debug.Log(builder.ToString());
	}
	private static void runCase(StringBuilder builder, int windowCount, TEST_MODE mode)
	{
		BenchmarkGlobalTouchSystem system = new();
		List<GameObject> gameObjects = new(windowCount);
		List<IMouseEventCollect> windows = new(windowCount);
		List<IMouseEventCollect> raycastResult = new(windowCount);
		HashSet<IMouseEventCollect> hoverList = new();
		HashSet<IMouseEventCollect> newHoverList = new();
		Ray ray = new(new Vector3(0.0f, 0.0f, -10.0f), Vector3.forward);
		try
		{
			createWindows(windowCount, mode, gameObjects, windows);
			Physics.SyncTransforms();
			// 预热一次Hover状态,让Current和Cached都从稳定状态开始
			refreshHover(system, windows, ray, raycastResult, hoverList, newHoverList);
			int frameCount = Math.Max(MIN_FRAME_COUNT, TARGET_RAYCAST_COUNT / windowCount);
			long checksum = 0;
			BenchmarkResult current = measure(() => checksum += runCurrent(system, windows, ray, raycastResult, hoverList, newHoverList, frameCount));
			BenchmarkResult cached = measure(() => checksum += runCached(hoverList, frameCount));
			builder.AppendLine("---------------- Mode:" + mode + " WindowCount:" + windowCount + " FrameCount:" + frameCount + " ----------------");
			appendResult(builder, "Current HoverUpdate", current, frameCount);
			appendResult(builder, "Cached Hover", cached, frameCount);
			double ratio = cached.mMedianMS > 0.0 ? current.mMedianMS / cached.mMedianMS : 0.0;
			double saveNS = (current.mMedianMS - cached.mMedianMS) * 1000000.0 / frameCount;
			builder.AppendLine("Current / Cached".PadRight(24) + ": " + ratio.ToString("F2") + "x | Save:" + saveNS.ToString("F2") + " ns/frame");
			builder.AppendLine("HoverCount:" + hoverList.Count + " Checksum:" + checksum);
		}
		finally
		{
			for (int i = 0; i < gameObjects.Count; ++i)
			{
				destroyObject(gameObjects[i]);
			}
		}
	}
	private static void createWindows(int windowCount, TEST_MODE mode, List<GameObject> gameObjects, List<IMouseEventCollect> windows)
	{
		for (int i = 0; i < windowCount; ++i)
		{
			GameObject go = new("StationaryRaycastBenchmark_" + mode + "_" + i);
			BoxCollider collider = go.AddComponent<BoxCollider>();
			collider.size = new Vector3(2.0f, 2.0f, 0.2f);
			bool hit = mode != TEST_MODE.ALL_MISS;
			go.transform.position = hit ? Vector3.zero : new Vector3(100.0f + i, 100.0f, 0.0f);
			bool passRay = mode != TEST_MODE.FIRST_BLOCK;
			BenchmarkMouseObject obj = new(go, collider, passRay);
			gameObjects.Add(go);
			windows.Add(obj);
		}
	}
	private static long runCurrent(BenchmarkGlobalTouchSystem system, List<IMouseEventCollect> windows, Ray ray,
		List<IMouseEventCollect> raycastResult, HashSet<IMouseEventCollect> hoverList, HashSet<IMouseEventCollect> newHoverList, int frameCount)
	{
		long checksum = 0;
		for (int frame = 0; frame < frameCount; ++frame)
		{
			checksum += refreshHover(system, windows, ray, raycastResult, hoverList, newHoverList);
		}
		return checksum;
	}
	private static long runCached(HashSet<IMouseEventCollect> hoverList, int frameCount)
	{
		long checksum = 0;
		int lastInputVersion = 1;
		int currentInputVersion = 1;
		Vector3 moveDelta = Vector3.zero;
		for (int frame = 0; frame < frameCount; ++frame)
		{
			// 模拟正式优化后最基本的缓存判定:鼠标没移动且输入场景版本没变化
			if (moveDelta.isZero() && lastInputVersion == currentInputVersion)
			{
				checksum += hoverList.Count;
				continue;
			}
			lastInputVersion = currentInputVersion;
		}
		return checksum;
	}
	private static int refreshHover(BenchmarkGlobalTouchSystem system, List<IMouseEventCollect> windows, Ray ray,
		List<IMouseEventCollect> raycastResult, HashSet<IMouseEventCollect> hoverList, HashSet<IMouseEventCollect> newHoverList)
	{
		system.raycast(windows, ray, raycastResult);
		newHoverList.Clear();
		for (int i = 0; i < raycastResult.Count; ++i)
		{
			newHoverList.Add(raycastResult[i]);
		}
		int changedCount = 0;
		foreach (IMouseEventCollect item in hoverList)
		{
			if (!newHoverList.Contains(item) && item.isActiveInHierarchy() && item.isHandleInput())
			{
				++changedCount;
			}
		}
		foreach (IMouseEventCollect item in newHoverList)
		{
			if (!hoverList.Contains(item) && item.isActiveInHierarchy() && item.isHandleInput())
			{
				++changedCount;
			}
		}
		hoverList.Clear();
		foreach (IMouseEventCollect item in newHoverList)
		{
			hoverList.Add(item);
		}
		return hoverList.Count + changedCount;
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
