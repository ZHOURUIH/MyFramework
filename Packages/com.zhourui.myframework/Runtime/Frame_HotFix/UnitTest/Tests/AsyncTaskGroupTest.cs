using System.Collections;
using System.Collections.Generic;
using static TestAssert;

// AsyncTaskGroup 单元测试 — 覆盖多协程任务组的添加/完成判定/回调触发
public static class AsyncTaskGroupTest
{
	public static void Run()
	{
		testResetProperty();
		testEmptyGroupDone();
		testPendingTaskNotDone();
		testAllTasksDoneTriggersCallback();
		testAddNullTaskIgnored();
		testSetCallbackOverrides();
		testDoneDoesNotClearTasks();
		// ── CustomAsyncOperation / CustomMultiAsyncOperation ──
		testCustomAsyncOpDefaultWaiting();
		testCustomAsyncOpSetFinish();
		testCustomAsyncOpReset();
		testCustomMultiAddOperation();
		testCustomMultiAllFinished();
		testCustomMultiAnyPending();
		testCustomMultiReset();
	}

	// 返回一个需执行一次 MoveNext 才完成的协程
	private static IEnumerator onceEnumerator()
	{
		yield return null;
	}

	// 返回一个立即完成的协程(空迭代)
	private static IEnumerator emptyEnumerator()
	{
		yield break;
	}

	// ─── resetProperty 清空 ───────────────────────────────────────────
	private static void testResetProperty()
	{
		var group = new AsyncTaskGroup();
		bool called = false;
		group.setCallback(() => { called = true; });
		group.addTask(emptyEnumerator());
		assertEqual(1, group.mEnumerators.Count, "添加任务后列表有1项");
		group.resetProperty();
		assertEqual(0, group.mEnumerators.Count, "resetProperty 清空任务列表");
		assertFalse(called, "resetProperty 不触发回调");
		group.checkDone();
		assertFalse(called, "清空后 checkDone 也不触发旧回调");
	}

	// ─── 空任务组立即完成并触发回调 ───────────────────────────────────
	private static void testEmptyGroupDone()
	{
		var group = new AsyncTaskGroup();
		bool called = false;
		group.setCallback(() => { called = true; });
		assertTrue(group.checkDone(), "空任务组 checkDone 立即返回 true");
		assertTrue(called, "空任务组 checkDone 触发回调");
	}

	// ─── 有待完成任务则未完成 ─────────────────────────────────────────
	private static void testPendingTaskNotDone()
	{
		var group = new AsyncTaskGroup();
		bool called = false;
		group.setCallback(() => { called = true; });
		group.addTask(onceEnumerator());
		assertFalse(group.checkDone(), "有待执行协程时 checkDone 返回 false");
		assertFalse(called, "未完成时不触发回调");
	}

	// ─── 所有任务完成后触发回调 ───────────────────────────────────────
	private static void testAllTasksDoneTriggersCallback()
	{
		var group = new AsyncTaskGroup();
		bool called = false;
		group.setCallback(() => { called = true; });
		group.addTask(onceEnumerator());
		group.addTask(emptyEnumerator());
		// 第一次 checkDone: onceEnumerator 执行一次 MoveNext 返回 true(未完成), emptyEnumerator 立即完成
		assertFalse(group.checkDone(), "还有任务未完成时返回 false");
		assertFalse(called, "未全部完成时不触发回调");
		// 第二次 checkDone: onceEnumerator 第二次 MoveNext 返回 false(完成), 全部完成
		assertTrue(group.checkDone(), "所有任务完成后 checkDone 返回 true");
		assertTrue(called, "全部完成后触发回调");
	}

	// ─── addTask(null) 不加入 ─────────────────────────────────────────
	private static void testAddNullTaskIgnored()
	{
		var group = new AsyncTaskGroup();
		group.addTask(null);
		assertEqual(0, group.mEnumerators.Count, "addTask(null) 不加入任务");
		assertTrue(group.checkDone(), "只有 null 任务时仍视为完成");
	}

