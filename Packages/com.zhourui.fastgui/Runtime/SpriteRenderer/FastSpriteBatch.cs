using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using EasyECS;
using UnityEngine;
using UnityEngine.Rendering;

[StructLayout(LayoutKind.Sequential)]
internal struct FastSpriteVertex
{
	public Vector3 mPosition;
	public Color32 mColor;
	public Vector2 mUV;
	public float mRootIndex;

	public FastSpriteVertex(Vector3 position, Color32 color, Vector2 uv, float rootIndex = -1.0f)
	{
		mPosition = position;
		mColor = color;
		mUV = uv;
		mRootIndex = rootIndex;
	}
}

internal readonly struct FastSpriteBatchKey : IEquatable<FastSpriteBatchKey>
{
	public readonly int mSortingLayerID;
	public readonly int mSortingOrder;
	public readonly int mMaterialID;
	public readonly int mTextureID;
	public readonly bool mGroupedStorage;
	public readonly Material mMaterial;
	public readonly Texture mTexture;

	public FastSpriteBatchKey(int sortingLayerID, int sortingOrder, Material material, Texture texture, bool groupedStorage = false)
	{
		mSortingLayerID = sortingLayerID;
		mSortingOrder = sortingOrder;
		mMaterial = material;
		mTexture = texture;
		mMaterialID = FastUnityObjectIDUtility.getLegacyIntID(material);
		mTextureID = FastUnityObjectIDUtility.getLegacyIntID(texture);
		mGroupedStorage = groupedStorage;
	}

	public bool Equals(FastSpriteBatchKey other)
	{
		return mSortingLayerID == other.mSortingLayerID &&
			mSortingOrder == other.mSortingOrder &&
			mMaterialID == other.mMaterialID &&
			mTextureID == other.mTextureID &&
			mGroupedStorage == other.mGroupedStorage;
	}

	public override bool Equals(object obj)
	{
		return obj is FastSpriteBatchKey other && Equals(other);
	}

	public override int GetHashCode()
	{
		unchecked
		{
			int hash = mSortingLayerID;
			hash = hash * 397 ^ mSortingOrder;
			hash = hash * 397 ^ mMaterialID;
			hash = hash * 397 ^ mTextureID;
			hash = hash * 397 ^ (mGroupedStorage ? 1 : 0);
			return hash;
		}
	}

	private FastSpriteBatchKey(int sortingLayerID, int sortingOrder, int materialID, int textureID, bool groupedStorage, Material material, Texture texture)
	{
		mSortingLayerID = sortingLayerID;
		mSortingOrder = sortingOrder;
		mMaterialID = materialID;
		mTextureID = textureID;
		mGroupedStorage = groupedStorage;
		mMaterial = material;
		mTexture = texture;
	}

	public FastSpriteBatchKey withSortingOrder(int sortingOrder)
	{
		return new FastSpriteBatchKey(mSortingLayerID, sortingOrder, mMaterialID, mTextureID, mGroupedStorage, mMaterial, mTexture);
	}

	public FastSpriteBatchKey withSortingLayerAndOrder(int sortingLayerID, int sortingOrder)
	{
		return new FastSpriteBatchKey(sortingLayerID, sortingOrder, mMaterialID, mTextureID, mGroupedStorage, mMaterial, mTexture);
	}
}

internal struct FastSpriteBatchFrameProfile
{
	public double mUpdateMS;
	public double mTopologyMS;
	public double mElementScanMS;
	public double mGeometryBuildMS;
	public double mVertexUploadMS;
	public double mSortIndexMS;
	public double mBoundsMS;
	public int mTopologyRebuildCount;
	public int mDirtyElementCount;
	public int mUVOnlyElementCount;
	public int mUVColorOnlyElementCount;
	public int mRootMotionScannedElementCount;
	public int mRootMotionDirtyElementCount;
	public int mRootMotionQuadFastPathCount;
	public int mVertexUploadCallCount;
	public int mIndexUploadCallCount;
}

// Logical batch plus Compat Mesh fallback storage.
internal sealed class FastSpriteBatch : IDisposable
{
	private const int VERTEX_STRIDE = 28;
	private static readonly int MAIN_TEX_ID = Shader.PropertyToID("_MainTex");
	private static readonly int BASE_MAP_ID = Shader.PropertyToID("_BaseMap");
	private static readonly VertexAttributeDescriptor[] VERTEX_LAYOUT =
	{
		new(VertexAttribute.Position, VertexAttributeFormat.Float32, 3, 0),
		new(VertexAttribute.Color, VertexAttributeFormat.UNorm8, 4, 0),
		new(VertexAttribute.TexCoord0, VertexAttributeFormat.Float32, 2, 0),
		new(VertexAttribute.TexCoord1, VertexAttributeFormat.Float32, 1, 0),
	};

	private struct ElementRange
	{
		public int mVertexStart;
		public int mVertexCount;
		public int mIndexStart;
		public int mIndexCount;
	}

	// Sprite local geometry is planar (z=0), so Compat Mesh only needs the first two
	// matrix columns plus translation. Caching this compact affine avoids two Matrix4x4
	// multiplications for every animation-only dirty renderer.
	private struct CachedAffine2D
	{
		public float m00, m01, m03;
		public float m10, m11, m13;
		public float m20, m21, m23;

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static CachedAffine2D fromMatrix(Matrix4x4 matrix)
		{
			return new CachedAffine2D
			{
				m00 = matrix.m00, m01 = matrix.m01, m03 = matrix.m03,
				m10 = matrix.m10, m11 = matrix.m11, m13 = matrix.m13,
				m20 = matrix.m20, m21 = matrix.m21, m23 = matrix.m23,
			};
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public static CachedAffine2D fromProduct(Matrix4x4 parent, Matrix4x4 local)
		{
			return new CachedAffine2D
			{
				m00 = parent.m00 * local.m00 + parent.m01 * local.m10 + parent.m02 * local.m20,
				m01 = parent.m00 * local.m01 + parent.m01 * local.m11 + parent.m02 * local.m21,
				m03 = parent.m00 * local.m03 + parent.m01 * local.m13 + parent.m02 * local.m23 + parent.m03,
				m10 = parent.m10 * local.m00 + parent.m11 * local.m10 + parent.m12 * local.m20,
				m11 = parent.m10 * local.m01 + parent.m11 * local.m11 + parent.m12 * local.m21,
				m13 = parent.m10 * local.m03 + parent.m11 * local.m13 + parent.m12 * local.m23 + parent.m13,
				m20 = parent.m20 * local.m00 + parent.m21 * local.m10 + parent.m22 * local.m20,
				m21 = parent.m20 * local.m01 + parent.m21 * local.m11 + parent.m22 * local.m21,
				m23 = parent.m20 * local.m03 + parent.m21 * local.m13 + parent.m22 * local.m23 + parent.m23,
			};
		}

		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		public readonly Vector3 multiplyPoint(Vector3 point)
		{
			float x = point.x;
			float y = point.y;
			return new Vector3(
				m00 * x + m01 * y + m03,
				m10 * x + m11 * y + m13,
				m20 * x + m21 * y + m23);
		}
	}

	private readonly FastSpriteRenderSystem mOwner;
	private readonly FastSpriteBatchKey mKey;
	internal FastSpriteGPUPlanCachedSpan mGPUPlanCachedSpan;
	private readonly List<FastSpriteRenderer> mElements = new(256);
	private readonly List<FastSpriteRenderer> mDirtyElements = new(128);
	private readonly List<int> mDirectRootMotionElements = new(128);
	private readonly List<int> mDirectRootMotionQuadElements = new(128);
	private int[] mDirectRootMotionEpochs = Array.Empty<int>();
	private int mDirectRootMotionEpoch = 1;
	private readonly List<int> mDrawOrder = new(256);
	private readonly Int_ECSList mGPUDrivenSlotOrder = new(256);
	private readonly List<FastSpriteVertex> mVertices = new(1024);
	private readonly List<Vector3> mLocalPositions = new(1024);
	private readonly List<int> mTopologyIndices = new(1536);
	private readonly List<int> mDrawIndices = new(1536);
	private readonly List<FastSpriteVertex> mScratchVertices = new(64);
	private readonly List<int> mScratchIndices = new(96);
	private ElementRange[] mRanges = Array.Empty<ElementRange>();
	private CachedAffine2D[] mLocalToBatchCache = Array.Empty<CachedAffine2D>();
	// Packed 3-bit triangle indices + reverse-winding bit for stable 4v/6i Simple quads.
	// Animation frames with identical topology then skip six index comparisons entirely.
	private int[] mSimpleQuadTopologyKeys = Array.Empty<int>();
	private float[] mSortValues = Array.Empty<float>();
	// Draw-index start for each stable element range. -1 means the element is currently absent
	// from the compact draw stream (for example after a real sort rebuild while retained).
	private int[] mDrawIndexStarts = Array.Empty<int>();

