using System;
using System.Collections.Generic;
using EasyECS;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

// Incremental depth sorter for SortGroups, with dense-change parallel fallback.
internal sealed class FastSpriteChunkedSortManager
{
	internal struct FrameStats
	{
		public int mDirtyCount;
		public int mNoMoveCount;
		public int mLocalRepairCount;
		public int mAdjacentChunkMoveCount;
		public int mGlobalRelocateCount;
		public int mMovedSlots;
		public int mBatchRepairLayerCount;
		public int mBatchRepairNodeCount;
		public int mParallelSortNodeCount;
		public int mParallelSortRunCount;
		public int mParallelSortDispatchCount;
	}

	private const int TARGET_CHUNK_SIZE = 64;
	private const int MAX_CHUNK_SIZE = 96;
	private const int MIN_CHUNK_SIZE = 24;
	private const int BATCH_REPAIR_MIN_CHANGED = 24;
	private const int BATCH_REPAIR_RATIO_NUMERATOR = 1;
	private const int BATCH_REPAIR_RATIO_DENOMINATOR = 4;
	private const int PARALLEL_SORT_MIN_CHANGED = 768;
	// Balances cache locality against merge-run count for dense repairs.
	private const int PARALLEL_SORT_CHUNK_SIZE = 576;
	private const int PARALLEL_SORT_INSERTION_THRESHOLD = 16;

	private sealed class Node
	{
		public FastSpriteSortGroup mGroup;
		public LayerState mLayer;
		public Chunk mChunk;
		public int mSlot;
		public long mStableID;
		public int mCachedLayerID;
		public int mCachedOrder;
		public bool mDirtyQueued;
		public bool mBatchOrderChanged;
		public int mBatchOldRank;
	}

	private sealed class Chunk
	{
		public readonly List<Node> mNodes = new(TARGET_CHUNK_SIZE + 8);
		public int mIndex;
	}

	private sealed class LayerState
	{
		public int mLayerID;
		public readonly List<Chunk> mChunks = new(8);
	}

	private readonly FastDictionary<FastSpriteSortGroup, Node> mNodes = new();
	private readonly FastDictionary<int, LayerState> mLayers = new();
	private readonly List<Node> mDirtyNodes = new(64);
	private readonly List<LayerState> mLayerScratch = new(8);
	private readonly List<Node> mBatchStableScratch = new(512);
	private readonly List<Node> mBatchDirtyScratch = new(256);
	private readonly List<Node> mBatchMergeScratch = new(512);
	private readonly FastSpriteDepthSortKeyData_ECSList mParallelSortKeys = new(1024);
	private readonly Int_ECSList mParallelSortIndices = new(1024);
	private readonly Int_ECSList mParallelMergeIndices = new(1024);
	private Int_ECSList mParallelSortedIndices;
	private int mParallelSortRunCountCurrent;
	private long mStableSequence;
	private int mRevision;
	private FrameStats mLastStats;

	[BurstCompile(OptimizeFor = OptimizeFor.Performance)]
	private unsafe struct ParallelDirtyIndexSortChunkJob : IJobParallelFor
	{
		[NativeDisableUnsafePtrRestriction] public FastSpriteDepthSortKeyData_ECSList.BurstView mKeys;
		[NativeDisableUnsafePtrRestriction] public Int_ECSList.BurstView mIndices;
		public int mChunkSize;

		public readonly void Execute(int chunkIndex)
		{
			mIndices.GetChunkRange(chunkIndex, mChunkSize, out int start, out int count);
			if (count <= 1)
			{
				return;
			}
			quickSortRange(start, start + count - 1);
		}

		private readonly void quickSortRange(int first, int last)
		{
			// Iterative quicksort with tail processing of the larger partition. Only the
			// 4-byte permutation moves; key columns remain read-only and cache resident.
			int* stack = stackalloc int[64];
			int stackCount = 0;
			stack[stackCount++] = first;
			stack[stackCount++] = last;
			while (stackCount > 0)
			{
				int hi = stack[--stackCount];
				int lo = stack[--stackCount];
				while (hi - lo + 1 > PARALLEL_SORT_INSERTION_THRESHOLD)
				{
					int middle = lo + ((hi - lo) >> 1);
					int pivotIndex = medianKeyIndex(mIndices.mValue[lo], mIndices.mValue[middle], mIndices.mValue[hi]);
					int left = lo;
					int right = hi;
					while (left <= right)
					{
						while (left <= hi && compareKeyIndex(mIndices.mValue[left], pivotIndex) < 0)
						{
							++left;
						}
						while (right >= lo && compareKeyIndex(mIndices.mValue[right], pivotIndex) > 0)
						{
							--right;
						}
						if (left <= right)
						{
							swapIndex(left, right);
							++left;
							--right;
						}
					}

					int leftSize = right - lo + 1;
					int rightSize = hi - left + 1;
					// Push the smaller side and keep iterating the larger side so stack depth
					// stays logarithmic even for strongly unbalanced partitions.
					if (leftSize < rightSize)
					{
						if (lo < right)
						{
							stack[stackCount++] = lo;
							stack[stackCount++] = right;
						}
						lo = left;
					}
					else
					{
						if (left < hi)
						{
							stack[stackCount++] = left;
							stack[stackCount++] = hi;
						}
						hi = right;
					}
				}
				insertionSort(lo, hi);
			}
		}

