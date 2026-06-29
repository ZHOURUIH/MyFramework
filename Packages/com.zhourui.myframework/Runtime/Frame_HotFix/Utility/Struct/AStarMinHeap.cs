
// 用于计算A*的最小堆,可快速找出F值最小的节点
class AStarMinHeap
{
	protected AStarNode[] mHeap;		// 总的节点列表,Length为当前的容量
	protected int[] mIndexToPos;        // 将下标转换为位置
	protected int mSize;				// 当前节点的数量
	public int Count => mSize;
	public int Capacity => mHeap.Length;
	public AStarMinHeap(int capacity)
	{
		mHeap = new AStarNode[capacity];
		mIndexToPos = new int[capacity];
		mSize = 0;
	}
	public void clear()
	{
		mSize = 0;
	}
	public void add(AStarNode node)
	{
		int pos = mSize++;
		mHeap[pos] = node;
		mIndexToPos[node.mIndex] = pos;
		siftUp(pos);
	}
	public AStarNode popMinF()
	{
		AStarNode root = mHeap[0];
		mIndexToPos[root.mIndex] = -1;

		mSize--;
		if (mSize > 0)
		{
			mHeap[0] = mHeap[mSize];
			mIndexToPos[mHeap[0].mIndex] = 0;
			siftDown(0);
		}
		return root;
	}
	public void updateNode(AStarNode node)
	{
		int pos = mIndexToPos[node.mIndex];
		mHeap[pos] = node;
		siftUp(pos);
	}
	//-------------------------------------------------------------------------------------------------------------------------------
	protected void siftUp(int pos)
	{
		while (pos > 0)
		{
			int parent = (pos - 1) >> 1;
			if (mHeap[parent].mF <= mHeap[pos].mF)
			{
				break;
			}
			swap(pos, parent);
			pos = parent;
		}
	}
	protected void siftDown(int pos)
	{
		while (true)
		{
			int left = (pos << 1) + 1;
			if (left >= mSize)
			{
				break;
			}
			int right = left + 1;
			int min = (right < mSize && mHeap[right].mF < mHeap[left].mF) ? right : left;
			if (mHeap[pos].mF <= mHeap[min].mF)
			{
				break;
			}
			swap(pos, min);
			pos = min;
		}
	}
	protected void swap(int a, int b)
	{
		(mHeap[a], mHeap[b]) = (mHeap[b], mHeap[a]);
		mIndexToPos[mHeap[a].mIndex] = a;
		mIndexToPos[mHeap[b].mIndex] = b;
	}
}
