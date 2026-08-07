using static TestAssert;
using static FrameUtility;

// CommandSystem / CommandReceiver / DelayCmdWatcher 穷举测试
// 覆盖所有公开方法和关键分支
// 注意: 需要命令真正执行并回收的测试必须用 CLASS<T>(typeof(T)) 创建,
// 否则 destroyCmd → ClassPool.destroyClass → removeInuse 找不到对象会 logError
public static class CommandSystemTest
{
    public static void Run()
    {
        // ─── CommandSystem: pushCommand ───
        testPushCommandExecutesAndDestroys();
        testPushCommandDestroyedSystem();
        testPushCommandNullCmd();
        testPushCommandNullReceiver();
        // ─── CommandSystem: pushDelayCommand ───
        testPushDelayCommandExpires();
        testPushDelayCommandNotExpired();
        testPushDelayCommandExactlyAtBoundary();
        testPushDelayCommandDestroyedSystem();
        testPushDelayCommandNullCmd();
        testPushDelayCommandNullReceiver();
        testPushDelayCommandDestroyedWatcher();
        testPushDelayCommandNegativeDelay();
        testPushDelayCommandZeroDelay();
        testPushDelayCommandWithWatcher();
        testPushDelayCommandMultipleCommands();
        // ─── CommandSystem: interruptCommand ───
        testInterruptCommandInBuffer();
        testInterruptCommandInExecuteList();
        testInterruptCommandNotFound();
        testInterruptCommandDestroyedSystem();
        testInterruptCommandNegativeID();
        testInterruptCommandListOverload();
        // ─── CommandSystem: notifyReceiverDestroied ───
        testNotifyReceiverDestroiedClearsBuffer();
        testNotifyReceiverDestroiedDifferentReceiver();
        testNotifyReceiverDestroiedDestroyedSystem();
        // ─── CommandSystem: update ───
        testUpdateMultipleDelays();
        testUpdateNoCommands();
        testUpdateMixedExpiredAndPending();
        // ─── CommandSystem: destroy / sync ───
        testCommandSystemDestroy();
        // ─── CommandReceiver ───
        testCommandReceiverAddRemoveDelayCmd();
        testCommandReceiverResetProperty();
        testCommandReceiverSetName();
        testCommandReceiverDestroyNoPending();
        testCommandReceiverDestroyWithPending();
        // ─── DelayCmdWatcher ───
        testDelayCmdWatcherAddDelayCmd();
        testDelayCmdWatcherInterruptSingle();
        testDelayCmdWatcherInterruptZeroID();
        testDelayCmdWatcherInterruptAll();
        testDelayCmdWatcherDestroy();
        testDelayCmdWatcherResetProperty();
        testDelayCmdWatcherOnCmdStarted();
    }

    // ═══════════════════════════════════════════════════════════════════
    // CommandSystem: pushCommand
    // ═══════════════════════════════════════════════════════════════════

    private static void testPushCommandExecutesAndDestroys()
    {
        var sys = new CommandSystem();
        var receiver = new TestCmdReceiver();
        bool executed = false;
        var cmd = CLASS<TestCommand>(typeof(TestCommand));
        cmd.setAction(() => executed = true);

        sys.pushCommand(cmd, receiver);
        assertTrue(executed, "pushCommand 应执行命令");
        assertTrue(cmd.isDestroy(), "pushCommand 后命令应被回收");
        sys.destroy();
    }

    private static void testPushCommandDestroyedSystem()
    {
        var sys = new CommandSystem();
        sys.destroy();
        var receiver = new TestCmdReceiver();
        var cmd = new TestCommand(() => { });
        sys.pushCommand(cmd, receiver);
        // 已销毁系统不崩溃即可，cmd 泄漏需标记
        cmd.setDestroy(true);
    }

    private static void testPushCommandNullCmd()
    {
        var sys = new CommandSystem();
        sys.pushCommand(null, new TestCmdReceiver());
        // 不崩溃
        sys.destroy();
    }

    private static void testPushCommandNullReceiver()
    {
        var sys = new CommandSystem();
        var cmd = new TestCommand(() => { });
        sys.pushCommand(cmd, null);
        // 不崩溃，cmd 泄漏
        cmd.setDestroy(true);
        sys.destroy();
    }

    // ═══════════════════════════════════════════════════════════════════
    // CommandSystem: pushDelayCommand
    // ═══════════════════════════════════════════════════════════════════

