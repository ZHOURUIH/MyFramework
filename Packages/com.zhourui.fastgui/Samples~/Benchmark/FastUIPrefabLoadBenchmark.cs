using System;
using System.Collections;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class FastUIPrefabLoadBenchmark
{
	public const string FAST_PREFAB_RESOURCE_PATH = "FastGUIBenchmark/FastGUI_Large";
	public const string UGUI_PREFAB_RESOURCE_PATH = "FastGUIBenchmark/UGUI_Large";
	public const int LARGE_PREFAB_ITEM_COUNT = 500;
	public const int COLD_LOAD_SAMPLE_COUNT = 3;
	public const int INSTANTIATE_SAMPLE_COUNT = 5;
	private struct InstanceSample
	{
		public double mInstantiateMS;
		public double mFirstFlushMS;
		public double mTotalMS;
		public int mRectCount;
		public int mGraphicCount;
		public int mTextCount;
		public int mFastLiveVertices;
		public int mFastIndices;
		public int mFastBatchCount;
		public bool mValid;
	}
	public static IEnumerator run(Action<string> log)
	{
		log ??= _ => { };
		int coldLoadSampleCount = COLD_LOAD_SAMPLE_COUNT;
		int instantiateSampleCount = INSTANTIATE_SAMPLE_COUNT;
		log("[Prefab Benchmark] Begin | LargePrefabItems=" + LARGE_PREFAB_ITEM_COUNT +
			" | ColdLoadSamples=" + coldLoadSampleCount + " | InstantiateSamples=" + instantiateSampleCount);
		GameObject fastProbe = Resources.Load<GameObject>(FAST_PREFAB_RESOURCE_PATH);
		GameObject uguiProbe = Resources.Load<GameObject>(UGUI_PREFAB_RESOURCE_PATH);
		if (fastProbe == null || uguiProbe == null)
		{
			log("[Prefab Benchmark] GeneratedPrefabMissing=True | Fast=" + (fastProbe != null) +
				" | UGUI=" + (uguiProbe != null) +
				" | Hint=Build前会由FastGUIPrefabBenchmarkGenerator自动生成，也可在Tools/FastGUI/Benchmark菜单手工生成");
			yield break;
		}
		bool shapeValid = logPrefabShape(fastProbe, uguiProbe, log);
		bool hierarchyValid = validatePrefabHierarchy(fastProbe, uguiProbe, log);
		if (!shapeValid || !hierarchyValid)
		{
			log("[Prefab Benchmark] LayoutInvalid=True | PerformanceSkipped=True");
			fastProbe = null;
			uguiProbe = null;
			yield return unloadUnused();
			yield break;
		}
		fastProbe = null;
		uguiProbe = null;
		yield return unloadUnused();
		double[] fastColdLoad = new double[coldLoadSampleCount];
		double[] uguiColdLoad = new double[coldLoadSampleCount];
		for (int sample = 0; sample < coldLoadSampleCount; ++sample)
		{
			if ((sample & 1) == 0)
			{
				yield return measureColdLoad(FAST_PREFAB_RESOURCE_PATH, value => fastColdLoad[sample] = value);
				yield return measureColdLoad(UGUI_PREFAB_RESOURCE_PATH, value => uguiColdLoad[sample] = value);
			}
			else
			{
				yield return measureColdLoad(UGUI_PREFAB_RESOURCE_PATH, value => uguiColdLoad[sample] = value);
				yield return measureColdLoad(FAST_PREFAB_RESOURCE_PATH, value => fastColdLoad[sample] = value);
			}
		}
		double fastColdMedian = median(fastColdLoad);
		double uguiColdMedian = median(uguiColdLoad);
		log("[Prefab AssetLoad] Mode=Resources.Load Cold | Fast=" + fastColdMedian.ToString("F4") +
			"ms | UGUI=" + uguiColdMedian.ToString("F4") + "ms | UGUI/Fast=" + ratio(uguiColdMedian, fastColdMedian).ToString("F2") + "x");
		GameObject fastPrefab = Resources.Load<GameObject>(FAST_PREFAB_RESOURCE_PATH);
		GameObject uguiPrefab = Resources.Load<GameObject>(UGUI_PREFAB_RESOURCE_PATH);
		if (fastPrefab == null || uguiPrefab == null)
		{
			log("[Prefab Benchmark] ReloadFailed=True");
			yield break;
		}
		double[] fastInstantiate = new double[instantiateSampleCount];
		double[] uguiInstantiate = new double[instantiateSampleCount];
		double[] fastFlush = new double[instantiateSampleCount];
		double[] uguiFlush = new double[instantiateSampleCount];
		double[] fastTotal = new double[instantiateSampleCount];
		double[] uguiTotal = new double[instantiateSampleCount];
		bool fastValid = true;
		bool uguiValid = true;
		InstanceSample fastLast = default;
		InstanceSample uguiLast = default;
		for (int sample = 0; sample < instantiateSampleCount; ++sample)
		{
			if ((sample & 1) == 0)
			{
				fastLast = measureInstantiate(fastPrefab, true);
				uguiLast = measureInstantiate(uguiPrefab, false);
			}
			else
			{
				uguiLast = measureInstantiate(uguiPrefab, false);
				fastLast = measureInstantiate(fastPrefab, true);
			}
			fastInstantiate[sample] = fastLast.mInstantiateMS;
			uguiInstantiate[sample] = uguiLast.mInstantiateMS;
			fastFlush[sample] = fastLast.mFirstFlushMS;
			uguiFlush[sample] = uguiLast.mFirstFlushMS;
			fastTotal[sample] = fastLast.mTotalMS;
			uguiTotal[sample] = uguiLast.mTotalMS;
			fastValid &= fastLast.mValid;
			uguiValid &= uguiLast.mValid;
			yield return null;
		}
		double fastInstantiateMedian = median(fastInstantiate);
		double uguiInstantiateMedian = median(uguiInstantiate);
		double fastFlushMedian = median(fastFlush);
		double uguiFlushMedian = median(uguiFlush);
		double fastTotalMedian = median(fastTotal);
		double uguiTotalMedian = median(uguiTotal);
		log("[Prefab Instantiate] AssetAlreadyLoaded=True | Fast=" + fastInstantiateMedian.ToString("F4") +
			"ms | UGUI=" + uguiInstantiateMedian.ToString("F4") + "ms | UGUI/Fast=" +
			ratio(uguiInstantiateMedian, fastInstantiateMedian).ToString("F2") + "x");
		log("[Prefab FirstFullFlush] FastCanvas.flushFrameNow/Canvas.ForceUpdateCanvases | Fast=" + fastFlushMedian.ToString("F4") +
			"ms | UGUI=" + uguiFlushMedian.ToString("F4") + "ms | UGUI/Fast=" +
			ratio(uguiFlushMedian, fastFlushMedian).ToString("F2") + "x");
		log("[Prefab InstantiateToReady] Fast=" + fastTotalMedian.ToString("F4") +
			"ms | UGUI=" + uguiTotalMedian.ToString("F4") + "ms | UGUI/Fast=" +
			ratio(uguiTotalMedian, fastTotalMedian).ToString("F2") + "x | FastValid=" + fastValid + " | UGUIValid=" + uguiValid +
			" | FastLiveVertices=" + fastLast.mFastLiveVertices + " | FastIndices=" + fastLast.mFastIndices +
			" | FastBatches=" + fastLast.mFastBatchCount);
		log("[Prefab LoadToReady] ColdAssetLoad+WarmInstantiate+FirstFullFlush | Fast=" +
			(fastColdMedian + fastTotalMedian).ToString("F4") + "ms | UGUI=" +
			(uguiColdMedian + uguiTotalMedian).ToString("F4") + "ms | UGUI/Fast=" +
			ratio(uguiColdMedian + uguiTotalMedian, fastColdMedian + fastTotalMedian).ToString("F2") + "x");
		fastPrefab = null;
		uguiPrefab = null;
		yield return unloadUnused();
		log("[Prefab Benchmark] Complete");
	}
	private static IEnumerator measureColdLoad(string resourcePath, Action<double> completed)
	{
		yield return unloadUnused();
		long start = Stopwatch.GetTimestamp();
		GameObject prefab = Resources.Load<GameObject>(resourcePath);
		double elapsed = (Stopwatch.GetTimestamp() - start) * 1000.0 / Stopwatch.Frequency;
		completed?.Invoke(prefab != null ? elapsed : double.PositiveInfinity);
		prefab = null;
		yield return unloadUnused();
	}
	private static IEnumerator unloadUnused()
	{
		AsyncOperation operation = Resources.UnloadUnusedAssets();
		if (operation != null)
		{
			yield return operation;
		}
		GC.Collect();
		GC.WaitForPendingFinalizers();
	}
	private static InstanceSample measureInstantiate(GameObject prefab, bool fast)
	{
		InstanceSample result = default;
		long instantiateStart = Stopwatch.GetTimestamp();
		GameObject instance = UnityEngine.Object.Instantiate(prefab);
		result.mInstantiateMS = (Stopwatch.GetTimestamp() - instantiateStart) * 1000.0 / Stopwatch.Frequency;
		result.mRectCount = instance.GetComponentsInChildren<RectTransform>(true).Length;
		if (fast)
		{
			result.mGraphicCount = instance.GetComponentsInChildren<FastImage>(true).Length;
			result.mTextCount = instance.GetComponentsInChildren<FastText>(true).Length;
			FastCanvas canvas = instance.GetComponent<FastCanvas>();
			long flushStart = Stopwatch.GetTimestamp();
			FastUIFrameStats stats = canvas != null ? canvas.flushFrameNow() : default;
			result.mFirstFlushMS = (Stopwatch.GetTimestamp() - flushStart) * 1000.0 / Stopwatch.Frequency;
			FastUIMeshRenderer renderer = canvas != null ? canvas.getMeshRenderer() : null;
			result.mFastLiveVertices = renderer != null ? renderer.getLiveVertexCount() : 0;
			result.mFastIndices = renderer != null ? renderer.getCurrentIndexCount() : 0;
			result.mFastBatchCount = renderer != null ? renderer.getBatchCount() : 0;
			result.mValid = canvas != null && stats.mCPUTimeMS >= 0.0 && result.mFastLiveVertices > 4 &&
				result.mFastIndices > 6 && result.mFastBatchCount > 0;
		}
		else
		{
			result.mGraphicCount = instance.GetComponentsInChildren<Image>(true).Length;
			result.mTextCount = instance.GetComponentsInChildren<TextMeshProUGUI>(true).Length;
			long flushStart = Stopwatch.GetTimestamp();
			Canvas.ForceUpdateCanvases();
			result.mFirstFlushMS = (Stopwatch.GetTimestamp() - flushStart) * 1000.0 / Stopwatch.Frequency;
			result.mValid = instance.GetComponent<Canvas>() != null && result.mGraphicCount > 0 && result.mTextCount > 0;
		}
		result.mTotalMS = result.mInstantiateMS + result.mFirstFlushMS;
		instance.SetActive(false);
		UnityEngine.Object.Destroy(instance);
		return result;
	}
	private static bool logPrefabShape(GameObject fastPrefab, GameObject uguiPrefab, Action<string> log)
	{
		int fastRects = fastPrefab.GetComponentsInChildren<RectTransform>(true).Length;
		int uguiRects = uguiPrefab.GetComponentsInChildren<RectTransform>(true).Length;
		int fastImages = fastPrefab.GetComponentsInChildren<FastImage>(true).Length;
		int uguiImages = uguiPrefab.GetComponentsInChildren<Image>(true).Length;
		int fastTexts = fastPrefab.GetComponentsInChildren<FastText>(true).Length;
		int uguiTexts = uguiPrefab.GetComponentsInChildren<TextMeshProUGUI>(true).Length;
		int fastRawImages = fastPrefab.GetComponentsInChildren<FastRawImage>(true).Length;
		int uguiRawImages = uguiPrefab.GetComponentsInChildren<RawImage>(true).Length;
		bool sameRect = fastRects == uguiRects;
		bool sameImages = fastImages == uguiImages;
		bool sameTexts = fastTexts == uguiTexts;
		bool normalSpriteUsage = fastImages > 0 && uguiImages > 0 && fastRawImages == 0 && uguiRawImages == 0;
		log("[Prefab Shape] Fast Rects=" + fastRects + "/FastImage=" + fastImages + "/RawImage=" + fastRawImages + "/Texts=" + fastTexts +
			" | UGUI Rects=" + uguiRects + "/Image=" + uguiImages + "/RawImage=" + uguiRawImages + "/Texts=" + uguiTexts +
			" | SameRect=" + sameRect + " | SameImages=" + sameImages + " | SameTexts=" + sameTexts +
			" | NormalSpriteUsage=" + normalSpriteUsage);
		return sameRect && sameImages && sameTexts && normalSpriteUsage;
	}
	private static bool validatePrefabHierarchy(GameObject fastPrefab, GameObject uguiPrefab, Action<string> log)
	{
		Transform fastContent = fastPrefab != null ? fastPrefab.transform.Find("Content") : null;
		Transform uguiContent = uguiPrefab != null ? uguiPrefab.transform.Find("Content") : null;
		bool hierarchyMatch = fastContent != null && uguiContent != null && compareHierarchy(fastContent, uguiContent);
		bool fastBackgroundFirst = true;
		bool uguiBackgroundFirst = true;
		if (fastContent == null || uguiContent == null)
		{
			fastBackgroundFirst = false;
			uguiBackgroundFirst = false;
		}
		else
		{
			for (int i = 0; i < LARGE_PREFAB_ITEM_COUNT; ++i)
			{
				fastBackgroundFirst &= isBackgroundFirst(fastContent.Find("Item_" + i));
				uguiBackgroundFirst &= isBackgroundFirst(uguiContent.Find("Item_" + i));
			}
		}
		bool valid = hierarchyMatch && fastBackgroundFirst && uguiBackgroundFirst;
		log("[Prefab Hierarchy] Match=" + hierarchyMatch +
			" | BackgroundFirst F/U:" + fastBackgroundFirst + "/" + uguiBackgroundFirst +
			" | CheckedItems=" + LARGE_PREFAB_ITEM_COUNT + " | Valid=" + valid);
		return valid;
	}
	private static bool compareHierarchy(Transform fast, Transform ugui)
	{
		if (fast == null || ugui == null || fast.childCount != ugui.childCount)
		{
			return false;
		}
		for (int i = 0; i < fast.childCount; ++i)
		{
			Transform fastChild = fast.GetChild(i);
			Transform uguiChild = ugui.GetChild(i);
			if (fastChild.name != uguiChild.name || !compareHierarchy(fastChild, uguiChild))
			{
				return false;
			}
		}
		return true;
	}
	private static bool isBackgroundFirst(Transform item)
	{
		return item != null && item.childCount > 0 && item.GetChild(0).name == "Background";
	}

	private static double median(double[] values)
	{
		if (values == null || values.Length == 0)
		{
			return 0.0;
		}
		double[] copy = new double[values.Length];
		Array.Copy(values, copy, values.Length);
		Array.Sort(copy);
		int middle = copy.Length >> 1;
		if ((copy.Length & 1) != 0)
		{
			return copy[middle];
		}
		return (copy[middle - 1] + copy[middle]) * 0.5;
	}
	private static double ratio(double numerator, double denominator)
	{
		return denominator > 0.0000001 ? numerator / denominator : 0.0;
	}
}
