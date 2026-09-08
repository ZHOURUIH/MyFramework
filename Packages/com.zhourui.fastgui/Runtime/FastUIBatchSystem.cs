using EasyECS;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

// FastUI批次构建与提交子系统。
// 负责维护BatchRun/FastUIDrawRunData、局部Batch边界修补，以及SubMesh、Material、Texture的最终提交。
// 该类拥有批次相关状态，不负责SOA DrawOrder投影本身。
public sealed class FastUIBatchSystem
{
	// 已冻结的生产阈值：只变化1个SubMesh时Partial，变化2个及以上直接Bulk。
	private const int MAX_PARTIAL_SUBMESH_APPLY_COUNT = 1;
	//  Limited Visible Runs：全Canvas最多额外拆4个SubMesh，只选择收益最大的中间Clip Hole。
	// 4096个Index约等于1365个Triangle，避免为了很小的GPU收益增加移动端DrawCall。
	private const int MAX_EXTRA_CLIP_DRAW_RUNS = 4;
	private const int MIN_CLIP_HOLE_SAVED_INDEX_COUNT = 4096;
	private const int ADAPTIVE_CURSOR_MIN_RANGE_COUNT = 16;
	private const int ADAPTIVE_CURSOR_MIN_ESTIMATED_SCAN_COUNT = 64;
	private const int RENDER_STATE_ACTIVE = 1 << 0;
	private const int RENDER_STATE_VISIBLE = 1 << 1;
	private const int RENDER_STATE_CULLED = 1 << 2;
	private const int RENDER_STATE_CLIP_OUTSIDE = 1 << 3;
	private const int RENDER_STATE_DRAWABLE_MASK = RENDER_STATE_ACTIVE | RENDER_STATE_VISIBLE;
	private static readonly int MAIN_TEX_ID = Shader.PropertyToID("_MainTex");
	private readonly FastCanvas mOwner;
	private readonly FastUIVertexStreamSystem mVertexStreams;
	private FastUIDrawOrderSystem mDrawOrder;
	private Mesh mMesh;
	private MeshRenderer mMeshRenderer;
	private MaterialPropertyBlock mPropertyBlock;
	private readonly FastUIBatchElementData_ECSList mElementBatchStates;
	private FastUIBatchRunData_ECSList mBatchRuns;
	private readonly FastUIRangeData_ECSList mPendingBatchRefreshRanges;
	private readonly FastUIRangeData_ECSList mExpandedBatchRefreshRanges;
	private FastUIBatchRunData_ECSList mBatchRunMergeBuffer;
	private readonly FastUIDrawRunData_ECSList mDrawRuns;
	// ：Limited Visible Runs临时状态全部使用EasyECS连续列，避免List<struct>/临时GC。
	private readonly Int_ECSList mClipTrimmedBatchStarts;
	private readonly Int_ECSList mClipTrimmedBatchEnds;
	private readonly Int_ECSList mClipHoleBatchIndices;
	private readonly Int_ECSList mClipHoleStarts;
	private readonly Int_ECSList mClipHoleEnds;
	private readonly Int_ECSList mClipHoleSavedIndexCounts;
	private bool mLimitedClipDrawRunsActive;
	private bool mClipSubmitRangeCullingEnabled;
	private int mLastLimitedClipExtraDrawRunCount;
	private int mLastLimitedClipSavedIndexCount;
	private readonly FastUIRangeData_ECSList mHiddenRenderRanges;
	private int mIndexHiddenRenderRangeCount;
	// IndexHidden按32个RenderIndex一个word存储，word自身带generation。
	// 保持Index Patch O(1)查询，同时把连续HiddenRange的标记写入从N次降低到约N/32次。
	private readonly Int_ECSList mIndexHiddenWordGenerations;
	private readonly Int_ECSList mIndexHiddenWordBits;
	private int mIndexHiddenGeneration = 1;
	private int mLastIndexHiddenWordWriteCount;
	private bool mHasIndexHiddenRanges;
	private readonly List<Material> mSharedMaterials = new();
	private readonly List<Texture> mAppliedTextures = new();
	private readonly FastUISubMeshDescriptorData_ECSList mSubMeshDescriptors;
	private readonly Int_ECSList mChangedSubMeshSlots;
	private readonly Int_ECSList mChangedTextureSlots;
	private int mCurrentBatchCount;
	private int mAppliedDrawRunCount;
	private int mSubMeshCapacity = 1;
	private int mAppliedMeshSubMeshCount = 1;
	private bool mForceTextureRebind;
	public FastUIBatchSystem(FastCanvas owner, FastUIVertexStreamSystem vertexStreams, Mesh mesh, MeshRenderer meshRenderer, MaterialPropertyBlock propertyBlock)
	{
		mOwner = owner;
		mVertexStreams = vertexStreams;
		mMesh = mesh;
		mMeshRenderer = meshRenderer;
		mPropertyBlock = propertyBlock;
		mElementBatchStates = new FastUIBatchElementData_ECSList(256);
		mBatchRuns = new FastUIBatchRunData_ECSList(64);
		mPendingBatchRefreshRanges = new FastUIRangeData_ECSList(64);
		mExpandedBatchRefreshRanges = new FastUIRangeData_ECSList(64);
		mBatchRunMergeBuffer = new FastUIBatchRunData_ECSList(64);
		mDrawRuns = new FastUIDrawRunData_ECSList(64);
		mClipTrimmedBatchStarts = new Int_ECSList(16);
		mClipTrimmedBatchEnds = new Int_ECSList(16);
		mClipHoleBatchIndices = new Int_ECSList(MAX_EXTRA_CLIP_DRAW_RUNS);
		mClipHoleStarts = new Int_ECSList(MAX_EXTRA_CLIP_DRAW_RUNS);
		mClipHoleEnds = new Int_ECSList(MAX_EXTRA_CLIP_DRAW_RUNS);
		mClipHoleSavedIndexCounts = new Int_ECSList(MAX_EXTRA_CLIP_DRAW_RUNS);
		mHiddenRenderRanges = new FastUIRangeData_ECSList(64);
		mIndexHiddenWordGenerations = new Int_ECSList(16);
		mIndexHiddenWordBits = new Int_ECSList(16);
		mSubMeshDescriptors = new FastUISubMeshDescriptorData_ECSList(64);
		mChangedSubMeshSlots = new Int_ECSList(256);
		mChangedTextureSlots = new Int_ECSList(256);
	}
	public void setDrawOrderSystem(FastUIDrawOrderSystem drawOrder)
	{
		mDrawOrder = drawOrder;
	}
	// 低频结构恢复时强制下一次Batch提交重新绑定所有有效SubMesh的_MainTex。
	// 这是结构恢复后的正确性保护，必须保留。
	public void forceTextureRebind()
	{
		mForceTextureRebind = true;
	}
	private int getRenderIndexForDrawIndex(int drawIndex)
	{
		return mDrawOrder != null ? mDrawOrder.getRenderIndexForDrawIndex(drawIndex) : drawIndex;
	}
	public FastUIBatchElementData_ECSList getElementBatchStates()
	{
		return mElementBatchStates;
	}
	public bool hasIndexHiddenRanges()
	{
		return mHasIndexHiddenRanges;
	}
	public int getIndexDrawableStateMask()
	{
		return RENDER_STATE_DRAWABLE_MASK;
	}
	public int getIndexCulledStateMask()
	{
		return RENDER_STATE_CULLED;
	}
	public FastUIBatchRunData_ECSList getBatchRuns()
	{
		return mBatchRuns;
	}
	public FastUIRangeData_ECSList getPendingRefreshRanges()
	{
		return mPendingBatchRefreshRanges;
	}
	public bool getClipSubmitRangeCullingEnabled()
	{
		return mClipSubmitRangeCullingEnabled;
	}
	public bool setClipSubmitRangeCullingEnabled(bool enabled)
	{
		if (mClipSubmitRangeCullingEnabled == enabled)
		{
			return false;
		}
		mClipSubmitRangeCullingEnabled = enabled;
		mLimitedClipDrawRunsActive = false;
		mLastLimitedClipExtraDrawRunCount = 0;
		mLastLimitedClipSavedIndexCount = 0;
		return true;
	}
	public FastUIRangeData_ECSList getExpandedRefreshRanges()
	{
		return mExpandedBatchRefreshRanges;
	}
	public FastUIBatchRunData_ECSList getMergeBuffer()
	{
		return mBatchRunMergeBuffer;
	}
	public FastUIRangeData_ECSList getHiddenRenderRanges()
	{
		return mHiddenRenderRanges;
	}
	public int getBatchCount()
	{
		return mCurrentBatchCount;
	}
	public int getSubMeshCapacity()
	{
		return mSubMeshCapacity;
	}
	public int getActiveSubMeshCount()
	{
		return mAppliedMeshSubMeshCount;
	}
	public int getAppliedMaterialSlotCount()
	{
		return mSharedMaterials.Count;
	}
	public int getMaxPartialSubMeshApplyCount()
	{
		return MAX_PARTIAL_SUBMESH_APPLY_COUNT;
	}
	public int getBatchRunCount()
	{
		return mBatchRuns.Count;
	}
	public int getHiddenRenderRangeCount()
	{
		return mHiddenRenderRanges.Count;
	}
	public int getIndexHiddenRenderRangeCount()
	{
		return mIndexHiddenRenderRangeCount;
	}
	public int getLastIndexHiddenWordWriteCount()
	{
		return mLastIndexHiddenWordWriteCount;
	}
	public int getLimitedClipExtraDrawRunCount()
	{
		return mLastLimitedClipExtraDrawRunCount;
	}
	public int getLimitedClipSavedIndexCount()
	{
		return mLastLimitedClipSavedIndexCount;
	}
	public int getMaxExtraClipDrawRuns()
	{
		return MAX_EXTRA_CLIP_DRAW_RUNS;
	}
	public int getMinClipHoleSavedIndexCount()
	{
		return MIN_CLIP_HOLE_SAVED_INDEX_COUNT;
	}
	public int getBatchRunRenderStart(int runIndex)
	{
		return (uint)runIndex < (uint)mBatchRuns.Count ? mBatchRuns.getRenderStartColumn()[runIndex] : -1;
	}
	public Material getBatchRunMaterial(int runIndex)
	{
		return (uint)runIndex < (uint)mBatchRuns.Count ? mBatchRuns.getMaterialColumn()[runIndex] : null;
	}
	public Texture getBatchRunTexture(int runIndex)
	{
		return (uint)runIndex < (uint)mBatchRuns.Count ? mBatchRuns.getTextureColumn()[runIndex] : null;
	}
	public int getEffectiveDrawRunCount(int renderElementCount)
	{
		if (mHiddenRenderRanges.Count > 0 || mLimitedClipDrawRunsActive)
		{
			return mDrawRuns.Count;
		}
		int count = mBatchRuns.Count;
		var starts = mBatchRuns.getRenderStartColumn();
		while (count > 0 && starts[count - 1] >= renderElementCount)
		{
			--count;
		}
		return count;
	}
	public int getEffectiveDrawRunStart(int runIndex, int renderElementCount)
	{
		int count = getEffectiveDrawRunCount(renderElementCount);
		if ((uint)runIndex >= (uint)count)
		{
			return -1;
		}
		return mHiddenRenderRanges.Count > 0 || mLimitedClipDrawRunsActive ? mDrawRuns.getRenderStartColumn()[runIndex] : mBatchRuns.getRenderStartColumn()[runIndex];
	}
	public int getEffectiveDrawRunEnd(int runIndex, int renderElementCount)
	{
		int count = getEffectiveDrawRunCount(renderElementCount);
		if ((uint)runIndex >= (uint)count)
		{
			return -1;
		}
		if (mHiddenRenderRanges.Count > 0 || mLimitedClipDrawRunsActive)
		{
			return mDrawRuns.getRenderEndColumn()[runIndex];
		}
		return runIndex + 1 < count ? mBatchRuns.getRenderStartColumn()[runIndex + 1] : renderElementCount;
	}
	public Material getEffectiveDrawRunMaterial(int runIndex, int renderElementCount)
	{
		int count = getEffectiveDrawRunCount(renderElementCount);
		if ((uint)runIndex >= (uint)count)
		{
			return null;
		}
		return mHiddenRenderRanges.Count > 0 || mLimitedClipDrawRunsActive ? mDrawRuns.getMaterialColumn()[runIndex] : mBatchRuns.getMaterialColumn()[runIndex];
	}
	public Texture getEffectiveDrawRunTexture(int runIndex, int renderElementCount)
	{
		int count = getEffectiveDrawRunCount(renderElementCount);
		if ((uint)runIndex >= (uint)count)
		{
			return null;
		}
		return mHiddenRenderRanges.Count > 0 || mLimitedClipDrawRunsActive ? mDrawRuns.getTextureColumn()[runIndex] : mBatchRuns.getTextureColumn()[runIndex];
	}
	public bool isIndexHiddenRenderIndex(int renderIndex)
	{
		if (!mHasIndexHiddenRanges || renderIndex < 0)
		{
			return false;
		}
		int wordIndex = renderIndex >> 5;
		if ((uint)wordIndex >= (uint)mIndexHiddenWordGenerations.Count)
		{
			return false;
		}
		if (mIndexHiddenWordGenerations.getValueColumn()[wordIndex] != mIndexHiddenGeneration)
		{
			return false;
		}
		return (mIndexHiddenWordBits.getValueColumn()[wordIndex] & (1 << (renderIndex & 31))) != 0;
	}
	public bool isElementIndexDrawable(int renderIndex)
	{
		if (renderIndex < 0 || renderIndex >= mElementBatchStates.Count)
		{
			return false;
		}
		if (mElementBatchStates.getMemberColumn()[renderIndex] == 0)
		{
			return false;
		}
		int state = mElementBatchStates.getRenderStateColumn()[renderIndex];
		return (state & RENDER_STATE_DRAWABLE_MASK) == RENDER_STATE_DRAWABLE_MASK &&
			(state & RENDER_STATE_CULLED) == 0 && !isIndexHiddenRenderIndex(renderIndex);
	}
	public bool isElementClipOutside(int renderIndex)
	{
		if ((uint)renderIndex >= (uint)mElementBatchStates.Count)
		{
			return false;
		}
		return (mElementBatchStates.getRenderStateColumn()[renderIndex] & RENDER_STATE_CLIP_OUTSIDE) != 0;
	}
	public void clearTransientRefreshState()
	{
		mPendingBatchRefreshRanges.Clear();
		mExpandedBatchRefreshRanges.Clear();
		mBatchRunMergeBuffer.Clear();
		mLimitedClipDrawRunsActive = false;
		mLastLimitedClipExtraDrawRunCount = 0;
		mLastLimitedClipSavedIndexCount = 0;
	}
	public void dispose()
	{
		mElementBatchStates.Dispose();
		mBatchRuns.Dispose();
		mPendingBatchRefreshRanges.Dispose();
		mExpandedBatchRefreshRanges.Dispose();
		mBatchRunMergeBuffer.Dispose();
		mDrawRuns.Dispose();
		mClipTrimmedBatchStarts.Dispose();
		mClipTrimmedBatchEnds.Dispose();
		mClipHoleBatchIndices.Dispose();
		mClipHoleStarts.Dispose();
		mClipHoleEnds.Dispose();
		mClipHoleSavedIndexCounts.Dispose();
		mHiddenRenderRanges.Dispose();
		mIndexHiddenWordGenerations.Dispose();
		mIndexHiddenWordBits.Dispose();
		mSubMeshDescriptors.Dispose();
		mChangedSubMeshSlots.Dispose();
		mChangedTextureSlots.Dispose();
		mSharedMaterials.Clear();
		mAppliedTextures.Clear();
		mMesh = null;
		mMeshRenderer = null;
		mPropertyBlock = null;
		mDrawOrder = null;
	}
	// BatchKey只在Setter/结构变化时从对象层同步一次；帧内SOA/Batch热循环只读下面的EasyECS连续列。
	public void rebuildElementBatchStates(List<FastUIRenderElement> renderElements, Material defaultMaterial)
	{
		mElementBatchStates.Clear();
		int count = renderElements != null ? renderElements.Count : 0;
		if (count <= 0)
		{
			return;
		}
		ensureElementBatchStateCount(count);
		syncElementBatchStateRange(renderElements, defaultMaterial, 0, count);
	}
	// 诊断：只在独立Benchmark Canvas上调用，生产rebuildElementBatchStates保持原单循环算法不变。
	public void syncElementBatchStateRange(List<FastUIRenderElement> renderElements, Material defaultMaterial, int start, int count)
	{
		int renderCount = renderElements != null ? renderElements.Count : 0;
		if (renderCount <= 0)
		{
			mElementBatchStates.Clear();
			return;
		}
		if (mElementBatchStates.Count < renderCount)
		{
			ensureElementBatchStateCount(renderCount);
		}
		int begin = Mathf.Clamp(start, 0, renderCount);
		int end = Mathf.Clamp(start + Mathf.Max(count, 0), begin, renderCount);
		var materialColumn = mElementBatchStates.getMaterialColumn();
		var textureColumn = mElementBatchStates.getTextureColumn();
		var memberColumn = mElementBatchStates.getMemberColumn();
		var renderStateColumn = mElementBatchStates.getRenderStateColumn();
		for (int renderIndex = begin; renderIndex < end; ++renderIndex)
		{
			FastUIRenderElement element = renderElements[renderIndex];
			bool member = isElementBatchMember(element);
			memberColumn[renderIndex] = member ? 1 : 0;
			if (member)
			{
				materialColumn[renderIndex] = element.getRenderMaterial(defaultMaterial);
				textureColumn[renderIndex] = element.getRenderTexture();
				renderStateColumn[renderIndex] = buildRenderState(element);
			}
			else
			{
				materialColumn[renderIndex] = null;
				textureColumn[renderIndex] = null;
				renderStateColumn[renderIndex] = 0;
			}
		}
	}
	public void syncElementBatchState(int renderIndex, FastUIRenderElement element, Material defaultMaterial)
	{
		if (renderIndex < 0)
		{
			return;
		}
		if (mElementBatchStates.Count <= renderIndex)
		{
			ensureElementBatchStateCount(renderIndex + 1);
		}
		var materialColumn = mElementBatchStates.getMaterialColumn();
		var textureColumn = mElementBatchStates.getTextureColumn();
		var memberColumn = mElementBatchStates.getMemberColumn();
		var renderStateColumn = mElementBatchStates.getRenderStateColumn();
		bool member = isElementBatchMember(element);
		memberColumn[renderIndex] = member ? 1 : 0;
		if (member)
		{
			materialColumn[renderIndex] = element.getRenderMaterial(defaultMaterial);
			textureColumn[renderIndex] = element.getRenderTexture();
			renderStateColumn[renderIndex] = buildRenderState(element);
		}
		else
		{
			materialColumn[renderIndex] = null;
			textureColumn[renderIndex] = null;
			renderStateColumn[renderIndex] = 0;
		}
	}
	public void syncElementRenderState(int renderIndex, FastUIRenderElement element)
	{
		if (renderIndex < 0)
		{
			return;
		}
		if (mElementBatchStates.Count <= renderIndex)
		{
			ensureElementBatchStateCount(renderIndex + 1);
		}
		mElementBatchStates.getRenderStateColumn()[renderIndex] = isElementBatchMember(element) ? buildRenderState(element) : 0;
	}
	public void patchElementActiveState(int renderIndex, bool active)
	{
		patchElementRenderStateBit(renderIndex, RENDER_STATE_ACTIVE, active);
	}
	public void patchElementVisibleState(int renderIndex, bool visible)
	{
		patchElementRenderStateBit(renderIndex, RENDER_STATE_VISIBLE, visible);
	}
	public void patchElementCullState(int renderIndex, bool culled)
	{
		patchElementRenderStateBit(renderIndex, RENDER_STATE_CULLED, culled);
	}
	public void patchElementClipCullState(int renderIndex, bool outside)
	{
		patchElementRenderStateBit(renderIndex, RENDER_STATE_CLIP_OUTSIDE, outside);
	}
	private void patchElementRenderStateBit(int renderIndex, int bit, bool enabled)
	{
		if ((uint)renderIndex >= (uint)mElementBatchStates.Count)
		{
			return;
		}
		var stateColumn = mElementBatchStates.getRenderStateColumn();
		int state = stateColumn[renderIndex];
		stateColumn[renderIndex] = enabled ? state | bit : state & ~bit;
	}
	public void syncElementTextureState(int renderIndex, Texture texture)
	{
		if (renderIndex < 0)
		{
			return;
		}
		if (mElementBatchStates.Count <= renderIndex)
		{
			ensureElementBatchStateCount(renderIndex + 1);
		}
		mElementBatchStates.getTextureColumn()[renderIndex] = texture;
	}
	private void ensureElementBatchStateCount(int count)
	{
		while (mElementBatchStates.Count < count)
		{
			mElementBatchStates.Add(default);
		}
	}
	private void ensureElementBatchStates(List<FastUIRenderElement> renderElements, Material defaultMaterial)
	{
		int count = renderElements != null ? renderElements.Count : 0;
		if (mElementBatchStates.Count != count)
		{
			rebuildElementBatchStates(renderElements, defaultMaterial);
		}
	}
	public void shiftBatchRunRenderIndices(int startRenderIndex, int delta)
	{
		if (delta == 0 || mBatchRuns.Count == 0)
		{
			return;
		}
		var renderStarts = mBatchRuns.getRenderStartColumn();
		for (int i = 0; i < mBatchRuns.Count; ++i)
		{
			if (renderStarts[i] < startRenderIndex)
			{
				continue;
			}
			renderStarts[i] += delta;
		}
	}
	// 开始一次局部Batch刷新，清空仅属于本帧的Range缓存，不破坏已提交BatchRun。
	public void beginBatchRefresh()
	{
		mPendingBatchRefreshRanges.Clear();
	}
	public void appendBatchRefreshRange(int renderStart, int renderCount)
	{
		if (renderCount <= 0)
		{
			return;
		}
		int start = Mathf.Max(renderStart, 0);
		int end = Mathf.Max(renderStart + renderCount, start);
		if (end <= start)
		{
			return;
		}
		if (mPendingBatchRefreshRanges.Count > 0)
		{
			int lastIndex = mPendingBatchRefreshRanges.Count - 1;
			var pendingEnds = mPendingBatchRefreshRanges.getEndColumn();
			if (start <= pendingEnds[lastIndex])
			{
				if (end > pendingEnds[lastIndex])
				{
					pendingEnds[lastIndex] = end;
				}
				return;
			}
		}
		mPendingBatchRefreshRanges.Add(new FastUIRangeData(start, end));
	}
	public int getAdaptiveCursorMinRangeCount()
	{
		return ADAPTIVE_CURSOR_MIN_RANGE_COUNT;
	}
	public int getAdaptiveCursorMinEstimatedScanCount()
	{
		return ADAPTIVE_CURSOR_MIN_ESTIMATED_SCAN_COUNT;
	}
	// 根据Range数量和估算扫描量选择Boundary或Cursor算法，避免小更新承担全量Merge成本。
	public FastUIBatchRefreshMode resolveBatchRefreshMode()
	{
		int rangeCount = mPendingBatchRefreshRanges.Count;
		int estimatedScanCount = 0;
		var rangeStarts = mPendingBatchRefreshRanges.getStartColumn();
		var rangeEnds = mPendingBatchRefreshRanges.getEndColumn();
		for (int i = 0; i < rangeCount; ++i)
		{
			estimatedScanCount += Mathf.Max(rangeEnds[i] - rangeStarts[i], 0) + 1;
		}
		if (rangeCount >= ADAPTIVE_CURSOR_MIN_RANGE_COUNT || estimatedScanCount >= ADAPTIVE_CURSOR_MIN_ESTIMATED_SCAN_COUNT)
		{
			return FastUIBatchRefreshMode.CursorLinearMerge;
		}
		return FastUIBatchRefreshMode.IncrementalBoundary;
	}
	// Full路径从当前DrawOrder重新生成全部BatchRun，只在结构大范围变化或局部路径回退时使用。
	public void rebuildBatchRunsFull(List<FastUIRenderElement> renderElements, Material defaultMaterial)
	{
		mBatchRuns.Clear();
		ensureElementBatchStates(renderElements, defaultMaterial);
		bool hasPrevious = false;
		Material previousMaterial = null;
		Texture previousTexture = null;
		int drawCount = renderElements.Count;
		// ：Full结构已知最终最多不会超过drawCount个Run，EasyECS一次预留，避免碎片UI从64逐级扩容。
		mBatchRuns.EnsureCapacity(drawCount);
		var materialColumn = mElementBatchStates.getMaterialColumn();
		var textureColumn = mElementBatchStates.getTextureColumn();
		var memberColumn = mElementBatchStates.getMemberColumn();
		for (int drawIndex = 0; drawIndex < drawCount; ++drawIndex)
		{
			int renderIndex = getRenderIndexForDrawIndex(drawIndex);
			if ((uint)renderIndex >= (uint)mElementBatchStates.Count || memberColumn[renderIndex] == 0)
			{
				continue;
			}
			Material material = materialColumn[renderIndex];
			Texture texture = textureColumn[renderIndex];
			if (!hasPrevious || previousMaterial != material || previousTexture != texture)
			{
				mBatchRuns.Add(new FastUIBatchRunData(drawIndex, material, texture));
			}
			hasPrevious = true;
			previousMaterial = material;
			previousTexture = texture;
		}
	}
	// Boundary增量算法只替换受影响Batch边界，保持未触及区间的Run不变。
	public void rebuildBatchRunsIncremental(List<FastUIRenderElement> renderElements, Material defaultMaterial, bool useDrawOrder)
	{
		if (renderElements.Count == 0)
		{
			mBatchRuns.Clear();
			return;
		}
		ensureElementBatchStates(renderElements, defaultMaterial);
		var materialColumn = mElementBatchStates.getMaterialColumn();
		var textureColumn = mElementBatchStates.getTextureColumn();
		var memberColumn = mElementBatchStates.getMemberColumn();
		var pendingStarts = mPendingBatchRefreshRanges.getStartColumn();
		var pendingEnds = mPendingBatchRefreshRanges.getEndColumn();
		for (int rangeIndex = 0; rangeIndex < mPendingBatchRefreshRanges.Count; ++rangeIndex)
		{
			int start = Mathf.Clamp(pendingStarts[rangeIndex], 0, renderElements.Count);
			int end = Mathf.Clamp(pendingEnds[rangeIndex], start, renderElements.Count);
			if (end <= start)
			{
				continue;
			}
			bool hasPrevious = tryFindPreviousBatchKey(
				renderElements.Count,
				start - 1,
				useDrawOrder,
				out Material previousMaterial,
				out Texture previousTexture
			);
			for (int orderIndex = start; orderIndex < end; ++orderIndex)
			{
				int renderIndex = useDrawOrder ? getRenderIndexForDrawIndex(orderIndex) : orderIndex;
				if ((uint)renderIndex >= (uint)mElementBatchStates.Count || memberColumn[renderIndex] == 0)
				{
					setBoundary(orderIndex, false, null, null);
					continue;
				}
				Material material = materialColumn[renderIndex];
				Texture texture = textureColumn[renderIndex];
				bool boundary = !hasPrevious || previousMaterial != material || previousTexture != texture;
				setBoundary(orderIndex, boundary, material, texture);
				hasPrevious = true;
				previousMaterial = material;
				previousTexture = texture;
			}
			int nextMember = findNextBatchMemberIndex(renderElements.Count, end, useDrawOrder);
			if (nextMember < 0)
			{
				continue;
			}
			int nextRenderIndex = useDrawOrder ? getRenderIndexForDrawIndex(nextMember) : nextMember;
			Material nextMaterial = materialColumn[nextRenderIndex];
			Texture nextTexture = textureColumn[nextRenderIndex];
			bool nextBoundary = !hasPrevious || previousMaterial != nextMaterial || previousTexture != nextTexture;
			setBoundary(nextMember, nextBoundary, nextMaterial, nextTexture);
		}
	}
	// Cursor算法适合Range较多的情况，通过一次线性游标合并避免重复二分查找。
	public void rebuildBatchRunsCursorLinear(List<FastUIRenderElement> renderElements, Material defaultMaterial, bool useDrawOrder)
	{
		mExpandedBatchRefreshRanges.Clear();
		mBatchRunMergeBuffer.Clear();
		if (renderElements.Count == 0)
		{
			mBatchRuns.Clear();
			return;
		}
		ensureElementBatchStates(renderElements, defaultMaterial);
		var materialColumn = mElementBatchStates.getMaterialColumn();
		var textureColumn = mElementBatchStates.getTextureColumn();
		var memberColumn = mElementBatchStates.getMemberColumn();
		var pendingStarts = mPendingBatchRefreshRanges.getStartColumn();
		var pendingEnds = mPendingBatchRefreshRanges.getEndColumn();
		for (int i = 0; i < mPendingBatchRefreshRanges.Count; ++i)
		{
			int start = Mathf.Clamp(pendingStarts[i], 0, renderElements.Count);
			int end = Mathf.Clamp(pendingEnds[i], start, renderElements.Count);
			if (end <= start)
			{
				continue;
			}
			int nextMember = findNextBatchMemberIndex(renderElements.Count, end, useDrawOrder);
			int expandedEnd = nextMember >= 0 ? nextMember + 1 : renderElements.Count;
			if (mExpandedBatchRefreshRanges.Count > 0)
			{
				int lastIndex = mExpandedBatchRefreshRanges.Count - 1;
				var expandedEnds0 = mExpandedBatchRefreshRanges.getEndColumn();
				if (start <= expandedEnds0[lastIndex])
				{
					if (expandedEnd > expandedEnds0[lastIndex])
					{
						expandedEnds0[lastIndex] = expandedEnd;
					}
					continue;
				}
			}
			mExpandedBatchRefreshRanges.Add(new FastUIRangeData(start, expandedEnd));
		}
		var batchStarts = mBatchRuns.getRenderStartColumn();
		var batchMaterials = mBatchRuns.getMaterialColumn();
		var batchTextures = mBatchRuns.getTextureColumn();
		int oldCursor = 0;
		var expandedStarts = mExpandedBatchRefreshRanges.getStartColumn();
		var expandedEnds = mExpandedBatchRefreshRanges.getEndColumn();
		for (int rangeIndex = 0; rangeIndex < mExpandedBatchRefreshRanges.Count; ++rangeIndex)
		{
			int rangeStart = expandedStarts[rangeIndex];
			int rangeEnd = expandedEnds[rangeIndex];
			while (oldCursor < mBatchRuns.Count && batchStarts[oldCursor] < rangeStart)
			{
				mBatchRunMergeBuffer.Add(new FastUIBatchRunData(batchStarts[oldCursor], batchMaterials[oldCursor], batchTextures[oldCursor]));
				++oldCursor;
			}
			bool hasPrevious = mBatchRunMergeBuffer.Count > 0;
			Material previousMaterial = hasPrevious ? mBatchRunMergeBuffer[^1].mMaterial : null;
			Texture previousTexture = hasPrevious ? mBatchRunMergeBuffer[^1].mTexture : null;
			while (oldCursor < mBatchRuns.Count && batchStarts[oldCursor] < rangeEnd)
			{
				++oldCursor;
			}
			for (int orderIndex = rangeStart; orderIndex < rangeEnd; ++orderIndex)
			{
				int renderIndex = useDrawOrder ? getRenderIndexForDrawIndex(orderIndex) : orderIndex;
				if ((uint)renderIndex >= (uint)mElementBatchStates.Count || memberColumn[renderIndex] == 0)
				{
					continue;
				}
				Material material = materialColumn[renderIndex];
				Texture texture = textureColumn[renderIndex];
				if (!hasPrevious || previousMaterial != material || previousTexture != texture)
				{
					mBatchRunMergeBuffer.Add(new FastUIBatchRunData(orderIndex, material, texture));
				}
				hasPrevious = true;
				previousMaterial = material;
				previousTexture = texture;
			}
		}
		while (oldCursor < mBatchRuns.Count)
		{
			mBatchRunMergeBuffer.Add(new FastUIBatchRunData(batchStarts[oldCursor], batchMaterials[oldCursor], batchTextures[oldCursor]));
			++oldCursor;
		}
		(mBatchRunMergeBuffer, mBatchRuns) = (mBatchRuns, mBatchRunMergeBuffer);
		mBatchRunMergeBuffer.Clear();
	}
	private bool tryFindPreviousBatchKey(
		int renderCount,
		int startIndex,
		bool useDrawOrder,
		out Material material,
		out Texture texture
	)
	{
		var materialColumn = mElementBatchStates.getMaterialColumn();
		var textureColumn = mElementBatchStates.getTextureColumn();
		var memberColumn = mElementBatchStates.getMemberColumn();
		for (int orderIndex = Mathf.Min(startIndex, renderCount - 1); orderIndex >= 0; --orderIndex)
		{
			int renderIndex = useDrawOrder ? getRenderIndexForDrawIndex(orderIndex) : orderIndex;
			if ((uint)renderIndex >= (uint)mElementBatchStates.Count || memberColumn[renderIndex] == 0)
			{
				continue;
			}
			material = materialColumn[renderIndex];
			texture = textureColumn[renderIndex];
			return true;
		}
		material = null;
		texture = null;
		return false;
	}
	private void setBoundary(int renderIndex, bool shouldExist, Material material, Texture texture)
	{
		int listIndex = findBoundaryListIndex(renderIndex, out bool found);
		if (!shouldExist)
		{
			if (found)
			{
				mBatchRuns.RemoveAt(listIndex);
			}
			return;
		}
		if (found)
		{
			var materials = mBatchRuns.getMaterialColumn();
			var textures = mBatchRuns.getTextureColumn();
			if (materials[listIndex] == material && textures[listIndex] == texture)
			{
				return;
			}
			materials[listIndex] = material;
			textures[listIndex] = texture;
			return;
		}
		mBatchRuns.Insert(listIndex, new FastUIBatchRunData(renderIndex, material, texture));
	}
	private int findBoundaryListIndex(int renderIndex, out bool found)
	{
		var renderStarts = mBatchRuns.getRenderStartColumn();
		int low = 0;
		int high = mBatchRuns.Count;
		while (low < high)
		{
			int mid = (low + high) >> 1;
			if (renderStarts[mid] < renderIndex)
			{
				low = mid + 1;
			}
			else
			{
				high = mid;
			}
		}
		found = low < mBatchRuns.Count && renderStarts[low] == renderIndex;
		return low;
	}
	private int findNextBatchMemberIndex(int renderCount, int startIndex, bool useDrawOrder)
	{
		var memberColumn = mElementBatchStates.getMemberColumn();
		for (int orderIndex = Mathf.Max(startIndex, 0); orderIndex < renderCount; ++orderIndex)
		{
			int renderIndex = useDrawOrder ? getRenderIndexForDrawIndex(orderIndex) : orderIndex;
			if ((uint)renderIndex < (uint)mElementBatchStates.Count && memberColumn[renderIndex] != 0)
			{
				return orderIndex;
			}
		}
		return -1;
	}
	private int buildRenderState(FastUIRenderElement element)
	{
		int state = 0;
		if (element.isRenderActive())
		{
			state |= RENDER_STATE_ACTIVE;
		}
		if (element.getVisible())
		{
			state |= RENDER_STATE_VISIBLE;
		}
		if (element.isCulled())
		{
			state |= RENDER_STATE_CULLED;
		}
		if (element != null && element.isClipCullOutside())
		{
			state |= RENDER_STATE_CLIP_OUTSIDE;
		}
		return state;
	}
	private void ensureIndexHiddenWordCount(int renderCount)
	{
		int wordCount = (renderCount + 31) >> 5;
		while (mIndexHiddenWordGenerations.Count < wordCount)
		{
			mIndexHiddenWordGenerations.Add(0);
			mIndexHiddenWordBits.Add(0);
		}
	}
	private void advanceIndexHiddenGeneration()
	{
		if (mIndexHiddenGeneration < int.MaxValue)
		{
			++mIndexHiddenGeneration;
			return;
		}
		var generationColumn = mIndexHiddenWordGenerations.getValueColumn();
		for (int i = 0; i < mIndexHiddenWordGenerations.Count; ++i)
		{
			generationColumn[i] = 0;
		}
		mIndexHiddenGeneration = 1;
	}
	public bool isElementBatchMember(FastUIRenderElement element)
	{
		return element != null && element.getCanvas() == mOwner && element.getVertexSlot() >= 0;
	}
	private bool ensureSubMeshCapacity(int requiredBatchCount)
	{
		int requiredCount = Mathf.Max(requiredBatchCount, 1);
		int requiredCapacity = Mathf.NextPowerOfTwo(requiredCount);
		bool changed = false;
		if (requiredCapacity > mSubMeshCapacity)
		{
			mSubMeshCapacity = requiredCapacity;
			changed = true;
		}
		// Capacity是增长策略，不等于当前需要物化的Descriptor数量。
		// 例如6001 Batch会保留8192容量，但只创建6001条有效ECS记录，避免首次Flush白算2191个空Descriptor。
		mSubMeshDescriptors.EnsureCapacity(mSubMeshCapacity);
		int vertexSpan = mVertexStreams.getVertexSpan();
		while (mSubMeshDescriptors.Count < requiredCount)
		{
			mSubMeshDescriptors.Add(new FastUISubMeshDescriptorData
			{
				mValue = createEmptySubMeshDescriptor(vertexSpan)
			});
		}
		return changed;
	}
	public int setHiddenRenderRanges(FastUIRangeData_ECSList drawHiddenRanges, FastUIRangeData_ECSList indexHiddenRanges)
	{
		mHiddenRenderRanges.Clear();
		mIndexHiddenRenderRangeCount = 0;
		mLastIndexHiddenWordWriteCount = 0;
		if (drawHiddenRanges != null)
		{
			var sourceStarts = drawHiddenRanges.getStartColumn();
			var sourceEnds = drawHiddenRanges.getEndColumn();
			for (int i = 0; i < drawHiddenRanges.Count; ++i)
			{
				int start = sourceStarts[i];
				int end = sourceEnds[i];
				if (end > start)
				{
					mHiddenRenderRanges.Add(new FastUIRangeData(start, end));
				}
			}
		}
		int renderCount = mElementBatchStates.Count;
		mHasIndexHiddenRanges = indexHiddenRanges != null && indexHiddenRanges.Count > 0 && renderCount > 0;
		if (!mHasIndexHiddenRanges)
		{
			return 0;
		}
		ensureIndexHiddenWordCount(renderCount);
		advanceIndexHiddenGeneration();
		int markedCount = 0;
		var sourceIndexStarts = indexHiddenRanges.getStartColumn();
		var sourceIndexEnds = indexHiddenRanges.getEndColumn();
		var wordGenerationColumn = mIndexHiddenWordGenerations.getValueColumn();
		var wordBitsColumn = mIndexHiddenWordBits.getValueColumn();
		for (int i = 0; i < indexHiddenRanges.Count; ++i)
		{
			int start = Mathf.Clamp(sourceIndexStarts[i], 0, renderCount);
			int end = Mathf.Clamp(sourceIndexEnds[i], start, renderCount);
			if (end <= start)
			{
				continue;
			}
			++mIndexHiddenRenderRangeCount;
			markedCount += end - start;
			int firstWord = start >> 5;
			int lastWord = (end - 1) >> 5;
			int firstMask = -1 << (start & 31);
			int endBit = end & 31;
			int lastMask = endBit == 0 ? -1 : (int)((1u << endBit) - 1u);
			if (firstWord == lastWord)
			{
				int mask = firstMask & lastMask;
				if (wordGenerationColumn[firstWord] != mIndexHiddenGeneration)
				{
					wordGenerationColumn[firstWord] = mIndexHiddenGeneration;
					wordBitsColumn[firstWord] = mask;
				}
				else
				{
					wordBitsColumn[firstWord] |= mask;
				}
				++mLastIndexHiddenWordWriteCount;
				continue;
			}
			if (wordGenerationColumn[firstWord] != mIndexHiddenGeneration)
			{
				wordGenerationColumn[firstWord] = mIndexHiddenGeneration;
				wordBitsColumn[firstWord] = firstMask;
			}
			else
			{
				wordBitsColumn[firstWord] |= firstMask;
			}
			++mLastIndexHiddenWordWriteCount;
			for (int wordIndex = firstWord + 1; wordIndex < lastWord; ++wordIndex)
			{
				wordGenerationColumn[wordIndex] = mIndexHiddenGeneration;
				wordBitsColumn[wordIndex] = -1;
				++mLastIndexHiddenWordWriteCount;
			}
			if (wordGenerationColumn[lastWord] != mIndexHiddenGeneration)
			{
				wordGenerationColumn[lastWord] = mIndexHiddenGeneration;
				wordBitsColumn[lastWord] = lastMask;
			}
			else
			{
				wordBitsColumn[lastWord] |= lastMask;
			}
			++mLastIndexHiddenWordWriteCount;
		}
		return markedCount;
	}