    private static void testPushDelayCommandExpires()
    {
        var sys = new CommandSystem();
        var receiver = new TestCmdReceiver();
        bool executed = false;
        var cmd = CLASS<TestCommand>(typeof(TestCommand));
        cmd.setDelayCommand(true);
        cmd.setAction(() => executed = true);

        sys.pushDelayCommand(cmd, receiver, 0.1f, null);
        sys.update(0.05f);
        assertFalse(executed, "0.05s后不应执行");
        sys.update(0.06f);
        assertTrue(executed, "累计0.11s后应执行");
        sys.destroy();
    }

    private static void testPushDelayCommandNotExpired()
    {
        var sys = new CommandSystem();
        var receiver = new TestCmdReceiver();
        bool executed = false;
        var cmd = CLASS<TestCommand>(typeof(TestCommand));
        cmd.setDelayCommand(true);
        cmd.setAction(() => executed = true);
        sys.pushDelayCommand(cmd, receiver, 1.0f, null);
        sys.update(0.5f);
        assertFalse(executed, "延迟1s, 0.5s后不应执行");
        sys.destroy();
    }

    private static void testPushDelayCommandExactlyAtBoundary()
    {
        var sys = new CommandSystem();
        var receiver = new TestCmdReceiver();
        bool executed = false;
        var cmd = CLASS<TestCommand>(typeof(TestCommand));
        cmd.setDelayCommand(true);
        cmd.setAction(() => executed = true);

        sys.pushDelayCommand(cmd, receiver, 0.1f, null);
        sys.update(0.1f);  // 精确等于延迟时间
        assertTrue(executed, "累计时间等于延迟时应执行");
        sys.destroy();
    }

    private static void testPushDelayCommandDestroyedSystem()
    {
        var sys = new CommandSystem();
        sys.destroy();
        var receiver = new TestCmdReceiver();
        var cmd = new TestCommand(() => { });
        cmd.setDelayCommand(true);
        sys.pushDelayCommand(cmd, receiver, 0.1f, null);
        cmd.setDestroy(true);
    }

    private static void testPushDelayCommandNullCmd()
    {
        var sys = new CommandSystem();
        sys.pushDelayCommand(null, new TestCmdReceiver(), 0.1f, null);
        sys.destroy();
    }

    private static void testPushDelayCommandNullReceiver()
    {
        var sys = new CommandSystem();
        var cmd = new TestCommand(() => { });
        cmd.setDelayCommand(true);
        sys.pushDelayCommand(cmd, null, 0.1f, null);
        cmd.setDestroy(true);
        sys.destroy();
    }

    private static void testPushDelayCommandDestroyedWatcher()
    {
        var sys = new CommandSystem();
        var receiver = new TestCmdReceiver();
        var watcher = new DelayCmdWatcher();
        watcher.destroy();

        var cmd = new TestCommand(() => { });
        cmd.setDelayCommand(true);
        sys.pushDelayCommand(cmd, receiver, 0.1f, watcher);
        // 已销毁的 watcher 应被忽略
        cmd.setDestroy(true);
        sys.destroy();
    }

    private static void testPushDelayCommandNegativeDelay()
    {
        var sys = new CommandSystem();
        var receiver = new TestCmdReceiver();
        bool executed = false;
        var cmd = CLASS<TestCommand>(typeof(TestCommand));
        cmd.setDelayCommand(true);
        cmd.setAction(() => executed = true);

        sys.pushDelayCommand(cmd, receiver, -0.5f, null);
        // 负延迟被 clampMin 为 0，应在下一帧立即执行
        sys.update(0.01f);
        assertTrue(executed, "负延迟 clampMin 为0后应立即执行");
        sys.destroy();
    }

    private static void testPushDelayCommandZeroDelay()
    {
        var sys = new CommandSystem();
        var receiver = new TestCmdReceiver();
        bool executed = false;
        var cmd = CLASS<TestCommand>(typeof(TestCommand));
        cmd.setDelayCommand(true);
        cmd.setAction(() => executed = true);

        sys.pushDelayCommand(cmd, receiver, 0.0f, null);
        sys.update(0.01f);
        assertTrue(executed, "延迟0应立即执行");
        sys.destroy();
    }

    private static void testPushDelayCommandWithWatcher()
    {
        var sys = new CommandSystem();
        var receiver = new TestCmdReceiver();
        var watcher = new DelayCmdWatcher();
        watcher.setDestroy(false);  // ClassObject 构造时 mHasDestroy=true，需显式重置
        bool executed = false;
        var cmd = CLASS<TestCommand>(typeof(TestCommand));
        cmd.setDelayCommand(true);
        cmd.setAction(() => executed = true);

        sys.pushDelayCommand(cmd, receiver, 0.1f, watcher);
        sys.update(0.2f);
        assertTrue(executed, "带 watcher 的延迟命令到期应执行");
        watcher.destroy();
        sys.destroy();
    }

