using System;
using UnityEngine;

// SOA只比较显式兼容ID，不依赖具体C#继承类型。
// 兼容ID表达的是“这个Hierarchy Role能否按相同渲染/几何语义组成SOA Lane”，不是类名。
public static class FastUIRenderSOACompatibility
{
	public const int RawImage = 1;
	public const int Image = 2;
	public const int Text = 3;
	public const int TextAtlasRun = 4;
	public const int InputFieldSelection = 5;
	public const int ClipStencil = 6;
	public const int MaskPop = 7;
}

[Flags]
public enum FastUIDirtyFlags : byte
{
	None = 0,
	Transform = 1 << 0,
	Geometry = 1 << 1,
	Color = 1 << 2,
	UV = 1 << 3,
	PositionDelta = 1 << 4,
	UV0Only = 1 << 5,
	AnyUV = UV | UV0Only,
	AllVertex = Transform | Geometry | Color | UV,
}

// FastCanvas中所有真实可渲染节点的唯一公共基类。
// Canvas注册、可见性、颜色、材质、Mask、Transform缓存、稳定Slot与Dirty通知直接位于这一层；
// Texture、UVRect、Sprite、Text/Glyph等具体数据仍由各自类型持有，只有真正类型相关的渲染/几何行为保留多态。
[ExecuteAlways]
[RequireComponent(typeof(RectTransform))]
public abstract class FastUIRenderElement : MonoBehaviour
{
	[SerializeField] protected Material mMaterial;
	[SerializeField] protected Color mColor = Color.white;
	[SerializeField] protected bool mCull;
	[SerializeField] protected bool mVisible = true;
	protected RectTransform mRectTransform;
	protected FastCanvas mCanvas;
	protected int mVertexSlot = -1;
	protected int mRenderOrderIndex = -1;
	protected int mCanvasRenderStart = -1;
	protected int mCanvasRenderCount = 1;
	protected bool mRenderActive;
	protected bool mHasActiveClip;
	protected bool mClipCullOutside;
	protected bool mInitialized;
	protected bool mDirectChildOfCanvas;
	protected bool mCanvasHierarchySuspended;
	protected bool mCloneRegistrationPending;
	// setSize/setPivot自己会在最终值写入后精确同步Geometry。
	// 期间屏蔽Unity同步触发的OnRectTransformDimensionsChange，避免同一Mutation重复进入Canvas Dirty入口。
	protected bool mSuppressRectTransformDimensionsCallback;
	protected Vector3 mLocalBottomLeft;
	protected Vector3 mLocalTopLeft;
	protected Vector3 mLocalTopRight;
	protected Vector3 mLocalBottomRight;
	protected Vector3 mCachedLocalPosition;
	protected Quaternion mCachedLocalRotation;
	protected Vector3 mCachedLocalScale;
	protected Rect mCachedRect;
	protected Matrix4x4 mCachedGeometryLocalToCanvasMatrix;
	protected bool mCachedGeometryLocalToCanvasMatrixValid;
	protected int mCachedGeometryLocalToCanvasMatrixVersion;
	protected int mCachedGeometryPositionTranslationSerial;
	protected static long sGeometryMatrixPositionTranslationCarryHitCount;
	public static void resetGeometryMatrixPositionTranslationCarryStats()
	{
		sGeometryMatrixPositionTranslationCarryHitCount = 0;
	}
	public static long getGeometryMatrixPositionTranslationCarryHitCount()
	{
		return sGeometryMatrixPositionTranslationCarryHitCount;
	}
	protected FastMask mAttachedMask;
	protected FastUIMaskState mMaskState;
	protected Material mMaskRuntimeMaterial;
	protected Material mMaskRuntimeSourceMaterial;
	protected virtual void Awake()
	{
		// Unity在Active对象AddComponent/Prefab Instantiate时会紧接着调用OnEnable。
		// Awake只缓存本组件数据，Canvas绑定统一留给OnEnable，避免同一次创建重复findCanvas/registerCanvas。
		ensureInit();
	}
	protected virtual void OnEnable()
	{
		ensureInit();
		if (FastUICloneUtility.tryBindGraphic(this, true))
		{
			return;
		}
		if (mCanvasHierarchySuspended && mCanvas != null && mVertexSlot >= 0)
		{
			mCanvasHierarchySuspended = false;
			return;
		}
		int oldSlot = mVertexSlot;
		registerCanvas();
		bool initialRegister = oldSlot < 0 && mVertexSlot >= 0;
		setRenderActiveState(true);
		// Awake刚缓存过Rect/LocalTransform，初次OnEnable紧随其后时这里必然无变化。
		// 只在重新Enable/重新注册路径做4组RectTransform属性比较，避免新建19200+ Graphic的纯读取成本。
		if (!initialRegister)
		{
			refreshChangedDataAfterEnable();
		}
		if (mAttachedMask != null)
		{
			mAttachedMask.notifyGraphicActiveChanged();
		}
		// 大量新建Graphic位于FastRectMask2D下时，逐元素calculateState会重复向上遍历同一Hierarchy。
		// 首帧ClipSystem本来就会用HierarchyDepthCache统一计算，初次注册直接复用那条路径。
		if (!initialRegister || mCanvas == null || !mCanvas.tryDeferInitialGraphicMaskRefresh())
		{
			refreshMaskStateFromHierarchy();
		}
	}
	protected virtual void OnDisable()
	{
		if (!mInitialized)
		{
			return;
		}
		if (mCanvas != null && !mCanvas.gameObject.activeInHierarchy)
		{
			mCanvasHierarchySuspended = true;
			return;
		}
		setRenderActiveState(false);
		if (mAttachedMask != null)
		{
			mAttachedMask.notifyGraphicActiveChanged();
		}
	}
	protected virtual void OnDestroy()
	{
		releaseMaskRuntimeMaterial();
		if (mAttachedMask != null)
		{
			mAttachedMask.detachGraphic(this);
		}
		unregisterCanvas();
	}
	protected virtual void OnTransformParentChanged()
	{
		if (!mInitialized)
		{
			return;
		}
		if (FastUICloneUtility.tryBindGraphic(this, isActiveAndEnabled))
		{
			return;
		}
		FastCanvas oldCanvas = mCanvas;
		FastCanvas newCanvas = findCanvas();
		if (oldCanvas != newCanvas)
		{
			if (oldCanvas != null)
			{
				oldCanvas.removeVisibilityRoot(mRectTransform);
				oldCanvas.unregister(this);
			}
			mCanvas = newCanvas;
			mVertexSlot = -1;
			mRenderOrderIndex = -1;
			mCanvasRenderStart = -1;
			mCanvasRenderCount = 1;
			refreshDirectChildMode();
			if (mCanvas != null)
			{
				mCanvas.register(this);
				if (!mVisible)
				{
					mCanvas.setVisible(mRectTransform, false);
				}
			}
		}
		else
		{
			refreshDirectChildMode();
			if (mCanvas != null)
			{
				mCanvas.notifyElementHierarchyOrderChanged(this);
			}
			if (mCanvas != null)
			{
				mCanvas.notifyElementTransformDirty(this);
			}
		}
		FastMaskUtility.refreshSubtree(transform);
	}
	protected virtual void OnRectTransformDimensionsChange()
	{
		if (!mInitialized || mSuppressRectTransformDimensionsCallback)
		{
			return;
		}
		// 某些Unity版本会把RectTransform尺寸通知延迟到Setter返回后。
		// 如果缓存已经是当前Rect，说明高层Setter已经完成同步，不再重复Dirty。
		if (mCachedRect == mRectTransform.rect)
		{
			return;
		}
		refreshLocalGeometryCache();
		syncLocalTransformCache();
		notifyRectGeometryChanged();
	}
	protected virtual void OnDidApplyAnimationProperties()
	{
		if (!mInitialized)
		{
			return;
		}
		refreshLocalGeometryCache();
		syncLocalTransformCache();
		if (mCanvas != null)
		{
			mCanvas.notifyElementGeometryDirty(this);
		}
	}
	public RectTransform getRectTransform()
	{
		ensureInit();
		return mRectTransform;
	}
	public FastCanvas getCanvas()
	{
		return mCanvas;
	}
	public Material getMaterial()
	{
		return mMaterial;
	}
	public Color getColor()
	{
		return mColor;
	}
	public bool isCulled()
	{
		return mCull;
	}
	public bool isRenderActive()
	{
		return mRenderActive;
	}
	public bool getVisible()
	{
		return mVisible;
	}
	public abstract Texture getRenderTexture();
	public abstract int getSOACompatibilityID();
	public virtual string getSOACompatibilityName()
	{
		return GetType().Name;
	}
	// matrix参数在RenderOrigin启用后表示World->当前RenderRoot局部几何空间；对外Canvas坐标语义由FastCanvas/FastUIMeshRenderer统一还原。
	public abstract void buildGeometry(FastUIGeometryBuilder builder, Matrix4x4 canvasWorldToLocal);
	// 只有“固定单Quad且整个GeometryRange统一使用一个UV Rect”的元素才返回True。
	public virtual bool tryGetSimpleUVRect(out Rect uvRect)
	{
		uvRect = default;
		return false;
	}
	// 固定单Quad元素在Rect/Transform变化时可直接更新Position Stream，跳过GeometryBuilder以及Color/UV重复拷贝。
	public virtual bool tryGetSimpleQuadPositionGeometry(Matrix4x4 canvasWorldToLocal, out Vector3 bottomLeft, out Vector3 topLeft, out Vector3 topRight, out Vector3 bottomRight)
	{
		bottomLeft = default; 
		topLeft = default; 
		topRight = default; 
		bottomRight = default; 
		return false;
	}
	public virtual bool tryGetRuntimeSimpleQuadPositionGeometry(Matrix4x4 canvasWorldToLocal, bool useCachedLocalState, out Vector3 bottomLeft, out Vector3 topLeft, out Vector3 topRight, out Vector3 bottomRight)
	{
		return tryGetSimpleQuadPositionGeometry(canvasWorldToLocal, out bottomLeft, out topLeft, out topRight, out bottomRight);
	}
	// Runtime SimpleQuad Batch已经用Candidate Metadata验证了具体类型与Simple Quad语义时，直接复用同一份缓存几何计算。
	// 该入口不改变数学计算和顶点顺序，只移除FastImage/FastRawImage override中的重复资格检查与初始化检查。
	public void calculateRuntimeSimpleQuadCachedPositionGeometryDirect(Matrix4x4 canvasWorldToLocal, out Vector3 bottomLeft, out Vector3 topLeft, out Vector3 topRight, out Vector3 bottomRight)
	{
		calculateRuntimeCachedSimpleQuadPositionGeometry(canvasWorldToLocal, out bottomLeft, out topLeft, out topRight, out bottomRight);
	}
	// 诊断用：0=CanvasDirectIdentity, 1=CanvasDirectAffine, 2=Matrix。正式路径不调用。
	public int getRuntimeSimpleQuadCachedGeometryMode()
	{
		if (!mDirectChildOfCanvas)
		{
			return 2;
		}
		return mCachedLocalRotation == Quaternion.identity && mCachedLocalScale == Vector3.one ? 0 : 1;
	}
	public virtual bool requiresTMPVertexLayout()
	{
		return false;
	}
	public bool hasActiveClip()
	{
		return mHasActiveClip;
	}
	public bool isClipCullOutside()
	{
		return mClipCullOutside;
	}
	public virtual void setClipCullOutside(bool outside)
	{
		mClipCullOutside = outside;
	}
	public FastUIMaskState getMaskState()
	{
		return mMaskState;
	}
	public bool hasActiveMask()
	{
		return mMaskState.mMode != FastUIMaskMaterialMode.None;
	}
	public FastMask getAttachedMask()
	{
		return mAttachedMask;
	}
	public bool isStencilBarrier()
	{
		return isStencilBarrierState(mMaskState);
	}
	public bool syncAfterCanvasHierarchyResume(out bool activeChanged, out bool vertexChanged)
	{
		mCanvasHierarchySuspended = false;
		bool active = isActiveAndEnabled;
		activeChanged = mRenderActive != active;
		mRenderActive = active;
		vertexChanged = active && refreshChangedDataAfterEnable();
		return active;
	}
	public void setAttachedMask(FastMask mask)
	{
		mAttachedMask = mask;
		refreshMaskStateFromHierarchy();
	}
	public void clearAttachedMask(FastMask mask)
	{
		if (mAttachedMask != mask)
		{
			return;
		}
		mAttachedMask = null;
		refreshMaskStateFromHierarchy();
	}
	public virtual void refreshMaskStateFromHierarchy()
	{
		FastUIMaskState state = FastMaskUtility.calculateState(this);
		setMaskState(state);
	}
	public void setMaskState(FastUIMaskState state)
	{
		setMaskState(state, false);
	}
	public void setMaskState(FastUIMaskState state, bool suppressBatchNotify)
	{
		if (mMaskState == state)
		{
			return;
		}
		bool oldBarrier = isStencilBarrierState(mMaskState);
		bool newBarrier = isStencilBarrierState(state);
		releaseMaskRuntimeMaterial();
		mMaskState = state;
		// Full DrawStructure会在本帧稍后重新读取所有最终Mask状态，此时逐Graphic Batch Patch没有任何价值。
		if (!suppressBatchNotify)
		{
			if (mCanvas != null)
			{
				mCanvas.notifyElementBatchChanged(this);
			}
		}
		if (oldBarrier != newBarrier)
		{
			if (mCanvas != null)
			{
				mCanvas.notifyStencilBarrierChanged();
			}
		}
	}
	public virtual bool requiresGeometryRebuildForColor()
	{
		return false;
	}
	public virtual bool geometryControlsVertexColor()
	{
		return false;
	}
	public virtual bool requiresGeometryRebuildForMatrixTransform()
	{
		return false;
	}
	public void setHasActiveClip(bool hasClip)
	{
		mHasActiveClip = hasClip;
	}
	public int getVertexSlot()
	{
		return mVertexSlot;
	}
	public int getRenderOrderIndex()
	{
		return mRenderOrderIndex;
	}
	public int getCanvasRenderStart()
	{
		return mCanvasRenderStart;
	}
	public int getCanvasRenderCount()
	{
		return mCanvasRenderCount;
	}
	public virtual void setMaterial(Material material)
	{
		if (mMaterial == material)
		{
			return;
		}
		releaseMaskRuntimeMaterial();
		mMaterial = material;
		notifyBatchChanged();
	}
	public virtual void setColor(Color color)
	{
		if (mColor == color)
		{
			return;
		}
		mColor = color;
		markVertexDirty(FastUIDirtyFlags.Color);
		if (mAttachedMask != null)
		{
			mAttachedMask.notifyGraphicColorChanged();
		}
	}
	public virtual void setAlpha(float alpha)
	{
		if (Mathf.Approximately(mColor.a, alpha))
		{
			return;
		}
		mColor.a = alpha;
		markVertexDirty(FastUIDirtyFlags.Color);
		if (mAttachedMask != null)
		{
			mAttachedMask.notifyGraphicColorChanged();
		}
	}
	public float getAlpha()
	{
		return mColor.a;
	}
	public virtual void cull(bool cull)
	{
		if (mCull == cull)
		{
			return;
		}
		mCull = cull;
		if (mCanvas != null)
		{
			mCanvas.notifyElementCullChanged(this);
		}
		if (mAttachedMask != null)
		{
			mAttachedMask.notifyGraphicCullChanged();
		}
	}
	public virtual void setVisible(bool visible)
	{
		if (mVisible == visible)
		{
			return;
		}
		bool oldVisible = mVisible;
		mVisible = visible;
		if (mCanvas != null)
		{
			mCanvas.notifyElementVisibleChanged(this, oldVisible, visible);
		}
	}
	public void setLocalPosition(Vector3 position)
	{
		ensureInit();
		Vector3 oldPosition = mCachedLocalPosition;
		if (oldPosition == position)
		{
			return;
		}
		mRectTransform.localPosition = position;
		mCachedLocalPosition = position;
		mCachedGeometryLocalToCanvasMatrixValid = false;
		if (mCanvas != null)
		{
			mCanvas.notifyElementPositionDeltaFast(this, mVertexSlot, mCanvasRenderCount, position - oldPosition);
		}
	}
	public void setAnchoredPosition(Vector2 position)
	{
		ensureInit();
		if (mRectTransform.anchoredPosition == position)
		{
			return;
		}
		Vector3 oldLocalPosition = mRectTransform.localPosition;
		mRectTransform.anchoredPosition = position;
		Vector3 newLocalPosition = mRectTransform.localPosition;
		mCachedLocalPosition = newLocalPosition;
		mCachedGeometryLocalToCanvasMatrixValid = false;
		if (mCanvas != null)
		{
			mCanvas.notifyElementPositionDeltaFast(this, mVertexSlot, mCanvasRenderCount, newLocalPosition - oldLocalPosition);
		}
	}
	public void setLocalScale(Vector3 scale)
	{
		ensureInit();
		if (mRectTransform.localScale == scale)
		{
			return;
		}
		Matrix4x4 oldLocalToWorld = mRectTransform.localToWorldMatrix;
		mRectTransform.localScale = scale;
		mCachedLocalScale = scale;
		mCachedGeometryLocalToCanvasMatrixValid = false;
		if (mCanvas != null)
		{
			mCanvas.notifyElementTransformMatrixChanged(this, oldLocalToWorld, mRectTransform.localToWorldMatrix);
		}
	}
	public void setLocalRotation(Quaternion rotation)
	{
		ensureInit();
		if (mRectTransform.localRotation == rotation)
		{
			return;
		}
		Matrix4x4 oldLocalToWorld = mRectTransform.localToWorldMatrix;
		mRectTransform.localRotation = rotation;
		mCachedLocalRotation = rotation;
		mCachedGeometryLocalToCanvasMatrixValid = false;
		if (mCanvas != null)
		{
			mCanvas.notifyElementTransformMatrixChanged(this, oldLocalToWorld, mRectTransform.localToWorldMatrix);
		}
	}
	public void setPivot(Vector2 pivot)
	{
		ensureInit();
		if (mRectTransform.pivot == pivot)
		{
			return;
		}
		mSuppressRectTransformDimensionsCallback = true;
		try
		{
			mRectTransform.pivot = pivot;
		}
		finally
		{
			mSuppressRectTransformDimensionsCallback = false;
		}
		refreshLocalGeometryCache();
		syncLocalTransformCache();
		notifyRectGeometryChanged();
	}
	public void setSize(Vector2 size)
	{
		ensureInit();
		Rect currentRect = mRectTransform.rect;
		if (currentRect.size == size)
		{
			return;
		}
		// RectTransform满足 rect.size = parentAnchorSpan + sizeDelta。
		// 已知当前rect.size与当前sizeDelta后，直接用尺寸差推导目标sizeDelta。
		Vector2 targetSizeDelta = mRectTransform.sizeDelta + (size - currentRect.size);
		mSuppressRectTransformDimensionsCallback = true;
		try
		{
			mRectTransform.sizeDelta = targetSizeDelta;
		}
		finally
		{
			mSuppressRectTransformDimensionsCallback = false;
		}
		refreshLocalGeometryCache();
		notifyRectGeometryChanged();
	}
	public Vector2 getSize()
	{
		ensureInit();
		return mRectTransform.rect.size;
	}
	public void notifyTransformChanged()
	{
		ensureInit();
		syncLocalTransformCache();
		if (mCanvas != null)
		{
			mCanvas.notifyElementTransformDirty(this);
		}
	}
	public void notifyTransformMatrixChanged(Matrix4x4 oldLocalToWorld)
	{
		ensureInit();
		syncLocalTransformCache();
		if (mCanvas != null)
		{
			mCanvas.notifyElementTransformMatrixChanged(this, oldLocalToWorld, mRectTransform.localToWorldMatrix);
		}
	}
	public void notifyPositionChanged(Vector3 oldLocalPosition)
	{
		ensureInit();
		Vector3 newLocalPosition = mRectTransform.localPosition;
		mCachedLocalPosition = newLocalPosition;
		if (mCanvas != null)
		{
			mCanvas.notifyElementPositionDeltaFast(this, mVertexSlot, mCanvasRenderCount, newLocalPosition - oldLocalPosition);
		}
	}
	public void notifyGeometryChanged()
	{
		ensureInit();
		refreshLocalGeometryCache();
		syncLocalTransformCache();
		if (mCanvas != null)
		{
			mCanvas.notifyElementGeometryDirty(this);
		}
	}
	public void notifyHierarchyChanged()
	{
		if (mCanvas != null)
		{
			mCanvas.notifyElementHierarchyOrderChanged(this);
		}
	}
	public void setSiblingIndex(int index)
	{
		if (transform.GetSiblingIndex() == index)
		{
			return;
		}
		transform.SetSiblingIndex(index);
		if (mCanvas != null)
		{
			mCanvas.notifyElementHierarchyOrderChanged(this);
		}
	}
	public void setAsFirstSibling()
	{
		if (transform.GetSiblingIndex() == 0)
		{
			return;
		}
		transform.SetAsFirstSibling();
		if (mCanvas != null)
		{
			mCanvas.notifyElementHierarchyOrderChanged(this);
		}
	}
	public void setAsLastSibling()
	{
		Transform parent = transform.parent;
		if (parent == null || transform.GetSiblingIndex() == parent.childCount - 1)
		{
			return;
		}
		transform.SetAsLastSibling();
		if (mCanvas != null)
		{
			mCanvas.notifyElementHierarchyOrderChanged(this);
		}
	}
	public void refreshCanvas()
	{
		ensureInit();
		registerCanvas();
	}
	public virtual Material getSourceRenderMaterial(Material defaultMaterial)
	{
		return mMaterial != null ? mMaterial : defaultMaterial;
	}
	public Material getRenderMaterial(Material defaultMaterial)
	{
		Material sourceMaterial = getSourceRenderMaterial(defaultMaterial);
		if (mMaskState.mMode == FastUIMaskMaterialMode.None || sourceMaterial == null)
		{
			releaseMaskRuntimeMaterial();
			return sourceMaterial;
		}
		if (mMaskRuntimeMaterial != null && mMaskRuntimeSourceMaterial == sourceMaterial)
		{
			return mMaskRuntimeMaterial;
		}
		releaseMaskRuntimeMaterial();
		mMaskRuntimeSourceMaterial = sourceMaterial;
		mMaskRuntimeMaterial = FastUIMaskMaterialCache.acquire(sourceMaterial, mMaskState);
		return mMaskRuntimeMaterial != null ? mMaskRuntimeMaterial : sourceMaterial;
	}
	public void notifyRenderMaterialPropertiesChanged()
	{
		Material sourceMaterial = mMaterial != null ? mMaterial : (mCanvas != null ? mCanvas.getDefaultMaterial() : null);
		FastUITextRenderMaterialCache.refresh(sourceMaterial);
		FastUIMaskMaterialCache.refresh(sourceMaterial);
		releaseMaskRuntimeMaterial();
		notifyBatchChanged();
	}
	public void setVertexSlot(int slot)
	{
		mVertexSlot = slot;
	}
	public void setRenderOrderIndex(int index)
	{
		mRenderOrderIndex = index;
	}
	public void setCanvasRenderRange(int renderStart, int renderCount)
	{
		mCanvasRenderStart = renderStart;
		mCanvasRenderCount = renderCount;
	}
	public void notifyAttachedMaskGeometryChanged()
	{
		if (mAttachedMask != null)
		{
			mAttachedMask.notifyGraphicGeometryChanged();
		}
	}
	public void refreshTransformCacheFromHierarchyChange()
	{
		syncLocalTransformCache();
		refreshDirectChildMode();
	}
	public void refreshPositionCacheFromExternalChange()
	{
		mCachedLocalPosition = mRectTransform.localPosition;
		mCachedGeometryLocalToCanvasMatrixValid = false;
	}
	public void refreshTransformCacheFromExternalChange()
	{
		syncLocalTransformCache();
	}
	protected Vector3 getRenderOriginOffset()
	{
		return mCanvas != null ? mCanvas.getRenderOriginOffset() : Vector3.zero;
	}
	protected Vector3 transformGeometryPoint(Matrix4x4 canvasWorldToLocal, Vector3 localPoint)
	{
		if (mDirectChildOfCanvas)
		{
			if (mCachedLocalRotation == Quaternion.identity && mCachedLocalScale == Vector3.one)
			{
				return localPoint + mCachedLocalPosition - getRenderOriginOffset();
			}
			return transformLocalPoint(localPoint);
		}
		Matrix4x4 matrix = canvasWorldToLocal * mRectTransform.localToWorldMatrix;
		return matrix.MultiplyPoint3x4(localPoint);
	}
	protected bool tryGetGeometryTranslation(out Vector3 translation)
	{
		if (mDirectChildOfCanvas && mCachedLocalRotation == Quaternion.identity && mCachedLocalScale == Vector3.one)
		{
			translation = mCachedLocalPosition - getRenderOriginOffset();
			return true;
		}
		translation = Vector3.zero;
		return false;
	}
	protected Matrix4x4 getGeometryLocalToCanvasMatrix(Matrix4x4 canvasWorldToLocal)
	{
		return getGeometryLocalToCanvasMatrix(canvasWorldToLocal, false);
	}
	protected Matrix4x4 getGeometryLocalToCanvasMatrix(Matrix4x4 geometryWorldToLocal, bool useCachedMatrix)
	{
		Vector3 renderOrigin = getRenderOriginOffset();
		if (mDirectChildOfCanvas)
		{
			return Matrix4x4.TRS(mCachedLocalPosition - renderOrigin, mCachedLocalRotation, mCachedLocalScale);
		}
		int canvasMatrixCacheVersion = mCanvas != null ? mCanvas.getGeometryMatrixCacheVersion() : 0;
		bool cacheValid = mCachedGeometryLocalToCanvasMatrixValid && mCachedGeometryLocalToCanvasMatrixVersion == canvasMatrixCacheVersion;
		if (!useCachedMatrix || !cacheValid)
		{
			Matrix4x4 canvasWorldToLocal = geometryWorldToLocal;
			canvasWorldToLocal.m03 += renderOrigin.x;
			canvasWorldToLocal.m13 += renderOrigin.y;
			canvasWorldToLocal.m23 += renderOrigin.z;
			mCachedGeometryLocalToCanvasMatrix = canvasWorldToLocal * mRectTransform.localToWorldMatrix;
			mCachedGeometryLocalToCanvasMatrixValid = true;
			mCachedGeometryLocalToCanvasMatrixVersion = canvasMatrixCacheVersion;
			mCachedGeometryPositionTranslationSerial = mCanvas != null ? mCanvas.getGeometryMatrixPositionTranslationSerial() : 0;
		}
		else if (mCanvas != null)
		{
			int translationSerial = mCanvas.applyGeometryMatrixPositionTranslations(mRenderOrderIndex, mCachedGeometryPositionTranslationSerial,
				ref mCachedGeometryLocalToCanvasMatrix, out bool translated);
			mCachedGeometryPositionTranslationSerial = translationSerial;
			if (translated)
			{
				++sGeometryMatrixPositionTranslationCarryHitCount;
			}
		}
		Matrix4x4 matrix = mCachedGeometryLocalToCanvasMatrix;
		matrix.m03 -= renderOrigin.x;
		matrix.m13 -= renderOrigin.y;
		matrix.m23 -= renderOrigin.z;
		return matrix;
	}
	public void invalidateCachedGeometryLocalToCanvasMatrix()
	{
		mCachedGeometryLocalToCanvasMatrixValid = false;
	}
	protected Vector3 transformLocalPoint(Vector3 point)
	{
		Vector3 scaled = new(point.x * mCachedLocalScale.x, point.y * mCachedLocalScale.y, point.z * mCachedLocalScale.z);
		return mCachedLocalPosition + mCachedLocalRotation * scaled - getRenderOriginOffset();
	}
	private void calculateCachedSimpleQuadPositionGeometry(Matrix4x4 canvasWorldToLocal, out Vector3 bottomLeft, out Vector3 topLeft, out Vector3 topRight, out Vector3 bottomRight)
	{
		if (mDirectChildOfCanvas)
		{
			if (mCachedLocalRotation == Quaternion.identity && mCachedLocalScale == Vector3.one)
			{
				Vector3 offset = mCachedLocalPosition - getRenderOriginOffset();
				bottomLeft = mLocalBottomLeft + offset;
				topLeft = mLocalTopLeft + offset;
				topRight = mLocalTopRight + offset;
				bottomRight = mLocalBottomRight + offset;
				return;
			}
			bottomLeft = transformLocalPoint(mLocalBottomLeft);
			topLeft = transformLocalPoint(mLocalTopLeft);
			topRight = transformLocalPoint(mLocalTopRight);
			bottomRight = transformLocalPoint(mLocalBottomRight);
			return;
		}
		Matrix4x4 matrix = getGeometryLocalToCanvasMatrix(canvasWorldToLocal, true);
		bottomLeft = matrix.MultiplyPoint3x4(mLocalBottomLeft);
		topLeft = matrix.MultiplyPoint3x4(mLocalTopLeft);
		topRight = matrix.MultiplyPoint3x4(mLocalTopRight);
		bottomRight = matrix.MultiplyPoint3x4(mLocalBottomRight);
	}
	// ：Runtime SimpleQuad的Matrix路径利用“局部几何恒为z=0矩形”这一既有语义。
	// 只变换左下角，再用Matrix的X/Y basis推导其余三角；不新增缓存、不改变顶点顺序或提交几何。
	protected void calculateRuntimeCachedSimpleQuadPositionGeometry(Matrix4x4 canvasWorldToLocal, out Vector3 bottomLeft, out Vector3 topLeft, out Vector3 topRight, out Vector3 bottomRight)
	{
		if (mDirectChildOfCanvas || mCanvas == null)
		{
			calculateCachedSimpleQuadPositionGeometry(canvasWorldToLocal, out bottomLeft, out topLeft, out topRight, out bottomRight);
			return;
		}
		Matrix4x4 matrix = getGeometryLocalToCanvasMatrix(canvasWorldToLocal, true);
		float xMin = mLocalBottomLeft.x;
		float yMin = mLocalBottomLeft.y;
		float width = mLocalBottomRight.x - xMin;
		float height = mLocalTopLeft.y - yMin;
		bottomLeft = new Vector3(
			matrix.m00 * xMin + matrix.m01 * yMin + matrix.m03,
			matrix.m10 * xMin + matrix.m11 * yMin + matrix.m13,
			matrix.m20 * xMin + matrix.m21 * yMin + matrix.m23);
		Vector3 xAxis = new(matrix.m00 * width, matrix.m10 * width, matrix.m20 * width);
		Vector3 yAxis = new(matrix.m01 * height, matrix.m11 * height, matrix.m21 * height);
		bottomRight = bottomLeft + xAxis;
		topLeft = bottomLeft + yAxis;
		topRight = bottomRight + yAxis;
	}
	protected virtual void refreshLocalGeometryCache()
	{
		Rect rect = mRectTransform.rect;
		mCachedRect = rect;
		mLocalBottomLeft = new Vector3(rect.xMin, rect.yMin, 0.0f);
		mLocalTopLeft = new Vector3(rect.xMin, rect.yMax, 0.0f);
		mLocalTopRight = new Vector3(rect.xMax, rect.yMax, 0.0f);
		mLocalBottomRight = new Vector3(rect.xMax, rect.yMin, 0.0f);
	}
	protected void syncLocalTransformCache()
	{
		mCachedLocalPosition = mRectTransform.localPosition;
		mCachedLocalRotation = mRectTransform.localRotation;
		mCachedLocalScale = mRectTransform.localScale;
		mCachedGeometryLocalToCanvasMatrixValid = false;
	}
	// Rect尺寸变化时，如果当前节点没有可渲染后代，只需更新自身Geometry。
	// 带可渲染子树的节点仍使用完整Geometry通知，使Anchor/Pivot影响继续正确传播到后代。
	private void notifyRectGeometryChanged()
	{
		if (mCanvas == null)
		{
			return;
		}
		if (mCanvasRenderCount == 1)
		{
			mCanvas.notifyElementOwnGeometryDirty(this);
		}
		else
		{
			mCanvas.notifyElementGeometryDirty(this);
		}
	}
	protected void markVertexDirty(FastUIDirtyFlags flags)
	{
		if (mCanvas != null)
		{
			mCanvas.markElementDirty(this, flags);
		}
	}
	protected void notifyBatchChanged()
	{
		if (mCanvas != null)
		{
			mCanvas.notifyElementBatchChanged(this);
		}
		if (mAttachedMask != null)
		{
			mAttachedMask.notifyGraphicBatchChanged();
		}
	}
	private void setRenderActiveState(bool active)
	{
		if (mRenderActive == active)
		{
			return;
		}
		bool oldActive = mRenderActive;
		mRenderActive = active;
		if (mCanvas != null)
		{
			mCanvas.notifyElementActiveChanged(this, oldActive, active);
		}
	}
	private bool refreshChangedDataAfterEnable()
	{
		if (mRectTransform == null || mCanvas == null || mVertexSlot < 0)
		{
			return false;
		}
		FastUIDirtyFlags flags = FastUIDirtyFlags.None;
		if (mCachedLocalPosition != mRectTransform.localPosition || mCachedLocalRotation != mRectTransform.localRotation || mCachedLocalScale != mRectTransform.localScale)
		{
			flags |= FastUIDirtyFlags.Transform;
		}
		if (mCachedRect != mRectTransform.rect)
		{
			flags |= FastUIDirtyFlags.Geometry | FastUIDirtyFlags.Transform;
		}
		if (flags == FastUIDirtyFlags.None)
		{
			return false;
		}
		refreshLocalGeometryCache();
		syncLocalTransformCache();
		mCanvas.markElementDirty(this, flags);
		return true;
	}
	public bool bindCloneCanvas(FastCanvas canvas, bool registerIfActive, bool deferRegistration = false)
	{
		ensureInit();
		if (canvas == null || mCanvas != null && mCanvas != canvas)
		{
			return false;
		}
		mCanvas = canvas;
		mCanvasHierarchySuspended = false;
		refreshDirectChildMode();
		if (registerIfActive)
		{
			mRenderActive = true;
		}
		if (registerIfActive && mVertexSlot < 0 && !deferRegistration)
		{
			mCanvas.registerClone(this);
			if (!mVisible)
			{
				mCanvas.setVisibilityRootVisible(mRectTransform, false);
			}
		}
		return true;
	}
	public bool tryMarkCloneRegistrationPending()
	{
		if (mVertexSlot >= 0 || mCloneRegistrationPending)
		{
			return false;
		}
		mCloneRegistrationPending = true;
		return true;
	}
	public void clearCloneRegistrationPending()
	{
		mCloneRegistrationPending = false;
	}
	private void registerCanvas()
	{
		FastCanvas canvas = findCanvas();
		if (mCanvas == canvas)
		{
			refreshDirectChildMode();
			if (mCanvas != null && mVertexSlot < 0)
			{
				mCanvas.register(this);
				if (!mVisible)
				{
					mCanvas.setVisible(mRectTransform, false);
				}
			}
			return;
		}
		if (mCanvas != null)
		{
			mCanvas.removeVisibilityRoot(mRectTransform);
			mCanvas.unregister(this);
		}
		mCanvas = canvas;
		mCanvasHierarchySuspended = false;
		mVertexSlot = -1;
		mRenderOrderIndex = -1;
		mCanvasRenderStart = -1;
		mCanvasRenderCount = 1;
		refreshDirectChildMode();
		if (mCanvas != null)
		{
			mCanvas.register(this);
			if (!mVisible)
			{
				mCanvas.setVisible(mRectTransform, false);
			}
		}
	}
	protected void unregisterCanvas()
	{
		mCloneRegistrationPending = false;
		if (mCanvas == null)
		{
			return;
		}
		FastCanvas canvas = mCanvas;
		mCanvas = null;
		mCanvasHierarchySuspended = false;
		canvas.removeVisibilityRoot(mRectTransform);
		canvas.unregister(this);
		mVertexSlot = -1;
		mRenderOrderIndex = -1;
		mCanvasRenderStart = -1;
		mCanvasRenderCount = 1;
		mDirectChildOfCanvas = false;
	}
	private FastCanvas findCanvas()
	{
		// GetComponentInParent在Unity原生侧一次完成祖先遍历；自定义单Canvas IsChildOf路径实测创建成本更高，因此使用原生最近祖先查找。
		Transform parent = mRectTransform != null ? mRectTransform.parent : transform.parent;
		return parent != null ? parent.GetComponentInParent<FastCanvas>(true) : null;
	}
	private void refreshDirectChildMode()
	{
		mCachedGeometryLocalToCanvasMatrixValid = false;
		mDirectChildOfCanvas = mCanvas != null && mRectTransform != null && mRectTransform.parent == mCanvas.transform;
	}
	protected void releaseMaskRuntimeMaterial()
	{
		if (mMaskRuntimeMaterial != null && mMaskRuntimeSourceMaterial != null)
		{
			FastUIMaskMaterialCache.release(mMaskRuntimeSourceMaterial, mMaskState, mMaskRuntimeMaterial);
		}
		mMaskRuntimeMaterial = null;
		mMaskRuntimeSourceMaterial = null;
	}
	protected void ensureInit()
	{
		if (mInitialized)
		{
			return;
		}
		// RequireComponent保证Transform就是RectTransform，不再做一次GetComponent组件表查询。
		mRectTransform = transform as RectTransform;
		refreshLocalGeometryCache();
		syncLocalTransformCache();
		mRenderActive = isActiveAndEnabled;
		mInitialized = true;
		// 此时Canvas尚未绑定，refreshDirectChildMode必然得到false；registerCanvas绑定后会统一刷新。
	}
	private bool isStencilBarrierState(FastUIMaskState state)
	{
		return state.mMode == FastUIMaskMaterialMode.Writer || state.mMode == FastUIMaskMaterialMode.Pop;
	}
}
