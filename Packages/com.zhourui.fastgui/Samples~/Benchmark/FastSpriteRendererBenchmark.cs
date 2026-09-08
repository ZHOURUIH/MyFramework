using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Rendering;
using Debug = UnityEngine.Debug;

// Comprehensive end-to-end runtime comparison between Unity SpriteRenderer and FastSpriteRenderer.
//
// Design goals:
// 1. Measure count scaling from 0/1 Sprite to 10k Sprites.
// 2. Cover flat and complex Transform hierarchies.
// 3. Cover batching pressure: SortingOrder / Texture / Material splits.
// 4. Cover object-level culling differences by changing the visible ratio.
// 5. Cover FullRect / tight mesh / Sliced / Tiled / mixed geometry.
// 6. Cover static, sparse mutation, full mutation, sorting and enable/disable churn.
// 7. Keep Fast and Native scenes structurally equivalent.
// 8. Use ABBA (Fast -> Native -> Native -> Fast) to reduce temporal/thermal bias.
//
// Run in an otherwise empty Scene. Publication numbers should come from non-development Player builds.
public sealed class FastSpriteRendererBenchmark : MonoBehaviour
{
	public enum BenchmarkSuite
	{
		Smoke,
		Core,
		Full,
		Custom,
	}

	public enum SceneProfile
	{
		FlatSameBatch,
		Grouped2Level,
		Grouped3Level,
		DeepGroupedHierarchy,
		MultiSortingOrder,
		MultiTexture,
		MultiMaterial,
		Visibility50Percent,
		Visibility10Percent,
		Visibility1Percent,
		FullOverlap,
		TightMesh,
		MixedDrawModes,
	}

	public enum BenchmarkCase
	{
		Static,
		MoveOne,
		Move10Percent,
		MoveAll,
		Rotate10Percent,
		RotateAll,
		Scale10Percent,
		ScaleAll,
		RootMoveAll,
		ParentMoveAll,
		ParentRotateAll,
		SpriteSwap10Percent,
		SpriteSwapAll,
		TextureSwap10Percent,
		TextureSwapAll,
		Color10Percent,
		ColorAll,
		FlipAll,
		SortingOrder10Percent,
		SortingOrderAll,
		MaterialSwap10Percent,
		MaterialSwapAll,
		EnableDisable1Percent,
		EnableDisable10Percent,
		EnableDisableAll,
		SlicedResize10Percent,
		SlicedResizeAll,
		TiledResize10Percent,
		TiledResizeAll,
		CameraMove,
	}

	private enum Target
	{
		Native,
		Fast,
	}

	private sealed class Scenario
	{
		public int mID;
		public SceneProfile mProfile;
		public int mCount;
		public int mVariantCount = 1;
		public FastSpriteSortMode mSortMode = FastSpriteSortMode.CameraDistance;
		public readonly List<BenchmarkCase> mCases = new();

		public float getVisibleFraction()
		{
			switch (mProfile)
			{
				case SceneProfile.Visibility50Percent: return 0.50f;
				case SceneProfile.Visibility10Percent: return 0.10f;
				case SceneProfile.Visibility1Percent: return 0.01f;
				default: return 1.0f;
			}
		}
	}

	private struct Result
	{
		public int mScenarioID;
		public int mSegment;
		public Target mTarget;
		public SceneProfile mProfile;
		public BenchmarkCase mCase;
		public FastSpriteSortMode mSortMode;
		public int mCount;
		public int mVariantCount;
		public float mVisibleFraction;
		public int mFrames;
		public double mWallMean;
		public double mWallMedian;
		public double mWallP95;
		public double mWallP99;
		public double mMutationMean;
		public double mDrawCalls;
		public double mBatches;
		public double mSetPass;
		public double mVertices;
		public double mTriangles;
		public double mVBUpload;
		public double mIBUpload;
		public double mGCAlloc;
		public int mGCCollections0;
		public double mCPUFrame;
		public double mMainThread;
		public double mRenderThread;
		public double mGPUFrame;
		public int mFastBatchCount;
		public int mFastVertexCount;
		public int mFastIndexCount;
		public double mFastInternalVBUpload;
		public double mFastInternalIBUpload;
		public double mFastSystemTotalMS;
		public double mFastRendererScanMS;
		public double mFastMembershipMS;
		public double mFastBatchUpdateMS;
		public double mFastTopologyMS;
		public double mFastElementScanMS;
		public double mFastGeometryBuildMS;
		public double mFastVertexUploadMS;
		public double mFastSortIndexMS;
		public double mFastBoundsMS;
		public double mFastTransformChangedCount;
		public double mFastMembershipCheckCount;
		public double mFastMembershipChangedCount;
		public double mFastTopologyRebuildCount;
		public double mFastDirtyElementCount;
		public double mFastVertexUploadCallCount;
		public double mFastIndexUploadCallCount;
	}

	private sealed class PairScene
	{
		public Scenario mScenario;
		public GameObject mNativeRoot;
		public SpriteRenderer[] mNative;
		public Transform[] mNativeMotionParents;
		public GameObject mFastRoot;
		public FastSpriteRenderer[] mFast;
		public Transform[] mFastMotionParents;
		public FastSpriteRenderSystem mFastSystem;
		public Vector3[] mLocalPositions;
		public Vector3 mCameraPosition;
		public Quaternion mCameraRotation;
		public float mCameraOrthographicSize;
	}

	[Header("Primary Sprites")]
	[Tooltip("SpriteA and SpriteB should normally be two sprites from the same atlas/texture and the same topology.")]
	public Sprite mSpriteA;
	public Sprite mSpriteB;
	[Tooltip("Optional. If empty, the benchmark generates a deterministic tight-mesh sprite pair.")]
	public Sprite mTightSpriteA;
	public Sprite mTightSpriteB;
	public Material mMaterial;

	[Header("Scene")]
	public Camera mCamera;
	public int mColumns = 100;
	public float mSpacing = 0.72f;
	[Min(1)] public int mGroupSize = 32;
	[Range(2, 12)] public int mDeepHierarchyDepth = 6;

	[Header("Suite")]
	public BenchmarkSuite mSuite = BenchmarkSuite.Core;
	public int mWarmupFrames = 30;
	public int mSampleFrames = 120;
	public bool mRunOnStart = true;
	public bool mUseSandwichOrder = true;
	public bool mDisableVSync = true;
	[Min(1)] public int mUnlockedTargetFrameRate = 1000;
	public bool mCollectFastInternalProfile = true;
	public bool mCollectGarbageBetweenScenarios = true;

	[Header("5 Minute Budget")]
	[Tooltip("Core uses the shorter frame counts below even if this component still has the old 30/120 serialized values.")]
	public bool mUseFiveMinuteCorePreset = true;
	[Min(1)] public int mCoreWarmupFrames = 4;
	[Min(10)] public int mCoreSampleFrames = 20;
	[Tooltip("Hard stop for measurement. 270 seconds leaves about 30 seconds for scene cleanup and report writing before five minutes.")]
	public bool mEnforceTotalTimeBudget = true;
	[Range(60.0f, 290.0f)] public float mMaxTotalDurationSeconds = 270.0f;

	[Header("Finished Overlay")]
	[Tooltip("Shown only after measurement has stopped, so it does not contaminate benchmark draw calls or CPU timings.")]
	public bool mShowFinishedOverlay = true;
	[Range(20, 72)] public int mFinishedOverlayFontSize = 42;

	[Header("Custom Suite")]
	public int[] mCustomCounts = { 1, 100, 1000, 5000 };
	public SceneProfile[] mCustomProfiles = { SceneProfile.FlatSameBatch };
	public BenchmarkCase[] mCustomCases = { BenchmarkCase.Static, BenchmarkCase.MoveAll, BenchmarkCase.ColorAll };
	[Min(1)] public int mCustomVariantCount = 8;
	public FastSpriteSortMode mCustomSortMode = FastSpriteSortMode.CameraDistance;

	private const int GENERATED_VARIANT_COUNT = 16;
	private readonly List<Result> mResults = new();
	private readonly FrameTiming[] mFrameTimings = new FrameTiming[1];
	private readonly List<Texture2D> mOwnedVariantTextures = new();
	private readonly List<Sprite> mOwnedVariantSprites = new();
	private readonly List<Material> mOwnedVariantMaterials = new();
	private readonly List<UnityEngine.Object> mOwnedExtraAssets = new();
	private Sprite[] mTextureVariantSprites;
	private Material[] mMaterialVariants;
	private ProfilerRecorder mDrawCalls;
	private ProfilerRecorder mBatches;
	private ProfilerRecorder mSetPass;
	private ProfilerRecorder mVertices;
	private ProfilerRecorder mTriangles;
	private ProfilerRecorder mVBUpload;
	private ProfilerRecorder mIBUpload;
	private ProfilerRecorder mGCAlloc;
	private bool mRecordersStarted;
	private bool mFrameTimingEnabled;
	private int mMutationStep;
	private Texture2D mOwnedTexture;
	private Sprite mOwnedSpriteA;
	private Sprite mOwnedSpriteB;
	private Material mOwnedMaterial;
	private Texture2D mOwnedTightTexture;
	private Sprite mOwnedTightSpriteA;
	private Sprite mOwnedTightSpriteB;
	private bool mBenchmarkRunning;
	private bool mBenchmarkFinished;
	private bool mStoppedByTimeBudget;
	private bool mStopRequested;
	private long mBenchmarkStartTimestamp;
	private long mBenchmarkDeadlineTimestamp;
	private double mBenchmarkElapsedSeconds;
	private int mCompletedScenarioCount;
	private int mTotalScenarioCount;
	private int mCompletedCaseCount;
	private int mTotalCaseCount;
	private string mLastReportPath;
	private GUIStyle mFinishedOverlayStyle;

	private void OnGUI()
	{
		// Never draw benchmark UI while measuring: OnGUI would pollute CPU time and draw-call counters.
		if (!mShowFinishedOverlay || !mBenchmarkFinished || mBenchmarkRunning) return;

		if (mFinishedOverlayStyle == null)
		{
			mFinishedOverlayStyle = new GUIStyle(GUI.skin.box)
			{
				alignment = TextAnchor.MiddleCenter,
				fontSize = Mathf.Max(20, mFinishedOverlayFontSize),
				fontStyle = FontStyle.Bold,
				wordWrap = true,
			};
		}

		float width = Mathf.Min(760.0f, Screen.width - 40.0f);
		float height = Mathf.Min(260.0f, Screen.height - 40.0f);
		Rect rect = new((Screen.width - width) * 0.5f, (Screen.height - height) * 0.5f, width, height);
		string reason = mStoppedByTimeBudget ? "\n已达到测试时间上限" : string.Empty;
		string progress = "\n完成场景: " + mCompletedScenarioCount + "/" + mTotalScenarioCount +
			"    完成用例: " + mCompletedCaseCount + "/" + mTotalCaseCount;
		string elapsed = "\n总耗时: " + mBenchmarkElapsedSeconds.ToString("F1", CultureInfo.InvariantCulture) + " 秒";
		GUI.Box(rect, "测试结束" + reason + progress + elapsed, mFinishedOverlayStyle);
	}

	private int getEffectiveWarmupFrames()
	{
		if (mSuite == BenchmarkSuite.Core && mUseFiveMinuteCorePreset)
		{
			return Mathf.Max(1, Mathf.Min(mWarmupFrames, mCoreWarmupFrames));
		}
		return Mathf.Max(1, mWarmupFrames);
	}

