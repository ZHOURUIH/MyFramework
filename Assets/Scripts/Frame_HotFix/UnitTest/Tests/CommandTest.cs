#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using static FrameUtility;
using static FrameBaseHotFix;

// Command 系统集成测试
// 需要框架完整启动：使用 CMD<T>() 从对象池创建命令，通过 mCommandSystem.pushCommand 执行
public static class CommandTest
{
    public static void Run()
    {
        testCmdCreate();
        testCmdIDUnique();
        testCmdInitialState();
        testCmdStartCallback();
        testCmdEndCallback();
        testCmdResetProperty();
    }

    // ─── CMD<T> 创建 ─────────────────────────────────────────────────────

    private static void testCmdCreate()
    {
        CMD(out TestCmd cmd);
        AssertNotNull(cmd, "CMD<T>: 不应返回 null");
        Assert(!cmd.isDestroy(), "CMD<T>: 刚创建的命令 isDestroy 应为 false");
        // 测试用命令接收者（框架需要 receiver 才能 push）
        var receiver = new TestCmdReceiver();
        mCommandSystem.pushCommand(cmd, receiver);
        // pushCommand 执行后命令被自动回收（isDestroy = true），引用不置 null
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

        // 归还，避免泄漏
        var receiver = new TestCmdReceiver();
        mCommandSystem.pushCommand(a, receiver);
        mCommandSystem.pushCommand(b, receiver);
        mCommandSystem.pushCommand(c, receiver);

        Assert(idA != idB, $"ID 唯一性: A({idA}) 与 B({idB}) 应不同");
        Assert(idB != idC, $"ID 唯一性: B({idB}) 与 C({idC}) 应不同");
        Assert(idA != idC, $"ID 唯一性: A({idA}) 与 C({idC}) 应不同");
    }

    // ─── 初始状态 ────────────────────────────────────────────────────────

    private static void testCmdInitialState()
    {
        CMD(out TestCmd cmd);
        // 刚从池中取出，初始状态应是 NOT_EXECUTE
        Assert(cmd.getState() == EXECUTE_STATE.NOT_EXECUTE,
            $"初始状态: 应为 NOT_EXECUTE，实际 {cmd.getState()}");
        Assert(!cmd.isDelayCommand(), "初始状态: isDelayCommand 应为 false");
        Assert(!cmd.isIgnoreTimeScale(), "初始状态: isIgnoreTimeScale 应为 false");

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

        Assert(startCalled, "StartCallback: pushCommand 后 start 回调应已执行");
    }

    // ─── EndCallback ─────────────────────────────────────────────────────

    private static void testCmdEndCallback()
    {
        CMD(out TestCmd cmd);
        bool endCalled   = false;
        bool executeHit  = false;
        cmd.onExecute    = () => { executeHit = true; };
        cmd.addEndCommandCallback(_ => { endCalled = true; });

        var receiver = new TestCmdReceiver();
        mCommandSystem.pushCommand(cmd, receiver);

        Assert(executeHit, "execute: TestCmd.execute 应已被调用");
        Assert(endCalled,  "EndCallback: pushCommand 后 end 回调应已执行");
    }

    // ─── resetProperty ───────────────────────────────────────────────────

    private static void testCmdResetProperty()
    {
        CMD(out TestCmd cmd);
        int id = cmd.getID();       // CmdID 不重置

        cmd.setDelayTime(5.0f);
        cmd.setIgnoreTimeScale(true);

        // resetProperty 模拟对象归还时的状态
        cmd.resetProperty();

        AssertEqual(0.0f, cmd.getDelayTime(),     "resetProperty: delayTime 应归零");
        Assert(!cmd.isIgnoreTimeScale(),          "resetProperty: ignoreTimeScale 应为 false");
        AssertEqual(EXECUTE_STATE.NOT_EXECUTE, cmd.getState(), "resetProperty: state 应回到 NOT_EXECUTE");
        AssertEqual(id, cmd.getID(),              "resetProperty: CmdID 不应被重置");

        // 防止泄漏：resetProperty 后 isDestroy = true（base.resetProperty()），直接丢弃
    }

    // Simple assertion methods
    private static void Assert(bool condition, string message = "")
    {
        if (!condition)
        {
            throw new Exception($"Assertion failed: {message}");
        }
    }

    private static void AssertEqual<T>(T expected, T actual, string message = "")
    {
        bool eq = (expected == null && actual == null)
               || (expected != null && expected.Equals(actual));
        if (!eq)
        {
            throw new Exception(
                string.IsNullOrEmpty(message)
                    ? $"Expected [{expected}] but got [{actual}]"
                    : $"{message} - Expected [{expected}] but got [{actual}]");
        }
    }

    private static void AssertNotNull(object obj, string message = "")
    {
        if (obj == null)
        {
            throw new Exception(string.IsNullOrEmpty(message) ? "Object should not be null" : message);
        }
    }
}

// ─── 测试专用命令 ────────────────────────────────────────────────────────────

public class TestCmd : Command
{
    public System.Action onExecute;
    public override void execute()
    {
        onExecute?.Invoke();
    }
    public override void resetProperty()
    {
        base.resetProperty();
        onExecute = null;
    }
}

// ─── 测试专用命令接收者 ──────────────────────────────────────────────────────

public class TestCmdReceiver : CommandReceiver
{
    public TestCmdReceiver()
    {
        mName = "TestCmdReceiver";
        // 模拟激活状态
        mHasDestroy = false;
    }
}
#endif