using System;
using EasyECS;
using UnityEngine;
using UnityEngine.Rendering;

// FastUI运行时使用的基础ECS数据声明。
// ECS结构只包含字段和必要构造逻辑，不添加实例Property，避免EasyECS Source Generator报错。

// EasyECS已内置Int_ECSList/Bool_ECSList，FastGUI不再声明单字段int/bool包装结构。
// EasyECS生成代码依赖字段布局，结构中不添加实例Property或额外包装访问。
[ECS]
public struct FastUIRangeData
{
	public int mStart;
	public int mEnd;
	public FastUIRangeData(int start, int end)
	{
		mStart = start;
		mEnd = end;
	}
}
// 多个Transform Root在同一业务循环中批量平移时使用的纯数据中间结构。
// notify阶段只解析一次Transform Range并把Canvas空间Delta写入ECS；endTransformPositionBatch统一连续消费。
// 相邻Range且Delta相同会在入队阶段直接合并，避免N个Root重复进入Position FastPath。
[ECS]
public struct FastUITransformPositionBatchData
{
	public int mRenderStart;
	public int mRenderCount;
	public Vector3 mCanvasDelta;
	public FastUITransformPositionBatchData(int renderStart, int renderCount, Vector3 canvasDelta)
	{
		mRenderStart = renderStart;
		mRenderCount = renderCount;
		mCanvasDelta = canvasDelta;
	}
}
// 以下四个结构不是为了绕过 EasyECS 基础类型限制。
// EasyECS的Color32/Vector2/Vector4 BuiltIn会拆成SoA标量列，而Mesh.SetVertexBufferData需要
// 连续打包的 Color32/Vector2/Vector4 Stream。这里保留单字段结构，是为了维持现有零拷贝 GPU 上传路径，
// 避免每次 Dirty Upload 额外做 SoA -> Packed Array 重组。
[ECS]
public struct FastUIColorData
{
	public Color32 mColor;
}
[ECS]
public struct FastUIUVData
{
	public Vector2 mUV;
}
// TMP生成材质使用的TEXCOORD0。xy是字形Atlas UV，w保存SDF Scale。
[ECS]
public struct FastUITMPUV0Data
{
	public Vector4 mUV;
}
// TMP生成材质使用的TEXCOORD1。FastText当前写入Character Mapping坐标。
[ECS]
public struct FastUITMPUV2Data
{
	public Vector2 mUV;
}
// FastText最常见的“单行、主字体、单Atlas、无富文本”路径使用的中间结果。
// 第一阶段仍由TMP完成字符查找与排版，但只把后续几何阶段真正需要的纯数值缓存到EasyECS。
// 第二阶段直接读取这些连续Column写入最终Vertex ECS，不再重新访问Glyph对象，也不经过GeometryBuilder二次拷贝。
[ECS]
public struct FastUITextSimpleGlyphData
{
	public float mPenX;
	public float mLeftOffset;
	public float mTopOffset;
	public float mRightOffset;
	public float mBottomOffset;
	public float mUVLeft;
	public float mUVBottom;
	public float mUVRight;
	public float mUVTop;
	public float mSDFScale;
}
// Canvas级Text Work。
// 复杂的字体/Glyph查询仍在FastText语义阶段完成；Canvas只保留后续连续顶点计算真正需要的纯数据。
[ECS]
public struct FastUITextBatchWorkData
{
	public int mSlot;
	public int mVertexStart;
	public int mGlyphStart;
	public int mGlyphCount;
	// Text局部坐标z恒为0，只缓存Matrix4x4中真正参与二维UI顶点变换的9个值。
	// x' = x*M00 + y*M01 + M03
	// y' = x*M10 + y*M11 + M13
	// z' = x*M20 + y*M21 + M23
	public float mM00;
	public float mM01;
	public float mM03;
	public float mM10;
	public float mM11;
	public float mM13;
	public float mM20;
	public float mM21;
	public float mM23;
	public Color32 mColor;
	public int mWriteUV;
	public int mWriteColor;
}
// 每个稳定VertexSlot映射到一段可变长度的顶点区域。
// mVertexCapacity允许同一元素在小范围拓扑变化时复用原区域，避免搬动其他元素。
[ECS]
public struct FastUIGeometryRangeData
{
	public int mVertexStart;
	public int mVertexCount;
	public int mVertexCapacity;
}
// EasyECS生成代码依赖字段布局，结构中不添加实例Property或额外包装访问。
[ECS]
public struct FastUISubMeshDescriptorData
{
	public SubMeshDescriptor mValue;
}
// 每个Logical RenderIndex对应的运行时BatchKey快照。
// Material/Texture仍然是Unity对象句柄，但热点扫描只读取EasyECS连续列，不再回到FastRawImage对象取状态。
[ECS]
public struct FastUIBatchElementData
{
	public Material mMaterial;
	public Texture mTexture;
	public int mMember;
	// Index Patch热循环不再回FastUIRenderElement做Active/Visible/Cull三个virtual状态查询。
	// bit0=RenderActive, bit1=Visible, bit2=Culled。
	public int mRenderState;
}
// 稳定VertexSlot对应的纯数据热状态。
// managed/Unity对象引用不进入该ECS；这里只保存跨帧反复扫描和计算的数据，使后续Direct/Burst路径都能连续访问。
// DeltaTransform缓存把Local PositionDelta转换到Canvas空间所需的3x3向量矩阵提前同步到ECS，Dirty消费不再反复查询RectTransform.parent/localToWorldMatrix。
[ECS]
public struct FastUIVertexSlotRuntimeData
{
	public int mDirtyState;
	public Vector3 mPendingLocalPositionDelta;
	public Color32 mColor;
	public Rect mUVRect;
	public int mDeltaTransformMode;
	public Vector3 mDeltaRow0;
	public Vector3 mDeltaRow1;
	public Vector3 mDeltaRow2;
	public int mRequiresGeometryRebuildForColor;
	public int mGeometryControlsVertexColor;
	// 本帧Canvas Text Batch已经为此Slot完成几何准备时记录结果，主Dirty循环直接消费，不再重复进入FastText。
	public int mTextBatchResultFlags;
	// 每个稳定VertexSlot持有一段跨帧复用的Simple Text Glyph Range。
	// Slot释放后保留Capacity，后续复用同一Slot时可直接复用这段缓存；只有需要更大容量时才追加新块。
	public int mPersistentTextGlyphStart;
	public int mPersistentTextGlyphCount;
	public int mPersistentTextGlyphCapacity;
}
// EasyECS生成代码依赖字段布局，结构中不添加实例Property或额外包装访问。
[ECS]
public struct FastUIBatchRunData
{
	public int mRenderStart;
	public Material mMaterial;
	public Texture mTexture;
	public FastUIBatchRunData(int renderStart, Material material, Texture texture)
	{
		mRenderStart = renderStart;
		mMaterial = material;
		mTexture = texture;
	}
}
// EasyECS生成代码依赖字段布局，结构中不添加实例Property或额外包装访问。
[ECS]
public struct FastUIDrawRunData
{
	public int mRenderStart;
	public int mRenderEnd;
	public Material mMaterial;
	public Texture mTexture;
	public FastUIDrawRunData(int renderStart, int renderEnd, Material material, Texture texture)
	{
		mRenderStart = renderStart;
		mRenderEnd = renderEnd;
		mMaterial = material;
		mTexture = texture;
	}
}
// EasyECS生成代码依赖字段布局，结构中不添加实例Property或额外包装访问。
[ECS]
public struct FastUISpatialBoundsData
{
	public float mMinX;
	public float mMinY;
	public float mMaxX;
	public float mMaxY;
	public float mCenterZ;
	public FastUISpatialBoundsData(float minX, float minY, float maxX, float maxY, float centerZ)
	{
		mMinX = minX;
		mMinY = minY;
		mMaxX = maxX;
		mMaxY = maxY;
		mCenterZ = centerZ;
	}
}