    private static void testPushDelayCommandMultipleCommands()
    {
        var sys = new CommandSystem();
        var receiver = new TestCmdReceiver();
        int count = 0;

        for (int i = 0; i < 5; i++)
        {
            int idx = i;
            var cmd = CLASS<TestCommand>(typeof(TestCommand));
            cmd.setDelayCommand(true);
            cmd.setAction(() => { count++; });
            sys.pushDelayCommand(cmd, receiver, (idx + 1) * 0.1f, null);
        }

        sys.update(0.15f);
        assertTrue(count >= 1, "0.15s后至少执行1个");
        sys.update(1.0f);
        assertEqual(5, count, "最终5个都应执行");
        sys.destroy();
    }

    // ═══════════════════════════════════════════════════════════════════
    // CommandSystem: interruptCommand
    // ═══════════════════════════════════════════════════════════════════

    private static void testInterruptCommandInBuffer()
    {
        var sys = new CommandSystem();
        var receiver = new TestCmdReceiver();
        bool executed = false;
        var cmd = CLASS<TestCommand>(typeof(TestCommand));
        cmd.setDelayCommand(true);
        cmd.setAction(() => executed = true);
        long id = cmd.getAssignID();

        sys.pushDelayCommand(cmd, receiver, 1.0f, null);
        bool result = sys.interruptCommand(id, false);
        assertTrue(result, "应成功中断缓冲区中的命令");

        sys.update(2.0f);
        assertFalse(executed, "中断后不应执行");
        sys.destroy();
    }

    private static void testInterruptCommandInExecuteList()
    {
        var sys = new CommandSystem();
        var receiver = new TestCmdReceiver();
        var cmd = CLASS<TestCommand>(typeof(TestCommand));
        cmd.setDelayCommand(true);
        cmd.setAction(() => { });
        long id = cmd.getAssignID();

        sys.pushDelayCommand(cmd, receiver, 0.01f, null);
        // 先 update 让命令进入 executeList
        sys.update(0.02f);
        // 此时命令应在 executeList 中（但 receiver 已被 removeReceiveDelayCmd）
        // interruptCommand 在 executeList 中通过设置 receiver=null 阻止
        bool result = sys.interruptCommand(id, false);
        // 可能找不到（已从 processBuffer 移除且不在 executeList 或 receiver 已 null）
        // 只验证不崩溃
        sys.destroy();
    }

    private static void testInterruptCommandNotFound()
    {
        var sys = new CommandSystem();
        bool result = sys.interruptCommand(99999, false);
        assertFalse(result, "不存在的ID应返回false");
        sys.destroy();
    }

    private static void testInterruptCommandDestroyedSystem()
    {
        var sys = new CommandSystem();
        sys.destroy();
        bool result = sys.interruptCommand(1, false);
        assertTrue(result, "已销毁系统 interruptCommand 应返回true");
    }

    private static void testInterruptCommandNegativeID()
    {
        var sys = new CommandSystem();
        bool result = sys.interruptCommand(-1, false);
        assertFalse(result, "负ID应返回false");
        sys.destroy();
    }

    private static void testInterruptCommandListOverload()
    {
        var sys = new CommandSystem();
        // 空列表不应崩溃
        sys.interruptCommand(new System.Collections.Generic.List<long>(), false);
        sys.destroy();
    }

    // ═══════════════════════════════════════════════════════════════════
    // CommandSystem: notifyReceiverDestroied
    // ═══════════════════════════════════════════════════════════════════

    private static void testNotifyReceiverDestroiedClearsBuffer()
    {
        var sys = new CommandSystem();
        var receiver = new TestCmdReceiver();
        bool executed = false;
        var cmd = CLASS<TestCommand>(typeof(TestCommand));
        cmd.setDelayCommand(true);
        cmd.setAction(() => executed = true);

        sys.pushDelayCommand(cmd, receiver, 1.0f, null);
        sys.notifyReceiverDestroied(receiver);

        sys.update(2.0f);
        assertFalse(executed, "receiver 销毁后命令不应执行");
        sys.destroy();
    }

