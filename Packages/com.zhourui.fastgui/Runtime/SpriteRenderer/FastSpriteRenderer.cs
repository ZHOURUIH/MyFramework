using System;
using UnityEngine;

// Auto polls transforms. Static/Manual skip polling; notifyTransformChanged() is the
// synchronization point when code intentionally moves an object in either mode.
public enum FastSpriteTransformTrackingMode
{
	Auto,
	Static,
	Manual,
}

// Lightweight scene Sprite component rendered by FastSpriteRenderSystem.
// The public properties intentionally mirror SpriteRenderer where practical.
[ExecuteAlways]
[DisallowMultipleComponent]
public sealed class FastSpriteRenderer : MonoBehaviour
{
	[SerializeField] private Sprite mSprite;
	[SerializeField] private Color mColor = Color.white;
	[SerializeField] private bool mFlipX;
	[SerializeField] private bool mFlipY;
	[SerializeField] private Material mMaterial;
	[SerializeField] private SpriteDrawMode mDrawMode = SpriteDrawMode.Simple;
	[SerializeField] private Vector2 mSize = Vector2.one;
	[SerializeField] private SpriteTileMode mTileMode = SpriteTileMode.Continuous;
	[SerializeField, Range(0.0f, 1.0f)] private float mAdaptiveModeThreshold = 0.5f;
	[SerializeField] private SpriteMaskInteraction mMaskInteraction = SpriteMaskInteraction.None;
	[SerializeField] private SpriteSortPoint mSpriteSortPoint = SpriteSortPoint.Center;
	[SerializeField] private int mSortingLayerID;
	[SerializeField] private int mSortingOrder;
	[SerializeField] private FastSpriteTransformTrackingMode mTransformTrackingMode = FastSpriteTransformTrackingMode.Auto;

	[NonSerialized] internal FastSpriteDirtyFlags mDirtyFlags = FastSpriteDirtyFlags.All;
	[NonSerialized] internal FastSpriteBatch mBatch;
	[NonSerialized] internal int mBatchElementIndex = -1;
	[NonSerialized] internal FastSpriteRenderSystem mSystemOwner;
	[NonSerialized] internal int mSystemRendererIndex = -1;
	[NonSerialized] internal long mRegistrationSequence;
	[NonSerialized] internal bool mTransformCacheValid;
	[NonSerialized] internal bool mSortValueCacheValid;
	[NonSerialized] internal float mSortValueCache;
	[NonSerialized] internal bool mLocalSortPointCacheValid;
	[NonSerialized] internal Vector3 mLocalSortPointCache;
	[NonSerialized] internal bool mBatchDirtyQueued;
	[NonSerialized] internal bool mMembershipDirtyQueued;
	[NonSerialized] internal FastSpriteSortGroup mSortGroup;
	[NonSerialized] internal bool mRetainedWhileDisabled;
	[NonSerialized] internal int mRetainedRootTransformRevision;
	[NonSerialized] internal bool mCompatSimpleQuadUVOnly;
	[NonSerialized] internal bool mRuntimeActiveState;
	[NonSerialized] internal bool mRootTrackingLocalToGroupValid;
	[NonSerialized] internal Matrix4x4 mRootTrackingLocalToGroup;
	[NonSerialized] internal int mRootTrackingChildIndex = -1;
	[NonSerialized] internal int mIndependentTransformTrackingIndex = -1;
	[NonSerialized] internal int mGPUDrivenSlot = -1;
	[NonSerialized] internal int mGPUDrivenGeometryOffset = -1;
	[NonSerialized] internal int mGPUDrivenGeometryCapacity;
	[NonSerialized] internal int mGPUDrivenGeometryVertexCount;
	[NonSerialized] internal FastSpriteDirtyFlags mGPUDrivenDirtyFlags = FastSpriteDirtyFlags.All;
	[NonSerialized] internal bool mGPUDrivenDirtyQueued;
	[NonSerialized] internal int mGPUDrivenDirectVisualEpoch;
	// Simple topology/quad metadata is cached because the GPU-driven QuadAsset path and
	// topology checks both need it; avoid repeated Sprite mesh metadata lookups.
	[NonSerialized] private bool mSimpleMeshInfoCacheValid;
	[NonSerialized] private bool mSimpleMeshQuadCacheValid;
	[NonSerialized] private int mSimpleMeshVertexCount;
	[NonSerialized] private int mSimpleMeshIndexCount;
	[NonSerialized] private bool mSimpleMeshQuadValid;
	[NonSerialized] private FastSpriteSimpleQuadData mSimpleMeshQuad;
	[NonSerialized] private Vector3 mSimpleMeshBoundsCenter;
	[NonSerialized] private Texture mSimpleMeshTexture;
	[NonSerialized] private int mSimpleMeshGeometryID;
	[NonSerialized] private int mSimpleMeshAssetID;

	private Action<FastSpriteRenderer> mSpriteChanged;

