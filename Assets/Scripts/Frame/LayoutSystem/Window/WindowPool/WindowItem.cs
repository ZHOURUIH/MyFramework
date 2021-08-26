using System;
using System.Collections.Generic;
using UnityEngine;

// 用于固定数量类,不能用于回收复用窗口
// 通常只是用于已经在预设中创建好的窗口,创建对象时不会创建新的节点
public abstract class WindowItem : FrameBase
{
	protected LayoutScript mScript;	// 所属的布局脚本
	protected myUGUIObject mRoot;	// 根节点
	public virtual void setScript(LayoutScript script) { mScript = script; }
	public virtual void assignWindow(myUIObject parent, string name)
	{
		mScript.newObject(out mRoot, parent, name);
	}
	public virtual void init() { }
	public virtual void destroy() { }
	public virtual void reset() { }
	public virtual void recycle() { }
	public bool isVisible() { return mRoot.isActive(); }
	public void setVisible(bool visible) { LT.ACTIVE(mRoot, visible); }
	public void setAsFirstSibling(bool notifyLayout = true) { mRoot.setAsFirstSibling(notifyLayout); }
	public void setAsLastSibling(bool notifyLayout = true) { mRoot.setAsLastSibling(notifyLayout); }
	public myUIObject getRoot() { return mRoot; }
}