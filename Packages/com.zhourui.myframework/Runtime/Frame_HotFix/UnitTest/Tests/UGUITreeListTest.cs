using UnityEngine;
using static TestAssert;

// UGUITreeList / UGUITreeNode 深度测试
// 覆盖树形列表控件的核心逻辑(不依赖真实渲染):
//   addNode: 根节点加入 mRootList, 子节点加入父的 mChildNodeList + depth 计算
//   expand/collapse: 展开/收起状态切换, recursive 递归, force 强制
//   expandAll/collapseAll: 全部展开/收起
//   selectNode: 单选语义(选中一个取消其他)
//   getSelectedNode: 查找选中节点
//   setNodeClickCallback: 回调分发到所有节点
//   UGUITreeNode: setTree/getTree/isExpand/isSelect/getDepth/getChildDepth/reset
//
// 环境: TestLayoutScriptDeep + setLayout(裸 GameLayout) + setRoot(myUGUICanvas, 预加 Canvas)
// 节点: TestTreeNode(UGUITreeNode 子类) 仅做逻辑节点, 不绑定 UI 节点(不调 init)
// 清理: rootGo 手动 DestroyImmediate
public static class UGUITreeListTest
{
	public static void Run()
	{
		testTreeAddRootNode();
		testTreeAddChildNode();
		testTreeExpandCollapse();
		testTreeExpandRecursive();
		testTreeCollapseRecursive();
		testTreeExpandAllCollapseAll();
		testTreeSelectNode();
		testTreeGetSelectedNode();
		testTreeNodeDepth();
		testTreeNodeReset();
		testTreeNodeClickCallback();
	}

	// ═════════════════════════════════════════════════════════════════
	// 辅助: 创建 script + 根节点
	// ═════════════════════════════════════════════════════════════════
	private static TestLayoutScriptDeep createScript(out GameObject rootGo)
	{
		rootGo = new GameObject("TestTreeRoot");
		rootGo.AddComponent<RectTransform>();
		rootGo.AddComponent<Canvas>();
		myUGUICanvas root = new myUGUICanvas();
		root.setObject(rootGo);
		root.init();
		TestLayoutScriptDeep script = new TestLayoutScriptDeep();
		script.setLayout(new GameLayout());
		script.setRoot(root);
		return script;
	}

	private static TestTreeNode createNode(TestLayoutScriptDeep script)
	{
		return new TestTreeNode(script);
	}

