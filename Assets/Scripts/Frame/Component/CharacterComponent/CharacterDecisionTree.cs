using System;
using System.Collections.Generic;

// 决策树,用于自动判断并决定角色应该做出的行为
public class CharacterDecisionTree : GameComponent
{
	protected Dictionary<uint, DTreeNode> mNodeList;    // 所有的节点列表
	protected DTreeNode mRootNode;
	protected MyTimer mTimer;
	public CharacterDecisionTree()
	{
		mNodeList = new Dictionary<uint, DTreeNode>();
		mTimer = new MyTimer();
	}
	public override void init(ComponentOwner owner)
	{
		base.init(owner);
		mTimer.init(1.0f, 1.0f);
	}
	public override void resetProperty()
	{
		base.resetProperty();
		mNodeList.Clear();
		mRootNode = null;
		mTimer.stop();
	}
	public override void destroy()
	{
		clear();
		base.destroy();
	}
	// 清空整个树
	public void clear()
	{
		clearNode(mRootNode);
	}
	// 清空并移除一个节点
	public void clearNode(DTreeNode node)
	{
		if(node == null)
		{
			return;
		}
		// 先清空子节点
		if(node.mChildList.Count > 0)
		{
			LIST(out List<DTreeNode> childList);
			childList.AddRange(node.mChildList);
			int count = childList.Count;
			for(int i = 0; i < count; ++i)
			{
				clearNode(childList[i]);
			}
			UN_LIST(childList);
		}
		// 然后将节点自身移除
		removeNode(node);
	}
	public DTreeNode getRoot() { return mRootNode; }
	public DTreeNode addNode(Type nodeType, DTreeNode parent)
	{
		DTreeNode node = createInstance<DTreeNode>(nodeType);
		node.setCharacter(mComponentOwner as Character);
		node.setID(generateGUID());
		addNode(parent, node);
		return node;
	}
	public void addNode(DTreeNode parent, DTreeNode node)
	{
		// 当前没有根节点,则设置根节点
		if(mRootNode == null)
		{
			mRootNode = node;
		}
		// 除了根节点以外,其他所有节点必须拥有一个已经添加到决策树的子节点
		else
		{
			if (parent != null && !mNodeList.ContainsKey(parent.getID()))
			{
				logError("找不到父节点,不能将节点添加到该节点下");
				return;
			}
		}
		mNodeList.Add(node.getID(), node);
		node.setParent(parent);
		parent?.addChild(node);
	}
	// 移除一个叶节点
	public void removeNode(DTreeNode node)
	{
		if(node.getChildList().Count > 0)
		{
			logError("can not remove a node which has child! use clearNode instead!");
			return;
		}
		// 从父节点中移除
		if (node.getParent() != null)
		{
			node.mParent.removeChild(node);
		}
		// 清除节点的父节点
		node.setParent(null);
		// 将节点从列表中移除
		mNodeList.Remove(node.getID());
		if(node == mRootNode)
		{
			mRootNode = null;
		}
	}
	public override void update(float elapsedTime)
	{
		base.update(elapsedTime);
		// 先更新所有节点
		foreach (var item in mNodeList)
		{
			if (!item.Value.mDeadNode)
			{
				item.Value.update(elapsedTime);
			}
		}
		if (mTimer.tickTimer(elapsedTime))
		{
			LIST(out List<DTreeNode> deadList);
			foreach (var item in mNodeList)
			{
				if(item.Value.mDeadNode)
				{
					deadList.Add(item.Value);
				}
			}
			// 移除已经死亡的节点
			int count = deadList.Count;
			for (int i = 0; i < count; ++i)
			{
				clearNode(deadList[i]);
			}
			UN_LIST(deadList);
			// 从根节点开始遍历
			if (mRootNode != null && mRootNode.condition())
			{
				mRootNode.execute();
			}
		}
	}
	public void setDecisionInterval(float interval) { mTimer.setInterval(interval); }
	public void resetDecisionTime() { mTimer.resetToInterval(); }
	public float getDecisionInterval() { return mTimer.mTimeInterval; }
	public float getCurDecisionTime() { return mTimer.mCurTime; }
	public Dictionary<uint, DTreeNode> getNodeList() { return mNodeList; }
}