	private GameObject mRenderObject;
	private MeshFilter mMeshFilter;
	private MeshRenderer mMeshRenderer;
	private Mesh mMesh;
	private MaterialPropertyBlock mPropertyBlock;

	private int mActiveElementCount;
	private int mRetainedInactiveCount;
	private int mVertexCount;
	private int mIndexCount;
	private int mVertexUploadBytes;
	private int mIndexUploadBytes;
	private bool mTopologyDirty = true;
	private bool mSortDirty = true;
	private bool mGPUDrivenSlotOrderValid;
	private bool mLogicalMeshValid;
	private bool mVisibilityIndexDirty;
	private int mVisibilityIndexDirtyStart = int.MaxValue;
	private int mVisibilityIndexDirtyEnd;
	private Bounds mCachedBounds;
	private FastSpriteBatchFrameProfile mLastProfile;

	public FastSpriteBatch(FastSpriteRenderSystem owner, FastSpriteBatchKey key)
	{
		mOwner = owner;
		mKey = key;
		createRenderObject();
	}

	public FastSpriteRenderSystem getOwner()
	{
		return mOwner;
	}
	public FastSpriteBatchKey getKey()
	{
		return mKey;
	}
	public int getCount()
	{
		return mActiveElementCount;
	}
	public int getVertexCount()
	{
		return mVertexCount;
	}
	public int getIndexCount()
	{
		return mIndexCount;
	}
	public int getVertexUploadBytes()
	{
		return mVertexUploadBytes;
	}
	public int getIndexUploadBytes()
	{
		return mIndexUploadBytes;
	}
	public FastSpriteBatchFrameProfile getLastProfile()
	{
		return mLastProfile;
	}
	internal bool hasRetainedInactiveElements()
	{
		return mRetainedInactiveCount > 0;
	}
	internal Int_ECSList getGPUDrivenSlotOrder()
	{
		return mGPUDrivenSlotOrder;
	}

	internal bool canRetainInactiveRenderer(FastSpriteRenderer renderer)
	{
		if (renderer == null || renderer.mBatch != this || renderer.mRetainedWhileDisabled)
		{
			return false;
		}
		int index = renderer.mBatchElementIndex;
		return (uint)index < (uint)mElements.Count && ReferenceEquals(mElements[index], renderer);
	}

	internal bool suspendRetainedRenderer(FastSpriteRenderer renderer)
	{
		if (!canRetainInactiveRenderer(renderer))
		{
			return false;
		}
		mActiveElementCount = Mathf.Max(0, mActiveElementCount - 1);
		++mRetainedInactiveCount;
		// Compat Mesh must patch the retained element's stable index slice. GPU-driven plans
		// intentionally retain inactive slots in persistent order, so visibility changes only
		// touch INSTANCE_FLAG_ACTIVE and must not dirty/rebuild Batch slot order.
		if (mOwner.getResolvedBackendMode() == FastSpriteBackendMode.CompatMesh &&
			!tryPatchRetainedVisibility(renderer, false))
		{
			mSortDirty = true;
		}
		return true;
	}

	internal bool resumeRetainedRenderer(FastSpriteRenderer renderer)
	{
		if (renderer == null || renderer.mBatch != this || !renderer.mRetainedWhileDisabled || mRetainedInactiveCount <= 0)
		{
			return false;
		}
		int index = renderer.mBatchElementIndex;
		if ((uint)index >= (uint)mElements.Count || !ReferenceEquals(mElements[index], renderer))
		{
			return false;
		}
		--mRetainedInactiveCount;
		++mActiveElementCount;
		// Compat Mesh restores the stable index slice. GPU-driven plans already keep this slot
		// in persistent order, so resume must not manufacture a Batch-order revision.
		if (mOwner.getResolvedBackendMode() == FastSpriteBackendMode.CompatMesh &&
			!tryPatchRetainedVisibility(renderer, true))
		{
			mSortDirty = true;
		}
		return true;
	}

	public bool add(FastSpriteRenderer renderer)
	{
		if (renderer == null)
		{
			return false;
		}
		int currentIndex = renderer.mBatchElementIndex;
		if ((uint)currentIndex < (uint)mElements.Count && ReferenceEquals(mElements[currentIndex], renderer))
		{
			return false;
		}
		int index = mElements.Count;
		mElements.Add(renderer);
		ensureElementCapacity(mElements.Count);
		renderer.mBatchElementIndex = index;
		renderer.mBatchDirtyQueued = false;
		if (renderer.mRetainedWhileDisabled)
		{
			++mRetainedInactiveCount;
		}
		else
		{
			++mActiveElementCount;
		}
		mTopologyDirty = true;
		invalidateOrderAndFallback();
		return false;
	}

	internal bool canFastTransferMembershipTo(FastSpriteBatch destination)
	{
		return destination != null && destination != this && destination.mOwner == mOwner;
	}

	internal bool tryFastTransferMembershipTo(FastSpriteBatch destination, FastSpriteRenderer renderer)
	{
		if (!canFastTransferMembershipTo(destination))
		{
			return false;
		}
		return tryFastTransferMembershipPrepared(destination, renderer);
	}

	internal bool tryFastTransferMembershipPrepared(FastSpriteBatch destination, FastSpriteRenderer renderer)
	{
		if (destination == null || renderer == null || renderer.mRetainedWhileDisabled)
		{
			return false;
		}
		int sourceIndex = renderer.mBatchElementIndex;
		if ((uint)sourceIndex >= (uint)mElements.Count || !ReferenceEquals(mElements[sourceIndex], renderer))
		{
			return false;
		}

		removeElementAt(sourceIndex, renderer, false);
		int destinationIndex = destination.mElements.Count;
		destination.mElements.Add(renderer);
		destination.ensureElementCapacity(destination.mElements.Count);
		++destination.mActiveElementCount;
		destination.mTopologyDirty = true;
		destination.invalidateOrderAndFallback();

		renderer.mBatch = destination;
		renderer.mBatchElementIndex = destinationIndex;
		renderer.mBatchDirtyQueued = false;
		return true;
	}

	public void remove(FastSpriteRenderer renderer)
	{
		if (renderer == null)
		{
			return;
		}
		int index = renderer.mBatchElementIndex;
		if ((uint)index >= (uint)mElements.Count || !ReferenceEquals(mElements[index], renderer))
		{
			index = mElements.IndexOf(renderer);
			if (index < 0)
			{
				renderer.mBatchElementIndex = -1;
				return;
			}
		}
		removeElementAt(index, renderer, true);
	}

	private void removeElementAt(int index, FastSpriteRenderer renderer, bool clearRendererIndex)
	{
		int last = mElements.Count - 1;
		if (index != last)
		{
			FastSpriteRenderer moved = mElements[last];
			mElements[index] = moved;
			if (moved != null)
			{
				moved.mBatchElementIndex = index;
			}
		}
		mElements.RemoveAt(last);
		if (renderer.mRetainedWhileDisabled)
		{
			mRetainedInactiveCount = Mathf.Max(0, mRetainedInactiveCount - 1);
		}
		else
		{
			mActiveElementCount = Mathf.Max(0, mActiveElementCount - 1);
		}
		if (clearRendererIndex)
		{
			renderer.mBatchElementIndex = -1;
		}
		renderer.mBatchDirtyQueued = false;
		mTopologyDirty = true;
		invalidateOrderAndFallback();
		if (mActiveElementCount == 0 && mMeshRenderer != null)
		{
			mMeshRenderer.enabled = false;
		}
	}

	public void markFullDirty()
	{
		mTopologyDirty = true;
		mSortDirty = true;
		mGPUDrivenSlotOrderValid = false;
		mLogicalMeshValid = false;
	}

