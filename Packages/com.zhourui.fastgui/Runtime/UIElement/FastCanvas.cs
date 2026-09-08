using EasyECS;
using System;
using System.Collections.Generic;
using Unity.Profiling;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

// FastUI的顶层Canvas调度器。
// 负责接收元素Dirty通知、维护Logical RenderOrder，并按固定阶段驱动Visibility、SOA DrawOrder、Batch和Mesh上传。
// 具体子职责通过独立System组合持有，Canvas本身保留一帧更新的核心编排。
[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
[DefaultExecutionOrder(32000)]
public class FastCanvas : MonoBehaviour
{
	protected sealed class BulkPositionSlotPlan
	{
		public readonly FastUIRangeData_ECSList mSlotRanges = new(64);
		public readonly FastUIRangeData_ECSList mComplementSlotRanges = new(16);
		public int mRenderStart;
		public int mRenderCount;
		public int mVertexCount;
		public int mComplementVertexCount;
		public void reset(int renderStart, int renderCount)
		{
			mSlotRanges.Clear();
			mComplementSlotRanges.Clear();
			mRenderStart = renderStart;
			mRenderCount = renderCount;
			mVertexCount = 0;
			mComplementVertexCount = 0;
		}
		public void dispose()
		{
			mSlotRanges.Dispose();
			mComplementSlotRanges.Dispose();
		}
	}
	// Canvas主循环Profiler。各子阶段继续使用独立Marker，便于区分Dirty消费、Position FastPath和Range重建。
	protected static readonly ProfilerMarker LATE_UPDATE_MARKER = new("FastUI.Canvas.LateUpdate");
	protected static readonly ProfilerMarker BUILD_VERTICES_MARKER = new("FastUI.Canvas.BuildDirtyVertexStreams");
	protected static readonly ProfilerMarker POSITION_BATCH_MARKER = new("FastUI.Canvas.TransformPositionBatch");
	protected static readonly ProfilerMarker REBUILD_TRANSFORM_RANGE_MARKER = new("FastUI.Canvas.RebuildTransformRanges");
	protected static readonly ProfilerMarker CLIP_STRUCTURE_MARKER = new("FastUI.Canvas.ClipStructure");
	protected static readonly ProfilerMarker DEFERRED_STRUCTURE_FLUSH_MARKER = new("FastUI.Canvas.DeferredStructureFlush");
	protected static readonly double TICK_TO_MS = 1000.0 / System.Diagnostics.Stopwatch.Frequency;
	protected const int BULK_POSITION_SLOT_CACHE_MIN_RENDER_COUNT = 16;
	protected const int VERTEX_DIRTY_FLAGS_MASK = 0xFF;
	protected const int VERTEX_DIRTY_QUEUED_BIT = 1 << 8;
	protected const int TEXT_DIRTY_QUEUED_BIT = 1 << 9;
	// 与Dirty Flags完全分离，只标记该Slot当前这次Dirty来自首次注册。消费完成后随DirtyState一起清零。
	protected const int INITIAL_DIRTY_BIT = 1 << 10;
	protected const int DIRTY_GEOMETRY_PATH_SIMPLE_TEXT = 1;
	protected const int DIRTY_GEOMETRY_PATH_SIMPLE_QUAD = 2;
	protected const int DIRTY_GEOMETRY_PATH_GENERIC = 3;
	protected const int TEXT_BATCH_PREPARED_BIT = 1 << 0;
	protected const int TEXT_BATCH_INDEX_COUNT_CHANGED_BIT = 1 << 1;
	protected const int TEXT_BATCH_VERTEX_RANGE_CHANGED_BIT = 1 << 2;
	protected const int TEXT_BATCH_INDEX_CAPACITY_CHANGED_BIT = 1 << 3;
	protected const int TEXT_BATCH_INDEX_CHANGE_MASK = TEXT_BATCH_INDEX_COUNT_CHANGED_BIT | TEXT_BATCH_VERTEX_RANGE_CHANGED_BIT | TEXT_BATCH_INDEX_CAPACITY_CHANGED_BIT;
	protected const int DELTA_TRANSFORM_UNINITIALIZED = -1;
	protected const int DELTA_TRANSFORM_DIRECT = 0;
	protected const int DELTA_TRANSFORM_MATRIX = 1;
	// 可序列化的运行参数。这里的阈值只决定路径选择，不改变FastUI的数据结构语义。
	[SerializeField] protected Material mDefaultMaterial;
	[SerializeField] protected string mSortingLayerName = "Default";
	[SerializeField] protected int mSortingOrder;
	[SerializeField] protected int mVisibilityIndexThreshold = 16;
	[SerializeField] protected int mPositionDeltaPackedQueueMinCount = 768;
	[SerializeField] protected int mSimpleTextCanvasBatchMinCount = 16;
	[SerializeField] protected int mInitialSimpleQuadBatchMinCount = 64;
	[SerializeField] protected int mRuntimeSimpleQuadBatchMinCount = 64;
	protected int mGeometryMatrixCacheVersion = 1;
	// Simple Text Layout中的SDF Scale只依赖Text/Canvas相对缩放。
	// Position/Scroll不会改变该值，因此使用独立Scale版本，避免沿用GeometryMatrix全局版本导致每次平移都让数千Text缓存失效。
	protected int mTextSDFScaleCacheVersion = 1;
	protected int mTextSDFCanvasScaleCheckEpoch = int.MinValue;
	protected float mTextSDFCanvasScaleY = 1.0f;
	// 同一个Mutation Epoch内发生的多次业务Setter都位于同一次Canvas Flush之前。
	// FastText用它识别“第一次正常同步，第二次及以后只保留最终文本”，避免依赖Time.frameCount导致同帧多次flush误判。
	protected int mMutationEpoch = 1;
	[SerializeField] protected int mInitialSimpleTextRangeBatchMinCount = 64;
	// 缓存稳定结构下的RenderIndex->VertexSlot，不要求Slot全局连续。
	// Root PositionBatch热路径直接读EasyECS映射和GeometryRange Column，避免每帧重新访问Managed RenderElement。
	// 大型平移仍允许本帧暂时关闭Clip Submit裁剪，但保留累计Translation/Geometry Range。
	// 平移停止后的恢复帧只消费累计Range，不再因为延迟策略强制整Canvas Full Bounds Scan。
	// 仅Clip Rect/Padding变化时复用已缓存Element Bounds重判Outside，避免再次读取整批Mesh Geometry Bounds。
	// Initial Batch/Partition只使用首次注册标记与预计算Candidate Metadata，不依赖逐Dirty计时。
	// 大型子树纯平移时，如果受影响顶点远多于补集顶点，直接移动内部RenderRoot并只反向补偿少量未移动节点。
	// 业务仍然只修改正常RectTransform；该优化完全由Canvas根据TransformRange自动选择。
	// 默认关闭是为了保持任意自定义Shader的Mesh Object-Space坐标语义；确认材质不依赖该语义后可按Canvas开启。
	[SerializeField] protected bool mRenderOriginShiftEnabled = false;
	[SerializeField] protected int mRenderOriginShiftMinSavedVertexCount = 4096;
	// 高频整窗口显隐优先使用setVisible(bool)，避免GameObject.SetActive触发整棵UI的MonoBehaviour生命周期回调。
	[SerializeField] protected bool mRenderVisible = true;
	// RenderOrder主序列。删除元素使用Tombstone保持Slot稳定，避免大范围移动后续元素。
	protected readonly FastUIRenderRegistry mRenderRegistry = new();
	protected readonly List<FastUIRenderElement> mDeferredHierarchyElements = new(256);
	protected Int_ECSList mDirtyVertexSlots;
	// 只记录本帧真正发生Geometry/Transform Dirty的FastText Slot。
	// Text Batch直接消费这个ECS，避免再次扫描全部Dirty Element。
	protected Int_ECSList mDirtyTextVertexSlots;
	// 首次AllVertex中的Simple Image/RawImage与剩余Dirty一次分区。Candidate按稳定VertexSlot预计算，Partition只读ECS列。
	protected Int_ECSList mInitialSimpleQuadBatchSlots;
	protected Int_ECSList mInitialSimpleQuadRemainingDirtySlots;
	protected Int_ECSList mInitialSimpleQuadCandidateECS;
	// 已存在稳定4顶点Range的Simple Image/RawImage在运行时Geometry/Transform批量Dirty时直接写Position SOA。
	// 只消费Position部分；同帧Color/UV仍留给原Dirty循环处理，避免改变Stream更新语义。
	protected Int_ECSList mRuntimeSimpleQuadBatchSlots;
	protected Int_ECSList mRuntimeSimpleQuadRemainingDirtySlots;
	// Initial FastText首次Stable Glyph Vertex Range一次性分配连续Arena。两个ECS按相同索引保存Slot与VertexCapacity。
	protected Int_ECSList mInitialSimpleTextRangeBatchSlots;
	protected Int_ECSList mInitialSimpleTextRangeBatchCapacities;
	// managed对象只负责语义入口和几何重建，不进入Burst/ECS纯数据热结构。
	// Dirty/Delta/Color/UV以及LocalDelta->CanvasDelta变换缓存按稳定VertexSlot驻留EasyECS。
	protected FastUIVertexSlotRuntimeData_ECSList mVertexSlotRuntimeECS;
	protected readonly FastUIGeometryBuilder mGeometryBuilder = new();
	protected FastUITextBatchWorkData_ECSList mSimpleTextBatchWorkECS;
	// 跨帧Persistent Glyph缓存。FastText Layout直接写入此ECS，Canvas Batch只引用Range。
	protected FastUITextSimpleGlyphData_ECSList mPersistentTextGlyphECS;
	protected FastUIPositionDeltaData_ECSList mLeafPositionDeltaECS;
	protected FastUITransformPositionBatchData_ECSList mTransformPositionBatchECS;
	// Position-only Root平移不再把所有Text Matrix Cache全局判失效。
	// 这里只记录少量Logical RenderRange+CanvasDelta；真正Dirty Text取矩阵时按RenderIndex懒惰叠加平移。
	// 旋转/缩放/泛化Transform Dirty仍走mGeometryMatrixCacheVersion全局失效，保持原正确性边界。
	protected FastUITransformPositionBatchData_ECSList mGeometryMatrixPositionTranslationECS;
	protected int mGeometryMatrixPositionTranslationSerial;
	protected const int GEOMETRY_MATRIX_POSITION_TRANSLATION_MAX_EVENTS = 64;
	protected int mTransformPositionBatchDepth;
	protected bool mInitialStoragePrewarmDone;
	protected int mLeafPositionDeltaStorageCount;
	protected int mPendingLeafPositionDeltaCount;
	protected FastUIRangeData_ECSList mIndexDirtyRanges;
	protected FastUIRangeData_ECSList mBatchDirtyRanges;
	// Batch通知保持Logical RenderIndex；真正提交前统一转换为当前DrawOrder区间，并与SOA Dirty Lane合并。
	protected FastUIRangeData_ECSList mBatchDrawDirtyRanges;
	protected FastUIRangeData_ECSList mMergedRenderRanges;
	protected readonly FastDictionary<Transform, BulkPositionSlotPlan> mBulkPositionSlotPlans = new();
	protected Int_ECSList mRenderVertexSlotMap;
	protected bool mRenderVertexSlotMapKnown;
	// 从Canvas中拆出的独立状态职责。每个System只拥有自己的数据，Canvas负责一帧执行顺序。
	protected FastUITransformRangeSystem mTransformRanges;
	protected FastUIVisibilitySystem mVisibility;
	protected FastUIClipSystem mClipSystem;
	protected FastUISOARenderSystem mSOARenderSystem;
	protected Transform mRenderRoot;
	// Mesh CPU顶点始终存储在RenderRoot局部空间。RenderRoot发生自动Origin Shift后，
	// 新重建的Geometry会减去该偏移，保证GPU最终位置仍然等价于Canvas空间语义。
	protected Vector3 mRenderOriginOffset;
	protected FastUIMeshRenderer mMeshRenderer;
	protected Material mRuntimeDefaultMaterial;
	protected FastUIFrameStats mLastFrameStats;
	protected int mActiveRenderElementCount;
	protected int mLiveRenderElementCount;
	protected int mTombstoneCount;
	protected int mTMPVertexLayoutElementCount;
	// CloneData批量克隆期间仍然逐元素立即注册，保证Awake/OnEnable之后VertexSlot立刻有效；
	// 但容量只预留一次、TMP布局只按真实Text计数、Structure Dirty只在整批结束时提交一次。
	protected int mCloneRegistrationBatchDepth;
	protected bool mCloneRegistrationStructureDirty;
	// CloneUtility只有在“批次开始前结构已稳定 + 新节点追加在Canvas DFS尾部 + 首个Clone验证注册顺序等于真实Hierarchy顺序”时，
	// 才允许DeferredStructure直接复用Registry。任意普通Hierarchy通知都会立即取消这个资格。
	protected bool mCloneRegistrationRegistryOrderCandidate;
	protected bool mCloneRegistrationRegistryOrderValidated;
	protected bool mDeferredRegistryOrderAuthoritative;
	// Register/Unregister/Hierarchy变化默认只记录源状态，真正的派生结构统一在帧末Flush。
	// 普通业务无需显式Begin/End；显式Scope只用于“跨多帧构建大窗口”时阻止中途Flush。
	protected int mDeferredStructureScopeDepth;
	protected bool mDeferredStructureDirty;
	protected bool mDeferredStructureTransformAllDirty;
	protected bool mFlushingDeferredStructure;
	// Clip外Text延迟Geometry在重新可见时需要在本帧Vertex Upload前补一次Dirty消费。
	protected bool mDeferredClipTextResumePending;
	// 仅Geometry真正改变Index Capacity时才允许DrawOrder逐元素比较Prefix容量。
	protected bool mIndexCapacityDirty;
	protected bool mFullDrawStructureDirty = true;
	protected bool mHierarchySuspended;
	protected bool mHierarchyResumePending;
	protected bool mInitialized;
	protected bool mECSCollectionsInitialized;
	protected bool mDestroying;
#if UNITY_EDITOR
	protected bool mDomainUnloadHookRegistered;
	protected bool mEditorGPUBufferValidationPending;
	protected bool mEditorPreviewRefreshQueued;
	protected bool mEditorPreviewRefreshing;
	protected bool mEditorSceneViewHookRegistered;
#endif
	private void Awake()
	{
		// Prefab/场景已有Hierarchy在Canvas Awake时已经存在；动态空Canvas则不要为了预热额外扫描。
		bool hasExistingHierarchy = transform.childCount > 0;
		ensureInit();
		if (hasExistingHierarchy)
		{
			prewarmExistingHierarchyRegistrationStorage();
		}
	}
	private void OnEnable()
	{
		ensureInit();
		mRenderRoot.gameObject.SetActive(true);
		mMeshRenderer.setRenderVisible(mRenderVisible);
#if UNITY_EDITOR
		mEditorGPUBufferValidationPending = true;
		if (!Application.isPlaying)
		{
			registerEditorSceneViewHook();
			queueEditorPreviewRefresh();
		}
#endif
		if (mHierarchySuspended)
		{
			mHierarchySuspended = false;
			mHierarchyResumePending = true;
		}
	}
	private void OnDisable()
	{
#if UNITY_EDITOR
		unregisterEditorSceneViewHook();
		if (mMeshRenderer != null)
		{
			mMeshRenderer.restoreDefaultCullingBounds();
		}
#endif
		if (!mDestroying && !gameObject.activeInHierarchy)
		{
			mHierarchySuspended = true;
		}
		if (mRenderRoot != null)
		{
			mRenderRoot.gameObject.SetActive(false);
		}
	}
#if UNITY_EDITOR
	private void OnValidate()
	{
		if (!Application.isPlaying)
		{
			queueEditorPreviewRefresh();
		}
	}
	private void OnTransformChildrenChanged()
	{
		if (!Application.isPlaying && !mEditorPreviewRefreshing)
		{
			queueEditorPreviewRefresh();
		}
	}
	private void registerEditorSceneViewHook()
	{
		if (Application.isPlaying || mEditorSceneViewHookRegistered)
		{
			return;
		}
		SceneView.duringSceneGui += onEditorSceneViewGUI;
		mEditorSceneViewHookRegistered = true;
	}
	private void unregisterEditorSceneViewHook()
	{
		if (!mEditorSceneViewHookRegistered)
		{
			return;
		}
		SceneView.duringSceneGui -= onEditorSceneViewGUI;
		mEditorSceneViewHookRegistered = false;
	}
	private void onEditorSceneViewGUI(SceneView sceneView)
	{
		if (Application.isPlaying || mDestroying || !isActiveAndEnabled || sceneView == null || sceneView.camera == null)
		{
			return;
		}
		ensureInit();
		if (mMeshRenderer != null)
		{
			mMeshRenderer.updateEditorPreviewCullingBounds(sceneView.camera);
		}
	}
	private void queueEditorPreviewRefresh()
	{
		if (Application.isPlaying || mDestroying || mEditorPreviewRefreshQueued)
		{
			return;
		}
		mEditorPreviewRefreshQueued = true;
		EditorApplication.delayCall += refreshEditorPreviewDelayed;
	}
	private void refreshEditorPreviewDelayed()
	{
		mEditorPreviewRefreshQueued = false;
		if (this == null || Application.isPlaying || mDestroying || !isActiveAndEnabled || mEditorPreviewRefreshing)
		{
			return;
		}
		mEditorPreviewRefreshing = true;
		try
		{
			ensureInit();
			refreshDescendantBindings();
			scheduleDeferredStructure(true);
			runFrameUpdate();
			SceneView sceneView = SceneView.lastActiveSceneView;
			if (sceneView != null && sceneView.camera != null && mMeshRenderer != null)
			{
				mMeshRenderer.updateEditorPreviewCullingBounds(sceneView.camera);
			}
			SceneView.RepaintAll();
		}
		finally
		{
			mEditorPreviewRefreshing = false;
		}
	}
#endif
	// 每帧只做流程编排：消费Dirty -> 同步Buffer容量 -> Visibility -> DrawStructure -> 上传/统计收尾。
	// 子系统内部不反向驱动Canvas，避免职责之间形成隐式循环。
	private void LateUpdate()
	{
		runFrameUpdate();
	}
	// 立即执行与LateUpdate完全相同的完整FastCanvas流水线。
	// 主要用于Prefab加载、同步创建窗口、编辑器工具等需要“现在就得到最终可渲染状态”的场景。
	// 普通业务仍然什么都不用调用，默认由本帧LateUpdate统一Flush。
	public FastUIFrameStats flushFrameNow()
	{
		ensureInit();
		runFrameUpdate();
		return mLastFrameStats;
	}
	private void runFrameUpdate()
	{
		// Unity对象销毁、Editor DomainUnload，或销毁回调中的重入Flush都可能发生在ECS容器释放之后。
		// 此时不能再进入正常Frame流水线，也不能为了Flush重新创建内部Renderer/ECS资源。
		if (mDestroying || !mInitialized || !mECSCollectionsInitialized || mMeshRenderer == null || mDirtyVertexSlots == null)
		{
			return;
		}
		using (LATE_UPDATE_MARKER.Auto())
		{
			long startTick = System.Diagnostics.Stopwatch.GetTimestamp();
			unchecked
			{
				if (++mMutationEpoch == 0)
				{
					++mMutationEpoch;
				}
			}
			// CloneUtility批量Instantiate期间如果业务主动flushFrameNow，先提交当前已创建Graphic，保证本次Flush看到完整Registry状态。
			FastUICloneUtility.flushPendingRegistration(this);
			FastUIFrameStats stats = createFrameStats();
			if (!mRenderVisible)
			{
				stats.mBatchCount = mMeshRenderer.getBatchCount();
				stats.mCPUTimeMS = (System.Diagnostics.Stopwatch.GetTimestamp() - startTick) * TICK_TO_MS;
				mLastFrameStats = stats;
				return;
			}
			flushDeferredStructure();
			syncHierarchyResume();
			refreshFrameClipStructure();
			prewarmInitialStorage();
			processDirtyElements(ref stats);
			bool clipWindowChanged = refreshFrameClipCull();
			if (mDeferredClipTextResumePending)
			{
				mDeferredClipTextResumePending = false;
				processDirtyElements(ref stats);
			}
#if UNITY_EDITOR
			if (mEditorGPUBufferValidationPending)
			{
				mEditorGPUBufferValidationPending = false;
				if (mMeshRenderer.validateEditorGPUBufferState())
				{
					mFullDrawStructureDirty = true;
				}
			}
#endif
			mMeshRenderer.syncVertexBufferCapacity(ref stats);
			bool visibilityDrawChanged = refreshFrameVisibility(ref stats);
			refreshFrameDrawStructure(visibilityDrawChanged, clipWindowChanged, ref stats);
			finalizeFrameStats(startTick, ref stats);
		}
	}
	// 首帧只按RenderElement*4预热基础顶点容量，不触发布局或几何构建。
	private void prewarmInitialStorage()
	{
		if (mInitialStoragePrewarmDone || mMeshRenderer == null || mRenderRegistry == null || mRenderRegistry.Count <= 0)
		{
			return;
		}
		mInitialStoragePrewarmDone = true;
		int renderCount = mRenderRegistry.Count;
		long baseVertexEstimate = renderCount * 4L;
		long totalVertexEstimate = Math.Min(baseVertexEstimate, 1L << 30);
		int minimumVertexCount = (int)Math.Max(totalVertexEstimate, 4L);
		mMeshRenderer.prewarmInitialCPUVertexStorage(minimumVertexCount);
		if (mPersistentTextGlyphECS != null && mDirtyTextVertexSlots != null && mDirtyTextVertexSlots.Count > 0)
		{
			int textCount = mDirtyTextVertexSlots.Count;
			int minimumGlyphCapacity = (int)Math.Min(textCount * 4L, 1L << 30);
			if (mPersistentTextGlyphECS.Capacity < minimumGlyphCapacity)
			{
				mPersistentTextGlyphECS.EnsureCapacity(minimumGlyphCapacity);
			}
			if (mSimpleTextBatchWorkECS != null && mSimpleTextBatchWorkECS.Capacity < textCount)
			{
				mSimpleTextBatchWorkECS.EnsureCapacity(textCount);
			}
		}
	}
	public void setDefaultMaterial(Material material)
	{
		ensureECSCollections();
		if (mDefaultMaterial == material)
		{
			return;
		}
		mDefaultMaterial = material;
		// DefaultMaterial是全Canvas派生BatchKey。Mutation阶段只改源状态，帧末Full DrawStructure统一读取最终值。
		mFullDrawStructureDirty = true;
	}
	public Material getDefaultMaterial()
	{
		ensureInit();
		return mDefaultMaterial != null ? mDefaultMaterial : mRuntimeDefaultMaterial;
	}
	// 默认Register本身已经是Deferred。这个Scope只在跨多帧创建大型窗口时使用，
	// 防止每个创建帧都提前Flush一次完整Hierarchy。
	public void beginDeferredStructureScope(int expectedAdditionalElements = 0)
	{
		ensureInit();
		if (mDeferredStructureScopeDepth++ > 0)
		{
			return;
		}
		if (expectedAdditionalElements <= 0)
		{
			return;
		}
		List<FastUIRenderElement> elements = mRenderRegistry.getElements();
		int required = elements.Count + expectedAdditionalElements;
		if (elements.Capacity < required)
		{
			elements.Capacity = Mathf.NextPowerOfTwo(Mathf.Max(required, 4));
		}
	}
	public void endDeferredStructureScope()
	{
		if (mDeferredStructureScopeDepth <= 0)
		{
			return;
		}
		--mDeferredStructureScopeDepth;
	}
	// 仅提供给确实需要“本帧中途立即读取最终RenderOrder/Range”的特殊工具。
	// 正常业务不要调用；FastCanvas会在LateUpdate统一Flush。
	public void flushDeferredChangesNow()
	{
		if (mDeferredStructureScopeDepth > 0 || !mDeferredStructureDirty)
		{
			return;
		}
		flushDeferredStructure();
	}
	public bool hasDeferredStructureChanges()
	{
		return mDeferredStructureDirty;
	}
	// 已有Prefab/场景Hierarchy可以在任何Graphic注册前知道大致最终规模。这里只Reserve Capacity，不增加Count、不注册元素，
	// 因此不会把创建工作隐藏到别的阶段，也不会改变动态AddComponent路径。
	private void prewarmExistingHierarchyRegistrationStorage()
	{
		// Unity把一个Transform根及全部后代保存在同一Hierarchy数据结构中，hierarchyCount可直接给出当前规模。
		// 复用首帧DeferredStructure本来就会使用的List，并一次性预留容量，避免Awake额外new临时List和多次List扩容。
		if (transform.root == transform)
		{
			int hierarchyCount = transform.hierarchyCount;
			if (mDeferredHierarchyElements.Capacity < hierarchyCount)
			{
				mDeferredHierarchyElements.Capacity = hierarchyCount;
			}
		}
		mDeferredHierarchyElements.Clear();
		GetComponentsInChildren(true, mDeferredHierarchyElements);
		int graphicCount = mDeferredHierarchyElements.Count;
		if (graphicCount <= 0)
		{
			mDeferredHierarchyElements.Clear();
			return;
		}
		int textCount = 0;
		for (int i = 0; i < graphicCount; ++i)
		{
			if (mDeferredHierarchyElements[i] is FastText)
			{
				++textCount;
			}
		}
		mRenderRegistry.ensureCapacity(graphicCount);
		mVertexSlotRuntimeECS.EnsureCapacity(graphicCount);
		mDirtyVertexSlots.EnsureCapacity(graphicCount);
		mMeshRenderer.prewarmInitialVertexSlotStorage(graphicCount);
		if (textCount > 0)
		{
			mDirtyTextVertexSlots.EnsureCapacity(textCount);
			mSimpleTextBatchWorkECS.EnsureCapacity(textCount);
		}
		// 不让Reserve阶段长期持有额外组件引用；Capacity保留给后续DeferredStructure首次收集直接复用。
		mDeferredHierarchyElements.Clear();
	}
	private void scheduleDeferredStructure(bool transformAllDirty, bool preserveCloneRegistryOrder = false)
	{
		mDeferredStructureDirty = true;
		mDeferredStructureTransformAllDirty |= transformAllDirty;
		mFullDrawStructureDirty = true;
		markTransformRangesDirty();
		if (!preserveCloneRegistryOrder)
		{
			mCloneRegistrationRegistryOrderCandidate = false;
			mDeferredRegistryOrderAuthoritative = false;
		}
	}
	private bool shouldSkipDerivedMutationForDeferredStructure()
	{
		return mDeferredStructureDirty && !mFlushingDeferredStructure;
	}
	private void flushDeferredStructure()
	{
		if (!mDeferredStructureDirty || mDeferredStructureScopeDepth > 0 || mFlushingDeferredStructure)
		{
			return;
		}
		using (DEFERRED_STRUCTURE_FLUSH_MARKER.Auto())
		{
			mFlushingDeferredStructure = true;
			try
			{
				List<FastUIRenderElement> elements = mRenderRegistry.getElements();
				bool reuseRegistryOrder = mDeferredRegistryOrderAuthoritative;
				mDeferredRegistryOrderAuthoritative = false;
				if (!reuseRegistryOrder)
				{
					mDeferredHierarchyElements.Clear();
					GetComponentsInChildren(true, mDeferredHierarchyElements);
					elements.Clear();
					for (int i = 0; i < mDeferredHierarchyElements.Count; ++i)
					{
						FastUIRenderElement element = mDeferredHierarchyElements[i];
						if (element != null && element.getCanvas() == this && element.getVertexSlot() >= 0)
						{
							elements.Add(element);
						}
					}
				}
				mTombstoneCount = 0;
				mLiveRenderElementCount = elements.Count;
				int count = elements.Count;
				if (!reuseRegistryOrder)
				{
					updateRenderOrderIndices(0, count);
					for (int i = 0; i < count; ++i)
					{
						FastUIRenderElement element = elements[i];
						if (element != null)
						{
							element.setCanvasRenderRange(i, 1);
						}
					}
				}
				int activeCount = 0;
				if (mDeferredStructureTransformAllDirty)
				{
					Matrix4x4 canvasWorldToLocal = transform.worldToLocalMatrix;
					for (int i = 0; i < count; ++i)
					{
						FastUIRenderElement element = elements[i];
						if (element == null)
						{
							continue;
						}
						element.refreshTransformCacheFromHierarchyChange();
						int slot = element.getVertexSlot();
						if (slot >= 0)
						{
							syncVertexSlotRuntimeMetadata(slot, element);
							syncVertexSlotDeltaTransform(slot, element, canvasWorldToLocal);
						}
						if (element.isRenderActive())
						{
							++activeCount;
						}
					}
				}
				else
				{
					for (int i = 0; i < count; ++i)
					{
						FastUIRenderElement element = elements[i];
						if (element != null && element.isRenderActive())
						{
							++activeCount;
						}
					}
				}
				mActiveRenderElementCount = activeCount;
				mIndexDirtyRanges.Clear();
				mBatchDirtyRanges.Clear();
				mBatchDrawDirtyRanges.Clear();
				mDeferredStructureDirty = false;

				markTransformRangesDirty();
				ensureTransformRanges();
				if (mDeferredStructureTransformAllDirty)
				{
					for (int i = 0; i < count; ++i)
					{
						markElementDirty(elements[i], FastUIDirtyFlags.Transform);
					}
				}
				mDeferredStructureTransformAllDirty = false;

				mVisibility?.markStructureChanged();
				mClipSystem?.markStructureDirty();
				mFullDrawStructureDirty = true;
				mIndexCapacityDirty = false;
			}
			finally
			{
				mFlushingDeferredStructure = false;
			}
		}
	}
	// 整个FastCanvas的高性能显隐。只控制FastGUI渲染提交，不改变GameObject.activeSelf。
	// 隐藏期间Dirty继续累积；恢复显示后在同一LateUpdate阶段统一消费。
	public bool getVisible()
	{
		return mRenderVisible;
	}
	public void setVisible(bool visible)
	{
		ensureInit();
		if (mRenderVisible == visible)
		{
			return;
		}
		mRenderVisible = visible;
		mMeshRenderer.setRenderVisible(visible && isActiveAndEnabled);
	}
	public void refreshRendererSetting()
	{
		ensureInit();
		mRenderRoot.gameObject.layer = gameObject.layer;
		mMeshRenderer.refreshRendererSetting(mSortingLayerName, mSortingOrder, gameObject.layer);
	}
	public void setSortingLayer(string layerName)
	{
		ensureInit();
		if (mSortingLayerName == layerName)
		{
			return;
		}
		mSortingLayerName = layerName;
		refreshRendererSetting();
	}
	public void setSortingOrder(int sortingOrder)
	{
		ensureInit();
		if (mSortingOrder == sortingOrder)
		{
			return;
		}
		mSortingOrder = sortingOrder;
		refreshRendererSetting();
	}
	public string getSortingLayer()
	{
		return mSortingLayerName;
	}
	public int getSortingOrder()
	{
		return mSortingOrder;
	}
	public int getRenderElementCount()
	{
		return mLiveRenderElementCount;
	}
	public int getTextElementCount()
	{
		return mTMPVertexLayoutElementCount;
	}
	public int getRenderOrderSlotCount()
	{
		return mRenderRegistry.Count;
	}
	public int getTombstoneCount()
	{
		return mTombstoneCount;
	}
	public string getTransformNodeBackendName()
	{
		return FastUITransformNode_ECSList.BackendName;
	}
	public string getTransformNodeBackendReason()
	{
		return FastUITransformNode_ECSList.BackendReason;
	}
	public bool isTransformNodeUnsafeBackend()
	{
		return FastUITransformNode_ECSList.IsUnsafeBackend;
	}
	public int getBatchCount()
	{
		return mMeshRenderer != null ? mMeshRenderer.getBatchCount() : 0;
	}
	public int getLogicalBatchCount()
	{
		return mMeshRenderer != null ? mMeshRenderer.getLogicalBatchCount() : 0;
	}
	public FastUIFrameStats getLastFrameStats()
	{
		return mLastFrameStats;
	}
	public int getVisibilityIndexThreshold()
	{
		return mVisibilityIndexThreshold;
	}
	public FastUIMeshRenderer getMeshRenderer()
	{
		ensureInit();
		return mMeshRenderer;
	}
	public bool getCompactIndexMode()
	{
		return true;
	}
	public int getLimitedClipExtraDrawRunCount()
	{
		ensureInit();
		return mMeshRenderer.getLimitedClipExtraDrawRunCount();
	}
	public int getLimitedClipSavedIndexCount()
	{
		ensureInit();
		return mMeshRenderer.getLimitedClipSavedIndexCount();
	}
	// 强制恢复Canvas中所有TMP文字的最终GPU输入状态。
	// 该接口只用于低频结构切换后的状态恢复，不进入正常帧热点。
	public void forceTMPTextRenderRefresh()
	{
		ensureInit();
		if (mTMPVertexLayoutElementCount > 0)
		{
			mMeshRenderer.setTMPVertexLayoutEnabled(true);
		}
		mMeshRenderer.forceBatchTextureRebind();
		for (int i = 0; i < mRenderRegistry.Count; ++i)
		{
			FastUIRenderElement element = mRenderRegistry[i];
			if (element == null || !mRenderRegistry.requiresTMPVertexLayout(element))
			{
				continue;
			}
			markElementDirty(element, FastUIDirtyFlags.Geometry | FastUIDirtyFlags.UV);
		}
		mFullDrawStructureDirty = true;
	}
	public void setPositionDeltaPackedQueueMinCount(int count)
	{
		mPositionDeltaPackedQueueMinCount = Mathf.Max(count, 1);
	}
	public int getPositionDeltaPackedQueueMinCount()
	{
		return Mathf.Max(mPositionDeltaPackedQueueMinCount, 1);
	}
	public void setRenderOriginShiftEnabled(bool enabled)
	{
		mRenderOriginShiftEnabled = enabled;
	}
	public bool getRenderOriginShiftEnabled()
	{
		return mRenderOriginShiftEnabled;
	}
	public void setRenderOriginShiftMinSavedVertexCount(int count)
	{
		mRenderOriginShiftMinSavedVertexCount = Mathf.Max(count, 1);
	}
	public int getRenderOriginShiftMinSavedVertexCount()
	{
		return Mathf.Max(mRenderOriginShiftMinSavedVertexCount, 1);
	}
	public Vector3 getRenderOriginOffset()
	{
		return mRenderOriginOffset;
	}
	public void setRuntimeSimpleQuadBatchMinCount(int count)
	{
		mRuntimeSimpleQuadBatchMinCount = Mathf.Max(count, 1);
	}
	public int getRuntimeSimpleQuadBatchMinCount()
	{
		return Mathf.Max(mRuntimeSimpleQuadBatchMinCount, 1);
	}
	public int getGeometryMatrixCacheVersion()
	{
		return mGeometryMatrixCacheVersion;
	}
	public int getMutationEpoch()
	{
		return mMutationEpoch;
	}
	public int getTextSDFScaleCacheVersion()
	{
		refreshTextSDFCanvasScaleCache();
		return mTextSDFScaleCacheVersion;
	}
	public float getTextSDFCanvasScaleY()
	{
		refreshTextSDFCanvasScaleCache();
		return mTextSDFCanvasScaleY;
	}
	private void refreshTextSDFCanvasScaleCache()
	{
		if (mTextSDFCanvasScaleCheckEpoch == mMutationEpoch)
		{
			return;
		}
		mTextSDFCanvasScaleCheckEpoch = mMutationEpoch;
		float scaleY = transform != null ? Mathf.Abs(transform.lossyScale.y) : 1.0f;
		if (scaleY <= 0.000001f)
		{
			scaleY = 1.0f;
		}
		if (!Mathf.Approximately(mTextSDFCanvasScaleY, scaleY))
		{
			mTextSDFCanvasScaleY = scaleY;
			invalidateTextSDFScaleCacheVersion();
		}
	}
	private void invalidateTextSDFScaleCacheVersion()
	{
		unchecked
		{
			if (++mTextSDFScaleCacheVersion == 0)
			{
				++mTextSDFScaleCacheVersion;
			}
		}
	}
	private static float getMatrixYScaleSquared(Matrix4x4 matrix)
	{
		return matrix.m01 * matrix.m01 + matrix.m11 * matrix.m11 + matrix.m21 * matrix.m21;
	}
	private void invalidateTextSDFScaleCacheIfNeeded(Matrix4x4 oldLocalToWorld, Matrix4x4 newLocalToWorld)
	{
		if (Mathf.Abs(getMatrixYScaleSquared(oldLocalToWorld) - getMatrixYScaleSquared(newLocalToWorld)) > 0.000001f)
		{
			invalidateTextSDFScaleCacheVersion();
		}
	}
	private void invalidateGeometryMatrixCacheVersion()
	{
		mGeometryMatrixPositionTranslationECS?.Clear();
		mGeometryMatrixPositionTranslationSerial = 0;
		unchecked
		{
			if (++mGeometryMatrixCacheVersion == 0)
			{
				++mGeometryMatrixCacheVersion;
			}
		}
	}
	public int getGeometryMatrixPositionTranslationSerial()
	{
		return mGeometryMatrixPositionTranslationSerial;
	}
	private void queueGeometryMatrixPositionTranslation(int renderStart, int renderCount, Vector3 canvasDelta)
	{
		if (renderCount <= 0 || canvasDelta == Vector3.zero)
		{
			return;
		}
		ensureECSCollections();
		if (mGeometryMatrixPositionTranslationECS.Count >= GEOMETRY_MATRIX_POSITION_TRANSLATION_MAX_EVENTS)
		{
			// 极端情况下直接回到全局失效；下一次读取会从真实Transform重算，不保留无限事件日志。
			invalidateGeometryMatrixCacheVersion();
		}
		mGeometryMatrixPositionTranslationECS.Add(new FastUITransformPositionBatchData(renderStart, renderCount, canvasDelta));
		mGeometryMatrixPositionTranslationSerial = mGeometryMatrixPositionTranslationECS.Count;
	}
	public int applyGeometryMatrixPositionTranslations(int renderIndex, int consumedSerial, ref Matrix4x4 matrix, out bool translated)
	{
		translated = false;
		if (mGeometryMatrixPositionTranslationECS == null ||
			mGeometryMatrixPositionTranslationSerial <= 0 ||
			consumedSerial >= mGeometryMatrixPositionTranslationSerial ||
			renderIndex < 0)
		{
			return mGeometryMatrixPositionTranslationSerial;
		}
		int count = mGeometryMatrixPositionTranslationECS.Count;
		int startIndex = Mathf.Clamp(consumedSerial, 0, count);
		var renderStartColumn = mGeometryMatrixPositionTranslationECS.getRenderStartColumn();
		var renderCountColumn = mGeometryMatrixPositionTranslationECS.getRenderCountColumn();
		var deltaColumn = mGeometryMatrixPositionTranslationECS.getCanvasDeltaColumn();
		Vector3 totalDelta = Vector3.zero;
		for (int i = startIndex; i < count; ++i)
		{
			int rangeStart = renderStartColumn[i];
			int rangeCount = renderCountColumn[i];
			if (renderIndex >= rangeStart && renderIndex < rangeStart + rangeCount)
			{
				totalDelta += deltaColumn[i];
			}
		}
		if (totalDelta != Vector3.zero)
		{
			matrix.m03 += totalDelta.x;
			matrix.m13 += totalDelta.y;
			matrix.m23 += totalDelta.z;
			translated = true;
		}
		return mGeometryMatrixPositionTranslationSerial;
	}
	public void setDirtyUploadMode(FastUIDirtyUploadMode mode)
	{
		ensureInit();
		mMeshRenderer.setDirtyUploadMode(mode);
	}
	public FastUIDirtyUploadMode getDirtyUploadMode()
	{
		ensureInit();
		return mMeshRenderer.getDirtyUploadMode();
	}
	public void setUploadCallPenaltyBytes(int bytes)
	{
		ensureInit();
		mMeshRenderer.setUploadCallPenaltyBytes(bytes);
	}
	public int getUploadCallPenaltyBytes()
	{
		ensureInit();
		return mMeshRenderer.getUploadCallPenaltyBytes();
	}
	public int getSOAGroupCount()
	{
		return mSOARenderSystem != null ? mSOARenderSystem.getGroupCount() : 0;
	}
	public int getSOAConvertedGroupCount()
	{
		return mSOARenderSystem != null ? mSOARenderSystem.getConvertedGroupCount() : 0;
	}
	public int getSOARootGroupCount()
	{
		return mSOARenderSystem != null ? mSOARenderSystem.getRootGroupCount() : 0;
	}
	public int getSOANestedGroupCount()
	{
		return mSOARenderSystem != null ? mSOARenderSystem.getNestedGroupCount() : 0;
	}
	public int getSOAMaxNestedDepth()
	{
		return mSOARenderSystem != null ? mSOARenderSystem.getMaxNestedDepth() : 0;
	}
	public int getSOAInvalidGroupCount()
	{
		return mSOARenderSystem != null ? mSOARenderSystem.getInvalidGroupCount() : 0;
	}
	public int getSOAAdjacentMergeClusterCount()
	{
		return mSOARenderSystem != null ? mSOARenderSystem.getAdjacentMergeClusterCount() : 0;
	}
	public int getSOAAdjacentMergedGroupCount()
	{
		return mSOARenderSystem != null ? mSOARenderSystem.getAdjacentMergedGroupCount() : 0;
	}
	public int getSOARecordCount()
	{
		return mSOARenderSystem != null ? mSOARenderSystem.getRecordCount() : 0;
	}
	public int getSOALaneCount()
	{
		return mSOARenderSystem != null ? mSOARenderSystem.getLaneCount() : 0;
	}
	public int getSOAProjectedElementCount()
	{
		return mSOARenderSystem != null ? mSOARenderSystem.getProjectedElementCount() : 0;
	}
	public int getSOABatchKeyGroupCount()
	{
		return mSOARenderSystem != null ? mSOARenderSystem.getBatchKeyGroupCount() : 0;
	}
	public int getSOALaneIndexForRenderIndex(int renderIndex)
	{
		return mSOARenderSystem != null ? mSOARenderSystem.getLaneIndexForRenderIndex(renderIndex) : -1;
	}
	public bool isSOADrawOrderActive()
	{
		return mSOARenderSystem != null && mSOARenderSystem.isDrawOrderReordered();
	}
	public bool isRenderBoundaryRoot(Transform root)
	{
		return root != null && root == transform;
	}
	public void copyRenderSettingsFrom(FastCanvas source, int sortingOrderOffset = 0)
	{
		if (source == null || source == this)
		{
			return;
		}
		setDefaultMaterial(source.mDefaultMaterial);
		setSortingLayer(source.getSortingLayer());
		setSortingOrder(source.getSortingOrder() + sortingOrderOffset);
	}
	// Canvas层级发生变化后重新绑定后代RenderElement，保证元素仍然归属于正确的RenderBoundary。
	public void refreshDescendantBindings()
	{
		ensureInit();
		FastUIRenderElement[] elements = GetComponentsInChildren<FastUIRenderElement>(true);
#if UNITY_EDITOR
		// Editor script reload can recreate the internal renderer with an empty
		// VertexStreamSystem while descendant elements still retain stale slot ids.
		// Invalidate only this impossible state so the normal refreshCanvas() path
		// performs a complete re-registration against the new renderer.
		if (!Application.isPlaying &&
			mMeshRenderer != null &&
			mMeshRenderer.getAllocatedSlotCount() == 0)
		{
			bool hasStaleSlots = false;
			for (int i = 0; i < elements.Length; ++i)
			{
				FastUIRenderElement element = elements[i];
				if (element != null &&
					element.getCanvas() == this &&
					element.getVertexSlot() >= 0)
				{
					hasStaleSlots = true;
					break;
				}
			}

			if (hasStaleSlots)
			{
				for (int i = 0; i < elements.Length; ++i)
				{
					FastUIRenderElement element = elements[i];
					if (element == null || element.getCanvas() != this)
					{
						continue;
					}
					element.setVertexSlot(-1);
					element.setRenderOrderIndex(-1);
					element.setCanvasRenderRange(-1, 1);
				}

				mRenderRegistry.Clear();
				mLiveRenderElementCount = 0;
				mActiveRenderElementCount = 0;
				mTombstoneCount = 0;
				mTMPVertexLayoutElementCount = 0;
				mFullDrawStructureDirty = true;
				mIndexCapacityDirty = true;
			}
		}
#endif
		List<FastCanvas> previousCanvases = new();
		for (int i = 0; i < elements.Length; ++i)
		{
			FastUIRenderElement element = elements[i];
			if (element == null)
			{
				continue;
			}
			FastCanvas previous = element.getCanvas();
			if (previous != null && previous != this && !previousCanvases.Contains(previous))
			{
				previousCanvases.Add(previous);
			}
			element.refreshCanvas();
		}
		FastUIVisibility[] visibilityList = GetComponentsInChildren<FastUIVisibility>(true);
		for (int i = 0; i < visibilityList.Length; ++i)
		{
			if (visibilityList[i] != null)
			{
				visibilityList[i].refreshCanvas();
			}
		}
		FastRectMask2D[] clipList = GetComponentsInChildren<FastRectMask2D>(true);
		for (int i = 0; i < clipList.Length; ++i)
		{
			if (clipList[i] != null)
			{
				clipList[i].refreshCanvas();
			}
		}
		FastSOARenderGroup[] soaGroups = GetComponentsInChildren<FastSOARenderGroup>(true);
		for (int i = 0; i < soaGroups.Length; ++i)
		{
			if (soaGroups[i] != null)
			{
				soaGroups[i].refreshCanvas();
			}
		}
		for (int i = 0; i < previousCanvases.Count; ++i)
		{
			previousCanvases[i].compactRenderOrderTombstones();
		}
	}
	// Tombstone达到条件后才压缩RenderOrder，避免频繁删除导致后续元素整体移动。
	public void compactRenderOrderTombstones()
	{
		if (mTombstoneCount <= 0 || mRenderRegistry.Count == 0)
		{
			return;
		}
		int writeIndex = 0;
		for (int readIndex = 0; readIndex < mRenderRegistry.Count; ++readIndex)
		{
			FastUIRenderElement element = mRenderRegistry[readIndex];
			if (element == null)
			{
				continue;
			}
			if (writeIndex != readIndex)
			{
				mRenderRegistry[writeIndex] = element;
			}
			++writeIndex;
		}
		if (writeIndex < mRenderRegistry.Count)
		{
			mRenderRegistry.RemoveRange(writeIndex, mRenderRegistry.Count - writeIndex);
		}
		mTombstoneCount = 0;
		updateRenderOrderIndices(0, mRenderRegistry.Count);
		markTransformRangesDirty();
		mFullDrawStructureDirty = true;
		mIndexDirtyRanges.Clear();
		mBatchDirtyRanges.Clear();
		markVisibilityStructureChanged();
		mClipSystem?.markStructureDirty();
	}
	private void ensureVertexSlotRuntimeCapacity(int requiredCount)
	{
		if (requiredCount <= 0)
		{
			return;
		}
		mRenderRegistry.ensureVertexSlotCapacity(requiredCount);
		while (mVertexSlotRuntimeECS.Count < requiredCount)
		{
			mVertexSlotRuntimeECS.Add(default);
		}
		while (mInitialSimpleQuadCandidateECS.Count < requiredCount)
		{
			mInitialSimpleQuadCandidateECS.Add(0);
		}
	}
	private void ensurePersistentTextGlyphStorageCount(int required)
	{
		if (mPersistentTextGlyphECS == null || required <= mPersistentTextGlyphECS.Count)
		{
			return;
		}
		if (mPersistentTextGlyphECS.Capacity < required)
		{
			mPersistentTextGlyphECS.EnsureCapacity(required);
		}
		while (mPersistentTextGlyphECS.Count < required)
		{
			mPersistentTextGlyphECS.Add(default);
		}
	}
	public bool tryAcquirePersistentTextGlyphRange(FastText element, int required,
		out FastUITextSimpleGlyphData_ECSList glyphData, out int glyphStart)
	{
		glyphData = null;
		glyphStart = 0;
		if (element == null || element.getCanvas() != this || mPersistentTextGlyphECS == null)
		{
			return false;
		}
		int slot = element.getVertexSlot();
		if (slot < 0 || slot >= mVertexSlotRuntimeECS.Count)
		{
			return false;
		}
		int safeRequired = Mathf.Max(required, 1);
		var startColumn = mVertexSlotRuntimeECS.getPersistentTextGlyphStartColumn();
		var capacityColumn = mVertexSlotRuntimeECS.getPersistentTextGlyphCapacityColumn();
		int capacity = capacityColumn[slot];
		if (capacity < safeRequired)
		{
			int newCapacity = Mathf.NextPowerOfTwo(Mathf.Max(safeRequired, 4));
			glyphStart = mPersistentTextGlyphECS.Count;
			ensurePersistentTextGlyphStorageCount(glyphStart + newCapacity);
			startColumn[slot] = glyphStart;
			capacityColumn[slot] = newCapacity;
		}
		else
		{
			glyphStart = startColumn[slot];
		}
		glyphData = mPersistentTextGlyphECS;
		return true;
	}
	public void setPersistentTextGlyphCount(FastText element, int glyphCount)
	{
		if (element == null || element.getCanvas() != this)
		{
			return;
		}
		int slot = element.getVertexSlot();
		if (slot < 0 || slot >= mVertexSlotRuntimeECS.Count)
		{
			return;
		}
		mVertexSlotRuntimeECS.getPersistentTextGlyphCountColumn()[slot] = Mathf.Max(glyphCount, 0);
	}
	public int getPersistentTextGlyphCapacity(FastText element)
	{
		if (element == null || element.getCanvas() != this)
		{
			return 0;
		}
		int slot = element.getVertexSlot();
		if (slot < 0 || slot >= mVertexSlotRuntimeECS.Count)
		{
			return 0;
		}
		return Mathf.Max(mVertexSlotRuntimeECS.getPersistentTextGlyphCapacityColumn()[slot], 0);
	}
	public bool tryGetPersistentTextGlyphRange(FastText element, out FastUITextSimpleGlyphData_ECSList glyphData,
		out int glyphStart, out int glyphCount)
	{
		glyphData = null;
		glyphStart = 0;
		glyphCount = 0;
		if (element == null || element.getCanvas() != this || mPersistentTextGlyphECS == null)
		{
			return false;
		}
		int slot = element.getVertexSlot();
		if (slot < 0 || slot >= mVertexSlotRuntimeECS.Count)
		{
			return false;
		}
		int capacity = mVertexSlotRuntimeECS.getPersistentTextGlyphCapacityColumn()[slot];
		if (capacity <= 0)
		{
			return false;
		}
		glyphStart = mVertexSlotRuntimeECS.getPersistentTextGlyphStartColumn()[slot];
		glyphCount = mVertexSlotRuntimeECS.getPersistentTextGlyphCountColumn()[slot];
		glyphData = mPersistentTextGlyphECS;
		return true;
	}
	public void syncTextLayoutRuntimeMetadata(FastText element)
	{
		if (element == null || element.getCanvas() != this)
		{
			return;
		}
		int slot = element.getVertexSlot();
		if (slot < 0 || slot >= mVertexSlotRuntimeECS.Count)
		{
			return;
		}
		int richColor = element.geometryControlsVertexColor() ? 1 : 0;
		mVertexSlotRuntimeECS.getRequiresGeometryRebuildForColorColumn()[slot] = richColor;
		mVertexSlotRuntimeECS.getGeometryControlsVertexColorColumn()[slot] = richColor;
	}
	private void syncVertexSlotRuntimeMetadata(int slot, FastUIRenderElement element)
	{
		if (slot < 0 || slot >= mVertexSlotRuntimeECS.Count || element == null)
		{
			return;
		}
		mVertexSlotRuntimeECS.getRequiresGeometryRebuildForColorColumn()[slot] = element.requiresGeometryRebuildForColor() ? 1 : 0;
		mVertexSlotRuntimeECS.getGeometryControlsVertexColorColumn()[slot] = element.geometryControlsVertexColor() ? 1 : 0;
	}
	private static int getInitialSimpleQuadCandidateValue(FastUIRenderElement element, bool simpleUVCompatible)
	{
		if (!simpleUVCompatible || element == null)
		{
			return 0;
		}
		Type elementType = element.GetType();
		return elementType == typeof(FastRawImage) || elementType == typeof(FastImage) ? 1 : 0;
	}
	private void syncInitialSimpleQuadCandidateMetadata(int slot, FastUIRenderElement element)
	{
		if (element == null || mInitialSimpleQuadCandidateECS == null || mVertexSlotRuntimeECS == null)
		{
			return;
		}
		if ((uint)slot >= (uint)mInitialSimpleQuadCandidateECS.Count ||
			(uint)slot >= (uint)mVertexSlotRuntimeECS.Count)
		{
			return;
		}
		bool simpleUVCompatible = element.tryGetSimpleUVRect(out Rect uvRect);
		int candidate = getInitialSimpleQuadCandidateValue(element, simpleUVCompatible);
		mInitialSimpleQuadCandidateECS.getValueColumn()[slot] = candidate;
		// Sliced在首次Flush前切回Simple时，注册阶段的UVRect可能还是default；Candidate变True时同步最终UV语义。
		if (candidate != 0)
		{
			mVertexSlotRuntimeECS.getUVRectColumn()[slot] = uvRect;
		}
	}
	public void notifyInitialSimpleQuadCandidateChanged(FastUIRenderElement element)
	{
		if (mDestroying || element == null || element.getCanvas() != this)
		{
			return;
		}
		if (!mECSCollectionsInitialized || mInitialSimpleQuadCandidateECS == null || mVertexSlotRuntimeECS == null)
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				queueEditorPreviewRefresh();
			}
#endif
			return;
		}
		syncInitialSimpleQuadCandidateMetadata(element.getVertexSlot(), element);
	}
	private void syncVertexSlotDeltaTransform(int slot, FastUIRenderElement element, Matrix4x4 canvasWorldToLocal)
	{
		if (slot < 0 || slot >= mVertexSlotRuntimeECS.Count || element == null)
		{
			return;
		}
		Transform parent = element.getRectTransform().parent;
		var modeColumn = mVertexSlotRuntimeECS.getDeltaTransformModeColumn();
		var row0Column = mVertexSlotRuntimeECS.getDeltaRow0Column();
		var row1Column = mVertexSlotRuntimeECS.getDeltaRow1Column();
		var row2Column = mVertexSlotRuntimeECS.getDeltaRow2Column();
		if (parent == transform || (parent != null && parent.parent == transform && parent.localRotation == Quaternion.identity && parent.localScale == Vector3.one))
		{
			modeColumn[slot] = DELTA_TRANSFORM_DIRECT;
			row0Column[slot] = Vector3.right;
			row1Column[slot] = Vector3.up;
			row2Column[slot] = Vector3.forward;
			return;
		}
		Matrix4x4 parentToCanvas = parent == null ? canvasWorldToLocal : canvasWorldToLocal * parent.localToWorldMatrix;
		modeColumn[slot] = DELTA_TRANSFORM_MATRIX;
		row0Column[slot] = new Vector3(parentToCanvas.m00, parentToCanvas.m01, parentToCanvas.m02);
		row1Column[slot] = new Vector3(parentToCanvas.m10, parentToCanvas.m11, parentToCanvas.m12);
		row2Column[slot] = new Vector3(parentToCanvas.m20, parentToCanvas.m21, parentToCanvas.m22);
	}
	private void ensureVertexSlotDeltaTransform(int slot, FastUIRenderElement element, Matrix4x4 canvasWorldToLocal)
	{
		if (slot < 0 || slot >= mVertexSlotRuntimeECS.Count || element == null)
		{
			return;
		}
		if (mVertexSlotRuntimeECS.getDeltaTransformModeColumn()[slot] == DELTA_TRANSFORM_UNINITIALIZED)
		{
			syncVertexSlotDeltaTransform(slot, element, canvasWorldToLocal);
		}
	}
	private static Vector3 transformLocalDeltaFromECS(int mode, Vector3 row0, Vector3 row1, Vector3 row2, Vector3 localDelta)
	{
		if (mode == DELTA_TRANSFORM_DIRECT)
		{
			return localDelta;
		}
		return new Vector3(row0.x * localDelta.x + row0.y * localDelta.y + row0.z * localDelta.z, row1.x * localDelta.x + row1.y * localDelta.y + row1.z * localDelta.z, row2.x * localDelta.x + row2.y * localDelta.y + row2.z * localDelta.z);
	}
	public void markBatchStructureDirty()
	{
		mFullDrawStructureDirty = true;
	}
	public void notifyHierarchyChanged()
	{
		scheduleDeferredStructure(true);
	}
	private void disposeLeafPositionDeltaECS()
	{
		if (mLeafPositionDeltaECS == null)
		{
			return;
		}
		mLeafPositionDeltaECS.Dispose();
		mLeafPositionDeltaECS = null;
		mLeafPositionDeltaStorageCount = 0;
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
		// DomainUnload期间Editor仍可能有已排队的Preview/LateUpdate/Flush回调。
		// 先进入销毁态，再释放ECS，避免释放后再次进入Frame流水线。
		mDestroying = true;
		disposeLeafPositionDeltaECS();
		disposeECSCollections();
	}
