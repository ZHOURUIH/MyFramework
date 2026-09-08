using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using TMPro;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Debug = UnityEngine.Debug;

// FastGUI publication benchmark.
// CPU: business mutation plus immediate FastCanvas/UGUI canvas update.
// Render: isolated FastGUI/UGUI roots with draw, geometry, upload, frame timing and wall-time metrics.
// The benchmark intentionally lives outside the FastGUI runtime assembly so production code has no UGUI dependency.
[DefaultExecutionOrder(32500)]
public class FastUIUGUIInventoryBenchmark : MonoBehaviour
{
	private const int BENCHMARK_FAST_SORTING_ORDER = 30000;
	private const int BENCHMARK_UGUI_SORTING_ORDER = 30001;
	// Publication workload is fixed so scene serialization cannot silently reduce test scale.
	protected const int PUBLICATION_ITEM_COUNT = 2400;
	protected const int PUBLICATION_VISIBLE_REFRESH_COUNT = 800;
	protected const int PUBLICATION_BATCH_REFRESH_COUNT = 1400;
	protected const int PUBLICATION_STRESS_REFRESH_COUNT = 2200;
	protected const int PUBLICATION_SCATTER_REFRESH_COUNT = 1400;
	protected const int PUBLICATION_SCATTER_STRIDE = 29;
	protected const int PUBLICATION_STRESS_MUTATION_PASSES = 2;
	protected const int QUICK_MIN_RENDER_SAMPLE_FRAMES = 2;
	protected const double QUICK_RENDER_WARMUP_BUDGET_MS = 80.0;
	protected const double QUICK_RENDER_PRIME_BUDGET_MS = 80.0;
	protected const double QUICK_RENDER_SAMPLE_BUDGET_MS = 250.0;
	protected const double MAX_PLAUSIBLE_GPU_FRAME_MS = 5000.0;
	protected const double MAX_PLAUSIBLE_CPU_FRAME_MS = 5000.0;
	protected const float RUNTIME_SIMPLE_QUAD_POSITION_TOLERANCE = 0.001f;
	protected const float RUNTIME_SIMPLE_QUAD_UV_TOLERANCE = 0.000001f;
	protected const double MAX_CPU_TO_WALL_RATIO = 1.75;
	protected const double CPU_TO_WALL_ABSOLUTE_TOLERANCE_MS = 50.0;
	protected const float BENCHMARK_COMPARISON_MARGIN_X = 16.0f;
	protected const float BENCHMARK_COMPARISON_MARGIN_Y = 16.0f;
	public enum BenchmarkExecutionProfile
	{
		Quick = 0,
		Publication = 1,
	}
	public BenchmarkExecutionProfile mBenchmarkProfile = BenchmarkExecutionProfile.Publication;
	public bool mForcePublicationProfile = true;
	public bool mRunLargePrefabLoadBenchmark = true;
	public enum InventoryCase
	{
		SingleItemRefresh,
		VisiblePageRefresh,
		FullInventoryRebind,
		QuantityTextRefresh,
		IconSpriteRefresh,
		ContentScroll,
		SelectionDetailRefresh,
		FilterToggle,
		WindowFastVisibility,
		WindowSetActive,
		MixedUse,
		IconPositionRefresh,
		ItemRootPositionRefresh,
		IconSizeRefresh,
		QualityColorRefresh,
		ScatteredIconSpriteRefresh,
		LeafVisibilityRefresh,
		TextColorRefresh,
		TextFontSizeRefresh,
		IconTransformRefresh,
		RectMaskResize,
		StressMixedRefresh,
		FullTextRefresh,
		FullStateRefresh,
		FullLeafVisibilityRefresh,
		ScatteredFilterToggle,
		StressIconGeometryRefresh,
		StressItemRootPositionRefresh,
		IconMaterialRefresh,
		RectMaskPaddingRefresh,
		RectMaskMoveResize,
	}
	protected enum BenchmarkTarget
	{
		FastUI,
		UGUI,
	}
	protected enum BenchmarkPhase
	{
		Waiting,
		Warmup,
		Sampling,
		Complete,
	}
	protected class FastItemView
	{
		public RectTransform mRoot;
		public FastUIVisibility mVisibility;
		public FastImage mBackground;
		public FastImage mIcon;
		public FastImage mQualityFrame;
		public FastImage mBindIcon;
		public FastImage mLockIcon;
		public FastImage mSelected;
		public FastText mCountText;
		public FastText mLevelText;
	}
	protected class UGUIItemView
	{
		public RectTransform mRoot;
		public Image mBackground;
		public Image mIcon;
		public Image mQualityFrame;
		public Image mBindIcon;
		public Image mLockIcon;
		public Image mSelected;
		public TextMeshProUGUI mCountText;
		public TextMeshProUGUI mLevelText;
	}
	protected class SparseNegativeValidationCase
	{
		public string mName;
		public string mExpectedError;
		public GameObject mRoot;
		public FastCanvas mCanvas;
		public FastSOARenderGroup mGroup;
	}
	protected struct SegmentResult
	{
		public int mFrames;
		public double mMutationMS;
		public double mFrameworkMS;
		public double mFastBatchCount;
		public double mFastRebuiltVertices;
		public double getMutationAverage() { return mFrames > 0 ? mMutationMS / mFrames : 0.0; }
		public double getFrameworkAverage() { return mFrames > 0 ? mFrameworkMS / mFrames : 0.0; }
		public double getScopedAverage() { return getMutationAverage() + getFrameworkAverage(); }
		public double getFastBatchAverage() { return mFrames > 0 ? mFastBatchCount / mFrames : 0.0; }
		public double getFastRebuiltVertexAverage() { return mFrames > 0 ? mFastRebuiltVertices / mFrames : 0.0; }
	}

	protected struct RenderSampleResult
	{
		public int mFrames;
		public double mDrawCalls;
		public double mBatches;
		public double mSetPassCalls;
		public double mTriangles;
		public double mVertices;
		public double mVertexUploadBytes;
		public double mIndexUploadBytes;
		public double mCPUFrameMS;
		public double mCPUMainThreadFrameMS;
		public double mCPURenderThreadFrameMS;
		public int mCPUFrameTimingFrames;
		public int mCPUInvalidTimingFrames;
		public double mGPUFrameMS;
		public int mGPUTimingFrames;
		public int mGPUInvalidTimingFrames;
		public double mWallFrameMS;
		public int mWallFrameSamples;
		public double mWallFrameMedianMS;
		public double mWallFrameP95MS;
		public double getDrawCallsAverage() { return mFrames > 0 ? mDrawCalls / mFrames : 0.0; }
		public double getBatchesAverage() { return mFrames > 0 ? mBatches / mFrames : 0.0; }
		public double getSetPassAverage() { return mFrames > 0 ? mSetPassCalls / mFrames : 0.0; }
		public double getTrianglesAverage() { return mFrames > 0 ? mTriangles / mFrames : 0.0; }
		public double getVerticesAverage() { return mFrames > 0 ? mVertices / mFrames : 0.0; }
		public double getVertexUploadBytesAverage() { return mFrames > 0 ? mVertexUploadBytes / mFrames : 0.0; }
		public double getIndexUploadBytesAverage() { return mFrames > 0 ? mIndexUploadBytes / mFrames : 0.0; }
		public double getCPUFrameAverage() { return mCPUFrameTimingFrames > 0 ? mCPUFrameMS / mCPUFrameTimingFrames : -1.0; }
		public double getCPUMainThreadFrameAverage() { return mCPUFrameTimingFrames > 0 ? mCPUMainThreadFrameMS / mCPUFrameTimingFrames : -1.0; }
		public double getCPURenderThreadFrameAverage() { return mCPUFrameTimingFrames > 0 ? mCPURenderThreadFrameMS / mCPUFrameTimingFrames : -1.0; }
		public double getGPUFrameAverage() { return mGPUTimingFrames > 0 ? mGPUFrameMS / mGPUTimingFrames : -1.0; }
		public double getWallFrameAverage() { return mWallFrameSamples > 0 ? mWallFrameMS / mWallFrameSamples : -1.0; }
		public double getWallFrameMedian() { return mWallFrameSamples > 0 ? mWallFrameMedianMS : -1.0; }
		public double getWallFrameP95() { return mWallFrameSamples > 0 ? mWallFrameP95MS : -1.0; }
	}
	protected class RenderGPUCalibrationRecord
	{
		public string mCaseName;
		public double mFastGPU;
		public double mUGUIGPU;
		public bool mComplete;
	}
	protected struct ComparisonResult
	{
		public double mFastScopedMS;
		public double mUGUIScopedMS;
		public double mFastSpread;
		public double mUGUISpread;
		public bool mStable;
	}
	protected static readonly InventoryCase[] TEST_CASES =
	{
		InventoryCase.SingleItemRefresh,
		InventoryCase.VisiblePageRefresh,
		InventoryCase.FullInventoryRebind,
		InventoryCase.QuantityTextRefresh,
		InventoryCase.IconSpriteRefresh,
		InventoryCase.ContentScroll,
		InventoryCase.SelectionDetailRefresh,
		InventoryCase.FilterToggle,
		InventoryCase.WindowFastVisibility,
		InventoryCase.WindowSetActive,
		InventoryCase.MixedUse,
		InventoryCase.IconPositionRefresh,
		InventoryCase.ItemRootPositionRefresh,
		InventoryCase.IconSizeRefresh,
		InventoryCase.QualityColorRefresh,
		InventoryCase.ScatteredIconSpriteRefresh,
		InventoryCase.LeafVisibilityRefresh,
		InventoryCase.TextColorRefresh,
		InventoryCase.TextFontSizeRefresh,
		InventoryCase.IconTransformRefresh,
		InventoryCase.RectMaskResize,
		InventoryCase.StressMixedRefresh,
		InventoryCase.FullTextRefresh,
		InventoryCase.FullStateRefresh,
		InventoryCase.FullLeafVisibilityRefresh,
		InventoryCase.ScatteredFilterToggle,
		InventoryCase.StressIconGeometryRefresh,
		InventoryCase.StressItemRootPositionRefresh,
		InventoryCase.IconMaterialRefresh,
		InventoryCase.RectMaskPaddingRefresh,
		InventoryCase.RectMaskMoveResize,
	};
	protected static readonly BenchmarkTarget[] FULL_SANDWICH_ORDER =
	{
		BenchmarkTarget.FastUI,
		BenchmarkTarget.UGUI,
		BenchmarkTarget.UGUI,
		BenchmarkTarget.FastUI,
	};
	protected static readonly BenchmarkTarget[] FAST_SANDWICH_ORDER =
	{
		BenchmarkTarget.FastUI,
		BenchmarkTarget.UGUI,
	};
	protected static readonly double TICK_TO_MS = 1000.0 / Stopwatch.Frequency;
	public Texture mTextureA;
	public Texture mTextureB;
	public Material mImageMaterial;
	public TMP_FontAsset mFont;
	public FastUIInventoryBenchmarkAtlas mInventoryAtlas;
	public int mItemCount = 1000;
	public int mColumnCount = 10;
	public int mVisibleRefreshCount = 200;
	public int mBatchRefreshCount = 300;
	// Stress covers large composite mutations; scatter covers sparse hierarchy/index updates.
	public int mStressRefreshCount = 600;
	public int mScatterRefreshCount = 300;
	public int mScatterStride = 7;
	public Vector2 mItemSize = new(72.0f, 72.0f);
	public Vector2 mItemInterval = new(76.0f, 76.0f);
	public Vector2 mViewportSize = new(800.0f, 900.0f);
	public Vector2 mWindowSize = new(1180.0f, 1080.0f);
	public float mScrollStep = 13.0f;
	public float mStartDelay = 1.0f;
	public float mWarmupTime = 0.25f;
	public float mSampleTime = 0.75f;
	public bool mAutoBenchmark = true;
	public bool mFastEnableSOA = true;
	public bool mFastEnableSuggestedSOA = true;
	public bool mFastEnableAdjacentSOAMerge = true;
	public bool mRunSparseSOAValidation = true;
	public bool mRunSparseSOANegativeValidation = true;
	public bool mRunNestedSOAValidation = true;
	public bool mRunRenderValidation = true;
	public bool mLogEndToEndFrameTime = true;
	public bool mLogRenderCheckpoints = true;
	public float mValidationTimeout = 10.0f;
	public int mFastRenderDrawCallAbortThreshold = 512;
	public bool mRunRenderDiagnosis = true;
	public int mRenderWarmupFrames = 30;
	public int mRenderSampleFrames = 120;
	public int mGPUPrimeFrames = 8;
	public bool mRunGPUWarmStateRepeatValidation = true;
	public int mGPUMeasurementBaselineWarmupFrames = 10;
	public int mGPUMeasurementBaselineSampleFrames = 60;
	public int mGPUMeasurementBaselinePrimeFrames = 4;
	public float mGPUMeasurementFloorToleranceMS = 0.5f;
	public float mGPUMeasurementFloorTolerancePercent = 3.0f;
	// 仅保留公开对比所需的CPU、Render与GPU测量状态。
	public string mCppCompilerConfigurationTag = "Master";
	protected float mComparisonDisplayScale = 1.0f;
	protected float mComparisonWindowOffsetX;
	protected GameObject mFastRoot;
	protected FastCanvas mFastCanvas;
	protected Camera mBenchmarkCamera;
	protected RectTransform mFastWindowRect;
	protected FastImage mFastWindowBackground;
	protected RectTransform mFastContent;
	protected FastSOARenderGroup mFastSOAGroup;
	protected FastSOARenderGroup mFastCurrencySOAGroup;
	protected FastSOARenderGroup mFastTabsSOAGroup;
	protected RectTransform mFastViewportRect;
	protected FastRectMask2D mFastViewportClip;
	protected SparseNegativeValidationCase[] mSparseNegativeValidationCases;
	protected bool mSparseNegativeValidationComplete;
	protected bool mSparseNegativeValidationValid = true;
	protected GameObject mSparseSOAValidationRoot;
	protected FastCanvas mSparseSOAValidationCanvas;
	protected FastSOARenderGroup mSparseSOAValidationGroup;
	protected FastRawImage[] mSparseSOAValidationA;
	protected FastRawImage[] mSparseSOAValidationB;
	protected FastRawImage[] mSparseSOAValidationX;
	protected FastRawImage[] mSparseSOAValidationC;
	protected FastRawImage[] mSparseSOAValidationD;
	protected bool mSparseSOAValidationComplete;
	protected bool mSparseSOAValidationValid = true;
	protected GameObject mNestedSOAValidationRoot;
	protected FastCanvas mNestedSOAValidationCanvas;
	protected FastSOARenderGroup mNestedSOAValidationOuterGroup;
	protected FastSOARenderGroup[] mNestedSOAValidationInnerGroups;
	protected FastRawImage[] mNestedSOAValidationA;
	protected FastRawImage[] mNestedSOAValidationB;
	protected FastRawImage[] mNestedSOAValidationC;
	protected bool mNestedSOAValidationComplete;
	protected bool mNestedSOAValidationValid = true;
	protected FastItemView[] mFastItems;
	protected FastImage mFastDetailIcon;
	protected FastText mFastDetailName;
	protected FastText mFastDetailDescription;
	protected GameObject mUGUIRoot;
	protected RectTransform mUGUIWindowRect;
	protected Canvas mUGUICanvas;
	protected RectTransform mUGUIViewportRect;
	protected RectTransform mUGUIContent;
	protected UGUIItemView[] mUGUIItems;
	protected Image mUGUIDetailIcon;
	protected TextMeshProUGUI mUGUIDetailName;
	protected TextMeshProUGUI mUGUIDetailDescription;
	protected string[] mNumberStrings;
	protected string[] mItemNames;
	protected string[] mItemDescriptions;
	protected BenchmarkPhase mPhase = BenchmarkPhase.Waiting;
	protected int mCaseIndex = -1;
	protected int mSegmentIndex;
	protected int mMutationStep;
	protected int mFastSelectedIndex = -1;
	protected int mUGUISelectedIndex = -1;
	protected float mReadyTime;
	protected float mPhaseStartTime;
	protected double mLastMutationMS;
	protected double mLastUGUIForceMS;
	protected SegmentResult mCurrentSegment;
	protected SegmentResult[] mSegmentResults = new SegmentResult[4];
	protected ComparisonResult[] mComparisonResults;
	protected int mFastRectCount;
	protected int mFastRenderElementCount;
	protected int mFastTextCount;
	protected int mFastHierarchyDepth;
	protected int mUGUIRectCount;
	protected int mFastImageCount;
	protected int mFastRawImageCount;
	protected int mUGUIGraphicCount;
	protected int mUGUIImageCount;
	protected int mUGUIRawImageCount;
	protected int mUGUITextCount;
	protected int mUGUICanvasRendererCount;
	protected int mUGUIHierarchyDepth;
	protected ProfilerRecorder mDrawCallsRecorder;
	protected ProfilerRecorder mBatchesRecorder;
	protected ProfilerRecorder mSetPassRecorder;
	protected ProfilerRecorder mTrianglesRecorder;
	protected ProfilerRecorder mVerticesRecorder;
	protected ProfilerRecorder mVertexUploadBytesRecorder;
	protected ProfilerRecorder mIndexUploadBytesRecorder;
	protected bool mRenderRecordersStarted;
	protected readonly FrameTiming[] mFrameTimings = new FrameTiming[1];
	protected bool mFrameTimingEnabled;
	protected bool mGPUFrameTimingAvailable;
	protected RenderSampleResult mGPUMeasurementBaselineStart;
	protected RenderSampleResult mGPUMeasurementBaselineEnd;
	protected bool mGPUMeasurementBaselineStartValid;
	protected bool mGPUMeasurementBaselineEndValid;
	protected readonly List<RenderGPUCalibrationRecord> mRenderGPUCalibrationRecords = new(64);
	protected bool mAllBenchmarkComplete;
	protected GameObject mBenchmarkCompleteRoot;
	protected Material mBenchmarkMaterialA;
	protected Material mBenchmarkMaterialB;
	protected RectMask2D mUGUIViewportMask;
	protected readonly StringBuilder mBenchmarkAnalysisLog = new(65536);
	protected string mBenchmarkAnalysisLogPath;
	protected bool mBenchmarkAnalysisLogFlushed;
	protected StackTraceLogType mPreviousBenchmarkLogStackTraceType;
	protected bool mBenchmarkLogStackTraceOverridden;
	protected bool mInitializationComplete;
	protected bool mBenchmarkAborted;
	protected float mValidationWaitStartTime;
	protected float mInitializationStartTime;
	protected double mFastCreateScaffoldMS;
	protected double mFastCreateItemsMS;
	protected double mFastCreateDetailMS;
	protected double mUGUICreateScaffoldMS;
	protected double mUGUICreateItemsMS;
	protected double mUGUICreateDetailMS;
	protected float mBenchmarkSuiteStartTime;
	protected int mLastInitializationProgress;
	public bool mShowBenchmarkCompleteScreen = true;
	public float mBenchmarkCompleteFontSize = 72.0f;
	protected void reportInitializationProgress(string stage, int completed, int total, bool force)
	{
		int safeTotal = Mathf.Max(total, 1);
		mLastInitializationProgress = completed;
		float elapsed = Time.realtimeSinceStartup - mInitializationStartTime;
		benchmarkLog("[INIT] Stage=" + stage + " | " + completed + "/" + safeTotal + " | " +
			(completed * 100.0f / safeTotal).ToString("F1") + "% | Elapsed=" + elapsed.ToString("F2") + "s");
	}

	protected void applyPublicationWorkload()
	{
		mItemCount = PUBLICATION_ITEM_COUNT;
		mVisibleRefreshCount = PUBLICATION_VISIBLE_REFRESH_COUNT;
		mBatchRefreshCount = PUBLICATION_BATCH_REFRESH_COUNT;
		mStressRefreshCount = PUBLICATION_STRESS_REFRESH_COUNT;
		mScatterRefreshCount = PUBLICATION_SCATTER_REFRESH_COUNT;
		mScatterStride = PUBLICATION_SCATTER_STRIDE;
	}
	protected BenchmarkTarget[] getActiveSandwichOrder()
	{
		return mBenchmarkProfile == BenchmarkExecutionProfile.Quick ? FAST_SANDWICH_ORDER : FULL_SANDWICH_ORDER;
	}
	protected bool isPublicationProfile() { return mBenchmarkProfile == BenchmarkExecutionProfile.Publication; }
	protected void applyBenchmarkExecutionProfile()
	{
		if (isPublicationProfile())
		{
			mStartDelay = 0.10f;
			mWarmupTime = 0.05f;
			mSampleTime = 0.20f;
			mRenderWarmupFrames = 3;
			mRenderSampleFrames = 20;
			mGPUPrimeFrames = 2;
			mGPUMeasurementBaselineWarmupFrames = 2;
			mGPUMeasurementBaselineSampleFrames = 16;
			mGPUMeasurementBaselinePrimeFrames = 2;
			mRunGPUWarmStateRepeatValidation = false;
			return;
		}
		mStartDelay = 0.05f;
		mWarmupTime = 0.02f;
		mSampleTime = 0.10f;
		mRenderWarmupFrames = 1;
		mRenderSampleFrames = 6;
		mGPUPrimeFrames = 1;
		mGPUMeasurementBaselineWarmupFrames = 1;
		mGPUMeasurementBaselineSampleFrames = 6;
		mGPUMeasurementBaselinePrimeFrames = 1;
	}
	protected int getGPUWarmRepeatWarmupFrames() { return 1; }
	protected int getGPUWarmRepeatSampleFrames() { return 8; }
	protected int getGPUWarmRepeatPrimeFrames() { return 1; }
	protected bool reachedQuickFrameBudget(long startTick, int completedFrames, double budgetMS, int minimumFrames)
	{
		return mBenchmarkProfile == BenchmarkExecutionProfile.Quick && completedFrames >= minimumFrames &&
			(System.Diagnostics.Stopwatch.GetTimestamp() - startTick) * TICK_TO_MS >= budgetMS;
	}
	protected bool isPlausibleCPUFrameTime(double cpuMS, double wallMS)
	{
		if (double.IsNaN(cpuMS) || double.IsInfinity(cpuMS) || cpuMS <= 0.0 || cpuMS > MAX_PLAUSIBLE_CPU_FRAME_MS)
		{
			return false;
		}
		if (wallMS <= 0.0)
		{
			return true;
		}
		double maxByWall = Math.Max(wallMS * MAX_CPU_TO_WALL_RATIO, wallMS + CPU_TO_WALL_ABSOLUTE_TOLERANCE_MS);
		return cpuMS <= maxByWall;
	}
	protected bool isPlausibleGPUFrameTime(double gpuMS, double cpuMS)
	{
		if (double.IsNaN(gpuMS) || double.IsInfinity(gpuMS) || gpuMS <= 0.0)
		{
			return false;
		}
		double maxMS = Math.Max(MAX_PLAUSIBLE_GPU_FRAME_MS, cpuMS > 0.0 ? cpuMS * 8.0 : 0.0);
		return gpuMS <= maxMS;
	}
	protected void logBenchmarkExecutionProfile()
	{
		benchmarkLog("[Benchmark Profile] Mode=" + mBenchmarkProfile +
			" | Purpose=" + (isPublicationProfile() ? "PublicationComparison" : "QuickCheck") +
			" | TargetMaxDuration=" + (isPublicationProfile() ? "600s" : "220s") +
			" | Cases=All | ItemScale=2400" +
			" | CPUWarmup=" + mWarmupTime.ToString("F2") + "s | CPUSample=" + mSampleTime.ToString("F2") + "s" +
			" | RenderWarmup=" + mRenderWarmupFrames + " | RenderSample=" + mRenderSampleFrames + " | GPUPrime=" + mGPUPrimeFrames +
			" | Sandwich=" + (mBenchmarkProfile == BenchmarkExecutionProfile.Quick ? "F/U" : "F/U/U/F") +
			" | RenderDiagnosis=" + mRunRenderDiagnosis);
		benchmarkLog("[README Matrix] CPUCases=" + TEST_CASES.Length + " | RenderCases=13 | Creation=OneFrameCreate+FirstFullFlush+Ready" +
			" | RenderMetrics=WallMedian+WallP95+GPU+DrawCalls+SetPass+Triangles+Vertices+VBUpload+IBUpload" +
			" | Sandwich=" + (mBenchmarkProfile == BenchmarkExecutionProfile.Quick ? "F/U" : "F/U/U/F"));
	}
	protected void initializeBenchmarkAnalysisLog()
	{
		mBenchmarkAnalysisLog.Clear();
		mBenchmarkAnalysisLogPath = null;
		mBenchmarkAnalysisLogFlushed = false;
		if (!mBenchmarkLogStackTraceOverridden)
		{
			mPreviousBenchmarkLogStackTraceType = Application.GetStackTraceLogType(LogType.Log);
			Application.SetStackTraceLogType(LogType.Log, StackTraceLogType.None);
			mBenchmarkLogStackTraceOverridden = true;
		}
		UnityEngine.Debug.Log("================ FastGUI Publication Benchmark ================");
		benchmarkLog("[Benchmark Environment] Device=" + SystemInfo.deviceModel + " | OS=" + SystemInfo.operatingSystem +
			" | CPU=" + SystemInfo.processorType + " | Cores=" + SystemInfo.processorCount + " | RAM=" + SystemInfo.systemMemorySize + "MB" +
			" | GPU=" + SystemInfo.graphicsDeviceName + " | API=" + SystemInfo.graphicsDeviceType + " | Unity=" + Application.unityVersion +
			" | IL2CPP=" + (Application.platform != RuntimePlatform.WindowsEditor && Application.platform != RuntimePlatform.OSXEditor && Application.platform != RuntimePlatform.LinuxEditor) +
			" | DevelopmentBuild=" + Debug.isDebugBuild);
		benchmarkLog("[README Environment] Device=" + SystemInfo.deviceModel + " | OS=" + SystemInfo.operatingSystem +
			" | CPU=" + SystemInfo.processorType + " | Cores=" + SystemInfo.processorCount + " | RAM=" + SystemInfo.systemMemorySize + "MB" +
			" | GPU=" + SystemInfo.graphicsDeviceName + " | API=" + SystemInfo.graphicsDeviceType + " | Unity=" + Application.unityVersion +
			" | DevelopmentBuild=" + Debug.isDebugBuild);
	}
	protected void restoreBenchmarkLogStackTrace()
	{
		if (!mBenchmarkLogStackTraceOverridden)
		{
			return;
		}
		Application.SetStackTraceLogType(LogType.Log, mPreviousBenchmarkLogStackTraceType);
		mBenchmarkLogStackTraceOverridden = false;
	}
	protected void benchmarkLog(object message)
	{
		string text = message != null ? message.ToString() : "null";
		appendFilteredBenchmarkAnalysis(text, false);
		UnityEngine.Debug.Log(text);
	}
	protected void benchmarkLogWarning(object message)
	{
		string text = message != null ? message.ToString() : "null";
		appendFilteredBenchmarkAnalysis(text, true);
		UnityEngine.Debug.LogWarning(text);
	}
	protected void benchmarkLogError(object message)
	{
		string text = message != null ? message.ToString() : "null";
		appendFilteredBenchmarkAnalysis(text, true);
		UnityEngine.Debug.LogError(text);
	}
	protected void appendFilteredBenchmarkAnalysis(string text, bool warning)
	{
		if (!isPublicationProfile() || string.IsNullOrEmpty(text)) { return; }
		string[] lines = text.Replace("\r", string.Empty).Split('\n');
		for (int i = 0; i < lines.Length; ++i)
		{
			string line = lines[i].Trim();
			if (shouldKeepBenchmarkAnalysisLine(line)) { appendBenchmarkAnalysisLine((warning ? "[Warning] " : string.Empty) + line); }
		}
	}
	protected bool shouldKeepBenchmarkAnalysisLine(string line)
	{
		if (string.IsNullOrEmpty(line)) { return false; }
		return line.StartsWith("[README ") || line.StartsWith("[Benchmark Environment]") ||
			line.StartsWith("[Prefab AssetLoad]") || line.StartsWith("[Prefab Instantiate]") || line.StartsWith("[Prefab FirstFullFlush]") ||
			line.StartsWith("[Prefab InstantiateToReady]") || line.StartsWith("[Prefab LoadToReady]") ||
			line.StartsWith("[Clone Compare]") || line.StartsWith("[Clone Utility]") ||
			line.StartsWith("[SpriteImage Compare]") || line.StartsWith("[SpriteImage Summary]") || line.StartsWith("[Weakness Compare]") || line.StartsWith("[Weakness Summary]") ||
			line.StartsWith("[FastUI vs UGUI EndToEnd]") || line.StartsWith("[FastUI Render Diagnosis]") || line.StartsWith("[UGUI Render Diagnosis]") ||
			line.StartsWith("[Inventory Sprite Usage]") || line.StartsWith("[Index Boundary Assert]") || line.StartsWith("[FastUI GPU Measurement Calibration]");
	}
	protected void appendBenchmarkAnalysisLine(string line)
	{
		if (mBenchmarkAnalysisLog.Length > 0) { mBenchmarkAnalysisLog.Append('\n'); }
		mBenchmarkAnalysisLog.Append(line);
	}
	protected void flushBenchmarkAnalysisCheckpoint() { }
	protected void flushBenchmarkAnalysisLog()
	{
		if (!isPublicationProfile() || mBenchmarkAnalysisLogFlushed) { return; }
		mBenchmarkAnalysisLogFlushed = true;
		UnityEngine.Debug.Log("================ README BENCHMARK DATA BEGIN ================");
		string[] lines = mBenchmarkAnalysisLog.ToString().Split('\n');
		for (int i = 0; i < lines.Length; ++i)
		{
			if (!string.IsNullOrEmpty(lines[i])) { UnityEngine.Debug.Log(lines[i]); }
		}
		UnityEngine.Debug.Log("================ README BENCHMARK DATA END ==================");
	}

