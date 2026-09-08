using UnityEngine;

// FastGUI层级裁剪组件。
// 裁剪通过Stencil实现，后代元素保持原始Geometry；父节点滚动/平移继续走PositionDelta FastPath，不再做CPU几何裁剪。
[DefaultExecutionOrder(-100)]
[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
[AddComponentMenu("FastGUI/Mask/Fast Rect Mask 2D")]
// FastGUI默认推荐的矩形裁剪组件。使用方式对齐UGUI RectMask2D，底层保持FastGUI专用Stencil快路径。
public class FastRectMask2D : MonoBehaviour
{
	[SerializeField] protected Vector4 mPadding;
	protected RectTransform mRectTransform;
	protected FastCanvas mCanvas;
	protected FastClipStencilGraphic mWriterGraphic;
	protected FastClipStencilGraphic mPopGraphic;
	protected Rect mCachedRect;
	protected Matrix4x4 mCachedLocalToWorld;
	protected Vector4 mCachedPadding;
	protected bool mInitialized;
	protected bool mChangingChildren;
	protected bool mCanvasHierarchySuspended;
	protected int mCachedParentStencilDepth = -1;
	protected int mReportedDepth = -1;
	private void Awake()
	{
		// Active创建时OnEnable紧随Awake执行完整Stencil初始化；这里不重复创建/注册/刷新。
		ensureInit();
	}
	private void OnEnable()
	{
		ensureInit();
		// 整个FastCanvas层级恢复时保持注册关系和Stencil activeSelf不变，
		// 由Canvas下一次LateUpdate只扫描Clip Root做轻量校验。
		if (mCanvasHierarchySuspended && mCanvas != null)
		{
			return;
		}
		refreshCanvas();
		ensureStencilGraphics();
		cacheState();
		refreshStencilStates();
	}
	private void OnDisable()
	{
		// Canvas整体SetActive(false)会触发所有子MonoBehaviour的OnDisable。
		// 这里不再反复注销Clip、关闭Writer/Pop再在恢复时重建整套关系。
		if (mCanvas != null && !mCanvas.gameObject.activeInHierarchy)
		{
			mCanvasHierarchySuspended = true;
			return;
		}
		mCanvasHierarchySuspended = false;
		setStencilGraphicsActive(false);
		unregisterCanvas();
	}
	private void OnDestroy()
	{
		setStencilGraphicsActive(false);
		unregisterCanvas();
		destroyStencilGraphics();
	}
	private void OnTransformParentChanged()
	{
		if (!mInitialized)
		{
			return;
		}
		FastCanvas oldCanvas = mCanvas;
		refreshCanvas();
		cacheState();
		refreshStencilStates();
		if (mCanvas != null && oldCanvas == mCanvas && !TryGetComponent<FastUIRenderElement>(out _))
		{
			mCanvas.notifyTransformHierarchyOrderChanged(mRectTransform);
		}
	}
	private void OnTransformChildrenChanged()
	{
		if (mChangingChildren || !isActiveAndEnabled)
		{
			return;
		}
		ensureStencilGraphics();
		keepStencilOrder();
		if (mCanvas != null)
		{
			mCanvas.markClipStructureDirty();
		}
	}
	private void OnRectTransformDimensionsChange()
	{
		if (!mInitialized)
		{
			return;
		}
		cacheState();
		notifyStencilGeometryChanged();
	}
	private void OnDidApplyAnimationProperties()
	{
		if (!mInitialized)
		{
			return;
		}
		cacheState();
		notifyStencilGeometryChanged();
	}
	// 外部直接修改RectTransform时只更新Mask自己的Writer/Pop Geometry，后代顶点不再因为Clip变化重建。
	private void LateUpdate()
	{
		if (!mInitialized || mCanvas == null)
		{
			return;
		}
		Rect rect = mRectTransform.rect;
		Matrix4x4 matrix = mRectTransform.localToWorldMatrix;
		if (rect == mCachedRect && matrix == mCachedLocalToWorld && mPadding == mCachedPadding)
		{
			return;
		}
		mCachedRect = rect;
		mCachedLocalToWorld = matrix;
		mCachedPadding = mPadding;
		notifyStencilGeometryChanged();
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
	public Vector4 getPadding()
	{
		return mPadding;
	}
	public FastClipStencilGraphic getWriterGraphic()
	{
		return mWriterGraphic;
	}
	public FastClipStencilGraphic getPopGraphic()
	{
		return mPopGraphic;
	}
	public void setPadding(Vector4 padding)
	{
		if (mPadding == padding)
		{
			return;
		}
		mPadding = padding;
		mCachedPadding = padding;
		notifyStencilGeometryChanged();
	}
	// Padding顺序固定为Left/Bottom/Right/Top，正值表示向内收缩Stencil Writer Geometry。
	public Rect getClipRect()
	{
		ensureInit();
		Rect rect = mRectTransform.rect;
		float xMin = rect.xMin + mPadding.x;
		float yMin = rect.yMin + mPadding.y;
		float xMax = rect.xMax - mPadding.z;
		float yMax = rect.yMax - mPadding.w;
		if (xMax <= xMin || yMax <= yMin)
		{
			return new Rect(xMin, yMin, 0.0f, 0.0f);
		}
		return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
	}
	public int getDepth()
	{
		return FastMaskUtility.countActiveMasks(transform.parent, mCanvas);
	}
	public bool isClipBasicActiveForCanvas(FastCanvas canvas)
	{
		return canvas != null && isActiveAndEnabled && mCanvas == canvas;
	}
	public bool isClipActiveForCanvas(FastCanvas canvas)
	{
		return isClipBasicActiveForCanvas(canvas) && getDepth() < FastMaskUtility.MAX_MASK_DEPTH;
	}
	public void refreshCanvas()
	{
		ensureInit();
		FastCanvas canvas = findCanvas();
		if (mCanvas == canvas)
		{
			if (mCanvas != null && isActiveAndEnabled)
			{
				mCanvas.registerRectMask(this);
			}
			refreshStencilGraphicCanvases();
			return;
		}
		unregisterCanvas();
		mCanvas = canvas;
		if (mCanvas != null && isActiveAndEnabled)
		{
			mCanvas.registerRectMask(this);
		}
		refreshStencilGraphicCanvases();
	}
	public void notifyClipChanged()
	{
		ensureInit();
		cacheState();
		notifyStencilGeometryChanged();
	}
	public void notifyStencilGeometryChanged()
	{
		if (mWriterGraphic != null)
		{
			mWriterGraphic.notifySourceGeometryChanged();
		}
		if (mPopGraphic != null)
		{
			mPopGraphic.notifySourceGeometryChanged();
		}
		if (mCanvas != null)
		{
			mCanvas.markClipCullBoundsDirty();
		}
	}
	public void refreshStencilStates()
	{
		if (!isActiveAndEnabled || mCanvas == null)
		{
			setStencilGraphicsActive(false);
			return;
		}
		ensureStencilGraphics();
		int parentDepth = getDepth();
		mCachedParentStencilDepth = parentDepth;
		if (parentDepth >= FastMaskUtility.MAX_MASK_DEPTH)
		{
			reportDepthOverflow(parentDepth + 1);
			setStencilGraphicsActive(false);
			mCanvas.markClipStructureDirty();
			return;
		}
		mWriterGraphic.setStencilState(FastUIMaskState.writer(parentDepth, false));
		mPopGraphic.setStencilState(FastUIMaskState.pop(parentDepth));
		setStencilGraphicsActive(true);
		keepStencilOrder();
		mCanvas.markClipStructureDirty();
	}
	// Canvas整体恢复时只校验Clip Root自身，不扫描所有RenderElement。
	// 返回true表示Clip/Mask层级语义发生变化，需要FastUIClipSystem做一次完整Reader状态刷新。
	public bool syncAfterCanvasHierarchyResume(FastCanvas owner)
	{
		if (!mCanvasHierarchySuspended)
		{
			return false;
		}
		mCanvasHierarchySuspended = false;
		if (owner == null || !isActiveAndEnabled)
		{
			return true;
		}
		FastCanvas hierarchyCanvas = findCanvas();
		if (hierarchyCanvas != mCanvas || hierarchyCanvas != owner)
		{
			refreshCanvas();
			if (mCanvas != null && isActiveAndEnabled)
			{
				ensureStencilGraphics();
				cacheState();
				refreshStencilStates();
			}
			return true;
		}
		Rect rect = mRectTransform.rect;
		Matrix4x4 matrix = mRectTransform.localToWorldMatrix;
		bool geometryChanged = rect != mCachedRect || matrix != mCachedLocalToWorld || mPadding != mCachedPadding;
		if (geometryChanged)
		{
			mCachedRect = rect;
			mCachedLocalToWorld = matrix;
			mCachedPadding = mPadding;
			notifyStencilGeometryChanged();
		}
		int parentDepth = getDepth();
		if (parentDepth != mCachedParentStencilDepth)
		{
			refreshStencilStates();
			return true;
		}
		return false;
	}
	public void reportDepthOverflow(int depth)
	{
		if (mReportedDepth == depth)
		{
			return;
		}
		mReportedDepth = depth;
		Debug.LogError("[FastRectMask2D] Stencil最多支持" + FastMaskUtility.MAX_MASK_DEPTH + "层嵌套,当前深度:" + depth + ",节点:" + name);
	}
	private FastCanvas findCanvas()
	{
		return (mRectTransform != null ? (Component)mRectTransform : this).GetComponentInParent<FastCanvas>(true);
	}
	private void unregisterCanvas()
	{
		if (mCanvas == null)
		{
			return;
		}
		FastCanvas canvas = mCanvas;
		mCanvas = null;
		mCanvasHierarchySuspended = false;
		canvas.unregisterRectMask(this);
	}
	private void ensureInit()
	{
		if (mInitialized)
		{
			return;
		}
		mRectTransform = transform as RectTransform;
		mInitialized = true;
	}
	private void refreshStencilGraphicCanvases()
	{
		if (mWriterGraphic != null && mWriterGraphic.gameObject.activeInHierarchy)
		{
			mWriterGraphic.refreshCanvas();
		}
		if (mPopGraphic != null && mPopGraphic.gameObject.activeInHierarchy)
		{
			mPopGraphic.refreshCanvas();
		}
	}
	private void ensureStencilGraphics()
	{
		if (mWriterGraphic == null)
		{
			mWriterGraphic = createStencilGraphic("__FastClipStencilWriter", true);
		}
		if (mPopGraphic == null)
		{
			mPopGraphic = createStencilGraphic("__FastClipStencilPop", false);
		}
		keepStencilOrder();
	}
	private FastClipStencilGraphic createStencilGraphic(string objectName, bool writer)
	{
		mChangingChildren = true;
		GameObject graphicObject = new(objectName, typeof(RectTransform))
		{
			hideFlags = HideFlags.HideAndDontSave
		};
		graphicObject.SetActive(false);
		RectTransform rect = (RectTransform)graphicObject.transform;
		rect.SetParent(transform, false);
		rect.anchorMin = Vector2.zero;
		rect.anchorMax = Vector2.one;
		rect.pivot = new Vector2(0.5f, 0.5f);
		rect.sizeDelta = Vector2.zero;
		rect.anchoredPosition = Vector2.zero;
		rect.localPosition = Vector3.zero;
		rect.localRotation = Quaternion.identity;
		rect.localScale = Vector3.one;
		FastClipStencilGraphic graphic = graphicObject.AddComponent<FastClipStencilGraphic>();
		graphic.setSource(this, writer);
		mChangingChildren = false;
		return graphic;
	}
	private void keepStencilOrder()
	{
		if (mWriterGraphic == null || mPopGraphic == null)
		{
			return;
		}
		mChangingChildren = true;
		Transform writerTransform = mWriterGraphic.transform;
		Transform popTransform = mPopGraphic.transform;
		if (writerTransform.GetSiblingIndex() != 0)
		{
			writerTransform.SetAsFirstSibling();
			mWriterGraphic.notifyHierarchyChanged();
		}
		if (popTransform.GetSiblingIndex() != transform.childCount - 1)
		{
			popTransform.SetAsLastSibling();
			mPopGraphic.notifyHierarchyChanged();
		}
		mChangingChildren = false;
	}
	private void setStencilGraphicsActive(bool active)
	{
		if (mWriterGraphic != null && mWriterGraphic.gameObject.activeSelf != active)
		{
			mWriterGraphic.gameObject.SetActive(active);
		}
		if (mPopGraphic != null && mPopGraphic.gameObject.activeSelf != active)
		{
			mPopGraphic.gameObject.SetActive(active);
		}
	}
	private void destroyStencilGraphics()
	{
		destroyStencilGraphic(ref mWriterGraphic);
		destroyStencilGraphic(ref mPopGraphic);
	}
	private void destroyStencilGraphic(ref FastClipStencilGraphic graphic)
	{
		if (graphic == null)
		{
			return;
		}
		GameObject graphicObject = graphic.gameObject;
		graphic = null;
		if (Application.isPlaying)
		{
			Destroy(graphicObject);
		}
		else
		{
			DestroyImmediate(graphicObject);
		}
	}
	private void cacheState()
	{
		ensureInit();
		mCachedRect = mRectTransform.rect;
		mCachedLocalToWorld = mRectTransform.localToWorldMatrix;
		mCachedPadding = mPadding;
	}
}
