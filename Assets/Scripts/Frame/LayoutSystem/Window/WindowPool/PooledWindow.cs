using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class PooledWindow : GameBase
{
	protected LayoutScript mScript;
	protected txUIObject mRoot;		// 需要在子类中被赋值
	protected int mAssignID;
	public virtual void setScript(LayoutScript script) { mScript = script; }
	public abstract void assignWindow(txUIObject parent, txUIObject template, string name);
	public virtual void init() { }
	public virtual void destroy() { }
	public virtual void reset() { }
	public void setVisible(bool visible) { LT.ACTIVE(mRoot, visible); }
	public void setParent(txUIObject parent) { mRoot.setParent(parent); }
	public void setAssignID(int assignID) { mAssignID = assignID; }
	public int getAssignID() { return mAssignID; }
}