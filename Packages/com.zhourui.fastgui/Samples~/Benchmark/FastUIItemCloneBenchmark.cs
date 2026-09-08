using System;
using System.Collections;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public static class FastUIItemCloneBenchmark
{
	private const int CLONE_COUNT = 2000;
	private const int SAMPLE_COUNT = 3;
	private static readonly double TICK_TO_MS = 1000.0 / Stopwatch.Frequency;

	private struct Sample
	{
		public double mInstantiateMS;
		public double mFirstFlushMS;
		public double mTotalReadyMS;
		public int mBulkRegisterCount;
		public int mBulkRegisterFlushCount;
		public bool mValid;
	}

	public static IEnumerator run(Action<object> log, Transform host, Texture textureA, Texture textureB, TMP_FontAsset font, Material imageMaterial)
	{
		log ??= _ => { };
		if (host == null || font == null)
		{
			log("[Clone Benchmark] Skipped=True | Reason=MissingHostOrFont");
			yield break;
		}

		GameObject templateHost = createInactiveTemplateHost("CloneBenchmarkTemplates");
		RectTransform fastTemplate = createFastItemTemplate((RectTransform)templateHost.transform, textureA, textureB, font);
		RectTransform uguiTemplate = createUGUIItemTemplate((RectTransform)templateHost.transform, textureA, textureB, font, imageMaterial);
		log("[Clone Benchmark] Begin | Items=" + CLONE_COUNT + " | Samples=" + SAMPLE_COUNT +
			" | Modes=FastGUI Object.Instantiate, UGUI Object.Instantiate, FastUICloneUtility");

		Sample[] fastObject = new Sample[SAMPLE_COUNT];
		Sample[] uguiObject = new Sample[SAMPLE_COUNT];
		Sample[] fastUtility = new Sample[SAMPLE_COUNT];

		for (int sample = 0; sample < SAMPLE_COUNT; ++sample)
		{
			collectGC();
			if ((sample & 1) == 0)
			{
				yield return measureFastObject(host, fastTemplate.gameObject, imageMaterial, value => fastObject[sample] = value);
				yield return measureUGUIObject(host, uguiTemplate.gameObject, value => uguiObject[sample] = value);
				yield return measureFastUtility(host, fastTemplate.gameObject, imageMaterial, value => fastUtility[sample] = value);
			}
			else
			{
				yield return measureFastUtility(host, fastTemplate.gameObject, imageMaterial, value => fastUtility[sample] = value);
				yield return measureUGUIObject(host, uguiTemplate.gameObject, value => uguiObject[sample] = value);
				yield return measureFastObject(host, fastTemplate.gameObject, imageMaterial, value => fastObject[sample] = value);
			}
		}

		double fastObjectInstantiate = median(fastObject, x => x.mInstantiateMS);
		double fastObjectFlush = median(fastObject, x => x.mFirstFlushMS);
		double fastObjectReady = median(fastObject, x => x.mTotalReadyMS);
		double uguiInstantiate = median(uguiObject, x => x.mInstantiateMS);
		double uguiFlush = median(uguiObject, x => x.mFirstFlushMS);
		double uguiReady = median(uguiObject, x => x.mTotalReadyMS);
		double utilityInstantiate = median(fastUtility, x => x.mInstantiateMS);
		double utilityFlush = median(fastUtility, x => x.mFirstFlushMS);
		double utilityReady = median(fastUtility, x => x.mTotalReadyMS);
		int bulkRegisterCount = medianInt(fastUtility, x => x.mBulkRegisterCount);
		int bulkFlushCount = medianInt(fastUtility, x => x.mBulkRegisterFlushCount);

		log("[Clone Compare] Mode=FastGUI Object.Instantiate | Items=" + CLONE_COUNT +
			" | Instantiate=" + fastObjectInstantiate.ToString("F4") + "ms | FirstFlush=" + fastObjectFlush.ToString("F4") +
			"ms | TotalReady=" + fastObjectReady.ToString("F4") + "ms | Valid=" + allValid(fastObject));
		log("[Clone Compare] Mode=UGUI Object.Instantiate | Items=" + CLONE_COUNT +
			" | Instantiate=" + uguiInstantiate.ToString("F4") + "ms | FirstFlush=" + uguiFlush.ToString("F4") +
			"ms | TotalReady=" + uguiReady.ToString("F4") + "ms | Valid=" + allValid(uguiObject));
		log("[Clone Utility] Mode=FastUICloneUtility | Items=" + CLONE_COUNT +
			" | Instantiate=" + utilityInstantiate.ToString("F4") + "ms | FirstFlush=" + utilityFlush.ToString("F4") +
			"ms | TotalReady=" + utilityReady.ToString("F4") + "ms | BulkRegister=" + bulkRegisterCount +
			" | BulkFlush=" + bulkFlushCount + " | Valid=" + allValid(fastUtility));
		log("[Clone Summary] UGUIReady/FastObjectReady=" + ratio(uguiReady, fastObjectReady).ToString("F2") +
			"x | UGUIReady/FastCloneUtilityReady=" + ratio(uguiReady, utilityReady).ToString("F2") +
			"x | FastObjectReady/FastCloneUtilityReady=" + ratio(fastObjectReady, utilityReady).ToString("F2") + "x");

		UnityEngine.Object.Destroy(templateHost);
		yield return new WaitForEndOfFrame();
		collectGC();
		log("[Clone Benchmark] Complete");
	}

	private static IEnumerator measureFastObject(Transform host, GameObject template, Material imageMaterial, Action<Sample> completed)
	{
		GameObject root = createFastCanvasRoot(host, imageMaterial, out FastCanvas canvas, out RectTransform content);
		long start = Stopwatch.GetTimestamp();
		for (int i = 0; i < CLONE_COUNT; ++i)
		{
			UnityEngine.Object.Instantiate(template, content, false);
		}
		double instantiateMS = elapsedMS(start);
		long flushStart = Stopwatch.GetTimestamp();
		FastUIFrameStats stats = canvas.flushFrameNow();
		double flushMS = elapsedMS(flushStart);
		FastUIMeshRenderer renderer = canvas.getMeshRenderer();
		Sample result = new()
		{
			mInstantiateMS = instantiateMS,
			mFirstFlushMS = flushMS,
			mTotalReadyMS = instantiateMS + flushMS,
			mValid = content.childCount == CLONE_COUNT && renderer != null && renderer.getLiveVertexCount() > 0 &&
				renderer.getCurrentIndexCount() > 0 && stats.mCPUTimeMS >= 0.0,
		};
		completed(result);
		UnityEngine.Object.Destroy(root);
		yield return new WaitForEndOfFrame();
	}

	private static IEnumerator measureUGUIObject(Transform host, GameObject template, Action<Sample> completed)
	{
		GameObject root = createUGUICanvasRoot(host, out RectTransform content);
		long start = Stopwatch.GetTimestamp();
		for (int i = 0; i < CLONE_COUNT; ++i)
		{
			UnityEngine.Object.Instantiate(template, content, false);
		}
		double instantiateMS = elapsedMS(start);
		long flushStart = Stopwatch.GetTimestamp();
		Canvas.ForceUpdateCanvases();
		double flushMS = elapsedMS(flushStart);
		Sample result = new()
		{
			mInstantiateMS = instantiateMS,
			mFirstFlushMS = flushMS,
			mTotalReadyMS = instantiateMS + flushMS,
			mValid = content.childCount == CLONE_COUNT &&
				content.GetComponentsInChildren<RawImage>(true).Length == CLONE_COUNT * 6 &&
				content.GetComponentsInChildren<TextMeshProUGUI>(true).Length == CLONE_COUNT * 2,
		};
		completed(result);
		UnityEngine.Object.Destroy(root);
		yield return new WaitForEndOfFrame();
	}

	private static IEnumerator measureFastUtility(Transform host, GameObject template, Material imageMaterial, Action<Sample> completed)
	{
		GameObject root = createFastCanvasRoot(host, imageMaterial, out FastCanvas canvas, out RectTransform content);
		long start = Stopwatch.GetTimestamp();
		GameObject[] clones = FastUICloneUtility.instantiate(template, content, CLONE_COUNT);
		double instantiateMS = elapsedMS(start);
		int bulkRegisterCount = FastUICloneUtility.getLastBulkRegisterCount();
		int bulkFlushCount = FastUICloneUtility.getLastBulkRegisterFlushCount();
		long flushStart = Stopwatch.GetTimestamp();
		FastUIFrameStats stats = canvas.flushFrameNow();
		double flushMS = elapsedMS(flushStart);
		FastUIMeshRenderer renderer = canvas.getMeshRenderer();
		Sample result = new()
		{
			mInstantiateMS = instantiateMS,
			mFirstFlushMS = flushMS,
			mTotalReadyMS = instantiateMS + flushMS,
			mBulkRegisterCount = bulkRegisterCount,
			mBulkRegisterFlushCount = bulkFlushCount,
			mValid = clones != null && clones.Length == CLONE_COUNT && content.childCount == CLONE_COUNT &&
				renderer != null && renderer.getLiveVertexCount() > 0 && renderer.getCurrentIndexCount() > 0 &&
				stats.mCPUTimeMS >= 0.0,
		};
		completed(result);
		UnityEngine.Object.Destroy(root);
		yield return new WaitForEndOfFrame();
	}

	private static GameObject createFastCanvasRoot(Transform host, Material imageMaterial, out FastCanvas canvas, out RectTransform content)
	{
		GameObject root = new("FastCloneCanvas", typeof(RectTransform));
		RectTransform rootRect = (RectTransform)root.transform;
		rootRect.SetParent(host, false);
		setupRect(rootRect, new Vector2(4096.0f, 4096.0f), new Vector3(40000.0f, 40000.0f, 0.0f));
		canvas = root.AddComponent<FastCanvas>();
		if (imageMaterial != null) { canvas.setDefaultMaterial(imageMaterial); }
		content = createRect("Content", rootRect, new Vector2(4096.0f, 4096.0f), Vector3.zero);
		return root;
	}

	private static GameObject createUGUICanvasRoot(Transform host, out RectTransform content)
	{
		GameObject root = new("UGUICloneCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
		RectTransform rootRect = (RectTransform)root.transform;
		rootRect.SetParent(host, false);
		setupRect(rootRect, new Vector2(4096.0f, 4096.0f), new Vector3(40000.0f, 40000.0f, 0.0f));
		Canvas canvas = root.GetComponent<Canvas>();
		canvas.renderMode = RenderMode.ScreenSpaceOverlay;
		root.GetComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
		content = createRect("Content", rootRect, new Vector2(4096.0f, 4096.0f), Vector3.zero);
		return root;
	}

	private static GameObject createInactiveTemplateHost(string name)
	{
		GameObject root = new(name, typeof(RectTransform));
		root.SetActive(false);
		setupRect((RectTransform)root.transform, new Vector2(100.0f, 100.0f), Vector3.zero);
		return root;
	}

	private static RectTransform createFastItemTemplate(RectTransform parent, Texture textureA, Texture textureB, TMP_FontAsset font)
	{
		RectTransform root = createRect("ItemTemplate", parent, new Vector2(78.0f, 78.0f), Vector3.zero);
		root.gameObject.AddComponent<FastUIVisibility>();
		createFastRawImage("Background", root, new Vector2(78.0f, 78.0f), Vector3.zero, textureA, new Color(0.16f, 0.17f, 0.20f, 1.0f));
		RectTransform content = createRect("ContentLayer", root, new Vector2(78.0f, 78.0f), Vector3.zero);
		createFastRawImage("Icon", content, new Vector2(54.0f, 54.0f), new Vector3(0.0f, 4.0f, 0.0f), textureA, Color.white);
		createFastRawImage("QualityFrame", content, new Vector2(62.0f, 62.0f), new Vector3(0.0f, 4.0f, 0.0f), textureA, new Color(0.45f, 0.75f, 1.0f, 1.0f));
		RectTransform state = createRect("StateLayer", content, new Vector2(78.0f, 78.0f), Vector3.zero);
		createFastRawImage("Bind", state, new Vector2(15.0f, 15.0f), new Vector3(-24.0f, 24.0f, 0.0f), textureB, new Color(0.4f, 0.8f, 1.0f, 1.0f));
		createFastRawImage("Lock", state, new Vector2(15.0f, 15.0f), new Vector3(24.0f, 24.0f, 0.0f), textureB, new Color(1.0f, 0.65f, 0.2f, 1.0f));
		RectTransform textLayer = createRect("TextLayer", content, new Vector2(78.0f, 78.0f), Vector3.zero);
		createFastText("Count", textLayer, new Vector2(42.0f, 18.0f), new Vector3(15.0f, -25.0f, 0.0f), "0", 15.0f, FastUITextHorizontalAlignment.Right, font);
		createFastText("Level", textLayer, new Vector2(30.0f, 18.0f), new Vector3(-20.0f, -25.0f, 0.0f), "1", 14.0f, FastUITextHorizontalAlignment.Left, font);
		FastRawImage selected = createFastRawImage("Selected", root, new Vector2(68.0f, 68.0f), Vector3.zero, textureA, new Color(1.0f, 0.85f, 0.2f, 0.32f));
		selected.setVisible(false);
		return root;
	}

	private static RectTransform createUGUIItemTemplate(RectTransform parent, Texture textureA, Texture textureB, TMP_FontAsset font, Material imageMaterial)
	{
		RectTransform root = createRect("ItemTemplate", parent, new Vector2(78.0f, 78.0f), Vector3.zero);
		createUGUIRawImage("Background", root, new Vector2(78.0f, 78.0f), Vector3.zero, textureA, new Color(0.16f, 0.17f, 0.20f, 1.0f), imageMaterial);
		RectTransform content = createRect("ContentLayer", root, new Vector2(78.0f, 78.0f), Vector3.zero);
		createUGUIRawImage("Icon", content, new Vector2(54.0f, 54.0f), new Vector3(0.0f, 4.0f, 0.0f), textureA, Color.white, imageMaterial);
		createUGUIRawImage("QualityFrame", content, new Vector2(62.0f, 62.0f), new Vector3(0.0f, 4.0f, 0.0f), textureA, new Color(0.45f, 0.75f, 1.0f, 1.0f), imageMaterial);
		RectTransform state = createRect("StateLayer", content, new Vector2(78.0f, 78.0f), Vector3.zero);
		createUGUIRawImage("Bind", state, new Vector2(15.0f, 15.0f), new Vector3(-24.0f, 24.0f, 0.0f), textureB, new Color(0.4f, 0.8f, 1.0f, 1.0f), imageMaterial);
		createUGUIRawImage("Lock", state, new Vector2(15.0f, 15.0f), new Vector3(24.0f, 24.0f, 0.0f), textureB, new Color(1.0f, 0.65f, 0.2f, 1.0f), imageMaterial);
		RectTransform textLayer = createRect("TextLayer", content, new Vector2(78.0f, 78.0f), Vector3.zero);
		createUGUIText("Count", textLayer, new Vector2(42.0f, 18.0f), new Vector3(15.0f, -25.0f, 0.0f), "0", 15.0f, TextAlignmentOptions.Right, font);
		createUGUIText("Level", textLayer, new Vector2(30.0f, 18.0f), new Vector3(-20.0f, -25.0f, 0.0f), "1", 14.0f, TextAlignmentOptions.Left, font);
		RawImage selected = createUGUIRawImage("Selected", root, new Vector2(68.0f, 68.0f), Vector3.zero, textureA, new Color(1.0f, 0.85f, 0.2f, 0.32f), imageMaterial);
		selected.enabled = false;
		return root;
	}

	private static FastRawImage createFastRawImage(string name, RectTransform parent, Vector2 size, Vector3 position, Texture texture, Color color)
	{
		RectTransform rect = createRect(name, parent, size, position);
		FastRawImage image = rect.gameObject.AddComponent<FastRawImage>();
		image.setTexture(texture);
		image.setColor(color);
		return image;
	}

	private static FastText createFastText(string name, RectTransform parent, Vector2 size, Vector3 position, string text, float fontSize, FastUITextHorizontalAlignment alignment, TMP_FontAsset font)
	{
		RectTransform rect = createRect(name, parent, size, position);
		FastText label = rect.gameObject.AddComponent<FastText>();
		label.setFont(font);
		label.setFontSize(fontSize);
		label.setText(text);
		label.setWordWrap(false);
		label.setHorizontalAlignment(alignment);
		label.setVerticalAlignment(FastUITextVerticalAlignment.Middle);
		return label;
	}

	private static RawImage createUGUIRawImage(string name, RectTransform parent, Vector2 size, Vector3 position, Texture texture, Color color, Material imageMaterial)
	{
		RectTransform rect = createRect(name, parent, size, position);
		RawImage image = rect.gameObject.AddComponent<RawImage>();
		image.texture = texture;
		image.color = color;
		image.raycastTarget = false;
		if (imageMaterial != null) { image.material = imageMaterial; }
		return image;
	}

	private static TextMeshProUGUI createUGUIText(string name, RectTransform parent, Vector2 size, Vector3 position, string text, float fontSize, TextAlignmentOptions alignment, TMP_FontAsset font)
	{
		RectTransform rect = createRect(name, parent, size, position);
		TextMeshProUGUI label = rect.gameObject.AddComponent<TextMeshProUGUI>();
		label.font = font;
		label.fontSize = fontSize;
		label.text = text;
		label.alignment = alignment;
		label.raycastTarget = false;
		label.richText = true;
		label.overflowMode = TextOverflowModes.Overflow;
		return label;
	}

	private static RectTransform createRect(string name, Transform parent, Vector2 size, Vector3 position)
	{
		GameObject gameObject = new(name, typeof(RectTransform));
		RectTransform rect = (RectTransform)gameObject.transform;
		rect.SetParent(parent, false);
		setupRect(rect, size, position);
		return rect;
	}

	private static void setupRect(RectTransform rect, Vector2 size, Vector3 position)
	{
		rect.anchorMin = new Vector2(0.5f, 0.5f);
		rect.anchorMax = new Vector2(0.5f, 0.5f);
		rect.pivot = new Vector2(0.5f, 0.5f);
		rect.sizeDelta = size;
		rect.localPosition = position;
		rect.localScale = Vector3.one;
		rect.localRotation = Quaternion.identity;
	}

	private static void collectGC()
	{
		GC.Collect();
		GC.WaitForPendingFinalizers();
		GC.Collect();
	}

	private static double elapsedMS(long start) { return (Stopwatch.GetTimestamp() - start) * TICK_TO_MS; }
	private static double ratio(double numerator, double denominator) { return denominator > 0.000001 ? numerator / denominator : 0.0; }

	private static double median(Sample[] values, Func<Sample, double> selector)
	{
		double[] data = new double[values.Length];
		for (int i = 0; i < values.Length; ++i) { data[i] = selector(values[i]); }
		Array.Sort(data);
		return data[data.Length >> 1];
	}

	private static int medianInt(Sample[] values, Func<Sample, int> selector)
	{
		int[] data = new int[values.Length];
		for (int i = 0; i < values.Length; ++i) { data[i] = selector(values[i]); }
		Array.Sort(data);
		return data[data.Length >> 1];
	}

	private static bool allValid(Sample[] values)
	{
		for (int i = 0; i < values.Length; ++i)
		{
			if (!values[i].mValid) { return false; }
		}
		return true;
	}
}
