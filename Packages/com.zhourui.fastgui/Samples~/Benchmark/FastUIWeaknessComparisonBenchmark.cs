using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

// 扩展使用场景与弱势探测Benchmark。
// 与主生产回归Case完全独立，只在主Benchmark结束后执行，不改变既有Case的采样口径。
public sealed class FastUIIndexBoundaryProbeElement : FastRawImage
{
	public const int QuadCount = 64;
	// Boundary Probe必须绕过RawImage的SimpleQuad fast path，否则Canvas只会为每个Probe保留4个顶点，无法真实跨过UInt16边界。
	public override bool tryGetSimpleQuadPositionGeometry(Matrix4x4 canvasWorldToLocal, out Vector3 bottomLeft, out Vector3 topLeft, out Vector3 topRight, out Vector3 bottomRight)
	{
		bottomLeft = default;
		topLeft = default;
		topRight = default;
		bottomRight = default;
		return false;
	}
	public override bool tryGetSimpleUVRect(out Rect uvRect)
	{
		uvRect = default;
		return false;
	}
	public override void buildGeometry(FastUIGeometryBuilder builder, Matrix4x4 canvasWorldToLocal)
	{
		ensureInit();
		refreshLocalGeometryCache();
		syncLocalTransformCache();
		builder.Clear();
		calculatePositionVertices(canvasWorldToLocal, out Vector3 bottomLeft, out Vector3 topLeft, out Vector3 topRight, out Vector3 bottomRight);
		for (int i = 0; i < QuadCount; ++i)
		{
			builder.AddQuad(bottomLeft, topLeft, topRight, bottomRight, Vector2.zero, Vector2.up, Vector2.one, Vector2.right);
		}
	}
}

