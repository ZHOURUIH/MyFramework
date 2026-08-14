using static TestAssert;

// MyTimer / MyTimer1 计时器测试
// 覆盖：init / start / stop / tickTimer / isCounting / getTimePercent /
//        resetToInterval / setInterval / setEnsureInterval / loop / non-loop
public static class MyTimerTest
{
    public static void Run()
    {
        testInitAndState();
        testTickNonLoop();
        testTickLoop();
        testTickLoopEnsureInterval();
        testStop();
        testResetToInterval();
        testGetTimePercent();
        testNotStarted();
        testZeroInterval();
        testMyTimer1Basic();
        testSetInterval();
        testNegativeDeltaTime();
        testGetTimePercentOverOne();
        testStopRestartCycle();
        testLoopMultipleFires();
        testPercentMonotonic();
        testIntervalChangeMidway();
        testStartResetsCurTime();
        testEnsureIntervalToggle();
        testResetToIntervalMidway();
        testInitTwiceResets();
    }

    // ─── init 与初始状态 ─────────────────────────────────────────────────
    private static void testInitAndState()
    {
        var t = new MyTimer();
        assert(!t.isCounting(), "MyTimer 默认未开始计时");

        t.init(0.0f, 1.0f, true);
        assert(t.isCounting(),                 "MyTimer init后开始计时");
        assert(t.mTimeInterval.isEqual(1.0f), "MyTimer interval=1");
        assert(t.mLoop,                         "MyTimer loop=true");

        t.start();
        assert(t.isCounting(), "MyTimer start 后 isCounting=true");
        assert(t.mCurTime.isEqual(0.0f), "MyTimer start curTime=0");
    }

    // ─── 非循环模式 tickTimer ────────────────────────────────────────────
    private static void testTickNonLoop()
    {
        var t = new MyTimer();
        t.init(0.0f, 1.0f, false);   // curTime=0, interval=1, loop=false

        // 未到达
        bool fired = t.tickTimer(0.5f);
        assert(!fired,           "non-loop 未到达不触发");
        assert(t.isCounting(),   "non-loop 未到达仍在计时");

        // 到达
        fired = t.tickTimer(0.6f);   // 0.5+0.6=1.1 ≥ 1
        assert(fired,             "non-loop 到达触发");
        assert(!t.isCounting(),   "non-loop 触发后停止计时");
        assert(t.mCurTime.isEqual(-1.0f), "non-loop 触发后 curTime=-1");

        // 触发后再 tick 不应触发
        fired = t.tickTimer(2.0f);
        assert(!fired, "non-loop 停止后 tick 不触发");
    }

    // ─── 循环模式 tickTimer ──────────────────────────────────────────────
    private static void testTickLoop()
    {
        var t = new MyTimer();
        t.init(0.0f, 1.0f, true);   // loop=true, ensureInterval=false

        // 第一次触发
        bool fired = t.tickTimer(1.5f);
        assert(fired, "loop 第一次触发");
        // 溢出0.5秒应被携带到下一个计时周期
        assert(t.isCounting(),   "loop 触发后继续计时");
        assert(t.mCurTime.isEqual(0.5f, 0.001f), "loop 携带溢出时间0.5");

        // 再 tick 0.4 不触发
        fired = t.tickTimer(0.4f);
        assert(!fired, "loop 0.9s 不触发");

        // 再 tick 0.1 触发
        fired = t.tickTimer(0.1f);
        assert(fired, "loop 第二次触发");
    }

    // ─── ensureInterval 模式 ─────────────────────────────────────────────
    private static void testTickLoopEnsureInterval()
    {
        var t = new MyTimer();
        t.init(0.0f, 1.0f, true);
        t.setEnsureInterval(true);

        bool fired = t.tickTimer(1.5f);
        assert(fired, "ensureInterval 触发");
        // ensureInterval=true 时 curTime 重置为0，不携带溢出
        assert(t.mCurTime.isEqual(0.0f), "ensureInterval 重置curTime=0");
    }

    // ─── stop ────────────────────────────────────────────────────────────
    private static void testStop()
    {
        var t = new MyTimer();
        t.init(0.0f, 2.0f, true);

        // stop 带reset
        t.stop(true);
        assert(!t.isCounting(),                 "stop(true) 停止计时");
        assert(t.mTimeInterval.isEqual(-1.0f), "stop(true) interval重置为-1");

        // stop 不带reset：只停时间，interval保留
        t.init(0.0f, 2.0f, true);
        t.stop(false);
        assert(!t.isCounting(),                     "stop(false) 停止计时");
        assert(t.mTimeInterval.isEqual(2.0f), "stop(false) interval保留2");
    }

    // ─── resetToInterval ─────────────────────────────────────────────────
    private static void testResetToInterval()
    {
        var t = new MyTimer();
        t.init(0.0f, 3.0f, false);
        t.resetToInterval();
        assert(t.mCurTime.isEqual(t.mTimeInterval), "resetToInterval curTime=interval");

        // resetToInterval 后第一次 tick 小量应立即触发
        bool fired = t.tickTimer(0.01f);
        assert(fired, "resetToInterval 后小量tick立即触发");
    }

