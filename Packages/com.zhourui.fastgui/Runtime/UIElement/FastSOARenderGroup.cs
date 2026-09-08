using UnityEngine;

public enum FastSOAStructureMode
{
	FixedStructure,
	SparseStructure,
}

// 显式SOA渲染分组。
// 该节点的直接子节点视为Record。
// FixedStructure要求同一SOA维度下各Record的RenderElement结构一致，并支持Nested SOA完全展开。
// SparseStructure允许Record缺失或额外RenderElement，通过相对Hierarchy Role和局部先后约束构建Lane；当前SparseStructure当前只支持单层Group。
// 应用层只有在确认允许改变这些Record之间的绘制顺序时才添加该组件。
// AdjacentGroupMerge是额外显式契约：仅当开发者确认相邻SOA Group之间也允许互相穿插重排时开启。
[DefaultExecutionOrder(-90)]
[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
public class FastSOARenderGroup : MonoBehaviour
{
	[SerializeField] protected FastSOAStructureMode mStructureMode = FastSOAStructureMode.FixedStructure;
	[SerializeField] protected bool mAllowAdjacentGroupMerge;
	protected RectTransform mRectTransform;
	protected FastCanvas mCanvas;
	protected bool mInitialized;
	protected bool mCanvasHierarchySuspended;
	protected bool mSuspendedStructureDirty;
	protected bool mLastBuildValid = true;
	protected string mLastBuildError;
	private void Awake()
	{
		// OnEnable统一完成Canvas注册，避免Active创建时Awake+OnEnable重复Hierarchy解析。
		ensureInit();
	}
	private void OnEnable()
	{
		ensureInit();
		if (mCanvasHierarchySuspended && mCanvas != null)
		{
			mCanvasHierarchySuspended = false;
			if (mSuspendedStructureDirty)
			{
				mSuspendedStructureDirty = false;
				refreshCanvas();
				if (mCanvas != null)
				{
					mCanvas.markSOAStructureDirty();
				}
			}
			return;
		}
		refreshCanvas();
	}
	private void OnDisable()
	{
		// 整个FastCanvas关闭时保留SOA注册和已经构建好的Lane/DrawOrder。
		// SetActive恢复后如果层级没有真实变化，就不触发SOA Full Build。
		if (mCanvas != null && !mCanvas.gameObject.activeInHierarchy)
		{
			mCanvasHierarchySuspended = true;
			return;
		}
		mCanvasHierarchySuspended = false;
		mSuspendedStructureDirty = false;
		unregisterCanvas();
	}
	private void OnDestroy()
	{
		mCanvasHierarchySuspended = false;
		mSuspendedStructureDirty = false;
		unregisterCanvas();
	}
	private void OnTransformParentChanged()
	{
		if (!mInitialized)
		{
			return;
		}
		if (mCanvasHierarchySuspended)
		{
			mSuspendedStructureDirty = true;
			return;
		}
		refreshCanvas();
		if (mCanvas != null)
		{
			mCanvas.markSOAStructureDirty();
		}
	}
	private void OnTransformChildrenChanged()
	{
		if (mCanvasHierarchySuspended)
		{
			mSuspendedStructureDirty = true;
			return;
		}
		if (mCanvas != null)
		{
			if (mCanvas != null)
			{
				mCanvas.markSOAStructureDirty();
			}
		}
	}
	private void OnValidate()
	{
		if (mInitialized && mCanvas != null)
		{
			mCanvas.markSOAStructureDirty();
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
	public FastSOAStructureMode getStructureMode()
	{
		return mStructureMode;
	}
	public bool getAllowAdjacentGroupMerge()
	{
		return mAllowAdjacentGroupMerge;
	}
	public bool getLastBuildValid()
	{
		return mLastBuildValid;
	}
	public string getLastBuildError()
	{
		return mLastBuildError;
	}
	public void setStructureMode(FastSOAStructureMode mode)
	{
		if (mStructureMode == mode)
		{
			return;
		}
		mStructureMode = mode;
		if (mCanvas != null)
		{
			mCanvas.markSOAStructureDirty();
		}
	}
	public void setAllowAdjacentGroupMerge(bool allow)
	{
		if (mAllowAdjacentGroupMerge == allow)
		{
			return;
		}
		mAllowAdjacentGroupMerge = allow;
		if (mCanvas != null)
		{
			mCanvas.markSOAStructureDirty();
		}
	}
	public void notifyStructureChanged()
	{
		if (mCanvas != null)
		{
			mCanvas.markSOAStructureDirty();
		}
	}
	public void refreshCanvas()
	{
		ensureInit();
		FastCanvas canvas = findCanvas();
		if (mCanvas == canvas)
		{
			if (mCanvas != null && isActiveAndEnabled)
			{
				mCanvas.registerSOARenderGroup(this);
			}
			return;
		}
		unregisterCanvas();
		mCanvas = canvas;
		if (mCanvas != null && isActiveAndEnabled)
		{
			mCanvas.registerSOARenderGroup(this);
		}
	}
	public void setBuildResult(bool valid, string error)
	{
		bool changed = mLastBuildValid != valid || mLastBuildError != error;
		mLastBuildValid = valid;
		mLastBuildError = error;
		if (!valid && changed)
		{
			string message = "FastSOARenderGroup构建失败,已回退Normal顺序,Root:" + name + ",原因:" + error;
			if (mStructureMode == FastSOAStructureMode.SparseStructure)
			{
				Debug.LogWarning(message, this);
			}
			else
			{
				Debug.LogError(message, this);
			}
		}
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
		canvas.unregisterSOARenderGroup(this);
	}
}