public static class FastUIWeaknessComparisonBenchmark
{
	private const int ELEMENT_COUNT = 2048;
	private const int WARMUP_COUNT = 4;
	private const int SAMPLE_COUNT = 16;
	private const int PROBE_SORTING_ORDER = -30000;
	private const int INDEX_BOUNDARY_CREATE_BATCH = 1000;
	private const int FAST_INDEX_BOUNDARY_QUADS_PER_ELEMENT = FastUIIndexBoundaryProbeElement.QuadCount;
	private const int FAST_INDEX_BOUNDARY_SMALL_ELEMENTS = 250;
	private const int FAST_INDEX_BOUNDARY_EXPANDED_ELEMENTS = 257;
	private static int mWarmupCount = WARMUP_COUNT;
	private static int mSampleCount = SAMPLE_COUNT;
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
		public FastRawImage[] mFastImages;
		public RectTransform[] mFastRects;
		public GameObject mUGUIRoot;
		public Canvas mUGUICanvas;
		public RawImage[] mUGUIImages;
		public RectTransform[] mUGUIRects;
		public Vector3[] mBasePositions;
	}
	public static string run(FastCanvas sourceCanvas, TMP_FontAsset font)
	{
		mWarmupCount = WARMUP_COUNT;
		mSampleCount = SAMPLE_COUNT;
		StringBuilder builder = new();
		builder.AppendLine("================ FastGUI vs UGUI Usage Coverage & Weakness Probe ========================================");
		builder.AppendLine("Scope:CPU Mutation+Immediate UI Flush | FastGUI=FastCanvas.flushFrameNow | UGUI=Canvas.ForceUpdateCanvases | GPU/RenderThread excluded");
		builder.AppendLine("Purpose:Find crossover points and scenarios where FastGUI is near parity or slower; existing production regression cases remain unchanged.");
		Material material = sourceCanvas != null ? sourceCanvas.getDefaultMaterial() : null;
		Material ownedMaterial = null;
		Texture2D textureB = null;
		List<CompareResult> results = new();
		try
		{
			if (material == null)
			{
				Shader shader = Shader.Find("Sprites/Default");
				if (shader == null)
				{
					builder.AppendLine("[Weakness Probe Assert] Setup=False | Reason=Sprites/Default shader not found");
					return builder.ToString();
				}
				ownedMaterial = new Material(shader) { name = "FastUIWeaknessProbeMaterial" };
				material = ownedMaterial;
			}
			textureB = new Texture2D(1, 1, TextureFormat.RGBA32, false, false) { name = "FastUIWeaknessProbeTextureB", hideFlags = HideFlags.HideAndDontSave };
			textureB.SetPixel(0, 0, Color.white);
			textureB.Apply(false, true);
			Texture textureA = Texture2D.whiteTexture;
			runSparsePosition(results, material, textureA);
			runSizeMutation(results, material, textureA);
			runSiblingReorder(results, material, textureA);
			runBatchFragmentation(results, material, textureA, textureB);
			runDeepHierarchy(results, material, textureA);
			runNestedMaskDepth(results, material, textureA);
			runManyCanvas(results, material, textureA);
			if (font != null)
			{
				runTextMutation(results, material, font);
			}
			else
			{
				builder.AppendLine("[Weakness Probe Skip] TextMutation | Reason=mTextBenchmarkFont is null");
			}
			for (int i = 0; i < results.Count; ++i)
			{
				appendResult(builder, results[i]);
			}
			appendIndexBoundary(builder, material, textureA);
			appendSummary(builder, results);
		}
		catch (Exception e)
		{
			builder.AppendLine("[Weakness Probe Assert] Exception=False | Type=" + e.GetType().FullName + " | Message=" + e.Message + " | Stack=" + e.StackTrace);
		}
		finally
		{
			if (textureB != null)
			{
				UnityEngine.Object.Destroy(textureB);
			}
			if (ownedMaterial != null)
			{
				UnityEngine.Object.Destroy(ownedMaterial);
			}
		}
		return builder.ToString();
	}
	// 正式Player Benchmark按阶段执行并在阶段间让Unity真正销毁临时对象，避免几十组测试对象在同一帧累积。
	// 同步run()继续保留给现有Reporter；正式长测试优先使用本Coroutine入口。
	public static IEnumerator runCoroutine(FastCanvas sourceCanvas, TMP_FontAsset font, Action<string> log)
	{
		mWarmupCount = WARMUP_COUNT;
		mSampleCount = SAMPLE_COUNT;
		if (log == null)
		{
			yield break;
		}
		StringBuilder builder = new();
		builder.AppendLine("================ FastGUI vs UGUI Usage Coverage & Weakness Probe ========================================");
		builder.AppendLine("Scope:CPU Mutation+Immediate UI Flush | FastGUI=FastCanvas.flushFrameNow | UGUI=Canvas.ForceUpdateCanvases | GPU/RenderThread excluded");
		builder.AppendLine("Purpose:Find crossover points and scenarios where FastGUI is near parity or slower; existing production regression cases remain unchanged.");
		if (!trySetupCoroutineResources(sourceCanvas, builder, out Material material, out Material ownedMaterial, out Texture2D textureB))
		{
			logBuilderLines(log, builder);
			yield break;
		}
		Texture textureA = Texture2D.whiteTexture;
		List<CompareResult> results = new();
		if (!tryRunCoroutineStage(() => runSparsePosition(results, material, textureA), builder, "SparsePosition"))
		{
			cleanupCoroutineResources(textureB, ownedMaterial);
			logBuilderLines(log, builder);
			yield break;
		}
		log("[Weakness Probe Progress] Stage=SparsePosition | Cases=" + results.Count);
		yield return new WaitForEndOfFrame();
		yield return null;
		if (!tryRunCoroutineStage(() => runSizeMutation(results, material, textureA), builder, "SizeMutation"))
		{
			cleanupCoroutineResources(textureB, ownedMaterial);
			logBuilderLines(log, builder);
			yield break;
		}
		log("[Weakness Probe Progress] Stage=SizeMutation | Cases=" + results.Count);
		yield return new WaitForEndOfFrame();
		yield return null;
		if (!tryRunCoroutineStage(() => runSiblingReorder(results, material, textureA), builder, "SiblingReorder"))
		{
			cleanupCoroutineResources(textureB, ownedMaterial);
			logBuilderLines(log, builder);
			yield break;
		}
		log("[Weakness Probe Progress] Stage=SiblingReorder | Cases=" + results.Count);
		yield return new WaitForEndOfFrame();
		yield return null;
		if (!tryRunCoroutineStage(() => runBatchFragmentation(results, material, textureA, textureB), builder, "BatchFragmentation"))
		{
			cleanupCoroutineResources(textureB, ownedMaterial);
			logBuilderLines(log, builder);
			yield break;
		}
		log("[Weakness Probe Progress] Stage=BatchFragmentation | Cases=" + results.Count);
		yield return new WaitForEndOfFrame();
		yield return null;
		if (!tryRunCoroutineStage(() => runDeepHierarchy(results, material, textureA), builder, "DeepHierarchy"))
		{
			cleanupCoroutineResources(textureB, ownedMaterial);
			logBuilderLines(log, builder);
			yield break;
		}
		log("[Weakness Probe Progress] Stage=DeepHierarchy | Cases=" + results.Count);
		yield return new WaitForEndOfFrame();
		yield return null;
		if (!tryRunCoroutineStage(() => runNestedMaskDepth(results, material, textureA), builder, "NestedMaskDepth"))
		{
			cleanupCoroutineResources(textureB, ownedMaterial);
			logBuilderLines(log, builder);
			yield break;
		}
		log("[Weakness Probe Progress] Stage=NestedMaskDepth | Cases=" + results.Count);
		yield return new WaitForEndOfFrame();
		yield return null;
		if (!tryRunCoroutineStage(() => runManyCanvas(results, material, textureA), builder, "ManyCanvas"))
		{
			cleanupCoroutineResources(textureB, ownedMaterial);
			logBuilderLines(log, builder);
			yield break;
		}
		log("[Weakness Probe Progress] Stage=ManyCanvas | Cases=" + results.Count);
		yield return new WaitForEndOfFrame();
		yield return null;
		if (font != null)
		{
			if (!tryRunCoroutineStage(() => runTextMutation(results, material, font), builder, "TextMutation"))
			{
				cleanupCoroutineResources(textureB, ownedMaterial);
				logBuilderLines(log, builder);
				yield break;
			}
			log("[Weakness Probe Progress] Stage=TextMutation | Cases=" + results.Count);
			yield return new WaitForEndOfFrame();
			yield return null;
		}
		else
		{
			builder.AppendLine("[Weakness Probe Skip] TextMutation | Reason=mTextBenchmarkFont is null");
		}
		for (int i = 0; i < results.Count; ++i)
		{
			appendResult(builder, results[i]);
		}
		IEnumerator indexBoundary = appendIndexBoundaryCoroutine(builder, material, textureA, log);
		try
		{
			while (indexBoundary.MoveNext())
			{
				yield return indexBoundary.Current;
			}
		}
		finally
		{
			(indexBoundary as IDisposable)?.Dispose();
		}
		appendSummary(builder, results);
		cleanupCoroutineResources(textureB, ownedMaterial);
		logBuilderLines(log, builder);
		yield return new WaitForEndOfFrame();
		yield return null;
	}
	private static void logBuilderLines(Action<string> log, StringBuilder builder)
	{
		if (log == null || builder == null || builder.Length == 0) return;
		string text = builder.ToString();
		int lineStart = 0;
		for (int i = 0; i <= text.Length; ++i)
		{
			if (i < text.Length && text[i] != '\n') continue;
			int lineEnd = i > lineStart && text[i - 1] == '\r' ? i - 1 : i;
			if (lineEnd > lineStart) log(text.Substring(lineStart, lineEnd - lineStart));
			lineStart = i + 1;
		}
	}
	private static bool trySetupCoroutineResources(FastCanvas sourceCanvas, StringBuilder builder, out Material material, out Material ownedMaterial, out Texture2D textureB)
	{
		material = sourceCanvas != null ? sourceCanvas.getDefaultMaterial() : null;
		ownedMaterial = null;
		textureB = null;
		try
		{
			if (material == null)
			{
				Shader shader = Shader.Find("Sprites/Default");
				if (shader == null)
				{
					builder.AppendLine("[Weakness Probe Assert] Setup=False | Reason=Sprites/Default shader not found");
					return false;
				}
				ownedMaterial = new Material(shader) { name = "FastUIWeaknessProbeMaterial" };
				material = ownedMaterial;
			}
			textureB = new Texture2D(1, 1, TextureFormat.RGBA32, false, false) { name = "FastUIWeaknessProbeTextureB", hideFlags = HideFlags.HideAndDontSave };
			textureB.SetPixel(0, 0, Color.white);
			textureB.Apply(false, true);
			return true;
		}
		catch (Exception e)
		{
			builder.AppendLine("[Weakness Probe Assert] Setup=False | Type=" + e.GetType().FullName + " | Message=" + e.Message);
			cleanupCoroutineResources(textureB, ownedMaterial);
			return false;
		}
	}
	private static bool tryRunCoroutineStage(Action action, StringBuilder builder, string stage)
	{
		try
		{
			action();
			return true;
		}
		catch (Exception e)
		{
			builder.AppendLine("[Weakness Probe Assert] Stage=" + stage + " | Exception=False | Type=" + e.GetType().FullName + " | Message=" + e.Message + " | Stack=" + e.StackTrace);
			return false;
		}
	}
	private static void cleanupCoroutineResources(Texture2D texture, Material material)
	{
		if (texture != null) UnityEngine.Object.Destroy(texture);
		if (material != null) UnityEngine.Object.Destroy(material);
	}
	private static void runSparsePosition(List<CompareResult> results, Material material, Texture texture)
	{
		PairScene scene = createPairScene(ELEMENT_COUNT, material, texture, 0);
		try
		{
			int[] counts = { 1, 16, 128, 512, ELEMENT_COUNT };
			for (int i = 0; i < counts.Length; ++i)
			{
				int count = counts[i];
				double fast = measure(iteration => mutateFastPosition(scene, count, iteration));
				double ugui = measure(iteration => mutateUGUIPosition(scene, count, iteration));
				results.Add(makeResult("SparsePosition", fast, ugui, scene.mFastCanvas.getBatchCount(), "Changed=" + count + "/" + ELEMENT_COUNT));
			}
		}
		finally { disposePairScene(scene); }
	}
	private static void runSizeMutation(List<CompareResult> results, Material material, Texture texture)
	{
		PairScene scene = createPairScene(ELEMENT_COUNT, material, texture, 0);
		try
		{
			int[] counts = { 1, 16, 128, 512, ELEMENT_COUNT };
			for (int i = 0; i < counts.Length; ++i)
			{
				int count = counts[i];
				double fast = measure(iteration => mutateFastSize(scene, count, iteration));
				double ugui = measure(iteration => mutateUGUISize(scene, count, iteration));
				results.Add(makeResult("SizeMutation", fast, ugui, scene.mFastCanvas.getBatchCount(), "Changed=" + count + "/" + ELEMENT_COUNT));
			}
		}
		finally { disposePairScene(scene); }
	}
	private static void runSiblingReorder(List<CompareResult> results, Material material, Texture texture)
	{
		int[] counts = { 1, 16, 128 };
		for (int i = 0; i < counts.Length; ++i)
		{
			PairScene scene = createPairScene(ELEMENT_COUNT, material, texture, 0);
			try
			{
				int count = counts[i];
				double fast = measure(iteration => mutateFastSibling(scene, count, iteration));
				double ugui = measure(iteration => mutateUGUISibling(scene, count, iteration));
				results.Add(makeResult("SiblingReorder", fast, ugui, scene.mFastCanvas.getBatchCount(), "Changed=" + count + "/" + ELEMENT_COUNT));
			}
			finally { disposePairScene(scene); }
		}
	}
	private static void runBatchFragmentation(List<CompareResult> results, Material material, Texture textureA, Texture textureB)
	{
		int[] runLengths = { ELEMENT_COUNT, 64, 8, 1 };
		for (int i = 0; i < runLengths.Length; ++i)
		{
			int runLength = runLengths[i];
			PairScene scene = createPairScene(ELEMENT_COUNT, material, textureA, runLength, textureB);
			try
			{
				double fast = measure(iteration => mutateFastTextureSparse(scene, 128, runLength, textureA, textureB, iteration));
				double ugui = measure(iteration => mutateUGUITextureSparse(scene, 128, runLength, textureA, textureB, iteration));
				results.Add(makeResult("BatchFragmentation", fast, ugui, scene.mFastCanvas.getBatchCount(), "BaseRunLength=" + runLength + " | Changed=128"));
			}
			finally { disposePairScene(scene); }
		}
	}
	private static void runDeepHierarchy(List<CompareResult> results, Material material, Texture texture)
	{
		int[] depths = { 1, 4, 8, 16 };
		for (int i = 0; i < depths.Length; ++i)
		{
			int depth = depths[i];
			PairScene scene = createDeepPairScene(ELEMENT_COUNT, depth, material, texture, out RectTransform fastMoveRoot, out RectTransform uguiMoveRoot);
			try
			{
				double fast = measure(iteration =>
				{
					Vector3 target = (iteration & 1) == 0 ? new Vector3(3.0f, 2.0f, 0.0f) : Vector3.zero;
					Vector3 old = fastMoveRoot.localPosition;
					if (old != target)
					{
						fastMoveRoot.localPosition = target;
						scene.mFastCanvas.notifyTransformPositionChanged(fastMoveRoot, old, target);
					}
					scene.mFastCanvas.flushFrameNow();
				});
				double ugui = measure(iteration =>
				{
					uguiMoveRoot.localPosition = (iteration & 1) == 0 ? new Vector3(3.0f, 2.0f, 0.0f) : Vector3.zero;
					Canvas.ForceUpdateCanvases();
				});
				results.Add(makeResult("DeepHierarchyRootMove", fast, ugui, scene.mFastCanvas.getBatchCount(), "Depth=" + depth + " | Leaves=" + ELEMENT_COUNT));
			}
			finally { disposePairScene(scene); }
		}
	}
	private static void runNestedMaskDepth(List<CompareResult> results, Material material, Texture texture)
	{
		int[] depths = { 1, 2, 4, 8 };
		const int leafCount = 512;
		for (int d = 0; d < depths.Length; ++d)
		{
			int depth = depths[d];
			GameObject fastRoot = new("FastUIMaskDepthFast", typeof(RectTransform));
			FastCanvas fastCanvas = fastRoot.AddComponent<FastCanvas>();
			fastCanvas.setDefaultMaterial(material);
			fastCanvas.setSortingOrder(PROBE_SORTING_ORDER);
			GameObject uguiRoot = new("FastUIMaskDepthUGUI", typeof(RectTransform));
			Canvas uguiCanvas = uguiRoot.AddComponent<Canvas>();
			uguiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
			uguiCanvas.sortingOrder = PROBE_SORTING_ORDER;
			Transform fastParent = fastRoot.transform;
			Transform uguiParent = uguiRoot.transform;
			RectTransform fastTarget = null;
			RectTransform uguiTarget = null;
			try
			{
				for (int i = 0; i < depth; ++i)
				{
					RectTransform fastRect = createRect("FastMask_" + i, fastParent, new Vector2(720.0f - i * 8.0f, 720.0f - i * 8.0f), Vector3.zero);
					fastRect.gameObject.AddComponent<FastRectMask2D>();
					RectTransform uguiRect = createRect("UGUIMask_" + i, uguiParent, new Vector2(720.0f - i * 8.0f, 720.0f - i * 8.0f), Vector3.zero);
					uguiRect.gameObject.AddComponent<RectMask2D>();
					fastParent = fastRect;
					uguiParent = uguiRect;
					fastTarget = fastRect;
					uguiTarget = uguiRect;
				}
				for (int i = 0; i < leafCount; ++i)
				{
					createFastImage(fastParent, texture, i);
					createUGUIImage(uguiParent, texture, i);
				}
				fastCanvas.flushFrameNow();
				Canvas.ForceUpdateCanvases();
				Vector2 baseFastSize = fastTarget.sizeDelta;
				Vector2 baseUGUISize = uguiTarget.sizeDelta;
				double fast = measure(iteration =>
				{
					fastTarget.sizeDelta = (iteration & 1) == 0 ? baseFastSize - new Vector2(4.0f, 2.0f) : baseFastSize;
					fastCanvas.flushFrameNow();
				});
				double ugui = measure(iteration =>
				{
					uguiTarget.sizeDelta = (iteration & 1) == 0 ? baseUGUISize - new Vector2(4.0f, 2.0f) : baseUGUISize;
					Canvas.ForceUpdateCanvases();
				});
				results.Add(makeResult("NestedRectMaskResize", fast, ugui, fastCanvas.getBatchCount(), "Depth=" + depth + " | Leaves=" + leafCount));
			}
			finally
			{
				disposeRoot(fastRoot);
				disposeRoot(uguiRoot);
			}
		}
	}
	private static void runManyCanvas(List<CompareResult> results, Material material, Texture texture)
	{
		int[] canvasCounts = { 1, 4, 16, 64 };
		for (int c = 0; c < canvasCounts.Length; ++c)
		{
			int canvasCount = canvasCounts[c];
			FastCanvas[] fastCanvases = new FastCanvas[canvasCount];
			FastRawImage[] fastImages = new FastRawImage[canvasCount];
			GameObject[] fastRoots = new GameObject[canvasCount];
			RawImage[] uguiImages = new RawImage[canvasCount];
			GameObject[] uguiRoots = new GameObject[canvasCount];
			try
			{
				int perCanvas = Mathf.Max(ELEMENT_COUNT / canvasCount, 1);
				for (int i = 0; i < canvasCount; ++i)
				{
					GameObject fastRootObject = new("FastCanvas_" + i, typeof(RectTransform));
					fastRoots[i] = fastRootObject;
					FastCanvas fastCanvas = fastRootObject.AddComponent<FastCanvas>();
					fastCanvas.setDefaultMaterial(material);
					fastCanvas.setSortingOrder(PROBE_SORTING_ORDER);
					fastCanvases[i] = fastCanvas;
					for (int j = 0; j < perCanvas; ++j)
					{
						FastRawImage image = createFastImage(fastRootObject.transform, texture, j);
						if (j == 0) fastImages[i] = image;
					}
					GameObject uguiRootObject = new("UGUICanvas_" + i, typeof(RectTransform));
					uguiRoots[i] = uguiRootObject;
					Canvas canvas = uguiRootObject.AddComponent<Canvas>();
					canvas.renderMode = RenderMode.ScreenSpaceOverlay;
					canvas.sortingOrder = PROBE_SORTING_ORDER;
					for (int j = 0; j < perCanvas; ++j)
					{
						RawImage image = createUGUIImage(uguiRootObject.transform, texture, j);
						if (j == 0) uguiImages[i] = image;
					}
				}
				for (int i = 0; i < fastCanvases.Length; ++i) fastCanvases[i].flushFrameNow();
				Canvas.ForceUpdateCanvases();
				double fast = measure(iteration =>
				{
					float x = (iteration & 1) == 0 ? 2.0f : 0.0f;
					for (int i = 0; i < fastImages.Length; ++i) fastImages[i].setLocalPosition(new Vector3(x, 0.0f, 0.0f));
					for (int i = 0; i < fastCanvases.Length; ++i) fastCanvases[i].flushFrameNow();
				});
				double ugui = measure(iteration =>
				{
					float x = (iteration & 1) == 0 ? 2.0f : 0.0f;
					for (int i = 0; i < uguiImages.Length; ++i) uguiImages[i].rectTransform.localPosition = new Vector3(x, 0.0f, 0.0f);
					Canvas.ForceUpdateCanvases();
				});
				int batches = 0;
				for (int i = 0; i < fastCanvases.Length; ++i) batches += fastCanvases[i].getBatchCount();
				results.Add(makeResult("ManyCanvasDirtyOneEach", fast, ugui, batches, "Canvases=" + canvasCount + " | TotalElements~=" + (perCanvas * canvasCount)));
			}
			finally
			{
				for (int i = 0; i < fastRoots.Length; ++i) disposeRoot(fastRoots[i]);
				for (int i = 0; i < uguiRoots.Length; ++i) disposeRoot(uguiRoots[i]);
			}
		}
	}
	private static void runTextMutation(List<CompareResult> results, Material material, TMP_FontAsset font)
	{
		int textCount = 512;
		GameObject fastRootObject = new("FastUITextWeakness", typeof(RectTransform));
		FastCanvas fastCanvas = fastRootObject.AddComponent<FastCanvas>();
		fastCanvas.setDefaultMaterial(material);
		fastCanvas.setSortingOrder(PROBE_SORTING_ORDER);
		FastText[] fastTexts = new FastText[textCount];
		GameObject uguiRootObject = new("UGUITextWeakness", typeof(RectTransform));
		Canvas uguiCanvas = uguiRootObject.AddComponent<Canvas>();
		uguiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
		uguiCanvas.sortingOrder = PROBE_SORTING_ORDER;
		TextMeshProUGUI[] uguiTexts = new TextMeshProUGUI[textCount];
		try
		{
			for (int i = 0; i < textCount; ++i)
			{
				RectTransform fastRect = createRect("FastText_" + i, fastRootObject.transform, new Vector2(180.0f, 32.0f), gridPosition(i));
				FastText fastText = fastRect.gameObject.AddComponent<FastText>();
				fastText.setFont(font);
				fastText.setFontSize(18.0f);
				fastText.setWordWrap(false);
				fastText.setText("HP:99");
				fastTexts[i] = fastText;
				RectTransform uguiRect = createRect("UGUIText_" + i, uguiRootObject.transform, new Vector2(180.0f, 32.0f), gridPosition(i));
				TextMeshProUGUI uguiText = uguiRect.gameObject.AddComponent<TextMeshProUGUI>();
				uguiText.font = font;
				uguiText.fontSize = 18.0f;
				uguiText.enableWordWrapping = false;
				uguiText.raycastTarget = false;
				uguiText.text = "HP:99";
				uguiTexts[i] = uguiText;
			}
			fastCanvas.flushFrameNow();
			Canvas.ForceUpdateCanvases();
			int[] counts = { 1, 32, 128, 512 };
			for (int c = 0; c < counts.Length; ++c)
			{
				int count = counts[c];
				double fast = measure(iteration =>
				{
					string text = (iteration & 1) == 0 ? "HP:100" : "HP:99";
					for (int i = 0; i < count; ++i) fastTexts[i].setText(text);
					fastCanvas.flushFrameNow();
				});
				double ugui = measure(iteration =>
				{
					string text = (iteration & 1) == 0 ? "HP:100" : "HP:99";
					for (int i = 0; i < count; ++i) uguiTexts[i].text = text;
					Canvas.ForceUpdateCanvases();
				});
				results.Add(makeResult("TextShortMutation", fast, ugui, fastCanvas.getBatchCount(), "Changed=" + count + "/512"));
			}
			const string longA = "FastGUI long wrapped text benchmark: item description, attributes, cooldown and multiline layout data 1234567890.";
			const string longB = "FastGUI long wrapped text benchmark changed: item description, attributes, cooldown, multiline layout and extra status 0987654321.";
			for (int i = 0; i < 128; ++i)
			{
				fastTexts[i].setSize(new Vector2(180.0f, 120.0f));
				fastTexts[i].setWordWrap(true);
				fastTexts[i].setText(longA);
				uguiTexts[i].rectTransform.sizeDelta = new Vector2(180.0f, 120.0f);
				uguiTexts[i].enableWordWrapping = true;
				uguiTexts[i].text = longA;
			}
			fastCanvas.flushFrameNow();
			Canvas.ForceUpdateCanvases();
			double fastLong = measure(iteration =>
			{
				string text = (iteration & 1) == 0 ? longB : longA;
				for (int i = 0; i < 128; ++i) fastTexts[i].setText(text);
				fastCanvas.flushFrameNow();
			});
			double uguiLong = measure(iteration =>
			{
				string text = (iteration & 1) == 0 ? longB : longA;
				for (int i = 0; i < 128; ++i) uguiTexts[i].text = text;
				Canvas.ForceUpdateCanvases();
			});
			results.Add(makeResult("TextWrappedLongMutation", fastLong, uguiLong, fastCanvas.getBatchCount(), "Changed=128/512 | Wrap=True"));
		}
		finally
		{
			disposeRoot(fastRootObject);
			disposeRoot(uguiRootObject);
		}
	}
	// Index边界只验证格式切换，不测创建速度。正式Player中分批创建，避免一次同步构建16512个GameObject导致窗口长时间无刷新。
	private static IEnumerator appendIndexBoundaryCoroutine(StringBuilder builder, Material material, Texture texture, Action<string> log)
	{
		GameObject root = new("FastUIIndexBoundary", typeof(RectTransform));
		root.hideFlags = HideFlags.DontSave;
		FastCanvas canvas = root.AddComponent<FastCanvas>();
		canvas.setDefaultMaterial(material);
		canvas.setSortingOrder(PROBE_SORTING_ORDER);
		const int smallCount = 16000;
		const int expandCount = 512;
		FastRawImage first = null;
		try
		{
			for (int i = 0; i < smallCount; ++i)
			{
				FastRawImage image = createFastImage(root.transform, texture, i);
				first ??= image;
				if ((i + 1) % INDEX_BOUNDARY_CREATE_BATCH == 0)
				{
					log?.Invoke("[Weakness Probe Progress] Stage=IndexBoundary.BuildSmall | Created=" + (i + 1) + "/" + smallCount);
					yield return null;
				}
			}
			canvas.flushFrameNow();
			FastUIMeshRenderer renderer = canvas.getMeshRenderer();
			int smallSpan = renderer.getVertexSpan();
			IndexFormat smallFormat = renderer.getGPUIndexFormat();
			first?.setColor(new Color(0.9f, 0.9f, 0.9f, 1.0f));
			canvas.flushFrameNow();
			for (int i = 0; i < expandCount; ++i) createFastImage(root.transform, texture, smallCount + i);
			yield return null;
			canvas.flushFrameNow();
			int largeSpan = renderer.getVertexSpan();
			IndexFormat largeFormat = renderer.getGPUIndexFormat();
			first?.setColor(Color.white);
			canvas.flushFrameNow();
			bool pass = smallSpan <= ushort.MaxValue && smallFormat == IndexFormat.UInt16 &&
				largeSpan > ushort.MaxValue && largeFormat == IndexFormat.UInt32;
			builder.AppendLine("[Index Boundary] Small=" + smallCount + " | VertexSpan=" + smallSpan + " | Format=" + smallFormat);
			builder.AppendLine("[Index Boundary] Expanded=" + (smallCount + expandCount) + " | VertexSpan=" + largeSpan + " | Format=" + largeFormat);
			builder.AppendLine("[Index Boundary Assert] UInt16ToUInt32=" + pass);
		}
		finally
		{
			disposeRoot(root);
		}
		yield return new WaitForEndOfFrame();
		yield return null;
	}
	private static void appendIndexBoundary(StringBuilder builder, Material material, Texture texture)
	{
		GameObject root = new("FastUIIndexBoundary", typeof(RectTransform));
		FastCanvas canvas = root.AddComponent<FastCanvas>();
		canvas.setDefaultMaterial(material);
		canvas.setSortingOrder(PROBE_SORTING_ORDER);
		try
		{
			const int smallCount = 16000;
			const int expandCount = 512;
			FastRawImage first = null;
			for (int i = 0; i < smallCount; ++i)
			{
				FastRawImage image = createFastImage(root.transform, texture, i);
				if (first == null) first = image;
			}
			canvas.flushFrameNow();
			FastUIMeshRenderer renderer = canvas.getMeshRenderer();
			int smallSpan = renderer.getVertexSpan();
			IndexFormat smallFormat = renderer.getGPUIndexFormat();
			first?.setColor(new Color(0.9f, 0.9f, 0.9f, 1.0f));
			canvas.flushFrameNow();
			for (int i = 0; i < expandCount; ++i) createFastImage(root.transform, texture, smallCount + i);
			canvas.flushFrameNow();
			int largeSpan = renderer.getVertexSpan();
			IndexFormat largeFormat = renderer.getGPUIndexFormat();
			first?.setColor(Color.white);
			canvas.flushFrameNow();
			bool pass = smallSpan <= ushort.MaxValue && smallFormat == IndexFormat.UInt16 &&
				largeSpan > ushort.MaxValue && largeFormat == IndexFormat.UInt32;
			builder.AppendLine("[Index Boundary] Small=" + smallCount + " | VertexSpan=" + smallSpan + " | Format=" + smallFormat);
			builder.AppendLine("[Index Boundary] Expanded=" + (smallCount + expandCount) + " | VertexSpan=" + largeSpan + " | Format=" + largeFormat);
			builder.AppendLine("[Index Boundary Assert] UInt16ToUInt32=" + pass);
		}
		finally { disposeRoot(root); }
	}
	private static PairScene createPairScene(int count, Material material, Texture texture, int runLength, Texture textureB = null)
	{
		PairScene scene = new();
		scene.mFastRoot = new GameObject("FastUIWeaknessFast", typeof(RectTransform));
		scene.mFastCanvas = scene.mFastRoot.AddComponent<FastCanvas>();
		scene.mFastCanvas.setDefaultMaterial(material);
		scene.mFastCanvas.setSortingOrder(PROBE_SORTING_ORDER);
		scene.mFastImages = new FastRawImage[count];
		scene.mFastRects = new RectTransform[count];
		scene.mUGUIRoot = new GameObject("FastUIWeaknessUGUI", typeof(RectTransform));
		scene.mUGUICanvas = scene.mUGUIRoot.AddComponent<Canvas>();
		scene.mUGUICanvas.renderMode = RenderMode.ScreenSpaceOverlay;
		scene.mUGUICanvas.sortingOrder = PROBE_SORTING_ORDER;
		scene.mUGUIImages = new RawImage[count];
		scene.mUGUIRects = new RectTransform[count];
		scene.mBasePositions = new Vector3[count];
		for (int i = 0; i < count; ++i)
		{
			Texture currentTexture = textureB != null && runLength > 0 && ((i / runLength) & 1) != 0 ? textureB : texture;
			FastRawImage fast = createFastImage(scene.mFastRoot.transform, currentTexture, i);
			RawImage ugui = createUGUIImage(scene.mUGUIRoot.transform, currentTexture, i);
			scene.mFastImages[i] = fast;
			scene.mFastRects[i] = fast.getRectTransform();
			scene.mUGUIImages[i] = ugui;
			scene.mUGUIRects[i] = ugui.rectTransform;
			scene.mBasePositions[i] = gridPosition(i);
		}
		scene.mFastCanvas.flushFrameNow();
		Canvas.ForceUpdateCanvases();
		return scene;
	}
	private static PairScene createDeepPairScene(int count, int depth, Material material, Texture texture, out RectTransform fastMoveRoot, out RectTransform uguiMoveRoot)
	{
		PairScene scene = new();
		scene.mFastRoot = new GameObject("FastUIDeepFast", typeof(RectTransform));
		scene.mFastCanvas = scene.mFastRoot.AddComponent<FastCanvas>();
		scene.mFastCanvas.setDefaultMaterial(material);
		scene.mFastCanvas.setSortingOrder(PROBE_SORTING_ORDER);
		scene.mUGUIRoot = new GameObject("FastUIDeepUGUI", typeof(RectTransform));
		scene.mUGUICanvas = scene.mUGUIRoot.AddComponent<Canvas>();
		scene.mUGUICanvas.renderMode = RenderMode.ScreenSpaceOverlay;
		scene.mUGUICanvas.sortingOrder = PROBE_SORTING_ORDER;
		Transform fastParent = scene.mFastRoot.transform;
		Transform uguiParent = scene.mUGUIRoot.transform;
		fastMoveRoot = null;
		uguiMoveRoot = null;
		for (int i = 0; i < depth; ++i)
		{
			RectTransform fast = createRect("FastDepth_" + i, fastParent, new Vector2(1024.0f, 1024.0f), Vector3.zero);
			RectTransform ugui = createRect("UGUIDepth_" + i, uguiParent, new Vector2(1024.0f, 1024.0f), Vector3.zero);
			if (i == 0)
			{
				fastMoveRoot = fast;
				uguiMoveRoot = ugui;
			}
			fastParent = fast;
			uguiParent = ugui;
		}
		scene.mFastImages = new FastRawImage[count];
		scene.mUGUIImages = new RawImage[count];
		for (int i = 0; i < count; ++i)
		{
			scene.mFastImages[i] = createFastImage(fastParent, texture, i);
			scene.mUGUIImages[i] = createUGUIImage(uguiParent, texture, i);
		}
		scene.mFastCanvas.flushFrameNow();
		Canvas.ForceUpdateCanvases();
		return scene;
	}
	private static FastRawImage createFastImage(Transform parent, Texture texture, int index)
	{
		RectTransform rect = createRect("FastImage_" + index, parent, new Vector2(20.0f, 20.0f), gridPosition(index));
		FastRawImage image = rect.gameObject.AddComponent<FastRawImage>();
		image.setTexture(texture);
		return image;
	}
	private static RawImage createUGUIImage(Transform parent, Texture texture, int index)
	{
		RectTransform rect = createRect("UGUIImage_" + index, parent, new Vector2(20.0f, 20.0f), gridPosition(index));
		RawImage image = rect.gameObject.AddComponent<RawImage>();
		image.texture = texture;
		image.raycastTarget = false;
		return image;
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
		rect.localScale = Vector3.one;
		rect.localRotation = Quaternion.identity;
		return rect;
	}
	private static Vector3 gridPosition(int index)
	{
		return new Vector3((index % 64) * 22.0f, -(index / 64) * 22.0f, 0.0f);
	}
	private static double measure(Action<int> action)
	{
		for (int i = 0; i < mWarmupCount; ++i) action(i);
		long start = System.Diagnostics.Stopwatch.GetTimestamp();
		for (int i = 0; i < mSampleCount; ++i) action(i + mWarmupCount);
		return (System.Diagnostics.Stopwatch.GetTimestamp() - start) * 1000.0 / System.Diagnostics.Stopwatch.Frequency / mSampleCount;
	}
	private static void mutateFastPosition(PairScene scene, int count, int iteration)
	{
		float offset = (iteration & 1) == 0 ? 3.0f : 0.0f;
		visitSparse(scene.mFastImages.Length, count, index => scene.mFastImages[index].setLocalPosition(scene.mBasePositions[index] + new Vector3(offset, 0.0f, 0.0f)));
		scene.mFastCanvas.flushFrameNow();
	}
	private static void mutateUGUIPosition(PairScene scene, int count, int iteration)
	{
		float offset = (iteration & 1) == 0 ? 3.0f : 0.0f;
		visitSparse(scene.mUGUIRects.Length, count, index => scene.mUGUIRects[index].localPosition = scene.mBasePositions[index] + new Vector3(offset, 0.0f, 0.0f));
		Canvas.ForceUpdateCanvases();
	}
	private static void mutateFastSize(PairScene scene, int count, int iteration)
	{
		Vector2 size = (iteration & 1) == 0 ? new Vector2(24.0f, 22.0f) : new Vector2(20.0f, 20.0f);
		visitSparse(scene.mFastImages.Length, count, index => scene.mFastImages[index].setSize(size));
		scene.mFastCanvas.flushFrameNow();
	}
	private static void mutateUGUISize(PairScene scene, int count, int iteration)
	{
		Vector2 size = (iteration & 1) == 0 ? new Vector2(24.0f, 22.0f) : new Vector2(20.0f, 20.0f);
		visitSparse(scene.mUGUIRects.Length, count, index => scene.mUGUIRects[index].sizeDelta = size);
		Canvas.ForceUpdateCanvases();
	}
	private static void mutateFastSibling(PairScene scene, int count, int iteration)
	{
		visitSparse(scene.mFastImages.Length, count, index =>
		{
			if ((iteration & 1) == 0) scene.mFastImages[index].setAsLastSibling();
			else scene.mFastImages[index].setAsFirstSibling();
		});
		scene.mFastCanvas.flushFrameNow();
	}
	private static void mutateUGUISibling(PairScene scene, int count, int iteration)
	{
		visitSparse(scene.mUGUIRects.Length, count, index =>
		{
			if ((iteration & 1) == 0) scene.mUGUIRects[index].SetAsLastSibling();
			else scene.mUGUIRects[index].SetAsFirstSibling();
		});
		Canvas.ForceUpdateCanvases();
	}
	private static void mutateFastTextureSparse(PairScene scene, int count, int runLength, Texture textureA, Texture textureB, int iteration)
	{
		visitSparse(scene.mFastImages.Length, count, index =>
		{
			Texture baseTexture = runLength > 0 && ((index / runLength) & 1) != 0 ? textureB : textureA;
			Texture otherTexture = baseTexture == textureA ? textureB : textureA;
			scene.mFastImages[index].setTexture((iteration & 1) == 0 ? otherTexture : baseTexture);
		});
		scene.mFastCanvas.flushFrameNow();
	}
	private static void mutateUGUITextureSparse(PairScene scene, int count, int runLength, Texture textureA, Texture textureB, int iteration)
	{
		visitSparse(scene.mUGUIImages.Length, count, index =>
		{
			Texture baseTexture = runLength > 0 && ((index / runLength) & 1) != 0 ? textureB : textureA;
			Texture otherTexture = baseTexture == textureA ? textureB : textureA;
			scene.mUGUIImages[index].texture = (iteration & 1) == 0 ? otherTexture : baseTexture;
		});
		Canvas.ForceUpdateCanvases();
	}
	private static void visitSparse(int total, int count, Action<int> action)
	{
		count = Mathf.Min(Mathf.Max(count, 1), total);
		int step = Mathf.Max(total / count, 1);
		for (int index = 0, changed = 0; index < total && changed < count; index += step, ++changed) action(index);
	}
	private static CompareResult makeResult(string name, double fast, double ugui, int batchCount, string detail)
	{
		return new CompareResult { mName = name, mFastMS = fast, mUGUIMS = ugui, mFastBatchCount = batchCount, mDetail = detail };
	}
	private static void appendResult(StringBuilder builder, CompareResult result)
	{
		double ratio = result.mFastMS > 0.0 ? result.mUGUIMS / result.mFastMS : 0.0;
		string verdict = ratio < 0.90 ? "FastWorse" : ratio < 1.20 ? "NearParity" : "FastAdvantage";
		builder.AppendLine("[Weakness Compare] " + result.mName + " | Fast=" + format(result.mFastMS) + " ms | UGUI=" + format(result.mUGUIMS) +
			" ms | U/F=" + ratio.ToString("F2", CultureInfo.InvariantCulture) + "x | FastBatch=" + result.mFastBatchCount + " | Verdict=" + verdict +
			" | " + result.mDetail);
	}
	private static void appendSummary(StringBuilder builder, List<CompareResult> results)
	{
		int fastWorse = 0;
		int nearParity = 0;
		int fastAdvantage = 0;
		double worstRatio = double.MaxValue;
		string worstName = "None";
		string worstDetail = "";
		for (int i = 0; i < results.Count; ++i)
		{
			double ratio = results[i].mFastMS > 0.0 ? results[i].mUGUIMS / results[i].mFastMS : double.MaxValue;
			if (ratio < 0.90) ++fastWorse;
			else if (ratio < 1.20) ++nearParity;
			else ++fastAdvantage;
			if (ratio < worstRatio)
			{
				worstRatio = ratio;
				worstName = results[i].mName;
				worstDetail = results[i].mDetail;
			}
		}
		builder.AppendLine("[Weakness Summary] Cases=" + results.Count + " | FastWorse=" + fastWorse + " | NearParity=" + nearParity +
			" | FastAdvantage=" + fastAdvantage + " | Worst=" + worstName + " | WorstU/F=" +
			(worstRatio < double.MaxValue ? worstRatio.ToString("F2", CultureInfo.InvariantCulture) : "N/A") + "x | " + worstDetail);
		builder.AppendLine("[Weakness Guidance] Prioritize FastWorse first, then NearParity cases with meaningful absolute cost. Do not optimize extreme micro-cases if absolute time is already negligible.");
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
