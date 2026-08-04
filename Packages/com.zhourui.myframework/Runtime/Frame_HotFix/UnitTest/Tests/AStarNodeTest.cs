using static TestAssert;

public class AStarNodeTest
{
	public static void Run()
	{
		testConstructor();
		testInit();
		testStateTransitions();
		testFValueCalculation();
	}

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

	private static void testStateTransitions()
	{
		AStarNode node = new(0, 0, 0, 1, -1, 0);
		// 初始 NONE
		assertEqual(0, (int)node.mState);

		// 放入开启列表
		node.mState = NODE_STATE.OPEN;
		assertEqual(1, (int)node.mState);

		// 放入关闭列表
		node.mState = NODE_STATE.CLOSE;
		assertEqual(2, (int)node.mState);
	}

	private static void testFValueCalculation()
	{
		// F = G + H
		AStarNode node = new(15, 25, 40, 1, -1, 0);
		assertEqual(40, node.mF);
		assertEqual(15 + 25, node.mF, "F = G + H");

		// init 后归零
		node.init(2);
		assertEqual(0, node.mF);
		assertEqual(0, node.mG);
		assertEqual(0, node.mH);
	}
}
