using EasyECS;
using Unity.Collections;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Rendering;

// FastUI Mesh顶点与索引流子系统。
// 独立拥有Position/Color/UV/Index CPU数据、VertexSlot映射、Dirty Range和Mesh上传逻辑。

public enum FastUIDirtyUploadMode
{
	RangeLimit,
	AdaptiveCost,
}
public enum FastUIDirtyUploadChoice
{
	None,
	Partial,
	Envelope,
	Full,
}
public sealed class FastUIVertexStreamSystem
{
	private static readonly ProfilerMarker UPLOAD_POSITION_MARKER = new("FastUI.Mesh.UploadPositionStream");
	private static readonly ProfilerMarker UPLOAD_COLOR_MARKER = new("FastUI.Mesh.UploadColorStream");
	private static readonly ProfilerMarker UPLOAD_UV_MARKER = new("FastUI.Mesh.UploadUVStream");
	private const int PREALLOCATED_SIMPLE_TEXT_ACTIVE_VERTEX_COUNT = -2;
	// Mesh引用由Renderer注入。该System不负责创建或销毁Unity组件，只管理CPU Stream和上传。
	private Mesh mMesh;
	private FastUIVertexUploadBackend mVertexUploadBackend = FastUIVertexUploadBackend.DirectGPUBuffer;
	// Index由Unity Mesh API维护。FastGUI会动态更新SubMeshDescriptor，Mesh.SetSubMesh(s)依赖Unity内部Index状态；
	// 因此生产路径统一使用SetIndexBufferData，避免GraphicsBuffer直写与Mesh内部状态分叉。
	private GraphicsBuffer mPositionGPUBuffer;
	private GraphicsBuffer mColorGPUBuffer;
	private GraphicsBuffer mUV0GPUBuffer;
	private GraphicsBuffer mUV2GPUBuffer;
	private bool mDirectGPUVertexFallbackSyncPending;
	// Unity Mesh在SetVertexBufferParams后由Mesh API先完成一次完整Seed，再进入GraphicsBuffer局部写入。
	// 这是DirectGPU Vertex的生命周期不变量：新建/重建Buffer不能继承旧GPU内容，也不能只依赖本帧Dirty Range。
	private bool mDirectGPUVertexSeedRequired = true;
	// VertexSlot只作为稳定元素ID，不再通过slot*4推导真实顶点地址。
	// 每个Slot映射到GeometryRange；范围扩容时只搬当前元素，其他元素地址保持不变。
	private readonly FastUIGeometryRangeData_ECSList mGeometryRanges;
	// Simple Text把每个Glyph当成固定Quad Slot。Active记录当前真正使用的顶点数，ClearEnd只在缩短/扩容时清理失效尾部。
	private readonly Int_ECSList mSimpleTextActiveVertexCounts;
	private readonly Int_ECSList mSimpleTextClearEndVertexCounts;
	private readonly FastUIRangeData_ECSList mFreeVertexRanges;
	private FastUIPositionData_ECSList mPositionECS;
	private int mPositionDataCount;
	private FastUIColorData_ECSList mColorECS;
	private int mColorDataCount;
	private FastUIUVData_ECSList mUVECS;
	private int mUVDataCount;
	private FastUITMPUV0Data_ECSList mTMPUV0ECS;
	private int mTMPUV0DataCount;
	private FastUITMPUV2Data_ECSList mTMPUV2ECS;
	private int mTMPUV2DataCount;
	private Int_ECSList mIndexECS;
	private int mIndexDataCount;
	private readonly Int_ECSList mFreeSlots;
	private readonly FastUIRangeData_ECSList mDirtyPositionRanges;
	private readonly FastUIRangeData_ECSList mDirtyColorRanges;
	private readonly FastUIRangeData_ECSList mDirtyUVRanges;
	private readonly FastUIRangeData_ECSList mDirtyUV2Ranges;
	private readonly FastUIRangeData_ECSList mMergedRanges;
	private int mNextSlot;
	private int mVertexSpan;
	private int mLiveVertexCount;
	private int mAllocatedSlotCount;
	private int mGPUVertexCapacity;
	private int mGPUIndexCapacity;
	private IndexFormat mGPUIndexFormat = IndexFormat.UInt16;
	private bool mGPUIndexFormatInitialized;
	private ushort[] mIndex16Staging;
	private int mCurrentIndexCount;
	private const int POSITION_BYTES_PER_VERTEX = 12;
	private const int COLOR_BYTES_PER_VERTEX = 4;
	private const int IMAGE_UV_BYTES_PER_VERTEX = 8;
	private const int TMP_UV_BYTES_PER_VERTEX = 24;
	private const int FULL_UPLOAD_REASON_NONE = 0;
	private const int FULL_UPLOAD_REASON_FORCE = 1;
	private const int FULL_UPLOAD_REASON_BUFFER_RESIZE = 2;
	private bool mForceFullPositionUpload;
	private bool mForceFullColorUpload;
	private bool mForceFullUVUpload;
	private int mForceFullPositionReason;
	private int mForceFullColorReason;
	private int mForceFullUVReason;
	private bool mTMPVertexLayoutEnabled;
	private bool mVertexLayoutDirty;
	private int mDirtyRangeMergeGap;
	private FastUIDirtyUploadMode mDirtyUploadMode = FastUIDirtyUploadMode.AdaptiveCost;
	// 把一次额外Mesh/GraphicsBuffer上传调用折算成等价字节成本。
	// AdaptiveCost会按各Stream实际每顶点字节数自动换算允许跨越的Gap，不再使用统一“顶点个数阈值”。
	private int mUploadCallPenaltyBytes = 1024;
	// 纯Image/RawImage UV0更新只写8B TexCoord0流。同进程校准连续两轮证明，
	// 512B策略比通用1024B显著减少过度Range合并，同时不增加GPU耗时；其他Stream继续使用1024B。
	private int mUV0OnlyUploadCallPenaltyBytes = 512;
	// 不能通过UV2 DirtyCount间接判断是否存在完整UV更新：TMP Layout关闭时完整UV也不会产生UV2 Range。
	// markUVSlotDirty显式记录本帧是否出现过完整UV Dirty，确保512B只用于真正纯UV0Only帧。
	private bool mHasFullUVDirty;
	public FastUIVertexStreamSystem(Mesh mesh)
	{
		mMesh = mesh;
		mGeometryRanges = new FastUIGeometryRangeData_ECSList(256);
		mSimpleTextActiveVertexCounts = new Int_ECSList(256);
		mSimpleTextClearEndVertexCounts = new Int_ECSList(256);
		mFreeVertexRanges = new FastUIRangeData_ECSList(64);
		mFreeSlots = new Int_ECSList(256);
		mDirtyPositionRanges = new FastUIRangeData_ECSList(64);
		mDirtyColorRanges = new FastUIRangeData_ECSList(64);
		mDirtyUVRanges = new FastUIRangeData_ECSList(64);
		mDirtyUV2Ranges = new FastUIRangeData_ECSList(64);
		mMergedRanges = new FastUIRangeData_ECSList(64);
	}
	public FastUIPositionData_ECSList getPositionECS()
	{
		return mPositionECS;
	}
	public FastUIGeometryRangeData_ECSList getGeometryRangeECS()
	{
		return mGeometryRanges;
	}
	public FastUIColorData_ECSList getColorECS()
	{
		return mColorECS;
	}
	public FastUIUVData_ECSList getUVECS()
	{
		return mUVECS;
	}
	public FastUITMPUV0Data_ECSList getTMPUV0ECS()
	{
		return mTMPUV0ECS;
	}
	public FastUITMPUV2Data_ECSList getTMPUV2ECS()
	{
		return mTMPUV2ECS;
	}
	public bool isTMPVertexLayoutEnabled()
	{
		return mTMPVertexLayoutEnabled;
	}
	public Int_ECSList getIndexECS()
	{
		return mIndexECS;
	}
	public int getPositionDataCount()
	{
		return mPositionDataCount;
	}
	public int getAllocatedSlotCount()
	{
		return mAllocatedSlotCount;
	}
	public int getSlotSpan()
	{
		return mNextSlot;
	}
	public int getVertexSlotCapacity()
	{
		return Mathf.NextPowerOfTwo(Mathf.Max(mNextSlot, 1));
	}
	public int getVertexSpan()
	{
		return mVertexSpan;
	}
	public int getLiveVertexCount()
	{
		return mLiveVertexCount;
	}
	// Compact Index的GPU容量必须来自GeometryRange本身，而不是RenderElement列表。
	// GeometryRange才是实际GPU几何的事实来源，容量统计不再依赖上层RenderElement视图。
	// 这里只在Full Index结构重建/模式切换等低频路径调用，避免为了缓存再维护一份容易失配的派生状态。
	public int getLiveGeometryIndexCount()
	{
		if (mGeometryRanges == null || mGeometryRanges.Count == 0)
		{
			return 0;
		}
		var vertexCounts = mGeometryRanges.getVertexCountColumn();
		long total = 0;
		for (int slot = 0; slot < mGeometryRanges.Count; ++slot)
		{
			int vertexCount = vertexCounts[slot];
			if (vertexCount <= 0)
			{
				continue;
			}
			total += vertexCount / 4 * 6L;
			if (total >= int.MaxValue)
			{
				return int.MaxValue;
			}
		}
		return (int)total;
	}
	// 只Reserve稳定VertexSlot的Geometry Range记录，不创建几何、不推进mNextSlot。
	public void prewarmInitialVertexSlotStorage(int minimumSlotCount)
	{
		if (minimumSlotCount <= 0 || mNextSlot > 0)
		{
			return;
		}
		mGeometryRanges.EnsureCapacity(minimumSlotCount);
		mSimpleTextActiveVertexCounts.EnsureCapacity(minimumSlotCount);
		mSimpleTextClearEndVertexCounts.EnsureCapacity(minimumSlotCount);
	}
	// 首次FullFlush只做容量预热，不改变VertexSpan/LiveVertexCount。
	// RenderElement至少按4顶点作为保守下界，复杂Text/自定义Geometry不足部分仍由正常Range增长补齐。
	public int prewarmInitialCPUVertexStorage(int minimumVertexCount)
	{
		if (minimumVertexCount <= 0 || mVertexSpan > 0 || mPositionDataCount > 0)
		{
			return mPositionECS != null ? mPositionECS.Capacity : 0;
		}
		int capacity = Mathf.NextPowerOfTwo(Mathf.Max(minimumVertexCount, 4));
		ensureCPUVertexCapacity(capacity);
		return mPositionECS != null ? mPositionECS.Capacity : 0;
	}
	public int getGeometryVertexStart(int slot)
	{
		return slot >= 0 && slot < mGeometryRanges.Count ? mGeometryRanges[slot].mVertexStart : -1;
	}
	public int getGeometryVertexCount(int slot)
	{
		return slot >= 0 && slot < mGeometryRanges.Count ? mGeometryRanges[slot].mVertexCount : 0;
	}
	public int getGeometryVertexCapacity(int slot)
	{
		return slot >= 0 && slot < mGeometryRanges.Count ? mGeometryRanges[slot].mVertexCapacity : 0;
	}
	public int getSimpleTextActiveVertexCount(int slot)
	{
		return slot >= 0 && slot < mSimpleTextActiveVertexCounts.Count ? Mathf.Max(mSimpleTextActiveVertexCounts[slot], 0) : 0;
	}
	public int getGeometryIndexCount(int slot)
	{
		return getGeometryVertexCount(slot) / 4 * 6;
	}
	// Index保留容量直接复用GeometryRange的VertexCapacity，不再维护第二套容量状态。
	// 普通单Quad仍然只保留6个Index；复杂Image/TMP扩过一次后可在该容量内局部改变实际Quad数量。
	public int getGeometryIndexCapacity(int slot)
	{
		return getGeometryVertexCapacity(slot) / 4 * 6;
	}
	public int getGPUIndexCapacity()
	{
		return mGPUIndexCapacity;
	}
	public IndexFormat getGPUIndexFormat()
	{
		return mGPUIndexFormat;
	}
	public int getGPUIndexStride()
	{
		return mGPUIndexFormat == IndexFormat.UInt16 ? sizeof(ushort) : sizeof(int);
	}
	public int getCurrentIndexCount()
	{
		return mCurrentIndexCount;
	}
	public void setCurrentIndexCount(int count)
	{
		mCurrentIndexCount = count;
	}
	public FastUIVertexUploadBackend getVertexUploadBackend()
	{
		return mVertexUploadBackend;
	}
	public int getDirtyRangeMergeGap()
	{
		return mDirtyRangeMergeGap;
	}
	public void setDirtyRangeMergeGap(int gap)
	{
		mDirtyRangeMergeGap = Mathf.Max(gap, 0);
	}
	public FastUIDirtyUploadMode getDirtyUploadMode()
	{
		return mDirtyUploadMode;
	}
	public void setDirtyUploadMode(FastUIDirtyUploadMode mode)
	{
		mDirtyUploadMode = mode;
	}
	public int getUploadCallPenaltyBytes()
	{
		return mUploadCallPenaltyBytes;
	}
	public void setUploadCallPenaltyBytes(int bytes)
	{
		mUploadCallPenaltyBytes = Mathf.Max(bytes, 0);
	}
	public int getUV0OnlyUploadCallPenaltyBytes()
	{
		return mUV0OnlyUploadCallPenaltyBytes;
	}
	public void setUV0OnlyUploadCallPenaltyBytes(int bytes)
	{
		mUV0OnlyUploadCallPenaltyBytes = Mathf.Max(bytes, 0);
	}
	public void setVertexUploadBackend(FastUIVertexUploadBackend backend)
	{
		if (mVertexUploadBackend == backend)
		{
			return;
		}
		releaseDirectGPUVertexBuffers();
		mVertexUploadBackend = backend;
		mDirectGPUVertexSeedRequired = backend == FastUIVertexUploadBackend.DirectGPUBuffer;
		forceFullVertexStreams(FULL_UPLOAD_REASON_FORCE);
	}
	public void setMesh(Mesh mesh)
	{
		if (mMesh == mesh)
		{
			return;
		}
		releaseDirectGPUBuffers();
		mMesh = mesh;
		mDirectGPUVertexSeedRequired = mVertexUploadBackend == FastUIVertexUploadBackend.DirectGPUBuffer;
		forceFullVertexStreams(FULL_UPLOAD_REASON_FORCE);
	}
	// Unity Editor在PlayMode/Domain生命周期切换时可能保留FastGUI托管状态，但清空Mesh原生Buffer。
	// 这里只在Canvas OnEnable后的首个LateUpdate调用；Player编译时整段检测被移除，不增加正式运行期开销。
	public bool validateEditorGPUBufferState()
	{
#if UNITY_EDITOR
		if (mMesh == null || mGPUVertexCapacity <= 0)
		{
			return false;
		}
		if (mMesh.vertexBufferCount > 0 && mMesh.vertexCount >= mGPUVertexCapacity)
		{
			return false;
		}
		releaseDirectGPUBuffers();
		mDirectGPUVertexSeedRequired = mVertexUploadBackend == FastUIVertexUploadBackend.DirectGPUBuffer;
		mGPUVertexCapacity = 0;
		mGPUIndexCapacity = 0;
		mGPUIndexFormatInitialized = false;
		mGPUIndexFormat = IndexFormat.UInt16;
		mVertexLayoutDirty = true;
		forceFullVertexStreams(FULL_UPLOAD_REASON_FORCE);
		return true;
#else
		return false;
#endif
	}
	// Canvas一旦进入TMP顶点布局后保持到Renderer销毁。
	// IMAGE->TMP提升时需要把已有普通UV补到TMP Stream；若之后再降级并二次提升，
	// copyBaseUVToTMPData会把已存在文字的SDF Scale/Character Mapping覆盖成普通图片默认值。
	// 图片Shader只读取TexCoord0.xy，不受额外TMP Stream影响，因此这里使用单向提升换取稳定正确性。
	public void setTMPVertexLayoutEnabled(bool enabled)
	{
		if (!enabled || mTMPVertexLayoutEnabled)
		{
			return;
		}
		mTMPVertexLayoutEnabled = true;
		mVertexLayoutDirty = true;
		ensureTMPVertexCapacity(Mathf.Max(mPositionDataCount, Mathf.Max(mVertexSpan, 4)));
		copyBaseUVToTMPData();
		forceFullVertexStreams(FULL_UPLOAD_REASON_FORCE);
	}
	public void dispose()
	{
		releaseDirectGPUBuffers();
		mGeometryRanges.Dispose();
		mSimpleTextActiveVertexCounts.Dispose();
		mSimpleTextClearEndVertexCounts.Dispose();
		mFreeVertexRanges.Dispose();
		mFreeSlots.Dispose();
		mDirtyPositionRanges.Dispose();
		mDirtyColorRanges.Dispose();
		mDirtyUVRanges.Dispose();
		mDirtyUV2Ranges.Dispose();
		mMergedRanges.Dispose();
		if (mPositionECS != null)
		{
			mPositionECS.Dispose();
			mPositionECS = null;
		}
		if (mColorECS != null)
		{
			mColorECS.Dispose();
			mColorECS = null;
		}
		if (mUVECS != null)
		{
			mUVECS.Dispose();
			mUVECS = null;
		}
		if (mTMPUV0ECS != null)
		{
			mTMPUV0ECS.Dispose();
			mTMPUV0ECS = null;
		}
		if (mTMPUV2ECS != null)
		{
			mTMPUV2ECS.Dispose();
			mTMPUV2ECS = null;
		}
		if (mIndexECS != null)
		{
			mIndexECS.Dispose();
			mIndexECS = null;
		}
		mIndex16Staging = null;
		mMesh = null;
	}
	// VertexSlot保持稳定，回收后的Slot可以复用，但不会因为RenderOrder变化重新编号。
	public int allocateVertexSlot()
	{
		int slot;
		if (mFreeSlots.Count > 0)
		{
			int lastIndex = mFreeSlots.Count - 1;
			slot = mFreeSlots[lastIndex];
			mFreeSlots.RemoveAt(lastIndex);
		}
		else
		{
			slot = mNextSlot++;
		}
		ensureGeometrySlotRecord(slot);
		++mAllocatedSlotCount;
		return slot;
	}