		private readonly void insertionSort(int first, int last)
		{
			for (int i = first + 1; i <= last; ++i)
			{
				int value = mIndices.mValue[i];
				int j = i - 1;
				while (j >= first && compareKeyIndex(mIndices.mValue[j], value) > 0)
				{
					mIndices.mValue[j + 1] = mIndices.mValue[j];
					--j;
				}
				mIndices.mValue[j + 1] = value;
			}
		}

		private readonly int medianKeyIndex(int a, int b, int c)
		{
			if (compareKeyIndex(a, b) > 0)
			{
				(b, a) = (a, b);
			}
			if (compareKeyIndex(b, c) > 0)
			{
				b = c;
			}
			if (compareKeyIndex(a, b) > 0)
			{
				b = a;
			}
			return b;
		}

		private readonly int compareKeyIndex(int leftIndex, int rightIndex)
		{
			int leftOrder = mKeys.mOrder[leftIndex];
			int rightOrder = mKeys.mOrder[rightIndex];
			if (leftOrder != rightOrder)
			{
				return leftOrder < rightOrder ? -1 : 1;
			}
			long leftStableID = mKeys.mStableID[leftIndex];
			long rightStableID = mKeys.mStableID[rightIndex];
			return leftStableID < rightStableID ? -1 : leftStableID > rightStableID ? 1 : 0;
		}

		private readonly void swapIndex(int left, int right)
		{
			if (left == right)
			{
				return;
			}
			(mIndices.mValue[right], mIndices.mValue[left]) = (mIndices.mValue[left], mIndices.mValue[right]);
		}
	}

	public int getCount()
	{
		return mNodes.Count;
	}
	public int getRevision()
	{
		return mRevision;
	}
	public FrameStats getLastStats()
	{
		return mLastStats;
	}

	public void register(FastSpriteSortGroup group)
	{
		if (group == null || mNodes.ContainsKey(group))
		{
			return;
		}
		Node node = new()
		{
			mGroup = group,
			mStableID = ++mStableSequence,
			mCachedLayerID = group.getSortingLayerID(),
			mCachedOrder = group.getSortingOrder(),
			mSlot = -1,
		};
		mNodes.Add(group, node);
		LayerState layer = getOrCreateLayer(node.mCachedLayerID);
		insertGlobal(layer, node);
		if (node.mChunk != null && node.mChunk.mNodes.Count > MAX_CHUNK_SIZE)
		{
			rebalanceLayer(layer);
		}
		++mRevision;
	}

	public void unregister(FastSpriteSortGroup group)
	{
		if (group == null || !mNodes.TryGetValue(group, out Node node))
		{
			return;
		}
		LayerState layer = node.mLayer;
		Chunk chunk = node.mChunk;
		removeNode(node);
		mNodes.Remove(group);
		if (layer != null)
		{
			if (chunk != null && chunk.mNodes.Count < MIN_CHUNK_SIZE && layer.mChunks.Count > 1)
			{
				rebalanceLayer(layer);
			}
			removeLayerIfEmpty(layer);
		}
		++mRevision;
	}

	public void markDirty(FastSpriteSortGroup group)
	{
		if (group == null || !mNodes.TryGetValue(group, out Node node) || node.mDirtyQueued)
		{
			return;
		}
		node.mDirtyQueued = true;
		mDirtyNodes.Add(node);
	}

