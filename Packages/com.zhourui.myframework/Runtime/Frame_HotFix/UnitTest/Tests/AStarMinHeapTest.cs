using static TestAssert;

// AStarMinHeap 最小堆测试
// 覆盖：构造函数 / Count / Capacity / clear / add / popMinF / updateNode
public static class AStarMinHeapTest
{
	public static void Run()
	{
		testConstructor();
		testAddAndCount();
		testPopMinFSorted();
		testClear();
		testUpdateNode();
		testPopMinFEmptyAfterFullPop();
		testAddMoreThanCapacity();
	}

	// ─── 构造函数 ────────────────────────────────────────────────────────────
	private static void testConstructor()
	{
		AStarMinHeap heap = new(10);
		assertEqual(0, heap.Count, "构造后 Count=0");
		assertEqual(10, heap.Capacity, "构造后 Capacity=10");
	}

	// ─── add 和 Count ──────────────────────────────────────────────────────
	private static void testAddAndCount()
	{
		AStarMinHeap heap = new(10);
		heap.add(new AStarNode(0, 0, 5, 1, -1, 0));
		assertEqual(1, heap.Count, "add 后 Count=1");
		heap.add(new AStarNode(0, 0, 3, 2, -1, 0));
		assertEqual(2, heap.Count, "第二次 add 后 Count=2");
		heap.add(new AStarNode(0, 0, 8, 3, -1, 0));
		assertEqual(3, heap.Count, "第三次 add 后 Count=3");
	}

	// ─── popMinF 按 F 值升序弹出 ──────────────────────────────────────────────
	private static void testPopMinFSorted()
	{
		AStarMinHeap heap = new(10);
		heap.add(new AStarNode(0, 0, 5, 1, -1, 0));
		heap.add(new AStarNode(0, 0, 3, 2, -1, 0));
		heap.add(new AStarNode(0, 0, 8, 3, -1, 0));
		heap.add(new AStarNode(0, 0, 1, 4, -1, 0));

		// 按 F 升序: 1, 3, 5, 8
		AStarNode node = heap.popMinF();
		assertEqual(1, node.mF, "popMinF 第一个 F=1");
		assertEqual(4, node.mIndex, "popMinF 第一个 index=4");

		node = heap.popMinF();
		assertEqual(3, node.mF, "popMinF 第二个 F=3");
		assertEqual(2, node.mIndex, "popMinF 第二个 index=2");

		node = heap.popMinF();
		assertEqual(5, node.mF, "popMinF 第三个 F=5");
		assertEqual(1, node.mIndex, "popMinF 第三个 index=1");

		node = heap.popMinF();
		assertEqual(8, node.mF, "popMinF 第四个 F=8");
		assertEqual(3, node.mIndex, "popMinF 第四个 index=3");

		assertEqual(0, heap.Count, "全部弹出后 Count=0");
	}

	// ─── clear ─────────────────────────────────────────────────────────────
	private static void testClear()
	{
		AStarMinHeap heap = new(10);
		heap.add(new AStarNode(0, 0, 5, 1, -1, 0));
		heap.add(new AStarNode(0, 0, 3, 2, -1, 0));
		assertEqual(2, heap.Count, "clear 前 Count=2");
		heap.clear();
		assertEqual(0, heap.Count, "clear 后 Count=0");
	}

	// ─── updateNode ────────────────────────────────────────────────────────
	private static void testUpdateNode()
	{
		AStarMinHeap heap = new(10);
		heap.add(new AStarNode(0, 0, 10, 1, -1, 0));
		heap.add(new AStarNode(0, 0, 5, 2, -1, 0));

		// 将节点1的 F 从 10 改为 2（应上浮到堆顶）
		heap.updateNode(new AStarNode(0, 0, 2, 1, -1, 0));
		AStarNode node = heap.popMinF();
		assertEqual(1, node.mIndex, "updateNode 后最小 index=1");
		assertEqual(2, node.mF, "updateNode 后最小 F=2");

		node = heap.popMinF();
		assertEqual(2, node.mIndex, "updateNode 后第二个 index=2");
		assertEqual(5, node.mF, "updateNode 后第二个 F=5");
	}

	// ─── 全部弹出后 Count=0 ────────────────────────────────────────────────
	private static void testPopMinFEmptyAfterFullPop()
	{
		AStarMinHeap heap = new(10);
		heap.add(new AStarNode(0, 0, 7, 1, -1, 0));
		heap.popMinF();
		assertEqual(0, heap.Count, "弹空后 Count=0");
	}

	// ─── 超过初始容量 ────────────────────────────────────────────────────────
	private static void testAddMoreThanCapacity()
	{
		// 容量=3，但 add 4 个节点
		AStarMinHeap heap = new(3);
		heap.add(new AStarNode(0, 0, 10, 1, -1, 0));
		heap.add(new AStarNode(0, 0, 5, 2, -1, 0));
		heap.add(new AStarNode(0, 0, 8, 3, -1, 0));
		heap.add(new AStarNode(0, 0, 1, 4, -1, 0));

		assertEqual(4, heap.Count, "超过容量后 Count=4");

		// 验证仍然能按 F 升序弹出
		AStarNode node = heap.popMinF();
		assertEqual(1, node.mF, "超容量 popMinF 最小 F=1");
		assertEqual(3, heap.Count, "弹出一个后 Count=3");
	}
}