	public void releaseVertexSlot(int slot)
	{
		if (slot < 0 || slot >= mNextSlot)
		{
			return;
		}
		releaseGeometryRange(slot);
		mFreeSlots.Add(slot);
		mAllocatedSlotCount = Mathf.Max(0, mAllocatedSlotCount - 1);
	}

	public void markPositionSlotDirty(int slot)
	{
		addSlotVertexRange(mDirtyPositionRanges, slot);
	}

	public void markColorSlotDirty(int slot)
	{
		addSlotVertexRange(mDirtyColorRanges, slot);
	}

	public void markUVSlotDirty(int slot)
	{
		mHasFullUVDirty = true;
		addSlotVertexRange(mDirtyUVRanges, slot);
		if (mTMPVertexLayoutEnabled)
		{
			addSlotVertexRange(mDirtyUV2Ranges, slot);
		}
	}
	// Simple Image/RawImage只改变TexCoord0.xy时，TMP UV2(character mapping)并没有变化。
	// 在混合Image+TMP Canvas中只标记UV0，避免为图片Sprite切换重复上传整段UV2 Stream。
	public void markUV0SlotDirty(int slot)
	{
		addSlotVertexRange(mDirtyUVRanges, slot);
	}

	public void markPositionSlotRangeDirty(int startSlot, int slotCount)
	{
		int endSlot = Mathf.Min(startSlot + slotCount, mGeometryRanges.Count);
		for (int slot = Mathf.Max(startSlot, 0); slot < endSlot; ++slot)
		{
			addSlotVertexRange(mDirtyPositionRanges, slot);
		}
	}
	// Bulk PositionDelta 已经知道一整块顶点区域都会在本帧提交时保持CPU数据有效，直接记录物理Vertex Range，避免先拆成逐Slot DirtyRange再排序合并。
	public void markPositionVertexRangeDirty(int startVertex, int vertexCount)
	{
		if (vertexCount <= 0 || startVertex < 0)
		{
			return;
		}
		int endVertex = Mathf.Min(startVertex + vertexCount, mVertexSpan);
		if (endVertex <= startVertex)
		{
			return;
		}
		mDirtyPositionRanges.Add(new FastUIRangeData(startVertex, endVertex));
	}

	// 把Builder中的完整几何写入当前Slot。返回值用于上层判断是否需要重建Index布局。
	public FastUIGeometryUpdateResult rebuildGeometrySlot(int slot, FastUIGeometryBuilder builder, Color32 color)
	{
		int vertexCount = builder != null ? builder.getVertexCount() : 0;
		FastUIGeometryUpdateResult result = ensureGeometryRange(slot, vertexCount);
		if (vertexCount <= 0)
		{
			return result;
		}
		int vertexStart = getGeometryVertexStart(slot);
		Vector3[] sourcePositions = builder.getPositions();
		Vector2[] sourceUVs = builder.getUVs();
		Vector4[] sourceTMPUV0s = builder.getTMPUV0s();
		Vector2[] sourceTMPUV2s = builder.getTMPUV2s();
		Color32[] sourceColors = builder.getColors();
		bool hasVertexColors = builder.hasVertexColors();
		bool hasTMPVertexData = builder.hasTMPVertexData();
		if (hasTMPVertexData && !mTMPVertexLayoutEnabled)
		{
			setTMPVertexLayoutEnabled(true);
		}
		var positionColumn = mPositionECS.getPositionColumn();
		var colorColumn = mColorECS.getColorColumn();
		var uvColumn = mUVECS.getUVColumn();
		if (mTMPVertexLayoutEnabled)
		{
			var tmpUV0Column = mTMPUV0ECS.getUVColumn();
			var tmpUV2Column = mTMPUV2ECS.getUVColumn();
			for (int i = 0; i < vertexCount; ++i)
			{
				int target = vertexStart + i;
				positionColumn[target] = sourcePositions[i];
				colorColumn[target] = hasVertexColors ? multiplyColor(color, sourceColors[i]) : color;
				Vector2 uv = sourceUVs[i];
				uvColumn[target] = uv;
				tmpUV0Column[target] = hasTMPVertexData ? sourceTMPUV0s[i] : new Vector4(uv.x, uv.y, 0.0f, 1.0f);
				tmpUV2Column[target] = hasTMPVertexData ? sourceTMPUV2s[i] : default;
			}
		}
		else
		{
			for (int i = 0; i < vertexCount; ++i)
			{
				int target = vertexStart + i;
				positionColumn[target] = sourcePositions[i];
				colorColumn[target] = hasVertexColors ? multiplyColor(color, sourceColors[i]) : color;
				uvColumn[target] = sourceUVs[i];
			}
		}
		return result;
	}