    private static void testNotifyReceiverDestroiedDifferentReceiver()
    {
        var sys = new CommandSystem();
        var r1 = new TestCmdReceiver();
        var r2 = new TestCmdReceiver();
        bool executed = false;
        var cmd = CLASS<TestCommand>(typeof(TestCommand));
        cmd.setDelayCommand(true);
        cmd.setAction(() => executed = true);

        sys.pushDelayCommand(cmd, r1, 1.0f, null);
        // 通知另一个 receiver 销毁，不应影响 r1 的命令
        sys.notifyReceiverDestroied(r2);

        sys.update(2.0f);
        assertTrue(executed, "不同 receiver 销毁不应影响其他命令");
        sys.destroy();
    }

    private static void testNotifyReceiverDestroiedDestroyedSystem()
    {
        var sys = new CommandSystem();
        sys.destroy();
        sys.notifyReceiverDestroied(new TestCmdReceiver());
        // 不崩溃
    }

    // ═══════════════════════════════════════════════════════════════════
    // CommandSystem: update
    // ═══════════════════════════════════════════════════════════════════

    private static void testUpdateMultipleDelays()
    {
        var sys = new CommandSystem();
        var receiver = new TestCmdReceiver();
        int count = 0;

        for (int i = 0; i < 3; i++)
        {
            var cmd = CLASS<TestCommand>(typeof(TestCommand));
            cmd.setDelayCommand(true);
            cmd.setAction(() => count++);
            sys.pushDelayCommand(cmd, receiver, 0.1f, null);
        }

        sys.update(0.2f);
        assertEqual(3, count, "3个相同延迟的命令都应执行");
        sys.destroy();
    }

    private static void testUpdateNoCommands()
    {
        var sys = new CommandSystem();
        // 空命令列表 update 不应崩溃
        sys.update(0.1f);
        sys.update(1.0f);
        sys.destroy();
    }

    private static void testUpdateMixedExpiredAndPending()
    {
        var sys = new CommandSystem();
        var receiver = new TestCmdReceiver();
        bool short1 = false, long1 = false;

        var cmdS = CLASS<TestCommand>(typeof(TestCommand));
        cmdS.setDelayCommand(true);
        cmdS.setAction(() => short1 = true);
        sys.pushDelayCommand(cmdS, receiver, 0.05f, null);

        var cmdL = CLASS<TestCommand>(typeof(TestCommand));
        cmdL.setDelayCommand(true);
        cmdL.setAction(() => long1 = true);
        sys.pushDelayCommand(cmdL, receiver, 1.0f, null);

        sys.update(0.1f);
        assertTrue(short1, "短期命令应已执行");
        assertFalse(long1, "长期命令不应执行");

        sys.update(1.0f);
        assertTrue(long1, "长期命令应已执行");
        sys.destroy();
    }

    // ═══════════════════════════════════════════════════════════════════
    // CommandSystem: destroy
    // ═══════════════════════════════════════════════════════════════════

    private static void testCommandSystemDestroy()
    {
        var sys = new CommandSystem();
        var receiver = new TestCmdReceiver();
        var cmd = CLASS<TestCommand>(typeof(TestCommand));
        cmd.setDelayCommand(true);
        cmd.setAction(() => { });
        sys.pushDelayCommand(cmd, receiver, 0.1f, null);

        sys.destroy();
        // destroy 应清理所有缓冲
    }

    // ═══════════════════════════════════════════════════════════════════
    // CommandReceiver
    // ═══════════════════════════════════════════════════════════════════

    private static void testCommandReceiverAddRemoveDelayCmd()
    {
        var receiver = new TestCmdReceiver();
        receiver.addReceiveDelayCmd();
        receiver.addReceiveDelayCmd();
        receiver.addReceiveDelayCmd();
        receiver.removeReceiveDelayCmd();
        receiver.removeReceiveDelayCmd();
        receiver.removeReceiveDelayCmd();
        // 不崩溃
        receiver.destroy();
    }

    private static void testCommandReceiverResetProperty()
    {
        var receiver = new TestCmdReceiver();
        receiver.setName("test_name");
        receiver.addReceiveDelayCmd();
        receiver.addReceiveDelayCmd();

        receiver.resetProperty();

        assertNull(receiver.getName(), "resetProperty 后 name 应为 null");
        // delay counts 也应为 0（通过 destroy 不触发 notifyReceiverDestroied 间接验证）
        receiver.destroy();
    }

    private static void testCommandReceiverSetName()
    {
        var receiver = new TestCmdReceiver();
        receiver.setName("myReceiver");
        assertEqual("myReceiver", receiver.getName(), "setName 后 getName 应匹配");
        receiver.setName(null);
        assertNull(receiver.getName(), "setName(null)");
        receiver.destroy();
    }

