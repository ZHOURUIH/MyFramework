using System.Collections.Generic;

public abstract class UGUITreeNode : WindowRecycleableUGUI
{
	protected List<UGUITreeNode> mChildNodeList = new();
	protected UGUITreeNode mParentNode;
	protected UGUITreeList mTree;
	protected bool mExpand;
	protected bool mSelect;
	protected int mDepth;
	public UGUITreeNode(IWindowObjectOwner parent) : base(parent) { }
	public override void init()
	{
		base.init();
		mRoot.registeCollider(onNodeClick);
	}
	public void setTree(UGUITreeList tree) { mTree = tree; }
	public void addChild(UGUITreeNode node) { mChildNodeList.Add(node); }
	public void setParent(UGUITreeNode parent)
	{
		mParentNode = parent;
		mDepth = mParentNode?.getChildDepth() ?? 0;
	}
	public virtual void setSelect(bool select) { mSelect = select; }
	public virtual void setExpand(bool expand) { mExpand = expand; }
	public UGUITreeList getTree() { return mTree; }
	public bool isExpand() { return mExpand; }
	public bool isSelect() { return mSelect; }
	public int getDepth() { return mDepth; }
	public int getChildDepth() { return mDepth + 1; }
	public List<UGUITreeNode> getChildNodeList() { return mChildNodeList; }
	public UGUITreeNode getParentNode() { return mParentNode; }
	//------------------------------------------------------------------------------------------------------------------------------
	protected virtual void onNodeClick()
	{
		mTree.selectNode(this);
		// 计算之前的顶部坐标,以便确保在收起或者展开以后,列表的上边界的坐标不变
		myUGUIObject treeContent = mTree.getContent();
		float contentTop = treeContent.getWindowTopInParent();
		if (mExpand)
		{
			mTree.collapse(this);
		}
		else
		{
			mTree.expand(this);
		}
		mTree.resizeTreeAreaSize();
		treeContent.setWindowTopInParent(contentTop);
	}
}