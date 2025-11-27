using System.Collections.Generic;
using UnityEngine;
using static MathUtility;
using static WidgetUtility;

public class UGUITreeList : WindowObjectUGUI, ICommonUI
{
	protected myUGUIObject mViewport;
	protected myUGUIDragView mContent;
	protected List<UGUITreeNode> mAllNodeList = new();
	protected List<UGUITreeNode> mRootList = new();
	protected bool mDirty;
	public UGUITreeList(IWindowObjectOwner parent) : base(parent) { }
	protected override void assignWindowInternal()
	{
		newObject(out mViewport, "Viewport");
		newObject(out mContent, mViewport, "Content");
	}
	public override void init()
	{
		base.init();
		mRoot.registeCollider();
		mContent.initDragView();
	}
	public override void update()
	{
		base.update();
		if (mDirty)
		{
			resizeTreeAreaSize();
		}
		float viewportHalfHeight = mViewport.getWindowSize().y * 0.5f;
		foreach (UGUITreeNode item in mAllNodeList)
		{
			myUGUIObject itemRoot = item.getRoot();
			Vector3 localPos = mViewport.worldToLocal(itemRoot.getWorldPosition());
			itemRoot.setHandleInput(abs(localPos.y) < viewportHalfHeight);
		}
	}
	public void selectNode(UGUITreeNode node)
	{
		foreach (UGUITreeNode item in mAllNodeList)
		{
			item.setSelect(node == item);
		}
	}
	public void addNode(UGUITreeNode parent, UGUITreeNode node)
	{
		if (parent == null)
		{
			mRootList.Add(node);
		}
		else
		{
			parent.addChild(node);
			node.setParent(parent);
		}
		mAllNodeList.Add(node);
		node.setTree(this);
		mDirty = true;
	}
	// 递归展开所有节点
	public void expandAll(bool resizeImmediately = false)
	{
		foreach (UGUITreeNode item in mRootList)
		{
			expand(item, true, true);
		}
		if (resizeImmediately)
		{
			resizeTreeAreaSize();
		}
	}
	public void collapseAll(bool resizeImmediately = false)
	{
		foreach (UGUITreeNode item in mRootList)
		{
			collapse(item, true, true);
		}
		if (resizeImmediately)
		{
			resizeTreeAreaSize();
		}
	}
	// 展开指定节点,recursive表示是否递归展开子节点
	public void expand(UGUITreeNode node, bool recursive = false, bool force = false)
	{
		if (!node.isExpand() || force)
		{
			node.setExpand(true);
			mDirty = true;
		}
		if (recursive)
		{
			foreach (UGUITreeNode item in node.getChildNodeList())
			{
				expand(item, recursive);
			}
		}
	}
	public void collapse(UGUITreeNode node, bool recursive = false, bool force = false)
	{
		if (node.isExpand() || force)
		{
			node.setExpand(false);
			mDirty = true;
		}
		if (recursive)
		{
			foreach (UGUITreeNode item in node.getChildNodeList())
			{
				collapse(item, recursive);
			}
		}
	}
	public UGUITreeNode getSelectedNode()
	{
		foreach (UGUITreeNode item in mAllNodeList)
		{
			if (item.isSelect())
			{
				return item;
			}
		}
		return null;
	}
	public void resizeTreeAreaSize()
	{
		mDirty = false;
		autoGridVertical(mContent, false);
	}
	public List<UGUITreeNode> getAllNodeList() { return mAllNodeList; }
	public myUGUIObject getContent() { return mContent; }
}