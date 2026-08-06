using System;
using static TestAssert;

// 延迟调用命令单元测试 — CmdGlobalDelayCall(无参) + CmdGlobalDelayCallParam1<T>(带参)
// 继承 Command(构造调用 makeID, 纯逻辑, new 安全), 覆盖 resetProperty/setGuard/execute 的 guard 校验机制
public static class CmdGlobalDelayCallTest
{
	public static void Run()
	{
		// ─── CmdGlobalDelayCall ───
		test_Execute_NoFunction();
		test_Execute_CallsFunction();
		test_Execute_GuardMatch();
		test_Execute_GuardMismatch();
		test_ResetProperty();
		// ─── CmdGlobalDelayCallParam1<T> ───
		test_Param1_Execute();
		test_Param1_GuardMismatch();
		test_Param1_ResetProperty();
	}

	// ═════════════════════════════════════════════════════════════════
	// execute — 无 function 安全不抛
	// ═════════════════════════════════════════════════════════════════
	private static void test_Execute_NoFunction()
	{
		CmdGlobalDelayCall cmd = new CmdGlobalDelayCall();
		cmd.execute();
		assertTrue(true, "无 function 时 execute 安全不抛");
	}

	// ═════════════════════════════════════════════════════════════════
	// execute — 有 function 正常调用
	// ═════════════════════════════════════════════════════════════════
	private static void test_Execute_CallsFunction()
	{
		CmdGlobalDelayCall cmd = new CmdGlobalDelayCall();
		bool called = false;
		cmd.mFunction = () => { called = true; };
		cmd.execute();
		assertTrue(called, "execute 应调用 mFunction");
	}

	// ═════════════════════════════════════════════════════════════════
	// execute — guard 匹配(assignID 一致)时执行
	// ═════════════════════════════════════════════════════════════════
	private static void test_Execute_GuardMatch()
	{
		CmdGlobalDelayCall cmd = new CmdGlobalDelayCall();
		bool called = false;
		cmd.mFunction = () => { called = true; };
		TestRecyclable guard = new TestRecyclable(100);
		cmd.setGuard(guard);
		cmd.execute();
		assertTrue(called, "guard 匹配时应调用 mFunction");
	}

	// ═════════════════════════════════════════════════════════════════
	// execute — guard 不匹配(assignID 已变化)时跳过
	// ═════════════════════════════════════════════════════════════════
	private static void test_Execute_GuardMismatch()
	{
		CmdGlobalDelayCall cmd = new CmdGlobalDelayCall();
		bool called = false;
		cmd.mFunction = () => { called = true; };
		TestRecyclable guard = new TestRecyclable(100);
		cmd.setGuard(guard);
		// 模拟对象被回收后 assignID 变化
		guard.mAssignID = 999;
		cmd.execute();
		assertFalse(called, "guard 不匹配时不应调用 mFunction");
	}

	// ═════════════════════════════════════════════════════════════════
	// resetProperty — 清空 function/guard
	// ═════════════════════════════════════════════════════════════════
	private static void test_ResetProperty()
	{
		CmdGlobalDelayCall cmd = new CmdGlobalDelayCall();
		bool called = false;
		cmd.mFunction = () => { called = true; };
		cmd.setGuard(new TestRecyclable(1));
		cmd.resetProperty();
		cmd.execute();
		assertFalse(called, "reset 后 function 被清空, execute 不调用");
	}

	// ═════════════════════════════════════════════════════════════════
	// CmdGlobalDelayCallParam1 — 带参执行
	// ═════════════════════════════════════════════════════════════════
	private static void test_Param1_Execute()
	{
		CmdGlobalDelayCallParam1<int> cmd = new CmdGlobalDelayCallParam1<int>();
		int received = 0;
		cmd.mFunction = (int value) => { received = value; };
		cmd.mParam = 42;
		cmd.execute();
		assertEqual(42, received, "带参 execute 应传入 mParam");
	}

	// ═════════════════════════════════════════════════════════════════
	// CmdGlobalDelayCallParam1 — guard 不匹配跳过
	// ═════════════════════════════════════════════════════════════════
	private static void test_Param1_GuardMismatch()
	{
		CmdGlobalDelayCallParam1<int> cmd = new CmdGlobalDelayCallParam1<int>();
		bool called = false;
		cmd.mFunction = (int value) => { called = true; };
		TestRecyclable guard = new TestRecyclable(7);
		cmd.setGuard(guard);
		guard.mAssignID = 8;
		cmd.execute();
		assertFalse(called, "guard 不匹配时带参 execute 跳过");
	}

	// ═════════════════════════════════════════════════════════════════
	// CmdGlobalDelayCallParam1 — resetProperty 清空
	// ═════════════════════════════════════════════════════════════════
	private static void test_Param1_ResetProperty()
	{
		CmdGlobalDelayCallParam1<string> cmd = new CmdGlobalDelayCallParam1<string>();
		string received = null;
		cmd.mFunction = (string value) => { received = value; };
		cmd.mParam = "hello";
		cmd.resetProperty();
		cmd.execute();
		assertNull(received, "reset 后带参 execute 不调用");
	}

	// ═════════════════════════════════════════════════════════════════
	// IRecyclable mock — 允许修改 assignID 以模拟对象被回收
	// ═════════════════════════════════════════════════════════════════
	private class TestRecyclable : IRecyclable
	{
		public long mAssignID;
		public TestRecyclable(long id) { mAssignID = id; }
		public void setAssignID(long assignID) { mAssignID = assignID; }
		public long getAssignID() { return mAssignID; }
	}
}