	public bool update()
	{
		mLastStats = default;
		if (mDirtyNodes.Count == 0)
		{
			return false;
		}

		if (tryBatchRepairSingleLayer())
		{
			mDirtyNodes.Clear();
			++mRevision;
			return true;
		}

		bool anyKeyChanged = false;
		for (int i = 0; i < mDirtyNodes.Count; ++i)
		{
			Node node = mDirtyNodes[i];
			if (node == null || node.mGroup == null || !mNodes.ContainsKey(node.mGroup))
			{
				continue;
			}
			node.mDirtyQueued = false;
			++mLastStats.mDirtyCount;

			int newLayerID = node.mGroup.getSortingLayerID();
			int newOrder = node.mGroup.getSortingOrder();
			if (newLayerID == node.mCachedLayerID && newOrder == node.mCachedOrder)
			{
				++mLastStats.mNoMoveCount;
				continue;
			}
			anyKeyChanged = true;

			if (newLayerID != node.mCachedLayerID)
			{
				LayerState oldLayer = node.mLayer;
				Chunk previousChunk = node.mChunk;
				removeNode(node);
				node.mCachedLayerID = newLayerID;
				node.mCachedOrder = newOrder;
				LayerState newLayer = getOrCreateLayer(newLayerID);
				insertGlobal(newLayer, node);
				if (node.mChunk != null && node.mChunk.mNodes.Count > MAX_CHUNK_SIZE)
				{
					rebalanceLayer(newLayer);
				}
				if (oldLayer != null)
				{
					if (previousChunk != null && previousChunk.mNodes.Count < MIN_CHUNK_SIZE && oldLayer.mChunks.Count > 1)
					{
						rebalanceLayer(oldLayer);
					}
					removeLayerIfEmpty(oldLayer);
				}
				++mLastStats.mGlobalRelocateCount;
				continue;
			}

			int oldRank = getGlobalRank(node);
			node.mCachedOrder = newOrder;
			if (isOrderedAgainstNeighbours(node))
			{
				++mLastStats.mNoMoveCount;
				continue;
			}

			// Hot path: shift only the crossed entries inside the current 64-entry chunk.
			if (repairInsideChunk(node) && isOrderedAgainstNeighbours(node))
			{
				mLastStats.mMovedSlots += Math.Abs(getGlobalRank(node) - oldRank);
				++mLastStats.mLocalRepairCount;
				continue;
			}

			LayerState layer = node.mLayer;
			Chunk oldChunk = node.mChunk;
			int oldChunkIndex = oldChunk != null ? oldChunk.mIndex : 0;
			removeFromChunk(node, oldChunk);
			if (oldChunk != null && oldChunk.mNodes.Count == 0 && layer.mChunks.Count > 1)
			{
				layer.mChunks.RemoveAt(oldChunk.mIndex);
				assignChunkIndices(layer, oldChunkIndex);
				oldChunkIndex = Mathf.Clamp(oldChunkIndex, 0, layer.mChunks.Count - 1);
			}

			Chunk destination = chooseDestinationChunk(layer, oldChunkIndex, node, out int path);
			insertIntoChunkSorted(destination, node);
			bool needsRebalance = destination.mNodes.Count > MAX_CHUNK_SIZE ||
				(oldChunk != null && oldChunk.mNodes.Count < MIN_CHUNK_SIZE && layer.mChunks.Count > 1);
			if (needsRebalance)
			{
				rebalanceLayer(layer);
			}

			mLastStats.mMovedSlots += Math.Abs(getGlobalRank(node) - oldRank);
			if (path == 1)
			{
				++mLastStats.mAdjacentChunkMoveCount;
			}
			else
			{
				++mLastStats.mGlobalRelocateCount;
			}
		}
		mDirtyNodes.Clear();
		if (anyKeyChanged)
		{
			++mRevision;
		}
		return anyKeyChanged;
	}

	private bool tryBatchRepairSingleLayer()
	{
		LayerState batchLayer = null;
		int validDirtyCount = 0;
		int changedCount = 0;

		// Probe only. Do not mutate cached keys until we know the batch path is valid.
		for (int i = 0; i < mDirtyNodes.Count; ++i)
		{
			Node node = mDirtyNodes[i];
			if (node == null || node.mGroup == null || !mNodes.ContainsKey(node.mGroup))
			{
				continue;
			}
			++validDirtyCount;

			int newLayerID = node.mGroup.getSortingLayerID();
			if (newLayerID != node.mCachedLayerID)
			{
				return false;
			}
			int newOrder = node.mGroup.getSortingOrder();
			if (newOrder == node.mCachedOrder)
			{
				continue;
			}
			++changedCount;
			if (batchLayer == null)
			{
				batchLayer = node.mLayer;
			}
			else if (!ReferenceEquals(batchLayer, node.mLayer))
			{
				return false;
			}
		}

		if (batchLayer == null || changedCount < BATCH_REPAIR_MIN_CHANGED)
		{
			return false;
		}
		int layerCount = getLayerNodeCount(batchLayer);
		if (layerCount <= 0 || changedCount * BATCH_REPAIR_RATIO_DENOMINATOR < layerCount * BATCH_REPAIR_RATIO_NUMERATOR)
		{
			return false;
		}

		mLastStats.mDirtyCount = validDirtyCount;
		for (int i = 0; i < mDirtyNodes.Count; ++i)
		{
			Node node = mDirtyNodes[i];
			if (node == null || node.mGroup == null || !mNodes.ContainsKey(node.mGroup))
			{
				continue;
			}
			node.mDirtyQueued = false;
			int newOrder = node.mGroup.getSortingOrder();
			if (newOrder == node.mCachedOrder)
			{
				++mLastStats.mNoMoveCount;
				continue;
			}
			node.mCachedOrder = newOrder;
			node.mBatchOrderChanged = true;
		}

		mBatchStableScratch.Clear();
		mBatchDirtyScratch.Clear();
		mBatchMergeScratch.Clear();
		int oldRank = 0;
		for (int chunkIndex = 0; chunkIndex < batchLayer.mChunks.Count; ++chunkIndex)
		{
			Chunk chunk = batchLayer.mChunks[chunkIndex];
			for (int slot = 0; slot < chunk.mNodes.Count; ++slot)
			{
				Node node = chunk.mNodes[slot];
				node.mBatchOldRank = oldRank++;
				if (node.mBatchOrderChanged)
				{
					mBatchDirtyScratch.Add(node);
				}
				else
				{
					mBatchStableScratch.Add(node);
				}
			}
		}

		if (!tryParallelSortDirtySubset())
		{
			mBatchDirtyScratch.Sort(compare);
			mergeSerialDirtyWithStable();
		}
		else
		{
			mergeParallelDirtyWithStable();
		}

		int mergedRank = 0;
		for (int chunkIndex = 0; chunkIndex < batchLayer.mChunks.Count; ++chunkIndex)
		{
			Chunk chunk = batchLayer.mChunks[chunkIndex];
			chunk.mIndex = chunkIndex;
			int count = chunk.mNodes.Count;
			for (int slot = 0; slot < count; ++slot, ++mergedRank)
			{
				Node node = mBatchMergeScratch[mergedRank];
				chunk.mNodes[slot] = node;
				node.mLayer = batchLayer;
				node.mChunk = chunk;
				node.mSlot = slot;
				if (node.mBatchOrderChanged)
				{
					int moved = Math.Abs(mergedRank - node.mBatchOldRank);
					mLastStats.mMovedSlots += moved;
					if (moved == 0)
					{
						++mLastStats.mNoMoveCount;
					}
					else
					{
						++mLastStats.mLocalRepairCount;
					}
					node.mBatchOrderChanged = false;
				}
			}
		}

		mLastStats.mBatchRepairLayerCount = 1;
		mLastStats.mBatchRepairNodeCount = changedCount;
		mBatchStableScratch.Clear();
		mBatchDirtyScratch.Clear();
		mBatchMergeScratch.Clear();
		return true;
	}