	// HiddenRange存在时DrawRun需要裁剪；无HiddenRange时可以继续使用DirectBatchRun快速路径。
	public void refreshVisibility(Material defaultMaterial, int renderElementCount, ref FastUIFrameStats stats)
	{
		applyBatchRuns(defaultMaterial, renderElementCount, ref stats);
		stats.mVisibilityRebuilt = true;
	}
	// Index容量增长只改变DrawIndex对应的IndexStart，不改变Batch边界。
	// 这里直接复用既有BatchRun/DrawRun重新计算Descriptor，避免重新扫描RenderElement构建Batch。
	public void refreshIndexLayout(Material defaultMaterial, int renderElementCount, ref FastUIFrameStats stats)
	{
		applyBatchRuns(defaultMaterial, renderElementCount, ref stats);
		stats.mBatchRebuilt = true;
	}
	// 把逻辑BatchRun提交到Unity SubMesh/Material/Texture，SubMesh使用冻结的Partial/Bulk阈值。
	public void applyBatchRuns(Material defaultMaterial, int renderElementCount, ref FastUIFrameStats stats)
	{
		bool hasHiddenRange = mHiddenRenderRanges.Count > 0;
		int drawRunCount;
		var batchStartColumn = mBatchRuns.getRenderStartColumn();
		var batchMaterialColumn = mBatchRuns.getMaterialColumn();
		var batchTextureColumn = mBatchRuns.getTextureColumn();
		mLimitedClipDrawRunsActive = false;
		mLastLimitedClipExtraDrawRunCount = 0;
		mLastLimitedClipSavedIndexCount = 0;
		// Clip Submit Range Culling只改SubMesh范围，不修改Index/Vertex Arena。
		// Visibility HiddenRange存在时仍沿用原DrawRun路径，避免两套分裂规则叠加导致DrawCall爆炸。
		if (!hasHiddenRange && shouldTryLimitedClipDrawRuns(renderElementCount))
		{
			mLimitedClipDrawRunsActive = buildLimitedClipDrawRuns(renderElementCount);
		}
		bool directDraw = !hasHiddenRange && !mLimitedClipDrawRunsActive;
		if (directDraw)
		{
			drawRunCount = renderElementCount > 0 ? mBatchRuns.Count : 0;
			while (drawRunCount > 0 && batchStartColumn[drawRunCount - 1] >= renderElementCount)
			{
				--drawRunCount;
			}
		}
		else
		{
			if (hasHiddenRange)
			{
				buildDrawRuns(renderElementCount);
			}
			drawRunCount = mDrawRuns.Count;
		}
		var drawStartColumn = mDrawRuns.getRenderStartColumn();
		var drawEndColumn = mDrawRuns.getRenderEndColumn();
		var drawMaterialColumn = mDrawRuns.getMaterialColumn();
		var drawTextureColumn = mDrawRuns.getTextureColumn();
		int activeSubMeshCount = Mathf.Max(drawRunCount, 1);
		bool activeSubMeshCountChanged = activeSubMeshCount != mAppliedMeshSubMeshCount;
		bool subMeshLayoutReset;
		bool materialDirty;
		if (directDraw)
		{
			subMeshLayoutReset = ensureSubMeshCapacity(drawRunCount);
			var descriptorColumn = mSubMeshDescriptors.getValueColumn();
			materialDirty = resizeAppliedBatchSlots(drawRunCount, defaultMaterial);
			int vertexCount = mVertexStreams.getVertexSpan();
			mChangedSubMeshSlots.Clear();
			mChangedTextureSlots.Clear();
			for (int i = 0; i < drawRunCount; ++i)
			{
				int renderStart = batchStartColumn[i];
				int renderEnd = i + 1 < drawRunCount ? batchStartColumn[i + 1] : renderElementCount;
				SubMeshDescriptor descriptor = createSubMeshDescriptor(renderStart, renderEnd, vertexCount);
				if (!isSameSubMeshDescriptor(descriptorColumn[i], descriptor))
				{
					descriptorColumn[i] = descriptor;
					mChangedSubMeshSlots.Add(i);
				}
				Material material = batchMaterialColumn[i] != null ? batchMaterialColumn[i] : defaultMaterial;
				if (mSharedMaterials[i] != material)
				{
					mSharedMaterials[i] = material;
					materialDirty = true;
				}
				Texture texture = batchTextureColumn[i];
				if (mAppliedTextures[i] != texture)
				{
					mAppliedTextures[i] = texture;
					mChangedTextureSlots.Add(i);
				}
			}
			int clearEnd = Mathf.Min(Mathf.Max(mAppliedDrawRunCount, drawRunCount), mSubMeshDescriptors.Count);
			for (int i = drawRunCount; i < clearEnd; ++i)
			{
				SubMeshDescriptor descriptor = createEmptySubMeshDescriptor(vertexCount);
				if (!isSameSubMeshDescriptor(descriptorColumn[i], descriptor))
				{
					descriptorColumn[i] = descriptor;
					mChangedSubMeshSlots.Add(i);
				}
			}
		}
		else
		{
			subMeshLayoutReset = ensureSubMeshCapacity(drawRunCount);
			var descriptorColumn = mSubMeshDescriptors.getValueColumn();
			materialDirty = resizeAppliedBatchSlots(drawRunCount, defaultMaterial);
			int vertexCount = mVertexStreams.getVertexSpan();
			mChangedSubMeshSlots.Clear();
			for (int i = 0; i < drawRunCount; ++i)
			{
				SubMeshDescriptor descriptor = createSubMeshDescriptor(drawStartColumn[i], drawEndColumn[i], vertexCount);
				if (!isSameSubMeshDescriptor(descriptorColumn[i], descriptor))
				{
					descriptorColumn[i] = descriptor;
					mChangedSubMeshSlots.Add(i);
				}
			}
			int clearEnd = Mathf.Min(Mathf.Max(mAppliedDrawRunCount, drawRunCount), mSubMeshDescriptors.Count);
			for (int i = drawRunCount; i < clearEnd; ++i)
			{
				SubMeshDescriptor descriptor = createEmptySubMeshDescriptor(vertexCount);
				if (!isSameSubMeshDescriptor(descriptorColumn[i], descriptor))
				{
					descriptorColumn[i] = descriptor;
					mChangedSubMeshSlots.Add(i);
				}
			}
		}

		applyChangedSubMeshes(subMeshLayoutReset || activeSubMeshCountChanged, activeSubMeshCount);
		mAppliedMeshSubMeshCount = activeSubMeshCount;

		// DirectBatchRun已经在Fused阶段同步Material；这里只有HiddenRange的DrawRun需要逐项检查。
		if (!directDraw)
		{
			for (int i = 0; i < drawRunCount; ++i)
			{
				Material material = drawMaterialColumn[i] != null ? drawMaterialColumn[i] : defaultMaterial;
				if (mSharedMaterials[i] != material)
				{
					mSharedMaterials[i] = material;
					materialDirty = true;
				}
			}
		}
		if (materialDirty)
		{
			mMeshRenderer.SetSharedMaterials(mSharedMaterials);
		}

		bool forceTextureRebind = mForceTextureRebind || materialDirty || subMeshLayoutReset;
		if (directDraw)
		{
			if (forceTextureRebind)
			{
				for (int i = 0; i < drawRunCount; ++i)
				{
					mPropertyBlock.Clear();
					mPropertyBlock.SetTexture(MAIN_TEX_ID, mAppliedTextures[i]);
					mMeshRenderer.SetPropertyBlock(mPropertyBlock, i);
				}
			}
			else
			{
				var changedTextureSlotColumn = mChangedTextureSlots.getValueColumn();
				for (int i = 0; i < mChangedTextureSlots.Count; ++i)
				{
					int slot = changedTextureSlotColumn[i];
					mPropertyBlock.Clear();
					mPropertyBlock.SetTexture(MAIN_TEX_ID, mAppliedTextures[slot]);
					mMeshRenderer.SetPropertyBlock(mPropertyBlock, slot);
				}
			}
		}
		else
		{
			for (int i = 0; i < drawRunCount; ++i)
			{
				Texture texture = drawTextureColumn[i];
				if (!forceTextureRebind && mAppliedTextures[i] == texture)
				{
					continue;
				}
				mAppliedTextures[i] = texture;
				mPropertyBlock.Clear();
				mPropertyBlock.SetTexture(MAIN_TEX_ID, texture);
				mMeshRenderer.SetPropertyBlock(mPropertyBlock, i);
			}
		}

		mForceTextureRebind = false;

		mMeshRenderer.enabled = drawRunCount > 0;
		mAppliedDrawRunCount = drawRunCount;
		mCurrentBatchCount = drawRunCount;
		stats.mBatchCount = drawRunCount;
	}
	// Changed=1且Active SubMesh数量没有变化时才逐项Partial；
	// Active数量变化必须Bulk提交，因为Mesh.subMeshCount必须和当前DrawRun数量同步收缩/增长。
	private void applyChangedSubMeshes(bool forceBulkApply, int activeSubMeshCount)
	{
		int changedCount = mChangedSubMeshSlots.Count;
		if (!forceBulkApply && changedCount == 0)
		{
			return;
		}
		bool usePartialApply = !forceBulkApply && changedCount <= MAX_PARTIAL_SUBMESH_APPLY_COUNT;
		MeshUpdateFlags flags = MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontNotifyMeshUsers;
		if (usePartialApply)
		{
			var changedSlotColumn = mChangedSubMeshSlots.getValueColumn();
			var descriptorColumn = mSubMeshDescriptors.getValueColumn();
			for (int i = 0; i < changedCount; ++i)
			{
				int slot = changedSlotColumn[i];
				mMesh.SetSubMesh(slot, descriptorColumn[slot], flags);
			}
			return;
		}
		applyAllSubMeshesDirect(flags, activeSubMeshCount);
	}
	private void applyAllSubMeshesDirect(MeshUpdateFlags flags, int activeSubMeshCount)
	{
		int applyCount = Mathf.Max(activeSubMeshCount, 1);
		if (mSubMeshDescriptors.Count < applyCount)
		{
			return;
		}
		var descriptorColumn = mSubMeshDescriptors.getValueColumn();
		ref SubMeshDescriptor first = ref descriptorColumn[0];
		NativeArray<SubMeshDescriptor> view = FastUINativeArrayBridge.createView(ref first, applyCount);
		// SetSubMeshes会把Mesh.subMeshCount同步成applyCount。
		// 这是Active数量，而不是历史最大Capacity，避免Unity把多余Material Slot重复绘制到最后一个有效SubMesh。
		mMesh.SetSubMeshes(view, 0, applyCount, flags);
	}

