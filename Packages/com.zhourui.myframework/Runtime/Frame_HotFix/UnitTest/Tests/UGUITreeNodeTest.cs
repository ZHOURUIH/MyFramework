using System;
using static TestAssert;

// UGUITreeNode 树节点测试(WindowRecyclableUGUI 子类, 纯状态逻辑)
// 不测 init(registeCollider 依赖)/onNodeClick(mTree null NRE)/reset(base 依赖窗口系统)
public static class UGUITreeNodeTest
{
	public static void Run()
	{
		testDefaultState();
		testSetTreeGetTree();
		testAddChild();
		testSetParentDepth();
		testSetParentNullDepthZero();
		testSetSelectToggle();
		testSetExpandToggle();
		testSetNodeClickCallback();
		testGetChildDepth();
		testMultipleChildren();
		testChildDepthChain();
	}

	// 测试节点子类
	private class TestNode : UGUITreeNode
	{
		public TestNode(IWindowObjectOwner parent) : base(parent) { }
		protected override void assignWindowInternal() { }
	}

	private static TestNode NewNode(TestLayoutScript script)
	{
		return new TestNode(script);
	}

	// 默认状态
	private static void testDefaultState()
	{
		var script = new TestLayoutScript();
		TestNode node = NewNode(script);
		assertFalse(node.isExpand(), "默认不展开");
		assertFalse(node.isSelect(), "默认不选中");
		assertEqual(0, node.getDepth(), "默认深度 0");
		assertNull(node.getTree(), "默认 tree null");
		assertNull(node.getParentNode(), "默认 parent null");
		assertEqual(0, node.getChildNodeList().Count, "默认无子节点");
	}

	// setTree 往返
	private static void testSetTreeGetTree()
	{
		var script = new TestLayoutScript();
		TestNode node = NewNode(script);
		UGUITreeList tree = null;
		node.setTree(tree);
		assertNull(node.getTree(), "set null 后 get null");
	}

	// addChild 后子列表
	private static void testAddChild()
	{
		var script = new TestLayoutScript();
		TestNode parent = NewNode(script);
		TestNode child = NewNode(script);
		parent.addChild(child);
		assertEqual(1, parent.getChildNodeList().Count, "addChild 后 1 个子节点");
	}

	// setParent 设置深度(父深度+1)
	private static void testSetParentDepth()
	{
		var script = new TestLayoutScript();
		TestNode parent = NewNode(script);
		TestNode child = NewNode(script);
		parent.setParent(null);
		child.setParent(parent);
		assertEqual(1, child.getDepth(), "子节点深度 = 父深度+1");
	}

	// setParent(null) 深度 0
	private static void testSetParentNullDepthZero()
	{
		var script = new TestLayoutScript();
		TestNode node = NewNode(script);
		node.setParent(null);
		assertEqual(0, node.getDepth(), "parent null 深度 0");
	}

	// setSelect 切换
	private static void testSetSelectToggle()
	{
		var script = new TestLayoutScript();
		TestNode node = NewNode(script);
		node.setSelect(true);
		assertTrue(node.isSelect(), "set true 后选中");
		node.setSelect(false);
		assertFalse(node.isSelect(), "set false 后不选中");
	}

	// setExpand 切换
	private static void testSetExpandToggle()
	{
		var script = new TestLayoutScript();
		TestNode node = NewNode(script);
		node.setExpand(true);
		assertTrue(node.isExpand(), "set true 后展开");
		node.setExpand(false);
		assertFalse(node.isExpand(), "set false 后不展开");
	}

	// setNodeClickCallback 赋值
	private static void testSetNodeClickCallback()
	{
		var script = new TestLayoutScript();
		TestNode node = NewNode(script);
		node.setNodeClickCallback(() => { });
		node.setNodeClickCallback(null);
		// 无异常即通过
	}

	// getChildDepth = 自身深度+1
	private static void testGetChildDepth()
	{
		var script = new TestLayoutScript();
		TestNode node = NewNode(script);
		assertEqual(1, node.getChildDepth(), "getChildDepth = depth+1");
	}

	// 多子节点
	private static void testMultipleChildren()
	{
		var script = new TestLayoutScript();
		TestNode parent = NewNode(script);
		for (int i = 0; i < 5; ++i)
		{
			parent.addChild(NewNode(script));
		}
		assertEqual(5, parent.getChildNodeList().Count, "5 个子节点");
	}

	// 深度链: 孙节点深度 2
	private static void testChildDepthChain()
	{
		var script = new TestLayoutScript();
		TestNode root = NewNode(script);
		TestNode child = NewNode(script);
		TestNode grandChild = NewNode(script);
		child.setParent(root);
		grandChild.setParent(child);
		assertEqual(1, child.getDepth(), "child 深度 1");
		assertEqual(2, grandChild.getDepth(), "grandChild 深度 2");
	}
}
