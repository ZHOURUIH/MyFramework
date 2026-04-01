#if UNITY_EDITOR || DEVELOPMENT_BUILD
using static TestAssert;
using static MathUtility;

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
    }

    // ─── init 与初始状态 ─────────────────────────────────────────────────
    private static void testInitAndState()
    {
        var t = new MyTimer();
        assert(!t.isCounting(), "MyTimer 默认未开始计时");

        t.init(0.0f, 1.0f, true);
        assert(t.isCounting(),                 "MyTimer init后开始计时");
        assert(isFloatEqual(t.mTimeInterval, 1.0f), "MyTimer interval=1");
        assert(t.mLoop,                         "MyTimer loop=true");

        t.start();
        assert(t.isCounting(), "MyTimer start 后 isCounting=true");
        assert(isFloatEqual(t.mCurTime, 0.0f), "MyTimer start curTime=0");
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
        assert(isFloatEqual(t.mCurTime, -1.0f), "non-loop 触发后 curTime=-1");

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
        assert(isFloatEqual(t.mCurTime, 0.5f, 0.001f), "loop 携带溢出时间0.5");

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
        assert(isFloatEqual(t.mCurTime, 0.0f), "ensureInterval 重置curTime=0");
    }

    // ─── stop ────────────────────────────────────────────────────────────
    private static void testStop()
    {
        var t = new MyTimer();
        t.init(0.0f, 2.0f, true);

        // stop 带reset
        t.stop(true);
        assert(!t.isCounting(),                 "stop(true) 停止计时");
        assert(isFloatEqual(t.mTimeInterval, -1.0f), "stop(true) interval重置为-1");

        // stop 不带reset：只停时间，interval保留
        t.init(0.0f, 2.0f, true);
        t.stop(false);
        assert(!t.isCounting(),                     "stop(false) 停止计时");
        assert(isFloatEqual(t.mTimeInterval, 2.0f), "stop(false) interval保留2");
    }

    // ─── resetToInterval ─────────────────────────────────────────────────
    private static void testResetToInterval()
    {
        var t = new MyTimer();
        t.init(0.0f, 3.0f, false);
        t.resetToInterval();
        assert(isFloatEqual(t.mCurTime, t.mTimeInterval), "resetToInterval curTime=interval");

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
        assert(isFloatEqual(pct, 0.5f, 0.001f), "getTimePercent 0.5");

        // interval<=0 时返回0
        var t2 = new MyTimer();
        t2.init(0.0f, -1.0f, true);
        assert(isFloatEqual(t2.getTimePercent(), 0.0f), "getTimePercent interval<=0返回0");
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
        assert(isFloatEqual(t.mCurTime, 0.0f), "interval=0 loop后curTime=0");
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
        assert(isFloatEqual(t.getTimePercent(), 0.0f), "MyTimer1 getTimePercent interval<0=0");
    }
    // ─── setInterval 动态修改 interval ──────────────────────────────────────
    private static void testSetInterval()
    {
        var t = new MyTimer();
        t.init(0.0f, 5.0f, false);
        t.setInterval(1.0f);
        assert(isFloatEqual(t.mTimeInterval, 1.0f), "setInterval 修改后interval=1");

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
        assert(isFloatEqual(t.getTimePercent(), 1.0f, 0.001f), "getTimePercent curTime=interval=1.0");
    }
}
#endif