    // ─── getTimePercent ──────────────────────────────────────────────────
    private static void testGetTimePercent()
    {
        var t = new MyTimer();
        t.init(0.0f, 2.0f, true);
        t.tickTimer(1.0f);   // curTime=1
        float pct = t.getTimePercent();
        assert(pct.isEqual(0.5f, 0.001f), "getTimePercent 0.5");

        // interval<=0 时返回0
        var t2 = new MyTimer();
        t2.init(0.0f, -1.0f, true);
        assert(t2.getTimePercent().isEqual(0.0f), "getTimePercent interval<=0返回0");
    }

    // ─── 未 init 直接 tick ──────────────────────────────────────────────
    private static void testNotStarted()
    {
        var t = new MyTimer();
        bool fired = t.tickTimer(10.0f);
        assert(!fired, "未init tick不触发");
    }

    // ─── interval=0 的极端情况 ─────────────────────────────────────────
    private static void testZeroInterval()
    {
        var t = new MyTimer();
        t.init(0.0f, 0.0f, true);
        // interval=0 理论上任意时刻都触发
        bool fired = t.tickTimer(0.0f);
        assert(fired, "interval=0 tick(0)立即触发");
        // loop 下 curTime 重置为0
        assert(t.mCurTime.isEqual(0.0f), "interval=0 loop后curTime=0");
    }

    // ─── MyTimer1 基础验证 ───────────────────────────────────────────────
    private static void testMyTimer1Basic()
    {
        var t = new MyTimer1();
        assert(!t.isCounting(), "MyTimer1 默认未计时");

        t.init(0.0f, 999.0f, false);
        assert(t.isCounting(), "MyTimer1 init后计时");

        // 短暂 tick 不应触发（interval=999）
        bool fired = t.tickTimer();
        assert(!fired, "MyTimer1 短暂tick不触发");

        // stop
        t.stop(true);
        assert(!t.isCounting(), "MyTimer1 stop后未计时");

        // resetToInterval → 下次tick应立即触发
        t.init(0.0f, 0.0f, false);
        t.resetToInterval();
        // interval=0, curTime=0 → 0>=0 → 触发
        fired = t.tickTimer();
        assert(fired, "MyTimer1 interval=0 tick触发");

        // getTimePercent: interval<=0 返回0
        t.init(0.0f, -1.0f, false);
        assert(t.getTimePercent().isEqual(0.0f), "MyTimer1 getTimePercent interval<0=0");
    }
    // ─── setInterval 动态修改 interval ──────────────────────────────────────
    private static void testSetInterval()
    {
        var t = new MyTimer();
        t.init(0.0f, 5.0f, false);
        t.setInterval(1.0f);
        assert(t.mTimeInterval.isEqual(1.0f), "setInterval 修改后interval=1");

        // 使用新 interval 触发
        bool fired = t.tickTimer(1.0f);
        assert(fired, "setInterval 修改后按新interval触发");

        // 动态缩短 interval：先走一半再缩短
        t.init(0.0f, 4.0f, false);
        t.tickTimer(2.0f);  // curTime=2
        t.setInterval(1.5f); // 新interval < curTime，下次tick应立即触发
        fired = t.tickTimer(0.0f);
        assert(fired, "setInterval 缩短后curTime已超新interval立即触发");
    }

    // ─── 负 deltaTime 不应触发也不应崩溃 ────────────────────────────────────
    private static void testNegativeDeltaTime()
    {
        var t = new MyTimer();
        t.init(0.0f, 1.0f, false);

        // 负 deltaTime：curTime 变为负值，tickTimer 不触发（mCurTime < 0 时提前 return false）
        // 注意：tickTimer 先加 elapsedTime 再判断 mCurTime >= mTimeInterval
        // mCurTime = 0 + (-0.5) = -0.5；此时 mCurTime < mTimeInterval → 不触发
        bool fired = t.tickTimer(-0.5f);
        assert(!fired, "负deltaTime 不触发");
        // mCurTime=-0.5 < 0 → isCounting()=false
        assert(!t.isCounting(), "负deltaTime后 curTime<0 isCounting=false");

        // 再传正值恢复计时（需要先 start）
        t.start();
        assert(t.isCounting(), "start 后 isCounting=true");
        fired = t.tickTimer(1.0f);
        assert(fired, "恢复后 tick 正常触发");
    }

    // ─── getTimePercent curTime > interval ───────────────────────────────────
    private static void testGetTimePercentOverOne()
    {
        var t = new MyTimer();
        t.init(0.0f, 1.0f, true);
        // tick 超过 interval（loop 情况下 curTime 会被减去 interval，这里手动设置）
        t.mCurTime = 1.5f;
        // getTimePercent = 1.5/1.0 = 1.5，框架不做 saturate
        float pct = t.getTimePercent();
        assert(pct > 1.0f, $"getTimePercent curTime>interval 结果>1: {pct}");

        // curTime == interval 时 = 1.0
        t.mCurTime = 1.0f;
        assert(t.getTimePercent().isEqual(1.0f, 0.001f), "getTimePercent curTime=interval=1.0");
    }