	public Sprite sprite
	{
		get => mSprite;
		set => setSprite(value);
	}
	public Color color
	{
		get => mColor;
		set => setColor(value);
	}
	public bool flipX
	{
		get => mFlipX;
		set => setFlipX(value);
	}
	public bool flipY
	{
		get => mFlipY;
		set => setFlipY(value);
	}
	// FastSpriteRenderer does not instantiate a private material when reading material.
	// material/sharedMaterial intentionally resolve to the same shared reference.
	public Material material
	{
		get => mMaterial;
		set => setMaterial(value);
	}
	public Material sharedMaterial
	{
		get => mMaterial;
		set => setMaterial(value);
	}
	public SpriteDrawMode drawMode
	{
		get => mDrawMode;
		set => setDrawMode(value);
	}
	public Vector2 size
	{
		get => mSize;
		set => setSize(value);
	}
	public SpriteTileMode tileMode
	{
		get => mTileMode;
		set => setTileMode(value);
	}
	public float adaptiveModeThreshold
	{
		get => mAdaptiveModeThreshold;
		set => setAdaptiveModeThreshold(value);
	}
	public SpriteMaskInteraction maskInteraction
	{
		get => mMaskInteraction;
		set => setMaskInteraction(value);
	}
	public SpriteSortPoint spriteSortPoint
	{
		get => mSpriteSortPoint;
		set => setSpriteSortPoint(value);
	}
	public int sortingLayerID
	{
		get => mSortingLayerID;
		set => setSortingLayerID(value);
	}
	public string sortingLayerName
	{
		get => SortingLayer.IDToName(mSortingLayerID);
		set => setSortingLayerID(SortingLayer.NameToID(value));
	}
	public int sortingOrder
	{
		get => mSortingOrder;
		set => setSortingOrder(value);
	}
	public FastSpriteTransformTrackingMode transformTrackingMode
	{
		get => mTransformTrackingMode;
		set => setTransformTrackingMode(value);
	}
	public Bounds bounds => FastSpriteGeometryUtility.getWorldBounds(this);
	public Bounds localBounds => FastSpriteGeometryUtility.getLocalBounds(this);

	public Sprite getSprite()
	{
		return mSprite;
	}
	public Color getColor()
	{
		return mColor;
	}
	public bool getFlipX()
	{
		return mFlipX;
	}
	public bool getFlipY()
	{
		return mFlipY;
	}
	public Material getMaterial()
	{
		return mMaterial;
	}
	public SpriteDrawMode getDrawMode()
	{
		return mDrawMode;
	}
	public Vector2 getSize()
	{
		return mSize;
	}
	public SpriteTileMode getTileMode()
	{
		return mTileMode;
	}
	public float getAdaptiveModeThreshold()
	{
		return mAdaptiveModeThreshold;
	}
	public SpriteMaskInteraction getMaskInteraction()
	{
		return mMaskInteraction;
	}
	public SpriteSortPoint getSpriteSortPoint()
	{
		return mSpriteSortPoint;
	}
	public int getSortingLayerID()
	{
		return mSortingLayerID;
	}
	public int getSortingOrder()
	{
		return mSortingOrder;
	}
	public FastSpriteTransformTrackingMode getTransformTrackingMode()
	{
		return mTransformTrackingMode;
	}
	public FastSpriteSortGroup getSortGroup()
	{
		return mSortGroup;
	}

	internal void refreshSortGroupBinding()
	{
		FastSpriteSortGroup next = findNearestActiveSortGroup();
		if (next == mSortGroup)
		{
			return;
		}
		if (mRetainedWhileDisabled)
		{
			if (mSystemOwner != null)
			{
				mSystemOwner.cancelRetainedRenderer(this);
			}
		}
		FastSpriteSortGroup previous = mSortGroup;
		mSortGroup = next;
		mRootTrackingLocalToGroupValid = false;
		if (previous != null)
		{
			previous.unregisterRenderer(this);
		}
		if (mSortGroup != null)
		{
			mSortGroup.registerRenderer(this);
		}
		if (mSystemOwner != null)
		{
			mSystemOwner.notifyTransformTrackingModeChanged(this);
		}
		if (isActiveAndEnabled && mSystemOwner != null)
		{
			// Grouped renderers use a stable Geometry-storage key; entering/leaving a
			// group therefore requires one membership migration, but group order churn does not.
			markDirty(FastSpriteDirtyFlags.Batch);
		}
	}

	private FastSpriteSortGroup findNearestActiveSortGroup()
	{
		Transform current = transform;
		while (current != null)
		{
			FastSpriteSortGroup group = current.GetComponent<FastSpriteSortGroup>();
			if (group != null && group.isActiveAndEnabled)
			{
				return group;
			}
			current = current.parent;
		}
		return null;
	}
	public void setTransformTrackingMode(FastSpriteTransformTrackingMode value)
	{
		if (mTransformTrackingMode == value)
		{
			return;
		}
		mTransformTrackingMode = value;
		mTransformCacheValid = false;
		if (mSystemOwner != null)
		{
			mSystemOwner.notifyTransformTrackingModeChanged(this);
		}
		// Snapshot the current transform immediately so Static/Manual never start from stale
		// geometry merely because they opt out of the next polling pass.
		notifyTransformChanged();
	}

