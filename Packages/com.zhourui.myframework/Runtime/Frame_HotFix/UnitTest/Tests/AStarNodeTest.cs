using System;
using static TestAssert;

public class AStarNodeTest
{
	public static void Run()
	{
		testConstructor();
		testInit();
	}

	// 测试构造函数
	private static void testConstructor()
	{
		AStarNode node = new(10, 20, 30, 5, 3, 0);
		assertEqual(10, node.mG);
		assertEqual(20, node.mH);
		assertEqual(30, node.mF);
		assertEqual(5, node.mIndex);
		assertEqual(3, node.mParent);
		assertEqual(0, (int)node.mState);
	}

	// 测试 init 方法
	private static void testInit()
	{
		AStarNode node = new(10, 20, 30, 5, 3, 0);
		node.init(7);
		assertEqual(0, node.mG);
		assertEqual(0, node.mH);
		assertEqual(0, node.mF);
		assertEqual(7, node.mIndex);
		assertEqual(-1, node.mParent);
		assertEqual(0, (int)node.mState);
	}
}