    // ─── 组合场景 ────────────────────────────────────────────────────────

    // start → stop → start 循环: 每次 start 重新计时
    private static void testStopRestartCycle()
    {
        var t = new MyTimer();
        for (int i = 0; i < 3; ++i)
        {
            // 每轮重新 init(上一轮 stop(true) 已重置 interval=-1 不再计时)
            t.init(0.0f, 1.0f, false);
            assert(t.isCounting(), "第 " + (i + 1) + " 轮 init 后计时中");
            bool fired = t.tickTimer(0.3f);
            assert(!fired, "第 " + (i + 1) + " 轮未到不触发");
            t.stop(true);
            assert(!t.isCounting(), "第 " + (i + 1) + " 轮 stop 后停止");
            t.start();
            assert(t.isCounting(), "第 " + (i + 1) + " 轮 start 重新计时");
            assert(t.mCurTime.isEqual(0.0f), "第 " + (i + 1) + " 轮 start 后 curTime=0");
            t.stop(true);
        }
    }

    // 循环模式多次触发: 计数递增
    private static void testLoopMultipleFires()
    {
        var t = new MyTimer();
        t.init(0.0f, 1.0f, true);   // loop
        int fireCount = 0;
        for (int i = 0; i < 5; ++i)
        {
            bool fired = t.tickTimer(1.0f);
            if (fired)
            {
                ++fireCount;
            }
        }
        assertEqual(5, fireCount, "循环模式 5 次 tick 触发 5 次");
    }

    // tick 过程中 percent 单调递增
    private static void testPercentMonotonic()
    {
        var t = new MyTimer();
        t.init(0.0f, 1.0f, true);
        float prev = 0.0f;
        for (int i = 1; i <= 5; ++i)
        {
            t.tickTimer(0.1f);
            float cur = t.getTimePercent();
            assert(cur >= prev - 0.001f, "percent 不递减: " + cur + " vs " + prev);
            prev = cur;
        }
    }

    // 计时中途 setInterval 改小 → 更快触发
    private static void testIntervalChangeMidway()
    {
        var t = new MyTimer();
        t.init(0.0f, 2.0f, false);
        t.tickTimer(1.0f);          // 1.0/2.0
        assert(t.isCounting(), "中途仍在计时");
        t.setInterval(1.0f);        // 间隔改为 1
        bool fired = t.tickTimer(0.1f);   // 1.0+0.1 ≥ 1
        assert(fired, "改小间隔后更快触发");
    }

    // start 重置 curTime
    private static void testStartResetsCurTime()
    {
        var t = new MyTimer();
        t.init(0.0f, 1.0f, false);
        t.tickTimer(0.5f);
        assert(t.mCurTime.isEqual(0.5f, 0.001f), "tick 后 curTime=0.5");
        t.stop(true);
        t.start();
        assert(t.mCurTime.isEqual(0.0f, 0.001f), "start 重置 curTime=0");
    }

    // ensureInterval 切换: true 溢出清零, false 溢出携带
    private static void testEnsureIntervalToggle()
    {
        var t = new MyTimer();
        t.init(0.0f, 1.0f, true);
        // true: tick(1.5) 溢出清零
        t.setEnsureInterval(true);
        t.tickTimer(1.5f);
        assert(t.mCurTime.isEqual(0.0f, 0.001f), "ensureInterval=true 溢出清零");
        // false: tick(1.5) 溢出携带(1.5-1=0.5)
        t.setEnsureInterval(false);
        t.tickTimer(1.5f);
        assert(t.mCurTime.isEqual(0.5f, 0.001f), "ensureInterval=false 溢出携带 0.5");
    }

    // 计时中途 resetToInterval: curTime 回到 interval
    private static void testResetToIntervalMidway()
    {
        var t = new MyTimer();
        t.init(0.0f, 1.0f, true);
        t.tickTimer(0.3f);
        assert(t.mCurTime.isEqual(0.3f, 0.001f), "tick 后 curTime=0.3");
        t.resetToInterval();
        assert(t.mCurTime.isEqual(1.0f, 0.001f), "resetToInterval 后 curTime=interval=1");
        // 重置后立即触发
        bool fired = t.tickTimer(0.0f);
        assert(fired, "curTime=interval 后 tick 立即触发");
    }

    // 重复 init: 状态完全重置
    private static void testInitTwiceResets()
    {
        var t = new MyTimer();
        t.init(0.0f, 1.0f, true);
        t.tickTimer(0.5f);
        t.init(0.0f, 2.0f, false);
        assert(t.mCurTime.isEqual(0.0f, 0.001f), "二次 init 后 curTime=0");
        assert(t.mTimeInterval.isEqual(2.0f, 0.001f), "二次 init 后 interval=2");
        assert(!t.mLoop, "二次 init 后 loop=false");
    }
}