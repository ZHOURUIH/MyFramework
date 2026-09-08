using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

// 真实 Sprite/Image 使用方式对比。
// 主生产回归保留 RawImage 历史基线；这里独立验证普通小图、九宫格和 Filled Image，避免把 RawImage 微测试误当成业务推荐用法。
public static class FastUIImageComparisonBenchmark
{
	private const int WARMUP_COUNT = 4;
	private const int SAMPLE_COUNT = 16;
	private static int mWarmupCount = WARMUP_COUNT;
	private static int mSampleCount = SAMPLE_COUNT;
	private const int SIMPLE_COUNT = 2048;
	private const int COMPLEX_COUNT = 512;
	private const int PROBE_SORTING_ORDER = -30000;
	private struct CompareResult
	{
		public string mName;
		public double mFastMS;
		public double mUGUIMS;
		public int mFastBatchCount;
		public string mDetail;
	}
	private sealed class PairScene
	{
		public GameObject mFastRoot;
		public FastCanvas mFastCanvas;
		public FastImage[] mFastImages;
		public RectTransform[] mFastRects;
		public GameObject mUGUIRoot;
		public Image[] mUGUIImages;
		public RectTransform[] mUGUIRects;
	}
	public static string run(FastCanvas sourceCanvas)
	{
		return runCore(sourceCanvas, null, null, true);
	}
	// 正式Inventory Benchmark传入生成好的MultiSprite子Sprite，确保专项测试也使用与真实背包相同的资源模型。
	public static string run(FastCanvas sourceCanvas, Sprite spriteA, Sprite spriteB)
	{
		return runCore(sourceCanvas, spriteA, spriteB, false);
	}
	private static string runCore(FastCanvas sourceCanvas, Sprite sourceSpriteA, Sprite sourceSpriteB, bool createRuntimeAtlas)
	{
		mWarmupCount = WARMUP_COUNT;
		mSampleCount = SAMPLE_COUNT;
		StringBuilder builder = new();
		builder.AppendLine("================ FastImage vs UGUI Image Sprite Benchmark ==============================================");
		builder.AppendLine(createRuntimeAtlas ?
			"Purpose:模拟普通小图使用。Sprite来自同一张测试Atlas，Sprite切换不会改变Texture。" :
			"Purpose:模拟普通小图使用。Sprite直接来自正式Inventory MultiSprite图集，Sprite切换不会改变Texture。");
		Material material = sourceCanvas != null ? sourceCanvas.getDefaultMaterial() : null;
		Material ownedMaterial = null;
		Texture2D ownedAtlas = null;
		Sprite ownedSpriteA = null;
		Sprite ownedSpriteB = null;
		Sprite spriteA = sourceSpriteA;
		Sprite spriteB = sourceSpriteB;
		List<CompareResult> results = new();
		try
		{
			if (material == null)
			{
				Shader shader = Shader.Find("Sprites/Default");
				if (shader == null)
				{
					builder.AppendLine("[SpriteImage Assert] Setup=False | Reason=Sprites/Default shader not found");
					return builder.ToString();
				}
				ownedMaterial = new Material(shader) { name = "FastUIImageBenchmarkMaterial" };
				material = ownedMaterial;
			}
			if (createRuntimeAtlas)
			{
				createAtlas(out ownedAtlas, out ownedSpriteA, out ownedSpriteB);
				spriteA = ownedSpriteA;
				spriteB = ownedSpriteB;
			}
			if (spriteA == null || spriteB == null || spriteA.texture == null || spriteB.texture == null || spriteA.texture != spriteB.texture)
			{
				builder.AppendLine("[SpriteImage Assert] Setup=False | Reason=SpriteA/SpriteB必须存在且来自同一张Texture");
				return builder.ToString();
			}
			builder.AppendLine("[SpriteImage Setup] Atlas=" + spriteA.texture.name + " | SpriteA=" + spriteA.name + " | SpriteB=" + spriteB.name + " | SameTexture=True | Source=" + (createRuntimeAtlas ? "RuntimeFallback" : "InventoryMultiSprite"));
			runSimpleSpriteSwap(results, material, spriteA, spriteB);
			runSimpleSize(results, material, spriteA);
			runSlicedSize(results, material, spriteA);
			runFilledAmount(results, material, spriteA);
			for (int i = 0; i < results.Count; ++i)
			{
				appendResult(builder, results[i]);
			}
			appendSummary(builder, results);
		}
		catch (Exception e)
		{
			builder.AppendLine("[SpriteImage Assert] Exception=False | Type=" + e.GetType().FullName + " | Message=" + e.Message + " | Stack=" + e.StackTrace);
		}
		finally
		{
			if (ownedSpriteA != null) UnityEngine.Object.Destroy(ownedSpriteA);
			if (ownedSpriteB != null) UnityEngine.Object.Destroy(ownedSpriteB);
			if (ownedAtlas != null) UnityEngine.Object.Destroy(ownedAtlas);
			if (ownedMaterial != null) UnityEngine.Object.Destroy(ownedMaterial);
		}
		return builder.ToString();
	}
	private static void runSimpleSpriteSwap(List<CompareResult> results, Material material, Sprite spriteA, Sprite spriteB)
	{
		PairScene scene = createPairScene(SIMPLE_COUNT, material, spriteA, FastUIImageType.Simple);
		try
		{
			int[] counts = { 1, 128, 1024, SIMPLE_COUNT };
			for (int i = 0; i < counts.Length; ++i)
			{
				int count = counts[i];
				double fast = measure(iteration => mutateFastSprite(scene, count, (iteration & 1) == 0 ? spriteB : spriteA));
				double ugui = measure(iteration => mutateUGUISprite(scene, count, (iteration & 1) == 0 ? spriteB : spriteA));
				results.Add(makeResult("SimpleSpriteSwapSameAtlas", fast, ugui, scene.mFastCanvas.getBatchCount(), "Changed=" + count + "/" + SIMPLE_COUNT));
			}
		}
		finally { disposePairScene(scene); }
	}
	private static void runSimpleSize(List<CompareResult> results, Material material, Sprite sprite)
	{
		PairScene scene = createPairScene(SIMPLE_COUNT, material, sprite, FastUIImageType.Simple);
		try
		{
			int[] counts = { 1, 128, 1024, SIMPLE_COUNT };
			for (int i = 0; i < counts.Length; ++i)
			{
				int count = counts[i];
				double fast = measure(iteration => mutateFastSize(scene, count, iteration));
				double ugui = measure(iteration => mutateUGUISize(scene, count, iteration));
				results.Add(makeResult("SimpleSpriteSize", fast, ugui, scene.mFastCanvas.getBatchCount(), "Changed=" + count + "/" + SIMPLE_COUNT));
			}
		}
		finally { disposePairScene(scene); }
	}
	private static void runSlicedSize(List<CompareResult> results, Material material, Sprite sprite)
	{
		PairScene scene = createPairScene(COMPLEX_COUNT, material, sprite, FastUIImageType.Sliced);
		try
		{
			double fast = measure(iteration => mutateFastSize(scene, COMPLEX_COUNT, iteration));
			double ugui = measure(iteration => mutateUGUISize(scene, COMPLEX_COUNT, iteration));
			results.Add(makeResult("SlicedSpriteSize", fast, ugui, scene.mFastCanvas.getBatchCount(), "Changed=" + COMPLEX_COUNT + "/" + COMPLEX_COUNT + " | Border=8px"));
		}
		finally { disposePairScene(scene); }
	}
	private static void runFilledAmount(List<CompareResult> results, Material material, Sprite sprite)
	{
		PairScene scene = createPairScene(COMPLEX_COUNT, material, sprite, FastUIImageType.Filled);
		try
		{
			double fast = measure(iteration =>
			{
				float amount = (iteration & 1) == 0 ? 0.35f : 0.85f;
				for (int i = 0; i < scene.mFastImages.Length; ++i) scene.mFastImages[i].setFillAmount(amount);
				scene.mFastCanvas.flushFrameNow();
			});
			double ugui = measure(iteration =>
			{
				float amount = (iteration & 1) == 0 ? 0.35f : 0.85f;
				for (int i = 0; i < scene.mUGUIImages.Length; ++i) scene.mUGUIImages[i].fillAmount = amount;
				Canvas.ForceUpdateCanvases();
			});
			results.Add(makeResult("FilledSpriteAmount", fast, ugui, scene.mFastCanvas.getBatchCount(), "Changed=" + COMPLEX_COUNT + "/" + COMPLEX_COUNT + " | Horizontal"));
		}
		finally { disposePairScene(scene); }
	}
	private static PairScene createPairScene(int count, Material material, Sprite sprite, FastUIImageType type)
	{
		PairScene scene = new();
		scene.mFastRoot = new GameObject("FastImageBenchmark_FastGUI", typeof(RectTransform));
		scene.mFastCanvas = scene.mFastRoot.AddComponent<FastCanvas>();
		scene.mFastCanvas.setDefaultMaterial(material);
		scene.mFastCanvas.setSortingOrder(PROBE_SORTING_ORDER);
		scene.mUGUIRoot = new GameObject("FastImageBenchmark_UGUI", typeof(RectTransform));
		Canvas uguiCanvas = scene.mUGUIRoot.AddComponent<Canvas>();
		uguiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
		uguiCanvas.sortingOrder = PROBE_SORTING_ORDER;
		scene.mFastImages = new FastImage[count];
		scene.mFastRects = new RectTransform[count];
		scene.mUGUIImages = new Image[count];
		scene.mUGUIRects = new RectTransform[count];
		for (int i = 0; i < count; ++i)
		{
			Vector3 position = gridPosition(i);
			RectTransform fastRect = createRect("FastImage_" + i, scene.mFastRoot.transform, new Vector2(20.0f, 20.0f), position);
			FastImage fast = fastRect.gameObject.AddComponent<FastImage>();
			fast.setSprite(sprite);
			fast.setType(type);
			if (type == FastUIImageType.Filled)
			{
				fast.setFillMethod(FastUIImageFillMethod.Horizontal);
				fast.setFillAmount(0.85f);
			}
			RectTransform uguiRect = createRect("UGUIImage_" + i, scene.mUGUIRoot.transform, new Vector2(20.0f, 20.0f), position);
			Image ugui = uguiRect.gameObject.AddComponent<Image>();
			ugui.sprite = sprite;
			if (type == FastUIImageType.Sliced) ugui.type = Image.Type.Sliced;
			else if (type == FastUIImageType.Filled)
			{
				ugui.type = Image.Type.Filled;
				ugui.fillMethod = Image.FillMethod.Horizontal;
				ugui.fillOrigin = 0;
				ugui.fillAmount = 0.85f;
			}
			scene.mFastImages[i] = fast;
			scene.mFastRects[i] = fastRect;
			scene.mUGUIImages[i] = ugui;
			scene.mUGUIRects[i] = uguiRect;
		}
		scene.mFastCanvas.flushFrameNow();
		Canvas.ForceUpdateCanvases();
		return scene;
	}
	private static void mutateFastSprite(PairScene scene, int count, Sprite sprite)
	{
		for (int i = 0; i < count; ++i) scene.mFastImages[getSparseIndex(i, scene.mFastImages.Length, count)].setSprite(sprite);
		scene.mFastCanvas.flushFrameNow();
	}
	private static void mutateUGUISprite(PairScene scene, int count, Sprite sprite)
	{
		for (int i = 0; i < count; ++i) scene.mUGUIImages[getSparseIndex(i, scene.mUGUIImages.Length, count)].sprite = sprite;
		Canvas.ForceUpdateCanvases();
	}
	private static void mutateFastSize(PairScene scene, int count, int iteration)
	{
		Vector2 size = (iteration & 1) == 0 ? new Vector2(28.0f, 24.0f) : new Vector2(20.0f, 20.0f);
		for (int i = 0; i < count; ++i) scene.mFastImages[getSparseIndex(i, scene.mFastImages.Length, count)].setSize(size);
		scene.mFastCanvas.flushFrameNow();
	}
	private static void mutateUGUISize(PairScene scene, int count, int iteration)
	{
		Vector2 size = (iteration & 1) == 0 ? new Vector2(28.0f, 24.0f) : new Vector2(20.0f, 20.0f);
		for (int i = 0; i < count; ++i) scene.mUGUIRects[getSparseIndex(i, scene.mUGUIRects.Length, count)].sizeDelta = size;
		Canvas.ForceUpdateCanvases();
	}
	private static void createAtlas(out Texture2D texture, out Sprite spriteA, out Sprite spriteB)
	{
		const int width = 128;
		const int height = 64;
		texture = new Texture2D(width, height, TextureFormat.RGBA32, false, false) { name = "FastUIImageBenchmarkAtlas", wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Point, hideFlags = HideFlags.HideAndDontSave };
		Color32[] pixels = new Color32[width * height];
		for (int y = 0; y < height; ++y)
		{
			for (int x = 0; x < width; ++x)
			{
				bool right = x >= 64;
				bool border = x % 64 < 8 || x % 64 >= 56 || y < 8 || y >= 56;
				pixels[y * width + x] = border ? new Color32(255, 255, 255, 255) : right ? new Color32(255, 150, 55, 255) : new Color32(55, 170, 255, 255);
			}
		}
		texture.SetPixels32(pixels);
		texture.Apply(false, false);
		Vector4 borderSize = new(8.0f, 8.0f, 8.0f, 8.0f);
		spriteA = Sprite.Create(texture, new Rect(0.0f, 0.0f, 64.0f, 64.0f), new Vector2(0.5f, 0.5f), 100.0f, 0, SpriteMeshType.FullRect, borderSize);
		spriteB = Sprite.Create(texture, new Rect(64.0f, 0.0f, 64.0f, 64.0f), new Vector2(0.5f, 0.5f), 100.0f, 0, SpriteMeshType.FullRect, borderSize);
		spriteA.name = "FastUIImageBenchmarkSpriteA";
		spriteB.name = "FastUIImageBenchmarkSpriteB";
		spriteA.hideFlags = HideFlags.HideAndDontSave;
		spriteB.hideFlags = HideFlags.HideAndDontSave;
	}
	private static RectTransform createRect(string name, Transform parent, Vector2 size, Vector3 position)
	{
		GameObject gameObject = new(name, typeof(RectTransform));
		RectTransform rect = gameObject.GetComponent<RectTransform>();
		rect.SetParent(parent, false);
		rect.anchorMin = new Vector2(0.5f, 0.5f);
		rect.anchorMax = new Vector2(0.5f, 0.5f);
		rect.pivot = new Vector2(0.5f, 0.5f);
		rect.sizeDelta = size;
		rect.localPosition = position;
		return rect;
	}
	private static Vector3 gridPosition(int index)
	{
		const int columns = 64;
		return new Vector3((index % columns) * 22.0f, -(index / columns) * 22.0f, 0.0f);
	}
	private static int getSparseIndex(int sampleIndex, int totalCount, int changedCount)
	{
		if (changedCount >= totalCount) return sampleIndex;
		int step = Mathf.Max(totalCount / changedCount, 1);
		return Mathf.Min(sampleIndex * step, totalCount - 1);
	}
	private static double measure(Action<int> action)
	{
		for (int i = 0; i < mWarmupCount; ++i) action(i);
		long start = System.Diagnostics.Stopwatch.GetTimestamp();
		for (int i = 0; i < mSampleCount; ++i) action(i + mWarmupCount);
		return (System.Diagnostics.Stopwatch.GetTimestamp() - start) * 1000.0 / System.Diagnostics.Stopwatch.Frequency / mSampleCount;
	}
	private static CompareResult makeResult(string name, double fast, double ugui, int batchCount, string detail)
	{
		return new CompareResult { mName = name, mFastMS = fast, mUGUIMS = ugui, mFastBatchCount = batchCount, mDetail = detail };
	}
	private static void appendResult(StringBuilder builder, CompareResult result)
	{
		double ratio = result.mFastMS > 0.0 ? result.mUGUIMS / result.mFastMS : 0.0;
		string verdict = ratio < 1.0 ? "FastWorse" : ratio < 1.2 ? "NearParity" : "FastAdvantage";
		builder.AppendLine("[SpriteImage Compare] Case=" + result.mName + " | FastGUI=" + format(result.mFastMS) + "ms | UGUI=" + format(result.mUGUIMS) + "ms | UGUI/Fast=" + ratio.ToString("F2", CultureInfo.InvariantCulture) + "x | FastBatch=" + result.mFastBatchCount + " | Verdict=" + verdict + " | " + result.mDetail);
	}
	private static void appendSummary(StringBuilder builder, List<CompareResult> results)
	{
		int worse = 0;
		int near = 0;
		double worstRatio = double.MaxValue;
		string worstName = "None";
		for (int i = 0; i < results.Count; ++i)
		{
			double ratio = results[i].mFastMS > 0.0 ? results[i].mUGUIMS / results[i].mFastMS : 0.0;
			if (ratio < 1.0) ++worse;
			else if (ratio < 1.2) ++near;
			if (ratio < worstRatio)
			{
				worstRatio = ratio;
				worstName = results[i].mName + " | " + results[i].mDetail;
			}
		}
		builder.AppendLine("[SpriteImage Summary] Cases=" + results.Count + " | FastWorse=" + worse + " | NearParity=" + near + " | Worst=" + worstName + " | UGUI/Fast=" + (worstRatio == double.MaxValue ? "N/A" : worstRatio.ToString("F2", CultureInfo.InvariantCulture) + "x"));
	}
	private static string format(double value) { return value.ToString("F4", CultureInfo.InvariantCulture); }
	private static void disposePairScene(PairScene scene)
	{
		if (scene == null) return;
		disposeRoot(scene.mFastRoot);
		disposeRoot(scene.mUGUIRoot);
	}
	private static void disposeRoot(GameObject root)
	{
		if (root == null) return;
		root.SetActive(false);
		UnityEngine.Object.Destroy(root);
	}
}