	private int getEffectiveSampleFrames()
	{
		if (mSuite == BenchmarkSuite.Core && mUseFiveMinuteCorePreset)
		{
			return Mathf.Max(10, Mathf.Min(mSampleFrames, mCoreSampleFrames));
		}
		return Mathf.Max(10, mSampleFrames);
	}

	private bool isTimeBudgetExpired()
	{
		return mEnforceTotalTimeBudget && Stopwatch.GetTimestamp() >= mBenchmarkDeadlineTimestamp;
	}

	private double getBenchmarkElapsedSeconds()
	{
		if (mBenchmarkStartTimestamp <= 0) return 0.0;
		return (Stopwatch.GetTimestamp() - mBenchmarkStartTimestamp) / (double)Stopwatch.Frequency;
	}

	private IEnumerator Start()
	{
		if (mRunOnStart)
		{
			yield return runAll();
		}
	}

	[ContextMenu("Run Benchmark")]
	public void runFromContextMenu()
	{
		if (Application.isPlaying)
		{
			StartCoroutine(runAll());
		}
		else
		{
			Debug.LogWarning("FastSpriteRendererBenchmark must run in Play Mode.");
		}
	}

	public IEnumerator runAll()
	{
		if (mBenchmarkRunning) yield break;

		mResults.Clear();
		mBenchmarkRunning = true;
		mBenchmarkFinished = false;
		mStoppedByTimeBudget = false;
		mStopRequested = false;
		mCompletedScenarioCount = 0;
		mCompletedCaseCount = 0;
		mLastReportPath = null;
		mBenchmarkStartTimestamp = Stopwatch.GetTimestamp();
		double budgetSeconds = Math.Max(60.0, Math.Min(290.0, mMaxTotalDurationSeconds));
		mBenchmarkDeadlineTimestamp = mBenchmarkStartTimestamp + (long)(budgetSeconds * Stopwatch.Frequency);

		prepareAssets();
		List<Scenario> plan = buildPlan();
		mTotalScenarioCount = plan.Count;
		mTotalCaseCount = 0;
		for (int i = 0; i < plan.Count; ++i) mTotalCaseCount += plan[i].mCases.Count;

		int effectiveWarmupFrames = getEffectiveWarmupFrames();
		int effectiveSampleFrames = getEffectiveSampleFrames();
		Debug.Log("[FastSprite Benchmark] Suite=" + mSuite + " | Scenarios=" + plan.Count + " | Cases=" + mTotalCaseCount +
			" | ABBA=" + mUseSandwichOrder + " | Warmup=" + effectiveWarmupFrames + " | Samples=" + effectiveSampleFrames +
			" | Budget=" + (mEnforceTotalTimeBudget ? budgetSeconds.ToString("F0", CultureInfo.InvariantCulture) + "s" : "Unlimited"));

		int oldVSync = QualitySettings.vSyncCount;
		int oldTargetFrameRate = Application.targetFrameRate;
		if (mDisableVSync)
		{
			QualitySettings.vSyncCount = 0;
			Application.targetFrameRate = Mathf.Max(1, mUnlockedTargetFrameRate);
		}

		startRecorders();
		try
		{
			for (int scenarioIndex = 0; scenarioIndex < plan.Count && !mStopRequested; ++scenarioIndex)
			{
				if (isTimeBudgetExpired())
				{
					mStoppedByTimeBudget = true;
					mStopRequested = true;
					break;
				}

				Scenario scenario = plan[scenarioIndex];
				Debug.Log("[FastSprite Benchmark] Scenario " + (scenarioIndex + 1) + "/" + plan.Count +
					" | ID=" + scenario.mID + " | Profile=" + scenario.mProfile + " | Count=" + scenario.mCount +
					" | Variants=" + scenario.mVariantCount + " | Sort=" + scenario.mSortMode +
					" | Cases=" + scenario.mCases.Count);

				PairScene scene = createPairScene(scenario);
				try
				{
					for (int caseIndex = 0; caseIndex < scenario.mCases.Count && !mStopRequested; ++caseIndex)
					{
						if (isTimeBudgetExpired())
						{
							mStoppedByTimeBudget = true;
							mStopRequested = true;
							break;
						}

						BenchmarkCase benchmarkCase = scenario.mCases[caseIndex];
						Target[] order = mUseSandwichOrder
							? new[] { Target.Fast, Target.Native, Target.Native, Target.Fast }
							: new[] { Target.Fast, Target.Native };

						bool completedCase = true;
						for (int segment = 0; segment < order.Length; ++segment)
						{
							if (isTimeBudgetExpired())
							{
								mStoppedByTimeBudget = true;
								mStopRequested = true;
								completedCase = false;
								break;
							}
							yield return runSegment(scene, order[segment], benchmarkCase, segment);
							if (mStopRequested)
							{
								completedCase = false;
								break;
							}
						}

						if (completedCase)
						{
							++mCompletedCaseCount;
							logCaseSummary(scenario, benchmarkCase);
						}
					}
				}
				finally
				{
					disposePairScene(scene);
				}

				if (!mStopRequested) ++mCompletedScenarioCount;
				yield return null;
				if (mCollectGarbageBetweenScenarios && !mStopRequested)
				{
					GC.Collect();
				}
			}
		}
		finally
		{
			stopRecorders();
			QualitySettings.vSyncCount = oldVSync;
			Application.targetFrameRate = oldTargetFrameRate;
			disposeOwnedAssets();
		}

		mBenchmarkElapsedSeconds = getBenchmarkElapsedSeconds();
		string report = buildReport();
		string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
		string path = Path.Combine(Application.persistentDataPath, "FastSpriteRendererBenchmark_V5_" + timestamp + ".csv");
		File.WriteAllText(path, report, Encoding.UTF8);
		mBenchmarkElapsedSeconds = getBenchmarkElapsedSeconds();
		string metaPath = Path.Combine(Application.persistentDataPath, "FastSpriteRendererBenchmark_V5_" + timestamp + "_meta.txt");
		File.WriteAllText(metaPath, buildMetadata(plan), Encoding.UTF8);
		mBenchmarkElapsedSeconds = getBenchmarkElapsedSeconds();
		mLastReportPath = path;
		Debug.Log("[FastSprite Benchmark] Report=" + path);
		Debug.Log("[FastSprite Benchmark] Metadata=" + metaPath);
		Debug.Log("[FastSprite Benchmark] ===== TEST FINISHED ===== | Elapsed=" +
			mBenchmarkElapsedSeconds.ToString("F1", CultureInfo.InvariantCulture) + "s | Cases=" + mCompletedCaseCount + "/" + mTotalCaseCount +
			(mStoppedByTimeBudget ? " | TimeBudgetReached=True" : string.Empty));

		mBenchmarkRunning = false;
		mBenchmarkFinished = true;
	}