	private bool tryParallelSortDirtySubset()
	{
		int dirtyCount = mBatchDirtyScratch.Count;
		mParallelSortRunCountCurrent = 0;
		if (dirtyCount < PARALLEL_SORT_MIN_CHANGED ||
			!BurstCompiler.IsEnabled ||
			!FastSpriteDepthSortKeyData_ECSList.IsUnsafeBackend ||
			!Int_ECSList.IsUnsafeBackend)
		{
			return false;
		}

		mParallelSortKeys.CompleteBurstJobs();
		mParallelSortIndices.CompleteBurstJobs();
		resizeParallelSortKeyCount(dirtyCount);
		resizeParallelIndexCount(mParallelSortIndices, dirtyCount);

		var orders = mParallelSortKeys.getOrderColumn();
		var stableIDs = mParallelSortKeys.getStableIDColumn();
		var indices = mParallelSortIndices.getValueColumn();
		for (int i = 0; i < dirtyCount; ++i)
		{
			Node node = mBatchDirtyScratch[i];
			orders[i] = node.mCachedOrder;
			stableIDs[i] = node.mStableID;
			indices[i] = i;
		}

		ParallelDirtyIndexSortChunkJob job = new()
		{
			mKeys = mParallelSortKeys.GetBurstView(),
			mIndices = mParallelSortIndices.GetBurstView(),
			mChunkSize = PARALLEL_SORT_CHUNK_SIZE,
		};
		JobHandle handle = mParallelSortIndices.ScheduleBurstChunk(job, PARALLEL_SORT_CHUNK_SIZE);
		mParallelSortKeys.RegisterBurstJob(handle);
		mParallelSortIndices.CompleteBurstJobs();
		mParallelSortKeys.CompleteBurstJobs();

		int runCount = (dirtyCount + PARALLEL_SORT_CHUNK_SIZE - 1) / PARALLEL_SORT_CHUNK_SIZE;
		mParallelSortRunCountCurrent = runCount;
		mParallelSortedIndices = runCount <= 2 ? null : collapseParallelSortedIndexRuns(dirtyCount);

		mLastStats.mParallelSortNodeCount = dirtyCount;
		mLastStats.mParallelSortRunCount = runCount;
		mLastStats.mParallelSortDispatchCount = 1;
		return true;
	}

	private void mergeSerialDirtyWithStable()
	{
		int stableIndex = 0;
		int dirtyIndex = 0;
		while (stableIndex < mBatchStableScratch.Count || dirtyIndex < mBatchDirtyScratch.Count)
		{
			if (stableIndex >= mBatchStableScratch.Count)
			{
				mBatchMergeScratch.Add(mBatchDirtyScratch[dirtyIndex++]);
			}
			else if (dirtyIndex >= mBatchDirtyScratch.Count)
			{
				mBatchMergeScratch.Add(mBatchStableScratch[stableIndex++]);
			}
			else if (compare(mBatchDirtyScratch[dirtyIndex], mBatchStableScratch[stableIndex]) < 0)
			{
				mBatchMergeScratch.Add(mBatchDirtyScratch[dirtyIndex++]);
			}
			else
			{
				mBatchMergeScratch.Add(mBatchStableScratch[stableIndex++]);
			}
		}
	}

