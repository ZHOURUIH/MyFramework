
// 寻路节点
public struct AStarNode
{
	public int mG;              // 到已经找到的上一个节点的实际消耗
	public int mH;              // 到终点的预估消耗
	public int mF;              // 预估消耗加实际消耗
	public int mIndex;          // 该节点的下标
	public int mParent;         // 该节点的父节点
	public NODE_STATE mState;   // 0表示没有在开启或者关闭列表里，1表示在开启，2表示在关闭
	public AStarNode(int g, int h, int f, int index, int parent, NODE_STATE state)
	{
		mG = g;
		mH = h;
		mF = f;
		mIndex = index;
		mParent = parent;
		mState = state;
	}
	public void init(int index)
	{
		mG = 0;
		mH = 0;
		mF = 0;
		mIndex = index;
		mParent = -1;
		mState = NODE_STATE.NONE;
	}
}