	// 小批量Text使用直接路径，Glyph的Scale/Padding/UV已在Layout阶段预计算。
	public FastUIGeometryUpdateResult rebuildSimpleTMPTextSlot(int slot, FastUITextSimpleGlyphData_ECSList glyphData, int glyphStart, int glyphCount, int glyphCapacity,
		float lineStartX, float baseline, bool translationOnly, Vector3 translation, Matrix4x4 geometryMatrix, Color32 color)
	{
		int safeGlyphCount = Mathf.Max(glyphCount, 0);
		int vertexCount = safeGlyphCount * 4;
		FastUIGeometryUpdateResult result = ensureSimpleTextGeometryRange(slot, vertexCount, Mathf.Max(glyphCapacity, safeGlyphCount) * 4);
		if (result.mVertexCount <= 0 || (vertexCount > 0 && glyphData == null))
		{
			finishSimpleTextGeometryWrite(slot, vertexCount, result.mVertexStart);
			return result;
		}
		if (!mTMPVertexLayoutEnabled)
		{
			setTMPVertexLayoutEnabled(true);
		}
		int vertexStart = getGeometryVertexStart(slot);
		var positionColumn = mPositionECS.getPositionColumn();
		var colorColumn = mColorECS.getColorColumn();
		var uvColumn = mUVECS.getUVColumn();
		var tmpUV0Column = mTMPUV0ECS.getUVColumn();
		var tmpUV2Column = mTMPUV2ECS.getUVColumn();
		var penXColumn = glyphData.getPenXColumn();
		var leftOffsetColumn = glyphData.getLeftOffsetColumn();
		var topOffsetColumn = glyphData.getTopOffsetColumn();
		var rightOffsetColumn = glyphData.getRightOffsetColumn();
		var bottomOffsetColumn = glyphData.getBottomOffsetColumn();
		var uvLeftColumn = glyphData.getUVLeftColumn();
		var uvBottomColumn = glyphData.getUVBottomColumn();
		var uvRightColumn = glyphData.getUVRightColumn();
		var uvTopColumn = glyphData.getUVTopColumn();
		var sdfScaleColumn = glyphData.getSDFScaleColumn();
		for (int i = 0; i < safeGlyphCount; ++i)
		{
			int source = glyphStart + i;
			float penX = lineStartX + penXColumn[source];
			float left = penX + leftOffsetColumn[source];
			float top = baseline + topOffsetColumn[source];
			float right = penX + rightOffsetColumn[source];
			float bottom = baseline + bottomOffsetColumn[source];
			Vector3 bottomLeft;
			Vector3 topLeft;
			Vector3 topRight;
			Vector3 bottomRight;
			if (translationOnly)
			{
				bottomLeft = new Vector3(left + translation.x, bottom + translation.y, translation.z);
				topLeft = new Vector3(left + translation.x, top + translation.y, translation.z);
				topRight = new Vector3(right + translation.x, top + translation.y, translation.z);
				bottomRight = new Vector3(right + translation.x, bottom + translation.y, translation.z);
			}
			else
			{
				bottomLeft = geometryMatrix.MultiplyPoint3x4(new Vector3(left, bottom, 0.0f));
				topLeft = geometryMatrix.MultiplyPoint3x4(new Vector3(left, top, 0.0f));
				topRight = geometryMatrix.MultiplyPoint3x4(new Vector3(right, top, 0.0f));
				bottomRight = geometryMatrix.MultiplyPoint3x4(new Vector3(right, bottom, 0.0f));
			}
			float uvLeft = uvLeftColumn[source];
			float uvBottom = uvBottomColumn[source];
			float uvRight = uvRightColumn[source];
			float uvTop = uvTopColumn[source];
			float sdfScale = sdfScaleColumn[source];
			int target = vertexStart + i * 4;
			positionColumn[target + 0] = bottomLeft;
			positionColumn[target + 1] = topLeft;
			positionColumn[target + 2] = topRight;
			positionColumn[target + 3] = bottomRight;
			colorColumn[target + 0] = color;
			colorColumn[target + 1] = color;
			colorColumn[target + 2] = color;
			colorColumn[target + 3] = color;
			uvColumn[target + 0] = new Vector2(uvLeft, uvBottom);
			uvColumn[target + 1] = new Vector2(uvLeft, uvTop);
			uvColumn[target + 2] = new Vector2(uvRight, uvTop);
			uvColumn[target + 3] = new Vector2(uvRight, uvBottom);
			tmpUV0Column[target + 0] = new Vector4(uvLeft, uvBottom, 0.0f, sdfScale);
			tmpUV0Column[target + 1] = new Vector4(uvLeft, uvTop, 0.0f, sdfScale);
			tmpUV0Column[target + 2] = new Vector4(uvRight, uvTop, 0.0f, sdfScale);
			tmpUV0Column[target + 3] = new Vector4(uvRight, uvBottom, 0.0f, sdfScale);
			tmpUV2Column[target + 0] = new Vector2(0.0f, 0.0f);
			tmpUV2Column[target + 1] = new Vector2(0.0f, 1.0f);
			tmpUV2Column[target + 2] = new Vector2(1.0f, 1.0f);
			tmpUV2Column[target + 3] = new Vector2(1.0f, 0.0f);
		}
		finishSimpleTextGeometryWrite(slot, vertexCount, vertexStart);
		return result;
	}
	public FastUIGeometryUpdateResult prepareSimpleTMPTextSlot(int slot, int glyphCount, int glyphCapacity)
	{
		int safeGlyphCount = Mathf.Max(glyphCount, 0);
		return ensureSimpleTextGeometryRange(slot, safeGlyphCount * 4, Mathf.Max(glyphCapacity, safeGlyphCount) * 4);
	}
	public bool refreshSimpleTMPTextUV0Slot(int slot, FastUITextSimpleGlyphData_ECSList glyphECS, int glyphStart, int glyphCount)
	{
		if (slot < 0 || glyphECS == null || glyphStart < 0 || glyphCount < 0 || glyphStart + glyphCount > glyphECS.Count)
		{
			return false;
		}
		int vertexCount = glyphCount * 4;
		int vertexStart = getGeometryVertexStart(slot);
		if (vertexStart < 0 || getSimpleTextActiveVertexCount(slot) != vertexCount || getGeometryVertexCount(slot) < vertexCount)
		{
			return false;
		}
		if (!mTMPVertexLayoutEnabled)
		{
			setTMPVertexLayoutEnabled(true);
		}
		var uvLeftColumn = glyphECS.getUVLeftColumn();
		var uvBottomColumn = glyphECS.getUVBottomColumn();
		var uvRightColumn = glyphECS.getUVRightColumn();
		var uvTopColumn = glyphECS.getUVTopColumn();
		var sdfScaleColumn = glyphECS.getSDFScaleColumn();
		var uvColumn = mUVECS.getUVColumn();
		var tmpUV0Column = mTMPUV0ECS.getUVColumn();
		for (int glyphIndex = 0; glyphIndex < glyphCount; ++glyphIndex)
		{
			int source = glyphStart + glyphIndex;
			int target = vertexStart + glyphIndex * 4;
			float uvLeft = uvLeftColumn[source];
			float uvBottom = uvBottomColumn[source];
			float uvRight = uvRightColumn[source];
			float uvTop = uvTopColumn[source];
			float sdfScale = sdfScaleColumn[source];
			uvColumn[target + 0] = new Vector2(uvLeft, uvBottom);
			uvColumn[target + 1] = new Vector2(uvLeft, uvTop);
			uvColumn[target + 2] = new Vector2(uvRight, uvTop);
			uvColumn[target + 3] = new Vector2(uvRight, uvBottom);
			tmpUV0Column[target + 0] = new Vector4(uvLeft, uvBottom, 0.0f, sdfScale);
			tmpUV0Column[target + 1] = new Vector4(uvLeft, uvTop, 0.0f, sdfScale);
			tmpUV0Column[target + 2] = new Vector4(uvRight, uvTop, 0.0f, sdfScale);
			tmpUV0Column[target + 3] = new Vector4(uvRight, uvBottom, 0.0f, sdfScale);
		}
		markUV0SlotDirty(slot);
		return true;
	}
	// Work ECS只保存Glyph Range引用；Glyph直接来自Canvas Persistent Glyph ECS。
	// 不再存在Per-Text Glyph -> 临时Canvas Glyph的每帧搬运。
	public void rebuildSimpleTMPTextBatch(FastUITextBatchWorkData_ECSList workECS, FastUITextSimpleGlyphData_ECSList glyphECS, int workCount,
		bool axisAlignedFastPathEnabled, out int axisAlignedWorkCount, out int axisAlignedGlyphCount)
	{
		axisAlignedWorkCount = 0;
		axisAlignedGlyphCount = 0;
		if (workECS == null || glyphECS == null || workCount <= 0)
		{
			return;
		}
		if (!mTMPVertexLayoutEnabled)
		{
			setTMPVertexLayoutEnabled(true);
		}
		var slotColumn = workECS.getSlotColumn();
		var vertexStartColumn = workECS.getVertexStartColumn();
		var glyphStartColumn = workECS.getGlyphStartColumn();
		var glyphCountColumn = workECS.getGlyphCountColumn();
		var m00Column = workECS.getM00Column();
		var m01Column = workECS.getM01Column();
		var m03Column = workECS.getM03Column();
		var m10Column = workECS.getM10Column();
		var m11Column = workECS.getM11Column();
		var m13Column = workECS.getM13Column();
		var m20Column = workECS.getM20Column();
		var m21Column = workECS.getM21Column();
		var m23Column = workECS.getM23Column();
		var workColorColumn = workECS.getColorColumn();
		var writeUVColumn = workECS.getWriteUVColumn();
		var writeColorColumn = workECS.getWriteColorColumn();
		var penXColumn = glyphECS.getPenXColumn();
		var leftOffsetColumn = glyphECS.getLeftOffsetColumn();
		var topOffsetColumn = glyphECS.getTopOffsetColumn();
		var rightOffsetColumn = glyphECS.getRightOffsetColumn();
		var bottomOffsetColumn = glyphECS.getBottomOffsetColumn();
		var uvLeftColumn = glyphECS.getUVLeftColumn();
		var uvBottomColumn = glyphECS.getUVBottomColumn();
		var uvRightColumn = glyphECS.getUVRightColumn();
		var uvTopColumn = glyphECS.getUVTopColumn();
		var sdfScaleColumn = glyphECS.getSDFScaleColumn();
		var positionColumn = mPositionECS.getPositionColumn();
		var colorColumn = mColorECS.getColorColumn();
		var uvColumn = mUVECS.getUVColumn();
		var tmpUV0Column = mTMPUV0ECS.getUVColumn();
		var tmpUV2Column = mTMPUV2ECS.getUVColumn();
		int safeWorkCount = Mathf.Min(workCount, workECS.Count);
		for (int workIndex = 0; workIndex < safeWorkCount; ++workIndex)
		{
			int slot = slotColumn[workIndex];
			int vertexStart = vertexStartColumn[workIndex];
			int glyphStart = glyphStartColumn[workIndex];
			int glyphCount = glyphCountColumn[workIndex];
			float m00 = m00Column[workIndex];
			float m01 = m01Column[workIndex];
			float m03 = m03Column[workIndex];
			float m10 = m10Column[workIndex];
			float m11 = m11Column[workIndex];
			float m13 = m13Column[workIndex];
			float m20 = m20Column[workIndex];
			float m21 = m21Column[workIndex];
			float m23 = m23Column[workIndex];
			Color32 color = workColorColumn[workIndex];
			bool writeUV = writeUVColumn[workIndex] != 0;
			bool writeColor = writeColorColumn[workIndex] != 0;
			bool axisAligned = axisAlignedFastPathEnabled && m01 == 0.0f && m10 == 0.0f && m20 == 0.0f && m21 == 0.0f;
			if (axisAligned)
			{
				++axisAlignedWorkCount;
				axisAlignedGlyphCount += glyphCount;
			}
			for (int glyphIndex = 0; glyphIndex < glyphCount; ++glyphIndex)
			{
				int source = glyphStart + glyphIndex;
				int target = vertexStart + glyphIndex * 4;
				float penX = penXColumn[source];
				float left = penX + leftOffsetColumn[source];
				float top = topOffsetColumn[source];
				float right = penX + rightOffsetColumn[source];
				float bottom = bottomOffsetColumn[source];
				if (axisAligned)
				{
					float xLeft = left * m00 + m03;
					float xRight = right * m00 + m03;
					float yBottom = bottom * m11 + m13;
					float yTop = top * m11 + m13;
					positionColumn[target + 0] = new Vector3(xLeft, yBottom, m23);
					positionColumn[target + 1] = new Vector3(xLeft, yTop, m23);
					positionColumn[target + 2] = new Vector3(xRight, yTop, m23);
					positionColumn[target + 3] = new Vector3(xRight, yBottom, m23);
				}
				else
				{
					float xLeft0 = left * m00;
					float xRight0 = right * m00;
					float yBottom0 = bottom * m01;
					float yTop0 = top * m01;
					float xLeft1 = left * m10;
					float xRight1 = right * m10;
					float yBottom1 = bottom * m11;
					float yTop1 = top * m11;
					float xLeft2 = left * m20;
					float xRight2 = right * m20;
					float yBottom2 = bottom * m21;
					float yTop2 = top * m21;
					positionColumn[target + 0] = new Vector3(xLeft0 + yBottom0 + m03, xLeft1 + yBottom1 + m13, xLeft2 + yBottom2 + m23);
					positionColumn[target + 1] = new Vector3(xLeft0 + yTop0 + m03, xLeft1 + yTop1 + m13, xLeft2 + yTop2 + m23);
					positionColumn[target + 2] = new Vector3(xRight0 + yTop0 + m03, xRight1 + yTop1 + m13, xRight2 + yTop2 + m23);
					positionColumn[target + 3] = new Vector3(xRight0 + yBottom0 + m03, xRight1 + yBottom1 + m13, xRight2 + yBottom2 + m23);
				}
				if (writeColor)
				{
					colorColumn[target + 0] = color;
					colorColumn[target + 1] = color;
					colorColumn[target + 2] = color;
					colorColumn[target + 3] = color;
				}
				if (writeUV)
				{
					float uvLeft = uvLeftColumn[source];
					float uvBottom = uvBottomColumn[source];
					float uvRight = uvRightColumn[source];
					float uvTop = uvTopColumn[source];
					float sdfScale = sdfScaleColumn[source];
					uvColumn[target + 0] = new Vector2(uvLeft, uvBottom);
					uvColumn[target + 1] = new Vector2(uvLeft, uvTop);
					uvColumn[target + 2] = new Vector2(uvRight, uvTop);
					uvColumn[target + 3] = new Vector2(uvRight, uvBottom);
					tmpUV0Column[target + 0] = new Vector4(uvLeft, uvBottom, 0.0f, sdfScale);
					tmpUV0Column[target + 1] = new Vector4(uvLeft, uvTop, 0.0f, sdfScale);
					tmpUV0Column[target + 2] = new Vector4(uvRight, uvTop, 0.0f, sdfScale);
					tmpUV0Column[target + 3] = new Vector4(uvRight, uvBottom, 0.0f, sdfScale);
					tmpUV2Column[target + 0] = new Vector2(0.0f, 0.0f);
					tmpUV2Column[target + 1] = new Vector2(0.0f, 1.0f);
					tmpUV2Column[target + 2] = new Vector2(1.0f, 1.0f);
					tmpUV2Column[target + 3] = new Vector2(1.0f, 0.0f);
				}
			}
			finishSimpleTextGeometryWrite(slot, glyphCount * 4, vertexStart);
			if (writeUV)
			{
				markUVSlotDirty(slot);
			}
			if (writeColor)
			{
				addSlotVertexRange(mDirtyColorRanges, slot);
			}
		}
	}

	// 为一批首次创建的Text Slot一次性分配连续物理Vertex Arena。
	// 每个Text仍保留自己的Stable Glyph Capacity，只把N次allocateVertexRange/ensureCPUVertexCapacity折叠为一次。
	// 存在Free Vertex Range时保守回退旧逐Slot路径，避免改变碎片复用策略。
	public bool tryAllocateInitialSimpleTextRangeBatch(Int_ECSList slots, Int_ECSList vertexCapacities, int slotCount,
		out int batchVertexStart, out int batchVertexCapacity)
	{
		batchVertexStart = -1;
		batchVertexCapacity = 0;
		if (slots == null || vertexCapacities == null || slotCount <= 0 || slotCount > slots.Count || slotCount > vertexCapacities.Count || mFreeVertexRanges.Count != 0)
		{
			return false;
		}
		var slotColumn = slots.getValueColumn();
		var capacityColumn = vertexCapacities.getValueColumn();
		var vertexStarts = mGeometryRanges.getVertexStartColumn();
		var vertexCounts = mGeometryRanges.getVertexCountColumn();
		var vertexRangeCapacities = mGeometryRanges.getVertexCapacityColumn();
		long totalCapacityLong = 0;
		int maxSlot = -1;
		for (int i = 0; i < slotCount; ++i)
		{
			int slot = slotColumn[i];
			int capacity = capacityColumn[i];
			if ((uint)slot >= (uint)mGeometryRanges.Count || capacity <= 0 || vertexStarts[slot] >= 0 || vertexCounts[slot] != 0 || vertexRangeCapacities[slot] != 0)
			{
				return false;
			}
			if (slot > maxSlot)
			{
				maxSlot = slot;
			}
			totalCapacityLong += capacity;
			if (totalCapacityLong > int.MaxValue)
			{
				return false;
			}
		}
		int totalCapacity = (int)totalCapacityLong;
		int start = allocateVertexRange(totalCapacity);
		ensureCPUVertexCapacity(start + totalCapacity);
		// ensureCPUVertexCapacity可能搬迁EasyECS底层，之后重新获取Geometry Direct Column。
		vertexStarts = mGeometryRanges.getVertexStartColumn();
		vertexCounts = mGeometryRanges.getVertexCountColumn();
		vertexRangeCapacities = mGeometryRanges.getVertexCapacityColumn();
		// 旧路径对4000个Text逐Slot调用EnsureCount。最终Count本来就一定达到maxSlot+1，
		// 所以先一次扩到最终Count完全等价，同时把“状态表首次增长”和“Range连续写”拆开计时。
		ensureSimpleTextSlotState(maxSlot);
		var activeVertexCounts = mSimpleTextActiveVertexCounts.getValueColumn();
		var clearEndVertexCounts = mSimpleTextClearEndVertexCounts.getValueColumn();
		int cursor = start;
		for (int i = 0; i < slotCount; ++i)
		{
			int slot = slotColumn[i];
			int capacity = capacityColumn[i];
			activeVertexCounts[slot] = PREALLOCATED_SIMPLE_TEXT_ACTIVE_VERTEX_COUNT;
			clearEndVertexCounts[slot] = 0;
			vertexStarts[slot] = cursor;
			vertexCounts[slot] = 0;
			vertexRangeCapacities[slot] = capacity;
			cursor += capacity;
		}
		batchVertexStart = start;
		batchVertexCapacity = totalCapacity;
		return true;
	}

