using System;

// 可用于WindowObjectPool和WindowObjectPoolMap的类,常用于可回收复用的窗口
// 每次创建新的对象时都从template克隆
public abstract class PooledWindow : FrameBase
{
	protected LayoutScript mScript;
	protected myUIObject mRoot;		// 需要在子类中被赋值
	public virtual void setScript(LayoutScript script) { mScript = script; }
	public abstract void assignWindow(myUIObject parent, myUIObject template, string name);
	public virtual void init() { }
	public virtual void destroy() { }
	public virtual void reset() { }
	public virtual void recycle() { }
	public bool isVisible() 
	{
		checkRoot();
		return mRoot.isActive(); 
	}
	public virtual void setVisible(bool visible) 
	{
		checkRoot();
		LT.ACTIVE(mRoot, visible); 
	}
	public void setAsFirstSibling(bool needSortChild = false, bool refreshUIDepth = true)
	{
		checkRoot();
		mRoot.setAsFirstSibling(needSortChild, refreshUIDepth);
	}
	public void setAsLastSibling(bool needSortChild = false, bool refreshUIDepth = true) 
	{
		checkRoot();
		mRoot.setAsLastSibling(needSortChild, refreshUIDepth);
	}
	public void setParent(myUIObject parent, bool needSortChild = false, bool notifyLayout = true) 
	{
		checkRoot();
		mRoot.setParent(parent, needSortChild, notifyLayout); 
	}
	public myUIObject getRoot() { return mRoot; }
	//------------------------------------------------------------------------------------------------------------------------------
	protected void checkRoot()
	{
		if(mRoot == null)
		{
			logError("可复用窗口的mRoot为空,请确保在assignWindow中已经给mRoot赋值了");
		}
	}
}