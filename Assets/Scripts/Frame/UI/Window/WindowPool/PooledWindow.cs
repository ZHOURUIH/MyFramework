using System;

// 可用于WindowObjectPool和WindowObjectPoolMap的类,常用于可回收复用的窗口
// 每次创建新的对象时都从template克隆
public abstract class PooledWindow : FrameBase
{
	protected LayoutScript mScript;	// 所属的布局脚本
	public virtual void setScript(LayoutScript script) { mScript = script; }
	// 对象构造之后调用一次
	public abstract void assignWindow(myUIObject parent, myUIObject template, string name);
	// 对象构造之后调用一次
	public virtual void init() { }
	// 对象被真正从内存中销毁时调用
	public virtual void destroy() { }
	// 在创建或复用后的第一次显示之前调用
	public virtual void reset() { }
	// 在对象被回收时调用
	public virtual void recycle() { }
	public virtual void setVisible(bool visible) { }
	public virtual void setParent(myUIObject parent, bool refreshDepth = true) { }
	public virtual void setAsLastSibling(bool refreshDepth = true) { }
}