#endif
	// Canvas所在层级整体SetActive(false)时不逐元素修改RenderActive。重新激活后统一扫描一次，
	// 只修正停用期间真实发生变化的节点，同时保留对停用期间直接修改RectTransform的兼容检查。
	private void syncHierarchyResume()
	{
		if (!mHierarchyResumePending)
		{
			return;
		}
		mHierarchyResumePending = false;
		int activeCount = 0;
		int scanCount = 0;
		int activeChangedCount = 0;
		int vertexChangedCount = 0;
		for (int i = 0; i < mRenderRegistry.Count; ++i)
		{
			FastUIRenderElement element = mRenderRegistry[i];
			if (element == null)
			{
				continue;
			}
			++scanCount;
			bool active = element.syncAfterCanvasHierarchyResume(out bool activeChanged, out bool vertexChanged);
			if (active)
			{
				++activeCount;
			}
			if (activeChanged)
			{
				++activeChangedCount;
				mMeshRenderer.patchBatchElementActiveState(i, active);
				addIndexDirtyRange(i, 1);
			}
			if (vertexChanged)
			{
				++vertexChangedCount;
			}
		}
		mActiveRenderElementCount = activeCount;
		mClipSystem?.syncHierarchyResume(this, out _);
	}
	// 创建本帧统计快照，同时把各子系统上一阶段的计数归零。
	private FastUIFrameStats createFrameStats()
	{
		return new FastUIFrameStats
		{
			mFrame = Time.frameCount,
			mRenderElementCount = mLiveRenderElementCount,
			mActiveRenderElementCount = mActiveRenderElementCount,
			mClippedElementCount = mClipSystem != null ? mClipSystem.getClippedElementCount() : 0,
			mSOAGroupCount = mSOARenderSystem != null ? mSOARenderSystem.getGroupCount() : 0,
		};
	}
	// SOA DrawOrder下Hierarchy HiddenRange不再对应连续DrawRange，因此统一退化为Index Hidden。
	// 这样Visibility仍按Logical RenderOrder维护，DrawOrder只负责RenderIndex->DrawIndex映射。
	private bool refreshFrameVisibility(ref FastUIFrameStats stats)
	{
		if (mVisibility == null || !mVisibility.isDirty())
		{
			return false;
		}
		if (mVisibility.resolveEmptyDirtyState(ref stats))
		{
			return false;
		}
		bool forceIndexMode = mSOARenderSystem != null && mSOARenderSystem.getGroupCount() > 0;
		bool visibilityDrawChanged = rebuildVisibilityRanges(forceIndexMode, ref stats);
		mMeshRenderer.setHiddenRenderRanges(mVisibility.getDrawHiddenRanges(), mVisibility.getIndexHiddenRanges());

		return visibilityDrawChanged;
	}
	// Full只用于Hierarchy/SOA结构变化。运行期BatchKey变化只处理Dirty Lane，不再把Lane局部重排升级为全量Index/Batch重建。
	private void refreshFrameDrawStructure(bool visibilityDrawChanged, bool clipWindowChanged, ref FastUIFrameStats stats)
	{
		Material defaultMaterial = getDefaultMaterial();
		if (defaultMaterial == null)
		{
			return;
		}
		if (mFullDrawStructureDirty)
		{
			ensureTransformRanges();
			// 首次可视Flush直接生成最终SOA DrawOrder。
			// Identity DrawOrder会先制造大量Batch，并扩大Unity SubMesh/Material既有提交槽，因此首次可视Flush直接生成最终SOA DrawOrder。
			mMeshRenderer.rebuildBatchElementStates(mRenderRegistry.getElements(), defaultMaterial);
			FastUIBatchElementData_ECSList fullBatchElementStates = mMeshRenderer.getBatchElementStates();
			mSOARenderSystem.buildDrawOrder(mRenderRegistry.getElements(), mTransformRanges, fullBatchElementStates, ref stats);
			mMeshRenderer.rebuildFullDrawStructure(mRenderRegistry.getElements(), defaultMaterial, mSOARenderSystem.getDrawOrder(), ref stats);
			mFullDrawStructureDirty = false;
			mIndexCapacityDirty = false;
			mIndexDirtyRanges.Clear();
			mBatchDirtyRanges.Clear();
			mBatchDrawDirtyRanges.Clear();
			return;
		}
		bool soaLaneDirty = mSOARenderSystem != null && mSOARenderSystem.hasDirtyLanes();
		FastUIBatchElementData_ECSList batchElementStates = mMeshRenderer.getBatchElementStates();
		bool soaOrderChanged = soaLaneDirty && mSOARenderSystem.refreshDirtyLanes(batchElementStates, ref stats);
		// Geometry容量变化必须先按旧DrawOrder修补Prefix；之后Lane只是在同一Draw区间内重排同一组元素，区间总Index容量保持不变。
		bool indexLayoutChanged = patchMergedIndexRanges(ref stats);
		if (soaOrderChanged && !mMeshRenderer.patchDrawOrderRanges(
			mRenderRegistry.getElements(),
			mSOARenderSystem.getDrawOrder(),
			mSOARenderSystem.getChangedLaneDrawRanges(),
			ref stats))
		{
			// 只有Index布局失效等异常情况才回退Full，正常BatchKey变化不再进入这里。
			mMeshRenderer.rebuildFullDrawStructure(mRenderRegistry.getElements(), defaultMaterial, mSOARenderSystem.getDrawOrder(), ref stats);
			mIndexDirtyRanges.Clear();
			mBatchDirtyRanges.Clear();
			mBatchDrawDirtyRanges.Clear();
			return;
		}
		refreshMergedBatchRanges(defaultMaterial, visibilityDrawChanged, indexLayoutChanged, soaLaneDirty, clipWindowChanged, ref stats);
	}
	private bool patchMergedIndexRanges(ref FastUIFrameStats stats)
	{
		if (mIndexDirtyRanges.Count <= 0)
		{
			return false;
		}
		if (mMeshRenderer.isDrawOrderReordered())
		{
			bool changed = mMeshRenderer.patchIndexRanges(mRenderRegistry.getElements(), mIndexDirtyRanges, ref stats);
			mIndexDirtyRanges.Clear();
			mIndexCapacityDirty = false;
			return changed;
		}
		int rangeCount = buildMergedRenderRanges(mIndexDirtyRanges);
		var rangeStarts = mMergedRenderRanges.getStartColumn();
		var rangeEnds = mMergedRenderRanges.getEndColumn();
		bool indexLayoutChanged = false;
		for (int i = 0; i < rangeCount; ++i)
		{
			indexLayoutChanged |= mMeshRenderer.patchIndexRange(mRenderRegistry.getElements(), rangeStarts[i], rangeEnds[i] - rangeStarts[i], ref stats);
		}
		mIndexDirtyRanges.Clear();
		mIndexCapacityDirty = false;
		return indexLayoutChanged;
	}
	private void refreshMergedBatchRanges(Material defaultMaterial, bool visibilityDrawChanged, bool indexLayoutChanged, bool soaLaneDirty,
		bool clipWindowChanged, ref FastUIFrameStats stats)
	{
		mMeshRenderer.convertRenderRangesToDrawRanges(mBatchDirtyRanges, mBatchDrawDirtyRanges, mRenderRegistry.Count);
		if (soaLaneDirty)
		{
			FastUIRangeData_ECSList soaRanges = mSOARenderSystem.getDirtyLaneDrawRanges();
			var soaStarts = soaRanges.getStartColumn();
			var soaEnds = soaRanges.getEndColumn();
			for (int i = 0; i < soaRanges.Count; ++i)
			{
				mBatchDrawDirtyRanges.Add(new FastUIRangeData(soaStarts[i], soaEnds[i]));
			}
		}
		FastUIRangeUtility.merge(mBatchDrawDirtyRanges);
		int rangeCount = mBatchDrawDirtyRanges.Count;
		if (rangeCount > 0)
		{
			mMeshRenderer.beginBatchRefresh();
			var rangeStarts = mBatchDrawDirtyRanges.getStartColumn();
			var rangeEnds = mBatchDrawDirtyRanges.getEndColumn();
			for (int i = 0; i < rangeCount; ++i)
			{
				mMeshRenderer.appendBatchRefreshRange(rangeStarts[i], rangeEnds[i] - rangeStarts[i]);
			}
			mMeshRenderer.finishBatchRefresh(mRenderRegistry.getElements(), defaultMaterial, ref stats);
		}
		else if (visibilityDrawChanged)
		{
			mMeshRenderer.refreshVisibility(defaultMaterial, mRenderRegistry.Count, ref stats);
		}
		else if (indexLayoutChanged || clipWindowChanged)
		{
			// Clip窗口只改SubMesh Descriptor边界，不改Index Buffer；因此复用Descriptor Refresh即可。
			mMeshRenderer.refreshIndexLayoutDescriptors(defaultMaterial, mRenderRegistry.Count, ref stats);
		}
		if (indexLayoutChanged && rangeCount > 0)
		{
			// Compact模式的实际IndexCount变化会移动后续所有Prefix；即使本帧同时存在局部BatchKey变化，也必须刷新全部SubMesh Index区间。
			mMeshRenderer.refreshIndexLayoutDescriptors(defaultMaterial, mRenderRegistry.Count, ref stats);
		}
		mBatchDirtyRanges.Clear();
		mBatchDrawDirtyRanges.Clear();
	}
	private void finalizeFrameStats(long startTick, ref FastUIFrameStats stats)
	{
		stats.mClippedElementCount = mClipSystem != null ? mClipSystem.getClippedElementCount() : 0;
		mMeshRenderer.setActiveElementCount(mActiveRenderElementCount);
		if (!stats.mBatchRebuilt)
		{
			stats.mBatchCount = mMeshRenderer.getBatchCount();
		}
		stats.mActiveRenderElementCount = mActiveRenderElementCount;
		mMeshRenderer.uploadDirtyVertexStreams(ref stats);
		stats.mCPUTimeMS = (System.Diagnostics.Stopwatch.GetTimestamp() - startTick) * TICK_TO_MS;
		mLastFrameStats = stats;
	}
	private void queueInitialElementAllVertexDirty(int slot, bool textElement)
	{
		var dirtyStateColumn = mVertexSlotRuntimeECS.getDirtyStateColumn();
		int state = (int)FastUIDirtyFlags.AllVertex | VERTEX_DIRTY_QUEUED_BIT;
		state |= INITIAL_DIRTY_BIT;
		// 初次注册的FastText已经携带最终序列化文本，但不会像运行期setText那样经过markTextElementGeometryDirty。
		// 必须在这里直接加入Text Dirty队列，否则Clone/Prefab首次Flush会绕过Canvas Text Batch，逐Text走GeometryBuilder。
		if (textElement)
		{
			state |= TEXT_DIRTY_QUEUED_BIT;
			mDirtyTextVertexSlots.Add(slot);
		}
		dirtyStateColumn[slot] = state;
		mDirtyVertexSlots.Add(slot);
	}
	// 初次创建且Canvas存在RectMask时，不需要每个Graphic各自向上扫描Mask层级。
	// ClipSystem首次Structure Refresh已经使用Transform深度缓存一次性计算所有Reader/Writer状态。
	public bool tryDeferInitialGraphicMaskRefresh()
	{
		if (mClipSystem == null || mClipSystem.getClipRootCount() <= 0)
		{
			return false;
		}
		mClipSystem.markStructureDirty();
		return true;
	}
	private static int saturatedAdd(int a, int b)
	{
		long value = (long)Mathf.Max(a, 0) + Mathf.Max(b, 0);
		return value >= int.MaxValue ? int.MaxValue : (int)value;
	}
	public double beginCloneRegistrationBatch(int additionalRenderElementCount, int additionalTextCount, int renderElementCapacity = 0, int textCapacity = 0, Transform targetParent = null)
	{
		ensureInit();
		bool structureWasClean = !mDeferredStructureDirty;
		if (++mCloneRegistrationBatchDepth > 1)
		{
			mCloneRegistrationRegistryOrderCandidate = false;
			return 0.0;
		}
		mCloneRegistrationRegistryOrderCandidate = structureWasClean && isCloneTargetAtHierarchyTail(targetParent);
		mCloneRegistrationRegistryOrderValidated = false;
		int minimumRenderCapacity = saturatedAdd(mRenderRegistry.Count, Mathf.Max(additionalRenderElementCount, 0));
		int targetRenderCapacity = renderElementCapacity < 0 ? -1 : Mathf.Max(renderElementCapacity > 0 ? renderElementCapacity : minimumRenderCapacity, minimumRenderCapacity);
		if (targetRenderCapacity >= 0)
		{
			mRenderRegistry.ensureCapacity(targetRenderCapacity);
			mVertexSlotRuntimeECS.EnsureCapacity(targetRenderCapacity);
			mInitialSimpleQuadCandidateECS.EnsureCapacity(targetRenderCapacity);
			mDirtyVertexSlots.EnsureCapacity(Mathf.Max(mDirtyVertexSlots.Count, targetRenderCapacity));
			mMeshRenderer.prewarmInitialVertexSlotStorage(targetRenderCapacity);
		}
		int minimumTextCapacity = saturatedAdd(mTMPVertexLayoutElementCount, Mathf.Max(additionalTextCount, 0));
		int targetTextCapacity = textCapacity < 0 ? -1 : Mathf.Max(textCapacity > 0 ? textCapacity : minimumTextCapacity, minimumTextCapacity);
		if (targetTextCapacity >= 0 && targetTextCapacity > 0)
		{
			mDirtyTextVertexSlots.EnsureCapacity(Mathf.Max(mDirtyTextVertexSlots.Count, targetTextCapacity));
			mSimpleTextBatchWorkECS.EnsureCapacity(targetTextCapacity);
		}
		// Clone期间Graphic先只绑定Canvas并进入CloneUtility待提交队列，批量Instantiate结束后一次性写入Registry/ECS。
		// 如果业务在某个OnEnable里主动Flush，runFrameUpdate会先提交当前队列，后续Clone继续进入新的批次。
		scheduleDeferredStructure(false, true);
		return 0.0;
	}
	public void setCloneRegistrationRegistryOrderValidated(bool valid)
	{
		if (mCloneRegistrationBatchDepth <= 0)
		{
			return;
		}
		mCloneRegistrationRegistryOrderValidated = valid;
		if (!valid)
		{
			mCloneRegistrationRegistryOrderCandidate = false;
		}
	}
	private bool isCloneTargetAtHierarchyTail(Transform targetParent)
	{
		if (targetParent == null)
		{
			return false;
		}
		Transform current = targetParent;
		while (current != null && current != transform)
		{
			Transform parent = current.parent;
			if (parent == null || current.GetSiblingIndex() != parent.childCount - 1)
			{
				return false;
			}
			current = parent;
		}
		return current == transform;
	}
	public void endCloneRegistrationBatch()
	{
		if (mCloneRegistrationBatchDepth <= 0)
		{
			return;
		}
		if (--mCloneRegistrationBatchDepth > 0)
		{
			return;
		}
		mDeferredRegistryOrderAuthoritative = mCloneRegistrationRegistryOrderCandidate && mCloneRegistrationRegistryOrderValidated;
		mCloneRegistrationRegistryOrderCandidate = false;
		mCloneRegistrationRegistryOrderValidated = false;
		if (mCloneRegistrationStructureDirty)
		{
			mCloneRegistrationStructureDirty = false;
			if (!mDeferredStructureDirty)
			{
				scheduleDeferredStructure(false);
			}
		}
		if (mClipSystem != null && mClipSystem.getClipRootCount() > 0)
		{
			mClipSystem.markStructureDirty();
		}
	}
	public void registerClone(FastUIRenderElement element)
	{
		if (element == null || element.getVertexSlot() >= 0)
		{
			return;
		}
		if (mCloneRegistrationBatchDepth <= 0)
		{
			register(element);
			return;
		}
		bool requiresTMPLayout = element is FastText;
		if (requiresTMPLayout && mTMPVertexLayoutElementCount == 0)
		{
			mMeshRenderer.setTMPVertexLayoutEnabled(true);
		}
		int slot = mMeshRenderer.allocateVertexSlot();
		if (slot < 0)
		{
			return;
		}
		if (requiresTMPLayout)
		{
			++mTMPVertexLayoutElementCount;
		}
		element.setVertexSlot(slot);
		ensureVertexSlotRuntimeCapacity(slot + 1);
		var slotDirtyStateColumn = mVertexSlotRuntimeECS.getDirtyStateColumn();
		var slotPendingDeltaColumn = mVertexSlotRuntimeECS.getPendingLocalPositionDeltaColumn();
		var slotColorColumn = mVertexSlotRuntimeECS.getColorColumn();
		var slotUVRectColumn = mVertexSlotRuntimeECS.getUVRectColumn();
		mRenderRegistry.setVertexSlotOwner(slot, element);
		slotDirtyStateColumn[slot] = 0;
		slotPendingDeltaColumn[slot] = Vector3.zero;
		slotColorColumn[slot] = (Color32)element.getColor();
		bool initialSimpleUVCompatible = element.tryGetSimpleUVRect(out Rect initialUVRect);
		slotUVRectColumn[slot] = initialSimpleUVCompatible ? initialUVRect : default;
		mVertexSlotRuntimeECS.getDeltaTransformModeColumn()[slot] = DELTA_TRANSFORM_UNINITIALIZED;
		mInitialSimpleQuadCandidateECS.getValueColumn()[slot] = getInitialSimpleQuadCandidateValue(element, initialSimpleUVCompatible);
		mVertexSlotRuntimeECS.getPersistentTextGlyphCountColumn()[slot] = 0;
		if (element is FastText registeredText)
		{
			registeredText.notifyPersistentGlyphCacheAvailable();
		}
		syncVertexSlotRuntimeMetadata(slot, element);

		int provisionalIndex = mRenderRegistry.Count;
		mRenderRegistry.Add(element);
		element.setRenderOrderIndex(provisionalIndex);
		element.setCanvasRenderRange(provisionalIndex, 1);
		++mLiveRenderElementCount;
		if (element.isRenderActive())
		{
			++mActiveRenderElementCount;
		}
		queueInitialElementAllVertexDirty(slot, element is FastText);

		mCloneRegistrationStructureDirty = true;
		if (!mDeferredStructureDirty)
		{
			scheduleDeferredStructure(false);
		}
	}
	// FastUICloneUtility批量Instantiate专用：两遍处理一次拿齐ECS Column，并把Structure Dirty合并为一次。
	// 第一遍分配稳定VertexSlot并原地压紧有效Graphic；第二遍连续写Registry/ECS，避免每个OnEnable重复Ensure/Column获取/调度。
	public int registerCloneBatch(FastUIRenderElement[] elements, int count)
	{
		if (elements == null || count <= 0)
		{
			return 0;
		}
		ensureInit();
		count = Mathf.Min(count, elements.Length);
		if (mCloneRegistrationBatchDepth <= 0)
		{
			int fallbackCount = 0;
			for (int i = 0; i < count; ++i)
			{
				FastUIRenderElement element = elements[i];
				if (element == null)
				{
					continue;
				}
				element.clearCloneRegistrationPending();
				if (element.getCanvas() != this || element.getVertexSlot() >= 0)
				{
					continue;
				}
				register(element);
				if (element.getVertexSlot() >= 0)
				{
					if (!element.getVisible())
					{
						setVisibilityRootVisible(element.getRectTransform(), false);
					}
					++fallbackCount;
				}
			}
			return fallbackCount;
		}
		int validCount = 0;
		int textCount = 0;
		int maxSlot = -1;
		for (int i = 0; i < count; ++i)
		{
			FastUIRenderElement element = elements[i];
			if (element == null)
			{
				continue;
			}
			element.clearCloneRegistrationPending();
			if (element.getCanvas() != this || element.getVertexSlot() >= 0)
			{
				continue;
			}
			int slot = mMeshRenderer.allocateVertexSlot();
			if (slot < 0)
			{
				continue;
			}
			element.setVertexSlot(slot);
			maxSlot = Mathf.Max(maxSlot, slot);
			if (element is FastText)
			{
				++textCount;
			}
			elements[validCount++] = element;
		}
		if (validCount <= 0)
		{
			return 0;
		}
		ensureVertexSlotRuntimeCapacity(maxSlot + 1);
		mRenderRegistry.ensureCapacity(saturatedAdd(mRenderRegistry.Count, validCount));
		if (textCount > 0)
		{
			if (mTMPVertexLayoutElementCount == 0)
			{
				mMeshRenderer.setTMPVertexLayoutEnabled(true);
			}
			mTMPVertexLayoutElementCount += textCount;
		}
		var slotDirtyStateColumn = mVertexSlotRuntimeECS.getDirtyStateColumn();
		var slotPendingDeltaColumn = mVertexSlotRuntimeECS.getPendingLocalPositionDeltaColumn();
		var slotColorColumn = mVertexSlotRuntimeECS.getColorColumn();
		var slotUVRectColumn = mVertexSlotRuntimeECS.getUVRectColumn();
		var slotDeltaTransformModeColumn = mVertexSlotRuntimeECS.getDeltaTransformModeColumn();
		var slotPersistentGlyphCountColumn = mVertexSlotRuntimeECS.getPersistentTextGlyphCountColumn();
		var slotRequiresGeometryColorColumn = mVertexSlotRuntimeECS.getRequiresGeometryRebuildForColorColumn();
		var slotGeometryControlsColorColumn = mVertexSlotRuntimeECS.getGeometryControlsVertexColorColumn();
		var slotInitialSimpleQuadCandidateColumn = mInitialSimpleQuadCandidateECS.getValueColumn();
		for (int i = 0; i < validCount; ++i)
		{
			FastUIRenderElement element = elements[i];
			int slot = element.getVertexSlot();
			mRenderRegistry.setVertexSlotOwner(slot, element);
			slotDirtyStateColumn[slot] = 0;
			slotPendingDeltaColumn[slot] = Vector3.zero;
			slotColorColumn[slot] = (Color32)element.getColor();
			bool initialSimpleUVCompatible = element.tryGetSimpleUVRect(out Rect initialUVRect);
			slotUVRectColumn[slot] = initialSimpleUVCompatible ? initialUVRect : default;
			slotDeltaTransformModeColumn[slot] = DELTA_TRANSFORM_UNINITIALIZED;
			slotPersistentGlyphCountColumn[slot] = 0;
			slotRequiresGeometryColorColumn[slot] = element.requiresGeometryRebuildForColor() ? 1 : 0;
			slotGeometryControlsColorColumn[slot] = element.geometryControlsVertexColor() ? 1 : 0;
			slotInitialSimpleQuadCandidateColumn[slot] = getInitialSimpleQuadCandidateValue(element, initialSimpleUVCompatible);
			if (element is FastText registeredText)
			{
				registeredText.notifyPersistentGlyphCacheAvailable();
			}

			int provisionalIndex = mRenderRegistry.Count;
			mRenderRegistry.Add(element);
			element.setRenderOrderIndex(provisionalIndex);
			element.setCanvasRenderRange(provisionalIndex, 1);
			++mLiveRenderElementCount;
			if (element.isRenderActive())
			{
				++mActiveRenderElementCount;
			}
			int initialDirtyState = (int)FastUIDirtyFlags.AllVertex | VERTEX_DIRTY_QUEUED_BIT;
			initialDirtyState |= INITIAL_DIRTY_BIT;
			if (element is FastText)
			{
				initialDirtyState |= TEXT_DIRTY_QUEUED_BIT;
				mDirtyTextVertexSlots.Add(slot);
			}
			slotDirtyStateColumn[slot] = initialDirtyState;
			mDirtyVertexSlots.Add(slot);

			if (!element.getVisible())
			{
				setVisibilityRootVisible(element.getRectTransform(), false);
			}
		}
		mCloneRegistrationStructureDirty = true;
		if (!mDeferredStructureDirty)
		{
			scheduleDeferredStructure(false);
		}
		return validCount;
	}
	// Register只做必须立即有效的源数据：VertexSlot、Owner、最终组件状态和O(1) Registry Append。
	// Hierarchy/TransformRange/SOA/Batch/Index/Visibility全部延迟到本帧统一Flush。
	public void register(FastUIRenderElement element)
	{
		if (mDestroying || element == null || element.getCanvas() != this)
		{
			return;
		}

		ensureInit();
		if (!mInitialized || mMeshRenderer == null)
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				queueEditorPreviewRefresh();
			}
