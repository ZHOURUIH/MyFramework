using System.Collections.Generic;
using static UnityUtility;

// 决策树节点基类,DTree表示DecisionTree
public class DTreeNode : ClassObject
{
	public Dictionary<int, DTreeNode> mChildMap = new();	// 以子节点的ID为索引的子节点列表
	public List<DTreeNode> mChildList = new();				// 带顺序的子节点列表
	public Character mCharacter;							// 所属角色
	public DTreeNode mParent;								// 父节点
	public float mRandomWeight = 1.0f;						// 随机选择时的权重,范围0-1
	public int mID;											// 节点唯一ID
	public int mPriority;									// 节点优先级
	public bool mDeadNode;									// 当前节点是否已死亡
	public override void resetProperty()
	{
		base.resetProperty();
		mChildMap.Clear();
		mChildList.Clear();
		mCharacter = null;
		mParent = null;
		mRandomWeight = 1.0f;
		mID = 0;
		mPriority = 0;
		mDeadNode = false;
	}
	public void setID(int id) { mID = id; }
	public int getID() { return mID; }
	public DTreeNode getParent() { return mParent; }
	public List<DTreeNode> getChildList() { return mChildList; }
	public void setCharacter(Character character) { mCharacter = character; }
	public void setRandomWeight(float weight) { mRandomWeight = weight; }
	public float getRandomWeight() { return mRandomWeight; }
	public void setPriority(int priority) { mPriority = priority; }
	public int getPriority() { return mPriority; }
	public virtual bool condition() { return true; }
	public virtual bool isActive() { return true; }
	public virtual void execute() { }
	public virtual void update(float elapsedTime) { }
	public bool addChild(DTreeNode child)
	{
		if (!mChildMap.TryAdd(child.getID(), child))
		{
			logError("不能再次添加同一个子节点");
			return false;
		}
		mChildList.Add(child);
		return true;
	}
	public bool setParent(DTreeNode parent)
	{
		if (mParent != null && parent != null)
		{
			logError("当前父节点不为空,不能挂接到其他父节点");
			return false;
		}
		mParent = parent;
		return true;
	}
	public bool removeChild(DTreeNode child)
	{
		if (!mChildMap.Remove(child.getID()))
		{
			logError("找不到子节点,无法移除");
			return false;
		}
		mChildList.Remove(child);
		return true;
	}
	//------------------------------------------------------------------------------------------------------------------------------
	protected virtual void notifyAttachParent(DTreeNode parent) { }
}