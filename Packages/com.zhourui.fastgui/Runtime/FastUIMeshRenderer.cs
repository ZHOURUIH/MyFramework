using EasyECS;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public enum FastUIBatchRefreshMode
{
	IncrementalBoundary,
	CursorLinearMerge,
}
public enum FastUIVertexUploadBackend
{
	MeshCPUCopy,
	DirectGPUBuffer,
}

// FastUI底层Mesh渲染Facade。
// 负责Unity Mesh/MeshRenderer生命周期，并组合VertexStream、Batch和DrawOrder三个独立子系统。
// 外部通过该类访问底层渲染能力，具体算法状态由各子系统自己持有。
[DisallowMultipleComponent]
public class FastUIMeshRenderer : MonoBehaviour
{
#if UNITY_EDITOR
	private const float EDITOR_PREVIEW_MIN_WORLD_SIZE = 1024.0f;
	private static readonly Vector3 EDITOR_PLAY_MODE_WORLD_SIZE = new(100000.0f, 100000.0f, 10000.0f);
	private const float EDITOR_PREVIEW_MAX_WORLD_SIZE = 16384.0f;
	private const float EDITOR_PREVIEW_MIN_SCALE = 0.00001f;
	private const float EDITOR_PREVIEW_MAX_LOCAL_VALUE = 10000000.0f;
#endif
	// Renderer只持有三个真正独立的运行时子系统，具体ECS状态不再堆在MonoBehaviour本体中。
	protected FastCanvas mOwner;
	protected MeshFilter mMeshFilter;
	protected MeshRenderer mMeshRenderer;
	protected Mesh mMesh;
	protected MaterialPropertyBlock mPropertyBlock;
	protected FastUIVertexStreamSystem mVertexStreams;
	protected FastUIBatchSystem mBatchSystem;
	protected FastUIDrawOrderSystem mDrawOrder;
	protected int mCurrentActiveElementCount;
	protected bool mInitialized;
	protected bool mSystemsInitialized;
	protected bool mDestroyed;
#if UNITY_EDITOR
	protected bool mDomainUnloadHookRegistered;
	protected bool mDomainUnloading;
#endif
	// 由FastCanvas统一初始化。Unity组件先就绪，再创建依赖Mesh的运行时System。
	public void init(FastCanvas owner)
	{
		mOwner = owner;
		ensureInit();
		ensureSystems();
	}
	public MeshRenderer getRenderer()
	{
		ensureInit();
		return mMeshRenderer;
	}
	public Vector3 getRenderOriginOffset()
	{
		return mOwner != null ? mOwner.getRenderOriginOffset() : Vector3.zero;
	}
	public bool getRenderVisible()
	{
		ensureInit();
		return mMeshRenderer.enabled;
	}
	public void setRenderVisible(bool visible)
	{
		ensureInit();
		if (mMeshRenderer.enabled != visible)
		{
			mMeshRenderer.enabled = visible;
		}
	}
	public string getPositionBackendName()
	{
		return FastUIPositionData_ECSList.BackendName;
	}
	public string getPositionBackendReason()
	{
		return FastUIPositionData_ECSList.BackendReason;
	}
	public bool isPositionUnsafeBackend()
	{
		return FastUIPositionData_ECSList.IsUnsafeBackend;
	}
	public FastUIVertexUploadBackend getVertexUploadBackend()
	{
		ensureSystems();
		return mVertexStreams.getVertexUploadBackend();
	}
	public void setVertexUploadBackend(FastUIVertexUploadBackend backend)
	{
		ensureSystems();
		mVertexStreams.setVertexUploadBackend(backend);
	}
	public int getDirtyRangeMergeGap()
	{
		ensureSystems();
		return mVertexStreams.getDirtyRangeMergeGap();
	}
	public void setDirtyRangeMergeGap(int gap)
	{
		ensureSystems();
		mVertexStreams.setDirtyRangeMergeGap(gap);
	}
	public FastUIDirtyUploadMode getDirtyUploadMode()
	{
		ensureSystems();
		return mVertexStreams.getDirtyUploadMode();
	}
	public void setDirtyUploadMode(FastUIDirtyUploadMode mode)
	{
		ensureSystems();
		mVertexStreams.setDirtyUploadMode(mode);
	}
	public int getUploadCallPenaltyBytes()
	{
		ensureSystems();
		return mVertexStreams.getUploadCallPenaltyBytes();
	}
	public void setUploadCallPenaltyBytes(int bytes)
	{
		ensureSystems();
		mVertexStreams.setUploadCallPenaltyBytes(bytes);
	}
	public int getUV0OnlyUploadCallPenaltyBytes()
	{
		ensureSystems();
		return mVertexStreams.getUV0OnlyUploadCallPenaltyBytes();
	}
	public void setUV0OnlyUploadCallPenaltyBytes(int bytes)
	{
		ensureSystems();
		mVertexStreams.setUV0OnlyUploadCallPenaltyBytes(bytes);
	}
	public string getPositionDeltaBackendName()
	{
		return FastUIPositionDeltaData_ECSList.BackendName;
	}
	public string getPositionDeltaBackendReason()
	{
		return FastUIPositionDeltaData_ECSList.BackendReason;
	}
	public bool isPositionDeltaUnsafeBackend()
	{
		return FastUIPositionDeltaData_ECSList.IsUnsafeBackend;
	}
	public string getIntBackendName()
	{
		return Int_ECSList.BackendName;
	}
	public string getIntBackendReason()
	{
		return Int_ECSList.BackendReason;
	}
	public bool isIntUnsafeBackend()
	{
		return Int_ECSList.IsUnsafeBackend;
	}
	public string getBoolBackendName()
	{
		return Bool_ECSList.BackendName;
	}
	public string getBoolBackendReason()
	{
		return Bool_ECSList.BackendReason;
	}
	public bool isBoolUnsafeBackend()
	{
		return Bool_ECSList.IsUnsafeBackend;
	}
	public string getRangeBackendName()
	{
		return FastUIRangeData_ECSList.BackendName;
	}
	public string getRangeBackendReason()
	{
		return FastUIRangeData_ECSList.BackendReason;
	}
	public bool isRangeUnsafeBackend()
	{
		return FastUIRangeData_ECSList.IsUnsafeBackend;
	}
	public string getColorBackendName()
	{
		return FastUIColorData_ECSList.BackendName;
	}
	public string getColorBackendReason()
	{
		return FastUIColorData_ECSList.BackendReason;
	}
	public bool isColorUnsafeBackend()
	{
		return FastUIColorData_ECSList.IsUnsafeBackend;
	}
	public string getUVBackendName()
	{
		return FastUIUVData_ECSList.BackendName;
	}
	public string getUVBackendReason()
	{
		return FastUIUVData_ECSList.BackendReason;
	}
	public bool isUVUnsafeBackend()
	{
		return FastUIUVData_ECSList.IsUnsafeBackend;
	}
	public string getGeometryRangeBackendName()
	{
		return FastUIGeometryRangeData_ECSList.BackendName;
	}
	public string getGeometryRangeBackendReason()
	{
		return FastUIGeometryRangeData_ECSList.BackendReason;
	}
	public bool isGeometryRangeUnsafeBackend()
	{
		return FastUIGeometryRangeData_ECSList.IsUnsafeBackend;
	}
	public string getBatchRunBackendName()
	{
		return FastUIBatchRunData_ECSList.BackendName;
	}
	public string getBatchRunBackendReason()
	{
		return FastUIBatchRunData_ECSList.BackendReason;
	}
	public bool isBatchRunUnsafeBackend()
	{
		return FastUIBatchRunData_ECSList.IsUnsafeBackend;
	}
	public string getDrawRunBackendName()
	{
		return FastUIDrawRunData_ECSList.BackendName;
	}
	public string getDrawRunBackendReason()
	{
		return FastUIDrawRunData_ECSList.BackendReason;
	}
	public bool isDrawRunUnsafeBackend()
	{
		return FastUIDrawRunData_ECSList.IsUnsafeBackend;
	}
	public string getSubMeshDescriptorBackendName()
	{
		return FastUISubMeshDescriptorData_ECSList.BackendName;
	}
	public string getSubMeshDescriptorBackendReason()
	{
		return FastUISubMeshDescriptorData_ECSList.BackendReason;
	}
	public bool isSubMeshDescriptorUnsafeBackend()
	{
		return FastUISubMeshDescriptorData_ECSList.IsUnsafeBackend;
	}
	public FastUIPositionData_ECSList getPositionECS()
	{
		ensureSystems();
		return mVertexStreams.getPositionECS();
	}
	public FastUIGeometryRangeData_ECSList getGeometryRangeECS()
	{
		ensureSystems();
		return mVertexStreams.getGeometryRangeECS();
	}
	public FastUIColorData_ECSList getColorECS()
	{
		ensureSystems();
		return mVertexStreams.getColorECS();
	}
	public FastUIUVData_ECSList getUVECS()
	{
		ensureSystems();
		return mVertexStreams.getUVECS();
	}
	public FastUITMPUV0Data_ECSList getTMPUV0ECS()
	{
		ensureSystems();
		return mVertexStreams.getTMPUV0ECS();
	}
	public FastUITMPUV2Data_ECSList getTMPUV2ECS()
	{
		ensureSystems();
		return mVertexStreams.getTMPUV2ECS();
	}
	public bool isTMPVertexLayoutEnabled()
	{
		return mVertexStreams != null && mVertexStreams.isTMPVertexLayoutEnabled();
	}
	public bool isRegistrationReady()
	{
		if (!canMutateRuntimeSystems())
		{
			return false;
		}
		ensureSystems();
		return mSystemsInitialized && mVertexStreams != null && mBatchSystem != null && mDrawOrder != null;
	}
	public void setTMPVertexLayoutEnabled(bool enabled)
	{
		if (!isRegistrationReady())
		{
			return;
		}
		mVertexStreams.setTMPVertexLayoutEnabled(enabled);
	}
	public int getAllocatedSlotCount()
	{
		return mVertexStreams != null ? mVertexStreams.getAllocatedSlotCount() : 0;
	}
	public int getSlotSpan()
	{
		return mVertexStreams != null ? mVertexStreams.getSlotSpan() : 0;
	}
	public int getVertexSlotCapacity()
	{
		return mVertexStreams != null ? mVertexStreams.getVertexSlotCapacity() : 0;
	}
	public int getBatchCount()
	{
		return mBatchSystem != null ? mBatchSystem.getBatchCount() : 0;
	}
	public int getLogicalBatchCount()
	{
		return mBatchSystem != null ? mBatchSystem.getBatchRunCount() : 0;
	}
	public void forceBatchTextureRebind()
	{
		ensureSystems();
		mBatchSystem.forceTextureRebind();
	}
	public bool getClipSubmitRangeCullingEnabled()
	{
		return mBatchSystem != null && mBatchSystem.getClipSubmitRangeCullingEnabled();
	}
	public int getLimitedClipExtraDrawRunCount()
	{
		return mBatchSystem != null ? mBatchSystem.getLimitedClipExtraDrawRunCount() : 0;
	}
	public int getLimitedClipSavedIndexCount()
	{
		return mBatchSystem != null ? mBatchSystem.getLimitedClipSavedIndexCount() : 0;
	}
	public bool setClipSubmitRangeCullingEnabled(bool enabled)
	{
		ensureSystems();
		return mBatchSystem.setClipSubmitRangeCullingEnabled(enabled);
	}
	public int getSubMeshCapacity()
	{
		return mBatchSystem != null ? mBatchSystem.getSubMeshCapacity() : 1;
	}
	public int getActiveSubMeshCount()
	{
		return mBatchSystem != null ? mBatchSystem.getActiveSubMeshCount() : 1;
	}
	public int getAppliedMaterialSlotCount()
	{
		return mBatchSystem != null ? mBatchSystem.getAppliedMaterialSlotCount() : 0;
	}
	public int getMeshSubMeshCount()
	{
		return mMesh != null ? mMesh.subMeshCount : 0;
	}
	public int getMaxPartialSubMeshApplyCount()
	{
		return mBatchSystem != null ? mBatchSystem.getMaxPartialSubMeshApplyCount() : 1;
	}
	public int getActiveElementCount()
	{
		return mCurrentActiveElementCount;
	}
	public void setActiveElementCount(int count)
	{
		mCurrentActiveElementCount = Mathf.Max(count, 0);
	}
	public bool isDrawOrderReordered()
	{
		return mDrawOrder != null && mDrawOrder.isReordered();
	}
	public bool getCompactIndexMode()
	{
		return true;
	}
	public Mesh getRuntimeMesh()
	{
		ensureInit();
		return mMesh;
	}
	public int getMeshVertexCount()
	{
		return mMesh != null ? mMesh.vertexCount : 0;
	}
	public int getVertexSpan()
	{
		return mVertexStreams != null ? mVertexStreams.getVertexSpan() : 0;
	}
	public int prewarmInitialCPUVertexStorage(int minimumVertexCount)
	{
		ensureSystems();
		return mVertexStreams.prewarmInitialCPUVertexStorage(minimumVertexCount);
	}
	public int getLiveVertexCount()
	{
		return mVertexStreams != null ? mVertexStreams.getLiveVertexCount() : 0;
	}
	public int getLiveGeometryIndexCount()
	{
		return mVertexStreams != null ? mVertexStreams.getLiveGeometryIndexCount() : 0;
	}
	public int getCurrentIndexCount()
	{
		return mVertexStreams != null ? mVertexStreams.getCurrentIndexCount() : 0;
	}
	public int getIndexDataCount()
	{
		return mVertexStreams != null ? mVertexStreams.getIndexDataCount() : 0;
	}
	public bool tryGetRawIndex(int indexOffset, out int vertexIndex)
	{
		ensureSystems();
		return mVertexStreams.tryGetRawIndex(indexOffset, out vertexIndex);
	}
	public int getGPUIndexCapacity()
	{
		return mVertexStreams != null ? mVertexStreams.getGPUIndexCapacity() : 0;
	}
	public IndexFormat getGPUIndexFormat()
	{
		return mVertexStreams != null ? mVertexStreams.getGPUIndexFormat() : IndexFormat.UInt16;
	}
	public int getGPUIndexStride()
	{
		return mVertexStreams != null ? mVertexStreams.getGPUIndexStride() : sizeof(ushort);
	}
	public int getGeometryVertexCapacity(int slot)
	{
		ensureSystems();
		return mVertexStreams.getGeometryVertexCapacity(slot);
	}
	public int getRenderIndexForDrawIndex(int drawIndex)
	{
		return mDrawOrder != null ? mDrawOrder.getRenderIndexForDrawIndex(drawIndex) : drawIndex;
	}
	public int getDrawIndexForRenderIndex(int renderIndex)
	{
		return mDrawOrder != null ? mDrawOrder.getDrawIndexForRenderIndex(renderIndex) : renderIndex;
	}
	public int getBatchRunCount()
	{
		return mBatchSystem != null ? mBatchSystem.getBatchRunCount() : 0;
	}
	public int getHiddenRenderRangeCount()
	{
		return mBatchSystem != null ? mBatchSystem.getHiddenRenderRangeCount() : 0;
	}
	public int getIndexHiddenRenderRangeCount()
	{
		return mBatchSystem != null ? mBatchSystem.getIndexHiddenRenderRangeCount() : 0;
	}
	public int getIndexHiddenWordWriteCount()
	{
		return mBatchSystem != null ? mBatchSystem.getLastIndexHiddenWordWriteCount() : 0;
	}
	public int getEffectiveDrawRunCount(int renderElementCount)
	{
		return mBatchSystem != null ? mBatchSystem.getEffectiveDrawRunCount(renderElementCount) : 0;
	}
	public int getEffectiveDrawRunStart(int runIndex, int renderElementCount)
	{
		return mBatchSystem != null ? mBatchSystem.getEffectiveDrawRunStart(runIndex, renderElementCount) : -1;
	}
	public int getEffectiveDrawRunEnd(int runIndex, int renderElementCount)
	{
		return mBatchSystem != null ? mBatchSystem.getEffectiveDrawRunEnd(runIndex, renderElementCount) : -1;
	}
	public Material getEffectiveDrawRunMaterial(int runIndex, int renderElementCount)
	{
		return mBatchSystem?.getEffectiveDrawRunMaterial(runIndex, renderElementCount);
	}
	public Texture getEffectiveDrawRunTexture(int runIndex, int renderElementCount)
	{
		return mBatchSystem?.getEffectiveDrawRunTexture(runIndex, renderElementCount);
	}
	public bool isIndexHiddenRenderIndex(int renderIndex)
	{
		return mBatchSystem != null && mBatchSystem.isIndexHiddenRenderIndex(renderIndex);
	}
	public int setHiddenRenderRanges(FastUIRangeData_ECSList drawHiddenRanges, FastUIRangeData_ECSList indexHiddenRanges)
	{
		ensureSystems();
		return mBatchSystem.setHiddenRenderRanges(drawHiddenRanges, indexHiddenRanges);
	}
	public void syncBatchElementRenderState(int renderIndex, FastUIRenderElement element)
	{
		if (!canMutateRuntimeSystems())
		{
			return;
		}
		ensureSystems();
		mBatchSystem?.syncElementRenderState(renderIndex, element);
	}
	public void patchBatchElementActiveState(int renderIndex, bool active)
	{
		if (!canMutateRuntimeSystems())
		{
			return;
		}
		ensureSystems();
		mBatchSystem?.patchElementActiveState(renderIndex, active);
	}
	public void patchBatchElementVisibleState(int renderIndex, bool visible)
	{
		if (!canMutateRuntimeSystems())
		{
			return;
		}
		ensureSystems();
		mBatchSystem?.patchElementVisibleState(renderIndex, visible);
	}
	public void patchBatchElementCullState(int renderIndex, bool culled)
	{
		if (!canMutateRuntimeSystems())
		{
			return;
		}
		ensureSystems();
		mBatchSystem?.patchElementCullState(renderIndex, culled);
	}
	public void patchBatchElementClipCullState(int renderIndex, bool outside)
	{
		if (!canMutateRuntimeSystems())
		{
			return;
		}
		ensureSystems();
		mBatchSystem?.patchElementClipCullState(renderIndex, outside);
	}
	public void syncBatchElementTextureState(int renderIndex, Texture texture)
	{
		if (!canMutateRuntimeSystems())
		{
			return;
		}
		ensureSystems();
		mBatchSystem?.syncElementTextureState(renderIndex, texture);
	}
	public void refreshVisibility(Material defaultMaterial, int renderElementCount, ref FastUIFrameStats stats)
	{
		ensureSystems();
		mBatchSystem.refreshVisibility(defaultMaterial, renderElementCount, ref stats);
	}
	public void shiftBatchRunRenderIndices(int startRenderIndex, int delta)
	{
		ensureSystems();
		mBatchSystem.shiftBatchRunRenderIndices(startRenderIndex, delta);
	}
	public int allocateVertexSlot()
	{
		if (!isRegistrationReady())
		{
			return -1;
		}
		return mVertexStreams.allocateVertexSlot();
	}
	public void prewarmInitialVertexSlotStorage(int minimumSlotCount)
	{
		if (!isRegistrationReady())
		{
			return;
		}
		mVertexStreams.prewarmInitialVertexSlotStorage(minimumSlotCount);
	}
	public void releaseVertexSlot(int slot)
	{
		mVertexStreams?.releaseVertexSlot(slot);
	}
	public void markPositionSlotDirty(int slot)
	{
		ensureSystems();
		mVertexStreams.markPositionSlotDirty(slot);
	}
	public void markColorSlotDirty(int slot)
	{
		ensureSystems();
		mVertexStreams.markColorSlotDirty(slot);
	}
	public void markUVSlotDirty(int slot)
	{
		ensureSystems();
		mVertexStreams.markUVSlotDirty(slot);
	}
	public void markUV0SlotDirty(int slot)
	{
		ensureSystems();
		mVertexStreams.markUV0SlotDirty(slot);
	}
	public void markPositionSlotRangeDirty(int startSlot, int slotCount)
	{
		ensureSystems();
		mVertexStreams.markPositionSlotRangeDirty(startSlot, slotCount);
	}
	public void markPositionVertexRangeDirty(int startVertex, int vertexCount)
	{
		ensureSystems();
		mVertexStreams.markPositionVertexRangeDirty(startVertex, vertexCount);
	}
	public FastUIGeometryUpdateResult rebuildGeometrySlot(int slot, FastUIGeometryBuilder builder, Color32 color)
	{
		ensureSystems();
		return mVertexStreams.rebuildGeometrySlot(slot, builder, color);
	}
	public FastUIGeometryUpdateResult rebuildSlicedImageSlot(int slot, Vector4 x, Vector4 y, Vector4 u, Vector4 v, bool fillCenter,
		bool translationOnly, Vector3 translation, Matrix4x4 geometryMatrix, Color32 color)
	{
		ensureSystems();
		return mVertexStreams.rebuildSlicedImageSlot(slot, x, y, u, v, fillCenter, translationOnly, translation, geometryMatrix, color);
	}
	public FastUIGeometryUpdateResult rebuildSimpleTMPTextSlot(int slot, FastUITextSimpleGlyphData_ECSList glyphData, int glyphStart, int glyphCount, int glyphCapacity,
		float lineStartX, float baseline, bool translationOnly, Vector3 translation, Matrix4x4 geometryMatrix, Color32 color)
	{
		ensureSystems();
		return mVertexStreams.rebuildSimpleTMPTextSlot(slot, glyphData, glyphStart, glyphCount, glyphCapacity, lineStartX, baseline,
			translationOnly, translation, geometryMatrix, color);
	}
	public FastUIGeometryUpdateResult prepareSimpleTMPTextSlot(int slot, int glyphCount, int glyphCapacity)
	{
		ensureSystems();
		return mVertexStreams.prepareSimpleTMPTextSlot(slot, glyphCount, glyphCapacity);
	}
	public bool tryAllocateInitialSimpleTextRangeBatch(Int_ECSList slots, Int_ECSList vertexCapacities, int slotCount,
		out int batchVertexStart, out int batchVertexCapacity)
	{
		ensureSystems();
		return mVertexStreams.tryAllocateInitialSimpleTextRangeBatch(slots, vertexCapacities, slotCount, out batchVertexStart, out batchVertexCapacity);
	}
	public int getSimpleTextActiveVertexCount(int slot)
	{
		ensureSystems();
		return mVertexStreams.getSimpleTextActiveVertexCount(slot);
	}
	public bool refreshSimpleTMPTextUV0Slot(int slot, FastUITextSimpleGlyphData_ECSList glyphECS, int glyphStart, int glyphCount)
	{
		ensureSystems();
		return mVertexStreams.refreshSimpleTMPTextUV0Slot(slot, glyphECS, glyphStart, glyphCount);
	}
	public void rebuildSimpleTMPTextBatch(FastUITextBatchWorkData_ECSList workECS, FastUITextSimpleGlyphData_ECSList glyphECS, int workCount,
		bool axisAlignedFastPathEnabled, out int axisAlignedWorkCount, out int axisAlignedGlyphCount)
	{
		ensureSystems();
		mVertexStreams.rebuildSimpleTMPTextBatch(workECS, glyphECS, workCount, axisAlignedFastPathEnabled, out axisAlignedWorkCount, out axisAlignedGlyphCount);
	}
	public int getGeometryVertexStart(int slot)
	{
		ensureSystems();
		return mVertexStreams.getGeometryVertexStart(slot);
	}
	public int getGeometryVertexCount(int slot)
	{
		ensureSystems();
		return mVertexStreams.getGeometryVertexCount(slot);
	}
	public int getGeometryIndexCount(int slot)
	{
		ensureSystems();
		return mVertexStreams.getGeometryIndexCount(slot);
	}
	public int getGeometryIndexCapacity(int slot)
	{
		ensureSystems();
		return mVertexStreams.getGeometryIndexCapacity(slot);
	}
	public bool tryGetRawPosition(int vertexIndex, out Vector3 position)
	{
		ensureSystems();
		return mVertexStreams.tryGetRawPosition(vertexIndex, out position);
	}
	public bool tryGetPositionStreamBounds(out Vector3 min, out Vector3 max, out int nonFiniteCount, out int nonZeroZCount, out int extremeCount)
	{
		ensureSystems();
		return mVertexStreams.tryGetPositionStreamBounds(out min, out max, out nonFiniteCount, out nonZeroZCount, out extremeCount);
	}
	public bool tryGetGeometryBounds(int slot, out FastUISpatialBoundsData bounds)
	{
		ensureSystems();
		return tryGetGeometryBounds(slot, 0, out bounds);
	}
	public bool tryGetGeometryBounds(int slot, int vertexCountLimit, out FastUISpatialBoundsData bounds)
	{
		ensureSystems();
		if (!mVertexStreams.tryGetGeometryBounds(slot, vertexCountLimit, out bounds))
		{
			return false;
		}
		// VertexStream保存RenderRoot局部坐标；对外Bounds仍保持Canvas局部坐标语义。
		Vector3 origin = mOwner != null ? mOwner.getRenderOriginOffset() : Vector3.zero;
		bounds.mMinX += origin.x;
		bounds.mMaxX += origin.x;
		bounds.mMinY += origin.y;
		bounds.mMaxY += origin.y;
		bounds.mCenterZ += origin.z;
		return true;
	}
	public bool tryAllocateInitialSimpleQuadBatch(Int_ECSList slots, int slotCount, out int batchVertexStart, out int batchVertexCount)
	{
		ensureSystems();
		return mVertexStreams.tryAllocateInitialSimpleQuadBatch(slots, slotCount, out batchVertexStart, out batchVertexCount);
	}
	public FastUIGeometryUpdateResult rebuildSimpleQuadPositionSlot(int slot, Vector3 bottomLeft, Vector3 topLeft, Vector3 topRight, Vector3 bottomRight)
	{
		ensureSystems();
		return mVertexStreams.rebuildSimpleQuadPositionSlot(slot, bottomLeft, topLeft, topRight, bottomRight);
	}
	// 保留旧版单QuadPosition写入API，实际地址由GeometryRange解析，不再使用slot*4。
	public void setPositionSlot(int slot, Vector3 bottomLeft, Vector3 topLeft, Vector3 topRight, Vector3 bottomRight)
	{
		ensureSystems();
		mVertexStreams.setPositionSlot(slot, bottomLeft, topLeft, topRight, bottomRight);
	}
	public void setColorSlot(int slot, Color32 color)
	{
		ensureSystems();
		mVertexStreams.setColorSlot(slot, color);
	}
	public void setUVSlot(int slot, Rect uvRect)
	{
		ensureSystems();
		mVertexStreams.setUVSlot(slot, uvRect);
	}
	public void offsetPositionSlotRange(int startSlot, int slotCount, Vector3 canvasDelta)
	{
		ensureSystems();
		mVertexStreams.offsetPositionSlotRange(startSlot, slotCount, canvasDelta);
	}
	public int offsetPositionSlotRanges(FastUIRangeData_ECSList slotRanges, int rangeCount, Vector3 canvasDelta, out int dirtyVertexStart, out int dirtyVertexEnd)
	{
		ensureSystems();
		return mVertexStreams.offsetPositionSlotRanges(slotRanges, rangeCount, canvasDelta, out dirtyVertexStart, out dirtyVertexEnd);
	}
	public void applyPositionDeltaQueue(FastUIPositionDeltaData_ECSList deltaECS, int deltaCount)
	{
		ensureSystems();
		mVertexStreams.applyPositionDeltaQueue(deltaECS, deltaCount);
	}
	public bool validateEditorGPUBufferState()
	{
		ensureSystems();
		return mVertexStreams != null && mVertexStreams.validateEditorGPUBufferState();
	}
	public void syncVertexBufferCapacity(ref FastUIFrameStats stats)
	{
		ensureSystems();
		mVertexStreams.syncVertexBufferCapacity(ref stats);
	}
	// Full DrawStructure只在Hierarchy/SOA结构改变时执行；SOA只改变DrawOrder，不改变Logical RenderOrder和VertexSlot。
	public void rebuildFullDrawStructure(List<FastUIRenderElement> renderElements, Material defaultMaterial, Int_ECSList drawOrder, ref FastUIFrameStats stats)
	{
		ensureSystems();
		mBatchSystem.getPendingRefreshRanges().Clear();
		mBatchSystem.getExpandedRefreshRanges().Clear();
		mBatchSystem.getMergeBuffer().Clear();
		mDrawOrder.setDrawOrder(drawOrder, renderElements != null ? renderElements.Count : 0);
		int indexCount = mDrawOrder.writeFullIndexBuffer(renderElements);
		mBatchSystem.rebuildBatchRunsFull(renderElements, defaultMaterial);
		mDrawOrder.rebuildCompactSubmitAndUpload(ref stats, false);
		mVertexStreams.setCurrentIndexCount(indexCount);
		mBatchSystem.applyBatchRuns(defaultMaterial, renderElements != null ? renderElements.Count : 0, ref stats);
		stats.mBatchRebuilt = true;
	}
	public bool patchIndexRange(List<FastUIRenderElement> renderElements, int renderStart, int renderCount, ref FastUIFrameStats stats)
	{
		ensureSystems();
		return mDrawOrder.patchIndexRange(renderElements, renderStart, renderCount, ref stats);
	}
	public bool patchIndexRanges(List<FastUIRenderElement> renderElements, FastUIRangeData_ECSList renderRanges, ref FastUIFrameStats stats)
	{
		ensureSystems();
		return mDrawOrder.patchIndexRanges(renderElements, renderRanges, ref stats);
	}
	public void rebuildIndexBuffer(List<FastUIRenderElement> renderElements, ref FastUIFrameStats stats)
	{
		ensureSystems();
		mDrawOrder.rebuildAndUploadFullIndexBuffer(renderElements, ref stats);
	}
	public FastUIBatchElementData_ECSList getBatchElementStates()
	{
		ensureSystems();
		return mBatchSystem.getElementBatchStates();
	}
	public void rebuildBatchElementStates(List<FastUIRenderElement> renderElements, Material defaultMaterial)
	{
		ensureSystems();
		mBatchSystem.rebuildElementBatchStates(renderElements, defaultMaterial);
	}
	public void syncBatchElementStateRange(List<FastUIRenderElement> renderElements, Material defaultMaterial, int start, int count)
	{
		ensureSystems();
		mBatchSystem.syncElementBatchStateRange(renderElements, defaultMaterial, start, count);
	}
	public void syncBatchElementState(int renderIndex, FastUIRenderElement element, Material defaultMaterial)
	{
		ensureSystems();
		mBatchSystem.syncElementBatchState(renderIndex, element, defaultMaterial);
	}
	public bool patchDrawOrderRanges(List<FastUIRenderElement> renderElements, Int_ECSList sourceOrder, FastUIRangeData_ECSList drawRanges, ref FastUIFrameStats stats)
	{
		ensureSystems();
		return mDrawOrder.patchDrawOrderRanges(renderElements, sourceOrder, drawRanges, ref stats);
	}
	public void convertRenderRangesToDrawRanges(FastUIRangeData_ECSList sourceRanges, FastUIRangeData_ECSList targetRanges, int renderCount)
	{
		ensureSystems();
		mDrawOrder.convertRenderRangesToDrawRanges(sourceRanges, targetRanges, renderCount);
	}
	// Index Prefix发生位移时BatchRun本身没有变化，只需要按现有Run重新计算SubMeshDescriptor。
	public void refreshIndexLayoutDescriptors(Material defaultMaterial, int renderElementCount, ref FastUIFrameStats stats)
	{
		ensureSystems();
		mBatchSystem.refreshIndexLayout(defaultMaterial, renderElementCount, ref stats);
	}
	public void beginBatchRefresh()
	{
		ensureSystems();
		mBatchSystem.beginBatchRefresh();
	}
	public void appendBatchRefreshRange(int renderStart, int renderCount)
	{
		ensureSystems();
		mBatchSystem.appendBatchRefreshRange(renderStart, renderCount);
	}
	public void refreshBatchRange(List<FastUIRenderElement> renderElements, Material defaultMaterial, int renderStart, int renderCount, ref FastUIFrameStats stats)
	{
		beginBatchRefresh();
		appendBatchRefreshRange(renderStart, renderCount);
		finishBatchRefresh(renderElements, defaultMaterial, ref stats);
	}
	public void finishBatchRefresh(List<FastUIRenderElement> renderElements, Material defaultMaterial, ref FastUIFrameStats stats)
	{
		ensureSystems();
		bool useDrawOrder = mDrawOrder.isReordered();
		FastUIBatchRefreshMode actualMode = mBatchSystem.resolveBatchRefreshMode();
		if (actualMode == FastUIBatchRefreshMode.CursorLinearMerge)
		{
			mBatchSystem.rebuildBatchRunsCursorLinear(renderElements, defaultMaterial, useDrawOrder);
		}
		else
		{
			mBatchSystem.rebuildBatchRunsIncremental(renderElements, defaultMaterial, useDrawOrder);
		}
		//  Batch边界变化后Submit Arena才需要重建；普通IndexCount变化仍只重打受影响Batch。
		mDrawOrder.refreshCompactSubmitLayoutAfterBatchChange(ref stats);
		mBatchSystem.applyBatchRuns(defaultMaterial, renderElements != null ? renderElements.Count : 0, ref stats);
		stats.mBatchRebuilt = true;
		mBatchSystem.getPendingRefreshRanges().Clear();
	}
	public int getAdaptiveCursorMinRangeCount()
	{
		return mBatchSystem != null ? mBatchSystem.getAdaptiveCursorMinRangeCount() : 16;
	}
	public int getAdaptiveCursorMinEstimatedScanCount()
	{
		return mBatchSystem != null ? mBatchSystem.getAdaptiveCursorMinEstimatedScanCount() : 64;
	}
	// Facade入口：最终Mesh Stream上传由VertexStreamSystem完成。
	public void uploadDirtyVertexStreams(ref FastUIFrameStats stats)
	{
		ensureSystems();
		mVertexStreams.uploadDirtyVertexStreams(ref stats);
	}
	public void clearDirtyVertexRanges()
	{
		mVertexStreams?.clearDirtyVertexRanges();
	}
	public void refreshRendererSetting(string sortingLayerName, int sortingOrder, int layer)
	{
		ensureInit();
		gameObject.layer = layer;
		mMeshRenderer.sortingLayerName = sortingLayerName;
		mMeshRenderer.sortingOrder = sortingOrder;
	}
#if UNITY_EDITOR
	// 编辑器SceneView不能使用“以Canvas原点为中心”的固定Mesh Bounds。
	// 当Canvas原点已经离开SceneView而其子元素仍在视野内时，Unity会在顶点着色之前直接裁掉整个MeshRenderer。
	// Preview阶段把Renderer Bounds中心临时放到当前SceneView Camera，使Frustum Cull只裁剪真实顶点，不再依赖Canvas中心。
	public void updateEditorPreviewCullingBounds(Camera camera)
	{
		if (Application.isPlaying || camera == null)
		{
			return;
		}
		ensureInit();
		if (mMesh == null)
		{
			return;
		}

		// Editor preview must never use the production HUGE_BOUNDS. Mesh/SubMesh bounds are
		// transformed by the Canvas hierarchy before Unity builds the renderer world AABB; a
		// large local bound can therefore become invalid under scaled or distant UI hierarchies.
		Vector3 scale = transform.lossyScale;
		Vector3 localCameraPosition = transform.InverseTransformPoint(camera.transform.position);
		if (!isFinite(scale) || !isFinite(localCameraPosition))
		{
			setEditorPreviewFallbackBounds();
			return;
		}

		float scaleX = Mathf.Abs(scale.x);
		float scaleY = Mathf.Abs(scale.y);
		float scaleZ = Mathf.Abs(scale.z);
		if (scaleX < EDITOR_PREVIEW_MIN_SCALE || scaleY < EDITOR_PREVIEW_MIN_SCALE || scaleZ < EDITOR_PREVIEW_MIN_SCALE)
		{
			setEditorPreviewFallbackBounds();
			return;
		}

		float previewWorldSize = camera.orthographic
			? Mathf.Clamp(camera.orthographicSize * 4.0f, EDITOR_PREVIEW_MIN_WORLD_SIZE, EDITOR_PREVIEW_MAX_WORLD_SIZE)
			: EDITOR_PREVIEW_MIN_WORLD_SIZE;
		Vector3 localSize = new(
			previewWorldSize / scaleX,
			previewWorldSize / scaleY,
			previewWorldSize / scaleZ);
		if (!isFinite(localSize) || exceedsEditorPreviewLocalLimit(localCameraPosition) || exceedsEditorPreviewLocalLimit(localSize))
		{
			setEditorPreviewFallbackBounds();
			return;
		}

		mMesh.bounds = new Bounds(localCameraPosition, localSize);
	}