	private IEnumerator runSegment(PairScene scene, Target target, BenchmarkCase benchmarkCase, int segment)
	{
		prepareTarget(scene, target);
		prepareCaseBaseline(scene, target, benchmarkCase);
		mMutationStep = 0;

		for (int i = 0; i < getEffectiveWarmupFrames(); ++i)
		{
			if (isTimeBudgetExpired())
			{
				mStoppedByTimeBudget = true;
				mStopRequested = true;
				yield break;
			}
			applyMutation(scene, target, benchmarkCase);
			yield return null;
		}

		int sampleFrames = getEffectiveSampleFrames();
		double[] wall = new double[sampleFrames];
		double mutationMS = 0.0;
		double drawCalls = 0.0;
		double batches = 0.0;
		double setPass = 0.0;
		double vertices = 0.0;
		double triangles = 0.0;
		double vbUpload = 0.0;
		double ibUpload = 0.0;
		double gcAlloc = 0.0;
		double cpuFrame = 0.0;
		double mainThread = 0.0;
		double renderThread = 0.0;
		double gpuFrame = 0.0;
		int cpuTimingFrames = 0;
		int gpuTimingFrames = 0;
		double fastInternalVB = 0.0;
		double fastInternalIB = 0.0;
		double fastSystemTotalMS = 0.0;
		double fastRendererScanMS = 0.0;
		double fastMembershipMS = 0.0;
		double fastBatchUpdateMS = 0.0;
		double fastTopologyMS = 0.0;
		double fastElementScanMS = 0.0;
		double fastGeometryBuildMS = 0.0;
		double fastVertexUploadMS = 0.0;
		double fastSortIndexMS = 0.0;
		double fastBoundsMS = 0.0;
		double fastTransformChangedCount = 0.0;
		double fastMembershipCheckCount = 0.0;
		double fastMembershipChangedCount = 0.0;
		double fastTopologyRebuildCount = 0.0;
		double fastDirtyElementCount = 0.0;
		double fastVertexUploadCallCount = 0.0;
		double fastIndexUploadCallCount = 0.0;
		int gc0Start = GC.CollectionCount(0);
		int actualSampleFrames = 0;

		for (int frame = 0; frame < sampleFrames; ++frame)
		{
			if (isTimeBudgetExpired())
			{
				mStoppedByTimeBudget = true;
				mStopRequested = true;
				break;
			}
			long frameStart = Stopwatch.GetTimestamp();
			long mutationStart = Stopwatch.GetTimestamp();
			applyMutation(scene, target, benchmarkCase);
			mutationMS += elapsedMS(mutationStart);

			FrameTimingManager.CaptureFrameTimings();
			yield return null;

			wall[frame] = elapsedMS(frameStart);
			++actualSampleFrames;
			if (mDrawCalls.Valid) drawCalls += mDrawCalls.LastValue;
			if (mBatches.Valid) batches += mBatches.LastValue;
			if (mSetPass.Valid) setPass += mSetPass.LastValue;
			if (mVertices.Valid) vertices += mVertices.LastValue;
			if (mTriangles.Valid) triangles += mTriangles.LastValue;
			if (mVBUpload.Valid) vbUpload += mVBUpload.LastValue;
			if (mIBUpload.Valid) ibUpload += mIBUpload.LastValue;
			if (mGCAlloc.Valid) gcAlloc += mGCAlloc.LastValue;

			if (mFrameTimingEnabled && FrameTimingManager.GetLatestTimings(1, mFrameTimings) > 0)
			{
				FrameTiming timing = mFrameTimings[0];
				if (timing.cpuFrameTime > 0.0 && timing.cpuFrameTime < 1000.0)
				{
					cpuFrame += timing.cpuFrameTime;
					mainThread += timing.cpuMainThreadFrameTime;
					renderThread += timing.cpuRenderThreadFrameTime;
					++cpuTimingFrames;
				}
				if (timing.gpuFrameTime > 0.0 && timing.gpuFrameTime < 1000.0)
				{
					gpuFrame += timing.gpuFrameTime;
					++gpuTimingFrames;
				}
			}

			if (target == Target.Fast && scene.mFastSystem != null)
			{
				fastInternalVB += scene.mFastSystem.getVertexUploadBytes();
				fastInternalIB += scene.mFastSystem.getIndexUploadBytes();
				if (mCollectFastInternalProfile)
				{
					FastSpriteFrameProfile profile = scene.mFastSystem.getLastProfile();
					fastSystemTotalMS += profile.mTotalMS;
					fastRendererScanMS += profile.mRendererScanMS;
					fastMembershipMS += profile.mMembershipMS;
					fastBatchUpdateMS += profile.mBatchUpdateMS;
					fastTopologyMS += profile.mTopologyMS;
					fastElementScanMS += profile.mElementScanMS;
					fastGeometryBuildMS += profile.mGeometryBuildMS;
					fastVertexUploadMS += profile.mVertexUploadMS;
					fastSortIndexMS += profile.mSortIndexMS;
					fastBoundsMS += profile.mBoundsMS;
					fastTransformChangedCount += profile.mTransformChangedCount;
					fastMembershipCheckCount += profile.mMembershipCheckCount;
					fastMembershipChangedCount += profile.mMembershipChangedCount;
					fastTopologyRebuildCount += profile.mTopologyRebuildCount;
					fastDirtyElementCount += profile.mDirtyElementCount;
					fastVertexUploadCallCount += profile.mVertexUploadCallCount;
					fastIndexUploadCallCount += profile.mIndexUploadCallCount;
				}
			}
		}

		// A time-budget stop can happen in the middle of a segment. Discard that partial segment
		// instead of mixing an incomplete Fast/Native sample into the final comparison.
		if (mStopRequested || actualSampleFrames <= 0) yield break;
		double[] measuredWall;
		if (actualSampleFrames == wall.Length)
		{
			measuredWall = wall;
		}
		else
		{
			measuredWall = new double[actualSampleFrames];
			Array.Copy(wall, measuredWall, actualSampleFrames);
		}
		Array.Sort(measuredWall);
		double mean = 0.0;
		for (int i = 0; i < measuredWall.Length; ++i) mean += measuredWall[i];
		mean /= measuredWall.Length;

		Scenario scenario = scene.mScenario;
		mResults.Add(new Result
		{
			mScenarioID = scenario.mID,
			mSegment = segment,
			mTarget = target,
			mProfile = scenario.mProfile,
			mCase = benchmarkCase,
			mSortMode = scenario.mSortMode,
			mCount = scenario.mCount,
			mVariantCount = scenario.mVariantCount,
			mVisibleFraction = scenario.getVisibleFraction(),
			mFrames = actualSampleFrames,
			mWallMean = mean,
			mWallMedian = percentileSorted(measuredWall, 0.50),
			mWallP95 = percentileSorted(measuredWall, 0.95),
			mWallP99 = percentileSorted(measuredWall, 0.99),
			mMutationMean = mutationMS / actualSampleFrames,
			mDrawCalls = drawCalls / actualSampleFrames,
			mBatches = batches / actualSampleFrames,
			mSetPass = setPass / actualSampleFrames,
			mVertices = vertices / actualSampleFrames,
			mTriangles = triangles / actualSampleFrames,
			mVBUpload = vbUpload / actualSampleFrames,
			mIBUpload = ibUpload / actualSampleFrames,
			mGCAlloc = gcAlloc / actualSampleFrames,
			mGCCollections0 = GC.CollectionCount(0) - gc0Start,
			mCPUFrame = cpuTimingFrames > 0 ? cpuFrame / cpuTimingFrames : -1.0,
			mMainThread = cpuTimingFrames > 0 ? mainThread / cpuTimingFrames : -1.0,
			mRenderThread = cpuTimingFrames > 0 ? renderThread / cpuTimingFrames : -1.0,
			mGPUFrame = gpuTimingFrames > 0 ? gpuFrame / gpuTimingFrames : -1.0,
			mFastBatchCount = target == Target.Fast && scene.mFastSystem != null ? scene.mFastSystem.getBatchCount() : 0,
			mFastVertexCount = target == Target.Fast && scene.mFastSystem != null ? scene.mFastSystem.getVertexCount() : 0,
			mFastIndexCount = target == Target.Fast && scene.mFastSystem != null ? scene.mFastSystem.getIndexCount() : 0,
			mFastInternalVBUpload = target == Target.Fast ? fastInternalVB / actualSampleFrames : 0.0,
			mFastInternalIBUpload = target == Target.Fast ? fastInternalIB / actualSampleFrames : 0.0,
			mFastSystemTotalMS = target == Target.Fast ? fastSystemTotalMS / actualSampleFrames : 0.0,
			mFastRendererScanMS = target == Target.Fast ? fastRendererScanMS / actualSampleFrames : 0.0,
			mFastMembershipMS = target == Target.Fast ? fastMembershipMS / actualSampleFrames : 0.0,
			mFastBatchUpdateMS = target == Target.Fast ? fastBatchUpdateMS / actualSampleFrames : 0.0,
			mFastTopologyMS = target == Target.Fast ? fastTopologyMS / actualSampleFrames : 0.0,
			mFastElementScanMS = target == Target.Fast ? fastElementScanMS / actualSampleFrames : 0.0,
			mFastGeometryBuildMS = target == Target.Fast ? fastGeometryBuildMS / actualSampleFrames : 0.0,
			mFastVertexUploadMS = target == Target.Fast ? fastVertexUploadMS / actualSampleFrames : 0.0,
			mFastSortIndexMS = target == Target.Fast ? fastSortIndexMS / actualSampleFrames : 0.0,
			mFastBoundsMS = target == Target.Fast ? fastBoundsMS / actualSampleFrames : 0.0,
			mFastTransformChangedCount = target == Target.Fast ? fastTransformChangedCount / actualSampleFrames : 0.0,
			mFastMembershipCheckCount = target == Target.Fast ? fastMembershipCheckCount / actualSampleFrames : 0.0,
			mFastMembershipChangedCount = target == Target.Fast ? fastMembershipChangedCount / actualSampleFrames : 0.0,
			mFastTopologyRebuildCount = target == Target.Fast ? fastTopologyRebuildCount / actualSampleFrames : 0.0,
			mFastDirtyElementCount = target == Target.Fast ? fastDirtyElementCount / actualSampleFrames : 0.0,
			mFastVertexUploadCallCount = target == Target.Fast ? fastVertexUploadCallCount / actualSampleFrames : 0.0,
			mFastIndexUploadCallCount = target == Target.Fast ? fastIndexUploadCallCount / actualSampleFrames : 0.0,
		});
	}

