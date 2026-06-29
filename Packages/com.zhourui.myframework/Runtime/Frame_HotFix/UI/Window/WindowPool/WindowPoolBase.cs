using static UnityUtility;

public class WindowPoolBase
{
	protected UGUIObjectCallback mDestroyCallback;	// 窗口销毁的回调,用于让外部定义窗口的销毁方式
	protected LayoutScript mScript;                 // 所属布局脚本
	protected WindowObjectBase mOwnerObject;        // 如果是子UI创建的,则存储所属的UI子对象
	protected bool mAutoRefreshDepth;				// 在添加窗口后是否刷新窗口的深度,只在需要移动到最后一个子节点时才会生效
	protected bool mMoveToLast;                     // 新添加的窗口是否需要移动到父节点的最后一个子节点
	public WindowPoolBase(IWindowObjectOwner parent)
	{
		if (parent is WindowObjectBase objBase)
		{
			mScript = objBase.getScript();
			mOwnerObject = objBase;
			mOwnerObject.addWindowPool(this);
		}
		else if (parent is LayoutScript script)
		{
			mScript = script;
			mScript.addWindowPool(this);
		}
		else if (parent is WindowStructPoolBase)
		{
			logError("对象池的父节点不能是对象池");
		}
	}
	public virtual void init()
	{
		mAutoRefreshDepth = false;
		mMoveToLast = true;
	}
	public virtual void destroy() { }
	public virtual void unuseAll() { }
	public virtual int getInUseCount() { return 0; }
	public bool isRootPool() { return mOwnerObject == null; }
	public void setAutoRefreshDepth(bool autoRefreshDepth) { mAutoRefreshDepth = autoRefreshDepth; }
	public void setMoveToLast(bool moveToLast) { mMoveToLast = moveToLast; }
	public void setDestroyCallback(UGUIObjectCallback callback) { mDestroyCallback = callback; }
}