	public void notifyTransformChanged()
	{
		mTransformCacheValid = false;
		if (mSystemOwner != null)
		{
			mSystemOwner.notifyManualTransformChanged(this);
		}
		else
		{
			markDirty(FastSpriteDirtyFlags.Transform | FastSpriteDirtyFlags.Bounds);
		}
	}

	internal void cacheRootTrackingLocalToGroup(FastSpriteSortGroup group)
	{
		cacheRootTrackingLocalToGroup(group, transform.localToWorldMatrix);
	}

	internal void cacheRootTrackingLocalToGroup(FastSpriteSortGroup group, Matrix4x4 localToWorld)
	{
		if (group == null)
		{
			mRootTrackingLocalToGroupValid = false;
			return;
		}
		mRootTrackingLocalToGroup = group.transform.worldToLocalMatrix * localToWorld;
		mRootTrackingLocalToGroupValid = true;
	}
	public void refreshAll()
	{
		invalidateSortValueCache();
		invalidateLocalSortPointCache();
		markDirty(FastSpriteDirtyFlags.All);
	}

	private void ensureSimpleMeshInfoCached()
	{
		if (mSimpleMeshInfoCacheValid)
		{
			return;
		}
		FastSpriteGeometryUtility.getSimpleMeshInfo(
			mSprite,
			out mSimpleMeshVertexCount,
			out mSimpleMeshIndexCount,
			out mSimpleMeshQuadValid,
			out mSimpleMeshQuad,
			out mSimpleMeshBoundsCenter,
			out mSimpleMeshTexture,
			out mSimpleMeshGeometryID,
			out mSimpleMeshAssetID);
		mSimpleMeshInfoCacheValid = true;
		mSimpleMeshQuadCacheValid = true;
	}

	private void ensureSimpleMeshQuadCached()
	{
		ensureSimpleMeshInfoCached();
		if (mSimpleMeshQuadCacheValid)
		{
			return;
		}
		mSimpleMeshQuadValid = FastSpriteGeometryUtility.tryGetSimpleQuadData(mSprite, out mSimpleMeshQuad);
		mSimpleMeshQuadCacheValid = true;
	}

	private void getSimpleMeshInfoCached(out int vertexCount, out int indexCount, out bool quadValid, out FastSpriteSimpleQuadData quad)
	{
		ensureSimpleMeshQuadCached();
		vertexCount = mSimpleMeshVertexCount;
		indexCount = mSimpleMeshIndexCount;
		quadValid = mSimpleMeshQuadValid;
		quad = mSimpleMeshQuad;
	}

	internal bool tryGetSimpleQuadDataCached(out FastSpriteSimpleQuadData quad)
	{
		getSimpleMeshInfoCached(out _, out _, out bool quadValid, out quad);
		return quadValid;
	}

	internal bool tryGetSimpleAssetIDCached(out int simpleAssetID)
	{
		ensureSimpleMeshInfoCached();
		simpleAssetID = mSimpleMeshAssetID;
		return mSimpleMeshQuadValid && simpleAssetID > 0;
	}