	// ═════════════════════════════════════════════════════════════════
	// addNode(null): 加入根节点列表
	// ═════════════════════════════════════════════════════════════════
	private static void testTreeAddRootNode()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo);
		try
		{
			TestTreeList tree = new TestTreeList(script);
			TestTreeNode nodeA = createNode(script);
			TestTreeNode nodeB = createNode(script);
			tree.addNode(null, nodeA);
			tree.addNode(null, nodeB);
			assertEqual(2, tree.getAllNodeList().Count, "两个根节点加入 mAllNodeList");
			assertEqual(2, tree.getRootNodeCount(), "两个根节点加入 mRootList");
			assertEqual(tree, nodeA.getTree(), "节点绑定所属树");
			assertEqual(tree, nodeB.getTree(), "节点绑定所属树");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// addNode(parent): 子节点加入父的 mChildNodeList + depth 递增
	private static void testTreeAddChildNode()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo);
		try
		{
			TestTreeList tree = new TestTreeList(script);
			TestTreeNode root = createNode(script);
			TestTreeNode child = createNode(script);
			TestTreeNode grandchild = createNode(script);
			tree.addNode(null, root);
			tree.addNode(root, child);
			tree.addNode(child, grandchild);
			assertEqual(1, root.getChildNodeList().Count, "根节点有 1 个子节点");
			assertEqual(child, root.getChildNodeList()[0], "子节点加入父的列表");
			assertEqual(root, child.getParentNode(), "子节点记录父节点");
			assertEqual(1, child.getDepth(), "子节点 depth=1");
			assertEqual(2, grandchild.getDepth(), "孙节点 depth=2");
			assertEqual(3, tree.getAllNodeList().Count, "三个节点都在 mAllNodeList");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// expand / collapse: 单节点展开收起
	private static void testTreeExpandCollapse()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo);
		try
		{
			TestTreeList tree = new TestTreeList(script);
			TestTreeNode node = createNode(script);
			tree.addNode(null, node);
			assertFalse(node.isExpand(), "初始未展开");
			tree.expand(node);
			assertTrue(node.isExpand(), "expand 后展开");
			// 已展开再 expand 不重复设置(无副作用)
			tree.expand(node);
			assertTrue(node.isExpand(), "重复 expand 保持展开");
			tree.collapse(node);
			assertFalse(node.isExpand(), "collapse 后收起");
			// 已收起再 collapse 无副作用
			tree.collapse(node);
			assertFalse(node.isExpand(), "重复 collapse 保持收起");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// expand recursive: 递归展开子节点
	private static void testTreeExpandRecursive()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo);
		try
		{
			TestTreeList tree = new TestTreeList(script);
			TestTreeNode root = createNode(script);
			TestTreeNode child = createNode(script);
			TestTreeNode grandchild = createNode(script);
			tree.addNode(null, root);
			tree.addNode(root, child);
			tree.addNode(child, grandchild);
			tree.expand(root, true);
			assertTrue(root.isExpand(), "递归展开根节点");
			assertTrue(child.isExpand(), "递归展开子节点");
			assertTrue(grandchild.isExpand(), "递归展开孙节点");
			// force=true 在已展开时也触发
			tree.collapse(root, true);
			assertFalse(root.isExpand(), "递归收起根节点");
			assertFalse(child.isExpand(), "递归收起子节点");
			assertFalse(grandchild.isExpand(), "递归收起孙节点");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// collapse recursive: 只收起自身, 不递归
	private static void testTreeCollapseRecursive()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo);
		try
		{
			TestTreeList tree = new TestTreeList(script);
			TestTreeNode root = createNode(script);
			TestTreeNode child = createNode(script);
			tree.addNode(null, root);
			tree.addNode(root, child);
			tree.expand(root, true);
			tree.collapse(root, false);
			assertFalse(root.isExpand(), "收起根节点");
			assertTrue(child.isExpand(), "非递归收起不影响子节点");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// expandAll / collapseAll: 所有根节点递归展开/收起
	private static void testTreeExpandAllCollapseAll()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo);
		try
		{
			TestTreeList tree = new TestTreeList(script);
			TestTreeNode root1 = createNode(script);
			TestTreeNode root2 = createNode(script);
			TestTreeNode child = createNode(script);
			tree.addNode(null, root1);
			tree.addNode(null, root2);
			tree.addNode(root1, child);
			tree.expandAll();
			assertTrue(root1.isExpand(), "expandAll 展开 root1");
			assertTrue(root2.isExpand(), "expandAll 展开 root2");
			assertTrue(child.isExpand(), "expandAll 递归展开子节点");
			tree.collapseAll();
			assertFalse(root1.isExpand(), "collapseAll 收起 root1");
			assertFalse(root2.isExpand(), "collapseAll 收起 root2");
			assertFalse(child.isExpand(), "collapseAll 递归收起子节点");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// selectNode: 单选, 选中新节点取消其他
	private static void testTreeSelectNode()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo);
		try
		{
			TestTreeList tree = new TestTreeList(script);
			TestTreeNode nodeA = createNode(script);
			TestTreeNode nodeB = createNode(script);
			tree.addNode(null, nodeA);
			tree.addNode(null, nodeB);
			tree.selectNode(nodeA);
			assertTrue(nodeA.isSelect(), "selectNode 后 nodeA 选中");
			assertFalse(nodeB.isSelect(), "nodeB 未选中");
			tree.selectNode(nodeB);
			assertFalse(nodeA.isSelect(), "选中 nodeB 后 nodeA 取消");
			assertTrue(nodeB.isSelect(), "选中 nodeB");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// getSelectedNode: 无选中返回 null, 选中返回节点
	private static void testTreeGetSelectedNode()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo);
		try
		{
			TestTreeList tree = new TestTreeList(script);
			assertNull(tree.getSelectedNode(), "空树无选中节点");
			TestTreeNode nodeA = createNode(script);
			TestTreeNode nodeB = createNode(script);
			tree.addNode(null, nodeA);
			tree.addNode(null, nodeB);
			assertNull(tree.getSelectedNode(), "未选中时返回 null");
			tree.selectNode(nodeB);
			assertEqual(nodeB, tree.getSelectedNode(), "getSelectedNode 返回选中节点");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// UGUITreeNode: getDepth / getChildDepth
	private static void testTreeNodeDepth()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo);
		try
		{
			TestTreeList tree = new TestTreeList(script);
			TestTreeNode root = createNode(script);
			TestTreeNode child = createNode(script);
			tree.addNode(null, root);
			tree.addNode(root, child);
			assertEqual(0, root.getDepth(), "根节点 depth=0");
			assertEqual(1, root.getChildDepth(), "根节点 childDepth=1");
			assertEqual(1, child.getDepth(), "子节点 depth=1");
			assertEqual(2, child.getChildDepth(), "子节点 childDepth=2");
			// setParent(null): depth 归 0
			child.setParent(null);
			assertEqual(0, child.getDepth(), "setParent(null) 后 depth=0");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// UGUITreeNode.reset: 清空子节点/父节点/树引用/选中展开状态
	private static void testTreeNodeReset()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo);
		try
		{
			TestTreeList tree = new TestTreeList(script);
			TestTreeNode root = createNode(script);
			TestTreeNode child = createNode(script);
			tree.addNode(null, root);
			tree.addNode(root, child);
			child.setSelect(true);
			child.setExpand(true);
			child.setNodeClickCallback(() => { });
			child.reset();
			assertEqual(0, child.getChildNodeList().Count, "reset 清空子节点列表");
			assertNull(child.getParentNode(), "reset 清空父节点");
			assertNull(child.getTree(), "reset 清空树引用");
			assertFalse(child.isSelect(), "reset 取消选中");
			assertFalse(child.isExpand(), "reset 取消展开");
			assertFalse(child.hasClickCallback(), "reset 清空点击回调");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}

	// setNodeClickCallback: 分发到所有节点
	private static void testTreeNodeClickCallback()
	{
		TestLayoutScriptDeep script = createScript(out GameObject rootGo);
		try
		{
			TestTreeList tree = new TestTreeList(script);
			TestTreeNode nodeA = createNode(script);
			TestTreeNode nodeB = createNode(script);
			tree.addNode(null, nodeA);
			tree.addNode(null, nodeB);
			int callbackCount = 0;
			tree.setNodeClickCallback(() => callbackCount++);
			nodeA.triggerClickCallback();
			nodeB.triggerClickCallback();
			assertEqual(2, callbackCount, "两个节点的回调都触发");
		}
		finally
		{
			UnityEngine.Object.DestroyImmediate(rootGo);
		}
	}
}

// ═════════════════════════════════════════════════════════════════
// 测试辅助: UGUITreeList 子类(暴露 mRootList 计数)
// ═════════════════════════════════════════════════════════════════
public class TestTreeList : UGUITreeList
{
	public TestTreeList(IWindowObjectOwner parent) : base(parent) { }
	public int getRootNodeCount() { return mRootList.Count; }
}

// ═════════════════════════════════════════════════════════════════
// 测试辅助: UGUITreeNode 子类(仅逻辑节点, 不绑定 UI)
// ═════════════════════════════════════════════════════════════════
public class TestTreeNode : UGUITreeNode
{
	public TestTreeNode(IWindowObjectOwner parent) : base(parent) { }
	protected override void assignWindowInternal() { }
	public bool hasClickCallback() { return mNodeClickCallback != null; }
	public void triggerClickCallback() { mNodeClickCallback?.Invoke(); }
}
