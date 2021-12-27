using System;
using System.Collections.Generic;
using UnityEngine;

// 用于固定数量类,不能用于回收复用窗口
// 通常只是用于已经在预设中创建好的窗口,创建对象时不会创建新的节点,也可以选择克隆到指定父节点下
public class WindowObject : FrameBase
{
	protected LayoutScript mScript;	// 所属的布局脚本
	public virtual void setScript(LayoutScript script) { mScript = script; }
	// 构造后调用一次
	public virtual void assignWindow(myUIObject parent, string name){}
	// 构造后调用一次,在指定的父节点下克隆一个物体
	public virtual void assignWindow(myUIObject parent, myUIObject template, string name){}
	// 构造后调用一次
	public virtual void init() { }
	// 对象真正从内存中销毁时调用
	public virtual void destroy() { }
	// 创建或者复用后的第一次显示之前调用
	public virtual void reset() { }
	// 在对象被回收时调用
	public virtual void recycle() { }
}