	public void setSprite(Sprite value)
	{
		// Reference identity is the batching contract; avoid UnityEngine.Object operator== on
		// the animation hot path. Destroyed-object pseudo-null semantics do not change a live
		// renderer's sprite identity.
		if (ReferenceEquals(mSprite, value))
		{
			return;
		}
		mCompatSimpleQuadUVOnly = false;
		Sprite oldSprite = mSprite;
		int oldVertexCount = 0;
		int oldIndexCount = 0;
		int oldGeometryID = 0;
		Texture oldTexture;
		FastSpriteDirtyFlags flags;
		Texture newTexture;
		bool sameSimpleQuadGeometry = false;

		if (mDrawMode == SpriteDrawMode.Simple)
		{
			ensureSimpleMeshInfoCached();
			oldVertexCount = mSimpleMeshVertexCount;
			oldIndexCount = mSimpleMeshIndexCount;
			oldGeometryID = mSimpleMeshGeometryID;
			oldTexture = mSimpleMeshTexture;

			FastSpriteGeometryUtility.getSimpleMeshHeader(
				value,
				out int newVertexCount,
				out int newIndexCount,
				out bool newQuadValid,
				out Vector3 newBoundsCenter,
				out newTexture,
				out int newGeometryID,
				out int newAssetID);

			sameSimpleQuadGeometry = oldVertexCount == 4 && oldIndexCount == 6 &&
				newVertexCount == 4 && newIndexCount == 6 &&
				oldGeometryID != 0 && oldGeometryID == newGeometryID;

			if (sameSimpleQuadGeometry && ReferenceEquals(oldTexture, newTexture))
			{
				mSprite = value;
				mSimpleMeshVertexCount = newVertexCount;
				mSimpleMeshIndexCount = newIndexCount;
				mSimpleMeshQuadValid = newQuadValid;
				mSimpleMeshQuadCacheValid = false;
				mSimpleMeshBoundsCenter = newBoundsCenter;
				mSimpleMeshTexture = newTexture;
				mSimpleMeshGeometryID = newGeometryID;
				mSimpleMeshAssetID = newAssetID;
				mSimpleMeshInfoCacheValid = true;
				mCompatSimpleQuadUVOnly = true;
				FastSpriteRenderSystem owner = mSystemOwner;
				if (owner == null || !owner.tryCommitGPUSpriteAsset(this, newAssetID))
				{
					markDirty(FastSpriteDirtyFlags.Vertex);
				}
				mSpriteChanged?.Invoke(this);
				return;
			}

			// Preserve the previous numeric point only for the uncommon path where Sprite
			// geometry or texture can affect sorting/bounds.
			Vector3 oldLocalSortPoint = mLocalSortPointCacheValid ? mLocalSortPointCache : getLocalSortPointCached();
			FastSpriteGeometryUtility.tryGetSimpleQuadData(value, out FastSpriteSimpleQuadData newQuad);
			mSprite = value;
			mSimpleMeshVertexCount = newVertexCount;
			mSimpleMeshIndexCount = newIndexCount;
			mSimpleMeshQuadValid = newQuadValid;
			mSimpleMeshQuad = newQuad;
			mSimpleMeshQuadCacheValid = true;
			mSimpleMeshBoundsCenter = newBoundsCenter;
			mSimpleMeshTexture = newTexture;
			mSimpleMeshGeometryID = newGeometryID;
			mSimpleMeshAssetID = newAssetID;
			mSimpleMeshInfoCacheValid = true;

			if (sameSimpleQuadGeometry)
			{
				mCompatSimpleQuadUVOnly = true;
				flags = FastSpriteDirtyFlags.Vertex;
			}
			else
			{
				flags = oldVertexCount == newVertexCount && oldIndexCount == newIndexCount
					? FastSpriteDirtyFlags.Vertex
					: FastSpriteDirtyFlags.Topology | FastSpriteDirtyFlags.Geometry;
			}

			bool textureChanged = !ReferenceEquals(oldTexture, newTexture);
			if (textureChanged)
			{
				// A cross-texture Sprite migration rebuilds destination draw order.
				invalidateSortValueCache();
			}

			bool localSortPointChanged = false;
			if (!sameSimpleQuadGeometry)
			{
				Vector3 newLocalSortPoint = calculateLocalSortPointFromCurrentState();
				localSortPointChanged = oldLocalSortPoint.x != newLocalSortPoint.x ||
					oldLocalSortPoint.y != newLocalSortPoint.y || oldLocalSortPoint.z != newLocalSortPoint.z;
				mLocalSortPointCache = newLocalSortPoint;
				mLocalSortPointCacheValid = true;
				if (localSortPointChanged && mSystemOwner != null)
				{
					mSystemOwner.syncTransformSortPoint(this, newLocalSortPoint);
				}
			}

			// Identical local geometry cannot change bounds. Avoid turning an atlas-frame swap
			// into world-position/bounds work even when it migrates to another texture.
			if (!sameSimpleQuadGeometry)
			{
				flags |= FastSpriteDirtyFlags.Bounds;
			}
			if (textureChanged)
			{
				flags |= FastSpriteDirtyFlags.Batch;
			}
			else if (localSortPointChanged && mSortGroup == null && mSpriteSortPoint == SpriteSortPoint.Center &&
				refreshSortValueAfterVisualChange())
			{
				flags |= FastSpriteDirtyFlags.Sorting;
			}
			markDirty(flags);
			mSpriteChanged?.Invoke(this);
			return;
		}

		oldTexture = oldSprite != null ? oldSprite.texture : null;
		Vector3 oldNonSimpleSortPoint = mLocalSortPointCacheValid ? mLocalSortPointCache : getLocalSortPointCached();
		mSprite = value;
		mSimpleMeshInfoCacheValid = false;
		mSimpleMeshQuadCacheValid = false;

		if (mDrawMode == SpriteDrawMode.Sliced)
		{
			newTexture = mSprite != null ? mSprite.texture : null;
			bool oldHasBorder = oldSprite != null && oldSprite.border != Vector4.zero;
			bool newHasBorder = mSprite != null && mSprite.border != Vector4.zero;
			flags = oldHasBorder == newHasBorder
				? FastSpriteDirtyFlags.Geometry
				: FastSpriteDirtyFlags.Topology | FastSpriteDirtyFlags.Geometry;
		}
		else
		{
			newTexture = mSprite != null ? mSprite.texture : null;
			// Tiled quad count can change with Sprite rect / border / PPU.
			flags = FastSpriteDirtyFlags.Topology | FastSpriteDirtyFlags.Geometry;
		}

		bool nonSimpleTextureChanged = !ReferenceEquals(oldTexture, newTexture);
		if (nonSimpleTextureChanged)
		{
			invalidateSortValueCache();
		}

		Vector3 newNonSimpleSortPoint = calculateLocalSortPointFromCurrentState();
		bool nonSimpleSortPointChanged = oldNonSimpleSortPoint.x != newNonSimpleSortPoint.x ||
			oldNonSimpleSortPoint.y != newNonSimpleSortPoint.y || oldNonSimpleSortPoint.z != newNonSimpleSortPoint.z;
		mLocalSortPointCache = newNonSimpleSortPoint;
		mLocalSortPointCacheValid = true;
		if (nonSimpleSortPointChanged && mSystemOwner != null)
		{
			mSystemOwner.syncTransformSortPoint(this, newNonSimpleSortPoint);
		}

		flags |= FastSpriteDirtyFlags.Bounds;
		if (nonSimpleTextureChanged)
		{
			flags |= FastSpriteDirtyFlags.Batch;
		}
		markDirty(flags);
		mSpriteChanged?.Invoke(this);
	}

