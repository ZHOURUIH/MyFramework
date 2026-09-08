using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using EasyECS;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Jobs;
using UnityEngine.Rendering;

public enum FastSpriteBackendMode
{
	Auto,
	GPUDrivenIndirect,
	GPUDrivenDirect,
	CompatMesh,
}

public struct FastSpriteFrameProfile
{
	public double mTotalMS;
	public double mRendererScanMS;
	public double mMembershipMS;
	public double mBatchUpdateMS;
	public double mTopologyMS;
	public double mElementScanMS;
	public double mGeometryBuildMS;
	public double mVertexUploadMS;
	public double mSortIndexMS;
	public double mBoundsMS;

	public double mSortGroupUpdateMS;
	// EasyECS/GPU-driven backend timings.
	public double mGPUDrivenDirtyBuildMS;
	public double mGPUDrivenPlanBuildMS;
	public double mGPUDrivenUploadMS;
	public double mGPUDrivenCommandBuildMS;

	public int mTransformChangedCount;
	public int mEasyECSBurstTransformCount;
	public int mEasyECSRuntimeInactiveGateCount;
	public int mRootTransformCheckedCount;
	public int mRootTransformChangedCount;
	public int mRootChildPropagatedCount;
	public int mRootMotionScannedElementCount;
	public int mRootMotionDirtyElementCount;
	public int mRootMotionQuadFastPathCount;
	public int mTransformTrackingAddCount;
	public int mTransformTrackingRemoveCount;
	public int mTransformTrackingFullRebuildCount;

	public int mMembershipCheckCount;
	public int mMembershipChangedCount;
	public int mSortingOrderFastTransferCount;
	public int mSortingOrderTransferGroupCount;
	public int mSortGroupDirtyCount;
	public int mSortGroupNoMoveCount;
	public int mSortGroupLocalRepairCount;
	public int mSortGroupAdjacentChunkMoveCount;
	public int mSortGroupGlobalRelocateCount;
	public int mSortGroupMovedSlots;
	public int mSortGroupBatchRepairLayerCount;
	public int mSortGroupBatchRepairNodeCount;
	public int mSortGroupParallelSortNodeCount;
	public int mSortGroupParallelSortRunCount;
	public int mSortGroupParallelSortDispatchCount;
	public int mBatchUpdateCount;
	public int mTopologyRebuildCount;
	public int mDirtyElementCount;
	public int mUVOnlyElementCount;
	public int mUVColorOnlyElementCount;
	public int mVertexUploadCallCount;
	public int mIndexUploadCallCount;
	public int mGPUDrivenActive;
	public int mGPUDrivenDrawCount;
	public int mGPUDrivenInstanceCount;
	public int mGPUDrivenDirtyRendererCount;
	public int mGPUDrivenHotWriteCount;
	public int mGPUDrivenSpriteAssetCommitCount;
	public int mGPUDrivenGeometryUploadBytes;
	public int mGPUDrivenInstanceUploadBytes;
	public int mGPUDrivenOrderUploadBytes;
	public int mGPUDrivenGeometryUploadCalls;
	public int mGPUDrivenInstanceUploadCalls;
	public int mGPUDrivenOrderUploadCalls;
	public int mGPUDrivenDirtyDensityChunkDispatchCount;
	public int mGPUDrivenLogicalBatchSkipCount;
}

// Scene-level coordinator for FastSpriteRenderer.
[ExecuteAlways]
[DefaultExecutionOrder(10000)]
[DisallowMultipleComponent]
public sealed class FastSpriteRenderSystem : MonoBehaviour
{
	private const int TRANSFORM_JOB_MIN_COUNT = 256;
	private const int ROOT_TRANSFORM_JOB_MIN_COUNT = 64;
	private const int MAX_CACHED_BATCH_COUNT = 1536;
	private const int TARGET_CACHED_BATCH_COUNT = 1024;
	[SerializeField] private Material mDefaultMaterial;
	[SerializeField] private FastSpriteSortMode mSortMode = FastSpriteSortMode.CameraDistance;
	[SerializeField] private Camera mSortCamera;
	[SerializeField] private bool mRenderInEditMode = true;
	[SerializeField] private bool mCollectProfileStats;
	// GPU-driven is opt-in at component level; when enabled, EasyECS/GPU owns the accelerated
	// render path and FastSpriteBatch is only the logical-Mesh fallback.
	[SerializeField] private bool mEnableGPUDrivenProcedural;
	[Tooltip("Auto selects the fastest validated backend for the current environment. Backend selection happens at system level, not per renderer mutation.")]
	[SerializeField] private FastSpriteBackendMode mBackendMode = FastSpriteBackendMode.Auto;
	private FastSpriteBackendMode mResolvedBackendMode = FastSpriteBackendMode.CompatMesh;
	private bool mGPUDrivenRuntimeActive;
	private string mBackendReason = "NotResolved";
	// Bulk registration/loading can pause per-frame system work without removing this system
	// from the live registry. Renderers can continue to register incrementally and one flush
	// rebuilds the final GPU state after the transaction completes.
	private bool mUpdatePaused;

	private readonly List<FastSpriteRenderer> mRenderers = new(1024);
	private readonly FastDictionary<FastSpriteBatchKey, FastSpriteBatch> mBatches = new();
	private readonly FastSpriteChunkedSortManager mSortGroupSortManager = new();
	private readonly List<FastSpriteSortGroup> mOrderedSortGroups = new(512);
	// SortGroup revisions consumed by the EasyECS/GPU plan.
	private int mSortGroupContentRevision;
	private int mSortGroupOrderRevision;
	private readonly List<FastSpriteRenderer> mMembershipDirtyRenderers = new(64);
	private readonly FastDictionary<SortingOrderTransferGroupKey, int> mSortingOrderTransferGroupLookup = new(256);
	private readonly FastSpriteSortingOrderTransferGroupData_ECSList mSortingOrderTransferGroups = new(256);
	private readonly FastSpriteSortingOrderTransferItemData_ECSList mSortingOrderTransferItems = new(2048);
	private readonly List<FastSpriteRenderer> mMembershipFallbackRenderers = new(64);
	private readonly List<FastSpriteBatchKey> mDormantBatchPruneKeys = new(128);
	private readonly List<FastSpriteBatch> mGPUPlanBatches = new(1024);
	private readonly Int_ECSList mGPUPlanSortingLayerCacheIDs = new(8);
	private readonly Int_ECSList mGPUPlanSortingLayerCacheValues = new(8);
	private readonly FastSpriteGPUDrivenBackend mGPUDrivenBackend = new();
	private readonly Int_ECSList mGPUDrivenValidationExpectedSlots = new(4096);
	private int mGPUDrivenBatchOrderRevision;
	private int mGPUDrivenPlanBatchRevision = -1;
	private int mGPUDrivenPlanGroupRevision = -1;
	private int mGPUDrivenPlanGroupOrderRevision = -1;
	private int mGPUDrivenPlanBatchOrderRevision = -1;
	private int mGPUDrivenPlanLayoutRevision = -1;
	private bool mGPUDrivenDirectStreamFrame;
	private int mBatchSetRevision;
	private TransformAccessArray mTransformAccessArray;
	private NativeArray<Matrix4x4> mTransformMatrices;
	private FastSpriteTransformSortData_ECSList mTransformSortECS;
	private readonly List<FastSpriteRenderer> mIndependentTransformRenderers = new(256);
	private TransformAccessArray mIndependentTransformAccessArray;
	private Int_ECSList mIndependentTransformECS;
	private readonly List<FastSpriteSortGroup> mRootTransformGroups = new(512);
	private readonly List<FastSpriteRenderer> mRootTrackedChildren = new(4096);
	private TransformAccessArray mRootTransformAccessArray;
	private NativeArray<Matrix4x4> mRootTransformMatrices;
	private Byte_ECSList mRootTransformECS;
	private Int_ECSList mRootChildECS;
	private NativeArray<Matrix4x4> mRootChildLocalToRoot;
	private Matrix4x4[] mCompatRootToBatchMatrices = Array.Empty<Matrix4x4>();
	private byte[] mCompatRootPatchActive = Array.Empty<byte>();
	private NativeArray<int> mRootTransformChangedCount;
	private JobHandle mRootTrackingJobHandle;
	private bool mRootTrackingJobScheduled;
	private double mRootTrackingScheduleCPUTimeMS;
	private int mRootChangedThisFrameCount;
	private int mPendingTransformTrackingAddCount;
	private int mPendingTransformTrackingRemoveCount;
	private Material mOwnedDefaultMaterial;
	// EasyECS/GPU-driven root matrix buffer. Logical Mesh fallback is CPU-only.
	private GraphicsBuffer mGPURootMatrixBuffer;
	private int mGPURootMatrixCapacity;
	private int mGPURootMatrixUploadedCount;
	private long mRegistrationSequence;
	private Matrix4x4 mLastSystemLocalToWorld;
	private bool mSystemTransformValid;
	private ulong mLastSortCameraObjectID;
	private TransparencySortMode mLastTransparencySortMode;
	private Vector3 mLastCameraSortPosition;
	private Vector3 mLastCameraSortForward;
	private Vector3 mLastTransparencySortAxis;
	private bool mCameraSortStateValid;

	private static readonly List<FastSpriteRenderSystem> sSystems = new();
	private static readonly List<FastSpriteRenderer> sPendingRenderers = new();

	private int mActiveBatchCount;
	private int mLastVertexCount;
	private int mLastIndexCount;
	private int mLastVertexUploadBytes;
	private int mLastIndexUploadBytes;
	private FastSpriteFrameProfile mLastProfile;