	// 为一批“全新且固定4顶点”的Slot一次性分配连续物理Vertex Range。
	// 只负责Range/Stream存储，不读取Managed Element，也不改变RenderOrder；存在Free Vertex Range时保守回退原逐Slot路径，避免改变碎片复用策略。
	public bool tryAllocateInitialSimpleQuadBatch(Int_ECSList slots, int slotCount, out int batchVertexStart, out int batchVertexCount)
	{
		batchVertexStart = -1;
		batchVertexCount = 0;
		if (slots == null || slotCount <= 0 || slotCount > slots.Count || slotCount > int.MaxValue / 4 || mFreeVertexRanges.Count != 0)
		{
			return false;
		}
		var slotColumn = slots.getValueColumn();
		var vertexStarts = mGeometryRanges.getVertexStartColumn();
		var vertexCounts = mGeometryRanges.getVertexCountColumn();
		var vertexCapacities = mGeometryRanges.getVertexCapacityColumn();
		for (int i = 0; i < slotCount; ++i)
		{
			int slot = slotColumn[i];
			if ((uint)slot >= (uint)mGeometryRanges.Count || vertexStarts[slot] >= 0 || vertexCounts[slot] != 0 || vertexCapacities[slot] != 0)
			{
				return false;
			}
		}
		int totalVertexCount = slotCount * 4;
		int start = allocateVertexRange(totalVertexCount);
		ensureCPUVertexCapacity(start + totalVertexCount);
		// ensureCPUVertexCapacity可能导致EasyECS底层搬迁，Direct Column必须在它之后重新获取。
		vertexStarts = mGeometryRanges.getVertexStartColumn();
		vertexCounts = mGeometryRanges.getVertexCountColumn();
		vertexCapacities = mGeometryRanges.getVertexCapacityColumn();
		for (int i = 0; i < slotCount; ++i)
		{
			int slot = slotColumn[i];
			resetSimpleTextSlotState(slot);
			vertexStarts[slot] = start + i * 4;
			vertexCounts[slot] = 4;
			vertexCapacities[slot] = 4;
		}
		mLiveVertexCount += totalVertexCount;
		int end = start + totalVertexCount;
		mDirtyPositionRanges.Add(new FastUIRangeData(start, end));
		mDirtyColorRanges.Add(new FastUIRangeData(start, end));
		mDirtyUVRanges.Add(new FastUIRangeData(start, end));
		mHasFullUVDirty = true;
		if (mTMPVertexLayoutEnabled)
		{
			mDirtyUV2Ranges.Add(new FastUIRangeData(start, end));
		}
		batchVertexStart = start;
		batchVertexCount = totalVertexCount;
		return true;
	}
	// Sliced固定3x3网格直接写入持久Vertex Column，避免GeometryBuilder Scratch构建后再复制一遍。
	public FastUIGeometryUpdateResult rebuildSlicedImageSlot(int slot, Vector4 x, Vector4 y, Vector4 u, Vector4 v, bool fillCenter,
		bool translationOnly, Vector3 translation, Matrix4x4 geometryMatrix, Color32 color)
	{
		const float epsilon = 0.0001f;
		bool x0Valid = x.y > x.x + epsilon;
		bool x1Valid = x.z > x.y + epsilon;
		bool x2Valid = x.w > x.z + epsilon;
		bool y0Valid = y.y > y.x + epsilon;
		bool y1Valid = y.z > y.y + epsilon;
		bool y2Valid = y.w > y.z + epsilon;
		int xValidCount = (x0Valid ? 1 : 0) + (x1Valid ? 1 : 0) + (x2Valid ? 1 : 0);
		int yValidCount = (y0Valid ? 1 : 0) + (y1Valid ? 1 : 0) + (y2Valid ? 1 : 0);
		int quadCount = xValidCount * yValidCount;
		if (!fillCenter && x1Valid && y1Valid)
		{
			--quadCount;
		}
		int vertexCount = Mathf.Max(quadCount, 0) * 4;
		FastUIGeometryUpdateResult result = ensureGeometryRange(slot, vertexCount);
		if (vertexCount <= 0)
		{
			return result;
		}
		int vertexStart = result.mVertexStart;
		var positionColumn = mPositionECS.getPositionColumn();
		var colorColumn = mColorECS.getColorColumn();
		var uvColumn = mUVECS.getUVColumn();
		var tmpUV0Column = mTMPVertexLayoutEnabled ? mTMPUV0ECS.getUVColumn() : default;
		var tmpUV2Column = mTMPVertexLayoutEnabled ? mTMPUV2ECS.getUVColumn() : default;
		int write = vertexStart;
		for (int yi = 0; yi < 3; ++yi)
		{
			float yMin = y[yi];
			float yMax = y[yi + 1];
			if (yMax <= yMin + epsilon)
			{
				continue;
			}
			float vMin = v[yi];
			float vMax = v[yi + 1];
			for (int xi = 0; xi < 3; ++xi)
			{
				if (!fillCenter && xi == 1 && yi == 1)
				{
					continue;
				}
				float xMin = x[xi];
				float xMax = x[xi + 1];
				if (xMax <= xMin + epsilon)
				{
					continue;
				}
				float uMin = u[xi];
				float uMax = u[xi + 1];
				Vector3 bottomLeft = new(xMin, yMin, 0.0f);
				Vector3 topLeft = new(xMin, yMax, 0.0f);
				Vector3 topRight = new(xMax, yMax, 0.0f);
				Vector3 bottomRight = new(xMax, yMin, 0.0f);
				if (translationOnly)
				{
					bottomLeft += translation;
					topLeft += translation;
					topRight += translation;
					bottomRight += translation;
				}
				else
				{
					bottomLeft = geometryMatrix.MultiplyPoint3x4(bottomLeft);
					topLeft = geometryMatrix.MultiplyPoint3x4(topLeft);
					topRight = geometryMatrix.MultiplyPoint3x4(topRight);
					bottomRight = geometryMatrix.MultiplyPoint3x4(bottomRight);
				}
				Vector2 uvBottomLeft = new(uMin, vMin);
				Vector2 uvTopLeft = new(uMin, vMax);
				Vector2 uvTopRight = new(uMax, vMax);
				Vector2 uvBottomRight = new(uMax, vMin);
				positionColumn[write + 0] = bottomLeft;
				positionColumn[write + 1] = topLeft;
				positionColumn[write + 2] = topRight;
				positionColumn[write + 3] = bottomRight;
				colorColumn[write + 0] = color;
				colorColumn[write + 1] = color;
				colorColumn[write + 2] = color;
				colorColumn[write + 3] = color;
				uvColumn[write + 0] = uvBottomLeft;
				uvColumn[write + 1] = uvTopLeft;
				uvColumn[write + 2] = uvTopRight;
				uvColumn[write + 3] = uvBottomRight;
				if (mTMPVertexLayoutEnabled)
				{
					tmpUV0Column[write + 0] = new Vector4(uvBottomLeft.x, uvBottomLeft.y, 0.0f, 1.0f);
					tmpUV0Column[write + 1] = new Vector4(uvTopLeft.x, uvTopLeft.y, 0.0f, 1.0f);
					tmpUV0Column[write + 2] = new Vector4(uvTopRight.x, uvTopRight.y, 0.0f, 1.0f);
					tmpUV0Column[write + 3] = new Vector4(uvBottomRight.x, uvBottomRight.y, 0.0f, 1.0f);
					tmpUV2Column[write + 0] = default;
					tmpUV2Column[write + 1] = default;
					tmpUV2Column[write + 2] = default;
					tmpUV2Column[write + 3] = default;
				}
				write += 4;
			}
		}
		return result;
	}
	// SimpleQuad专用Position路径。首次创建时可直接从0顶点建立4顶点Range，避免先走GeometryBuilder再回写SOA。
	public FastUIGeometryUpdateResult rebuildSimpleQuadPositionSlot(int slot, Vector3 bottomLeft, Vector3 topLeft, Vector3 topRight, Vector3 bottomRight)
	{
		if (slot < 0)
		{
			return default;
		}
		ensureGeometrySlotRecord(slot);
		int currentVertexCount = getGeometryVertexCount(slot);
		if (currentVertexCount != 0 && currentVertexCount != 4)
		{
			return new FastUIGeometryUpdateResult(getGeometryVertexStart(slot), currentVertexCount, false, false, false);
		}
		FastUIGeometryUpdateResult result = ensureGeometryRange(slot, 4);
		int vertexStart = result.mVertexStart;
		var positionColumn = mPositionECS.getPositionColumn();
		positionColumn[vertexStart + 0] = bottomLeft;
		positionColumn[vertexStart + 1] = topLeft;
		positionColumn[vertexStart + 2] = topRight;
		positionColumn[vertexStart + 3] = bottomRight;
		return result;
	}
	// 兼容旧版单Quad调用入口。
	public void setPositionSlot(int slot, Vector3 bottomLeft, Vector3 topLeft, Vector3 topRight, Vector3 bottomRight)
	{
		rebuildSimpleQuadPositionSlot(slot, bottomLeft, topLeft, topRight, bottomRight);
	}
	public void setColorSlot(int slot, Color32 color)
	{
		int vertexStart = getGeometryVertexStart(slot);
		int vertexCount = getGeometryVertexCount(slot);
		if (vertexStart < 0 || vertexCount <= 0 || mColorECS == null)
		{
			return;
		}
		var colorColumn = mColorECS.getColorColumn();
		for (int i = 0; i < vertexCount; ++i)
		{
			colorColumn[vertexStart + i] = color;
		}
	}

	public void setUVSlot(int slot, Rect uvRect)
	{
		int vertexStart = getGeometryVertexStart(slot);
		int vertexCount = getGeometryVertexCount(slot);
		if (vertexStart < 0 || vertexCount != 4 || mUVECS == null)
		{
			return;
		}
		float left = uvRect.xMin;
		float bottom = uvRect.yMin;
		float right = uvRect.xMax;
		float top = uvRect.yMax;
		var uvColumn = mUVECS.getUVColumn();
		uvColumn[vertexStart + 0] = new Vector2(left, bottom);
		uvColumn[vertexStart + 1] = new Vector2(left, top);
		uvColumn[vertexStart + 2] = new Vector2(right, top);
		uvColumn[vertexStart + 3] = new Vector2(right, bottom);
		if (mTMPVertexLayoutEnabled)
		{
			var tmpUV0Column = mTMPUV0ECS.getUVColumn();
			var tmpUV2Column = mTMPUV2ECS.getUVColumn();
			tmpUV0Column[vertexStart + 0] = new Vector4(left, bottom, 0.0f, 1.0f);
			tmpUV0Column[vertexStart + 1] = new Vector4(left, top, 0.0f, 1.0f);
			tmpUV0Column[vertexStart + 2] = new Vector4(right, top, 0.0f, 1.0f);
			tmpUV0Column[vertexStart + 3] = new Vector4(right, bottom, 0.0f, 1.0f);
			tmpUV2Column[vertexStart + 0] = default;
			tmpUV2Column[vertexStart + 1] = default;
			tmpUV2Column[vertexStart + 2] = default;
			tmpUV2Column[vertexStart + 3] = default;
		}
	}

	public void offsetPositionSlotRange(int startSlot, int slotCount, Vector3 canvasDelta)
	{
		if (slotCount <= 0 || mPositionECS == null)
		{
			return;
		}
		int safeStart = Mathf.Max(startSlot, 0);
		int endSlot = Mathf.Min(startSlot + slotCount, mGeometryRanges.Count);
		int safeCount = endSlot - safeStart;
		if (safeCount <= 0)
		{
			return;
		}
		if (FastUIBurstPositionJobs.shouldUseOffset(safeCount))
		{
			FastUIBurstPositionJobs.applyOffsetSlotRange(mGeometryRanges, mPositionECS, safeStart, safeCount, canvasDelta);
			return;
		}
		var vertexStartColumn = mGeometryRanges.getVertexStartColumn();
		var vertexCountColumn = mGeometryRanges.getVertexCountColumn();
		var positionColumn = mPositionECS.getPositionColumn();
		for (int slot = safeStart; slot < endSlot; ++slot)
		{
			int vertexStart = vertexStartColumn[slot];
			int vertexCount = vertexCountColumn[slot];
			for (int i = 0; i < vertexCount; ++i)
			{
				positionColumn[vertexStart + i] += canvasDelta;
			}
		}
	}
	public int offsetPositionSlotRanges(FastUIRangeData_ECSList slotRanges, int rangeCount, Vector3 canvasDelta, out int dirtyVertexStart, out int dirtyVertexEnd)
	{
		dirtyVertexStart = int.MaxValue;
		dirtyVertexEnd = -1;
		if (slotRanges == null || rangeCount <= 0 || mPositionECS == null || canvasDelta == Vector3.zero)
		{
			return 0;
		}
		int count = Mathf.Min(rangeCount, slotRanges.Count);
		if (count <= 0)
		{
			return 0;
		}
		var rangeStarts = slotRanges.getStartColumn();
		var rangeEnds = slotRanges.getEndColumn();
		var vertexStarts = mGeometryRanges.getVertexStartColumn();
		var vertexCounts = mGeometryRanges.getVertexCountColumn();
		var positions = mPositionECS.getPositionColumn();
		int geometryCount = mGeometryRanges.Count;
		int changedVertexCount = 0;
		for (int rangeIndex = 0; rangeIndex < count; ++rangeIndex)
		{
			int startSlot = Mathf.Clamp(rangeStarts[rangeIndex], 0, geometryCount);
			int endSlot = Mathf.Clamp(rangeEnds[rangeIndex], startSlot, geometryCount);
			for (int slot = startSlot; slot < endSlot; ++slot)
			{
				int vertexStart = vertexStarts[slot];
				int vertexCount = vertexCounts[slot];
				if (vertexStart < 0 || vertexCount <= 0)
				{
					continue;
				}
				dirtyVertexStart = Mathf.Min(dirtyVertexStart, vertexStart);
				dirtyVertexEnd = Mathf.Max(dirtyVertexEnd, vertexStart + vertexCount);
				changedVertexCount += vertexCount;
				for (int vertex = 0; vertex < vertexCount; ++vertex)
				{
					positions[vertexStart + vertex] += canvasDelta;
				}
			}
		}
		return changedVertexCount;
	}

	// PackedQueue批量应用PositionDelta。小规模继续Direct Column，大规模使用EasyECS BurstView并行原地更新。
	public void applyPositionDeltaQueue(FastUIPositionDeltaData_ECSList deltaECS, int deltaCount)
	{
		if (deltaECS == null || deltaCount <= 0 || mPositionECS == null)
		{
			return;
		}
		int count = Mathf.Min(deltaCount, deltaECS.Count);
		if (count <= 0)
		{
			return;
		}

		if (FastUIBurstPositionJobs.shouldUseDeltaQueue(count))
		{
			FastUIBurstPositionJobs.applyPositionDeltaQueue(deltaECS, mGeometryRanges, mPositionECS, count);
			return;
		}
		var slotColumn = deltaECS.getVertexSlotColumn();
		var deltaColumn = deltaECS.getDeltaColumn();
		var vertexStartColumn = mGeometryRanges.getVertexStartColumn();
		var vertexCountColumn = mGeometryRanges.getVertexCountColumn();
		var positionColumn = mPositionECS.getPositionColumn();
		for (int i = 0; i < count; ++i)
		{
			int slot = slotColumn[i];
			if ((uint)slot >= (uint)mGeometryRanges.Count)
			{
				continue;
			}
			int vertexStart = vertexStartColumn[slot];
			int vertexCount = vertexCountColumn[slot];
			Vector3 delta = deltaColumn[i];
			for (int vertex = 0; vertex < vertexCount; ++vertex)
			{
				positionColumn[vertexStart + vertex] += delta;
			}
		}
	}

