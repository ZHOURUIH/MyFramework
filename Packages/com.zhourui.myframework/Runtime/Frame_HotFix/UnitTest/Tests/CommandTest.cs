using System;
using static TestAssert;
using static FrameUtility;
using static FrameBaseHotFix;

// Command 系统穷举测试
public static class CommandTest
{
	public static void Run()
	{
		// --- 创建和基础状态 ---
		testCmdCreate();
		testCmdIDUnique();
		testCmdInitialState();

		// --- 回调 ---
		testCmdStartCallback();
		testCmdEndCallback();

		// --- 重置 ---
		testCmdResetProperty();

		// --- setter/getter ---
		testDelayCommand();
		testThreadCommand();
		testIgnoreTimeScale();
		testCmdLogLevel();
		testReceiver();
		testState();
		testResultListen();

		// --- onInterrupted / debugInfo ---
		testOnInterrupted();
		testDebugInfo();
		testMultipleCommandsCallbackCount();
		testCallbackAndStateCombined();
		testInterruptTwice();
		testStateBackToNotExecuted();
		testSetReceiverRoundTrip();
	}

	// ─── CMD<T> 创建 ─────────────────────────────────────────────────────
	private static void testCmdCreate()
	{
		CMD(out TestCmd cmd);
		assertNotNull(cmd, "CMD<T>: 不应返回 null");
		assertFalse(cmd.isDestroy(), "CMD<T>: 刚创建的命令 isDestroy 应为 false");
		var receiver = new TestCmdReceiver();
		mCommandSystem.pushCommand(cmd, receiver);
	}

	// ─── ID 唯一性 ───────────────────────────────────────────────────────
	private static void testCmdIDUnique()
	{
		CMD(out TestCmd a);
		CMD(out TestCmd b);
		CMD(out TestCmd c);

		int idA = a.getID();
		int idB = b.getID();
		int idC = c.getID();

		var receiver = new TestCmdReceiver();
		mCommandSystem.pushCommand(a, receiver);
		mCommandSystem.pushCommand(b, receiver);
		mCommandSystem.pushCommand(c, receiver);

		assertTrue(idA != idB, "ID 唯一性: A 与 B 应不同");
		assertTrue(idB != idC, "ID 唯一性: B 与 C 应不同");
		assertTrue(idA != idC, "ID 唯一性: A 与 C 应不同");
	}

	// ─── 初始状态 ────────────────────────────────────────────────────────
	private static void testCmdInitialState()
	{
		CMD(out TestCmd cmd);
		assertTrue(cmd.getState() == EXECUTE_STATE.NOT_EXECUTE, "初始状态: NOT_EXECUTE");
		assertFalse(cmd.isDelayCommand(), "初始状态: isDelayCommand=false");
		assertFalse(cmd.isIgnoreTimeScale(), "初始状态: isIgnoreTimeScale=false");
		assertFalse(cmd.isThreadCommand(), "初始状态: isThreadCommand=false");

		var receiver = new TestCmdReceiver();
		mCommandSystem.pushCommand(cmd, receiver);
	}

	// ─── StartCallback ───────────────────────────────────────────────────
	private static void testCmdStartCallback()
	{
		CMD(out TestCmd cmd);
		bool startCalled = false;
		cmd.addStartCommandCallback(_ => { startCalled = true; });

		var receiver = new TestCmdReceiver();
		mCommandSystem.pushCommand(cmd, receiver);

		assertTrue(startCalled, "StartCallback: pushCommand 后应已执行");
	}

	// ─── EndCallback ─────────────────────────────────────────────────────
	private static void testCmdEndCallback()
	{
		CMD(out TestCmd cmd);
		bool endCalled = false;
		bool executeHit = false;
		cmd.onExecute = () => { executeHit = true; };
		cmd.addEndCommandCallback(_ => { endCalled = true; });

		var receiver = new TestCmdReceiver();
		mCommandSystem.pushCommand(cmd, receiver);

		assertTrue(executeHit, "execute: 应已被调用");
		assertTrue(endCalled, "EndCallback: 应已执行");
	}