	private readonly struct SortingOrderTransferGroupKey : IEquatable<SortingOrderTransferGroupKey>
	{
		public readonly FastSpriteBatch mSourceBatch;
		public readonly int mTargetSortingOrder;

		public SortingOrderTransferGroupKey(FastSpriteBatch sourceBatch, int targetSortingOrder)
		{
			mSourceBatch = sourceBatch;
			mTargetSortingOrder = targetSortingOrder;
		}

		public bool Equals(SortingOrderTransferGroupKey other)
		{
			return ReferenceEquals(mSourceBatch, other.mSourceBatch) &&
				mTargetSortingOrder == other.mTargetSortingOrder;
		}

		public override bool Equals(object obj)
		{
			return obj is SortingOrderTransferGroupKey other && Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((mSourceBatch != null ? mSourceBatch.GetHashCode() : 0) * 397) ^ mTargetSortingOrder;
			}
		}
	}

	internal struct FastSortContext
	{
		public FastSpriteSortMode mMode;
		public TransparencySortMode mTransparencyMode;
		public Vector3 mCameraPosition;
		public Vector3 mCameraForward;
		public Vector3 mCustomAxis;
		public bool mHasCamera;
	}

	[BurstCompile(OptimizeFor = OptimizeFor.Performance)]
	private unsafe struct DetectRootTransformJob : IJobParallelForTransform
	{
		public NativeArray<Matrix4x4> mPreviousMatrices;
		[NativeDisableUnsafePtrRestriction] public Byte_ECSList.BurstView mTracking;
		[NativeDisableUnsafePtrRestriction] public int* mChangedRootCount;
		public byte mForceChanged;

		public void Execute(int index, TransformAccess transformAccess)
		{
			if (!transformAccess.isValid)
			{
				mTracking.mValue[index] = 0;
				return;
			}
			Matrix4x4 current = transformAccess.localToWorldMatrix;
			Matrix4x4 previous = mPreviousMatrices[index];
			bool changed = mForceChanged != 0 || !matrixEqualsAffine(current, previous);
			if (!changed)
			{
				mTracking.mValue[index] = 0;
				return;
			}
			mPreviousMatrices[index] = current;
			mTracking.mValue[index] = 1;
			System.Threading.Interlocked.Increment(ref *mChangedRootCount);
		}

		private static bool matrixEqualsAffine(Matrix4x4 left, Matrix4x4 right)
		{
			// Unity Transform localToWorld matrices are affine. Compare only the
			// meaningful 3x4 payload so Burst can inline the check directly in this job.
			return left.m00 == right.m00 && left.m01 == right.m01 && left.m02 == right.m02 && left.m03 == right.m03 &&
				left.m10 == right.m10 && left.m11 == right.m11 && left.m12 == right.m12 && left.m13 == right.m13 &&
				left.m20 == right.m20 && left.m21 == right.m21 && left.m22 == right.m22 && left.m23 == right.m23;
		}
	}

	[BurstCompile(OptimizeFor = OptimizeFor.Performance)]
	private unsafe struct DetectTransformAndSortJob : IJobParallelForTransform
	{
		public NativeArray<Matrix4x4> mPreviousMatrices;
		[NativeDisableUnsafePtrRestriction] public FastSpriteTransformSortData_ECSList.BurstView mState;
		public FastSortContext mSortContext;

		public void Execute(int index, TransformAccess transformAccess)
		{
			if (!transformAccess.isValid)
			{
				mState.mTransformChanged[index] = 0;
				return;
			}

			Matrix4x4 current = transformAccess.localToWorldMatrix;
			Matrix4x4 previous = mPreviousMatrices[index];
			if (matrixEqualsAffine(current, previous))
			{
				mState.mTransformChanged[index] = 0;
				return;
			}

			mPreviousMatrices[index] = current;
			mState.mTransformChanged[index] = 1;
			if (mSortContext.mMode == FastSpriteSortMode.Registration)
			{
				mState.mSortValue[index] = 0.0f;
				return;
			}

			float localX = mState.mSortPointX[index];
			float localY = mState.mSortPointY[index];
			float localZ = mState.mSortPointZ[index];
			float worldX;
			float worldY;
			float worldZ;
			if (localX == 0.0f && localY == 0.0f && localZ == 0.0f)
			{
				worldX = current.m03;
				worldY = current.m13;
				worldZ = current.m23;
			}
			else
			{
				worldX = current.m00 * localX + current.m01 * localY + current.m02 * localZ + current.m03;
				worldY = current.m10 * localX + current.m11 * localY + current.m12 * localZ + current.m13;
				worldZ = current.m20 * localX + current.m21 * localY + current.m22 * localZ + current.m23;
			}

			float sortValue;
			if (mSortContext.mMode == FastSpriteSortMode.YAxis)
			{
				sortValue = worldY;
			}
			else if (!mSortContext.mHasCamera)
			{
				sortValue = 0.0f;
			}
			else if (mSortContext.mTransparencyMode == TransparencySortMode.CustomAxis)
			{
				sortValue = worldX * mSortContext.mCustomAxis.x + worldY * mSortContext.mCustomAxis.y + worldZ * mSortContext.mCustomAxis.z;
			}
			else if (mSortContext.mTransparencyMode == TransparencySortMode.Orthographic)
			{
				sortValue = worldX * mSortContext.mCameraForward.x + worldY * mSortContext.mCameraForward.y + worldZ * mSortContext.mCameraForward.z;
			}
			else
			{
				float dx = worldX - mSortContext.mCameraPosition.x;
				float dy = worldY - mSortContext.mCameraPosition.y;
				float dz = worldZ - mSortContext.mCameraPosition.z;
				sortValue = dx * dx + dy * dy + dz * dz;
			}
			mState.mSortValue[index] = sortValue;
		}

		private static bool matrixEqualsAffine(Matrix4x4 left, Matrix4x4 right)
		{
			// Unity Transform localToWorld matrices are affine. The last row is always
			// 0,0,0,1, so comparing the meaningful 3x4 payload avoids four useless loads.
			return left.m00 == right.m00 && left.m01 == right.m01 && left.m02 == right.m02 && left.m03 == right.m03 &&
				left.m10 == right.m10 && left.m11 == right.m11 && left.m12 == right.m12 && left.m13 == right.m13 &&
				left.m20 == right.m20 && left.m21 == right.m21 && left.m22 == right.m22 && left.m23 == right.m23;
		}
	}

	[BurstCompile]
	private unsafe struct DetectIndependentTransformAndSortJob : IJobParallelForTransform
	{
		[NativeDisableUnsafePtrRestriction] public Int_ECSList.BurstView mTracking;
		[NativeDisableParallelForRestriction] public NativeArray<Matrix4x4> mPreviousMatrices;
		[NativeDisableUnsafePtrRestriction] public FastSpriteTransformSortData_ECSList.BurstView mState;
		public FastSortContext mSortContext;

		public void Execute(int subsetIndex, TransformAccess transformAccess)
		{
			int rendererIndex = mTracking.mValue[subsetIndex];
			if ((uint)rendererIndex >= (uint)mPreviousMatrices.Length)
			{
				return;
			}
			if (!transformAccess.isValid)
			{
				mState.mTransformChanged[rendererIndex] = 0;
				return;
			}

			Matrix4x4 current = transformAccess.localToWorldMatrix;
			Matrix4x4 previous = mPreviousMatrices[rendererIndex];
			if (matrixEqualsAffine(current, previous))
			{
				mState.mTransformChanged[rendererIndex] = 0;
				return;
			}

			mPreviousMatrices[rendererIndex] = current;
			mState.mTransformChanged[rendererIndex] = 1;
			if (mSortContext.mMode == FastSpriteSortMode.Registration)
			{
				mState.mSortValue[rendererIndex] = 0.0f;
				return;
			}

			float localX = mState.mSortPointX[rendererIndex];
			float localY = mState.mSortPointY[rendererIndex];
			float localZ = mState.mSortPointZ[rendererIndex];
			float worldX;
			float worldY;
			float worldZ;
			if (localX == 0.0f && localY == 0.0f && localZ == 0.0f)
			{
				worldX = current.m03;
				worldY = current.m13;
				worldZ = current.m23;
			}
			else
			{
				worldX = current.m00 * localX + current.m01 * localY + current.m02 * localZ + current.m03;
				worldY = current.m10 * localX + current.m11 * localY + current.m12 * localZ + current.m13;
				worldZ = current.m20 * localX + current.m21 * localY + current.m22 * localZ + current.m23;
			}

			float sortValue;
			if (mSortContext.mMode == FastSpriteSortMode.YAxis)
			{
				sortValue = worldY;
			}
			else if (!mSortContext.mHasCamera)
			{
				sortValue = 0.0f;
			}
			else if (mSortContext.mTransparencyMode == TransparencySortMode.CustomAxis)
			{
				sortValue = worldX * mSortContext.mCustomAxis.x + worldY * mSortContext.mCustomAxis.y + worldZ * mSortContext.mCustomAxis.z;
			}
			else if (mSortContext.mTransparencyMode == TransparencySortMode.Orthographic)
			{
				sortValue = worldX * mSortContext.mCameraForward.x + worldY * mSortContext.mCameraForward.y + worldZ * mSortContext.mCameraForward.z;
			}
			else
			{
				float dx = worldX - mSortContext.mCameraPosition.x;
				float dy = worldY - mSortContext.mCameraPosition.y;
				float dz = worldZ - mSortContext.mCameraPosition.z;
				sortValue = dx * dx + dy * dy + dz * dz;
			}
			mState.mSortValue[rendererIndex] = sortValue;
		}

		private static bool matrixEqualsAffine(Matrix4x4 left, Matrix4x4 right)
		{
			return left.m00 == right.m00 && left.m01 == right.m01 && left.m02 == right.m02 && left.m03 == right.m03 &&
				left.m10 == right.m10 && left.m11 == right.m11 && left.m12 == right.m12 && left.m13 == right.m13 &&
				left.m20 == right.m20 && left.m21 == right.m21 && left.m22 == right.m22 && left.m23 == right.m23;
		}
	}

	public FastSpriteSortMode getSortMode()
	{
		return mSortMode;
	}
	public Camera getSortCamera()
	{
		return mSortCamera != null ? mSortCamera : Camera.main;
	}
	public int getRendererCount()
	{
		return mRenderers.Count;
	}
	public int getBatchCount()
	{
		return mActiveBatchCount;
	}
	public bool getGPUDrivenProceduralEnabled()
	{
		return mEnableGPUDrivenProcedural;
	}
	public FastSpriteBackendMode getBackendMode()
	{
		return mBackendMode;
	}
	public FastSpriteBackendMode getResolvedBackendMode()
	{
		if (!mGPUDrivenRuntimeActive)
		{
			return FastSpriteBackendMode.CompatMesh;
		}
		if (mResolvedBackendMode == FastSpriteBackendMode.GPUDrivenIndirect &&
			mGPUDrivenBackend.hasCommittedCommands() && !mGPUDrivenBackend.isUsingIndirectSubmission())
		{
			return FastSpriteBackendMode.GPUDrivenDirect;
		}
		return mResolvedBackendMode;
	}
	public string getBackendReason()
	{
		return mBackendReason;
	}
	public string getBackendDisplayName()
	{
		switch (getResolvedBackendMode())
		{
			case FastSpriteBackendMode.GPUDrivenIndirect: return "GPU Indirect";
			case FastSpriteBackendMode.GPUDrivenDirect: return "GPU Direct";
			default: return "Compat Mesh";
		}
	}
	public static bool isAndroidEmulatorEnvironment()
	{
		return FastSpriteGPUDrivenBackend.isLikelyAndroidEmulator();
	}

	public void setBackendMode(FastSpriteBackendMode mode)
	{
		if (mBackendMode == mode && (Application.isPlaying || !mEnableGPUDrivenProcedural))
		{
			return;
		}
		mBackendMode = mode;
		resolveBackendSelection(true);
	}

	public void setGPUDrivenProceduralEnabled(bool enabled)
	{
		if (mEnableGPUDrivenProcedural == enabled)
		{
			resolveBackendSelection(false);
			return;
		}
		mEnableGPUDrivenProcedural = enabled;
		resolveBackendSelection(true);
	}

	private void invalidateGPUPlanRevisions()
	{
		mGPUDrivenPlanBatchRevision = -1;
		mGPUDrivenPlanGroupRevision = -1;
		mGPUDrivenPlanGroupOrderRevision = -1;
		mGPUDrivenPlanBatchOrderRevision = -1;
		mGPUDrivenPlanLayoutRevision = -1;
	}

	private void warmGPUBackendForExistingRenderers()
	{
		for (int i = 0; i < mRenderers.Count; ++i)
		{
			FastSpriteRenderer renderer = mRenderers[i];
			if (renderer != null)
			{
				mGPUDrivenBackend.registerRenderer(renderer);
			}
		}
		foreach (FastSpriteBatch batch in mBatches.Values)
		{
			batch?.refreshGPUDrivenSlotOrder();
		}
	}

	private void resolveBackendSelection(bool restoreLogicalOnFallback)
	{
		bool wasGPUActive = mGPUDrivenRuntimeActive;
		FastSpriteBackendMode selected;
		string reason;

		if (!mEnableGPUDrivenProcedural)
		{
			selected = FastSpriteBackendMode.CompatMesh;
			reason = "GPUDrivenDisabled";
		}
		else if (mBackendMode == FastSpriteBackendMode.Auto)
		{
			if (FastSpriteGPUDrivenBackend.isLikelyAndroidEmulator())
			{
				selected = FastSpriteBackendMode.CompatMesh;
				reason = "Auto/AndroidEmulator";
			}
			else
			{
				selected = FastSpriteBackendMode.GPUDrivenIndirect;
				reason = "Auto/FastestGPUPath";
			}
		}
		else
		{
			selected = mBackendMode;
			reason = "Forced/" + mBackendMode;
		}

		mGPUDrivenBackend.setSubmissionMode(selected);
		bool gpuActive = selected != FastSpriteBackendMode.CompatMesh && mGPUDrivenBackend.isRuntimeSupported();
		if (!gpuActive && selected != FastSpriteBackendMode.CompatMesh)
		{
			reason += "/Unsupported->CompatMesh";
			selected = FastSpriteBackendMode.CompatMesh;
			mGPUDrivenBackend.setSubmissionMode(selected);
		}

		mResolvedBackendMode = selected;
		mGPUDrivenRuntimeActive = gpuActive;
		mBackendReason = reason;
		invalidateGPUPlanRevisions();
		mGPUDrivenDirectStreamFrame = false;

		if (mGPUDrivenRuntimeActive)
		{
			if (!wasGPUActive)
			{
				warmGPUBackendForExistingRenderers();
			}
		}
		else
		{
			mGPUDrivenBackend.invalidatePlan();
			mGPUDrivenBackend.detachCommandBuffer();
			if (wasGPUActive && restoreLogicalOnFallback)
			{
				restoreLogicalMeshesFromCPU();
			}
		}
	}

	internal bool tryCommitGPUSpriteAsset(FastSpriteRenderer renderer, int simpleAssetID)
	{
		if (!mGPUDrivenRuntimeActive || renderer == null || simpleAssetID <= 0 ||
			renderer.mBatch == null || renderer.mGPUDrivenSlot < 0 || !renderer.mRuntimeActiveState)
		{
			return false;
		}
		return mGPUDrivenBackend.tryCommitSpriteAsset(renderer, simpleAssetID);
	}

	internal bool tryQueueGPUVisualDirty(FastSpriteRenderer renderer, FastSpriteDirtyFlags flags)
	{
		const FastSpriteDirtyFlags allowed = FastSpriteDirtyFlags.Vertex | FastSpriteDirtyFlags.Color | FastSpriteDirtyFlags.Bounds;
		if ((flags & ~allowed) != 0)
		{
			return false;
		}
		FastSpriteDirtyFlags visual = flags & (FastSpriteDirtyFlags.Vertex | FastSpriteDirtyFlags.Color);
		if (visual != FastSpriteDirtyFlags.Vertex && visual != FastSpriteDirtyFlags.Color)
		{
			return false;
		}
		// Caller is renderer.mSystemOwner, so owner/null identity checks would be redundant on
		// every mutation. Runtime-active is the lifecycle-backed managed mirror.
		if (!mGPUDrivenRuntimeActive ||
			renderer.mBatch == null || renderer.mGPUDrivenSlot < 0 || !renderer.mRuntimeActiveState)
		{
			return false;
		}

		mGPUDrivenBackend.queueVisualDirty(renderer, visual);
		return true;
	}

	internal void notifyGPUDrivenRendererDirty(FastSpriteRenderer renderer, FastSpriteDirtyFlags flags)
	{
		if (!mGPUDrivenRuntimeActive ||
			renderer == null || renderer.mSystemOwner != this)
		{
			return;
		}
		mGPUDrivenBackend.queueDirty(renderer, flags);
	}

	internal void notifyGPUDrivenOrderDirty()
	{
		if (!mGPUDrivenRuntimeActive)
		{
			return;
		}
		unchecked
		{
			++mGPUDrivenBatchOrderRevision;
		}
	}

	public int getVertexCount()
	{
		return mLastVertexCount;
	}
	public int getIndexCount()
	{
		return mLastIndexCount;
	}
	public int getVertexUploadBytes()
	{
		return mLastVertexUploadBytes;
	}
	public int getIndexUploadBytes()
	{
		return mLastIndexUploadBytes;
	}
	public FastSpriteFrameProfile getLastProfile()
	{
		return mLastProfile;
	}
	public void setProfileStatsEnabled(bool enabled)
	{
		mCollectProfileStats = enabled;
	}
	internal bool isProfileStatsEnabled()
	{
		return mCollectProfileStats;
	}
	public void setUpdatePaused(bool paused)
	{
		mUpdatePaused = paused;
	}

	public void reserveRendererRegistrationCapacity(int requiredCount)
	{
		requiredCount = Mathf.Max(requiredCount, mRenderers.Count);
		if (requiredCount <= 0)
		{
			return;
		}

		if (mRenderers.Capacity < requiredCount)
		{
			mRenderers.Capacity = requiredCount;
		}
		if (mIndependentTransformRenderers.Capacity < requiredCount)
		{
			mIndependentTransformRenderers.Capacity = requiredCount;
		}
		if (mRootTransformGroups.Capacity < requiredCount)
		{
			mRootTransformGroups.Capacity = requiredCount;
		}
		if (mRootTrackedChildren.Capacity < requiredCount)
		{
			mRootTrackedChildren.Capacity = requiredCount;
		}

		ensureTransformTrackingCapacity(requiredCount);
		ensureIndependentTransformTrackingCapacity(requiredCount);
		ensureRootTrackingNativeCapacity(requiredCount, requiredCount);
		if (mGPUDrivenRuntimeActive)
		{
			mGPUDrivenBackend.reserveRendererCapacity(requiredCount);
		}
	}
	public bool getUpdatePaused()
	{
		return mUpdatePaused;
	}

	internal void notifyTransformTrackingModeChanged(FastSpriteRenderer renderer)
	{
		updateRendererTransformTracking(renderer);
	}

	internal void notifySortGroupTransformTrackingChanged(FastSpriteSortGroup group)
	{
		if (group == null)
		{
			return;
		}
		if (group.usesRootOnlyTransformTracking())
		{
			addRootTrackingGroup(group);
		}
		else
		{
			removeRootTrackingGroup(group);
		}
		List<FastSpriteRenderer> children = group.getRenderersUnsafe();
		for (int i = 0; i < children.Count; ++i)
		{
			updateRendererTransformTracking(children[i]);
		}
	}

	internal void notifySortGroupRendererAdded(FastSpriteSortGroup group, FastSpriteRenderer renderer)
	{
		if (group != null && group.usesRootOnlyTransformTracking())
		{
			addRootTrackingGroup(group);
		}
		updateRendererTransformTracking(renderer);
	}

	internal void notifySortGroupRendererRemoved(FastSpriteRenderer renderer)
	{
		removeRootTrackedChild(renderer);
	}

	internal void notifyManualTransformChanged(FastSpriteRenderer renderer)
	{
		if (renderer == null || renderer.mSystemOwner != this)
		{
			return;
		}
		int index = renderer.mSystemRendererIndex;
		if (index < 0 || !mTransformMatrices.IsCreated || index >= mTransformMatrices.Length)
		{
			return;
		}
		Matrix4x4 localToWorld = renderer.transform.localToWorldMatrix;
		mTransformMatrices[index] = localToWorld;
		FastSpriteSortGroup group = renderer.getSortGroup();
		if (group != null && group.usesRootOnlyTransformTracking())
		{
			renderer.cacheRootTrackingLocalToGroup(group, localToWorld);
			updateRootTrackedChildLocalMatrix(renderer, group);
			renderer.markTransformDirty(false);
			return;
		}
		FastSortContext sortContext = buildSortContext();
		applyTransformDirty(renderer, localToWorld, sortContext, false);
	}
	internal bool shouldBypassLogicalMeshSubmission()
	{
		return mGPUDrivenDirectStreamFrame;
	}
	public int getSortGroupCount()
	{
		return mSortGroupSortManager.getCount();
	}

	public bool validatePhysicalOrderForBenchmark(out string error)
	{
		if (mGPUDrivenRuntimeActive)
		{
			if (!isGPUDrivenPlanCurrent())
			{
				updateGPUDrivenProcedural();
			}
			return validateGPUDrivenOrderForBenchmark(out error);
		}
		error = null;
		return true;
	}

	private bool validateGPUDrivenOrderForBenchmark(out string error)
	{
		error = null;
		if (!mGPUDrivenBackend.isPlanValid())
		{
			error = "GPU-driven plan is not active/valid.";
			return false;
		}

		mGPUPlanBatches.Clear();
		foreach (FastSpriteBatch batch in mBatches.Values)
		{
			if (batch != null && !batch.getKey().mGroupedStorage &&
				(batch.getCount() > 0 || batch.hasRetainedInactiveElements()))
			{
				mGPUPlanBatches.Add(batch);
			}
		}
		mGPUPlanBatches.Sort(compareBatchOrder);
		mOrderedSortGroups.Clear();
		mSortGroupSortManager.appendOrderedGroups(mOrderedSortGroups);
		mGPUDrivenValidationExpectedSlots.Clear();

		int batchIndex = 0;
		int groupIndex = 0;
		while (batchIndex < mGPUPlanBatches.Count || groupIndex < mOrderedSortGroups.Count)
		{
			FastSpriteBatch batch = batchIndex < mGPUPlanBatches.Count ? mGPUPlanBatches[batchIndex] : null;
			FastSpriteSortGroup group = groupIndex < mOrderedSortGroups.Count ? mOrderedSortGroups[groupIndex] : null;
			bool takeGroup = group != null && (batch == null || compareGroupToBatch(group, batch) <= 0);
			Int_ECSList span;
			if (!takeGroup)
			{
				span = batch.getGPUDrivenSlotOrder();
				++batchIndex;
			}
			else
			{
				span = group.getGPUDrivenOrderedSlots(this);
				++groupIndex;
			}
			if (span == null || span.Count == 0)
			{
				continue;
			}
			var slots = span.getValueColumn();
			for (int i = 0; i < span.Count; ++i)
			{
				int slot = slots[i];
				if (mGPUDrivenBackend.isPlanSlotDrawable(slot))
				{
					mGPUDrivenValidationExpectedSlots.Add(slot);
				}
			}
		}

		int persistentCount = mGPUDrivenBackend.getOrderedSlotCount();
		int actualActiveIndex = 0;
		var expected = mGPUDrivenValidationExpectedSlots.getValueColumn();
		for (int i = 0; i < persistentCount; ++i)
		{
			int actualSlot = mGPUDrivenBackend.getOrderedSlot(i);
			if (!mGPUDrivenBackend.isPlanSlotDrawable(actualSlot))
			{
				continue;
			}
			if (actualActiveIndex >= mGPUDrivenValidationExpectedSlots.Count)
			{
				error = "GPU-driven active order contains extra slot. ActualSlot=" + actualSlot;
				return false;
			}
			if (expected[actualActiveIndex] != actualSlot)
			{
				error = "GPU-driven order mismatch at active token " + actualActiveIndex + ". ExpectedSlot=" + expected[actualActiveIndex] + " ActualSlot=" + actualSlot;
				return false;
			}
			++actualActiveIndex;
		}
		if (actualActiveIndex != mGPUDrivenValidationExpectedSlots.Count)
		{
			error = "GPU-driven active order count mismatch. Expected=" + mGPUDrivenValidationExpectedSlots.Count + " Actual=" + actualActiveIndex + " Persistent=" + persistentCount;
			return false;
		}
		return mGPUDrivenBackend.validateActiveOrder(this, out error);
	}

	public bool validateTrackedTransformsForBenchmark(out string error)
	{
		error = null;
		if (!mTransformMatrices.IsCreated || mTransformMatrices.Length < mRenderers.Count)
		{
			error = "Tracked transform storage is unavailable.";
			return false;
		}
		if (mTransformSortECS == null || mTransformSortECS.Count != mRenderers.Count)
		{
			error = "EasyECS transform-sort row count mismatch. ECS=" + (mTransformSortECS != null ? mTransformSortECS.Count : -1) +
				" Renderers=" + mRenderers.Count;
			return false;
		}
		const float epsilon = 0.0001f;
		var runtimeActive = mTransformSortECS.getRuntimeActiveColumn();
		for (int i = 0; i < mRenderers.Count; ++i)
		{
			FastSpriteRenderer renderer = mRenderers[i];
			if (renderer == null)
			{
				continue;
			}
			byte expectedRuntimeActive = renderer.mRuntimeActiveState ? (byte)1 : (byte)0;
			if (runtimeActive[i] != expectedRuntimeActive)
			{
				error = "EasyECS runtime-active mirror mismatch at renderer slot " + i +
					" Renderer#" + FastUnityObjectIDUtility.getDebugID(renderer) + " ECS=" + runtimeActive[i] +
					" Managed=" + expectedRuntimeActive;
				return false;
			}
			Matrix4x4 expected = renderer.transform.localToWorldMatrix;
			Matrix4x4 actual = getTrackedLocalToWorldMatrix(renderer);
			if (!matrixApproximatelyEqual3x4(expected, actual, epsilon))
			{
				error = "Tracked transform mismatch at renderer slot " + i + " Renderer#" + FastUnityObjectIDUtility.getDebugID(renderer);
				return false;
			}
		}
		return true;
	}

	private static bool matrixApproximatelyEqual3x4(Matrix4x4 left, Matrix4x4 right, float epsilon)
	{
		return Mathf.Abs(left.m00 - right.m00) <= epsilon && Mathf.Abs(left.m01 - right.m01) <= epsilon &&
			Mathf.Abs(left.m02 - right.m02) <= epsilon && Mathf.Abs(left.m03 - right.m03) <= epsilon &&
			Mathf.Abs(left.m10 - right.m10) <= epsilon && Mathf.Abs(left.m11 - right.m11) <= epsilon &&
			Mathf.Abs(left.m12 - right.m12) <= epsilon && Mathf.Abs(left.m13 - right.m13) <= epsilon &&
			Mathf.Abs(left.m20 - right.m20) <= epsilon && Mathf.Abs(left.m21 - right.m21) <= epsilon &&
			Mathf.Abs(left.m22 - right.m22) <= epsilon && Mathf.Abs(left.m23 - right.m23) <= epsilon;
	}

	public bool validatePhysicalActiveReservationsForBenchmark(out string error)
	{
		if (mGPUDrivenRuntimeActive)
		{
			if (!isGPUDrivenPlanCurrent())
			{
				updateGPUDrivenProcedural();
			}
			return mGPUDrivenBackend.validateActiveOrder(this, out error);
		}
		error = null;
		return true;
	}

	internal void registerSortGroup(FastSpriteSortGroup group)
	{
		if (group != null)
		{
			// Planner handles are backend-instance local. A group can survive system disable/
			// rebind, so never carry a direct cache reference across registration boundaries.
			group.mGPUPlanCachedSpan = null;
		}
		mSortGroupSortManager.register(group);
		if (group != null && group.usesRootOnlyTransformTracking())
		{
			addRootTrackingGroup(group);
		}
		++mSortGroupContentRevision;
		++mSortGroupOrderRevision;
	}

	internal void unregisterSortGroup(FastSpriteSortGroup group)
	{
		removeRootTrackingGroup(group);
		mSortGroupSortManager.unregister(group);
		if (group != null)
		{
			group.mGPUPlanCachedSpan = null;
		}
		++mSortGroupContentRevision;
		++mSortGroupOrderRevision;
	}

	internal void notifySortGroupOrderDirty(FastSpriteSortGroup group)
	{
		mSortGroupSortManager.markDirty(group);
	}

	internal void notifySortGroupContentDirty()
	{
		++mSortGroupContentRevision;
	}

	public static void clearSpriteMeshCache()
	{
		FastSpriteGeometryUtility.clearSpriteMeshCache();
	}

	public void setSortMode(FastSpriteSortMode mode)
	{
		if (mSortMode == mode)
		{
			return;
		}
		mSortMode = mode;
		invalidateRendererSortCaches();
		foreach (FastSpriteBatch batch in mBatches.Values)
		{
			batch.markSortDirty();
		}
	}

	public void setSortCamera(Camera camera)
	{
		if (mSortCamera == camera)
		{
			return;
		}
		mSortCamera = camera;
		mCameraSortStateValid = false;
		invalidateRendererSortCaches();
		foreach (FastSpriteBatch batch in mBatches.Values)
		{
			batch.markSortDirty();
		}
	}

	public Material getDefaultMaterial()
	{
		if (mDefaultMaterial != null)
		{
			return mDefaultMaterial;
		}
		if (mOwnedDefaultMaterial != null)
		{
			return mOwnedDefaultMaterial;
		}
		Shader shader = Shader.Find("Sprites/Default");
		if (shader == null)
		{
			return null;
		}
		mOwnedDefaultMaterial = new Material(shader)
		{
			name = "[FastSprite] Default Material",
			hideFlags = HideFlags.HideAndDontSave,
		};
		return mOwnedDefaultMaterial;
	}

	internal bool tryRetainRendererWhileDisabled(FastSpriteRenderer renderer)
	{
		bool canRetain = mGPUDrivenRuntimeActive
			? mGPUDrivenBackend.isPlanValid()
			: mResolvedBackendMode == FastSpriteBackendMode.CompatMesh;
		if (!Application.isPlaying || !canRetain || renderer == null || !renderer.gameObject.activeInHierarchy ||
			renderer.mSystemOwner != this || renderer.mBatch == null || renderer.mRetainedWhileDisabled)
		{
			return false;
		}
		FastSpriteBatch batch = renderer.mBatch;
		if (!batch.canRetainInactiveRenderer(renderer))
		{
			return false;
		}
		bool batchWasActive = batch.getCount() > 0;
		if (!batch.suspendRetainedRenderer(renderer))
		{
			return false;
		}
		if (batchWasActive && batch.getCount() == 0)
		{
			mActiveBatchCount = Mathf.Max(0, mActiveBatchCount - 1);
		}
		if (mGPUDrivenRuntimeActive)
		{
			mGPUDrivenBackend.notifyActiveStateChanged(renderer);
		}
		syncRuntimeActiveECS(renderer);
		// GPU mode updates only the instance active flag. Compat Mesh patches its stable index
		// slice when possible and falls back to order rebuild only when the draw map is stale.
		return true;
	}

	internal void cancelRetainedRenderer(FastSpriteRenderer renderer)
	{
		if (renderer == null || renderer.mSystemOwner != this || !renderer.mRetainedWhileDisabled)
		{
			return;
		}
		unregisterInternal(renderer);
		renderer.mRetainedWhileDisabled = false;
	}

	internal bool resumeRetainedRenderer(FastSpriteRenderer renderer)
	{
		if (renderer == null || renderer.mSystemOwner != this || renderer.mBatch == null || !renderer.mRetainedWhileDisabled)
		{
			return false;
		}
		FastSpriteBatch batch = renderer.mBatch;
		bool batchWasDormant = batch.getCount() == 0;
		if (!batch.resumeRetainedRenderer(renderer))
		{
			return false;
		}
		renderer.mRetainedWhileDisabled = false;
		if (batchWasDormant && batch.getCount() > 0)
		{
			++mActiveBatchCount;
		}
		if (mGPUDrivenRuntimeActive)
		{
			mGPUDrivenBackend.notifyActiveStateChanged(renderer);
		}
		syncRuntimeActiveECS(renderer);
		// OnEnable submits any Transform/property changes separately; visibility itself only
		// restores the stable slot to the current backend's draw order.
		return true;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void syncRuntimeActiveECS(FastSpriteRenderer renderer)
	{
		if (renderer == null || renderer.mSystemOwner != this || mTransformSortECS == null)
		{
			return;
		}
		int index = renderer.mSystemRendererIndex;
		if ((uint)index >= (uint)mRenderers.Count || index >= mTransformSortECS.Count ||
			!ReferenceEquals(mRenderers[index], renderer))
		{
			return;
		}
		mTransformSortECS.getRuntimeActiveColumn()[index] = renderer.mRuntimeActiveState ? (byte)1 : (byte)0;
	}

	public void flushNow()
	{
		if (mUpdatePaused)
		{
			return;
		}
		if (!isActiveAndEnabled)
		{
			return;
		}
#if UNITY_EDITOR
		if (!Application.isPlaying && !mRenderInEditMode)
		{
			return;
		}
#endif
		updateSystem();
	}

	internal static void registerRenderer(FastSpriteRenderer renderer)
	{
		if (renderer == null)
		{
			return;
		}
		FastSpriteRenderSystem system = findSystemFor();
		if (system != null)
		{
			system.registerInternal(renderer);
			return;
		}
		if (!sPendingRenderers.Contains(renderer))
		{
			sPendingRenderers.Add(renderer);
		}
	}

	internal static void unregisterRenderer(FastSpriteRenderer renderer)
	{
		if (renderer == null)
		{
			return;
		}
		sPendingRenderers.Remove(renderer);
		if (renderer.mSystemOwner != null)
		{
			renderer.mSystemOwner.unregisterInternal(renderer);
			return;
		}
		if (renderer.mBatch != null)
		{
			FastSpriteRenderSystem owner = renderer.mBatch.getOwner();
			if (owner != null)
			{
				owner.unregisterInternal(renderer);
				return;
			}
		}
		for (int i = 0; i < sSystems.Count; ++i)
		{
			if (sSystems[i] != null)
			{
				sSystems[i].unregisterInternal(renderer);
			}
		}
	}

	internal static void notifyRendererDirty(FastSpriteRenderer renderer, FastSpriteDirtyFlags flags)
	{
		if (renderer == null || !renderer.mRuntimeActiveState)
		{
			return;
		}
		if (renderer.mSystemOwner == null || renderer.mBatch == null)
		{
			registerRenderer(renderer);
		}
		FastSpriteRenderSystem owner = renderer.mSystemOwner;
		if (owner != null)
		{
			owner.notifyGPUDrivenRendererDirty(renderer, flags);
		}
		if (owner != null && (flags & (FastSpriteDirtyFlags.Vertex | FastSpriteDirtyFlags.Geometry | FastSpriteDirtyFlags.Topology | FastSpriteDirtyFlags.Sorting)) != 0)
		{
			renderer.getLocalSortPointCached();
		}
		if ((flags & FastSpriteDirtyFlags.Batch) != 0)
		{
			if (owner != null)
			{
				owner.queueMembershipDirty(renderer);
			}
		}
	}

	internal FastSpriteBatchKey getBatchKey(FastSpriteRenderer renderer)
	{
		Material material = renderer.getMaterial() != null ? renderer.getMaterial() : getDefaultMaterial();
		Texture texture = renderer.getSprite() != null && renderer.getSprite().texture != null
			? renderer.getSprite().texture
			: Texture2D.whiteTexture;
		if (renderer.getSortGroup() != null)
		{
			return new FastSpriteBatchKey(0, 0, material, texture, true);
		}
		return new FastSpriteBatchKey(
			renderer.getSortingLayerID(),
			renderer.getSortingOrder(),
			material,
			texture);
	}

	internal Matrix4x4 getWorldToBatchMatrix()
	{
		return transform.worldToLocalMatrix;
	}

	internal float getSortValue(FastSpriteRenderer renderer)
	{
		if (renderer == null || mSortMode == FastSpriteSortMode.Registration)
		{
			return 0.0f;
		}
		Vector3 worldPoint = renderer.getWorldSortPoint();
		if (mSortMode == FastSpriteSortMode.YAxis)
		{
			return worldPoint.y;
		}
		Camera camera = getSortCamera();
		return camera != null ? getTransparencySortValue(camera, worldPoint) : 0.0f;
	}

	private static TransparencySortMode getEffectiveTransparencySortMode(Camera camera)
	{
		if (camera == null)
		{
			return TransparencySortMode.Default;
		}
		TransparencySortMode mode = camera.transparencySortMode;
		if (mode == TransparencySortMode.Default)
		{
			mode = camera.orthographic ? TransparencySortMode.Orthographic : TransparencySortMode.Perspective;
		}
		return mode;
	}

	private static float getTransparencySortValue(Camera camera, Vector3 worldPoint)
	{
		TransparencySortMode mode = getEffectiveTransparencySortMode(camera);
		if (mode == TransparencySortMode.CustomAxis)
		{
			return Vector3.Dot(worldPoint, GraphicsSettings.transparencySortAxis);
		}
		if (mode == TransparencySortMode.Orthographic)
		{
			// Camera translation adds the same constant to every orthographic depth and
			// therefore cannot change relative transparent ordering. Cache a translation-
			// invariant scalar so camera scrolling does not invalidate every renderer.
			return Vector3.Dot(worldPoint, camera.transform.forward);
		}
		return (worldPoint - camera.transform.position).sqrMagnitude;
	}

	internal FastSortContext buildSortContext()
	{
		FastSortContext context = new()
		{
			mMode = mSortMode,
		};
		if (mSortMode != FastSpriteSortMode.CameraDistance)
		{
			return context;
		}
		Camera camera = getSortCamera();
		if (camera == null)
		{
			return context;
		}
		TransparencySortMode mode = getEffectiveTransparencySortMode(camera);
		Transform cameraTransform = camera.transform;
		context.mHasCamera = true;
		context.mTransparencyMode = mode;
		context.mCameraPosition = cameraTransform.position;
		context.mCameraForward = cameraTransform.forward;
		context.mCustomAxis = GraphicsSettings.transparencySortAxis;
		return context;
	}

	internal static float getSortValueFromMatrix(FastSpriteRenderer renderer, Matrix4x4 localToWorld, FastSortContext context)
	{
		if (renderer == null || context.mMode == FastSpriteSortMode.Registration)
		{
			return 0.0f;
		}
		Vector3 localPoint = renderer.getLocalSortPointCached();
		// Pivot sorting and centered Sprite pivots use the local origin. Avoid a full
		// Matrix4x4.MultiplyPoint3x4 for the overwhelmingly common zero-point case;
		// the world point is exactly the matrix translation column.
		Vector3 worldPoint = localPoint.x == 0.0f && localPoint.y == 0.0f && localPoint.z == 0.0f
			? new Vector3(localToWorld.m03, localToWorld.m13, localToWorld.m23)
			: localToWorld.MultiplyPoint3x4(localPoint);
		if (context.mMode == FastSpriteSortMode.YAxis)
		{
			return worldPoint.y;
		}
		if (!context.mHasCamera)
		{
			return 0.0f;
		}
		if (context.mTransparencyMode == TransparencySortMode.CustomAxis)
		{
			return Vector3.Dot(worldPoint, context.mCustomAxis);
		}
		if (context.mTransparencyMode == TransparencySortMode.Orthographic)
		{
			return Vector3.Dot(worldPoint, context.mCameraForward);
		}
		return (worldPoint - context.mCameraPosition).sqrMagnitude;
	}

	internal void getGPUDrivenTransform(FastSpriteRenderer renderer, out Matrix4x4 localMatrix, out int rootIndex)
	{
		rootIndex = -1;
		if (renderer != null && renderer.mSystemOwner == this)
		{
			int childSlot = renderer.mRootTrackingChildIndex;
			if ((uint)childSlot < (uint)mRootTrackedChildren.Count &&
				mRootChildECS != null && childSlot < mRootChildECS.Count &&
				mRootChildLocalToRoot.IsCreated && childSlot < mRootChildLocalToRoot.Length)
			{
				int index = mRootChildECS.getValueColumn()[childSlot];
				if ((uint)index < (uint)mRootTransformGroups.Count)
				{
					localMatrix = mRootChildLocalToRoot[childSlot];
					rootIndex = index;
					return;
				}
			}
		}
		localMatrix = getTrackedLocalToWorldMatrix(renderer);
	}

	internal Matrix4x4 getTrackedLocalToWorldMatrix(FastSpriteRenderer renderer)
	{
		if (renderer != null && renderer.mSystemOwner == this)
		{
			int childSlot = renderer.mRootTrackingChildIndex;
			if ((uint)childSlot < (uint)mRootTrackedChildren.Count &&
				mRootChildECS != null && childSlot < mRootChildECS.Count &&
				mRootChildLocalToRoot.IsCreated && childSlot < mRootChildLocalToRoot.Length &&
				mRootTransformMatrices.IsCreated)
			{
				int rootIndex = mRootChildECS.getValueColumn()[childSlot];
				if ((uint)rootIndex < (uint)mRootTransformGroups.Count && rootIndex < mRootTransformMatrices.Length)
				{
					return mRootTransformMatrices[rootIndex] * mRootChildLocalToRoot[childSlot];
				}
			}
			int index = renderer.mSystemRendererIndex;
			if (index >= 0 && mTransformMatrices.IsCreated && index < mTransformMatrices.Length)
			{
				return mTransformMatrices[index];
			}
		}
		return renderer != null ? renderer.transform.localToWorldMatrix : Matrix4x4.identity;
	}

	internal bool hasRootTransformChangesThisFrame()
	{
		return mRootChangedThisFrameCount > 0;
	}

	private void updateGPURootMatrixBuffer()
	{
		// Only the primary EasyECS/GPU-driven path consumes root matrices now.
		// Logical Mesh fallback composes rootMatrix * localToRoot on CPU.
		if (!mGPUDrivenRuntimeActive || mRootTransformGroups.Count == 0)
		{
			mGPURootMatrixUploadedCount = 0;
			return;
		}
		int count = mRootTransformGroups.Count;
		int required = Mathf.NextPowerOfTwo(Mathf.Max(count, 4));
		if (mGPURootMatrixBuffer == null || mGPURootMatrixCapacity < required)
		{
			mGPURootMatrixBuffer?.Dispose();
			mGPURootMatrixBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, required, 64);
			mGPURootMatrixCapacity = required;
			mGPURootMatrixUploadedCount = 0;
		}
		if (mRootChangedThisFrameCount > 0 || mGPURootMatrixUploadedCount != count)
		{
			mGPURootMatrixBuffer.SetData(mRootTransformMatrices, 0, 0, count);
			mGPURootMatrixUploadedCount = count;
		}
	}

	private static FastSpriteRenderSystem findSystemFor()
	{
		for (int i = 0; i < sSystems.Count; ++i)
		{
			FastSpriteRenderSystem system = sSystems[i];
			if (system != null && system.isActiveAndEnabled)
			{
				return system;
			}
		}
		return null;
	}

	private void registerInternal(FastSpriteRenderer renderer)
	{
		if (renderer == null)
		{
			return;
		}
		if (renderer.mSystemOwner == this)
		{
			int currentIndex = renderer.mSystemRendererIndex;
			if (currentIndex >= 0 && currentIndex < mRenderers.Count && mRenderers[currentIndex] == renderer)
			{
				return;
			}
		}
		else if (renderer.mSystemOwner != null)
		{
			renderer.mSystemOwner.unregisterInternal(renderer);
		}

		int rendererIndex = mRenderers.Count;
		ensureTransformTrackingCapacity(rendererIndex + 1);
		renderer.mSystemOwner = this;
		renderer.mSystemRendererIndex = rendererIndex;
		if (renderer.mSortGroup != null)
		{
			renderer.mSortGroup.bindOwner(this);
		}
		renderer.mMembershipDirtyQueued = false;
		mRenderers.Add(renderer);
		mTransformAccessArray.Add(renderer.transform);
		mTransformMatrices[rendererIndex] = renderer.transform.localToWorldMatrix;
		Vector3 localSortPoint = renderer.getLocalSortPointCached();
		mTransformSortECS.Add(new FastSpriteTransformSortData(localSortPoint.x, localSortPoint.y, localSortPoint.z, renderer.mRuntimeActiveState));
		renderer.mRegistrationSequence = ++mRegistrationSequence;
		renderer.mTransformCacheValid = true;
		renderer.invalidateSortValueCache();
		renderer.mDirtyFlags |= FastSpriteDirtyFlags.All;
		ensureMembership(renderer);
		updateRendererTransformTracking(renderer);
		if (mGPUDrivenRuntimeActive)
		{
			mGPUDrivenBackend.registerRenderer(renderer);
		}
	}

	private void unregisterInternal(FastSpriteRenderer renderer)
	{
		if (renderer == null)
		{
			return;
		}
		mGPUDrivenBackend.unregisterRenderer(renderer);
		removeRendererTransformTracking(renderer);
		if (renderer.getSortGroup() != null)
		{
			++mSortGroupContentRevision;
		}

		if (renderer.mSystemOwner == this)
		{
			int index = renderer.mSystemRendererIndex;
			if (index < 0 || index >= mRenderers.Count || mRenderers[index] != renderer)
			{
				// Domain-reload recovery only. Normal unregister is O(1).
				index = mRenderers.IndexOf(renderer);
			}
			if (index >= 0)
			{
				int lastIndex = mRenderers.Count - 1;
				if (index != lastIndex)
				{
					FastSpriteRenderer moved = mRenderers[lastIndex];
					mRenderers[index] = moved;
					mTransformMatrices[index] = mTransformMatrices[lastIndex];
					if (mTransformSortECS != null && lastIndex < mTransformSortECS.Count)
					{
						var sortPointX = mTransformSortECS.getSortPointXColumn();
						var sortPointY = mTransformSortECS.getSortPointYColumn();
						var sortPointZ = mTransformSortECS.getSortPointZColumn();
						var sortValue = mTransformSortECS.getSortValueColumn();
						var transformChanged = mTransformSortECS.getTransformChangedColumn();
						var runtimeActive = mTransformSortECS.getRuntimeActiveColumn();

						sortPointX[index] = sortPointX[lastIndex];
						sortPointY[index] = sortPointY[lastIndex];
						sortPointZ[index] = sortPointZ[lastIndex];
						sortValue[index] = sortValue[lastIndex];
						transformChanged[index] = transformChanged[lastIndex];
						runtimeActive[index] = runtimeActive[lastIndex];
					}
					if (moved != null)
					{
						moved.mSystemRendererIndex = index;
						int independentIndex = moved.mIndependentTransformTrackingIndex;
						if ((uint)independentIndex < (uint)mIndependentTransformRenderers.Count &&
							mIndependentTransformECS != null && independentIndex < mIndependentTransformECS.Count)
						{
							mIndependentTransformECS.getValueColumn()[independentIndex] = index;
						}
					}
				}
				if (mTransformAccessArray.isCreated && index < mTransformAccessArray.length)
				{
					mTransformAccessArray.RemoveAtSwapBack(index);
				}
				mRenderers.RemoveAt(lastIndex);
				if (mTransformSortECS != null && lastIndex < mTransformSortECS.Count)
				{
					mTransformSortECS.RemoveAt(lastIndex);
				}
			}
			renderer.mSystemOwner = null;
			renderer.mSystemRendererIndex = -1;
			renderer.mMembershipDirtyQueued = false;
		}

		if (renderer.mBatch != null && renderer.mBatch.getOwner() == this)
		{
			FastSpriteBatch batch = renderer.mBatch;
			bool wasActive = batch.getCount() > 0;
			batch.remove(renderer);
			renderer.mBatch = null;
			renderer.mBatchElementIndex = -1;
			if (wasActive && batch.getCount() == 0)
			{
				mActiveBatchCount = Mathf.Max(0, mActiveBatchCount - 1);
			}
		}
	}

	private bool ensureMembership(FastSpriteRenderer renderer)
	{
		if (renderer != null)
		{
			renderer.mMembershipDirtyQueued = false;
		}
		if (renderer == null || !renderer.isActiveAndEnabled)
		{
			return false;
		}

		FastSpriteBatch oldBatch = renderer.mBatch;
		FastSpriteDirtyFlags dirtyFlags = renderer.peekDirtyFlags();
		FastSpriteBatchKey derivedOrderOnlyKey = default;
		bool hasDerivedOrderOnlyKey = false;

		FastSpriteDirtyFlags nonOrderDirty = dirtyFlags &
			~(FastSpriteDirtyFlags.Batch | FastSpriteDirtyFlags.SortingOrderBatch);
		if (oldBatch != null && oldBatch.getOwner() == this &&
			(dirtyFlags & FastSpriteDirtyFlags.SortingOrderBatch) != 0 &&
			nonOrderDirty == FastSpriteDirtyFlags.None && !renderer.mBatchDirtyQueued)
		{
			FastSpriteBatchKey oldKey = oldBatch.getKey();
			int targetSortingOrder = renderer.getSortingOrder();
			if (oldKey.mSortingOrder == targetSortingOrder)
			{
				renderer.clearDirtyFlags(FastSpriteDirtyFlags.Batch | FastSpriteDirtyFlags.SortingOrderBatch);
				return false;
			}

			derivedOrderOnlyKey = oldKey.withSortingOrder(targetSortingOrder);
			hasDerivedOrderOnlyKey = true;
			if (mBatches.TryGetValue(derivedOrderOnlyKey, out FastSpriteBatch cachedDestination) &&
				cachedDestination != null && cachedDestination != oldBatch)
			{
				bool oldWasActive = oldBatch.getCount() > 0;
				bool destinationWasDormant = cachedDestination.getCount() == 0;
				if (oldBatch.tryFastTransferMembershipTo(cachedDestination, renderer))
				{
					if (oldWasActive && oldBatch.getCount() == 0)
					{
						mActiveBatchCount = Mathf.Max(0, mActiveBatchCount - 1);
					}
					if (destinationWasDormant && cachedDestination.getCount() > 0)
					{
						++mActiveBatchCount;
					}
					renderer.clearDirtyFlags(FastSpriteDirtyFlags.Batch | FastSpriteDirtyFlags.SortingOrderBatch);
					++mLastProfile.mSortingOrderFastTransferCount;
					return true;
				}
			}
		}

		// General Batch mutations still use the full renderer-derived key. This path
		// covers Material/Texture/SortingLayer changes, first-time destination batches,
		// variable topology, and mixed dirty flags where correctness needs a rebuild.
		FastSpriteBatchKey key = hasDerivedOrderOnlyKey ? derivedOrderOnlyKey : getBatchKey(renderer);
		if (renderer.mBatch != null && renderer.mBatch.getOwner() == this && renderer.mBatch.getKey().Equals(key))
		{
			renderer.clearDirtyFlags(FastSpriteDirtyFlags.Batch | FastSpriteDirtyFlags.SortingOrderBatch);
			return false;
		}

		if (renderer.mBatch != null)
		{
			oldBatch = renderer.mBatch;
			bool wasActive = oldBatch.getCount() > 0;
			oldBatch.remove(renderer);
			renderer.mBatch = null;
			renderer.mBatchElementIndex = -1;
			if (wasActive && oldBatch.getCount() == 0)
			{
				mActiveBatchCount = Mathf.Max(0, mActiveBatchCount - 1);
			}
		}

		if (!mBatches.TryGetValue(key, out FastSpriteBatch batch))
		{
			batch = new FastSpriteBatch(this, key);
			mBatches.Add(key, batch);
			++mBatchSetRevision;
		}
		bool wasDormant = batch.getCount() == 0;
		renderer.mBatch = batch;
		batch.add(renderer);
		if (wasDormant && batch.getCount() > 0)
		{
			++mActiveBatchCount;
		}
		pruneDormantBatchesIfNeeded();

		if (renderer.getSortGroup() != null)
		{
			++mSortGroupContentRevision;
		}
		renderer.clearDirtyFlags(FastSpriteDirtyFlags.Batch | FastSpriteDirtyFlags.SortingOrderBatch);
		return true;
	}

	private void pruneDormantBatchesIfNeeded()
	{
		if (mBatches.Count <= MAX_CACHED_BATCH_COUNT)
		{
			return;
		}
		mDormantBatchPruneKeys.Clear();
		foreach (KeyValuePair<FastSpriteBatchKey, FastSpriteBatch> pair in mBatches)
		{
			if (pair.Value != null && pair.Value.getCount() == 0)
			{
				mDormantBatchPruneKeys.Add(pair.Key);
				if (mBatches.Count - mDormantBatchPruneKeys.Count <= TARGET_CACHED_BATCH_COUNT)
				{
					break;
				}
			}
		}
		for (int i = 0; i < mDormantBatchPruneKeys.Count; ++i)
		{
			FastSpriteBatchKey pruneKey = mDormantBatchPruneKeys[i];
			if (mBatches.TryGetValue(pruneKey, out FastSpriteBatch dormant) && dormant != null && dormant.getCount() == 0)
			{
				mBatches.Remove(pruneKey);
				++mBatchSetRevision;
				dormant.Dispose();
			}
		}
		mDormantBatchPruneKeys.Clear();
	}
	private void LateUpdate()
	{
		flushNow();
	}

	private void updateSystem()
	{
		bool collectProfile = mCollectProfileStats;
		long totalStart = collectProfile ? System.Diagnostics.Stopwatch.GetTimestamp() : 0L;
		mLastProfile = default;
		mLastProfile.mTransformTrackingAddCount = mPendingTransformTrackingAddCount;
		mLastProfile.mTransformTrackingRemoveCount = mPendingTransformTrackingRemoveCount;
		mPendingTransformTrackingAddCount = 0;
		mPendingTransformTrackingRemoveCount = 0;

		bool systemTransformChanged = pollSystemTransformChanged();
		bool cameraSortChanged = pollCameraSortChanged();
		double transformScanCPU = scheduleRootTransformTracking(systemTransformChanged, collectProfile);

		long sortGroupStart = collectProfile ? System.Diagnostics.Stopwatch.GetTimestamp() : 0L;
		if (mSortGroupSortManager.update())
		{
			++mSortGroupOrderRevision;
		}
		if (collectProfile)
		{
			FastSpriteChunkedSortManager.FrameStats stats = mSortGroupSortManager.getLastStats();
			mLastProfile.mSortGroupUpdateMS = elapsedProfileMS(sortGroupStart);
			mLastProfile.mSortGroupDirtyCount = stats.mDirtyCount;
			mLastProfile.mSortGroupNoMoveCount = stats.mNoMoveCount;
			mLastProfile.mSortGroupLocalRepairCount = stats.mLocalRepairCount;
			mLastProfile.mSortGroupAdjacentChunkMoveCount = stats.mAdjacentChunkMoveCount;
			mLastProfile.mSortGroupGlobalRelocateCount = stats.mGlobalRelocateCount;
			mLastProfile.mSortGroupMovedSlots = stats.mMovedSlots;
			mLastProfile.mSortGroupBatchRepairLayerCount = stats.mBatchRepairLayerCount;
			mLastProfile.mSortGroupBatchRepairNodeCount = stats.mBatchRepairNodeCount;
			mLastProfile.mSortGroupParallelSortNodeCount = stats.mParallelSortNodeCount;
			mLastProfile.mSortGroupParallelSortRunCount = stats.mParallelSortRunCount;
			mLastProfile.mSortGroupParallelSortDispatchCount = stats.mParallelSortDispatchCount;
		}

		long membershipStart = collectProfile ? System.Diagnostics.Stopwatch.GetTimestamp() : 0L;
		processMembershipDirtyQueue(collectProfile);
		if (collectProfile)
		{
			mLastProfile.mMembershipMS = elapsedProfileMS(membershipStart);
		}

		long scanFinishStart = collectProfile ? System.Diagnostics.Stopwatch.GetTimestamp() : 0L;
		finishRootTransformTracking(collectProfile);
		if (!mGPUDrivenRuntimeActive && mResolvedBackendMode == FastSpriteBackendMode.CompatMesh)
		{
			propagateRootTransformChangesToCompatMesh();
		}
		updateGPURootMatrixBuffer();
		pollIndependentRendererTransforms(systemTransformChanged, collectProfile);
		if (systemTransformChanged)
		{
			refreshNonPolledRenderersForSystemTransform(collectProfile);
		}
		if (collectProfile)
		{
			transformScanCPU += elapsedProfileMS(scanFinishStart);
			mLastProfile.mRendererScanMS = transformScanCPU;
		}

		if (cameraSortChanged && mSortMode == FastSpriteSortMode.CameraDistance)
		{
			invalidateRendererSortCaches();
			foreach (FastSpriteBatch batch in mBatches.Values)
			{
				batch?.markSortDirty();
			}
		}

		mLastVertexCount = 0;
		mLastIndexCount = 0;
		mLastVertexUploadBytes = 0;
		mLastIndexUploadBytes = 0;

		bool gpuDrivenAuthoritative = mGPUDrivenRuntimeActive;
		mGPUDrivenDirectStreamFrame = gpuDrivenAuthoritative;
		if (collectProfile && gpuDrivenAuthoritative)
		{
			mLastProfile.mGPUDrivenActive = 1;
		}

		long batchStart = collectProfile ? System.Diagnostics.Stopwatch.GetTimestamp() : 0L;
		foreach (FastSpriteBatch batch in mBatches.Values)
		{
			if (batch == null || (batch.getCount() == 0 && !batch.hasRetainedInactiveElements()))
			{
				continue;
			}
			if (gpuDrivenAuthoritative)
			{
				batch.updateGPUDrivenOnly();
				if (collectProfile)
				{
					++mLastProfile.mGPUDrivenLogicalBatchSkipCount;
				}
			}
			else
			{
				batch.update();
			}

			mLastVertexCount += batch.getVertexCount();
			mLastIndexCount += batch.getIndexCount();
			mLastVertexUploadBytes += batch.getVertexUploadBytes();
			mLastIndexUploadBytes += batch.getIndexUploadBytes();
			if (!collectProfile)
			{
				continue;
			}

			FastSpriteBatchFrameProfile batchProfile = batch.getLastProfile();
			++mLastProfile.mBatchUpdateCount;
			mLastProfile.mTopologyMS += batchProfile.mTopologyMS;
			mLastProfile.mElementScanMS += batchProfile.mElementScanMS;
			mLastProfile.mGeometryBuildMS += batchProfile.mGeometryBuildMS;
			mLastProfile.mVertexUploadMS += batchProfile.mVertexUploadMS;
			mLastProfile.mSortIndexMS += batchProfile.mSortIndexMS;
			mLastProfile.mBoundsMS += batchProfile.mBoundsMS;
			mLastProfile.mTopologyRebuildCount += batchProfile.mTopologyRebuildCount;
			mLastProfile.mDirtyElementCount += batchProfile.mDirtyElementCount;
			mLastProfile.mUVOnlyElementCount += batchProfile.mUVOnlyElementCount;
			mLastProfile.mUVColorOnlyElementCount += batchProfile.mUVColorOnlyElementCount;
			mLastProfile.mRootMotionScannedElementCount += batchProfile.mRootMotionScannedElementCount;
			mLastProfile.mRootMotionDirtyElementCount += batchProfile.mRootMotionDirtyElementCount;
			mLastProfile.mRootMotionQuadFastPathCount += batchProfile.mRootMotionQuadFastPathCount;
			mLastProfile.mTransformChangedCount += batchProfile.mRootMotionDirtyElementCount;
			mLastProfile.mVertexUploadCallCount += batchProfile.mVertexUploadCallCount;
			mLastProfile.mIndexUploadCallCount += batchProfile.mIndexUploadCallCount;
		}
		if (collectProfile)
		{
			mLastProfile.mBatchUpdateMS = elapsedProfileMS(batchStart);
		}

		updateGPUDrivenProcedural();
		if (collectProfile)
		{
			mLastProfile.mTotalMS = elapsedProfileMS(totalStart);
		}
	}

	private bool isGPUDrivenPlanCurrent()
	{
		return mGPUDrivenBackend.isPlanValid() &&
			mGPUDrivenPlanBatchRevision == mBatchSetRevision &&
			mGPUDrivenPlanGroupRevision == mSortGroupContentRevision &&
			mGPUDrivenPlanGroupOrderRevision == mSortGroupOrderRevision &&
			mGPUDrivenPlanBatchOrderRevision == mGPUDrivenBatchOrderRevision &&
			mGPUDrivenPlanLayoutRevision == mGPUDrivenBackend.getLayoutRevision();
	}

	private void restoreLogicalMeshesFromCPU()
	{
		for (int i = 0; i < mRenderers.Count; ++i)
		{
			FastSpriteRenderer renderer = mRenderers[i];
			if (renderer == null || renderer.mSystemOwner != this)
			{
				continue;
			}
			renderer.mDirtyFlags |= FastSpriteDirtyFlags.All;
			renderer.mBatchDirtyQueued = false;
		}
		foreach (FastSpriteBatch batch in mBatches.Values)
		{
			batch?.markFullDirty();
		}
		foreach (FastSpriteBatch batch in mBatches.Values)
		{
			if (batch == null)
			{
				continue;
			}
			if (batch.getCount() > 0 || batch.hasRetainedInactiveElements())
			{
				batch.update();
				batch.submitLogicalMeshFromCPU();
			}
			batch.setRenderEnabled(true);
		}
	}

	private void collectGPUDrivenFrameStats()
	{
		if (!mCollectProfileStats)
		{
			return;
		}
		FastSpriteGPUDrivenBackend.FrameStats stats = mGPUDrivenBackend.getFrameStats();
		mLastProfile.mGPUDrivenDirtyBuildMS += stats.mDirtyBuildMS;
		mLastProfile.mGPUDrivenUploadMS += stats.mUploadMS;
		mLastProfile.mGPUDrivenCommandBuildMS += stats.mCommandBuildMS;
		mLastProfile.mGPUDrivenDrawCount = stats.mDrawCount;
		mLastProfile.mGPUDrivenInstanceCount = stats.mInstanceCount;
		mLastProfile.mGPUDrivenDirtyRendererCount += stats.mDirtyRendererCount;
		mLastProfile.mGPUDrivenHotWriteCount += stats.mHotWriteCount;
		mLastProfile.mGPUDrivenSpriteAssetCommitCount += stats.mSpriteAssetCommitCount;
		mLastProfile.mGPUDrivenGeometryUploadBytes += stats.mGeometryUploadBytes;
		mLastProfile.mGPUDrivenInstanceUploadBytes += stats.mInstanceUploadBytes;
		mLastProfile.mGPUDrivenOrderUploadBytes += stats.mOrderUploadBytes;
		mLastProfile.mGPUDrivenGeometryUploadCalls += stats.mGeometryUploadCalls;
		mLastProfile.mGPUDrivenInstanceUploadCalls += stats.mInstanceUploadCalls;
		mLastProfile.mGPUDrivenOrderUploadCalls += stats.mOrderUploadCalls;
		mLastProfile.mGPUDrivenDirtyDensityChunkDispatchCount += stats.mDirtyDensityChunkDispatchCount;
	}

	private bool updateGPUDrivenProcedural()
	{
		if (!mGPUDrivenRuntimeActive)
		{
			mGPUDrivenDirectStreamFrame = false;
			return false;
		}

		mGPUDrivenBackend.beginFrame();
		mGPUDrivenBackend.processDirtyRenderers(this);

		bool planInvalid = !mGPUDrivenBackend.isPlanValid();
		bool batchSetChanged = mGPUDrivenPlanBatchRevision != mBatchSetRevision;
		bool groupContentChanged = mGPUDrivenPlanGroupRevision != mSortGroupContentRevision;
		bool groupOrderChanged = mGPUDrivenPlanGroupOrderRevision != mSortGroupOrderRevision;
		bool batchOrderChanged = mGPUDrivenPlanBatchOrderRevision != mGPUDrivenBatchOrderRevision;
		bool layoutChanged = mGPUDrivenPlanLayoutRevision != mGPUDrivenBackend.getLayoutRevision();
		bool rebuildPlan = planInvalid || batchSetChanged || groupContentChanged || groupOrderChanged || batchOrderChanged || layoutChanged;

		if (rebuildPlan)
		{
			long planStart = mCollectProfileStats ? System.Diagnostics.Stopwatch.GetTimestamp() : 0L;
			// Pure group-order changes can reuse cached child slot spans.
			bool pureGroupOrderChange = !planInvalid && groupOrderChanged &&
				!batchSetChanged && !groupContentChanged && !batchOrderChanged && !layoutChanged;
			bool planBuilt = pureGroupOrderChange && rebuildGPUDrivenProceduralPlanCachedOrderOnly();
			if (!planBuilt)
			{
				planBuilt = rebuildGPUDrivenProceduralPlan();
			}
			if (!planBuilt)
			{
				collectGPUDrivenFrameStats();
				mGPUDrivenBackend.invalidatePlan();
				mGPUDrivenBackend.detachCommandBuffer();
				mGPUDrivenDirectStreamFrame = false;
				restoreLogicalMeshesFromCPU();
				return false;
			}
			if (mCollectProfileStats)
			{
				mLastProfile.mGPUDrivenPlanBuildMS += elapsedProfileMS(planStart);
			}
			mGPUDrivenPlanBatchRevision = mBatchSetRevision;
			mGPUDrivenPlanGroupRevision = mSortGroupContentRevision;
			mGPUDrivenPlanGroupOrderRevision = mSortGroupOrderRevision;
			mGPUDrivenPlanBatchOrderRevision = mGPUDrivenBatchOrderRevision;
			mGPUDrivenPlanLayoutRevision = mGPUDrivenBackend.getLayoutRevision();
		}

		Camera camera = getSortCamera();
		if (!mGPUDrivenBackend.uploadAndBuildCommands(camera, mGPURootMatrixBuffer, mGPURootMatrixUploadedCount))
		{
			collectGPUDrivenFrameStats();
			mGPUDrivenBackend.invalidatePlan();
			mGPUDrivenBackend.detachCommandBuffer();
			mGPUDrivenDirectStreamFrame = false;
			restoreLogicalMeshesFromCPU();
			return false;
		}

		collectGPUDrivenFrameStats();
		mGPUDrivenDirectStreamFrame = true;
		foreach (FastSpriteBatch batch in mBatches.Values)
		{
			batch?.setRenderEnabled(false);
		}
		if (mCollectProfileStats)
		{
			mLastProfile.mGPUDrivenActive = 1;
		}
		return true;
	}

	private bool rebuildGPUDrivenProceduralPlan()
	{
		mGPUDrivenBackend.beginPlan(mRenderers.Count);
		resetGPUPlanSortingLayerCache();
		mGPUPlanBatches.Clear();
		foreach (FastSpriteBatch batch1 in mBatches.Values)
		{
			if (batch1 != null &&
				!batch1.getKey().mGroupedStorage &&
				(batch1.getCount() > 0 || batch1.hasRetainedInactiveElements()))
			{
				mGPUPlanBatches.Add(batch1);
			}
		}
		mGPUPlanBatches.Sort(compareBatchOrder);
		mOrderedSortGroups.Clear();
		mSortGroupSortManager.appendOrderedGroups(mOrderedSortGroups);

		int batchCount = mGPUPlanBatches.Count;
		int groupCount = mOrderedSortGroups.Count;
		int batchIndex = 0;
		int groupIndex = 0;
		FastSpriteBatch batch = batchCount > 0 ? mGPUPlanBatches[0] : null;
		FastSpriteBatchKey batchKey = batch != null ? batch.getKey() : default;
		while (batchIndex < batchCount || groupIndex < groupCount)
		{
			FastSpriteSortGroup group = groupIndex < groupCount ? mOrderedSortGroups[groupIndex] : null;
			if (group == null)
			{
				if (!mGPUDrivenBackend.appendSlotSpan(batch, ref batch.mGPUPlanCachedSpan, batch.getGPUDrivenSlotOrder(),
					batchKey.mSortingLayerID, batchKey.mSortingOrder, true))
				{
					return false;
				}
				++batchIndex;
				batch = batchIndex < batchCount ? mGPUPlanBatches[batchIndex] : null;
				batchKey = batch != null ? batch.getKey() : default;
				continue;
			}

			int groupLayerID = group.getSortingLayerID();
			int groupOrder = group.getSortingOrder();
			bool takeGroup = batch == null || shouldTakeGroupForGPUPlan(groupLayerID, groupOrder, batchKey);
			if (takeGroup)
			{
				if (!mGPUDrivenBackend.appendSlotSpan(group, ref group.mGPUPlanCachedSpan, group.getGPUDrivenOrderedSlots(this),
					groupLayerID, groupOrder, true))
				{
					return false;
				}
				++groupIndex;
				continue;
			}

			if (!mGPUDrivenBackend.appendSlotSpan(batch, ref batch.mGPUPlanCachedSpan, batch.getGPUDrivenSlotOrder(),
				batchKey.mSortingLayerID, batchKey.mSortingOrder, true))
			{
				return false;
			}
			++batchIndex;
			batch = batchIndex < batchCount ? mGPUPlanBatches[batchIndex] : null;
			batchKey = batch != null ? batch.getKey() : default;
		}

		bool planBuilt = mGPUDrivenBackend.endPlan();
		return planBuilt;
	}

	private bool rebuildGPUDrivenProceduralPlanCachedOrderOnly()
	{
		mGPUDrivenBackend.beginCachedOrderPlan(mRenderers.Count);
		resetGPUPlanSortingLayerCache();
		mGPUPlanBatches.Clear();
		foreach (FastSpriteBatch batch1 in mBatches.Values)
		{
			if (batch1 != null &&
				!batch1.getKey().mGroupedStorage &&
				(batch1.getCount() > 0 || batch1.hasRetainedInactiveElements()))
			{
				mGPUPlanBatches.Add(batch1);
			}
		}
		mGPUPlanBatches.Sort(compareBatchOrder);
		mOrderedSortGroups.Clear();
		mSortGroupSortManager.appendOrderedGroups(mOrderedSortGroups);

		int batchCount = mGPUPlanBatches.Count;
		int groupCount = mOrderedSortGroups.Count;
		int batchIndex = 0;
		int groupIndex = 0;
		FastSpriteBatch batch = batchCount > 0 ? mGPUPlanBatches[0] : null;
		FastSpriteBatchKey batchKey = batch != null ? batch.getKey() : default;
		while (batchIndex < batchCount || groupIndex < groupCount)
		{
			FastSpriteSortGroup group = groupIndex < groupCount ? mOrderedSortGroups[groupIndex] : null;
			if (group == null)
			{
				if (!mGPUDrivenBackend.appendCachedSlotSpan(batch, ref batch.mGPUPlanCachedSpan,
					batchKey.mSortingLayerID, batchKey.mSortingOrder))
				{
					return false;
				}
				++batchIndex;
				batch = batchIndex < batchCount ? mGPUPlanBatches[batchIndex] : null;
				batchKey = batch != null ? batch.getKey() : default;
				continue;
			}

			int groupLayerID = group.getSortingLayerID();
			int groupOrder = group.getSortingOrder();
			bool takeGroup = batch == null || shouldTakeGroupForGPUPlan(groupLayerID, groupOrder, batchKey);
			if (takeGroup)
			{
				if (!mGPUDrivenBackend.appendCachedSlotSpan(group, ref group.mGPUPlanCachedSpan, groupLayerID, groupOrder))
				{
					return false;
				}
				++groupIndex;
				continue;
			}

			if (!mGPUDrivenBackend.appendCachedSlotSpan(batch, ref batch.mGPUPlanCachedSpan,
				batchKey.mSortingLayerID, batchKey.mSortingOrder))
			{
				return false;
			}
			++batchIndex;
			batch = batchIndex < batchCount ? mGPUPlanBatches[batchIndex] : null;
			batchKey = batch != null ? batch.getKey() : default;
		}

		bool planBuilt = mGPUDrivenBackend.endPlan();
		return planBuilt;
	}

	private void resetGPUPlanSortingLayerCache()
	{
		mGPUPlanSortingLayerCacheIDs.Clear();
		mGPUPlanSortingLayerCacheValues.Clear();
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private int getGPUPlanSortingLayerValue(int sortingLayerID)
	{
		var cacheIDs = mGPUPlanSortingLayerCacheIDs.getValueColumn();
		var cacheValues = mGPUPlanSortingLayerCacheValues.getValueColumn();
		for (int i = 0; i < mGPUPlanSortingLayerCacheIDs.Count; ++i)
		{
			if (cacheIDs[i] == sortingLayerID)
			{
				return cacheValues[i];
			}
		}

		int value = SortingLayer.GetLayerValueFromID(sortingLayerID);
		mGPUPlanSortingLayerCacheIDs.Add(sortingLayerID);
		mGPUPlanSortingLayerCacheValues.Add(value);
		return value;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private bool shouldTakeGroupForGPUPlan(int groupLayerID, int groupOrder, FastSpriteBatchKey batchKey)
	{
		if (groupLayerID == batchKey.mSortingLayerID)
		{
			// Exact same layer ID guarantees the same layer value. Avoid the EasyECS layer-cache
			// lookup entirely; equal sortingOrder still preserves the batch-first tie rule.
			return groupOrder < batchKey.mSortingOrder;
		}

		int groupLayer = getGPUPlanSortingLayerValue(groupLayerID);
		int batchLayer = getGPUPlanSortingLayerValue(batchKey.mSortingLayerID);
		if (groupLayer != batchLayer)
		{
			return groupLayer < batchLayer;
		}
		return groupOrder < batchKey.mSortingOrder;
	}

	private static int compareGroupToBatch(FastSpriteSortGroup group, FastSpriteBatch batch)
	{
		FastSpriteBatchKey key = batch.getKey();
		int groupLayer = SortingLayer.GetLayerValueFromID(group.getSortingLayerID());
		int batchLayer = SortingLayer.GetLayerValueFromID(key.mSortingLayerID);
		if (groupLayer != batchLayer)
		{
			return groupLayer < batchLayer ? -1 : 1;
		}
		if (group.getSortingOrder() != key.mSortingOrder)
		{
			return group.getSortingOrder() < key.mSortingOrder ? -1 : 1;
		}
		return 1;
	}

	private static int compareBatchOrder(FastSpriteBatch left, FastSpriteBatch right)
	{
		if (ReferenceEquals(left, right))
		{
			return 0;
		}
		if (left == null)
		{
			return -1;
		}
		if (right == null)
		{
			return 1;
		}
		FastSpriteBatchKey a = left.getKey();
		FastSpriteBatchKey b = right.getKey();
		if (a.mSortingLayerID != b.mSortingLayerID)
		{
			int layerA = SortingLayer.GetLayerValueFromID(a.mSortingLayerID);
			int layerB = SortingLayer.GetLayerValueFromID(b.mSortingLayerID);
			if (layerA != layerB)
			{
				return layerA < layerB ? -1 : 1;
			}
		}
		if (a.mSortingOrder != b.mSortingOrder)
		{
			return a.mSortingOrder < b.mSortingOrder ? -1 : 1;
		}
		if (a.mMaterialID != b.mMaterialID)
		{
			return a.mMaterialID < b.mMaterialID ? -1 : 1;
		}
		if (a.mTextureID != b.mTextureID)
		{
			return a.mTextureID < b.mTextureID ? -1 : 1;
		}
		return 0;
	}

	private unsafe double scheduleRootTransformTracking(bool systemTransformChanged, bool collectProfile)
	{
		mRootChangedThisFrameCount = 0;
		int rootCount = mRootTransformGroups.Count;
		if (rootCount == 0)
		{
			mRootTrackingJobScheduled = false;
			return 0.0;
		}
		ensureRootTrackingNativeCapacity(rootCount, mRootTrackedChildren.Count);
		var rootChangedFlags = mRootTransformECS.getValueColumn();
		for (int i = 0; i < rootCount; ++i)
		{
			rootChangedFlags[i] = 0;
		}
		mRootTransformChangedCount[0] = 0;

		bool useBurst = BurstCompiler.IsEnabled && mRootTransformECS != null &&
			Byte_ECSList.IsUnsafeBackend &&
			(systemTransformChanged || rootCount >= ROOT_TRANSFORM_JOB_MIN_COUNT);
		long start = collectProfile ? System.Diagnostics.Stopwatch.GetTimestamp() : 0L;
		if (!useBurst)
		{
			mRootTrackingJobScheduled = false;
			if (collectProfile)
			{
				mLastProfile.mRootTransformCheckedCount += rootCount;
			}
			pollRootTransformsMainThreadFallback(collectProfile);
			return collectProfile ? elapsedProfileMS(start) : 0.0;
		}

		Byte_ECSList.BurstView rootTrackingView = mRootTransformECS.GetBurstView();
		DetectRootTransformJob rootJob = new()
		{
			mPreviousMatrices = mRootTransformMatrices,
			mTracking = rootTrackingView,
			mChangedRootCount = (int*)NativeArrayUnsafeUtility.GetUnsafePtr(mRootTransformChangedCount),
			mForceChanged = systemTransformChanged ? (byte)1 : (byte)0,
		};
		JobHandle dependency = mRootTransformECS.GetBurstDependency();
		mRootTrackingJobHandle = rootJob.ScheduleReadOnly(mRootTransformAccessArray, 64, dependency);
		mRootTransformECS.RegisterBurstJob(mRootTrackingJobHandle);
		mRootTrackingJobScheduled = true;
		if (collectProfile)
		{
			mLastProfile.mRootTransformCheckedCount += rootCount;
			mRootTrackingScheduleCPUTimeMS = elapsedProfileMS(start);
			return mRootTrackingScheduleCPUTimeMS;
		}
		return 0.0;
	}

	private void finishRootTransformTracking(bool collectProfile)
	{
		if (!mRootTrackingJobScheduled)
		{
			return;
		}
		mRootTrackingJobHandle.Complete();
		// Keep EasyECS dependency bookkeeping in sync with the explicit root JobHandle.
		mRootTransformECS?.CompleteBurstJobs();
		mRootTrackingJobScheduled = false;
		mRootChangedThisFrameCount = mRootTransformChangedCount.IsCreated ? Mathf.Min(mRootTransformChangedCount[0], mRootTransformGroups.Count) : 0;
		if (collectProfile)
		{
			mLastProfile.mRootTransformChangedCount += mRootChangedThisFrameCount;
			mLastProfile.mEasyECSBurstTransformCount += mRootTransformGroups.Count;
		}
	}

	private void propagateRootTransformChangesToCompatMesh()
	{
		if (mRootChangedThisFrameCount <= 0 || mRootTransformECS == null || mRootChildECS == null ||
			!mRootTransformMatrices.IsCreated || !mRootChildLocalToRoot.IsCreated)
		{
			return;
		}

		var rootChangedFlags = mRootTransformECS.getValueColumn();
		var childRootIndices = mRootChildECS.getValueColumn();
		Matrix4x4 worldToBatch = getWorldToBatchMatrix();
		int rootCount = Mathf.Min(mRootTransformGroups.Count, Mathf.Min(mRootTransformMatrices.Length, mRootTransformECS.Count));
		ensureCompatRootPatchCapacity(rootCount);
		for (int rootIndex = 0; rootIndex < rootCount; ++rootIndex)
		{
			if (rootChangedFlags[rootIndex] == 0)
			{
				mCompatRootPatchActive[rootIndex] = 0;
				continue;
			}
			FastSpriteSortGroup group = mRootTransformGroups[rootIndex];
			if (group == null || !group.isRuntimeActive())
			{
				mCompatRootPatchActive[rootIndex] = 0;
				continue;
			}
			mCompatRootPatchActive[rootIndex] = 1;
			mCompatRootToBatchMatrices[rootIndex] = worldToBatch * mRootTransformMatrices[rootIndex];
		}

		int childCount = Mathf.Min(
			mRootTrackedChildren.Count,
			Mathf.Min(mRootChildECS.Count, mRootChildLocalToRoot.Length));
		for (int childIndex = 0; childIndex < childCount; ++childIndex)
		{
			int rootIndex = childRootIndices[childIndex];
			if ((uint)rootIndex >= (uint)rootCount || mCompatRootPatchActive[rootIndex] == 0)
			{
				continue;
			}
			FastSpriteRenderer renderer = mRootTrackedChildren[childIndex];
			if (renderer == null || renderer.mSystemOwner != this || renderer.mRetainedWhileDisabled || !renderer.mRuntimeActiveState)
			{
				continue;
			}
			++mLastProfile.mRootChildPropagatedCount;
			FastSpriteBatch batch = renderer.mBatch;
			if (batch != null && renderer.mRootTrackingLocalToGroupValid &&
				batch.queueRootOnlyTransformPatch(renderer, mCompatRootToBatchMatrices[rootIndex], mRootChildLocalToRoot[childIndex]))
			{
				continue;
			}

			// Safety fallback for topology rebuilds, stale membership or a child whose local-to-root
			// cache was invalidated explicitly. The ordinary dirty path preserves correctness.
			renderer.markCompatTransformDirty();
		}
	}

	private void ensureCompatRootPatchCapacity(int requiredCount)
	{
		if (mCompatRootToBatchMatrices.Length >= requiredCount)
		{
			return;
		}
		int capacity = Mathf.NextPowerOfTwo(Mathf.Max(requiredCount, 16));
		Array.Resize(ref mCompatRootToBatchMatrices, capacity);
		Array.Resize(ref mCompatRootPatchActive, capacity);
	}

	private void pollIndependentRendererTransforms(bool systemTransformChanged, bool collectProfile)
	{
		if (mRootTransformGroups.Count == 0 && mIndependentTransformRenderers.Count == mRenderers.Count)
		{
			pollAllAutoRendererTransformsBurst(systemTransformChanged, collectProfile);
			return;
		}

		int count = mIndependentTransformRenderers.Count;
		if (count <= 0)
		{
			return;
		}

		if (count >= TRANSFORM_JOB_MIN_COUNT && BurstCompiler.IsEnabled &&
			mTransformSortECS != null && FastSpriteTransformSortData_ECSList.IsUnsafeBackend &&
			mIndependentTransformECS != null && Int_ECSList.IsUnsafeBackend &&
			mIndependentTransformECS.Count == count &&
			mIndependentTransformAccessArray.isCreated && mIndependentTransformAccessArray.length == count)
		{
			FastSortContext sortContext = buildSortContext();
			FastSpriteTransformSortData_ECSList.BurstView stateView = mTransformSortECS.GetBurstView();
			Int_ECSList.BurstView trackingView = mIndependentTransformECS.GetBurstView();
			JobHandle dependency = JobHandle.CombineDependencies(
				mTransformSortECS.GetBurstDependency(),
				mIndependentTransformECS.GetBurstDependency());
			DetectIndependentTransformAndSortJob job = new()
			{
				mTracking = trackingView,
				mPreviousMatrices = mTransformMatrices,
				mState = stateView,
				mSortContext = sortContext,
			};
			JobHandle handle = job.ScheduleReadOnly(mIndependentTransformAccessArray, 64, dependency);
			mTransformSortECS.RegisterBurstJob(handle);
			mIndependentTransformECS.RegisterBurstJob(handle);
			mTransformSortECS.CompleteBurstJobs();
			mIndependentTransformECS.CompleteBurstJobs();
			if (collectProfile)
			{
				mLastProfile.mEasyECSBurstTransformCount += count;
			}

			var changedFlags = mTransformSortECS.getTransformChangedColumn();
			var sortValues = mTransformSortECS.getSortValueColumn();
			var runtimeActive = mTransformSortECS.getRuntimeActiveColumn();
			var rendererIndices = mIndependentTransformECS.getValueColumn();
			for (int subsetIndex = 0; subsetIndex < count; ++subsetIndex)
			{
				int rendererIndex = rendererIndices[subsetIndex];
				if ((uint)rendererIndex >= (uint)mRenderers.Count)
				{
					continue;
				}
				bool transformChanged = changedFlags[rendererIndex] != 0;
				if (!systemTransformChanged && !transformChanged)
				{
					continue;
				}
				if (runtimeActive[rendererIndex] == 0)
				{
					if (collectProfile)
					{
						++mLastProfile.mEasyECSRuntimeInactiveGateCount;
					}
					continue;
				}

				FastSpriteRenderer renderer = mIndependentTransformRenderers[subsetIndex];
				if (ReferenceEquals(renderer, null) || renderer.mSystemOwner != this ||
					renderer.mRetainedWhileDisabled || !renderer.mRuntimeActiveState)
				{
					continue;
				}
				if (collectProfile)
				{
					++mLastProfile.mTransformChangedCount;
				}
				bool sortValueChanged = false;
				if (transformChanged && mSortMode != FastSpriteSortMode.Registration)
				{
					sortValueChanged = renderer.refreshSortValueCache(sortValues[rendererIndex]);
				}
				renderer.markTransformDirty(sortValueChanged);
			}
			return;
		}

		FastSortContext fallbackSortContext = buildSortContext();
		for (int i = 0; i < count; ++i)
		{
			FastSpriteRenderer renderer = mIndependentTransformRenderers[i];
			if (ReferenceEquals(renderer, null) || renderer.mSystemOwner != this || renderer.mRetainedWhileDisabled || !renderer.mRuntimeActiveState)
			{
				continue;
			}
			bool transformChanged = renderer.pollTransformChanged();
			if (!transformChanged && !systemTransformChanged)
			{
				continue;
			}
			int rendererIndex = renderer.mSystemRendererIndex;
			if ((uint)rendererIndex >= (uint)mRenderers.Count)
			{
				continue;
			}
			Matrix4x4 localToWorld = transformChanged ? renderer.transform.localToWorldMatrix : mTransformMatrices[rendererIndex];
			if (transformChanged)
			{
				mTransformMatrices[rendererIndex] = localToWorld;
			}
			applyTransformDirty(renderer, localToWorld, fallbackSortContext, collectProfile);
		}
	}

	private void pollRootTransformsMainThreadFallback(bool collectProfile)
	{
		mRootChangedThisFrameCount = 0;
		var rootChangedFlags = mRootTransformECS.getValueColumn();
		for (int rootIndex = 0; rootIndex < mRootTransformGroups.Count; ++rootIndex)
		{
			FastSpriteSortGroup group = mRootTransformGroups[rootIndex];
			if (group == null || !group.isRuntimeActive() || !group.pollRootTransformChanged(out Matrix4x4 rootMatrix))
			{
				continue;
			}
			mRootTransformMatrices[rootIndex] = rootMatrix;
			rootChangedFlags[rootIndex] = 1;
			++mRootChangedThisFrameCount;
			if (collectProfile)
			{
				++mLastProfile.mRootTransformChangedCount;
			}
		}
	}

	private void pollAllAutoRendererTransformsBurst(bool systemTransformChanged, bool collectProfile)
	{
		int count = mRenderers.Count;
		if (count <= 0)
		{
			return;
		}
		FastSortContext sortContext = buildSortContext();
		if (count < TRANSFORM_JOB_MIN_COUNT || !BurstCompiler.IsEnabled || mTransformSortECS == null || !FastSpriteTransformSortData_ECSList.IsUnsafeBackend)
		{
			for (int i = 0; i < count; ++i)
			{
				FastSpriteRenderer renderer = mRenderers[i];
				if (ReferenceEquals(renderer, null) || renderer.mRetainedWhileDisabled)
				{
					continue;
				}
				bool transformChanged = renderer.pollTransformChanged();
				Matrix4x4 localToWorld = mTransformMatrices[i];
				if (transformChanged)
				{
					localToWorld = renderer.transform.localToWorldMatrix;
					mTransformMatrices[i] = localToWorld;
				}
				if (transformChanged || systemTransformChanged)
				{
					applyTransformDirty(renderer, localToWorld, sortContext, collectProfile);
				}
			}
			return;
		}

		FastSpriteTransformSortData_ECSList.BurstView stateView = mTransformSortECS.GetBurstView();
		JobHandle dependency = mTransformSortECS.GetBurstDependency();
		DetectTransformAndSortJob job = new()
		{
			mPreviousMatrices = mTransformMatrices,
			mState = stateView,
			mSortContext = sortContext,
		};
		JobHandle handle = job.ScheduleReadOnly(mTransformAccessArray, 64, dependency);
		mTransformSortECS.RegisterBurstJob(handle);
		mTransformSortECS.CompleteBurstJobs();
		if (collectProfile)
		{
			mLastProfile.mEasyECSBurstTransformCount += count;
		}

		var changedFlags = mTransformSortECS.getTransformChangedColumn();
		var sortValues = mTransformSortECS.getSortValueColumn();
		for (int i = 0; i < count; ++i)
		{
			bool transformChanged = changedFlags[i] != 0;
			if (!systemTransformChanged && !transformChanged)
			{
				continue;
			}
			FastSpriteRenderer renderer = mRenderers[i];
			if (ReferenceEquals(renderer, null) || renderer.mRetainedWhileDisabled)
			{
				continue;
			}
			if (collectProfile)
			{
				++mLastProfile.mTransformChangedCount;
			}
			bool sortValueChanged = false;
			if (transformChanged && mSortMode != FastSpriteSortMode.Registration)
			{
				sortValueChanged = renderer.refreshSortValueCache(sortValues[i]);
			}
			renderer.markTransformDirty(sortValueChanged);
		}
	}

	private void refreshNonPolledRenderersForSystemTransform(bool collectProfile)
	{
		FastSortContext sortContext = buildSortContext();
		for (int i = 0; i < mRenderers.Count; ++i)
		{
			FastSpriteRenderer renderer = mRenderers[i];
			if (ReferenceEquals(renderer, null) || renderer.mRetainedWhileDisabled || !renderer.mRuntimeActiveState)
			{
				continue;
			}
			if (renderer.mRootTrackingChildIndex >= 0 || renderer.mIndependentTransformTrackingIndex >= 0)
			{
				continue;
			}
			Matrix4x4 localToWorld = renderer.transform.localToWorldMatrix;
			mTransformMatrices[i] = localToWorld;
			applyTransformDirty(renderer, localToWorld, sortContext, collectProfile);
		}
	}

	private void updateRendererTransformTracking(FastSpriteRenderer renderer)
	{
		if (renderer == null || renderer.mSystemOwner != this)
		{
			return;
		}
		FastSpriteSortGroup group = renderer.getSortGroup();
		if (group != null && group.usesRootOnlyTransformTracking())
		{
			removeIndependentTransformRenderer(renderer);
			addRootTrackingGroup(group);
			addRootTrackedChild(renderer, group);
			return;
		}
		removeRootTrackedChild(renderer);
		if (renderer.getTransformTrackingMode() == FastSpriteTransformTrackingMode.Auto)
		{
			addIndependentTransformRenderer(renderer);
		}
		else
		{
			removeIndependentTransformRenderer(renderer);
		}
	}

	private void removeRendererTransformTracking(FastSpriteRenderer renderer)
	{
		removeRootTrackedChild(renderer);
		removeIndependentTransformRenderer(renderer);
	}

	private void ensureIndependentTransformTrackingCapacity(int requiredCount)
	{
		int capacity = Mathf.NextPowerOfTwo(Mathf.Max(requiredCount, 16));
		if (!mIndependentTransformAccessArray.isCreated)
		{
			mIndependentTransformAccessArray = new TransformAccessArray(capacity);
			for (int i = 0; i < mIndependentTransformRenderers.Count; ++i)
			{
				FastSpriteRenderer existing = mIndependentTransformRenderers[i];
				if (!ReferenceEquals(existing, null))
				{
					mIndependentTransformAccessArray.Add(existing.transform);
				}
			}
		}
		else if (mIndependentTransformAccessArray.capacity < requiredCount)
		{
			TransformAccessArray replacement = new(capacity);
			for (int i = 0; i < mIndependentTransformRenderers.Count; ++i)
			{
				FastSpriteRenderer existing = mIndependentTransformRenderers[i];
				if (!ReferenceEquals(existing, null))
				{
					replacement.Add(existing.transform);
				}
			}
			mIndependentTransformAccessArray.Dispose();
			mIndependentTransformAccessArray = replacement;
		}

		if (mIndependentTransformECS == null)
		{
			mIndependentTransformECS = new Int_ECSList(capacity);
			for (int i = 0; i < mIndependentTransformRenderers.Count; ++i)
			{
				FastSpriteRenderer existing = mIndependentTransformRenderers[i];
				mIndependentTransformECS.Add(
					!ReferenceEquals(existing, null) ? existing.mSystemRendererIndex : -1);
			}
		}
		else
		{
			mIndependentTransformECS.EnsureCapacity(capacity);
		}
	}

	private void addIndependentTransformRenderer(FastSpriteRenderer renderer)
	{
		if (renderer == null || renderer.mIndependentTransformTrackingIndex >= 0)
		{
			return;
		}
		int rendererIndex = renderer.mSystemRendererIndex;
		if ((uint)rendererIndex < (uint)mRenderers.Count && mTransformMatrices.IsCreated && rendererIndex < mTransformMatrices.Length)
		{
			mTransformMatrices[rendererIndex] = renderer.transform.localToWorldMatrix;
		}
		int independentIndex = mIndependentTransformRenderers.Count;
		ensureIndependentTransformTrackingCapacity(independentIndex + 1);
		renderer.mIndependentTransformTrackingIndex = independentIndex;
		mIndependentTransformRenderers.Add(renderer);
		mIndependentTransformAccessArray.Add(renderer.transform);
		mIndependentTransformECS.Add(rendererIndex);
		++mPendingTransformTrackingAddCount;
	}

	private void removeIndependentTransformRenderer(FastSpriteRenderer renderer)
	{
		if (renderer == null)
		{
			return;
		}
		int index = renderer.mIndependentTransformTrackingIndex;
		if ((uint)index >= (uint)mIndependentTransformRenderers.Count || !ReferenceEquals(mIndependentTransformRenderers[index], renderer))
		{
			renderer.mIndependentTransformTrackingIndex = -1;
			return;
		}
		int last = mIndependentTransformRenderers.Count - 1;
		if (mIndependentTransformAccessArray.isCreated && index < mIndependentTransformAccessArray.length)
		{
			mIndependentTransformAccessArray.RemoveAtSwapBack(index);
		}
		if (index != last)
		{
			FastSpriteRenderer moved = mIndependentTransformRenderers[last];
			mIndependentTransformRenderers[index] = moved;
			if (mIndependentTransformECS != null && last < mIndependentTransformECS.Count)
			{
				var rendererIndices = mIndependentTransformECS.getValueColumn();
				rendererIndices[index] = rendererIndices[last];
			}
			if (moved != null)
			{
				moved.mIndependentTransformTrackingIndex = index;
			}
		}
		mIndependentTransformRenderers.RemoveAt(last);
		if (mIndependentTransformECS != null && last < mIndependentTransformECS.Count)
		{
			mIndependentTransformECS.RemoveAt(last);
		}
		renderer.mIndependentTransformTrackingIndex = -1;
		++mPendingTransformTrackingRemoveCount;
	}

	private void addRootTrackingGroup(FastSpriteSortGroup group)
	{
		if (group == null || !group.usesRootOnlyTransformTracking() || group.mRootTrackingIndex >= 0)
		{
			return;
		}
		ensureRootTrackingNativeCapacity(mRootTransformGroups.Count + 1, mRootTrackedChildren.Count);
		int index = mRootTransformGroups.Count;
		mRootTransformGroups.Add(group);
		mRootTransformAccessArray.Add(group.transform);
		mRootTransformMatrices[index] = group.transform.localToWorldMatrix;
		mRootTransformECS.Add(0);
		group.mRootTrackingIndex = index;
		++mPendingTransformTrackingAddCount;
		List<FastSpriteRenderer> children = group.getRenderersUnsafe();
		for (int i = 0; i < children.Count; ++i)
		{
			updateRendererTransformTracking(children[i]);
		}
	}

	private void removeRootTrackingGroup(FastSpriteSortGroup group)
	{
		if (group == null)
		{
			return;
		}
		int index = group.mRootTrackingIndex;
		if ((uint)index >= (uint)mRootTransformGroups.Count || !ReferenceEquals(mRootTransformGroups[index], group))
		{
			group.mRootTrackingIndex = -1;
			return;
		}
		List<FastSpriteRenderer> ownChildren = group.getRenderersUnsafe();
		for (int i = ownChildren.Count - 1; i >= 0; --i)
		{
			removeRootTrackedChild(ownChildren[i]);
		}
		int last = mRootTransformGroups.Count - 1;
		FastSpriteSortGroup moved = index != last ? mRootTransformGroups[last] : null;
		if (mRootTransformAccessArray.isCreated && index < mRootTransformAccessArray.length)
		{
			mRootTransformAccessArray.RemoveAtSwapBack(index);
		}
		if (index != last)
		{
			mRootTransformGroups[index] = moved;
			mRootTransformMatrices[index] = mRootTransformMatrices[last];
			if (mRootTransformECS != null && last < mRootTransformECS.Count)
			{
				var changedFlags = mRootTransformECS.getValueColumn();
				changedFlags[index] = changedFlags[last];
			}
			if (moved != null)
			{
				moved.mRootTrackingIndex = index;
				List<FastSpriteRenderer> movedChildren = moved.getRenderersUnsafe();
				for (int i = 0; i < movedChildren.Count; ++i)
				{
					FastSpriteRenderer child = movedChildren[i];
					int childSlot = child != null ? child.mRootTrackingChildIndex : -1;
					if ((uint)childSlot < (uint)mRootTrackedChildren.Count &&
						mRootChildECS != null && childSlot < mRootChildECS.Count)
					{
						mRootChildECS.getValueColumn()[childSlot] = index;
					}
					if (child != null && child.isActiveAndEnabled)
					{
						child.markDirty(FastSpriteDirtyFlags.Vertex | FastSpriteDirtyFlags.Bounds);
					}
				}
			}
		}
		mRootTransformGroups.RemoveAt(last);
		if (mRootTransformECS != null && last < mRootTransformECS.Count)
		{
			mRootTransformECS.RemoveAt(last);
		}
		group.mRootTrackingIndex = -1;
		++mPendingTransformTrackingRemoveCount;
	}

	private void addRootTrackedChild(FastSpriteRenderer renderer, FastSpriteSortGroup group)
	{
		if (renderer == null || group == null || renderer.mSystemOwner != this || renderer.getSortGroup() != group)
		{
			return;
		}
		int rendererIndex = renderer.mSystemRendererIndex;
		if ((uint)rendererIndex >= (uint)mRenderers.Count || !ReferenceEquals(mRenderers[rendererIndex], renderer))
		{
			return;
		}
		int rootIndex = group.mRootTrackingIndex;
		if ((uint)rootIndex >= (uint)mRootTransformGroups.Count)
		{
			return;
		}
		if (renderer.mRootTrackingChildIndex >= 0)
		{
			updateRootTrackedChildLocalMatrix(renderer, group);
			return;
		}
		ensureRootTrackingNativeCapacity(mRootTransformGroups.Count, mRootTrackedChildren.Count + 1);
		if (!renderer.mRootTrackingLocalToGroupValid)
		{
			renderer.cacheRootTrackingLocalToGroup(group);
		}
		int childIndex = mRootTrackedChildren.Count;
		mRootTrackedChildren.Add(renderer);
		mRootChildECS.Add(rootIndex);
		mRootChildLocalToRoot[childIndex] = renderer.mRootTrackingLocalToGroup;
		renderer.mRootTrackingChildIndex = childIndex;
		// The stored vertex representation must switch from world-space to root-local.
		renderer.markDirty(FastSpriteDirtyFlags.Vertex | FastSpriteDirtyFlags.Bounds);
		++mPendingTransformTrackingAddCount;
	}

	private void removeRootTrackedChild(FastSpriteRenderer renderer)
	{
		if (renderer == null)
		{
			return;
		}
		int index = renderer.mRootTrackingChildIndex;
		if ((uint)index >= (uint)mRootTrackedChildren.Count || !ReferenceEquals(mRootTrackedChildren[index], renderer))
		{
			renderer.mRootTrackingChildIndex = -1;
			return;
		}
		int last = mRootTrackedChildren.Count - 1;
		if (index != last)
		{
			FastSpriteRenderer moved = mRootTrackedChildren[last];
			mRootTrackedChildren[index] = moved;
			if (mRootChildECS != null && last < mRootChildECS.Count)
			{
				var rootIndices = mRootChildECS.getValueColumn();
				rootIndices[index] = rootIndices[last];
			}
			mRootChildLocalToRoot[index] = mRootChildLocalToRoot[last];
			if (moved != null)
			{
				moved.mRootTrackingChildIndex = index;
			}
		}
		mRootTrackedChildren.RemoveAt(last);
		if (mRootChildECS != null && last < mRootChildECS.Count)
		{
			mRootChildECS.RemoveAt(last);
		}
		renderer.mRootTrackingChildIndex = -1;
		// Leaving RootOnly requires restoring ordinary world-space CPU vertices.
		renderer.markDirty(FastSpriteDirtyFlags.Vertex | FastSpriteDirtyFlags.Bounds);
		++mPendingTransformTrackingRemoveCount;
	}

	private void updateRootTrackedChildLocalMatrix(FastSpriteRenderer renderer, FastSpriteSortGroup group)
	{
		if (renderer == null || group == null)
		{
			return;
		}
		int slot = renderer.mRootTrackingChildIndex;
		if ((uint)slot >= (uint)mRootTrackedChildren.Count || !ReferenceEquals(mRootTrackedChildren[slot], renderer))
		{
			return;
		}
		if (!renderer.mRootTrackingLocalToGroupValid)
		{
			renderer.cacheRootTrackingLocalToGroup(group);
		}
		mRootChildLocalToRoot[slot] = renderer.mRootTrackingLocalToGroup;
		if (mRootChildECS != null && slot < mRootChildECS.Count)
		{
			mRootChildECS.getValueColumn()[slot] = group.mRootTrackingIndex;
		}
	}

	private void applyTransformDirty(FastSpriteRenderer renderer, Matrix4x4 localToWorld, FastSortContext sortContext, bool collectProfile)
	{
		if (renderer == null || renderer.mRetainedWhileDisabled)
		{
			return;
		}
		if (collectProfile)
		{
			++mLastProfile.mTransformChangedCount;
		}
		bool sortValueChanged = false;
		if (mSortMode != FastSpriteSortMode.Registration)
		{
			float sortValue = getSortValueFromMatrix(renderer, localToWorld, sortContext);
			sortValueChanged = renderer.refreshSortValueCache(sortValue);
		}
		renderer.markTransformDirty(sortValueChanged);
	}

	internal void syncTransformSortPoint(FastSpriteRenderer renderer, Vector3 localPoint)
	{
		if (renderer == null || renderer.mSystemOwner != this || mTransformSortECS == null)
		{
			return;
		}
		int index = renderer.mSystemRendererIndex;
		if ((uint)index >= (uint)mTransformSortECS.Count)
		{
			return;
		}
		var x = mTransformSortECS.getSortPointXColumn();
		var y = mTransformSortECS.getSortPointYColumn();
		var z = mTransformSortECS.getSortPointZColumn();
		x[index] = localPoint.x;
		y[index] = localPoint.y;
		z[index] = localPoint.z;
	}

	private void queueMembershipDirty(FastSpriteRenderer renderer)
	{
		if (renderer == null || renderer.mSystemOwner != this || renderer.mMembershipDirtyQueued)
		{
			return;
		}
		renderer.mMembershipDirtyQueued = true;
		mMembershipDirtyRenderers.Add(renderer);
	}

	private bool tryQueueSortingOrderTransferGroup(FastSpriteRenderer renderer, FastSpriteDirtyFlags dirtyFlags)
	{
		if (renderer == null || renderer.mSystemOwner != this)
		{
			return false;
		}

		FastSpriteBatch oldBatch = renderer.mBatch;
		FastSpriteDirtyFlags nonOrderDirty = dirtyFlags &
			~(FastSpriteDirtyFlags.Batch | FastSpriteDirtyFlags.SortingOrderBatch);
		if (oldBatch == null || oldBatch.getOwner() != this ||
			(dirtyFlags & FastSpriteDirtyFlags.SortingOrderBatch) == 0 ||
			nonOrderDirty != FastSpriteDirtyFlags.None || renderer.mBatchDirtyQueued)
		{
			return false;
		}

		int targetSortingOrder = renderer.getSortingOrder();
		if (oldBatch.getKey().mSortingOrder == targetSortingOrder)
		{
			renderer.clearDirtyFlags(FastSpriteDirtyFlags.Batch | FastSpriteDirtyFlags.SortingOrderBatch);
			return true;
		}

		SortingOrderTransferGroupKey groupKey = new(oldBatch, targetSortingOrder);
		if (!mSortingOrderTransferGroupLookup.TryGetValue(groupKey, out int groupIndex))
		{
			groupIndex = mSortingOrderTransferGroups.Count;
			mSortingOrderTransferGroupLookup.Add(groupKey, groupIndex);
			mSortingOrderTransferGroups.Add(new FastSpriteSortingOrderTransferGroupData
			{
				mSourceBatch = oldBatch,
				mTargetSortingOrder = targetSortingOrder,
				mFirstItemIndex = -1,
				mLastItemIndex = -1,
				mItemCount = 0,
			});
		}

		int itemIndex = mSortingOrderTransferItems.Count;
		mSortingOrderTransferItems.Add(new FastSpriteSortingOrderTransferItemData
		{
			mRenderer = renderer,
			mNextItemIndex = -1,
		});

		var groupFirstItems = mSortingOrderTransferGroups.getFirstItemIndexColumn();
		var groupLastItems = mSortingOrderTransferGroups.getLastItemIndexColumn();
		var groupItemCounts = mSortingOrderTransferGroups.getItemCountColumn();
		var itemNextItems = mSortingOrderTransferItems.getNextItemIndexColumn();
		int lastItemIndex = groupLastItems[groupIndex];
		if (lastItemIndex >= 0)
		{
			itemNextItems[lastItemIndex] = itemIndex;
		}
		else
		{
			groupFirstItems[groupIndex] = itemIndex;
		}
		groupLastItems[groupIndex] = itemIndex;
		++groupItemCounts[groupIndex];
		return true;
	}

	private void queueSortingOrderTransferGroupFallback(FastSpriteSortingOrderTransferGroupData group)
	{
		int itemIndex = group.mFirstItemIndex;
		while (itemIndex >= 0)
		{
			FastSpriteSortingOrderTransferItemData item = mSortingOrderTransferItems.Get(itemIndex);
			if (item.mRenderer != null)
			{
				mMembershipFallbackRenderers.Add(item.mRenderer);
			}
			itemIndex = item.mNextItemIndex;
		}
	}

	private void processSortingOrderTransferGroups(bool collectProfile)
	{
		for (int groupIndex = 0; groupIndex < mSortingOrderTransferGroups.Count; ++groupIndex)
		{
			FastSpriteSortingOrderTransferGroupData group = mSortingOrderTransferGroups.Get(groupIndex);
			FastSpriteBatch sourceBatch = group.mSourceBatch;
			if (sourceBatch == null || sourceBatch.getOwner() != this || group.mItemCount <= 0)
			{
				queueSortingOrderTransferGroupFallback(group);
				continue;
			}

			FastSpriteBatchKey destinationKey = sourceBatch.getKey().withSortingOrder(group.mTargetSortingOrder);
			if (!mBatches.TryGetValue(destinationKey, out FastSpriteBatch destinationBatch) ||
				destinationBatch == null || destinationBatch == sourceBatch ||
				!sourceBatch.canFastTransferMembershipTo(destinationBatch))
			{
				queueSortingOrderTransferGroupFallback(group);
				continue;
			}

			bool sourceWasActive = sourceBatch.getCount() > 0;
			bool destinationWasDormant = destinationBatch.getCount() == 0;
			bool anyTransferred = false;
			int itemIndex = group.mFirstItemIndex;
			while (itemIndex >= 0)
			{
				FastSpriteSortingOrderTransferItemData item = mSortingOrderTransferItems.Get(itemIndex);
				FastSpriteRenderer renderer = item.mRenderer;
				itemIndex = item.mNextItemIndex;
				if (renderer == null || renderer.mSystemOwner != this)
				{
					continue;
				}
				if (renderer.mBatch != sourceBatch)
				{
					mMembershipFallbackRenderers.Add(renderer);
					continue;
				}
				if (!sourceBatch.tryFastTransferMembershipPrepared(destinationBatch, renderer))
				{
					mMembershipFallbackRenderers.Add(renderer);
					continue;
				}

				renderer.clearDirtyFlags(FastSpriteDirtyFlags.Batch | FastSpriteDirtyFlags.SortingOrderBatch);
				++mLastProfile.mSortingOrderFastTransferCount;
				if (collectProfile)
				{
					++mLastProfile.mMembershipChangedCount;
				}
				anyTransferred = true;
			}

			if (!anyTransferred)
			{
				continue;
			}
			if (sourceWasActive && sourceBatch.getCount() == 0)
			{
				mActiveBatchCount = Mathf.Max(0, mActiveBatchCount - 1);
			}
			if (destinationWasDormant && destinationBatch.getCount() > 0)
			{
				++mActiveBatchCount;
			}
			++mLastProfile.mSortingOrderTransferGroupCount;
		}
	}

	private void processMembershipDirtyQueue(bool collectProfile)
	{
		mSortingOrderTransferGroupLookup.Clear();
		mSortingOrderTransferGroups.Clear();
		mSortingOrderTransferItems.Clear();
		mMembershipFallbackRenderers.Clear();

		for (int i = 0; i < mMembershipDirtyRenderers.Count; ++i)
		{
			FastSpriteRenderer renderer = mMembershipDirtyRenderers[i];
			if (renderer == null || renderer.mSystemOwner != this)
			{
				continue;
			}
			renderer.mMembershipDirtyQueued = false;
			FastSpriteDirtyFlags dirtyFlags = renderer.peekDirtyFlags();
			if ((dirtyFlags & FastSpriteDirtyFlags.Batch) == 0)
			{
				continue;
			}
			if (collectProfile)
			{
				++mLastProfile.mMembershipCheckCount;
			}
			if (!tryQueueSortingOrderTransferGroup(renderer, dirtyFlags))
			{
				mMembershipFallbackRenderers.Add(renderer);
			}
		}

		processSortingOrderTransferGroups(collectProfile);

		for (int i = 0; i < mMembershipFallbackRenderers.Count; ++i)
		{
			FastSpriteRenderer renderer = mMembershipFallbackRenderers[i];
			if (renderer == null || renderer.mSystemOwner != this ||
				(renderer.peekDirtyFlags() & FastSpriteDirtyFlags.Batch) == 0)
			{
				continue;
			}
			bool changed = ensureMembership(renderer);
			if (collectProfile && changed)
			{
				++mLastProfile.mMembershipChangedCount;
			}
		}

		mMembershipDirtyRenderers.Clear();
		mMembershipFallbackRenderers.Clear();
		mSortingOrderTransferGroupLookup.Clear();
		mSortingOrderTransferGroups.Clear();
		mSortingOrderTransferItems.Clear();
	}

	private void ensureTransformTrackingCapacity(int requiredCount)
	{
		int capacity = Mathf.NextPowerOfTwo(Mathf.Max(requiredCount, 16));
		if (!mTransformAccessArray.isCreated)
		{
			mTransformAccessArray = new TransformAccessArray(capacity);
			for (int i = 0; i < mRenderers.Count; ++i)
			{
				FastSpriteRenderer renderer = mRenderers[i];
				if (renderer != null)
				{
					mTransformAccessArray.Add(renderer.transform);
				}
			}
		}
		else if (mTransformAccessArray.capacity < requiredCount)
		{
			TransformAccessArray replacement = new(capacity);
			for (int i = 0; i < mRenderers.Count; ++i)
			{
				FastSpriteRenderer renderer = mRenderers[i];
				if (renderer != null)
				{
					replacement.Add(renderer.transform);
				}
			}
			mTransformAccessArray.Dispose();
			mTransformAccessArray = replacement;
		}

		if (!mTransformMatrices.IsCreated || mTransformMatrices.Length < requiredCount)
		{
			NativeArray<Matrix4x4> newMatrices = new(capacity, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			if (mTransformMatrices.IsCreated)
			{
				int copyCount = Mathf.Min(mRenderers.Count, mTransformMatrices.Length);
				for (int i = 0; i < copyCount; ++i)
				{
					newMatrices[i] = mTransformMatrices[i];
				}
				mTransformMatrices.Dispose();
			}
			mTransformMatrices = newMatrices;
		}
		if (mTransformSortECS == null)
		{
			mTransformSortECS = new FastSpriteTransformSortData_ECSList(capacity);
		}
		else
		{
			mTransformSortECS.EnsureCapacity(capacity);
		}
	}

	private void ensureRootTrackingNativeCapacity(int requiredRoots, int requiredChildren)
	{
		int rootCapacity = Mathf.NextPowerOfTwo(Mathf.Max(requiredRoots, 16));
		if (!mRootTransformAccessArray.isCreated)
		{
			mRootTransformAccessArray = new TransformAccessArray(rootCapacity);
		}
		else if (mRootTransformAccessArray.capacity < requiredRoots)
		{
			TransformAccessArray replacement = new(rootCapacity);
			for (int i = 0; i < mRootTransformGroups.Count; ++i)
			{
				FastSpriteSortGroup group = mRootTransformGroups[i];
				if (group != null)
				{
					replacement.Add(group.transform);
				}
			}
			mRootTransformAccessArray.Dispose();
			mRootTransformAccessArray = replacement;
		}
		if (!mRootTransformMatrices.IsCreated || mRootTransformMatrices.Length < requiredRoots)
		{
			NativeArray<Matrix4x4> next = new(rootCapacity, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			if (mRootTransformMatrices.IsCreated)
			{
				int copy = Mathf.Min(mRootTransformGroups.Count, mRootTransformMatrices.Length);
				NativeArray<Matrix4x4>.Copy(mRootTransformMatrices, 0, next, 0, copy);
				mRootTransformMatrices.Dispose();
			}
			mRootTransformMatrices = next;
		}
		if (mRootTransformECS == null)
		{
			mRootTransformECS = new Byte_ECSList(rootCapacity);
			for (int i = 0; i < mRootTransformGroups.Count; ++i)
			{
				mRootTransformECS.Add(0);
			}
		}
		else
		{
			mRootTransformECS.EnsureCapacity(rootCapacity);
		}
		// Structural operations keep these counts lock-step. Repair only defensive/domain-reload
		// mismatches here before any BurstView is exposed.
		while (mRootTransformECS.Count < mRootTransformGroups.Count)
		{
			mRootTransformECS.Add(0);
		}
		while (mRootTransformECS.Count > mRootTransformGroups.Count)
		{
			mRootTransformECS.RemoveAt(mRootTransformECS.Count - 1);
		}

		int childCapacity = Mathf.NextPowerOfTwo(Mathf.Max(requiredChildren, 32));
		if (!mRootChildLocalToRoot.IsCreated || mRootChildLocalToRoot.Length < requiredChildren)
		{
			NativeArray<Matrix4x4> nextLocal = new(childCapacity, Allocator.Persistent, NativeArrayOptions.UninitializedMemory);
			if (mRootChildLocalToRoot.IsCreated)
			{
				int copy = Mathf.Min(mRootTrackedChildren.Count, mRootChildLocalToRoot.Length);
				NativeArray<Matrix4x4>.Copy(mRootChildLocalToRoot, 0, nextLocal, 0, copy);
				mRootChildLocalToRoot.Dispose();
			}
			mRootChildLocalToRoot = nextLocal;
		}
		if (mRootChildECS == null)
		{
			mRootChildECS = new Int_ECSList(childCapacity);
			for (int i = 0; i < mRootTrackedChildren.Count; ++i)
			{
				FastSpriteRenderer child = mRootTrackedChildren[i];
				FastSpriteSortGroup group = child != null ? child.getSortGroup() : null;
				int rootIndex = group != null ? group.mRootTrackingIndex : -1;
				mRootChildECS.Add(rootIndex);
			}
		}
		else
		{
			mRootChildECS.EnsureCapacity(childCapacity);
		}
		while (mRootChildECS.Count < mRootTrackedChildren.Count)
		{
			int index = mRootChildECS.Count;
			FastSpriteRenderer child = mRootTrackedChildren[index];
			FastSpriteSortGroup group = child != null ? child.getSortGroup() : null;
			mRootChildECS.Add(group != null ? group.mRootTrackingIndex : -1);
		}
		while (mRootChildECS.Count > mRootTrackedChildren.Count)
		{
			mRootChildECS.RemoveAt(mRootChildECS.Count - 1);
		}
		if (!mRootTransformChangedCount.IsCreated)
		{
			mRootTransformChangedCount = new NativeArray<int>(1, Allocator.Persistent, NativeArrayOptions.ClearMemory);
		}
	}

	private void disposeTransformTracking()
	{
		if (mRootTrackingJobScheduled)
		{
			mRootTrackingJobHandle.Complete();
			mRootTransformECS?.CompleteBurstJobs();
			mRootTrackingJobScheduled = false;
		}
		if (mRootTransformAccessArray.isCreated)
		{
			mRootTransformAccessArray.Dispose();
		}
		if (mRootTransformMatrices.IsCreated)
		{
			mRootTransformMatrices.Dispose();
		}
		if (mRootTransformECS != null)
		{
			mRootTransformECS.Dispose();
			mRootTransformECS = null;
		}
		if (mRootChildECS != null)
		{
			mRootChildECS.Dispose();
			mRootChildECS = null;
		}
		if (mRootChildLocalToRoot.IsCreated)
		{
			mRootChildLocalToRoot.Dispose();
		}
		if (mRootTransformChangedCount.IsCreated)
		{
			mRootTransformChangedCount.Dispose();
		}
		for (int i = 0; i < mRootTransformGroups.Count; ++i)
		{
			FastSpriteSortGroup group = mRootTransformGroups[i];
			if (group != null)
			{
				group.mRootTrackingIndex = -1;
			}
		}
		for (int i = 0; i < mRootTrackedChildren.Count; ++i)
		{
			FastSpriteRenderer renderer = mRootTrackedChildren[i];
			if (renderer != null)
			{
				renderer.mRootTrackingChildIndex = -1;
			}
		}
		for (int i = 0; i < mIndependentTransformRenderers.Count; ++i)
		{
			FastSpriteRenderer renderer = mIndependentTransformRenderers[i];
			if (renderer != null)
			{
				renderer.mIndependentTransformTrackingIndex = -1;
			}
		}
		if (mIndependentTransformAccessArray.isCreated)
		{
			mIndependentTransformAccessArray.Dispose();
		}
		if (mIndependentTransformECS != null)
		{
			mIndependentTransformECS.Dispose();
			mIndependentTransformECS = null;
		}
		mRootTransformGroups.Clear();
		mRootTrackedChildren.Clear();
		mIndependentTransformRenderers.Clear();
		if (mTransformAccessArray.isCreated)
		{
			mTransformAccessArray.Dispose();
		}
		if (mTransformMatrices.IsCreated)
		{
			mTransformMatrices.Dispose();
		}
		if (mTransformSortECS != null)
		{
			mTransformSortECS.Dispose();
			mTransformSortECS = null;
		}
	}

	private void invalidateRendererSortCaches()
	{
		for (int i = 0; i < mRenderers.Count; ++i)
		{
			if (mRenderers[i] != null)
			{
				mRenderers[i].invalidateSortValueCache();
			}
		}
	}

	private static double elapsedProfileMS(long start)
	{
		return (System.Diagnostics.Stopwatch.GetTimestamp() - start) * 1000.0 / System.Diagnostics.Stopwatch.Frequency;
	}

	private bool pollSystemTransformChanged()
	{
		Matrix4x4 matrix = transform.localToWorldMatrix;
		if (!mSystemTransformValid)
		{
			mLastSystemLocalToWorld = matrix;
			mSystemTransformValid = true;
			return true;
		}
		if (mLastSystemLocalToWorld == matrix)
		{
			return false;
		}
		mLastSystemLocalToWorld = matrix;
		return true;
	}

	private bool pollCameraSortChanged()
	{
		Camera camera = getSortCamera();
		if (camera == null)
		{
			// Losing the sort camera changes CameraDistance values to the no-camera
			// fallback, so invalidate once if a valid camera state existed before.
			bool cameraLostChanged = mCameraSortStateValid;
			mCameraSortStateValid = false;
			mLastSortCameraObjectID = 0UL;
			return cameraLostChanged;
		}

		Transform cameraTransform = camera.transform;
		TransparencySortMode mode = getEffectiveTransparencySortMode(camera);
		Vector3 position = cameraTransform.position;
		Vector3 forward = cameraTransform.forward;
		Vector3 customAxis = GraphicsSettings.transparencySortAxis;
		ulong cameraObjectID = FastUnityObjectIDUtility.getID64(camera);

		bool changed = !mCameraSortStateValid ||
			mLastSortCameraObjectID != cameraObjectID ||
			mLastTransparencySortMode != mode;
		if (!changed && mCameraSortStateValid)
		{
			switch (mode)
			{
				case TransparencySortMode.Orthographic:
					// Orthographic camera translation shifts every depth by the same
					// constant. Only a forward-axis change can alter relative order.
					changed = mLastCameraSortForward != forward;
					break;
				case TransparencySortMode.CustomAxis:
					// CustomAxis sorting is camera-transform independent.
					changed = mLastTransparencySortAxis != customAxis;
					break;
				case TransparencySortMode.Perspective:
					// The current perspective scalar is squared distance to camera;
					// camera rotation is irrelevant, position is not.
					changed = mLastCameraSortPosition != position;
					break;
			}
		}

		mLastSortCameraObjectID = cameraObjectID;
		mLastTransparencySortMode = mode;
		mLastCameraSortPosition = position;
		mLastCameraSortForward = forward;
		mLastTransparencySortAxis = customAxis;
		mCameraSortStateValid = true;
		return changed;
	}

	private void OnEnable()
	{
		resolveBackendSelection(false);
		if (!sSystems.Contains(this))
		{
			sSystems.Add(this);
		}
		foreach (FastSpriteBatch batch in mBatches.Values)
		{
			batch.setRenderEnabled(true);
		}
		for (int i = sPendingRenderers.Count - 1; i >= 0; --i)
		{
			FastSpriteRenderer renderer = sPendingRenderers[i];
			if (renderer == null)
			{
				sPendingRenderers.RemoveAt(i);
				continue;
			}
			if (renderer.isActiveAndEnabled)
			{
				registerInternal(renderer);
				sPendingRenderers.RemoveAt(i);
			}
		}

#if UNITY_EDITOR
		if (!Application.isPlaying)
		{
			FastSpriteRenderer[] renderers = FindObjectsByType<FastSpriteRenderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
			for (int i = 0; i < renderers.Length; ++i)
			{
				registerInternal(renderers[i]);
			}
		}
#endif
		mSystemTransformValid = false;
		mCameraSortStateValid = false;
		mGPUDrivenPlanBatchRevision = -1;
		mGPUDrivenPlanGroupRevision = -1;
		mGPUDrivenPlanGroupOrderRevision = -1;
		mGPUDrivenPlanBatchOrderRevision = -1;
		mGPUDrivenPlanLayoutRevision = -1;
	}

	private void releaseRetainedRenderersForSystemDisable()
	{
		for (int i = mRenderers.Count - 1; i >= 0; --i)
		{
			FastSpriteRenderer renderer = mRenderers[i];
			if (renderer == null || !renderer.mRetainedWhileDisabled)
			{
				continue;
			}
			FastSpriteSortGroup group = renderer.mSortGroup;
			unregisterInternal(renderer);
			if (group != null)
			{
				group.unregisterRenderer(renderer);
			}
			renderer.mSortGroup = null;
			renderer.mRetainedWhileDisabled = false;
		}
	}

	private void OnDisable()
	{
		releaseRetainedRenderersForSystemDisable();
		sSystems.Remove(this);
		bool logicalStreamsMayBeStale = mGPUDrivenDirectStreamFrame;
		mGPUDrivenBackend.detachCommandBuffer();
		mGPUDrivenBackend.invalidatePlan();
		mGPUDrivenDirectStreamFrame = false;
		foreach (FastSpriteBatch batch in mBatches.Values)
		{
			if (logicalStreamsMayBeStale && batch != null && batch.getCount() > 0)
			{
				batch.submitLogicalMeshFromCPU();
			}
			batch?.setRenderEnabled(false);
		}
	}

	private void OnDestroy()
	{
		sSystems.Remove(this);
		for (int i = 0; i < mRenderers.Count; ++i)
		{
			FastSpriteRenderer renderer = mRenderers[i];
			if (renderer != null)
			{
				if (renderer.isActiveAndEnabled && !sPendingRenderers.Contains(renderer))
				{
					sPendingRenderers.Add(renderer);
				}
				if (renderer.mSystemOwner == this)
				{
					renderer.mRetainedWhileDisabled = false;
					renderer.mSystemOwner = null;
					renderer.mSystemRendererIndex = -1;
					renderer.mRootTrackingChildIndex = -1;
					renderer.mIndependentTransformTrackingIndex = -1;
					renderer.mGPUDrivenSlot = -1;
					renderer.mGPUDrivenGeometryOffset = -1;
					renderer.mGPUDrivenGeometryCapacity = 0;
					renderer.mGPUDrivenGeometryVertexCount = 0;
					renderer.mGPUDrivenDirtyQueued = false;
					renderer.mGPUDrivenDirtyFlags = FastSpriteDirtyFlags.None;
				}
				if (renderer.mBatch != null && renderer.mBatch.getOwner() == this)
				{
					renderer.mBatch = null;
					renderer.mBatchElementIndex = -1;
					renderer.mBatchDirtyQueued = false;
				}
				renderer.mMembershipDirtyQueued = false;
			}
		}
		foreach (FastSpriteBatch batch in mBatches.Values)
		{
			batch.Dispose();
		}
		mBatches.Clear();
		mSortGroupSortManager.Dispose();
		mOrderedSortGroups.Clear();
		mActiveBatchCount = 0;
		mGPUDrivenBackend.Dispose();
		mGPUDrivenValidationExpectedSlots.Dispose();
		mGPUPlanSortingLayerCacheIDs.Dispose();
		mGPUPlanSortingLayerCacheValues.Dispose();
		mSortingOrderTransferGroups.Dispose();
		mSortingOrderTransferItems.Dispose();
		mRenderers.Clear();
		mMembershipDirtyRenderers.Clear();
		disposeTransformTracking();
		if (mOwnedDefaultMaterial != null)
		{
			if (Application.isPlaying)
			{
				Destroy(mOwnedDefaultMaterial);
			}
			else
			{
				DestroyImmediate(mOwnedDefaultMaterial);
			}
			mOwnedDefaultMaterial = null;
		}
		mGPURootMatrixBuffer?.Dispose();
		mGPURootMatrixBuffer = null;
		mGPURootMatrixCapacity = 0;
		mGPURootMatrixUploadedCount = 0;
	}

	private void OnValidate()
	{
		if (Application.isPlaying)
		{
			resolveBackendSelection(true);
		}
		invalidateRendererSortCaches();
		foreach (FastSpriteBatch batch in mBatches.Values)
		{
			batch.markFullDirty();
		}
	}
}

public enum FastSpriteSortMode
{
	// Highest-throughput mode. Elements sharing the same Sorting Layer / Order are emitted
	// in registration order and can update vertices without rebuilding the index order.
	Registration = 0,
	// Common 2D mode: higher Y is emitted first, lower Y later.
	YAxis = 1,
	// Closer to SpriteRenderer transparent sorting for one Camera: farther sprites first.
	CameraDistance = 2,
}