	private static bool isFinite(Vector3 value)
	{
		return !float.IsNaN(value.x) && !float.IsInfinity(value.x) &&
			   !float.IsNaN(value.y) && !float.IsInfinity(value.y) &&
			   !float.IsNaN(value.z) && !float.IsInfinity(value.z);
	}
	private static bool exceedsEditorPreviewLocalLimit(Vector3 value)
	{
		return Mathf.Abs(value.x) > EDITOR_PREVIEW_MAX_LOCAL_VALUE ||
			   Mathf.Abs(value.y) > EDITOR_PREVIEW_MAX_LOCAL_VALUE ||
			   Mathf.Abs(value.z) > EDITOR_PREVIEW_MAX_LOCAL_VALUE;
	}
	private void setEditorPreviewFallbackBounds()
	{
		setEditorScaleAdjustedBounds(Vector3.zero, new Vector3(EDITOR_PREVIEW_MIN_WORLD_SIZE, EDITOR_PREVIEW_MIN_WORLD_SIZE, EDITOR_PREVIEW_MIN_WORLD_SIZE));
	}
	private void setEditorPlayModeSafeBounds()
	{
		// Match the production default world coverage without letting Canvas lossyScale multiply
		// the local HUGE_BOUNDS into an invalid Editor world AABB. Player builds keep the original
		// production bounds path unchanged.
		setEditorScaleAdjustedBounds(Vector3.zero, EDITOR_PLAY_MODE_WORLD_SIZE);
	}
	private void setEditorScaleAdjustedBounds(Vector3 localCenter, Vector3 targetWorldSize)
	{
		if (mMesh == null)
		{
			return;
		}
		Vector3 scale = transform.lossyScale;
		if (!isFinite(scale) || !isFinite(localCenter) || !isFinite(targetWorldSize))
		{
			mMesh.bounds = new Bounds(Vector3.zero, Vector3.one);
			return;
		}

		float scaleX = Mathf.Max(Mathf.Abs(scale.x), EDITOR_PREVIEW_MIN_SCALE);
		float scaleY = Mathf.Max(Mathf.Abs(scale.y), EDITOR_PREVIEW_MIN_SCALE);
		float scaleZ = Mathf.Max(Mathf.Abs(scale.z), EDITOR_PREVIEW_MIN_SCALE);
		Vector3 localSize = new(
			Mathf.Min(Mathf.Abs(targetWorldSize.x) / scaleX, EDITOR_PREVIEW_MAX_LOCAL_VALUE),
			Mathf.Min(Mathf.Abs(targetWorldSize.y) / scaleY, EDITOR_PREVIEW_MAX_LOCAL_VALUE),
			Mathf.Min(Mathf.Abs(targetWorldSize.z) / scaleZ, EDITOR_PREVIEW_MAX_LOCAL_VALUE));
		mMesh.bounds = new Bounds(localCenter, localSize);
	}
	public void restoreDefaultCullingBounds()
	{
		if (mMesh != null)
		{
			if (Application.isPlaying)
			{
				setEditorPlayModeSafeBounds();
			}
			else
			{
				setEditorPreviewFallbackBounds();
			}
		}
	}
#endif
	private void OnDestroy()
	{
		FastCanvas owner = mOwner;
		destroyRenderer();
		if (owner != null)
		{
			owner.notifyMeshRendererDestroyed(this);
		}
	}
	public void destroyRenderer()
	{
		if (mDestroyed)
		{
			return;
		}
		mDestroyed = true;
		unregisterDomainUnloadHook();
		disposeSystems();
		if (mMesh != null)
		{
			if (Application.isPlaying)
			{
				Destroy(mMesh);
			}
			else
			{
				DestroyImmediate(mMesh);
			}
			mMesh = null;
		}
		mOwner = null;
		mMeshFilter = null;
		mMeshRenderer = null;
		mPropertyBlock = null;
		mInitialized = false;
	}
	private void ensureInit()
	{
		if (mDestroyed)
		{
			return;
		}
#if UNITY_EDITOR
		if (mDomainUnloading)
		{
			return;
		}
#endif
		if (mInitialized)
		{
			return;
		}
		if (!TryGetComponent(out mMeshFilter))
		{
			mMeshFilter = gameObject.AddComponent<MeshFilter>();
		}
		if (!TryGetComponent(out mMeshRenderer))
		{
			mMeshRenderer = gameObject.AddComponent<MeshRenderer>();
		}
		mMeshRenderer.shadowCastingMode = ShadowCastingMode.Off;
		mMeshRenderer.receiveShadows = false;
		mMeshRenderer.lightProbeUsage = LightProbeUsage.Off;
		mMeshRenderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
		mMeshRenderer.motionVectorGenerationMode = MotionVectorGenerationMode.ForceNoMotion;
		mPropertyBlock = new MaterialPropertyBlock();
		mMesh = new Mesh
		{
			name = "FastUIStableGeometryMesh",
			subMeshCount = 1
		};
		mMesh.MarkDynamic();
#if UNITY_EDITOR
		if (Application.isPlaying)
		{
			setEditorPlayModeSafeBounds();
		}
		else
		{
			setEditorPreviewFallbackBounds();
		}
#else
		mMesh.bounds = FastUIMeshUtility.HUGE_BOUNDS;
#endif
		mMeshFilter.sharedMesh = mMesh;
		mInitialized = true;
	}
	// 生命周期函数可能在Renderer/Domain已经开始释放后继续触发Element的OnEnable/OnDisable。
	// 这类派生状态通知在销毁阶段没有必要再写Batch状态，也绝不能重新创建已经释放的System。
	private bool canMutateRuntimeSystems()
	{
		if (mDestroyed || mOwner == null)
		{
			return false;
		}
#if UNITY_EDITOR
		if (mDomainUnloading)
		{
			return false;
		}
#endif
		return true;
	}
	// 子系统只在Mesh就绪后创建，并通过显式依赖连接，不使用owner反向访问隐藏状态。
	private void ensureSystems()
	{
		if (!canMutateRuntimeSystems())
		{
			return;
		}
		if (mSystemsInitialized)
		{
			return;
		}
		ensureInit();
		if (mOwner == null)
		{
			return;
		}
		try
		{
			mVertexStreams = new FastUIVertexStreamSystem(mMesh);
			mBatchSystem = new FastUIBatchSystem(mOwner, mVertexStreams, mMesh, mMeshRenderer, mPropertyBlock);
			mDrawOrder = new FastUIDrawOrderSystem(mOwner, mVertexStreams, mBatchSystem);
			mBatchSystem.setDrawOrderSystem(mDrawOrder);
			mSystemsInitialized = true;
			registerDomainUnloadHook();
		}
		catch
		{
			disposeSystems();
			throw;
		}
	}
	private void disposeSystems()
	{
		mBatchSystem?.setDrawOrderSystem(null);
		mDrawOrder?.dispose();
		mDrawOrder = null;
		mBatchSystem?.dispose();
		mBatchSystem = null;
		mVertexStreams?.dispose();
		mVertexStreams = null;
		mSystemsInitialized = false;
	}
	private void registerDomainUnloadHook()
	{
#if UNITY_EDITOR
		if (mDomainUnloadHookRegistered)
		{
			return;
		}
		AppDomain.CurrentDomain.DomainUnload += onDomainUnload;
		mDomainUnloadHookRegistered = true;
#endif
	}
	private void unregisterDomainUnloadHook()
	{
#if UNITY_EDITOR
		if (!mDomainUnloadHookRegistered)
		{
			return;
		}
		AppDomain.CurrentDomain.DomainUnload -= onDomainUnload;
		mDomainUnloadHookRegistered = false;
#endif
	}
#if UNITY_EDITOR
	private void onDomainUnload(object sender, EventArgs e)
	{
		mDomainUnloading = true;
		disposeSystems();
	}
#endif
}