	public void markSortDirty()
	{
		mSortDirty = true;
		mGPUDrivenSlotOrderValid = false;
		// Sorting changes do not invalidate cached geometry in the Compat Mesh backend.
		// The incremental path rewrites only the index stream.
	}

	public void notifyDirty(FastSpriteRenderer renderer)
	{
		if (renderer == null || renderer.mBatch != this || renderer.mBatchDirtyQueued)
		{
			return;
		}
		renderer.mBatchDirtyQueued = true;
		mDirtyElements.Add(renderer);

		FastSpriteDirtyFlags flags = renderer.peekDirtyFlags();
		if ((flags & FastSpriteDirtyFlags.Topology) != 0)
		{
			mTopologyDirty = true;
		}
		if ((flags & FastSpriteDirtyFlags.Sorting) != 0)
		{
			mSortDirty = true;
		}
		// Stable-topology mutations intentionally keep mLogicalMeshValid=true so Compat Mesh
		// can patch the renderer's cached range instead of rebuilding every element.
	}

	internal bool queueRootOnlyTransformPatch(FastSpriteRenderer renderer, Matrix4x4 rootToBatch, Matrix4x4 childLocalToRoot)
	{
		if (!mLogicalMeshValid || mTopologyDirty || renderer == null || renderer.mBatch != this)
		{
			return false;
		}
		if (renderer.mRetainedWhileDisabled || !renderer.mRuntimeActiveState)
		{
			return true;
		}
		int elementIndex = renderer.mBatchElementIndex;
		if ((uint)elementIndex >= (uint)mElements.Count || !ReferenceEquals(mElements[elementIndex], renderer))
		{
			return false;
		}
		if ((uint)elementIndex >= (uint)mRanges.Length || (uint)elementIndex >= (uint)mLocalToBatchCache.Length ||
			(uint)elementIndex >= (uint)mDirectRootMotionEpochs.Length)
		{
			return false;
		}
		mLocalToBatchCache[elementIndex] = CachedAffine2D.fromProduct(rootToBatch, childLocalToRoot);

		// A child-local Transform mutation already owns the ordinary Transform dirty path.
		// Keep one writer for that element and let the dirty queue consume the refreshed matrix.
		if ((renderer.peekDirtyFlags() & FastSpriteDirtyFlags.Transform) != 0)
		{
			return true;
		}
		if (mDirectRootMotionEpochs[elementIndex] == mDirectRootMotionEpoch)
		{
			return true;
		}
		mDirectRootMotionEpochs[elementIndex] = mDirectRootMotionEpoch;

		ElementRange range = mRanges[elementIndex];
		if (range.mVertexCount <= 0 || range.mVertexStart < 0 ||
			range.mVertexStart + range.mVertexCount > mVertices.Count ||
			range.mVertexStart + range.mVertexCount > mLocalPositions.Count)
		{
			return false;
		}
		if (range.mVertexCount == 4)
		{
			mDirectRootMotionQuadElements.Add(elementIndex);
		}
		else
		{
			mDirectRootMotionElements.Add(elementIndex);
		}
		return true;
	}

	public void setRenderEnabled(bool enabled)
	{
		if (mMeshRenderer != null)
		{
			mMeshRenderer.enabled = enabled && !mOwner.shouldBypassLogicalMeshSubmission() && mIndexCount > 0;
		}
	}

	internal void refreshGPUDrivenSlotOrder()
	{
		buildGPUDrivenDrawOrder();
		syncGPUDrivenSlotOrder();
		mSortDirty = false;
	}

	internal void updateGPUDrivenOnly()
	{
		mLastProfile = default;
		mVertexUploadBytes = 0;
		mIndexUploadBytes = 0;
		bool sortDirty = mSortDirty;
		for (int i = 0; i < mDirtyElements.Count; ++i)
		{
			FastSpriteRenderer renderer = mDirtyElements[i];
			if (renderer == null || renderer.mBatch != this)
			{
				continue;
			}
			FastSpriteDirtyFlags flags = renderer.peekDirtyFlags();
			if ((flags & FastSpriteDirtyFlags.Sorting) != 0)
			{
				sortDirty = true;
			}
			if ((flags & FastSpriteDirtyFlags.Topology) != 0)
			{
				mTopologyDirty = true;
			}
			renderer.clearDirtyFlags(flags);
			renderer.mBatchDirtyQueued = false;
			mLogicalMeshValid = false;
		}
		mDirtyElements.Clear();

		if (mKey.mGroupedStorage)
		{
			// RootOnly motion is consumed directly by the GPU root-matrix buffer and does not
			// dirty every child. Keep the dormant CPU fallback explicitly stale.
			if (mOwner.hasRootTransformChangesThisFrame())
			{
				mLogicalMeshValid = false;
			}
			mSortDirty = false;
			if (mMeshRenderer != null)
			{
				mMeshRenderer.enabled = false;
			}
			return;
		}
		if (sortDirty || !mGPUDrivenSlotOrderValid)
		{
			buildGPUDrivenDrawOrder();
			syncGPUDrivenSlotOrder();
			mSortDirty = false;
			mOwner.notifyGPUDrivenOrderDirty();
		}
		if (mMeshRenderer != null)
		{
			mMeshRenderer.enabled = false;
		}
	}

	public void update()
	{
		bool collectProfile = mOwner.isProfileStatsEnabled();
		long updateStart = collectProfile ? System.Diagnostics.Stopwatch.GetTimestamp() : 0L;
		mLastProfile = default;
		mVertexUploadBytes = 0;
		mIndexUploadBytes = 0;

		if (mActiveElementCount <= 0)
		{
			clearDirtyElementQueue();
			mVertexCount = 0;
			mIndexCount = 0;
			mLogicalMeshValid = true;
			if (mMeshRenderer != null)
			{
				mMeshRenderer.enabled = false;
			}
			if (collectProfile)
			{
				mLastProfile.mUpdateMS = elapsedProfileMS(updateStart);
			}
			return;
		}

		// Compat Mesh root motion is propagated to only the children of changed roots by the
		// system before Batch updates. It therefore stays on the same stable-range patch path.
		if (!mLogicalMeshValid || mTopologyDirty)
		{
			rebuildLogicalMesh(collectProfile);
		}
		else if (mSortDirty || mVisibilityIndexDirty || mDirtyElements.Count > 0 ||
			mDirectRootMotionElements.Count > 0 || mDirectRootMotionQuadElements.Count > 0)
		{
			updateLogicalMeshIncremental(collectProfile);
		}
		if (collectProfile)
		{
			mLastProfile.mUpdateMS = elapsedProfileMS(updateStart);
		}
	}

