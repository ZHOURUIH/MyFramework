#if UNITY_EDITOR || DEVELOPMENT_BUILD
using System;
using static FrameUtility;
using static FrameBaseHotFix;

// EventSystem 集成测试
public static class EventSystemTest
{
    // 测试用事件类型
    private class TestEvent : GameEvent { }
    private class TestEventWithValue : GameEvent
    {
        public int value;
        public override void resetProperty()
        {
            base.resetProperty();
            value = 0;
        }
    }

    // 用于接收事件的监听器
    private class TestListener : IEventListener { }

    public static void Run()
    {
        testListenAndPush_NoParam();
        testListenAndPush_WithParam();
        testUnlisten_StopsReceiving();
    }

    // ─── 无参事件 ─────────────────────────────────────────────────────────

    private static void testListenAndPush_NoParam()
    {
        int received = 0;
        var listener = new TestListener();

        mEventSystem.listenEvent<TestEvent>(_ => received++, listener);
        mEventSystem.pushEvent<TestEvent>();

        AssertEqual(1, received, "EventSystem: 推送后应收到 1 次");

        mEventSystem.unlistenEvent(listener);
        mEventSystem.pushEvent<TestEvent>();

        AssertEqual(1, received, "EventSystem: unlistenEvent 后不应再收到");
    }

    // ─── 带参数事件 ───────────────────────────────────────────────────────

    private static void testListenAndPush_WithParam()
    {
        int lastValue = -1;
        var listener  = new TestListener();

        mEventSystem.listenEvent<TestEventWithValue>(e => lastValue = e.value, listener);

        var evt = CLASS<TestEventWithValue>();
        evt.value = 42;
        mEventSystem.pushEvent(evt);

        AssertEqual(42, lastValue, "EventSystem 带参: 收到的 value 应为 42");

        mEventSystem.unlistenEvent(listener);
    }

    // ─── 取消监听 ─────────────────────────────────────────────────────────

    private static void testUnlisten_StopsReceiving()
    {
        int count = 0;
        var listener = new TestListener();

        mEventSystem.listenEvent<TestEvent>(_ => count++, listener);
        mEventSystem.pushEvent<TestEvent>();
        mEventSystem.pushEvent<TestEvent>();
        AssertEqual(2, count, "EventSystem: 两次推送应收到 2 次");

        mEventSystem.unlistenEvent<TestEvent>(listener);
        mEventSystem.pushEvent<TestEvent>();
        AssertEqual(2, count, "EventSystem: unlistenEvent<T> 后不应再收到");
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
}
#endif