	private bool resizeAppliedBatchSlots(int drawRunCount, Material defaultMaterial)
	{
		int requiredMaterialSlots = Mathf.Max(drawRunCount, 1);
		int oldCount = mSharedMaterials.Count;
		if (oldCount == requiredMaterialSlots)
		{
			return false;
		}
		if (oldCount < requiredMaterialSlots)
		{
			// 一次预留最终Slot容量，碎片化Clone不再让两个Managed List反复几何扩容。
			int capacity = Mathf.NextPowerOfTwo(requiredMaterialSlots);
			if (mSharedMaterials.Capacity < capacity)
			{
				mSharedMaterials.Capacity = capacity;
			}
			if (mAppliedTextures.Capacity < capacity)
			{
				mAppliedTextures.Capacity = capacity;
			}
			while (mSharedMaterials.Count < requiredMaterialSlots)
			{
				mSharedMaterials.Add(defaultMaterial);
				mAppliedTextures.Add(null);
			}
		}
		else
		{
			int removeCount = oldCount - requiredMaterialSlots;
			mSharedMaterials.RemoveRange(requiredMaterialSlots, removeCount);
			mAppliedTextures.RemoveRange(requiredMaterialSlots, removeCount);
		}
		// List.Capacity不会因为RemoveRange收缩，因此这里既修复实际提交Slot数量，又保留历史分配容量避免GC。
		return true;
	}