// 顶点Position Stream使用的ECS数据。
// 四个顶点位置按元素连续存储，供Direct Column批量更新和上传。
// FastUI Position Stream的唯一CPU数据源。
// EasyECS BuiltIn Vector3 会拆成 x/y/z 三个 float SoA Column。
// FastGUI Position Stream 需要把连续 Vector3 直接零拷贝交给 Mesh.SetVertexBufferData，因此这里故意保留
// 单字段 Packed Vector3 ECS 结构；它不是为了绕过基础类型容器能力，而是 GPU Stream 的存储格式适配层。
[ECS]
public struct FastUIPositionData
{
	public Vector3 mPosition;
}

// 叶节点PositionDelta PackedQueue使用的ECS数据。
// 记录VertexSlot和本帧位移增量，用于大批量纯平移节点的快速应用。
// 叶节点PositionDelta Packed Queue使用的连续增量数据。
// 只保存最终消费阶段需要的数据:VertexSlot + Canvas空间Delta.
// 纯unmanaged结构,用于EasyECS Unsafe SoA连续访问.
// EasyECS生成代码依赖字段布局，结构中不添加实例Property或额外包装访问。
[ECS]
public struct FastUIPositionDeltaData
{
	public int mVertexSlot;
	public Vector3 mDelta;
}