	// 结构容量发生变化时统一调整CPU/GPU Buffer。Direct Column只能在结构变化完成后重新获取。
	public void syncVertexBufferCapacity(ref FastUIFrameStats stats)
	{
		int requiredVertexCapacity = Mathf.NextPowerOfTwo(Mathf.Max(mVertexSpan, 4));
		bool capacityChanged = requiredVertexCapacity > mGPUVertexCapacity;
		if (!capacityChanged && !mVertexLayoutDirty)
		{
			return;
		}
		if (capacityChanged)
		{
			mGPUVertexCapacity = requiredVertexCapacity;
		}
		releaseDirectGPUBuffers();
		mDirectGPUVertexSeedRequired = mVertexUploadBackend == FastUIVertexUploadBackend.DirectGPUBuffer;
		// SetVertexBufferParams can reset Mesh state. Preserve the renderer-owned Editor preview
		// bounds instead of replacing them with HUGE_BOUNDS, otherwise the Canvas transform can
		// turn that large local bound into an invalid world AABB in SceneView/Inspector events.
#if UNITY_EDITOR
		Bounds preservedEditorBounds = mMesh.bounds;
#endif
		if (mTMPVertexLayoutEnabled)
		{
			ensureTMPVertexCapacity(Mathf.Max(mPositionDataCount, mGPUVertexCapacity));
			mMesh.SetVertexBufferParams(mGPUVertexCapacity, FastUIVertex.TMP_VERTEX_LAYOUT);
		}
		else
		{
			mMesh.SetVertexBufferParams(mGPUVertexCapacity, FastUIVertex.IMAGE_VERTEX_LAYOUT);
		}
#if UNITY_EDITOR
		// FastUIMeshRenderer owns a transform-safe Editor bound in both Edit Mode and Play Mode.
		mMesh.bounds = preservedEditorBounds;
#else
		mMesh.bounds = FastUIMeshUtility.HUGE_BOUNDS;
#endif
		forceFullVertexStreams(capacityChanged ? FULL_UPLOAD_REASON_BUFFER_RESIZE : FULL_UPLOAD_REASON_FORCE);
		mVertexLayoutDirty = false;
		if (capacityChanged)
		{
			stats.mVertexBufferResized = true;
		}
	}

	// 仅上传本帧实际Dirty的Position/Color/UV顶点区间，未变化Stream不会产生Mesh API调用。
	// DirectGPUBuffer会持续缓存GraphicsBuffer wrapper；只有已知会重建Mesh GPU Buffer的操作才主动失效缓存。
	public void uploadDirtyVertexStreams(ref FastUIFrameStats stats)
	{
		if (mVertexUploadBackend == FastUIVertexUploadBackend.DirectGPUBuffer && mDirectGPUVertexSeedRequired)
		{
			if (seedDirectGPUVertexBuffers(ref stats))
			{
				return;
			}
			// 当前Mesh/平台若无法建立Direct Vertex Buffer，统一退回Unity标准Mesh上传。
			// 不保留半Direct状态，后续所有Stream继续使用同一Backend，保证表现与Mesh原生状态一致。
			mVertexUploadBackend = FastUIVertexUploadBackend.MeshCPUCopy;
			mDirectGPUVertexSeedRequired = false;
			forceFullVertexStreams(FULL_UPLOAD_REASON_FORCE);
		}
		uploadPositionStream(ref stats);
		uploadColorStream(ref stats);
		uploadUVStream(ref stats);
		if (mDirectGPUVertexFallbackSyncPending)
		{
			syncFullVertexCPUCopyAfterDirectFallback(ref stats);
			mDirectGPUVertexFallbackSyncPending = false;
		}
	}

	public void clearDirtyVertexRanges()
	{
		mDirtyPositionRanges.Clear();
		mDirtyColorRanges.Clear();
		mDirtyUVRanges.Clear();
		mDirtyUV2Ranges.Clear();
		mHasFullUVDirty = false;
		mMergedRanges.Clear();
	}

	private bool seedDirectGPUVertexBuffers(ref FastUIFrameStats stats)
	{
		if (mMesh == null)
		{
			return false;
		}
		int vertexCount = mVertexSpan;
		if (vertexCount <= 0)
		{
			mDirectGPUVertexSeedRequired = false;
			clearVertexUploadStateAfterFullSeed();
			return true;
		}
		if (mPositionECS == null || mColorECS == null || mUVECS == null ||
			vertexCount > mPositionDataCount || vertexCount > mColorDataCount || vertexCount > mUVDataCount)
		{
			return false;
		}
		// GetVertexBuffer返回的wrapper必须在Mesh API改写前释放；Seed完成后稳定帧再重新获取。
		releaseDirectGPUVertexBuffers();
		try
		{
			var positionColumn = mPositionECS.getPositionColumn();
			ref Vector3 firstPosition = ref positionColumn[0];
			NativeArray<Vector3> positionView = FastUINativeArrayBridge.createView(ref firstPosition, vertexCount);
			uploadVertexStreamDataCPU(positionView, 0, vertexCount, FastUIVertex.POSITION_STREAM);
			++stats.mVertexUploadCallCount;
			stats.mUploadedVertexCount += vertexCount;
			var colorColumn = mColorECS.getColorColumn();
			ref Color32 firstColor = ref colorColumn[0];
			NativeArray<Color32> colorView = FastUINativeArrayBridge.createView(ref firstColor, vertexCount);
			uploadVertexStreamDataCPU(colorView, 0, vertexCount, FastUIVertex.COLOR_STREAM);
			++stats.mVertexUploadCallCount;
			stats.mUploadedVertexCount += vertexCount;
			if (mTMPVertexLayoutEnabled)
			{
				if (mTMPUV0ECS == null || mTMPUV2ECS == null || vertexCount > mTMPUV0DataCount || vertexCount > mTMPUV2DataCount)
				{
					return false;
				}
				var uv0Column = mTMPUV0ECS.getUVColumn();
				ref Vector4 firstUV0 = ref uv0Column[0];
				NativeArray<Vector4> uv0View = FastUINativeArrayBridge.createView(ref firstUV0, vertexCount);
				uploadVertexStreamDataCPU(uv0View, 0, vertexCount, FastUIVertex.UV_STREAM);
				var uv2Column = mTMPUV2ECS.getUVColumn();
				ref Vector2 firstUV2 = ref uv2Column[0];
				NativeArray<Vector2> uv2View = FastUINativeArrayBridge.createView(ref firstUV2, vertexCount);
				uploadVertexStreamDataCPU(uv2View, 0, vertexCount, FastUIVertex.TMP_UV2_STREAM);
				stats.mVertexUploadCallCount += 2;

				stats.mUploadedVertexCount += vertexCount;
			}
			else
			{
				var uvColumn = mUVECS.getUVColumn();
				ref Vector2 firstUV = ref uvColumn[0];
				NativeArray<Vector2> uvView = FastUINativeArrayBridge.createView(ref firstUV, vertexCount);
				uploadVertexStreamDataCPU(uvView, 0, vertexCount, FastUIVertex.UV_STREAM);
				++stats.mVertexUploadCallCount;
				stats.mUploadedVertexCount += vertexCount;
			}
		}
		catch (System.Exception)
		{
			releaseDirectGPUVertexBuffers();
			return false;
		}
		mDirectGPUVertexSeedRequired = false;

		clearVertexUploadStateAfterFullSeed();
		return true;
	}
	private void clearVertexUploadStateAfterFullSeed()
	{
		mForceFullPositionUpload = false;
		mForceFullColorUpload = false;
		mForceFullUVUpload = false;
		mForceFullPositionReason = FULL_UPLOAD_REASON_NONE;
		mForceFullColorReason = FULL_UPLOAD_REASON_NONE;
		mForceFullUVReason = FULL_UPLOAD_REASON_NONE;
		mDirtyPositionRanges.Clear();
		mDirtyColorRanges.Clear();
		mDirtyUVRanges.Clear();
		mDirtyUV2Ranges.Clear();
		mHasFullUVDirty = false;
		mMergedRanges.Clear();
	}
	private void uploadPositionStream(ref FastUIFrameStats stats)
	{
		int mergedCount = buildMergedRanges(mDirtyPositionRanges, POSITION_BYTES_PER_VERTEX, 1, mUploadCallPenaltyBytes, out int mergedVertexCount);

		FastUIDirtyUploadChoice choice = chooseDirtyUploadChoice(mForceFullPositionUpload, mergedCount, mergedVertexCount, POSITION_BYTES_PER_VERTEX, 1, mUploadCallPenaltyBytes);
		if (choice == FastUIDirtyUploadChoice.None)
		{
			return;
		}
		if (!FastUIPositionData_ECSList.IsUnsafeBackend)
		{
			Debug.LogError("[FastUI] Position直接上传要求EasyECS Unsafe Backend,当前:" + FastUIPositionData_ECSList.BackendName + ",Reason:" + FastUIPositionData_ECSList.BackendReason);
			mForceFullPositionUpload = false;
			mForceFullPositionReason = FULL_UPLOAD_REASON_NONE;
			mDirtyPositionRanges.Clear();
			return;
		}
		using (UPLOAD_POSITION_MARKER.Auto())
		{
			if (choice == FastUIDirtyUploadChoice.Full)
			{
				int vertexCount = mVertexSpan;
				if (vertexCount > 0)
				{
					uploadPositionRangeDirect(0, vertexCount);
					++stats.mVertexUploadCallCount;
					stats.mUploadedVertexCount += vertexCount;
				}
			}
			else if (choice == FastUIDirtyUploadChoice.Envelope)
			{
				getMergedEnvelope(mergedCount, out int startVertex, out int vertexCount);
				if (vertexCount > 0)
				{
					uploadPositionRangeDirect(startVertex, vertexCount);
					++stats.mVertexUploadCallCount;
					stats.mUploadedVertexCount += vertexCount;
				}
			}
			else
			{
				var rangeStarts = mMergedRanges.getStartColumn();
				var rangeEnds = mMergedRanges.getEndColumn();
				for (int i = 0; i < mergedCount; ++i)
				{
					int startVertex = rangeStarts[i];
					int vertexCount = rangeEnds[i] - startVertex;
					if (vertexCount <= 0)
					{
						continue;
					}
					uploadPositionRangeDirect(startVertex, vertexCount);
					++stats.mVertexUploadCallCount;
					stats.mUploadedVertexCount += vertexCount;
				}
			}
		}
		mForceFullPositionUpload = false;
		mForceFullPositionReason = FULL_UPLOAD_REASON_NONE;
		mDirtyPositionRanges.Clear();
	}
	private void uploadPositionRangeDirect(int startVertex, int vertexCount)
	{
		if (vertexCount <= 0 || mPositionECS == null || startVertex < 0 || startVertex + vertexCount > mPositionDataCount)
		{
			return;
		}
		var positionColumn = mPositionECS.getPositionColumn();
		ref Vector3 firstPosition = ref positionColumn[startVertex];
		NativeArray<Vector3> positionView = FastUINativeArrayBridge.createView(ref firstPosition, vertexCount);
		uploadVertexStreamData(positionView, startVertex, vertexCount, FastUIVertex.POSITION_STREAM);
	}
	private void uploadColorStream(ref FastUIFrameStats stats)
	{
		int mergedCount = buildMergedRanges(mDirtyColorRanges, COLOR_BYTES_PER_VERTEX, 1, mUploadCallPenaltyBytes, out int mergedVertexCount);

		FastUIDirtyUploadChoice choice = chooseDirtyUploadChoice(mForceFullColorUpload, mergedCount, mergedVertexCount, COLOR_BYTES_PER_VERTEX, 1, mUploadCallPenaltyBytes);
		if (choice == FastUIDirtyUploadChoice.None)
		{
			return;
		}
		using (UPLOAD_COLOR_MARKER.Auto())
		{
			if (choice == FastUIDirtyUploadChoice.Full)
			{
				uploadColorRangeDirect(0, mVertexSpan);
				++stats.mVertexUploadCallCount;
				stats.mUploadedVertexCount += mVertexSpan;
			}
			else if (choice == FastUIDirtyUploadChoice.Envelope)
			{
				getMergedEnvelope(mergedCount, out int startVertex, out int vertexCount);
				uploadColorRangeDirect(startVertex, vertexCount);
				++stats.mVertexUploadCallCount;
				stats.mUploadedVertexCount += vertexCount;
			}
			else
			{
				var starts = mMergedRanges.getStartColumn();
				var ends = mMergedRanges.getEndColumn();
				for (int i = 0; i < mergedCount; ++i)
				{
					int startVertex = starts[i];
					int vertexCount = ends[i] - startVertex;
					if (vertexCount <= 0)
					{
						continue;
					}
					uploadColorRangeDirect(startVertex, vertexCount);
					++stats.mVertexUploadCallCount;
					stats.mUploadedVertexCount += vertexCount;
				}
			}
		}
		mForceFullColorUpload = false;
		mForceFullColorReason = FULL_UPLOAD_REASON_NONE;
		mDirtyColorRanges.Clear();
	}
	private void uploadUVStream(ref FastUIFrameStats stats)
	{
		int dirtyCount = mDirtyUVRanges.Count;
		// UV0继续沿用旧版“TMP Canvas=24B/vertex + 2次调用”的AdaptiveCost决策模型，
		// 保持既定的Range成本模型不变；仅在确认整帧都是UV0Only时切换到实测更优的512B调用成本。
		int policyBytesPerVertex = mTMPVertexLayoutEnabled ? TMP_UV_BYTES_PER_VERTEX : IMAGE_UV_BYTES_PER_VERTEX;
		int policyCallsPerRange = mTMPVertexLayoutEnabled ? 2 : 1;
		bool pureUV0Only = dirtyCount > 0 && !mHasFullUVDirty && !mForceFullUVUpload;
		int uv0PenaltyBytes = pureUV0Only ? mUV0OnlyUploadCallPenaltyBytes : mUploadCallPenaltyBytes;
		int mergedCount = buildMergedRanges(mDirtyUVRanges, policyBytesPerVertex, policyCallsPerRange, uv0PenaltyBytes, out int mergedVertexCount);

		FastUIDirtyUploadChoice choice = chooseDirtyUploadChoice(mForceFullUVUpload, mergedCount, mergedVertexCount, policyBytesPerVertex, policyCallsPerRange, uv0PenaltyBytes);
		bool hasUV2Work = mTMPVertexLayoutEnabled && (mForceFullUVUpload || mDirtyUV2Ranges.Count > 0);
		// UV2只会由markUVSlotDirty与UV0成对追加；若Count完全相同，说明本帧没有UV0Only图片更新，
		// 两个Dirty序列天然一一对应，可直接复用UV0已经完成的排序/合并结果，避免Text路径重复Merge。
		bool reuseUV0MergedForUV2 = !mForceFullUVUpload && dirtyCount > 0 && dirtyCount == mDirtyUV2Ranges.Count;
		if (choice == FastUIDirtyUploadChoice.None && !hasUV2Work)
		{
			mDirtyUVRanges.Clear();
			mDirtyUV2Ranges.Clear();
			mHasFullUVDirty = false;
			return;
		}
		using (UPLOAD_UV_MARKER.Auto())
		{
			if (choice == FastUIDirtyUploadChoice.Full)
			{
				uploadUV0RangeDirect(0, mVertexSpan);
				++stats.mVertexUploadCallCount;
				stats.mUploadedVertexCount += mVertexSpan;
			}
			else if (choice == FastUIDirtyUploadChoice.Envelope)
			{
				getMergedEnvelope(mergedCount, out int startVertex, out int vertexCount);
				if (vertexCount > 0)
				{
					uploadUV0RangeDirect(startVertex, vertexCount);
					++stats.mVertexUploadCallCount;
					stats.mUploadedVertexCount += vertexCount;
				}
			}
			else if (choice == FastUIDirtyUploadChoice.Partial)
			{
				var starts = mMergedRanges.getStartColumn();
				var ends = mMergedRanges.getEndColumn();
				for (int i = 0; i < mergedCount; ++i)
				{
					int startVertex = starts[i];
					int vertexCount = ends[i] - startVertex;
					if (vertexCount <= 0)
					{
						continue;
					}
					uploadUV0RangeDirect(startVertex, vertexCount);
					++stats.mVertexUploadCallCount;
					stats.mUploadedVertexCount += vertexCount;
				}
			}
			if (hasUV2Work)
			{
				uploadUV2Stream(ref stats, reuseUV0MergedForUV2, mergedCount, choice);
			}
		}
		mForceFullUVUpload = false;
		mForceFullUVReason = FULL_UPLOAD_REASON_NONE;
		mDirtyUVRanges.Clear();
		mDirtyUV2Ranges.Clear();
		mHasFullUVDirty = false;
	}
	private void uploadUV2Stream(ref FastUIFrameStats stats, bool reuseUV0Merged, int uv0MergedCount, FastUIDirtyUploadChoice uv0Choice)
	{
		int mergedCount;
		FastUIDirtyUploadChoice choice;
		if (reuseUV0Merged)
		{
			// 纯Text/完整UV更新时，UV0与UV2 Dirty Range来源完全一致。复用同一份MergedRanges，
			// 同时沿用原24B+2Calls组合成本决策，保持原有决策语义，同时省掉第二次Sort/Merge。
			mergedCount = uv0MergedCount;
			choice = uv0Choice;
		}
		else
		{
			mergedCount = buildMergedRanges(mDirtyUV2Ranges, IMAGE_UV_BYTES_PER_VERTEX, 1, mUploadCallPenaltyBytes, out int mergedVertexCount);
			choice = chooseDirtyUploadChoice(mForceFullUVUpload, mergedCount, mergedVertexCount, IMAGE_UV_BYTES_PER_VERTEX, 1, mUploadCallPenaltyBytes);
		}
		if (choice == FastUIDirtyUploadChoice.Full)
		{
			uploadUV2RangeDirect(0, mVertexSpan);

			++stats.mVertexUploadCallCount;
		}
		else if (choice == FastUIDirtyUploadChoice.Envelope)
		{
			getMergedEnvelope(mergedCount, out int startVertex, out int vertexCount);
			if (vertexCount > 0)
			{
				uploadUV2RangeDirect(startVertex, vertexCount);

				++stats.mVertexUploadCallCount;
			}
		}
		else if (choice == FastUIDirtyUploadChoice.Partial)
		{
			var starts = mMergedRanges.getStartColumn();
			var ends = mMergedRanges.getEndColumn();
			for (int i = 0; i < mergedCount; ++i)
			{
				int startVertex = starts[i];
				int vertexCount = ends[i] - startVertex;
				if (vertexCount <= 0)
				{
					continue;
				}
				uploadUV2RangeDirect(startVertex, vertexCount);

				++stats.mVertexUploadCallCount;
			}
		}
	}