	private void updateLogicalMeshIncremental(bool collectProfile)
	{
		long topologyStart = collectProfile ? System.Diagnostics.Stopwatch.GetTimestamp() : 0L;
		bool sortDirty = mSortDirty;
		bool indexContentDirty = false;
		bool visibilityIndexDirty = mVisibilityIndexDirty;
		bool boundsExpanded = false;
		int minVertexUpload = int.MaxValue;
		int maxVertexUpload = 0;

		Vector3 boundsMin = mCachedBounds.min;
		Vector3 boundsMax = mCachedBounds.max;
		Matrix4x4 worldToBatch = mOwner.getWorldToBatchMatrix();

		long geometryStart = collectProfile ? System.Diagnostics.Stopwatch.GetTimestamp() : 0L;
		for (int i = 0; i < mDirectRootMotionQuadElements.Count; ++i)
		{
			int elementIndex = mDirectRootMotionQuadElements[i];
			ElementRange range = mRanges[elementIndex];
			CachedAffine2D localToBatch = mLocalToBatchCache[elementIndex];
			int vertexStart = range.mVertexStart;
			patchRootMotionQuadVertex(vertexStart, localToBatch, ref boundsMin, ref boundsMax, ref boundsExpanded);
			patchRootMotionQuadVertex(vertexStart + 1, localToBatch, ref boundsMin, ref boundsMax, ref boundsExpanded);
			patchRootMotionQuadVertex(vertexStart + 2, localToBatch, ref boundsMin, ref boundsMax, ref boundsExpanded);
			patchRootMotionQuadVertex(vertexStart + 3, localToBatch, ref boundsMin, ref boundsMax, ref boundsExpanded);
			minVertexUpload = Mathf.Min(minVertexUpload, vertexStart);
			maxVertexUpload = Mathf.Max(maxVertexUpload, vertexStart + 4);
			if (collectProfile)
			{
				++mLastProfile.mRootMotionScannedElementCount;
				++mLastProfile.mRootMotionDirtyElementCount;
				++mLastProfile.mRootMotionQuadFastPathCount;
			}
		}
		mDirectRootMotionQuadElements.Clear();

		for (int i = 0; i < mDirectRootMotionElements.Count; ++i)
		{
			int elementIndex = mDirectRootMotionElements[i];
			if ((uint)elementIndex >= (uint)mElements.Count)
			{
				rebuildLogicalMesh(collectProfile);
				return;
			}
			FastSpriteRenderer renderer = mElements[elementIndex];
			if (renderer == null || renderer.mBatch != this || renderer.mRetainedWhileDisabled || !renderer.mRuntimeActiveState)
			{
				continue;
			}
			ElementRange range = mRanges[elementIndex];
			if (range.mVertexCount <= 0 || range.mVertexStart < 0 ||
				range.mVertexStart + range.mVertexCount > mVertices.Count ||
				range.mVertexStart + range.mVertexCount > mLocalPositions.Count)
			{
				rebuildLogicalMesh(collectProfile);
				return;
			}
			CachedAffine2D localToBatch = mLocalToBatchCache[elementIndex];
			int vertexEnd = range.mVertexStart + range.mVertexCount;
			for (int vertexIndex = range.mVertexStart; vertexIndex < vertexEnd; ++vertexIndex)
			{
				FastSpriteVertex vertex = mVertices[vertexIndex];
				vertex.mPosition = localToBatch.multiplyPoint(mLocalPositions[vertexIndex]);
				mVertices[vertexIndex] = vertex;
				boundsExpanded |= expandBoundsIfNeeded(ref boundsMin, ref boundsMax, vertex.mPosition);
			}
			minVertexUpload = Mathf.Min(minVertexUpload, range.mVertexStart);
			maxVertexUpload = Mathf.Max(maxVertexUpload, vertexEnd);
			if (collectProfile)
			{
				++mLastProfile.mRootMotionScannedElementCount;
				++mLastProfile.mRootMotionDirtyElementCount;
			}
		}
		mDirectRootMotionElements.Clear();
		advanceDirectRootMotionEpoch();

		for (int i = 0; i < mDirtyElements.Count; ++i)
		{
			FastSpriteRenderer renderer = mDirtyElements[i];
			if (renderer == null || renderer.mBatch != this)
			{
				continue;
			}

			FastSpriteDirtyFlags flags = renderer.peekDirtyFlags();
			if ((flags & (FastSpriteDirtyFlags.Topology | FastSpriteDirtyFlags.Batch)) != 0)
			{
				rebuildLogicalMesh(collectProfile);
				return;
			}

			int elementIndex = renderer.mBatchElementIndex;
			if ((uint)elementIndex >= (uint)mElements.Count || !ReferenceEquals(mElements[elementIndex], renderer))
			{
				rebuildLogicalMesh(collectProfile);
				return;
			}
			// Dormant retained renderers keep their property bits until OnEnable re-submits them.
			// They must not invalidate stable ranges or force a full Compat rebuild while hidden.
			if (renderer.mRetainedWhileDisabled || !renderer.mRuntimeActiveState)
			{
				renderer.mBatchDirtyQueued = false;
				continue;
			}

			if ((flags & FastSpriteDirtyFlags.Sorting) != 0)
			{
				sortDirty = true;
			}
			ElementRange range = mRanges[elementIndex];
			if (range.mVertexCount <= 0 || range.mIndexCount <= 0 ||
				range.mVertexStart < 0 || range.mVertexStart + range.mVertexCount > mVertices.Count ||
				range.mVertexStart + range.mVertexCount > mLocalPositions.Count)
			{
				rebuildLogicalMesh(collectProfile);
				return;
			}

			// Pure sort metadata never requires vertex work.
			bool sortingOnly = (flags & FastSpriteDirtyFlags.Sorting) != 0 &&
				(flags & ~(FastSpriteDirtyFlags.Sorting | FastSpriteDirtyFlags.Bounds)) == FastSpriteDirtyFlags.None;
			bool colorOnly = (flags & FastSpriteDirtyFlags.Color) != 0 &&
				(flags & ~(FastSpriteDirtyFlags.Color | FastSpriteDirtyFlags.Bounds)) == FastSpriteDirtyFlags.None;
			bool transformOnly = (flags & FastSpriteDirtyFlags.Transform) != 0 &&
				(flags & ~(FastSpriteDirtyFlags.Transform | FastSpriteDirtyFlags.Bounds | FastSpriteDirtyFlags.Sorting)) == FastSpriteDirtyFlags.None;
			FastSpriteDirtyFlags simpleVisualMask = FastSpriteDirtyFlags.Vertex | FastSpriteDirtyFlags.Color;
			bool simpleUVOnly = flags == FastSpriteDirtyFlags.Vertex &&
				renderer.mCompatSimpleQuadUVOnly &&
				renderer.getDrawMode() == SpriteDrawMode.Simple &&
				range.mVertexCount == 4 && range.mIndexCount == 6;
			bool simpleUVColorOnly = (flags & FastSpriteDirtyFlags.Vertex) != 0 &&
				(flags & FastSpriteDirtyFlags.Color) != 0 &&
				(flags & ~simpleVisualMask) == FastSpriteDirtyFlags.None &&
				renderer.mCompatSimpleQuadUVOnly &&
				renderer.getDrawMode() == SpriteDrawMode.Simple &&
				range.mVertexCount == 4 && range.mIndexCount == 6;

			if (sortingOnly)
			{
				// Nothing to patch here; buildDrawOrder below consumes the new scalar.
			}
			else if ((simpleUVOnly || simpleUVColorOnly) &&
				renderer.tryGetSimpleQuadDataCached(out FastSpriteSimpleQuadData uvQuad))
			{
				FastSpriteVertex vertex0 = mVertices[range.mVertexStart + 0];
				FastSpriteVertex vertex1 = mVertices[range.mVertexStart + 1];
				FastSpriteVertex vertex2 = mVertices[range.mVertexStart + 2];
				FastSpriteVertex vertex3 = mVertices[range.mVertexStart + 3];
				vertex0.mUV = uvQuad.mUV0;
				vertex1.mUV = uvQuad.mUV1;
				vertex2.mUV = uvQuad.mUV2;
				vertex3.mUV = uvQuad.mUV3;
				if (simpleUVColorOnly)
				{
					Color32 color = renderer.getColor();
					vertex0.mColor = color;
					vertex1.mColor = color;
					vertex2.mColor = color;
					vertex3.mColor = color;
				}
				mVertices[range.mVertexStart + 0] = vertex0;
				mVertices[range.mVertexStart + 1] = vertex1;
				mVertices[range.mVertexStart + 2] = vertex2;
				mVertices[range.mVertexStart + 3] = vertex3;
				minVertexUpload = Mathf.Min(minVertexUpload, range.mVertexStart);
				maxVertexUpload = Mathf.Max(maxVertexUpload, range.mVertexStart + 4);
				if (collectProfile)
				{
					if (simpleUVColorOnly)
					{
						++mLastProfile.mUVColorOnlyElementCount;
					}
					else
					{
						++mLastProfile.mUVOnlyElementCount;
					}
				}
			}
			else if (colorOnly)
			{
				Color32 color = renderer.getColor();
				int vertexEnd = range.mVertexStart + range.mVertexCount;
				for (int vertexIndex = range.mVertexStart; vertexIndex < vertexEnd; ++vertexIndex)
				{
					FastSpriteVertex vertex = mVertices[vertexIndex];
					vertex.mColor = color;
					mVertices[vertexIndex] = vertex;
				}
				minVertexUpload = Mathf.Min(minVertexUpload, range.mVertexStart);
				maxVertexUpload = Mathf.Max(maxVertexUpload, range.mVertexStart + range.mVertexCount);
			}
			else if (transformOnly)
			{
				CachedAffine2D localToBatch = cacheCurrentLocalToBatch(elementIndex, renderer, worldToBatch);
				int vertexEnd = range.mVertexStart + range.mVertexCount;
				for (int vertexIndex = range.mVertexStart; vertexIndex < vertexEnd; ++vertexIndex)
				{
					FastSpriteVertex vertex = mVertices[vertexIndex];
					vertex.mPosition = localToBatch.multiplyPoint(mLocalPositions[vertexIndex]);
					mVertices[vertexIndex] = vertex;
					boundsExpanded |= expandBoundsIfNeeded(ref boundsMin, ref boundsMax, vertex.mPosition);
				}
				minVertexUpload = Mathf.Min(minVertexUpload, range.mVertexStart);
				maxVertexUpload = Mathf.Max(maxVertexUpload, range.mVertexStart + range.mVertexCount);
			}
			else if (renderer.getDrawMode() == SpriteDrawMode.Simple &&
				range.mVertexCount == 4 && range.mIndexCount == 6 &&
				renderer.tryGetSimpleQuadDataCached(out FastSpriteSimpleQuadData quad))
			{
				CachedAffine2D localToBatch = (flags & FastSpriteDirtyFlags.Transform) != 0
					? cacheCurrentLocalToBatch(elementIndex, renderer, worldToBatch)
					: mLocalToBatchCache[elementIndex];
				Color32 color = renderer.getColor();
				bool flipX = renderer.getFlipX();
				bool flipY = renderer.getFlipY();
				writeSimpleQuadVertex(range.mVertexStart + 0, quad.mPosition0, quad.mUV0, flipX, flipY, localToBatch, color, ref boundsMin, ref boundsMax, ref boundsExpanded);
				writeSimpleQuadVertex(range.mVertexStart + 1, quad.mPosition1, quad.mUV1, flipX, flipY, localToBatch, color, ref boundsMin, ref boundsMax, ref boundsExpanded);
				writeSimpleQuadVertex(range.mVertexStart + 2, quad.mPosition2, quad.mUV2, flipX, flipY, localToBatch, color, ref boundsMin, ref boundsMax, ref boundsExpanded);
				writeSimpleQuadVertex(range.mVertexStart + 3, quad.mPosition3, quad.mUV3, flipX, flipY, localToBatch, color, ref boundsMin, ref boundsMax, ref boundsExpanded);

				bool reverseWinding = flipX ^ flipY;
				int topologyKey = getSimpleQuadTopologyKey(quad, reverseWinding);
				if (mSimpleQuadTopologyKeys[elementIndex] != topologyKey)
				{
					writeSimpleQuadTopology(range, quad, reverseWinding);
					mSimpleQuadTopologyKeys[elementIndex] = topologyKey;
					indexContentDirty = true;
				}
				minVertexUpload = Mathf.Min(minVertexUpload, range.mVertexStart);
				maxVertexUpload = Mathf.Max(maxVertexUpload, range.mVertexStart + range.mVertexCount);
			}
			else
			{
				FastSpriteGeometryUtility.buildLocal(renderer, mScratchVertices, mScratchIndices);
				if (mScratchVertices.Count != range.mVertexCount || mScratchIndices.Count != range.mIndexCount)
				{
					rebuildLogicalMesh(collectProfile);
					return;
				}

				CachedAffine2D localToBatch = (flags & FastSpriteDirtyFlags.Transform) != 0
					? cacheCurrentLocalToBatch(elementIndex, renderer, worldToBatch)
					: mLocalToBatchCache[elementIndex];
				for (int v = 0; v < mScratchVertices.Count; ++v)
				{
					FastSpriteVertex localVertex = mScratchVertices[v];
					Vector3 localPosition = localVertex.mPosition;
					mLocalPositions[range.mVertexStart + v] = localPosition;

					FastSpriteVertex vertex = localVertex;
					vertex.mPosition = localToBatch.multiplyPoint(localPosition);
					vertex.mRootIndex = -1.0f;
					mVertices[range.mVertexStart + v] = vertex;
					boundsExpanded |= expandBoundsIfNeeded(ref boundsMin, ref boundsMax, vertex.mPosition);
				}

				for (int index = 0; index < mScratchIndices.Count; ++index)
				{
					int topologyIndex = range.mVertexStart + mScratchIndices[index];
					int destinationIndex = range.mIndexStart + index;
					if (mTopologyIndices[destinationIndex] == topologyIndex)
					{
						continue;
					}
					mTopologyIndices[destinationIndex] = topologyIndex;
					indexContentDirty = true;
				}
				minVertexUpload = Mathf.Min(minVertexUpload, range.mVertexStart);
				maxVertexUpload = Mathf.Max(maxVertexUpload, range.mVertexStart + range.mVertexCount);
			}

			renderer.clearDirtyFlags(flags);
			renderer.mBatchDirtyQueued = false;
			if (collectProfile)
			{
				++mLastProfile.mDirtyElementCount;
			}
		}
		mDirtyElements.Clear();
		if (collectProfile)
		{
			mLastProfile.mGeometryBuildMS = elapsedProfileMS(geometryStart);
		}

		long sortStart = collectProfile ? System.Diagnostics.Stopwatch.GetTimestamp() : 0L;
		if (sortDirty || indexContentDirty)
		{
			// A real order/topology change can move slices, so rebuild the compact order and
			// refresh its per-element index-start map. Visibility-only changes skip this block.
			rebuildDrawIndicesFromOrder();
			visibilityIndexDirty = false;
		}
		mVertexCount = mVertices.Count;
		if (collectProfile)
		{
			mLastProfile.mSortIndexMS = elapsedProfileMS(sortStart);
		}

		if (boundsExpanded)
		{
			mCachedBounds = new Bounds((boundsMin + boundsMax) * 0.5f, boundsMax - boundsMin);
			if (mCachedBounds.size.z < 0.01f)
			{
				Vector3 size = mCachedBounds.size;
				size.z = 0.01f;
				mCachedBounds.size = size;
			}
		}

		long uploadStart = collectProfile ? System.Diagnostics.Stopwatch.GetTimestamp() : 0L;
		MeshUpdateFlags meshFlags = MeshUpdateFlags.DontRecalculateBounds |
			MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontNotifyMeshUsers;

		if (minVertexUpload < maxVertexUpload && mMesh != null)
		{
			int uploadCount = maxVertexUpload - minVertexUpload;
			mMesh.SetVertexBufferData(mVertices, minVertexUpload, minVertexUpload, uploadCount, 0, meshFlags);
			mVertexUploadBytes = uploadCount * VERTEX_STRIDE;
			++mLastProfile.mVertexUploadCallCount;
		}

		bool fullIndexUpload = sortDirty || indexContentDirty;
		if (mMesh != null && (fullIndexUpload || visibilityIndexDirty))
		{
			if (mIndexCount > 0)
			{
				if (!fullIndexUpload && mVisibilityIndexDirtyStart < mVisibilityIndexDirtyEnd)
				{
					int uploadCount = mVisibilityIndexDirtyEnd - mVisibilityIndexDirtyStart;
					mMesh.SetIndexBufferData(mDrawIndices, mVisibilityIndexDirtyStart,
						mVisibilityIndexDirtyStart, uploadCount, meshFlags);
					mIndexUploadBytes = uploadCount * sizeof(int);
				}
				else
				{
					mMesh.SetIndexBufferData(mDrawIndices, 0, 0, mIndexCount, meshFlags);
					mIndexUploadBytes = mIndexCount * sizeof(int);
				}
				++mLastProfile.mIndexUploadCallCount;
			}
			// Visibility patching keeps the same index count/submesh, while full order rebuilds
			// can change it and therefore refresh the SubMeshDescriptor.
			if (fullIndexUpload)
			{
				mMesh.subMeshCount = 1;
				mMesh.SetSubMesh(0, new SubMeshDescriptor(0, mIndexCount, MeshTopology.Triangles)
				{
					bounds = mCachedBounds,
					vertexCount = mVertexCount,
				}, meshFlags);
			}
		}
		if (boundsExpanded && mMesh != null)
		{
			mMesh.bounds = mCachedBounds;
		}
		if (collectProfile)
		{
			mLastProfile.mVertexUploadMS = elapsedProfileMS(uploadStart);
		}

		mSortDirty = false;
		clearVisibilityIndexDirty();
		mLogicalMeshValid = true;
		if (mMeshRenderer != null)
		{
			mMeshRenderer.enabled = !mOwner.shouldBypassLogicalMeshSubmission() && mActiveElementCount > 0 && mIndexCount > 0;
		}
		if (collectProfile)
		{
			mLastProfile.mTopologyMS = elapsedProfileMS(topologyStart);
		}
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void patchRootMotionQuadVertex(int vertexIndex, CachedAffine2D localToBatch,
		ref Vector3 boundsMin, ref Vector3 boundsMax, ref bool boundsExpanded)
	{
		FastSpriteVertex vertex = mVertices[vertexIndex];
		vertex.mPosition = localToBatch.multiplyPoint(mLocalPositions[vertexIndex]);
		mVertices[vertexIndex] = vertex;
		boundsExpanded |= expandBoundsIfNeeded(ref boundsMin, ref boundsMax, vertex.mPosition);
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private CachedAffine2D cacheCurrentLocalToBatch(int elementIndex, FastSpriteRenderer renderer, Matrix4x4 worldToBatch)
	{
		CachedAffine2D cached = CachedAffine2D.fromMatrix(worldToBatch * mOwner.getTrackedLocalToWorldMatrix(renderer));
		mLocalToBatchCache[elementIndex] = cached;
		return cached;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private void writeSimpleQuadVertex(int destination, Vector2 local, Vector2 uv, bool flipX, bool flipY,
		CachedAffine2D localToBatch, Color32 color, ref Vector3 boundsMin, ref Vector3 boundsMax, ref bool boundsExpanded)
	{
		if (flipX)
		{
			local.x = -local.x;
		}
		if (flipY)
		{
			local.y = -local.y;
		}
		Vector3 localPosition = new(local.x, local.y, 0.0f);
		mLocalPositions[destination] = localPosition;
		FastSpriteVertex vertex = new(localPosition, color, uv)
		{
			mPosition = localToBatch.multiplyPoint(localPosition),
			mRootIndex = -1.0f
		};
		mVertices[destination] = vertex;
		boundsExpanded |= expandBoundsIfNeeded(ref boundsMin, ref boundsMax, vertex.mPosition);
	}

	private static int getSimpleQuadTopologyKey(FastSpriteSimpleQuadData quad, bool reverseWinding)
	{
		int key = reverseWinding ? 1 << 18 : 0;
		for (int i = 0; i < 6; ++i)
		{
			key |= (getSimpleQuadTriangleIndex(quad, i, reverseWinding) & 0x7) << (i * 3);
		}
		return key;
	}

	private void writeSimpleQuadTopology(ElementRange range, FastSpriteSimpleQuadData quad, bool reverseWinding)
	{
		for (int i = 0; i < 6; ++i)
		{
			mTopologyIndices[range.mIndexStart + i] = range.mVertexStart + getSimpleQuadTriangleIndex(quad, i, reverseWinding);
		}
	}

	private static int getSimpleQuadTriangleIndex(FastSpriteSimpleQuadData quad, int index, bool reverseWinding)
	{
		if (!reverseWinding)
		{
			return quad.getIndex(index);
		}
		int triangleOffset = index % 3;
		if (triangleOffset == 1)
		{
			return quad.getIndex(index + 1);
		}
		if (triangleOffset == 2)
		{
			return quad.getIndex(index - 1);
		}
		return quad.getIndex(index);
	}

	private bool tryPatchRetainedVisibility(FastSpriteRenderer renderer, bool visible)
	{
		// Keep GPU backend retention semantics exactly as before. The direct index patch is
		// exclusively a Compat Mesh optimization.
		if (mOwner.getResolvedBackendMode() != FastSpriteBackendMode.CompatMesh || !mLogicalMeshValid || mTopologyDirty || renderer == null)
		{
			return false;
		}
		int elementIndex = renderer.mBatchElementIndex;
		if ((uint)elementIndex >= (uint)mElements.Count || !ReferenceEquals(mElements[elementIndex], renderer) ||
			(uint)elementIndex >= (uint)mDrawIndexStarts.Length)
		{
			return false;
		}
		ElementRange range = mRanges[elementIndex];
		int drawIndexStart = mDrawIndexStarts[elementIndex];
		if (range.mIndexCount <= 0 || drawIndexStart < 0 || drawIndexStart + range.mIndexCount > mDrawIndices.Count)
		{
			return false;
		}

		int degenerateIndex = range.mVertexStart;
		for (int i = 0; i < range.mIndexCount; ++i)
		{
			mDrawIndices[drawIndexStart + i] = visible
				? mTopologyIndices[range.mIndexStart + i]
				: degenerateIndex;
		}
		mVisibilityIndexDirty = true;
		mVisibilityIndexDirtyStart = Mathf.Min(mVisibilityIndexDirtyStart, drawIndexStart);
		mVisibilityIndexDirtyEnd = Mathf.Max(mVisibilityIndexDirtyEnd, drawIndexStart + range.mIndexCount);
		mIndexCount = mDrawIndices.Count;
		return true;
	}

	private void clearVisibilityIndexDirty()
	{
		mVisibilityIndexDirty = false;
		mVisibilityIndexDirtyStart = int.MaxValue;
		mVisibilityIndexDirtyEnd = 0;
	}

	private void rebuildDrawIndicesFromOrder()
	{
		buildDrawOrder();
		for (int i = 0; i < mElements.Count && i < mDrawIndexStarts.Length; ++i)
		{
			mDrawIndexStarts[i] = -1;
		}
		mDrawIndices.Clear();
		for (int orderIndex = 0; orderIndex < mDrawOrder.Count; ++orderIndex)
		{
			int elementIndex = mDrawOrder[orderIndex];
			ElementRange range = mRanges[elementIndex];
			mDrawIndexStarts[elementIndex] = mDrawIndices.Count;
			for (int index = 0; index < range.mIndexCount; ++index)
			{
				mDrawIndices.Add(mTopologyIndices[range.mIndexStart + index]);
			}
		}
		mIndexCount = mDrawIndices.Count;
		clearVisibilityIndexDirty();
	}

	internal void submitLogicalMeshFromCPU()
	{
		if (mMesh == null || mMeshRenderer == null)
		{
			return;
		}
		if (mActiveElementCount <= 0)
		{
			mMeshRenderer.enabled = false;
			return;
		}
		if (!mLogicalMeshValid)
		{
			rebuildLogicalMesh(false);
		}
		applyRendererState();
		mMeshRenderer.enabled = mIndexCount > 0;
	}

	private void rebuildLogicalMesh(bool collectProfile)
	{
		long topologyStart = collectProfile ? System.Diagnostics.Stopwatch.GetTimestamp() : 0L;
		++mLastProfile.mTopologyRebuildCount;
		mVertices.Clear();
		mLocalPositions.Clear();
		mTopologyIndices.Clear();
		mDrawIndices.Clear();
		ensureElementCapacity(mElements.Count);
		Matrix4x4 worldToBatch = mOwner.getWorldToBatchMatrix();
		bool boundsValid = false;
		Vector3 boundsMin = default;
		Vector3 boundsMax = default;

		long geometryStart = collectProfile ? System.Diagnostics.Stopwatch.GetTimestamp() : 0L;
		for (int i = 0; i < mElements.Count; ++i)
		{
			FastSpriteRenderer renderer = mElements[i];
			mRanges[i] = default;
			mSimpleQuadTopologyKeys[i] = -1;
			if (renderer == null || renderer.mBatch != this)
			{
				continue;
			}
			if (renderer.mRetainedWhileDisabled || !renderer.mRuntimeActiveState)
			{
				// Keep dormant dirty flags for OnEnable, but release the transient Batch queue bit
				// because this full rebuild consumes and clears mDirtyElements below.
				renderer.mBatchDirtyQueued = false;
				continue;
			}
			if (collectProfile)
			{
				++mLastProfile.mDirtyElementCount;
			}

			FastSpriteGeometryUtility.buildLocal(renderer, mScratchVertices, mScratchIndices);
			int vertexStart = mVertices.Count;
			int indexStart = mTopologyIndices.Count;
			CachedAffine2D localToBatch = cacheCurrentLocalToBatch(i, renderer, worldToBatch);
			for (int v = 0; v < mScratchVertices.Count; ++v)
			{
				FastSpriteVertex localVertex = mScratchVertices[v];
				Vector3 localPosition = localVertex.mPosition;
				mLocalPositions.Add(localPosition);

				FastSpriteVertex vertex = localVertex;
				vertex.mPosition = localToBatch.multiplyPoint(localPosition);
				vertex.mRootIndex = -1.0f;
				mVertices.Add(vertex);
				encapsulatePoint(ref boundsValid, ref boundsMin, ref boundsMax, vertex.mPosition);
			}
			for (int index = 0; index < mScratchIndices.Count; ++index)
			{
				mTopologyIndices.Add(vertexStart + mScratchIndices[index]);
			}
			mRanges[i] = new ElementRange
			{
				mVertexStart = vertexStart,
				mVertexCount = mScratchVertices.Count,
				mIndexStart = indexStart,
				mIndexCount = mScratchIndices.Count,
			};
			if (mScratchVertices.Count == 4 && mScratchIndices.Count == 6 &&
				renderer.getDrawMode() == SpriteDrawMode.Simple && renderer.tryGetSimpleQuadDataCached(out FastSpriteSimpleQuadData rebuildQuad))
			{
				mSimpleQuadTopologyKeys[i] = getSimpleQuadTopologyKey(rebuildQuad, renderer.getFlipX() ^ renderer.getFlipY());
			}
			renderer.clearAllDirtyFlags();
			renderer.mBatchDirtyQueued = false;
		}
		mDirtyElements.Clear();
		mDirectRootMotionElements.Clear();
		advanceDirectRootMotionEpoch();
		if (collectProfile)
		{
			mLastProfile.mGeometryBuildMS = elapsedProfileMS(geometryStart);
		}

		long sortStart = collectProfile ? System.Diagnostics.Stopwatch.GetTimestamp() : 0L;
		rebuildDrawIndicesFromOrder();
		mVertexCount = mVertices.Count;
		if (collectProfile)
		{
			mLastProfile.mSortIndexMS = elapsedProfileMS(sortStart);
		}

		long boundsStart = collectProfile ? System.Diagnostics.Stopwatch.GetTimestamp() : 0L;
		mCachedBounds = createBounds(boundsValid, boundsMin, boundsMax);
		if (mCachedBounds.size.z < 0.01f)
		{
			Vector3 size = mCachedBounds.size;
			size.z = 0.01f;
			mCachedBounds.size = size;
		}
		if (collectProfile)
		{
			mLastProfile.mBoundsMS = elapsedProfileMS(boundsStart);
		}

		long uploadStart = collectProfile ? System.Diagnostics.Stopwatch.GetTimestamp() : 0L;
		configureAndUploadMesh();
		if (collectProfile)
		{
			mLastProfile.mVertexUploadMS = elapsedProfileMS(uploadStart);
		}
		mVertexUploadBytes = mVertexCount * VERTEX_STRIDE;
		mIndexUploadBytes = mIndexCount * sizeof(int);
		if (mVertexCount > 0)
		{
			++mLastProfile.mVertexUploadCallCount;
		}
		if (mIndexCount > 0)
		{
			++mLastProfile.mIndexUploadCallCount;
		}
		mTopologyDirty = false;
		mSortDirty = false;
		mLogicalMeshValid = true;
		if (collectProfile)
		{
			mLastProfile.mTopologyMS = elapsedProfileMS(topologyStart);
		}
	}

	private void configureAndUploadMesh()
	{
		if (mMesh == null)
		{
			return;
		}
		MeshUpdateFlags flags = MeshUpdateFlags.DontRecalculateBounds |
			MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontNotifyMeshUsers;
		mMesh.SetVertexBufferParams(Mathf.Max(mVertexCount, 1), VERTEX_LAYOUT);
		mMesh.SetIndexBufferParams(Mathf.Max(mIndexCount, 1), IndexFormat.UInt32);
		if (mVertexCount > 0)
		{
			mMesh.SetVertexBufferData(mVertices, 0, 0, mVertexCount, 0, flags);
		}
		if (mIndexCount > 0)
		{
			mMesh.SetIndexBufferData(mDrawIndices, 0, 0, mIndexCount, flags);
		}
		mMesh.bounds = mCachedBounds;
		mMesh.subMeshCount = 1;
		mMesh.SetSubMesh(0, new SubMeshDescriptor(0, mIndexCount, MeshTopology.Triangles)
		{
			bounds = mCachedBounds,
			vertexCount = mVertexCount,
		}, flags);
		applyRendererState();
		mMeshRenderer.enabled = !mOwner.shouldBypassLogicalMeshSubmission() && mIndexCount > 0;
	}

	private void buildGPUDrivenDrawOrder()
	{
		mDrawOrder.Clear();
		if (mKey.mGroupedStorage)
		{
			return;
		}
		FastSpriteSortMode mode = mOwner.getSortMode();
		FastSpriteRenderSystem.FastSortContext sortContext = mode != FastSpriteSortMode.Registration ? mOwner.buildSortContext() : default;
		bool hasSortValue = false;
		float minSortValue = 0.0f;
		float maxSortValue = 0.0f;
		ensureElementCapacity(mElements.Count);
		for (int i = 0; i < mElements.Count; ++i)
		{
			FastSpriteRenderer renderer = mElements[i];
			if (renderer == null || renderer.mBatch != this || renderer.mGPUDrivenSlot < 0 ||
				(!renderer.mRuntimeActiveState && !renderer.mRetainedWhileDisabled))
			{
				continue;
			}
			mDrawOrder.Add(i);
			if (mode == FastSpriteSortMode.Registration)
			{
				continue;
			}
			float sortValue = renderer.mSortValueCacheValid
				? renderer.mSortValueCache
				: FastSpriteRenderSystem.getSortValueFromMatrix(renderer, mOwner.getTrackedLocalToWorldMatrix(renderer), sortContext);
			if (!renderer.mSortValueCacheValid)
			{
				renderer.setSortValueCache(sortValue);
			}
			mSortValues[i] = sortValue;
			if (!hasSortValue)
			{
				minSortValue = maxSortValue = sortValue;
				hasSortValue = true;
			}
			else
			{
				minSortValue = Mathf.Min(minSortValue, sortValue);
				maxSortValue = Mathf.Max(maxSortValue, sortValue);
			}
		}
		sortDrawOrder(mode, hasSortValue, minSortValue, maxSortValue);
	}

	private void buildDrawOrder()
	{
		mDrawOrder.Clear();
		if (mKey.mGroupedStorage)
		{
			for (int i = 0; i < mElements.Count; ++i)
			{
				FastSpriteRenderer renderer = mElements[i];
				if (renderer != null && renderer.mBatch == this && !renderer.mRetainedWhileDisabled && renderer.mRuntimeActiveState && mRanges[i].mIndexCount > 0)
				{
					mDrawOrder.Add(i);
				}
			}
			return;
		}

		FastSpriteSortMode mode = mOwner.getSortMode();
		FastSpriteRenderSystem.FastSortContext sortContext = mode != FastSpriteSortMode.Registration ? mOwner.buildSortContext() : default;
		bool hasSortValue = false;
		float minSortValue = 0.0f;
		float maxSortValue = 0.0f;
		for (int i = 0; i < mElements.Count; ++i)
		{
			FastSpriteRenderer renderer = mElements[i];
			if (renderer == null || renderer.mBatch != this || renderer.mRetainedWhileDisabled || !renderer.mRuntimeActiveState || mRanges[i].mIndexCount <= 0)
			{
				continue;
			}
			mDrawOrder.Add(i);
			if (mode == FastSpriteSortMode.Registration)
			{
				continue;
			}
			float sortValue = renderer.mSortValueCacheValid
				? renderer.mSortValueCache
				: FastSpriteRenderSystem.getSortValueFromMatrix(renderer, mOwner.getTrackedLocalToWorldMatrix(renderer), sortContext);
			renderer.setSortValueCache(sortValue);
			mSortValues[i] = sortValue;
			if (!hasSortValue)
			{
				minSortValue = maxSortValue = sortValue;
				hasSortValue = true;
			}
			else
			{
				minSortValue = Mathf.Min(minSortValue, sortValue);
				maxSortValue = Mathf.Max(maxSortValue, sortValue);
			}
		}
		sortDrawOrder(mode, hasSortValue, minSortValue, maxSortValue);
	}

	private void sortDrawOrder(FastSpriteSortMode mode, bool hasSortValue, float minSortValue, float maxSortValue)
	{
		if (mode == FastSpriteSortMode.Registration || (hasSortValue && Mathf.Approximately(minSortValue, maxSortValue)))
		{
			mDrawOrder.Sort(compareRegistration);
		}
		else
		{
			mDrawOrder.Sort(compareCachedSortValue);
		}
	}

	private int compareRegistration(int a, int b)
	{
		long left = mElements[a].mRegistrationSequence;
		long right = mElements[b].mRegistrationSequence;
		return left < right ? -1 : left > right ? 1 : 0;
	}

	private int compareCachedSortValue(int a, int b)
	{
		float left = mSortValues[a];
		float right = mSortValues[b];
		if (!Mathf.Approximately(left, right))
		{
			return left > right ? -1 : 1;
		}
		return compareRegistration(a, b);
	}

	private void syncGPUDrivenSlotOrder()
	{
		mGPUDrivenSlotOrder.Clear();
		int drawCount = mDrawOrder.Count;
		if (drawCount <= 0)
		{
			mGPUDrivenSlotOrderValid = true;
			return;
		}
		mGPUDrivenSlotOrder.EnsureCount(drawCount);
		var slots = mGPUDrivenSlotOrder.getValueColumn();
		int write = 0;
		for (int i = 0; i < drawCount; ++i)
		{
			int elementIndex = mDrawOrder[i];
			if ((uint)elementIndex >= (uint)mElements.Count)
			{
				continue;
			}
			FastSpriteRenderer renderer = mElements[elementIndex];
			if (renderer == null || renderer.mBatch != this || renderer.mGPUDrivenSlot < 0 ||
				(!renderer.mRuntimeActiveState && !renderer.mRetainedWhileDisabled))
			{
				continue;
			}
			slots[write++] = renderer.mGPUDrivenSlot;
		}
		if (write < mGPUDrivenSlotOrder.Count)
		{
			mGPUDrivenSlotOrder.RemoveRange(write, mGPUDrivenSlotOrder.Count - write);
		}
		mGPUDrivenSlotOrderValid = true;
	}

	private void ensureElementCapacity(int count)
	{
		int capacity = Mathf.NextPowerOfTwo(Mathf.Max(count, 16));
		if (mRanges.Length < count)
		{
			Array.Resize(ref mRanges, capacity);
		}
		if (mLocalToBatchCache.Length < count)
		{
			Array.Resize(ref mLocalToBatchCache, capacity);
		}
		if (mSimpleQuadTopologyKeys.Length < count)
		{
			int oldLength = mSimpleQuadTopologyKeys.Length;
			Array.Resize(ref mSimpleQuadTopologyKeys, capacity);
			for (int i = oldLength; i < mSimpleQuadTopologyKeys.Length; ++i)
			{
				mSimpleQuadTopologyKeys[i] = -1;
			}
		}
		if (mSortValues.Length < count)
		{
			Array.Resize(ref mSortValues, capacity);
		}
		if (mDrawIndexStarts.Length < count)
		{
			int oldLength = mDrawIndexStarts.Length;
			Array.Resize(ref mDrawIndexStarts, capacity);
			for (int i = oldLength; i < mDrawIndexStarts.Length; ++i)
			{
				mDrawIndexStarts[i] = -1;
			}
		}
		if (mDirectRootMotionEpochs.Length < count)
		{
			Array.Resize(ref mDirectRootMotionEpochs, capacity);
		}
	}

	private void invalidateOrderAndFallback()
	{
		mSortDirty = true;
		mGPUDrivenSlotOrderValid = false;
		mLogicalMeshValid = false;
	}

	private void clearDirtyElementQueue()
	{
		for (int i = 0; i < mDirtyElements.Count; ++i)
		{
			FastSpriteRenderer renderer = mDirtyElements[i];
			if (renderer != null && renderer.mBatch == this)
			{
				renderer.mBatchDirtyQueued = false;
			}
		}
		mDirtyElements.Clear();
		clearDirectRootMotionQueue();
	}

	private void clearDirectRootMotionQueue()
	{
		mDirectRootMotionElements.Clear();
		mDirectRootMotionQuadElements.Clear();
		advanceDirectRootMotionEpoch();
	}

	private void advanceDirectRootMotionEpoch()
	{
		if (++mDirectRootMotionEpoch != 0)
		{
			return;
		}
		Array.Clear(mDirectRootMotionEpochs, 0, mDirectRootMotionEpochs.Length);
		mDirectRootMotionEpoch = 1;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool expandBoundsIfNeeded(ref Vector3 min, ref Vector3 max, Vector3 position)
	{
		bool changed = false;
		if (position.x < min.x)
		{
			min.x = position.x;
			changed = true;
		}
		else if (position.x > max.x)
		{
			max.x = position.x;
			changed = true;
		}
		if (position.y < min.y)
		{
			min.y = position.y;
			changed = true;
		}
		else if (position.y > max.y)
		{
			max.y = position.y;
			changed = true;
		}
		if (position.z < min.z)
		{
			min.z = position.z;
			changed = true;
		}
		else if (position.z > max.z)
		{
			max.z = position.z;
			changed = true;
		}
		return changed;
	}

	private static void encapsulatePoint(ref bool valid, ref Vector3 min, ref Vector3 max, Vector3 position)
	{
		if (!valid)
		{
			min = max = position;
			valid = true;
			return;
		}
		min = Vector3.Min(min, position);
		max = Vector3.Max(max, position);
	}

	private static Bounds createBounds(bool valid, Vector3 min, Vector3 max)
	{
		return valid ? new Bounds((min + max) * 0.5f, max - min) : new Bounds(Vector3.zero, Vector3.zero);
	}

	private static double elapsedProfileMS(long start)
	{
		return (System.Diagnostics.Stopwatch.GetTimestamp() - start) * 1000.0 / System.Diagnostics.Stopwatch.Frequency;
	}

	private void createRenderObject()
	{
		mRenderObject = new("[FastSpriteBatch] " + mKey.mSortingLayerID + ":" + mKey.mSortingOrder + ":" + mKey.mTextureID, typeof(MeshFilter), typeof(MeshRenderer))
		{
			hideFlags = HideFlags.HideAndDontSave
		};
		mRenderObject.transform.SetParent(mOwner.transform, false);
		mMeshFilter = mRenderObject.GetComponent<MeshFilter>();
		mMeshRenderer = mRenderObject.GetComponent<MeshRenderer>();
		mMesh = new Mesh
		{
			name = "[FastSprite] Batch Mesh",
			hideFlags = HideFlags.HideAndDontSave
		};
		mMesh.MarkDynamic();
		mMeshFilter.sharedMesh = mMesh;
		mPropertyBlock = new MaterialPropertyBlock();
		applyRendererState();
	}

	private void applyRendererState()
	{
		if (mMeshRenderer == null)
		{
			return;
		}
		mMeshRenderer.sharedMaterial = mKey.mMaterial;
		mMeshRenderer.sortingLayerID = mKey.mSortingLayerID;
		mMeshRenderer.sortingOrder = mKey.mSortingOrder;
		mPropertyBlock.Clear();
		if (mKey.mTexture != null)
		{
			mPropertyBlock.SetTexture(MAIN_TEX_ID, mKey.mTexture);
			mPropertyBlock.SetTexture(BASE_MAP_ID, mKey.mTexture);
		}
		mMeshRenderer.SetPropertyBlock(mPropertyBlock);
	}

	public void Dispose()
	{
		mGPUPlanCachedSpan = null;
		clearDirtyElementQueue();
		for (int i = 0; i < mElements.Count; ++i)
		{
			FastSpriteRenderer renderer = mElements[i];
			if (renderer != null && renderer.mBatch == this)
			{
				renderer.mBatch = null;
				renderer.mBatchElementIndex = -1;
				renderer.mBatchDirtyQueued = false;
			}
		}
		if (mMesh != null)
		{
			if (Application.isPlaying)
			{
				UnityEngine.Object.Destroy(mMesh);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(mMesh);
			}
			mMesh = null;
		}
		if (mRenderObject != null)
		{
			if (Application.isPlaying)
			{
				UnityEngine.Object.Destroy(mRenderObject);
			}
			else
			{
				UnityEngine.Object.DestroyImmediate(mRenderObject);
			}
			mRenderObject = null;
		}
		mElements.Clear();
		mDrawOrder.Clear();
		mGPUDrivenSlotOrder.Dispose();
	}
}