	private Int_ECSList collapseParallelSortedIndexRuns(int dirtyCount)
	{
		if (dirtyCount <= PARALLEL_SORT_CHUNK_SIZE)
		{
			return mParallelSortIndices;
		}

		resizeParallelIndexCount(mParallelMergeIndices, dirtyCount);
		Int_ECSList source = mParallelSortIndices;
		Int_ECSList destination = mParallelMergeIndices;
		var orders = mParallelSortKeys.getOrderColumn();
		var stableIDs = mParallelSortKeys.getStableIDColumn();
		int runWidth = PARALLEL_SORT_CHUNK_SIZE;
		while (runWidth < dirtyCount)
		{
			var sourceIndices = source.getValueColumn();
			var destinationIndices = destination.getValueColumn();
			int pairWidth = runWidth << 1;
			for (int leftStart = 0; leftStart < dirtyCount; leftStart += pairWidth)
			{
				int middle = Math.Min(leftStart + runWidth, dirtyCount);
				int rightEnd = Math.Min(leftStart + pairWidth, dirtyCount);
				if (middle >= rightEnd)
				{
					for (int copy = leftStart; copy < rightEnd; ++copy)
					{
						destinationIndices[copy] = sourceIndices[copy];
					}
					continue;
				}

				// If both adjacent runs already form one ordered range, avoid key-by-key
				// merging and stream-copy the permutation only.
				int leftLastIndex = sourceIndices[middle - 1];
				int rightFirstIndex = sourceIndices[middle];
				if (compareParallelKeyValues(
					orders[leftLastIndex], stableIDs[leftLastIndex],
					orders[rightFirstIndex], stableIDs[rightFirstIndex]) <= 0)
				{
					for (int copy = leftStart; copy < rightEnd; ++copy)
					{
						destinationIndices[copy] = sourceIndices[copy];
					}
					continue;
				}

				int left = leftStart;
				int right = middle;
				int output = leftStart;
				while (left < middle && right < rightEnd)
				{
					int leftIndex = sourceIndices[left];
					int rightIndex = sourceIndices[right];
					if (compareParallelKeyValues(
						orders[leftIndex], stableIDs[leftIndex],
						orders[rightIndex], stableIDs[rightIndex]) <= 0)
					{
						destinationIndices[output++] = leftIndex;
						++left;
					}
					else
					{
						destinationIndices[output++] = rightIndex;
						++right;
					}
				}
				while (left < middle)
				{
					destinationIndices[output++] = sourceIndices[left++];
				}
				while (right < rightEnd)
				{
					destinationIndices[output++] = sourceIndices[right++];
				}
			}

			(destination, source) = (source, destination);
			if (runWidth > (dirtyCount >> 1))
			{
				break;
			}
			runWidth <<= 1;
		}
		return source;
	}

	private void mergeParallelDirtyWithStable()
	{
		if (mParallelSortRunCountCurrent == 2)
		{
			mergeTwoParallelDirtyRunsWithStable();
			return;
		}
		mergeSortedParallelPermutationWithStable(mParallelSortedIndices ?? mParallelSortIndices);
	}

	private void mergeTwoParallelDirtyRunsWithStable()
	{
		int dirtyCount = mBatchDirtyScratch.Count;
		int rightStart = Math.Min(PARALLEL_SORT_CHUNK_SIZE, dirtyCount);
		if (rightStart >= dirtyCount)
		{
			mergeSortedParallelPermutationWithStable(mParallelSortIndices);
			return;
		}

		var permutation = mParallelSortIndices.getValueColumn();
		var orders = mParallelSortKeys.getOrderColumn();
		var stableIDs = mParallelSortKeys.getStableIDColumn();

		// If both cache-local runs are already globally ordered, their contiguous
		// permutation storage is itself the final dirty order. This is a zero-copy
		// collapse and falls through to the ordinary two-stream stable merge.
		int leftLast = permutation[rightStart - 1];
		int rightFirst = permutation[rightStart];
		if (compareParallelKeyValues(
			orders[leftLast], stableIDs[leftLast],
			orders[rightFirst], stableIDs[rightFirst]) <= 0)
		{
			mergeSortedParallelPermutationWithStable(mParallelSortIndices);
			return;
		}

		int leftPosition = 0;
		int rightPosition = rightStart;
		int stablePosition = 0;
		while (stablePosition < mBatchStableScratch.Count ||
			leftPosition < rightStart || rightPosition < dirtyCount)
		{
			if (leftPosition >= rightStart && rightPosition >= dirtyCount)
			{
				mBatchMergeScratch.Add(mBatchStableScratch[stablePosition++]);
				continue;
			}

			bool takeLeftRun;
			int dirtyIndex;
			if (leftPosition >= rightStart)
			{
				takeLeftRun = false;
				dirtyIndex = permutation[rightPosition];
			}
			else if (rightPosition >= dirtyCount)
			{
				takeLeftRun = true;
				dirtyIndex = permutation[leftPosition];
			}
			else
			{
				int leftIndex = permutation[leftPosition];
				int rightIndex = permutation[rightPosition];
				takeLeftRun = compareParallelKeyValues(
					orders[leftIndex], stableIDs[leftIndex],
					orders[rightIndex], stableIDs[rightIndex]) <= 0;
				dirtyIndex = takeLeftRun ? leftIndex : rightIndex;
			}

			if (stablePosition < mBatchStableScratch.Count)
			{
				Node stable = mBatchStableScratch[stablePosition];
				if (compareParallelKeyValues(
					orders[dirtyIndex], stableIDs[dirtyIndex],
					stable.mCachedOrder, stable.mStableID) >= 0)
				{
					mBatchMergeScratch.Add(stable);
					++stablePosition;
					continue;
				}
			}

			mBatchMergeScratch.Add(mBatchDirtyScratch[dirtyIndex]);
			if (takeLeftRun)
			{
				++leftPosition;
			}
			else
			{
				++rightPosition;
			}
		}
	}

