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
		// ─── CmdGlobalDelayCallParam2<T0,T1> 与 Param5<T0..T4> 代表性多参变体 ───
		test_Param2_ExecuteMultiParams();
		test_Param2_GuardMismatch();
		test_Param5_ExecuteMultiParams();
		test_Param5_GuardMismatch();
		test_Param5_ResetProperty();
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
	// CmdGlobalDelayCallParam2 — 双参传递 + guard
	// ═════════════════════════════════════════════════════════════════
	private static void test_Param2_ExecuteMultiParams()
	{
		CmdGlobalDelayCallParam2<int, string> cmd = new CmdGlobalDelayCallParam2<int, string>();
		int i0 = 0; string i1 = null;
		cmd.mFunction = (int a, string b) => { i0 = a; i1 = b; };
		cmd.mParam0 = 7; cmd.mParam1 = "hello";
		cmd.execute();
		assertEqual(7, i0, "Param2 execute 应传 mParam0");
		assertEqual("hello", i1, "Param2 execute 应传 mParam1");
	}

	// ═════════════════════════════════════════════════════════════════
	// CmdGlobalDelayCallParam2 — guard 不匹配跳过(多参版)
	// ═════════════════════════════════════════════════════════════════
	private static void test_Param2_GuardMismatch()
	{
		CmdGlobalDelayCallParam2<float, bool> cmd = new CmdGlobalDelayCallParam2<float, bool>();
		bool called = false;
		cmd.mFunction = (float a, bool b) => { called = true; };
		TestRecyclable guard = new TestRecyclable(1);
		cmd.setGuard(guard);
		guard.mAssignID = 2;
		cmd.execute();
		assertFalse(called, "Param2 guard 不匹配时应跳过");
	}

	// ═════════════════════════════════════════════════════════════════
	// CmdGlobalDelayCallParam5 — 五参传递(参数最多变体)
	// ═════════════════════════════════════════════════════════════════
	private static void test_Param5_ExecuteMultiParams()
	{
		CmdGlobalDelayCallParam5<int, int, int, int, int> cmd = new CmdGlobalDelayCallParam5<int, int, int, int, int>();
		int[] got = new int[5];
		cmd.mFunction = (int a, int b, int c, int d, int e) => { got[0] = a; got[1] = b; got[2] = c; got[3] = d; got[4] = e; };
		cmd.mParam0 = 1; cmd.mParam1 = 2; cmd.mParam2 = 3; cmd.mParam3 = 4; cmd.mParam4 = 5;
		cmd.execute();
		assertTrue(got[0] == 1 && got[1] == 2 && got[2] == 3 && got[3] == 4 && got[4] == 5,
			"Param5 execute 应按序传 mParam0..mParam4");
	}

	// ═════════════════════════════════════════════════════════════════
	// CmdGlobalDelayCallParam5 — guard 不匹配跳过
	// ═════════════════════════════════════════════════════════════════
	private static void test_Param5_GuardMismatch()
	{
		CmdGlobalDelayCallParam5<int, string, bool, float, long> cmd = new CmdGlobalDelayCallParam5<int, string, bool, float, long>();
		bool called = false;
		cmd.mFunction = (int a, string b, bool c, float d, long e) => { called = true; };
		TestRecyclable guard = new TestRecyclable(9);
		cmd.setGuard(guard);
		guard.mAssignID = 10;
		cmd.execute();
		assertFalse(called, "Param5 guard 不匹配时应跳过");
	}

	// ═════════════════════════════════════════════════════════════════
	// CmdGlobalDelayCallParam5 — resetProperty 清空多参状态
	// ═════════════════════════════════════════════════════════════════
	private static void test_Param5_ResetProperty()
	{
		CmdGlobalDelayCallParam5<int, string, bool, float, long> cmd = new CmdGlobalDelayCallParam5<int, string, bool, float, long>();
		bool called = false;
		cmd.mFunction = (int a, string b, bool c, float d, long e) => { called = true; };
		cmd.mParam0 = 1;
		cmd.resetProperty();
		cmd.execute();
		assertFalse(called, "Param5 reset 后 function 被清空, execute 不调用");
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
