using static TestAssert;
public static class GameEventRegisteInfoTest
{
    public static void Run()
    {
        testFields();
        testResetProperty();
        testCallbacks();
    }
    static void testFields()
    {
        GameEventRegisteInfo info = new();
        info.mEventTypeID = 100;
        info.mCharacterID = 200L;
        assertEqual(100, info.mEventTypeID);
        assertEqual(200L, info.mCharacterID);
    }
    static void testResetProperty()
    {
        GameEventRegisteInfo info = new();
        info.mEventTypeID = 999;
        info.mCharacterID = 888L;
        info.resetProperty();
        assertEqual(0, info.mEventTypeID);
        assertEqual(0L, info.mCharacterID);
        assertEqual(null, info.mListener);
        assertEqual(null, info.mBaseCallback);
    }
    static void testCallbacks()
    {
        bool called = false;
        GameEventRegisteInfo info = new();
        info.mBaseCallback = () => { called = true; };
        info.call(new GameEvent());
        assertTrue(called, "base callback should fire");

        int val = 0;
        GameEventRegisteInfoT<TestGameEvent> infoT = new();
        infoT.mCallback = (e) => { val = e.mValue; };
        TestGameEvent evt = new();
        evt.mValue = 42;
        infoT.call(evt);
        assertEqual(42, val, "generic callback should pass value");
    }
    class TestGameEvent : GameEvent
    {
        public int mValue;
        public override void resetProperty()
        {
            base.resetProperty();
            mValue = 0;
        }
    }
}