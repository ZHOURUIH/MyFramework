using static TestAssert;

public static class UIDepthTest
{
	public static void Run()
	{
		testRootDepth();
		testChildDepthAndCompare();
		testDepthOverAllChildPriority();
		testMultiLevelDepth();
	}
	private static void testRootDepth()
	{
		UIDepth root = new();
		root.setDepthValue(null, 10, false);
		assertEqual(10, root.getOrderInParent());
		assertEqual(0, root.getPriority());
		assertTrue(root.toDepthString().StartsWith("10 "), "根节点深度字符串首层应为 10");
	}
	private static void testChildDepthAndCompare()
	{
		UIDepth root = new();
		root.setDepthValue(null, 1, false);
		UIDepth childLow = new();
		UIDepth childHigh = new();
		childLow.setDepthValue(root, 2, false);
		childHigh.setDepthValue(root, 3, false);
		assertEqual(2, childLow.getOrderInParent());
		assertEqual(3, childHigh.getOrderInParent());
		assertTrue(UIDepth.compare(childHigh, childLow) < 0, "order 更大的节点应排序在前");
		assertTrue(UIDepth.compare(childLow, childHigh) > 0, "order 更小的节点应排序在后");
		assertEqual(0, UIDepth.compare(childLow, childLow), "同一深度比较应相等");
	}
	private static void testDepthOverAllChildPriority()
	{
		UIDepth root = new();
		root.setDepthValue(null, 1, false);
		UIDepth normal = new();
		UIDepth overAllChild = new();
		normal.setDepthValue(root, 5, false);
		overAllChild.setDepthValue(root, 5, true);
		assertEqual(0, normal.getPriority());
		assertEqual(-1, overAllChild.getPriority());
		assertEqual(6, overAllChild.getOrderInParent(), "depthOverAllChild 会把实际排序深度加 1");
		assertTrue(UIDepth.compare(overAllChild, normal) < 0, "depthOverAllChild 节点应排在同层普通节点前");
	}
	private static void testMultiLevelDepth()
	{
		UIDepth root = new();
		root.setDepthValue(null, 1, false);
		UIDepth parent = root;
		for (int i = 2; i <= 8; ++i)
		{
			UIDepth child = new();
			child.setDepthValue(parent, i, false);
			assertEqual(i, child.getOrderInParent(), "多层级 order 应可读取");
			parent = child;
		}
		assertTrue(parent.toDepthString().Contains("8 "), "深层节点深度字符串应包含末级 order");
	}
}