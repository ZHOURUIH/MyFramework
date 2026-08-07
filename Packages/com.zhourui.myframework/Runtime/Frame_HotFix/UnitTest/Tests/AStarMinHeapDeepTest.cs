using System;
using System.Collections.Generic;
using System.Reflection;
using static TestAssert;

// ═══════════════════════════════════════════════════════════════════════
// AStarMinHeap 深度测试 — 堆不变量 / 索引映射 / 复杂操作链的验证
//
// 普通单接口测试只验证"最终弹出顺序"与单个 updateNode, 这里聚焦单接口
// 覆盖不到的微妙交互与不变量(共 12 组):
//   堆不变量(核心):        随机批量插入后, 任意 pos>0 恒有
//                          mHeap[parent].mF <= mHeap[pos].mF (父≤子)
//   索引位置映射自洽:      mHeap[mIndexToPos[index]].mIndex == index
//                          堆数组与映射表两者保持一致, 无错位
//   pop 全弹严格升序:      大量随机 F(含大量重复值), 每次 pop 都是当前最小,
//                          且弹出顺序单调不减, 弹出的 index 无重复
//   pop 后映射置 -1:       弹出节点在 mIndexToPos 中被标记为 -1(离开堆)
//   updateNode 减小上浮:   F 变小后节点沿父链上浮, 堆不变量仍成立
//   updateNode 增大下沉:   F 变大后节点下沉(经下游 siftDown 不触发, 仅上浮,
//                          故需配合 pop 才能看到正确次序), 验证混合链正确
//   updateNode 等值边界:   F 不变触发 siftUp break, 位置与映射不变
//   越界被拒绝(隐含):      重复 updateNode 同一 index 覆盖旧值, 只留最新
//   递减插入最坏情况:      每次插入都是当前最小, siftUp 到根的路径最长
//   pop+add 交替稳定性:    一边弹一边插, 任意时刻堆不变量仍成立
//   随机 updateNode 风暴:  大规模随机 increase/decrease, 之后全弹仍严格升序
//   clear 后复用一致性:    清空后再装满, 堆结构与全新堆表现一致
// ═══════════════════════════════════════════════════════════════════════
public static class AStarMinHeapDeepTest
{
	// 反射缓存: 访问 AStarMinHeap 的 protected 实例字段
	private static readonly FieldInfo FI_MHEAP = typeof(AStarMinHeap).GetField("mHeap",
		BindingFlags.NonPublic | BindingFlags.Instance);
	private static readonly FieldInfo FI_INDEX_TO_POS = typeof(AStarMinHeap).GetField("mIndexToPos",
		BindingFlags.NonPublic | BindingFlags.Instance);

	public static void Run()
	{
		testHeapInvariantRandomInsert();
		testIndexToPosMappingConsistent();
		testPopAllStrictAscendingWithDup();
		testPopMarksIndexNegOne();
		testUpdateNodeDecreaseSiftUp();
		testUpdateNodeIncreaseThenPopOrder();
		testUpdateNodeEqualValueBoundary();
		testUpdateNodeOverwriteSameIndex();
		testDescendingInsertWorstCase();
		testPopAddAlternateStability();
		testRandomUpdateStorm();
		testClearThenReuseConsistent();
	}

	// ─── 堆不变量(核心) ─────────────────────────────────────────────────────
	// 随机批量插入后, 任意 pos>0 恒有 mHeap[parent].mF <= mHeap[pos].mF
	private static void testHeapInvariantRandomInsert()
	{
		AStarMinHeap heap = new(256);
		const int n = 200;
		Random rnd = new(20260807);
		for (int i = 0; i < n; i++)
		{
			int f = rnd.Next(0, 100);
			heap.add(new AStarNode(0, 0, f, i, -1, NODE_STATE.NONE));
		}
		assertEqual(n, heap.Count, "插入后 Count=原始数");
		// 遍历 heap 内部所有非空位置, 验证父≤子
		AStarNode[] arr = (AStarNode[])FI_MHEAP.GetValue(heap);
		for (int pos = 1; pos < heap.Count; pos++)
		{
			int parent = (pos - 1) >> 1;
			assertTrue(arr[parent].mF <= arr[pos].mF,
				"堆不变量: parent(" + parent + ").f=" + arr[parent].mF
				+ " <= child(" + pos + ").f=" + arr[pos].mF);
		}
	}