	private void mergeSortedParallelPermutationWithStable(Int_ECSList sortedIndices)
	{
		var permutation = sortedIndices.getValueColumn();
		var orders = mParallelSortKeys.getOrderColumn();
		var stableIDs = mParallelSortKeys.getStableIDColumn();
		int dirtyCount = mBatchDirtyScratch.Count;

		int stableIndex = 0;
		int dirtyPosition = 0;
		while (stableIndex < mBatchStableScratch.Count || dirtyPosition < dirtyCount)
		{
			if (stableIndex >= mBatchStableScratch.Count)
			{
				mBatchMergeScratch.Add(mBatchDirtyScratch[permutation[dirtyPosition++]]);
				continue;
			}
			if (dirtyPosition >= dirtyCount)
			{
				mBatchMergeScratch.Add(mBatchStableScratch[stableIndex++]);
				continue;
			}

			int dirtyIndex = permutation[dirtyPosition];
			Node stable = mBatchStableScratch[stableIndex];
			if (compareParallelKeyValues(
				orders[dirtyIndex], stableIDs[dirtyIndex], stable.mCachedOrder, stable.mStableID) < 0)
			{
				mBatchMergeScratch.Add(mBatchDirtyScratch[dirtyIndex]);
				++dirtyPosition;
			}
			else
			{
				mBatchMergeScratch.Add(stable);
				++stableIndex;
			}
		}
	}

	private static int compareParallelKeyValues(int leftOrder, long leftStableID, int rightOrder, long rightStableID)
	{
		if (leftOrder != rightOrder)
		{
			return leftOrder < rightOrder ? -1 : 1;
		}
		return leftStableID < rightStableID ? -1 : leftStableID > rightStableID ? 1 : 0;
	}

	private void resizeParallelSortKeyCount(int count)
	{
		if (mParallelSortKeys.Count < count)
		{
			mParallelSortKeys.EnsureCount(count);
		}
		else if (mParallelSortKeys.Count > count)
		{
			mParallelSortKeys.RemoveRange(count, mParallelSortKeys.Count - count);
		}
	}

	private static void resizeParallelIndexCount(Int_ECSList list, int count)
	{
		if (list.Count < count)
		{
			list.EnsureCount(count, 0);
		}
		else if (list.Count > count)
		{
			list.RemoveRange(count, list.Count - count);
		}
	}

	private static int getLayerNodeCount(LayerState layer)
	{
		if (layer == null)
		{
			return 0;
		}
		int count = 0;
		for (int i = 0; i < layer.mChunks.Count; ++i)
		{
			count += layer.mChunks[i].mNodes.Count;
		}
		return count;
	}

