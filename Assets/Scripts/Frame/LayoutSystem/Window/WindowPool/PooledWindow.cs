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
	public bool isVisible() { return mRoot.isActive(); }
	public virtual void setVisible(bool visible) { LT.ACTIVE(mRoot, visible); }
	public void setAsFirstSibling(bool notifyLayout = true) { mRoot.setAsFirstSibling(notifyLayout); }
	public void setAsLastSibling(bool notifyLayout = true) { mRoot.setAsLastSibling(notifyLayout); }
	public void setParent(myUIObject parent, bool needSortChild = true) { mRoot.setParent(parent, needSortChild); }
	public myUIObject getRoot() { return mRoot; }
}