	private bool shouldTryLimitedClipDrawRuns(int renderElementCount)
	{
		return mClipSubmitRangeCullingEnabled && renderElementCount > 0 && mBatchRuns.Count > 0 && mDrawOrder != null &&
			mOwner != null && mOwner.getClipCullOutsideCount() > 0;
	}
	//  Limited Visible Runs：
	// 1. 先保留的Batch首尾Clip裁剪；
	// 2. 再扫描首尾之间的连续Outside Hole；
	// 3. 全Canvas只保留收益最大的4个Hole，且每个至少节省4096个Index；
	// 4. 只拆SubMesh Descriptor，不修改CPU/GPU Index Buffer，因此不会重新引入IBUpload。
	private bool buildLimitedClipDrawRuns(int renderElementCount)
	{
		mDrawRuns.Clear();
		mClipHoleBatchIndices.Clear();
		mClipHoleStarts.Clear();
		mClipHoleEnds.Clear();
		mClipHoleSavedIndexCounts.Clear();
		int batchCount = mBatchRuns.Count;
		if (batchCount <= 0 || renderElementCount <= 0 || mDrawOrder == null)
		{
			return false;
		}
		mClipTrimmedBatchStarts.EnsureCount(batchCount);
		mClipTrimmedBatchEnds.EnsureCount(batchCount);
		var trimmedStarts = mClipTrimmedBatchStarts.getValueColumn();
		var trimmedEnds = mClipTrimmedBatchEnds.getValueColumn();
		var batchStarts = mBatchRuns.getRenderStartColumn();
		for (int batchIndex = 0; batchIndex < batchCount; ++batchIndex)
		{
			int rawStart = Mathf.Clamp(batchStarts[batchIndex], 0, renderElementCount);
			int rawEnd = Mathf.Clamp(batchIndex + 1 < batchCount ? batchStarts[batchIndex + 1] : renderElementCount, rawStart, renderElementCount);
			int trimmedStart = rawStart;
			int trimmedEnd = rawEnd;
			while (trimmedStart < trimmedEnd && isClipOutsideDrawIndex(trimmedStart))
			{
				++trimmedStart;
			}
			while (trimmedEnd > trimmedStart && isClipOutsideDrawIndex(trimmedEnd - 1))
			{
				--trimmedEnd;
			}
			trimmedStarts[batchIndex] = trimmedStart;
			trimmedEnds[batchIndex] = trimmedEnd;
			if (trimmedEnd - trimmedStart <= 1)
			{
				continue;
			}
			int cursor = trimmedStart + 1;
			while (cursor < trimmedEnd - 1)
			{
				while (cursor < trimmedEnd - 1 && !isClipOutsideDrawIndex(cursor))
				{
					++cursor;
				}
				if (cursor >= trimmedEnd - 1)
				{
					break;
				}
				int holeStart = cursor;
				while (cursor < trimmedEnd && isClipOutsideDrawIndex(cursor))
				{
					++cursor;
				}
				int holeEnd = cursor;
				if (holeStart <= trimmedStart || holeEnd >= trimmedEnd)
				{
					continue;
				}
				int holeIndexStart = mDrawOrder.getIndexStartForDrawIndex(holeStart);
				int holeIndexEnd = mDrawOrder.getIndexStartForDrawIndex(holeEnd);
				int savedIndexCount = Mathf.Max(holeIndexEnd - holeIndexStart, 0);
				considerLimitedClipHole(batchIndex, holeStart, holeEnd, savedIndexCount);
			}
		}
		if (mClipHoleBatchIndices.Count <= 0)
		{
			mDrawRuns.Clear();
			return false;
		}
		var batchMaterials = mBatchRuns.getMaterialColumn();
		var batchTextures = mBatchRuns.getTextureColumn();
		for (int batchIndex = 0; batchIndex < batchCount; ++batchIndex)
		{
			int runStart = trimmedStarts[batchIndex];
			int runEnd = trimmedEnds[batchIndex];
			if (runEnd <= runStart)
			{
				continue;
			}
			Material material = batchMaterials[batchIndex];
			Texture texture = batchTextures[batchIndex];
			int cursor = runStart;
			while (cursor < runEnd)
			{
				int nextHoleIndex = findNextSelectedClipHole(batchIndex, cursor, runEnd);
				if (nextHoleIndex < 0)
				{
					mDrawRuns.Add(new FastUIDrawRunData(cursor, runEnd, material, texture));
					break;
				}
				int holeStart = mClipHoleStarts[nextHoleIndex];
				int holeEnd = mClipHoleEnds[nextHoleIndex];
				if (holeStart > cursor)
				{
					mDrawRuns.Add(new FastUIDrawRunData(cursor, holeStart, material, texture));
				}
				cursor = Mathf.Max(cursor, holeEnd);
			}
		}
		mLastLimitedClipExtraDrawRunCount = Mathf.Max(mDrawRuns.Count - countNonEmptyTrimmedBatches(batchCount), 0);
		mLastLimitedClipSavedIndexCount = 0;
		var savedIndices = mClipHoleSavedIndexCounts.getValueColumn();
		for (int i = 0; i < mClipHoleSavedIndexCounts.Count; ++i)
		{
			mLastLimitedClipSavedIndexCount += savedIndices[i];
		}
		return mLastLimitedClipExtraDrawRunCount > 0;
	}
	//  Render弱项诊断：只保留Batch首尾裁剪，不拆内部Outside Hole。默认关闭此分支，Production仍执行原路径。