// FastSpriteRenderer transform-sort hot state.
// Matrix4x4 itself stays in NativeArray because geometry jobs consume it as one cohesive value.
// Only the scalar fields that are independently streamed by the transform scan live in EasyECS SoA.
[ECS]
public struct FastSpriteTransformSortData
{
	public float mSortPointX;
	public float mSortPointY;
	public float mSortPointZ;
	public float mSortValue;
	public byte mTransformChanged;
	public byte mRuntimeActive;

	public FastSpriteTransformSortData(float sortPointX, float sortPointY, float sortPointZ, bool runtimeActive)
	{
		mSortPointX = sortPointX;
		mSortPointY = sortPointY;
		mSortPointZ = sortPointZ;
		mSortValue = 0.0f;
		mTransformChanged = 0;
		mRuntimeActive = runtimeActive ? (byte)1 : (byte)0;
	}
}

// Slot-indexed hot instance state consumed by the GPU-driven backend.
[ECS]
public struct FastSpriteGPUInstanceHotData
{
	public Matrix4x4 mLocalMatrix;
	public Vector4 mColor;
	public int mGeometryOffset;
	public int mRootIndex;
	public int mFlags;
}

// Stable plan-slot metadata; managed render resources stay outside ECS columns.
[ECS]
public struct FastSpriteGPUPlanSlotData
{
	[NotECS] public Material mMaterial;
	[NotECS] public Texture mTexture;
	[NotECS] public int mMaterialID;
	[NotECS] public int mTextureID;
	[NotECS] public int mVertexCount;
	public int mActive;
	public int mRenderStateID;
}

// Compact draw-command stream generated from ordered plan slots.
[ECS]
public struct FastSpriteGPUDrawCommandData
{
	[NotECS] public Material mMaterial;
	[NotECS] public Texture mTexture;
	[NotECS] public int mMaterialID;
	[NotECS] public int mTextureID;
	[NotECS] public int mVertexCount;
	public int mRenderStateID;
	public int mOrderStart;
	public int mInstanceCount;
	public int mSortingLayerID;
	public int mFirstSortingOrder;
	public int mLastSortingOrder;

	public FastSpriteGPUDrawCommandData(Material material, Texture texture, int materialID, int textureID,
		int vertexCount, int renderStateID, int orderStart, int instanceCount, int sortingLayerID, int firstSortingOrder, int lastSortingOrder)
	{
		mMaterial = material;
		mTexture = texture;
		mMaterialID = materialID;
		mTextureID = textureID;
		mVertexCount = vertexCount;
		mRenderStateID = renderStateID;
		mOrderStart = orderStart;
		mInstanceCount = instanceCount;
		mSortingLayerID = sortingLayerID;
		mFirstSortingOrder = firstSortingOrder;
		mLastSortingOrder = lastSortingOrder;
	}
}

// Per-chunk dirty ranges used to choose dense upload work.
[ECS]
public struct FastSpriteDirtyChunkStats
{
	public int mMatrixMin;
	public int mMatrixMax;
	public int mMatrixCount;
	public int mColorMin;
	public int mColorMax;
	public int mColorCount;
	public int mGeometryMin;
	public int mGeometryMax;
	public int mGeometryCount;
	public int mRootMin;
	public int mRootMax;
	public int mRootCount;
	public int mFlagsMin;
	public int mFlagsMax;
	public int mFlagsCount;
}

// Scratch SoA for grouped sorting-order transfers.
[ECS]
internal struct FastSpriteSortingOrderTransferGroupData
{
	[NotECS] public FastSpriteBatch mSourceBatch;
	public int mTargetSortingOrder;
	public int mFirstItemIndex;
	public int mLastItemIndex;
	public int mItemCount;
}

[ECS]
internal struct FastSpriteSortingOrderTransferItemData
{
	[NotECS] public FastSpriteRenderer mRenderer;
	public int mNextItemIndex;
}

// Read-only sort key; only the index permutation is moved during depth sorting.
[ECS]
internal struct FastSpriteDepthSortKeyData
{
	public int mOrder;
	public long mStableID;
}

// FastCanvas层级节点使用的ECS数据。
// 保存Transform关系和RenderRange索引等热状态。
// FastCanvas按需建立的Transform->RenderRange索引,只依赖真实Unity Transform层级。
// ParentIndex按Hierarchy DFS顺序稳定保存，Clip/Visibility等结构系统可直接复用，不再重复向上解析Transform父链。
// EasyECS生成代码依赖字段布局，结构中不添加实例Property或额外包装访问。
[ECS]
public struct FastUITransformNode
{
	public Transform mTransform;
	public int mParentIndex;
	public int mRenderStart;
	public int mRenderCount;
	public FastUITransformNode(Transform transform, int parentIndex, int renderStart, int renderCount)
	{
		mTransform = transform;
		mParentIndex = parentIndex;
		mRenderStart = renderStart;
		mRenderCount = renderCount;
	}
}