using UnityEngine;

// FastUI层级可见性控制组件。
// 用于把一个Transform子树作为Visibility Root交给FastCanvas管理。

// 非渲染容器节点的专用可见性组件。
// 如果当前节点本身就是FastCanvas RenderBoundary,可见性直接绑定到自身Canvas并裁剪整组DrawRun.
// setVisible只控制FastUI绘制,不会修改GameObject.activeSelf,不会触发OnEnable/OnDisable.
[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
public class FastUIVisibility : MonoBehaviour
{
	[SerializeField] protected bool mVisible = true;
	protected RectTransform mRectTransform;
	protected FastCanvas mCanvas;
	protected bool mInitialized;
	private void Awake()
	{
		// Active对象创建后OnEnable会立即完成Canvas绑定，Awake只初始化，避免每个Item重复Hierarchy查找。
		ensureInit();
	}
	private void OnEnable()
	{
		ensureInit();
		if (!FastUICloneUtility.tryBindVisibility(this))
		{
			refreshCanvas();
		}
	}
	private void OnDestroy()
	{
		if (mCanvas != null)
		{
			mCanvas.removeVisibilityRoot(mRectTransform);
		}
		mCanvas = null;
	}
	private void OnTransformParentChanged()
	{
		if (!mInitialized)
		{
			return;
		}
		if (!FastUICloneUtility.tryBindVisibility(this))
		{
			refreshCanvas();
		}
	}
	public bool getVisible()
	{
		return mVisible;
	}
	public void setVisible(bool visible)
	{
		if (mVisible == visible)
		{
			return;
		}
		mVisible = visible;
		if (mCanvas != null)
		{
			mCanvas.setVisibilityRootVisible(mRectTransform, visible);
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
	public bool bindCloneCanvas(FastCanvas canvas)
	{
		ensureInit();
		bool changed = mCanvas != canvas;
		if (mCanvas != null && changed)
		{
			mCanvas.removeVisibilityRoot(mRectTransform);
		}
		mCanvas = canvas;
		if (mCanvas != null && !mVisible)
		{
			mCanvas.setVisibilityRootVisible(mRectTransform, false);
		}
		return changed;
	}
	public void refreshCanvas()
	{
		ensureInit();
		FastCanvas canvas = findCanvas();
		if (mCanvas == canvas)
		{
			if (mCanvas != null)
			{
				mCanvas.setVisibilityRootVisible(mRectTransform, mVisible);
			}
			return;
		}
		if (mCanvas != null)
		{
			mCanvas.removeVisibilityRoot(mRectTransform);
		}
		mCanvas = canvas;
		if (mCanvas != null)
		{
			mCanvas.setVisibilityRootVisible(mRectTransform, mVisible);
		}
	}
	private FastCanvas findCanvas()
	{
		return (mRectTransform != null ? (Component)mRectTransform : this).GetComponentInParent<FastCanvas>(true);
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
}
