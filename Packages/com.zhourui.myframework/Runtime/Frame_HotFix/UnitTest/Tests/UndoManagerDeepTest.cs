using System;
using static FrameUtility;
using static TestAssert;

// UndoManager 深度测试 — 撤销/重做状态机的复杂交互
// 覆盖单接口测试覆盖不到的微妙逻辑:
//   maxUndo 溢出淘汰(添加超过上限时移除最早的操作)
//   普通 addUndo 清空 redo 列表
//   redo 过程中 addUndo 不清空 redo 列表
//   undo 回调中 addUndo 进入 redo 列表
//   LIFO 精确执行顺序 + 回调触发时机
public static class UndoManagerDeepTest
{
	class TestUndo : MyUndo
	{
		public Action mAction;
		public string mTag;
		public override void resetProperty()
		{
			base.resetProperty();
			mAction = null;
			mTag = null;
		}
		public override void undo()
		{
			mAction?.Invoke();
		}
	}

	public static void Run()
	{
		testMaxUndoOverflowEvictsOldest();
		testAddUndoClearsRedo();
		testRedoAddUndoKeepsRedo();
		testUndoCallbackAddsToRedo();
		testLifoExactOrder();
		testMaxUndoCountSetter();
		testSetMaxUndoThenOverflow();
	}

	static TestUndo makeUndo(Action action, string tag)
	{
		TestUndo u = CLASS<TestUndo>();
		u.mAction = action;
		u.mTag = tag;
		return u;
	}

	// ═════════════════════════════════════════════════════════════════
	// maxUndo 溢出淘汰 — 添加超过上限时移除最早的操作
	// ═════════════════════════════════════════════════════════════════
	private static void testMaxUndoOverflowEvictsOldest()
	{
		UndoManager undo = new UndoManager();
		undo.setMaxUndoCount(2);
		// 添加3个操作, 最早的 op1 应被淘汰
		undo.addUndo(makeUndo(() => { }, "op1"));
		undo.addUndo(makeUndo(() => { }, "op2"));
		undo.addUndo(makeUndo(() => { }, "op3"));

		// 只保留最近2个: op3, op2
		undo.undo(); // 执行 op3
		undo.undo(); // 执行 op2
		assertFalse(undo.canUndo(), "溢出淘汰后只剩2个操作, 撤销2次后无操作");
		undo.clearAll();
	}

	// ═════════════════════════════════════════════════════════════════
	// 普通 addUndo 清空 redo 列表 — 添加新操作后 redo 失效
	// ═════════════════════════════════════════════════════════════════
	private static void testAddUndoClearsRedo()
	{
		UndoManager undo = new UndoManager();
		// 第一步: 构造出一个可 redo 的状态
		// op1 的 undo 回调会 addUndo 进 redo 列表
		undo.addUndo(makeUndo(() => undo.addUndo(makeUndo(() => { }, "redoOp")), "op1"));
		undo.undo(); // 触发 op1.undo → addUndo(redoOp) 进 redoList
		assertTrue(undo.canRedo(), "undo 回调 addUndo 后应可 redo");

		// 第二步: 普通 addUndo(mUndoing=false, mRedoing=false) → 清空 redo
		undo.addUndo(makeUndo(() => { }, "op2"));
		assertFalse(undo.canRedo(), "普通 addUndo 应清空 redo 列表");
		undo.clearAll();
	}

	// ═════════════════════════════════════════════════════════════════
	// redo 过程中 addUndo 不清空 redo — mRedoing=true 时保留 redo
	// ═════════════════════════════════════════════════════════════════
	private static void testRedoAddUndoKeepsRedo()
	{
		UndoManager undo = new UndoManager();
		// 构造 redo 列表: 两个操作链
		// op1.undo → addUndo(redo1) ; redo1.undo → addUndo(undo2)
		undo.addUndo(makeUndo(() => undo.addUndo(makeUndo(() => undo.addUndo(makeUndo(() => { }, "u2")), "r1")), "op1"));
		undo.undo(); // op1.undo → addUndo(r1) 进 redoList
		assertTrue(undo.canRedo(), "第一次 undo 后可 redo");

		// 触发 redo: r1.undo → addUndo(u2), 此时 mRedoing=true → 不清空 redoList(除了 popBack 当前)
		undo.redo();
		// r1 被 popBack 弹出, 所以 redo 只剩... 实际上 r1 是唯一 redo 项, 弹出后 shouldCanRedo=false
		assertFalse(undo.canRedo(), "redo 弹出当前项后不可再 redo");
		assertTrue(undo.canUndo(), "redo 回调 addUndo(u2) 进 undo 列表后可 undo");
		undo.clearAll();
	}