	private void uploadColorRangeDirect(int startVertex, int vertexCount)
	{
		if (vertexCount <= 0 || mColorECS == null || startVertex < 0 || startVertex + vertexCount > mColorDataCount)
		{
			return;
		}
		var colorColumn = mColorECS.getColorColumn();
		ref Color32 first = ref colorColumn[startVertex];
		NativeArray<Color32> view = FastUINativeArrayBridge.createView(ref first, vertexCount);
		uploadVertexStreamData(view, startVertex, vertexCount, FastUIVertex.COLOR_STREAM);
	}
	private void uploadUV0RangeDirect(int startVertex, int vertexCount)
	{
		if (vertexCount <= 0 || startVertex < 0 || startVertex + vertexCount > mUVDataCount)
		{
			return;
		}
		if (mTMPVertexLayoutEnabled)
		{
			if (mTMPUV0ECS == null)
			{
				return;
			}
			var uv0Column = mTMPUV0ECS.getUVColumn();
			ref Vector4 firstUV0 = ref uv0Column[startVertex];
			NativeArray<Vector4> uv0View = FastUINativeArrayBridge.createView(ref firstUV0, vertexCount);
			uploadVertexStreamData(uv0View, startVertex, vertexCount, FastUIVertex.UV_STREAM);
			return;
		}
		if (mUVECS == null)
		{
			return;
		}
		var uvColumn = mUVECS.getUVColumn();
		ref Vector2 first = ref uvColumn[startVertex];
		NativeArray<Vector2> view = FastUINativeArrayBridge.createView(ref first, vertexCount);
		uploadVertexStreamData(view, startVertex, vertexCount, FastUIVertex.UV_STREAM);
	}
	private void uploadUV2RangeDirect(int startVertex, int vertexCount)
	{
		if (!mTMPVertexLayoutEnabled || mTMPUV2ECS == null || vertexCount <= 0 || startVertex < 0 || startVertex + vertexCount > mUVDataCount)
		{
			return;
		}
		var uv2Column = mTMPUV2ECS.getUVColumn();
		ref Vector2 firstUV2 = ref uv2Column[startVertex];
		NativeArray<Vector2> uv2View = FastUINativeArrayBridge.createView(ref firstUV2, vertexCount);
		uploadVertexStreamData(uv2View, startVertex, vertexCount, FastUIVertex.TMP_UV2_STREAM);
	}

	private void uploadVertexStreamDataCPU<T>(NativeArray<T> view, int startVertex, int vertexCount, int stream) where T : struct
	{
		mMesh.SetVertexBufferData(
			view,
			0,
			startVertex,
			vertexCount,
			stream,
			MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontNotifyMeshUsers
		);
	}
	private void uploadVertexStreamData<T>(NativeArray<T> view, int startVertex, int vertexCount, int stream) where T : struct
	{
		if (mVertexUploadBackend == FastUIVertexUploadBackend.DirectGPUBuffer)
		{
			try
			{
				GraphicsBuffer buffer = getDirectGPUVertexBuffer(stream);
				if (buffer != null && buffer.IsValid())
				{
					buffer.SetData(view, 0, startVertex, vertexCount);

					return;
				}
			}
			catch (System.Exception)
			{
				releaseDirectGPUVertexBuffers();
			}
		}
		uploadVertexStreamDataCPU(view, startVertex, vertexCount, stream);
		if (mVertexUploadBackend == FastUIVertexUploadBackend.DirectGPUBuffer)
		{
			// Direct Vertex在当前平台/当前Mesh不可用时稳定降级到MeshCPUCopy。
			// 不按场景、顶点数量或Dirty规模反复切换；本帧结束前完整同步一次CPU Mesh副本，确保此前Direct写入不会留下分叉状态。
			releaseDirectGPUVertexBuffers();
			mVertexUploadBackend = FastUIVertexUploadBackend.MeshCPUCopy;
			mDirectGPUVertexFallbackSyncPending = true;
		}
	}
	private void syncFullVertexCPUCopyAfterDirectFallback(ref FastUIFrameStats stats)
	{
		int vertexCount = mVertexSpan;
		if (vertexCount <= 0)
		{
			return;
		}
		uploadPositionRangeDirect(0, vertexCount);
		++stats.mVertexUploadCallCount;
		stats.mUploadedVertexCount += vertexCount;
		uploadColorRangeDirect(0, vertexCount);
		++stats.mVertexUploadCallCount;
		stats.mUploadedVertexCount += vertexCount;
		uploadUV0RangeDirect(0, vertexCount);
		++stats.mVertexUploadCallCount;
		stats.mUploadedVertexCount += vertexCount;
		if (mTMPVertexLayoutEnabled)
		{
			uploadUV2RangeDirect(0, vertexCount);

			++stats.mVertexUploadCallCount;
		}
	}
	private GraphicsBuffer getDirectGPUVertexBuffer(int stream)
	{
		GraphicsBuffer buffer = getCachedDirectGPUVertexBuffer(stream);
		if (buffer != null && buffer.IsValid())
		{
			return buffer;
		}
		disposeDirectGPUVertexBuffer(stream);
		if (mMesh == null || stream < 0 || stream >= mMesh.vertexBufferCount)
		{
			return null;
		}
		buffer = mMesh.GetVertexBuffer(stream);
		setCachedDirectGPUVertexBuffer(stream, buffer);

		return buffer;
	}
	private GraphicsBuffer getCachedDirectGPUVertexBuffer(int stream)
	{
		if (stream == FastUIVertex.POSITION_STREAM)
		{
			return mPositionGPUBuffer;
		}
		if (stream == FastUIVertex.COLOR_STREAM)
		{
			return mColorGPUBuffer;
		}
		if (stream == FastUIVertex.UV_STREAM)
		{
			return mUV0GPUBuffer;
		}
		if (stream == FastUIVertex.TMP_UV2_STREAM)
		{
			return mUV2GPUBuffer;
		}
		return null;
	}
	private void setCachedDirectGPUVertexBuffer(int stream, GraphicsBuffer buffer)
	{
		if (stream == FastUIVertex.POSITION_STREAM)
		{
			mPositionGPUBuffer = buffer;
		}
		else if (stream == FastUIVertex.COLOR_STREAM)
		{
			mColorGPUBuffer = buffer;
		}
		else if (stream == FastUIVertex.UV_STREAM)
		{
			mUV0GPUBuffer = buffer;
		}
		else if (stream == FastUIVertex.TMP_UV2_STREAM)
		{
			mUV2GPUBuffer = buffer;
		}
	}
	private void disposeDirectGPUVertexBuffer(int stream)
	{
		GraphicsBuffer buffer = getCachedDirectGPUVertexBuffer(stream);
		buffer?.Dispose();
		setCachedDirectGPUVertexBuffer(stream, null);
	}
	private void releaseDirectGPUVertexBuffers()
	{
		disposeDirectGPUVertexBuffer(FastUIVertex.POSITION_STREAM);
		disposeDirectGPUVertexBuffer(FastUIVertex.COLOR_STREAM);
		disposeDirectGPUVertexBuffer(FastUIVertex.UV_STREAM);
		disposeDirectGPUVertexBuffer(FastUIVertex.TMP_UV2_STREAM);
		if (mVertexUploadBackend == FastUIVertexUploadBackend.DirectGPUBuffer)
		{
			mDirectGPUVertexSeedRequired = true;
		}
	}
	private void releaseDirectGPUBuffers()
	{
		releaseDirectGPUVertexBuffers();
	}
	// Submit层使用独立CPU Index staging。逻辑Index仍保留原布局，GPU只接收去掉大空洞后的Submit数据。
	public void uploadExternalIndexRange(Int_ECSList sourceIndices, int sourceStart, int destinationStart, int indexCount, ref FastUIFrameStats stats)
	{
		if (indexCount <= 0 || sourceIndices == null || sourceStart < 0 || sourceStart + indexCount > sourceIndices.Count)
		{
			return;
		}
		bool use16 = mGPUIndexFormatInitialized && mGPUIndexFormat == IndexFormat.UInt16;
		if (use16 && !fillIndex16Staging(sourceIndices, sourceStart, indexCount))
		{
			upgradeGPUIndexFormatToUInt32(ref stats);
			use16 = false;
		}
		var indexColumn = sourceIndices.getValueColumn();
		NativeArray<int> view32 = default;
		if (!use16)
		{
			ref int first = ref indexColumn[sourceStart];
			view32 = FastUINativeArrayBridge.createView(ref first, indexCount);
		}
		if (use16)
		{
			mMesh.SetIndexBufferData(
				mIndex16Staging,
				0,
				destinationStart,
				indexCount,
				MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontNotifyMeshUsers
			);
		}
		else
		{
			mMesh.SetIndexBufferData(
				view32,
				0,
				destinationStart,
				indexCount,
				MeshUpdateFlags.DontRecalculateBounds | MeshUpdateFlags.DontValidateIndices | MeshUpdateFlags.DontNotifyMeshUsers
			);
		}
	}
	private bool fillIndex16Staging(Int_ECSList sourceIndices, int sourceStart, int indexCount)
	{
		if (sourceIndices == null || sourceStart < 0 || indexCount <= 0 || sourceStart + indexCount > sourceIndices.Count)
		{
			return false;
		}
		if (mIndex16Staging == null || mIndex16Staging.Length < indexCount)
		{
			mIndex16Staging = new ushort[Mathf.NextPowerOfTwo(Mathf.Max(indexCount, 64))];
		}
		var indexColumn = sourceIndices.getValueColumn();
		for (int i = 0; i < indexCount; ++i)
		{
			int value = indexColumn[sourceStart + i];
			if ((uint)value >= ushort.MaxValue)
			{
				return false;
			}
			mIndex16Staging[i] = (ushort)value;
		}
		return true;
	}
	private IndexFormat chooseGPUIndexFormat()
	{
		// 16位索引限制的是最大顶点地址，不是Index数量。VertexSpan<=65535时最大索引<=65534，避开部分图形API把0xFFFF作为特殊索引值的兼容风险。
		if (mGPUIndexFormatInitialized && mGPUIndexFormat == IndexFormat.UInt32)
		{
			return IndexFormat.UInt32;
		}
		return mVertexSpan <= ushort.MaxValue ? IndexFormat.UInt16 : IndexFormat.UInt32;
	}
	private void upgradeGPUIndexFormatToUInt32(ref FastUIFrameStats stats)
	{
		if (mGPUIndexFormatInitialized && mGPUIndexFormat == IndexFormat.UInt32)
		{
			return;
		}
		mGPUIndexFormat = IndexFormat.UInt32;
		mGPUIndexFormatInitialized = true;
		if (mMesh != null && mGPUIndexCapacity > 0)
		{
			mMesh.SetIndexBufferParams(mGPUIndexCapacity, IndexFormat.UInt32);
			stats.mIndexBufferResized = true;
		}
	}

