﻿using System;

// 窗口对象池基类
public class WindowStructPoolBase
{
	protected WindowObjectBase mOwnerObject;	// 如果是WindowObject中创建的对象池,则会存储此WindowObject
	protected LayoutScript mScript;				// 所属的布局脚本
	protected myUIObject mItemParent;			// 创建节点时默认的父节点
	protected myUIObject mTemplate;				// 创建节点时使用的模板
	protected string mPreName;					// 创建物体的名字前缀
	protected Type mObjectType;					// 物体类型
	protected static long mAssignIDSeed;		// 分配ID种子,用于设置唯一分配ID,只会递增,不会减少
	protected bool mNewItemMoveToLast;			// 新创建的物体是否需要放到父节点的最后,也就是是否在意其渲染顺序
	protected bool mInited;						// 是否已经初始化
	public WindowStructPoolBase(IWindowObjectOwner parent)
	{
		if (parent is WindowObjectBase objBase)
		{
			mScript = objBase.getScript();
			mOwnerObject = objBase;
		}
		else if (parent is LayoutScript script)
		{
			mScript = script;
			mOwnerObject = null;
		}
		mOwnerObject?.addWindowPool(this);
		mScript.addWindowStructPool(this);
		mNewItemMoveToLast = true;
	}
	public virtual void destroy() {}
	public virtual void init(myUIObject parent, myUIObject template, Type objectType, bool newItemToLast = true)
	{
		mItemParent = parent;
		mTemplate = template;
		mNewItemMoveToLast = newItemToLast;
		mPreName = template?.getName();
		mObjectType = objectType;
		mTemplate.setActive(false);
		mInited = true;
	}
	public myUIObject getInUseParent() { return mItemParent; }
	public void setItemPreName(string preName) { mPreName = preName; }
	public virtual void unuseAll() { }
	public bool isRootPool() { return mOwnerObject == null; }
}