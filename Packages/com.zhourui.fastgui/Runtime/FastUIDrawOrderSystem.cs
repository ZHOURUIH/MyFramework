using EasyECS;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;

// Logical RenderOrder与最终GPU DrawOrder之间的映射。
// Normal模式保持Identity；SOA模式只在结构构建时提交一个稳定Permutation。
// 普通Position/Color/UV更新仍然按稳定RenderIndex通知，再由这里映射到DrawIndex。
public sealed class FastUIDrawOrderSystem
{
	private static readonly ProfilerMarker UPLOAD_INDEX_MARKER = new("FastUI.Mesh.UploadIndexBuffer");
	private readonly FastCanvas mOwnerCanvas;
	private readonly FastUIVertexStreamSystem mVertexStreams;
	private readonly FastUIBatchSystem mBatchSystem;
	private readonly Int_ECSList mDrawOrder;
	private readonly Int_ECSList mRenderToDrawIndex;
	private readonly Int_ECSList mDrawIndexStarts;
	// 两层Index：mDrawIndexStarts/mIndexECS继续作为CPU逻辑Compact布局；
	// Submit层按Batch保留少量尾部余量，GPU只上传受影响Batch，避免单个文字IndexCount变化搬动并上传整个后缀。
	private readonly Int_ECSList mSubmitIndexECS;
	private readonly Int_ECSList mSubmitDrawIndexStarts;
	private readonly Int_ECSList mSubmitBatchRenderStarts;
	private readonly Int_ECSList mSubmitBatchArenaStarts;
	private readonly Int_ECSList mSubmitBatchCapacities;
	private readonly Int_ECSList mSubmitBatchActualCounts;
	// Draw边界->结束于该边界的Submit Batch索引。只在Submit Layout重建时生成，SubMesh Descriptor查询O(1)。
	private readonly Int_ECSList mSubmitDrawBoundaryBatchIndices;
	// 多个Dirty Range同帧合并到Batch级一次处理。用少量常驻内存换掉重复repack/copy，避免为了省上传反而拖慢CPU。
	private readonly Int_ECSList mDeferredSubmitBatchDirtyStarts;
	private readonly Int_ECSList mDeferredSubmitBatchDirtyEnds;
	private bool mSubmitIndexLayoutValid;
	private bool mDeferSubmitBatchUpdates;
	private bool mDeferredSubmitFullRebuild;
	// Submit层明确以更多内存换CPU稳定性：每Batch预留约50%增长空间，Reserve不进入SubMesh实际绘制。
	// 当前场景即使总容量升到约1.5x Actual也仍只是约1~2MB级Index内存，但可显著减少文字/显隐导致的Arena重排。
	private const int SUBMIT_BATCH_ALIGNMENT = 256;
	private const int SUBMIT_BATCH_MIN_RESERVE = 768;
	private const int SUBMIT_BATCH_RESERVE_SHIFT = 1;
	// 海量碎片Batch时继续给每个Batch保留768+并按256对齐，会把少量有效Index膨胀成数百万Submit Slot。
	// 仅在Batch数很高时切换小Arena策略；正常UI继续沿用原大Reserve，避免影响常规局部更新CPU。
	private const int SUBMIT_BATCH_FRAGMENTED_THRESHOLD = 256;
	private const int SUBMIT_BATCH_FRAGMENTED_ALIGNMENT = 32;
	private const int SUBMIT_BATCH_FRAGMENTED_MIN_RESERVE = 32;
	private readonly Int_ECSList mIndexPatchDrawIndices;
	private readonly Int_ECSList mIndexPatchDrawGenerations;
	private int mIndexPatchDrawGeneration = 1;
	private bool mDrawOrderReordered;
	// Compact Index是正式生产模式：GPU只提交当前真实Index，不再为Geometry Capacity空洞生成退化三角形。
	// Index数量变化通过Prefix局部移动与MeshCPUCopy安全上传处理，不依赖DirectGPU Index。
	// Layout切换到Compact时允许下一次Full Rebuild把历史Stable的大Index Buffer收缩到“当前真实Geometry总量”。
	// 只在模式切换/首次Compact构建时执行一次，稳定帧只允许增长，避免结构刷新造成反复Resize。
	private bool mCompactGPUCapacityResetPending = true;
	// ：Reordered Dense Index Patch使用Direct Column写入，覆盖Stable与Compact；Sparse与容量变化路径保持原实现。
	// ：没有IndexHiddenRange时直接读取BatchState的Member/RenderState列，避免每个DrawIndex重复调用Drawable查询。
	public FastUIDrawOrderSystem(FastCanvas ownerCanvas, FastUIVertexStreamSystem vertexStreams, FastUIBatchSystem batchSystem)
	{
		mOwnerCanvas = ownerCanvas;
		mVertexStreams = vertexStreams;
		mBatchSystem = batchSystem;
		mDrawOrder = new Int_ECSList(256);
		mRenderToDrawIndex = new Int_ECSList(256);
		mDrawIndexStarts = new Int_ECSList(257);
		mSubmitIndexECS = new Int_ECSList(256);
		mSubmitDrawIndexStarts = new Int_ECSList(257);
		mSubmitBatchRenderStarts = new Int_ECSList(32);
		mSubmitBatchArenaStarts = new Int_ECSList(32);
		mSubmitBatchCapacities = new Int_ECSList(32);
		mSubmitBatchActualCounts = new Int_ECSList(32);
		mSubmitDrawBoundaryBatchIndices = new Int_ECSList(257);
		mDeferredSubmitBatchDirtyStarts = new Int_ECSList(32);
		mDeferredSubmitBatchDirtyEnds = new Int_ECSList(32);
		mIndexPatchDrawIndices = new Int_ECSList(256);
		mIndexPatchDrawGenerations = new Int_ECSList(256);
	}
	public bool isReordered()
	{
		return mDrawOrderReordered;
	}
	public bool getCompactIndexMode()
	{
		return true;
	}
	public int getRenderIndexForDrawIndex(int drawIndex)
	{
		if (mDrawOrderReordered && (uint)drawIndex < (uint)mDrawOrder.Count)
		{
			return mDrawOrder[drawIndex];
		}
		return drawIndex;
	}
	public int getDrawIndexForRenderIndex(int renderIndex)
	{
		if (mDrawOrderReordered && (uint)renderIndex < (uint)mRenderToDrawIndex.Count)
		{
			return mRenderToDrawIndex[renderIndex];
		}
		return renderIndex;
	}
	public int getIndexStartForDrawIndex(int drawIndex)
	{
		// Batch/SubMesh查询使用Submit布局；CPU Compact patch继续通过getLogicalIndexStartForDrawIndex访问逻辑布局。
		if (mSubmitIndexLayoutValid && mSubmitDrawIndexStarts.Count == mDrawIndexStarts.Count)
		{
			return getIndexStart(mSubmitDrawIndexStarts, drawIndex);
		}
		return getLogicalIndexStartForDrawIndex(drawIndex);
	}
	// SubMesh的结束边界不能把Batch尾部Reserve算进实际绘制。
	// start仍指向下一个Batch Arena起点，end在Batch边界时返回该Batch真实Index末尾。
	public int getIndexEndForDrawIndex(int drawIndex)
	{
		if (!mSubmitIndexLayoutValid || mSubmitDrawIndexStarts.Count != mDrawIndexStarts.Count)
		{
			return getLogicalIndexStartForDrawIndex(drawIndex);
		}
		// ：旧实现对每个SubMesh结束边界从Batch0开始线性扫描。6001 Batch时退化为O(B²)，
		// 仅这一步就会产生约1800万次Batch边界查询。Submit Layout重建时已记录边界映射，这里直接O(1)。
		if ((uint)drawIndex < (uint)mSubmitDrawBoundaryBatchIndices.Count)
		{
			int batchIndex = mSubmitDrawBoundaryBatchIndices[drawIndex];
			if ((uint)batchIndex < (uint)mSubmitBatchArenaStarts.Count)
			{
				return mSubmitBatchArenaStarts[batchIndex] + mSubmitBatchActualCounts[batchIndex];
			}
		}
		return getIndexStart(mSubmitDrawIndexStarts, drawIndex);
	}
	private int getLogicalIndexStartForDrawIndex(int drawIndex)
	{
		return getIndexStart(mDrawIndexStarts, drawIndex);
	}
	private int getIndexStart(Int_ECSList starts, int drawIndex)
	{
		if (starts == null || starts.Count == 0)
		{
			return 0;
		}
		if (drawIndex <= 0)
		{
			return starts[0];
		}
		if (drawIndex >= starts.Count)
		{
			return starts[^1];
		}
		return starts[drawIndex];
	}
	public int getTotalIndexCount()
	{
		return mDrawIndexStarts.Count > 0 ? mDrawIndexStarts[^1] : 0;
	}
	public int getSubmitIndexCount()
	{
		return mSubmitIndexLayoutValid && mSubmitDrawIndexStarts.Count > 0 ? mSubmitDrawIndexStarts[^1] : 0;
	}
	public int getSubmitBatchCount()
	{
		return mSubmitIndexLayoutValid ? mSubmitBatchRenderStarts.Count : 0;
	}
	private void invalidateSubmitIndexLayout()
	{
		mSubmitIndexLayoutValid = false;
		mDeferSubmitBatchUpdates = false;
		mDeferredSubmitFullRebuild = false;
		mSubmitDrawIndexStarts.Clear();
		mSubmitBatchRenderStarts.Clear();
		mSubmitBatchArenaStarts.Clear();
		mSubmitBatchCapacities.Clear();
		mSubmitBatchActualCounts.Clear();
		mSubmitDrawBoundaryBatchIndices.Clear();
	}
	private int alignSubmitBatchCapacity(int value, int alignment)
	{
		if (value <= 0)
		{
			return 0;
		}
		alignment = Mathf.Max(alignment, 1);
		long aligned = ((long)value + alignment - 1L) / alignment * alignment;
		return aligned >= int.MaxValue ? int.MaxValue : (int)aligned;
	}
	private int calculateSubmitBatchCapacity(int actualIndexCount, int batchCount)
	{
		if (actualIndexCount <= 0)
		{
			return 0;
		}
		bool fragmented = batchCount > SUBMIT_BATCH_FRAGMENTED_THRESHOLD;
		int minReserve = fragmented ? SUBMIT_BATCH_FRAGMENTED_MIN_RESERVE : SUBMIT_BATCH_MIN_RESERVE;
		int alignment = fragmented ? SUBMIT_BATCH_FRAGMENTED_ALIGNMENT : SUBMIT_BATCH_ALIGNMENT;
		int reserve = Mathf.Max(minReserve, actualIndexCount >> SUBMIT_BATCH_RESERVE_SHIFT);
		long wanted = (long)actualIndexCount + reserve;
		return alignSubmitBatchCapacity(wanted >= int.MaxValue ? int.MaxValue : (int)wanted, alignment);
	}
	private int getBatchRunCountForSubmit(int drawCount)
	{
		FastUIBatchRunData_ECSList batchRuns = mBatchSystem?.getBatchRuns();
		int runCount = batchRuns != null ? batchRuns.Count : 0;
		return runCount > 0 ? runCount : (drawCount > 0 ? 1 : 0);
	}
	private int getSubmitBatchDrawStart(FastUIBatchRunData_ECSList batchRuns, int batchIndex)
	{
		if (batchRuns == null || batchRuns.Count == 0)
		{
			return 0;
		}
		return batchRuns.getRenderStartColumn()[batchIndex];
	}
	private int findSubmitBatchForDrawIndex(int drawIndex)
	{
		int batchCount = mSubmitBatchRenderStarts.Count;
		if (batchCount <= 0)
		{
			return -1;
		}
		var starts = mSubmitBatchRenderStarts.getValueColumn();
		int low = 0;
		int high = batchCount - 1;
		while (low <= high)
		{
			int mid = low + ((high - low) >> 1);
			if (starts[mid] <= drawIndex)
			{
				low = mid + 1;
			}
			else
			{
				high = mid - 1;
			}
		}
		return Mathf.Clamp(high, 0, batchCount - 1);
	}
	private int getStoredSubmitBatchDrawEnd(int batchIndex, int drawCount)
	{
		return batchIndex + 1 < mSubmitBatchRenderStarts.Count ? mSubmitBatchRenderStarts[batchIndex + 1] : drawCount;
	}
	private bool isSubmitBatchLayoutCurrent()
	{
		if (!mSubmitIndexLayoutValid || mDrawIndexStarts.Count == 0)
		{
			return false;
		}
		int drawCount = mDrawIndexStarts.Count - 1;
		FastUIBatchRunData_ECSList batchRuns = mBatchSystem?.getBatchRuns();
		int batchCount = getBatchRunCountForSubmit(drawCount);
		if (mSubmitBatchRenderStarts.Count != batchCount || 
			mSubmitDrawIndexStarts.Count != mDrawIndexStarts.Count || 
			mSubmitDrawBoundaryBatchIndices.Count != mDrawIndexStarts.Count)
		{
			return false;
		}
		var storedStarts = mSubmitBatchRenderStarts.getValueColumn();
		for (int batchIndex = 0; batchIndex < batchCount; ++batchIndex)
		{
			if (storedStarts[batchIndex] != getSubmitBatchDrawStart(batchRuns, batchIndex))
			{
				return false;
			}
		}
		return true;
	}
	private int rebuildCompactSubmitLayout()
	{
		if (mDrawIndexStarts.Count == 0)
		{
			invalidateSubmitIndexLayout();
			return getTotalIndexCount();
		}
		int drawCount = mDrawIndexStarts.Count - 1;
		FastUIBatchRunData_ECSList batchRuns = mBatchSystem?.getBatchRuns();
		int batchCount = getBatchRunCountForSubmit(drawCount);
		mSubmitDrawIndexStarts.Clear();
		mSubmitDrawIndexStarts.EnsureCount(drawCount + 1);
		mSubmitBatchRenderStarts.Clear();
		mSubmitBatchRenderStarts.EnsureCount(batchCount);
		mSubmitBatchArenaStarts.Clear();
		mSubmitBatchArenaStarts.EnsureCount(batchCount);
		mSubmitBatchCapacities.Clear();
		mSubmitBatchCapacities.EnsureCount(batchCount);
		mSubmitBatchActualCounts.Clear();
		mSubmitBatchActualCounts.EnsureCount(batchCount);
		mSubmitDrawBoundaryBatchIndices.Clear();
		mSubmitDrawBoundaryBatchIndices.EnsureCount(drawCount + 1);
		var submitBoundaryBatchIndices = mSubmitDrawBoundaryBatchIndices.getValueColumn();
		for (int drawIndex = 0; drawIndex <= drawCount; ++drawIndex)
		{
			submitBoundaryBatchIndices[drawIndex] = -1;
		}
		var submitPrefix = mSubmitDrawIndexStarts.getValueColumn();
		var batchRenderStarts = mSubmitBatchRenderStarts.getValueColumn();
		var batchArenaStarts = mSubmitBatchArenaStarts.getValueColumn();
		var batchCapacities = mSubmitBatchCapacities.getValueColumn();
		var batchActualCounts = mSubmitBatchActualCounts.getValueColumn();
		var logicalPrefix = mDrawIndexStarts.getValueColumn();
		var batchRunStarts = batchRuns != null && batchRuns.Count > 0 ? batchRuns.getRenderStartColumn() : default;
		// ：先只计算Batch Arena元数据和总容量，再一次性扩EasyECS Submit Index。
		// 旧实现对碎片化UI的6001个Batch逐Batch EnsureCount，等价于把同一条增长逻辑调用6001次；
		// ParentSOA只有4个Batch所以几乎看不到这项成本。这里不改变Batch/DrawOrder语义，只消除重复容量维护。
		int submitIndexCount = 0;
		for (int batchIndex = 0; batchIndex < batchCount; ++batchIndex)
		{
			int drawStart = batchRuns == null || batchRuns.Count == 0 ? 0 : batchRunStarts[batchIndex];
			int drawEnd = batchRuns == null || batchRuns.Count == 0 ? drawCount : batchIndex + 1 < batchCount ? batchRunStarts[batchIndex + 1] : drawCount;
			drawStart = Mathf.Clamp(drawStart, 0, drawCount);
			drawEnd = Mathf.Clamp(drawEnd, drawStart, drawCount);
			int actualCount = Mathf.Max(logicalPrefix[drawEnd] - logicalPrefix[drawStart], 0);
			int capacity = calculateSubmitBatchCapacity(actualCount, batchCount);
			batchRenderStarts[batchIndex] = drawStart;
			batchArenaStarts[batchIndex] = submitIndexCount;
			batchCapacities[batchIndex] = capacity;
			batchActualCounts[batchIndex] = actualCount;
			submitBoundaryBatchIndices[drawEnd] = batchIndex;
			submitIndexCount += capacity;
		}
		// EasyECS一次扩到最终Count，后面的热循环只拿Direct Column连续写，不再在Batch循环内反复Add/Ensure。
		mSubmitIndexECS.EnsureCount(submitIndexCount);
		Int_ECSList logicalIndices = mVertexStreams.getIndexECS();
		var logicalIndexColumn = logicalIndices.getValueColumn();
		var submitIndices = mSubmitIndexECS.getValueColumn();
		for (int batchIndex = 0; batchIndex < batchCount; ++batchIndex)
		{
			int drawStart = batchRenderStarts[batchIndex];
			int drawEnd = batchIndex + 1 < batchCount ? batchRenderStarts[batchIndex + 1] : drawCount;
			int logicalStart = logicalPrefix[drawStart];
			int actualCount = batchActualCounts[batchIndex];
			int arenaStart = batchArenaStarts[batchIndex];
			for (int drawIndex = drawStart; drawIndex < drawEnd; ++drawIndex)
			{
				submitPrefix[drawIndex] = arenaStart + logicalPrefix[drawIndex] - logicalStart;
			}
			for (int i = 0; i < actualCount; ++i)
			{
				submitIndices[arenaStart + i] = logicalIndexColumn[logicalStart + i];
			}
			// Batch边界Prefix仍保存Arena Capacity末尾；SubMesh结束通过ActualCount读取，不会绘制Reserve。
			submitPrefix[drawEnd] = arenaStart + batchCapacities[batchIndex];
		}
		if (batchCount == 0)
		{
			submitPrefix[0] = 0;
		}
		mSubmitIndexLayoutValid = true;
		return submitIndexCount;
	}
	private bool repackSubmitBatch(int batchIndex, int dirtyDrawStart, int dirtyDrawEnd, ref FastUIFrameStats stats)
	{
		// 调用方已在一次Batch更新入口验证Submit Layout；不要在每个受影响Batch上再次O(B)扫描全部边界。
		if (!mSubmitIndexLayoutValid || mSubmitDrawIndexStarts.Count != mDrawIndexStarts.Count || (uint)batchIndex >= (uint)mSubmitBatchRenderStarts.Count)
		{
			return false;
		}
		int drawCount = mDrawIndexStarts.Count - 1;
		int batchDrawStart = mSubmitBatchRenderStarts[batchIndex];
		int batchDrawEnd = getStoredSubmitBatchDrawEnd(batchIndex, drawCount);
		int uploadDrawStart = Mathf.Clamp(dirtyDrawStart, batchDrawStart, batchDrawEnd);
		int uploadDrawEnd = Mathf.Clamp(dirtyDrawEnd, uploadDrawStart, batchDrawEnd);
		if (uploadDrawEnd <= uploadDrawStart)
		{
			return true;
		}
		int logicalStart = getLogicalIndexStartForDrawIndex(batchDrawStart);
		int logicalEnd = getLogicalIndexStartForDrawIndex(batchDrawEnd);
		int newActualCount = Mathf.Max(logicalEnd - logicalStart, 0);
		int oldActualCount = mSubmitBatchActualCounts[batchIndex];
		int capacity = mSubmitBatchCapacities[batchIndex];
		if (newActualCount > capacity)
		{
			return false;
		}
		int arenaStart = mSubmitBatchArenaStarts[batchIndex];
		var logicalPrefix = mDrawIndexStarts.getValueColumn();
		var submitPrefix = mSubmitDrawIndexStarts.getValueColumn();
		int oldLocalStart = uploadDrawStart <= batchDrawStart ? 0 : submitPrefix[uploadDrawStart] - arenaStart;
		int oldLocalEnd = uploadDrawEnd >= batchDrawEnd ? oldActualCount : submitPrefix[uploadDrawEnd] - arenaStart;
		int newLocalStart = logicalPrefix[uploadDrawStart] - logicalStart;
		int newLocalEnd = uploadDrawEnd >= batchDrawEnd ? newActualCount : logicalPrefix[uploadDrawEnd] - logicalStart;
		// Dirty区间之前的Prefix理论上不会改变。若检测到异常，宁可本Batch全拷，也绝不扩散到其它Batch。
		bool prefixStartShifted = oldLocalStart != newLocalStart;
		bool suffixShifted = prefixStartShifted || oldLocalEnd != newLocalEnd || oldActualCount != newActualCount;
		int copyLocalStart = prefixStartShifted ? 0 : newLocalStart;
		int copyLocalEnd = suffixShifted ? newActualCount : newLocalEnd;
		Int_ECSList logicalIndices = mVertexStreams.getIndexECS();
		var logicalIndexColumn = logicalIndices.getValueColumn();
		var submitIndices = mSubmitIndexECS.getValueColumn();
		int copyCount = Mathf.Max(copyLocalEnd - copyLocalStart, 0);
		for (int i = 0; i < copyCount; ++i)
		{
			submitIndices[arenaStart + copyLocalStart + i] = logicalIndexColumn[logicalStart + copyLocalStart + i];
		}
		// Shrink后的旧尾部不会被SubMesh引用，不清零、不上传；只更新Descriptor实际IndexCount即可。
		int prefixUpdateStart = prefixStartShifted ? batchDrawStart : uploadDrawStart;
		int prefixUpdateEnd = suffixShifted ? batchDrawEnd : uploadDrawEnd;
		for (int drawIndex = prefixUpdateStart; drawIndex < prefixUpdateEnd; ++drawIndex)
		{
			submitPrefix[drawIndex] = arenaStart + logicalPrefix[drawIndex] - logicalStart;
		}
		if (prefixUpdateEnd >= batchDrawEnd)
		{
			// 该边界同时也是下一Batch起点，因此Prefix继续保存Arena Capacity末尾；
			// SubMesh结束边界通过getIndexEndForDrawIndex读取Actual末尾，不会绘制Reserve。
			submitPrefix[batchDrawEnd] = arenaStart + capacity;
		}
		else
		{
			submitPrefix[prefixUpdateEnd] = arenaStart + logicalPrefix[prefixUpdateEnd] - logicalStart;
		}
		mSubmitBatchActualCounts[batchIndex] = newActualCount;
		int uploadLocalStart = copyLocalStart;
		int uploadLocalEnd = copyLocalEnd;
		int uploadCount = Mathf.Max(uploadLocalEnd - uploadLocalStart, 0);
		if (uploadCount > 0)
		{
			uploadSubmitIndexRange(arenaStart + uploadLocalStart, uploadCount, ref stats);
		}
		return true;
	}
	private void resetDeferredSubmitBatchDirtyRanges()
	{
		int batchCount = mSubmitBatchRenderStarts.Count;
		mDeferredSubmitBatchDirtyStarts.Clear();
		mDeferredSubmitBatchDirtyStarts.EnsureCount(batchCount);
		mDeferredSubmitBatchDirtyEnds.Clear();
		mDeferredSubmitBatchDirtyEnds.EnsureCount(batchCount);
		var dirtyStarts = mDeferredSubmitBatchDirtyStarts.getValueColumn();
		var dirtyEnds = mDeferredSubmitBatchDirtyEnds.getValueColumn();
		for (int i = 0; i < batchCount; ++i)
		{
			dirtyStarts[i] = int.MaxValue;
			dirtyEnds[i] = -1;
		}
	}
	private void beginDeferredSubmitBatchUpdates()
	{
		mDeferSubmitBatchUpdates = true;
		mDeferredSubmitFullRebuild = !isSubmitBatchLayoutCurrent();
		resetDeferredSubmitBatchDirtyRanges();
	}
	private void recordDeferredSubmitDrawRange(int drawStart, int drawEnd)
	{
		if (!mDeferSubmitBatchUpdates)
		{
			return;
		}
		// beginDeferredSubmitBatchUpdates已经验证过一次布局；本轮逻辑Index patch不会改变Batch边界。
		if (mDeferredSubmitFullRebuild)
		{
			return;
		}
		int drawCount = Mathf.Max(mDrawIndexStarts.Count - 1, 0);
		drawStart = Mathf.Clamp(drawStart, 0, drawCount);
		drawEnd = Mathf.Clamp(drawEnd, drawStart, drawCount);
		if (drawEnd <= drawStart)
		{
			return;
		}
		int firstBatch = findSubmitBatchForDrawIndex(drawStart);
		int lastBatch = findSubmitBatchForDrawIndex(drawEnd - 1);
		if (firstBatch < 0 || lastBatch < firstBatch)
		{
			mDeferredSubmitFullRebuild = true;
			return;
		}
		var dirtyStarts = mDeferredSubmitBatchDirtyStarts.getValueColumn();
		var dirtyEnds = mDeferredSubmitBatchDirtyEnds.getValueColumn();
		for (int batchIndex = firstBatch; batchIndex <= lastBatch; ++batchIndex)
		{
			int batchStart = mSubmitBatchRenderStarts[batchIndex];
			int batchEnd = getStoredSubmitBatchDrawEnd(batchIndex, drawCount);
			int localStart = Mathf.Max(drawStart, batchStart);
			int localEnd = Mathf.Min(drawEnd, batchEnd);
			if (localStart < dirtyStarts[batchIndex])
			{
				dirtyStarts[batchIndex] = localStart;
			}
			if (localEnd > dirtyEnds[batchIndex])
			{
				dirtyEnds[batchIndex] = localEnd;
			}
		}
	}
	private bool ensureSubmitGPUStateCurrent(ref FastUIFrameStats stats)
	{
		if (!mSubmitIndexLayoutValid)
		{
			return false;
		}
		int oldCapacity = mVertexStreams.getGPUIndexCapacity();
		UnityEngine.Rendering.IndexFormat oldFormat = mVertexStreams.getGPUIndexFormat();
		mVertexStreams.ensureGPUIndexSubmitCapacity(getSubmitIndexCount(), false, ref stats);
		return oldCapacity != mVertexStreams.getGPUIndexCapacity() || oldFormat != mVertexStreams.getGPUIndexFormat();
	}
	private bool endDeferredSubmitBatchUpdates(ref FastUIFrameStats stats)
	{
		if (!mDeferSubmitBatchUpdates)
		{
			return false;
		}
		mDeferSubmitBatchUpdates = false;
		if (mDeferredSubmitFullRebuild || !isSubmitBatchLayoutCurrent())
		{
			mDeferredSubmitFullRebuild = false;
			rebuildCompactSubmitAndUpload(ref stats, false);
			return true;
		}
		mDeferredSubmitFullRebuild = false;
		// VertexSpan跨过UInt16边界时SetIndexBufferParams会丢失原GPU内容；这种低频生命周期统一重建Submit，避免局部上传留下未初始化Batch。
		if (ensureSubmitGPUStateCurrent(ref stats))
		{
			rebuildCompactSubmitAndUpload(ref stats, false);
			return true;
		}
		int batchCount = mSubmitBatchRenderStarts.Count;
		var dirtyStarts = mDeferredSubmitBatchDirtyStarts.getValueColumn();
		var dirtyEnds = mDeferredSubmitBatchDirtyEnds.getValueColumn();
		for (int batchIndex = 0; batchIndex < batchCount; ++batchIndex)
		{
			if (dirtyEnds[batchIndex] <= dirtyStarts[batchIndex])
			{
				continue;
			}
			if (!repackSubmitBatch(batchIndex, dirtyStarts[batchIndex], dirtyEnds[batchIndex], ref stats))
			{
				rebuildCompactSubmitAndUpload(ref stats, false);
				return true;
			}
		}
		return false;
	}
	private bool uploadCompactSubmitDrawRange(int drawStart, int drawEnd, ref FastUIFrameStats stats)
	{
		int drawCount = Mathf.Max(mDrawIndexStarts.Count - 1, 0);
		drawStart = Mathf.Clamp(drawStart, 0, drawCount);
		drawEnd = Mathf.Clamp(drawEnd, drawStart, drawCount);
		if (drawEnd <= drawStart)
		{
			return false;
		}
		if (mDeferSubmitBatchUpdates)
		{
			recordDeferredSubmitDrawRange(drawStart, drawEnd);
			return false;
		}
		if (!isSubmitBatchLayoutCurrent())
		{
			rebuildCompactSubmitAndUpload(ref stats, false);
			return true;
		}
		if (ensureSubmitGPUStateCurrent(ref stats))
		{
			rebuildCompactSubmitAndUpload(ref stats, false);
			return true;
		}
		int firstBatch = findSubmitBatchForDrawIndex(drawStart);
		int lastBatch = findSubmitBatchForDrawIndex(drawEnd - 1);
		if (firstBatch < 0 || lastBatch < firstBatch)
		{
			rebuildCompactSubmitAndUpload(ref stats, false);
			return true;
		}
		for (int batchIndex = firstBatch; batchIndex <= lastBatch; ++batchIndex)
		{
			int batchStart = mSubmitBatchRenderStarts[batchIndex];
			int batchEnd = getStoredSubmitBatchDrawEnd(batchIndex, drawCount);
			if (!repackSubmitBatch(batchIndex, Mathf.Max(drawStart, batchStart), Mathf.Min(drawEnd, batchEnd), ref stats))
			{
				rebuildCompactSubmitAndUpload(ref stats, false);
				return true;
			}
		}
		return false;
	}
	private void uploadSubmitIndexRange(int indexStart, int indexCount, ref FastUIFrameStats stats)
	{
		if (indexCount <= 0)
		{
			return;
		}
		using (UPLOAD_INDEX_MARKER.Auto())
		{
			mVertexStreams.uploadExternalIndexRange(mSubmitIndexECS, indexStart, indexStart, indexCount, ref stats);
		}
		++stats.mIndexUploadCallCount;
		stats.mUploadedIndexCount += indexCount;
	}
	public void rebuildCompactSubmitAndUpload(ref FastUIFrameStats stats, bool allowShrink)
	{
		int submitIndexCount = rebuildCompactSubmitLayout();
		mVertexStreams.ensureGPUIndexSubmitCapacity(submitIndexCount, allowShrink || mCompactGPUCapacityResetPending, ref stats);
		if (submitIndexCount > 0)
		{
			uploadSubmitIndexRange(0, submitIndexCount, ref stats);
		}
		mCompactGPUCapacityResetPending = false;
		if (mDeferSubmitBatchUpdates)
		{
			// 延迟批处理中途若触发了Full Rebuild，从当前Submit状态重新累计后续Dirty，避免结束时重复Full Upload。
			mDeferredSubmitFullRebuild = false;
			resetDeferredSubmitBatchDirtyRanges();
		}
		stats.mIndexRebuilt = true;
	}
	public bool refreshCompactSubmitLayoutAfterBatchChange(ref FastUIFrameStats stats)
	{
		if (isSubmitBatchLayoutCurrent())
		{
			return false;
		}
		rebuildCompactSubmitAndUpload(ref stats, false);
		return true;
	}
	public void setDrawOrder(Int_ECSList sourceOrder, int count)
	{
		invalidateSubmitIndexLayout();
		mDrawOrderReordered = false;
		if (sourceOrder == null || sourceOrder.Count != count)
		{
			mDrawOrder.Clear();
			mRenderToDrawIndex.Clear();
			return;
		}
		var source = sourceOrder.getValueColumn();
		for (int i = 0; i < count; ++i)
		{
			if (source[i] != i)
			{
				mDrawOrderReordered = true;
				break;
			}
		}
		if (!mDrawOrderReordered)
		{
			mDrawOrder.Clear();
			mRenderToDrawIndex.Clear();
			return;
		}
		mDrawOrder.Clear();
		mDrawOrder.EnsureCount(count);
		mRenderToDrawIndex.Clear();
		mRenderToDrawIndex.EnsureCount(count);
		var drawOrder = mDrawOrder.getValueColumn();
		var renderToDraw = mRenderToDrawIndex.getValueColumn();
		for (int drawIndex = 0; drawIndex < count; ++drawIndex)
		{
			int renderIndex = source[drawIndex];
			drawOrder[drawIndex] = renderIndex;
			renderToDraw[renderIndex] = drawIndex;
		}
	}
	// SOA Lane只会在固定Draw区间内重排同一组RenderElement，因此无需升级为Full DrawStructure。
	// 先验证Lane重排前后的Index容量总和一致，再只改该Draw区间的Permutation、Index Prefix和Index数据。
	public bool patchDrawOrderRanges(List<FastUIRenderElement> renderElements, Int_ECSList sourceOrder, FastUIRangeData_ECSList drawRanges, ref FastUIFrameStats stats)
	{
		// 生产默认Compact路径与历史Stable基线只在入口分流一次，热点循环内部不再逐元素判断Layout模式。
		return patchDrawOrderRangesCompact(renderElements, sourceOrder, drawRanges, ref stats);
	}
	private bool patchDrawOrderRangesCompact(List<FastUIRenderElement> renderElements, Int_ECSList sourceOrder, FastUIRangeData_ECSList drawRanges, ref FastUIFrameStats stats)
	{
		int renderCount = renderElements != null ? renderElements.Count : 0;
		if (sourceOrder == null || sourceOrder.Count != renderCount || drawRanges == null || drawRanges.Count == 0 || !hasValidIndexLayout(renderElements))
		{
			return false;
		}
		ensureExplicitDrawOrder(renderCount);
		var source = sourceOrder.getValueColumn();
		var drawOrder = mDrawOrder.getValueColumn();
		var renderToDraw = mRenderToDrawIndex.getValueColumn();
		var prefix = mDrawIndexStarts.getValueColumn();
		var rangeStarts = drawRanges.getStartColumn();
		var rangeEnds = drawRanges.getEndColumn();
		for (int rangeIndex = 0; rangeIndex < drawRanges.Count; ++rangeIndex)
		{
			int start = Mathf.Clamp(rangeStarts[rangeIndex], 0, renderCount);
			int end = Mathf.Clamp(rangeEnds[rangeIndex], start, renderCount);
			if (end <= start)
			{
				continue;
			}
			int oldRegionStart = prefix[start];
			int oldRegionEnd = prefix[end];
			int newRegionCapacity = 0;
			for (int drawIndex = start; drawIndex < end; ++drawIndex)
			{
				newRegionCapacity += getCompactElementIndexCount(source[drawIndex], getElement(renderElements, source[drawIndex]));
			}
			if (newRegionCapacity != oldRegionEnd - oldRegionStart)
			{
				return false;
			}
			for (int drawIndex = start; drawIndex < end; ++drawIndex)
			{
				int renderIndex = source[drawIndex];
				drawOrder[drawIndex] = renderIndex;
				renderToDraw[renderIndex] = drawIndex;
			}
			int writeIndex = oldRegionStart;
			for (int drawIndex = start; drawIndex < end; ++drawIndex)
			{
				prefix[drawIndex] = writeIndex;
				int renderIndex = drawOrder[drawIndex];
				FastUIRenderElement element = getElement(renderElements, renderIndex);
				writeIndex += getCompactElementIndexCount(renderIndex, element);
			}
			prefix[end] = writeIndex;
			for (int drawIndex = start; drawIndex < end; ++drawIndex)
			{
				int renderIndex = drawOrder[drawIndex];
				FastUIRenderElement element = getElement(renderElements, renderIndex);
				writeCompactLayoutIndexBlock(prefix[drawIndex], element, prefix[drawIndex + 1] - prefix[drawIndex]);
			}
			uploadCompactSubmitDrawRange(start, end, ref stats);
		}
		mDrawOrderReordered = true;
		mVertexStreams.setCurrentIndexCount(getTotalIndexCount());
		return true;
	}
	// Canvas的Batch Dirty仍以Logical RenderIndex记录；SOA启用后在提交前一次性转换成DrawIndex连续区间。
	public void convertRenderRangesToDrawRanges(FastUIRangeData_ECSList sourceRanges, FastUIRangeData_ECSList targetRanges, int renderCount)
	{
		targetRanges.Clear();
		if (sourceRanges == null || sourceRanges.Count == 0 || renderCount <= 0)
		{
			return;
		}
		if (!mDrawOrderReordered)
		{
			var starts = sourceRanges.getStartColumn();
			var ends = sourceRanges.getEndColumn();
			for (int i = 0; i < sourceRanges.Count; ++i)
			{
				int start = Mathf.Clamp(starts[i], 0, renderCount);
				int end = Mathf.Clamp(ends[i], start, renderCount);
				if (end > start)
				{
					targetRanges.Add(new FastUIRangeData(start, end));
				}
			}
			FastUIRangeUtility.merge(targetRanges);
			return;
		}
		mIndexPatchDrawIndices.Clear();
		var sourceStarts = sourceRanges.getStartColumn();
		var sourceEnds = sourceRanges.getEndColumn();
		for (int rangeIndex = 0; rangeIndex < sourceRanges.Count; ++rangeIndex)
		{
			int start = Mathf.Clamp(sourceStarts[rangeIndex], 0, renderCount);
			int end = Mathf.Clamp(sourceEnds[rangeIndex], start, renderCount);
			for (int renderIndex = start; renderIndex < end; ++renderIndex)
			{
				mIndexPatchDrawIndices.Add(getDrawIndexForRenderIndex(renderIndex));
			}
		}
		if (mIndexPatchDrawIndices.Count == 0)
		{
			return;
		}
		mIndexPatchDrawIndices.SortFast();
		var drawIndices = mIndexPatchDrawIndices.getValueColumn();
		int rangeStart = drawIndices[0];
		int previous = rangeStart;
		for (int i = 1; i < mIndexPatchDrawIndices.Count; ++i)
		{
			int current = drawIndices[i];
			if (current == previous)
			{
				continue;
			}
			if (current != previous + 1)
			{
				targetRanges.Add(new FastUIRangeData(rangeStart, previous + 1));
				rangeStart = current;
			}
			previous = current;
		}
		targetRanges.Add(new FastUIRangeData(rangeStart, previous + 1));
	}
	public int writeFullIndexBuffer(List<FastUIRenderElement> renderElements)
	{
		return writeCompactFullIndexBuffer(renderElements);
	}
	private int writeCompactFullIndexBuffer(List<FastUIRenderElement> renderElements)
	{
		int count = renderElements != null ? renderElements.Count : 0;
		int maxIndexCount = calculateTotalIndexCapacity(renderElements);
		mVertexStreams.ensureCPUIndexCapacity(maxIndexCount);
		ensureIndexPrefixCount(count + 1);
		var prefixColumn = mDrawIndexStarts.getValueColumn();
		int indexStart = 0;
		for (int drawIndex = 0; drawIndex < count; ++drawIndex)
		{
			prefixColumn[drawIndex] = indexStart;
			int renderIndex = getRenderIndexForDrawIndex(drawIndex);
			FastUIRenderElement element = getElement(renderElements, renderIndex);
			int indexCount = getCompactElementIndexCount(renderIndex, element);
			if (indexCount > 0)
			{
				writeCompactIndexBlock(indexStart, element, indexCount);
				indexStart += indexCount;
			}
		}
		prefixColumn[count] = indexStart;
		return indexStart;
	}
	private void ensureIndexPatchGenerationCount(int count)
	{
		mIndexPatchDrawGenerations.EnsureCount(count);
	}
	private void advanceIndexPatchGeneration()
	{
		if (mIndexPatchDrawGeneration < int.MaxValue)
		{
			++mIndexPatchDrawGeneration;
			return;
		}
		var generationColumn = mIndexPatchDrawGenerations.getValueColumn();
		for (int i = 0; i < mIndexPatchDrawGenerations.Count; ++i)
		{
			generationColumn[i] = 0;
		}
		mIndexPatchDrawGeneration = 1;
	}
	public bool patchIndexRanges(List<FastUIRenderElement> renderElements, FastUIRangeData_ECSList renderRanges, ref FastUIFrameStats stats)
	{
		return patchIndexRangesCompact(renderElements, renderRanges, ref stats);
	}
	private bool patchIndexRangesCompact(List<FastUIRenderElement> renderElements, FastUIRangeData_ECSList renderRanges, ref FastUIFrameStats stats)
	{
		if (renderRanges == null || renderRanges.Count <= 0 || renderElements == null || renderElements.Count == 0)
		{
			return false;
		}
		if (!mDrawOrderReordered)
		{
			// 普通RenderOrder下Dirty Range可能很多（例如1400个数量文字）。
			// 逻辑Index仍按原算法逐Range修补，但Submit层延迟到最后按Batch合并一次，避免同一Batch重复Copy/SetIndexBufferData。
			beginDeferredSubmitBatchUpdates();
			bool changed = false;
			var starts = renderRanges.getStartColumn();
			var ends = renderRanges.getEndColumn();
			for (int i = 0; i < renderRanges.Count; ++i)
			{
				changed |= patchIndexRangeCompact(renderElements, starts[i], ends[i] - starts[i], ref stats);
			}
			changed |= endDeferredSubmitBatchUpdates(ref stats);
			return changed;
		}
		if (!hasValidIndexLayout(renderElements))
		{
			rebuildAndUploadFullIndexBuffer(renderElements, ref stats);
			return true;
		}
		int drawCount = renderElements.Count;
		ensureIndexPatchGenerationCount(drawCount);
		advanceIndexPatchGeneration();
		mIndexPatchDrawIndices.Clear();
		var generationColumn = mIndexPatchDrawGenerations.getValueColumn();
		var rangeStarts = renderRanges.getStartColumn();
		var rangeEnds = renderRanges.getEndColumn();
		int rawDrawIndexCount = 0;
		int minDrawIndex = drawCount;
		int maxDrawIndex = -1;
		for (int rangeIndex = 0; rangeIndex < renderRanges.Count; ++rangeIndex)
		{
			int start = Mathf.Clamp(rangeStarts[rangeIndex], 0, drawCount);
			int end = Mathf.Clamp(rangeEnds[rangeIndex], start, drawCount);
			rawDrawIndexCount += end - start;
			for (int renderIndex = start; renderIndex < end; ++renderIndex)
			{
				int drawIndex = getDrawIndexForRenderIndex(renderIndex);
				if (generationColumn[drawIndex] == mIndexPatchDrawGeneration)
				{
					continue;
				}
				generationColumn[drawIndex] = mIndexPatchDrawGeneration;
				mIndexPatchDrawIndices.Add(drawIndex);
				if (drawIndex < minDrawIndex)
				{
					minDrawIndex = drawIndex;
				}
				if (drawIndex > maxDrawIndex)
				{
					maxDrawIndex = drawIndex;
				}
			}
		}
		int uniqueCount = mIndexPatchDrawIndices.Count;
		if (uniqueCount <= 0)
		{
			return false;
		}
		bool capacityChanged = false;
		var touchedDrawIndices = mIndexPatchDrawIndices.getValueColumn();
		for (int i = 0; i < uniqueCount; ++i)
		{
			int drawIndex = touchedDrawIndices[i];
			int renderIndex = getRenderIndexForDrawIndex(drawIndex);
			FastUIRenderElement element = getElement(renderElements, renderIndex);
			int oldCount = getLogicalIndexStartForDrawIndex(drawIndex + 1) - getLogicalIndexStartForDrawIndex(drawIndex);
			if (oldCount != getCompactElementIndexCount(renderIndex, element))
			{
				capacityChanged = true;
				break;
			}
		}
		if (capacityChanged)
		{
			bool changed = patchIndexCapacityRangeCompact(renderElements, minDrawIndex, maxDrawIndex + 1, true, ref stats);
			return changed;
		}
		int totalIndexCount = getTotalIndexCount();
		if (totalIndexCount > mVertexStreams.getGPUIndexCapacity())
		{
			rebuildAndUploadFullIndexBuffer(renderElements, ref stats);
			return true;
		}
		// 超过20%的DrawIndex被修改时，线性扫描整个DrawOrder比排序大量离散索引更稳定。
		// 小集合继续Sort unique touched indices，避免为几十/几百个节点扫描完整Canvas。
		bool denseScan = uniqueCount * 5 >= drawCount;
		if (!denseScan)
		{
			mIndexPatchDrawIndices.SortFast();
			touchedDrawIndices = mIndexPatchDrawIndices.getValueColumn();
		}
		if (denseScan)
		{
			if (!writeDenseCompactIndexBlocksDirect(renderElements, drawCount))
			{
				for (int drawIndex = 0; drawIndex < drawCount; ++drawIndex)
				{
					if (generationColumn[drawIndex] != mIndexPatchDrawGeneration)
					{
						continue;
					}
					int renderIndex = getRenderIndexForDrawIndex(drawIndex);
					FastUIRenderElement element = getElement(renderElements, renderIndex);
					writeCompactLayoutIndexBlock(getLogicalIndexStartForDrawIndex(drawIndex), element, getCompactElementIndexCount(renderIndex, element));
				}
			}
		}
		else
		{
			for (int i = 0; i < uniqueCount; ++i)
			{
				int drawIndex = touchedDrawIndices[i];
				int renderIndex = getRenderIndexForDrawIndex(drawIndex);
				FastUIRenderElement element = getElement(renderElements, renderIndex);
				writeCompactLayoutIndexBlock(getLogicalIndexStartForDrawIndex(drawIndex), element, getCompactElementIndexCount(renderIndex, element));
			}
		}
		int uploadRangeCount = 0;
		if (denseScan)
		{
			bool inRange = false;
			for (int drawIndex = 0; drawIndex < drawCount; ++drawIndex)
			{
				bool touched = generationColumn[drawIndex] == mIndexPatchDrawGeneration;
				if (touched && !inRange)
				{
					++uploadRangeCount;
					inRange = true;
				}
				else if (!touched)
				{
					inRange = false;
				}
			}
		}
		else
		{
			uploadRangeCount = 1;
			for (int i = 1; i < uniqueCount; ++i)
			{
				if (touchedDrawIndices[i] != touchedDrawIndices[i - 1] + 1)
				{
					++uploadRangeCount;
				}
			}
		}
		if (uploadRangeCount > FastUIMeshUtility.MAX_PARTIAL_UPLOAD_RANGE)
		{
			uploadCompactSubmitDrawRange(0, drawCount, ref stats);
		}
		else if (denseScan)
		{
			int rangeStart = -1;
			for (int drawIndex = 0; drawIndex <= drawCount; ++drawIndex)
			{
				bool touched = drawIndex < drawCount && generationColumn[drawIndex] == mIndexPatchDrawGeneration;
				if (touched && rangeStart < 0)
				{
					rangeStart = drawIndex;
				}
				if (!touched && rangeStart >= 0)
				{
					uploadCompactSubmitDrawRange(rangeStart, drawIndex, ref stats);
					rangeStart = -1;
				}
			}
		}
		else
		{
			int rangeStart = touchedDrawIndices[0];
			int previous = rangeStart;
			for (int i = 1; i <= uniqueCount; ++i)
			{
				bool flush = i == uniqueCount || touchedDrawIndices[i] != previous + 1;
				if (flush)
				{
					uploadCompactSubmitDrawRange(rangeStart, previous + 1, ref stats);
					if (i < uniqueCount)
					{
						rangeStart = touchedDrawIndices[i];
					}
				}
				if (i < uniqueCount)
				{
					previous = touchedDrawIndices[i];
				}
			}
		}
		mVertexStreams.setCurrentIndexCount(totalIndexCount);
		return false;
	}