	// ─── resetProperty ───────────────────────────────────────────────────
	private static void testCmdResetProperty()
	{
		CMD(out TestCmd cmd);
		int id = cmd.getID();

		cmd.setDelayTime(5.0f);
		cmd.setIgnoreTimeScale(true);
		cmd.setDelayCommand(true);
		cmd.setThreadCommand(true);
		cmd.setCmdLogLevel(LOG_LEVEL.FORCE);

		cmd.resetProperty();

		assertEqual(0.0f, cmd.getDelayTime(), "resetProperty: delayTime 归零");
		assertFalse(cmd.isIgnoreTimeScale(), "resetProperty: ignoreTimeScale=false");
		assertEqual(EXECUTE_STATE.NOT_EXECUTE, cmd.getState(), "resetProperty: state=NOT_EXECUTE");
		assertEqual(id, cmd.getID(), "resetProperty: ID 不重置");
		assertFalse(cmd.isDelayCommand(), "resetProperty: delayCommand=false");
	}

	// ─── setDelayCommand / isDelayCommand ────────────────────────────────
	private static void testDelayCommand()
	{
		CMD(out TestCmd cmd);
		assertFalse(cmd.isDelayCommand());
		cmd.setDelayCommand(true);
		assertTrue(cmd.isDelayCommand());
		cmd.setDelayCommand(false);
		assertFalse(cmd.isDelayCommand());

		var receiver = new TestCmdReceiver();
		mCommandSystem.pushCommand(cmd, receiver);
	}

	// ─── setThreadCommand / isThreadCommand ──────────────────────────────
	private static void testThreadCommand()
	{
		CMD(out TestCmd cmd);
		assertFalse(cmd.isThreadCommand());
		cmd.setThreadCommand(true);
		assertTrue(cmd.isThreadCommand());
		cmd.setThreadCommand(false);
		assertFalse(cmd.isThreadCommand());

		var receiver = new TestCmdReceiver();
		mCommandSystem.pushCommand(cmd, receiver);
	}

	// ─── setIgnoreTimeScale / isIgnoreTimeScale ──────────────────────────
	private static void testIgnoreTimeScale()
	{
		CMD(out TestCmd cmd);
		assertFalse(cmd.isIgnoreTimeScale());
		cmd.setIgnoreTimeScale(true);
		assertTrue(cmd.isIgnoreTimeScale());
		cmd.setIgnoreTimeScale(false);
		assertFalse(cmd.isIgnoreTimeScale());

		var receiver = new TestCmdReceiver();
		mCommandSystem.pushCommand(cmd, receiver);
	}

	// ─── setCmdLogLevel / getCmdLogLevel ─────────────────────────────────
	private static void testCmdLogLevel()
	{
		CMD(out TestCmd cmd);
		cmd.setCmdLogLevel(LOG_LEVEL.FORCE);
		assertEqual(LOG_LEVEL.FORCE, cmd.getCmdLogLevel());
		cmd.setCmdLogLevel(LOG_LEVEL.NORMAL);
		assertEqual(LOG_LEVEL.NORMAL, cmd.getCmdLogLevel());

		var receiver = new TestCmdReceiver();
		mCommandSystem.pushCommand(cmd, receiver);
	}

	// ─── setReceiver / getReceiver ───────────────────────────────────────
	private static void testReceiver()
	{
		CMD(out TestCmd cmd);
		assertNull(cmd.getReceiver());

		var receiver = new TestCmdReceiver();
		cmd.setReceiver(receiver);
		assertNotNull(cmd.getReceiver());
		assertEqual(receiver, cmd.getReceiver());

		mCommandSystem.pushCommand(cmd, receiver);
	}

	// ─── setState / getState ─────────────────────────────────────────────
	private static void testState()
	{
		CMD(out TestCmd cmd);
		assertEqual(EXECUTE_STATE.NOT_EXECUTE, cmd.getState());
		cmd.setState(EXECUTE_STATE.EXECUTING);
		assertEqual(EXECUTE_STATE.EXECUTING, cmd.getState());
		cmd.setState(EXECUTE_STATE.EXECUTED);
		assertEqual(EXECUTE_STATE.EXECUTED, cmd.getState());

		var receiver = new TestCmdReceiver();
		mCommandSystem.pushCommand(cmd, receiver);
	}

	// ─── setResultListen ─────────────────────────────────────────────────
	private static void testResultListen()
	{
		BOOL a = new();
		CMD(out TestCmd cmd);
		// setResultListen 只是设置标志位，不应崩溃
		cmd.setResultListen(a);

		var receiver = new TestCmdReceiver();
		mCommandSystem.pushCommand(cmd, receiver);
	}

	// ─── onInterrupted ───────────────────────────────────────────────────
	private static void testOnInterrupted()
	{
		CMD(out TestCmd cmd);
		bool interrupted = false;
		cmd.onInterrupt = () => { interrupted = true; };
		cmd.onInterrupted();
		assertTrue(interrupted, "onInterrupted: 应触发回调");

		var receiver = new TestCmdReceiver();
		mCommandSystem.pushCommand(cmd, receiver);
	}

