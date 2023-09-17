using System;
using static UnityUtility;
using static FrameUtility;
using static MathUtility;
using static StringUtility;

// 表示UI的深度
public class UIDepth
{
	public const int BYTE_LENGTH = sizeof(ulong);	// 表示深度的数据类型单个占的字节数
	public const int DEPTH_COUNT = 3;				// 有多少个表示深度的数据类型
	public const int LEVEL_LENGTH = 2;				// 每一层使用多少个字节表示
	// ulong8个字节中每2个字节表示一层深度,所以三个ulong共24个字节最多可以支持12层UI窗口的深度
	// 为了减少内存使用,不使用数组
	protected ulong mWindowDepth0;					// 窗口深度,总的深度实际上是每一层深度组合起来的值
	protected ulong mWindowDepth1;					// 窗口深度,总的深度实际上是每一层深度组合起来的值
	protected ulong mWindowDepth2;					// 窗口深度,总的深度实际上是每一层深度组合起来的值
	protected ulong mOriginDepth0;					// 调整前的深度,因为调整的深度不能影响子节点的深度计算,所以计算子节点的深度时需要使用原始的深度
	protected ulong mOriginDepth1;					// 调整前的深度,因为调整的深度不能影响子节点的深度计算,所以计算子节点的深度时需要使用原始的深度
	protected ulong mOriginDepth2;					// 调整前的深度,因为调整的深度不能影响子节点的深度计算,所以计算子节点的深度时需要使用原始的深度
	protected int mDepthLevel;						// UI的层级相对于Layout根节点,根节点的层级为0,每增加一层,则DepthLevel加1
	protected int mPriority;						// 在比较窗口深度时,如果窗口的所有深度都相同,则比较mPriority
	public int getOrderInParent()
	{
		int longIndex = mDepthLevel / (BYTE_LENGTH / LEVEL_LENGTH);
		int ushortIndexInLong = mDepthLevel % (BYTE_LENGTH / LEVEL_LENGTH);
		int offsetBit = (BYTE_LENGTH - LEVEL_LENGTH - ushortIndexInLong * LEVEL_LENGTH) << 3;
		ulong curDepth = 0;
		if (longIndex == 0)
		{
			curDepth = mWindowDepth0;
		}
		else if (longIndex == 1)
		{
			curDepth = mWindowDepth1;
		}
		else if (longIndex == 1)
		{
			curDepth = mWindowDepth2;
		}
		return (int)((curDepth & ((ulong)ushort.MaxValue << offsetBit)) >> offsetBit);
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
			mWindowDepth0 = (ulong)orderInParent << ((BYTE_LENGTH - LEVEL_LENGTH) << 3);
			mWindowDepth1 = 0;
			mWindowDepth2 = 0;
			mOriginDepth0 = (ulong)orderInParent << ((BYTE_LENGTH - LEVEL_LENGTH) << 3);
			mOriginDepth1 = 0;
			mOriginDepth2 = 0;
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
		int offsetBit = (BYTE_LENGTH - LEVEL_LENGTH - ushortIndexInLong * LEVEL_LENGTH) << 3;
		ulong parentOriginDepth0 = parentDepth.mOriginDepth0;
		ulong parentOriginDepth1 = parentDepth.mOriginDepth1;
		ulong parentOriginDepth2 = parentDepth.mOriginDepth2;

		if (longIndex > 2)
		{
			mWindowDepth0 = parentOriginDepth0;
			mWindowDepth1 = parentOriginDepth1;
			mWindowDepth2 = parentOriginDepth2;
			mOriginDepth0 = parentOriginDepth0;
			mOriginDepth1 = parentOriginDepth1;
			mOriginDepth2 = parentOriginDepth2;
		}
		else if (longIndex == 2)
		{
			mWindowDepth0 = parentOriginDepth0;
			mWindowDepth1 = parentOriginDepth1;
			mWindowDepth2 = parentOriginDepth2 | ((ulong)orderInParent << offsetBit);
			mOriginDepth0 = parentOriginDepth0;
			mOriginDepth1 = parentOriginDepth1;
			mOriginDepth2 = parentOriginDepth2 | ((ulong)originOrderInParent << offsetBit);
		}
		else if (longIndex == 1)
		{
			mWindowDepth0 = parentOriginDepth0;
			mWindowDepth1 = parentOriginDepth1 | ((ulong)orderInParent << offsetBit);
			mWindowDepth2 = 0;
			mOriginDepth0 = parentOriginDepth0;
			mOriginDepth1 = parentOriginDepth1 | ((ulong)originOrderInParent << offsetBit);
			mOriginDepth2 = 0;
		}
		else if (longIndex == 0)
		{
			mWindowDepth0 = parentOriginDepth0 | ((ulong)orderInParent << offsetBit);
			mWindowDepth1 = 0;
			mWindowDepth2 = 0;
			mOriginDepth0 = parentOriginDepth0 | ((ulong)originOrderInParent << offsetBit);
			mOriginDepth1 = 0;
			mOriginDepth2 = 0;
		}
		else
		{
			mWindowDepth0 = 0;
			mWindowDepth1 = 0;
			mWindowDepth2 = 0;
			mOriginDepth0 = 0;
			mOriginDepth1 = 0;
			mOriginDepth2 = 0;
		}
	}
	public int getPriority() { return mPriority; }
	public string toDepthString()
	{
		using (new ClassScope<MyStringBuilder>(out var str))
		{
			for (int i = 0; i < BYTE_LENGTH * DEPTH_COUNT / LEVEL_LENGTH; ++i)
			{
				int longIndex = i / (BYTE_LENGTH / LEVEL_LENGTH);
				int ushortIndexInLong = i % (BYTE_LENGTH / LEVEL_LENGTH);
				int offsetBit = (BYTE_LENGTH - LEVEL_LENGTH - ushortIndexInLong * LEVEL_LENGTH) * 8;
				int levelDepth = 0;
				if (longIndex == 0)
				{
					levelDepth = (int)((mWindowDepth0 & ((ulong)ushort.MaxValue << offsetBit)) >> offsetBit);
				}
				else if (longIndex == 1)
				{
					levelDepth = (int)((mWindowDepth1 & ((ulong)ushort.MaxValue << offsetBit)) >> offsetBit);
				}
				else if (longIndex == 2)
				{
					levelDepth = (int)((mWindowDepth2 & ((ulong)ushort.MaxValue << offsetBit)) >> offsetBit);
				}
				str.append(IToS(levelDepth), " ");
			}
			return str.ToString();
		}
	}
	public static int compare(UIDepth depth0, UIDepth depth1)
	{
		ulong depthValue0 = depth0.mWindowDepth0;
		ulong depthValue1 = depth1.mWindowDepth0;
		if (depthValue0 != depthValue1)
		{
			return sign(depthValue1, depthValue0);
		}

		depthValue0 = depth0.mWindowDepth1;
		depthValue1 = depth1.mWindowDepth1;
		if (depthValue0 != depthValue1)
		{
			return sign(depthValue1, depthValue0);
		}

		depthValue0 = depth0.mWindowDepth2;
		depthValue1 = depth1.mWindowDepth2;
		if (depthValue0 != depthValue1)
		{
			return sign(depthValue1, depthValue0);
		}
		return sign(depth1.mPriority - depth0.mPriority);
	}
}