#endif
			return;
		}

		if (element.getVertexSlot() >= 0)
		{
			return;
		}

		bool requiresTMPLayout = mRenderRegistry.requiresTMPVertexLayout(element);
		if (requiresTMPLayout)
		{
			mMeshRenderer.setTMPVertexLayoutEnabled(true);
		}

		int slot = mMeshRenderer.allocateVertexSlot();
		if (slot < 0)
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				queueEditorPreviewRefresh();
			}
#endif
			return;
		}

		if (requiresTMPLayout)
		{
			++mTMPVertexLayoutElementCount;
		}

		element.setVertexSlot(slot);
		ensureVertexSlotRuntimeCapacity(slot + 1);
		var slotDirtyStateColumn = mVertexSlotRuntimeECS.getDirtyStateColumn();
		var slotPendingDeltaColumn = mVertexSlotRuntimeECS.getPendingLocalPositionDeltaColumn();
		var slotColorColumn = mVertexSlotRuntimeECS.getColorColumn();
		var slotUVRectColumn = mVertexSlotRuntimeECS.getUVRectColumn();
		mRenderRegistry.setVertexSlotOwner(slot, element);
		slotDirtyStateColumn[slot] = 0;
		slotPendingDeltaColumn[slot] = Vector3.zero;
		slotColorColumn[slot] = (Color32)element.getColor();
		bool initialSimpleUVCompatible = element.tryGetSimpleUVRect(out Rect initialUVRect);
		slotUVRectColumn[slot] = initialSimpleUVCompatible ? initialUVRect : default;
		mVertexSlotRuntimeECS.getDeltaTransformModeColumn()[slot] = DELTA_TRANSFORM_UNINITIALIZED;
		mInitialSimpleQuadCandidateECS.getValueColumn()[slot] = getInitialSimpleQuadCandidateValue(element, initialSimpleUVCompatible);
		mVertexSlotRuntimeECS.getPersistentTextGlyphCountColumn()[slot] = 0;
		if (element is FastText registeredText)
		{
			registeredText.notifyPersistentGlyphCacheAvailable();
		}

		syncVertexSlotRuntimeMetadata(slot, element);

		int provisionalIndex = mRenderRegistry.Count;
		mRenderRegistry.Add(element);
		element.setRenderOrderIndex(provisionalIndex);
		element.setCanvasRenderRange(provisionalIndex, 1);
		++mLiveRenderElementCount;
		if (element.isRenderActive())
		{
			++mActiveRenderElementCount;
		}

		queueInitialElementAllVertexDirty(slot, element is FastText);
		scheduleDeferredStructure(false);
	}
	// 注销元素优先留下Tombstone，真正压缩由独立策略延后执行。
	public void unregister(FastUIRenderElement element)
	{
		if (mDestroying || element == null)
		{
			return;
		}
		int slot = element.getVertexSlot();
		if (slot < 0)
		{
			return;
		}
		int index = element.getRenderOrderIndex();
		if (index < 0 || index >= mRenderRegistry.Count || mRenderRegistry[index] != element)
		{
			index = mRenderRegistry.IndexOf(element);
		}
		if (element.isRenderActive())
		{
			mActiveRenderElementCount = Mathf.Max(0, mActiveRenderElementCount - 1);
		}
		if (index >= 0)
		{
			mRenderRegistry[index] = null;
			++mTombstoneCount;
		}
		mLiveRenderElementCount = Mathf.Max(0, mLiveRenderElementCount - 1);
		if (slot < mVertexSlotRuntimeECS.Count)
		{
			mRenderRegistry.clearVertexSlotOwner(slot, element);
			mVertexSlotRuntimeECS.getDirtyStateColumn()[slot] = 0;
			mVertexSlotRuntimeECS.getPendingLocalPositionDeltaColumn()[slot] = Vector3.zero;
			mVertexSlotRuntimeECS.getColorColumn()[slot] = default;
			mVertexSlotRuntimeECS.getUVRectColumn()[slot] = default;
			mVertexSlotRuntimeECS.getDeltaTransformModeColumn()[slot] = DELTA_TRANSFORM_DIRECT;
			mVertexSlotRuntimeECS.getDeltaRow0Column()[slot] = Vector3.right;
			mVertexSlotRuntimeECS.getDeltaRow1Column()[slot] = Vector3.up;
			mVertexSlotRuntimeECS.getDeltaRow2Column()[slot] = Vector3.forward;
			mVertexSlotRuntimeECS.getRequiresGeometryRebuildForColorColumn()[slot] = 0;
			mVertexSlotRuntimeECS.getGeometryControlsVertexColorColumn()[slot] = 0;
			mVertexSlotRuntimeECS.getTextBatchResultFlagsColumn()[slot] = 0;
			mVertexSlotRuntimeECS.getPersistentTextGlyphCountColumn()[slot] = 0;
			if (slot < mInitialSimpleQuadCandidateECS.Count)
			{
				mInitialSimpleQuadCandidateECS.getValueColumn()[slot] = 0;
			}
		}
		mMeshRenderer.releaseVertexSlot(slot);
		if (mRenderRegistry.requiresTMPVertexLayout(element))
		{
			mTMPVertexLayoutElementCount = Mathf.Max(0, mTMPVertexLayoutElementCount - 1);
			if (mTMPVertexLayoutElementCount == 0)
			{
				mMeshRenderer.setTMPVertexLayoutEnabled(false);
			}
		}
		element.setVertexSlot(-1);
		element.setRenderOrderIndex(-1);
		element.setCanvasRenderRange(-1, 1);
		scheduleDeferredStructure(false);
	}
	public void notifyElementActiveChanged(FastUIRenderElement element, bool oldActive, bool newActive)
	{
		if (mDestroying || mMeshRenderer == null || element == null || element.getCanvas() != this || element.getVertexSlot() < 0 || oldActive == newActive)
		{
			return;
		}
		if (shouldSkipDerivedMutationForDeferredStructure())
		{
			return;
		}
		mActiveRenderElementCount += newActive ? 1 : -1;
		mActiveRenderElementCount = Mathf.Clamp(mActiveRenderElementCount, 0, mLiveRenderElementCount);
		int renderIndex = element.getRenderOrderIndex();
		if (renderIndex < 0)
		{
			return;
		}
		mMeshRenderer.patchBatchElementActiveState(renderIndex, newActive);
		addIndexDirtyRange(renderIndex, 1);
	}
	public void notifyElementVisibleChanged(FastUIRenderElement element, bool oldVisible, bool newVisible)
	{
		if (mDestroying || mMeshRenderer == null || element == null || element.getCanvas() != this || element.getVertexSlot() < 0 || oldVisible == newVisible)
		{
			return;
		}
		if (shouldSkipDerivedMutationForDeferredStructure())
		{
			return;
		}
		int renderIndex = element.getRenderOrderIndex();
		if (renderIndex >= 0)
		{
			mMeshRenderer.patchBatchElementVisibleState(renderIndex, newVisible);
		}
		ensureTransformRanges();
		int renderStart = element.getCanvasRenderStart();
		int renderCount = element.getCanvasRenderCount();
		if (renderStart < 0 || renderCount <= 1)
		{
			mVisibility?.removeRoot(element.getRectTransform());
			if (renderIndex >= 0)
			{
				addIndexDirtyRange(renderIndex, 1);
			}
			return;
		}
		mVisibility?.setElementVisibleRoot(element.getRectTransform(), newVisible);
	}
	public void notifyElementCullChanged(FastUIRenderElement element)
	{
		if (mDestroying || mMeshRenderer == null || element == null || element.getCanvas() != this || element.getVertexSlot() < 0)
		{
			return;
		}
		if (shouldSkipDerivedMutationForDeferredStructure())
		{
			return;
		}
		int renderIndex = element.getRenderOrderIndex();
		if (renderIndex >= 0)
		{
			mMeshRenderer.patchBatchElementCullState(renderIndex, element.isCulled());
			addIndexDirtyRange(renderIndex, 1);
		}
	}
	// RawImage Texture只改BatchKey的Texture列，避免重新查询Material/Member/RenderState。
	public void notifyElementTextureChanged(FastUIRenderElement element)
	{
		if (mDestroying || mMeshRenderer == null || element == null || element.getCanvas() != this || element.getVertexSlot() < 0)
		{
			return;
		}
		if (shouldSkipDerivedMutationForDeferredStructure())
		{
			return;
		}
		int renderIndex = element.getRenderOrderIndex();
		if (renderIndex < 0)
		{
			return;
		}
		mMeshRenderer.syncBatchElementTextureState(renderIndex, element.getRenderTexture());
		if (mSOARenderSystem != null && mSOARenderSystem.markBatchKeyDirty(renderIndex))
		{
			return;
		}
		addBatchDirtyRange(renderIndex, 1);
	}
	public void notifyElementBatchChanged(FastUIRenderElement element)
	{
		if (mDestroying || mMeshRenderer == null || element == null || element.getCanvas() != this || element.getVertexSlot() < 0)
		{
			return;
		}
		if (shouldSkipDerivedMutationForDeferredStructure())
		{
			return;
		}
		int renderIndex = element.getRenderOrderIndex();
		if (renderIndex < 0)
		{
			return;
		}
		Material defaultMaterial = mDefaultMaterial != null ? mDefaultMaterial : mRuntimeDefaultMaterial;
		if (defaultMaterial != null)
		{
			mMeshRenderer.syncBatchElementState(renderIndex, element, defaultMaterial);
		}
		// 已成功转换的SOA元素直接O(1)定位Lane，不再扫描Group/Record，也不升级为Full SOA Build。
		if (mSOARenderSystem != null && mSOARenderSystem.markBatchKeyDirty(renderIndex))
		{
			return;
		}
		addBatchDirtyRange(renderIndex, 1);
	}
	public void notifyStencilBarrierChanged()
	{
		if (hasSOARenderGroups())
		{
			mFullDrawStructureDirty = true;
		}
	}
	public void notifyElementHierarchyOrderChanged(FastUIRenderElement element)
	{
		if (element == null || element.getCanvas() != this || element.getVertexSlot() < 0)
		{
			return;
		}
		scheduleDeferredStructure(false);
	}
	public void notifyTransformHierarchyOrderChanged(Transform root)
	{
		if (root == null || root == transform)
		{
			return;
		}
		scheduleDeferredStructure(false);
	}
	// FastText Layout属性变化走专用入口：不在Mutation阶段同步触发Layout，也不让通用Dirty队列做运行时类型判断。
	public void requestDeferredClipTextResume()
	{
		mDeferredClipTextResumePending = true;
	}
	public void markTextElementGeometryDirty(FastText element)
	{
		if (element == null || element.getCanvas() != this)
		{
			return;
		}
		int slot = element.getVertexSlot();
		if (slot < 0)
		{
			return;
		}
		queueElementDirtySlot(element, slot, FastUIDirtyFlags.Geometry, true);
	}
	public void markTextElementUV0Dirty(FastText element)
	{
		if (element == null || element.getCanvas() != this)
		{
			return;
		}
		int slot = element.getVertexSlot();
		if (slot < 0)
		{
			return;
		}
		queueElementDirtySlot(element, slot, FastUIDirtyFlags.UV0Only);
	}
	public void markElementDirty(FastUIRenderElement element, FastUIDirtyFlags flags)
	{
		if (mDestroying || element == null || flags == FastUIDirtyFlags.None || element.getCanvas() != this)
		{
			return;
		}
		if (!mECSCollectionsInitialized || mVertexSlotRuntimeECS == null || mDirtyVertexSlots == null)
		{
#if UNITY_EDITOR
			if (!Application.isPlaying)
			{
				queueEditorPreviewRefresh();
			}
#endif
			return;
		}
		int slot = element.getVertexSlot();
		if ((uint)slot >= (uint)mVertexSlotRuntimeECS.Count)
		{
			return;
		}
		if ((flags & FastUIDirtyFlags.Color) != 0)
		{
			mVertexSlotRuntimeECS.getColorColumn()[slot] = (Color32)element.getColor();
		}
		if ((flags & FastUIDirtyFlags.AnyUV) != 0)
		{
			if (element.tryGetSimpleUVRect(out Rect uvRect))
			{
				mVertexSlotRuntimeECS.getUVRectColumn()[slot] = uvRect;
			}
			else
			{
				flags = (flags & ~FastUIDirtyFlags.AnyUV) | FastUIDirtyFlags.Geometry;
			}
		}
		// RuntimeMetadata只会随Geometry语义变化；单纯Color变化不会改变requiresGeometryRebuildForColor/geometryControlsVertexColor。
		// FastText RichColor状态也由Text/Layout Geometry变化驱动，因此Color热路径不再重复两个virtual查询。
		if ((flags & FastUIDirtyFlags.Geometry) != 0)
		{
			syncVertexSlotRuntimeMetadata(slot, element);
		}
		if ((flags & (FastUIDirtyFlags.Transform | FastUIDirtyFlags.Geometry)) != 0)
		{
			mClipSystem?.markGeometryDirty(element.getRenderOrderIndex());
		}
		queueElementDirtySlot(element, slot, flags);
	}
	private void queueElementDirtySlot(FastUIRenderElement element, int slot, FastUIDirtyFlags flags, bool queueTextGeometry = false)
	{
		var dirtyStateColumn = mVertexSlotRuntimeECS.getDirtyStateColumn();
		int state = dirtyStateColumn[slot];
		FastUIDirtyFlags oldFlags = (FastUIDirtyFlags)(state & VERTEX_DIRTY_FLAGS_MASK);
		if ((flags & FastUIDirtyFlags.PositionDelta) != 0 && (oldFlags & FastUIDirtyFlags.PositionDelta) == 0)
		{
			++mPendingLeafPositionDeltaCount;
		}
		if ((flags & FastUIDirtyFlags.Geometry) != 0 && (oldFlags & FastUIDirtyFlags.Geometry) == 0)
		{
			element.notifyAttachedMaskGeometryChanged();
		}
		state |= (int)flags;
		if (queueTextGeometry && (state & TEXT_DIRTY_QUEUED_BIT) == 0)
		{
			state |= TEXT_DIRTY_QUEUED_BIT;
			mDirtyTextVertexSlots.Add(slot);
		}
		if ((state & VERTEX_DIRTY_QUEUED_BIT) != 0)
		{
			dirtyStateColumn[slot] = state;
			return;
		}
		dirtyStateColumn[slot] = state | VERTEX_DIRTY_QUEUED_BIT;
		mDirtyVertexSlots.Add(slot);
	}
	private void queueLeafPositionDeltaFast(int slot, Vector3 localDelta)
	{
		var dirtyStateColumn = mVertexSlotRuntimeECS.getDirtyStateColumn();
		int state = dirtyStateColumn[slot];
		FastUIDirtyFlags oldFlags = (FastUIDirtyFlags)(state & VERTEX_DIRTY_FLAGS_MASK);
		if ((oldFlags & FastUIDirtyFlags.PositionDelta) == 0)
		{
			++mPendingLeafPositionDeltaCount;
		}
		mVertexSlotRuntimeECS.getPendingLocalPositionDeltaColumn()[slot] += localDelta;
		state |= (int)FastUIDirtyFlags.PositionDelta;
		if ((state & VERTEX_DIRTY_QUEUED_BIT) != 0)
		{
			dirtyStateColumn[slot] = state;
			return;
		}
		dirtyStateColumn[slot] = state | VERTEX_DIRTY_QUEUED_BIT;
		mDirtyVertexSlots.Add(slot);
	}
	public void notifyElementPositionChanged(FastUIRenderElement element, Vector3 oldLocalPosition, Vector3 newLocalPosition)
	{
		if (element == null || element.getCanvas() != this || oldLocalPosition == newLocalPosition)
		{
			return;
		}
		if (mDeferredStructureDirty && !mFlushingDeferredStructure)
		{
			mDeferredStructureTransformAllDirty = true;
			return;
		}
		int slot = element.getVertexSlot();
		if (slot < 0)
		{
			return;
		}
		notifyElementPositionDeltaFast(element, slot, element.getCanvasRenderCount(), newLocalPosition - oldLocalPosition);
	}
	public void notifyElementPositionDeltaFast(FastUIRenderElement element, int vertexSlot, int cachedRenderCount, Vector3 localDelta)
	{
		if (element == null || vertexSlot < 0 || localDelta == Vector3.zero)
		{
			return;
		}
		if (mDeferredStructureDirty && !mFlushingDeferredStructure)
		{
			mDeferredStructureTransformAllDirty = true;
			return;
		}

		mClipSystem?.markGeometryDirty(element.getRenderOrderIndex());
		int dirtyState = mVertexSlotRuntimeECS.getDirtyStateColumn()[vertexSlot];
		FastUIDirtyFlags queuedFlags = (FastUIDirtyFlags)(dirtyState & VERTEX_DIRTY_FLAGS_MASK);
		if ((dirtyState & VERTEX_DIRTY_QUEUED_BIT) != 0 &&
			(queuedFlags & (FastUIDirtyFlags.Transform | FastUIDirtyFlags.Geometry)) != 0)
		{
			return;
		}
		if ((dirtyState & VERTEX_DIRTY_QUEUED_BIT) != 0 &&
			(queuedFlags & FastUIDirtyFlags.PositionDelta) != 0 &&
			(queuedFlags & (FastUIDirtyFlags.Transform | FastUIDirtyFlags.Geometry)) == 0)
		{
			mVertexSlotRuntimeECS.getPendingLocalPositionDeltaColumn()[vertexSlot] += localDelta;
			return;
		}
		if (!mTransformRanges.isDirty() && cachedRenderCount == 1)
		{
			queueLeafPositionDeltaFast(vertexSlot, localDelta);
			return;
		}
		ensureTransformRanges();
		int renderStart = element.getCanvasRenderStart();
		int renderCount = element.getCanvasRenderCount();
		if (renderStart < 0 || renderCount <= 0)
		{
			queueElementDirtySlot(element, vertexSlot, FastUIDirtyFlags.Transform);
			return;
		}

		if (renderCount == 1)
		{
			queueLeafPositionDeltaFast(vertexSlot, localDelta);
			return;
		}
		applyPositionDelta(element.getRectTransform(), renderStart, renderCount, localDelta);
	}
	public void notifyElementTransformDirty(FastUIRenderElement element)
	{
		if (element == null || element.getCanvas() != this || element.getVertexSlot() < 0)
		{
			return;
		}
		// 泛化Transform Dirty没有old/new Matrix，无法证明Scale未变，保守失效一次SDF Scale缓存。
		invalidateTextSDFScaleCacheVersion();
		if (mDeferredStructureDirty && !mFlushingDeferredStructure)
		{
			mDeferredStructureTransformAllDirty = true;
			return;
		}

		ensureTransformRanges();
		int renderStart = element.getCanvasRenderStart();
		int renderCount = element.getCanvasRenderCount();
		if (renderStart < 0 || renderCount <= 0)
		{
			markElementDirty(element, FastUIDirtyFlags.Transform);
			return;
		}

		markRenderRangeDirty(renderStart, renderCount, FastUIDirtyFlags.Transform);
	}
	// 只更新当前RenderElement自身Geometry。
	// 适用于Rect尺寸变化但不存在可渲染子节点的叶节点，避免为单个Quad重复构建Transform Range并传播Transform Dirty。
	// DeferredStructure期间仍退回完整Geometry+Transform路径，保证Hierarchy尚未稳定时的正确性。
	public void notifyElementOwnGeometryDirty(FastUIRenderElement element)
	{
		if (element == null || element.getCanvas() != this || element.getVertexSlot() < 0)
		{
			return;
		}
		if (mDeferredStructureDirty && !mFlushingDeferredStructure)
		{
			markElementDirty(element, FastUIDirtyFlags.Geometry | FastUIDirtyFlags.Transform);
			mDeferredStructureTransformAllDirty = true;
			return;
		}
		markElementDirty(element, FastUIDirtyFlags.Geometry);
	}
	public void notifyElementGeometryDirty(FastUIRenderElement element)
	{
		if (element == null || element.getCanvas() != this || element.getVertexSlot() < 0)
		{
			return;
		}
		if (mDeferredStructureDirty && !mFlushingDeferredStructure)
		{
			markElementDirty(element, FastUIDirtyFlags.Geometry | FastUIDirtyFlags.Transform);
			mDeferredStructureTransformAllDirty = true;

			return;
		}
		int vertexSlot = element.getVertexSlot();
		int dirtyState = mVertexSlotRuntimeECS.getDirtyStateColumn()[vertexSlot];
		if ((dirtyState & VERTEX_DIRTY_QUEUED_BIT) != 0 &&
			(((FastUIDirtyFlags)(dirtyState & VERTEX_DIRTY_FLAGS_MASK)) & FastUIDirtyFlags.Geometry) != 0)
		{
			return;
		}

		ensureTransformRanges();
		int renderStart = element.getCanvasRenderStart();
		int renderCount = element.getCanvasRenderCount();
		if (renderStart < 0 || renderCount <= 0)
		{
			markElementDirty(element, FastUIDirtyFlags.Geometry | FastUIDirtyFlags.Transform);
			return;
		}

		element.invalidateCachedGeometryLocalToCanvasMatrix();
		markElementDirty(element, FastUIDirtyFlags.Geometry | FastUIDirtyFlags.Transform);
		int renderEnd = Mathf.Min(renderStart + renderCount, mRenderRegistry.Count);
		for (int i = renderStart; i < renderEnd; ++i)
		{
			FastUIRenderElement child = mRenderRegistry[i];
			if (child != element && child != null)
			{
				child.invalidateCachedGeometryLocalToCanvasMatrix();
				markElementDirty(child, FastUIDirtyFlags.Transform);
			}
		}
	}
	// 同一业务循环批量移动多个UI Root时使用。
	// begin/end之间的Root Position通知只解析Transform Range并写入EasyECS；
	// 相邻Range且Canvas Delta相同会直接合并，end时用一个连续Position FastPath统一应用。
	public void beginTransformPositionBatch()
	{
		ensureECSCollections();
		if (mTransformPositionBatchDepth++ == 0)
		{
			mTransformPositionBatchECS.Clear();
		}
	}
	public void endTransformPositionBatch()
	{
		if (mTransformPositionBatchDepth <= 0)
		{
			return;
		}
		if (--mTransformPositionBatchDepth == 0)
		{
			flushTransformPositionBatch();
		}
	}
	private void queueTransformPositionBatch(int renderStart, int renderCount, Vector3 canvasDelta)
	{
		if (renderCount <= 0 || canvasDelta == Vector3.zero)
		{
			return;
		}

		int count = mTransformPositionBatchECS.Count;
		if (count > 0)
		{
			int last = count - 1;
			var startColumn = mTransformPositionBatchECS.getRenderStartColumn();
			var countColumn = mTransformPositionBatchECS.getRenderCountColumn();
			var deltaColumn = mTransformPositionBatchECS.getCanvasDeltaColumn();
			if (startColumn[last] + countColumn[last] == renderStart && deltaColumn[last] == canvasDelta)
			{
				countColumn[last] += renderCount;
				return;
			}
		}
		mTransformPositionBatchECS.Add(new FastUITransformPositionBatchData(renderStart, renderCount, canvasDelta));
	}
	private void flushTransformPositionBatch()
	{
		if (mTransformPositionBatchECS == null || mTransformPositionBatchECS.Count == 0)
		{
			return;
		}
		using (POSITION_BATCH_MARKER.Auto())
		{
			var startColumn = mTransformPositionBatchECS.getRenderStartColumn();
			var countColumn = mTransformPositionBatchECS.getRenderCountColumn();
			var deltaColumn = mTransformPositionBatchECS.getCanvasDeltaColumn();
			int count = mTransformPositionBatchECS.Count;
			for (int i = 0; i < count; ++i)
			{
				int renderStart = startColumn[i];
				int renderCount = countColumn[i];
				Vector3 canvasDelta = deltaColumn[i];
				queueGeometryMatrixPositionTranslation(renderStart, renderCount, canvasDelta);
				offsetRenderRange(renderStart, renderCount, canvasDelta);
				mClipSystem?.markTranslatedRange(renderStart, renderCount, canvasDelta);
			}
		}
		mTransformPositionBatchECS.Clear();
	}
	private void invalidateRenderSlotContiguityCache()
	{
		mRenderVertexSlotMapKnown = false;
	}
	public void notifyTransformPositionChanged(RectTransform root, Vector3 oldLocalPosition)
	{
		if (mDeferredStructureDirty && !mFlushingDeferredStructure)
		{
			mDeferredStructureTransformAllDirty = true;
			return;
		}
		if (root != null)
		{
			notifyTransformPositionChanged(root, oldLocalPosition, root.localPosition);
		}
	}
	public void notifyTransformPositionChanged(RectTransform root, Vector3 oldLocalPosition, Vector3 newLocalPosition)
	{
		if (mDeferredStructureDirty && !mFlushingDeferredStructure)
		{
			mDeferredStructureTransformAllDirty = true;
			return;
		}
		if (root == null || root == transform || oldLocalPosition == newLocalPosition)
		{
			return;
		}
		if (root.TryGetComponent(out FastUIRenderElement rootElement) && rootElement.getCanvas() == this)
		{
			rootElement.refreshPositionCacheFromExternalChange();
			notifyElementPositionChanged(rootElement, oldLocalPosition, newLocalPosition);
			return;
		}

		if (!tryGetTransformRange(root, out int renderStart, out int renderCount) || renderCount <= 0)
		{
			return;
		}
		Vector3 localDelta = newLocalPosition - oldLocalPosition;
		if (mTransformPositionBatchDepth > 0)
		{
			Vector3 canvasDelta = convertLocalPositionDeltaToCanvas(root, localDelta);
			queueTransformPositionBatch(renderStart, renderCount, canvasDelta);
			return;
		}
		applyPositionDelta(root, renderStart, renderCount, localDelta);
	}
	public void notifyElementTransformMatrixChanged(FastUIRenderElement element, Matrix4x4 oldLocalToWorld, Matrix4x4 newLocalToWorld)
	{
		if (element == null || element.getCanvas() != this || element.getVertexSlot() < 0)
		{
			return;
		}

		ensureTransformRanges();
		int renderStart = element.getCanvasRenderStart();
		int renderCount = element.getCanvasRenderCount();
		if (renderStart < 0 || renderCount <= 1)
		{
			invalidateTextSDFScaleCacheIfNeeded(oldLocalToWorld, newLocalToWorld);
			markElementDirty(element, FastUIDirtyFlags.Transform);
			return;
		}
		applyMatrixDelta(renderStart, renderCount, oldLocalToWorld, newLocalToWorld);
	}
	public void notifyTransformMatrixChanged(RectTransform root, Matrix4x4 oldLocalToWorld)
	{
		if (root == null || root == transform)
		{
			return;
		}
		if (root.TryGetComponent(out FastUIRenderElement rootElement) && rootElement.getCanvas() == this)
		{
			rootElement.refreshTransformCacheFromExternalChange();
			notifyElementTransformMatrixChanged(rootElement, oldLocalToWorld, root.localToWorldMatrix);
			return;
		}

		if (!tryGetTransformRange(root, out int renderStart, out int renderCount) || renderCount <= 0)
		{
			return;
		}
		applyMatrixDelta(renderStart, renderCount, oldLocalToWorld, root.localToWorldMatrix);
	}
	public void notifyTransformDirty(Transform root)
	{
		if (root == null || root == transform)
		{
			return;
		}
		// 无old/new Matrix的泛化入口无法证明只是Position变化，保守失效一次SDF Scale缓存。
		invalidateTextSDFScaleCacheVersion();
		if (!tryGetTransformRange(root, out int renderStart, out int renderCount) || renderCount <= 0)
		{
			return;
		}
		markRenderRangeDirty(renderStart, renderCount, FastUIDirtyFlags.Transform);
	}
	// 首次FastText Stable Glyph Vertex Range批量连续分配，并拆分冷启动内部阶段；Text Slot State一次扩到最终Count。
	// 只处理Initial Dirty且尚无Geometry Range的Text；任何不满足条件或FreeRange存在时都保留原逐Text路径。
	private void prepareInitialSimpleTextRangeBatch(FastUIRenderElement[] slotElements)
	{
		if (mMeshRenderer == null ||
			mDirtyTextVertexSlots == null ||
			mInitialSimpleTextRangeBatchSlots == null ||
			mInitialSimpleTextRangeBatchCapacities == null)
		{
			return;
		}
		int dirtyTextCount = mDirtyTextVertexSlots.Count;
		if (dirtyTextCount < Mathf.Max(mInitialSimpleTextRangeBatchMinCount, 1))
		{
			return;
		}
		mInitialSimpleTextRangeBatchSlots.Clear();
		mInitialSimpleTextRangeBatchCapacities.Clear();
		if (mInitialSimpleTextRangeBatchSlots.Capacity < dirtyTextCount)
		{
			mInitialSimpleTextRangeBatchSlots.EnsureCapacity(dirtyTextCount);
		}
		if (mInitialSimpleTextRangeBatchCapacities.Capacity < dirtyTextCount)
		{
			mInitialSimpleTextRangeBatchCapacities.EnsureCapacity(dirtyTextCount);
		}
		var dirtyTextSlots = mDirtyTextVertexSlots.getValueColumn();
		var dirtyStateColumn = mVertexSlotRuntimeECS.getDirtyStateColumn();
		var persistentGlyphCapacityColumn = mVertexSlotRuntimeECS.getPersistentTextGlyphCapacityColumn();
		FastUIGeometryRangeData_ECSList geometryRanges = mMeshRenderer.getGeometryRangeECS();
		var vertexStarts = geometryRanges.getVertexStartColumn();
		var vertexCounts = geometryRanges.getVertexCountColumn();
		var vertexCapacities = geometryRanges.getVertexCapacityColumn();
		for (int i = 0; i < dirtyTextCount; ++i)
		{
			int slot = dirtyTextSlots[i];
			if ((uint)slot >= (uint)mVertexSlotRuntimeECS.Count || (uint)slot >= (uint)geometryRanges.Count)
			{
				continue;
			}
			int dirtyState = dirtyStateColumn[slot];
			if ((dirtyState & (INITIAL_DIRTY_BIT | TEXT_DIRTY_QUEUED_BIT)) != (INITIAL_DIRTY_BIT | TEXT_DIRTY_QUEUED_BIT) ||
				vertexStarts[slot] >= 0 || vertexCounts[slot] != 0 || vertexCapacities[slot] != 0)
			{
				continue;
			}
			FastText text = slotElements != null && slot < slotElements.Length ? slotElements[slot] as FastText : null;
			if (text == null)
			{
				continue;
			}
			int semanticCapacityHint = text.getInitialSimpleTextVertexCapacityHint();
			if (semanticCapacityHint <= 0)
			{
				continue;
			}
			int glyphCapacity = persistentGlyphCapacityColumn[slot];
			int capacityHint = glyphCapacity > 0 && glyphCapacity <= int.MaxValue / 4 ? glyphCapacity * 4 : semanticCapacityHint;
			if (capacityHint <= 0)
			{
				continue;
			}
			mInitialSimpleTextRangeBatchSlots.Add(slot);
			mInitialSimpleTextRangeBatchCapacities.Add(capacityHint);
		}

		int candidateCount = mInitialSimpleTextRangeBatchSlots.Count;
		if (candidateCount < Mathf.Max(mInitialSimpleTextRangeBatchMinCount, 1))
		{
			return;
		}
		if (!mMeshRenderer.tryAllocateInitialSimpleTextRangeBatch(mInitialSimpleTextRangeBatchSlots, mInitialSimpleTextRangeBatchCapacities, candidateCount, out _, out _))
		{
			return;
		}
		// Range Capacity已经从0建立，后续Text Prepare会返回与旧逐Slot首次分配相同的IndexCapacityChanged语义。
		mIndexCapacityDirty = true;
	}
	// Text Batch入口只消费已收集的Text Dirty Slot。
	// Dirty入口已经把FastText Slot单独收集到EasyECS，这里不再二次扫描全部Dirty Slot。
	// 当数量达到阈值时，复杂Text语义只做一次Prepare，随后统一交给纯数据Vertex Batch。
	private void prepareSimpleTextCanvasBatch(Matrix4x4 canvasWorldToLocal, FastUIRenderElement[] slotElements)
	{
		if (mSimpleTextBatchWorkECS == null || mPersistentTextGlyphECS == null || mDirtyTextVertexSlots == null)
		{
			return;
		}
		int count = mDirtyTextVertexSlots.Count;
		if (count < Mathf.Max(mSimpleTextCanvasBatchMinCount, 1))
		{
			return;
		}
		if (mSimpleTextBatchWorkECS.Capacity < count)
		{
			mSimpleTextBatchWorkECS.EnsureCapacity(count);
		}
		mSimpleTextBatchWorkECS.Clear();
		var dirtyTextSlotColumn = mDirtyTextVertexSlots.getValueColumn();
		var dirtyStateColumn = mVertexSlotRuntimeECS.getDirtyStateColumn();
		var slotColorColumn = mVertexSlotRuntimeECS.getColorColumn();
		var textBatchResultColumn = mVertexSlotRuntimeECS.getTextBatchResultFlagsColumn();
		for (int i = 0; i < count; ++i)
		{
			int slot = dirtyTextSlotColumn[i];
			if (slot < 0 || slot >= mVertexSlotRuntimeECS.Count)
			{
				continue;
			}
			int dirtyState = dirtyStateColumn[slot];
			FastUIDirtyFlags flags = (FastUIDirtyFlags)(dirtyState & VERTEX_DIRTY_FLAGS_MASK);
			bool initialDirty = (dirtyState & INITIAL_DIRTY_BIT) != 0;
			if ((flags & (FastUIDirtyFlags.Transform | FastUIDirtyFlags.Geometry)) == 0)
			{
				continue;
			}
			FastUIRenderElement element = slotElements != null && slot < slotElements.Length ? slotElements[slot] : null;
			FastText fastText = element as FastText;
			if (fastText == null)
			{
				continue;
			}
			bool useCachedLocalState = !initialDirty;
			bool useCachedRectAlignment = useCachedLocalState;
			bool useCachedGeometryMatrix = useCachedLocalState;
			if (!fastText.tryAppendSimplePrimaryBatchGeometry(mSimpleTextBatchWorkECS, mMeshRenderer, slot,
				slotColorColumn[slot], flags, canvasWorldToLocal, useCachedLocalState, useCachedRectAlignment, useCachedGeometryMatrix, out FastUIGeometryUpdateResult result, out _,
				out _, out _))
			{
				continue;
			}
			int resultFlags = TEXT_BATCH_PREPARED_BIT;
			if (result.mIndexCountChanged)
			{
				resultFlags |= TEXT_BATCH_INDEX_COUNT_CHANGED_BIT;
			}
			if (result.mVertexRangeChanged)
			{
				resultFlags |= TEXT_BATCH_VERTEX_RANGE_CHANGED_BIT;
			}
			if (result.mIndexCapacityChanged)
			{
				resultFlags |= TEXT_BATCH_INDEX_CAPACITY_CHANGED_BIT;
				mIndexCapacityDirty = true;
			}
			textBatchResultColumn[slot] = resultFlags;
		}
		if (mSimpleTextBatchWorkECS.Count <= 0)
		{
			return;
		}
		mMeshRenderer.rebuildSimpleTMPTextBatch(mSimpleTextBatchWorkECS, mPersistentTextGlyphECS, mSimpleTextBatchWorkECS.Count, false, out _, out _);
	}
	// Initial SimpleQuad Batch：Candidate在注册/语义变化时预计算到稳定VertexSlot ECS。
	// Partition只读取DirtyState+Candidate两列；GeometryRange合法性由最终Batch分配统一验证，失败仍完整回退原mDirtyVertexSlots。
	private bool prepareInitialSimpleQuadBatch(Matrix4x4 canvasWorldToLocal, FastUIRenderElement[] slotElements, ref FastUIFrameStats stats)
	{
		if (mInitialSimpleQuadBatchSlots == null || mInitialSimpleQuadRemainingDirtySlots == null || mInitialSimpleQuadCandidateECS == null || mMeshRenderer == null || mVertexSlotRuntimeECS == null || mDirtyVertexSlots == null)
		{
			return false;
		}
		int dirtyCount = mDirtyVertexSlots.Count;
		int minimumCount = Mathf.Max(mInitialSimpleQuadBatchMinCount, 1);
		if (dirtyCount < minimumCount)
		{
			return false;
		}
		mInitialSimpleQuadBatchSlots.Clear();
		mInitialSimpleQuadRemainingDirtySlots.Clear();
		mInitialSimpleQuadBatchSlots.EnsureCapacity(dirtyCount);
		mInitialSimpleQuadRemainingDirtySlots.EnsureCapacity(dirtyCount);
		var dirtySlots = mDirtyVertexSlots.getValueColumn();
		var dirtyStates = mVertexSlotRuntimeECS.getDirtyStateColumn();
		var candidateMetadata = mInitialSimpleQuadCandidateECS.getValueColumn();
		for (int i = 0; i < dirtyCount; ++i)
		{
			int slot = dirtySlots[i];
			bool simpleQuad = false;
			if ((uint)slot < (uint)mVertexSlotRuntimeECS.Count && (uint)slot < (uint)mInitialSimpleQuadCandidateECS.Count)
			{
				int dirtyState = dirtyStates[slot];
				if ((dirtyState & (INITIAL_DIRTY_BIT | TEXT_DIRTY_QUEUED_BIT)) == INITIAL_DIRTY_BIT && candidateMetadata[slot] != 0)
				{
					simpleQuad = (((FastUIDirtyFlags)(dirtyState & VERTEX_DIRTY_FLAGS_MASK)) & FastUIDirtyFlags.AllVertex) == FastUIDirtyFlags.AllVertex;
				}
			}
			if (simpleQuad)
			{
				mInitialSimpleQuadBatchSlots.Add(slot);
			}
			else
			{
				mInitialSimpleQuadRemainingDirtySlots.Add(slot);
			}
		}

		int quadCount = mInitialSimpleQuadBatchSlots.Count;
		if (quadCount < minimumCount)
		{
			mInitialSimpleQuadBatchSlots.Clear();
			mInitialSimpleQuadRemainingDirtySlots.Clear();
			return false;
		}
		if (!mMeshRenderer.tryAllocateInitialSimpleQuadBatch(mInitialSimpleQuadBatchSlots, quadCount, out int batchVertexStart, out int batchVertexCount))
		{
			mInitialSimpleQuadBatchSlots.Clear();
			mInitialSimpleQuadRemainingDirtySlots.Clear();
			return false;
		}

		var batchSlots = mInitialSimpleQuadBatchSlots.getValueColumn();
		var slotColors = mVertexSlotRuntimeECS.getColorColumn();
		var slotUVRects = mVertexSlotRuntimeECS.getUVRectColumn();
		var pendingLocalDeltaColumn = mVertexSlotRuntimeECS.getPendingLocalPositionDeltaColumn();
		var textBatchResultColumn = mVertexSlotRuntimeECS.getTextBatchResultFlagsColumn();
		var positionColumn = mMeshRenderer.getPositionECS().getPositionColumn();
		var colorColumn = mMeshRenderer.getColorECS().getColorColumn();
		var uvColumn = mMeshRenderer.getUVECS().getUVColumn();
		bool tmpLayout = mMeshRenderer.isTMPVertexLayoutEnabled();
		var tmpUV0Column = tmpLayout ? mMeshRenderer.getTMPUV0ECS().getUVColumn() : default;
		var tmpUV2Column = tmpLayout ? mMeshRenderer.getTMPUV2ECS().getUVColumn() : default;
		bool needPartialIndexDirty = !mFullDrawStructureDirty;
		for (int i = 0; i < quadCount; ++i)
		{
			int slot = batchSlots[i];
			FastUIRenderElement element = slotElements[slot];
			ensureVertexSlotDeltaTransform(slot, element, canvasWorldToLocal);
			// Initial SimpleQuad 与 Runtime SimpleQuad 复用同一份长期维护的 Rect/LocalTransform/GeometryMatrix 缓存。
			// 首次创建期间 Setter/RectTransform 回调已经同步缓存，避免这里再次逐元素读取 RectTransform 和重复 localToWorldMatrix。
			element.tryGetRuntimeSimpleQuadPositionGeometry(canvasWorldToLocal, true, out Vector3 bottomLeft, out Vector3 topLeft, out Vector3 topRight, out Vector3 bottomRight);
			int target = batchVertexStart + i * 4;
			positionColumn[target + 0] = bottomLeft;
			positionColumn[target + 1] = topLeft;
			positionColumn[target + 2] = topRight;
			positionColumn[target + 3] = bottomRight;
			Color32 color = slotColors[slot];
			colorColumn[target + 0] = color;
			colorColumn[target + 1] = color;
			colorColumn[target + 2] = color;
			colorColumn[target + 3] = color;
			Rect uvRect = slotUVRects[slot];
			float left = uvRect.xMin;
			float bottom = uvRect.yMin;
			float right = uvRect.xMax;
			float top = uvRect.yMax;
			uvColumn[target + 0] = new Vector2(left, bottom);
			uvColumn[target + 1] = new Vector2(left, top);
			uvColumn[target + 2] = new Vector2(right, top);
			uvColumn[target + 3] = new Vector2(right, bottom);
			// TMP布局下Image/RawImage的TMPUV0就是同一份UV Rect，直接在主循环写入，避免再次遍历全部Initial Quad。
			if (tmpLayout)
			{
				tmpUV0Column[target + 0] = new Vector4(left, bottom, 0.0f, 1.0f);
				tmpUV0Column[target + 1] = new Vector4(left, top, 0.0f, 1.0f);
				tmpUV0Column[target + 2] = new Vector4(right, top, 0.0f, 1.0f);
				tmpUV0Column[target + 3] = new Vector4(right, bottom, 0.0f, 1.0f);
				tmpUV2Column[target + 0] = default;
				tmpUV2Column[target + 1] = default;
				tmpUV2Column[target + 2] = default;
				tmpUV2Column[target + 3] = default;
			}
			if (needPartialIndexDirty)
			{
				addIndexDirtyRange(element.getRenderOrderIndex(), 1);
			}
			dirtyStates[slot] = 0;
			pendingLocalDeltaColumn[slot] = Vector3.zero;
			textBatchResultColumn[slot] = 0;
		}
		stats.mRebuiltVertexCount += batchVertexCount;
		mIndexCapacityDirty = true;
		return true;
	}
	// Runtime SimpleQuad Batch：
	// Initial Batch只覆盖“首次AllVertex+尚未分配Range”；运行期已稳定为4顶点的Simple Image/RawImage仍会逐元素
	// rebuildSimpleQuadPositionSlot -> ensureGeometryRange。这里用Candidate Metadata + GeometryRange Direct Column先筛出稳定4顶点Slot，
	// 批量直接写Position SOA，并只从DirtyState消费Transform/Geometry/PositionDelta。Color/UV若同帧存在，保留给原循环继续处理。
	// 这样不会改变Range分配、Index、Color/UV或Mask语义；任何不满足稳定Range/批量条件的Slot完整回退原路径。
	private bool prepareRuntimeSimpleQuadBatch(Matrix4x4 canvasWorldToLocal, FastUIRenderElement[] slotElements, Int_ECSList sourceDirtySlots, ref FastUIFrameStats stats)
	{
		if (sourceDirtySlots == null || mRuntimeSimpleQuadBatchSlots == null || mRuntimeSimpleQuadRemainingDirtySlots == null ||
			mInitialSimpleQuadCandidateECS == null || mVertexSlotRuntimeECS == null || mMeshRenderer == null)
		{
			return false;
		}
		int dirtyCount = sourceDirtySlots.Count;
		int minimumCount = Mathf.Max(mRuntimeSimpleQuadBatchMinCount, 1);
		if (dirtyCount < minimumCount)
		{
			return false;
		}
		mRuntimeSimpleQuadBatchSlots.Clear();
		mRuntimeSimpleQuadRemainingDirtySlots.Clear();
		mRuntimeSimpleQuadBatchSlots.EnsureCapacity(dirtyCount);
		mRuntimeSimpleQuadRemainingDirtySlots.EnsureCapacity(dirtyCount);
		var sourceSlots = sourceDirtySlots.getValueColumn();
		var dirtyStates = mVertexSlotRuntimeECS.getDirtyStateColumn();
		var candidateMetadata = mInitialSimpleQuadCandidateECS.getValueColumn();
		FastUIGeometryRangeData_ECSList geometryRanges = mMeshRenderer.getGeometryRangeECS();
		var vertexStarts = geometryRanges.getVertexStartColumn();
		var vertexCounts = geometryRanges.getVertexCountColumn();
		var vertexCapacities = geometryRanges.getVertexCapacityColumn();
		for (int i = 0; i < dirtyCount; ++i)
		{
			int slot = sourceSlots[i];
			bool runtimeSimpleQuad = false;
			if ((uint)slot < (uint)mVertexSlotRuntimeECS.Count && (uint)slot < (uint)mInitialSimpleQuadCandidateECS.Count && (uint)slot < (uint)geometryRanges.Count)
			{
				int dirtyState = dirtyStates[slot];
				FastUIDirtyFlags flags = (FastUIDirtyFlags)(dirtyState & VERTEX_DIRTY_FLAGS_MASK);
				runtimeSimpleQuad = (dirtyState & INITIAL_DIRTY_BIT) == 0 &&
					candidateMetadata[slot] != 0 &&
					(flags & (FastUIDirtyFlags.Transform | FastUIDirtyFlags.Geometry)) != 0 &&
					vertexStarts[slot] >= 0 &&
					vertexCounts[slot] == 4 &&
					vertexCapacities[slot] >= 4;
			}
			if (runtimeSimpleQuad)
			{
				mRuntimeSimpleQuadBatchSlots.Add(slot);
			}
			else
			{
				mRuntimeSimpleQuadRemainingDirtySlots.Add(slot);
			}
		}
		int candidateCount = mRuntimeSimpleQuadBatchSlots.Count;
		if (candidateCount < minimumCount)
		{
			mRuntimeSimpleQuadBatchSlots.Clear();
			mRuntimeSimpleQuadRemainingDirtySlots.Clear();
			return false;
		}
		var batchSlots = mRuntimeSimpleQuadBatchSlots.getValueColumn();
		var pendingLocalDeltaColumn = mVertexSlotRuntimeECS.getPendingLocalPositionDeltaColumn();
		var textBatchResultColumn = mVertexSlotRuntimeECS.getTextBatchResultFlagsColumn();
		var deltaTransformModeColumn = mVertexSlotRuntimeECS.getDeltaTransformModeColumn();
		var deltaRow0Column = mVertexSlotRuntimeECS.getDeltaRow0Column();
		var deltaRow1Column = mVertexSlotRuntimeECS.getDeltaRow1Column();
		var deltaRow2Column = mVertexSlotRuntimeECS.getDeltaRow2Column();
		var positionColumn = mMeshRenderer.getPositionECS().getPositionColumn();
		Transform canvasTransform = transform;
		Transform lastDeltaParent = null;
		bool lastDeltaParentValid = false;
		int lastDeltaMode = DELTA_TRANSFORM_DIRECT;
		Vector3 lastDeltaRow0 = Vector3.right;
		Vector3 lastDeltaRow1 = Vector3.up;
		Vector3 lastDeltaRow2 = Vector3.forward;
		int positionDirtyVertexStart = -1;
		int positionDirtyVertexEnd = -1;
		int completedCount = 0;
		for (int i = 0; i < candidateCount; ++i)
		{
			int slot = batchSlots[i];
			FastUIRenderElement element = slotElements != null && (uint)slot < (uint)slotElements.Length ? slotElements[slot] : null;
			if (element == null)
			{
				mRuntimeSimpleQuadRemainingDirtySlots.Add(slot);
				continue;
			}
			int dirtyState = dirtyStates[slot];
			FastUIDirtyFlags flags = (FastUIDirtyFlags)(dirtyState & VERTEX_DIRTY_FLAGS_MASK);
			if ((flags & FastUIDirtyFlags.Transform) != 0)
			{
				Transform parent = element.getRectTransform().parent;
				if (!lastDeltaParentValid || parent != lastDeltaParent)
				{
					lastDeltaParent = parent;
					lastDeltaParentValid = true;
					if (parent == canvasTransform || (parent != null && parent.parent == canvasTransform && parent.localRotation == Quaternion.identity && parent.localScale == Vector3.one))
					{
						lastDeltaMode = DELTA_TRANSFORM_DIRECT;
						lastDeltaRow0 = Vector3.right;
						lastDeltaRow1 = Vector3.up;
						lastDeltaRow2 = Vector3.forward;
					}
					else
					{
						Matrix4x4 parentToCanvas = parent == null ? canvasWorldToLocal : canvasWorldToLocal * parent.localToWorldMatrix;
						lastDeltaMode = DELTA_TRANSFORM_MATRIX;
						lastDeltaRow0 = new Vector3(parentToCanvas.m00, parentToCanvas.m01, parentToCanvas.m02);
						lastDeltaRow1 = new Vector3(parentToCanvas.m10, parentToCanvas.m11, parentToCanvas.m12);
						lastDeltaRow2 = new Vector3(parentToCanvas.m20, parentToCanvas.m21, parentToCanvas.m22);
					}
				}
				deltaTransformModeColumn[slot] = lastDeltaMode;
				deltaRow0Column[slot] = lastDeltaRow0;
				deltaRow1Column[slot] = lastDeltaRow1;
				deltaRow2Column[slot] = lastDeltaRow2;
			}
			if (!element.tryGetRuntimeSimpleQuadPositionGeometry(canvasWorldToLocal, true, out Vector3 bottomLeft, out Vector3 topLeft, out Vector3 topRight, out Vector3 bottomRight))
			{
				mRuntimeSimpleQuadRemainingDirtySlots.Add(slot);

				continue;
			}
			int vertexStart = vertexStarts[slot];
			positionColumn[vertexStart + 0] = bottomLeft;
			positionColumn[vertexStart + 1] = topLeft;
			positionColumn[vertexStart + 2] = topRight;
			positionColumn[vertexStart + 3] = bottomRight;
			if (positionDirtyVertexStart < 0)
			{
				positionDirtyVertexStart = vertexStart;
				positionDirtyVertexEnd = vertexStart + 4;
			}
			else if (vertexStart == positionDirtyVertexEnd)
			{
				positionDirtyVertexEnd += 4;
			}
			else
			{
				mMeshRenderer.markPositionVertexRangeDirty(positionDirtyVertexStart, positionDirtyVertexEnd - positionDirtyVertexStart);

				positionDirtyVertexStart = vertexStart;
				positionDirtyVertexEnd = vertexStart + 4;
			}

			FastUIDirtyFlags remainingFlags = flags & ~(FastUIDirtyFlags.Transform | FastUIDirtyFlags.Geometry | FastUIDirtyFlags.PositionDelta);
			pendingLocalDeltaColumn[slot] = Vector3.zero;
			textBatchResultColumn[slot] = 0;
			if (remainingFlags == FastUIDirtyFlags.None)
			{
				dirtyStates[slot] = 0;
				stats.mRebuiltVertexCount += 4;
			}
			else
			{
				dirtyStates[slot] = (int)remainingFlags | VERTEX_DIRTY_QUEUED_BIT;
				mRuntimeSimpleQuadRemainingDirtySlots.Add(slot);
			}
			++completedCount;
		}
		if (positionDirtyVertexStart >= 0)
		{
			mMeshRenderer.markPositionVertexRangeDirty(positionDirtyVertexStart, positionDirtyVertexEnd - positionDirtyVertexStart);
		}
		return true;
	}
	// 统一消费Dirty Slot。Position FastPath、PackedQueue和普通Stream更新都从这里分流。
	private void processDirtyElements(ref FastUIFrameStats stats)
	{
		// 防御销毁/DomainUnload阶段的重入。正常Frame路径在runFrameUpdate入口已经保证该容器有效。
		if (mDirtyVertexSlots == null || mDirtyVertexSlots.Count <= 0)
		{
			return;
		}
		// 生产路径固定使用阈值自动选择：大批量纯平移进入PackedQueue，小规模更新走Direct。
		if (mPendingLeafPositionDeltaCount >= Mathf.Max(mPositionDeltaPackedQueueMinCount, 1))
		{
			processDirtyElementsPacked(ref stats);
		}
		else
		{
			processDirtyElementsDirect(ref stats);
		}
	}
	// PackedQueue只在批量纯平移足够多时启用，小规模更新继续走Direct以避免固定开销。
	private void processDirtyElementsPacked(ref FastUIFrameStats stats)
	{
		int count = mDirtyVertexSlots.Count;
		if (count <= 0)
		{
			return;
		}
		FastUIRenderElement[] slotElements = mRenderRegistry.getVertexSlotOwners();
		var dirtyStateColumn = mVertexSlotRuntimeECS.getDirtyStateColumn();
		var pendingLocalDeltaColumn = mVertexSlotRuntimeECS.getPendingLocalPositionDeltaColumn();
		var slotColorColumn = mVertexSlotRuntimeECS.getColorColumn();
		var slotUVRectColumn = mVertexSlotRuntimeECS.getUVRectColumn();
		var deltaTransformModeColumn = mVertexSlotRuntimeECS.getDeltaTransformModeColumn();
		var deltaRow0Column = mVertexSlotRuntimeECS.getDeltaRow0Column();
		var deltaRow1Column = mVertexSlotRuntimeECS.getDeltaRow1Column();
		var deltaRow2Column = mVertexSlotRuntimeECS.getDeltaRow2Column();
		var requiresColorGeometryColumn = mVertexSlotRuntimeECS.getRequiresGeometryRebuildForColorColumn();
		var textBatchResultColumn = mVertexSlotRuntimeECS.getTextBatchResultFlagsColumn();
		var geometryVertexCountColumn = mMeshRenderer.getGeometryRangeECS().getVertexCountColumn();
		Matrix4x4 canvasWorldToLocal = getGeometryWorldToLocalMatrix();
		int positionDirtyRangeStartSlot = -1;
		int positionDirtyRangeLastSlot = -2;
		ensureLeafPositionDeltaQueueCapacity(Mathf.Max(mPendingLeafPositionDeltaCount, 1));
		var deltaSlotColumn = mLeafPositionDeltaECS.getVertexSlotColumn();
		var deltaColumn = mLeafPositionDeltaECS.getDeltaColumn();
		int deltaCount = 0;
		using (BUILD_VERTICES_MARKER.Auto())
		{
			prepareInitialSimpleTextRangeBatch(slotElements);
			prepareSimpleTextCanvasBatch(canvasWorldToLocal, slotElements);
			bool initialQuadPartitioned = prepareInitialSimpleQuadBatch(canvasWorldToLocal, slotElements, ref stats);
			Int_ECSList dirtyWorkSlots = initialQuadPartitioned ? mInitialSimpleQuadRemainingDirtySlots : mDirtyVertexSlots;
			bool runtimeQuadPartitioned = !initialQuadPartitioned && prepareRuntimeSimpleQuadBatch(canvasWorldToLocal, slotElements, dirtyWorkSlots, ref stats);
			if (runtimeQuadPartitioned)
			{
				dirtyWorkSlots = mRuntimeSimpleQuadRemainingDirtySlots;
			}
			int workCount = dirtyWorkSlots.Count;
			var dirtyVertexSlotColumn = dirtyWorkSlots.getValueColumn();

			for (int i = 0; i < workCount; ++i)
			{
				int slot = dirtyVertexSlotColumn[i];
				if (slot < 0 || slot >= mVertexSlotRuntimeECS.Count)
				{
					continue;
				}
				FastUIRenderElement element = slotElements != null && slot < slotElements.Length ? slotElements[slot] : null;
				if (element == null)
				{
					dirtyStateColumn[slot] = 0;
					pendingLocalDeltaColumn[slot] = Vector3.zero;
					textBatchResultColumn[slot] = 0;
					continue;
				}
				int dirtyState = dirtyStateColumn[slot];
				FastUIDirtyFlags flags = (FastUIDirtyFlags)(dirtyState & VERTEX_DIRTY_FLAGS_MASK);
				if (flags == FastUIDirtyFlags.None)
				{
					dirtyStateColumn[slot] = 0;
					pendingLocalDeltaColumn[slot] = Vector3.zero;
					textBatchResultColumn[slot] = 0;
					continue;
				}
				bool fullPositionDirty = (flags & (FastUIDirtyFlags.Transform | FastUIDirtyFlags.Geometry)) != 0;
				bool positionDeltaDirty = !fullPositionDirty && (flags & FastUIDirtyFlags.PositionDelta) != 0;
				bool positionUpdated = false;
				bool colorDirty = (flags & FastUIDirtyFlags.Color) != 0;
				bool uvDirty = (flags & FastUIDirtyFlags.AnyUV) != 0;
				bool uv0OnlyDirty = (flags & FastUIDirtyFlags.UV0Only) != 0 && (flags & FastUIDirtyFlags.UV) == 0;
				if (fullPositionDirty)
				{
					int textBatchResult = textBatchResultColumn[slot];
					if ((textBatchResult & TEXT_BATCH_PREPARED_BIT) != 0)
					{
						appendPositionDirtySlot(slot, ref positionDirtyRangeStartSlot, ref positionDirtyRangeLastSlot);
						if ((textBatchResult & TEXT_BATCH_INDEX_CHANGE_MASK) != 0)
						{
							addIndexDirtyRange(element.getRenderOrderIndex(), 1);
						}
					}
					else
					{
						FastUIGeometryUpdateResult geometryResult = rebuildElementGeometry(element, slot, canvasWorldToLocal, flags);
						appendPositionDirtySlot(slot, ref positionDirtyRangeStartSlot, ref positionDirtyRangeLastSlot);
						if (geometryResult.mIndexCapacityChanged)
						{
							mIndexCapacityDirty = true;
						}
						if (geometryResult.mIndexCountChanged || geometryResult.mVertexRangeChanged || geometryResult.mIndexCapacityChanged)
						{
							addIndexDirtyRange(element.getRenderOrderIndex(), 1);
						}
					}
					positionUpdated = true;
				}
				else if (positionDeltaDirty)
				{
					Vector3 localDelta = pendingLocalDeltaColumn[slot];
					if (localDelta != Vector3.zero)
					{
						ensureVertexSlotDeltaTransform(slot, element, canvasWorldToLocal);
						Vector3 canvasDelta = transformLocalDeltaFromECS(deltaTransformModeColumn[slot], deltaRow0Column[slot], deltaRow1Column[slot], deltaRow2Column[slot], localDelta);
						deltaSlotColumn[deltaCount] = slot;
						deltaColumn[deltaCount] = canvasDelta;
						++deltaCount;
						appendPositionDirtySlot(slot, ref positionDirtyRangeStartSlot, ref positionDirtyRangeLastSlot);
						positionUpdated = true;
					}
				}
				if (colorDirty && !fullPositionDirty)
				{
					if (requiresColorGeometryColumn[slot] != 0)
					{
						rebuildElementColorGeometry(element, slot, canvasWorldToLocal);
					}
					else
					{
						mMeshRenderer.setColorSlot(slot, slotColorColumn[slot]);
						mMeshRenderer.markColorSlotDirty(slot);
					}
				}
				if (uvDirty && !fullPositionDirty)
				{
					if (element is FastText fastText && uv0OnlyDirty &&
						fastText.tryRefreshSimplePrimaryUV0Direct(mMeshRenderer, slot, out _, out _))
					{
						// 顶点UV已由FastText直接写入。
					}
					else if (element is FastText)
					{
						// Text UV专用路径如果因同帧其它状态变化失效，回退完整Text Geometry，绝不套用Quad UV。
						FastUIGeometryUpdateResult textFallbackResult = rebuildElementGeometry(element, slot, canvasWorldToLocal, flags | FastUIDirtyFlags.Geometry);
						appendPositionDirtySlot(slot, ref positionDirtyRangeStartSlot, ref positionDirtyRangeLastSlot);
						if (textFallbackResult.mIndexCapacityChanged)
						{
							mIndexCapacityDirty = true;
						}
						if (textFallbackResult.mIndexCountChanged || textFallbackResult.mVertexRangeChanged || textFallbackResult.mIndexCapacityChanged)
						{
							addIndexDirtyRange(element.getRenderOrderIndex(), 1);
						}
						positionUpdated = true;
					}
					else
					{
						mMeshRenderer.setUVSlot(slot, slotUVRectColumn[slot]);
						if (uv0OnlyDirty)
						{
							mMeshRenderer.markUV0SlotDirty(slot);
						}
						else
						{
							mMeshRenderer.markUVSlotDirty(slot);
						}
					}
				}

				if (positionUpdated || colorDirty || uvDirty)
				{
					stats.mRebuiltVertexCount += geometryVertexCountColumn[slot];
				}
				dirtyStateColumn[slot] = 0;
				pendingLocalDeltaColumn[slot] = Vector3.zero;
				textBatchResultColumn[slot] = 0;
			}

			if (deltaCount > 0)
			{
				mMeshRenderer.applyPositionDeltaQueue(mLeafPositionDeltaECS, deltaCount);
			}

			flushPositionDirtySlotRange(ref positionDirtyRangeStartSlot, ref positionDirtyRangeLastSlot);
		}
		mDirtyVertexSlots.Clear();
		mDirtyTextVertexSlots.Clear();
		mPendingLeafPositionDeltaCount = 0;
	}
	private void processDirtyElementsDirect(ref FastUIFrameStats stats)
	{
		int count = mDirtyVertexSlots.Count;
		if (count <= 0)
		{
			return;
		}
		FastUIRenderElement[] slotElements = mRenderRegistry.getVertexSlotOwners();
		var dirtyStateColumn = mVertexSlotRuntimeECS.getDirtyStateColumn();
		var pendingLocalDeltaColumn = mVertexSlotRuntimeECS.getPendingLocalPositionDeltaColumn();
		var slotColorColumn = mVertexSlotRuntimeECS.getColorColumn();
		var slotUVRectColumn = mVertexSlotRuntimeECS.getUVRectColumn();
		var deltaTransformModeColumn = mVertexSlotRuntimeECS.getDeltaTransformModeColumn();
		var deltaRow0Column = mVertexSlotRuntimeECS.getDeltaRow0Column();
		var deltaRow1Column = mVertexSlotRuntimeECS.getDeltaRow1Column();
		var deltaRow2Column = mVertexSlotRuntimeECS.getDeltaRow2Column();
		var requiresColorGeometryColumn = mVertexSlotRuntimeECS.getRequiresGeometryRebuildForColorColumn();
		var textBatchResultColumn = mVertexSlotRuntimeECS.getTextBatchResultFlagsColumn();
		var geometryVertexCountColumn = mMeshRenderer.getGeometryRangeECS().getVertexCountColumn();
		Matrix4x4 canvasWorldToLocal = getGeometryWorldToLocalMatrix();
		int positionDirtyRangeStartSlot = -1;
		int positionDirtyRangeLastSlot = -2;
		using (BUILD_VERTICES_MARKER.Auto())
		{
			prepareInitialSimpleTextRangeBatch(slotElements);
			prepareSimpleTextCanvasBatch(canvasWorldToLocal, slotElements);
			bool initialQuadPartitioned = prepareInitialSimpleQuadBatch(canvasWorldToLocal, slotElements, ref stats);
			Int_ECSList dirtyWorkSlots = initialQuadPartitioned ? mInitialSimpleQuadRemainingDirtySlots : mDirtyVertexSlots;
			bool runtimeQuadPartitioned = !initialQuadPartitioned && prepareRuntimeSimpleQuadBatch(canvasWorldToLocal, slotElements, dirtyWorkSlots, ref stats);
			if (runtimeQuadPartitioned)
			{
				dirtyWorkSlots = mRuntimeSimpleQuadRemainingDirtySlots;
			}
			int workCount = dirtyWorkSlots.Count;
			var dirtyVertexSlotColumn = dirtyWorkSlots.getValueColumn();

			for (int i = 0; i < workCount; ++i)
			{
				int slot = dirtyVertexSlotColumn[i];
				if (slot < 0 || slot >= mVertexSlotRuntimeECS.Count)
				{
					continue;
				}
				FastUIRenderElement element = slotElements != null && slot < slotElements.Length ? slotElements[slot] : null;
				if (element == null)
				{
					dirtyStateColumn[slot] = 0;
					pendingLocalDeltaColumn[slot] = Vector3.zero;
					textBatchResultColumn[slot] = 0;
					continue;
				}
				int dirtyState = dirtyStateColumn[slot];
				FastUIDirtyFlags flags = (FastUIDirtyFlags)(dirtyState & VERTEX_DIRTY_FLAGS_MASK);
				if (flags == FastUIDirtyFlags.None)
				{
					dirtyStateColumn[slot] = 0;
					pendingLocalDeltaColumn[slot] = Vector3.zero;
					textBatchResultColumn[slot] = 0;
					continue;
				}
				bool fullPositionDirty = (flags & (FastUIDirtyFlags.Transform | FastUIDirtyFlags.Geometry)) != 0;
				bool positionDeltaDirty = !fullPositionDirty && (flags & FastUIDirtyFlags.PositionDelta) != 0;
				bool positionUpdated = false;
				bool colorDirty = (flags & FastUIDirtyFlags.Color) != 0;
				bool uvDirty = (flags & FastUIDirtyFlags.AnyUV) != 0;
				bool uv0OnlyDirty = (flags & FastUIDirtyFlags.UV0Only) != 0 && (flags & FastUIDirtyFlags.UV) == 0;
				if (fullPositionDirty)
				{
					int textBatchResult = textBatchResultColumn[slot];
					if ((textBatchResult & TEXT_BATCH_PREPARED_BIT) != 0)
					{
						appendPositionDirtySlot(slot, ref positionDirtyRangeStartSlot, ref positionDirtyRangeLastSlot);
						if ((textBatchResult & TEXT_BATCH_INDEX_CHANGE_MASK) != 0)
						{
							addIndexDirtyRange(element.getRenderOrderIndex(), 1);
						}
					}
					else
					{
						FastUIGeometryUpdateResult geometryResult = rebuildElementGeometry(element, slot, canvasWorldToLocal, flags);
						appendPositionDirtySlot(slot, ref positionDirtyRangeStartSlot, ref positionDirtyRangeLastSlot);
						if (geometryResult.mIndexCapacityChanged)
						{
							mIndexCapacityDirty = true;
						}
						if (geometryResult.mIndexCountChanged || geometryResult.mVertexRangeChanged || geometryResult.mIndexCapacityChanged)
						{
							addIndexDirtyRange(element.getRenderOrderIndex(), 1);
						}
					}
					positionUpdated = true;
				}
				else if (positionDeltaDirty)
				{
					Vector3 localDelta = pendingLocalDeltaColumn[slot];
					if (localDelta != Vector3.zero)
					{
						ensureVertexSlotDeltaTransform(slot, element, canvasWorldToLocal);
						Vector3 canvasDelta = transformLocalDeltaFromECS(deltaTransformModeColumn[slot], deltaRow0Column[slot], deltaRow1Column[slot], deltaRow2Column[slot], localDelta);
						mMeshRenderer.offsetPositionSlotRange(slot, 1, canvasDelta);
						appendPositionDirtySlot(slot, ref positionDirtyRangeStartSlot, ref positionDirtyRangeLastSlot);
						positionUpdated = true;
					}
				}
				if (colorDirty && !fullPositionDirty)
				{
					if (requiresColorGeometryColumn[slot] != 0)
					{
						rebuildElementColorGeometry(element, slot, canvasWorldToLocal);
					}
					else
					{
						mMeshRenderer.setColorSlot(slot, slotColorColumn[slot]);
						mMeshRenderer.markColorSlotDirty(slot);
					}
				}
				if (uvDirty && !fullPositionDirty)
				{
					if (element is FastText fastText && uv0OnlyDirty && fastText.tryRefreshSimplePrimaryUV0Direct(mMeshRenderer, slot, out _, out _))
					{
						// 顶点UV已由FastText直接写入。
					}
					else if (element is FastText)
					{
						// Text UV专用路径如果因同帧其它状态变化失效，回退完整Text Geometry，绝不套用Quad UV。
						FastUIGeometryUpdateResult textFallbackResult = rebuildElementGeometry(element, slot, canvasWorldToLocal, flags | FastUIDirtyFlags.Geometry);
						appendPositionDirtySlot(slot, ref positionDirtyRangeStartSlot, ref positionDirtyRangeLastSlot);
						if (textFallbackResult.mIndexCapacityChanged)
						{
							mIndexCapacityDirty = true;
						}
						if (textFallbackResult.mIndexCountChanged || textFallbackResult.mVertexRangeChanged || textFallbackResult.mIndexCapacityChanged)
						{
							addIndexDirtyRange(element.getRenderOrderIndex(), 1);
						}
						positionUpdated = true;
					}
					else
					{
						mMeshRenderer.setUVSlot(slot, slotUVRectColumn[slot]);
						if (uv0OnlyDirty)
						{
							mMeshRenderer.markUV0SlotDirty(slot);
						}
						else
						{
							mMeshRenderer.markUVSlotDirty(slot);
						}
					}
				}

				if (positionUpdated || colorDirty || uvDirty)
				{
					stats.mRebuiltVertexCount += geometryVertexCountColumn[slot];
				}
				dirtyStateColumn[slot] = 0;
				pendingLocalDeltaColumn[slot] = Vector3.zero;
				textBatchResultColumn[slot] = 0;
			}
			flushPositionDirtySlotRange(ref positionDirtyRangeStartSlot, ref positionDirtyRangeLastSlot);
		}
		mDirtyVertexSlots.Clear();
		mDirtyTextVertexSlots.Clear();
		mPendingLeafPositionDeltaCount = 0;
	}
	// Geometry/Transform Dirty统一从组件重新生成完整几何。
	// Range搬迁时Color也必须重新上传；Quad数量变化会由上层升级为Full Index布局重建。
	private FastUIGeometryUpdateResult rebuildElementGeometry(FastUIRenderElement element, int slot, Matrix4x4 canvasWorldToLocal, FastUIDirtyFlags flags)
	{
		if ((flags & FastUIDirtyFlags.Transform) != 0)
		{
			syncVertexSlotDeltaTransform(slot, element, canvasWorldToLocal);
		}
		int previousSimpleTextActiveVertexCount = element is FastText ? mMeshRenderer.getSimpleTextActiveVertexCount(slot) : 0;
		if (element is FastText fastText &&
			fastText.tryRebuildSimplePrimaryGeometryDirect(mMeshRenderer, slot, canvasWorldToLocal, mVertexSlotRuntimeECS.getColorColumn()[slot],
				out FastUIGeometryUpdateResult textResult, out int textGlyphCount))
		{
			mMeshRenderer.markUVSlotDirty(slot);
			// Stable Glyph Slot增长到此前未使用的Slot时需要初始化Color；同GlyphCount换字符仍不重复上传Color Stream。
			if ((flags & FastUIDirtyFlags.Color) != 0 || textGlyphCount * 4 > previousSimpleTextActiveVertexCount ||
				textResult.mVertexRangeChanged || textResult.mIndexCountChanged || textResult.mIndexCapacityChanged)
			{
				mMeshRenderer.markColorSlotDirty(slot);
			}
			return textResult;
		}
		int currentVertexCount = mMeshRenderer.getGeometryVertexCount(slot);
		if ((currentVertexCount == 0 || currentVertexCount == 4) &&
			(flags & (FastUIDirtyFlags.Transform | FastUIDirtyFlags.Geometry)) != 0 &&
			element.tryGetSimpleQuadPositionGeometry(canvasWorldToLocal, out Vector3 bottomLeft, out Vector3 topLeft, out Vector3 topRight, out Vector3 bottomRight))
		{
			FastUIGeometryUpdateResult quadResult = mMeshRenderer.rebuildSimpleQuadPositionSlot(slot, bottomLeft, topLeft, topRight, bottomRight);
			bool rangeChanged = quadResult.mVertexRangeChanged || quadResult.mIndexCountChanged || quadResult.mIndexCapacityChanged;
			if ((flags & FastUIDirtyFlags.Color) != 0 || rangeChanged)
			{
				mMeshRenderer.setColorSlot(slot, mVertexSlotRuntimeECS.getColorColumn()[slot]);
				mMeshRenderer.markColorSlotDirty(slot);
			}
			if (((flags & FastUIDirtyFlags.AnyUV) != 0 || rangeChanged) && element.tryGetSimpleUVRect(out Rect simpleUVRect))
			{
				mMeshRenderer.setUVSlot(slot, simpleUVRect);
				bool uv0OnlyDirty = !rangeChanged && (flags & FastUIDirtyFlags.UV0Only) != 0 && (flags & FastUIDirtyFlags.UV) == 0;
				if (uv0OnlyDirty)
				{
					mMeshRenderer.markUV0SlotDirty(slot);
				}
				else
				{
					mMeshRenderer.markUVSlotDirty(slot);
				}
			}
			return quadResult;
		}
		if (currentVertexCount > 0 && element is FastImage slicedImage &&
			slicedImage.tryRebuildRuntimeSlicedGeometryDirect(mMeshRenderer, slot, canvasWorldToLocal, mVertexSlotRuntimeECS.getColorColumn()[slot],
				out FastUIGeometryUpdateResult slicedResult))
		{
			bool slicedTransformRebuildsGeometryUV = (flags & FastUIDirtyFlags.Transform) != 0 && element.requiresGeometryRebuildForMatrixTransform();
			if ((flags & (FastUIDirtyFlags.Geometry | FastUIDirtyFlags.AnyUV)) != 0 || slicedResult.mVertexRangeChanged || slicedTransformRebuildsGeometryUV)
			{
				mMeshRenderer.markUVSlotDirty(slot);
			}
			bool slicedGeometryColorChanged = mVertexSlotRuntimeECS.getGeometryControlsVertexColorColumn()[slot] != 0 && (flags & FastUIDirtyFlags.Geometry) != 0;
			if ((flags & FastUIDirtyFlags.Color) != 0 || slicedResult.mVertexRangeChanged || slicedResult.mIndexCountChanged || slicedGeometryColorChanged)
			{
				mMeshRenderer.markColorSlotDirty(slot);
			}
			return slicedResult;
		}
		element.buildGeometry(mGeometryBuilder, canvasWorldToLocal);
		FastUIGeometryUpdateResult result = mMeshRenderer.rebuildGeometrySlot(slot, mGeometryBuilder, mVertexSlotRuntimeECS.getColorColumn()[slot]);
		bool transformRebuildsGeometryUV = (flags & FastUIDirtyFlags.Transform) != 0 && element.requiresGeometryRebuildForMatrixTransform();
		if ((flags & (FastUIDirtyFlags.Geometry | FastUIDirtyFlags.AnyUV)) != 0 || result.mVertexRangeChanged || transformRebuildsGeometryUV)
		{
			mMeshRenderer.markUVSlotDirty(slot);
		}
		bool geometryColorChanged = mVertexSlotRuntimeECS.getGeometryControlsVertexColorColumn()[slot] != 0 && (flags & FastUIDirtyFlags.Geometry) != 0;
		if ((flags & FastUIDirtyFlags.Color) != 0 || result.mVertexRangeChanged || result.mIndexCountChanged || geometryColorChanged)
		{
			mMeshRenderer.markColorSlotDirty(slot);
		}
		return result;
	}
	private void rebuildElementColorGeometry(FastUIRenderElement element, int slot, Matrix4x4 canvasWorldToLocal)
	{
		int oldVertexStart = mMeshRenderer.getGeometryVertexStart(slot);
		int oldVertexCount = mMeshRenderer.getGeometryVertexCount(slot);
		element.buildGeometry(mGeometryBuilder, canvasWorldToLocal);
		FastUIGeometryUpdateResult result = mMeshRenderer.rebuildGeometrySlot(slot, mGeometryBuilder, mVertexSlotRuntimeECS.getColorColumn()[slot]);
		mMeshRenderer.markColorSlotDirty(slot);

		if (result.mIndexCapacityChanged)
		{
			mIndexCapacityDirty = true;
		}
		if (result.mVertexRangeChanged ||
			result.mIndexCountChanged ||
			result.mIndexCapacityChanged ||
			oldVertexStart != result.mVertexStart ||
			oldVertexCount != result.mVertexCount)
		{
			mMeshRenderer.markPositionSlotDirty(slot);
			mMeshRenderer.markUVSlotDirty(slot);
			addIndexDirtyRange(element.getRenderOrderIndex(), 1);
		}
	}
	private void ensureLeafPositionDeltaQueueCapacity(int requiredCount)
	{
		if (requiredCount <= 0)
		{
			return;
		}
		int targetCount = Mathf.NextPowerOfTwo(Mathf.Max(requiredCount, 4));
		mLeafPositionDeltaECS ??= new FastUIPositionDeltaData_ECSList(targetCount);
		if (mLeafPositionDeltaStorageCount >= requiredCount)
		{
			return;
		}
		while (mLeafPositionDeltaStorageCount < targetCount)
		{
			mLeafPositionDeltaECS.Add(new FastUIPositionDeltaData
			{
				mVertexSlot = 0,
				mDelta = Vector3.zero,
			});
			++mLeafPositionDeltaStorageCount;
		}
	}
	private void appendPositionDirtySlot(int slot, ref int rangeStartSlot, ref int rangeLastSlot)
	{
		if (rangeStartSlot < 0)
		{
			rangeStartSlot = slot;
			rangeLastSlot = slot;
			return;
		}
		if (slot == rangeLastSlot + 1)
		{
			rangeLastSlot = slot;
			return;
		}
		flushPositionDirtySlotRange(ref rangeStartSlot, ref rangeLastSlot);
		rangeStartSlot = slot;
		rangeLastSlot = slot;
	}
	private void flushPositionDirtySlotRange(ref int rangeStartSlot, ref int rangeLastSlot)
	{
		if (rangeStartSlot < 0)
		{
			return;
		}
		mMeshRenderer.markPositionSlotRangeDirty(rangeStartSlot, rangeLastSlot - rangeStartSlot + 1);

		rangeStartSlot = -1;
		rangeLastSlot = -2;
	}
	private Matrix4x4 getGeometryWorldToLocalMatrix()
	{
		Matrix4x4 matrix = transform.worldToLocalMatrix;
		if (mRenderOriginOffset != Vector3.zero)
		{
			matrix.m03 -= mRenderOriginOffset.x;
			matrix.m13 -= mRenderOriginOffset.y;
			matrix.m23 -= mRenderOriginOffset.z;
		}
		return matrix;
	}
	private void markRenderRangeDirty(int renderStart, int renderCount, FastUIDirtyFlags flags)
	{
		int renderEnd = Mathf.Min(renderStart + renderCount, mRenderRegistry.Count);
		if ((flags & FastUIDirtyFlags.Transform) != 0)
		{
			invalidateGeometryMatrixCacheVersion();
		}
		for (int i = Mathf.Max(renderStart, 0); i < renderEnd; ++i)
		{
			markElementDirty(mRenderRegistry[i], flags);
		}
	}
	private void applyPositionDelta(Transform root, int renderStart, int renderCount, Vector3 localDelta)
	{
		Vector3 canvasDelta = convertLocalPositionDeltaToCanvas(root, localDelta);
		queueGeometryMatrixPositionTranslation(renderStart, renderCount, canvasDelta);
		if (renderCount >= BULK_POSITION_SLOT_CACHE_MIN_RENDER_COUNT)
		{
			if (!tryApplyBulkPositionSlotPlan(root, renderStart, renderCount, canvasDelta, out BulkPositionSlotPlan capturePlan))
			{
				offsetRenderRange(renderStart, renderCount, canvasDelta, capturePlan);
				if (capturePlan != null)
				{
					buildBulkPositionComplementPlan(capturePlan);
				}
			}
		}
		else
		{
			offsetRenderRange(renderStart, renderCount, canvasDelta);
		}
		mClipSystem?.markTranslatedRange(renderStart, renderCount, canvasDelta);
	}
	private bool tryApplyBulkPositionSlotPlan(Transform root, int renderStart, int renderCount, Vector3 canvasDelta, out BulkPositionSlotPlan capturePlan)
	{
		capturePlan = null;
		if (root == null || canvasDelta == Vector3.zero || mMeshRenderer == null)
		{
			return false;
		}
		if (!mBulkPositionSlotPlans.TryGetValue(root, out BulkPositionSlotPlan plan))
		{
			plan = new BulkPositionSlotPlan();
			mBulkPositionSlotPlans.Add(root, plan);
		}
		if (plan.mRenderStart != renderStart || plan.mRenderCount != renderCount || plan.mSlotRanges.Count == 0)
		{
			plan.reset(renderStart, renderCount);
			capturePlan = plan;
			return false;
		}

		if (tryApplyRenderOriginShift(plan, canvasDelta))
		{
			return true;
		}
		mMeshRenderer.offsetPositionSlotRanges(plan.mSlotRanges, plan.mSlotRanges.Count, canvasDelta, out int dirtyVertexStart, out int dirtyVertexEnd);
		if (dirtyVertexEnd > dirtyVertexStart)
		{
			mMeshRenderer.markPositionVertexRangeDirty(dirtyVertexStart, dirtyVertexEnd - dirtyVertexStart);
		}
		else
		{
			var starts = plan.mSlotRanges.getStartColumn();
			var ends = plan.mSlotRanges.getEndColumn();
			for (int i = 0; i < plan.mSlotRanges.Count; ++i)
			{
				mMeshRenderer.markPositionSlotRangeDirty(starts[i], ends[i] - starts[i]);
			}
		}
		return true;
	}
	private void buildBulkPositionComplementPlan(BulkPositionSlotPlan plan)
	{
		if (plan == null || mMeshRenderer == null)
		{
			return;
		}
		plan.mComplementSlotRanges.Clear();
		plan.mComplementVertexCount = 0;
		FastUIRenderElement[] owners = mRenderRegistry.getVertexSlotOwners();
		int slotSpan = mMeshRenderer.getSlotSpan();
		int affectedStart = Mathf.Max(plan.mRenderStart, 0);
		int affectedEnd = Mathf.Min(plan.mRenderStart + plan.mRenderCount, mRenderRegistry.Count);
		int rangeStart = -1;
		int lastSlot = -2;
		for (int slot = 0; slot < slotSpan; ++slot)
		{
			int vertexCount = mMeshRenderer.getGeometryVertexCount(slot);
			if (vertexCount <= 0)
			{
				continue;
			}
			FastUIRenderElement owner = owners != null && slot < owners.Length ? owners[slot] : null;
			int renderIndex = owner != null ? owner.getRenderOrderIndex() : -1;
			bool affected = owner != null && owner.getCanvas() == this && renderIndex >= affectedStart && renderIndex < affectedEnd;
			if (affected)
			{
				continue;
			}
			plan.mComplementVertexCount += vertexCount;
			if (rangeStart < 0)
			{
				rangeStart = slot;
				lastSlot = slot;
				continue;
			}
			if (slot == lastSlot + 1)
			{
				lastSlot = slot;
				continue;
			}
			plan.mComplementSlotRanges.Add(new FastUIRangeData(rangeStart, lastSlot + 1));
			rangeStart = slot;
			lastSlot = slot;
		}
		if (rangeStart >= 0)
		{
			plan.mComplementSlotRanges.Add(new FastUIRangeData(rangeStart, lastSlot + 1));
		}
	}
	private bool tryApplyRenderOriginShift(BulkPositionSlotPlan plan, Vector3 canvasDelta)
	{
		if (!mRenderOriginShiftEnabled || plan == null || canvasDelta == Vector3.zero || mRenderRoot == null || mMeshRenderer == null)
		{
			return false;
		}
		int affectedVertexCount = plan.mVertexCount;
		int complementVertexCount = plan.mComplementVertexCount;
		int savedVertexCount = affectedVertexCount - complementVertexCount;
		if (affectedVertexCount <= complementVertexCount || savedVertexCount < Mathf.Max(mRenderOriginShiftMinSavedVertexCount, 1))
		{
			return false;
		}
		mRenderOriginOffset += canvasDelta;
		mRenderRoot.localPosition = mRenderOriginOffset;
		if (plan.mComplementSlotRanges.Count > 0)
		{
			mMeshRenderer.offsetPositionSlotRanges(plan.mComplementSlotRanges, plan.mComplementSlotRanges.Count, -canvasDelta, out _, out _);
			var starts = plan.mComplementSlotRanges.getStartColumn();
			var ends = plan.mComplementSlotRanges.getEndColumn();
			for (int i = 0; i < plan.mComplementSlotRanges.Count; ++i)
			{
				mMeshRenderer.markPositionSlotRangeDirty(starts[i], ends[i] - starts[i]);
			}
		}
		return true;
	}
	private void offsetRenderRange(int renderStart, int renderCount, Vector3 canvasDelta, BulkPositionSlotPlan capturePlan = null)
	{
		if (ensureRenderVertexSlotMap())
		{
			offsetRenderRangeBySlotMap(renderStart, renderCount, canvasDelta, capturePlan);
			return;
		}
		int renderEnd = Mathf.Min(renderStart + renderCount, mRenderRegistry.Count);
		int start = Mathf.Max(renderStart, 0);
		int rangeStartSlot = -1;
		int lastSlot = -2;
		int vertexCount = 0;
		int dirtyVertexStart = int.MaxValue;
		int dirtyVertexEnd = -1;
		for (int i = start; i < renderEnd; ++i)
		{
			FastUIRenderElement element = mRenderRegistry[i];
			if (element == null || element.getCanvas() != this)
			{
				continue;
			}
			int slot = element.getVertexSlot();
			if (slot < 0)
			{
				continue;
			}
			int geometryVertexCount = mMeshRenderer.getGeometryVertexCount(slot);
			vertexCount += geometryVertexCount;
			if (geometryVertexCount > 0)
			{
				int geometryVertexStart = mMeshRenderer.getGeometryVertexStart(slot);
				if (geometryVertexStart >= 0)
				{
					dirtyVertexStart = Mathf.Min(dirtyVertexStart, geometryVertexStart);
					dirtyVertexEnd = Mathf.Max(dirtyVertexEnd, geometryVertexStart + geometryVertexCount);
				}
			}
			if (rangeStartSlot < 0)
			{
				rangeStartSlot = slot;
				lastSlot = slot;
				continue;
			}
			if (slot == lastSlot + 1)
			{
				lastSlot = slot;
				continue;
			}
			flushOffsetSlotRange(rangeStartSlot, lastSlot, canvasDelta, capturePlan);
			rangeStartSlot = slot;
			lastSlot = slot;
		}
		if (rangeStartSlot >= 0)
		{
			flushOffsetSlotRange(rangeStartSlot, lastSlot, canvasDelta, capturePlan);
		}
		if (dirtyVertexEnd > dirtyVertexStart)
		{
			mMeshRenderer.markPositionVertexRangeDirty(dirtyVertexStart, dirtyVertexEnd - dirtyVertexStart);
		}
		if (capturePlan != null)
		{
			capturePlan.mVertexCount = vertexCount;
		}
	}
	// 与旧offsetRenderRange完全相同的Slot Range合并/Dirty Envelope语义，只把Managed Registry读取换成稳定ECS映射。
	private void offsetRenderRangeBySlotMap(int renderStart, int renderCount, Vector3 canvasDelta, BulkPositionSlotPlan capturePlan)
	{
		if (mRenderVertexSlotMap == null || mMeshRenderer == null || renderCount <= 0 || canvasDelta == Vector3.zero)
		{
			return;
		}
		int start = Mathf.Max(renderStart, 0);
		int renderEnd = Mathf.Min(renderStart + renderCount, Mathf.Min(mRenderRegistry.Count, mRenderVertexSlotMap.Count));
		if (renderEnd <= start)
		{
			return;
		}
		FastUIGeometryRangeData_ECSList geometryRanges = mMeshRenderer.getGeometryRangeECS();
		if (geometryRanges == null)
		{
			return;
		}
		var slotColumn = mRenderVertexSlotMap.getValueColumn();
		var vertexStartColumn = geometryRanges.getVertexStartColumn();
		var vertexCountColumn = geometryRanges.getVertexCountColumn();
		int rangeStartSlot = -1;
		int lastSlot = -2;
		int vertexCount = 0;
		int dirtyVertexStart = int.MaxValue;
		int dirtyVertexEnd = -1;
		for (int i = start; i < renderEnd; ++i)
		{
			int slot = slotColumn[i];
			if ((uint)slot >= (uint)geometryRanges.Count)
			{
				continue;
			}
			int geometryVertexCount = vertexCountColumn[slot];
			vertexCount += geometryVertexCount;
			if (geometryVertexCount > 0)
			{
				int geometryVertexStart = vertexStartColumn[slot];
				if (geometryVertexStart >= 0)
				{
					dirtyVertexStart = Mathf.Min(dirtyVertexStart, geometryVertexStart);
					dirtyVertexEnd = Mathf.Max(dirtyVertexEnd, geometryVertexStart + geometryVertexCount);
				}
			}
			if (rangeStartSlot < 0)
			{
				rangeStartSlot = slot;
				lastSlot = slot;
				continue;
			}
			if (slot == lastSlot + 1)
			{
				lastSlot = slot;
				continue;
			}
			flushOffsetSlotRange(rangeStartSlot, lastSlot, canvasDelta, capturePlan);
			rangeStartSlot = slot;
			lastSlot = slot;
		}
		if (rangeStartSlot >= 0)
		{
			flushOffsetSlotRange(rangeStartSlot, lastSlot, canvasDelta, capturePlan);
		}
		if (dirtyVertexEnd > dirtyVertexStart)
		{
			mMeshRenderer.markPositionVertexRangeDirty(dirtyVertexStart, dirtyVertexEnd - dirtyVertexStart);
		}
		if (capturePlan != null)
		{
			capturePlan.mVertexCount = vertexCount;
		}
	}
	private bool ensureRenderVertexSlotMap()
	{
		if (mRenderVertexSlotMapKnown && mRenderVertexSlotMap != null && mRenderVertexSlotMap.Count == mRenderRegistry.Count)
		{
			return true;
		}
		ensureECSCollections();
		if (mRenderVertexSlotMap == null)
		{
			return false;
		}
		int count = mRenderRegistry.Count;
		mRenderVertexSlotMap.Clear();
		if (count <= 0)
		{
			mRenderVertexSlotMapKnown = true;
			return true;
		}
		mRenderVertexSlotMap.EnsureCount(count, -1);
		var slotColumn = mRenderVertexSlotMap.getValueColumn();
		for (int i = 0; i < count; ++i)
		{
			FastUIRenderElement element = mRenderRegistry[i];
			slotColumn[i] = element != null && element.getCanvas() == this ? element.getVertexSlot() : -1;
		}
		mRenderVertexSlotMapKnown = true;
		return true;
	}
	private void flushOffsetSlotRange(int startSlot, int endSlot, Vector3 canvasDelta, BulkPositionSlotPlan capturePlan = null)
	{
		int slotCount = endSlot - startSlot + 1;
		if (slotCount <= 0)
		{
			return;
		}
		capturePlan?.mSlotRanges.Add(new FastUIRangeData(startSlot, endSlot + 1));
		mMeshRenderer.offsetPositionSlotRange(startSlot, slotCount, canvasDelta);
	}
	private void applyMatrixDelta(int renderStart, int renderCount, Matrix4x4 oldLocalToWorld, Matrix4x4 newLocalToWorld)
	{
		invalidateTextSDFScaleCacheIfNeeded(oldLocalToWorld, newLocalToWorld);
		if (Mathf.Abs(oldLocalToWorld.determinant) <= 0.0000001f)
		{
			markRenderRangeDirty(renderStart, renderCount, FastUIDirtyFlags.Transform);
			return;
		}
		invalidateGeometryMatrixCacheVersion();
		mClipSystem?.markGeometryRangeDirty(renderStart, renderCount);
	}
	private Vector3 convertLocalPositionDeltaToCanvas(Transform root, Vector3 localDelta)
	{
		Transform parent = root.parent;
		if (parent == transform)
		{
			return localDelta;
		}
		if (parent == null)
		{
			return transform.worldToLocalMatrix.MultiplyVector(localDelta);
		}
		Matrix4x4 parentToCanvas = transform.worldToLocalMatrix * parent.localToWorldMatrix;
		return parentToCanvas.MultiplyVector(localDelta);
	}
	private void updateRenderOrderIndices(int start, int count)
	{
		int end = Mathf.Min(start + count, mRenderRegistry.Count);
		for (int i = Mathf.Max(start, 0); i < end; ++i)
		{
			FastUIRenderElement element = mRenderRegistry[i];
			if (element != null)
			{
				element.setRenderOrderIndex(i);
			}
		}
	}
	private void addIndexDirtyRange(int start, int count)
	{
		FastUIRangeUtility.addRange(mIndexDirtyRanges, start, count, mRenderRegistry.Count);
	}
	private void addBatchDirtyRange(int start, int count)
	{
		FastUIRangeUtility.addRange(mBatchDirtyRanges, start, count, mRenderRegistry.Count);
	}
	private int buildMergedRenderRanges(FastUIRangeData_ECSList source)
	{
		mMergedRenderRanges.Clear();
		int count = source.Count;
		if (count == 0)
		{
			return 0;
		}
		var sourceStarts = source.getStartColumn();
		var sourceEnds = source.getEndColumn();
		for (int i = 0; i < count; ++i)
		{
			mMergedRenderRanges.Add(new FastUIRangeData(sourceStarts[i], sourceEnds[i]));
		}
		if (count > 1)
		{
			mMergedRenderRanges.SortFast();
		}
		var starts = mMergedRenderRanges.getStartColumn();
		var ends = mMergedRenderRanges.getEndColumn();
		int writeIndex = 0;
		for (int readIndex = 1; readIndex < count; ++readIndex)
		{
			if (starts[readIndex] <= ends[writeIndex])
			{
				if (ends[readIndex] > ends[writeIndex])
				{
					ends[writeIndex] = ends[readIndex];
				}
				continue;
			}
			++writeIndex;
			starts[writeIndex] = starts[readIndex];
			ends[writeIndex] = ends[readIndex];
		}
		int mergedCount = writeIndex + 1;
		if (mMergedRenderRanges.Count > mergedCount)
		{
			mMergedRenderRanges.RemoveRange(mergedCount, mMergedRenderRanges.Count - mergedCount);
		}
		return mergedCount;
	}
	private void markTransformRangesDirty()
	{
		mTransformRanges?.markDirty();
		clearBulkPositionSlotPlans();
		invalidateRenderSlotContiguityCache();
		// Translation日志按Logical RenderIndex记录；结构变化后Range含义可能变化，必须回到真实Transform重算。
		invalidateGeometryMatrixCacheVersion();
	}
	private void clearBulkPositionSlotPlans()
	{
		foreach (KeyValuePair<Transform, BulkPositionSlotPlan> pair in mBulkPositionSlotPlans)
		{
			pair.Value?.dispose();
		}
		mBulkPositionSlotPlans.Clear();
	}
	private bool tryGetTransformRange(Transform root, out int renderStart, out int renderCount)
	{
		ensureTransformRanges();
		return mTransformRanges.tryGetRange(root, out renderStart, out renderCount);
	}
	private void ensureTransformRanges()
	{
		ensureECSCollections();
		if (!mTransformRanges.isDirty())
		{
			return;
		}
		using (REBUILD_TRANSFORM_RANGE_MARKER.Auto())
		{
			mTransformRanges.rebuild(mRenderRegistry.getElements(), this, transform);
		}
	}
	public int getTransformNodeCount()
	{
		ensureTransformRanges();
		return mTransformRanges.getCount();
	}
	public int getHiddenRootCount()
	{
		ensureECSCollections();
		return mVisibility.getHiddenRootCount();
	}
	public int getHiddenRangeCount()
	{
		ensureECSCollections();
		return mVisibility.getHiddenRangeCount();
	}
	public bool getVisible(Transform root)
	{
		ensureECSCollections();
		return mVisibility.getVisible(root, this);
	}
	public void setVisible(Transform root, bool visible)
	{
		ensureECSCollections();
		mVisibility.setVisible(root, visible, this, transform);
	}
	// FastUIVisibility/FastUIRenderElement已经绑定到当前Canvas，运行期显隐不重复扫描父链校验Canvas归属。
	public void setVisibilityRootVisible(RectTransform root, bool visible)
	{
		ensureECSCollections();
		mVisibility.setManagedVisible(root, visible);
	}
	public void removeVisibilityRoot(Transform root)
	{
		if (mDestroying || root == null)
		{
			return;
		}
		ensureECSCollections();
		mVisibility.removeRoot(root);
	}
	public void clearVisibility()
	{
		ensureECSCollections();
		mVisibility.clear();
	}
	private void markVisibilityStructureChanged()
	{
		mVisibility?.markStructureChanged();
	}
	private bool rebuildVisibilityRanges(bool forceIndexMode, ref FastUIFrameStats stats)
	{
		ensureTransformRanges();
		return mVisibility.rebuild(this, transform, mRenderRegistry.getElements(), mTransformRanges, mIndexDirtyRanges, forceIndexMode, mVisibilityIndexThreshold, ref stats);
	}
	private bool hasSOARenderGroups()
	{
		return mSOARenderSystem != null && mSOARenderSystem.getGroupCount() > 0;
	}
	public void registerSOARenderGroup(FastSOARenderGroup group)
	{
		if (mDestroying || group == null)
		{
			return;
		}
		ensureECSCollections();
		if (mDestroying || mSOARenderSystem == null)
		{
			return;
		}
		int oldCount = mSOARenderSystem.getGroupCount();
		mSOARenderSystem.register(group);
		if (mSOARenderSystem.getGroupCount() != oldCount)
		{
			markSOAStructureDirty();
		}
	}
	public void unregisterSOARenderGroup(FastSOARenderGroup group)
	{
		if (mDestroying || group == null || mSOARenderSystem == null)
		{
			return;
		}
		int oldCount = mSOARenderSystem.getGroupCount();
		mSOARenderSystem.unregister(group);
		if (mSOARenderSystem.getGroupCount() != oldCount)
		{
			markSOAStructureDirty();
		}
	}
	public void markSOAStructureDirty()
	{
		if (mDestroying)
		{
			return;
		}
		mFullDrawStructureDirty = true;
		markTransformRangesDirty();
		mVisibility?.markStructureChanged();
	}
	public bool tryGetTransformRenderRange(Transform root, out int renderStart, out int renderCount)
	{
		if (root == null)
		{
			renderStart = -1;
			renderCount = 0;
			return false;
		}
		ensureTransformRanges();
		return mTransformRanges.tryGetRange(root, out renderStart, out renderCount);
	}
	public int getClipRootCount()
	{
		ensureECSCollections();
		return mClipSystem.getClipRootCount();
	}
	public int getClippedElementCount()
	{
		ensureECSCollections();
		return mClipSystem.getClippedElementCount();
	}
	public int getClipHierarchyNodeCount()
	{
		ensureECSCollections();
		return mClipSystem.getLastHierarchyNodeCount();
	}
	public int getClipCullOutsideCount()
	{
		return mClipSystem != null ? mClipSystem.getClipCullOutsideCount() : 0;
	}
	public void markClipCullBoundsDirty()
	{
		mClipSystem?.markClipBoundsDirty();
	}
	public void patchClipCullState(int renderIndex, bool outside)
	{
		if (renderIndex < 0 || mMeshRenderer == null)
		{
			return;
		}
		mMeshRenderer.patchBatchElementClipCullState(renderIndex, outside);
	}
	public bool tryGetClipCullGeometryBounds(FastUIRenderElement element, out FastUISpatialBoundsData bounds)
	{
		bounds = default;
		if (element == null || element.getCanvas() != this || mMeshRenderer == null)
		{
			return false;
		}
		int slot = element.getVertexSlot();
		if (slot < 0)
		{
			return false;
		}
		if (element is FastText)
		{
			int activeVertexCount = mMeshRenderer.getSimpleTextActiveVertexCount(slot);
			if (activeVertexCount > 0)
			{
				return mMeshRenderer.tryGetGeometryBounds(slot, activeVertexCount, out bounds);
			}
		}
		return mMeshRenderer.tryGetGeometryBounds(slot, out bounds);
	}
	public void registerRectMask(FastRectMask2D clip)
	{
		if (mDestroying || clip == null)
		{
			return;
		}
		ensureECSCollections();
		mClipSystem.register(clip);
	}
	public void unregisterRectMask(FastRectMask2D clip)
	{
		if (mDestroying || clip == null)
		{
			return;
		}
		ensureECSCollections();
		mClipSystem.unregister(clip);
	}
	public void markClipStructureDirty()
	{
		mClipSystem?.markStructureDirty();
	}
	private bool refreshFrameClipCull()
	{
		if (mClipSystem == null || mMeshRenderer == null)
		{
			return false;
		}
		ensureTransformRanges();
		bool enabledChanged = mMeshRenderer.setClipSubmitRangeCullingEnabled(true);
		bool changed = mClipSystem.refreshCull(mRenderRegistry.getElements(), this, mTransformRanges, mFullDrawStructureDirty, true);
		return changed || enabledChanged;
	}

	private void refreshFrameClipStructure()
	{
		if (mClipSystem == null || !mClipSystem.isStructureDirty())
		{
			return;
		}
		// Clip直接复用TransformRange的DFS Node/ParentIndex，避免再次解析整棵Transform层级。
		ensureTransformRanges();
		using (CLIP_STRUCTURE_MARKER.Auto())
		{
			mClipSystem.refreshStructure(mRenderRegistry.getElements(), this, mTransformRanges, mFullDrawStructureDirty);
		}
	}
	public void setVisibilityIndexThreshold(int count)
	{
		count = Mathf.Max(count, 0);
		if (mVisibilityIndexThreshold == count)
		{
			return;
		}
		mVisibilityIndexThreshold = count;
		mVisibility?.markStructureChanged();
	}
	public void markStructureDirty()
	{
		scheduleDeferredStructure(true);
	}
	// ECS容器延迟初始化，避免MonoBehaviour字段初始化阶段创建依赖Unity生命周期的资源。
	private void ensureECSCollections()
	{
		if (mECSCollectionsInitialized)
		{
			return;
		}
		try
		{
			mDirtyVertexSlots = new Int_ECSList(256);
			mDirtyTextVertexSlots = new Int_ECSList(128);
			mInitialSimpleQuadBatchSlots = new Int_ECSList(256);
			mInitialSimpleQuadRemainingDirtySlots = new Int_ECSList(256);
			mInitialSimpleQuadCandidateECS = new Int_ECSList(256);
			mRuntimeSimpleQuadBatchSlots = new Int_ECSList(256);
			mRuntimeSimpleQuadRemainingDirtySlots = new Int_ECSList(256);
			mInitialSimpleTextRangeBatchSlots = new Int_ECSList(128);
			mInitialSimpleTextRangeBatchCapacities = new Int_ECSList(128);
			mVertexSlotRuntimeECS = new FastUIVertexSlotRuntimeData_ECSList(256);
			mSimpleTextBatchWorkECS = new FastUITextBatchWorkData_ECSList(128);
			mPersistentTextGlyphECS = new FastUITextSimpleGlyphData_ECSList(2048);
			mTransformPositionBatchECS = new FastUITransformPositionBatchData_ECSList(128);
			mGeometryMatrixPositionTranslationECS = new FastUITransformPositionBatchData_ECSList(8);
			mRenderVertexSlotMap = new Int_ECSList(256);
			mTransformRanges = new FastUITransformRangeSystem(256);
			mIndexDirtyRanges = new FastUIRangeData_ECSList(64);
			mBatchDirtyRanges = new FastUIRangeData_ECSList(64);
			mBatchDrawDirtyRanges = new FastUIRangeData_ECSList(64);
			mMergedRenderRanges = new FastUIRangeData_ECSList(64);
			mVisibility = new FastUIVisibilitySystem(64);
			mClipSystem = new FastUIClipSystem();
			mSOARenderSystem = new FastUISOARenderSystem(this);
			mECSCollectionsInitialized = true;
			registerDomainUnloadHook();
		}
		catch
		{
			disposeECSCollections();
			throw;
		}
	}
	private void disposeECSCollections()
	{
		clearBulkPositionSlotPlans();
		mDirtyVertexSlots?.Dispose();
		mDirtyVertexSlots = null;
		mDirtyTextVertexSlots?.Dispose();
		mDirtyTextVertexSlots = null;
		mInitialSimpleQuadBatchSlots?.Dispose();
		mInitialSimpleQuadBatchSlots = null;
		mInitialSimpleQuadRemainingDirtySlots?.Dispose();
		mInitialSimpleQuadRemainingDirtySlots = null;
		mInitialSimpleQuadCandidateECS?.Dispose();
		mInitialSimpleQuadCandidateECS = null;
		mRuntimeSimpleQuadBatchSlots?.Dispose();
		mRuntimeSimpleQuadBatchSlots = null;
		mRuntimeSimpleQuadRemainingDirtySlots?.Dispose();
		mRuntimeSimpleQuadRemainingDirtySlots = null;
		mInitialSimpleTextRangeBatchSlots?.Dispose();
		mInitialSimpleTextRangeBatchSlots = null;
		mInitialSimpleTextRangeBatchCapacities?.Dispose();
		mInitialSimpleTextRangeBatchCapacities = null;
		mVertexSlotRuntimeECS?.Dispose();
		mVertexSlotRuntimeECS = null;
		mSimpleTextBatchWorkECS?.Dispose();
		mSimpleTextBatchWorkECS = null;
		mPersistentTextGlyphECS?.Dispose();
		mPersistentTextGlyphECS = null;
		mTransformPositionBatchECS?.Dispose();
		mTransformPositionBatchECS = null;
		mGeometryMatrixPositionTranslationECS?.Dispose();
		mGeometryMatrixPositionTranslationECS = null;
		mGeometryMatrixPositionTranslationSerial = 0;
		mRenderVertexSlotMap?.Dispose();
		mRenderVertexSlotMap = null;
		mRenderVertexSlotMapKnown = false;
		mTransformPositionBatchDepth = 0;
		mRenderRegistry.clearVertexSlotOwners();
		mTransformRanges?.dispose();
		mTransformRanges = null;
		mIndexDirtyRanges?.Dispose();
		mIndexDirtyRanges = null;
		mBatchDirtyRanges?.Dispose();
		mBatchDirtyRanges = null;
		mBatchDrawDirtyRanges?.Dispose();
		mBatchDrawDirtyRanges = null;
		mMergedRenderRanges?.Dispose();
		mMergedRenderRanges = null;
		mVisibility?.dispose();
		mVisibility = null;
		mClipSystem?.dispose();
		mClipSystem = null;
		mSOARenderSystem?.dispose();
		mSOARenderSystem = null;
		mInitialStoragePrewarmDone = false;
		mECSCollectionsInitialized = false;
	}
	private void OnDestroy()
	{
#if UNITY_EDITOR
		unregisterEditorSceneViewHook();
		if (mEditorPreviewRefreshQueued)
		{
			EditorApplication.delayCall -= refreshEditorPreviewDelayed;
			mEditorPreviewRefreshQueued = false;
		}
#endif
		mDestroying = true;
		mHierarchySuspended = false;
		mHierarchyResumePending = false;
#if UNITY_EDITOR
		mEditorGPUBufferValidationPending = false;
#endif
		unregisterDomainUnloadHook();
		for (int i = 0; i < mRenderRegistry.Count; ++i)
		{
			FastUIRenderElement element = mRenderRegistry[i];
			if (element != null && element.getCanvas() == this)
			{
				element.setVertexSlot(-1);
				element.setRenderOrderIndex(-1);
				element.setCanvasRenderRange(-1, 1);
			}
		}
		mRenderRegistry.Clear();
		disposeECSCollections();
		mTMPVertexLayoutElementCount = 0;
		disposeLeafPositionDeltaECS();
		mPendingLeafPositionDeltaCount = 0;
		if (mMeshRenderer != null)
		{
			mMeshRenderer.destroyRenderer();
		}
		if (mRuntimeDefaultMaterial != null)
		{
			if (Application.isPlaying)
			{
				Destroy(mRuntimeDefaultMaterial);
			}
			else
			{
				DestroyImmediate(mRuntimeDefaultMaterial);
			}
			mRuntimeDefaultMaterial = null;
		}
		if (mRenderRoot != null)
		{
			GameObject rootObject = mRenderRoot.gameObject;
			mRenderRoot = null;
			if (Application.isPlaying)
			{
				Destroy(rootObject);
			}
			else
			{
				DestroyImmediate(rootObject);
			}
		}
		mMeshRenderer = null;
		mInitialized = false;
	}

