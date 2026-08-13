using System;
using static FrameUtility;
using static TestAssert;

// WaitingManager 深度测试 — 等待对象的复杂调度逻辑(update 生命周期)
// 覆盖 WaitingManager 在 update 中遍历等待对象, 根据完成/取消状态触发 done 回调并自动销毁的复杂交互
//   isDone → done() 回调 + autoDestroy 移除
//   isCancel → 不调 done, autoDestroy 移除
//   非 autoDestroy → 完成后保留在列表(可复用)
//   多等待对象部分完成/全部完成的混合调度
public static class WaitingManagerTest
{
	// 测试子类, 暴露 protected mList
	class TestWaitingManager : WaitingManager
	{
		public int GetListCount() { return mList.count(); }
	}

	public static void Run()
	{
		testConditionDoneAutoDestroy();
		testConditionNotDoneKept();
		testCancelDoneNotCalled();
		testNonAutoDestroyKeptAfterDone();
		testMultipleWaitMixed();
		testAsyncWaitOverload();
		testCancelRefDestroy();
		testManualDestroyWait();
	}

	// ═════════════════════════════════════════════════════════════════
	// 条件满足 → update 触发 done 回调 + autoDestroy 移除
	// ═════════════════════════════════════════════════════════════════
	private static void testConditionDoneAutoDestroy()
	{
		var mgr = new TestWaitingManager();
		bool condition = false;
		int doneCount = 0;
		mgr.wait(() => condition, () => doneCount++, true);

		// 条件未满足: update 不回调不移除
		mgr.update(0.1f);
		assertEqual(0, doneCount, "条件未满足不触发回调");
		assertEqual(1, mgr.GetListCount(), "条件未满足不移除");

		// 条件满足: update 触发回调 + autoDestroy 移除
		condition = true;
		mgr.update(0.1f);
		assertEqual(1, doneCount, "条件满足触发一次回调");
		assertEqual(0, mgr.GetListCount(), "autoDestroy 移除等待对象");
	}

	// ═════════════════════════════════════════════════════════════════
	// 条件一直未满足 → 一直保留, 不回调
	// ═════════════════════════════════════════════════════════════════
	private static void testConditionNotDoneKept()
	{
		var mgr = new TestWaitingManager();
		int doneCount = 0;
		mgr.wait(() => false, () => doneCount++, true);

		mgr.update(0.1f);
		mgr.update(0.1f);
		mgr.update(0.1f);
		assertEqual(0, doneCount, "条件从未满足, 永不回调");
		assertEqual(1, mgr.GetListCount(), "条件未满足一直保留");
	}

	// ═════════════════════════════════════════════════════════════════
	// 取消 → 不调 done, 但 autoDestroy 仍移除
	// ═════════════════════════════════════════════════════════════════
	private static void testCancelDoneNotCalled()
	{
		var mgr = new TestWaitingManager();
		bool condition = false;
		bool cancel = false;
		int doneCount = 0;
		var w = mgr.wait(() => condition, () => doneCount++, true);
		w.setCancelCondition(() => cancel);

		// 取消前 update: 不移除不回调
		mgr.update(0.1f);
		assertEqual(0, doneCount, "取消前不回调");
		assertEqual(1, mgr.GetListCount(), "取消前不移除");

		// 设为取消: update 不调 done, 但移除
		cancel = true;
		mgr.update(0.1f);
		assertEqual(0, doneCount, "取消时不调用 done 回调");
		assertEqual(0, mgr.GetListCount(), "取消时 autoDestroy 移除");
	}