	private List<Scenario> buildPlan()
	{
		List<Scenario> plan = new();
		if (mSuite == BenchmarkSuite.Custom)
		{
			int id = 1;
			for (int p = 0; p < mCustomProfiles.Length; ++p)
			{
				for (int c = 0; c < mCustomCounts.Length; ++c)
				{
					Scenario scenario = new()
					{
						mID = id++,
						mProfile = mCustomProfiles[p],
						mCount = Mathf.Max(0, mCustomCounts[c]),
						mVariantCount = Mathf.Max(1, mCustomVariantCount),
						mSortMode = mCustomSortMode,
					};
					for (int i = 0; i < mCustomCases.Length; ++i)
					{
						addCaseIfValid(scenario, mCustomCases[i]);
					}
					if (scenario.mCases.Count > 0) plan.Add(scenario);
				}
			}
			return plan;
		}

		if (mSuite == BenchmarkSuite.Smoke)
		{
			addScenario(plan, SceneProfile.FlatSameBatch, 1, 1, FastSpriteSortMode.CameraDistance,
				BenchmarkCase.Static, BenchmarkCase.MoveAll, BenchmarkCase.ColorAll, BenchmarkCase.SpriteSwapAll);
			addScenario(plan, SceneProfile.FlatSameBatch, 1000, 1, FastSpriteSortMode.CameraDistance,
				BenchmarkCase.Static, BenchmarkCase.Move10Percent, BenchmarkCase.MoveAll, BenchmarkCase.ColorAll, BenchmarkCase.SpriteSwapAll);
			addScenario(plan, SceneProfile.Grouped3Level, 1000, 1, FastSpriteSortMode.CameraDistance,
				BenchmarkCase.Static, BenchmarkCase.ParentMoveAll);
			addScenario(plan, SceneProfile.MultiSortingOrder, 1000, 8, FastSpriteSortMode.CameraDistance,
				BenchmarkCase.Static, BenchmarkCase.SortingOrder10Percent);
			addScenario(plan, SceneProfile.Visibility10Percent, 5000, 1, FastSpriteSortMode.CameraDistance,
				BenchmarkCase.Static);
			return plan;
		}

		// Core: enough coverage for every optimization iteration without exploding runtime.
		int[] coreCounts = { 0, 1, 10, 100, 1000, 5000, 10000 };
		for (int i = 0; i < coreCounts.Length; ++i)
		{
			addScenario(plan, SceneProfile.FlatSameBatch, coreCounts[i], 1, FastSpriteSortMode.CameraDistance,
				BenchmarkCase.Static,
				BenchmarkCase.Move10Percent,
				BenchmarkCase.MoveAll,
				BenchmarkCase.SpriteSwap10Percent,
				BenchmarkCase.ColorAll);
		}
		addScenario(plan, SceneProfile.Grouped3Level, 3000, 1, FastSpriteSortMode.CameraDistance,
			BenchmarkCase.Static, BenchmarkCase.MoveAll, BenchmarkCase.ParentMoveAll, BenchmarkCase.ParentRotateAll, BenchmarkCase.RootMoveAll);
		addScenario(plan, SceneProfile.MultiSortingOrder, 5000, 8, FastSpriteSortMode.CameraDistance,
			BenchmarkCase.Static, BenchmarkCase.MoveAll, BenchmarkCase.SortingOrder10Percent);
		addScenario(plan, SceneProfile.MultiSortingOrder, 5000, 32, FastSpriteSortMode.CameraDistance,
			BenchmarkCase.Static, BenchmarkCase.SortingOrder10Percent);
		addScenario(plan, SceneProfile.MultiTexture, 5000, 4, FastSpriteSortMode.CameraDistance,
			BenchmarkCase.Static, BenchmarkCase.MoveAll, BenchmarkCase.TextureSwap10Percent);
		addScenario(plan, SceneProfile.MultiMaterial, 5000, 4, FastSpriteSortMode.CameraDistance,
			BenchmarkCase.Static, BenchmarkCase.MoveAll, BenchmarkCase.MaterialSwap10Percent);
		addScenario(plan, SceneProfile.Visibility50Percent, 10000, 1, FastSpriteSortMode.CameraDistance,
			BenchmarkCase.Static, BenchmarkCase.Move10Percent);
		addScenario(plan, SceneProfile.Visibility10Percent, 10000, 1, FastSpriteSortMode.CameraDistance,
			BenchmarkCase.Static, BenchmarkCase.Move10Percent);
		addScenario(plan, SceneProfile.Visibility1Percent, 10000, 1, FastSpriteSortMode.CameraDistance,
			BenchmarkCase.Static);
		addScenario(plan, SceneProfile.TightMesh, 3000, 1, FastSpriteSortMode.CameraDistance,
			BenchmarkCase.Static, BenchmarkCase.MoveAll, BenchmarkCase.SpriteSwapAll);
		addScenario(plan, SceneProfile.MixedDrawModes, 1000, 1, FastSpriteSortMode.CameraDistance,
			BenchmarkCase.Static, BenchmarkCase.MoveAll, BenchmarkCase.ColorAll);
		addScenario(plan, SceneProfile.FlatSameBatch, 1000, 1, FastSpriteSortMode.CameraDistance,
			BenchmarkCase.SlicedResizeAll, BenchmarkCase.TiledResizeAll,
			BenchmarkCase.EnableDisable10Percent, BenchmarkCase.SortingOrder10Percent, BenchmarkCase.MaterialSwap10Percent);
		addScenario(plan, SceneProfile.FullOverlap, 3000, 1, FastSpriteSortMode.CameraDistance,
			BenchmarkCase.Static);

		if (mSuite == BenchmarkSuite.Core)
		{
			return plan;
		}

		// Full adds boundary counts, hierarchy depth, batch scaling, churn, sorting modes and geometry stress.
		int[] extraCounts = { 500, 3000 };
		for (int i = 0; i < extraCounts.Length; ++i)
		{
			addScenario(plan, SceneProfile.FlatSameBatch, extraCounts[i], 1, FastSpriteSortMode.CameraDistance,
				BenchmarkCase.Static,
				BenchmarkCase.MoveOne,
				BenchmarkCase.Move10Percent,
				BenchmarkCase.MoveAll,
				BenchmarkCase.Rotate10Percent,
				BenchmarkCase.RotateAll,
				BenchmarkCase.Scale10Percent,
				BenchmarkCase.ScaleAll,
				BenchmarkCase.SpriteSwapAll,
				BenchmarkCase.Color10Percent,
				BenchmarkCase.FlipAll);
		}

		addScenario(plan, SceneProfile.Grouped2Level, 3000, 1, FastSpriteSortMode.CameraDistance,
			BenchmarkCase.Static, BenchmarkCase.MoveAll, BenchmarkCase.ParentMoveAll, BenchmarkCase.ParentRotateAll, BenchmarkCase.RootMoveAll);
		addScenario(plan, SceneProfile.DeepGroupedHierarchy, 3000, 1, FastSpriteSortMode.CameraDistance,
			BenchmarkCase.Static, BenchmarkCase.MoveAll, BenchmarkCase.ParentMoveAll, BenchmarkCase.ParentRotateAll, BenchmarkCase.RootMoveAll);

		int[] batchVariants = { 2, 4, 16, 64 };
		for (int i = 0; i < batchVariants.Length; ++i)
		{
			addScenario(plan, SceneProfile.MultiSortingOrder, 5000, batchVariants[i], FastSpriteSortMode.CameraDistance,
				BenchmarkCase.Static, BenchmarkCase.Move10Percent, BenchmarkCase.MoveAll, BenchmarkCase.SortingOrder10Percent, BenchmarkCase.SortingOrderAll);
		}
		int[] resourceVariants = { 2, 8, 16 };
		for (int i = 0; i < resourceVariants.Length; ++i)
		{
			addScenario(plan, SceneProfile.MultiTexture, 5000, resourceVariants[i], FastSpriteSortMode.CameraDistance,
				BenchmarkCase.Static, BenchmarkCase.MoveAll, BenchmarkCase.TextureSwap10Percent, BenchmarkCase.TextureSwapAll);
			addScenario(plan, SceneProfile.MultiMaterial, 5000, resourceVariants[i], FastSpriteSortMode.CameraDistance,
				BenchmarkCase.Static, BenchmarkCase.MoveAll, BenchmarkCase.MaterialSwap10Percent, BenchmarkCase.MaterialSwapAll);
		}

		addScenario(plan, SceneProfile.FlatSameBatch, 1000, 1, FastSpriteSortMode.CameraDistance,
			BenchmarkCase.EnableDisable1Percent,
			BenchmarkCase.EnableDisable10Percent,
			BenchmarkCase.EnableDisableAll,
			BenchmarkCase.SortingOrderAll,
			BenchmarkCase.MaterialSwapAll,
			BenchmarkCase.TextureSwapAll,
			BenchmarkCase.SlicedResize10Percent,
			BenchmarkCase.SlicedResizeAll,
			BenchmarkCase.TiledResize10Percent,
			BenchmarkCase.TiledResizeAll);
		addScenario(plan, SceneProfile.FlatSameBatch, 5000, 1, FastSpriteSortMode.CameraDistance,
			BenchmarkCase.EnableDisable1Percent, BenchmarkCase.EnableDisable10Percent,
			BenchmarkCase.SortingOrder10Percent, BenchmarkCase.MaterialSwap10Percent, BenchmarkCase.TextureSwap10Percent);

		addScenario(plan, SceneProfile.TightMesh, 1, 1, FastSpriteSortMode.CameraDistance,
			BenchmarkCase.Static, BenchmarkCase.MoveAll, BenchmarkCase.SpriteSwapAll);
		addScenario(plan, SceneProfile.TightMesh, 5000, 1, FastSpriteSortMode.CameraDistance,
			BenchmarkCase.Static, BenchmarkCase.Move10Percent, BenchmarkCase.MoveAll, BenchmarkCase.SpriteSwapAll, BenchmarkCase.ColorAll);
		addScenario(plan, SceneProfile.MixedDrawModes, 3000, 1, FastSpriteSortMode.CameraDistance,
			BenchmarkCase.Static, BenchmarkCase.Move10Percent, BenchmarkCase.MoveAll, BenchmarkCase.ColorAll);
		addScenario(plan, SceneProfile.FullOverlap, 1000, 1, FastSpriteSortMode.CameraDistance, BenchmarkCase.Static);
		addScenario(plan, SceneProfile.FullOverlap, 5000, 1, FastSpriteSortMode.CameraDistance, BenchmarkCase.Static);

		addScenario(plan, SceneProfile.FlatSameBatch, 5000, 1, FastSpriteSortMode.Registration,
			BenchmarkCase.Static, BenchmarkCase.Move10Percent, BenchmarkCase.MoveAll);
		addScenario(plan, SceneProfile.FlatSameBatch, 5000, 1, FastSpriteSortMode.YAxis,
			BenchmarkCase.Static, BenchmarkCase.Move10Percent, BenchmarkCase.MoveAll);
		addScenario(plan, SceneProfile.FlatSameBatch, 5000, 1, FastSpriteSortMode.CameraDistance,
			BenchmarkCase.CameraMove);

		return plan;
	}

	private void addScenario(List<Scenario> plan, SceneProfile profile, int count, int variantCount, FastSpriteSortMode sortMode, params BenchmarkCase[] cases)
	{
		// Merge identical scene configurations so expensive GameObject creation is paid once.
		for (int i = 0; i < plan.Count; ++i)
		{
			Scenario existing = plan[i];
			if (existing.mProfile == profile && existing.mCount == count && existing.mVariantCount == variantCount && existing.mSortMode == sortMode)
			{
				for (int c = 0; c < cases.Length; ++c) addCaseIfValid(existing, cases[c]);
				return;
			}
		}

		Scenario scenario = new()
		{
			mID = plan.Count + 1,
			mProfile = profile,
			mCount = Mathf.Max(0, count),
			mVariantCount = Mathf.Max(1, variantCount),
			mSortMode = sortMode,
		};
		for (int i = 0; i < cases.Length; ++i) addCaseIfValid(scenario, cases[i]);
		if (scenario.mCases.Count > 0) plan.Add(scenario);
	}

	private static void addCaseIfValid(Scenario scenario, BenchmarkCase benchmarkCase)
	{
		if (scenario.mCount == 0 && benchmarkCase != BenchmarkCase.Static && benchmarkCase != BenchmarkCase.CameraMove)
		{
			return;
		}
		if ((benchmarkCase == BenchmarkCase.ParentMoveAll || benchmarkCase == BenchmarkCase.ParentRotateAll) &&
			scenario.mProfile != SceneProfile.Grouped2Level &&
			scenario.mProfile != SceneProfile.Grouped3Level &&
			scenario.mProfile != SceneProfile.DeepGroupedHierarchy)
		{
			return;
		}
		if (!scenario.mCases.Contains(benchmarkCase)) scenario.mCases.Add(benchmarkCase);
	}

	private PairScene createPairScene(Scenario scenario)
	{
		PairScene scene = new()
		{
			mScenario = scenario,
			mNative = new SpriteRenderer[scenario.mCount],
			mFast = new FastSpriteRenderer[scenario.mCount],
			mLocalPositions = new Vector3[scenario.mCount],
		};

		scene.mNativeRoot = new GameObject("SpriteRenderer_Benchmark_Native_" + scenario.mID);
		scene.mNativeRoot.SetActive(false);
		scene.mFastRoot = new GameObject("SpriteRenderer_Benchmark_Fast_" + scenario.mID);
		scene.mFastRoot.SetActive(false);

		GameObject systemGO = new("FastSpriteRenderSystem_Benchmark_" + scenario.mID);
		scene.mFastSystem = systemGO.AddComponent<FastSpriteRenderSystem>();
		scene.mFastSystem.setSortMode(scenario.mSortMode);
		scene.mFastSystem.setSortCamera(getCamera());
		scene.mFastSystem.setProfileStatsEnabled(mCollectFastInternalProfile);

		Transform[] nativeParents = buildHierarchy(scene.mNativeRoot.transform, scenario, "N");
		Transform[] fastParents = buildHierarchy(scene.mFastRoot.transform, scenario, "F");
		scene.mNativeMotionParents = nativeParents;
		scene.mFastMotionParents = fastParents;

		int visibleCount = getVisibleCount(scenario);
		for (int i = 0; i < scenario.mCount; ++i)
		{
			Vector3 localPosition = profilePosition(scenario, i, visibleCount);
			scene.mLocalPositions[i] = localPosition;
			Transform nativeParent = chooseParent(scene.mNativeRoot.transform, nativeParents, i);
			Transform fastParent = chooseParent(scene.mFastRoot.transform, fastParents, i);

			GameObject nativeGO = new("Native_" + i);
			nativeGO.transform.SetParent(nativeParent, false);
			nativeGO.transform.localPosition = localPosition;
			SpriteRenderer nativeRenderer = nativeGO.AddComponent<SpriteRenderer>();
			applyNativeProfileBaseline(nativeRenderer, scenario, i);
			scene.mNative[i] = nativeRenderer;

			GameObject fastGO = new("Fast_" + i);
			fastGO.transform.SetParent(fastParent, false);
			fastGO.transform.localPosition = localPosition;
			FastSpriteRenderer fastRenderer = fastGO.AddComponent<FastSpriteRenderer>();
			applyFastProfileBaseline(fastRenderer, scenario, i);
			scene.mFast[i] = fastRenderer;
		}

		fitCameraForScenario(scenario, visibleCount);
		Camera camera = getCamera();
		if (camera != null)
		{
			scene.mCameraPosition = camera.transform.position;
			scene.mCameraRotation = camera.transform.rotation;
			scene.mCameraOrthographicSize = camera.orthographicSize;
		}
		return scene;
	}

	private Transform[] buildHierarchy(Transform root, Scenario scenario, string prefix)
	{
		if (scenario.mProfile != SceneProfile.Grouped2Level &&
			scenario.mProfile != SceneProfile.Grouped3Level &&
			scenario.mProfile != SceneProfile.DeepGroupedHierarchy)
		{
			return Array.Empty<Transform>();
		}

		int groupSize = Mathf.Max(1, mGroupSize);
		int groupCount = Mathf.CeilToInt(scenario.mCount / (float)groupSize);
		Transform[] leaves = new Transform[groupCount];
		for (int group = 0; group < groupCount; ++group)
		{
			Transform parent = root;
			if (scenario.mProfile == SceneProfile.Grouped2Level)
			{
				parent = createChild(parent, prefix + "_Group_" + group);
			}
			else if (scenario.mProfile == SceneProfile.Grouped3Level)
			{
				int section = group / 8;
				Transform sectionTransform = findOrCreateDirectChild(root, prefix + "_Section_" + section);
				parent = createChild(sectionTransform, prefix + "_Group_" + group);
			}
			else
			{
				for (int depth = 0; depth < Mathf.Max(2, mDeepHierarchyDepth); ++depth)
				{
					parent = createChild(parent, prefix + "_G" + group + "_D" + depth);
				}
			}
			leaves[group] = parent;
		}
		return leaves;
	}

