using System;
using System.Collections.Generic;
using EasyECS;
using UnityEngine;

// RootOnly assumes child local transforms stay stable. Call
// FastSpriteRenderer.notifyTransformChanged() when a child local transform changes.
public enum FastSpriteGroupTransformTrackingMode
{
	PerRenderer,
	RootOnly,
}

// Atomic sorting unit: the group owns external layer/order while child order remains local.
[ExecuteAlways]
[DisallowMultipleComponent]
public sealed class FastSpriteSortGroup : MonoBehaviour
{
	[SerializeField] private int mSortingLayerID;
	[SerializeField] private int mSortingOrder;
	[SerializeField] private FastSpriteGroupTransformTrackingMode mTransformTrackingMode = FastSpriteGroupTransformTrackingMode.PerRenderer;

	private readonly List<FastSpriteRenderer> mRenderers = new(8);
	private readonly List<FastSpriteRenderer> mOrderedRenderers = new(8);
	private readonly Int_ECSList mGPUDrivenOrderedSlots = new(8);
	private int mGPUDrivenSlotRevision = -1;
	private bool mChildOrderDirty = true;
	private FastSpriteRenderSystem mOwner;
	[NonSerialized] internal FastSpriteGPUPlanCachedSpan mGPUPlanCachedSpan;
	private bool mRegisteredWithOwner;
	private int mContentRevision;
	private bool mRootTransformCacheValid;
	private Matrix4x4 mRootTransformCache;
	[NonSerialized] internal bool mRuntimeActiveState;
	// Lets retained RootOnly children detect root motion that happened while dormant.
	private int mRootTransformRevision;
	[NonSerialized] internal int mRootTrackingIndex = -1;

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

	public FastSpriteGroupTransformTrackingMode transformTrackingMode
	{
		get => mTransformTrackingMode;
		set => setTransformTrackingMode(value);
	}

	public int getSortingLayerID()
	{
		return mSortingLayerID;
	}
	public int getSortingOrder()
	{
		return mSortingOrder;
	}
	public FastSpriteGroupTransformTrackingMode getTransformTrackingMode()
	{
		return mTransformTrackingMode;
	}
	public bool usesRootOnlyTransformTracking()
	{
		return mTransformTrackingMode == FastSpriteGroupTransformTrackingMode.RootOnly;
	}
	internal bool isRuntimeActive()
	{
		return mRuntimeActiveState;
	}
	internal int getRootTransformRevision()
	{
		return mRootTransformRevision;
	}
	internal List<FastSpriteRenderer> getRenderersUnsafe()
	{
		return mRenderers;
	}
	private void ensureOrderedRendererCache()
	{
		if (!mChildOrderDirty)
		{
			return;
		}
		mOrderedRenderers.Clear();
		for (int i = 0; i < mRenderers.Count; ++i)
		{
			FastSpriteRenderer renderer = mRenderers[i];
			if (renderer != null)
			{
				mOrderedRenderers.Add(renderer);
			}
		}
		mOrderedRenderers.Sort(compareChildOrder);
		mChildOrderDirty = false;
	}
	internal Int_ECSList getGPUDrivenOrderedSlots(FastSpriteRenderSystem owner)
	{
		ensureOrderedRendererCache();
		if (mGPUDrivenSlotRevision == mContentRevision)
		{
			return mGPUDrivenOrderedSlots;
		}
		mGPUDrivenOrderedSlots.Clear();
		if (mOrderedRenderers.Count > 0)
		{
			mGPUDrivenOrderedSlots.EnsureCount(mOrderedRenderers.Count);
		}
		var slots = mGPUDrivenOrderedSlots.getValueColumn();
		int write = 0;
		for (int i = 0; i < mOrderedRenderers.Count; ++i)
		{
			FastSpriteRenderer renderer = mOrderedRenderers[i];
			if (renderer == null || renderer.mSystemOwner != owner || renderer.getSortGroup() != this || renderer.mBatch == null || renderer.mGPUDrivenSlot < 0)
			{
				continue;
			}
			slots[write++] = renderer.mGPUDrivenSlot;
		}
		if (write < mGPUDrivenOrderedSlots.Count)
		{
			mGPUDrivenOrderedSlots.RemoveRange(write, mGPUDrivenOrderedSlots.Count - write);
		}
		mGPUDrivenSlotRevision = mContentRevision;
		return mGPUDrivenOrderedSlots;
	}