#if UNITY_EDITOR
	// Edit Mode下HideAndDontSave的内部RenderRoot可能被Unity在Undo/重载/Preview清理阶段独立销毁。
	// Renderer一旦丢失，旧VertexSlot与旧ECS元数据已经没有意义；这里立即把Canvas恢复到“可重新注册”的干净状态，
	// 下一次Preview Refresh会重新创建Renderer并让所有后代重新注册。
	public void notifyMeshRendererDestroyed(FastUIMeshRenderer renderer)
	{
		// OnDestroy执行期间UnityEngine.Object的== null语义可能已经为true，
		// 因此这里使用托管引用身份判断，确保内部Renderer销毁通知不会被吞掉。
		if (mDestroying || !ReferenceEquals(mMeshRenderer, renderer))
		{
			return;
		}

		for (int i = 0; i < mRenderRegistry.Count; ++i)
		{
			FastUIRenderElement element = mRenderRegistry[i];
			if (element == null || element.getCanvas() != this)
			{
				continue;
			}
			element.setVertexSlot(-1);
			element.setRenderOrderIndex(-1);
			element.setCanvasRenderRange(-1, 1);
		}

		mRenderRegistry.Clear();
		mLiveRenderElementCount = 0;
		mActiveRenderElementCount = 0;
		mTombstoneCount = 0;
		mTMPVertexLayoutElementCount = 0;
		mPendingLeafPositionDeltaCount = 0;
		mMeshRenderer = null;
		mRenderRoot = null;
		mInitialized = false;

		disposeLeafPositionDeltaECS();
		disposeECSCollections();

		mDeferredStructureDirty = false;
		mDeferredStructureTransformAllDirty = false;
		mFullDrawStructureDirty = true;
		mIndexCapacityDirty = false;
		mHierarchySuspended = false;
		mHierarchyResumePending = false;

		if (!Application.isPlaying)
		{
			queueEditorPreviewRefresh();
		}
	}