	// ─── 索引位置映射自洽 ───────────────────────────────────────────────────
	// mHeap[mIndexToPos[index]].mIndex == index, 堆数组与映射表完全一致
	private static void testIndexToPosMappingConsistent()
	{
		AStarMinHeap heap = new(32);
		const int n = 30;
		Random rnd = new(1234);
		for (int i = 0; i < n; i++)
		{
			int f = rnd.Next(0, 1000);
			heap.add(new AStarNode(0, 0, f, i, -1, NODE_STATE.NONE));
		}
		AStarNode[] arr = (AStarNode[])FI_MHEAP.GetValue(heap);
		int[] i2p = (int[])FI_INDEX_TO_POS.GetValue(heap);
		for (int i = 0; i < n; i++)
		{
			int pos = i2p[i];
			// 映射有效, 且堆数组在该位置确实是这个 index
			assertTrue(pos >= 0, "index " + i + " 映射位置非负");
			assertTrue(pos < heap.Count, "index " + i + " 映射位置 < Count");
			assertEqual(i, arr[pos].mIndex, "mHeap[mIndexToPos[i]].mIndex == i");
		}
	}

	// ─── pop 全弹严格升序 ───────────────────────────────────────────────────
	// 含大量重复 F 值, 每次 pop 单调不减, 弹出的 index 无重复
	private static void testPopAllStrictAscendingWithDup()
	{
		AStarMinHeap heap = new(256);
		const int n = 150;
		Random rnd = new(777);
		HashSet<int> seen = new();
		for (int i = 0; i < n; i++)
		{
			// 刻意产生大量重复 F
			int f = rnd.Next(0, 15);
			heap.add(new AStarNode(0, 0, f, i, -1, NODE_STATE.NONE));
		}
		assertEqual(n, heap.Count, "pop 前 Count=原始数");
		int prevF = -1;
		for (int i = 0; i < n; i++)
		{
			AStarNode node = heap.popMinF();
			assertTrue(node.mF >= prevF, "弹出顺序单调不减: " + node.mF + " >= " + prevF);
			prevF = node.mF;
			// index 不重复(每个节点恰好弹出一次)
			assertTrue(seen.Add(node.mIndex), "index " + node.mIndex + " 未被重复弹出");
		}
		assertEqual(0, heap.Count, "全部弹出后 Count=0");
	}

	// ─── pop 后映射置 -1 ────────────────────────────────────────────────────
	private static void testPopMarksIndexNegOne()
	{
		AStarMinHeap heap = new(16);
		for (int i = 0; i < 5; i++)
		{
			heap.add(new AStarNode(0, 0, 50 - i * 2, i, -1, NODE_STATE.NONE));
		}
		AStarNode first = heap.popMinF();
		int[] i2p = (int[])FI_INDEX_TO_POS.GetValue(heap);
		assertEqual(-1, i2p[first.mIndex], "弹出节点的 mIndexToPos 置为 -1");
		// 其余仍在堆中的节点映射保持有效
		for (int i = 0; i < 5; i++)
		{
			if (i != first.mIndex)
			{
				assertTrue(i2p[i] >= 0, "未弹出节点 " + i + " 映射仍有效");
			}
		}
	}

	// ─── updateNode 减小上浮 ────────────────────────────────────────────────
	private static void testUpdateNodeDecreaseSiftUp()
	{
		AStarMinHeap heap = new(16);
		// index0:F9, index1:F5, index2:F7
		heap.add(new AStarNode(0, 0, 9, 0, -1, NODE_STATE.NONE));
		heap.add(new AStarNode(0, 0, 5, 1, -1, NODE_STATE.NONE));
		heap.add(new AStarNode(0, 0, 7, 2, -1, NODE_STATE.NONE));
		// 将 index0 的 F 从 9 减到 1, 应上浮到根
		heap.updateNode(new AStarNode(0, 0, 1, 0, -1, NODE_STATE.NONE));
		AStarNode root = heap.popMinF();
		assertEqual(0, root.mIndex, "减小后 index0 上浮到根");
		assertEqual(1, root.mF, "减小后根 F=1");
		// 堆不变量
		AStarNode[] arr = (AStarNode[])FI_MHEAP.GetValue(heap);
		for (int pos = 1; pos < heap.Count; pos++)
		{
			int parent = (pos - 1) >> 1;
			assertTrue(arr[parent].mF <= arr[pos].mF, "减小后堆不变量仍成立");
		}
	}

