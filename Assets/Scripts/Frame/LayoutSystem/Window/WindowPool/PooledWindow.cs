using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class PooledWindow : GameBase
{
	protected LayoutScript mScript;
	protected myUIObject mRoot;		// 需要在子类中被赋值
	protected int mAssignID;
	public virtual void setScript(LayoutScript script) { mScript = script; }
	public abstract void assignWindow(myUIObject parent, myUIObject template, string name);
	public virtual void init() { }
	public virtual void destroy() { }
	public virtual void reset() { }
	public virtual void recycle() { }
	public bool isVisible() { return mRoot.isActive(); }
	public void setVisible(bool visible) { LT.ACTIVE(mRoot, visible); }
	public void setAsFirstSibling() { mRoot.setAsFirstSibling(); }
	public void setAsLastSibling() { mRoot.setAsLastSibling(); }
	public void setParent(myUIObject parent) { mRoot.setParent(parent); }
	public void setAssignID(int assignID) { mAssignID = assignID; }
	public int getAssignID() { return mAssignID; }
	public myUIObject getRoot() { return mRoot; }
}