	public void setColor(Color value)
	{
		if (mColor.r == value.r && mColor.g == value.g && mColor.b == value.b && mColor.a == value.a)
		{
			return;
		}
		mColor = value;
		markDirty(FastSpriteDirtyFlags.Color);
	}

	public void setFlipX(bool value)
	{
		if (mFlipX == value)
		{
			return;
		}
		mFlipX = value;
		invalidateLocalSortPointCache();
		FastSpriteDirtyFlags flags = FastSpriteDirtyFlags.Vertex | FastSpriteDirtyFlags.Bounds;
		if (mSortGroup == null && mSpriteSortPoint == SpriteSortPoint.Center && mDrawMode == SpriteDrawMode.Simple && refreshSortValueAfterVisualChange())
		{
			flags |= FastSpriteDirtyFlags.Sorting;
		}
		markDirty(flags);
	}

	public void setFlipY(bool value)
	{
		if (mFlipY == value)
		{
			return;
		}
		mFlipY = value;
		invalidateLocalSortPointCache();
		FastSpriteDirtyFlags flags = FastSpriteDirtyFlags.Vertex | FastSpriteDirtyFlags.Bounds;
		if (mSortGroup == null && mSpriteSortPoint == SpriteSortPoint.Center && mDrawMode == SpriteDrawMode.Simple && refreshSortValueAfterVisualChange())
		{
			flags |= FastSpriteDirtyFlags.Sorting;
		}
		markDirty(flags);
	}

	public void setMaterial(Material value)
	{
		if (mMaterial == value)
		{
			return;
		}
		mMaterial = value;
		markDirty(FastSpriteDirtyFlags.Batch);
	}

	public void setDrawMode(SpriteDrawMode value)
	{
		if (mDrawMode == value)
		{
			return;
		}
		mDrawMode = value;
		invalidateLocalSortPointCache();
		// Switching between Simple and Sliced/Tiled changes how Center sort point is
		// interpreted, so the draw order may need to be refreshed as well.
		FastSpriteDirtyFlags flags = FastSpriteDirtyFlags.Topology | FastSpriteDirtyFlags.Geometry | FastSpriteDirtyFlags.Bounds;
		if (mSortGroup == null && refreshSortValueAfterVisualChange())
		{
			flags |= FastSpriteDirtyFlags.Sorting;
		}
		markDirty(flags);
	}

	public void setSize(Vector2 value)
	{
		value.x = Mathf.Max(0.0f, value.x);
		value.y = Mathf.Max(0.0f, value.y);
		if (mSize == value)
		{
			return;
		}
		mSize = value;
		// Tiled can change quad count when size changes. Sliced keeps a stable topology,
		// but using Topology here keeps the first implementation correct for all modes.
		markDirty(mDrawMode == SpriteDrawMode.Tiled
			? FastSpriteDirtyFlags.Topology | FastSpriteDirtyFlags.Geometry
			: FastSpriteDirtyFlags.Geometry);
	}

	public void setTileMode(SpriteTileMode value)
	{
		if (mTileMode == value)
		{
			return;
		}
		mTileMode = value;
		if (mDrawMode == SpriteDrawMode.Tiled)
		{
			markDirty(FastSpriteDirtyFlags.Topology | FastSpriteDirtyFlags.Geometry);
		}
	}

	public void setAdaptiveModeThreshold(float value)
	{
		value = Mathf.Clamp01(value);
		if (Mathf.Approximately(mAdaptiveModeThreshold, value))
		{
			return;
		}
		mAdaptiveModeThreshold = value;
		if (mDrawMode == SpriteDrawMode.Tiled && mTileMode == SpriteTileMode.Adaptive)
		{
			markDirty(FastSpriteDirtyFlags.Topology | FastSpriteDirtyFlags.Geometry);
		}
	}