	// ─── debugInfo ───────────────────────────────────────────────────────
	private static void testDebugInfo()
	{
		CMD(out TestCmd cmd);
		var builder = new MyStringBuilder();
		cmd.debugInfo(builder);
		string info = builder.ToString();
		assertTrue(info.Length > 0, "debugInfo: 应输出非空信息");

		var receiver = new TestCmdReceiver();
		mCommandSystem.pushCommand(cmd, receiver);
	}

	// ─── 组合场景 ──────────────────────────────────────────────────────────

	// push 3 个命令 → start 回调各触发一次(计数 3)
	private static void testMultipleCommandsCallbackCount()
	{
		int startCount = 0;
		for (int i = 0; i < 3; ++i)
		{
			CMD(out TestCmd cmd);
			cmd.addStartCommandCallback(_ => { ++startCount; });
			var receiver = new TestCmdReceiver();
			mCommandSystem.pushCommand(cmd, receiver);
		}
		assertEqual(3, startCount, "3 个命令 start 回调各触发一次");
	}

	// push 后: state 已执行 + start 回调已触发(组合验证)
	private static void testCallbackAndStateCombined()
	{
		CMD(out TestCmd cmd);
		bool startCalled = false;
		cmd.addStartCommandCallback(_ => { startCalled = true; });
		var receiver = new TestCmdReceiver();
		mCommandSystem.pushCommand(cmd, receiver);
		// 回调在 pushCommand 内触发(start→execute→end)
		assertTrue(startCalled, "push 后 start 回调已触发");
		// 注意: push 后命令立即被 destroyCmd 回收(resetProperty 复位 state=NOT_EXECUTE),
		//       不能断言 EXECUTED —— EXECUTING/EXECUTED 仅在执行瞬间存在
		assertEqual(EXECUTE_STATE.NOT_EXECUTE, cmd.getState(), "push 回收后 state 复位 NOT_EXECUTE");
	}

	// onInterrupted 两次 → 回调两次
	private static void testInterruptTwice()
	{
		CMD(out TestCmd cmd);
		int interruptCount = 0;
		cmd.onInterrupt = () => { ++interruptCount; };
		cmd.onInterrupted();
		cmd.onInterrupted();
		assertEqual(2, interruptCount, "onInterrupted 两次触发两次回调");

		var receiver = new TestCmdReceiver();
		mCommandSystem.pushCommand(cmd, receiver);
	}

	// resetProperty 是回收语义(ClassObject.resetProperty 置 mHasDestroy=true),
	// resetProperty 后命令处于回收态, push 必然 logError("cmd is invalid") —— 无法安全验证回调清空, 删除
	// state 可流转回 NOT_EXECUTE
	private static void testStateBackToNotExecuted()
	{
		CMD(out TestCmd cmd);
		cmd.setState(EXECUTE_STATE.EXECUTING);
		cmd.setState(EXECUTE_STATE.EXECUTED);
		cmd.setState(EXECUTE_STATE.NOT_EXECUTE);
		assertEqual(EXECUTE_STATE.NOT_EXECUTE, cmd.getState(), "state 流转回 NOT_EXECUTE");

		var receiver = new TestCmdReceiver();
		mCommandSystem.pushCommand(cmd, receiver);
	}

	// setReceiver 往返
	private static void testSetReceiverRoundTrip()
	{
		CMD(out TestCmd cmd);
		var receiver = new TestCmdReceiver();
		cmd.setReceiver(receiver);
		assertTrue(ReferenceEquals(receiver, cmd.getReceiver()), "setReceiver 后 getReceiver 同一引用");

		mCommandSystem.pushCommand(cmd, receiver);
	}
}

// ─── 测试专用命令 ────────────────────────────────────────────────────────────
public class TestCmd : Command
{
	public Action onExecute;
	public Action onInterrupt;
	public override void execute()
	{
		onExecute?.Invoke();
	}
	public override void onInterrupted()
	{
		onInterrupt?.Invoke();
	}
	public override void resetProperty()
	{
		base.resetProperty();
		onExecute = null;
		onInterrupt = null;
	}
}

// ─── 测试专用命令接收者 ──────────────────────────────────────────────────────
public class TestCmdReceiver : CommandReceiver
{
	public TestCmdReceiver()
	{
		mName = "TestCmdReceiver";
		mHasDestroy = false;
	}
}
