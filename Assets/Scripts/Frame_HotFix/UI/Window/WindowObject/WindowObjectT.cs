using UnityEngine;
using static UnityUtility;
using static MathUtility;
using static FrameDefine;

public abstract class WindowObjectT<T> : WindowObjectBase where T : myUGUIObject, new()
{
	protected T mRoot;                                  // 根节点
	protected bool mRootIsFromClone;                    // 根节点是否是克隆来的
	protected bool mChangePositionAsInvisible;			// 是否使用移动位置来代替隐藏
	public WindowObjectT(IWindowObjectOwner parent) : base(parent) { }
	// 对象真正从内存中销毁时调用
	public override void destroy()
	{
		base.destroy();
		if (mRootIsFromClone && mRoot != null)
		{
			LayoutScript.destroyCloned(mRoot);
			mRoot = null;
		}
	}
	// 在parent下根据template克隆一个物体作为Root,设置名字为name
	public void assignWindow(myUGUIObject parent, myUGUIObject template, string name)
	{
		mRootIsFromClone = true;
		mScript.cloneObject(out mRoot, parent, template, name);
		assignWindowInternal();
	}
	// 使用itemRoot作为Root
	public void assignWindow(myUGUIObject itemRoot)
	{
		mRoot = itemRoot as T;
		assignWindowInternal();
	}
	// 在指定的父节点下获取一个物体,将parent下名字为name的节点作为Root
	public void assignWindow(myUGUIObject parent, string name)
	{
		newObject(out mRoot, parent, name);
		assignWindowInternal();
	}
	public void assignWindow(myUGUIObject parent, string name, bool showError)
	{
		newObject(out mRoot, parent, name, showError);
		assignWindowInternal();
	}
	public bool isValid() { return mRoot != null; }
	public override bool isActive() { return mRoot?.isActiveInHierarchy() ?? false; }
	public override void setActive(bool active) 
	{
		bool curActive = isActive();
		if (curActive && mChangePositionAsInvisible && isVectorEqual(mRoot.getPosition(), FAR_POSITION))
		{
			curActive = false;
		}
		if (active == curActive)
		{
			return;
		}
		base.setActive(active);
		if (mChangePositionAsInvisible)
		{
			if (!active)
			{
				mRoot.setPosition(FAR_POSITION);
			}
		}
		else
		{
			mRoot.setActive(active);
		}
	}
	public virtual void setPosition(Vector3 pos) { mRoot.setPosition(pos); }
	public T getRoot() { return mRoot; }
	public Vector3 getPosition() { return mRoot.getPosition(); }
	public Vector2 getSize() { return mRoot.getWindowSize(); }
	public int getSibling()
	{
		checkRoot();
		return mRoot.getSibling();
	}
	public bool setSibling(int index, bool refreshDepth = true)
	{
		checkRoot();
		return mRoot.setSibling(index, refreshDepth);
	}
	public override void setAsFirstSibling(bool refreshDepth = true)
	{
		checkRoot();
		mRoot.setAsFirstSibling(refreshDepth);
	}
	public override void setAsLastSibling(bool refreshDepth = true)
	{
		checkRoot();
		mRoot.setAsLastSibling(refreshDepth);
	}
	public override void setParent(myUGUIObject parent, bool refreshDepth = true)
	{
		checkRoot();
		mRoot.setParent(parent, refreshDepth);
	}
	public bool isVisible()
	{
		checkRoot();
		return mRoot.isActiveInHierarchy();
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected void checkRoot()
	{
		if (mRoot == null)
		{
			logError("mRoot为空,请确保在assignWindow中已经给mRoot赋值了");
		}
	}
	protected T0 newObject<T0>(out T0 obj, string name) where T0 : myUGUIObject, new()
	{
		return newObject(out obj, mRoot, name, true);
	}
	protected T0 newObject<T0>(out T0 obj, string name, bool showError) where T0 : myUGUIObject, new()
	{
		return newObject(out obj, mRoot, name, showError);
	}
}