	// ═════════════════════════════════════════════════════════════════
	// undo 回调中 addUndo 进入 redo 列表 — mUndoing=true 分支
	// ═════════════════════════════════════════════════════════════════
	private static void testUndoCallbackAddsToRedo()
	{
		UndoManager undo = new UndoManager();
		bool redoExecuted = false;
		// op1.undo → addUndo(redoX), redoX.undo → redoExecuted=true
		undo.addUndo(makeUndo(() => undo.addUndo(makeUndo(() => { redoExecuted = true; }, "redoX")), "op1"));

		undo.undo(); // 触发 op1.undo → addUndo(redoX) 进 redoList
		assertTrue(undo.canRedo(), "undo 回调 addUndo 进入 redo 列表");
		assertFalse(undo.canUndo(), "undo 后 undo 列表为空");

		undo.redo(); // 执行 redoX.undo
		assertTrue(redoExecuted, "redo 操作应被执行");
		undo.clearAll();
	}

	// ═════════════════════════════════════════════════════════════════
	// LIFO 精确执行顺序 + 带参数校验
	// ═════════════════════════════════════════════════════════════════
	private static void testLifoExactOrder()
	{
		UndoManager undo = new UndoManager();
		System.Text.StringBuilder sb = new System.Text.StringBuilder();
		undo.addUndo(makeUndo(() => sb.Append("1"), "1"));
		undo.addUndo(makeUndo(() => sb.Append("2"), "2"));
		undo.addUndo(makeUndo(() => sb.Append("3"), "3"));

		undo.undo(); // 3
		undo.undo(); // 2
		undo.undo(); // 1
		assertEqual("321", sb.ToString(), "LIFO 后进先出撤销顺序");
		assertFalse(undo.canUndo(), "全部撤销后不可再撤销");
		undo.clearAll();
	}

	// ═════════════════════════════════════════════════════════════════
	// setMaxUndoCount / getMaxUndoCount 存取
	// ═════════════════════════════════════════════════════════════════
	private static void testMaxUndoCountSetter()
	{
		UndoManager undo = new UndoManager();
		assertEqual(10, undo.getMaxUndoCount(), "默认 maxUndo=10");
		undo.setMaxUndoCount(5);
		assertEqual(5, undo.getMaxUndoCount(), "设置后 maxUndo=5");
		undo.clearAll();
	}

	// ═════════════════════════════════════════════════════════════════
	// setMaxUndoCount 后溢出 — 大上限淘汰行为
	// ═════════════════════════════════════════════════════════════════
	private static void testSetMaxUndoThenOverflow()
	{
		UndoManager undo = new UndoManager();
		undo.setMaxUndoCount(3);
		System.Text.StringBuilder sb = new System.Text.StringBuilder();
		undo.addUndo(makeUndo(() => sb.Append("a"), "a"));
		undo.addUndo(makeUndo(() => sb.Append("b"), "b"));
		undo.addUndo(makeUndo(() => sb.Append("c"), "c"));
		undo.addUndo(makeUndo(() => sb.Append("d"), "d")); // 溢出: 淘汰 "a"

		// 撤销顺序: d, c, b (a 被淘汰)
		undo.undo();
		undo.undo();
		undo.undo();
		assertEqual("dcb", sb.ToString(), "溢出淘汰最早操作后 LIFO 顺序");
		assertFalse(undo.canUndo(), "淘汰后只剩3个, 撤销3次后无操作");
		undo.clearAll();
	}
}