	// ═════════════════════════════════════════════════════════════════
	// 非 autoDestroy → 完成后保留在列表, 回调只触发一次
	// ═════════════════════════════════════════════════════════════════
	private static void testNonAutoDestroyKeptAfterDone()
	{
		var mgr = new TestWaitingManager();
		bool condition = false;
		int doneCount = 0;
		mgr.wait(() => condition, () => doneCount++, false); // autoDestroy=false

		condition = true;
		mgr.update(0.1f);
		assertEqual(1, doneCount, "完成触发回调");
		assertEqual(1, mgr.GetListCount(), "非 autoDestroy 完成后保留");

		// 再次 update: 因 mHasCallDone 不再重复回调, 且非 autoDestroy 不移除
		mgr.update(0.1f);
		assertEqual(1, doneCount, "重复 update 不再回调");
		assertEqual(1, mgr.GetListCount(), "仍保留");
	}

	// ═════════════════════════════════════════════════════════════════
	// 混合多个等待对象 — 部分完成、部分未完成
	// ═════════════════════════════════════════════════════════════════
	private static void testMultipleWaitMixed()
	{
		var mgr = new TestWaitingManager();
		bool condA = false;
		bool condB = false;
		int doneA = 0, doneB = 0;
		mgr.wait(() => condA, () => doneA++, true);
		mgr.wait(() => condB, () => doneB++, true);
		assertEqual(2, mgr.GetListCount(), "两个等待对象");

		// 只满足 A
		condA = true;
		mgr.update(0.1f);
		assertEqual(1, doneA, "A 完成回调");
		assertEqual(0, doneB, "B 未完成不回调");
		assertEqual(1, mgr.GetListCount(), "只移除 A, 剩 B");

		// 满足 B
		condB = true;
		mgr.update(0.1f);
		assertEqual(1, doneB, "B 完成回调");
		assertEqual(0, mgr.GetListCount(), "B 也移除, 列表空");
	}

	// ═════════════════════════════════════════════════════════════════
	// wait(op0, op1) — 两个异步操作都完成才回调
	// ═════════════════════════════════════════════════════════════════
	private static void testAsyncWaitOverload()
	{
		var mgr = new TestWaitingManager();
		var op0 = new CustomAsyncOperation();
		var op1 = new CustomAsyncOperation();
		int doneCount = 0;
		mgr.wait(op0, op1, () => doneCount++, true);

		// 只完成 op0: 未全部完成
		op0.setFinish();
		mgr.update(0.1f);
		assertEqual(0, doneCount, "op0 完成但 op1 未完成, 不回调");
		assertEqual(1, mgr.GetListCount(), "未全部完成不移除");

		// 完成 op1: 全部完成
		op1.setFinish();
		mgr.update(0.1f);
		assertEqual(1, doneCount, "两个 op 都完成回调");
		assertEqual(0, mgr.GetListCount(), "全部完成 autoDestroy 移除");
	}

	// ═════════════════════════════════════════════════════════════════
	// cancel(ref waiting) — 手动取消等待对象
	// ═════════════════════════════════════════════════════════════════
	private static void testCancelRefDestroy()
	{
		var mgr = new TestWaitingManager();
		bool condition = false;
		int doneCount = 0;
		var w = mgr.wait(() => condition, () => doneCount++, true);
		assertEqual(1, mgr.GetListCount(), "创建后有1个");

		mgr.cancel(ref w);
		assertEqual(0, mgr.GetListCount(), "cancel 后移除");
		assertEqual(0, doneCount, "cancel 不触发 done 回调");

		// 条件再满足, 对象已移除, 不再回调
		condition = true;
		mgr.update(0.1f);
		assertEqual(0, doneCount, "已取消的对象不再回调");
	}

	// ═════════════════════════════════════════════════════════════════
	// destroyWait — 手动销毁等待对象
	// ═════════════════════════════════════════════════════════════════
	private static void testManualDestroyWait()
	{
		var mgr = new TestWaitingManager();
		int doneCount = 0;
		var w = mgr.createWait(() => doneCount++, true);
		assertEqual(1, mgr.GetListCount(), "createWait 后列表有1个");

		mgr.destroyWait(ref w);
		assertEqual(0, mgr.GetListCount(), "destroyWait 后移除");
	}
}