	private void forceFullVertexStreams(int reason)
	{
		mForceFullPositionUpload = true;
		mForceFullColorUpload = true;
		mForceFullUVUpload = true;
		mForceFullPositionReason = mergeFullUploadReason(mForceFullPositionReason, reason);
		mForceFullColorReason = mergeFullUploadReason(mForceFullColorReason, reason);
		mForceFullUVReason = mergeFullUploadReason(mForceFullUVReason, reason);
	}
	private int mergeFullUploadReason(int currentReason, int newReason)
	{
		if (currentReason == FULL_UPLOAD_REASON_BUFFER_RESIZE || newReason == FULL_UPLOAD_REASON_BUFFER_RESIZE)
		{
			return FULL_UPLOAD_REASON_BUFFER_RESIZE;
		}
		return newReason != FULL_UPLOAD_REASON_NONE ? newReason : currentReason;
	}
	// 合并重叠、相邻或小Gap Dirty Range；允许少量冗余顶点换取更少的上传区间与Native边界切换。
	// 先得到严格有序区间，再按“额外上传字节 vs 省一次Upload调用”做成本型Gap合并。
	// RangeLimit使用固定区间阈值；AdaptiveCost综合上传调用成本与传输顶点数。
	private int buildMergedRanges(FastUIRangeData_ECSList source, int bytesPerVertex, int uploadCallsPerRange, int uploadCallPenaltyBytes, out int mergedVertexCount)
	{
		mMergedRanges.Clear();
		mergedVertexCount = 0;
		int count = source.Count;

		if (count == 0)
		{
			return 0;
		}
		var sourceStarts = source.getStartColumn();
		var sourceEnds = source.getEndColumn();
		bool ordered = true;
		int previousStart = sourceStarts[0];
		for (int i = 0; i < count; ++i)
		{
			int currentStart = sourceStarts[i];
			if (i > 0 && currentStart < previousStart)
			{
				ordered = false;
			}
			previousStart = currentStart;
			mMergedRanges.Add(new FastUIRangeData(currentStart, sourceEnds[i]));
		}
		if (count > 1 && !ordered)
		{
			mMergedRanges.SortFast();
		}
		var starts = mMergedRanges.getStartColumn();
		var ends = mMergedRanges.getEndColumn();
		int writeIndex = 0;
		int safeBytesPerVertex = Mathf.Max(bytesPerVertex, 1);
		int safeCallsPerRange = Mathf.Max(uploadCallsPerRange, 1);
		long savedCallCostBytes = (long)Mathf.Max(uploadCallPenaltyBytes, 0) * safeCallsPerRange;
		for (int readIndex = 1; readIndex < count; ++readIndex)
		{
			int previousEnd = ends[writeIndex];
			int currentStart = starts[readIndex];
			int currentEnd = ends[readIndex];
			int gap = currentStart - previousEnd;
			bool merge = gap <= 0;
			if (!merge && gap <= mDirtyRangeMergeGap)
			{
				merge = true;
			}
			if (!merge && mDirtyUploadMode == FastUIDirtyUploadMode.AdaptiveCost)
			{
				long extraUploadBytes = (long)gap * safeBytesPerVertex;
				merge = extraUploadBytes <= savedCallCostBytes;
			}
			if (merge)
			{
				if (currentEnd > previousEnd)
				{
					ends[writeIndex] = currentEnd;
				}
				continue;
			}
			++writeIndex;
			starts[writeIndex] = currentStart;
			ends[writeIndex] = currentEnd;
		}
		int mergedCount = writeIndex + 1;
		for (int i = 0; i < mergedCount; ++i)
		{
			mergedVertexCount += ends[i] - starts[i];
		}
		if (mMergedRanges.Count > mergedCount)
		{
			mMergedRanges.RemoveRange(mergedCount, mMergedRanges.Count - mergedCount);
		}

		return mergedCount;
	}
	private FastUIDirtyUploadChoice chooseDirtyUploadChoice(bool forceFull, int mergedCount, int mergedVertexCount, int bytesPerVertex, int uploadCallsPerRange, int uploadCallPenaltyBytes)
	{
		if (forceFull)
		{
			return FastUIDirtyUploadChoice.Full;
		}
		if (mergedCount <= 0)
		{
			return FastUIDirtyUploadChoice.None;
		}
		if (mDirtyUploadMode == FastUIDirtyUploadMode.RangeLimit)
		{
			return mergedCount > FastUIMeshUtility.MAX_PARTIAL_UPLOAD_RANGE ? FastUIDirtyUploadChoice.Full : FastUIDirtyUploadChoice.Partial;
		}
		if (mergedCount == 1)
		{
			return FastUIDirtyUploadChoice.Partial;
		}
		var starts = mMergedRanges.getStartColumn();
		var ends = mMergedRanges.getEndColumn();
		int envelopeStart = starts[0];
		int envelopeEnd = ends[mergedCount - 1];
		int envelopeVertexCount = Mathf.Max(envelopeEnd - envelopeStart, 0);
		int safeBytesPerVertex = Mathf.Max(bytesPerVertex, 1);
		int safeCallsPerRange = Mathf.Max(uploadCallsPerRange, 1);
		long callPenalty = Mathf.Max(uploadCallPenaltyBytes, 0);
		long partialCost = (long)mergedVertexCount * safeBytesPerVertex + (long)mergedCount * safeCallsPerRange * callPenalty;
		long envelopeCost = (long)envelopeVertexCount * safeBytesPerVertex + safeCallsPerRange * callPenalty;
		long fullCost = (long)mVertexSpan * safeBytesPerVertex + safeCallsPerRange * callPenalty;
		if (partialCost <= envelopeCost && partialCost <= fullCost)
		{
			return FastUIDirtyUploadChoice.Partial;
		}
		if (envelopeCost < fullCost || envelopeStart > 0 || envelopeEnd < mVertexSpan)
		{
			return FastUIDirtyUploadChoice.Envelope;
		}
		return FastUIDirtyUploadChoice.Full;
	}
	private void getMergedEnvelope(int mergedCount, out int startVertex, out int vertexCount)
	{
		startVertex = 0;
		vertexCount = 0;
		if (mergedCount <= 0)
		{
			return;
		}
		var starts = mMergedRanges.getStartColumn();
		var ends = mMergedRanges.getEndColumn();
		startVertex = starts[0];
		vertexCount = Mathf.Max(ends[mergedCount - 1] - startVertex, 0);
	}
	private void addSlotVertexRange(FastUIRangeData_ECSList list, int slot)
	{
		int vertexStart = getGeometryVertexStart(slot);
		int vertexCount = getGeometryVertexCount(slot);
		if (vertexStart < 0 || vertexCount <= 0)
		{
			return;
		}
		list.Add(new FastUIRangeData(vertexStart, vertexStart + vertexCount));
	}

