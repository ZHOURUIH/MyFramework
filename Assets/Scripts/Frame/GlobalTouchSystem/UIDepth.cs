using System;

// 表示UI的深度
public class UIDepth : FrameBase
{
	// 表示深度的数据类型单个占的字节数
	public const int BYTE_LENGTH = sizeof(ulong);
	// 有多少个表示深度的数据类型
	public const int DEPTH_COUNT = 3;
	// 每一层使用多少个字节表示
	public const int LEVEL_LENGTH = 2;
	// ulong8个字节中每2个字节表示一层深度,所以两个ulong共16个字节最多可以支持8层UI窗口的深度
	// mWindowDepth[0]的开始2个字节是布局的深度
	protected ulong[] mWindowDepth;		// 窗口深度,总的深度实际上是每一层深度组合起来的值
	protected ulong[] mOriginDepth;		// 调整前的深度,因为调整的深度不能影响子节点的深度计算,所以计算子节点的深度时需要使用原始的深度
	protected int mDepthLevel;          // UI的层级相对于Layout根节点,根节点的层级为0,每增加一层,则DepthLevel加1
	protected int mPriority;			// 在比较窗口深度时,如果窗口的所有深度都相同,则比较mPriority
	public UIDepth()
	{
		mWindowDepth = new ulong[DEPTH_COUNT];
		mOriginDepth = new ulong[DEPTH_COUNT];
	}
	public int getOrderInParent()
	{
		int longIndex = mDepthLevel / (BYTE_LENGTH / LEVEL_LENGTH);
		int ushortIndexInLong = mDepthLevel % (BYTE_LENGTH / LEVEL_LENGTH);
		int offsetBit = (BYTE_LENGTH - LEVEL_LENGTH - ushortIndexInLong * LEVEL_LENGTH) * 8;
		return (int)((mWindowDepth[longIndex] & ((ulong)ushort.MaxValue << offsetBit)) >> offsetBit);
	}
	// 根据父节点的深度和在父节点中的顺序,更新深度值
	public void setDepthValue(UIDepth parentDepth, int orderInParent, bool depthOverAllChild)
	{
		if (orderInParent < 0 || orderInParent > ushort.MaxValue)
		{
			logError("节点在父节点中的顺序值无效,有效范围是1~" + ushort.MaxValue);
			return;
		}
		if (parentDepth == null)
		{
			mDepthLevel = 0;
			for (int i = 0; i < DEPTH_COUNT; ++i)
			{
				if(i == 0)
				{
					mWindowDepth[i] = (ulong)orderInParent << ((BYTE_LENGTH - LEVEL_LENGTH) * 8);
				}
				else
				{
					mWindowDepth[i] = 0;
				}
				mOriginDepth[i] = mWindowDepth[i];
			}
			return;
		}
		// 如果窗口需要调整为深度在所有子节点之上,则计算额外的深度调整
		// 有父节点才允许这么做,没有父节点的是根节点,根节点调整深度基本没意义
		int originOrderInParent = orderInParent;
		if (depthOverAllChild)
		{
			mPriority = -1;
			++orderInParent;
		}
		else
		{
			mPriority = 0;
		}
		mDepthLevel = parentDepth.mDepthLevel + 1;
		if (mDepthLevel > BYTE_LENGTH * DEPTH_COUNT / LEVEL_LENGTH)
		{
			logError("UI层次超过上限,最多为" + (BYTE_LENGTH * DEPTH_COUNT / LEVEL_LENGTH) + "层");
			return;
		}
		int longIndex = mDepthLevel / (BYTE_LENGTH / LEVEL_LENGTH);
		int ushortIndexInLong = mDepthLevel % (BYTE_LENGTH / LEVEL_LENGTH);
		int offsetBit = (BYTE_LENGTH - LEVEL_LENGTH - ushortIndexInLong * LEVEL_LENGTH) * 8;
		for (int i = 0; i < DEPTH_COUNT; ++i)
		{
			if (i < longIndex)
			{
				mWindowDepth[i] = parentDepth.mOriginDepth[i];
				mOriginDepth[i] = mWindowDepth[i];
			}
			else if (i == longIndex)
			{
				mWindowDepth[i] = parentDepth.mOriginDepth[i] | ((ulong)orderInParent << offsetBit);
				mOriginDepth[i] = parentDepth.mOriginDepth[i] | ((ulong)originOrderInParent << offsetBit);
			}
			else
			{
				mWindowDepth[i] = 0;
				mOriginDepth[i] = mWindowDepth[i];
			}
		}
	}
	public int getPriority() { return mPriority; }
	public string toDepthString()
	{
		string str = EMPTY;
		for(int i = 0; i < BYTE_LENGTH * DEPTH_COUNT / LEVEL_LENGTH; ++i)
		{
			int longIndex = i / (BYTE_LENGTH / LEVEL_LENGTH);
			int ushortIndexInLong = i % (BYTE_LENGTH / LEVEL_LENGTH);
			int offsetBit = (BYTE_LENGTH - LEVEL_LENGTH - ushortIndexInLong * LEVEL_LENGTH) * 8;
			int levelDepth = (int)((mWindowDepth[longIndex] & ((ulong)ushort.MaxValue << offsetBit)) >> offsetBit);
			str += intToString(levelDepth) + " ";
		}
		return str;
	}
	public static int compare(UIDepth depth0, UIDepth depth1)
	{
		for (int i = 0; i < DEPTH_COUNT; ++i)
		{
			if (depth0.mWindowDepth[i] != depth1.mWindowDepth[i])
			{
				return sign(depth1.mWindowDepth[i], depth0.mWindowDepth[i]);
			}
		}
		return sign(depth1.mPriority - depth0.mPriority);
	}
}