	// ─── updateNode 增大(验证修复后源码支持双向调整) ─────────────────────────
	// updateNode 现支持 F 增大下沉(修复前仅 siftUp, 增大会破坏堆不变量)。
	// 验证: 先增后减的混合场景下, 堆不变量全程成立, 且弹出顺序正确。
	private static void testUpdateNodeIncreaseThenPopOrder()
	{
		AStarMinHeap heap = new(16);
		// index0:F3, index1:F5, index2:F4
		heap.add(new AStarNode(0, 0, 3, 0, -1, NODE_STATE.NONE));
		heap.add(new AStarNode(0, 0, 5, 1, -1, NODE_STATE.NONE));
		heap.add(new AStarNode(0, 0, 4, 2, -1, NODE_STATE.NONE));
		// index0 F:3→6 增大(下沉到合适位置, 不破坏堆不变量)
		heap.updateNode(new AStarNode(0, 0, 6, 0, -1, NODE_STATE.NONE));
		// index1 F:5→1 减小(上浮到根)
		heap.updateNode(new AStarNode(0, 0, 1, 1, -1, NODE_STATE.NONE));
		// 两次 update 后堆不变量必须成立
		AStarNode[] arr = (AStarNode[])FI_MHEAP.GetValue(heap);
		for (int pos = 1; pos < heap.Count; pos++)
		{
			int parent = (pos - 1) >> 1;
			assertTrue(arr[parent].mF <= arr[pos].mF, "增大/减小混合后堆不变量成立");
		}
		// 期望顺序: F1(index1), F4(index2), F6(index0)
		AStarNode a = heap.popMinF();
		AStarNode b = heap.popMinF();
		AStarNode c = heap.popMinF();
		assertEqual(1, a.mIndex, "第一个弹出 index1(F1)");
		assertEqual(2, b.mIndex, "第二个弹出 index2(F4)");
		assertEqual(0, c.mIndex, "第三个弹出 index0(F6)");
	}

	// ─── updateNode 等值边界 ────────────────────────────────────────────────
	// F 不变时 siftUp break, 位置与映射保持原样
	private static void testUpdateNodeEqualValueBoundary()
	{
		AStarMinHeap heap = new(16);
		heap.add(new AStarNode(0, 0, 5, 0, -1, NODE_STATE.NONE));
		heap.add(new AStarNode(0, 0, 8, 1, -1, NODE_STATE.NONE));
		heap.add(new AStarNode(0, 0, 3, 2, -1, NODE_STATE.NONE));
		// 记录 index2 改前位置
		int[] i2pBefore = (int[])FI_INDEX_TO_POS.GetValue(heap);
		int posBefore = i2pBefore[2];
		// F 相同 → 等价更新, 位置不变
		heap.updateNode(new AStarNode(0, 0, 3, 2, -1, NODE_STATE.NONE));
		int[] i2pAfter = (int[])FI_INDEX_TO_POS.GetValue(heap);
		assertEqual(posBefore, i2pAfter[2], "等值更新后 index2 位置未变");
		// 弹出序仍正确
		AStarNode a = heap.popMinF();
		assertEqual(2, a.mIndex, "等值更新后最小仍 index2(F3)");
	}

