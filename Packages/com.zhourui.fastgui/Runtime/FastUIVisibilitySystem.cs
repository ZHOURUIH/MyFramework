using System.Collections.Generic;
using UnityEngine;

// FastCanvas的可见性状态子系统。
// Managed Root来自FastUIVisibility/FastUIRenderElement，本身已经绑定到当前Canvas；
// External Root来自FastCanvas.setVisible(Transform)，需要保留完整层级合法性校验。
public sealed class FastUIVisibilitySystem
{
	private readonly HashSet<Transform> mHiddenRoots = new();
	private readonly HashSet<Transform> mManagedHiddenRoots = new();
	private readonly FastUIRangeData_ECSList mHiddenRenderRanges;
	private readonly FastUIRangeData_ECSList mIndexHiddenRenderRanges;
	private readonly FastUIRangeData_ECSList mPreviousHiddenRenderRanges;
	private readonly FastUIRangeData_ECSList mPreviousIndexHiddenRenderRanges;
	private readonly List<Transform> mRemoveCache = new();
	private bool mDirty = true;
	private bool mValidateManagedRoots;
	public FastUIVisibilitySystem(int rangeCapacity)
	{
		int capacity = Mathf.Max(rangeCapacity, 4);
		mHiddenRenderRanges = new FastUIRangeData_ECSList(capacity);
		mIndexHiddenRenderRanges = new FastUIRangeData_ECSList(capacity);
		mPreviousHiddenRenderRanges = new FastUIRangeData_ECSList(capacity);
		mPreviousIndexHiddenRenderRanges = new FastUIRangeData_ECSList(capacity);
	}
	public FastUIRangeData_ECSList getDrawHiddenRanges()
	{
		return mHiddenRenderRanges;
	}
	public FastUIRangeData_ECSList getIndexHiddenRanges()
	{
		return mIndexHiddenRenderRanges;
	}
	public int getHiddenRootCount()
	{
		return mHiddenRoots.Count + mManagedHiddenRoots.Count;
	}
	public int getHiddenRangeCount()
	{
		return mHiddenRenderRanges.Count + mIndexHiddenRenderRanges.Count;
	}
	public bool isDirty()
	{
		return mDirty;
	}
	public bool resolveEmptyDirtyState(ref FastUIFrameStats stats)
	{
		if (!mDirty || hasHierarchyState())
		{
			return false;
		}
		mDirty = false;
		mValidateManagedRoots = false;
		stats.mVisibilityRebuilt = true;
		return true;
	}
	public bool getVisible(Transform root, FastCanvas owner)
	{
		if (root == null)
		{
			return true;
		}
		if (root.TryGetComponent(out FastUIRenderElement element) && element.getCanvas() == owner && !element.getVisible())
		{
			return false;
		}
		return !mManagedHiddenRoots.Contains(root) && !mHiddenRoots.Contains(root);
	}
	public void setVisible(Transform root, bool visible, FastCanvas owner, Transform canvasRoot)
	{
		if (root == null || !isTransformInCanvas(root, owner, canvasRoot))
		{
			return;
		}
		bool changed = visible ? mHiddenRoots.Remove(root) : mHiddenRoots.Add(root);
		if (changed)
		{
			mDirty = true;
		}
	}
	public void setManagedVisible(Transform root, bool visible)
	{
		if (root == null)
		{
			return;
		}
		bool changed = visible ? mManagedHiddenRoots.Remove(root) : mManagedHiddenRoots.Add(root);
		if (changed)
		{
			mDirty = true;
		}
	}
	public void removeRoot(Transform root)
	{
		if (root == null)
		{
			return;
		}
		bool changed = mHiddenRoots.Remove(root);
		changed |= mManagedHiddenRoots.Remove(root);
		if (changed)
		{
			mDirty = true;
		}
	}
	public void setElementVisibleRoot(Transform root, bool visible)
	{
		setManagedVisible(root, visible);
	}
	public void clear()
	{
		if (mHiddenRoots.Count == 0 && mManagedHiddenRoots.Count == 0)
		{
			return;
		}
		mHiddenRoots.Clear();
		mManagedHiddenRoots.Clear();
		mDirty = true;
	}
	public void markStructureChanged()
	{
		if (hasHierarchyState())
		{
			mDirty = true;
			mValidateManagedRoots = true;
		}
	}
	public bool rebuild(FastCanvas owner, Transform canvasRoot, List<FastUIRenderElement> renderElements, FastUITransformRangeSystem transformRanges,
		FastUIRangeData_ECSList indexDirtyRanges, bool forceIndexMode, int visibilityIndexThreshold, ref FastUIFrameStats stats)
	{
		FastUIRangeUtility.copy(mHiddenRenderRanges, mPreviousHiddenRenderRanges);
		FastUIRangeUtility.copy(mIndexHiddenRenderRanges, mPreviousIndexHiddenRenderRanges);

			mHiddenRenderRanges.Clear();
			mIndexHiddenRenderRanges.Clear();
			mRemoveCache.Clear();
			int indexThreshold = Mathf.Max(visibilityIndexThreshold, 0);
			bool validateManagedRoots = mValidateManagedRoots;
			if (mManagedHiddenRoots.Count > 0)
			{
				foreach (Transform root in mManagedHiddenRoots)
				{
					if (root == null || validateManagedRoots && !isTransformInCanvas(root, owner, canvasRoot))
					{
						mRemoveCache.Add(root);
						continue;
					}
					appendRootRange(root, canvasRoot, renderElements.Count, transformRanges, forceIndexMode, indexThreshold);
				}
			}
			if (mHiddenRoots.Count > 0)
			{
				foreach (Transform root in mHiddenRoots)
				{
					if (root == null || !isTransformInCanvas(root, owner, canvasRoot))
					{
						mRemoveCache.Add(root);
						continue;
					}
					appendRootRange(root, canvasRoot, renderElements.Count, transformRanges, forceIndexMode, indexThreshold);
				}
			}
			for (int i = 0; i < mRemoveCache.Count; ++i)
			{
				Transform root = mRemoveCache[i];
				mManagedHiddenRoots.Remove(root);
				mHiddenRoots.Remove(root);
			}
			FastUIRangeUtility.merge(mHiddenRenderRanges);
			FastUIRangeUtility.merge(mIndexHiddenRenderRanges);
			appendIndexDirtyRanges(mPreviousIndexHiddenRenderRanges, indexDirtyRanges, renderElements.Count);
			appendIndexDirtyRanges(mIndexHiddenRenderRanges, indexDirtyRanges, renderElements.Count);

		mDirty = false;
		mValidateManagedRoots = false;
		stats.mVisibilityRebuilt = true;
		return !FastUIRangeUtility.isSame(mPreviousHiddenRenderRanges, mHiddenRenderRanges);
	}
	public void dispose()
	{
		mHiddenRoots.Clear();
		mManagedHiddenRoots.Clear();
		mRemoveCache.Clear();
		mHiddenRenderRanges.Dispose();
		mIndexHiddenRenderRanges.Dispose();
		mPreviousHiddenRenderRanges.Dispose();
		mPreviousIndexHiddenRenderRanges.Dispose();
	}
	private void appendRootRange(Transform root, Transform canvasRoot, int renderCount, FastUITransformRangeSystem transformRanges,
		bool forceIndexMode, int indexThreshold)
	{
		int start;
		int end;
		if (root == canvasRoot)
		{
			start = 0;
			end = renderCount;
		}
		else
		{
			if (!transformRanges.tryGetNodeIndex(root, out int nodeIndex))
			{
				return;
			}
			FastUITransformNodeRef node = transformRanges.getNode(nodeIndex);
			start = Mathf.Clamp(node.mRenderStart, 0, renderCount);
			end = Mathf.Clamp(node.mRenderStart + node.mRenderCount, start, renderCount);
		}
		int count = end - start;
		if (count <= 0)
		{
			return;
		}
		if (forceIndexMode || count <= indexThreshold)
		{
			mIndexHiddenRenderRanges.Add(new FastUIRangeData(start, end));
		}
		else
		{
			mHiddenRenderRanges.Add(new FastUIRangeData(start, end));
		}
	}
	private bool hasHierarchyState()
	{
		return mHiddenRoots.Count > 0 || mManagedHiddenRoots.Count > 0 || mHiddenRenderRanges.Count > 0 || mIndexHiddenRenderRanges.Count > 0 ||
			mPreviousHiddenRenderRanges.Count > 0 || mPreviousIndexHiddenRenderRanges.Count > 0;
	}
	private bool isTransformInCanvas(Transform root, FastCanvas owner, Transform canvasRoot)
	{
		Transform current = root;
		while (current != null)
		{
			if (current == canvasRoot)
			{
				return true;
			}
			if (current.TryGetComponent(out FastCanvas canvas) && canvas != owner)
			{
				return false;
			}
			current = current.parent;
		}
		return false;
	}
	private void appendIndexDirtyRanges(FastUIRangeData_ECSList source, FastUIRangeData_ECSList target, int renderCount)
	{
		var starts = source.getStartColumn();
		var ends = source.getEndColumn();
		for (int i = 0; i < source.Count; ++i)
		{
			FastUIRangeUtility.addRange(target, starts[i], ends[i] - starts[i], renderCount);
		}
	}
}