	private static Transform createChild(Transform parent, string name)
	{
		GameObject go = new(name);
		go.transform.SetParent(parent, false);
		return go.transform;
	}

	private static Transform findOrCreateDirectChild(Transform parent, string name)
	{
		Transform existing = parent.Find(name);
		return existing != null ? existing : createChild(parent, name);
	}

	private Transform chooseParent(Transform root, Transform[] parents, int index)
	{
		if (parents == null || parents.Length == 0) return root;
		int group = Mathf.Clamp(index / Mathf.Max(1, mGroupSize), 0, parents.Length - 1);
		return parents[group];
	}

	private void prepareTarget(PairScene scene, Target target)
	{
		// Important fairness detail: the Fast system must not run during Native segments.
		if (target == Target.Native)
		{
			scene.mFastRoot.SetActive(false);
			if (scene.mFastSystem != null) scene.mFastSystem.enabled = false;
			scene.mNativeRoot.SetActive(true);
		}
		else
		{
			scene.mNativeRoot.SetActive(false);
			if (scene.mFastSystem != null) scene.mFastSystem.enabled = true;
			scene.mFastRoot.SetActive(true);
			if (scene.mFastSystem != null) scene.mFastSystem.flushNow();
		}
	}

	private void prepareCaseBaseline(PairScene scene, Target target, BenchmarkCase benchmarkCase)
	{
		resetCamera(scene);
		resetRootAndParents(scene, target);

		if (target == Target.Native)
		{
			for (int i = 0; i < scene.mNative.Length; ++i)
			{
				SpriteRenderer renderer = scene.mNative[i];
				renderer.enabled = true;
				renderer.transform.localPosition = scene.mLocalPositions[i];
				renderer.transform.localRotation = Quaternion.identity;
				renderer.transform.localScale = Vector3.one;
				applyNativeProfileBaseline(renderer, scene.mScenario, i);
				applyNativeCaseBaseline(renderer, benchmarkCase);
			}
			return;
		}

		for (int i = 0; i < scene.mFast.Length; ++i)
		{
			FastSpriteRenderer renderer = scene.mFast[i];
			renderer.enabled = true;
			renderer.transform.localPosition = scene.mLocalPositions[i];
			renderer.transform.localRotation = Quaternion.identity;
			renderer.transform.localScale = Vector3.one;
			applyFastProfileBaseline(renderer, scene.mScenario, i);
			applyFastCaseBaseline(renderer, benchmarkCase);
		}
		if (scene.mFastSystem != null) scene.mFastSystem.flushNow();
	}

	private void resetCamera(PairScene scene)
	{
		Camera camera = getCamera();
		if (camera == null) return;
		camera.transform.position = scene.mCameraPosition;
		camera.transform.rotation = scene.mCameraRotation;
		if (camera.orthographic) camera.orthographicSize = scene.mCameraOrthographicSize;
	}

	private static void resetRootAndParents(PairScene scene, Target target)
	{
		GameObject root = target == Target.Native ? scene.mNativeRoot : scene.mFastRoot;
		if (root != null)
		{
			root.transform.localPosition = Vector3.zero;
			root.transform.localRotation = Quaternion.identity;
			root.transform.localScale = Vector3.one;
		}
		Transform[] parents = target == Target.Native ? scene.mNativeMotionParents : scene.mFastMotionParents;
		if (parents == null) return;
		for (int i = 0; i < parents.Length; ++i)
		{
			if (parents[i] == null) continue;
			parents[i].localPosition = Vector3.zero;
			parents[i].localRotation = Quaternion.identity;
			parents[i].localScale = Vector3.one;
		}
	}

	private void applyNativeProfileBaseline(SpriteRenderer renderer, Scenario scenario, int index)
	{
		renderer.sprite = getBaseSprite(scenario, index);
		renderer.color = Color.white;
		renderer.flipX = false;
		renderer.flipY = false;
		renderer.sharedMaterial = getBaseMaterial(scenario, index);
		renderer.sortingLayerID = 0;
		renderer.sortingOrder = getBaseSortingOrder(scenario, index);
		renderer.drawMode = getBaseDrawMode(scenario, index);
		applyDrawModeSize(renderer, scenario, index);
	}

	private void applyFastProfileBaseline(FastSpriteRenderer renderer, Scenario scenario, int index)
	{
		renderer.setSprite(getBaseSprite(scenario, index));
		renderer.setColor(Color.white);
		renderer.setFlipX(false);
		renderer.setFlipY(false);
		renderer.setMaterial(getBaseMaterial(scenario, index));
		renderer.setSortingLayerID(0);
		renderer.setSortingOrder(getBaseSortingOrder(scenario, index));
		renderer.setDrawMode(getBaseDrawMode(scenario, index));
		applyDrawModeSize(renderer, scenario, index);
	}

	private void applyNativeCaseBaseline(SpriteRenderer renderer, BenchmarkCase benchmarkCase)
	{
		if (benchmarkCase == BenchmarkCase.SlicedResize10Percent || benchmarkCase == BenchmarkCase.SlicedResizeAll)
		{
			renderer.drawMode = SpriteDrawMode.Sliced;
			renderer.size = getResizeBaseSize(renderer.sprite);
		}
		else if (benchmarkCase == BenchmarkCase.TiledResize10Percent || benchmarkCase == BenchmarkCase.TiledResizeAll)
		{
			renderer.drawMode = SpriteDrawMode.Tiled;
			renderer.tileMode = SpriteTileMode.Continuous;
			renderer.size = getResizeBaseSize(renderer.sprite);
		}
	}

	private void applyFastCaseBaseline(FastSpriteRenderer renderer, BenchmarkCase benchmarkCase)
	{
		if (benchmarkCase == BenchmarkCase.SlicedResize10Percent || benchmarkCase == BenchmarkCase.SlicedResizeAll)
		{
			renderer.setDrawMode(SpriteDrawMode.Sliced);
			renderer.setSize(getResizeBaseSize(renderer.getSprite()));
		}
		else if (benchmarkCase == BenchmarkCase.TiledResize10Percent || benchmarkCase == BenchmarkCase.TiledResizeAll)
		{
			renderer.setDrawMode(SpriteDrawMode.Tiled);
			renderer.setTileMode(SpriteTileMode.Continuous);
			renderer.setSize(getResizeBaseSize(renderer.getSprite()));
		}
	}

	private void applyDrawModeSize(SpriteRenderer renderer, Scenario scenario, int index)
	{
		if (renderer.drawMode == SpriteDrawMode.Simple) return;
		renderer.size = getMixedSize(renderer.sprite, renderer.drawMode, index);
		if (renderer.drawMode == SpriteDrawMode.Tiled) renderer.tileMode = SpriteTileMode.Continuous;
	}

	private void applyDrawModeSize(FastSpriteRenderer renderer, Scenario scenario, int index)
	{
		if (renderer.getDrawMode() == SpriteDrawMode.Simple) return;
		renderer.setSize(getMixedSize(renderer.getSprite(), renderer.getDrawMode(), index));
		if (renderer.getDrawMode() == SpriteDrawMode.Tiled) renderer.setTileMode(SpriteTileMode.Continuous);
	}

	private static Vector2 getResizeBaseSize(Sprite sprite)
	{
		Vector2 nativeSize = sprite != null ? sprite.bounds.size : Vector2.one;
		return new Vector2(Mathf.Max(nativeSize.x * 1.6f, 0.1f), Mathf.Max(nativeSize.y * 1.6f, 0.1f));
	}

	private static Vector2 getMixedSize(Sprite sprite, SpriteDrawMode mode, int index)
	{
		Vector2 nativeSize = sprite != null ? sprite.bounds.size : Vector2.one;
		float scale = mode == SpriteDrawMode.Tiled ? 2.4f + (index % 3) * 0.35f : 1.8f;
		return new Vector2(Mathf.Max(nativeSize.x * scale, 0.1f), Mathf.Max(nativeSize.y * scale, 0.1f));
	}

	private Sprite getBaseSprite(Scenario scenario, int index)
	{
		if (scenario.mProfile == SceneProfile.TightMesh)
		{
			return mTightSpriteA != null ? mTightSpriteA : mSpriteA;
		}
		if (scenario.mProfile == SceneProfile.MultiTexture)
		{
			int variant = index % Mathf.Min(scenario.mVariantCount, mTextureVariantSprites.Length);
			return mTextureVariantSprites[variant];
		}
		return mSpriteA;
	}

	private Material getBaseMaterial(Scenario scenario, int index)
	{
		if (scenario.mProfile == SceneProfile.MultiMaterial)
		{
			int variant = index % Mathf.Min(scenario.mVariantCount, mMaterialVariants.Length);
			return mMaterialVariants[variant];
		}
		return mMaterialVariants[0];
	}

	private static int getBaseSortingOrder(Scenario scenario, int index)
	{
		return scenario.mProfile == SceneProfile.MultiSortingOrder ? index % Mathf.Max(1, scenario.mVariantCount) : 0;
	}

	private static SpriteDrawMode getBaseDrawMode(Scenario scenario, int index)
	{
		if (scenario.mProfile != SceneProfile.MixedDrawModes) return SpriteDrawMode.Simple;
		int mod = index % 10;
		if (mod < 7) return SpriteDrawMode.Simple;
		if (mod < 9) return SpriteDrawMode.Sliced;
		return SpriteDrawMode.Tiled;
	}

	private void applyMutation(PairScene scene, Target target, BenchmarkCase benchmarkCase)
	{
		bool alternate = (mMutationStep++ & 1) != 0;
		int count = scene.mScenario.mCount;
		if (benchmarkCase == BenchmarkCase.Static)
		{
			return;
		}
		if (benchmarkCase == BenchmarkCase.CameraMove)
		{
			Camera camera = getCamera();
			if (camera != null)
			{
				Vector3 p = scene.mCameraPosition;
				p.x += alternate ? 0.02f : -0.02f;
				camera.transform.position = p;
			}
			return;
		}
		if (benchmarkCase == BenchmarkCase.RootMoveAll)
		{
			Transform root = target == Target.Native ? scene.mNativeRoot.transform : scene.mFastRoot.transform;
			root.localPosition = alternate ? new Vector3(0.02f, -0.01f, 0.0f) : Vector3.zero;
			return;
		}
		if (benchmarkCase == BenchmarkCase.ParentMoveAll || benchmarkCase == BenchmarkCase.ParentRotateAll)
		{
			Transform[] parents = target == Target.Native ? scene.mNativeMotionParents : scene.mFastMotionParents;
			for (int i = 0; i < parents.Length; ++i)
			{
				if (benchmarkCase == BenchmarkCase.ParentMoveAll)
				{
					parents[i].localPosition = alternate ? new Vector3(0.02f, -0.01f, 0.0f) : Vector3.zero;
				}
				else
				{
					parents[i].localRotation = alternate ? Quaternion.Euler(0.0f, 0.0f, 1.5f) : Quaternion.identity;
				}
			}
			return;
		}

		int changed = getChangedCount(benchmarkCase, count);
		if (target == Target.Native)
		{
			for (int i = 0; i < changed; ++i)
			{
				int index = sparseIndex(i, count, changed);
				applyNativeMutation(scene, scene.mNative[index], index, benchmarkCase, alternate);
			}
			return;
		}

		for (int i = 0; i < changed; ++i)
		{
			int index = sparseIndex(i, count, changed);
			applyFastMutation(scene, scene.mFast[index], index, benchmarkCase, alternate);
		}
	}