	private bool isClipOutsideDrawIndex(int drawIndex)
	{
		int renderIndex = mDrawOrder != null ? mDrawOrder.getRenderIndexForDrawIndex(drawIndex) : drawIndex;
		return isElementClipOutside(renderIndex);
	}
	private int countNonEmptyTrimmedBatches(int batchCount)
	{
		int count = 0;
		var starts = mClipTrimmedBatchStarts.getValueColumn();
		var ends = mClipTrimmedBatchEnds.getValueColumn();
		for (int i = 0; i < batchCount; ++i)
		{
			if (ends[i] > starts[i])
			{
				++count;
			}
		}
		return count;
	}
	private void considerLimitedClipHole(int batchIndex, int holeStart, int holeEnd, int savedIndexCount)
	{
		if (holeEnd <= holeStart || savedIndexCount < MIN_CLIP_HOLE_SAVED_INDEX_COUNT)
		{
			return;
		}
		int count = mClipHoleSavedIndexCounts.Count;
		if (count < MAX_EXTRA_CLIP_DRAW_RUNS)
		{
			mClipHoleBatchIndices.Add(batchIndex);
			mClipHoleStarts.Add(holeStart);
			mClipHoleEnds.Add(holeEnd);
			mClipHoleSavedIndexCounts.Add(savedIndexCount);
			return;
		}
		var saved = mClipHoleSavedIndexCounts.getValueColumn();
		int smallestIndex = 0;
		int smallestSaved = saved[0];
		for (int i = 1; i < count; ++i)
		{
			if (saved[i] < smallestSaved)
			{
				smallestSaved = saved[i];
				smallestIndex = i;
			}
		}
		if (savedIndexCount <= smallestSaved)
		{
			return;
		}
		mClipHoleBatchIndices[smallestIndex] = batchIndex;
		mClipHoleStarts[smallestIndex] = holeStart;
		mClipHoleEnds[smallestIndex] = holeEnd;
		mClipHoleSavedIndexCounts[smallestIndex] = savedIndexCount;
	}
	private int findNextSelectedClipHole(int batchIndex, int cursor, int runEnd)
	{
		int result = -1;
		int bestStart = int.MaxValue;
		var batchIndices = mClipHoleBatchIndices.getValueColumn();
		var starts = mClipHoleStarts.getValueColumn();
		var ends = mClipHoleEnds.getValueColumn();
		for (int i = 0; i < mClipHoleBatchIndices.Count; ++i)
		{
			if (batchIndices[i] != batchIndex || starts[i] < cursor || ends[i] > runEnd || starts[i] >= bestStart)
			{
				continue;
			}
			bestStart = starts[i];
			result = i;
		}
		return result;
	}
	private SubMeshDescriptor createSubMeshDescriptor(int renderStart, int renderEnd, int vertexCount)
	{
		// Clip-aware Batch Window只裁掉Batch两端连续、完全位于RectMask外的Draw。
		// 不改CPU/GPU Index Buffer；Compact模式持续启用，Stable模式只在Canvas稳定帧启用，动态整段平移时回退Stencil全提交。
		if (mClipSubmitRangeCullingEnabled && mDrawOrder != null && renderEnd > renderStart)
		{
			while (renderStart < renderEnd)
			{
				int sourceRenderIndex = mDrawOrder.getRenderIndexForDrawIndex(renderStart);
				if (!isElementClipOutside(sourceRenderIndex))
				{
					break;
				}
				++renderStart;
			}
			while (renderEnd > renderStart)
			{
				int sourceRenderIndex = mDrawOrder.getRenderIndexForDrawIndex(renderEnd - 1);
				if (!isElementClipOutside(sourceRenderIndex))
				{
					break;
				}
				--renderEnd;
			}
		}
		// BatchRun保存的是DrawIndex边界。可变几何后每个Draw占用的Index数量不同，
		// 因此SubMesh必须通过DrawOrder维护的Index前缀表取得真实Index区间。
		int indexStart = mDrawOrder != null ? mDrawOrder.getIndexStartForDrawIndex(renderStart) : 0;
		//  Submit Arena允许Batch尾部保留增长Reserve，但Reserve不应进入SubMesh实际绘制。
		int indexEnd = mDrawOrder != null ? mDrawOrder.getIndexEndForDrawIndex(renderEnd) : indexStart;
		int indexCount = Mathf.Max(indexEnd - indexStart, 0);
		// Index Stream保存的是全局绝对Vertex Index，baseVertex始终为0。
		// SubMeshDescriptor.firstVertex/vertexCount只是声明该SubMesh允许访问的Mesh Vertex范围，
		// 必须以Unity当前真实Vertex Buffer容量为准，不能使用逻辑mVertexSpan。
		// mVertexSpan会随Geometry Range分配/回收变化，而Mesh Vertex Buffer只按2次幂扩容且不会同步收缩；
		// 两者瞬时不一致时把mVertexSpan写进Descriptor会触发Unity SetSubMeshes的lastVertex断言。
		int descriptorVertexCount = getSafeMeshVertexCount(vertexCount);
		return new SubMeshDescriptor(indexStart, indexCount, MeshTopology.Triangles)
		{
			bounds = getSubmitBounds(),
			firstVertex = 0,
			vertexCount = descriptorVertexCount,
		};
	}
	private SubMeshDescriptor createEmptySubMeshDescriptor(int vertexCount)
	{
		return new SubMeshDescriptor(0, 0, MeshTopology.Triangles)
		{
			bounds = getSubmitBounds(),
			firstVertex = 0,
			vertexCount = getSafeMeshVertexCount(vertexCount)
		};
	}
	private Bounds getSubmitBounds()
	{
#if UNITY_EDITOR
		// In the Editor, including Play Mode, the renderer owns a transform-safe Mesh bounds.
		// Reuse it for every SubMesh so a descriptor cannot recreate an invalid worldAABB.
		return mMesh != null ? mMesh.bounds : new Bounds(Vector3.zero, Vector3.one);
#else
		return FastUIMeshUtility.HUGE_BOUNDS;
#endif
	}
	private int getSafeMeshVertexCount(int fallbackVertexCount)
	{
		int meshVertexCount = mMesh != null ? mMesh.vertexCount : 0;
		return meshVertexCount > 0 ? meshVertexCount : Mathf.Max(fallbackVertexCount, 0);
	}
	private bool isSameSubMeshDescriptor(SubMeshDescriptor a, SubMeshDescriptor b)
	{
		return a.indexStart == b.indexStart && a.indexCount == b.indexCount && a.topology == b.topology && a.firstVertex == b.firstVertex &&
			a.vertexCount == b.vertexCount;
	}
	private void buildDrawRuns(int renderElementCount)
	{
		mDrawRuns.Clear();
		int batchCount = mBatchRuns.Count;
		if (batchCount == 0 || renderElementCount <= 0)
		{
			return;
		}
		var renderStarts = mBatchRuns.getRenderStartColumn();
		var materials = mBatchRuns.getMaterialColumn();
		var textures = mBatchRuns.getTextureColumn();
		if (mHiddenRenderRanges.Count == 0)
		{
			for (int i = 0; i < batchCount; ++i)
			{
				int runStart = renderStarts[i];
				int endRender = i + 1 < batchCount ? renderStarts[i + 1] : renderElementCount;
				if (endRender > runStart)
				{
					mDrawRuns.Add(new FastUIDrawRunData(runStart, endRender, materials[i], textures[i]));
				}
			}
			return;
		}
		var hiddenStarts = mHiddenRenderRanges.getStartColumn();
		var hiddenEnds = mHiddenRenderRanges.getEndColumn();
		int hiddenIndex = 0;
		for (int i = 0; i < batchCount; ++i)
		{
			int runStart = renderStarts[i];
			int runEnd = i + 1 < batchCount ? renderStarts[i + 1] : renderElementCount;
			Material runMaterial = materials[i];
			Texture runTexture = textures[i];
			if (runEnd <= runStart)
			{
				continue;
			}
			while (hiddenIndex < mHiddenRenderRanges.Count && hiddenEnds[hiddenIndex] <= runStart)
			{
				++hiddenIndex;
			}
			int cursor = runStart;
			int rangeIndex = hiddenIndex;
			while (rangeIndex < mHiddenRenderRanges.Count)
			{
				int hiddenStart = hiddenStarts[rangeIndex];
				int hiddenEnd = hiddenEnds[rangeIndex];
				if (hiddenStart >= runEnd)
				{
					break;
				}
				if (hiddenEnd <= cursor)
				{
					++rangeIndex;
					continue;
				}
				if (hiddenStart > cursor)
				{
					int visibleEnd = Mathf.Min(hiddenStart, runEnd);
					if (visibleEnd > cursor)
					{
						mDrawRuns.Add(new FastUIDrawRunData(cursor, visibleEnd, runMaterial, runTexture));
					}
				}
				cursor = Mathf.Max(cursor, hiddenEnd);
				if (cursor >= runEnd)
				{
					break;
				}
				++rangeIndex;
			}
			if (cursor < runEnd)
			{
				mDrawRuns.Add(new FastUIDrawRunData(cursor, runEnd, runMaterial, runTexture));
			}
		}
	}
}