	private static int compareChildOrder(FastSpriteRenderer left, FastSpriteRenderer right)
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
		int layerLeft = SortingLayer.GetLayerValueFromID(left.getSortingLayerID());
		int layerRight = SortingLayer.GetLayerValueFromID(right.getSortingLayerID());
		if (layerLeft != layerRight)
		{
			return layerLeft < layerRight ? -1 : 1;
		}
		if (left.getSortingOrder() != right.getSortingOrder())
		{
			return left.getSortingOrder() < right.getSortingOrder() ? -1 : 1;
		}
		return left.mRegistrationSequence < right.mRegistrationSequence ? -1 : left.mRegistrationSequence > right.mRegistrationSequence ? 1 : 0;
	}

	public void setTransformTrackingMode(FastSpriteGroupTransformTrackingMode value)
	{
		if (mTransformTrackingMode == value)
		{
			return;
		}
		mTransformTrackingMode = value;
		mRootTransformCacheValid = false;
		ensureOwnerFromChildren();
		if (mOwner != null)
		{
			mOwner.notifySortGroupTransformTrackingChanged(this);
		}
	}

	internal bool pollRootTransformChanged(out Matrix4x4 localToWorld)
	{
		if (!mRootTransformCacheValid)
		{
			localToWorld = transform.localToWorldMatrix;
			mRootTransformCache = localToWorld;
			mRootTransformCacheValid = true;
			transform.hasChanged = false;
			unchecked
		{
			++mRootTransformRevision;
		}
			return true;
		}
		if (!transform.hasChanged)
		{
			localToWorld = mRootTransformCache;
			return false;
		}
		localToWorld = transform.localToWorldMatrix;
		mRootTransformCache = localToWorld;
		transform.hasChanged = false;
		unchecked
		{
			++mRootTransformRevision;
		}
		return true;
	}
	public void setSortingLayerID(int value)
	{
		if (mSortingLayerID == value)
		{
			return;
		}
		mSortingLayerID = value;
		ensureOwnerFromChildren();
		if (mOwner != null)
		{
			mOwner.notifySortGroupOrderDirty(this);
		}
	}

	public void setSortingOrder(int value)
	{
		if (mSortingOrder == value)
		{
			return;
		}
		mSortingOrder = value;
		ensureOwnerFromChildren();
		if (mOwner != null)
		{
			mOwner.notifySortGroupOrderDirty(this);
		}
	}

	internal void registerRenderer(FastSpriteRenderer renderer)
	{
		if (renderer == null)
		{
			return;
		}
		if (!mRenderers.Contains(renderer))
		{
			mRenderers.Add(renderer);
			mChildOrderDirty = true;
			++mContentRevision;
			mGPUDrivenSlotRevision = -1;
		}
		if (renderer.mSystemOwner != null)
		{
			bindOwner(renderer.mSystemOwner);
			if (mOwner != null)
			{
				mOwner.notifySortGroupRendererAdded(this, renderer);
			}
			if (mOwner != null)
			{
				mOwner.notifySortGroupContentDirty();
			}
		}
	}

	internal void unregisterRenderer(FastSpriteRenderer renderer)
	{
		if (renderer == null)
		{
			return;
		}
		if (mRenderers.Remove(renderer))
		{
			if (mOwner != null)
			{
				mOwner.notifySortGroupRendererRemoved(renderer);
			}
			mChildOrderDirty = true;
			++mContentRevision;
			mGPUDrivenSlotRevision = -1;
			if (mOwner != null)
			{
				mOwner.notifySortGroupContentDirty();
			}
		}
		if (mRenderers.Count == 0 && mOwner != null && mRegisteredWithOwner)
		{
			mOwner.unregisterSortGroup(this);
			mRegisteredWithOwner = false;
		}
	}

	internal void notifyChildOrderDirty()
	{
		mChildOrderDirty = true;
		++mContentRevision;
		mGPUDrivenSlotRevision = -1;
		ensureOwnerFromChildren();
		if (mOwner != null)
		{
			mOwner.notifySortGroupContentDirty();
		}
	}

	internal void bindOwner(FastSpriteRenderSystem owner)
	{
		if (owner == null)
		{
			return;
		}
		if (mOwner == owner)
		{
			if (!mRegisteredWithOwner)
			{
				mOwner.registerSortGroup(this);
				mRegisteredWithOwner = true;
			}
			return;
		}

		if (mOwner != null && mRegisteredWithOwner)
		{
			mOwner.unregisterSortGroup(this);
		}
		mOwner = owner;
		mOwner.registerSortGroup(this);
		mRegisteredWithOwner = true;
	}

	private void ensureOwnerFromChildren()
	{
		if (mOwner != null)
		{
			return;
		}
		for (int i = 0; i < mRenderers.Count; ++i)
		{
			FastSpriteRenderer renderer = mRenderers[i];
			if (renderer != null && renderer.mSystemOwner != null)
			{
				bindOwner(renderer.mSystemOwner);
				return;
			}
		}
	}

	private void refreshChildBindings()
	{
		FastSpriteRenderer[] renderers = GetComponentsInChildren<FastSpriteRenderer>(true);
		for (int i = 0; i < renderers.Length; ++i)
		{
			if (renderers[i] != null)
			{
				renderers[i].refreshSortGroupBinding();
			}
		}
	}

	private void OnEnable()
	{
		mRuntimeActiveState = true;
		mRootTransformCacheValid = false;
		refreshChildBindings();
		ensureOwnerFromChildren();
		if (mOwner != null)
		{
			mOwner.notifySortGroupContentDirty();
		}
	}

	private void OnDisable()
	{
		mRuntimeActiveState = false;
		mRootTransformCacheValid = false;
		refreshChildBindings();
		if (mOwner != null && mRegisteredWithOwner)
		{
			mOwner.unregisterSortGroup(this);
			mRegisteredWithOwner = false;
		}
	}

	private void OnDestroy()
	{
		mRuntimeActiveState = false;
		if (mOwner != null && mRegisteredWithOwner)
		{
			mOwner.unregisterSortGroup(this);
			mRegisteredWithOwner = false;
		}
		mGPUDrivenOrderedSlots.Dispose();
	}

	private void OnValidate()
	{
		mRootTransformCacheValid = false;
		ensureOwnerFromChildren();
		if (mOwner != null)
		{
			mOwner.notifySortGroupTransformTrackingChanged(this);
		}
		if (mOwner != null)
		{
			mOwner.notifySortGroupOrderDirty(this);
		}
		if (mOwner != null)
		{
			mOwner.notifySortGroupContentDirty();
		}
	}
}
