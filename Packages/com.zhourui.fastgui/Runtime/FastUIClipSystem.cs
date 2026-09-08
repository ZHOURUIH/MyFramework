using EasyECS;
using System.Collections.Generic;
using UnityEngine;

// FastRectMask2D层级成员系统。
// 在Stencil正确性路径之上增加Clip-aware Batch Window：缓存Canvas空间Geometry/Clip AABB，
// 只标记完全位于有效RectMask交集之外的元素；Batch只裁掉连续外部前后缀，不修改Index Buffer。
public sealed class FastUIClipSystem
{
	private const int DEPTH_MASK = 0xFF;
	private const int CLIP_DEPTH_SHIFT = 8;
	private const int NODE_FLAG_CLIP = 1;
	private const int NODE_FLAG_MASK = 2;
	private const int CULL_FLAG_HAS_CLIP = 1 << 0;
	private const int CULL_FLAG_BOUNDS_VALID = 1 << 1;
	private const int CULL_FLAG_OUTSIDE = 1 << 2;
	private readonly List<FastRectMask2D> mClipRects = new();
	private readonly List<FastRectMask2D> mHierarchyResumeCache = new();
	private readonly Int_ECSList mNodePackedDepths = new(256);
	private readonly Int_ECSList mNodeFlags = new(256);
	private readonly FastUISpatialBoundsData_ECSList mNodeClipBounds = new(256);
	private readonly Bool_ECSList mNodeHasClipBounds = new(256);
	private readonly FastUISpatialBoundsData_ECSList mElementBounds = new(256);
	private readonly FastUISpatialBoundsData_ECSList mElementClipBounds = new(256);
	private readonly Int_ECSList mElementCullFlags = new(256);
	private readonly FastUITransformPositionBatchData_ECSList mPendingTranslations = new(64);
	private readonly FastUIRangeData_ECSList mPendingGeometryRanges = new(64);
	private FastRectMask2D[] mNodeClipRefs = System.Array.Empty<FastRectMask2D>();
	private bool mStructureDirty = true;
	private bool mClipBoundsDirty = true;
	private bool mCullAllDirty = true;
	private const int MAX_DEFERRED_TRANSLATION_RANGES = 64;
	private const int MAX_DEFERRED_GEOMETRY_RANGES = 64;
	private int mTranslatedElementCountThisFrame;
	private int mLastCullFullScanElementCount;
	private int mLastCullIncrementalElementCount;
	private int mLastCullCachedClipBoundsElementCount;
	private int mLastTranslationElementCount;
	private int mLastTranslationRangeCount;
	private bool mLastCullDeferredLargeTranslation;
	private int mClippedElementCount;
	private int mLastHierarchyNodeCount;
	private int mClipCullOutsideCount;
	public int getClipRootCount()
	{
		return mClipRects.Count;
	}
	public int getClippedElementCount()
	{
		return mClippedElementCount;
	}
	public int getLastHierarchyNodeCount()
	{
		return mLastHierarchyNodeCount;
	}
	public int getClipCullOutsideCount()
	{
		return mClipCullOutsideCount;
	}
	public bool isStructureDirty()
	{
		return mStructureDirty;
	}
	public int getLastCullFullScanElementCount()
	{
		return mLastCullFullScanElementCount;
	}
	public int getLastCullIncrementalElementCount()
	{
		return mLastCullIncrementalElementCount;
	}
	public int getLastCullCachedClipBoundsElementCount()
	{
		return mLastCullCachedClipBoundsElementCount;
	}
	public int getLastTranslationElementCount()
	{
		return mLastTranslationElementCount;
	}
	public int getLastTranslationRangeCount()
	{
		return mLastTranslationRangeCount;
	}
	public bool getLastCullDeferredLargeTranslation()
	{
		return mLastCullDeferredLargeTranslation;
	}
	public bool shouldDeferCullForLargeTranslation()
	{
		if (mTranslatedElementCountThisFrame <= 0 || mClippedElementCount <= 0)
		{
			return false;
		}
		int threshold = Mathf.Max(256, mClippedElementCount >> 2);
		return mTranslatedElementCountThisFrame >= threshold;
	}
	public void deferCullAfterLargeTranslation(bool preservePendingWork = false)
	{
		mLastCullFullScanElementCount = 0;
		mLastCullIncrementalElementCount = 0;
		mLastCullCachedClipBoundsElementCount = 0;
		mLastTranslationElementCount = mTranslatedElementCountThisFrame;
		mLastTranslationRangeCount = mPendingTranslations.Count;
		mLastCullDeferredLargeTranslation = true;
		mTranslatedElementCountThisFrame = 0;
		if (preservePendingWork && mPendingTranslations.Count <= MAX_DEFERRED_TRANSLATION_RANGES && mPendingGeometryRanges.Count <= MAX_DEFERRED_GEOMETRY_RANGES)
		{
			// ：大型平移帧仍然关闭Clip Submit裁剪，避免本帧扫描大量元素；
			// 但不能丢掉已经收集的Translation/Geometry Range，否则恢复帧只能mCullAllDirty全量重算。
			// 保留Range后，连续移动期间仍然零Cull；停止移动时只按累计Range修正缓存Bounds并恢复裁剪。
			// 长时间移动且Range持续碎片化时设置上限，超过后自动回退旧FullScan恢复语义，避免Deferred列表无界增长。
			return;
		}
		// 兼容及更早行为，保留为诊断/快速回退路径。
		mCullAllDirty = true;
		mPendingTranslations.Clear();
		mPendingGeometryRanges.Clear();
	}
	public void register(FastRectMask2D clip)
	{
		if (clip == null || mClipRects.Contains(clip))
		{
			return;
		}
		mClipRects.Add(clip);
		mStructureDirty = true;
		mClipBoundsDirty = true;
		mCullAllDirty = true;
	}
	public void unregister(FastRectMask2D clip)
	{
		if (clip != null && mClipRects.Remove(clip))
		{
			mStructureDirty = true;
			mClipBoundsDirty = true;
			mCullAllDirty = true;
		}
	}
	public void markStructureDirty()
	{
		if (mClipRects.Count == 0 && mClippedElementCount == 0)
		{
			mStructureDirty = false;
			return;
		}
		mStructureDirty = true;
		mCullAllDirty = true;
	}
	public void markClipBoundsDirty()
	{
		// ：Clip Rect/Padding变化本身不会让Element Geometry失效。
		// 保留已有Element Bounds缓存，刷新时只重算Clip Bounds并用缓存Bounds重新判定Outside；
		// 若同帧Element Geometry/Translation也变化，继续由Pending Range覆盖对应元素。
		mClipBoundsDirty = true;
	}
	public void markCullAllDirty()
	{
		mCullAllDirty = true;
	}
	public void markGeometryDirty(int renderIndex)
	{
		if (renderIndex < 0 || mCullAllDirty)
		{
			return;
		}
		FastUIRangeUtility.addRange(mPendingGeometryRanges, renderIndex, 1, mElementCullFlags.Count);
	}
	public void markGeometryRangeDirty(int renderStart, int renderCount)
	{
		if (renderCount <= 0 || mCullAllDirty)
		{
			return;
		}
		FastUIRangeUtility.addRange(mPendingGeometryRanges, renderStart, renderCount, mElementCullFlags.Count);
	}
	public void markTranslatedRange(int renderStart, int renderCount, Vector3 canvasDelta)
	{
		if (renderCount <= 0 || canvasDelta == Vector3.zero)
		{
			return;
		}
		mTranslatedElementCountThisFrame += renderCount;
		if (mCullAllDirty || mElementCullFlags.Count == 0)
		{
			return;
		}
		int start = Mathf.Clamp(renderStart, 0, mElementCullFlags.Count);
		int end = Mathf.Clamp(renderStart + renderCount, start, mElementCullFlags.Count);
		if (end <= start)
		{
			return;
		}
		int count = mPendingTranslations.Count;
		if (count > 0)
		{
			int last = count - 1;
			var startColumn = mPendingTranslations.getRenderStartColumn();
			var countColumn = mPendingTranslations.getRenderCountColumn();
			var deltaColumn = mPendingTranslations.getCanvasDeltaColumn();
			int rangeCount = end - start;
			// ：连续Deferred帧常常反复移动同一大片RenderRange。只累计最终Delta，避免恢复时重复扫描同一Range。
			if (startColumn[last] == start && countColumn[last] == rangeCount)
			{
				Vector3 mergedDelta = deltaColumn[last] + canvasDelta;
				if (mergedDelta == Vector3.zero)
				{
					mPendingTranslations.RemoveAt(last);
				}
				else
				{
					deltaColumn[last] = mergedDelta;
				}
				return;
			}
			if (startColumn[last] + countColumn[last] == start && deltaColumn[last] == canvasDelta)
			{
				countColumn[last] += rangeCount;
				return;
			}
		}
		mPendingTranslations.Add(new FastUITransformPositionBatchData(start, end - start, canvasDelta));
	}
	// 直接复用TransformRangeSystem已经构建好的DFS Node/ParentIndex。
	// FastRectMask2D由注册表定位节点；FastMask必须与FastUIRenderElement同节点，因此直接从Graphic的AttachedMask标记。
	public int refreshStructure(List<FastUIRenderElement> renderElements, FastCanvas owner,
		FastUITransformRangeSystem transformRanges, bool fullDrawStructurePending)
	{
		if (!mStructureDirty || renderElements == null || owner == null || transformRanges == null)
		{
			return 0;
		}
		removeInvalidClips(owner);
		int nodeCount = transformRanges.getCount();
		mLastHierarchyNodeCount = nodeCount;
		mNodePackedDepths.Clear();
		mNodeFlags.Clear();
		mNodePackedDepths.EnsureCount(nodeCount);
		mNodeFlags.EnsureCount(nodeCount);
		mNodeClipBounds.Clear();
		mNodeHasClipBounds.Clear();
		mNodeClipBounds.EnsureCount(nodeCount);
		mNodeHasClipBounds.EnsureCount(nodeCount);
		if (mNodeClipRefs.Length < nodeCount)
		{
			mNodeClipRefs = new FastRectMask2D[Mathf.NextPowerOfTwo(Mathf.Max(nodeCount, 1))];
		}
		else
		{
			System.Array.Clear(mNodeClipRefs, 0, nodeCount);
		}
		var nodeFlags = mNodeFlags.getValueColumn();
		for (int i = 0; i < mClipRects.Count; ++i)
		{
			FastRectMask2D clip = mClipRects[i];
			if (clip != null && clip.isClipBasicActiveForCanvas(owner) && transformRanges.tryGetNodeIndex(clip.getRectTransform(), out int nodeIndex))
			{
				nodeFlags[nodeIndex] |= NODE_FLAG_CLIP;
				mNodeClipRefs[nodeIndex] = clip;
			}
		}
		Int_ECSList elementNodeIndicesList = transformRanges.getElementNodeIndices();
		var elementNodeIndices = elementNodeIndicesList.getValueColumn();
		int renderElementCount = renderElements.Count;
		for (int i = 0; i < renderElementCount; ++i)
		{
			FastUIRenderElement element = renderElements[i];
			if (element == null || element.getCanvas() != owner || (uint)i >= (uint)elementNodeIndicesList.Count)
			{
				continue;
			}
			FastMask mask = element.getAttachedMask();
			int nodeIndex = elementNodeIndices[i];
			if (nodeIndex >= 0 && mask != null && mask.isMaskBasicActiveForCanvas(owner))
			{
				nodeFlags[nodeIndex] |= NODE_FLAG_MASK;
			}
		}
		FastUITransformNode_ECSList nodes = transformRanges.getNodes();
		var parentIndices = nodes.getParentIndexColumn();
		var packedDepths = mNodePackedDepths.getValueColumn();
		for (int nodeIndex = 0; nodeIndex < nodeCount; ++nodeIndex)
		{
			int parentIndex = parentIndices[nodeIndex];
			int parentPackedDepth = parentIndex >= 0 ? packedDepths[parentIndex] : 0;
			int maskDepth = getMaskDepth(parentPackedDepth);
			int clipDepth = getClipDepth(parentPackedDepth);
			int flags = nodeFlags[nodeIndex];
			if ((flags & NODE_FLAG_CLIP) != 0 && maskDepth < FastMaskUtility.MAX_MASK_DEPTH)
			{
				++maskDepth;
				++clipDepth;
			}
			if ((flags & NODE_FLAG_MASK) != 0 && maskDepth < FastMaskUtility.MAX_MASK_DEPTH)
			{
				++maskDepth;
			}
			packedDepths[nodeIndex] = packDepth(maskDepth, clipDepth);
		}
		if (!rebuildNodeClipBounds(owner, transformRanges))
		{
			mStructureDirty = true;
			mCullAllDirty = true;
			return 0;
		}
		mElementBounds.Clear();
		mElementClipBounds.Clear();
		mElementCullFlags.Clear();
		mElementBounds.EnsureCount(renderElementCount);
		mElementClipBounds.EnsureCount(renderElementCount);
		mElementCullFlags.EnsureCount(renderElementCount, 0);
		int scanCount = 0;
		mClippedElementCount = 0;
		mClipCullOutsideCount = 0;
		var elementFlags = mElementCullFlags.getValueColumn();
		var nodeHasClip = mNodeHasClipBounds.getValueColumn();
		var nodeClipMinX = mNodeClipBounds.getMinXColumn();
		var nodeClipMinY = mNodeClipBounds.getMinYColumn();
		var nodeClipMaxX = mNodeClipBounds.getMaxXColumn();
		var nodeClipMaxY = mNodeClipBounds.getMaxYColumn();
		var elementClipMinX = mElementClipBounds.getMinXColumn();
		var elementClipMinY = mElementClipBounds.getMinYColumn();
		var elementClipMaxX = mElementClipBounds.getMaxXColumn();
		var elementClipMaxY = mElementClipBounds.getMaxYColumn();
		for (int i = 0; i < renderElementCount; ++i)
		{
			FastUIRenderElement element = renderElements[i];
			if (element == null || element.getCanvas() != owner || (uint)i >= (uint)elementNodeIndicesList.Count)
			{
				continue;
			}
			int nodeIndex = elementNodeIndices[i];
			if ((uint)nodeIndex >= (uint)nodeCount)
			{
				continue;
			}
			++scanCount;
			int parentIndex = parentIndices[nodeIndex];
			int selfDepth = packedDepths[nodeIndex];
			int parentDepth = parentIndex >= 0 ? packedDepths[parentIndex] : 0;
			int selfClipDepth = getClipDepth(selfDepth);
			int parentClipDepth = getClipDepth(parentDepth);
			bool isClipStencil = element is FastClipStencilGraphic;
			bool hasClip = !isClipStencil && selfClipDepth > 0;
			element.setHasActiveClip(hasClip);
			if (hasClip)
			{
				++mClippedElementCount;
			}
			if (!isClipStencil && element is not FastMaskPopGraphic)
			{
				refreshElementMaskState(element, selfDepth, parentDepth, selfClipDepth > parentClipDepth, fullDrawStructurePending);
			}
			int cullFlags = element.isClipCullOutside() ? CULL_FLAG_OUTSIDE : 0;
			if (hasClip && nodeHasClip[nodeIndex])
			{
				cullFlags |= CULL_FLAG_HAS_CLIP;
				elementClipMinX[i] = nodeClipMinX[nodeIndex];
				elementClipMinY[i] = nodeClipMinY[nodeIndex];
				elementClipMaxX[i] = nodeClipMaxX[nodeIndex];
				elementClipMaxY[i] = nodeClipMaxY[nodeIndex];
			}
			elementFlags[i] = cullFlags;
			if ((cullFlags & CULL_FLAG_OUTSIDE) != 0)
			{
				++mClipCullOutsideCount;
			}
		}
		mPendingTranslations.Clear();
		mPendingGeometryRanges.Clear();
		mTranslatedElementCountThisFrame = 0;
		mClipBoundsDirty = false;
		mCullAllDirty = true;
		mStructureDirty = false;
		return scanCount;
	}
	public bool refreshCull(List<FastUIRenderElement> renderElements, FastCanvas owner, FastUITransformRangeSystem transformRanges,
		bool fullDrawStructurePending, bool cachedClipBoundsCullEnabled = true)
	{
		mLastCullFullScanElementCount = 0;
		mLastCullIncrementalElementCount = 0;
		mLastCullCachedClipBoundsElementCount = 0;
		mLastTranslationElementCount = mTranslatedElementCountThisFrame;
		mLastTranslationRangeCount = mPendingTranslations.Count;
		mLastCullDeferredLargeTranslation = false;
		if (renderElements == null || owner == null || transformRanges == null)
		{
			return false;
		}
		if (mStructureDirty)
		{
			return false;
		}
		bool clipBoundsChanged = false;
		if (mClipBoundsDirty)
		{
			if (!rebuildNodeClipBounds(owner, transformRanges))
			{
				// TransformRange与Clip结构计数短暂不同步时不能读取Unsafe Column。
				// 标回StructureDirty，下一帧走完整Structure Refresh。
				mStructureDirty = true;
				mCullAllDirty = true;
				mClipBoundsDirty = true;
				return false;
			}
			if (cachedClipBoundsCullEnabled && !mCullAllDirty &&
				canUseCachedClipBoundsCull(renderElements, transformRanges) &&
				refreshElementClipBoundsFromStableStructure(transformRanges))
			{
				clipBoundsChanged = true;
			}
			else
			{
				refreshElementClipBounds(renderElements, owner, transformRanges);
				mCullAllDirty = true;
			}
			mClipBoundsDirty = false;
		}
		bool changed = false;
		if (mCullAllDirty)
		{
			mPendingTranslations.Clear();
			mPendingGeometryRanges.Clear();
			int count = Mathf.Min(renderElements.Count, mElementCullFlags.Count);
			for (int i = 0; i < count; ++i)
			{
				changed |= refreshElementBoundsAndCull(i, renderElements[i], owner, fullDrawStructurePending);
			}
			mCullAllDirty = false;
			mLastCullFullScanElementCount = count;
			mTranslatedElementCountThisFrame = 0;
			return changed;
		}
		changed |= applyPendingTranslations(renderElements, owner, fullDrawStructurePending);
		if (mPendingGeometryRanges.Count > 1)
		{
			FastUIRangeUtility.merge(mPendingGeometryRanges);
		}
		var rangeStarts = mPendingGeometryRanges.getStartColumn();
		var rangeEnds = mPendingGeometryRanges.getEndColumn();
		for (int rangeIndex = 0; rangeIndex < mPendingGeometryRanges.Count; ++rangeIndex)
		{
			int start = Mathf.Clamp(rangeStarts[rangeIndex], 0, renderElements.Count);
			int end = Mathf.Clamp(rangeEnds[rangeIndex], start, renderElements.Count);
			mLastCullIncrementalElementCount += end - start;
			for (int i = start; i < end; ++i)
			{
				changed |= refreshElementBoundsAndCull(i, renderElements[i], owner, fullDrawStructurePending);
			}
		}
		if (clipBoundsChanged)
		{
			// ：Geometry Range里的元素刚刚已经按“新Geometry + 新ClipBounds”完成Cull，
			// CachedBounds只处理其余元素，避免Size变化时同一批后代被扫描两次。
			changed |= refreshCullFromCachedElementBounds(renderElements, owner, fullDrawStructurePending, mPendingGeometryRanges);
		}
		mPendingGeometryRanges.Clear();
		mTranslatedElementCountThisFrame = 0;
		return changed;
	}
	// 整个Canvas恢复时通常Clip结构没有任何变化。
	// 只检查Clip Root本身；只有Active/Canvas/Stencil Depth真的变化才把Structure标脏。
	public int syncHierarchyResume(FastCanvas owner, out int dirtyCount)
	{
		dirtyCount = 0;
		if (owner == null || mClipRects.Count == 0)
		{
			return 0;
		}
		mHierarchyResumeCache.Clear();
		for (int i = 0; i < mClipRects.Count; ++i)
		{
			FastRectMask2D clip = mClipRects[i];
			if (clip != null)
			{
				mHierarchyResumeCache.Add(clip);
			}
		}
		int count = mHierarchyResumeCache.Count;
		for (int i = 0; i < count; ++i)
		{
			FastRectMask2D clip = mHierarchyResumeCache[i];
			if (clip != null && clip.syncAfterCanvasHierarchyResume(owner))
			{
				++dirtyCount;
			}
		}
		if (dirtyCount > 0)
		{
			mStructureDirty = true;
			mCullAllDirty = true;
		}
		return count;
	}
	public bool hasActiveClipInHierarchy(Transform target, FastCanvas owner)
	{
		Transform current = target;
		while (current != null)
		{
			if (current.TryGetComponent(out FastRectMask2D clip) && clip.isClipActiveForCanvas(owner))
			{
				return true;
			}
			if (current == owner.transform)
			{
				break;
			}
			current = current.parent;
		}
		return false;
	}
	public void clear()
	{
		mClipRects.Clear();
		mHierarchyResumeCache.Clear();
		mNodePackedDepths.Clear();
		mNodeFlags.Clear();
		mNodeClipBounds.Clear();
		mNodeHasClipBounds.Clear();
		mElementBounds.Clear();
		mElementClipBounds.Clear();
		mElementCullFlags.Clear();
		mPendingTranslations.Clear();
		mPendingGeometryRanges.Clear();
		mClippedElementCount = 0;
		mClipCullOutsideCount = 0;
		mLastHierarchyNodeCount = 0;
		mStructureDirty = true;
		mClipBoundsDirty = true;
		mCullAllDirty = true;
		mTranslatedElementCountThisFrame = 0;
		mLastCullFullScanElementCount = 0;
		mLastCullIncrementalElementCount = 0;
		mLastCullCachedClipBoundsElementCount = 0;
		mLastTranslationElementCount = 0;
		mLastTranslationRangeCount = 0;
		mLastCullDeferredLargeTranslation = false;
	}
	public void dispose()
	{
		mClipRects.Clear();
		mHierarchyResumeCache.Clear();
		mNodePackedDepths.Dispose();
		mNodeFlags.Dispose();
		mNodeClipBounds.Dispose();
		mNodeHasClipBounds.Dispose();
		mElementBounds.Dispose();
		mElementClipBounds.Dispose();
		mElementCullFlags.Dispose();
		mPendingTranslations.Dispose();
		mPendingGeometryRanges.Dispose();
		mNodeClipRefs = System.Array.Empty<FastRectMask2D>();
		mClippedElementCount = 0;
		mClipCullOutsideCount = 0;
		mLastHierarchyNodeCount = 0;
		mStructureDirty = true;
		mClipBoundsDirty = true;
		mCullAllDirty = true;
		mTranslatedElementCountThisFrame = 0;
		mLastCullFullScanElementCount = 0;
		mLastCullIncrementalElementCount = 0;
		mLastCullCachedClipBoundsElementCount = 0;
		mLastTranslationElementCount = 0;
		mLastTranslationRangeCount = 0;
		mLastCullDeferredLargeTranslation = false;
	}
	private bool applyPendingTranslations(List<FastUIRenderElement> renderElements, FastCanvas owner, bool fullDrawStructurePending)
	{
		bool changed = false;
		var translationStarts = mPendingTranslations.getRenderStartColumn();
		var translationCounts = mPendingTranslations.getRenderCountColumn();
		var translationDeltas = mPendingTranslations.getCanvasDeltaColumn();
		var flags = mElementCullFlags.getValueColumn();
		var minX = mElementBounds.getMinXColumn();
		var minY = mElementBounds.getMinYColumn();
		var maxX = mElementBounds.getMaxXColumn();
		var maxY = mElementBounds.getMaxYColumn();
		var clipMinX = mElementClipBounds.getMinXColumn();
		var clipMinY = mElementClipBounds.getMinYColumn();
		var clipMaxX = mElementClipBounds.getMaxXColumn();
		var clipMaxY = mElementClipBounds.getMaxYColumn();
		for (int rangeIndex = 0; rangeIndex < mPendingTranslations.Count; ++rangeIndex)
		{
			int start = Mathf.Clamp(translationStarts[rangeIndex], 0, mElementCullFlags.Count);
			int end = Mathf.Clamp(start + translationCounts[rangeIndex], start, mElementCullFlags.Count);
			mLastCullIncrementalElementCount += end - start;
			Vector3 delta = translationDeltas[rangeIndex];
			for (int i = start; i < end; ++i)
			{
				int state = flags[i];
				if ((state & CULL_FLAG_HAS_CLIP) == 0)
				{
					continue;
				}
				if ((state & CULL_FLAG_BOUNDS_VALID) == 0)
				{
					FastUIRangeUtility.addRange(mPendingGeometryRanges, i, 1, mElementCullFlags.Count);
					continue;
				}
				minX[i] += delta.x;
				maxX[i] += delta.x;
				minY[i] += delta.y;
				maxY[i] += delta.y;
				bool outside = isOutside(minX[i], minY[i], maxX[i], maxY[i], clipMinX[i], clipMinY[i], clipMaxX[i], clipMaxY[i]);
				bool oldOutside = (state & CULL_FLAG_OUTSIDE) != 0;
				if (outside == oldOutside)
				{
					continue;
				}
				flags[i] = outside ? state | CULL_FLAG_OUTSIDE : state & ~CULL_FLAG_OUTSIDE;
				mClipCullOutsideCount += outside ? 1 : -1;
				FastUIRenderElement element = i < renderElements.Count ? renderElements[i] : null;
				if (element != null && element.getCanvas() == owner)
				{
					element.setClipCullOutside(outside);
					if (!fullDrawStructurePending)
					{
						owner.patchClipCullState(i, outside);
					}
					changed = true;
				}
			}
		}
		mPendingTranslations.Clear();
		return changed;
	}
	private bool refreshElementBoundsAndCull(int renderIndex, FastUIRenderElement renderElement, FastCanvas owner, bool fullDrawStructurePending)
	{
		if ((uint)renderIndex >= (uint)mElementCullFlags.Count)
		{
			return false;
		}
		FastUIRenderElement element = renderElement;
		if (element == null || element.getCanvas() != owner)
		{
			return false;
		}
		var flagsColumn = mElementCullFlags.getValueColumn();
		int state = flagsColumn[renderIndex];
		bool hasClip = (state & CULL_FLAG_HAS_CLIP) != 0;
		bool outside = false;
		if (hasClip && owner.tryGetClipCullGeometryBounds(element, out FastUISpatialBoundsData bounds))
		{
			var minX = mElementBounds.getMinXColumn();
			var minY = mElementBounds.getMinYColumn();
			var maxX = mElementBounds.getMaxXColumn();
			var maxY = mElementBounds.getMaxYColumn();
			minX[renderIndex] = bounds.mMinX;
			minY[renderIndex] = bounds.mMinY;
			maxX[renderIndex] = bounds.mMaxX;
			maxY[renderIndex] = bounds.mMaxY;
			state |= CULL_FLAG_BOUNDS_VALID;
			var clipMinX = mElementClipBounds.getMinXColumn();
			var clipMinY = mElementClipBounds.getMinYColumn();
			var clipMaxX = mElementClipBounds.getMaxXColumn();
			var clipMaxY = mElementClipBounds.getMaxYColumn();
			outside = isOutside(bounds.mMinX, bounds.mMinY, bounds.mMaxX, bounds.mMaxY,
				clipMinX[renderIndex], clipMinY[renderIndex], clipMaxX[renderIndex], clipMaxY[renderIndex]);
		}
		else
		{
			state &= ~CULL_FLAG_BOUNDS_VALID;
		}
		bool oldOutside = (state & CULL_FLAG_OUTSIDE) != 0;
		if (outside)
		{
			state |= CULL_FLAG_OUTSIDE;
		}
		else
		{
			state &= ~CULL_FLAG_OUTSIDE;
		}
		flagsColumn[renderIndex] = state;
		if (oldOutside == outside && element.isClipCullOutside() == outside)
		{
			return false;
		}
		if (oldOutside != outside)
		{
			mClipCullOutsideCount += outside ? 1 : -1;
		}
		element.setClipCullOutside(outside);
		if (!fullDrawStructurePending)
		{
			owner.patchClipCullState(renderIndex, outside);
		}
		return true;
	}
	// ：EasyECS Direct Column在Unsafe后端必须先验证所有并行Storage长度。
	// 任一结构计数不一致都直接回退旧Full路径，避免缓存Cull在生命周期边界读取越界Column。
	private bool canUseCachedClipBoundsCull(List<FastUIRenderElement> renderElements, FastUITransformRangeSystem transformRanges)
	{
		if (renderElements == null || transformRanges == null)
		{
			return false;
		}
		int count = renderElements.Count;
		if (mElementCullFlags.Count != count || mElementBounds.Count != count || mElementClipBounds.Count != count)
		{
			return false;
		}
		Int_ECSList elementNodeIndices = transformRanges.getElementNodeIndices();
		if (elementNodeIndices.Count < count)
		{
			return false;
		}
		int nodeCount = transformRanges.getCount();
		if (nodeCount < 0 || mNodeHasClipBounds.Count < nodeCount || mNodeClipBounds.Count < nodeCount ||
			mNodeFlags.Count < nodeCount || mNodePackedDepths.Count < nodeCount)
		{
			return false;
		}
		return true;
	}
	// ：Clip结构没有变化时，HAS_CLIP和Element->Node映射都稳定。
	// 这里只更新每个被裁剪Element对应的新Clip AABB，保留已经缓存好的Element Geometry Bounds有效位。
	private bool refreshElementClipBoundsFromStableStructure(FastUITransformRangeSystem transformRanges)
	{
		if (transformRanges == null)
		{
			return false;
		}
		Int_ECSList elementNodeIndicesList = transformRanges.getElementNodeIndices();
		int count = mElementCullFlags.Count;
		if (elementNodeIndicesList.Count < count || mElementBounds.Count < count || mElementClipBounds.Count < count)
		{
			return false;
		}
		var elementNodeIndices = elementNodeIndicesList.getValueColumn();
		var nodeHasClip = mNodeHasClipBounds.getValueColumn();
		var nodeMinX = mNodeClipBounds.getMinXColumn();
		var nodeMinY = mNodeClipBounds.getMinYColumn();
		var nodeMaxX = mNodeClipBounds.getMaxXColumn();
		var nodeMaxY = mNodeClipBounds.getMaxYColumn();
		var elementMinX = mElementClipBounds.getMinXColumn();
		var elementMinY = mElementClipBounds.getMinYColumn();
		var elementMaxX = mElementClipBounds.getMaxXColumn();
		var elementMaxY = mElementClipBounds.getMaxYColumn();
		var flags = mElementCullFlags.getValueColumn();
		for (int i = 0; i < count; ++i)
		{
			if ((flags[i] & CULL_FLAG_HAS_CLIP) == 0)
			{
				continue;
			}
			int nodeIndex = elementNodeIndices[i];
			if ((uint)nodeIndex >= (uint)mNodeHasClipBounds.Count || !nodeHasClip[nodeIndex])
			{
				return false;
			}
			elementMinX[i] = nodeMinX[nodeIndex];
			elementMinY[i] = nodeMinY[nodeIndex];
			elementMaxX[i] = nodeMaxX[nodeIndex];
			elementMaxY[i] = nodeMaxY[nodeIndex];
		}
		return true;
	}
	// Clip Bounds变化不等于Element Geometry变化。直接复用mElementBounds缓存做Outside重判，
	// 避免旧路径再次从Mesh读取每个Element的Geometry Bounds。
	private bool refreshCullFromCachedElementBounds(List<FastUIRenderElement> renderElements, FastCanvas owner,
		bool fullDrawStructurePending, FastUIRangeData_ECSList refreshedGeometryRanges)
	{
		bool changed = false;
		int count = renderElements.Count;
		if (mElementCullFlags.Count != count || mElementBounds.Count != count || mElementClipBounds.Count != count)
		{
			// 这个检查理论上已由canUseCachedClipBoundsCull保证；保留二次防线，Unsafe后端宁可回退下一帧Full也不越界。
			mCullAllDirty = true;
			return false;
		}
		var flags = mElementCullFlags.getValueColumn();
		var minX = mElementBounds.getMinXColumn();
		var minY = mElementBounds.getMinYColumn();
		var maxX = mElementBounds.getMaxXColumn();
		var maxY = mElementBounds.getMaxYColumn();
		var clipMinX = mElementClipBounds.getMinXColumn();
		var clipMinY = mElementClipBounds.getMinYColumn();
		var clipMaxX = mElementClipBounds.getMaxXColumn();
		var clipMaxY = mElementClipBounds.getMaxYColumn();
		int refreshedRangeIndex = 0;
		int refreshedRangeCount = refreshedGeometryRanges.Count;
		var refreshedStarts = refreshedGeometryRanges.getStartColumn();
		var refreshedEnds = refreshedGeometryRanges.getEndColumn();
		for (int i = 0; i < count; ++i)
		{
			while (refreshedRangeIndex < refreshedRangeCount && refreshedEnds[refreshedRangeIndex] <= i)
			{
				++refreshedRangeIndex;
			}
			if (refreshedRangeIndex < refreshedRangeCount &&
				refreshedStarts[refreshedRangeIndex] <= i && i < refreshedEnds[refreshedRangeIndex])
			{
				continue;
			}
			int state = flags[i];
			if ((state & CULL_FLAG_HAS_CLIP) == 0)
			{
				continue;
			}
			if ((state & CULL_FLAG_BOUNDS_VALID) == 0)
			{
				// 缓存缺失只重算这个元素，不把Unknown错误当作Inside。
				++mLastCullIncrementalElementCount;
				changed |= refreshElementBoundsAndCull(i, renderElements[i], owner, fullDrawStructurePending);
				continue;
			}
			++mLastCullCachedClipBoundsElementCount;
			bool outside = isOutside(minX[i], minY[i], maxX[i], maxY[i], clipMinX[i], clipMinY[i], clipMaxX[i], clipMaxY[i]);
			bool oldOutside = (state & CULL_FLAG_OUTSIDE) != 0;
			if (outside == oldOutside)
			{
				// ：绝大多数Padding变化只改变Clip AABB但不会跨过元素边界。
				// Flags未变化时不再读取Managed RenderElement/getCanvas/isClipCullOutside。
				continue;
			}
			flags[i] = outside ? state | CULL_FLAG_OUTSIDE : state & ~CULL_FLAG_OUTSIDE;
			mClipCullOutsideCount += outside ? 1 : -1;
			FastUIRenderElement element = renderElements[i];
			if (element == null || element.getCanvas() != owner)
			{
				continue;
			}
			element.setClipCullOutside(outside);
			if (!fullDrawStructurePending)
			{
				owner.patchClipCullState(i, outside);
			}
			changed = true;
		}
		return changed;
	}
	private void refreshElementClipBounds(List<FastUIRenderElement> renderElements, FastCanvas owner, FastUITransformRangeSystem transformRanges)
	{
		Int_ECSList elementNodeIndicesList = transformRanges.getElementNodeIndices();
		var elementNodeIndices = elementNodeIndicesList.getValueColumn();
		var nodeHasClip = mNodeHasClipBounds.getValueColumn();
		var nodeMinX = mNodeClipBounds.getMinXColumn();
		var nodeMinY = mNodeClipBounds.getMinYColumn();
		var nodeMaxX = mNodeClipBounds.getMaxXColumn();
		var nodeMaxY = mNodeClipBounds.getMaxYColumn();
		var elementMinX = mElementClipBounds.getMinXColumn();
		var elementMinY = mElementClipBounds.getMinYColumn();
		var elementMaxX = mElementClipBounds.getMaxXColumn();
		var elementMaxY = mElementClipBounds.getMaxYColumn();
		var flags = mElementCullFlags.getValueColumn();
		int count = Mathf.Min(renderElements.Count, mElementCullFlags.Count);
		for (int i = 0; i < count; ++i)
		{
			FastUIRenderElement element = renderElements[i];
			int nodeIndex = (uint)i < (uint)elementNodeIndicesList.Count ? elementNodeIndices[i] : -1;
			bool hasClip = element != null && element.getCanvas() == owner && element.hasActiveClip() &&
				(uint)nodeIndex < (uint)mNodeHasClipBounds.Count && nodeHasClip[nodeIndex];
			int state = flags[i] & ~(CULL_FLAG_HAS_CLIP | CULL_FLAG_BOUNDS_VALID);
			if (hasClip)
			{
				state |= CULL_FLAG_HAS_CLIP;
				elementMinX[i] = nodeMinX[nodeIndex];
				elementMinY[i] = nodeMinY[nodeIndex];
				elementMaxX[i] = nodeMaxX[nodeIndex];
				elementMaxY[i] = nodeMaxY[nodeIndex];
			}
			flags[i] = state;
		}
	}
	private bool rebuildNodeClipBounds(FastCanvas owner, FastUITransformRangeSystem transformRanges)
	{
		int nodeCount = transformRanges != null ? transformRanges.getCount() : 0;
		if (nodeCount <= 0)
		{
			return true;
		}
		FastUITransformNode_ECSList nodes = transformRanges.getNodes();
		if (nodes.Count < nodeCount || mNodeFlags.Count < nodeCount || mNodePackedDepths.Count < nodeCount ||
			mNodeClipRefs == null || mNodeClipRefs.Length < nodeCount)
		{
			return false;
		}
		mNodeClipBounds.EnsureCount(nodeCount);
		mNodeHasClipBounds.EnsureCount(nodeCount);
		var parentIndices = nodes.getParentIndexColumn();
		var nodeFlags = mNodeFlags.getValueColumn();
		var packedDepths = mNodePackedDepths.getValueColumn();
		var hasBounds = mNodeHasClipBounds.getValueColumn();
		var minX = mNodeClipBounds.getMinXColumn();
		var minY = mNodeClipBounds.getMinYColumn();
		var maxX = mNodeClipBounds.getMaxXColumn();
		var maxY = mNodeClipBounds.getMaxYColumn();
		for (int nodeIndex = 0; nodeIndex < nodeCount; ++nodeIndex)
		{
			int parentIndex = parentIndices[nodeIndex];
			if (parentIndex >= nodeIndex || parentIndex < -1)
			{
				return false;
			}
			bool parentHas = parentIndex >= 0 && hasBounds[parentIndex];
			float currentMinX = parentHas ? minX[parentIndex] : 0.0f;
			float currentMinY = parentHas ? minY[parentIndex] : 0.0f;
			float currentMaxX = parentHas ? maxX[parentIndex] : 0.0f;
			float currentMaxY = parentHas ? maxY[parentIndex] : 0.0f;
			bool currentHas = parentHas;
			int parentPacked = parentIndex >= 0 ? packedDepths[parentIndex] : 0;
			bool selfClipActive = (nodeFlags[nodeIndex] & NODE_FLAG_CLIP) != 0 && getMaskDepth(parentPacked) < FastMaskUtility.MAX_MASK_DEPTH;
			FastRectMask2D clip = mNodeClipRefs[nodeIndex];
			if (selfClipActive && clip != null && tryGetClipCanvasBounds(clip, owner, out FastUISpatialBoundsData selfBounds))
			{
				if (currentHas)
				{
					currentMinX = Mathf.Max(currentMinX, selfBounds.mMinX);
					currentMinY = Mathf.Max(currentMinY, selfBounds.mMinY);
					currentMaxX = Mathf.Min(currentMaxX, selfBounds.mMaxX);
					currentMaxY = Mathf.Min(currentMaxY, selfBounds.mMaxY);
				}
				else
				{
					currentMinX = selfBounds.mMinX;
					currentMinY = selfBounds.mMinY;
					currentMaxX = selfBounds.mMaxX;
					currentMaxY = selfBounds.mMaxY;
					currentHas = true;
				}
			}
			hasBounds[nodeIndex] = currentHas;
			if (currentHas)
			{
				minX[nodeIndex] = currentMinX;
				minY[nodeIndex] = currentMinY;
				maxX[nodeIndex] = currentMaxX;
				maxY[nodeIndex] = currentMaxY;
			}
		}
		return true;
	}
	private bool tryGetClipCanvasBounds(FastRectMask2D clip, FastCanvas owner, out FastUISpatialBoundsData bounds)
	{
		bounds = default;
		if (clip == null || owner == null || !clip.isClipActiveForCanvas(owner))
		{
			return false;
		}
		RectTransform rectTransform = clip.getRectTransform();
		Rect rect = clip.getClipRect();
		Matrix4x4 localToCanvas = owner.transform.worldToLocalMatrix * rectTransform.localToWorldMatrix;
		Vector3 p0 = localToCanvas.MultiplyPoint3x4(new Vector3(rect.xMin, rect.yMin, 0.0f));
		Vector3 p1 = localToCanvas.MultiplyPoint3x4(new Vector3(rect.xMin, rect.yMax, 0.0f));
		Vector3 p2 = localToCanvas.MultiplyPoint3x4(new Vector3(rect.xMax, rect.yMax, 0.0f));
		Vector3 p3 = localToCanvas.MultiplyPoint3x4(new Vector3(rect.xMax, rect.yMin, 0.0f));
		float minX = Mathf.Min(Mathf.Min(p0.x, p1.x), Mathf.Min(p2.x, p3.x));
		float minY = Mathf.Min(Mathf.Min(p0.y, p1.y), Mathf.Min(p2.y, p3.y));
		float maxX = Mathf.Max(Mathf.Max(p0.x, p1.x), Mathf.Max(p2.x, p3.x));
		float maxY = Mathf.Max(Mathf.Max(p0.y, p1.y), Mathf.Max(p2.y, p3.y));
		bounds = new FastUISpatialBoundsData(minX, minY, maxX, maxY, 0.0f);
		return true;
	}
	private bool isOutside(float minX, float minY, float maxX, float maxY, float clipMinX, float clipMinY, float clipMaxX, float clipMaxY)
	{
		return maxX <= clipMinX || minX >= clipMaxX || maxY <= clipMinY || minY >= clipMaxY;
	}
	private void refreshElementMaskState(FastUIRenderElement element, int selfPackedDepth, int parentPackedDepth, bool selfClipActive, bool suppressBatchNotify)
	{
		int parentDepth = getMaskDepth(parentPackedDepth);
		int readerDepth = parentDepth + (selfClipActive ? 1 : 0);
		int selfDepth = getMaskDepth(selfPackedDepth);
		FastMask attachedMask = element.getAttachedMask();
		bool selfMaskActive = attachedMask != null && attachedMask.getGraphic() == element && selfDepth > readerDepth;
		if (selfMaskActive)
		{
			element.setMaskState(FastUIMaskState.writer(readerDepth, attachedMask.getShowMaskGraphic()), suppressBatchNotify);
			return;
		}
		element.setMaskState(readerDepth > 0 ? FastUIMaskState.reader(Mathf.Min(readerDepth, FastMaskUtility.MAX_MASK_DEPTH)) : default, suppressBatchNotify);
	}
	private int packDepth(int maskDepth, int clipDepth)
	{
		return (maskDepth & DEPTH_MASK) | ((clipDepth & DEPTH_MASK) << CLIP_DEPTH_SHIFT);
	}
	private int getMaskDepth(int packedDepth)
	{
		return packedDepth & DEPTH_MASK;
	}
	private int getClipDepth(int packedDepth)
	{
		return (packedDepth >> CLIP_DEPTH_SHIFT) & DEPTH_MASK;
	}
	private void removeInvalidClips(FastCanvas owner)
	{
		for (int i = mClipRects.Count - 1; i >= 0; --i)
		{
			FastRectMask2D clip = mClipRects[i];
			if (clip == null || clip.getCanvas() != owner || !clip.isActiveAndEnabled)
			{
				mClipRects.RemoveAt(i);
			}
		}
	}
}