	private Color32 multiplyColor(Color32 baseColor, Color32 vertexColor)
	{
		return new Color32(
			(byte)((baseColor.r * vertexColor.r + 127) / 255),
			(byte)((baseColor.g * vertexColor.g + 127) / 255),
			(byte)((baseColor.b * vertexColor.b + 127) / 255),
			(byte)((baseColor.a * vertexColor.a + 127) / 255)
		);
	}
	// CPU Stream首次创建时使用2次幂作为构造容量。
	// 后续只补齐有效Count；如果Count超过当前Capacity，由EasyECS Add内部按自身规则自动resize。
	private void ensureCPUVertexCapacity(int vertexCount)
	{
		int positionCapacity = mPositionECS != null ? mPositionECS.Capacity : 0;
		int colorCapacity = mColorECS != null ? mColorECS.Capacity : 0;
		int uvCapacity = mUVECS != null ? mUVECS.Capacity : 0;
		bool baseReady = positionCapacity >= vertexCount && colorCapacity >= vertexCount && uvCapacity >= vertexCount;
		bool tmpReady = !mTMPVertexLayoutEnabled || (mTMPUV0ECS != null && mTMPUV2ECS != null &&
			mTMPUV0ECS.Capacity >= vertexCount && mTMPUV2ECS.Capacity >= vertexCount);
		if (baseReady && tmpReady)
		{
			return;
		}
		int capacity = Mathf.NextPowerOfTwo(Mathf.Max(vertexCount, 4));
		if (mPositionECS == null)
		{
			mPositionECS = new FastUIPositionData_ECSList(capacity);
			mPositionDataCount = 0;
		}
		if (mColorECS == null)
		{
			mColorECS = new FastUIColorData_ECSList(capacity);
			mColorDataCount = 0;
		}
		if (mUVECS == null)
		{
			mUVECS = new FastUIUVData_ECSList(capacity);
			mUVDataCount = 0;
		}
		mPositionECS.EnsureCapacity(capacity);
		mColorECS.EnsureCapacity(capacity);
		mUVECS.EnsureCapacity(capacity);
		while (mPositionDataCount < capacity)
		{
			mPositionECS.Add(new FastUIPositionData
			{
				mPosition = Vector3.zero
			});
			++mPositionDataCount;
		}
		while (mColorDataCount < capacity)
		{
			mColorECS.Add(new FastUIColorData
			{
				mColor = default
			});
			++mColorDataCount;
		}
		while (mUVDataCount < capacity)
		{
			mUVECS.Add(new FastUIUVData
			{
				mUV = default
			});
			++mUVDataCount;
		}
		if (mTMPVertexLayoutEnabled)
		{
			ensureTMPVertexCapacity(capacity);
		}
	}
	private void ensureTMPVertexCapacity(int vertexCount)
	{
		if (vertexCount <= 0)
		{
			return;
		}
		int capacity = Mathf.NextPowerOfTwo(Mathf.Max(vertexCount, 4));
		if (mTMPUV0ECS == null)
		{
			mTMPUV0ECS = new FastUITMPUV0Data_ECSList(capacity);
			mTMPUV0DataCount = 0;
		}
		if (mTMPUV2ECS == null)
		{
			mTMPUV2ECS = new FastUITMPUV2Data_ECSList(capacity);
			mTMPUV2DataCount = 0;
		}
		mTMPUV0ECS.EnsureCapacity(capacity);
		mTMPUV2ECS.EnsureCapacity(capacity);
		while (mTMPUV0DataCount < capacity)
		{
			mTMPUV0ECS.Add(new FastUITMPUV0Data
			{
				mUV = default
			});
			++mTMPUV0DataCount;
		}
		while (mTMPUV2DataCount < capacity)
		{
			mTMPUV2ECS.Add(new FastUITMPUV2Data
			{
				mUV = default
			});
			++mTMPUV2DataCount;
		}
	}
	private void copyBaseUVToTMPData()
	{
		if (mUVECS == null || mTMPUV0ECS == null || mTMPUV2ECS == null)
		{
			return;
		}
		int count = Mathf.Min(mUVDataCount, mTMPUV0DataCount);
		var uvColumn = mUVECS.getUVColumn();
		var tmpUV0Column = mTMPUV0ECS.getUVColumn();
		var tmpUV2Column = mTMPUV2ECS.getUVColumn();
		for (int i = 0; i < count; ++i)
		{
			Vector2 uv = uvColumn[i];
			tmpUV0Column[i] = new Vector4(uv.x, uv.y, 0.0f, 1.0f);
			tmpUV2Column[i] = default;
		}
	}
	private void ensureGeometrySlotRecord(int slot)
	{
		while (mGeometryRanges.Count <= slot)
		{
			mGeometryRanges.Add(new FastUIGeometryRangeData
			{
				mVertexStart = -1,
				mVertexCount = 0,
				mVertexCapacity = 0,
			});
		}
	}
	private void ensureSimpleTextSlotState(int slot)
	{
		if (slot < 0)
		{
			return;
		}
		mSimpleTextActiveVertexCounts.EnsureCount(slot + 1);
		mSimpleTextClearEndVertexCounts.EnsureCount(slot + 1);
	}
	private void resetSimpleTextSlotState(int slot)
	{
		if (slot < 0 || slot >= mSimpleTextActiveVertexCounts.Count)
		{
			return;
		}
		mSimpleTextActiveVertexCounts[slot] = -1;
		mSimpleTextClearEndVertexCounts[slot] = 0;
	}
	// Stable Glyph Slots：Simple Text的提交拓扑固定为Glyph Capacity，而不是当前GlyphCount。
	// 只要文本长度没有超过Persistent Glyph Capacity，文字变化就只改Vertex/UV，不再改Index。
	private FastUIGeometryUpdateResult ensureSimpleTextGeometryRange(int slot, int requiredVertexCount, int requestedVertexCapacity)
	{
		ensureGeometrySlotRecord(slot);
		bool hadSimpleTextState = slot >= 0 && slot < mSimpleTextActiveVertexCounts.Count;
		int oldActiveVertexCount = hadSimpleTextState ? mSimpleTextActiveVertexCounts[slot] : -1;
		ensureSimpleTextSlotState(slot);
		var vertexStarts = mGeometryRanges.getVertexStartColumn();
		var vertexCounts = mGeometryRanges.getVertexCountColumn();
		var vertexCapacities = mGeometryRanges.getVertexCapacityColumn();
		int oldStart = vertexStarts[slot];
		int oldCount = vertexCounts[slot];
		int oldCapacity = vertexCapacities[slot];
		bool wasPreallocated = oldActiveVertexCount == PREALLOCATED_SIMPLE_TEXT_ACTIVE_VERTEX_COUNT;
		bool wasSimpleText = oldActiveVertexCount >= 0;
		int minimumCapacity = Mathf.Max(Mathf.Max(requiredVertexCount, requestedVertexCapacity), 4);
		if (oldStart >= 0 && minimumCapacity <= oldCapacity)
		{
			int submittedVertexCount = oldCapacity;
			vertexCounts[slot] = submittedVertexCount;
			mLiveVertexCount += submittedVertexCount - oldCount;
			bool indexCountChanged = oldCount != submittedVertexCount;
			int clearEnd = !wasSimpleText || indexCountChanged ? submittedVertexCount : Mathf.Max(requiredVertexCount, oldActiveVertexCount);
			mSimpleTextClearEndVertexCounts[slot] = Mathf.Min(clearEnd, submittedVertexCount);
			// 预分配Range对当前Text来说仍是首次Range/Capacity建立，必须保持旧路径的Index/写流语义。
			return new FastUIGeometryUpdateResult(oldStart, submittedVertexCount, wasPreallocated, indexCountChanged, wasPreallocated);
		}
		int newCapacity = Mathf.NextPowerOfTwo(minimumCapacity);
		int newStart = allocateVertexRange(newCapacity);
		ensureCPUVertexCapacity(newStart + newCapacity);
		if (oldStart >= 0 && oldCapacity > 0)
		{
			releaseVertexRange(oldStart, oldCapacity);
		}
		vertexStarts[slot] = newStart;
		vertexCounts[slot] = newCapacity;
		vertexCapacities[slot] = newCapacity;
		mLiveVertexCount += newCapacity - oldCount;
		mSimpleTextClearEndVertexCounts[slot] = newCapacity;
		return new FastUIGeometryUpdateResult(newStart, newCapacity, oldStart != newStart, oldCount != newCapacity, oldCapacity != newCapacity);
	}
	private void finishSimpleTextGeometryWrite(int slot, int activeVertexCount, int vertexStart)
	{
		ensureSimpleTextSlotState(slot);
		int submittedVertexCount = getGeometryVertexCount(slot);
		int safeActiveVertexCount = Mathf.Clamp(activeVertexCount, 0, submittedVertexCount);
		int clearEnd = Mathf.Clamp(mSimpleTextClearEndVertexCounts[slot], safeActiveVertexCount, submittedVertexCount);
		if (vertexStart >= 0 && clearEnd > safeActiveVertexCount && mPositionECS != null)
		{
			var positionColumn = mPositionECS.getPositionColumn();
			for (int vertexIndex = safeActiveVertexCount; vertexIndex < clearEnd; ++vertexIndex)
			{
				positionColumn[vertexStart + vertexIndex] = Vector3.zero;
			}
		}
		mSimpleTextActiveVertexCounts[slot] = safeActiveVertexCount;
		mSimpleTextClearEndVertexCounts[slot] = 0;
	}
	private FastUIGeometryUpdateResult ensureGeometryRange(int slot, int requiredVertexCount)
	{
		ensureGeometrySlotRecord(slot);
		resetSimpleTextSlotState(slot);
		var vertexStarts = mGeometryRanges.getVertexStartColumn();
		var vertexCounts = mGeometryRanges.getVertexCountColumn();
		var vertexCapacities = mGeometryRanges.getVertexCapacityColumn();
		int oldStart = vertexStarts[slot];
		int oldCount = vertexCounts[slot];
		int oldCapacity = vertexCapacities[slot];
		if (requiredVertexCount <= oldCapacity)
		{
			vertexCounts[slot] = requiredVertexCount;
			mLiveVertexCount += requiredVertexCount - oldCount;
			return new FastUIGeometryUpdateResult(oldStart, requiredVertexCount, false, oldCount != requiredVertexCount, false);
		}
		int newCapacity = Mathf.NextPowerOfTwo(Mathf.Max(requiredVertexCount, 4));
		int newStart = allocateVertexRange(newCapacity);
		ensureCPUVertexCapacity(newStart + newCapacity);
		if (oldStart >= 0 && oldCapacity > 0)
		{
			releaseVertexRange(oldStart, oldCapacity);
		}
		vertexStarts[slot] = newStart;
		vertexCounts[slot] = requiredVertexCount;
		vertexCapacities[slot] = newCapacity;
		mLiveVertexCount += requiredVertexCount - oldCount;
		return new FastUIGeometryUpdateResult(
			newStart,
			requiredVertexCount,
			oldStart != newStart,
			oldCount != requiredVertexCount,
			oldCapacity != newCapacity
		);
	}
	private int allocateVertexRange(int capacity)
	{
		int bestIndex = -1;
		int bestSize = int.MaxValue;
		var freeStarts = mFreeVertexRanges.getStartColumn();
		var freeEnds = mFreeVertexRanges.getEndColumn();
		for (int i = 0; i < mFreeVertexRanges.Count; ++i)
		{
			int size = freeEnds[i] - freeStarts[i];
			if (size >= capacity && size < bestSize)
			{
				bestIndex = i;
				bestSize = size;
			}
		}
		if (bestIndex >= 0)
		{
			int start = freeStarts[bestIndex];
			if (bestSize == capacity)
			{
				mFreeVertexRanges.RemoveAt(bestIndex);
			}
			else
			{
				freeStarts[bestIndex] = start + capacity;
			}
			return start;
		}
		int appendedStart = mVertexSpan;
		mVertexSpan += capacity;
		return appendedStart;
	}
	private void releaseGeometryRange(int slot)
	{
		if (slot < 0 || slot >= mGeometryRanges.Count)
		{
			return;
		}
		resetSimpleTextSlotState(slot);
		var vertexStarts = mGeometryRanges.getVertexStartColumn();
		var vertexCounts = mGeometryRanges.getVertexCountColumn();
		var vertexCapacities = mGeometryRanges.getVertexCapacityColumn();
		int vertexStart = vertexStarts[slot];
		int vertexCount = vertexCounts[slot];
		int vertexCapacity = vertexCapacities[slot];
		if (vertexStart >= 0 && vertexCapacity > 0)
		{
			releaseVertexRange(vertexStart, vertexCapacity);
		}
		mLiveVertexCount = Mathf.Max(0, mLiveVertexCount - vertexCount);
		vertexStarts[slot] = -1;
		vertexCounts[slot] = 0;
		vertexCapacities[slot] = 0;
	}
	private void releaseVertexRange(int start, int capacity)
	{
		if (start < 0 || capacity <= 0)
		{
			return;
		}
		mFreeVertexRanges.Add(new FastUIRangeData(start, start + capacity));
		mFreeVertexRanges.SortFast();
		var starts = mFreeVertexRanges.getStartColumn();
		var ends = mFreeVertexRanges.getEndColumn();
		int write = 0;
		for (int read = 1; read < mFreeVertexRanges.Count; ++read)
		{
			if (starts[read] <= ends[write])
			{
				ends[write] = Mathf.Max(ends[write], ends[read]);
				continue;
			}
			++write;
			starts[write] = starts[read];
			ends[write] = ends[read];
		}
		int mergedCount = write + 1;
		if (mFreeVertexRanges.Count > mergedCount)
		{
			mFreeVertexRanges.RemoveRange(mergedCount, mFreeVertexRanges.Count - mergedCount);
		}
		while (mFreeVertexRanges.Count > 0)
		{
			int lastIndex = mFreeVertexRanges.Count - 1;
			var tailStarts = mFreeVertexRanges.getStartColumn();
			var tailEnds = mFreeVertexRanges.getEndColumn();
			if (tailEnds[lastIndex] != mVertexSpan)
			{
				break;
			}
			mVertexSpan = tailStarts[lastIndex];
			mFreeVertexRanges.RemoveAt(lastIndex);
		}
	}
	public bool tryGetRawPosition(int vertexIndex, out Vector3 position)
	{
		position = Vector3.zero;
		if (mPositionECS == null || vertexIndex < 0 || vertexIndex >= mPositionDataCount)
		{
			return false;
		}
		position = mPositionECS.getPositionColumn()[vertexIndex];
		return true;
	}
	public bool tryGetRawIndex(int indexOffset, out int vertexIndex)
	{
		vertexIndex = -1;
		if (mIndexECS == null || indexOffset < 0 || indexOffset >= mCurrentIndexCount || indexOffset >= mIndexDataCount)
		{
			return false;
		}
		vertexIndex = mIndexECS.getValueColumn()[indexOffset];
		return true;
	}
	public int getIndexDataCount()
	{
		return mIndexDataCount;
	}
	public bool tryGetPositionStreamBounds(out Vector3 min, out Vector3 max, out int nonFiniteCount, out int nonZeroZCount, out int extremeCount)
	{
		min = Vector3.zero;
		max = Vector3.zero;
		nonFiniteCount = 0;
		nonZeroZCount = 0;
		extremeCount = 0;
		if (mPositionECS == null || mPositionDataCount <= 0)
		{
			return false;
		}
		var positions = mPositionECS.getPositionColumn();
		bool hasFinite = false;
		for (int i = 0; i < mPositionDataCount; ++i)
		{
			Vector3 p = positions[i];
			bool finite = !float.IsNaN(p.x) && !float.IsInfinity(p.x) && !float.IsNaN(p.y) && !float.IsInfinity(p.y) && !float.IsNaN(p.z) && !float.IsInfinity(p.z);
			if (!finite)
			{
				++nonFiniteCount;
				continue;
			}
			if (!hasFinite)
			{
				min = p;
				max = p;
				hasFinite = true;
			}
			else
			{
				min = Vector3.Min(min, p);
				max = Vector3.Max(max, p);
			}
			if (Mathf.Abs(p.z) > 0.0001f)
			{
				++nonZeroZCount;
			}
			if (Mathf.Abs(p.x) > 1000000.0f || Mathf.Abs(p.y) > 1000000.0f || Mathf.Abs(p.z) > 1000000.0f)
			{
				++extremeCount;
			}
		}
		return hasFinite;
	}
	public bool tryGetGeometryBounds(int slot, out FastUISpatialBoundsData bounds)
	{
		return tryGetGeometryBounds(slot, 0, out bounds);
	}
	public bool tryGetGeometryBounds(int slot, int vertexCountLimit, out FastUISpatialBoundsData bounds)
	{
		bounds = default;
		int vertexStart = getGeometryVertexStart(slot);
		int vertexCount = getGeometryVertexCount(slot);
		if (vertexCountLimit > 0)
		{
			vertexCount = Mathf.Min(vertexCount, vertexCountLimit);
		}
		if (vertexStart < 0 || vertexCount <= 0 || mPositionECS == null)
		{
			return false;
		}
		var positions = mPositionECS.getPositionColumn();
		Vector3 first = positions[vertexStart];
		float minX = first.x;
		float minY = first.y;
		float maxX = first.x;
		float maxY = first.y;
		float minZ = first.z;
		float maxZ = first.z;
		for (int i = 1; i < vertexCount; ++i)
		{
			Vector3 p = positions[vertexStart + i];
			minX = Mathf.Min(minX, p.x);
			minY = Mathf.Min(minY, p.y);
			maxX = Mathf.Max(maxX, p.x);
			maxY = Mathf.Max(maxY, p.y);
			minZ = Mathf.Min(minZ, p.z);
			maxZ = Mathf.Max(maxZ, p.z);
		}
		// Dynamic Spatial目前只对Canvas平面内的2D几何排序。
		// 如果一个自定义几何跨越明显Z厚度，继续沿用旧逻辑并把它视为不可参与Spatial排序。
		if (maxZ - minZ > 0.0001f)
		{
			return false;
		}
		bounds = new FastUISpatialBoundsData(minX, minY, maxX, maxY, (minZ + maxZ) * 0.5f);
		return true;
	}

	public void ensureCPUIndexCapacity(int indexCount)
	{
		int capacity = Mathf.NextPowerOfTwo(Mathf.Max(indexCount, 6));
		if (mIndexECS == null)
		{
			mIndexECS = new Int_ECSList(capacity);
			mIndexDataCount = 0;
		}
		else if (mIndexECS.Capacity >= capacity)
		{
			return;
		}
		mIndexECS.EnsureCount(capacity);
		mIndexDataCount = capacity;
	}
	private int calculateGPUIndexSubmitCapacity(int requiredIndexCount)
	{
		// Submit层已经在每个Batch内部保留少量局部余量，这里不再叠加6.25%全局空洞，只做256 Index对齐。
		long required = Mathf.Max(requiredIndexCount, 6);
		long aligned = (required + 255L) & ~255L;
		return aligned >= int.MaxValue ? int.MaxValue : (int)aligned;
	}
	public void ensureGPUIndexSubmitCapacity(int requiredIndexCount, bool allowShrink, ref FastUIFrameStats stats)
	{
		IndexFormat desiredFormat = chooseGPUIndexFormat();
		bool formatChanged = !mGPUIndexFormatInitialized || desiredFormat != mGPUIndexFormat;
		int desiredCapacity = calculateGPUIndexSubmitCapacity(requiredIndexCount);
		bool growRequired = requiredIndexCount > mGPUIndexCapacity;
		bool shrinkRequired = allowShrink && mGPUIndexCapacity > 0 && desiredCapacity < mGPUIndexCapacity;
		if (!growRequired && !shrinkRequired && !formatChanged)
		{
			return;
		}
		int capacity = growRequired || shrinkRequired ? desiredCapacity : mGPUIndexCapacity;
		if (capacity <= 0)
		{
			capacity = desiredCapacity;
		}
		mGPUIndexCapacity = capacity;
		mGPUIndexFormat = desiredFormat;
		mGPUIndexFormatInitialized = true;
		mMesh.SetIndexBufferParams(mGPUIndexCapacity, mGPUIndexFormat);
		stats.mIndexBufferResized = true;
	}
}