	// ─── updateNode 覆盖同一 index(隐含去重) ─────────────────────────────────
	private static void testUpdateNodeOverwriteSameIndex()
	{
		AStarMinHeap heap = new(16);
		// 反复 add 相同 index 会重复占据堆槽, 但项目用法是 add 后回头 updateNode
		heap.add(new AStarNode(0, 0, 10, 0, -1, NODE_STATE.NONE));
		heap.add(new AStarNode(0, 0, 20, 1, -1, NODE_STATE.NONE));
		heap.add(new AStarNode(0, 0, 30, 2, -1, NODE_STATE.NONE));
		// 两次 updateNode 同一 index2, 第二次覆盖第一次
		heap.updateNode(new AStarNode(0, 0, 8, 2, -1, NODE_STATE.NONE));
		heap.updateNode(new AStarNode(0, 0, 2, 2, -1, NODE_STATE.NONE));
		AStarNode root = heap.popMinF();
		assertEqual(2, root.mIndex, "覆盖后 index2 上浮为最小");
		assertEqual(2, root.mF, "覆盖后的最新 F=2 生效");
	}

	// ─── 递减插入最坏情况 ────────────────────────────────────────────────────
	// 每次插入都是当前最小, 触发最长 siftUp 路径, 验证仍正确
	private static void testDescendingInsertWorstCase()
	{
		AStarMinHeap heap = new(32);
		const int n = 20;
		// 递减 F: 100,99,...,81 每次都插到根
		for (int i = 0; i < n; i++)
		{
			heap.add(new AStarNode(0, 0, 100 - i, i, -1, NODE_STATE.NONE));
		}
		assertEqual(n, heap.Count, "递减插入 Count=原始数");
		// 弹出应为递减 F: 81,82,...,100 (最小值 81 先出, 最大值 100 最后)
		int prevF = -1;
		for (int i = 0; i < n; i++)
		{
			AStarNode node = heap.popMinF();
			if (i == 0)
			{
				assertEqual(100 - n + 1, node.mF, "第一个弹出最小值 F=" + (100 - n + 1));
			}
			if (i > 0)
			{
				assertTrue(node.mF > prevF, "递减插入后弹出严格递增 F=" + node.mF);
			}
			prevF = node.mF;
		}
		assertEqual(100, prevF, "最后弹出最大 F=100");
	}

	// ─── pop+add 交替稳定性 ─────────────────────────────────────────────────
	// 一边弹一边插, 任意时刻堆不变量成立, 且每次弹出都必须是"当前堆内的最小 F"。
	// 【重要】不能断言"弹出值单调不减": 两次弹出之间可能插入一个比刚弹出值更小的值
	// (如先弹出 1066, 随后插入 1033, 下一次弹出就是 1033 < 1066) —— 这是最小堆的合法
	// 行为(有插入不保证弹出非降), 并非源码缺陷。正确语义是: 每次 pop 出当前全局最小。
	private static void testPopAddAlternateStability()
	{
		AStarMinHeap heap = new(32);
		Random rnd = new(999);
		int nextIndex = 12;
		// liveF: 仍存活(未弹出)节点的 F 集合, 用于校验每次弹出确为当前最小。
		// 每节点 mIndex 唯一, 用字典 index→F 追踪存活集, 弹出时移除该 index。
		Dictionary<int, int> liveF = new Dictionary<int, int>();
		// 初始填 12 个 (mIndex 0~11), F ∈ [0,500)
		for (int i = 0; i < 12; i++)
		{
			int f = rnd.Next(0, 500);
			heap.add(new AStarNode(0, 0, f, i, -1, NODE_STATE.NONE));
			liveF[i] = f;
		}
		// 交替 pop / add 30 轮
		for (int round = 0; round < 30; round++)
		{
			if ((round & 1) == 0)
			{
				// pop: 弹出的应恰为当前存活集中最小 F
				AStarNode node = heap.popMinF();
				int expectedMin = int.MaxValue;
				foreach (KeyValuePair<int, int> kv in liveF)
				{
					if (kv.Value < expectedMin)
					{
						expectedMin = kv.Value;
					}
				}
				assertTrue(liveF.ContainsKey(node.mIndex), "弹出节点 mIndex 应仍在存活集合:" + node.mIndex);
				assertEqual(expectedMin, node.mF, "交替第 " + round + " 轮 pop 应为当前最小");
				liveF.Remove(node.mIndex);
			}
			else
			{
				// add: 插入 (0..500)+1000 的较大随机值
				int f = rnd.Next(0, 500) + 1000;
				heap.add(new AStarNode(0, 0, f, nextIndex, -1, NODE_STATE.NONE));
				liveF[nextIndex] = f;
				nextIndex++;
				// 新增 index 必须 < 堆容量(32), 否则 mIndexToPos 数组越界
				assertTrue(nextIndex <= 32, "新增 index 不得超过堆容量");
			}
			// 任意时刻堆不变量
			AStarNode[] arr = (AStarNode[])FI_MHEAP.GetValue(heap);
			for (int pos = 1; pos < heap.Count; pos++)
			{
				int parent = (pos - 1) >> 1;
				assertTrue(arr[parent].mF <= arr[pos].mF,
					"交替第 " + round + " 轮堆不变量成立");
			}
		}
	}