	public bool patchIndexRange(List<FastUIRenderElement> renderElements, int renderStart, int renderCount, ref FastUIFrameStats stats)
	{
		return patchIndexRangeCompact(renderElements, renderStart, renderCount, ref stats);
	}
	private bool patchIndexRangeCompact(List<FastUIRenderElement> renderElements, int renderStart, int renderCount, ref FastUIFrameStats stats)
	{
		if (renderCount <= 0 || renderElements == null || renderElements.Count == 0)
		{
			return false;
		}
		if (mDrawOrderReordered)
		{
			return patchReorderedIndexRangeCompact(renderElements, renderStart, renderCount, ref stats);
		}
		if (!hasValidIndexLayout(renderElements))
		{
			rebuildAndUploadFullIndexBuffer(renderElements, ref stats);
			return true;
		}
		int start = Mathf.Clamp(renderStart, 0, renderElements.Count);
		int end = Mathf.Clamp(renderStart + renderCount, start, renderElements.Count);
		if (end <= start)
		{
			return false;
		}
		bool capacityChanged = false;
		for (int drawIndex = start; drawIndex < end; ++drawIndex)
		{
			int oldCapacity = getLogicalIndexStartForDrawIndex(drawIndex + 1) - getLogicalIndexStartForDrawIndex(drawIndex);
			if (oldCapacity != getCompactElementIndexCount(drawIndex, renderElements[drawIndex]))
			{
				capacityChanged = true;
				break;
			}
		}
		if (capacityChanged)
		{
			return patchIndexCapacityRangeCompact(renderElements, start, end, false, ref stats);
		}
		int totalIndexCount = getTotalIndexCount();
		if (totalIndexCount > mVertexStreams.getGPUIndexCapacity())
		{
			rebuildAndUploadFullIndexBuffer(renderElements, ref stats);
			return true;
		}
		for (int drawIndex = start; drawIndex < end; ++drawIndex)
		{
			FastUIRenderElement element = renderElements[drawIndex];
			int indexCapacity = getCompactElementIndexCount(drawIndex, element);
			writeCompactLayoutIndexBlock(getLogicalIndexStartForDrawIndex(drawIndex), element, indexCapacity);
		}
		bool submitLayoutChanged = uploadCompactSubmitDrawRange(start, end, ref stats);
		mVertexStreams.setCurrentIndexCount(totalIndexCount);
		return submitLayoutChanged;
	}
	public void dispose()
	{
		mDrawOrder.Dispose();
		mRenderToDrawIndex.Dispose();
		mDrawIndexStarts.Dispose();
		mSubmitIndexECS.Dispose();
		mSubmitDrawIndexStarts.Dispose();
		mSubmitBatchRenderStarts.Dispose();
		mSubmitBatchArenaStarts.Dispose();
		mSubmitBatchCapacities.Dispose();
		mSubmitBatchActualCounts.Dispose();
		mSubmitDrawBoundaryBatchIndices.Dispose();
		mDeferredSubmitBatchDirtyStarts.Dispose();
		mDeferredSubmitBatchDirtyEnds.Dispose();
		mIndexPatchDrawIndices.Dispose();
		mIndexPatchDrawGenerations.Dispose();
	}
	private void ensureExplicitDrawOrder(int count)
	{
		if (mDrawOrderReordered && mDrawOrder.Count == count && mRenderToDrawIndex.Count == count)
		{
			return;
		}
		mDrawOrder.Clear();
		mDrawOrder.EnsureCount(count);
		mRenderToDrawIndex.Clear();
		mRenderToDrawIndex.EnsureCount(count);
		var drawOrder = mDrawOrder.getValueColumn();
		var renderToDraw = mRenderToDrawIndex.getValueColumn();
		for (int i = 0; i < count; ++i)
		{
			drawOrder[i] = i;
			renderToDraw[i] = i;
		}
		mDrawOrderReordered = true;
	}
	public int getRecommendedGPUIndexCapacity(List<FastUIRenderElement> renderElements)
	{
		//  Compact GPU容量由Submit层决定；逻辑Compact Index仍可自由移动，不再要求GPU跟随整个逻辑后缀。
		if (mSubmitIndexLayoutValid)
		{
			return Mathf.Max(getSubmitIndexCount(), 1);
		}
		int renderCount = renderElements != null ? renderElements.Count : 0;
		if (mDrawIndexStarts.Count == renderCount + 1)
		{
			return Mathf.Max(getTotalIndexCount(), 1);
		}
		return Mathf.Max(mVertexStreams.getLiveGeometryIndexCount(), 1);
	}
	private int calculateTotalIndexCapacity(List<FastUIRenderElement> renderElements)
	{
		if (renderElements == null)
		{
			return 0;
		}
		int total = 0;
		for (int renderIndex = 0; renderIndex < renderElements.Count; ++renderIndex)
		{
			total += getElementIndexCapacity(renderElements[renderIndex]);
		}
		return total;
	}
	private bool patchReorderedIndexRangeCompact(List<FastUIRenderElement> renderElements, int renderStart, int renderCount, ref FastUIFrameStats stats)
	{
		if (!hasValidIndexLayout(renderElements))
		{
			rebuildAndUploadFullIndexBuffer(renderElements, ref stats);
			return true;
		}
		int start = Mathf.Clamp(renderStart, 0, renderElements.Count);
		int end = Mathf.Clamp(renderStart + renderCount, start, renderElements.Count);
		if (end <= start)
		{
			return false;
		}
		mIndexPatchDrawIndices.Clear();
		for (int renderIndex = start; renderIndex < end; ++renderIndex)
		{
			mIndexPatchDrawIndices.Add(getDrawIndexForRenderIndex(renderIndex));
		}
		mIndexPatchDrawIndices.SortFast();
		var patchDrawColumn = mIndexPatchDrawIndices.getValueColumn();
		bool capacityChanged = false;
		for (int i = 0; i < mIndexPatchDrawIndices.Count; ++i)
		{
			int drawIndex = patchDrawColumn[i];
			FastUIRenderElement element = getElement(renderElements, getRenderIndexForDrawIndex(drawIndex));
			int oldCapacity = getLogicalIndexStartForDrawIndex(drawIndex + 1) - getLogicalIndexStartForDrawIndex(drawIndex);
			if (oldCapacity != getCompactElementIndexCount(getRenderIndexForDrawIndex(drawIndex), element))
			{
				capacityChanged = true;
				break;
			}
		}
		if (capacityChanged)
		{
			int drawStart = patchDrawColumn[0];
			int drawEnd = patchDrawColumn[mIndexPatchDrawIndices.Count - 1] + 1;
			return patchIndexCapacityRangeCompact(renderElements, drawStart, drawEnd, true, ref stats);
		}
		int totalIndexCount = getTotalIndexCount();
		if (totalIndexCount > mVertexStreams.getGPUIndexCapacity())
		{
			rebuildAndUploadFullIndexBuffer(renderElements, ref stats);
			return true;
		}
		for (int i = 0; i < mIndexPatchDrawIndices.Count; ++i)
		{
			int drawIndex = patchDrawColumn[i];
			int renderIndex = getRenderIndexForDrawIndex(drawIndex);
			FastUIRenderElement element = getElement(renderElements, renderIndex);
			writeCompactLayoutIndexBlock(getLogicalIndexStartForDrawIndex(drawIndex), element, getCompactElementIndexCount(renderIndex, element));
		}
		int rangeCount = 1;
		for (int i = 1; i < mIndexPatchDrawIndices.Count; ++i)
		{
			if (patchDrawColumn[i] != patchDrawColumn[i - 1] + 1)
			{
				++rangeCount;
			}
		}
		if (rangeCount > FastUIMeshUtility.MAX_PARTIAL_UPLOAD_RANGE)
		{
			uploadCompactSubmitDrawRange(0, renderElements.Count, ref stats);
		}
		else
		{
			int rangeStart = patchDrawColumn[0];
			int previous = rangeStart;
			for (int uploadRangeIndex = 1; uploadRangeIndex <= mIndexPatchDrawIndices.Count; ++uploadRangeIndex)
			{
				bool flush = uploadRangeIndex == mIndexPatchDrawIndices.Count || patchDrawColumn[uploadRangeIndex] != previous + 1;
				if (flush)
				{
					uploadCompactSubmitDrawRange(rangeStart, previous + 1, ref stats);
					if (uploadRangeIndex < mIndexPatchDrawIndices.Count)
					{
						rangeStart = patchDrawColumn[uploadRangeIndex];
					}
				}
				if (uploadRangeIndex < mIndexPatchDrawIndices.Count)
				{
					previous = patchDrawColumn[uploadRangeIndex];
				}
			}
		}
		mVertexStreams.setCurrentIndexCount(totalIndexCount);
		return false;
	}
	private bool patchIndexCapacityRangeCompact(List<FastUIRenderElement> renderElements, int drawStart, int drawEnd, bool useDrawOrder, ref FastUIFrameStats stats)
	{
		drawStart = Mathf.Clamp(drawStart, 0, renderElements.Count);
		drawEnd = Mathf.Clamp(drawEnd, drawStart, renderElements.Count);
		if (drawEnd <= drawStart)
		{
			return false;
		}
		int oldTotal = getTotalIndexCount();
		int oldRegionStart = getLogicalIndexStartForDrawIndex(drawStart);
		int oldRegionEnd = getLogicalIndexStartForDrawIndex(drawEnd);
		int newRegionCapacity = 0;
		for (int drawIndex = drawStart; drawIndex < drawEnd; ++drawIndex)
		{
			int renderIndex = useDrawOrder ? getRenderIndexForDrawIndex(drawIndex) : drawIndex;
			newRegionCapacity += getCompactElementIndexCount(renderIndex, getElement(renderElements, renderIndex));
		}
		int delta = newRegionCapacity - (oldRegionEnd - oldRegionStart);
		int newTotal = oldTotal + delta;
		mVertexStreams.ensureCPUIndexCapacity(newTotal);
		var indices = mVertexStreams.getIndexECS().getValueColumn();
		if (delta > 0)
		{
			for (int source = oldTotal - 1; source >= oldRegionEnd; --source)
			{
				indices[source + delta] = indices[source];
			}
		}
		else if (delta < 0)
		{
			for (int source = oldRegionEnd; source < oldTotal; ++source)
			{
				indices[source + delta] = indices[source];
			}
		}
		var prefixColumn = mDrawIndexStarts.getValueColumn();
		int writeIndex = oldRegionStart;
		for (int drawIndex = drawStart; drawIndex < drawEnd; ++drawIndex)
		{
			prefixColumn[drawIndex] = writeIndex;
			int renderIndex = useDrawOrder ? getRenderIndexForDrawIndex(drawIndex) : drawIndex;
			FastUIRenderElement element = getElement(renderElements, renderIndex);
			int indexCapacity = getCompactElementIndexCount(renderIndex, element);
			writeCompactLayoutIndexBlock(writeIndex, element, indexCapacity);
			writeIndex += indexCapacity;
		}
		prefixColumn[drawEnd] = writeIndex;
		if (delta != 0)
		{
			for (int drawIndex = drawEnd + 1; drawIndex < mDrawIndexStarts.Count; ++drawIndex)
			{
				prefixColumn[drawIndex] += delta;
			}
		}
		bool submitLayoutChanged = uploadCompactSubmitDrawRange(drawStart, drawEnd, ref stats);
		mVertexStreams.setCurrentIndexCount(newTotal);
		// DrawRun可能因Visibility在Batch内部切段；逻辑Prefix数量变化时仍刷新Descriptor，但GPU Submit Arena不移动后续Batch。
		return delta != 0 || submitLayoutChanged;
	}
	private bool hasValidIndexLayout(List<FastUIRenderElement> renderElements)
	{
		if (renderElements == null || mDrawIndexStarts.Count != renderElements.Count + 1)
		{
			return false;
		}
		return getTotalIndexCount() == mVertexStreams.getCurrentIndexCount();
	}
	private void ensureIndexPrefixCount(int count)
	{
		if (mDrawIndexStarts.Count == count)
		{
			return;
		}
		mDrawIndexStarts.Clear();
		mDrawIndexStarts.EnsureCount(count);
	}
	public void rebuildAndUploadFullIndexBuffer(List<FastUIRenderElement> renderElements, ref FastUIFrameStats stats)
	{
		int totalIndexCount = writeFullIndexBuffer(renderElements);
		rebuildCompactSubmitAndUpload(ref stats, mCompactGPUCapacityResetPending);
		mVertexStreams.setCurrentIndexCount(totalIndexCount);
	}
	private FastUIRenderElement getElement(List<FastUIRenderElement> renderElements, int renderIndex)
	{
		return renderElements != null && renderIndex >= 0 && renderIndex < renderElements.Count ? renderElements[renderIndex] : null;
	}
	private int getElementIndexCount(FastUIRenderElement element)
	{
		if (element == null || element.getCanvas() != mOwnerCanvas || element.getVertexSlot() < 0)
		{
			return 0;
		}
		return mVertexStreams.getGeometryIndexCount(element.getVertexSlot());
	}
	private int getElementIndexCapacity(FastUIRenderElement element)
	{
		if (element == null || element.getCanvas() != mOwnerCanvas || element.getVertexSlot() < 0)
		{
			return 0;
		}
		return mVertexStreams.getGeometryIndexCapacity(element.getVertexSlot());
	}
	private bool writeDenseCompactIndexBlocksDirect(List<FastUIRenderElement> renderElements, int drawCount)
	{
		if (renderElements == null || renderElements.Count != drawCount || mDrawOrder.Count != drawCount || mDrawIndexStarts.Count != drawCount + 1)
		{
			return false;
		}
		FastUIGeometryRangeData_ECSList ranges = mVertexStreams.getGeometryRangeECS();
		Int_ECSList indexECS = mVertexStreams.getIndexECS();
		if (ranges == null || indexECS == null)
		{
			return false;
		}
		FastUIBatchElementData_ECSList batchStates = mBatchSystem.getElementBatchStates();
		if (batchStates == null || batchStates.Count < renderElements.Count)
		{
			return false;
		}
		var generationColumn = mIndexPatchDrawGenerations.getValueColumn();
		var drawOrder = mDrawOrder.getValueColumn();
		var prefix = mDrawIndexStarts.getValueColumn();
		var vertexStarts = ranges.getVertexStartColumn();
		var indices = indexECS.getValueColumn();
		var memberColumn = batchStates.getMemberColumn();
		var renderStateColumn = batchStates.getRenderStateColumn();
		bool directDrawableState = !mBatchSystem.hasIndexHiddenRanges();
		int drawableStateMask = mBatchSystem.getIndexDrawableStateMask();
		int culledStateMask = mBatchSystem.getIndexCulledStateMask();
		for (int drawIndex = 0; drawIndex < drawCount; ++drawIndex)
		{
			if (generationColumn[drawIndex] != mIndexPatchDrawGeneration)
			{
				continue;
			}
			int indexStart = prefix[drawIndex];
			int indexCount = prefix[drawIndex + 1] - indexStart;
			if (indexCount <= 0)
			{
				continue;
			}
			int renderIndex = drawOrder[drawIndex];
			if ((uint)renderIndex >= (uint)renderElements.Count)
			{
				return false;
			}
			FastUIRenderElement element = renderElements[renderIndex];
			if (element == null)
			{
				return false;
			}
			bool drawable;
			if (directDrawableState)
			{
				int renderState = renderStateColumn[renderIndex];
				drawable = memberColumn[renderIndex] != 0 &&
					(renderState & drawableStateMask) == drawableStateMask &&
					(renderState & culledStateMask) == 0;
			}
			else
			{
				drawable = mBatchSystem.isElementIndexDrawable(renderIndex);
			}
			if (!drawable)
			{
				return false;
			}
			int slot = element.getVertexSlot();
			if ((uint)slot >= (uint)ranges.Count)
			{
				return false;
			}
			int vertexStart = vertexStarts[slot];
			if (vertexStart < 0 || indexStart < 0 || indexStart + indexCount > indexECS.Count || indexCount % 6 != 0)
			{
				return false;
			}
			if (indexCount == 6)
			{
				indices[indexStart + 0] = vertexStart + 0;
				indices[indexStart + 1] = vertexStart + 1;
				indices[indexStart + 2] = vertexStart + 2;
				indices[indexStart + 3] = vertexStart + 0;
				indices[indexStart + 4] = vertexStart + 2;
				indices[indexStart + 5] = vertexStart + 3;
				continue;
			}
			int quadCount = indexCount / 6;
			for (int quadIndex = 0; quadIndex < quadCount; ++quadIndex)
			{
				int quadVertexStart = vertexStart + quadIndex * 4;
				int quadIndexStart = indexStart + quadIndex * 6;
				indices[quadIndexStart + 0] = quadVertexStart + 0;
				indices[quadIndexStart + 1] = quadVertexStart + 1;
				indices[quadIndexStart + 2] = quadVertexStart + 2;
				indices[quadIndexStart + 3] = quadVertexStart + 0;
				indices[quadIndexStart + 4] = quadVertexStart + 2;
				indices[quadIndexStart + 5] = quadVertexStart + 3;
			}
		}
		return true;
	}
	private void writeCompactLayoutIndexBlock(int indexStart, FastUIRenderElement element, int indexCount)
	{
		if (indexCount > 0)
		{
			writeCompactIndexBlock(indexStart, element, indexCount);
		}
	}
	private int getCompactElementIndexCount(int renderIndex, FastUIRenderElement element)
	{
		if (element == null || !mBatchSystem.isElementIndexDrawable(renderIndex))
		{
			return 0;
		}
		return getElementIndexCount(element);
	}
	private void writeCompactIndexBlock(int indexStart, FastUIRenderElement element, int indexCount)
	{
		int vertexStart = mVertexStreams.getGeometryVertexStart(element.getVertexSlot());
		int quadCount = indexCount / 6;
		var indices = mVertexStreams.getIndexECS().getValueColumn();
		for (int quadIndex = 0; quadIndex < quadCount; ++quadIndex)
		{
			int quadVertexStart = vertexStart + quadIndex * 4;
			int quadIndexStart = indexStart + quadIndex * 6;
			indices[quadIndexStart + 0] = quadVertexStart + 0;
			indices[quadIndexStart + 1] = quadVertexStart + 1;
			indices[quadIndexStart + 2] = quadVertexStart + 2;
			indices[quadIndexStart + 3] = quadVertexStart + 0;
			indices[quadIndexStart + 4] = quadVertexStart + 2;
			indices[quadIndexStart + 5] = quadVertexStart + 3;
		}
	}
}