	public int compareGroups(FastSpriteSortGroup left, FastSpriteSortGroup right)
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
		if (mNodes.TryGetValue(left, out Node leftNode) && mNodes.TryGetValue(right, out Node rightNode))
		{
			int cachedLeftLayer = SortingLayer.GetLayerValueFromID(leftNode.mCachedLayerID);
			int cachedRightLayer = SortingLayer.GetLayerValueFromID(rightNode.mCachedLayerID);
			if (cachedLeftLayer != cachedRightLayer)
			{
				return cachedLeftLayer < cachedRightLayer ? -1 : 1;
			}
			return compare(leftNode, rightNode);
		}
		int leftLayer = SortingLayer.GetLayerValueFromID(left.getSortingLayerID());
		int rightLayer = SortingLayer.GetLayerValueFromID(right.getSortingLayerID());
		if (leftLayer != rightLayer)
		{
			return leftLayer < rightLayer ? -1 : 1;
		}
		if (left.getSortingOrder() != right.getSortingOrder())
		{
			return left.getSortingOrder() < right.getSortingOrder() ? -1 : 1;
		}
		return left.GetInstanceID().CompareTo(right.GetInstanceID());
	}

	public bool tryGetOrderRank(FastSpriteSortGroup group, out int layerValue, out int rank)
	{
		layerValue = 0;
		rank = 0;
		if (group == null || !mNodes.TryGetValue(group, out Node node) || node == null || node.mLayer == null || node.mChunk == null)
		{
			return false;
		}
		layerValue = SortingLayer.GetLayerValueFromID(node.mCachedLayerID);
		rank = getGlobalRank(node);
		return true;
	}

	public void appendOrderedGroups(List<FastSpriteSortGroup> output)
	{
		if (output == null)
		{
			return;
		}
		mLayerScratch.Clear();
		foreach (LayerState layer in mLayers.Values)
		{
			mLayerScratch.Add(layer);
		}
		mLayerScratch.Sort(compareLayer);
		for (int layerIndex = 0; layerIndex < mLayerScratch.Count; ++layerIndex)
		{
			LayerState layer = mLayerScratch[layerIndex];
			for (int chunkIndex = 0; chunkIndex < layer.mChunks.Count; ++chunkIndex)
			{
				Chunk chunk = layer.mChunks[chunkIndex];
				for (int i = 0; i < chunk.mNodes.Count; ++i)
				{
					FastSpriteSortGroup group = chunk.mNodes[i].mGroup;
					if (group != null && group.isRuntimeActive())
					{
						output.Add(group);
					}
				}
			}
		}
	}

	public void clear()
	{
		mDirtyNodes.Clear();
		mNodes.Clear();
		mLayers.Clear();
		mLayerScratch.Clear();
		mBatchStableScratch.Clear();
		mBatchDirtyScratch.Clear();
		mBatchMergeScratch.Clear();
		mParallelSortKeys.Clear();
		mParallelSortIndices.Clear();
		mParallelMergeIndices.Clear();
		mParallelSortedIndices = null;
		mLastStats = default;
		mStableSequence = 0;
		++mRevision;
	}

	public void Dispose()
	{
		clear();
		mParallelSortKeys.Dispose();
		mParallelSortIndices.Dispose();
		mParallelMergeIndices.Dispose();
	}

	private static int compareLayer(LayerState a, LayerState b)
	{
		int valueA = SortingLayer.GetLayerValueFromID(a.mLayerID);
		int valueB = SortingLayer.GetLayerValueFromID(b.mLayerID);
		if (valueA != valueB)
		{
			return valueA < valueB ? -1 : 1;
		}
		return a.mLayerID.CompareTo(b.mLayerID);
	}

	private LayerState getOrCreateLayer(int layerID)
	{
		if (!mLayers.TryGetValue(layerID, out LayerState layer))
		{
			layer = new LayerState
			{
				mLayerID = layerID
			};
			layer.mChunks.Add(new Chunk
			{
				mIndex = 0
			});
			mLayers.Add(layerID, layer);
		}
		return layer;
	}

	private void removeLayerIfEmpty(LayerState layer)
	{
		if (layer == null)
		{
			return;
		}
		for (int i = 0; i < layer.mChunks.Count; ++i)
		{
			if (layer.mChunks[i].mNodes.Count > 0)
			{
				return;
			}
		}
		mLayers.Remove(layer.mLayerID);
	}

	private void insertGlobal(LayerState layer, Node node)
	{
		node.mLayer = layer;
		Chunk chunk = findChunkForKey(layer, node);
		insertIntoChunkSorted(chunk, node);
	}

	private Chunk findChunkForKey(LayerState layer, Node node)
	{
		if (layer.mChunks.Count == 0)
		{
			Chunk created = new()
			{
				mIndex = 0
			};
			layer.mChunks.Add(created);
			return created;
		}
		int low = 0;
		int high = layer.mChunks.Count - 1;
		while (low < high)
		{
			int mid = (low + high) >> 1;
			Chunk chunk = layer.mChunks[mid];
			if (chunk.mNodes.Count == 0 || compare(node, chunk.mNodes[^1]) <= 0)
			{
				high = mid;
			}
			else
			{
				low = mid + 1;
			}
		}
		return layer.mChunks[low];
	}

	private Chunk chooseDestinationChunk(LayerState layer, int oldChunkIndex, Node node, out int path)
	{
		oldChunkIndex = Mathf.Clamp(oldChunkIndex, 0, layer.mChunks.Count - 1);
		Chunk current = layer.mChunks[oldChunkIndex];
		if (fitsChunk(current, node))
		{
			path = 1;
			return current;
		}
		if (oldChunkIndex > 0)
		{
			Chunk previous = layer.mChunks[oldChunkIndex - 1];
			if (fitsChunk(previous, node))
			{
				path = 1;
				return previous;
			}
		}
		if (oldChunkIndex + 1 < layer.mChunks.Count)
		{
			Chunk next = layer.mChunks[oldChunkIndex + 1];
			if (fitsChunk(next, node))
			{
				path = 1;
				return next;
			}
		}
		path = 2;
		return findChunkForKey(layer, node);
	}

	private static bool fitsChunk(Chunk chunk, Node node)
	{
		if (chunk == null || chunk.mNodes.Count == 0)
		{
			return true;
		}
		return compare(node, chunk.mNodes[0]) >= 0 && compare(node, chunk.mNodes[^1]) <= 0;
	}

	private static bool repairInsideChunk(Node node)
	{
		Chunk chunk = node?.mChunk;
		if (chunk == null || chunk.mNodes.Count <= 1)
		{
			return true;
		}
		int oldSlot = node.mSlot;
		int target = oldSlot;
		while (target > 0 && compare(chunk.mNodes[target - 1], node) > 0)
		{
			--target;
		}
		if (target == oldSlot)
		{
			while (target + 1 < chunk.mNodes.Count && compare(node, chunk.mNodes[target + 1]) > 0)
			{
				++target;
			}
		}
		if (target == oldSlot)
		{
			return true;
		}

		if (target < oldSlot)
		{
			for (int i = oldSlot; i > target; --i)
			{
				Node moved = chunk.mNodes[i - 1];
				chunk.mNodes[i] = moved;
				moved.mSlot = i;
			}
		}
		else
		{
			for (int i = oldSlot; i < target; ++i)
			{
				Node moved = chunk.mNodes[i + 1];
				chunk.mNodes[i] = moved;
				moved.mSlot = i;
			}
		}
		chunk.mNodes[target] = node;
		node.mSlot = target;
		return true;
	}

	private static void insertIntoChunkSorted(Chunk chunk, Node node)
	{
		int low = 0;
		int high = chunk.mNodes.Count;
		while (low < high)
		{
			int mid = (low + high) >> 1;
			if (compare(node, chunk.mNodes[mid]) < 0)
			{
				high = mid;
			}
			else
			{
				low = mid + 1;
			}
		}
		chunk.mNodes.Insert(low, node);
		node.mChunk = chunk;
		for (int i = low; i < chunk.mNodes.Count; ++i)
		{
			chunk.mNodes[i].mSlot = i;
		}
	}

	private static void removeFromChunk(Node node, Chunk chunk)
	{
		if (node == null || chunk == null)
		{
			return;
		}
		int index = node.mSlot;
		if (index < 0 || index >= chunk.mNodes.Count || !ReferenceEquals(chunk.mNodes[index], node))
		{
			return;
		}
		chunk.mNodes.RemoveAt(index);
		for (int i = index; i < chunk.mNodes.Count; ++i)
		{
			chunk.mNodes[i].mSlot = i;
		}
		node.mChunk = null;
		node.mSlot = -1;
	}

	private static void removeNode(Node node)
	{
		if (node != null)
		{
			removeFromChunk(node, node.mChunk);
		}
	}

	private bool rebalanceLayer(LayerState layer)
	{
		bool changed = false;
		for (int i = 0; i < layer.mChunks.Count; ++i)
		{
			Chunk chunk = layer.mChunks[i];
			if (chunk.mNodes.Count > MAX_CHUNK_SIZE)
			{
				Chunk split = new();
				int splitIndex = Mathf.Min(TARGET_CHUNK_SIZE, chunk.mNodes.Count / 2);
				int moveCount = chunk.mNodes.Count - splitIndex;
				for (int j = 0; j < moveCount; ++j)
				{
					split.mNodes.Add(chunk.mNodes[splitIndex + j]);
				}
				chunk.mNodes.RemoveRange(splitIndex, moveCount);
				layer.mChunks.Insert(i + 1, split);
				changed = true;
				++i;
			}
		}
		for (int i = layer.mChunks.Count - 1; i >= 0; --i)
		{
			Chunk chunk = layer.mChunks[i];
			if (chunk.mNodes.Count == 0 && layer.mChunks.Count > 1)
			{
				layer.mChunks.RemoveAt(i);
				changed = true;
				continue;
			}
			if (chunk.mNodes.Count >= MIN_CHUNK_SIZE || layer.mChunks.Count <= 1)
			{
				continue;
			}
			int neighbourIndex = i > 0 ? i - 1 : i + 1;
			if (neighbourIndex < 0 || neighbourIndex >= layer.mChunks.Count)
			{
				continue;
			}
			Chunk neighbour = layer.mChunks[neighbourIndex];
			if (chunk.mNodes.Count + neighbour.mNodes.Count > MAX_CHUNK_SIZE)
			{
				continue;
			}
			if (neighbourIndex < i)
			{
				neighbour.mNodes.AddRange(chunk.mNodes);
				layer.mChunks.RemoveAt(i);
			}
			else
			{
				chunk.mNodes.AddRange(neighbour.mNodes);
				layer.mChunks.RemoveAt(neighbourIndex);
			}
			changed = true;
		}
		assignChunkIndices(layer, 0);
		return changed;
	}

	private static void assignChunkIndices(LayerState layer, int startIndex)
	{
		if (layer == null)
		{
			return;
		}
		for (int chunkIndex = Mathf.Max(startIndex, 0); chunkIndex < layer.mChunks.Count; ++chunkIndex)
		{
			Chunk chunk = layer.mChunks[chunkIndex];
			chunk.mIndex = chunkIndex;
			for (int i = 0; i < chunk.mNodes.Count; ++i)
			{
				Node node = chunk.mNodes[i];
				node.mLayer = layer;
				node.mChunk = chunk;
				node.mSlot = i;
			}
		}
	}

	private static bool isOrderedAgainstNeighbours(Node node)
	{
		if (node == null || node.mChunk == null)
		{
			return true;
		}
		Node previous = getPrevious(node);
		if (previous != null && compare(previous, node) > 0)
		{
			return false;
		}
		Node next = getNext(node);
		return next == null || compare(node, next) <= 0;
	}

	private static Node getPrevious(Node node)
	{
		Chunk chunk = node.mChunk;
		if (node.mSlot > 0)
		{
			return chunk.mNodes[node.mSlot - 1];
		}
		LayerState layer = node.mLayer;
		int chunkIndex = chunk.mIndex;
		if (chunkIndex <= 0)
		{
			return null;
		}
		Chunk previous = layer.mChunks[chunkIndex - 1];
		return previous.mNodes.Count > 0 ? previous.mNodes[^1] : null;
	}

	private static Node getNext(Node node)
	{
		Chunk chunk = node.mChunk;
		if (node.mSlot + 1 < chunk.mNodes.Count)
		{
			return chunk.mNodes[node.mSlot + 1];
		}
		LayerState layer = node.mLayer;
		int chunkIndex = chunk.mIndex;
		if (chunkIndex < 0 || chunkIndex + 1 >= layer.mChunks.Count)
		{
			return null;
		}
		Chunk next = layer.mChunks[chunkIndex + 1];
		return next.mNodes.Count > 0 ? next.mNodes[0] : null;
	}

	private static int compare(Node left, Node right)
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
		if (left.mCachedOrder != right.mCachedOrder)
		{
			return left.mCachedOrder < right.mCachedOrder ? -1 : 1;
		}
		return left.mStableID < right.mStableID ? -1 : left.mStableID > right.mStableID ? 1 : 0;
	}

	private static int getGlobalRank(Node node)
	{
		if (node == null || node.mLayer == null || node.mChunk == null)
		{
			return 0;
		}
		int rank = node.mSlot;
		LayerState layer = node.mLayer;
		for (int i = 0; i < node.mChunk.mIndex; ++i)
		{
			rank += layer.mChunks[i].mNodes.Count;
		}
		return rank;
	}
}