#else
	public void notifyMeshRendererDestroyed(FastUIMeshRenderer renderer)
	{
	}
#endif

	// Canvas初始化只执行一次，同时创建内部RenderRoot、Renderer和各独立状态System。
	private void ensureInit()
	{
		// 销毁阶段禁止重入初始化。某些业务OnDisable/OnDestroy回调可能间接调用flushFrameNow。
		if (mDestroying)
		{
			return;
		}
		// 已初始化后只有在Renderer仍然可注册时才直接返回。
		// Edit Mode下内部HideAndDontSave RenderRoot可能被Unity独立清理，不能只相信mInitialized。
		if (mInitialized)
		{
#if UNITY_EDITOR
			if (!Application.isPlaying && (mMeshRenderer == null || !mMeshRenderer.isRegistrationReady()))
			{
				if (mMeshRenderer != null)
				{
					notifyMeshRendererDestroyed(mMeshRenderer);
				}
				else
				{
					// Unity已经把Renderer对象判为Destroyed时，无法再通过回调参数做身份判断。
					// 这里同样清空旧Slot/Registry，保证后续refreshDescendantBindings能够完整重新注册。
					for (int i = 0; i < mRenderRegistry.Count; ++i)
					{
						FastUIRenderElement element = mRenderRegistry[i];
						if (element != null && element.getCanvas() == this)
						{
							element.setVertexSlot(-1);
							element.setRenderOrderIndex(-1);
							element.setCanvasRenderRange(-1, 1);
						}
					}
					mRenderRegistry.Clear();
					mLiveRenderElementCount = 0;
					mActiveRenderElementCount = 0;
					mTombstoneCount = 0;
					mTMPVertexLayoutElementCount = 0;
					mPendingLeafPositionDeltaCount = 0;
					mRenderRoot = null;
					mMeshRenderer = null;
					mInitialized = false;
					disposeLeafPositionDeltaECS();
					disposeECSCollections();
					queueEditorPreviewRefresh();
				}
			}
			else
#endif
			{
				return;
			}
		}
		ensureECSCollections();
		GameObject rootObject = new("FastUIRenderRoot")
		{
			hideFlags = HideFlags.HideAndDontSave,
			layer = gameObject.layer
		};
		mRenderRoot = rootObject.transform;
		mRenderRoot.SetParent(transform, false);
		mRenderOriginOffset = Vector3.zero;
		mRenderRoot.SetLocalPositionAndRotation(Vector3.zero, Quaternion.identity);
		mRenderRoot.localScale = Vector3.one;
		mMeshRenderer = rootObject.AddComponent<FastUIMeshRenderer>();
		mMeshRenderer.init(this);
		mMeshRenderer.setRenderVisible(mRenderVisible && isActiveAndEnabled);
		if (mDefaultMaterial == null)
		{
			Shader shader = Shader.Find("Sprites/Default");
			if (shader != null)
			{
				mRuntimeDefaultMaterial = new Material(shader)
				{
					name = "FastUI_Default_Runtime",
					hideFlags = HideFlags.HideAndDontSave,
				};
			}
			else
			{
				Debug.LogError("FastCanvas找不到Unity内置Shader:Sprites/Default");
			}
		}

		mInitialized = true;
		refreshRendererSetting();
	}
}
