using System;
using static UnityUtility;

// 可用于WindowObjectPool和WindowObjectPoolMap的类,常用于可回收复用的窗口
// 每次创建新的对象时都从template克隆
// 根节点是myUGUIObject类型
public abstract class PooledWindowUGUI : PooledWindow
{
	protected myUGUIObject mRoot;     // 需要在子类中被赋值
	// 对象构造之后调用一次
	public bool isVisible()
	{
		if (mRoot == null)
		{
			logError("可复用窗口的mRoot为空,请确保在assignWindow中已经给mRoot赋值了");
		}
		return mRoot.isActive();
	}
	public override void setVisible(bool visible)
	{
		if (mRoot == null)
		{
			logError("可复用窗口的mRoot为空,请确保在assignWindow中已经给mRoot赋值了");
		}
		mRoot.setActive(visible);
	}
	public void setSibling(int index, bool refreshDepth = true)
	{
		if (mRoot == null)
		{
			logError("可复用窗口的mRoot为空,请确保在assignWindow中已经给mRoot赋值了");
		}
		mRoot.setSibling(index, refreshDepth);
	}
	public void setAsFirstSibling(bool refreshDepth = true)
	{
		if (mRoot == null)
		{
			logError("可复用窗口的mRoot为空,请确保在assignWindow中已经给mRoot赋值了");
		}
		mRoot.setAsFirstSibling(refreshDepth);
	}
	public override void setAsLastSibling(bool refreshDepth = true)
	{
		if (mRoot == null)
		{
			logError("可复用窗口的mRoot为空,请确保在assignWindow中已经给mRoot赋值了");
		}
		mRoot.setAsLastSibling(refreshDepth);
	}
	public override void setParent(myUIObject parent, bool refreshDepth = true)
	{
		if (mRoot == null)
		{
			logError("可复用窗口的mRoot为空,请确保在assignWindow中已经给mRoot赋值了");
		}
		mRoot.setParent(parent, refreshDepth);
	}
	public myUGUIObject getRoot() { return mRoot; }
}