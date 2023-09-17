using System;
using UnityEngine;

// 用于固定数量类,不能用于回收复用窗口
// 通常只是用于已经在预设中创建好的窗口,创建对象时不会创建新的节点,也可以选择克隆到指定父节点下
public class WindowObjectUGUI : WindowObject
{
	protected myUGUIObject mRoot;     // 根节点
	// 使用itemRoot作为Root
	public virtual void assignWindow(myUGUIObject itemRoot)
	{
		mRoot = itemRoot;
	}
	// 构造后调用一次,将parent下名字为name的节点作为Root
	public override void assignWindow(myUIObject parent, string name)
	{
		mScript.newObject(out mRoot, parent, name);
	}
	// 构造后调用一次,在指定的父节点下克隆一个物体
	public override void assignWindow(myUIObject parent, myUIObject template, string name)
	{
		mScript.cloneObject(out mRoot, parent, template, name);
	}
	public bool isVisible() { return mRoot.isActive(); }
	public void setVisible(bool visible) { mRoot.setActive(visible); }
	public void setAsFirstSibling(bool refreshDepth = true) { mRoot.setAsFirstSibling(refreshDepth); }
	public void setAsLastSibling(bool refreshDepth = true) { mRoot.setAsLastSibling(refreshDepth); }
	public void setPosition(Vector3 pos) { mRoot.setPosition(pos); }
	public myUIObject getRoot() { return mRoot; }
	public Vector3 getPosition() { return mRoot.getPosition(); }
	public Vector2 getSize() { return mRoot.getWindowSize(); }
}