	protected IEnumerator Start()
	{
		mBenchmarkSuiteStartTime = Time.realtimeSinceStartup;
		QualitySettings.vSyncCount = 0;
		Application.targetFrameRate = -1;
		if (mForcePublicationProfile) { mBenchmarkProfile = BenchmarkExecutionProfile.Publication; }
		initializeBenchmarkAnalysisLog();
		applyPublicationWorkload();
		applyBenchmarkExecutionProfile();
		logBenchmarkExecutionProfile();
		if (!configureBenchmarkCameraForVisualParity())
		{
			abortBenchmark("视觉一致性初始化失败：Benchmark需要一个启用的正交MainCamera。");
			yield break;
		}
		mInitializationComplete = false;
		mInitializationStartTime = Time.realtimeSinceStartup;
		reportInitializationProgress("Begin", 0, mItemCount, true);
		mItemCount = Mathf.Clamp(mItemCount, 1, 5000);
		mColumnCount = Mathf.Clamp(mColumnCount, 1, mItemCount);
		mVisibleRefreshCount = Mathf.Clamp(mVisibleRefreshCount, 1, mItemCount);
		mBatchRefreshCount = Mathf.Clamp(mBatchRefreshCount, 1, mItemCount);
		mStressRefreshCount = Mathf.Clamp(mStressRefreshCount, 1, mItemCount);
		mScatterRefreshCount = Mathf.Clamp(mScatterRefreshCount, 1, mItemCount);
		mScatterStride = Mathf.Max(mScatterStride, 1);
		if (mTextureA == null || mFont == null || !isInventoryAtlasValid())
		{
			abortBenchmark("必须设置RawImage专项Texture、TMP_FontAsset和正常背包Sprite Atlas。请重新执行Tools/FastGUI Benchmark/Generate Inventory Benchmark。");
			yield break;
		}
		if (TMP_Settings.instance == null)
		{
			abortBenchmark("TMP Settings不存在，请先导入TextMesh Pro Essential Resources.");
			yield break;
		}
		if (mTextureB == null)
		{
			mTextureB = mTextureA;
		}
		if (mRunLargePrefabLoadBenchmark)
		{
			yield return FastUIPrefabLoadBenchmark.run(benchmarkLog);
		}
		Material materialSource = mImageMaterial != null ? mImageMaterial : Graphic.defaultGraphicMaterial;
		if (materialSource != null)
		{
			mBenchmarkMaterialA = new Material(materialSource);
			mBenchmarkMaterialA.name = "FastGUI_Benchmark_Material_A";
			mBenchmarkMaterialA.hideFlags = HideFlags.HideAndDontSave;
			mBenchmarkMaterialB = new Material(materialSource);
			mBenchmarkMaterialB.name = "FastGUI_Benchmark_Material_B";
			mBenchmarkMaterialB.hideFlags = HideFlags.HideAndDontSave;
		}
		prepareStrings();
		mFastItems = new FastItemView[mItemCount];
		mUGUIItems = new UGUIItemView[mItemCount];
		mComparisonResults = new ComparisonResult[TEST_CASES.Length];
		reportInitializationProgress("OneFrameCreate.Begin", 0, mItemCount, true);
		long fastCreateStart = Stopwatch.GetTimestamp();
		createFastInventoryOneFrame();
		double fastCreateMS = (Stopwatch.GetTimestamp() - fastCreateStart) * TICK_TO_MS;
		benchmarkLog("[Creation] Framework=FastGUI | Items=" + mItemCount + " | Create=" + fastCreateMS.ToString("F4") +
			"ms | RenderElements=" + mFastCanvas.getRenderElementCount() + " | DeferredPending=" + mFastCanvas.hasDeferredStructureChanges());

		long uguiCreateStart = Stopwatch.GetTimestamp();
		createUGUIInventoryOneFrame();
		double uguiCreateMS = (Stopwatch.GetTimestamp() - uguiCreateStart) * TICK_TO_MS;
		benchmarkLog("[Creation] Framework=UGUI | Items=" + mItemCount + " | Create=" + uguiCreateMS.ToString("F4") +
			"ms | Graphics=" + mUGUIRoot.GetComponentsInChildren<Graphic>(true).Length);

		long fastFlushStart = Stopwatch.GetTimestamp();
		FastUIFrameStats fastInitStats = mFastCanvas.flushFrameNow();
		double fastFirstFlushMS = (Stopwatch.GetTimestamp() - fastFlushStart) * TICK_TO_MS;
		long uguiFlushStart = Stopwatch.GetTimestamp();
		Canvas.ForceUpdateCanvases();
		double uguiFirstFlushMS = (Stopwatch.GetTimestamp() - uguiFlushStart) * TICK_TO_MS;
		benchmarkLog("[Creation FirstFullFlush] FastGUI=" + fastFirstFlushMS.ToString("F4") + "ms | UGUI=" + uguiFirstFlushMS.ToString("F4") +
			"ms | U/F=" + (fastFirstFlushMS > 0.000001 ? (uguiFirstFlushMS / fastFirstFlushMS).ToString("F2") : "N/A") + "x" +
			" | FastCanvasCPU=" + fastInitStats.mCPUTimeMS.ToString("F4") + "ms");
		double fastReadyMS = fastCreateMS + fastFirstFlushMS;
		double uguiReadyMS = uguiCreateMS + uguiFirstFlushMS;
		FastUIMeshRenderer initRenderer = mFastCanvas.getMeshRenderer();
		int logicalBatchCount = initRenderer != null ? initRenderer.getLogicalBatchCount() : 0;
		int submitDrawRunCount = initRenderer != null ? initRenderer.getBatchCount() : 0;
		int activeSubMeshCount = initRenderer != null ? initRenderer.getActiveSubMeshCount() : 0;
		int meshSubMeshCount = initRenderer != null ? initRenderer.getMeshSubMeshCount() : 0;
		int materialSlotCount = initRenderer != null ? initRenderer.getAppliedMaterialSlotCount() : 0;
		int subMeshCapacity = initRenderer != null ? initRenderer.getSubMeshCapacity() : 0;
		int expectedActiveSlots = Mathf.Max(submitDrawRunCount, 1);
		bool batchSlotValid = activeSubMeshCount == expectedActiveSlots && meshSubMeshCount == expectedActiveSlots &&
			materialSlotCount == expectedActiveSlots;
		benchmarkLog("[Batch Slot Validation] LogicalBatch=" + logicalBatchCount + " | SubmitDrawRuns=" + submitDrawRunCount +
			" | ActiveSubMeshes=" + activeSubMeshCount + " | MeshSubMeshes=" + meshSubMeshCount +
			" | MaterialSlots=" + materialSlotCount + " | Capacity=" + subMeshCapacity +
			" | ExpectedSubmitSlots=" + expectedActiveSlots + " | Valid=" + batchSlotValid);
		benchmarkLog("[Creation TotalReady] FastGUI=" + fastReadyMS.ToString("F4") +
			"ms | UGUI=" + uguiReadyMS.ToString("F4") + "ms | UGUI/Fast=" +
			(fastReadyMS > 0.000001 ? (uguiReadyMS / fastReadyMS).ToString("F2") : "0.00") + "x" +
			" | Order=FastGUIThenUGUI");
		benchmarkLog("[README Creation] Items=" + mItemCount + " | CreateFast=" + fastCreateMS.ToString("F4") + "ms | CreateUGUI=" + uguiCreateMS.ToString("F4") + "ms | CreateU/F=" + (fastCreateMS > 0.000001 ? (uguiCreateMS / fastCreateMS).ToString("F2") : "N/A") + "x | FirstFlushFast=" + fastFirstFlushMS.ToString("F4") + "ms | FirstFlushUGUI=" + uguiFirstFlushMS.ToString("F4") + "ms | FlushU/F=" + (fastFirstFlushMS > 0.000001 ? (uguiFirstFlushMS / fastFirstFlushMS).ToString("F2") : "N/A") + "x | ReadyFast=" + fastReadyMS.ToString("F4") + "ms | ReadyUGUI=" + uguiReadyMS.ToString("F4") + "ms | ReadyU/F=" + (fastReadyMS > 0.000001 ? (uguiReadyMS / fastReadyMS).ToString("F2") : "N/A") + "x");
		if (mRunSparseSOAValidation)
		{
			createSparseSOAValidation();
		}
		else
		{
			mSparseSOAValidationComplete = true;
		}
		if (mRunSparseSOANegativeValidation)
		{
			createSparseSOANegativeValidation();
		}
		else
		{
			mSparseNegativeValidationComplete = true;
		}
		if (mRunNestedSOAValidation)
		{
			createNestedSOAValidation();
		}
		else
		{
			mNestedSOAValidationComplete = true;
		}
		reportInitializationProgress("Validation", mItemCount, mItemCount, true);
		collectHierarchyStats();
		resetBothToBaseline();
		mFastCanvas.flushFrameNow();
		Canvas.ForceUpdateCanvases();
		yield return null;
		if (!validateBenchmarkVisualLayout())
		{
			abortBenchmark("视觉布局验证失败：FastGUI与UGUI没有形成等尺寸、左右镜像的可视窗口，停止性能测试。");
			yield break;
		}
		if (!validateFastVertexCoordinateSpace())
		{
			abortBenchmark("顶点坐标验证失败：RectTransform与FastGUI Position Stream不一致，停止性能测试。请根据[Vertex]日志定位坐标空间问题。");
			yield break;
		}
		logEnvironment();
		if (!validateNormalSpriteUsage() || !validateFastClipStencil() || !validateBenchmarkEnvironment())
		{
			abortBenchmark("初始化环境或FastClipStencil验证失败.");
			yield break;
		}
		FastUIMeshRenderer fastRenderer = mFastCanvas != null ? mFastCanvas.getMeshRenderer() : null;
		int fastMeshVertexCount = fastRenderer != null ? fastRenderer.getMeshVertexCount() : 0;
		int fastLiveVertexCount = fastRenderer != null ? fastRenderer.getLiveVertexCount() : 0;
		int fastCurrentIndexCount = fastRenderer != null ? fastRenderer.getCurrentIndexCount() : 0;
		int fastLogicalBatchCount = fastRenderer != null ? fastRenderer.getLogicalBatchCount() : 0;
		int fastSubmitDrawRunCount = fastRenderer != null ? fastRenderer.getBatchCount() : 0;
		bool fastRendererEnabled = fastRenderer != null && fastRenderer.getRenderer() != null && fastRenderer.getRenderer().enabled;
		bool fastRenderValid = fastLiveVertexCount > 4 && fastCurrentIndexCount > 6 && fastLogicalBatchCount > 0 && fastSubmitDrawRunCount > 0 && fastRendererEnabled;
		benchmarkLog("[Init Render Validation] MeshVertexCount=" + fastMeshVertexCount +
			" | LiveVertices=" + fastLiveVertexCount + " | CurrentIndices=" + fastCurrentIndexCount +
			" | LogicalBatch=" + fastLogicalBatchCount + " | SubmitDrawRuns=" + fastSubmitDrawRunCount + " | RendererEnabled=" + fastRendererEnabled +
			" | VertexBackend=" + (fastRenderer != null ? fastRenderer.getVertexUploadBackend().ToString() : "None") +
			" | GPUIndexCapacity=" + (fastRenderer != null ? fastRenderer.getGPUIndexCapacity() : 0) +
			" | IndexFormat=" + (fastRenderer != null ? fastRenderer.getGPUIndexFormat().ToString() : "None") +
			" | IndexStride=" + (fastRenderer != null ? fastRenderer.getGPUIndexStride() : 0) +
			" | IndexUpload=MeshCPUCopy" +
			" | RenderElements=" + (mFastCanvas != null ? mFastCanvas.getRenderElementCount() : 0) +
			" | Valid=" + fastRenderValid);
		if (!batchSlotValid || !fastRenderValid)
		{
			abortBenchmark("初始化Renderer或Batch Slot验证失败.");
			yield break;
		}
		mInitializationComplete = true;
		reportInitializationProgress("Ready", mItemCount, mItemCount, true);
		mValidationWaitStartTime = Time.realtimeSinceStartup;
		mReadyTime = Time.realtimeSinceStartup + Mathf.Max(mStartDelay, 0.0f);
	}
	protected void Update()
	{
		mLastMutationMS = 0.0;
		mLastUGUIForceMS = 0.0;
		if (!mInitializationComplete || !mAutoBenchmark || mPhase == BenchmarkPhase.Waiting || mPhase == BenchmarkPhase.Complete || mCaseIndex < 0)
		{
			return;
		}
		BenchmarkTarget[] sandwichOrder = getActiveSandwichOrder();
		BenchmarkTarget target = sandwichOrder[mSegmentIndex];
		long mutationStart = Stopwatch.GetTimestamp();
		executeMutation(TEST_CASES[mCaseIndex], target);
		mLastMutationMS = (Stopwatch.GetTimestamp() - mutationStart) * TICK_TO_MS;
		if (target == BenchmarkTarget.UGUI)
		{
			long forceStart = Stopwatch.GetTimestamp();
			Canvas.ForceUpdateCanvases();
			mLastUGUIForceMS = (Stopwatch.GetTimestamp() - forceStart) * TICK_TO_MS;
		}
		++mMutationStep;
	}
	protected void LateUpdate()
	{
		if (!mInitializationComplete || !mAutoBenchmark || mPhase == BenchmarkPhase.Complete)
		{
			return;
		}
		float now = Time.realtimeSinceStartup;
		if (mPhase == BenchmarkPhase.Waiting)
		{
			if (now - mValidationWaitStartTime > Mathf.Max(mValidationTimeout, 1.0f))
			{
				abortBenchmark("前置Validation等待超时:" + (now - mValidationWaitStartTime).ToString("F2") + "s");
				return;
			}
			if (!mSparseSOAValidationComplete)
			{
				if (!tryCompleteSparseSOAValidation())
				{
					return;
				}
				if (!mSparseSOAValidationValid)
				{
					abortBenchmark("Sparse SOA Validation失败.");
					return;
				}
			}
			if (!mSparseNegativeValidationComplete)
			{
				if (!tryCompleteSparseSOANegativeValidation())
				{
					return;
				}
				if (!mSparseNegativeValidationValid)
				{
					abortBenchmark("Sparse SOA Negative Validation失败.");
					return;
				}
			}
			if (!mNestedSOAValidationComplete)
			{
				if (!tryCompleteNestedSOAValidation())
				{
					return;
				}
				if (!mNestedSOAValidationValid)
				{
					abortBenchmark("Nested SOA Validation失败.");
					return;
				}
			}
			if (now < mReadyTime || mFastCanvas == null || mFastCanvas.getRenderElementCount() <= 0 || mFastCanvas.getBatchCount() <= 0)
			{
				return;
			}
			if (!validateFastSOA() || !validateFastSuggestedSOABatchResult() || !validateFastClipStencilRenderOrder())
			{
				abortBenchmark("正式Inventory SOA/Batch/Stencil RenderOrder验证失败.");
				return;
			}
			beginCase(0);
			return;
		}
		if (mPhase == BenchmarkPhase.Warmup)
		{
			if (now - mPhaseStartTime >= Mathf.Max(mWarmupTime, 0.0f))
			{
				beginSampling();
			}
			return;
		}
		accumulateSampleFrame(getActiveSandwichOrder()[mSegmentIndex]);
		if (now - mPhaseStartTime < Mathf.Max(mSampleTime, 0.1f))
		{
			return;
		}
		finishSegment();
	}
	public void restartBenchmark()
	{
		if (!Application.isPlaying || !mInitializationComplete || mFastItems == null || mUGUIItems == null)
		{
			return;
		}
		mBenchmarkAborted = false;
		mAutoBenchmark = true;
		mPhase = BenchmarkPhase.Waiting;
		mCaseIndex = -1;
		mSegmentIndex = 0;
		mReadyTime = Time.realtimeSinceStartup + Mathf.Max(mStartDelay, 0.0f);
		resetBothToBaseline();
		Canvas.ForceUpdateCanvases();
		benchmarkLog("[FastUI vs UGUI Inventory] 重新开始Benchmark.");
	}
	protected void logCPUCheckpoint(string phase)
	{
		if (mCaseIndex < 0 || mCaseIndex >= TEST_CASES.Length || mSegmentIndex < 0 || mSegmentIndex >= getActiveSandwichOrder().Length)
		{
			return;
		}
		benchmarkLog("[CPU Checkpoint] Case=" + getCaseName(TEST_CASES[mCaseIndex]) +
			" | Segment=" + (mSegmentIndex + 1) + "/" + getActiveSandwichOrder().Length +
			" | Phase=" + phase + " | Target=" + getActiveSandwichOrder()[mSegmentIndex] +
			" | Elapsed=" + (Time.realtimeSinceStartup - mInitializationStartTime).ToString("F2") + "s");
		flushBenchmarkAnalysisCheckpoint();
	}
	protected void beginCase(int caseIndex)
	{
		mCaseIndex = caseIndex;
		mSegmentIndex = 0;
		for (int i = 0; i < mSegmentResults.Length; ++i)
		{
			mSegmentResults[i] = default;
		}
		beginSegment();
	}
	protected void beginSegment()
	{
		resetBothToBaseline();
		Canvas.ForceUpdateCanvases();
		mMutationStep = 0;
		mCurrentSegment = default;
		mPhase = BenchmarkPhase.Warmup;
		mPhaseStartTime = Time.realtimeSinceStartup;
		benchmarkLog("[FastUI vs UGUI Inventory] 开始:" + getCaseName(TEST_CASES[mCaseIndex]) + " | Segment:" +
			(mSegmentIndex + 1) + "/" + getActiveSandwichOrder().Length + " | Target:" + getActiveSandwichOrder()[mSegmentIndex] + " | Warmup:" + mWarmupTime.ToString("F2") + "s");
		logCPUCheckpoint("Begin");
	}
	protected void beginSampling()
	{
		mCurrentSegment = default;
		mMutationStep = 0;
		mPhase = BenchmarkPhase.Sampling;
		mPhaseStartTime = Time.realtimeSinceStartup;
	}
	protected void accumulateSampleFrame(BenchmarkTarget target)
	{
		++mCurrentSegment.mFrames;
		mCurrentSegment.mMutationMS += mLastMutationMS;
		if (target == BenchmarkTarget.UGUI)
		{
			mCurrentSegment.mFrameworkMS += mLastUGUIForceMS;
			return;
		}
		if (mFastRoot == null || !mFastRoot.activeInHierarchy || mFastCanvas == null) { return; }
		FastUIFrameStats stats = mFastCanvas.getLastFrameStats();
		if (stats.mFrame != Time.frameCount) { return; }
		mCurrentSegment.mFrameworkMS += stats.mCPUTimeMS;
		mCurrentSegment.mFastBatchCount += mFastCanvas.getLogicalBatchCount();
		mCurrentSegment.mFastRebuiltVertices += stats.mRebuiltVertexCount;
	}
	protected void finishSegment()
	{
		mSegmentResults[mSegmentIndex] = mCurrentSegment;
		logCPUCheckpoint("SampleComplete");
		++mSegmentIndex;
		BenchmarkTarget[] sandwichOrder = getActiveSandwichOrder();
		if (mSegmentIndex < sandwichOrder.Length)
		{
			beginSegment();
			return;
		}
		if (mBenchmarkProfile == BenchmarkExecutionProfile.Quick)
		{
			mSegmentResults[2] = mSegmentResults[1];
			mSegmentResults[3] = mSegmentResults[0];
		}
		reportCurrentCase();
		flushBenchmarkAnalysisCheckpoint();
		int nextCase = mCaseIndex + 1;
		if (nextCase < TEST_CASES.Length)
		{
			beginCase(nextCase);
			return;
		}
		finishAll();
	}
	protected void reportCurrentCase()
	{
		SegmentResult fastA = mSegmentResults[0];
		SegmentResult uguiA = mSegmentResults[1];
		SegmentResult uguiB = mSegmentResults[2];
		SegmentResult fastB = mSegmentResults[3];
		double fastMutation = average(fastA.getMutationAverage(), fastB.getMutationAverage());
		double fastFramework = average(fastA.getFrameworkAverage(), fastB.getFrameworkAverage());
		double fastScoped = fastMutation + fastFramework;
		double uguiMutation = average(uguiA.getMutationAverage(), uguiB.getMutationAverage());
		double uguiFramework = average(uguiA.getFrameworkAverage(), uguiB.getFrameworkAverage());
		double uguiScoped = uguiMutation + uguiFramework;
		double fastSpread = getSpreadPercent(fastA.getScopedAverage(), fastB.getScopedAverage());
		double uguiSpread = getSpreadPercent(uguiA.getScopedAverage(), uguiB.getScopedAverage());
		bool stable = fastSpread <= 20.0 && uguiSpread <= 20.0;
		double ratio = fastScoped > 0.000001 ? uguiScoped / fastScoped : 0.0;
		double fastBatch = average(fastA.getFastBatchAverage(), fastB.getFastBatchAverage());
		double fastVerts = average(fastA.getFastRebuiltVertexAverage(), fastB.getFastRebuiltVertexAverage());
		mComparisonResults[mCaseIndex] = new ComparisonResult
		{
			mFastScopedMS = fastScoped,
			mUGUIScopedMS = uguiScoped,
			mFastSpread = fastSpread,
			mUGUISpread = uguiSpread,
			mStable = stable
		};
		string caseName = getCaseName(TEST_CASES[mCaseIndex]);
		benchmarkLog("[CPU Compare] Case=" + caseName +
			" | Fast=" + fastScoped.ToString("F4") + "ms(Mutation=" + fastMutation.ToString("F4") + "/Canvas=" + fastFramework.ToString("F4") + ")" +
			" | UGUI=" + uguiScoped.ToString("F4") + "ms(Mutation=" + uguiMutation.ToString("F4") + "/Canvas=" + uguiFramework.ToString("F4") + ")" +
			" | U/F=" + ratio.ToString("F2") + "x | SpreadF/U=" + fastSpread.ToString("F1") + "%/" + uguiSpread.ToString("F1") + "%" +
			" | Stable=" + stable + " | FastBatch=" + fastBatch.ToString("F1") + " | FastRebuiltVertices=" + fastVerts.ToString("F1"));
		benchmarkLog("[README CPU] Case=" + caseName + " | Fast=" + fastScoped.ToString("F4") + "ms | UGUI=" + uguiScoped.ToString("F4") +
			"ms | U/F=" + ratio.ToString("F2") + "x | FastSpread=" + fastSpread.ToString("F1") + "% | UGUISpread=" + uguiSpread.ToString("F1") + "% | Stable=" + stable);
	}
	protected void finishAll()
	{
		mPhase = BenchmarkPhase.Complete;
		resetBothToBaseline();
		Canvas.ForceUpdateCanvases();
		int stableCount = 0;
		int fastClearlyLower = 0;
		int uguiClearlyLower = 0;
		int closeCount = 0;
		for (int i = 0; i < mComparisonResults.Length; ++i)
		{
			ComparisonResult result = mComparisonResults[i];
			if (!result.mStable)
			{
				continue;
			}
			++stableCount;
			if (result.mFastScopedMS <= 0.0 || result.mUGUIScopedMS <= 0.0)
			{
				continue;
			}
			double delta = (result.mUGUIScopedMS - result.mFastScopedMS) / result.mFastScopedMS;
			if (delta >= 0.10)
			{
				++fastClearlyLower;
			}
			else if (delta <= -0.10)
			{
				++uguiClearlyLower;
			}
			else
			{
				++closeCount;
			}
		}
		benchmarkLog("[README CPU Summary] Cases=" + TEST_CASES.Length + " | Stable=" + stableCount + " | FastLowerBy10PctOrMore=" + fastClearlyLower + " | UGUILowerBy10PctOrMore=" + uguiClearlyLower + " | Within10Pct=" + closeCount);
		benchmarkLog("================ FastUI vs UGUI Real-World Inventory Benchmark Complete =================\n" +
			"StableCases:" + stableCount + "/" + TEST_CASES.Length +
			" | FastUI ScopedCPU Lower>=10%:" + fastClearlyLower +
			" | UGUI ScopedCPU Lower>=10%:" + uguiClearlyLower +
			" | Within10%:" + closeCount + "\n" +
			"注意:CPU范围仍为业务Mutation + FastCanvas CPU / Canvas.ForceUpdateCanvases；Render在后续独立阶段统计渲染侧指标.");
		if (mRunRenderValidation)
		{
			StartCoroutine(runRenderValidation());
		}
		else
		{
			completeAllBenchmark();
		}
	}
	protected IEnumerator runRenderValidation()
	{
		mRenderWarmupFrames = Mathf.Max(mRenderWarmupFrames, 1);
		mRenderSampleFrames = Mathf.Max(mRenderSampleFrames, isPublicationProfile() ? 10 : QUICK_MIN_RENDER_SAMPLE_FRAMES);
		mGPUPrimeFrames = Mathf.Max(mGPUPrimeFrames, 1);
		mFrameTimingEnabled = FrameTimingManager.IsFeatureEnabled();
		mGPUFrameTimingAvailable = false;
		startRenderRecorders();
		benchmarkLog("================ FastUI vs UGUI Render Validation =================" +
			"Scope=IsolatedRoot | WarmupFrames=" + mRenderWarmupFrames + " | SampleFrames=" + mRenderSampleFrames +
			" | GPUPrimeFrames=" + mGPUPrimeFrames + " | FrameTiming=" + mFrameTimingEnabled + " | Profile=" + mBenchmarkProfile);
		if (mFrameTimingEnabled)
		{
			yield return runGPUMeasurementEmptyBaseline(true);
			if (mBenchmarkAborted) { yield break; }
			mGPUFrameTimingAvailable = mGPUMeasurementBaselineStart.mGPUTimingFrames > 0;
			benchmarkLog("[GPU Timing Capability] Available=" + mGPUFrameTimingAvailable);
		}
		InventoryCase?[] cases =
		{
			null, InventoryCase.ContentScroll, InventoryCase.IconSpriteRefresh, InventoryCase.QuantityTextRefresh,
			InventoryCase.LeafVisibilityRefresh, InventoryCase.RectMaskResize, InventoryCase.FullInventoryRebind,
			InventoryCase.FilterToggle, InventoryCase.ScatteredFilterToggle, InventoryCase.StressMixedRefresh,
			InventoryCase.IconMaterialRefresh, InventoryCase.RectMaskPaddingRefresh, InventoryCase.WindowSetActive,
		};
		string[] names =
		{
			"静态背包", "持续滚动", mBatchRefreshCount + "图标同图集Sprite变化", mBatchRefreshCount + "数量文字变化",
			mBatchRefreshCount + "叶节点显隐", "RectMask尺寸变化", mItemCount + "道具全量重绑",
			mBatchRefreshCount + "Item连续子树显隐", mScatterRefreshCount + "Item离散子树显隐", mStressRefreshCount + "Item重负荷混合刷新",
			mBatchRefreshCount + "图标Material变化", "RectMask Padding变化", "整窗口SetActive",
		};
		for (int i = 0; i < names.Length; ++i)
		{
			yield return runRenderCase(names[i], cases[i]);
			if (mBenchmarkAborted) { yield break; }
		}
		if (mFrameTimingEnabled && mGPUFrameTimingAvailable)
		{
			yield return runGPUMeasurementEmptyBaseline(false);
			if (mBenchmarkAborted) { yield break; }
			reportGPUMeasurementCalibration();
		}
		stopRenderRecorders();
		mFrameTimingEnabled = false;
		if (mRunRenderDiagnosis) { yield return runRenderDiagnosis(); }
		resetBothToBaseline();
		Canvas.ForceUpdateCanvases();
		benchmarkLog("================ FastUI vs UGUI Render Validation Complete =================");
		completeAllBenchmark();
	}
	protected void completeAllBenchmark()
	{
		if (mBenchmarkAborted || mAllBenchmarkComplete) { return; }
		mAllBenchmarkComplete = true;
		StartCoroutine(recoverFastUIAfterAllBenchmark());
	}
	// 会创建/销毁多组FastTextAtlasRun。验证必须使用独立Canvas，不能把测试节点挂到正式Inventory Canvas上。