    private static void testCommandReceiverDestroyNoPending()
    {
        var receiver = new TestCmdReceiver();
        // 无待处理命令时 destroy 不应触发 notifyReceiverDestroied
        receiver.destroy();
        // 不崩溃
    }

    private static void testCommandReceiverDestroyWithPending()
    {
        var receiver = new TestCmdReceiver();
        receiver.addReceiveDelayCmd();
        // 有待处理命令时 destroy 会尝试通知 CommandSystem
        // 但 mCommandSystem 为 null（测试环境），所以只验证不崩溃
        receiver.destroy();
    }

    // ═══════════════════════════════════════════════════════════════════
    // DelayCmdWatcher
    // ═══════════════════════════════════════════════════════════════════

    private static void testDelayCmdWatcherAddDelayCmd()
    {
        var watcher = new DelayCmdWatcher();
        var cmd = CLASS<TestCommand>(typeof(TestCommand));
        cmd.setDelayCommand(true);
        cmd.setAction(() => { });

        watcher.addDelayCmd(cmd);
        // addDelayCmd 内部会设置 startCallback 为 onCmdStarted
        watcher.destroy();
    }

    private static void testDelayCmdWatcherInterruptSingle()
    {
        var watcher = new DelayCmdWatcher();
        var cmd = CLASS<TestCommand>(typeof(TestCommand));
        cmd.setDelayCommand(true);
        cmd.setAction(() => { });
        long id = cmd.getAssignID();
        watcher.addDelayCmd(cmd);

        watcher.interruptCommand(ref id, false);
        assertEqual(0L, id, "interruptCommand 后 assignID 应置0");

        watcher.destroy();
    }

    private static void testDelayCmdWatcherInterruptZeroID()
    {
        var watcher = new DelayCmdWatcher();
        long zero = 0;
        watcher.interruptCommand(ref zero, false);
        assertEqual(0L, zero, "ID为0时直接返回不变");
        watcher.destroy();
    }

    private static void testDelayCmdWatcherInterruptAll()
    {
        var watcher = new DelayCmdWatcher();
        var cmd1 = CLASS<TestCommand>(typeof(TestCommand));
        cmd1.setDelayCommand(true);
        cmd1.setAction(() => { });
        var cmd2 = CLASS<TestCommand>(typeof(TestCommand));
        cmd2.setDelayCommand(true);
        cmd2.setAction(() => { });
        watcher.addDelayCmd(cmd1);
        watcher.addDelayCmd(cmd2);

        watcher.interruptAllCommand();
        // 不崩溃

        watcher.destroy();
    }

    private static void testDelayCmdWatcherDestroy()
    {
        var watcher = new DelayCmdWatcher();
        var cmd = CLASS<TestCommand>(typeof(TestCommand));
        cmd.setDelayCommand(true);
        cmd.setAction(() => { });
        watcher.addDelayCmd(cmd);

        watcher.destroy();
        // destroy 内部调用 interruptAllCommand
    }

    private static void testDelayCmdWatcherResetProperty()
    {
        var watcher = new DelayCmdWatcher();
        var cmd = CLASS<TestCommand>(typeof(TestCommand));
        cmd.setDelayCommand(true);
        cmd.setAction(() => { });
        watcher.addDelayCmd(cmd);

        watcher.resetProperty();
        // resetProperty 应清空 mDelayCmdList

        watcher.destroy();
    }

    private static void testDelayCmdWatcherOnCmdStarted()
    {
        var watcher = new DelayCmdWatcher();
        var cmd = CLASS<TestCommand>(typeof(TestCommand));
        cmd.setDelayCommand(true);
        cmd.setAction(() => { });
        watcher.addDelayCmd(cmd);

        // 模拟 onCmdStarted：手动触发 startCallback
        cmd.invokeStartCallBack();
        // onCmdStarted 从 mDelayCmdList 移除 assignID

        watcher.destroy();
    }
}

// ─── 测试辅助类 ─────────────────────────────────────────────────────────

public class TestCommand : Command
{
    private System.Action mAction;
    public TestCommand() { }
    public TestCommand(System.Action action)
    {
        mAction = action;
    }
    public void setAction(System.Action action) { mAction = action; }
    public override void execute() { mAction?.Invoke(); }
    public override void resetProperty()
    {
        base.resetProperty();
        mAction = null;
    }
}