	public void setMaskInteraction(SpriteMaskInteraction value)
	{
		if (mMaskInteraction == value)
		{
			return;
		}
		mMaskInteraction = value;
		// SpriteMask rendering is not implemented; retain the value for API/serialization compatibility.
		markDirty(FastSpriteDirtyFlags.Batch);
	}

	public void setSpriteSortPoint(SpriteSortPoint value)
	{
		if (mSpriteSortPoint == value)
		{
			return;
		}
		mSpriteSortPoint = value;
		invalidateLocalSortPointCache();
		if (mSortGroup == null && refreshSortValueAfterVisualChange())
		{
			markDirty(FastSpriteDirtyFlags.Sorting);
		}
	}

	public void setSortingLayerID(int value)
	{
		if (mSortingLayerID == value)
		{
			return;
		}
		mSortingLayerID = value;
		if (mSortGroup != null)
		{
			// Inside a SortGroup this is local ordering metadata, not Geometry ownership.
			mSortGroup.notifyChildOrderDirty();
			return;
		}
		markDirty(FastSpriteDirtyFlags.Batch);
	}

	public void setSortingOrder(int value)
	{
		if (mSortingOrder == value)
		{
			return;
		}
		mSortingOrder = value;
		if (mSortGroup != null)
		{
			mSortGroup.notifyChildOrderDirty();
			return;
		}
		FastSpriteDirtyFlags flags = FastSpriteDirtyFlags.Batch | FastSpriteDirtyFlags.SortingOrderBatch;
		mDirtyFlags |= flags;
		FastSpriteRenderSystem.notifyRendererDirty(this, flags);
	}

	public void RegisterSpriteChangeCallback(Action<FastSpriteRenderer> callback)
	{
		mSpriteChanged += callback;
	}

	public void UnregisterSpriteChangeCallback(Action<FastSpriteRenderer> callback)
	{
		mSpriteChanged -= callback;
	}

	internal void markDirty(FastSpriteDirtyFlags flags)
	{
		FastSpriteRenderSystem owner = mSystemOwner;
		if (owner != null && owner.tryQueueGPUVisualDirty(this, flags))
		{
			return;
		}

		mDirtyFlags |= flags;
		if (!mRuntimeActiveState && mRetainedWhileDisabled)
		{
			return;
		}
		FastSpriteBatch batch = mBatch;
		if (batch != null && !mBatchDirtyQueued)
		{
			batch.notifyDirty(this);
		}

		if (owner != null && batch != null && (flags & FastSpriteDirtyFlags.Batch) == 0 &&
			owner.getResolvedBackendMode() == FastSpriteBackendMode.CompatMesh)
		{
			if (mSortGroup == null && !mLocalSortPointCacheValid &&
				(flags & (FastSpriteDirtyFlags.Vertex | FastSpriteDirtyFlags.Geometry |
					FastSpriteDirtyFlags.Topology | FastSpriteDirtyFlags.Sorting)) != 0)
			{
				getLocalSortPointCached();
			}
			return;
		}
		FastSpriteRenderSystem.notifyRendererDirty(this, flags);
	}

	internal void markTransformDirty(bool sortValueChanged)
	{
		FastSpriteDirtyFlags flags = FastSpriteDirtyFlags.Transform | FastSpriteDirtyFlags.Bounds;
		if (sortValueChanged && mSortGroup == null)
		{
			flags |= FastSpriteDirtyFlags.Sorting;
		}
		mDirtyFlags |= flags;
		FastSpriteBatch batch = mBatch;
		if (batch != null && !mBatchDirtyQueued)
		{
			batch.notifyDirty(this);
		}
		if (mSystemOwner != null)
		{
			mSystemOwner.notifyGPUDrivenRendererDirty(this, flags);
		}
	}

	internal void markCompatTransformDirty()
	{
		FastSpriteDirtyFlags flags = FastSpriteDirtyFlags.Transform | FastSpriteDirtyFlags.Bounds;
		mDirtyFlags |= flags;
		FastSpriteBatch batch = mBatch;
		if (batch != null && !mBatchDirtyQueued)
		{
			batch.notifyDirty(this);
		}
	}

	private bool refreshSortValueAfterVisualChange()
	{
		FastSpriteRenderSystem owner = mSystemOwner;
		if (owner == null || owner.getSortMode() == FastSpriteSortMode.Registration)
		{
			return false;
		}
		return refreshSortValueCache(owner.getSortValue(this));
	}

	internal FastSpriteDirtyFlags peekDirtyFlags()
	{
		return mDirtyFlags;
	}