	protected IEnumerator recoverFastUIAfterAllBenchmark()
	{
		yield return new WaitForEndOfFrame();
		yield return null;
		resetBothToBaseline();
		mFastCanvas.forceTMPTextRenderRefresh();
		yield return new WaitForEndOfFrame();
		yield return null;
		yield return new WaitForEndOfFrame();
		FastUIMeshRenderer renderer = mFastCanvas.getMeshRenderer();
		FastText validationText = mFastDetailName;
		bool tmpLayoutValid = renderer.isTMPVertexLayoutEnabled();
		bool tmpDataValid = validateFinalTMPVertexData(renderer, validationText);
		TMP_FontAsset finalFont = validationText != null ? validationText.getFont() : null;
		Material finalMaterial = validationText != null ? validationText.getMaterial() : null;
		Texture finalAtlas = finalFont != null ? finalFont.atlasTexture : null;
		bool fontSourceValid = finalFont != null && finalMaterial != null && finalMaterial.shader != null && finalAtlas != null && validationText.getRenderTexture() == finalAtlas;
		benchmarkLog("[Final Validation] CompactMode=" + mFastCanvas.getCompactIndexMode() + " | Indices=" + renderer.getCurrentIndexCount() +
			" | TMPLayout=" + tmpLayoutValid + " | TMPVertex=" + tmpDataValid + " | FontSource=" + fontSourceValid +
			" | Valid=" + (mFastCanvas.getCompactIndexMode() && tmpLayoutValid && tmpDataValid && fontSourceValid));

		yield return FastUIItemCloneBenchmark.run(benchmarkLog, transform, mTextureA, mTextureB, mFont, mImageMaterial);
		benchmarkLog(FastUIImageComparisonBenchmark.run(mFastCanvas, getInventoryIconSprite(0), getInventoryIconSprite(1)));
		yield return new WaitForEndOfFrame();
		yield return null;
		yield return StartCoroutine(FastUIWeaknessComparisonBenchmark.runCoroutine(mFastCanvas, mFont, benchmarkLog));

		float totalDuration = Time.realtimeSinceStartup - mBenchmarkSuiteStartTime;
		float targetDuration = isPublicationProfile() ? 600.0f : 220.0f;
		bool durationWithinTarget = totalDuration <= targetDuration;
		benchmarkLog("[Benchmark Duration] Profile=" + mBenchmarkProfile + " | Total=" + totalDuration.ToString("F2") +
			"s | Target=" + targetDuration.ToString("F2") + "s | WithinTarget=" + durationWithinTarget);
		benchmarkLog("[README Benchmark Complete] Profile=" + mBenchmarkProfile + " | TotalSeconds=" + totalDuration.ToString("F2") +
			" | CPUCases=" + TEST_CASES.Length + " | RenderDiagnosis=" + mRunRenderDiagnosis);
		flushBenchmarkAnalysisLog();
		benchmarkLog("================ FASTGUI BENCHMARK ALL TESTS COMPLETE =================");
		createBenchmarkCompleteScreen();
		restoreBenchmarkLogStackTrace();
	}
	protected void abortBenchmark(string reason)
	{
		if (mBenchmarkAborted || mAllBenchmarkComplete)
		{
			return;
		}
		mBenchmarkAborted = true;
		mAutoBenchmark = false;
		mPhase = BenchmarkPhase.Complete;
		stopRenderRecorders();
		benchmarkLogError("[Benchmark Abort] Reason=" + reason);
		benchmarkLogError("================ FASTGUI BENCHMARK ABORTED =================");
		createBenchmarkAbortScreen(reason);
		restoreBenchmarkLogStackTrace();
	}
	protected void createBenchmarkAbortScreen(string reason)
	{
		if (!mShowBenchmarkCompleteScreen || mBenchmarkCompleteRoot != null)
		{
			return;
		}
		mBenchmarkCompleteRoot = new GameObject("BenchmarkAbortScreen", typeof(RectTransform), typeof(Canvas));
		mBenchmarkCompleteRoot.hideFlags = HideFlags.DontSave;
		Canvas canvas = mBenchmarkCompleteRoot.GetComponent<Canvas>();
		canvas.renderMode = RenderMode.ScreenSpaceOverlay;
		canvas.sortingOrder = 32767;
		GameObject backgroundObject = new GameObject("Background", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
		RectTransform backgroundRect = backgroundObject.GetComponent<RectTransform>();
		backgroundRect.SetParent(mBenchmarkCompleteRoot.transform, false);
		backgroundRect.anchorMin = Vector2.zero;
		backgroundRect.anchorMax = Vector2.one;
		backgroundRect.offsetMin = Vector2.zero;
		backgroundRect.offsetMax = Vector2.zero;
		Image background = backgroundObject.GetComponent<Image>();
		background.color = new Color(0.0f, 0.0f, 0.0f, 0.88f);
		background.raycastTarget = false;
		GameObject textObject = new GameObject("AbortText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
		RectTransform textRect = textObject.GetComponent<RectTransform>();
		textRect.SetParent(mBenchmarkCompleteRoot.transform, false);
		textRect.anchorMin = Vector2.zero;
		textRect.anchorMax = Vector2.one;
		textRect.offsetMin = new Vector2(80.0f, 80.0f);
		textRect.offsetMax = new Vector2(-80.0f, -80.0f);
		TextMeshProUGUI abortText = textObject.GetComponent<TextMeshProUGUI>();
		abortText.fontSize = Mathf.Max(mBenchmarkCompleteFontSize * 0.72f, 32.0f);
		abortText.alignment = TextAlignmentOptions.Center;
		abortText.color = Color.white;
		abortText.raycastTarget = false;
		string chineseAbort = "BENCHMARK ABORTED\n\n测试已终止\n" + reason + "\n\n可以关闭程序";
		string englishAbort = "BENCHMARK ABORTED\n\nBenchmark stopped.\nSee Android Studio logcat for details.\n\nYou can close this window.";
		applyBenchmarkOverlayText(abortText, chineseAbort, englishAbort, "Abort");
	}
	protected void createBenchmarkCompleteScreen()
	{
		if (!mShowBenchmarkCompleteScreen || mBenchmarkCompleteRoot != null)
		{
			return;
		}
		// 完成提示不能再覆盖整屏。Benchmark结束后保持正式Inventory可见，只在屏幕中央显示一个紧凑提示面板。
		// 即使TMP字体资源异常，最坏也只是面板没有文字，不会再出现“整屏黑色+一个小点”的误导画面。
		mBenchmarkCompleteRoot = new GameObject("BenchmarkCompleteScreen", typeof(RectTransform), typeof(Canvas));
		mBenchmarkCompleteRoot.hideFlags = HideFlags.DontSave;
		Canvas canvas = mBenchmarkCompleteRoot.GetComponent<Canvas>();
		canvas.renderMode = RenderMode.ScreenSpaceOverlay;
		canvas.sortingOrder = 32767;
		GameObject panelObject = new GameObject("Panel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
		RectTransform panelRect = panelObject.GetComponent<RectTransform>();
		panelRect.SetParent(mBenchmarkCompleteRoot.transform, false);
		panelRect.anchorMin = new Vector2(0.5f, 0.5f);
		panelRect.anchorMax = new Vector2(0.5f, 0.5f);
		panelRect.pivot = new Vector2(0.5f, 0.5f);
		panelRect.anchoredPosition = Vector2.zero;
		panelRect.sizeDelta = new Vector2(760.0f, 210.0f);
		Image panel = panelObject.GetComponent<Image>();
		panel.color = new Color(0.0f, 0.0f, 0.0f, 0.82f);
		panel.raycastTarget = false;
		GameObject textObject = new GameObject("CompleteText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
		RectTransform textRect = textObject.GetComponent<RectTransform>();
		textRect.SetParent(panelRect, false);
		textRect.anchorMin = Vector2.zero;
		textRect.anchorMax = Vector2.one;
		textRect.offsetMin = new Vector2(24.0f, 18.0f);
		textRect.offsetMax = new Vector2(-24.0f, -18.0f);
		TextMeshProUGUI completeText = textObject.GetComponent<TextMeshProUGUI>();
		completeText.fontSize = Mathf.Clamp(mBenchmarkCompleteFontSize * 0.62f, 28.0f, 48.0f);
		completeText.enableAutoSizing = true;
		completeText.fontSizeMin = 24.0f;
		completeText.fontSizeMax = Mathf.Clamp(mBenchmarkCompleteFontSize * 0.62f, 28.0f, 48.0f);
		completeText.alignment = TextAlignmentOptions.Center;
		completeText.color = Color.white;
		completeText.raycastTarget = false;
		applyBenchmarkOverlayText(completeText, "BENCHMARK COMPLETE\n测试已全部完成\n可以关闭程序",
			"BENCHMARK COMPLETE\nAll tests completed successfully.\nYou can close this window.", "Complete");
	}
	protected void applyBenchmarkOverlayText(TextMeshProUGUI text, string chineseText, string englishText, string screenName)
	{
		TMP_FontAsset selectedFont = null;
		bool chineseSupported = false;
		TMP_FontAsset defaultFont = TMP_Settings.defaultFontAsset;
		if (mFont != null && canRenderTMPText(mFont, chineseText))
		{
			selectedFont = mFont;
			chineseSupported = true;
		}
		else if (defaultFont != null && defaultFont != mFont && canRenderTMPText(defaultFont, chineseText))
		{
			selectedFont = defaultFont;
			chineseSupported = true;
		}
		else
		{
			selectedFont = defaultFont != null ? defaultFont : mFont;
		}
		if (selectedFont != null)
		{
			text.font = selectedFont;
		}
		text.text = chineseSupported ? chineseText : englishText;
		benchmarkLog("[Benchmark Overlay Text] Screen=" + screenName + " | Font=" + (selectedFont != null ? selectedFont.name : "null") +
			" | ChineseGlyphs=" + chineseSupported + " | TextMode=" + (chineseSupported ? "Chinese" : "EnglishFallback"));
	}
	protected bool canRenderTMPText(TMP_FontAsset font, string text)
	{
		if (font == null || string.IsNullOrEmpty(text))
		{
			return false;
		}
		for (int i = 0; i < text.Length; ++i)
		{
			char character = text[i];
			if (char.IsWhiteSpace(character) || character < 128)
			{
				continue;
			}
			if (!font.HasCharacter(character))
			{
				return false;
			}
		}
		return true;
	}
	protected bool validateFinalTMPVertexData(FastUIMeshRenderer renderer, FastText text)
	{
		if (renderer == null || text == null || text.getVertexSlot() < 0 || !renderer.isTMPVertexLayoutEnabled())
		{
			return false;
		}
		int vertexStart = renderer.getGeometryVertexStart(text.getVertexSlot());
		int vertexCount = renderer.getGeometryVertexCount(text.getVertexSlot());
		FastUITMPUV0Data_ECSList uv0List = renderer.getTMPUV0ECS();
		FastUITMPUV2Data_ECSList uv2List = renderer.getTMPUV2ECS();
		if (vertexStart < 0 || vertexCount < 4 || uv0List == null || uv2List == null || vertexStart + 3 >= uv0List.Count || vertexStart + 3 >= uv2List.Count)
		{
			return false;
		}
		var uv0 = uv0List.getUVColumn();
		var uv2 = uv2List.getUVColumn();
		return uv0[vertexStart].w > 0.0f && uv2[vertexStart] == new Vector2(0.0f, 0.0f) &&
			uv2[vertexStart + 1] == new Vector2(0.0f, 1.0f) && uv2[vertexStart + 2] == new Vector2(1.0f, 1.0f) &&
			uv2[vertexStart + 3] == new Vector2(1.0f, 0.0f);
	}
	protected IEnumerator runGPUMeasurementEmptyBaseline(bool start)
	{
		int oldWarmupFrames = mRenderWarmupFrames;
		int oldSampleFrames = mRenderSampleFrames;
		int oldGPUPrimeFrames = mGPUPrimeFrames;
		int baselineMinWarmup = isPublicationProfile() ? 2 : 1;
		int baselineMinSamples = isPublicationProfile() ? 30 : 12;
		int baselineMinPrime = isPublicationProfile() ? 2 : 1;
		mRenderWarmupFrames = Mathf.Max(mGPUMeasurementBaselineWarmupFrames, baselineMinWarmup);
		mRenderSampleFrames = Mathf.Max(mGPUMeasurementBaselineSampleFrames, baselineMinSamples);
		mGPUPrimeFrames = Mathf.Max(mGPUMeasurementBaselinePrimeFrames, baselineMinPrime);
		resetBothToBaseline();
		mFastRoot.SetActive(false);
		mUGUIRoot.SetActive(false);
		Canvas.ForceUpdateCanvases();
		for (int i = 0; i < mRenderWarmupFrames; ++i)
		{
			yield return new WaitForEndOfFrame();
		}
		for (int i = 0; i < mGPUPrimeFrames; ++i)
		{
			FrameTimingManager.CaptureFrameTimings();
			yield return new WaitForEndOfFrame();
		}
		RenderSampleResult result = default;
		double[] wallFrameSamples = mLogEndToEndFrameTime ? new double[mRenderSampleFrames] : null;
		int wallFrameSampleCount = 0;
		long previousFrameEndTick = Stopwatch.GetTimestamp();
		for (int i = 0; i < mRenderSampleFrames; ++i)
		{
			FrameTimingManager.CaptureFrameTimings();
			yield return new WaitForEndOfFrame();
			long frameEndTick = Stopwatch.GetTimestamp();
			double currentWallMS = (frameEndTick - previousFrameEndTick) * TICK_TO_MS;
			if (wallFrameSamples != null)
			{
				double wallMS = currentWallMS;
				if (wallMS > 0.0 && wallFrameSampleCount < wallFrameSamples.Length)
				{
					wallFrameSamples[wallFrameSampleCount++] = wallMS;
					result.mWallFrameMS += wallMS;
				}
			}
			previousFrameEndTick = frameEndTick;
			accumulateRenderSample(ref result, BenchmarkTarget.UGUI, currentWallMS);
		}
		if (wallFrameSamples != null)
		{
			finalizeWallFrameSamples(ref result, wallFrameSamples, wallFrameSampleCount);
		}
		if (start)
		{
			mGPUMeasurementBaselineStart = result;
			mGPUMeasurementBaselineStartValid = result.getGPUFrameAverage() >= 0.0;
		}
		else
		{
			mGPUMeasurementBaselineEnd = result;
			mGPUMeasurementBaselineEndValid = result.getGPUFrameAverage() >= 0.0;
		}
		benchmarkLog("[FastUI GPU Measurement Baseline] Stage=" + (start ? "Start" : "End") +
			" | DrawCalls=" + result.getDrawCallsAverage().ToString("F1") +
			" | SetPass=" + result.getSetPassAverage().ToString("F1") +
			" | Triangles=" + result.getTrianglesAverage().ToString("F1") +
			" | Vertices=" + result.getVerticesAverage().ToString("F1") +
			" | GPU=" + formatOptionalMS(result.getGPUFrameAverage()) +
			" | CPU=" + formatOptionalMS(result.getCPUFrameAverage()) +
			" | Main=" + formatOptionalMS(result.getCPUMainThreadFrameAverage()) +
			" | Render=" + formatOptionalMS(result.getCPURenderThreadFrameAverage()) +
			" | WallMedian=" + formatOptionalMS(result.getWallFrameMedian()) +
			" | Frames=" + result.mGPUTimingFrames + " | InvalidCPUFrames=" + result.mCPUInvalidTimingFrames + " | InvalidGPUFrames=" + result.mGPUInvalidTimingFrames);
		mRenderWarmupFrames = oldWarmupFrames;
		mRenderSampleFrames = oldSampleFrames;
		mGPUPrimeFrames = oldGPUPrimeFrames;
	}
	protected void reportGPUMeasurementCalibration()
	{
		double startGPU = mGPUMeasurementBaselineStartValid ? mGPUMeasurementBaselineStart.getGPUFrameAverage() : -1.0;
		double endGPU = mGPUMeasurementBaselineEndValid ? mGPUMeasurementBaselineEnd.getGPUFrameAverage() : -1.0;
		double baselineGPU = averageAvailable(startGPU, endGPU);
		if (baselineGPU < 0.0)
		{
			benchmarkLog("[FastUI GPU Measurement Calibration] Baseline=Unavailable | CaseCount=" + mRenderGPUCalibrationRecords.Count);
			return;
		}
		double baselineDrift = startGPU >= 0.0 && endGPU >= 0.0 ? Math.Abs(endGPU - startGPU) : 0.0;
		double tolerance = Math.Max(mGPUMeasurementFloorToleranceMS, baselineGPU * Math.Max(mGPUMeasurementFloorTolerancePercent, 0.0f) * 0.01);
		tolerance = Math.Max(tolerance, baselineDrift * 0.5);
		benchmarkLog("================ FastUI GPU Measurement Calibration =================\n" +
			"EmptyGPU Start/End/Baseline:" + formatOptionalMS(startGPU) + "/" + formatOptionalMS(endGPU) + "/" + formatOptionalMS(baselineGPU) +
			" | Drift:" + baselineDrift.ToString("F3") + "ms | FloorTolerance:" + tolerance.ToString("F3") +
			"ms | Rule:ExcessAboveEmpty=max(RawGPU-EmptyBaseline,0), FloorBound=RawGPU<=Baseline+Tolerance");
		for (int i = 0; i < mRenderGPUCalibrationRecords.Count; ++i)
		{
			RenderGPUCalibrationRecord record = mRenderGPUCalibrationRecords[i];
			if (!record.mComplete || record.mFastGPU < 0.0 || record.mUGUIGPU < 0.0)
			{
				benchmarkLog("[FastUI GPU Calibrated] Case=" + record.mCaseName + " | Raw F/U:Unavailable | Baseline=" + formatOptionalMS(baselineGPU));
				continue;
			}
			double fastExcess = Math.Max(record.mFastGPU - baselineGPU, 0.0);
			double uguiExcess = Math.Max(record.mUGUIGPU - baselineGPU, 0.0);
			bool fastFloorBound = record.mFastGPU <= baselineGPU + tolerance;
			bool uguiFloorBound = record.mUGUIGPU <= baselineGPU + tolerance;
			string sensitivity = fastFloorBound && uguiFloorBound ? "FloorBound" : "LoadSensitive";
			benchmarkLog("[FastUI GPU Calibrated] Case=" + record.mCaseName +
				" | Raw F/U:" + formatOptionalMS(record.mFastGPU) + "/" + formatOptionalMS(record.mUGUIGPU) +
				" | EmptyBaseline=" + formatOptionalMS(baselineGPU) +
				" | ExcessAboveEmpty F/U:" + fastExcess.ToString("F3") + "ms/" + uguiExcess.ToString("F3") + "ms" +
				" | FloorBound F/U:" + fastFloorBound + "/" + uguiFloorBound +
				" | Sensitivity=" + sensitivity);
		}
		benchmarkLog("================ FastUI GPU Measurement Calibration Complete =================");
	}
	protected IEnumerator runRenderCase(string caseName, InventoryCase? mutationCase)
	{
		RenderSampleResult[] results = new RenderSampleResult[4];
		BenchmarkTarget[] sandwichOrder = getActiveSandwichOrder();
		for (int segment = 0; segment < sandwichOrder.Length; ++segment)
		{
			BenchmarkTarget target = sandwichOrder[segment];
			prepareRenderValidationTarget(target);
			if (!logRenderCheckpoint("Render", caseName, segment + 1, sandwichOrder.Length, "Begin", target))
			{
				yield break;
			}
			mMutationStep = 0;
			int warmupFrames = 0;
			bool warmupCapped = false;
			long warmupStartTick = System.Diagnostics.Stopwatch.GetTimestamp();
			for (int i = 0; i < mRenderWarmupFrames; ++i)
			{
				executeRenderMutation(mutationCase, target);
				yield return new WaitForEndOfFrame();
				++warmupFrames;
				if (reachedQuickFrameBudget(warmupStartTick, warmupFrames, QUICK_RENDER_WARMUP_BUDGET_MS, 1))
				{
					warmupCapped = warmupFrames < mRenderWarmupFrames;
					break;
				}
			}
			if (!logRenderCheckpoint("Render", caseName, segment + 1, sandwichOrder.Length, "WarmupComplete", target))
			{
				yield break;
			}
			int primeFrames = 0;
			bool primeCapped = false;
			bool runGPUPrime = mFrameTimingEnabled && mGPUFrameTimingAvailable;
			if (runGPUPrime)
			{
				long primeStartTick = System.Diagnostics.Stopwatch.GetTimestamp();
				for (int i = 0; i < mGPUPrimeFrames; ++i)
				{
					executeRenderMutation(mutationCase, target);
					FrameTimingManager.CaptureFrameTimings();
					yield return new WaitForEndOfFrame();
					++primeFrames;
					if (reachedQuickFrameBudget(primeStartTick, primeFrames, QUICK_RENDER_PRIME_BUDGET_MS, 1))
					{
						primeCapped = primeFrames < mGPUPrimeFrames;
						break;
					}
				}
			}
			RenderSampleResult result = default;
			double[] wallFrameSamples = mLogEndToEndFrameTime ? new double[mRenderSampleFrames] : null;
			int wallFrameSampleCount = 0;
			long previousFrameEndTick = Stopwatch.GetTimestamp();
			long sampleStartTick = previousFrameEndTick;
			bool sampleCapped = false;
			for (int i = 0; i < mRenderSampleFrames; ++i)
			{
				executeRenderMutation(mutationCase, target);
				if (mFrameTimingEnabled)
				{
					FrameTimingManager.CaptureFrameTimings();
				}
				yield return new WaitForEndOfFrame();
				long frameEndTick = Stopwatch.GetTimestamp();
				double currentWallMS = (frameEndTick - previousFrameEndTick) * TICK_TO_MS;
				if (wallFrameSamples != null)
				{
					double wallMS = currentWallMS;
					if (wallMS > 0.0 && wallFrameSampleCount < wallFrameSamples.Length)
					{
						wallFrameSamples[wallFrameSampleCount++] = wallMS;
						result.mWallFrameMS += wallMS;
					}
				}
				previousFrameEndTick = frameEndTick;
				accumulateRenderSample(ref result, target, currentWallMS);
				if (reachedQuickFrameBudget(sampleStartTick, result.mFrames, QUICK_RENDER_SAMPLE_BUDGET_MS, QUICK_MIN_RENDER_SAMPLE_FRAMES))
				{
					sampleCapped = result.mFrames < mRenderSampleFrames;
					break;
				}
			}
			if (wallFrameSamples != null)
			{
				finalizeWallFrameSamples(ref result, wallFrameSamples, wallFrameSampleCount);
			}
			results[segment] = result;
			if (warmupCapped || primeCapped || sampleCapped)
			{
				benchmarkLog("[FastUI Render Budget] Case=" + caseName + " | Segment=" + (segment + 1) + "/" + sandwichOrder.Length +
					" | Target=" + target + " | Warmup=" + warmupFrames + "/" + mRenderWarmupFrames +
					" | Prime=" + primeFrames + "/" + (runGPUPrime ? mGPUPrimeFrames : 0) +
					" | Sample=" + result.mFrames + "/" + mRenderSampleFrames + " | TimeCapped=True");
			}
			if (!logRenderCheckpoint("Render", caseName, segment + 1, sandwichOrder.Length, "SampleComplete", target))
			{
				yield break;
			}
		}
		if (mBenchmarkProfile == BenchmarkExecutionProfile.Quick)
		{
			results[2] = results[1];
			results[3] = results[0];
		}
		reportRenderCase(caseName, results[0], results[1], results[2], results[3]);
	}
	protected void finalizeWallFrameSamples(ref RenderSampleResult result, double[] samples, int count)
	{
		result.mWallFrameSamples = count;
		if (samples == null || count <= 0)
		{
			return;
		}
		Array.Sort(samples, 0, count);
		int middle = count >> 1;
		result.mWallFrameMedianMS = (count & 1) != 0 ? samples[middle] : (samples[middle - 1] + samples[middle]) * 0.5;
		int p95Index = Mathf.Clamp(Mathf.CeilToInt(count * 0.95f) - 1, 0, count - 1);
		result.mWallFrameP95MS = samples[p95Index];
	}
	protected bool logRenderCheckpoint(string group, string caseName, int segment, int segmentCount, string phase, BenchmarkTarget target)
	{
		FastUIMeshRenderer renderer = mFastCanvas != null ? mFastCanvas.getMeshRenderer() : null;
		int logicalBatch = renderer != null ? renderer.getLogicalBatchCount() : 0;
		int submitDrawRuns = renderer != null ? renderer.getBatchCount() : 0;
		int activeSubMeshes = renderer != null ? renderer.getActiveSubMeshCount() : 0;
		int meshSubMeshes = renderer != null ? renderer.getMeshSubMeshCount() : 0;
		int materialSlots = renderer != null ? renderer.getAppliedMaterialSlotCount() : 0;
		int capacity = renderer != null ? renderer.getSubMeshCapacity() : 0;
		bool clipSubmitCull = renderer != null && renderer.getClipSubmitRangeCullingEnabled();
		int expected = Mathf.Max(submitDrawRuns, 1);
		bool slotValid = renderer != null && activeSubMeshes == expected && meshSubMeshes == expected && materialSlots == expected;
		long drawCalls = mDrawCallsRecorder.Valid ? mDrawCallsRecorder.LastValue : -1;
		bool drawCallValid = target != BenchmarkTarget.FastUI || phase == "Begin" || drawCalls < 0 ||
			drawCalls <= Mathf.Max(mFastRenderDrawCallAbortThreshold, 64);
		if (mLogRenderCheckpoints || !slotValid || !drawCallValid)
		{
			benchmarkLog("[FastUI Render Checkpoint] Group=" + group + " | Case=" + caseName + " | Segment=" + segment + "/" + segmentCount +
				" | Phase=" + phase + " | Target=" + target + " | LogicalBatch=" + logicalBatch + " | SubmitDrawRuns=" + submitDrawRuns +
				" | ActiveSubMesh=" + activeSubMeshes + " | MeshSubMesh=" + meshSubMeshes +
				" | MaterialSlots=" + materialSlots + " | Capacity=" + capacity + " | ClipSubmitCull=" + clipSubmitCull +
				" | DrawCalls=" + drawCalls + " | SlotValid=" + slotValid + " | DrawCallValid=" + drawCallValid +
				" | Elapsed=" + (Time.realtimeSinceStartup - mInitializationStartTime).ToString("F2") + "s");
			flushBenchmarkAnalysisCheckpoint();
		}
		if (!slotValid)
		{
			abortBenchmark("Render状态Batch Slot失配:Case=" + caseName + ",Phase=" + phase);
			return false;
		}
		if (!drawCallValid)
		{
			abortBenchmark("FastGUI DrawCall异常超过阈值:Case=" + caseName + ",Phase=" + phase +
				",DrawCalls=" + drawCalls + ",Threshold=" + mFastRenderDrawCallAbortThreshold);
			return false;
		}
		return true;
	}

	protected double getFastStatsAverage(double value, int frames)
	{
		return frames > 0 ? value / frames : 0.0;
	}
	protected void executeRenderMutation(InventoryCase? mutationCase, BenchmarkTarget target)
	{
		if (!mutationCase.HasValue)
		{
			return;
		}
		executeMutation(mutationCase.Value, target);
		++mMutationStep;
	}
	protected IEnumerator runRenderDiagnosis()
	{
		benchmarkLog("================ FastUI vs UGUI Render Diagnosis =================");
		prepareRenderDiagnosisTarget(BenchmarkTarget.FastUI, false);
		yield return new WaitForEndOfFrame();
		logFastRenderDiagnosis("静态背包");
		prepareRenderDiagnosisTarget(BenchmarkTarget.UGUI, false);
		yield return new WaitForEndOfFrame();
		logUGUIRenderDiagnosis("静态背包");
		prepareRenderDiagnosisTarget(BenchmarkTarget.FastUI, true);
		yield return new WaitForEndOfFrame();
		logFastRenderDiagnosis("中段滚动");
		prepareRenderDiagnosisTarget(BenchmarkTarget.UGUI, true);
		yield return new WaitForEndOfFrame();
		logUGUIRenderDiagnosis("中段滚动");
		benchmarkLog("================ FastUI vs UGUI Render Diagnosis Complete =================");
	}
	protected void prepareRenderDiagnosisTarget(BenchmarkTarget target, bool scrolled)
	{
		prepareRenderValidationTarget(target);
		if (!scrolled)
		{
			return;
		}
		float maxScroll = getMaxScroll();
		mMutationStep = maxScroll > 0.0f ? Mathf.Max(1, Mathf.RoundToInt(maxScroll * 0.5f / Mathf.Max(mScrollStep, 0.0001f))) : 0;
		scrollContent(target);
		Canvas.ForceUpdateCanvases();
	}
	protected void logFastRenderDiagnosis(string caseName)
	{
		if (mFastCanvas == null)
		{
			return;
		}
		FastUIMeshRenderer renderer = mFastCanvas.getMeshRenderer();
		if (renderer == null)
		{
			return;
		}
		int renderSlotCount = mFastCanvas.getRenderOrderSlotCount();
		FastUIRenderElement[] byRenderIndex = buildFastRenderIndexMap(renderSlotCount);
		int liveVertices = 0;
		int vertexCapacity = 0;
		int visibleVertices = 0;
		int hiddenVertices = 0;
		int imageVertices = 0;
		int textVertices = 0;
		int stencilVertices = 0;
		int liveIndices = 0;
		int indexCapacity = 0;
		int nonDegenerateIndices = 0;
		int hiddenDegenerateIndices = 0;
		int capacityDegenerateIndices = 0;
		int hiddenElementCount = 0;
		int visibleElementCount = 0;
		for (int renderIndex = 0; renderIndex < byRenderIndex.Length; ++renderIndex)
		{
			FastUIRenderElement element = byRenderIndex[renderIndex];
			if (element == null || element.getCanvas() != mFastCanvas || element.getVertexSlot() < 0)
			{
				continue;
			}
			int slot = element.getVertexSlot();
			int vertexCount = renderer.getGeometryVertexCount(slot);
			int elementVertexCapacity = renderer.getGeometryVertexCapacity(slot);
			int indexCount = renderer.getGeometryIndexCount(slot);
			int elementIndexCapacity = renderer.getGeometryIndexCapacity(slot);
			bool hidden = !element.isRenderActive() || !element.getVisible() || element.isCulled() || renderer.isIndexHiddenRenderIndex(renderIndex);
			liveVertices += vertexCount;
			vertexCapacity += elementVertexCapacity;
			liveIndices += indexCount;
			indexCapacity += elementIndexCapacity;
			if (hidden)
			{
				hiddenVertices += vertexCount;
				hiddenDegenerateIndices += elementIndexCapacity;
				++hiddenElementCount;
			}
			else
			{
				visibleVertices += vertexCount;
				nonDegenerateIndices += indexCount;
				capacityDegenerateIndices += Mathf.Max(elementIndexCapacity - indexCount, 0);
				++visibleElementCount;
			}
			if (element is FastClipStencilGraphic)
			{
				stencilVertices += vertexCount;
			}
			else if (element is FastText)
			{
				textVertices += vertexCount;
			}
			else
			{
				imageVertices += vertexCount;
			}
		}
		int drawOrderIndexCount = renderer.getCurrentIndexCount();
		int degenerateIndices = hiddenDegenerateIndices + capacityDegenerateIndices;
		benchmarkLog("[FastUI Render Diagnosis] " + caseName +
			" | MeshVertexCount=" + renderer.getMeshVertexCount() +
			" | VertexSpan=" + renderer.getVertexSpan() +
			" | LiveVertices=" + renderer.getLiveVertexCount() +
			" | ElementLiveVertices=" + liveVertices +
			" | ElementVertexCapacity=" + vertexCapacity +
			" | Visible/HiddenVertices=" + visibleVertices + "/" + hiddenVertices +
			" | Image/Text/StencilVertices=" + imageVertices + "/" + textVertices + "/" + stencilVertices +
			" | Visible/HiddenElements=" + visibleElementCount + "/" + hiddenElementCount +
			" | LiveIndices=" + liveIndices +
			" | IndexCapacity=" + indexCapacity +
			" | DrawOrderIndices=" + drawOrderIndexCount +
			" | GPUIndexCapacity=" + renderer.getGPUIndexCapacity() +
			" | NonDegenerateIndices=" + nonDegenerateIndices +
			" | DegenerateIndices=" + degenerateIndices +
			"(Hidden=" + hiddenDegenerateIndices + ",Capacity=" + capacityDegenerateIndices + ")" +
			" | LogicalTriangles=" + (nonDegenerateIndices / 3) +
			" | SubmittedIndexTriangles=" + (drawOrderIndexCount / 3));
		logFastBatchDiagnosis(caseName, renderer, byRenderIndex, renderSlotCount);
	}
	protected FastUIRenderElement[] buildFastRenderIndexMap(int renderSlotCount)
	{
		FastUIRenderElement[] result = new FastUIRenderElement[Mathf.Max(renderSlotCount, 0)];
		FastUIRenderElement[] elements = mFastRoot.GetComponentsInChildren<FastUIRenderElement>(true);
		for (int i = 0; i < elements.Length; ++i)
		{
			FastUIRenderElement element = elements[i];
			if (element == null || element.getCanvas() != mFastCanvas)
			{
				continue;
			}
			int renderIndex = element.getRenderOrderIndex();
			if ((uint)renderIndex < (uint)result.Length)
			{
				result[renderIndex] = element;
			}
		}
		return result;
	}
	protected void logFastBatchDiagnosis(string caseName, FastUIMeshRenderer renderer, FastUIRenderElement[] byRenderIndex, int renderSlotCount)
	{
		int batchRunCount = renderer.getBatchRunCount();
		int drawRunCount = renderer.getEffectiveDrawRunCount(renderSlotCount);
		int soaRunCount = 0;
		int normalRunCount = 0;
		int stencilRunCount = 0;
		int mixedRunCount = 0;
		int materialBreakCount = 0;
		int textureBreakCount = 0;
		int sameShaderTextureDifferentMaterialCount = 0;
		int samePipelineStateDifferentMaterialCount = 0;
		Material previousMaterial = null;
		Texture previousTexture = null;
		string previousPipelineState = null;
		HashSet<int> materialIDs = new();
		HashSet<int> shaderIDs = new();
		HashSet<int> textureIDs = new();
		HashSet<string> pipelineStates = new();
		for (int runIndex = 0; runIndex < drawRunCount; ++runIndex)
		{
			int drawStart = renderer.getEffectiveDrawRunStart(runIndex, renderSlotCount);
			int drawEnd = renderer.getEffectiveDrawRunEnd(runIndex, renderSlotCount);
			Material material = renderer.getEffectiveDrawRunMaterial(runIndex, renderSlotCount);
			Texture texture = renderer.getEffectiveDrawRunTexture(runIndex, renderSlotCount);
			string pipelineState = getFastMaterialPipelineStateKey(material, texture);
			if (material != null)
			{
				materialIDs.Add(FastUnityObjectIDUtility.getLegacyIntID(material));
				if (material.shader != null)
				{
					shaderIDs.Add(FastUnityObjectIDUtility.getLegacyIntID(material.shader));
				}
			}
			if (texture != null)
			{
				textureIDs.Add(FastUnityObjectIDUtility.getLegacyIntID(texture));
			}
			pipelineStates.Add(pipelineState);
			if (runIndex > 0)
			{
				if (material != previousMaterial)
				{
					++materialBreakCount;
					if (material != null && previousMaterial != null && material.shader == previousMaterial.shader && texture == previousTexture)
					{
						++sameShaderTextureDifferentMaterialCount;
					}
					if (pipelineState == previousPipelineState)
					{
						++samePipelineStateDifferentMaterialCount;
					}
				}
				else if (texture != previousTexture)
				{
					++textureBreakCount;
				}
			}
			previousMaterial = material;
			previousTexture = texture;
			previousPipelineState = pipelineState;
			int laneState = int.MinValue;
			bool hasSOA = false;
			bool hasNormal = false;
			bool hasStencil = false;
			int elementCount = 0;
			int hiddenElementCount = 0;
			int vertexCount = 0;
			int logicalIndexCount = 0;
			FastUIRenderElement first = null;
			FastUIRenderElement last = null;
			for (int drawIndex = drawStart; drawIndex < drawEnd; ++drawIndex)
			{
				int renderIndex = renderer.getRenderIndexForDrawIndex(drawIndex);
				if ((uint)renderIndex >= (uint)byRenderIndex.Length)
				{
					continue;
				}
				FastUIRenderElement element = byRenderIndex[renderIndex];
				if (element == null)
				{
					continue;
				}
				if (first == null)
				{
					first = element;
				}
				last = element;
				++elementCount;
				int slot = element.getVertexSlot();
				if (slot >= 0)
				{
					vertexCount += renderer.getGeometryVertexCount(slot);
					logicalIndexCount += renderer.getGeometryIndexCount(slot);
				}
				if (!element.isRenderActive() || !element.getVisible() || element.isCulled() || renderer.isIndexHiddenRenderIndex(renderIndex))
				{
					++hiddenElementCount;
				}
				if (element.isStencilBarrier())
				{
					hasStencil = true;
				}
				int lane = mFastCanvas.getSOALaneIndexForRenderIndex(renderIndex);
				if (lane >= 0)
				{
					hasSOA = true;
					if (laneState == int.MinValue)
					{
						laneState = lane;
					}
					else if (laneState != lane)
					{
						laneState = -2;
					}
				}
				else
				{
					hasNormal = true;
				}
			}
			string source;
			if (hasStencil && !hasSOA && !hasNormal)
			{
				source = "Stencil";
				++stencilRunCount;
			}
			else if (hasSOA && !hasNormal && !hasStencil && laneState >= 0)
			{
				source = "SOA-Lane" + laneState;
				++soaRunCount;
			}
			else if (!hasSOA && !hasStencil)
			{
				source = "Normal";
				++normalRunCount;
			}
			else
			{
				source = "Mixed";
				++mixedRunCount;
			}
			benchmarkLog("[FastUI Batch Run Diagnosis] Case=" + caseName +
				" | Run=" + runIndex + " | Draw=" + drawStart + "-" + drawEnd +
				" | Source=" + source + " | Elements=" + elementCount + " | Hidden=" + hiddenElementCount +
				" | Vertices=" + vertexCount + " | LogicalIndices=" + logicalIndexCount +
				" | Material=" + getObjectDiagnosticName(material) +
				" | Shader=" + getObjectDiagnosticName(material != null ? material.shader : null) +
				" | Texture=" + getObjectDiagnosticName(texture) +
				" | Queue=" + (material != null ? material.renderQueue : -1) +
				" | Passes=" + (material != null ? material.passCount : 0) +
				" | Keywords=" + getMaterialKeywordDiagnostic(material) +
				" | Stencil=" + getMaterialStencilDiagnostic(material) +
				" | First=" + getFastElementPath(first) + " | Last=" + getFastElementPath(last));
		}
		benchmarkLog("[FastUI Batch Diagnosis] " + caseName +
			" | BatchRuns=" + batchRunCount + " | DrawRuns=" + drawRunCount + " | FastBatch=" + renderer.getLogicalBatchCount() + " | SubmitDrawRuns=" + renderer.getBatchCount() +
			" | SOA=" + soaRunCount + " | Normal=" + normalRunCount + " | Stencil=" + stencilRunCount + " | Mixed=" + mixedRunCount +
			" | UniqueMaterials=" + materialIDs.Count + " | UniqueShaders=" + shaderIDs.Count + " | UniqueTextures=" + textureIDs.Count +
			" | UniquePipelineStates=" + pipelineStates.Count + " | MaterialBreak=" + materialBreakCount + " | TextureBreak=" + textureBreakCount +
			" | SameShaderTextureDifferentMaterial=" + sameShaderTextureDifferentMaterialCount +
			" | SamePipelineStateDifferentMaterial=" + samePipelineStateDifferentMaterialCount +
			" | VisibilitySplitExtra=" + Mathf.Max(drawRunCount - batchRunCount, 0) +
			" | SOABatchKeyGroups=" + mFastCanvas.getSOABatchKeyGroupCount());
	}
	protected string getFastMaterialPipelineStateKey(Material material, Texture texture)
	{
		if (material == null)
		{
			return "None|Tex=" + (texture != null ? FastUnityObjectIDUtility.getLegacyIntID(texture) : 0);
		}
		StringBuilder builder = new();
		Shader shader = material.shader;
		builder.Append("Shader=").Append(shader != null ? FastUnityObjectIDUtility.getLegacyIntID(shader) : 0)
			.Append("|Tex=").Append(texture != null ? FastUnityObjectIDUtility.getLegacyIntID(texture) : 0)
			.Append("|Queue=").Append(material.renderQueue)
			.Append("|Passes=").Append(material.passCount)
			.Append("|KW=").Append(getMaterialKeywordDiagnostic(material));
		appendMaterialStateFloat(builder, material, "_Stencil");
		appendMaterialStateFloat(builder, material, "_StencilComp");
		appendMaterialStateFloat(builder, material, "_StencilOp");
		appendMaterialStateFloat(builder, material, "_StencilReadMask");
		appendMaterialStateFloat(builder, material, "_StencilWriteMask");
		appendMaterialStateFloat(builder, material, "_ColorMask");
		appendMaterialStateFloat(builder, material, "_UseUIAlphaClip");
		appendMaterialStateFloat(builder, material, "_SrcBlend");
		appendMaterialStateFloat(builder, material, "_DstBlend");
		appendMaterialStateFloat(builder, material, "_ZWrite");
		appendMaterialStateFloat(builder, material, "_ZTest");
		appendMaterialStateFloat(builder, material, "_Cull");
		return builder.ToString();
	}
	protected string getMaterialKeywordDiagnostic(Material material)
	{
		if (material == null)
		{
			return "None";
		}
		string[] keywords = material.shaderKeywords;
		if (keywords == null || keywords.Length == 0)
		{
			return "None";
		}
		Array.Sort(keywords, StringComparer.Ordinal);
		return string.Join(",", keywords);
	}
	protected string getMaterialStencilDiagnostic(Material material)
	{
		if (material == null)
		{
			return "None";
		}
		StringBuilder builder = new();
		appendMaterialDiagnosticFloat(builder, material, "Ref", "_Stencil");
		appendMaterialDiagnosticFloat(builder, material, "Comp", "_StencilComp");
		appendMaterialDiagnosticFloat(builder, material, "Op", "_StencilOp");
		appendMaterialDiagnosticFloat(builder, material, "Read", "_StencilReadMask");
		appendMaterialDiagnosticFloat(builder, material, "Write", "_StencilWriteMask");
		appendMaterialDiagnosticFloat(builder, material, "ColorMask", "_ColorMask");
		appendMaterialDiagnosticFloat(builder, material, "AlphaClip", "_UseUIAlphaClip");
		return builder.Length > 0 ? builder.ToString() : "NoStencilProperties";
	}
	protected void appendMaterialStateFloat(StringBuilder builder, Material material, string propertyName)
	{
		if (material == null || !material.HasProperty(propertyName))
		{
			return;
		}
		builder.Append('|').Append(propertyName).Append('=').Append(material.GetFloat(propertyName).ToString("R"));
	}
	protected void appendMaterialDiagnosticFloat(StringBuilder builder, Material material, string label, string propertyName)
	{
		if (material == null || !material.HasProperty(propertyName))
		{
			return;
		}
		if (builder.Length > 0)
		{
			builder.Append(',');
		}
		builder.Append(label).Append('=').Append(material.GetFloat(propertyName).ToString("R"));
	}

	protected void logUGUIRenderDiagnosis(string caseName)
	{
		if (mUGUIRoot == null)
		{
			return;
		}
		CanvasRenderer[] renderers = mUGUIRoot.GetComponentsInChildren<CanvasRenderer>(true);
		int totalVertices = 0;
		int renderedVertices = 0;
		int culledVertices = 0;
		long totalIndices = 0;
		long renderedIndices = 0;
		long culledIndices = 0;
		int renderedRendererCount = 0;
		int culledRendererCount = 0;
		int imageVertices = 0;
		int rawImageVertices = 0;
		int textVertices = 0;
		for (int i = 0; i < renderers.Length; ++i)
		{
			CanvasRenderer canvasRenderer = renderers[i];
			if (canvasRenderer == null)
			{
				continue;
			}
			Mesh mesh = canvasRenderer.GetMesh();
			int vertexCount = mesh != null ? mesh.vertexCount : 0;
			long indexCount = 0;
			if (mesh != null)
			{
				for (int subMesh = 0; subMesh < mesh.subMeshCount; ++subMesh)
				{
					indexCount += (long)mesh.GetIndexCount(subMesh);
				}
			}
			bool culled = canvasRenderer.cull || !canvasRenderer.gameObject.activeInHierarchy;
			totalVertices += vertexCount;
			totalIndices += indexCount;
			if (culled)
			{
				culledVertices += vertexCount;
				culledIndices += indexCount;
				++culledRendererCount;
			}
			else
			{
				renderedVertices += vertexCount;
				renderedIndices += indexCount;
				++renderedRendererCount;
			}
			if (canvasRenderer.GetComponent<TextMeshProUGUI>() != null)
			{
				textVertices += vertexCount;
			}
			else if (canvasRenderer.GetComponent<Image>() != null)
			{
				imageVertices += vertexCount;
			}
			else if (canvasRenderer.GetComponent<RawImage>() != null)
			{
				rawImageVertices += vertexCount;
			}
		}
		benchmarkLog("[UGUI Render Diagnosis] " + caseName +
			" | CanvasRenderers=" + renderers.Length +
			" | Rendered/Culled=" + renderedRendererCount + "/" + culledRendererCount +
			" | MeshVertices Total/Rendered/Culled=" + totalVertices + "/" + renderedVertices + "/" + culledVertices +
			" | MeshIndices Total/Rendered/Culled=" + totalIndices + "/" + renderedIndices + "/" + culledIndices +
			" | Image/RawImage/TextVertices=" + imageVertices + "/" + rawImageVertices + "/" + textVertices +
			" | RenderedLogicalTriangles=" + (renderedIndices / 3));
	}
	protected string getFastElementPath(FastUIRenderElement element)
	{
		if (element == null)
		{
			return "None";
		}
		return getRelativePath(element.transform, mFastRoot != null ? mFastRoot.transform : null);
	}
	protected string getRelativePath(Transform node, Transform root)
	{
		if (node == null)
		{
			return "None";
		}
		StringBuilder builder = new();
		Transform current = node;
		while (current != null && current != root)
		{
			if (builder.Length > 0)
			{
				builder.Insert(0, '/');
			}
			builder.Insert(0, current.name);
			current = current.parent;
		}
		return builder.Length > 0 ? builder.ToString() : node.name;
	}
	protected string getObjectDiagnosticName(UnityEngine.Object obj)
	{
		return obj != null ? obj.name + "(" + FastUnityObjectIDUtility.getLegacyIntID(obj) + ")" : "None";
	}
	protected void prepareRenderValidationTarget(BenchmarkTarget target)
	{
		resetBothToBaseline();
		mFastRoot.SetActive(target == BenchmarkTarget.FastUI);
		mUGUIRoot.SetActive(target == BenchmarkTarget.UGUI);
		Canvas.ForceUpdateCanvases();
	}
	protected void startRenderRecorders()
	{
		stopRenderRecorders();
		mDrawCallsRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count");
		mBatchesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Total Batches Count");
		mSetPassRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "SetPass Calls Count");
		mTrianglesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Triangles Count");
		mVerticesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Vertices Count");
		mVertexUploadBytesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Vertex Buffer Upload In Frame Bytes");
		mIndexUploadBytesRecorder = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Index Buffer Upload In Frame Bytes");
		mRenderRecordersStarted = true;
	}
	protected void stopRenderRecorders()
	{
		if (!mRenderRecordersStarted)
		{
			return;
		}
		mDrawCallsRecorder.Dispose();
		mBatchesRecorder.Dispose();
		mSetPassRecorder.Dispose();
		mTrianglesRecorder.Dispose();
		mVerticesRecorder.Dispose();
		mVertexUploadBytesRecorder.Dispose();
		mIndexUploadBytesRecorder.Dispose();
		mRenderRecordersStarted = false;
	}
	protected void accumulateRenderSample(ref RenderSampleResult result, BenchmarkTarget target, double wallMS)
	{
		++result.mFrames;
		if (mDrawCallsRecorder.Valid) result.mDrawCalls += mDrawCallsRecorder.LastValue;
		if (mBatchesRecorder.Valid) result.mBatches += mBatchesRecorder.LastValue;
		if (mSetPassRecorder.Valid) result.mSetPassCalls += mSetPassRecorder.LastValue;
		if (mTrianglesRecorder.Valid) result.mTriangles += mTrianglesRecorder.LastValue;
		if (mVerticesRecorder.Valid) result.mVertices += mVerticesRecorder.LastValue;
		if (mVertexUploadBytesRecorder.Valid) result.mVertexUploadBytes += mVertexUploadBytesRecorder.LastValue;
		if (mIndexUploadBytesRecorder.Valid) result.mIndexUploadBytes += mIndexUploadBytesRecorder.LastValue;
		if (!mFrameTimingEnabled || FrameTimingManager.GetLatestTimings(1, mFrameTimings) == 0) { return; }
		FrameTiming timing = mFrameTimings[0];
		double cpuMS = timing.cpuFrameTime;
		if (isPlausibleCPUFrameTime(cpuMS, wallMS))
		{
			result.mCPUFrameMS += cpuMS;
			result.mCPUMainThreadFrameMS += timing.cpuMainThreadFrameTime;
			result.mCPURenderThreadFrameMS += timing.cpuRenderThreadFrameTime;
			++result.mCPUFrameTimingFrames;
		}
		else { ++result.mCPUInvalidTimingFrames; }
		double gpuMS = timing.gpuFrameTime;
		if (isPlausibleGPUFrameTime(gpuMS, cpuMS))
		{
			result.mGPUFrameMS += gpuMS;
			++result.mGPUTimingFrames;
			mGPUFrameTimingAvailable = true;
		}
		else if (gpuMS > 0.0) { ++result.mGPUInvalidTimingFrames; }
	}

	protected void reportRenderCase(string caseName, RenderSampleResult fastA, RenderSampleResult uguiA, RenderSampleResult uguiB,
		RenderSampleResult fastB)
	{
		double fastDraw = average(fastA.getDrawCallsAverage(), fastB.getDrawCallsAverage());
		double uguiDraw = average(uguiA.getDrawCallsAverage(), uguiB.getDrawCallsAverage());
		double fastSetPass = average(fastA.getSetPassAverage(), fastB.getSetPassAverage());
		double uguiSetPass = average(uguiA.getSetPassAverage(), uguiB.getSetPassAverage());
		double fastTriangles = average(fastA.getTrianglesAverage(), fastB.getTrianglesAverage());
		double uguiTriangles = average(uguiA.getTrianglesAverage(), uguiB.getTrianglesAverage());
		double fastVertices = average(fastA.getVerticesAverage(), fastB.getVerticesAverage());
		double uguiVertices = average(uguiA.getVerticesAverage(), uguiB.getVerticesAverage());
		double fastVB = average(fastA.getVertexUploadBytesAverage(), fastB.getVertexUploadBytesAverage());
		double uguiVB = average(uguiA.getVertexUploadBytesAverage(), uguiB.getVertexUploadBytesAverage());
		double fastIB = average(fastA.getIndexUploadBytesAverage(), fastB.getIndexUploadBytesAverage());
		double uguiIB = average(uguiA.getIndexUploadBytesAverage(), uguiB.getIndexUploadBytesAverage());
		double fastGPU = averageAvailable(fastA.getGPUFrameAverage(), fastB.getGPUFrameAverage());
		double uguiGPU = averageAvailable(uguiA.getGPUFrameAverage(), uguiB.getGPUFrameAverage());
		double fastWallMedian = averageAvailable(fastA.getWallFrameMedian(), fastB.getWallFrameMedian());
		double uguiWallMedian = averageAvailable(uguiA.getWallFrameMedian(), uguiB.getWallFrameMedian());
		double fastWallP95 = averageAvailable(fastA.getWallFrameP95(), fastB.getWallFrameP95());
		double uguiWallP95 = averageAvailable(uguiA.getWallFrameP95(), uguiB.getWallFrameP95());
		double wallRatio = fastWallMedian > 0.0 && uguiWallMedian > 0.0 ? uguiWallMedian / fastWallMedian : 0.0;
		benchmarkLog("[Render Compare] Case=" + caseName + " | DrawCallsF/U=" + fastDraw.ToString("F1") + "/" + uguiDraw.ToString("F1") +
			" | SetPassF/U=" + fastSetPass.ToString("F1") + "/" + uguiSetPass.ToString("F1") +
			" | TrianglesF/U=" + fastTriangles.ToString("F1") + "/" + uguiTriangles.ToString("F1") +
			" | VerticesF/U=" + fastVertices.ToString("F1") + "/" + uguiVertices.ToString("F1") +
			" | VBUploadF/U=" + fastVB.ToString("F0") + "/" + uguiVB.ToString("F0") + "B" +
			" | IBUploadF/U=" + fastIB.ToString("F0") + "/" + uguiIB.ToString("F0") + "B" +
			" | WallMedianF/U=" + formatOptionalMS(fastWallMedian) + "/" + formatOptionalMS(uguiWallMedian) +
			" | WallU/F=" + wallRatio.ToString("F2") + "x | GPUF/U=" + formatOptionalMS(fastGPU) + "/" + formatOptionalMS(uguiGPU));
		benchmarkLog("[README Render] Case=" + caseName + " | DrawCallsF/U=" + fastDraw.ToString("F1") + "/" + uguiDraw.ToString("F1") +
			" | SetPassF/U=" + fastSetPass.ToString("F1") + "/" + uguiSetPass.ToString("F1") +
			" | TrianglesF/U=" + fastTriangles.ToString("F1") + "/" + uguiTriangles.ToString("F1") +
			" | VerticesF/U=" + fastVertices.ToString("F1") + "/" + uguiVertices.ToString("F1") +
			" | VBUploadF/U=" + fastVB.ToString("F0") + "/" + uguiVB.ToString("F0") + "B" +
			" | IBUploadF/U=" + fastIB.ToString("F0") + "/" + uguiIB.ToString("F0") + "B" +
			" | WallMedianF/U=" + formatOptionalMS(fastWallMedian) + "/" + formatOptionalMS(uguiWallMedian) +
			" | WallP95F/U=" + formatOptionalMS(fastWallP95) + "/" + formatOptionalMS(uguiWallP95) +
			" | GPUF/U=" + formatOptionalMS(fastGPU) + "/" + formatOptionalMS(uguiGPU));
	}
	protected void reportEndToEndFrameCase(string caseName, RenderSampleResult fastA, RenderSampleResult uguiA,
		RenderSampleResult uguiB, RenderSampleResult fastB)
	{
		double fastWallA = fastA.getWallFrameMedian();
		double fastWallB = fastB.getWallFrameMedian();
		double uguiWallA = uguiA.getWallFrameMedian();
		double uguiWallB = uguiB.getWallFrameMedian();
		double fastWall = averageAvailable(fastWallA, fastWallB);
		double uguiWall = averageAvailable(uguiWallA, uguiWallB);
		double fastWallMean = averageAvailable(fastA.getWallFrameAverage(), fastB.getWallFrameAverage());
		double uguiWallMean = averageAvailable(uguiA.getWallFrameAverage(), uguiB.getWallFrameAverage());
		double fastWallP95 = averageAvailable(fastA.getWallFrameP95(), fastB.getWallFrameP95());
		double uguiWallP95 = averageAvailable(uguiA.getWallFrameP95(), uguiB.getWallFrameP95());
		double fastWallSpread = fastWallA >= 0.0 && fastWallB >= 0.0 ? getSpreadPercent(fastWallA, fastWallB) : -1.0;
		double uguiWallSpread = uguiWallA >= 0.0 && uguiWallB >= 0.0 ? getSpreadPercent(uguiWallA, uguiWallB) : -1.0;
		bool wallComplete = fastWallA >= 0.0 && fastWallB >= 0.0 && uguiWallA >= 0.0 && uguiWallB >= 0.0;
		bool wallStable = wallComplete && fastWallSpread <= 20.0 && uguiWallSpread <= 20.0;
		double wallRatio = fastWall > 0.000001 && uguiWall >= 0.0 ? uguiWall / fastWall : -1.0;
		double fastCPUFrame = averageAvailable(fastA.getCPUFrameAverage(), fastB.getCPUFrameAverage());
		double uguiCPUFrame = averageAvailable(uguiA.getCPUFrameAverage(), uguiB.getCPUFrameAverage());
		double fastMainFrame = averageAvailable(fastA.getCPUMainThreadFrameAverage(), fastB.getCPUMainThreadFrameAverage());
		double uguiMainFrame = averageAvailable(uguiA.getCPUMainThreadFrameAverage(), uguiB.getCPUMainThreadFrameAverage());
		double fastRenderFrame = averageAvailable(fastA.getCPURenderThreadFrameAverage(), fastB.getCPURenderThreadFrameAverage());
		double uguiRenderFrame = averageAvailable(uguiA.getCPURenderThreadFrameAverage(), uguiB.getCPURenderThreadFrameAverage());
		double fastGPUFrame = averageAvailable(fastA.getGPUFrameAverage(), fastB.getGPUFrameAverage());
		double uguiGPUFrame = averageAvailable(uguiA.getGPUFrameAverage(), uguiB.getGPUFrameAverage());
		benchmarkLog("[FastUI vs UGUI EndToEnd] " + caseName +
			" | WallMedian F/U:" + formatOptionalMS(fastWall) + "/" + formatOptionalMS(uguiWall) +
			" | WallMean F/U:" + formatOptionalMS(fastWallMean) + "/" + formatOptionalMS(uguiWallMean) +
			" | WallP95 F/U:" + formatOptionalMS(fastWallP95) + "/" + formatOptionalMS(uguiWallP95) +
			" | Wall U/F:" + formatOptionalRatio(wallRatio) +
			" | WallSpread F/U:" + formatOptionalPercent(fastWallSpread) + "/" + formatOptionalPercent(uguiWallSpread) +
			" | WallStable:" + (wallComplete ? wallStable.ToString() : "Unavailable") +
			" | FrameTimingCPU F/U:" + formatOptionalMS(fastCPUFrame) + "/" + formatOptionalMS(uguiCPUFrame) +
			" | Main F/U:" + formatOptionalMS(fastMainFrame) + "/" + formatOptionalMS(uguiMainFrame) +
			" | Render F/U:" + formatOptionalMS(fastRenderFrame) + "/" + formatOptionalMS(uguiRenderFrame) +
			" | GPU F/U:" + formatOptionalMS(fastGPUFrame) + "/" + formatOptionalMS(uguiGPUFrame));
	}

	protected bool validateFastSOA()
	{
		if (!mFastEnableSOA)
		{
			benchmarkLog("[FastUI vs UGUI Inventory] FastSOA:Enabled=False | Valid=True");
			return true;
		}
		int expectedGroupCount = mFastEnableSuggestedSOA ? 3 : 1;
		int expectedRecordCount = mFastEnableSuggestedSOA ? mItemCount + 9 : mItemCount;
		bool expectAdjacentMerge = mFastEnableSuggestedSOA && mFastEnableAdjacentSOAMerge;
		int expectedLaneCount = mFastEnableSuggestedSOA ? (expectAdjacentMerge ? 10 : 12) : 8;
		int expectedElementCount = mFastEnableSuggestedSOA ? mItemCount * 8 + 18 : mItemCount * 8;
		int expectedBatchKeyGroupCount = mFastEnableSuggestedSOA ? (expectAdjacentMerge ? 10 : 12) : 8;
		int expectedAdjacentMergeClusters = expectAdjacentMerge ? 1 : 0;
		int expectedAdjacentMergedGroups = expectAdjacentMerge ? 2 : 0;
		bool groupsValid = mFastSOAGroup != null && mFastSOAGroup.getLastBuildValid();
		if (mFastEnableSuggestedSOA)
		{
			groupsValid &= mFastCurrencySOAGroup != null && mFastCurrencySOAGroup.getLastBuildValid() &&
				mFastTabsSOAGroup != null && mFastTabsSOAGroup.getLastBuildValid();
		}
		bool valid = groupsValid &&
			mFastCanvas.getSOAGroupCount() == expectedGroupCount &&
			mFastCanvas.getSOAConvertedGroupCount() == expectedGroupCount &&
			mFastCanvas.getSOARootGroupCount() == expectedGroupCount &&
			mFastCanvas.getSOANestedGroupCount() == 0 &&
			mFastCanvas.getSOAInvalidGroupCount() == 0 &&
			mFastCanvas.getSOAAdjacentMergeClusterCount() == expectedAdjacentMergeClusters &&
			mFastCanvas.getSOAAdjacentMergedGroupCount() == expectedAdjacentMergedGroups &&
			mFastCanvas.getSOARecordCount() == expectedRecordCount &&
			mFastCanvas.getSOAProjectedElementCount() == expectedElementCount &&
			mFastCanvas.getSOALaneCount() == expectedLaneCount &&
			mFastCanvas.getSOABatchKeyGroupCount() == expectedBatchKeyGroupCount &&
			mFastCanvas.isSOADrawOrderActive();
		benchmarkLog("[FastUI vs UGUI Inventory] FastSOA:Enabled=True | SuggestedSOA=" + mFastEnableSuggestedSOA +
			" | Groups=" + mFastCanvas.getSOAGroupCount() + " | Root=" + mFastCanvas.getSOARootGroupCount() +
			" | Nested=" + mFastCanvas.getSOANestedGroupCount() + " | Converted=" + mFastCanvas.getSOAConvertedGroupCount() +
			" | Invalid=" + mFastCanvas.getSOAInvalidGroupCount() + " | AdjacentMerge=" + mFastCanvas.getSOAAdjacentMergeClusterCount() + "/" + mFastCanvas.getSOAAdjacentMergedGroupCount() +
			" | Records=" + mFastCanvas.getSOARecordCount() +
			" | Lanes=" + mFastCanvas.getSOALaneCount() + " | Elements=" + mFastCanvas.getSOAProjectedElementCount() +
			" | BatchKeyGroups=" + mFastCanvas.getSOABatchKeyGroupCount() + " | ExpectedBatchKeyGroups=" + expectedBatchKeyGroupCount +
			" | Reordered=" + mFastCanvas.isSOADrawOrderActive() + " | Valid=" + valid);
		if (!valid)
		{
			benchmarkLogError("[FastUI vs UGUI Inventory] Fixed-Structure SOA构建无效，停止Benchmark。ExpectedBatchKeyGroups=" + expectedBatchKeyGroupCount +
				",ActualBatchKeyGroups=" + mFastCanvas.getSOABatchKeyGroupCount() + " | ContentError=" + (mFastSOAGroup != null ? mFastSOAGroup.getLastBuildError() : "null") +
				" | CurrencyError=" + (mFastCurrencySOAGroup != null ? mFastCurrencySOAGroup.getLastBuildError() : "null") +
				" | TabsError=" + (mFastTabsSOAGroup != null ? mFastTabsSOAGroup.getLastBuildError() : "null"));
		}
		return valid;
	}
	protected bool validateFastSuggestedSOABatchResult()
	{
		if (!mFastEnableSOA || !mFastEnableSuggestedSOA)
		{
			benchmarkLog("[FastUI vs UGUI Inventory] SuggestedSOABatch:Enabled=False | Valid=True");
			return true;
		}
		int logicalBatchCount = mFastCanvas.getLogicalBatchCount();
		int submitDrawRunCount = mFastCanvas.getBatchCount();
		int expectedBatchCount = mFastEnableAdjacentSOAMerge ? 11 : 13;
		bool valid = logicalBatchCount == expectedBatchCount;
		benchmarkLog("[FastUI vs UGUI Inventory] SuggestedSOABatch:Enabled=True | AdjacentMerge=" + mFastEnableAdjacentSOAMerge + " | FastBatch=" + logicalBatchCount +
			" | SubmitDrawRuns=" + submitDrawRunCount + " | Expected=" + expectedBatchCount + " | Atlas=SingleTextureMultiSprite | Valid=" + valid);
		if (!valid)
		{
			benchmarkLogError("[FastUI vs UGUI Inventory] 正常MultiSprite背包应用SOA后的逻辑Batch数量不符合预期,Expected=" + expectedBatchCount + ",Actual=" + logicalBatchCount + ",SubmitDrawRuns=" + submitDrawRunCount);
		}
		return valid;
	}
	protected bool validateFastClipStencilRenderOrder()
	{
		FastClipStencilGraphic writer = mFastViewportClip?.getWriterGraphic();
		FastClipStencilGraphic pop = mFastViewportClip?.getPopGraphic();
		if (writer == null || pop == null)
		{
			benchmarkLogError("[FastUI vs UGUI Inventory] Stencil RenderOrder校验失败:Writer/Pop为空.");
			return false;
		}
		FastUIMeshRenderer renderer = mFastCanvas.getMeshRenderer();
		int writerRenderOrder = writer.getRenderOrderIndex();
		int popRenderOrder = pop.getRenderOrderIndex();
		int writerOrder = renderer.getDrawIndexForRenderIndex(writerRenderOrder);
		int popOrder = renderer.getDrawIndexForRenderIndex(popRenderOrder);
		if (writerOrder < 0 || popOrder <= writerOrder)
		{
			benchmarkLogError("[FastUI vs UGUI Inventory] Stencil DrawOrder校验失败:Writer=" + writerOrder + ",Pop=" + popOrder);
			return false;
		}
		FastUIRenderElement[] elements = mFastViewportClip.GetComponentsInChildren<FastUIRenderElement>(true);
		int checkedCount = 0;
		for (int i = 0; i < elements.Length; ++i)
		{
			FastUIRenderElement element = elements[i];
			if (element == null || element == writer || element == pop || element.getCanvas() != mFastCanvas)
			{
				continue;
			}
			int renderOrder = element.getRenderOrderIndex();
			if (renderOrder < 0)
			{
				continue;
			}
			int order = renderer.getDrawIndexForRenderIndex(renderOrder);
			++checkedCount;
			if (order <= writerOrder || order >= popOrder)
			{
				benchmarkLogError("[FastUI vs UGUI Inventory] Stencil RenderOrder校验失败:节点=" + element.name + ",Order=" + order +
					",Writer=" + writerOrder + ",Pop=" + popOrder);
				return false;
			}
		}
		benchmarkLog("[FastUI vs UGUI Inventory] FastClipStencilDrawOrder:Writer=" + writerOrder + " | Pop=" + popOrder +
			" | DescendantsChecked=" + checkedCount + " | SOAActive=" + mFastCanvas.isSOADrawOrderActive() +
			" | Reordered=" + renderer.isDrawOrderReordered() + " | Valid=True");
		return true;
	}
	protected double averageAvailable(double first, double second)
	{
		if (first < 0.0)
		{
			return second;
		}
		if (second < 0.0)
		{
			return first;
		}
		return average(first, second);
	}
	protected string formatOptionalMS(double value) { return value >= 0.0 ? value.ToString("F3") + "ms" : "Unavailable"; }
	protected string formatOptionalRatio(double value) { return value >= 0.0 ? value.ToString("F2") + "x" : "Unavailable"; }
	protected string formatOptionalPercent(double value) { return value >= 0.0 ? value.ToString("F1") + "%" : "Unavailable"; }
	protected void executeMutation(InventoryCase testCase, BenchmarkTarget target)
	{
		switch (testCase)
		{
			case InventoryCase.SingleItemRefresh: refreshRange(target, getWrappedIndex(mMutationStep * 17), 1, true, true, true); break;
			case InventoryCase.VisiblePageRefresh: refreshRange(target, getRangeStart(mMutationStep * 13, mVisibleRefreshCount), mVisibleRefreshCount, true, true, true); break;
			case InventoryCase.FullInventoryRebind: refreshRange(target, 0, mItemCount, true, true, true); break;
			case InventoryCase.QuantityTextRefresh: refreshRange(target, getRangeStart(mMutationStep * 7, mBatchRefreshCount), mBatchRefreshCount, false, true, false); break;
			case InventoryCase.IconSpriteRefresh: refreshRange(target, getRangeStart(mMutationStep * 11, mBatchRefreshCount), mBatchRefreshCount, true, false, false); break;
			case InventoryCase.ContentScroll: scrollContent(target); break;
			case InventoryCase.SelectionDetailRefresh: refreshSelectionAndDetail(target); break;
			case InventoryCase.FilterToggle: toggleFilterItems(target); break;
			case InventoryCase.WindowFastVisibility: toggleWindowFastVisibility(target); break;
			case InventoryCase.WindowSetActive: toggleWindowActive(target); break;
			case InventoryCase.MixedUse: executeMixedUse(target); break;
			case InventoryCase.IconPositionRefresh: refreshIconPositions(target); break;
			case InventoryCase.ItemRootPositionRefresh: refreshItemRootPositions(target); break;
			case InventoryCase.IconSizeRefresh: refreshIconSizes(target); break;
			case InventoryCase.QualityColorRefresh: refreshQualityColors(target); break;
			case InventoryCase.ScatteredIconSpriteRefresh: refreshScatteredIconSprites(target); break;
			case InventoryCase.LeafVisibilityRefresh: refreshLeafVisibility(target); break;
			case InventoryCase.TextColorRefresh: refreshTextColors(target); break;
			case InventoryCase.TextFontSizeRefresh: refreshTextFontSizes(target); break;
			case InventoryCase.IconTransformRefresh: refreshIconTransforms(target); break;
			case InventoryCase.RectMaskResize: resizeRectMask(target); break;
			case InventoryCase.StressMixedRefresh: executeStressMixedRefresh(target); break;
			case InventoryCase.FullTextRefresh: refreshRange(target, 0, mItemCount, false, true, false); break;
			case InventoryCase.FullStateRefresh: refreshRange(target, 0, mItemCount, false, false, true); break;
			case InventoryCase.FullLeafVisibilityRefresh: refreshAllLeafVisibility(target); break;
			case InventoryCase.ScatteredFilterToggle: toggleScatteredFilterItems(target); break;
			case InventoryCase.StressIconGeometryRefresh: refreshStressIconGeometry(target); break;
			case InventoryCase.StressItemRootPositionRefresh: refreshStressItemRootPositions(target); break;
			case InventoryCase.IconMaterialRefresh: refreshIconMaterials(target); break;
			case InventoryCase.RectMaskPaddingRefresh: refreshRectMaskPadding(target); break;
			case InventoryCase.RectMaskMoveResize: moveResizeRectMask(target); break;
		}
	}
	protected void refreshRange(BenchmarkTarget target, int start, int count, bool texture, bool text, bool state, int variantOffset = 0)
	{
		int end = Mathf.Min(start + count, mItemCount);
		for (int i = start; i < end; ++i)
		{
			int variant = mMutationStep + variantOffset + i;
			if (target == BenchmarkTarget.FastUI)
			{
				refreshFastItem(mFastItems[i], i, variant, texture, text, state);
			}
			else
			{
				refreshUGUIItem(mUGUIItems[i], i, variant, texture, text, state);
			}
		}
	}
	protected void refreshFastItem(FastItemView item, int index, int variant, bool image, bool text, bool state)
	{
		if (image)
		{
			item.mIcon.setSprite(getInventoryIconSprite(index + variant));
		}
		if (text)
		{
			item.mCountText.setText(mNumberStrings[(variant * 37 + index * 11) % mNumberStrings.Length]);
			item.mLevelText.setText(mNumberStrings[(variant + index) % 100]);
		}
		if (state)
		{
			item.mQualityFrame.setColor(getQualityColor(index + variant));
			item.mLockIcon.setVisible((variant & 3) == 0);
			item.mBindIcon.setVisible((variant & 7) <= 1);
		}
	}
	protected void refreshUGUIItem(UGUIItemView item, int index, int variant, bool image, bool text, bool state)
	{
		if (image)
		{
			item.mIcon.sprite = getInventoryIconSprite(index + variant);
		}
		if (text)
		{
			item.mCountText.text = mNumberStrings[(variant * 37 + index * 11) % mNumberStrings.Length];
			item.mLevelText.text = mNumberStrings[(variant + index) % 100];
		}
		if (state)
		{
			item.mQualityFrame.color = getQualityColor(index + variant);
			item.mLockIcon.enabled = (variant & 3) == 0;
			item.mBindIcon.enabled = (variant & 7) <= 1;
		}
	}
	protected void scrollContent(BenchmarkTarget target)
	{
		float maxScroll = getMaxScroll();
		float distance = maxScroll <= 0.0f ? 0.0f : (mMutationStep * mScrollStep) % (maxScroll * 2.0f);
		float y = distance <= maxScroll ? distance : maxScroll * 2.0f - distance;
		if (target == BenchmarkTarget.FastUI)
		{
			Vector3 oldPosition = mFastContent.localPosition;
			Vector3 newPosition = new(oldPosition.x, y, oldPosition.z);
			if (oldPosition == newPosition)
			{
				return;
			}
			mFastContent.localPosition = newPosition;
			mFastCanvas.notifyTransformPositionChanged(mFastContent, oldPosition, newPosition);
		}
		else
		{
			Vector3 position = mUGUIContent.localPosition;
			position.y = y;
			mUGUIContent.localPosition = position;
		}
	}
	protected void refreshSelectionAndDetail(BenchmarkTarget target)
	{
		int index = getWrappedIndex(mMutationStep * 19);
		if (target == BenchmarkTarget.FastUI)
		{
			if (mFastSelectedIndex >= 0 && mFastSelectedIndex != index)
			{
				mFastItems[mFastSelectedIndex].mSelected.setVisible(false);
			}
			mFastItems[index].mSelected.setVisible(true);
			mFastSelectedIndex = index;
			mFastDetailIcon.setSprite(getInventoryIconSprite(index + mMutationStep));
			mFastDetailName.setText(mItemNames[index]);
			mFastDetailDescription.setText(mItemDescriptions[index]);
		}
		else
		{
			if (mUGUISelectedIndex >= 0 && mUGUISelectedIndex != index)
			{
				mUGUIItems[mUGUISelectedIndex].mSelected.enabled = false;
			}
			mUGUIItems[index].mSelected.enabled = true;
			mUGUISelectedIndex = index;
			mUGUIDetailIcon.sprite = getInventoryIconSprite(index + mMutationStep);
			mUGUIDetailName.text = mItemNames[index];
			mUGUIDetailDescription.text = mItemDescriptions[index];
		}
	}
	protected void toggleFilterItems(BenchmarkTarget target)
	{
		int count = Mathf.Min(mBatchRefreshCount, mItemCount);
		int start = getRangeStart(0, count);
		bool visible = (mMutationStep & 1) == 0;
		for (int i = start; i < start + count; ++i)
		{
			if (target == BenchmarkTarget.FastUI)
			{
				mFastItems[i].mVisibility.setVisible(visible);
			}
			else
			{
				mUGUIItems[i].mRoot.gameObject.SetActive(visible);
			}
		}
	}
	protected void toggleWindowFastVisibility(BenchmarkTarget target)
	{
		bool visible = (mMutationStep & 1) == 0;
		if (target == BenchmarkTarget.FastUI)
		{
			mFastCanvas.setVisible(visible);
		}
		else
		{
			mUGUIRoot.SetActive(visible);
		}
	}
	protected void toggleWindowActive(BenchmarkTarget target)
	{
		bool active = (mMutationStep & 1) == 0;
		if (target == BenchmarkTarget.FastUI)
		{
			mFastRoot.SetActive(active);
		}
		else
		{
			mUGUIRoot.SetActive(active);
		}
	}
	protected void executeMixedUse(BenchmarkTarget target)
	{
		scrollContent(target);
		refreshRange(target, getRangeStart(mMutationStep * 5, 20), Mathf.Min(20, mItemCount), false, true, false);
		refreshRange(target, getRangeStart(mMutationStep * 23, 4), Mathf.Min(4, mItemCount), true, false, true);
		refreshSelectionAndDetail(target);
	}
	protected void refreshIconPositions(BenchmarkTarget target)
	{
		int count = Mathf.Min(mBatchRefreshCount, mItemCount);
		int start = getRangeStart(mMutationStep * 7, count);
		float offset = (mMutationStep & 1) == 0 ? 3.0f : -3.0f;
		for (int i = start; i < start + count; ++i)
		{
			Vector3 position = new(offset, 4.0f + ((i & 1) == 0 ? 2.0f : -2.0f), 0.0f);
			if (target == BenchmarkTarget.FastUI)
			{
				mFastItems[i].mIcon.setLocalPosition(position);
			}
			else
			{
				mUGUIItems[i].mIcon.rectTransform.localPosition = position;
			}
		}
	}
	protected void refreshItemRootPositions(BenchmarkTarget target)
	{
		int count = Mathf.Min(mVisibleRefreshCount, mItemCount);
		int start = getRangeStart(mMutationStep * 5, count);
		float offset = (mMutationStep & 1) == 0 ? 4.0f : -4.0f;
		if (target == BenchmarkTarget.FastUI)
		{
			mFastCanvas.beginTransformPositionBatch();
		}
		try
		{
			for (int i = start; i < start + count; ++i)
			{
				Vector3 position = getItemPosition(i) + new Vector3(offset, 0.0f, 0.0f);
				if (target == BenchmarkTarget.FastUI)
				{
					RectTransform rect = mFastItems[i].mRoot;
					Vector3 oldPosition = rect.localPosition;
					if (oldPosition != position)
					{
						rect.localPosition = position;
						mFastCanvas.notifyTransformPositionChanged(rect, oldPosition, position);
					}
				}
				else
				{
					mUGUIItems[i].mRoot.localPosition = position;
				}
			}
		}
		finally
		{
			if (target == BenchmarkTarget.FastUI)
			{
				mFastCanvas.endTransformPositionBatch();
			}
		}
	}
	protected void refreshIconSizes(BenchmarkTarget target)
	{
		int count = Mathf.Min(mBatchRefreshCount, mItemCount);
		int start = getRangeStart(mMutationStep * 9, count);
		float size = (mMutationStep & 1) == 0 ? 58.0f : 50.0f;
		Vector2 value = new(size, size);
		for (int i = start; i < start + count; ++i)
		{
			if (target == BenchmarkTarget.FastUI)
			{
				mFastItems[i].mIcon.setSize(value);
			}
			else
			{
				mUGUIItems[i].mIcon.rectTransform.sizeDelta = value;
			}
		}
	}
	protected void refreshQualityColors(BenchmarkTarget target)
	{
		int count = Mathf.Min(mBatchRefreshCount, mItemCount);
		int start = getRangeStart(mMutationStep * 3, count);
		for (int i = start; i < start + count; ++i)
		{
			Color color = getQualityColor(i + mMutationStep + 1);
			if (target == BenchmarkTarget.FastUI)
			{
				mFastItems[i].mQualityFrame.setColor(color);
			}
			else
			{
				mUGUIItems[i].mQualityFrame.color = color;
			}
		}
	}
	protected void refreshScatteredIconSprites(BenchmarkTarget target)
	{
		int count = Mathf.Min(mBatchRefreshCount, mItemCount);
		int phase = getWrappedIndex(mMutationStep * 17);
		for (int i = 0; i < count; ++i)
		{
			int index = (phase + i * 37) % mItemCount;
			Sprite sprite = getInventoryIconSprite(index + mMutationStep + 1);
			if (target == BenchmarkTarget.FastUI)
			{
				mFastItems[index].mIcon.setSprite(sprite);
			}
			else
			{
				mUGUIItems[index].mIcon.sprite = sprite;
			}
		}
	}
	protected void refreshLeafVisibility(BenchmarkTarget target)
	{
		int count = Mathf.Min(mBatchRefreshCount, mItemCount);
		int start = getRangeStart(mMutationStep * 13, count);
		bool visible = (mMutationStep & 1) == 0;
		for (int i = start; i < start + count; ++i)
		{
			if (target == BenchmarkTarget.FastUI)
			{
				mFastItems[i].mBindIcon.setVisible(visible);
			}
			else
			{
				mUGUIItems[i].mBindIcon.enabled = visible;
			}
		}
	}
	protected void refreshTextColors(BenchmarkTarget target)
	{
		int count = Mathf.Min(mBatchRefreshCount, mItemCount);
		int start = getRangeStart(mMutationStep * 17, count);
		Color color = (mMutationStep & 1) == 0 ? Color.white : new Color(1.0f, 0.75f, 0.3f, 1.0f);
		for (int i = start; i < start + count; ++i)
		{
			if (target == BenchmarkTarget.FastUI)
			{
				mFastItems[i].mCountText.setColor(color);
			}
			else
			{
				mUGUIItems[i].mCountText.color = color;
			}
		}
	}
	protected void refreshTextFontSizes(BenchmarkTarget target)
	{
		int count = Mathf.Min(mBatchRefreshCount, mItemCount);
		int start = getRangeStart(mMutationStep * 19, count);
		float fontSize = (mMutationStep & 1) == 0 ? 17.0f : 13.0f;
		for (int i = start; i < start + count; ++i)
		{
			if (target == BenchmarkTarget.FastUI)
			{
				mFastItems[i].mCountText.setFontSize(fontSize);
			}
			else
			{
				mUGUIItems[i].mCountText.fontSize = fontSize;
			}
		}
	}
	protected void refreshIconTransforms(BenchmarkTarget target)
	{
		int count = Mathf.Min(mVisibleRefreshCount, mItemCount);
		int start = getRangeStart(mMutationStep * 7, count);
		float scale = (mMutationStep & 1) == 0 ? 1.08f : 0.92f;
		Quaternion rotation = Quaternion.Euler(0.0f, 0.0f, (mMutationStep & 1) == 0 ? 6.0f : -6.0f);
		for (int i = start; i < start + count; ++i)
		{
			if (target == BenchmarkTarget.FastUI)
			{
				mFastItems[i].mIcon.setLocalScale(new Vector3(scale, scale, 1.0f));
				mFastItems[i].mIcon.setLocalRotation(rotation);
			}
			else
			{
				RectTransform rect = mUGUIItems[i].mIcon.rectTransform;
				rect.localScale = new Vector3(scale, scale, 1.0f);
				rect.localRotation = rotation;
			}
		}
	}
	protected void resizeRectMask(BenchmarkTarget target)
	{
		Vector2 size = (mMutationStep & 1) == 0 ? mViewportSize : mViewportSize - new Vector2(48.0f, 36.0f);
		if (target == BenchmarkTarget.FastUI)
		{
			mFastViewportRect.sizeDelta = size;
		}
		else
		{
			mUGUIViewportRect.sizeDelta = size;
		}
	}
	protected void executeStressMixedRefresh(BenchmarkTarget target)
	{
		scrollContent(target);
		int count = Mathf.Min(mStressRefreshCount, mItemCount);
		for (int pass = 0; pass < PUBLICATION_STRESS_MUTATION_PASSES; ++pass)
		{
			int start = getRangeStart(mMutationStep * 29 + pass * 197, count);
			refreshRange(target, start, count, true, true, true, (pass + 1) * 31);
		}
		refreshStressIconGeometry(target);
		refreshSelectionAndDetail(target);
	}
	protected void refreshAllLeafVisibility(BenchmarkTarget target)
	{
		bool visible = (mMutationStep & 1) == 0;
		for (int i = 0; i < mItemCount; ++i)
		{
			if (target == BenchmarkTarget.FastUI)
			{
				mFastItems[i].mBindIcon.setVisible(visible);
			}
			else
			{
				mUGUIItems[i].mBindIcon.enabled = visible;
			}
		}
	}
	protected void toggleScatteredFilterItems(BenchmarkTarget target)
	{
		int count = Mathf.Min(mScatterRefreshCount, mItemCount);
		bool visible = (mMutationStep & 1) == 0;
		int phase = getWrappedIndex(mMutationStep * 11);
		for (int i = 0; i < count; ++i)
		{
			int index = (phase + i * mScatterStride) % mItemCount;
			if (target == BenchmarkTarget.FastUI)
			{
				mFastItems[index].mVisibility.setVisible(visible);
			}
			else
			{
				mUGUIItems[index].mRoot.gameObject.SetActive(visible);
			}
		}
	}
	protected void refreshStressIconGeometry(BenchmarkTarget target)
	{
		int count = Mathf.Min(mStressRefreshCount, mItemCount);
		int start = getRangeStart(mMutationStep * 31, count);
		float size = (mMutationStep & 1) == 0 ? 60.0f : 48.0f;
		float offset = (mMutationStep & 1) == 0 ? 4.0f : -4.0f;
		Vector2 sizeValue = new(size, size);
		for (int i = start; i < start + count; ++i)
		{
			Vector3 position = new(offset, 4.0f + ((i & 1) == 0 ? 2.0f : -2.0f), 0.0f);
			Sprite sprite = getInventoryIconSprite(i + mMutationStep);
			if (target == BenchmarkTarget.FastUI)
			{
				mFastItems[i].mIcon.setSize(sizeValue);
				mFastItems[i].mIcon.setSprite(sprite);
				mFastItems[i].mIcon.setLocalPosition(position);
			}
			else
			{
				Image image = mUGUIItems[i].mIcon;
				image.rectTransform.sizeDelta = sizeValue;
				image.sprite = sprite;
				image.rectTransform.localPosition = position;
			}
		}
	}
	protected void refreshStressItemRootPositions(BenchmarkTarget target)
	{
		int count = Mathf.Min(mStressRefreshCount, mItemCount);
		int start = getRangeStart(mMutationStep * 37, count);
		float offset = (mMutationStep & 1) == 0 ? 5.0f : -5.0f;
		if (target == BenchmarkTarget.FastUI)
		{
			mFastCanvas.beginTransformPositionBatch();
		}
		try
		{
			for (int i = start; i < start + count; ++i)
			{
				Vector3 position = getItemPosition(i) + new Vector3(offset, 0.0f, 0.0f);
				if (target == BenchmarkTarget.FastUI)
				{
					RectTransform rect = mFastItems[i].mRoot;
					Vector3 oldPosition = rect.localPosition;
					if (oldPosition != position)
					{
						rect.localPosition = position;
						mFastCanvas.notifyTransformPositionChanged(rect, oldPosition, position);
					}
				}
				else
				{
					mUGUIItems[i].mRoot.localPosition = position;
				}
			}
		}
		finally
		{
			if (target == BenchmarkTarget.FastUI)
			{
				mFastCanvas.endTransformPositionBatch();
			}
		}
	}
	protected void refreshIconMaterials(BenchmarkTarget target)
	{
		if (mBenchmarkMaterialA == null || mBenchmarkMaterialB == null)
		{
			return;
		}
		int count = Mathf.Min(mBatchRefreshCount, mItemCount);
		int start = getRangeStart(mMutationStep * 41, count);
		Material material = (mMutationStep & 1) == 0 ? mBenchmarkMaterialA : mBenchmarkMaterialB;
		for (int i = start; i < start + count; ++i)
		{
			if (target == BenchmarkTarget.FastUI)
			{
				mFastItems[i].mIcon.setMaterial(material);
			}
			else
			{
				mUGUIItems[i].mIcon.material = material;
			}
		}
	}
	protected void refreshRectMaskPadding(BenchmarkTarget target)
	{
		Vector4 padding = (mMutationStep & 1) == 0 ? Vector4.zero : new Vector4(12.0f, 8.0f, 12.0f, 8.0f);
		if (target == BenchmarkTarget.FastUI)
		{
			mFastViewportClip.setPadding(padding);
		}
		else if (mUGUIViewportMask != null)
		{
			mUGUIViewportMask.padding = padding;
		}
	}
	protected void moveResizeRectMask(BenchmarkTarget target)
	{
		Vector2 size = (mMutationStep & 1) == 0 ? mViewportSize : mViewportSize - new Vector2(56.0f, 40.0f);
		Vector3 baselinePosition = new(-150.0f, -40.0f, 0.0f);
		Vector3 position = baselinePosition + ((mMutationStep & 1) == 0 ? Vector3.zero : new Vector3(10.0f, -8.0f, 0.0f));
		if (target == BenchmarkTarget.FastUI)
		{
			Vector3 oldPosition = mFastViewportRect.localPosition;
			mFastViewportRect.localPosition = position;
			mFastViewportRect.sizeDelta = size;
			if (oldPosition != position)
			{
				mFastCanvas.notifyTransformPositionChanged(mFastViewportRect, oldPosition, position);
			}
		}
		else
		{
			mUGUIViewportRect.localPosition = position;
			mUGUIViewportRect.sizeDelta = size;
		}
	}
	protected void resetBothToBaseline()
	{
		resetFastBaseline();
		resetUGUIBaseline();
	}
	protected void resetFastBaseline()
	{
		if (mFastRoot == null)
		{
			return;
		}
		mFastRoot.SetActive(true);
		mFastCanvas.setVisible(true);
		if (mFastViewportClip != null)
		{
			mFastViewportClip.enabled = true;
		}
		if (mFastViewportRect != null)
		{
			Vector3 oldPosition = mFastViewportRect.localPosition;
			Vector3 baselinePosition = new(-150.0f, -40.0f, 0.0f);
			mFastViewportRect.localPosition = baselinePosition;
			mFastViewportRect.sizeDelta = mViewportSize;
			if (oldPosition != baselinePosition)
			{
				mFastCanvas.notifyTransformPositionChanged(mFastViewportRect, oldPosition, baselinePosition);
			}
		}
		mFastViewportClip?.setPadding(Vector4.zero);
		if (mFastContent.localPosition != Vector3.zero)
		{
			Vector3 oldPosition = mFastContent.localPosition;
			mFastContent.localPosition = Vector3.zero;
			mFastCanvas.notifyTransformPositionChanged(mFastContent, oldPosition, Vector3.zero);
		}
		for (int i = 0; i < mFastItems.Length; ++i)
		{
			FastItemView item = mFastItems[i];
			item.mVisibility.setVisible(true);
			Vector3 basePosition = getItemPosition(i);
			Vector3 oldRootPosition = item.mRoot.localPosition;
			if (oldRootPosition != basePosition)
			{
				item.mRoot.localPosition = basePosition;
				mFastCanvas.notifyTransformPositionChanged(item.mRoot, oldRootPosition, basePosition);
			}
			item.mIcon.setLocalPosition(new Vector3(0.0f, 4.0f, 0.0f));
			item.mIcon.setSize(new Vector2(54.0f, 54.0f));
			item.mIcon.setLocalScale(Vector3.one);
			item.mIcon.setLocalRotation(Quaternion.identity);
			item.mIcon.setMaterial(null);
			item.mIcon.setSprite(getInventoryIconSprite(i));
			item.mQualityFrame.setColor(getQualityColor(i));
			item.mBindIcon.setVisible((i & 7) == 0);
			item.mLockIcon.setVisible((i & 15) == 0);
			item.mBackground.setVisible(true);
			item.mIcon.setVisible(true);
			item.mQualityFrame.setVisible(true);
			item.mSelected.setVisible(false);
			item.mCountText.setVisible(true);
			item.mLevelText.setVisible(true);
			item.mCountText.setColor(Color.white);
			item.mCountText.setFontSize(15.0f);
			item.mCountText.setText(mNumberStrings[(i * 7) % mNumberStrings.Length]);
			item.mLevelText.setText(mNumberStrings[(i % 99) + 1]);
		}
		mFastSelectedIndex = -1;
		mFastDetailIcon.setSprite(getInventoryIconSprite(0));
		mFastDetailName.setText(mItemNames[0]);
		mFastDetailDescription.setText(mItemDescriptions[0]);
	}
	protected void resetUGUIBaseline()
	{
		if (mUGUIRoot == null)
		{
			return;
		}
		mUGUIRoot.SetActive(true);
		if (mUGUIViewportMask != null)
		{
			mUGUIViewportMask.enabled = true;
		}
		if (mUGUIViewportRect != null)
		{
			mUGUIViewportRect.localPosition = new Vector3(-150.0f, -40.0f, 0.0f);
			mUGUIViewportRect.sizeDelta = mViewportSize;
		}
		if (mUGUIViewportMask != null)
		{
			mUGUIViewportMask.padding = Vector4.zero;
		}
		mUGUIContent.localPosition = Vector3.zero;
		for (int i = 0; i < mUGUIItems.Length; ++i)
		{
			UGUIItemView item = mUGUIItems[i];
			item.mRoot.gameObject.SetActive(true);
			item.mRoot.localPosition = getItemPosition(i);
			item.mIcon.rectTransform.localPosition = new Vector3(0.0f, 4.0f, 0.0f);
			item.mIcon.rectTransform.sizeDelta = new Vector2(54.0f, 54.0f);
			item.mIcon.rectTransform.localScale = Vector3.one;
			item.mIcon.rectTransform.localRotation = Quaternion.identity;
			item.mIcon.material = mImageMaterial;
			item.mIcon.sprite = getInventoryIconSprite(i);
			item.mQualityFrame.color = getQualityColor(i);
			item.mBindIcon.enabled = (i & 7) == 0;
			item.mLockIcon.enabled = (i & 15) == 0;
			item.mBackground.enabled = true;
			item.mIcon.enabled = true;
			item.mQualityFrame.enabled = true;
			item.mSelected.enabled = false;
			item.mCountText.enabled = true;
			item.mLevelText.enabled = true;
			item.mCountText.color = Color.white;
			item.mCountText.fontSize = 15.0f;
			item.mCountText.text = mNumberStrings[(i * 7) % mNumberStrings.Length];
			item.mLevelText.text = mNumberStrings[(i % 99) + 1];
		}
		mUGUISelectedIndex = -1;
		mUGUIDetailIcon.sprite = getInventoryIconSprite(0);
		mUGUIDetailName.text = mItemNames[0];
		mUGUIDetailDescription.text = mItemDescriptions[0];
	}
	protected void createSparseSOANegativeValidation()
	{
		mSparseNegativeValidationCases = new SparseNegativeValidationCase[5];
		mSparseNegativeValidationCases[0] = createSparseCycleNegativeCase();
		mSparseNegativeValidationCases[1] = createSparseTypeMismatchNegativeCase();
		mSparseNegativeValidationCases[2] = createSparseEmptyRecordNegativeCase();
		mSparseNegativeValidationCases[3] = createSparseStencilBarrierNegativeCase();
		mSparseNegativeValidationCases[4] = createSparseNestedNegativeCase();
	}
	protected SparseNegativeValidationCase createSparseNegativeValidationCase(string name, string expectedError, out RectTransform sparseRoot)
	{
		GameObject rootObject = new("FastUI_SparseNegative_" + name, typeof(RectTransform));
		RectTransform rootRect = rootObject.GetComponent<RectTransform>();
		rootRect.SetParent(transform, false);
		setupRect(rootRect, new Vector2(420.0f, 320.0f), new Vector3(12000.0f, 10000.0f, 0.0f));
		FastCanvas canvas = rootObject.AddComponent<FastCanvas>();
		if (mImageMaterial != null)
		{
			canvas.setDefaultMaterial(mImageMaterial);
		}
		sparseRoot = createRect("SparseSOA", rootRect, new Vector2(380.0f, 280.0f), Vector3.zero);
		FastSOARenderGroup group = sparseRoot.gameObject.AddComponent<FastSOARenderGroup>();
		group.setStructureMode(FastSOAStructureMode.SparseStructure);
		return new SparseNegativeValidationCase { mName = name, mExpectedError = expectedError, mRoot = rootObject, mCanvas = canvas, mGroup = group };
	}
	protected SparseNegativeValidationCase createSparseCycleNegativeCase()
	{
		SparseNegativeValidationCase result = createSparseNegativeValidationCase("Cycle", "冲突环", out RectTransform root);
		RectTransform record0 = createRect("Record_0", root, new Vector2(100.0f, 100.0f), Vector3.zero);
		createFastRawImage("A", record0, new Vector2(8.0f, 8.0f), Vector3.zero, mTextureA, Color.white);
		createFastRawImage("B", record0, new Vector2(8.0f, 8.0f), Vector3.zero, mTextureA, Color.white);
		createFastRawImage("C", record0, new Vector2(8.0f, 8.0f), Vector3.zero, mTextureA, Color.white);
		RectTransform record1 = createRect("Record_1", root, new Vector2(100.0f, 100.0f), Vector3.zero);
		createFastRawImage("A", record1, new Vector2(8.0f, 8.0f), Vector3.zero, mTextureA, Color.white);
		createFastRawImage("C", record1, new Vector2(8.0f, 8.0f), Vector3.zero, mTextureA, Color.white);
		createFastRawImage("B", record1, new Vector2(8.0f, 8.0f), Vector3.zero, mTextureA, Color.white);
		return result;
	}
	protected SparseNegativeValidationCase createSparseTypeMismatchNegativeCase()
	{
		SparseNegativeValidationCase result = createSparseNegativeValidationCase("TypeMismatch", "类型不一致", out RectTransform root);
		RectTransform record0 = createRect("Record_0", root, new Vector2(100.0f, 100.0f), Vector3.zero);
		createFastRawImage("A", record0, new Vector2(8.0f, 8.0f), Vector3.zero, mTextureA, Color.white);
		RectTransform record1 = createRect("Record_1", root, new Vector2(100.0f, 100.0f), Vector3.zero);
		createFastText("A", record1, new Vector2(30.0f, 18.0f), Vector3.zero, "A", 12.0f, FastUITextHorizontalAlignment.Left);
		return result;
	}
	protected SparseNegativeValidationCase createSparseEmptyRecordNegativeCase()
	{
		SparseNegativeValidationCase result = createSparseNegativeValidationCase("EmptyRecord", "Record没有RenderElement", out RectTransform root);
		RectTransform record0 = createRect("Record_0", root, new Vector2(100.0f, 100.0f), Vector3.zero);
		createFastRawImage("A", record0, new Vector2(8.0f, 8.0f), Vector3.zero, mTextureA, Color.white);
		createRect("Record_1", root, new Vector2(100.0f, 100.0f), Vector3.zero);
		return result;
	}
	protected SparseNegativeValidationCase createSparseStencilBarrierNegativeCase()
	{
		SparseNegativeValidationCase result = createSparseNegativeValidationCase("StencilBarrier", "Stencil Writer/Pop", out RectTransform root);
		for (int i = 0; i < 2; ++i)
		{
			RectTransform record = createRect("Record_" + i, root, new Vector2(100.0f, 100.0f), Vector3.zero);
			RectTransform clip = createRect("Clip", record, new Vector2(80.0f, 80.0f), Vector3.zero);
			clip.gameObject.AddComponent<FastRectMask2D>();
			createFastRawImage("A", clip, new Vector2(8.0f, 8.0f), Vector3.zero, mTextureA, Color.white);
		}
		return result;
	}
	protected SparseNegativeValidationCase createSparseNestedNegativeCase()
	{
		SparseNegativeValidationCase result = createSparseNegativeValidationCase("Nested", "只支持单层SOA Group", out RectTransform root);
		RectTransform record0 = createRect("Record_0", root, new Vector2(100.0f, 100.0f), Vector3.zero);
		createFastRawImage("A", record0, new Vector2(8.0f, 8.0f), Vector3.zero, mTextureA, Color.white);
		RectTransform inner = createRect("InnerSOA", record0, new Vector2(80.0f, 80.0f), Vector3.zero);
		FastSOARenderGroup innerGroup = inner.gameObject.AddComponent<FastSOARenderGroup>();
		innerGroup.setStructureMode(FastSOAStructureMode.SparseStructure);
		RectTransform sub0 = createRect("Sub_0", inner, new Vector2(30.0f, 30.0f), Vector3.zero);
		createFastRawImage("B", sub0, new Vector2(8.0f, 8.0f), Vector3.zero, mTextureA, Color.white);
		RectTransform sub1 = createRect("Sub_1", inner, new Vector2(30.0f, 30.0f), Vector3.zero);
		createFastRawImage("B", sub1, new Vector2(8.0f, 8.0f), Vector3.zero, mTextureA, Color.white);
		RectTransform record1 = createRect("Record_1", root, new Vector2(100.0f, 100.0f), Vector3.zero);
		createFastRawImage("A", record1, new Vector2(8.0f, 8.0f), Vector3.zero, mTextureA, Color.white);
		return result;
	}
	protected bool tryCompleteSparseSOANegativeValidation()
	{
		if (mSparseNegativeValidationCases == null || mSparseNegativeValidationCases.Length == 0)
		{
			mSparseNegativeValidationComplete = true;
			mSparseNegativeValidationValid = false;
			benchmarkLogError("[FastUI Sparse Negative Validation] 初始化失败.");
			return true;
		}
		for (int i = 0; i < mSparseNegativeValidationCases.Length; ++i)
		{
			SparseNegativeValidationCase test = mSparseNegativeValidationCases[i];
			if (test == null || test.mCanvas == null || test.mGroup == null)
			{
				mSparseNegativeValidationComplete = true;
				mSparseNegativeValidationValid = false;
				benchmarkLogError("[FastUI Sparse Negative Validation] Case初始化失败,Index=" + i);
				return true;
			}
			if (test.mGroup.getLastBuildValid() && test.mCanvas.getSOAInvalidGroupCount() == 0)
			{
				return false;
			}
		}
		bool allValid = true;
		for (int i = 0; i < mSparseNegativeValidationCases.Length; ++i)
		{
			SparseNegativeValidationCase test = mSparseNegativeValidationCases[i];
			string error = test.mGroup.getLastBuildError();
			bool valid = !test.mGroup.getLastBuildValid() && test.mCanvas.getSOAConvertedGroupCount() == 0 &&
				test.mCanvas.getSOAInvalidGroupCount() > 0 && !test.mCanvas.isSOADrawOrderActive() &&
				!string.IsNullOrEmpty(error) && error.Contains(test.mExpectedError);
			allValid &= valid;
			benchmarkLog("[FastUI Sparse Negative Validation] Case=" + test.mName +
				" | Converted=" + test.mCanvas.getSOAConvertedGroupCount() +
				" | Invalid=" + test.mCanvas.getSOAInvalidGroupCount() +
				" | Reordered=" + test.mCanvas.isSOADrawOrderActive() +
				" | ErrorMatched=" + (!string.IsNullOrEmpty(error) && error.Contains(test.mExpectedError)) +
				" | FallbackNormal=" + !test.mCanvas.isSOADrawOrderActive() + " | Valid=" + valid);
		}
		benchmarkLog("[FastUI Sparse Negative Validation] Cases=" + mSparseNegativeValidationCases.Length + " | AllFallbackValid=" + allValid);
		mSparseNegativeValidationValid = allValid;
		mSparseNegativeValidationComplete = true;
		for (int i = 0; i < mSparseNegativeValidationCases.Length; ++i)
		{
			SparseNegativeValidationCase test = mSparseNegativeValidationCases[i];
			if (test?.mRoot != null)
			{
				test.mRoot.SetActive(false);
				Destroy(test.mRoot);
			}
		}
		return true;
	}
	protected void createSparseSOAValidation()
	{
		mSparseSOAValidationRoot = new GameObject("FastUI_SparseSOA_Validation", typeof(RectTransform));
		RectTransform rootRect = mSparseSOAValidationRoot.GetComponent<RectTransform>();
		rootRect.SetParent(transform, false);
		setupRect(rootRect, new Vector2(500.0f, 400.0f), new Vector3(11000.0f, 10000.0f, 0.0f));
		mSparseSOAValidationCanvas = mSparseSOAValidationRoot.AddComponent<FastCanvas>();
		if (mImageMaterial != null)
		{
			mSparseSOAValidationCanvas.setDefaultMaterial(mImageMaterial);
		}
		RectTransform sparseRoot = createRect("SparseSOA", rootRect, new Vector2(460.0f, 360.0f), Vector3.zero);
		mSparseSOAValidationGroup = sparseRoot.gameObject.AddComponent<FastSOARenderGroup>();
		mSparseSOAValidationGroup.setStructureMode(FastSOAStructureMode.SparseStructure);
		mSparseSOAValidationA = new FastRawImage[4];
		mSparseSOAValidationB = new FastRawImage[4];
		mSparseSOAValidationX = new FastRawImage[1];
		mSparseSOAValidationC = new FastRawImage[4];
		mSparseSOAValidationD = new FastRawImage[3];
		for (int recordIndex = 0; recordIndex < 4; ++recordIndex)
		{
			RectTransform record = createRect("Record_" + recordIndex, sparseRoot, new Vector2(100.0f, 100.0f), Vector3.zero);
			mSparseSOAValidationA[recordIndex] = createFastRawImage("A", record, new Vector2(8.0f, 8.0f), Vector3.zero, mTextureA, Color.white);
			mSparseSOAValidationB[recordIndex] = createFastRawImage("B", record, new Vector2(8.0f, 8.0f), Vector3.zero, mTextureA, Color.white);
			if (recordIndex == 3)
			{
				mSparseSOAValidationX[0] = createFastRawImage("X", record, new Vector2(8.0f, 8.0f), Vector3.zero, mTextureA, Color.white);
			}
			mSparseSOAValidationC[recordIndex] = createFastRawImage("C", record, new Vector2(8.0f, 8.0f), Vector3.zero, mTextureA, Color.white);
			if (recordIndex != 2)
			{
				int dIndex = recordIndex < 2 ? recordIndex : 2;
				mSparseSOAValidationD[dIndex] = createFastRawImage("D", record, new Vector2(8.0f, 8.0f), Vector3.zero, mTextureA, Color.white);
			}
		}
	}
	protected bool tryCompleteSparseSOAValidation()
	{
		if (mSparseSOAValidationCanvas == null || mSparseSOAValidationGroup == null)
		{
			mSparseSOAValidationComplete = true;
			mSparseSOAValidationValid = false;
			benchmarkLogError("[FastUI Sparse SOA Validation] 初始化失败.");
			return true;
		}
		int groupCount = mSparseSOAValidationCanvas.getSOAGroupCount();
		int converted = mSparseSOAValidationCanvas.getSOAConvertedGroupCount();
		int invalid = mSparseSOAValidationCanvas.getSOAInvalidGroupCount();
		if (groupCount < 1 || converted == 0 && invalid == 0)
		{
			return false;
		}
		FastUIMeshRenderer renderer = mSparseSOAValidationCanvas.getMeshRenderer();
		bool valid = groupCount == 1 && converted == 1 && invalid == 0 &&
			mSparseSOAValidationGroup.getStructureMode() == FastSOAStructureMode.SparseStructure &&
			mSparseSOAValidationGroup.getLastBuildValid() &&
			mSparseSOAValidationCanvas.getSOARootGroupCount() == 1 &&
			mSparseSOAValidationCanvas.getSOANestedGroupCount() == 0 &&
			mSparseSOAValidationCanvas.getSOARecordCount() == 4 &&
			mSparseSOAValidationCanvas.getSOALaneCount() == 5 &&
			mSparseSOAValidationCanvas.getSOAProjectedElementCount() == 16 &&
			mSparseSOAValidationCanvas.getSOABatchKeyGroupCount() == 5 &&
			mSparseSOAValidationCanvas.isSOADrawOrderActive() && renderer != null &&
			validateSparseSOALane(mSparseSOAValidationA, 0) &&
			validateSparseSOALane(mSparseSOAValidationB, 1) &&
			validateSparseSOALane(mSparseSOAValidationX, 2) &&
			validateSparseSOALane(mSparseSOAValidationC, 3) &&
			validateSparseSOALane(mSparseSOAValidationD, 4) &&
			validateSparseSOADrawOrder(renderer);
		benchmarkLog("[FastUI Sparse SOA Validation] Mode=" + mSparseSOAValidationGroup.getStructureMode() +
			" | Groups=" + groupCount +
			" | Converted=" + converted + " | Invalid=" + invalid +
			" | Records=" + mSparseSOAValidationCanvas.getSOARecordCount() +
			" | Lanes=" + mSparseSOAValidationCanvas.getSOALaneCount() +
			" | Elements=" + mSparseSOAValidationCanvas.getSOAProjectedElementCount() +
			" | BatchKeyGroups=" + mSparseSOAValidationCanvas.getSOABatchKeyGroupCount() +
			" | Order=A,B,X,C,D | Reordered=" + mSparseSOAValidationCanvas.isSOADrawOrderActive() +
			" | Valid=" + valid);
		if (!valid)
		{
			benchmarkLogError("[FastUI Sparse SOA Validation] Sparse SOA校验失败,Error=" + mSparseSOAValidationGroup.getLastBuildError());
		}
		mSparseSOAValidationValid = valid;
		mSparseSOAValidationComplete = true;
		mSparseSOAValidationRoot.SetActive(false);
		Destroy(mSparseSOAValidationRoot);
		mSparseSOAValidationRoot = null;
		return true;
	}
	protected bool validateSparseSOALane(FastRawImage[] elements, int expectedLane)
	{
		if (elements == null)
		{
			return false;
		}
		for (int i = 0; i < elements.Length; ++i)
		{
			FastUIRenderElement element = elements[i];
			if (element == null || mSparseSOAValidationCanvas.getSOALaneIndexForRenderIndex(element.getRenderOrderIndex()) != expectedLane)
			{
				return false;
			}
		}
		return true;
	}
	protected bool validateSparseSOADrawOrder(FastUIMeshRenderer renderer)
	{
		int drawIndex = 0;
		if (!validateSparseSOADrawRange(renderer, mSparseSOAValidationA, ref drawIndex) ||
			!validateSparseSOADrawRange(renderer, mSparseSOAValidationB, ref drawIndex) ||
			!validateSparseSOADrawRange(renderer, mSparseSOAValidationX, ref drawIndex) ||
			!validateSparseSOADrawRange(renderer, mSparseSOAValidationC, ref drawIndex) ||
			!validateSparseSOADrawRange(renderer, mSparseSOAValidationD, ref drawIndex))
		{
			return false;
		}
		return drawIndex == 16;
	}
	protected bool validateSparseSOADrawRange(FastUIMeshRenderer renderer, FastRawImage[] elements, ref int drawIndex)
	{
		for (int i = 0; i < elements.Length; ++i)
		{
			if (renderer.getRenderIndexForDrawIndex(drawIndex++) != elements[i].getRenderOrderIndex())
			{
				return false;
			}
		}
		return true;
	}
	protected void createNestedSOAValidation()
	{
		mNestedSOAValidationRoot = new GameObject("FastUI_NestedSOA_Validation", typeof(RectTransform));
		RectTransform rootRect = mNestedSOAValidationRoot.GetComponent<RectTransform>();
		rootRect.SetParent(transform, false);
		setupRect(rootRect, new Vector2(400.0f, 400.0f), new Vector3(10000.0f, 10000.0f, 0.0f));
		mNestedSOAValidationCanvas = mNestedSOAValidationRoot.AddComponent<FastCanvas>();
		if (mImageMaterial != null)
		{
			mNestedSOAValidationCanvas.setDefaultMaterial(mImageMaterial);
		}
		RectTransform outer = createRect("OuterSOA", rootRect, new Vector2(360.0f, 360.0f), Vector3.zero);
		mNestedSOAValidationOuterGroup = outer.gameObject.AddComponent<FastSOARenderGroup>();
		mNestedSOAValidationInnerGroups = new FastSOARenderGroup[3];
		mNestedSOAValidationA = new FastRawImage[3];
		mNestedSOAValidationB = new FastRawImage[6];
		mNestedSOAValidationC = new FastRawImage[6];
		for (int recordIndex = 0; recordIndex < 3; ++recordIndex)
		{
			RectTransform record = createRect("Record_" + recordIndex, outer, new Vector2(100.0f, 100.0f), Vector3.zero);
			mNestedSOAValidationA[recordIndex] = createFastRawImage("A", record, new Vector2(10.0f, 10.0f), Vector3.zero, mTextureA, Color.white);
			RectTransform inner = createRect("InnerSOA", record, new Vector2(80.0f, 80.0f), Vector3.zero);
			mNestedSOAValidationInnerGroups[recordIndex] = inner.gameObject.AddComponent<FastSOARenderGroup>();
			for (int subIndex = 0; subIndex < 2; ++subIndex)
			{
				RectTransform subRecord = createRect("Sub_" + subIndex, inner, new Vector2(30.0f, 30.0f), Vector3.zero);
				int flatIndex = recordIndex * 2 + subIndex;
				mNestedSOAValidationB[flatIndex] = createFastRawImage("B", subRecord, new Vector2(8.0f, 8.0f), Vector3.zero, mTextureA, Color.white);
				mNestedSOAValidationC[flatIndex] = createFastRawImage("C", subRecord, new Vector2(8.0f, 8.0f), Vector3.zero, mTextureA, Color.white);
			}
		}
	}
	protected bool tryCompleteNestedSOAValidation()
	{
		if (mNestedSOAValidationCanvas == null || mNestedSOAValidationOuterGroup == null)
		{
			mNestedSOAValidationComplete = true;
			mNestedSOAValidationValid = false;
			benchmarkLogError("[FastUI Nested SOA Validation] 初始化失败.");
			return true;
		}
		int groupCount = mNestedSOAValidationCanvas.getSOAGroupCount();
		int converted = mNestedSOAValidationCanvas.getSOAConvertedGroupCount();
		int invalid = mNestedSOAValidationCanvas.getSOAInvalidGroupCount();
		if (groupCount < 4 || converted == 0 && invalid == 0)
		{
			return false;
		}
		FastUIMeshRenderer renderer = mNestedSOAValidationCanvas.getMeshRenderer();
		bool valid = groupCount == 4 && converted == 4 && invalid == 0 &&
			mNestedSOAValidationCanvas.getSOARootGroupCount() == 1 &&
			mNestedSOAValidationCanvas.getSOANestedGroupCount() == 3 &&
			mNestedSOAValidationCanvas.getSOAMaxNestedDepth() == 1 &&
			mNestedSOAValidationCanvas.getSOARecordCount() == 9 &&
			mNestedSOAValidationCanvas.getSOALaneCount() == 3 &&
			mNestedSOAValidationCanvas.getSOAProjectedElementCount() == 15 &&
			mNestedSOAValidationCanvas.getSOABatchKeyGroupCount() == 3 &&
			mNestedSOAValidationCanvas.isSOADrawOrderActive() && renderer != null &&
			validateNestedSOALane(mNestedSOAValidationA, 0) &&
			validateNestedSOALane(mNestedSOAValidationB, 1) &&
			validateNestedSOALane(mNestedSOAValidationC, 2) &&
			validateNestedSOADrawOrder(renderer);
		for (int i = 0; i < mNestedSOAValidationInnerGroups.Length; ++i)
		{
			valid &= mNestedSOAValidationInnerGroups[i] != null && mNestedSOAValidationInnerGroups[i].getLastBuildValid();
		}
		valid &= mNestedSOAValidationOuterGroup.getLastBuildValid();
		benchmarkLog("[FastUI Nested SOA Validation] Groups=" + groupCount +
			" | Root=" + mNestedSOAValidationCanvas.getSOARootGroupCount() +
			" | Nested=" + mNestedSOAValidationCanvas.getSOANestedGroupCount() +
			" | MaxDepth=" + mNestedSOAValidationCanvas.getSOAMaxNestedDepth() +
			" | Converted=" + converted + " | Invalid=" + invalid +
			" | Records=" + mNestedSOAValidationCanvas.getSOARecordCount() +
			" | Lanes=" + mNestedSOAValidationCanvas.getSOALaneCount() +
			" | Elements=" + mNestedSOAValidationCanvas.getSOAProjectedElementCount() +
			" | BatchKeyGroups=" + mNestedSOAValidationCanvas.getSOABatchKeyGroupCount() +
			" | Reordered=" + mNestedSOAValidationCanvas.isSOADrawOrderActive() +
			" | Valid=" + valid);
		if (!valid)
		{
			benchmarkLogError("[FastUI Nested SOA Validation] Nested SOA完全展开校验失败,OuterError=" +
				mNestedSOAValidationOuterGroup.getLastBuildError());
		}
		mNestedSOAValidationValid = valid;
		mNestedSOAValidationComplete = true;
		mNestedSOAValidationRoot.SetActive(false);
		Destroy(mNestedSOAValidationRoot);
		mNestedSOAValidationRoot = null;
		return true;
	}
	protected bool validateNestedSOALane(FastRawImage[] elements, int expectedLane)
	{
		if (elements == null)
		{
			return false;
		}
		for (int i = 0; i < elements.Length; ++i)
		{
			FastUIRenderElement element = elements[i];
			if (element == null || mNestedSOAValidationCanvas.getSOALaneIndexForRenderIndex(element.getRenderOrderIndex()) != expectedLane)
			{
				return false;
			}
		}
		return true;
	}
	protected bool validateNestedSOADrawOrder(FastUIMeshRenderer renderer)
	{
		int drawIndex = 0;
		for (int i = 0; i < mNestedSOAValidationA.Length; ++i)
		{
			if (renderer.getRenderIndexForDrawIndex(drawIndex++) != mNestedSOAValidationA[i].getRenderOrderIndex())
			{
				return false;
			}
		}
		for (int i = 0; i < mNestedSOAValidationB.Length; ++i)
		{
			if (renderer.getRenderIndexForDrawIndex(drawIndex++) != mNestedSOAValidationB[i].getRenderOrderIndex())
			{
				return false;
			}
		}
		for (int i = 0; i < mNestedSOAValidationC.Length; ++i)
		{
			if (renderer.getRenderIndexForDrawIndex(drawIndex++) != mNestedSOAValidationC[i].getRenderOrderIndex())
			{
				return false;
			}
		}
		return drawIndex == 15;
	}
	protected bool configureBenchmarkCameraForVisualParity()
	{
		mBenchmarkCamera = Camera.main;
		if (mBenchmarkCamera == null || !mBenchmarkCamera.enabled)
		{
			benchmarkLogError("[Visual Camera] MainCamera=null或未启用 | Valid=False");
			return false;
		}
		mBenchmarkCamera.orthographic = true;
		int pixelWidth = Mathf.Max(Screen.width, 1);
		int pixelHeight = Mathf.Max(Screen.height, 1);
		mBenchmarkCamera.orthographicSize = pixelHeight * 0.5f;
		float pixelsPerWorldUnit = pixelHeight / Mathf.Max(mBenchmarkCamera.orthographicSize * 2.0f, 0.0001f);
		float halfScreenWidth = pixelWidth * 0.5f;
		float availableWindowWidth = Mathf.Max(halfScreenWidth - BENCHMARK_COMPARISON_MARGIN_X * 2.0f, 1.0f);
		float availableWindowHeight = Mathf.Max(pixelHeight - BENCHMARK_COMPARISON_MARGIN_Y * 2.0f, 1.0f);
		float widthScale = availableWindowWidth / Mathf.Max(mWindowSize.x, 1.0f);
		float heightScale = availableWindowHeight / Mathf.Max(mWindowSize.y, 1.0f);
		mComparisonDisplayScale = Mathf.Clamp(Mathf.Min(widthScale, heightScale), 0.01f, 1.0f);
		mComparisonWindowOffsetX = pixelWidth * 0.25f;
		bool valid = Mathf.Abs(pixelsPerWorldUnit - 1.0f) <= 0.001f && mComparisonDisplayScale > 0.0f;
		benchmarkLog("[Visual Camera] Screen=" + Screen.width + "x" + Screen.height +
			" | OrthographicSize=" + mBenchmarkCamera.orthographicSize.ToString("F2") +
			" | PixelsPerWorldUnit=" + pixelsPerWorldUnit.ToString("F4") +
			" | Target=1.0000 | Valid=" + valid);
		benchmarkLog("[Comparison Layout] Scale=" + mComparisonDisplayScale.ToString("F4") +
			" | WindowOffsetX=" + mComparisonWindowOffsetX.ToString("F2") +
			" | LogicalWindow=" + mWindowSize.x.ToString("F0") + "x" + mWindowSize.y.ToString("F0") +
			" | DisplayWindow=" + (mWindowSize.x * mComparisonDisplayScale).ToString("F1") + "x" +
			(mWindowSize.y * mComparisonDisplayScale).ToString("F1") +
			" | Margin=" + BENCHMARK_COMPARISON_MARGIN_X.ToString("F0") + "/" + BENCHMARK_COMPARISON_MARGIN_Y.ToString("F0") +
			" | Valid=" + valid);
		return valid;
	}
	protected bool validateBenchmarkVisualLayout()
	{
		if (mBenchmarkCamera == null || mFastRoot == null || mFastCanvas == null || mFastWindowRect == null || mUGUIWindowRect == null ||
			mFastViewportRect == null || mUGUIViewportRect == null || mFastItems == null || mUGUIItems == null || mFastItems.Length == 0 || mUGUIItems.Length == 0)
		{
			benchmarkLogError("[Visual Layout Validation] RequiredReferenceMissing=True | Valid=False");
			return false;
		}
		Rect fastWindow = projectFastRectToScreen(mFastWindowRect);
		Rect uguiWindow = projectUGUIRectToScreen(mUGUIWindowRect);
		Rect fastViewport = projectFastRectToScreen(mFastViewportRect);
		Rect uguiViewport = projectUGUIRectToScreen(mUGUIViewportRect);
		Rect fastItem = projectFastRectToScreen(mFastItems[0].mRoot);
		Rect uguiItem = projectUGUIRectToScreen(mUGUIItems[0].mRoot);
		FastUIMeshRenderer fastMeshRenderer = mFastCanvas.getMeshRenderer();
		Renderer unityRenderer = fastMeshRenderer != null ? fastMeshRenderer.getRenderer() : null;
		bool fastRootAtOrigin = mFastRoot.transform.localPosition.sqrMagnitude <= 0.0001f &&
			Quaternion.Angle(mFastRoot.transform.localRotation, Quaternion.identity) <= 0.001f &&
			(mFastRoot.transform.localScale - Vector3.one).sqrMagnitude <= 0.0001f;
		bool rendererAtCanvasOrigin = unityRenderer != null && (unityRenderer.transform.position - mFastRoot.transform.position).sqrMagnitude <= 0.0001f;
		bool windowLocalValid = Mathf.Abs(mFastWindowRect.localPosition.x + mComparisonWindowOffsetX) <= 0.01f &&
			Mathf.Abs(mUGUIWindowRect.localPosition.x - mComparisonWindowOffsetX) <= 0.01f;
		bool windowScaleValid = Mathf.Abs(mFastWindowRect.localScale.x - mComparisonDisplayScale) <= 0.0001f &&
			Mathf.Abs(mFastWindowRect.localScale.y - mComparisonDisplayScale) <= 0.0001f &&
			Mathf.Abs(mUGUIWindowRect.localScale.x - mComparisonDisplayScale) <= 0.0001f &&
			Mathf.Abs(mUGUIWindowRect.localScale.y - mComparisonDisplayScale) <= 0.0001f;
		Vector2 displayWindowSize = mWindowSize * mComparisonDisplayScale;
		Vector2 displayViewportSize = mViewportSize * mComparisonDisplayScale;
		Vector2 displayItemSize = mItemSize * mComparisonDisplayScale;
		bool windowSizeValid = rectSizeNear(fastWindow, uguiWindow, 1.0f) && rectSizeNear(fastWindow, displayWindowSize, 1.0f);
		bool viewportSizeValid = rectSizeNear(fastViewport, uguiViewport, 1.0f) && rectSizeNear(fastViewport, displayViewportSize, 1.0f);
		bool itemSizeValid = rectSizeNear(fastItem, uguiItem, 1.0f) && rectSizeNear(fastItem, displayItemSize, 1.0f);
		float expectedHorizontalDelta = mComparisonWindowOffsetX * 2.0f;
		bool windowOffsetValid = Mathf.Abs((uguiWindow.center.x - fastWindow.center.x) - expectedHorizontalDelta) <= 1.0f &&
			Mathf.Abs(fastWindow.center.y - uguiWindow.center.y) <= 1.0f;
		bool viewportOffsetValid = Mathf.Abs((uguiViewport.center.x - fastViewport.center.x) - expectedHorizontalDelta) <= 1.0f &&
			Mathf.Abs(fastViewport.center.y - uguiViewport.center.y) <= 1.0f;
		bool itemOffsetValid = Mathf.Abs((uguiItem.center.x - fastItem.center.x) - expectedHorizontalDelta) <= 1.0f &&
			Mathf.Abs(fastItem.center.y - uguiItem.center.y) <= 1.0f;
		bool valid = fastRootAtOrigin && windowLocalValid && windowScaleValid && windowSizeValid && viewportSizeValid && itemSizeValid &&
			windowOffsetValid && viewportOffsetValid && itemOffsetValid;
		benchmarkLog("[Visual Layout Validation] FastRootAtOrigin=" + fastRootAtOrigin +
			" | RendererAtCanvasOrigin=" + rendererAtCanvasOrigin + " | WindowLocalValid=" + windowLocalValid +
			" | WindowScaleValid=" + windowScaleValid + " | WindowSizeValid=" + windowSizeValid + " | ViewportSizeValid=" + viewportSizeValid + " | ItemSizeValid=" + itemSizeValid +
			" | WindowOffsetValid=" + windowOffsetValid + " | ViewportOffsetValid=" + viewportOffsetValid + " | ItemOffsetValid=" + itemOffsetValid +
			" | Valid=" + valid);
		benchmarkLog("[Visual Screen Bounds] FastWindow=" + formatScreenRect(fastWindow) + " | UGUIWindow=" + formatScreenRect(uguiWindow) +
			" | FastViewport=" + formatScreenRect(fastViewport) + " | UGUIViewport=" + formatScreenRect(uguiViewport) +
			" | FastItem0=" + formatScreenRect(fastItem) + " | UGUIItem0=" + formatScreenRect(uguiItem));
		return valid;
	}
	protected bool validateFastVertexCoordinateSpace()
	{
		FastUIMeshRenderer renderer = mFastCanvas != null ? mFastCanvas.getMeshRenderer() : null;
		if (renderer == null || mFastWindowBackground == null || mFastItems == null || mFastItems.Length == 0 || mFastItems[0] == null || mFastItems[0].mIcon == null)
		{
			benchmarkLogError("[Vertex Validation] RequiredReferenceMissing=True | Valid=False");
			return false;
		}
		Vector3 streamMin;
		Vector3 streamMax;
		int nonFiniteCount;
		int nonZeroZCount;
		int extremeCount;
		bool streamBoundsValid = renderer.tryGetPositionStreamBounds(out streamMin, out streamMax, out nonFiniteCount, out nonZeroZCount, out extremeCount);
		Vector3 origin = renderer.getRenderOriginOffset();
		Renderer unityRenderer = renderer.getRenderer();
		Matrix4x4 rendererLocalToWorld = unityRenderer != null ? unityRenderer.transform.localToWorldMatrix : Matrix4x4.identity;
		benchmarkLog("[Vertex Stream Bounds] Valid=" + streamBoundsValid +
			" | Min=" + formatVector3(streamMin) + " | Max=" + formatVector3(streamMax) +
			" | Size=" + formatVector3(streamMax - streamMin) + " | NonFinite=" + nonFiniteCount +
			" | NonZeroZ=" + nonZeroZCount + " | Extreme=" + extremeCount +
			" | RenderOrigin=" + formatVector3(origin));
		benchmarkLog("[Vertex Renderer Transform] Position=" + formatVector3(unityRenderer != null ? unityRenderer.transform.position : Vector3.zero) +
			" | LocalPosition=" + formatVector3(unityRenderer != null ? unityRenderer.transform.localPosition : Vector3.zero) +
			" | LossyScale=" + formatVector3(unityRenderer != null ? unityRenderer.transform.lossyScale : Vector3.one) +
			" | M00=" + rendererLocalToWorld.m00.ToString("F4") + " | M01=" + rendererLocalToWorld.m01.ToString("F4") +
			" | M03=" + rendererLocalToWorld.m03.ToString("F4") + " | M10=" + rendererLocalToWorld.m10.ToString("F4") +
			" | M11=" + rendererLocalToWorld.m11.ToString("F4") + " | M13=" + rendererLocalToWorld.m13.ToString("F4") +
			" | M20=" + rendererLocalToWorld.m20.ToString("F4") + " | M21=" + rendererLocalToWorld.m21.ToString("F4") +
			" | M22=" + rendererLocalToWorld.m22.ToString("F4") + " | M23=" + rendererLocalToWorld.m23.ToString("F4"));
		bool windowValid = validateFastGeometryBounds("WindowBackground", mFastWindowBackground, renderer, 1.0f);
		bool itemBackgroundValid = validateFastGeometryBounds("Item0/Background", mFastItems[0].mBackground, renderer, 1.0f);
		bool iconValid = validateFastSimpleQuadPositions("Item0/Icon", mFastItems[0].mIcon, renderer, origin, 0.05f);
		bool qualityValid = validateFastSimpleQuadPositions("Item0/QualityFrame", mFastItems[0].mQualityFrame, renderer, origin, 0.05f);
		bool globalValid = streamBoundsValid && nonFiniteCount == 0 && nonZeroZCount == 0 && extremeCount == 0;
		bool valid = globalValid && windowValid && itemBackgroundValid && iconValid && qualityValid;
		benchmarkLog("[Vertex Validation] Global=" + globalValid + " | Window=" + windowValid +
			" | ItemBackground=" + itemBackgroundValid + " | Icon=" + iconValid + " | Quality=" + qualityValid + " | Valid=" + valid);
		return valid;
	}
	protected bool validateFastGeometryBounds(string label, FastImage image, FastUIMeshRenderer renderer, float tolerance)
	{
		if (image == null)
		{
			benchmarkLogError("[Vertex Bounds] " + label + " | Image=null | Valid=False");
			return false;
		}
		RectTransform rect = image.transform as RectTransform;
		int slot = image.getVertexSlot();
		int vertexStart = renderer.getGeometryVertexStart(slot);
		int vertexCount = renderer.getGeometryVertexCount(slot);
		FastUISpatialBoundsData actualBounds;
		bool actualValid = renderer.tryGetGeometryBounds(slot, out actualBounds);
		Vector3 expectedMin;
		Vector3 expectedMax;
		getRectCanvasLocalBounds(rect, out expectedMin, out expectedMax);
		bool boundsNear = actualValid &&
			Mathf.Abs(actualBounds.mMinX - expectedMin.x) <= tolerance && Mathf.Abs(actualBounds.mMaxX - expectedMax.x) <= tolerance &&
			Mathf.Abs(actualBounds.mMinY - expectedMin.y) <= tolerance && Mathf.Abs(actualBounds.mMaxY - expectedMax.y) <= tolerance &&
			Mathf.Abs(actualBounds.mCenterZ - (expectedMin.z + expectedMax.z) * 0.5f) <= tolerance;
		benchmarkLog("[Vertex Bounds] " + label + " | Slot=" + slot + " | Start=" + vertexStart + " | Count=" + vertexCount +
			" | ExpectedMin=" + formatVector3(expectedMin) + " | ExpectedMax=" + formatVector3(expectedMax) +
			" | ActualValid=" + actualValid +
			" | ActualMin=(" + actualBounds.mMinX.ToString("F3") + "," + actualBounds.mMinY.ToString("F3") + ")" +
			" | ActualMax=(" + actualBounds.mMaxX.ToString("F3") + "," + actualBounds.mMaxY.ToString("F3") + ")" +
			" | ActualZ=" + actualBounds.mCenterZ.ToString("F3") + " | Valid=" + boundsNear);
		return boundsNear;
	}
	protected bool validateFastSimpleQuadPositions(string label, FastImage image, FastUIMeshRenderer renderer, Vector3 renderOrigin, float tolerance)
	{
		if (image == null)
		{
			benchmarkLogError("[Vertex Quad] " + label + " | Image=null | Valid=False");
			return false;
		}
		RectTransform rect = image.transform as RectTransform;
		int slot = image.getVertexSlot();
		int vertexStart = renderer.getGeometryVertexStart(slot);
		int vertexCount = renderer.getGeometryVertexCount(slot);
		if (rect == null || slot < 0 || vertexStart < 0 || vertexCount != 4)
		{
			benchmarkLogError("[Vertex Quad] " + label + " | Slot=" + slot + " | Start=" + vertexStart + " | Count=" + vertexCount + " | ExpectedCount=4 | Valid=False");
			return false;
		}
		Vector3[] expectedCanvas = new Vector3[4];
		rect.GetWorldCorners(expectedCanvas);
		for (int i = 0; i < 4; ++i)
		{
			expectedCanvas[i] = mFastRoot.transform.InverseTransformPoint(expectedCanvas[i]);
		}
		Vector3[] actualRaw = new Vector3[4];
		bool readValid = true;
		float maxRenderLocalDelta = 0.0f;
		float maxCanvasLocalDelta = 0.0f;
		for (int i = 0; i < 4; ++i)
		{
			if (!renderer.tryGetRawPosition(vertexStart + i, out actualRaw[i]))
			{
				readValid = false;
				continue;
			}
			Vector3 expectedRenderLocal = expectedCanvas[i] - renderOrigin;
			maxRenderLocalDelta = Mathf.Max(maxRenderLocalDelta, Vector3.Distance(actualRaw[i], expectedRenderLocal));
			maxCanvasLocalDelta = Mathf.Max(maxCanvasLocalDelta, Vector3.Distance(actualRaw[i], expectedCanvas[i]));
		}
		bool renderLocalValid = readValid && maxRenderLocalDelta <= tolerance;
		benchmarkLog("[Vertex Quad] " + label + " | Slot=" + slot + " | Start=" + vertexStart + " | Count=" + vertexCount +
			" | Origin=" + formatVector3(renderOrigin) + " | MaxDeltaRenderLocal=" + maxRenderLocalDelta.ToString("F4") +
			" | MaxDeltaCanvasLocal=" + maxCanvasLocalDelta.ToString("F4") + " | ReadValid=" + readValid + " | Valid=" + renderLocalValid);
		for (int i = 0; i < 4; ++i)
		{
			benchmarkLog("[Vertex Quad Point] " + label + " | I=" + i + " | Canvas=" + formatVector3(expectedCanvas[i]) +
				" | RenderExpected=" + formatVector3(expectedCanvas[i] - renderOrigin) + " | Raw=" + formatVector3(actualRaw[i]));
		}
		return renderLocalValid;
	}
	protected void getRectCanvasLocalBounds(RectTransform rect, out Vector3 min, out Vector3 max)
	{
		Vector3[] corners = new Vector3[4];
		rect.GetWorldCorners(corners);
		for (int i = 0; i < 4; ++i)
		{
			corners[i] = mFastRoot.transform.InverseTransformPoint(corners[i]);
		}
		min = corners[0];
		max = corners[0];
		for (int i = 1; i < 4; ++i)
		{
			min = Vector3.Min(min, corners[i]);
			max = Vector3.Max(max, corners[i]);
		}
	}
	protected string formatVector3(Vector3 value)
	{
		return "(" + value.x.ToString("F3") + "," + value.y.ToString("F3") + "," + value.z.ToString("F3") + ")";
	}
	protected Rect projectFastRectToScreen(RectTransform rect)
	{
		Vector3[] corners = new Vector3[4];
		rect.GetWorldCorners(corners);
		return projectCornersToScreen(corners, true);
	}
	protected Rect projectUGUIRectToScreen(RectTransform rect)
	{
		Vector3[] corners = new Vector3[4];
		rect.GetWorldCorners(corners);
		return projectCornersToScreen(corners, false);
	}
	protected Rect projectCornersToScreen(Vector3[] corners, bool useBenchmarkCamera)
	{
		float minX = float.MaxValue;
		float minY = float.MaxValue;
		float maxX = float.MinValue;
		float maxY = float.MinValue;
		for (int i = 0; i < corners.Length; ++i)
		{
			Vector2 screenPoint;
			if (useBenchmarkCamera)
			{
				Vector3 projected = mBenchmarkCamera.WorldToScreenPoint(corners[i]);
				screenPoint = new Vector2(projected.x, projected.y);
			}
			else
			{
				screenPoint = RectTransformUtility.WorldToScreenPoint(null, corners[i]);
			}
			minX = Mathf.Min(minX, screenPoint.x);
			minY = Mathf.Min(minY, screenPoint.y);
			maxX = Mathf.Max(maxX, screenPoint.x);
			maxY = Mathf.Max(maxY, screenPoint.y);
		}
		return Rect.MinMaxRect(minX, minY, maxX, maxY);
	}
	protected bool rectSizeNear(Rect a, Rect b, float tolerance)
	{
		return Mathf.Abs(a.width - b.width) <= tolerance && Mathf.Abs(a.height - b.height) <= tolerance;
	}
	protected bool rectSizeNear(Rect rect, Vector2 expectedSize, float tolerance)
	{
		return Mathf.Abs(rect.width - expectedSize.x) <= tolerance && Mathf.Abs(rect.height - expectedSize.y) <= tolerance;
	}
	protected string formatScreenRect(Rect rect)
	{
		return "C(" + rect.center.x.ToString("F1") + "," + rect.center.y.ToString("F1") + ")/S(" + rect.width.ToString("F1") + "," + rect.height.ToString("F1") + ")";
	}
	protected void createFastInventoryOneFrame()
	{
		reportInitializationProgress("FastGUI.OneFrameCreate", 0, mItemCount, true);
		long createStageStart = Stopwatch.GetTimestamp();
		mFastRoot = new GameObject("FastUI_Inventory", typeof(RectTransform));
		RectTransform rootRect = mFastRoot.GetComponent<RectTransform>();
		rootRect.SetParent(transform, false);
		// FastCanvas/MeshRenderer必须保持在世界原点。窗口偏移放到Canvas下面的Window节点，避免顶点世界坐标与Renderer Transform重复应用位移。
		setupRect(rootRect, mWindowSize, Vector3.zero);
		mFastCanvas = mFastRoot.AddComponent<FastCanvas>();
		// Publication benchmark uses the production compact index path.
		mFastCanvas.setRenderOriginShiftEnabled(true);
		if (mImageMaterial != null)
		{
			mFastCanvas.setDefaultMaterial(mImageMaterial);
		}
		mFastCanvas.setSortingLayer("Default");
		mFastCanvas.setSortingOrder(0);
		mFastWindowRect = createRect("Window", rootRect, mWindowSize, new Vector3(-mComparisonWindowOffsetX, 0.0f, 0.0f));
		mFastWindowRect.localScale = new Vector3(mComparisonDisplayScale, mComparisonDisplayScale, 1.0f);
		mFastWindowBackground = createFastImage("WindowBackground", mFastWindowRect, mWindowSize, Vector3.zero, mInventoryAtlas.mDetailBackground, new Color(0.10f, 0.11f, 0.14f, 1.0f), FastUIImageType.Sliced);
		createFastHeader(mFastWindowRect);
		createFastTabs(mFastWindowRect);
		RectTransform viewport = createRect("Viewport", mFastWindowRect, mViewportSize, new Vector3(-150.0f, -40.0f, 0.0f));
		mFastViewportRect = viewport;
		mFastViewportClip = viewport.gameObject.AddComponent<FastRectMask2D>();
		float contentHeight = getContentHeight();
		mFastContent = createRect("Content", viewport, new Vector2(mViewportSize.x, contentHeight), Vector3.zero);
		if (mFastEnableSOA)
		{
			mFastSOAGroup = mFastContent.gameObject.AddComponent<FastSOARenderGroup>();
		}
		mFastCreateScaffoldMS = (Stopwatch.GetTimestamp() - createStageStart) * TICK_TO_MS;
		createStageStart = Stopwatch.GetTimestamp();
		for (int i = 0; i < mItemCount; ++i)
		{
			mFastItems[i] = createFastItem(mFastContent, i);
		}
		mFastCreateItemsMS = (Stopwatch.GetTimestamp() - createStageStart) * TICK_TO_MS;
		createStageStart = Stopwatch.GetTimestamp();
		createFastDetailPanel(mFastWindowRect);
		mFastCreateDetailMS = (Stopwatch.GetTimestamp() - createStageStart) * TICK_TO_MS;
		reportInitializationProgress("FastGUI.OneFrameCreated", mItemCount, mItemCount, true);
	}
	protected void createUGUIInventoryOneFrame()
	{
		reportInitializationProgress("UGUI.OneFrameCreate", 0, mItemCount, true);
		long createStageStart = Stopwatch.GetTimestamp();
		mUGUIRoot = new GameObject("UGUI_Inventory", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
		RectTransform rootRect = mUGUIRoot.GetComponent<RectTransform>();
		rootRect.SetParent(transform, false);
		mUGUICanvas = mUGUIRoot.GetComponent<Canvas>();
		mUGUICanvas.renderMode = RenderMode.ScreenSpaceOverlay;
		mUGUICanvas.sortingOrder = 100;
		CanvasScaler scaler = mUGUIRoot.GetComponent<CanvasScaler>();
		scaler.uiScaleMode = CanvasScaler.ScaleMode.ConstantPixelSize;
		RectTransform window = createRect("Window", rootRect, mWindowSize, new Vector3(mComparisonWindowOffsetX, 0.0f, 0.0f));
		window.localScale = new Vector3(mComparisonDisplayScale, mComparisonDisplayScale, 1.0f);
		mUGUIWindowRect = window;
		createUGUIImage("WindowBackground", window, mWindowSize, Vector3.zero, mInventoryAtlas.mDetailBackground, new Color(0.10f, 0.11f, 0.14f, 1.0f), Image.Type.Sliced);
		createUGUIHeader(window);
		createUGUITabs(window);
		RectTransform viewport = createRect("Viewport", window, mViewportSize, new Vector3(-150.0f, -40.0f, 0.0f));
		mUGUIViewportRect = viewport;
		mUGUIViewportMask = viewport.gameObject.AddComponent<RectMask2D>();
		float contentHeight = getContentHeight();
		mUGUIContent = createRect("Content", viewport, new Vector2(mViewportSize.x, contentHeight), Vector3.zero);
		mUGUICreateScaffoldMS = (Stopwatch.GetTimestamp() - createStageStart) * TICK_TO_MS;
		createStageStart = Stopwatch.GetTimestamp();
		for (int i = 0; i < mItemCount; ++i)
		{
			mUGUIItems[i] = createUGUIItem(mUGUIContent, i);
		}
		mUGUICreateItemsMS = (Stopwatch.GetTimestamp() - createStageStart) * TICK_TO_MS;
		createStageStart = Stopwatch.GetTimestamp();
		createUGUIDetailPanel(window);
		mUGUICreateDetailMS = (Stopwatch.GetTimestamp() - createStageStart) * TICK_TO_MS;
		reportInitializationProgress("UGUI.OneFrameCreated", mItemCount, mItemCount, true);
	}
	protected FastItemView createFastItem(RectTransform parent, int index)
	{
		Vector3 itemPosition = getItemPosition(index);
		RectTransform root = createRect("Item_" + index, parent, mItemSize, itemPosition);
		FastUIVisibility visibility = root.gameObject.AddComponent<FastUIVisibility>();
		FastImage background = createFastImage("Background", root, mItemSize, Vector3.zero, mInventoryAtlas.mSlotBackground, new Color(0.86f, 0.90f, 1.0f, 1.0f), FastUIImageType.Sliced);
		RectTransform content = createRect("ContentLayer", root, mItemSize, Vector3.zero);
		FastImage icon = createFastImage("Icon", content, new Vector2(54.0f, 54.0f), new Vector3(0.0f, 4.0f, 0.0f), getInventoryIconSprite(index), Color.white);
		FastImage quality = createFastImage("QualityFrame", content, new Vector2(62.0f, 62.0f), new Vector3(0.0f, 4.0f, 0.0f), mInventoryAtlas.mQualityFrame, getQualityColor(index));
		RectTransform state = createRect("StateLayer", content, mItemSize, Vector3.zero);
		FastImage bind = createFastImage("Bind", state, new Vector2(15.0f, 15.0f), new Vector3(-24.0f, 24.0f, 0.0f), mInventoryAtlas.mBindIcon, Color.white);
		FastImage lockIcon = createFastImage("Lock", state, new Vector2(15.0f, 15.0f), new Vector3(24.0f, 24.0f, 0.0f), mInventoryAtlas.mLockIcon, Color.white);
		RectTransform textLayer = createRect("TextLayer", content, mItemSize, Vector3.zero);
		FastText count = createFastText("Count", textLayer, new Vector2(42.0f, 18.0f), new Vector3(15.0f, -25.0f, 0.0f), "0", 15.0f, FastUITextHorizontalAlignment.Right);
		FastText level = createFastText("Level", textLayer, new Vector2(30.0f, 18.0f), new Vector3(-20.0f, -25.0f, 0.0f), "1", 14.0f, FastUITextHorizontalAlignment.Left);
		FastImage selected = createFastImage("Selected", root, new Vector2(68.0f, 68.0f), Vector3.zero, mInventoryAtlas.mSelected, Color.white);
		selected.setVisible(false);
		return new FastItemView
		{
			mRoot = root,
			mVisibility = visibility,
			mBackground = background,
			mIcon = icon,
			mQualityFrame = quality,
			mBindIcon = bind,
			mLockIcon = lockIcon,
			mSelected = selected,
			mCountText = count,
			mLevelText = level,
		};
	}
	protected UGUIItemView createUGUIItem(RectTransform parent, int index)
	{
		Vector3 itemPosition = getItemPosition(index);
		RectTransform root = createRect("Item_" + index, parent, mItemSize, itemPosition);
		Image background = createUGUIImage("Background", root, mItemSize, Vector3.zero, mInventoryAtlas.mSlotBackground, new Color(0.86f, 0.90f, 1.0f, 1.0f), Image.Type.Sliced);
		RectTransform content = createRect("ContentLayer", root, mItemSize, Vector3.zero);
		Image icon = createUGUIImage("Icon", content, new Vector2(54.0f, 54.0f), new Vector3(0.0f, 4.0f, 0.0f), getInventoryIconSprite(index), Color.white);
		Image quality = createUGUIImage("QualityFrame", content, new Vector2(62.0f, 62.0f), new Vector3(0.0f, 4.0f, 0.0f), mInventoryAtlas.mQualityFrame, getQualityColor(index));
		RectTransform state = createRect("StateLayer", content, mItemSize, Vector3.zero);
		Image bind = createUGUIImage("Bind", state, new Vector2(15.0f, 15.0f), new Vector3(-24.0f, 24.0f, 0.0f), mInventoryAtlas.mBindIcon, Color.white);
		Image lockIcon = createUGUIImage("Lock", state, new Vector2(15.0f, 15.0f), new Vector3(24.0f, 24.0f, 0.0f), mInventoryAtlas.mLockIcon, Color.white);
		RectTransform textLayer = createRect("TextLayer", content, mItemSize, Vector3.zero);
		TextMeshProUGUI count = createUGUIText("Count", textLayer, new Vector2(42.0f, 18.0f), new Vector3(15.0f, -25.0f, 0.0f), "0", 15.0f, TextAlignmentOptions.Right);
		TextMeshProUGUI level = createUGUIText("Level", textLayer, new Vector2(30.0f, 18.0f), new Vector3(-20.0f, -25.0f, 0.0f), "1", 14.0f, TextAlignmentOptions.Left);
		Image selected = createUGUIImage("Selected", root, new Vector2(68.0f, 68.0f), Vector3.zero, mInventoryAtlas.mSelected, Color.white);
		selected.enabled = false;
		return new UGUIItemView
		{
			mRoot = root,
			mBackground = background,
			mIcon = icon,
			mQualityFrame = quality,
			mBindIcon = bind,
			mLockIcon = lockIcon,
			mSelected = selected,
			mCountText = count,
			mLevelText = level,
		};
	}
	protected void createFastHeader(RectTransform window)
	{
		RectTransform header = createRect("Header", window, new Vector2(1080.0f, 90.0f), new Vector3(0.0f, 470.0f, 0.0f));
		createFastImage("HeaderBG", header, new Vector2(1080.0f, 90.0f), Vector3.zero, mInventoryAtlas.mHeaderBackground, Color.white, FastUIImageType.Sliced);
		createFastText("Title", header, new Vector2(260.0f, 52.0f), new Vector3(-380.0f, 0.0f, 0.0f), "背包", 30.0f, FastUITextHorizontalAlignment.Left);
		RectTransform currencies = createRect("Currencies", header, new Vector2(570.0f, 54.0f), new Vector3(380.0f, 0.0f, 0.0f));
		if (mFastEnableSOA && mFastEnableSuggestedSOA)
		{
			mFastCurrencySOAGroup = currencies.gameObject.AddComponent<FastSOARenderGroup>();
			mFastCurrencySOAGroup.setAllowAdjacentGroupMerge(mFastEnableAdjacentSOAMerge);
		}
		for (int i = 0; i < 3; ++i)
		{
			float localX = -190.0f + i * 190.0f;
			RectTransform record = createRect("Currency_" + i, currencies, new Vector2(180.0f, 54.0f), new Vector3(localX, 0.0f, 0.0f));
			createFastImage("Icon", record, new Vector2(32.0f, 32.0f), new Vector3(-54.0f, 0.0f, 0.0f), getInventoryIconSprite(20 + i), Color.white);
			createFastText("Text", record, new Vector2(120.0f, 36.0f), new Vector3(21.0f, 0.0f, 0.0f), mNumberStrings[1000 + i * 357], 20.0f, FastUITextHorizontalAlignment.Left);
		}
	}
	protected void createUGUIHeader(RectTransform window)
	{
		RectTransform header = createRect("Header", window, new Vector2(1080.0f, 90.0f), new Vector3(0.0f, 470.0f, 0.0f));
		createUGUIImage("HeaderBG", header, new Vector2(1080.0f, 90.0f), Vector3.zero, mInventoryAtlas.mHeaderBackground, Color.white, Image.Type.Sliced);
		createUGUIText("Title", header, new Vector2(260.0f, 52.0f), new Vector3(-380.0f, 0.0f, 0.0f), "背包", 30.0f, TextAlignmentOptions.Left);
		RectTransform currencies = createRect("Currencies", header, new Vector2(570.0f, 54.0f), new Vector3(380.0f, 0.0f, 0.0f));
		for (int i = 0; i < 3; ++i)
		{
			float localX = -190.0f + i * 190.0f;
			RectTransform record = createRect("Currency_" + i, currencies, new Vector2(180.0f, 54.0f), new Vector3(localX, 0.0f, 0.0f));
			createUGUIImage("Icon", record, new Vector2(32.0f, 32.0f), new Vector3(-54.0f, 0.0f, 0.0f), getInventoryIconSprite(20 + i), Color.white);
			createUGUIText("Text", record, new Vector2(120.0f, 36.0f), new Vector3(21.0f, 0.0f, 0.0f), mNumberStrings[1000 + i * 357], 20.0f, TextAlignmentOptions.Left);
		}
	}
	protected void createFastTabs(RectTransform window)
	{
		RectTransform tabs = createRect("Tabs", window, new Vector2(800.0f, 54.0f), new Vector3(-150.0f, 405.0f, 0.0f));
		if (mFastEnableSOA && mFastEnableSuggestedSOA)
		{
			mFastTabsSOAGroup = tabs.gameObject.AddComponent<FastSOARenderGroup>();
			mFastTabsSOAGroup.setAllowAdjacentGroupMerge(mFastEnableAdjacentSOAMerge);
		}
		for (int i = 0; i < 6; ++i)
		{
			RectTransform tab = createRect("Tab_" + i, tabs, new Vector2(124.0f, 48.0f), new Vector3(-330.0f + i * 132.0f, 0.0f, 0.0f));
			createFastImage("BG", tab, new Vector2(124.0f, 48.0f), Vector3.zero, mInventoryAtlas.mTabBackground, i == 0 ? Color.white : new Color(0.62f, 0.68f, 0.78f, 1.0f), FastUIImageType.Sliced);
			createFastText("Text", tab, new Vector2(100.0f, 32.0f), Vector3.zero, "分类" + (i + 1), 18.0f, FastUITextHorizontalAlignment.Center);
		}
	}
	protected void createUGUITabs(RectTransform window)
	{
		RectTransform tabs = createRect("Tabs", window, new Vector2(800.0f, 54.0f), new Vector3(-150.0f, 405.0f, 0.0f));
		for (int i = 0; i < 6; ++i)
		{
			RectTransform tab = createRect("Tab_" + i, tabs, new Vector2(124.0f, 48.0f), new Vector3(-330.0f + i * 132.0f, 0.0f, 0.0f));
			createUGUIImage("BG", tab, new Vector2(124.0f, 48.0f), Vector3.zero, mInventoryAtlas.mTabBackground, i == 0 ? Color.white : new Color(0.62f, 0.68f, 0.78f, 1.0f), Image.Type.Sliced);
			createUGUIText("Text", tab, new Vector2(100.0f, 32.0f), Vector3.zero, "分类" + (i + 1), 18.0f, TextAlignmentOptions.Center);
		}
	}
	protected void createFastDetailPanel(RectTransform window)
	{
		RectTransform detail = createRect("Detail", window, new Vector2(300.0f, 900.0f), new Vector3(425.0f, -40.0f, 0.0f));
		createFastImage("BG", detail, new Vector2(300.0f, 900.0f), Vector3.zero, mInventoryAtlas.mDetailBackground, Color.white, FastUIImageType.Sliced);
		mFastDetailIcon = createFastImage("Icon", detail, new Vector2(120.0f, 120.0f), new Vector3(0.0f, 320.0f, 0.0f), getInventoryIconSprite(0), Color.white);
		mFastDetailName = createFastText("Name", detail, new Vector2(250.0f, 46.0f), new Vector3(0.0f, 230.0f, 0.0f), mItemNames[0], 24.0f, FastUITextHorizontalAlignment.Center);
		mFastDetailDescription = createFastText("Description", detail, new Vector2(250.0f, 260.0f), new Vector3(0.0f, 70.0f, 0.0f), mItemDescriptions[0], 17.0f, FastUITextHorizontalAlignment.Left);
		mFastDetailDescription.setVerticalAlignment(FastUITextVerticalAlignment.Top);
		mFastDetailDescription.setWordWrap(true);
		for (int i = 0; i < 6; ++i)
		{
			createFastText("Property_" + i, detail, new Vector2(240.0f, 28.0f), new Vector3(0.0f, -120.0f - i * 34.0f, 0.0f), "属性" + (i + 1) + "  +" + (20 + i * 7), 16.0f, FastUITextHorizontalAlignment.Left);
		}
	}
	protected void createUGUIDetailPanel(RectTransform window)
	{
		RectTransform detail = createRect("Detail", window, new Vector2(300.0f, 900.0f), new Vector3(425.0f, -40.0f, 0.0f));
		createUGUIImage("BG", detail, new Vector2(300.0f, 900.0f), Vector3.zero, mInventoryAtlas.mDetailBackground, Color.white, Image.Type.Sliced);
		mUGUIDetailIcon = createUGUIImage("Icon", detail, new Vector2(120.0f, 120.0f), new Vector3(0.0f, 320.0f, 0.0f), getInventoryIconSprite(0), Color.white);
		mUGUIDetailName = createUGUIText("Name", detail, new Vector2(250.0f, 46.0f), new Vector3(0.0f, 230.0f, 0.0f), mItemNames[0], 24.0f, TextAlignmentOptions.Center);
		mUGUIDetailDescription = createUGUIText("Description", detail, new Vector2(250.0f, 260.0f), new Vector3(0.0f, 70.0f, 0.0f), mItemDescriptions[0], 17.0f, TextAlignmentOptions.TopLeft);
		for (int i = 0; i < 6; ++i)
		{
			createUGUIText("Property_" + i, detail, new Vector2(240.0f, 28.0f), new Vector3(0.0f, -120.0f - i * 34.0f, 0.0f), "属性" + (i + 1) + "  +" + (20 + i * 7), 16.0f, TextAlignmentOptions.Left);
		}
	}
	protected FastImage createFastImage(string name, RectTransform parent, Vector2 size, Vector3 position, Sprite sprite, Color color, FastUIImageType type = FastUIImageType.Simple)
	{
		RectTransform rect = createRect(name, parent, size, position);
		FastImage image = rect.gameObject.AddComponent<FastImage>();
		image.setSprite(sprite);
		image.setType(type);
		image.setColor(color);
		return image;
	}
	protected Image createUGUIImage(string name, RectTransform parent, Vector2 size, Vector3 position, Sprite sprite, Color color, Image.Type type = Image.Type.Simple)
	{
		RectTransform rect = createRect(name, parent, size, position);
		Image image = rect.gameObject.AddComponent<Image>();
		image.sprite = sprite;
		image.type = type;
		image.color = color;
		image.raycastTarget = false;
		if (mImageMaterial != null)
		{
			image.material = mImageMaterial;
		}
		return image;
	}
	protected FastRawImage createFastRawImage(string name, RectTransform parent, Vector2 size, Vector3 position, Texture texture, Color color)
	{
		RectTransform rect = createRect(name, parent, size, position);
		FastRawImage image = rect.gameObject.AddComponent<FastRawImage>();
		image.setTexture(texture);
		image.setColor(color);
		return image;
	}
	protected FastText createFastText(string name, RectTransform parent, Vector2 size, Vector3 position, string text, float fontSize, FastUITextHorizontalAlignment alignment)
	{
		RectTransform rect = createRect(name, parent, size, position);
		FastText label = rect.gameObject.AddComponent<FastText>();
		label.setFont(mFont);
		label.setFontSize(fontSize);
		label.setText(text);
		label.setWordWrap(false);
		label.setHorizontalAlignment(alignment);
		label.setVerticalAlignment(FastUITextVerticalAlignment.Middle);
		return label;
	}
	protected RawImage createUGUIRawImage(string name, RectTransform parent, Vector2 size, Vector3 position, Texture texture, Color color)
	{
		RectTransform rect = createRect(name, parent, size, position);
		RawImage image = rect.gameObject.AddComponent<RawImage>();
		image.texture = texture;
		image.color = color;
		image.raycastTarget = false;
		if (mImageMaterial != null)
		{
			image.material = mImageMaterial;
		}
		return image;
	}
	protected TextMeshProUGUI createUGUIText(string name, RectTransform parent, Vector2 size, Vector3 position, string text, float fontSize, TextAlignmentOptions alignment)
	{
		RectTransform rect = createRect(name, parent, size, position);
		TextMeshProUGUI label = rect.gameObject.AddComponent<TextMeshProUGUI>();
		label.font = mFont;
		label.fontSize = fontSize;
		label.text = text;
		label.alignment = alignment;
		label.raycastTarget = false;
		label.richText = true;
		label.overflowMode = TextOverflowModes.Overflow;
		return label;
	}
	protected RectTransform createRect(string name, Transform parent, Vector2 size, Vector3 position)
	{
		GameObject gameObject = new(name, typeof(RectTransform));
		RectTransform rect = gameObject.GetComponent<RectTransform>();
		rect.SetParent(parent, false);
		setupRect(rect, size, position);
		return rect;
	}
	protected void setupRect(RectTransform rect, Vector2 size, Vector3 position)
	{
		rect.anchorMin = new Vector2(0.5f, 0.5f);
		rect.anchorMax = new Vector2(0.5f, 0.5f);
		rect.pivot = new Vector2(0.5f, 0.5f);
		rect.sizeDelta = size;
		rect.localPosition = position;
		rect.localScale = Vector3.one;
		rect.localRotation = Quaternion.identity;
	}
	protected void prepareStrings()
	{
		mNumberStrings = new string[10000];
		for (int i = 0; i < mNumberStrings.Length; ++i)
		{
			mNumberStrings[i] = i.ToString();
		}
		mItemNames = new string[mItemCount];
		mItemDescriptions = new string[mItemCount];
		for (int i = 0; i < mItemCount; ++i)
		{
			mItemNames[i] = "道具 " + (i + 1);
			mItemDescriptions[i] = "这是背包真实场景Benchmark中的道具描述。包含多行中文文字、属性信息与使用说明，用于同时覆盖图片、文字、裁剪和复杂层级。ID:" + (i + 1);
		}
	}
	protected void collectHierarchyStats()
	{
		mFastRectCount = mFastRoot.GetComponentsInChildren<RectTransform>(true).Length;
		mFastRenderElementCount = mFastRoot.GetComponentsInChildren<FastUIRenderElement>(true).Length;
		mFastImageCount = mFastRoot.GetComponentsInChildren<FastImage>(true).Length;
		mFastRawImageCount = mFastRoot.GetComponentsInChildren<FastRawImage>(true).Length;
		mFastTextCount = mFastRoot.GetComponentsInChildren<FastText>(true).Length;
		mFastHierarchyDepth = getMaxHierarchyDepth(mFastRoot.transform);
		mUGUIRectCount = mUGUIRoot.GetComponentsInChildren<RectTransform>(true).Length;
		mUGUIGraphicCount = mUGUIRoot.GetComponentsInChildren<Graphic>(true).Length;
		mUGUIImageCount = mUGUIRoot.GetComponentsInChildren<Image>(true).Length;
		mUGUIRawImageCount = mUGUIRoot.GetComponentsInChildren<RawImage>(true).Length;
		mUGUITextCount = mUGUIRoot.GetComponentsInChildren<TextMeshProUGUI>(true).Length;
		mUGUICanvasRendererCount = mUGUIRoot.GetComponentsInChildren<CanvasRenderer>(true).Length;
		mUGUIHierarchyDepth = getMaxHierarchyDepth(mUGUIRoot.transform);
	}
	protected bool isInventoryAtlasValid()
	{
		return mInventoryAtlas != null && mInventoryAtlas.mTexture != null && mInventoryAtlas.mSlotBackground != null &&
			mInventoryAtlas.mQualityFrame != null && mInventoryAtlas.mBindIcon != null && mInventoryAtlas.mLockIcon != null &&
			mInventoryAtlas.mSelected != null && mInventoryAtlas.mHeaderBackground != null && mInventoryAtlas.mTabBackground != null &&
			mInventoryAtlas.mDetailBackground != null && mInventoryAtlas.mItemIcons != null && mInventoryAtlas.mItemIcons.Length > 1;
	}
	protected Sprite getInventoryIconSprite(int index)
	{
		return mInventoryAtlas != null ? mInventoryAtlas.getItemIcon(index) : null;
	}
	protected bool validateNormalSpriteUsage()
	{
		Texture atlasTexture = mInventoryAtlas != null ? mInventoryAtlas.mTexture : null;
		bool fastItemsValid = true;
		bool uguiItemsValid = true;
		for (int i = 0; i < mItemCount; ++i)
		{
			FastItemView fast = mFastItems[i];
			UGUIItemView ugui = mUGUIItems[i];
			fastItemsValid &= fast != null && fast.mIcon != null && fast.mIcon.getSprite() != null && fast.mIcon.getRenderTexture() == atlasTexture;
			uguiItemsValid &= ugui != null && ugui.mIcon != null && ugui.mIcon.sprite != null && ugui.mIcon.sprite.texture == atlasTexture;
		}
		bool valid = isInventoryAtlasValid() && fastItemsValid && uguiItemsValid && mFastImageCount > 0 && mUGUIImageCount > 0 &&
			mFastRawImageCount == 0 && mUGUIRawImageCount == 0;
		benchmarkLog("[Inventory Sprite Usage] Atlas=" + (atlasTexture != null ? atlasTexture.name : "null") +
			" | FastImage=" + mFastImageCount + " | FastRawImage=" + mFastRawImageCount + " | UGUIImage=" + mUGUIImageCount +
			" | UGUIRawImage=" + mUGUIRawImageCount + " | ItemSpriteTextureShared=" + (fastItemsValid && uguiItemsValid) +
			" | NormalSpriteUsage=" + valid);
		if (!valid)
		{
			benchmarkLogError("[Inventory Sprite Usage] 主背包不是正常Sprite/Image模型，停止Benchmark。请重新生成Inventory Benchmark场景。");
		}
		return valid;
	}
	protected int getMaxHierarchyDepth(Transform root)
	{
		int maxDepth = 0;
		collectMaxDepth(root, 0, ref maxDepth);
		return maxDepth;
	}
	protected void collectMaxDepth(Transform transformNode, int depth, ref int maxDepth)
	{
		if (depth > maxDepth)
		{
			maxDepth = depth;
		}
		for (int i = 0; i < transformNode.childCount; ++i)
		{
			collectMaxDepth(transformNode.GetChild(i), depth + 1, ref maxDepth);
		}
	}
	protected void logEnvironment()
	{
		FastUIMeshRenderer renderer = mFastCanvas != null ? mFastCanvas.getMeshRenderer() : null;
		bool unsafeBackend = renderer != null && isAllRuntimeStructUnsafe(renderer);
		benchmarkLog("[README Scene] Items=" + mItemCount + " | Columns=" + mColumnCount + " | FastRects=" + mFastRectCount + " | FastRenderElements=" + mFastRenderElementCount + " | FastImages=" + mFastImageCount + " | FastTexts=" + mFastTextCount + " | UGUIRects=" + mUGUIRectCount + " | UGUIGraphics=" + mUGUIGraphicCount + " | UGUIImages=" + mUGUIImageCount + " | UGUITMP=" + mUGUITextCount + " | HierarchyDepthF/U=" + mFastHierarchyDepth + "/" + mUGUIHierarchyDepth + " | VisualElementsPerItem=8 | TextPerItem=2");
		benchmarkLog("================ FastUI vs UGUI Real-World Inventory Benchmark =================\n" +
			"Runtime:" + Application.unityVersion + " | Platform:" + Application.platform + " | Editor:" + Application.isEditor + " | VSync:" + QualitySettings.vSyncCount +
			" | TargetFPS:" + Application.targetFrameRate + " | ScriptingBackend:" + getScriptingBackendName() + " | DevelopmentBuild:" + Debug.isDebugBuild +
			" | CppConfiguration:" + getCppConfigurationName() + "\n" +
			"EasyECS:Unsafe=" + unsafeBackend + (renderer != null ? " | Position=" + renderer.getPositionBackendName() + " | Range=" + renderer.getRangeBackendName() +
			" | TransformNode=" + mFastCanvas.getTransformNodeBackendName() : "") + "\n" +
			"Inventory:Profile=NormalSpriteInventory | Items=" + mItemCount + " | Columns=" + mColumnCount + " | VisibleRefresh=" + mVisibleRefreshCount +
			" | BatchRefresh=" + mBatchRefreshCount + " | StressRefresh=" + mStressRefreshCount + "x" + PUBLICATION_STRESS_MUTATION_PASSES +
			" | ScatterRefresh=" + mScatterRefreshCount + " | InitMode=OneFrameSync" +
			" | VisualElements/Item=8 | Text/Item=2 | Mask=FastRectMask2D(Stencil)/RectMask2D | Layout=ManualSamePositions" +
			" | FastSOA=" + mFastEnableSOA + " | SuggestedSOA=" + mFastEnableSuggestedSOA + " | AdjacentSOAMerge=" + mFastEnableAdjacentSOAMerge + " | SOAMode=FixedStructure\n" +
			"FastUI:Rects=" + mFastRectCount + " | RenderElements=" + mFastRenderElementCount + " | FastImage=" + mFastImageCount + " | FastRawImage=" + mFastRawImageCount + " | Text=" + mFastTextCount + " | HierarchyDepth=" + mFastHierarchyDepth + "\n" +
			"UGUI:Rects=" + mUGUIRectCount + " | Graphics=" + mUGUIGraphicCount + " | Image=" + mUGUIImageCount + " | RawImage=" + mUGUIRawImageCount + " | TMP=" + mUGUITextCount +
			" | CanvasRenderer=" + mUGUICanvasRendererCount + " | HierarchyDepth=" + mUGUIHierarchyDepth + "\n" +
			"Sequence:FastUI-A -> UGUI-A -> UGUI-B -> FastUI-B | StabilitySpread<=20%\n" +
			"Scope:FastUI=Mutation+FastCanvas CPU; UGUI=Mutation+Canvas.ForceUpdateCanvases. Native Canvas.BuildBatch/RenderThread/GPU不在本轮统计范围.");
	}
	protected bool validateFastClipStencil()
	{
		if (mFastViewportClip == null || mFastCanvas == null)
		{
			benchmarkLogError("[FastUI vs UGUI Inventory] FastRectMask2D Stencil初始化失败:Viewport或FastCanvas为空.");
			return false;
		}
		mFastViewportClip.refreshStencilStates();
		FastClipStencilGraphic writer = mFastViewportClip.getWriterGraphic();
		FastClipStencilGraphic pop = mFastViewportClip.getPopGraphic();
		FastUIMaskState writerState = writer != null ? writer.getMaskState() : default;
		FastUIMaskState popState = pop != null ? pop.getMaskState() : default;
		Material defaultMaterial = mFastCanvas.getDefaultMaterial();
		bool valid = writer != null && pop != null && writerState.mMode == FastUIMaskMaterialMode.Writer &&
			popState.mMode == FastUIMaskMaterialMode.Pop && FastUIMaskMaterialCache.isMaterialSupported(defaultMaterial);
		benchmarkLog("[FastUI vs UGUI Inventory] FastClipStencil:Writer=" + (writer != null) + " | Pop=" + (pop != null) +
			" | WriterRef=" + writerState.mStencilRef + " | PopRef=" + popState.mStencilRef + " | MaterialSupported=" +
			FastUIMaskMaterialCache.isMaterialSupported(defaultMaterial) + " | Valid=" + valid);
		if (!valid)
		{
			benchmarkLogError("[FastUI vs UGUI Inventory] FastRectMask2D Stencil环境无效，停止Benchmark.");
		}
		return valid;
	}
	protected bool validateBenchmarkEnvironment()
	{
		if (Application.isEditor)
		{
			benchmarkLog("[FastUI vs UGUI Inventory] BenchmarkEnvironment:EditorReference | FormalPlayer=False");
			return true;
		}
		FastUIMeshRenderer renderer = mFastCanvas != null ? mFastCanvas.getMeshRenderer() : null;
		bool il2Cpp = isIL2CPP();
		bool unsafeBackend = renderer != null && isAllRuntimeStructUnsafe(renderer);
		bool masterBuildTag = isFormalMasterBuild();
		bool developmentOff = !Debug.isDebugBuild;
		bool valid = il2Cpp && unsafeBackend && developmentOff;
		benchmarkLog("[FastUI vs UGUI Inventory] BenchmarkEnvironment:Player | IL2CPP:" + il2Cpp + " | EasyECSUnsafe:" + unsafeBackend +
			" | DevelopmentBuildOff:" + developmentOff + " | FormalMasterBuildTag:" + masterBuildTag + " | Valid:" + valid);
		if (!valid)
		{
			benchmarkLogError("[FastUI vs UGUI Inventory] Benchmark环境无效，停止自动Benchmark。Player测试要求IL2CPP + EasyECS Unsafe + DevelopmentBuild关闭。C++ Configuration=Master由Editor构建配置保证，Runtime不再依赖人为Define阻断测试.");
		}
		return valid;
	}
	protected bool isAllRuntimeStructUnsafe(FastUIMeshRenderer renderer)
	{
		return renderer.isPositionUnsafeBackend() &&
			renderer.isPositionDeltaUnsafeBackend() &&
			renderer.isIntUnsafeBackend() &&
			renderer.isBoolUnsafeBackend() &&
			renderer.isRangeUnsafeBackend() &&
			renderer.isColorUnsafeBackend() &&
			renderer.isUVUnsafeBackend() &&
			renderer.isGeometryRangeUnsafeBackend() &&
			renderer.isBatchRunUnsafeBackend() &&
			renderer.isDrawRunUnsafeBackend() &&
			renderer.isSubMeshDescriptorUnsafeBackend() &&
			mFastCanvas.isTransformNodeUnsafeBackend();
	}
	protected void OnDestroy()
	{
		stopRenderRecorders();
		restoreBenchmarkLogStackTrace();
		if (mBenchmarkCompleteRoot != null)
		{
			Destroy(mBenchmarkCompleteRoot);
			mBenchmarkCompleteRoot = null;
		}
		if (mBenchmarkMaterialA != null)
		{
			Destroy(mBenchmarkMaterialA);
			mBenchmarkMaterialA = null;
		}
		if (mBenchmarkMaterialB != null)
		{
			Destroy(mBenchmarkMaterialB);
			mBenchmarkMaterialB = null;
		}
	}
	protected string getScriptingBackendName()
	{
#if ENABLE_IL2CPP
		return "IL2CPP";
#elif ENABLE_MONO
		return "Mono";
#else
		return "Unknown";
#endif
	}
	protected bool isIL2CPP()
	{
#if ENABLE_IL2CPP
		return true;
#else
		return false;
#endif
	}
	protected bool isFormalMasterBuild()
	{
#if FASTGUI_FORMAL_BENCHMARK_MASTER
		return true;
#else
		return false;
#endif
	}
	protected string getCppConfigurationName()
	{
#if FASTGUI_FORMAL_BENCHMARK_MASTER
		return "Master(FormalBuild)";
#else
		return string.IsNullOrEmpty(mCppCompilerConfigurationTag) ? "Unknown(Unverified)" : mCppCompilerConfigurationTag + "(UnverifiedTag)";
#endif
	}
	protected Vector3 getItemPosition(int index)
	{
		int x = index % mColumnCount;
		int y = index / mColumnCount;
		float totalWidth = (mColumnCount - 1) * mItemInterval.x;
		float startX = -totalWidth * 0.5f;
		float startY = getContentHeight() * 0.5f - mItemSize.y * 0.5f;
		return new Vector3(startX + x * mItemInterval.x, startY - y * mItemInterval.y, 0.0f);
	}
	protected float getContentHeight()
	{
		int rowCount = Mathf.CeilToInt((float)mItemCount / mColumnCount);
		return Mathf.Max(mViewportSize.y, rowCount * mItemInterval.y + 24.0f);
	}
	protected float getMaxScroll() { return Mathf.Max(0.0f, getContentHeight() - mViewportSize.y); }
	protected int getWrappedIndex(int value)
	{
		if (mItemCount <= 0)
		{
			return 0;
		}
		value %= mItemCount;
		return value < 0 ? value + mItemCount : value;
	}
	protected int getRangeStart(int value, int count)
	{
		count = Mathf.Clamp(count, 1, mItemCount);
		int maxStart = Mathf.Max(mItemCount - count, 0);
		if (maxStart <= 0)
		{
			return 0;
		}
		value %= maxStart + 1;
		return value < 0 ? value + maxStart + 1 : value;
	}
	protected Color getQualityColor(int value)
	{
		switch (Mathf.Abs(value) & 3)
		{
			case 0: return new Color(0.65f, 0.65f, 0.68f, 1.0f);
			case 1: return new Color(0.30f, 0.78f, 0.42f, 1.0f);
			case 2: return new Color(0.32f, 0.52f, 0.95f, 1.0f);
			default: return new Color(0.78f, 0.38f, 0.92f, 1.0f);
		}
	}
	protected string getCaseName(InventoryCase testCase)
	{
		switch (testCase)
		{
			case InventoryCase.SingleItemRefresh: return "单个道具完整刷新";
			case InventoryCase.VisiblePageRefresh: return "可见区批量" + mVisibleRefreshCount + "道具混合刷新";
			case InventoryCase.FullInventoryRebind: return "整理背包" + mItemCount + "道具全量重绑";
			case InventoryCase.QuantityTextRefresh: return mBatchRefreshCount + "数量文字变化";
			case InventoryCase.IconSpriteRefresh: return mBatchRefreshCount + "图标同图集Sprite变化";
			case InventoryCase.ContentScroll: return "背包Content持续滚动+裁剪";
			case InventoryCase.SelectionDetailRefresh: return "选中道具+详情面板刷新";
			case InventoryCase.FilterToggle: return "筛选显隐" + mBatchRefreshCount + "个Item(FastVisibility vs SetActive)";
			case InventoryCase.WindowFastVisibility: return "整窗口FastCanvas.setVisible vs UGUI SetActive";
			case InventoryCase.WindowSetActive: return "整窗口SetActive兼容路径";
			case InventoryCase.MixedUse: return "真实混合操作:滚动+20数量+4图标状态+详情";
			case InventoryCase.IconPositionRefresh: return mBatchRefreshCount + "图标局部Position变化";
			case InventoryCase.ItemRootPositionRefresh: return mVisibleRefreshCount + "Item Root整体移动";
			case InventoryCase.IconSizeRefresh: return mBatchRefreshCount + "图标Size变化";
			case InventoryCase.QualityColorRefresh: return mBatchRefreshCount + "Quality Color变化";
			case InventoryCase.ScatteredIconSpriteRefresh: return mBatchRefreshCount + "离散图标同图集Sprite变化";
			case InventoryCase.LeafVisibilityRefresh: return mBatchRefreshCount + "叶节点显隐(FastVisible vs Graphic.enabled)";
			case InventoryCase.TextColorRefresh: return mBatchRefreshCount + "文本Color变化";
			case InventoryCase.TextFontSizeRefresh: return mBatchRefreshCount + "文本FontSize变化";
			case InventoryCase.IconTransformRefresh: return mVisibleRefreshCount + "图标Scale+Rotation变化";
			case InventoryCase.RectMaskResize: return "Viewport FastRectMask2D尺寸变化";
			case InventoryCase.StressMixedRefresh: return mStressRefreshCount + "Item重负荷复合刷新(Sprite+Text+State+Geometry+Scroll)";
			case InventoryCase.FullTextRefresh: return mItemCount + "Item全量双文本内容变化";
			case InventoryCase.FullStateRefresh: return mItemCount + "Item全量状态变化(Color+2 Visibility)";
			case InventoryCase.FullLeafVisibilityRefresh: return mItemCount + "叶节点全量显隐";
			case InventoryCase.ScatteredFilterToggle: return mScatterRefreshCount + "Item离散子树显隐(FastUIVisibility vs SetActive)";
			case InventoryCase.StressIconGeometryRefresh: return mStressRefreshCount + "图标复合Geometry变化(Size+Sprite+Position)";
			case InventoryCase.StressItemRootPositionRefresh: return mStressRefreshCount + "Item Root批量整体移动";
			case InventoryCase.IconMaterialRefresh: return mBatchRefreshCount + "图标Material变化";
			case InventoryCase.RectMaskPaddingRefresh: return "Viewport FastRectMask2D Padding变化";
			case InventoryCase.RectMaskMoveResize: return "Viewport FastRectMask2D移动+尺寸变化";
			default: return testCase.ToString();
		}
	}
	protected double average(double a, double b) { return (a + b) * 0.5; }
	protected double getSpreadPercent(double a, double b)
	{
		double avg = average(a, b);
		return avg > 0.000001 ? Math.Abs(a - b) / avg * 100.0 : 0.0;
	}
}
