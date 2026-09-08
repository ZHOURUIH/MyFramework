using UnityEngine;

[DisallowMultipleComponent]
[AddComponentMenu("FastGUI/Mask/Fast Mask")]
// 异形/Graphic裁剪组件。绝大多数矩形裁剪优先使用FastRectMask2D。
public class FastMask : MonoBehaviour
{
	[SerializeField] protected bool mShowMaskGraphic = true;
	protected FastUIRenderElement mGraphic;
	protected FastMaskPopGraphic mPopGraphic;
	protected bool mInitialized;
	protected bool mChangingChildren;
	protected int mReportedDepth = -1;
	private void Awake()
	{
		ensureInit();
	}
	private void OnEnable()
	{
		ensureInit();
		if (mGraphic == null)
		{
			return;
		}
		mGraphic.setAttachedMask(this);
		ensurePopGraphic();
		refreshMaskStates();
	}
	private void OnDisable()
	{
		if (!mInitialized)
		{
			return;
		}
		if (mPopGraphic != null)
		{
			mPopGraphic.gameObject.SetActive(false);
		}
		if (mGraphic != null)
		{
			mGraphic.clearAttachedMask(this);
		}
		FastMaskUtility.refreshSubtree(transform);
	}
	private void OnDestroy()
	{
		if (mGraphic != null)
		{
			mGraphic.clearAttachedMask(this);
		}
		if (mPopGraphic != null)
		{
			GameObject popObject = mPopGraphic.gameObject;
			mPopGraphic = null;
			if (Application.isPlaying)
			{
				Destroy(popObject);
			}
			else
			{
				DestroyImmediate(popObject);
			}
		}
		FastMaskUtility.refreshSubtree(transform);
	}
	private void OnTransformParentChanged()
	{
		if (isActiveAndEnabled)
		{
			refreshMaskStates();
		}
	}
	private void OnTransformChildrenChanged()
	{
		if (mChangingChildren || !isActiveAndEnabled)
		{
			return;
		}
		ensurePopGraphic();
		keepPopLast();
		FastMaskUtility.refreshSubtree(transform);
	}
	public FastUIRenderElement getGraphic()
	{
		ensureInit();
		return mGraphic;
	}
	public FastMaskPopGraphic getPopGraphic()
	{
		return mPopGraphic;
	}
	public bool getShowMaskGraphic()
	{
		return mShowMaskGraphic;
	}
	public void setShowMaskGraphic(bool show)
	{
		if (mShowMaskGraphic == show)
		{
			return;
		}
		mShowMaskGraphic = show;
		refreshMaskStates();
	}
	public bool isMaskBasicActiveForCanvas(FastCanvas canvas)
	{
		ensureInit();
		return canvas != null && isActiveAndEnabled && mGraphic != null && mGraphic.isActiveAndEnabled && mGraphic.isRenderActive() &&
			mGraphic.getCanvas() == canvas;
	}
	public bool isMaskActive()
	{
		ensureInit();
		FastCanvas canvas = mGraphic != null ? mGraphic.getCanvas() : null;
		return isMaskBasicActiveForCanvas(canvas) && getDepth() < FastMaskUtility.MAX_MASK_DEPTH;
	}
	public int getDepth()
	{
		FastCanvas canvas = mGraphic != null ? mGraphic.getCanvas() : null;
		int depth = FastMaskUtility.countActiveMasks(transform.parent, canvas);
		if (TryGetComponent(out FastRectMask2D clip) && clip.isClipActiveForCanvas(canvas))
		{
			++depth;
		}
		return depth;
	}
	public void refreshMaskStates()
	{
		if (!isActiveAndEnabled || mGraphic == null)
		{
			return;
		}
		ensurePopGraphic();
		int parentDepth = getDepth();
		if (parentDepth >= FastMaskUtility.MAX_MASK_DEPTH)
		{
			reportDepthOverflow(parentDepth + 1);
			if (mPopGraphic != null)
			{
				mPopGraphic.gameObject.SetActive(false);
			}
			FastMaskUtility.refreshSubtree(transform);
			return;
		}
		if (mPopGraphic != null)
		{
			if (!mPopGraphic.gameObject.activeSelf)
			{
				mPopGraphic.gameObject.SetActive(true);
			}
			mPopGraphic.setSource(mGraphic);
			mPopGraphic.setPopState(FastUIMaskState.pop(parentDepth));
			mPopGraphic.syncSourceColor();
			mPopGraphic.cull(mGraphic.isCulled());
		}
		keepPopLast();
		FastMaskUtility.refreshSubtree(transform);
	}
	public void notifyGraphicActiveChanged()
	{
		if (!isActiveAndEnabled)
		{
			return;
		}
		bool active = isMaskActive();
		if (mPopGraphic != null)
		{
			mPopGraphic.gameObject.SetActive(active);
		}
		FastMaskUtility.refreshSubtree(transform);
	}
	public void notifyGraphicBatchChanged()
	{
		if (mPopGraphic != null)
		{
			mPopGraphic.notifySourceBatchChanged();
		}
	}
	public void notifyGraphicColorChanged()
	{
		if (mPopGraphic != null)
		{
			mPopGraphic.syncSourceColor();
		}
	}
	public void notifyGraphicGeometryChanged()
	{
		if (mPopGraphic != null)
		{
			mPopGraphic.notifySourceGeometryChanged();
		}
	}
	public void notifyGraphicCullChanged()
	{
		if (mPopGraphic != null && mGraphic != null)
		{
			mPopGraphic.cull(mGraphic.isCulled());
		}
	}
	public void detachGraphic(FastUIRenderElement graphic)
	{
		if (mGraphic != graphic)
		{
			return;
		}
		mGraphic = null;
		if (mPopGraphic != null)
		{
			mPopGraphic.gameObject.SetActive(false);
		}
	}
	public void reportDepthOverflow(int depth)
	{
		if (mReportedDepth == depth)
		{
			return;
		}
		mReportedDepth = depth;
		Debug.LogError("[FastMask] Stencil最多支持" + FastMaskUtility.MAX_MASK_DEPTH + "层嵌套,当前深度:" + depth + ",节点:" + name);
	}
	private void ensureInit()
	{
		if (mInitialized)
		{
			return;
		}
		mGraphic = GetComponent<FastUIRenderElement>();
		mInitialized = true;
		if (mGraphic == null)
		{
			Debug.LogError("[FastMask] FastMask必须和FastUIRenderElement放在同一个GameObject:" + name);
		}
	}
	private void ensurePopGraphic()
	{
		if (mGraphic == null || mPopGraphic != null)
		{
			return;
		}
		mChangingChildren = true;
		GameObject popObject = new("__FastMaskPop", typeof(RectTransform))
		{
			hideFlags = HideFlags.HideAndDontSave
		};
		RectTransform popTransform = (RectTransform)popObject.transform;
		popTransform.SetParent(transform, false);
		popTransform.anchorMin = Vector2.zero;
		popTransform.anchorMax = Vector2.one;
		popTransform.pivot = mGraphic.getRectTransform().pivot;
		popTransform.sizeDelta = Vector2.zero;
		popTransform.anchoredPosition = Vector2.zero;
		popTransform.localPosition = Vector3.zero;
		popTransform.localRotation = Quaternion.identity;
		popTransform.localScale = Vector3.one;
		mPopGraphic = popObject.AddComponent<FastMaskPopGraphic>();
		mPopGraphic.setSource(mGraphic);
		mChangingChildren = false;
		keepPopLast();
	}
	private void keepPopLast()
	{
		if (mPopGraphic == null)
		{
			return;
		}
		int targetIndex = transform.childCount - 1;
		if (TryGetComponent(out FastRectMask2D clip))
		{
			FastClipStencilGraphic clipPop = clip.getPopGraphic();
			if (clipPop != null && clipPop.gameObject.activeSelf)
			{
				targetIndex = Mathf.Max(0, targetIndex - 1);
			}
		}
		Transform popTransform = mPopGraphic.transform;
		if (popTransform.GetSiblingIndex() == targetIndex)
		{
			return;
		}
		mChangingChildren = true;
		popTransform.SetSiblingIndex(targetIndex);
		mChangingChildren = false;
		mPopGraphic.notifyHierarchyChanged();
	}
}
