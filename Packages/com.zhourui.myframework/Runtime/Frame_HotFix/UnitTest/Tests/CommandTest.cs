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
