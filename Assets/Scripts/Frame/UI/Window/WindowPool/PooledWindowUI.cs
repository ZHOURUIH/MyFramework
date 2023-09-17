using System;
using static UnityUtility;

// 可用于WindowObjectPool和WindowObjectPoolMap的类,常用于可回收复用的窗口
// 每次创建新的对象时都从template克隆
// 根节点是txUIObject类型
public abstract class PooledWindowUI : PooledWindow
{
	protected myUIObject mRoot;     // 需要在子类中被赋值
	// 对象构造之后调用一次
	public bool isVisible()
	{
		checkRoot();
		return mRoot.isActive();
	}
	public override void setVisible(bool visible)
	{
		checkRoot();
		mRoot.setActive(visible);
	}
	public void setSibling(int index, bool refreshDepth = true)
	{
		checkRoot();
		mRoot.setSibling(index, refreshDepth);
	}
	public void setAsFirstSibling(bool refreshDepth = true)
	{
		checkRoot();
		mRoot.setAsFirstSibling(refreshDepth);
	}
	public override void setAsLastSibling(bool refreshDepth = true)
	{
		checkRoot();
		mRoot.setAsLastSibling(refreshDepth);
	}
	public override void setParent(myUIObject parent, bool refreshDepth = true)
	{
		checkRoot();
		mRoot.setParent(parent, refreshDepth);
	}
	public myUIObject getRoot() { return mRoot; }
	//------------------------------------------------------------------------------------------------------------------------------
	protected void checkRoot()
	{
		if (mRoot == null)
		{
			logError("可复用窗口的mRoot为空,请确保在assignWindow中已经给mRoot赋值了");
		}
	}
}