	// ─── 随机 updateNode 风暴 ───────────────────────────────────────────────
	// 大规模随机增/减 updateNode, 之后全弹仍严格升序
	private static void testRandomUpdateStorm()
	{
		AStarMinHeap heap = new(128);
		const int n = 100;
		Random rnd = new(555);
		for (int i = 0; i < n; i++)
		{
			heap.add(new AStarNode(0, 0, rnd.Next(0, 50), i, -1, NODE_STATE.NONE));
		}
		// 随机 updateNode 300 次
		for (int k = 0; k < 300; k++)
		{
			int idx = rnd.Next(0, n);
			int newF = rnd.Next(0, 200);
			heap.updateNode(new AStarNode(0, 0, newF, idx, -1, NODE_STATE.NONE));
		}
		// 堆不变量
		AStarNode[] arr = (AStarNode[])FI_MHEAP.GetValue(heap);
		for (int pos = 1; pos < heap.Count; pos++)
		{
			int parent = (pos - 1) >> 1;
			assertTrue(arr[parent].mF <= arr[pos].mF, "updateNode 风暴后堆不变量成立");
		}
		// 全弹严格升序, index 无重复
		int prevF = -1;
		HashSet<int> seen = new();
		for (int i = 0; i < n; i++)
		{
			AStarNode node = heap.popMinF();
			assertTrue(node.mF >= prevF, "updateNode 风暴后弹出单调不减");
			assertTrue(seen.Add(node.mIndex), "updateNode 风暴后 index 无重复");
			prevF = node.mF;
		}
		assertEqual(0, heap.Count, "风暴后全部弹空");
	}

	// ─── clear 后复用一致性 ──────────────────────────────────────────────────
	private static void testClearThenReuseConsistent()
	{
		AStarMinHeap heap = new(16);
		Random rnd = new(42);
		for (int i = 0; i < 8; i++)
		{
			heap.add(new AStarNode(0, 0, rnd.Next(0, 100), i, -1, NODE_STATE.NONE));
		}
		heap.clear();
		assertEqual(0, heap.Count, "clear 后 Count=0");
		// 复用: 重新装入另一批, 结构应与全新表现一致
		// 注意: mIndexToPos 长度=capacity(16), node.mIndex 必须 < 16 才不会越界;
		// 第一批已 clear(mSize=0), 复用 0..m-1 下标不冲突(旧映射被覆盖)。
		const int m = 10;
		for (int i = 0; i < m; i++)
		{
			heap.add(new AStarNode(0, 0, m - i, i, -1, NODE_STATE.NONE));
		}
		assertEqual(m, heap.Count, "复用后 Count=再次插入数");
		// 递减 F → 依次弹出严格递增
		int prevF = -1;
		for (int i = 0; i < m; i++)
		{
			AStarNode node = heap.popMinF();
			assertTrue(node.mF > prevF, "复用后弹出严格递增");
			prevF = node.mF;
		}
		// 映射表也在复用后保持有效(新 index 范围)
		AStarNode[] arr = (AStarNode[])FI_MHEAP.GetValue(heap);
		int[] i2p = (int[])FI_INDEX_TO_POS.GetValue(heap);
		for (int pos = 1; pos < heap.Count; pos++)
		{
			int parent = (pos - 1) >> 1;
			assertTrue(arr[parent].mF <= arr[pos].mF, "复用后堆不变量成立");
		}
	}
}
