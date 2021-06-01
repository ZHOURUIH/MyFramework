using System.Collections.Generic;

// 决策树节点基类,DTree表示DecisionTree
public class DTreeNode : FrameBase
{
	public Dictionary<uint, DTreeNode> mChildMap;   // 以子节点的ID为索引的子节点列表
	public List<DTreeNode> mChildList;              // 带顺序的子节点列表
	public Character mCharacter;
	public DTreeNode mParent;
	public object mUserData;        // 用户自定义数据
	public float mRandomWeight;		// 随机选择时的权重,范围0-1
	public bool mDeadNode;			// 当前节点是否已死亡
	public uint mID;                // 节点唯一ID
	public int mPriority;           // 节点优先级
	public DTreeNode()
	{
		mRandomWeight = 1;
		mChildList = new List<DTreeNode>();
		mChildMap = new Dictionary<uint, DTreeNode>();
	}
	public void setID(uint id) { mID = id; }
	public uint getID() { return mID; }
	public DTreeNode getParent() { return mParent; }
	public List<DTreeNode> getChildList() { return mChildList; }
	public void setUserData(object userData) { mUserData = userData; }
	public object getUserData() { return mUserData; }
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
		if (mChildMap.ContainsKey(child.getID()))
		{
			logError("不能再次添加同一个子节点");
			return false;
		}
		mChildList.Add(child);
		mChildMap.Add(child.getID(), child);
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
		if (!mChildMap.ContainsKey(child.getID()))
		{
			logError("找不到子节点,无法移除");
			return false;
		}
		mChildList.Remove(child);
		mChildMap.Remove(child.getID());
		return true;
	}
	//-----------------------------------------------------------------------------------------------
	protected virtual void notifyAttachParent(DTreeNode parent) { }
}