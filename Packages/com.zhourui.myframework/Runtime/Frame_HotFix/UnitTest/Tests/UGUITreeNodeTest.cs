using System.Collections.Generic;
using static TestAssert;

// UGUITreeNode: 树节点(纯 C# 字段逻辑: 父子/深度/展开/选中), 
// 不调 init(依赖 mRoot.registeCollider), 用测试子类 + 真实 parent 直接 new 可测
public static class UGUITreeNodeTest
{
	public static void Run()
	{
		testDefaultState();
		testTreeGetSet();
		testAddChild();
		testSetParentDepth();
		testExpandSelect();
		testNodeClickCallback();
	}

	// 构造默认值
	private static void testDefaultState()
	{
		TestUGUITreeNode node = createNode();
		assertFalse(node.isExpand(), "默认未展开");
		assertFalse(node.isSelect(), "默认未选中");
		assertEqual(0, node.getDepth(), "默认深度 0");
		assertEqual(1, node.getChildDepth(), "默认子深度 1");
		assertTrue(node.getParentNode() == null, "默认无父节点");
		assertTrue(node.getTree() == null, "默认无树");
		assertEqual(0, node.getChildNodeList().Count, "默认无子节点");
	}

	// setTree/getTree: 引用存储
	private static void testTreeGetSet()
	{
		TestUGUITreeNode node = createNode();
		UGUITreeList tree = new UGUITreeList(new TestLayoutScriptDeep());
		node.setTree(tree);
		assertTrue(ReferenceEquals(tree, node.getTree()), "setTree 引用存储");
		node.setTree(null);
		assertTrue(node.getTree() == null, "setTree(null) 清空");
	}

	// addChild/getChildNodeList: 子节点列表操作
	private static void testAddChild()
	{
		TestUGUITreeNode parent = createNode();
		TestUGUITreeNode child = createNode();
		parent.addChild(child);
		List<UGUITreeNode> children = parent.getChildNodeList();
		assertEqual(1, children.Count, "addChild 后子节点数 1");
		assertTrue(ReferenceEquals(child, children[0]), "子节点引用正确");
	}

	// setParent/getParentNode/getDepth: 深度 = 父节点子深度(父深度 + 1)
	private static void testSetParentDepth()
	{
		TestUGUITreeNode root = createNode();
		TestUGUITreeNode level1 = createNode();
		TestUGUITreeNode level2 = createNode();
		// 无父节点: 深度 0
		level1.setParent(null);
		assertEqual(0, level1.getDepth(), "无父节点深度 0");
		assertTrue(level1.getParentNode() == null, "无父节点 getParentNode null");
		// level1 挂 root 下: 深度 = root.getChildDepth() = 0 + 1 = 1
		level1.setParent(root);
		assertTrue(ReferenceEquals(root, level1.getParentNode()), "父节点引用正确");
		assertEqual(1, level1.getDepth(), "深度 = 父深度 0 + 1");
		// level2 挂 level1 下: 深度 2
		level2.setParent(level1);
		assertEqual(2, level2.getDepth(), "深度 = 父深度 1 + 1");
	}

	// setExpand/isExpand, setSelect/isSelect: 读写
	private static void testExpandSelect()
	{
		TestUGUITreeNode node = createNode();
		node.setExpand(true);
		assertTrue(node.isExpand(), "setExpand(true) 读回");
		node.setExpand(false);
		assertFalse(node.isExpand(), "setExpand(false) 读回");
		node.setSelect(true);
		assertTrue(node.isSelect(), "setSelect(true) 读回");
		node.setSelect(false);
		assertFalse(node.isSelect(), "setSelect(false) 读回");
	}

	// setNodeClickCallback: 存储不触发(onNodeClick 依赖树结构, 不测)
	private static void testNodeClickCallback()
	{
		TestUGUITreeNode node = createNode();
		node.setNodeClickCallback(() => { });
		node.setNodeClickCallback(null);
		// 无异常即通过
	}

	// ═════════════════════════════════════════════════════════════════
	// 辅助: parent 必须传真实对象(WindowObjectFixedT 构造 mScript.addWindowObject(this))
	// ═════════════════════════════════════════════════════════════════
	private static TestUGUITreeNode createNode()
	{
		return new TestUGUITreeNode(new TestLayoutScriptDeep());
	}
}

// 测试辅助: 实例化抽象 UGUITreeNode(无额外字段, 无需 resetProperty)
// assignWindowInternal 基类为 abstract(WindowObjectBase), 测试不触发场景节点查找, 空实现即可
// (同 UGUITreeList 模式: 该抽象方法基类无具体实现, 不调 base)
public class TestUGUITreeNode : UGUITreeNode
{
	public TestUGUITreeNode(IWindowObjectOwner parent) : base(parent) { }

	protected override void assignWindowInternal()
	{
		// 测试环境不绑定真实 UI 节点, 空实现(不调 base: 基类为 abstract)
	}
}