	// ─── setCallback 覆盖旧回调 ───────────────────────────────────────
	private static void testSetCallbackOverrides()
	{
		var group = new AsyncTaskGroup();
		bool firstCalled = false;
		bool secondCalled = false;
		group.setCallback(() => { firstCalled = true; });
		group.setCallback(() => { secondCalled = true; });
		group.checkDone();
		assertFalse(firstCalled, "覆盖后旧回调不再触发");
		assertTrue(secondCalled, "覆盖后的新回调触发");
	}

	// ─── checkDone 完成时不自动清空任务(可再次调用) ───────────────────
	private static void testDoneDoesNotClearTasks()
	{
		var group = new AsyncTaskGroup();
		int callCount = 0;
		group.setCallback(() => { callCount++; });
		group.addTask(emptyEnumerator());
		assertTrue(group.checkDone(), "首次 checkDone 返回 true");
		assertTrue(group.checkDone(), "任务列表未清空时仍返回 true");
		assertEqual(2, callCount, "每次 checkDone 都触发回调");
	}

	// ═══════════════════════════════════════════════════════════════════
	// CustomAsyncOperation
	// ═══════════════════════════════════════════════════════════════════

	// 新建操作默认未完成(keepWaiting=true)
	private static void testCustomAsyncOpDefaultWaiting()
	{
		var op = new CustomAsyncOperation();
		assertTrue(op.keepWaiting, "新建 CustomAsyncOperation 默认 keepWaiting=true");
	}

	// setFinish() 置为完成, 返回 this
	private static void testCustomAsyncOpSetFinish()
	{
		var op = new CustomAsyncOperation();
		assertTrue(op.keepWaiting, "setFinish 前 keepWaiting=true");
		var ret = op.setFinish();
		assertTrue(ret == op, "setFinish 返回 this");
		assertFalse(op.keepWaiting, "setFinish 后 keepWaiting=false");
	}

	// Reset() 恢复为未完成
	private static void testCustomAsyncOpReset()
	{
		var op = new CustomAsyncOperation();
		op.setFinish();
		assertFalse(op.keepWaiting, "setFinish 后完成");
		op.Reset();
		assertTrue(op.keepWaiting, "Reset 后恢复 keepWaiting=true");
	}

	// ═══════════════════════════════════════════════════════════════════
	// CustomMultiAsyncOperation
	// ═══════════════════════════════════════════════════════════════════

	// addOperation 加入子操作
	private static void testCustomMultiAddOperation()
	{
		var multi = new CustomMultiAsyncOperation();
		var op = new CustomAsyncOperation();
		multi.addOperation(op);
		// 未加子操作时 keepWaiting 恒 false(直接走 mOperationList 为空的分支)
		multi.addOperation(op);
		assertTrue(multi.keepWaiting, "加入未完成子操作后 keepWaiting=true");
	}

	// 所有子操作完成 → keepWaiting=false
	private static void testCustomMultiAllFinished()
	{
		var multi = new CustomMultiAsyncOperation();
		var a = new CustomAsyncOperation().setFinish();
		var b = new CustomAsyncOperation().setFinish();
		multi.addOperation(a);
		multi.addOperation(b);
		assertFalse(multi.keepWaiting, "所有子操作完成 → keepWaiting=false");
	}

	// 任一子操作未完成 → keepWaiting=true
	private static void testCustomMultiAnyPending()
	{
		var multi = new CustomMultiAsyncOperation();
		var done = new CustomAsyncOperation().setFinish();
		var pending = new CustomAsyncOperation();
		multi.addOperation(done);
		multi.addOperation(pending);
		assertTrue(multi.keepWaiting, "任一子操作未完成 → keepWaiting=true");
	}

	// Reset() 清空子操作列表, 恢复默认(空列表→keepWaiting=false)
	private static void testCustomMultiReset()
	{
		var multi = new CustomMultiAsyncOperation();
		var pending = new CustomAsyncOperation();
		multi.addOperation(pending);
		assertTrue(multi.keepWaiting, "Reset 前有待完成子操作");
		multi.Reset();
		assertFalse(multi.keepWaiting, "Reset 清空子操作后 keepWaiting=false");
	}
}