	internal bool refreshSortValueCache(float value)
	{
		if (!mSortValueCacheValid)
		{
			mSortValueCache = value;
			mSortValueCacheValid = true;
			return true;
		}
		// Orthographic 2D movement commonly changes X/Y while leaving the transparent
		// sort scalar bit-identical. Skip Mathf.Approximately entirely for that hot path.
		if (mSortValueCache == value)
		{
			return false;
		}
		bool changed = !Mathf.Approximately(mSortValueCache, value);
		mSortValueCache = value;
		return changed;
	}

	internal void setSortValueCache(float value)
	{
		mSortValueCache = value;
		mSortValueCacheValid = true;
	}

	internal void invalidateSortValueCache()
	{
		mSortValueCacheValid = false;
	}

	internal void clearDirtyFlags(FastSpriteDirtyFlags flags)
	{
		mDirtyFlags &= ~flags;
		if ((flags & FastSpriteDirtyFlags.Vertex) != 0)
		{
			mCompatSimpleQuadUVOnly = false;
		}
	}

	internal void clearAllDirtyFlags()
	{
		mDirtyFlags = FastSpriteDirtyFlags.None;
		mCompatSimpleQuadUVOnly = false;
	}

	internal bool pollTransformChanged()
	{
		// Transform.hasChanged is substantially cheaper than reading/comparing a full
		// localToWorld matrix for every static Sprite each frame.
		if (!mTransformCacheValid)
		{
			mTransformCacheValid = true;
			transform.hasChanged = false;
			return true;
		}
		if (!transform.hasChanged)
		{
			return false;
		}
		transform.hasChanged = false;
		return true;
	}

	private Vector3 calculateLocalSortPointFromCurrentState()
	{
		Vector3 localPoint = Vector3.zero;
		if (mSpriteSortPoint == SpriteSortPoint.Center && mSprite != null && mDrawMode == SpriteDrawMode.Simple)
		{
			getSimpleMeshInfoCached(out _, out _, out _, out _);
			localPoint = mSimpleMeshBoundsCenter;
			if (mFlipX)
			{
				localPoint.x = -localPoint.x;
			}
			if (mFlipY)
			{
				localPoint.y = -localPoint.y;
			}
		}
		return localPoint;
	}

	internal Vector3 getLocalSortPointCached()
	{
		if (mLocalSortPointCacheValid)
		{
			return mLocalSortPointCache;
		}
		Vector3 localPoint = calculateLocalSortPointFromCurrentState();
		mLocalSortPointCache = localPoint;
		mLocalSortPointCacheValid = true;
		// EasyECS transform-sort state mirrors only this pure numeric input. Sync it
		// when the cached point is actually rebuilt, not on every frame/transform.
		if (mSystemOwner != null)
		{
			mSystemOwner.syncTransformSortPoint(this, localPoint);
		}
		return localPoint;
	}

	internal void invalidateLocalSortPointCache()
	{
		mLocalSortPointCacheValid = false;
	}

	internal Vector3 getWorldSortPoint()
	{
		return transform.TransformPoint(getLocalSortPointCached());
	}

	private void OnEnable()
	{
		mRuntimeActiveState = true;
		if (mRetainedWhileDisabled)
		{
			FastSpriteDirtyFlags pendingFlags = mDirtyFlags;
			if (mSystemOwner != null && mBatch != null && mSystemOwner.resumeRetainedRenderer(this))
			{
				if (mSortGroup != null && !mSortGroup.isRuntimeActive())
				{
					refreshSortGroupBinding();
				}

				FastSpriteBackendMode backendMode = mSystemOwner.getResolvedBackendMode();
				bool rootOnly = mSortGroup != null && mSortGroup.usesRootOnlyTransformTracking();
				if (rootOnly)
				{
					bool childTransformPending = (pendingFlags & FastSpriteDirtyFlags.Transform) != 0;
					if (backendMode == FastSpriteBackendMode.CompatMesh)
					{
						bool rootMovedWhileDormant = mRetainedRootTransformRevision != mSortGroup.getRootTransformRevision();
						if (rootMovedWhileDormant || childTransformPending)
						{
							if (childTransformPending && !mRootTrackingLocalToGroupValid)
							{
								notifyTransformChanged();
							}
							else
							{
								markCompatTransformDirty();
							}
						}
					}
					else if (childTransformPending)
					{
						// An explicit/manual child-local change is rare and must refresh the cached
						// local-to-root matrix. Root motion alone needs no child instance rewrite.
						notifyTransformChanged();
					}
				}
				else
				{
					bool gpuRetained = backendMode != FastSpriteBackendMode.CompatMesh;
					// Ungrouped/ordinary renderers keep SpriteRenderer-like transform discovery.
					// GPU retained slots only need a transform refresh if Unity reports a real move.
					bool transformChangedWhileDormant = !gpuRetained || pollTransformChanged();
					if (transformChangedWhileDormant)
					{
						if (mSortGroup != null)
						{
							mSortValueCacheValid = false;
						}
						mLocalSortPointCacheValid = false;
						mRootTrackingLocalToGroupValid = false;
						notifyTransformChanged();
					}
				}

				// Property mutations made while disabled were intentionally not sent through the
				// active producer lane. Re-submit those pre-existing non-transform bits exactly once.
				pendingFlags &= ~(FastSpriteDirtyFlags.Transform | FastSpriteDirtyFlags.Bounds);
				if (pendingFlags != FastSpriteDirtyFlags.None)
				{
					mBatch?.notifyDirty(this);
					FastSpriteRenderSystem.notifyRendererDirty(this, pendingFlags);
				}
				return;
			}
			// Defensive recovery: if the retained slot can no longer resume, detach it
			// transactionally before falling back to a normal registration.
			if (mSystemOwner != null)
			{
				mSystemOwner.cancelRetainedRenderer(this);
			}
		}

		mRetainedWhileDisabled = false;
		refreshSortGroupBinding();
		mTransformCacheValid = false;
		mSortValueCacheValid = false;
		mLocalSortPointCacheValid = false;
		mRootTrackingLocalToGroupValid = false;
		mDirtyFlags = FastSpriteDirtyFlags.All;
		FastSpriteRenderSystem.registerRenderer(this);
	}