	private static int getChangedCount(BenchmarkCase benchmarkCase, int count)
	{
		if (count <= 0) return 0;
		switch (benchmarkCase)
		{
			case BenchmarkCase.MoveOne:
				return 1;
			case BenchmarkCase.EnableDisable1Percent:
				return Mathf.Max(1, Mathf.CeilToInt(count * 0.01f));
			case BenchmarkCase.Move10Percent:
			case BenchmarkCase.Rotate10Percent:
			case BenchmarkCase.Scale10Percent:
			case BenchmarkCase.SpriteSwap10Percent:
			case BenchmarkCase.TextureSwap10Percent:
			case BenchmarkCase.Color10Percent:
			case BenchmarkCase.SortingOrder10Percent:
			case BenchmarkCase.MaterialSwap10Percent:
			case BenchmarkCase.EnableDisable10Percent:
			case BenchmarkCase.SlicedResize10Percent:
			case BenchmarkCase.TiledResize10Percent:
				return Mathf.Max(1, Mathf.CeilToInt(count * 0.10f));
			default:
				return count;
		}
	}

	private void applyNativeMutation(PairScene scene, SpriteRenderer renderer, int index, BenchmarkCase benchmarkCase, bool alternate)
	{
		switch (benchmarkCase)
		{
			case BenchmarkCase.MoveOne:
			case BenchmarkCase.Move10Percent:
			case BenchmarkCase.MoveAll:
			{
				Vector3 p = scene.mLocalPositions[index];
				p.x += alternate ? 0.015f : -0.015f;
				renderer.transform.localPosition = p;
				break;
			}
			case BenchmarkCase.Rotate10Percent:
			case BenchmarkCase.RotateAll:
				renderer.transform.localRotation = alternate ? Quaternion.Euler(0.0f, 0.0f, 3.0f) : Quaternion.identity;
				break;
			case BenchmarkCase.Scale10Percent:
			case BenchmarkCase.ScaleAll:
				renderer.transform.localScale = alternate ? new Vector3(1.03f, 0.97f, 1.0f) : Vector3.one;
				break;
			case BenchmarkCase.SpriteSwap10Percent:
			case BenchmarkCase.SpriteSwapAll:
				renderer.sprite = alternate ? getSwapSprite(scene.mScenario) : getBaseSprite(scene.mScenario, index);
				break;
			case BenchmarkCase.TextureSwap10Percent:
			case BenchmarkCase.TextureSwapAll:
				renderer.sprite = alternate ? getOtherTextureSprite(scene.mScenario, index) : getBaseSprite(scene.mScenario, index);
				break;
			case BenchmarkCase.Color10Percent:
			case BenchmarkCase.ColorAll:
				renderer.color = alternate ? new Color32(220, 240, 255, 255) : new Color32(255, 255, 255, 255);
				break;
			case BenchmarkCase.FlipAll:
				renderer.flipX = alternate;
				break;
			case BenchmarkCase.SortingOrder10Percent:
			case BenchmarkCase.SortingOrderAll:
				renderer.sortingOrder = alternate ? getBaseSortingOrder(scene.mScenario, index) + 1000 : getBaseSortingOrder(scene.mScenario, index);
				break;
			case BenchmarkCase.MaterialSwap10Percent:
			case BenchmarkCase.MaterialSwapAll:
				renderer.sharedMaterial = alternate ? getOtherMaterial(scene.mScenario, index) : getBaseMaterial(scene.mScenario, index);
				break;
			case BenchmarkCase.EnableDisable1Percent:
			case BenchmarkCase.EnableDisable10Percent:
			case BenchmarkCase.EnableDisableAll:
				renderer.enabled = !alternate;
				break;
			case BenchmarkCase.SlicedResize10Percent:
			case BenchmarkCase.SlicedResizeAll:
			case BenchmarkCase.TiledResize10Percent:
			case BenchmarkCase.TiledResizeAll:
			{
				Vector2 native = renderer.sprite != null ? renderer.sprite.bounds.size : Vector2.one;
				float scale = alternate ? 2.2f : 1.6f;
				renderer.size = new Vector2(Mathf.Max(native.x * scale, 0.1f), Mathf.Max(native.y * scale, 0.1f));
				break;
			}
		}
	}

	private void applyFastMutation(PairScene scene, FastSpriteRenderer renderer, int index, BenchmarkCase benchmarkCase, bool alternate)
	{
		switch (benchmarkCase)
		{
			case BenchmarkCase.MoveOne:
			case BenchmarkCase.Move10Percent:
			case BenchmarkCase.MoveAll:
			{
				Vector3 p = scene.mLocalPositions[index];
				p.x += alternate ? 0.015f : -0.015f;
				renderer.transform.localPosition = p;
				break;
			}
			case BenchmarkCase.Rotate10Percent:
			case BenchmarkCase.RotateAll:
				renderer.transform.localRotation = alternate ? Quaternion.Euler(0.0f, 0.0f, 3.0f) : Quaternion.identity;
				break;
			case BenchmarkCase.Scale10Percent:
			case BenchmarkCase.ScaleAll:
				renderer.transform.localScale = alternate ? new Vector3(1.03f, 0.97f, 1.0f) : Vector3.one;
				break;
			case BenchmarkCase.SpriteSwap10Percent:
			case BenchmarkCase.SpriteSwapAll:
				renderer.setSprite(alternate ? getSwapSprite(scene.mScenario) : getBaseSprite(scene.mScenario, index));
				break;
			case BenchmarkCase.TextureSwap10Percent:
			case BenchmarkCase.TextureSwapAll:
				renderer.setSprite(alternate ? getOtherTextureSprite(scene.mScenario, index) : getBaseSprite(scene.mScenario, index));
				break;
			case BenchmarkCase.Color10Percent:
			case BenchmarkCase.ColorAll:
				renderer.setColor(alternate ? new Color32(220, 240, 255, 255) : new Color32(255, 255, 255, 255));
				break;
			case BenchmarkCase.FlipAll:
				renderer.setFlipX(alternate);
				break;
			case BenchmarkCase.SortingOrder10Percent:
			case BenchmarkCase.SortingOrderAll:
				renderer.setSortingOrder(alternate ? getBaseSortingOrder(scene.mScenario, index) + 1000 : getBaseSortingOrder(scene.mScenario, index));
				break;
			case BenchmarkCase.MaterialSwap10Percent:
			case BenchmarkCase.MaterialSwapAll:
				renderer.setMaterial(alternate ? getOtherMaterial(scene.mScenario, index) : getBaseMaterial(scene.mScenario, index));
				break;
			case BenchmarkCase.EnableDisable1Percent:
			case BenchmarkCase.EnableDisable10Percent:
			case BenchmarkCase.EnableDisableAll:
				renderer.enabled = !alternate;
				break;
			case BenchmarkCase.SlicedResize10Percent:
			case BenchmarkCase.SlicedResizeAll:
			case BenchmarkCase.TiledResize10Percent:
			case BenchmarkCase.TiledResizeAll:
			{
				Sprite sprite = renderer.getSprite();
				Vector2 native = sprite != null ? sprite.bounds.size : Vector2.one;
				float scale = alternate ? 2.2f : 1.6f;
				renderer.setSize(new Vector2(Mathf.Max(native.x * scale, 0.1f), Mathf.Max(native.y * scale, 0.1f)));
				break;
			}
		}
	}

	private Sprite getSwapSprite(Scenario scenario)
	{
		if (scenario.mProfile == SceneProfile.TightMesh)
		{
			return mTightSpriteB != null ? mTightSpriteB : mSpriteB;
		}
		return mSpriteB;
	}

	private Sprite getOtherTextureSprite(Scenario scenario, int index)
	{
		int variantCount = Mathf.Min(Mathf.Max(2, scenario.mVariantCount), mTextureVariantSprites.Length);
		int baseVariant = scenario.mProfile == SceneProfile.MultiTexture ? index % variantCount : 0;
		return mTextureVariantSprites[(baseVariant + 1) % variantCount];
	}

	private Material getOtherMaterial(Scenario scenario, int index)
	{
		int variantCount = Mathf.Min(Mathf.Max(2, scenario.mVariantCount), mMaterialVariants.Length);
		int baseVariant = scenario.mProfile == SceneProfile.MultiMaterial ? index % variantCount : 0;
		return mMaterialVariants[(baseVariant + 1) % variantCount];
	}

	private Vector3 profilePosition(Scenario scenario, int index, int visibleCount)
	{
		if (scenario.mProfile == SceneProfile.FullOverlap)
		{
			return new Vector3((index % 7) * 0.002f, -(index % 11) * 0.002f, (index % 3) * 0.0001f);
		}
		if (scenario.getVisibleFraction() < 0.999f && index >= visibleCount)
		{
			Vector3 p = gridPosition(index - visibleCount, Mathf.Max(1, scenario.mCount - visibleCount));
			p.x += 10000.0f;
			p.y += (index & 1) == 0 ? 10000.0f : -10000.0f;
			return p;
		}
		return gridPosition(index, Mathf.Max(1, visibleCount));
	}

	private static int getVisibleCount(Scenario scenario)
	{
		if (scenario.mCount <= 0) return 0;
		return Mathf.Clamp(Mathf.CeilToInt(scenario.mCount * scenario.getVisibleFraction()), 1, scenario.mCount);
	}

	private void fitCameraForScenario(Scenario scenario, int visibleCount)
	{
		Camera camera = getCamera();
		if (camera == null || !camera.orthographic) return;
		if (scenario.mProfile == SceneProfile.FullOverlap)
		{
			camera.orthographicSize = 2.0f;
			camera.transform.position = new Vector3(0.0f, 0.0f, -10.0f);
			camera.transform.rotation = Quaternion.identity;
			return;
		}
		fitCameraToGrid(Mathf.Max(1, visibleCount));
	}

