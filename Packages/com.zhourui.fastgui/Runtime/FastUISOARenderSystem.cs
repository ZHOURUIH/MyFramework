using EasyECS;
using System.Collections.Generic;
using UnityEngine;

// 应用层显式SOA分组的RenderOrder投影器。
// Logical Hierarchy始终保持AoS；Full Build只在结构变化时执行。
// BatchKey变化只标记对应Lane Dirty，不重新扫描全部Record。
// Nested SOA会在Full Build阶段递归展开为最终扁平Lane；按递归深度复用Scratch，Warmup后不再为Record/Nested数组反复分配。运行期Dirty Lane仍然直接O(1)定位。
// FixedStructure保持原Fast Path与Nested能力；SparseStructure只在Full Build阶段分析相对Hierarchy Role和先后约束。
// SparseStructure当前只支持单层Group，避免把Sparse与Nested递归一次性耦合进运行时。
// Adjacent Group Merge只在Full Build处理：双方显式允许、顶层FixedStructure、无Nested且Logical RenderRange连续时，合并为一个虚拟Record集合。
public sealed class FastUISOARenderSystem
{
	private sealed class NestedBuildScratch
	{
		public readonly List<Transform> mRecords = new();
		public readonly Int_ECSList mRecordStarts = new(16);
		public readonly List<FastSOARenderGroup> mReferenceNestedGroups = new();
		public readonly Int_ECSList mNestedRelativeStarts = new(8);
		public readonly Int_ECSList mNestedRegionCounts = new(8);
		public readonly List<List<FastSOARenderGroup>> mNestedGroupSets = new();
		public readonly List<FastSOARenderGroup> mNestedScratch = new();
		public void clearGroupSet()
		{
			mRecords.Clear();
		}
		public void prepareRecordSet(int recordCount)
		{
			mRecordStarts.EnsureCount(recordCount);
			mReferenceNestedGroups.Clear();
			mNestedScratch.Clear();
		}
		public void prepareNestedSets(int nestedCount)
		{
			mNestedRelativeStarts.EnsureCount(nestedCount);
			mNestedRegionCounts.EnsureCount(nestedCount);
			while (mNestedGroupSets.Count < nestedCount)
			{
				mNestedGroupSets.Add(new List<FastSOARenderGroup>(8));
			}
			for (int i = 0; i < mNestedGroupSets.Count; ++i)
			{
				mNestedGroupSets[i].Clear();
			}
		}
		public void dispose()
		{
			mRecordStarts.Dispose();
			mNestedRelativeStarts.Dispose();
			mNestedRegionCounts.Dispose();
			mRecords.Clear();
			mReferenceNestedGroups.Clear();
			mNestedScratch.Clear();
			for (int i = 0; i < mNestedGroupSets.Count; ++i)
			{
				mNestedGroupSets[i].Clear();
			}
			mNestedGroupSets.Clear();
		}
	}
	private readonly FastCanvas mOwnerCanvas;
	private readonly HashSet<FastSOARenderGroup> mGroups = new();
	private readonly List<FastSOARenderGroup> mSortedGroups = new();
	private readonly HashSet<FastSOARenderGroup> mConsumedGroups = new();
	private readonly List<List<int>> mBuildLanes = new();
	private readonly List<FastSOARenderGroup> mBuildGroups = new();
	private readonly List<int> mBuildGroupDepths = new();
	private readonly HashSet<FastSOARenderGroup> mBuildGroupSet = new();
	private readonly List<FastSOARenderGroup> mNestedRootGroupSet = new(1);
	private readonly List<NestedBuildScratch> mNestedBuildScratch = new();
	private readonly HashSet<int> mBuildRenderIndexSet = new();
	private readonly FastDictionary<string, int> mSparseRoleMap = new();
	private readonly Int_ECSList mSparseRoleCompatibilityIDs;
	private readonly List<string> mSparseRoleCompatibilityNames = new();
	private readonly List<List<int>> mSparseRoleMembers = new();
	private readonly List<List<int>> mSparseOutgoing = new();
	private readonly List<int> mSparseIndegrees = new();
	private readonly List<bool> mSparseRoleEmitted = new();
	private readonly List<int> mSparseTopoOrder = new();
	private readonly HashSet<int> mSparseRecordRoleSet = new();
	private readonly List<int> mSparseRecordRoles = new();
	private readonly List<Transform> mSparsePathNodes = new();
	private readonly List<FastUIRenderElement> mSparseSameTransformElements = new(4);
	private readonly Int_ECSList mDrawOrder;
	private readonly Int_ECSList mRecordStarts;
	private readonly Int_ECSList mRenderToLaneIndex;
	private readonly Int_ECSList mLaneDrawStarts;
	private readonly Int_ECSList mLaneCounts;
	private readonly Int_ECSList mLaneBatchKeyCounts;
	private readonly Int_ECSList mDirtyLanes;
	private readonly Bool_ECSList mLaneDirtyFlags;
	private readonly Int_ECSList mLaneMembers;
	// Dirty Lane已经天然是DrawOrder中的连续区间。保留本帧Dirty/Changed区间，让Index和Batch只修补受影响Lane。
	private readonly FastUIRangeData_ECSList mDirtyLaneDrawRanges;
	private readonly FastUIRangeData_ECSList mChangedLaneDrawRanges;
	// 由FastUIBatchSystem持有并按Logical RenderIndex维护；SOA热循环只读取其Direct Column。
	private FastUIBatchElementData_ECSList mBatchElementStates;
	private readonly List<Material> mLaneMaterials = new();
	private readonly List<Texture> mLaneTextures = new();
	private int mConvertedGroupCount;
	private int mRootGroupCount;
	private int mNestedGroupCount;
	private int mMaxNestedDepth;
	private int mRecordCount;
	private int mLaneCount;
	private int mProjectedElementCount;
	private int mBatchKeyGroupCount;
	private int mInvalidGroupCount;
	private int mAdjacentMergeClusterCount;
	private int mAdjacentMergedGroupCount;
	private int mBuildLaneCount;
	private int mBuildRecordCount;
	private int mSparseRoleCount;
	private bool mDrawOrderReordered;
	public FastUISOARenderSystem(FastCanvas ownerCanvas)
	{
		mOwnerCanvas = ownerCanvas;
		mDrawOrder = new Int_ECSList(256);
		mRecordStarts = new Int_ECSList(256);
		mRenderToLaneIndex = new Int_ECSList(256);
		mLaneDrawStarts = new Int_ECSList(16);
		mLaneCounts = new Int_ECSList(16);
		mLaneBatchKeyCounts = new Int_ECSList(16);
		mDirtyLanes = new Int_ECSList(16);
		mLaneDirtyFlags = new Bool_ECSList(16);
		mLaneMembers = new Int_ECSList(256);
		mDirtyLaneDrawRanges = new FastUIRangeData_ECSList(16);
		mChangedLaneDrawRanges = new FastUIRangeData_ECSList(16);
		mSparseRoleCompatibilityIDs = new Int_ECSList(16);
	}
	public int getGroupCount()
	{
		return mGroups.Count;
	}
	public int getConvertedGroupCount()
	{
		return mConvertedGroupCount;
	}
	public int getRootGroupCount()
	{
		return mRootGroupCount;
	}
	public int getNestedGroupCount()
	{
		return mNestedGroupCount;
	}
	public int getMaxNestedDepth()
	{
		return mMaxNestedDepth;
	}
	public int getRecordCount()
	{
		return mRecordCount;
	}
	public int getLaneCount()
	{
		return mLaneCount;
	}
	public int getProjectedElementCount()
	{
		return mProjectedElementCount;
	}
	public int getBatchKeyGroupCount()
	{
		return mBatchKeyGroupCount;
	}
	public int getInvalidGroupCount()
	{
		return mInvalidGroupCount;
	}
	public int getAdjacentMergeClusterCount()
	{
		return mAdjacentMergeClusterCount;
	}
	public int getAdjacentMergedGroupCount()
	{
		return mAdjacentMergedGroupCount;
	}
	public bool isDrawOrderReordered()
	{
		return mDrawOrderReordered;
	}
	public bool hasDirtyLanes()
	{
		return mDirtyLanes.Count > 0;
	}
	public Int_ECSList getDrawOrder()
	{
		return mDrawOrder;
	}
	public FastUIRangeData_ECSList getDirtyLaneDrawRanges()
	{
		return mDirtyLaneDrawRanges;
	}
	public FastUIRangeData_ECSList getChangedLaneDrawRanges()
	{
		return mChangedLaneDrawRanges;
	}
	public int getLaneIndexForRenderIndex(int renderIndex)
	{
		return (uint)renderIndex < (uint)mRenderToLaneIndex.Count ? mRenderToLaneIndex[renderIndex] : -1;
	}
	public void register(FastSOARenderGroup group)
	{
		if (group != null)
		{
			mGroups.Add(group);
		}
	}
	public void unregister(FastSOARenderGroup group)
	{
		if (group != null)
		{
			mGroups.Remove(group);
		}
	}
	// O(1)判断RenderElement是否属于已成功转换的SOA Lane，并去重记录Dirty Lane。
	public bool markBatchKeyDirty(int renderIndex)
	{
		if ((uint)renderIndex >= (uint)mRenderToLaneIndex.Count)
		{
			return false;
		}
		int laneIndex = mRenderToLaneIndex[renderIndex];
		if ((uint)laneIndex >= (uint)mLaneDrawStarts.Count)
		{
			return false;
		}
		var laneDirtyFlags = mLaneDirtyFlags.getValueColumn();
		if (!laneDirtyFlags[laneIndex])
		{
			laneDirtyFlags[laneIndex] = true;
			mDirtyLanes.Add(laneIndex);
		}
		return true;
	}
	public void buildDrawOrder(List<FastUIRenderElement> renderElements, FastUITransformRangeSystem transformRanges,
		FastUIBatchElementData_ECSList batchElementStates, ref FastUIFrameStats stats)
	{
		mBatchElementStates = batchElementStates;
		int renderCount = renderElements != null ? renderElements.Count : 0;
		prepareIdentityOrder(renderCount);
		prepareRenderToLaneIndex(renderCount);
		mLaneDrawStarts.Clear();
		mLaneCounts.Clear();
		mLaneBatchKeyCounts.Clear();
		mLaneDirtyFlags.Clear();
		mDirtyLanes.Clear();
		mDirtyLaneDrawRanges.Clear();
		mChangedLaneDrawRanges.Clear();
		mConsumedGroups.Clear();
		mConvertedGroupCount = 0;
		mRootGroupCount = 0;
		mNestedGroupCount = 0;
		mMaxNestedDepth = 0;
		mRecordCount = 0;
		mLaneCount = 0;
		mProjectedElementCount = 0;
		mBatchKeyGroupCount = 0;
		mInvalidGroupCount = 0;
		mAdjacentMergeClusterCount = 0;
		mAdjacentMergedGroupCount = 0;
		mSortedGroups.Clear();
		if (mGroups.Count == 1)
		{
			foreach (FastSOARenderGroup group in mGroups)
			{
				if (isUsableGroup(group))
				{
					mSortedGroups.Add(group);
				}
			}
		}
		else
		{
			foreach (FastSOARenderGroup group in mGroups)
			{
				if (isUsableGroup(group) && !hasUsableSOAAncestor(group.transform))
				{
					mSortedGroups.Add(group);
				}
			}
		}
		mSortedGroups.Sort(compareGroupByRenderStart);
		mRootGroupCount = mSortedGroups.Count;
		int previousGroupEnd = -1;
		for (int groupIndex = 0; groupIndex < mSortedGroups.Count;)
		{
			int adjacentEnd = findAdjacentMergeChainEnd(groupIndex, transformRanges);
			bool merged = false;
			for (int candidateEnd = adjacentEnd; candidateEnd > groupIndex; --candidateEnd)
			{
				if (!tryBuildAdjacentFlatGroupSet(renderElements, transformRanges, groupIndex, candidateEnd, previousGroupEnd, out int mergedGroupEnd))
				{
					continue;
				}
				previousGroupEnd = mergedGroupEnd;
				groupIndex = candidateEnd + 1;
				merged = true;
				break;
			}
			if (merged)
			{
				continue;
			}
			FastSOARenderGroup group = mSortedGroups[groupIndex++];
			if (tryBuildGroup(renderElements, transformRanges, group, previousGroupEnd, out int groupEnd))
			{
				previousGroupEnd = groupEnd;
			}
		}
		refreshDrawOrderReordered(renderCount);
		writeStats(ref stats);
	}
	// BatchKey变化只重排Dirty Lane。Nested SOA在Full Build后已经完全扁平化，因此这里与单层SOA完全相同。
	public bool refreshDirtyLanes(FastUIBatchElementData_ECSList batchElementStates, ref FastUIFrameStats stats)
	{
		mBatchElementStates = batchElementStates;
		mDirtyLaneDrawRanges.Clear();
		mChangedLaneDrawRanges.Clear();
		if (mDirtyLanes.Count == 0)
		{
			return false;
		}
		bool orderChanged = false;
		var dirtyLaneColumn = mDirtyLanes.getValueColumn();
		var laneDirtyFlags = mLaneDirtyFlags.getValueColumn();
		var laneDrawStarts = mLaneDrawStarts.getValueColumn();
		var laneCounts = mLaneCounts.getValueColumn();
		for (int i = 0; i < mDirtyLanes.Count; ++i)
		{
			int laneIndex = dirtyLaneColumn[i];
			if ((uint)laneIndex >= (uint)mLaneDrawStarts.Count)
			{
				continue;
			}
			int drawStart = laneDrawStarts[laneIndex];
			int drawCount = laneCounts[laneIndex];
			if (drawCount > 0)
			{
				mDirtyLaneDrawRanges.Add(new FastUIRangeData(drawStart, drawStart + drawCount));
			}
			bool laneOrderChanged = reorderLane(laneIndex);
			if (laneOrderChanged && drawCount > 0)
			{
				mChangedLaneDrawRanges.Add(new FastUIRangeData(drawStart, drawStart + drawCount));
			}
			orderChanged |= laneOrderChanged;
			laneDirtyFlags[laneIndex] = false;
		}
		mDirtyLanes.Clear();
		FastUIRangeUtility.merge(mDirtyLaneDrawRanges);
		FastUIRangeUtility.merge(mChangedLaneDrawRanges);
		if (orderChanged)
		{
			mDrawOrderReordered = true;
		}
		writeStats(ref stats);
		return orderChanged;
	}
	public void dispose()
	{
		mGroups.Clear();
		mSortedGroups.Clear();
		mConsumedGroups.Clear();
		mBuildGroups.Clear();
		mBuildGroupDepths.Clear();
		mBuildGroupSet.Clear();
		mNestedRootGroupSet.Clear();
		for (int i = 0; i < mNestedBuildScratch.Count; ++i)
		{
			mNestedBuildScratch[i].dispose();
		}
		mNestedBuildScratch.Clear();
		mBuildRenderIndexSet.Clear();
		mSparseRoleMap.Clear();
		mSparseRoleCompatibilityNames.Clear();
		for (int i = 0; i < mSparseRoleMembers.Count; ++i)
		{
			mSparseRoleMembers[i].Clear();
			mSparseOutgoing[i].Clear();
		}
		mSparseRoleMembers.Clear();
		mSparseOutgoing.Clear();
		mSparseIndegrees.Clear();
		mSparseRoleEmitted.Clear();
		mSparseTopoOrder.Clear();
		mSparseRecordRoleSet.Clear();
		mSparseRecordRoles.Clear();
		mSparsePathNodes.Clear();
		for (int i = 0; i < mBuildLanes.Count; ++i)
		{
			mBuildLanes[i].Clear();
		}
		mBuildLanes.Clear();
		mLaneMaterials.Clear();
		mLaneTextures.Clear();
		mDrawOrder.Dispose();
		mRecordStarts.Dispose();
		mRenderToLaneIndex.Dispose();
		mLaneDrawStarts.Dispose();
		mLaneCounts.Dispose();
		mLaneBatchKeyCounts.Dispose();
		mDirtyLanes.Dispose();
		mLaneDirtyFlags.Dispose();
		mLaneMembers.Dispose();
		mDirtyLaneDrawRanges.Dispose();
		mChangedLaneDrawRanges.Dispose();
		mSparseRoleCompatibilityIDs.Dispose();
	}
	private bool isUsableGroup(FastSOARenderGroup group)
	{
		return group != null && group.isActiveAndEnabled && group.getCanvas() == mOwnerCanvas;
	}
	private bool hasUsableSOAAncestor(Transform transform)
	{
		Transform current = transform != null ? transform.parent : null;
		while (current != null)
		{
			if (current.TryGetComponent(out FastSOARenderGroup group) && isUsableGroup(group))
			{
				return true;
			}
			if (current == mOwnerCanvas.transform)
			{
				break;
			}
			current = current.parent;
		}
		return false;
	}
	private int compareGroupByRenderStart(FastSOARenderGroup first, FastSOARenderGroup second)
	{
		return getGroupRenderStart(first).CompareTo(getGroupRenderStart(second));
	}
	private int getGroupRenderStart(FastSOARenderGroup group)
	{
		if (group == null || group.getRectTransform().childCount == 0)
		{
			return int.MaxValue;
		}
		Transform firstChild = group.getRectTransform().GetChild(0);
		if (mOwnerCanvas.tryGetTransformRenderRange(firstChild, out int start, out _))
		{
			return start;
		}
		return int.MaxValue;
	}
	private int findAdjacentMergeChainEnd(int startIndex, FastUITransformRangeSystem transformRanges)
	{
		if ((uint)startIndex >= (uint)mSortedGroups.Count)
		{
			return startIndex;
		}
		FastSOARenderGroup first = mSortedGroups[startIndex];
		if (!isAdjacentMergeEligible(first) || !tryGetGroupRegion(first, transformRanges, out int firstStart, out int firstCount, out _) || firstCount <= 0)
		{
			return startIndex;
		}
		int expectedStart = firstStart + firstCount;
		int endIndex = startIndex;
		for (int groupIndex = startIndex + 1; groupIndex < mSortedGroups.Count; ++groupIndex)
		{
			FastSOARenderGroup group = mSortedGroups[groupIndex];
			if (!isAdjacentMergeEligible(group) || !tryGetGroupRegion(group, transformRanges, out int regionStart, out int regionCount, out _) ||
				regionCount <= 0 || regionStart != expectedStart)
			{
				break;
			}
			endIndex = groupIndex;
			expectedStart += regionCount;
		}
		return endIndex;
	}
	private bool isAdjacentMergeEligible(FastSOARenderGroup group)
	{
		return isUsableGroup(group) && group.getAllowAdjacentGroupMerge() && group.getStructureMode() == FastSOAStructureMode.FixedStructure &&
			group.getRectTransform().childCount > 1 && !hasUsableSOADescendant(group);
	}
	private bool tryBuildAdjacentFlatGroupSet(List<FastUIRenderElement> renderElements, FastUITransformRangeSystem transformRanges,
		int startGroupIndex, int endGroupIndex, int previousGroupEnd, out int groupEnd)
	{
		groupEnd = previousGroupEnd;
		if (endGroupIndex <= startGroupIndex)
		{
			return false;
		}
		resetBuildScratch();
		mRecordStarts.Clear();
		int firstStart = -1;
		int expectedStart = -1;
		int recordRenderCount = -1;
		int totalRecordCount = 0;
		for (int groupIndex = startGroupIndex; groupIndex <= endGroupIndex; ++groupIndex)
		{
			FastSOARenderGroup group = mSortedGroups[groupIndex];
			if (!isAdjacentMergeEligible(group))
			{
				resetBuildScratch();
				return false;
			}
			addBuildGroup(group, 0);
			RectTransform root = group.getRectTransform();
			for (int recordIndex = 0; recordIndex < root.childCount; ++recordIndex)
			{
				Transform record = root.GetChild(recordIndex);
				if (!transformRanges.tryGetRange(record, out int recordStart, out int renderCount) || renderCount <= 0)
				{
					resetBuildScratch();
					return false;
				}
				if (recordRenderCount < 0)
				{
					recordRenderCount = renderCount;
					firstStart = recordStart;
					expectedStart = recordStart;
				}
				else if (renderCount != recordRenderCount)
				{
					resetBuildScratch();
					return false;
				}
				if (recordStart != expectedStart)
				{
					resetBuildScratch();
					return false;
				}
				for (int slot = 0; slot < renderCount; ++slot)
				{
					int renderIndex = recordStart + slot;
					FastUIRenderElement element = (uint)renderIndex < (uint)renderElements.Count ? renderElements[renderIndex] : null;
					if (element == null || element.isStencilBarrier())
					{
						resetBuildScratch();
						return false;
					}
					if (totalRecordCount > 0)
					{
						FastUIRenderElement reference = renderElements[mRecordStarts[0] + slot];
						if (reference == null || reference.getSOACompatibilityID() != element.getSOACompatibilityID())
						{
							resetBuildScratch();
							return false;
						}
					}
				}
				mRecordStarts.Add(recordStart);
				++totalRecordCount;
				expectedStart += renderCount;
			}
		}
		if (firstStart < 0 || firstStart <= previousGroupEnd || recordRenderCount <= 0 || totalRecordCount <= 1)
		{
			resetBuildScratch();
			return false;
		}
		var recordStarts = mRecordStarts.getValueColumn();
		for (int slot = 0; slot < recordRenderCount; ++slot)
		{
			List<int> lane = acquireBuildLane();
			for (int recordIndex = 0; recordIndex < totalRecordCount; ++recordIndex)
			{
				lane.Add(recordStarts[recordIndex] + slot);
			}
		}
		int regionEnd = expectedStart;
		if (!validateBuildProjection(firstStart, regionEnd, out _))
		{
			resetBuildScratch();
			return false;
		}
		commitBuildProjection(firstStart);
		commitBuildGroups();
		mBuildRecordCount = totalRecordCount;
		mRecordCount += totalRecordCount;
		mProjectedElementCount += regionEnd - firstStart;
		++mAdjacentMergeClusterCount;
		mAdjacentMergedGroupCount += endGroupIndex - startGroupIndex + 1;
		groupEnd = regionEnd - 1;
		return true;
	}
	private bool tryBuildGroup(List<FastUIRenderElement> renderElements, FastUITransformRangeSystem transformRanges,
		FastSOARenderGroup group, int previousGroupEnd, out int groupEnd)
	{
		bool hasNested = hasUsableSOADescendant(group);
		if (group.getStructureMode() == FastSOAStructureMode.SparseStructure)
		{
			if (hasNested)
			{
				groupEnd = previousGroupEnd;
				markGroupTreeInvalid(group, "SparseStructure当前只支持单层SOA Group,暂不与Nested SOA混用");
				return false;
			}
			return tryBuildSparseGroup(renderElements, transformRanges, group, previousGroupEnd, out groupEnd);
		}
		if (mGroups.Count == mRootGroupCount || !hasNested)
		{
			return tryBuildFlatGroup(renderElements, transformRanges, group, previousGroupEnd, out groupEnd);
		}
		return tryBuildNestedGroup(renderElements, transformRanges, group, previousGroupEnd, out groupEnd);
	}
	private bool hasUsableSOADescendant(FastSOARenderGroup root)
	{
		if (root == null || mGroups.Count <= mRootGroupCount)
		{
			return false;
		}
		foreach (FastSOARenderGroup group in mGroups)
		{
			if (group != root && isUsableGroup(group) && group.transform.IsChildOf(root.transform))
			{
				return true;
			}
		}
		return false;
	}
	// 单层Fixed-Structure SOA保持独立直接构建路径。
	// 不创建Managed Record/List/HashSet，不递归扫描Hierarchy；Nested功能不会让普通SOA承担额外构建成本。
	private bool tryBuildFlatGroup(List<FastUIRenderElement> renderElements, FastUITransformRangeSystem transformRanges,
		FastSOARenderGroup group, int previousGroupEnd, out int groupEnd)
	{
		groupEnd = previousGroupEnd;
		RectTransform root = group.getRectTransform();
		int recordCount = root.childCount;
		if (recordCount <= 1)
		{
			group.setBuildResult(true, null);
			return true;
		}
		mRecordStarts.Clear();
		int recordRenderCount = -1;
		int firstStart = -1;
		int expectedStart = -1;
		for (int recordIndex = 0; recordIndex < recordCount; ++recordIndex)
		{
			Transform record = root.GetChild(recordIndex);
			if (!transformRanges.tryGetRange(record, out int recordStart, out int renderCount) || renderCount <= 0)
			{
				group.setBuildResult(false, "直接子节点没有RenderElement,Record=" + record.name);
				++mInvalidGroupCount;
				return false;
			}
			if (recordRenderCount < 0)
			{
				recordRenderCount = renderCount;
				firstStart = recordStart;
				expectedStart = recordStart;
			}
			else if (recordRenderCount != renderCount)
			{
				group.setBuildResult(false, "Record RenderElement数量不一致,期望=" + recordRenderCount + ",实际=" + renderCount + ",Record=" + record.name);
				++mInvalidGroupCount;
				return false;
			}
			if (recordStart != expectedStart)
			{
				group.setBuildResult(false, "Record RenderRange不连续,当前仍为Fixed-Structure SOA,Record=" + record.name);
				++mInvalidGroupCount;
				return false;
			}
			for (int slot = 0; slot < renderCount; ++slot)
			{
				int renderIndex = recordStart + slot;
				FastUIRenderElement element = (uint)renderIndex < (uint)renderElements.Count ? renderElements[renderIndex] : null;
				// recordStart/renderCount来自当前TransformRange的DFS连续子树，本区间内元素无需再次逐个IsChildOf验证。
				if (element == null)
				{
					group.setBuildResult(false, "Record中存在空RenderSlot,Record=" + record.name + ",Slot=" + slot);
					++mInvalidGroupCount;
					return false;
				}
				if (element.isStencilBarrier())
				{
					group.setBuildResult(false, "Record内部包含Stencil Writer/Pop,SOA不能跨越Stencil Barrier,Record=" + record.name);
					++mInvalidGroupCount;
					return false;
				}
				if (recordIndex > 0)
				{
					FastUIRenderElement reference = renderElements[mRecordStarts[0] + slot];
					if (reference == null || reference.getSOACompatibilityID() != element.getSOACompatibilityID())
					{
						group.setBuildResult(false, "相同RenderSlot类型不一致,Slot=" + slot + ",Record=" + record.name +
							",期望=" + (reference != null ? reference.getSOACompatibilityName() : "null") + ",实际=" + element.getSOACompatibilityName());
						++mInvalidGroupCount;
						return false;
					}
				}
			}
			mRecordStarts.Add(recordStart);
			expectedStart += renderCount;
		}
		int regionEnd = firstStart + recordCount * recordRenderCount;
		if (firstStart <= previousGroupEnd)
		{
			group.setBuildResult(false, "顶层SOA Group RenderRange发生重叠");
			++mInvalidGroupCount;
			return false;
		}
		int writeIndex = firstStart;
		var recordStarts = mRecordStarts.getValueColumn();
		var drawOrder = mDrawOrder.getValueColumn();
		var renderToLane = mRenderToLaneIndex.getValueColumn();
		var batchMaterialColumn = mBatchElementStates.getMaterialColumn();
		var batchTextureColumn = mBatchElementStates.getTextureColumn();
		for (int slot = 0; slot < recordRenderCount; ++slot)
		{
			int laneIndex = mLaneDrawStarts.Count;
			int laneDrawStart = writeIndex;
			mLaneMaterials.Clear();
			mLaneTextures.Clear();
			for (int recordIndex = 0; recordIndex < recordCount; ++recordIndex)
			{
				int renderIndex = recordStarts[recordIndex] + slot;
				Material material = batchMaterialColumn[renderIndex];
				Texture texture = batchTextureColumn[renderIndex];
				if (!containsBatchKey(material, texture))
				{
					mLaneMaterials.Add(material);
					mLaneTextures.Add(texture);
				}
			}
			for (int keyIndex = 0; keyIndex < mLaneMaterials.Count; ++keyIndex)
			{
				Material material = mLaneMaterials[keyIndex];
				Texture texture = mLaneTextures[keyIndex];
				for (int recordIndex = 0; recordIndex < recordCount; ++recordIndex)
				{
					int renderIndex = recordStarts[recordIndex] + slot;
					if (batchMaterialColumn[renderIndex] == material && batchTextureColumn[renderIndex] == texture)
					{
						drawOrder[writeIndex++] = renderIndex;
					}
				}
			}
			mLaneDrawStarts.Add(laneDrawStart);
			mLaneCounts.Add(recordCount);
			mLaneBatchKeyCounts.Add(mLaneMaterials.Count);
			mLaneDirtyFlags.Add(false);
			for (int recordIndex = 0; recordIndex < recordCount; ++recordIndex)
			{
				renderToLane[recordStarts[recordIndex] + slot] = laneIndex;
			}
			mBatchKeyGroupCount += mLaneMaterials.Count;
			++mLaneCount;
		}
		if (writeIndex != regionEnd)
		{
			for (int i = firstStart; i < regionEnd; ++i)
			{
				drawOrder[i] = i;
				renderToLane[i] = -1;
			}
			group.setBuildResult(false, "SOA投影成员数量异常");
			++mInvalidGroupCount;
			return false;
		}
		group.setBuildResult(true, null);
		++mConvertedGroupCount;
		mRecordCount += recordCount;
		mProjectedElementCount += recordCount * recordRenderCount;
		groupEnd = regionEnd - 1;
		return true;
	}
	private bool tryBuildSparseGroup(List<FastUIRenderElement> renderElements, FastUITransformRangeSystem transformRanges,
		FastSOARenderGroup group, int previousGroupEnd, out int groupEnd)
	{
		groupEnd = previousGroupEnd;
		if (!tryGetGroupRegion(group, transformRanges, out int firstStart, out int regionCount, out string regionError))
		{
			group.setBuildResult(false, regionError);
			++mInvalidGroupCount;
			return false;
		}
		if (regionCount <= 0)
		{
			group.setBuildResult(true, null);
			return true;
		}
		int regionEnd = firstStart + regionCount;
		if (firstStart <= previousGroupEnd)
		{
			group.setBuildResult(false, "顶层SOA Group RenderRange发生重叠");
			++mInvalidGroupCount;
			return false;
		}
		resetBuildScratch();
		resetSparseScratch();
		addBuildGroup(group, 0);
		RectTransform root = group.getRectTransform();
		int recordCount = root.childCount;
		for (int recordIndex = 0; recordIndex < recordCount; ++recordIndex)
		{
			Transform record = root.GetChild(recordIndex);
			if (!transformRanges.tryGetRange(record, out int recordStart, out int renderCount) || renderCount <= 0)
			{
				group.setBuildResult(false, "Sparse SOA Record没有RenderElement,Record=" + record.name);
				++mInvalidGroupCount;
				return false;
			}
			mSparseRecordRoleSet.Clear();
			mSparseRecordRoles.Clear();
			for (int offset = 0; offset < renderCount; ++offset)
			{
				int renderIndex = recordStart + offset;
				FastUIRenderElement element = (uint)renderIndex < (uint)renderElements.Count ? renderElements[renderIndex] : null;
				if (!validateSparseElement(record, element, renderIndex, out string elementError))
				{
					group.setBuildResult(false, elementError);
					++mInvalidGroupCount;
					return false;
				}
				string roleKey = getSparseRoleKey(record, element);
				if (string.IsNullOrEmpty(roleKey))
				{
					group.setBuildResult(false, "Sparse SOA无法生成Hierarchy Role,Record=" + record.name + ",RenderIndex=" + renderIndex);
					++mInvalidGroupCount;
					return false;
				}
				int compatibilityID = element.getSOACompatibilityID();
				int roleIndex = getOrCreateSparseRole(roleKey, compatibilityID, element.getSOACompatibilityName());
				if (mSparseRoleCompatibilityIDs[roleIndex] != compatibilityID)
				{
					group.setBuildResult(false, "Sparse SOA相同Hierarchy Role类型不一致,Role=" + roleKey +
						",期望=" + mSparseRoleCompatibilityNames[roleIndex] + ",实际=" + element.getSOACompatibilityName());
					++mInvalidGroupCount;
					return false;
				}
				if (!mSparseRecordRoleSet.Add(roleIndex))
				{
					group.setBuildResult(false, "Sparse SOA同一Record出现重复Hierarchy Role,Record=" + record.name + ",Role=" + roleKey);
					++mInvalidGroupCount;
					return false;
				}
				mSparseRoleMembers[roleIndex].Add(renderIndex);
				if (mSparseRecordRoles.Count > 0)
				{
					addSparseEdge(mSparseRecordRoles[^1], roleIndex);
				}
				mSparseRecordRoles.Add(roleIndex);
			}
		}
		if (!tryBuildSparseTopologicalOrder(out string topologyError))
		{
			group.setBuildResult(false, topologyError);
			++mInvalidGroupCount;
			return false;
		}
		for (int orderIndex = 0; orderIndex < mSparseTopoOrder.Count; ++orderIndex)
		{
			List<int> lane = acquireBuildLane();
			lane.AddRange(mSparseRoleMembers[mSparseTopoOrder[orderIndex]]);
		}
		if (!validateBuildProjection(firstStart, regionEnd, out string projectionError))
		{
			group.setBuildResult(false, projectionError);
			++mInvalidGroupCount;
			return false;
		}
		commitBuildProjection(firstStart);
		commitBuildGroups();
		mBuildRecordCount = recordCount;
		mRecordCount += recordCount;
		mProjectedElementCount += regionCount;
		groupEnd = regionEnd - 1;
		return true;
	}
	private bool validateSparseElement(Transform record, FastUIRenderElement element, int renderIndex, out string error)
	{
		error = null;
		if (element == null || element.getCanvas() != mOwnerCanvas)
		{
			error = "Sparse SOA存在空RenderElement或元素不属于当前FastCanvas,Record=" + record.name + ",RenderIndex=" + renderIndex;
			return false;
		}
		Transform elementTransform = element.transform;
		if (elementTransform != record && !elementTransform.IsChildOf(record))
		{
			error = "Sparse SOA Record RenderRange包含非本Record元素,Record=" + record.name + ",RenderIndex=" + renderIndex;
			return false;
		}
		if (element.isStencilBarrier())
		{
			error = "Sparse SOA Record内部包含Stencil Writer/Pop,SOA不能跨越Stencil Barrier,Record=" + record.name;
			return false;
		}
		return true;
	}
	private void resetSparseScratch()
	{
		mSparseRoleMap.Clear();
		for (int i = 0; i < mSparseRoleCount; ++i)
		{
			mSparseRoleMembers[i].Clear();
			mSparseOutgoing[i].Clear();
			mSparseIndegrees[i] = 0;
			mSparseRoleEmitted[i] = false;
		}
		mSparseRoleCount = 0;
		mSparseTopoOrder.Clear();
		mSparseRecordRoleSet.Clear();
		mSparseRecordRoles.Clear();
		mSparsePathNodes.Clear();
	}
	private int getOrCreateSparseRole(string roleKey, int compatibilityID, string compatibilityName)
	{
		if (mSparseRoleMap.TryGetValue(roleKey, out int roleIndex))
		{
			return roleIndex;
		}
		roleIndex = mSparseRoleCount++;
		mSparseRoleMap.Add(roleKey, roleIndex);
		if (roleIndex < mSparseRoleMembers.Count)
		{
			mSparseRoleCompatibilityIDs[roleIndex] = compatibilityID;
			mSparseRoleCompatibilityNames[roleIndex] = compatibilityName;
			mSparseRoleMembers[roleIndex].Clear();
			mSparseOutgoing[roleIndex].Clear();
			mSparseIndegrees[roleIndex] = 0;
			mSparseRoleEmitted[roleIndex] = false;
		}
		else
		{
			mSparseRoleCompatibilityIDs.Add(compatibilityID);
			mSparseRoleCompatibilityNames.Add(compatibilityName);
			mSparseRoleMembers.Add(new List<int>(16));
			mSparseOutgoing.Add(new List<int>(4));
			mSparseIndegrees.Add(0);
			mSparseRoleEmitted.Add(false);
		}
		return roleIndex;
	}
	private string getSparseRoleKey(Transform record, FastUIRenderElement element)
	{
		Transform elementTransform = element != null ? element.transform : null;
		mSparsePathNodes.Clear();
		Transform current = elementTransform;
		while (current != null && current != record)
		{
			mSparsePathNodes.Add(current);
			current = current.parent;
		}
		if (current != record || elementTransform == null)
		{
			return null;
		}
		string key = mSparsePathNodes.Count == 0 ? "$SELF" : string.Empty;
		for (int i = mSparsePathNodes.Count - 1; i >= 0; --i)
		{
			Transform node = mSparsePathNodes[i];
			if (key.Length > 0)
			{
				key += "/";
			}
			key += node.name + "#" + getSameNameSiblingOccurrence(node);
		}
		mSparseSameTransformElements.Clear();
		elementTransform.GetComponents(mSparseSameTransformElements);
		int componentOccurrence = 0;
		for (int i = 0; i < mSparseSameTransformElements.Count; ++i)
		{
			if (mSparseSameTransformElements[i] == element)
			{
				componentOccurrence = i;
				break;
			}
		}
		return key + "@R" + componentOccurrence;
	}
	private int getSameNameSiblingOccurrence(Transform node)
	{
		Transform parent = node != null ? node.parent : null;
		if (parent == null)
		{
			return 0;
		}
		int occurrence = 0;
		int siblingIndex = node.GetSiblingIndex();
		for (int i = 0; i < siblingIndex; ++i)
		{
			if (parent.GetChild(i).name == node.name)
			{
				++occurrence;
			}
		}
		return occurrence;
	}
	private void addSparseEdge(int fromRole, int toRole)
	{
		if (fromRole == toRole)
		{
			return;
		}
		List<int> outgoing = mSparseOutgoing[fromRole];
		for (int i = 0; i < outgoing.Count; ++i)
		{
			if (outgoing[i] == toRole)
			{
				return;
			}
		}
		outgoing.Add(toRole);
		mSparseIndegrees[toRole] = mSparseIndegrees[toRole] + 1;
	}
	private bool tryBuildSparseTopologicalOrder(out string error)
	{
		error = null;
		mSparseTopoOrder.Clear();
		for (int pass = 0; pass < mSparseRoleCount; ++pass)
		{
			int selectedRole = -1;
			for (int roleIndex = 0; roleIndex < mSparseRoleCount; ++roleIndex)
			{
				if (!mSparseRoleEmitted[roleIndex] && mSparseIndegrees[roleIndex] == 0)
				{
					selectedRole = roleIndex;
					break;
				}
			}
			if (selectedRole < 0)
			{
				error = "Sparse SOA不同Record的局部绘制顺序存在冲突环,无法生成稳定Lane顺序";
				return false;
			}
			mSparseRoleEmitted[selectedRole] = true;
			mSparseTopoOrder.Add(selectedRole);
			List<int> outgoing = mSparseOutgoing[selectedRole];
			for (int i = 0; i < outgoing.Count; ++i)
			{
				int targetRole = outgoing[i];
				mSparseIndegrees[targetRole] = mSparseIndegrees[targetRole] - 1;
			}
		}
		return true;
	}
	private bool tryBuildNestedGroup(List<FastUIRenderElement> renderElements, FastUITransformRangeSystem transformRanges,
		FastSOARenderGroup group, int previousGroupEnd, out int groupEnd)
	{
		groupEnd = previousGroupEnd;
		if (!tryGetGroupRegion(group, transformRanges, out int firstStart, out int regionCount, out string regionError))
		{
			markGroupTreeInvalid(group, regionError);
			return false;
		}
		if (regionCount <= 0)
		{
			group.setBuildResult(true, null);
			return true;
		}
		int regionEnd = firstStart + regionCount;
		if (firstStart <= previousGroupEnd)
		{
			markGroupTreeInvalid(group, "顶层SOA Group RenderRange发生重叠");
			return false;
		}
		resetBuildScratch();
		mNestedRootGroupSet.Clear();
		mNestedRootGroupSet.Add(group);
		if (!tryCollectGroupSet(mNestedRootGroupSet, 0, renderElements, transformRanges, out string error))
		{
			markGroupTreeInvalid(group, error);
			return false;
		}
		if (!validateBuildProjection(firstStart, regionEnd, out error))
		{
			markGroupTreeInvalid(group, error);
			return false;
		}
		commitBuildProjection(firstStart);
		commitBuildGroups();
		mRecordCount += mBuildRecordCount;
		mProjectedElementCount += regionCount;
		groupEnd = regionEnd - 1;
		return true;
	}
	private bool tryCollectGroupSet(List<FastSOARenderGroup> groups, int depth, List<FastUIRenderElement> renderElements,
		FastUITransformRangeSystem transformRanges, out string error)
	{
		error = null;
		if (groups == null || groups.Count == 0)
		{
			return true;
		}
		NestedBuildScratch scratch = acquireNestedBuildScratch(depth);
		scratch.clearGroupSet();
		int recordCountPerGroup = -1;
		for (int i = 0; i < groups.Count; ++i)
		{
			FastSOARenderGroup group = groups[i];
			if (!isUsableGroup(group))
			{
				error = "Nested SOA Group未激活或不属于当前FastCanvas";
				return false;
			}
			if (group.getStructureMode() != FastSOAStructureMode.FixedStructure)
			{
				error = "Nested SOA当前只支持FixedStructure,SparseStructure当前只支持单层Group,Group=" + group.name;
				return false;
			}
			int childCount = group.getRectTransform().childCount;
			if (recordCountPerGroup < 0)
			{
				recordCountPerGroup = childCount;
			}
			else if (recordCountPerGroup != childCount)
			{
				error = "Nested SOA对应Group的Record数量不一致,Depth=" + depth + ",Root=" + group.name;
				return false;
			}
			addBuildGroup(group, depth);
		}
		if (recordCountPerGroup <= 0)
		{
			return true;
		}
		List<Transform> records = scratch.mRecords;
		for (int groupIndex = 0; groupIndex < groups.Count; ++groupIndex)
		{
			RectTransform root = groups[groupIndex].getRectTransform();
			for (int recordIndex = 0; recordIndex < recordCountPerGroup; ++recordIndex)
			{
				records.Add(root.GetChild(recordIndex));
			}
		}
		mBuildRecordCount += records.Count;
		return tryCollectRecordSet(scratch, depth, renderElements, transformRanges, out error);
	}
	private bool tryCollectRecordSet(NestedBuildScratch scratch, int depth, List<FastUIRenderElement> renderElements,
		FastUITransformRangeSystem transformRanges, out string error)
	{
		error = null;
		List<Transform> records = scratch.mRecords;
		if (records.Count == 0)
		{
			return true;
		}
		scratch.prepareRecordSet(records.Count);
		var recordStarts = scratch.mRecordStarts.getValueColumn();
		int recordRenderCount = -1;
		for (int i = 0; i < records.Count; ++i)
		{
			Transform record = records[i];
			if (!transformRanges.tryGetRange(record, out int recordStart, out int renderCount) || renderCount <= 0)
			{
				error = "SOA Record没有RenderElement,Record=" + (record != null ? record.name : "null");
				return false;
			}
			if (recordRenderCount < 0)
			{
				recordRenderCount = renderCount;
			}
			else if (recordRenderCount != renderCount)
			{
				error = "Record RenderElement数量不一致,期望=" + recordRenderCount + ",实际=" + renderCount +
					",Record=" + record.name + ",Depth=" + depth;
				return false;
			}
			recordStarts[i] = recordStart;
		}
		List<FastSOARenderGroup> referenceNestedGroups = scratch.mReferenceNestedGroups;
		collectImmediateNestedGroups(records[0], referenceNestedGroups);
		sortGroupsByRenderStart(referenceNestedGroups, transformRanges);
		int nestedCount = referenceNestedGroups.Count;
		scratch.prepareNestedSets(nestedCount);
		var nestedRelativeStarts = scratch.mNestedRelativeStarts.getValueColumn();
		var nestedRegionCounts = scratch.mNestedRegionCounts.getValueColumn();
		List<List<FastSOARenderGroup>> nestedGroupSets = scratch.mNestedGroupSets;
		int referenceCursor = 0;
		for (int nestedIndex = 0; nestedIndex < nestedCount; ++nestedIndex)
		{
			FastSOARenderGroup nestedGroup = referenceNestedGroups[nestedIndex];
			if (!tryGetGroupRegion(nestedGroup, transformRanges, out int nestedStart, out int nestedRegionCount, out string nestedError))
			{
				error = nestedError;
				return false;
			}
			int relativeStart = nestedStart - recordStarts[0];
			if (nestedRegionCount <= 0 || relativeStart < referenceCursor || relativeStart + nestedRegionCount > recordRenderCount)
			{
				error = "Nested SOA RenderRange不在父Record连续范围内,Group=" + nestedGroup.name + ",Depth=" + (depth + 1);
				return false;
			}
			nestedRelativeStarts[nestedIndex] = relativeStart;
			nestedRegionCounts[nestedIndex] = nestedRegionCount;
			referenceCursor = relativeStart + nestedRegionCount;
		}
		List<FastSOARenderGroup> nestedScratch = scratch.mNestedScratch;
		for (int recordIndex = 0; recordIndex < records.Count; ++recordIndex)
		{
			nestedScratch.Clear();
			collectImmediateNestedGroups(records[recordIndex], nestedScratch);
			sortGroupsByRenderStart(nestedScratch, transformRanges);
			if (nestedScratch.Count != nestedCount)
			{
				error = "Record Nested SOA数量不一致,期望=" + nestedCount + ",实际=" + nestedScratch.Count +
					",Record=" + records[recordIndex].name + ",Depth=" + depth;
				return false;
			}
			for (int nestedIndex = 0; nestedIndex < nestedCount; ++nestedIndex)
			{
				FastSOARenderGroup nestedGroup = nestedScratch[nestedIndex];
				if (!tryGetGroupRegion(nestedGroup, transformRanges, out int nestedStart, out int nestedRegionCount, out string nestedError))
				{
					error = nestedError;
					return false;
				}
				int relativeStart = nestedStart - recordStarts[recordIndex];
				if (relativeStart != nestedRelativeStarts[nestedIndex] || nestedRegionCount != nestedRegionCounts[nestedIndex])
				{
					error = "Record Nested SOA布局不一致,Record=" + records[recordIndex].name + ",NestedIndex=" + nestedIndex +
						",Depth=" + depth;
					return false;
				}
				nestedGroupSets[nestedIndex].Add(nestedGroup);
			}
		}
		int cursor = 0;
		for (int nestedIndex = 0; nestedIndex < nestedCount; ++nestedIndex)
		{
			int nestedStart = nestedRelativeStarts[nestedIndex];
			for (; cursor < nestedStart; ++cursor)
			{
				if (!tryAddSlotLane(records, scratch.mRecordStarts, cursor, renderElements, out error))
				{
					return false;
				}
			}
			if (!tryCollectGroupSet(nestedGroupSets[nestedIndex], depth + 1, renderElements, transformRanges, out error))
			{
				return false;
			}
			cursor = nestedStart + nestedRegionCounts[nestedIndex];
		}
		for (; cursor < recordRenderCount; ++cursor)
		{
			if (!tryAddSlotLane(records, scratch.mRecordStarts, cursor, renderElements, out error))
			{
				return false;
			}
		}
		return true;
	}
	private bool tryAddSlotLane(List<Transform> records, Int_ECSList recordStarts, int slot, List<FastUIRenderElement> renderElements, out string error)
	{
		error = null;
		List<int> lane = acquireBuildLane();
		bool hasReferenceCompatibility = false;
		int referenceCompatibilityID = 0;
		string referenceCompatibilityName = null;
		for (int recordIndex = 0; recordIndex < records.Count; ++recordIndex)
		{
			int renderIndex = recordStarts[recordIndex] + slot;
			FastUIRenderElement element = (uint)renderIndex < (uint)renderElements.Count ? renderElements[renderIndex] : null;
			if (element == null || element.getCanvas() != mOwnerCanvas)
			{
				error = "Record中存在空RenderSlot或元素不属于当前FastCanvas,Record=" + records[recordIndex].name + ",Slot=" + slot;
				return false;
			}
			Transform elementTransform = element.transform;
			Transform record = records[recordIndex];
			if (elementTransform != record && !elementTransform.IsChildOf(record))
			{
				error = "Record RenderRange包含非本Record元素,Record=" + record.name + ",Slot=" + slot;
				return false;
			}
			if (element.isStencilBarrier())
			{
				error = "SOA Record内部包含Stencil Writer/Pop,SOA不能跨越Stencil Barrier,Record=" + record.name + ",Slot=" + slot;
				return false;
			}
			int compatibilityID = element.getSOACompatibilityID();
			if (!hasReferenceCompatibility)
			{
				hasReferenceCompatibility = true;
				referenceCompatibilityID = compatibilityID;
				referenceCompatibilityName = element.getSOACompatibilityName();
			}
			else if (referenceCompatibilityID != compatibilityID)
			{
				error = "相同SOA Slot类型不一致,Slot=" + slot + ",Record=" + record.name +
					",期望=" + referenceCompatibilityName + ",实际=" + element.getSOACompatibilityName();
				return false;
			}
			lane.Add(renderIndex);
		}
		return true;
	}
	private void collectImmediateNestedGroups(Transform root, List<FastSOARenderGroup> result)
	{
		if (root == null)
		{
			return;
		}
		if (root.TryGetComponent(out FastSOARenderGroup rootGroup) && isUsableGroup(rootGroup))
		{
			result.Add(rootGroup);
			return;
		}
		for (int i = 0; i < root.childCount; ++i)
		{
			collectImmediateNestedGroups(root.GetChild(i), result);
		}
	}
	private NestedBuildScratch acquireNestedBuildScratch(int depth)
	{
		while (mNestedBuildScratch.Count <= depth)
		{
			mNestedBuildScratch.Add(new NestedBuildScratch());
		}
		return mNestedBuildScratch[depth];
	}
	private void sortGroupsByRenderStart(List<FastSOARenderGroup> groups, FastUITransformRangeSystem transformRanges)
	{
		for (int i = 1; i < groups.Count; ++i)
		{
			FastSOARenderGroup value = groups[i];
			int valueStart = getGroupRegionStart(value, transformRanges);
			int j = i - 1;
			while (j >= 0 && getGroupRegionStart(groups[j], transformRanges) > valueStart)
			{
				groups[j + 1] = groups[j];
				--j;
			}
			groups[j + 1] = value;
		}
	}
	private int getGroupRegionStart(FastSOARenderGroup group, FastUITransformRangeSystem transformRanges)
	{
		if (tryGetGroupRegion(group, transformRanges, out int start, out _, out _))
		{
			return start;
		}
		return int.MaxValue;
	}
	private bool tryGetGroupRegion(FastSOARenderGroup group, FastUITransformRangeSystem transformRanges,
		out int regionStart, out int regionCount, out string error)
	{
		regionStart = -1;
		regionCount = 0;
		error = null;
		if (group == null)
		{
			error = "FastSOARenderGroup为空";
			return false;
		}
		RectTransform root = group.getRectTransform();
		int recordCount = root.childCount;
		if (recordCount == 0)
		{
			return true;
		}
		int expectedStart = -1;
		for (int recordIndex = 0; recordIndex < recordCount; ++recordIndex)
		{
			Transform record = root.GetChild(recordIndex);
			if (!transformRanges.tryGetRange(record, out int recordStart, out int renderCount) || renderCount <= 0)
			{
				error = "SOA直接子Record没有RenderElement,Group=" + group.name + ",Record=" + record.name;
				return false;
			}
			if (recordIndex == 0)
			{
				regionStart = recordStart;
				expectedStart = recordStart;
			}
			if (recordStart != expectedStart)
			{
				error = "SOA Record RenderRange不连续,Group=" + group.name + ",Record=" + record.name;
				return false;
			}
			expectedStart += renderCount;
		}
		regionCount = expectedStart - regionStart;
		return true;
	}
	private void resetBuildScratch()
	{
		for (int i = 0; i < mBuildLaneCount; ++i)
		{
			mBuildLanes[i].Clear();
		}
		mBuildLaneCount = 0;
		mBuildRecordCount = 0;
		mBuildGroups.Clear();
		mBuildGroupDepths.Clear();
		mBuildGroupSet.Clear();
		mBuildRenderIndexSet.Clear();
	}
	private List<int> acquireBuildLane()
	{
		List<int> lane;
		if (mBuildLaneCount < mBuildLanes.Count)
		{
			lane = mBuildLanes[mBuildLaneCount];
			lane.Clear();
		}
		else
		{
			lane = new List<int>(16);
			mBuildLanes.Add(lane);
		}
		++mBuildLaneCount;
		return lane;
	}
	private void addBuildGroup(FastSOARenderGroup group, int depth)
	{
		if (group == null || !mBuildGroupSet.Add(group))
		{
			return;
		}
		mBuildGroups.Add(group);
		mBuildGroupDepths.Add(depth);
	}
	private bool validateBuildProjection(int regionStart, int regionEnd, out string error)
	{
		error = null;
		mBuildRenderIndexSet.Clear();
		int memberCount = 0;
		for (int laneIndex = 0; laneIndex < mBuildLaneCount; ++laneIndex)
		{
			List<int> lane = mBuildLanes[laneIndex];
			for (int i = 0; i < lane.Count; ++i)
			{
				int renderIndex = lane[i];
				if (renderIndex < regionStart || renderIndex >= regionEnd)
				{
					error = "SOA投影成员超出顶层Group RenderRange,RenderIndex=" + renderIndex;
					return false;
				}
				if (!mBuildRenderIndexSet.Add(renderIndex))
				{
					error = "SOA投影成员重复,RenderIndex=" + renderIndex;
					return false;
				}
				++memberCount;
			}
		}
		if (memberCount != regionEnd - regionStart)
		{
			error = "SOA投影成员数量异常,期望=" + (regionEnd - regionStart) + ",实际=" + memberCount;
			return false;
		}
		return true;
	}
	private void commitBuildProjection(int writeStart)
	{
		int writeIndex = writeStart;
		var drawOrder = mDrawOrder.getValueColumn();
		var renderToLane = mRenderToLaneIndex.getValueColumn();
		var batchMaterialColumn = mBatchElementStates.getMaterialColumn();
		var batchTextureColumn = mBatchElementStates.getTextureColumn();
		for (int buildLaneIndex = 0; buildLaneIndex < mBuildLaneCount; ++buildLaneIndex)
		{
			List<int> lane = mBuildLanes[buildLaneIndex];
			int laneIndex = mLaneDrawStarts.Count;
			int laneDrawStart = writeIndex;
			mLaneMaterials.Clear();
			mLaneTextures.Clear();
			for (int i = 0; i < lane.Count; ++i)
			{
				int renderIndex = lane[i];
				Material material = batchMaterialColumn[renderIndex];
				Texture texture = batchTextureColumn[renderIndex];
				if (!containsBatchKey(material, texture))
				{
					mLaneMaterials.Add(material);
					mLaneTextures.Add(texture);
				}
			}
			for (int keyIndex = 0; keyIndex < mLaneMaterials.Count; ++keyIndex)
			{
				Material material = mLaneMaterials[keyIndex];
				Texture texture = mLaneTextures[keyIndex];
				for (int i = 0; i < lane.Count; ++i)
				{
					int renderIndex = lane[i];
					if (batchMaterialColumn[renderIndex] == material && batchTextureColumn[renderIndex] == texture)
					{
						drawOrder[writeIndex++] = renderIndex;
					}
				}
			}
			mLaneDrawStarts.Add(laneDrawStart);
			mLaneCounts.Add(lane.Count);
			mLaneBatchKeyCounts.Add(mLaneMaterials.Count);
			mLaneDirtyFlags.Add(false);
			for (int i = 0; i < lane.Count; ++i)
			{
				renderToLane[lane[i]] = laneIndex;
			}
			mBatchKeyGroupCount += mLaneMaterials.Count;
			++mLaneCount;
		}
	}
	private void commitBuildGroups()
	{
		for (int i = 0; i < mBuildGroups.Count; ++i)
		{
			FastSOARenderGroup group = mBuildGroups[i];
			int depth = mBuildGroupDepths[i];
			group.setBuildResult(true, null);
			if (!mConsumedGroups.Add(group))
			{
				continue;
			}
			++mConvertedGroupCount;
			if (depth > 0)
			{
				++mNestedGroupCount;
			}
			if (depth > mMaxNestedDepth)
			{
				mMaxNestedDepth = depth;
			}
		}
	}
	private void markGroupTreeInvalid(FastSOARenderGroup root, string error)
	{
		int invalidCount = 0;
		foreach (FastSOARenderGroup group in mGroups)
		{
			if (!isUsableGroup(group) || !isSameOrChildGroup(group, root))
			{
				continue;
			}
			group.setBuildResult(false, group == root ? error : "父SOA Group构建失败:" + error);
			++invalidCount;
		}
		mInvalidGroupCount += Mathf.Max(invalidCount, 1);
	}
	private bool isSameOrChildGroup(FastSOARenderGroup group, FastSOARenderGroup root)
	{
		if (group == root)
		{
			return true;
		}
		return group != null && root != null && group.transform.IsChildOf(root.transform);
	}
	private bool reorderLane(int laneIndex)
	{
		int drawStart = mLaneDrawStarts[laneIndex];
		int count = mLaneCounts[laneIndex];
		if (count <= 1)
		{
			return false;
		}
		mLaneMembers.Clear();
		mLaneMembers.EnsureCount(count);
		var laneMembers = mLaneMembers.getValueColumn();
		var drawOrder = mDrawOrder.getValueColumn();
		var batchMaterialColumn = mBatchElementStates.getMaterialColumn();
		var batchTextureColumn = mBatchElementStates.getTextureColumn();
		for (int i = 0; i < count; ++i)
		{
			laneMembers[i] = drawOrder[drawStart + i];
		}
		mLaneMaterials.Clear();
		mLaneTextures.Clear();
		for (int i = 0; i < count; ++i)
		{
			int renderIndex = laneMembers[i];
			Material material = batchMaterialColumn[renderIndex];
			Texture texture = batchTextureColumn[renderIndex];
			if (!containsBatchKey(material, texture))
			{
				mLaneMaterials.Add(material);
				mLaneTextures.Add(texture);
			}
		}
		bool changed = false;
		int writeIndex = drawStart;
		for (int keyIndex = 0; keyIndex < mLaneMaterials.Count; ++keyIndex)
		{
			Material material = mLaneMaterials[keyIndex];
			Texture texture = mLaneTextures[keyIndex];
			for (int i = 0; i < count; ++i)
			{
				int renderIndex = laneMembers[i];
				if (batchMaterialColumn[renderIndex] != material || batchTextureColumn[renderIndex] != texture)
				{
					continue;
				}
				if (drawOrder[writeIndex] != renderIndex)
				{
					changed = true;
				}
				drawOrder[writeIndex++] = renderIndex;
			}
		}
		var laneBatchKeyCounts = mLaneBatchKeyCounts.getValueColumn();
		int oldKeyCount = laneBatchKeyCounts[laneIndex];
		int newKeyCount = mLaneMaterials.Count;
		laneBatchKeyCounts[laneIndex] = newKeyCount;
		mBatchKeyGroupCount += newKeyCount - oldKeyCount;
		return changed;
	}
	private bool containsBatchKey(Material material, Texture texture)
	{
		for (int i = 0; i < mLaneMaterials.Count; ++i)
		{
			if (mLaneMaterials[i] == material && mLaneTextures[i] == texture)
			{
				return true;
			}
		}
		return false;
	}
	private void prepareIdentityOrder(int count)
	{
		mDrawOrder.Clear();
		mDrawOrder.EnsureCount(count);
		var order = mDrawOrder.getValueColumn();
		for (int i = 0; i < count; ++i)
		{
			order[i] = i;
		}
	}
	private void prepareRenderToLaneIndex(int count)
	{
		mRenderToLaneIndex.Clear();
		mRenderToLaneIndex.EnsureCount(count);
		var laneIndexColumn = mRenderToLaneIndex.getValueColumn();
		for (int i = 0; i < count; ++i)
		{
			laneIndexColumn[i] = -1;
		}
	}
	private void refreshDrawOrderReordered(int renderCount)
	{
		mDrawOrderReordered = false;
		var order = mDrawOrder.getValueColumn();
		for (int i = 0; i < renderCount; ++i)
		{
			if (order[i] != i)
			{
				mDrawOrderReordered = true;
				return;
			}
		}
	}
	private void writeStats(ref FastUIFrameStats stats)
	{
		stats.mSOAGroupCount = mGroups.Count;
	}
}