	private void OnDisable()
	{
		mRuntimeActiveState = false;
		// Pooled grouped/fixed-slot renderers may retain their stable GPU-plan slot.
		// Everything else keeps the ordinary unregister behavior.
		if (mSystemOwner != null && mSystemOwner.tryRetainRendererWhileDisabled(this))
		{
			mRetainedWhileDisabled = true;
			mRetainedRootTransformRevision = mSortGroup != null && mSortGroup.usesRootOnlyTransformTracking()
				? mSortGroup.getRootTransformRevision()
				: 0;
			// Ungrouped batches need the previous scalar on resume to distinguish a real
			// depth move from a pure visibility toggle. Grouped order is owned by SortGroup.
			if (mSortGroup != null)
			{
				mSortValueCacheValid = false;
			}
			mLocalSortPointCacheValid = false;
			mMembershipDirtyQueued = false;
			return;
		}

		mRetainedWhileDisabled = false;
		mRetainedRootTransformRevision = 0;
		FastSpriteRenderSystem.unregisterRenderer(this);
		if (mSortGroup != null)
		{
			mSortGroup.unregisterRenderer(this);
		}
		mSortGroup = null;
		mRootTrackingLocalToGroupValid = false;
		mBatch = null;
		mBatchElementIndex = -1;
		mSortValueCacheValid = false;
		mLocalSortPointCacheValid = false;
		mBatchDirtyQueued = false;
		mMembershipDirtyQueued = false;
	}

	private void OnDestroy()
	{
		mRuntimeActiveState = false;
		// OnDisable may have retained this renderer. Destruction must always remove the
		// dormant source from the system/group/batch for real.
		FastSpriteRenderSystem.unregisterRenderer(this);
		if (mSortGroup != null)
		{
			mSortGroup.unregisterRenderer(this);
		}
		mRetainedWhileDisabled = false;
		mRetainedRootTransformRevision = 0;
		mSortGroup = null;
		mRootTrackingLocalToGroupValid = false;
		mBatch = null;
		mBatchElementIndex = -1;
		mSortValueCacheValid = false;
		mLocalSortPointCacheValid = false;
		mBatchDirtyQueued = false;
		mMembershipDirtyQueued = false;
	}

	private void OnTransformParentChanged()
	{
		refreshSortGroupBinding();
	}

	private void OnValidate()
	{
		mSize.x = Mathf.Max(0.0f, mSize.x);
		mSize.y = Mathf.Max(0.0f, mSize.y);
		mAdaptiveModeThreshold = Mathf.Clamp01(mAdaptiveModeThreshold);
		mDirtyFlags |= FastSpriteDirtyFlags.All;
		mSimpleMeshInfoCacheValid = false;
		mSimpleMeshQuadCacheValid = false;
		invalidateSortValueCache();
		invalidateLocalSortPointCache();
		mRootTrackingLocalToGroupValid = false;
		if (mSystemOwner != null)
		{
			mSystemOwner.notifyTransformTrackingModeChanged(this);
		}
		mBatch?.notifyDirty(this);
		FastSpriteRenderSystem.notifyRendererDirty(this, FastSpriteDirtyFlags.All);
	}
}

[Flags]
internal enum FastSpriteDirtyFlags
{
	None = 0,
	Vertex = 1 << 0,
	Geometry = 1 << 1,
	Topology = 1 << 2,
	Batch = 1 << 3,
	Sorting = 1 << 4,
	Transform = 1 << 5,
	Bounds = 1 << 6,
	Color = 1 << 7,
	// Reason bit for a Batch dirty caused specifically by SortingOrder. It is not
	// part of All: generic full invalidation must never masquerade as an order-only
	// migration.
	SortingOrderBatch = 1 << 8,
	All = Vertex | Geometry | Topology | Batch | Sorting | Transform | Bounds | Color,
}