	private void logCaseSummary(Scenario scenario, BenchmarkCase benchmarkCase)
	{
		List<Result> fast = getResults(Target.Fast, scenario.mID, benchmarkCase);
		List<Result> native = getResults(Target.Native, scenario.mID, benchmarkCase);
		if (fast.Count == 0 || native.Count == 0) return;
		double fastMedian = average(fast, r => r.mWallMedian);
		double nativeMedian = average(native, r => r.mWallMedian);
		double ratio = fastMedian > 0.0 ? nativeMedian / fastMedian : 0.0;
		Debug.Log("[FastSprite Compare] Scenario=" + scenario.mID + " | Profile=" + scenario.mProfile +
			" | Count=" + scenario.mCount + " | Case=" + benchmarkCase +
			" | WallMedian Fast/Native=" + format(fastMedian) + "/" + format(nativeMedian) + "ms" +
			" | Native/Fast=" + ratio.ToString("F2", CultureInfo.InvariantCulture) + "x" +
			" | DrawCalls Fast/Native=" + format(average(fast, r => r.mDrawCalls)) + "/" + format(average(native, r => r.mDrawCalls)) +
			" | FastBatch=" + average(fast, r => r.mFastBatchCount).ToString("F1", CultureInfo.InvariantCulture) +
			" | FastSystem=" + format(average(fast, r => r.mFastSystemTotalMS)) + "ms" +
			" [Scan=" + format(average(fast, r => r.mFastRendererScanMS)) +
			", Membership=" + format(average(fast, r => r.mFastMembershipMS)) +
			", Topology=" + format(average(fast, r => r.mFastTopologyMS)) +
			", Geometry=" + format(average(fast, r => r.mFastGeometryBuildMS)) +
			", Upload=" + format(average(fast, r => r.mFastVertexUploadMS)) +
			", Sort=" + format(average(fast, r => r.mFastSortIndexMS)) +
			", Bounds=" + format(average(fast, r => r.mFastBoundsMS)) + "]");
	}

	private string buildReport()
	{
		StringBuilder builder = new(131072);
		builder.AppendLine("ScenarioID,Segment,Target,Profile,Case,SortMode,Count,VariantCount,VisiblePercent,Frames,WallMeanMS,WallMedianMS,WallP95MS,WallP99MS,MutationMeanMS,DrawCalls,Batches,SetPass,Vertices,Triangles,VBUploadBytes,IBUploadBytes,GCAllocBytes,GCCollectionsGen0,CPUFrameMS,MainThreadMS,RenderThreadMS,GPUFrameMS,FastBatchCount,FastVertexCount,FastIndexCount,FastInternalVBUploadBytes,FastInternalIBUploadBytes,FastSystemTotalMS,FastRendererScanMS,FastMembershipMS,FastBatchUpdateMS,FastTopologyMS,FastElementScanMS,FastGeometryBuildMS,FastVertexUploadMS,FastSortIndexMS,FastBoundsMS,FastTransformChangedCount,FastMembershipCheckCount,FastMembershipChangedCount,FastTopologyRebuildCount,FastDirtyElementCount,FastVertexUploadCallCount,FastIndexUploadCallCount");
		for (int i = 0; i < mResults.Count; ++i)
		{
			Result r = mResults[i];
			builder.Append(r.mScenarioID).Append(',')
				.Append(r.mSegment).Append(',')
				.Append(r.mTarget).Append(',')
				.Append(r.mProfile).Append(',')
				.Append(r.mCase).Append(',')
				.Append(r.mSortMode).Append(',')
				.Append(r.mCount).Append(',')
				.Append(r.mVariantCount).Append(',')
				.Append(format(r.mVisibleFraction * 100.0)).Append(',')
				.Append(r.mFrames).Append(',')
				.Append(format(r.mWallMean)).Append(',')
				.Append(format(r.mWallMedian)).Append(',')
				.Append(format(r.mWallP95)).Append(',')
				.Append(format(r.mWallP99)).Append(',')
				.Append(format(r.mMutationMean)).Append(',')
				.Append(format(r.mDrawCalls)).Append(',')
				.Append(format(r.mBatches)).Append(',')
				.Append(format(r.mSetPass)).Append(',')
				.Append(format(r.mVertices)).Append(',')
				.Append(format(r.mTriangles)).Append(',')
				.Append(format(r.mVBUpload)).Append(',')
				.Append(format(r.mIBUpload)).Append(',')
				.Append(format(r.mGCAlloc)).Append(',')
				.Append(r.mGCCollections0).Append(',')
				.Append(format(r.mCPUFrame)).Append(',')
				.Append(format(r.mMainThread)).Append(',')
				.Append(format(r.mRenderThread)).Append(',')
				.Append(format(r.mGPUFrame)).Append(',')
				.Append(r.mFastBatchCount).Append(',')
				.Append(r.mFastVertexCount).Append(',')
				.Append(r.mFastIndexCount).Append(',')
				.Append(format(r.mFastInternalVBUpload)).Append(',')
				.Append(format(r.mFastInternalIBUpload)).Append(',')
				.Append(format(r.mFastSystemTotalMS)).Append(',')
				.Append(format(r.mFastRendererScanMS)).Append(',')
				.Append(format(r.mFastMembershipMS)).Append(',')
				.Append(format(r.mFastBatchUpdateMS)).Append(',')
				.Append(format(r.mFastTopologyMS)).Append(',')
				.Append(format(r.mFastElementScanMS)).Append(',')
				.Append(format(r.mFastGeometryBuildMS)).Append(',')
				.Append(format(r.mFastVertexUploadMS)).Append(',')
				.Append(format(r.mFastSortIndexMS)).Append(',')
				.Append(format(r.mFastBoundsMS)).Append(',')
				.Append(format(r.mFastTransformChangedCount)).Append(',')
				.Append(format(r.mFastMembershipCheckCount)).Append(',')
				.Append(format(r.mFastMembershipChangedCount)).Append(',')
				.Append(format(r.mFastTopologyRebuildCount)).Append(',')
				.Append(format(r.mFastDirtyElementCount)).Append(',')
				.Append(format(r.mFastVertexUploadCallCount)).Append(',')
				.Append(format(r.mFastIndexUploadCallCount))
				.AppendLine();
		}
		return builder.ToString();
	}

	private string buildMetadata(List<Scenario> plan)
	{
		StringBuilder builder = new(8192);
		builder.AppendLine("FastSpriteRenderer Benchmark V5 Metadata");
		builder.AppendLine("Timestamp=" + DateTime.Now.ToString("O", CultureInfo.InvariantCulture));
		builder.AppendLine("Suite=" + mSuite);
		builder.AppendLine("Unity=" + Application.unityVersion);
		builder.AppendLine("Platform=" + Application.platform);
		builder.AppendLine("IsEditor=" + Application.isEditor);
		builder.AppendLine("GraphicsAPI=" + SystemInfo.graphicsDeviceType);
		builder.AppendLine("GraphicsDevice=" + SystemInfo.graphicsDeviceName);
		builder.AppendLine("GraphicsVendor=" + SystemInfo.graphicsDeviceVendor);
		builder.AppendLine("GraphicsMemoryMB=" + SystemInfo.graphicsMemorySize);
		builder.AppendLine("CPU=" + SystemInfo.processorType);
		builder.AppendLine("CPUCount=" + SystemInfo.processorCount);
		builder.AppendLine("SystemMemoryMB=" + SystemInfo.systemMemorySize);
		builder.AppendLine("Resolution=" + Screen.width + "x" + Screen.height);
		builder.AppendLine("RenderPipeline=" + (GraphicsSettings.currentRenderPipeline != null ? GraphicsSettings.currentRenderPipeline.GetType().FullName : "BuiltIn"));
		builder.AppendLine("ConfiguredWarmupFrames=" + mWarmupFrames);
		builder.AppendLine("ConfiguredSampleFrames=" + mSampleFrames);
		builder.AppendLine("EffectiveWarmupFrames=" + getEffectiveWarmupFrames());
		builder.AppendLine("EffectiveSampleFrames=" + getEffectiveSampleFrames());
		builder.AppendLine("FiveMinuteCorePreset=" + mUseFiveMinuteCorePreset);
		builder.AppendLine("TimeBudgetEnabled=" + mEnforceTotalTimeBudget);
		builder.AppendLine("TimeBudgetSeconds=" + mMaxTotalDurationSeconds.ToString("F1", CultureInfo.InvariantCulture));
		builder.AppendLine("StoppedByTimeBudget=" + mStoppedByTimeBudget);
		builder.AppendLine("ElapsedSeconds=" + mBenchmarkElapsedSeconds.ToString("F3", CultureInfo.InvariantCulture));
		builder.AppendLine("CompletedCases=" + mCompletedCaseCount + "/" + mTotalCaseCount);
		builder.AppendLine("ABBA=" + mUseSandwichOrder);
		builder.AppendLine("VSyncDisabledByBenchmark=" + mDisableVSync);
		builder.AppendLine("UnlockedTargetFrameRate=" + mUnlockedTargetFrameRate);
		builder.AppendLine("FastInternalProfileEnabled=" + mCollectFastInternalProfile);
		builder.AppendLine("WallClockSource=StopwatchAcrossYield");
		builder.AppendLine("GroupSize=" + mGroupSize);
		builder.AppendLine("DeepHierarchyDepth=" + mDeepHierarchyDepth);
		builder.AppendLine("ScenarioCount=" + plan.Count);
		builder.AppendLine();
		for (int i = 0; i < plan.Count; ++i)
		{
			Scenario s = plan[i];
			builder.Append("Scenario ").Append(s.mID)
				.Append(": Profile=").Append(s.mProfile)
				.Append(", Count=").Append(s.mCount)
				.Append(", Variants=").Append(s.mVariantCount)
				.Append(", Visible=").Append((s.getVisibleFraction() * 100.0f).ToString("F1", CultureInfo.InvariantCulture)).Append('%')
				.Append(", Sort=").Append(s.mSortMode)
				.Append(", Cases=");
			for (int c = 0; c < s.mCases.Count; ++c)
			{
				if (c > 0) builder.Append('|');
				builder.Append(s.mCases[c]);
			}
			builder.AppendLine();
		}
		return builder.ToString();
	}

	private List<Result> getResults(Target target, int scenarioID, BenchmarkCase benchmarkCase)
	{
		List<Result> result = new();
		for (int i = 0; i < mResults.Count; ++i)
		{
			Result item = mResults[i];
			if (item.mTarget == target && item.mScenarioID == scenarioID && item.mCase == benchmarkCase)
			{
				result.Add(item);
			}
		}
		return result;
	}

	private static double average(List<Result> values, Func<Result, double> selector)
	{
		if (values.Count == 0) return 0.0;
		double total = 0.0;
		for (int i = 0; i < values.Count; ++i) total += selector(values[i]);
		return total / values.Count;
	}

	private void prepareAssets()
	{
		if (mSpriteA == null || mSpriteB == null)
		{
			createRuntimeAtlas();
		}
		if (mMaterial == null)
		{
			Shader shader = Shader.Find("Sprites/Default");
			if (shader != null)
			{
				mOwnedMaterial = new Material(shader) { name = "FastSpriteBenchmarkMaterial" };
				mMaterial = mOwnedMaterial;
			}
		}
		if (mSpriteA != null && mSpriteB != null && mSpriteA.texture != mSpriteB.texture)
		{
			Debug.LogWarning("SpriteA/SpriteB are on different textures. Same-atlas SpriteSwap cases will also become batch-migration cases. Prefer same-texture SpriteA/SpriteB.");
		}
		if (mTightSpriteA == null || mTightSpriteB == null)
		{
			createRuntimeTightSprites();
		}
		createTextureVariants();
		createMaterialVariants();
	}

	private void createTextureVariants()
	{
		mTextureVariantSprites = new Sprite[GENERATED_VARIANT_COUNT];
		mTextureVariantSprites[0] = mSpriteA;
		for (int i = 1; i < GENERATED_VARIANT_COUNT; ++i)
		{
			Texture2D texture = new(64, 64, TextureFormat.RGBA32, false)
			{
				name = "FastSpriteBenchmark_TextureVariant_" + i,
				filterMode = FilterMode.Point,
				wrapMode = TextureWrapMode.Clamp,
			};
			Color32 color = new((byte)(40 + (i * 37) % 200), (byte)(50 + (i * 67) % 190), (byte)(60 + (i * 97) % 180), 255);
			Color32[] pixels = new Color32[64 * 64];
			for (int p = 0; p < pixels.Length; ++p) pixels[p] = color;
			texture.SetPixels32(pixels);
			texture.Apply(false, false);
			Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 100.0f, 0, SpriteMeshType.FullRect, new Vector4(8, 8, 8, 8));
			sprite.name = "FastSpriteBenchmark_TextureVariantSprite_" + i;
			mOwnedVariantTextures.Add(texture);
			mOwnedVariantSprites.Add(sprite);
			mTextureVariantSprites[i] = sprite;
		}
	}

	private void createMaterialVariants()
	{
		mMaterialVariants = new Material[GENERATED_VARIANT_COUNT];
		mMaterialVariants[0] = mMaterial;
		for (int i = 1; i < GENERATED_VARIANT_COUNT; ++i)
		{
			Material material = mMaterial != null ? new Material(mMaterial) : null;
			if (material != null)
			{
				material.name = "FastSpriteBenchmark_MaterialVariant_" + i;
				mOwnedVariantMaterials.Add(material);
			}
			mMaterialVariants[i] = material;
		}
	}

	private void createRuntimeAtlas()
	{
		const int width = 128;
		const int height = 64;
		mOwnedTexture = new Texture2D(width, height, TextureFormat.RGBA32, false)
		{
			name = "FastSpriteBenchmarkAtlas",
			filterMode = FilterMode.Point,
			wrapMode = TextureWrapMode.Clamp,
		};
		Color32[] pixels = new Color32[width * height];
		for (int y = 0; y < height; ++y)
		{
			for (int x = 0; x < width; ++x)
			{
				bool right = x >= 64;
				pixels[y * width + x] = right ? new Color32(255, 170, 70, 255) : new Color32(70, 180, 255, 255);
			}
		}
		mOwnedTexture.SetPixels32(pixels);
		mOwnedTexture.Apply(false, false);
		Vector4 border = new(8.0f, 8.0f, 8.0f, 8.0f);
		mOwnedSpriteA = Sprite.Create(mOwnedTexture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 100.0f, 0, SpriteMeshType.FullRect, border);
		mOwnedSpriteB = Sprite.Create(mOwnedTexture, new Rect(64, 0, 64, 64), new Vector2(0.5f, 0.5f), 100.0f, 0, SpriteMeshType.FullRect, border);
		mOwnedSpriteA.name = "FastSpriteBenchmarkA";
		mOwnedSpriteB.name = "FastSpriteBenchmarkB";
		mSpriteA = mOwnedSpriteA;
		mSpriteB = mOwnedSpriteB;
	}

	private void createRuntimeTightSprites()
	{
		mOwnedTightTexture = new Texture2D(128, 64, TextureFormat.RGBA32, false)
		{
			name = "FastSpriteBenchmarkTightAtlas",
			filterMode = FilterMode.Point,
			wrapMode = TextureWrapMode.Clamp,
		};
		Color32[] pixels = new Color32[128 * 64];
		for (int y = 0; y < 64; ++y)
		{
			for (int x = 0; x < 128; ++x)
			{
				int localX = x % 64;
				float dx = localX - 31.5f;
				float dy = y - 31.5f;
				bool inside = dx * dx + dy * dy < 31.0f * 31.0f;
				pixels[y * 128 + x] = inside ? (x < 64 ? new Color32(120, 255, 150, 255) : new Color32(255, 120, 180, 255)) : new Color32(0, 0, 0, 0);
			}
		}
		mOwnedTightTexture.SetPixels32(pixels);
		mOwnedTightTexture.Apply(false, false);
		mOwnedTightSpriteA = Sprite.Create(mOwnedTightTexture, new Rect(0, 0, 64, 64), new Vector2(0.5f, 0.5f), 100.0f, 0, SpriteMeshType.FullRect);
		mOwnedTightSpriteB = Sprite.Create(mOwnedTightTexture, new Rect(64, 0, 64, 64), new Vector2(0.5f, 0.5f), 100.0f, 0, SpriteMeshType.FullRect);
		// OverrideGeometry expects Sprite.rect-space pixel coordinates, not local/world units.
		// Keep every point strictly inside the 64x64 rect because Unity 6 rejects vertices
		// that land on/outside the upper edge on some versions/backends.
		Vector2[] vertices =
		{
			new(1.0f, 32.0f), new(9.0f, 55.0f), new(32.0f, 63.0f), new(55.0f, 55.0f),
			new(63.0f, 32.0f), new(55.0f, 9.0f), new(32.0f, 1.0f), new(9.0f, 9.0f),
		};
		ushort[] triangles =
		{
			0, 1, 7, 1, 2, 7, 2, 6, 7, 2, 3, 6, 3, 5, 6, 3, 4, 5,
		};
		mOwnedTightSpriteA.OverrideGeometry(vertices, triangles);
		mOwnedTightSpriteB.OverrideGeometry(vertices, triangles);
		mOwnedTightSpriteA.name = "FastSpriteBenchmarkTightA";
		mOwnedTightSpriteB.name = "FastSpriteBenchmarkTightB";
		mTightSpriteA = mOwnedTightSpriteA;
		mTightSpriteB = mOwnedTightSpriteB;
	}

	private Material getBenchmarkMaterial()
	{
		return mMaterial;
	}

	private Camera getCamera()
	{
		if (mCamera != null) return mCamera;
		mCamera = Camera.main;
		return mCamera;
	}

	private void fitCameraToGrid(int count)
	{
		Camera camera = getCamera();
		if (camera == null || !camera.orthographic) return;
		int safeCount = Mathf.Max(1, count);
		int columns = Mathf.Max(1, Mathf.Min(mColumns, safeCount));
		int rows = Mathf.CeilToInt(safeCount / (float)columns);
		float width = Mathf.Max(columns - 1, 1) * mSpacing + 2.0f;
		float height = Mathf.Max(rows - 1, 1) * mSpacing + 2.0f;
		float aspect = Mathf.Max(camera.aspect, 0.1f);
		camera.orthographicSize = Mathf.Max(height * 0.5f, width * 0.5f / aspect);
		camera.transform.position = new Vector3((columns - 1) * mSpacing * 0.5f, -(rows - 1) * mSpacing * 0.5f, -10.0f);
		camera.transform.rotation = Quaternion.identity;
	}

	private Vector3 gridPosition(int index, int count)
	{
		int safeCount = Mathf.Max(1, count);
		int columns = Mathf.Max(1, Mathf.Min(mColumns, safeCount));
		return new Vector3((index % columns) * mSpacing, -(index / columns) * mSpacing, 0.0f);
	}

	private static int sparseIndex(int i, int total, int changed)
	{
		if (total <= 0) return 0;
		if (changed >= total) return i;
		int step = Mathf.Max(total / changed, 1);
		return Mathf.Min(i * step, total - 1);
	}

	private void startRecorders()
	{
		stopRecorders();
		mDrawCalls = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count");
		mBatches = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Total Batches Count");
		mSetPass = ProfilerRecorder.StartNew(ProfilerCategory.Render, "SetPass Calls Count");
		mVertices = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Vertices Count");
		mTriangles = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Triangles Count");
		mVBUpload = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Vertex Buffer Upload In Frame Bytes");
		mIBUpload = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Index Buffer Upload In Frame Bytes");
		mGCAlloc = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");
		mRecordersStarted = true;
		mFrameTimingEnabled = FrameTimingManager.IsFeatureEnabled();
	}

	private void stopRecorders()
	{
		if (!mRecordersStarted) return;
		mDrawCalls.Dispose();
		mBatches.Dispose();
		mSetPass.Dispose();
		mVertices.Dispose();
		mTriangles.Dispose();
		mVBUpload.Dispose();
		mIBUpload.Dispose();
		mGCAlloc.Dispose();
		mRecordersStarted = false;
	}

	private void disposePairScene(PairScene scene)
	{
		if (scene == null) return;
		if (scene.mNativeRoot != null) Destroy(scene.mNativeRoot);
		if (scene.mFastRoot != null) Destroy(scene.mFastRoot);
		if (scene.mFastSystem != null) Destroy(scene.mFastSystem.gameObject);
	}

	private void disposeOwnedAssets()
	{
		for (int i = 0; i < mOwnedVariantSprites.Count; ++i) if (mOwnedVariantSprites[i] != null) Destroy(mOwnedVariantSprites[i]);
		for (int i = 0; i < mOwnedVariantTextures.Count; ++i) if (mOwnedVariantTextures[i] != null) Destroy(mOwnedVariantTextures[i]);
		for (int i = 0; i < mOwnedVariantMaterials.Count; ++i) if (mOwnedVariantMaterials[i] != null) Destroy(mOwnedVariantMaterials[i]);
		for (int i = 0; i < mOwnedExtraAssets.Count; ++i) if (mOwnedExtraAssets[i] != null) Destroy(mOwnedExtraAssets[i]);
		mOwnedVariantSprites.Clear();
		mOwnedVariantTextures.Clear();
		mOwnedVariantMaterials.Clear();
		mOwnedExtraAssets.Clear();

		if (mOwnedTightSpriteA != null) Destroy(mOwnedTightSpriteA);
		if (mOwnedTightSpriteB != null) Destroy(mOwnedTightSpriteB);
		if (mOwnedTightTexture != null) Destroy(mOwnedTightTexture);
		if (mOwnedSpriteA != null) Destroy(mOwnedSpriteA);
		if (mOwnedSpriteB != null) Destroy(mOwnedSpriteB);
		if (mOwnedTexture != null) Destroy(mOwnedTexture);
		if (mOwnedMaterial != null) Destroy(mOwnedMaterial);

		if (mSpriteA == mOwnedSpriteA) mSpriteA = null;
		if (mSpriteB == mOwnedSpriteB) mSpriteB = null;
		if (mTightSpriteA == mOwnedTightSpriteA) mTightSpriteA = null;
		if (mTightSpriteB == mOwnedTightSpriteB) mTightSpriteB = null;
		if (mMaterial == mOwnedMaterial) mMaterial = null;
		mOwnedSpriteA = null;
		mOwnedSpriteB = null;
		mOwnedTexture = null;
		mOwnedMaterial = null;
		mOwnedTightSpriteA = null;
		mOwnedTightSpriteB = null;
		mOwnedTightTexture = null;
		mTextureVariantSprites = null;
		mMaterialVariants = null;
	}

	private static double elapsedMS(long start)
	{
		return (Stopwatch.GetTimestamp() - start) * 1000.0 / Stopwatch.Frequency;
	}

	private static double percentileSorted(double[] sorted, double p)
	{
		if (sorted == null || sorted.Length == 0) return 0.0;
		int index = Mathf.Clamp(Mathf.CeilToInt((float)(sorted.Length * p)) - 1, 0, sorted.Length - 1);
		return sorted[index];
	}

	private static string format(double value)
	{
		return value.ToString("F4", CultureInfo.InvariantCulture);
	}
}
